using System;
using UnityEngine;

namespace Slate;

[ExecuteInEditMode]
public class DirectorGUI : MonoBehaviour
{
	public delegate void SubtitlesGUIDelegate(string text, Color color);

	public delegate void TextOverlayGUIDelegate(string text, Color color, float size, TextAnchor alignment, Vector2 position);

	public delegate void TextureOverlayGUIDelegate(Texture texture, Color color, Vector2 position, Vector2 scale);

	public delegate void ScreenFadeGUIDelegate(Color color);

	public delegate void LetterboxGUIDelegate(float completion);

	public Font subtitlesFont;

	public Font overlayTextFont;

	private const float CINEBOX_SIZE = 20f;

	private const float SUBS_SIZE = 18f;

	[NonSerialized]
	private static DirectorGUI _current;

	[NonSerialized]
	private static Texture dissolver;

	[NonSerialized]
	private static float dissolveCompletion;

	[NonSerialized]
	private static float letterboxCompletion;

	[NonSerialized]
	public static Color fadeColor;

	[NonSerialized]
	private static string subsText;

	[NonSerialized]
	private static Color subsColor;

	[NonSerialized]
	private static string overlayText;

	[NonSerialized]
	private static Color overlayTextColor;

	[NonSerialized]
	private static float overlayTextSize;

	[NonSerialized]
	private static TextAnchor overlayTextAnchor;

	[NonSerialized]
	private static Vector2 overlayTextPos;

	[NonSerialized]
	private static Texture overlayTexture;

	[NonSerialized]
	private static Color overlayTextureColor;

	[NonSerialized]
	private static Vector2 overlayTextureScale;

	[NonSerialized]
	private static Vector2 overlayTexturePosition;

	public static DirectorGUI current
	{
		get
		{
			if (_current == null)
			{
				_current = UnityEngine.Object.FindObjectOfType<DirectorGUI>();
				if (_current == null && DirectorCamera.current != null)
				{
					_current = DirectorCamera.current.gameObject.GetAddComponent<DirectorGUI>();
				}
			}
			return _current;
		}
	}

	private static GUIStyle subsStyle { get; set; }

	private static GUIStyle overlayTextStyle { get; set; }

	public static event SubtitlesGUIDelegate OnSubtitlesGUI;

	public static event TextOverlayGUIDelegate OnTextOverlayGUI;

	public static event TextureOverlayGUIDelegate OnTextureOverlayGUI;

	public static event ScreenFadeGUIDelegate OnScreenFadeGUI;

	public static event LetterboxGUIDelegate OnLetterboxGUI;

	public static event Action OnGUIEnable;

	public static event Action OnGUIDisable;

	private void Awake()
	{
		if (_current != null && _current != this)
		{
			UnityEngine.Object.DestroyImmediate(this);
		}
		else
		{
			_current = this;
		}
	}

	private void OnEnable()
	{
		subsStyle = new GUIStyle();
		subsStyle.normal.textColor = Color.white;
		subsStyle.richText = true;
		subsStyle.padding = new RectOffset(10, 10, 2, 2);
		subsStyle.alignment = TextAnchor.LowerCenter;
		subsStyle.font = subtitlesFont;
		overlayTextStyle = new GUIStyle();
		overlayTextStyle.normal.textColor = Color.white;
		overlayTextStyle.richText = true;
		overlayTextStyle.font = overlayTextFont;
		if (DirectorGUI.OnGUIEnable != null)
		{
			DirectorGUI.OnGUIEnable();
		}
	}

	private void OnDisable()
	{
		UpdateDissolve(null, 0f);
		UpdateLetterbox(0f);
		UpdateFade(Color.clear);
		UpdateSubtitles(null, Color.clear);
		UpdateOverlayText(null, Color.clear, 0f, TextAnchor.UpperLeft, Vector2.zero);
		UpdateOverlayTexture(null, Color.clear, Vector2.zero, Vector2.zero);
		if (DirectorGUI.OnGUIDisable != null)
		{
			DirectorGUI.OnGUIDisable();
		}
	}

	public static void UpdateDissolve(Texture texture, float completion)
	{
		if (current != null)
		{
			dissolver = texture;
			dissolveCompletion = completion;
		}
	}

	public static void UpdateLetterbox(float completion)
	{
		if (DirectorGUI.OnLetterboxGUI != null)
		{
			DirectorGUI.OnLetterboxGUI(completion);
		}
		else if (current != null)
		{
			letterboxCompletion = completion;
		}
	}

	public static void UpdateFade(Color color)
	{
		if (DirectorGUI.OnScreenFadeGUI != null)
		{
			DirectorGUI.OnScreenFadeGUI(color);
		}
		else if (current != null)
		{
			fadeColor = color;
		}
	}

	public static void UpdateSubtitles(string text, Color color)
	{
		if (DirectorGUI.OnSubtitlesGUI != null)
		{
			DirectorGUI.OnSubtitlesGUI(text, color);
			return;
		}
		if (current != null)
		{
			subsText = text;
		}
		subsColor = color;
	}

