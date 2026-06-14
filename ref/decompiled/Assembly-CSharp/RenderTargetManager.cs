using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTargetManager : MonoBehaviour
{
	public delegate void RtHandler();

	private class RT
	{
		public RenderTexture m_RenderTarget;

		public int m_ID;

		public bool m_bUsed;

		public int m_Width;

		public int m_Height;

		public int m_Depth;

		public RenderTextureFormat m_Format;

		public string m_DebugName;
	}

	private static RenderTargetManager s_SharedInstance;

	private static List<RT> m_ListOfRTs = new List<RT>();

	private static int m_DebugCount = 0;

	public float m_CheckRTInterval = 5f;

	private float m_TimeUntilNextCheck;

	private static bool m_bRanInit = false;

	private bool m_bHasScheduledRenderTargetCheck;

	private static int rtOutputCount = 0;

	public static event RtHandler CheckForLostRTEvents;

	public static RenderTargetManager GetShared()
	{
		return s_SharedInstance;
	}

	private void Awake()
	{
		if (s_SharedInstance == null)
		{
			s_SharedInstance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (s_SharedInstance == this)
		{
			s_SharedInstance = null;
		}
	}

	private void Update()
	{
		if (m_bRanInit)
		{
			if (m_TimeUntilNextCheck <= 0f)
			{
				m_TimeUntilNextCheck += m_CheckRTInterval;
				CheckForLostRTs();
			}
			m_TimeUntilNextCheck -= UpdateManager.unscaledDeltaTime;
		}
	}

	public static void Init()
	{
		m_bRanInit = true;
		m_DebugCount = 0;
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 16, RenderTextureFormat.ARGB32, "A");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 16, RenderTextureFormat.ARGB32, "B");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 16, RenderTextureFormat.ARGB32, "C");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 16, RenderTextureFormat.ARGB32, "D");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 0, RenderTextureFormat.ARGB32, "E");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 0, RenderTextureFormat.R8, "F");
		CreateNewRT((int)((float)Screen.width * 1.2f), (int)((float)Screen.height * 1.2f), 0, RenderTextureFormat.R8, "G");
		CreateNewRT(1272, 451, 24, RenderTextureFormat.ARGB32, "Loading");
		for (int i = 0; i < 40; i++)
		{
			CreateNewRT(80, 96, 24, RenderTextureFormat.ARGB32, "PrisonSetup");
		}
	}

	private static RT CreateNewRT(int width, int height, int depth, RenderTextureFormat format, string debugName)
	{
		RT rT = new RT();
		rT.m_ID = 6 + m_ListOfRTs.Count;
		rT.m_RenderTarget = new RenderTexture(width, height, depth, format);
		rT.m_RenderTarget.filterMode = FilterMode.Point;
		rT.m_RenderTarget.Create();
		rT.m_Width = width;
		rT.m_Height = height;
		rT.m_Depth = depth;
		rT.m_Format = format;
		rT.m_DebugName = debugName;
		m_ListOfRTs.Add(rT);
		return rT;
	}

	public static void DebugMinorAlloc()
	{
		int ID = 0;
		RequestRenderTarget(640 + m_DebugCount, 480, 0, RenderTextureFormat.ARGB32, ref ID, "DebugMinorAlloc");
		m_DebugCount++;
		Debug.Log("   ****** RenderTaret Debugs " + m_DebugCount);
	}

	public static RenderTexture RequestRenderTarget(int width, int height, int depth, RenderTextureFormat format, ref int ID, string debugName)
	{
		int count = m_ListOfRTs.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			RT rT = m_ListOfRTs[i];
			if (!rT.m_bUsed && rT.m_Width == width && rT.m_Height == height && rT.m_Depth == depth && rT.m_Format == format)
			{
				if (!rT.m_RenderTarget.IsCreated())
				{
					CheckForLostRTs();
				}
				rT.m_bUsed = true;
				ID = rT.m_ID;
				return rT.m_RenderTarget;
			}
		}
		if (!flag)
		{
			RT rT2 = CreateNewRT(width, height, depth, format, debugName);
			rT2.m_bUsed = true;
			ID = rT2.m_ID;
			return rT2.m_RenderTarget;
		}
		return null;
	}

	public static void ReleaseRenderTarget(ref int ID)
	{
		int count = m_ListOfRTs.Count;
		if (ID > 0)
		{
			for (int i = 0; i < count; i++)
			{
				if (m_ListOfRTs[i].m_ID == ID)
				{
					m_ListOfRTs[i].m_bUsed = false;
					ID = 0;
					break;
				}
			}
		}
		else if (ID == 0)
		{
			Debug.Log(" *****  ");
		}
	}

	public static void DebugInfo()
	{
		int count = m_ListOfRTs.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (m_ListOfRTs[i].m_bUsed)
			{
				Debug.Log("  *** Used   " + m_ListOfRTs[i].m_DebugName + "\t    ID=" + m_ListOfRTs[i].m_ID + "\t size:" + m_ListOfRTs[i].m_Width + "x" + m_ListOfRTs[i].m_Height);
				num++;
			}
		}
		Debug.Log("   *** Render Target  " + num + "  used    out of " + count);
	}

	public static void CheckForLostRTs(bool force = false)
	{
		int count = m_ListOfRTs.Count;
		if (s_SharedInstance != null)
		{
			s_SharedInstance.m_TimeUntilNextCheck = s_SharedInstance.m_CheckRTInterval;
		}
		for (int i = 0; i < count; i++)
		{
			RT rT = m_ListOfRTs[i];
			if (rT != null && rT.m_RenderTarget != null && (!rT.m_RenderTarget.IsCreated() || force))
			{
				rT.m_RenderTarget.Create();
			}
		}
		rtOutputCount++;
		if (rtOutputCount >= 40)
		{
			for (int j = 0; j < count; j++)
			{
				RT rT2 = m_ListOfRTs[j];
				if (rT2 != null)
				{
				}
			}
			rtOutputCount = 0;
		}
		if (RenderTargetManager.CheckForLostRTEvents != null)
		{
			RenderTargetManager.CheckForLostRTEvents();
		}
	}

	public static void DelayedCheckForRt()
	{
		if (s_SharedInstance != null)
		{
			s_SharedInstance.StartCoroutine(s_SharedInstance.WaitSecondsThenCheckForLostRts());
		}
	}

	private IEnumerator WaitSecondsThenCheckForLostRts()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		CheckForLostRTs();
	}

	public static void CheckRtEndOfFrame(int framesToWait)
	{
		if (s_SharedInstance != null && !s_SharedInstance.m_bHasScheduledRenderTargetCheck)
		{
			s_SharedInstance.m_bHasScheduledRenderTargetCheck = true;
			s_SharedInstance.StartCoroutine(s_SharedInstance.EndOfFrameThenCheckForLostRts(framesToWait));
		}
	}

	private IEnumerator EndOfFrameThenCheckForLostRts(int framesToWait)
	{
		WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
		for (int i = 0; i < framesToWait; i++)
		{
			yield return waitForFrame;
		}
		m_bHasScheduledRenderTargetCheck = false;
		CheckForLostRTs(force: true);
	}

	public static void ForceRtRecreate()
	{
		CheckForLostRTs(force: true);
	}

	private void OnApplicationFocus(bool focus)
	{
		CheckForLostRTs();
	}
}
