using System;

namespace Barotrauma
{
	// Token: 0x02000281 RID: 641
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class Header : Attribute
	{
		// Token: 0x06002D35 RID: 11573 RVA: 0x00129AB8 File Offset: 0x00127CB8
		public Header(string text = "", string localizedTextTag = null)
		{
			this.Text = ((localizedTextTag != null) ? TextManager.Get(localizedTextTag) : text);
		}

		// Token: 0x04001642 RID: 5698
		public readonly LocalizedString Text;
	}
}
