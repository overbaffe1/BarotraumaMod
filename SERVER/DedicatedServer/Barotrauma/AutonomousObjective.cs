using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D0 RID: 208
	public class AutonomousObjective
	{
		// Token: 0x0600172F RID: 5935 RVA: 0x000C23A8 File Offset: 0x000C05A8
		public AutonomousObjective(XElement element)
		{
			this.Identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (this.Identifier == Identifier.Empty)
			{
				this.Identifier = element.GetAttributeIdentifier("aitag", Identifier.Empty);
			}
			this.Option = element.GetAttributeIdentifier("option", Identifier.Empty);
			this.PriorityModifier = element.GetAttributeFloat("prioritymodifier", 1f);
			this.PriorityModifier = MathHelper.Max(this.PriorityModifier, 0f);
			this.IgnoreAtOutpost = element.GetAttributeBool("ignoreatoutpost", false);
			this.IgnoreAtNonOutpost = element.GetAttributeBool("ignoreatnonoutpost", false);
		}

		// Token: 0x04000B2B RID: 2859
		public readonly Identifier Identifier;

		// Token: 0x04000B2C RID: 2860
		public readonly Identifier Option;

		// Token: 0x04000B2D RID: 2861
		public readonly float PriorityModifier;

		// Token: 0x04000B2E RID: 2862
		public readonly bool IgnoreAtOutpost;

		// Token: 0x04000B2F RID: 2863
		public readonly bool IgnoreAtNonOutpost;
	}
}
