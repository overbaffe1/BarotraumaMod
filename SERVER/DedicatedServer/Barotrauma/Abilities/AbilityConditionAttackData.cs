using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma.Abilities
{
	// Token: 0x020002EE RID: 750
	internal class AbilityConditionAttackData : AbilityConditionData
	{
		// Token: 0x060031BC RID: 12732 RVA: 0x00152730 File Offset: 0x00150930
		public AbilityConditionAttackData(CharacterTalent characterTalent, ContentXElement conditionElement) : base(characterTalent, conditionElement)
		{
			this.itemIdentifier = conditionElement.GetAttributeString("itemIdentifier", string.Empty);
			this.tags = conditionElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
			this.ignoreNonHarmfulAttacks = conditionElement.GetAttributeBool("ignoreNonHarmfulAttacks", false);
			this.ignoreOwnAttacks = conditionElement.GetAttributeBool("ignoreOwnAttacks", false);
			string weaponTypeStr = conditionElement.GetAttributeString("weapontype", "Any");
			if (!Enum.TryParse<AbilityConditionAttackData.WeaponType>(weaponTypeStr, true, out this.weapontype))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent \"");
				defaultInterpolatedStringHandler.AppendFormatted(characterTalent.DebugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": \"");
				defaultInterpolatedStringHandler.AppendFormatted(weaponTypeStr);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid weapon type.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, conditionElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060031BD RID: 12733 RVA: 0x00152810 File Offset: 0x00150A10
		protected override bool MatchesConditionSpecific(AbilityObject abilityObject)
		{
			AbilityAttackData attackData = abilityObject as AbilityAttackData;
			if (attackData == null)
			{
				base.LogAbilityConditionError(abilityObject, typeof(AbilityAttackData));
				return false;
			}
			AbilityConditionAttackData.<>c__DisplayClass8_0 CS$<>8__locals1 = new AbilityConditionAttackData.<>c__DisplayClass8_0();
			if (this.ignoreOwnAttacks && attackData.Attacker == this.character)
			{
				return false;
			}
			if (this.ignoreNonHarmfulAttacks && attackData.SourceAttack != null && attackData.SourceAttack.Stun <= 0f)
			{
				Dictionary<Affliction, XElement> afflictions = attackData.SourceAttack.Afflictions;
				bool flag;
				if (afflictions == null)
				{
					flag = true;
				}
				else
				{
					flag = afflictions.All((KeyValuePair<Affliction, XElement> a) => a.Key.Prefab.IsBuff);
				}
				if (flag)
				{
					return false;
				}
			}
			AbilityConditionAttackData.<>c__DisplayClass8_0 CS$<>8__locals2 = CS$<>8__locals1;
			Item item;
			if (attackData == null)
			{
				item = null;
			}
			else
			{
				Attack sourceAttack = attackData.SourceAttack;
				item = ((sourceAttack != null) ? sourceAttack.SourceItem : null);
			}
			CS$<>8__locals2.item = item;
			if (!string.IsNullOrEmpty(this.itemIdentifier))
			{
				Item item2 = CS$<>8__locals1.item;
				Identifier? identifier;
				Identifier? identifier2;
				if (item2 == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					identifier2 = new Identifier?(item2.Prefab.Identifier);
				}
				identifier = identifier2;
				if (identifier != this.itemIdentifier)
				{
					return false;
				}
			}
			if (this.tags.Any<Identifier>() && !this.tags.All(delegate(Identifier t)
			{
				Item item9 = CS$<>8__locals1.item;
				return item9 != null && item9.HasTag(t);
			}))
			{
				return false;
			}
			if (this.weapontype != AbilityConditionAttackData.WeaponType.Any)
			{
				foreach (AbilityConditionAttackData.WeaponType wt in AbilityConditionAttackData.WeaponTypeValues)
				{
					if (wt != AbilityConditionAttackData.WeaponType.Any && this.weapontype.HasFlag(wt))
					{
						switch (wt)
						{
						case AbilityConditionAttackData.WeaponType.Melee:
						{
							Item item3 = CS$<>8__locals1.item;
							Projectile projectile3 = (item3 != null) ? item3.GetComponent<Projectile>() : null;
							if (projectile3 == null || !projectile3.IsActive)
							{
								Item item4 = CS$<>8__locals1.item;
								if (((item4 != null) ? item4.GetComponent<MeleeWeapon>() : null) != null)
								{
									return true;
								}
							}
							break;
						}
						case AbilityConditionAttackData.WeaponType.Ranged:
						{
							Item item5 = CS$<>8__locals1.item;
							MeleeWeapon meleeWeapon = (item5 != null) ? item5.GetComponent<MeleeWeapon>() : null;
							if (meleeWeapon == null || !meleeWeapon.Hitting)
							{
								Item item6 = CS$<>8__locals1.item;
								if (((item6 != null) ? item6.GetComponent<Projectile>() : null) != null)
								{
									return true;
								}
							}
							break;
						}
						case AbilityConditionAttackData.WeaponType.Melee | AbilityConditionAttackData.WeaponType.Ranged:
							break;
						case AbilityConditionAttackData.WeaponType.HandheldRanged:
						{
							Item item7 = CS$<>8__locals1.item;
							Projectile projectile = (item7 != null) ? item7.GetComponent<Projectile>() : null;
							bool flag2;
							if (projectile == null)
							{
								flag2 = (null != null);
							}
							else
							{
								Item launcher = projectile.Launcher;
								flag2 = (((launcher != null) ? launcher.GetComponent<Holdable>() : null) != null);
							}
							if (flag2)
							{
								return true;
							}
							break;
						}
						default:
							if (wt != AbilityConditionAttackData.WeaponType.Turret)
							{
								if (wt == AbilityConditionAttackData.WeaponType.NoWeapon)
								{
									if (CS$<>8__locals1.item == null)
									{
										return true;
									}
								}
							}
							else
							{
								Item item8 = CS$<>8__locals1.item;
								Projectile projectile2 = (item8 != null) ? item8.GetComponent<Projectile>() : null;
								bool flag3;
								if (projectile2 == null)
								{
									flag3 = (null != null);
								}
								else
								{
									Item launcher2 = projectile2.Launcher;
									flag3 = (((launcher2 != null) ? launcher2.GetComponent<Turret>() : null) != null);
								}
								if (flag3)
								{
									return true;
								}
							}
							break;
						}
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x04001882 RID: 6274
		private static readonly List<AbilityConditionAttackData.WeaponType> WeaponTypeValues = Enum.GetValues(typeof(AbilityConditionAttackData.WeaponType)).Cast<AbilityConditionAttackData.WeaponType>().ToList<AbilityConditionAttackData.WeaponType>();

		// Token: 0x04001883 RID: 6275
		private readonly string itemIdentifier;

		// Token: 0x04001884 RID: 6276
		private readonly Identifier[] tags;

		// Token: 0x04001885 RID: 6277
		private readonly AbilityConditionAttackData.WeaponType weapontype;

		// Token: 0x04001886 RID: 6278
		private readonly bool ignoreNonHarmfulAttacks;

		// Token: 0x04001887 RID: 6279
		private readonly bool ignoreOwnAttacks;

		// Token: 0x02000B8E RID: 2958
		[Flags]
		private enum WeaponType
		{
			// Token: 0x040039C7 RID: 14791
			Any = 0,
			// Token: 0x040039C8 RID: 14792
			Melee = 1,
			// Token: 0x040039C9 RID: 14793
			Ranged = 2,
			// Token: 0x040039CA RID: 14794
			HandheldRanged = 4,
			// Token: 0x040039CB RID: 14795
			Turret = 8,
			// Token: 0x040039CC RID: 14796
			NoWeapon = 16
		}
	}
}
