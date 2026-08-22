using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Lights
{
	// Token: 0x020004D5 RID: 1237
	internal class LightSourceParams : ISerializableEntity
	{
		// Token: 0x17001478 RID: 5240
		// (get) Token: 0x06005066 RID: 20582 RVA: 0x002B4F07 File Offset: 0x002B3107
		public string Name
		{
			get
			{
				return "Light Source";
			}
		}

		// Token: 0x17001479 RID: 5241
		// (get) Token: 0x06005067 RID: 20583 RVA: 0x002B4F0E File Offset: 0x002B310E
		// (set) Token: 0x06005068 RID: 20584 RVA: 0x002B4F16 File Offset: 0x002B3116
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; } = new Dictionary<Identifier, SerializableProperty>();

		// Token: 0x1700147A RID: 5242
		// (get) Token: 0x06005069 RID: 20585 RVA: 0x002B4F1F File Offset: 0x002B311F
		// (set) Token: 0x0600506A RID: 20586 RVA: 0x002B4F27 File Offset: 0x002B3127
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", true)]
		[Editable]
		public Color Color { get; set; }

		// Token: 0x1700147B RID: 5243
		// (get) Token: 0x0600506B RID: 20587 RVA: 0x002B4F30 File Offset: 0x002B3130
		// (set) Token: 0x0600506C RID: 20588 RVA: 0x002B4F38 File Offset: 0x002B3138
		[Serialize(100f, IsPropertySaveable.Yes, "", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
		public float Range
		{
			get
			{
				return this.range;
			}
			set
			{
				this.range = MathHelper.Clamp(value, 0f, 4096f);
				this.TextureRange = this.range;
				if (this.OverrideLightTexture != null)
				{
					this.TextureRange *= 1f + Math.Max(Math.Abs(this.OverrideLightTexture.RelativeOrigin.X - 0.5f), Math.Abs(this.OverrideLightTexture.RelativeOrigin.Y - 0.5f));
				}
			}
		}

		// Token: 0x1700147C RID: 5244
		// (get) Token: 0x0600506D RID: 20589 RVA: 0x002B4FBD File Offset: 0x002B31BD
		// (set) Token: 0x0600506E RID: 20590 RVA: 0x002B4FC5 File Offset: 0x002B31C5
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.01f, 100f, 1, ValueStep = 0.1f, DecimalCount = 2)]
		public float Scale { get; set; }

		// Token: 0x1700147D RID: 5245
		// (get) Token: 0x0600506F RID: 20591 RVA: 0x002B4FCE File Offset: 0x002B31CE
		// (set) Token: 0x06005070 RID: 20592 RVA: 0x002B4FD6 File Offset: 0x002B31D6
		[Serialize("0, 0", IsPropertySaveable.Yes, "", "", false)]
		[Editable(ValueStep = 1f, DecimalCount = 1, MinValueFloat = -1000f, MaxValueFloat = 1000f)]
		public Vector2 Offset { get; set; }

		// Token: 0x1700147E RID: 5246
		// (get) Token: 0x06005071 RID: 20593 RVA: 0x002B4FDF File Offset: 0x002B31DF
		// (set) Token: 0x06005072 RID: 20594 RVA: 0x002B4FE7 File Offset: 0x002B31E7
		public float RotationRad { get; private set; }

		// Token: 0x1700147F RID: 5247
		// (get) Token: 0x06005073 RID: 20595 RVA: 0x002B4FF0 File Offset: 0x002B31F0
		// (set) Token: 0x06005074 RID: 20596 RVA: 0x002B4FFD File Offset: 0x002B31FD
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = -360f, MaxValueFloat = 360f, ValueStep = 1f, DecimalCount = 0)]
		public float Rotation
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRad);
			}
			set
			{
				this.RotationRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17001480 RID: 5248
		// (get) Token: 0x06005075 RID: 20597 RVA: 0x002B500B File Offset: 0x002B320B
		// (set) Token: 0x06005076 RID: 20598 RVA: 0x002B5013 File Offset: 0x002B3213
		[Serialize(false, IsPropertySaveable.Yes, "Directional lights only shine in \"one direction\", meaning no shadows are cast behind them. Note that this does not affect how the light texture is drawn: if you want something like a conical spotlight, you should use an appropriate texture for that.", "", false)]
		public bool Directional { get; set; }

		// Token: 0x06005077 RID: 20599 RVA: 0x002B501C File Offset: 0x002B321C
		public Vector2 GetOffset()
		{
			return Vector2.Transform(this.Offset, Matrix.CreateRotationZ(MathHelper.ToRadians(this.Rotation)));
		}

		// Token: 0x17001481 RID: 5249
		// (get) Token: 0x06005078 RID: 20600 RVA: 0x002B5039 File Offset: 0x002B3239
		// (set) Token: 0x06005079 RID: 20601 RVA: 0x002B5041 File Offset: 0x002B3241
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How heavily the light flickers. 0 = no flickering, 1 = the light will alternate between completely dark and full brightness.", "", false)]
		public float Flicker
		{
			get
			{
				return this.flicker;
			}
			set
			{
				this.flicker = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17001482 RID: 5250
		// (get) Token: 0x0600507A RID: 20602 RVA: 0x002B5059 File Offset: 0x002B3259
		// (set) Token: 0x0600507B RID: 20603 RVA: 0x002B5061 File Offset: 0x002B3261
		[Editable]
		[Serialize(1f, IsPropertySaveable.No, "How fast the light flickers.", "", false)]
		public float FlickerSpeed { get; set; }

		// Token: 0x17001483 RID: 5251
		// (get) Token: 0x0600507C RID: 20604 RVA: 0x002B506A File Offset: 0x002B326A
		// (set) Token: 0x0600507D RID: 20605 RVA: 0x002B5072 File Offset: 0x002B3272
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "How rapidly the light pulsates (in Hz). 0 = no blinking.", "", false)]
		public float PulseFrequency
		{
			get
			{
				return this.pulseFrequency;
			}
			set
			{
				this.pulseFrequency = MathHelper.Clamp(value, 0f, 60f);
			}
		}

		// Token: 0x17001484 RID: 5252
		// (get) Token: 0x0600507E RID: 20606 RVA: 0x002B508A File Offset: 0x002B328A
		// (set) Token: 0x0600507F RID: 20607 RVA: 0x002B5092 File Offset: 0x002B3292
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "How much light pulsates. 0 = not at all, 1 = alternates between full brightness and off.", "", false)]
		public float PulseAmount
		{
			get
			{
				return this.pulseAmount;
			}
			set
			{
				this.pulseAmount = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17001485 RID: 5253
		// (get) Token: 0x06005080 RID: 20608 RVA: 0x002B50AA File Offset: 0x002B32AA
		// (set) Token: 0x06005081 RID: 20609 RVA: 0x002B50B2 File Offset: 0x002B32B2
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "How rapidly the light blinks on and off (in Hz). 0 = no blinking.", "", false)]
		public float BlinkFrequency
		{
			get
			{
				return this.blinkFrequency;
			}
			set
			{
				this.blinkFrequency = MathHelper.Clamp(value, 0f, 60f);
			}
		}

		// Token: 0x17001486 RID: 5254
		// (get) Token: 0x06005082 RID: 20610 RVA: 0x002B50CA File Offset: 0x002B32CA
		// (set) Token: 0x06005083 RID: 20611 RVA: 0x002B50D2 File Offset: 0x002B32D2
		public float TextureRange { get; private set; }

		// Token: 0x17001487 RID: 5255
		// (get) Token: 0x06005084 RID: 20612 RVA: 0x002B50DB File Offset: 0x002B32DB
		// (set) Token: 0x06005085 RID: 20613 RVA: 0x002B50E3 File Offset: 0x002B32E3
		public Sprite OverrideLightTexture { get; private set; }

		// Token: 0x17001488 RID: 5256
		// (get) Token: 0x06005086 RID: 20614 RVA: 0x002B50EC File Offset: 0x002B32EC
		// (set) Token: 0x06005087 RID: 20615 RVA: 0x002B50F4 File Offset: 0x002B32F4
		public Sprite LightSprite { get; private set; }

		// Token: 0x17001489 RID: 5257
		// (get) Token: 0x06005088 RID: 20616 RVA: 0x002B50FD File Offset: 0x002B32FD
		// (set) Token: 0x06005089 RID: 20617 RVA: 0x002B5105 File Offset: 0x002B3305
		public ContentXElement DeformableLightSpriteElement { get; private set; }

		// Token: 0x0600508A RID: 20618 RVA: 0x002B5110 File Offset: 0x002B3310
		public LightSourceParams(ContentXElement element)
		{
			this.Deserialize(element);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite") && !(a == "lightsprite"))
				{
					if (!(a == "deformablesprite"))
					{
						if (a == "lighttexture")
						{
							this.OverrideLightTexture = new Sprite(subElement, "", "", false, 1f);
							this.Range = this.range;
						}
					}
					else
					{
						this.DeformableLightSpriteElement = subElement;
						float spriteAlpha = subElement.GetAttributeFloat("alpha", -1f);
						if (spriteAlpha >= 0f)
						{
							this.OverrideLightSpriteAlpha = new float?(spriteAlpha);
						}
					}
				}
				else
				{
					this.LightSprite = new Sprite(subElement, "", "", false, 1f);
					float spriteAlpha2 = subElement.GetAttributeFloat("alpha", -1f);
					if (spriteAlpha2 >= 0f)
					{
						this.OverrideLightSpriteAlpha = new float?(spriteAlpha2);
					}
				}
			}
		}

		// Token: 0x0600508B RID: 20619 RVA: 0x002B5260 File Offset: 0x002B3460
		public LightSourceParams(float range, Color color)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
			this.Range = range;
			this.Color = color;
		}

		// Token: 0x0600508C RID: 20620 RVA: 0x002B528E File Offset: 0x002B348E
		public bool Deserialize(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			return this.SerializableProperties != null;
		}

		// Token: 0x0600508D RID: 20621 RVA: 0x002B52A6 File Offset: 0x002B34A6
		public void Serialize(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, true, false);
		}

		// Token: 0x04002A76 RID: 10870
		public bool Persistent;

		// Token: 0x04002A79 RID: 10873
		private float range;

		// Token: 0x04002A7E RID: 10878
		private float flicker;

		// Token: 0x04002A80 RID: 10880
		private float pulseFrequency;

		// Token: 0x04002A81 RID: 10881
		private float pulseAmount;

		// Token: 0x04002A82 RID: 10882
		private float blinkFrequency;

		// Token: 0x04002A87 RID: 10887
		public float? OverrideLightSpriteAlpha;
	}
}
