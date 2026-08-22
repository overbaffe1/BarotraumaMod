using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F8 RID: 504
	internal sealed class TalentSubTree
	{
		// Token: 0x17000E29 RID: 3625
		// (get) Token: 0x060034DF RID: 13535 RVA: 0x0020EBBE File Offset: 0x0020CDBE
		public Identifier Identifier { get; }

		// Token: 0x17000E2A RID: 3626
		// (get) Token: 0x060034E0 RID: 13536 RVA: 0x0020EBC6 File Offset: 0x0020CDC6
		public LocalizedString DisplayName { get; }

		// Token: 0x060034E1 RID: 13537 RVA: 0x0020EBD0 File Offset: 0x0020CDD0
		public bool HasEnoughTalents(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.All((TalentOption option) => option.HasEnoughTalents(talents));
		}

		// Token: 0x060034E2 RID: 13538 RVA: 0x0020EC04 File Offset: 0x0020CE04
		public bool HasMaxTalents(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.All((TalentOption option) => option.HasMaxTalents(talents));
		}

		// Token: 0x060034E3 RID: 13539 RVA: 0x0020EC38 File Offset: 0x0020CE38
		public bool HasAnyTalent(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.Any((TalentOption option) => option.HasSelectedTalent(talents));
		}

		// Token: 0x060034E4 RID: 13540 RVA: 0x0020EC6C File Offset: 0x0020CE6C
		public TalentSubTree(ContentXElement subTreeElement)
		{
			this.Identifier = subTreeElement.GetAttributeIdentifier("identifier", "");
			string nameIdentifier = subTreeElement.GetAttributeString("nameidentifier", string.Empty);
			if (string.IsNullOrWhiteSpace(nameIdentifier))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("talenttree.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				nameIdentifier = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			this.DisplayName = TextManager.Get(nameIdentifier).Fallback(this.Identifier.Value, true);
			string key = "type";
			TalentTreeType talentTreeType = TalentTreeType.Specialization;
			this.Type = subTreeElement.GetAttributeEnum<TalentTreeType>(key, talentTreeType);
			this.RequiredTrees = subTreeElement.GetAttributeIdentifierImmutableHashSet("requires", ImmutableHashSet<Identifier>.Empty, true);
			this.BlockedTrees = subTreeElement.GetAttributeIdentifierImmutableHashSet("blocks", ImmutableHashSet<Identifier>.Empty, true);
			List<TalentOption> talentOptionStages = new List<TalentOption>();
			foreach (ContentXElement talentOptionsElement in subTreeElement.GetChildElements("talentoptions"))
			{
				talentOptionStages.Add(new TalentOption(talentOptionsElement, this.Identifier));
			}
			this.TalentOptionStages = talentOptionStages.ToImmutableArray<TalentOption>();
			this.AllTalentIdentifiers = this.TalentOptionStages.SelectMany((TalentOption t) => t.TalentIdentifiers).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x04001B8A RID: 7050
		public readonly ImmutableArray<TalentOption> TalentOptionStages;

		// Token: 0x04001B8B RID: 7051
		public readonly ImmutableHashSet<Identifier> AllTalentIdentifiers;

		// Token: 0x04001B8C RID: 7052
		public readonly TalentTreeType Type;

		// Token: 0x04001B8D RID: 7053
		public readonly ImmutableHashSet<Identifier> RequiredTrees;

		// Token: 0x04001B8E RID: 7054
		public readonly ImmutableHashSet<Identifier> BlockedTrees;
	}
}
