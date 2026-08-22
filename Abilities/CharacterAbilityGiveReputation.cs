using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000402 RID: 1026
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityGiveReputation : CharacterAbility
	{
		// Token: 0x060046D7 RID: 18135 RVA: 0x0026E420 File Offset: 0x0026C620
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

		// Token: 0x060046D8 RID: 18136 RVA: 0x0026E4CC File Offset: 0x0026C6CC
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

		// Token: 0x060046D9 RID: 18137 RVA: 0x0026E558 File Offset: 0x0026C758
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x040024BF RID: 9407
		private readonly Identifier factionIdentifier;

		// Token: 0x040024C0 RID: 9408
		private readonly float amount;
	}
}
