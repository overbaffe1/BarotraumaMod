using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200046A RID: 1130
	public interface IConfigInfo : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x17001012 RID: 4114
		// (get) Token: 0x06003D5B RID: 15707
		string DataType { get; }

		// Token: 0x17001013 RID: 4115
		// (get) Token: 0x06003D5C RID: 15708
		XElement Element { get; }

		// Token: 0x17001014 RID: 4116
		// (get) Token: 0x06003D5D RID: 15709
		RunState EditableStates { get; }

		// Token: 0x17001015 RID: 4117
		// (get) Token: 0x06003D5E RID: 15710
		NetSync NetSync { get; }
	}
}
