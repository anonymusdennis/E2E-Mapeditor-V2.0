using Rotorz.Tile;
using UnityEngine;

public class ItemCover : T17MonoBehaviour
{
	public MeshRenderer m_MeshRenderer;

	private void Start()
	{
		TileSystem componentInParent = GetComponentInParent<TileSystem>();
		bool flag = false;
		if (componentInParent != null)
		{
			flag = !componentInParent.CompareTag("GroundTiles");
		}
		Vector3 position = base.transform.position;
		if (flag)
		{
			if (LevelScript.GetInstance().m_Processed)
			{
				Vector3 position2 = base.transform.position;
				position2.y -= 1f;
				float zOffset = LayerHelper.GetZOffset(position2);
				Vector3 localPosition = base.transform.localPosition;
				localPosition.z += zOffset;
				base.transform.localPosition = localPosition;
			}
			else
			{
				base.transform.position = new Vector3(position.x, position.y, position.z - 0.1f);
			}
		}
		else
		{
			base.transform.position = new Vector3(position.x, position.y, position.z - 0.05f);
		}
		CullingObjectCollector.GetInstance().Runtime_AddToBucket(m_MeshRenderer, bCheckForMaterialBlock: true, bAlsoFloorsAbove: true);
	}

	protected virtual void OnDestroy()
	{
		if (CullingObjectCollector.GetInstance() != null)
		{
			CullingObjectCollector.GetInstance().Runtime_RemoveFromBucket(m_MeshRenderer, bCheckForMaterialBlock: true);
		}
		if (CameraManager.GetInstance() != null)
		{
			CameraManager.GetInstance().ForceAnUpdateForActiveCameras();
		}
	}

	public void SetMaterial(Material material)
	{
		if (!(material == null) && m_MeshRenderer != null)
		{
			m_MeshRenderer.material = new Material(material);
			m_MeshRenderer.material.color = Color.white;
		}
	}
}
