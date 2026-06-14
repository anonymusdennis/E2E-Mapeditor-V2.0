public interface ICarryableObjectConsumer
{
	bool WillAcceptInput(CarryObjectInteraction theObject);

	bool OnCarriedObjectDroppedOnUs(CarryObjectInteraction theObject);
}
