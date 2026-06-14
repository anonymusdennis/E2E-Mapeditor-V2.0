using System;
using UnityEngine;

namespace Slate;

[ExecuteInEditMode]
public class OnGUIEventDispatcher : MonoBehaviour
{
	public event Action onGUI;

	private void OnGUI()
	{
		if (this.onGUI != null)
		{
			this.onGUI();
		}
	}
}
