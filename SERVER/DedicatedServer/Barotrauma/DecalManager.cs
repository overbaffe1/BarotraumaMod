using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016E RID: 366
	internal static class DecalManager
	{
		// Token: 0x17000866 RID: 2150
		// (get) Token: 0x06001D7C RID: 7548 RVA: 0x000D2042 File Offset: 0x000D0242
		// (set) Token: 0x06001D7D RID: 7549 RVA: 0x000D2049 File Offset: 0x000D0249
		public static int GrimeSpriteCount { get; private set; } = 0;

		// Token: 0x06001D7E RID: 7550 RVA: 0x000D2054 File Offset: 0x000D0254
		public static void LoadFromFile(DecalsFile configFile)
		{
			XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
			if (doc == null)
			{
				return;
			}
			bool allowOverriding = false;
			ContentXElement mainElement = doc.Root.FromPackage(configFile.ContentPackage);
			if (doc.Root.IsOverride())
			{
				mainElement = mainElement.FirstElement();
				allowOverriding = true;
			}
			int grimeIndex = 0;
			foreach (ContentXElement sourceElement in mainElement.Elements())
			{
				ContentXElement element = sourceElement.IsOverride() ? sourceElement.FirstElement() : sourceElement;
				bool isOverride = allowOverriding || sourceElement.IsOverride();
				string name = element.Name.ToString().ToLowerInvariant();
				if (name == "grime")
				{
					DecalManager.GrimeSprites.Add(new GrimeSprite(new Sprite(element, "", "", false, 1f), configFile, grimeIndex), isOverride);
					grimeIndex++;
				}
				else
				{
					DecalPrefab prefab = new DecalPrefab(element, configFile);
					DecalManager.Prefabs.Add(prefab, isOverride);
				}
			}
		}

		// Token: 0x06001D7F RID: 7551 RVA: 0x000D2170 File Offset: 0x000D0370
		public static void RemoveByFile(DecalsFile configFile)
		{
			DecalManager.Prefabs.RemoveByFile(configFile);
			DecalManager.GrimeSprites.RemoveByFile(configFile);
		}

		// Token: 0x06001D80 RID: 7552 RVA: 0x000D2188 File Offset: 0x000D0388
		public static void SortAll()
		{
			DecalManager.Prefabs.SortAll();
			DecalManager.GrimeSprites.SortAll();
		}

		// Token: 0x06001D81 RID: 7553 RVA: 0x000D21A0 File Offset: 0x000D03A0
		public static Decal CreateDecal(string decalName, float scale, Vector2 worldPosition, Hull hull, int? spriteIndex = null)
		{
			string lowerCaseDecalName = decalName.ToLowerInvariant();
			if (!DecalManager.Prefabs.ContainsKey(lowerCaseDecalName))
			{
				DebugConsole.ThrowError("Decal prefab " + decalName + " not found!", null, null, false, false);
				return null;
			}
			DecalPrefab prefab = DecalManager.Prefabs[lowerCaseDecalName];
			return new Decal(prefab, scale, worldPosition, hull, spriteIndex);
		}

		// Token: 0x04000D52 RID: 3410
		public static readonly PrefabCollection<DecalPrefab> Prefabs = new PrefabCollection<DecalPrefab>();

		// Token: 0x04000D54 RID: 3412
		public static readonly PrefabCollection<GrimeSprite> GrimeSprites = new PrefabCollection<GrimeSprite>(delegate(GrimeSprite sprite, bool b)
		{
			DecalManager.GrimeSpriteCount = Math.Max(DecalManager.GrimeSpriteCount, sprite.IndexInFile + 1);
		}, delegate(GrimeSprite s)
		{
			DecalManager.GrimeSpriteCount = (from p in DecalManager.GrimeSprites.AllPrefabs.SelectMany((KeyValuePair<Identifier, PrefabSelector<GrimeSprite>> kvp) => kvp.Value)
			where p != s
			select p.IndexInFile + 1).MaxOrNull<int>().GetValueOrDefault();
		}, null, null, null);
	}
}
