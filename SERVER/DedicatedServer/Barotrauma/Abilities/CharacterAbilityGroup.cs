using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000360 RID: 864
	internal abstract class CharacterAbilityGroup
	{
		// Token: 0x17000E44 RID: 3652
		// (get) Token: 0x0600330B RID: 13067 RVA: 0x00158CB4 File Offset: 0x00156EB4
		public CharacterTalent CharacterTalent { get; }

		// Token: 0x17000E45 RID: 3653
		// (get) Token: 0x0600330C RID: 13068 RVA: 0x00158CBC File Offset: 0x00156EBC
		public Character Character { get; }

		// Token: 0x17000E46 RID: 3654
		// (get) Token: 0x0600330D RID: 13069 RVA: 0x00158CC4 File Offset: 0x00156EC4
		// (set) Token: 0x0600330E RID: 13070 RVA: 0x00158CCC File Offset: 0x00156ECC
		public bool IsActive { get; private set; } = true;

		// Token: 0x0600330F RID: 13071 RVA: 0x00158CD8 File Offset: 0x00156ED8
		public CharacterAbilityGroup(AbilityEffectType abilityEffectType, CharacterTalent characterTalent, ContentXElement abilityElementGroup)
		{
			this.AbilityEffectType = abilityEffectType;
			if (characterTalent == null)
			{
				throw new ArgumentNullException("characterTalent");
			}
			this.CharacterTalent = characterTalent;
			this.Character = this.CharacterTalent.Character;
			this.maxTriggerCount = abilityElementGroup.GetAttributeInt("maxtriggercount", int.MaxValue);
			foreach (ContentXElement subElement in abilityElementGroup.Elements())
			{
				string a2 = subElement.Name.ToString().ToLowerInvariant();
				if (!(a2 == "abilities"))
				{
					if (!(a2 == "fallbackabilities"))
					{
						if (!(a2 == "condition") && !(a2 == "conditions"))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Error in talent ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(characterTalent.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(": unrecognized xml element \"");
							defaultInterpolatedStringHandler.AppendFormatted<XName>(subElement.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						}
						else
						{
							this.LoadConditions(subElement);
						}
					}
					else
					{
						this.LoadFallbackAbilities(subElement);
					}
				}
				else
				{
					this.LoadAbilities(subElement);
				}
			}
			if (abilityEffectType == AbilityEffectType.OnDieToCharacter)
			{
				if (this.characterAbilities.Any((CharacterAbility a) => a.RequiresAlive))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(155, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Potential error in talent ");
					defaultInterpolatedStringHandler2.AppendFormatted<CharacterTalent>(characterTalent);
					defaultInterpolatedStringHandler2.AppendLiteral(": an ability group has the type ");
					defaultInterpolatedStringHandler2.AppendFormatted<AbilityEffectType>(AbilityEffectType.OnDieToCharacter);
					defaultInterpolatedStringHandler2.AppendLiteral(", but includes abilities that require the character to be alive, meaning they will never execute.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), characterTalent.Prefab.ContentPackage);
				}
			}
		}

		// Token: 0x06003310 RID: 13072 RVA: 0x00158EE4 File Offset: 0x001570E4
		public void ActivateAbilityGroup(bool addingFirstTime)
		{
			if (!this.CheckActivatingCondition())
			{
				return;
			}
			foreach (CharacterAbility characterAbility in this.characterAbilities)
			{
				characterAbility.InitializeAbility(addingFirstTime);
			}
			foreach (CharacterAbility characterAbility2 in this.fallbackAbilities)
			{
				characterAbility2.InitializeAbility(addingFirstTime);
			}
		}

		// Token: 0x06003311 RID: 13073 RVA: 0x00158F84 File Offset: 0x00157184
		private bool CheckActivatingCondition()
		{
			if (this.AbilityEffectType != AbilityEffectType.None)
			{
				return true;
			}
			return !this.abilityConditions.Any((AbilityCondition abilityCondition) => !abilityCondition.MatchesCondition());
		}

		// Token: 0x06003312 RID: 13074 RVA: 0x00158FC0 File Offset: 0x001571C0
		public void LoadConditions(ContentXElement conditionElements)
		{
			foreach (ContentXElement conditionElement in conditionElements.Elements())
			{
				AbilityCondition newCondition = this.ConstructCondition(this.CharacterTalent, conditionElement, true);
				if (newCondition == null)
				{
					DebugConsole.ThrowError("AbilityCondition was not found in talent " + this.CharacterTalent.DebugIdentifier + "!", null, conditionElement.ContentPackage, false, false);
					break;
				}
				if (!newCondition.AllowClientSimulation && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					this.IsActive = false;
				}
				this.abilityConditions.Add(newCondition);
			}
		}

		// Token: 0x06003313 RID: 13075 RVA: 0x00159070 File Offset: 0x00157270
		public void AddAbility(CharacterAbility characterAbility)
		{
			if (characterAbility == null)
			{
				DebugConsole.ThrowError("Trying to add null ability for talent " + this.CharacterTalent.DebugIdentifier + "!", null, this.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			this.characterAbilities.Add(characterAbility);
		}

		// Token: 0x06003314 RID: 13076 RVA: 0x001590C0 File Offset: 0x001572C0
		public void AddFallbackAbility(CharacterAbility characterAbility)
		{
			if (characterAbility == null)
			{
				DebugConsole.ThrowError("Trying to add null ability for talent " + this.CharacterTalent.DebugIdentifier + "!", null, this.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			this.fallbackAbilities.Add(characterAbility);
		}

		// Token: 0x06003315 RID: 13077 RVA: 0x00159110 File Offset: 0x00157310
		private AbilityCondition ConstructCondition(CharacterTalent characterTalent, ContentXElement conditionElement, bool errorMessages = true)
		{
			string type = conditionElement.Name.ToString().ToLowerInvariant();
			Type conditionType;
			try
			{
				conditionType = ReflectionUtils.GetTypeWithBackwardsCompatibility(ToolBox.BarotraumaAssembly, "Barotrauma.Abilities", type, false, true);
				if (conditionType == null)
				{
					if (errorMessages)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Could not find the component \"",
							type,
							"\" (",
							characterTalent.DebugIdentifier,
							")"
						}), null, characterTalent.Prefab.ContentPackage, false, false);
					}
					return null;
				}
			}
			catch (Exception e)
			{
				if (errorMessages)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Could not find the component \"",
						type,
						"\" (",
						characterTalent.DebugIdentifier,
						")"
					}), e, characterTalent.Prefab.ContentPackage, false, false);
				}
				return null;
			}
			object[] args = new object[]
			{
				characterTalent,
				conditionElement
			};
			AbilityCondition newCondition;
			try
			{
				newCondition = (AbilityCondition)Activator.CreateInstance(conditionType, args);
			}
			catch (TargetInvocationException e2)
			{
				string str = "Error while creating an instance of an ability condition of the type ";
				Type type2 = conditionType;
				DebugConsole.ThrowError(str + ((type2 != null) ? type2.ToString() : null) + ".", e2.InnerException, characterTalent.Prefab.ContentPackage, false, false);
				return null;
			}
			if (newCondition == null)
			{
				string str2 = "Error while creating an instance of an ability condition of the type ";
				Type type3 = conditionType;
				DebugConsole.ThrowError(str2 + ((type3 != null) ? type3.ToString() : null) + ", instance was null", null, characterTalent.Prefab.ContentPackage, false, false);
				return null;
			}
			return newCondition;
		}

		// Token: 0x06003316 RID: 13078 RVA: 0x0015929C File Offset: 0x0015749C
		private void LoadAbilities(ContentXElement abilityElements)
		{
			foreach (ContentXElement abilityElementGroup in abilityElements.Elements())
			{
				this.AddAbility(this.ConstructAbility(abilityElementGroup, this.CharacterTalent));
			}
		}

		// Token: 0x06003317 RID: 13079 RVA: 0x001592F8 File Offset: 0x001574F8
		private void LoadFallbackAbilities(ContentXElement abilityElements)
		{
			foreach (ContentXElement abilityElementGroup in abilityElements.Elements())
			{
				this.AddFallbackAbility(this.ConstructAbility(abilityElementGroup, this.CharacterTalent));
			}
		}

		// Token: 0x06003318 RID: 13080 RVA: 0x00159354 File Offset: 0x00157554
		private CharacterAbility ConstructAbility(ContentXElement abilityElement, CharacterTalent characterTalent)
		{
			CharacterAbility newAbility = CharacterAbility.Load(abilityElement, this, true);
			if (newAbility == null)
			{
				DebugConsole.ThrowError("Unable to create an ability for " + characterTalent.DebugIdentifier + "!", null, characterTalent.Prefab.ContentPackage, false, false);
				return null;
			}
			return newAbility;
		}

		// Token: 0x06003319 RID: 13081 RVA: 0x00159398 File Offset: 0x00157598
		public static List<StatusEffect> ParseStatusEffects(CharacterTalent characterTalent, ContentXElement statusEffectElements)
		{
			ContentXElement contentXElement = null;
			if (statusEffectElements == contentXElement)
			{
				DebugConsole.ThrowError("StatusEffect list was not found in talent " + characterTalent.DebugIdentifier, null, characterTalent.Prefab.ContentPackage, false, false);
				return null;
			}
			List<StatusEffect> statusEffects = new List<StatusEffect>();
			foreach (ContentXElement statusEffectElement in statusEffectElements.Elements())
			{
				StatusEffect statusEffect = StatusEffect.Load(statusEffectElement, characterTalent.DebugIdentifier);
				statusEffects.Add(statusEffect);
			}
			return statusEffects;
		}

		// Token: 0x0600331A RID: 13082 RVA: 0x00159430 File Offset: 0x00157630
		public static StatTypes ParseStatType(string statTypeString, string debugIdentifier)
		{
			if (statTypeString.Equals("MedicalItemDurationMultiplier", StringComparison.OrdinalIgnoreCase))
			{
				statTypeString = "BuffItemApplyingMultiplier";
			}
			StatTypes statType;
			if (!Enum.TryParse<StatTypes>(statTypeString, true, out statType))
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Invalid stat type type \"",
					statTypeString,
					"\" in CharacterTalent (",
					debugIdentifier,
					")"
				}), null, null, false, false);
			}
			return statType;
		}

		// Token: 0x0600331B RID: 13083 RVA: 0x00159494 File Offset: 0x00157694
		public static List<Affliction> ParseAfflictions(CharacterTalent characterTalent, ContentXElement afflictionElements)
		{
			ContentXElement contentXElement = null;
			if (afflictionElements == contentXElement)
			{
				DebugConsole.ThrowError("Affliction list was not found in talent " + characterTalent.DebugIdentifier, null, characterTalent.Prefab.ContentPackage, false, false);
				return null;
			}
			List<Affliction> afflictions = new List<Affliction>();
			foreach (ContentXElement afflictionElement in afflictionElements.Elements())
			{
				Identifier afflictionIdentifier = afflictionElement.GetAttributeIdentifier("identifier", "");
				AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab ap) => ap.Identifier == afflictionIdentifier);
				if (afflictionPrefab == null)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in CharacterTalent (",
						characterTalent.DebugIdentifier,
						") - Affliction prefab with the identifier \"",
						afflictionIdentifier.ToString(),
						"\" not found."
					}), null, characterTalent.Prefab.ContentPackage, false, false);
				}
				else
				{
					Affliction afflictionInstance = afflictionPrefab.Instantiate(afflictionElement.GetAttributeFloat(1f, new string[]
					{
						"amount",
						"strength"
					}), null);
					afflictionInstance.Probability = afflictionElement.GetAttributeFloat(1f, new string[]
					{
						"probability"
					});
					afflictions.Add(afflictionInstance);
				}
			}
			return afflictions;
		}

		// Token: 0x0600331C RID: 13084 RVA: 0x00159600 File Offset: 0x00157800
		public static AbilityFlags ParseFlagType(string flagTypeString, string debugIdentifier)
		{
			AbilityFlags flagType;
			if (!Enum.TryParse<AbilityFlags>(flagTypeString, true, out flagType))
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Invalid flag type type \"",
					flagTypeString,
					"\" in CharacterTalent (",
					debugIdentifier,
					")"
				}), null, null, false, false);
			}
			return flagType;
		}

		// Token: 0x04001958 RID: 6488
		public readonly AbilityEffectType AbilityEffectType;

		// Token: 0x04001959 RID: 6489
		protected readonly int maxTriggerCount;

		// Token: 0x0400195A RID: 6490
		protected int timesTriggered;

		// Token: 0x0400195B RID: 6491
		protected readonly List<AbilityCondition> abilityConditions = new List<AbilityCondition>();

		// Token: 0x0400195C RID: 6492
		protected readonly List<CharacterAbility> characterAbilities = new List<CharacterAbility>();

		// Token: 0x0400195D RID: 6493
		protected readonly List<CharacterAbility> fallbackAbilities = new List<CharacterAbility>();
	}
}
