using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class PrisonCustomisationManager : T17MonoBehaviour, Saveable, IDeserializable
{
	private class PrisonCustomisationManSaveData_V2
	{
		public string m_OldData;

		public UnityEngine.Random.State m_Seed;
	}

	private T17NetView m_NetView;

	public static Customisation[] m_NpcCustomisations = null;

	public static UnityEngine.Random.State m_CustomisationSeed;

	private static CustomisationNetData[] m_NetNpcCustomisations;

	private static PrisonData m_PrisonForCustomisation;

	public static bool m_bNpcCustomisationsInit = false;

	public static bool m_bUGCBlockEnforced = false;

	private static SaveDataRegister m_SaveData = null;

	private static PrisonCustomisationManager m_Instance = null;

	public static readonly byte[] memCustomisation = new byte[512];

	public static PrisonCustomisationManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		PhotonPeer.RegisterType(typeof(CustomisationNetData), 67, SerializeCustomisation, DeserializeCustomisation);
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 3);
	}

	protected virtual void OnDestroy()
	{
		Cleanup();
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
			m_SaveData = null;
		}
		m_NetView = null;
	}

	public void Cleanup()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
			m_NpcCustomisations = null;
			m_NetNpcCustomisations = null;
			m_PrisonForCustomisation = null;
		}
	}

	public static Customisation GetNPCDetails(int id)
	{
		Customisation customisation = null;
		if (m_NpcCustomisations != null && id >= 0 && id < m_NpcCustomisations.Length)
		{
			customisation = m_NpcCustomisations[id];
		}
		if (customisation == null)
		{
		}
		return customisation;
	}

	public static Customisation GetRandomNPCDetails(CharacterRole desiredRole)
	{
		Customisation customisation = null;
		LevelScript instance = LevelScript.GetInstance();
		if (instance != null)
		{
			PrisonData levelSetup = instance.m_LevelSetup;
			if (levelSetup != null)
			{
				int num = 0;
				int num2 = 0;
				if (levelSetup.m_CustomisableRoles != null && levelSetup.m_CustomisableRoles.Length > 0)
				{
					for (CharacterRole characterRole = CharacterRole.Inmate; characterRole < desiredRole; characterRole++)
					{
						num += levelSetup.m_CustomisableRoles[(int)characterRole];
					}
					num2 = num + levelSetup.m_CustomisableRoles[(int)desiredRole];
				}
				int num3 = UnityEngine.Random.Range(num, num2 + 1);
				if (num3 >= 0 && num3 < m_NpcCustomisations.Length)
				{
					customisation = m_NpcCustomisations[num3];
				}
			}
		}
		if (customisation == null)
		{
		}
		return customisation;
	}

	public static void ClearCache()
	{
		m_PrisonForCustomisation = null;
		m_NetNpcCustomisations = null;
	}

	public string CreateSnapshot()
	{
		PrisonCustomisationManSaveData_V2 prisonCustomisationManSaveData_V = new PrisonCustomisationManSaveData_V2();
		prisonCustomisationManSaveData_V.m_OldData = CustomisationSerialiser.SerialiseCustomisations_ToJSON(m_NpcCustomisations);
		prisonCustomisationManSaveData_V.m_Seed = m_CustomisationSeed;
		return JsonUtility.ToJson(prisonCustomisationManSaveData_V);
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool Deserialize(string data, ref string error)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		try
		{
			PrisonCustomisationManSaveData_V2 prisonCustomisationManSaveData_V = JsonUtility.FromJson<PrisonCustomisationManSaveData_V2>(data);
			if (prisonCustomisationManSaveData_V != null)
			{
				if (prisonCustomisationManSaveData_V.m_OldData != null)
				{
					m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(prisonCustomisationManSaveData_V.m_OldData);
					if (m_NpcCustomisations != null)
					{
						m_CustomisationSeed = prisonCustomisationManSaveData_V.m_Seed;
					}
					else
					{
						m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(data);
					}
				}
				else
				{
					m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(data);
				}
			}
			else
			{
				m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(data);
			}
		}
		catch
		{
			m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations_FromJSON(data);
		}
		m_PrisonForCustomisation = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		CustomisationManager.EnforceRobinson(m_PrisonForCustomisation, m_NpcCustomisations);
		m_NetNpcCustomisations = CustomisationSerialiser.SerialiseCustomisations(m_NpcCustomisations).data;
		return true;
	}

	public void RequestState_CustomisationRPC()
	{
		if (null != m_NetView)
		{
			m_NetView.RPCQuestion("RPC_RequestState_Customisation", NetTargets.MasterClient);
		}
	}

	[PunRPC]
	public void RPC_RequestState_Customisation(int RPCID, PhotonMessageInfo info)
	{
		PrisonData currentSelectedPrisonData = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData == m_PrisonForCustomisation && m_NetNpcCustomisations != null)
		{
			SendCustomisationResponse(RPCID);
			return;
		}
		Customisation[] customisableNpcsForPrison = CustomisationManager.GetInstance().GetCustomisableNpcsForPrison(currentSelectedPrisonData, generateIfNotFound: true);
		if (customisableNpcsForPrison != null && customisableNpcsForPrison.Length > 0)
		{
			CustomisationCollectionNetData customisationCollectionNetData = CustomisationSerialiser.SerialiseCustomisations(customisableNpcsForPrison);
			m_NetNpcCustomisations = customisationCollectionNetData.data;
			m_PrisonForCustomisation = currentSelectedPrisonData;
			SendCustomisationResponse(RPCID);
		}
	}

	private void SendCustomisationResponse(int RPCID)
	{
		if (m_bNpcCustomisationsInit)
		{
			Platform.GetInstance().IsUGCRestrictedRequest(delegate(bool isRestricted, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
			{
				m_bUGCBlockEnforced = isRestricted;
				m_NetView.RPCResponse("RPC_RequestStateResponce_Yes_Customisation", RPCID, m_NetNpcCustomisations, JsonUtility.ToJson(m_CustomisationSeed), m_bUGCBlockEnforced);
			});
		}
		else
		{
			m_NetView.RPCResponse("RPC_RequestStateResponce_NotYet_Customisation", RPCID);
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_Customisation(CustomisationNetData[] data, string seed, bool ugcBlockEnforced, PhotonMessageInfo info)
	{
		m_NetNpcCustomisations = data;
		m_NpcCustomisations = CustomisationSerialiser.DeserialiseCustomisations(data);
		m_CustomisationSeed = JsonUtility.FromJson<UnityEngine.Random.State>(seed);
		m_bUGCBlockEnforced = ugcBlockEnforced;
		m_PrisonForCustomisation = GlobalStart.GetInstance().GetCurrentSelectedPrisonData();
		m_bNpcCustomisationsInit = true;
	}

	[PunRPC]
	public void RPC_RequestStateResponce_NotYet_Customisation(PhotonMessageInfo info)
	{
		RequestState_CustomisationRPC();
	}

	private static short SerializeCustomisation(StreamBuffer outStream, object customobject)
	{
		int targetOffset = 0;
		CustomisationNetData customisationNetData = (CustomisationNetData)customobject;
		lock (memCustomisation)
		{
			byte[] array = memCustomisation;
			byte[] array2 = Protocol.Serialize(customisationNetData.name);
			byte[] array3 = Protocol.Serialize(customisationNetData.safeName);
			byte[] array4 = Protocol.Serialize(customisationNetData.prefixKey);
			byte[] array5 = Protocol.Serialize(customisationNetData.appearance);
			Protocol.Serialize(array2.Length, array, ref targetOffset);
			Protocol.Serialize(array3.Length, array, ref targetOffset);
			Protocol.Serialize(array4.Length, array, ref targetOffset);
			Protocol.Serialize(array5.Length, array, ref targetOffset);
			if (array2.Length > 0)
			{
				Array.Copy(array2, 0, array, targetOffset, array2.Length);
				targetOffset += array2.Length;
			}
			if (array3.Length > 0)
			{
				Array.Copy(array3, 0, array, targetOffset, array3.Length);
				targetOffset += array3.Length;
			}
			if (array4.Length > 0)
			{
				Array.Copy(array4, 0, array, targetOffset, array4.Length);
				targetOffset += array4.Length;
			}
			if (array5.Length > 0)
			{
				Array.Copy(array5, 0, array, targetOffset, array5.Length);
				targetOffset += array5.Length;
			}
			outStream.Write(array, 0, targetOffset);
		}
		return (short)targetOffset;
	}

	private static object DeserializeCustomisation(StreamBuffer inStream, short length)
	{
		CustomisationNetData customisationNetData = new CustomisationNetData();
		lock (memCustomisation)
		{
			int offset = 0;
			int value = 0;
			int value2 = 0;
			int value3 = 0;
			int value4 = 0;
			inStream.Read(memCustomisation, 0, length);
			Protocol.Deserialize(out value, memCustomisation, ref offset);
			Protocol.Deserialize(out value2, memCustomisation, ref offset);
			Protocol.Deserialize(out value3, memCustomisation, ref offset);
			Protocol.Deserialize(out value4, memCustomisation, ref offset);
			if (value > 0)
			{
				byte[] array = new byte[value];
				Array.Copy(memCustomisation, offset, array, 0, value);
				customisationNetData.name = (string)Protocol.Deserialize(array);
				offset += value;
			}
			if (value2 > 0)
			{
				byte[] array2 = new byte[value2];
				Array.Copy(memCustomisation, offset, array2, 0, value2);
				customisationNetData.safeName = (string)Protocol.Deserialize(array2);
				offset += value2;
			}
			if (value3 > 0)
			{
				byte[] array3 = new byte[value3];
				Array.Copy(memCustomisation, offset, array3, 0, value3);
				customisationNetData.prefixKey = (string)Protocol.Deserialize(array3);
				offset += value3;
			}
			if (value4 > 0)
			{
				byte[] array4 = new byte[value4];
				Array.Copy(memCustomisation, offset, array4, 0, value4);
				customisationNetData.appearance = (long)Protocol.Deserialize(array4);
				offset += value4;
			}
		}
		return customisationNetData;
	}
}
