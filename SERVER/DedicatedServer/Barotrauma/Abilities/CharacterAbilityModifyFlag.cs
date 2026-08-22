using System;

namespace Barotrauma.Abilities
{
	// Token: 0x02000345 RID: 837
	internal class CharacterAbilityModifyFlag : CharacterAbility
	{
		// Token: 0x17000E36 RID: 3638
		// (get) Token: 0x060032B3 RID: 12979 RVA: 0x00156DC3 File Offset: 0x00154FC3
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060032B4 RID: 12980 RVA: 0x00156DC6 File Offset: 0x00154FC6
		public CharacterAbilityModifyFlag(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.abilityFlag = CharacterAbilityGroup.ParseFlagType(abilityElement.GetAttributeString("flagtype", ""), base.CharacterTalent.DebugIdentifier);
		}

		// Token: 0x060032B5 RID: 12981 RVA: 0x00156DF6 File Offset: 0x00154FF6
		protected override void VerifyState(bool conditionsMatched, float timeSinceLastUpdate)
		{
			if (conditionsMatched != this.lastState)
			{
				if (conditionsMatched)
				{
					base.Character.AddAbilityFlag(this.abilityFlag);
				}
				else
				{
					base.Character.RemoveAbilityFlag(this.abilityFlag);
				}
				this.lastState = conditionsMatched;
			}
		}

		// Token: 0x04001910 RID: 6416
		private readonly AbilityFlags abilityFlag;

		// Token: 0x04001911 RID: 6417
		private bool lastState;
	}
}
