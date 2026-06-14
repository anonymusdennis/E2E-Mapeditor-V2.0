using System.Collections.Generic;
using UnityEngine;

public class CustomisationGeneratorTools
{
	public static List<Character> FindAllCharacters()
	{
		List<Character> list = new List<Character>();
		Character[] array = Resources.FindObjectsOfTypeAll<Character>();
		foreach (Character character in array)
		{
			if (!(character == null) && !(character.m_CharacterCustomisation == null) && !string.IsNullOrEmpty(character.gameObject.scene.name) && !(character.m_CharacterStats == null) && !character.m_CharacterStats.m_bIsPlayer && (!Application.isPlaying || !(character.gameObject.scene.name == LevelDetailsManager.GetInstance().GetBlockSceneName())))
			{
				list.Add(character);
			}
		}
		list.Sort(CompareCharacters);
		return list;
	}

	public static int CompareCharacters(Character x, Character y)
	{
		int num = x.m_CharacterRole.CompareTo(y.m_CharacterRole);
		if (num == 0 && x.m_CharacterRole == CharacterRole.Guard && y.m_CharacterRole == CharacterRole.Guard)
		{
			AICharacter component = x.GetComponent<AICharacter>();
			AICharacter component2 = y.GetComponent<AICharacter>();
			if (component != null && component2 != null)
			{
				num = component.m_ActiveAlertness.CompareTo(component2.m_ActiveAlertness);
			}
		}
		if (num == 0 && x.transform.parent == y.transform.parent)
		{
			num = x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex());
		}
		return num;
	}

	public static bool AssignCharacterIdentifiers(List<Character> characters)
	{
		characters.Sort(CompareCharacters);
		int num = 0;
		for (int i = 0; i < characters.Count; i++)
		{
			Character character = characters[i];
			int num2 = num;
			if (character.m_CharacterCustomisation.m_Mode != CharacterCustomisation.Mode.Blueprint)
			{
				num2 = -1;
			}
			if (character.m_CharacterCustomisation.m_BlueprintIdentifier != num2)
			{
				character.m_CharacterCustomisation.m_BlueprintIdentifier = num2;
			}
			if (num2 >= 0)
			{
				num++;
			}
		}
		return true;
	}

	public static void UpdateCharacterOutfits(List<Character> characters, PrisonData prisonData)
	{
		int num = 0;
		for (int i = 0; i < characters.Count; i++)
		{
			if (characters[i].m_CharacterCustomisation.m_BlueprintIdentifier > num)
			{
				num = characters[i].m_CharacterCustomisation.m_BlueprintIdentifier;
			}
		}
		prisonData.m_RoleStartingOutfitData = new ItemData[num + 1];
		for (int j = 0; j < characters.Count; j++)
		{
			if (!(characters[j].m_CharacterCustomisation != null) || characters[j].m_CharacterCustomisation.m_BlueprintIdentifier < 0 || characters[j].m_CharacterCustomisation.m_Mode != CharacterCustomisation.Mode.Blueprint)
			{
				continue;
			}
			int blueprintIdentifier = characters[j].m_CharacterCustomisation.m_BlueprintIdentifier;
			ItemContainer itemContainer = ((!(characters[j].m_ItemContainer != null)) ? characters[j].GetComponent<ItemContainer>() : characters[j].m_ItemContainer);
			if (!(itemContainer != null))
			{
				continue;
			}
			for (int k = 0; k < itemContainer.m_StartingItems.Count; k++)
			{
				if (itemContainer.m_StartingItems[k] != null && itemContainer.m_StartingItems[k].IsOutfit())
				{
					prisonData.m_RoleStartingOutfitData[blueprintIdentifier] = itemContainer.m_StartingItems[k];
					break;
				}
			}
		}
	}
}
