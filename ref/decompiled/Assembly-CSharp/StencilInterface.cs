using UnityEngine;

public interface StencilInterface
{
	StencilInterface GetPickedUpBy();

	StencilInterface GetCarrying();

	Directionx4 GetFacingDirectionEnum();

	Vector3 GetCachedCurrentPosition();

	float GetCharacterID();

	int GetFloorIndex();

	int GetCharacterListIndex();

	bool GetIsHiddenOrDisabled();

	bool ConsiderForCloseCheck();

	void SetLastCloseFrame(int framenum);

	int GetLastCloseFrame();
}
