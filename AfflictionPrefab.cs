using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C9 RID: 457
	internal class AfflictionPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000D17 RID: 3351
		// (get) Token: 0x060031F2 RID: 12786 RVA: 0x00206F47 File Offset: 0x00205147
		public static AfflictionPrefab InternalDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["internaldamage"];
			}
		}

		// Token: 0x17000D18 RID: 3352
		// (get) Token: 0x060031F3 RID: 12787 RVA: 0x00206F58 File Offset: 0x00205158
		public static AfflictionPrefab BiteWounds
		{
			get
			{
				return AfflictionPrefab.Prefabs["bitewounds"];
			}
		}

		// Token: 0x17000D19 RID: 3353
		// (get) Token: 0x060031F4 RID: 12788 RVA: 0x00206F69 File Offset: 0x00205169
		public static AfflictionPrefab ImpactDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["blunttrauma"];
			}
		}

		// Token: 0x17000D1A RID: 3354
		// (get) Token: 0x060031F5 RID: 12789 RVA: 0x00206F7A File Offset: 0x0020517A
		public static AfflictionPrefab Bleeding
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.BleedingType];
			}
		}

		// Token: 0x17000D1B RID: 3355
		// (get) Token: 0x060031F6 RID: 12790 RVA: 0x00206F8B File Offset: 0x0020518B
		public static AfflictionPrefab Burn
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.BurnType];
			}
		}

		// Token: 0x17000D1C RID: 3356
		// (get) Token: 0x060031F7 RID: 12791 RVA: 0x00206F9C File Offset: 0x0020519C
		public static AfflictionPrefab OxygenLow
		{
			get
			{
				return AfflictionPrefab.Prefabs["oxygenlow"];
			}
		}

		// Token: 0x17000D1D RID: 3357
		// (get) Token: 0x060031F8 RID: 12792 RVA: 0x00206FAD File Offset: 0x002051AD
		public static AfflictionPrefab Bloodloss
		{
			get
			{
				return AfflictionPrefab.Prefabs["bloodloss"];
			}
		}

		// Token: 0x17000D1E RID: 3358
		// (get) Token: 0x060031F9 RID: 12793 RVA: 0x00206FBE File Offset: 0x002051BE
		public static AfflictionPrefab Pressure
		{
			get
			{
				return AfflictionPrefab.Prefabs["pressure"];
			}
		}

		// Token: 0x17000D1F RID: 3359
		// (get) Token: 0x060031FA RID: 12794 RVA: 0x00206FCF File Offset: 0x002051CF
		public static AfflictionPrefab OrganDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["organdamage"];
			}
		}

		// Token: 0x17000D20 RID: 3360
		// (get) Token: 0x060031FB RID: 12795 RVA: 0x00206FE0 File Offset: 0x002051E0
		public static AfflictionPrefab Stun
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.StunType];
			}
		}

		// Token: 0x17000D21 RID: 3361
		// (get) Token: 0x060031FC RID: 12796 RVA: 0x00206FF1 File Offset: 0x002051F1
		public static AfflictionPrefab RadiationSickness
		{
			get
			{
				return AfflictionPrefab.Prefabs["radiationsickness"];
			}
		}

		// Token: 0x17000D22 RID: 3362
		// (get) Token: 0x060031FD RID: 12797 RVA: 0x00207002 File Offset: 0x00205202
		public static AfflictionPrefab HuskInfection
		{
			get
			{
				return AfflictionPrefab.Prefabs["huskinfection"];
			}
		}

		// Token: 0x17000D23 RID: 3363
		// (get) Token: 0x060031FE RID: 12798 RVA: 0x00207013 File Offset: 0x00205213
		public static AfflictionPrefab JovianRadiation
		{
			get
			{
				return AfflictionPrefab.Prefabs["jovianradiation"];
			}
		}

		// Token: 0x17000D24 RID: 3364
		// (get) Token: 0x060031FF RID: 12799 RVA: 0x00207024 File Offset: 0x00205224
		public static IEnumerable<AfflictionPrefab> List
		{
			get
			{
				return AfflictionPrefab.Prefabs;
			}
		}

		// Token: 0x06003200 RID: 12800 RVA: 0x0020702B File Offset: 0x0020522B
		public override void Dispose()
		{
		}

		// Token: 0x17000D25 RID: 3365
		// (get) Token: 0x06003201 RID: 12801 RVA: 0x0020702D File Offset: 0x0020522D
		// (set) Token: 0x06003202 RID: 12802 RVA: 0x00207035 File Offset: 0x00205235
		public Identifier[] TargetSpecies { get; protected set; }

		// Token: 0x17000D26 RID: 3366
		// (get) Token: 0x06003203 RID: 12803 RVA: 0x0020703E File Offset: 0x0020523E
		public IEnumerable<AfflictionPrefab.Effect> Effects
		{
			get
			{
				return this.effects;
			}
		}

		// Token: 0x17000D27 RID: 3367
		// (get) Token: 0x06003204 RID: 12804 RVA: 0x00207046 File Offset: 0x00205246
		public IList<AfflictionPrefab.PeriodicEffect> PeriodicEffects
		{
			get
			{
				return this.periodicEffects;
			}
		}

		// Token: 0x17000D28 RID: 3368
		// (get) Token: 0x06003205 RID: 12805 RVA: 0x0020704E File Offset: 0x0020524E
		// (set) Token: 0x06003206 RID: 12806 RVA: 0x00207056 File Offset: 0x00205256
		public float AfflictionOverlayAnimSpeed { get; set; }

		// Token: 0x17000D29 RID: 3369
		// (get) Token: 0x06003207 RID: 12807 RVA: 0x0020705F File Offset: 0x0020525F
		// (set) Token: 0x06003208 RID: 12808 RVA: 0x00207067 File Offset: 0x00205267
		public ImmutableDictionary<Identifier, float> TreatmentSuitabilities { get; private set; } = new Dictionary<Identifier, float>().ToImmutableDictionary<Identifier, float>();

		// Token: 0x17000D2A RID: 3370
		// (get) Token: 0x06003209 RID: 12809 RVA: 0x00207070 File Offset: 0x00205270
		// (set) Token: 0x0600320A RID: 12810 RVA: 0x00207078 File Offset: 0x00205278
		public bool HasTreatments { get; private set; }

		// Token: 0x0600320B RID: 12811 RVA: 0x00207084 File Offset: 0x00205284
		public AfflictionPrefab(ContentXElement element, AfflictionsFile file, Type type) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.configElement = element;
			this.AfflictionType = element.GetAttributeIdentifier("type", "");
			this.TranslationIdentifier = element.GetAttributeIdentifier("translationoverride", this.Identifier);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("AfflictionName.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TranslationIdentifier);
			this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			string fallbackName = element.GetAttributeString("name", "");
			if (!string.IsNullOrEmpty(fallbackName))
			{
				this.Name = this.Name.Fallback(fallbackName, true);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(22, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("AfflictionDescription.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TranslationIdentifier);
			this.defaultDescription = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			string fallbackDescription = element.GetAttributeString("description", "");
			if (!string.IsNullOrEmpty(fallbackDescription))
			{
				this.defaultDescription = this.defaultDescription.Fallback(fallbackDescription, true);
			}
			this.ShowDescriptionInTooltip = element.GetAttributeBool("ShowDescriptionInTooltip", true);
			this.IsBuff = element.GetAttributeBool("IsBuff", false);
			this.AffectedByAttackMultipliers = element.GetAttributeBool("AffectedByAttackMultipliers", !this.IsBuff);
			this.AffectMachines = element.GetAttributeBool("AffectMachines", true);
			this.ShowBarInHealthMenu = element.GetAttributeBool("showbarinhealthmenu", true);
			this.HealableInMedicalClinic = element.GetAttributeBool("healableinmedicalclinic", !this.IsBuff && this.AfflictionType != "geneticmaterialbuff" && this.AfflictionType != "geneticmaterialdebuff");
			this.HealCostMultiplier = element.GetAttributeFloat("HealCostMultiplier", 1f);
			this.BaseHealCost = element.GetAttributeInt("BaseHealCost", 0);
			this.IgnoreTreatmentIfAfflictedBy = element.GetAttributeIdentifierArray("IgnoreTreatmentIfAfflictedBy", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.Duration = element.GetAttributeFloat("Duration", 0f);
			if (element.GetAttribute("nameidentifier") != null)
			{
				string nameIdentifier = element.GetAttributeString("nameidentifier", string.Empty);
				this.Name = TextManager.Get(nameIdentifier).Fallback(TextManager.Get("AfflictionName." + nameIdentifier), true).Fallback(this.Name, true);
			}
			this.LimbSpecific = element.GetAttributeBool("limbspecific", false);
			if (!this.LimbSpecific)
			{
				string indicatorLimbName = element.GetAttributeString("indicatorlimb", "Torso");
				if (!Enum.TryParse<LimbType>(indicatorLimbName, out this.IndicatorLimb))
				{
					DebugConsole.ThrowErrorLocalized("Error in affliction prefab " + this.Name + " - limb type \"" + indicatorLimbName + "\" not found.", null, null, false, false);
				}
			}
			this.HideIconAfterDelay = element.GetAttributeBool("HideIconAfterDelay", false);
			this.ActivationThreshold = element.GetAttributeFloat("ActivationThreshold", 0f);
			if (this.Identifier == AfflictionPrefab.StunType && this.ActivationThreshold > 0f)
			{
				this.ActivationThreshold = 0f;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(194, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Error in affliction prefab ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral(": activation threshold of the stun affliction must be 0, because the strength of the affliction represents the length of the stun and any amount of stun has an effect.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
			}
			this.ShowIconThreshold = element.GetAttributeFloat("ShowIconThreshold", Math.Max(this.ActivationThreshold, 0.05f));
			this.ShowIconToOthersThreshold = element.GetAttributeFloat("ShowIconToOthersThreshold", this.ShowIconThreshold);
			this.MaxStrength = element.GetAttributeFloat("MaxStrength", 100f);
			this.GrainBurst = element.GetAttributeFloat("GrainBurst", 0f);
			this.ShowInHealthScannerThreshold = element.GetAttributeFloat("ShowInHealthScannerThreshold", Math.Max(this.ActivationThreshold, (this.AfflictionType == "talentbuff") ? float.MaxValue : this.ShowIconToOthersThreshold));
			this.TreatmentThreshold = element.GetAttributeFloat("TreatmentThreshold", Math.Max(this.ActivationThreshold, 10f));
			this.TreatmentSuggestionThreshold = element.GetAttributeFloat("TreatmentSuggestionThreshold", this.TreatmentThreshold);
			bool alwaysRequiresTreatment = this.AfflictionType == AfflictionPrefab.ParalysisType || this.AfflictionType == AfflictionPrefab.PoisonType || this is AfflictionPrefabHusk;
			this.VitalityLossRequiredForTreatment = element.GetAttributeBool("VitalityLossRequiredForTreatment", !alwaysRequiresTreatment);
			this.DamageOverlayAlpha = element.GetAttributeFloat("DamageOverlayAlpha", 0f);
			this.BurnOverlayAlpha = element.GetAttributeFloat("BurnOverlayAlpha", 0f);
			this.KarmaChangeOnApplied = element.GetAttributeFloat("KarmaChangeOnApplied", 0f);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("AfflictionCauseOfDeath.");
			defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.TranslationIdentifier);
			this.CauseOfDeathDescription = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear()).Fallback(TextManager.Get(element.GetAttributeString("causeofdeathdescription", "")), true).Fallback(element.GetAttributeString("causeofdeathdescription", ""), true);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("AfflictionCauseOfDeathSelf.");
			defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(this.TranslationIdentifier);
			this.SelfCauseOfDeathDescription = TextManager.Get(defaultInterpolatedStringHandler5.ToStringAndClear()).Fallback(TextManager.Get(element.GetAttributeString("selfcauseofdeathdescription", "")), true).Fallback(element.GetAttributeString("selfcauseofdeathdescription", ""), true);
			this.IconColors = element.GetAttributeColorArray("IconColors", null);
			this.AfflictionOverlayAlphaIsLinear = element.GetAttributeBool("AfflictionOverlayAlphaIsLinear", false);
			this.AchievementOnReceived = element.GetAttributeIdentifier("AchievementOnReceived", "");
			this.AchievementOnRemoved = element.GetAttributeIdentifier("AchievementOnRemoved", "");
			this.TargetSpecies = element.GetAttributeIdentifierArray("targets", Array.Empty<Identifier>(), true);
			this.ResetBetweenRounds = element.GetAttributeBool("resetbetweenrounds", false);
			this.DamageParticles = element.GetAttributeBool("DamageParticles", true);
			this.WeaponsSkillGain = element.GetAttributeFloat("WeaponsSkillGain", 0f);
			this.MedicalSkillGain = element.GetAttributeFloat("MedicalSkillGain", 0f);
			List<AfflictionPrefab.Description> descriptions = new List<AfflictionPrefab.Description>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length <= 9)
					{
						if (length != 4)
						{
							if (length != 6)
							{
								if (length == 9)
								{
									if (text == "statvalue")
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(90, 1);
										defaultInterpolatedStringHandler6.AppendLiteral("Error in affliction \"");
										defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(this.Identifier);
										defaultInterpolatedStringHandler6.AppendLiteral("\" - stat values should be configured inside the affliction's effects.");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler6.ToStringAndClear(), null, element.ContentPackage, false, false);
										continue;
									}
								}
							}
							else if (text == "effect")
							{
								continue;
							}
						}
						else if (text == "icon")
						{
							this.Icon = new Sprite(subElement, "", "", false, 1f);
							continue;
						}
					}
					else if (length <= 14)
					{
						if (length != 11)
						{
							if (length == 14)
							{
								if (text == "periodiceffect")
								{
									continue;
								}
							}
						}
						else if (text == "description")
						{
							descriptions.Add(new AfflictionPrefab.Description(subElement, this));
							continue;
						}
					}
					else if (length != 17)
					{
						if (length == 25)
						{
							if (text == "afflictionoverlayanimated")
							{
								this.AfflictionOverlay = new SpriteSheet(subElement, "", "");
								this.AfflictionOverlayAnimSpeed = subElement.GetAttributeFloat("animspeed", 1f);
								continue;
							}
						}
					}
					else if (text == "afflictionoverlay")
					{
						this.AfflictionOverlay = new Sprite(subElement, "", "", false, 1f);
						continue;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(40, 2);
				defaultInterpolatedStringHandler7.AppendLiteral("Unrecognized element in affliction \"");
				defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler7.AppendLiteral("\" (");
				defaultInterpolatedStringHandler7.AppendFormatted<XName>(subElement.Name);
				defaultInterpolatedStringHandler7.AppendLiteral(")");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler7.ToStringAndClear(), element.ContentPackage);
			}
			this.Descriptions = descriptions.ToImmutableList<AfflictionPrefab.Description>();
			this.constructor = type.GetConstructor(new Type[]
			{
				typeof(AfflictionPrefab),
				typeof(float)
			});
		}

		// Token: 0x0600320C RID: 12812 RVA: 0x002079F8 File Offset: 0x00205BF8
		private void RefreshTreatmentSuitabilities()
		{
			Dictionary<Identifier, float> newTreatmentSuitabilities = new Dictionary<Identifier, float>();
			foreach (ItemPrefab itemPrefab in ItemPrefab.Prefabs)
			{
				float suitability = itemPrefab.GetTreatmentSuitability(this.Identifier) + itemPrefab.GetTreatmentSuitability(this.AfflictionType);
				if (!MathUtils.NearlyEqual(suitability, 0f, 0.0001f))
				{
					newTreatmentSuitabilities.TryAdd(itemPrefab.Identifier, suitability);
				}
			}
			this.HasTreatments = newTreatmentSuitabilities.Any((KeyValuePair<Identifier, float> kvp) => kvp.Value > 0f);
			this.TreatmentSuitabilities = newTreatmentSuitabilities.ToImmutableDictionary<Identifier, float>();
		}

		// Token: 0x0600320D RID: 12813 RVA: 0x00207AB4 File Offset: 0x00205CB4
		public LocalizedString GetDescription(float strength, AfflictionPrefab.Description.TargetType targetType)
		{
			foreach (AfflictionPrefab.Description description in this.Descriptions)
			{
				if (strength >= description.MinStrength && strength <= description.MaxStrength)
				{
					if (targetType != AfflictionPrefab.Description.TargetType.Self)
					{
						if (targetType == AfflictionPrefab.Description.TargetType.OtherCharacter)
						{
							if (description.Target == AfflictionPrefab.Description.TargetType.Self)
							{
								continue;
							}
						}
					}
					else if (description.Target == AfflictionPrefab.Description.TargetType.OtherCharacter)
					{
						continue;
					}
					return description.Text;
				}
			}
			return this.defaultDescription;
		}

		// Token: 0x0600320E RID: 12814 RVA: 0x00207B44 File Offset: 0x00205D44
		public static void LoadAllEffectsAndTreatmentSuitabilities()
		{
			foreach (AfflictionPrefab prefab in AfflictionPrefab.Prefabs)
			{
				prefab.RefreshTreatmentSuitabilities();
				prefab.LoadEffects();
			}
		}

		// Token: 0x0600320F RID: 12815 RVA: 0x00207B98 File Offset: 0x00205D98
		public static void ClearAllEffects()
		{
			AfflictionPrefab.Prefabs.ForEach(delegate(AfflictionPrefab p)
			{
				p.ClearEffects();
			});
		}

		// Token: 0x06003210 RID: 12816 RVA: 0x00207BC4 File Offset: 0x00205DC4
		private void LoadEffects()
		{
			this.ClearEffects();
			foreach (ContentXElement subElement in this.configElement.Elements())
			{
				string a2 = subElement.Name.ToString().ToLowerInvariant();
				if (!(a2 == "effect"))
				{
					if (a2 == "periodiceffect")
					{
						this.periodicEffects.Add(new AfflictionPrefab.PeriodicEffect(subElement, this.Name.Value));
					}
				}
				else
				{
					this.effects.Add(new AfflictionPrefab.Effect(subElement, this.Name.Value));
				}
			}
			for (int i = 0; i < this.effects.Count; i++)
			{
				for (int j = i + 1; j < this.effects.Count; j++)
				{
					AfflictionPrefab.Effect a = this.effects[i];
					AfflictionPrefab.Effect b = this.effects[j];
					if (a.MinStrength < b.MaxStrength && b.MinStrength < a.MaxStrength)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(144, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Affliction \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" contains effects with overlapping strength ranges. Only one effect can be active at a time, meaning one of the effects won't work.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), base.ContentPackage);
					}
				}
			}
		}

		// Token: 0x06003211 RID: 12817 RVA: 0x00207D3C File Offset: 0x00205F3C
		private void ClearEffects()
		{
			this.effects.Clear();
			this.periodicEffects.Clear();
		}

		// Token: 0x06003212 RID: 12818 RVA: 0x00207D54 File Offset: 0x00205F54
		public void ReloadSoundsIfNeeded()
		{
			foreach (AfflictionPrefab.Effect effect in this.effects)
			{
				foreach (StatusEffect statusEffect in effect.StatusEffects)
				{
					foreach (RoundSound sound in statusEffect.Sounds)
					{
						if (sound.Sound == null)
						{
							RoundSound.Reload(sound);
						}
					}
				}
			}
			foreach (AfflictionPrefab.PeriodicEffect periodicEffect in this.periodicEffects)
			{
				foreach (StatusEffect statusEffect2 in periodicEffect.StatusEffects)
				{
					foreach (RoundSound sound2 in statusEffect2.Sounds)
					{
						if (sound2.Sound == null)
						{
							RoundSound.Reload(sound2);
						}
					}
				}
			}
		}

		// Token: 0x06003213 RID: 12819 RVA: 0x00207ED4 File Offset: 0x002060D4
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
			defaultInterpolatedStringHandler.AppendLiteral("AfflictionPrefab (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003214 RID: 12820 RVA: 0x00207F18 File Offset: 0x00206118
		public Affliction Instantiate(float strength, Character source = null)
		{
			object instance = null;
			try
			{
				instance = this.constructor.Invoke(new object[]
				{
					this,
					strength
				});
			}
			catch (Exception ex)
			{
				DebugConsole.ThrowError((ex.InnerException != null) ? ex.InnerException.ToString() : ex.ToString(), null, null, false, false);
			}
			Affliction affliction = instance as Affliction;
			affliction.Source = source;
			return affliction;
		}

		// Token: 0x06003215 RID: 12821 RVA: 0x00207F90 File Offset: 0x00206190
		public AfflictionPrefab.Effect GetActiveEffect(float currentStrength)
		{
			foreach (AfflictionPrefab.Effect effect in this.effects)
			{
				if (currentStrength > effect.MinStrength && currentStrength <= effect.MaxStrength)
				{
					return effect;
				}
			}
			AfflictionPrefab.Effect strongestEffect = null;
			float largestStrength = currentStrength;
			foreach (AfflictionPrefab.Effect effect2 in this.effects)
			{
				if (currentStrength > effect2.MaxStrength && (strongestEffect == null || effect2.MaxStrength > largestStrength))
				{
					strongestEffect = effect2;
					largestStrength = effect2.MaxStrength;
				}
			}
			return strongestEffect;
		}

		// Token: 0x06003216 RID: 12822 RVA: 0x0020805C File Offset: 0x0020625C
		public float GetTreatmentSuitability(Item item)
		{
			if (item == null)
			{
				return 0f;
			}
			return Math.Max(item.Prefab.GetTreatmentSuitability(this.Identifier), item.Prefab.GetTreatmentSuitability(this.AfflictionType));
		}

		// Token: 0x04001A0D RID: 6669
		public static readonly Identifier DamageType = "damage".ToIdentifier();

		// Token: 0x04001A0E RID: 6670
		public static readonly Identifier BurnType = "burn".ToIdentifier();

		// Token: 0x04001A0F RID: 6671
		public static readonly Identifier BleedingType = "bleeding".ToIdentifier();

		// Token: 0x04001A10 RID: 6672
		public static readonly Identifier ParalysisType = "paralysis".ToIdentifier();

		// Token: 0x04001A11 RID: 6673
		public static readonly Identifier PoisonType = "poison".ToIdentifier();

		// Token: 0x04001A12 RID: 6674
		public static readonly Identifier StunType = "stun".ToIdentifier();

		// Token: 0x04001A13 RID: 6675
		public static readonly Identifier EMPType = "emp".ToIdentifier();

		// Token: 0x04001A14 RID: 6676
		public static readonly Identifier SpaceHerpesType = "spaceherpes".ToIdentifier();

		// Token: 0x04001A15 RID: 6677
		public static readonly Identifier AlienInfectionType = "alieninfection".ToIdentifier();

		// Token: 0x04001A16 RID: 6678
		public static readonly Identifier InvertControlsType = "invertcontrols".ToIdentifier();

		// Token: 0x04001A17 RID: 6679
		public static readonly Identifier DisguisedAsHuskType = "disguiseashusk".ToIdentifier();

		// Token: 0x04001A18 RID: 6680
		public static readonly PrefabCollection<AfflictionPrefab> Prefabs = new PrefabCollection<AfflictionPrefab>();

		// Token: 0x04001A19 RID: 6681
		private readonly ContentXElement configElement;

		// Token: 0x04001A1A RID: 6682
		public readonly LocalizedString Name;

		// Token: 0x04001A1B RID: 6683
		public readonly LocalizedString CauseOfDeathDescription;

		// Token: 0x04001A1C RID: 6684
		public readonly LocalizedString SelfCauseOfDeathDescription;

		// Token: 0x04001A1D RID: 6685
		private readonly LocalizedString defaultDescription;

		// Token: 0x04001A1E RID: 6686
		public readonly ImmutableList<AfflictionPrefab.Description> Descriptions;

		// Token: 0x04001A1F RID: 6687
		public readonly bool ShowDescriptionInTooltip;

		// Token: 0x04001A20 RID: 6688
		public readonly Identifier AfflictionType;

		// Token: 0x04001A21 RID: 6689
		public readonly bool LimbSpecific;

		// Token: 0x04001A22 RID: 6690
		public readonly LimbType IndicatorLimb;

		// Token: 0x04001A23 RID: 6691
		public readonly Identifier TranslationIdentifier;

		// Token: 0x04001A24 RID: 6692
		public readonly bool IsBuff;

		// Token: 0x04001A25 RID: 6693
		public readonly bool AffectedByAttackMultipliers;

		// Token: 0x04001A26 RID: 6694
		public readonly bool AffectMachines;

		// Token: 0x04001A27 RID: 6695
		public readonly bool HealableInMedicalClinic;

		// Token: 0x04001A28 RID: 6696
		public readonly float HealCostMultiplier;

		// Token: 0x04001A29 RID: 6697
		public readonly int BaseHealCost;

		// Token: 0x04001A2A RID: 6698
		public readonly bool ShowBarInHealthMenu;

		// Token: 0x04001A2B RID: 6699
		public readonly bool HideIconAfterDelay;

		// Token: 0x04001A2C RID: 6700
		public readonly float ActivationThreshold;

		// Token: 0x04001A2D RID: 6701
		public readonly float ShowIconThreshold = 0.05f;

		// Token: 0x04001A2E RID: 6702
		public readonly float ShowIconToOthersThreshold = 0.05f;

		// Token: 0x04001A2F RID: 6703
		public readonly float MaxStrength = 100f;

		// Token: 0x04001A30 RID: 6704
		public readonly float GrainBurst;

		// Token: 0x04001A31 RID: 6705
		public readonly float ShowInHealthScannerThreshold;

		// Token: 0x04001A32 RID: 6706
		public readonly float TreatmentThreshold;

		// Token: 0x04001A33 RID: 6707
		public readonly float TreatmentSuggestionThreshold;

		// Token: 0x04001A34 RID: 6708
		public readonly bool VitalityLossRequiredForTreatment;

		// Token: 0x04001A35 RID: 6709
		public ImmutableHashSet<Identifier> IgnoreTreatmentIfAfflictedBy;

		// Token: 0x04001A36 RID: 6710
		public readonly float Duration;

		// Token: 0x04001A37 RID: 6711
		public float KarmaChangeOnApplied;

		// Token: 0x04001A38 RID: 6712
		public readonly float BurnOverlayAlpha;

		// Token: 0x04001A39 RID: 6713
		public readonly float DamageOverlayAlpha;

		// Token: 0x04001A3A RID: 6714
		public readonly Identifier AchievementOnReceived;

		// Token: 0x04001A3B RID: 6715
		public readonly Identifier AchievementOnRemoved;

		// Token: 0x04001A3C RID: 6716
		public readonly Color[] IconColors;

		// Token: 0x04001A3D RID: 6717
		public readonly bool AfflictionOverlayAlphaIsLinear;

		// Token: 0x04001A3E RID: 6718
		public readonly bool ResetBetweenRounds;

		// Token: 0x04001A3F RID: 6719
		public readonly bool DamageParticles;

		// Token: 0x04001A40 RID: 6720
		public readonly float MedicalSkillGain;

		// Token: 0x04001A41 RID: 6721
		public readonly float WeaponsSkillGain;

		// Token: 0x04001A43 RID: 6723
		private readonly List<AfflictionPrefab.Effect> effects = new List<AfflictionPrefab.Effect>();

		// Token: 0x04001A44 RID: 6724
		private readonly List<AfflictionPrefab.PeriodicEffect> periodicEffects = new List<AfflictionPrefab.PeriodicEffect>();

		// Token: 0x04001A45 RID: 6725
		private readonly ConstructorInfo constructor;

		// Token: 0x04001A46 RID: 6726
		public readonly Sprite Icon;

		// Token: 0x04001A47 RID: 6727
		public readonly Sprite AfflictionOverlay;

		// Token: 0x02000EAE RID: 3758
		public sealed class Effect
		{
			// Token: 0x17001B14 RID: 6932
			// (get) Token: 0x060084E5 RID: 34021 RVA: 0x003A0C83 File Offset: 0x0039EE83
			// (set) Token: 0x060084E6 RID: 34022 RVA: 0x003A0C8B File Offset: 0x0039EE8B
			[Serialize(0f, IsPropertySaveable.No, "Minimum affliction strength required for this effect to be active.", "", false)]
			public float MinStrength { get; private set; }

			// Token: 0x17001B15 RID: 6933
			// (get) Token: 0x060084E7 RID: 34023 RVA: 0x003A0C94 File Offset: 0x0039EE94
			// (set) Token: 0x060084E8 RID: 34024 RVA: 0x003A0C9C File Offset: 0x0039EE9C
			[Serialize(0f, IsPropertySaveable.No, "Maximum affliction strength for which this effect will be active.", "", false)]
			public float MaxStrength { get; private set; }

			// Token: 0x17001B16 RID: 6934
			// (get) Token: 0x060084E9 RID: 34025 RVA: 0x003A0CA5 File Offset: 0x0039EEA5
			// (set) Token: 0x060084EA RID: 34026 RVA: 0x003A0CAD File Offset: 0x0039EEAD
			[Serialize(0f, IsPropertySaveable.No, "The amount of vitality that is lost at this effect's lowest strength.", "", false)]
			public float MinVitalityDecrease { get; private set; }

			// Token: 0x17001B17 RID: 6935
			// (get) Token: 0x060084EB RID: 34027 RVA: 0x003A0CB6 File Offset: 0x0039EEB6
			// (set) Token: 0x060084EC RID: 34028 RVA: 0x003A0CBE File Offset: 0x0039EEBE
			[Serialize(0f, IsPropertySaveable.No, "The amount of vitality that is lost at this effect's highest strength.", "", false)]
			public float MaxVitalityDecrease { get; private set; }

			// Token: 0x17001B18 RID: 6936
			// (get) Token: 0x060084ED RID: 34029 RVA: 0x003A0CC7 File Offset: 0x0039EEC7
			// (set) Token: 0x060084EE RID: 34030 RVA: 0x003A0CCF File Offset: 0x0039EECF
			[Serialize(0f, IsPropertySaveable.No, "How much the affliction's strength changes every second while this effect is active.", "", false)]
			public float StrengthChange { get; private set; }

			// Token: 0x17001B19 RID: 6937
			// (get) Token: 0x060084EF RID: 34031 RVA: 0x003A0CD8 File Offset: 0x0039EED8
			// (set) Token: 0x060084F0 RID: 34032 RVA: 0x003A0CE0 File Offset: 0x0039EEE0
			[Serialize(false, IsPropertySaveable.No, "If set to true, MinVitalityDecrease and MaxVitalityDecrease represent a fraction of the affected character's maximum vitality, with 1 meaning 100%, instead of the same amount for all species.", "", false)]
			public bool MultiplyByMaxVitality { get; private set; }

			// Token: 0x17001B1A RID: 6938
			// (get) Token: 0x060084F1 RID: 34033 RVA: 0x003A0CE9 File Offset: 0x0039EEE9
			// (set) Token: 0x060084F2 RID: 34034 RVA: 0x003A0CF1 File Offset: 0x0039EEF1
			[Serialize(0f, IsPropertySaveable.No, "Blur effect strength at this effect's lowest strength.", "", false)]
			public float MinScreenBlur { get; private set; }

			// Token: 0x17001B1B RID: 6939
			// (get) Token: 0x060084F3 RID: 34035 RVA: 0x003A0CFA File Offset: 0x0039EEFA
			// (set) Token: 0x060084F4 RID: 34036 RVA: 0x003A0D02 File Offset: 0x0039EF02
			[Serialize(0f, IsPropertySaveable.No, "Blur effect strength at this effect's highest strength.", "", false)]
			public float MaxScreenBlur { get; private set; }

			// Token: 0x17001B1C RID: 6940
			// (get) Token: 0x060084F5 RID: 34037 RVA: 0x003A0D0B File Offset: 0x0039EF0B
			// (set) Token: 0x060084F6 RID: 34038 RVA: 0x003A0D13 File Offset: 0x0039EF13
			[Serialize(0f, IsPropertySaveable.No, "Generic distortion effect strength at this effect's lowest strength.", "", false)]
			public float MinScreenDistort { get; private set; }

			// Token: 0x17001B1D RID: 6941
			// (get) Token: 0x060084F7 RID: 34039 RVA: 0x003A0D1C File Offset: 0x0039EF1C
			// (set) Token: 0x060084F8 RID: 34040 RVA: 0x003A0D24 File Offset: 0x0039EF24
			[Serialize(0f, IsPropertySaveable.No, "Generic distortion effect strength at this effect's highest strength.", "", false)]
			public float MaxScreenDistort { get; private set; }

			// Token: 0x17001B1E RID: 6942
			// (get) Token: 0x060084F9 RID: 34041 RVA: 0x003A0D2D File Offset: 0x0039EF2D
			// (set) Token: 0x060084FA RID: 34042 RVA: 0x003A0D35 File Offset: 0x0039EF35
			[Serialize(0f, IsPropertySaveable.No, "Radial distortion effect strength at this effect's lowest strength.", "", false)]
			public float MinRadialDistort { get; private set; }

			// Token: 0x17001B1F RID: 6943
			// (get) Token: 0x060084FB RID: 34043 RVA: 0x003A0D3E File Offset: 0x0039EF3E
			// (set) Token: 0x060084FC RID: 34044 RVA: 0x003A0D46 File Offset: 0x0039EF46
			[Serialize(0f, IsPropertySaveable.No, "Radial distortion effect strength at this effect's highest strength.", "", false)]
			public float MaxRadialDistort { get; private set; }

			// Token: 0x17001B20 RID: 6944
			// (get) Token: 0x060084FD RID: 34045 RVA: 0x003A0D4F File Offset: 0x0039EF4F
			// (set) Token: 0x060084FE RID: 34046 RVA: 0x003A0D57 File Offset: 0x0039EF57
			[Serialize(0f, IsPropertySaveable.No, "Chromatic aberration effect strength at this effect's lowest strength.", "", false)]
			public float MinChromaticAberration { get; private set; }

			// Token: 0x17001B21 RID: 6945
			// (get) Token: 0x060084FF RID: 34047 RVA: 0x003A0D60 File Offset: 0x0039EF60
			// (set) Token: 0x06008500 RID: 34048 RVA: 0x003A0D68 File Offset: 0x0039EF68
			[Serialize(0f, IsPropertySaveable.No, "Chromatic aberration effect strength at this effect's highest strength.", "", false)]
			public float MaxChromaticAberration { get; private set; }

			// Token: 0x17001B22 RID: 6946
			// (get) Token: 0x06008501 RID: 34049 RVA: 0x003A0D71 File Offset: 0x0039EF71
			// (set) Token: 0x06008502 RID: 34050 RVA: 0x003A0D79 File Offset: 0x0039EF79
			[Serialize("255,255,255,255", IsPropertySaveable.No, "Radiation grain effect color.", "", false)]
			public Color GrainColor { get; private set; }

			// Token: 0x17001B23 RID: 6947
			// (get) Token: 0x06008503 RID: 34051 RVA: 0x003A0D82 File Offset: 0x0039EF82
			// (set) Token: 0x06008504 RID: 34052 RVA: 0x003A0D8A File Offset: 0x0039EF8A
			[Serialize(0f, IsPropertySaveable.No, "Radiation grain effect strength at this effect's lowest strength.", "", false)]
			public float MinGrainStrength { get; private set; }

			// Token: 0x17001B24 RID: 6948
			// (get) Token: 0x06008505 RID: 34053 RVA: 0x003A0D93 File Offset: 0x0039EF93
			// (set) Token: 0x06008506 RID: 34054 RVA: 0x003A0D9B File Offset: 0x0039EF9B
			[Serialize(0f, IsPropertySaveable.No, "Radiation grain effect strength at this effect's highest strength.", "", false)]
			public float MaxGrainStrength { get; private set; }

			// Token: 0x17001B25 RID: 6949
			// (get) Token: 0x06008507 RID: 34055 RVA: 0x003A0DA4 File Offset: 0x0039EFA4
			// (set) Token: 0x06008508 RID: 34056 RVA: 0x003A0DAC File Offset: 0x0039EFAC
			[Serialize(0f, IsPropertySaveable.No, "The maximum rate of fluctuation to apply to visual effects caused by this affliction effect. Effective fluctuation is proportional to the affliction's current strength.", "", false)]
			public float ScreenEffectFluctuationFrequency { get; private set; }

			// Token: 0x17001B26 RID: 6950
			// (get) Token: 0x06008509 RID: 34057 RVA: 0x003A0DB5 File Offset: 0x0039EFB5
			// (set) Token: 0x0600850A RID: 34058 RVA: 0x003A0DBD File Offset: 0x0039EFBD
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for the affliction overlay's opacity at this effect's lowest strength. See the list of elements for more details.", "", false)]
			public float MinAfflictionOverlayAlphaMultiplier { get; private set; }

			// Token: 0x17001B27 RID: 6951
			// (get) Token: 0x0600850B RID: 34059 RVA: 0x003A0DC6 File Offset: 0x0039EFC6
			// (set) Token: 0x0600850C RID: 34060 RVA: 0x003A0DCE File Offset: 0x0039EFCE
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for the affliction overlay's opacity at this effect's highest strength. See the list of elements for more details.", "", false)]
			public float MaxAfflictionOverlayAlphaMultiplier { get; private set; }

			// Token: 0x17001B28 RID: 6952
			// (get) Token: 0x0600850D RID: 34061 RVA: 0x003A0DD7 File Offset: 0x0039EFD7
			// (set) Token: 0x0600850E RID: 34062 RVA: 0x003A0DDF File Offset: 0x0039EFDF
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for every buff's decay rate at this effect's lowest strength. Only applies to afflictions of class BuffDurationIncrease.", "", false)]
			public float MinBuffMultiplier { get; private set; }

			// Token: 0x17001B29 RID: 6953
			// (get) Token: 0x0600850F RID: 34063 RVA: 0x003A0DE8 File Offset: 0x0039EFE8
			// (set) Token: 0x06008510 RID: 34064 RVA: 0x003A0DF0 File Offset: 0x0039EFF0
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for every buff's decay rate at this effect's highest strength. Only applies to afflictions of class BuffDurationIncrease.", "", false)]
			public float MaxBuffMultiplier { get; private set; }

			// Token: 0x17001B2A RID: 6954
			// (get) Token: 0x06008511 RID: 34065 RVA: 0x003A0DF9 File Offset: 0x0039EFF9
			// (set) Token: 0x06008512 RID: 34066 RVA: 0x003A0E01 File Offset: 0x0039F001
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to the affected character's speed at this effect's lowest strength.", "", false)]
			public float MinSpeedMultiplier { get; private set; }

			// Token: 0x17001B2B RID: 6955
			// (get) Token: 0x06008513 RID: 34067 RVA: 0x003A0E0A File Offset: 0x0039F00A
			// (set) Token: 0x06008514 RID: 34068 RVA: 0x003A0E12 File Offset: 0x0039F012
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to the affected character's speed at this effect's highest strength.", "", false)]
			public float MaxSpeedMultiplier { get; private set; }

			// Token: 0x17001B2C RID: 6956
			// (get) Token: 0x06008515 RID: 34069 RVA: 0x003A0E1B File Offset: 0x0039F01B
			// (set) Token: 0x06008516 RID: 34070 RVA: 0x003A0E23 File Offset: 0x0039F023
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to all of the affected character's skill levels at this effect's lowest strength.", "", false)]
			public float MinSkillMultiplier { get; private set; }

			// Token: 0x17001B2D RID: 6957
			// (get) Token: 0x06008517 RID: 34071 RVA: 0x003A0E2C File Offset: 0x0039F02C
			// (set) Token: 0x06008518 RID: 34072 RVA: 0x003A0E34 File Offset: 0x0039F034
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to all of the affected character's skill levels at this effect's highest strength.", "", false)]
			public float MaxSkillMultiplier { get; private set; }

			// Token: 0x17001B2E RID: 6958
			// (get) Token: 0x06008519 RID: 34073 RVA: 0x003A0E3D File Offset: 0x0039F03D
			// (set) Token: 0x0600851A RID: 34074 RVA: 0x003A0E45 File Offset: 0x0039F045
			[Serialize(0f, IsPropertySaveable.No, "The amount of resistance to the afflictions specified by ResistanceFor to apply at this effect's lowest strength.", "", false)]
			public float MinResistance { get; private set; }

			// Token: 0x17001B2F RID: 6959
			// (get) Token: 0x0600851B RID: 34075 RVA: 0x003A0E4E File Offset: 0x0039F04E
			// (set) Token: 0x0600851C RID: 34076 RVA: 0x003A0E56 File Offset: 0x0039F056
			[Serialize(0f, IsPropertySaveable.No, "The amount of resistance to the afflictions specified by ResistanceFor to apply at this effect's highest strength.", "", false)]
			public float MaxResistance { get; private set; }

			// Token: 0x17001B30 RID: 6960
			// (get) Token: 0x0600851D RID: 34077 RVA: 0x003A0E5F File Offset: 0x0039F05F
			// (set) Token: 0x0600851E RID: 34078 RVA: 0x003A0E67 File Offset: 0x0039F067
			[Serialize("", IsPropertySaveable.No, "Identifier used by AI to determine conversation lines to say when this effect is active.", "", false)]
			public Identifier DialogFlag { get; private set; }

			// Token: 0x17001B31 RID: 6961
			// (get) Token: 0x0600851F RID: 34079 RVA: 0x003A0E70 File Offset: 0x0039F070
			// (set) Token: 0x06008520 RID: 34080 RVA: 0x003A0E78 File Offset: 0x0039F078
			[Serialize("", IsPropertySaveable.No, "Tag that enemy AI may use to target the affected character when this effect is active.", "", false)]
			public Identifier Tag { get; private set; }

			// Token: 0x17001B32 RID: 6962
			// (get) Token: 0x06008521 RID: 34081 RVA: 0x003A0E81 File Offset: 0x0039F081
			// (set) Token: 0x06008522 RID: 34082 RVA: 0x003A0E89 File Offset: 0x0039F089
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's face with at this effect's lowest strength. The alpha channel is used to determine how much to tint the character's face.", "", false)]
			public Color MinFaceTint { get; private set; }

			// Token: 0x17001B33 RID: 6963
			// (get) Token: 0x06008523 RID: 34083 RVA: 0x003A0E92 File Offset: 0x0039F092
			// (set) Token: 0x06008524 RID: 34084 RVA: 0x003A0E9A File Offset: 0x0039F09A
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's face with at this effect's highest strength. The alpha channel is used to determine how much to tint the character's face.", "", false)]
			public Color MaxFaceTint { get; private set; }

			// Token: 0x17001B34 RID: 6964
			// (get) Token: 0x06008525 RID: 34085 RVA: 0x003A0EA3 File Offset: 0x0039F0A3
			// (set) Token: 0x06008526 RID: 34086 RVA: 0x003A0EAB File Offset: 0x0039F0AB
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's entire body with at this effect's lowest strength. The alpha channel is used to determine how much to tint the character.", "", false)]
			public Color MinBodyTint { get; private set; }

			// Token: 0x17001B35 RID: 6965
			// (get) Token: 0x06008527 RID: 34087 RVA: 0x003A0EB4 File Offset: 0x0039F0B4
			// (set) Token: 0x06008528 RID: 34088 RVA: 0x003A0EBC File Offset: 0x0039F0BC
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's entire body with at this effect's highest strength. The alpha channel is used to determine how much to tint the character.", "", false)]
			public Color MaxBodyTint { get; private set; }

			// Token: 0x17001B36 RID: 6966
			// (get) Token: 0x06008529 RID: 34089 RVA: 0x003A0EC5 File Offset: 0x0039F0C5
			// (set) Token: 0x0600852A RID: 34090 RVA: 0x003A0ECD File Offset: 0x0039F0CD
			[Serialize(0f, IsPropertySaveable.No, "Range of the \"thermal goggles overlay\" enabled by the affliction.", "", false)]
			public float ThermalOverlayRange { get; private set; }

			// Token: 0x17001B37 RID: 6967
			// (get) Token: 0x0600852B RID: 34091 RVA: 0x003A0ED6 File Offset: 0x0039F0D6
			// (set) Token: 0x0600852C RID: 34092 RVA: 0x003A0EDE File Offset: 0x0039F0DE
			[Serialize("255,0,0,255", IsPropertySaveable.No, "Color of the \"thermal goggles overlay\" enabled by the affliction. Only has an effect if ThermalOverlayRange is larger than 0.", "", false)]
			public Color ThermalOverlayColor { get; private set; }

			// Token: 0x17001B38 RID: 6968
			// (get) Token: 0x0600852D RID: 34093 RVA: 0x003A0EE7 File Offset: 0x0039F0E7
			// (set) Token: 0x0600852E RID: 34094 RVA: 0x003A0EEF File Offset: 0x0039F0EF
			[Serialize(0f, IsPropertySaveable.No, "Multiplier for the convulsion/seizure effect on the character's ragdoll when this effect is active.", "", false)]
			public float ConvulseAmount { get; private set; }

			// Token: 0x0600852F RID: 34095 RVA: 0x003A0EF8 File Offset: 0x0039F0F8
			public Effect(ContentXElement element, string parentDebugName)
			{
				SerializableProperty.DeserializeProperties(this, element);
				this.ResistanceFor = element.GetAttributeIdentifierArray("resistancefor", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
				this.ResistanceLimbs = element.GetAttributeEnumArray<LimbType>("resistancelimbs", Array.Empty<LimbType>()).ToImmutableArray<LimbType>();
				this.BlockTransformation = element.GetAttributeIdentifierArray("blocktransformation", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
				Dictionary<StatTypes, AfflictionPrefab.Effect.AppliedStatValue> afflictionStatValues = new Dictionary<StatTypes, AfflictionPrefab.Effect.AppliedStatValue>();
				List<StatusEffect> statusEffects = new List<StatusEffect>();
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "statuseffect"))
					{
						if (!(a == "statvalue"))
						{
							if (!(a == "abilityflag"))
							{
								if (a == "affliction")
								{
									DebugConsole.AddWarning("Error in affliction \"" + parentDebugName + "\" - additional afflictions caused by the affliction should be configured inside status effects.", element.ContentPackage);
								}
							}
							else
							{
								ContentXElement contentXElement = subElement;
								string key = "flagtype";
								AbilityFlags abilityFlags = AbilityFlags.None;
								AbilityFlags flagType = contentXElement.GetAttributeEnum<AbilityFlags>(key, abilityFlags);
								if (flagType == AbilityFlags.None)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
									defaultInterpolatedStringHandler.AppendLiteral("Error in affliction \"");
									defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
									defaultInterpolatedStringHandler.AppendLiteral("\" - invalid ability flag type \"");
									defaultInterpolatedStringHandler.AppendFormatted(subElement.GetAttributeString("flagtype", ""));
									defaultInterpolatedStringHandler.AppendLiteral("\".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
								}
								else
								{
									this.AfflictionAbilityFlags |= flagType;
								}
							}
						}
						else
						{
							AfflictionPrefab.Effect.AppliedStatValue newStatValue = new AfflictionPrefab.Effect.AppliedStatValue(subElement);
							if (newStatValue.StatType == StatTypes.None || !afflictionStatValues.TryAdd(newStatValue.StatType, newStatValue))
							{
								DebugConsole.ThrowError("Invalid stat value in the affliction \"" + parentDebugName + "\".", null, element.ContentPackage, false, false);
							}
						}
					}
					else
					{
						statusEffects.Add(StatusEffect.Load(subElement, parentDebugName));
					}
				}
				this.AfflictionStatValues = afflictionStatValues.ToImmutableDictionary<StatTypes, AfflictionPrefab.Effect.AppliedStatValue>();
				this.StatusEffects = statusEffects.ToImmutableArray<StatusEffect>();
			}

			// Token: 0x06008530 RID: 34096 RVA: 0x003A113C File Offset: 0x0039F33C
			public float GetStrengthFactor(Affliction affliction)
			{
				return this.GetStrengthFactor(affliction.Strength);
			}

			// Token: 0x06008531 RID: 34097 RVA: 0x003A114A File Offset: 0x0039F34A
			public float GetStrengthFactor(float strength)
			{
				return MathUtils.InverseLerp(this.MinStrength, this.MaxStrength, strength);
			}

			// Token: 0x040052FD RID: 21245
			public readonly ImmutableArray<Identifier> ResistanceFor;

			// Token: 0x040052FE RID: 21246
			public readonly ImmutableArray<LimbType> ResistanceLimbs;

			// Token: 0x0400530A RID: 21258
			public readonly ImmutableArray<Identifier> BlockTransformation;

			// Token: 0x0400530B RID: 21259
			public readonly ImmutableDictionary<StatTypes, AfflictionPrefab.Effect.AppliedStatValue> AfflictionStatValues;

			// Token: 0x0400530C RID: 21260
			public readonly AbilityFlags AfflictionAbilityFlags;

			// Token: 0x0400530D RID: 21261
			public readonly ImmutableArray<StatusEffect> StatusEffects;

			// Token: 0x02001553 RID: 5459
			public readonly struct AppliedStatValue
			{
				// Token: 0x06009D7F RID: 40319 RVA: 0x003ED1D8 File Offset: 0x003EB3D8
				public AppliedStatValue(ContentXElement element)
				{
					this.Value = element.GetAttributeFloat("value", 0f);
					string key = "stattype";
					StatTypes statTypes = StatTypes.None;
					this.StatType = element.GetAttributeEnum<StatTypes>(key, statTypes);
					this.MinValue = element.GetAttributeFloat("minvalue", this.Value);
					this.MaxValue = element.GetAttributeFloat("maxvalue", this.Value);
				}

				// Token: 0x0400681C RID: 26652
				public readonly StatTypes StatType;

				// Token: 0x0400681D RID: 26653
				public readonly float MinValue;

				// Token: 0x0400681E RID: 26654
				public readonly float MaxValue;

				// Token: 0x0400681F RID: 26655
				private readonly float Value;
			}
		}

		// Token: 0x02000EAF RID: 3759
		public sealed class Description
		{
			// Token: 0x06008532 RID: 34098 RVA: 0x003A1160 File Offset: 0x0039F360
			public Description(ContentXElement element, AfflictionPrefab affliction)
			{
				this.TextTag = element.GetAttributeIdentifier("textidentifier", Identifier.Empty);
				if (!this.TextTag.IsEmpty)
				{
					this.Text = TextManager.Get(this.TextTag);
				}
				string text = element.GetAttributeString("text", string.Empty);
				if (!text.IsNullOrEmpty())
				{
					LocalizedString text2 = this.Text;
					this.Text = (((text2 != null) ? text2.Fallback(text, true) : null) ?? text);
				}
				else if (this.TextTag.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Error in affliction \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(affliction.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" - no text defined for one of the descriptions.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				this.MinStrength = element.GetAttributeFloat("MinStrength", 0f);
				this.MaxStrength = element.GetAttributeFloat("MaxStrength", 100f);
				if (this.MinStrength >= this.MaxStrength)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(61, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in affliction \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(affliction.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\" - max strength is not larger than min.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
				string key = "Target";
				AfflictionPrefab.Description.TargetType targetType = AfflictionPrefab.Description.TargetType.Any;
				this.Target = element.GetAttributeEnum<AfflictionPrefab.Description.TargetType>(key, targetType);
			}

			// Token: 0x0400530E RID: 21262
			public readonly LocalizedString Text;

			// Token: 0x0400530F RID: 21263
			public readonly Identifier TextTag;

			// Token: 0x04005310 RID: 21264
			public readonly float MinStrength;

			// Token: 0x04005311 RID: 21265
			public readonly float MaxStrength;

			// Token: 0x04005312 RID: 21266
			public readonly AfflictionPrefab.Description.TargetType Target;

			// Token: 0x02001554 RID: 5460
			public enum TargetType
			{
				// Token: 0x04006821 RID: 26657
				Any,
				// Token: 0x04006822 RID: 26658
				Self,
				// Token: 0x04006823 RID: 26659
				OtherCharacter
			}
		}

		// Token: 0x02000EB0 RID: 3760
		public sealed class PeriodicEffect
		{
			// Token: 0x06008533 RID: 34099 RVA: 0x003A12D0 File Offset: 0x0039F4D0
			public PeriodicEffect(ContentXElement element, string parentDebugName)
			{
				foreach (ContentXElement subElement in element.Elements())
				{
					this.StatusEffects.Add(StatusEffect.Load(subElement, parentDebugName));
				}
				if (element.GetAttribute("interval") != null)
				{
					this.MinInterval = (this.MaxInterval = Math.Max(element.GetAttributeFloat("interval", 1f), 1f));
					return;
				}
				this.MinInterval = Math.Max(element.GetAttributeFloat("MinInterval", 1f), 0.1f);
				this.MaxInterval = Math.Max(element.GetAttributeFloat("MaxInterval", 1f), this.MinInterval);
				this.MinStrength = Math.Max(element.GetAttributeFloat("MinStrength", 0f), 0f);
				this.MaxStrength = Math.Max(element.GetAttributeFloat("MaxStrength", this.MinStrength), this.MinStrength);
			}

			// Token: 0x04005313 RID: 21267
			public readonly List<StatusEffect> StatusEffects = new List<StatusEffect>();

			// Token: 0x04005314 RID: 21268
			public readonly float MinInterval;

			// Token: 0x04005315 RID: 21269
			public readonly float MaxInterval;

			// Token: 0x04005316 RID: 21270
			public readonly float MinStrength;

			// Token: 0x04005317 RID: 21271
			public readonly float MaxStrength;
		}
	}
}
