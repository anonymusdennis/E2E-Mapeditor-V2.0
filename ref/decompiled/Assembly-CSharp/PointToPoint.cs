using System.Collections.Generic;
using UnityEngine;

public class PointToPoint : MonoBehaviour
{
	public List<GameObject> points = new List<GameObject>();

	public float speed = 1f;

	private int listIndex;

	private bool bMoving = true;

	private void Update()
	{
		if (bMoving)
		{
			Vector3 vector = points[listIndex].transform.position - base.transform.position;
			float z = base.transform.position.z;
			base.transform.position += vector.normalized * (speed * UpdateManager.deltaTime);
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, z);
			if (Vector2.Distance(new Vector2(base.transform.position.x, base.transform.position.y), new Vector2(points[listIndex].transform.position.x, points[listIndex].transform.position.y)) < 0.1f)
			{
				bMoving = false;
			}
			return;
		}
		listIndex++;
		if (listIndex >= points.Count)
		{
			listIndex = 0;
		}
		base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, base.transform.eulerAngles.z + 90f);
		if (base.transform.eulerAngles.z >= 360f)
		{
			base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, base.transform.eulerAngles.z + 360f);
		}
		bMoving = true;
	}
}
