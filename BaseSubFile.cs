using System;
using System.Security.Cryptography;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000246 RID: 582
	public abstract class BaseSubFile : ContentFile
	{
		// Token: 0x0600374F RID: 14159 RVA: 0x00214B28 File Offset: 0x00212D28
		protected BaseSubFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
			using (MD5 md5 = MD5.Create())
			{
				this.UintIdentifier = ToolBoxCore.StringToUInt32Hash(Barotrauma.IO.Path.GetFileNameWithoutExtension(path.Value), md5);
			}
		}

		// Token: 0x06003750 RID: 14160 RVA: 0x00214B78 File Offset: 0x00212D78
		public override void LoadFile()
		{
			SubmarineInfo.RefreshSavedSub(this.Path.Value);
		}

		// Token: 0x06003751 RID: 14161 RVA: 0x00214B8A File Offset: 0x00212D8A
		public override void UnloadFile()
		{
			SubmarineInfo.RemoveSavedSub(this.Path.Value);
		}

		// Token: 0x06003752 RID: 14162 RVA: 0x00214B9C File Offset: 0x00212D9C
		public override void Sort()
		{
		}

		// Token: 0x06003753 RID: 14163 RVA: 0x00214B9E File Offset: 0x00212D9E
		public override Md5Hash CalculateHash()
		{
			return Md5Hash.CalculateForFile(this.Path.FullPath, Md5Hash.StringHashOptions.BytePerfect);
		}

		// Token: 0x04001C0E RID: 7182
		public readonly uint UintIdentifier;
	}
}
