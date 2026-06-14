using UnityEngine;

public class T17BlockKeyboardAutoFocus : MonoBehaviour
{
	public static int c_TotalBlockingKeyboardAutoFocus;

	private void OnEnable()
	{
		c_TotalBlockingKeyboardAutoFocus++;
	}

	private void OnDisable()
	{
		c_TotalBlockingKeyboardAutoFocus--;
		if (c_TotalBlockingKeyboardAutoFocus < 0)
		{
			c_TotalBlockingKeyboardAutoFocus = 0;
		}
	}

	public static bool IsAutoFocusBlocked()
	{
		return c_TotalBlockingKeyboardAutoFocus > 0;
	}
}
