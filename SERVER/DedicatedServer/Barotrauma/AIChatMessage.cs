using System;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000A2 RID: 162
	internal class AIChatMessage
	{
		// Token: 0x06001366 RID: 4966 RVA: 0x000A7C84 File Offset: 0x000A5E84
		public AIChatMessage(string message, ChatMessageType? type, Identifier identifier = default(Identifier), float delay = 0f)
		{
			this.Message = message;
			this.MessageType = type;
			this.Identifier = identifier;
			this.SendDelay = delay;
		}

		// Token: 0x04000934 RID: 2356
		public readonly string Message;

		// Token: 0x04000935 RID: 2357
		public readonly Identifier Identifier;

		// Token: 0x04000936 RID: 2358
		public ChatMessageType? MessageType;

		// Token: 0x04000937 RID: 2359
		public float SendDelay;

		// Token: 0x04000938 RID: 2360
		public double SendTime;
	}
}
