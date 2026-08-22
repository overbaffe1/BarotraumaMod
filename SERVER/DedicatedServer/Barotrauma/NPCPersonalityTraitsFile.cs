using System;

namespace Barotrauma
{
	// Token: 0x02000140 RID: 320
	internal sealed class NPCPersonalityTraitsFile : GenericPrefabFile<NPCPersonalityTrait>
	{
		// Token: 0x06001C2A RID: 7210 RVA: 0x000CE768 File Offset: 0x000CC968
		public NPCPersonalityTraitsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C2B RID: 7211 RVA: 0x000CE772 File Offset: 0x000CC972
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "personalitytrait";
		}

		// Token: 0x06001C2C RID: 7212 RVA: 0x000CE780 File Offset: 0x000CC980
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "personalitytraits";
		}

		// Token: 0x1700082D RID: 2093
		// (get) Token: 0x06001C2D RID: 7213 RVA: 0x000CE78E File Offset: 0x000CC98E
		protected override PrefabCollection<NPCPersonalityTrait> Prefabs
		{
			get
			{
				return NPCPersonalityTrait.Traits;
			}
		}

		// Token: 0x06001C2E RID: 7214 RVA: 0x000CE795 File Offset: 0x000CC995
		protected override NPCPersonalityTrait CreatePrefab(ContentXElement element)
		{
			return new NPCPersonalityTrait(element, this);
		}
	}
}
