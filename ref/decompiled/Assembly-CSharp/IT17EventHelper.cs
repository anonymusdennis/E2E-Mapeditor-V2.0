using UnityEngine;

public interface IT17EventHelper
{
	void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem = null);

	T17EventSystem GetDomain();

	GameObject GetGameobject();

	bool CanReselectOnMouseDisable();

	bool ReleaseSelectionOnPointerClickOrExit();
}
