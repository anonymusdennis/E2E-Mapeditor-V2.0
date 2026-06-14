public interface IMinigameMasher
{
	bool HasCompletedRep();

	void EnableForPlayer(Player thePlayer);

	void Disable();

	bool IsEnabled();

	bool IsSignificantMomentInMinigame();
}
