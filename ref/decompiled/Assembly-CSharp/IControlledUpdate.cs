public interface IControlledUpdate
{
	void ControlledUpdate();

	void ControlledFixedUpdate();

	void ControlledLateUpdate();

	void ControlledPreUpdate();

	void ControlledPreFixedUpdate();

	bool RequiresControlledUpdate();

	bool RequiresControlledFixedUpdate();

	bool RequiresControlledLateUpdate();

	bool RequiresControlledPreUpdate();

	bool RequiresControlledPreFixedUpdate();
}
