using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000383 RID: 899
	internal interface IServerSerializable : INetSerializable
	{
		// Token: 0x06003612 RID: 13842
		void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null);
	}
}
