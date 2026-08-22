using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058F RID: 1423
	public interface IAssemblyResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
		// Token: 0x17001586 RID: 5510
		// (get) Token: 0x060056EB RID: 22251
		[XmlAttribute("FriendlyName")]
		string FriendlyName { get; }

		// Token: 0x17001587 RID: 5511
		// (get) Token: 0x060056EC RID: 22252
		[XmlAttribute("IsScript")]
		bool IsScript { get; }

		// Token: 0x17001588 RID: 5512
		// (get) Token: 0x060056ED RID: 22253
		[XmlAttribute("UseInternalAccessName")]
		bool UseInternalAccessName { get; }

		// Token: 0x17001589 RID: 5513
		// (get) Token: 0x060056EE RID: 22254
		[XmlAttribute("IsReferenceModeOnly")]
		bool IsReferenceModeOnly { get; }
	}
}
