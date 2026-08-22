using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002AE RID: 686
	public class TextPack
	{
		// Token: 0x17000DB5 RID: 3509
		// (get) Token: 0x06002F40 RID: 12096 RVA: 0x0013A5E0 File Offset: 0x001387E0
		public ImmutableDictionary<Identifier, ImmutableArray<TextPack.Text>> Texts
		{
			get
			{
				if (this.texts == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Accessed texts in an unloaded text package (");
					defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(this.Language);
					defaultInterpolatedStringHandler.AppendLiteral("). Loading the text pack...");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					this.VerifyLoaded();
				}
				return this.texts;
			}
		}

		// Token: 0x06002F41 RID: 12097 RVA: 0x0013A648 File Offset: 0x00138848
		public TextPack(TextFile file, ContentXElement mainElement, LanguageIdentifier language, bool load = false)
		{
			this.ContentFile = file;
			Identifier languageName = mainElement.GetAttributeIdentifier("language", Identifier.Empty);
			if (languageName.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Language not defined in text file \"");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(file.Path);
				defaultInterpolatedStringHandler.AppendLiteral("\". Setting the language as ");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(TextManager.DefaultLanguage);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), mainElement.ContentPackage);
				languageName = TextManager.DefaultLanguage.Value;
			}
			this.Language = language;
			this.TranslatedName = mainElement.GetAttributeString("translatedname", languageName.Value);
			this.NoWhitespace = mainElement.GetAttributeBool("nowhitespace", false);
			if (load)
			{
				this.VerifyLoaded();
			}
		}

		// Token: 0x06002F42 RID: 12098 RVA: 0x0013A71C File Offset: 0x0013891C
		public void VerifyLoaded()
		{
			TextPack.<>c__DisplayClass9_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.texts != null)
			{
				return;
			}
			XDocument doc = XMLExtensions.TryLoadXml(this.ContentFile.Path);
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentFile.ContentPackage);
			CS$<>8__locals1.texts = new Dictionary<Identifier, List<TextPack.Text>>();
			this.<VerifyLoaded>g__LoadElements|9_0(mainElement, mainElement.IsOverride(), ref CS$<>8__locals1);
			this.texts = (from kvp in CS$<>8__locals1.texts
			select new ValueTuple<Identifier, ImmutableArray<TextPack.Text>>(kvp.Key, kvp.Value.ToImmutableArray<TextPack.Text>())).ToImmutableDictionary<Identifier, ImmutableArray<TextPack.Text>>();
		}

		// Token: 0x06002F43 RID: 12099 RVA: 0x0013A7B7 File Offset: 0x001389B7
		public void Unload()
		{
			this.texts = null;
		}

		// Token: 0x06002F44 RID: 12100 RVA: 0x0013A7C0 File Offset: 0x001389C0
		[CompilerGenerated]
		private void <VerifyLoaded>g__LoadElements|9_0(XElement parentElement, bool isOverride, ref TextPack.<>c__DisplayClass9_0 A_3)
		{
			foreach (XElement element in parentElement.Elements())
			{
				Identifier elemName = element.NameAsIdentifier();
				if (element.IsOverride())
				{
					this.<VerifyLoaded>g__LoadElements|9_0(element, true, ref A_3);
				}
				else
				{
					if (!A_3.texts.ContainsKey(elemName))
					{
						A_3.texts.Add(elemName, new List<TextPack.Text>());
					}
					string str = element.ElementInnerText().Replace("\\n", "\n").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'");
					A_3.texts[elemName].Add(new TextPack.Text(str, isOverride, this));
				}
			}
		}

		// Token: 0x040017A0 RID: 6048
		public readonly TextFile ContentFile;

		// Token: 0x040017A1 RID: 6049
		public readonly LanguageIdentifier Language;

		// Token: 0x040017A2 RID: 6050
		private ImmutableDictionary<Identifier, ImmutableArray<TextPack.Text>> texts;

		// Token: 0x040017A3 RID: 6051
		public readonly string TranslatedName;

		// Token: 0x040017A4 RID: 6052
		public readonly bool NoWhitespace;

		// Token: 0x02000B3B RID: 2875
		public readonly struct Text : IEquatable<TextPack.Text>
		{
			// Token: 0x0600602F RID: 24623 RVA: 0x00209897 File Offset: 0x00207A97
			public Text(string String, bool IsOverride, TextPack TextPack)
			{
				this.String = String;
				this.IsOverride = IsOverride;
				this.TextPack = TextPack;
			}

			// Token: 0x170015E0 RID: 5600
			// (get) Token: 0x06006030 RID: 24624 RVA: 0x002098AE File Offset: 0x00207AAE
			// (set) Token: 0x06006031 RID: 24625 RVA: 0x002098B6 File Offset: 0x00207AB6
			public string String { get; set; }

			// Token: 0x170015E1 RID: 5601
			// (get) Token: 0x06006032 RID: 24626 RVA: 0x002098BF File Offset: 0x00207ABF
			// (set) Token: 0x06006033 RID: 24627 RVA: 0x002098C7 File Offset: 0x00207AC7
			public bool IsOverride { get; set; }

			// Token: 0x170015E2 RID: 5602
			// (get) Token: 0x06006034 RID: 24628 RVA: 0x002098D0 File Offset: 0x00207AD0
			// (set) Token: 0x06006035 RID: 24629 RVA: 0x002098D8 File Offset: 0x00207AD8
			public TextPack TextPack { get; set; }

			// Token: 0x06006036 RID: 24630 RVA: 0x002098E4 File Offset: 0x00207AE4
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Text");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006037 RID: 24631 RVA: 0x00209930 File Offset: 0x00207B30
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("String = ");
				builder.Append(this.String);
				builder.Append(", IsOverride = ");
				builder.Append(this.IsOverride.ToString());
				builder.Append(", TextPack = ");
				builder.Append(this.TextPack);
				return true;
			}

			// Token: 0x06006038 RID: 24632 RVA: 0x00209997 File Offset: 0x00207B97
			[CompilerGenerated]
			public static bool operator !=(TextPack.Text left, TextPack.Text right)
			{
				return !(left == right);
			}

			// Token: 0x06006039 RID: 24633 RVA: 0x002099A3 File Offset: 0x00207BA3
			[CompilerGenerated]
			public static bool operator ==(TextPack.Text left, TextPack.Text right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600603A RID: 24634 RVA: 0x002099AD File Offset: 0x00207BAD
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<string>.Default.GetHashCode(this.<String>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsOverride>k__BackingField)) * -1521134295 + EqualityComparer<TextPack>.Default.GetHashCode(this.<TextPack>k__BackingField);
			}

			// Token: 0x0600603B RID: 24635 RVA: 0x002099ED File Offset: 0x00207BED
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is TextPack.Text && this.Equals((TextPack.Text)obj);
			}

			// Token: 0x0600603C RID: 24636 RVA: 0x00209A08 File Offset: 0x00207C08
			[CompilerGenerated]
			public bool Equals(TextPack.Text other)
			{
				return EqualityComparer<string>.Default.Equals(this.<String>k__BackingField, other.<String>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsOverride>k__BackingField, other.<IsOverride>k__BackingField) && EqualityComparer<TextPack>.Default.Equals(this.<TextPack>k__BackingField, other.<TextPack>k__BackingField);
			}

			// Token: 0x0600603D RID: 24637 RVA: 0x00209A5D File Offset: 0x00207C5D
			[CompilerGenerated]
			public void Deconstruct(out string String, out bool IsOverride, out TextPack TextPack)
			{
				String = this.String;
				IsOverride = this.IsOverride;
				TextPack = this.TextPack;
			}
		}
	}
}
