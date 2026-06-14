using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using ExtensionMethods;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestScript : MonoBehaviour
{
	private class GAEOnLoginSuccess
	{
		static GAEOnLoginSuccess()
		{
			GAELoggedIn = true;
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetGaeLogin))
			{
			}
		}
	}

	public enum LevelNames
	{
		Area_17,
		Centre_Perks,
		Dictator_Prison,
		Gulag_Prison,
		Oil_Rig,
		OldWestFort,
		POW_Camp,
		SpacePrison,
		Transport_Boat,
		Transport_Plane
	}

	public enum ResultCode
	{
		Success,
		ErrorMissingResourcce,
		ErrorDeserializeFailure,
		ErrorWaitForPlayerCountTimeout,
		ErrorWaitToBecomeMasterTimeout,
		ErrorOnlineGameListJoinRoomTimeout,
		ErrorLoadLevelTimeout,
		ErrorWaitForGAELoginTimeout,
		ErrorLoadingOnlineGameListScreen,
		ErrorInstantiatingOnlineGameListScreen,
		ErrorFindingOnlineGameListScreenComponents
	}

	public delegate void OnTestScriptCompleteDelegate(ResultCode result);

	public abstract class Command
	{
		private string m_comment;

		private object[] m_commentArgs;

		public string comment
		{
			get
			{
				return ExpandVar(m_comment);
			}
			set
			{
				m_comment = value;
			}
		}

		public object[] commentArgs
		{
			get
			{
				if (m_commentArgs != null && m_commentArgs.Length > 0)
				{
					List<object> list = new List<object>(m_commentArgs.Length);
					object[] array = m_commentArgs;
					foreach (object obj in array)
					{
						if (obj is string input)
						{
							list.Add(ExpandVar(input));
						}
						else
						{
							list.Add(obj);
						}
					}
					return list.ToArray();
				}
				return m_commentArgs;
			}
			set
			{
				m_commentArgs = value;
			}
		}

		public bool disabled { get; set; }

		public float timeoutSeconds { get; set; }

		public abstract IEnumerator Run();

		public void DisplayComment()
		{
			if (!string.IsNullOrEmpty(comment))
			{
				bool active = DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.TestScript);
				DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.TestScript, active: true);
				if (commentArgs != null)
				{
				}
				DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.TestScript, active);
			}
		}
	}

	public class GameLoadLevel : Command
	{
		private string m_levelName;

		private string m_inviteRoomName;

		public string levelName
		{
			get
			{
				return ExpandVar(m_levelName);
			}
			private set
			{
				m_levelName = value;
			}
		}

		public string inviteRoomName
		{
			get
			{
				return ExpandVar(m_inviteRoomName);
			}
			private set
			{
				m_inviteRoomName = value;
			}
		}

		public GameLoadLevel(string levelName, float timeoutSeconds, string inviteRoomName)
		{
			this.levelName = levelName;
			base.timeoutSeconds = timeoutSeconds;
			this.inviteRoomName = inviteRoomName;
		}

		public override IEnumerator Run()
		{
			ResultCode result = ResultCode.Success;
			GlobalStart.GLOBALSTART_MODE lastState = GlobalStart.GetInstance().GetMode();
			bool done = false;
			while (!done)
			{
				if (lastState != GlobalStart.GetInstance().GetMode())
				{
					lastState = GlobalStart.GetInstance().GetMode();
					done = GlobalStart.GLOBALSTART_MODE.IN_LEVEL == GlobalStart.GetInstance().GetMode();
				}
			}
			if (base.timeoutSeconds > 0f && (float)T17NetLoadSync.LevelLoadingTime > base.timeoutSeconds)
			{
				Debug.LogErrorFormat("WaitLoadLevel - Timeout exceeded: Loaded '{0}' in {1}s (>{2}s)", Helpers.GetLoadedSceneName(), T17NetLoadSync.LevelLoadingTime, base.timeoutSeconds);
				result = ResultCode.ErrorLoadLevelTimeout;
			}
			else if (string.IsNullOrEmpty(inviteRoomName))
			{
			}
			yield return result;
		}
	}

	public class WaitLoadLevel : Command
	{
		public WaitLoadLevel(float timeoutSeconds)
		{
			base.timeoutSeconds = timeoutSeconds;
		}

		public override IEnumerator Run()
		{
			ResultCode result = ResultCode.Success;
			GlobalStart.GLOBALSTART_MODE lastState = GlobalStart.GetInstance().GetMode();
			bool done = false;
			while (!done)
			{
				if (lastState != GlobalStart.GetInstance().GetMode())
				{
					lastState = GlobalStart.GetInstance().GetMode();
					done = GlobalStart.GLOBALSTART_MODE.IN_LEVEL == GlobalStart.GetInstance().GetMode();
				}
			}
			if (base.timeoutSeconds > 0f && (float)T17NetLoadSync.LevelLoadingTime > base.timeoutSeconds)
			{
				Debug.LogErrorFormat("WaitLoadLevel - Timeout exceeded: Loaded '{0}' in {1}s (>{2}s)", Helpers.GetLoadedSceneName(), T17NetLoadSync.LevelLoadingTime, base.timeoutSeconds);
				result = ResultCode.ErrorLoadLevelTimeout;
			}
			yield return result;
		}
	}

	public class TitleScreenStartCampaign : Command
	{
		public string levelName { get; private set; }

		public TitleScreenStartCampaign(string levelName, float timeoutSeconds)
		{
			this.levelName = levelName;
			base.timeoutSeconds = timeoutSeconds;
		}

		public override IEnumerator Run()
		{
			ResultCode result = ResultCode.Success;
			GlobalStart.GetInstance().SetCurrentSelectedLevel(levelName, 0);
			GlobalStart.GetInstance().SetupLoadStatesAndStartGameWithConfig();
			GlobalStart.GLOBALSTART_MODE lastState = GlobalStart.GetInstance().GetMode();
			bool done = false;
			while (!done)
			{
				if (lastState != GlobalStart.GetInstance().GetMode())
				{
					lastState = GlobalStart.GetInstance().GetMode();
					done = GlobalStart.GLOBALSTART_MODE.IN_LEVEL == GlobalStart.GetInstance().GetMode();
				}
			}
			if (base.timeoutSeconds > 0f && (float)T17NetLoadSync.LevelLoadingTime > base.timeoutSeconds)
			{
				Debug.LogErrorFormat("TitleScreenStartCampaign - Timeout exceeded: Loaded '{0}' in {1}s (>{2}s)", Helpers.GetLoadedSceneName(), T17NetLoadSync.LevelLoadingTime, base.timeoutSeconds);
				result = ResultCode.ErrorLoadLevelTimeout;
			}
			yield return result;
		}
	}

	public class UIExecuteEvent : Command
	{
		public enum EventType
		{
			BeginDrag,
			Cancel,
			Deselect,
			Drag,
			Drop,
			EndDrag,
			InitializePotentialDrag,
			Move,
			PointerClick,
			PointerDown,
			PointerEnter,
			PointerExit,
			PointerUp,
			Scroll,
			Select,
			Submit,
			UpdateSelected
		}

		public string name { get; private set; }

		public EventType eventType { get; private set; }

		public UIExecuteEvent(string name, EventType eventType)
		{
			this.name = name;
			this.eventType = eventType;
		}

		public override IEnumerator Run()
		{
			if (!string.IsNullOrEmpty(name))
			{
				GameObject gameObject = GameObject.Find(name);
				if (gameObject != null)
				{
					bool flag = false;
					switch (eventType)
					{
					case EventType.BeginDrag:
						if (ExecuteEvents.CanHandleEvent<IBeginDragHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.beginDragHandler);
							flag = true;
						}
						break;
					case EventType.Cancel:
						if (ExecuteEvents.CanHandleEvent<ICancelHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.cancelHandler);
							flag = true;
						}
						break;
					case EventType.Deselect:
						if (ExecuteEvents.CanHandleEvent<IDeselectHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.deselectHandler);
							flag = true;
						}
						break;
					case EventType.Drag:
						if (ExecuteEvents.CanHandleEvent<IDragHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.dragHandler);
							flag = true;
						}
						break;
					case EventType.Drop:
						if (ExecuteEvents.CanHandleEvent<IDropHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.dropHandler);
							flag = true;
						}
						break;
					case EventType.EndDrag:
						if (ExecuteEvents.CanHandleEvent<IEndDragHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.endDragHandler);
							flag = true;
						}
						break;
					case EventType.InitializePotentialDrag:
						if (ExecuteEvents.CanHandleEvent<IInitializePotentialDragHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.initializePotentialDrag);
							flag = true;
						}
						break;
					case EventType.Move:
						if (ExecuteEvents.CanHandleEvent<IMoveHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.moveHandler);
							flag = true;
						}
						break;
					case EventType.PointerClick:
						if (ExecuteEvents.CanHandleEvent<IPointerClickHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
							flag = true;
						}
						break;
					case EventType.PointerDown:
						if (ExecuteEvents.CanHandleEvent<IPointerDownHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
							flag = true;
						}
						break;
					case EventType.PointerEnter:
						if (ExecuteEvents.CanHandleEvent<IPointerEnterHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
							flag = true;
						}
						break;
					case EventType.PointerExit:
						if (ExecuteEvents.CanHandleEvent<IPointerExitHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
							flag = true;
						}
						break;
					case EventType.PointerUp:
						if (ExecuteEvents.CanHandleEvent<IPointerUpHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
							flag = true;
						}
						break;
					case EventType.Scroll:
						if (ExecuteEvents.CanHandleEvent<IScrollHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.scrollHandler);
							flag = true;
						}
						break;
					case EventType.Select:
						if (ExecuteEvents.CanHandleEvent<ISelectHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.selectHandler);
							flag = true;
						}
						break;
					case EventType.Submit:
						if (ExecuteEvents.CanHandleEvent<ISubmitHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
							flag = true;
						}
						break;
					case EventType.UpdateSelected:
						if (ExecuteEvents.CanHandleEvent<IUpdateSelectedHandler>(gameObject))
						{
							ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.updateSelectedHandler);
							flag = true;
						}
						break;
					default:
						Debug.LogErrorFormat("UIExecuteEvent.Run - Switch case does not exist for event type \"{0}\".", eventType.ToString());
						flag = true;
						break;
					}
					if (!flag)
					{
						Debug.LogErrorFormat("UIExecuteEvent.Run - GameObject \"{0}\" can not handle event type \"{1}\".", name, eventType.ToString());
					}
				}
				else
				{
					Debug.LogErrorFormat("UIExecuteEvent.Run - failed to locate GameObject with name \"{0}\".", name);
				}
			}
			else
			{
				Debug.LogErrorFormat("UIExecuteEvent.Run - no name specified.");
			}
			yield return ResultCode.Success;
		}
	}

	public class Wait : Command
	{
		public float waitForSeconds { get; private set; }

		public Wait(float waitForSeconds)
		{
			this.waitForSeconds = waitForSeconds;
		}

		public override IEnumerator Run()
		{
			float endTime = Time.realtimeSinceStartup + waitForSeconds;
			while (Time.realtimeSinceStartup < endTime)
			{
				Thread.Sleep(1000);
			}
			yield return ResultCode.Success;
		}
	}

	public class ForceQuit : Command
	{
		public float waitForSeconds { get; private set; }

		public ForceQuit(float waitForSeconds)
		{
			this.waitForSeconds = waitForSeconds;
		}

		public override IEnumerator Run()
		{
			float endTime = Time.realtimeSinceStartup + waitForSeconds;
			while (Time.realtimeSinceStartup < endTime)
			{
				Thread.Sleep(1000);
			}
			Application.Quit();
			yield return ResultCode.Success;
		}
	}

	public class WaitForPlayerCount : Command
	{
		public int playerCount { get; private set; }

		public WaitForPlayerCount(int playerCount, float timeoutSeconds)
		{
			this.playerCount = playerCount;
			base.timeoutSeconds = timeoutSeconds;
		}

		public override IEnumerator Run()
		{
			ResultCode result = ResultCode.Success;
			float startTime = Time.realtimeSinceStartup;
			Gamer.GetNumLocalGamers();
			while (result == ResultCode.Success && Gamer.GetNumLocalGamers() < playerCount)
			{
				if (Time.realtimeSinceStartup - startTime > base.timeoutSeconds)
				{
					Debug.LogError("WaitForPlayerCount - timeout exceeded.");
					result = ResultCode.ErrorWaitForPlayerCountTimeout;
				}
			}
			yield return result;
		}
	}

	public class WaitToBecomeMaster : Command
	{
		public WaitToBecomeMaster(float timeoutSeconds)
		{
			base.timeoutSeconds = timeoutSeconds;
		}

		public override IEnumerator Run()
		{
			ResultCode result = ResultCode.Success;
			float startTime = Time.realtimeSinceStartup;
			while (result == ResultCode.Success && !T17NetManager.IsMasterClient)
			{
				if (Time.realtimeSinceStartup - startTime > base.timeoutSeconds)
				{
					Debug.LogError("WaitToBecomeMaster - timeout exceeded.");
					result = ResultCode.ErrorWaitToBecomeMasterTimeout;
				}
			}
			yield return result;
		}
	}

	public class SubScript : Command
	{
		public string scriptName { get; private set; }

		public List<Command> commands { get; private set; }

		public SubScript(string scriptName, List<Command> commands)
		{
			this.scriptName = scriptName;
			this.commands = commands;
		}

		public override IEnumerator Run()
		{
			if (!string.IsNullOrEmpty(scriptName))
			{
				commands = new List<Command>(ReadCommands(scriptName));
			}
			if (m_result == ResultCode.Success)
			{
				foreach (Command command in commands)
				{
					if (!command.disabled)
					{
						command.DisplayComment();
						MonoBehaviorExtensions.Coroutine<ResultCode> routine = Instance.StartCoroutine<ResultCode>(command.Run());
						yield return routine.coroutine;
						if ((m_result = routine.Value) != 0)
						{
							break;
						}
					}
				}
			}
			yield return m_result;
		}
	}

	public class ForEach : Command
	{
		public string item { get; private set; }

		public string collection { get; private set; }

		public string scriptName { get; private set; }

		public List<Command> commands { get; private set; }

		public ForEach(string item, string collection, string scriptName, List<Command> commands)
		{
			this.item = item;
			this.collection = collection;
			this.scriptName = scriptName;
			this.commands = commands;
		}

		public override IEnumerator Run()
		{
			List<string> values = new List<string>();
			Type enumType = Type.GetType(collection);
			values = ((enumType == null || !enumType.IsEnum) ? JsonConvert.DeserializeObject<List<string>>(collection) : new List<string>(Enum.GetNames(enumType)));
			if (values != null && values.Count > 0)
			{
				string key = string.Format("{0}{1}", "$", item);
				foreach (string strValue in values)
				{
					m_variableExpansion[key] = strValue;
					if (!string.IsNullOrEmpty(scriptName))
					{
						commands = new List<Command>(ReadCommands(scriptName));
					}
					if (m_result != 0 || commands == null || commands.Count <= 0)
					{
						continue;
					}
					foreach (Command command in commands)
					{
						if (!command.disabled)
						{
							command.DisplayComment();
							MonoBehaviorExtensions.Coroutine<ResultCode> routine = Instance.StartCoroutine<ResultCode>(command.Run());
							yield return routine.coroutine;
							if ((m_result = routine.Value) != 0)
							{
								break;
							}
						}
					}
				}
				m_variableExpansion.Remove(key);
			}
			yield return m_result;
		}
	}

	private static GAEOnLoginSuccess m_sGAEOnLoginSuccess = new GAEOnLoginSuccess();

	private static TestScript m_instance = null;

	private static ResultCode m_result = ResultCode.Success;

	private static Dictionary<string, string> m_variableExpansion = new Dictionary<string, string>(StringComparer.Ordinal);

	private const string VarPrefix = "$";

	public static bool GAELoggedIn { get; private set; }

	public static TestScript Instance => m_instance;

	public void Awake()
	{
		if (m_instance != null)
		{
			Debug.LogError("More than one TestScript instance has been created, it expects to be a singleton.", this);
		}
		else
		{
			m_instance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
		}
	}

	private static string ExpandVar(string input)
	{
		string value = null;
		if (!string.IsNullOrEmpty(input))
		{
			if (input.StartsWith("$"))
			{
				if (!m_variableExpansion.TryGetValue(input, out value))
				{
					value = input;
				}
			}
			else
			{
				value = input;
			}
		}
		return value;
	}

	public void Execute(string scriptName, OnTestScriptCompleteDelegate onComplete)
	{
		ReadOnlyCollection<Command> commands = ReadCommands(scriptName);
		if (m_result == ResultCode.Success)
		{
			StartCoroutine(RunCommands(commands, onComplete));
		}
	}

	private static ReadOnlyCollection<Command> ReadCommands(string filename)
	{
		TextAsset textAsset = (TextAsset)Resources.Load("TestScripts/" + filename, typeof(TextAsset));
		if (textAsset != null)
		{
			return ReadCommands(textAsset);
		}
		m_result = ResultCode.ErrorMissingResourcce;
		return new List<Command>().AsReadOnly();
	}

	private static ReadOnlyCollection<Command> ReadCommands(TextAsset txt)
	{
		List<Command> list;
		try
		{
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
			JsonSerializerSettings settings = jsonSerializerSettings;
			list = JsonConvert.DeserializeObject<List<Command>>(txt.text, settings);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			list = new List<Command>();
			m_result = ResultCode.ErrorDeserializeFailure;
		}
		return list.AsReadOnly();
	}

	private IEnumerator RunCommands(IEnumerable<Command> commands, OnTestScriptCompleteDelegate onComplete)
	{
		foreach (Command command in commands)
		{
			if (!command.disabled)
			{
				command.DisplayComment();
				MonoBehaviorExtensions.Coroutine<ResultCode> routine = this.StartCoroutine<ResultCode>(command.Run());
				yield return routine.coroutine;
				if ((m_result = routine.Value) != 0)
				{
					break;
				}
			}
		}
		onComplete?.Invoke(m_result);
	}
}
