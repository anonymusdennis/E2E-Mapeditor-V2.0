using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CSharpZombieDetector;

public class ZombieObjectDetector : MonoBehaviour
{
	[Flags]
	public enum LoggingOptions
	{
		IgnoredMembers = 1,
		FieldInfoNotFound = 2,
		MemberType = 4,
		FieldType = 8,
		AlreadySearched = 0x10,
		InvalidType = 0x20,
		Exceptions = 0x40,
		ListBadEquals = 0x80,
		ListScannedObjects = 0x100,
		StaticFieldZombieCount = 0x200,
		FullZombieStackTrace = 0x400
	}

	private static ZombieObjectDetector s_Instance;

	[SerializeField]
	private LoggingOptions m_LoggingOptions = LoggingOptions.FullZombieStackTrace;

	[SerializeField]
	private string m_LogTag = string.Empty;

	[SerializeField]
	private string[] m_IgnoredTypeStrings = new string[0];

	[SerializeField]
	private string[] m_TypesToScanStrings = new string[0];

	[SerializeField]
	private KeyCode m_LogZombieKeyCode;

	private bool m_IsLogging;

	private Stack<MemberInfo> m_MemberInfoChain = new Stack<MemberInfo>();

	private Stack<object> m_ScannedObjects = new Stack<object>();

	private List<Type> m_InvalidTypes = new List<Type>();

	private float m_Progress;

	private float m_ProgressIncrements;

	private object m_TestObject = new object();

	private int m_ZombieObjectCount;

	private bool m_HasFoundZombies;

	private const BindingFlags k_AllStaticFields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

	private const BindingFlags k_AllFields = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

	private const BindingFlags k_AllNonStaticFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

	private StreamWriter m_FileOutputStream;

	private string m_LogFileFolder;

	public bool m_bDoZombieDetect;

	private void Awake()
	{
		m_LogFileFolder = Path.Combine(Application.dataPath, ".ZombieLogs");
	}

