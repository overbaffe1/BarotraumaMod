using System;
using System.Runtime.CompilerServices;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020002CC RID: 716
	[NullableContext(1)]
	[Nullable(0)]
	public readonly struct CampaignDataPath
	{
		// Token: 0x06003039 RID: 12345 RVA: 0x0014B4F4 File Offset: 0x001496F4
		public CampaignDataPath(string loadPath, string savePath)
		{
			uint num;
			if (CampaignDataPath.IsBackupPath(savePath, out num))
			{
				throw new ArgumentException("Save path cannot be a backup path.", "savePath");
			}
			this.LoadPath = loadPath;
			this.SavePath = savePath;
		}

		// Token: 0x0600303A RID: 12346 RVA: 0x0014B529 File Offset: 0x00149729
		public static CampaignDataPath CreateRegular(string savePath)
		{
			return new CampaignDataPath(savePath, savePath);
		}

		// Token: 0x0600303B RID: 12347 RVA: 0x0014B534 File Offset: 0x00149734
		public static bool IsBackupPath(string path, out uint foundIndex)
		{
			string extension = Path.GetExtension(path);
			if (!extension.StartsWith(".bk", StringComparison.OrdinalIgnoreCase))
			{
				foundIndex = 0U;
				return false;
			}
			return SaveUtil.TryGetBackupIndexFromFileName(path, out foundIndex);
		}

		// Token: 0x0400182A RID: 6186
		public readonly string LoadPath;

		// Token: 0x0400182B RID: 6187
		public readonly string SavePath;

		// Token: 0x0400182C RID: 6188
		public static readonly CampaignDataPath Empty = new CampaignDataPath(string.Empty, string.Empty);
	}
}
