using System;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001A6 RID: 422
	internal class AIChatMessage
	{
		// Token: 0x06003041 RID: 12353 RVA: 0x001F95D1 File Offset: 0x001F77D1
		public AIChatMessage(string message, ChatMessageType? type, Identifier identifier = default(Identifier), float delay = 0f)
		{
			this.Message = message;
			this.MessageType = type;
			this.Identifier = identifier;
			this.SendDelay = delay;
		}

		// Token: 0x0400191F RID: 6431
		public readonly string Message;

		// Token: 0x04001920 RID: 6432
		public readonly Identifier Identifier;

		// Token: 0x04001921 RID: 6433
		public ChatMessageType? MessageType;

		// Token: 0x04001922 RID: 6434
		public float SendDelay;

		// Token: 0x04001923 RID: 6435
		public double SendTime;
	}
}
