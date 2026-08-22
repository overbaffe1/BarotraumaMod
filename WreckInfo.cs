using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000329 RID: 809
	internal class WreckInfo : ExtraSubmarineInfo
	{
		// Token: 0x1700112B RID: 4395
		// (get) Token: 0x0600409E RID: 16542 RVA: 0x0023DDEA File Offset: 0x0023BFEA
		// (set) Token: 0x0600409F RID: 16543 RVA: 0x0023DDF2 File Offset: 0x0023BFF2
		[Serialize(WreckInfo.HasThalamus.Unknown, IsPropertySaveable.Yes, "", "", false)]
		public WreckInfo.HasThalamus WreckContainsThalamus { get; private set; }

		// Token: 0x060040A0 RID: 16544 RVA: 0x0023DDFB File Offset: 0x0023BFFB
		public WreckInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "WreckInfo (" + submarineInfo.Name + ")";
			this.TryDetermineThalamusIfUnknown(element);
		}

		// Token: 0x060040A1 RID: 16545 RVA: 0x0023DE27 File Offset: 0x0023C027
		public WreckInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "WreckInfo (" + submarineInfo.Name + ")";
			this.TryDetermineThalamusIfUnknown(submarineInfo.SubmarineElement);
		}

		// Token: 0x060040A2 RID: 16546 RVA: 0x0023DE57 File Offset: 0x0023C057
		public WreckInfo(WreckInfo original) : base(original)
		{
		}

		// Token: 0x060040A3 RID: 16547 RVA: 0x0023DE60 File Offset: 0x0023C060
		private void TryDetermineThalamusIfUnknown(XElement element)
		{
			if (this.WreckContainsThalamus != WreckInfo.HasThalamus.Unknown)
			{
				return;
			}
			if (element == null)
			{
				this.WreckContainsThalamus = WreckInfo.HasThalamus.Unknown;
				return;
			}
			foreach (XElement subElement in element.Elements())
			{
				if (string.Equals(subElement.Name.ToString(), "Item", StringComparison.InvariantCultureIgnoreCase))
				{
					ImmutableHashSet<Identifier> tags = subElement.GetAttributeIdentifierImmutableHashSet("Tags", ImmutableHashSet<Identifier>.Empty, true);
					if (tags.Contains(Tags.Thalamus))
					{
						this.WreckContainsThalamus = WreckInfo.HasThalamus.Yes;
						return;
					}
				}
			}
			this.WreckContainsThalamus = WreckInfo.HasThalamus.No;
		}

		// Token: 0x02001022 RID: 4130
		public enum HasThalamus
		{
			// Token: 0x04005775 RID: 22389
			Unknown,
			// Token: 0x04005776 RID: 22390
			Yes,
			// Token: 0x04005777 RID: 22391
			No
		}
	}
}
