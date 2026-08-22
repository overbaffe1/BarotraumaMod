using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200047F RID: 1151
	internal interface IServerSerializable : INetSerializable
	{
		// Token: 0x06004DCE RID: 19918
		void ClientEventRead(IReadMessage msg, float sendingTime);
	}
}
