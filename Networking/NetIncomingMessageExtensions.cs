using System;
using System.Runtime.CompilerServices;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020004B3 RID: 1203
	internal static class NetIncomingMessageExtensions
	{
		// Token: 0x06004F5E RID: 20318 RVA: 0x002AE8C8 File Offset: 0x002ACAC8
		public unsafe static T ReadHeader<T>(this NetIncomingMessage msg) where T : Enum
		{
			byte header = msg.ReadByte();
			return *Unsafe.As<byte, T>(ref header);
		}

		// Token: 0x06004F5F RID: 20319 RVA: 0x002AE8E8 File Offset: 0x002ACAE8
		public static IReadMessage ToReadMessage(this NetIncomingMessage msg)
		{
			return new ReadWriteMessage(msg.Data, 0, msg.LengthBits, false);
		}
	}
}
