using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Abilities
{
	// Token: 0x020003F0 RID: 1008
	internal class CharacterAbilityApplyForce : CharacterAbility
	{
		// Token: 0x1700121C RID: 4636
		// (get) Token: 0x06004691 RID: 18065 RVA: 0x0026CBD8 File Offset: 0x0026ADD8
		public override bool AppliesEffectOnIntervalUpdate
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004692 RID: 18066 RVA: 0x0026CBDC File Offset: 0x0026ADDC
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

		// Token: 0x06004693 RID: 18067 RVA: 0x0026CCE0 File Offset: 0x0026AEE0
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

		// Token: 0x04002483 RID: 9347
		private readonly float force;

		// Token: 0x04002484 RID: 9348
		private readonly float maxVelocity;

		// Token: 0x04002485 RID: 9349
		private readonly string afflictionIdentifier;

		// Token: 0x04002486 RID: 9350
		private readonly HashSet<LimbType> limbTypes = new HashSet<LimbType>();
	}
}
