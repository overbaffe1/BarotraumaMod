using System;
using System.Runtime.InteropServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004A2 RID: 1186
	[StructLayout(LayoutKind.Explicit)]
	public struct SingleUIntUnion
	{
		// Token: 0x040029BE RID: 10686
		[FieldOffset(0)]
		public float SingleValue;

		// Token: 0x040029BF RID: 10687
		[FieldOffset(0)]
		public uint UIntValue;
	}
}
