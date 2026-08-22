using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000262 RID: 610
	internal static class DecalManager
	{
		// Token: 0x17000EBA RID: 3770
		// (get) Token: 0x06003848 RID: 14408 RVA: 0x0021767A File Offset: 0x0021587A
		// (set) Token: 0x06003849 RID: 14409 RVA: 0x00217681 File Offset: 0x00215881
		public static int GrimeSpriteCount { get; private set; } = 0;

		// Token: 0x0600384A RID: 14410 RVA: 0x0021768C File Offset: 0x0021588C
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

		// Token: 0x0600384B RID: 14411 RVA: 0x002177A8 File Offset: 0x002159A8
		public static void RemoveByFile(DecalsFile configFile)
		{
			DecalManager.Prefabs.RemoveByFile(configFile);
			DecalManager.GrimeSprites.RemoveByFile(configFile);
		}

		// Token: 0x0600384C RID: 14412 RVA: 0x002177C0 File Offset: 0x002159C0
		public static void SortAll()
		{
			DecalManager.Prefabs.SortAll();
			DecalManager.GrimeSprites.SortAll();
		}

		// Token: 0x0600384D RID: 14413 RVA: 0x002177D8 File Offset: 0x002159D8
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

		// Token: 0x04001C49 RID: 7241
		public static readonly PrefabCollection<DecalPrefab> Prefabs = new PrefabCollection<DecalPrefab>();

		// Token: 0x04001C4B RID: 7243
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
