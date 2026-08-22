using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200047E RID: 1150
	internal interface IClientSerializable : INetSerializable
	{
		// Token: 0x06004DCD RID: 19917
		void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null);
	}
}
