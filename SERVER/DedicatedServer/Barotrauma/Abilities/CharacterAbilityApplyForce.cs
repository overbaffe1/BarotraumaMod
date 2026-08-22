using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x0200032A RID: 810
	internal class CharacterAbilityApplyForce : CharacterAbility
	{
		// Token: 0x17000E2C RID: 3628
		// (get) Token: 0x06003257 RID: 12887 RVA: 0x00154D58 File Offset: 0x00152F58
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06003258 RID: 12888 RVA: 0x00154D5C File Offset: 0x00152F5C
		public CharacterAbilityApplyForce(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
			this.force = abilityElement.GetAttributeFloat("force", 0f);
			this.maxVelocity = abilityElement.GetAttributeFloat("maxvelocity", 10f);
			this.afflictionIdentifier = abilityElement.GetAttributeString("afflictionidentifier", "");
			string[] limbTypesStr = abilityElement.GetAttributeStringArray("limbtypes", new string[0], false);
			foreach (string limbTypeStr in limbTypesStr)
			{
				LimbType limbType;
				if (Enum.TryParse<LimbType>(limbTypeStr, out limbType))
				{
					this.limbTypes.Add(limbType);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in talent \"");
					defaultInterpolatedStringHandler.AppendFormatted(characterAbilityGroup.CharacterTalent.DebugIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" - \"");
					defaultInterpolatedStringHandler.AppendFormatted(limbTypeStr);
					defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid limb type.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, abilityElement.ContentPackage, false, false);
				}
			}
		}

		// Token: 0x06003259 RID: 12889 RVA: 0x00154E60 File Offset: 0x00153060
		protected override void ApplyEffect()
		{
			float strength = 1f;
			if (!string.IsNullOrEmpty(this.afflictionIdentifier))
			{
				Affliction affliction = base.Character.CharacterHealth.GetAffliction(this.afflictionIdentifier, true);
				if (affliction == null)
				{
					return;
				}
				strength = affliction.Strength / affliction.Prefab.MaxStrength;
			}
			foreach (Limb limb in base.Character.AnimController.Limbs)
			{
				if (!limb.IsSevered && !limb.Removed && (!this.limbTypes.Any<LimbType>() || this.limbTypes.Contains(limb.type)) && base.Character.AnimController.TargetMovement.LengthSquared() >= 0.001f)
				{
					limb.body.ApplyForce(Vector2.Normalize(limb.Mass * base.Character.AnimController.TargetMovement) * this.force * strength, this.maxVelocity);
				}
			}
		}

		// Token: 0x040018C2 RID: 6338
		private readonly float force;

		// Token: 0x040018C3 RID: 6339
		private readonly float maxVelocity;

		// Token: 0x040018C4 RID: 6340
		private readonly string afflictionIdentifier;

		// Token: 0x040018C5 RID: 6341
		private readonly HashSet<LimbType> limbTypes = new HashSet<LimbType>();
	}
}
