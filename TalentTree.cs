using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F6 RID: 502
	internal sealed class TalentTree : Prefab
	{
		// Token: 0x17000E28 RID: 3624
		// (get) Token: 0x060034D4 RID: 13524 RVA: 0x0020E71E File Offset: 0x0020C91E
		// (set) Token: 0x060034D5 RID: 13525 RVA: 0x0020E726 File Offset: 0x0020C926
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x060034D6 RID: 13526 RVA: 0x0020E730 File Offset: 0x0020C930
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

		// Token: 0x060034D7 RID: 13527 RVA: 0x0020E844 File Offset: 0x0020CA44
		public bool TalentIsInTree(Identifier talentIdentifier)
		{
			return this.AllTalentIdentifiers.Contains(talentIdentifier);
		}

		// Token: 0x060034D8 RID: 13528 RVA: 0x0020E854 File Offset: 0x0020CA54
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

		// Token: 0x060034D9 RID: 13529 RVA: 0x0020E88C File Offset: 0x0020CA8C
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

		// Token: 0x060034DA RID: 13530 RVA: 0x0020E908 File Offset: 0x0020CB08
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

		// Token: 0x060034DB RID: 13531 RVA: 0x0020EA20 File Offset: 0x0020CC20
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

		// Token: 0x060034DC RID: 13532 RVA: 0x0020EB38 File Offset: 0x0020CD38
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

		// Token: 0x060034DD RID: 13533 RVA: 0x0020EBB0 File Offset: 0x0020CDB0
		public override void Dispose()
		{
		}

		// Token: 0x04001B81 RID: 7041
		public static readonly PrefabCollection<TalentTree> JobTalentTrees = new PrefabCollection<TalentTree>();

		// Token: 0x04001B82 RID: 7042
		public readonly ImmutableArray<TalentSubTree> TalentSubTrees;

		// Token: 0x04001B83 RID: 7043
		public readonly ImmutableHashSet<Identifier> AllTalentIdentifiers;

		// Token: 0x02000EDC RID: 3804
		public enum TalentStages
		{
			// Token: 0x0400541C RID: 21532
			Invalid,
			// Token: 0x0400541D RID: 21533
			Locked,
			// Token: 0x0400541E RID: 21534
			Unlocked,
			// Token: 0x0400541F RID: 21535
			Available,
			// Token: 0x04005420 RID: 21536
			Highlighted
		}
	}
}
