using System;
using System.Runtime.InteropServices;

namespace Barotrauma.Networking
{
	// Token: 0x020003A5 RID: 933
	[StructLayout(LayoutKind.Explicit)]
	public struct SingleUIntUnion
	{
		// Token: 0x04001BC7 RID: 7111
		[FieldOffset(0)]
		public float SingleValue;

		// Token: 0x04001BC8 RID: 7112
		[FieldOffset(0)]
		public uint UIntValue;
	}
}
