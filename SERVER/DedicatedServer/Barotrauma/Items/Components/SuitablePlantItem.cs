using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D5 RID: 1237
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct SuitablePlantItem
	{
		// Token: 0x0600465B RID: 18011 RVA: 0x001C1676 File Offset: 0x001BF876
		public SuitablePlantItem(Item item, PlantItemType type, string progressBarMessage)
		{
			this.Item = item;
			this.Type = type;
			this.ProgressBarMessage = progressBarMessage;
		}

		// Token: 0x0600465C RID: 18012 RVA: 0x001C168D File Offset: 0x001BF88D
		public bool IsNull()
		{
			return this.Item == null;
		}

		// Token: 0x040021D9 RID: 8665
		[Nullable(2)]
		public readonly Item Item;

		// Token: 0x040021DA RID: 8666
		public readonly PlantItemType Type;

		// Token: 0x040021DB RID: 8667
		public readonly string ProgressBarMessage;
	}
}
