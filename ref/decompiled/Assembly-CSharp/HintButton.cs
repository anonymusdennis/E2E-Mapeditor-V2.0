using UnityEngine;

public class HintButton : MonoBehaviour
{
	public T17Text m_Text;

	public T17Button m_Button;

	private HintConfig.HintData m_HintData;

	private HintConfig.CraftHintData m_CraftHintData;

	private int m_HintIndex = -1;

	public HintConfig.HintData HintData
	{
		get
		{
			return m_HintData;
		}
		set
		{
			m_HintData = value;
		}
	}

	public HintConfig.CraftHintData CraftHintData
	{
		get
		{
			return m_CraftHintData;
		}
		set
		{
			m_CraftHintData = value;
		}
	}

	public int HintIndex
	{
		get
		{
			return m_HintIndex;
		}
		set
		{
			m_HintIndex = value;
		}
	}

	public bool IsCraftHint
	{
		get
		{
			if (m_HintData == null && m_CraftHintData != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsRegularHint
	{
		get
		{
			if (m_CraftHintData == null && m_HintData != null)
			{
				return true;
			}
			return false;
		}
	}
}
