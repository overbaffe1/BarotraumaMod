using System;
using System.Collections.Generic;

namespace Barotrauma.Abilities
{
	// Token: 0x0200034C RID: 844
	internal class CharacterAbilityPutItem : CharacterAbility
	{
		// Token: 0x17000E3D RID: 3645
		// (get) Token: 0x060032C9 RID: 13001 RVA: 0x001574E9 File Offset: 0x001556E9
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032CA RID: 13002 RVA: 0x001574EC File Offset: 0x001556EC
		public CharacterAbilityPutItem(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.itemIdentifier = abilityElement.GetAttributeIdentifier("itemidentifier", "");
			this.amount = abilityElement.GetAttributeInt("amount", 1);
			if (this.itemIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent \"" + characterAbilityGroup.CharacterTalent.DebugIdentifier + "\" - itemIdentifier not defined.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032CB RID: 13003 RVA: 0x00157560 File Offset: 0x00155760
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

		// Token: 0x04001926 RID: 6438
		private readonly Identifier itemIdentifier;

		// Token: 0x04001927 RID: 6439
		private readonly int amount;
	}
}
