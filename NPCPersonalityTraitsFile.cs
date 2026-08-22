using System;

namespace Barotrauma
{
	// Token: 0x02000236 RID: 566
	internal sealed class NPCPersonalityTraitsFile : GenericPrefabFile<NPCPersonalityTrait>
	{
		// Token: 0x06003709 RID: 14089 RVA: 0x00214200 File Offset: 0x00212400
		public NPCPersonalityTraitsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600370A RID: 14090 RVA: 0x0021420A File Offset: 0x0021240A
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "personalitytrait";
		}

		// Token: 0x0600370B RID: 14091 RVA: 0x00214218 File Offset: 0x00212418
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "personalitytraits";
		}

		// Token: 0x17000E8E RID: 3726
		// (get) Token: 0x0600370C RID: 14092 RVA: 0x00214226 File Offset: 0x00212426
		protected override PrefabCollection<NPCPersonalityTrait> Prefabs
		{
			get
			{
				return NPCPersonalityTrait.Traits;
			}
		}

		// Token: 0x0600370D RID: 14093 RVA: 0x0021422D File Offset: 0x0021242D
		protected override NPCPersonalityTrait CreatePrefab(ContentXElement element)
		{
			return new NPCPersonalityTrait(element, this);
		}
	}
}
