public interface IUpdateController
{
	void Register(IControlledUpdate behaviour);

	void Unregister(IControlledUpdate behaviour);

	void RunPreUpdates();

	void RunUpdates();

	void RunPreFixedUpdates();

	void RunFixedUpdates();

	void RunLateUpdates();

	void UnregisterAll();

	bool RequiresRunPreUpdates();

	bool RequiresRunUpdates();

	bool RequiresPreFixedUpdate();

	bool RequiresFixedUpdate();

	bool RequiresLateUpdates();
}
