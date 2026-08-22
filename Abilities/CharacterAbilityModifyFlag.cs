using System;

namespace Barotrauma.Abilities
{
	// Token: 0x0200040B RID: 1035
	internal class CharacterAbilityModifyFlag : CharacterAbility
	{
		// Token: 0x17001226 RID: 4646
		// (get) Token: 0x060046ED RID: 18157 RVA: 0x0026EC43 File Offset: 0x0026CE43
		public override bool AllowClientSimulation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060046EE RID: 18158 RVA: 0x0026EC46 File Offset: 0x0026CE46
		public CharacterAbilityModifyFlag(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.abilityFlag = CharacterAbilityGroup.ParseFlagType(abilityElement.GetAttributeString("flagtype", ""), base.CharacterTalent.DebugIdentifier);
		}

		// Token: 0x060046EF RID: 18159 RVA: 0x0026EC76 File Offset: 0x0026CE76
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

		// Token: 0x040024D1 RID: 9425
		private readonly AbilityFlags abilityFlag;

		// Token: 0x040024D2 RID: 9426
		private bool lastState;
	}
}
