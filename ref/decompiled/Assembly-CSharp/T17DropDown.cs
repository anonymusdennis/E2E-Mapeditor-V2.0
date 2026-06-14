using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("T17_UI/Dropdown", 35)]
public class T17DropDown : Dropdown, IT17EventHelper
{
	public void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem)
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
	}

	public T17EventSystem GetDomain()
	{
		return null;
	}

	public GameObject GetGameobject()
	{
		return base.gameObject;
	}

	public bool CanReselectOnMouseDisable()
	{
		return true;
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		return true;
	}
}
