using System.IO;
using UnityEngine;

public class PlaylistDataCarousel : UICarousel<PlaylistData>
{
	public T17Image m_PreviewImage;

	public T17Text m_PrisonNameLabel;

	public T17Text m_PrisonDescriptionLabel;

	private int m_DifficultyStampsCount = 3;

	public GameObject[] m_DifficultyStamps = new GameObject[3];

	protected override void Awake()
	{
		base.Awake();
		if (m_PreviewImage == null || m_PrisonNameLabel == null || m_PrisonDescriptionLabel == null)
		{
		}
		for (int i = 0; i < m_DifficultyStampsCount; i++)
		{
			if (m_DifficultyStamps[i] != null)
			{
				m_DifficultyStamps[i].SetActive(value: false);
			}
		}
	}

	protected override void OnDestroy()
	{
		m_PreviewImage.sprite = null;
		base.OnDestroy();
	}

	protected override void UpdateUIForSelectedIndex(int index)
	{
		PlaylistData playlistData = m_Options[index];
		string path = playlistData.m_ImagePath;
		if (!KeyAwardManager.AreAllPrisonsUnlocked && playlistData.m_UnlockMilestone != null && !string.IsNullOrEmpty(playlistData.m_ImageLockedPath))
		{
			ProgressManager instance = ProgressManager.GetInstance();
			if (instance != null && !instance.GetMilestoneAchieved(playlistData.m_UnlockMilestone.id))
			{
				path = playlistData.m_ImageLockedPath;
			}
		}
		Sprite sprite = null;
		bool flag = false;
		if (playlistData.m_Prisons.Count > 0 && playlistData.m_Prisons[0] != null)
		{
			if (playlistData.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
			{
				flag = true;
			}
			else if (m_DifficultyStamps != null)
			{
				for (int i = 0; i < m_DifficultyStampsCount; i++)
				{
					if (m_DifficultyStamps[i] != null)
					{
						m_DifficultyStamps[i].SetActive(playlistData.m_Prisons[0].m_PrisonData.m_PrisonDifficulty == (PrisonData.PrisonDifficulty)i);
					}
				}
			}
		}
		if (flag)
		{
			if (File.Exists(path))
			{
				Texture2D texture2D = new Texture2D(2, 2);
				byte[] data = File.ReadAllBytes(path);
				texture2D.LoadImage(data);
				sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			}
		}
		else
		{
			sprite = Resources.Load<Sprite>(path);
		}
		if (sprite != null && m_PreviewImage != null)
		{
			m_PreviewImage.sprite = sprite;
		}
		if (m_PrisonNameLabel != null)
		{
			m_PrisonNameLabel.text = playlistData.m_NameLocalisationKey;
			m_PrisonNameLabel.SetNewPlaceHolder(playlistData.m_NameLocalisationKey);
			m_PrisonNameLabel.SetNewLocalizationTag(playlistData.m_NameLocalisationKey);
		}
		if (m_PrisonDescriptionLabel != null)
		{
			m_PrisonDescriptionLabel.text = playlistData.m_DescriptionLocalisationKey;
			m_PrisonNameLabel.SetNewPlaceHolder(playlistData.m_DescriptionLocalisationKey);
			m_PrisonDescriptionLabel.SetNewLocalizationTag(playlistData.m_DescriptionLocalisationKey);
		}
	}
}
