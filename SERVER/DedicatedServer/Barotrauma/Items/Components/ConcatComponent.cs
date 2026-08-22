using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E9 RID: 1257
	internal class ConcatComponent : StringComponent
	{
		// Token: 0x17001309 RID: 4873
		// (get) Token: 0x060046F9 RID: 18169 RVA: 0x001C4E88 File Offset: 0x001C3088
		// (set) Token: 0x060046FA RID: 18170 RVA: 0x001C4E90 File Offset: 0x001C3090
		[Editable]
		[Serialize(256, IsPropertySaveable.No, "The maximum length of the output string. Warning: Large values can lead to large memory usage or networking load.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x1700130A RID: 4874
		// (get) Token: 0x060046FB RID: 18171 RVA: 0x001C4E9F File Offset: 0x001C309F
		// (set) Token: 0x060046FC RID: 18172 RVA: 0x001C4EA7 File Offset: 0x001C30A7
		[InGameEditable]
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Separator { get; set; }

		// Token: 0x060046FD RID: 18173 RVA: 0x001C4EB0 File Offset: 0x001C30B0
		public ConcatComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046FE RID: 18174 RVA: 0x001C4EBC File Offset: 0x001C30BC
		protected override string Calculate(string signal1, string signal2)
		{
			string output;
			if (string.IsNullOrEmpty(this.Separator))
			{
				output = signal1 + signal2;
			}
			else
			{
				output = signal1 + this.Separator + signal2;
			}
			if (output.Length > this.maxOutputLength)
			{
				return output.Substring(0, this.MaxOutputLength);
			}
			return output;
		}

		// Token: 0x0400223B RID: 8763
		private int maxOutputLength;
	}
}
