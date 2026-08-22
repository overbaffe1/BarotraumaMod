using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000425 RID: 1061
	internal class CharacterAbilityWarStories : CharacterAbility
	{
		// Token: 0x06004742 RID: 18242 RVA: 0x002708E0 File Offset: 0x0026EAE0
		public CharacterAbilityWarStories(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.targetStat = abilityElement.GetAttributeIdentifier("target", Identifier.Empty);
			this.normalQualityThreshold = abilityElement.GetAttributeFloat("normalqualitythreshold", 4f);
			this.goodQualityThreshold = abilityElement.GetAttributeFloat("goodqualitythreshold", 10f);
			this.excellentQualityThreshold = abilityElement.GetAttributeFloat("excellentqualitythreshold", 20f);
			this.masterworkQualityThreshold = abilityElement.GetAttributeFloat("masterworkqualitythreshold", 30f);
			if (this.targetStat.IsEmpty)
			{
				DebugConsole.ThrowError("CharacterAbilityWarStories: target stat is not defined", null, abilityElement.ContentPackage, false, false);
			}
			Identifier spawnedItem = abilityElement.GetAttributeIdentifier("item", Identifier.Empty);
			if (!ItemPrefab.Prefabs.TryGet(spawnedItem, out this.prefab))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 2);
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityWarStories");
				defaultInterpolatedStringHandler.AppendLiteral(": spawned item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(spawnedItem);
				defaultInterpolatedStringHandler.AppendLiteral("\" could not be found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06004743 RID: 18243 RVA: 0x002709F4 File Offset: 0x0026EBF4
		protected override void ApplyEffect()
		{
			if (this.prefab == null || base.Character == null)
			{
				return;
			}
			CharacterInfo info = base.Character.Info;
			float statValue = (info != null) ? info.GetSavedStatValue(StatTypes.None, this.targetStat) : 0f;
			if (statValue < this.normalQualityThreshold)
			{
				return;
			}
			int quality = 0;
			if (statValue >= this.masterworkQualityThreshold)
			{
				quality = 3;
			}
			else if (statValue >= this.excellentQualityThreshold)
			{
				quality = 2;
			}
			else if (statValue >= this.goodQualityThreshold)
			{
				quality = 1;
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null || gameSession.RoundEnding)
			{
				Item item2 = new Item(this.prefab, base.Character.WorldPosition, base.Character.Submarine, 0, true)
				{
					Quality = quality
				};
				base.Character.Inventory.TryPutItem(item2, base.Character, item2.AllowedSlots, true, false, true);
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			ItemPrefab itemPrefab = this.prefab;
			Inventory inventory = base.Character.Inventory;
			int? quality2 = new int?(quality);
			spawner.AddItemToSpawnQueue(itemPrefab, inventory, null, quality2, delegate(Item item)
			{
				item.Quality = quality;
			}, true, false, InvSlotType.None);
		}

		// Token: 0x06004744 RID: 18244 RVA: 0x00270B2C File Offset: 0x0026ED2C
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x04002510 RID: 9488
		private readonly Identifier targetStat;

		// Token: 0x04002511 RID: 9489
		private readonly float normalQualityThreshold;

		// Token: 0x04002512 RID: 9490
		private readonly float goodQualityThreshold;

		// Token: 0x04002513 RID: 9491
		private readonly float excellentQualityThreshold;

		// Token: 0x04002514 RID: 9492
		private readonly float masterworkQualityThreshold;

		// Token: 0x04002515 RID: 9493
		private readonly ItemPrefab prefab;
	}
}
