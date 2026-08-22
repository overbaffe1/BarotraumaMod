using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C1 RID: 1217
	internal class Sprayer : RangedWeapon
	{
		// Token: 0x17001294 RID: 4756
		// (get) Token: 0x06004568 RID: 17768 RVA: 0x001BCAD0 File Offset: 0x001BACD0
		// (set) Token: 0x06004569 RID: 17769 RVA: 0x001BCAD8 File Offset: 0x001BACD8
		[Serialize(0f, IsPropertySaveable.No, "The distance at which the item can spray walls.", "", false)]
		public float Range { get; set; }

		// Token: 0x17001295 RID: 4757
		// (get) Token: 0x0600456A RID: 17770 RVA: 0x001BCAE1 File Offset: 0x001BACE1
		// (set) Token: 0x0600456B RID: 17771 RVA: 0x001BCAE9 File Offset: 0x001BACE9
		[Serialize(1f, IsPropertySaveable.No, "How fast the item changes the color of the walls.", "", false)]
		public float SprayStrength { get; set; }

		// Token: 0x17001296 RID: 4758
		// (get) Token: 0x0600456C RID: 17772 RVA: 0x001BCAF2 File Offset: 0x001BACF2
		// (set) Token: 0x0600456D RID: 17773 RVA: 0x001BCAFA File Offset: 0x001BACFA
		public ItemContainer LiquidContainer { get; private set; }

		// Token: 0x0600456E RID: 17774 RVA: 0x001BCB04 File Offset: 0x001BAD04
		public Sprayer(Item item, ContentXElement element) : base(item, element)
		{
			item.IsShootable = true;
			item.RequireAimToUse = true;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "paintcolors")
				{
					Dictionary<Identifier, Color> liquidColors = new Dictionary<Identifier, Color>();
					foreach (ContentXElement cxe in subElement.Elements())
					{
						XElement paintElement = cxe;
						Identifier paintName = paintElement.GetAttributeIdentifier("paintitem", Identifier.Empty);
						Color paintColor = paintElement.GetAttributeColor("color", Color.Transparent);
						if (paintName != string.Empty)
						{
							liquidColors.Add(paintName, paintColor);
						}
					}
					this.LiquidColors = liquidColors.ToImmutableDictionary<Identifier, Color>();
				}
			}
		}

		// Token: 0x0600456F RID: 17775 RVA: 0x001BCC18 File Offset: 0x001BAE18
		public override void OnItemLoaded()
		{
			this.LiquidContainer = this.item.GetComponent<ItemContainer>();
		}

		// Token: 0x06004570 RID: 17776 RVA: 0x001BCC2B File Offset: 0x001BAE2B
		public override bool Use(float deltaTime, Character character = null)
		{
			return character != null || character.Removed;
		}

		// Token: 0x0400215C RID: 8540
		public readonly ImmutableDictionary<Identifier, Color> LiquidColors;
	}
}
