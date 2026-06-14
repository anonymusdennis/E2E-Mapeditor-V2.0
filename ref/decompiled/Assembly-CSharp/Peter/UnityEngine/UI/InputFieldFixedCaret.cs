using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Peter.UnityEngine.UI;

[AddComponentMenu("UI/Input Field", 31)]
public class InputFieldFixedCaret : Selectable, IUpdateSelectedHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, ISubmitHandler, ICanvasElement, ILayoutElement, IEventSystemHandler
{
	public delegate char OnValidateInput(string text, int charIndex, char addedChar);

	[Serializable]
	public class SubmitEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class OnChangeEvent : UnityEvent<string>
	{
	}

	protected enum EditState
	{
		Continue,
		Finish
	}

	protected TouchScreenKeyboard m_Keyboard;

	private static readonly char[] kSeparators = new char[6] { ' ', '.', ',', '\t', '\r', '\n' };

	[FormerlySerializedAs("text")]
	[SerializeField]
	protected Text m_TextComponent;

	[SerializeField]
	protected string OSKTitle;

	[SerializeField]
	protected string OSKDescription;

	[SerializeField]
	protected Graphic m_Placeholder;

	[SerializeField]
	private InputField.ContentType m_ContentType;

	[SerializeField]
	[FormerlySerializedAs("inputType")]
	private InputField.InputType m_InputType;

	[SerializeField]
	[FormerlySerializedAs("asteriskChar")]
	private char m_AsteriskChar = '*';

	[SerializeField]
	[FormerlySerializedAs("keyboardType")]
	private TouchScreenKeyboardType m_KeyboardType;

	[SerializeField]
	private InputField.LineType m_LineType;

	[FormerlySerializedAs("hideMobileInput")]
	[SerializeField]
	private bool m_HideMobileInput;

	[FormerlySerializedAs("validation")]
	[SerializeField]
	private InputField.CharacterValidation m_CharacterValidation;

	[FormerlySerializedAs("characterLimit")]
	[SerializeField]
	private int m_CharacterLimit;

	[FormerlySerializedAs("onSubmit")]
	[SerializeField]
	[FormerlySerializedAs("m_OnSubmit")]
	[FormerlySerializedAs("m_EndEdit")]
	private SubmitEvent m_OnEndEdit = new SubmitEvent();

	[SerializeField]
	private SubmitEvent m_OnConfirmValue = new SubmitEvent();

	[FormerlySerializedAs("onValueChange")]
	[SerializeField]
	[FormerlySerializedAs("m_OnValueChange")]
	private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

	[FormerlySerializedAs("onValidateInput")]
	[SerializeField]
	private OnValidateInput m_OnValidateInput;

	[SerializeField]
	private Color m_CaretColor = new Color(10f / 51f, 10f / 51f, 10f / 51f, 1f);

	[SerializeField]
	private bool m_CustomCaretColor;

	[FormerlySerializedAs("selectionColor")]
	[SerializeField]
	private Color m_SelectionColor = new Color(56f / 85f, 0.80784315f, 1f, 64f / 85f);

	[SerializeField]
	[FormerlySerializedAs("mValue")]
	protected string m_Text = string.Empty;

	[Range(0f, 4f)]
	[SerializeField]
	private float m_CaretBlinkRate = 0.85f;

	[Range(1f, 5f)]
	[SerializeField]
	private int m_CaretWidth = 1;

	[SerializeField]
	private bool m_ReadOnly;

	protected int m_CaretPosition;

	protected int m_CaretSelectPosition;

	private RectTransform caretRectTrans;

	protected UIVertex[] m_CursorVerts;

	private TextGenerator m_InputTextCache;

	private CanvasRenderer m_CachedInputRenderer;

	private bool m_PreventFontCallback;

	[NonSerialized]
	protected Mesh m_Mesh;

	private bool m_AllowInput;

	private bool m_ShouldActivateNextUpdate;

	private bool m_UpdateDrag;

	private bool m_DragPositionOutOfBounds;

	private const float kHScrollSpeed = 0.05f;

	private const float kVScrollSpeed = 0.1f;

	protected bool m_CaretVisible;

	private Coroutine m_BlinkCoroutine;

	private float m_BlinkStartTime;

	protected int m_DrawStart;

	protected int m_DrawEnd;

	private Coroutine m_DragCoroutine;

	private string m_OriginalText = string.Empty;

	private bool m_WasCanceled;

	private bool m_HasDoneFocusTransition;

	private const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";

	private Event m_ProcessingEvent = new Event();

	protected virtual BaseInput input
	{
		get
		{
			if ((bool)EventSystem.current && (bool)EventSystem.current.currentInputModule)
			{
				return EventSystem.current.currentInputModule.input;
			}
			return null;
		}
	}

	private string compositionString => (!(input != null)) ? Input.compositionString : input.compositionString;

	protected Mesh mesh
	{
		get
		{
			if (m_Mesh == null)
			{
				m_Mesh = new Mesh();
			}
			return m_Mesh;
		}
	}

	protected TextGenerator cachedInputTextGenerator
	{
		get
		{
			if (m_InputTextCache == null)
			{
				m_InputTextCache = new TextGenerator();
			}
			return m_InputTextCache;
		}
	}

	public bool shouldHideMobileInput
	{
		get
		{
			switch (Application.platform)
			{
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
			case RuntimePlatform.TizenPlayer:
			case RuntimePlatform.tvOS:
				return m_HideMobileInput;
			default:
				return true;
			}
		}
		set
		{
			SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
		}
	}

	private bool shouldActivateOnSelect => Application.platform != RuntimePlatform.tvOS;

	public string text
	{
		get
		{
			return m_Text;
		}
		set
		{
			if (text == value)
			{
				return;
			}
			if (value == null)
			{
				value = string.Empty;
			}
			if (m_LineType == InputField.LineType.SingleLine)
			{
				value = value.Replace("\n", string.Empty).Replace("\t", string.Empty);
			}
			if (this.onValidateInput != null || characterValidation != 0)
			{
				m_Text = string.Empty;
				OnValidateInput onValidateInput = this.onValidateInput ?? new OnValidateInput(Validate);
				m_CaretPosition = (m_CaretSelectPosition = value.Length);
				int num = ((characterLimit <= 0) ? value.Length : Math.Min(characterLimit, value.Length));
				for (int i = 0; i < num; i++)
				{
					char c = onValidateInput(m_Text, m_Text.Length, value[i]);
					if (c != 0)
					{
						m_Text += c;
					}
				}
			}
			else
			{
				m_Text = ((characterLimit <= 0 || value.Length <= characterLimit) ? value : value.Substring(0, characterLimit));
			}
			if (m_Keyboard != null)
			{
				m_Keyboard.text = m_Text;
			}
			if (m_CaretPosition > m_Text.Length)
			{
				m_CaretPosition = (m_CaretSelectPosition = m_Text.Length);
			}
			else if (m_CaretSelectPosition > m_Text.Length)
			{
				m_CaretSelectPosition = m_Text.Length;
			}
			SendOnValueChangedAndUpdateLabel();
		}
	}

