using System;

namespace Barotrauma
{
	// Token: 0x02000155 RID: 341
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TutorialsFile : GenericPrefabFile<TutorialPrefab>
	{
		// Token: 0x17000836 RID: 2102
		// (get) Token: 0x06001C7B RID: 7291 RVA: 0x000CF2EE File Offset: 0x000CD4EE
		protected override PrefabCollection<TutorialPrefab> Prefabs
		{
			get
			{
				return TutorialPrefab.Prefabs;
			}
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x000CF2F5 File Offset: 0x000CD4F5
		public TutorialsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x000CF2FF File Offset: 0x000CD4FF
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "Tutorial";
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x000CF30D File Offset: 0x000CD50D
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "Tutorials";
		}

		// Token: 0x06001C7F RID: 7295 RVA: 0x000CF31B File Offset: 0x000CD51B
		protected override TutorialPrefab CreatePrefab(ContentXElement element)
		{
			return new TutorialPrefab(this, element);
		}
	}
}
