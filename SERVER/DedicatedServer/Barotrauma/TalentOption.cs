using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020000FD RID: 253
	internal readonly struct TalentOption
	{
		// Token: 0x170007C3 RID: 1987
		// (get) Token: 0x060019E4 RID: 6628 RVA: 0x000C83AC File Offset: 0x000C65AC
		public IEnumerable<Identifier> TalentIdentifiers
		{
			get
			{
				return this.talentIdentifiers;
			}
		}

		// Token: 0x060019E5 RID: 6629 RVA: 0x000C83B4 File Offset: 0x000C65B4
		public bool HasEnoughTalents(CharacterInfo character)
		{
			return this.CountMatchingTalents(character.UnlockedTalents) >= this.RequiredTalents;
		}

		// Token: 0x060019E6 RID: 6630 RVA: 0x000C83CD File Offset: 0x000C65CD
		public bool HasEnoughTalents(IReadOnlyCollection<Identifier> selectedTalents)
		{
			return this.CountMatchingTalents(selectedTalents) >= this.RequiredTalents;
		}

		// Token: 0x060019E7 RID: 6631 RVA: 0x000C83E1 File Offset: 0x000C65E1
		public bool HasMaxTalents(IReadOnlyCollection<Identifier> selectedTalents)
		{
			return this.CountMatchingTalents(selectedTalents) >= this.MaxChosenTalents;
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x000C83F8 File Offset: 0x000C65F8
		public bool HasSelectedTalent(IReadOnlyCollection<Identifier> selectedTalents)
		{
			foreach (Identifier talent in selectedTalents)
			{
				if (this.talentIdentifiers.Contains(talent))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060019E9 RID: 6633 RVA: 0x000C8450 File Offset: 0x000C6650
		public int CountMatchingTalents(IReadOnlyCollection<Identifier> talents)
		{
			int i = 0;
			foreach (Identifier talent in talents)
			{
				if (this.talentIdentifiers.Contains(talent))
				{
					i++;
				}
			}
			return i;
		}

		// Token: 0x060019EA RID: 6634 RVA: 0x000C84A8 File Offset: 0x000C66A8
		public TalentOption(ContentXElement talentOptionsElement, Identifier debugIdentifier)
		{
			this.ShowCaseTalents = new Dictionary<Identifier, ImmutableHashSet<Identifier>>();
			this.MaxChosenTalents = talentOptionsElement.GetAttributeInt("MaxChosenTalents", 1);
			this.RequiredTalents = talentOptionsElement.GetAttributeInt("RequiredTalents", this.MaxChosenTalents);
			if (this.RequiredTalents > this.MaxChosenTalents)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in talent tree ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(debugIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(" - MaxChosenTalents is larger than RequiredTalents.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, talentOptionsElement.ContentPackage, false, false);
			}
			HashSet<Identifier> identifiers = new HashSet<Identifier>();
			foreach (ContentXElement talentOptionElement in talentOptionsElement.Elements())
			{
				Identifier elementName = talentOptionElement.Name.ToIdentifier<XName>();
				if (elementName == "talentoption")
				{
					identifiers.Add(talentOptionElement.GetAttributeIdentifier("identifier", Identifier.Empty));
				}
				else if (elementName == "showcasetalent")
				{
					Identifier showCaseIdentifier = talentOptionElement.GetAttributeIdentifier("identifier", Identifier.Empty);
					HashSet<Identifier> showCaseTalentIdentifiers = new HashSet<Identifier>();
					foreach (ContentXElement subElement in talentOptionElement.Elements())
					{
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						showCaseTalentIdentifiers.Add(identifier);
						identifiers.Add(identifier);
					}
					this.ShowCaseTalents.Add(showCaseIdentifier, showCaseTalentIdentifiers.ToImmutableHashSet<Identifier>());
				}
			}
			this.talentIdentifiers = identifiers.ToImmutableHashSet<Identifier>();
			if (this.RequiredTalents > this.talentIdentifiers.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(105, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in talent tree ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(debugIdentifier);
				defaultInterpolatedStringHandler2.AppendLiteral(" - completing a stage of the tree requires more talents than there are in the stage.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, talentOptionsElement.ContentPackage, false, false);
			}
			if (this.MaxChosenTalents > this.talentIdentifiers.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(97, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Error in talent tree ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(debugIdentifier);
				defaultInterpolatedStringHandler3.AppendLiteral(" - maximum number of talents to choose is larger than the number of talents.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, talentOptionsElement.ContentPackage, false, false);
			}
		}

		// Token: 0x04000C68 RID: 3176
		private readonly ImmutableHashSet<Identifier> talentIdentifiers;

		// Token: 0x04000C69 RID: 3177
		public readonly int RequiredTalents;

		// Token: 0x04000C6A RID: 3178
		public readonly int MaxChosenTalents;

		// Token: 0x04000C6B RID: 3179
		public readonly Dictionary<Identifier, ImmutableHashSet<Identifier>> ShowCaseTalents;
	}
}
