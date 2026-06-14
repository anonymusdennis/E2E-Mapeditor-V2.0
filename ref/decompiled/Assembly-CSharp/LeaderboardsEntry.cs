using System;
using UnityEngine;

public class LeaderboardsEntry : MonoBehaviour
{
	public T17Text m_Username;

	public T17Text m_Level;

	public T17Text m_Days;

	public T17Text m_Score;

	public Color m_MyScoreColour = Color.yellow;

	private Platform.DisplayableRank m_Rank;

	public void SetLBInfo(Platform.DisplayableRank info)
	{
		m_Rank = info;
		if (m_Username != null)
		{
			m_Username.text = info.m_Name;
			if (info.m_bMyScore)
			{
				m_Username.color = m_MyScoreColour;
			}
		}
		if (m_Level != null)
		{
			m_Level.text = info.m_Rank.ToString();
			if (info.m_bMyScore)
			{
				m_Level.color = m_MyScoreColour;
			}
		}
		if (!(m_Score != null))
		{
			return;
		}
		double num = info.m_Score;
		TimeSpan timeSpan;
		if (num < TimeSpan.MaxValue.TotalSeconds)
		{
			try
			{
				timeSpan = TimeSpan.FromSeconds(num);
			}
			catch
			{
				timeSpan = TimeSpan.MaxValue;
			}
		}
		else
		{
			timeSpan = TimeSpan.MaxValue;
		}
		string text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		m_Score.text = text.ToString();
		if (info.m_bMyScore)
		{
			m_Score.color = m_MyScoreColour;
		}
		if (m_Days != null)
		{
			m_Days.text = (timeSpan.Days + 1).ToString();
			if (info.m_bMyScore)
			{
				m_Days.color = m_MyScoreColour;
			}
		}
	}

	public void OnSelectItem()
	{
		Platform instance = Platform.GetInstance();
		if (instance != null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (m_Rank != null && primaryGamer != null)
			{
				instance.ShowGamerCard(primaryGamer, m_Rank.m_OnlineID);
			}
		}
	}

	public void SetBlank()
	{
		m_Username.text = string.Empty;
		m_Level.text = string.Empty;
		m_Days.text = string.Empty;
		m_Score.text = string.Empty;
		m_Rank = null;
	}
}
