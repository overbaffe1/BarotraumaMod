using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma.Abilities
{
	// Token: 0x0200035E RID: 862
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CharacterAbilityUnlockApprenticeshipTalentTree : CharacterAbility
	{
		// Token: 0x17000E43 RID: 3651
		// (get) Token: 0x06003302 RID: 13058 RVA: 0x00158764 File Offset: 0x00156964
		public override bool AllowClientSimulation
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003303 RID: 13059 RVA: 0x00158767 File Offset: 0x00156967
		public CharacterAbilityUnlockApprenticeshipTalentTree(CharacterAbilityGroup characterAbilityGroup, ContentXElement abilityElement) : base(characterAbilityGroup, abilityElement)
		{
		}

		// Token: 0x06003304 RID: 13060 RVA: 0x00158774 File Offset: 0x00156974
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

		// Token: 0x06003305 RID: 13061 RVA: 0x001589D8 File Offset: 0x00156BD8
		protected override void ApplyEffect(AbilityObject abilityObject)
		{
			this.ApplyEffect();
		}

		// Token: 0x06003307 RID: 13063 RVA: 0x001589F4 File Offset: 0x00156BF4
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
