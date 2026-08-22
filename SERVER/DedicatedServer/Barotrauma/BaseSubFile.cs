using System;
using System.Security.Cryptography;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000150 RID: 336
	public abstract class BaseSubFile : ContentFile
	{
		// Token: 0x06001C66 RID: 7270 RVA: 0x000CEF64 File Offset: 0x000CD164
		protected BaseSubFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
			using (MD5 md5 = MD5.Create())
			{
				this.UintIdentifier = ToolBoxCore.StringToUInt32Hash(Barotrauma.IO.Path.GetFileNameWithoutExtension(path.Value), md5);
			}
		}

		// Token: 0x06001C67 RID: 7271 RVA: 0x000CEFB4 File Offset: 0x000CD1B4
		public override void LoadFile()
		{
			SubmarineInfo.RefreshSavedSub(this.Path.Value);
		}

		// Token: 0x06001C68 RID: 7272 RVA: 0x000CEFC6 File Offset: 0x000CD1C6
		public override void UnloadFile()
		{
			SubmarineInfo.RemoveSavedSub(this.Path.Value);
		}

		// Token: 0x06001C69 RID: 7273 RVA: 0x000CEFD8 File Offset: 0x000CD1D8
		public override void Sort()
		{
		}

		// Token: 0x06001C6A RID: 7274 RVA: 0x000CEFDA File Offset: 0x000CD1DA
		public override Md5Hash CalculateHash()
		{
			return Md5Hash.CalculateForFile(this.Path.FullPath, Md5Hash.StringHashOptions.BytePerfect);
		}

		// Token: 0x04000D01 RID: 3329
		public readonly uint UintIdentifier;
	}
}
