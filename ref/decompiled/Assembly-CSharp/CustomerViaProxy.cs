using System.Collections.Generic;
using SaveHelpers;

public class CustomerViaProxy : BaseCustomerInformation_v1
{
	public bool m_bIsReadyForService;

	public CustomerViaProxy(ulong[] serialisedData)
		: base(serialisedData)
	{
	}

	public CustomerViaProxy(ulong[] serialisedData, ref int headIndex)
		: base(serialisedData, ref headIndex)
	{
	}

	public CustomerViaProxy(AICharacter_JobCustomer character)
		: base(character)
	{
	}

	public override List<ulong> Serialise()
	{
		List<ulong> list = base.Serialise();
		BitField bitField = new BitField();
		bitField.Set(1, m_bIsReadyForService ? 1u : 0u);
		list.Add((ulong)bitField);
		return list;
	}

	protected override void Deserialise(ulong[] jobData, ref int headIndex)
	{
		base.Deserialise(jobData, ref headIndex);
		BitField bitField = new BitField(jobData[headIndex++]);
		m_bIsReadyForService = bitField.GetBool();
	}
}
