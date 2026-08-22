using System;
using System.Collections.Generic;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020001F1 RID: 497
	internal class CharacterTalent
	{
		// Token: 0x17000E1A RID: 3610
		// (get) Token: 0x060034A7 RID: 13479 RVA: 0x0020DB93 File Offset: 0x0020BD93
		public Character Character { get; }

		// Token: 0x17000E1B RID: 3611
		// (get) Token: 0x060034A8 RID: 13480 RVA: 0x0020DB9B File Offset: 0x0020BD9B
		public string DebugIdentifier { get; }

		// Token: 0x17000E1C RID: 3612
		// (get) Token: 0x060034A9 RID: 13481 RVA: 0x0020DBA3 File Offset: 0x0020BDA3
		public List<Identifier> UnlockedRecipes { get; } = new List<Identifier>();

		// Token: 0x17000E1D RID: 3613
		// (get) Token: 0x060034AA RID: 13482 RVA: 0x0020DBAB File Offset: 0x0020BDAB
		public List<Identifier> UnlockedStoreItems { get; } = new List<Identifier>();

		// Token: 0x060034AB RID: 13483 RVA: 0x0020DBB4 File Offset: 0x0020BDB4
		public CharacterTalent(TalentPrefab talentPrefab, Character character)
		{
			if (character == null)
			{
				throw new ArgumentNullException("character");
			}
			this.Character = character;
			if (talentPrefab == null)
			{
				throw new ArgumentNullException("talentPrefab");
			}
			this.Prefab = talentPrefab;
			ContentXElement element = talentPrefab.ConfigElement;
			this.DebugIdentifier = talentPrefab.OriginalName;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "abilitygroupeffect"))
				{
					if (!(a == "abilitygroupinterval"))
					{
						if (!(a == "addedrecipe"))
						{
							if (a == "addedstoreitem")
							{
								Identifier storeItemTag = subElement.GetAttributeIdentifier("itemtag", Identifier.Empty);
								if (!storeItemTag.IsEmpty)
								{
									this.UnlockedStoreItems.Add(storeItemTag);
								}
								else
								{
									DebugConsole.ThrowError("No store item identifier defined for talent " + this.DebugIdentifier, null, element.ContentPackage, false, false);
								}
							}
						}
						else
						{
							Identifier recipeIdentifier = subElement.GetAttributeIdentifier("itemidentifier", Identifier.Empty);
							if (!recipeIdentifier.IsEmpty)
							{
								this.UnlockedRecipes.Add(recipeIdentifier);
							}
							else
							{
								DebugConsole.ThrowError("No recipe identifier defined for talent " + this.DebugIdentifier, null, element.ContentPackage, false, false);
							}
						}
					}
					else
					{
						this.LoadAbilityGroupInterval(subElement);
					}
				}
				else
				{
					this.LoadAbilityGroupEffect(subElement);
				}
			}
		}

		// Token: 0x060034AC RID: 13484 RVA: 0x0020DD7C File Offset: 0x0020BF7C
		public virtual void UpdateTalent(float deltaTime)
		{
			foreach (CharacterAbilityGroupInterval characterAbilityGroupInterval in this.characterAbilityGroupIntervals)
			{
				characterAbilityGroupInterval.UpdateAbilityGroup(deltaTime);
			}
		}

		// Token: 0x060034AD RID: 13485 RVA: 0x0020DDD0 File Offset: 0x0020BFD0
		public static void CheckTalentsForCrew(IEnumerable<Character> crew, AbilityEffectType type, AbilityObject abilityObject)
		{
			CharacterTalent.checkedNonStackableTalents.Clear();
			foreach (Character character in crew)
			{
				foreach (CharacterTalent characterTalent in character.CharacterTalents)
				{
					if (!characterTalent.Prefab.AbilityEffectsStackWithSameTalent)
					{
						if (CharacterTalent.checkedNonStackableTalents.Contains(characterTalent.Prefab.Identifier))
						{
							continue;
						}
						CharacterTalent.checkedNonStackableTalents.Add(characterTalent.Prefab.Identifier);
					}
					characterTalent.CheckTalent(type, abilityObject);
				}
			}
		}

		// Token: 0x060034AE RID: 13486 RVA: 0x0020DE94 File Offset: 0x0020C094
		public void CheckTalent(AbilityEffectType abilityEffectType, AbilityObject abilityObject)
		{
			List<CharacterAbilityGroupEffect> characterAbilityGroups;
			if (this.characterAbilityGroupEffectDictionary.TryGetValue(abilityEffectType, out characterAbilityGroups))
			{
				foreach (CharacterAbilityGroupEffect characterAbilityGroup in characterAbilityGroups)
				{
					characterAbilityGroup.CheckAbilityGroup(abilityObject);
				}
			}
		}

		// Token: 0x060034AF RID: 13487 RVA: 0x0020DEF4 File Offset: 0x0020C0F4
		public void ActivateTalent(bool addingFirstTime)
		{
			foreach (List<CharacterAbilityGroupEffect> characterAbilityGroups in this.characterAbilityGroupEffectDictionary.Values)
			{
				foreach (CharacterAbilityGroupEffect characterAbilityGroup in characterAbilityGroups)
				{
					characterAbilityGroup.ActivateAbilityGroup(addingFirstTime);
				}
			}
		}

		// Token: 0x060034B0 RID: 13488 RVA: 0x0020DF84 File Offset: 0x0020C184
		private void LoadAbilityGroupInterval(ContentXElement abilityGroup)
		{
			this.characterAbilityGroupIntervals.Add(new CharacterAbilityGroupInterval(AbilityEffectType.Undefined, this, abilityGroup));
		}

		// Token: 0x060034B1 RID: 13489 RVA: 0x0020DF9C File Offset: 0x0020C19C
		private void LoadAbilityGroupEffect(ContentXElement abilityGroup)
		{
			AbilityEffectType abilityEffectType = CharacterTalent.ParseAbilityEffectType(this, abilityGroup.GetAttributeString("abilityeffecttype", "none"));
			this.AddAbilityGroupEffect(new CharacterAbilityGroupEffect(abilityEffectType, this, abilityGroup), abilityEffectType);
		}

		// Token: 0x060034B2 RID: 13490 RVA: 0x0020DFD0 File Offset: 0x0020C1D0
		public void AddAbilityGroupEffect(CharacterAbilityGroupEffect characterAbilityGroup, AbilityEffectType abilityEffectType = AbilityEffectType.None)
		{
			List<CharacterAbilityGroupEffect> characterAbilityList;
			if (this.characterAbilityGroupEffectDictionary.TryGetValue(abilityEffectType, out characterAbilityList))
			{
				characterAbilityList.Add(characterAbilityGroup);
				return;
			}
			List<CharacterAbilityGroupEffect> characterAbilityGroups = new List<CharacterAbilityGroupEffect>();
			characterAbilityGroups.Add(characterAbilityGroup);
			this.characterAbilityGroupEffectDictionary.Add(abilityEffectType, characterAbilityGroups);
		}

		// Token: 0x060034B3 RID: 13491 RVA: 0x0020E010 File Offset: 0x0020C210
		public static AbilityEffectType ParseAbilityEffectType(CharacterTalent characterTalent, string abilityEffectTypeString)
		{
			AbilityEffectType abilityEffectType;
			if (!Enum.TryParse<AbilityEffectType>(abilityEffectTypeString, true, out abilityEffectType))
			{
				string error = string.Concat(new string[]
				{
					"Invalid ability effect type \"",
					abilityEffectTypeString,
					"\" in CharacterTalent (",
					characterTalent.DebugIdentifier,
					")"
				});
				Exception e = null;
				ContentPackage contentPackage;
				if (characterTalent == null)
				{
					contentPackage = null;
				}
				else
				{
					TalentPrefab prefab = characterTalent.Prefab;
					contentPackage = ((prefab != null) ? prefab.ContentPackage : null);
				}
				DebugConsole.ThrowError(error, e, contentPackage, false, false);
			}
			if (abilityEffectType == AbilityEffectType.Undefined)
			{
				string error2 = "Ability effect type not defined in CharacterTalent (" + characterTalent.DebugIdentifier + ")";
				Exception e2 = null;
				ContentPackage contentPackage2;
				if (characterTalent == null)
				{
					contentPackage2 = null;
				}
				else
				{
					TalentPrefab prefab2 = characterTalent.Prefab;
					contentPackage2 = ((prefab2 != null) ? prefab2.ContentPackage : null);
				}
				DebugConsole.ThrowError(error2, e2, contentPackage2, false, false);
			}
			return abilityEffectType;
		}

		// Token: 0x04001B68 RID: 7016
		public readonly TalentPrefab Prefab;

		// Token: 0x04001B69 RID: 7017
		public bool AddedThisRound = true;

		// Token: 0x04001B6A RID: 7018
		private readonly Dictionary<AbilityEffectType, List<CharacterAbilityGroupEffect>> characterAbilityGroupEffectDictionary = new Dictionary<AbilityEffectType, List<CharacterAbilityGroupEffect>>();

		// Token: 0x04001B6B RID: 7019
		private readonly List<CharacterAbilityGroupInterval> characterAbilityGroupIntervals = new List<CharacterAbilityGroupInterval>();

		// Token: 0x04001B6E RID: 7022
		private static readonly HashSet<Identifier> checkedNonStackableTalents = new HashSet<Identifier>();
	}
}
