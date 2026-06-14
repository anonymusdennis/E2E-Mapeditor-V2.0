using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_PhotonViews : BaseComponentSetup
{
	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_7;
	}

	public override SetupReturnState Setup()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		List<GameObject> photonViewList = instance.GetPhotonViewList();
		int count = photonViewList.Count;
		int i = 100;
		for (int j = 0; j < count; j++)
		{
			if (!(photonViewList[j] != null))
			{
				continue;
			}
			PhotonView[] componentsInChildren = photonViewList[j].GetComponentsInChildren<PhotonView>(includeInactive: true);
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				if (null != componentsInChildren[num] && componentsInChildren[num].viewID == 0)
				{
					for (; PhotonNetwork.networkingPeer.GetPhotonViewInDictionary(i) != null; i++)
					{
					}
					componentsInChildren[num].viewID = i++;
				}
			}
		}
		instance.ClearPhotonViewList();
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
