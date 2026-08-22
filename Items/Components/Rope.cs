using System;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D4 RID: 1492
	internal class Rope : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x17001829 RID: 6185
		// (get) Token: 0x06005FA0 RID: 24480 RVA: 0x0031D714 File Offset: 0x0031B914
		// (set) Token: 0x06005FA1 RID: 24481 RVA: 0x0031D71C File Offset: 0x0031B91C
		[Serialize(5, IsPropertySaveable.No, "", "", false)]
		public int SpriteWidth { get; set; }

		// Token: 0x1700182A RID: 6186
		// (get) Token: 0x06005FA2 RID: 24482 RVA: 0x0031D725 File Offset: 0x0031B925
		// (set) Token: 0x06005FA3 RID: 24483 RVA: 0x0031D72D File Offset: 0x0031B92D
		[Serialize("255,255,255,255", IsPropertySaveable.No, "", "", false)]
		public Color SpriteColor { get; set; }

		// Token: 0x1700182B RID: 6187
		// (get) Token: 0x06005FA4 RID: 24484 RVA: 0x0031D736 File Offset: 0x0031B936
		// (set) Token: 0x06005FA5 RID: 24485 RVA: 0x0031D73E File Offset: 0x0031B93E
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool Tile { get; set; }

		// Token: 0x1700182C RID: 6188
		// (get) Token: 0x06005FA6 RID: 24486 RVA: 0x0031D747 File Offset: 0x0031B947
		// (set) Token: 0x06005FA7 RID: 24487 RVA: 0x0031D74F File Offset: 0x0031B94F
		[Serialize("0.5,0.5", IsPropertySaveable.No, "", "", false)]
		public Vector2 Origin { get; set; }

		// Token: 0x1700182D RID: 6189
		// (get) Token: 0x06005FA8 RID: 24488 RVA: 0x0031D758 File Offset: 0x0031B958
		// (set) Token: 0x06005FA9 RID: 24489 RVA: 0x0031D760 File Offset: 0x0031B960
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool BreakFromMiddle { get; set; }

		// Token: 0x1700182E RID: 6190
		// (get) Token: 0x06005FAA RID: 24490 RVA: 0x0031D76C File Offset: 0x0031B96C
		public Vector2 DrawSize
		{
			get
			{
				if (this.target == null || this.source == null)
				{
					return Vector2.Zero;
				}
				Vector2 sourcePos = this.GetSourcePos(false);
				return new Vector2(Math.Abs(this.target.DrawPosition.X - sourcePos.X), Math.Abs(this.target.DrawPosition.Y - sourcePos.Y)) * 2.2f;
			}
		}

		// Token: 0x1700182F RID: 6191
		// (get) Token: 0x06005FAB RID: 24491 RVA: 0x0031D7DE File Offset: 0x0031B9DE
		// (set) Token: 0x06005FAC RID: 24492 RVA: 0x0031D7E6 File Offset: 0x0031B9E6
		[Serialize("1.0, 1.0", IsPropertySaveable.No, "When reeling in, the pitch slides from X to Y, depending on the length of the rope.", "", false)]
		public Vector2 ReelSoundPitchSlide
		{
			get
			{
				return this._reelSoundPitchSlide;
			}
			set
			{
				this._reelSoundPitchSlide = new Vector2(Math.Max(value.X, 0.25f), Math.Min(value.Y, 4f));
			}
		}

		// Token: 0x06005FAD RID: 24493 RVA: 0x0031D814 File Offset: 0x0031BA14
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.target == null || this.target.Removed)
			{
				return;
			}
			if (this.target.ParentInventory != null)
			{
				return;
			}
			Limb limb = this.source as Limb;
			if (limb != null && limb.Removed)
			{
				return;
			}
			Entity e = this.source as Entity;
			if (e != null && e.Removed)
			{
				return;
			}
			Vector2 startPos = this.GetSourcePos(true);
			startPos.Y = -startPos.Y;
			Item item = this.source as Item;
			Turret turret = (item != null) ? item.GetComponent<Turret>() : null;
			if (turret != null)
			{
				if (turret.BarrelSprite != null)
				{
					startPos += new Vector2((float)Math.Cos((double)turret.Rotation), (float)Math.Sin((double)turret.Rotation)) * turret.BarrelSprite.size.Y * turret.BarrelSprite.RelativeOrigin.Y * turret.Item.Scale * this.BarrelLengthMultiplier;
				}
				startPos -= turret.GetRecoilOffset();
			}
			Vector2 endPos = new Vector2(this.target.DrawPosition.X, this.target.DrawPosition.Y);
			Vector2 flippedPos = this.target.Sprite.size * this.target.Scale * (this.Origin - new Vector2(0.5f));
			if (this.target.body.Dir < 0f)
			{
				flippedPos.X = -flippedPos.X;
			}
			endPos += Vector2.Transform(flippedPos, Matrix.CreateRotationZ(this.target.body.Rotation));
			endPos.Y = -endPos.Y;
			if (this.Snapped)
			{
				float snapState = 1f - this.snapTimer / this.SnapAnimDuration;
				Vector2 diff = this.target.DrawPosition - new Vector2(startPos.X, -startPos.Y);
				diff.Y = -diff.Y;
				int width = (int)((float)this.SpriteWidth * snapState);
				if ((float)width > 0f)
				{
					float positionMultiplier = snapState;
					if (this.BreakFromMiddle)
					{
						positionMultiplier /= 2f;
						this.DrawRope(spriteBatch, endPos - diff * positionMultiplier, endPos, width, null);
					}
					this.DrawRope(spriteBatch, startPos, startPos + diff * positionMultiplier, width, null);
				}
			}
			else
			{
				this.DrawRope(spriteBatch, startPos, endPos, this.SpriteWidth, null);
			}
			if (this.startSprite != null || this.endSprite != null)
			{
				Vector2 dir = endPos - startPos;
				float angle = (float)Math.Atan2((double)dir.Y, (double)dir.X);
				if (this.startSprite != null)
				{
					float depth = Math.Min(this.item.GetDrawDepth() + (this.startSprite.Depth - this.item.Sprite.Depth), 0.999f);
					Sprite sprite = this.startSprite;
					if (sprite != null)
					{
						sprite.Draw(spriteBatch, startPos, overrideColor ?? this.SpriteColor, angle, 1f, SpriteEffects.None, new float?(depth));
					}
				}
				if (this.endSprite != null && (!this.Snapped || this.BreakFromMiddle))
				{
					float depth2 = Math.Min(this.item.GetDrawDepth() + (this.endSprite.Depth - this.item.Sprite.Depth), 0.999f);
					Sprite sprite2 = this.endSprite;
					if (sprite2 == null)
					{
						return;
					}
					sprite2.Draw(spriteBatch, endPos, overrideColor ?? this.SpriteColor, angle, 1f, SpriteEffects.None, new float?(depth2));
				}
			}
		}

		// Token: 0x06005FAE RID: 24494 RVA: 0x0031DC08 File Offset: 0x0031BE08
		private void DrawRope(SpriteBatch spriteBatch, Vector2 startPos, Vector2 endPos, int width, Color? overrideColor = null)
		{
			float depth = (this.sprite == null) ? (this.item.Sprite.Depth + 0.001f) : Math.Min(this.item.GetDrawDepth() + (this.sprite.Depth - this.item.Sprite.Depth), 0.999f);
			Sprite sprite = this.sprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				GUI.DrawLine(spriteBatch, startPos, endPos, overrideColor ?? this.SpriteColor, depth, (float)width);
				return;
			}
			if (this.Tile)
			{
				float length = Vector2.Distance(startPos, endPos);
				Vector2 dir = (endPos - startPos) / length;
				float x;
				for (x = 0f; x <= length - this.sprite.size.X; x += this.sprite.size.X)
				{
					GUI.DrawLine(spriteBatch, this.sprite, startPos + dir * (x - 5f), startPos + dir * (x + this.sprite.size.X), overrideColor ?? this.SpriteColor, depth, width);
				}
				float leftOver = length - x;
				if (leftOver > 0f)
				{
					GUI.DrawLine(spriteBatch, this.sprite, startPos + dir * (x - 5f), endPos, overrideColor ?? this.SpriteColor, depth, width);
					return;
				}
			}
			else
			{
				GUI.DrawLine(spriteBatch, this.sprite, startPos, endPos, overrideColor ?? this.SpriteColor, depth, width);
			}
		}

		// Token: 0x06005FAF RID: 24495 RVA: 0x0031DDD4 File Offset: 0x0031BFD4
		private void PlaySound(RoundSound sound, Vector2 position)
		{
			if (sound == null)
			{
				return;
			}
			if (sound == this.reelSound)
			{
				SoundChannel soundChannel = this.reelSoundChannel;
				if (soundChannel != null && soundChannel.IsPlaying)
				{
					this.reelSoundChannel.Position = new Vector3?(new Vector3(position, 0f));
					this.reelSoundChannel.Gain = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(this.MinPullDistance, this.MaxLength, MathUtils.Pow(this.currentRopeLength, 1.5f)));
					this.reelSoundChannel.FrequencyMultiplier = MathHelper.Lerp(this.ReelSoundPitchSlide.X, this.ReelSoundPitchSlide.Y, MathUtils.InverseLerp(this.MinPullDistance, this.MaxLength, this.currentRopeLength));
					return;
				}
				this.reelSoundChannel = SoundPlayer.PlaySound(sound, position, null, null);
				if (this.reelSoundChannel != null)
				{
					this.reelSoundChannel.Looping = true;
					return;
				}
			}
			else
			{
				SoundPlayer.PlaySound(sound, position, null, null);
			}
		}

		// Token: 0x06005FB0 RID: 24496 RVA: 0x0031DED8 File Offset: 0x0031C0D8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.snapped = msg.ReadBoolean();
			if (!this.snapped)
			{
				ushort targetId = msg.ReadUInt16();
				ushort sourceId = msg.ReadUInt16();
				byte limbIndex = msg.ReadByte();
				Item target = Entity.FindEntityByID(targetId) as Item;
				if (target == null)
				{
					return;
				}
				Entity source = Entity.FindEntityByID(sourceId);
				Entity entity = source;
				Character sourceCharacter = entity as Character;
				ISpatialEntity spatialEntity;
				if (sourceCharacter == null)
				{
					spatialEntity = entity;
					if (spatialEntity == null)
					{
						return;
					}
				}
				else
				{
					if (limbIndex >= 0 && (int)limbIndex < sourceCharacter.AnimController.Limbs.Length)
					{
						Limb sourceLimb = sourceCharacter.AnimController.Limbs[(int)limbIndex];
						this.Attach(sourceLimb, target);
						sourceCharacter.AnimController.DragWithRope();
						return;
					}
					spatialEntity = entity;
				}
				this.Attach(spatialEntity, target);
			}
		}

		// Token: 0x06005FB1 RID: 24497 RVA: 0x0031DF90 File Offset: 0x0031C190
		protected override void RemoveComponentSpecific()
		{
			Sprite sprite = this.sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.sprite = null;
			Sprite sprite2 = this.startSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			this.startSprite = null;
			Sprite sprite3 = this.endSprite;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			this.endSprite = null;
			SoundChannel soundChannel = this.reelSoundChannel;
			if (soundChannel != null)
			{
				soundChannel.FadeOutAndDispose();
			}
			this.reelSoundChannel = null;
		}

		// Token: 0x06005FB2 RID: 24498 RVA: 0x0031E000 File Offset: 0x0031C200
		private void SetSource(ISpatialEntity source)
		{
			this.source = source;
			Limb sourceLimb = source as Limb;
			if (sourceLimb != null)
			{
				sourceLimb.AttachedRope = this;
				float offset = sourceLimb.Params.GetSpriteOrientation() - 1.5707964f;
				this.launchDir = new Vector2?(VectorExtensions.Forward(sourceLimb.body.TransformedRotation - offset * sourceLimb.character.AnimController.Dir, 1f));
			}
		}

		// Token: 0x06005FB3 RID: 24499 RVA: 0x0031E06C File Offset: 0x0031C26C
		private void ResetSource()
		{
			Limb sourceLimb = this.source as Limb;
			if (sourceLimb != null && sourceLimb.AttachedRope == this)
			{
				sourceLimb.AttachedRope = null;
			}
			this.source = null;
		}

		// Token: 0x17001830 RID: 6192
		// (get) Token: 0x06005FB4 RID: 24500 RVA: 0x0031E09F File Offset: 0x0031C29F
		// (set) Token: 0x06005FB5 RID: 24501 RVA: 0x0031E0A7 File Offset: 0x0031C2A7
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SnapAnimDuration { get; set; }

		// Token: 0x17001831 RID: 6193
		// (get) Token: 0x06005FB6 RID: 24502 RVA: 0x0031E0B0 File Offset: 0x0031C2B0
		// (set) Token: 0x06005FB7 RID: 24503 RVA: 0x0031E0B8 File Offset: 0x0031C2B8
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the projectile the rope is attached to.", "", false)]
		public float ProjectilePullForce { get; set; }

		// Token: 0x17001832 RID: 6194
		// (get) Token: 0x06005FB8 RID: 24504 RVA: 0x0031E0C1 File Offset: 0x0031C2C1
		// (set) Token: 0x06005FB9 RID: 24505 RVA: 0x0031E0C9 File Offset: 0x0031C2C9
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the target the rope is attached to.", "", false)]
		public float TargetPullForce { get; set; }

		// Token: 0x17001833 RID: 6195
		// (get) Token: 0x06005FBA RID: 24506 RVA: 0x0031E0D2 File Offset: 0x0031C2D2
		// (set) Token: 0x06005FBB RID: 24507 RVA: 0x0031E0DA File Offset: 0x0031C2DA
		[Serialize(0f, IsPropertySaveable.No, "How much force is applied to pull the source the rope is attached to.", "", false)]
		public float SourcePullForce { get; set; }

		// Token: 0x17001834 RID: 6196
		// (get) Token: 0x06005FBC RID: 24508 RVA: 0x0031E0E3 File Offset: 0x0031C2E3
		// (set) Token: 0x06005FBD RID: 24509 RVA: 0x0031E0EB File Offset: 0x0031C2EB
		[Serialize(1000f, IsPropertySaveable.No, "How far the source item can be from the projectile until the rope breaks.", "", false)]
		public float MaxLength { get; set; }

		// Token: 0x17001835 RID: 6197
		// (get) Token: 0x06005FBE RID: 24510 RVA: 0x0031E0F4 File Offset: 0x0031C2F4
		// (set) Token: 0x06005FBF RID: 24511 RVA: 0x0031E0FC File Offset: 0x0031C2FC
		[Serialize(200f, IsPropertySaveable.No, "At which distance the user stops pulling the target?", "", false)]
		public float MinPullDistance { get; set; }

		// Token: 0x17001836 RID: 6198
		// (get) Token: 0x06005FC0 RID: 24512 RVA: 0x0031E105 File Offset: 0x0031C305
		// (set) Token: 0x06005FC1 RID: 24513 RVA: 0x0031E10D File Offset: 0x0031C30D
		[Serialize(360f, IsPropertySaveable.No, "The maximum angle from the source to the target until the rope breaks.", "", false)]
		public float MaxAngle { get; set; }

		// Token: 0x17001837 RID: 6199
		// (get) Token: 0x06005FC2 RID: 24514 RVA: 0x0031E116 File Offset: 0x0031C316
		// (set) Token: 0x06005FC3 RID: 24515 RVA: 0x0031E11E File Offset: 0x0031C31E
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when it collides with a structure/submarine (if not, it will just go through it).", "", false)]
		public bool SnapOnCollision { get; set; }

		// Token: 0x17001838 RID: 6200
		// (get) Token: 0x06005FC4 RID: 24516 RVA: 0x0031E127 File Offset: 0x0031C327
		// (set) Token: 0x06005FC5 RID: 24517 RVA: 0x0031E12F File Offset: 0x0031C32F
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when the character drops the aim?", "", false)]
		public bool SnapWhenNotAimed { get; set; }

		// Token: 0x17001839 RID: 6201
		// (get) Token: 0x06005FC6 RID: 24518 RVA: 0x0031E138 File Offset: 0x0031C338
		// (set) Token: 0x06005FC7 RID: 24519 RVA: 0x0031E140 File Offset: 0x0031C340
		[Serialize(true, IsPropertySaveable.No, "Should the rope snap when the weapon it was fired from is fired again? I.e. can there be multiple ropes coming from the weapon at the same time?", "", false)]
		public bool SnapWhenWeaponFiredAgain { get; set; }

		// Token: 0x1700183A RID: 6202
		// (get) Token: 0x06005FC8 RID: 24520 RVA: 0x0031E149 File Offset: 0x0031C349
		// (set) Token: 0x06005FC9 RID: 24521 RVA: 0x0031E151 File Offset: 0x0031C351
		[Serialize(0.9f, IsPropertySaveable.No, "Multiplier for the length of the barrel when determining where the rope should start from.", "", false)]
		public float BarrelLengthMultiplier { get; set; }

		// Token: 0x1700183B RID: 6203
		// (get) Token: 0x06005FCA RID: 24522 RVA: 0x0031E15A File Offset: 0x0031C35A
		// (set) Token: 0x06005FCB RID: 24523 RVA: 0x0031E162 File Offset: 0x0031C362
		[Serialize(30f, IsPropertySaveable.No, "How much mass is required for the target to pull the source towards it. Static and kinematic targets are always treated heavy enough.", "", false)]
		public float TargetMinMass { get; set; }

		// Token: 0x1700183C RID: 6204
		// (get) Token: 0x06005FCC RID: 24524 RVA: 0x0031E16B File Offset: 0x0031C36B
		// (set) Token: 0x06005FCD RID: 24525 RVA: 0x0031E173 File Offset: 0x0031C373
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool LerpForces { get; set; }

		// Token: 0x1700183D RID: 6205
		// (get) Token: 0x06005FCE RID: 24526 RVA: 0x0031E17C File Offset: 0x0031C37C
		// (set) Token: 0x06005FCF RID: 24527 RVA: 0x0031E184 File Offset: 0x0031C384
		[Serialize(true, IsPropertySaveable.No, "Should the force be dynamically adjusted to make it more difficult for targets to escape the pull?", "", false)]
		public bool IncreaseForceForEscapingTargets { get; set; }

		// Token: 0x1700183E RID: 6206
		// (get) Token: 0x06005FD0 RID: 24528 RVA: 0x0031E18D File Offset: 0x0031C38D
		// (set) Token: 0x06005FD1 RID: 24529 RVA: 0x0031E198 File Offset: 0x0031C398
		public bool Snapped
		{
			get
			{
				return this.snapped;
			}
			set
			{
				if (this.snapped == value)
				{
					return;
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				this.snapped = value;
				if (!this.snapped)
				{
					this.snapTimer = 0f;
					return;
				}
				if (this.target != null && this.source != null && this.target != this.source)
				{
					this.PlaySound(this.snapSound, this.source.WorldPosition);
					this.PlaySound(this.snapSound, this.target.WorldPosition);
				}
			}
		}

		// Token: 0x06005FD2 RID: 24530 RVA: 0x0031E22A File Offset: 0x0031C42A
		public Rope(Item item, ContentXElement element) : base(item, element)
		{
			this.InitProjSpecific(element);
		}

		// Token: 0x06005FD3 RID: 24531 RVA: 0x0031E23C File Offset: 0x0031C43C
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "startsprite"))
					{
						if (!(a == "endsprite"))
						{
							if (!(a == "snapsound"))
							{
								if (a == "reelsound")
								{
									this.reelSound = RoundSound.Load(subElement);
								}
							}
							else
							{
								this.snapSound = RoundSound.Load(subElement);
							}
						}
						else
						{
							this.endSprite = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.startSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.sprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
		}

		// Token: 0x06005FD4 RID: 24532 RVA: 0x0031E350 File Offset: 0x0031C550
		public void Snap()
		{
			this.Snapped = true;
		}

		// Token: 0x06005FD5 RID: 24533 RVA: 0x0031E35C File Offset: 0x0031C55C
		public void Attach(ISpatialEntity source, Item target)
		{
			this.target = target;
			this.SetSource(source);
			this.Snapped = false;
			base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, null, new Vector2?(this.item.WorldPosition), 1f);
			this.IsActive = true;
		}

		// Token: 0x06005FD6 RID: 24534 RVA: 0x0031E3AC File Offset: 0x0031C5AC
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateProjSpecific();
			this.isReelingIn = false;
			Projectile component = this.item.GetComponent<Projectile>();
			Character user = (component != null) ? component.User : null;
			if (this.source != null && this.target != null && !this.target.Removed)
			{
				Entity entity = this.source as Entity;
				if (entity == null || !entity.Removed)
				{
					Limb limb = this.source as Limb;
					if ((limb == null || !limb.Removed) && (user == null || !user.Removed))
					{
						if (this.Snapped)
						{
							this.snapTimer += deltaTime;
							if (this.snapTimer >= this.SnapAnimDuration)
							{
								this.IsActive = false;
							}
							return;
						}
						Vector2 diff = this.target.WorldPosition - this.GetSourcePos(false);
						float lengthSqr = diff.LengthSquared();
						if (lengthSqr > this.MaxLength * this.MaxLength)
						{
							this.Snap();
							return;
						}
						if (this.MaxAngle < 180f && lengthSqr > 2500f)
						{
							Vector2 value = this.launchDir.GetValueOrDefault();
							if (this.launchDir == null)
							{
								value = diff;
								this.launchDir = new Vector2?(value);
							}
							float angle = MathHelper.ToDegrees(this.launchDir.Value.Angle(diff));
							if (angle > this.MaxAngle)
							{
								this.Snap();
								return;
							}
						}
						this.item.ResetCachedVisibleSize();
						Projectile projectile = this.target.GetComponent<Projectile>();
						if (projectile == null)
						{
							return;
						}
						if (this.SnapOnCollision)
						{
							this.raycastTimer += deltaTime;
							if (this.raycastTimer > 0.2f)
							{
								if (Submarine.PickBody(ConvertUnits.ToSimUnits(this.source.WorldPosition), ConvertUnits.ToSimUnits(this.target.WorldPosition), null, new Category?(Category.Cat1 | Category.Cat8), true, delegate(Fixture f)
								{
									foreach (Body body2 in projectile.Hits)
									{
										Submarine alreadyHitSub = null;
										Structure hitStructure = body2.UserData as Structure;
										if (hitStructure != null)
										{
											alreadyHitSub = hitStructure.Submarine;
										}
										else
										{
											Submarine hitSub = body2.UserData as Submarine;
											if (hitSub != null)
											{
												alreadyHitSub = hitSub;
											}
										}
										if (alreadyHitSub != null)
										{
											Body body3 = f.Body;
											MapEntity me = ((body3 != null) ? body3.UserData : null) as MapEntity;
											if (me != null && me.Submarine == alreadyHitSub)
											{
												return false;
											}
											Body body4 = f.Body;
											if (((body4 != null) ? body4.UserData : null) as Submarine == alreadyHitSub)
											{
												return false;
											}
										}
									}
									Body stickTarget = projectile.StickTarget;
									Submarine targetSub = (((stickTarget != null) ? stickTarget.UserData : null) as Submarine) ?? this.target.Submarine;
									Body body5 = f.Body;
									MapEntity mapEntity = ((body5 != null) ? body5.UserData : null) as MapEntity;
									if (mapEntity != null && mapEntity.Submarine != null)
									{
										if (mapEntity.Submarine == targetSub || mapEntity.Submarine == this.source.Submarine)
										{
											return false;
										}
									}
									else
									{
										Body body6 = f.Body;
										Submarine sub = ((body6 != null) ? body6.UserData : null) as Submarine;
										if (sub != null && (sub == targetSub || sub == this.source.Submarine))
										{
											return false;
										}
									}
									return true;
								}, false) != null)
								{
									this.Snap();
									return;
								}
								this.raycastTimer = 0f;
							}
						}
						Vector2 forceDir = diff;
						this.currentRopeLength = diff.Length();
						if (this.currentRopeLength > 0.001f)
						{
							forceDir = Vector2.Normalize(forceDir);
						}
						if (Math.Abs(this.ProjectilePullForce) > 0.001f)
						{
							Item item = projectile.Item;
							if (item != null)
							{
								PhysicsBody body = item.body;
								if (body != null)
								{
									body.ApplyForce(-forceDir * this.ProjectilePullForce, 64f);
								}
							}
						}
						if (projectile.StickTarget != null)
						{
							float targetMass = float.MaxValue;
							Character targetCharacter = null;
							object userData = projectile.StickTarget.UserData;
							Limb targetLimb = userData as Limb;
							if (targetLimb == null)
							{
								Character character = userData as Character;
								if (character == null)
								{
									if (userData is Item)
									{
										targetMass = projectile.StickTarget.Mass;
									}
								}
								else
								{
									targetCharacter = character;
									targetMass = character.Mass;
								}
							}
							else
							{
								targetCharacter = targetLimb.character;
								targetMass = targetLimb.ragdoll.Mass;
							}
							if (projectile.StickTarget.BodyType != BodyType.Dynamic)
							{
								targetMass = float.MaxValue;
							}
							if (user != null)
							{
								if (!this.snapped && (projectile.Launcher == null || projectile.Launcher.GetComponent<Holdable>() != null))
								{
									user.AnimController.HoldToRope();
									if (targetCharacter != null)
									{
										targetCharacter.AnimController.DragWithRope();
									}
									if (user.InWater)
									{
										user.AnimController.HangWithRope();
									}
								}
								if (Math.Abs(this.SourcePullForce) > 0.001f && targetMass > this.TargetMinMass)
								{
									PhysicsBody sourceBody = Rope.GetBodyToPull(this.source);
									if (sourceBody != null)
									{
										PhysicsBody targetBody = Rope.GetBodyToPull(this.target);
										if (sourceBody.UserData is Character)
										{
											this.isReelingIn = ((user.InWater && user.IsRagdolled) || (!user.InWater && targetCharacter != null && !targetCharacter.IsIncapacitated));
											if (this.isReelingIn)
											{
												float pullForce = this.SourcePullForce;
												if (!user.InWater)
												{
													pullForce *= 0.1f;
												}
												float lengthFactor = MathUtils.InverseLerp(0f, this.MaxLength / 2f, this.currentRopeLength);
												float force = this.LerpForces ? MathHelper.Lerp(0f, pullForce, lengthFactor) : pullForce;
												sourceBody.ApplyForce(forceDir * force, 64f);
												if (targetBody != null)
												{
													if (targetCharacter != null)
													{
														if (targetBody.LinearVelocity != Vector2.Zero && sourceBody.LinearVelocity != Vector2.Zero)
														{
															Vector2 targetDir = Vector2.Normalize(targetBody.LinearVelocity);
															float movementDot = Vector2.Dot(Vector2.Normalize(sourceBody.LinearVelocity), targetDir);
															if (movementDot < 0f)
															{
																float inverseLengthFactor = MathHelper.Lerp(1f, 0f, lengthFactor);
																sourceBody.ApplyForce(targetBody.LinearVelocity * Math.Min(targetBody.Mass * 5f, 250f) * sourceBody.Mass * -movementDot * inverseLengthFactor, 64f);
															}
															float forceDot = Vector2.Dot(forceDir, targetDir);
															if (forceDot > 0f)
															{
																float targetSpeed = targetBody.LinearVelocity.Length();
																sourceBody.ApplyForce(forceDir * targetSpeed * sourceBody.Mass * 25f * forceDot * lengthFactor, 64f);
															}
															float colliderMainLimbDistance = Vector2.Distance(sourceBody.SimPosition, user.AnimController.MainLimb.SimPosition);
															if (colliderMainLimbDistance > 1f && !(sourceBody.UserData is Submarine))
															{
																float correctionForce = MathHelper.Lerp(10f, 64f, MathUtils.InverseLerp(1f, 10f, colliderMainLimbDistance));
																Vector2 targetPos = sourceBody.SimPosition + new Vector2((float)Math.Sin((double)(-(double)sourceBody.Rotation)), (float)Math.Cos((double)(-(double)sourceBody.Rotation))) * 0.4f;
																user.AnimController.MainLimb.MoveToPos(targetPos, correctionForce, false);
															}
														}
													}
													else
													{
														sourceBody.ApplyForce(targetBody.LinearVelocity * sourceBody.Mass, 64f);
													}
												}
											}
										}
										else
										{
											float distance = Vector2.Distance(this.source.WorldPosition, this.target.WorldPosition);
											float force2 = this.LerpForces ? MathHelper.Lerp(0f, this.SourcePullForce, MathUtils.InverseLerp(0f, this.MaxLength / 2f, distance)) : this.SourcePullForce;
											sourceBody.ApplyForce(forceDir * force2, 64f);
										}
									}
								}
							}
							if (Math.Abs(this.TargetPullForce) > 0.001f && (user == null || !user.IsRagdolled))
							{
								PhysicsBody targetBody2 = Rope.GetBodyToPull(this.target);
								if (targetBody2 == null)
								{
									return;
								}
								bool lerpForces = this.LerpForces;
								float maxVelocity = 16f;
								float maxPullDistance = this.MaxLength / 3f;
								float minPullDistance = this.MinPullDistance;
								if (targetCharacter != null)
								{
									if (targetCharacter.IsRagdolled || targetCharacter.IsUnconscious)
									{
										if (!targetCharacter.InWater)
										{
											maxVelocity = 4.8f;
										}
									}
									else
									{
										minPullDistance = 50f;
										maxPullDistance = 200f;
									}
								}
								minPullDistance = MathHelper.Max(minPullDistance, 50f);
								if (this.currentRopeLength < minPullDistance)
								{
									return;
								}
								maxPullDistance = MathHelper.Max(minPullDistance * 2f, maxPullDistance);
								float force3 = lerpForces ? MathHelper.Lerp(0f, this.TargetPullForce, MathUtils.InverseLerp(minPullDistance, maxPullDistance, this.currentRopeLength)) : this.TargetPullForce;
								targetBody2.ApplyForce(-forceDir * force3, maxVelocity);
								AnimController targetRagdoll = (targetCharacter != null) ? targetCharacter.AnimController : null;
								if (((targetRagdoll != null) ? targetRagdoll.Collider : null) != null)
								{
									this.isReelingIn = true;
									if (targetRagdoll.InWater || targetRagdoll.OnGround)
									{
										float forceMultiplier = 1f;
										if (!targetCharacter.IsRagdolled && !targetCharacter.IsIncapacitated && this.IncreaseForceForEscapingTargets)
										{
											Vector2 targetMovement = targetCharacter.AnimController.TargetMovement;
											float dot = Vector2.Dot(Vector2.Normalize(targetMovement), forceDir);
											if (dot > 0f)
											{
												float targetVelocity = targetMovement.Length();
												float massFactor = Math.Max((float)Math.Log((double)(targetCharacter.Mass / 10f)), 1f);
												forceMultiplier = Math.Max(targetVelocity * massFactor * 2.5f * dot, 1f);
											}
										}
										targetRagdoll.Collider.ApplyForce(-forceDir * force3 * forceMultiplier, maxVelocity);
									}
								}
							}
						}
						return;
					}
				}
			}
			this.ResetSource();
			this.target = null;
			this.IsActive = false;
		}

		// Token: 0x06005FD7 RID: 24535 RVA: 0x0031EC7C File Offset: 0x0031CE7C
		private void UpdateProjSpecific()
		{
			if (this.isReelingIn && !this.Snapped)
			{
				this.PlaySound(this.reelSound, this.source.WorldPosition);
				return;
			}
			SoundChannel soundChannel = this.reelSoundChannel;
			if (soundChannel != null)
			{
				soundChannel.FadeOutAndDispose();
			}
			this.reelSoundChannel = null;
		}

		// Token: 0x06005FD8 RID: 24536 RVA: 0x0031ECC9 File Offset: 0x0031CEC9
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			if (this.Snapped)
			{
				this.snapTimer += deltaTime;
				if (this.snapTimer >= this.SnapAnimDuration)
				{
					this.IsActive = false;
				}
			}
		}

		// Token: 0x06005FD9 RID: 24537 RVA: 0x0031ED00 File Offset: 0x0031CF00
		private Vector2 GetSourcePos(bool useDrawPosition = false)
		{
			Vector2 sourcePos = this.source.WorldPosition;
			Item sourceItem = this.source as Item;
			if (sourceItem != null)
			{
				if (useDrawPosition)
				{
					sourcePos = sourceItem.DrawPosition;
				}
				if (!sourceItem.Removed)
				{
					Turret turret = sourceItem.GetComponent<Turret>();
					if (turret != null)
					{
						sourcePos = new Vector2((float)sourceItem.WorldRect.X + turret.TransformedBarrelPos.X, (float)sourceItem.WorldRect.Y - turret.TransformedBarrelPos.Y);
					}
					else
					{
						RangedWeapon weapon = sourceItem.GetComponent<RangedWeapon>();
						if (weapon != null)
						{
							sourcePos += ConvertUnits.ToDisplayUnits(weapon.TransformedBarrelPos);
						}
					}
				}
			}
			else if (useDrawPosition)
			{
				Limb sourceLimb = this.source as Limb;
				if (sourceLimb != null && sourceLimb.body != null)
				{
					sourcePos = sourceLimb.body.DrawPosition;
				}
			}
			return sourcePos;
		}

		// Token: 0x06005FDA RID: 24538 RVA: 0x0031EDCC File Offset: 0x0031CFCC
		private static PhysicsBody GetBodyToPull(ISpatialEntity target)
		{
			Item targetItem = target as Item;
			if (targetItem != null)
			{
				Inventory parentInventory = targetItem.ParentInventory;
				if (parentInventory is CharacterInventory)
				{
					Character ownerCharacter = parentInventory.Owner as Character;
					if (ownerCharacter != null)
					{
						if (ownerCharacter.Removed)
						{
							return null;
						}
						return ownerCharacter.AnimController.Collider;
					}
				}
				Projectile projectile = targetItem.GetComponent<Projectile>();
				if (projectile != null && projectile.StickTarget != null)
				{
					object userData = projectile.StickTarget.UserData;
					Structure structure = userData as Structure;
					PhysicsBody result;
					if (structure == null)
					{
						Submarine sub = userData as Submarine;
						if (sub == null)
						{
							Item item = userData as Item;
							if (item == null)
							{
								Limb limb = userData as Limb;
								if (limb == null)
								{
									result = null;
								}
								else
								{
									result = limb.body;
								}
							}
							else
							{
								result = item.body;
							}
						}
						else
						{
							result = sub.PhysicsBody;
						}
					}
					else
					{
						Submarine submarine = structure.Submarine;
						result = ((submarine != null) ? submarine.PhysicsBody : null);
					}
					return result;
				}
				if (targetItem.body != null)
				{
					return targetItem.body;
				}
				if (targetItem.StaticFixtures.Any<Fixture>() && targetItem.Submarine != null)
				{
					return targetItem.Submarine.PhysicsBody;
				}
			}
			else
			{
				Limb targetLimb = target as Limb;
				if (targetLimb != null)
				{
					return targetLimb.body;
				}
			}
			return null;
		}

		// Token: 0x0400317A RID: 12666
		private Sprite sprite;

		// Token: 0x0400317B RID: 12667
		private Sprite startSprite;

		// Token: 0x0400317C RID: 12668
		private Sprite endSprite;

		// Token: 0x0400317D RID: 12669
		private RoundSound snapSound;

		// Token: 0x0400317E RID: 12670
		private RoundSound reelSound;

		// Token: 0x0400317F RID: 12671
		private SoundChannel reelSoundChannel;

		// Token: 0x04003185 RID: 12677
		private Vector2 _reelSoundPitchSlide;

		// Token: 0x04003186 RID: 12678
		private ISpatialEntity source;

		// Token: 0x04003187 RID: 12679
		private Item target;

		// Token: 0x04003188 RID: 12680
		private Vector2? launchDir;

		// Token: 0x04003189 RID: 12681
		private float currentRopeLength;

		// Token: 0x0400318A RID: 12682
		private float snapTimer;

		// Token: 0x0400318C RID: 12684
		private float raycastTimer;

		// Token: 0x0400318D RID: 12685
		private const float RayCastInterval = 0.2f;

		// Token: 0x0400319B RID: 12699
		private bool isReelingIn;

		// Token: 0x0400319C RID: 12700
		private bool snapped;
	}
}
