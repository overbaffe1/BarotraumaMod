using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000233 RID: 563
	internal sealed class MapGenerationParametersFile : ContentFile
	{
		// Token: 0x060036FC RID: 14076 RVA: 0x00213FD9 File Offset: 0x002121D9
		public MapGenerationParametersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036FD RID: 14077 RVA: 0x00213FE4 File Offset: 0x002121E4
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

		// Token: 0x060036FE RID: 14078 RVA: 0x0021406B File Offset: 0x0021226B
		public override void UnloadFile()
		{
			MapGenerationParams.Params.RemoveByFile(this, null);
		}

		// Token: 0x060036FF RID: 14079 RVA: 0x00214079 File Offset: 0x00212279
		public override void Sort()
		{
			MapGenerationParams.Params.Sort();
		}
	}
}
