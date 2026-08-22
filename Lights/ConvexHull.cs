using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Lights
{
	// Token: 0x020004D2 RID: 1234
	internal class ConvexHull
	{
		// Token: 0x17001465 RID: 5221
		// (get) Token: 0x06005013 RID: 20499 RVA: 0x002B0A23 File Offset: 0x002AEC23
		// (set) Token: 0x06005014 RID: 20500 RVA: 0x002B0A2B File Offset: 0x002AEC2B
		public VertexPositionColor[] ShadowVertices { get; private set; }

		// Token: 0x17001466 RID: 5222
		// (get) Token: 0x06005015 RID: 20501 RVA: 0x002B0A34 File Offset: 0x002AEC34
		// (set) Token: 0x06005016 RID: 20502 RVA: 0x002B0A3C File Offset: 0x002AEC3C
		public VertexPositionTexture[] PenumbraVertices { get; private set; }

		// Token: 0x17001467 RID: 5223
		// (get) Token: 0x06005017 RID: 20503 RVA: 0x002B0A45 File Offset: 0x002AEC45
		// (set) Token: 0x06005018 RID: 20504 RVA: 0x002B0A4D File Offset: 0x002AEC4D
		public int ShadowVertexCount { get; private set; }

		// Token: 0x17001468 RID: 5224
		// (get) Token: 0x06005019 RID: 20505 RVA: 0x002B0A56 File Offset: 0x002AEC56
		// (set) Token: 0x0600501A RID: 20506 RVA: 0x002B0A5E File Offset: 0x002AEC5E
		public int PenumbraVertexCount { get; private set; }

		// Token: 0x17001469 RID: 5225
		// (get) Token: 0x0600501B RID: 20507 RVA: 0x002B0A67 File Offset: 0x002AEC67
		// (set) Token: 0x0600501C RID: 20508 RVA: 0x002B0A6F File Offset: 0x002AEC6F
		public MapEntity ParentEntity { get; private set; }

		// Token: 0x1700146A RID: 5226
		// (get) Token: 0x0600501D RID: 20509 RVA: 0x002B0A78 File Offset: 0x002AEC78
		// (set) Token: 0x0600501E RID: 20510 RVA: 0x002B0A80 File Offset: 0x002AEC80
		public bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				this.enabled = value;
				this.LastVertexChangeTime = (float)Timing.TotalTime;
			}
		}

		// Token: 0x1700146B RID: 5227
		// (get) Token: 0x0600501F RID: 20511 RVA: 0x002B0A9F File Offset: 0x002AEC9F
		// (set) Token: 0x06005020 RID: 20512 RVA: 0x002B0AA7 File Offset: 0x002AECA7
		public float LastVertexChangeTime { get; private set; }

		// Token: 0x1700146C RID: 5228
		// (get) Token: 0x06005021 RID: 20513 RVA: 0x002B0AB0 File Offset: 0x002AECB0
		// (set) Token: 0x06005022 RID: 20514 RVA: 0x002B0AB8 File Offset: 0x002AECB8
		public Rectangle BoundingBox { get; private set; }

		// Token: 0x1700146D RID: 5229
		// (get) Token: 0x06005023 RID: 20515 RVA: 0x002B0AC1 File Offset: 0x002AECC1
		// (set) Token: 0x06005024 RID: 20516 RVA: 0x002B0AC9 File Offset: 0x002AECC9
		public bool IsInvalid { get; private set; }

		// Token: 0x06005025 RID: 20517 RVA: 0x002B0AD4 File Offset: 0x002AECD4
		public ConvexHull(Rectangle rect, bool isHorizontal, MapEntity parent)
		{
			if (ConvexHull.shadowEffect == null)
			{
				ConvexHull.shadowEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					VertexColorEnabled = true
				};
			}
			if (ConvexHull.penumbraEffect == null)
			{
				ConvexHull.penumbraEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
				{
					TextureEnabled = true,
					LightingEnabled = false,
					Texture = TextureLoader.FromFile("Content/Lights/penumbra.png", true, false, null)
				};
			}
			this.ParentEntity = parent;
			this.ShadowVertices = new VertexPositionColor[24];
			this.PenumbraVertices = new VertexPositionTexture[24];
			this.BoundingBox = rect;
			this.isHorizontal = isHorizontal;
			Vector2[] verts = new Vector2[]
			{
				new Vector2((float)rect.X, (float)rect.Bottom),
				new Vector2((float)rect.Right, (float)rect.Bottom),
				new Vector2((float)rect.Right, (float)rect.Y),
				new Vector2((float)rect.X, (float)rect.Y)
			};
			Vector2[] losVerts;
			if (this.isHorizontal)
			{
				this.thickness = rect.Height;
				losVerts = new Vector2[]
				{
					new Vector2((float)rect.X, (float)rect.Center.Y),
					new Vector2((float)rect.Right, (float)rect.Center.Y)
				};
			}
			else
			{
				this.thickness = rect.Width;
				losVerts = new Vector2[]
				{
					new Vector2((float)rect.Center.X, (float)rect.Y),
					new Vector2((float)rect.Center.X, (float)rect.Bottom)
				};
			}
			this.SetVertices(verts, losVerts, true, null);
			this.Enabled = true;
			ConvexHullList chList = ConvexHull.HullLists.Find((ConvexHullList h) => h.Submarine == parent.Submarine);
			if (chList == null)
			{
				chList = new ConvexHullList(parent.Submarine);
				ConvexHull.HullLists.Add(chList);
			}
			foreach (ConvexHull ch in chList.List)
			{
				this.MergeLosVertices(ch, true);
				ch.MergeLosVertices(this, true);
			}
			chList.List.Add(this);
		}

		// Token: 0x06005026 RID: 20518 RVA: 0x002B0D98 File Offset: 0x002AEF98
		private void MergeLosVertices(ConvexHull ch, bool refreshOtherOverlappingHulls = true)
		{
			if (ch == this)
			{
				return;
			}
			ConvexHull.<>c__DisplayClass48_0 CS$<>8__locals1;
			CS$<>8__locals1.mergeDistParallel = MathHelper.Clamp((float)ch.thickness * 0.65f, 16f, 512f);
			if (this.MaxMergeLosVerticesDist != null)
			{
				CS$<>8__locals1.mergeDistParallel = Math.Max(CS$<>8__locals1.mergeDistParallel, this.MaxMergeLosVerticesDist.Value);
			}
			else
			{
				Rectangle inflatedAABB = ch.BoundingBox;
				inflatedAABB.Inflate(2, 2);
				if (!inflatedAABB.Contains(this.losVertices[0].Pos) && !inflatedAABB.Contains(this.losVertices[1].Pos))
				{
					CS$<>8__locals1.mergeDistParallel = Math.Min(CS$<>8__locals1.mergeDistParallel, Vector2.Distance(this.losVertices[0].Pos, this.losVertices[1].Pos) * 0.5f);
				}
			}
			CS$<>8__locals1.mergeDistPerpendicular = Math.Min(CS$<>8__locals1.mergeDistParallel, (float)this.thickness * 0.35f);
			Vector2 center = (this.losVertices[0].Pos + this.losVertices[1].Pos) / 2f;
			bool changed = false;
			for (int i = 0; i < this.losVertices.Length; i++)
			{
				ConvexHull.<>c__DisplayClass48_1 CS$<>8__locals2;
				CS$<>8__locals2.segmentDir = Vector2.Normalize(this.losVertices[i].Pos - center);
				if (ConvexHull.<MergeLosVertices>g__isCloseEnough|48_0(MathUtils.GetClosestPointOnLineSegment(ch.losVertices[0].Pos, ch.losVertices[1].Pos, this.losVertices[i].Pos), this.losVertices[i].Pos, ref CS$<>8__locals1, ref CS$<>8__locals2))
				{
					Vector2 closest = MathUtils.GetClosestPointOnLineSegment(ch.losVertices[0].Pos + ch.losOffsets[0], ch.losVertices[1].Pos + ch.losOffsets[1], this.losVertices[i].Pos);
					if (ConvexHull.<MergeLosVertices>g__isCloseEnough|48_0(closest, this.losVertices[i].Pos, ref CS$<>8__locals1, ref CS$<>8__locals2))
					{
						Vector2 intersection;
						if (MathUtils.GetLineIntersection(ch.losVertices[0].Pos + ch.losOffsets[0], ch.losVertices[1].Pos + ch.losOffsets[1], this.losVertices[0].Pos, this.losVertices[1].Pos, true, out intersection) && Vector2.Dot(CS$<>8__locals2.segmentDir, intersection - this.losVertices[i].Pos) > 0f && (Vector2.DistanceSquared(intersection, this.losVertices[i].Pos) < CS$<>8__locals1.mergeDistParallel * CS$<>8__locals1.mergeDistParallel || Vector2.DistanceSquared(intersection, closest) < 256f))
						{
							closest = intersection;
						}
						if (Vector2.DistanceSquared(this.losVertices[1 - i].Pos + this.losOffsets[1 - i], closest) >= CS$<>8__locals1.mergeDistPerpendicular * CS$<>8__locals1.mergeDistPerpendicular)
						{
							this.losOffsets[i] = closest - this.losVertices[i].Pos;
							this.overlappingHulls.Add(ch);
							ch.overlappingHulls.Add(this);
							changed = true;
						}
					}
				}
			}
			if (changed && refreshOtherOverlappingHulls)
			{
				foreach (ConvexHull overlapping in this.overlappingHulls)
				{
					overlapping.MergeLosVertices(this, false);
				}
			}
		}

		// Token: 0x06005027 RID: 20519 RVA: 0x002B1194 File Offset: 0x002AF394
		public bool LosIntersects(Vector2 pos1, Vector2 pos2)
		{
			return MathUtils.LineSegmentsIntersect(this.losVertices[0].Pos + this.losOffsets[0], this.losVertices[1].Pos + this.losOffsets[1], pos1, pos2);
		}

		// Token: 0x06005028 RID: 20520 RVA: 0x002B11EC File Offset: 0x002AF3EC
		public void Rotate(Vector2 origin, float amount)
		{
			Matrix rotationMatrix = Matrix.CreateTranslation(-origin.X, -origin.Y, 0f) * Matrix.CreateRotationZ(amount) * Matrix.CreateTranslation(origin.X, origin.Y, 0f);
			this.SetVertices((from v in this.vertices
			select v.Pos).ToArray<Vector2>(), (from v in this.losVertices
			select v.Pos).ToArray<Vector2>(), true, new Matrix?(rotationMatrix));
		}

		// Token: 0x06005029 RID: 20521 RVA: 0x002B12A4 File Offset: 0x002AF4A4
		private void CalculateDimensions()
		{
			float minX = this.vertices[0].Pos.X;
			float minY = this.vertices[0].Pos.Y;
			float maxX = this.vertices[0].Pos.X;
			float maxY = this.vertices[0].Pos.Y;
			for (int i = 1; i < this.vertices.Length; i++)
			{
				minX = Math.Min(minX, this.vertices[i].Pos.X);
				minY = Math.Min(minY, this.vertices[i].Pos.Y);
				maxX = Math.Max(maxX, this.vertices[i].Pos.X);
				maxY = Math.Max(maxY, this.vertices[i].Pos.Y);
			}
			this.BoundingBox = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
		}

		// Token: 0x0600502A RID: 20522 RVA: 0x002B13B8 File Offset: 0x002AF5B8
		public void Move(Vector2 amount)
		{
			for (int i = 0; i < this.vertices.Length; i++)
			{
				SegmentPoint[] array = this.vertices;
				int num = i;
				array[num].Pos = array[num].Pos + amount;
				Segment segment = this.segments[i];
				segment.Start.Pos = segment.Start.Pos + amount;
				Segment segment2 = this.segments[i];
				segment2.End.Pos = segment2.End.Pos + amount;
			}
			for (int j = 0; j < this.losVertices.Length; j++)
			{
				SegmentPoint[] array2 = this.losVertices;
				int num2 = j;
				array2[num2].Pos = array2[num2].Pos + amount;
			}
			this.LastVertexChangeTime = (float)Timing.TotalTime;
			this.overlappingHulls.Clear();
			this.CalculateDimensions();
			if (this.ParentEntity == null)
			{
				return;
			}
			ConvexHullList chList = ConvexHull.HullLists.Find((ConvexHullList h) => h.Submarine == this.ParentEntity.Submarine);
			if (chList != null)
			{
				this.overlappingHulls.Clear();
				foreach (ConvexHull ch in chList.List)
				{
					this.MergeLosVertices(ch, true);
					ch.MergeLosVertices(this, true);
				}
			}
		}

		// Token: 0x0600502B RID: 20523 RVA: 0x002B1514 File Offset: 0x002AF714
		public static void RecalculateAll(Submarine sub)
		{
			ConvexHullList chList = ConvexHull.HullLists.Find((ConvexHullList h) => h.Submarine == sub);
			if (chList != null)
			{
				foreach (ConvexHull ch in chList.List)
				{
					ch.overlappingHulls.Clear();
					for (int i = 0; i < ch.losOffsets.Length; i++)
					{
						ch.losOffsets[i] = Vector2.Zero;
					}
				}
				for (int j = 0; j < chList.List.Count; j++)
				{
					for (int k = j + 1; k < chList.List.Count; k++)
					{
						chList.List[j].MergeLosVertices(chList.List[k], true);
						chList.List[k].MergeLosVertices(chList.List[j], true);
					}
				}
			}
		}

		// Token: 0x0600502C RID: 20524 RVA: 0x002B1638 File Offset: 0x002AF838
		public void SetVertices(Vector2[] points, Vector2[] losPoints, bool mergeOverlappingSegments = true, Matrix? rotationMatrix = null)
		{
			this.LastVertexChangeTime = (float)Timing.TotalTime;
			for (int i = 0; i < 4; i++)
			{
				this.vertices[i] = new SegmentPoint(points[i], this);
			}
			for (int j = 0; j < 2; j++)
			{
				this.losVertices[j] = new SegmentPoint(losPoints[j], this);
				this.losOffsets[j] = Vector2.Zero;
			}
			this.overlappingHulls.Clear();
			if (rotationMatrix != null)
			{
				for (int k = 0; k < this.vertices.Length; k++)
				{
					this.vertices[k].Pos = Vector2.Transform(this.vertices[k].Pos, rotationMatrix.Value);
				}
				for (int l = 0; l < this.losVertices.Length; l++)
				{
					this.losVertices[l].Pos = Vector2.Transform(this.losVertices[l].Pos, rotationMatrix.Value);
				}
			}
			for (int m = 0; m < 4; m++)
			{
				this.segments[m] = new Segment(this.vertices[m], this.vertices[(m + 1) % 4], this);
			}
			this.CalculateDimensions();
			if (this.ParentEntity == null)
			{
				return;
			}
			if (mergeOverlappingSegments)
			{
				ConvexHullList chList = ConvexHull.HullLists.Find((ConvexHullList h) => h.Submarine == this.ParentEntity.Submarine);
				if (chList != null)
				{
					this.overlappingHulls.Clear();
					foreach (ConvexHull ch in chList.List)
					{
						this.MergeLosVertices(ch, true);
					}
				}
			}
		}

		// Token: 0x0600502D RID: 20525 RVA: 0x002B1808 File Offset: 0x002AFA08
		public bool Intersects(Rectangle rect)
		{
			if (!this.Enabled)
			{
				return false;
			}
			Rectangle transformedBounds = this.BoundingBox;
			MapEntity parentEntity = this.ParentEntity;
			if (parentEntity != null && parentEntity.Submarine != null)
			{
				transformedBounds.X += (int)this.ParentEntity.Submarine.Position.X;
				transformedBounds.Y += (int)this.ParentEntity.Submarine.Position.Y;
			}
			return transformedBounds.Intersects(rect);
		}

		// Token: 0x0600502E RID: 20526 RVA: 0x002B1884 File Offset: 0x002AFA84
		public void GetVisibleSegments(Vector2 viewPosition, List<Segment> visibleSegments)
		{
			for (int i = 0; i < 4; i++)
			{
				if (ConvexHull.IsSegmentFacing(this.vertices[i].WorldPos, this.vertices[(i + 1) % 4].WorldPos, viewPosition))
				{
					visibleSegments.Add(this.segments[i]);
				}
			}
		}

		// Token: 0x0600502F RID: 20527 RVA: 0x002B18DC File Offset: 0x002AFADC
		public void RefreshWorldPositions()
		{
			for (int i = 0; i < 4; i++)
			{
				this.vertices[i].WorldPos = this.vertices[i].Pos;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.vertices[i].WorldPos, "vertices[i].Pos");
				this.segments[i].Start.WorldPos = this.segments[i].Start.Pos;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.segments[i].Start.WorldPos, "segments[i].Start.Pos");
				this.segments[i].End.WorldPos = this.segments[i].End.Pos;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.segments[i].End.WorldPos, "segments[i].End.Pos");
			}
			if (this.ParentEntity == null || this.ParentEntity.Submarine == null)
			{
				return;
			}
			for (int j = 0; j < 4; j++)
			{
				SegmentPoint[] array = this.vertices;
				int num = j;
				array[num].WorldPos = array[num].WorldPos + this.ParentEntity.Submarine.DrawPosition;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.vertices[j].WorldPos, "vertices[i].WorldPos");
				Segment segment = this.segments[j];
				segment.Start.WorldPos = segment.Start.WorldPos + this.ParentEntity.Submarine.DrawPosition;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.segments[j].Start.WorldPos, "segments[i].Start.WorldPos");
				Segment segment2 = this.segments[j];
				segment2.End.WorldPos = segment2.End.WorldPos + this.ParentEntity.Submarine.DrawPosition;
				this.<RefreshWorldPositions>g__ValidateVertex|57_0(this.segments[j].End.WorldPos, "segments[i].End.WorldPos");
			}
		}

		// Token: 0x06005030 RID: 20528 RVA: 0x002B1AD0 File Offset: 0x002AFCD0
		public void CalculateLosVertices(Vector2 lightSourcePos)
		{
			Vector3 offset = Vector3.Zero;
			if (this.ParentEntity != null && this.ParentEntity.Submarine != null)
			{
				offset = new Vector3(this.ParentEntity.Submarine.DrawPosition.X, this.ParentEntity.Submarine.DrawPosition.Y, 0f);
			}
			this.ShadowVertexCount = 0;
			for (int i = 0; i < this.losVertices.Length; i++)
			{
				int currentIndex = i;
				int nextIndex = (currentIndex + 1) % 2;
				Vector3 vertexPos0 = new Vector3(this.losVertices[currentIndex].Pos + this.losOffsets[currentIndex], 0f);
				Vector3 vertexPos = new Vector3(this.losVertices[nextIndex].Pos + this.losOffsets[nextIndex], 0f);
				if (Vector3.DistanceSquared(vertexPos0, vertexPos) >= 1f)
				{
					Vector3 L2P0 = vertexPos0 - new Vector3(lightSourcePos, 0f);
					L2P0.Normalize();
					Vector3 extruded0 = new Vector3(lightSourcePos, 0f) + L2P0 * 9000f;
					Vector3 L2P = vertexPos - new Vector3(lightSourcePos, 0f);
					L2P.Normalize();
					Vector3 extruded = new Vector3(lightSourcePos, 0f) + L2P * 9000f;
					this.ShadowVertices[this.ShadowVertexCount] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = vertexPos + offset
					};
					this.ShadowVertices[this.ShadowVertexCount + 1] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = vertexPos0 + offset
					};
					this.ShadowVertices[this.ShadowVertexCount + 2] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = extruded0 + offset
					};
					this.ShadowVertices[this.ShadowVertexCount + 3] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = vertexPos + offset
					};
					this.ShadowVertices[this.ShadowVertexCount + 4] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = extruded0 + offset
					};
					this.ShadowVertices[this.ShadowVertexCount + 5] = new VertexPositionColor
					{
						Color = Color.Black,
						Position = extruded + offset
					};
					this.ShadowVertexCount += 6;
				}
			}
			if (ConvexHull.IsSegmentFacing(this.losVertices[0].Pos, this.losVertices[1].Pos, lightSourcePos))
			{
				Array.Reverse<VertexPositionColor>(this.ShadowVertices, 0, this.ShadowVertexCount);
			}
			this.CalculateLosPenumbraVertices(lightSourcePos);
		}

		// Token: 0x06005031 RID: 20529 RVA: 0x002B1DD8 File Offset: 0x002AFFD8
		private static bool IsSegmentFacing(Vector2 segmentPos1, Vector2 segmentPos2, Vector2 viewPosition)
		{
			Vector2 segmentMid = (segmentPos1 + segmentPos2) / 2f;
			Vector2 segmentDiff = segmentPos2 - segmentPos1;
			Vector2 segmentNormal = new Vector2(-segmentDiff.Y, segmentDiff.X);
			Vector2 viewDirection = viewPosition - segmentMid;
			return Vector2.Dot(segmentNormal, viewDirection) > 0f;
		}

		// Token: 0x06005032 RID: 20530 RVA: 0x002B1E2C File Offset: 0x002B002C
		private void CalculateLosPenumbraVertices(Vector2 lightSourcePos)
		{
			Vector3 offset = Vector3.Zero;
			if (this.ParentEntity != null && this.ParentEntity.Submarine != null)
			{
				offset = new Vector3(this.ParentEntity.Submarine.DrawPosition.X, this.ParentEntity.Submarine.DrawPosition.Y, 0f);
			}
			this.PenumbraVertexCount = 0;
			for (int i = 0; i < this.losVertices.Length; i++)
			{
				int currentIndex = i;
				int nextIndex = (i + 1) % 2;
				Vector2 vertexPos0 = this.losVertices[currentIndex].Pos + this.losOffsets[currentIndex];
				Vector2 vertexPos = this.losVertices[nextIndex].Pos + this.losOffsets[nextIndex];
				if (Vector2.DistanceSquared(vertexPos0, vertexPos) >= 1f)
				{
					Vector3 penumbraStart = new Vector3(vertexPos0, 0f);
					this.PenumbraVertices[this.PenumbraVertexCount] = new VertexPositionTexture
					{
						Position = penumbraStart + offset,
						TextureCoordinate = new Vector2(0f, 1f)
					};
					for (int j = 0; j < 2; j++)
					{
						this.PenumbraVertices[this.PenumbraVertexCount + j + 1] = default(VertexPositionTexture);
						Vector3 vertexDir = penumbraStart - new Vector3(lightSourcePos, 0f);
						vertexDir.Normalize();
						Vector3 normal = (j == 0) ? new Vector3(-vertexDir.Y, vertexDir.X, 0f) : (new Vector3(vertexDir.Y, -vertexDir.X, 0f) * 0.05f);
						vertexDir = penumbraStart - (new Vector3(lightSourcePos, 0f) - normal * 20f);
						vertexDir.Normalize();
						this.PenumbraVertices[this.PenumbraVertexCount + j + 1].Position = new Vector3(lightSourcePos, 0f) + vertexDir * 9000f + offset;
						this.PenumbraVertices[this.PenumbraVertexCount + j + 1].TextureCoordinate = ((j == 0) ? new Vector2(0.05f, 0f) : new Vector2(1f, 0f));
					}
					this.PenumbraVertexCount += 3;
					penumbraStart = new Vector3(vertexPos, 0f);
					this.PenumbraVertices[this.PenumbraVertexCount] = new VertexPositionTexture
					{
						Position = penumbraStart + offset,
						TextureCoordinate = new Vector2(0f, 1f)
					};
					for (int k = 0; k < 2; k++)
					{
						this.PenumbraVertices[this.PenumbraVertexCount + (1 - k) + 1] = default(VertexPositionTexture);
						Vector3 vertexDir2 = penumbraStart - new Vector3(lightSourcePos, 0f);
						vertexDir2.Normalize();
						Vector3 normal2 = (k == 0) ? new Vector3(-vertexDir2.Y, vertexDir2.X, 0f) : (new Vector3(vertexDir2.Y, -vertexDir2.X, 0f) * 0.05f);
						vertexDir2 = penumbraStart - (new Vector3(lightSourcePos, 0f) + normal2 * 20f);
						vertexDir2.Normalize();
						this.PenumbraVertices[this.PenumbraVertexCount + (1 - k) + 1].Position = new Vector3(lightSourcePos, 0f) + vertexDir2 * 9000f + offset;
						this.PenumbraVertices[this.PenumbraVertexCount + (1 - k) + 1].TextureCoordinate = ((k == 0) ? new Vector2(0.05f, 0f) : new Vector2(1f, 0f));
					}
					this.PenumbraVertexCount += 3;
				}
			}
		}

		// Token: 0x06005033 RID: 20531 RVA: 0x002B2240 File Offset: 0x002B0440
		public void DebugDraw(SpriteBatch spriteBatch)
		{
			ConvexHull.<>c__DisplayClass61_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			this.<DebugDraw>g__DrawLine|61_0(this.losVertices[0].Pos, this.losVertices[1].Pos, Color.Gray * 0.5f, 3, ref CS$<>8__locals1);
			this.<DebugDraw>g__DrawLine|61_0(this.losVertices[0].Pos + this.losOffsets[0], this.losVertices[1].Pos + this.losOffsets[1], Color.LightGreen, 2, ref CS$<>8__locals1);
			this.<DebugDraw>g__DrawLine|61_0(GameMain.GameScreen.Cam.Position + Vector2.One * 1000f, GameMain.GameScreen.Cam.Position - Vector2.One * 1000f, Color.Magenta, 2, ref CS$<>8__locals1);
			if (GameMain.LightManager.LightingEnabled)
			{
				for (int i = 0; i < this.vertices.Length; i++)
				{
					Vector2 start = this.vertices[i].Pos;
					Vector2 end = this.vertices[(i + 1) % 4].Pos;
					this.<DebugDraw>g__DrawLine|61_0(start, end, Color.Yellow * 0.5f, 4, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06005034 RID: 20532 RVA: 0x002B23A0 File Offset: 0x002B05A0
		public static List<ConvexHull> GetHullsInRange(Vector2 position, float range, Submarine ParentSub)
		{
			List<ConvexHull> list = new List<ConvexHull>();
			foreach (ConvexHullList chList in ConvexHull.HullLists)
			{
				if (ParentSub == null)
				{
					if (chList.Submarine == null)
					{
						list.AddRange(chList.List.FindAll((ConvexHull ch) => MathUtils.CircleIntersectsRectangle(position, range, ch.BoundingBox)));
					}
					else
					{
						Rectangle subBorders = chList.Submarine.Borders;
						subBorders.Y -= chList.Submarine.Borders.Height;
						if (MathUtils.CircleIntersectsRectangle(position - chList.Submarine.WorldPosition, range, subBorders))
						{
							position -= chList.Submarine.WorldPosition - chList.Submarine.HiddenSubPosition;
							list.AddRange(chList.List.FindAll((ConvexHull ch) => MathUtils.CircleIntersectsRectangle(position, range, ch.BoundingBox)));
						}
					}
				}
				else if (chList.Submarine != null)
				{
					if (chList.Submarine == ParentSub)
					{
						list.AddRange(chList.List.FindAll((ConvexHull ch) => MathUtils.CircleIntersectsRectangle(position, range, ch.BoundingBox)));
					}
					else
					{
						position -= chList.Submarine.Position - ParentSub.Position;
						Rectangle subBorders2 = chList.Submarine.Borders;
						subBorders2.Location += chList.Submarine.HiddenSubPosition.ToPoint() - new Point(0, chList.Submarine.Borders.Height);
						if (MathUtils.CircleIntersectsRectangle(position, range, subBorders2))
						{
							list.AddRange(chList.List.FindAll((ConvexHull ch) => MathUtils.CircleIntersectsRectangle(position, range, ch.BoundingBox)));
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06005035 RID: 20533 RVA: 0x002B25EC File Offset: 0x002B07EC
		public void Remove()
		{
			ConvexHullList chList = ConvexHull.HullLists.Find((ConvexHullList h) => h.Submarine == this.ParentEntity.Submarine);
			if (chList != null)
			{
				chList.List.Remove(this);
				if (chList.List.Count == 0)
				{
					ConvexHull.HullLists.Remove(chList);
				}
				foreach (ConvexHull ch2 in this.overlappingHulls.ToList<ConvexHull>())
				{
					ch2.overlappingHulls.Remove(this);
					foreach (ConvexHull ch3 in chList.List)
					{
						ch3.MergeLosVertices(ch2, true);
					}
				}
			}
		}

		// Token: 0x06005037 RID: 20535 RVA: 0x002B26E0 File Offset: 0x002B08E0
		[CompilerGenerated]
		internal static bool <MergeLosVertices>g__isCloseEnough|48_0(Vector2 closest, Vector2 vertex, ref ConvexHull.<>c__DisplayClass48_0 A_2, ref ConvexHull.<>c__DisplayClass48_1 A_3)
		{
			float dist = Vector2.Distance(closest, vertex);
			if (dist < 0.001f)
			{
				return true;
			}
			if (dist > A_2.mergeDistParallel)
			{
				return false;
			}
			Vector2 closestDir = (closest - vertex) / dist;
			float dot = Math.Abs(Vector2.Dot(A_3.segmentDir, closestDir));
			float distAlongAxis = dist * dot;
			if (distAlongAxis > A_2.mergeDistParallel)
			{
				return false;
			}
			float distPerpendicular = dist * (1f - dot);
			return distPerpendicular <= A_2.mergeDistPerpendicular;
		}

		// Token: 0x0600503A RID: 20538 RVA: 0x002B277C File Offset: 0x002B097C
		[CompilerGenerated]
		private void <RefreshWorldPositions>g__ValidateVertex|57_0(Vector2 vertex, string debugName)
		{
			if (!MathUtils.IsValid(vertex))
			{
				this.IsInvalid = true;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid vertex on convex hull (");
				defaultInterpolatedStringHandler.AppendFormatted(debugName);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(vertex);
				defaultInterpolatedStringHandler.AppendLiteral(", parent entity: ");
				MapEntity parentEntity = this.ParentEntity;
				defaultInterpolatedStringHandler.AppendFormatted(((parentEntity != null) ? parentEntity.ToString() : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(").");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				GameAnalyticsManager.AddErrorEventOnce("ConvexHull.RefreshWorldPositions:InvalidVertex", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			}
		}

		// Token: 0x0600503B RID: 20539 RVA: 0x002B281C File Offset: 0x002B0A1C
		[CompilerGenerated]
		private void <DebugDraw>g__DrawLine|61_0(Vector2 vertexPos0, Vector2 vertexPos1, Color color, int width, ref ConvexHull.<>c__DisplayClass61_0 A_5)
		{
			if (this.ParentEntity != null && this.ParentEntity.Submarine != null)
			{
				vertexPos0 += this.ParentEntity.Submarine.DrawPosition;
				vertexPos1 += this.ParentEntity.Submarine.DrawPosition;
			}
			float alpha = 1f;
			if (LightManager.ViewTarget != null)
			{
				alpha = (ConvexHull.IsSegmentFacing(vertexPos0, vertexPos1, LightManager.ViewTarget.WorldPosition) ? 1f : 0.5f);
			}
			vertexPos0.Y = -vertexPos0.Y;
			vertexPos1.Y = -vertexPos1.Y;
			GUI.DrawLine(A_5.spriteBatch, vertexPos0, vertexPos1, color * alpha, 0f, (float)width);
		}

		// Token: 0x04002A3E RID: 10814
		public static List<ConvexHullList> HullLists = new List<ConvexHullList>();

		// Token: 0x04002A3F RID: 10815
		public static BasicEffect shadowEffect;

		// Token: 0x04002A40 RID: 10816
		public static BasicEffect penumbraEffect;

		// Token: 0x04002A41 RID: 10817
		private readonly Segment[] segments = new Segment[4];

		// Token: 0x04002A42 RID: 10818
		private readonly SegmentPoint[] vertices = new SegmentPoint[4];

		// Token: 0x04002A43 RID: 10819
		private readonly SegmentPoint[] losVertices = new SegmentPoint[2];

		// Token: 0x04002A44 RID: 10820
		private readonly Vector2[] losOffsets = new Vector2[2];

		// Token: 0x04002A45 RID: 10821
		private readonly bool isHorizontal;

		// Token: 0x04002A46 RID: 10822
		private readonly int thickness;

		// Token: 0x04002A4B RID: 10827
		public float? MaxMergeLosVerticesDist;

		// Token: 0x04002A4C RID: 10828
		private readonly HashSet<ConvexHull> overlappingHulls = new HashSet<ConvexHull>();

		// Token: 0x04002A4E RID: 10830
		private bool enabled;
	}
}
