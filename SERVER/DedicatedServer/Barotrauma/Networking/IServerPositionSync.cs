using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000384 RID: 900
	internal interface IServerPositionSync : IServerSerializable, INetSerializable
	{
		// Token: 0x06003613 RID: 13843
		void ServerWritePosition(ReadWriteMessage tempBuffer, Client c);
	}
}
