using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001CE RID: 462
	public class AutonomousObjective
	{
		// Token: 0x0600326C RID: 12908 RVA: 0x002097A8 File Offset: 0x002079A8
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

		// Token: 0x04001A6F RID: 6767
		public readonly Identifier Identifier;

		// Token: 0x04001A70 RID: 6768
		public readonly Identifier Option;

		// Token: 0x04001A71 RID: 6769
		public readonly float PriorityModifier;

		// Token: 0x04001A72 RID: 6770
		public readonly bool IgnoreAtOutpost;

		// Token: 0x04001A73 RID: 6771
		public readonly bool IgnoreAtNonOutpost;
	}
}
