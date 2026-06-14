using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Framework.Internal;

public class VersionUpdateProxyGraph : MonoBehaviour
{
	public string _serializedGraph;

	public List<Object> _objectReferences;

	public void GetSerializationData(out string json, out List<Object> references)
	{
		json = _serializedGraph;
		references = ((_objectReferences == null) ? null : new List<Object>(_objectReferences));
	}
}
