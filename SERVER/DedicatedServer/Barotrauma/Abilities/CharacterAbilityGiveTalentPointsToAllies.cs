using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Abilities
{
	// Token: 0x02000340 RID: 832
	internal sealed class CharacterAbilityGiveTalentPointsToAllies : CharacterAbility
	{
		// Token: 0x060032A6 RID: 12966 RVA: 0x001568B8 File Offset: 0x00154AB8
		[NullableContext(1)]
		public CharacterAbilityGiveTalentPointsToAllies(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.amount = abilityElement.GetAttributeInt("amount", 0);
			if (this.amount == 0)
			{
				DebugConsole.ThrowError("Error in talent " + base.CharacterTalent.DebugIdentifier + ", amount of talent points to give is 0.", null, abilityElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060032A7 RID: 12967 RVA: 0x00156910 File Offset: 0x00154B10
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

		// Token: 0x04001905 RID: 6405
		private readonly int amount;
	}
}
