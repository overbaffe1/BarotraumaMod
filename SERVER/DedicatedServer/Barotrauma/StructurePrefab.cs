using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200025B RID: 603
	internal class StructurePrefab : MapEntityPrefab
	{
		// Token: 0x17000CE3 RID: 3299
		// (get) Token: 0x06002B4B RID: 11083 RVA: 0x0011C44F File Offset: 0x0011A64F
		public override LocalizedString Name { get; }

		// Token: 0x17000CE4 RID: 3300
		// (get) Token: 0x06002B4C RID: 11084 RVA: 0x0011C457 File Offset: 0x0011A657
		public override bool CanSpriteFlipX { get; }

		// Token: 0x17000CE5 RID: 3301
		// (get) Token: 0x06002B4D RID: 11085 RVA: 0x0011C45F File Offset: 0x0011A65F
		public override bool CanSpriteFlipY { get; }

		// Token: 0x17000CE6 RID: 3302
		// (get) Token: 0x06002B4E RID: 11086 RVA: 0x0011C467 File Offset: 0x0011A667
		public Vector2 ScaledSize
		{
			get
			{
				return this.Size * base.Scale;
			}
		}

		// Token: 0x17000CE7 RID: 3303
		// (get) Token: 0x06002B4F RID: 11087 RVA: 0x0011C47A File Offset: 0x0011A67A
		public override Sprite Sprite { get; }

		// Token: 0x17000CE8 RID: 3304
		// (get) Token: 0x06002B50 RID: 11088 RVA: 0x0011C482 File Offset: 0x0011A682
		public override string OriginalName { get; }

		// Token: 0x17000CE9 RID: 3305
		// (get) Token: 0x06002B51 RID: 11089 RVA: 0x0011C48A File Offset: 0x0011A68A
		public override ImmutableHashSet<Identifier> Tags { get; }

		// Token: 0x17000CEA RID: 3306
		// (get) Token: 0x06002B52 RID: 11090 RVA: 0x0011C492 File Offset: 0x0011A692
		public override ImmutableHashSet<Identifier> AllowedLinks { get; }

		// Token: 0x17000CEB RID: 3307
		// (get) Token: 0x06002B53 RID: 11091 RVA: 0x0011C49A File Offset: 0x0011A69A
		public override MapEntityCategory Category { get; }

		// Token: 0x17000CEC RID: 3308
		// (get) Token: 0x06002B54 RID: 11092 RVA: 0x0011C4A2 File Offset: 0x0011A6A2
		public override ImmutableHashSet<string> Aliases { get; }

		// Token: 0x17000CED RID: 3309
		// (get) Token: 0x06002B55 RID: 11093 RVA: 0x0011C4AA File Offset: 0x0011A6AA
		// (set) Token: 0x06002B56 RID: 11094 RVA: 0x0011C4B2 File Offset: 0x0011A6B2
		[Serialize(false, IsPropertySaveable.No, "Does the structure have a physics body?", "", false)]
		public bool Body { get; private set; }

		// Token: 0x17000CEE RID: 3310
		// (get) Token: 0x06002B57 RID: 11095 RVA: 0x0011C4BB File Offset: 0x0011A6BB
		// (set) Token: 0x06002B58 RID: 11096 RVA: 0x0011C4C3 File Offset: 0x0011A6C3
		[Serialize(0f, IsPropertySaveable.No, "Rotation of the physics body in degrees.", "", false)]
		public float BodyRotation { get; private set; }

		// Token: 0x17000CEF RID: 3311
		// (get) Token: 0x06002B59 RID: 11097 RVA: 0x0011C4CC File Offset: 0x0011A6CC
		// (set) Token: 0x06002B5A RID: 11098 RVA: 0x0011C4D4 File Offset: 0x0011A6D4
		[Serialize(0f, IsPropertySaveable.No, "Width of the physics body in pixels.", "", false)]
		public float BodyWidth { get; private set; }

		// Token: 0x17000CF0 RID: 3312
		// (get) Token: 0x06002B5B RID: 11099 RVA: 0x0011C4DD File Offset: 0x0011A6DD
		// (set) Token: 0x06002B5C RID: 11100 RVA: 0x0011C4E5 File Offset: 0x0011A6E5
		[Serialize(0f, IsPropertySaveable.No, "Height of the physics body in pixels.", "", false)]
		public float BodyHeight { get; private set; }

		// Token: 0x17000CF1 RID: 3313
		// (get) Token: 0x06002B5D RID: 11101 RVA: 0x0011C4EE File Offset: 0x0011A6EE
		// (set) Token: 0x06002B5E RID: 11102 RVA: 0x0011C4F6 File Offset: 0x0011A6F6
		[Serialize("0.0,0.0", IsPropertySaveable.No, "Offset of the physics body from the center of the structure in pixels.", "", false)]
		public Vector2 BodyOffset { get; private set; }

		// Token: 0x17000CF2 RID: 3314
		// (get) Token: 0x06002B5F RID: 11103 RVA: 0x0011C4FF File Offset: 0x0011A6FF
		// (set) Token: 0x06002B60 RID: 11104 RVA: 0x0011C507 File Offset: 0x0011A707
		[Serialize(false, IsPropertySaveable.No, "Is the structure a platform (i.e. a \"floor\" the players can pass through)? Only relevant if the structure has a physics body.", "", false)]
		public bool Platform { get; private set; }

		// Token: 0x17000CF3 RID: 3315
		// (get) Token: 0x06002B61 RID: 11105 RVA: 0x0011C510 File Offset: 0x0011A710
		// (set) Token: 0x06002B62 RID: 11106 RVA: 0x0011C518 File Offset: 0x0011A718
		[Serialize(false, IsPropertySaveable.No, "Can items like signal components be attached on this structure? Should be enabled on structures like decorative background walls.", "", false)]
		public bool AllowAttachItems { get; private set; }

		// Token: 0x17000CF4 RID: 3316
		// (get) Token: 0x06002B63 RID: 11107 RVA: 0x0011C521 File Offset: 0x0011A721
		// (set) Token: 0x06002B64 RID: 11108 RVA: 0x0011C529 File Offset: 0x0011A729
		[Serialize(true, IsPropertySaveable.No, "Can the structure be rotated in the submarine editor?", "", false)]
		public bool AllowRotatingInEditor { get; set; }

		// Token: 0x17000CF5 RID: 3317
		// (get) Token: 0x06002B65 RID: 11109 RVA: 0x0011C532 File Offset: 0x0011A732
		// (set) Token: 0x06002B66 RID: 11110 RVA: 0x0011C53A File Offset: 0x0011A73A
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float MinHealth { get; private set; }

		// Token: 0x17000CF6 RID: 3318
		// (get) Token: 0x06002B67 RID: 11111 RVA: 0x0011C543 File Offset: 0x0011A743
		// (set) Token: 0x06002B68 RID: 11112 RVA: 0x0011C54B File Offset: 0x0011A74B
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

		// Token: 0x17000CF7 RID: 3319
		// (get) Token: 0x06002B69 RID: 11113 RVA: 0x0011C55F File Offset: 0x0011A75F
		// (set) Token: 0x06002B6A RID: 11114 RVA: 0x0011C567 File Offset: 0x0011A767
		[Serialize(true, IsPropertySaveable.No, "Should the structure be indestructible when used in an outpost?", "", false)]
		public bool IndestructibleInOutposts { get; private set; }

		// Token: 0x17000CF8 RID: 3320
		// (get) Token: 0x06002B6B RID: 11115 RVA: 0x0011C570 File Offset: 0x0011A770
		// (set) Token: 0x06002B6C RID: 11116 RVA: 0x0011C578 File Offset: 0x0011A778
		[Serialize(false, IsPropertySaveable.No, "Should the structure cast shadows and obstruct visibility when LOS is enabled?", "", false)]
		public bool CastShadow { get; private set; }

		// Token: 0x17000CF9 RID: 3321
		// (get) Token: 0x06002B6D RID: 11117 RVA: 0x0011C581 File Offset: 0x0011A781
		// (set) Token: 0x06002B6E RID: 11118 RVA: 0x0011C589 File Offset: 0x0011A789
		[Serialize(Direction.None, IsPropertySaveable.No, "Makes the structure function as a staircase.", "", false)]
		public Direction StairDirection { get; private set; }

		// Token: 0x17000CFA RID: 3322
		// (get) Token: 0x06002B6F RID: 11119 RVA: 0x0011C592 File Offset: 0x0011A792
		// (set) Token: 0x06002B70 RID: 11120 RVA: 0x0011C59A File Offset: 0x0011A79A
		[Serialize(45f, IsPropertySaveable.No, "Angle of the stairs in degrees. Only relevant if StairDirection is something else than None.", "", false)]
		public float StairAngle { get; private set; }

		// Token: 0x17000CFB RID: 3323
		// (get) Token: 0x06002B71 RID: 11121 RVA: 0x0011C5A3 File Offset: 0x0011A7A3
		// (set) Token: 0x06002B72 RID: 11122 RVA: 0x0011C5AB File Offset: 0x0011A7AB
		[Serialize(false, IsPropertySaveable.No, "If enabled, monsters will not be able to target this structure.", "", false)]
		public bool NoAITarget { get; private set; }

		// Token: 0x17000CFC RID: 3324
		// (get) Token: 0x06002B73 RID: 11123 RVA: 0x0011C5B4 File Offset: 0x0011A7B4
		// (set) Token: 0x06002B74 RID: 11124 RVA: 0x0011C5BC File Offset: 0x0011A7BC
		[Serialize("0,0", IsPropertySaveable.Yes, "Size of the structure in pixels. If not set, the size is determined, based on the attributes width and height, and if those aren't defined either, based on the size of the structure's sprite.", "", false)]
		public Vector2 Size { get; private set; }

		// Token: 0x17000CFD RID: 3325
		// (get) Token: 0x06002B75 RID: 11125 RVA: 0x0011C5C5 File Offset: 0x0011A7C5
		// (set) Token: 0x06002B76 RID: 11126 RVA: 0x0011C5CD File Offset: 0x0011A7CD
		[Serialize("", IsPropertySaveable.Yes, "Tag of the sound that plays when something damages the wall.", "", false)]
		public string DamageSound { get; private set; }

		// Token: 0x17000CFE RID: 3326
		// (get) Token: 0x06002B77 RID: 11127 RVA: 0x0011C5D6 File Offset: 0x0011A7D6
		// (set) Token: 0x06002B78 RID: 11128 RVA: 0x0011C5DE File Offset: 0x0011A7DE
		[Serialize("shrapnel", IsPropertySaveable.Yes, "Identifier of the particles emitted when something damages the wall.", "", false)]
		public string DamageParticle { get; private set; }

		// Token: 0x17000CFF RID: 3327
		// (get) Token: 0x06002B79 RID: 11129 RVA: 0x0011C5E7 File Offset: 0x0011A7E7
		// (set) Token: 0x06002B7A RID: 11130 RVA: 0x0011C5EF File Offset: 0x0011A7EF
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

		// Token: 0x06002B7B RID: 11131 RVA: 0x0011C628 File Offset: 0x0011A828
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

		// Token: 0x06002B7C RID: 11132 RVA: 0x0011C6B4 File Offset: 0x0011A8B4
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
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "backgroundsprite"))
					{
						if (!(a == "decorativesprite"))
						{
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
					}
				}
				else
				{
					this.Sprite = new Sprite(subElement, "", "", true, 1f);
					if (subElement.GetAttribute("sourcerect") == null && subElement.GetAttribute("sheetindex") == null)
					{
						DebugConsole.ThrowErrorLocalized("Warning - sprite sourcerect not configured for structure \"" + this.Name + "\"!", null, null, false, false);
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

		// Token: 0x06002B7D RID: 11133 RVA: 0x0011CCA0 File Offset: 0x0011AEA0
		protected override void CreateInstance(Rectangle rect)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002B7E RID: 11134 RVA: 0x0011CCA7 File Offset: 0x0011AEA7
		public override void Dispose()
		{
		}

		// Token: 0x04001534 RID: 5428
		public static readonly PrefabCollection<StructurePrefab> Prefabs = new PrefabCollection<StructurePrefab>();

		// Token: 0x04001536 RID: 5430
		public readonly ContentXElement ConfigElement;

		// Token: 0x04001539 RID: 5433
		public readonly bool? IsHorizontal;

		// Token: 0x0400153A RID: 5434
		public readonly Sprite BackgroundSprite;

		// Token: 0x0400154A RID: 5450
		private float health;

		// Token: 0x04001553 RID: 5459
		protected Vector2 textureScale = Vector2.One;
	}
}
