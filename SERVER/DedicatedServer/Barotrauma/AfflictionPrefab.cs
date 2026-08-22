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
	// Token: 0x020000C8 RID: 200
	internal class AfflictionPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000671 RID: 1649
		// (get) Token: 0x06001632 RID: 5682 RVA: 0x000BC07B File Offset: 0x000BA27B
		public static AfflictionPrefab InternalDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["internaldamage"];
			}
		}

		// Token: 0x17000672 RID: 1650
		// (get) Token: 0x06001633 RID: 5683 RVA: 0x000BC08C File Offset: 0x000BA28C
		public static AfflictionPrefab BiteWounds
		{
			get
			{
				return AfflictionPrefab.Prefabs["bitewounds"];
			}
		}

		// Token: 0x17000673 RID: 1651
		// (get) Token: 0x06001634 RID: 5684 RVA: 0x000BC09D File Offset: 0x000BA29D
		public static AfflictionPrefab ImpactDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["blunttrauma"];
			}
		}

		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x06001635 RID: 5685 RVA: 0x000BC0AE File Offset: 0x000BA2AE
		public static AfflictionPrefab Bleeding
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.BleedingType];
			}
		}

		// Token: 0x17000675 RID: 1653
		// (get) Token: 0x06001636 RID: 5686 RVA: 0x000BC0BF File Offset: 0x000BA2BF
		public static AfflictionPrefab Burn
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.BurnType];
			}
		}

		// Token: 0x17000676 RID: 1654
		// (get) Token: 0x06001637 RID: 5687 RVA: 0x000BC0D0 File Offset: 0x000BA2D0
		public static AfflictionPrefab OxygenLow
		{
			get
			{
				return AfflictionPrefab.Prefabs["oxygenlow"];
			}
		}

		// Token: 0x17000677 RID: 1655
		// (get) Token: 0x06001638 RID: 5688 RVA: 0x000BC0E1 File Offset: 0x000BA2E1
		public static AfflictionPrefab Bloodloss
		{
			get
			{
				return AfflictionPrefab.Prefabs["bloodloss"];
			}
		}

		// Token: 0x17000678 RID: 1656
		// (get) Token: 0x06001639 RID: 5689 RVA: 0x000BC0F2 File Offset: 0x000BA2F2
		public static AfflictionPrefab Pressure
		{
			get
			{
				return AfflictionPrefab.Prefabs["pressure"];
			}
		}

		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x0600163A RID: 5690 RVA: 0x000BC103 File Offset: 0x000BA303
		public static AfflictionPrefab OrganDamage
		{
			get
			{
				return AfflictionPrefab.Prefabs["organdamage"];
			}
		}

		// Token: 0x1700067A RID: 1658
		// (get) Token: 0x0600163B RID: 5691 RVA: 0x000BC114 File Offset: 0x000BA314
		public static AfflictionPrefab Stun
		{
			get
			{
				return AfflictionPrefab.Prefabs[AfflictionPrefab.StunType];
			}
		}

		// Token: 0x1700067B RID: 1659
		// (get) Token: 0x0600163C RID: 5692 RVA: 0x000BC125 File Offset: 0x000BA325
		public static AfflictionPrefab RadiationSickness
		{
			get
			{
				return AfflictionPrefab.Prefabs["radiationsickness"];
			}
		}

		// Token: 0x1700067C RID: 1660
		// (get) Token: 0x0600163D RID: 5693 RVA: 0x000BC136 File Offset: 0x000BA336
		public static AfflictionPrefab HuskInfection
		{
			get
			{
				return AfflictionPrefab.Prefabs["huskinfection"];
			}
		}

		// Token: 0x1700067D RID: 1661
		// (get) Token: 0x0600163E RID: 5694 RVA: 0x000BC147 File Offset: 0x000BA347
		public static AfflictionPrefab JovianRadiation
		{
			get
			{
				return AfflictionPrefab.Prefabs["jovianradiation"];
			}
		}

		// Token: 0x1700067E RID: 1662
		// (get) Token: 0x0600163F RID: 5695 RVA: 0x000BC158 File Offset: 0x000BA358
		public static IEnumerable<AfflictionPrefab> List
		{
			get
			{
				return AfflictionPrefab.Prefabs;
			}
		}

		// Token: 0x06001640 RID: 5696 RVA: 0x000BC15F File Offset: 0x000BA35F
		public override void Dispose()
		{
		}

		// Token: 0x1700067F RID: 1663
		// (get) Token: 0x06001641 RID: 5697 RVA: 0x000BC161 File Offset: 0x000BA361
		// (set) Token: 0x06001642 RID: 5698 RVA: 0x000BC169 File Offset: 0x000BA369
		public Identifier[] TargetSpecies { get; protected set; }

		// Token: 0x17000680 RID: 1664
		// (get) Token: 0x06001643 RID: 5699 RVA: 0x000BC172 File Offset: 0x000BA372
		public IEnumerable<AfflictionPrefab.Effect> Effects
		{
			get
			{
				return this.effects;
			}
		}

		// Token: 0x17000681 RID: 1665
		// (get) Token: 0x06001644 RID: 5700 RVA: 0x000BC17A File Offset: 0x000BA37A
		public IList<AfflictionPrefab.PeriodicEffect> PeriodicEffects
		{
			get
			{
				return this.periodicEffects;
			}
		}

		// Token: 0x17000682 RID: 1666
		// (get) Token: 0x06001645 RID: 5701 RVA: 0x000BC182 File Offset: 0x000BA382
		// (set) Token: 0x06001646 RID: 5702 RVA: 0x000BC18A File Offset: 0x000BA38A
		public float AfflictionOverlayAnimSpeed { get; set; }

		// Token: 0x17000683 RID: 1667
		// (get) Token: 0x06001647 RID: 5703 RVA: 0x000BC193 File Offset: 0x000BA393
		// (set) Token: 0x06001648 RID: 5704 RVA: 0x000BC19B File Offset: 0x000BA39B
		public ImmutableDictionary<Identifier, float> TreatmentSuitabilities { get; private set; } = new Dictionary<Identifier, float>().ToImmutableDictionary<Identifier, float>();

		// Token: 0x17000684 RID: 1668
		// (get) Token: 0x06001649 RID: 5705 RVA: 0x000BC1A4 File Offset: 0x000BA3A4
		// (set) Token: 0x0600164A RID: 5706 RVA: 0x000BC1AC File Offset: 0x000BA3AC
		public bool HasTreatments { get; private set; }

		// Token: 0x0600164B RID: 5707 RVA: 0x000BC1B8 File Offset: 0x000BA3B8
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

		// Token: 0x0600164C RID: 5708 RVA: 0x000BCB2C File Offset: 0x000BAD2C
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

		// Token: 0x0600164D RID: 5709 RVA: 0x000BCBE8 File Offset: 0x000BADE8
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

		// Token: 0x0600164E RID: 5710 RVA: 0x000BCC78 File Offset: 0x000BAE78
		public static void LoadAllEffectsAndTreatmentSuitabilities()
		{
			foreach (AfflictionPrefab prefab in AfflictionPrefab.Prefabs)
			{
				prefab.RefreshTreatmentSuitabilities();
				prefab.LoadEffects();
			}
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x000BCCCC File Offset: 0x000BAECC
		public static void ClearAllEffects()
		{
			AfflictionPrefab.Prefabs.ForEach(delegate(AfflictionPrefab p)
			{
				p.ClearEffects();
			});
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x000BCCF8 File Offset: 0x000BAEF8
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

		// Token: 0x06001651 RID: 5713 RVA: 0x000BCE70 File Offset: 0x000BB070
		private void ClearEffects()
		{
			this.effects.Clear();
			this.periodicEffects.Clear();
		}

		// Token: 0x06001652 RID: 5714 RVA: 0x000BCE88 File Offset: 0x000BB088
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
			defaultInterpolatedStringHandler.AppendLiteral("AfflictionPrefab (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001653 RID: 5715 RVA: 0x000BCECC File Offset: 0x000BB0CC
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

		// Token: 0x06001654 RID: 5716 RVA: 0x000BCF44 File Offset: 0x000BB144
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

		// Token: 0x06001655 RID: 5717 RVA: 0x000BD010 File Offset: 0x000BB210
		public float GetTreatmentSuitability(Item item)
		{
			if (item == null)
			{
				return 0f;
			}
			return Math.Max(item.Prefab.GetTreatmentSuitability(this.Identifier), item.Prefab.GetTreatmentSuitability(this.AfflictionType));
		}

		// Token: 0x04000AA2 RID: 2722
		public static readonly Identifier DamageType = "damage".ToIdentifier();

		// Token: 0x04000AA3 RID: 2723
		public static readonly Identifier BurnType = "burn".ToIdentifier();

		// Token: 0x04000AA4 RID: 2724
		public static readonly Identifier BleedingType = "bleeding".ToIdentifier();

		// Token: 0x04000AA5 RID: 2725
		public static readonly Identifier ParalysisType = "paralysis".ToIdentifier();

		// Token: 0x04000AA6 RID: 2726
		public static readonly Identifier PoisonType = "poison".ToIdentifier();

		// Token: 0x04000AA7 RID: 2727
		public static readonly Identifier StunType = "stun".ToIdentifier();

		// Token: 0x04000AA8 RID: 2728
		public static readonly Identifier EMPType = "emp".ToIdentifier();

		// Token: 0x04000AA9 RID: 2729
		public static readonly Identifier SpaceHerpesType = "spaceherpes".ToIdentifier();

		// Token: 0x04000AAA RID: 2730
		public static readonly Identifier AlienInfectionType = "alieninfection".ToIdentifier();

		// Token: 0x04000AAB RID: 2731
		public static readonly Identifier InvertControlsType = "invertcontrols".ToIdentifier();

		// Token: 0x04000AAC RID: 2732
		public static readonly Identifier DisguisedAsHuskType = "disguiseashusk".ToIdentifier();

		// Token: 0x04000AAD RID: 2733
		public static readonly PrefabCollection<AfflictionPrefab> Prefabs = new PrefabCollection<AfflictionPrefab>();

		// Token: 0x04000AAE RID: 2734
		private readonly ContentXElement configElement;

		// Token: 0x04000AAF RID: 2735
		public readonly LocalizedString Name;

		// Token: 0x04000AB0 RID: 2736
		public readonly LocalizedString CauseOfDeathDescription;

		// Token: 0x04000AB1 RID: 2737
		public readonly LocalizedString SelfCauseOfDeathDescription;

		// Token: 0x04000AB2 RID: 2738
		private readonly LocalizedString defaultDescription;

		// Token: 0x04000AB3 RID: 2739
		public readonly ImmutableList<AfflictionPrefab.Description> Descriptions;

		// Token: 0x04000AB4 RID: 2740
		public readonly bool ShowDescriptionInTooltip;

		// Token: 0x04000AB5 RID: 2741
		public readonly Identifier AfflictionType;

		// Token: 0x04000AB6 RID: 2742
		public readonly bool LimbSpecific;

		// Token: 0x04000AB7 RID: 2743
		public readonly LimbType IndicatorLimb;

		// Token: 0x04000AB8 RID: 2744
		public readonly Identifier TranslationIdentifier;

		// Token: 0x04000AB9 RID: 2745
		public readonly bool IsBuff;

		// Token: 0x04000ABA RID: 2746
		public readonly bool AffectedByAttackMultipliers;

		// Token: 0x04000ABB RID: 2747
		public readonly bool AffectMachines;

		// Token: 0x04000ABC RID: 2748
		public readonly bool HealableInMedicalClinic;

		// Token: 0x04000ABD RID: 2749
		public readonly float HealCostMultiplier;

		// Token: 0x04000ABE RID: 2750
		public readonly int BaseHealCost;

		// Token: 0x04000ABF RID: 2751
		public readonly bool ShowBarInHealthMenu;

		// Token: 0x04000AC0 RID: 2752
		public readonly bool HideIconAfterDelay;

		// Token: 0x04000AC1 RID: 2753
		public readonly float ActivationThreshold;

		// Token: 0x04000AC2 RID: 2754
		public readonly float ShowIconThreshold = 0.05f;

		// Token: 0x04000AC3 RID: 2755
		public readonly float ShowIconToOthersThreshold = 0.05f;

		// Token: 0x04000AC4 RID: 2756
		public readonly float MaxStrength = 100f;

		// Token: 0x04000AC5 RID: 2757
		public readonly float GrainBurst;

		// Token: 0x04000AC6 RID: 2758
		public readonly float ShowInHealthScannerThreshold;

		// Token: 0x04000AC7 RID: 2759
		public readonly float TreatmentThreshold;

		// Token: 0x04000AC8 RID: 2760
		public readonly float TreatmentSuggestionThreshold;

		// Token: 0x04000AC9 RID: 2761
		public readonly bool VitalityLossRequiredForTreatment;

		// Token: 0x04000ACA RID: 2762
		public ImmutableHashSet<Identifier> IgnoreTreatmentIfAfflictedBy;

		// Token: 0x04000ACB RID: 2763
		public readonly float Duration;

		// Token: 0x04000ACC RID: 2764
		public float KarmaChangeOnApplied;

		// Token: 0x04000ACD RID: 2765
		public readonly float BurnOverlayAlpha;

		// Token: 0x04000ACE RID: 2766
		public readonly float DamageOverlayAlpha;

		// Token: 0x04000ACF RID: 2767
		public readonly Identifier AchievementOnReceived;

		// Token: 0x04000AD0 RID: 2768
		public readonly Identifier AchievementOnRemoved;

		// Token: 0x04000AD1 RID: 2769
		public readonly Color[] IconColors;

		// Token: 0x04000AD2 RID: 2770
		public readonly bool AfflictionOverlayAlphaIsLinear;

		// Token: 0x04000AD3 RID: 2771
		public readonly bool ResetBetweenRounds;

		// Token: 0x04000AD4 RID: 2772
		public readonly bool DamageParticles;

		// Token: 0x04000AD5 RID: 2773
		public readonly float MedicalSkillGain;

		// Token: 0x04000AD6 RID: 2774
		public readonly float WeaponsSkillGain;

		// Token: 0x04000AD8 RID: 2776
		private readonly List<AfflictionPrefab.Effect> effects = new List<AfflictionPrefab.Effect>();

		// Token: 0x04000AD9 RID: 2777
		private readonly List<AfflictionPrefab.PeriodicEffect> periodicEffects = new List<AfflictionPrefab.PeriodicEffect>();

		// Token: 0x04000ADA RID: 2778
		private readonly ConstructorInfo constructor;

		// Token: 0x04000ADB RID: 2779
		public readonly Sprite Icon;

		// Token: 0x04000ADC RID: 2780
		public readonly Sprite AfflictionOverlay;

		// Token: 0x0200087A RID: 2170
		public sealed class Effect
		{
			// Token: 0x17001441 RID: 5185
			// (get) Token: 0x060054E2 RID: 21730 RVA: 0x001F1A37 File Offset: 0x001EFC37
			// (set) Token: 0x060054E3 RID: 21731 RVA: 0x001F1A3F File Offset: 0x001EFC3F
			[Serialize(0f, IsPropertySaveable.No, "Minimum affliction strength required for this effect to be active.", "", false)]
			public float MinStrength { get; private set; }

			// Token: 0x17001442 RID: 5186
			// (get) Token: 0x060054E4 RID: 21732 RVA: 0x001F1A48 File Offset: 0x001EFC48
			// (set) Token: 0x060054E5 RID: 21733 RVA: 0x001F1A50 File Offset: 0x001EFC50
			[Serialize(0f, IsPropertySaveable.No, "Maximum affliction strength for which this effect will be active.", "", false)]
			public float MaxStrength { get; private set; }

			// Token: 0x17001443 RID: 5187
			// (get) Token: 0x060054E6 RID: 21734 RVA: 0x001F1A59 File Offset: 0x001EFC59
			// (set) Token: 0x060054E7 RID: 21735 RVA: 0x001F1A61 File Offset: 0x001EFC61
			[Serialize(0f, IsPropertySaveable.No, "The amount of vitality that is lost at this effect's lowest strength.", "", false)]
			public float MinVitalityDecrease { get; private set; }

			// Token: 0x17001444 RID: 5188
			// (get) Token: 0x060054E8 RID: 21736 RVA: 0x001F1A6A File Offset: 0x001EFC6A
			// (set) Token: 0x060054E9 RID: 21737 RVA: 0x001F1A72 File Offset: 0x001EFC72
			[Serialize(0f, IsPropertySaveable.No, "The amount of vitality that is lost at this effect's highest strength.", "", false)]
			public float MaxVitalityDecrease { get; private set; }

			// Token: 0x17001445 RID: 5189
			// (get) Token: 0x060054EA RID: 21738 RVA: 0x001F1A7B File Offset: 0x001EFC7B
			// (set) Token: 0x060054EB RID: 21739 RVA: 0x001F1A83 File Offset: 0x001EFC83
			[Serialize(0f, IsPropertySaveable.No, "How much the affliction's strength changes every second while this effect is active.", "", false)]
			public float StrengthChange { get; private set; }

			// Token: 0x17001446 RID: 5190
			// (get) Token: 0x060054EC RID: 21740 RVA: 0x001F1A8C File Offset: 0x001EFC8C
			// (set) Token: 0x060054ED RID: 21741 RVA: 0x001F1A94 File Offset: 0x001EFC94
			[Serialize(false, IsPropertySaveable.No, "If set to true, MinVitalityDecrease and MaxVitalityDecrease represent a fraction of the affected character's maximum vitality, with 1 meaning 100%, instead of the same amount for all species.", "", false)]
			public bool MultiplyByMaxVitality { get; private set; }

			// Token: 0x17001447 RID: 5191
			// (get) Token: 0x060054EE RID: 21742 RVA: 0x001F1A9D File Offset: 0x001EFC9D
			// (set) Token: 0x060054EF RID: 21743 RVA: 0x001F1AA5 File Offset: 0x001EFCA5
			[Serialize(0f, IsPropertySaveable.No, "Blur effect strength at this effect's lowest strength.", "", false)]
			public float MinScreenBlur { get; private set; }

			// Token: 0x17001448 RID: 5192
			// (get) Token: 0x060054F0 RID: 21744 RVA: 0x001F1AAE File Offset: 0x001EFCAE
			// (set) Token: 0x060054F1 RID: 21745 RVA: 0x001F1AB6 File Offset: 0x001EFCB6
			[Serialize(0f, IsPropertySaveable.No, "Blur effect strength at this effect's highest strength.", "", false)]
			public float MaxScreenBlur { get; private set; }

			// Token: 0x17001449 RID: 5193
			// (get) Token: 0x060054F2 RID: 21746 RVA: 0x001F1ABF File Offset: 0x001EFCBF
			// (set) Token: 0x060054F3 RID: 21747 RVA: 0x001F1AC7 File Offset: 0x001EFCC7
			[Serialize(0f, IsPropertySaveable.No, "Generic distortion effect strength at this effect's lowest strength.", "", false)]
			public float MinScreenDistort { get; private set; }

			// Token: 0x1700144A RID: 5194
			// (get) Token: 0x060054F4 RID: 21748 RVA: 0x001F1AD0 File Offset: 0x001EFCD0
			// (set) Token: 0x060054F5 RID: 21749 RVA: 0x001F1AD8 File Offset: 0x001EFCD8
			[Serialize(0f, IsPropertySaveable.No, "Generic distortion effect strength at this effect's highest strength.", "", false)]
			public float MaxScreenDistort { get; private set; }

			// Token: 0x1700144B RID: 5195
			// (get) Token: 0x060054F6 RID: 21750 RVA: 0x001F1AE1 File Offset: 0x001EFCE1
			// (set) Token: 0x060054F7 RID: 21751 RVA: 0x001F1AE9 File Offset: 0x001EFCE9
			[Serialize(0f, IsPropertySaveable.No, "Radial distortion effect strength at this effect's lowest strength.", "", false)]
			public float MinRadialDistort { get; private set; }

			// Token: 0x1700144C RID: 5196
			// (get) Token: 0x060054F8 RID: 21752 RVA: 0x001F1AF2 File Offset: 0x001EFCF2
			// (set) Token: 0x060054F9 RID: 21753 RVA: 0x001F1AFA File Offset: 0x001EFCFA
			[Serialize(0f, IsPropertySaveable.No, "Radial distortion effect strength at this effect's highest strength.", "", false)]
			public float MaxRadialDistort { get; private set; }

			// Token: 0x1700144D RID: 5197
			// (get) Token: 0x060054FA RID: 21754 RVA: 0x001F1B03 File Offset: 0x001EFD03
			// (set) Token: 0x060054FB RID: 21755 RVA: 0x001F1B0B File Offset: 0x001EFD0B
			[Serialize(0f, IsPropertySaveable.No, "Chromatic aberration effect strength at this effect's lowest strength.", "", false)]
			public float MinChromaticAberration { get; private set; }

			// Token: 0x1700144E RID: 5198
			// (get) Token: 0x060054FC RID: 21756 RVA: 0x001F1B14 File Offset: 0x001EFD14
			// (set) Token: 0x060054FD RID: 21757 RVA: 0x001F1B1C File Offset: 0x001EFD1C
			[Serialize(0f, IsPropertySaveable.No, "Chromatic aberration effect strength at this effect's highest strength.", "", false)]
			public float MaxChromaticAberration { get; private set; }

			// Token: 0x1700144F RID: 5199
			// (get) Token: 0x060054FE RID: 21758 RVA: 0x001F1B25 File Offset: 0x001EFD25
			// (set) Token: 0x060054FF RID: 21759 RVA: 0x001F1B2D File Offset: 0x001EFD2D
			[Serialize("255,255,255,255", IsPropertySaveable.No, "Radiation grain effect color.", "", false)]
			public Color GrainColor { get; private set; }

			// Token: 0x17001450 RID: 5200
			// (get) Token: 0x06005500 RID: 21760 RVA: 0x001F1B36 File Offset: 0x001EFD36
			// (set) Token: 0x06005501 RID: 21761 RVA: 0x001F1B3E File Offset: 0x001EFD3E
			[Serialize(0f, IsPropertySaveable.No, "Radiation grain effect strength at this effect's lowest strength.", "", false)]
			public float MinGrainStrength { get; private set; }

			// Token: 0x17001451 RID: 5201
			// (get) Token: 0x06005502 RID: 21762 RVA: 0x001F1B47 File Offset: 0x001EFD47
			// (set) Token: 0x06005503 RID: 21763 RVA: 0x001F1B4F File Offset: 0x001EFD4F
			[Serialize(0f, IsPropertySaveable.No, "Radiation grain effect strength at this effect's highest strength.", "", false)]
			public float MaxGrainStrength { get; private set; }

			// Token: 0x17001452 RID: 5202
			// (get) Token: 0x06005504 RID: 21764 RVA: 0x001F1B58 File Offset: 0x001EFD58
			// (set) Token: 0x06005505 RID: 21765 RVA: 0x001F1B60 File Offset: 0x001EFD60
			[Serialize(0f, IsPropertySaveable.No, "The maximum rate of fluctuation to apply to visual effects caused by this affliction effect. Effective fluctuation is proportional to the affliction's current strength.", "", false)]
			public float ScreenEffectFluctuationFrequency { get; private set; }

			// Token: 0x17001453 RID: 5203
			// (get) Token: 0x06005506 RID: 21766 RVA: 0x001F1B69 File Offset: 0x001EFD69
			// (set) Token: 0x06005507 RID: 21767 RVA: 0x001F1B71 File Offset: 0x001EFD71
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for the affliction overlay's opacity at this effect's lowest strength. See the list of elements for more details.", "", false)]
			public float MinAfflictionOverlayAlphaMultiplier { get; private set; }

			// Token: 0x17001454 RID: 5204
			// (get) Token: 0x06005508 RID: 21768 RVA: 0x001F1B7A File Offset: 0x001EFD7A
			// (set) Token: 0x06005509 RID: 21769 RVA: 0x001F1B82 File Offset: 0x001EFD82
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for the affliction overlay's opacity at this effect's highest strength. See the list of elements for more details.", "", false)]
			public float MaxAfflictionOverlayAlphaMultiplier { get; private set; }

			// Token: 0x17001455 RID: 5205
			// (get) Token: 0x0600550A RID: 21770 RVA: 0x001F1B8B File Offset: 0x001EFD8B
			// (set) Token: 0x0600550B RID: 21771 RVA: 0x001F1B93 File Offset: 0x001EFD93
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for every buff's decay rate at this effect's lowest strength. Only applies to afflictions of class BuffDurationIncrease.", "", false)]
			public float MinBuffMultiplier { get; private set; }

			// Token: 0x17001456 RID: 5206
			// (get) Token: 0x0600550C RID: 21772 RVA: 0x001F1B9C File Offset: 0x001EFD9C
			// (set) Token: 0x0600550D RID: 21773 RVA: 0x001F1BA4 File Offset: 0x001EFDA4
			[Serialize(1f, IsPropertySaveable.No, "Multiplier for every buff's decay rate at this effect's highest strength. Only applies to afflictions of class BuffDurationIncrease.", "", false)]
			public float MaxBuffMultiplier { get; private set; }

			// Token: 0x17001457 RID: 5207
			// (get) Token: 0x0600550E RID: 21774 RVA: 0x001F1BAD File Offset: 0x001EFDAD
			// (set) Token: 0x0600550F RID: 21775 RVA: 0x001F1BB5 File Offset: 0x001EFDB5
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to the affected character's speed at this effect's lowest strength.", "", false)]
			public float MinSpeedMultiplier { get; private set; }

			// Token: 0x17001458 RID: 5208
			// (get) Token: 0x06005510 RID: 21776 RVA: 0x001F1BBE File Offset: 0x001EFDBE
			// (set) Token: 0x06005511 RID: 21777 RVA: 0x001F1BC6 File Offset: 0x001EFDC6
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to the affected character's speed at this effect's highest strength.", "", false)]
			public float MaxSpeedMultiplier { get; private set; }

			// Token: 0x17001459 RID: 5209
			// (get) Token: 0x06005512 RID: 21778 RVA: 0x001F1BCF File Offset: 0x001EFDCF
			// (set) Token: 0x06005513 RID: 21779 RVA: 0x001F1BD7 File Offset: 0x001EFDD7
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to all of the affected character's skill levels at this effect's lowest strength.", "", false)]
			public float MinSkillMultiplier { get; private set; }

			// Token: 0x1700145A RID: 5210
			// (get) Token: 0x06005514 RID: 21780 RVA: 0x001F1BE0 File Offset: 0x001EFDE0
			// (set) Token: 0x06005515 RID: 21781 RVA: 0x001F1BE8 File Offset: 0x001EFDE8
			[Serialize(1f, IsPropertySaveable.No, "Multiplier to apply to all of the affected character's skill levels at this effect's highest strength.", "", false)]
			public float MaxSkillMultiplier { get; private set; }

			// Token: 0x1700145B RID: 5211
			// (get) Token: 0x06005516 RID: 21782 RVA: 0x001F1BF1 File Offset: 0x001EFDF1
			// (set) Token: 0x06005517 RID: 21783 RVA: 0x001F1BF9 File Offset: 0x001EFDF9
			[Serialize(0f, IsPropertySaveable.No, "The amount of resistance to the afflictions specified by ResistanceFor to apply at this effect's lowest strength.", "", false)]
			public float MinResistance { get; private set; }

			// Token: 0x1700145C RID: 5212
			// (get) Token: 0x06005518 RID: 21784 RVA: 0x001F1C02 File Offset: 0x001EFE02
			// (set) Token: 0x06005519 RID: 21785 RVA: 0x001F1C0A File Offset: 0x001EFE0A
			[Serialize(0f, IsPropertySaveable.No, "The amount of resistance to the afflictions specified by ResistanceFor to apply at this effect's highest strength.", "", false)]
			public float MaxResistance { get; private set; }

			// Token: 0x1700145D RID: 5213
			// (get) Token: 0x0600551A RID: 21786 RVA: 0x001F1C13 File Offset: 0x001EFE13
			// (set) Token: 0x0600551B RID: 21787 RVA: 0x001F1C1B File Offset: 0x001EFE1B
			[Serialize("", IsPropertySaveable.No, "Identifier used by AI to determine conversation lines to say when this effect is active.", "", false)]
			public Identifier DialogFlag { get; private set; }

			// Token: 0x1700145E RID: 5214
			// (get) Token: 0x0600551C RID: 21788 RVA: 0x001F1C24 File Offset: 0x001EFE24
			// (set) Token: 0x0600551D RID: 21789 RVA: 0x001F1C2C File Offset: 0x001EFE2C
			[Serialize("", IsPropertySaveable.No, "Tag that enemy AI may use to target the affected character when this effect is active.", "", false)]
			public Identifier Tag { get; private set; }

			// Token: 0x1700145F RID: 5215
			// (get) Token: 0x0600551E RID: 21790 RVA: 0x001F1C35 File Offset: 0x001EFE35
			// (set) Token: 0x0600551F RID: 21791 RVA: 0x001F1C3D File Offset: 0x001EFE3D
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's face with at this effect's lowest strength. The alpha channel is used to determine how much to tint the character's face.", "", false)]
			public Color MinFaceTint { get; private set; }

			// Token: 0x17001460 RID: 5216
			// (get) Token: 0x06005520 RID: 21792 RVA: 0x001F1C46 File Offset: 0x001EFE46
			// (set) Token: 0x06005521 RID: 21793 RVA: 0x001F1C4E File Offset: 0x001EFE4E
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's face with at this effect's highest strength. The alpha channel is used to determine how much to tint the character's face.", "", false)]
			public Color MaxFaceTint { get; private set; }

			// Token: 0x17001461 RID: 5217
			// (get) Token: 0x06005522 RID: 21794 RVA: 0x001F1C57 File Offset: 0x001EFE57
			// (set) Token: 0x06005523 RID: 21795 RVA: 0x001F1C5F File Offset: 0x001EFE5F
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's entire body with at this effect's lowest strength. The alpha channel is used to determine how much to tint the character.", "", false)]
			public Color MinBodyTint { get; private set; }

			// Token: 0x17001462 RID: 5218
			// (get) Token: 0x06005524 RID: 21796 RVA: 0x001F1C68 File Offset: 0x001EFE68
			// (set) Token: 0x06005525 RID: 21797 RVA: 0x001F1C70 File Offset: 0x001EFE70
			[Serialize("0,0,0,0", IsPropertySaveable.No, "Color to tint the affected character's entire body with at this effect's highest strength. The alpha channel is used to determine how much to tint the character.", "", false)]
			public Color MaxBodyTint { get; private set; }

			// Token: 0x17001463 RID: 5219
			// (get) Token: 0x06005526 RID: 21798 RVA: 0x001F1C79 File Offset: 0x001EFE79
			// (set) Token: 0x06005527 RID: 21799 RVA: 0x001F1C81 File Offset: 0x001EFE81
			[Serialize(0f, IsPropertySaveable.No, "Range of the \"thermal goggles overlay\" enabled by the affliction.", "", false)]
			public float ThermalOverlayRange { get; private set; }

			// Token: 0x17001464 RID: 5220
			// (get) Token: 0x06005528 RID: 21800 RVA: 0x001F1C8A File Offset: 0x001EFE8A
			// (set) Token: 0x06005529 RID: 21801 RVA: 0x001F1C92 File Offset: 0x001EFE92
			[Serialize("255,0,0,255", IsPropertySaveable.No, "Color of the \"thermal goggles overlay\" enabled by the affliction. Only has an effect if ThermalOverlayRange is larger than 0.", "", false)]
			public Color ThermalOverlayColor { get; private set; }

			// Token: 0x17001465 RID: 5221
			// (get) Token: 0x0600552A RID: 21802 RVA: 0x001F1C9B File Offset: 0x001EFE9B
			// (set) Token: 0x0600552B RID: 21803 RVA: 0x001F1CA3 File Offset: 0x001EFEA3
			[Serialize(0f, IsPropertySaveable.No, "Multiplier for the convulsion/seizure effect on the character's ragdoll when this effect is active.", "", false)]
			public float ConvulseAmount { get; private set; }

			// Token: 0x0600552C RID: 21804 RVA: 0x001F1CAC File Offset: 0x001EFEAC
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

			// Token: 0x0600552D RID: 21805 RVA: 0x001F1EF0 File Offset: 0x001F00F0
			public float GetStrengthFactor(Affliction affliction)
			{
				return this.GetStrengthFactor(affliction.Strength);
			}

			// Token: 0x0600552E RID: 21806 RVA: 0x001F1EFE File Offset: 0x001F00FE
			public float GetStrengthFactor(float strength)
			{
				return MathUtils.InverseLerp(this.MinStrength, this.MaxStrength, strength);
			}

			// Token: 0x04002FE4 RID: 12260
			public readonly ImmutableArray<Identifier> ResistanceFor;

			// Token: 0x04002FE5 RID: 12261
			public readonly ImmutableArray<LimbType> ResistanceLimbs;

			// Token: 0x04002FF1 RID: 12273
			public readonly ImmutableArray<Identifier> BlockTransformation;

			// Token: 0x04002FF2 RID: 12274
			public readonly ImmutableDictionary<StatTypes, AfflictionPrefab.Effect.AppliedStatValue> AfflictionStatValues;

			// Token: 0x04002FF3 RID: 12275
			public readonly AbilityFlags AfflictionAbilityFlags;

			// Token: 0x04002FF4 RID: 12276
			public readonly ImmutableArray<StatusEffect> StatusEffects;

			// Token: 0x02000E7E RID: 3710
			public readonly struct AppliedStatValue
			{
				// Token: 0x06006A12 RID: 27154 RVA: 0x00225A08 File Offset: 0x00223C08
				public AppliedStatValue(ContentXElement element)
				{
					this.Value = element.GetAttributeFloat("value", 0f);
					string key = "stattype";
					StatTypes statTypes = StatTypes.None;
					this.StatType = element.GetAttributeEnum<StatTypes>(key, statTypes);
					this.MinValue = element.GetAttributeFloat("minvalue", this.Value);
					this.MaxValue = element.GetAttributeFloat("maxvalue", this.Value);
				}

				// Token: 0x0400427D RID: 17021
				public readonly StatTypes StatType;

				// Token: 0x0400427E RID: 17022
				public readonly float MinValue;

				// Token: 0x0400427F RID: 17023
				public readonly float MaxValue;

				// Token: 0x04004280 RID: 17024
				private readonly float Value;
			}
		}

		// Token: 0x0200087B RID: 2171
		public sealed class Description
		{
			// Token: 0x0600552F RID: 21807 RVA: 0x001F1F14 File Offset: 0x001F0114
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

			// Token: 0x04002FF5 RID: 12277
			public readonly LocalizedString Text;

			// Token: 0x04002FF6 RID: 12278
			public readonly Identifier TextTag;

			// Token: 0x04002FF7 RID: 12279
			public readonly float MinStrength;

			// Token: 0x04002FF8 RID: 12280
			public readonly float MaxStrength;

			// Token: 0x04002FF9 RID: 12281
			public readonly AfflictionPrefab.Description.TargetType Target;

			// Token: 0x02000E7F RID: 3711
			public enum TargetType
			{
				// Token: 0x04004282 RID: 17026
				Any,
				// Token: 0x04004283 RID: 17027
				Self,
				// Token: 0x04004284 RID: 17028
				OtherCharacter
			}
		}

		// Token: 0x0200087C RID: 2172
		public sealed class PeriodicEffect
		{
			// Token: 0x06005530 RID: 21808 RVA: 0x001F2084 File Offset: 0x001F0284
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

			// Token: 0x04002FFA RID: 12282
			public readonly List<StatusEffect> StatusEffects = new List<StatusEffect>();

			// Token: 0x04002FFB RID: 12283
			public readonly float MinInterval;

			// Token: 0x04002FFC RID: 12284
			public readonly float MaxInterval;

			// Token: 0x04002FFD RID: 12285
			public readonly float MinStrength;

			// Token: 0x04002FFE RID: 12286
			public readonly float MaxStrength;
		}
	}
}
