using System.Collections.Generic;
using SaveHelpers;

public abstract class BaseCustomerInformation_v1 : PrisonSnapshotIO.SnapshotData_Base
{
	public AICharacter_JobCustomer m_AiCustomer;

	public BaseCustomerInformation_v1(ulong[] serialisedData)
	{
		m_Version = 1;
		StartDeserialise(serialisedData);
	}

	public BaseCustomerInformation_v1(ulong[] serialisedData, ref int headIndex)
	{
		m_Version = 1;
		Deserialise(serialisedData, ref headIndex);
	}

	public BaseCustomerInformation_v1(AICharacter_JobCustomer character)
	{
		m_Version = 1;
		m_AiCustomer = character;
	}

	public virtual List<ulong> Serialise()
	{
		List<ulong> list = new List<ulong>();
		BitField bitField = new BitField();
		bitField.Set(12, (uint)m_AiCustomer.m_NetView.viewID);
		list.Add((ulong)bitField);
		return list;
	}

	public void StartDeserialise(ulong[] jobData)
	{
		int headIndex = 0;
		Deserialise(jobData, ref headIndex);
	}

	protected virtual void Deserialise(ulong[] jobData, ref int headIndex)
	{
		BitField bitField = new BitField(jobData[headIndex++]);
		int uInt = (int)bitField.GetUInt(12);
		m_AiCustomer = T17NetView.Find<AICharacter_JobCustomer>(uInt);
	}
}
