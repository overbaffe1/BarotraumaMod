using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000588 RID: 1416
	public interface IResourceInfo : IPlatformInfo
	{
		// Token: 0x17001579 RID: 5497
		// (get) Token: 0x060056CC RID: 22220
		[XmlAttribute("LoadPriority")]
		int LoadPriority { get; }

		// Token: 0x1700157A RID: 5498
		// (get) Token: 0x060056CD RID: 22221
		[Required]
		ImmutableArray<ContentPath> FilePaths { get; }

		// Token: 0x1700157B RID: 5499
		// (get) Token: 0x060056CE RID: 22222
		[XmlAttribute("Optional")]
		bool Optional { get; }
	}
}
