using System;
using System.Runtime.CompilerServices;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020003B6 RID: 950
	internal static class NetIncomingMessageExtensions
	{
		// Token: 0x06003789 RID: 14217 RVA: 0x00175D8C File Offset: 0x00173F8C
		public unsafe static T ReadHeader<T>(this NetIncomingMessage msg) where T : Enum
		{
			byte header = msg.ReadByte();
			return *Unsafe.As<byte, T>(ref header);
		}

		// Token: 0x0600378A RID: 14218 RVA: 0x00175DAC File Offset: 0x00173FAC
		public static IReadMessage ToReadMessage(this NetIncomingMessage msg)
		{
			return new ReadWriteMessage(msg.Data, 0, msg.LengthBits, false);
		}
	}
}
