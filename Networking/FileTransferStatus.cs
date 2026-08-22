using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200047A RID: 1146
	internal enum FileTransferStatus
	{
		// Token: 0x040028F3 RID: 10483
		NotStarted,
		// Token: 0x040028F4 RID: 10484
		Sending,
		// Token: 0x040028F5 RID: 10485
		Receiving,
		// Token: 0x040028F6 RID: 10486
		Finished,
		// Token: 0x040028F7 RID: 10487
		Canceled,
		// Token: 0x040028F8 RID: 10488
		Error
	}
}
