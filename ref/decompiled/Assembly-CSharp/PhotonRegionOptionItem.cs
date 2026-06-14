using UnityEngine;

public class PhotonRegionOptionItem : BaseOptionItem
{
	private PhotonRegionOptionSelector m_Option;

	public PhotonRegionOptionItem(MonoBehaviour theUIObject, float defaultValue)
		: base(theUIObject, defaultValue)
	{
	}

	public override void Initialise()
	{
		m_Option = m_theUIObject as PhotonRegionOptionSelector;
		m_InitialValue = PhotonRegionOptionSelector.GetOptionsSettingValue(NetConnectAndJoinRoom.PhotonRegion);
		if (null != m_Option)
		{
			m_Option.SetFromRegion(NetConnectAndJoinRoom.PhotonRegion);
		}
		m_SaveKey = "Network:PhotonRegion";
		base.Initialise();
	}

	public override void OnValueChanged()
	{
		if (m_Option != null)
		{
			m_CurrentValue = m_Option.GetCurrent().m_OptionValue;
			base.OnValueChanged();
		}
	}

	protected override void SyncUIObject(bool bForce = false)
	{
		if (m_Option != null)
		{
			m_Option.SetFromOptionsSettingValue(m_CurrentValue);
			base.SyncUIObject(bForce);
		}
	}

	public override void OnApply()
	{
		base.OnApply();
		if (null != m_Option)
		{
			NetConnectAndJoinRoom.PhotonRegion = m_Option.GetCurrent().m_PhotonRegion;
			if (NetConnectAndJoinRoom.PhotonRegion != PhotonNetwork.networkingPeer.CloudRegion)
			{
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			}
		}
	}
}
