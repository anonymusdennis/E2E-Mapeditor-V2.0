using UnityEngine;

public class SpeechBubbleHUD : MonoBehaviour
{
	public Sprite m_NormalImage;

	public T17Image m_Background;

	public T17Image m_Tail;

	public T17Text m_Text;

	public GameObject m_SpeechBubbleTail;

	public Color m_NormalTextColour;

	public Color m_NegativeTextColour;

	private Vector3 m_OriginalLocalPosition;

	private FloorManager m_FloorManager;

	private Transform m_CachedTransform;

	private const float LOCAL_SHIFT_FOR_OVERLAP = 18f;

	private float m_ZShiftForFacade;

	private float m_YShiftForFacade;

	private float m_LastTimeBubbleShiftChecked;

	private void Awake()
	{
		CacheInitialTransformValues();
	}

	protected virtual void OnDestroy()
	{
		m_FloorManager = null;
	}

	private void CacheInitialTransformValues()
	{
		m_CachedTransform = GetComponent<Transform>();
		m_OriginalLocalPosition = base.transform.localPosition;
	}

	public void SetTextColour(SpeechTone tone)
	{
		if (tone == SpeechTone.Negative)
		{
			m_Text.color = m_NegativeTextColour;
		}
		else
		{
			m_Text.color = m_NormalTextColour;
		}
	}

	public void ResetFacadeOffset()
	{
		SetFacadeOffset(isClippedLeft: false, isClippedRight: false, isSplitScreen: false);
	}

	public void SetFacadeOffset(bool isClippedLeft, bool isClippedRight, bool isSplitScreen)
	{
		bool flag = isClippedLeft || isClippedRight;
		if (m_CachedTransform == null)
		{
			CacheInitialTransformValues();
		}
		Vector3 vector = m_OriginalLocalPosition;
		if (flag)
		{
			if (isSplitScreen)
			{
				vector += new Vector3(0f, -18f, 0f);
				if (isClippedLeft && !isClippedRight)
				{
					vector.x += 18f;
				}
				if (isClippedRight && !isClippedLeft)
				{
					vector.x -= 18f;
				}
			}
			else
			{
				if (m_FloorManager == null)
				{
					m_FloorManager = FloorManager.GetInstance();
					m_ZShiftForFacade = (float)m_FloorManager.m_FloorOffset - m_FloorManager.m_HalfFloorOffset - 1f;
					m_YShiftForFacade = (m_ZShiftForFacade - (float)m_FloorManager.m_FloorOffset) / (float)m_FloorManager.m_FloorOffset * -1f;
				}
				if (m_FloorManager != null)
				{
					vector = base.transform.InverseTransformPoint(base.transform.position + new Vector3(0f, m_YShiftForFacade, m_ZShiftForFacade));
				}
			}
		}
		if (base.transform.localPosition != vector)
		{
			base.transform.localPosition = vector;
		}
	}

	public float GetLastShiftTestTimestamp()
	{
		return m_LastTimeBubbleShiftChecked;
	}

	public void RecordShiftTestDone()
	{
		m_LastTimeBubbleShiftChecked = UpdateManager.time;
	}
}
