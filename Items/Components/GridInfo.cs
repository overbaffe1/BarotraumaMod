using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000603 RID: 1539
	internal class GridInfo
	{
		// Token: 0x060063DB RID: 25563 RVA: 0x0033EBFC File Offset: 0x0033CDFC
		public GridInfo(int id)
		{
			this.ID = id;
		}

		// Token: 0x060063DC RID: 25564 RVA: 0x0033EC21 File Offset: 0x0033CE21
		public void RemoveConnection(Connection c)
		{
			this.Connections.Remove(c);
			if (this.Connections.Count == 0 && Powered.Grids.ContainsKey(this.ID))
			{
				Powered.Grids.Remove(this.ID);
			}
		}

		// Token: 0x060063DD RID: 25565 RVA: 0x0033EC60 File Offset: 0x0033CE60
		public void AddConnection(Connection c)
		{
			this.Connections.Add(c);
		}

		// Token: 0x060063DE RID: 25566 RVA: 0x0033EC70 File Offset: 0x0033CE70
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

		// Token: 0x040033CF RID: 13263
		public readonly int ID;

		// Token: 0x040033D0 RID: 13264
		public float Voltage;

		// Token: 0x040033D1 RID: 13265
		public float Load;

		// Token: 0x040033D2 RID: 13266
		public float Power;

		// Token: 0x040033D3 RID: 13267
		public readonly List<Connection> Connections = new List<Connection>();

		// Token: 0x040033D4 RID: 13268
		public readonly SortedList<PowerPriority, PowerSourceGroup> PowerSourceGroups = new SortedList<PowerPriority, PowerSourceGroup>();
	}
}
