using UnityEngine;

public class ToggleEnabledOnKeyPress : MonoBehaviour
{
	public Light partner;

	public CustomLight customPartner;

	public bool activeLight;

	public bool swappedThisFrame;

	private void Start()
	{
		activeLight = true;
		if (partner != null)
		{
			partner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().activeLight = false;
		}
		if (customPartner != null)
		{
			customPartner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().activeLight = false;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown("l"))
		{
			Light component = GetComponent<Light>();
			if ((bool)component && activeLight)
			{
				component.enabled = !component.enabled;
			}
			CustomLight component2 = GetComponent<CustomLight>();
			if ((bool)component2 && activeLight)
			{
				component2.enabled = !component2.enabled;
			}
		}
		if (Input.GetKeyDown("s"))
		{
			Light component3 = GetComponent<Light>();
			if ((bool)component3)
			{
				if (component3.shadows == LightShadows.Soft)
				{
					component3.shadows = LightShadows.None;
				}
				else
				{
					component3.shadows = LightShadows.Soft;
				}
			}
		}
		if (Input.GetKeyDown("t"))
		{
			Light component4 = GetComponent<Light>();
			if (partner != null && activeLight && (bool)component4 && component4.enabled)
			{
				if (!swappedThisFrame)
				{
					activeLight = false;
					partner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().activeLight = true;
					partner.enabled = !partner.enabled;
					partner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().swappedThisFrame = true;
					component4.enabled = !component4.enabled;
				}
				else
				{
					swappedThisFrame = false;
				}
			}
			CustomLight component5 = GetComponent<CustomLight>();
			if (customPartner != null && activeLight && (bool)component5 && component5.enabled)
			{
				if (!swappedThisFrame)
				{
					activeLight = false;
					customPartner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().activeLight = true;
					customPartner.enabled = !customPartner.enabled;
					customPartner.gameObject.GetComponent<ToggleEnabledOnKeyPress>().swappedThisFrame = true;
					component4.enabled = !component4.enabled;
				}
				else
				{
					swappedThisFrame = false;
				}
			}
		}
		if (swappedThisFrame)
		{
			swappedThisFrame = false;
		}
	}
}
