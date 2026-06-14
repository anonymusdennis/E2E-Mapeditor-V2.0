using System.Runtime.InteropServices;

namespace nn.swkbd;

public struct KeyboardConfig
{
	public string guideText;

	public string headerText;

	public string okText;

	public InitialCursorPos initialCursorPos;

	public InputFormMode inputFormMode;

	public KeyboardMode keyboardMode;

	public PasswordMode passwordMode;

	[MarshalAs(UnmanagedType.U1)]
	public bool isPredictionEnabled;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseBlurBackground;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseNewLine;

	[MarshalAs(UnmanagedType.U1)]
	public bool isUseUtf8;

	public int textMaxLength;

	public int textMinLength;
}
