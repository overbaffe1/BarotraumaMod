using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200058C RID: 1420
	public interface IBaseResourceInfo : IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo
	{
	}
}
