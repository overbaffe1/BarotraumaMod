using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CC RID: 204
	internal class CharacterHealth
	{
		// Token: 0x17000685 RID: 1669
		// (get) Token: 0x0600165E RID: 5726 RVA: 0x000BD4AC File Offset: 0x000BB6AC
		// (set) Token: 0x0600165F RID: 5727 RVA: 0x000BD4C3 File Offset: 0x000BB6C3
		protected float UnmodifiedMaxVitality
		{
			get
			{
				return this.Character.Params.Health.Vitality;
			}
			set
			{
				this.Character.Params.Health.Vitality = value;
			}
		}

		// Token: 0x17000686 RID: 1670
		// (get) Token: 0x06001660 RID: 5728 RVA: 0x000BD4DB File Offset: 0x000BB6DB
		// (set) Token: 0x06001661 RID: 5729 RVA: 0x000BD509 File Offset: 0x000BB709
		public bool DoesBleed
		{
			get
			{
				return this.Character.Params.Health.DoesBleed && !this.Character.Params.IsMachine;
			}
			private set
			{
				this.Character.Params.Health.DoesBleed = value;
			}
		}

		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x06001662 RID: 5730 RVA: 0x000BD521 File Offset: 0x000BB721
		// (set) Token: 0x06001663 RID: 5731 RVA: 0x000BD538 File Offset: 0x000BB738
		public bool UseHealthWindow
		{
			get
			{
				return this.Character.Params.Health.UseHealthWindow;
			}
			set
			{
				this.Character.Params.Health.UseHealthWindow = value;
			}
		}

		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x06001664 RID: 5732 RVA: 0x000BD550 File Offset: 0x000BB750
		// (set) Token: 0x06001665 RID: 5733 RVA: 0x000BD567 File Offset: 0x000BB767
		public float CrushDepth
		{
			get
			{
				return this.Character.Params.Health.CrushDepth;
			}
			private set
			{
				this.Character.Params.Health.CrushDepth = value;
			}
		}

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x06001666 RID: 5734 RVA: 0x000BD57F File Offset: 0x000BB77F
		public Affliction BloodlossAffliction
		{
			get
			{
				return this.bloodlossAffliction;
			}
		}

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x06001667 RID: 5735 RVA: 0x000BD587 File Offset: 0x000BB787
		public bool IsUnconscious
		{
			get
			{
				return this.Character.IsDead || (this.Vitality <= 0f && !this.Character.HasAbilityFlag(AbilityFlags.AlwaysStayConscious));
			}
		}

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x06001668 RID: 5736 RVA: 0x000BD5BA File Offset: 0x000BB7BA
		// (set) Token: 0x06001669 RID: 5737 RVA: 0x000BD5C2 File Offset: 0x000BB7C2
		public float PressureKillDelay { get; private set; } = 5f;

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x0600166A RID: 5738 RVA: 0x000BD5CB File Offset: 0x000BB7CB
		public float Vitality
		{
			get
			{
				if (this.Character.IsDead)
				{
					return this.minVitality;
				}
				return this.vitality;
			}
		}

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x0600166B RID: 5739 RVA: 0x000BD5E7 File Offset: 0x000BB7E7
		public float VitalityDisregardingDeath
		{
			get
			{
				return this.vitality;
			}
		}

		// Token: 0x1700068E RID: 1678
		// (get) Token: 0x0600166C RID: 5740 RVA: 0x000BD5EF File Offset: 0x000BB7EF
		public float HealthPercentage
		{
			get
			{
				return MathUtils.Percentage(this.Vitality, this.MaxVitality);
			}
		}

		// Token: 0x1700068F RID: 1679
		// (get) Token: 0x0600166D RID: 5741 RVA: 0x000BD604 File Offset: 0x000BB804
		public float MaxVitality
		{
			get
			{
				float max = this.UnmodifiedMaxVitality;
				Character character = this.Character;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					if (info == null)
					{
						flag = (null != null);
					}
					else
					{
						Job job = info.Job;
						flag = (((job != null) ? job.Prefab : null) != null);
					}
				}
				if (flag)
				{
					max += this.Character.Info.Job.Prefab.VitalityModifier;
				}
				max *= this.Character.HumanPrefabHealthMultiplier;
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					max *= (this.Character.IsOnPlayerTeam ? campaign.Settings.CrewVitalityMultiplier : campaign.Settings.NonCrewVitalityMultiplier);
				}
				max *= 1f + this.Character.GetStatValue(StatTypes.MaximumHealthMultiplier, true);
				return max * this.Character.HealthMultiplier;
			}
		}

		// Token: 0x17000690 RID: 1680
		// (get) Token: 0x0600166E RID: 5742 RVA: 0x000BD6D0 File Offset: 0x000BB8D0
		public float MinVitality
		{
			get
			{
				Character character = this.Character;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					if (info == null)
					{
						flag = (null != null);
					}
					else
					{
						Job job = info.Job;
						flag = (((job != null) ? job.Prefab : null) != null);
					}
				}
				if (flag)
				{
					return -this.MaxVitality;
				}
				return this.minVitality;
			}
		}

		// Token: 0x17000691 RID: 1681
		// (get) Token: 0x0600166F RID: 5743 RVA: 0x000BD70C File Offset: 0x000BB90C
		// (set) Token: 0x06001670 RID: 5744 RVA: 0x000BD714 File Offset: 0x000BB914
		public Color FaceTint { get; private set; }

		// Token: 0x17000692 RID: 1682
		// (get) Token: 0x06001671 RID: 5745 RVA: 0x000BD71D File Offset: 0x000BB91D
		// (set) Token: 0x06001672 RID: 5746 RVA: 0x000BD725 File Offset: 0x000BB925
		public Color BodyTint { get; private set; }

		// Token: 0x17000693 RID: 1683
		// (get) Token: 0x06001673 RID: 5747 RVA: 0x000BD72E File Offset: 0x000BB92E
		// (set) Token: 0x06001674 RID: 5748 RVA: 0x000BD76C File Offset: 0x000BB96C
		public float OxygenAmount
		{
			get
			{
				if (!this.Character.NeedsOxygen || this.Unkillable || this.Character.GodMode)
				{
					return 100f;
				}
				return -this.oxygenLowAffliction.Strength + 100f;
			}
			set
			{
				if (!this.Character.NeedsOxygen || this.Unkillable || this.Character.GodMode)
				{
					return;
				}
				this.oxygenLowAffliction.Strength = MathHelper.Clamp(-value + 100f, 0f, 200f);
			}
		}

		// Token: 0x17000694 RID: 1684
		// (get) Token: 0x06001675 RID: 5749 RVA: 0x000BD7BE File Offset: 0x000BB9BE
		// (set) Token: 0x06001676 RID: 5750 RVA: 0x000BD7CB File Offset: 0x000BB9CB
		public float BloodlossAmount
		{
			get
			{
				return this.bloodlossAffliction.Strength;
			}
			set
			{
				this.bloodlossAffliction.Strength = MathHelper.Clamp(value, 0f, this.bloodlossAffliction.Prefab.MaxStrength);
			}
		}

		// Token: 0x17000695 RID: 1685
		// (get) Token: 0x06001677 RID: 5751 RVA: 0x000BD7F3 File Offset: 0x000BB9F3
		// (set) Token: 0x06001678 RID: 5752 RVA: 0x000BD800 File Offset: 0x000BBA00
		public float Stun
		{
			get
			{
				return this.stunAffliction.Strength;
			}
			set
			{
				if (this.Character.GodMode)
				{
					return;
				}
				this.stunAffliction.Strength = MathHelper.Clamp(value, 0f, this.stunAffliction.Prefab.MaxStrength);
			}
		}

		// Token: 0x17000696 RID: 1686
		// (get) Token: 0x06001679 RID: 5753 RVA: 0x000BD836 File Offset: 0x000BBA36
		// (set) Token: 0x0600167A RID: 5754 RVA: 0x000BD83E File Offset: 0x000BBA3E
		public bool IsParalyzed { get; private set; }

		// Token: 0x17000697 RID: 1687
		// (get) Token: 0x0600167B RID: 5755 RVA: 0x000BD847 File Offset: 0x000BBA47
		// (set) Token: 0x0600167C RID: 5756 RVA: 0x000BD84F File Offset: 0x000BBA4F
		public float StunTimer { get; private set; }

		// Token: 0x17000698 RID: 1688
		// (get) Token: 0x0600167D RID: 5757 RVA: 0x000BD858 File Offset: 0x000BBA58
		// (set) Token: 0x0600167E RID: 5758 RVA: 0x000BD860 File Offset: 0x000BBA60
		public bool WasInFullHealth { get; private set; }

		// Token: 0x17000699 RID: 1689
		// (get) Token: 0x0600167F RID: 5759 RVA: 0x000BD869 File Offset: 0x000BBA69
		public Affliction PressureAffliction
		{
			get
			{
				return this.pressureAffliction;
			}
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x000BD874 File Offset: 0x000BBA74
		public CharacterHealth(Character character)
		{
			this.Character = character;
			this.vitality = 100f;
			this.DoesBleed = true;
			this.UseHealthWindow = false;
			this.InitIrremovableAfflictions();
			this.limbHealths.Add(new CharacterHealth.LimbHealth());
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x000BD934 File Offset: 0x000BBB34
		public CharacterHealth(ContentXElement element, Character character, ContentXElement limbHealthElement = null)
		{
			this.Character = character;
			this.InitIrremovableAfflictions();
			this.vitality = this.UnmodifiedMaxVitality;
			this.minVitality = element.GetAttributeFloat("MinVitality", character.IsHuman ? -100f : 0f);
			this.limbHealths.Clear();
			if (limbHealthElement == null)
			{
				limbHealthElement = element;
			}
			foreach (ContentXElement subElement in limbHealthElement.Elements())
			{
				if (subElement.Name.ToString().Equals("limb", StringComparison.OrdinalIgnoreCase))
				{
					this.limbHealths.Add(new CharacterHealth.LimbHealth(subElement, this));
				}
			}
			if (this.limbHealths.Count == 0)
			{
				this.limbHealths.Add(new CharacterHealth.LimbHealth());
			}
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x000BDA8C File Offset: 0x000BBC8C
		public void CheckForErrors()
		{
			int i;
			int j;
			for (i = 0; i < this.limbHealths.Count; i = j + 1)
			{
				if (this.Character.AnimController.Limbs.None((Limb l) => l.HealthIndex == i))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(114, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Potential error in character \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Character.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\": none of the limbs have been set to use the LimbHealth #");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(", and it will do nothing. ");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "Did you forget to set the HealthIndex values of the limbs?", this.Character.ContentPackage);
				}
				j = i;
			}
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x000BDB68 File Offset: 0x000BBD68
		private void InitIrremovableAfflictions()
		{
			this.irremovableAfflictions.Add(this.bloodlossAffliction = new Affliction(AfflictionPrefab.Bloodloss, 0f));
			this.irremovableAfflictions.Add(this.stunAffliction = new Affliction(AfflictionPrefab.Stun, 0f));
			this.irremovableAfflictions.Add(this.pressureAffliction = new Affliction(AfflictionPrefab.Pressure, 0f));
			this.irremovableAfflictions.Add(this.oxygenLowAffliction = new Affliction(AfflictionPrefab.OxygenLow, 0f));
			foreach (Affliction affliction in this.irremovableAfflictions)
			{
				this.afflictions.Add(affliction, null);
			}
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x000BDC54 File Offset: 0x000BBE54
		public IReadOnlyCollection<Affliction> GetAllAfflictions()
		{
			return this.afflictions.Keys;
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x000BDC61 File Offset: 0x000BBE61
		public IEnumerable<Affliction> GetAllAfflictions(Func<Affliction, bool> limbHealthFilter)
		{
			return this.afflictions.Keys.Where(limbHealthFilter);
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x000BDC74 File Offset: 0x000BBE74
		private float GetTotalDamage(CharacterHealth.LimbHealth limbHealth)
		{
			float totalDamage = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (kvp.Value == limbHealth)
				{
					Affliction affliction = kvp.Key;
					totalDamage += affliction.GetVitalityDecrease(this);
				}
			}
			return totalDamage;
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x000BDCE4 File Offset: 0x000BBEE4
		private CharacterHealth.LimbHealth GetMatchingLimbHealth(Limb limb)
		{
			if (limb != null)
			{
				return this.limbHealths[limb.HealthIndex];
			}
			return null;
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x000BDCFC File Offset: 0x000BBEFC
		private CharacterHealth.LimbHealth GetMatchingLimbHealth(Affliction affliction)
		{
			return this.GetMatchingLimbHealth(this.Character.AnimController.GetLimb(affliction.Prefab.IndicatorLimb, false, false, false));
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x000BDD22 File Offset: 0x000BBF22
		public Affliction GetAffliction(string identifier, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction(identifier.ToIdentifier(), allowLimbAfflictions);
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x000BDD34 File Offset: 0x000BBF34
		public Affliction GetAffliction(Identifier identifier, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction((Affliction a) => a.Prefab.Identifier == identifier, allowLimbAfflictions);
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x000BDD64 File Offset: 0x000BBF64
		public Affliction GetAfflictionOfType(Identifier afflictionType, bool allowLimbAfflictions = true)
		{
			return this.GetAffliction((Affliction a) => a.Prefab.AfflictionType == afflictionType, allowLimbAfflictions);
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x000BDD94 File Offset: 0x000BBF94
		private Affliction GetAffliction(Func<Affliction, bool> predicate, bool allowLimbAfflictions = true)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if ((allowLimbAfflictions || kvp.Value == null) && predicate(kvp.Key))
				{
					return kvp.Key;
				}
			}
			return null;
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x000BDE08 File Offset: 0x000BC008
		public T GetAffliction<T>(Identifier identifier, bool allowLimbAfflictions = true) where T : Affliction
		{
			return this.GetAffliction(identifier, allowLimbAfflictions) as T;
		}

		// Token: 0x0600168E RID: 5774 RVA: 0x000BDE1C File Offset: 0x000BC01C
		public Affliction GetAffliction(Identifier identifier, Limb limb)
		{
			if (limb.HealthIndex < 0 || limb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					limb.type.ToString(),
					" is targeting index ",
					limb.HealthIndex.ToString()
				}), null, null, false, false);
				return null;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (this.limbHealths[limb.HealthIndex] == kvp.Value && kvp.Key.Prefab.Identifier == identifier)
				{
					return kvp.Key;
				}
			}
			return null;
		}

		// Token: 0x0600168F RID: 5775 RVA: 0x000BDF4C File Offset: 0x000BC14C
		public Limb GetAfflictionLimb(Affliction affliction)
		{
			CharacterHealth.LimbHealth limbHealth;
			if (affliction != null && this.afflictions.TryGetValue(affliction, out limbHealth))
			{
				if (limbHealth == null)
				{
					return null;
				}
				int limbHealthIndex = this.limbHealths.IndexOf(limbHealth);
				foreach (Limb limb in this.Character.AnimController.Limbs)
				{
					if (limb.HealthIndex == limbHealthIndex)
					{
						return limb;
					}
				}
			}
			return null;
		}

		// Token: 0x06001690 RID: 5776 RVA: 0x000BDFB0 File Offset: 0x000BC1B0
		public float GetAfflictionStrength(Identifier afflictionType, Limb limb, bool requireLimbSpecific)
		{
			if (requireLimbSpecific && this.limbHealths.Count == 1)
			{
				return 0f;
			}
			float strength = 0f;
			CharacterHealth.LimbHealth limbHealth = this.limbHealths[limb.HealthIndex];
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (kvp.Value == limbHealth)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength >= affliction.Prefab.ActivationThreshold && affliction.Prefab.AfflictionType == afflictionType)
					{
						strength += affliction.Strength;
					}
				}
			}
			return strength;
		}

		// Token: 0x06001691 RID: 5777 RVA: 0x000BE074 File Offset: 0x000BC274
		public float GetAfflictionStrengthByType(Identifier afflictionType, bool allowLimbAfflictions = true)
		{
			return this.GetAfflictionStrength(afflictionType, Identifier.Empty, allowLimbAfflictions);
		}

		// Token: 0x06001692 RID: 5778 RVA: 0x000BE083 File Offset: 0x000BC283
		public float GetAfflictionStrengthByIdentifier(Identifier afflictionIdentifier, bool allowLimbAfflictions = true)
		{
			return this.GetAfflictionStrength(Identifier.Empty, afflictionIdentifier, allowLimbAfflictions);
		}

		// Token: 0x06001693 RID: 5779 RVA: 0x000BE094 File Offset: 0x000BC294
		public float GetAfflictionStrength(Identifier afflictionType, Identifier afflictionidentifier, bool allowLimbAfflictions = true)
		{
			float strength = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				if (allowLimbAfflictions || kvp.Value == null)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength >= affliction.Prefab.ActivationThreshold && (affliction.Prefab.AfflictionType == afflictionType || afflictionType.IsEmpty) && (affliction.Prefab.Identifier == afflictionidentifier || afflictionidentifier.IsEmpty))
					{
						strength += affliction.Strength;
					}
				}
			}
			return strength;
		}

		// Token: 0x06001694 RID: 5780 RVA: 0x000BE154 File Offset: 0x000BC354
		public void ApplyAffliction(Limb targetLimb, Affliction affliction, bool allowStacking = true, bool ignoreUnkillability = false, bool recalculateVitality = true)
		{
			if (this.Character.GodMode)
			{
				return;
			}
			if (!ignoreUnkillability && !affliction.Prefab.IsBuff && this.Unkillable)
			{
				return;
			}
			if (affliction.Prefab.LimbSpecific)
			{
				if (targetLimb == null)
				{
					using (List<CharacterHealth.LimbHealth>.Enumerator enumerator = this.limbHealths.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							CharacterHealth.LimbHealth limbHealth = enumerator.Current;
							this.AddLimbAffliction(limbHealth, null, affliction, allowStacking, recalculateVitality);
						}
						return;
					}
				}
				this.AddLimbAffliction(targetLimb, affliction, allowStacking, recalculateVitality);
				return;
			}
			this.AddAffliction(affliction, allowStacking);
		}

		// Token: 0x06001695 RID: 5781 RVA: 0x000BE1F8 File Offset: 0x000BC3F8
		public float GetResistance(AfflictionPrefab afflictionPrefab, LimbType limbType)
		{
			float resistance = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				resistance += affliction.GetResistance(afflictionPrefab.Identifier, limbType);
			}
			float abilityResistanceMultiplier = this.Character.GetAbilityResistance(afflictionPrefab);
			return 1f - (1f - resistance) * abilityResistanceMultiplier;
		}

		// Token: 0x06001696 RID: 5782 RVA: 0x000BE280 File Offset: 0x000BC480
		public float GetStatValue(StatTypes statType)
		{
			float value = 0f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				value += affliction.GetStatValue(statType);
			}
			return value;
		}

		// Token: 0x06001697 RID: 5783 RVA: 0x000BE2E8 File Offset: 0x000BC4E8
		public bool HasFlag(AbilityFlags flagType)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				if (affliction.HasFlag(flagType))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001698 RID: 5784 RVA: 0x000BE34C File Offset: 0x000BC54C
		public void ReduceAllAfflictionsOnAllLimbs(float amount, ActionType? treatmentAction = null)
		{
			this.matchingAfflictions.Clear();
			this.matchingAfflictions.AddRange(this.afflictions.Keys);
			this.ReduceMatchingAfflictions(amount, treatmentAction, null);
		}

		// Token: 0x06001699 RID: 5785 RVA: 0x000BE378 File Offset: 0x000BC578
		public void ReduceAfflictionOnAllLimbs(Identifier afflictionIdOrType, float amount, ActionType? treatmentAction = null, Character attacker = null)
		{
			if (afflictionIdOrType.IsEmpty)
			{
				throw new ArgumentException("afflictionIdOrType is empty");
			}
			this.matchingAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> affliction in this.afflictions)
			{
				if (affliction.Key.Prefab.Identifier == afflictionIdOrType || affliction.Key.Prefab.AfflictionType == afflictionIdOrType)
				{
					this.matchingAfflictions.Add(affliction.Key);
				}
			}
			this.ReduceMatchingAfflictions(amount, treatmentAction, attacker);
		}

		// Token: 0x0600169A RID: 5786 RVA: 0x000BE434 File Offset: 0x000BC634
		private IEnumerable<Affliction> GetAfflictionsForLimb(Limb targetLimb)
		{
			return from k in this.afflictions.Keys
			where this.afflictions[k] == this.limbHealths[targetLimb.HealthIndex]
			select k;
		}

		// Token: 0x0600169B RID: 5787 RVA: 0x000BE471 File Offset: 0x000BC671
		public void ReduceAllAfflictionsOnLimb(Limb targetLimb, float amount, ActionType? treatmentAction = null)
		{
			if (targetLimb == null)
			{
				throw new ArgumentNullException("targetLimb");
			}
			this.matchingAfflictions.Clear();
			this.matchingAfflictions.AddRange(this.GetAfflictionsForLimb(targetLimb));
			this.ReduceMatchingAfflictions(amount, treatmentAction, null);
		}

		// Token: 0x0600169C RID: 5788 RVA: 0x000BE4A8 File Offset: 0x000BC6A8
		public void ReduceAfflictionOnLimb(Limb targetLimb, Identifier afflictionIdOrType, float amount, ActionType? treatmentAction = null, Character attacker = null)
		{
			if (afflictionIdOrType.IsEmpty)
			{
				throw new ArgumentException("afflictionIdOrType is empty");
			}
			if (targetLimb == null)
			{
				throw new ArgumentNullException("targetLimb");
			}
			this.matchingAfflictions.Clear();
			CharacterHealth.LimbHealth targetLimbHealth = this.limbHealths[targetLimb.HealthIndex];
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> affliction in this.afflictions)
			{
				if ((affliction.Key.Prefab.Identifier == afflictionIdOrType || affliction.Key.Prefab.AfflictionType == afflictionIdOrType) && affliction.Value == targetLimbHealth)
				{
					this.matchingAfflictions.Add(affliction.Key);
				}
			}
			this.ReduceMatchingAfflictions(amount, treatmentAction, attacker);
		}

		// Token: 0x0600169D RID: 5789 RVA: 0x000BE590 File Offset: 0x000BC790
		private void ReduceMatchingAfflictions(float amount, ActionType? treatmentAction, Character attacker = null)
		{
			if (this.matchingAfflictions.Count == 0)
			{
				return;
			}
			float reduceAmount = amount / (float)this.matchingAfflictions.Count;
			if (reduceAmount > 0f)
			{
				AbilityReduceAffliction abilityReduceAffliction = new AbilityReduceAffliction(this.Character, reduceAmount);
				if (attacker != null)
				{
					attacker.CheckTalents(AbilityEffectType.OnReduceAffliction, abilityReduceAffliction);
				}
				reduceAmount = abilityReduceAffliction.Value;
			}
			for (int i = this.matchingAfflictions.Count - 1; i >= 0; i--)
			{
				Affliction matchingAffliction = this.matchingAfflictions[i];
				if (matchingAffliction.Strength < reduceAmount)
				{
					float surplus = reduceAmount - matchingAffliction.Strength;
					amount -= matchingAffliction.Strength;
					matchingAffliction.Strength = 0f;
					this.matchingAfflictions.RemoveAt(i);
					if (i == 0)
					{
						i = this.matchingAfflictions.Count;
					}
					if (i > 0)
					{
						reduceAmount += surplus / (float)i;
					}
					AchievementManager.OnAfflictionRemoved(matchingAffliction, this.Character);
				}
				else
				{
					matchingAffliction.Strength -= reduceAmount;
					amount -= reduceAmount;
					if (treatmentAction != null)
					{
						if (treatmentAction.Value == ActionType.OnUse || treatmentAction.Value == ActionType.OnSuccess)
						{
							matchingAffliction.AppliedAsSuccessfulTreatmentTime = Timing.TotalTime;
						}
						else if (treatmentAction.Value == ActionType.OnFailure)
						{
							matchingAffliction.AppliedAsFailedTreatmentTime = Timing.TotalTime;
						}
					}
				}
			}
			this.CalculateVitality();
		}

		// Token: 0x0600169E RID: 5790 RVA: 0x000BE6C4 File Offset: 0x000BC8C4
		public void ApplyDamage(Limb hitLimb, AttackResult attackResult, bool allowStacking = true, bool recalculateVitality = true)
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			if (hitLimb.HealthIndex < 0 || hitLimb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					hitLimb.type.ToString(),
					" is targeting index ",
					hitLimb.HealthIndex.ToString()
				}), null, null, false, false);
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventCharacterApplyDamage>(delegate(IEventCharacterApplyDamage x)
			{
				bool? flag = x.OnCharacterApplyDamage(this, attackResult, hitLimb, allowStacking);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			foreach (Affliction newAffliction in attackResult.Afflictions)
			{
				if (newAffliction.Prefab.LimbSpecific)
				{
					this.AddLimbAffliction(hitLimb, newAffliction, allowStacking, recalculateVitality);
				}
				else
				{
					this.AddAffliction(newAffliction, allowStacking);
				}
			}
		}

		// Token: 0x0600169F RID: 5791 RVA: 0x000BE87C File Offset: 0x000BCA7C
		private void KillIfOutOfVitality()
		{
			if (this.Vitality <= this.MinVitality && !this.Character.HasAbilityFlag(AbilityFlags.CanNotDieToAfflictions))
			{
				this.Kill();
			}
		}

		// Token: 0x060016A0 RID: 5792 RVA: 0x000BE8A4 File Offset: 0x000BCAA4
		public void SetAllDamage(float damageAmount, float bleedingDamageAmount, float burnDamageAmount)
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where a.Prefab.AfflictionType == AfflictionPrefab.InternalDamage.AfflictionType || a.Prefab.AfflictionType == AfflictionPrefab.Burn.AfflictionType || a.Prefab.AfflictionType == AfflictionPrefab.Bleeding.AfflictionType
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction);
			}
			foreach (CharacterHealth.LimbHealth limbHealth in this.limbHealths)
			{
				if (damageAmount > 0f)
				{
					this.afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(damageAmount, null), limbHealth);
				}
				if (bleedingDamageAmount > 0f && this.DoesBleed)
				{
					this.afflictions.Add(AfflictionPrefab.Bleeding.Instantiate(bleedingDamageAmount, null), limbHealth);
				}
				if (burnDamageAmount > 0f)
				{
					this.afflictions.Add(AfflictionPrefab.Burn.Instantiate(burnDamageAmount, null), limbHealth);
				}
			}
			this.RecalculateVitality();
		}

		// Token: 0x060016A1 RID: 5793 RVA: 0x000BEA00 File Offset: 0x000BCC00
		public float GetLimbDamage(Limb limb, Identifier afflictionType)
		{
			if (limb.IsSevered)
			{
				return 1f;
			}
			float max = this.MaxVitality / 2f;
			float damageStrength;
			if (afflictionType.IsEmpty)
			{
				float damage = this.GetAfflictionStrength(AfflictionPrefab.DamageType, limb, true);
				float bleeding = this.GetAfflictionStrength(AfflictionPrefab.BleedingType, limb, true);
				float burn = this.GetAfflictionStrength(AfflictionPrefab.BurnType, limb, true);
				damageStrength = Math.Min(damage + bleeding + burn, max);
			}
			else
			{
				damageStrength = Math.Min(this.GetAfflictionStrength(afflictionType, limb, true), max);
			}
			return damageStrength / max;
		}

		// Token: 0x060016A2 RID: 5794 RVA: 0x000BEA80 File Offset: 0x000BCC80
		public void RemoveAfflictions(Func<Affliction, bool> predicate)
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from affliction in this.afflictions.Keys
			where predicate(affliction)
			select affliction);
			foreach (Affliction affliction2 in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction2);
			}
			this.CalculateVitality();
		}

		// Token: 0x060016A3 RID: 5795 RVA: 0x000BEB1C File Offset: 0x000BCD1C
		public void RemoveAllAfflictions()
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where !this.irremovableAfflictions.Contains(a)
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				affliction.Strength = 0f;
				this.afflictions.Remove(affliction);
			}
			foreach (Affliction affliction2 in this.irremovableAfflictions)
			{
				affliction2.Strength = 0f;
			}
			this.CalculateVitality();
		}

		// Token: 0x060016A4 RID: 5796 RVA: 0x000BEBFC File Offset: 0x000BCDFC
		public void RemoveNegativeAfflictions()
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToRemove.AddRange(from a in this.afflictions.Keys
			where !this.irremovableAfflictions.Contains(a) && !a.Prefab.IsBuff && a.Prefab.AfflictionType != "geneticmaterialbuff" && a.Prefab.AfflictionType != "geneticmaterialdebuff"
			select a);
			foreach (Affliction affliction in CharacterHealth.afflictionsToRemove)
			{
				this.afflictions.Remove(affliction);
			}
			foreach (Affliction affliction2 in this.irremovableAfflictions)
			{
				affliction2.Strength = 0f;
			}
			this.CalculateVitality();
		}

		// Token: 0x060016A5 RID: 5797 RVA: 0x000BECD0 File Offset: 0x000BCED0
		private void AddLimbAffliction(Limb limb, Affliction newAffliction, bool allowStacking = true, bool recalculateVitality = true)
		{
			if (!newAffliction.Prefab.LimbSpecific || limb == null)
			{
				return;
			}
			if (limb.HealthIndex < 0 || limb.HealthIndex >= this.limbHealths.Count)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Limb health index out of bounds. Character\"",
					this.Character.Name,
					"\" only has health configured for",
					this.limbHealths.Count.ToString(),
					" limbs but the limb ",
					limb.type.ToString(),
					" is targeting index ",
					limb.HealthIndex.ToString()
				}), null, null, false, false);
				return;
			}
			this.AddLimbAffliction(this.limbHealths[limb.HealthIndex], limb, newAffliction, allowStacking, recalculateVitality);
		}

		// Token: 0x060016A6 RID: 5798 RVA: 0x000BEDAC File Offset: 0x000BCFAC
		private void AddLimbAffliction(CharacterHealth.LimbHealth limbHealth, Limb limb, Affliction newAffliction, bool allowStacking = true, bool recalculateVitality = true)
		{
			LimbType limbType = (limb != null) ? limb.type : LimbType.None;
			if (this.Character.Params.IsMachine && !newAffliction.Prefab.AffectMachines)
			{
				return;
			}
			if (!this.DoesBleed && newAffliction is AfflictionBleeding)
			{
				return;
			}
			if (!this.Character.NeedsOxygen && newAffliction.Prefab == AfflictionPrefab.OxygenLow)
			{
				return;
			}
			if (this.Character.Params.Health.StunImmunity && newAffliction.Prefab.AfflictionType == AfflictionPrefab.StunType && (this.Character.EmpVulnerability <= 0f || this.GetAfflictionStrengthByType(AfflictionPrefab.EMPType, false) <= 0f))
			{
				return;
			}
			if (this.Character.Params.Health.PoisonImmunity && (newAffliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || newAffliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType))
			{
				return;
			}
			if (this.Character.EmpVulnerability <= 0f && newAffliction.Prefab.AfflictionType == AfflictionPrefab.EMPType)
			{
				return;
			}
			if (newAffliction.Prefab.TargetSpecies.Any<Identifier>() && newAffliction.Prefab.TargetSpecies.None(delegate(Identifier s)
			{
				Identifier speciesName = this.Character.SpeciesName;
				return s == speciesName;
			}))
			{
				return;
			}
			if (this.Character.Params.Health.ImmunityIdentifiers.Contains(newAffliction.Identifier))
			{
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventCharacterApplyAffliction>(delegate(IEventCharacterApplyAffliction x)
			{
				bool? flag = x.OnCharacterApplyAffliction(this, limbHealth, newAffliction, allowStacking);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			Affliction existingAffliction = null;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth value = limbHealth2;
				if (value == limbHealth && affliction.Prefab == newAffliction.Prefab)
				{
					existingAffliction = affliction;
					break;
				}
			}
			float modifiedStrength = newAffliction.Strength * (100f / this.MaxVitality) * (1f - this.GetResistance(newAffliction.Prefab, limbType));
			if (newAffliction.Prefab.AfflictionType == AfflictionPrefab.StunType && (double)modifiedStrength < 0.016666666666666666 && this.Stun <= 0f)
			{
				return;
			}
			if (existingAffliction != null)
			{
				float newStrength = modifiedStrength;
				if (allowStacking)
				{
					newStrength += existingAffliction.Strength;
				}
				newStrength = Math.Min(existingAffliction.Prefab.MaxStrength, newStrength);
				existingAffliction.Strength = newStrength;
				if (existingAffliction == this.stunAffliction)
				{
					this.Character.SetStun(newStrength, true, true);
				}
				existingAffliction.Duration = existingAffliction.Prefab.Duration;
				if (newAffliction.Source != null)
				{
					existingAffliction.Source = newAffliction.Source;
				}
				if (recalculateVitality)
				{
					this.RecalculateVitality();
				}
				return;
			}
			Affliction copyAffliction = newAffliction.Prefab.Instantiate(Math.Min(newAffliction.Prefab.MaxStrength, modifiedStrength), newAffliction.Source);
			this.afflictions.Add(copyAffliction, limbHealth);
			AchievementManager.OnAfflictionReceived(copyAffliction, this.Character);
			MedicalClinic.OnAfflictionCountChanged(this.Character);
			this.Character.HealthUpdateInterval = 0f;
			if (recalculateVitality)
			{
				this.RecalculateVitality();
			}
		}

		// Token: 0x060016A7 RID: 5799 RVA: 0x000BF1A8 File Offset: 0x000BD3A8
		private void AddAffliction(Affliction newAffliction, bool allowStacking = true)
		{
			this.AddLimbAffliction(null, null, newAffliction, allowStacking, true);
		}

		// Token: 0x060016A8 RID: 5800 RVA: 0x000BF1B8 File Offset: 0x000BD3B8
		public void Update(float deltaTime)
		{
			this.WasInFullHealth = (this.vitality >= this.MaxVitality);
			this.UpdateOxygen(deltaTime);
			this.StunTimer = ((this.Stun > 0f) ? (this.StunTimer + deltaTime) : 0f);
			if (!this.Character.GodMode)
			{
				CharacterHealth.afflictionsToRemove.Clear();
				CharacterHealth.afflictionsToUpdate.Clear();
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
				{
					Affliction affliction = kvp.Key;
					if (affliction.Strength <= 0f)
					{
						AchievementManager.OnAfflictionRemoved(affliction, this.Character);
						if (!this.irremovableAfflictions.Contains(affliction))
						{
							CharacterHealth.afflictionsToRemove.Add(affliction);
						}
					}
					else
					{
						if (affliction.Prefab.Duration > 0f)
						{
							affliction.Duration -= deltaTime;
							if (affliction.Duration <= 0f)
							{
								affliction.Strength = 0f;
								CharacterHealth.afflictionsToRemove.Add(affliction);
								continue;
							}
						}
						CharacterHealth.afflictionsToUpdate.Add(kvp);
					}
				}
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in CharacterHealth.afflictionsToUpdate)
				{
					Affliction affliction2 = kvp2.Key;
					Limb targetLimb = null;
					if (kvp2.Value != null)
					{
						int healthIndex = this.limbHealths.IndexOf(kvp2.Value);
						targetLimb = (this.Character.AnimController.Limbs.LastOrDefault((Limb l) => !l.IsSevered && !l.Hidden && l.HealthIndex == healthIndex) ?? this.Character.AnimController.MainLimb);
					}
					affliction2.Update(this, targetLimb, deltaTime);
					affliction2.DamagePerSecondTimer += deltaTime;
					AfflictionBleeding bleeding = affliction2 as AfflictionBleeding;
					this.Character.StackSpeedMultiplier(affliction2.GetSpeedMultiplier());
				}
				foreach (Affliction affliction3 in CharacterHealth.afflictionsToRemove)
				{
					this.afflictions.Remove(affliction3);
				}
				if (CharacterHealth.afflictionsToRemove.Count != 0)
				{
					MedicalClinic.OnAfflictionCountChanged(this.Character);
				}
			}
			this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.MovementSpeed, true));
			if (this.Character.InWater)
			{
				this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.SwimmingSpeed, true));
			}
			else
			{
				this.Character.StackSpeedMultiplier(1f + this.Character.GetStatValue(StatTypes.WalkingSpeed, true));
			}
			this.UpdateDamageReductions(deltaTime);
			if (!this.Character.GodMode)
			{
				this.RecalculateVitality();
			}
		}

		// Token: 0x060016A9 RID: 5801 RVA: 0x000BF4C8 File Offset: 0x000BD6C8
		public void ForceUpdateVisuals()
		{
		}

		// Token: 0x060016AA RID: 5802 RVA: 0x000BF4CC File Offset: 0x000BD6CC
		private void UpdateDamageReductions(float deltaTime)
		{
			float healthRegen = this.Character.Params.Health.ConstantHealthRegeneration;
			if (healthRegen > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("damage".ToIdentifier(), healthRegen * deltaTime, null, null);
			}
			float burnReduction = this.Character.Params.Health.BurnReduction;
			if (burnReduction > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("burn".ToIdentifier(), burnReduction * deltaTime, null, null);
			}
			float bleedingReduction = this.Character.Params.Health.BleedingReduction;
			if (bleedingReduction > 0f)
			{
				this.ReduceAfflictionOnAllLimbs("bleeding".ToIdentifier(), bleedingReduction * deltaTime, null, null);
			}
		}

		// Token: 0x1700069A RID: 1690
		// (get) Token: 0x060016AB RID: 5803 RVA: 0x000BF58A File Offset: 0x000BD78A
		public float OxygenLowResistance
		{
			get
			{
				if (this.Character.NeedsOxygen)
				{
					return this.GetResistance(this.oxygenLowAffliction.Prefab, LimbType.None);
				}
				return 1f;
			}
		}

		// Token: 0x060016AC RID: 5804 RVA: 0x000BF5B4 File Offset: 0x000BD7B4
		private void UpdateOxygen(float deltaTime)
		{
			if (!this.Character.NeedsOxygen)
			{
				this.oxygenLowAffliction.Strength = 0f;
				return;
			}
			float oxygenlowResistance = this.GetResistance(this.oxygenLowAffliction.Prefab, LimbType.None);
			float prevOxygen = this.OxygenAmount;
			if (this.IsUnconscious)
			{
				float decreaseSpeed = Math.Max(0.1f, 1f - oxygenlowResistance);
				this.OxygenAmount = MathHelper.Clamp(this.OxygenAmount - decreaseSpeed * deltaTime, -100f, 100f);
				return;
			}
			float decreaseSpeed2 = -5f;
			float increaseSpeed = 10f;
			decreaseSpeed2 *= 1f - oxygenlowResistance;
			increaseSpeed *= 1f + oxygenlowResistance;
			float holdBreathMultiplier = this.Character.GetStatValue(StatTypes.HoldBreathMultiplier, true);
			if (holdBreathMultiplier <= -1f)
			{
				this.OxygenAmount = -100f;
				return;
			}
			decreaseSpeed2 /= 1f + this.Character.GetStatValue(StatTypes.HoldBreathMultiplier, true);
			this.OxygenAmount = MathHelper.Clamp(this.OxygenAmount + deltaTime * ((this.Character.OxygenAvailable < 30f) ? decreaseSpeed2 : increaseSpeed), -100f, 100f);
		}

		// Token: 0x060016AD RID: 5805 RVA: 0x000BF6C7 File Offset: 0x000BD8C7
		public void SetVitality(float newVitality)
		{
			this.UnmodifiedMaxVitality = newVitality;
			this.CalculateVitality();
		}

		// Token: 0x060016AE RID: 5806 RVA: 0x000BF6D8 File Offset: 0x000BD8D8
		private void CalculateVitality()
		{
			this.vitality = this.MaxVitality;
			this.IsParalyzed = false;
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> keyValuePair in this.afflictions)
			{
				Affliction affliction2;
				CharacterHealth.LimbHealth limbHealth2;
				keyValuePair.Deconstruct(out affliction2, out limbHealth2);
				Affliction affliction = affliction2;
				CharacterHealth.LimbHealth limbHealth = limbHealth2;
				float vitalityDecrease = affliction.GetVitalityDecrease(this);
				if (limbHealth != null)
				{
					vitalityDecrease *= CharacterHealth.GetVitalityMultiplier(affliction, limbHealth);
				}
				this.vitality -= vitalityDecrease;
				affliction.CalculateDamagePerSecond(vitalityDecrease);
				if (affliction.Strength >= affliction.Prefab.MaxStrength && affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType)
				{
					this.IsParalyzed = true;
				}
			}
		}

		// Token: 0x060016AF RID: 5807 RVA: 0x000BF7C4 File Offset: 0x000BD9C4
		public void RecalculateVitality()
		{
			this.CalculateVitality();
			this.KillIfOutOfVitality();
		}

		// Token: 0x060016B0 RID: 5808 RVA: 0x000BF7D4 File Offset: 0x000BD9D4
		private static float GetVitalityMultiplier(Affliction affliction, CharacterHealth.LimbHealth limbHealth)
		{
			float multiplier = 1f;
			float vitalityMultiplier;
			if (limbHealth.VitalityMultipliers.TryGetValue(affliction.Prefab.Identifier, out vitalityMultiplier))
			{
				multiplier *= vitalityMultiplier;
			}
			float vitalityTypeMultiplier;
			if (limbHealth.VitalityTypeMultipliers.TryGetValue(affliction.Prefab.AfflictionType, out vitalityTypeMultiplier))
			{
				multiplier *= vitalityTypeMultiplier;
			}
			return multiplier;
		}

		// Token: 0x060016B1 RID: 5809 RVA: 0x000BF824 File Offset: 0x000BDA24
		private float GetVitalityDecreaseWithVitalityMultipliers(Affliction affliction)
		{
			float vitalityDecrease = affliction.GetVitalityDecrease(this);
			CharacterHealth.LimbHealth limbHealth;
			if (this.afflictions.TryGetValue(affliction, out limbHealth) && limbHealth != null)
			{
				vitalityDecrease *= CharacterHealth.GetVitalityMultiplier(affliction, limbHealth);
			}
			return vitalityDecrease;
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x000BF858 File Offset: 0x000BDA58
		private void Kill()
		{
			if (this.Unkillable || this.Character.GodMode)
			{
				return;
			}
			ValueTuple<CauseOfDeathType, Affliction> causeOfDeath = this.GetCauseOfDeath();
			CauseOfDeathType type = causeOfDeath.Item1;
			Affliction affliction = causeOfDeath.Item2;
			this.Character.Kill(type, affliction, false, true);
			this.WasInFullHealth = false;
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x000BF8A4 File Offset: 0x000BDAA4
		public void ApplyAfflictionStatusEffects(ActionType type)
		{
			if (this.isApplyingAfflictionStatusEffects)
			{
				using (List<Affliction>.Enumerator enumerator = this.afflictions.Keys.ToList<Affliction>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Affliction affliction = enumerator.Current;
						affliction.ApplyStatusEffects(type, 1f, this, this.GetAfflictionLimb(affliction));
					}
					return;
				}
			}
			this.isApplyingAfflictionStatusEffects = true;
			this.afflictionsCopy.Clear();
			this.afflictionsCopy.AddRange(this.afflictions.Keys);
			this.isApplyingAfflictionStatusEffects = true;
			foreach (Affliction affliction2 in this.afflictionsCopy)
			{
				affliction2.ApplyStatusEffects(type, 1f, this, this.GetAfflictionLimb(affliction2));
			}
			this.isApplyingAfflictionStatusEffects = false;
		}

		// Token: 0x060016B4 RID: 5812 RVA: 0x000BF9A0 File Offset: 0x000BDBA0
		[return: TupleElementNames(new string[]
		{
			"type",
			"affliction"
		})]
		public ValueTuple<CauseOfDeathType, Affliction> GetCauseOfDeath()
		{
			IEnumerable<Affliction> currentAfflictions = this.GetAllAfflictions(true, null);
			Affliction strongestAffliction = null;
			float largestStrength = 0f;
			foreach (Affliction affliction in currentAfflictions)
			{
				if (strongestAffliction == null || affliction.GetVitalityDecrease(this) > largestStrength)
				{
					strongestAffliction = affliction;
					largestStrength = affliction.GetVitalityDecrease(this);
				}
			}
			CauseOfDeathType causeOfDeath = (strongestAffliction == null) ? CauseOfDeathType.Unknown : CauseOfDeathType.Affliction;
			if (strongestAffliction == this.oxygenLowAffliction)
			{
				causeOfDeath = (this.Character.AnimController.InWater ? CauseOfDeathType.Drowning : CauseOfDeathType.Suffocation);
			}
			return new ValueTuple<CauseOfDeathType, Affliction>(causeOfDeath, strongestAffliction);
		}

		// Token: 0x060016B5 RID: 5813 RVA: 0x000BFA44 File Offset: 0x000BDC44
		private IEnumerable<Affliction> GetAllAfflictions(bool mergeSameAfflictions, Func<Affliction, bool> predicate = null)
		{
			this.allAfflictions.Clear();
			if (!mergeSameAfflictions)
			{
				List<Affliction> list = this.allAfflictions;
				IEnumerable<Affliction> collection;
				if (predicate != null)
				{
					collection = this.afflictions.Keys.Where(predicate);
				}
				else
				{
					IEnumerable<Affliction> keys = this.afflictions.Keys;
					collection = keys;
				}
				list.AddRange(collection);
			}
			else
			{
				using (Dictionary<Affliction, CharacterHealth.LimbHealth>.KeyCollection.Enumerator enumerator = this.afflictions.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Affliction affliction = enumerator.Current;
						if (predicate == null || predicate(affliction))
						{
							Affliction existingAffliction = this.allAfflictions.Find((Affliction a) => a.Prefab == affliction.Prefab);
							if (existingAffliction == null)
							{
								Affliction newAffliction = affliction.Prefab.Instantiate(affliction.Strength, null);
								if (affliction.Source != null)
								{
									newAffliction.Source = affliction.Source;
								}
								newAffliction.DamagePerSecond = affliction.DamagePerSecond;
								newAffliction.DamagePerSecondTimer = affliction.DamagePerSecondTimer;
								this.allAfflictions.Add(newAffliction);
							}
							else
							{
								existingAffliction.DamagePerSecond += affliction.DamagePerSecond;
								existingAffliction.Strength += affliction.Strength;
							}
						}
					}
				}
			}
			return this.allAfflictions;
		}

		// Token: 0x060016B6 RID: 5814 RVA: 0x000BFBBC File Offset: 0x000BDDBC
		public void GetSuitableTreatments(Dictionary<Identifier, float> treatmentSuitability, Character user, Limb limb = null, bool ignoreHiddenAfflictions = false, bool checkTreatmentThreshold = true, bool checkTreatmentSuggestionThreshold = true, float predictFutureDuration = 0f)
		{
			treatmentSuitability.Clear();
			float minSuitability = -10f;
			float maxSuitability = 10f;
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				CharacterHealth.LimbHealth limbHealth = kvp.Value;
				if (limb != null && affliction.Prefab.LimbSpecific && this.GetMatchingLimbHealth(affliction) != this.GetMatchingLimbHealth(limb))
				{
					if (limbHealth == null)
					{
						continue;
					}
					int healthIndex = this.limbHealths.IndexOf(limbHealth);
					if (limb.HealthIndex != healthIndex)
					{
						continue;
					}
				}
				float strength = affliction.Strength;
				if (predictFutureDuration > 0f)
				{
					strength = this.GetPredictedStrength(affliction, predictFutureDuration, limb);
				}
				float totalAfflictionStrength = strength + this.GetTotalAdjustedAfflictionStrength(affliction, 0.3f, false);
				if (!this.afflictions.Any((KeyValuePair<Affliction, CharacterHealth.LimbHealth> otherAffliction) => affliction.Prefab.IgnoreTreatmentIfAfflictedBy.Contains(otherAffliction.Key.Identifier)))
				{
					if (ignoreHiddenAfflictions)
					{
						if (user == this.Character)
						{
							if (strength < affliction.Prefab.ShowIconThreshold)
							{
								continue;
							}
						}
						else if (strength < affliction.Prefab.ShowIconToOthersThreshold)
						{
							continue;
						}
					}
					foreach (KeyValuePair<Identifier, float> treatment in affliction.Prefab.TreatmentSuitabilities)
					{
						float suitability = treatment.Value * strength;
						if (suitability <= 0f || ((!checkTreatmentThreshold || totalAfflictionStrength >= affliction.Prefab.TreatmentThreshold) && (!checkTreatmentSuggestionThreshold || totalAfflictionStrength >= affliction.Prefab.TreatmentSuggestionThreshold)))
						{
							if (treatment.Value > strength)
							{
								float overtreatmentFactor = MathHelper.Clamp(treatment.Value / strength, 1f, 10f);
								suitability /= overtreatmentFactor;
							}
							if (!treatmentSuitability.ContainsKey(treatment.Key))
							{
								treatmentSuitability[treatment.Key] = suitability;
							}
							else
							{
								Identifier key = treatment.Key;
								treatmentSuitability[key] += suitability;
							}
							minSuitability = Math.Min(treatmentSuitability[treatment.Key], minSuitability);
							maxSuitability = Math.Max(treatmentSuitability[treatment.Key], maxSuitability);
						}
					}
				}
			}
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x000BFE74 File Offset: 0x000BE074
		public float GetTotalAdjustedAfflictionStrength(Affliction affliction, float otherAfflictionMultiplier = 0.3f, bool includeSameAffliction = true)
		{
			float totalAfflictionStrength = includeSameAffliction ? affliction.Strength : 0f;
			if (affliction.Prefab.LimbSpecific)
			{
				foreach (Affliction otherAffliction in this.afflictions.Keys)
				{
					if (affliction.Prefab == otherAffliction.Prefab && affliction != otherAffliction)
					{
						totalAfflictionStrength += otherAffliction.Strength * otherAfflictionMultiplier;
					}
				}
			}
			return totalAfflictionStrength;
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x000BFF04 File Offset: 0x000BE104
		public IEnumerable<Identifier> GetActiveAfflictionTags()
		{
			this.afflictionTags.Clear();
			foreach (Affliction affliction in this.afflictions.Keys)
			{
				AfflictionPrefab.Effect currentEffect = affliction.GetActiveEffect();
				if (currentEffect != null && !currentEffect.Tag.IsEmpty)
				{
					this.afflictionTags.Add(currentEffect.Tag);
				}
			}
			return this.afflictionTags;
		}

		// Token: 0x060016B9 RID: 5817 RVA: 0x000BFF94 File Offset: 0x000BE194
		public float GetPredictedStrength(Affliction affliction, float predictFutureDuration, Limb limb = null)
		{
			float strength = affliction.Strength;
			Func<ISerializableEntity, bool> <>9__0;
			foreach (DurationListElement statusEffect in StatusEffect.DurationList)
			{
				IEnumerable<ISerializableEntity> targets = statusEffect.Targets;
				Func<ISerializableEntity, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((ISerializableEntity t) => t == this.Character || (limb != null && this.Character.AnimController.Limbs.Contains(t))));
				}
				if (targets.Any(predicate))
				{
					float statusEffectDuration = Math.Min(statusEffect.Timer, predictFutureDuration);
					foreach (Affliction statusEffectAffliction in statusEffect.Parent.Afflictions)
					{
						if (statusEffectAffliction.Prefab == affliction.Prefab)
						{
							strength += statusEffectAffliction.Strength * statusEffectDuration;
						}
					}
					foreach (ValueTuple<Identifier, float> statusEffectAffliction2 in statusEffect.Parent.ReduceAffliction)
					{
						Identifier identifier = affliction.Identifier;
						if (statusEffectAffliction2.Item1 == identifier || statusEffectAffliction2.Item1 == affliction.Prefab.AfflictionType)
						{
							strength -= statusEffectAffliction2.Item2 * statusEffectDuration;
						}
					}
				}
			}
			return MathHelper.Clamp(strength, 0f, affliction.Prefab.MaxStrength);
		}

		// Token: 0x060016BA RID: 5818 RVA: 0x000C0158 File Offset: 0x000BE358
		public void ServerWrite(IWriteMessage msg)
		{
			this.activeAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				if (kvp.Value == null && affliction.Strength > 0f && affliction.Strength >= affliction.Prefab.ActivationThreshold)
				{
					this.activeAfflictions.Add(affliction);
				}
			}
			msg.WriteByte((byte)this.activeAfflictions.Count);
			foreach (Affliction affliction2 in this.activeAfflictions)
			{
				msg.WriteUInt32(affliction2.Prefab.UintIdentifier);
				msg.WriteRangedSingle(MathHelper.Clamp(affliction2.Strength, 0f, affliction2.Prefab.MaxStrength), 0f, affliction2.Prefab.MaxStrength, 8);
				msg.WriteByte((byte)affliction2.Prefab.PeriodicEffects.Count);
				foreach (AfflictionPrefab.PeriodicEffect periodicEffect in affliction2.Prefab.PeriodicEffects)
				{
					msg.WriteRangedSingle(affliction2.PeriodicEffectTimers[periodicEffect], 0f, periodicEffect.MaxInterval, 8);
				}
			}
			this.limbAfflictions.Clear();
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in this.afflictions)
			{
				Affliction limbAffliction = kvp2.Key;
				CharacterHealth.LimbHealth limbHealth = kvp2.Value;
				if (limbHealth != null && limbAffliction.Strength > 0f && limbAffliction.Strength >= limbAffliction.Prefab.ActivationThreshold)
				{
					this.limbAfflictions.Add(new ValueTuple<CharacterHealth.LimbHealth, Affliction>(limbHealth, limbAffliction));
				}
			}
			msg.WriteByte((byte)this.limbAfflictions.Count);
			foreach (ValueTuple<CharacterHealth.LimbHealth, Affliction> valueTuple in this.limbAfflictions)
			{
				CharacterHealth.LimbHealth limbHealth2 = valueTuple.Item1;
				Affliction affliction3 = valueTuple.Item2;
				msg.WriteRangedInteger(this.limbHealths.IndexOf(limbHealth2), 0, this.limbHealths.Count - 1);
				msg.WriteUInt32(affliction3.Prefab.UintIdentifier);
				msg.WriteRangedSingle(MathHelper.Clamp(affliction3.Strength, 0f, affliction3.Prefab.MaxStrength), 0f, affliction3.Prefab.MaxStrength, 8);
				msg.WriteByte((byte)affliction3.Prefab.PeriodicEffects.Count);
				foreach (AfflictionPrefab.PeriodicEffect periodicEffect2 in affliction3.Prefab.PeriodicEffects)
				{
					msg.WriteRangedSingle(affliction3.PeriodicEffectTimers[periodicEffect2], periodicEffect2.MinInterval, periodicEffect2.MaxInterval, 8);
				}
			}
		}

		// Token: 0x060016BB RID: 5819 RVA: 0x000C0528 File Offset: 0x000BE728
		public void Remove()
		{
			CharacterHealth.afflictionsToRemove.Clear();
			CharacterHealth.afflictionsToUpdate.Clear();
		}

		// Token: 0x060016BC RID: 5820 RVA: 0x000C0540 File Offset: 0x000BE740
		public static IEnumerable<Affliction> SortAfflictionsBySeverity(IEnumerable<Affliction> afflictions, bool excludeBuffs = true)
		{
			return from a in afflictions
			where !excludeBuffs || !a.Prefab.IsBuff
			orderby a.DamagePerSecond descending, a.Strength / a.Prefab.MaxStrength descending
			select a;
		}

		// Token: 0x060016BD RID: 5821 RVA: 0x000C05B4 File Offset: 0x000BE7B4
		public void Save(XElement healthElement)
		{
			foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in this.afflictions)
			{
				Affliction affliction = kvp.Key;
				CharacterHealth.LimbHealth limbHealth = kvp.Value;
				if (affliction.Strength > 0f && limbHealth == null && !kvp.Key.Prefab.ResetBetweenRounds)
				{
					healthElement.Add(new XElement("Affliction", new object[]
					{
						new XAttribute("identifier", affliction.Identifier),
						new XAttribute("strength", affliction.Strength.ToString("G", CultureInfo.InvariantCulture))
					}));
				}
			}
			int i;
			Func<KeyValuePair<Affliction, CharacterHealth.LimbHealth>, bool> <>9__0;
			int j;
			for (i = 0; i < this.limbHealths.Count; i = j + 1)
			{
				XElement limbHealthElement = new XElement("LimbHealth", new XAttribute("i", i));
				healthElement.Add(limbHealthElement);
				IEnumerable<KeyValuePair<Affliction, CharacterHealth.LimbHealth>> source = this.afflictions;
				Func<KeyValuePair<Affliction, CharacterHealth.LimbHealth>, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((KeyValuePair<Affliction, CharacterHealth.LimbHealth> a) => a.Value == this.limbHealths[i]));
				}
				foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp2 in source.Where(predicate))
				{
					Affliction affliction2 = kvp2.Key;
					CharacterHealth.LimbHealth limbHealth2 = kvp2.Value;
					if (affliction2.Strength > 0f)
					{
						limbHealthElement.Add(new XElement("Affliction", new object[]
						{
							new XAttribute("identifier", affliction2.Identifier),
							new XAttribute("strength", affliction2.Strength.ToString("G", CultureInfo.InvariantCulture))
						}));
					}
				}
				j = i;
			}
		}

		// Token: 0x060016BE RID: 5822 RVA: 0x000C0800 File Offset: 0x000BEA00
		public void Load(XElement element, Func<AfflictionPrefab, bool> afflictionPredicate = null)
		{
			CharacterHealth.<>c__DisplayClass155_0 CS$<>8__locals1;
			CS$<>8__locals1.afflictionPredicate = afflictionPredicate;
			CS$<>8__locals1.<>4__this = this;
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "affliction"))
				{
					if (a == "limbhealth")
					{
						int limbHealthIndex = subElement.GetAttributeInt("i", -1);
						if (limbHealthIndex < 0 || limbHealthIndex >= this.limbHealths.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Error while loading character health: limb index \"");
							defaultInterpolatedStringHandler.AppendFormatted<int>(limbHealthIndex);
							defaultInterpolatedStringHandler.AppendLiteral("\" out of range.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						}
						else
						{
							foreach (XElement afflictionElement in subElement.Elements())
							{
								this.<Load>g__LoadAffliction|155_0(afflictionElement, this.limbHealths[limbHealthIndex], ref CS$<>8__locals1);
							}
						}
					}
				}
				else
				{
					this.<Load>g__LoadAffliction|155_0(subElement, null, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x000C09D8 File Offset: 0x000BEBD8
		[CompilerGenerated]
		private void <Load>g__LoadAffliction|155_0(XElement afflictionElement, CharacterHealth.LimbHealth limbHealth = null, ref CharacterHealth.<>c__DisplayClass155_0 A_3)
		{
			string id = afflictionElement.GetAttributeString("identifier", "");
			AfflictionPrefab afflictionPrefab = AfflictionPrefab.Prefabs.Find((AfflictionPrefab a) => a.Identifier == id);
			if (afflictionPrefab == null)
			{
				DebugConsole.ThrowError("Error while loading character health: affliction \"" + id + "\" not found.", null, null, false, false);
				return;
			}
			if (A_3.afflictionPredicate != null && !A_3.afflictionPredicate(afflictionPrefab))
			{
				return;
			}
			float strength = afflictionElement.GetAttributeFloat("strength", 0f);
			Affliction irremovableAffliction = this.irremovableAfflictions.FirstOrDefault((Affliction a) => a.Prefab == afflictionPrefab);
			if (irremovableAffliction != null)
			{
				irremovableAffliction.Strength = strength;
				return;
			}
			this.afflictions.Add(afflictionPrefab.Instantiate(strength, null), limbHealth);
		}

		// Token: 0x04000AE4 RID: 2788
		public const float InsufficientOxygenThreshold = 30f;

		// Token: 0x04000AE5 RID: 2789
		public const float LowOxygenThreshold = 50f;

		// Token: 0x04000AE6 RID: 2790
		protected float minVitality;

		// Token: 0x04000AE7 RID: 2791
		public bool Unkillable;

		// Token: 0x04000AE8 RID: 2792
		private readonly List<CharacterHealth.LimbHealth> limbHealths = new List<CharacterHealth.LimbHealth>();

		// Token: 0x04000AE9 RID: 2793
		private readonly Dictionary<Affliction, CharacterHealth.LimbHealth> afflictions = new Dictionary<Affliction, CharacterHealth.LimbHealth>();

		// Token: 0x04000AEA RID: 2794
		private readonly HashSet<Affliction> irremovableAfflictions = new HashSet<Affliction>();

		// Token: 0x04000AEB RID: 2795
		private Affliction bloodlossAffliction;

		// Token: 0x04000AEC RID: 2796
		private Affliction oxygenLowAffliction;

		// Token: 0x04000AED RID: 2797
		private Affliction pressureAffliction;

		// Token: 0x04000AEE RID: 2798
		private Affliction stunAffliction;

		// Token: 0x04000AF0 RID: 2800
		private float vitality;

		// Token: 0x04000AF1 RID: 2801
		public static readonly Color DefaultFaceTint = Color.TransparentBlack;

		// Token: 0x04000AF7 RID: 2807
		public bool ShowDamageOverlay = true;

		// Token: 0x04000AF8 RID: 2808
		public readonly Character Character;

		// Token: 0x04000AF9 RID: 2809
		private readonly List<Affliction> matchingAfflictions = new List<Affliction>();

		// Token: 0x04000AFA RID: 2810
		private static readonly List<Affliction> afflictionsToRemove = new List<Affliction>();

		// Token: 0x04000AFB RID: 2811
		private static readonly List<KeyValuePair<Affliction, CharacterHealth.LimbHealth>> afflictionsToUpdate = new List<KeyValuePair<Affliction, CharacterHealth.LimbHealth>>();

		// Token: 0x04000AFC RID: 2812
		private readonly List<Affliction> afflictionsCopy = new List<Affliction>();

		// Token: 0x04000AFD RID: 2813
		private bool isApplyingAfflictionStatusEffects;

		// Token: 0x04000AFE RID: 2814
		private readonly List<Affliction> allAfflictions = new List<Affliction>();

		// Token: 0x04000AFF RID: 2815
		private readonly HashSet<Identifier> afflictionTags = new HashSet<Identifier>();

		// Token: 0x04000B00 RID: 2816
		private readonly List<Affliction> activeAfflictions = new List<Affliction>();

		// Token: 0x04000B01 RID: 2817
		[TupleElementNames(new string[]
		{
			"limbHealth",
			"affliction"
		})]
		private readonly List<ValueTuple<CharacterHealth.LimbHealth, Affliction>> limbAfflictions = new List<ValueTuple<CharacterHealth.LimbHealth, Affliction>>();

		// Token: 0x0200087F RID: 2175
		public class LimbHealth
		{
			// Token: 0x06005538 RID: 21816 RVA: 0x001F21FA File Offset: 0x001F03FA
			public LimbHealth()
			{
			}

			// Token: 0x06005539 RID: 21817 RVA: 0x001F2218 File Offset: 0x001F0418
			public LimbHealth(ContentXElement element, CharacterHealth characterHealth)
			{
				string limbName = element.GetAttributeString("name", null) ?? "generic";
				if (limbName != "generic")
				{
					this.Name = TextManager.Get("HealthLimbName." + limbName);
				}
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "sprite"))
					{
						if (!(a == "highlightsprite"))
						{
							if (a == "vitalitymultiplier")
							{
								if (subElement.GetAttribute("name") != null)
								{
									DebugConsole.ThrowError("Error in character health config (" + characterHealth.Character.Name + ") - define vitality multipliers using affliction identifiers or types instead of names.", null, element.ContentPackage, false, false);
								}
								else
								{
									Identifier[] vitalityMultipliers = subElement.GetAttributeIdentifierArray("identifier", null, true) ?? subElement.GetAttributeIdentifierArray("identifiers", null, true);
									if (vitalityMultipliers != null)
									{
										float multiplier = subElement.GetAttributeFloat("multiplier", 1f);
										Identifier[] array = vitalityMultipliers;
										for (int i = 0; i < array.Length; i++)
										{
											Identifier vitalityMultiplier = array[i];
											this.VitalityMultipliers.Add(vitalityMultiplier, multiplier);
											if (AfflictionPrefab.Prefabs.None((AfflictionPrefab p) => p.Identifier == vitalityMultiplier))
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(169, 2);
												defaultInterpolatedStringHandler.AppendLiteral("Potentially incorrectly defined vitality multiplier in \"");
												defaultInterpolatedStringHandler.AppendFormatted(characterHealth.Character.Name);
												defaultInterpolatedStringHandler.AppendLiteral("\". Could not find any afflictions with the identifier \"");
												defaultInterpolatedStringHandler.AppendFormatted<Identifier>(vitalityMultiplier);
												defaultInterpolatedStringHandler.AppendLiteral("\". Did you mean to define the afflictions by type instead?");
												DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
											}
										}
									}
									Identifier[] vitalityTypeMultipliers = subElement.GetAttributeIdentifierArray("type", null, true) ?? subElement.GetAttributeIdentifierArray("types", null, true);
									if (vitalityTypeMultipliers != null)
									{
										float multiplier2 = subElement.GetAttributeFloat("multiplier", 1f);
										Identifier[] array2 = vitalityTypeMultipliers;
										for (int j = 0; j < array2.Length; j++)
										{
											Identifier vitalityTypeMultiplier = array2[j];
											this.VitalityTypeMultipliers.Add(vitalityTypeMultiplier, multiplier2);
											if (AfflictionPrefab.Prefabs.None((AfflictionPrefab p) => p.AfflictionType == vitalityTypeMultiplier))
											{
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(167, 2);
												defaultInterpolatedStringHandler2.AppendLiteral("Potentially incorrectly defined vitality multiplier in \"");
												defaultInterpolatedStringHandler2.AppendFormatted(characterHealth.Character.Name);
												defaultInterpolatedStringHandler2.AppendLiteral("\". Could not find any afflictions of the type \"");
												defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(vitalityTypeMultiplier);
												defaultInterpolatedStringHandler2.AppendLiteral("\". Did you mean to define the afflictions by identifier instead?");
												DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), element.ContentPackage);
											}
										}
									}
									if (vitalityMultipliers == null && this.VitalityTypeMultipliers == null)
									{
										DebugConsole.ThrowError("Error in character health config " + characterHealth.Character.Name + ": affliction identifier(s) or type(s) not defined in the \"VitalityMultiplier\" elements!", null, element.ContentPackage, false, false);
									}
								}
							}
						}
						else
						{
							this.HighlightSprite = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.IndicatorSprite = new Sprite(subElement, "", "", false, 1f);
						ContentXElement contentXElement = subElement;
						string key = "highlightarea";
						Rectangle rectangle = new Rectangle(0, 0, (int)this.IndicatorSprite.size.X, (int)this.IndicatorSprite.size.Y);
						this.HighlightArea = contentXElement.GetAttributeRect(key, rectangle);
					}
				}
			}

			// Token: 0x04003004 RID: 12292
			public Sprite IndicatorSprite;

			// Token: 0x04003005 RID: 12293
			public Sprite HighlightSprite;

			// Token: 0x04003006 RID: 12294
			public Rectangle HighlightArea;

			// Token: 0x04003007 RID: 12295
			public readonly LocalizedString Name;

			// Token: 0x04003008 RID: 12296
			public readonly Dictionary<Identifier, float> VitalityMultipliers = new Dictionary<Identifier, float>();

			// Token: 0x04003009 RID: 12297
			public readonly Dictionary<Identifier, float> VitalityTypeMultipliers = new Dictionary<Identifier, float>();
		}
	}
}
