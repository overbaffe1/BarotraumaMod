using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000480 RID: 1152
	internal interface IServerPositionSync : IServerSerializable, INetSerializable
	{
		// Token: 0x06004DCF RID: 19919
		void ClientReadPosition(IReadMessage msg, float sendingTime);
	}
}
