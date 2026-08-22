using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Particles
{
	// Token: 0x02000455 RID: 1109
	internal class ParticlePrefab : Prefab, ISerializableEntity
	{
		// Token: 0x06004A2C RID: 18988 RVA: 0x0028F11C File Offset: 0x0028D31C
		public override void Dispose()
		{
			ParticleManager particleManager = GameMain.ParticleManager;
			if (particleManager != null)
			{
				particleManager.RemoveByPrefab(this);
			}
			foreach (Sprite spr in this.Sprites)
			{
				spr.Remove();
			}
			this.Sprites.Clear();
		}

		// Token: 0x170012DF RID: 4831
		// (get) Token: 0x06004A2D RID: 18989 RVA: 0x0028F18C File Offset: 0x0028D38C
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x170012E0 RID: 4832
		// (get) Token: 0x06004A2E RID: 18990 RVA: 0x0028F199 File Offset: 0x0028D399
		// (set) Token: 0x06004A2F RID: 18991 RVA: 0x0028F1A1 File Offset: 0x0028D3A1
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(5f, IsPropertySaveable.No, "How many seconds the particle remains alive.", "", false)]
		public float LifeTime { get; private set; }

		// Token: 0x170012E1 RID: 4833
		// (get) Token: 0x06004A30 RID: 18992 RVA: 0x0028F1AA File Offset: 0x0028D3AA
		// (set) Token: 0x06004A31 RID: 18993 RVA: 0x0028F1B2 File Offset: 0x0028D3B2
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(0f, IsPropertySaveable.No, "Will randomize lifetime value between lifetime and lifetimeMin. If left to 0 will use only lifetime value.", "", false)]
		public float LifeTimeMin { get; private set; }

		// Token: 0x170012E2 RID: 4834
		// (get) Token: 0x06004A32 RID: 18994 RVA: 0x0028F1BB File Offset: 0x0028D3BB
		// (set) Token: 0x06004A33 RID: 18995 RVA: 0x0028F1C3 File Offset: 0x0028D3C3
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How long it takes for the particle to appear after spawning it.", "", false)]
		public float StartDelayMin { get; private set; }

		// Token: 0x170012E3 RID: 4835
		// (get) Token: 0x06004A34 RID: 18996 RVA: 0x0028F1CC File Offset: 0x0028D3CC
		// (set) Token: 0x06004A35 RID: 18997 RVA: 0x0028F1D4 File Offset: 0x0028D3D4
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How long it takes for the particle to appear after spawning it.", "", false)]
		public float StartDelayMax { get; private set; }

		// Token: 0x170012E4 RID: 4836
		// (get) Token: 0x06004A36 RID: 18998 RVA: 0x0028F1DD File Offset: 0x0028D3DD
		// (set) Token: 0x06004A37 RID: 18999 RVA: 0x0028F1E5 File Offset: 0x0028D3E5
		public float AngularVelocityMinRad { get; private set; }

		// Token: 0x170012E5 RID: 4837
		// (get) Token: 0x06004A38 RID: 19000 RVA: 0x0028F1EE File Offset: 0x0028D3EE
		// (set) Token: 0x06004A39 RID: 19001 RVA: 0x0028F1F6 File Offset: 0x0028D3F6
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AngularVelocityMin
		{
			get
			{
				return this.angularVelocityMin;
			}
			private set
			{
				this.angularVelocityMin = value;
				this.AngularVelocityMinRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170012E6 RID: 4838
		// (get) Token: 0x06004A3A RID: 19002 RVA: 0x0028F20B File Offset: 0x0028D40B
		// (set) Token: 0x06004A3B RID: 19003 RVA: 0x0028F213 File Offset: 0x0028D413
		public float AngularVelocityMaxRad { get; private set; }

		// Token: 0x170012E7 RID: 4839
		// (get) Token: 0x06004A3C RID: 19004 RVA: 0x0028F21C File Offset: 0x0028D41C
		// (set) Token: 0x06004A3D RID: 19005 RVA: 0x0028F224 File Offset: 0x0028D424
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AngularVelocityMax
		{
			get
			{
				return this.angularVelocityMax;
			}
			private set
			{
				this.angularVelocityMax = value;
				this.AngularVelocityMaxRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170012E8 RID: 4840
		// (get) Token: 0x06004A3E RID: 19006 RVA: 0x0028F239 File Offset: 0x0028D439
		// (set) Token: 0x06004A3F RID: 19007 RVA: 0x0028F241 File Offset: 0x0028D441
		public float StartRotationMinRad { get; private set; }

		// Token: 0x170012E9 RID: 4841
		// (get) Token: 0x06004A40 RID: 19008 RVA: 0x0028F24A File Offset: 0x0028D44A
		// (set) Token: 0x06004A41 RID: 19009 RVA: 0x0028F252 File Offset: 0x0028D452
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "The minimum initial rotation of the particle (in degrees).", "", false)]
		public float StartRotationMin
		{
			get
			{
				return this.startRotationMin;
			}
			private set
			{
				this.startRotationMin = value;
				this.StartRotationMinRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170012EA RID: 4842
		// (get) Token: 0x06004A42 RID: 19010 RVA: 0x0028F267 File Offset: 0x0028D467
		// (set) Token: 0x06004A43 RID: 19011 RVA: 0x0028F26F File Offset: 0x0028D46F
		public float StartRotationMaxRad { get; private set; }

		// Token: 0x170012EB RID: 4843
		// (get) Token: 0x06004A44 RID: 19012 RVA: 0x0028F278 File Offset: 0x0028D478
		// (set) Token: 0x06004A45 RID: 19013 RVA: 0x0028F280 File Offset: 0x0028D480
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "The maximum initial rotation of the particle (in degrees).", "", false)]
		public float StartRotationMax
		{
			get
			{
				return this.startRotationMax;
			}
			private set
			{
				this.startRotationMax = value;
				this.StartRotationMaxRad = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x170012EC RID: 4844
		// (get) Token: 0x06004A46 RID: 19014 RVA: 0x0028F295 File Offset: 0x0028D495
		// (set) Token: 0x06004A47 RID: 19015 RVA: 0x0028F29D File Offset: 0x0028D49D
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Should the particle face the direction it's moving towards.", "", false)]
		public bool RotateToDirection { get; private set; }

		// Token: 0x170012ED RID: 4845
		// (get) Token: 0x06004A48 RID: 19016 RVA: 0x0028F2A6 File Offset: 0x0028D4A6
		// (set) Token: 0x06004A49 RID: 19017 RVA: 0x0028F2AE File Offset: 0x0028D4AE
		[Editable(0f, 3.4028235E+38f, 1, DecimalCount = 3)]
		[Serialize(0f, IsPropertySaveable.No, "Drag applied to the particle when it's moving through air.", "", false)]
		public float Drag { get; private set; }

		// Token: 0x170012EE RID: 4846
		// (get) Token: 0x06004A4A RID: 19018 RVA: 0x0028F2B7 File Offset: 0x0028D4B7
		// (set) Token: 0x06004A4B RID: 19019 RVA: 0x0028F2BF File Offset: 0x0028D4BF
		[Editable(0f, 3.4028235E+38f, 1, DecimalCount = 3)]
		[Serialize(0f, IsPropertySaveable.No, "Drag applied to the particle when it's moving through water.", "", false)]
		public float WaterDrag { get; private set; }

		// Token: 0x170012EF RID: 4847
		// (get) Token: 0x06004A4C RID: 19020 RVA: 0x0028F2C8 File Offset: 0x0028D4C8
		// (set) Token: 0x06004A4D RID: 19021 RVA: 0x0028F2D0 File Offset: 0x0028D4D0
		public Vector2 VelocityChangeDisplay { get; private set; }

		// Token: 0x170012F0 RID: 4848
		// (get) Token: 0x06004A4E RID: 19022 RVA: 0x0028F2D9 File Offset: 0x0028D4D9
		// (set) Token: 0x06004A4F RID: 19023 RVA: 0x0028F2E1 File Offset: 0x0028D4E1
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.No, "How much the velocity of the particle changes per second.", "", false)]
		public Vector2 VelocityChange
		{
			get
			{
				return this.velocityChange;
			}
			private set
			{
				this.velocityChange = value;
				this.VelocityChangeDisplay = ConvertUnits.ToDisplayUnits(value);
			}
		}

		// Token: 0x170012F1 RID: 4849
		// (get) Token: 0x06004A50 RID: 19024 RVA: 0x0028F2F6 File Offset: 0x0028D4F6
		// (set) Token: 0x06004A51 RID: 19025 RVA: 0x0028F2FE File Offset: 0x0028D4FE
		public Vector2 VelocityChangeWaterDisplay { get; private set; }

		// Token: 0x170012F2 RID: 4850
		// (get) Token: 0x06004A52 RID: 19026 RVA: 0x0028F307 File Offset: 0x0028D507
		// (set) Token: 0x06004A53 RID: 19027 RVA: 0x0028F30F File Offset: 0x0028D50F
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.No, "How much the velocity of the particle changes per second when in water.", "", false)]
		public Vector2 VelocityChangeWater
		{
			get
			{
				return this.velocityChangeWater;
			}
			private set
			{
				this.velocityChangeWater = value;
				this.VelocityChangeWaterDisplay = ConvertUnits.ToDisplayUnits(value);
			}
		}

		// Token: 0x170012F3 RID: 4851
		// (get) Token: 0x06004A54 RID: 19028 RVA: 0x0028F324 File Offset: 0x0028D524
		// (set) Token: 0x06004A55 RID: 19029 RVA: 0x0028F32C File Offset: 0x0028D52C
		[Editable]
		[Serialize(true, IsPropertySaveable.No, "Is the particle considered to be inside a submarine if it spawns at a position inside a hull (causing it to move with the sub)?", "", false)]
		public bool CanEnterSubs { get; private set; }

		// Token: 0x170012F4 RID: 4852
		// (get) Token: 0x06004A56 RID: 19030 RVA: 0x0028F335 File Offset: 0x0028D535
		// (set) Token: 0x06004A57 RID: 19031 RVA: 0x0028F33D File Offset: 0x0028D53D
		[Editable(0f, 10000f, 1)]
		[Serialize(0f, IsPropertySaveable.No, "Radius of the particle's collider. Only has an effect if UseCollision is set to true.", "", false)]
		public float CollisionRadius { get; private set; }

		// Token: 0x170012F5 RID: 4853
		// (get) Token: 0x06004A58 RID: 19032 RVA: 0x0028F346 File Offset: 0x0028D546
		// (set) Token: 0x06004A59 RID: 19033 RVA: 0x0028F34E File Offset: 0x0028D54E
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "If enabled, the size (or changes in size) of the particle doesn't affect the size of the collider.", "", false)]
		public bool InvariantCollisionSize { get; private set; }

		// Token: 0x170012F6 RID: 4854
		// (get) Token: 0x06004A5A RID: 19034 RVA: 0x0028F357 File Offset: 0x0028D557
		// (set) Token: 0x06004A5B RID: 19035 RVA: 0x0028F35F File Offset: 0x0028D55F
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the particle collide with the walls of the submarine and the level.", "", false)]
		public bool UseCollision { get; private set; }

		// Token: 0x170012F7 RID: 4855
		// (get) Token: 0x06004A5C RID: 19036 RVA: 0x0028F368 File Offset: 0x0028D568
		// (set) Token: 0x06004A5D RID: 19037 RVA: 0x0028F370 File Offset: 0x0028D570
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the particle disappear when it collides with something.", "", false)]
		public bool DeleteOnCollision { get; private set; }

		// Token: 0x170012F8 RID: 4856
		// (get) Token: 0x06004A5E RID: 19038 RVA: 0x0028F379 File Offset: 0x0028D579
		// (set) Token: 0x06004A5F RID: 19039 RVA: 0x0028F381 File Offset: 0x0028D581
		[Editable(0f, 1f, 1)]
		[Serialize(0.5f, IsPropertySaveable.No, "The friction coefficient of the particle, i.e. how much it slows down when it's sliding against a surface.", "", false)]
		public float Friction { get; private set; }

		// Token: 0x170012F9 RID: 4857
		// (get) Token: 0x06004A60 RID: 19040 RVA: 0x0028F38A File Offset: 0x0028D58A
		// (set) Token: 0x06004A61 RID: 19041 RVA: 0x0028F392 File Offset: 0x0028D592
		[Editable(0f, 1f, 1)]
		[Serialize(0.5f, IsPropertySaveable.No, "How much of the particle's velocity is conserved when it collides with something, i.e. the \"bounciness\" of the particle. (0.0 = the particle stops completely).", "", false)]
		public float Restitution { get; private set; }

		// Token: 0x170012FA RID: 4858
		// (get) Token: 0x06004A62 RID: 19042 RVA: 0x0028F39B File Offset: 0x0028D59B
		// (set) Token: 0x06004A63 RID: 19043 RVA: 0x0028F3A3 File Offset: 0x0028D5A3
		[Editable(DecimalCount = 3)]
		[Serialize("1.0,1.0", IsPropertySaveable.No, "The minimum initial size of the particle.", "", false)]
		public Vector2 StartSizeMin { get; private set; }

		// Token: 0x170012FB RID: 4859
		// (get) Token: 0x06004A64 RID: 19044 RVA: 0x0028F3AC File Offset: 0x0028D5AC
		// (set) Token: 0x06004A65 RID: 19045 RVA: 0x0028F3B4 File Offset: 0x0028D5B4
		[Editable(DecimalCount = 3)]
		[Serialize("1.0,1.0", IsPropertySaveable.No, "The maximum initial size of the particle.", "", false)]
		public Vector2 StartSizeMax { get; private set; }

		// Token: 0x170012FC RID: 4860
		// (get) Token: 0x06004A66 RID: 19046 RVA: 0x0028F3BD File Offset: 0x0028D5BD
		// (set) Token: 0x06004A67 RID: 19047 RVA: 0x0028F3C5 File Offset: 0x0028D5C5
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.No, "How much the size of the particle changes per second. The rate of growth for each particle is randomize between SizeChangeMin and SizeChangeMax.", "", false)]
		public Vector2 SizeChangeMin { get; private set; }

		// Token: 0x170012FD RID: 4861
		// (get) Token: 0x06004A68 RID: 19048 RVA: 0x0028F3CE File Offset: 0x0028D5CE
		// (set) Token: 0x06004A69 RID: 19049 RVA: 0x0028F3D6 File Offset: 0x0028D5D6
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.No, "How much the size of the particle changes per second. The rate of growth for each particle is randomize between SizeChangeMin and SizeChangeMax.", "", false)]
		public Vector2 SizeChangeMax { get; private set; }

		// Token: 0x170012FE RID: 4862
		// (get) Token: 0x06004A6A RID: 19050 RVA: 0x0028F3DF File Offset: 0x0028D5DF
		// (set) Token: 0x06004A6B RID: 19051 RVA: 0x0028F3E7 File Offset: 0x0028D5E7
		[Editable(0f, 3.4028235E+38f, 2)]
		[Serialize(0f, IsPropertySaveable.No, "How many seconds it takes for the particle to grow to it's initial size.", "", false)]
		public float GrowTime { get; private set; }

		// Token: 0x170012FF RID: 4863
		// (get) Token: 0x06004A6C RID: 19052 RVA: 0x0028F3F0 File Offset: 0x0028D5F0
		// (set) Token: 0x06004A6D RID: 19053 RVA: 0x0028F3F8 File Offset: 0x0028D5F8
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "The initial color of the particle.", "", false)]
		public Color StartColor { get; private set; }

		// Token: 0x17001300 RID: 4864
		// (get) Token: 0x06004A6E RID: 19054 RVA: 0x0028F401 File Offset: 0x0028D601
		// (set) Token: 0x06004A6F RID: 19055 RVA: 0x0028F409 File Offset: 0x0028D609
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "The initial color of the particle.", "", false)]
		public Color MiddleColor { get; private set; }

		// Token: 0x17001301 RID: 4865
		// (get) Token: 0x06004A70 RID: 19056 RVA: 0x0028F412 File Offset: 0x0028D612
		// (set) Token: 0x06004A71 RID: 19057 RVA: 0x0028F41A File Offset: 0x0028D61A
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.No, "The color of the particle at the end of its lifetime.", "", false)]
		public Color EndColor { get; private set; }

		// Token: 0x17001302 RID: 4866
		// (get) Token: 0x06004A72 RID: 19058 RVA: 0x0028F423 File Offset: 0x0028D623
		// (set) Token: 0x06004A73 RID: 19059 RVA: 0x0028F42B File Offset: 0x0028D62B
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "If true the color will go from StartColor to EndcColor and back to StartColor.", "", false)]
		public bool UseMiddleColor { get; private set; }

		// Token: 0x17001303 RID: 4867
		// (get) Token: 0x06004A74 RID: 19060 RVA: 0x0028F434 File Offset: 0x0028D634
		// (set) Token: 0x06004A75 RID: 19061 RVA: 0x0028F43C File Offset: 0x0028D63C
		[Editable]
		[Serialize(ParticlePrefab.DrawTargetType.Air, IsPropertySaveable.No, "Should the particle be rendered in air, water or both.", "", false)]
		public ParticlePrefab.DrawTargetType DrawTarget { get; private set; }

		// Token: 0x17001304 RID: 4868
		// (get) Token: 0x06004A76 RID: 19062 RVA: 0x0028F445 File Offset: 0x0028D645
		// (set) Token: 0x06004A77 RID: 19063 RVA: 0x0028F44D File Offset: 0x0028D64D
		[Editable]
		[Serialize(ParticleDrawOrder.Default, IsPropertySaveable.No, "Should the particle be always forced to render on top of entities or behind everything?", "", false)]
		public ParticleDrawOrder DrawOrder { get; private set; }

		// Token: 0x17001305 RID: 4869
		// (get) Token: 0x06004A78 RID: 19064 RVA: 0x0028F456 File Offset: 0x0028D656
		// (set) Token: 0x06004A79 RID: 19065 RVA: 0x0028F45E File Offset: 0x0028D65E
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Draw the particle even when it's calculated to be outside of view (the formula doesn't take scales into account). ", "", false)]
		public bool DrawAlways { get; private set; }

		// Token: 0x17001306 RID: 4870
		// (get) Token: 0x06004A7A RID: 19066 RVA: 0x0028F467 File Offset: 0x0028D667
		// (set) Token: 0x06004A7B RID: 19067 RVA: 0x0028F46F File Offset: 0x0028D66F
		[Editable]
		[Serialize(ParticleBlendState.AlphaBlend, IsPropertySaveable.No, "The type of blending to use when rendering the particle.", "", false)]
		public ParticleBlendState BlendState { get; private set; }

		// Token: 0x17001307 RID: 4871
		// (get) Token: 0x06004A7C RID: 19068 RVA: 0x0028F478 File Offset: 0x0028D678
		// (set) Token: 0x06004A7D RID: 19069 RVA: 0x0028F480 File Offset: 0x0028D680
		[Editable]
		[Serialize(0, IsPropertySaveable.No, "Particles with a higher priority can replace lower-priority ones if the maximum number of active particles has been reached.", "", false)]
		public int Priority { get; private set; }

		// Token: 0x17001308 RID: 4872
		// (get) Token: 0x06004A7E RID: 19070 RVA: 0x0028F489 File Offset: 0x0028D689
		// (set) Token: 0x06004A7F RID: 19071 RVA: 0x0028F491 File Offset: 0x0028D691
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(1f, IsPropertySaveable.No, "The duration of the particle's animation cycle (if it's animated).", "", false)]
		public float AnimDuration { get; private set; }

		// Token: 0x17001309 RID: 4873
		// (get) Token: 0x06004A80 RID: 19072 RVA: 0x0028F49A File Offset: 0x0028D69A
		// (set) Token: 0x06004A81 RID: 19073 RVA: 0x0028F4A2 File Offset: 0x0028D6A2
		[Editable]
		[Serialize(true, IsPropertySaveable.No, "Should the sprite animation be looped, or stay at the last frame when the animation finishes.", "", false)]
		public bool LoopAnim { get; private set; }

		// Token: 0x1700130A RID: 4874
		// (get) Token: 0x06004A82 RID: 19074 RVA: 0x0028F4AB File Offset: 0x0028D6AB
		// (set) Token: 0x06004A83 RID: 19075 RVA: 0x0028F4B3 File Offset: 0x0028D6B3
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06004A84 RID: 19076 RVA: 0x0028F4BC File Offset: 0x0028D6BC
		public ParticlePrefab(ContentXElement element, ContentFile file) : base(file, element.NameAsIdentifier())
		{
			this.Sprites = new List<Sprite>();
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element.GetAttributeBool("drawontop", false))
			{
				this.DrawOrder = ParticleDrawOrder.Foreground;
			}
			if (this.BlendState == ParticleBlendState.Additive && this.DrawOrder == ParticleDrawOrder.Background)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in particle prefab ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(": additive particles cannot be rendered in the background.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "spritesheet") && !(a == "animatedsprite"))
					{
						if (a == "particleemitter" || a == "emitter" || a == "subemitter")
						{
							this.SubEmitters.Add(new ParticleEmitterPrefab(subElement));
						}
					}
					else
					{
						this.Sprites.Add(new SpriteSheet(subElement, "", ""));
					}
				}
				else
				{
					this.Sprites.Add(new Sprite(subElement, "", "", false, 1f));
				}
			}
			if (this.Sprites.Count == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Particle prefab \"");
				defaultInterpolatedStringHandler2.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("\" in the file \"");
				defaultInterpolatedStringHandler2.AppendFormatted<ContentFile>(file);
				defaultInterpolatedStringHandler2.AppendLiteral("\" has no sprites defined!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			if (element.GetAttribute("velocitychangewater") == null)
			{
				this.VelocityChangeWater = this.VelocityChange;
			}
			if (element.GetAttribute("angularvelocity") != null)
			{
				this.AngularVelocityMin = element.GetAttributeFloat("angularvelocity", 0f);
				this.AngularVelocityMax = this.AngularVelocityMin;
			}
			if (element.GetAttribute("startsize") != null)
			{
				string key = "startsize";
				Vector2 vector = Vector2.One;
				this.StartSizeMin = element.GetAttributeVector2(key, vector);
				this.StartSizeMax = this.StartSizeMin;
			}
			if (element.GetAttribute("sizechange") != null)
			{
				string key2 = "sizechange";
				Vector2 vector = Vector2.Zero;
				this.SizeChangeMin = element.GetAttributeVector2(key2, vector);
				this.SizeChangeMax = this.SizeChangeMin;
			}
			if (element.GetAttribute("startrotation") != null)
			{
				this.StartRotationMin = element.GetAttributeFloat("startrotation", 0f);
				this.StartRotationMax = this.StartRotationMin;
			}
			if (this.CollisionRadius <= 0f && this.UseCollision)
			{
				this.CollisionRadius = ((this.Sprites.Count > 0) ? ((float)this.Sprites[0].SourceRect.Width / 2f) : 1f);
			}
		}

		// Token: 0x06004A85 RID: 19077 RVA: 0x0028F7E4 File Offset: 0x0028D9E4
		public Vector2 CalculateEndPosition(Vector2 startPosition, Vector2 velocity)
		{
			return startPosition + velocity * this.LifeTime + 0.5f * this.VelocityChangeDisplay * this.LifeTime * this.LifeTime;
		}

		// Token: 0x06004A86 RID: 19078 RVA: 0x0028F823 File Offset: 0x0028DA23
		public Vector2 CalculateEndSize()
		{
			return this.StartSizeMax + 0.5f * this.SizeChangeMax * this.LifeTime * this.LifeTime;
		}

		// Token: 0x040026C9 RID: 9929
		public static readonly PrefabCollection<ParticlePrefab> Prefabs = new PrefabCollection<ParticlePrefab>();

		// Token: 0x040026CA RID: 9930
		public readonly List<Sprite> Sprites;

		// Token: 0x040026CF RID: 9935
		private float angularVelocityMin;

		// Token: 0x040026D1 RID: 9937
		private float angularVelocityMax;

		// Token: 0x040026D3 RID: 9939
		private float startRotationMin;

		// Token: 0x040026D5 RID: 9941
		private float startRotationMax;

		// Token: 0x040026DA RID: 9946
		private Vector2 velocityChange;

		// Token: 0x040026DC RID: 9948
		private Vector2 velocityChangeWater;

		// Token: 0x040026F5 RID: 9973
		public readonly List<ParticleEmitterPrefab> SubEmitters = new List<ParticleEmitterPrefab>();

		// Token: 0x020011B6 RID: 4534
		[Flags]
		public enum DrawTargetType
		{
			// Token: 0x04005CC2 RID: 23746
			Air = 1,
			// Token: 0x04005CC3 RID: 23747
			Water = 2,
			// Token: 0x04005CC4 RID: 23748
			Both = 3
		}
	}
}
