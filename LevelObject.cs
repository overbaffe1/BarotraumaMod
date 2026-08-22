using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000DA RID: 218
	internal class LevelObject : ILevelRenderableObject, ISpatialEntity, IDamageable, ISerializableEntity
	{
		// Token: 0x17000800 RID: 2048
		// (get) Token: 0x06001E1A RID: 7706 RVA: 0x0012B5AA File Offset: 0x001297AA
		// (set) Token: 0x06001E1B RID: 7707 RVA: 0x0012B5B2 File Offset: 0x001297B2
		public Vector2 CurrentScale { get; private set; } = Vector2.One;

		// Token: 0x17000801 RID: 2049
		// (get) Token: 0x06001E1C RID: 7708 RVA: 0x0012B5BB File Offset: 0x001297BB
		// (set) Token: 0x06001E1D RID: 7709 RVA: 0x0012B5C3 File Offset: 0x001297C3
		public LightSource[] LightSources { get; private set; }

		// Token: 0x17000802 RID: 2050
		// (get) Token: 0x06001E1E RID: 7710 RVA: 0x0012B5CC File Offset: 0x001297CC
		// (set) Token: 0x06001E1F RID: 7711 RVA: 0x0012B5D4 File Offset: 0x001297D4
		public LevelTrigger[] LightSourceTriggers { get; private set; }

		// Token: 0x17000803 RID: 2051
		// (get) Token: 0x06001E20 RID: 7712 RVA: 0x0012B5DD File Offset: 0x001297DD
		// (set) Token: 0x06001E21 RID: 7713 RVA: 0x0012B5E5 File Offset: 0x001297E5
		public ParticleEmitter[] ParticleEmitters { get; private set; }

		// Token: 0x17000804 RID: 2052
		// (get) Token: 0x06001E22 RID: 7714 RVA: 0x0012B5EE File Offset: 0x001297EE
		// (set) Token: 0x06001E23 RID: 7715 RVA: 0x0012B5F6 File Offset: 0x001297F6
		public LevelTrigger[] ParticleEmitterTriggers { get; private set; }

		// Token: 0x17000805 RID: 2053
		// (get) Token: 0x06001E24 RID: 7716 RVA: 0x0012B5FF File Offset: 0x001297FF
		// (set) Token: 0x06001E25 RID: 7717 RVA: 0x0012B607 File Offset: 0x00129807
		public RoundSound[] Sounds { get; private set; }

		// Token: 0x17000806 RID: 2054
		// (get) Token: 0x06001E26 RID: 7718 RVA: 0x0012B610 File Offset: 0x00129810
		// (set) Token: 0x06001E27 RID: 7719 RVA: 0x0012B618 File Offset: 0x00129818
		public SoundChannel[] SoundChannels { get; private set; }

		// Token: 0x17000807 RID: 2055
		// (get) Token: 0x06001E28 RID: 7720 RVA: 0x0012B621 File Offset: 0x00129821
		// (set) Token: 0x06001E29 RID: 7721 RVA: 0x0012B629 File Offset: 0x00129829
		public LevelTrigger[] SoundTriggers { get; private set; }

		// Token: 0x17000808 RID: 2056
		// (get) Token: 0x06001E2A RID: 7722 RVA: 0x0012B632 File Offset: 0x00129832
		// (set) Token: 0x06001E2B RID: 7723 RVA: 0x0012B63A File Offset: 0x0012983A
		public Vector2[,] CurrentSpriteDeformation { get; private set; }

		// Token: 0x17000809 RID: 2057
		// (get) Token: 0x06001E2C RID: 7724 RVA: 0x0012B643 File Offset: 0x00129843
		// (set) Token: 0x06001E2D RID: 7725 RVA: 0x0012B64B File Offset: 0x0012984B
		public bool VisibleOnSonar { get; private set; }

		// Token: 0x1700080A RID: 2058
		// (get) Token: 0x06001E2E RID: 7726 RVA: 0x0012B654 File Offset: 0x00129854
		// (set) Token: 0x06001E2F RID: 7727 RVA: 0x0012B65C File Offset: 0x0012985C
		public float SonarRadius { get; private set; }

		// Token: 0x1700080B RID: 2059
		// (get) Token: 0x06001E30 RID: 7728 RVA: 0x0012B665 File Offset: 0x00129865
		// (set) Token: 0x06001E31 RID: 7729 RVA: 0x0012B66D File Offset: 0x0012986D
		public bool CanBeVisible { get; private set; }

		// Token: 0x06001E32 RID: 7730 RVA: 0x0012B678 File Offset: 0x00129878
		public void Update(float deltaTime, Camera cam)
		{
			this.CurrentRotation = this.Rotation;
			if (this.ActivePrefab.SwingFrequency > 0f)
			{
				this.SwingTimer += deltaTime * this.ActivePrefab.SwingFrequency;
				this.SwingTimer %= 6.2831855f;
				this.CurrentSwingAmount = MathHelper.Lerp(this.CurrentSwingAmount, this.ActivePrefab.SwingAmountRad, deltaTime * 10f);
				if (this.ActivePrefab.SwingAmountRad > 0f)
				{
					this.CurrentRotation += (float)Math.Sin((double)this.SwingTimer) * this.CurrentSwingAmount;
				}
			}
			this.CurrentScale = Vector2.One * this.Scale;
			if (this.ActivePrefab.ScaleOscillationFrequency > 0f)
			{
				this.ScaleOscillateTimer += deltaTime * this.ActivePrefab.ScaleOscillationFrequency;
				this.ScaleOscillateTimer %= 6.2831855f;
				this.CurrentScaleOscillation = Vector2.Lerp(this.CurrentScaleOscillation, this.ActivePrefab.ScaleOscillation, deltaTime * 10f);
				float sin = (float)Math.Sin((double)this.ScaleOscillateTimer);
				this.CurrentScale *= new Vector2(1f + sin * this.CurrentScaleOscillation.X, 1f + sin * this.CurrentScaleOscillation.Y);
			}
			if (this.LightSources != null)
			{
				Vector2 position2D = new Vector2(this.Position.X, this.Position.Y);
				Vector2 camDiff = position2D - cam.WorldViewCenter;
				for (int i = 0; i < this.LightSources.Length; i++)
				{
					if (this.LightSourceTriggers[i] != null)
					{
						this.LightSources[i].Enabled = this.LightSourceTriggers[i].IsTriggered;
					}
					this.LightSources[i].Rotation = -this.CurrentRotation;
					this.LightSources[i].SpriteScale = this.CurrentScale;
					this.LightSources[i].Position = position2D - camDiff * this.Position.Z * 0.0001f;
				}
			}
			if (this.spriteDeformations.Count > 0)
			{
				this.UpdateDeformations(deltaTime);
			}
			if (this.ParticleEmitters != null)
			{
				for (int j = 0; j < this.ParticleEmitters.Length; j++)
				{
					if (this.ParticleEmitterTriggers[j] == null || this.ParticleEmitterTriggers[j].IsTriggered)
					{
						Vector2 emitterPos = this.LocalToWorld(this.Prefab.EmitterPositions[j], 0f);
						this.ParticleEmitters[j].Emit(deltaTime, emitterPos, null, this.ParticleEmitters[j].Prefab.Properties.CopyEntityAngle ? (-this.CurrentRotation + 3.1415927f) : 0f, 0f, 1f, 1f, 1f, null, null, false, null);
					}
				}
			}
			for (int k = 0; k < this.Sounds.Length; k++)
			{
				if (this.Sounds[k] != null)
				{
					if (this.SoundTriggers[k] == null || this.SoundTriggers[k].IsTriggered)
					{
						RoundSound roundSound = this.Sounds[k];
						Vector2 soundPos = this.LocalToWorld(new Vector2(this.Prefab.Sounds[k].Position.X, this.Prefab.Sounds[k].Position.Y), 0f);
						if (Vector2.DistanceSquared(new Vector2(GameMain.SoundManager.ListenerPosition.X, GameMain.SoundManager.ListenerPosition.Y), soundPos) < roundSound.Range * roundSound.Range)
						{
							if (this.SoundChannels[k] == null || !this.SoundChannels[k].IsPlaying)
							{
								this.SoundChannels[k] = roundSound.Sound.Play(roundSound.Volume, roundSound.Range, roundSound.GetRandomFrequencyMultiplier(), soundPos, false);
							}
							if (this.SoundChannels[k] != null)
							{
								this.SoundChannels[k].Position = new Vector3?(new Vector3(soundPos.X, soundPos.Y, 0f));
							}
						}
					}
					else if (this.SoundChannels[k] != null && this.SoundChannels[k].IsPlaying)
					{
						this.SoundChannels[k].FadeOutAndDispose();
						this.SoundChannels[k] = null;
					}
				}
			}
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x0012BB1C File Offset: 0x00129D1C
		private void UpdateDeformations(float deltaTime)
		{
			if (this.ActivePrefab.DeformableSprite == null)
			{
				return;
			}
			foreach (SpriteDeformation deformation in this.spriteDeformations)
			{
				PositionalDeformation positionalDeformation = deformation as PositionalDeformation;
				if (positionalDeformation != null)
				{
					this.UpdatePositionalDeformation(positionalDeformation, deltaTime);
				}
				deformation.Update(deltaTime);
			}
			this.CurrentSpriteDeformation = SpriteDeformation.GetDeformation(this.spriteDeformations, this.ActivePrefab.DeformableSprite.Size, false, false);
			if (this.LightSources != null)
			{
				foreach (LightSource lightSource in this.LightSources)
				{
					if (((lightSource != null) ? lightSource.DeformableLightSprite : null) != null)
					{
						lightSource.DeformableLightSprite.Deform(this.CurrentSpriteDeformation);
					}
				}
			}
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x0012BBFC File Offset: 0x00129DFC
		private void UpdatePositionalDeformation(PositionalDeformation positionalDeformation, float deltaTime)
		{
			if (this.Triggers == null)
			{
				return;
			}
			Matrix matrix = this.ActivePrefab.DeformableSprite.GetTransform(this.Position, this.ActivePrefab.DeformableSprite.Origin, this.CurrentRotation, Vector2.One * this.Scale);
			Matrix rotationMatrix = Matrix.CreateRotationZ(this.CurrentRotation);
			foreach (LevelTrigger trigger in this.Triggers)
			{
				foreach (Entity triggerer in trigger.Triggerers)
				{
					Vector2 moveAmount = triggerer.WorldPosition - trigger.TriggererPosition[triggerer];
					moveAmount = Vector2.Transform(moveAmount, rotationMatrix);
					moveAmount /= this.ActivePrefab.DeformableSprite.Size * this.Scale;
					moveAmount.Y = -moveAmount.Y;
					positionalDeformation.Deform(trigger.WorldPosition, moveAmount, deltaTime, Matrix.Invert(matrix) * Matrix.CreateScale(1f / this.ActivePrefab.DeformableSprite.Size.X, 1f / this.ActivePrefab.DeformableSprite.Size.Y, 1f));
				}
			}
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x0012BDAC File Offset: 0x00129FAC
		public void ClientRead(IReadMessage msg)
		{
			if (this.Triggers == null)
			{
				return;
			}
			if (this.Prefab.TakeLevelWallDamage)
			{
				float newHealth = msg.ReadRangedSingle(0f, this.Prefab.Health, 8);
				this.AddDamage(this.Health - newHealth, 1f, null, true);
			}
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				if (this.Triggers[i].UseNetworkSyncing)
				{
					this.Triggers[i].ClientRead(msg);
				}
			}
		}

		// Token: 0x1700080C RID: 2060
		// (get) Token: 0x06001E36 RID: 7734 RVA: 0x0012BE37 File Offset: 0x0012A037
		// (set) Token: 0x06001E37 RID: 7735 RVA: 0x0012BE3F File Offset: 0x0012A03F
		public Vector3 Position { get; set; }

		// Token: 0x1700080D RID: 2061
		// (get) Token: 0x06001E38 RID: 7736 RVA: 0x0012BE48 File Offset: 0x0012A048
		// (set) Token: 0x06001E39 RID: 7737 RVA: 0x0012BE50 File Offset: 0x0012A050
		public PhysicsBody PhysicsBody { get; private set; }

		// Token: 0x1700080E RID: 2062
		// (get) Token: 0x06001E3A RID: 7738 RVA: 0x0012BE59 File Offset: 0x0012A059
		// (set) Token: 0x06001E3B RID: 7739 RVA: 0x0012BE61 File Offset: 0x0012A061
		public List<LevelTrigger> Triggers { get; private set; }

		// Token: 0x1700080F RID: 2063
		// (get) Token: 0x06001E3C RID: 7740 RVA: 0x0012BE6A File Offset: 0x0012A06A
		// (set) Token: 0x06001E3D RID: 7741 RVA: 0x0012BEAA File Offset: 0x0012A0AA
		public bool NeedsNetworkSyncing
		{
			get
			{
				if (this.tookDamage)
				{
					return true;
				}
				if (this.Triggers != null)
				{
					return this.Triggers.Any((LevelTrigger t) => t.NeedsNetworkSyncing);
				}
				return false;
			}
			set
			{
				if (this.Triggers == null)
				{
					return;
				}
				this.Triggers.ForEach(delegate(LevelTrigger t)
				{
					t.NeedsNetworkSyncing = false;
				});
				this.tookDamage = false;
			}
		}

		// Token: 0x17000810 RID: 2064
		// (get) Token: 0x06001E3E RID: 7742 RVA: 0x0012BEE6 File Offset: 0x0012A0E6
		// (set) Token: 0x06001E3F RID: 7743 RVA: 0x0012BEEE File Offset: 0x0012A0EE
		public bool NeedsUpdate { get; private set; }

		// Token: 0x17000811 RID: 2065
		// (get) Token: 0x06001E40 RID: 7744 RVA: 0x0012BEF7 File Offset: 0x0012A0F7
		// (set) Token: 0x06001E41 RID: 7745 RVA: 0x0012BEFF File Offset: 0x0012A0FF
		public float Health { get; private set; }

		// Token: 0x17000812 RID: 2066
		// (get) Token: 0x06001E42 RID: 7746 RVA: 0x0012BF08 File Offset: 0x0012A108
		public Sprite Sprite
		{
			get
			{
				LevelObjectPrefab activePrefab = this.ActivePrefab;
				LevelObjectPrefab prefab = (activePrefab != null && activePrefab.Sprites.Count > 0) ? this.ActivePrefab : this.Prefab;
				if (this.spriteIndex >= 0 && prefab.Sprites.Count != 0)
				{
					return prefab.Sprites[this.spriteIndex % prefab.Sprites.Count];
				}
				return null;
			}
		}

		// Token: 0x17000813 RID: 2067
		// (get) Token: 0x06001E43 RID: 7747 RVA: 0x0012BF75 File Offset: 0x0012A175
		Vector2 ISpatialEntity.Position
		{
			get
			{
				return new Vector2(this.Position.X, this.Position.Y);
			}
		}

		// Token: 0x17000814 RID: 2068
		// (get) Token: 0x06001E44 RID: 7748 RVA: 0x0012BF92 File Offset: 0x0012A192
		public Vector2 WorldPosition
		{
			get
			{
				return new Vector2(this.Position.X, this.Position.Y);
			}
		}

		// Token: 0x17000815 RID: 2069
		// (get) Token: 0x06001E45 RID: 7749 RVA: 0x0012BFAF File Offset: 0x0012A1AF
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.WorldPosition);
			}
		}

		// Token: 0x17000816 RID: 2070
		// (get) Token: 0x06001E46 RID: 7750 RVA: 0x0012BFBC File Offset: 0x0012A1BC
		public Submarine Submarine
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000817 RID: 2071
		// (get) Token: 0x06001E47 RID: 7751 RVA: 0x0012BFBF File Offset: 0x0012A1BF
		public string Name
		{
			get
			{
				LevelObjectPrefab prefab = this.Prefab;
				return ((prefab != null) ? prefab.Name : null) ?? "LevelObject (null)";
			}
		}

		// Token: 0x17000818 RID: 2072
		// (get) Token: 0x06001E48 RID: 7752 RVA: 0x0012BFDC File Offset: 0x0012A1DC
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; } = new Dictionary<Identifier, SerializableProperty>();

		// Token: 0x06001E49 RID: 7753 RVA: 0x0012BFE4 File Offset: 0x0012A1E4
		public LevelObject(LevelObjectPrefab prefab, Vector3 position, float scale, float rotation = 0f)
		{
			this.Prefab = prefab;
			this.ActivePrefab = prefab;
			this.Position = position;
			this.Scale = scale;
			this.Rotation = rotation;
			this.Health = prefab.Health;
			this.spriteIndex = (this.ActivePrefab.Sprites.Any<Sprite>() ? Rand.Int(this.ActivePrefab.Sprites.Count, Rand.RandSync.ServerAndClient) : -1);
			if (this.Sprite != null && prefab.SpriteSpecificPhysicsBodyElements.ContainsKey(this.Sprite))
			{
				this.PhysicsBody = new PhysicsBody(prefab.SpriteSpecificPhysicsBodyElements[this.Sprite], ConvertUnits.ToSimUnits(new Vector2(position.X, position.Y)), this.Scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, true);
			}
			else if (prefab.PhysicsBodyElement != null)
			{
				this.PhysicsBody = new PhysicsBody(prefab.PhysicsBodyElement, ConvertUnits.ToSimUnits(new Vector2(position.X, position.Y)), this.Scale, null, Category.Cat5, Category.Cat1 | Category.Cat3 | Category.Cat8, true);
			}
			if (this.PhysicsBody != null)
			{
				this.PhysicsBody.UserData = this;
				this.PhysicsBody.SetTransformIgnoreContacts(this.PhysicsBody.SimPosition, -this.Rotation, true);
				this.PhysicsBody.BodyType = BodyType.Static;
				this.PhysicsBody.CollisionCategories = Category.Cat8;
				this.PhysicsBody.CollidesWith = (this.Prefab.TakeLevelWallDamage ? (Category.Cat1 | Category.Cat2 | Category.Cat7) : (Category.Cat1 | Category.Cat2));
			}
			foreach (ContentXElement triggerElement in prefab.LevelTriggerElements)
			{
				if (this.Triggers == null)
				{
					this.Triggers = new List<LevelTrigger>();
				}
				ContentXElement contentXElement = triggerElement;
				string key = "position";
				Vector2 zero = Vector2.Zero;
				Vector2 triggerPosition = contentXElement.GetAttributeVector2(key, zero) * scale;
				if (rotation != 0f)
				{
					float ca = (float)Math.Cos((double)rotation);
					float sa = (float)Math.Sin((double)rotation);
					triggerPosition = new Vector2(ca * triggerPosition.X + sa * triggerPosition.Y, -sa * triggerPosition.X + ca * triggerPosition.Y);
				}
				LevelTrigger newTrigger = new LevelTrigger(triggerElement, new Vector2(position.X, position.Y) + triggerPosition, -rotation, scale, prefab.Name);
				if (newTrigger.PhysicsBody != null)
				{
					newTrigger.PhysicsBody.UserData = this;
				}
				int parentTriggerIndex = prefab.LevelTriggerElements.IndexOf(triggerElement.Parent);
				if (parentTriggerIndex > -1)
				{
					newTrigger.ParentTrigger = this.Triggers[parentTriggerIndex];
				}
				this.Triggers.Add(newTrigger);
			}
			if (this.spriteIndex == -1)
			{
				foreach (LevelObjectPrefab overrideProperties in prefab.OverrideProperties)
				{
					if (overrideProperties != null && overrideProperties.Sprites.Count > 0)
					{
						this.spriteIndex = Rand.Int(overrideProperties.Sprites.Count, Rand.RandSync.ServerAndClient);
						break;
					}
				}
			}
			this.NeedsUpdate = (this.NeedsNetworkSyncing || (this.Triggers != null && this.Triggers.Any<LevelTrigger>()) || this.Prefab.PhysicsBodyTriggerIndex > -1);
			this.InitProjSpecific();
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x0012C39C File Offset: 0x0012A59C
		private void InitProjSpecific()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.EnsureLazyLoaded(false);
			}
			DeformableSprite deformableSprite = this.Prefab.DeformableSprite;
			if (deformableSprite != null)
			{
				deformableSprite.EnsureLazyLoaded();
			}
			this.CurrentSwingAmount = this.Prefab.SwingAmountRad;
			this.CurrentScaleOscillation = this.Prefab.ScaleOscillation;
			this.SwingTimer = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
			this.ScaleOscillateTimer = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
			if (this.Prefab.ParticleEmitterPrefabs != null)
			{
				this.ParticleEmitters = new ParticleEmitter[this.Prefab.ParticleEmitterPrefabs.Count];
				this.ParticleEmitterTriggers = new LevelTrigger[this.Prefab.ParticleEmitterPrefabs.Count];
				for (int i = 0; i < this.Prefab.ParticleEmitterPrefabs.Count; i++)
				{
					this.ParticleEmitters[i] = new ParticleEmitter(this.Prefab.ParticleEmitterPrefabs[i]);
					this.ParticleEmitterTriggers[i] = ((this.Prefab.ParticleEmitterTriggerIndex[i] > -1) ? this.Triggers[this.Prefab.ParticleEmitterTriggerIndex[i]] : null);
				}
			}
			if (this.Prefab.LightSourceParams != null && this.Prefab.LightSourceParams.Count > 0)
			{
				this.LightSources = new LightSource[this.Prefab.LightSourceParams.Count];
				this.LightSourceTriggers = new LevelTrigger[this.Prefab.LightSourceParams.Count];
				for (int j = 0; j < this.Prefab.LightSourceParams.Count; j++)
				{
					this.LightSources[j] = new LightSource(this.Prefab.LightSourceParams[j])
					{
						Position = new Vector2(this.Position.X, this.Position.Y),
						IsBackground = true
					};
					this.LightSourceTriggers[j] = ((this.Prefab.LightSourceTriggerIndex[j] > -1) ? this.Triggers[this.Prefab.LightSourceTriggerIndex[j]] : null);
				}
			}
			this.Sounds = new RoundSound[this.Prefab.Sounds.Count];
			this.SoundChannels = new SoundChannel[this.Prefab.Sounds.Count];
			this.SoundTriggers = new LevelTrigger[this.Prefab.Sounds.Count];
			for (int k = 0; k < this.Prefab.Sounds.Count; k++)
			{
				this.Sounds[k] = RoundSound.Load(this.Prefab.Sounds[k].SoundElement);
				this.SoundTriggers[k] = ((this.Prefab.Sounds[k].TriggerIndex > -1) ? this.Triggers[this.Prefab.Sounds[k].TriggerIndex] : null);
			}
			int l = 0;
			foreach (XElement subElement in this.Prefab.Config.Elements())
			{
				if (subElement.Name.ToString().Equals("deformablesprite", StringComparison.OrdinalIgnoreCase))
				{
					foreach (XElement animationElement in subElement.Elements())
					{
						SpriteDeformation newDeformation = SpriteDeformation.Load(animationElement, this.Prefab.Name);
						if (newDeformation != null)
						{
							newDeformation.Params = this.Prefab.SpriteDeformations[l].Params;
							this.spriteDeformations.Add(newDeformation);
							l++;
						}
					}
				}
			}
			bool visibleOnSonar;
			if (this.Prefab.SonarDisruption <= 0f)
			{
				if (!this.Prefab.OverrideProperties.Any((LevelObjectPrefab p) => p != null && p.SonarDisruption > 0f))
				{
					if (this.Triggers != null)
					{
						visibleOnSonar = this.Triggers.Any((LevelTrigger t) => (!MathUtils.NearlyEqual(t.Force, Vector2.Zero, 0.0001f) && t.ForceMode != LevelTrigger.TriggerForceMode.LimitVelocity) || !t.InfectIdentifier.IsEmpty);
						goto IL_44E;
					}
					visibleOnSonar = false;
					goto IL_44E;
				}
			}
			visibleOnSonar = true;
			IL_44E:
			this.VisibleOnSonar = visibleOnSonar;
			if (this.VisibleOnSonar && this.Triggers.Any<LevelTrigger>())
			{
				this.SonarRadius = (from t in this.Triggers
				select t.ColliderRadius * 1.5f).Max();
			}
			bool canBeVisible;
			if (this.Sprite == null && this.Prefab.DeformableSprite == null)
			{
				ParticleEmitter[] particleEmitters = this.ParticleEmitters;
				if (particleEmitters == null || particleEmitters.Length <= 0)
				{
					if (GameMain.DebugDraw)
					{
						List<LevelTrigger> triggers = this.Triggers;
						if (triggers != null && triggers.Count > 0)
						{
							goto IL_514;
						}
					}
					canBeVisible = this.Prefab.OverrideProperties.Any((LevelObjectPrefab p) => p != null && (p.Sprites.Any<Sprite>() || p.DeformableSprite != null));
					goto IL_515;
				}
			}
			IL_514:
			canBeVisible = true;
			IL_515:
			this.CanBeVisible = canBeVisible;
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x0012C8E0 File Offset: 0x0012AAE0
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			if (this.Health <= 0f)
			{
				return new AttackResult(0f, null);
			}
			float damage = 0f;
			if (this.Prefab.TakeLevelWallDamage)
			{
				damage += attack.GetLevelWallDamage(deltaTime);
			}
			damage = Math.Max(this.Health, damage);
			this.AddDamage(damage, deltaTime, attacker, false);
			return new AttackResult(damage, null);
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x0012C944 File Offset: 0x0012AB44
		public void AddDamage(float damage, float deltaTime, Entity attacker, bool isNetworkEvent = false)
		{
			if (this.Health <= 0f)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && !isNetworkEvent)
			{
				return;
			}
			this.tookDamage |= !MathUtils.NearlyEqual(damage, 0f, 0.0001f);
			this.Health -= damage;
			if (this.Health <= 0f)
			{
				GameSession gameSession = GameMain.GameSession;
				bool flag;
				if (gameSession == null)
				{
					flag = (null != null);
				}
				else
				{
					Level level = gameSession.Level;
					flag = (((level != null) ? level.LevelObjectManager : null) != null);
				}
				if (flag)
				{
					GameMain.GameSession.Level.LevelObjectManager.ForceRefreshVisibleObjects = true;
				}
				if (this.PhysicsBody != null)
				{
					this.PhysicsBody.Enabled = false;
				}
				foreach (LevelTrigger trigger in this.Triggers)
				{
					trigger.PhysicsBody.Enabled = false;
					foreach (StatusEffect effect in trigger.StatusEffects)
					{
						if (effect.type == ActionType.OnBroken)
						{
							effect.Apply(effect.type, deltaTime, attacker, this, new Vector2?(this.WorldPosition));
						}
					}
				}
			}
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x0012CAA4 File Offset: 0x0012ACA4
		public Vector2 LocalToWorld(Vector2 localPosition, float swingState = 0f)
		{
			Vector2 emitterPos = localPosition * this.Scale;
			if (this.Rotation != 0f || this.Prefab.SwingAmountRad != 0f)
			{
				float rot = this.Rotation + swingState * this.Prefab.SwingAmountRad;
				float ca = (float)Math.Cos((double)rot);
				float sa = (float)Math.Sin((double)rot);
				emitterPos = new Vector2(ca * emitterPos.X + sa * emitterPos.Y, -sa * emitterPos.X + ca * emitterPos.Y);
			}
			return new Vector2(this.Position.X, this.Position.Y) + emitterPos;
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x0012CB4F File Offset: 0x0012AD4F
		public void Remove()
		{
			this.RemoveProjSpecific();
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x0012CB58 File Offset: 0x0012AD58
		private void RemoveProjSpecific()
		{
			for (int i = 0; i < this.Sounds.Length; i++)
			{
				SoundChannel soundChannel = this.SoundChannels[i];
				if (soundChannel != null)
				{
					soundChannel.Dispose();
				}
				this.SoundChannels[i] = null;
			}
			if (this.LightSources != null)
			{
				for (int j = 0; j < this.LightSources.Length; j++)
				{
					this.LightSources[j].Remove();
				}
				this.LightSources = null;
			}
		}

		// Token: 0x06001E50 RID: 7760 RVA: 0x0012CBC3 File Offset: 0x0012ADC3
		public override string ToString()
		{
			return "LevelObject (" + this.ActivePrefab.Name + ")";
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x0012CBE0 File Offset: 0x0012ADE0
		public void ServerWrite(IWriteMessage msg, Client c)
		{
			if (this.Triggers == null)
			{
				return;
			}
			if (this.Prefab.TakeLevelWallDamage)
			{
				msg.WriteRangedSingle(MathHelper.Clamp(this.Health, 0f, this.Prefab.Health), 0f, this.Prefab.Health, 8);
			}
			for (int i = 0; i < this.Triggers.Count; i++)
			{
				if (this.Triggers[i].UseNetworkSyncing)
				{
					this.Triggers[i].ServerWrite(msg, c);
				}
			}
		}

		// Token: 0x04000F58 RID: 3928
		public float SwingTimer;

		// Token: 0x04000F59 RID: 3929
		public float ScaleOscillateTimer;

		// Token: 0x04000F5A RID: 3930
		public float CurrentSwingAmount;

		// Token: 0x04000F5B RID: 3931
		public Vector2 CurrentScaleOscillation;

		// Token: 0x04000F5C RID: 3932
		public float CurrentRotation;

		// Token: 0x04000F5D RID: 3933
		private readonly List<SpriteDeformation> spriteDeformations = new List<SpriteDeformation>();

		// Token: 0x04000F6A RID: 3946
		public readonly LevelObjectPrefab Prefab;

		// Token: 0x04000F6C RID: 3948
		public float NetworkUpdateTimer;

		// Token: 0x04000F6D RID: 3949
		public float Scale;

		// Token: 0x04000F6E RID: 3950
		public float Rotation;

		// Token: 0x04000F6F RID: 3951
		private int spriteIndex;

		// Token: 0x04000F70 RID: 3952
		protected bool tookDamage;

		// Token: 0x04000F71 RID: 3953
		public LevelObjectPrefab ActivePrefab;

		// Token: 0x04000F77 RID: 3959
		public Level.Cave ParentCave;
	}
}
