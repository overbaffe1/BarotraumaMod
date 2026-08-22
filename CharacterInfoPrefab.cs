using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001BD RID: 445
	internal class CharacterInfoPrefab
	{
		// Token: 0x0600315D RID: 12637 RVA: 0x0020427C File Offset: 0x0020247C
		public CharacterInfoPrefab(CharacterPrefab characterPrefab, ContentXElement headsElement, XElement varsElement, XElement menuCategoryElement, XElement pronounsElement)
		{
			ContentXElement contentXElement = null;
			if (headsElement == contentXElement)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(151, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No heads configured for the character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(characterPrefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Characters with CharacterInfo must have head sprites. Please add a <Heads> element to the character's config.");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.Heads = (from e in headsElement.Elements()
			select new CharacterInfo.HeadPreset(this, e)).ToImmutableArray<CharacterInfo.HeadPreset>();
			if (varsElement != null)
			{
				this.VarTags = (from e in varsElement.Elements()
				select new ValueTuple<Identifier, ImmutableHashSet<Identifier>>(e.GetAttributeIdentifier("var", ""), e.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>())).ToImmutableDictionary<Identifier, ImmutableHashSet<Identifier>>();
			}
			else
			{
				this.VarTags = new ValueTuple<Identifier, ImmutableHashSet<Identifier>>[]
				{
					new ValueTuple<Identifier, ImmutableHashSet<Identifier>>("GENDER".ToIdentifier(), new Identifier[]
					{
						"female".ToIdentifier(),
						"male".ToIdentifier()
					}.ToImmutableHashSet<Identifier>())
				}.ToImmutableDictionary<Identifier, ImmutableHashSet<Identifier>>();
			}
			this.MenuCategoryVar = ((menuCategoryElement != null) ? menuCategoryElement.GetAttributeIdentifier("var", Identifier.Empty) : "GENDER".ToIdentifier());
			this.Pronouns = ((pronounsElement != null) ? pronounsElement.GetAttributeIdentifier("vars", Identifier.Empty) : "GENDER".ToIdentifier());
		}

		// Token: 0x0600315E RID: 12638 RVA: 0x002043D9 File Offset: 0x002025D9
		public string ReplaceVars(string str, CharacterInfo.HeadPreset headPreset)
		{
			return this.ReplaceVars(str, headPreset.TagSet);
		}

		// Token: 0x0600315F RID: 12639 RVA: 0x002043E8 File Offset: 0x002025E8
		public string ReplaceVars(string str, ImmutableHashSet<Identifier> tagSet)
		{
			using (IEnumerator<Identifier> enumerator = this.VarTags.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier key = enumerator.Current;
					string text = str;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(key);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					str = text.Replace(defaultInterpolatedStringHandler.ToStringAndClear(), tagSet.FirstOrDefault((Identifier t) => this.VarTags[key].Contains(t)).Value, StringComparison.OrdinalIgnoreCase);
				}
			}
			return str;
		}

		// Token: 0x040019B0 RID: 6576
		public readonly ImmutableArray<CharacterInfo.HeadPreset> Heads;

		// Token: 0x040019B1 RID: 6577
		public readonly ImmutableDictionary<Identifier, ImmutableHashSet<Identifier>> VarTags;

		// Token: 0x040019B2 RID: 6578
		public readonly Identifier MenuCategoryVar;

		// Token: 0x040019B3 RID: 6579
		public readonly Identifier Pronouns;
	}
}
