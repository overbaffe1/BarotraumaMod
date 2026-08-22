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
	// Token: 0x020002DD RID: 733
	internal class WearableSprite
	{
		// Token: 0x1700101D RID: 4125
		// (get) Token: 0x06003D1A RID: 15642 RVA: 0x0022D483 File Offset: 0x0022B683
		// (set) Token: 0x06003D1B RID: 15643 RVA: 0x0022D48B File Offset: 0x0022B68B
		public ContentPath UnassignedSpritePath { get; private set; }

		// Token: 0x1700101E RID: 4126
		// (get) Token: 0x06003D1C RID: 15644 RVA: 0x0022D494 File Offset: 0x0022B694
		// (set) Token: 0x06003D1D RID: 15645 RVA: 0x0022D49C File Offset: 0x0022B69C
		public string SpritePath { get; private set; }

		// Token: 0x1700101F RID: 4127
		// (get) Token: 0x06003D1E RID: 15646 RVA: 0x0022D4A5 File Offset: 0x0022B6A5
		// (set) Token: 0x06003D1F RID: 15647 RVA: 0x0022D4AD File Offset: 0x0022B6AD
		public ContentXElement SourceElement { get; private set; }

		// Token: 0x17001020 RID: 4128
		// (get) Token: 0x06003D20 RID: 15648 RVA: 0x0022D4B6 File Offset: 0x0022B6B6
		// (set) Token: 0x06003D21 RID: 15649 RVA: 0x0022D4BE File Offset: 0x0022B6BE
		public WearableType Type { get; private set; }

		// Token: 0x17001021 RID: 4129
		// (get) Token: 0x06003D22 RID: 15650 RVA: 0x0022D4C7 File Offset: 0x0022B6C7
		// (set) Token: 0x06003D23 RID: 15651 RVA: 0x0022D4CF File Offset: 0x0022B6CF
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

		// Token: 0x17001022 RID: 4130
		// (get) Token: 0x06003D24 RID: 15652 RVA: 0x0022D4F5 File Offset: 0x0022B6F5
		// (set) Token: 0x06003D25 RID: 15653 RVA: 0x0022D4FD File Offset: 0x0022B6FD
		public LimbType Limb { get; private set; }

		// Token: 0x17001023 RID: 4131
		// (get) Token: 0x06003D26 RID: 15654 RVA: 0x0022D506 File Offset: 0x0022B706
		// (set) Token: 0x06003D27 RID: 15655 RVA: 0x0022D50E File Offset: 0x0022B70E
		public bool HideLimb { get; private set; }

		// Token: 0x17001024 RID: 4132
		// (get) Token: 0x06003D28 RID: 15656 RVA: 0x0022D517 File Offset: 0x0022B717
		// (set) Token: 0x06003D29 RID: 15657 RVA: 0x0022D51F File Offset: 0x0022B71F
		public WearableSprite.ObscuringMode ObscureOtherWearables { get; private set; }

		// Token: 0x17001025 RID: 4133
		// (get) Token: 0x06003D2A RID: 15658 RVA: 0x0022D528 File Offset: 0x0022B728
		public bool HideOtherWearables
		{
			get
			{
				return this.ObscureOtherWearables == WearableSprite.ObscuringMode.Hide;
			}
		}

		// Token: 0x17001026 RID: 4134
		// (get) Token: 0x06003D2B RID: 15659 RVA: 0x0022D533 File Offset: 0x0022B733
		public bool AlphaClipOtherWearables
		{
			get
			{
				return this.ObscureOtherWearables == WearableSprite.ObscuringMode.AlphaClip;
			}
		}

		// Token: 0x17001027 RID: 4135
		// (get) Token: 0x06003D2C RID: 15660 RVA: 0x0022D53E File Offset: 0x0022B73E
		// (set) Token: 0x06003D2D RID: 15661 RVA: 0x0022D546 File Offset: 0x0022B746
		public bool CanBeHiddenByOtherWearables { get; private set; }

		// Token: 0x17001028 RID: 4136
		// (get) Token: 0x06003D2E RID: 15662 RVA: 0x0022D54F File Offset: 0x0022B74F
		// (set) Token: 0x06003D2F RID: 15663 RVA: 0x0022D557 File Offset: 0x0022B757
		public ImmutableHashSet<Identifier> CanBeHiddenByItem { get; private set; }

		// Token: 0x17001029 RID: 4137
		// (get) Token: 0x06003D30 RID: 15664 RVA: 0x0022D560 File Offset: 0x0022B760
		// (set) Token: 0x06003D31 RID: 15665 RVA: 0x0022D568 File Offset: 0x0022B768
		public List<WearableType> HideWearablesOfType { get; private set; }

		// Token: 0x1700102A RID: 4138
		// (get) Token: 0x06003D32 RID: 15666 RVA: 0x0022D571 File Offset: 0x0022B771
		// (set) Token: 0x06003D33 RID: 15667 RVA: 0x0022D579 File Offset: 0x0022B779
		public bool InheritLimbDepth { get; private set; }

		// Token: 0x1700102B RID: 4139
		// (get) Token: 0x06003D34 RID: 15668 RVA: 0x0022D582 File Offset: 0x0022B782
		// (set) Token: 0x06003D35 RID: 15669 RVA: 0x0022D58A File Offset: 0x0022B78A
		public bool InheritScale { get; private set; }

		// Token: 0x1700102C RID: 4140
		// (get) Token: 0x06003D36 RID: 15670 RVA: 0x0022D593 File Offset: 0x0022B793
		// (set) Token: 0x06003D37 RID: 15671 RVA: 0x0022D59B File Offset: 0x0022B79B
		public bool IgnoreRagdollScale { get; private set; }

		// Token: 0x1700102D RID: 4141
		// (get) Token: 0x06003D38 RID: 15672 RVA: 0x0022D5A4 File Offset: 0x0022B7A4
		// (set) Token: 0x06003D39 RID: 15673 RVA: 0x0022D5AC File Offset: 0x0022B7AC
		public bool IgnoreLimbScale { get; private set; }

		// Token: 0x1700102E RID: 4142
		// (get) Token: 0x06003D3A RID: 15674 RVA: 0x0022D5B5 File Offset: 0x0022B7B5
		// (set) Token: 0x06003D3B RID: 15675 RVA: 0x0022D5BD File Offset: 0x0022B7BD
		public bool IgnoreTextureScale { get; private set; }

		// Token: 0x1700102F RID: 4143
		// (get) Token: 0x06003D3C RID: 15676 RVA: 0x0022D5C6 File Offset: 0x0022B7C6
		// (set) Token: 0x06003D3D RID: 15677 RVA: 0x0022D5CE File Offset: 0x0022B7CE
		public bool InheritOrigin { get; private set; }

		// Token: 0x17001030 RID: 4144
		// (get) Token: 0x06003D3E RID: 15678 RVA: 0x0022D5D7 File Offset: 0x0022B7D7
		// (set) Token: 0x06003D3F RID: 15679 RVA: 0x0022D5DF File Offset: 0x0022B7DF
		public bool InheritSourceRect { get; private set; }

		// Token: 0x17001031 RID: 4145
		// (get) Token: 0x06003D40 RID: 15680 RVA: 0x0022D5E8 File Offset: 0x0022B7E8
		// (set) Token: 0x06003D41 RID: 15681 RVA: 0x0022D5F0 File Offset: 0x0022B7F0
		public float Scale { get; private set; }

		// Token: 0x17001032 RID: 4146
		// (get) Token: 0x06003D42 RID: 15682 RVA: 0x0022D5F9 File Offset: 0x0022B7F9
		// (set) Token: 0x06003D43 RID: 15683 RVA: 0x0022D601 File Offset: 0x0022B801
		public float Rotation { get; private set; }

		// Token: 0x17001033 RID: 4147
		// (get) Token: 0x06003D44 RID: 15684 RVA: 0x0022D60A File Offset: 0x0022B80A
		// (set) Token: 0x06003D45 RID: 15685 RVA: 0x0022D612 File Offset: 0x0022B812
		public LimbType DepthLimb { get; private set; }

		// Token: 0x17001034 RID: 4148
		// (get) Token: 0x06003D46 RID: 15686 RVA: 0x0022D61B File Offset: 0x0022B81B
		// (set) Token: 0x06003D47 RID: 15687 RVA: 0x0022D623 File Offset: 0x0022B823
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

		// Token: 0x17001035 RID: 4149
		// (get) Token: 0x06003D48 RID: 15688 RVA: 0x0022D649 File Offset: 0x0022B849
		// (set) Token: 0x06003D49 RID: 15689 RVA: 0x0022D651 File Offset: 0x0022B851
		public string Sound { get; private set; }

		// Token: 0x17001036 RID: 4150
		// (get) Token: 0x06003D4A RID: 15690 RVA: 0x0022D65A File Offset: 0x0022B85A
		// (set) Token: 0x06003D4B RID: 15691 RVA: 0x0022D662 File Offset: 0x0022B862
		public Point? SheetIndex { get; private set; }

		// Token: 0x17001037 RID: 4151
		// (get) Token: 0x06003D4C RID: 15692 RVA: 0x0022D66B File Offset: 0x0022B86B
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

		// Token: 0x17001038 RID: 4152
		// (get) Token: 0x06003D4D RID: 15693 RVA: 0x0022D67E File Offset: 0x0022B87E
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

		// Token: 0x17001039 RID: 4153
		// (get) Token: 0x06003D4E RID: 15694 RVA: 0x0022D699 File Offset: 0x0022B899
		// (set) Token: 0x06003D4F RID: 15695 RVA: 0x0022D6A1 File Offset: 0x0022B8A1
		public int Variant { get; set; }

		// Token: 0x1700103A RID: 4154
		// (get) Token: 0x06003D50 RID: 15696 RVA: 0x0022D6AA File Offset: 0x0022B8AA
		// (set) Token: 0x06003D51 RID: 15697 RVA: 0x0022D6B2 File Offset: 0x0022B8B2
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

		// Token: 0x06003D52 RID: 15698 RVA: 0x0022D6EC File Offset: 0x0022B8EC
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

		// Token: 0x06003D53 RID: 15699 RVA: 0x0022D73C File Offset: 0x0022B93C
		public WearableSprite(ContentXElement subElement, Wearable wearable, int variant = 0)
		{
			this.Type = WearableType.Item;
			this.WearableComponent = wearable;
			this.Variant = Math.Max(variant, 0);
			this.UnassignedSpritePath = this.ParseSpritePath(subElement);
			this.SourceElement = subElement;
		}

		// Token: 0x06003D54 RID: 15700 RVA: 0x0022D774 File Offset: 0x0022B974
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

		// Token: 0x06003D55 RID: 15701 RVA: 0x0022D800 File Offset: 0x0022BA00
		public void ParsePath(bool parseSpritePath)
		{
			this.SpritePath = this.UnassignedSpritePath.Value;
			Character picker = this._picker;
			if (((picker != null) ? picker.Info : null) != null)
			{
				this.SpritePath = this._picker.Info.ReplaceVars(this.SpritePath);
			}
			this.SpritePath = this.SpritePath.Replace("[VARIANT]", this.Variant.ToString());
			if (!File.Exists(this.SpritePath))
			{
				this.SpritePath = this.SpritePath.Replace("[VARIANT]", "1");
			}
			if (!File.Exists(this.SpritePath))
			{
				Character picker2 = this._picker;
				if (((picker2 != null) ? picker2.Info : null) == null)
				{
					CharacterInfoPrefab charInfoPrefab = CharacterPrefab.HumanPrefab.CharacterInfoPrefab;
					this.SpritePath = charInfoPrefab.ReplaceVars(this.SpritePath, charInfoPrefab.Heads.First<CharacterInfo.HeadPreset>());
				}
			}
			if (parseSpritePath)
			{
				Sprite sprite = this.Sprite;
				if (sprite == null)
				{
					return;
				}
				sprite.ParseTexturePath("", this.SpritePath);
			}
		}

		// Token: 0x1700103B RID: 4155
		// (get) Token: 0x06003D56 RID: 15702 RVA: 0x0022D902 File Offset: 0x0022BB02
		// (set) Token: 0x06003D57 RID: 15703 RVA: 0x0022D90A File Offset: 0x0022BB0A
		public bool IsInitialized { get; private set; }

		// Token: 0x06003D58 RID: 15704 RVA: 0x0022D914 File Offset: 0x0022BB14
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

		// Token: 0x06003D59 RID: 15705 RVA: 0x0022DC60 File Offset: 0x0022BE60
		public void Remove()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this._picker = null;
		}

		// Token: 0x04001FEC RID: 8172
		private Sprite _sprite;

		// Token: 0x04001FFD RID: 8189
		private Wearable _wearableComponent;

		// Token: 0x04002000 RID: 8192
		private List<LightComponent> _lightComponents;

		// Token: 0x04002002 RID: 8194
		private Character _picker;

		// Token: 0x02000F84 RID: 3972
		public enum ObscuringMode
		{
			// Token: 0x040055DF RID: 21983
			None,
			// Token: 0x040055E0 RID: 21984
			Hide,
			// Token: 0x040055E1 RID: 21985
			AlphaClip
		}
	}
}
