using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200003E RID: 62
	[NullableContext(1)]
	[Nullable(0)]
	internal class CircuitBoxWireRenderer
	{
		// Token: 0x06000977 RID: 2423 RVA: 0x000562B4 File Offset: 0x000544B4
		[NullableContext(2)]
		public CircuitBoxWireRenderer([Nullable(new byte[]
		{
			0,
			1
		})] Option<CircuitBoxWire> wire, Vector2 start, Vector2 end, Color color, Sprite wireSprite)
		{
			this.wire = wire;
			this.texture = (((wireSprite != null) ? wireSprite.Texture : null) ?? GUI.WhiteTexture);
			this.Recompute(start, end, color);
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x00056310 File Offset: 0x00054510
		private void UpdateColor(Color color)
		{
			for (int i = 0; i < 80; i++)
			{
				this.verts[i].Color = color;
			}
			this.lastColor = color;
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x00056344 File Offset: 0x00054544
		public void Recompute(Vector2 start, Vector2 end, Color color)
		{
			if (!MathUtils.NearlyEqual(this.lastStart, start, 0.0001f) || !MathUtils.NearlyEqual(this.lastEnd, end, 0.0001f))
			{
				this.lastStart = start;
				this.lastEnd = end;
				this.lastColor = color;
				this.skeleton = ToolBox.GetSquareLineBetweenPoints(start, end, 16f);
				Vector2[] points = this.skeleton.Points;
				Vector2 centerOfLine = (points[2] + points[3]) / 2f;
				ImmutableArray<Vector2> points2 = CircuitBoxWireRenderer.<Recompute>g__GetLinePoints|14_0(points[1], points[2], centerOfLine);
				ImmutableArray<Vector2> points3 = CircuitBoxWireRenderer.<Recompute>g__GetLinePoints|14_0(centerOfLine, points[3], points[4]);
				this.colliders[0] = CircuitBoxWireRenderer.<Recompute>g__ConstructQuads|14_1(ref this.verts, 0, points2, color);
				this.colliders[1] = CircuitBoxWireRenderer.<Recompute>g__ConstructQuads|14_1(ref this.verts, 40, points3, color);
				return;
			}
			if (this.lastColor == color)
			{
				return;
			}
			this.UpdateColor(color);
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x00056440 File Offset: 0x00054640
		public bool Contains(Vector2 pos)
		{
			pos.Y = -pos.Y;
			foreach (Vector2[] collider in this.colliders)
			{
				if (ToolBox.PointIntersectsWithPolygon(pos, collider, false))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x00056484 File Offset: 0x00054684
		public void Draw(SpriteBatch spriteBatch, Color selectionColor)
		{
			if (GameMain.DebugDraw)
			{
				for (int i = 0; i < this.skeleton.Points.Length; i++)
				{
					Vector2 point = this.skeleton.Points[i];
					spriteBatch.DrawPoint(point, Color.White, 25f);
					Vector2 pos = point - new Vector2(5f, 17f);
					string text = i.ToString();
					Color black = Color.Black;
					GUIFont largeFont = GUIStyle.LargeFont;
					GUI.DrawString(spriteBatch, pos, text, black, null, 0, largeFont, ForceUpperCase.Inherit);
				}
				spriteBatch.DrawLine(this.skeleton.Points[0], this.skeleton.Points[1], GUIStyle.Green, 2f);
				spriteBatch.DrawLine(this.skeleton.Points[1], this.skeleton.Points[2], GUIStyle.Green, 2f);
				spriteBatch.DrawLine(this.skeleton.Points[2], this.skeleton.Points[3], GUIStyle.Green, 2f);
				spriteBatch.DrawLine(this.skeleton.Points[3], this.skeleton.Points[4], GUIStyle.Green, 2f);
				spriteBatch.DrawLine(this.skeleton.Points[4], this.skeleton.Points[5], GUIStyle.Green, 2f);
			}
			CircuitBoxWire w;
			bool isSelected = this.wire.TryUnwrap(out w) && w.IsSelected;
			if (isSelected)
			{
				foreach (Vector2[] colliderPolys in this.colliders)
				{
					spriteBatch.DrawPolygon(Vector2.Zero, colliderPolys, selectionColor, 5f);
				}
			}
			spriteBatch.Draw(this.texture, this.verts, 0f, null);
			if (this.skeleton.Type == SquareLine.LineType.SixPointBackwardsLine)
			{
				Vector2 expandedEnd = this.skeleton.Points[1];
				Vector2 expandedStart = this.skeleton.Points[4];
				expandedEnd.X += 5f;
				expandedStart.X -= 5f;
				spriteBatch.DrawLineWithTexture(this.texture, this.skeleton.Points[0], expandedEnd, this.lastColor, 10f);
				spriteBatch.DrawLineWithTexture(this.texture, expandedStart, this.skeleton.Points[5], this.lastColor, 10f);
				RectangleF startKnob = new RectangleF(this.skeleton.Points[1] - new Vector2(7.5f), new Vector2(15f));
				RectangleF endKnob = new RectangleF(this.skeleton.Points[4] - new Vector2(7.5f), new Vector2(15f));
				GUI.DrawFilledRectangle(spriteBatch, startKnob, this.lastColor, 0f);
				GUI.DrawFilledRectangle(spriteBatch, endKnob, this.lastColor, 0f);
			}
			if (!GameMain.DebugDraw)
			{
				return;
			}
			foreach (Vector2[] colliderPolys2 in this.colliders)
			{
				spriteBatch.DrawPolygonInner(Vector2.Zero, colliderPolys2, Color.Lime, 1f);
			}
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x0005680C File Offset: 0x00054A0C
		[NullableContext(0)]
		[CompilerGenerated]
		internal static ImmutableArray<Vector2> <Recompute>g__GetLinePoints|14_0(Vector2 start, Vector2 control, Vector2 end)
		{
			ImmutableArray<Vector2>.Builder points = ImmutableArray.CreateBuilder<Vector2>(10);
			for (int i = 0; i < 10; i++)
			{
				float t = (float)i / 9f;
				Vector2 pos = MathUtils.Bezier(start, control, end, t);
				points.Add(pos);
			}
			return points.ToImmutable();
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x00056850 File Offset: 0x00054A50
		[CompilerGenerated]
		internal static Vector2[] <Recompute>g__ConstructQuads|14_1(ref VertexPositionColorTexture[] verts, int startOffset, IReadOnlyList<Vector2> points, Color color)
		{
			Vector2[] collider = new Vector2[36];
			int leftIndex = collider.Length - 1;
			int rightIndex = 0;
			for (int i = 0; i < points.Count - 1; i++)
			{
				bool isFirst = i == 0 && startOffset == 0;
				bool isLast = i == points.Count - 2 && startOffset > 0;
				Vector2 start = points[i];
				Vector2 end = points[i + 1];
				Vector2 dir = Vector2.Normalize(end - start);
				Vector2 length = new Vector2(dir.Y, -dir.X) * 5f;
				int vertIndex = startOffset + i * 4;
				Vector2 topRight = end + length;
				Vector2 topLeft = end - length;
				int prevIndex = vertIndex - 4;
				Vector2 bottomRight;
				Vector2 bottomLeft;
				if (prevIndex - startOffset >= 0)
				{
					Vector3 prevTopRight = verts[CircuitBoxWireRenderer.<Recompute>g__TopRight|14_3(prevIndex)].Position;
					Vector3 prevTopLeft = verts[CircuitBoxWireRenderer.<Recompute>g__TopLeft|14_4(prevIndex)].Position;
					bottomRight = CircuitBoxWireRenderer.<Recompute>g__ToVector2|14_7(prevTopRight);
					bottomLeft = CircuitBoxWireRenderer.<Recompute>g__ToVector2|14_7(prevTopLeft);
				}
				else
				{
					bottomRight = start + length;
					bottomLeft = start - length;
				}
				if (isFirst)
				{
					if (MathF.Abs(dir.Y) > MathF.Abs(dir.X))
					{
						float offset = (dir.Y < 0f) ? 5f : -5f;
						bottomRight.Y = start.Y - offset;
						bottomLeft.Y = start.Y - offset;
					}
					else
					{
						bottomRight.X = start.X;
						bottomLeft.X = start.X;
					}
				}
				else if (isLast)
				{
					if (MathF.Abs(dir.Y) > MathF.Abs(dir.X))
					{
						float offset2 = (dir.Y < 0f) ? 5f : -5f;
						topRight.Y = end.Y + offset2;
						topLeft.Y = end.Y + offset2;
					}
					else
					{
						topRight.X = end.X;
						topLeft.X = end.X;
					}
				}
				collider[rightIndex++] = bottomLeft;
				collider[rightIndex++] = topLeft;
				collider[leftIndex--] = bottomRight;
				collider[leftIndex--] = topRight;
				Vector2 uvTopRight = new Vector2(0f, 1f);
				Vector2 uvTopLeft = new Vector2(0f, 0f);
				Vector2 uvBottomRight = new Vector2(1f, 1f);
				Vector2 uvBottomLeft = new Vector2(1f, 0f);
				CircuitBoxWireRenderer.<Recompute>g__SetPos|14_2(ref verts, CircuitBoxWireRenderer.<Recompute>g__TopRight|14_3(vertIndex), topRight, color, uvTopRight);
				CircuitBoxWireRenderer.<Recompute>g__SetPos|14_2(ref verts, CircuitBoxWireRenderer.<Recompute>g__TopLeft|14_4(vertIndex), topLeft, color, uvTopLeft);
				CircuitBoxWireRenderer.<Recompute>g__SetPos|14_2(ref verts, CircuitBoxWireRenderer.<Recompute>g__BottomRight|14_5(vertIndex), bottomRight, color, uvBottomRight);
				CircuitBoxWireRenderer.<Recompute>g__SetPos|14_2(ref verts, CircuitBoxWireRenderer.<Recompute>g__BottomLeft|14_6(vertIndex), bottomLeft, color, uvBottomLeft);
			}
			return collider;
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x00056B22 File Offset: 0x00054D22
		[CompilerGenerated]
		internal static void <Recompute>g__SetPos|14_2(ref VertexPositionColorTexture[] verts, int index, Vector2 pos, Color color, Vector2 uv)
		{
			verts[index].Position = CircuitBoxWireRenderer.<Recompute>g__ToVector3|14_8(pos);
			verts[index].Color = color;
			verts[index].TextureCoordinate = uv;
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x00056B54 File Offset: 0x00054D54
		[CompilerGenerated]
		internal static Vector3 <Recompute>g__ToVector3|14_8(Vector2 v)
		{
			return new Vector3(v.X, v.Y, 0f);
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x00056B6C File Offset: 0x00054D6C
		[CompilerGenerated]
		internal static int <Recompute>g__TopRight|14_3(int vertIndex)
		{
			return vertIndex;
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x00056B6F File Offset: 0x00054D6F
		[CompilerGenerated]
		internal static int <Recompute>g__TopLeft|14_4(int vertIndex)
		{
			return vertIndex + 1;
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x00056B74 File Offset: 0x00054D74
		[CompilerGenerated]
		internal static int <Recompute>g__BottomRight|14_5(int vertIndex)
		{
			return vertIndex + 2;
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x00056B79 File Offset: 0x00054D79
		[CompilerGenerated]
		internal static int <Recompute>g__BottomLeft|14_6(int vertIndex)
		{
			return vertIndex + 3;
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x00056B7E File Offset: 0x00054D7E
		[CompilerGenerated]
		internal static Vector2 <Recompute>g__ToVector2|14_7(Vector3 v)
		{
			return new Vector2(v.X, v.Y);
		}

		// Token: 0x040004EB RID: 1259
		private const int VertsPerQuad = 4;

		// Token: 0x040004EC RID: 1260
		private const int QuadsPerLine = 10;

		// Token: 0x040004ED RID: 1261
		private const int VertsPerLine = 40;

		// Token: 0x040004EE RID: 1262
		private const int TotalVertsPerWire = 80;

		// Token: 0x040004EF RID: 1263
		private readonly Texture2D texture;

		// Token: 0x040004F0 RID: 1264
		private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[80];

		// Token: 0x040004F1 RID: 1265
		private readonly Vector2[][] colliders = new Vector2[2][];

		// Token: 0x040004F2 RID: 1266
		private SquareLine skeleton;

		// Token: 0x040004F3 RID: 1267
		private Vector2 lastStart;

		// Token: 0x040004F4 RID: 1268
		private Vector2 lastEnd;

		// Token: 0x040004F5 RID: 1269
		private Color lastColor;

		// Token: 0x040004F6 RID: 1270
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<CircuitBoxWire> wire;
	}
}
