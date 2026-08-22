using System;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x020003BD RID: 957
	internal class AbilityConditionIsAiming : AbilityConditionDataless
	{
		// Token: 0x06004610 RID: 17936 RVA: 0x0026B008 File Offset: 0x00269208
		public AbilityConditionIsAiming(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.hittingCountsAsAiming = conditionElement.GetAttributeBool("hittingcountsasaiming", false);
			string attributeString = conditionElement.GetAttributeString("weapontype", "");
			if (attributeString == "melee")
			{
				this.weapontype = AbilityConditionIsAiming.WeaponType.Melee;
				return;
			}
			if (!(attributeString == "ranged"))
			{
				return;
			}
			this.weapontype = AbilityConditionIsAiming.WeaponType.Ranged;
		}

		// Token: 0x06004611 RID: 17937 RVA: 0x0026B06C File Offset: 0x0026926C
		protected override bool MatchesConditionSpecific()
		{
			HumanoidAnimController animController = this.character.AnimController as HumanoidAnimController;
			if (animController != null)
			{
				foreach (Item item in this.character.HeldItems)
				{
					AbilityConditionIsAiming.WeaponType weaponType = this.weapontype;
					if (weaponType != AbilityConditionIsAiming.WeaponType.Melee)
					{
						if (weaponType != AbilityConditionIsAiming.WeaponType.Ranged)
						{
							if (animController.IsAiming || animController.IsAimingMelee)
							{
								return true;
							}
						}
						else if (animController.IsAiming && item.GetComponent<RangedWeapon>() != null)
						{
							return true;
						}
					}
					else
					{
						MeleeWeapon meleeWeapon = item.GetComponent<MeleeWeapon>();
						if (meleeWeapon != null && (animController.IsAimingMelee || (meleeWeapon.Hitting && this.hittingCountsAsAiming)))
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x04002450 RID: 9296
		private readonly bool hittingCountsAsAiming;

		// Token: 0x04002451 RID: 9297
		private readonly AbilityConditionIsAiming.WeaponType weapontype;

		// Token: 0x020010DC RID: 4316
		private enum WeaponType
		{
			// Token: 0x040059E4 RID: 23012
			Any,
			// Token: 0x040059E5 RID: 23013
			Melee,
			// Token: 0x040059E6 RID: 23014
			Ranged
		}
	}
}
