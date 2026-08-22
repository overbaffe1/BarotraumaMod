using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002A5 RID: 677
	public class TagLString : LocalizedString
	{
		// Token: 0x17000DA8 RID: 3496
		// (get) Token: 0x06002ED7 RID: 11991 RVA: 0x00138E70 File Offset: 0x00137070
		// (set) Token: 0x06002ED8 RID: 11992 RVA: 0x00138E78 File Offset: 0x00137078
		public bool UsingDefaultLanguageAsFallback { get; private set; }

		// Token: 0x06002ED9 RID: 11993 RVA: 0x00138E81 File Offset: 0x00137081
		[NullableContext(1)]
		public TagLString(params Identifier[] tags)
		{
			this.tags = tags.ToImmutableArray<Identifier>();
		}

		// Token: 0x17000DA9 RID: 3497
		// (get) Token: 0x06002EDA RID: 11994 RVA: 0x00138E9A File Offset: 0x0013709A
		public override bool Loaded
		{
			get
			{
				if (this.loadedSuccessfully == LocalizedString.LoadedSuccessfully.Unknown)
				{
					this.RetrieveValue();
				}
				return this.loadedSuccessfully == LocalizedString.LoadedSuccessfully.Yes;
			}
		}

		// Token: 0x06002EDB RID: 11995 RVA: 0x00138EB4 File Offset: 0x001370B4
		public override void RetrieveValue()
		{
			base.UpdateLanguage();
			this.UsingDefaultLanguageAsFallback = false;
			ValueTuple<string, bool> valueTuple = this.<RetrieveValue>g__tryLoad|9_0(base.Language);
			string value = valueTuple.Item1;
			bool loaded = valueTuple.Item2;
			this.loadedSuccessfully = (loaded ? LocalizedString.LoadedSuccessfully.Yes : LocalizedString.LoadedSuccessfully.No);
			this.cachedValue = value;
			if (!loaded && base.Language != TextManager.DefaultLanguage)
			{
				ValueTuple<string, bool> valueTuple2 = this.<RetrieveValue>g__tryLoad|9_0(TextManager.DefaultLanguage);
				value = valueTuple2.Item1;
				bool fallbackLoaded = valueTuple2.Item2;
				this.cachedValue = value;
				this.UsingDefaultLanguageAsFallback = fallbackLoaded;
			}
		}

		// Token: 0x06002EDC RID: 11996 RVA: 0x00138F38 File Offset: 0x00137138
		[CompilerGenerated]
		[return: TupleElementNames(new string[]
		{
			"value",
			"loaded"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private ValueTuple<string, bool> <RetrieveValue>g__tryLoad|9_0(LanguageIdentifier lang)
		{
			IReadOnlyList<TextPack.Text> candidates = Array.Empty<TextPack.Text>();
			int tagIndex = 0;
			ImmutableList<TextPack> packs;
			if (TextManager.TextPacks.TryGetValue(lang, out packs))
			{
				while (candidates.Count == 0 && tagIndex < this.tags.Length)
				{
					foreach (TextPack pack in packs)
					{
						ImmutableArray<TextPack.Text> texts;
						if (pack.Texts.TryGetValue(this.tags[tagIndex], out texts))
						{
							candidates = candidates.ListConcat(texts);
						}
					}
					tagIndex++;
				}
			}
			if (candidates.Count == 0)
			{
				return new ValueTuple<string, bool>(string.Empty, false);
			}
			TextPack.Text firstOverride = candidates.FirstOrDefault((TextPack.Text c) => c.IsOverride);
			if (firstOverride != default(TextPack.Text))
			{
				return new ValueTuple<string, bool>((from c in candidates
				where c.IsOverride
				where c.TextPack == firstOverride.TextPack
				select c).GetRandomUnsynced<TextPack.Text>().String, true);
			}
			return new ValueTuple<string, bool>(candidates.GetRandomUnsynced<TextPack.Text>().String, true);
		}

		// Token: 0x0400177E RID: 6014
		private readonly ImmutableArray<Identifier> tags;

		// Token: 0x04001780 RID: 6016
		private LocalizedString.LoadedSuccessfully loadedSuccessfully;
	}
}
