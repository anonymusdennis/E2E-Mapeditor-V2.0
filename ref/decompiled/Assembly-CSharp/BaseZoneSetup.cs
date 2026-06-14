using System.Collections.Generic;
using UnityEngine;

public abstract class BaseZoneSetup : MonoBehaviour
{
	public abstract void SetupZone(LevelEditor_ZoneManager.Zone myZone, LevelEditor_ZoneManager.Zone.ObjectsInZone objInZone, ref List<Object> tempZoneData);
}
