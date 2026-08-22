using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002CE RID: 718
	internal readonly struct CampaignSettingDefinitions
	{
		// Token: 0x06003CB9 RID: 15545 RVA: 0x0022BF60 File Offset: 0x0022A160
		public CampaignSettingDefinitions(XElement element)
		{
			this.Attributes = element.Attributes().ToImmutableDictionary((XAttribute a) => a.NameAsIdentifier(), (XAttribute a) => a);
		}

		// Token: 0x04001F55 RID: 8021
		public readonly ImmutableDictionary<Identifier, XAttribute> Attributes;
	}
}
