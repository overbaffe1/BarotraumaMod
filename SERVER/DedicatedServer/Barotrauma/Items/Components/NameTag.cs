using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D3 RID: 1235
	internal class NameTag : ItemComponent
	{
		// Token: 0x170012E0 RID: 4832
		// (get) Token: 0x06004658 RID: 18008 RVA: 0x001C1646 File Offset: 0x001BF846
		// (set) Token: 0x06004659 RID: 18009 RVA: 0x001C164E File Offset: 0x001BF84E
		[InGameEditable(MaxLength = 32)]
		[Serialize("", IsPropertySaveable.No, "Name written on the tag.", "", true)]
		public string WrittenName { get; set; }

		// Token: 0x0600465A RID: 18010 RVA: 0x001C1657 File Offset: 0x001BF857
		public NameTag(Item item, ContentXElement element) : base(item, element)
		{
			base.AllowInGameEditing = true;
			base.DrawHudWhenEquipped = true;
			item.EditableWhenEquipped = true;
		}
	}
}
