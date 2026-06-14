using UnityEngine;

public class FloorIndicatorHUD : MonoBehaviour
{
	public T17Text m_FloorNumberText;

	public int m_PlayerIndex = -1;

	private Player m_Player;

	private int m_PreviousFloorNum = -999;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(m_Player != null) || !(m_FloorNumberText != null))
		{
			return;
		}
		FloorManager.Floor currentFloor = m_Player.GetCurrentFloor();
		if (currentFloor != null && currentFloor.m_FloorUINumber != m_PreviousFloorNum)
		{
			int num = (m_PreviousFloorNum = currentFloor.m_FloorUINumber);
			if (num > 0)
			{
				m_FloorNumberText.text = NumberToStringCache.GetIntAsString(num, bSingleAs2: false);
			}
			else if (num == -1)
			{
				m_FloorNumberText.text = "-1";
			}
			else
			{
				m_FloorNumberText.text = string.Empty;
			}
		}
	}

	public void SetPlayer(Player player, int index)
	{
		m_Player = player;
		m_PlayerIndex = index;
	}
}
