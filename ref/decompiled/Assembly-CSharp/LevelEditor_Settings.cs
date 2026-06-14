using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_Settings : ScriptableObject
{
	[Header("Difficulty Settings")]
	public DifficultySettings[] m_DifficultySettings = new DifficultySettings[3]
	{
		new DifficultySettings
		{
			DifficultyLevel = LevelDetailsManager.DiffecultyLevel.Easy,
			Name = "Easy",
			InmateDeskConfig = null,
			GuardDeskConfig = null,
			MedicDeskConfig = null,
			MaintanenceDeskConfig = null,
			InmateDeskMinMax = Vector2.zero,
			GuardDeskMinMax = Vector2.zero,
			MedicDeskMinMax = Vector2.zero,
			MaintanenceDeskMinMax = Vector2.zero
		},
		new DifficultySettings
		{
			DifficultyLevel = LevelDetailsManager.DiffecultyLevel.Medium,
			Name = "Medium",
			InmateDeskConfig = null,
			GuardDeskConfig = null,
			MedicDeskConfig = null,
			MaintanenceDeskConfig = null,
			InmateDeskMinMax = Vector2.zero,
			GuardDeskMinMax = Vector2.zero,
			MedicDeskMinMax = Vector2.zero,
			MaintanenceDeskMinMax = Vector2.zero
		},
		new DifficultySettings
		{
			DifficultyLevel = LevelDetailsManager.DiffecultyLevel.Hard,
			Name = "Hard",
			InmateDeskConfig = null,
			GuardDeskConfig = null,
			MedicDeskConfig = null,
			MaintanenceDeskConfig = null,
			InmateDeskMinMax = Vector2.zero,
			GuardDeskMinMax = Vector2.zero,
			MedicDeskMinMax = Vector2.zero,
			MaintanenceDeskMinMax = Vector2.zero
		}
	};

	[Header("Inmate/Player Desk Config")]
	public List<ItemGroupSetting> m_ItemGroupSettingsList = new List<ItemGroupSetting>();

	[Header("Brush Error Settings")]
	public BrushError[] m_BrushErrors = new BrushError[14]
	{
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eInvalid,
			Description = "Text.Editor.BrushError.Invalid"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eNoClearance,
			Description = "Text.Editor.BrushError.NoClearance"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eOutOfStock,
			Description = "Text.Editor.BrushError.OutOfStock"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eInsideRequired,
			Description = "Text.Editor.BrushError.InsideRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eInsideAboveRequired,
			Description = "Text.Editor.BrushError.InsideAboveRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eInsideBelowRequired,
			Description = "Text.Editor.BrushError.InsideBelowRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eOutsideRequired,
			Description = "Text.Editor.BrushError.OutsideRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eOutsideAboveRequired,
			Description = "Text.Editor.BrushError.OutsideAboveRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eOutsideBelowRequired,
			Description = "Text.Editor.BrushError.OutsideBelowRequired"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eBlocked,
			Description = "Text.Editor.BrushError.Blocked"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eBlockedBelow,
			Description = "Text.Editor.BrushError.BlockedBelow"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eBlockedAbove,
			Description = "Text.Editor.BrushError.BlockedAbove"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eRoomBlocked,
			Description = "Text.Editor.BrushError.RoomBlocked"
		},
		new BrushError
		{
			m_BrushError = BaseLevelManager.BrushError.eOutOfBounds,
			Description = "Text.Editor.BrushError.OutOfBounds"
		}
	};
}
