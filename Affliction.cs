using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C4 RID: 452
	internal class Affliction : ISerializableEntity
	{
		// Token: 0x17000D03 RID: 3331
		// (get) Token: 0x0600319C RID: 12700 RVA: 0x00204AB9 File Offset: 0x00202CB9
		public string Name
		{
			get
			{
				return this.ToString();
			}
		}

		// Token: 0x17000D04 RID: 3332
		// (get) Token: 0x0600319D RID: 12701 RVA: 0x00204AC1 File Offset: 0x00202CC1
		// (set) Token: 0x0600319E RID: 12702 RVA: 0x00204AC9 File Offset: 0x00202CC9
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17000D05 RID: 3333
		// (get) Token: 0x0600319F RID: 12703 RVA: 0x00204AD2 File Offset: 0x00202CD2
		// (set) Token: 0x060031A0 RID: 12704 RVA: 0x00204ADA File Offset: 0x00202CDA
		public float PendingGrainEffectStrength { get; set; }

		// Token: 0x17000D06 RID: 3334
		// (get) Token: 0x060031A1 RID: 12705 RVA: 0x00204AE3 File Offset: 0x00202CE3
		// (set) Token: 0x060031A2 RID: 12706 RVA: 0x00204AEB File Offset: 0x00202CEB
		public float GrainEffectStrength { get; set; }

		// Token: 0x17000D07 RID: 3335
		// (get) Token: 0x060031A3 RID: 12707 RVA: 0x00204AF4 File Offset: 0x00202CF4
		// (set) Token: 0x060031A4 RID: 12708 RVA: 0x00204AFC File Offset: 0x00202CFC
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

		// Token: 0x17000D08 RID: 3336
		// (get) Token: 0x060031A5 RID: 12709 RVA: 0x00204B7E File Offset: 0x00202D7E
		// (set) Token: 0x060031A6 RID: 12710 RVA: 0x00204B86 File Offset: 0x00202D86
		public float Penetration { get; set; }

		// Token: 0x17000D09 RID: 3337
		// (get) Token: 0x060031A7 RID: 12711 RVA: 0x00204B8F File Offset: 0x00202D8F
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

		// Token: 0x17000D0A RID: 3338
		// (get) Token: 0x060031A8 RID: 12712 RVA: 0x00204BAB File Offset: 0x00202DAB
		// (set) Token: 0x060031A9 RID: 12713 RVA: 0x00204BB3 File Offset: 0x00202DB3
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Identifier Identifier { get; private set; }

		// Token: 0x17000D0B RID: 3339
		// (get) Token: 0x060031AA RID: 12714 RVA: 0x00204BBC File Offset: 0x00202DBC
		// (set) Token: 0x060031AB RID: 12715 RVA: 0x00204BC4 File Offset: 0x00202DC4
		[Serialize(1f, IsPropertySaveable.Yes, "The probability for the affliction to be applied.", "", false)]
		[Editable(0f, 1f, 1)]
		public float Probability { get; set; } = 1f;

		// Token: 0x17000D0C RID: 3340
		// (get) Token: 0x060031AC RID: 12716 RVA: 0x00204BCD File Offset: 0x00202DCD
		// (set) Token: 0x060031AD RID: 12717 RVA: 0x00204BD5 File Offset: 0x00202DD5
		[Serialize(true, IsPropertySaveable.Yes, "Explosion damage is applied per each affected limb. Should this affliction damage be divided by the count of affected limbs (1-15) or applied in full? Default: true. Only affects status effects and explosions.", "", false)]
		[Editable]
		public bool DivideByLimbCount { get; set; }

		// Token: 0x17000D0D RID: 3341
		// (get) Token: 0x060031AE RID: 12718 RVA: 0x00204BDE File Offset: 0x00202DDE
		// (set) Token: 0x060031AF RID: 12719 RVA: 0x00204BE6 File Offset: 0x00202DE6
		[Serialize(false, IsPropertySaveable.Yes, "Is the damage relative to the max vitality (percentage) or absolute (normal)", "", false)]
		[Editable]
		public bool MultiplyByMaxVitality { get; set; }

		// Token: 0x17000D0E RID: 3342
		// (get) Token: 0x060031B0 RID: 12720 RVA: 0x00204BEF File Offset: 0x00202DEF
		public bool AffectedByAttackMultipliers
		{
			get
			{
				return this.Prefab.AffectedByAttackMultipliers;
			}
		}

		// Token: 0x060031B1 RID: 12721 RVA: 0x00204BFC File Offset: 0x00202DFC
		public Affliction(AfflictionPrefab prefab, float strength)
		{
			if (prefab != null)
			{
				prefab.ReloadSoundsIfNeeded();
			}
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

		// Token: 0x060031B2 RID: 12722 RVA: 0x00204CEC File Offset: 0x00202EEC
		public void CopyProperties(Affliction source)
		{
			this.Probability = source.Probability;
			this.DivideByLimbCount = source.DivideByLimbCount;
			this.MultiplyByMaxVitality = source.MultiplyByMaxVitality;
			this.Penetration = source.Penetration;
		}

		// Token: 0x060031B3 RID: 12723 RVA: 0x00204D1E File Offset: 0x00202F1E
		public void Serialize(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, false, false);
		}

		// Token: 0x060031B4 RID: 12724 RVA: 0x00204D2C File Offset: 0x00202F2C
		public void Deserialize(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element.GetAttribute("amount", StringComparison.OrdinalIgnoreCase) != null && element.GetAttribute("strength", StringComparison.OrdinalIgnoreCase) == null)
			{
				this.Strength = element.GetAttributeFloat("amount", 0f);
			}
		}

		// Token: 0x060031B5 RID: 12725 RVA: 0x00204D78 File Offset: 0x00202F78
		public Affliction CreateMultiplied(float multiplier, Affliction affliction)
		{
			Affliction instance = this.Prefab.Instantiate(this.NonClampedStrength * multiplier, this.Source);
			instance.CopyProperties(affliction);
			return instance;
		}

		// Token: 0x060031B6 RID: 12726 RVA: 0x00204DA8 File Offset: 0x00202FA8
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

		// Token: 0x060031B7 RID: 12727 RVA: 0x00204DFE File Offset: 0x00202FFE
		public LocalizedString GetStrengthText()
		{
			return Affliction.GetStrengthText(this.Strength, this.Prefab.MaxStrength);
		}

		// Token: 0x060031B8 RID: 12728 RVA: 0x00204E16 File Offset: 0x00203016
		public static LocalizedString GetStrengthText(float strength, float maxStrength)
		{
			return Affliction.strengthTexts[MathHelper.Clamp((int)Math.Floor((double)(strength / maxStrength * (float)Affliction.strengthTexts.Length)), 0, Affliction.strengthTexts.Length - 1)];
		}

		// Token: 0x060031B9 RID: 12729 RVA: 0x00204E40 File Offset: 0x00203040
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

		// Token: 0x060031BA RID: 12730 RVA: 0x00204E7A File Offset: 0x0020307A
		public float GetVitalityDecrease(CharacterHealth characterHealth)
		{
			return this.GetVitalityDecrease(characterHealth, this.Strength);
		}

		// Token: 0x060031BB RID: 12731 RVA: 0x00204E8C File Offset: 0x0020308C
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

		// Token: 0x060031BC RID: 12732 RVA: 0x00204F24 File Offset: 0x00203124
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

		// Token: 0x060031BD RID: 12733 RVA: 0x00204FC8 File Offset: 0x002031C8
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

		// Token: 0x060031BE RID: 12734 RVA: 0x00205038 File Offset: 0x00203238
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

		// Token: 0x060031BF RID: 12735 RVA: 0x002050A8 File Offset: 0x002032A8
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

		// Token: 0x060031C0 RID: 12736 RVA: 0x00205118 File Offset: 0x00203318
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

		// Token: 0x060031C1 RID: 12737 RVA: 0x002051A0 File Offset: 0x002033A0
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

		// Token: 0x060031C2 RID: 12738 RVA: 0x002051F0 File Offset: 0x002033F0
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

		// Token: 0x060031C3 RID: 12739 RVA: 0x00205240 File Offset: 0x00203440
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

		// Token: 0x060031C4 RID: 12740 RVA: 0x002052B0 File Offset: 0x002034B0
		private float GetScreenEffectFluctuation(AfflictionPrefab.Effect currentEffect)
		{
			if (currentEffect == null || currentEffect.ScreenEffectFluctuationFrequency <= 0f)
			{
				return 1f;
			}
			return ((float)Math.Sin((double)(this.fluctuationTimer * 6.2831855f)) + 1f) * 0.5f;
		}

		// Token: 0x060031C5 RID: 12741 RVA: 0x002052E8 File Offset: 0x002034E8
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

		// Token: 0x060031C6 RID: 12742 RVA: 0x00205338 File Offset: 0x00203538
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

		// Token: 0x060031C7 RID: 12743 RVA: 0x0020538C File Offset: 0x0020358C
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

		// Token: 0x060031C8 RID: 12744 RVA: 0x00205444 File Offset: 0x00203644
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

		// Token: 0x060031C9 RID: 12745 RVA: 0x00205494 File Offset: 0x00203694
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

		// Token: 0x060031CA RID: 12746 RVA: 0x002054E0 File Offset: 0x002036E0
		public bool HasFlag(AbilityFlags flagType)
		{
			AfflictionPrefab.Effect currentEffect = this.GetViableEffect();
			return currentEffect != null && currentEffect.AfflictionAbilityFlags.HasFlag(flagType);
		}

		// Token: 0x060031CB RID: 12747 RVA: 0x0020550F File Offset: 0x0020370F
		private AfflictionPrefab.Effect GetViableEffect()
		{
			if (this.Strength < this.Prefab.ActivationThreshold)
			{
				return null;
			}
			return this.GetActiveEffect();
		}

		// Token: 0x060031CC RID: 12748 RVA: 0x0020552C File Offset: 0x0020372C
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

		// Token: 0x060031CD RID: 12749 RVA: 0x002058C8 File Offset: 0x00203AC8
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

		// Token: 0x060031CE RID: 12750 RVA: 0x0020590C File Offset: 0x00203B0C
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

		// Token: 0x060031CF RID: 12751 RVA: 0x00205A48 File Offset: 0x00203C48
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

		// Token: 0x060031D0 RID: 12752 RVA: 0x00205A97 File Offset: 0x00203C97
		public bool ShouldShowIcon(Character afflictedCharacter)
		{
			return this.Strength >= ((afflictedCharacter == Character.Controlled) ? this.Prefab.ShowIconThreshold : this.Prefab.ShowIconToOthersThreshold);
		}

		// Token: 0x040019D1 RID: 6609
		public readonly AfflictionPrefab Prefab;

		// Token: 0x040019D5 RID: 6613
		private float fluctuationTimer;

		// Token: 0x040019D6 RID: 6614
		private AfflictionPrefab.Effect activeEffect;

		// Token: 0x040019D7 RID: 6615
		private float prevActiveEffectStrength;

		// Token: 0x040019D8 RID: 6616
		protected bool activeEffectDirty = true;

		// Token: 0x040019D9 RID: 6617
		protected float _strength;

		// Token: 0x040019DB RID: 6619
		private float _nonClampedStrength = -1f;

		// Token: 0x040019E0 RID: 6624
		public float DamagePerSecond;

		// Token: 0x040019E1 RID: 6625
		public float DamagePerSecondTimer;

		// Token: 0x040019E2 RID: 6626
		public float PreviousVitalityDecrease;

		// Token: 0x040019E3 RID: 6627
		[TupleElementNames(new string[]
		{
			"Value",
			"Source"
		})]
		public ValueTuple<float, Affliction> StrengthDiminishMultiplier = new ValueTuple<float, Affliction>(1f, null);

		// Token: 0x040019E4 RID: 6628
		public readonly Dictionary<AfflictionPrefab.PeriodicEffect, float> PeriodicEffectTimers = new Dictionary<AfflictionPrefab.PeriodicEffect, float>();

		// Token: 0x040019E5 RID: 6629
		public double AppliedAsSuccessfulTreatmentTime;

		// Token: 0x040019E6 RID: 6630
		public double AppliedAsFailedTreatmentTime;

		// Token: 0x040019E7 RID: 6631
		public float Duration;

		// Token: 0x040019E8 RID: 6632
		public Character Source;

		// Token: 0x040019E9 RID: 6633
		private static readonly LocalizedString[] strengthTexts = new LocalizedString[]
		{
			TextManager.Get("AfflictionStrengthLow"),
			TextManager.Get("AfflictionStrengthMedium"),
			TextManager.Get("AfflictionStrengthHigh")
		};

		// Token: 0x040019EA RID: 6634
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();
	}
}
