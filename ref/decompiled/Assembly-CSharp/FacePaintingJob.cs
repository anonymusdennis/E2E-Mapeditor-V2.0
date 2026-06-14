using System;
using System.Collections.Generic;
using UnityEngine;

public class FacePaintingJob : ServiceItemJob
{
	[Serializable]
	public class FaceCombo
	{
		public CustomisationData.LowerFaceAccessory m_LowerFace = CustomisationData.LowerFaceAccessory.NULL;

		public CustomisationData.UpperFaceAccessory m_UpperFace = CustomisationData.UpperFaceAccessory.NULL;
	}

	[Header("FacePaintingJob")]
	public List<FaceCombo> m_PossibleFacePaints;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		for (int num = base.RoomData.m_Collectors.Count - 1; num >= 0; num--)
		{
			if (!(base.RoomData.m_Collectors[num] == null))
			{
				ServiceItemMinigameInteractiveObject component = base.RoomData.m_Collectors[num].GetComponent<ServiceItemMinigameInteractiveObject>();
				if (component != null)
				{
					component.m_MinigameCompletionHelper = m_MinigameSetup;
				}
			}
		}
	}

	protected override void SetUpCustomer(CustomerViaProxy customer)
	{
		base.SetUpCustomer(customer);
		customer.m_AiCustomer.m_Character.m_CharacterCustomisation.ClearOverrides();
	}

	protected override void OnCustomerServiced(CustomerViaProxy customer, ServiceCustomer sender, Character servicingCharacter)
	{
		base.OnCustomerServiced(customer, sender, servicingCharacter);
		if (m_PossibleFacePaints.Count > 0)
		{
			FaceCombo faceCombo = m_PossibleFacePaints[m_NetworkSycnedRandom.Next(0, m_PossibleFacePaints.Count)];
			customer.m_AiCustomer.m_Character.m_CharacterCustomisation.SetFaceOverrides(faceCombo.m_LowerFace, faceCombo.m_UpperFace);
		}
	}
}
