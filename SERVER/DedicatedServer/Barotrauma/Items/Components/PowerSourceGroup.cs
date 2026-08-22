using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DC RID: 1244
	internal class PowerSourceGroup
	{
		// Token: 0x0400220C RID: 8716
		public PowerRange MinMaxPower;

		// Token: 0x0400220D RID: 8717
		public readonly List<Connection> Connections = new List<Connection>();
	}
}
