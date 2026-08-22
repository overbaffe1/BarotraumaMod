using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001E1 RID: 481
	internal readonly struct CampaignSettingDefinitions
	{
		// Token: 0x060022B7 RID: 8887 RVA: 0x000E8F00 File Offset: 0x000E7100
		public CampaignSettingDefinitions(XElement element)
		{
			this.Attributes = element.Attributes().ToImmutableDictionary((XAttribute a) => a.NameAsIdentifier(), (XAttribute a) => a);
		}

		// Token: 0x040010B9 RID: 4281
		public readonly ImmutableDictionary<Identifier, XAttribute> Attributes;
	}
}
