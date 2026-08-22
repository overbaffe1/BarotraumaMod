using System;

namespace Barotrauma
{
	// Token: 0x0200022C RID: 556
	[NotSyncedInMultiplayer]
	public abstract class HashlessFile : ContentFile
	{
		// Token: 0x060036DA RID: 14042 RVA: 0x00213C02 File Offset: 0x00211E02
		public HashlessFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036DB RID: 14043 RVA: 0x00213C0C File Offset: 0x00211E0C
		public sealed override Md5Hash CalculateHash()
		{
			return Md5Hash.Blank;
		}
	}
}