	public static void UpdateOverlayText(string text, Color color, float size, TextAnchor anchor, Vector2 pos)
	{
		if (DirectorGUI.OnTextOverlayGUI != null)
		{
			DirectorGUI.OnTextOverlayGUI(text, color, size, anchor, pos);
		}
		else if (current != null)
		{
			overlayText = text;
			overlayTextColor = color;
			overlayTextSize = size;
			overlayTextAnchor = anchor;
			overlayTextPos = pos;
		}
	}

	public static void UpdateOverlayTexture(Texture texture, Color color, Vector2 scale, Vector2 positionOffset)
	{
		if (DirectorGUI.OnTextureOverlayGUI != null)
		{
			DirectorGUI.OnTextureOverlayGUI(texture, color, scale, positionOffset);
		}
		else if (current != null)
		{
			overlayTexture = texture;
			overlayTextureColor = color;
			overlayTextureScale = scale;
			overlayTexturePosition = positionOffset;
		}
	}

	private void DoDissolve()
	{
		Rect position = new Rect(0f, 0f, Screen.width, Screen.height);
		GUI.color = new Color(1f, 1f, 1f, 1f - dissolveCompletion);
		GUI.DrawTexture(position, dissolver);
		GUI.color = Color.white;
	}

	private void DoLetterbox()
	{
		Rect position = new Rect(0f, 0f, Screen.width, 20f);
		Rect position2 = new Rect(0f, 0f, Screen.width, 20f);
		float t = Easing.Ease(EaseType.QuadraticInOut, 0f, 1f, letterboxCompletion);
		position.y = Mathf.Lerp(-20f, 0f, t);
		position2.y = Mathf.Lerp(Screen.height, (float)Screen.height - 20f, t);
		GUI.color = new Color(0.05f, 0.05f, 0.05f, letterboxCompletion);
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		GUI.DrawTexture(position2, Texture2D.whiteTexture);
		GUI.color = Color.white;
	}

	private void DoFade()
	{
		Rect position = new Rect(0f, 0f, Screen.width, Screen.height);
		GUI.color = fadeColor;
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		GUI.color = Color.white;
	}

	private void DoSubs()
	{
		string text = $"<size={18f}><b><i>{subsText}</i></b></size>";
		Vector2 vector = subsStyle.CalcSize(new GUIContent(text));
		Rect position = new Rect(0f, 0f, vector.x, vector.y);
		position.center = new Vector2(Screen.width / 2, (float)Screen.height - vector.y / 2f - 12f);
		GUI.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.2f, subsColor.a));
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		position.center -= new Vector2(2f, -2f);
		GUI.color = new Color(0f, 0f, 0f, subsColor.a);
		GUI.Label(position, text, subsStyle);
		position.center += new Vector2(2f, -2f);
		GUI.color = subsColor;
		GUI.Label(position, text, subsStyle);
		GUI.color = Color.white;
	}

	private void DoOverlayText()
	{
		overlayTextStyle.alignment = overlayTextAnchor;
		Rect position = Rect.MinMaxRect(20f, 10f, Screen.width - 20, Screen.height - 10);
		overlayTextPos.y *= -1f;
		position.center += overlayTextPos;
		string text = $"<size={overlayTextSize}><b>{overlayText}</b></size>";
		GUI.color = new Color(0f, 0f, 0f, overlayTextColor.a);
		GUI.Label(position, text, overlayTextStyle);
		position.center += new Vector2(2f, -2f);
		GUI.color = overlayTextColor;
		GUI.Label(position, text, overlayTextStyle);
		GUI.color = Color.white;
	}

	private void DoOverlayTexture()
	{
		Rect position = new Rect(0f, 0f, (float)overlayTexture.width * overlayTextureScale.x, (float)overlayTexture.height * overlayTextureScale.y);
		position.center = new Vector2(Screen.width / 2, Screen.height / 2) + overlayTexturePosition;
		GUI.color = overlayTextureColor;
		GUI.DrawTexture(position, overlayTexture);
		GUI.color = Color.white;
	}

	private void DoRuleOfThirds()
	{
		int num = 1;
		Rect position = new Rect(Screen.width / 3, 0f, num, Screen.height);
		Rect position2 = new Rect(position.x * 2f, 0f, num, Screen.height);
		Rect position3 = new Rect(0f, Screen.height / 3, Screen.width, num);
		Rect position4 = new Rect(0f, position3.y * 2f, Screen.width, num);
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.DrawTexture(position3, Texture2D.whiteTexture);
		GUI.DrawTexture(position4, Texture2D.whiteTexture);
		GUI.DrawTexture(position, Texture2D.whiteTexture);
		GUI.DrawTexture(position2, Texture2D.whiteTexture);
		GUI.color = Color.white;
	}
}
