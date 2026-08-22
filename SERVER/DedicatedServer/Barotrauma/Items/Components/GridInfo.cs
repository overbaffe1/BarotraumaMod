using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DB RID: 1243
	internal class GridInfo
	{
		// Token: 0x06004698 RID: 18072 RVA: 0x001C31C0 File Offset: 0x001C13C0
		public GridInfo(int id)
		{
			this.ID = id;
		}

		// Token: 0x06004699 RID: 18073 RVA: 0x001C31E5 File Offset: 0x001C13E5
		public void RemoveConnection(Connection c)
		{
			this.Connections.Remove(c);
			if (this.Connections.Count == 0 && Powered.Grids.ContainsKey(this.ID))
			{
				Powered.Grids.Remove(this.ID);
			}
		}

		// Token: 0x0600469A RID: 18074 RVA: 0x001C3224 File Offset: 0x001C1424
		public void AddConnection(Connection c)
		{
			this.Connections.Add(c);
		}

		// Token: 0x0600469B RID: 18075 RVA: 0x001C3234 File Offset: 0x001C1434
		public void AddSrc(Connection c)
		{
			if (this.PowerSourceGroups.ContainsKey(c.Priority))
			{
				this.PowerSourceGroups[c.Priority].Connections.Add(c);
				return;
			}
			PowerSourceGroup group = new PowerSourceGroup();
			group.Connections.Add(c);
			this.PowerSourceGroups[c.Priority] = group;
		}

		// Token: 0x04002206 RID: 8710
		public readonly int ID;

		// Token: 0x04002207 RID: 8711
		public float Voltage;

		// Token: 0x04002208 RID: 8712
		public float Load;

		// Token: 0x04002209 RID: 8713
		public float Power;

		// Token: 0x0400220A RID: 8714
		public readonly List<Connection> Connections = new List<Connection>();

		// Token: 0x0400220B RID: 8715
		public readonly SortedList<PowerPriority, PowerSourceGroup> PowerSourceGroups = new SortedList<PowerPriority, PowerSourceGroup>();
	}
}
