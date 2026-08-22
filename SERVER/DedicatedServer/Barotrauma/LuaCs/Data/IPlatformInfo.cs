using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000468 RID: 1128
	public interface IPlatformInfo
	{
		// Token: 0x1700100D RID: 4109
		// (get) Token: 0x06003D56 RID: 15702
		[Required]
		[XmlAttribute("Platform")]
		Platform SupportedPlatforms { get; }

		// Token: 0x1700100E RID: 4110
		// (get) Token: 0x06003D57 RID: 15703
		[Required]
		[XmlAttribute("Target")]
		Target SupportedTargets { get; }
	}
}
