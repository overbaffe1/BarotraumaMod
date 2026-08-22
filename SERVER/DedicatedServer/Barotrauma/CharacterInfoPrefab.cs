using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020000BC RID: 188
	internal class CharacterInfoPrefab
	{
		// Token: 0x0600159D RID: 5533 RVA: 0x000B9474 File Offset: 0x000B7674
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

		// Token: 0x0600159E RID: 5534 RVA: 0x000B95D1 File Offset: 0x000B77D1
		public string ReplaceVars(string str, CharacterInfo.HeadPreset headPreset)
		{
			return this.ReplaceVars(str, headPreset.TagSet);
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x000B95E0 File Offset: 0x000B77E0
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

		// Token: 0x04000A45 RID: 2629
		public readonly ImmutableArray<CharacterInfo.HeadPreset> Heads;

		// Token: 0x04000A46 RID: 2630
		public readonly ImmutableDictionary<Identifier, ImmutableHashSet<Identifier>> VarTags;

		// Token: 0x04000A47 RID: 2631
		public readonly Identifier MenuCategoryVar;

		// Token: 0x04000A48 RID: 2632
		public readonly Identifier Pronouns;
	}
}
