using System;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using SaveHelpers;

public abstract class CustomerJob<CustomerClass> : BaseCustomerJob where CustomerClass : BaseCustomerInformation_v1
{
	public BehaviourTree m_CustomerBehaviour;

	private List<CustomerClass> m_Customers = new List<CustomerClass>();

	private const int NUM_BITS_HEADER = 5;

	protected abstract CustomerClass CreateCustomerFromSerialisedData(ulong[] data);

	protected abstract CustomerClass CreateCustomerFromSerialisedData(ulong[] data, ref int headIndex);

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		if (T17NetManager.IsMasterClient)
		{
			List<AICharacter_JobCustomer> list = new List<AICharacter_JobCustomer>();
			for (int num = m_Customers.Count - 1; num >= 0; num--)
			{
				list.Add(m_Customers[num].m_AiCustomer);
			}
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				DismissCustomerRPC(list[num2]);
			}
		}
	}

	protected void RegisterCustomerRPC(CustomerClass customer)
	{
		if (customer != null)
		{
			ulong[] array = customer.Serialise().ToArray();
			byte[] array2 = new byte[array.Length * 8];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
			m_NetView.RPC("RPC_ALL_RegisterCustomer", NetTargets.All, array2);
		}
	}

	[PunRPC]
	public void RPC_ALL_RegisterCustomer(byte[] customerInformation)
	{
		ulong[] array = new ulong[customerInformation.Length / 8];
		Buffer.BlockCopy(customerInformation, 0, array, 0, customerInformation.Length);
		Local_RegisterCustomer(CreateCustomerFromSerialisedData(array));
	}

	private void Local_RegisterCustomer(CustomerClass customer)
	{
		if (customer != null)
		{
			CustomerClass val = FindCustomer(customer.m_AiCustomer);
			if (val != null)
			{
				m_Customers.Remove(val);
			}
			m_Customers.Add(customer);
			SetUpCustomer(customer);
			base.RequiresSerialization = true;
		}
	}

	protected virtual void SetUpCustomer(CustomerClass customer)
	{
		customer.m_AiCustomer.ClearNonGenericJobBlackboardValues();
		customer.m_AiCustomer.SetupForJob(this, m_CustomerBehaviour);
		customer.m_AiCustomer.SetIsBeingUsed(isbeingUsed: true, m_bCustomerWantsRandomCustomisation);
	}

	protected sealed override void Local_DismissCustomer(AICharacter_JobCustomer aiCustomer)
	{
		base.Local_DismissCustomer(aiCustomer);
		CustomerClass val = FindCustomer(aiCustomer);
		if (val != null)
		{
			m_Customers.Remove(val);
			OnCustomerDismissed(val);
		}
	}

	protected virtual void OnCustomerDismissed(CustomerClass customer)
	{
		customer.m_AiCustomer.SetIsBeingUsed(isbeingUsed: false, needsCustomisationReset: false);
	}

	protected CustomerClass FindCustomer(int aiCharacterId)
	{
		return m_Customers.Find((CustomerClass x) => x.m_AiCustomer.m_NetView.viewID == aiCharacterId);
	}

	protected CustomerClass FindCustomer(AICharacter_JobCustomer character)
	{
		return m_Customers.Find((CustomerClass x) => x.m_AiCustomer == character);
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		BitField bitField = new BitField();
		int count = m_Customers.Count;
		bitField.Set(5, (uint)count);
		list.Add((ulong)bitField);
		int count2 = m_Customers.Count;
		for (int i = 0; i < count2; i++)
		{
			CustomerClass val = m_Customers[i];
			list.AddRange(val.Serialise());
		}
		return list;
	}

	public override void Deserialize(ulong[] jobData)
	{
		base.Deserialize(jobData);
		int headIndex = 1;
		DeserializeJobSpecificData(jobData, ref headIndex);
	}

	protected virtual void DeserializeJobSpecificData(ulong[] jobData, ref int headIndex)
	{
		BitField bitField = new BitField(jobData[headIndex++]);
		int uInt = (int)bitField.GetUInt(5);
		for (int i = 0; i < uInt; i++)
		{
			CustomerClass val = CreateCustomerFromSerialisedData(jobData, ref headIndex);
			m_Customers.Add(val);
			SetUpCustomer(val);
		}
	}
}
