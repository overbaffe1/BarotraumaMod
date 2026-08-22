using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000B2 RID: 178
	public static class ShapeExtensions
	{
		// Token: 0x0600162E RID: 5678 RVA: 0x000CF718 File Offset: 0x000CD918
		private static Texture2D GetTexture(SpriteBatch spriteBatch)
		{
			if (ShapeExtensions._whitePixelTexture == null)
			{
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					ShapeExtensions._whitePixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
					ShapeExtensions._whitePixelTexture.SetData<Color>(new Color[]
					{
						Color.White
					});
				});
			}
			return ShapeExtensions._whitePixelTexture;
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x000CF74F File Offset: 0x000CD94F
		public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2 position, Polygon polygon, Color color, float thickness = 1f)
		{
			spriteBatch.DrawPolygon(position, polygon.Vertices, color, thickness);
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x000CF764 File Offset: 0x000CD964
		public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2 offset, IReadOnlyList<Vector2> points, Color color, float thickness = 1f)
		{
			if (points.Count == 0)
			{
				return;
			}
			if (points.Count == 1)
			{
				spriteBatch.DrawPoint(points[0], color, (float)((int)thickness));
				return;
			}
			Texture2D texture = ShapeExtensions.GetTexture(spriteBatch);
			for (int i = 0; i < points.Count - 1; i++)
			{
				ShapeExtensions.DrawPolygonEdge(spriteBatch, points[i] + offset, points[i + 1] + offset, color, thickness);
			}
			ShapeExtensions.DrawPolygonEdge(spriteBatch, points[points.Count - 1] + offset, points[0] + offset, color, thickness);
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x000CF800 File Offset: 0x000CDA00
		public static void DrawPolygonInner(this SpriteBatch spriteBatch, Vector2 offset, IReadOnlyList<Vector2> points, Color color, float thickness = 1f)
		{
			if (points.Count == 0)
			{
				return;
			}
			if (points.Count == 1)
			{
				spriteBatch.DrawPoint(points[0], color, (float)((int)thickness));
				return;
			}
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 point = points[i] + offset;
				Vector2 point2 = points[i + 1] + offset;
				ShapeExtensions.DrawPolygonEdgeInner(spriteBatch, point, point2, color, thickness);
			}
			ShapeExtensions.DrawPolygonEdgeInner(spriteBatch, points[points.Count - 1] + offset, points[0] + offset, color, thickness);
		}

		// Token: 0x06001632 RID: 5682 RVA: 0x000CF898 File Offset: 0x000CDA98
		private static void DrawPolygonEdgeInner(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
		{
			float length = Vector2.Distance(point1, point2) + thickness;
			float angle = (float)Math.Atan2((double)(point2.Y - point1.Y), (double)(point2.X - point1.X));
			Vector2 scale = new Vector2(length, thickness);
			Vector2 middle = new Vector2((point1.X + point2.X) / 2f, (point1.Y + point2.Y) / 2f);
			Texture2D tex = GUI.WhiteTexture;
			spriteBatch.Draw(tex, middle, null, color, angle, new Vector2((float)tex.Width / 2f, (float)tex.Height / 2f), scale, SpriteEffects.None, 0f);
		}

		// Token: 0x06001633 RID: 5683 RVA: 0x000CF950 File Offset: 0x000CDB50
		private static void DrawPolygonEdge(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
		{
			float length = Vector2.Distance(point1, point2);
			float angle = (float)Math.Atan2((double)(point2.Y - point1.Y), (double)(point2.X - point1.X));
			Vector2 scale = new Vector2(length, thickness);
			spriteBatch.Draw(ShapeExtensions.GetTexture(spriteBatch), point1, null, color, angle, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		// Token: 0x06001634 RID: 5684 RVA: 0x000CF9B6 File Offset: 0x000CDBB6
		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness = 1f)
		{
			spriteBatch.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
		}

		// Token: 0x06001635 RID: 5685 RVA: 0x000CF9D4 File Offset: 0x000CDBD4
		public static void DrawLineWithTexture(this SpriteBatch spriteBatch, Texture2D tex, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
		{
			float distance = Vector2.Distance(point1, point2);
			float angle = (float)Math.Atan2((double)(point2.Y - point1.Y), (double)(point2.X - point1.X));
			spriteBatch.DrawLine(tex, point1, distance, angle, color, thickness);
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x000CFA1C File Offset: 0x000CDC1C
		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
		{
			float distance = Vector2.Distance(point1, point2);
			float angle = (float)Math.Atan2((double)(point2.Y - point1.Y), (double)(point2.X - point1.X));
			spriteBatch.DrawLine(ShapeExtensions.GetTexture(spriteBatch), point1, distance, angle, color, thickness);
		}

		// Token: 0x06001637 RID: 5687 RVA: 0x000CFA68 File Offset: 0x000CDC68
		public static void DrawLine(this SpriteBatch spriteBatch, Texture2D tex, Vector2 point, float length, float angle, Color color, float thickness = 1f)
		{
			Vector2 origin = new Vector2(0f, (float)tex.Height / 2f);
			Vector2 scale = new Vector2(length / (float)tex.Width, thickness / (float)tex.Height);
			spriteBatch.Draw(tex, point, null, color, angle, origin, scale, SpriteEffects.None, 0f);
		}

		// Token: 0x06001638 RID: 5688 RVA: 0x000CFAC5 File Offset: 0x000CDCC5
		public static void DrawPoint(this SpriteBatch spriteBatch, float x, float y, Color color, float size = 1f)
		{
			spriteBatch.DrawPoint(new Vector2(x, y), color, size);
		}

		// Token: 0x06001639 RID: 5689 RVA: 0x000CFAD8 File Offset: 0x000CDCD8
		public static void DrawPoint(this SpriteBatch spriteBatch, Vector2 position, Color color, float size = 1f)
		{
			Vector2 offset = new Vector2(0.5f) - new Vector2(size * 0.5f);
			spriteBatch.Draw(ShapeExtensions.GetTexture(spriteBatch), position + offset, null, color, 0f, Vector2.Zero, new Vector2(size), SpriteEffects.None, 0f);
		}

		// Token: 0x0600163A RID: 5690 RVA: 0x000CFB34 File Offset: 0x000CDD34
		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color, float thickness = 1f)
		{
			spriteBatch.DrawPolygon(center, ShapeExtensions.CreateCircle((double)radius, sides), color, thickness);
		}

		// Token: 0x0600163B RID: 5691 RVA: 0x000CFB49 File Offset: 0x000CDD49
		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color, float thickness = 1f)
		{
			spriteBatch.DrawPolygon(new Vector2(x, y), ShapeExtensions.CreateCircle((double)radius, sides), color, thickness);
		}

		// Token: 0x0600163C RID: 5692 RVA: 0x000CFB65 File Offset: 0x000CDD65
		public static void DrawSector(this SpriteBatch spriteBatch, Vector2 center, float radius, float radians, int sides, Color color, float offset = 0f, float thickness = 1f)
		{
			spriteBatch.DrawPolygon(center, ShapeExtensions.CreateSector((double)radius, sides, radians, offset), color, thickness);
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x000CFB80 File Offset: 0x000CDD80
		private static Vector2[] CreateSector(double radius, int sides, float radians, float offset = 0f)
		{
			Vector2[] points = new Vector2[(radians < 6.2831855f) ? (sides + 1) : sides];
			float step = radians / (float)sides;
			double theta = (double)offset;
			for (int i = 0; i < sides; i++)
			{
				points[i] = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * (float)radius;
				theta += (double)step;
			}
			return points;
		}

		// Token: 0x0600163E RID: 5694 RVA: 0x000CFBDC File Offset: 0x000CDDDC
		private static Vector2[] CreateCircle(double radius, int sides)
		{
			return ShapeExtensions.CreateSector(radius, sides, 6.2831855f, 0f);
		}

		// Token: 0x04000B29 RID: 2857
		private static Texture2D _whitePixelTexture;
	}
}
