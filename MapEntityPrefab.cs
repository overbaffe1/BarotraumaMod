using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000E8 RID: 232
	internal abstract class MapEntityPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x06002060 RID: 8288 RVA: 0x001454A0 File Offset: 0x001436A0
		public RichString CreateTooltipText()
		{
			LocalizedString name = this.Category.HasFlag(MapEntityCategory.Legacy) ? TextManager.GetWithVariable("legacyitemformat", "[name]", this.Name, FormatCapitals.No) : this.Name;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(GUIStyle.TextColorBright.ToStringHex());
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(name);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
			LocalizedString tooltip = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!this.Description.IsNullOrEmpty())
			{
				tooltip += '\n' + this.Description;
			}
			if (this.IsModded)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 3);
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(tooltip);
				defaultInterpolatedStringHandler2.AppendLiteral("\n‖color:");
				defaultInterpolatedStringHandler2.AppendFormatted(Color.MediumPurple.ToStringHex());
				defaultInterpolatedStringHandler2.AppendLiteral("‖");
				ContentPackage contentPackage = base.ContentPackage;
				defaultInterpolatedStringHandler2.AppendFormatted((contentPackage != null) ? contentPackage.Name : null);
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
				tooltip = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			return RichString.Rich(tooltip, null);
		}

		// Token: 0x170008B4 RID: 2228
		// (get) Token: 0x06002061 RID: 8289 RVA: 0x001455E3 File Offset: 0x001437E3
		public bool IsModded
		{
			get
			{
				return base.ContentPackage != GameMain.VanillaContent && base.ContentPackage != null;
			}
		}

		// Token: 0x06002062 RID: 8290 RVA: 0x00145600 File Offset: 0x00143800
		public virtual void UpdatePlacing(Camera cam)
		{
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				MapEntityPrefab.Selected = null;
				return;
			}
			Vector2 placeSize = Submarine.GridSize;
			if (MapEntityPrefab.placePosition == Vector2.Zero)
			{
				Vector2 position = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
				if (PlayerInput.PrimaryMouseButtonHeld() && GUI.MouseOn == null)
				{
					MapEntityPrefab.placePosition = position;
					return;
				}
			}
			else
			{
				Vector2 position2 = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
				if (this.ResizeHorizontal)
				{
					placeSize.X = position2.X - MapEntityPrefab.placePosition.X;
				}
				if (this.ResizeVertical)
				{
					placeSize.Y = MapEntityPrefab.placePosition.Y - position2.Y;
				}
				Rectangle newRect = Submarine.AbsRect(MapEntityPrefab.placePosition, placeSize);
				newRect.Width = (int)Math.Max((float)newRect.Width, Submarine.GridSize.X);
				newRect.Height = (int)Math.Max((float)newRect.Height, Submarine.GridSize.Y);
				if (Submarine.MainSub != null)
				{
					newRect.Location -= MathUtils.ToPoint(Submarine.MainSub.Position);
				}
				if (PlayerInput.PrimaryMouseButtonReleased() && GUI.MouseOn == null)
				{
					this.CreateInstance(newRect);
					MapEntityPrefab.placePosition = Vector2.Zero;
					if (!PlayerInput.IsShiftDown())
					{
						MapEntityPrefab.Selected = null;
					}
				}
				newRect.Y = -newRect.Y;
			}
		}

		// Token: 0x06002063 RID: 8291 RVA: 0x00145768 File Offset: 0x00143968
		public virtual void DrawPlacing(SpriteBatch spriteBatch, Camera cam)
		{
			if (MapEntityPrefab.placePosition == Vector2.Zero)
			{
				Vector2 position = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
				GUI.DrawLine(spriteBatch, new Vector2(position.X - (float)GameMain.GraphicsWidth, -position.Y), new Vector2(position.X + (float)GameMain.GraphicsWidth, -position.Y), Color.White, 0f, (float)((int)(2f / cam.Zoom)));
				GUI.DrawLine(spriteBatch, new Vector2(position.X, -(position.Y - (float)GameMain.GraphicsHeight)), new Vector2(position.X, -(position.Y + (float)GameMain.GraphicsHeight)), Color.White, 0f, (float)((int)(2f / cam.Zoom)));
				return;
			}
			Vector2 placeSize = Submarine.GridSize;
			Vector2 position2 = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
			if (this.ResizeHorizontal)
			{
				placeSize.X = position2.X - MapEntityPrefab.placePosition.X;
			}
			if (this.ResizeVertical)
			{
				placeSize.Y = MapEntityPrefab.placePosition.Y - position2.Y;
			}
			Rectangle newRect = Submarine.AbsRect(MapEntityPrefab.placePosition, placeSize);
			newRect.Width = (int)Math.Max((float)newRect.Width, Submarine.GridSize.X);
			newRect.Height = (int)Math.Max((float)newRect.Height, Submarine.GridSize.Y);
			if (Submarine.MainSub != null)
			{
				newRect.Location -= Submarine.MainSub.Position.ToPoint();
			}
			newRect.Y = -newRect.Y;
			GUI.DrawRectangle(spriteBatch, newRect, Color.DarkBlue, false, 0f, 1f);
		}

		// Token: 0x06002064 RID: 8292 RVA: 0x0014593C File Offset: 0x00143B3C
		public virtual void DrawPlacing(SpriteBatch spriteBatch, Rectangle drawRect, float scale = 1f, float rotation = 0f, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			if (Submarine.MainSub != null)
			{
				drawRect.Location -= Submarine.MainSub.Position.ToPoint();
			}
			drawRect.Y = -drawRect.Y;
			GUI.DrawRectangle(spriteBatch, drawRect, Color.White, false, 0f, 1f);
		}

		// Token: 0x06002065 RID: 8293 RVA: 0x00145999 File Offset: 0x00143B99
		public void DrawListLine(SpriteBatch spriteBatch, Vector2 pos, Color color)
		{
			GUIStyle.Font.DrawString(spriteBatch, this.OriginalName, pos, color, ForceUpperCase.Inherit, false);
		}

		// Token: 0x170008B5 RID: 2229
		// (get) Token: 0x06002066 RID: 8294 RVA: 0x001459B0 File Offset: 0x00143BB0
		public static IEnumerable<MapEntityPrefab> List
		{
			get
			{
				return new MapEntityPrefab.<get_List>d__8(-2);
			}
		}

		// Token: 0x170008B6 RID: 2230
		// (get) Token: 0x06002067 RID: 8295 RVA: 0x001459C6 File Offset: 0x00143BC6
		// (set) Token: 0x06002068 RID: 8296 RVA: 0x001459CD File Offset: 0x00143BCD
		public static MapEntityPrefab Selected { get; set; }

		// Token: 0x06002069 RID: 8297 RVA: 0x001459D5 File Offset: 0x00143BD5
		public static bool SelectPrefab(object selection)
		{
			if ((MapEntityPrefab.Selected = (selection as MapEntityPrefab)) != null)
			{
				MapEntityPrefab.placePosition = Vector2.Zero;
				return true;
			}
			return false;
		}

		// Token: 0x0600206A RID: 8298 RVA: 0x001459F2 File Offset: 0x00143BF2
		public static object GetSelected()
		{
			return MapEntityPrefab.Selected;
		}

		// Token: 0x0600206B RID: 8299 RVA: 0x001459F9 File Offset: 0x00143BF9
		[Obsolete("Prefer MapEntityPrefab.FindByIdentifier or MapEntityPrefab.FindByName")]
		public static MapEntityPrefab Find(string name, string identifier = null, bool showErrorMessages = true)
		{
			return MapEntityPrefab.Find(name, (identifier ?? "").ToIdentifier(), showErrorMessages);
		}

		// Token: 0x0600206C RID: 8300 RVA: 0x00145A14 File Offset: 0x00143C14
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

		// Token: 0x0600206D RID: 8301 RVA: 0x00145C64 File Offset: 0x00143E64
		public static MapEntityPrefab GetRandom(Predicate<MapEntityPrefab> predicate, Rand.RandSync sync)
		{
			return MapEntityPrefab.List.GetRandom((MapEntityPrefab p) => predicate(p), sync);
		}

		// Token: 0x0600206E RID: 8302 RVA: 0x00145C98 File Offset: 0x00143E98
		public static MapEntityPrefab Find(Predicate<MapEntityPrefab> predicate)
		{
			return MapEntityPrefab.List.FirstOrDefault((MapEntityPrefab p) => predicate(p));
		}

		// Token: 0x0600206F RID: 8303 RVA: 0x00145CC8 File Offset: 0x00143EC8
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

		// Token: 0x06002070 RID: 8304 RVA: 0x00145D74 File Offset: 0x00143F74
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

		// Token: 0x170008B7 RID: 2231
		// (get) Token: 0x06002071 RID: 8305
		public abstract Sprite Sprite { get; }

		// Token: 0x170008B8 RID: 2232
		// (get) Token: 0x06002072 RID: 8306 RVA: 0x00145DC6 File Offset: 0x00143FC6
		public virtual bool CanSpriteFlipX { get; }

		// Token: 0x170008B9 RID: 2233
		// (get) Token: 0x06002073 RID: 8307 RVA: 0x00145DCE File Offset: 0x00143FCE
		public virtual bool CanSpriteFlipY { get; }

		// Token: 0x170008BA RID: 2234
		// (get) Token: 0x06002074 RID: 8308
		public abstract string OriginalName { get; }

		// Token: 0x170008BB RID: 2235
		// (get) Token: 0x06002075 RID: 8309
		public abstract LocalizedString Name { get; }

		// Token: 0x170008BC RID: 2236
		// (get) Token: 0x06002076 RID: 8310
		public abstract ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x170008BD RID: 2237
		// (get) Token: 0x06002077 RID: 8311
		public abstract ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x170008BE RID: 2238
		// (get) Token: 0x06002078 RID: 8312
		public abstract MapEntityCategory Category { get; }

		// Token: 0x170008BF RID: 2239
		// (get) Token: 0x06002079 RID: 8313
		public abstract ImmutableHashSet<string> Aliases { get; }

		// Token: 0x170008C0 RID: 2240
		// (get) Token: 0x0600207A RID: 8314 RVA: 0x00145DD6 File Offset: 0x00143FD6
		// (set) Token: 0x0600207B RID: 8315 RVA: 0x00145DDE File Offset: 0x00143FDE
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ResizeHorizontal { get; protected set; }

		// Token: 0x170008C1 RID: 2241
		// (get) Token: 0x0600207C RID: 8316 RVA: 0x00145DE7 File Offset: 0x00143FE7
		// (set) Token: 0x0600207D RID: 8317 RVA: 0x00145DEF File Offset: 0x00143FEF
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ResizeVertical { get; protected set; }

		// Token: 0x170008C2 RID: 2242
		// (get) Token: 0x0600207E RID: 8318 RVA: 0x00145DF8 File Offset: 0x00143FF8
		// (set) Token: 0x0600207F RID: 8319 RVA: 0x00145E00 File Offset: 0x00144000
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public LocalizedString Description { get; protected set; }

		// Token: 0x170008C3 RID: 2243
		// (get) Token: 0x06002080 RID: 8320 RVA: 0x00145E09 File Offset: 0x00144009
		// (set) Token: 0x06002081 RID: 8321 RVA: 0x00145E11 File Offset: 0x00144011
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string AllowedUpgrades { get; protected set; }

		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x06002082 RID: 8322 RVA: 0x00145E1A File Offset: 0x0014401A
		// (set) Token: 0x06002083 RID: 8323 RVA: 0x00145E22 File Offset: 0x00144022
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool HideInMenus { get; protected set; }

		// Token: 0x170008C5 RID: 2245
		// (get) Token: 0x06002084 RID: 8324 RVA: 0x00145E2B File Offset: 0x0014402B
		// (set) Token: 0x06002085 RID: 8325 RVA: 0x00145E33 File Offset: 0x00144033
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool HideInEditors { get; protected set; }

		// Token: 0x170008C6 RID: 2246
		// (get) Token: 0x06002086 RID: 8326 RVA: 0x00145E3C File Offset: 0x0014403C
		// (set) Token: 0x06002087 RID: 8327 RVA: 0x00145E44 File Offset: 0x00144044
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Subcategory { get; protected set; }

		// Token: 0x170008C7 RID: 2247
		// (get) Token: 0x06002088 RID: 8328 RVA: 0x00145E4D File Offset: 0x0014404D
		// (set) Token: 0x06002089 RID: 8329 RVA: 0x00145E55 File Offset: 0x00144055
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool Linkable { get; protected set; }

		// Token: 0x170008C8 RID: 2248
		// (get) Token: 0x0600208A RID: 8330 RVA: 0x00145E5E File Offset: 0x0014405E
		// (set) Token: 0x0600208B RID: 8331 RVA: 0x00145E66 File Offset: 0x00144066
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "", "", false)]
		public Color SpriteColor { get; protected set; }

		// Token: 0x170008C9 RID: 2249
		// (get) Token: 0x0600208C RID: 8332 RVA: 0x00145E6F File Offset: 0x0014406F
		// (set) Token: 0x0600208D RID: 8333 RVA: 0x00145E77 File Offset: 0x00144077
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.1f, 10f, 1, DecimalCount = 3)]
		public float Scale { get; protected set; }

		// Token: 0x0600208E RID: 8334 RVA: 0x00145E80 File Offset: 0x00144080
		protected MapEntityPrefab(Identifier identifier) : base(null, identifier)
		{
		}

		// Token: 0x0600208F RID: 8335 RVA: 0x00145E95 File Offset: 0x00144095
		public MapEntityPrefab(ContentXElement element, ContentFile file) : base(file, element)
		{
		}

		// Token: 0x06002090 RID: 8336 RVA: 0x00145EAC File Offset: 0x001440AC
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

		// Token: 0x06002091 RID: 8337 RVA: 0x00145EF0 File Offset: 0x001440F0
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

		// Token: 0x06002092 RID: 8338 RVA: 0x00145F34 File Offset: 0x00144134
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

		// Token: 0x06002093 RID: 8339 RVA: 0x00145F9C File Offset: 0x0014419C
		public bool HasSubCategory(string subcategory)
		{
			return subcategory != null && subcategory.Equals(this.Subcategory, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002094 RID: 8340
		protected abstract void CreateInstance(Rectangle rect);

		// Token: 0x06002095 RID: 8341 RVA: 0x00145FB0 File Offset: 0x001441B0
		public bool NameMatches(string name, StringComparison comparisonType)
		{
			return this.OriginalName.Equals(name, comparisonType) || (this.Aliases != null && this.Aliases.Any((string a) => a.Equals(name, comparisonType)));
		}

		// Token: 0x06002096 RID: 8342 RVA: 0x00146010 File Offset: 0x00144210
		public bool NameMatches(IEnumerable<string> allowedNames, StringComparison comparisonType)
		{
			return allowedNames.Any((string n) => this.NameMatches(n, comparisonType));
		}

		// Token: 0x06002097 RID: 8343 RVA: 0x00146044 File Offset: 0x00144244
		public bool IsLinkAllowed(MapEntityPrefab target)
		{
			return target != null && ((target is StructurePrefab && this.AllowedLinks.Contains("structure".ToIdentifier())) || (target is ItemPrefab && this.AllowedLinks.Contains("item".ToIdentifier())) || (target is LinkedSubmarinePrefab && this.Tags.Contains("dock".ToIdentifier())) || (this is LinkedSubmarinePrefab && target.Tags.Contains("dock".ToIdentifier())) || this.AllowedLinks.Contains(target.Identifier) || target.AllowedLinks.Contains(this.Identifier) || target.Tags.Any((Identifier t) => this.AllowedLinks.Contains(t)) || this.Tags.Any((Identifier t) => target.AllowedLinks.Contains(t)));
		}

		// Token: 0x06002098 RID: 8344 RVA: 0x00146170 File Offset: 0x00144370
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

		// Token: 0x0400108A RID: 4234
		protected static Vector2 placePosition;

		// Token: 0x04001097 RID: 4247
		private string cachedAllowedUpgrades = "";

		// Token: 0x04001098 RID: 4248
		private ImmutableHashSet<Identifier> allowedUpgradeSet;
	}
}
