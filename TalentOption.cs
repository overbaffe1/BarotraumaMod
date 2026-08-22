using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001F9 RID: 505
	internal readonly struct TalentOption
	{
		// Token: 0x17000E2B RID: 3627
		// (get) Token: 0x060034E5 RID: 13541 RVA: 0x0020EDE8 File Offset: 0x0020CFE8
		public IEnumerable<Identifier> TalentIdentifiers
		{
			get
			{
				return this.talentIdentifiers;
			}
		}

		// Token: 0x060034E6 RID: 13542 RVA: 0x0020EDF0 File Offset: 0x0020CFF0
		public bool HasEnoughTalents(CharacterInfo character)
		{
			return this.CountMatchingTalents(character.UnlockedTalents) >= this.RequiredTalents;
		}

		// Token: 0x060034E7 RID: 13543 RVA: 0x0020EE09 File Offset: 0x0020D009
		public bool HasEnoughTalents(IReadOnlyCollection<Identifier> selectedTalents)
		{
			return this.CountMatchingTalents(selectedTalents) >= this.RequiredTalents;
		}

		// Token: 0x060034E8 RID: 13544 RVA: 0x0020EE1D File Offset: 0x0020D01D
		public bool HasMaxTalents(IReadOnlyCollection<Identifier> selectedTalents)
		{
			return this.CountMatchingTalents(selectedTalents) >= this.MaxChosenTalents;
		}

		// Token: 0x060034E9 RID: 13545 RVA: 0x0020EE34 File Offset: 0x0020D034
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

		// Token: 0x060034EA RID: 13546 RVA: 0x0020EE8C File Offset: 0x0020D08C
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

		// Token: 0x060034EB RID: 13547 RVA: 0x0020EEE4 File Offset: 0x0020D0E4
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

		// Token: 0x04001B8F RID: 7055
		private readonly ImmutableHashSet<Identifier> talentIdentifiers;

		// Token: 0x04001B90 RID: 7056
		public readonly int RequiredTalents;

		// Token: 0x04001B91 RID: 7057
		public readonly int MaxChosenTalents;

		// Token: 0x04001B92 RID: 7058
		public readonly Dictionary<Identifier, ImmutableHashSet<Identifier>> ShowCaseTalents;
	}
}
