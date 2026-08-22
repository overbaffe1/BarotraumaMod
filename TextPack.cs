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
	// Token: 0x0200037A RID: 890
	public class TextPack
	{
		// Token: 0x170011B8 RID: 4536
		// (get) Token: 0x060043D8 RID: 17368 RVA: 0x00254B24 File Offset: 0x00252D24
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

		// Token: 0x060043D9 RID: 17369 RVA: 0x00254B8C File Offset: 0x00252D8C
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

		// Token: 0x060043DA RID: 17370 RVA: 0x00254C60 File Offset: 0x00252E60
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

		// Token: 0x060043DB RID: 17371 RVA: 0x00254CFB File Offset: 0x00252EFB
		public void Unload()
		{
			this.texts = null;
		}

		// Token: 0x060043DC RID: 17372 RVA: 0x00254D04 File Offset: 0x00252F04
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

		// Token: 0x04002383 RID: 9091
		public readonly TextFile ContentFile;

		// Token: 0x04002384 RID: 9092
		public readonly LanguageIdentifier Language;

		// Token: 0x04002385 RID: 9093
		private ImmutableDictionary<Identifier, ImmutableArray<TextPack.Text>> texts;

		// Token: 0x04002386 RID: 9094
		public readonly string TranslatedName;

		// Token: 0x04002387 RID: 9095
		public readonly bool NoWhitespace;

		// Token: 0x02001097 RID: 4247
		public readonly struct Text : IEquatable<TextPack.Text>
		{
			// Token: 0x06008D37 RID: 36151 RVA: 0x003B1667 File Offset: 0x003AF867
			public Text(string String, bool IsOverride, TextPack TextPack)
			{
				this.String = String;
				this.IsOverride = IsOverride;
				this.TextPack = TextPack;
			}

			// Token: 0x17001C7A RID: 7290
			// (get) Token: 0x06008D38 RID: 36152 RVA: 0x003B167E File Offset: 0x003AF87E
			// (set) Token: 0x06008D39 RID: 36153 RVA: 0x003B1686 File Offset: 0x003AF886
			public string String { get; set; }

			// Token: 0x17001C7B RID: 7291
			// (get) Token: 0x06008D3A RID: 36154 RVA: 0x003B168F File Offset: 0x003AF88F
			// (set) Token: 0x06008D3B RID: 36155 RVA: 0x003B1697 File Offset: 0x003AF897
			public bool IsOverride { get; set; }

			// Token: 0x17001C7C RID: 7292
			// (get) Token: 0x06008D3C RID: 36156 RVA: 0x003B16A0 File Offset: 0x003AF8A0
			// (set) Token: 0x06008D3D RID: 36157 RVA: 0x003B16A8 File Offset: 0x003AF8A8
			public TextPack TextPack { get; set; }

			// Token: 0x06008D3E RID: 36158 RVA: 0x003B16B4 File Offset: 0x003AF8B4
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

			// Token: 0x06008D3F RID: 36159 RVA: 0x003B1700 File Offset: 0x003AF900
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

			// Token: 0x06008D40 RID: 36160 RVA: 0x003B1767 File Offset: 0x003AF967
			[CompilerGenerated]
			public static bool operator !=(TextPack.Text left, TextPack.Text right)
			{
				return !(left == right);
			}

			// Token: 0x06008D41 RID: 36161 RVA: 0x003B1773 File Offset: 0x003AF973
			[CompilerGenerated]
			public static bool operator ==(TextPack.Text left, TextPack.Text right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008D42 RID: 36162 RVA: 0x003B177D File Offset: 0x003AF97D
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<string>.Default.GetHashCode(this.<String>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsOverride>k__BackingField)) * -1521134295 + EqualityComparer<TextPack>.Default.GetHashCode(this.<TextPack>k__BackingField);
			}

			// Token: 0x06008D43 RID: 36163 RVA: 0x003B17BD File Offset: 0x003AF9BD
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is TextPack.Text && this.Equals((TextPack.Text)obj);
			}

			// Token: 0x06008D44 RID: 36164 RVA: 0x003B17D8 File Offset: 0x003AF9D8
			[CompilerGenerated]
			public bool Equals(TextPack.Text other)
			{
				return EqualityComparer<string>.Default.Equals(this.<String>k__BackingField, other.<String>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsOverride>k__BackingField, other.<IsOverride>k__BackingField) && EqualityComparer<TextPack>.Default.Equals(this.<TextPack>k__BackingField, other.<TextPack>k__BackingField);
			}

			// Token: 0x06008D45 RID: 36165 RVA: 0x003B182D File Offset: 0x003AFA2D
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
