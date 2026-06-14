using System;
using System.Linq;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Serializable]
public class Statement : IStatement
{
	[SerializeField]
	private string _text = string.Empty;

	[SerializeField]
	private AudioClip _audio;

	[SerializeField]
	private string _meta = string.Empty;

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public AudioClip audio
	{
		get
		{
			return _audio;
		}
		set
		{
			_audio = value;
		}
	}

	public string meta
	{
		get
		{
			return _meta;
		}
		set
		{
			_meta = value;
		}
	}

	public Statement()
	{
	}

	public Statement(string text)
	{
		this.text = text;
	}

	public Statement(string text, AudioClip audio)
	{
		this.text = text;
		this.audio = audio;
	}

	public Statement(string text, AudioClip audio, string meta)
	{
		this.text = text;
		this.audio = audio;
		this.meta = meta;
	}

	public Statement BlackboardReplace(IBlackboard bb)
	{
		string text = this.text;
		int startIndex = 0;
		while ((startIndex = text.IndexOf('[', startIndex)) != -1)
		{
			int num = text.Substring(startIndex + 1).IndexOf(']');
			string text2 = text.Substring(startIndex + 1, num);
			string text3 = text.Substring(startIndex, num + 2);
			object obj = null;
			if (bb != null)
			{
				Variable variable = bb.GetVariable(text2, typeof(object));
				if (variable != null)
				{
					obj = variable.value;
				}
			}
			if (text2.Contains("/"))
			{
				GlobalBlackboard globalBlackboard = GlobalBlackboard.Find(text2.Split('/').First());
				if (globalBlackboard != null)
				{
					Variable variable2 = globalBlackboard.GetVariable(text2.Split('/').Last(), typeof(object));
					if (variable2 != null)
					{
						obj = variable2.value;
					}
				}
			}
			text = text.Replace(text3, (obj == null) ? text3 : obj.ToString());
			startIndex++;
		}
		return new Statement(text, audio, meta);
	}

	public override string ToString()
	{
		return text;
	}
}
