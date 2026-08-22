using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x02000412 RID: 1042
	internal class CharacterAbilityPutItem : CharacterAbility
	{
		// Token: 0x1700122D RID: 4653
		// (get) Token: 0x06004703 RID: 18179 RVA: 0x0026F369 File Offset: 0x0026D569
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004704 RID: 18180 RVA: 0x0026F36C File Offset: 0x0026D56C
		public CharacterAbilityPutItem(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.itemIdentifier = abilityElement.GetAttributeIdentifier("itemidentifier", "");
			this.amount = abilityElement.GetAttributeInt("amount", 1);
			if (this.itemIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent \"" + characterAbilityGroup.CharacterTalent.DebugIdentifier + "\" - itemIdentifier not defined.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06004705 RID: 18181 RVA: 0x0026F3E0 File Offset: 0x0026D5E0
		protected override void ApplyEffect()
		{
			if (this.itemIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Cannot put item in inventory - itemIdentifier not defined.", null, base.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			ItemPrefab itemPrefab = ItemPrefab.Find(null, this.itemIdentifier);
			if (itemPrefab == null)
			{
				DebugConsole.ThrowError("Cannot put item in inventory - item prefab " + this.itemIdentifier.ToString() + " not found.", null, base.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			int i = 0;
			while (i < this.amount)
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null && !gameSession.RoundEnding)
				{
					goto IL_12B;
				}
				Item item = new Item(itemPrefab, base.Character.WorldPosition, base.Character.Submarine, 0, true);
				if (!base.Character.Inventory.TryPutItem(item, base.Character, item.AllowedSlots, true, false, true))
				{
					using (IEnumerator<Item> enumerator = base.Character.Inventory.AllItemsMod.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Item containedItem = enumerator.Current;
							ItemInventory ownInventory = containedItem.OwnInventory;
							if (ownInventory != null && ownInventory.TryPutItem(item, base.Character, null, true, false, true))
							{
								break;
							}
						}
						goto IL_159;
					}
					goto IL_12B;
				}
				IL_159:
				i++;
				continue;
				IL_12B:
				Entity.Spawner.AddItemToSpawnQueue(itemPrefab, base.Character.Inventory, null, null, null, true, false, InvSlotType.None);
				goto IL_159;
			}
		}

		// Token: 0x040024E7 RID: 9447
		private readonly Identifier itemIdentifier;

		// Token: 0x040024E8 RID: 9448
		private readonly int amount;
	}
}
