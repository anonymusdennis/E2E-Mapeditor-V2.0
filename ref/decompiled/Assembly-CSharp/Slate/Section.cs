using System;
using UnityEngine;

namespace Slate;

[Serializable]
public class Section
{
	public static readonly Color DEFAULT_COLOR = new Color(0f, 0f, 0f, 0.4f);

	[SerializeField]
	private string _UID;

	[SerializeField]
	private string _name;

	[SerializeField]
	private float _time;

	[SerializeField]
	private Color _color = DEFAULT_COLOR;

	[SerializeField]
	private bool _colorizeBackground;

	public string UID
	{
		get
		{
			return _UID;
		}
		private set
		{
			_UID = value;
		}
	}

	public string name
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

	public float time
	{
		get
		{
			return _time;
		}
		set
		{
			_time = value;
		}
	}

	public Color color
	{
		get
		{
			return (!(_color.a > 0.1f)) ? DEFAULT_COLOR : _color;
		}
		set
		{
			_color = value;
		}
	}

	public bool colorizeBackground
	{
		get
		{
			return _colorizeBackground;
		}
		set
		{
			_colorizeBackground = value;
		}
	}

	public Section(string name, float time)
	{
		this.name = name;
		this.time = time;
		UID = Guid.NewGuid().ToString();
	}

	public override string ToString()
	{
		return name;
	}
}
