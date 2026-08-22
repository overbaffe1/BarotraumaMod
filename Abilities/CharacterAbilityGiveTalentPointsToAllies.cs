using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000406 RID: 1030
	internal sealed class CharacterAbilityGiveTalentPointsToAllies : CharacterAbility
	{
		// Token: 0x060046E0 RID: 18144 RVA: 0x0026E738 File Offset: 0x0026C938
		[NullableContext(1)]
		public CharacterAbilityGiveTalentPointsToAllies(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", amount of talent points to give is 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060046E1 RID: 18145 RVA: 0x0026E790 File Offset: 0x0026C990
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (!addingFirstTime)
			{
				return;
			}
			foreach (Character character in Character.GetFriendlyCrew(base.Character))
			{
				character.Info.AdditionalTalentPoints += this.amount;
			}
		}

		// Token: 0x040024C6 RID: 9414
		private readonly int amount;
	}
}
