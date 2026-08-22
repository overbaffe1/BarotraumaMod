using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200013D RID: 317
	internal sealed class MapGenerationParametersFile : ContentFile
	{
		// Token: 0x06001C1D RID: 7197 RVA: 0x000CE541 File Offset: 0x000CC741
		public MapGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x000CE54C File Offset: 0x000CC74C
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Loading map generation parameters file failed: ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentPackage);
			bool isOverride = mainElement.IsOverride();
			if (isOverride)
			{
				mainElement = mainElement.FirstElement();
			}
			MapGenerationParams prefab = new MapGenerationParams(mainElement, this);
			MapGenerationParams.Params.Add(prefab, isOverride);
		}

		// Token: 0x06001C1F RID: 7199 RVA: 0x000CE5D3 File Offset: 0x000CC7D3
		public override void UnloadFile()
		{
			MapGenerationParams.Params.RemoveByFile(this, null);
		}

		// Token: 0x06001C20 RID: 7200 RVA: 0x000CE5E1 File Offset: 0x000CC7E1
		public override void Sort()
		{
			MapGenerationParams.Params.Sort();
		}
	}
}
