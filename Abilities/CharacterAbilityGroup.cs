using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.Abilities
{
	// Token: 0x02000426 RID: 1062
	internal abstract class CharacterAbilityGroup
	{
		// Token: 0x17001234 RID: 4660
		// (get) Token: 0x06004745 RID: 18245 RVA: 0x00270B34 File Offset: 0x0026ED34
		public CharacterTalent CharacterTalent { get; }

		// Token: 0x17001235 RID: 4661
		// (get) Token: 0x06004746 RID: 18246 RVA: 0x00270B3C File Offset: 0x0026ED3C
		public Character Character { get; }

		// Token: 0x17001236 RID: 4662
		// (get) Token: 0x06004747 RID: 18247 RVA: 0x00270B44 File Offset: 0x0026ED44
		// (set) Token: 0x06004748 RID: 18248 RVA: 0x00270B4C File Offset: 0x0026ED4C
		public bool IsActive { get; private set; } = true;

		// Token: 0x06004749 RID: 18249 RVA: 0x00270B58 File Offset: 0x0026ED58
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

		// Token: 0x0600474A RID: 18250 RVA: 0x00270D64 File Offset: 0x0026EF64
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

		// Token: 0x0600474B RID: 18251 RVA: 0x00270E04 File Offset: 0x0026F004
		private bool CheckActivatingCondition()
		{
			if (this.AbilityEffectType != AbilityEffectType.None)
			{
				return true;
			}
			return !this.abilityConditions.Any((AbilityCondition abilityCondition) => !abilityCondition.MatchesCondition());
		}

		// Token: 0x0600474C RID: 18252 RVA: 0x00270E40 File Offset: 0x0026F040
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

		// Token: 0x0600474D RID: 18253 RVA: 0x00270EF0 File Offset: 0x0026F0F0
		public void AddAbility(CharacterAbility characterAbility)
		{
			if (characterAbility == null)
			{
				DebugConsole.ThrowError("Trying to add null ability for talent " + this.CharacterTalent.DebugIdentifier + "!", null, this.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			this.characterAbilities.Add(characterAbility);
		}

		// Token: 0x0600474E RID: 18254 RVA: 0x00270F40 File Offset: 0x0026F140
		public void AddFallbackAbility(CharacterAbility characterAbility)
		{
			if (characterAbility == null)
			{
				DebugConsole.ThrowError("Trying to add null ability for talent " + this.CharacterTalent.DebugIdentifier + "!", null, this.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			this.fallbackAbilities.Add(characterAbility);
		}

		// Token: 0x0600474F RID: 18255 RVA: 0x00270F90 File Offset: 0x0026F190
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

		// Token: 0x06004750 RID: 18256 RVA: 0x0027111C File Offset: 0x0026F31C
		private void LoadAbilities(ContentXElement abilityElements)
		{
			foreach (ContentXElement abilityElementGroup in abilityElements.Elements())
			{
				this.AddAbility(this.ConstructAbility(abilityElementGroup, this.CharacterTalent));
			}
		}

		// Token: 0x06004751 RID: 18257 RVA: 0x00271178 File Offset: 0x0026F378
		private void LoadFallbackAbilities(ContentXElement abilityElements)
		{
			foreach (ContentXElement abilityElementGroup in abilityElements.Elements())
			{
				this.AddFallbackAbility(this.ConstructAbility(abilityElementGroup, this.CharacterTalent));
			}
		}

		// Token: 0x06004752 RID: 18258 RVA: 0x002711D4 File Offset: 0x0026F3D4
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

		// Token: 0x06004753 RID: 18259 RVA: 0x00271218 File Offset: 0x0026F418
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

		// Token: 0x06004754 RID: 18260 RVA: 0x002712B0 File Offset: 0x0026F4B0
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

		// Token: 0x06004755 RID: 18261 RVA: 0x00271314 File Offset: 0x0026F514
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

		// Token: 0x06004756 RID: 18262 RVA: 0x00271480 File Offset: 0x0026F680
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

		// Token: 0x04002519 RID: 9497
		public readonly AbilityEffectType AbilityEffectType;

		// Token: 0x0400251A RID: 9498
		protected readonly int maxTriggerCount;

		// Token: 0x0400251B RID: 9499
		protected int timesTriggered;

		// Token: 0x0400251C RID: 9500
		protected readonly List<AbilityCondition> abilityConditions = new List<AbilityCondition>();

		// Token: 0x0400251D RID: 9501
		protected readonly List<CharacterAbility> characterAbilities = new List<CharacterAbility>();

		// Token: 0x0400251E RID: 9502
		protected readonly List<CharacterAbility> fallbackAbilities = new List<CharacterAbility>();
	}
}
