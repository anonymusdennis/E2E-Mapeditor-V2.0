using UnityEngine;

public class ToggleDayNight : MonoBehaviour
{
	private bool isDay = true;

	public float DayIntensity = 1.156f;

	public Color DayColour = new Color(255f, 255f, 255f, 255f);

	public float NightIntensity;

	public Color NightColour;

	private void Start()
	{
		RenderSettings.ambientLight = DayColour;
		RenderSettings.ambientIntensity = DayIntensity;
	}

	private void Update()
	{
		if (Input.GetKeyDown("d"))
		{
			if (isDay)
			{
				RenderSettings.ambientLight = NightColour;
				RenderSettings.ambientIntensity = NightIntensity;
			}
			else
			{
				RenderSettings.ambientLight = DayColour;
				RenderSettings.ambientIntensity = DayIntensity;
			}
			isDay = !isDay;
		}
	}
}
