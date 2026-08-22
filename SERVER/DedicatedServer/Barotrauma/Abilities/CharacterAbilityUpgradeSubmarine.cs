using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000353 RID: 851
	[NullableContext(1)]
	[Nullable(0)]
	internal class CharacterAbilityUpgradeSubmarine : CharacterAbility
	{
		// Token: 0x17000E40 RID: 3648
		// (get) Token: 0x060032E3 RID: 13027 RVA: 0x00157CE0 File Offset: 0x00155EE0
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032E4 RID: 13028 RVA: 0x00157CE4 File Offset: 0x00155EE4
		public CharacterAbilityUpgradeSubmarine(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			Identifier prefabIdentifier = abilityElement.GetAttributeIdentifier("upgradePrefab", Identifier.Empty);
			Identifier categoryIdentifier = abilityElement.GetAttributeIdentifier("upgradeCategory", Identifier.Empty);
			this.giveOnAddingFirstTime = abilityElement.GetAttributeBool("giveonaddingfirsttime", characterAbilityGroup.AbilityEffectType == AbilityEffectType.None);
			UpgradePrefab foundUpgradePrefab = UpgradePrefab.Find(prefabIdentifier);
			if (foundUpgradePrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilityUpgradeSubmarine");
				defaultInterpolatedStringHandler.AppendLiteral(" - ");
				defaultInterpolatedStringHandler.AppendFormatted("upgradePrefab");
				defaultInterpolatedStringHandler.AppendLiteral(" not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
			else
			{
				this.upgradePrefab = foundUpgradePrefab;
			}
			UpgradeCategory foundUpgradeCategory = UpgradeCategory.Find(categoryIdentifier);
			if (foundUpgradeCategory == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(32, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler2.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(", ");
				defaultInterpolatedStringHandler2.AppendFormatted("CharacterAbilityUpgradeSubmarine");
				defaultInterpolatedStringHandler2.AppendLiteral(" - ");
				defaultInterpolatedStringHandler2.AppendFormatted("upgradeCategory");
				defaultInterpolatedStringHandler2.AppendLiteral(" not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
			else
			{
				this.upgradeCategory = foundUpgradeCategory;
			}
			this.level = abilityElement.GetAttributeInt("level", 1);
		}

		// Token: 0x060032E5 RID: 13029 RVA: 0x00157E5F File Offset: 0x0015605F
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x060032E6 RID: 13030 RVA: 0x00157E67 File Offset: 0x00156067
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x060032E7 RID: 13031 RVA: 0x00157E6F File Offset: 0x0015606F
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime && this.giveOnAddingFirstTime)
			{
				this.ApplyEffectSpecific();
			}
		}

		// Token: 0x060032E8 RID: 13032 RVA: 0x00157E84 File Offset: 0x00156084
		private void ApplyEffectSpecific()
		{
			if (this.upgradePrefab == null || this.upgradeCategory == null)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			UpgradeManager upgradeManager2;
			if (gameSession == null)
			{
				upgradeManager2 = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				upgradeManager2 = ((campaign != null) ? campaign.UpgradeManager : null);
			}
			UpgradeManager upgradeManager = upgradeManager2;
			if (upgradeManager == null)
			{
				return;
			}
			upgradeManager.AddUpgradeExternally(this.upgradePrefab, this.upgradeCategory, this.level);
		}

		// Token: 0x04001935 RID: 6453
		[Nullable(2)]
		private readonly UpgradePrefab upgradePrefab;

		// Token: 0x04001936 RID: 6454
		[Nullable(2)]
		private readonly UpgradeCategory upgradeCategory;

		// Token: 0x04001937 RID: 6455
		public readonly int level;

		// Token: 0x04001938 RID: 6456
		private readonly bool giveOnAddingFirstTime;
	}
}