	public bool isFocused => m_AllowInput;

	public float caretBlinkRate
	{
		get
		{
			return m_CaretBlinkRate;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_CaretBlinkRate, value) && m_AllowInput)
			{
				SetCaretActive();
			}
		}
	}

	public int caretWidth
	{
		get
		{
			return m_CaretWidth;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_CaretWidth, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public Text textComponent
	{
		get
		{
			return m_TextComponent;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_TextComponent, value);
		}
	}

	public Graphic placeholder
	{
		get
		{
			return m_Placeholder;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_Placeholder, value);
		}
	}

	public Color caretColor
	{
		get
		{
			return (!customCaretColor) ? textComponent.color : m_CaretColor;
		}
		set
		{
			if (SetPropertyUtility.SetColor(ref m_CaretColor, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public bool customCaretColor
	{
		get
		{
			return m_CustomCaretColor;
		}
		set
		{
			if (m_CustomCaretColor != value)
			{
				m_CustomCaretColor = value;
				MarkGeometryAsDirty();
			}
		}
	}

	public Color selectionColor
	{
		get
		{
			return m_SelectionColor;
		}
		set
		{
			if (SetPropertyUtility.SetColor(ref m_SelectionColor, value))
			{
				MarkGeometryAsDirty();
			}
		}
	}

	public SubmitEvent onEndEdit
	{
		get
		{
			return m_OnEndEdit;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_OnEndEdit, value);
		}
	}

	public SubmitEvent onConfirmValue
	{
		get
		{
			return m_OnConfirmValue;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_OnConfirmValue, value);
		}
	}

	[Obsolete("onValueChange has been renamed to onValueChanged")]
	public OnChangeEvent onValueChange
	{
		get
		{
			return onValueChanged;
		}
		set
		{
			onValueChanged = value;
		}
	}

	public OnChangeEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_OnValueChanged, value);
		}
	}

	public OnValidateInput onValidateInput
	{
		get
		{
			return m_OnValidateInput;
		}
		set
		{
			SetPropertyUtility.SetClass(ref m_OnValidateInput, value);
		}
	}

	public int characterLimit
	{
		get
		{
			return m_CharacterLimit;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_CharacterLimit, Math.Max(0, value)))
			{
				UpdateLabel();
			}
		}
	}

	public InputField.ContentType contentType
	{
		get
		{
			return m_ContentType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_ContentType, value))
			{
				EnforceContentType();
			}
		}
	}

	public InputField.LineType lineType
	{
		get
		{
			return m_LineType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_LineType, value))
			{
				SetToCustomIfContentTypeIsNot(InputField.ContentType.Standard, InputField.ContentType.Autocorrected);
			}
		}
	}

	public InputField.InputType inputType
	{
		get
		{
			return m_InputType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_InputType, value))
			{
				SetToCustom();
			}
		}
	}

	public TouchScreenKeyboardType keyboardType
	{
		get
		{
			return m_KeyboardType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value))
			{
				SetToCustom();
			}
		}
	}

	public InputField.CharacterValidation characterValidation
	{
		get
		{
			return m_CharacterValidation;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value))
			{
				SetToCustom();
			}
		}
	}

	public bool readOnly
	{
		get
		{
			return m_ReadOnly;
		}
		set
		{
			m_ReadOnly = value;
		}
	}

	public bool multiLine => m_LineType == InputField.LineType.MultiLineNewline || lineType == InputField.LineType.MultiLineSubmit;

	public char asteriskChar
	{
		get
		{
			return m_AsteriskChar;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_AsteriskChar, value))
			{
				UpdateLabel();
			}
		}
	}

	public bool wasCanceled => m_WasCanceled;

	protected int caretPositionInternal
	{
		get
		{
			return m_CaretPosition + compositionString.Length;
		}
		set
		{
			m_CaretPosition = value;
			ClampPos(ref m_CaretPosition);
		}
	}

	protected int caretSelectPositionInternal
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			m_CaretSelectPosition = value;
			ClampPos(ref m_CaretSelectPosition);
		}
	}

	private bool hasSelection => caretPositionInternal != caretSelectPositionInternal;

	public int caretPosition
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			selectionAnchorPosition = value;
			selectionFocusPosition = value;
		}
	}

	public int selectionAnchorPosition
	{
		get
		{
			return m_CaretPosition + compositionString.Length;
		}
		set
		{
			if (compositionString.Length == 0)
			{
				m_CaretPosition = value;
				ClampPos(ref m_CaretPosition);
			}
		}
	}

	public int selectionFocusPosition
	{
		get
		{
			return m_CaretSelectPosition + compositionString.Length;
		}
		set
		{
			if (compositionString.Length == 0)
			{
				m_CaretSelectPosition = value;
				ClampPos(ref m_CaretSelectPosition);
			}
		}
	}

	private static string clipboard
	{
		get
		{
			return GUIUtility.systemCopyBuffer;
		}
		set
		{
			GUIUtility.systemCopyBuffer = value;
		}
	}

	public virtual float minWidth => 0f;

	public virtual float preferredWidth
	{
		get
		{
			if (textComponent == null)
			{
				return 0f;
			}
			TextGenerationSettings generationSettings = textComponent.GetGenerationSettings(Vector2.zero);
			return textComponent.cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, generationSettings) / textComponent.pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => 0f;

	public virtual float preferredHeight
	{
		get
		{
			if (textComponent == null)
			{
				return 0f;
			}
			TextGenerationSettings generationSettings = textComponent.GetGenerationSettings(new Vector2(textComponent.rectTransform.rect.size.x, 0f));
			return textComponent.cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, generationSettings) / textComponent.pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => 1;

	protected InputFieldFixedCaret()
	{
	}

	protected void ClampPos(ref int pos)
	{
		if (pos < 0)
		{
			pos = 0;
		}
		else if (pos > text.Length)
		{
			pos = text.Length;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_Text == null)
		{
			m_Text = string.Empty;
		}
		m_DrawStart = 0;
		m_DrawEnd = m_Text.Length;
		if (m_CachedInputRenderer != null)
		{
			m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
		}
		if (m_TextComponent != null)
		{
			m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
			m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
			UpdateLabel();
		}
	}

	protected override void OnDisable()
	{
		m_BlinkCoroutine = null;
		DeactivateInputField();
		if (m_TextComponent != null)
		{
			m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
			m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
		}
		CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
		if (m_CachedInputRenderer != null)
		{
			m_CachedInputRenderer.Clear();
		}
		if (m_Mesh != null)
		{
			global::UnityEngine.Object.DestroyImmediate(m_Mesh);
		}
		m_Mesh = null;
		base.OnDisable();
	}

	private IEnumerator CaretBlink()
	{
		m_CaretVisible = true;
		yield return null;
		while (isFocused && m_CaretBlinkRate > 0f)
		{
			float blinkPeriod = 1f / m_CaretBlinkRate;
			bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2f;
			if (m_CaretVisible != blinkState)
			{
				m_CaretVisible = blinkState;
				if (!hasSelection)
				{
					MarkGeometryAsDirty();
				}
			}
			yield return null;
		}
		m_BlinkCoroutine = null;
	}

	private void SetCaretVisible()
	{
		if (m_AllowInput)
		{
			m_CaretVisible = true;
			m_BlinkStartTime = Time.unscaledTime;
			SetCaretActive();
		}
	}

	private void SetCaretActive()
	{
		if (!m_AllowInput)
		{
			return;
		}
		if (m_CaretBlinkRate > 0f)
		{
			if (m_BlinkCoroutine == null)
			{
				m_BlinkCoroutine = StartCoroutine(CaretBlink());
			}
		}
		else
		{
			m_CaretVisible = true;
		}
	}

	protected void OnFocus()
	{
		if (SelectAllOnFocus())
		{
			SelectAll();
		}
		else if (MoveCaretToEndOnFocus())
		{
			MoveTextEnd(shift: false);
		}
	}

	protected virtual bool SelectAllOnFocus()
	{
		return true;
	}

	protected virtual bool MoveCaretToEndOnFocus()
	{
		return true;
	}

	protected void SelectAll()
	{
		caretPositionInternal = text.Length;
		caretSelectPositionInternal = 0;
	}

	public void MoveTextEnd(bool shift)
	{
		int length = text.Length;
		if (shift)
		{
			caretSelectPositionInternal = length;
		}
		else
		{
			caretPositionInternal = length;
			caretSelectPositionInternal = caretPositionInternal;
		}
		UpdateLabel();
	}

	public void MoveTextStart(bool shift)
	{
		int num = 0;
		if (shift)
		{
			caretSelectPositionInternal = num;
		}
		else
		{
			caretPositionInternal = num;
			caretSelectPositionInternal = caretPositionInternal;
		}
		UpdateLabel();
	}

	private bool InPlaceEditing()
	{
		return !TouchScreenKeyboard.isSupported;
	}

	protected virtual void LateUpdate()
	{
		if (m_ShouldActivateNextUpdate)
		{
			if (!isFocused)
			{
				ActivateInputFieldInternal();
				m_ShouldActivateNextUpdate = false;
				return;
			}
			m_ShouldActivateNextUpdate = false;
		}
		if (InPlaceEditing() || !isFocused)
		{
			return;
		}
		AssignPositioningIfNeeded();
		if (m_Keyboard == null || m_Keyboard.done)
		{
			if (m_Keyboard != null)
			{
				if (!m_ReadOnly)
				{
					this.text = m_Keyboard.text;
				}
				if (m_Keyboard.wasCanceled)
				{
					m_WasCanceled = true;
				}
			}
			OnDeselect(null);
			return;
		}
		string text = m_Keyboard.text;
		if (m_Text != text)
		{
			if (m_ReadOnly)
			{
				m_Keyboard.text = m_Text;
			}
			else
			{
				m_Text = string.Empty;
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					if (c == '\r' || c == '\u0003')
					{
						c = '\n';
					}
					if (onValidateInput != null)
					{
						c = onValidateInput(m_Text, m_Text.Length, c);
					}
					else if (characterValidation != 0)
					{
						c = Validate(m_Text, m_Text.Length, c);
					}
					if (lineType == InputField.LineType.MultiLineSubmit && c == '\n')
					{
						m_Keyboard.text = m_Text;
						OnDeselect(null);
						return;
					}
					if (c != 0)
					{
						m_Text += c;
					}
				}
				if (characterLimit > 0 && m_Text.Length > characterLimit)
				{
					m_Text = m_Text.Substring(0, characterLimit);
				}
				int num = (caretSelectPositionInternal = m_Text.Length);
				caretPositionInternal = num;
				if (m_Text != text)
				{
					m_Keyboard.text = m_Text;
				}
				SendOnValueChangedAndUpdateLabel();
			}
		}
		if (m_Keyboard.done)
		{
			if (m_Keyboard.wasCanceled)
			{
				m_WasCanceled = true;
			}
			OnDeselect(null);
		}
	}

	[Obsolete("This function is no longer used. Please use RectTransformUtility.ScreenPointToLocalPointInRectangle() instead.")]
	public Vector2 ScreenToLocal(Vector2 screen)
	{
		Canvas canvas = m_TextComponent.canvas;
		if (canvas == null)
		{
			return screen;
		}
		Vector3 vector = Vector3.zero;
		if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			vector = m_TextComponent.transform.InverseTransformPoint(screen);
		}
		else if (canvas.worldCamera != null)
		{
			Ray ray = canvas.worldCamera.ScreenPointToRay(screen);
			new Plane(m_TextComponent.transform.forward, m_TextComponent.transform.position).Raycast(ray, out var enter);
			vector = m_TextComponent.transform.InverseTransformPoint(ray.GetPoint(enter));
		}
		return new Vector2(vector.x, vector.y);
	}

	private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator)
	{
		if (!multiLine)
		{
			return 0;
		}
		float num = pos.y * m_TextComponent.pixelsPerUnit;
		float num2 = 0f;
		for (int i = 0; i < generator.lineCount; i++)
		{
			float topY = generator.lines[i].topY;
			float num3 = topY - (float)generator.lines[i].height;
			if (num > topY)
			{
				float num4 = topY - num2;
				if (num > topY - 0.5f * num4)
				{
					return i - 1;
				}
				return i;
			}
			if (num > num3)
			{
				return i;
			}
			num2 = num3;
		}
		return generator.lineCount;
	}

	protected int GetCharacterIndexFromPosition(Vector2 pos)
	{
		TextGenerator cachedTextGenerator = m_TextComponent.cachedTextGenerator;
		if (cachedTextGenerator.lineCount == 0)
		{
			return 0;
		}
		int unclampedCharacterLineFromPosition = GetUnclampedCharacterLineFromPosition(pos, cachedTextGenerator);
		if (unclampedCharacterLineFromPosition < 0)
		{
			return 0;
		}
		if (unclampedCharacterLineFromPosition >= cachedTextGenerator.lineCount)
		{
			return cachedTextGenerator.characterCountVisible;
		}
		int startCharIdx = cachedTextGenerator.lines[unclampedCharacterLineFromPosition].startCharIdx;
		int lineEndPosition = GetLineEndPosition(cachedTextGenerator, unclampedCharacterLineFromPosition);
		for (int i = startCharIdx; i < lineEndPosition && i < cachedTextGenerator.characterCountVisible; i++)
		{
			UICharInfo uICharInfo = cachedTextGenerator.characters[i];
			Vector2 vector = uICharInfo.cursorPos / m_TextComponent.pixelsPerUnit;
			float num = pos.x - vector.x;
			float num2 = vector.x + uICharInfo.charWidth / m_TextComponent.pixelsPerUnit - pos.x;
			if (num < num2)
			{
				return i;
			}
		}
		return lineEndPosition;
	}

	private bool MayDrag(PointerEventData eventData)
	{
		return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left && m_TextComponent != null && m_Keyboard == null;
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			m_UpdateDrag = true;
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
			caretSelectPositionInternal = GetCharacterIndexFromPosition(localPoint) + m_DrawStart;
			MarkGeometryAsDirty();
			m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textComponent.rectTransform, eventData.position, eventData.pressEventCamera);
			if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
			{
				m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));
			}
			eventData.Use();
		}
	}

	private IEnumerator MouseDragOutsideRect(PointerEventData eventData)
	{
		while (m_UpdateDrag && m_DragPositionOutOfBounds)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePos);
			Rect rect = textComponent.rectTransform.rect;
			if (multiLine)
			{
				if (localMousePos.y > rect.yMax)
				{
					MoveUp(shift: true, goToFirstChar: true);
				}
				else if (localMousePos.y < rect.yMin)
				{
					MoveDown(shift: true, goToLastChar: true);
				}
			}
			else if (localMousePos.x < rect.xMin)
			{
				MoveLeft(shift: true, ctrl: false);
			}
			else if (localMousePos.x > rect.xMax)
			{
				MoveRight(shift: true, ctrl: false);
			}
			UpdateLabel();
			float delay = ((!multiLine) ? 0.05f : 0.1f);
			yield return new WaitForSecondsRealtime(delay);
		}
		m_DragCoroutine = null;
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData))
		{
			m_UpdateDrag = false;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!MayDrag(eventData))
		{
			return;
		}
		EventSystem.current.SetSelectedGameObject(base.gameObject, eventData);
		bool allowInput = m_AllowInput;
		base.OnPointerDown(eventData);
		if (!InPlaceEditing() && (m_Keyboard == null || !m_Keyboard.active))
		{
			OnSelect(eventData);
			return;
		}
		if (allowInput || !SelectAllOnFocus())
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
			int num2 = (caretPositionInternal = GetCharacterIndexFromPosition(localPoint) + m_DrawStart);
			caretSelectPositionInternal = num2;
		}
		UpdateLabel();
		eventData.Use();
	}

	protected EditState KeyPressed(Event evt)
	{
		EventModifiers modifiers = evt.modifiers;
		bool flag = ((SystemInfo.operatingSystemFamily != OperatingSystemFamily.MacOSX) ? ((modifiers & EventModifiers.Control) != 0) : ((modifiers & EventModifiers.Command) != 0));
		bool flag2 = (modifiers & EventModifiers.Shift) != 0;
		bool flag3 = (modifiers & EventModifiers.Alt) != 0;
		bool flag4 = flag && !flag3 && !flag2;
		switch (evt.keyCode)
		{
		case KeyCode.Backspace:
			Backspace();
			return EditState.Continue;
		case KeyCode.Delete:
			ForwardSpace();
			return EditState.Continue;
		case KeyCode.Home:
			MoveTextStart(flag2);
			return EditState.Continue;
		case KeyCode.End:
			MoveTextEnd(flag2);
			return EditState.Continue;
		case KeyCode.A:
			if (flag4)
			{
				SelectAll();
				return EditState.Continue;
			}
			break;
		case KeyCode.C:
			if (flag4)
			{
				if (inputType != InputField.InputType.Password)
				{
					clipboard = GetSelectedString();
				}
				else
				{
					clipboard = string.Empty;
				}
				return EditState.Continue;
			}
			break;
		case KeyCode.V:
			if (flag4)
			{
				Append(clipboard);
				return EditState.Continue;
			}
			break;
		case KeyCode.X:
			if (flag4)
			{
				if (inputType != InputField.InputType.Password)
				{
					clipboard = GetSelectedString();
				}
				else
				{
					clipboard = string.Empty;
				}
				Delete();
				SendOnValueChangedAndUpdateLabel();
				return EditState.Continue;
			}
			break;
		case KeyCode.LeftArrow:
			MoveLeft(flag2, flag);
			return EditState.Continue;
		case KeyCode.RightArrow:
			MoveRight(flag2, flag);
			return EditState.Continue;
		case KeyCode.UpArrow:
			MoveUp(flag2);
			return EditState.Continue;
		case KeyCode.DownArrow:
			MoveDown(flag2);
			return EditState.Continue;
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			if (lineType != InputField.LineType.MultiLineNewline)
			{
				return EditState.Finish;
			}
			break;
		case KeyCode.Escape:
			m_WasCanceled = true;
			return EditState.Finish;
		}
		char c = evt.character;
		if (!multiLine && (c == '\t' || c == '\r' || c == '\n'))
		{
			return EditState.Continue;
		}
		if (c == '\r' || c == '\u0003')
		{
			c = '\n';
		}
		if (IsValidChar(c))
		{
			Append(c);
		}
		if (c == '\0' && compositionString.Length > 0)
		{
			UpdateLabel();
		}
		return EditState.Continue;
	}

	private bool IsValidChar(char c)
	{
		switch (c)
		{
		case '\u007f':
			return false;
		case '\t':
		case '\n':
			return true;
		default:
			return m_TextComponent.font.HasCharacter(c);
		}
	}

	public void ProcessEvent(Event e)
	{
		KeyPressed(e);
	}

	public virtual void OnUpdateSelected(BaseEventData eventData)
	{
		if (!isFocused)
		{
			return;
		}
		bool flag = false;
		while (Event.PopEvent(m_ProcessingEvent))
		{
			if (m_ProcessingEvent.rawType == EventType.KeyDown)
			{
				flag = true;
				EditState editState = KeyPressed(m_ProcessingEvent);
				if (editState == EditState.Finish)
				{
					DeactivateInputField();
					break;
				}
			}
			EventType type = m_ProcessingEvent.type;
			if (type == EventType.ValidateCommand || type == EventType.ExecuteCommand)
			{
				string commandName = m_ProcessingEvent.commandName;
				if (commandName != null && commandName == "SelectAll")
				{
					SelectAll();
					flag = true;
				}
			}
		}
		if (flag)
		{
			UpdateLabel();
		}
		eventData.Use();
	}

	private string GetSelectedString()
	{
		if (!hasSelection)
		{
			return string.Empty;
		}
		int num = caretPositionInternal;
		int num2 = caretSelectPositionInternal;
		if (num > num2)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		return text.Substring(num, num2 - num);
	}

	private int FindtNextWordBegin()
	{
		if (caretSelectPositionInternal + 1 >= text.Length)
		{
			return text.Length;
		}
		int num = text.IndexOfAny(kSeparators, caretSelectPositionInternal + 1);
		if (num == -1)
		{
			return text.Length;
		}
		return num + 1;
	}

	private void MoveRight(bool shift, bool ctrl)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
			return;
		}
		int num3 = ((!ctrl) ? (caretSelectPositionInternal + 1) : FindtNextWordBegin());
		if (shift)
		{
			caretSelectPositionInternal = num3;
			return;
		}
		num2 = (caretPositionInternal = num3);
		caretSelectPositionInternal = num2;
	}

	private int FindtPrevWordBegin()
	{
		if (caretSelectPositionInternal - 2 < 0)
		{
			return 0;
		}
		int num = text.LastIndexOfAny(kSeparators, caretSelectPositionInternal - 2);
		if (num == -1)
		{
			return 0;
		}
		return num + 1;
	}

	private void MoveLeft(bool shift, bool ctrl)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
			return;
		}
		int num3 = ((!ctrl) ? (caretSelectPositionInternal - 1) : FindtPrevWordBegin());
		if (shift)
		{
			caretSelectPositionInternal = num3;
			return;
		}
		num2 = (caretPositionInternal = num3);
		caretSelectPositionInternal = num2;
	}

	private int DetermineCharacterLine(int charPos, TextGenerator generator)
	{
		for (int i = 0; i < generator.lineCount - 1; i++)
		{
			if (generator.lines[i + 1].startCharIdx > charPos)
			{
				return i;
			}
		}
		return generator.lineCount - 1;
	}

	private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
	{
		if (originalPos >= cachedInputTextGenerator.characters.Count)
		{
			return 0;
		}
		UICharInfo uICharInfo = cachedInputTextGenerator.characters[originalPos];
		int num = DetermineCharacterLine(originalPos, cachedInputTextGenerator);
		if (num <= 0)
		{
			return (!goToFirstChar) ? originalPos : 0;
		}
		int num2 = cachedInputTextGenerator.lines[num].startCharIdx - 1;
		for (int i = cachedInputTextGenerator.lines[num - 1].startCharIdx; i < num2; i++)
		{
			if (cachedInputTextGenerator.characters[i].cursorPos.x >= uICharInfo.cursorPos.x)
			{
				return i;
			}
		}
		return num2;
	}

	private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
	{
		if (originalPos >= cachedInputTextGenerator.characterCountVisible)
		{
			return text.Length;
		}
		UICharInfo uICharInfo = cachedInputTextGenerator.characters[originalPos];
		int num = DetermineCharacterLine(originalPos, cachedInputTextGenerator);
		if (num + 1 >= cachedInputTextGenerator.lineCount)
		{
			return (!goToLastChar) ? originalPos : text.Length;
		}
		int lineEndPosition = GetLineEndPosition(cachedInputTextGenerator, num + 1);
		for (int i = cachedInputTextGenerator.lines[num + 1].startCharIdx; i < lineEndPosition; i++)
		{
			if (cachedInputTextGenerator.characters[i].cursorPos.x >= uICharInfo.cursorPos.x)
			{
				return i;
			}
		}
		return lineEndPosition;
	}

	private void MoveDown(bool shift)
	{
		MoveDown(shift, goToLastChar: true);
	}

	private void MoveDown(bool shift, bool goToLastChar)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
		}
		int num3 = ((!multiLine) ? text.Length : LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar));
		if (shift)
		{
			caretSelectPositionInternal = num3;
			return;
		}
		num2 = (caretSelectPositionInternal = num3);
		caretPositionInternal = num2;
	}

	private void MoveUp(bool shift)
	{
		MoveUp(shift, goToFirstChar: true);
	}

	private void MoveUp(bool shift, bool goToFirstChar)
	{
		int num2;
		if (hasSelection && !shift)
		{
			num2 = (caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal));
			caretPositionInternal = num2;
		}
		int num3 = (multiLine ? LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0);
		if (shift)
		{
			caretSelectPositionInternal = num3;
			return;
		}
		num2 = (caretPositionInternal = num3);
		caretSelectPositionInternal = num2;
	}

	private void Delete()
	{
		if (!m_ReadOnly && caretPositionInternal != caretSelectPositionInternal)
		{
			if (caretPositionInternal < caretSelectPositionInternal)
			{
				m_Text = text.Substring(0, caretPositionInternal) + text.Substring(caretSelectPositionInternal, text.Length - caretSelectPositionInternal);
				caretSelectPositionInternal = caretPositionInternal;
			}
			else
			{
				m_Text = text.Substring(0, caretSelectPositionInternal) + text.Substring(caretPositionInternal, text.Length - caretPositionInternal);
				caretPositionInternal = caretSelectPositionInternal;
			}
		}
	}

	private void ForwardSpace()
	{
		if (!m_ReadOnly)
		{
			if (hasSelection)
			{
				Delete();
				SendOnValueChangedAndUpdateLabel();
			}
			else if (caretPositionInternal < text.Length)
			{
				m_Text = text.Remove(caretPositionInternal, 1);
				SendOnValueChangedAndUpdateLabel();
			}
		}
	}

	private void Backspace()
	{
		if (m_ReadOnly)
		{
			return;
		}
		if (hasSelection)
		{
			Delete();
			SendOnValueChangedAndUpdateLabel();
		}
		else if (caretPositionInternal > 0)
		{
			if (caretPositionInternal > text.Length)
			{
				int num = (caretPositionInternal = text.Length);
				caretSelectPositionInternal = num;
			}
			m_Text = text.Remove(caretPositionInternal - 1, 1);
			caretSelectPositionInternal = --caretPositionInternal;
			SendOnValueChangedAndUpdateLabel();
		}
	}

	private void Insert(char c)
	{
		if (!m_ReadOnly)
		{
			string text = c.ToString();
			Delete();
			if (characterLimit <= 0 || this.text.Length < characterLimit)
			{
				m_Text = this.text.Insert(m_CaretPosition, text);
				caretSelectPositionInternal = (caretPositionInternal += text.Length);
				SendOnValueChanged();
			}
		}
	}

	private void SendOnValueChangedAndUpdateLabel()
	{
		SendOnValueChanged();
		UpdateLabel();
	}

	private void SendOnValueChanged()
	{
		if (onValueChanged != null)
		{
			onValueChanged.Invoke(text);
		}
	}

	protected void SendOnSubmit()
	{
		if (onEndEdit != null)
		{
			onEndEdit.Invoke(m_Text);
		}
	}

	protected void SendOnConfirm()
	{
		if (onConfirmValue != null)
		{
			onConfirmValue.Invoke(m_Text);
		}
	}

	protected virtual void Append(string input)
	{
		if (m_ReadOnly || !InPlaceEditing())
		{
			return;
		}
		int i = 0;
		for (int length = input.Length; i < length; i++)
		{
			char c = input[i];
			if (c >= ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\n')
			{
				Append(c);
			}
		}
	}

	protected virtual void Append(char input)
	{
		if (!m_ReadOnly && InPlaceEditing())
		{
			if (onValidateInput != null)
			{
				input = onValidateInput(text, caretPositionInternal, input);
			}
			else if (characterValidation != 0)
			{
				input = Validate(text, caretPositionInternal, input);
			}
			if (input != 0)
			{
				Insert(input);
			}
		}
	}

	protected void UpdateLabel()
	{
		if (!(m_TextComponent != null) || !(m_TextComponent.font != null) || m_PreventFontCallback)
		{
			return;
		}
		m_PreventFontCallback = true;
		string text = ((compositionString.Length <= 0) ? this.text : (this.text.Substring(0, m_CaretPosition) + compositionString + this.text.Substring(m_CaretPosition)));
		string text2 = ((inputType != InputField.InputType.Password) ? text : new string(asteriskChar, text.Length));
		bool flag = string.IsNullOrEmpty(text);
		if (m_Placeholder != null)
		{
			m_Placeholder.enabled = flag;
		}
		if (!m_AllowInput)
		{
			m_DrawStart = 0;
			m_DrawEnd = m_Text.Length;
		}
		if (!flag)
		{
			Vector2 size = m_TextComponent.rectTransform.rect.size;
			TextGenerationSettings generationSettings = m_TextComponent.GetGenerationSettings(size);
			generationSettings.generateOutOfBounds = true;
			cachedInputTextGenerator.PopulateWithErrors(text2, generationSettings, base.gameObject);
			SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);
			text2 = text2.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, text2.Length) - m_DrawStart);
			if (m_CaretBlinkRate > 0f)
			{
				SetCaretVisible();
			}
		}
		m_TextComponent.text = text2;
		MarkGeometryAsDirty();
		m_PreventFontCallback = false;
	}

	private bool IsSelectionVisible()
	{
		if (m_DrawStart > caretPositionInternal || m_DrawStart > caretSelectPositionInternal)
		{
			return false;
		}
		if (m_DrawEnd < caretPositionInternal || m_DrawEnd < caretSelectPositionInternal)
		{
			return false;
		}
		return true;
	}

	private static int GetLineStartPosition(TextGenerator gen, int line)
	{
		line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
		return gen.lines[line].startCharIdx;
	}

	private static int GetLineEndPosition(TextGenerator gen, int line)
	{
		line = Mathf.Max(line, 0);
		if (line + 1 < gen.lines.Count)
		{
			return gen.lines[line + 1].startCharIdx - 1;
		}
		return gen.characterCountVisible;
	}

	private void SetDrawRangeToContainCaretPosition(int caretPos)
	{
		if (cachedInputTextGenerator.lineCount <= 0)
		{
			return;
		}
		Vector2 size = cachedInputTextGenerator.rectExtents.size;
		if (multiLine)
		{
			IList<UILineInfo> lines = cachedInputTextGenerator.lines;
			int num = DetermineCharacterLine(caretPos, cachedInputTextGenerator);
			if (caretPos > m_DrawEnd)
			{
				m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, num);
				float num2 = lines[num].topY + (float)lines[num].height;
				int num3;
				for (num3 = num; num3 > 0; num3--)
				{
					float topY = lines[num3 - 1].topY;
					if (topY - num2 > size.y)
					{
						break;
					}
				}
				m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, num3);
				return;
			}
			if (caretPos < m_DrawStart)
			{
				m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, num);
			}
			int num4 = DetermineCharacterLine(m_DrawStart, cachedInputTextGenerator);
			int i = num4;
			float topY2 = lines[num4].topY;
			float num5 = lines[i].topY - (float)lines[i].height;
			for (; i < lines.Count - 1; i++)
			{
				num5 = lines[i + 1].topY - (float)lines[i + 1].height;
				if (topY2 - num5 > size.y)
				{
					break;
				}
			}
			m_DrawEnd = GetLineEndPosition(cachedInputTextGenerator, i);
			while (num4 > 0)
			{
				topY2 = lines[num4 - 1].topY;
				if (topY2 - num5 > size.y)
				{
					break;
				}
				num4--;
			}
			m_DrawStart = GetLineStartPosition(cachedInputTextGenerator, num4);
			return;
		}
		IList<UICharInfo> characters = cachedInputTextGenerator.characters;
		if (m_DrawEnd > cachedInputTextGenerator.characterCountVisible)
		{
			m_DrawEnd = cachedInputTextGenerator.characterCountVisible;
		}
		float num6 = 0f;
		if (caretPos > m_DrawEnd || (caretPos == m_DrawEnd && m_DrawStart > 0))
		{
			m_DrawEnd = caretPos;
			m_DrawStart = m_DrawEnd - 1;
			while (m_DrawStart >= 0 && !(num6 + characters[m_DrawStart].charWidth > size.x))
			{
				num6 += characters[m_DrawStart].charWidth;
				m_DrawStart--;
			}
			m_DrawStart++;
		}
		else
		{
			if (caretPos < m_DrawStart)
			{
				m_DrawStart = caretPos;
			}
			m_DrawEnd = m_DrawStart;
		}
		while (m_DrawEnd < cachedInputTextGenerator.characterCountVisible)
		{
			num6 += characters[m_DrawEnd].charWidth;
			if (num6 > size.x)
			{
				break;
			}
			m_DrawEnd++;
		}
	}

	public void ForceLabelUpdate()
	{
		UpdateLabel();
	}

	private void MarkGeometryAsDirty()
	{
		CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
	}

	public virtual void Rebuild(CanvasUpdate update)
	{
		if (update == CanvasUpdate.LatePreRender)
		{
			UpdateGeometry();
		}
	}

	public virtual void LayoutComplete()
	{
	}

	public virtual void GraphicUpdateComplete()
	{
	}

	private void UpdateGeometry()
	{
		if (shouldHideMobileInput)
		{
			if (m_CachedInputRenderer == null && m_TextComponent != null)
			{
				GameObject gameObject = new GameObject(base.transform.name + " Input Caret");
				gameObject.hideFlags = HideFlags.DontSave;
				gameObject.transform.SetParent(m_TextComponent.transform.parent);
				gameObject.transform.SetAsFirstSibling();
				gameObject.layer = base.gameObject.layer;
				caretRectTrans = gameObject.AddComponent<RectTransform>();
				m_CachedInputRenderer = gameObject.AddComponent<CanvasRenderer>();
				m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
				gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
				AssignPositioningIfNeeded();
			}
			if (!(m_CachedInputRenderer == null))
			{
				OnFillVBO(mesh);
				m_CachedInputRenderer.SetMesh(mesh);
			}
		}
	}

	private void AssignPositioningIfNeeded()
	{
		if (m_TextComponent != null && caretRectTrans != null && (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition || caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation || caretRectTrans.localScale != m_TextComponent.rectTransform.localScale || caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin || caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax || caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition || caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta || caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
		{
			caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
			caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
			caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
			caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
			caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
			caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
			caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
			caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
		}
	}

	private void OnFillVBO(Mesh vbo)
	{
		using VertexHelper vertexHelper = new VertexHelper();
		if (!isFocused)
		{
			vertexHelper.FillMesh(vbo);
			return;
		}
		Vector2 roundingOffset = m_TextComponent.PixelAdjustPoint(Vector2.zero);
		if (!hasSelection)
		{
			GenerateCaret(vertexHelper, roundingOffset);
		}
		else
		{
			GenerateHightlight(vertexHelper, roundingOffset);
		}
		vertexHelper.FillMesh(vbo);
	}

	private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
	{
		if (!m_CaretVisible)
		{
			return;
		}
		if (m_CursorVerts == null)
		{
			CreateCursorVerts();
		}
		float num = m_CaretWidth;
		int num2 = Mathf.Max(0, caretPositionInternal - m_DrawStart);
		TextGenerator cachedTextGenerator = m_TextComponent.cachedTextGenerator;
		if (cachedTextGenerator == null || cachedTextGenerator.lineCount == 0)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		if (num2 < cachedTextGenerator.characters.Count)
		{
			zero.x = cachedTextGenerator.characters[num2].cursorPos.x;
		}
		zero.x /= m_TextComponent.pixelsPerUnit;
		if (zero.x > m_TextComponent.rectTransform.rect.xMax)
		{
			zero.x = m_TextComponent.rectTransform.rect.xMax;
		}
		int index = DetermineCharacterLine(num2, cachedTextGenerator);
		zero.y = cachedTextGenerator.lines[index].topY / m_TextComponent.pixelsPerUnit;
		float num3 = (float)cachedTextGenerator.lines[index].height / m_TextComponent.pixelsPerUnit;
		for (int i = 0; i < m_CursorVerts.Length; i++)
		{
			m_CursorVerts[i].color = caretColor;
		}
		m_CursorVerts[0].position = new Vector3(zero.x, zero.y - num3, 0f);
		m_CursorVerts[1].position = new Vector3(zero.x + num, zero.y - num3, 0f);
		m_CursorVerts[2].position = new Vector3(zero.x + num, zero.y, 0f);
		m_CursorVerts[3].position = new Vector3(zero.x, zero.y, 0f);
		if (roundingOffset != Vector2.zero)
		{
			for (int j = 0; j < m_CursorVerts.Length; j++)
			{
				UIVertex uIVertex = m_CursorVerts[j];
				uIVertex.position.x += roundingOffset.x;
				uIVertex.position.y += roundingOffset.y;
			}
		}
		vbo.AddUIVertexQuad(m_CursorVerts);
		int num4 = Screen.height;
		int targetDisplay = m_TextComponent.canvas.targetDisplay;
		if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
		{
			num4 = Display.displays[targetDisplay].renderingHeight;
		}
		zero.y = (float)num4 - zero.y;
		input.compositionCursorPos = zero;
	}

	private void CreateCursorVerts()
	{
		m_CursorVerts = new UIVertex[4];
		for (int i = 0; i < m_CursorVerts.Length; i++)
		{
			ref UIVertex reference = ref m_CursorVerts[i];
			reference = UIVertex.simpleVert;
			m_CursorVerts[i].uv0 = Vector2.zero;
		}
	}

	private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset)
	{
		int num = Mathf.Max(0, caretPositionInternal - m_DrawStart);
		int num2 = Mathf.Max(0, caretSelectPositionInternal - m_DrawStart);
		if (num > num2)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		num2--;
		TextGenerator cachedTextGenerator = m_TextComponent.cachedTextGenerator;
		if (cachedTextGenerator.lineCount <= 0)
		{
			return;
		}
		int num4 = DetermineCharacterLine(num, cachedTextGenerator);
		int lineEndPosition = GetLineEndPosition(cachedTextGenerator, num4);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.uv0 = Vector2.zero;
		simpleVert.color = selectionColor;
		for (int i = num; i <= num2 && i < cachedTextGenerator.characterCount; i++)
		{
			if (i == lineEndPosition || i == num2)
			{
				UICharInfo uICharInfo = cachedTextGenerator.characters[num];
				UICharInfo uICharInfo2 = cachedTextGenerator.characters[i];
				Vector2 vector = new Vector2(uICharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit, cachedTextGenerator.lines[num4].topY / m_TextComponent.pixelsPerUnit);
				Vector2 vector2 = new Vector2((uICharInfo2.cursorPos.x + uICharInfo2.charWidth) / m_TextComponent.pixelsPerUnit, vector.y - (float)cachedTextGenerator.lines[num4].height / m_TextComponent.pixelsPerUnit);
				if (vector2.x > m_TextComponent.rectTransform.rect.xMax || vector2.x < m_TextComponent.rectTransform.rect.xMin)
				{
					vector2.x = m_TextComponent.rectTransform.rect.xMax;
				}
				int currentVertCount = vbo.currentVertCount;
				simpleVert.position = new Vector3(vector.x, vector2.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(simpleVert);
				simpleVert.position = new Vector3(vector2.x, vector2.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(simpleVert);
				simpleVert.position = new Vector3(vector2.x, vector.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(simpleVert);
				simpleVert.position = new Vector3(vector.x, vector.y, 0f) + (Vector3)roundingOffset;
				vbo.AddVert(simpleVert);
				vbo.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
				vbo.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
				num = i + 1;
				num4++;
				lineEndPosition = GetLineEndPosition(cachedTextGenerator, num4);
			}
		}
	}

	protected char Validate(string text, int pos, char ch)
	{
		if (characterValidation == InputField.CharacterValidation.None || !base.enabled)
		{
			return ch;
		}
		if (characterValidation == InputField.CharacterValidation.Integer || characterValidation == InputField.CharacterValidation.Decimal)
		{
			bool flag = pos == 0 && text.Length > 0 && text[0] == '-';
			bool flag2 = caretPositionInternal == 0 || caretSelectPositionInternal == 0;
			if (!flag)
			{
				if (ch >= '0' && ch <= '9')
				{
					return ch;
				}
				if (ch == '-' && (pos == 0 || flag2))
				{
					return ch;
				}
				if (ch == '.' && characterValidation == InputField.CharacterValidation.Decimal && !text.Contains("."))
				{
					return ch;
				}
			}
		}
		else if (characterValidation == InputField.CharacterValidation.Alphanumeric)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (characterValidation == InputField.CharacterValidation.Name)
		{
			char c = ((text.Length <= 0) ? ' ' : text[Mathf.Clamp(pos, 0, text.Length - 1)]);
			char c2 = ((text.Length <= 0) ? '\n' : text[Mathf.Clamp(pos + 1, 0, text.Length - 1)]);
			if (char.IsLetter(ch))
			{
				if (char.IsLower(ch) && c == ' ')
				{
					return char.ToUpper(ch);
				}
				if (char.IsUpper(ch) && c != ' ' && c != '\'')
				{
					return char.ToLower(ch);
				}
				return ch;
			}
			switch (ch)
			{
			case '\'':
				if (c != ' ' && c != '\'' && c2 != '\'' && !text.Contains("'"))
				{
					return ch;
				}
				break;
			case ' ':
				if (c != ' ' && c != '\'' && c2 != ' ' && c2 != '\'')
				{
					return ch;
				}
				break;
			}
		}
		else if (characterValidation == InputField.CharacterValidation.EmailAddress)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '@' && text.IndexOf('@') == -1)
			{
				return ch;
			}
			if ("!#$%&'*+-/=?^_`{|}~".IndexOf(ch) != -1)
			{
				return ch;
			}
			if (ch == '.')
			{
				char c3 = ((text.Length <= 0) ? ' ' : text[Mathf.Clamp(pos, 0, text.Length - 1)]);
				char c4 = ((text.Length <= 0) ? '\n' : text[Mathf.Clamp(pos + 1, 0, text.Length - 1)]);
				if (c3 != '.' && c4 != '.')
				{
					return ch;
				}
			}
		}
		return '\0';
	}

	public void ActivateInputField()
	{
		if (!(m_TextComponent == null) && !(m_TextComponent.font == null) && IsActive() && IsInteractable())
		{
			if (isFocused && m_Keyboard != null && !m_Keyboard.active)
			{
				m_Keyboard.active = true;
				m_Keyboard.text = m_Text;
			}
			m_ShouldActivateNextUpdate = true;
		}
	}

	private void ActivateInputFieldInternal()
	{
		if (EventSystem.current == null)
		{
			return;
		}
		if (EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
		if (TouchScreenKeyboard.isSupported)
		{
			if (input.touchSupported)
			{
				TouchScreenKeyboard.hideInput = shouldHideMobileInput;
			}
			m_Keyboard = ((inputType != InputField.InputType.Password) ? TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputField.InputType.AutoCorrect, multiLine) : TouchScreenKeyboard.Open(m_Text, keyboardType, autocorrection: false, multiLine, secure: true));
			MoveTextEnd(shift: false);
		}
		else
		{
			input.imeCompositionMode = IMECompositionMode.On;
			OnFocus();
		}
		m_AllowInput = true;
		m_OriginalText = text;
		m_WasCanceled = false;
		if (m_CaretBlinkRate > 0f)
		{
			SetCaretVisible();
		}
		UpdateLabel();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (shouldActivateOnSelect)
		{
			ActivateInputField();
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ActivateInputField();
		}
	}

	public void DeactivateInputField()
	{
		if (!m_AllowInput)
		{
			return;
		}
		m_HasDoneFocusTransition = false;
		m_AllowInput = false;
		if (m_Placeholder != null)
		{
			m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);
		}
		if (m_TextComponent != null && IsInteractable())
		{
			if (m_WasCanceled)
			{
				text = m_OriginalText;
			}
			if (m_Keyboard != null)
			{
				m_Keyboard.active = false;
				m_Keyboard = null;
			}
			m_CaretPosition = (m_CaretSelectPosition = 0);
			SendOnSubmit();
			input.imeCompositionMode = IMECompositionMode.Auto;
		}
		MarkGeometryAsDirty();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		DeactivateInputField();
		base.OnDeselect(eventData);
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		if (IsActive() && IsInteractable() && !isFocused)
		{
			m_ShouldActivateNextUpdate = true;
		}
	}

	private void EnforceContentType()
	{
		switch (contentType)
		{
		case InputField.ContentType.Standard:
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = InputField.CharacterValidation.None;
			break;
		case InputField.ContentType.Autocorrected:
			m_InputType = InputField.InputType.AutoCorrect;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = InputField.CharacterValidation.None;
			break;
		case InputField.ContentType.IntegerNumber:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.NumberPad;
			m_CharacterValidation = InputField.CharacterValidation.Integer;
			break;
		case InputField.ContentType.DecimalNumber:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
			m_CharacterValidation = InputField.CharacterValidation.Decimal;
			break;
		case InputField.ContentType.Alphanumeric:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
			m_CharacterValidation = InputField.CharacterValidation.Alphanumeric;
			break;
		case InputField.ContentType.Name:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = InputField.CharacterValidation.Name;
			break;
		case InputField.ContentType.EmailAddress:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Standard;
			m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
			m_CharacterValidation = InputField.CharacterValidation.EmailAddress;
			break;
		case InputField.ContentType.Password:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Password;
			m_KeyboardType = TouchScreenKeyboardType.Default;
			m_CharacterValidation = InputField.CharacterValidation.None;
			break;
		case InputField.ContentType.Pin:
			m_LineType = InputField.LineType.SingleLine;
			m_InputType = InputField.InputType.Password;
			m_KeyboardType = TouchScreenKeyboardType.NumberPad;
			m_CharacterValidation = InputField.CharacterValidation.Integer;
			break;
		}
	}

	private void SetToCustomIfContentTypeIsNot(params InputField.ContentType[] allowedContentTypes)
	{
		if (contentType == InputField.ContentType.Custom)
		{
			return;
		}
		for (int i = 0; i < allowedContentTypes.Length; i++)
		{
			if (contentType == allowedContentTypes[i])
			{
				return;
			}
		}
		contentType = InputField.ContentType.Custom;
	}

	private void SetToCustom()
	{
		if (contentType != InputField.ContentType.Custom)
		{
			contentType = InputField.ContentType.Custom;
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (m_HasDoneFocusTransition)
		{
			state = SelectionState.Highlighted;
		}
		else if (state == SelectionState.Pressed)
		{
			m_HasDoneFocusTransition = true;
		}
		base.DoStateTransition(state, instant);
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	Transform ICanvasElement.get_transform()
	{
		return base.transform;
	}

	bool ICanvasElement.IsDestroyed()
	{
		return IsDestroyed();
	}
}
