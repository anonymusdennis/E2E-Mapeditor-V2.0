using System;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotImprover : MonoBehaviour
{
	[Serializable]
	public class WhatToDo
	{
		public enum Action
		{
			Disable,
			Enable
		}

		public Action m_Action;

		public GameObject m_GameObject;

		[NonSerialized]
		public bool m_PreviousState;
	}

	public List<WhatToDo> m_Actions = new List<WhatToDo>();

	private void Start()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.RegisterSnapshotAction(OnSnapshot);
		}
	}

	private void OnDestroy()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.UnregisterSnapshotAction(OnSnapshot);
		}
	}

	public void OnSnapshot(bool bBefore)
	{
		for (int num = m_Actions.Count - 1; num >= 0; num--)
		{
			if (m_Actions[num] != null)
			{
				WhatToDo.Action action = m_Actions[num].m_Action;
				if (action == WhatToDo.Action.Disable || action == WhatToDo.Action.Enable)
				{
					if (m_Actions[num].m_GameObject == null)
					{
						break;
					}
					if (bBefore)
					{
						m_Actions[num].m_PreviousState = m_Actions[num].m_GameObject.activeSelf;
						m_Actions[num].m_GameObject.SetActive(m_Actions[num].m_Action == WhatToDo.Action.Enable);
					}
					else
					{
						m_Actions[num].m_GameObject.SetActive(m_Actions[num].m_PreviousState);
					}
				}
			}
		}
	}
}
