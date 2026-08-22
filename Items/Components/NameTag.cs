using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005FD RID: 1533
	internal class NameTag : ItemComponent
	{
		// Token: 0x17001944 RID: 6468
		// (get) Token: 0x060063CF RID: 25551 RVA: 0x0033EADA File Offset: 0x0033CCDA
		// (set) Token: 0x060063D0 RID: 25552 RVA: 0x0033EAE2 File Offset: 0x0033CCE2
		[InGameEditable(MaxLength = 32)]
		[Serialize("", IsPropertySaveable.No, "Name written on the tag.", "", true)]
		public string WrittenName { get; set; }

		// Token: 0x060063D1 RID: 25553 RVA: 0x0033EAEB File Offset: 0x0033CCEB
		public NameTag(Item item, ContentXElement element) : base(item, element)
		{
			base.AllowInGameEditing = true;
			base.DrawHudWhenEquipped = true;
			item.EditableWhenEquipped = true;
		}
	}
}
