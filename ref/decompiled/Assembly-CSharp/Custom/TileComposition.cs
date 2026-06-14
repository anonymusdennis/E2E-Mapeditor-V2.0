using System;
using Rotorz.Tile;
using UnityEngine;

namespace Custom;

[Serializable]
public sealed class TileComposition
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private int _rows;

	[SerializeField]
	private int _columns;

	[SerializeField]
	private TileData[] _map;

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public int Rows => _rows;

	public int Columns => _columns;

	public TileData this[int row, int column]
	{
		get
		{
			TileData tileData = _map[row * _columns + column];
			return (tileData == null || tileData.Empty || !(tileData.brush != null)) ? null : tileData;
		}
		set
		{
			_map[row * _columns + column] = value;
		}
	}

	public TileComposition(string name, int rows, int columns)
	{
		_name = name;
		Resize(rows, columns);
	}

	public void Resize(int rows, int columns)
	{
		if (rows == _rows && columns == _columns)
		{
			Clear();
			return;
		}
		_rows = rows;
		_columns = columns;
		_map = new TileData[rows * columns];
	}

	public void Clear()
	{
		for (int i = 0; i < _map.Length; i++)
		{
			_map[i] = null;
		}
	}
}
