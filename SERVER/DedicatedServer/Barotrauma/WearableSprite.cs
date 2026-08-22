using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001F3 RID: 499
	internal class WearableSprite
	{
		// Token: 0x17000A29 RID: 2601
		// (get) Token: 0x06002374 RID: 9076 RVA: 0x000EDE13 File Offset: 0x000EC013
		// (set) Token: 0x06002375 RID: 9077 RVA: 0x000EDE1B File Offset: 0x000EC01B
		public ContentPath UnassignedSpritePath { get; private set; }

		// Token: 0x17000A2A RID: 2602
		// (get) Token: 0x06002376 RID: 9078 RVA: 0x000EDE24 File Offset: 0x000EC024
		// (set) Token: 0x06002377 RID: 9079 RVA: 0x000EDE2C File Offset: 0x000EC02C
		public string SpritePath { get; private set; }

		// Token: 0x17000A2B RID: 2603
		// (get) Token: 0x06002378 RID: 9080 RVA: 0x000EDE35 File Offset: 0x000EC035
		// (set) Token: 0x06002379 RID: 9081 RVA: 0x000EDE3D File Offset: 0x000EC03D
		public ContentXElement SourceElement { get; private set; }

		// Token: 0x17000A2C RID: 2604
		// (get) Token: 0x0600237A RID: 9082 RVA: 0x000EDE46 File Offset: 0x000EC046
		// (set) Token: 0x0600237B RID: 9083 RVA: 0x000EDE4E File Offset: 0x000EC04E
		public WearableType Type { get; private set; }

		// Token: 0x17000A2D RID: 2605
		// (get) Token: 0x0600237C RID: 9084 RVA: 0x000EDE57 File Offset: 0x000EC057
		// (set) Token: 0x0600237D RID: 9085 RVA: 0x000EDE5F File Offset: 0x000EC05F
		public Sprite Sprite
		{
			get
			{
				return this._sprite;
			}
			private set
			{
				if (value == this._sprite)
				{
					return;
				}
				if (this._sprite != null)
				{
					this._sprite.Remove();
				}
				this._sprite = value;
			}
		}

		// Token: 0x17000A2E RID: 2606
		// (get) Token: 0x0600237E RID: 9086 RVA: 0x000EDE85 File Offset: 0x000EC085
		// (set) Token: 0x0600237F RID: 9087 RVA: 0x000EDE8D File Offset: 0x000EC08D
		public LimbType Limb { get; private set; }

		// Token: 0x17000A2F RID: 2607
		// (get) Token: 0x06002380 RID: 9088 RVA: 0x000EDE96 File Offset: 0x000EC096
		// (set) Token: 0x06002381 RID: 9089 RVA: 0x000EDE9E File Offset: 0x000EC09E
		public bool HideLimb { get; private set; }

		// Token: 0x17000A30 RID: 2608
		// (get) Token: 0x06002382 RID: 9090 RVA: 0x000EDEA7 File Offset: 0x000EC0A7
		// (set) Token: 0x06002383 RID: 9091 RVA: 0x000EDEAF File Offset: 0x000EC0AF
		public WearableSprite.ObscuringMode ObscureOtherWearables { get; private set; }

		// Token: 0x17000A31 RID: 2609
		// (get) Token: 0x06002384 RID: 9092 RVA: 0x000EDEB8 File Offset: 0x000EC0B8
		public bool HideOtherWearables
		{
			get
			{
				return this.ObscureOtherWearables == WearableSprite.ObscuringMode.Hide;
			}
		}

		// Token: 0x17000A32 RID: 2610
		// (get) Token: 0x06002385 RID: 9093 RVA: 0x000EDEC3 File Offset: 0x000EC0C3
		public bool AlphaClipOtherWearables
		{
			get
			{
				return this.ObscureOtherWearables == WearableSprite.ObscuringMode.AlphaClip;
			}
		}

		// Token: 0x17000A33 RID: 2611
		// (get) Token: 0x06002386 RID: 9094 RVA: 0x000EDECE File Offset: 0x000EC0CE
		// (set) Token: 0x06002387 RID: 9095 RVA: 0x000EDED6 File Offset: 0x000EC0D6
		public bool CanBeHiddenByOtherWearables { get; private set; }

		// Token: 0x17000A34 RID: 2612
		// (get) Token: 0x06002388 RID: 9096 RVA: 0x000EDEDF File Offset: 0x000EC0DF
		// (set) Token: 0x06002389 RID: 9097 RVA: 0x000EDEE7 File Offset: 0x000EC0E7
		public ImmutableHashSet<Identifier> CanBeHiddenByItem { get; private set; }

		// Token: 0x17000A35 RID: 2613
		// (get) Token: 0x0600238A RID: 9098 RVA: 0x000EDEF0 File Offset: 0x000EC0F0
		// (set) Token: 0x0600238B RID: 9099 RVA: 0x000EDEF8 File Offset: 0x000EC0F8
		public List<WearableType> HideWearablesOfType { get; private set; }

		// Token: 0x17000A36 RID: 2614
		// (get) Token: 0x0600238C RID: 9100 RVA: 0x000EDF01 File Offset: 0x000EC101
		// (set) Token: 0x0600238D RID: 9101 RVA: 0x000EDF09 File Offset: 0x000EC109
		public bool InheritLimbDepth { get; private set; }

		// Token: 0x17000A37 RID: 2615
		// (get) Token: 0x0600238E RID: 9102 RVA: 0x000EDF12 File Offset: 0x000EC112
		// (set) Token: 0x0600238F RID: 9103 RVA: 0x000EDF1A File Offset: 0x000EC11A
		public bool InheritScale { get; private set; }

		// Token: 0x17000A38 RID: 2616
		// (get) Token: 0x06002390 RID: 9104 RVA: 0x000EDF23 File Offset: 0x000EC123
		// (set) Token: 0x06002391 RID: 9105 RVA: 0x000EDF2B File Offset: 0x000EC12B
		public bool IgnoreRagdollScale { get; private set; }

		// Token: 0x17000A39 RID: 2617
		// (get) Token: 0x06002392 RID: 9106 RVA: 0x000EDF34 File Offset: 0x000EC134
		// (set) Token: 0x06002393 RID: 9107 RVA: 0x000EDF3C File Offset: 0x000EC13C
		public bool IgnoreLimbScale { get; private set; }

		// Token: 0x17000A3A RID: 2618
		// (get) Token: 0x06002394 RID: 9108 RVA: 0x000EDF45 File Offset: 0x000EC145
		// (set) Token: 0x06002395 RID: 9109 RVA: 0x000EDF4D File Offset: 0x000EC14D
		public bool IgnoreTextureScale { get; private set; }

		// Token: 0x17000A3B RID: 2619
		// (get) Token: 0x06002396 RID: 9110 RVA: 0x000EDF56 File Offset: 0x000EC156
		// (set) Token: 0x06002397 RID: 9111 RVA: 0x000EDF5E File Offset: 0x000EC15E
		public bool InheritOrigin { get; private set; }

		// Token: 0x17000A3C RID: 2620
		// (get) Token: 0x06002398 RID: 9112 RVA: 0x000EDF67 File Offset: 0x000EC167
		// (set) Token: 0x06002399 RID: 9113 RVA: 0x000EDF6F File Offset: 0x000EC16F
		public bool InheritSourceRect { get; private set; }

		// Token: 0x17000A3D RID: 2621
		// (get) Token: 0x0600239A RID: 9114 RVA: 0x000EDF78 File Offset: 0x000EC178
		// (set) Token: 0x0600239B RID: 9115 RVA: 0x000EDF80 File Offset: 0x000EC180
		public float Scale { get; private set; }

		// Token: 0x17000A3E RID: 2622
		// (get) Token: 0x0600239C RID: 9116 RVA: 0x000EDF89 File Offset: 0x000EC189
		// (set) Token: 0x0600239D RID: 9117 RVA: 0x000EDF91 File Offset: 0x000EC191
		public float Rotation { get; private set; }

		// Token: 0x17000A3F RID: 2623
		// (get) Token: 0x0600239E RID: 9118 RVA: 0x000EDF9A File Offset: 0x000EC19A
		// (set) Token: 0x0600239F RID: 9119 RVA: 0x000EDFA2 File Offset: 0x000EC1A2
		public LimbType DepthLimb { get; private set; }

		// Token: 0x17000A40 RID: 2624
		// (get) Token: 0x060023A0 RID: 9120 RVA: 0x000EDFAB File Offset: 0x000EC1AB
		// (set) Token: 0x060023A1 RID: 9121 RVA: 0x000EDFB3 File Offset: 0x000EC1B3
		public Wearable WearableComponent
		{
			get
			{
				return this._wearableComponent;
			}
			set
			{
				if (value == this._wearableComponent)
				{
					return;
				}
				if (this._wearableComponent != null)
				{
					this._wearableComponent.Remove();
				}
				this._wearableComponent = value;
			}
		}

		// Token: 0x17000A41 RID: 2625
		// (get) Token: 0x060023A2 RID: 9122 RVA: 0x000EDFD9 File Offset: 0x000EC1D9
		// (set) Token: 0x060023A3 RID: 9123 RVA: 0x000EDFE1 File Offset: 0x000EC1E1
		public string Sound { get; private set; }

		// Token: 0x17000A42 RID: 2626
		// (get) Token: 0x060023A4 RID: 9124 RVA: 0x000EDFEA File Offset: 0x000EC1EA
		// (set) Token: 0x060023A5 RID: 9125 RVA: 0x000EDFF2 File Offset: 0x000EC1F2
		public Point? SheetIndex { get; private set; }

		// Token: 0x17000A43 RID: 2627
		// (get) Token: 0x060023A6 RID: 9126 RVA: 0x000EDFFB File Offset: 0x000EC1FB
		public LightComponent LightComponent
		{
			get
			{
				List<LightComponent> lightComponents = this.LightComponents;
				if (lightComponents == null)
				{
					return null;
				}
				return lightComponents.FirstOrDefault<LightComponent>();
			}
		}

		// Token: 0x17000A44 RID: 2628
		// (get) Token: 0x060023A7 RID: 9127 RVA: 0x000EE00E File Offset: 0x000EC20E
		public List<LightComponent> LightComponents
		{
			get
			{
				if (this._lightComponents == null)
				{
					this._lightComponents = new List<LightComponent>();
				}
				return this._lightComponents;
			}
		}

		// Token: 0x17000A45 RID: 2629
		// (get) Token: 0x060023A8 RID: 9128 RVA: 0x000EE029 File Offset: 0x000EC229
		// (set) Token: 0x060023A9 RID: 9129 RVA: 0x000EE031 File Offset: 0x000EC231
		public int Variant { get; set; }

		// Token: 0x17000A46 RID: 2630
		// (get) Token: 0x060023AA RID: 9130 RVA: 0x000EE03A File Offset: 0x000EC23A
		// (set) Token: 0x060023AB RID: 9131 RVA: 0x000EE042 File Offset: 0x000EC242
		public Character Picker
		{
			get
			{
				return this._picker;
			}
			set
			{
				if (value == this._picker)
				{
					return;
				}
				this._picker = value;
				this.IsInitialized = false;
				this.UnassignedSpritePath = this.ParseSpritePath(this.SourceElement);
				this.Init(this._picker);
			}
		}

		// Token: 0x060023AC RID: 9132 RVA: 0x000EE07C File Offset: 0x000EC27C
		public WearableSprite(ContentXElement subElement, WearableType type)
		{
			this.Type = type;
			this.SourceElement = subElement;
			this.UnassignedSpritePath = (subElement.GetAttributeContentPath("texture") ?? ContentPath.Empty);
			this.Init(null);
			if (type - WearableType.Hair <= 5)
			{
				this.Limb = LimbType.Head;
			}
		}

		// Token: 0x060023AD RID: 9133 RVA: 0x000EE0CC File Offset: 0x000EC2CC
		public WearableSprite(ContentXElement subElement, Wearable wearable, int variant = 0)
		{
			this.Type = WearableType.Item;
			this.WearableComponent = wearable;
			this.Variant = Math.Max(variant, 0);
			this.UnassignedSpritePath = this.ParseSpritePath(subElement);
			this.SourceElement = subElement;
		}

		// Token: 0x060023AE RID: 9134 RVA: 0x000EE104 File Offset: 0x000EC304
		private ContentPath ParseSpritePath(ContentXElement element)
		{
			if (element.DoesAttributeReferenceFileNameAlone("texture"))
			{
				ItemPrefab basePrefab = this.WearableComponent.Item.Prefab.ParentPrefab ?? this.WearableComponent.Item.Prefab;
				string textureName = element.GetAttributeString("texture", "");
				return ContentPath.FromRaw(element.ContentPackage, Path.GetDirectoryName(basePrefab.FilePath) + "/" + textureName);
			}
			return element.GetAttributeContentPath("texture") ?? ContentPath.Empty;
		}

		// Token: 0x060023AF RID: 9135 RVA: 0x000EE190 File Offset: 0x000EC390
		public void ParsePath(bool parseSpritePath)
		{
		}

		// Token: 0x17000A47 RID: 2631
		// (get) Token: 0x060023B0 RID: 9136 RVA: 0x000EE19D File Offset: 0x000EC39D
		// (set) Token: 0x060023B1 RID: 9137 RVA: 0x000EE1A5 File Offset: 0x000EC3A5
		public bool IsInitialized { get; private set; }

		// Token: 0x060023B2 RID: 9138 RVA: 0x000EE1B0 File Offset: 0x000EC3B0
		public void Init(Character picker = null)
		{
			if (this.IsInitialized)
			{
				return;
			}
			this._picker = picker;
			this.ParsePath(false);
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = new Sprite(this.SourceElement, "", this.SpritePath, false, 1f);
			this.Limb = (LimbType)Enum.Parse(typeof(LimbType), this.SourceElement.GetAttributeString("limb", "Head"), true);
			this.HideLimb = this.SourceElement.GetAttributeBool("hidelimb", false);
			foreach (WearableSprite.ObscuringMode mode in Enum.GetValues<WearableSprite.ObscuringMode>())
			{
				if (mode != WearableSprite.ObscuringMode.None)
				{
					ContentXElement sourceElement = this.SourceElement;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
					defaultInterpolatedStringHandler.AppendFormatted<WearableSprite.ObscuringMode>(mode);
					defaultInterpolatedStringHandler.AppendLiteral("OtherWearables");
					if (sourceElement.GetAttributeBool(defaultInterpolatedStringHandler.ToStringAndClear(), false))
					{
						this.ObscureOtherWearables = mode;
					}
				}
			}
			this.CanBeHiddenByOtherWearables = this.SourceElement.GetAttributeBool("canbehiddenbyotherwearables", true);
			this.CanBeHiddenByItem = this.SourceElement.GetAttributeIdentifierArray("CanBeHiddenByItem", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.InheritLimbDepth = this.SourceElement.GetAttributeBool("inheritlimbdepth", true);
			XAttribute scale = this.SourceElement.GetAttribute("inheritscale");
			if (scale != null)
			{
				this.InheritScale = scale.GetAttributeBool(this.Type > WearableType.Item);
			}
			else
			{
				this.InheritScale = this.SourceElement.GetAttributeBool("inherittexturescale", this.Type > WearableType.Item);
			}
			this.IgnoreLimbScale = this.SourceElement.GetAttributeBool("ignorelimbscale", false);
			this.IgnoreTextureScale = this.SourceElement.GetAttributeBool("ignoretexturescale", false);
			this.IgnoreRagdollScale = this.SourceElement.GetAttributeBool("ignoreragdollscale", false);
			this.SourceElement.GetAttributeBool("inherittexturescale", false);
			this.InheritOrigin = this.SourceElement.GetAttributeBool("inheritorigin", this.Type > WearableType.Item);
			this.InheritSourceRect = this.SourceElement.GetAttributeBool("inheritsourcerect", this.Type > WearableType.Item);
			this.DepthLimb = (LimbType)Enum.Parse(typeof(LimbType), this.SourceElement.GetAttributeString("depthlimb", "None"), true);
			this.Sound = this.SourceElement.GetAttributeString("sound", "");
			this.Scale = this.SourceElement.GetAttributeFloat("scale", 1f);
			this.Rotation = MathHelper.ToRadians(this.SourceElement.GetAttributeFloat("rotation", 0f));
			ContentXElement sourceElement2 = this.SourceElement;
			string key = "sheetindex";
			Point point = new Point(-1, -1);
			Point index = sourceElement2.GetAttributePoint(key, point);
			if (index.X > -1 && index.Y > -1)
			{
				this.SheetIndex = new Point?(index);
			}
			this.HideWearablesOfType = new List<WearableType>();
			string[] wearableTypes = this.SourceElement.GetAttributeStringArray("hidewearablesoftype", null, false);
			if (wearableTypes != null && wearableTypes.Length != 0)
			{
				foreach (string value in wearableTypes)
				{
					WearableType wearableType;
					if (Enum.TryParse<WearableType>(value, true, out wearableType))
					{
						this.HideWearablesOfType.Add(wearableType);
					}
				}
			}
			this.IsInitialized = true;
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x000EE4FC File Offset: 0x000EC6FC
		public void Remove()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this._picker = null;
		}

		// Token: 0x04001163 RID: 4451
		private Sprite _sprite;

		// Token: 0x04001174 RID: 4468
		private Wearable _wearableComponent;

		// Token: 0x04001177 RID: 4471
		private List<LightComponent> _lightComponents;

		// Token: 0x04001179 RID: 4473
		private Character _picker;

		// Token: 0x020009A7 RID: 2471
		public enum ObscuringMode
		{
			// Token: 0x040033FD RID: 13309
			None,
			// Token: 0x040033FE RID: 13310
			Hide,
			// Token: 0x040033FF RID: 13311
			AlphaClip
		}
	}
}
