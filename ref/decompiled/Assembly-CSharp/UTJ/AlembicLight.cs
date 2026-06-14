using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicLight : AlembicElement
{
	public override void AbcSetup(AlembicStream abcStream, AbcAPI.aiObject abcObj, AbcAPI.aiSchema abcSchema)
	{
		base.AbcSetup(abcStream, abcObj, abcSchema);
		Light orAddComponent = GetOrAddComponent<Light>();
		orAddComponent.enabled = false;
	}

	public override void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged)
	{
	}

	public override void AbcUpdate()
	{
		if (AbcIsDirty())
		{
			AbcClean();
		}
	}
}
