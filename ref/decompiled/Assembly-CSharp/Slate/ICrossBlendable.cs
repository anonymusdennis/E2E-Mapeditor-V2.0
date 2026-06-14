namespace Slate;

public interface ICrossBlendable : IDirectable
{
	new float blendIn { get; set; }

	new float blendOut { get; set; }
}
