using System.Collections.Generic;

public class CS_HijackRandomNPC : CS_HijackIngameCharacter
{
	public CharacterRole m_CharacterTypeToHijack;

	public override Character GetCharacterToHijack()
	{
		Character randomCharacter = GetRandomCharacter();
		if (randomCharacter == null)
		{
			randomCharacter = GetRandomCharacter(noHidden: false);
		}
		if (randomCharacter == null)
		{
		}
		return randomCharacter;
	}

	private Character GetRandomCharacter(bool noHidden = true)
	{
		List<Character> list = Character.GetAllCharacters().FindAll(delegate(Character targetCharacter)
		{
			if (targetCharacter.m_CharacterRole == m_CharacterTypeToHijack)
			{
				if (noHidden && targetCharacter.GetIsDisabled())
				{
					return false;
				}
				if (m_CharacterTypeToHijack == CharacterRole.Guard)
				{
					AICharacter_Guard component = targetCharacter.GetComponent<AICharacter_Guard>();
					if (component != null && (int)component.m_ActiveAlertness < 6)
					{
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		});
		if (list.Count == 0)
		{
			return null;
		}
		return list[CutsceneManagerBase.GetInstance().GetRandomInt(0, list.Count)];
	}
}
