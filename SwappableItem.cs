using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002E4 RID: 740
	internal class SwappableItem
	{
		// Token: 0x17001042 RID: 4162
		// (get) Token: 0x06003D73 RID: 15731 RVA: 0x0022EF3D File Offset: 0x0022D13D
		public int BasePrice { get; }

		// Token: 0x06003D74 RID: 15732 RVA: 0x0022EF48 File Offset: 0x0022D148
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

		// Token: 0x06003D75 RID: 15733 RVA: 0x0022EF94 File Offset: 0x0022D194
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

		// Token: 0x04002040 RID: 8256
		public readonly bool CanBeBought;

		// Token: 0x04002041 RID: 8257
		public readonly Identifier ReplacementOnUninstall;

		// Token: 0x04002042 RID: 8258
		public string SpawnWithId;

		// Token: 0x04002043 RID: 8259
		public string SwapIdentifier;

		// Token: 0x04002044 RID: 8260
		public readonly Vector2 SwapOrigin;

		// Token: 0x04002045 RID: 8261
		[TupleElementNames(new string[]
		{
			"requiredTag",
			"swapTo"
		})]
		public List<ValueTuple<Identifier, Identifier>> ConnectedItemsToSwap = new List<ValueTuple<Identifier, Identifier>>();

		// Token: 0x04002046 RID: 8262
		public readonly Sprite SchematicSprite;
	}
}
