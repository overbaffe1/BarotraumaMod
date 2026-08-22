using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047B RID: 1147
	public interface IStorageServiceConfig : IService, IDisposable
	{
		// Token: 0x17001035 RID: 4149
		// (get) Token: 0x06003D9D RID: 15773
		string LocalModsDirectory { get; }

		// Token: 0x17001036 RID: 4150
		// (get) Token: 0x06003D9E RID: 15774
		string WorkshopModsDirectory { get; }

		// Token: 0x17001037 RID: 4151
		// (get) Token: 0x06003D9F RID: 15775
		string GameSettingsConfigPath { get; }

		// Token: 0x17001038 RID: 4152
		// (get) Token: 0x06003DA0 RID: 15776
		string LocalDataSavePath { get; }

		// Token: 0x17001039 RID: 4153
		// (get) Token: 0x06003DA1 RID: 15777
		string LocalDataPathRegex { get; }

		// Token: 0x1700103A RID: 4154
		// (get) Token: 0x06003DA2 RID: 15778
		string LocalPackageDataPath { get; }
	}
}
