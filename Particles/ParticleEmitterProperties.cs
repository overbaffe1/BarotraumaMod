using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Particles
{
	// Token: 0x0200044F RID: 1103
	[NullableContext(1)]
	[Nullable(0)]
	internal class ParticleEmitterProperties : ISerializableEntity
	{
		// Token: 0x170012BC RID: 4796
		// (get) Token: 0x060049D5 RID: 18901 RVA: 0x0028DF4E File Offset: 0x0028C14E
		public string Name
		{
			get
			{
				return "ParticleEmitter";
			}
		}

		// Token: 0x170012BD RID: 4797
		// (get) Token: 0x060049D6 RID: 18902 RVA: 0x0028DF55 File Offset: 0x0028C155
		// (set) Token: 0x060049D7 RID: 18903 RVA: 0x0028DF5D File Offset: 0x0028C15D
		public float AngleMinRad { get; private set; }

		// Token: 0x170012BE RID: 4798
		// (get) Token: 0x060049D8 RID: 18904 RVA: 0x0028DF66 File Offset: 0x0028C166
		// (set) Token: 0x060049D9 RID: 18905 RVA: 0x0028DF6E File Offset: 0x0028C16E
		public float AngleMaxRad { get; private set; }

		// Token: 0x170012BF RID: 4799
		// (get) Token: 0x060049DA RID: 18906 RVA: 0x0028DF77 File Offset: 0x0028C177
		// (set) Token: 0x060049DB RID: 18907 RVA: 0x0028DF7F File Offset: 0x0028C17F
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 360f, MinValueFloat = -360f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float AngleMin
		{
			get
			{
				return this.angleMin;
			}
			set
			{
				this.angleMin = value;
				this.AngleMinRad = MathHelper.ToRadians(MathHelper.Clamp(value, -360f, 360f));
			}
		}

		// Token: 0x170012C0 RID: 4800
		// (get) Token: 0x060049DC RID: 18908 RVA: 0x0028DFA3 File Offset: 0x0028C1A3
		// (set) Token: 0x060049DD RID: 18909 RVA: 0x0028DFAB File Offset: 0x0028C1AB
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 360f, MinValueFloat = -360f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float AngleMax
		{
			get
			{
				return this.angleMax;
			}
			set
			{
				this.angleMax = value;
				this.AngleMaxRad = MathHelper.ToRadians(MathHelper.Clamp(value, -360f, 360f));
			}
		}

		// Token: 0x170012C1 RID: 4801
		// (get) Token: 0x060049DE RID: 18910 RVA: 0x0028DFCF File Offset: 0x0028C1CF
		// (set) Token: 0x060049DF RID: 18911 RVA: 0x0028DFD7 File Offset: 0x0028C1D7
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float DistanceMin { get; set; }

		// Token: 0x170012C2 RID: 4802
		// (get) Token: 0x060049E0 RID: 18912 RVA: 0x0028DFE0 File Offset: 0x0028C1E0
		// (set) Token: 0x060049E1 RID: 18913 RVA: 0x0028DFE8 File Offset: 0x0028C1E8
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float DistanceMax { get; set; }

		// Token: 0x170012C3 RID: 4803
		// (get) Token: 0x060049E2 RID: 18914 RVA: 0x0028DFF1 File Offset: 0x0028C1F1
		// (set) Token: 0x060049E3 RID: 18915 RVA: 0x0028DFF9 File Offset: 0x0028C1F9
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float VelocityMin { get; set; }

		// Token: 0x170012C4 RID: 4804
		// (get) Token: 0x060049E4 RID: 18916 RVA: 0x0028E002 File Offset: 0x0028C202
		// (set) Token: 0x060049E5 RID: 18917 RVA: 0x0028E00A File Offset: 0x0028C20A
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 2.1474836E+09f, MinValueFloat = -2.1474836E+09f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float VelocityMax { get; set; }

		// Token: 0x170012C5 RID: 4805
		// (get) Token: 0x060049E6 RID: 18918 RVA: 0x0028E013 File Offset: 0x0028C213
		// (set) Token: 0x060049E7 RID: 18919 RVA: 0x0028E01B File Offset: 0x0028C21B
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 100f, MinValueFloat = 0f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float ScaleMin { get; set; }

		// Token: 0x170012C6 RID: 4806
		// (get) Token: 0x060049E8 RID: 18920 RVA: 0x0028E024 File Offset: 0x0028C224
		// (set) Token: 0x060049E9 RID: 18921 RVA: 0x0028E02C File Offset: 0x0028C22C
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 100f, MinValueFloat = 0f)]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float ScaleMax { get; set; }

		// Token: 0x170012C7 RID: 4807
		// (get) Token: 0x060049EA RID: 18922 RVA: 0x0028E035 File Offset: 0x0028C235
		// (set) Token: 0x060049EB RID: 18923 RVA: 0x0028E03D File Offset: 0x0028C23D
		[Editable]
		[Serialize("1,1", IsPropertySaveable.Yes, "", "", false)]
		public Vector2 ScaleMultiplier { get; set; }

		// Token: 0x170012C8 RID: 4808
		// (get) Token: 0x060049EC RID: 18924 RVA: 0x0028E046 File Offset: 0x0028C246
		// (set) Token: 0x060049ED RID: 18925 RVA: 0x0028E04E File Offset: 0x0028C24E
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 100f, MinValueFloat = 0f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float EmitInterval { get; set; }

		// Token: 0x170012C9 RID: 4809
		// (get) Token: 0x060049EE RID: 18926 RVA: 0x0028E057 File Offset: 0x0028C257
		// (set) Token: 0x060049EF RID: 18927 RVA: 0x0028E05F File Offset: 0x0028C25F
		[Editable(ValueStep = 1f, MinValueInt = 0, MaxValueInt = 1000)]
		[Serialize(0, IsPropertySaveable.Yes, "The number of particles to spawn per frame, or every x seconds if EmitInterval is set.", "", false)]
		public int ParticleAmount { get; set; }

		// Token: 0x170012CA RID: 4810
		// (get) Token: 0x060049F0 RID: 18928 RVA: 0x0028E068 File Offset: 0x0028C268
		// (set) Token: 0x060049F1 RID: 18929 RVA: 0x0028E070 File Offset: 0x0028C270
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 1000f, MinValueFloat = 0f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float ParticlesPerSecond { get; set; }

		// Token: 0x170012CB RID: 4811
		// (get) Token: 0x060049F2 RID: 18930 RVA: 0x0028E079 File Offset: 0x0028C279
		// (set) Token: 0x060049F3 RID: 18931 RVA: 0x0028E081 File Offset: 0x0028C281
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 10f, MinValueFloat = 0f)]
		[Serialize(0f, IsPropertySaveable.Yes, "If larger than 0, a particle is spawned every x pixels across the ray cast by a hitscan weapon.", "", false)]
		public float EmitAcrossRayInterval { get; set; }

		// Token: 0x170012CC RID: 4812
		// (get) Token: 0x060049F4 RID: 18932 RVA: 0x0028E08A File Offset: 0x0028C28A
		// (set) Token: 0x060049F5 RID: 18933 RVA: 0x0028E092 File Offset: 0x0028C292
		[Editable(ValueStep = 1f, DecimalCount = 2, MaxValueFloat = 100f, MinValueFloat = 0f)]
		[Serialize(0f, IsPropertySaveable.Yes, "Delay before the emitter becomes active after being created.", "", false)]
		public float InitialDelay { get; set; }

		// Token: 0x170012CD RID: 4813
		// (get) Token: 0x060049F6 RID: 18934 RVA: 0x0028E09B File Offset: 0x0028C29B
		// (set) Token: 0x060049F7 RID: 18935 RVA: 0x0028E0A3 File Offset: 0x0028C2A3
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HighQualityCollisionDetection { get; set; }

		// Token: 0x170012CE RID: 4814
		// (get) Token: 0x060049F8 RID: 18936 RVA: 0x0028E0AC File Offset: 0x0028C2AC
		// (set) Token: 0x060049F9 RID: 18937 RVA: 0x0028E0B4 File Offset: 0x0028C2B4
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool CopyEntityAngle { get; set; }

		// Token: 0x170012CF RID: 4815
		// (get) Token: 0x060049FA RID: 18938 RVA: 0x0028E0BD File Offset: 0x0028C2BD
		// (set) Token: 0x060049FB RID: 18939 RVA: 0x0028E0C5 File Offset: 0x0028C2C5
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the entity heading direction be applied to the particle rotation? Only affects after flipping the texture and when CopyEntityAngle is true.", "", false)]
		public bool CopyEntityDir { get; set; }

		// Token: 0x170012D0 RID: 4816
		// (get) Token: 0x060049FC RID: 18940 RVA: 0x0028E0CE File Offset: 0x0028C2CE
		// (set) Token: 0x060049FD RID: 18941 RVA: 0x0028E0D6 File Offset: 0x0028C2D6
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Only relevant for status effects. Makes the emitter copy the angle from the target of the effect instead of the entity applying the effect.", "", false)]
		public bool CopyTargetAngle { get; set; }

		// Token: 0x170012D1 RID: 4817
		// (get) Token: 0x060049FE RID: 18942 RVA: 0x0028E0DF File Offset: 0x0028C2DF
		// (set) Token: 0x060049FF RID: 18943 RVA: 0x0028E0E7 File Offset: 0x0028C2E7
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Only relevant for particles spawned by another particle. Makes the emitter copy the scale of the parent particle.", "", false)]
		public bool CopyParentParticleScale { get; set; }

		// Token: 0x170012D2 RID: 4818
		// (get) Token: 0x06004A00 RID: 18944 RVA: 0x0028E0F0 File Offset: 0x0028C2F0
		// (set) Token: 0x06004A01 RID: 18945 RVA: 0x0028E0F8 File Offset: 0x0028C2F8
		[Editable]
		[Serialize("1,1,1,1", IsPropertySaveable.Yes, "", "", false)]
		public Color ColorMultiplier { get; set; }

		// Token: 0x170012D3 RID: 4819
		// (get) Token: 0x06004A02 RID: 18946 RVA: 0x0028E101 File Offset: 0x0028C301
		// (set) Token: 0x06004A03 RID: 18947 RVA: 0x0028E109 File Offset: 0x0028C309
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float LifeTimeMultiplier { get; set; }

		// Token: 0x170012D4 RID: 4820
		// (get) Token: 0x06004A04 RID: 18948 RVA: 0x0028E112 File Offset: 0x0028C312
		// (set) Token: 0x06004A05 RID: 18949 RVA: 0x0028E11A File Offset: 0x0028C31A
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the particle be drawn as a tracer (a line from a weapon to the point it hit)? Only supported on hitscan projectiles and repair tools. Defaults to true on hitscan projectiles.", "", false)]
		public bool UseTracerPoints { get; set; }

		// Token: 0x170012D5 RID: 4821
		// (get) Token: 0x06004A06 RID: 18950 RVA: 0x0028E123 File Offset: 0x0028C323
		// (set) Token: 0x06004A07 RID: 18951 RVA: 0x0028E12B File Offset: 0x0028C32B
		[Editable]
		[Serialize(ParticleDrawOrder.Default, IsPropertySaveable.Yes, "", "", false)]
		public ParticleDrawOrder DrawOrder { get; set; }

		// Token: 0x170012D6 RID: 4822
		// (get) Token: 0x06004A08 RID: 18952 RVA: 0x0028E134 File Offset: 0x0028C334
		// (set) Token: 0x06004A09 RID: 18953 RVA: 0x0028E13C File Offset: 0x0028C33C
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Angle
		{
			get
			{
				return this.AngleMin;
			}
			set
			{
				this.AngleMax = value;
				this.AngleMin = value;
			}
		}

		// Token: 0x170012D7 RID: 4823
		// (get) Token: 0x06004A0A RID: 18954 RVA: 0x0028E159 File Offset: 0x0028C359
		// (set) Token: 0x06004A0B RID: 18955 RVA: 0x0028E164 File Offset: 0x0028C364
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Distance
		{
			get
			{
				return this.DistanceMin;
			}
			set
			{
				this.DistanceMax = value;
				this.DistanceMin = value;
			}
		}

		// Token: 0x170012D8 RID: 4824
		// (get) Token: 0x06004A0C RID: 18956 RVA: 0x0028E181 File Offset: 0x0028C381
		// (set) Token: 0x06004A0D RID: 18957 RVA: 0x0028E18C File Offset: 0x0028C38C
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Velocity
		{
			get
			{
				return this.VelocityMin;
			}
			set
			{
				this.VelocityMax = value;
				this.VelocityMin = value;
			}
		}

		// Token: 0x170012D9 RID: 4825
		// (get) Token: 0x06004A0E RID: 18958 RVA: 0x0028E1A9 File Offset: 0x0028C3A9
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; }

		// Token: 0x06004A0F RID: 18959 RVA: 0x0028E1B1 File Offset: 0x0028C3B1
		public ParticleEmitterProperties(XElement element)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element.GetAttributeBool("drawontop", false))
			{
				this.DrawOrder = ParticleDrawOrder.Foreground;
			}
		}

		// Token: 0x04002699 RID: 9881
		private const float MinValue = -2.1474836E+09f;

		// Token: 0x0400269A RID: 9882
		private const float MaxValue = 2.1474836E+09f;

		// Token: 0x0400269B RID: 9883
		private float angleMin;

		// Token: 0x0400269C RID: 9884
		private float angleMax;
	}
}
