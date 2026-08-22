using System;

namespace Barotrauma
{
	// Token: 0x02000092 RID: 146
	internal abstract class ShipGlobalIssue
	{
		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x060012A1 RID: 4769 RVA: 0x000A43AA File Offset: 0x000A25AA
		// (set) Token: 0x060012A2 RID: 4770 RVA: 0x000A43B2 File Offset: 0x000A25B2
		public float GlobalImportance { get; set; }

		// Token: 0x060012A3 RID: 4771 RVA: 0x000A43BB File Offset: 0x000A25BB
		public ShipGlobalIssue(ShipCommandManager shipCommandManager)
		{
			this.shipCommandManager = shipCommandManager;
		}

		// Token: 0x060012A4 RID: 4772
		public abstract void CalculateGlobalIssue();

		// Token: 0x040008D3 RID: 2259
		protected ShipCommandManager shipCommandManager;
	}
}
