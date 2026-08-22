using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000469 RID: 1129
	public interface IResourceInfo : IPlatformInfo
	{
		// Token: 0x1700100F RID: 4111
		// (get) Token: 0x06003D58 RID: 15704
		[XmlAttribute("LoadPriority")]
		int LoadPriority { get; }

		// Token: 0x17001010 RID: 4112
		// (get) Token: 0x06003D59 RID: 15705
		[Required]
		ImmutableArray<ContentPath> FilePaths { get; }

		// Token: 0x17001011 RID: 4113
		// (get) Token: 0x06003D5A RID: 15706
		[XmlAttribute("Optional")]
		bool Optional { get; }
	}
}
