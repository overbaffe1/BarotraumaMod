using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000159 RID: 345
	internal static class ToolBox
	{
		// Token: 0x06002A45 RID: 10821 RVA: 0x001D3460 File Offset: 0x001D1660
		[NullableContext(1)]
		public static bool PointIntersectsWithPolygon(Vector2 point, Vector2[] verts, bool checkBoundingBox = true)
		{
			Vector2 vector = point;
			float num;
			float num2;
			vector.Deconstruct(out num, out num2);
			float x = num;
			float y = num2;
			if (checkBoundingBox)
			{
				float minX = verts[0].X;
				float maxX = verts[0].X;
				float minY = verts[0].Y;
				float maxY = verts[0].Y;
				foreach (Vector2 vector in verts)
				{
					vector.Deconstruct(out num2, out num);
					float vertX = num2;
					float vertY = num;
					minX = Math.Min(vertX, minX);
					maxX = Math.Max(vertX, maxX);
					minY = Math.Min(vertY, minY);
					maxY = Math.Max(vertY, maxY);
				}
				if (x < minX || x > maxX || y < minY || y > maxY)
				{
					return false;
				}
			}
			bool isInside = false;
			int i = 0;
			int j = verts.Length - 1;
			while (i < verts.Length)
			{
				if (verts[i].Y > y != verts[j].Y > y && x < (verts[j].X - verts[i].X) * (y - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X)
				{
					isInside = !isInside;
				}
				j = i++;
			}
			return isInside;
		}

		// Token: 0x06002A46 RID: 10822 RVA: 0x001D35D8 File Offset: 0x001D17D8
		[NullableContext(1)]
		public static Vector2 GetPolygonBoundingBoxSize(List<Vector2> verticess)
		{
			float minX = verticess[0].X;
			float maxX = verticess[0].X;
			float minY = verticess[0].Y;
			float maxY = verticess[0].Y;
			foreach (Vector2 vector in verticess)
			{
				float num;
				float num2;
				vector.Deconstruct(out num, out num2);
				float vertX = num;
				float vertY = num2;
				minX = Math.Min(vertX, minX);
				maxX = Math.Max(vertX, maxX);
				minY = Math.Min(vertY, minY);
				maxY = Math.Max(vertY, maxY);
			}
			return new Vector2(maxX - minX, maxY - minY);
		}

		// Token: 0x06002A47 RID: 10823 RVA: 0x001D3698 File Offset: 0x001D1898
		[NullableContext(1)]
		public static List<Vector2> ScalePolygon(List<Vector2> vertices, Vector2 scale)
		{
			List<Vector2> newVertices = new List<Vector2>();
			Vector2 center = ToolBox.GetPolygonCentroid(vertices);
			foreach (Vector2 vert in vertices)
			{
				Vector2 centerVector = vert - center;
				Vector2 centerVectorScale = centerVector * scale;
				Vector2 scaledVector = centerVectorScale + center;
				newVertices.Add(scaledVector);
			}
			return newVertices;
		}

		// Token: 0x06002A48 RID: 10824 RVA: 0x001D3714 File Offset: 0x001D1914
		[NullableContext(1)]
		public static Vector2 GetPolygonCentroid(List<Vector2> poly)
		{
			float accumulatedArea = 0f;
			float centerX = 0f;
			float centerY = 0f;
			int i = 0;
			int j = poly.Count - 1;
			while (i < poly.Count)
			{
				float temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
				accumulatedArea += temp;
				centerX += (poly[i].X + poly[j].X) * temp;
				centerY += (poly[i].Y + poly[j].Y) * temp;
				j = i++;
			}
			if (Math.Abs(accumulatedArea) < 1E-07f)
			{
				return Vector2.Zero;
			}
			accumulatedArea *= 3f;
			return new Vector2(centerX / accumulatedArea, centerY / accumulatedArea);
		}

		// Token: 0x06002A49 RID: 10825 RVA: 0x001D37FC File Offset: 0x001D19FC
		[NullableContext(1)]
		public static List<Vector2> SnapVertices(List<Vector2> points, int treshold = 1)
		{
			Stack<Vector2> toCheck = new Stack<Vector2>();
			List<Vector2> newPoints = new List<Vector2>();
			foreach (Vector2 point in points)
			{
				toCheck.Push(point);
			}
			Vector2 point2;
			while (toCheck.TryPop(out point2))
			{
				Vector2 newPoint = new Vector2(point2.X, point2.Y);
				foreach (Vector2 otherPoint in toCheck.Concat(newPoints))
				{
					float diffX = Math.Abs(newPoint.X - otherPoint.X);
					float diffY = Math.Abs(newPoint.Y - otherPoint.Y);
					if (diffX <= (float)treshold)
					{
						newPoint.X = Math.Max(newPoint.X, otherPoint.X);
					}
					if (diffY <= (float)treshold)
					{
						newPoint.Y = Math.Max(newPoint.Y, otherPoint.Y);
					}
				}
				newPoints.Add(newPoint);
			}
			return newPoints;
		}

		// Token: 0x06002A4A RID: 10826 RVA: 0x001D3930 File Offset: 0x001D1B30
		public static ImmutableArray<RectangleF> SnapRectangles([Nullable(1)] IEnumerable<RectangleF> rects, int treshold = 1)
		{
			List<RectangleF> list = new List<RectangleF>();
			List<Vector2> points = new List<Vector2>();
			foreach (RectangleF rect in rects)
			{
				points.Add(new Vector2(rect.Left, rect.Top));
				points.Add(new Vector2(rect.Right, rect.Top));
				points.Add(new Vector2(rect.Right, rect.Bottom));
				points.Add(new Vector2(rect.Left, rect.Bottom));
			}
			points = ToolBox.SnapVertices(points, treshold);
			for (int i = 0; i < points.Count; i += 4)
			{
				Vector2 topLeft = points[i];
				Vector2 bottomRight = points[i + 2];
				list.Add(new RectangleF(topLeft, bottomRight - topLeft));
			}
			return list.ToImmutableArray<RectangleF>();
		}

		// Token: 0x06002A4B RID: 10827 RVA: 0x001D3A30 File Offset: 0x001D1C30
		[NullableContext(1)]
		public static List<List<Vector2>> CombineRectanglesIntoShape(IEnumerable<RectangleF> rectangles)
		{
			Func<RectangleF, IEnumerable<Vector2>> selector;
			if ((selector = ToolBox.<>O.<0>__RectangleToPoints) == null)
			{
				selector = (ToolBox.<>O.<0>__RectangleToPoints = new Func<RectangleF, IEnumerable<Vector2>>(ToolBox.<CombineRectanglesIntoShape>g__RectangleToPoints|6_7));
			}
			List<Vector2> points = (from point in rectangles.SelectMany(selector)
			group point by point into g
			where g.Count<Vector2>() % 2 == 1
			select g.Key).ToList<Vector2>();
			List<Vector2> sortedY = (from p in points
			orderby p.Y, p.X descending
			select p).ToList<Vector2>();
			List<Vector2> sortedX = (from p in points
			orderby p.X, p.Y descending
			select p).ToList<Vector2>();
			Dictionary<Vector2, Vector2> edgesH = new Dictionary<Vector2, Vector2>();
			Dictionary<Vector2, Vector2> edgesV = new Dictionary<Vector2, Vector2>();
			int i = 0;
			while (i < points.Count)
			{
				float currY = sortedY[i].Y;
				while (i < points.Count && Math.Abs(sortedY[i].Y - currY) < 0.01f)
				{
					edgesH[sortedY[i]] = sortedY[i + 1];
					edgesH[sortedY[i + 1]] = sortedY[i];
					i += 2;
				}
			}
			i = 0;
			while (i < points.Count)
			{
				float currX = sortedX[i].X;
				while (i < points.Count && Math.Abs(sortedX[i].X - currX) < 0.01f)
				{
					edgesV[sortedX[i]] = sortedX[i + 1];
					edgesV[sortedX[i + 1]] = sortedX[i];
					i += 2;
				}
			}
			List<List<Vector2>> polygons = new List<List<Vector2>>();
			while (edgesH.Any<KeyValuePair<Vector2, Vector2>>())
			{
				Vector2 vector;
				Vector2 vector2;
				edgesH.First<KeyValuePair<Vector2, Vector2>>().Deconstruct(out vector, out vector2);
				Vector2 key = vector;
				List<ValueTuple<Vector2, int>> polygon = new List<ValueTuple<Vector2, int>>
				{
					new ValueTuple<Vector2, int>(key, 0)
				};
				edgesH.Remove(key);
				ValueTuple<Vector2, int> valueTuple2;
				ValueTuple<Vector2, int> valueTuple3;
				do
				{
					List<ValueTuple<Vector2, int>> list = polygon;
					ValueTuple<Vector2, int> valueTuple = list[list.Count - 1];
					Vector2 curr = valueTuple.Item1;
					if (valueTuple.Item2 == 0)
					{
						Vector2 nextVertex = edgesV[curr];
						edgesV.Remove(curr);
						polygon.Add(new ValueTuple<Vector2, int>(nextVertex, 1));
					}
					else
					{
						Vector2 nextVertex2 = edgesH[curr];
						edgesH.Remove(curr);
						polygon.Add(new ValueTuple<Vector2, int>(nextVertex2, 0));
					}
					List<ValueTuple<Vector2, int>> list2 = polygon;
					valueTuple2 = list2[list2.Count - 1];
					valueTuple3 = polygon[0];
				}
				while (!(valueTuple2.Item1 == valueTuple3.Item1) || valueTuple2.Item2 != valueTuple3.Item2);
				List<ValueTuple<Vector2, int>> list3 = polygon;
				List<ValueTuple<Vector2, int>> list4 = polygon;
				list3.Remove(list4[list4.Count - 1]);
				List<Vector2> poly = (from t in polygon
				select t.Item1).ToList<Vector2>();
				foreach (Vector2 vertex in poly)
				{
					if (edgesH.ContainsKey(vertex))
					{
						edgesH.Remove(vertex);
					}
					if (edgesV.ContainsKey(vertex))
					{
						edgesV.Remove(vertex);
					}
				}
				polygons.Add(poly);
			}
			return polygons;
		}

		// Token: 0x06002A4C RID: 10828 RVA: 0x001D3E28 File Offset: 0x001D2028
		public static Vector3 RgbToHLS(this Color color)
		{
			return ToolBox.RgbToHLS(color.ToVector3());
		}

		// Token: 0x06002A4D RID: 10829 RVA: 0x001D3E38 File Offset: 0x001D2038
		public static Color HLSToRGB(Vector3 hls)
		{
			double h = (double)hls.X;
			double i = (double)hls.Y;
			double s = (double)hls.Z;
			double p2;
			if (i <= 0.5)
			{
				p2 = i * (1.0 + s);
			}
			else
			{
				p2 = i + s - i * s;
			}
			double p3 = 2.0 * i - p2;
			double double_r;
			double double_g;
			double double_b;
			if (s == 0.0)
			{
				double_r = i;
				double_g = i;
				double_b = i;
			}
			else
			{
				double_r = ToolBox.QqhToRgb(p3, p2, h + 120.0);
				double_g = ToolBox.QqhToRgb(p3, p2, h);
				double_b = ToolBox.QqhToRgb(p3, p2, h - 120.0);
			}
			return new Color((int)((byte)(double_r * 255.0)), (int)((byte)(double_g * 255.0)), (int)((byte)(double_b * 255.0)));
		}

		// Token: 0x06002A4E RID: 10830 RVA: 0x001D3F08 File Offset: 0x001D2108
		private static double QqhToRgb(double q1, double q2, double hue)
		{
			if (hue > 360.0)
			{
				hue -= 360.0;
			}
			else if (hue < 0.0)
			{
				hue += 360.0;
			}
			if (hue < 60.0)
			{
				return q1 + (q2 - q1) * hue / 60.0;
			}
			if (hue < 180.0)
			{
				return q2;
			}
			if (hue < 240.0)
			{
				return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
			}
			return q1;
		}

		// Token: 0x06002A4F RID: 10831 RVA: 0x001D3FA0 File Offset: 0x001D21A0
		public static Vector3 RGBToHSV(Color color)
		{
			float r = (float)color.R / 255f;
			float g = (float)color.G / 255f;
			float b = (float)color.B / 255f;
			float min = Math.Min(r, Math.Min(g, b));
			float max = Math.Max(r, Math.Max(g, b));
			float v = max;
			float delta = max - min;
			float s;
			float h;
			if (max != 0f)
			{
				s = delta / max;
				if (MathUtils.NearlyEqual(r, max, 0.0001f))
				{
					h = (g - b) / delta;
				}
				else if (MathUtils.NearlyEqual(g, max, 0.0001f))
				{
					h = 2f + (b - r) / delta;
				}
				else
				{
					h = 4f + (r - g) / delta;
				}
				h *= 60f;
				if (h < 0f)
				{
					h += 360f;
				}
				return new Vector3(h, s, v);
			}
			s = 0f;
			h = -1f;
			return new Vector3(h, s, v);
		}

		// Token: 0x06002A50 RID: 10832 RVA: 0x001D4094 File Offset: 0x001D2294
		public static Color Add(this Color sourceColor, Color color)
		{
			return new Color((int)(sourceColor.R + color.R), (int)(sourceColor.G + color.G), (int)(sourceColor.B + color.B), (int)(sourceColor.A + color.A));
		}

		// Token: 0x06002A51 RID: 10833 RVA: 0x001D40E4 File Offset: 0x001D22E4
		public static Color Subtract(this Color sourceColor, Color color)
		{
			return new Color((int)(sourceColor.R - color.R), (int)(sourceColor.G - color.G), (int)(sourceColor.B - color.B), (int)(sourceColor.A - color.A));
		}

		// Token: 0x06002A52 RID: 10834 RVA: 0x001D4132 File Offset: 0x001D2332
		[NullableContext(1)]
		public static LocalizedString LimitString(LocalizedString str, GUIFont font, int maxWidth)
		{
			return new LimitLString(str, font, maxWidth);
		}

		// Token: 0x06002A53 RID: 10835 RVA: 0x001D413C File Offset: 0x001D233C
		[NullableContext(1)]
		public static LocalizedString LimitString(string str, GUIFont font, int maxWidth)
		{
			return ToolBox.LimitString(str, font, maxWidth);
		}

		// Token: 0x06002A54 RID: 10836 RVA: 0x001D414C File Offset: 0x001D234C
		[NullableContext(1)]
		public static string LimitString(string str, ScalableFont font, int maxWidth)
		{
			if (maxWidth <= 0 || string.IsNullOrWhiteSpace(str))
			{
				return "";
			}
			float currWidth = font.MeasureString("...", false).X;
			for (int i = 0; i < str.Length; i++)
			{
				currWidth += font.MeasureString(str[i].ToString(), false).X;
				if (currWidth > (float)maxWidth)
				{
					return str.Substring(0, Math.Max(i - 2, 1)) + "...";
				}
			}
			return str;
		}

		// Token: 0x06002A55 RID: 10837 RVA: 0x001D41D0 File Offset: 0x001D23D0
		[NullableContext(1)]
		public static string LimitStringHeight(string str, ScalableFont font, int maxHeight)
		{
			if (maxHeight <= 0 || string.IsNullOrWhiteSpace(str))
			{
				return string.Empty;
			}
			float currHeight = font.MeasureString("...", false).Y;
			string[] lines = str.Split('\n', StringSplitOptions.None);
			StringBuilder sb = new StringBuilder();
			foreach (string line in lines)
			{
				float num;
				float num2;
				font.MeasureString(line, false).Deconstruct(out num, out num2);
				float lineX = num;
				float lineY = num2;
				currHeight += lineY;
				if (currHeight > (float)maxHeight)
				{
					string modifiedLine = line;
					while (font.MeasureString(modifiedLine + "...", false).X > lineX && modifiedLine.Length != 0)
					{
						string text = modifiedLine;
						modifiedLine = text.Substring(0, text.Length - 1);
					}
					StringBuilder stringBuilder = sb;
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder);
					appendInterpolatedStringHandler.AppendFormatted(modifiedLine);
					appendInterpolatedStringHandler.AppendLiteral("...");
					stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
					return sb.ToString();
				}
				sb.AppendLine(line);
			}
			return str;
		}

		// Token: 0x06002A56 RID: 10838 RVA: 0x001D42DC File Offset: 0x001D24DC
		[NullableContext(1)]
		public static Color GradientLerp(float t, params Color[] gradient)
		{
			if (!MathUtils.IsValid(t))
			{
				return Color.Purple;
			}
			if (gradient.Length == 0)
			{
				GameAnalyticsManager.AddErrorEventOnce("ToolBox.GradientLerp:EmptyColorArray", GameAnalyticsManager.ErrorSeverity.Error, "Empty color array passed to the GradientLerp method.\n" + Environment.StackTrace.CleanupStackTrace());
				return Color.Black;
			}
			if (t <= 0f || !MathUtils.IsValid(t))
			{
				return gradient[0];
			}
			if (t >= 1f)
			{
				return gradient[gradient.Length - 1];
			}
			float scaledT = t * (float)(gradient.Length - 1);
			return Color.Lerp(gradient[(int)scaledT], gradient[(int)Math.Min(scaledT + 1f, (float)(gradient.Length - 1))], scaledT - (float)((int)scaledT));
		}

		// Token: 0x06002A57 RID: 10839 RVA: 0x001D4380 File Offset: 0x001D2580
		[NullableContext(1)]
		public static LocalizedString WrapText(LocalizedString text, float lineLength, GUIFont font, float textScale = 1f)
		{
			return new WrappedLString(text, lineLength, font, textScale);
		}

		// Token: 0x06002A58 RID: 10840 RVA: 0x001D438B File Offset: 0x001D258B
		[NullableContext(1)]
		public static string WrapText(string text, float lineLength, ScalableFont font, float textScale = 1f)
		{
			return font.WrapText(text, lineLength / textScale);
		}

		// Token: 0x06002A59 RID: 10841 RVA: 0x001D4398 File Offset: 0x001D2598
		[NullableContext(1)]
		public static bool VersionNewerIgnoreRevision(Version a, Version b)
		{
			if (b.Major > a.Major)
			{
				return true;
			}
			if (b.Major < a.Major)
			{
				return false;
			}
			if (b.Minor > a.Minor)
			{
				return true;
			}
			if (b.Minor < a.Minor)
			{
				return false;
			}
			if (b.Build > a.Build)
			{
				return true;
			}
			int build = b.Build;
			int build2 = a.Build;
			return false;
		}

		// Token: 0x06002A5A RID: 10842 RVA: 0x001D4404 File Offset: 0x001D2604
		[NullableContext(1)]
		public static void OpenFileWithShell(string filename)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = filename,
				UseShellExecute = true
			};
			Process.Start(startInfo);
		}

		// Token: 0x06002A5B RID: 10843 RVA: 0x001D442C File Offset: 0x001D262C
		[NullableContext(1)]
		public static Vector2 PaddingSizeParentRelative(RectTransform parent, float padding)
		{
			float num;
			float num2;
			parent.NonScaledSize.ToVector2().Deconstruct(out num, out num2);
			float sizeX = num;
			float sizeY = num2;
			float higher = sizeX;
			float lower = sizeY;
			bool swap = lower > higher;
			if (swap)
			{
				float num3 = lower;
				lower = higher;
				higher = num3;
			}
			float diffY = lower - lower * padding;
			float paddingX = (higher - diffY) / higher;
			float paddingY = padding;
			if (swap)
			{
				float num4 = paddingY;
				paddingY = paddingX;
				paddingX = num4;
			}
			return new Vector2(paddingX, paddingY);
		}

		// Token: 0x06002A5C RID: 10844 RVA: 0x001D4494 File Offset: 0x001D2694
		[NullableContext(1)]
		public static string ColorSectionOfString(string text, int start, int length, Color color)
		{
			int end = start + length;
			if (start < 0 || length < 0 || end > text.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid start (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(start);
				defaultInterpolatedStringHandler.AppendLiteral(") or length (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(length);
				defaultInterpolatedStringHandler.AppendLiteral(") for text \"");
				defaultInterpolatedStringHandler.AppendFormatted(text);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				throw new ArgumentOutOfRangeException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			string stichedString = string.Empty;
			if (start > 0)
			{
				stichedString += text.Substring(0, start);
			}
			stichedString += ToolBox.<ColorSectionOfString>g__ColorString|23_0(text.Substring(start, end - start), color);
			if (end < text.Length)
			{
				stichedString += text.Substring(end);
			}
			return stichedString;
		}

		// Token: 0x06002A5D RID: 10845 RVA: 0x001D4560 File Offset: 0x001D2760
		[NullableContext(1)]
		public static byte[] HexStringToBytes(string raw)
		{
			string value = string.Join(string.Empty, raw.Split(" ", StringSplitOptions.None));
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < value.Length; i += 2)
			{
				string hex = value.Substring(i, 2);
				byte b = Convert.ToByte(hex, 16);
				bytes.Add(b);
			}
			return bytes.ToArray();
		}

		// Token: 0x17000ABC RID: 2748
		// (get) Token: 0x06002A5E RID: 10846 RVA: 0x001D45BC File Offset: 0x001D27BC
		public static Assembly BarotraumaAssembly
		{
			get
			{
				return Assembly.GetAssembly(typeof(GameMain));
			}
		}

		// Token: 0x06002A5F RID: 10847 RVA: 0x001D45D0 File Offset: 0x001D27D0
		public static bool IsProperFilenameCase(string filename)
		{
			return true;
		}

		// Token: 0x06002A60 RID: 10848 RVA: 0x001D45E0 File Offset: 0x001D27E0
		public static string CorrectFilenameCase(string filename, out bool corrected, string directory = "")
		{
			char[] delimiters = new char[]
			{
				'/',
				'\\'
			};
			string[] subDirs = filename.Split(delimiters);
			string originalFilename = filename;
			filename = "";
			corrected = false;
			string existingName;
			if (ToolBox.cachedFileNames.TryGetValue(originalFilename, out existingName))
			{
				return existingName;
			}
			string startPath = directory ?? "";
			string saveFolder = SaveUtil.DefaultSaveFolder.Replace('\\', '/');
			if (originalFilename.Replace('\\', '/').StartsWith(saveFolder))
			{
				startPath = (saveFolder.EndsWith('/') ? saveFolder : (saveFolder + "/"));
				filename = startPath;
				subDirs = subDirs.Skip(saveFolder.Split('/', StringSplitOptions.None).Length).ToArray<string>();
			}
			else if (Path.IsPathRooted(originalFilename))
			{
				return originalFilename;
			}
			for (int i = 0; i < subDirs.Length; i++)
			{
				if (i == subDirs.Length - 1 && string.IsNullOrEmpty(subDirs[i]))
				{
					break;
				}
				string subDir = subDirs[i].TrimEnd();
				string enumPath = Path.Combine(new string[]
				{
					startPath,
					filename
				});
				if (string.IsNullOrWhiteSpace(filename))
				{
					enumPath = (string.IsNullOrWhiteSpace(startPath) ? "./" : startPath);
				}
				IEnumerable<string> fileSystemEntries = Directory.GetFileSystemEntries(enumPath);
				Func<string, string> selector;
				if ((selector = ToolBox.<>O.<1>__GetFileName) == null)
				{
					selector = (ToolBox.<>O.<1>__GetFileName = new Func<string, string>(Path.GetFileName));
				}
				string[] filePaths = fileSystemEntries.Select(selector).ToArray<string>();
				if (filePaths.Any((string s) => s.Equals(subDir, StringComparison.Ordinal)))
				{
					filename += subDir;
				}
				else
				{
					string[] correctedPaths = (from s in filePaths
					where s.Equals(subDir, StringComparison.OrdinalIgnoreCase)
					select s).ToArray<string>();
					if (!correctedPaths.Any<string>())
					{
						corrected = false;
						return originalFilename;
					}
					corrected = true;
					filename += correctedPaths.First<string>();
				}
				if (i < subDirs.Length - 1)
				{
					filename += "/";
				}
			}
			ToolBox.cachedFileNames.TryAdd(originalFilename, filename);
			return filename;
		}

		// Token: 0x06002A61 RID: 10849 RVA: 0x001D47C8 File Offset: 0x001D29C8
		public static string RemoveInvalidFileNameChars(string fileName)
		{
			IEnumerable<char> invalidChars = Path.GetInvalidFileNameCharsCrossPlatform().Concat(new char[]
			{
				';'
			});
			foreach (char invalidChar in invalidChars)
			{
				fileName = fileName.Replace(invalidChar.ToString(), "");
			}
			return fileName;
		}

		// Token: 0x06002A62 RID: 10850 RVA: 0x001D4834 File Offset: 0x001D2A34
		public static string RemoveBBCodeTags(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			return ToolBox.removeBBCodeRegex.Replace(str, "");
		}

		// Token: 0x06002A63 RID: 10851 RVA: 0x001D4850 File Offset: 0x001D2A50
		public static string RandomSeed(int length)
		{
			string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			return new string((from s in Enumerable.Repeat<string>(chars, length)
			select s[Rand.Int(s.Length, Rand.RandSync.Unsynced)]).ToArray<char>());
		}

		// Token: 0x06002A64 RID: 10852 RVA: 0x001D4898 File Offset: 0x001D2A98
		public static int IdentifierToInt(Identifier id)
		{
			return ToolBox.StringToInt(id.Value.ToLowerInvariant());
		}

		// Token: 0x06002A65 RID: 10853 RVA: 0x001D48AC File Offset: 0x001D2AAC
		public static int StringToInt(string str)
		{
			int hash = 352654597;
			int hash2 = hash;
			for (int i = 0; i < str.Length; i += 2)
			{
				hash = ((hash << 5) + hash ^ (int)str[i]);
				if (i == str.Length - 1)
				{
					break;
				}
				hash2 = ((hash2 << 5) + hash2 ^ (int)str[i + 1]);
			}
			return hash + hash2 * 1566083941;
		}

		// Token: 0x06002A66 RID: 10854 RVA: 0x001D4904 File Offset: 0x001D2B04
		public static string ConvertInputType(string inputType)
		{
			if (inputType == "ActionHit" || inputType == "Action")
			{
				return "Use";
			}
			if (inputType == "SecondaryHit" || inputType == "Secondary")
			{
				return "Aim";
			}
			return inputType;
		}

		// Token: 0x06002A67 RID: 10855 RVA: 0x001D4952 File Offset: 0x001D2B52
		public static string GetDebugSymbol(bool isFinished, bool isRunning = false)
		{
			if (!isRunning)
			{
				return "[‖color:" + (isFinished ? "0,255,0‖x" : "255,0,0‖o") + "‖color:end‖]";
			}
			return "[‖color:243,162,50‖x‖color:end‖]";
		}

		// Token: 0x06002A68 RID: 10856 RVA: 0x001D497C File Offset: 0x001D2B7C
		public static string ColorizeObject(this object obj)
		{
			string text;
			if (obj is bool)
			{
				bool b = (bool)obj;
				text = (b ? "80,250,123" : "255,85,85");
			}
			else if (!(obj is string))
			{
				if (!(obj is Identifier))
				{
					if (!(obj is int))
					{
						if (!(obj is float))
						{
							if (!(obj is double))
							{
								if (obj != null)
								{
									text = "139,233,253";
								}
								else
								{
									text = "255,85,85";
								}
							}
							else
							{
								text = "189,147,249";
							}
						}
						else
						{
							text = "189,147,249";
						}
					}
					else
					{
						text = "189,147,249";
					}
				}
				else
				{
					text = "241,250,140";
				}
			}
			else
			{
				text = "241,250,140";
			}
			string color = text;
			if (!(obj is string) && !(obj is Identifier))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(color);
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<object>(obj ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler2.AppendFormatted(color);
			defaultInterpolatedStringHandler2.AppendLiteral("‖\"");
			defaultInterpolatedStringHandler2.AppendFormatted<object>(obj);
			defaultInterpolatedStringHandler2.AppendLiteral("\"‖color:end‖");
			return defaultInterpolatedStringHandler2.ToStringAndClear();
		}

		// Token: 0x06002A69 RID: 10857 RVA: 0x001D4AB8 File Offset: 0x001D2CB8
		public static Vector3 RgbToHLS(Vector3 color)
		{
			double double_r = (double)color.X;
			double double_g = (double)color.Y;
			double double_b = (double)color.Z;
			double max = double_r;
			if (max < double_g)
			{
				max = double_g;
			}
			if (max < double_b)
			{
				max = double_b;
			}
			double min = double_r;
			if (min > double_g)
			{
				min = double_g;
			}
			if (min > double_b)
			{
				min = double_b;
			}
			double diff = max - min;
			double i = (max + min) / 2.0;
			double s;
			double h;
			if (Math.Abs(diff) < 1E-05)
			{
				s = 0.0;
				h = 0.0;
			}
			else
			{
				if (i <= 0.5)
				{
					s = diff / (max + min);
				}
				else
				{
					s = diff / (2.0 - max - min);
				}
				double r_dist = (max - double_r) / diff;
				double g_dist = (max - double_g) / diff;
				double b_dist = (max - double_b) / diff;
				if (double_r == max)
				{
					h = b_dist - g_dist;
				}
				else if (double_g == max)
				{
					h = 2.0 + r_dist - b_dist;
				}
				else
				{
					h = 4.0 + g_dist - r_dist;
				}
				h *= 60.0;
				if (h < 0.0)
				{
					h += 360.0;
				}
			}
			return new Vector3((float)h, (float)i, (float)s);
		}

		// Token: 0x06002A6A RID: 10858 RVA: 0x001D4BFC File Offset: 0x001D2DFC
		public static int LevenshteinDistance(string s, string t)
		{
			int i = s.Length;
			int j = t.Length;
			int[,] d = new int[i + 1, j + 1];
			if (i == 0 || j == 0)
			{
				return 0;
			}
			int k = 0;
			while (k <= i)
			{
				d[k, 0] = k++;
			}
			int l = 0;
			while (l <= j)
			{
				d[0, l] = l++;
			}
			for (int m = 1; m <= i; m++)
			{
				for (int n = 1; n <= j; n++)
				{
					int cost = (t[n - 1] != s[m - 1]) ? 1 : 0;
					d[m, n] = Math.Min(Math.Min(d[m - 1, n] + 1, d[m, n - 1] + 1), d[m - 1, n - 1] + cost);
				}
			}
			return d[i, j];
		}

		// Token: 0x06002A6B RID: 10859 RVA: 0x001D4CE0 File Offset: 0x001D2EE0
		public static LocalizedString SecondsToReadableTime(float seconds)
		{
			int s = (int)(seconds % 60f);
			if (seconds < 60f)
			{
				return TextManager.GetWithVariable("timeformatseconds", "[seconds]", s.ToString(), FormatCapitals.No);
			}
			int h = (int)(seconds / 3600f);
			int i = (int)(seconds / 60f % 60f);
			LocalizedString text = "";
			if (h != 0)
			{
				text = TextManager.GetWithVariable("timeformathours", "[hours]", h.ToString(), FormatCapitals.No);
			}
			if (i != 0)
			{
				LocalizedString minutesText = TextManager.GetWithVariable("timeformatminutes", "[minutes]", i.ToString(), FormatCapitals.No);
				text = (text.IsNullOrEmpty() ? minutesText : LocalizedString.Join(" ", new LocalizedString[]
				{
					text,
					minutesText
				}));
			}
			if (s != 0)
			{
				LocalizedString secondsText = TextManager.GetWithVariable("timeformatseconds", "[seconds]", s.ToString(), FormatCapitals.No);
				text = (text.IsNullOrEmpty() ? secondsText : LocalizedString.Join(" ", new LocalizedString[]
				{
					text,
					secondsText
				}));
			}
			return text;
		}

		// Token: 0x06002A6C RID: 10860 RVA: 0x001D4DF0 File Offset: 0x001D2FF0
		public static string GetRandomLine(string filePath, Rand.RandSync randSync = Rand.RandSync.ServerAndClient)
		{
			List<string> lines;
			if (ToolBox.cachedLines.ContainsKey(filePath))
			{
				lines = ToolBox.cachedLines[filePath];
			}
			else
			{
				try
				{
					lines = File.ReadAllLines(filePath, null, false).ToList<string>();
					ToolBox.cachedLines.Add(filePath, lines);
					if (lines.Count == 0)
					{
						DebugConsole.ThrowError("File \"" + filePath + "\" is empty!", null, null, false, false);
						return "";
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Couldn't open file \"" + filePath + "\"!", e, null, false, false);
					return "";
				}
			}
			if (lines.Count == 0)
			{
				return "";
			}
			return lines[Rand.Range(0, lines.Count, randSync)];
		}

		// Token: 0x06002A6D RID: 10861 RVA: 0x001D4EB4 File Offset: 0x001D30B4
		public static IReadMessage ExtractBits(this IReadMessage originalBuffer, int numberOfBits)
		{
			ReadWriteMessage buffer = new ReadWriteMessage();
			for (int i = 0; i < numberOfBits; i++)
			{
				bool bit = originalBuffer.ReadBoolean();
				buffer.WriteBoolean(bit);
			}
			buffer.BitPosition = 0;
			return buffer;
		}

		// Token: 0x06002A6E RID: 10862 RVA: 0x001D4EE9 File Offset: 0x001D30E9
		public static T SelectWeightedRandom<T>(IEnumerable<T> objects, Func<T, float> weightMethod, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(objects, weightMethod, Rand.GetRNG(randSync));
		}

		// Token: 0x06002A6F RID: 10863 RVA: 0x001D4EF8 File Offset: 0x001D30F8
		public static T SelectWeightedRandom<T>(IEnumerable<T> objects, Func<T, float> weightMethod, Random random)
		{
			if (typeof(PrefabWithUintIdentifier).IsAssignableFrom(typeof(T)))
			{
				objects = objects.OrderBy(delegate(T p)
				{
					PrefabWithUintIdentifier prefabWithUintIdentifier = p as PrefabWithUintIdentifier;
					if (prefabWithUintIdentifier == null)
					{
						return 0U;
					}
					return prefabWithUintIdentifier.UintIdentifier;
				});
			}
			List<T> objectList = objects.ToList<T>();
			List<float> weights = objectList.Select(weightMethod).ToList<float>();
			return ToolBox.SelectWeightedRandom<T>(objectList, weights, random);
		}

		// Token: 0x06002A70 RID: 10864 RVA: 0x001D4F63 File Offset: 0x001D3163
		public static T SelectWeightedRandom<T>(IList<T> objects, IList<float> weights, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(objects, weights, Rand.GetRNG(randSync));
		}

		// Token: 0x06002A71 RID: 10865 RVA: 0x001D4F74 File Offset: 0x001D3174
		public static T SelectWeightedRandom<T>(IList<T> objects, IList<float> weights, Random random)
		{
			if (objects.Count == 0)
			{
				return default(T);
			}
			if (objects.Count != weights.Count)
			{
				DebugConsole.ThrowError("Error in SelectWeightedRandom, number of objects does not match the number of weights.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return objects[0];
			}
			float totalWeight = weights.Sum();
			float randomNum = (float)(random.NextDouble() * (double)totalWeight);
			T objectWithNonZeroWeight = default(T);
			for (int i = 0; i < objects.Count; i++)
			{
				if (weights[i] > 0f)
				{
					objectWithNonZeroWeight = objects[i];
				}
				if (randomNum <= weights[i])
				{
					return objects[i];
				}
				randomNum -= weights[i];
			}
			return objectWithNonZeroWeight;
		}

		// Token: 0x06002A72 RID: 10866 RVA: 0x001D502D File Offset: 0x001D322D
		public static T CreateCopy<T>(this T source, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : new()
		{
			return ToolBox.CopyValues<T>(source, Activator.CreateInstance<T>(), flags);
		}

		// Token: 0x06002A73 RID: 10867 RVA: 0x001D503B File Offset: 0x001D323B
		public static T CopyValuesTo<T>(this T source, T target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			return ToolBox.CopyValues<T>(source, target, flags);
		}

		// Token: 0x06002A74 RID: 10868 RVA: 0x001D5048 File Offset: 0x001D3248
		public static T CopyValues<T>(T source, T destination, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			if (source == null)
			{
				throw new Exception("Failed to copy object. Source is null.");
			}
			if (destination == null)
			{
				throw new Exception("Failed to copy object. Destination is null.");
			}
			Type type = source.GetType();
			PropertyInfo[] properties = type.GetProperties(flags);
			foreach (PropertyInfo property in properties)
			{
				if (property.CanWrite)
				{
					property.SetValue(destination, property.GetValue(source, null), null);
				}
			}
			FieldInfo[] fields = type.GetFields(flags);
			foreach (FieldInfo field in fields)
			{
				field.SetValue(destination, field.GetValue(source));
			}
			return destination;
		}

		// Token: 0x06002A75 RID: 10869 RVA: 0x001D5110 File Offset: 0x001D3310
		public static void SiftElement<T>(this List<T> list, int from, int to)
		{
			if (from < 0 || from >= list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("from parameter out of range (from=");
				defaultInterpolatedStringHandler.AppendFormatted<int>(from);
				defaultInterpolatedStringHandler.AppendLiteral(", range=[0..");
				defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count - 1);
				defaultInterpolatedStringHandler.AppendLiteral("])");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (to < 0 || to >= list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("to parameter out of range (to=");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(to);
				defaultInterpolatedStringHandler2.AppendLiteral(", range=[0..");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(list.Count - 1);
				defaultInterpolatedStringHandler2.AppendLiteral("])");
				throw new ArgumentException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			T elem = list[from];
			if (from > to)
			{
				for (int i = from; i > to; i--)
				{
					list[i] = list[i - 1];
				}
				list[to] = elem;
				return;
			}
			if (from < to)
			{
				for (int j = from; j < to; j++)
				{
					list[j] = list[j + 1];
				}
				list[to] = elem;
			}
		}

		// Token: 0x06002A76 RID: 10870 RVA: 0x001D523A File Offset: 0x001D343A
		public static string EscapeCharacters(string str)
		{
			return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}

		// Token: 0x06002A77 RID: 10871 RVA: 0x001D525C File Offset: 0x001D345C
		public static string UnescapeCharacters(string str)
		{
			string retVal = "";
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != '\\')
				{
					ReadOnlySpan<char> str2 = retVal;
					char c = str[i];
					retVal = str2 + new ReadOnlySpan<char>(ref c);
				}
				else if (i + 1 < str.Length)
				{
					if (str[i + 1] == '\\')
					{
						retVal += "\\";
					}
					else if (str[i + 1] == '"')
					{
						retVal += "\"";
					}
					i++;
				}
			}
			return retVal;
		}

		// Token: 0x06002A78 RID: 10872 RVA: 0x001D52EC File Offset: 0x001D34EC
		public static string[] SplitCommand(string command)
		{
			command = command.Trim();
			List<string> commands = new List<string>();
			int escape = 0;
			bool inQuotes = false;
			string piece = "";
			for (int i = 0; i < command.Length; i++)
			{
				if (command[i] == '\\')
				{
					if (escape == 0)
					{
						escape = 2;
					}
					else
					{
						piece += "\\";
					}
				}
				else if (command[i] == '"')
				{
					if (escape == 0)
					{
						inQuotes = !inQuotes;
					}
					else
					{
						piece += "\"";
					}
				}
				else if (command[i] == ' ' && !inQuotes)
				{
					if (!string.IsNullOrWhiteSpace(piece))
					{
						commands.Add(piece);
					}
					piece = "";
				}
				else if (escape == 0)
				{
					ReadOnlySpan<char> str = piece;
					char c = command[i];
					piece = str + new ReadOnlySpan<char>(ref c);
				}
				if (escape > 0)
				{
					escape--;
				}
			}
			if (!string.IsNullOrWhiteSpace(piece))
			{
				commands.Add(piece);
			}
			return commands.ToArray();
		}

		// Token: 0x06002A79 RID: 10873 RVA: 0x001D53D4 File Offset: 0x001D35D4
		public static string CleanUpPathCrossPlatform(this string path, bool correctFilenameCase = true, string directory = "")
		{
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}
			path = path.Replace('\\', '/');
			if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring("file:".Length);
			}
			while (path.IndexOf("//") >= 0)
			{
				path = path.Replace("//", "/");
			}
			if (correctFilenameCase)
			{
				bool flag;
				string correctedPath = ToolBox.CorrectFilenameCase(path, out flag, directory);
				if (!string.IsNullOrEmpty(correctedPath))
				{
					path = correctedPath;
				}
			}
			return path;
		}

		// Token: 0x06002A7A RID: 10874 RVA: 0x001D5456 File Offset: 0x001D3656
		public static string CleanUpPath(this string path)
		{
			return path.CleanUpPathCrossPlatform(false, "");
		}

		// Token: 0x06002A7B RID: 10875 RVA: 0x001D5464 File Offset: 0x001D3664
		public static float GetEasing(TransitionMode easing, float t)
		{
			float result;
			switch (easing)
			{
			case TransitionMode.Linear:
				result = t;
				break;
			case TransitionMode.Smooth:
				result = MathUtils.SmoothStep(t);
				break;
			case TransitionMode.Smoother:
				result = MathUtils.SmootherStep(t);
				break;
			case TransitionMode.EaseIn:
				result = MathUtils.EaseIn(t);
				break;
			case TransitionMode.EaseOut:
				result = MathUtils.EaseOut(t);
				break;
			case TransitionMode.Exponential:
				result = t * t;
				break;
			default:
				result = t;
				break;
			}
			return result;
		}

		// Token: 0x06002A7C RID: 10876 RVA: 0x001D54C4 File Offset: 0x001D36C4
		public static Rectangle GetWorldBounds(Point center, Point size)
		{
			Point halfSize = size.Divide(2);
			Point topLeft = new Point(center.X - halfSize.X, center.Y + halfSize.Y);
			return new Rectangle(topLeft, size);
		}

		// Token: 0x06002A7D RID: 10877 RVA: 0x001D5501 File Offset: 0x001D3701
		public static void ThrowIfNull<T>([NotNull] T o)
		{
			if (o == null)
			{
				throw new ArgumentNullException();
			}
		}

		// Token: 0x06002A7E RID: 10878 RVA: 0x001D5514 File Offset: 0x001D3714
		public static string GetFormattedPercentage(float v)
		{
			return TextManager.GetWithVariable("percentageformat", "[value]", ((int)MathF.Round(v * 100f)).ToString(), FormatCapitals.No).Value;
		}

		// Token: 0x06002A7F RID: 10879 RVA: 0x001D5550 File Offset: 0x001D3750
		public static string ExtendColorToPercentageSigns(string original)
		{
			char[] chars = original.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				char currentChar;
				if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(i, chars, out currentChar) && ToolBox.affectedCharacters.Contains(currentChar))
				{
					char c;
					if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(i - 1, chars, out c) && c == '‖')
					{
						int offset = "‖color:end‖".Length;
						if (ToolBox.<ExtendColorToPercentageSigns>g__MatchesSequence|63_1(i - offset, "‖color:end‖", chars))
						{
							char prev = currentChar;
							for (int j = i - offset; j <= i; j++)
							{
								if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(j, chars, out c))
								{
									chars[j] = prev;
									prev = c;
								}
							}
							goto IL_FA;
						}
					}
					if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(i + 1, chars, out c) && c == '‖' && ToolBox.<ExtendColorToPercentageSigns>g__MatchesSequence|63_1(i + 1, "‖color:", chars))
					{
						int offset2 = ToolBox.<ExtendColorToPercentageSigns>g__FindNextDefinitionOffset|63_0(i, "‖color:".Length, chars);
						if (offset2 <= chars.Length)
						{
							char prev2 = currentChar;
							for (int k = i + offset2; k >= i; k--)
							{
								if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(k, chars, out c))
								{
									chars[k] = prev2;
									prev2 = c;
								}
							}
							i += offset2;
						}
					}
				}
				IL_FA:;
			}
			return new string(chars);
		}

		// Token: 0x06002A80 RID: 10880 RVA: 0x001D566A File Offset: 0x001D386A
		public static bool StatIdentifierMatches(Identifier original, Identifier match)
		{
			return original == match || ToolBox.<StatIdentifierMatches>g__Matches|64_0(original, match) || ToolBox.<StatIdentifierMatches>g__Matches|64_0(match, original);
		}

		// Token: 0x06002A81 RID: 10881 RVA: 0x001D568B File Offset: 0x001D388B
		public static bool EquivalentTo(this IPEndPoint self, IPEndPoint other)
		{
			return self.Address.EquivalentTo(other.Address) && self.Port == other.Port;
		}

		// Token: 0x06002A82 RID: 10882 RVA: 0x001D56B0 File Offset: 0x001D38B0
		public static bool EquivalentTo(this IPAddress self, IPAddress other)
		{
			if (self.IsIPv4MappedToIPv6)
			{
				self = self.MapToIPv4();
			}
			if (other.IsIPv4MappedToIPv6)
			{
				other = other.MapToIPv4();
			}
			return self.Equals(other);
		}

		// Token: 0x06002A83 RID: 10883 RVA: 0x001D56D9 File Offset: 0x001D38D9
		public static float ShortAudioSampleToFloat(short value)
		{
			return (float)value / 32767f;
		}

		// Token: 0x06002A84 RID: 10884 RVA: 0x001D56E4 File Offset: 0x001D38E4
		public static short FloatToShortAudioSample(float value)
		{
			int temp = (int)(32767f * value);
			if (temp > 32767)
			{
				temp = 32767;
			}
			else if (temp < -32768)
			{
				temp = -32768;
			}
			return (short)temp;
		}

		// Token: 0x06002A85 RID: 10885 RVA: 0x001D571C File Offset: 0x001D391C
		public static SquareLine GetSquareLineBetweenPoints(Vector2 start, Vector2 end, float knobLength = 24f)
		{
			Vector2[] points = new Vector2[6];
			Vector2[] array = points;
			int num = 0;
			Vector2[] array2 = points;
			int num2 = 1;
			points[2] = start;
			array[num] = (array2[num2] = start);
			Vector2[] array3 = points;
			int num3 = 5;
			Vector2[] array4 = points;
			int num4 = 4;
			points[3] = end;
			array3[num3] = (array4[num4] = end);
			Vector2[] array5 = points;
			int num5 = 2;
			array5[num5].X = array5[num5].X + (points[3].X - points[2].X) / 2f;
			points[2].X = Math.Max(points[2].X, points[0].X + knobLength);
			points[3].X = points[2].X;
			bool isBehind = false;
			if (points[2].X <= points[0].X + knobLength)
			{
				isBehind = true;
				Vector2[] array6 = points;
				int num6 = 1;
				array6[num6].X = array6[num6].X + knobLength;
				points[2].X = points[2].X;
				Vector2[] array7 = points;
				int num7 = 2;
				array7[num7].Y = array7[num7].Y + (points[4].Y - points[1].Y) / 2f;
			}
			if (points[3].X >= points[5].X - knobLength)
			{
				isBehind = true;
				Vector2[] array8 = points;
				int num8 = 4;
				array8[num8].X = array8[num8].X - knobLength;
				points[3].X = points[4].X;
				Vector2[] array9 = points;
				int num9 = 3;
				array9[num9].Y = array9[num9].Y - (points[3].Y - points[2].Y);
			}
			SquareLine.LineType type = isBehind ? SquareLine.LineType.SixPointBackwardsLine : SquareLine.LineType.FourPointForwardsLine;
			return new SquareLine(points, type);
		}

		// Token: 0x06002A86 RID: 10886 RVA: 0x001D58D4 File Offset: 0x001D3AD4
		public static string BytesToHexString(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("X2"));
			}
			return sb.ToString();
		}

		// Token: 0x06002A87 RID: 10887 RVA: 0x001D5914 File Offset: 0x001D3B14
		public static Vector2 GetClosestPointOnRectangle(RectangleF rect, Vector2 point)
		{
			Vector2 closest = new Vector2(MathHelper.Clamp(point.X, rect.Left, rect.Right), MathHelper.Clamp(point.Y, rect.Top, rect.Bottom));
			if (point.X < rect.Left)
			{
				closest.X = rect.Left;
			}
			else if (point.X > rect.Right)
			{
				closest.X = rect.Right;
			}
			if (point.Y < rect.Top)
			{
				closest.Y = rect.Top;
			}
			else if (point.Y > rect.Bottom)
			{
				closest.Y = rect.Bottom;
			}
			return closest;
		}

		// Token: 0x06002A88 RID: 10888 RVA: 0x001D59D3 File Offset: 0x001D3BD3
		public static ImmutableArray<uint> PrefabCollectionToUintIdentifierArray(IEnumerable<PrefabWithUintIdentifier> prefabs)
		{
			return (from p in prefabs
			select p.UintIdentifier).ToImmutableArray<uint>();
		}

		// Token: 0x06002A89 RID: 10889 RVA: 0x001D5A00 File Offset: 0x001D3C00
		public static ImmutableArray<T> UintIdentifierArrayToPrefabCollection<T>(PrefabCollection<T> Prefabs, IEnumerable<uint> uintIdentifiers) where T : PrefabWithUintIdentifier
		{
			ImmutableArray<T>.Builder builder = ImmutableArray.CreateBuilder<T>();
			using (IEnumerator<uint> enumerator = uintIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					uint uintIdentifier = enumerator.Current;
					T matchingPrefab = Prefabs.Find((T p) => p.UintIdentifier == uintIdentifier);
					if (matchingPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unable to find prefab with uint identifier ");
						defaultInterpolatedStringHandler.AppendFormatted<uint>(uintIdentifier);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						builder.Add(matchingPrefab);
					}
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x06002A8B RID: 10891 RVA: 0x001D5AF0 File Offset: 0x001D3CF0
		[NullableContext(1)]
		[CompilerGenerated]
		internal static IEnumerable<Vector2> <CombineRectanglesIntoShape>g__RectangleToPoints|6_7(RectangleF rect)
		{
			float left = rect.Left;
			float top = rect.Top;
			float right = rect.Right;
			float y2 = rect.Bottom;
			float x2 = right;
			float y3 = top;
			float x3 = left;
			return new Vector2[]
			{
				new Vector2(x3, y3),
				new Vector2(x2, y3),
				new Vector2(x2, y2),
				new Vector2(x3, y2)
			};
		}

		// Token: 0x06002A8C RID: 10892 RVA: 0x001D5B60 File Offset: 0x001D3D60
		[NullableContext(1)]
		[CompilerGenerated]
		internal static string <ColorSectionOfString>g__ColorString|23_0(string text, Color color)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(color.ToStringHex());
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted(text);
			defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06002A8D RID: 10893 RVA: 0x001D5BB8 File Offset: 0x001D3DB8
		[CompilerGenerated]
		internal static bool <HexStringToBytes>g__IsHexChar|24_0(char c)
		{
			if (c >= 'A')
			{
				if (c >= 'a')
				{
					if (c > 'f')
					{
						goto IL_26;
					}
				}
				else if (c > 'F')
				{
					goto IL_26;
				}
			}
			else if (c < '0' || c > '9')
			{
				goto IL_26;
			}
			return true;
			IL_26:
			return false;
		}

		// Token: 0x06002A8E RID: 10894 RVA: 0x001D5BF0 File Offset: 0x001D3DF0
		[CompilerGenerated]
		internal static int <ExtendColorToPercentageSigns>g__FindNextDefinitionOffset|63_0(int index, int initialOffset, char[] chars)
		{
			int offset = initialOffset;
			char c;
			while (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(index + offset, chars, out c) && c != '‖')
			{
				offset++;
			}
			return offset;
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x001D5C1C File Offset: 0x001D3E1C
		[CompilerGenerated]
		internal static bool <ExtendColorToPercentageSigns>g__MatchesSequence|63_1(int index, string sequence, char[] chars)
		{
			for (int i = 0; i < sequence.Length; i++)
			{
				char c;
				if (!ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|63_2(index + i, chars, out c) || c != sequence[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x001D5C54 File Offset: 0x001D3E54
		[CompilerGenerated]
		internal static bool <ExtendColorToPercentageSigns>g__TryGetAt|63_2(int i, char[] chars, out char c)
		{
			if (i >= 0 && i < chars.Length)
			{
				c = chars[i];
				return true;
			}
			c = '\0';
			return false;
		}

		// Token: 0x06002A91 RID: 10897 RVA: 0x001D5C6C File Offset: 0x001D3E6C
		[CompilerGenerated]
		internal static bool <StatIdentifierMatches>g__Matches|64_0(Identifier a, Identifier b)
		{
			for (int i = 0; i < b.Value.Length; i++)
			{
				if (i >= a.Value.Length)
				{
					return b[i] == '~';
				}
				if (!ToolBox.<StatIdentifierMatches>g__CharEquals|64_1(a[i], b[i]))
				{
					return false;
				}
			}
			return false;
		}

		// Token: 0x06002A92 RID: 10898 RVA: 0x001D5CC6 File Offset: 0x001D3EC6
		[CompilerGenerated]
		internal static bool <StatIdentifierMatches>g__CharEquals|64_1(char a, char b)
		{
			return char.ToLowerInvariant(a) == char.ToLowerInvariant(b);
		}

		// Token: 0x0400161C RID: 5660
		private static readonly ConcurrentDictionary<string, string> cachedFileNames = new ConcurrentDictionary<string, string>();

		// Token: 0x0400161D RID: 5661
		private static readonly Regex removeBBCodeRegex = new Regex("\\[\\/?(?:b|i|u|url|quote|code|img|color|size)*?.*?\\]");

		// Token: 0x0400161E RID: 5662
		private static Dictionary<string, List<string>> cachedLines = new Dictionary<string, List<string>>();

		// Token: 0x0400161F RID: 5663
		private static readonly ImmutableHashSet<char> affectedCharacters = ImmutableHashSet.Create<char>(new char[]
		{
			'%',
			'+',
			'％'
		});

		// Token: 0x02000DD2 RID: 3538
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040050AD RID: 20653
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Func<RectangleF, IEnumerable<Vector2>> <0>__RectangleToPoints;

			// Token: 0x040050AE RID: 20654
			public static Func<string, string> <1>__GetFileName;
		}
	}
}
