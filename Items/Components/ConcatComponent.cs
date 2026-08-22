using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060E RID: 1550
	internal class ConcatComponent : StringComponent
	{
		// Token: 0x1700194F RID: 6479
		// (get) Token: 0x06006408 RID: 25608 RVA: 0x0033F520 File Offset: 0x0033D720
		// (set) Token: 0x06006409 RID: 25609 RVA: 0x0033F528 File Offset: 0x0033D728
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

		// Token: 0x17001950 RID: 6480
		// (get) Token: 0x0600640A RID: 25610 RVA: 0x0033F537 File Offset: 0x0033D737
		// (set) Token: 0x0600640B RID: 25611 RVA: 0x0033F53F File Offset: 0x0033D73F
		[InGameEditable]
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Separator { get; set; }

		// Token: 0x0600640C RID: 25612 RVA: 0x0033F548 File Offset: 0x0033D748
		public ConcatComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600640D RID: 25613 RVA: 0x0033F554 File Offset: 0x0033D754
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

		// Token: 0x040033E7 RID: 13287
		private int maxOutputLength;
	}
}
