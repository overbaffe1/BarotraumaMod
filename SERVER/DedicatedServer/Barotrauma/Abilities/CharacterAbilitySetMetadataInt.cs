using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000351 RID: 849
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilitySetMetadataInt : CharacterAbility
	{
		// Token: 0x060032DD RID: 13021 RVA: 0x00157AA4 File Offset: 0x00155CA4
		public CharacterAbilitySetMetadataInt(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.identifier = abilityElement.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.value = abilityElement.GetAttributeInt("value", 0);
			if (this.identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
				defaultInterpolatedStringHandler.AppendFormatted(base.CharacterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted("CharacterAbilitySetMetadataInt");
				defaultInterpolatedStringHandler.AppendLiteral(" - identifier is empty.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032DE RID: 13022 RVA: 0x00157B4F File Offset: 0x00155D4F
		public override void InitializeAbility(bool addingFirstTime)
		{
			this.ApplyEffect();
		}

		// Token: 0x060032DF RID: 13023 RVA: 0x00157B58 File Offset: 0x00155D58
		protected override void ApplyEffect()
		{
			if (this.identifier == Identifier.Empty)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMetadata campaignMetadata;
			if (gameSession == null)
			{
				campaignMetadata = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				campaignMetadata = ((campaign != null) ? campaign.CampaignMetadata : null);
			}
			CampaignMetadata metadata = campaignMetadata;
			if (metadata == null)
			{
				return;
			}
			metadata.SetValue(this.identifier, this.value);
		}

		// Token: 0x060032E0 RID: 13024 RVA: 0x00157BB1 File Offset: 0x00155DB1
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x0400192F RID: 6447
		private readonly Identifier identifier;

		// Token: 0x04001930 RID: 6448
		private readonly int value;
	}
}
