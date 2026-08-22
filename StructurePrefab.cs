using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000EB RID: 235
	internal class StructurePrefab : MapEntityPrefab
	{
		// Token: 0x0600211C RID: 8476 RVA: 0x0014CC2C File Offset: 0x0014AE2C
		public override void UpdatePlacing(Camera cam)
		{
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				MapEntityPrefab.Selected = null;
				return;
			}
			Vector2 position = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
			Vector2 size = this.ScaledSize;
			Rectangle newRect = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
			if (MapEntityPrefab.placePosition == Vector2.Zero)
			{
				if (PlayerInput.PrimaryMouseButtonHeld() && GUI.MouseOn == null)
				{
					MapEntityPrefab.placePosition = Submarine.MouseToWorldGrid(cam, Submarine.MainSub, null, false);
				}
				newRect.X = (int)position.X;
				newRect.Y = (int)position.Y;
				return;
			}
			Vector2 placeSize = size;
			if (base.ResizeHorizontal)
			{
				placeSize.X = position.X - MapEntityPrefab.placePosition.X;
			}
			if (base.ResizeVertical)
			{
				placeSize.Y = MapEntityPrefab.placePosition.Y - position.Y;
			}
			if (base.ResizeHorizontal && Math.Abs(placeSize.X) < Submarine.GridSize.X)
			{
				placeSize.X = Submarine.GridSize.X;
			}
			if (base.ResizeVertical && Math.Abs(placeSize.Y) < Submarine.GridSize.Y)
			{
				placeSize.Y = Submarine.GridSize.Y;
			}
			newRect = Submarine.AbsRect(MapEntityPrefab.placePosition, placeSize);
			if (PlayerInput.PrimaryMouseButtonReleased())
			{
				newRect.Location -= MathUtils.ToPoint(Submarine.MainSub.Position);
				Structure structure = new Structure(newRect, this, Submarine.MainSub, 0, null)
				{
					Submarine = Submarine.MainSub
				};
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
				{
					structure
				}, false, true));
				MapEntityPrefab.placePosition = Vector2.Zero;
				if (!PlayerInput.IsShiftDown())
				{
					MapEntityPrefab.Selected = null;
				}
				return;
			}
		}

		// Token: 0x0600211D RID: 8477 RVA: 0x0014CE04 File Offset: 0x0014B004
		public override void DrawPlacing(SpriteBatch spriteBatch, Camera cam)
		{
			Submarine mainSub = Submarine.MainSub;
			Vector2? mousePos = null;
			Vector2 position = Submarine.MouseToWorldGrid(cam, mainSub, mousePos, false);
			Rectangle newRect = new Rectangle((int)position.X, (int)position.Y, (int)this.ScaledSize.X, (int)this.ScaledSize.Y);
			if (MapEntityPrefab.placePosition == Vector2.Zero)
			{
				newRect.X = (int)position.X;
				newRect.Y = (int)position.Y;
			}
			else
			{
				Vector2 placeSize = this.ScaledSize;
				if (base.ResizeHorizontal)
				{
					placeSize.X = position.X - MapEntityPrefab.placePosition.X;
				}
				if (base.ResizeVertical)
				{
					placeSize.Y = MapEntityPrefab.placePosition.Y - position.Y;
				}
				newRect = Submarine.AbsRect(MapEntityPrefab.placePosition, placeSize);
			}
			Sprite sprite = this.Sprite;
			Vector2 position2 = new Vector2((float)newRect.X, (float)(-(float)newRect.Y));
			Vector2 targetSize = new Vector2((float)newRect.Width, (float)newRect.Height);
			float rotation = 0f;
			mousePos = new Vector2?(this.TextureScale * base.Scale);
			Color? color = new Color?(base.SpriteColor);
			sprite.DrawTiled(spriteBatch, position2, targetSize, rotation, null, color, null, mousePos, null);
			float thickness = Math.Max(1f / cam.Zoom, 1f);
			int zoomInvariantWidth = (int)((float)GameMain.GraphicsWidth / cam.Zoom);
			int zoomInvariantHeight = (int)((float)GameMain.GraphicsHeight / cam.Zoom);
			GUI.DrawRectangle(spriteBatch, new Rectangle(newRect.X - zoomInvariantWidth, -newRect.Y, newRect.Width + zoomInvariantWidth * 2, newRect.Height), Color.White, false, 0f, thickness);
			GUI.DrawRectangle(spriteBatch, new Rectangle(newRect.X, -newRect.Y - zoomInvariantHeight, newRect.Width, newRect.Height + zoomInvariantHeight * 2), Color.White, false, 0f, thickness);
		}

		// Token: 0x0600211E RID: 8478 RVA: 0x0014CFFC File Offset: 0x0014B1FC
		public override void DrawPlacing(SpriteBatch spriteBatch, Rectangle placeRect, float scale = 1f, float rotation = 0f, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			Vector2 position = placeRect.Location.ToVector2().FlipY();
			position += placeRect.Size.ToVector2() * 0.5f;
			Sprite sprite = this.Sprite;
			Vector2 position2 = position;
			Vector2 targetSize = placeRect.Size.ToVector2();
			Color? color = new Color?(base.SpriteColor * 0.8f);
			Vector2? origin = new Vector2?(placeRect.Size.ToVector2() * 0.5f);
			Vector2? vector = new Vector2?(this.TextureScale * scale);
			sprite.DrawTiled(spriteBatch, position2, targetSize, spriteEffects ^ this.Sprite.effects, rotation, origin, color, null, vector, null);
		}

		// Token: 0x170008F3 RID: 2291
		// (get) Token: 0x0600211F RID: 8479 RVA: 0x0014D0D0 File Offset: 0x0014B2D0
		public override LocalizedString Name { get; }

		// Token: 0x170008F4 RID: 2292
		// (get) Token: 0x06002120 RID: 8480 RVA: 0x0014D0D8 File Offset: 0x0014B2D8
		public override bool CanSpriteFlipX { get; }

		// Token: 0x170008F5 RID: 2293
		// (get) Token: 0x06002121 RID: 8481 RVA: 0x0014D0E0 File Offset: 0x0014B2E0
		public override bool CanSpriteFlipY { get; }

		// Token: 0x170008F6 RID: 2294
		// (get) Token: 0x06002122 RID: 8482 RVA: 0x0014D0E8 File Offset: 0x0014B2E8
		public Vector2 ScaledSize
		{
			get
			{
				return this.Size * base.Scale;
			}
		}

		// Token: 0x170008F7 RID: 2295
		// (get) Token: 0x06002123 RID: 8483 RVA: 0x0014D0FB File Offset: 0x0014B2FB
		public override Sprite Sprite { get; }

		// Token: 0x170008F8 RID: 2296
		// (get) Token: 0x06002124 RID: 8484 RVA: 0x0014D103 File Offset: 0x0014B303
		public override string OriginalName { get; }

		// Token: 0x170008F9 RID: 2297
		// (get) Token: 0x06002125 RID: 8485 RVA: 0x0014D10B File Offset: 0x0014B30B
		public override ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x170008FA RID: 2298
		// (get) Token: 0x06002126 RID: 8486 RVA: 0x0014D113 File Offset: 0x0014B313
		public override ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x170008FB RID: 2299
		// (get) Token: 0x06002127 RID: 8487 RVA: 0x0014D11B File Offset: 0x0014B31B
		public override MapEntityCategory Category { get; }

		// Token: 0x170008FC RID: 2300
		// (get) Token: 0x06002128 RID: 8488 RVA: 0x0014D123 File Offset: 0x0014B323
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x170008FD RID: 2301
		// (get) Token: 0x06002129 RID: 8489 RVA: 0x0014D12B File Offset: 0x0014B32B
		// (set) Token: 0x0600212A RID: 8490 RVA: 0x0014D133 File Offset: 0x0014B333
		[Serialize(false, IsPropertySaveable.No, "Does the structure have a physics body?", "", false)]
		public bool Body { get; private set; }

		// Token: 0x170008FE RID: 2302
		// (get) Token: 0x0600212B RID: 8491 RVA: 0x0014D13C File Offset: 0x0014B33C
		// (set) Token: 0x0600212C RID: 8492 RVA: 0x0014D144 File Offset: 0x0014B344
		[Serialize(0f, IsPropertySaveable.No, "Rotation of the physics body in degrees.", "", false)]
		public float BodyRotation { get; private set; }

		// Token: 0x170008FF RID: 2303
		// (get) Token: 0x0600212D RID: 8493 RVA: 0x0014D14D File Offset: 0x0014B34D
		// (set) Token: 0x0600212E RID: 8494 RVA: 0x0014D155 File Offset: 0x0014B355
		[Serialize(0f, IsPropertySaveable.No, "Width of the physics body in pixels.", "", false)]
		public float BodyWidth { get; private set; }

		// Token: 0x17000900 RID: 2304
		// (get) Token: 0x0600212F RID: 8495 RVA: 0x0014D15E File Offset: 0x0014B35E
		// (set) Token: 0x06002130 RID: 8496 RVA: 0x0014D166 File Offset: 0x0014B366
		[Serialize(0f, IsPropertySaveable.No, "Height of the physics body in pixels.", "", false)]
		public float BodyHeight { get; private set; }

		// Token: 0x17000901 RID: 2305
		// (get) Token: 0x06002131 RID: 8497 RVA: 0x0014D16F File Offset: 0x0014B36F
		// (set) Token: 0x06002132 RID: 8498 RVA: 0x0014D177 File Offset: 0x0014B377
		[Serialize("0.0,0.0", IsPropertySaveable.No, "Offset of the physics body from the center of the structure in pixels.", "", false)]
		public Vector2 BodyOffset { get; private set; }

		// Token: 0x17000902 RID: 2306
		// (get) Token: 0x06002133 RID: 8499 RVA: 0x0014D180 File Offset: 0x0014B380
		// (set) Token: 0x06002134 RID: 8500 RVA: 0x0014D188 File Offset: 0x0014B388
		[Serialize(false, IsPropertySaveable.No, "Is the structure a platform (i.e. a \"floor\" the players can pass through)? Only relevant if the structure has a physics body.", "", false)]
		public bool Platform { get; private set; }

		// Token: 0x17000903 RID: 2307
		// (get) Token: 0x06002135 RID: 8501 RVA: 0x0014D191 File Offset: 0x0014B391
		// (set) Token: 0x06002136 RID: 8502 RVA: 0x0014D199 File Offset: 0x0014B399
		[Serialize(false, IsPropertySaveable.No, "Can items like signal components be attached on this structure? Should be enabled on structures like decorative background walls.", "", false)]
		public bool AllowAttachItems { get; private set; }

		// Token: 0x17000904 RID: 2308
		// (get) Token: 0x06002137 RID: 8503 RVA: 0x0014D1A2 File Offset: 0x0014B3A2
		// (set) Token: 0x06002138 RID: 8504 RVA: 0x0014D1AA File Offset: 0x0014B3AA
		[Serialize(true, IsPropertySaveable.No, "Can the structure be rotated in the submarine editor?", "", false)]
		public bool AllowRotatingInEditor { get; set; }

		// Token: 0x17000905 RID: 2309
		// (get) Token: 0x06002139 RID: 8505 RVA: 0x0014D1B3 File Offset: 0x0014B3B3
		// (set) Token: 0x0600213A RID: 8506 RVA: 0x0014D1BB File Offset: 0x0014B3BB
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float MinHealth { get; private set; }

		// Token: 0x17000906 RID: 2310
		// (get) Token: 0x0600213B RID: 8507 RVA: 0x0014D1C4 File Offset: 0x0014B3C4
		// (set) Token: 0x0600213C RID: 8508 RVA: 0x0014D1CC File Offset: 0x0014B3CC
		[Serialize(100f, IsPropertySaveable.No, "", "", false)]
		public float Health
		{
			get
			{
				return this.health;
			}
			private set
			{
				this.health = Math.Max(value, this.MinHealth);
			}
		}

		// Token: 0x17000907 RID: 2311
		// (get) Token: 0x0600213D RID: 8509 RVA: 0x0014D1E0 File Offset: 0x0014B3E0
		// (set) Token: 0x0600213E RID: 8510 RVA: 0x0014D1E8 File Offset: 0x0014B3E8
		[Serialize(true, IsPropertySaveable.No, "Should the structure be indestructible when used in an outpost?", "", false)]
		public bool IndestructibleInOutposts { get; private set; }

		// Token: 0x17000908 RID: 2312
		// (get) Token: 0x0600213F RID: 8511 RVA: 0x0014D1F1 File Offset: 0x0014B3F1
		// (set) Token: 0x06002140 RID: 8512 RVA: 0x0014D1F9 File Offset: 0x0014B3F9
		[Serialize(false, IsPropertySaveable.No, "Should the structure cast shadows and obstruct visibility when LOS is enabled?", "", false)]
		public bool CastShadow { get; private set; }

		// Token: 0x17000909 RID: 2313
		// (get) Token: 0x06002141 RID: 8513 RVA: 0x0014D202 File Offset: 0x0014B402
		// (set) Token: 0x06002142 RID: 8514 RVA: 0x0014D20A File Offset: 0x0014B40A
		[Serialize(Direction.None, IsPropertySaveable.No, "Makes the structure function as a staircase.", "", false)]
		public Direction StairDirection { get; private set; }

		// Token: 0x1700090A RID: 2314
		// (get) Token: 0x06002143 RID: 8515 RVA: 0x0014D213 File Offset: 0x0014B413
		// (set) Token: 0x06002144 RID: 8516 RVA: 0x0014D21B File Offset: 0x0014B41B
		[Serialize(45f, IsPropertySaveable.No, "Angle of the stairs in degrees. Only relevant if StairDirection is something else than None.", "", false)]
		public float StairAngle { get; private set; }

		// Token: 0x1700090B RID: 2315
		// (get) Token: 0x06002145 RID: 8517 RVA: 0x0014D224 File Offset: 0x0014B424
		// (set) Token: 0x06002146 RID: 8518 RVA: 0x0014D22C File Offset: 0x0014B42C
		[Serialize(false, IsPropertySaveable.No, "If enabled, monsters will not be able to target this structure.", "", false)]
		public bool NoAITarget { get; private set; }

		// Token: 0x1700090C RID: 2316
		// (get) Token: 0x06002147 RID: 8519 RVA: 0x0014D235 File Offset: 0x0014B435
		// (set) Token: 0x06002148 RID: 8520 RVA: 0x0014D23D File Offset: 0x0014B43D
		[Serialize("0,0", IsPropertySaveable.Yes, "Size of the structure in pixels. If not set, the size is determined, based on the attributes width and height, and if those aren't defined either, based on the size of the structure's sprite.", "", false)]
		public Vector2 Size { get; private set; }

		// Token: 0x1700090D RID: 2317
		// (get) Token: 0x06002149 RID: 8521 RVA: 0x0014D246 File Offset: 0x0014B446
		// (set) Token: 0x0600214A RID: 8522 RVA: 0x0014D24E File Offset: 0x0014B44E
		[Serialize("", IsPropertySaveable.Yes, "Tag of the sound that plays when something damages the wall.", "", false)]
		public string DamageSound { get; private set; }

		// Token: 0x1700090E RID: 2318
		// (get) Token: 0x0600214B RID: 8523 RVA: 0x0014D257 File Offset: 0x0014B457
		// (set) Token: 0x0600214C RID: 8524 RVA: 0x0014D25F File Offset: 0x0014B45F
		[Serialize("shrapnel", IsPropertySaveable.Yes, "Identifier of the particles emitted when something damages the wall.", "", false)]
		public string DamageParticle { get; private set; }

		// Token: 0x1700090F RID: 2319
		// (get) Token: 0x0600214D RID: 8525 RVA: 0x0014D268 File Offset: 0x0014B468
		// (set) Token: 0x0600214E RID: 8526 RVA: 0x0014D270 File Offset: 0x0014B470
		[Editable(DecimalCount = 3)]
		[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "", "", false)]
		public Vector2 TextureScale
		{
			get
			{
				return this.textureScale;
			}
			private set
			{
				this.textureScale = new Vector2(MathHelper.Clamp(value.X, 0.01f, 10f), MathHelper.Clamp(value.Y, 0.01f, 10f));
			}
		}

		// Token: 0x0600214F RID: 8527 RVA: 0x0014D2A8 File Offset: 0x0014B4A8
		protected override Identifier DetermineIdentifier(XElement element)
		{
			Identifier identifier = base.DetermineIdentifier(element);
			string originalName = element.GetAttributeString("name", "");
			if (identifier.IsEmpty && !string.IsNullOrEmpty(originalName))
			{
				string categoryStr = element.GetAttributeString("category", "Misc");
				MapEntityCategory category;
				if (Enum.TryParse<MapEntityCategory>(categoryStr, true, out category) && category.HasFlag(MapEntityCategory.Legacy))
				{
					identifier = ("legacystructure_" + originalName.Replace(" ", "")).ToIdentifier();
				}
			}
			return identifier;
		}

		// Token: 0x06002150 RID: 8528 RVA: 0x0014D334 File Offset: 0x0014B534
		public StructurePrefab(ContentXElement element, StructureFile file) : base(element, file)
		{
			this.OriginalName = element.GetAttributeString("name", "");
			this.ConfigElement = element;
			ContentXElement parent = element.Parent;
			Identifier parentType = (parent != null) ? parent.GetAttributeIdentifier("prefabtype", Identifier.Empty) : Identifier.Empty;
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", "");
			Identifier fallbackNameIdentifier = element.GetAttributeIdentifier("fallbacknameidentifier", "");
			string[] array = new string[2];
			int num = 0;
			string text;
			if (!nameIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameIdentifier);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				text = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			array[num] = text;
			int num2 = 1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("EntityName.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(fallbackNameIdentifier);
			array[num2] = defaultInterpolatedStringHandler3.ToStringAndClear();
			this.Name = TextManager.Get(array);
			if (parentType == "wrecked")
			{
				this.Name = TextManager.GetWithVariable("wreckeditemformat", "[name]", this.Name, FormatCapitals.No);
			}
			if (!string.IsNullOrEmpty(this.OriginalName))
			{
				this.Name = this.Name.Fallback(this.OriginalName, true);
			}
			HashSet<Identifier> tags = new HashSet<Identifier>();
			string joinedTags = element.GetAttributeString("tags", "");
			if (string.IsNullOrEmpty(joinedTags))
			{
				joinedTags = element.GetAttributeString("Tags", "");
			}
			foreach (string tag in joinedTags.Split(',', StringSplitOptions.None))
			{
				tags.Add(tag.Trim().ToIdentifier());
			}
			if (element.GetAttribute("ishorizontal") != null)
			{
				this.IsHorizontal = new bool?(element.GetAttributeBool("ishorizontal", false));
			}
			List<DecorativeSprite> decorativeSprites = new List<DecorativeSprite>();
			Dictionary<int, List<DecorativeSprite>> decorativeSpriteGroups = new Dictionary<int, List<DecorativeSprite>>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "backgroundsprite"))
					{
						if (a == "decorativesprite")
						{
							string decorativeSpriteFolder = "";
							if (subElement.DoesAttributeReferenceFileNameAlone("texture"))
							{
								decorativeSpriteFolder = Path.GetDirectoryName(file.Path);
							}
							DecorativeSprite decorativeSprite = null;
							int groupID;
							if (subElement.GetAttribute("texture") == null)
							{
								groupID = subElement.GetAttributeInt("randomgroupid", 0);
							}
							else
							{
								decorativeSprite = new DecorativeSprite(subElement, decorativeSpriteFolder, "", true);
								decorativeSprites.Add(decorativeSprite);
								groupID = decorativeSprite.RandomGroupID;
							}
							if (!decorativeSpriteGroups.ContainsKey(groupID))
							{
								decorativeSpriteGroups.Add(groupID, new List<DecorativeSprite>());
							}
							decorativeSpriteGroups[groupID].Add(decorativeSprite);
						}
					}
					else
					{
						this.BackgroundSprite = new Sprite(subElement, "", "", true, 1f);
						if (subElement.GetAttribute("sourcerect") == null && this.Sprite != null)
						{
							this.BackgroundSprite.SourceRect = this.Sprite.SourceRect;
							this.BackgroundSprite.size = this.Sprite.size;
							Sprite backgroundSprite = this.BackgroundSprite;
							backgroundSprite.size.X = backgroundSprite.size.X * (float)this.Sprite.SourceRect.Width;
							Sprite backgroundSprite2 = this.BackgroundSprite;
							backgroundSprite2.size.Y = backgroundSprite2.size.Y * (float)this.Sprite.SourceRect.Height;
							Sprite backgroundSprite3 = this.BackgroundSprite;
							ContentXElement contentXElement = subElement;
							string key = "origin";
							Vector2 vector = new Vector2(0.5f, 0.5f);
							backgroundSprite3.RelativeOrigin = contentXElement.GetAttributeVector2(key, vector);
						}
						if (subElement.GetAttributeBool("fliphorizontal", false))
						{
							this.BackgroundSprite.effects = SpriteEffects.FlipHorizontally;
						}
						if (subElement.GetAttributeBool("flipvertical", false))
						{
							this.BackgroundSprite.effects = SpriteEffects.FlipVertically;
						}
						ContentXElement contentXElement2 = subElement;
						string key2 = "color";
						Color white = Color.White;
						this.BackgroundSpriteColor = contentXElement2.GetAttributeColor(key2, white);
					}
				}
				else
				{
					this.Sprite = new Sprite(subElement, "", "", true, 1f);
					if (subElement.GetAttribute("sourcerect") == null && subElement.GetAttribute("sheetindex") == null)
					{
						DebugConsole.ThrowErrorLocalized("Warning - sprite sourcerect not configured for structure \"" + this.Name + "\"!", null, null, false, false);
					}
					if (subElement.GetAttributeBool("fliphorizontal", false))
					{
						this.Sprite.effects = SpriteEffects.FlipHorizontally;
					}
					if (subElement.GetAttributeBool("flipvertical", false))
					{
						this.Sprite.effects = SpriteEffects.FlipVertically;
					}
					this.CanSpriteFlipX = subElement.GetAttributeBool("canflipx", true);
					this.CanSpriteFlipY = subElement.GetAttributeBool("canflipy", true);
					if (subElement.GetAttribute("name") == null && !this.Name.IsNullOrWhiteSpace())
					{
						this.Sprite.Name = this.Name.Value;
					}
					this.Sprite.EntityIdentifier = this.Identifier;
				}
			}
			this.DecorativeSprites = decorativeSprites.ToImmutableArray<DecorativeSprite>();
			this.DecorativeSpriteGroups = (from kvp in decorativeSpriteGroups
			select new ValueTuple<int, ImmutableArray<DecorativeSprite>>(kvp.Key, kvp.Value.ToImmutableArray<DecorativeSprite>())).ToImmutableDictionary<int, ImmutableArray<DecorativeSprite>>();
			string categoryStr = element.GetAttributeString("category", "Structure");
			MapEntityCategory category;
			if (!Enum.TryParse<MapEntityCategory>(categoryStr, true, out category))
			{
				category = MapEntityCategory.Structure;
			}
			this.Category = category;
			this.Aliases = (element.GetAttributeStringArray("aliases", null, true) ?? element.GetAttributeStringArray("Aliases", Array.Empty<string>(), true)).ToImmutableHashSet<string>();
			string nonTranslatedName = element.GetAttributeString("name", null) ?? element.Name.ToString();
			this.Aliases.Add(nonTranslatedName.ToLowerInvariant());
			SerializableProperty.DeserializeProperties(this, element);
			if (this.Body)
			{
				tags.Add("wall".ToIdentifier());
			}
			base.LoadDescription(element);
			if (element.GetAttribute("size") == null)
			{
				this.Size = Vector2.Zero;
				if (element.GetAttribute("width") == null && element.GetAttribute("height") == null)
				{
					this.Size = this.Sprite.SourceRect.Size.ToVector2();
				}
				else
				{
					this.Size = new Vector2(element.GetAttributeFloat("width", 0f), element.GetAttributeFloat("height", 0f));
				}
			}
			if (categoryStr.Equals("Thalamus", StringComparison.OrdinalIgnoreCase))
			{
				this.Category = 4096;
				base.Subcategory = "Thalamus";
			}
			if (this.Identifier == Identifier.Empty)
			{
				DebugConsole.ThrowError("Structure prefab \"" + this.Name.Value + "\" has no identifier. All structure prefabs have a unique identifier string that's used to differentiate between items during saving and loading.", null, base.ContentPackage, false, false);
			}
			this.Tags = tags.ToImmutableHashSet<Identifier>();
			this.AllowedLinks = ImmutableHashSet<Identifier>.Empty;
		}

		// Token: 0x06002151 RID: 8529 RVA: 0x0014DA8C File Offset: 0x0014BC8C
		protected override void CreateInstance(Rectangle rect)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002152 RID: 8530 RVA: 0x0014DA93 File Offset: 0x0014BC93
		public override void Dispose()
		{
		}

		// Token: 0x040010C8 RID: 4296
		public readonly Color BackgroundSpriteColor;

		// Token: 0x040010C9 RID: 4297
		public readonly ImmutableArray<DecorativeSprite> DecorativeSprites;

		// Token: 0x040010CA RID: 4298
		public readonly ImmutableDictionary<int, ImmutableArray<DecorativeSprite>> DecorativeSpriteGroups;

		// Token: 0x040010CB RID: 4299
		public static readonly PrefabCollection<StructurePrefab> Prefabs = new PrefabCollection<StructurePrefab>();

		// Token: 0x040010CD RID: 4301
		public readonly ContentXElement ConfigElement;

		// Token: 0x040010D0 RID: 4304
		public readonly bool? IsHorizontal;

		// Token: 0x040010D1 RID: 4305
		public readonly Sprite BackgroundSprite;

		// Token: 0x040010E1 RID: 4321
		private float health;

		// Token: 0x040010EA RID: 4330
		protected Vector2 textureScale = Vector2.One;
	}
}
