using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Slate.ActionClips;
using UnityEngine;

namespace Slate;

[DisallowMultipleComponent]
public class Cutscene : MonoBehaviour, IDirector
{
	public enum StopMode
	{
		Skip,
		Rewind,
		Hold
	}

	public enum WrapMode
	{
		Once,
		Loop,
		PingPong
	}

	public enum UpdateMode
	{
		Normal,
		AnimatePhysics,
		UnscaledTime
	}

	public enum PlayingDirection
	{
		Forwards,
		Backwards
	}

	public const float VERSION_NUMBER = 1.65f;

	[SerializeField]
	private UpdateMode _updateMode;

	[SerializeField]
	private StopMode _defaultStopMode;

	[SerializeField]
	private WrapMode _defaultWrapMode;

	[SerializeField]
	private bool _explicitActiveLayers;

	[SerializeField]
	private LayerMask _activeLayers = -1;

	[SerializeField]
	private float _playbackSpeed = 1f;

	[HideInInspector]
	public List<CutsceneGroup> groups = new List<CutsceneGroup>();

	[SerializeField]
	[HideInInspector]
	private float _length = 20f;

	[HideInInspector]
	[SerializeField]
	private float _viewTimeMin;

	[SerializeField]
	[HideInInspector]
	private float _viewTimeMax = 21f;

	[NonSerialized]
	private float _currentTime;

	[NonSerialized]
	private float _playTimeStart;

	[NonSerialized]
	private float _playTimeEnd;

	[NonSerialized]
	private Transform _groupsRoot;

	[NonSerialized]
	private List<IDirectableTimePointer> timePointers;

	[NonSerialized]
	private List<IDirectableTimePointer> trackOrderedTimePointers;

	[NonSerialized]
	private Dictionary<GameObject, bool> affectedLayerGOStates;

	[NonSerialized]
	private static Dictionary<string, Cutscene> allSceneCutscenes = new Dictionary<string, Cutscene>();

	[NonSerialized]
	private bool preInitialized;

	[NonSerialized]
	private bool _isReSampleFrame;

	bool IDirector.isReSampleFrame => _isReSampleFrame;

	GameObject IDirector.context => base.gameObject;

	public Transform groupsRoot
	{
		get
		{
			if (_groupsRoot == null)
			{
				_groupsRoot = base.transform.Find("__GroupsRoot__");
				if (_groupsRoot == null)
				{
					_groupsRoot = new GameObject("__GroupsRoot__").transform;
					_groupsRoot.SetParent(base.transform);
				}
			}
			return _groupsRoot;
		}
	}

	public UpdateMode updateMode
	{
		get
		{
			return _updateMode;
		}
		set
		{
			_updateMode = value;
		}
	}

	public StopMode defaultStopMode
	{
		get
		{
			return _defaultStopMode;
		}
		set
		{
			_defaultStopMode = value;
		}
	}

	public WrapMode defaultWrapMode
	{
		get
		{
			return _defaultWrapMode;
		}
		set
		{
			_defaultWrapMode = value;
		}
	}

	public bool explicitActiveLayers
	{
		get
		{
			return _explicitActiveLayers;
		}
		set
		{
			_explicitActiveLayers = value;
		}
	}

	public LayerMask activeLayers
	{
		get
		{
			return _activeLayers;
		}
		set
		{
			_activeLayers = value;
		}
	}

	public DirectorGroup directorGroup
	{
		get
		{
			if (groups.Count > 0 && groups[0] is DirectorGroup)
			{
				return (DirectorGroup)groups[0];
			}
			return groups.Find((CutsceneGroup g) => g is DirectorGroup) as DirectorGroup;
		}
	}

	public CameraTrack cameraTrack => directorGroup.tracks.Find((CutsceneTrack t) => t is CameraTrack) as CameraTrack;

	public float currentTime
	{
		get
		{
			return _currentTime;
		}
		set
		{
			_currentTime = Mathf.Clamp(value, 0f, length);
		}
	}

