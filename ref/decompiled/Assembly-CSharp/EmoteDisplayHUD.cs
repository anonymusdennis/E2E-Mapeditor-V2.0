using UnityEngine;

public class EmoteDisplayHUD : BaseMenuBehaviour
{
	[Header("UI References")]
	public T17Text m_Text;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_Text != null)
		{
			m_Text.SetGamerForEventSystem(currentGamer, base.CachedEventSystem);
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void SetupEmote(string textKey, string input, GameObject extraDisplayElement)
	{
		if (m_Text != null)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(textKey) && Localization.Get(textKey, out var localized))
			{
				string text = $"[IN={input}]{localized}";
				m_Text.SetNonLocalizedText(text, checkMarkup: true);
				flag = true;
			}
			if (!flag)
			{
				string text2 = $"[IN={input}]{base.gameObject.name}";
				m_Text.m_bNeedsLocalization = false;
				m_Text.m_bForceRichText = true;
				m_Text.text = text2;
			}
		}
		if (extraDisplayElement != null)
		{
			extraDisplayElement.SetActive(value: true);
		}
	}
}