	protected void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
	}

	private void Update()
	{
		if (m_bDoZombieDetect)
		{
			m_bDoZombieDetect = false;
			StartCoroutine(LogZombies());
		}
	}

	private void OnGUI()
	{
		if (m_IsLogging)
		{
			GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), "Logging Zombie Objects!");
		}
	}

	private IEnumerator LogZombies()
	{
		m_IsLogging = true;
		yield return new WaitForSeconds(0.5f);
		LogAllZombieObjects();
		m_IsLogging = false;
		yield return null;
	}

	public void LogAllZombieObjects()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
		if (m_LoggingOptions == (LoggingOptions)0)
		{
			Debug.LogWarning("No Logging Options Set");
			return;
		}
		CreateFile();
		m_MemberInfoChain.Clear();
		m_ScannedObjects.Clear();
		m_InvalidTypes.Clear();
		Log("Logging Zombie Objects");
		Log(string.Format("Logging Options: {0}{1}IgnoredTypes: {2}{1}ScannedTypes: {3}", m_LoggingOptions.ToString(), Environment.NewLine, string.Join(", ", m_IgnoredTypeStrings), string.Join(", ", m_TypesToScanStrings)));
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Type[] types = executingAssembly.GetTypes();
		m_ProgressIncrements = 1f / (float)types.Length;
		m_Progress = 0f;
		Type[] array = types;
		foreach (Type type in array)
		{
			m_Progress += m_ProgressIncrements;
			if (IsValidZombieType(type) && (m_TypesToScanStrings.Length <= 0 || m_TypesToScanStrings.Contains(type.FullName)))
			{
				m_MemberInfoChain.Clear();
				TraverseStaticMembersFromType(type);
			}
		}
		if (HasLoggingOptions(LoggingOptions.ListBadEquals) && m_InvalidTypes.Count > 0)
		{
			Log($"Bad Implementation of .Equals In Types: {m_InvalidTypes.Count.ToString()}{Environment.NewLine}{string.Join(Environment.NewLine, m_InvalidTypes.Select((Type x) => x.ToString()).Reverse().ToArray())}");
		}
		if (HasLoggingOptions(LoggingOptions.ListScannedObjects) && m_ScannedObjects.Count > 0)
		{
			Log($"Scanned Objects: {m_ScannedObjects.Count.ToString()}{Environment.NewLine}{string.Join(Environment.NewLine, m_ScannedObjects.Select((object x) => x.GetType().ToString()).Reverse().ToArray())}");
		}
		if (m_HasFoundZombies)
		{
			Log("Scan Complete: Zombie Objects Found. Total Scanned Objects:" + m_ScannedObjects.Count);
		}
		else
		{
			Log("Scan Complete: No Zombie Objects Found");
		}
		CloseFile();
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
		m_ScannedObjects.Clear();
	}

	private void TraverseStaticMembersFromType(Type type)
	{
		List<MemberInfo> list = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
		foreach (MemberInfo item in list)
		{
			m_ZombieObjectCount = 0;
			m_MemberInfoChain.Push(item);
			switch (item.MemberType)
			{
			case MemberTypes.Event:
				if (HasLoggingOptions(LoggingOptions.MemberType))
				{
					Log($"Static Event:{DebugMemberInfo(item)}");
				}
				TraverseMemberEvent(item, null);
				break;
			case MemberTypes.Field:
				if (HasLoggingOptions(LoggingOptions.MemberType))
				{
					Log($"Static Field:{DebugMemberInfo(item)}");
				}
				TraverseMemberField(item, null);
				break;
			default:
				if (HasLoggingOptions(LoggingOptions.IgnoredMembers))
				{
					Log("Member Ignored:" + item.MemberType);
				}
				break;
			}
			m_MemberInfoChain.Pop();
			if (HasLoggingOptions(LoggingOptions.StaticFieldZombieCount) && m_ZombieObjectCount > 0)
			{
				LogWarning($"Number of ZombieObjects: {m_ZombieObjectCount}{Environment.NewLine}{DebugMemberInfo(item)}");
			}
		}
	}

	private void TraverseMemberEvent(MemberInfo memberInfo, object memberParentObject)
	{
		FieldInfo field = memberInfo.ReflectedType.GetField(memberInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (field != null)
		{
			if (!IsValidZombieType(field.FieldType) || !(field.GetValue(memberParentObject) is MulticastDelegate multicastDelegate))
			{
				return;
			}
			Delegate[] invocationList = multicastDelegate.GetInvocationList();
			Delegate[] array = invocationList;
			foreach (Delegate @delegate in array)
			{
				object target = @delegate.Target;
				if (target != null)
				{
					TraverseAllMembersFromObject(target, memberInfo);
					continue;
				}
				Log("Potentially leaked unity object for " + @delegate.Method.Name + " under " + ((memberParentObject == null) ? "null parent" : memberParentObject.ToString()) + " " + DebugMemberInfo(memberInfo));
				if (!@delegate.Method.Name.Contains("Gamer"))
				{
				}
			}
		}
		else if (HasLoggingOptions(LoggingOptions.FieldInfoNotFound))
		{
			Log($"Failed to find FieldInfo: Type: {memberInfo.ReflectedType} Member: {memberInfo.Name}");
		}
	}

	private void TraverseMemberField(MemberInfo memberInfo, object memberParentObject)
	{
		FieldInfo field = memberInfo.ReflectedType.GetField(memberInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (field == null)
		{
			return;
		}
		if (IsValidZombieType(field.FieldType))
		{
			if (!field.FieldType.IsArray)
			{
				try
				{
					if (HasLoggingOptions(LoggingOptions.FieldType))
					{
						Log($"Other: {DebugMemberInfo(memberInfo)}");
					}
					object value = field.GetValue(memberParentObject);
					if (value != null)
					{
						TraverseAllMembersFromObject(value, memberInfo);
					}
					return;
				}
				catch (Exception ex)
				{
					if (HasLoggingOptions(LoggingOptions.Exceptions))
					{
						Debug.LogErrorFormat("Error: {0}{1}{2}", DebugMemberInfo(memberInfo), Environment.NewLine, ex);
					}
					return;
				}
			}
			if (HasLoggingOptions(LoggingOptions.FieldType))
			{
				Log($"Array: {DebugMemberInfo(memberInfo)}");
			}
			if (!(field.GetValue(memberParentObject) is IEnumerable enumerable))
			{
				return;
			}
			{
				foreach (object item in enumerable)
				{
					if (item != null)
					{
						TraverseAllMembersFromObject(item, memberInfo);
					}
				}
				return;
			}
		}
		if (HasLoggingOptions(LoggingOptions.FieldInfoNotFound))
		{
			Log($"Failed to find FieldInfo: Type: {memberInfo.ReflectedType} Member: {memberInfo.Name}");
		}
	}

	private void TraverseAllMembersFromObject(object obj, MemberInfo sourceMemberInfo)
	{
		if (!IsValidZombieType(obj.GetType()))
		{
			return;
		}
		try
		{
			obj.Equals(m_TestObject);
			if (m_ScannedObjects.Contains(obj))
			{
				if (HasLoggingOptions(LoggingOptions.AlreadySearched))
				{
					Log("Debug: Already Searched: " + obj.GetType());
				}
				return;
			}
		}
		catch (Exception ex)
		{
			if (HasLoggingOptions(LoggingOptions.Exceptions))
			{
				Debug.LogErrorFormat("Error In Objects .Equals: {0}{1}{2}", obj.GetType(), Environment.NewLine, ex);
			}
			if (!m_InvalidTypes.Contains(obj.GetType()))
			{
				m_InvalidTypes.Add(obj.GetType());
			}
			return;
		}
		m_ScannedObjects.Push(obj);
		LogIsZombieObject(obj);
		List<MemberInfo> list = obj.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToList();
		foreach (MemberInfo item in list)
		{
			m_MemberInfoChain.Push(item);
			switch (item.MemberType)
			{
			case MemberTypes.Event:
				if (HasLoggingOptions(LoggingOptions.MemberType))
				{
					Log($"Event:{DebugMemberInfo(item)}");
				}
				TraverseMemberEvent(item, obj);
				break;
			case MemberTypes.Field:
				if (HasLoggingOptions(LoggingOptions.MemberType))
				{
					Log($"Field:{DebugMemberInfo(item)}");
				}
				TraverseMemberField(item, obj);
				break;
			default:
				if (HasLoggingOptions(LoggingOptions.IgnoredMembers))
				{
					Log("Member Ignored:" + item.MemberType);
				}
				break;
			}
			m_MemberInfoChain.Pop();
		}
	}

	private void LogIsZombieObject(object obj)
	{
		if (obj.ToString() == "null")
		{
			m_HasFoundZombies = true;
			m_ZombieObjectCount++;
			if (HasLoggingOptions(LoggingOptions.FullZombieStackTrace))
			{
				LogWarning($"ZombiedObject: {obj.GetType()}{Environment.NewLine}{DebugMemberInfoChain()}");
			}
		}
	}

	private string DebugMemberInfoChain()
	{
		return string.Join(Environment.NewLine, m_MemberInfoChain.Select((MemberInfo x) => DebugMemberInfo(x)).ToArray());
	}

	private string DebugMemberInfo(MemberInfo memberInfo)
	{
		return $"->Class: {memberInfo.ReflectedType}: Member: {memberInfo.ToString()}";
	}

	private bool IsValidZombieType(Type type)
	{
		bool result = false;
		if (!TypeHelper.IsZombieType(type))
		{
			if (HasLoggingOptions(LoggingOptions.InvalidType))
			{
				Log($"InvalidType: Basic value types: {type}");
			}
		}
		else if (m_InvalidTypes.Contains(type))
		{
			if (HasLoggingOptions(LoggingOptions.InvalidType))
			{
				Log($"InvalidType: Already found: {type}");
			}
		}
		else if (type.IsPointer)
		{
			if (HasLoggingOptions(LoggingOptions.InvalidType))
			{
				Log($"InvalidType: C# style pointer: {type}");
			}
		}
		else if (type.ContainsGenericParameters || type.IsGenericParameter || type.IsGenericTypeDefinition)
		{
			if (HasLoggingOptions(LoggingOptions.InvalidType))
			{
				Log(string.Format("InvalidFieldType: GenericParamType: {0}{1}{2}", type, Environment.NewLine, "Ex Singleton<T> : MonoBehaviour where T: MonoBehaviour"));
			}
		}
		else if (m_IgnoredTypeStrings.Contains(type.FullName))
		{
			if (HasLoggingOptions(LoggingOptions.InvalidType))
			{
				Log($"InvalidType: User Defined Ignore: {type}");
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	private void CreateFile()
	{
		try
		{
			Directory.CreateDirectory(m_LogFileFolder);
			m_FileOutputStream = File.CreateText(Path.Combine(m_LogFileFolder, string.Format("{0}_{1}_{2}{3}", "ZombieDetector", DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss"), m_LogTag, ".log")));
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			m_FileOutputStream = null;
		}
	}

	private void CloseFile()
	{
		if (m_FileOutputStream != null)
		{
			m_FileOutputStream.Close();
			Debug.Log("ZombieLog Saved to: " + m_LogFileFolder);
		}
		else
		{
			Debug.Log("ZombieLog Failed to Save to: " + m_LogFileFolder);
		}
	}

	private bool HasLoggingOptions(LoggingOptions loggingOptionsToCheckFor)
	{
		return (m_LoggingOptions & loggingOptionsToCheckFor) != 0;
	}

	private void Log(string outputText)
	{
		Debug.Log(outputText);
		if (m_FileOutputStream != null)
		{
			m_FileOutputStream.WriteLine(Environment.NewLine + outputText);
		}
	}

	private void LogWarning(string outputText)
	{
		Debug.LogWarning(outputText);
		if (m_FileOutputStream != null)
		{
			m_FileOutputStream.WriteLine(Environment.NewLine + outputText);
		}
	}

	public static ZombieObjectDetector GetInstance()
	{
		if (s_Instance == null)
		{
			GameObject gameObject = new GameObject("ZombieObjectDetector");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			s_Instance = gameObject.AddComponent<ZombieObjectDetector>();
			s_Instance.m_LogFileFolder = Path.Combine(Application.dataPath, ".ZombieLogs");
		}
		return s_Instance;
	}
}
