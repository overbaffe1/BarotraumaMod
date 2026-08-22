using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000D2 RID: 210
	internal class BackgroundCreature : ISteerable, ILevelRenderableObject
	{
		// Token: 0x17000742 RID: 1858
		// (get) Token: 0x06001C2B RID: 7211 RVA: 0x001194E6 File Offset: 0x001176E6
		// (set) Token: 0x06001C2C RID: 7212 RVA: 0x001194EE File Offset: 0x001176EE
		public float Depth { get; private set; }

		// Token: 0x17000743 RID: 1859
		// (get) Token: 0x06001C2D RID: 7213 RVA: 0x001194F7 File Offset: 0x001176F7
		// (set) Token: 0x06001C2E RID: 7214 RVA: 0x001194FF File Offset: 0x001176FF
		public Vector2[,] CurrentSpriteDeformation { get; private set; }

		// Token: 0x17000744 RID: 1860
		// (get) Token: 0x06001C2F RID: 7215 RVA: 0x00119508 File Offset: 0x00117708
		// (set) Token: 0x06001C30 RID: 7216 RVA: 0x00119510 File Offset: 0x00117710
		public Vector2[,] CurrentLightSpriteDeformation { get; private set; }

		// Token: 0x17000745 RID: 1861
		// (get) Token: 0x06001C31 RID: 7217 RVA: 0x00119519 File Offset: 0x00117719
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.position);
			}
		}

		// Token: 0x17000746 RID: 1862
		// (get) Token: 0x06001C32 RID: 7218 RVA: 0x00119526 File Offset: 0x00117726
		public Vector2 WorldPosition
		{
			get
			{
				return this.position;
			}
		}

		// Token: 0x17000747 RID: 1863
		// (get) Token: 0x06001C33 RID: 7219 RVA: 0x0011952E File Offset: 0x0011772E
		public Vector2 Velocity
		{
			get
			{
				return new Vector2(this.velocity.X, this.velocity.Y);
			}
		}

		// Token: 0x17000748 RID: 1864
		// (get) Token: 0x06001C34 RID: 7220 RVA: 0x0011954B File Offset: 0x0011774B
		// (set) Token: 0x06001C35 RID: 7221 RVA: 0x00119553 File Offset: 0x00117753
		public Vector2 Steering { get; set; }

		// Token: 0x17000749 RID: 1865
		// (get) Token: 0x06001C36 RID: 7222 RVA: 0x0011955C File Offset: 0x0011775C
		public Vector3 Position
		{
			get
			{
				return new Vector3(this.position.X, this.position.Y, this.Depth);
			}
		}

		// Token: 0x06001C37 RID: 7223 RVA: 0x00119580 File Offset: 0x00117780
		public BackgroundCreature(BackgroundCreaturePrefab prefab, Vector2 position)
		{
			this.Prefab = prefab;
			this.position = position;
			this.drawPosition = position;
			this.steeringManager = new SteeringManager(this);
			this.velocity = new Vector3(Rand.Range(-prefab.Speed, prefab.Speed, Rand.RandSync.ClientOnly), Rand.Range(-prefab.Speed, prefab.Speed, Rand.RandSync.ClientOnly), Rand.Range(0f, prefab.WanderZAmount, Rand.RandSync.ClientOnly));
			this.Depth = Rand.Range(prefab.MinDepth, prefab.MaxDepth, Rand.RandSync.ClientOnly);
			this.checkWallsTimer = Rand.Range(0f, 5f, Rand.RandSync.ClientOnly);
			foreach (XElement subElement in prefab.Config.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				List<SpriteDeformation> deformationList;
				if (!(a == "deformablesprite"))
				{
					if (!(a == "deformablelightsprite"))
					{
						continue;
					}
					deformationList = this.lightSpriteDeformations;
				}
				else
				{
					deformationList = this.spriteDeformations;
				}
				int i = 0;
				foreach (XElement animationElement in subElement.Elements())
				{
					SpriteDeformation deformation = null;
					int sync = animationElement.GetAttributeInt("sync", -1);
					if (sync > -1)
					{
						string typeName = animationElement.GetAttributeString("type", "").ToLowerInvariant();
						deformation = this.uniqueSpriteDeformations.Find((SpriteDeformation d) => d.TypeName == typeName && d.Sync == sync);
					}
					if (deformation == null)
					{
						deformation = SpriteDeformation.Load(animationElement, prefab.Name);
						if (deformation != null)
						{
							deformation.Params = this.Prefab.SpriteDeformations[i].Params;
							this.uniqueSpriteDeformations.Add(deformation);
							if (prefab.DeformableSprite != null && (deformation.Resolution.X > prefab.DeformableSprite.Subdivisions.X || deformation.Resolution.Y > prefab.DeformableSprite.Subdivisions.Y))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(218, 4);
								defaultInterpolatedStringHandler.AppendLiteral("Potential error in background creature ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(": deformation ");
								defaultInterpolatedStringHandler.AppendFormatted<Type>(deformation.GetType());
								defaultInterpolatedStringHandler.AppendLiteral(" has a larger resolution (");
								defaultInterpolatedStringHandler.AppendFormatted<Point>(deformation.Resolution);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								defaultInterpolatedStringHandler.AppendLiteral(" than the amount of subdivisions on the deformable sprite (");
								defaultInterpolatedStringHandler.AppendFormatted<Point>(prefab.DeformableSprite.Subdivisions);
								defaultInterpolatedStringHandler.AppendLiteral("). Should the sprite be subdivided further to make full use of the deformation?");
								DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
							}
							i++;
						}
					}
					if (deformation != null)
					{
						deformationList.Add(deformation);
					}
				}
			}
			this.flashTimer = Rand.Range(0f, prefab.FlashInterval, Rand.RandSync.Unsynced);
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x001198F8 File Offset: 0x00117AF8
		public void Update(float deltaTime)
		{
			this.position += new Vector2(this.velocity.X, this.velocity.Y) * deltaTime;
			this.Depth = MathHelper.Clamp(this.Depth + this.velocity.Z * deltaTime, this.Prefab.MinDepth, this.Prefab.MaxDepth);
			if (this.Prefab.FlashInterval > 0f)
			{
				this.flashTimer -= deltaTime;
				if (this.flashTimer > 0f)
				{
					this.alpha = 0f;
				}
				else
				{
					this.alpha = (float)Math.Sin((double)(-(double)this.flashTimer / this.Prefab.FlashDuration * 3.1415927f)) * PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.5f);
					if (this.flashTimer < -this.Prefab.FlashDuration)
					{
						this.flashTimer = this.Prefab.FlashInterval;
					}
				}
			}
			this.checkWallsTimer -= deltaTime;
			if (this.checkWallsTimer <= 0f && Level.Loaded != null)
			{
				this.checkWallsTimer = 5f;
				this.obstacleDiff = Vector2.Zero;
				if (this.position.Y > (float)Level.Loaded.Size.Y)
				{
					this.obstacleDiff = Vector2.UnitY;
				}
				else if (this.position.Y < 0f)
				{
					this.obstacleDiff = -Vector2.UnitY;
				}
				else if (this.position.X < 0f)
				{
					this.obstacleDiff = -Vector2.UnitX;
				}
				else if (this.position.X > (float)Level.Loaded.Size.X)
				{
					this.obstacleDiff = Vector2.UnitX;
				}
				else
				{
					List<VoronoiCell> cells = Level.Loaded.GetCells(this.position, 1);
					if (cells.Count > 0)
					{
						int cellCount = 0;
						foreach (VoronoiCell cell in cells)
						{
							Vector2 diff = cell.Center - this.position;
							if (diff.LengthSquared() <= 25000000f)
							{
								this.obstacleDiff += diff;
								cellCount++;
							}
						}
						if (cellCount > 0)
						{
							this.obstacleDiff /= (float)cellCount;
							this.obstacleDist = this.obstacleDiff.Length();
							this.obstacleDiff = Vector2.Normalize(this.obstacleDiff);
						}
					}
				}
			}
			if (this.Swarm != null)
			{
				Vector2 midPoint = this.Swarm.MidPoint();
				float midPointDist = Vector2.Distance(this.SimPosition, midPoint) * 100f;
				if (midPointDist > this.Swarm.MaxDistance)
				{
					this.steeringManager.SteeringSeek(midPoint, (midPointDist / this.Swarm.MaxDistance - 1f) * this.Prefab.Speed);
				}
				this.steeringManager.SteeringManual(deltaTime, this.Swarm.AvgVelocity() * this.Swarm.Cohesion);
			}
			if (this.Prefab.WanderAmount > 0f)
			{
				this.steeringManager.SteeringWander(this.Prefab.Speed, false);
			}
			if (this.obstacleDiff != Vector2.Zero)
			{
				this.steeringManager.SteeringManual(deltaTime, -this.obstacleDiff * (1f - this.obstacleDist / 5000f) * this.Prefab.Speed);
			}
			this.steeringManager.Update(this.Prefab.Speed);
			if (this.Prefab.WanderZAmount > 0f)
			{
				this.wanderZPhase += Rand.Range(-this.Prefab.WanderZAmount, this.Prefab.WanderZAmount, Rand.RandSync.Unsynced);
				this.velocity.Z = (float)Math.Sin((double)this.wanderZPhase) * this.Prefab.Speed;
			}
			this.velocity = Vector3.Lerp(this.velocity, new Vector3(this.Steering.X, this.Steering.Y, this.velocity.Z), deltaTime);
			if (Math.Abs(this.velocity.X) > this.Prefab.Speed * 0.1f)
			{
				this.flippedHorizontally = (!this.Prefab.DisableFlipping && this.velocity.X < 0f);
			}
			this.UpdateDeformations(deltaTime, this.flippedHorizontally);
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x00119DCC File Offset: 0x00117FCC
		public void DrawLightSprite(SpriteBatch spriteBatch, Camera cam)
		{
			this.Draw(spriteBatch, cam, this.Prefab.LightSprite, this.Prefab.DeformableLightSprite, this.CurrentLightSpriteDeformation, Color.White * this.alpha);
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x00119E04 File Offset: 0x00118004
		public void Draw(SpriteBatch spriteBatch, Camera cam)
		{
			Color color = this.Prefab.FadeOut ? (Color.Lerp(Color.White, Level.Loaded.BackgroundColor, this.Depth / this.Prefab.FadeOutDepth) * this.alpha) : (Color.White * this.alpha);
			this.Draw(spriteBatch, cam, this.Prefab.Sprite, this.Prefab.DeformableSprite, this.CurrentSpriteDeformation, color);
		}

		// Token: 0x06001C3B RID: 7227 RVA: 0x00119E88 File Offset: 0x00118088
		private void Draw(SpriteBatch spriteBatch, Camera cam, Sprite sprite, DeformableSprite deformableSprite, Vector2[,] currentSpriteDeformation, Color color)
		{
			if (sprite == null && deformableSprite == null)
			{
				return;
			}
			if (color.A == 0)
			{
				return;
			}
			float rotation = 0f;
			if (!this.Prefab.DisableRotation)
			{
				rotation = MathUtils.VectorToAngle(new Vector2(this.velocity.X, -this.velocity.Y));
				if (this.flippedHorizontally)
				{
					rotation -= 3.1415927f;
				}
			}
			this.drawPosition = this.GetDrawPosition(cam);
			float scale = this.GetScale();
			if (sprite != null)
			{
				sprite.Draw(spriteBatch, new Vector2(this.drawPosition.X, -this.drawPosition.Y), color, rotation, scale, this.flippedHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None, new float?(Math.Min(this.Depth / 10000f, 1f)));
			}
			if (deformableSprite != null)
			{
				if (currentSpriteDeformation != null)
				{
					deformableSprite.Deform(currentSpriteDeformation);
				}
				else
				{
					deformableSprite.Reset();
				}
				if (deformableSprite != null)
				{
					deformableSprite.Draw(cam, new Vector3(this.drawPosition.X, this.drawPosition.Y, Math.Min(this.Depth / 10000f, 1f)), deformableSprite.Origin, rotation, Vector2.One * scale, color, this.flippedHorizontally, false);
				}
			}
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x00119FC4 File Offset: 0x001181C4
		public Vector2 GetDrawPosition(Camera cam)
		{
			Vector2 drawPosition = this.WorldPosition;
			if (this.Depth >= 0f)
			{
				Vector2 camOffset = drawPosition - cam.WorldViewCenter;
				drawPosition -= camOffset * this.Depth / 10000f;
			}
			return drawPosition;
		}

		// Token: 0x06001C3D RID: 7229 RVA: 0x0011A010 File Offset: 0x00118210
		public float GetScale()
		{
			return Math.Max(1f - this.Depth / 10000f, 0.05f) * this.Prefab.Scale;
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x0011A03C File Offset: 0x0011823C
		public Rectangle GetExtents(Camera cam)
		{
			Vector2 min = this.GetDrawPosition(cam);
			Vector2 max = min;
			BackgroundCreature.<>c__DisplayClass50_0 CS$<>8__locals1;
			CS$<>8__locals1.scale = this.GetScale();
			BackgroundCreature.<GetExtents>g__GetSpriteExtents|50_0(this.Prefab.Sprite, ref min, ref max, ref CS$<>8__locals1);
			BackgroundCreature.<GetExtents>g__GetSpriteExtents|50_0(this.Prefab.LightSprite, ref min, ref max, ref CS$<>8__locals1);
			DeformableSprite deformableSprite = this.Prefab.DeformableSprite;
			BackgroundCreature.<GetExtents>g__GetSpriteExtents|50_0((deformableSprite != null) ? deformableSprite.Sprite : null, ref min, ref max, ref CS$<>8__locals1);
			DeformableSprite deformableLightSprite = this.Prefab.DeformableLightSprite;
			BackgroundCreature.<GetExtents>g__GetSpriteExtents|50_0((deformableLightSprite != null) ? deformableLightSprite.Sprite : null, ref min, ref max, ref CS$<>8__locals1);
			return new Rectangle(min.ToPoint(), (max - min).ToPoint());
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x0011A0EC File Offset: 0x001182EC
		private void UpdateDeformations(float deltaTime, bool flippedHorizontally)
		{
			foreach (SpriteDeformation deformation in this.uniqueSpriteDeformations)
			{
				deformation.Update(deltaTime);
			}
			if (this.spriteDeformations.Count > 0)
			{
				this.CurrentSpriteDeformation = SpriteDeformation.GetDeformation(this.spriteDeformations, this.Prefab.DeformableSprite.Size, flippedHorizontally, false);
			}
			if (this.lightSpriteDeformations.Count > 0)
			{
				this.CurrentLightSpriteDeformation = SpriteDeformation.GetDeformation(this.lightSpriteDeformations, this.Prefab.DeformableLightSprite.Size, flippedHorizontally, false);
			}
		}

		// Token: 0x06001C40 RID: 7232 RVA: 0x0011A1A4 File Offset: 0x001183A4
		[CompilerGenerated]
		internal static void <GetExtents>g__GetSpriteExtents|50_0(Sprite sprite, ref Vector2 min, ref Vector2 max, ref BackgroundCreature.<>c__DisplayClass50_0 A_3)
		{
			if (sprite == null)
			{
				return;
			}
			min.X = Math.Min(min.X, min.X - sprite.size.X * sprite.RelativeOrigin.X * A_3.scale);
			min.Y = Math.Min(min.Y, min.Y - sprite.size.Y * sprite.RelativeOrigin.Y * A_3.scale);
			max.X = Math.Max(max.X, max.X + sprite.size.X * (1f - sprite.RelativeOrigin.X) * A_3.scale);
			max.Y = Math.Max(max.Y, max.Y + sprite.size.Y * (1f - sprite.RelativeOrigin.Y) * A_3.scale);
		}

		// Token: 0x04000E71 RID: 3697
		private const float MaxDepth = 10000f;

		// Token: 0x04000E72 RID: 3698
		private const float CheckWallsInterval = 5f;

		// Token: 0x04000E73 RID: 3699
		public bool Visible;

		// Token: 0x04000E74 RID: 3700
		public readonly BackgroundCreaturePrefab Prefab;

		// Token: 0x04000E75 RID: 3701
		private readonly List<SpriteDeformation> uniqueSpriteDeformations = new List<SpriteDeformation>();

		// Token: 0x04000E76 RID: 3702
		private readonly List<SpriteDeformation> spriteDeformations = new List<SpriteDeformation>();

		// Token: 0x04000E77 RID: 3703
		private readonly List<SpriteDeformation> lightSpriteDeformations = new List<SpriteDeformation>();

		// Token: 0x04000E78 RID: 3704
		private Vector2 position;

		// Token: 0x04000E79 RID: 3705
		private Vector3 velocity;

		// Token: 0x04000E7B RID: 3707
		private float alpha = 1f;

		// Token: 0x04000E7C RID: 3708
		private readonly SteeringManager steeringManager;

		// Token: 0x04000E7D RID: 3709
		private float checkWallsTimer;

		// Token: 0x04000E7E RID: 3710
		private float flashTimer;

		// Token: 0x04000E7F RID: 3711
		private float wanderZPhase;

		// Token: 0x04000E80 RID: 3712
		private Vector2 obstacleDiff;

		// Token: 0x04000E81 RID: 3713
		private float obstacleDist;

		// Token: 0x04000E82 RID: 3714
		public Swarm Swarm;

		// Token: 0x04000E83 RID: 3715
		private Vector2 drawPosition;

		// Token: 0x04000E84 RID: 3716
		private bool flippedHorizontally;
	}
}
