using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000472 RID: 1138
	public interface IAssemblyResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
		// Token: 0x17001021 RID: 4129
		// (get) Token: 0x06003D7C RID: 15740
		[XmlAttribute("FriendlyName")]
		string FriendlyName { get; }

		// Token: 0x17001022 RID: 4130
		// (get) Token: 0x06003D7D RID: 15741
		[XmlAttribute("IsScript")]
		bool IsScript { get; }

		// Token: 0x17001023 RID: 4131
		// (get) Token: 0x06003D7E RID: 15742
		[XmlAttribute("UseInternalAccessName")]
		bool UseInternalAccessName { get; }

		// Token: 0x17001024 RID: 4132
		// (get) Token: 0x06003D7F RID: 15743
		[XmlAttribute("IsReferenceModeOnly")]
		bool IsReferenceModeOnly { get; }
	}
}
