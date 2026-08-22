using System;
using Concentus.Enums;
using Concentus.Structs;

namespace Barotrauma.Networking
{
	// Token: 0x02000474 RID: 1140
	internal static class VoipConfig
	{
		// Token: 0x06004DC6 RID: 19910 RVA: 0x002AB700 File Offset: 0x002A9900
		public static OpusEncoder CreateEncoder()
		{
			return new OpusEncoder(48000, 1, OpusApplication.OPUS_APPLICATION_VOIP)
			{
				Bandwidth = OpusBandwidth.OPUS_BANDWIDTH_AUTO,
				Bitrate = 16000,
				SignalType = OpusSignal.OPUS_SIGNAL_VOICE
			};
		}

		// Token: 0x06004DC7 RID: 19911 RVA: 0x002AB740 File Offset: 0x002A9940
		public static OpusDecoder CreateDecoder()
		{
			return new OpusDecoder(48000, 1);
		}

		// Token: 0x040028AE RID: 10414
		public const int MAX_COMPRESSED_SIZE = 40;

		// Token: 0x040028AF RID: 10415
		public static readonly TimeSpan SEND_INTERVAL = new TimeSpan(0, 0, 0, 0, 20);

		// Token: 0x040028B0 RID: 10416
		public const int FREQUENCY = 48000;

		// Token: 0x040028B1 RID: 10417
		public const int BITRATE = 16000;

		// Token: 0x040028B2 RID: 10418
		public const int BUFFER_SIZE = 960;
	}
}
