using UTJ;
using UnityEngine;

namespace Slate;

public class AlembicStreamRoot : AlembicStream
{
	public void Sample(float time)
	{
		if (!AbcIsValid())
		{
			Debug.LogError($"Alembic {base.gameObject.name} is invalid", base.gameObject);
		}
		else
		{
			AbcUpdateBegin(time);
		}
	}

	public void Validate()
	{
		AlembicCamera[] componentsInChildren = GetComponentsInChildren<AlembicCamera>(includeInactive: true);
		foreach (AlembicCamera alembicCamera in componentsInChildren)
		{
			ShotCamera component = alembicCamera.GetComponent<ShotCamera>();
			if (component == null)
			{
				component = alembicCamera.gameObject.AddComponent<ShotCamera>();
			}
		}
	}

	public void Initialize()
	{
		Refresh();
	}
}