	public float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = Mathf.Max(value, 1f);
		}
	}

	public float viewTimeMin
	{
		get
		{
			return _viewTimeMin;
		}
		set
		{
			if (viewTimeMax > 0f)
			{
				_viewTimeMin = Mathf.Min(value, viewTimeMax - 0.25f);
			}
		}
	}

	public float viewTimeMax
	{
		get
		{
			return _viewTimeMax;
		}
		set
		{
			_viewTimeMax = Mathf.Max(value, viewTimeMin + 0.25f, 0f);
		}
	}

	public float playTimeStart
	{
		get
		{
			return _playTimeStart;
		}
		set
		{
			_playTimeStart = Mathf.Clamp(value, 0f, playTimeEnd);
		}
	}

	public float playTimeEnd
	{
		get
		{
			return _playTimeEnd;
		}
		set
		{
			_playTimeEnd = Mathf.Clamp(value, playTimeStart, length);
		}
	}

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			_playbackSpeed = value;
		}
	}

	public List<IDirectable> directables { get; private set; }

	public bool isActive { get; private set; }

	public bool isPaused { get; private set; }

	public PlayingDirection playingDirection { get; private set; }

	public WrapMode playingWrapMode { get; private set; }

	public float previousTime { get; private set; }

	public float remainingTime
	{
		get
		{
			if (playingDirection == PlayingDirection.Forwards)
			{
				return playTimeEnd - currentTime;
			}
			if (playingDirection == PlayingDirection.Backwards)
			{
				return currentTime - playTimeStart;
			}
			return 0f;
		}
	}

	public static event Action<Cutscene> OnCutsceneStarted;

	public static event Action<Cutscene> OnCutsceneStopped;

	public event Action<Section> OnSectionReached;

	public event Action<string, object> OnGlobalMessageSend;

	private event Action OnStop;

	public IEnumerable<GameObject> GetAffectedActors()
	{
		return from g in groups.OfType<ActorGroup>()
			select g.actor;
	}

	public float[] GetKeyTimes()
	{
		if (timePointers == null)
		{
			InitializeTimePointers();
		}
		return timePointers.Select((IDirectableTimePointer t) => t.time).ToArray();
	}

	public void Play()
	{
		Play(0f);
	}

	public void Play(Action callback)
	{
		Play(0f, callback);
	}

	public void Play(float startTime)
	{
		Play(startTime, length, defaultWrapMode);
	}

	public void Play(float startTime, Action callback)
	{
		Play(startTime, length, defaultWrapMode, callback);
	}

	public void Play(float startTime, float endTime, WrapMode wrapMode = WrapMode.Once, Action callback = null, PlayingDirection playDirection = PlayingDirection.Forwards)
	{
		if (startTime > endTime && playDirection != PlayingDirection.Backwards)
		{
			Debug.LogError("End Time must be greater than Start Time.", base.gameObject);
			return;
		}
		if (isPaused)
		{
			Debug.LogWarning("Play called on a Paused cutscene. Cutscene will now resume instead.", base.gameObject);
			playingDirection = playDirection;
			Resume();
			return;
		}
		if (isActive)
		{
			Debug.LogWarning("Cutscene is already Running.", base.gameObject);
			return;
		}
		playTimeStart = 0f;
		playTimeEnd = endTime;
		playTimeStart = startTime;
		currentTime = startTime;
		playingWrapMode = wrapMode;
		playingDirection = playDirection;
		if (playDirection == PlayingDirection.Forwards && currentTime >= playTimeEnd)
		{
			currentTime = playTimeStart;
		}
		if (playDirection == PlayingDirection.Backwards && currentTime <= playTimeStart)
		{
			currentTime = playTimeEnd;
		}
		isActive = true;
		isPaused = false;
		this.OnStop = ((callback == null) ? this.OnStop : callback);
		SendGlobalMessage("OnCutsceneStarted");
		if (Cutscene.OnCutsceneStarted != null)
		{
			Cutscene.OnCutsceneStarted(this);
		}
		Sample();
		StartCoroutine(Internal_UpdateCutscene());
	}

	public void Stop()
	{
		Stop(defaultStopMode);
	}

	public void Stop(StopMode stopMode)
	{
		if (!isActive)
		{
			Debug.LogWarning("Called Stop on a non-active cutscene", base.gameObject);
			return;
		}
		isActive = false;
		isPaused = false;
		if (stopMode == StopMode.Skip)
		{
			Sample((playingDirection != 0) ? playTimeStart : playTimeEnd);
		}
		if (stopMode == StopMode.Rewind)
		{
			Sample((playingDirection != 0) ? playTimeEnd : playTimeStart);
		}
		SendGlobalMessage("OnCutsceneStopped");
		if (Cutscene.OnCutsceneStopped != null)
		{
			Cutscene.OnCutsceneStopped(this);
		}
		if (this.OnStop != null)
		{
			this.OnStop();
		}
	}

	public void PlayReverse()
	{
		PlayReverse(0f, length);
	}

	public void PlayReverse(float startTime, float endTime)
	{
		Play(startTime, endTime, WrapMode.Once, null, PlayingDirection.Backwards);
	}

	public void Pause()
	{
		isPaused = true;
	}

	public void Resume()
	{
		isPaused = false;
	}

	public void Rewind()
	{
		if (isActive)
		{
			Stop(StopMode.Rewind);
		}
		else
		{
			Sample(0f);
		}
	}

	public void SkipAll()
	{
		if (isActive)
		{
			Stop(StopMode.Skip);
		}
		else
		{
			Sample(length);
		}
	}

	public void RewindNoUndo()
	{
		if (isActive)
		{
			Stop(StopMode.Hold);
		}
		currentTime = ((playingDirection != 0) ? length : 0f);
		previousTime = currentTime;
		Sample();
	}

	[Obsolete("Use 'SkipCurrentSection' instead")]
	public void Skip()
	{
		SkipCurrentSection();
	}

	public void SkipCurrentSection()
	{
		bool flag = playingDirection == PlayingDirection.Forwards;
		currentTime = ((!flag) ? directorGroup.GetSectionBefore(currentTime) : directorGroup.GetSectionAfter(currentTime))?.time ?? ((!flag) ? 0f : length);
	}

	public bool JumpToSection(string name)
	{
		return JumpToSection(GetSectionByName(name));
	}

	public bool JumpToSection(Section section)
	{
		if (section == null)
		{
			Debug.LogError("Null Section Provided", base.gameObject);
			return false;
		}
		currentTime = section.time;
		return true;
	}

	public bool PlayFromSection(string name)
	{
		return PlayFromSection(name, defaultWrapMode);
	}

	public bool PlayFromSection(string name, WrapMode wrap, Action callback = null)
	{
		Section sectionByName = directorGroup.GetSectionByName(name);
		if (sectionByName == null)
		{
			Debug.LogError("Null Section Provided", base.gameObject);
			return false;
		}
		Play(sectionByName.time, length, wrap, callback);
		return true;
	}

	public bool PlaySection(string name)
	{
		return PlaySection(GetSectionByName(name), defaultWrapMode);
	}

	public bool PlaySection(string name, WrapMode wrap, Action callback = null)
	{
		return PlaySection(GetSectionByName(name), wrap, callback);
	}

	public bool PlaySection(Section section)
	{
		return PlaySection(section, defaultWrapMode);
	}

	public bool PlaySection(Section section, WrapMode wrap, Action callback = null)
	{
		if (section == null)
		{
			Debug.LogError("Null Section Provided", base.gameObject);
			return false;
		}
		float endTime = directorGroup.GetSectionAfter(section.time)?.time ?? length;
		Play(section.time, endTime, wrap, callback);
		return true;
	}

	public void Sample()
	{
		Sample(currentTime);
	}

	public void Sample(float time)
	{
		currentTime = time;
		if ((currentTime != 0f && currentTime != length) || previousTime != currentTime)
		{
			if (currentTime > 0f && currentTime < length && (previousTime == 0f || previousTime == length))
			{
				OnSampleEnable();
			}
			if (!preInitialized && currentTime > 0f && previousTime == 0f)
			{
				InitializeTimePointers();
			}
			if (timePointers != null)
			{
				Internal_SamplePointers(currentTime, previousTime);
			}
			if ((currentTime == 0f || currentTime == length) && previousTime > 0f && previousTime < length)
			{
				OnSampleDisable();
			}
			previousTime = currentTime;
		}
	}

	private void Internal_SamplePointers(float currentTime, float previousTime)
	{
		if (!Application.isPlaying || currentTime > previousTime)
		{
			for (int i = 0; i < timePointers.Count; i++)
			{
				try
				{
					timePointers[i].TriggerForward(currentTime, previousTime);
				}
				catch (Exception ex)
				{
					Debug.LogError($"{ex.Message}\n{ex.StackTrace}", base.gameObject);
				}
			}
		}
		if (!Application.isPlaying || currentTime < previousTime)
		{
			for (int num = timePointers.Count - 1; num >= 0; num--)
			{
				try
				{
					timePointers[num].TriggerBackward(currentTime, previousTime);
				}
				catch (Exception ex2)
				{
					Debug.LogError($"{ex2.Message}\n{ex2.StackTrace}", base.gameObject);
				}
			}
		}
		if (trackOrderedTimePointers == null)
		{
			return;
		}
		for (int j = 0; j < trackOrderedTimePointers.Count; j++)
		{
			try
			{
				trackOrderedTimePointers[j].Update(currentTime, previousTime);
			}
			catch (Exception ex3)
			{
				Debug.LogError($"{ex3.Message}\n{ex3.StackTrace}", base.gameObject);
			}
		}
	}

	public void ReSample()
	{
		if (!Application.isPlaying && currentTime > 0f && currentTime < length && timePointers != null)
		{
			_isReSampleFrame = true;
			Internal_SamplePointers(0f, currentTime);
			Internal_SamplePointers(currentTime, 0f);
			_isReSampleFrame = false;
		}
	}

	private void InitializeTimePointers()
	{
		timePointers = new List<IDirectableTimePointer>();
		trackOrderedTimePointers = new List<IDirectableTimePointer>();
		foreach (CutsceneGroup item in groups.AsEnumerable().Reverse())
		{
			if (!((IDirectable)item).isActive || !((IDirectable)item).Initialize())
			{
				continue;
			}
			TimeInPointer timeInPointer = new TimeInPointer(item);
			timePointers.Add(timeInPointer);
			foreach (IDirectable item2 in ((IDirectable)item).children.Reverse())
			{
				if (!item2.isActive || !item2.Initialize())
				{
					continue;
				}
				TimeInPointer timeInPointer2 = new TimeInPointer(item2);
				timePointers.Add(timeInPointer2);
				foreach (IDirectable child in item2.children)
				{
					if (child.isActive && child.Initialize())
					{
						TimeInPointer timeInPointer3 = new TimeInPointer(child);
						timePointers.Add(timeInPointer3);
						trackOrderedTimePointers.Add(timeInPointer3);
						timePointers.Add(new TimeOutPointer(child));
					}
				}
				trackOrderedTimePointers.Add(timeInPointer2);
				timePointers.Add(new TimeOutPointer(item2));
			}
			trackOrderedTimePointers.Add(timeInPointer);
			timePointers.Add(new TimeOutPointer(item));
		}
		timePointers = timePointers.OrderBy((IDirectableTimePointer p) => p.time).ToList();
	}

	private void OnSampleEnable()
	{
		SetLayersActive();
	}

	private void OnSampleDisable()
	{
		RestoreLayersActive();
	}

	private void SetLayersActive()
	{
		if (explicitActiveLayers)
		{
			GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
			affectedLayerGOStates = new Dictionary<GameObject, bool>();
			foreach (GameObject gameObject in rootGameObjects)
			{
				affectedLayerGOStates[gameObject] = gameObject.activeInHierarchy;
				gameObject.SetActive((activeLayers.value & (1 << gameObject.layer)) > 0);
			}
		}
	}

	private void RestoreLayersActive()
	{
		if (affectedLayerGOStates == null)
		{
			return;
		}
		foreach (KeyValuePair<GameObject, bool> affectedLayerGOState in affectedLayerGOStates)
		{
			if (affectedLayerGOState.Key != null)
			{
				affectedLayerGOState.Key.SetActive(affectedLayerGOState.Value);
			}
		}
	}

	private IEnumerator Internal_UpdateCutscene()
	{
		while (isActive)
		{
			while (isPaused)
			{
				if (updateMode == UpdateMode.AnimatePhysics)
				{
					yield return new WaitForFixedUpdate();
				}
				Sample();
				yield return null;
			}
			if (!isActive)
			{
				break;
			}
			if (updateMode == UpdateMode.AnimatePhysics)
			{
				yield return new WaitForFixedUpdate();
			}
			float delta2 = UpdateManager.deltaTime;
			if (updateMode == UpdateMode.AnimatePhysics)
			{
				delta2 = Time.fixedDeltaTime;
			}
			if (updateMode == UpdateMode.UnscaledTime)
			{
				delta2 = Time.unscaledDeltaTime;
			}
			delta2 *= playbackSpeed;
			currentTime += ((playingDirection != 0) ? (0f - delta2) : delta2);
			if (playingWrapMode == WrapMode.Once)
			{
				if (currentTime >= playTimeEnd && playingDirection == PlayingDirection.Forwards)
				{
					Stop();
					break;
				}
				if (currentTime <= playTimeStart && playingDirection == PlayingDirection.Backwards)
				{
					Stop();
					break;
				}
			}
			if (playingWrapMode == WrapMode.Loop)
			{
				if (currentTime >= playTimeEnd)
				{
					currentTime = playTimeStart + float.Epsilon;
				}
				if (currentTime <= playTimeStart)
				{
					currentTime = playTimeEnd - float.Epsilon;
				}
			}
			if (playingWrapMode == WrapMode.PingPong)
			{
				if (currentTime >= playTimeEnd)
				{
					currentTime = playTimeEnd - float.Epsilon;
					playingDirection = ((playbackSpeed >= 0f) ? PlayingDirection.Backwards : PlayingDirection.Forwards);
				}
				if (currentTime <= playTimeStart)
				{
					currentTime = playTimeStart + float.Epsilon;
					playingDirection = ((!(playbackSpeed >= 0f)) ? PlayingDirection.Backwards : PlayingDirection.Forwards);
				}
			}
			Sample();
			yield return null;
		}
	}

	protected void OnValidate()
	{
		if (!Application.isPlaying)
		{
			Validate();
		}
	}

	protected void Awake()
	{
		Validate();
		allSceneCutscenes[base.name] = this;
		if (!(directorGroup != null))
		{
			return;
		}
		directorGroup.OnSectionReached += delegate(Section section)
		{
			if (this.OnSectionReached != null)
			{
				this.OnSectionReached(section);
			}
		};
	}

	protected void OnDestroy()
	{
		isActive = false;
		allSceneCutscenes.Remove(base.name);
	}

	public void Validate()
	{
		if (groupsRoot.transform.parent != base.transform)
		{
			groupsRoot.transform.parent = base.transform;
		}
		directables = new List<IDirectable>();
		foreach (CutsceneGroup item in groups.AsEnumerable().Reverse())
		{
			((IDirectable)item).Validate((IDirector)this, (IDirectable)null);
			directables.Add(item);
			foreach (IDirectable item2 in ((IDirectable)item).children.Reverse())
			{
				item2.Validate(this, item);
				directables.Add(item2);
				foreach (IDirectable child in item2.children)
				{
					child.Validate(this, item2);
					directables.Add(child);
				}
			}
		}
	}

	public static Cutscene Play(string name)
	{
		return Play(name, null);
	}

	public static Cutscene Play(string name, Action callback)
	{
		Cutscene cutscene = FindFromResources(name);
		if (cutscene != null)
		{
			Cutscene instance = UnityEngine.Object.Instantiate(cutscene);
			Debug.Log("Instantiating cutscene from Resources");
			instance.Play(delegate
			{
				UnityEngine.Object.Destroy(instance.gameObject);
				Debug.Log("Instantiated Cutscene Destroyed");
				if (callback != null)
				{
					callback();
				}
			});
			return cutscene;
		}
		cutscene = Find(name);
		if (cutscene != null)
		{
			cutscene.Play(callback);
			return cutscene;
		}
		return null;
	}

	public static Cutscene FindFromResources(string name)
	{
		GameObject gameObject = Resources.Load(name, typeof(GameObject)) as GameObject;
		if (gameObject != null)
		{
			Cutscene component = gameObject.GetComponent<Cutscene>();
			if (component != null)
			{
				return component;
			}
		}
		Debug.LogWarning($"Cutscene of name '{name}' does not exists in the Resources folder");
		return null;
	}

	public static Cutscene Find(string name)
	{
		Cutscene value = null;
		if (allSceneCutscenes.TryGetValue(name, out value))
		{
			return value;
		}
		Debug.LogError($"Cutscene of name '{name}' does not exists in the scene");
		return null;
	}

	public void SendGlobalMessage(string message, object value = null)
	{
		base.gameObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
		foreach (GameObject affectedActor in GetAffectedActors())
		{
			if (affectedActor != null)
			{
				affectedActor.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (this.OnGlobalMessageSend != null)
		{
			this.OnGlobalMessageSend(message, value);
		}
	}

	public void SetGroupActorOfName(string groupName, GameObject newActor)
	{
		if (currentTime > 0f)
		{
			Debug.LogError("Setting a Group Actor is only allowed when the Cutscene is not active and is rewinded", base.gameObject);
			return;
		}
		ActorGroup actorGroup = groups.OfType<ActorGroup>().FirstOrDefault((ActorGroup g) => g.name.ToLower() == groupName.ToLower());
		if (actorGroup == null)
		{
			Debug.LogError($"Actor Group with name '{groupName}' doesn't exist in cutscene", base.gameObject);
		}
		else
		{
			actorGroup.actor = newActor;
		}
	}

	public override string ToString()
	{
		return $"'{base.name}' Cutscene\n Time: {currentTime}";
	}

	public Section GetSectionByName(string name)
	{
		return directorGroup.GetSectionByName(name);
	}

	public Section GetSectionByUID(string UID)
	{
		return directorGroup.GetSectionByUID(UID);
	}

	public Section[] GetSections()
	{
		return directorGroup.sections.ToArray();
	}

	public float GetSectionLength(string name)
	{
		Section sectionByName = directorGroup.GetSectionByName(name);
		if (sectionByName != null)
		{
			Section sectionAfter = directorGroup.GetSectionAfter(sectionByName.time);
			return (sectionAfter == null) ? (length - sectionByName.time) : (sectionAfter.time - sectionByName.time);
		}
		return -1f;
	}

	public string[] GetSectionNames()
	{
		return directorGroup.sections.Select((Section s) => s.name).ToArray();
	}

	public string[] GetDefinedEventNames()
	{
		List<string> list = new List<string>();
		foreach (DirectorActionTrack item in directorGroup.tracks.OfType<DirectorActionTrack>())
		{
			foreach (SendGlobalMessage item2 in item.actions.OfType<SendGlobalMessage>())
			{
				list.Add(item2.message);
			}
		}
		return list.ToArray();
	}

	public void PreInitialize()
	{
		InitializeTimePointers();
		preInitialized = true;
	}

	public void RenderCutscene(int width, int height, int frameRate, Action<Texture2D[]> Callback)
	{
		if (!Application.isPlaying)
		{
			Debug.LogError("Rendering Cutscene with RenderCutscene function is only meant for runtime", this);
			return;
		}
		if (isActive)
		{
			Debug.LogWarning("You called RenderCutscene to an actively playing Cutscene. The cutscene will now Stop.", this);
			Stop();
		}
		StartCoroutine(Internal_RenderCutscene(width, height, frameRate, Callback));
	}

	private IEnumerator Internal_RenderCutscene(int width, int height, int frameRate, Action<Texture2D[]> Callback)
	{
		List<Texture2D> renderSequence = new List<Texture2D>();
		float sampleRate = 1f / (float)frameRate;
		for (float i = sampleRate; i <= length; i += sampleRate)
		{
			Sample(i);
			yield return new WaitForEndOfFrame();
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, mipmap: false);
			texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			texture.Apply();
			renderSequence.Add(texture);
		}
		Callback(renderSequence.ToArray());
	}
}
