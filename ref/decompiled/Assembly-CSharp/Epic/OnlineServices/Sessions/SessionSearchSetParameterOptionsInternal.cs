using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchSetParameterOptionsInternal : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Parameter;

	private ComparisonOp m_ComparisonOp;

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

	public AttributeDataInternal Parameter
	{
		get
		{
			return Helper.GetAllocation<AttributeDataInternal>(m_Parameter);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Parameter, value);
		}
	}

	public ComparisonOp ComparisonOp
	{
		get
		{
			return m_ComparisonOp;
		}
		set
		{
			m_ComparisonOp = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Parameter);
	}
}
