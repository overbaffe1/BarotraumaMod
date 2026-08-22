using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000417 RID: 1047
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilitySetMetadataInt : CharacterAbility
	{
		// Token: 0x06004717 RID: 18199 RVA: 0x0026F924 File Offset: 0x0026DB24
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

		// Token: 0x06004718 RID: 18200 RVA: 0x0026F9CF File Offset: 0x0026DBCF
		public override void InitializeAbility(bool addingFirstTime)
		{
			this.ApplyEffect();
		}

		// Token: 0x06004719 RID: 18201 RVA: 0x0026F9D8 File Offset: 0x0026DBD8
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

		// Token: 0x0600471A RID: 18202 RVA: 0x0026FA31 File Offset: 0x0026DC31
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x040024F0 RID: 9456
		private readonly Identifier identifier;

		// Token: 0x040024F1 RID: 9457
		private readonly int value;
	}
}
