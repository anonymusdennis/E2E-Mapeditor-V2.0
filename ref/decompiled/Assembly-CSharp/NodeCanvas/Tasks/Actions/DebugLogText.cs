using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Display a UI label on the agent's position if seconds to run is not 0 and also logs the message")]
[Category("✫ Utility")]
public class DebugLogText : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<string> log = "Hello World";

	public float labelYOffset;

	public float secondsToRun = 1f;

	public CompactStatus finishStatus = CompactStatus.Success;

	private Texture2D _tex;

	protected override string info => "Log " + log.ToString() + ((!(secondsToRun > 0f)) ? string.Empty : (" for " + secondsToRun + " sec."));

	private Texture2D tex
	{
		get
		{
			if (_tex == null)
			{
				_tex = new Texture2D(1, 1);
				_tex.SetPixel(0, 0, Color.white);
				_tex.Apply();
			}
			return _tex;
		}
	}

	protected override void OnExecute()
	{
		Debug.Log($"(<b>{base.agent.gameObject.name}</b>) {log.value}", base.agent.gameObject);
		if (secondsToRun > 0f)
		{
			MonoManager.current.onGUI += OnGUI;
		}
	}

	protected override void OnStop()
	{
		if (secondsToRun > 0f)
		{
			MonoManager.current.onGUI -= OnGUI;
		}
	}

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= secondsToRun)
		{
			EndAction(finishStatus == CompactStatus.Success);
		}
	}

	private void OnGUI()
	{
		if (!(Camera.main == null))
		{
			Vector3 vector = Camera.main.WorldToScreenPoint(base.agent.position + new Vector3(0f, labelYOffset, 0f));
			Vector2 vector2 = new GUIStyle("label").CalcSize(new GUIContent(log.value));
			Rect position = new Rect(vector.x - vector2.x / 2f, (float)Screen.height - vector.y, vector2.x + 10f, vector2.y);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			GUI.DrawTexture(position, tex);
			GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
			position.x += 4f;
			GUI.Label(position, log.value);
			GUI.color = Color.white;
		}
	}
}
