using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200024E RID: 590
	internal abstract class MapEntityPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000C85 RID: 3205
		// (get) Token: 0x06002A49 RID: 10825 RVA: 0x00114E58 File Offset: 0x00113058
		public static IEnumerable<MapEntityPrefab> List
		{
			get
			{
				return new MapEntityPrefab.<get_List>d__1(-2);
			}
		}

		// Token: 0x17000C86 RID: 3206
		// (get) Token: 0x06002A4A RID: 10826 RVA: 0x00114E6E File Offset: 0x0011306E
		// (set) Token: 0x06002A4B RID: 10827 RVA: 0x00114E75 File Offset: 0x00113075
		public static MapEntityPrefab Selected { get; set; }

		// Token: 0x06002A4C RID: 10828 RVA: 0x00114E7D File Offset: 0x0011307D
		public static bool SelectPrefab(object selection)
		{
			if ((MapEntityPrefab.Selected = (selection as MapEntityPrefab)) != null)
			{
				MapEntityPrefab.placePosition = Vector2.Zero;
				return true;
			}
			return false;
		}

		// Token: 0x06002A4D RID: 10829 RVA: 0x00114E9A File Offset: 0x0011309A
		public static object GetSelected()
		{
			return MapEntityPrefab.Selected;
		}

		// Token: 0x06002A4E RID: 10830 RVA: 0x00114EA1 File Offset: 0x001130A1
		[Obsolete("Prefer MapEntityPrefab.FindByIdentifier or MapEntityPrefab.FindByName")]
		public static MapEntityPrefab Find(string name, string identifier = null, bool showErrorMessages = true)
		{
			return MapEntityPrefab.Find(name, (identifier ?? "").ToIdentifier(), showErrorMessages);
		}

		// Token: 0x06002A4F RID: 10831 RVA: 0x00114EBC File Offset: 0x001130BC
		[Obsolete("Prefer MapEntityPrefab.FindByIdentifier or MapEntityPrefab.FindByName")]
		public static MapEntityPrefab Find(string name, Identifier identifier, bool showErrorMessages = true)
		{
			if (string.IsNullOrEmpty(name) && !identifier.IsEmpty)
			{
				if (CoreEntityPrefab.Prefabs.ContainsKey(identifier))
				{
					return CoreEntityPrefab.Prefabs[identifier];
				}
				if (StructurePrefab.Prefabs.ContainsKey(identifier))
				{
					return StructurePrefab.Prefabs[identifier];
				}
				if (ItemPrefab.Prefabs.ContainsKey(identifier))
				{
					return ItemPrefab.Prefabs[identifier];
				}
				if (ItemAssemblyPrefab.Prefabs.ContainsKey(identifier))
				{
					return ItemAssemblyPrefab.Prefabs[identifier];
				}
			}
			Func<string, bool> <>9__0;
			Func<string, bool> <>9__1;
			foreach (MapEntityPrefab prefab in MapEntityPrefab.List)
			{
				if (!identifier.IsEmpty)
				{
					if (prefab.Identifier != identifier)
					{
						if (prefab.Aliases == null)
						{
							continue;
						}
						IEnumerable<string> aliases = prefab.Aliases;
						Func<string, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((string a) => a == identifier));
						}
						if (aliases.Any(predicate))
						{
							return prefab;
						}
						continue;
					}
					else if (string.IsNullOrEmpty(name))
					{
						return prefab;
					}
				}
				if (!string.IsNullOrEmpty(name))
				{
					if (!prefab.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !prefab.OriginalName.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						if (prefab.Aliases == null)
						{
							continue;
						}
						IEnumerable<string> aliases2 = prefab.Aliases;
						Func<string, bool> predicate2;
						if ((predicate2 = <>9__1) == null)
						{
							predicate2 = (<>9__1 = ((string a) => a.Equals(name, StringComparison.OrdinalIgnoreCase)));
						}
						if (!aliases2.Any(predicate2))
						{
							continue;
						}
					}
					return prefab;
				}
			}
			if (showErrorMessages)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Failed to find a matching MapEntityPrefab (name: \"",
					name,
					"\", identifier: \"",
					identifier.ToString(),
					"\").\n",
					Environment.StackTrace.CleanupStackTrace()
				}), null, null, false, false);
			}
			return null;
		}

		// Token: 0x06002A50 RID: 10832 RVA: 0x0011510C File Offset: 0x0011330C
		public static MapEntityPrefab GetRandom(Predicate<MapEntityPrefab> predicate, Rand.RandSync sync)
		{
			return MapEntityPrefab.List.GetRandom((MapEntityPrefab p) => predicate(p), sync);
		}

		// Token: 0x06002A51 RID: 10833 RVA: 0x00115140 File Offset: 0x00113340
		public static MapEntityPrefab Find(Predicate<MapEntityPrefab> predicate)
		{
			return MapEntityPrefab.List.FirstOrDefault((MapEntityPrefab p) => predicate(p));
		}

		// Token: 0x06002A52 RID: 10834 RVA: 0x00115170 File Offset: 0x00113370
		public static MapEntityPrefab FindByName(string name)
		{
			if (name.IsNullOrEmpty())
			{
				throw new ArgumentException("name must not be null or empty");
			}
			Func<string, bool> <>9__1;
			IEnumerable<MapEntityPrefab> matches = MapEntityPrefab.List.Where(delegate(MapEntityPrefab prefab)
			{
				if (prefab.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || prefab.OriginalName.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (prefab.Aliases != null)
				{
					IEnumerable<string> aliases = prefab.Aliases;
					Func<string, bool> predicate;
					if ((predicate = <>9__1) == null)
					{
						predicate = (<>9__1 = ((string a) => a.Equals(name, StringComparison.OrdinalIgnoreCase)));
					}
					return aliases.Any(predicate);
				}
				return false;
			});
			if (matches.Count<MapEntityPrefab>() > 1)
			{
				MapEntityPrefab mapEntityPrefab;
				if ((mapEntityPrefab = matches.FirstOrDefault((MapEntityPrefab prefab) => !prefab.HideInMenus)) == null)
				{
					mapEntityPrefab = matches.FirstOrDefault(delegate(MapEntityPrefab prefab)
					{
						ItemPrefab ip = prefab as ItemPrefab;
						return ip != null && ip.VariantOf.IsEmpty;
					});
				}
				MapEntityPrefab bestMatch = mapEntityPrefab;
				if (bestMatch != null)
				{
					return bestMatch;
				}
			}
			return matches.FirstOrDefault<MapEntityPrefab>();
		}

		// Token: 0x06002A53 RID: 10835 RVA: 0x0011521C File Offset: 0x0011341C
		public static MapEntityPrefab FindByIdentifier(Identifier identifier)
		{
			CoreEntityPrefab corePrefab;
			if (CoreEntityPrefab.Prefabs.TryGet(identifier, out corePrefab))
			{
				return corePrefab;
			}
			ItemPrefab itemPrefab;
			if (ItemPrefab.Prefabs.TryGet(identifier, out itemPrefab))
			{
				return itemPrefab;
			}
			StructurePrefab structurePrefab;
			if (StructurePrefab.Prefabs.TryGet(identifier, out structurePrefab))
			{
				return structurePrefab;
			}
			ItemAssemblyPrefab itemAssemblyPrefab;
			if (!ItemAssemblyPrefab.Prefabs.TryGet(identifier, out itemAssemblyPrefab))
			{
				return null;
			}
			return itemAssemblyPrefab;
		}

		// Token: 0x17000C87 RID: 3207
		// (get) Token: 0x06002A54 RID: 10836
		public abstract Sprite Sprite { get; }

		// Token: 0x17000C88 RID: 3208
		// (get) Token: 0x06002A55 RID: 10837 RVA: 0x0011526E File Offset: 0x0011346E
		public virtual bool CanSpriteFlipX { get; }

		// Token: 0x17000C89 RID: 3209
		// (get) Token: 0x06002A56 RID: 10838 RVA: 0x00115276 File Offset: 0x00113476
		public virtual bool CanSpriteFlipY { get; }

		// Token: 0x17000C8A RID: 3210
		// (get) Token: 0x06002A57 RID: 10839
		public abstract string OriginalName { get; }

		// Token: 0x17000C8B RID: 3211
		// (get) Token: 0x06002A58 RID: 10840
		public abstract LocalizedString Name { get; }

		// Token: 0x17000C8C RID: 3212
		// (get) Token: 0x06002A59 RID: 10841
		public abstract ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x17000C8D RID: 3213
		// (get) Token: 0x06002A5A RID: 10842
		public abstract ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x17000C8E RID: 3214
		// (get) Token: 0x06002A5B RID: 10843
		public abstract MapEntityCategory Category { get; }

		// Token: 0x17000C8F RID: 3215
		// (get) Token: 0x06002A5C RID: 10844
		public abstract ImmutableHashSet<string> Aliases { get; }

		// Token: 0x17000C90 RID: 3216
		// (get) Token: 0x06002A5D RID: 10845 RVA: 0x0011527E File Offset: 0x0011347E
		// (set) Token: 0x06002A5E RID: 10846 RVA: 0x00115286 File Offset: 0x00113486
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ResizeHorizontal { get; protected set; }

		// Token: 0x17000C91 RID: 3217
		// (get) Token: 0x06002A5F RID: 10847 RVA: 0x0011528F File Offset: 0x0011348F
		// (set) Token: 0x06002A60 RID: 10848 RVA: 0x00115297 File Offset: 0x00113497
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ResizeVertical { get; protected set; }

		// Token: 0x17000C92 RID: 3218
		// (get) Token: 0x06002A61 RID: 10849 RVA: 0x001152A0 File Offset: 0x001134A0
		// (set) Token: 0x06002A62 RID: 10850 RVA: 0x001152A8 File Offset: 0x001134A8
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public LocalizedString Description { get; protected set; }

		// Token: 0x17000C93 RID: 3219
		// (get) Token: 0x06002A63 RID: 10851 RVA: 0x001152B1 File Offset: 0x001134B1
		// (set) Token: 0x06002A64 RID: 10852 RVA: 0x001152B9 File Offset: 0x001134B9
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string AllowedUpgrades { get; protected set; }

		// Token: 0x17000C94 RID: 3220
		// (get) Token: 0x06002A65 RID: 10853 RVA: 0x001152C2 File Offset: 0x001134C2
		// (set) Token: 0x06002A66 RID: 10854 RVA: 0x001152CA File Offset: 0x001134CA
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool HideInMenus { get; protected set; }

		// Token: 0x17000C95 RID: 3221
		// (get) Token: 0x06002A67 RID: 10855 RVA: 0x001152D3 File Offset: 0x001134D3
		// (set) Token: 0x06002A68 RID: 10856 RVA: 0x001152DB File Offset: 0x001134DB
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool HideInEditors { get; protected set; }

		// Token: 0x17000C96 RID: 3222
		// (get) Token: 0x06002A69 RID: 10857 RVA: 0x001152E4 File Offset: 0x001134E4
		// (set) Token: 0x06002A6A RID: 10858 RVA: 0x001152EC File Offset: 0x001134EC
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Subcategory { get; protected set; }

		// Token: 0x17000C97 RID: 3223
		// (get) Token: 0x06002A6B RID: 10859 RVA: 0x001152F5 File Offset: 0x001134F5
		// (set) Token: 0x06002A6C RID: 10860 RVA: 0x001152FD File Offset: 0x001134FD
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool Linkable { get; protected set; }

		// Token: 0x17000C98 RID: 3224
		// (get) Token: 0x06002A6D RID: 10861 RVA: 0x00115306 File Offset: 0x00113506
		// (set) Token: 0x06002A6E RID: 10862 RVA: 0x0011530E File Offset: 0x0011350E
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "", "", false)]
		public Color SpriteColor { get; protected set; }

		// Token: 0x17000C99 RID: 3225
		// (get) Token: 0x06002A6F RID: 10863 RVA: 0x00115317 File Offset: 0x00113517
		// (set) Token: 0x06002A70 RID: 10864 RVA: 0x0011531F File Offset: 0x0011351F
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.1f, 10f, 1, DecimalCount = 3)]
		public float Scale { get; protected set; }

		// Token: 0x06002A71 RID: 10865 RVA: 0x00115328 File Offset: 0x00113528
		protected MapEntityPrefab(Identifier identifier) : base(null, identifier)
		{
		}

		// Token: 0x06002A72 RID: 10866 RVA: 0x0011533D File Offset: 0x0011353D
		public MapEntityPrefab(ContentXElement element, ContentFile file) : base(file, element)
		{
		}

		// Token: 0x06002A73 RID: 10867 RVA: 0x00115354 File Offset: 0x00113554
		public string GetItemNameTextId()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("entityname.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			string textId = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!TextManager.ContainsTag(textId))
			{
				return null;
			}
			return textId;
		}

		// Token: 0x06002A74 RID: 10868 RVA: 0x00115398 File Offset: 0x00113598
		public string GetHullNameTextId()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("roomname.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			string textId = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!TextManager.ContainsTag(textId))
			{
				return null;
			}
			return textId;
		}

		// Token: 0x06002A75 RID: 10869 RVA: 0x001153DC File Offset: 0x001135DC
		public IEnumerable<Identifier> GetAllowedUpgrades()
		{
			if (string.IsNullOrWhiteSpace(this.AllowedUpgrades))
			{
				return Enumerable.Empty<Identifier>();
			}
			if (this.allowedUpgradeSet == null || this.cachedAllowedUpgrades != this.AllowedUpgrades)
			{
				this.allowedUpgradeSet = this.AllowedUpgrades.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
				this.cachedAllowedUpgrades = this.AllowedUpgrades;
			}
			return this.allowedUpgradeSet;
		}

		// Token: 0x06002A76 RID: 10870 RVA: 0x00115444 File Offset: 0x00113644
		public bool HasSubCategory(string subcategory)
		{
			return subcategory != null && subcategory.Equals(this.Subcategory, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002A77 RID: 10871
		protected abstract void CreateInstance(Rectangle rect);

		// Token: 0x06002A78 RID: 10872 RVA: 0x00115458 File Offset: 0x00113658
		public bool NameMatches(string name, StringComparison comparisonType)
		{
			return this.OriginalName.Equals(name, comparisonType) || (this.Aliases != null && this.Aliases.Any((string a) => a.Equals(name, comparisonType)));
		}

		// Token: 0x06002A79 RID: 10873 RVA: 0x001154B8 File Offset: 0x001136B8
		public bool NameMatches(IEnumerable<string> allowedNames, StringComparison comparisonType)
		{
			return allowedNames.Any((string n) => this.NameMatches(n, comparisonType));
		}

		// Token: 0x06002A7A RID: 10874 RVA: 0x001154EC File Offset: 0x001136EC
		public bool IsLinkAllowed(MapEntityPrefab target)
		{
			return target != null && ((target is StructurePrefab && this.AllowedLinks.Contains("structure".ToIdentifier())) || (target is ItemPrefab && this.AllowedLinks.Contains("item".ToIdentifier())) || (target is LinkedSubmarinePrefab && this.Tags.Contains("dock".ToIdentifier())) || (this is LinkedSubmarinePrefab && target.Tags.Contains("dock".ToIdentifier())) || this.AllowedLinks.Contains(target.Identifier) || target.AllowedLinks.Contains(this.Identifier) || target.Tags.Any((Identifier t) => this.AllowedLinks.Contains(t)) || this.Tags.Any((Identifier t) => target.AllowedLinks.Contains(t)));
		}

		// Token: 0x06002A7B RID: 10875 RVA: 0x00115618 File Offset: 0x00113818
		protected void LoadDescription(ContentXElement element)
		{
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", Identifier.Empty);
			string originalDescription = this.Description.Value;
			XAttribute descriptionIdenfifierAttribute = element.GetAttribute("descriptionidentifier");
			if (descriptionIdenfifierAttribute != null)
			{
				Identifier descriptionIdentifier = element.GetAttributeIdentifier("descriptionidentifier", Identifier.Empty);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EntityDescription.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(descriptionIdentifier);
				this.Description = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			else if (nameIdentifier == Identifier.Empty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("EntityDescription.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				this.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("EntityDescription.");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(nameIdentifier);
				this.Description = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			if (!originalDescription.IsNullOrEmpty())
			{
				this.Description = this.Description.Fallback(originalDescription, true);
			}
		}

		// Token: 0x040014D7 RID: 5335
		protected static Vector2 placePosition;

		// Token: 0x040014E4 RID: 5348
		private string cachedAllowedUpgrades = "";

		// Token: 0x040014E5 RID: 5349
		private ImmutableHashSet<Identifier> allowedUpgradeSet;
	}
}
