using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.PerkBehaviors;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000EC RID: 236
	internal class Submarine : Entity, IServerPositionSync, IServerSerializable, INetSerializable
	{
		// Token: 0x06002154 RID: 8532 RVA: 0x0014DAA4 File Offset: 0x0014BCA4
		public static void CullEntities(Camera cam)
		{
			Rectangle camView = cam.WorldView;
			camView = new Rectangle(camView.X - 50, camView.Y + 50, camView.Width + 100, camView.Height + 100);
			Level level = Level.Loaded;
			float? num;
			if (level == null)
			{
				num = null;
			}
			else
			{
				LevelRenderer renderer = level.Renderer;
				num = ((renderer != null) ? new float?(renderer.CollapseEffectStrength) : null);
			}
			float? num2 = num;
			if (num2 != null && num2.GetValueOrDefault() > 0f)
			{
				camView = Rectangle.Union(Submarine.AbsRect(camView.Location.ToVector2(), camView.Size.ToVector2()), new Rectangle(Point.Zero, Level.Loaded.Size));
				camView.Y += camView.Height;
			}
			if (Math.Abs(camView.X - Submarine.prevCullArea.X) < 50 && Math.Abs(camView.Y - Submarine.prevCullArea.Y) < 50 && Math.Abs(camView.Right - Submarine.prevCullArea.Right) < 50 && Math.Abs(camView.Bottom - Submarine.prevCullArea.Bottom) < 50 && Submarine.prevCullTime > Timing.TotalTime - 0.25)
			{
				return;
			}
			Submarine.visibleSubs.Clear();
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (Level.Loaded == null || sub.WorldPosition.Y >= -1000000f)
				{
					Rectangle worldBorders = new Rectangle(sub.VisibleBorders.X + (int)sub.WorldPosition.X, sub.VisibleBorders.Y + (int)sub.WorldPosition.Y, sub.VisibleBorders.Width, sub.VisibleBorders.Height);
					if (Submarine.RectsOverlap(worldBorders, camView, true))
					{
						Submarine.visibleSubs.Add(sub);
					}
				}
			}
			if (Submarine.visibleEntities == null)
			{
				Submarine.visibleEntities = new List<MapEntity>(MapEntity.MapEntityList.Count);
			}
			else
			{
				Submarine.visibleEntities.Clear();
			}
			foreach (MapEntity entity in MapEntity.MapEntityList)
			{
				if (entity != null && !entity.Removed && (entity.Submarine == null || Submarine.visibleSubs.Contains(entity.Submarine)) && entity.IsVisible(camView))
				{
					Submarine.visibleEntities.Add(entity);
				}
			}
			Submarine.prevCullArea = camView;
			Submarine.prevCullTime = Timing.TotalTime;
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x0014DD84 File Offset: 0x0014BF84
		public static void ForceVisibilityRecheck()
		{
			Submarine.prevCullTime = 0.0;
		}

		// Token: 0x06002156 RID: 8534 RVA: 0x0014DD94 File Offset: 0x0014BF94
		public static void ForceRemoveFromVisibleEntities(MapEntity entity)
		{
			List<MapEntity> list = Submarine.visibleEntities;
			if (list == null)
			{
				return;
			}
			list.Remove(entity);
		}

		// Token: 0x06002157 RID: 8535 RVA: 0x0014DDA8 File Offset: 0x0014BFA8
		public static void Draw(SpriteBatch spriteBatch, bool editing = false)
		{
			List<MapEntity> entitiesToRender = (!editing && Submarine.visibleEntities != null) ? Submarine.visibleEntities : MapEntity.MapEntityList;
			foreach (MapEntity e in entitiesToRender)
			{
				e.Draw(spriteBatch, editing, true);
			}
		}

		// Token: 0x06002158 RID: 8536 RVA: 0x0014DE10 File Offset: 0x0014C010
		public static void DrawFront(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
		{
			List<MapEntity> entitiesToRender = (!editing && Submarine.visibleEntities != null) ? Submarine.visibleEntities : MapEntity.MapEntityList;
			foreach (MapEntity e in entitiesToRender)
			{
				if (e.DrawOverWater && (predicate == null || predicate(e)))
				{
					e.Draw(spriteBatch, editing, false);
				}
			}
			if (GameMain.DebugDraw)
			{
				foreach (Submarine sub in Submarine.Loaded)
				{
					Rectangle worldBorders = sub.Borders;
					worldBorders.Location += (sub.DrawPosition + sub.HiddenSubPosition).ToPoint();
					worldBorders.Y = -worldBorders.Y;
					GUI.DrawRectangle(spriteBatch, worldBorders, Color.White, false, 0f, 5f);
					if (sub.SubBody != null && sub.subBody.PositionBuffer.Count >= 2)
					{
						Vector2 prevPos = ConvertUnits.ToDisplayUnits(sub.subBody.PositionBuffer[0].Position);
						prevPos.Y = -prevPos.Y;
						for (int i = 1; i < sub.subBody.PositionBuffer.Count; i++)
						{
							Vector2 currPos = ConvertUnits.ToDisplayUnits(sub.subBody.PositionBuffer[i].Position);
							currPos.Y = -currPos.Y;
							GUI.DrawRectangle(spriteBatch, new Rectangle((int)currPos.X - 10, (int)currPos.Y - 10, 20, 20), Color.Blue * 0.6f, true, 0.01f, 1f);
							GUI.DrawLine(spriteBatch, prevPos, currPos, Color.Cyan * 0.5f, 0f, 5f);
							prevPos = currPos;
						}
					}
				}
			}
		}

		// Token: 0x06002159 RID: 8537 RVA: 0x0014E058 File Offset: 0x0014C258
		public static void DrawDamageable(SpriteBatch spriteBatch, Effect damageEffect, bool editing = false, Predicate<MapEntity> predicate = null)
		{
			List<MapEntity> entitiesToRender = (!editing && Submarine.visibleEntities != null) ? Submarine.visibleEntities : MapEntity.MapEntityList;
			Submarine.depthSortedDamageable.Clear();
			foreach (MapEntity e in entitiesToRender)
			{
				Structure structure = e as Structure;
				if (structure != null && structure.DrawDamageEffect && (predicate == null || predicate(e)))
				{
					float drawDepth = structure.GetDrawDepth();
					int i;
					for (i = 0; i < Submarine.depthSortedDamageable.Count; i++)
					{
						float otherDrawDepth = Submarine.depthSortedDamageable[i].GetDrawDepth();
						if (otherDrawDepth < drawDepth)
						{
							break;
						}
					}
					Submarine.depthSortedDamageable.Insert(i, structure);
				}
			}
			foreach (Structure s in Submarine.depthSortedDamageable)
			{
				s.DrawDamage(spriteBatch, damageEffect, editing);
			}
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x0014E16C File Offset: 0x0014C36C
		public static void DrawPaintedColors(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
		{
			List<MapEntity> entitiesToRender = (!editing && Submarine.visibleEntities != null) ? Submarine.visibleEntities : MapEntity.MapEntityList;
			foreach (MapEntity e in entitiesToRender)
			{
				Hull hull = e as Hull;
				if (hull != null && hull.SupportsPaintedColors && (predicate == null || predicate(e)))
				{
					hull.DrawSectionColors(spriteBatch);
				}
			}
		}

		// Token: 0x0600215B RID: 8539 RVA: 0x0014E1F0 File Offset: 0x0014C3F0
		public static void DrawBack(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
		{
			List<MapEntity> entitiesToRender = (!editing && Submarine.visibleEntities != null) ? Submarine.visibleEntities : MapEntity.MapEntityList;
			foreach (MapEntity e in entitiesToRender)
			{
				if (e.DrawBelowWater && (predicate == null || predicate(e)))
				{
					e.Draw(spriteBatch, editing, true);
				}
			}
		}

		// Token: 0x0600215C RID: 8540 RVA: 0x0014E26C File Offset: 0x0014C46C
		public static void DrawGrid(SpriteBatch spriteBatch, int gridCells, Vector2 gridCenter, Vector2 roundedGridCenter, float alpha = 1f, Color? color = null)
		{
			Vector2 topLeft = roundedGridCenter - Vector2.One * Submarine.GridSize * (float)gridCells / 2f;
			Vector2 bottomRight = roundedGridCenter + Vector2.One * Submarine.GridSize * (float)gridCells / 2f;
			for (int i = 0; i < gridCells; i++)
			{
				float middleIndex = (float)(gridCells - 1) / 2f;
				float normalizedPos = Math.Abs(((float)i - middleIndex) / middleIndex);
				float expandX = MathHelper.Lerp(30f, 0f, normalizedPos);
				float expandY = expandX;
				Color lineColor = color ?? Color.White;
				GUI.DrawLine(spriteBatch, new Vector2(topLeft.X - expandX, -bottomRight.Y + (float)i * Submarine.GridSize.Y), new Vector2(bottomRight.X + expandX, -bottomRight.Y + (float)i * Submarine.GridSize.Y), lineColor * (1f - normalizedPos) * alpha, 0.6f, 3f);
				GUI.DrawLine(spriteBatch, new Vector2(topLeft.X + (float)i * Submarine.GridSize.X, -topLeft.Y + expandY), new Vector2(topLeft.X + (float)i * Submarine.GridSize.X, -bottomRight.Y - expandY), lineColor * (1f - normalizedPos) * alpha, 0.6f, 3f);
			}
		}

		// Token: 0x0600215D RID: 8541 RVA: 0x0014E400 File Offset: 0x0014C600
		[Obsolete("Use MiniMap.CreateMiniMap()")]
		public void CreateMiniMap(GUIComponent parent, IEnumerable<Entity> pointsOfInterest = null, bool ignoreOutpost = false)
		{
			Submarine.<>c__DisplayClass17_0 CS$<>8__locals1 = new Submarine.<>c__DisplayClass17_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.ignoreOutpost = ignoreOutpost;
			CS$<>8__locals1.worldBorders = this.GetDockedBorders(true);
			Submarine.<>c__DisplayClass17_0 CS$<>8__locals2 = CS$<>8__locals1;
			CS$<>8__locals2.worldBorders.Location = CS$<>8__locals2.worldBorders.Location + this.WorldPosition.ToPoint();
			float aspectRatio = (float)CS$<>8__locals1.worldBorders.Width / (float)CS$<>8__locals1.worldBorders.Height;
			float parentAspectRatio = (float)parent.Rect.Width / (float)parent.Rect.Height;
			float scale = 0.9f;
			CS$<>8__locals1.hullContainer = new GUIFrame(new RectTransform(((parentAspectRatio > aspectRatio) ? new Vector2(aspectRatio / parentAspectRatio, 1f) : new Vector2(1f, parentAspectRatio / aspectRatio)) * scale, parent.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				UserData = "hullcontainer"
			};
			CS$<>8__locals1.connectedSubs = this.GetConnectedSubs();
			HashSet<Hull> hullList = (from hull in Hull.HullList
			where hull.Submarine == CS$<>8__locals1.<>4__this || CS$<>8__locals1.connectedSubs.Contains(hull.Submarine)
			where !CS$<>8__locals1.ignoreOutpost || CS$<>8__locals1.<>4__this.IsEntityFoundOnThisSub(hull, true, false, false)
			select hull).ToHashSet<Hull>();
			Dictionary<Hull, HashSet<Hull>> combinedHulls = new Dictionary<Hull, HashSet<Hull>>();
			using (HashSet<Hull>.Enumerator enumerator = hullList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Hull hull = enumerator.Current;
					if (!combinedHulls.ContainsKey(hull) && !combinedHulls.Values.Any((HashSet<Hull> hh) => hh.Contains(hull)))
					{
						List<Hull> linkedHulls3 = new List<Hull>();
						hull.GetLinkedHulls(linkedHulls3, false);
						linkedHulls3.Remove(hull);
						foreach (Hull linkedHull in linkedHulls3)
						{
							if (!combinedHulls.ContainsKey(hull))
							{
								combinedHulls.Add(hull, new HashSet<Hull>());
							}
							combinedHulls[hull].Add(linkedHull);
						}
					}
				}
			}
			using (HashSet<Hull>.Enumerator enumerator3 = hullList.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					Hull hull = enumerator3.Current;
					Vector2 relativeHullPos = new Vector2((float)(hull.WorldRect.X - CS$<>8__locals1.worldBorders.X) / (float)CS$<>8__locals1.worldBorders.Width, (float)(CS$<>8__locals1.worldBorders.Y - hull.WorldRect.Y) / (float)CS$<>8__locals1.worldBorders.Height);
					Vector2 relativeHullSize = new Vector2((float)hull.Rect.Width / (float)CS$<>8__locals1.worldBorders.Width, (float)hull.Rect.Height / (float)CS$<>8__locals1.worldBorders.Height);
					if (!combinedHulls.ContainsKey(hull) && !combinedHulls.Values.Any((HashSet<Hull> hh) => hh.Contains(hull)))
					{
						Color color = Color.DarkCyan * 0.8f;
						GUIFrame hullFrame = new GUIFrame(new RectTransform(relativeHullSize, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
						{
							RelativeOffset = relativeHullPos
						}, "MiniMapRoom", new Color?(color))
						{
							UserData = hull
						};
						new GUIFrame(new RectTransform(Vector2.One, hullFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ScanLines", new Color?(color));
					}
				}
			}
			foreach (KeyValuePair<Hull, HashSet<Hull>> keyValuePair in combinedHulls)
			{
				Hull mainHull2;
				HashSet<Hull> linkedHulls2;
				keyValuePair.Deconstruct(out mainHull2, out linkedHulls2);
				Hull mainHull = mainHull2;
				HashSet<Hull> linkedHulls = linkedHulls2;
				MiniMapHullData data = Submarine.ConstructLinkedHulls(mainHull, linkedHulls, CS$<>8__locals1.hullContainer, CS$<>8__locals1.worldBorders);
				Vector2 relativeHullPos2 = new Vector2((data.Bounds.X - (float)CS$<>8__locals1.worldBorders.X) / (float)CS$<>8__locals1.worldBorders.Width, ((float)CS$<>8__locals1.worldBorders.Y - data.Bounds.Y) / (float)CS$<>8__locals1.worldBorders.Height);
				Vector2 relativeHullSize2 = new Vector2(data.Bounds.Width / (float)CS$<>8__locals1.worldBorders.Width, data.Bounds.Height / (float)CS$<>8__locals1.worldBorders.Height);
				Color color2 = Color.DarkCyan * 0.8f;
				float highestY = 0f;
				float highestX = 0f;
				ValueTuple<RectangleF, Hull>[] rectDatas = data.RectDatas;
				for (int i = 0; i < rectDatas.Length; i++)
				{
					RectangleF r = rectDatas[i].Item1;
					float y = r.Y - -r.Height;
					float x = r.X;
					if (y > highestY)
					{
						highestY = y;
					}
					if (x > highestX)
					{
						highestX = x;
					}
				}
				HashSet<GUIFrame> frames = new HashSet<GUIFrame>();
				foreach (ValueTuple<RectangleF, Hull> valueTuple in data.RectDatas)
				{
					RectangleF snappredRect = valueTuple.Item1;
					Hull hull2 = valueTuple.Item2;
					RectangleF rect = snappredRect;
					rect.Height = -rect.Height;
					rect.Y -= rect.Height;
					float num;
					float num2;
					CS$<>8__locals1.hullContainer.Rect.Size.ToVector2().Deconstruct(out num, out num2);
					float parentW = num;
					float parentH = num2;
					Vector2 size = new Vector2(rect.Width / parentW, rect.Height / parentH);
					Vector2 pos = new Vector2(rect.X / parentW, rect.Y / parentH);
					GUIFrame hullFrame2 = new GUIFrame(new RectTransform(size, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						RelativeOffset = pos
					}, "ScanLinesSeamless", new Color?(color2))
					{
						UserData = hull2,
						UVOffset = new Vector2(highestX - rect.X, highestY - rect.Y)
					};
					frames.Add(hullFrame2);
				}
				GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(relativeHullSize2, CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = relativeHullPos2
				}, delegate(SpriteBatch spriteBatch, GUICustomComponent component)
				{
					foreach (List<Vector2> list in data.Polygon)
					{
						spriteBatch.DrawPolygonInner(CS$<>8__locals1.hullContainer.Rect.Location.ToVector2(), list, component.Color, 2f);
					}
				}, delegate(float deltaTime, GUICustomComponent component)
				{
					if (component.Parent.Rect.Size != data.ParentSize)
					{
						data = Submarine.ConstructLinkedHulls(mainHull, linkedHulls, CS$<>8__locals1.hullContainer, CS$<>8__locals1.worldBorders);
					}
				});
				guicustomComponent.UserData = frames;
				guicustomComponent.Color = color2;
				guicustomComponent.CanBeFocused = false;
			}
			if (pointsOfInterest != null)
			{
				foreach (Entity entity in pointsOfInterest)
				{
					Vector2 relativePos = new Vector2((entity.WorldPosition.X - (float)CS$<>8__locals1.worldBorders.X) / (float)CS$<>8__locals1.worldBorders.Width, ((float)CS$<>8__locals1.worldBorders.Y - entity.WorldPosition.Y) / (float)CS$<>8__locals1.worldBorders.Height);
					GUIFrame guiframe = new GUIFrame(new RectTransform(new Point(1, 1), CS$<>8__locals1.hullContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
					{
						RelativeOffset = relativePos
					}, null, null);
					guiframe.CanBeFocused = false;
					guiframe.UserData = entity;
				}
			}
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x0014ECF4 File Offset: 0x0014CEF4
		public static MiniMapHullData ConstructLinkedHulls(Hull mainHull, HashSet<Hull> linkedHulls, GUIComponent parent, Rectangle worldBorders)
		{
			Rectangle parentRect = parent.Rect;
			Dictionary<Hull, Rectangle> rects = new Dictionary<Hull, Rectangle>();
			Rectangle worldRect = mainHull.WorldRect;
			worldRect.Y = -worldRect.Y;
			rects.Add(mainHull, worldRect);
			foreach (Hull hull in linkedHulls)
			{
				Rectangle rect = hull.WorldRect;
				rect.Y = -rect.Y;
				worldRect = Rectangle.Union(worldRect, rect);
				rects.Add(hull, rect);
			}
			worldRect.Y = -worldRect.Y;
			List<RectangleF> normalizedRects = new List<RectangleF>();
			List<Hull> hullRefs = new List<Hull>();
			foreach (KeyValuePair<Hull, Rectangle> keyValuePair in rects)
			{
				Hull hull3;
				Rectangle rectangle;
				keyValuePair.Deconstruct(out hull3, out rectangle);
				Hull hull2 = hull3;
				Rectangle rect2 = rectangle;
				Rectangle wRect = rect2;
				wRect.Y = -wRect.Y;
				float num;
				float num2;
				new Vector2((float)(wRect.X - worldBorders.X) / (float)worldBorders.Width, (float)(worldBorders.Y - wRect.Y) / (float)worldBorders.Height).Deconstruct(out num, out num2);
				float posX = num;
				float posY = num2;
				new Vector2((float)wRect.Width / (float)worldBorders.Width, (float)wRect.Height / (float)worldBorders.Height).Deconstruct(out num2, out num);
				float scaleX = num2;
				float scaleY = num;
				RectangleF newRect = new RectangleF(posX * (float)parentRect.Width, posY * (float)parentRect.Height, scaleX * (float)parentRect.Width, scaleY * (float)parentRect.Height);
				normalizedRects.Add(newRect);
				hullRefs.Add(hull2);
			}
			ImmutableArray<RectangleF> snappedRectangles = ToolBox.SnapRectangles(normalizedRects, 1);
			List<List<Vector2>> polygon = ToolBox.CombineRectanglesIntoShape(snappedRectangles);
			List<List<Vector2>> scaledPolygon = new List<List<Vector2>>();
			foreach (List<Vector2> list in polygon)
			{
				float num;
				float num2;
				ToolBox.GetPolygonBoundingBoxSize(list).Deconstruct(out num, out num2);
				float polySizeX = num;
				float polySizeY = num2;
				float sizeX = polySizeX - 1f;
				float sizeY = polySizeY - 1f;
				scaledPolygon.Add(ToolBox.ScalePolygon(list, new Vector2(sizeX / polySizeX, sizeY / polySizeY)));
			}
			return new MiniMapHullData(scaledPolygon, worldRect, parentRect.Size, snappedRectangles, hullRefs.ToImmutableArray<Hull>());
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x0014EFB8 File Offset: 0x0014D1B8
		public void CheckForErrors()
		{
			List<string> errorMsgs = new List<string>();
			List<SubEditorScreen.WarningType> warnings = new List<SubEditorScreen.WarningType>();
			if (!Hull.HullList.Any<Hull>() && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoWaypoints))
			{
				errorMsgs.Add(TextManager.Get("NoHullsWarning").Value);
				warnings.Add(SubEditorScreen.WarningType.NoHulls);
			}
			if (this.Info.Type == SubmarineType.OutpostModule)
			{
				OutpostModuleInfo outpostModuleInfo = this.Info.OutpostModuleInfo;
				bool flag;
				if (outpostModuleInfo == null)
				{
					flag = true;
				}
				else
				{
					flag = outpostModuleInfo.ModuleFlags.Any((Identifier f) => f != "hallwayvertical" && f != "hallwayhorizontal");
				}
				if (!flag)
				{
					goto IL_EB;
				}
			}
			if (!WayPoint.WayPointList.Any((WayPoint wp) => wp.ShouldBeSaved && wp.SpawnType == SpawnType.Path) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoWaypoints))
			{
				errorMsgs.Add(TextManager.Get("NoWaypointsWarning").Value);
				warnings.Add(SubEditorScreen.WarningType.NoWaypoints);
			}
			IL_EB:
			if (Hull.HullList.Any((Hull h) => h.WaterVolume > 0f))
			{
				errorMsgs.Add(TextManager.Get("WaterInHullsWarning").Value);
				warnings.Add(SubEditorScreen.WarningType.WaterInHulls);
				Hull.ShowHulls = true;
			}
			if (this.Info.IsWreck)
			{
				Point vanillaBrainSize = new Point(204, 204);
				if (WreckAI.GetPotentialBrainRooms(this, WreckAIConfig.GetRandom(), vanillaBrainSize, null).None(null))
				{
					errorMsgs.Add(TextManager.Get("NoSuitableBrainRoomsWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.NoSuitableBrainRooms);
				}
			}
			if (!Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NotEnoughContainers))
			{
				HashSet<ContainerTagPrefab> missingContainerTags = new HashSet<ContainerTagPrefab>();
				using (IEnumerator<ContainerTagPrefab> enumerator = ContainerTagPrefab.Prefabs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContainerTagPrefab prefab = enumerator.Current;
						if (prefab.IsRecommendedForSub(this) && prefab.WarnIfLess)
						{
							int count = Item.ItemList.Count((Item i) => i.HasTag(prefab.Identifier));
							if (count < prefab.RecommendedAmount)
							{
								missingContainerTags.Add(prefab);
							}
						}
					}
				}
				if (missingContainerTags.Any<ContainerTagPrefab>())
				{
					StringBuilder sb = new StringBuilder();
					int count2 = 0;
					foreach (ContainerTagPrefab tag in missingContainerTags)
					{
						StringBuilder stringBuilder = sb;
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder);
						appendInterpolatedStringHandler.AppendLiteral("- ");
						appendInterpolatedStringHandler.AppendFormatted<LocalizedString>(tag.Name);
						stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
						count2++;
						if (missingContainerTags.Count > count2 && count2 >= 3)
						{
							string moreIndicator = TextManager.GetWithVariable("upgradeuitooltip.moreindicator", "[amount]", (missingContainerTags.Count - count2).ToString(), FormatCapitals.No).Value;
							sb.AppendLine(moreIndicator);
							break;
						}
					}
					errorMsgs.Add(TextManager.GetWithVariable("ContainerTagUI.CountWarning", "[tags]", sb.ToString(), FormatCapitals.No).Value);
					warnings.Add(SubEditorScreen.WarningType.NotEnoughContainers);
				}
			}
			if (this.Info.Type == SubmarineType.Player)
			{
				foreach (Item item4 in Item.ItemList)
				{
					if (item4.GetComponent<Vent>() != null && !item4.linkedTo.Any<MapEntity>())
					{
						if (!Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.DisconnectedVents))
						{
							errorMsgs.Add(TextManager.Get("DisconnectedVentsWarning").Value);
							warnings.Add(SubEditorScreen.WarningType.DisconnectedVents);
							break;
						}
						break;
					}
				}
				foreach (Item item2 in Item.ItemList)
				{
					OxygenGenerator oxygenGenerator = item2.GetComponent<OxygenGenerator>();
					if (oxygenGenerator != null)
					{
						oxygenGenerator.GetVents();
						Dictionary<Hull, float> hullOxygenFlow = new Dictionary<Hull, float>();
						foreach (MapEntity linkedTo in item2.linkedTo)
						{
							Item linkedItem = linkedTo as Item;
							if (linkedItem != null)
							{
								Vent vent = linkedItem.GetComponent<Vent>();
								if (vent != null)
								{
									if (vent.Item.CurrentHull == null)
									{
										vent.Item.FindHull();
										if (vent.Item.CurrentHull == null)
										{
											continue;
										}
									}
									float oxygenFlow = oxygenGenerator.GetVentOxygenFlow(vent);
									if (!hullOxygenFlow.ContainsKey(vent.Item.CurrentHull))
									{
										hullOxygenFlow[vent.Item.CurrentHull] = oxygenFlow;
									}
									else
									{
										Dictionary<Hull, float> dictionary = hullOxygenFlow;
										Hull currentHull = vent.Item.CurrentHull;
										dictionary[currentHull] += oxygenFlow;
									}
								}
							}
						}
						foreach (KeyValuePair<Hull, float> keyValuePair in hullOxygenFlow)
						{
							Hull currentHull;
							float num;
							keyValuePair.Deconstruct(out currentHull, out num);
							Hull hull = currentHull;
							float oxygenFlow2 = num;
							if (oxygenFlow2 < 700f)
							{
								errorMsgs.Add(TextManager.GetWithVariable("LowOxygenOutputWarning", "[roomname]", hull.DisplayName, FormatCapitals.No).Value);
								warnings.Add(SubEditorScreen.WarningType.LowOxygenOutputWarning);
							}
						}
					}
				}
				if (!WayPoint.WayPointList.Any((WayPoint wp) => wp.ShouldBeSaved && wp.SpawnType == SpawnType.Human) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoHumanSpawnpoints))
				{
					errorMsgs.Add(TextManager.Get("NoHumanSpawnpointWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.NoHumanSpawnpoints);
				}
				if (WayPoint.WayPointList.Find((WayPoint wp) => wp.SpawnType == SpawnType.Cargo) == null && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoCargoSpawnpoints))
				{
					errorMsgs.Add(TextManager.Get("NoCargoSpawnpointWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.NoCargoSpawnpoints);
				}
				if (Item.ItemList.None((Item it) => it.GetComponent<Pump>() != null && it.HasTag(Tags.Ballast)) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoBallastTag))
				{
					errorMsgs.Add(TextManager.Get("NoBallastTagsWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.NoBallastTag);
				}
				if (Item.ItemList.None((Item it) => it.HasTag(Tags.HiddenItemContainer)) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NoHiddenContainers))
				{
					errorMsgs.Add(TextManager.Get("NoHiddenContainersWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.NoHiddenContainers);
				}
				if ((this.Info.Dimensions.X * Physics.DisplayToRealWorldRatio > 80f || this.Info.Dimensions.Y * Physics.DisplayToRealWorldRatio > 32f) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.TooLargeForEndGame))
				{
					errorMsgs.Add(TextManager.Get("TooLargeForEndGameWarning").Value);
					warnings.Add(SubEditorScreen.WarningType.TooLargeForEndGame);
				}
			}
			else if (this.Info.Type == SubmarineType.OutpostModule)
			{
				using (List<Item>.Enumerator enumerator7 = Item.ItemList.GetEnumerator())
				{
					while (enumerator7.MoveNext())
					{
						Item item = enumerator7.Current;
						PowerTransfer junctionBox = item.GetComponent<PowerTransfer>();
						if (junctionBox != null)
						{
							int doorLinks = item.linkedTo.Count(delegate(MapEntity lt)
							{
								if (!(lt is Gap))
								{
									Item it2 = lt as Item;
									return it2 != null && it2.GetComponent<Door>() != null;
								}
								return true;
							}) + Item.ItemList.Count((Item it2) => it2.linkedTo.Contains(item) && !item.linkedTo.Contains(it2));
							for (int j = 0; j < item.Connections.Count; j++)
							{
								int wireCount = item.Connections[j].Wires.Count;
								if (doorLinks + wireCount > item.Connections[j].MaxWires)
								{
									errorMsgs.Add(TextManager.GetWithVariables("InsufficientFreeConnectionsWarning", new ValueTuple<string, string>[]
									{
										new ValueTuple<string, string>("[doorcount]", doorLinks.ToString()),
										new ValueTuple<string, string>("[freeconnectioncount]", (item.Connections[j].MaxWires - wireCount).ToString())
									}).Value);
									warnings.Add(SubEditorScreen.WarningType.InsufficientFreeConnectionsWarning);
									break;
								}
							}
						}
					}
				}
			}
			if (Gap.GapList.Any((Gap g) => g.linkedTo.Count == 0) && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.NonLinkedGaps))
			{
				errorMsgs.Add(TextManager.Get("NonLinkedGapsWarning").Value);
				warnings.Add(SubEditorScreen.WarningType.NonLinkedGaps);
			}
			float entityCountWarningThreshold = 0.75f;
			if ((float)Item.ItemList.Count > 5000f * entityCountWarningThreshold && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.ItemCount))
			{
				errorMsgs.Add(TextManager.Get("subeditor.itemcountwarning").Value);
				warnings.Add(SubEditorScreen.WarningType.ItemCount);
			}
			if ((float)(MapEntity.MapEntityList.Count - Item.ItemList.Count - Hull.HullList.Count - WayPoint.WayPointList.Count - Gap.GapList.Count) > 2000f * entityCountWarningThreshold && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.StructureCount))
			{
				errorMsgs.Add(TextManager.Get("subeditor.structurecountwarning").Value);
				warnings.Add(SubEditorScreen.WarningType.StructureCount);
			}
			if ((float)Structure.WallList.Count > 2000f * entityCountWarningThreshold && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.WallCount))
			{
				errorMsgs.Add(TextManager.Get("subeditor.wallcountwarning").Value);
				warnings.Add(SubEditorScreen.WarningType.WallCount);
			}
			if ((float)Submarine.GetLightCount() > 600f * entityCountWarningThreshold && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.LightCount))
			{
				errorMsgs.Add(TextManager.Get("subeditor.lightcountwarning").Value);
				warnings.Add(SubEditorScreen.WarningType.LightCount);
			}
			if ((float)Submarine.GetShadowCastingLightCount() > 100f * entityCountWarningThreshold && !Submarine.<CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType.ShadowCastingLightCount))
			{
				errorMsgs.Add(TextManager.Get("subeditor.shadowcastinglightswarning").Value);
				warnings.Add(SubEditorScreen.WarningType.ShadowCastingLightCount);
			}
			if (errorMsgs.Any<string>())
			{
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("Warning"), string.Empty, new Vector2?(new Vector2(0.25f, 0f)), new Point?(new Point(GUI.IntScale(650f), GUI.IntScale(650f))), GUIMessageBox.Type.Default);
				if (warnings.Any<SubEditorScreen.WarningType>())
				{
					GUIListBox textListBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.75f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
					GUITextBlock text = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), textListBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Join("\n\n", errorMsgs), null, null, Alignment.Left, true, "", null)
					{
						CanBeFocused = false
					};
					text.RectTransform.MinSize = new Point(0, (int)text.TextSize.Y);
					Point size = msgBox.RectTransform.NonScaledSize;
					GUITickBox suppress = new GUITickBox(new RectTransform(new Vector2(1f, 0.33f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.suppresswarnings"), null, "");
					msgBox.RectTransform.NonScaledSize = new Point(size.X, size.Y + suppress.RectTransform.NonScaledSize.Y);
					GUIButton guibutton = msgBox.Buttons[0];
					guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object obj)
					{
						if (suppress.Selected)
						{
							foreach (SubEditorScreen.WarningType warning2 in from warning in warnings
							where !SubEditorScreen.SuppressedWarnings.Contains(warning)
							select warning)
							{
								SubEditorScreen.SuppressedWarnings.Add(warning2);
							}
						}
						return true;
					}));
				}
			}
			foreach (MapEntity e2 in MapEntity.MapEntityList)
			{
				if (Vector2.Distance(e2.Position, this.HiddenSubPosition) > 20000f)
				{
					Item item3 = e2 as Item;
					if (item3 != null && item3.body != null && !item3.body.Enabled)
					{
						item3.SetTransform(ConvertUnits.ToSimUnits(this.HiddenSubPosition), 0f, true, true, null);
					}
				}
			}
			using (List<MapEntity>.Enumerator enumerator9 = MapEntity.MapEntityList.GetEnumerator())
			{
				while (enumerator9.MoveNext())
				{
					MapEntity e = enumerator9.Current;
					if (Vector2.Distance(e.Position, this.HiddenSubPosition) > 20000f)
					{
						GUIMessageBox msgBox2 = new GUIMessageBox(TextManager.Get("Warning"), TextManager.Get("FarAwayEntitiesWarning"), new LocalizedString[]
						{
							TextManager.Get("Yes"),
							TextManager.Get("No")
						}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
						GUIButton guibutton2 = msgBox2.Buttons[0];
						guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object obj)
						{
							GameMain.SubEditorScreen.Cam.Position = e.WorldPosition;
							return true;
						}));
						GUIButton guibutton3 = msgBox2.Buttons[0];
						guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(msgBox2.Close));
						GUIButton guibutton4 = msgBox2.Buttons[1];
						guibutton4.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton4.OnClicked, new GUIButton.OnClickedHandler(msgBox2.Close));
						break;
					}
				}
			}
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x0014FF14 File Offset: 0x0014E114
		public static int GetLightCount()
		{
			int disabledItemLightCount = 0;
			foreach (Item item in Item.ItemList)
			{
				if (item.ParentInventory != null)
				{
					disabledItemLightCount += item.GetComponents<LightComponent>().Count<LightComponent>();
				}
			}
			return GameMain.LightManager.Lights.Count<LightSource>() - disabledItemLightCount;
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x0014FF88 File Offset: 0x0014E188
		public static int GetShadowCastingLightCount()
		{
			int disabledItemLightCount = 0;
			foreach (Item item in Item.ItemList)
			{
				if (item.ParentInventory != null)
				{
					disabledItemLightCount += item.GetComponents<LightComponent>().Count<LightComponent>();
				}
			}
			return GameMain.LightManager.Lights.Count((LightSource l) => l.CastShadows && !l.IsBackground) - disabledItemLightCount;
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x0015001C File Offset: 0x0014E21C
		public static Vector2 MouseToWorldGrid(Camera cam, Submarine sub, Vector2? mousePos = null, bool round = false)
		{
			Vector2 position = mousePos ?? PlayerInput.MousePosition;
			position = cam.ScreenToWorld(position);
			return Submarine.VectorToWorldGrid(position, sub, round);
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x00150058 File Offset: 0x0014E258
		public void ClientReadPosition(IReadMessage msg, float sendingTime)
		{
			PosInfo posInfo = this.PhysicsBody.ClientRead(msg, sendingTime, this.Info.Name);
			msg.ReadPadBits();
			if (posInfo != null)
			{
				int index = 0;
				while (index < this.subBody.PositionBuffer.Count && sendingTime > this.subBody.PositionBuffer[index].Timestamp)
				{
					index++;
				}
				this.subBody.PositionBuffer.Insert(index, posInfo);
			}
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x001500D0 File Offset: 0x0014E2D0
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			Identifier layerIdentifier = msg.ReadIdentifier();
			bool enabled = msg.ReadBoolean();
			this.SetLayerEnabled(layerIdentifier, enabled, false);
		}

		// Token: 0x17000910 RID: 2320
		// (get) Token: 0x06002165 RID: 8549 RVA: 0x001500F4 File Offset: 0x0014E2F4
		// (set) Token: 0x06002166 RID: 8550 RVA: 0x001500FC File Offset: 0x0014E2FC
		public SubmarineInfo Info { get; private set; }

		// Token: 0x06002167 RID: 8551 RVA: 0x00150108 File Offset: 0x0014E308
		public static ImmutableArray<SubItemSwapPerk> GetSubItemSwapPerksFromTeamPerks(ImmutableArray<DisembarkPerkPrefab> teamPerks)
		{
			ImmutableArray<SubItemSwapPerk>.Builder builder = ImmutableArray.CreateBuilder<SubItemSwapPerk>();
			foreach (DisembarkPerkPrefab prefab in teamPerks)
			{
				foreach (PerkBase perk in prefab.PerkBehaviors)
				{
					SubItemSwapPerk subSwapPerk = perk as SubItemSwapPerk;
					if (subSwapPerk != null)
					{
						builder.Add(subSwapPerk);
					}
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x17000911 RID: 2321
		// (get) Token: 0x06002168 RID: 8552 RVA: 0x00150175 File Offset: 0x0014E375
		// (set) Token: 0x06002169 RID: 8553 RVA: 0x0015017D File Offset: 0x0014E37D
		public Vector2 HiddenSubPosition { get; private set; }

		// Token: 0x17000912 RID: 2322
		// (get) Token: 0x0600216A RID: 8554 RVA: 0x00150186 File Offset: 0x0014E386
		// (set) Token: 0x0600216B RID: 8555 RVA: 0x0015018E File Offset: 0x0014E38E
		public ushort IdOffset { get; private set; }

		// Token: 0x17000913 RID: 2323
		// (get) Token: 0x0600216C RID: 8556 RVA: 0x00150197 File Offset: 0x0014E397
		// (set) Token: 0x0600216D RID: 8557 RVA: 0x001501A0 File Offset: 0x0014E3A0
		public static Submarine MainSub
		{
			get
			{
				return Submarine.MainSubs[0];
			}
			set
			{
				Submarine.MainSubs[0] = value;
			}
		}

		// Token: 0x17000914 RID: 2324
		// (get) Token: 0x0600216E RID: 8558 RVA: 0x001501AA File Offset: 0x0014E3AA
		public static IEnumerable<MapEntity> VisibleEntities
		{
			get
			{
				return Submarine.visibleEntities;
			}
		}

		// Token: 0x17000915 RID: 2325
		// (get) Token: 0x0600216F RID: 8559 RVA: 0x001501B4 File Offset: 0x0014E3B4
		public IEnumerable<Submarine> DockedTo
		{
			get
			{
				Submarine.<get_DockedTo>d__55 <get_DockedTo>d__ = new Submarine.<get_DockedTo>d__55(-2);
				<get_DockedTo>d__.<>4__this = this;
				return <get_DockedTo>d__;
			}
		}

		// Token: 0x17000916 RID: 2326
		// (get) Token: 0x06002170 RID: 8560 RVA: 0x001501D1 File Offset: 0x0014E3D1
		public static Vector2 LastPickedPosition
		{
			get
			{
				return Submarine.lastPickedPosition;
			}
		}

		// Token: 0x17000917 RID: 2327
		// (get) Token: 0x06002171 RID: 8561 RVA: 0x001501D8 File Offset: 0x0014E3D8
		public static float LastPickedFraction
		{
			get
			{
				return Submarine.lastPickedFraction;
			}
		}

		// Token: 0x17000918 RID: 2328
		// (get) Token: 0x06002172 RID: 8562 RVA: 0x001501DF File Offset: 0x0014E3DF
		public static Fixture LastPickedFixture
		{
			get
			{
				return Submarine.lastPickedFixture;
			}
		}

		// Token: 0x17000919 RID: 2329
		// (get) Token: 0x06002173 RID: 8563 RVA: 0x001501E6 File Offset: 0x0014E3E6
		public static Vector2 LastPickedNormal
		{
			get
			{
				return Submarine.lastPickedNormal;
			}
		}

		// Token: 0x1700091A RID: 2330
		// (get) Token: 0x06002174 RID: 8564 RVA: 0x001501ED File Offset: 0x0014E3ED
		// (set) Token: 0x06002175 RID: 8565 RVA: 0x001501F5 File Offset: 0x0014E3F5
		public bool Loading { get; private set; }

		// Token: 0x1700091B RID: 2331
		// (get) Token: 0x06002176 RID: 8566 RVA: 0x001501FE File Offset: 0x0014E3FE
		// (set) Token: 0x06002177 RID: 8567 RVA: 0x00150206 File Offset: 0x0014E406
		public bool GodMode { get; set; }

		// Token: 0x1700091C RID: 2332
		// (get) Token: 0x06002178 RID: 8568 RVA: 0x0015020F File Offset: 0x0014E40F
		public static List<Submarine> Loaded
		{
			get
			{
				return Submarine.loaded;
			}
		}

		// Token: 0x1700091D RID: 2333
		// (get) Token: 0x06002179 RID: 8569 RVA: 0x00150216 File Offset: 0x0014E416
		public SubmarineBody SubBody
		{
			get
			{
				return this.subBody;
			}
		}

		// Token: 0x1700091E RID: 2334
		// (get) Token: 0x0600217A RID: 8570 RVA: 0x0015021E File Offset: 0x0014E41E
		public PhysicsBody PhysicsBody
		{
			get
			{
				SubmarineBody submarineBody = this.subBody;
				if (submarineBody == null)
				{
					return null;
				}
				return submarineBody.Body;
			}
		}

		// Token: 0x1700091F RID: 2335
		// (get) Token: 0x0600217B RID: 8571 RVA: 0x00150231 File Offset: 0x0014E431
		public Rectangle Borders
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Borders;
				}
				return Rectangle.Empty;
			}
		}

		// Token: 0x17000920 RID: 2336
		// (get) Token: 0x0600217C RID: 8572 RVA: 0x0015024C File Offset: 0x0014E44C
		public Rectangle VisibleBorders
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.VisibleBorders;
				}
				return Rectangle.Empty;
			}
		}

		// Token: 0x17000921 RID: 2337
		// (get) Token: 0x0600217D RID: 8573 RVA: 0x00150267 File Offset: 0x0014E467
		public override Vector2 Position
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Position - this.HiddenSubPosition;
				}
				return Vector2.Zero;
			}
		}

		// Token: 0x17000922 RID: 2338
		// (get) Token: 0x0600217E RID: 8574 RVA: 0x0015028D File Offset: 0x0014E48D
		public override Vector2 WorldPosition
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Position;
				}
				return Vector2.Zero;
			}
		}

		// Token: 0x17000923 RID: 2339
		// (get) Token: 0x0600217F RID: 8575 RVA: 0x001502A8 File Offset: 0x0014E4A8
		public float RealWorldCrushDepth
		{
			get
			{
				if (this.realWorldCrushDepth == null)
				{
					this.realWorldCrushDepth = new float?(float.PositiveInfinity);
					foreach (Structure structure in Structure.WallList)
					{
						if (structure.Submarine == this && structure.HasBody && !structure.Indestructible)
						{
							this.realWorldCrushDepth = new float?(Math.Min(structure.CrushDepth, this.realWorldCrushDepth.Value));
						}
					}
				}
				return this.realWorldCrushDepth.Value;
			}
		}

		// Token: 0x17000924 RID: 2340
		// (get) Token: 0x06002180 RID: 8576 RVA: 0x00150358 File Offset: 0x0014E558
		public float RealWorldDepth
		{
			get
			{
				Level level = Level.Loaded;
				if (((level != null) ? level.GenerationParams : null) == null)
				{
					return -this.WorldPosition.Y * Physics.DisplayToRealWorldRatio;
				}
				return Level.Loaded.GetRealWorldDepth(this.WorldPosition.Y);
			}
		}

		// Token: 0x17000925 RID: 2341
		// (get) Token: 0x06002181 RID: 8577 RVA: 0x00150395 File Offset: 0x0014E595
		public bool IsAboveLevel
		{
			get
			{
				return Level.IsPositionAboveLevel(this.WorldPosition);
			}
		}

		// Token: 0x17000926 RID: 2342
		// (get) Token: 0x06002182 RID: 8578 RVA: 0x001503A4 File Offset: 0x0014E5A4
		public bool AtEndExit
		{
			get
			{
				if (Level.Loaded == null)
				{
					return false;
				}
				if (Level.Loaded.EndOutpost != null)
				{
					if (this.DockedTo.Contains(Level.Loaded.EndOutpost))
					{
						return true;
					}
					if (Level.Loaded.EndOutpost.exitPoints.Any<WayPoint>())
					{
						return this.IsAtOutpostExit(Level.Loaded.EndOutpost);
					}
				}
				else if (Level.Loaded.Type == LevelData.LevelType.Outpost && Level.Loaded.StartOutpost != null)
				{
					return this.IsAtOutpostExit(Level.Loaded.StartOutpost);
				}
				return Vector2.DistanceSquared(this.Position + this.HiddenSubPosition, Level.Loaded.EndExitPosition) < 36000000f;
			}
		}

		// Token: 0x17000927 RID: 2343
		// (get) Token: 0x06002183 RID: 8579 RVA: 0x00150458 File Offset: 0x0014E658
		public bool AtStartExit
		{
			get
			{
				if (Level.Loaded == null)
				{
					return false;
				}
				if (Level.Loaded.StartOutpost != null)
				{
					if (this.DockedTo.Contains(Level.Loaded.StartOutpost))
					{
						return true;
					}
					if (Level.Loaded.StartOutpost.exitPoints.Any<WayPoint>())
					{
						return this.IsAtOutpostExit(Level.Loaded.StartOutpost);
					}
				}
				return Vector2.DistanceSquared(this.Position + this.HiddenSubPosition, Level.Loaded.StartExitPosition) < 36000000f;
			}
		}

		// Token: 0x17000928 RID: 2344
		// (get) Token: 0x06002184 RID: 8580 RVA: 0x001504E1 File Offset: 0x0014E6E1
		public bool AtEitherExit
		{
			get
			{
				return this.AtStartExit || this.AtEndExit;
			}
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x001504F4 File Offset: 0x0014E6F4
		private bool IsAtOutpostExit(Submarine outpost)
		{
			if (outpost.exitPoints.Any<WayPoint>())
			{
				Rectangle worldBorders = this.GetDockedBorders(true);
				worldBorders.Location += this.WorldPosition.ToPoint();
				foreach (WayPoint exitPoint in outpost.exitPoints)
				{
					if (exitPoint.ExitPointSize != Point.Zero)
					{
						if (Submarine.RectsOverlap(worldBorders, exitPoint.ExitPointWorldRect, true))
						{
							return true;
						}
					}
					else if (Submarine.RectContains(worldBorders, exitPoint.WorldPosition, false))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x17000929 RID: 2345
		// (get) Token: 0x06002186 RID: 8582 RVA: 0x001505B8 File Offset: 0x0014E7B8
		// (set) Token: 0x06002187 RID: 8583 RVA: 0x001505C0 File Offset: 0x0014E7C0
		public new Vector2 DrawPosition { get; private set; }

		// Token: 0x1700092A RID: 2346
		// (get) Token: 0x06002188 RID: 8584 RVA: 0x001505C9 File Offset: 0x0014E7C9
		public override Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x06002189 RID: 8585 RVA: 0x001505D6 File Offset: 0x0014E7D6
		// (set) Token: 0x0600218A RID: 8586 RVA: 0x001505F1 File Offset: 0x0014E7F1
		public Vector2 Velocity
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Velocity;
				}
				return Vector2.Zero;
			}
			set
			{
				if (this.subBody == null)
				{
					return;
				}
				this.subBody.Velocity = value;
			}
		}

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x0600218B RID: 8587 RVA: 0x00150608 File Offset: 0x0014E808
		public List<Vector2> HullVertices
		{
			get
			{
				SubmarineBody submarineBody = this.subBody;
				if (submarineBody == null)
				{
					return null;
				}
				return submarineBody.HullVertices;
			}
		}

		// Token: 0x1700092D RID: 2349
		// (get) Token: 0x0600218C RID: 8588 RVA: 0x0015061C File Offset: 0x0014E81C
		public int SubmarineSpecificIDTag
		{
			get
			{
				int value = this.submarineSpecificIDTag.GetValueOrDefault();
				if (this.submarineSpecificIDTag == null)
				{
					Level level = Level.Loaded;
					value = ToolBox.StringToInt(((level != null) ? level.Seed : null) + this.Info.Name);
					this.submarineSpecificIDTag = new int?(value);
				}
				return this.submarineSpecificIDTag.Value;
			}
		}

		// Token: 0x1700092E RID: 2350
		// (get) Token: 0x0600218D RID: 8589 RVA: 0x00150680 File Offset: 0x0014E880
		public bool AtDamageDepth
		{
			get
			{
				return Level.Loaded != null && this.subBody != null && this.RealWorldDepth > Level.Loaded.RealWorldCrushDepth && this.RealWorldDepth > this.RealWorldCrushDepth;
			}
		}

		// Token: 0x1700092F RID: 2351
		// (get) Token: 0x0600218E RID: 8590 RVA: 0x001506B8 File Offset: 0x0014E8B8
		public bool AtCosmeticDamageDepth
		{
			get
			{
				return Level.Loaded != null && this.subBody != null && this.RealWorldDepth > Level.Loaded.RealWorldCrushDepth + -500f && this.RealWorldDepth > this.RealWorldCrushDepth + -500f;
			}
		}

		// Token: 0x17000930 RID: 2352
		// (get) Token: 0x0600218F RID: 8591 RVA: 0x00150704 File Offset: 0x0014E904
		public bool IsRespawnShuttle
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				RespawnManager respawnManager = (networkMember != null) ? networkMember.RespawnManager : null;
				return respawnManager != null && respawnManager.RespawnShuttles.Contains(this);
			}
		}

		// Token: 0x17000931 RID: 2353
		// (get) Token: 0x06002190 RID: 8592 RVA: 0x00150734 File Offset: 0x0014E934
		public IReadOnlyList<WayPoint> ExitPoints
		{
			get
			{
				return this.exitPoints;
			}
		}

		// Token: 0x06002191 RID: 8593 RVA: 0x0015073C File Offset: 0x0014E93C
		public override string ToString()
		{
			string[] array = new string[5];
			array[0] = "Barotrauma.Submarine (";
			int num = 1;
			SubmarineInfo info = this.Info;
			array[num] = (((info != null) ? info.Name : null) ?? "[NULL INFO]");
			array[2] = ", ";
			array[3] = this.IdOffset.ToString();
			array[4] = ")";
			return string.Concat(array);
		}

		// Token: 0x06002192 RID: 8594 RVA: 0x0015079C File Offset: 0x0014E99C
		public int CalculateBasePrice()
		{
			int minPrice = 1000;
			float volume = (from h in Hull.HullList
			where h.Submarine == this
			select h).Sum((Hull h) => h.Volume);
			float itemValue = (float)(from it in Item.ItemList
			where it.Submarine == this
			select it).Sum((Item it) => it.Prefab.GetMinPrice().GetValueOrDefault());
			float price = volume / 500f + itemValue / 100f;
			return Math.Max(minPrice, (int)price);
		}

		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x06002193 RID: 8595 RVA: 0x0015083E File Offset: 0x0014EA3E
		// (set) Token: 0x06002194 RID: 8596 RVA: 0x00150846 File Offset: 0x0014EA46
		public bool ImmuneToBallastFlora { get; set; }

		// Token: 0x06002195 RID: 8597 RVA: 0x00150850 File Offset: 0x0014EA50
		public void AttemptBallastFloraInfection(Identifier identifier, float deltaTime, float probability)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.ImmuneToBallastFlora)
			{
				return;
			}
			if (this.ballastFloraTimer < 1f)
			{
				this.ballastFloraTimer += deltaTime;
				return;
			}
			this.ballastFloraTimer = 0f;
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) >= probability)
			{
				return;
			}
			List<Pump> pumps = new List<Pump>();
			List<Item> allItems = this.GetItems(true);
			bool anyHasTag = allItems.Any((Item i) => i.HasTag(Tags.Ballast));
			foreach (Item item in allItems)
			{
				if (!anyHasTag || item.HasTag(Tags.Ballast))
				{
					Pump pump = item.GetComponent<Pump>();
					if (pump != null)
					{
						pumps.Add(pump);
					}
				}
			}
			if (!pumps.Any<Pump>())
			{
				return;
			}
			Pump randomPump = pumps.GetRandom(Rand.RandSync.Unsynced);
			if (randomPump.IsOn && randomPump.HasPower && randomPump.FlowPercentage > 0f && randomPump.Item.Condition > 0f)
			{
				randomPump.InfectBallast(identifier, false);
			}
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x00150994 File Offset: 0x0014EB94
		public void MakeWreck()
		{
			this.Info.Type = SubmarineType.Wreck;
			this.ShowSonarMarker = false;
			this.DockedTo.ForEach(delegate(Submarine s)
			{
				s.ShowSonarMarker = false;
			});
			this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
			this.TeamID = CharacterTeamType.None;
		}

		// Token: 0x17000933 RID: 2355
		// (get) Token: 0x06002197 RID: 8599 RVA: 0x001509F6 File Offset: 0x0014EBF6
		// (set) Token: 0x06002198 RID: 8600 RVA: 0x001509FE File Offset: 0x0014EBFE
		public WreckAI WreckAI { get; private set; }

		// Token: 0x17000934 RID: 2356
		// (get) Token: 0x06002199 RID: 8601 RVA: 0x00150A07 File Offset: 0x0014EC07
		// (set) Token: 0x0600219A RID: 8602 RVA: 0x00150A0F File Offset: 0x0014EC0F
		public SubmarineTurretAI TurretAI { get; private set; }

		// Token: 0x0600219B RID: 8603 RVA: 0x00150A18 File Offset: 0x0014EC18
		public bool CreateWreckAI()
		{
			this.WreckAI = WreckAI.Create(this);
			return this.WreckAI != null;
		}

		// Token: 0x0600219C RID: 8604 RVA: 0x00150A30 File Offset: 0x0014EC30
		public bool CreateTurretAI()
		{
			this.TurretAI = new SubmarineTurretAI(this, default(Identifier));
			return this.TurretAI != null;
		}

		// Token: 0x0600219D RID: 8605 RVA: 0x00150A5B File Offset: 0x0014EC5B
		public void DisableWreckAI()
		{
			if (this.WreckAI == null)
			{
				WreckAI.RemoveThalamusItems(this);
				return;
			}
			WreckAI wreckAI = this.WreckAI;
			if (wreckAI != null)
			{
				wreckAI.Remove();
			}
			this.WreckAI = null;
		}

		// Token: 0x0600219E RID: 8606 RVA: 0x00150A84 File Offset: 0x0014EC84
		public Rectangle GetDockedBorders(bool allowDifferentTeam = true)
		{
			Submarine.checkSubmarineBorders.Clear();
			return this.GetDockedBordersRecursive(allowDifferentTeam);
		}

		// Token: 0x0600219F RID: 8607 RVA: 0x00150A98 File Offset: 0x0014EC98
		private Rectangle GetDockedBordersRecursive(bool allowDifferentTeam)
		{
			Rectangle dockedBorders = this.Borders;
			Submarine.checkSubmarineBorders.Add(this);
			IEnumerable<Submarine> connectedSubs = from s in this.DockedTo
			where !Submarine.checkSubmarineBorders.Contains(s) && !s.Info.IsOutpost && (allowDifferentTeam || s.TeamID == this.TeamID)
			select s;
			foreach (Submarine dockedSub in connectedSubs)
			{
				Vector2? expectedLocation = Submarine.CalculateDockOffset(this, dockedSub);
				if (expectedLocation != null)
				{
					Rectangle dockedSubBorders = dockedSub.GetDockedBordersRecursive(allowDifferentTeam);
					dockedSubBorders.Location += MathUtils.ToPoint(expectedLocation.Value);
					dockedBorders.Y = -dockedBorders.Y;
					dockedSubBorders.Y = -dockedSubBorders.Y;
					dockedBorders = Rectangle.Union(dockedBorders, dockedSubBorders);
					dockedBorders.Y = -dockedBorders.Y;
				}
			}
			return dockedBorders;
		}

		// Token: 0x060021A0 RID: 8608 RVA: 0x00150B94 File Offset: 0x0014ED94
		public IEnumerable<Submarine> GetConnectedSubs()
		{
			return this.connectedSubs;
		}

		// Token: 0x060021A1 RID: 8609 RVA: 0x00150B9C File Offset: 0x0014ED9C
		public void RefreshConnectedSubs()
		{
			this.connectedSubs.Clear();
			this.connectedSubs.Add(this);
			this.GetConnectedSubsRecursive(this.connectedSubs);
		}

		// Token: 0x060021A2 RID: 8610 RVA: 0x00150BC4 File Offset: 0x0014EDC4
		private void GetConnectedSubsRecursive(HashSet<Submarine> subs)
		{
			foreach (Submarine dockedSub in this.DockedTo)
			{
				if (!subs.Contains(dockedSub))
				{
					subs.Add(dockedSub);
					dockedSub.GetConnectedSubsRecursive(subs);
				}
			}
		}

		// Token: 0x060021A3 RID: 8611 RVA: 0x00150C24 File Offset: 0x0014EE24
		public Vector2 FindSpawnPos(Vector2 spawnPos, Point? submarineSize = null, float subDockingPortOffset = 0f, int verticalMoveDir = 0)
		{
			Submarine.<>c__DisplayClass159_0 CS$<>8__locals1;
			CS$<>8__locals1.subDockingPortOffset = subDockingPortOffset;
			Rectangle dockedBorders = this.GetDockedBorders(true);
			Vector2 diffFromDockedBorders = new Vector2((float)dockedBorders.Center.X, (float)(dockedBorders.Y - dockedBorders.Height / 2)) - new Vector2((float)this.Borders.Center.X, (float)(this.Borders.Y - this.Borders.Height / 2));
			CS$<>8__locals1.minWidth = Math.Max((submarineSize != null) ? submarineSize.Value.X : dockedBorders.Width, 500);
			int minHeight = Math.Max((submarineSize != null) ? submarineSize.Value.Y : dockedBorders.Height, 1000);
			int padding = 100;
			CS$<>8__locals1.minWidth += padding;
			minHeight += padding;
			int iterations = 0;
			do
			{
				Vector2 potentialPos = spawnPos;
				if (verticalMoveDir != 0)
				{
					verticalMoveDir = Math.Sign(verticalMoveDir);
					Vector2 rayEnd = new Vector2(potentialPos.X, (float)((verticalMoveDir > 0) ? Level.Loaded.Size.Y : 0));
					Vector2 closestPickedPos = rayEnd;
					for (float x = -1f; x <= 1f; x += 0.2f)
					{
						Vector2 xOffset = Vector2.UnitX * (float)CS$<>8__locals1.minWidth / 2f * x;
						xOffset.X += CS$<>8__locals1.subDockingPortOffset;
						if (Submarine.PickBody(ConvertUnits.ToSimUnits(potentialPos + xOffset), ConvertUnits.ToSimUnits(rayEnd + xOffset), null, new Category?(Category.Cat1 | Category.Cat8), true, delegate(Fixture f)
						{
							VoronoiCell voronoiCell = f.UserData as VoronoiCell;
							return voronoiCell == null || !voronoiCell.IsDestructible;
						}, false) != null)
						{
							int offsetFromWall = 10 * -verticalMoveDir;
							float pickedPos = ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition.Y) + (float)offsetFromWall;
							closestPickedPos.Y = ((verticalMoveDir > 0) ? Math.Min(closestPickedPos.Y, pickedPos) : Math.Max(closestPickedPos.Y, pickedPos));
						}
					}
					potentialPos.Y = closestPickedPos.Y;
				}
				Vector2 limits = Submarine.<FindSpawnPos>g__GetHorizontalLimits|159_0(new Vector2(potentialPos.X, potentialPos.Y - (float)dockedBorders.Height * 0.5f * (float)verticalMoveDir), (float)CS$<>8__locals1.minWidth, (float)minHeight, verticalMoveDir, padding, ref CS$<>8__locals1);
				if (limits.Y - limits.X >= (float)CS$<>8__locals1.minWidth)
				{
					Vector2 newSpawnPos = new Vector2(spawnPos.X, potentialPos.Y - (float)dockedBorders.Height * 0.5f * (float)verticalMoveDir);
					bool couldMoveInVerticalMoveDir = Math.Sign(newSpawnPos.Y - spawnPos.Y) == Math.Sign(verticalMoveDir);
					if (!couldMoveInVerticalMoveDir)
					{
						break;
					}
					spawnPos = Submarine.<FindSpawnPos>g__ClampToHorizontalLimits|159_1(newSpawnPos, limits, ref CS$<>8__locals1);
				}
				iterations++;
			}
			while (iterations < 5);
			spawnPos.Y = MathHelper.Clamp(spawnPos.Y, (float)(dockedBorders.Height / 2 + 10), (float)(Level.Loaded.Size.Y - dockedBorders.Height / 2 - padding * 2));
			return spawnPos - diffFromDockedBorders;
		}

		// Token: 0x060021A4 RID: 8612 RVA: 0x00150F40 File Offset: 0x0014F140
		public void UpdateTransform(bool interpolate = true)
		{
			this.DrawPosition = (interpolate ? Timing.Interpolate(this.prevPosition, this.Position) : this.Position);
			if (!interpolate)
			{
				this.prevPosition = this.Position;
			}
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x00150F74 File Offset: 0x0014F174
		public static Vector2 VectorToWorldGrid(Vector2 position, Submarine sub = null, bool round = false)
		{
			if (round)
			{
				position.X = MathF.Round(position.X / Submarine.GridSize.X) * Submarine.GridSize.X;
				position.Y = MathF.Round(position.Y / Submarine.GridSize.Y) * Submarine.GridSize.Y;
			}
			else
			{
				position.X = MathF.Floor(position.X / Submarine.GridSize.X) * Submarine.GridSize.X;
				position.Y = MathF.Ceiling(position.Y / Submarine.GridSize.Y) * Submarine.GridSize.Y;
			}
			if (sub != null)
			{
				position.X += sub.Position.X % Submarine.GridSize.X;
				position.Y += sub.Position.Y % Submarine.GridSize.Y;
			}
			return position;
		}

		// Token: 0x060021A6 RID: 8614 RVA: 0x0015106C File Offset: 0x0014F26C
		public Rectangle CalculateDimensions(bool onlyHulls = true)
		{
			List<MapEntity> entities = onlyHulls ? Hull.HullList.FindAll((Hull h) => h.Submarine == this).Cast<MapEntity>().ToList<MapEntity>() : MapEntity.MapEntityList.FindAll((MapEntity me) => me.Submarine == this);
			entities.RemoveAll(delegate(MapEntity e)
			{
				Item item2 = e as Item;
				if (item2 != null)
				{
					if (item2.GetComponent<Turret>() != null)
					{
						return false;
					}
					if (item2.body != null && !item2.body.Enabled)
					{
						return true;
					}
				}
				return e.IsHidden;
			});
			if (entities.Count == 0)
			{
				return Rectangle.Empty;
			}
			float minX = (float)entities[0].Rect.X;
			float minY = (float)(entities[0].Rect.Y - entities[0].Rect.Height);
			float maxX = (float)entities[0].Rect.Right;
			float maxY = (float)entities[0].Rect.Y;
			for (int i = 1; i < entities.Count; i++)
			{
				Item item = entities[i] as Item;
				if (item != null)
				{
					Turret turret = item.GetComponent<Turret>();
					if (turret != null)
					{
						minX = Math.Min(minX, (float)entities[i].Rect.X + turret.TransformedBarrelPos.X * 2f);
						minY = Math.Min(minY, (float)(entities[i].Rect.Y - entities[i].Rect.Height) - turret.TransformedBarrelPos.Y * 2f);
						maxX = Math.Max(maxX, (float)entities[i].Rect.Right + turret.TransformedBarrelPos.X * 2f);
						maxY = Math.Max(maxY, (float)entities[i].Rect.Y - turret.TransformedBarrelPos.Y * 2f);
					}
				}
				minX = Math.Min(minX, (float)entities[i].Rect.X);
				minY = Math.Min(minY, (float)(entities[i].Rect.Y - entities[i].Rect.Height));
				maxX = Math.Max(maxX, (float)entities[i].Rect.Right);
				maxY = Math.Max(maxY, (float)entities[i].Rect.Y);
			}
			return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
		}

		// Token: 0x060021A7 RID: 8615 RVA: 0x001512F0 File Offset: 0x0014F4F0
		public static Rectangle AbsRect(Vector2 pos, Vector2 size)
		{
			if (size.X < 0f)
			{
				pos.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				pos.Y -= size.Y;
				size.Y = -size.Y;
			}
			return new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
		}

		// Token: 0x060021A8 RID: 8616 RVA: 0x00151378 File Offset: 0x0014F578
		public static RectangleF AbsRectF(Vector2 pos, Vector2 size)
		{
			if (size.X < 0f)
			{
				pos.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				pos.Y += size.Y;
				size.Y = -size.Y;
			}
			return new RectangleF(pos.X, pos.Y, size.X, size.Y);
		}

		// Token: 0x060021A9 RID: 8617 RVA: 0x001513FC File Offset: 0x0014F5FC
		public static bool RectContains(Rectangle rect, Vector2 pos, bool inclusive = false)
		{
			if (inclusive)
			{
				return pos.X >= (float)rect.X && pos.X <= (float)(rect.X + rect.Width) && pos.Y <= (float)rect.Y && pos.Y >= (float)(rect.Y - rect.Height);
			}
			return pos.X > (float)rect.X && pos.X < (float)(rect.X + rect.Width) && pos.Y < (float)rect.Y && pos.Y > (float)(rect.Y - rect.Height);
		}

		// Token: 0x060021AA RID: 8618 RVA: 0x001514A8 File Offset: 0x0014F6A8
		public static bool RectsOverlap(Rectangle rect1, Rectangle rect2, bool inclusive = true)
		{
			if (inclusive)
			{
				return rect1.X <= rect2.X + rect2.Width && rect1.X + rect1.Width >= rect2.X && rect1.Y >= rect2.Y - rect2.Height && rect1.Y - rect1.Height <= rect2.Y;
			}
			return rect1.X < rect2.X + rect2.Width && rect1.X + rect1.Width > rect2.X && rect1.Y > rect2.Y - rect2.Height && rect1.Y - rect1.Height < rect2.Y;
		}

		// Token: 0x060021AB RID: 8619 RVA: 0x00151568 File Offset: 0x0014F768
		public static bool RectsOverlap(RectangleF rect1, RectangleF rect2, bool inclusive = true)
		{
			if (inclusive)
			{
				return rect1.X <= rect2.X + rect2.Width && rect1.X + rect1.Width >= rect2.X && rect1.Y >= rect2.Y - rect2.Height && rect1.Y - rect1.Height <= rect2.Y;
			}
			return rect1.X < rect2.X + rect2.Width && rect1.X + rect1.Width > rect2.X && rect1.Y > rect2.Y - rect2.Height && rect1.Y - rect1.Height < rect2.Y;
		}

		// Token: 0x060021AC RID: 8620 RVA: 0x00151628 File Offset: 0x0014F828
		public static Body PickBody(Vector2 rayStart, Vector2 rayEnd, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null, bool allowInsideFixture = false)
		{
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 0.0001f)
			{
				return null;
			}
			float closestFraction = 1f;
			Vector2 closestNormal = Vector2.Zero;
			Fixture closestFixture = null;
			Body closestBody = null;
			if (allowInsideFixture)
			{
				AABB aabb = new AABB(rayStart - Vector2.One * 0.001f, rayStart + Vector2.One * 0.001f);
				GameMain.World.QueryAABB(delegate(Fixture fixture)
				{
					if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
					{
						return true;
					}
					Transform transform;
					fixture.Body.GetTransform(out transform);
					if (!fixture.Shape.TestPoint(ref transform, ref rayStart))
					{
						return true;
					}
					closestFraction = 0f;
					closestNormal = Vector2.Normalize(rayEnd - rayStart);
					closestFixture = fixture;
					if (fixture.Body != null)
					{
						closestBody = fixture.Body;
					}
					return false;
				}, ref aabb);
				if (closestFraction <= 0f)
				{
					Submarine.lastPickedPosition = rayStart;
					Submarine.lastPickedFraction = closestFraction;
					Submarine.lastPickedFixture = closestFixture;
					Submarine.lastPickedNormal = closestNormal;
					return closestBody;
				}
			}
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
				{
					return -1f;
				}
				if (fraction < closestFraction)
				{
					closestFraction = fraction;
					closestNormal = normal;
					closestFixture = fixture;
					if (fixture.Body != null)
					{
						closestBody = fixture.Body;
					}
				}
				return fraction;
			}, rayStart, rayEnd, collisionCategory.GetValueOrDefault(Category.All));
			Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * closestFraction;
			Submarine.lastPickedFraction = closestFraction;
			Submarine.lastPickedFixture = closestFixture;
			Submarine.lastPickedNormal = closestNormal;
			return closestBody;
		}

		// Token: 0x060021AD RID: 8621 RVA: 0x001517C4 File Offset: 0x0014F9C4
		public static float LastPickedBodyDist(Body body)
		{
			if (!Submarine.bodyDist.ContainsKey(body))
			{
				return 0f;
			}
			return Submarine.bodyDist[body];
		}

		// Token: 0x060021AE RID: 8622 RVA: 0x001517E4 File Offset: 0x0014F9E4
		public static IEnumerable<Body> PickBodies(Vector2 rayStart, Vector2 rayEnd, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null, bool allowInsideFixture = false)
		{
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 1E-05f)
			{
				rayEnd += Vector2.UnitX * 0.001f;
			}
			float closestFraction = 1f;
			Submarine.bodies.Clear();
			Submarine.bodyDist.Clear();
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
				{
					return -1f;
				}
				if (fixture.Body != null)
				{
					Submarine.bodies.Add(fixture.Body);
					Submarine.bodyDist[fixture.Body] = fraction;
				}
				if (fraction < closestFraction)
				{
					Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * fraction;
					Submarine.lastPickedFraction = fraction;
					Submarine.lastPickedNormal = normal;
					Submarine.lastPickedFixture = fixture;
				}
				return -1f;
			}, rayStart, rayEnd, collisionCategory.GetValueOrDefault(Category.All));
			if (allowInsideFixture)
			{
				AABB aabb = new AABB(rayStart - Vector2.One * 0.001f, rayStart + Vector2.One * 0.001f);
				GameMain.World.QueryAABB(delegate(Fixture fixture)
				{
					if (Submarine.bodies.Contains(fixture.Body) || fixture.Body == null)
					{
						return true;
					}
					if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
					{
						return true;
					}
					Transform transform;
					fixture.Body.GetTransform(out transform);
					if (!fixture.Shape.TestPoint(ref transform, ref rayStart))
					{
						return true;
					}
					closestFraction = 0f;
					Submarine.lastPickedPosition = rayStart;
					Submarine.lastPickedFraction = 0f;
					Submarine.lastPickedNormal = Vector2.Normalize(rayEnd - rayStart);
					Submarine.lastPickedFixture = fixture;
					Submarine.bodies.Add(fixture.Body);
					Submarine.bodyDist[fixture.Body] = 0f;
					return false;
				}, ref aabb);
			}
			Submarine.bodies.Sort((Body b1, Body b2) => Submarine.bodyDist[b1].CompareTo(Submarine.bodyDist[b2]));
			return Submarine.bodies;
		}

		// Token: 0x060021AF RID: 8623 RVA: 0x00151934 File Offset: 0x0014FB34
		private static bool CheckFixtureCollision(Fixture fixture, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null)
		{
			if (fixture == null || (ignoreSensors && fixture.IsSensor) || fixture.CollisionCategories == Category.None || fixture.CollisionCategories == Category.Cat5)
			{
				return false;
			}
			if (customPredicate != null && !customPredicate(fixture))
			{
				return false;
			}
			if (collisionCategory != null && !fixture.CollisionCategories.HasFlag(collisionCategory.Value) && !collisionCategory.Value.HasFlag(fixture.CollisionCategories))
			{
				return false;
			}
			if (ignoredBodies != null && ignoredBodies.Contains(fixture.Body))
			{
				return false;
			}
			Structure structure = fixture.Body.UserData as Structure;
			return structure == null || !structure.IsPlatform || collisionCategory == null || collisionCategory.Value.HasFlag(Category.Cat3);
		}

		// Token: 0x060021B0 RID: 8624 RVA: 0x00151A10 File Offset: 0x0014FC10
		public static Body CheckVisibility(Vector2 rayStart, Vector2 rayEnd, bool ignoreLevel = false, bool ignoreSubs = false, bool ignoreSensors = true, bool ignoreDisabledWalls = true, bool ignoreBranches = true, Predicate<Fixture> blocksVisibilityPredicate = null)
		{
			Body closestBody = null;
			float closestFraction = 1f;
			Fixture closestFixture = null;
			Vector2 closestNormal = Vector2.Zero;
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 0.01f)
			{
				Submarine.lastPickedPosition = rayEnd;
				return null;
			}
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (fixture == null)
				{
					return -1f;
				}
				if (ignoreSensors && fixture.IsSensor)
				{
					return -1f;
				}
				if (ignoreLevel && fixture.CollisionCategories.HasFlag(Category.Cat8))
				{
					return -1f;
				}
				if (!fixture.CollisionCategories.HasFlag(Category.Cat8) && !fixture.CollisionCategories.HasFlag(Category.Cat1) && !fixture.CollisionCategories.HasFlag(Category.Cat9))
				{
					return -1f;
				}
				if (ignoreSubs && fixture.Body.UserData is Submarine)
				{
					return -1f;
				}
				if (ignoreBranches && fixture.Body.UserData is VineTile)
				{
					return -1f;
				}
				if (fixture.Body.UserData as string == "ruinroom")
				{
					return -1f;
				}
				if (fixture.UserData is Hull)
				{
					return -1f;
				}
				Structure structure = fixture.Body.UserData as Structure;
				if (structure != null)
				{
					if (structure.IsPlatform || structure.StairDirection != Direction.None)
					{
						return -1f;
					}
					if (ignoreDisabledWalls)
					{
						int sectionIndex = structure.FindSectionIndex(ConvertUnits.ToDisplayUnits(point), false, false);
						if (sectionIndex > -1 && structure.SectionBodyDisabled(sectionIndex))
						{
							return -1f;
						}
					}
				}
				if (blocksVisibilityPredicate != null && !blocksVisibilityPredicate(fixture))
				{
					return -1f;
				}
				if (fraction < closestFraction)
				{
					closestBody = fixture.Body;
					closestFraction = fraction;
					closestFixture = fixture;
					closestNormal = normal;
				}
				return closestFraction;
			}, rayStart, rayEnd, Category.All);
			Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * closestFraction;
			Submarine.lastPickedFraction = closestFraction;
			Submarine.lastPickedFixture = closestFixture;
			Submarine.lastPickedNormal = closestNormal;
			return closestBody;
		}

		// Token: 0x17000935 RID: 2357
		// (get) Token: 0x060021B1 RID: 8625 RVA: 0x00151AEC File Offset: 0x0014FCEC
		public bool FlippedX
		{
			get
			{
				return this.flippedX;
			}
		}

		// Token: 0x060021B2 RID: 8626 RVA: 0x00151AF4 File Offset: 0x0014FCF4
		public void FlipX(List<Submarine> parents = null)
		{
			if (parents == null)
			{
				parents = new List<Submarine>();
			}
			parents.Add(this);
			this.flippedX = !this.flippedX;
			Item.UpdateHulls();
			List<Item> bodyItems = Item.ItemList.FindAll((Item it) => it.Submarine == this && it.body != null);
			List<MapEntity> subEntities = MapEntity.MapEntityList.FindAll((MapEntity me) => me.Submarine == this);
			foreach (MapEntity e in subEntities)
			{
				if (!(e is Item))
				{
					LinkedSubmarine linkedSub = e as LinkedSubmarine;
					if (linkedSub != null)
					{
						Submarine sub = linkedSub.Sub;
						if (sub == null)
						{
							Vector2 relative = linkedSub.Position - this.SubBody.Position;
							relative.X = -relative.X;
							linkedSub.Rect = new Rectangle((relative + this.SubBody.Position).ToPoint(), linkedSub.Rect.Size);
						}
						else if (!parents.Contains(sub))
						{
							Vector2 relative2 = sub.SubBody.Position - this.SubBody.Position;
							relative2.X = -relative2.X;
							sub.SetPosition(relative2 + this.SubBody.Position, new List<Submarine>(parents), true);
							sub.FlipX(parents);
						}
					}
					else
					{
						e.FlipX(true, false);
					}
				}
			}
			foreach (MapEntity mapEntity in subEntities)
			{
				mapEntity.Move(-this.HiddenSubPosition, true);
			}
			BodyType prevBodyType = this.subBody.Body.BodyType;
			Vector2 pos = new Vector2(this.subBody.Position.X, this.subBody.Position.Y);
			this.subBody.Body.Remove();
			this.subBody = new SubmarineBody(this, true);
			this.subBody.Body.BodyType = prevBodyType;
			this.SetPosition(pos, new List<Submarine>(from p in parents
			where p != this
			select p), true);
			if (this.entityGrid != null)
			{
				Hull.EntityGrids.Remove(this.entityGrid);
				this.entityGrid = null;
			}
			this.entityGrid = Hull.GenerateEntityGrid(this);
			this.SubBody.FlipX();
			foreach (MapEntity mapEntity2 in subEntities)
			{
				mapEntity2.Move(this.HiddenSubPosition, true);
			}
			for (int i = 0; i < 2; i++)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.GetComponent<DockingPort>() != null != (i == 0))
					{
						if (bodyItems.Contains(item))
						{
							item.Submarine = this;
							if (this.Position == Vector2.Zero)
							{
								item.Move(-this.HiddenSubPosition, true);
							}
						}
						else if (item.Submarine != this)
						{
							continue;
						}
						item.FlipX(true, false);
						if (!item.Prefab.CanFlipX && item.Prefab.AllowRotatingInEditor)
						{
							item.Rotation = -item.Rotation;
						}
					}
				}
			}
			foreach (DockingPort dockingPort in DockingPort.List)
			{
				DockingPort dockingTarget = dockingPort.DockingTarget;
				if (dockingTarget != null)
				{
					dockingPort.Undock(true);
					dockingPort.Dock(dockingTarget);
				}
			}
			Item.UpdateHulls();
			Gap.UpdateHulls();
			ConvexHull.RecalculateAll(this);
		}

		// Token: 0x060021B3 RID: 8627 RVA: 0x00151F5C File Offset: 0x0015015C
		public void EnableFactionSpecificEntities(Identifier factionIdentifier)
		{
			foreach (FactionPrefab faction in FactionPrefab.Prefabs)
			{
				this.SetLayerEnabled(faction.Identifier, faction.Identifier == factionIdentifier, false);
			}
		}

		// Token: 0x060021B4 RID: 8628 RVA: 0x00151FBC File Offset: 0x001501BC
		public static bool LayerExistsInAnySub(Identifier layer)
		{
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				if (me.Layer == layer)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060021B5 RID: 8629 RVA: 0x00152020 File Offset: 0x00150220
		public bool LayerExists(Identifier layer)
		{
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				if (me.Submarine == this || me.Layer == layer)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060021B6 RID: 8630 RVA: 0x0015208C File Offset: 0x0015028C
		public void SetLayerEnabled(Identifier layer, bool enabled, bool sendNetworkEvent = false)
		{
			Submarine.SetLayerEnabled(layer, enabled, from m in MapEntity.MapEntityList
			where m.Submarine == this
			select m);
		}

		// Token: 0x060021B7 RID: 8631 RVA: 0x001520AC File Offset: 0x001502AC
		public static void SetLayerEnabled(Identifier layer, bool enabled, IEnumerable<MapEntity> entities)
		{
			foreach (MapEntity entity in MapEntity.MapEntityList)
			{
				if (!string.IsNullOrEmpty(entity.Layer) && !(entity.Layer != layer))
				{
					entity.IsLayerHidden = !enabled;
					WayPoint wp = entity as WayPoint;
					if (wp != null)
					{
						if (enabled)
						{
							wp.SpawnType = wp.SpawnType.RemoveFlag(SpawnType.Disabled);
						}
						else
						{
							wp.SpawnType = wp.SpawnType.AddFlag(SpawnType.Disabled);
						}
					}
					else
					{
						Item item = entity as Item;
						if (item != null)
						{
							Submarine.<SetLayerEnabled>g__SetItemHidden|183_0(item, entity.IsLayerHidden);
						}
					}
				}
			}
		}

		// Token: 0x060021B8 RID: 8632 RVA: 0x00152170 File Offset: 0x00150370
		public void Update(float deltaTime)
		{
			this.RefreshConnectedSubs();
			if (this.Info.IsWreck)
			{
				WreckAI wreckAI = this.WreckAI;
				if (wreckAI != null)
				{
					wreckAI.Update(deltaTime);
				}
			}
			SubmarineTurretAI turretAI = this.TurretAI;
			if (turretAI != null)
			{
				turretAI.Update(deltaTime);
			}
			SubmarineBody submarineBody = this.subBody;
			if (((submarineBody != null) ? submarineBody.Body : null) == null)
			{
				return;
			}
			if (Level.Loaded != null && this.WorldPosition.Y < -1000000f && this.subBody.Body.Enabled && !this.IsRespawnShuttle)
			{
				this.subBody.Body.ResetDynamics();
				this.subBody.Body.Enabled = false;
				foreach (Character c in Character.CharacterList)
				{
					if (c.Submarine == this)
					{
						c.Kill(CauseOfDeathType.Pressure, null, false, true);
						c.Enabled = false;
					}
				}
				return;
			}
			this.subBody.Body.LinearVelocity = new Vector2(Submarine.LockX ? 0f : this.subBody.Body.LinearVelocity.X, Submarine.LockY ? 0f : this.subBody.Body.LinearVelocity.Y);
			this.subBody.Update(deltaTime);
			for (int i = 0; i < 2; i++)
			{
				if (Submarine.MainSubs[i] != null && this != Submarine.MainSubs[i] && Submarine.MainSubs[i].DockedTo.Contains(this))
				{
					return;
				}
			}
			this.networkUpdateTimer -= MathHelper.Clamp(this.Velocity.Length() * 10f, 0.1f, 5f) * deltaTime;
			if (this.networkUpdateTimer < 0f)
			{
				this.networkUpdateTimer = 1f;
			}
		}

		// Token: 0x060021B9 RID: 8633 RVA: 0x00152364 File Offset: 0x00150564
		public void ApplyForce(Vector2 force)
		{
			if (this.subBody != null)
			{
				this.subBody.ApplyForce(force);
			}
		}

		// Token: 0x060021BA RID: 8634 RVA: 0x0015237C File Offset: 0x0015057C
		public void EnableMaintainPosition()
		{
			using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					if (item.Submarine == this)
					{
						Steering steering = item.GetComponent<Steering>();
						if (steering != null && item.Connections != null)
						{
							List<Item> connectedItems = new List<Item>();
							foreach (Connection c in item.Connections)
							{
								if (!c.IsPower)
								{
									connectedItems.AddRange(from engine in item.GetConnectedComponentsRecursive<Engine>(c, false, true)
									select engine.Item);
									connectedItems.AddRange(from pump in item.GetConnectedComponentsRecursive<Pump>(c, false, true)
									select pump.Item);
								}
							}
							if (connectedItems.Count((Item it) => it.Submarine != item.Submarine) <= connectedItems.Count / 2)
							{
								steering.MaintainPos = true;
								steering.PosToMaintain = new Vector2?(this.WorldPosition);
								steering.AutoPilot = true;
							}
						}
					}
				}
			}
		}

		// Token: 0x060021BB RID: 8635 RVA: 0x00152528 File Offset: 0x00150728
		public void NeutralizeBallast()
		{
			if (this.PhysicsBody.BodyType != BodyType.Dynamic)
			{
				return;
			}
			float neutralBallastLevel = 0.5f;
			int selectedSteeringValue = 0;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == this)
				{
					Steering steering = item.GetComponent<Steering>();
					if (steering != null)
					{
						int steeringValue = 1;
						ConnectionPanel component = item.GetComponent<ConnectionPanel>();
						Connection connection;
						if (component == null)
						{
							connection = null;
						}
						else
						{
							connection = component.Connections.Find((Connection c) => c.Name == "velocity_x_out");
						}
						Connection connectionX = connection;
						ConnectionPanel component2 = item.GetComponent<ConnectionPanel>();
						Connection connection2;
						if (component2 == null)
						{
							connection2 = null;
						}
						else
						{
							connection2 = component2.Connections.Find((Connection c) => c.Name == "velocity_y_out");
						}
						Connection connectionY = connection2;
						if (connectionX != null)
						{
							foreach (Engine engine in steering.Item.GetConnectedComponentsRecursive<Engine>(connectionX, false, true))
							{
								if (engine.Item.Submarine == this)
								{
									steeringValue++;
								}
							}
						}
						if (connectionY != null)
						{
							foreach (Pump pump in steering.Item.GetConnectedComponentsRecursive<Pump>(connectionY, false, true))
							{
								if (pump.Item.Submarine == this)
								{
									steeringValue++;
								}
							}
						}
						if (steeringValue > selectedSteeringValue)
						{
							neutralBallastLevel = steering.NeutralBallastLevel;
						}
					}
				}
			}
			HashSet<Hull> ballastHulls = new HashSet<Hull>();
			foreach (Item item2 in Item.ItemList)
			{
				if (item2.Submarine == this)
				{
					Pump pump2 = item2.GetComponent<Pump>();
					if (pump2 != null && item2.CurrentHull != null && item2.GetComponent<ConnectionPanel>() != null && (item2.HasTag(Tags.Ballast) || item2.CurrentHull.RoomName.Contains("ballast", StringComparison.OrdinalIgnoreCase)))
					{
						pump2.FlowPercentage = 0f;
						ballastHulls.Add(item2.CurrentHull);
					}
				}
			}
			float waterVolume = 0f;
			float volume = 0f;
			float excessWater = 0f;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine == this)
				{
					waterVolume += hull.WaterVolume;
					volume += hull.Volume;
					if (!ballastHulls.Contains(hull))
					{
						excessWater += hull.WaterVolume;
					}
				}
			}
			neutralBallastLevel -= excessWater / volume;
			neutralBallastLevel *= 0.9f;
			foreach (Hull hull2 in ballastHulls)
			{
				hull2.WaterVolume = hull2.Volume * neutralBallastLevel;
			}
		}

		// Token: 0x060021BC RID: 8636 RVA: 0x001528C4 File Offset: 0x00150AC4
		public void SetPrevTransform(Vector2 position)
		{
			this.prevPosition = position;
		}

		// Token: 0x060021BD RID: 8637 RVA: 0x001528D0 File Offset: 0x00150AD0
		public void SetPosition(Vector2 position, List<Submarine> checkd = null, bool forceUndockFromStaticSubmarines = true)
		{
			if (!MathUtils.IsValid(position))
			{
				return;
			}
			if (checkd == null)
			{
				checkd = new List<Submarine>();
			}
			if (checkd.Contains(this))
			{
				return;
			}
			checkd.Add(this);
			this.subBody.SetPosition(position);
			this.UpdateTransform(false);
			foreach (Submarine dockedSub in this.DockedTo)
			{
				DockingPort port;
				if (dockedSub.PhysicsBody.BodyType == BodyType.Static && forceUndockFromStaticSubmarines && this.ConnectedDockingPorts.TryGetValue(dockedSub, out port))
				{
					port.Undock(false);
				}
				else
				{
					Vector2? expectedLocation = Submarine.CalculateDockOffset(this, dockedSub);
					if (expectedLocation != null)
					{
						dockedSub.SetPosition(position + expectedLocation.Value, checkd, forceUndockFromStaticSubmarines);
						dockedSub.UpdateTransform(false);
					}
				}
			}
		}

		// Token: 0x060021BE RID: 8638 RVA: 0x001529A8 File Offset: 0x00150BA8
		public static Vector2? CalculateDockOffset(Submarine sub, Submarine dockedSub)
		{
			Item myPort = sub.ConnectedDockingPorts.ContainsKey(dockedSub) ? sub.ConnectedDockingPorts[dockedSub].Item : null;
			if (myPort == null)
			{
				return null;
			}
			Item theirPort = dockedSub.ConnectedDockingPorts.ContainsKey(sub) ? dockedSub.ConnectedDockingPorts[sub].Item : null;
			if (theirPort == null)
			{
				return null;
			}
			return new Vector2?(myPort.Position - sub.HiddenSubPosition - (theirPort.Position - dockedSub.HiddenSubPosition));
		}

		// Token: 0x060021BF RID: 8639 RVA: 0x00152A41 File Offset: 0x00150C41
		public void Translate(Vector2 amount)
		{
			if (amount == Vector2.Zero || !MathUtils.IsValid(amount))
			{
				return;
			}
			this.subBody.SetPosition(this.subBody.Position + amount);
		}

		// Token: 0x060021C0 RID: 8640 RVA: 0x00152A78 File Offset: 0x00150C78
		public static Submarine FindClosest(Vector2 worldPosition, bool ignoreOutposts = false, bool ignoreOutsideLevel = true, bool ignoreRespawnShuttle = false, CharacterTeamType? teamType = null)
		{
			Submarine closest = null;
			float closestDist = 0f;
			foreach (Submarine sub in Submarine.loaded)
			{
				if ((!ignoreOutposts || !sub.Info.IsOutpost) && (!ignoreOutsideLevel || Level.Loaded == null || !sub.IsAboveLevel) && (!ignoreRespawnShuttle || !sub.IsRespawnShuttle))
				{
					if (teamType != null)
					{
						CharacterTeamType teamID = sub.TeamID;
						CharacterTeamType? characterTeamType = teamType;
						if (!(teamID == characterTeamType.GetValueOrDefault() & characterTeamType != null))
						{
							continue;
						}
					}
					float dist = Vector2.DistanceSquared(worldPosition, sub.WorldPosition);
					if (closest == null || dist < closestDist)
					{
						closest = sub;
						closestDist = dist;
					}
				}
			}
			return closest;
		}

		// Token: 0x060021C1 RID: 8641 RVA: 0x00152B3C File Offset: 0x00150D3C
		public bool IsConnectedTo(Submarine otherSub)
		{
			return this == otherSub || this.GetConnectedSubs().Contains(otherSub);
		}

		// Token: 0x060021C2 RID: 8642 RVA: 0x00152B50 File Offset: 0x00150D50
		public List<Hull> GetHulls(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Hull>(alsoFromConnectedSubs, Hull.HullList);
		}

		// Token: 0x060021C3 RID: 8643 RVA: 0x00152B5E File Offset: 0x00150D5E
		public List<Gap> GetGaps(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Gap>(alsoFromConnectedSubs, Gap.GapList);
		}

		// Token: 0x060021C4 RID: 8644 RVA: 0x00152B6C File Offset: 0x00150D6C
		public List<Item> GetItems(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Item>(alsoFromConnectedSubs, Item.ItemList);
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x00152B7A File Offset: 0x00150D7A
		public List<WayPoint> GetWaypoints(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<WayPoint>(alsoFromConnectedSubs, WayPoint.WayPointList);
		}

		// Token: 0x060021C6 RID: 8646 RVA: 0x00152B88 File Offset: 0x00150D88
		public List<Structure> GetWalls(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Structure>(alsoFromConnectedSubs, Structure.WallList);
		}

		// Token: 0x060021C7 RID: 8647 RVA: 0x00152B98 File Offset: 0x00150D98
		public List<T> GetEntities<T>(bool includingConnectedSubs, List<T> list) where T : MapEntity
		{
			return list.FindAll((T e) => this.IsEntityFoundOnThisSub(e, includingConnectedSubs, false, false));
		}

		// Token: 0x060021C8 RID: 8648 RVA: 0x00152BCC File Offset: 0x00150DCC
		[return: TupleElementNames(new string[]
		{
			"container",
			"freeSlots"
		})]
		public List<ValueTuple<ItemContainer, int>> GetCargoContainers()
		{
			List<ValueTuple<ItemContainer, int>> containers = new List<ValueTuple<ItemContainer, int>>();
			IEnumerable<Submarine> connectedSubs = this.GetConnectedSubs().Where(delegate(Submarine sub)
			{
				SubmarineInfo info = sub.Info;
				SubmarineType? submarineType = (info != null) ? new SubmarineType?(info.Type) : null;
				SubmarineType type = this.Info.Type;
				return submarineType.GetValueOrDefault() == type & submarineType != null;
			});
			foreach (Item item in Item.ItemList.ToList<Item>())
			{
				if (connectedSubs.Contains(item.Submarine) && item.HasTag(Tags.CargoContainer) && !item.HasTag(Tags.DisallowCargo) && !item.NonInteractable && !item.IsHidden)
				{
					ItemContainer itemContainer = item.GetComponent<ItemContainer>();
					if (itemContainer != null)
					{
						int emptySlots = 0;
						for (int i = 0; i < itemContainer.Inventory.Capacity; i++)
						{
							if (itemContainer.Inventory.GetItemAt(i) == null)
							{
								emptySlots++;
							}
						}
						containers.Add(new ValueTuple<ItemContainer, int>(itemContainer, emptySlots));
					}
				}
			}
			return containers;
		}

		// Token: 0x060021C9 RID: 8649 RVA: 0x00152CC8 File Offset: 0x00150EC8
		public IEnumerable<T> GetEntities<T>(bool includingConnectedSubs, IEnumerable<T> list) where T : MapEntity
		{
			return from e in list
			where this.IsEntityFoundOnThisSub(e, includingConnectedSubs, false, false)
			select e;
		}

		// Token: 0x060021CA RID: 8650 RVA: 0x00152CFC File Offset: 0x00150EFC
		public bool IsEntityFoundOnThisSub(MapEntity entity, bool includingConnectedSubs, bool allowDifferentTeam = false, bool allowDifferentType = false)
		{
			if (entity == null)
			{
				return false;
			}
			if (entity.Submarine == this)
			{
				return true;
			}
			if (entity.Submarine == null)
			{
				return false;
			}
			if (includingConnectedSubs)
			{
				foreach (Submarine s in this.connectedSubs)
				{
					if (s == entity.Submarine && (allowDifferentTeam || entity.Submarine.TeamID == this.TeamID) && (allowDifferentType || entity.Submarine.Info.Type == this.Info.Type))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060021CB RID: 8651 RVA: 0x00152DAC File Offset: 0x00150FAC
		public static Submarine FindContainingInLocalCoordinates(Vector2 position, float inflate = 500f)
		{
			foreach (Submarine sub in Submarine.Loaded)
			{
				Rectangle subBorders = sub.Borders;
				subBorders.Location += MathUtils.ToPoint(sub.HiddenSubPosition) - new Point(0, sub.Borders.Height);
				subBorders.Inflate(inflate, inflate);
				if (subBorders.Contains(position))
				{
					return sub;
				}
			}
			return null;
		}

		// Token: 0x060021CC RID: 8652 RVA: 0x00152E4C File Offset: 0x0015104C
		public static Submarine FindContaining(Vector2 worldPosition, float inflate = 500f)
		{
			foreach (Submarine sub in Submarine.Loaded)
			{
				Rectangle worldBorders = sub.Borders;
				worldBorders.Location += sub.WorldPosition.ToPoint() - new Point(0, sub.Borders.Height);
				worldBorders.Inflate(inflate, inflate);
				if (worldBorders.Contains(worldPosition))
				{
					return sub;
				}
			}
			return null;
		}

		// Token: 0x060021CD RID: 8653 RVA: 0x00152EF0 File Offset: 0x001510F0
		public static Rectangle GetBorders(XElement submarineElement)
		{
			Vector4 bounds = new Vector4(float.MaxValue, float.MinValue, float.MinValue, float.MaxValue);
			foreach (XElement element in submarineElement.Elements())
			{
				if (element.Name == "Structure")
				{
					string name = element.GetAttributeString("name", "");
					Identifier identifier = element.GetAttributeIdentifier("identifier", "");
					StructurePrefab prefab = Structure.FindPrefab(name, identifier);
					if (prefab != null && prefab.Body)
					{
						Rectangle rect = element.GetAttributeRect("rect", Rectangle.Empty);
						bounds = new Vector4(Math.Min((float)rect.X, bounds.X), Math.Max((float)rect.Y, bounds.Y), Math.Max((float)rect.Right, bounds.Z), Math.Min((float)(rect.Y - rect.Height), bounds.W));
					}
				}
				else if (element.Name == "LinkedSubmarine")
				{
					Point dimensions = element.GetAttributePoint("dimensions", Point.Zero);
					Point pos = element.GetAttributeVector2("pos", Vector2.Zero).ToPoint();
					bounds = new Vector4(Math.Min((float)(pos.X - dimensions.X / 2), bounds.X), Math.Max((float)(pos.Y + dimensions.Y / 2), bounds.Y), Math.Max((float)(pos.X + dimensions.X / 2), bounds.Z), Math.Min((float)(pos.Y - dimensions.Y / 2), bounds.W));
				}
			}
			if (bounds.X == 3.4028235E+38f || bounds.Y == -3.4028235E+38f || bounds.Z == -3.4028235E+38f || bounds.W == 3.4028235E+38f)
			{
				return Rectangle.Empty;
			}
			return new Rectangle((int)bounds.X, (int)bounds.Y, (int)(bounds.Z - bounds.X), (int)(bounds.Y - bounds.W));
		}

		// Token: 0x060021CE RID: 8654 RVA: 0x00153158 File Offset: 0x00151358
		public Submarine(SubmarineInfo info, bool showErrorMessages = true, Func<Submarine, List<MapEntity>> loadEntities = null, IdRemap linkedRemap = null) : base(null, 0)
		{
			Stopwatch sw = Stopwatch.StartNew();
			this.connectedSubs = new HashSet<Submarine>(2)
			{
				this
			};
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Submarine");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			this.upgradeEventIdentifier = new Identifier(defaultInterpolatedStringHandler.ToStringAndClear());
			this.Loading = true;
			GameMain.World.Enabled = false;
			try
			{
				Submarine.loaded.Add(this);
				this.Info = new SubmarineInfo(info);
				this.ConnectedDockingPorts = new Dictionary<Submarine, DockingPort>();
				this.HiddenSubPosition = Submarine.HiddenSubStartPosition;
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.LevelData : null) != null)
				{
					this.HiddenSubPosition += Vector2.UnitY * (float)GameMain.GameSession.LevelData.Size.Y;
				}
				for (int i = 0; i < Submarine.loaded.Count; i++)
				{
					Submarine sub = Submarine.loaded[i];
					this.HiddenSubPosition = new Vector2(-this.HiddenSubPosition.X, this.HiddenSubPosition.Y + (float)sub.Borders.Height + 5000f);
				}
				this.IdOffset = IdRemap.DetermineNewOffset();
				List<MapEntity> newEntities = new List<MapEntity>();
				if (loadEntities == null)
				{
					if (this.Info.SubmarineElement != null)
					{
						newEntities = MapEntity.LoadAll(this, this.Info.SubmarineElement, this.Info.FilePath, (int)this.IdOffset);
					}
				}
				else
				{
					newEntities = loadEntities(this);
					newEntities.ForEach(delegate(MapEntity me)
					{
						me.Submarine = this;
					});
				}
				if (newEntities != null)
				{
					foreach (MapEntity e in newEntities)
					{
						if (linkedRemap != null)
						{
							e.ResolveLinks(linkedRemap);
						}
						e.unresolvedLinkedToID = null;
					}
				}
				Vector2 center = Vector2.Zero;
				List<Hull> matchingHulls = Hull.HullList.FindAll((Hull h) => h.Submarine == this);
				if (matchingHulls.Any<Hull>())
				{
					Vector2 topLeft = new Vector2((float)matchingHulls[0].Rect.X, (float)matchingHulls[0].Rect.Y);
					Vector2 bottomRight = new Vector2((float)matchingHulls[0].Rect.X, (float)matchingHulls[0].Rect.Y);
					foreach (Hull hull in matchingHulls)
					{
						if ((float)hull.Rect.X < topLeft.X)
						{
							topLeft.X = (float)hull.Rect.X;
						}
						if ((float)hull.Rect.Y > topLeft.Y)
						{
							topLeft.Y = (float)hull.Rect.Y;
						}
						if ((float)hull.Rect.Right > bottomRight.X)
						{
							bottomRight.X = (float)hull.Rect.Right;
						}
						if ((float)(hull.Rect.Y - hull.Rect.Height) < bottomRight.Y)
						{
							bottomRight.Y = (float)(hull.Rect.Y - hull.Rect.Height);
						}
					}
					center = (topLeft + bottomRight) / 2f;
					center.X -= center.X % Submarine.GridSize.X;
					center.Y -= center.Y % Submarine.GridSize.Y;
					Submarine.RepositionEntities(-center, from me in MapEntity.MapEntityList
					where me.Submarine == this
					select me);
				}
				this.subBody = new SubmarineBody(this, showErrorMessages);
				Vector2 pos = ConvertUnits.ToSimUnits(this.HiddenSubPosition);
				this.subBody.Body.FarseerBody.SetTransformIgnoreContacts(ref pos, 0f);
				if (info.IsOutpost)
				{
					this.ShowSonarMarker = false;
					this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
					this.TeamID = CharacterTeamType.FriendlyNPC;
					foreach (Submarine dockedSub in this.DockedTo)
					{
						dockedSub.TeamID = CharacterTeamType.FriendlyNPC;
					}
					bool flag;
					if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.DestructibleOutposts)
					{
						OutpostGenerationParams outpostGenerationParams = info.OutpostGenerationParams;
						flag = (outpostGenerationParams == null || !outpostGenerationParams.AlwaysDestructible);
					}
					else
					{
						flag = false;
					}
					bool indestructible = flag;
					using (List<MapEntity>.Enumerator enumerator4 = MapEntity.MapEntityList.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							MapEntity me3 = enumerator4.Current;
							if (me3.Submarine == this)
							{
								Item item = me3 as Item;
								if (item != null)
								{
									item.AllowStealing = true;
									if (info.OutpostGenerationParams != null)
									{
										item.SpawnedInCurrentOutpost = true;
										Item item3 = item;
										bool allowStealing;
										if (!info.OutpostGenerationParams.AllowStealing)
										{
											Item rootContainer = item.RootContainer;
											if (rootContainer != null)
											{
												ItemPrefab prefab = rootContainer.Prefab;
												if (prefab != null)
												{
													allowStealing = prefab.AllowStealingContainedItems;
													goto IL_543;
												}
											}
											allowStealing = false;
										}
										else
										{
											allowStealing = true;
										}
										IL_543:
										item3.AllowStealing = allowStealing;
									}
									if (item.GetComponent<Repairable>() != null && indestructible)
									{
										item.Indestructible = true;
									}
									using (List<ItemComponent>.Enumerator enumerator5 = item.Components.GetEnumerator())
									{
										while (enumerator5.MoveNext())
										{
											ItemComponent ic = enumerator5.Current;
											ConnectionPanel connectionPanel = ic as ConnectionPanel;
											if (connectionPanel != null)
											{
												if (info.OutpostGenerationParams != null && !info.OutpostGenerationParams.AlwaysRewireable)
												{
													connectionPanel.Locked = true;
												}
											}
											else
											{
												Holdable holdable = ic as Holdable;
												if (holdable != null && holdable.Attached && item.GetComponent<LevelResource>() == null)
												{
													GameSession gameSession2 = GameMain.GameSession;
													if (!(((gameSession2 != null) ? gameSession2.GameMode : null) is TutorialMode))
													{
														holdable.CanBePicked = false;
														holdable.CanBeSelected = false;
													}
												}
											}
										}
										continue;
									}
								}
								Structure structure = me3 as Structure;
								if (structure != null && structure.Prefab.IndestructibleInOutposts && indestructible)
								{
									structure.Indestructible = true;
								}
							}
						}
						goto IL_66C;
					}
				}
				if (info.IsRuin)
				{
					this.ShowSonarMarker = false;
					this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
				}
				IL_66C:
				if (this.entityGrid != null)
				{
					Hull.EntityGrids.Remove(this.entityGrid);
					this.entityGrid = null;
				}
				this.entityGrid = Hull.GenerateEntityGrid(this);
				for (int j = 0; j < MapEntity.MapEntityList.Count; j++)
				{
					if (MapEntity.MapEntityList[j].Submarine == this)
					{
						MapEntity.MapEntityList[j].Move(this.HiddenSubPosition, true);
					}
				}
				this.Loading = false;
				MapEntity.MapLoaded(newEntities, true);
				foreach (MapEntity me2 in MapEntity.MapEntityList)
				{
					if (me2.Submarine == this)
					{
						LinkedSubmarine linkedSub = me2 as LinkedSubmarine;
						if (linkedSub != null)
						{
							linkedSub.LinkDummyToMainSubmarine();
						}
						else
						{
							WayPoint wayPoint = me2 as WayPoint;
							if (wayPoint != null && wayPoint.SpawnType.HasFlag(SpawnType.ExitPoint))
							{
								this.exitPoints.Add(wayPoint);
							}
						}
					}
				}
				foreach (Hull hull2 in matchingHulls)
				{
					if (string.IsNullOrEmpty(hull2.RoomName))
					{
						hull2.RoomName = hull2.CreateRoomName();
					}
				}
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					foreach (Identifier layer in this.Info.LayersHiddenByDefault)
					{
						this.SetLayerEnabled(layer, false, false);
					}
				}
				GameSession gameSession3 = GameMain.GameSession;
				if (gameSession3 != null)
				{
					CampaignMode campaign = gameSession3.Campaign;
					if (campaign != null)
					{
						UpgradeManager upgradeManager = campaign.UpgradeManager;
						if (upgradeManager != null)
						{
							upgradeManager.OnUpgradesChanged.Register(this.upgradeEventIdentifier, delegate(UpgradeManager _)
							{
								this.ResetCrushDepth();
							});
						}
					}
				}
				GameMain.LightManager.OnMapLoaded();
				ConvexHull.RecalculateAll(this);
				if (showErrorMessages && !string.IsNullOrEmpty(this.Info.FilePath) && Screen.Selected != GameMain.SubEditorScreen && (this.Info.GameVersion == null || this.Info.GameVersion < new Version("0.8.9.0")))
				{
					DebugConsole.ThrowError("The submarine \"" + this.Info.Name + "\" was made using an older version of the Barotrauma that used a different formula to calculate the lighting. The game automatically adjusts the lights make them look better with the new formula, but it's recommended to open the submarine in the submarine editor and make sure everything looks right after the automatic conversion.", null, null, false, false);
					foreach (Item item2 in Item.ItemList)
					{
						if (item2.Submarine == this && item2.ParentInventory == null && item2.body == null)
						{
							foreach (LightComponent light in item2.GetComponents<LightComponent>())
							{
								light.LightColor = new Color(light.LightColor, (float)light.LightColor.A / 255f * 0.5f);
							}
						}
					}
				}
				this.GenerateOutdoorNodes();
			}
			finally
			{
				this.Loading = false;
				GameMain.World.Enabled = true;
			}
			sw.Stop();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Loading ");
			SubmarineInfo info2 = this.Info;
			defaultInterpolatedStringHandler2.AppendFormatted(((info2 != null) ? info2.Name : null) ?? "unknown");
			defaultInterpolatedStringHandler2.AppendLiteral(" took ");
			defaultInterpolatedStringHandler2.AppendFormatted<long>(sw.ElapsedMilliseconds);
			defaultInterpolatedStringHandler2.AppendLiteral(" ms.");
			string debugMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
			DebugConsole.Log(debugMsg);
		}

		// Token: 0x060021CF RID: 8655 RVA: 0x00153C88 File Offset: 0x00151E88
		protected override ushort DetermineID(ushort id, Submarine submarine)
		{
			return (ushort)(65532 - Submarine.loaded.Count);
		}

		// Token: 0x060021D0 RID: 8656 RVA: 0x00153C9B File Offset: 0x00151E9B
		public static Submarine Load(SubmarineInfo info, bool unloadPrevious, IdRemap linkedRemap = null)
		{
			if (unloadPrevious)
			{
				Submarine.Unload();
			}
			return new Submarine(info, false, null, linkedRemap);
		}

		// Token: 0x060021D1 RID: 8657 RVA: 0x00153CAE File Offset: 0x00151EAE
		private void ResetCrushDepth()
		{
			this.realWorldCrushDepth = null;
		}

		// Token: 0x060021D2 RID: 8658 RVA: 0x00153CBC File Offset: 0x00151EBC
		public void SetCrushDepth(float realWorldCrushDepth)
		{
			foreach (Structure structure in Structure.WallList)
			{
				if (structure.Submarine == this && structure.HasBody && !structure.Indestructible)
				{
					structure.CrushDepth = realWorldCrushDepth;
				}
			}
			this.realWorldCrushDepth = new float?(realWorldCrushDepth);
		}

		// Token: 0x060021D3 RID: 8659 RVA: 0x00153D34 File Offset: 0x00151F34
		public static void RepositionEntities(Vector2 moveAmount, IEnumerable<MapEntity> entities)
		{
			if (moveAmount.LengthSquared() < 1E-05f)
			{
				return;
			}
			foreach (MapEntity entity in entities)
			{
				Item item = entity as Item;
				if (item != null)
				{
					Wire component = item.GetComponent<Wire>();
					if (component != null)
					{
						component.MoveNodes(moveAmount);
					}
				}
				entity.Move(moveAmount, true);
			}
		}

		// Token: 0x060021D4 RID: 8660 RVA: 0x00153DA8 File Offset: 0x00151FA8
		public bool CheckFuel()
		{
			float fuel = (from i in this.GetItems(true)
			where i.HasTag(Tags.ReactorFuel)
			select i).Sum((Item i) => i.Condition);
			this.Info.LowFuel = (fuel < 200f);
			return !this.Info.LowFuel;
		}

		// Token: 0x060021D5 RID: 8661 RVA: 0x00153E28 File Offset: 0x00152028
		public void SaveToXElement(XElement element)
		{
			element.Add(new XAttribute("name", this.Info.Name));
			element.Add(new XAttribute("description", this.Info.Description ?? ""));
			element.Add(new XAttribute("checkval", Rand.Int(int.MaxValue, Rand.RandSync.Unsynced)));
			element.Add(new XAttribute("price", this.Info.Price));
			element.Add(new XAttribute("tier", this.Info.Tier));
			element.Add(new XAttribute("initialsuppliesspawned", this.Info.InitialSuppliesSpawned));
			element.Add(new XAttribute("noitems", this.Info.NoItems));
			element.Add(new XAttribute("lowfuel", !this.CheckFuel()));
			element.Add(new XAttribute("type", this.Info.Type.ToString()));
			element.Add(new XAttribute("ismanuallyoutfitted", this.Info.IsManuallyOutfitted));
			if (this.Info.IsPlayer && !this.Info.HasTag(SubmarineTag.Shuttle))
			{
				element.Add(new XAttribute("class", this.Info.SubmarineClass.ToString()));
			}
			element.Add(new XAttribute("tags", this.Info.Tags.ToString()));
			element.Add(new XAttribute("outposttags", this.Info.OutpostTags.ConvertToString(",")));
			element.Add(new XAttribute("triggeroutpostmissionevents", this.Info.TriggerOutpostMissionEvents.ConvertToString(",")));
			element.Add(new XAttribute("gameversion", GameMain.Version.ToString()));
			Rectangle dimensions = this.VisibleBorders;
			element.Add(new XAttribute("dimensions", XMLExtensions.Vector2ToString(dimensions.Size.ToVector2())));
			List<ValueTuple<ItemContainer, int>> cargoContainers = this.GetCargoContainers();
			int cargoCapacity = cargoContainers.Sum(([TupleElementNames(new string[]
			{
				"container",
				"freeSlots"
			})] ValueTuple<ItemContainer, int> c) => c.Item1.Capacity);
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				LinkedSubmarine linkedSub = me as LinkedSubmarine;
				if (linkedSub != null && linkedSub.Submarine == this)
				{
					cargoCapacity += linkedSub.CargoCapacity;
				}
			}
			element.Add(new XAttribute("cargocapacity", cargoCapacity));
			element.Add(new XAttribute("recommendedcrewsizemin", this.Info.RecommendedCrewSizeMin));
			element.Add(new XAttribute("recommendedcrewsizemax", this.Info.RecommendedCrewSizeMax));
			element.Add(new XAttribute("recommendedcrewexperience", this.Info.RecommendedCrewExperience.ToString()));
			element.Add(new XAttribute("requiredcontentpackages", string.Join(", ", this.Info.RequiredContentPackages)));
			if (this.Info.LayersHiddenByDefault.Any<Identifier>())
			{
				element.Add(new XAttribute("layerhiddenbydefault", string.Join<Identifier>(", ", this.Info.LayersHiddenByDefault)));
			}
			if (this.Info.WreckInfo != null)
			{
				bool hasThalamus = false;
				IEnumerable<Identifier> wreckAiEntities = from p in WreckAIConfig.Prefabs
				select p.Entity;
				IEnumerable<ItemPrefab> prefabsOnSub = (from i in this.GetItems(true)
				select i.Prefab).Distinct<ItemPrefab>();
				foreach (ItemPrefab prefab in prefabsOnSub)
				{
					foreach (Identifier entity in wreckAiEntities)
					{
						if (WreckAI.IsThalamus(prefab, entity))
						{
							hasThalamus = true;
							break;
						}
					}
					if (hasThalamus)
					{
						break;
					}
				}
				element.Add(new XAttribute("WreckContainsThalamus", hasThalamus ? WreckInfo.HasThalamus.Yes : WreckInfo.HasThalamus.No));
			}
			if (this.Info.Type == SubmarineType.OutpostModule)
			{
				OutpostModuleInfo outpostModuleInfo = this.Info.OutpostModuleInfo;
				if (outpostModuleInfo != null)
				{
					outpostModuleInfo.Save(element);
				}
			}
			ExtraSubmarineInfo extraSubInfo = this.Info.GetExtraSubmarineInfo;
			if (extraSubInfo != null)
			{
				extraSubInfo.Save(element);
			}
			foreach (Item item in Item.ItemList)
			{
				ItemPrefab pendingItemSwap = item.PendingItemSwap;
				if (((pendingItemSwap != null) ? pendingItemSwap.SwappableItem : null) != null)
				{
					Dictionary<Item, ItemPrefab> connectedItemsToSwap = item.GetConnectedItemsToSwap(item.PendingItemSwap.SwappableItem);
					foreach (KeyValuePair<Item, ItemPrefab> kvp in connectedItemsToSwap)
					{
						Item itemToSwap = kvp.Key;
						ItemPrefab swapTo = kvp.Value;
						itemToSwap.PurchasedNewSwap = item.PurchasedNewSwap;
						if (itemToSwap.Prefab != swapTo)
						{
							itemToSwap.PendingItemSwap = swapTo;
						}
					}
				}
			}
			Dictionary<int, MapEntity> savedEntities = new Dictionary<int, MapEntity>();
			foreach (MapEntity e2 in from e in MapEntity.MapEntityList
			orderby e.ID
			select e)
			{
				if (e2.ShouldBeSaved)
				{
					MapEntity duplicateEntity;
					if (e2.Removed)
					{
						string identifier = "Submarine.SaveToXElement:Removed" + e2.Name;
						GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Attempted to save a removed entity (\"");
						defaultInterpolatedStringHandler.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\"). Duplicate ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(savedEntities.ContainsKey((int)e2.ID));
						GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler.ToStringAndClear());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(146, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Error while saving the submarine. Attempted to save a removed entity (\"");
						defaultInterpolatedStringHandler2.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted<ushort>(e2.ID);
						defaultInterpolatedStringHandler2.AppendLiteral(")\"). The entity will not be saved to avoid corrupting the submarine file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					}
					else if (savedEntities.TryGetValue((int)e2.ID, out duplicateEntity))
					{
						string identifier2 = "Submarine.SaveToXElement:DuplicateId" + e2.Name;
						GameAnalyticsManager.ErrorSeverity errorSeverity2 = GameAnalyticsManager.ErrorSeverity.Error;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(53, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Attempted to save an entity with a duplicate ID (");
						defaultInterpolatedStringHandler3.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler3.AppendLiteral(", ");
						defaultInterpolatedStringHandler3.AppendFormatted(duplicateEntity.Name);
						defaultInterpolatedStringHandler3.AppendLiteral(").");
						GameAnalyticsManager.AddErrorEventOnce(identifier2, errorSeverity2, defaultInterpolatedStringHandler3.ToStringAndClear());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(142, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("Error while saving the submarine. The entity \"");
						defaultInterpolatedStringHandler4.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler4.AppendLiteral("\" has the same ID as \"");
						defaultInterpolatedStringHandler4.AppendFormatted(duplicateEntity.Name);
						defaultInterpolatedStringHandler4.AppendLiteral("\" (");
						defaultInterpolatedStringHandler4.AppendFormatted<ushort>(e2.ID);
						defaultInterpolatedStringHandler4.AppendLiteral("). The entity will not be saved to avoid corrupting the submarine file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						Item item2 = e2 as Item;
						if (item2 != null)
						{
							if (item2.FindParentInventory((Inventory inv) => inv is CharacterInventory) != null)
							{
								continue;
							}
							if (Screen.Selected == GameMain.SubEditorScreen)
							{
								e2.Submarine = this;
							}
							if (e2.Submarine != this)
							{
								continue;
							}
							if (item2.RootContainer != null && item2.RootContainer.Submarine != this)
							{
								continue;
							}
						}
						else if (e2.Submarine != this)
						{
							continue;
						}
						e2.Save(element);
						savedEntities.Add((int)e2.ID, e2);
					}
				}
			}
			this.Info.CheckSubsLeftBehind(element);
		}

		// Token: 0x060021D6 RID: 8662 RVA: 0x001547A0 File Offset: 0x001529A0
		public bool TrySaveAs(string filePath, MemoryStream previewImage = null)
		{
			SubmarineInfo newInfo = new SubmarineInfo(this)
			{
				Type = this.Info.Type,
				FilePath = filePath,
				OutpostModuleInfo = ((this.Info.OutpostModuleInfo != null) ? new OutpostModuleInfo(this.Info.OutpostModuleInfo) : null),
				BeaconStationInfo = ((this.Info.BeaconStationInfo != null) ? new BeaconStationInfo(this.Info.BeaconStationInfo) : null),
				EnemySubmarineInfo = ((this.Info.EnemySubmarineInfo != null) ? new EnemySubmarineInfo(this.Info.EnemySubmarineInfo) : null),
				WreckInfo = ((this.Info.WreckInfo != null) ? new WreckInfo(this.Info.WreckInfo) : null),
				Name = Barotrauma.IO.Path.GetFileNameWithoutExtension(filePath)
			};
			this.Info.PreviewImage = null;
			this.Info.Dispose();
			this.Info = newInfo;
			try
			{
				newInfo.SaveAs(filePath, previewImage);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving submarine \"" + filePath + "\" failed!", e, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x17000936 RID: 2358
		// (get) Token: 0x060021D7 RID: 8663 RVA: 0x001548CC File Offset: 0x00152ACC
		// (set) Token: 0x060021D8 RID: 8664 RVA: 0x001548D3 File Offset: 0x00152AD3
		public static bool Unloading { get; private set; }

		// Token: 0x060021D9 RID: 8665 RVA: 0x001548DC File Offset: 0x00152ADC
		public static void Unload()
		{
			if (Submarine.Unloading)
			{
				DebugConsole.AddWarning("Called Unload when already unloading.", null);
				return;
			}
			Submarine.Unloading = true;
			try
			{
				RoundSound.RemoveAllRoundSounds();
				LightManager lightManager = GameMain.LightManager;
				if (lightManager != null)
				{
					lightManager.ClearLights();
				}
				List<Submarine> _loaded = new List<Submarine>(Submarine.loaded);
				foreach (Submarine sub in _loaded)
				{
					sub.Remove();
					if (sub.Info.LazyLoad)
					{
						sub.Info.UnloadSubmarineElement();
					}
				}
				Submarine.loaded.Clear();
				Submarine.visibleEntities = null;
				if (GameMain.GameScreen.Cam != null)
				{
					GameMain.GameScreen.Cam.TargetPos = Vector2.Zero;
				}
				Entity.RemoveAll();
				if (Item.ItemList.Count > 0)
				{
					List<Item> items = new List<Item>(Item.ItemList);
					foreach (Item item in items)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error while unloading submarines - item \"",
							item.Name,
							"\" (ID:",
							item.ID.ToString(),
							") not removed"
						}), null, null, false, false);
						try
						{
							item.Remove();
						}
						catch (Exception e)
						{
							DebugConsole.ThrowError("Error while removing \"" + item.Name + "\"!", e, null, false, false);
						}
					}
					Item.ItemList.Clear();
				}
				Ragdoll.RemoveAll();
				PhysicsBody.RemoveAll();
				StatusEffect.StopAll();
				GameMain.World = null;
				Powered.Grids.Clear();
				Powered.ChangedConnections.Clear();
				GC.Collect();
			}
			finally
			{
				Submarine.Unloading = false;
			}
		}

		// Token: 0x060021DA RID: 8666 RVA: 0x00154B00 File Offset: 0x00152D00
		public override void Remove()
		{
			base.Remove();
			SubmarineBody submarineBody = this.subBody;
			if (submarineBody != null)
			{
				submarineBody.Remove();
			}
			this.subBody = null;
			List<PathNode> list = this.outdoorNodes;
			if (list != null)
			{
				list.Clear();
			}
			this.outdoorNodes = null;
			this.obstructedNodes.Clear();
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign != null)
				{
					UpgradeManager upgradeManager = campaign.UpgradeManager;
					if (upgradeManager != null)
					{
						NamedEvent<UpgradeManager> onUpgradesChanged = upgradeManager.OnUpgradesChanged;
						if (onUpgradesChanged != null)
						{
							onUpgradesChanged.TryDeregister(this.upgradeEventIdentifier);
						}
					}
				}
			}
			if (this.entityGrid != null)
			{
				Hull.EntityGrids.Remove(this.entityGrid);
				this.entityGrid = null;
			}
			Submarine.visibleEntities = null;
			Submarine.bodyDist.Clear();
			Submarine.bodies.Clear();
			if (Submarine.MainSub == this)
			{
				Submarine.MainSub = null;
			}
			if (Submarine.MainSubs[1] == this)
			{
				Submarine.MainSubs[1] = null;
			}
			Dictionary<Submarine, DockingPort> connectedDockingPorts = this.ConnectedDockingPorts;
			if (connectedDockingPorts != null)
			{
				connectedDockingPorts.Clear();
			}
			Powered.ChangedConnections.Clear();
			Powered.Grids.Clear();
			Submarine.loaded.Remove(this);
		}

		// Token: 0x060021DB RID: 8667 RVA: 0x00154C10 File Offset: 0x00152E10
		public void Dispose()
		{
			this.Remove();
		}

		// Token: 0x17000937 RID: 2359
		// (get) Token: 0x060021DC RID: 8668 RVA: 0x00154C18 File Offset: 0x00152E18
		private List<PathNode> OutdoorNodes
		{
			get
			{
				if (this.outdoorNodes == null)
				{
					this.GenerateOutdoorNodes();
				}
				return this.outdoorNodes;
			}
		}

		// Token: 0x060021DD RID: 8669 RVA: 0x00154C30 File Offset: 0x00152E30
		private void GenerateOutdoorNodes()
		{
			List<WayPoint> waypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Path && wp.Submarine == this && wp.CurrentHull == null);
			this.outdoorNodes = PathNode.GenerateNodes(waypoints, false);
		}

		// Token: 0x060021DE RID: 8670 RVA: 0x00154C64 File Offset: 0x00152E64
		public void DisableObstructedWayPoints()
		{
			foreach (PathNode node in this.OutdoorNodes)
			{
				if (node != null && node.Waypoint != null)
				{
					WayPoint wp = node.Waypoint;
					if (!wp.IsObstructed)
					{
						foreach (PathNode connection in node.connections)
						{
							WayPoint connectedWp = connection.Waypoint;
							if (!connectedWp.IsObstructed)
							{
								Vector2 start = ConvertUnits.ToSimUnits(wp.WorldPosition);
								Vector2 end = ConvertUnits.ToSimUnits(connectedWp.WorldPosition);
								Body body = Submarine.PickBody(start, end, null, new Category?(Category.Cat8), true, null, false);
								if (body != null)
								{
									connectedWp.IsObstructed = true;
									wp.IsObstructed = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060021DF RID: 8671 RVA: 0x00154D70 File Offset: 0x00152F70
		public void DisableObstructedWayPoints(Submarine otherSub)
		{
			if (otherSub == null)
			{
				return;
			}
			if (otherSub == this)
			{
				return;
			}
			foreach (PathNode node in this.OutdoorNodes)
			{
				if (node != null && node.Waypoint != null)
				{
					WayPoint wp = node.Waypoint;
					if (!wp.IsObstructed)
					{
						foreach (PathNode connection in node.connections)
						{
							WayPoint connectedWp = connection.Waypoint;
							if (!connectedWp.IsObstructed && connectedWp.Ladders == null)
							{
								Hull h = wp.CurrentHull;
								bool isObstructed = h != null && h.Submarine != this;
								if (!isObstructed)
								{
									Vector2 start = ConvertUnits.ToSimUnits(wp.WorldPosition) - otherSub.SimPosition;
									Vector2 end = ConvertUnits.ToSimUnits(connectedWp.WorldPosition) - otherSub.SimPosition;
									Body body = Submarine.PickBody(start, end, null, new Category?(Category.Cat1), true, null, true);
									if (body != null)
									{
										Structure wall = body.UserData as Structure;
										if ((wall != null && !wall.IsPlatform) || (body.UserData is Item && body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1)))
										{
											isObstructed = true;
										}
									}
								}
								if (isObstructed)
								{
									connectedWp.IsObstructed = true;
									wp.IsObstructed = true;
									HashSet<PathNode> nodes;
									if (!this.obstructedNodes.TryGetValue(otherSub, out nodes))
									{
										nodes = new HashSet<PathNode>();
										this.obstructedNodes.Add(otherSub, nodes);
									}
									nodes.Add(node);
									nodes.Add(connection);
									break;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060021E0 RID: 8672 RVA: 0x00154F78 File Offset: 0x00153178
		public void EnableObstructedWaypoints(Submarine otherSub)
		{
			HashSet<PathNode> nodes;
			if (this.obstructedNodes.TryGetValue(otherSub, out nodes))
			{
				nodes.ForEach(delegate(PathNode n)
				{
					n.Waypoint.IsObstructed = false;
				});
				nodes.Clear();
				this.obstructedNodes.Remove(otherSub);
			}
		}

		// Token: 0x060021E1 RID: 8673 RVA: 0x00154FCD File Offset: 0x001531CD
		public void RefreshOutdoorNodes()
		{
			this.OutdoorNodes.ForEach(delegate(PathNode n)
			{
				if (n != null)
				{
					WayPoint waypoint = n.Waypoint;
					if (waypoint == null)
					{
						return;
					}
					waypoint.FindHull();
				}
			});
		}

		// Token: 0x060021E2 RID: 8674 RVA: 0x00154FFC File Offset: 0x001531FC
		public Item FindContainerFor(Item item, bool onlyPrimary, bool checkTransferConditions = false, bool allowConnectedSubs = false)
		{
			HashSet<Submarine> connectedSubs = (from s in this.GetConnectedSubs()
			where s.Info.Type == SubmarineType.Player
			select s).ToHashSet<Submarine>();
			Item selectedContainer = null;
			foreach (Item potentialContainer in Item.ItemList)
			{
				if (!potentialContainer.Removed && !potentialContainer.NonInteractable && !potentialContainer.IsHidden)
				{
					if (allowConnectedSubs)
					{
						if (!connectedSubs.Contains(potentialContainer.Submarine))
						{
							continue;
						}
					}
					else if (potentialContainer.Submarine != this)
					{
						continue;
					}
					if (potentialContainer != item && potentialContainer.Condition > 0f && potentialContainer.OwnInventory != null && potentialContainer.GetRootInventoryOwner() == potentialContainer)
					{
						ItemContainer container = potentialContainer.GetComponent<ItemContainer>();
						bool flag;
						bool isPreferencesDefined;
						bool isSecondary;
						if (container != null && potentialContainer.OwnInventory.CanBePut(item) && container.ShouldBeContained(item, out flag) && item.Prefab.IsContainerPreferred(item, container, out isPreferencesDefined, out isSecondary, false, checkTransferConditions) && isPreferencesDefined && (!onlyPrimary || !isSecondary))
						{
							if (potentialContainer.Submarine == this && !isSecondary)
							{
								return potentialContainer;
							}
							selectedContainer = potentialContainer;
						}
					}
				}
			}
			return selectedContainer;
		}

		// Token: 0x060021E3 RID: 8675 RVA: 0x00155140 File Offset: 0x00153340
		public static Vector2 GetRelativeSimPosition(ISpatialEntity from, ISpatialEntity to, Vector2? targetWorldPos = null)
		{
			if (targetWorldPos == null)
			{
				return Submarine.GetRelativeSimPosition(to.SimPosition, from.Submarine, to.Submarine);
			}
			return Submarine.GetRelativeSimPositionFromWorldPosition(targetWorldPos.Value, from.Submarine, to.Submarine);
		}

		// Token: 0x060021E4 RID: 8676 RVA: 0x0015517C File Offset: 0x0015337C
		public static Vector2 GetRelativeSimPositionFromWorldPosition(Vector2 targetWorldPos, Submarine fromSub, Submarine toSub)
		{
			Vector2 worldPos = targetWorldPos;
			if (toSub != null)
			{
				worldPos -= toSub.Position;
			}
			return Submarine.GetRelativeSimPosition(ConvertUnits.ToSimUnits(worldPos), fromSub, toSub);
		}

		// Token: 0x060021E5 RID: 8677 RVA: 0x001551A8 File Offset: 0x001533A8
		public static Vector2 GetRelativeSimPosition(Vector2 targetSimPos, Submarine fromSub, Submarine toSub)
		{
			Vector2 targetPos = targetSimPos;
			if (fromSub == null && toSub != null)
			{
				targetPos += toSub.SimPosition;
			}
			else if (fromSub != null && toSub == null)
			{
				targetPos -= fromSub.SimPosition;
			}
			else if (fromSub != toSub && fromSub != null && toSub != null)
			{
				Vector2 diff = fromSub.SimPosition - toSub.SimPosition;
				targetPos -= diff;
			}
			return targetPos;
		}

		// Token: 0x060021E7 RID: 8679 RVA: 0x00155284 File Offset: 0x00153484
		[CompilerGenerated]
		internal static bool <CheckForErrors>g__IsWarningSuppressed|19_8(SubEditorScreen.WarningType type)
		{
			return SubEditorScreen.SuppressedWarnings.Contains(type);
		}

		// Token: 0x060021EA RID: 8682 RVA: 0x001552A8 File Offset: 0x001534A8
		[CompilerGenerated]
		internal static Vector2 <FindSpawnPos>g__GetHorizontalLimits|159_0(Vector2 spawnPos, float maxHorizontalMoveAmount, float minHeight, int verticalMoveDir, int padding, ref Submarine.<>c__DisplayClass159_0 A_5)
		{
			Vector2 refPos = spawnPos - Vector2.UnitY * minHeight * 0.5f * (float)Math.Sign(verticalMoveDir);
			float minX = float.MinValue;
			float maxX = float.MaxValue;
			foreach (VoronoiCell cell in Level.Loaded.GetAllCells())
			{
				foreach (GraphEdge e in cell.Edges)
				{
					if ((e.Point1.Y >= refPos.Y - minHeight * 0.5f || e.Point2.Y >= refPos.Y - minHeight * 0.5f) && (e.Point1.Y <= refPos.Y + minHeight * 0.5f || e.Point2.Y <= refPos.Y + minHeight * 0.5f))
					{
						if (cell.Site.Coord.X < (double)refPos.X)
						{
							minX = Math.Max(minX, Math.Max(e.Point1.X, e.Point2.X));
						}
						else
						{
							maxX = Math.Min(maxX, Math.Min(e.Point1.X, e.Point2.X));
						}
					}
				}
			}
			foreach (Ruin ruin in Level.Loaded.Ruins)
			{
				if (Math.Abs((float)ruin.Area.Center.Y - refPos.Y) <= (minHeight + (float)ruin.Area.Height) * 0.5f)
				{
					if ((float)ruin.Area.Center.X < refPos.X)
					{
						minX = Math.Max(minX, (float)(ruin.Area.Right + padding));
					}
					else
					{
						maxX = Math.Min(maxX, (float)(ruin.Area.X - padding));
					}
				}
			}
			minX += A_5.subDockingPortOffset;
			maxX += A_5.subDockingPortOffset;
			return new Vector2(Math.Max(Math.Max(minX, spawnPos.X - maxHorizontalMoveAmount - (float)padding), 0f), Math.Min(Math.Min(maxX, spawnPos.X + maxHorizontalMoveAmount + (float)padding), (float)Level.Loaded.Size.X));
		}

		// Token: 0x060021EB RID: 8683 RVA: 0x001555A0 File Offset: 0x001537A0
		[CompilerGenerated]
		internal static Vector2 <FindSpawnPos>g__ClampToHorizontalLimits|159_1(Vector2 spawnPos, Vector2 limits, ref Submarine.<>c__DisplayClass159_0 A_2)
		{
			if (limits.X >= 0f || limits.Y <= (float)Level.Loaded.Size.X)
			{
				if (limits.X < 0f)
				{
					spawnPos.X = limits.Y - (float)A_2.minWidth * 0.5f - 100f + A_2.subDockingPortOffset;
				}
				else if (limits.Y > (float)Level.Loaded.Size.X)
				{
					spawnPos.X = limits.X + (float)A_2.minWidth * 0.5f + 100f + A_2.subDockingPortOffset;
				}
				else
				{
					spawnPos.X = (limits.X + limits.Y) / 2f + A_2.subDockingPortOffset;
				}
			}
			return spawnPos;
		}

		// Token: 0x060021F2 RID: 8690 RVA: 0x001556BC File Offset: 0x001538BC
		[CompilerGenerated]
		internal static void <SetLayerEnabled>g__SetItemHidden|183_0(Item item, bool isHidden)
		{
			foreach (Item containedItem in item.ContainedItems)
			{
				Submarine.<SetLayerEnabled>g__SetItemHidden|183_0(containedItem, isHidden);
			}
			foreach (ConnectionPanel connectionPanel in item.GetComponents<ConnectionPanel>())
			{
				foreach (Connection connection in connectionPanel.Connections)
				{
					foreach (Wire wire in connection.Wires)
					{
						wire.Item.IsLayerHidden = isHidden;
					}
				}
			}
			if (isHidden)
			{
				foreach (LightComponent lightComponent in item.GetComponents<LightComponent>())
				{
					lightComponent.Light.Enabled = false;
				}
			}
		}

		// Token: 0x040010EB RID: 4331
		private static readonly HashSet<Submarine> visibleSubs = new HashSet<Submarine>();

		// Token: 0x040010EC RID: 4332
		private static double prevCullTime;

		// Token: 0x040010ED RID: 4333
		private static Rectangle prevCullArea;

		// Token: 0x040010EE RID: 4334
		private const float CullInterval = 0.25f;

		// Token: 0x040010EF RID: 4335
		private const int CullMargin = 50;

		// Token: 0x040010F0 RID: 4336
		private const int CullMoveThreshold = 50;

		// Token: 0x040010F1 RID: 4337
		public static float DamageEffectCutoff;

		// Token: 0x040010F2 RID: 4338
		private static readonly List<Structure> depthSortedDamageable = new List<Structure>();

		// Token: 0x040010F4 RID: 4340
		public CharacterTeamType TeamID;

		// Token: 0x040010F5 RID: 4341
		public static readonly Vector2 HiddenSubStartPosition = new Vector2(-50000f, 10000f);

		// Token: 0x040010F8 RID: 4344
		public static bool LockX;

		// Token: 0x040010F9 RID: 4345
		public static bool LockY;

		// Token: 0x040010FA RID: 4346
		public static readonly Vector2 GridSize = new Vector2(16f, 16f);

		// Token: 0x040010FB RID: 4347
		public static readonly Submarine[] MainSubs = new Submarine[2];

		// Token: 0x040010FC RID: 4348
		private static readonly List<Submarine> loaded = new List<Submarine>();

		// Token: 0x040010FD RID: 4349
		private readonly Identifier upgradeEventIdentifier;

		// Token: 0x040010FE RID: 4350
		private static List<MapEntity> visibleEntities;

		// Token: 0x040010FF RID: 4351
		private SubmarineBody subBody;

		// Token: 0x04001100 RID: 4352
		public readonly Dictionary<Submarine, DockingPort> ConnectedDockingPorts;

		// Token: 0x04001101 RID: 4353
		private static Vector2 lastPickedPosition;

		// Token: 0x04001102 RID: 4354
		private static float lastPickedFraction;

		// Token: 0x04001103 RID: 4355
		private static Fixture lastPickedFixture;

		// Token: 0x04001104 RID: 4356
		private static Vector2 lastPickedNormal;

		// Token: 0x04001105 RID: 4357
		private Vector2 prevPosition;

		// Token: 0x04001106 RID: 4358
		private float networkUpdateTimer;

		// Token: 0x04001107 RID: 4359
		private EntityGrid entityGrid;

		// Token: 0x04001108 RID: 4360
		public bool ShowSonarMarker = true;

		// Token: 0x0400110B RID: 4363
		public List<WayPoint> ForcedOutpostModuleWayPoints = new List<WayPoint>();

		// Token: 0x0400110C RID: 4364
		private float? realWorldCrushDepth;

		// Token: 0x0400110E RID: 4366
		private int? submarineSpecificIDTag;

		// Token: 0x0400110F RID: 4367
		private readonly List<WayPoint> exitPoints = new List<WayPoint>();

		// Token: 0x04001110 RID: 4368
		private float ballastFloraTimer;

		// Token: 0x04001114 RID: 4372
		private static readonly HashSet<Submarine> checkSubmarineBorders = new HashSet<Submarine>();

		// Token: 0x04001115 RID: 4373
		private readonly HashSet<Submarine> connectedSubs;

		// Token: 0x04001116 RID: 4374
		private static readonly Dictionary<Body, float> bodyDist = new Dictionary<Body, float>();

		// Token: 0x04001117 RID: 4375
		private static readonly List<Body> bodies = new List<Body>();

		// Token: 0x04001118 RID: 4376
		private bool flippedX;

		// Token: 0x0400111A RID: 4378
		private List<PathNode> outdoorNodes;

		// Token: 0x0400111B RID: 4379
		private readonly Dictionary<Submarine, HashSet<PathNode>> obstructedNodes = new Dictionary<Submarine, HashSet<PathNode>>();
	}
}
