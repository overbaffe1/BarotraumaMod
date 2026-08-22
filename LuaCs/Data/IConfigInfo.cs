using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000574 RID: 1396
	public interface IConfigInfo : IConfigDisplayInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x1700153B RID: 5435
		// (get) Token: 0x060055FA RID: 22010
		string DataType { get; }

		// Token: 0x1700153C RID: 5436
		// (get) Token: 0x060055FB RID: 22011
		XElement Element { get; }

		// Token: 0x1700153D RID: 5437
		// (get) Token: 0x060055FC RID: 22012
		RunState EditableStates { get; }

		// Token: 0x1700153E RID: 5438
		// (get) Token: 0x060055FD RID: 22013
		NetSync NetSync { get; }
	}
}
