using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma.Abilities
{
	// Token: 0x02000424 RID: 1060
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityUnlockApprenticeshipTalentTree : CharacterAbility
	{
		// Token: 0x17001233 RID: 4659
		// (get) Token: 0x0600473C RID: 18236 RVA: 0x002705E4 File Offset: 0x0026E7E4
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600473D RID: 18237 RVA: 0x002705E7 File Offset: 0x0026E7E7
		public CharacterAbilityUnlockApprenticeshipTalentTree(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x0600473E RID: 18238 RVA: 0x002705F4 File Offset: 0x0026E7F4
		public override void InitializeAbility(bool addingFirstTime)
		{
			if (!addingFirstTime)
			{
				return;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			JobPrefab apprentice = CharacterAbilityApplyStatusEffectsToApprenticeship.GetApprenticeJob(base.Character, JobPrefab.Prefabs.ToImmutableHashSet<JobPrefab>());
			if (apprentice == null)
			{
				DebugConsole.ThrowError("CharacterAbilityUnlockApprenticeshipTalentTree: Could not find apprentice job for character " + base.Character.Name, null, base.CharacterTalent.Prefab.ContentPackage, false, false);
				return;
			}
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(apprentice.Identifier, out talentTree))
			{
				return;
			}
			IEnumerable<Character> characters = Character.GetFriendlyCrew(base.Character);
			HashSet<ImmutableHashSet<Identifier>> talentsTrees = new HashSet<ImmutableHashSet<Identifier>>();
			foreach (TalentSubTree subTree in talentTree.TalentSubTrees)
			{
				if (subTree.Type == TalentTreeType.Specialization)
				{
					HashSet<Identifier> identifiers = new HashSet<Identifier>();
					foreach (TalentOption option in subTree.TalentOptionStages)
					{
						foreach (Identifier identifier in option.TalentIdentifiers)
						{
							if (!CharacterAbilityUnlockApprenticeshipTalentTree.<InitializeAbility>g__IsShowCaseTalent|3_0(identifier, option) && !base.Character.IsTalentLocked(identifier))
							{
								identifiers.Add(identifier);
							}
						}
						foreach (KeyValuePair<Identifier, ImmutableHashSet<Identifier>> keyValuePair in option.ShowCaseTalents)
						{
							Identifier identifier3;
							ImmutableHashSet<Identifier> immutableHashSet;
							keyValuePair.Deconstruct(out identifier3, out immutableHashSet);
							ImmutableHashSet<Identifier> value = immutableHashSet;
							ImmutableHashSet<Identifier> ids = (from i in value
							where !base.Character.IsTalentLocked(i)
							select i).ToImmutableHashSet<Identifier>();
							if (ids.Count != 0)
							{
								identifiers.Add(value.GetRandomUnsynced<Identifier>());
							}
						}
					}
					talentsTrees.Add(identifiers.ToImmutableHashSet<Identifier>());
				}
			}
			ImmutableHashSet<Identifier> selectedTalentTree = talentsTrees.GetRandomUnsynced<ImmutableHashSet<Identifier>>();
			if (selectedTalentTree != null)
			{
				foreach (Identifier identifier2 in selectedTalentTree)
				{
					if (!base.Character.HasTalent(identifier2))
					{
						base.Character.GiveTalent(identifier2, true);
						base.Character.Info.ResettableExtraTalents.Add(identifier2);
					}
				}
			}
		}

		// Token: 0x0600473F RID: 18239 RVA: 0x00270858 File Offset: 0x0026EA58
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x06004741 RID: 18241 RVA: 0x00270874 File Offset: 0x0026EA74
		[CompilerGenerated]
		internal static bool <InitializeAbility>g__IsShowCaseTalent|3_0(Identifier identifier, TalentOption option)
		{
			foreach (KeyValuePair<Identifier, ImmutableHashSet<Identifier>> keyValuePair in option.ShowCaseTalents)
			{
				Identifier identifier2;
				ImmutableHashSet<Identifier> immutableHashSet;
				keyValuePair.Deconstruct(out identifier2, out immutableHashSet);
				ImmutableHashSet<Identifier> value = immutableHashSet;
				if (value.Contains(identifier))
				{
					return true;
				}
			}
			return false;
		}
	}
}
