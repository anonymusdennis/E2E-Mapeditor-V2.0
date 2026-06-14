using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;

public class DebugHelpers
{
	public enum LogNetGroup
	{
		NetPhotonError,
		NetPhotonEvent,
		NetPhotonRpc,
		NetPhotonViewList,
		NetViewID,
		NetLobbyRoomState,
		NetSendRpcWhenConnected,
		NetOpRaiseEvent,
		NetCustomProperty,
		NetConnectionState,
		NetClientDisconnect,
		NetModeTransition,
		NetLevelLoadState,
		NetLogLevelLoadState,
		NetAnalytics,
		NetworkAI,
		NetworkPlayers,
		NetworkPlayerPos,
		NetGaeLogin,
		NetBluePrintDetails,
		NetPrisonViewDetails
	}

	public enum LogGroup
	{
		TestScript,
		PlayerCreate,
		PlayerSpawn,
		PlayerUpdate,
		PlayerGamer,
		PlayerInventory,
		PlayerEffects,
		LevelLoadState,
		PlayerAnimSingleShot,
		SpeechEvent
	}

	[Flags]
	public enum Prefix
	{
		[Prefix("get_LocalTimeString")]
		ShowLocalTime = 1,
		[Prefix("get_ServerTimeString")]
		ShowServerTime = 2,
		[Prefix("get_PingString")]
		ShowPing = 4,
		[Prefix("get_ServerAddressString")]
		ShowServer = 8
	}

	public class PrefixEqualityComparer : IEqualityComparer<Prefix>
	{
		private static PrefixEqualityComparer defaultInstance;

		public static PrefixEqualityComparer Default
		{
			get
			{
				if (defaultInstance == null)
				{
					defaultInstance = new PrefixEqualityComparer();
				}
				return defaultInstance;
			}
		}

		public bool Equals(Prefix x, Prefix y)
		{
			return x == y;
		}

		public int GetHashCode(Prefix obj)
		{
			return (int)obj;
		}
	}

	private class PrefixAttribute : Attribute
	{
		public MethodInfo methodInfo { get; private set; }

