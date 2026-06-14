using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Achievements;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OnAchievementsUnlockedCallbackInfoInternal : ICallbackInfo, IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_UserId;

	private uint m_AchievementsCount;

	private IntPtr m_AchievementIds;

	public object ClientData => Helper.GetClientData(m_ClientData);

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId UserId => Helper.GetHandle<ProductUserId>(m_UserId);

	public string[] AchievementIds => Helper.GetAllocation<string[]>(m_AchievementIds, (int)m_AchievementsCount);

	public void Dispose()
	{
	}
}
