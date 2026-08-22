using System;

namespace Barotrauma
{
	// Token: 0x02000136 RID: 310
	[NotSyncedInMultiplayer]
	public abstract class HashlessFile : ContentFile
	{
		// Token: 0x06001BFB RID: 7163 RVA: 0x000CE16A File Offset: 0x000CC36A
		public HashlessFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BFC RID: 7164 RVA: 0x000CE174 File Offset: 0x000CC374
		public sealed override Md5Hash CalculateHash()
		{
			return Md5Hash.Blank;
		}
	}
}