		public PrefixAttribute(string propertyName)
		{
			methodInfo = typeof(DebugHelpers).GetMethod(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private PrefixAttribute()
		{
			throw new NotImplementedException();
		}
	}

	public static BitArray ActiveLogNetGroups;

	public static BitArray ActiveLogGroups;

	public static Prefix ActivePrefixes;

	private static Dictionary<Prefix, MethodInfo> m_prefixMethods;

	private static PrimitiveDrawer m_sPrimitiveDrawer;

	public static EnumCacheSlow<Prefix> PrefixCache;

	private static StringBuilder m_prefixSB;

	private static StringBuilder LineFileSB;

	private static StringBuilder LogMethodSB;

	private static Dictionary<int, HashSet<string>> m_loggedErrorFormat;

	public static string LocalTimeString
	{
		get
		{
			if (T17NetTimeManager.Instance != null)
			{
				return $"TS: {T17NetTimeManager.Instance.GetLocalMachineTimeStamp()} [{T17NetTimeManager.Instance.GetTimeModeString(T17NetTimeManager.ServerTimeMode.LocalTimeMode)}]";
			}
			return $"TS: ";
		}
	}

	public static string ServerTimeString
	{
		get
		{
			if (T17NetTimeManager.Instance != null)
			{
				return $"TS: {T17NetTimeManager.Instance.GetServerTimeStamp()} [{T17NetTimeManager.Instance.GetServerTimeMode()}]";
			}
			return $"TS: ";
		}
	}

	public static string PingString => $"Ping: {PhotonNetwork.networkingPeer.RoundTripTime} [+/-{PhotonNetwork.networkingPeer.RoundTripTimeVariance}] ms";

	public static string ServerAddressString
	{
		get
		{
			string serverAddress = PhotonNetwork.ServerAddress;
			if (!string.IsNullOrEmpty(serverAddress))
			{
				return $"SIP:{serverAddress}";
			}
			return string.Empty;
		}
	}

	public static string LineFile
	{
		get
		{
			LogLineFileInternal(null, null, 3, LineFileSB);
			return LineFileSB.ToString();
		}
	}

	static DebugHelpers()
	{
		ActiveLogNetGroups = new BitArray(Enum.GetNames(typeof(LogNetGroup)).Length, defaultValue: false);
		ActiveLogGroups = new BitArray(Enum.GetNames(typeof(LogGroup)).Length, defaultValue: false);
		m_prefixMethods = new Dictionary<Prefix, MethodInfo>(PrefixEqualityComparer.Default);
		PrefixCache = new EnumCacheSlow<Prefix>();
		m_prefixSB = new StringBuilder(256);
		LineFileSB = new StringBuilder(128);
		LogMethodSB = new StringBuilder(512);
		m_loggedErrorFormat = new Dictionary<int, HashSet<string>>(128);
		foreach (Prefix value2 in Enum.GetValues(typeof(Prefix)))
		{
			Enum value = PrefixCache.Get(value2);
			m_prefixMethods.Add(value2, EnumHelpers.GetAttribute<PrefixAttribute>(ref value).methodInfo);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void PrimitiveDrawerLine(Vector3 v1, Vector3 v2, Color? colour = null, float duration = 0f)
	{
		m_sPrimitiveDrawer = ((!(m_sPrimitiveDrawer == null)) ? m_sPrimitiveDrawer : ((PrimitiveDrawer)GlobalStart.PrimitiveDrawer));
		if (m_sPrimitiveDrawer != null)
		{
			m_sPrimitiveDrawer.AddLine(v1, v2, (!colour.HasValue) ? Color.white : colour.Value, duration);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void PrimitiveDrawerTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color? colour = null, float duration = 0f)
	{
		m_sPrimitiveDrawer = ((!(m_sPrimitiveDrawer == null)) ? m_sPrimitiveDrawer : ((PrimitiveDrawer)GlobalStart.PrimitiveDrawer));
		if (m_sPrimitiveDrawer != null)
		{
			m_sPrimitiveDrawer.AddTriangle(v1, v2, v3, (!colour.HasValue) ? Color.white : colour.Value, duration);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void PrimitiveDrawerCircleXZ(Vector3 centre, float radius = 1f, Color? _colour = null, uint numSegments = 12u, float duration = 0f)
	{
		m_sPrimitiveDrawer = ((!(m_sPrimitiveDrawer == null)) ? m_sPrimitiveDrawer : ((PrimitiveDrawer)GlobalStart.PrimitiveDrawer));
		if (m_sPrimitiveDrawer != null && numSegments != 0)
		{
			Color colour = ((!_colour.HasValue) ? Color.white : _colour.Value);
			float num = (float)Math.PI * 2f / (float)numSegments;
			Vector3 from = centre + new Vector3(radius * Mathf.Sin(0f), 0f, radius * Mathf.Cos(0f));
			for (uint num2 = 0u; num2 <= numSegments; num2++)
			{
				Vector3 vector = centre + new Vector3(radius * Mathf.Sin(num * (float)num2), 0f, radius * Mathf.Cos(num * (float)num2));
				m_sPrimitiveDrawer.AddLine(from, vector, colour, duration);
				from = vector;
			}
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void PrimitiveDrawerCrosshair(Vector3 centre, float size = 1f, Color? _colour = null, float duration = 0f)
	{
		m_sPrimitiveDrawer = ((!(m_sPrimitiveDrawer == null)) ? m_sPrimitiveDrawer : ((PrimitiveDrawer)GlobalStart.PrimitiveDrawer));
		if (m_sPrimitiveDrawer != null)
		{
			Color colour = ((!_colour.HasValue) ? Color.white : _colour.Value);
			Vector3 from = centre + new Vector3(size, 0f, 0f);
			Vector3 to = centre - new Vector3(size, 0f, 0f);
			m_sPrimitiveDrawer.AddLine(from, to, colour, duration);
			from = centre + new Vector3(0f, 0f, size);
			to = centre - new Vector3(0f, 0f, size);
			m_sPrimitiveDrawer.AddLine(from, to, colour, duration);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCircle(Vector3 centre, Vector3 up, float radius = 1f, Color? _colour = null, uint numSegments = 12u, float duration = 0f, bool depthTest = true)
	{
		if (numSegments != 0)
		{
			Color color = ((!_colour.HasValue) ? Color.white : _colour.Value);
			float num = (float)Math.PI * 2f / (float)numSegments;
			Vector3 vector = up.normalized * radius;
			Vector3 rhs = Vector3.Slerp(vector, -vector, 0.5f);
			Vector3 vector2 = Vector3.Cross(vector, rhs).normalized * radius;
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x[0] = vector2.x;
			matrix4x[1] = vector2.y;
			matrix4x[2] = vector2.z;
			matrix4x[4] = vector.x;
			matrix4x[5] = vector.y;
			matrix4x[6] = vector.z;
			matrix4x[8] = rhs.x;
			matrix4x[9] = rhs.y;
			matrix4x[10] = rhs.z;
			Vector3 start = centre + matrix4x.MultiplyPoint3x4(Vector3.right);
			Vector3 vector3 = default(Vector3);
			for (uint num2 = 0u; num2 <= numSegments; num2++)
			{
				vector3.x = Mathf.Cos(num * (float)num2);
				vector3.z = Mathf.Sin(num * (float)num2);
				vector3.y = 0f;
				vector3 = centre + matrix4x.MultiplyPoint3x4(vector3);
				UnityEngine.Debug.DrawLine(start, vector3, color, duration, depthTest);
				start = vector3;
			}
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCircleXZ(Vector3 centre, float radius = 1f, Color? colour = null, uint numSegments = 12u, float duration = 0f, bool depthTest = true)
	{
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCrosshair(Vector3 centre, float size = 1f, Color? _colour = null, float duration = 0f, bool depthTest = true)
	{
		Color color = ((!_colour.HasValue) ? Color.white : _colour.Value);
		Vector3 start = centre + new Vector3(size, 0f, 0f);
		Vector3 end = centre - new Vector3(size, 0f, 0f);
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
		start = centre + new Vector3(0f, 0f, size);
		end = centre - new Vector3(0f, 0f, size);
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCapsule(Vector3 start, Vector3 end, float radius, Color? _colour = null, uint numSegments = 12u, float duration = 0f, bool depthTest = true)
	{
		Color color = ((!_colour.HasValue) ? Color.white : _colour.Value);
		Vector3 vector = (end - start).normalized * radius;
		Vector3 vector2 = Vector3.Slerp(vector, -vector, 0.5f);
		Vector3 vector3 = Vector3.Cross(vector, vector2).normalized * radius;
		float magnitude = (start - end).magnitude;
		float num = Mathf.Max(0f, magnitude * 0.5f);
		Vector3 vector4 = (end + start) * 0.5f;
		start = vector4 + (start - vector4).normalized * num;
		end = vector4 + (end - vector4).normalized * num;
		UnityEngine.Debug.DrawLine(start + vector3, end + vector3, color, duration, depthTest);
		UnityEngine.Debug.DrawLine(start - vector3, end - vector3, color, duration, depthTest);
		UnityEngine.Debug.DrawLine(start + vector2, end + vector2, color, duration, depthTest);
		UnityEngine.Debug.DrawLine(start - vector2, end - vector2, color, duration, depthTest);
		for (int i = 1; i <= numSegments; i++)
		{
			float t = (float)i / (float)numSegments;
			float t2 = (float)(i - 1) / (float)numSegments;
			UnityEngine.Debug.DrawLine(Vector3.Slerp(vector3, -vector, t) + start, Vector3.Slerp(vector3, -vector, t2) + start, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(-vector3, -vector, t) + start, Vector3.Slerp(-vector3, -vector, t2) + start, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(vector2, -vector, t) + start, Vector3.Slerp(vector2, -vector, t2) + start, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(-vector2, -vector, t) + start, Vector3.Slerp(-vector2, -vector, t2) + start, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(vector3, vector, t) + end, Vector3.Slerp(vector3, vector, t2) + end, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(-vector3, vector, t) + end, Vector3.Slerp(-vector3, vector, t2) + end, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(vector2, vector, t) + end, Vector3.Slerp(vector2, vector, t2) + end, color, duration, depthTest);
			UnityEngine.Debug.DrawLine(Vector3.Slerp(-vector2, vector, t) + end, Vector3.Slerp(-vector2, vector, t2) + end, color, duration, depthTest);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawSphere(Vector3 centre, float radius = 1f, Color? _colour = null, uint numSegments = 12u, float duration = 0f, bool depthTest = true)
	{
		if (numSegments != 0)
		{
			Color color = ((!_colour.HasValue) ? Color.white : _colour.Value);
			float num = (float)Math.PI * 2f / (float)numSegments;
			Vector3 start = new Vector3(centre.x, centre.y, centre.z + radius);
			Vector3 start2 = new Vector3(centre.x + radius, centre.y, centre.z);
			Vector3 start3 = new Vector3(centre.x + radius, centre.y, centre.z);
			for (uint num2 = 0u; num2 <= numSegments; num2++)
			{
				Vector3 vector = new Vector3(centre.x, centre.y + radius * Mathf.Sin(num * (float)num2), centre.z + radius * Mathf.Cos(num * (float)num2));
				Vector3 vector2 = new Vector3(centre.x + radius * Mathf.Cos(num * (float)num2), centre.y, centre.z + radius * Mathf.Sin(num * (float)num2));
				Vector3 vector3 = new Vector3(centre.x + radius * Mathf.Cos(num * (float)num2), centre.y + radius * Mathf.Sin(num * (float)num2), centre.z);
				UnityEngine.Debug.DrawLine(start, vector, color, duration, depthTest);
				UnityEngine.Debug.DrawLine(start2, vector2, color, duration, depthTest);
				UnityEngine.Debug.DrawLine(start3, vector3, color, duration, depthTest);
				start = vector;
				start2 = vector2;
				start3 = vector3;
			}
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCross(Vector3 position, float size, Color colour, float time = 0f)
	{
		UnityEngine.Debug.DrawLine(position - new Vector3(size, 0f, 0f), position + new Vector3(size, 0f, 0f), colour, time);
		UnityEngine.Debug.DrawLine(position - new Vector3(0f, size, 0f), position + new Vector3(0f, size, 0f), colour, time);
		UnityEngine.Debug.DrawLine(position - new Vector3(0f, 0f, size), position + new Vector3(0f, 0f, size), colour, time);
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void DebugDrawCross(Vector3 position, Color colour, float size, bool inScene, float maxYDelta, float referenceY)
	{
		if (!(Mathf.Abs(position.y - referenceY) > maxYDelta))
		{
			float num = size / 2f;
			if (!inScene)
			{
			}
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogFormat(LogNetGroup logNetGroup, string format, params object[] args)
	{
		if (!LogGroupActive(logNetGroup))
		{
		}
	}

	public static bool LogGroupActive(LogGroup logGroup)
	{
		return ActiveLogGroups.Get((int)logGroup);
	}

	public static void LogGroupActive(LogGroup logGroup, bool active)
	{
		ActiveLogGroups.Set((int)logGroup, active);
	}

	public static bool LogGroupActive(LogNetGroup logGroup)
	{
		return ActiveLogNetGroups.Get((int)logGroup);
	}

	public static void LogGroupActive(LogNetGroup logNetGroup, bool active)
	{
		ActiveLogNetGroups.Set((int)logNetGroup, active);
	}

	public static bool PrefixActive(Prefix prefix)
	{
		return (ActivePrefixes & prefix) == prefix;
	}

	public static void PrefixActive(Prefix prefix, bool active)
	{
		if (active)
		{
			ActivePrefixes |= prefix;
		}
		else
		{
			ActivePrefixes &= ~prefix;
		}
	}

	public static string MakePrefix(LogGroup logGroup)
	{
		return MakePrefix(logGroup.ToString());
	}

	public static string MakeNetPrefix(LogNetGroup logNetGroup)
	{
		return MakePrefix(logNetGroup.ToString());
	}

	internal static string MakePrefix(string _prefix)
	{
		m_prefixSB.Length = 0;
		if (ActivePrefixes != 0)
		{
			Dictionary<Prefix, MethodInfo>.Enumerator enumerator = m_prefixMethods.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Prefix key = enumerator.Current.Key;
				if (PrefixActive(key))
				{
					MethodInfo value = enumerator.Current.Value;
					if (value != null)
					{
						m_prefixSB.Append((string)value.Invoke(null, null));
						m_prefixSB.Append(" ¶ ");
					}
				}
			}
		}
		return m_prefixSB.ToString();
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogErrorFormat(LogGroup logGroup, string format, params object[] args)
	{
		if (!LogGroupActive(logGroup))
		{
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogFormat(LogGroup logGroup, string format, params object[] args)
	{
		if (!LogGroupActive(logGroup))
		{
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogFormat(LogGroup logGroup, UnityEngine.Object context, string format, params object[] args)
	{
		if (!LogGroupActive(logGroup))
		{
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogLineFile(LogNetGroup logNetGroup, UnityEngine.Object context = null)
	{
		if (LogGroupActive(logNetGroup))
		{
			LogLineFileInternal(MakeNetPrefix(logNetGroup), context, 3, null);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogLineFile(LogGroup logGroup, UnityEngine.Object context = null)
	{
		if (LogGroupActive(logGroup))
		{
			LogLineFileInternal(MakePrefix(logGroup), context, 3, null);
		}
	}

	private static void LogLineFileInternal(string _prefix, UnityEngine.Object context, int targetStackFrame, StringBuilder buffer)
	{
		try
		{
			string value = ((!string.IsNullOrEmpty(_prefix)) ? $"{_prefix}" : string.Empty);
			if (buffer != null)
			{
				buffer.Length = 0;
			}
			int num = 0;
			string stackTrace = Environment.StackTrace;
			int length = stackTrace.Length;
			int num2 = 0;
			int num3 = stackTrace.IndexOf("\r\n", num2);
			while (num3 != -1)
			{
				string text = stackTrace.Substring(num2, num3 - num2);
				if (targetStackFrame == -1 || num == targetStackFrame)
				{
					int num4 = text.IndexOf(" at ");
					int startIndex = num4 + 4;
					int num5 = text.IndexOf(" in ", startIndex);
					int num6 = num5 + 4;
					int num7 = text.IndexOf(":line", num6);
					int num8 = num7 + 6;
					string value2 = text.Substring(num6, num7 - num6);
					string value3 = text.Substring(num8, text.Length - num8);
					if (buffer != null)
					{
						buffer.Append(value);
						buffer.Append(value2);
						buffer.Append(":");
						buffer.Append(value3);
						buffer.AppendLine();
					}
					if (targetStackFrame != -1)
					{
						break;
					}
				}
				num2 = num3 + 2;
				if (num2 < length)
				{
					num3 = stackTrace.IndexOf("\r\n", num2);
					if (num3 == -1)
					{
						num3 = length;
					}
				}
				else
				{
					num3 = -1;
				}
				num++;
			}
		}
		catch (Exception)
		{
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogMethod(LogNetGroup logNetGroup, params object[] arguments)
	{
		if (LogGroupActive(logNetGroup))
		{
			LogMethodInternal(MakeNetPrefix(logNetGroup), null, arguments);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogMethod(LogNetGroup logNetGroup, UnityEngine.Object context, params object[] arguments)
	{
		if (LogGroupActive(logNetGroup))
		{
			LogMethodInternal(MakeNetPrefix(logNetGroup), context, arguments);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogMethod(LogGroup logGroup, params object[] arguments)
	{
		if (LogGroupActive(logGroup))
		{
			LogMethodInternal(MakePrefix(logGroup), null, arguments);
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogMethod(LogGroup logGroup, UnityEngine.Object context, params object[] arguments)
	{
		if (LogGroupActive(logGroup))
		{
			LogMethodInternal(MakePrefix(logGroup), context, arguments);
		}
	}

	private static void LogMethodInternal(string _prefix, UnityEngine.Object context, params object[] arguments)
	{
		try
		{
			LogMethodSB.Length = 0;
			string value = ((!string.IsNullOrEmpty(_prefix)) ? $"{_prefix}" : string.Empty);
			LogMethodSB.Append(value);
		}
		catch (Exception)
		{
		}
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogErrorFormatOnceOnly(string format, params object[] args)
	{
		_LogErrorFormatOnceOnly(null, format, args);
	}

	[Conditional("THIS_DOESN_DO_ANYTHING")]
	public static void LogErrorFormatOnceOnly(UnityEngine.Object context, string format, params object[] args)
	{
		_LogErrorFormatOnceOnly(context, format, args);
	}

	private static void _LogErrorFormatOnceOnly(UnityEngine.Object context, string format, params object[] args)
	{
		HashSet<string> value = null;
		int key = ((!(context != null)) ? (-1) : context.GetInstanceID());
		if (!m_loggedErrorFormat.TryGetValue(key, out value))
		{
			value = new HashSet<string>();
			m_loggedErrorFormat.Add(key, value);
		}
		if (value != null)
		{
			LogLineFileInternal(null, null, 4, LineFileSB);
			string item = LineFileSB.ToString();
			if (!value.Contains(item))
			{
				value.Add(item);
			}
		}
	}

	internal static void ClearActiveLogGroups()
	{
		ActiveLogGroups.SetAll(value: false);
	}

	internal static void ClearActiveLogNetGroups()
	{
		ActiveLogNetGroups.SetAll(value: false);
	}

	internal static void ClearActivePrefixes()
	{
		ActivePrefixes = (Prefix)0;
	}
}
