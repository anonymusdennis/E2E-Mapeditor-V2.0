using System.Runtime.InteropServices;

namespace DataHelpers;

[StructLayout(LayoutKind.Explicit)]
internal struct FloatIntConversion
{
	[FieldOffset(0)]
	public uint m_ValueInt;

	[FieldOffset(0)]
	public float m_ValueFloat;
}
