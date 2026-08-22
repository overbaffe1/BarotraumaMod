using System;

namespace Barotrauma
{
	// Token: 0x02000243 RID: 579
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class SoundsFile : GenericPrefabFile<SoundPrefab>
	{
		// Token: 0x0600373F RID: 14143 RVA: 0x00214A2D File Offset: 0x00212C2D
		public SoundsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x17000E94 RID: 3732
		// (get) Token: 0x06003740 RID: 14144 RVA: 0x00214A37 File Offset: 0x00212C37
		protected override PrefabCollection<SoundPrefab> Prefabs
		{
			get
			{
				return SoundPrefab.Prefabs;
			}
		}

		// Token: 0x06003741 RID: 14145 RVA: 0x00214A40 File Offset: 0x00212C40
		protected override SoundPrefab CreatePrefab(ContentXElement element)
		{
			Identifier elemName = element.NameAsIdentifier();
			if (SoundPrefab.TagToDerivedPrefab.ContainsKey(elemName))
			{
				return Activator.CreateInstance(SoundPrefab.TagToDerivedPrefab[elemName], new object[]
				{
					element,
					this
				}) as SoundPrefab;
			}
			return new SoundPrefab(element, this, false);
		}

		// Token: 0x06003742 RID: 14146 RVA: 0x00214A8D File Offset: 0x00212C8D
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "sounds";
		}

		// Token: 0x06003743 RID: 14147 RVA: 0x00214A9B File Offset: 0x00212C9B
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06003744 RID: 14148 RVA: 0x00214AA7 File Offset: 0x00212CA7
		public override Md5Hash CalculateHash()
		{
			return Md5Hash.Blank;
		}
	}
}
