using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000587 RID: 1415
	public interface IPlatformInfo
	{
		// Token: 0x17001577 RID: 5495
		// (get) Token: 0x060056CA RID: 22218
		[Required]
		[XmlAttribute("Platform")]
		Platform SupportedPlatforms { get; }

		// Token: 0x17001578 RID: 5496
		// (get) Token: 0x060056CB RID: 22219
		[Required]
		[XmlAttribute("Target")]
		Target SupportedTargets { get; }
	}
}
