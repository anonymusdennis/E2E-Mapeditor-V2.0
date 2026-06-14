using Photon;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
[DisallowMultipleComponent]
public class T17NetShowInfo : Photon.MonoBehaviour
{
	public float CharacterSize;

	public Font font;

	public bool DisableOnOwnObjects;

	public Color colour { get; set; }

	public void Awake()
	{
	}

	protected virtual void OnDestroy()
	{
		font = null;
	}
}
