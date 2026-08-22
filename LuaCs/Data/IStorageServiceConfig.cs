using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000597 RID: 1431
	public interface IStorageServiceConfig : IService, IDisposable
	{
		// Token: 0x17001599 RID: 5529
		// (get) Token: 0x06005703 RID: 22275
		string LocalModsDirectory { get; }

		// Token: 0x1700159A RID: 5530
		// (get) Token: 0x06005704 RID: 22276
		string WorkshopModsDirectory { get; }

		// Token: 0x1700159B RID: 5531
		// (get) Token: 0x06005705 RID: 22277
		string GameSettingsConfigPath { get; }

		// Token: 0x1700159C RID: 5532
		// (get) Token: 0x06005706 RID: 22278
		string TempDownloadsDirectory { get; }

		// Token: 0x1700159D RID: 5533
		// (get) Token: 0x06005707 RID: 22279
		string LocalDataSavePath { get; }

		// Token: 0x1700159E RID: 5534
		// (get) Token: 0x06005708 RID: 22280
		string LocalDataPathRegex { get; }

		// Token: 0x1700159F RID: 5535
		// (get) Token: 0x06005709 RID: 22281
		string LocalPackageDataPath { get; }
	}
}
