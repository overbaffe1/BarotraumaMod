using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005FF RID: 1535
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct SuitablePlantItem
	{
		// Token: 0x060063D2 RID: 25554 RVA: 0x0033EB0A File Offset: 0x0033CD0A
		public SuitablePlantItem(Item item, PlantItemType type, string progressBarMessage)
		{
			this.Item = item;
			this.Type = type;
			this.ProgressBarMessage = progressBarMessage;
		}

		// Token: 0x060063D3 RID: 25555 RVA: 0x0033EB21 File Offset: 0x0033CD21
		public bool IsNull()
		{
			return this.Item == null;
		}

		// Token: 0x040033C1 RID: 13249
		[Nullable(2)]
		public readonly Item Item;

		// Token: 0x040033C2 RID: 13250
		public readonly PlantItemType Type;

		// Token: 0x040033C3 RID: 13251
		public readonly string ProgressBarMessage;
	}
}
