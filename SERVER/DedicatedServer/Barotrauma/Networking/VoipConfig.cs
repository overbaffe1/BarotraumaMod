using System;
using Concentus.Structs;

namespace Barotrauma.Networking
{
	// Token: 0x020003CD RID: 973
	internal static class VoipConfig
	{
		// Token: 0x06003800 RID: 14336 RVA: 0x00177478 File Offset: 0x00175678
		public static OpusDecoder CreateDecoder()
		{
			return new OpusDecoder(48000, 1);
		}

		// Token: 0x04001C1E RID: 7198
		public const int MAX_COMPRESSED_SIZE = 40;

		// Token: 0x04001C1F RID: 7199
		public static readonly TimeSpan SEND_INTERVAL = new TimeSpan(0, 0, 0, 0, 20);

		// Token: 0x04001C20 RID: 7200
		public const int FREQUENCY = 48000;

		// Token: 0x04001C21 RID: 7201
		public const int BITRATE = 16000;

		// Token: 0x04001C22 RID: 7202
		public const int BUFFER_SIZE = 960;
	}
}
