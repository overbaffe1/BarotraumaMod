using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200035D RID: 861
	internal static class ServerLanguageOptions
	{
		// Token: 0x060042EB RID: 17131 RVA: 0x0025049C File Offset: 0x0024E69C
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

		// Token: 0x060042EC RID: 17132 RVA: 0x00250534 File Offset: 0x0024E734
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

		// Token: 0x040022B3 RID: 8883
		public static readonly ImmutableArray<ServerLanguageOptions.LanguageOption> Options;

		// Token: 0x02001079 RID: 4217
		public readonly struct LanguageOption : IEquatable<ServerLanguageOptions.LanguageOption>
		{
			// Token: 0x06008CC4 RID: 36036 RVA: 0x003B047E File Offset: 0x003AE67E
			public LanguageOption(string Label, LanguageIdentifier Identifier, ImmutableArray<LanguageIdentifier> MapsFrom)
			{
				this.Label = Label;
				this.Identifier = Identifier;
				this.MapsFrom = MapsFrom;
			}

			// Token: 0x17001C6E RID: 7278
			// (get) Token: 0x06008CC5 RID: 36037 RVA: 0x003B0495 File Offset: 0x003AE695
			// (set) Token: 0x06008CC6 RID: 36038 RVA: 0x003B049D File Offset: 0x003AE69D
			public string Label { get; set; }

			// Token: 0x17001C6F RID: 7279
			// (get) Token: 0x06008CC7 RID: 36039 RVA: 0x003B04A6 File Offset: 0x003AE6A6
			// (set) Token: 0x06008CC8 RID: 36040 RVA: 0x003B04AE File Offset: 0x003AE6AE
			public LanguageIdentifier Identifier { get; set; }

			// Token: 0x17001C70 RID: 7280
			// (get) Token: 0x06008CC9 RID: 36041 RVA: 0x003B04B7 File Offset: 0x003AE6B7
			// (set) Token: 0x06008CCA RID: 36042 RVA: 0x003B04BF File Offset: 0x003AE6BF
			public ImmutableArray<LanguageIdentifier> MapsFrom { get; set; }

			// Token: 0x06008CCB RID: 36043 RVA: 0x003B04C8 File Offset: 0x003AE6C8
			public static ServerLanguageOptions.LanguageOption FromXElement(XElement element)
			{
				return new ServerLanguageOptions.LanguageOption(element.GetAttributeString("label", ""), element.GetAttributeIdentifier("identifier", LanguageIdentifier.None.Value).ToLanguageIdentifier(), (from id in element.GetAttributeIdentifierArray("mapsFrom", Array.Empty<Identifier>(), true)
				select id.ToLanguageIdentifier()).ToImmutableArray<LanguageIdentifier>());
			}

			// Token: 0x06008CCC RID: 36044 RVA: 0x003B0540 File Offset: 0x003AE740
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

			// Token: 0x06008CCD RID: 36045 RVA: 0x003B058C File Offset: 0x003AE78C
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

			// Token: 0x06008CCE RID: 36046 RVA: 0x003B0601 File Offset: 0x003AE801
			[CompilerGenerated]
			public static bool operator !=(ServerLanguageOptions.LanguageOption left, ServerLanguageOptions.LanguageOption right)
			{
				return !(left == right);
			}

			// Token: 0x06008CCF RID: 36047 RVA: 0x003B060D File Offset: 0x003AE80D
			[CompilerGenerated]
			public static bool operator ==(ServerLanguageOptions.LanguageOption left, ServerLanguageOptions.LanguageOption right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008CD0 RID: 36048 RVA: 0x003B0617 File Offset: 0x003AE817
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<string>.Default.GetHashCode(this.<Label>k__BackingField) * -1521134295 + EqualityComparer<LanguageIdentifier>.Default.GetHashCode(this.<Identifier>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<LanguageIdentifier>>.Default.GetHashCode(this.<MapsFrom>k__BackingField);
			}

			// Token: 0x06008CD1 RID: 36049 RVA: 0x003B0657 File Offset: 0x003AE857
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ServerLanguageOptions.LanguageOption && this.Equals((ServerLanguageOptions.LanguageOption)obj);
			}

			// Token: 0x06008CD2 RID: 36050 RVA: 0x003B0670 File Offset: 0x003AE870
			[CompilerGenerated]
			public bool Equals(ServerLanguageOptions.LanguageOption other)
			{
				return EqualityComparer<string>.Default.Equals(this.<Label>k__BackingField, other.<Label>k__BackingField) && EqualityComparer<LanguageIdentifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<ImmutableArray<LanguageIdentifier>>.Default.Equals(this.<MapsFrom>k__BackingField, other.<MapsFrom>k__BackingField);
			}

			// Token: 0x06008CD3 RID: 36051 RVA: 0x003B06C5 File Offset: 0x003AE8C5
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
