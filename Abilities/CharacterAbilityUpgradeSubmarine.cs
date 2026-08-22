using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000419 RID: 1049
	[NullableContext(1)]
	[Nullable(0)]
	internal class CharacterAbilityUpgradeSubmarine : CharacterAbility
	{
		// Token: 0x17001230 RID: 4656
		// (get) Token: 0x0600471D RID: 18205 RVA: 0x0026FB60 File Offset: 0x0026DD60
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600471E RID: 18206 RVA: 0x0026FB64 File Offset: 0x0026DD64
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

		// Token: 0x0600471F RID: 18207 RVA: 0x0026FCDF File Offset: 0x0026DEDF
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x06004720 RID: 18208 RVA: 0x0026FCE7 File Offset: 0x0026DEE7
		protected override void ApplyEffect()
		{
			this.ApplyEffectSpecific();
		}

		// Token: 0x06004721 RID: 18209 RVA: 0x0026FCEF File Offset: 0x0026DEEF
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (addingFirstTime && this.giveOnAddingFirstTime)
			{
				this.ApplyEffectSpecific();
			}
		}

		// Token: 0x06004722 RID: 18210 RVA: 0x0026FD04 File Offset: 0x0026DF04
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

		// Token: 0x040024F6 RID: 9462
		[Nullable(2)]
		private readonly UpgradePrefab upgradePrefab;

		// Token: 0x040024F7 RID: 9463
		[Nullable(2)]
		private readonly UpgradeCategory upgradeCategory;

		// Token: 0x040024F8 RID: 9464
		public readonly int level;

		// Token: 0x040024F9 RID: 9465
		private readonly bool giveOnAddingFirstTime;
	}
}
