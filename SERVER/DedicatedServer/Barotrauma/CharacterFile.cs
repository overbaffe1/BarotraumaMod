using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200012B RID: 299
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CharacterFile : ContentFile
	{
		// Token: 0x06001BBF RID: 7103 RVA: 0x000CD980 File Offset: 0x000CBB80
		public CharacterFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x000CD98C File Offset: 0x000CBB8C
		public override void LoadFile()
		{
			CharacterFile.ClearCaches();
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Loading character file failed: ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.ContentPackage, false, false);
				return;
			}
			if (CharacterPrefab.Prefabs.AllPrefabs.Any((KeyValuePair<Identifier, PrefabSelector<CharacterPrefab>> kvp) => kvp.Value.Any((CharacterPrefab cf) => ((cf != null) ? cf.ContentFile : null) == this)))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Duplicate path: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(this.Path);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.ContentPackage, false, false);
				return;
			}
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentPackage);
			bool isOverride = mainElement.IsOverride();
			if (isOverride)
			{
				mainElement = mainElement.FirstElement();
			}
			Identifier i;
			if (!CharacterPrefab.CheckSpeciesName(mainElement, this, out i))
			{
				return;
			}
			CharacterPrefab prefab = new CharacterPrefab(mainElement, this);
			CharacterPrefab.Prefabs.Add(prefab, isOverride);
		}

		// Token: 0x06001BC1 RID: 7105 RVA: 0x000CDA86 File Offset: 0x000CBC86
		public override void UnloadFile()
		{
			CharacterPrefab.Prefabs.RemoveByFile(this);
			CharacterFile.ClearCaches();
		}

		// Token: 0x06001BC2 RID: 7106 RVA: 0x000CDA98 File Offset: 0x000CBC98
		private static void ClearCaches()
		{
			RagdollParams.ClearCache();
			AnimationParams.ClearCache();
		}

		// Token: 0x06001BC3 RID: 7107 RVA: 0x000CDAA4 File Offset: 0x000CBCA4
		public override void Sort()
		{
			CharacterPrefab.Prefabs.SortAll();
		}

		// Token: 0x06001BC4 RID: 7108 RVA: 0x000CDAB0 File Offset: 0x000CBCB0
		public override void Preload(Action<Sprite> addPreloadedSprite)
		{
		}
	}
}
