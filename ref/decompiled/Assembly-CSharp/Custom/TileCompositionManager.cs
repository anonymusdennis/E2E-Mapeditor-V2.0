using System.Collections.Generic;
using UnityEngine;

namespace Custom;

public class TileCompositionManager : ScriptableObject
{
	[SerializeField]
	[HideInInspector]
	private List<TileComposition> _compositions = new List<TileComposition>();

	public IList<TileComposition> Compositions => _compositions;
}
