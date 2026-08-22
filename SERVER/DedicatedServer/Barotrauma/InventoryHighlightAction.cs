using System;

namespace Barotrauma
{
	// Token: 0x020001A1 RID: 417
	internal class InventoryHighlightAction : EventAction
	{
		// Token: 0x170008EC RID: 2284
		// (get) Token: 0x06001F2E RID: 7982 RVA: 0x000D835C File Offset: 0x000D655C
		// (set) Token: 0x06001F2F RID: 7983 RVA: 0x000D8364 File Offset: 0x000D6564
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity or entities whose inventory the item should be highlighted in. Must be a character or an item with an inventory.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170008ED RID: 2285
		// (get) Token: 0x06001F30 RID: 7984 RVA: 0x000D836D File Offset: 0x000D656D
		// (set) Token: 0x06001F31 RID: 7985 RVA: 0x000D8375 File Offset: 0x000D6575
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the item(s) to highlight.", "", false)]
		public Identifier ItemIdentifier { get; set; }

		// Token: 0x170008EE RID: 2286
		// (get) Token: 0x06001F32 RID: 7986 RVA: 0x000D837E File Offset: 0x000D657E
		// (set) Token: 0x06001F33 RID: 7987 RVA: 0x000D8386 File Offset: 0x000D6586
		[Serialize(-1, IsPropertySaveable.Yes, "If the target is an item with multiple ItemContainer components (i.e. multiple inventories), such as a fabricator, this determines which inventory to highlight the item in (0 = first, 1 = second). If negative, it doesn't matter which inventory the item is in.", "", false)]
		public int ItemContainerIndex { get; set; }

		// Token: 0x170008EF RID: 2287
		// (get) Token: 0x06001F34 RID: 7988 RVA: 0x000D838F File Offset: 0x000D658F
		// (set) Token: 0x06001F35 RID: 7989 RVA: 0x000D8397 File Offset: 0x000D6597
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the action will go look through all the containers in the target inventory (e.g. highlighting a tank in a welding tool in the target inventory).", "", false)]
		public bool Recursive { get; set; }

		// Token: 0x06001F36 RID: 7990 RVA: 0x000D83A0 File Offset: 0x000D65A0
		public InventoryHighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x000D83AA File Offset: 0x000D65AA
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x000D83BC File Offset: 0x000D65BC
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x000D83C4 File Offset: 0x000D65C4
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000EE5 RID: 3813
		private bool isFinished;
	}
}
