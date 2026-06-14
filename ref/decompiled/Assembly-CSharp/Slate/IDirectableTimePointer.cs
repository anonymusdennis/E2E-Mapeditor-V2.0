namespace Slate;

public interface IDirectableTimePointer
{
	float time { get; }

	void TriggerForward(float currentTime, float previousTime);

	void TriggerBackward(float currentTime, float previousTime);

	void Update(float currentTime, float previousTime);
}
