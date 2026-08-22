using System;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x020002F7 RID: 759
	internal class AbilityConditionIsAiming : AbilityConditionDataless
	{
		// Token: 0x060031D6 RID: 12758 RVA: 0x00153188 File Offset: 0x00151388
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

		// Token: 0x060031D7 RID: 12759 RVA: 0x001531EC File Offset: 0x001513EC
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

		// Token: 0x0400188F RID: 6287
		private readonly bool hittingCountsAsAiming;

		// Token: 0x04001890 RID: 6288
		private readonly AbilityConditionIsAiming.WeaponType weapontype;

		// Token: 0x02000B93 RID: 2963
		private enum WeaponType
		{
			// Token: 0x040039D4 RID: 14804
			Any,
			// Token: 0x040039D5 RID: 14805
			Melee,
			// Token: 0x040039D6 RID: 14806
			Ranged
		}
	}
}
