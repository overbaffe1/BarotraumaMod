using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000230 RID: 560
	internal sealed class LevelGenerationParametersFile : ContentFile
	{
		// Token: 0x060036EB RID: 14059 RVA: 0x00213DC0 File Offset: 0x00211FC0
		public LevelGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036EC RID: 14060 RVA: 0x00213DCC File Offset: 0x00211FCC
		private void LoadBiomes(ContentXElement element, bool isOverride)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				Biome biome = new Biome(subElement, this);
				Biome.Prefabs.Add(biome, isOverride);
			}
		}

		// Token: 0x060036ED RID: 14061 RVA: 0x00213E28 File Offset: 0x00212028
		private void LoadLevelGenerationParams(ContentXElement element, bool isOverride)
		{
			LevelGenerationParams lParams = new LevelGenerationParams(element, this);
			LevelGenerationParams.LevelParams.Add(lParams, isOverride);
		}

		// Token: 0x060036EE RID: 14062 RVA: 0x00213E4C File Offset: 0x0021204C
		private void LoadSubElements(ContentXElement element, bool overridePropagation)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.IsOverride())
				{
					this.LoadSubElements(subElement, true);
				}
				else
				{
					Identifier identifier = subElement.NameAsIdentifier();
					if (identifier == "clear")
					{
						LevelGenerationParams.LevelParams.AddOverrideFile(this);
						Biome.Prefabs.AddOverrideFile(this);
					}
					else
					{
						identifier = subElement.NameAsIdentifier();
						if (identifier == "biomes")
						{
							this.LoadBiomes(subElement, overridePropagation);
						}
						else
						{
							this.LoadLevelGenerationParams(subElement, overridePropagation);
						}
					}
				}
			}
		}

		// Token: 0x060036EF RID: 14063 RVA: 0x00213EF8 File Offset: 0x002120F8
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			this.LoadSubElements(doc.Root.FromPackage(this.ContentPackage), false);
		}

		// Token: 0x060036F0 RID: 14064 RVA: 0x00213F2D File Offset: 0x0021212D
		public override void UnloadFile()
		{
			LevelGenerationParams.LevelParams.RemoveByFile(this);
			Biome.Prefabs.RemoveByFile(this);
		}

		// Token: 0x060036F1 RID: 14065 RVA: 0x00213F45 File Offset: 0x00212145
		public override void Sort()
		{
			LevelGenerationParams.LevelParams.SortAll();
			Biome.Prefabs.SortAll();
		}
	}
}
