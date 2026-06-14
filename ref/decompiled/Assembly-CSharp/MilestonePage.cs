using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MilestonePage
{
	public string m_HeaderTag = string.Empty;

	public List<GameObject> m_MilestonePrefabs = new List<GameObject>();

	public List<ProgressMilestone> m_Milestones = new List<ProgressMilestone>();
}
