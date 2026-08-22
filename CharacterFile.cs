using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000221 RID: 545
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class CharacterFile : ContentFile
	{
		// Token: 0x0600369D RID: 13981 RVA: 0x00213103 File Offset: 0x00211303
		public CharacterFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600369E RID: 13982 RVA: 0x00213110 File Offset: 0x00211310
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

		// Token: 0x0600369F RID: 13983 RVA: 0x0021320A File Offset: 0x0021140A
		public override void UnloadFile()
		{
			CharacterPrefab.Prefabs.RemoveByFile(this);
			CharacterFile.ClearCaches();
		}

		// Token: 0x060036A0 RID: 13984 RVA: 0x0021321C File Offset: 0x0021141C
		private static void ClearCaches()
		{
			RagdollParams.ClearCache();
			AnimationParams.ClearCache();
		}

		// Token: 0x060036A1 RID: 13985 RVA: 0x00213228 File Offset: 0x00211428
		public override void Sort()
		{
			CharacterPrefab.Prefabs.SortAll();
		}

		// Token: 0x060036A2 RID: 13986 RVA: 0x00213234 File Offset: 0x00211434
		public override void Preload(Action<Sprite> addPreloadedSprite)
		{
			CharacterFile.<>c__DisplayClass5_0 CS$<>8__locals1;
			CS$<>8__locals1.characterPrefab = CharacterPrefab.FindByFilePath(this.Path.Value);
			CharacterPrefab characterPrefab = CS$<>8__locals1.characterPrefab;
			ContentXElement contentXElement = (characterPrefab != null) ? characterPrefab.ConfigElement : null;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to load the character config file from ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			ContentXElement mainElement = CS$<>8__locals1.characterPrefab.ConfigElement;
			mainElement.GetChildElements("sound").ForEach(delegate(ContentXElement e)
			{
				RoundSound.Load(e);
			});
			Identifier speciesName;
			if (!CharacterPrefab.CheckSpeciesName(mainElement, this, out speciesName))
			{
				return;
			}
			bool humanoid = mainElement.GetAttributeBool("humanoid", false);
			try
			{
				if (humanoid)
				{
					CS$<>8__locals1.ragdollParams = RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(speciesName, mainElement, this.ContentPackage);
				}
				else
				{
					CS$<>8__locals1.ragdollParams = RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(speciesName, mainElement, this.ContentPackage);
				}
			}
			catch (Exception e)
			{
				Exception e2;
				DebugConsole.ThrowError("Failed to preload a ragdoll file for the character \"" + CS$<>8__locals1.characterPrefab.Name + "\"", e2, CS$<>8__locals1.characterPrefab.ContentPackage, false, false);
				return;
			}
			if (CS$<>8__locals1.ragdollParams != null)
			{
				CharacterFile.<>c__DisplayClass5_1 CS$<>8__locals2;
				CS$<>8__locals2.texturePaths = new HashSet<ContentPath>();
				CharacterFile.<Preload>g__AddTexturePath|5_1(CS$<>8__locals1.ragdollParams.Texture, ref CS$<>8__locals1, ref CS$<>8__locals2);
				foreach (RagdollParams.LimbParams limb in CS$<>8__locals1.ragdollParams.Limbs)
				{
					RagdollParams.SpriteParams normalSpriteParams = limb.normalSpriteParams;
					CharacterFile.<Preload>g__AddTexturePath|5_1((normalSpriteParams != null) ? normalSpriteParams.Texture : null, ref CS$<>8__locals1, ref CS$<>8__locals2);
					RagdollParams.DeformSpriteParams deformSpriteParams = limb.deformSpriteParams;
					CharacterFile.<Preload>g__AddTexturePath|5_1((deformSpriteParams != null) ? deformSpriteParams.Texture : null, ref CS$<>8__locals1, ref CS$<>8__locals2);
					RagdollParams.SpriteParams damagedSpriteParams = limb.damagedSpriteParams;
					CharacterFile.<Preload>g__AddTexturePath|5_1((damagedSpriteParams != null) ? damagedSpriteParams.Texture : null, ref CS$<>8__locals1, ref CS$<>8__locals2);
					foreach (RagdollParams.DecorativeSpriteParams decorativeSprite in limb.decorativeSpriteParams)
					{
						CharacterFile.<Preload>g__AddTexturePath|5_1(decorativeSprite.Texture, ref CS$<>8__locals1, ref CS$<>8__locals2);
					}
				}
				foreach (ContentPath texturePath in CS$<>8__locals2.texturePaths)
				{
					addPreloadedSprite(new Sprite(texturePath.Value, Vector2.Zero));
				}
			}
		}

		// Token: 0x060036A5 RID: 13989 RVA: 0x00213524 File Offset: 0x00211724
		[CompilerGenerated]
		internal static void <Preload>g__AddTexturePath|5_1(string path, ref CharacterFile.<>c__DisplayClass5_0 A_1, ref CharacterFile.<>c__DisplayClass5_1 A_2)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			ContentPath contentPath = ContentPath.FromRaw(A_1.characterPrefab.ContentPackage, A_1.ragdollParams.Texture);
			if (contentPath.FullPath.Contains("[GENDER]"))
			{
				return;
			}
			A_2.texturePaths.Add(contentPath);
		}
	}
}
