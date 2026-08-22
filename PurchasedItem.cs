using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020002BF RID: 703
	internal class PurchasedItem
	{
		// Token: 0x17000FD9 RID: 4057
		// (get) Token: 0x06003C65 RID: 15461 RVA: 0x0022AF61 File Offset: 0x00229161
		public ItemPrefab ItemPrefab
		{
			get
			{
				return ItemPrefab.Prefabs[this.ItemPrefabIdentifier];
			}
		}

		// Token: 0x17000FDA RID: 4058
		// (get) Token: 0x06003C66 RID: 15462 RVA: 0x0022AF73 File Offset: 0x00229173
		public Identifier ItemPrefabIdentifier { get; }

		// Token: 0x17000FDB RID: 4059
		// (get) Token: 0x06003C67 RID: 15463 RVA: 0x0022AF7B File Offset: 0x0022917B
		// (set) Token: 0x06003C68 RID: 15464 RVA: 0x0022AF83 File Offset: 0x00229183
		public int Quantity { get; set; }

		// Token: 0x17000FDC RID: 4060
		// (get) Token: 0x06003C69 RID: 15465 RVA: 0x0022AF8C File Offset: 0x0022918C
		// (set) Token: 0x06003C6A RID: 15466 RVA: 0x0022AF94 File Offset: 0x00229194
		public bool? IsStoreComponentEnabled { get; set; }

		// Token: 0x17000FDD RID: 4061
		// (get) Token: 0x06003C6B RID: 15467 RVA: 0x0022AF9D File Offset: 0x0022919D
		// (set) Token: 0x06003C6C RID: 15468 RVA: 0x0022AFA5 File Offset: 0x002291A5
		public bool DeliverImmediately { get; set; }

		// Token: 0x06003C6D RID: 15469 RVA: 0x0022AFB0 File Offset: 0x002291B0
		public PurchasedItem(ItemPrefab itemPrefab, int quantity, int buyerCharacterInfoId)
		{
			this.ItemPrefabIdentifier = itemPrefab.Identifier;
			this.Quantity = quantity;
			this.IsStoreComponentEnabled = null;
			this.BuyerCharacterInfoIdentifier = buyerCharacterInfoId;
		}

		// Token: 0x06003C6E RID: 15470 RVA: 0x0022AFEC File Offset: 0x002291EC
		public PurchasedItem(ItemPrefab itemPrefab, int quantity) : this(itemPrefab, quantity, null)
		{
		}

		// Token: 0x06003C6F RID: 15471 RVA: 0x0022AFF7 File Offset: 0x002291F7
		public PurchasedItem(ItemPrefab itemPrefab, int quantity, Client buyer) : this(itemPrefab.Identifier, quantity, buyer)
		{
		}

		// Token: 0x06003C70 RID: 15472 RVA: 0x0022B008 File Offset: 0x00229208
		public PurchasedItem(Identifier itemPrefabId, int quantity, Client buyer)
		{
			this.ItemPrefabIdentifier = itemPrefabId;
			this.Quantity = quantity;
			this.IsStoreComponentEnabled = null;
			int? num;
			if (buyer == null)
			{
				num = null;
			}
			else
			{
				Character character = buyer.Character;
				if (character == null)
				{
					num = null;
				}
				else
				{
					CharacterInfo info = character.Info;
					num = ((info != null) ? new int?(info.GetIdentifier()) : null);
				}
			}
			int? num2 = num;
			int valueOrDefault;
			if (num2 == null)
			{
				Character controlled = Character.Controlled;
				int? num3;
				if (controlled == null)
				{
					num3 = null;
				}
				else
				{
					CharacterInfo info2 = controlled.Info;
					num3 = ((info2 != null) ? new int?(info2.GetIdentifier()) : null);
				}
				int? num4 = num3;
				valueOrDefault = num4.GetValueOrDefault();
			}
			else
			{
				valueOrDefault = num2.GetValueOrDefault();
			}
			this.BuyerCharacterInfoIdentifier = valueOrDefault;
		}

		// Token: 0x06003C71 RID: 15473 RVA: 0x0022B0CC File Offset: 0x002292CC
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.ItemPrefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Quantity);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001F0E RID: 7950
		public readonly int BuyerCharacterInfoIdentifier;

		// Token: 0x04001F10 RID: 7952
		public bool Delivered;
	}
}
