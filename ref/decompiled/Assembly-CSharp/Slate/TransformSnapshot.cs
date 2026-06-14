using System.Collections.Generic;
using UnityEngine;

namespace Slate;

public class TransformSnapshot
{
	public enum StoreMode
	{
		All,
		RootOnly,
		ChildrenOnly
	}

	private struct TransformData
	{
		public Transform transform;

		public Transform parent;

		public Vector3 pos;

		public Quaternion rot;

		public Vector3 scale;

		public TransformData(Transform transform, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale)
		{
			this.transform = transform;
			this.parent = parent;
			this.pos = pos;
			this.rot = rot;
			this.scale = scale;
		}
	}

	private List<TransformData> data;

	public TransformSnapshot(GameObject root, StoreMode mode)
	{
		Store(root, mode);
	}

	public void Store(GameObject root, StoreMode mode)
	{
		if (root == null)
		{
			return;
		}
		data = new List<TransformData>();
		if (mode == StoreMode.RootOnly)
		{
			Transform transform = root.transform;
			data.Add(new TransformData(transform, transform.parent, transform.localPosition, transform.localRotation, transform.localScale));
			return;
		}
		Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform2 in componentsInChildren)
		{
			if (transform2 != root.transform || mode == StoreMode.All)
			{
				data.Add(new TransformData(transform2, transform2.parent, transform2.localPosition, transform2.localRotation, transform2.localScale));
			}
		}
	}

	public void Restore()
	{
		for (int i = 0; i < data.Count; i++)
		{
			TransformData transformData = data[i];
			if (!(transformData.transform == null))
			{
				transformData.transform.SetParent(transformData.parent, !(transformData.transform is RectTransform));
				transformData.transform.localPosition = transformData.pos;
				transformData.transform.localRotation = transformData.rot;
				transformData.transform.localScale = transformData.scale;
			}
		}
	}
}
