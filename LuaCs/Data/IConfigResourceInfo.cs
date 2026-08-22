using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058D RID: 1421
	public interface IConfigResourceInfo : IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
	}
}
