using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000371 RID: 881
	public class TagLString : LocalizedString
	{
		// Token: 0x170011AA RID: 4522
		// (get) Token: 0x0600436C RID: 17260 RVA: 0x002532B0 File Offset: 0x002514B0
		// (set) Token: 0x0600436D RID: 17261 RVA: 0x002532B8 File Offset: 0x002514B8
		public bool UsingDefaultLanguageAsFallback { get; private set; }

		// Token: 0x0600436E RID: 17262 RVA: 0x002532C1 File Offset: 0x002514C1
		[NullableContext(1)]
		public TagLString(params Identifier[] tags)
		{
			this.tags = tags.ToImmutableArray<Identifier>();
		}

		// Token: 0x170011AB RID: 4523
		// (get) Token: 0x0600436F RID: 17263 RVA: 0x002532DA File Offset: 0x002514DA
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

		// Token: 0x06004370 RID: 17264 RVA: 0x002532F4 File Offset: 0x002514F4
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

		// Token: 0x06004371 RID: 17265 RVA: 0x00253378 File Offset: 0x00251578
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

		// Token: 0x0400235E RID: 9054
		private readonly ImmutableArray<Identifier> tags;

		// Token: 0x04002360 RID: 9056
		private LocalizedString.LoadedSuccessfully loadedSuccessfully;
	}
}
