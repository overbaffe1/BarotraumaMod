using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200023E RID: 574
	internal class LevelObjectPrefab : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17000BA4 RID: 2980
		// (get) Token: 0x060027EB RID: 10219 RVA: 0x00103F8E File Offset: 0x0010218E
		// (set) Token: 0x060027EC RID: 10220 RVA: 0x00103F96 File Offset: 0x00102196
		public List<Sprite> Sprites { get; private set; } = new List<Sprite>();

		// Token: 0x17000BA5 RID: 2981
		// (get) Token: 0x060027ED RID: 10221 RVA: 0x00103F9F File Offset: 0x0010219F
		// (set) Token: 0x060027EE RID: 10222 RVA: 0x00103FA7 File Offset: 0x001021A7
		public DeformableSprite DeformableSprite { get; private set; }

		// Token: 0x17000BA6 RID: 2982
		// (get) Token: 0x060027EF RID: 10223 RVA: 0x00103FB0 File Offset: 0x001021B0
		// (set) Token: 0x060027F0 RID: 10224 RVA: 0x00103FB8 File Offset: 0x001021B8
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public float MinSize { get; private set; }

		// Token: 0x17000BA7 RID: 2983
		// (get) Token: 0x060027F1 RID: 10225 RVA: 0x00103FC1 File Offset: 0x001021C1
		// (set) Token: 0x060027F2 RID: 10226 RVA: 0x00103FC9 File Offset: 0x001021C9
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public float MaxSize { get; private set; }

		// Token: 0x17000BA8 RID: 2984
		// (get) Token: 0x060027F3 RID: 10227 RVA: 0x00103FD2 File Offset: 0x001021D2
		// (set) Token: 0x060027F4 RID: 10228 RVA: 0x00103FDA File Offset: 0x001021DA
		[Serialize(Alignment.Left | Alignment.Right | Alignment.Top | Alignment.Bottom, IsPropertySaveable.Yes, "Which sides of a wall the object can spawn on.", "", false)]
		[Editable]
		public Alignment Alignment { get; private set; }

		// Token: 0x17000BA9 RID: 2985
		// (get) Token: 0x060027F5 RID: 10229 RVA: 0x00103FE3 File Offset: 0x001021E3
		// (set) Token: 0x060027F6 RID: 10230 RVA: 0x00103FEB File Offset: 0x001021EB
		[Serialize(LevelObjectPrefab.SpawnPosType.Wall, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public LevelObjectPrefab.SpawnPosType SpawnPos { get; private set; }

		// Token: 0x17000BAA RID: 2986
		// (get) Token: 0x060027F7 RID: 10231 RVA: 0x00103FF4 File Offset: 0x001021F4
		// (set) Token: 0x060027F8 RID: 10232 RVA: 0x00103FFC File Offset: 0x001021FC
		public XElement Config { get; private set; }

		// Token: 0x17000BAB RID: 2987
		// (get) Token: 0x060027F9 RID: 10233 RVA: 0x00104005 File Offset: 0x00102205
		// (set) Token: 0x060027FA RID: 10234 RVA: 0x0010400D File Offset: 0x0010220D
		public XElement PhysicsBodyElement { get; private set; }

		// Token: 0x17000BAC RID: 2988
		// (get) Token: 0x060027FB RID: 10235 RVA: 0x00104016 File Offset: 0x00102216
		// (set) Token: 0x060027FC RID: 10236 RVA: 0x0010401E File Offset: 0x0010221E
		public int PhysicsBodyTriggerIndex { get; private set; } = -1;

		// Token: 0x17000BAD RID: 2989
		// (get) Token: 0x060027FD RID: 10237 RVA: 0x00104027 File Offset: 0x00102227
		// (set) Token: 0x060027FE RID: 10238 RVA: 0x0010402F File Offset: 0x0010222F
		public Dictionary<Sprite, XElement> SpriteSpecificPhysicsBodyElements { get; private set; } = new Dictionary<Sprite, XElement>();

		// Token: 0x17000BAE RID: 2990
		// (get) Token: 0x060027FF RID: 10239 RVA: 0x00104038 File Offset: 0x00102238
		// (set) Token: 0x06002800 RID: 10240 RVA: 0x00104040 File Offset: 0x00102240
		[Serialize(10000, IsPropertySaveable.No, "Maximum number of this specific object per level.", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public int MaxCount { get; private set; }

		// Token: 0x17000BAF RID: 2991
		// (get) Token: 0x06002801 RID: 10241 RVA: 0x00104049 File Offset: 0x00102249
		// (set) Token: 0x06002802 RID: 10242 RVA: 0x00104051 File Offset: 0x00102251
		[Serialize("0.0,1.0", IsPropertySaveable.Yes, "The sprite depth of the object (min, max). Values of 0 or less make the object render in front of walls, values larger than 0 make it render behind walls with a parallax effect.", "", false)]
		[Editable]
		public Vector2 DepthRange { get; private set; }

		// Token: 0x17000BB0 RID: 2992
		// (get) Token: 0x06002803 RID: 10243 RVA: 0x0010405A File Offset: 0x0010225A
		// (set) Token: 0x06002804 RID: 10244 RVA: 0x00104062 File Offset: 0x00102262
		[Serialize(3000f, IsPropertySaveable.Yes, "Objects fade out to the background color of the level the further they are from the camera. This value is the depth at which the object becomes \"maximally\" faded out.", "", false)]
		[Editable]
		public float FadeOutDepth { get; private set; }

		// Token: 0x17000BB1 RID: 2993
		// (get) Token: 0x06002805 RID: 10245 RVA: 0x0010406B File Offset: 0x0010226B
		// (set) Token: 0x06002806 RID: 10246 RVA: 0x00104073 File Offset: 0x00102273
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		[Serialize(0f, IsPropertySaveable.Yes, "The tendency for the prefab to form clusters. Used as an exponent for perlin noise values that are used to determine the probability for an object to spawn at a specific position.", "", false)]
		public float ClusteringAmount { get; private set; }

		// Token: 0x17000BB2 RID: 2994
		// (get) Token: 0x06002807 RID: 10247 RVA: 0x0010407C File Offset: 0x0010227C
		// (set) Token: 0x06002808 RID: 10248 RVA: 0x00104084 File Offset: 0x00102284
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		[Serialize(0f, IsPropertySaveable.Yes, "A value between 0-1 that determines the z-coordinate to sample perlin noise from when determining the probability  for an object to spawn at a specific position. Using the same (or close) value for different objects means the objects tend to form clusters in the same areas.", "", false)]
		public float ClusteringGroup { get; private set; }

		// Token: 0x17000BB3 RID: 2995
		// (get) Token: 0x06002809 RID: 10249 RVA: 0x0010408D File Offset: 0x0010228D
		// (set) Token: 0x0600280A RID: 10250 RVA: 0x00104095 File Offset: 0x00102295
		[Editable]
		[Serialize("0,0", IsPropertySaveable.Yes, "Random offset from the surface the object spawns on.", "", false)]
		public Vector2 RandomOffset { get; private set; }

		// Token: 0x17000BB4 RID: 2996
		// (get) Token: 0x0600280B RID: 10251 RVA: 0x0010409E File Offset: 0x0010229E
		// (set) Token: 0x0600280C RID: 10252 RVA: 0x001040A6 File Offset: 0x001022A6
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the object be rotated to align it with the wall surface it spawns on.", "", false)]
		public bool AlignWithSurface { get; private set; }

		// Token: 0x17000BB5 RID: 2997
		// (get) Token: 0x0600280D RID: 10253 RVA: 0x001040AF File Offset: 0x001022AF
		// (set) Token: 0x0600280E RID: 10254 RVA: 0x001040B7 File Offset: 0x001022B7
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the object be placed near the start of the level.", "", false)]
		public bool AllowAtStart { get; private set; }

		// Token: 0x17000BB6 RID: 2998
		// (get) Token: 0x0600280F RID: 10255 RVA: 0x001040C0 File Offset: 0x001022C0
		// (set) Token: 0x06002810 RID: 10256 RVA: 0x001040C8 File Offset: 0x001022C8
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the object be placed near the end of the level.", "", false)]
		public bool AllowAtEnd { get; private set; }

		// Token: 0x17000BB7 RID: 2999
		// (get) Token: 0x06002811 RID: 10257 RVA: 0x001040D1 File Offset: 0x001022D1
		// (set) Token: 0x06002812 RID: 10258 RVA: 0x001040D9 File Offset: 0x001022D9
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum length of a graph edge the object can spawn on.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float MinSurfaceWidth { get; private set; }

		// Token: 0x17000BB8 RID: 3000
		// (get) Token: 0x06002813 RID: 10259 RVA: 0x001040E2 File Offset: 0x001022E2
		// (set) Token: 0x06002814 RID: 10260 RVA: 0x00104109 File Offset: 0x00102309
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "How much the rotation of the object can vary (min and max values in degrees).", "", false)]
		public Vector2 RandomRotation
		{
			get
			{
				return new Vector2(MathHelper.ToDegrees(this.randomRotation.X), MathHelper.ToDegrees(this.randomRotation.Y));
			}
			private set
			{
				this.randomRotation = new Vector2(MathHelper.ToRadians(value.X), MathHelper.ToRadians(value.Y));
			}
		}

		// Token: 0x17000BB9 RID: 3001
		// (get) Token: 0x06002815 RID: 10261 RVA: 0x0010412C File Offset: 0x0010232C
		public Vector2 RandomRotationRad
		{
			get
			{
				return this.randomRotation;
			}
		}

		// Token: 0x17000BBA RID: 3002
		// (get) Token: 0x06002816 RID: 10262 RVA: 0x00104134 File Offset: 0x00102334
		// (set) Token: 0x06002817 RID: 10263 RVA: 0x00104141 File Offset: 0x00102341
		[Serialize(0f, IsPropertySaveable.Yes, "How much the object swings (in degrees).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 360f)]
		public float SwingAmount
		{
			get
			{
				return MathHelper.ToDegrees(this.swingAmount);
			}
			private set
			{
				this.swingAmount = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000BBB RID: 3003
		// (get) Token: 0x06002818 RID: 10264 RVA: 0x0010414F File Offset: 0x0010234F
		public float SwingAmountRad
		{
			get
			{
				return this.swingAmount;
			}
		}

		// Token: 0x17000BBC RID: 3004
		// (get) Token: 0x06002819 RID: 10265 RVA: 0x00104157 File Offset: 0x00102357
		// (set) Token: 0x0600281A RID: 10266 RVA: 0x0010415F File Offset: 0x0010235F
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the object swings.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SwingFrequency { get; private set; }

		// Token: 0x17000BBD RID: 3005
		// (get) Token: 0x0600281B RID: 10267 RVA: 0x00104168 File Offset: 0x00102368
		// (set) Token: 0x0600281C RID: 10268 RVA: 0x00104170 File Offset: 0x00102370
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "How much the scale of the object oscillates on each axis. A value of 0.5,0.5 would make the object's scale oscillate from 100% to 150%.", "", false)]
		public Vector2 ScaleOscillation { get; private set; }

		// Token: 0x17000BBE RID: 3006
		// (get) Token: 0x0600281D RID: 10269 RVA: 0x00104179 File Offset: 0x00102379
		// (set) Token: 0x0600281E RID: 10270 RVA: 0x00104181 File Offset: 0x00102381
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the object's scale oscillates.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float ScaleOscillationFrequency { get; private set; }

		// Token: 0x17000BBF RID: 3007
		// (get) Token: 0x0600281F RID: 10271 RVA: 0x0010418A File Offset: 0x0010238A
		// (set) Token: 0x06002820 RID: 10272 RVA: 0x00104192 File Offset: 0x00102392
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "How likely it is for the object to spawn in a level. This is relative to the commonness of the other objects - for example, having an object with a commonness of 1 and another with a commonness of 10 would mean the latter appears in levels 10 times as frequently as the former. The commonness value can be overridden on specific level types.", "", false)]
		public float Commonness { get; private set; }

		// Token: 0x17000BC0 RID: 3008
		// (get) Token: 0x06002821 RID: 10273 RVA: 0x0010419B File Offset: 0x0010239B
		// (set) Token: 0x06002822 RID: 10274 RVA: 0x001041A3 File Offset: 0x001023A3
		[Serialize(0f, IsPropertySaveable.Yes, "How much the object disrupts submarine's sonar.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SonarDisruption { get; private set; }

		// Token: 0x17000BC1 RID: 3009
		// (get) Token: 0x06002823 RID: 10275 RVA: 0x001041AC File Offset: 0x001023AC
		// (set) Token: 0x06002824 RID: 10276 RVA: 0x001041B4 File Offset: 0x001023B4
		[Serialize(false, IsPropertySaveable.Yes, "Can the object take damage from weapons/attacks that damage level walls.", "", false)]
		[Editable]
		public bool TakeLevelWallDamage { get; private set; }

		// Token: 0x17000BC2 RID: 3010
		// (get) Token: 0x06002825 RID: 10277 RVA: 0x001041BD File Offset: 0x001023BD
		// (set) Token: 0x06002826 RID: 10278 RVA: 0x001041C5 File Offset: 0x001023C5
		[Serialize(false, IsPropertySaveable.Yes, "Should the object disappear if the object is destroyed? Only relevant if TakeLevelWallDamage is true.", "", false)]
		[Editable]
		public bool HideWhenBroken { get; private set; }

		// Token: 0x17000BC3 RID: 3011
		// (get) Token: 0x06002827 RID: 10279 RVA: 0x001041CE File Offset: 0x001023CE
		// (set) Token: 0x06002828 RID: 10280 RVA: 0x001041D6 File Offset: 0x001023D6
		[Serialize(100f, IsPropertySaveable.Yes, "Amount of health the object has. Only relevant if TakeLevelWallDamage is true.", "", false)]
		[Editable]
		public float Health { get; private set; }

		// Token: 0x17000BC4 RID: 3012
		// (get) Token: 0x06002829 RID: 10281 RVA: 0x001041DF File Offset: 0x001023DF
		// (set) Token: 0x0600282A RID: 10282 RVA: 0x001041E7 File Offset: 0x001023E7
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color SpriteColor { get; private set; }

		// Token: 0x17000BC5 RID: 3013
		// (get) Token: 0x0600282B RID: 10283 RVA: 0x001041F0 File Offset: 0x001023F0
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000BC6 RID: 3014
		// (get) Token: 0x0600282C RID: 10284 RVA: 0x001041FD File Offset: 0x001023FD
		// (set) Token: 0x0600282D RID: 10285 RVA: 0x00104205 File Offset: 0x00102405
		public List<LevelObjectPrefab.ChildObject> ChildObjects { get; private set; }

		// Token: 0x17000BC7 RID: 3015
		// (get) Token: 0x0600282E RID: 10286 RVA: 0x0010420E File Offset: 0x0010240E
		// (set) Token: 0x0600282F RID: 10287 RVA: 0x00104216 File Offset: 0x00102416
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000BC8 RID: 3016
		// (get) Token: 0x06002830 RID: 10288 RVA: 0x0010421F File Offset: 0x0010241F
		// (set) Token: 0x06002831 RID: 10289 RVA: 0x00104227 File Offset: 0x00102427
		public List<LevelObjectPrefab> OverrideProperties { get; private set; }

		// Token: 0x06002832 RID: 10290 RVA: 0x00104230 File Offset: 0x00102430
		public override string ToString()
		{
			return "LevelObjectPrefab (" + this.Identifier.ToString() + ")";
		}

		// Token: 0x06002833 RID: 10291 RVA: 0x00104254 File Offset: 0x00102454
		public LevelObjectPrefab(ContentXElement element, LevelObjectPrefabsFile file, Identifier identifierOverride = default(Identifier)) : base(file, LevelObjectPrefab.ParseIdentifier(identifierOverride, element))
		{
			this.ChildObjects = new List<LevelObjectPrefab.ChildObject>();
			this.LevelTriggerElements = new List<ContentXElement>();
			this.OverrideProperties = new List<LevelObjectPrefab>();
			this.OverrideCommonness = new Dictionary<Identifier, float>();
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			ContentXElement contentXElement = null;
			if (element != contentXElement)
			{
				this.Config = element;
				this.LoadElements(file, element, -1);
			}
			contentXElement = null;
			if (element != contentXElement && element.GetAttribute("minsurfacewidth") == null)
			{
				if (this.Sprites.Any<Sprite>())
				{
					this.MinSurfaceWidth = this.Sprites[0].size.X * this.MaxSize * 0.8f;
				}
				if (this.DeformableSprite != null)
				{
					this.MinSurfaceWidth = Math.Max(this.MinSurfaceWidth, this.DeformableSprite.Size.X * this.MaxSize * 0.8f);
				}
			}
		}

		// Token: 0x06002834 RID: 10292 RVA: 0x00104378 File Offset: 0x00102578
		public static Identifier ParseIdentifier(Identifier identifierOverride, XElement element)
		{
			if (!identifierOverride.IsEmpty)
			{
				return identifierOverride;
			}
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			if (identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Level object prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(element.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" has no identifier! Using the name as the identifier instead...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				identifier = element.NameAsIdentifier();
			}
			return identifier;
		}

		// Token: 0x06002835 RID: 10293 RVA: 0x001043F0 File Offset: 0x001025F0
		private void LoadElements(LevelObjectPrefabsFile file, ContentXElement element, int parentTriggerIndex)
		{
			int propertyOverrideCount = 0;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (a == "deformablesprite")
					{
						this.DeformableSprite = new DeformableSprite(subElement, null, null, "", true, false, 1f);
					}
				}
				else
				{
					Sprite newSprite = new Sprite(subElement, "", "", true, 1f);
					this.Sprites.Add(newSprite);
					ContentXElement contentXElement;
					if ((contentXElement = subElement.GetChildElement("PhysicsBody")) == null && (contentXElement = subElement.GetChildElement("Body")) == null)
					{
						contentXElement = (subElement.GetChildElement("physicsbody") ?? subElement.GetChildElement("body"));
					}
					ContentXElement spriteSpecificPhysicsBodyElement = contentXElement;
					ContentXElement contentXElement2 = null;
					if (spriteSpecificPhysicsBodyElement != contentXElement2)
					{
						this.SpriteSpecificPhysicsBodyElements.Add(newSprite, spriteSpecificPhysicsBodyElement);
					}
				}
			}
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string text = subElement2.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length <= 7)
					{
						if (length != 4)
						{
							if (length != 7)
							{
								continue;
							}
							if (!(text == "trigger"))
							{
								continue;
							}
						}
						else
						{
							if (!(text == "body"))
							{
								continue;
							}
							goto IL_379;
						}
					}
					else if (length != 11)
					{
						if (length != 12)
						{
							if (length != 18)
							{
								continue;
							}
							char c = text[8];
							if (c != 'c')
							{
								if (c != 'p')
								{
									continue;
								}
								if (!(text == "overrideproperties"))
								{
									continue;
								}
								ContentXElement element2 = subElement2;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral("-");
								defaultInterpolatedStringHandler.AppendFormatted<int>(propertyOverrideCount);
								LevelObjectPrefab propertyOverride = new LevelObjectPrefab(element2, file, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
								this.OverrideProperties[this.OverrideProperties.Count - 1] = propertyOverride;
								if (!propertyOverride.Sprites.Any<Sprite>() && propertyOverride.DeformableSprite == null)
								{
									propertyOverride.Sprites = this.Sprites;
									propertyOverride.DeformableSprite = this.DeformableSprite;
								}
								propertyOverrideCount++;
								continue;
							}
							else
							{
								if (!(text == "overridecommonness"))
								{
									continue;
								}
								Identifier levelType = subElement2.GetAttributeIdentifier("leveltype", Identifier.Empty);
								if (!this.OverrideCommonness.ContainsKey(levelType))
								{
									this.OverrideCommonness.Add(levelType, subElement2.GetAttributeFloat("commonness", 1f));
									continue;
								}
								continue;
							}
						}
						else if (!(text == "leveltrigger"))
						{
							continue;
						}
					}
					else
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c != 'p')
							{
								continue;
							}
							if (!(text == "physicsbody"))
							{
								continue;
							}
							goto IL_379;
						}
						else
						{
							if (!(text == "childobject"))
							{
								continue;
							}
							this.ChildObjects.Add(new LevelObjectPrefab.ChildObject(subElement2));
							continue;
						}
					}
					this.OverrideProperties.Add(null);
					this.LevelTriggerElements.Add(subElement2);
					this.LoadElements(file, subElement2, this.LevelTriggerElements.Count - 1);
					continue;
					IL_379:
					this.PhysicsBodyElement = subElement2;
					this.PhysicsBodyTriggerIndex = parentTriggerIndex;
				}
			}
		}

		// Token: 0x06002836 RID: 10294 RVA: 0x001047D8 File Offset: 0x001029D8
		public float GetCommonness(CaveGenerationParams generationParams, bool requireCaveSpecificOverride = true)
		{
			float commonness;
			if (generationParams != null && generationParams.Identifier != Identifier.Empty && this.OverrideCommonness.TryGetValue(generationParams.Identifier, out commonness))
			{
				return commonness;
			}
			if (!requireCaveSpecificOverride)
			{
				return this.Commonness;
			}
			return 0f;
		}

		// Token: 0x06002837 RID: 10295 RVA: 0x00104820 File Offset: 0x00102A20
		public float GetCommonness(LevelData levelData)
		{
			float commonness;
			if ((levelData.GenerationParams != null && levelData.GenerationParams.Identifier != Identifier.Empty && this.OverrideCommonness.TryGetValue(levelData.GenerationParams.Identifier, out commonness)) || (!levelData.GenerationParams.OldIdentifier.IsEmpty && this.OverrideCommonness.TryGetValue(levelData.GenerationParams.OldIdentifier, out commonness)))
			{
				return commonness;
			}
			float biomeCommonness;
			if (((levelData != null) ? levelData.Biome : null) != null && this.OverrideCommonness.TryGetValue(levelData.Biome.Identifier, out biomeCommonness))
			{
				return biomeCommonness;
			}
			return this.Commonness;
		}

		// Token: 0x06002838 RID: 10296 RVA: 0x001048C7 File Offset: 0x00102AC7
		public override void Dispose()
		{
		}

		// Token: 0x04001397 RID: 5015
		public static readonly PrefabCollection<LevelObjectPrefab> Prefabs = new PrefabCollection<LevelObjectPrefab>();

		// Token: 0x0400139F RID: 5023
		public readonly List<ContentXElement> LevelTriggerElements;

		// Token: 0x040013A0 RID: 5024
		public readonly Dictionary<Identifier, float> OverrideCommonness;

		// Token: 0x040013AE RID: 5038
		private Vector2 randomRotation;

		// Token: 0x040013AF RID: 5039
		private float swingAmount;

		// Token: 0x02000A23 RID: 2595
		public class ChildObject
		{
			// Token: 0x06005C13 RID: 23571 RVA: 0x001FFBEB File Offset: 0x001FDDEB
			public ChildObject()
			{
				this.AllowedNames = new List<string>();
				this.MinCount = 1;
				this.MaxCount = 1;
			}

			// Token: 0x06005C14 RID: 23572 RVA: 0x001FFC0C File Offset: 0x001FDE0C
			public ChildObject(XElement element)
			{
				this.AllowedNames = element.GetAttributeStringArray("names", Array.Empty<string>(), true, false).ToList<string>();
				this.MinCount = element.GetAttributeInt("mincount", 1);
				this.MaxCount = Math.Max(element.GetAttributeInt("maxcount", 1), this.MinCount);
			}

			// Token: 0x04003556 RID: 13654
			public List<string> AllowedNames;

			// Token: 0x04003557 RID: 13655
			public int MinCount;

			// Token: 0x04003558 RID: 13656
			public int MaxCount;
		}

		// Token: 0x02000A24 RID: 2596
		[Flags]
		public enum SpawnPosType
		{
			// Token: 0x0400355A RID: 13658
			None = 0,
			// Token: 0x0400355B RID: 13659
			MainPathWall = 1,
			// Token: 0x0400355C RID: 13660
			SidePathWall = 2,
			// Token: 0x0400355D RID: 13661
			CaveWall = 4,
			// Token: 0x0400355E RID: 13662
			NestWall = 8,
			// Token: 0x0400355F RID: 13663
			RuinWall = 16,
			// Token: 0x04003560 RID: 13664
			SeaFloor = 32,
			// Token: 0x04003561 RID: 13665
			MainPath = 64,
			// Token: 0x04003562 RID: 13666
			LevelStart = 128,
			// Token: 0x04003563 RID: 13667
			LevelEnd = 256,
			// Token: 0x04003564 RID: 13668
			OutpostWall = 512,
			// Token: 0x04003565 RID: 13669
			Wall = 7
		}
	}
}
