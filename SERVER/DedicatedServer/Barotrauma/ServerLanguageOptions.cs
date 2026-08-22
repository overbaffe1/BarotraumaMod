using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200028C RID: 652
	internal static class ServerLanguageOptions
	{
		// Token: 0x06002DD7 RID: 11735 RVA: 0x0012FDA8 File Offset: 0x0012DFA8
		static ServerLanguageOptions()
		{
			XDocument xdocument = XMLExtensions.TryLoadXml("Data/languageoptions.xml");
			IEnumerable<XElement> enumerable;
			if (xdocument == null)
			{
				enumerable = null;
			}
			else
			{
				XElement root = xdocument.Root;
				enumerable = ((root != null) ? root.Elements() : null);
			}
			IEnumerable<XElement> enumerable2;
			if ((enumerable2 = enumerable) == null)
			{
				IEnumerable<XElement> enumerable3 = Enumerable.Empty<XElement>();
				enumerable2 = enumerable3;
			}
			IEnumerable<XElement> languageOptionElements = enumerable2;
			ServerLanguageOptions.Options = (from p in languageOptionElements.Select(new Func<XElement, ServerLanguageOptions.LanguageOption>(ServerLanguageOptions.LanguageOption.FromXElement)).DistinctBy((ServerLanguageOptions.LanguageOption p) => p.Identifier)
			where !p.Label.IsNullOrWhiteSpace() && p.Identifier != LanguageIdentifier.None
			orderby p.Label
			select p).ToImmutableArray<ServerLanguageOptions.LanguageOption>();
		}

		// Token: 0x06002DD8 RID: 11736 RVA: 0x0012FE40 File Offset: 0x0012E040
		public static LanguageIdentifier PickLanguage(LanguageIdentifier id)
		{
			if (id == LanguageIdentifier.None)
			{
				id = GameSettings.CurrentConfig.Language;
			}
			foreach (ServerLanguageOptions.LanguageOption languageOption in ServerLanguageOptions.Options)
			{
				string text;
				LanguageIdentifier languageIdentifier;
				ImmutableArray<LanguageIdentifier> immutableArray;
				languageOption.Deconstruct(out text, out languageIdentifier, out immutableArray);
				LanguageIdentifier identifier = languageIdentifier;
				ImmutableArray<LanguageIdentifier> mapsFrom = immutableArray;
				if (id == identifier || mapsFrom.Contains(id))
				{
					return identifier;
				}
			}
			return TextManager.DefaultLanguage;
		}

		// Token: 0x0400166E RID: 5742
		public static readonly ImmutableArray<ServerLanguageOptions.LanguageOption> Options;

		// Token: 0x02000AFF RID: 2815
		public readonly struct LanguageOption : IEquatable<ServerLanguageOptions.LanguageOption>
		{
			// Token: 0x06005F2F RID: 24367 RVA: 0x00206B3E File Offset: 0x00204D3E
			public LanguageOption(string Label, LanguageIdentifier Identifier, ImmutableArray<LanguageIdentifier> MapsFrom)
			{
				this.Label = Label;
				this.Identifier = Identifier;
				this.MapsFrom = MapsFrom;
			}

			// Token: 0x170015B1 RID: 5553
			// (get) Token: 0x06005F30 RID: 24368 RVA: 0x00206B55 File Offset: 0x00204D55
			// (set) Token: 0x06005F31 RID: 24369 RVA: 0x00206B5D File Offset: 0x00204D5D
			public string Label { get; set; }

			// Token: 0x170015B2 RID: 5554
			// (get) Token: 0x06005F32 RID: 24370 RVA: 0x00206B66 File Offset: 0x00204D66
			// (set) Token: 0x06005F33 RID: 24371 RVA: 0x00206B6E File Offset: 0x00204D6E
			public LanguageIdentifier Identifier { get; set; }

			// Token: 0x170015B3 RID: 5555
			// (get) Token: 0x06005F34 RID: 24372 RVA: 0x00206B77 File Offset: 0x00204D77
			// (set) Token: 0x06005F35 RID: 24373 RVA: 0x00206B7F File Offset: 0x00204D7F
			public ImmutableArray<LanguageIdentifier> MapsFrom { get; set; }

			// Token: 0x06005F36 RID: 24374 RVA: 0x00206B88 File Offset: 0x00204D88
			public static ServerLanguageOptions.LanguageOption FromXElement(XElement element)
			{
				return new ServerLanguageOptions.LanguageOption(element.GetAttributeString("label", ""), element.GetAttributeIdentifier("identifier", LanguageIdentifier.None.Value).ToLanguageIdentifier(), (from id in element.GetAttributeIdentifierArray("mapsFrom", Array.Empty<Identifier>(), true)
				select id.ToLanguageIdentifier()).ToImmutableArray<LanguageIdentifier>());
			}

			// Token: 0x06005F37 RID: 24375 RVA: 0x00206C00 File Offset: 0x00204E00
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LanguageOption");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06005F38 RID: 24376 RVA: 0x00206C4C File Offset: 0x00204E4C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Label = ");
				builder.Append(this.Label);
				builder.Append(", Identifier = ");
				builder.Append(this.Identifier.ToString());
				builder.Append(", MapsFrom = ");
				builder.Append(this.MapsFrom.ToString());
				return true;
			}

			// Token: 0x06005F39 RID: 24377 RVA: 0x00206CC1 File Offset: 0x00204EC1
			[CompilerGenerated]
			public static bool operator !=(ServerLanguageOptions.LanguageOption left, ServerLanguageOptions.LanguageOption right)
			{
				return !(left == right);
			}

			// Token: 0x06005F3A RID: 24378 RVA: 0x00206CCD File Offset: 0x00204ECD
			[CompilerGenerated]
			public static bool operator ==(ServerLanguageOptions.LanguageOption left, ServerLanguageOptions.LanguageOption right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005F3B RID: 24379 RVA: 0x00206CD7 File Offset: 0x00204ED7
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<string>.Default.GetHashCode(this.<Label>k__BackingField) * -1521134295 + EqualityComparer<LanguageIdentifier>.Default.GetHashCode(this.<Identifier>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<LanguageIdentifier>>.Default.GetHashCode(this.<MapsFrom>k__BackingField);
			}

			// Token: 0x06005F3C RID: 24380 RVA: 0x00206D17 File Offset: 0x00204F17
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ServerLanguageOptions.LanguageOption && this.Equals((ServerLanguageOptions.LanguageOption)obj);
			}

			// Token: 0x06005F3D RID: 24381 RVA: 0x00206D30 File Offset: 0x00204F30
			[CompilerGenerated]
			public bool Equals(ServerLanguageOptions.LanguageOption other)
			{
				return EqualityComparer<string>.Default.Equals(this.<Label>k__BackingField, other.<Label>k__BackingField) && EqualityComparer<LanguageIdentifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<ImmutableArray<LanguageIdentifier>>.Default.Equals(this.<MapsFrom>k__BackingField, other.<MapsFrom>k__BackingField);
			}

			// Token: 0x06005F3E RID: 24382 RVA: 0x00206D85 File Offset: 0x00204F85
			[CompilerGenerated]
			public void Deconstruct(out string Label, out LanguageIdentifier Identifier, out ImmutableArray<LanguageIdentifier> MapsFrom)
			{
				Label = this.Label;
				Identifier = this.Identifier;
				MapsFrom = this.MapsFrom;
			}
		}
	}
}
