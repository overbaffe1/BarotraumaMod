using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058E RID: 1422
	public interface ILuaScriptResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
		// Token: 0x17001584 RID: 5508
		// (get) Token: 0x060056E9 RID: 22249
		[XmlAttribute("IsAutorun")]
		bool IsAutorun { get; }

		// Token: 0x17001585 RID: 5509
		// (get) Token: 0x060056EA RID: 22250
		[XmlAttribute("RunUnrestricted")]
		bool RunUnrestricted { get; }
	}
}
