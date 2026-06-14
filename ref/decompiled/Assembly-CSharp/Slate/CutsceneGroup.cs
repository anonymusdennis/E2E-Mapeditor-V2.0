using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slate;

public abstract class CutsceneGroup : MonoBehaviour, IDirectable
{
	public enum ActorReferenceMode
	{
		UseOriginal,
		UseInstanceHideOriginal
	}

	public enum ActorInitialTransformation
	{
		UseOriginal,
		UseLocal
	}

	[SerializeField]
	[HideInInspector]
	private List<CutsceneTrack> _tracks = new List<CutsceneTrack>();

	[HideInInspector]
	[SerializeField]
	private List<Section> _sections = new List<Section>();

	[SerializeField]
	[HideInInspector]
	private bool _isCollapsed;

	[SerializeField]
	[HideInInspector]
	private bool _active = true;

	private TransformSnapshot transformSnapshot;

	private ObjectSnapshot objectSnapshot;

	private GameObject originalActor;

	IEnumerable<IDirectable> IDirectable.children => tracks.Cast<IDirectable>();

	float IDirectable.startTime => 0f;

	float IDirectable.endTime => root.length;

	float IDirectable.blendIn => 0f;

	float IDirectable.blendOut => 0f;

	IDirectable IDirectable.parent => null;

	public new abstract string name { get; }

	public abstract GameObject actor { get; set; }

	public abstract ActorReferenceMode referenceMode { get; set; }

	public abstract ActorInitialTransformation initialTransformation { get; set; }

	public abstract Vector3 initialLocalPosition { get; set; }

	public abstract Vector3 initialLocalRotation { get; set; }

	public List<CutsceneTrack> tracks
	{
		get
		{
			return _tracks;
		}
		set
		{
			_tracks = value;
		}
	}

	public List<Section> sections
	{
		get
		{
			return _sections;
		}
		set
		{
			_sections = value;
		}
	}

	public IDirector root { get; private set; }

	public bool isActive
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	public bool isCollapsed
	{
		get
		{
			return _isCollapsed;
		}
		set
		{
			_isCollapsed = value;
		}
	}

	public event Action<Section> OnSectionReached;

	public void Validate(IDirector root, IDirectable parent)
	{
		this.root = root;
		CutsceneTrack[] componentsInChildren = GetComponentsInChildren<CutsceneTrack>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!tracks.Contains(componentsInChildren[i]))
			{
				tracks.Add(componentsInChildren[i]);
			}
		}
		if (tracks.Any((CutsceneTrack t) => t == null))
		{
			tracks = componentsInChildren.ToList();
		}
	}

	public Section GetSectionByName(string name)
	{
		if (name.ToUpper() == "INTRO")
		{
			return new Section("Intro", 0f);
		}
		return sections.Find((Section s) => s.name.ToUpper() == name.ToUpper());
	}

	public Section GetSectionByUID(string UID)
	{
		return sections.Find((Section s) => s.UID == UID);
	}

	public Section GetSectionAfter(float time)
	{
		return sections.FirstOrDefault((Section s) => s.time > time);
	}

	public Section GetSectionBefore(float time)
	{
		return sections.LastOrDefault((Section s) => s.time < time);
	}

	public Vector3 TransformPoint(Vector3 point, TransformSpace space)
	{
		Transform spaceTransform = GetSpaceTransform(space);
		return (!(spaceTransform != null)) ? point : spaceTransform.TransformPoint(point);
	}

	public Vector3 InverseTransformPoint(Vector3 point, TransformSpace space)
	{
		Transform spaceTransform = GetSpaceTransform(space);
		return (!(spaceTransform != null)) ? point : spaceTransform.InverseTransformPoint(point);
	}

	public Transform GetSpaceTransform(TransformSpace space)
	{
		return space switch
		{
			TransformSpace.CutsceneSpace => (root == null) ? null : root.context.transform, 
			TransformSpace.ActorSpace => (!(actor != null)) ? null : actor.transform, 
			_ => null, 
		};
	}

	public Vector3 ActorPositionInSpace(TransformSpace space)
	{
		return (!(actor != null)) ? root.context.transform.position : InverseTransformPoint(actor.transform.position, space);
	}

	bool IDirectable.Initialize()
	{
		if (actor == null)
		{
			return false;
		}
		return true;
	}

	void IDirectable.Enter()
	{
		if (root.isReSampleFrame)
		{
			return;
		}
		if (referenceMode == ActorReferenceMode.UseInstanceHideOriginal)
		{
			InstantiateLocalActor();
			return;
		}
		Store();
		if (initialTransformation == ActorInitialTransformation.UseLocal)
		{
			InitLocalCoords(actor);
		}
	}

	void IDirectable.Reverse()
	{
		if (!root.isReSampleFrame)
		{
			if (referenceMode == ActorReferenceMode.UseInstanceHideOriginal)
			{
				ReleaseLocalActorInstance();
			}
			else
			{
				Restore();
			}
		}
	}

	void IDirectable.Update(float time, float previousTime)
	{
		if (root.isReSampleFrame || this.OnSectionReached == null)
		{
			return;
		}
		for (int i = 0; i < sections.Count; i++)
		{
			if (time >= sections[i].time && previousTime < sections[i].time)
			{
				this.OnSectionReached(sections[i]);
			}
		}
	}

	void IDirectable.Exit()
	{
		if (!root.isReSampleFrame && Application.isPlaying && referenceMode == ActorReferenceMode.UseInstanceHideOriginal)
		{
			ReleaseLocalActorInstance();
		}
	}

	void IDirectable.ReverseEnter()
	{
		if (!root.isReSampleFrame && Application.isPlaying && referenceMode == ActorReferenceMode.UseInstanceHideOriginal)
		{
			InstantiateLocalActor();
		}
	}

	private void Store()
	{
		objectSnapshot = new ObjectSnapshot(actor);
		transformSnapshot = new TransformSnapshot(actor, TransformSnapshot.StoreMode.All);
	}

	private void Restore()
	{
		if (objectSnapshot != null)
		{
			objectSnapshot.Restore();
		}
		if (transformSnapshot != null)
		{
			transformSnapshot.Restore();
		}
	}

	private void InstantiateLocalActor()
	{
		originalActor = actor;
		actor = UnityEngine.Object.Instantiate(actor);
		actor.SetActive(value: true);
		SceneManager.MoveGameObjectToScene(actor, root.context.scene);
		if (initialTransformation == ActorInitialTransformation.UseLocal)
		{
			InitLocalCoords(actor);
		}
		originalActor.SetActive(value: false);
	}

	private void ReleaseLocalActorInstance()
	{
		if (actor != originalActor)
		{
			UnityEngine.Object.DestroyImmediate(actor);
			actor = originalActor;
			actor.SetActive(value: true);
			originalActor.SetActive(value: true);
			originalActor = null;
		}
	}

	private void InitLocalCoords(GameObject target)
	{
		Vector3 vector = ((!(target.transform.parent != null)) ? Vector3.zero : target.transform.localPosition);
		target.transform.position = root.context.transform.TransformPoint(initialLocalPosition);
		target.transform.eulerAngles = root.context.transform.eulerAngles + initialLocalRotation;
		target.transform.position += vector;
	}
}
