public static class FrontendUserPath
{
	public static FrontEndFlow.MenuType m_FrontendSection;

	public static int m_FrontendMenuIndex;

	public static int m_MenuChildIndex;

	public static void RecordFrontendPath(FrontEndFlow.MenuType section, int menuIndex, int menuChildIndex)
	{
		m_FrontendSection = section;
		m_FrontendMenuIndex = menuIndex;
		m_MenuChildIndex = menuChildIndex;
	}

	public static void ClearPath()
	{
		m_FrontendSection = FrontEndFlow.MenuType.Unassigned;
		m_FrontendMenuIndex = -1;
		m_MenuChildIndex = -1;
	}
}
