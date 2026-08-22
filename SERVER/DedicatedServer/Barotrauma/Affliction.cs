using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C3 RID: 195
	internal class Affliction : ISerializableEntity
	{
		// Token: 0x1700065D RID: 1629
		// (get) Token: 0x060015DC RID: 5596 RVA: 0x000B9CB1 File Offset: 0x000B7EB1
		public string Name
		{
			get
			{
				return this.ToString();
			}
		}

		// Token: 0x1700065E RID: 1630
		// (get) Token: 0x060015DD RID: 5597 RVA: 0x000B9CB9 File Offset: 0x000B7EB9
		// (set) Token: 0x060015DE RID: 5598 RVA: 0x000B9CC1 File Offset: 0x000B7EC1
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x1700065F RID: 1631
		// (get) Token: 0x060015DF RID: 5599 RVA: 0x000B9CCA File Offset: 0x000B7ECA
		// (set) Token: 0x060015E0 RID: 5600 RVA: 0x000B9CD2 File Offset: 0x000B7ED2
		public float PendingGrainEffectStrength { get; set; }

		// Token: 0x17000660 RID: 1632
		// (get) Token: 0x060015E1 RID: 5601 RVA: 0x000B9CDB File Offset: 0x000B7EDB
		// (set) Token: 0x060015E2 RID: 5602 RVA: 0x000B9CE3 File Offset: 0x000B7EE3
		public float GrainEffectStrength { get; set; }

		// Token: 0x17000661 RID: 1633
		// (get) Token: 0x060015E3 RID: 5603 RVA: 0x000B9CEC File Offset: 0x000B7EEC
		// (set) Token: 0x060015E4 RID: 5604 RVA: 0x000B9CF4 File Offset: 0x000B7EF4
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public virtual float Strength
		{
			get
			{
				return this._strength;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				if (this._nonClampedStrength < 0f && value > 0f)
				{
					this._nonClampedStrength = value;
				}
				float newValue = MathHelper.Clamp(value, 0f, this.Prefab.MaxStrength);
				if (newValue > this._strength)
				{
					this.PendingGrainEffectStrength = this.Prefab.GrainBurst;
					this.Duration = this.Prefab.Duration;
				}
				this._strength = newValue;
				this.activeEffectDirty = true;
			}
		}

		// Token: 0x17000662 RID: 1634
		// (get) Token: 0x060015E5 RID: 5605 RVA: 0x000B9D76 File Offset: 0x000B7F76
		// (set) Token: 0x060015E6 RID: 5606 RVA: 0x000B9D7E File Offset: 0x000B7F7E
		public float Penetration { get; set; }

		// Token: 0x17000663 RID: 1635
		// (get) Token: 0x060015E7 RID: 5607 RVA: 0x000B9D87 File Offset: 0x000B7F87
		public float NonClampedStrength
		{
			get
			{
				if (this._nonClampedStrength <= 0f)
				{
					return this._strength;
				}
				return this._nonClampedStrength;
			}
		}

		// Token: 0x17000664 RID: 1636
		// (get) Token: 0x060015E8 RID: 5608 RVA: 0x000B9DA3 File Offset: 0x000B7FA3
		// (set) Token: 0x060015E9 RID: 5609 RVA: 0x000B9DAB File Offset: 0x000B7FAB
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Identifier Identifier { get; private set; }

		// Token: 0x17000665 RID: 1637
		// (get) Token: 0x060015EA RID: 5610 RVA: 0x000B9DB4 File Offset: 0x000B7FB4
		// (set) Token: 0x060015EB RID: 5611 RVA: 0x000B9DBC File Offset: 0x000B7FBC
		[Serialize(1f, IsPropertySaveable.Yes, "The probability for the affliction to be applied.", "", false)]
		[Editable(0f, 1f, 1)]
		public float Probability { get; set; } = 1f;

		// Token: 0x17000666 RID: 1638
		// (get) Token: 0x060015EC RID: 5612 RVA: 0x000B9DC5 File Offset: 0x000B7FC5
		// (set) Token: 0x060015ED RID: 5613 RVA: 0x000B9DCD File Offset: 0x000B7FCD
		[Serialize(true, IsPropertySaveable.Yes, "Explosion damage is applied per each affected limb. Should this affliction damage be divided by the count of affected limbs (1-15) or applied in full? Default: true. Only affects status effects and explosions.", "", false)]
		[Editable]
		public bool DivideByLimbCount { get; set; }

		// Token: 0x17000667 RID: 1639
		// (get) Token: 0x060015EE RID: 5614 RVA: 0x000B9DD6 File Offset: 0x000B7FD6
		// (set) Token: 0x060015EF RID: 5615 RVA: 0x000B9DDE File Offset: 0x000B7FDE
		[Serialize(false, IsPropertySaveable.Yes, "Is the damage relative to the max vitality (percentage) or absolute (normal)", "", false)]
		[Editable]
		public bool MultiplyByMaxVitality { get; set; }

		// Token: 0x17000668 RID: 1640
		// (get) Token: 0x060015F0 RID: 5616 RVA: 0x000B9DE7 File Offset: 0x000B7FE7
		public bool AffectedByAttackMultipliers
		{
			get
			{
				return this.Prefab.AffectedByAttackMultipliers;
			}
		}

		// Token: 0x060015F1 RID: 5617 RVA: 0x000B9DF4 File Offset: 0x000B7FF4
		public Affliction(AfflictionPrefab prefab, float strength)
		{
			this.Prefab = prefab;
			this.PendingGrainEffectStrength = this.Prefab.GrainBurst;
			this._strength = strength;
			this.Identifier = prefab.Identifier;
			this.Duration = prefab.Duration;
			foreach (AfflictionPrefab.PeriodicEffect periodicEffect in prefab.PeriodicEffects)
			{
				this.PeriodicEffectTimers[periodicEffect] = Rand.Range(periodicEffect.MinInterval, periodicEffect.MaxInterval, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x060015F2 RID: 5618 RVA: 0x000B9EDC File Offset: 0x000B80DC
		public void CopyProperties(Affliction source)
		{
			this.Probability = source.Probability;
			this.DivideByLimbCount = source.DivideByLimbCount;
			this.MultiplyByMaxVitality = source.MultiplyByMaxVitality;
			this.Penetration = source.Penetration;
		}

		// Token: 0x060015F3 RID: 5619 RVA: 0x000B9F0E File Offset: 0x000B810E
		public void Serialize(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
		}

		// Token: 0x060015F4 RID: 5620 RVA: 0x000B9F1C File Offset: 0x000B811C
		public void Deserialize(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element.GetAttribute("amount", StringComparison.OrdinalIgnoreCase) != null && element.GetAttribute("strength", StringComparison.OrdinalIgnoreCase) == null)
			{
				this.Strength = element.GetAttributeFloat("amount", 0f);
			}
		}

		// Token: 0x060015F5 RID: 5621 RVA: 0x000B9F68 File Offset: 0x000B8168
		public Affliction CreateMultiplied(float multiplier, Affliction affliction)
		{
			Affliction instance = this.Prefab.Instantiate(this.NonClampedStrength * multiplier, this.Source);
			instance.CopyProperties(affliction);
			return instance;
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x000B9F98 File Offset: 0x000B8198
		public override string ToString()
		{
			if (this.Prefab != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Affliction (");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return "Affliction (Invalid)";
		}

		// Token: 0x060015F7 RID: 5623 RVA: 0x000B9FEE File Offset: 0x000B81EE
		public LocalizedString GetStrengthText()
		{
			return Affliction.GetStrengthText(this.Strength, this.Prefab.MaxStrength);
		}

		// Token: 0x060015F8 RID: 5624 RVA: 0x000BA006 File Offset: 0x000B8206
		public static LocalizedString GetStrengthText(float strength, float maxStrength)
		{
			return Affliction.strengthTexts[MathHelper.Clamp((int)Math.Floor((double)(strength / maxStrength * (float)Affliction.strengthTexts.Length)), 0, Affliction.strengthTexts.Length - 1)];
		}

		// Token: 0x060015F9 RID: 5625 RVA: 0x000BA030 File Offset: 0x000B8230
		public AfflictionPrefab.Effect GetActiveEffect()
		{
			if (this.activeEffectDirty)
			{
				this.activeEffect = this.Prefab.GetActiveEffect(this._strength);
				this.prevActiveEffectStrength = this._strength;
				this.activeEffectDirty = false;
			}
			return this.activeEffect;
		}

		// Token: 0x060015FA RID: 5626 RVA: 0x000BA06A File Offset: 0x000B826A
		public float GetVitalityDecrease(CharacterHealth characterHealth)
		{
			return this.GetVitalityDecrease(characterHealth, this.Strength);
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x000BA07C File Offset: 0x000B827C
		public float GetVitalityDecrease(CharacterHealth characterHealth, float strength)
		{
			if (strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			strength = MathHelper.Clamp(strength, 0f, this.Prefab.MaxStrength);
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxStrength - currentEffect.MinStrength <= 0f)
			{
				return 0f;
			}
			float currVitalityDecrease = MathHelper.Lerp(currentEffect.MinVitalityDecrease, currentEffect.MaxVitalityDecrease, currentEffect.GetStrengthFactor(strength));
			if (currentEffect.MultiplyByMaxVitality)
			{
				currVitalityDecrease *= ((characterHealth != null) ? characterHealth.MaxVitality : 100f);
			}
			return currVitalityDecrease;
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x000BA114 File Offset: 0x000B8314
		public float GetScreenGrainStrength()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (MathUtils.NearlyEqual(currentEffect.MaxGrainStrength, 0f, 0.0001f))
			{
				return 0f;
			}
			float amount = MathHelper.Lerp(currentEffect.MinGrainStrength, currentEffect.MaxGrainStrength, currentEffect.GetStrengthFactor(this)) * this.GetScreenEffectFluctuation(currentEffect);
			if (this.Prefab.GrainBurst > 0f && this.GrainEffectStrength > amount)
			{
				return Math.Min(this.GrainEffectStrength, 1f);
			}
			return amount;
		}

		// Token: 0x060015FD RID: 5629 RVA: 0x000BA1B8 File Offset: 0x000B83B8
		public float GetScreenDistortStrength()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxScreenDistort - currentEffect.MinScreenDistort < 0f)
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinScreenDistort, currentEffect.MaxScreenDistort, currentEffect.GetStrengthFactor(this)) * this.GetScreenEffectFluctuation(currentEffect);
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x000BA228 File Offset: 0x000B8428
		public float GetRadialDistortStrength()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxRadialDistort - currentEffect.MinRadialDistort < 0f)
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinRadialDistort, currentEffect.MaxRadialDistort, currentEffect.GetStrengthFactor(this)) * this.GetScreenEffectFluctuation(currentEffect);
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x000BA298 File Offset: 0x000B8498
		public float GetChromaticAberrationStrength()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxChromaticAberration - currentEffect.MinChromaticAberration < 0f)
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinChromaticAberration, currentEffect.MaxChromaticAberration, currentEffect.GetStrengthFactor(this)) * this.GetScreenEffectFluctuation(currentEffect);
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x000BA308 File Offset: 0x000B8508
		public float GetAfflictionOverlayMultiplier()
		{
			if (this.Prefab.AfflictionOverlayAlphaIsLinear)
			{
				return this.Strength / this.Prefab.MaxStrength;
			}
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxAfflictionOverlayAlphaMultiplier - currentEffect.MinAfflictionOverlayAlphaMultiplier < 0f)
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinAfflictionOverlayAlphaMultiplier, currentEffect.MaxAfflictionOverlayAlphaMultiplier, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x000BA390 File Offset: 0x000B8590
		public Color GetFaceTint()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return Color.TransparentBlack;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return Color.TransparentBlack;
			}
			return Color.Lerp(currentEffect.MinFaceTint, currentEffect.MaxFaceTint, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001602 RID: 5634 RVA: 0x000BA3E0 File Offset: 0x000B85E0
		public Color GetBodyTint()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return Color.TransparentBlack;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return Color.TransparentBlack;
			}
			return Color.Lerp(currentEffect.MinBodyTint, currentEffect.MaxBodyTint, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x000BA430 File Offset: 0x000B8630
		public float GetScreenBlurStrength()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (currentEffect.MaxScreenBlur - currentEffect.MinScreenBlur < 0f)
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinScreenBlur, currentEffect.MaxScreenBlur, currentEffect.GetStrengthFactor(this)) * this.GetScreenEffectFluctuation(currentEffect);
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x000BA4A0 File Offset: 0x000B86A0
		private float GetScreenEffectFluctuation(AfflictionPrefab.Effect currentEffect)
		{
			if (currentEffect == null || currentEffect.ScreenEffectFluctuationFrequency <= 0f)
			{
				return 1f;
			}
			return ((float)Math.Sin((double)(this.fluctuationTimer * 6.2831855f)) + 1f) * 0.5f;
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x000BA4D8 File Offset: 0x000B86D8
		public float GetSkillMultiplier()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 1f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 1f;
			}
			return MathHelper.Lerp(currentEffect.MinSkillMultiplier, currentEffect.MaxSkillMultiplier, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x000BA528 File Offset: 0x000B8728
		public void CalculateDamagePerSecond(float currentVitalityDecrease)
		{
			this.DamagePerSecond = Math.Max(this.DamagePerSecond, currentVitalityDecrease - this.PreviousVitalityDecrease);
			if (this.DamagePerSecondTimer >= 1f)
			{
				this.DamagePerSecond = currentVitalityDecrease - this.PreviousVitalityDecrease;
				this.PreviousVitalityDecrease = currentVitalityDecrease;
				this.DamagePerSecondTimer = 0f;
			}
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x000BA57C File Offset: 0x000B877C
		public float GetResistance(Identifier afflictionId, LimbType limbType)
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 0f;
			}
			AfflictionPrefab affliction = AfflictionPrefab.Prefabs[afflictionId];
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			if (!currentEffect.ResistanceFor.Any((Identifier identifier) => identifier == affliction.Identifier || identifier == affliction.AfflictionType))
			{
				return 0f;
			}
			if (limbType != LimbType.None && !currentEffect.ResistanceLimbs.None(null) && !currentEffect.ResistanceLimbs.Contains(limbType))
			{
				return 0f;
			}
			return MathHelper.Lerp(currentEffect.MinResistance, currentEffect.MaxResistance, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x000BA634 File Offset: 0x000B8834
		public float GetSpeedMultiplier()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return 1f;
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return 1f;
			}
			return MathHelper.Lerp(currentEffect.MinSpeedMultiplier, currentEffect.MaxSpeedMultiplier, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x000BA684 File Offset: 0x000B8884
		public float GetStatValue(StatTypes statType)
		{
			AfflictionPrefab.Effect currentEffect = this.GetViableEffect();
			if (currentEffect == null)
			{
				return 0f;
			}
			AfflictionPrefab.Effect.AppliedStatValue appliedStat;
			if (!currentEffect.AfflictionStatValues.TryGetValue(statType, out appliedStat))
			{
				return 0f;
			}
			return MathHelper.Lerp(appliedStat.MinValue, appliedStat.MaxValue, currentEffect.GetStrengthFactor(this));
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x000BA6D0 File Offset: 0x000B88D0
		public bool HasFlag(AbilityFlags flagType)
		{
			AfflictionPrefab.Effect currentEffect = this.GetViableEffect();
			return currentEffect != null && currentEffect.AfflictionAbilityFlags.HasFlag(flagType);
		}

		// Token: 0x0600160B RID: 5643 RVA: 0x000BA6FF File Offset: 0x000B88FF
		private AfflictionPrefab.Effect GetViableEffect()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return null;
			}
			return this.GetActiveEffect();
		}

		// Token: 0x0600160C RID: 5644 RVA: 0x000BA71C File Offset: 0x000B891C
		public virtual void Update(CharacterHealth characterHealth, Limb targetLimb, float deltaTime)
		{
			foreach (AfflictionPrefab.PeriodicEffect periodicEffect in this.Prefab.PeriodicEffects)
			{
				if (this.Strength > periodicEffect.MinStrength && (periodicEffect.MaxStrength <= 0f || this.Strength <= periodicEffect.MaxStrength))
				{
					Dictionary<AfflictionPrefab.PeriodicEffect, float> periodicEffectTimers = this.PeriodicEffectTimers;
					AfflictionPrefab.PeriodicEffect key = periodicEffect;
					periodicEffectTimers[key] -= deltaTime;
					if (this.PeriodicEffectTimers[periodicEffect] <= 0f)
					{
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
						{
							this.PeriodicEffectTimers[periodicEffect] = 0f;
						}
						else
						{
							characterHealth.Character.healthUpdateTimer = 0f;
							foreach (StatusEffect statusEffect in periodicEffect.StatusEffects)
							{
								this.ApplyStatusEffect(ActionType.OnActive, statusEffect, 1f, characterHealth, targetLimb);
								this.PeriodicEffectTimers[periodicEffect] = Rand.Range(periodicEffect.MinInterval, periodicEffect.MaxInterval, Rand.RandSync.Unsynced);
							}
						}
					}
				}
			}
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect == null)
			{
				return;
			}
			this.fluctuationTimer += deltaTime * currentEffect.ScreenEffectFluctuationFrequency;
			this.fluctuationTimer %= 1f;
			if (currentEffect.StrengthChange < 0f)
			{
				float stat = characterHealth.Character.GetStatValue(this.Prefab.IsBuff ? StatTypes.BuffDurationMultiplier : StatTypes.DebuffDurationMultiplier, true);
				float durationMultiplier = 1f / (1f + stat);
				this._strength += currentEffect.StrengthChange * deltaTime * this.StrengthDiminishMultiplier.Item1 * durationMultiplier;
			}
			else if (currentEffect.StrengthChange > 0f)
			{
				this._strength += currentEffect.StrengthChange * deltaTime * (1f - characterHealth.GetResistance(this.Prefab, (targetLimb != null) ? targetLimb.type : LimbType.None));
			}
			this._strength = MathHelper.Clamp(this._strength, 0f, this.Prefab.MaxStrength);
			this.activeEffectDirty |= !MathUtils.NearlyEqual(this.prevActiveEffectStrength, this._strength, 0.0001f);
			foreach (StatusEffect statusEffect2 in currentEffect.StatusEffects)
			{
				this.ApplyStatusEffect(ActionType.OnActive, statusEffect2, deltaTime, characterHealth, targetLimb);
			}
			if (currentEffect.ConvulseAmount > 0f)
			{
				foreach (Limb limb in characterHealth.Character.AnimController.Limbs)
				{
					if (!limb.IsSevered && !limb.Hidden)
					{
						float force = Rand.Value(Rand.RandSync.Unsynced) * limb.Mass * currentEffect.ConvulseAmount;
						limb.body.ApplyLinearImpulse(Rand.Vector(force, Rand.RandSync.Unsynced), 32f);
					}
				}
			}
			float amount = deltaTime;
			if (this.Prefab.GrainBurst > 0f)
			{
				amount /= this.Prefab.GrainBurst;
			}
			if (this.PendingGrainEffectStrength >= 0f)
			{
				this.GrainEffectStrength += amount;
				this.PendingGrainEffectStrength -= deltaTime;
				return;
			}
			if (this.GrainEffectStrength > 0f)
			{
				this.GrainEffectStrength -= amount;
			}
		}

		// Token: 0x0600160D RID: 5645 RVA: 0x000BAAB8 File Offset: 0x000B8CB8
		public void ApplyStatusEffects(ActionType type, float deltaTime, CharacterHealth characterHealth, Limb targetLimb)
		{
			AfflictionPrefab.Effect currentEffect = this.GetActiveEffect();
			if (currentEffect != null)
			{
				foreach (StatusEffect statusEffect in currentEffect.StatusEffects)
				{
					this.ApplyStatusEffect(type, statusEffect, deltaTime, characterHealth, targetLimb);
				}
			}
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x000BAAFC File Offset: 0x000B8CFC
		public void ApplyStatusEffect(ActionType type, StatusEffect statusEffect, float deltaTime, CharacterHealth characterHealth, Limb targetLimb)
		{
			if (type == ActionType.OnDamaged && !statusEffect.HasRequiredAfflictions(characterHealth.Character.LastDamage))
			{
				return;
			}
			statusEffect.SetUser(this.Source);
			if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
			{
				statusEffect.Apply(type, deltaTime, characterHealth.Character, characterHealth.Character, null);
			}
			if (targetLimb != null && statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
			{
				statusEffect.Apply(type, deltaTime, characterHealth.Character, targetLimb, null);
			}
			bool flag;
			if (characterHealth == null)
			{
				flag = (null != null);
			}
			else
			{
				Character character = characterHealth.Character;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					AnimController animController = character.AnimController;
					flag = (((animController != null) ? animController.Limbs : null) != null);
				}
			}
			if (flag && statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
			{
				statusEffect.Apply(type, deltaTime, characterHealth.Character, characterHealth.Character.AnimController.Limbs, null);
			}
			if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) || statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
			{
				this.targets.Clear();
				statusEffect.AddNearbyTargets(characterHealth.Character.WorldPosition, this.targets);
				statusEffect.Apply(type, deltaTime, characterHealth.Character, this.targets, null);
			}
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x000BAC38 File Offset: 0x000B8E38
		public void SetStrength(float strength)
		{
			if (!MathUtils.IsValid(strength))
			{
				return;
			}
			this._nonClampedStrength = strength;
			this._strength = this._nonClampedStrength;
			this.activeEffectDirty |= !MathUtils.NearlyEqual(this._strength, this.prevActiveEffectStrength, 0.0001f);
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x000BAC87 File Offset: 0x000B8E87
		public bool ShouldShowIcon(Character afflictedCharacter)
		{
			return this.Strength >= ((afflictedCharacter == Character.Controlled) ? this.Prefab.ShowIconThreshold : this.Prefab.ShowIconToOthersThreshold);
		}

		// Token: 0x04000A66 RID: 2662
		public readonly AfflictionPrefab Prefab;

		// Token: 0x04000A6A RID: 2666
		private float fluctuationTimer;

		// Token: 0x04000A6B RID: 2667
		private AfflictionPrefab.Effect activeEffect;

		// Token: 0x04000A6C RID: 2668
		private float prevActiveEffectStrength;

		// Token: 0x04000A6D RID: 2669
		protected bool activeEffectDirty = true;

		// Token: 0x04000A6E RID: 2670
		protected float _strength;

		// Token: 0x04000A70 RID: 2672
		private float _nonClampedStrength = -1f;

		// Token: 0x04000A75 RID: 2677
		public float DamagePerSecond;

		// Token: 0x04000A76 RID: 2678
		public float DamagePerSecondTimer;

		// Token: 0x04000A77 RID: 2679
		public float PreviousVitalityDecrease;

		// Token: 0x04000A78 RID: 2680
		[TupleElementNames(new string[]
		{
			"Value",
			"Source"
		})]
		public ValueTuple<float, Affliction> StrengthDiminishMultiplier = new ValueTuple<float, Affliction>(1f, null);

		// Token: 0x04000A79 RID: 2681
		public readonly Dictionary<AfflictionPrefab.PeriodicEffect, float> PeriodicEffectTimers = new Dictionary<AfflictionPrefab.PeriodicEffect, float>();

		// Token: 0x04000A7A RID: 2682
		public double AppliedAsSuccessfulTreatmentTime;

		// Token: 0x04000A7B RID: 2683
		public double AppliedAsFailedTreatmentTime;

		// Token: 0x04000A7C RID: 2684
		public float Duration;

		// Token: 0x04000A7D RID: 2685
		public Character Source;

		// Token: 0x04000A7E RID: 2686
		private static readonly LocalizedString[] strengthTexts = new LocalizedString[]
		{
			TextManager.Get("AfflictionStrengthLow"),
			TextManager.Get("AfflictionStrengthMedium"),
			TextManager.Get("AfflictionStrengthHigh")
		};

		// Token: 0x04000A7F RID: 2687
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();
	}
}
