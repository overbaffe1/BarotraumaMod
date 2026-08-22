using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000604 RID: 1540
	internal class PowerSourceGroup
	{
		// Token: 0x040033D5 RID: 13269
		public PowerRange MinMaxPower;

		// Token: 0x040033D6 RID: 13270
		public readonly List<Connection> Connections = new List<Connection>();
	}
}
