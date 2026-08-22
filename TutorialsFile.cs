using System;

namespace Barotrauma
{
	// Token: 0x0200024B RID: 587
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class TutorialsFile : GenericPrefabFile<TutorialPrefab>
	{
		// Token: 0x17000E99 RID: 3737
		// (get) Token: 0x06003764 RID: 14180 RVA: 0x00214EB2 File Offset: 0x002130B2
		protected override PrefabCollection<TutorialPrefab> Prefabs
		{
			get
			{
				return TutorialPrefab.Prefabs;
			}
		}

		// Token: 0x06003765 RID: 14181 RVA: 0x00214EB9 File Offset: 0x002130B9
		public TutorialsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003766 RID: 14182 RVA: 0x00214EC3 File Offset: 0x002130C3
		protected override bool MatchesSingular(Identifier identifier)
		{
			return identifier == "Tutorial";
		}

		// Token: 0x06003767 RID: 14183 RVA: 0x00214ED1 File Offset: 0x002130D1
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "Tutorials";
		}

		// Token: 0x06003768 RID: 14184 RVA: 0x00214EDF File Offset: 0x002130DF
		protected override TutorialPrefab CreatePrefab(ContentXElement element)
		{
			return new TutorialPrefab(this, element);
		}
	}
}
