using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Lights
{
	// Token: 0x020004D6 RID: 1238
	internal class LightSource
	{
		// Token: 0x1700148A RID: 5258
		// (get) Token: 0x0600508E RID: 20622 RVA: 0x002B52B1 File Offset: 0x002B34B1
		// (set) Token: 0x0600508F RID: 20623 RVA: 0x002B52C6 File Offset: 0x002B34C6
		public bool CastShadows
		{
			get
			{
				return this.castShadows && !this.IsBackground;
			}
			set
			{
				this.castShadows = value;
			}
		}

		// Token: 0x1700148B RID: 5259
		// (get) Token: 0x06005090 RID: 20624 RVA: 0x002B52D0 File Offset: 0x002B34D0
		// (set) Token: 0x06005091 RID: 20625 RVA: 0x002B5318 File Offset: 0x002B3518
		public bool NeedsRecalculation
		{
			get
			{
				PhysicsBody parentBody = this.ParentBody;
				Item it = ((parentBody != null) ? parentBody.UserData : null) as Item;
				return (it != null && it.Prefab.Identifier == "flashlight") || this.needsRecalculation;
			}
			set
			{
				if (value)
				{
					foreach (ConvexHullList chList in this.convexHullsInRange)
					{
						chList.IsHidden.Clear();
					}
				}
				this.needsRecalculation = value;
				if (this.needsRecalculation && this.state != LightSource.LightVertexState.UpToDate)
				{
					this.needsRecalculationWhenUpToDate = true;
				}
			}
		}

		// Token: 0x1700148C RID: 5260
		// (get) Token: 0x06005092 RID: 20626 RVA: 0x002B5390 File Offset: 0x002B3590
		// (set) Token: 0x06005093 RID: 20627 RVA: 0x002B5398 File Offset: 0x002B3598
		public float LastRecalculationTime { get; private set; }

		// Token: 0x1700148D RID: 5261
		// (get) Token: 0x06005094 RID: 20628 RVA: 0x002B53A1 File Offset: 0x002B35A1
		public LightSourceParams LightSourceParams
		{
			get
			{
				return this.lightSourceParams;
			}
		}

		// Token: 0x1700148E RID: 5262
		// (get) Token: 0x06005095 RID: 20629 RVA: 0x002B53A9 File Offset: 0x002B35A9
		// (set) Token: 0x06005096 RID: 20630 RVA: 0x002B53B4 File Offset: 0x002B35B4
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				Vector2 moveAmount = value - this.position;
				if (Math.Abs(moveAmount.X) < 0.1f && Math.Abs(moveAmount.Y) < 0.1f)
				{
					return;
				}
				this.position = value;
				if (Vector2.DistanceSquared(this.prevCalculatedPosition, this.position) < 100f && this.vertices != null)
				{
					this.translateVertices = this.position - this.prevCalculatedPosition;
					return;
				}
				this.HullsUpToDate.Clear();
				this.NeedsRecalculation = true;
			}
		}

		// Token: 0x1700148F RID: 5263
		// (get) Token: 0x06005097 RID: 20631 RVA: 0x002B5444 File Offset: 0x002B3644
		// (set) Token: 0x06005098 RID: 20632 RVA: 0x002B544C File Offset: 0x002B364C
		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				if (Math.Abs(value - this.rotation) < 0.001f)
				{
					return;
				}
				this.rotation = value;
				this.RefreshDirection();
				if (Math.Abs(this.rotation - this.prevCalculatedRotation) < 0.02f && this.vertices != null)
				{
					return;
				}
				this.HullsUpToDate.Clear();
				this.NeedsRecalculation = true;
			}
		}

		// Token: 0x17001490 RID: 5264
		// (get) Token: 0x06005099 RID: 20633 RVA: 0x002B54AF File Offset: 0x002B36AF
		// (set) Token: 0x0600509A RID: 20634 RVA: 0x002B54C7 File Offset: 0x002B36C7
		public Vector2 SpriteScale
		{
			get
			{
				return this._spriteScale * this.lightSourceParams.Scale;
			}
			set
			{
				this._spriteScale = value;
			}
		}

		// Token: 0x17001491 RID: 5265
		// (get) Token: 0x0600509B RID: 20635 RVA: 0x002B54D0 File Offset: 0x002B36D0
		// (set) Token: 0x0600509C RID: 20636 RVA: 0x002B54DD File Offset: 0x002B36DD
		public float? OverrideLightSpriteAlpha
		{
			get
			{
				return this.lightSourceParams.OverrideLightSpriteAlpha;
			}
			set
			{
				this.lightSourceParams.OverrideLightSpriteAlpha = value;
			}
		}

		// Token: 0x17001492 RID: 5266
		// (get) Token: 0x0600509D RID: 20637 RVA: 0x002B54EB File Offset: 0x002B36EB
		public Vector2 WorldPosition
		{
			get
			{
				if (this.ParentSub != null)
				{
					return this.position + this.ParentSub.Position;
				}
				return this.position;
			}
		}

		// Token: 0x17001493 RID: 5267
		// (get) Token: 0x0600509E RID: 20638 RVA: 0x002B5512 File Offset: 0x002B3712
		public static Texture2D LightTexture
		{
			get
			{
				if (LightSource.lightTexture == null)
				{
					LightSource.lightTexture = TextureLoader.FromFile("Content/Lights/pointlight_bright.png", true, false, null);
				}
				return LightSource.lightTexture;
			}
		}

		// Token: 0x17001494 RID: 5268
		// (get) Token: 0x0600509F RID: 20639 RVA: 0x002B5532 File Offset: 0x002B3732
		public Sprite OverrideLightTexture
		{
			get
			{
				return this.lightSourceParams.OverrideLightTexture;
			}
		}

		// Token: 0x17001495 RID: 5269
		// (get) Token: 0x060050A0 RID: 20640 RVA: 0x002B553F File Offset: 0x002B373F
		public Sprite LightSprite
		{
			get
			{
				return this.lightSourceParams.LightSprite;
			}
		}

		// Token: 0x17001496 RID: 5270
		// (get) Token: 0x060050A1 RID: 20641 RVA: 0x002B554C File Offset: 0x002B374C
		private Vector2 OverrideLightTextureOrigin
		{
			get
			{
				return this.OverrideLightTexture.Origin + this.LightSourceParams.Offset;
			}
		}

		// Token: 0x17001497 RID: 5271
		// (get) Token: 0x060050A2 RID: 20642 RVA: 0x002B5569 File Offset: 0x002B3769
		// (set) Token: 0x060050A3 RID: 20643 RVA: 0x002B5576 File Offset: 0x002B3776
		public Color Color
		{
			get
			{
				return this.lightSourceParams.Color;
			}
			set
			{
				this.lightSourceParams.Color = value;
			}
		}

		// Token: 0x17001498 RID: 5272
		// (get) Token: 0x060050A4 RID: 20644 RVA: 0x002B5584 File Offset: 0x002B3784
		// (set) Token: 0x060050A5 RID: 20645 RVA: 0x002B558C File Offset: 0x002B378C
		public float CurrentBrightness { get; private set; }

		// Token: 0x17001499 RID: 5273
		// (get) Token: 0x060050A6 RID: 20646 RVA: 0x002B5595 File Offset: 0x002B3795
		// (set) Token: 0x060050A7 RID: 20647 RVA: 0x002B55A4 File Offset: 0x002B37A4
		public float Range
		{
			get
			{
				return this.lightSourceParams.Range;
			}
			set
			{
				this.lightSourceParams.Range = value;
				if (Math.Abs(this.prevCalculatedRange - this.lightSourceParams.Range) < 10f)
				{
					return;
				}
				this.HullsUpToDate.Clear();
				this.NeedsRecalculation = true;
				this.prevCalculatedRange = this.lightSourceParams.Range;
			}
		}

		// Token: 0x1700149A RID: 5274
		// (get) Token: 0x060050A8 RID: 20648 RVA: 0x002B55FF File Offset: 0x002B37FF
		// (set) Token: 0x060050A9 RID: 20649 RVA: 0x002B5607 File Offset: 0x002B3807
		public Vector2 LightTextureTargetSize
		{
			get
			{
				return this.lightTextureTargetSize;
			}
			set
			{
				this.NeedsRecalculation = true;
				this.lightTextureTargetSize = value;
				this.HullsUpToDate.Clear();
			}
		}

		// Token: 0x1700149B RID: 5275
		// (get) Token: 0x060050AA RID: 20650 RVA: 0x002B5622 File Offset: 0x002B3822
		// (set) Token: 0x060050AB RID: 20651 RVA: 0x002B562A File Offset: 0x002B382A
		public Vector2 LightTextureOffset { get; set; }

		// Token: 0x1700149C RID: 5276
		// (get) Token: 0x060050AC RID: 20652 RVA: 0x002B5633 File Offset: 0x002B3833
		// (set) Token: 0x060050AD RID: 20653 RVA: 0x002B563B File Offset: 0x002B383B
		public Vector2 LightTextureScale { get; set; } = Vector2.One;

		// Token: 0x1700149D RID: 5277
		// (get) Token: 0x060050AE RID: 20654 RVA: 0x002B5644 File Offset: 0x002B3844
		public float TextureRange
		{
			get
			{
				return this.lightSourceParams.TextureRange;
			}
		}

		// Token: 0x1700149E RID: 5278
		// (get) Token: 0x060050AF RID: 20655 RVA: 0x002B5651 File Offset: 0x002B3851
		// (set) Token: 0x060050B0 RID: 20656 RVA: 0x002B5659 File Offset: 0x002B3859
		public bool IsBackground { get; set; }

		// Token: 0x1700149F RID: 5279
		// (get) Token: 0x060050B1 RID: 20657 RVA: 0x002B5662 File Offset: 0x002B3862
		// (set) Token: 0x060050B2 RID: 20658 RVA: 0x002B566A File Offset: 0x002B386A
		public PhysicsBody ParentBody { get; set; }

		// Token: 0x170014A0 RID: 5280
		// (get) Token: 0x060050B3 RID: 20659 RVA: 0x002B5673 File Offset: 0x002B3873
		// (set) Token: 0x060050B4 RID: 20660 RVA: 0x002B567B File Offset: 0x002B387B
		public DeformableSprite DeformableLightSprite { get; private set; }

		// Token: 0x060050B5 RID: 20661 RVA: 0x002B5684 File Offset: 0x002B3884
		public LightSource(ContentXElement element, ISerializableEntity conditionalTarget = null) : this(Vector2.Zero, 100f, Color.White, null, true)
		{
			this.lightSourceParams = new LightSourceParams(element);
			this.CastShadows = element.GetAttributeBool("castshadows", true);
			this.logicalOperator = element.GetAttributeEnum<PropertyConditional.LogicalOperatorType>("comparison", this.logicalOperator);
			ContentXElement deformableLightSpriteElement = this.lightSourceParams.DeformableLightSpriteElement;
			ContentXElement contentXElement = null;
			if (deformableLightSpriteElement != contentXElement)
			{
				this.DeformableLightSprite = new DeformableSprite(this.lightSourceParams.DeformableLightSpriteElement, null, null, "", false, true, 1f);
			}
			this.conditionalTarget = conditionalTarget;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "conditional")
				{
					this.conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
				}
			}
			this.RefreshDirection();
			this.NeedsRecalculation = true;
		}

		// Token: 0x060050B6 RID: 20662 RVA: 0x002B57AC File Offset: 0x002B39AC
		public LightSource(LightSourceParams lightSourceParams) : this(Vector2.Zero, 100f, Color.White, null, true)
		{
			this.lightSourceParams = lightSourceParams;
			lightSourceParams.Persistent = true;
			ContentXElement deformableLightSpriteElement = lightSourceParams.DeformableLightSpriteElement;
			ContentXElement contentXElement = null;
			if (deformableLightSpriteElement != contentXElement)
			{
				this.DeformableLightSprite = new DeformableSprite(lightSourceParams.DeformableLightSpriteElement, null, null, "", false, true, 1f);
			}
			this.RefreshDirection();
			this.NeedsRecalculation = true;
		}

		// Token: 0x060050B7 RID: 20663 RVA: 0x002B5830 File Offset: 0x002B3A30
		public LightSource(Vector2 position, float range, Color color, Submarine submarine, bool addLight = true)
		{
			this.convexHullsInRange = new List<ConvexHullList>();
			this.ParentSub = submarine;
			this.position = position;
			this.lightSourceParams = new LightSourceParams(range, color);
			this.CastShadows = true;
			this.texture = LightSource.LightTexture;
			this.diffToSub = new Dictionary<Submarine, Vector2>();
			if (addLight)
			{
				GameMain.LightManager.AddLight(this);
			}
		}

		// Token: 0x060050B8 RID: 20664 RVA: 0x002B5917 File Offset: 0x002B3B17
		private void RefreshDirection()
		{
			this.dir = new Vector2(MathF.Cos(this.rotation - this.LightSourceParams.RotationRad), -MathF.Sin(this.rotation - this.LightSourceParams.RotationRad));
		}

		// Token: 0x060050B9 RID: 20665 RVA: 0x002B5954 File Offset: 0x002B3B54
		public void Update(float time)
		{
			float brightness = 1f;
			if (this.lightSourceParams.BlinkFrequency > 0f)
			{
				float blinkTimer = time * this.lightSourceParams.BlinkFrequency % 1f;
				if (blinkTimer > 0.5f)
				{
					this.CurrentBrightness = 0f;
					return;
				}
			}
			if (this.lightSourceParams.PulseFrequency > 0f && this.lightSourceParams.PulseAmount > 0f)
			{
				float pulseState = time * this.lightSourceParams.PulseFrequency % 1f;
				brightness *= 1f - (float)(Math.Sin((double)(pulseState * 6.2831855f)) + 1.0) / 2f * this.lightSourceParams.PulseAmount;
			}
			if (this.lightSourceParams.Flicker > 0f && this.lightSourceParams.FlickerSpeed > 0f)
			{
				float flickerState = time * this.lightSourceParams.FlickerSpeed % 255f;
				brightness *= 1f - PerlinNoise.GetPerlin(flickerState, flickerState * 0.5f) * this.lightSourceParams.Flicker;
			}
			this.CurrentBrightness = brightness;
		}

		// Token: 0x060050BA RID: 20666 RVA: 0x002B5A70 File Offset: 0x002B3C70
		private void RefreshConvexHullList(ConvexHullList chList, Vector2 lightPos, Submarine sub)
		{
			ConvexHullList fullChList = ConvexHull.HullLists.FirstOrDefault((ConvexHullList chList) => chList.Submarine == sub);
			if (fullChList == null)
			{
				return;
			}
			Vector2 ray = new Vector2(this.dir.X, -this.dir.Y) * this.TextureRange;
			Vector2 normal = new Vector2(-ray.Y, ray.X);
			chList2.List.Clear();
			foreach (ConvexHull convexHull in fullChList.List)
			{
				if (MathUtils.CircleIntersectsRectangle(lightPos, this.TextureRange, convexHull.BoundingBox))
				{
					if (this.lightSourceParams.Directional)
					{
						Rectangle bounds = convexHull.BoundingBox;
						bounds.Y -= bounds.Height;
						Vector2 vector;
						if (Vector2.Dot(ray, convexHull.BoundingBox.Center.ToVector2() - lightPos) <= 0f && !MathUtils.GetLineWorldRectangleIntersection(lightPos, lightPos + ray, bounds, out vector) && !MathUtils.GetLineWorldRectangleIntersection(lightPos + normal, lightPos - normal, bounds, out vector))
						{
							continue;
						}
					}
					chList2.List.Add(convexHull);
				}
			}
			chList2.IsHidden.RemoveWhere((ConvexHull ch) => !chList2.List.Contains(ch));
			chList2.HasBeenVisible.RemoveWhere((ConvexHull ch) => !chList2.List.Contains(ch));
			this.HullsUpToDate.Add(sub);
		}

		// Token: 0x060050BB RID: 20667 RVA: 0x002B5C38 File Offset: 0x002B3E38
		private void CheckConvexHullsInRange()
		{
			foreach (Submarine sub in Submarine.Loaded)
			{
				this.CheckHullsInRange(sub);
			}
			this.CheckHullsInRange(null);
		}

		// Token: 0x060050BC RID: 20668 RVA: 0x002B5C94 File Offset: 0x002B3E94
		private void CheckHullsInRange(Submarine sub)
		{
			ConvexHullList chList = null;
			foreach (ConvexHullList chl in this.convexHullsInRange)
			{
				if (chl.Submarine == sub)
				{
					chList = chl;
					break;
				}
			}
			if (chList == null)
			{
				chList = new ConvexHullList(sub);
				this.convexHullsInRange.Add(chList);
				this.NeedsRecalculation = true;
			}
			foreach (ConvexHull ch in chList.List)
			{
				if (ch.LastVertexChangeTime > this.LastRecalculationTime && (!chList.IsHidden.Contains(ch) || chList.HasBeenVisible.Contains(ch)))
				{
					this.NeedsRecalculation = true;
					break;
				}
			}
			Vector2 lightPos = this.position;
			if (this.ParentSub == null)
			{
				if (sub == null)
				{
					if (!this.HullsUpToDate.Contains(null))
					{
						this.RefreshConvexHullList(chList, lightPos, null);
						return;
					}
				}
				else
				{
					lightPos -= sub.Position;
					Rectangle subBorders = sub.Borders;
					subBorders.Location += sub.HiddenSubPosition.ToPoint() - new Point(0, sub.Borders.Height);
					if (!MathUtils.CircleIntersectsRectangle(lightPos, this.TextureRange, subBorders))
					{
						if (chList.List.Count > 0)
						{
							this.NeedsRecalculation = true;
						}
						chList.List.Clear();
						return;
					}
					this.RefreshConvexHullList(chList, lightPos, sub);
					return;
				}
			}
			else
			{
				if (sub == null)
				{
					return;
				}
				if (sub == this.ParentSub)
				{
					if (!this.HullsUpToDate.Contains(sub))
					{
						this.RefreshConvexHullList(chList, lightPos, sub);
						return;
					}
				}
				else
				{
					if (sub.DockedTo.Contains(this.ParentSub) && this.HullsUpToDate.Contains(sub))
					{
						return;
					}
					lightPos -= sub.Position - this.ParentSub.Position;
					Rectangle subBorders2 = sub.Borders;
					subBorders2.Location += sub.HiddenSubPosition.ToPoint() - new Point(0, sub.Borders.Height);
					if (!MathUtils.CircleIntersectsRectangle(lightPos, this.TextureRange, subBorders2))
					{
						if (chList.List.Count > 0)
						{
							this.NeedsRecalculation = true;
						}
						chList.List.Clear();
						return;
					}
					Vector2 diff = this.ParentSub.WorldPosition - sub.WorldPosition;
					Vector2 prevDiff;
					if (!this.diffToSub.TryGetValue(sub, out prevDiff))
					{
						this.diffToSub.Add(sub, diff);
						this.NeedsRecalculation = true;
					}
					else if (Vector2.DistanceSquared(diff, prevDiff) > 25f)
					{
						this.diffToSub[sub] = diff;
						this.NeedsRecalculation = true;
					}
					this.RefreshConvexHullList(chList, lightPos, sub);
				}
			}
		}

		// Token: 0x060050BD RID: 20669 RVA: 0x002B5F7C File Offset: 0x002B417C
		private void FindRaycastHits()
		{
			if (!this.CastShadows || this.Range < 1f || this.Color.A < 1)
			{
				this.state = LightSource.LightVertexState.PendingVertexRecalculation;
				return;
			}
			Vector2 drawPos = this.position;
			if (this.ParentSub != null)
			{
				drawPos += this.ParentSub.DrawPosition;
			}
			this.visibleSegments.Clear();
			foreach (ConvexHullList chList in this.convexHullsInRange)
			{
				foreach (ConvexHull hull in chList.List)
				{
					if (!hull.IsInvalid && hull.Enabled && (!chList.IsHidden.Contains(hull) || chList.HasBeenVisible.Contains(hull)))
					{
						object obj = LightSource.mutex;
						lock (obj)
						{
							hull.RefreshWorldPositions();
							hull.GetVisibleSegments(drawPos, this.visibleSegments);
							foreach (Segment visibleSegment in this.visibleSegments)
							{
								ConvexHull convexHull = visibleSegment.ConvexHull;
								bool flag2;
								if (convexHull == null)
								{
									flag2 = (null != null);
								}
								else
								{
									MapEntity parentEntity = convexHull.ParentEntity;
									flag2 = (((parentEntity != null) ? parentEntity.Submarine : null) != null);
								}
								if (flag2)
								{
									visibleSegment.SubmarineDrawPos = visibleSegment.ConvexHull.ParentEntity.Submarine.DrawPosition;
								}
							}
						}
					}
				}
				foreach (ConvexHull hull2 in chList.List)
				{
					if (!hull2.Enabled)
					{
						chList.IsHidden.Remove(hull2);
						chList.HasBeenVisible.Add(hull2);
					}
					else
					{
						chList.IsHidden.Add(hull2);
					}
				}
			}
			this.state = LightSource.LightVertexState.PendingRayCasts;
			GameMain.LightManager.AddRayCastTask(this, drawPos, this.rotation);
		}

		// Token: 0x060050BE RID: 20670 RVA: 0x002B6228 File Offset: 0x002B4428
		public void RayCastTask(Vector2 drawPos, float rotation)
		{
			this.visibleConvexHulls.Clear();
			Vector2 drawOffset = Vector2.Zero;
			float boundsExtended = this.TextureRange;
			if (this.OverrideLightTexture != null)
			{
				Vector2 overrideTextureDims = new Vector2((float)this.OverrideLightTexture.SourceRect.Width, (float)this.OverrideLightTexture.SourceRect.Height);
				Vector2 origin = this.OverrideLightTextureOrigin;
				origin /= Math.Max(overrideTextureDims.X, overrideTextureDims.Y);
				origin -= Vector2.One * 0.5f;
				if (Math.Abs(origin.X) >= 0.45f || Math.Abs(origin.Y) >= 0.45f)
				{
					boundsExtended += 5f;
				}
				origin *= this.TextureRange;
				float cos = this.dir.X;
				float sin = this.dir.Y;
				drawOffset.X = -origin.X * cos - origin.Y * sin;
				drawOffset.Y = origin.X * sin + origin.Y * cos;
			}
			Vector2 boundsMin = drawPos + drawOffset + new Vector2(-boundsExtended, -boundsExtended);
			Vector2 boundsMax = drawPos + drawOffset + new Vector2(boundsExtended, boundsExtended);
			this.boundaryCorners[0] = new SegmentPoint(boundsMax, null);
			this.boundaryCorners[1] = new SegmentPoint(new Vector2(boundsMax.X, boundsMin.Y), null);
			this.boundaryCorners[2] = new SegmentPoint(boundsMin, null);
			this.boundaryCorners[3] = new SegmentPoint(new Vector2(boundsMin.X, boundsMax.Y), null);
			for (int i = 0; i < 4; i++)
			{
				Segment s = new Segment(this.boundaryCorners[i], this.boundaryCorners[(i + 1) % 4], null);
				this.visibleSegments.Add(s);
			}
			object obj = LightSource.mutex;
			lock (obj)
			{
				for (int j = 0; j < this.visibleSegments.Count; j++)
				{
					Vector2 p1a = this.visibleSegments[j].Start.WorldPos;
					Vector2 p1b = this.visibleSegments[j].End.WorldPos;
					for (int k = j + 1; k < this.visibleSegments.Count; k++)
					{
						if (!this.visibleSegments[j].IsAxisAligned || !this.visibleSegments[k].IsAxisAligned || this.visibleSegments[j].IsHorizontal != this.visibleSegments[k].IsHorizontal)
						{
							Vector2 p2a = this.visibleSegments[k].Start.WorldPos;
							Vector2 p2b = this.visibleSegments[k].End.WorldPos;
							if (Vector2.DistanceSquared(p1a, p2a) >= 5f && Vector2.DistanceSquared(p1a, p2b) >= 5f && Vector2.DistanceSquared(p1b, p2a) >= 5f && Vector2.DistanceSquared(p1b, p2b) >= 5f)
							{
								Vector2 intersection = Vector2.Zero;
								bool intersects;
								if (this.visibleSegments[j].IsAxisAligned)
								{
									intersects = MathUtils.GetAxisAlignedLineIntersection(p2a, p2b, p1a, p1b, this.visibleSegments[j].IsHorizontal, out intersection);
								}
								else if (this.visibleSegments[k].IsAxisAligned)
								{
									intersects = MathUtils.GetAxisAlignedLineIntersection(p1a, p1b, p2a, p2b, this.visibleSegments[k].IsHorizontal, out intersection);
								}
								else
								{
									intersects = MathUtils.GetLineSegmentIntersection(p1a, p1b, p2a, p2b, out intersection);
								}
								if (intersects)
								{
									SegmentPoint start = this.visibleSegments[j].Start;
									SegmentPoint end = this.visibleSegments[j].End;
									SegmentPoint mid = new SegmentPoint(intersection, null);
									mid.Pos -= this.visibleSegments[j].SubmarineDrawPos;
									if (Vector2.DistanceSquared(start.WorldPos, mid.WorldPos) >= 5f && Vector2.DistanceSquared(end.WorldPos, mid.WorldPos) >= 5f)
									{
										Segment seg = new Segment(start, mid, this.visibleSegments[j].ConvexHull)
										{
											IsHorizontal = this.visibleSegments[j].IsHorizontal
										};
										Segment seg2 = new Segment(mid, end, this.visibleSegments[j].ConvexHull)
										{
											IsHorizontal = this.visibleSegments[j].IsHorizontal
										};
										this.visibleSegments[j] = seg;
										this.visibleSegments.Insert(j + 1, seg2);
										j--;
										break;
									}
								}
							}
						}
					}
				}
				this.points.Clear();
				for (int l = 0; l < this.visibleSegments.Count; l++)
				{
					Segment s3 = this.visibleSegments[l];
					if (Math.Abs(s3.Start.WorldPos.X - drawPos.X - drawOffset.X) > boundsExtended + 1f || Math.Abs(s3.Start.WorldPos.Y - drawPos.Y - drawOffset.Y) > boundsExtended + 1f || Math.Abs(s3.End.WorldPos.X - drawPos.X - drawOffset.X) > boundsExtended + 1f || Math.Abs(s3.End.WorldPos.Y - drawPos.Y - drawOffset.Y) > boundsExtended + 1f)
					{
						this.visibleSegments.RemoveAt(l);
						l--;
					}
					else
					{
						this.points.Add(s3.Start);
						this.points.Add(s3.End);
					}
				}
				for (int m = 0; m < this.points.Count; m += 2)
				{
					for (int n = Math.Min(m + 2, this.points.Count - 1); n > m; n--)
					{
						if (Math.Abs(this.points[m].WorldPos.X - this.points[n].WorldPos.X) < 6f && Math.Abs(this.points[m].WorldPos.Y - this.points[n].WorldPos.Y) < 6f)
						{
							this.points.RemoveAt(n);
						}
					}
				}
				try
				{
					CompareSegmentPointCW compareCW = new CompareSegmentPointCW(drawPos);
					this.points.Sort(compareCW);
				}
				catch (Exception e)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Constructing light volumes failed (");
					defaultInterpolatedStringHandler.AppendFormatted("CompareSegmentPointCW");
					defaultInterpolatedStringHandler.AppendLiteral(")! Light pos: ");
					defaultInterpolatedStringHandler.AppendFormatted<Vector2>(drawPos);
					defaultInterpolatedStringHandler.AppendLiteral(", Hull verts:\n");
					StringBuilder sb = new StringBuilder(defaultInterpolatedStringHandler.ToStringAndClear());
					foreach (SegmentPoint sp in this.points)
					{
						StringBuilder stringBuilder = sb;
						Vector2 pos = sp.Pos;
						stringBuilder.AppendLine(pos.ToString());
					}
					DebugConsole.ThrowError(sb.ToString(), e, null, false, false);
				}
				this.visibleSegments.Sort((Segment s1, Segment s2) => MathUtils.LineToPointDistanceSquared(s1.Start.WorldPos, s1.End.WorldPos, drawPos).CompareTo(MathUtils.LineToPointDistanceSquared(s2.Start.WorldPos, s2.End.WorldPos, drawPos)));
				this.verts.Clear();
				foreach (SegmentPoint p in this.points)
				{
					Vector2 diff = p.WorldPos - drawPos;
					float dist = diff.Length();
					if (dist > 0.0001f)
					{
						Vector2 dir = diff / dist;
						Vector2 dirNormal = new Vector2(-dir.Y, dir.X) * 6f;
						ValueTuple<int, Vector2> intersection2 = LightSource.RayCast(drawPos, drawPos + dir * boundsExtended * 2f - dirNormal, this.visibleSegments);
						if (intersection2.Item1 < 0)
						{
							return;
						}
						ValueTuple<int, Vector2> intersection3 = LightSource.RayCast(drawPos, drawPos + dir * boundsExtended * 2f + dirNormal, this.visibleSegments);
						if (intersection3.Item1 < 0)
						{
							return;
						}
						Segment seg3 = this.visibleSegments[intersection2.Item1];
						Segment seg4 = this.visibleSegments[intersection3.Item1];
						bool isPoint = MathUtils.LineToPointDistanceSquared(seg3.Start.WorldPos, seg3.End.WorldPos, p.WorldPos) < 25f;
						bool isPoint2 = MathUtils.LineToPointDistanceSquared(seg4.Start.WorldPos, seg4.End.WorldPos, p.WorldPos) < 25f;
						bool markAsVisible = false;
						if (isPoint && isPoint2)
						{
							this.verts.Add(p.WorldPos);
							markAsVisible = true;
						}
						else if (intersection2.Item1 != intersection3.Item1)
						{
							if (isPoint)
							{
								LightSource.<RayCastTask>g__TryAddPoints|123_1(intersection3.Item2, p.WorldPos, drawPos, this.verts);
								markAsVisible = true;
							}
							else if (isPoint2)
							{
								LightSource.<RayCastTask>g__TryAddPoints|123_1(intersection2.Item2, p.WorldPos, drawPos, this.verts);
								markAsVisible = true;
							}
							else
							{
								this.verts.Add(intersection2.Item2);
								this.verts.Add(intersection3.Item2);
							}
						}
						if (markAsVisible)
						{
							this.visibleConvexHulls.Add(p.ConvexHull);
							this.visibleConvexHulls.Add(seg3.ConvexHull);
							this.visibleConvexHulls.Add(seg4.ConvexHull);
						}
					}
				}
			}
			for (int i2 = 0; i2 < this.verts.Count - 1; i2++)
			{
				for (int j2 = this.verts.Count - 1; j2 > i2; j2--)
				{
					if (Math.Abs(this.verts[i2].X - this.verts[j2].X) < 6f && Math.Abs(this.verts[i2].Y - this.verts[j2].Y) < 6f)
					{
						this.verts.RemoveAt(j2);
					}
				}
			}
			try
			{
				CompareCW compareCW2 = new CompareCW(drawPos);
				this.verts.Sort(compareCW2);
			}
			catch (Exception e2)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Constructing light volumes failed (");
				defaultInterpolatedStringHandler2.AppendFormatted("CompareSegmentPointCW");
				defaultInterpolatedStringHandler2.AppendLiteral(")! Light pos: ");
				defaultInterpolatedStringHandler2.AppendFormatted<Vector2>(drawPos);
				defaultInterpolatedStringHandler2.AppendLiteral(", verts:\n");
				StringBuilder sb2 = new StringBuilder(defaultInterpolatedStringHandler2.ToStringAndClear());
				foreach (Vector2 v in this.verts)
				{
					sb2.AppendLine(v.ToString());
				}
				DebugConsole.ThrowError(sb2.ToString(), e2, null, false, false);
			}
			this.calculatedDrawPos = drawPos;
			this.state = LightSource.LightVertexState.PendingVertexRecalculation;
		}

		// Token: 0x060050BF RID: 20671 RVA: 0x002B6F08 File Offset: 0x002B5108
		[return: TupleElementNames(new string[]
		{
			"index",
			"pos"
		})]
		private static ValueTuple<int, Vector2> RayCast(Vector2 rayStart, Vector2 rayEnd, List<Segment> segments)
		{
			Vector2? closestIntersection = null;
			int segment = -1;
			float minX = Math.Min(rayStart.X, rayEnd.X);
			float maxX = Math.Max(rayStart.X, rayEnd.X);
			float minY = Math.Min(rayStart.Y, rayEnd.Y);
			float maxY = Math.Max(rayStart.Y, rayEnd.Y);
			for (int i = 0; i < segments.Count; i++)
			{
				Segment s = segments[i];
				if (s.Start.WorldPos.Y <= maxY && s.End.WorldPos.Y >= minY)
				{
					if (s.Start.WorldPos.X > s.End.WorldPos.X)
					{
						if (s.Start.WorldPos.X < minX)
						{
							goto IL_1CE;
						}
						if (s.End.WorldPos.X > maxX)
						{
							goto IL_1CE;
						}
					}
					else if (s.End.WorldPos.X < minX || s.Start.WorldPos.X > maxX)
					{
						goto IL_1CE;
					}
					Vector2 intersection;
					bool intersects;
					if (s.IsAxisAligned)
					{
						intersects = MathUtils.GetAxisAlignedLineIntersection(rayStart, rayEnd, s.Start.WorldPos, s.End.WorldPos, s.IsHorizontal, out intersection);
					}
					else
					{
						intersects = MathUtils.GetLineSegmentIntersection(rayStart, rayEnd, s.Start.WorldPos, s.End.WorldPos, out intersection);
					}
					if (intersects)
					{
						closestIntersection = new Vector2?(intersection);
						rayEnd = intersection;
						minX = Math.Min(rayStart.X, rayEnd.X);
						maxX = Math.Max(rayStart.X, rayEnd.X);
						minY = Math.Min(rayStart.Y, rayEnd.Y);
						maxY = Math.Max(rayStart.Y, rayEnd.Y);
						segment = i;
					}
				}
				IL_1CE:;
			}
			return new ValueTuple<int, Vector2>(segment, (closestIntersection == null) ? rayEnd : closestIntersection.Value);
		}

		// Token: 0x060050C0 RID: 20672 RVA: 0x002B7110 File Offset: 0x002B5310
		private void CalculateLightVertices(List<Vector2> rayCastHits)
		{
			this.vertexCount = rayCastHits.Count * 2 + 1;
			this.indexCount = rayCastHits.Count * 9;
			if (this.vertices == null || this.vertices.Length < this.vertexCount || this.vertices.Length > this.vertexCount * 3)
			{
				this.vertices = new VertexPositionColorTexture[this.vertexCount];
				this.indices = new short[this.indexCount];
			}
			Vector2 drawPos = this.calculatedDrawPos;
			Vector2 uvOffset = Vector2.Zero;
			Vector2 overrideTextureDims = Vector2.One;
			Vector2 dir = this.dir;
			if (this.OverrideLightTexture != null)
			{
				overrideTextureDims = new Vector2((float)this.OverrideLightTexture.SourceRect.Width, (float)this.OverrideLightTexture.SourceRect.Height);
				Vector2 origin = this.OverrideLightTextureOrigin;
				if (this.LightSpriteEffect == SpriteEffects.FlipHorizontally)
				{
					origin.X = (float)this.OverrideLightTexture.SourceRect.Width - origin.X;
					dir = -dir;
				}
				if (this.LightSpriteEffect == SpriteEffects.FlipVertically)
				{
					origin.Y = (float)this.OverrideLightTexture.SourceRect.Height - origin.Y;
				}
				uvOffset = origin / overrideTextureDims - new Vector2(0.5f, 0.5f);
			}
			this.vertices[0] = new VertexPositionColorTexture(new Vector3(this.position.X, this.position.Y, 0f), Color.White, LightSource.<CalculateLightVertices>g__GetUV|125_0(new Vector2(0.5f, 0.5f) + uvOffset, this.LightSpriteEffect));
			for (int i = 0; i < rayCastHits.Count; i++)
			{
				Vector2 vertex = rayCastHits[i];
				Vector2 prevVertex = rayCastHits[(i > 0) ? (i - 1) : (rayCastHits.Count - 1)];
				Vector2 nextVertex = rayCastHits[(i < rayCastHits.Count - 1) ? (i + 1) : 0];
				Vector2 rawDiff = vertex - drawPos;
				Vector2 nDiff = vertex - nextVertex;
				nDiff = new Vector2(-nDiff.Y, nDiff.X);
				nDiff /= Math.Max(Math.Abs(nDiff.X), Math.Abs(nDiff.Y));
				if (Vector2.DistanceSquared(nDiff, rawDiff) > Vector2.DistanceSquared(-nDiff, rawDiff))
				{
					nDiff = -nDiff;
				}
				Vector2 nDiff2 = prevVertex - vertex;
				nDiff2 = new Vector2(-nDiff2.Y, nDiff2.X);
				nDiff2 /= Math.Max(Math.Abs(nDiff2.X), Math.Abs(nDiff2.Y));
				if (Vector2.DistanceSquared(nDiff2, rawDiff) > Vector2.DistanceSquared(-nDiff2, rawDiff))
				{
					nDiff2 = -nDiff2;
				}
				float blurDistance = 25f;
				Vector2 nDiff3 = nDiff * blurDistance;
				Vector2 intersection;
				if (MathUtils.GetLineIntersection(vertex + nDiff * blurDistance, nextVertex + nDiff * blurDistance, vertex + nDiff2 * blurDistance, prevVertex + nDiff2 * blurDistance, true, out intersection))
				{
					nDiff3 = intersection - vertex;
					if (nDiff3.LengthSquared() > 10000f)
					{
						nDiff3 /= Math.Max(Math.Abs(nDiff3.X), Math.Abs(nDiff3.Y));
						nDiff3 *= 100f;
					}
				}
				Vector2 diff = rawDiff;
				diff /= this.Range * 2f;
				if (this.OverrideLightTexture != null)
				{
					Vector2 originDiff = diff;
					diff.X = originDiff.X * dir.X - originDiff.Y * dir.Y;
					diff.Y = originDiff.X * dir.Y + originDiff.Y * dir.X;
					diff *= overrideTextureDims / this.OverrideLightTexture.size;
					diff += uvOffset;
				}
				VertexPositionColorTexture fullVert = new VertexPositionColorTexture(new Vector3(this.position.X + rawDiff.X, this.position.Y + rawDiff.Y, 0f), Color.White, LightSource.<CalculateLightVertices>g__GetUV|125_0(new Vector2(0.5f, 0.5f) + diff, this.LightSpriteEffect));
				VertexPositionColorTexture fadeVert = new VertexPositionColorTexture(new Vector3(this.position.X + rawDiff.X + nDiff3.X, this.position.Y + rawDiff.Y + nDiff3.Y, 0f), Color.White * 0f, LightSource.<CalculateLightVertices>g__GetUV|125_0(new Vector2(0.5f, 0.5f) + diff, this.LightSpriteEffect));
				this.vertices[1 + i * 2] = fullVert;
				this.vertices[1 + i * 2 + 1] = fadeVert;
			}
			for (int j = 0; j < rayCastHits.Count - 1; j++)
			{
				this.indices[j * 9] = 0;
				this.indices[j * 9 + 1] = (short)((j * 2 + 3) % this.vertexCount);
				this.indices[j * 9 + 2] = (short)((j * 2 + 1) % this.vertexCount);
				this.indices[j * 9 + 3] = (short)((j * 2 + 1) % this.vertexCount);
				this.indices[j * 9 + 4] = (short)((j * 2 + 3) % this.vertexCount);
				this.indices[j * 9 + 5] = (short)((j * 2 + 4) % this.vertexCount);
				this.indices[j * 9 + 6] = (short)((j * 2 + 2) % this.vertexCount);
				this.indices[j * 9 + 7] = (short)((j * 2 + 1) % this.vertexCount);
				this.indices[j * 9 + 8] = (short)((j * 2 + 4) % this.vertexCount);
			}
			this.indices[(rayCastHits.Count - 1) * 9] = 0;
			this.indices[(rayCastHits.Count - 1) * 9 + 1] = 1;
			this.indices[(rayCastHits.Count - 1) * 9 + 2] = (short)(this.vertexCount - 2);
			this.indices[(rayCastHits.Count - 1) * 9 + 3] = 1;
			this.indices[(rayCastHits.Count - 1) * 9 + 4] = (short)(this.vertexCount - 1);
			this.indices[(rayCastHits.Count - 1) * 9 + 5] = (short)(this.vertexCount - 2);
			this.indices[(rayCastHits.Count - 1) * 9 + 6] = 1;
			this.indices[(rayCastHits.Count - 1) * 9 + 7] = 2;
			this.indices[(rayCastHits.Count - 1) * 9 + 8] = (short)(this.vertexCount - 1);
			if (this.lightVolumeBuffer == null)
			{
				this.lightVolumeBuffer = new DynamicVertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, Math.Max(64, (int)((double)this.vertexCount * 1.5)), BufferUsage.None);
				this.lightVolumeIndexBuffer = new DynamicIndexBuffer(GameMain.Instance.GraphicsDevice, typeof(short), Math.Max(192, (int)((double)this.indexCount * 1.5)), BufferUsage.None);
			}
			else if (this.vertexCount > this.lightVolumeBuffer.VertexCount || this.indexCount > this.lightVolumeIndexBuffer.IndexCount)
			{
				this.lightVolumeBuffer.Dispose();
				this.lightVolumeIndexBuffer.Dispose();
				this.lightVolumeBuffer = new DynamicVertexBuffer(GameMain.Instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, (int)((double)this.vertexCount * 1.5), BufferUsage.None);
				this.lightVolumeIndexBuffer = new DynamicIndexBuffer(GameMain.Instance.GraphicsDevice, typeof(short), (int)((double)this.indexCount * 1.5), BufferUsage.None);
			}
			this.lightVolumeBuffer.SetData<VertexPositionColorTexture>(this.vertices, 0, this.vertexCount);
			this.lightVolumeIndexBuffer.SetData<short>(this.indices, 0, this.indexCount);
			this.translateVertices = Vector2.Zero;
			this.prevCalculatedPosition = this.position;
			this.prevCalculatedRotation = this.rotation;
		}

		// Token: 0x060050C1 RID: 20673 RVA: 0x002B7968 File Offset: 0x002B5B68
		public void DrawSprite(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.DeformableLightSprite != null)
			{
				Vector2 origin = this.DeformableLightSprite.Origin + this.LightSourceParams.GetOffset();
				Vector2 drawPos = this.position;
				if (this.ParentSub != null)
				{
					drawPos += this.ParentSub.DrawPosition;
				}
				if (this.LightSpriteEffect == SpriteEffects.FlipHorizontally)
				{
					origin.X = (float)this.DeformableLightSprite.Sprite.SourceRect.Width - origin.X;
				}
				if (this.LightSpriteEffect == SpriteEffects.FlipVertically)
				{
					origin.Y = (float)this.DeformableLightSprite.Sprite.SourceRect.Height - origin.Y;
				}
				this.DeformableLightSprite.Draw(cam, new Vector3(drawPos, 0f), origin, -this.Rotation + MathHelper.ToRadians(this.LightSourceParams.Rotation), this.SpriteScale, new Color(this.Color, (this.lightSourceParams.OverrideLightSpriteAlpha ?? ((float)this.Color.A / 255f)) * this.CurrentBrightness), this.LightSpriteEffect == SpriteEffects.FlipVertically, false);
			}
			if (this.LightSprite != null)
			{
				Vector2 origin2 = this.LightSprite.Origin + this.LightSourceParams.GetOffset();
				if ((this.LightSpriteEffect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally)
				{
					origin2.X = (float)this.LightSprite.SourceRect.Width - origin2.X;
				}
				if ((this.LightSpriteEffect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically)
				{
					origin2.Y = (float)this.LightSprite.SourceRect.Height - origin2.Y;
				}
				Vector2 drawPos2 = this.position;
				if (this.ParentSub != null)
				{
					drawPos2 += this.ParentSub.DrawPosition;
				}
				drawPos2.Y = -drawPos2.Y;
				Color color = new Color(this.Color, (this.lightSourceParams.OverrideLightSpriteAlpha ?? ((float)this.Color.A / 255f)) * this.CurrentBrightness);
				if (this.LightTextureTargetSize != Vector2.Zero)
				{
					Sprite lightSprite = this.LightSprite;
					Vector2 vector = drawPos2;
					Vector2 targetSize = this.LightTextureTargetSize;
					float num = 0f;
					Color? color2 = new Color?(color);
					Vector2? startOffset = new Vector2?(this.LightTextureOffset);
					Vector2? textureScale = new Vector2?(this.LightTextureScale);
					lightSprite.DrawTiled(spriteBatch, vector, targetSize, num, null, color2, startOffset, textureScale, null);
				}
				else
				{
					this.LightSprite.Draw(spriteBatch, drawPos2, color, origin2, -this.Rotation + MathHelper.ToRadians(this.LightSourceParams.Rotation), this.SpriteScale, this.LightSpriteEffect, null);
				}
			}
			if (GameMain.DebugDraw && Screen.Selected.Cam.Zoom > 0.1f)
			{
				Vector2 drawPos3 = this.position;
				if (this.ParentSub != null)
				{
					drawPos3 += this.ParentSub.DrawPosition;
				}
				drawPos3.Y = -drawPos3.Y;
				if (this.CastShadows && Screen.Selected == GameMain.SubEditorScreen)
				{
					GUI.DrawRectangle(spriteBatch, drawPos3 - Vector2.One * 20f, Vector2.One * 40f, GUIStyle.Orange, false, 0f, 1f);
					GUI.DrawLine(spriteBatch, drawPos3 - Vector2.One * 20f, drawPos3 + Vector2.One * 20f, GUIStyle.Orange, 0f, 1f);
					GUI.DrawLine(spriteBatch, drawPos3 - new Vector2(1f, -1f) * 20f, drawPos3 + new Vector2(1f, -1f) * 20f, GUIStyle.Orange, 0f, 1f);
				}
				float timeSinceRecalculation = (float)Timing.TotalTime - this.LastRecalculationTime;
				if (timeSinceRecalculation < 0.1f)
				{
					GUI.DrawRectangle(spriteBatch, drawPos3 - Vector2.One * 10f, Vector2.One * 20f, GUIStyle.Red * (1f - timeSinceRecalculation * 10f), true, 0f, 1f);
					GUI.DrawLine(spriteBatch, drawPos3 - Vector2.One * this.Range, drawPos3 + Vector2.One * this.Range, this.Color, 0f, 1f);
					GUI.DrawLine(spriteBatch, drawPos3 - new Vector2(1f, -1f) * this.Range, drawPos3 + new Vector2(1f, -1f) * this.Range, this.Color, 0f, 1f);
				}
			}
		}

		// Token: 0x060050C2 RID: 20674 RVA: 0x002B7E91 File Offset: 0x002B6091
		public void CheckConditionals()
		{
			if (this.conditionals.None(null))
			{
				return;
			}
			if (this.conditionalTarget == null)
			{
				return;
			}
			this.Enabled = PropertyConditional.CheckConditionals(this.conditionalTarget, this.conditionals, this.logicalOperator);
		}

		// Token: 0x060050C3 RID: 20675 RVA: 0x002B7EC8 File Offset: 0x002B60C8
		public void DebugDrawVertices(SpriteBatch spriteBatch)
		{
			if (this.Range < 1f || this.Color.A < 1 || this.CurrentBrightness <= 0f)
			{
				return;
			}
			if (GameMain.DebugDraw && this.vertices != null)
			{
				PhysicsBody parentBody = this.ParentBody;
				Item it = ((parentBody != null) ? parentBody.UserData : null) as Item;
				if (it != null && it.Prefab.Identifier == "flashlight")
				{
					for (int i = 1; i < this.vertices.Length - 1; i += 2)
					{
						Vector2 vert = new Vector2(this.vertices[i].Position.X, this.vertices[i].Position.Y);
						int nextIndex = (i + 2) % this.vertices.Length;
						if (nextIndex == 0)
						{
							nextIndex++;
						}
						Vector2 vert2 = new Vector2(this.vertices[nextIndex].Position.X, this.vertices[nextIndex].Position.Y);
						if (this.ParentSub != null)
						{
							vert += this.ParentSub.DrawPosition;
							vert2 += this.ParentSub.DrawPosition;
						}
						vert.Y = -vert.Y;
						vert2.Y = -vert2.Y;
						Color randomColor = ToolBox.GradientLerp((float)i / (float)this.vertices.Length, new Color[]
						{
							Color.Magenta,
							Color.Blue,
							Color.Yellow,
							Color.Green,
							Color.Cyan,
							Color.Red,
							Color.Purple,
							Color.Yellow
						});
						GUI.DrawLine(spriteBatch, vert, vert2, randomColor * 0.8f, 0f, 2f);
					}
				}
			}
		}

		// Token: 0x060050C4 RID: 20676 RVA: 0x002B80CC File Offset: 0x002B62CC
		public void DrawLightVolume(SpriteBatch spriteBatch, BasicEffect lightEffect, Matrix transform, bool allowRecalculation, ref int recalculationCount)
		{
			if (this.Range < 1f || this.Color.A < 1 || this.CurrentBrightness <= 0f)
			{
				return;
			}
			if (!this.CastShadows)
			{
				Texture2D currentTexture = this.texture ?? LightSource.LightTexture;
				if (this.OverrideLightTexture != null)
				{
					currentTexture = this.OverrideLightTexture.Texture;
				}
				Vector2 center = (this.OverrideLightTexture == null) ? new Vector2((float)(currentTexture.Width / 2), (float)(currentTexture.Height / 2)) : this.OverrideLightTexture.Origin;
				float scale = this.Range / ((float)currentTexture.Width / 2f);
				Vector2 drawPos = this.position;
				if (this.ParentSub != null)
				{
					drawPos += this.ParentSub.DrawPosition;
				}
				drawPos.Y = -drawPos.Y;
				spriteBatch.Draw(currentTexture, drawPos, null, this.Color.Multiply(this.CurrentBrightness, false), -this.rotation + MathHelper.ToRadians(this.LightSourceParams.Rotation), center, scale, SpriteEffects.None, 1f);
				return;
			}
			this.CheckConvexHullsInRange();
			if (this.NeedsRecalculation && allowRecalculation)
			{
				if (this.state == LightSource.LightVertexState.UpToDate)
				{
					recalculationCount++;
					this.FindRaycastHits();
				}
				else if (this.state == LightSource.LightVertexState.PendingVertexRecalculation)
				{
					if (this.verts == null)
					{
						this.Enabled = false;
						return;
					}
					foreach (ConvexHull visibleConvexHull in this.visibleConvexHulls)
					{
						foreach (ConvexHullList convexHullList in this.convexHullsInRange)
						{
							convexHullList.IsHidden.Remove(visibleConvexHull);
							convexHullList.HasBeenVisible.Add(visibleConvexHull);
						}
					}
					this.CalculateLightVertices(this.verts);
					this.LastRecalculationTime = (float)Timing.TotalTime;
					this.NeedsRecalculation = this.needsRecalculationWhenUpToDate;
					this.needsRecalculationWhenUpToDate = false;
					this.state = LightSource.LightVertexState.UpToDate;
				}
			}
			if (this.vertexCount == 0)
			{
				return;
			}
			Vector2 offset = (this.ParentSub == null) ? Vector2.Zero : this.ParentSub.DrawPosition;
			lightEffect.World = Matrix.CreateTranslation(-new Vector3(this.position, 0f)) * Matrix.CreateTranslation(new Vector3(this.position + offset + this.translateVertices, 0f)) * transform;
			lightEffect.DiffuseColor = new Vector3((float)this.Color.R, (float)this.Color.G, (float)this.Color.B) * ((float)this.Color.A / 255f * this.CurrentBrightness) / 255f;
			if (this.OverrideLightTexture != null)
			{
				lightEffect.Texture = this.OverrideLightTexture.Texture;
			}
			else
			{
				lightEffect.Texture = (this.texture ?? LightSource.LightTexture);
			}
			lightEffect.CurrentTechnique.Passes[0].Apply();
			GameMain.Instance.GraphicsDevice.SetVertexBuffer(this.lightVolumeBuffer);
			GameMain.Instance.GraphicsDevice.Indices = this.lightVolumeIndexBuffer;
			GameMain.Instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.indexCount / 3);
		}

		// Token: 0x060050C5 RID: 20677 RVA: 0x002B846C File Offset: 0x002B666C
		public void Reset()
		{
			this.HullsUpToDate.Clear();
			this.convexHullsInRange.Clear();
			this.diffToSub.Clear();
			this.NeedsRecalculation = true;
			this.vertexCount = 0;
			if (this.lightVolumeBuffer != null)
			{
				this.lightVolumeBuffer.Dispose();
				this.lightVolumeBuffer = null;
			}
			this.indexCount = 0;
			if (this.lightVolumeIndexBuffer != null)
			{
				this.lightVolumeIndexBuffer.Dispose();
				this.lightVolumeIndexBuffer = null;
			}
		}

		// Token: 0x060050C6 RID: 20678 RVA: 0x002B84E4 File Offset: 0x002B66E4
		public void Remove()
		{
			if (!this.lightSourceParams.Persistent)
			{
				Sprite lightSprite = this.LightSprite;
				if (lightSprite != null)
				{
					lightSprite.Remove();
				}
				Sprite overrideLightTexture = this.OverrideLightTexture;
				if (overrideLightTexture != null)
				{
					overrideLightTexture.Remove();
				}
			}
			DeformableSprite deformableLightSprite = this.DeformableLightSprite;
			if (deformableLightSprite != null)
			{
				deformableLightSprite.Remove();
			}
			this.DeformableLightSprite = null;
			DynamicVertexBuffer dynamicVertexBuffer = this.lightVolumeBuffer;
			if (dynamicVertexBuffer != null)
			{
				dynamicVertexBuffer.Dispose();
			}
			this.lightVolumeBuffer = null;
			DynamicIndexBuffer dynamicIndexBuffer = this.lightVolumeIndexBuffer;
			if (dynamicIndexBuffer != null)
			{
				dynamicIndexBuffer.Dispose();
			}
			this.lightVolumeIndexBuffer = null;
			GameMain.LightManager.RemoveLight(this);
		}

		// Token: 0x060050C8 RID: 20680 RVA: 0x002B8580 File Offset: 0x002B6780
		[CompilerGenerated]
		internal static void <RayCastTask>g__TryAddPoints|123_1(Vector2 intersection, Vector2 point, Vector2 refPos, List<Vector2> verts)
		{
			if (Vector2.DistanceSquared(intersection, refPos) >= Vector2.DistanceSquared(point, refPos) * 0.8f)
			{
				verts.Add(point);
			}
			verts.Add(intersection);
		}

		// Token: 0x060050C9 RID: 20681 RVA: 0x002B85B8 File Offset: 0x002B67B8
		[CompilerGenerated]
		internal static Vector2 <CalculateLightVertices>g__GetUV|125_0(Vector2 vert, SpriteEffects effects)
		{
			if (effects == SpriteEffects.FlipHorizontally)
			{
				vert.X = 1f - vert.X;
			}
			else if (effects == SpriteEffects.FlipVertically)
			{
				vert.Y = 1f - vert.Y;
			}
			else if (effects == (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically))
			{
				vert.X = 1f - vert.X;
				vert.Y = 1f - vert.Y;
			}
			vert.Y = 1f - vert.Y;
			return vert;
		}

		// Token: 0x04002A88 RID: 10888
		private const float MovementRecalculationThreshold = 10f;

		// Token: 0x04002A89 RID: 10889
		private const float RotationRecalculationThreshold = 0.02f;

		// Token: 0x04002A8A RID: 10890
		private static Texture2D lightTexture;

		// Token: 0x04002A8B RID: 10891
		private VertexPositionColorTexture[] vertices;

		// Token: 0x04002A8C RID: 10892
		private short[] indices;

		// Token: 0x04002A8D RID: 10893
		private readonly List<ConvexHullList> convexHullsInRange;

		// Token: 0x04002A8E RID: 10894
		private readonly HashSet<ConvexHull> visibleConvexHulls = new HashSet<ConvexHull>();

		// Token: 0x04002A8F RID: 10895
		public Texture2D texture;

		// Token: 0x04002A90 RID: 10896
		public SpriteEffects LightSpriteEffect;

		// Token: 0x04002A91 RID: 10897
		public Submarine ParentSub;

		// Token: 0x04002A92 RID: 10898
		private bool castShadows;

		// Token: 0x04002A93 RID: 10899
		private float prevCalculatedRange;

		// Token: 0x04002A94 RID: 10900
		private Vector2 prevCalculatedPosition;

		// Token: 0x04002A95 RID: 10901
		public HashSet<Submarine> HullsUpToDate = new HashSet<Submarine>();

		// Token: 0x04002A96 RID: 10902
		private bool needsRecalculation;

		// Token: 0x04002A97 RID: 10903
		private bool needsRecalculationWhenUpToDate;

		// Token: 0x04002A99 RID: 10905
		private LightSource.LightVertexState state;

		// Token: 0x04002A9A RID: 10906
		private Vector2 calculatedDrawPos;

		// Token: 0x04002A9B RID: 10907
		private readonly Dictionary<Submarine, Vector2> diffToSub;

		// Token: 0x04002A9C RID: 10908
		private DynamicVertexBuffer lightVolumeBuffer;

		// Token: 0x04002A9D RID: 10909
		private DynamicIndexBuffer lightVolumeIndexBuffer;

		// Token: 0x04002A9E RID: 10910
		private int vertexCount;

		// Token: 0x04002A9F RID: 10911
		private int indexCount;

		// Token: 0x04002AA0 RID: 10912
		private Vector2 translateVertices;

		// Token: 0x04002AA1 RID: 10913
		private readonly LightSourceParams lightSourceParams;

		// Token: 0x04002AA2 RID: 10914
		private Vector2 position;

		// Token: 0x04002AA3 RID: 10915
		private float prevCalculatedRotation;

		// Token: 0x04002AA4 RID: 10916
		private float rotation;

		// Token: 0x04002AA5 RID: 10917
		private Vector2 dir = Vector2.UnitX;

		// Token: 0x04002AA6 RID: 10918
		private Vector2 _spriteScale = Vector2.One;

		// Token: 0x04002AA8 RID: 10920
		public float Priority;

		// Token: 0x04002AA9 RID: 10921
		public float PriorityMultiplier = 1f;

		// Token: 0x04002AAA RID: 10922
		private Vector2 lightTextureTargetSize;

		// Token: 0x04002AAF RID: 10927
		public Vector2 OffsetFromBody;

		// Token: 0x04002AB1 RID: 10929
		public bool Enabled = true;

		// Token: 0x04002AB2 RID: 10930
		private readonly ISerializableEntity conditionalTarget;

		// Token: 0x04002AB3 RID: 10931
		private readonly PropertyConditional.LogicalOperatorType logicalOperator;

		// Token: 0x04002AB4 RID: 10932
		private readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

		// Token: 0x04002AB5 RID: 10933
		private static readonly object mutex = new object();

		// Token: 0x04002AB6 RID: 10934
		private readonly List<Segment> visibleSegments = new List<Segment>();

		// Token: 0x04002AB7 RID: 10935
		private readonly List<SegmentPoint> points = new List<SegmentPoint>();

		// Token: 0x04002AB8 RID: 10936
		private readonly List<Vector2> verts = new List<Vector2>();

		// Token: 0x04002AB9 RID: 10937
		private readonly SegmentPoint[] boundaryCorners = new SegmentPoint[4];

		// Token: 0x04002ABA RID: 10938
		private const float MinPointDistance = 6f;

		// Token: 0x02001263 RID: 4707
		private enum LightVertexState
		{
			// Token: 0x04005EFB RID: 24315
			UpToDate,
			// Token: 0x04005EFC RID: 24316
			PendingRayCasts,
			// Token: 0x04005EFD RID: 24317
			PendingVertexRecalculation
		}
	}
}
