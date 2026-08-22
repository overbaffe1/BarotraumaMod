using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020000DA RID: 218
	internal class NPCPersonalityTrait : PrefabWithUintIdentifier
	{
		// Token: 0x170006EA RID: 1770
		// (get) Token: 0x0600178E RID: 6030 RVA: 0x000C3505 File Offset: 0x000C1705
		public float Commonness
		{
			get
			{
				return this.commonness;
			}
		}

		// Token: 0x0600178F RID: 6031 RVA: 0x000C3510 File Offset: 0x000C1710
		public NPCPersonalityTrait(XElement element, NPCPersonalityTraitsFile file) : base(file, element.GetAttributeIdentifier("identifier", element.GetAttributeIdentifier("name", Identifier.Empty)))
		{
			string name = element.GetAttributeString("name", null);
			if (name == null)
			{
				this.DisplayName = TextManager.Get("personalitytrait." + this.Identifier.ToString()).Fallback(this.Identifier.ToString(), true);
			}
			else
			{
				this.DisplayName = name;
			}
			this.AllowedDialogTags = new List<string>(element.GetAttributeStringArray("alloweddialogtags", Array.Empty<string>(), true, false));
			this.commonness = element.GetAttributeFloat("commonness", 1f);
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x000C35D4 File Offset: 0x000C17D4
		public static NPCPersonalityTrait GetRandom(string seed)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(seed));
			return ToolBox.SelectWeightedRandom<NPCPersonalityTrait>(from t in NPCPersonalityTrait.Traits
			orderby t.UintIdentifier
			select t, (NPCPersonalityTrait t) => t.commonness, rand);
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x000C363B File Offset: 0x000C183B
		public override void Dispose()
		{
		}

		// Token: 0x04000B77 RID: 2935
		public static readonly PrefabCollection<NPCPersonalityTrait> Traits = new PrefabCollection<NPCPersonalityTrait>();

		// Token: 0x04000B78 RID: 2936
		public readonly LocalizedString DisplayName;

		// Token: 0x04000B79 RID: 2937
		public readonly List<string> AllowedDialogTags;

		// Token: 0x04000B7A RID: 2938
		private readonly float commonness;
	}
}
