using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FA RID: 506
	internal class SwappableItem
	{
		// Token: 0x17000A4E RID: 2638
		// (get) Token: 0x060023CD RID: 9165 RVA: 0x000EF7D9 File Offset: 0x000ED9D9
		public int BasePrice { get; }

		// Token: 0x060023CE RID: 9166 RVA: 0x000EF7E4 File Offset: 0x000ED9E4
		public int GetPrice(Location location = null)
		{
			int price = (location != null) ? location.GetAdjustedMechanicalCost(this.BasePrice) : this.BasePrice;
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				price = (int)((float)price * campaign.Settings.ShipyardPriceMultiplier);
			}
			return price;
		}

		// Token: 0x060023CF RID: 9167 RVA: 0x000EF830 File Offset: 0x000EDA30
		public SwappableItem(ContentXElement element)
		{
			this.BasePrice = Math.Max(element.GetAttributeInt("price", 0), 0);
			this.SwapIdentifier = element.GetAttributeString("swapidentifier", string.Empty);
			this.CanBeBought = element.GetAttributeBool("canbebought", this.BasePrice != 0);
			this.ReplacementOnUninstall = element.GetAttributeIdentifier("replacementonuninstall", "");
			string key = "origin";
			Vector2 one = Vector2.One;
			this.SwapOrigin = element.GetAttributeVector2(key, one);
			this.SpawnWithId = element.GetAttributeString("spawnwithid", string.Empty);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "schematicsprite"))
				{
					if (a == "swapconnecteditem")
					{
						this.ConnectedItemsToSwap.Add(new ValueTuple<Identifier, Identifier>(subElement.GetAttributeIdentifier("tag", ""), subElement.GetAttributeIdentifier("swapto", "")));
					}
				}
				else
				{
					this.SchematicSprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
		}

		// Token: 0x040011B7 RID: 4535
		public readonly bool CanBeBought;

		// Token: 0x040011B8 RID: 4536
		public readonly Identifier ReplacementOnUninstall;

		// Token: 0x040011B9 RID: 4537
		public string SpawnWithId;

		// Token: 0x040011BA RID: 4538
		public string SwapIdentifier;

		// Token: 0x040011BB RID: 4539
		public readonly Vector2 SwapOrigin;

		// Token: 0x040011BC RID: 4540
		[TupleElementNames(new string[]
		{
			"requiredTag",
			"swapTo"
		})]
		public List<ValueTuple<Identifier, Identifier>> ConnectedItemsToSwap = new List<ValueTuple<Identifier, Identifier>>();

		// Token: 0x040011BD RID: 4541
		public readonly Sprite SchematicSprite;
	}
}
