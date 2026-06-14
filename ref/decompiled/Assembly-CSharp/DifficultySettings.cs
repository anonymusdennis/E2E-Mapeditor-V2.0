using System;
using UnityEngine;

[Serializable]
public struct DifficultySettings
{
	[ReadOnly]
	public LevelDetailsManager.DiffecultyLevel DifficultyLevel;

	public string Name;

	public string Description;

	public ItemContainerConfig InmateDeskConfig;

	public ItemContainerConfig GuardDeskConfig;

	public ItemContainerConfig MedicDeskConfig;

	public ItemContainerConfig MaintanenceDeskConfig;

	public Vector2 InmateDeskMinMax;

	public Vector2 GuardDeskMinMax;

	public Vector2 MedicDeskMinMax;

	public Vector2 MaintanenceDeskMinMax;
}
