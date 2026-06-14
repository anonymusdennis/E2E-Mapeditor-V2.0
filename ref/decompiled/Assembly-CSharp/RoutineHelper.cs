public static class RoutineHelper
{
	public static bool IsValid(Routines routine)
	{
		return routine != Routines.UNASSIGNED && routine != Routines.COUNT;
	}
}
