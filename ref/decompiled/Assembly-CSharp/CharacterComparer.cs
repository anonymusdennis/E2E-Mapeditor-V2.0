using System.Collections.Generic;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CharacterComparer : IEqualityComparer<Character>
{
	public bool Equals(Character x, Character y)
	{
		int characterListIndex = x.GetCharacterListIndex();
		int characterListIndex2 = y.GetCharacterListIndex();
		if (characterListIndex == -1 || characterListIndex2 == -1)
		{
			return x == y;
		}
		return characterListIndex == characterListIndex2;
	}

	public int GetHashCode(Character a)
	{
		int characterListIndex = a.GetCharacterListIndex();
		if (characterListIndex == -1)
		{
			return a.GetHashCode();
		}
		return characterListIndex;
	}
}
