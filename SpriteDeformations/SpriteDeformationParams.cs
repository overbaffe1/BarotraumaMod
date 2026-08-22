using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000439 RID: 1081
	internal abstract class SpriteDeformationParams : ISerializableEntity
	{
		// Token: 0x1700125A RID: 4698
		// (get) Token: 0x0600482B RID: 18475 RVA: 0x0027977C File Offset: 0x0027797C
		// (set) Token: 0x0600482C RID: 18476 RVA: 0x00279784 File Offset: 0x00277984
		[Serialize(-1, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-1, 100)]
		public int Sync { get; private set; }

		// Token: 0x1700125B RID: 4699
		// (get) Token: 0x0600482D RID: 18477 RVA: 0x0027978D File Offset: 0x0027798D
		// (set) Token: 0x0600482E RID: 18478 RVA: 0x00279795 File Offset: 0x00277995
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public string Type { get; set; }

		// Token: 0x1700125C RID: 4700
		// (get) Token: 0x0600482F RID: 18479 RVA: 0x0027979E File Offset: 0x0027799E
		// (set) Token: 0x06004830 RID: 18480 RVA: 0x002797A6 File Offset: 0x002779A6
		[Serialize(SpriteDeformation.DeformationBlendMode.Add, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public SpriteDeformation.DeformationBlendMode BlendMode { get; set; }

		// Token: 0x1700125D RID: 4701
		// (get) Token: 0x06004831 RID: 18481 RVA: 0x002797AF File Offset: 0x002779AF
		public string Name
		{
			get
			{
				return "Deformation (" + this.Type + ")";
			}
		}

		// Token: 0x1700125E RID: 4702
		// (get) Token: 0x06004832 RID: 18482 RVA: 0x002797C6 File Offset: 0x002779C6
		// (set) Token: 0x06004833 RID: 18483 RVA: 0x002797CE File Offset: 0x002779CE
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2, ValueStep = 0.01f)]
		public float Strength { get; private set; }

		// Token: 0x1700125F RID: 4703
		// (get) Token: 0x06004834 RID: 18484 RVA: 0x002797D7 File Offset: 0x002779D7
		// (set) Token: 0x06004835 RID: 18485 RVA: 0x002797DF File Offset: 0x002779DF
		[Serialize(90f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 90f)]
		public float MaxRotation { get; private set; }

		// Token: 0x17001260 RID: 4704
		// (get) Token: 0x06004836 RID: 18486 RVA: 0x002797E8 File Offset: 0x002779E8
		// (set) Token: 0x06004837 RID: 18487 RVA: 0x002797F0 File Offset: 0x002779F0
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool UseMovementSine { get; set; }

		// Token: 0x17001261 RID: 4705
		// (get) Token: 0x06004838 RID: 18488 RVA: 0x002797F9 File Offset: 0x002779F9
		// (set) Token: 0x06004839 RID: 18489 RVA: 0x00279801 File Offset: 0x00277A01
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool StopWhenHostIsDead { get; set; }

		// Token: 0x17001262 RID: 4706
		// (get) Token: 0x0600483A RID: 18490 RVA: 0x0027980A File Offset: 0x00277A0A
		// (set) Token: 0x0600483B RID: 18491 RVA: 0x00279812 File Offset: 0x00277A12
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool OnlyInWater { get; set; }

		// Token: 0x17001263 RID: 4707
		// (get) Token: 0x0600483C RID: 18492 RVA: 0x0027981B File Offset: 0x00277A1B
		// (set) Token: 0x0600483D RID: 18493 RVA: 0x00279823 File Offset: 0x00277A23
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float SineOffset { get; set; }

		// Token: 0x17001264 RID: 4708
		// (get) Token: 0x0600483E RID: 18494 RVA: 0x0027982C File Offset: 0x00277A2C
		// (set) Token: 0x0600483F RID: 18495 RVA: 0x00279834 File Offset: 0x00277A34
		public virtual float Frequency { get; set; } = 1f;

		// Token: 0x17001265 RID: 4709
		// (get) Token: 0x06004840 RID: 18496 RVA: 0x0027983D File Offset: 0x00277A3D
		// (set) Token: 0x06004841 RID: 18497 RVA: 0x00279845 File Offset: 0x00277A45
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17001266 RID: 4710
		// (get) Token: 0x06004842 RID: 18498 RVA: 0x0027984E File Offset: 0x00277A4E
		// (set) Token: 0x06004843 RID: 18499 RVA: 0x00279856 File Offset: 0x00277A56
		[Serialize("2,2", IsPropertySaveable.Yes, "", "", false)]
		public Point Resolution
		{
			get
			{
				return this._resolution;
			}
			set
			{
				if (this._resolution == value)
				{
					return;
				}
				this._resolution = value.Clamp(new Point(2, 2), SpriteDeformationParams.ShaderMaxResolution);
			}
		}

		// Token: 0x06004844 RID: 18500 RVA: 0x00279880 File Offset: 0x00277A80
		public SpriteDeformationParams(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element != null && string.IsNullOrEmpty(this.Type))
			{
				this.Type = element.GetAttributeString("typename", string.Empty);
			}
		}

		// Token: 0x04002572 RID: 9586
		public static readonly Point ShaderMaxResolution = new Point(15, 15);

		// Token: 0x04002573 RID: 9587
		private Point _resolution;
	}
}
