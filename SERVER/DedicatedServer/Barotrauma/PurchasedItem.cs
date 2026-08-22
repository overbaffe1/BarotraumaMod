using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001D1 RID: 465
	internal class PurchasedItem
	{
		// Token: 0x170009D0 RID: 2512
		// (get) Token: 0x06002262 RID: 8802 RVA: 0x000E7C15 File Offset: 0x000E5E15
		public ItemPrefab ItemPrefab
		{
			get
			{
				return ItemPrefab.Prefabs[this.ItemPrefabIdentifier];
			}
		}

		// Token: 0x170009D1 RID: 2513
		// (get) Token: 0x06002263 RID: 8803 RVA: 0x000E7C27 File Offset: 0x000E5E27
		public Identifier ItemPrefabIdentifier { get; }

		// Token: 0x170009D2 RID: 2514
		// (get) Token: 0x06002264 RID: 8804 RVA: 0x000E7C2F File Offset: 0x000E5E2F
		// (set) Token: 0x06002265 RID: 8805 RVA: 0x000E7C37 File Offset: 0x000E5E37
		public int Quantity { get; set; }

		// Token: 0x170009D3 RID: 2515
		// (get) Token: 0x06002266 RID: 8806 RVA: 0x000E7C40 File Offset: 0x000E5E40
		// (set) Token: 0x06002267 RID: 8807 RVA: 0x000E7C48 File Offset: 0x000E5E48
		public bool? IsStoreComponentEnabled { get; set; }

		// Token: 0x170009D4 RID: 2516
		// (get) Token: 0x06002268 RID: 8808 RVA: 0x000E7C51 File Offset: 0x000E5E51
		// (set) Token: 0x06002269 RID: 8809 RVA: 0x000E7C59 File Offset: 0x000E5E59
		public bool DeliverImmediately { get; set; }

		// Token: 0x0600226A RID: 8810 RVA: 0x000E7C64 File Offset: 0x000E5E64
		public PurchasedItem(ItemPrefab itemPrefab, int quantity, int buyerCharacterInfoId)
		{
			this.ItemPrefabIdentifier = itemPrefab.Identifier;
			this.Quantity = quantity;
			this.IsStoreComponentEnabled = null;
			this.BuyerCharacterInfoIdentifier = buyerCharacterInfoId;
		}

		// Token: 0x0600226B RID: 8811 RVA: 0x000E7CA0 File Offset: 0x000E5EA0
		public PurchasedItem(ItemPrefab itemPrefab, int quantity, Client buyer) : this(itemPrefab.Identifier, quantity, buyer)
		{
		}

		// Token: 0x0600226C RID: 8812 RVA: 0x000E7CB0 File Offset: 0x000E5EB0
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

		// Token: 0x0600226D RID: 8813 RVA: 0x000E7D74 File Offset: 0x000E5F74
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.ItemPrefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Quantity);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001074 RID: 4212
		public readonly int BuyerCharacterInfoIdentifier;

		// Token: 0x04001076 RID: 4214
		public bool Delivered;
	}
}
