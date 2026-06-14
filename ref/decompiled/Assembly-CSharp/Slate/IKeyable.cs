namespace Slate;

public interface IKeyable : IDirectable
{
	AnimationDataCollection animationData { get; }

	object animatedParametersTarget { get; }
}
