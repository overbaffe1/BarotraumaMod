using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000FA RID: 250
	internal sealed class TalentTree : Prefab
	{
		// Token: 0x170007C0 RID: 1984
		// (get) Token: 0x060019D3 RID: 6611 RVA: 0x000C7CE2 File Offset: 0x000C5EE2
		// (set) Token: 0x060019D4 RID: 6612 RVA: 0x000C7CEA File Offset: 0x000C5EEA
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x060019D5 RID: 6613 RVA: 0x000C7CF4 File Offset: 0x000C5EF4
		public TalentTree(ContentXElement element, TalentTreesFile file) : base(file, element.GetAttributeIdentifier("jobIdentifier", ""))
		{
			this.ConfigElement = element;
			if (this.Identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No job defined for talent tree in \"");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
				defaultInterpolatedStringHandler.AppendLiteral("\"!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				return;
			}
			List<TalentSubTree> subTrees = new List<TalentSubTree>();
			foreach (ContentXElement subTreeElement in element.GetChildElements("subtree"))
			{
				subTrees.Add(new TalentSubTree(subTreeElement));
			}
			this.TalentSubTrees = subTrees.ToImmutableArray<TalentSubTree>();
			this.AllTalentIdentifiers = this.TalentSubTrees.SelectMany((TalentSubTree t) => t.AllTalentIdentifiers).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x060019D6 RID: 6614 RVA: 0x000C7E08 File Offset: 0x000C6008
		public bool TalentIsInTree(Identifier talentIdentifier)
		{
			return this.AllTalentIdentifiers.Contains(talentIdentifier);
		}

		// Token: 0x060019D7 RID: 6615 RVA: 0x000C7E18 File Offset: 0x000C6018
		public static bool IsViableTalentForCharacter(Character character, Identifier talentIdentifier)
		{
			IReadOnlyCollection<Identifier> readOnlyCollection;
			if (character == null)
			{
				readOnlyCollection = null;
			}
			else
			{
				CharacterInfo info = character.Info;
				readOnlyCollection = ((info != null) ? info.UnlockedTalents : null);
			}
			IReadOnlyCollection<Identifier> readOnlyCollection2 = readOnlyCollection;
			return TalentTree.IsViableTalentForCharacter(character, talentIdentifier, readOnlyCollection2 ?? Array.Empty<Identifier>());
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x000C7E50 File Offset: 0x000C6050
		public static bool TalentTreeMeetsRequirements(TalentTree tree, TalentSubTree targetTree, IReadOnlyCollection<Identifier> selectedTalents)
		{
			IEnumerable<TalentSubTree> blockingSubTrees = from tst in tree.TalentSubTrees
			where tst.BlockedTrees.Contains(targetTree.Identifier)
			select tst;
			IEnumerable<TalentSubTree> requiredSubTrees = from tst in tree.TalentSubTrees
			where targetTree.RequiredTrees.Contains(tst.Identifier)
			select tst;
			return requiredSubTrees.All((TalentSubTree tst) => tst.HasEnoughTalents(selectedTalents)) && !blockingSubTrees.Any((TalentSubTree tst) => tst.HasAnyTalent(selectedTalents) && !tst.HasMaxTalents(selectedTalents));
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x000C7ECC File Offset: 0x000C60CC
		public static TalentTree.TalentStages GetTalentOptionStageState(Character character, Identifier subTreeIdentifier, int index, IReadOnlyCollection<Identifier> selectedTalents)
		{
			bool flag;
			if (character == null)
			{
				flag = (null != null);
			}
			else
			{
				CharacterInfo info = character.Info;
				flag = (((info != null) ? info.Job.Prefab : null) != null);
			}
			if (!flag)
			{
				return TalentTree.TalentStages.Invalid;
			}
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(character.Info.Job.Prefab.Identifier, out talentTree))
			{
				return TalentTree.TalentStages.Invalid;
			}
			TalentSubTree subTree = talentTree.TalentSubTrees.FirstOrDefault(delegate(TalentSubTree tst)
			{
				Identifier identifier = tst.Identifier;
				return identifier == subTreeIdentifier;
			});
			if (subTree == null)
			{
				return TalentTree.TalentStages.Invalid;
			}
			TalentOption targetTalentOption = subTree.TalentOptionStages[index];
			if (targetTalentOption.HasEnoughTalents(character.Info))
			{
				return TalentTree.TalentStages.Unlocked;
			}
			if (!TalentTree.TalentTreeMeetsRequirements(talentTree, subTree, selectedTalents))
			{
				return TalentTree.TalentStages.Locked;
			}
			if (targetTalentOption.HasSelectedTalent(selectedTalents))
			{
				return TalentTree.TalentStages.Highlighted;
			}
			bool hasTalentInLastTier = true;
			bool isLastTalentPurchased = true;
			int lastindex = index - 1;
			if (lastindex >= 0)
			{
				TalentOption lastLatentOption = subTree.TalentOptionStages[lastindex];
				hasTalentInLastTier = lastLatentOption.HasEnoughTalents(selectedTalents);
				isLastTalentPurchased = lastLatentOption.HasEnoughTalents(character.Info);
			}
			if (!hasTalentInLastTier)
			{
				return TalentTree.TalentStages.Locked;
			}
			bool hasPointsForNewTalent = character.Info.GetTotalTalentPoints() - selectedTalents.Count > 0;
			if (!hasPointsForNewTalent)
			{
				return TalentTree.TalentStages.Locked;
			}
			if (!isLastTalentPurchased)
			{
				return TalentTree.TalentStages.Available;
			}
			return TalentTree.TalentStages.Highlighted;
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x000C7FE4 File Offset: 0x000C61E4
		public static bool IsViableTalentForCharacter(Character character, Identifier talentIdentifier, IReadOnlyCollection<Identifier> selectedTalents)
		{
			bool flag;
			if (character == null)
			{
				flag = (null != null);
			}
			else
			{
				CharacterInfo info = character.Info;
				flag = (((info != null) ? info.Job.Prefab : null) != null);
			}
			if (!flag)
			{
				return false;
			}
			if (character.Info.GetTotalTalentPoints() - selectedTalents.Count <= 0)
			{
				return false;
			}
			TalentTree talentTree;
			if (!TalentTree.JobTalentTrees.TryGet(character.Info.Job.Prefab.Identifier, out talentTree))
			{
				return false;
			}
			if (character.IsTalentLocked(talentIdentifier))
			{
				return false;
			}
			if (character.Info.GetUnlockedTalentsInTree().Contains(talentIdentifier))
			{
				return true;
			}
			foreach (TalentSubTree subTree in talentTree.TalentSubTrees)
			{
				if (subTree.AllTalentIdentifiers.Contains(talentIdentifier) && subTree.HasMaxTalents(selectedTalents))
				{
					return false;
				}
				foreach (TalentOption talentOptionStage in subTree.TalentOptionStages)
				{
					if (talentOptionStage.TalentIdentifiers.Contains(talentIdentifier))
					{
						return !talentOptionStage.HasMaxTalents(selectedTalents) && TalentTree.TalentTreeMeetsRequirements(talentTree, subTree, selectedTalents);
					}
					bool optionStageCompleted = talentOptionStage.HasEnoughTalents(selectedTalents);
					if (!optionStageCompleted)
					{
						break;
					}
				}
			}
			return false;
		}

		// Token: 0x060019DB RID: 6619 RVA: 0x000C80FC File Offset: 0x000C62FC
		public static List<Identifier> CheckTalentSelection(Character controlledCharacter, IEnumerable<Identifier> selectedTalents)
		{
			List<Identifier> viableTalents = new List<Identifier>();
			bool canStillUnlock = true;
			while (canStillUnlock && selectedTalents.Any<Identifier>())
			{
				canStillUnlock = false;
				foreach (Identifier talent in selectedTalents)
				{
					if (!viableTalents.Contains(talent) && TalentTree.IsViableTalentForCharacter(controlledCharacter, talent, viableTalents))
					{
						viableTalents.Add(talent);
						canStillUnlock = true;
					}
				}
			}
			return viableTalents;
		}

		// Token: 0x060019DC RID: 6620 RVA: 0x000C8174 File Offset: 0x000C6374
		public override void Dispose()
		{
		}

		// Token: 0x04000C5A RID: 3162
		public static readonly PrefabCollection<TalentTree> JobTalentTrees = new PrefabCollection<TalentTree>();

		// Token: 0x04000C5B RID: 3163
		public readonly ImmutableArray<TalentSubTree> TalentSubTrees;

		// Token: 0x04000C5C RID: 3164
		public readonly ImmutableHashSet<Identifier> AllTalentIdentifiers;

		// Token: 0x020008C3 RID: 2243
		public enum TalentStages
		{
			// Token: 0x04003139 RID: 12601
			Invalid,
			// Token: 0x0400313A RID: 12602
			Locked,
			// Token: 0x0400313B RID: 12603
			Unlocked,
			// Token: 0x0400313C RID: 12604
			Available,
			// Token: 0x0400313D RID: 12605
			Highlighted
		}
	}
}
