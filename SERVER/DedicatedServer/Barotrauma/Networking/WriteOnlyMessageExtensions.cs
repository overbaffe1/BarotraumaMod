using System;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x020003B5 RID: 949
	internal static class WriteOnlyMessageExtensions
	{
		// Token: 0x06003786 RID: 14214 RVA: 0x00175D49 File Offset: 0x00173F49
		public static IWriteMessage WithHeader(this IWriteMessage msg, ServerPacketHeader header)
		{
			msg.WriteByte((byte)header);
			return msg;
		}

		// Token: 0x06003787 RID: 14215 RVA: 0x00175D54 File Offset: 0x00173F54
		public static void WriteNetSerializableStruct<T>(this IWriteMessage msg, T serializableStruct) where T : INetSerializableStruct
		{
			serializableStruct.Write(msg);
		}

		// Token: 0x06003788 RID: 14216 RVA: 0x00175D64 File Offset: 0x00173F64
		public static NetOutgoingMessage ToLidgren(this IWriteMessage msg, NetPeer peer)
		{
			NetOutgoingMessage outMsg = peer.CreateMessage();
			outMsg.Write(msg.Buffer, 0, msg.LengthBytes);
			return outMsg;
		}
	}
}
