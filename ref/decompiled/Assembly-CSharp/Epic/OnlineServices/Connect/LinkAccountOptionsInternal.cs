using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LinkAccountOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_ContinuanceToken;

	public int ApiVersion
	{
		get
		{
			return m_ApiVersion;
		}
		set
		{
			m_ApiVersion = value;
		}
	}

	public ProductUserId LocalUserId
	{
		get
		{
			return Helper.GetHandle<ProductUserId>(m_LocalUserId);
		}
		set
		{
			m_LocalUserId = Helper.GetInnerHandle(value);
		}
	}

	public ContinuanceToken ContinuanceToken
	{
		get
		{
			return Helper.GetHandle<ContinuanceToken>(m_ContinuanceToken);
		}
		set
		{
			m_ContinuanceToken = Helper.GetInnerHandle(value);
		}
	}

	public void Dispose()
	{
	}
}
