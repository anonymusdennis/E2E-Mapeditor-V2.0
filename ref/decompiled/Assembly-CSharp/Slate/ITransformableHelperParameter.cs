using UnityEngine;

namespace Slate;

public interface ITransformableHelperParameter
{
	Transform transform { get; }

	TransformSpace space { get; }

	bool useAnimation { get; }
}
