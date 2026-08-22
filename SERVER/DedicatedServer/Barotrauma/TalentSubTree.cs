using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000FC RID: 252
	internal sealed class TalentSubTree
	{
		// Token: 0x170007C1 RID: 1985
		// (get) Token: 0x060019DE RID: 6622 RVA: 0x000C8182 File Offset: 0x000C6382
		public Identifier Identifier { get; }

		// Token: 0x170007C2 RID: 1986
		// (get) Token: 0x060019DF RID: 6623 RVA: 0x000C818A File Offset: 0x000C638A
		public LocalizedString DisplayName { get; }

		// Token: 0x060019E0 RID: 6624 RVA: 0x000C8194 File Offset: 0x000C6394
		public bool HasEnoughTalents(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.All((TalentOption option) => option.HasEnoughTalents(talents));
		}

		// Token: 0x060019E1 RID: 6625 RVA: 0x000C81C8 File Offset: 0x000C63C8
		public bool HasMaxTalents(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.All((TalentOption option) => option.HasMaxTalents(talents));
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x000C81FC File Offset: 0x000C63FC
		public bool HasAnyTalent(IReadOnlyCollection<Identifier> talents)
		{
			return this.TalentOptionStages.Any((TalentOption option) => option.HasSelectedTalent(talents));
		}

		// Token: 0x060019E3 RID: 6627 RVA: 0x000C8230 File Offset: 0x000C6430
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

		// Token: 0x04000C63 RID: 3171
		public readonly ImmutableArray<TalentOption> TalentOptionStages;

		// Token: 0x04000C64 RID: 3172
		public readonly ImmutableHashSet<Identifier> AllTalentIdentifiers;

		// Token: 0x04000C65 RID: 3173
		public readonly TalentTreeType Type;

		// Token: 0x04000C66 RID: 3174
		public readonly ImmutableHashSet<Identifier> RequiredTrees;

		// Token: 0x04000C67 RID: 3175
		public readonly ImmutableHashSet<Identifier> BlockedTrees;
	}
}
