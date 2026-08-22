using System;

namespace Barotrauma
{
	// Token: 0x02000130 RID: 304
	public sealed class DecalsFile : ContentFile
	{
		// Token: 0x06001BDE RID: 7134 RVA: 0x000CDEB8 File Offset: 0x000CC0B8
		public DecalsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x000CDEC2 File Offset: 0x000CC0C2
		public override void LoadFile()
		{
			DecalManager.LoadFromFile(this);
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x000CDECA File Offset: 0x000CC0CA
		public override void UnloadFile()
		{
			DecalManager.RemoveByFile(this);
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x000CDED2 File Offset: 0x000CC0D2
		public override void Sort()
		{
			DecalManager.SortAll();
		}
	}
}
