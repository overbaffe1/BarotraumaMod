using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200024C RID: 588
	public sealed class UIStyleFile : HashlessFile
	{
		// Token: 0x06003769 RID: 14185 RVA: 0x00214EE8 File Offset: 0x002130E8
		public UIStyleFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600376A RID: 14186 RVA: 0x00214EF4 File Offset: 0x002130F4
		public void LoadFromXElement(ContentXElement parentElement, bool overriding)
		{
			Identifier elemName = parentElement.NameAsIdentifier();
			Identifier elemNameWithFontSuffix = elemName.AppendIfMissing("Font");
			if (parentElement.IsOverride())
			{
				using (IEnumerator<ContentXElement> enumerator = parentElement.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement element = enumerator.Current;
						this.LoadFromXElement(element, true);
					}
					return;
				}
			}
			if (GUIStyle.Fonts.ContainsKey(elemNameWithFontSuffix))
			{
				GUIFontPrefab prefab = new GUIFontPrefab(parentElement, this);
				GUIStyle.Fonts[elemNameWithFontSuffix].Prefabs.Add(prefab, overriding);
				return;
			}
			if (GUIStyle.Sprites.ContainsKey(elemName))
			{
				GUISpritePrefab prefab2 = new GUISpritePrefab(parentElement, this);
				GUIStyle.Sprites[elemName].Prefabs.Add(prefab2, overriding);
				return;
			}
			if (GUIStyle.SpriteSheets.ContainsKey(elemName))
			{
				GUISpriteSheetPrefab prefab3 = new GUISpriteSheetPrefab(parentElement, this);
				GUIStyle.SpriteSheets[elemName].Prefabs.Add(prefab3, overriding);
				return;
			}
			if (GUIStyle.Colors.ContainsKey(elemName))
			{
				GUIColorPrefab prefab4 = new GUIColorPrefab(parentElement, this);
				GUIStyle.Colors[elemName].Prefabs.Add(prefab4, overriding);
				return;
			}
			if (elemName == "cursor")
			{
				GUICursorPrefab prefab5 = new GUICursorPrefab(parentElement, this);
				GUIStyle.CursorSprite.Prefabs.Add(prefab5, overriding);
				return;
			}
			if (elemName == "style")
			{
				using (IEnumerator<ContentXElement> enumerator2 = parentElement.Elements().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ContentXElement element2 = enumerator2.Current;
						this.LoadFromXElement(element2, overriding);
					}
					return;
				}
			}
			GUIComponentStyle prefab6 = new GUIComponentStyle(parentElement, this, null);
			GUIStyle.ComponentStyles.Add(prefab6, overriding);
		}

		// Token: 0x0600376B RID: 14187 RVA: 0x002150B0 File Offset: 0x002132B0
		public sealed override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			ContentXElement rootElement = doc.Root.FromPackage(this.ContentPackage);
			this.LoadFromXElement(rootElement, false);
		}

		// Token: 0x0600376C RID: 14188 RVA: 0x002150E8 File Offset: 0x002132E8
		public sealed override void UnloadFile()
		{
			GUIStyle.ComponentStyles.RemoveByFile(this);
			GUIStyle.CursorSprite.Prefabs.RemoveByFile(this, null);
			GUIStyle.Fonts.Values.ForEach(delegate(GUIFont p)
			{
				p.Prefabs.RemoveByFile(this, null);
			});
			GUIStyle.Sprites.Values.ForEach(delegate(GUISprite p)
			{
				p.Prefabs.RemoveByFile(this, null);
			});
			GUIStyle.SpriteSheets.Values.ForEach(delegate(GUISpriteSheet p)
			{
				p.Prefabs.RemoveByFile(this, null);
			});
			GUIStyle.Colors.Values.ForEach(delegate(GUIColor p)
			{
				p.Prefabs.RemoveByFile(this, null);
			});
		}

		// Token: 0x0600376D RID: 14189 RVA: 0x00215180 File Offset: 0x00213380
		public sealed override void Sort()
		{
			GUIStyle.ComponentStyles.SortAll();
			GUIStyle.CursorSprite.Prefabs.Sort();
			GUIStyle.Fonts.Values.ForEach(delegate(GUIFont p)
			{
				p.Prefabs.Sort();
			});
			GUIStyle.Sprites.Values.ForEach(delegate(GUISprite p)
			{
				p.Prefabs.Sort();
			});
			GUIStyle.SpriteSheets.Values.ForEach(delegate(GUISpriteSheet p)
			{
				p.Prefabs.Sort();
			});
			GUIStyle.Colors.Values.ForEach(delegate(GUIColor p)
			{
				p.Prefabs.Sort();
			});
		}
	}
}
