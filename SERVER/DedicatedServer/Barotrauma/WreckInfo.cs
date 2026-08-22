using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000252 RID: 594
	internal class WreckInfo : ExtraSubmarineInfo
	{
		// Token: 0x17000CA8 RID: 3240
		// (get) Token: 0x06002A9C RID: 10908 RVA: 0x00115A92 File Offset: 0x00113C92
		// (set) Token: 0x06002A9D RID: 10909 RVA: 0x00115A9A File Offset: 0x00113C9A
		[Serialize(WreckInfo.HasThalamus.Unknown, IsPropertySaveable.Yes, "", "", false)]
		public WreckInfo.HasThalamus WreckContainsThalamus { get; private set; }

		// Token: 0x06002A9E RID: 10910 RVA: 0x00115AA3 File Offset: 0x00113CA3
		public WreckInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "WreckInfo (" + submarineInfo.Name + ")";
			this.TryDetermineThalamusIfUnknown(element);
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x00115ACF File Offset: 0x00113CCF
		public WreckInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "WreckInfo (" + submarineInfo.Name + ")";
			this.TryDetermineThalamusIfUnknown(submarineInfo.SubmarineElement);
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x00115AFF File Offset: 0x00113CFF
		public WreckInfo(WreckInfo original) : base(original)
		{
		}

		// Token: 0x06002AA1 RID: 10913 RVA: 0x00115B08 File Offset: 0x00113D08
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

		// Token: 0x02000A8B RID: 2699
		public enum HasThalamus
		{
			// Token: 0x0400366A RID: 13930
			Unknown,
			// Token: 0x0400366B RID: 13931
			Yes,
			// Token: 0x0400366C RID: 13932
			No
		}
	}
}
