using System;

namespace Barotrauma
{
	// Token: 0x02000226 RID: 550
	public sealed class DecalsFile : ContentFile
	{
		// Token: 0x060036BD RID: 14013 RVA: 0x00213950 File Offset: 0x00211B50
		public DecalsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036BE RID: 14014 RVA: 0x0021395A File Offset: 0x00211B5A
		public override void LoadFile()
		{
			DecalManager.LoadFromFile(this);
		}

		// Token: 0x060036BF RID: 14015 RVA: 0x00213962 File Offset: 0x00211B62
		public override void UnloadFile()
		{
			DecalManager.RemoveByFile(this);
		}

		// Token: 0x060036C0 RID: 14016 RVA: 0x0021396A File Offset: 0x00211B6A
		public override void Sort()
		{
			DecalManager.SortAll();
		}
	}
}
