using System;
using System.Runtime.CompilerServices;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000397 RID: 919
	[NullableContext(1)]
	[Nullable(0)]
	public readonly struct CampaignDataPath
	{
		// Token: 0x060044B7 RID: 17591 RVA: 0x00264FB8 File Offset: 0x002631B8
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

		// Token: 0x060044B8 RID: 17592 RVA: 0x00264FED File Offset: 0x002631ED
		public static CampaignDataPath CreateRegular(string savePath)
		{
			return new CampaignDataPath(savePath, savePath);
		}

		// Token: 0x060044B9 RID: 17593 RVA: 0x00264FF8 File Offset: 0x002631F8
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

		// Token: 0x040023FC RID: 9212
		public readonly string LoadPath;

		// Token: 0x040023FD RID: 9213
		public readonly string SavePath;

		// Token: 0x040023FE RID: 9214
		public static readonly CampaignDataPath Empty = new CampaignDataPath(string.Empty, string.Empty);
	}
}
