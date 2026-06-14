using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Slate;

public abstract class CutsceneTrack : MonoBehaviour, IDirectable
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private Color _color = Color.white;

	[HideInInspector]
	[SerializeField]
	private bool _active = true;

	[HideInInspector]
	[SerializeField]
	private List<ActionClip> _actionClips = new List<ActionClip>();

	IEnumerable<IDirectable> IDirectable.children => actions.Cast<IDirectable>();

	public GameObject actor => (parent == null) ? null : parent.actor;

	public new string name
	{
		get
		{
			return (!string.IsNullOrEmpty(_name)) ? _name : StringExtensions.SplitCamelCase(GetType().Name);
		}
		set
		{
			if (_name != value)
			{
				_name = value;
				base.name = value;
			}
		}
	}

	public Color color => (!(_color.a > 0.1f)) ? Color.white : _color;

	public virtual string info => string.Empty;

	public List<ActionClip> actions
	{
		get
		{
			return _actionClips;
		}
		set
		{
			_actionClips = value;
		}
	}

	public int layerOrder { get; set; }

	public IDirector root => (parent == null) ? null : parent.root;

	public IDirectable parent { get; private set; }

	public bool isCollapsed => parent != null && parent.isCollapsed;

	public bool isActive
	{
		get
		{
			return parent != null && parent.isActive && _active;
		}
		set
		{
			_active = value;
		}
	}

	public virtual float startTime
	{
		get
		{
			return parent.startTime;
		}
		set
		{
		}
	}

	public virtual float endTime
	{
		get
		{
			return parent.endTime;
		}
		set
		{
		}
	}

	public virtual float blendIn
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public virtual float blendOut
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	bool IDirectable.Initialize()
	{
		layerOrder = parent.children.Where((IDirectable t) => t.GetType() == GetType()).Reverse().ToList()
			.IndexOf(this);
		return OnInitialize();
	}

	void IDirectable.Enter()
	{
		OnEnter();
	}

	void IDirectable.Update(float time, float previousTime)
	{
		OnUpdate(time, previousTime);
	}

	void IDirectable.Exit()
	{
		OnExit();
	}

	void IDirectable.ReverseEnter()
	{
		OnReverseEnter();
	}

	void IDirectable.Reverse()
	{
		OnReverse();
	}

	protected virtual bool OnInitialize()
	{
		return true;
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnUpdate(float time, float previousTime)
	{
	}

	protected virtual void OnExit()
	{
	}

	protected virtual void OnReverseEnter()
	{
	}

	protected virtual void OnReverse()
	{
	}

	protected virtual void OnDrawGizmosSelected()
	{
	}

	protected virtual void OnSceneGUI()
	{
	}

	protected virtual void OnCreate()
	{
	}

	protected virtual void OnAfterValidate()
	{
	}

	public void PostCreate(IDirectable parent)
	{
		this.parent = parent;
		OnCreate();
	}

	public void Validate(IDirector root, IDirectable parent)
	{
		this.parent = parent;
		actions = (from a in GetComponents<ActionClip>()
			orderby a.startTime
			select a).ToList();
		OnAfterValidate();
	}

	public float GetTrackWeight(float time)
	{
		if (time < blendIn)
		{
			return time / blendIn;
		}
		if (time > endTime - startTime - blendOut)
		{
			return (endTime - startTime - time) / blendOut;
		}
		return 1f;
	}

	public Vector3 TransformPoint(Vector3 point, TransformSpace space)
	{
		return (parent == null) ? point : parent.TransformPoint(point, space);
	}

	public Vector3 InverseTransformPoint(Vector3 point, TransformSpace space)
	{
		return (parent == null) ? point : parent.InverseTransformPoint(point, space);
	}

	public Vector3 ActorPositionInSpace(TransformSpace space)
	{
		return (parent == null) ? Vector3.zero : parent.ActorPositionInSpace(space);
	}

	public Transform GetSpaceTransform(TransformSpace space)
	{
		return (parent == null) ? null : parent.GetSpaceTransform(space);
	}
}
