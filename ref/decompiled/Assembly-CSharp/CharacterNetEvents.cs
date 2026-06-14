using System.Collections.Generic;
using UnityEngine;

public class CharacterNetEvents : MonoBehaviour
{
	private enum CharacterNetEventType : byte
	{
		SendDamage,
		SetAttacking,
		DamageSelf,
		SetLooting
	}

	public static void SendDamageSelfEvent(Character self, float damage)
	{
		if (self != null && self.m_NetView != null)
		{
			object[] payload = new object[3]
			{
				(byte)2,
				(short)self.GetCharacterSerializerIndex(),
				damage
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1];
			raiseEventOptions.SequenceChannel = 0;
			if (self.m_NetView.ownerId == 0)
			{
				raiseEventOptions.TargetActors[0] = T17NetManager.MasterClientID;
			}
			else
			{
				raiseEventOptions.TargetActors[0] = self.m_NetView.ownerId;
			}
			SendEvent(raiseEventOptions, payload);
		}
	}

	private static void DamageSelfEventReceived(object[] parameters)
	{
		if (parameters == null || parameters.Length != 3)
		{
			return;
		}
		short num = (short)parameters[1];
		FastList<Character> sortedCharacterList = CharacterSerializer.GetInstance().GetSortedCharacterList();
		if (sortedCharacterList != null && num >= 0 && num < sortedCharacterList.Count)
		{
			Character character = sortedCharacterList[num];
			if (character != null)
			{
				character.DamageSelfEvent((float)parameters[2]);
			}
		}
	}

	public static void SendDamageCharacterEvent(Character target, Character attacker, float damage, short itemViewID, bool bNormalDamage)
	{
		if (!(target != null) || !(attacker != null) || !(target.m_NetView != null))
		{
			return;
		}
		object[] array = new object[6]
		{
			(byte)0,
			(short)target.GetCharacterSerializerIndex(),
			(short)attacker.GetCharacterSerializerIndex(),
			damage,
			itemViewID,
			bNormalDamage
		};
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		raiseEventOptions.SequenceChannel = 0;
		if (target.m_NetView.isMine)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				PhotonNetwork.OnEventCall(20, array, PhotonNetwork.player.ID);
			}
			return;
		}
		raiseEventOptions.TargetActors = new int[1];
		if (target.m_NetView.ownerId == 0)
		{
			raiseEventOptions.TargetActors[0] = T17NetManager.MasterClientID;
		}
		else
		{
			raiseEventOptions.TargetActors[0] = target.m_NetView.ownerId;
		}
		SendEvent(raiseEventOptions, array);
	}

	private static void SendDamageEventReceived(object[] parameters)
	{
		if (parameters.Length != 6)
		{
			return;
		}
		short num = (short)parameters[1];
		short num2 = (short)parameters[2];
		FastList<Character> sortedCharacterList = CharacterSerializer.GetInstance().GetSortedCharacterList();
		if (sortedCharacterList == null || num < 0 || num >= sortedCharacterList.Count)
		{
			return;
		}
		Character character = sortedCharacterList[num];
		if (character != null)
		{
			Character character2 = null;
			if (num2 >= 0 && num2 < sortedCharacterList.Count)
			{
				character2 = sortedCharacterList[num2];
			}
			character2.DamageCharacterEvent(character, (float)parameters[3], (short)parameters[4], (bool)parameters[5], Character.GamelogicRunModes.NonAudioOnly);
		}
	}

	public static void SendSetAttackingEvent(Character attackingCharacter)
	{
		if (attackingCharacter != null)
		{
			object[] payload = new object[2]
			{
				(byte)1,
				(short)attackingCharacter.GetCharacterSerializerIndex()
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1];
			raiseEventOptions.TargetActors[0] = T17NetManager.MasterClientID;
			raiseEventOptions.SequenceChannel = 0;
			SendEvent(raiseEventOptions, payload);
		}
	}

	private static void SetAttackingEventReceived(object[] parameters)
	{
		if (parameters == null || parameters.Length != 2)
		{
			return;
		}
		short num = (short)parameters[1];
		FastList<Character> sortedCharacterList = CharacterSerializer.GetInstance().GetSortedCharacterList();
		if (sortedCharacterList != null && num >= 0 && num < sortedCharacterList.Count)
		{
			Character character = sortedCharacterList[num];
			if (character != null)
			{
				character.SetIsAttacking(attacking: true);
			}
		}
	}

	public static void SendSetLootingEvent(Character lootingCharacter)
	{
		if (lootingCharacter != null)
		{
			object[] payload = new object[2]
			{
				(byte)3,
				(short)lootingCharacter.GetCharacterSerializerIndex()
			};
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1];
			raiseEventOptions.TargetActors[0] = T17NetManager.MasterClientID;
			raiseEventOptions.SequenceChannel = 0;
			SendEvent(raiseEventOptions, payload);
		}
	}

	private static void SetLootingEventReceived(object[] parameters)
	{
		if (parameters == null || parameters.Length != 2)
		{
			return;
		}
		short num = (short)parameters[1];
		FastList<Character> sortedCharacterList = CharacterSerializer.GetInstance().GetSortedCharacterList();
		if (sortedCharacterList != null && num >= 0 && num < sortedCharacterList.Count)
		{
			Character character = sortedCharacterList[num];
			if (character != null)
			{
				character.SetIsLooting(value: true);
			}
		}
	}

	private static void SendEvent(RaiseEventOptions options, object payload)
	{
		options.Encrypt = true;
		if (PhotonNetwork.offlineMode)
		{
			if (PhotonNetwork.OnEventCall != null)
			{
				PhotonNetwork.OnEventCall(20, payload, PhotonNetwork.player.ID);
			}
		}
		else
		{
			PhotonNetwork.RaiseEvent(20, payload, sendReliable: true, options);
		}
	}

	public static void OnEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal == T17NetConfig.NetEventTypes.CombatEvent && payload != null)
		{
			object[] array = (object[])payload;
			switch ((CharacterNetEventType)array[0])
			{
			case CharacterNetEventType.SendDamage:
				SendDamageEventReceived(array);
				break;
			case CharacterNetEventType.SetAttacking:
				SetAttackingEventReceived(array);
				break;
			case CharacterNetEventType.DamageSelf:
				DamageSelfEventReceived(array);
				break;
			case CharacterNetEventType.SetLooting:
				SetLootingEventReceived(array);
				break;
			}
		}
	}
}
