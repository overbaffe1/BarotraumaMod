using System;

namespace Barotrauma
{
	// Token: 0x02000352 RID: 850
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class Header : Attribute
	{
		// Token: 0x06004247 RID: 16967 RVA: 0x00249BA8 File Offset: 0x00247DA8
		public Header(string text = "", string localizedTextTag = null)
		{
			this.Text = ((localizedTextTag != null) ? TextManager.Get(localizedTextTag) : text);
		}

		// Token: 0x04002286 RID: 8838
		public readonly LocalizedString Text;
	}
}
