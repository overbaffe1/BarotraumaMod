using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000471 RID: 1137
	public interface ILuaScriptResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
		// Token: 0x1700101F RID: 4127
		// (get) Token: 0x06003D7A RID: 15738
		[XmlAttribute("IsAutorun")]
		bool IsAutorun { get; }

		// Token: 0x17001020 RID: 4128
		// (get) Token: 0x06003D7B RID: 15739
		[XmlAttribute("RunUnrestricted")]
		bool RunUnrestricted { get; }
	}
}
