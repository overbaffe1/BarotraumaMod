using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000382 RID: 898
	internal interface IClientSerializable : INetSerializable
	{
		// Token: 0x06003611 RID: 13841
		void ServerEventRead(IReadMessage msg, Client c);
	}
}
