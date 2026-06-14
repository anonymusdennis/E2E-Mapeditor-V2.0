using System;
using System.Collections.Generic;
using Rotorz.Tile;
using UnityEngine;

namespace Custom;

public sealed class CompositeTileMap : MonoBehaviour, ISerializationCallbackReceiver
{
	[SerializeField]
	private bool _destroyOnAwake;

	[SerializeField]
	private bool _stripOnBuild;

	private TileSystem _system;

	[HideInInspector]
	[SerializeField]
	private List<int> __groupLengths = new List<int>();

	[HideInInspector]
	[SerializeField]
	private List<int> __groupTileIndices = new List<int>();

	private Dictionary<int, List<int>> _groups = new Dictionary<int, List<int>>();

	public bool DestroyOnAwake
	{
		get
		{
			return _destroyOnAwake;
		}
		set
		{
			_destroyOnAwake = value;
		}
	}

	public bool StripOnBuild
	{
		get
		{
			return _stripOnBuild;
		}
		set
		{
			_stripOnBuild = value;
		}
	}

	private TileSystem TileSystem
	{
		get
		{
			if (_system == null)
			{
				_system = GetComponent<TileSystem>();
			}
			return _system;
		}
	}

	private void Awake()
	{
		if (DestroyOnAwake)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		_groups.Clear();
		int num = 0;
		foreach (int _groupLength in __groupLengths)
		{
			List<int> list = new List<int>(_groupLength);
			for (int i = 0; i < _groupLength; i++)
			{
				int num2 = __groupTileIndices[num++];
				list.Add(num2);
				_groups[num2] = list;
			}
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		HashSet<List<int>> hashSet = new HashSet<List<int>>();
		__groupLengths.Clear();
		__groupTileIndices.Clear();
		foreach (List<int> value in _groups.Values)
		{
			if (!hashSet.Contains(value))
			{
				__groupLengths.Add(value.Count);
				__groupTileIndices.AddRange(value);
				hashSet.Add(value);
			}
		}
	}

	public int FlatIndexFromTileIndex(TileIndex index)
	{
		if (!TileSystem.InBounds(index))
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return index.row * TileSystem.ColumnCount + index.column;
	}

	public TileIndex TileIndexFromFlatIndex(int index)
	{
		int rowCount = TileSystem.RowCount;
		int columnCount = TileSystem.ColumnCount;
		if ((uint)index >= rowCount * columnCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		TileIndex result = default(TileIndex);
		result.row = index / columnCount;
		result.column = index % columnCount;
		return result;
	}

	public List<int> GetGroupedTileIndices(TileIndex index)
	{
		_groups.TryGetValue(FlatIndexFromTileIndex(index), out var value);
		return value;
	}

	public void GroupTiles(TileIndex index1, TileIndex index2)
	{
		int num = FlatIndexFromTileIndex(index1);
		int num2 = FlatIndexFromTileIndex(index2);
		_groups.TryGetValue(num, out var value);
		_groups.TryGetValue(num2, out var value2);
		if (value == null)
		{
			value = value2;
			value2 = null;
		}
		else if (value == value2)
		{
			return;
		}
		if (num == num2)
		{
			value = new List<int>();
			value.Add(num);
			_groups[num] = value;
		}
		else if (value == null)
		{
			value = new List<int>();
			value.Add(num);
			value.Add(num2);
			_groups[num] = value;
			_groups[num2] = value;
		}
		else if (value2 == null)
		{
			value.Add(num2);
			_groups[num2] = value;
		}
		else
		{
			for (int i = 0; i < value2.Count; i++)
			{
				num2 = value2[i];
				value.Add(num2);
				_groups[num2] = value;
			}
		}
	}

	public void UngroupTiles(TileIndex index)
	{
		List<int> groupedTileIndices = GetGroupedTileIndices(index);
		if (groupedTileIndices != null)
		{
			int num = groupedTileIndices.Count;
			while (--num >= 0)
			{
				int key = groupedTileIndices[num];
				_groups.Remove(key);
			}
		}
	}

	public void GroupIndividualTile(TileIndex index)
	{
		GroupTiles(index, index);
	}

	public void UngroupIndividualTile(TileIndex index)
	{
		int num = FlatIndexFromTileIndex(index);
		_groups.TryGetValue(num, out var value);
		if (value != null)
		{
			value.Remove(num);
			if (value.Count == 1)
			{
				_groups[num] = null;
			}
		}
	}

	public bool IsGrouped(TileIndex index)
	{
		return GetGroupedTileIndices(index) != null;
	}

	public void UngroupAll()
	{
		_groups.Clear();
		__groupLengths.Clear();
		__groupTileIndices.Clear();
	}
}
