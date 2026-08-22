using System;
using System.Collections.Generic;
using Barotrauma.Abilities;

namespace Barotrauma
{
	// Token: 0x020000F5 RID: 245
	internal class CharacterTalent
	{
		// Token: 0x170007B2 RID: 1970
		// (get) Token: 0x060019A6 RID: 6566 RVA: 0x000C718F File Offset: 0x000C538F
		public Character Character { get; }

		// Token: 0x170007B3 RID: 1971
		// (get) Token: 0x060019A7 RID: 6567 RVA: 0x000C7197 File Offset: 0x000C5397
		public string DebugIdentifier { get; }

		// Token: 0x170007B4 RID: 1972
		// (get) Token: 0x060019A8 RID: 6568 RVA: 0x000C719F File Offset: 0x000C539F
		public List<Identifier> UnlockedRecipes { get; } = new List<Identifier>();

		// Token: 0x170007B5 RID: 1973
		// (get) Token: 0x060019A9 RID: 6569 RVA: 0x000C71A7 File Offset: 0x000C53A7
		public List<Identifier> UnlockedStoreItems { get; } = new List<Identifier>();

		// Token: 0x060019AA RID: 6570 RVA: 0x000C71B0 File Offset: 0x000C53B0
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

		// Token: 0x060019AB RID: 6571 RVA: 0x000C7378 File Offset: 0x000C5578
		public virtual void UpdateTalent(float deltaTime)
		{
			foreach (CharacterAbilityGroupInterval characterAbilityGroupInterval in this.characterAbilityGroupIntervals)
			{
				characterAbilityGroupInterval.UpdateAbilityGroup(deltaTime);
			}
		}

		// Token: 0x060019AC RID: 6572 RVA: 0x000C73CC File Offset: 0x000C55CC
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

		// Token: 0x060019AD RID: 6573 RVA: 0x000C7490 File Offset: 0x000C5690
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

		// Token: 0x060019AE RID: 6574 RVA: 0x000C74F0 File Offset: 0x000C56F0
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

		// Token: 0x060019AF RID: 6575 RVA: 0x000C7580 File Offset: 0x000C5780
		private void LoadAbilityGroupInterval(ContentXElement abilityGroup)
		{
			this.characterAbilityGroupIntervals.Add(new CharacterAbilityGroupInterval(AbilityEffectType.Undefined, this, abilityGroup));
		}

		// Token: 0x060019B0 RID: 6576 RVA: 0x000C7598 File Offset: 0x000C5798
		private void LoadAbilityGroupEffect(ContentXElement abilityGroup)
		{
			AbilityEffectType abilityEffectType = CharacterTalent.ParseAbilityEffectType(this, abilityGroup.GetAttributeString("abilityeffecttype", "none"));
			this.AddAbilityGroupEffect(new CharacterAbilityGroupEffect(abilityEffectType, this, abilityGroup), abilityEffectType);
		}

		// Token: 0x060019B1 RID: 6577 RVA: 0x000C75CC File Offset: 0x000C57CC
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

		// Token: 0x060019B2 RID: 6578 RVA: 0x000C760C File Offset: 0x000C580C
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

		// Token: 0x04000C42 RID: 3138
		public readonly TalentPrefab Prefab;

		// Token: 0x04000C43 RID: 3139
		public bool AddedThisRound = true;

		// Token: 0x04000C44 RID: 3140
		private readonly Dictionary<AbilityEffectType, List<CharacterAbilityGroupEffect>> characterAbilityGroupEffectDictionary = new Dictionary<AbilityEffectType, List<CharacterAbilityGroupEffect>>();

		// Token: 0x04000C45 RID: 3141
		private readonly List<CharacterAbilityGroupInterval> characterAbilityGroupIntervals = new List<CharacterAbilityGroupInterval>();

		// Token: 0x04000C48 RID: 3144
		private static readonly HashSet<Identifier> checkedNonStackableTalents = new HashSet<Identifier>();
	}
}
