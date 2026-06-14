using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMapKeyFilter : MonoBehaviour
{
	[Serializable]
	public class KeyElementFilterMask
	{
		public Transform KeyElement;

		public bool[] FilterMask = new bool[6];
	}

	public List<KeyElementFilterMask> KeyFilterMasks = new List<KeyElementFilterMask>();
}
