using System;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020004B2 RID: 1202
	internal static class WriteOnlyMessageExtensions
	{
		// Token: 0x06004F5B RID: 20315 RVA: 0x002AE885 File Offset: 0x002ACA85
		public static IWriteMessage WithHeader(this IWriteMessage msg, ClientPacketHeader header)
		{
			msg.WriteByte((byte)header);
			return msg;
		}

		// Token: 0x06004F5C RID: 20316 RVA: 0x002AE890 File Offset: 0x002ACA90
		public static void WriteNetSerializableStruct<T>(this IWriteMessage msg, T serializableStruct) where T : INetSerializableStruct
		{
			serializableStruct.Write(msg);
		}

		// Token: 0x06004F5D RID: 20317 RVA: 0x002AE8A0 File Offset: 0x002ACAA0
		public static NetOutgoingMessage ToLidgren(this IWriteMessage msg, NetPeer peer)
		{
			NetOutgoingMessage outMsg = peer.CreateMessage();
			outMsg.Write(msg.Buffer, 0, msg.LengthBytes);
			return outMsg;
		}
	}
}
