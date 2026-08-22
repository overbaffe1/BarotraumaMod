using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x0200033C RID: 828
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveReputation : CharacterAbility
	{
		// Token: 0x0600329D RID: 12957 RVA: 0x001565A0 File Offset: 0x001547A0
		public CharacterAbilityGiveReputation(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.factionIdentifier = abilityElement.GetAttributeIdentifier("identifier", Identifier.Empty);
			this.amount = abilityElement.GetAttributeFloat("amount", 0f);
			if (this.factionIdentifier.IsEmpty)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", faction identifier not defined.", null, abilityElement.ContentPackage, false, false);
			}
			if (this.amount == 0f)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", amount of reputation to give is 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x0600329E RID: 12958 RVA: 0x0015664C File Offset: 0x0015484C
		protected override void ApplyEffect()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign == null)
			{
				return;
			}
			foreach (Faction faction in campaign.Factions)
			{
				if (!(faction.Prefab.Identifier != this.factionIdentifier))
				{
					faction.Reputation.AddReputation(this.amount, float.MaxValue);
					break;
				}
			}
		}

		// Token: 0x0600329F RID: 12959 RVA: 0x001566D8 File Offset: 0x001548D8
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x040018FE RID: 6398
		private readonly Identifier factionIdentifier;

		// Token: 0x040018FF RID: 6399
		private readonly float amount;
	}
}
