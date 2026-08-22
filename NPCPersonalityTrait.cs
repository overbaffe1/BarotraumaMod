using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001D6 RID: 470
	internal class NPCPersonalityTrait : PrefabWithUintIdentifier
	{
		// Token: 0x17000D51 RID: 3409
		// (get) Token: 0x0600328A RID: 12938 RVA: 0x00209CFB File Offset: 0x00207EFB
		public float Commonness
		{
			get
			{
				return this.commonness;
			}
		}

		// Token: 0x0600328B RID: 12939 RVA: 0x00209D04 File Offset: 0x00207F04
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

		// Token: 0x0600328C RID: 12940 RVA: 0x00209DC8 File Offset: 0x00207FC8
		public static NPCPersonalityTrait GetRandom(string seed)
		{
			MTRandom rand = new MTRandom(ToolBox.StringToInt(seed));
			return ToolBox.SelectWeightedRandom<NPCPersonalityTrait>(from t in NPCPersonalityTrait.Traits
			orderby t.UintIdentifier
			select t, (NPCPersonalityTrait t) => t.commonness, rand);
		}

		// Token: 0x0600328D RID: 12941 RVA: 0x00209E2F File Offset: 0x0020802F
		public override void Dispose()
		{
		}

		// Token: 0x04001A9C RID: 6812
		public static readonly PrefabCollection<NPCPersonalityTrait> Traits = new PrefabCollection<NPCPersonalityTrait>();

		// Token: 0x04001A9D RID: 6813
		public readonly LocalizedString DisplayName;

		// Token: 0x04001A9E RID: 6814
		public readonly List<string> AllowedDialogTags;

		// Token: 0x04001A9F RID: 6815
		private readonly float commonness;
	}
}
