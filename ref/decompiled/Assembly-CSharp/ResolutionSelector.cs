using System.Collections.Generic;
using UnityEngine;

public class ResolutionSelector : MonoBehaviour
{
	public int m_ConfirmChangeTime = 5;

	public T17Text m_UIText;

	public T17Toggle m_WindowedToggle;

	private const float kSmallestSupportedWidth = 1024f;

	private const float kSmallestSupportedHeight = 720f;

	private ResolutionOptionItem m_optionItem;

	private FastList<Resolution> m_supportedResolutionList;

	private float[] m_supportedAspectRatios;

	private Vector2 m_SmallestSupportedResolution = new Vector2(1024f, 720f);

	private int m_currentIndex;

	private FrontendMenuBehaviour m_pendingMenu;

	public void Awake()
	{
		Display.onDisplaysUpdated += UpdateSupportedResolutions;
		Display.onDisplaysUpdated += SetIndexToCurrentResolution;
	}

	protected virtual void OnDestroy()
	{
		Display.onDisplaysUpdated -= UpdateSupportedResolutions;
		Display.onDisplaysUpdated -= SetIndexToCurrentResolution;
	}

	public void Initialise(ResolutionOptionItem optionItem)
	{
		m_optionItem = optionItem;
		m_pendingMenu = null;
		UpdateSupportedResolutions();
		SetIndexToCurrentResolution();
	}

	public bool HasPendingChanges()
	{
		return m_optionItem.HasPendingChanges();
	}

	public void SetBackingOut(FrontendMenuBehaviour menu)
	{
		m_pendingMenu = menu;
	}

	public bool IsBackingOut()
	{
		return m_pendingMenu != null;
	}

	public FrontendMenuBehaviour GetPendingMenu()
	{
		return m_pendingMenu;
	}

	private static bool AspectRatioSupported(float fRatio, float[] supportedAspectRatios)
	{
		for (int i = 0; i != supportedAspectRatios.Length; i++)
		{
			if (IsRoughlyEqual(fRatio, supportedAspectRatios[i], 0.05f))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateSupportedResolutions()
	{
		m_supportedAspectRatios = GlobalStart.GetInstance().m_SupportedAspectRatios;
		if (m_supportedAspectRatios.Length == 0)
		{
			Debug.LogError("ERROR: Supported aspect ratio list is empty!");
			m_UIText.SetNonLocalizedText("Missing ratios!");
			m_supportedResolutionList = null;
		}
		else
		{
			m_supportedResolutionList = GetSupportedResolutionList(m_SmallestSupportedResolution, m_supportedAspectRatios);
		}
	}

	public static FastList<Resolution> GetSupportedResolutionList(Vector2 smallestSupportedResolution, float[] supportedAspectRatios)
	{
		Display main = Display.main;
		int systemWidth = main.systemWidth;
		int systemHeight = main.systemHeight;
		Resolution[] resolutions = Screen.resolutions;
		FastList<Resolution> fastList = new FastList<Resolution>(resolutions.Length);
		for (int i = 0; i != resolutions.Length; i++)
		{
			int width = resolutions[i].width;
			int height = resolutions[i].height;
			if ((float)width >= 1024f && (float)height >= 720f && width <= systemWidth && height <= systemHeight)
			{
				float fRatio = (float)width / (float)height;
				if (AspectRatioSupported(fRatio, supportedAspectRatios))
				{
					fastList.Add(resolutions[i]);
				}
			}
		}
		return fastList;
	}

	public void SetIndexToCurrentResolution()
	{
		if (m_supportedAspectRatios != null)
		{
			SetIndexFromResolution(Screen.width, Screen.height);
		}
	}

	public void SelectPrevious()
	{
		if (m_supportedResolutionList != null)
		{
			m_currentIndex--;
			if (m_currentIndex < 0)
			{
				m_currentIndex = m_supportedResolutionList.Count - 1;
			}
			UpdateString();
		}
	}

	public void SelectNext()
	{
		if (m_supportedResolutionList != null)
		{
			m_currentIndex++;
			if (m_currentIndex >= m_supportedResolutionList.Count)
			{
				m_currentIndex = 0;
			}
			UpdateString();
		}
	}

	public void ResetToDefault()
	{
		SetIndexFromResolution(Display.main.systemWidth, Display.main.systemHeight);
	}

	public Resolution GetCurrentSelected()
	{
		if (m_supportedResolutionList == null)
		{
			return default(Resolution);
		}
		return m_supportedResolutionList[m_currentIndex];
	}

	private void SetIndexFromResolution(int width, int height)
	{
		if (m_supportedResolutionList == null)
		{
			return;
		}
		m_currentIndex = -1;
		for (int i = 0; i != m_supportedResolutionList.Count; i++)
		{
			if (m_supportedResolutionList[i].width == width && m_supportedResolutionList[i].height == height)
			{
				m_currentIndex = i;
				break;
			}
		}
		if (m_currentIndex == -1)
		{
			m_currentIndex = m_supportedResolutionList.Count - 1;
		}
		UpdateString();
	}

	private void UpdateString()
	{
		if (m_supportedResolutionList != null && !(m_UIText == null))
		{
			m_UIText.SetNonLocalizedText(m_supportedResolutionList[m_currentIndex].width + "x" + m_supportedResolutionList[m_currentIndex].height);
		}
	}

	private static bool IsRoughlyEqual(float a, float b, float fThreshold)
	{
		return ((!(a < b)) ? (a - b) : (b - a)) <= fThreshold;
	}
}
