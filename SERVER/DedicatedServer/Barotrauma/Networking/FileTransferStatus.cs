using System;

namespace Barotrauma.Networking
{
	// Token: 0x0200037E RID: 894
	internal enum FileTransferStatus
	{
		// Token: 0x04001B08 RID: 6920
		NotStarted,
		// Token: 0x04001B09 RID: 6921
		Sending,
		// Token: 0x04001B0A RID: 6922
		Receiving,
		// Token: 0x04001B0B RID: 6923
		Finished,
		// Token: 0x04001B0C RID: 6924
		Canceled,
		// Token: 0x04001B0D RID: 6925
		Error
	}
}
