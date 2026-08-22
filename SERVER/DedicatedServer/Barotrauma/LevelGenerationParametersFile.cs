using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200013A RID: 314
	internal sealed class LevelGenerationParametersFile : ContentFile
	{
		// Token: 0x06001C0C RID: 7180 RVA: 0x000CE328 File Offset: 0x000CC528
		public LevelGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x000CE334 File Offset: 0x000CC534
		private void LoadBiomes(ContentXElement element, bool isOverride)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				Biome biome = new Biome(subElement, this);
				Biome.Prefabs.Add(biome, isOverride);
			}
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x000CE390 File Offset: 0x000CC590
		private void LoadLevelGenerationParams(ContentXElement element, bool isOverride)
		{
			LevelGenerationParams lParams = new LevelGenerationParams(element, this);
			LevelGenerationParams.LevelParams.Add(lParams, isOverride);
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x000CE3B4 File Offset: 0x000CC5B4
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

		// Token: 0x06001C10 RID: 7184 RVA: 0x000CE460 File Offset: 0x000CC660
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			this.LoadSubElements(doc.Root.FromPackage(this.ContentPackage), false);
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x000CE495 File Offset: 0x000CC695
		public override void UnloadFile()
		{
			LevelGenerationParams.LevelParams.RemoveByFile(this);
			Biome.Prefabs.RemoveByFile(this);
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x000CE4AD File Offset: 0x000CC6AD
		public override void Sort()
		{
			LevelGenerationParams.LevelParams.SortAll();
			Biome.Prefabs.SortAll();
		}
	}
}
