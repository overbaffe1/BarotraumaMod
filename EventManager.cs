using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200004D RID: 77
	internal class EventManager
	{
		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000AED RID: 2797 RVA: 0x00062F4A File Offset: 0x0006114A
		// (set) Token: 0x06000AEE RID: 2798 RVA: 0x00062F52 File Offset: 0x00061152
		[Nullable(2)]
		public Event PinnedEvent { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x06000AEF RID: 2799 RVA: 0x00062F5C File Offset: 0x0006115C
		[NullableContext(1)]
		public void DebugDraw(SpriteBatch spriteBatch)
		{
			foreach (Event ev in this.activeEvents)
			{
				Vector2 drawPos = ev.DebugDrawPos;
				drawPos.Y = -drawPos.Y;
				Vector2 textOffset = new Vector2(-150f, 0f);
				spriteBatch.DrawCircle(drawPos, 600f, 6, Color.White, 20f);
				GUI.DrawString(spriteBatch, drawPos + textOffset, ev.ToString(), Color.White, new Color?(Color.Black), 0, GUIStyle.LargeFont, ForceUpperCase.Inherit);
			}
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x00063010 File Offset: 0x00061210
		[NullableContext(1)]
		public void DebugDrawHUD(SpriteBatch spriteBatch, float y)
		{
			EventManager.<>c__DisplayClass13_0 CS$<>8__locals1 = new EventManager.<>c__DisplayClass13_0();
			CS$<>8__locals1.y = y;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			CS$<>8__locals1.<>4__this = this;
			foreach (ScriptedEvent scriptedEvent in (from ev in this.activeEvents
			where !ev.IsFinished && ev is ScriptedEvent
			select ev).Cast<ScriptedEvent>())
			{
				EventManager.DrawEventTargetTags(CS$<>8__locals1.spriteBatch, scriptedEvent);
			}
			float theoreticalMaxMonsterStrength = 10000f;
			float num = theoreticalMaxMonsterStrength;
			GameSession gameSession = GameMain.GameSession;
			float? num2;
			if (gameSession == null)
			{
				num2 = null;
			}
			else
			{
				Level level = gameSession.Level;
				num2 = ((level != null) ? new float?(level.Difficulty) : null);
			}
			float? num3 = num2;
			float relativeMaxMonsterStrength = num * num3.GetValueOrDefault() / 100f;
			float absoluteMonsterStrength = this.monsterStrength / theoreticalMaxMonsterStrength;
			float relativeMonsterStrength = this.monsterStrength / relativeMaxMonsterStrength;
			GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2(10f, CS$<>8__locals1.y), "EventManager", Color.White, new Color?(Color.Black * 0.6f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Event cooldown", (double)Math.Max(this.eventCoolDown, 0f), Color.White, 20);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Current intensity", Math.Round((double)(this.currentIntensity * 100f)), Color.Lerp(Color.White, GUIStyle.Red, this.currentIntensity), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Target intensity", Math.Round((double)(this.targetIntensity * 100f)), Color.Lerp(Color.White, GUIStyle.Red, this.targetIntensity), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Crew health", Math.Round((double)(this.avgCrewHealth * 100f)), Color.Lerp(GUIStyle.Red, GUIStyle.Green, this.avgCrewHealth), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Hull integrity", Math.Round((double)(this.avgHullIntegrity * 100f)), Color.Lerp(GUIStyle.Red, GUIStyle.Green, this.avgHullIntegrity), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Flooding amount", Math.Round((double)(this.floodingAmount * 100f)), Color.Lerp(GUIStyle.Green, GUIStyle.Red, this.floodingAmount), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Fire amount", Math.Round((double)(this.fireAmount * 100f)), Color.Lerp(GUIStyle.Green, GUIStyle.Red, this.fireAmount), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Enemy danger", Math.Round((double)(this.enemyDanger * 100f)), Color.Lerp(GUIStyle.Green, GUIStyle.Red, this.enemyDanger), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Current monster strength (total)", Math.Round((double)this.monsterStrength), Color.Lerp(GUIStyle.Green, GUIStyle.Red, relativeMonsterStrength), 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Main events", Math.Round((double)this.CumulativeMonsterStrengthMain), Color.White, 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Ruin events", Math.Round((double)this.CumulativeMonsterStrengthRuins), Color.White, 15);
			CS$<>8__locals1.<DebugDrawHUD>g__DrawString|0("Wreck events", Math.Round((double)this.CumulativeMonsterStrengthWrecks), Color.White, 15);
			if (this.intensityGraph == null)
			{
				int graphDensity = 360;
				this.intensityGraph = new Graph(graphDensity);
				this.targetIntensityGraph = new Graph(graphDensity);
				this.monsterStrengthGraph = new Graph(graphDensity);
			}
			if (Timing.TotalTime > (double)(this.lastIntensityUpdate + 10f))
			{
				this.intensityGraph.Update(this.currentIntensity);
				this.targetIntensityGraph.Update(this.targetIntensity);
				this.monsterStrengthGraph.Update(relativeMonsterStrength);
				this.lastIntensityUpdate = (float)Timing.TotalTime;
			}
			CS$<>8__locals1.graphRect = new Rectangle(15, (int)(CS$<>8__locals1.y + GUI.AdjustForTextScale(55f)), (int)(200f * GUI.xScale), (int)(100f * GUI.yScale));
			CS$<>8__locals1.isGraphHovered = CS$<>8__locals1.graphRect.Contains(PlayerInput.MousePosition);
			bool leftMousePressed = PlayerInput.PrimaryMouseButtonDown() || PlayerInput.PrimaryMouseButtonHeld();
			bool rightMousePressed = PlayerInput.SecondaryMouseButtonHeld() || PlayerInput.SecondaryMouseButtonDown();
			if ((!this.isGraphSelected & CS$<>8__locals1.isGraphHovered) && leftMousePressed)
			{
				this.isGraphSelected = true;
			}
			if (this.isGraphSelected && rightMousePressed)
			{
				this.isGraphSelected = false;
			}
			Color intensityColor = Color.Lerp(Color.White, GUIStyle.Red, this.currentIntensity);
			if (CS$<>8__locals1.isGraphHovered || this.isGraphSelected)
			{
				int padding = 15;
				int graphHeight = Math.Min((int)((float)GameMain.GraphicsHeight * 0.35f), GameMain.GraphicsHeight - (CS$<>8__locals1.graphRect.Top + 3 * padding));
				CS$<>8__locals1.graphRect.Size = new Point(GameMain.GraphicsWidth - 2 * padding, graphHeight);
				intensityColor = Color.Red;
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.graphRect, Color.Black * 0.95f, true, 0f, 1f);
			}
			else
			{
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.graphRect, Color.Black * 0.6f, true, 0f, 1f);
			}
			this.intensityGraph.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.graphRect, new float?(1f), 0f, new Color?(intensityColor), delegate(SpriteBatch sBatch, float value, int order, Vector2 pos)
			{
				if (CS$<>8__locals1.isGraphHovered || CS$<>8__locals1.<>4__this.isGraphSelected)
				{
					Vector2 bottomPoint = new Vector2(pos.X, (float)CS$<>8__locals1.graphRect.Bottom);
					float height = 3f * GUI.yScale;
					if (order % 6 == 0)
					{
						height *= 3f;
						string text = (order / 6).ToString();
						GUIFont font = GUIStyle.SmallFont;
						Vector2 textSize = font.MeasureString(text, false);
						Vector2 textPos = new Vector2(bottomPoint.X - textSize.X / 2f, bottomPoint.Y + height * 1.5f);
						Vector2 pos2 = textPos;
						string text2 = text;
						Color white = Color.White;
						GUIFont font2 = font;
						GUI.DrawString(sBatch, pos2, text2, white, null, 0, font2, ForceUpperCase.Inherit);
					}
					GUI.DrawLine(sBatch, bottomPoint, bottomPoint + Vector2.UnitY * height, Color.White, 0f, Math.Max(GUI.Scale, 1f));
					base.<DebugDrawHUD>g__DrawTimeStamps|2(sBatch, Color.Red, pos, order);
				}
			});
			this.targetIntensityGraph.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.graphRect, new float?(1f), 0f, new Color?(intensityColor * 0.5f), null);
			if (CS$<>8__locals1.isGraphHovered || this.isGraphSelected)
			{
				float? maxValue = new float?((float)1);
				Color color = Color.White;
				if (relativeMonsterStrength > 1f)
				{
					maxValue = null;
					color = Color.Yellow;
				}
				this.monsterStrengthGraph.Draw(CS$<>8__locals1.spriteBatch, CS$<>8__locals1.graphRect, maxValue, 0f, new Color?(color), delegate(SpriteBatch sBatch, float value, int order, Vector2 pos)
				{
					CS$<>8__locals1.<DebugDrawHUD>g__DrawTimeStamps|2(sBatch, color, pos, order);
				});
			}
			GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.graphRect.Right, (float)CS$<>8__locals1.graphRect.Y + (float)CS$<>8__locals1.graphRect.Height * (1f - this.eventThreshold)), new Vector2((float)(CS$<>8__locals1.graphRect.Right + 5), (float)CS$<>8__locals1.graphRect.Y + (float)CS$<>8__locals1.graphRect.Height * (1f - this.eventThreshold)), Color.Orange, 0f, 3f);
			int yStep = (int)(20f * GUI.yScale);
			CS$<>8__locals1.y = (float)(CS$<>8__locals1.graphRect.Bottom + yStep);
			if (CS$<>8__locals1.isGraphHovered || this.isGraphSelected)
			{
				CS$<>8__locals1.y += (float)yStep;
			}
			int x = CS$<>8__locals1.graphRect.X;
			float adjustedYStep = GUI.AdjustForTextScale(15f);
			if (this.isCrewAway && this.crewAwayDuration < this.settings.FreezeDurationWhenCrewAway)
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "Events frozen (crew away from sub): " + ToolBox.SecondsToReadableTime(this.settings.FreezeDurationWhenCrewAway - this.crewAwayDuration), Color.LightGreen * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				CS$<>8__locals1.y += adjustedYStep;
			}
			else if (this.crewAwayResetTimer > 0f)
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "Events frozen (crew just returned to the sub): " + ToolBox.SecondsToReadableTime(this.crewAwayResetTimer), Color.LightGreen * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				CS$<>8__locals1.y += adjustedYStep;
			}
			else if (this.eventCoolDown > 0f)
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "Event cooldown active: " + ToolBox.SecondsToReadableTime(this.eventCoolDown), Color.LightGreen * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				CS$<>8__locals1.y += adjustedYStep;
			}
			else if (this.currentIntensity > this.eventThreshold)
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), string.Concat(new string[]
				{
					"Intensity too high for new events: ",
					((int)(this.currentIntensity * 100f)).ToString(),
					"%/",
					((int)(this.eventThreshold * 100f)).ToString(),
					"%"
				}), Color.LightGreen * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				CS$<>8__locals1.y += adjustedYStep;
			}
			adjustedYStep = GUI.AdjustForTextScale(12f);
			foreach (EventSet eventSet in this.pendingEventSets)
			{
				if (Submarine.MainSub == null)
				{
					break;
				}
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "New event (ID " + eventSet.Identifier.ToString() + ") after: ", Color.Orange * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				CS$<>8__locals1.y += adjustedYStep;
				if (eventSet.PerCave)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "    submarine near cave", Color.Orange * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					CS$<>8__locals1.y += adjustedYStep;
				}
				if (eventSet.PerWreck)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "    submarine near the wreck", Color.Orange * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					CS$<>8__locals1.y += adjustedYStep;
				}
				if (eventSet.PerRuin)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "    submarine near the ruins", Color.Orange * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					CS$<>8__locals1.y += adjustedYStep;
				}
				if (this.roundDuration < eventSet.MinMissionTime)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), string.Concat(new string[]
					{
						"    ",
						((int)(eventSet.MinDistanceTraveled * 100f)).ToString(),
						"% travelled (current: ",
						((int)(this.distanceTraveled * 100f)).ToString(),
						" %)"
					}), ((Submarine.MainSub == null || this.distanceTraveled < eventSet.MinDistanceTraveled) ? Color.Lerp(GUIStyle.Yellow, GUIStyle.Red, eventSet.MinDistanceTraveled - this.distanceTraveled) : GUIStyle.Green) * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					CS$<>8__locals1.y += adjustedYStep;
				}
				if (this.CurrentIntensity < eventSet.MinIntensity || this.CurrentIntensity > eventSet.MaxIntensity)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "    intensity between " + eventSet.MinIntensity.FormatDoubleDecimal() + " and " + eventSet.MaxIntensity.FormatDoubleDecimal(), Color.Orange * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					CS$<>8__locals1.y += adjustedYStep;
				}
				if (this.roundDuration < eventSet.MinMissionTime)
				{
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "    " + ((int)(eventSet.MinMissionTime - this.roundDuration)).ToString() + " s", Color.Lerp(GUIStyle.Yellow, GUIStyle.Red, eventSet.MinMissionTime - this.roundDuration), null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				}
				CS$<>8__locals1.y += GUI.AdjustForTextScale(15f);
				if (CS$<>8__locals1.y > (float)GameMain.GraphicsHeight * 0.9f)
				{
					CS$<>8__locals1.y = (float)(CS$<>8__locals1.graphRect.Bottom + yStep * 2);
					x += 300;
				}
			}
			GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)x, CS$<>8__locals1.y), "Current events: ", Color.White * 0.9f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			CS$<>8__locals1.y += (float)yStep;
			adjustedYStep = GUI.AdjustForTextScale(18f);
			foreach (Event ev2 in from ev in this.activeEvents
			where !ev.IsFinished || PlayerInput.IsShiftDown()
			select ev)
			{
				GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)(x + 5), CS$<>8__locals1.y), ev2.ToString(), ((!ev2.IsFinished) ? Color.White : Color.Red) * 0.8f, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				Rectangle rect = new Rectangle(new Point(x + 5, (int)CS$<>8__locals1.y), GUIStyle.SmallFont.MeasureString(ev2.ToString(), false).ToPoint());
				Rectangle outlineRect = new Rectangle(rect.Location, rect.Size);
				outlineRect.Inflate(4, 4);
				if (this.PinnedEvent == ev2)
				{
					GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, outlineRect, Color.White, false, 0f, 1f);
				}
				if (rect.Contains(PlayerInput.MousePosition))
				{
					GUI.MouseCursor = CursorState.Hand;
					GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, outlineRect, Color.White, false, 0f, 1f);
					if (ev2 != this.PinnedEvent)
					{
						this.DrawEvent(CS$<>8__locals1.spriteBatch, ev2, new Rectangle?(rect));
					}
					else if (rightMousePressed)
					{
						this.PinnedEvent = null;
					}
					if (leftMousePressed)
					{
						this.PinnedEvent = ev2;
					}
				}
				CS$<>8__locals1.y += adjustedYStep;
				if (CS$<>8__locals1.y > (float)GameMain.GraphicsHeight * 0.9f)
				{
					CS$<>8__locals1.y = (float)(CS$<>8__locals1.graphRect.Bottom + yStep * 2);
					x += 300;
				}
			}
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x0006402C File Offset: 0x0006222C
		[NullableContext(1)]
		public void DrawPinnedEvent(SpriteBatch spriteBatch)
		{
			if (this.PinnedEvent != null)
			{
				Rectangle rect = this.DrawEvent(spriteBatch, this.PinnedEvent, null);
				if (rect != Rectangle.Empty && rect.Contains(PlayerInput.MousePosition) && !this.isDragging)
				{
					GUI.MouseCursor = CursorState.Move;
					if (PlayerInput.PrimaryMouseButtonDown() || PlayerInput.PrimaryMouseButtonHeld())
					{
						this.isDragging = true;
					}
					if (PlayerInput.SecondaryMouseButtonClicked() || PlayerInput.SecondaryMouseButtonHeld())
					{
						this.PinnedEvent = null;
						this.isDragging = false;
					}
				}
				if (this.isDragging)
				{
					GUI.MouseCursor = CursorState.Dragging;
					this.pinnedPosition = PlayerInput.MousePosition - new Vector2((float)rect.Width / 2f, -24f);
					if (!PlayerInput.PrimaryMouseButtonHeld())
					{
						this.isDragging = false;
					}
				}
			}
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x000640F8 File Offset: 0x000622F8
		[NullableContext(1)]
		private static void DrawEventTargetTags(SpriteBatch spriteBatch, ScriptedEvent scriptedEvent)
		{
			GameScreen screen = Screen.Selected as GameScreen;
			if (screen != null)
			{
				Camera cam = screen.Cam;
				Dictionary<Entity, List<Identifier>> tagsDictionary = new Dictionary<Entity, List<Identifier>>();
				foreach (KeyValuePair<Identifier, List<Entity>> keyValuePair in scriptedEvent.Targets)
				{
					Identifier identifier2;
					List<Entity> list;
					keyValuePair.Deconstruct(out identifier2, out list);
					Identifier key = identifier2;
					List<Entity> value = list;
					foreach (Entity entity in value)
					{
						if (tagsDictionary.ContainsKey(entity))
						{
							tagsDictionary[entity].Add(key);
						}
						else
						{
							tagsDictionary.Add(entity, new List<Identifier>
							{
								key
							});
						}
					}
				}
				Identifier identifier = scriptedEvent.Prefab.Identifier;
				foreach (KeyValuePair<Entity, List<Identifier>> keyValuePair2 in tagsDictionary)
				{
					Entity entity3;
					List<Identifier> list2;
					keyValuePair2.Deconstruct(out entity3, out list2);
					Entity entity2 = entity3;
					List<Identifier> tags = list2;
					if (!entity2.Removed)
					{
						string text = tags.Aggregate("Tags:\n", (string current, Identifier tag) => current + "    " + tag.ColorizeObject() + "\n").TrimEnd(new char[]
						{
							'\r',
							'\n'
						});
						if (!identifier.IsEmpty)
						{
							text = "Event: " + identifier.ColorizeObject() + "\n" + text;
						}
						ImmutableArray<RichTextData>? richTextData = RichTextData.GetRichTextData(text, out text);
						Vector2 entityPos = cam.WorldToScreen(entity2.WorldPosition);
						Vector2 infoSize = GUIStyle.SmallFont.MeasureString(text, false);
						Vector2 infoPos = entityPos + new Vector2(128f * cam.Zoom, -(128f * cam.Zoom));
						infoPos.Y -= infoSize.Y / 2f;
						Rectangle infoRect = new Rectangle(infoPos.ToPoint(), infoSize.ToPoint());
						infoRect.Inflate(4, 4);
						GUI.DrawRectangle(spriteBatch, infoRect, Color.Black * 0.8f, true, 0f, 1f);
						GUI.DrawRectangle(spriteBatch, infoRect, Color.White, false, 0f, 1f);
						Vector2 pos = infoPos;
						string text2 = text;
						Color white = Color.White;
						GUIFont smallFont = GUIStyle.SmallFont;
						GUI.DrawStringWithColors(spriteBatch, pos, text2, white, richTextData, null, 0, smallFont, 0f);
						GUI.DrawLine(spriteBatch, entityPos, new Vector2((float)infoRect.Location.X, (float)(infoRect.Location.Y + infoRect.Height / 2)), Color.White, 0f, 1f);
					}
				}
			}
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x00064408 File Offset: 0x00062608
		[NullableContext(1)]
		private Rectangle DrawEvent(SpriteBatch spriteBatch, Event ev, Rectangle? parentRect = null)
		{
			ScriptedEvent scriptedEvent = ev as ScriptedEvent;
			Rectangle result;
			if (scriptedEvent == null)
			{
				ArtifactEvent artifactEvent = ev as ArtifactEvent;
				if (artifactEvent == null)
				{
					MonsterEvent monsterEvent = ev as MonsterEvent;
					if (monsterEvent == null)
					{
						result = Rectangle.Empty;
					}
					else
					{
						result = this.DrawMonsterEvent(spriteBatch, monsterEvent, parentRect);
					}
				}
				else
				{
					result = this.DrawArtifactEvent(spriteBatch, artifactEvent, parentRect);
				}
			}
			else
			{
				result = this.DrawScriptedEvent(spriteBatch, scriptedEvent, parentRect);
			}
			return result;
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x00064460 File Offset: 0x00062660
		[NullableContext(1)]
		private Rectangle DrawScriptedEvent(SpriteBatch spriteBatch, ScriptedEvent scriptedEvent, Rectangle? parentRect = null)
		{
			List<EventManager.DebugLine> positions = new List<EventManager.DebugLine>();
			string text = scriptedEvent.GetDebugInfo();
			if (scriptedEvent.Targets != null)
			{
				foreach (KeyValuePair<Identifier, List<Entity>> keyValuePair in scriptedEvent.Targets)
				{
					Identifier identifier;
					List<Entity> list;
					keyValuePair.Deconstruct(out identifier, out list);
					List<Entity> entities = list;
					if (entities != null && entities.Any<Entity>())
					{
						foreach (Entity entity in entities)
						{
							positions.Add(new EventManager.DebugLine(entity.WorldPosition, Color.White));
						}
					}
				}
			}
			return this.DrawInfoRectangle(spriteBatch, scriptedEvent, text, parentRect, positions);
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x00064538 File Offset: 0x00062738
		[NullableContext(1)]
		private Rectangle DrawArtifactEvent(SpriteBatch spriteBatch, ArtifactEvent artifactEvent, Rectangle? parentRect = null)
		{
			this.debugPositions.Clear();
			string text = artifactEvent.GetDebugInfo();
			if (artifactEvent.Item != null && !artifactEvent.Item.Removed)
			{
				Vector2 pos = artifactEvent.Item.WorldPosition;
				this.debugPositions.Add(new EventManager.DebugLine(pos, Color.White));
			}
			return this.DrawInfoRectangle(spriteBatch, artifactEvent, text, parentRect, this.debugPositions);
		}

		// Token: 0x06000AF6 RID: 2806 RVA: 0x000645A0 File Offset: 0x000627A0
		[NullableContext(1)]
		private Rectangle DrawMonsterEvent(SpriteBatch spriteBatch, MonsterEvent monsterEvent, Rectangle? parentRect = null)
		{
			this.debugPositions.Clear();
			string text = monsterEvent.GetDebugInfo();
			if (monsterEvent.SpawnPos != null && Submarine.MainSub != null)
			{
				Vector2 pos = monsterEvent.SpawnPos.Value;
				text = text + "Distance from submarine: " + Vector2.Distance(pos, Submarine.MainSub.WorldPosition).ColorizeObject() + "\n";
				this.debugPositions.Add(new EventManager.DebugLine(pos, Color.White));
			}
			if (monsterEvent.Monsters != null)
			{
				text += ((!monsterEvent.Monsters.Any<Character>()) ? ("Monsters: " + "None".ColorizeObject()) : "Monsters:\n");
				foreach (Character monster in monsterEvent.Monsters)
				{
					string str = text;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 4);
					defaultInterpolatedStringHandler.AppendLiteral("    ");
					defaultInterpolatedStringHandler.AppendFormatted(monster.ColorizeObject());
					defaultInterpolatedStringHandler.AppendLiteral(" -> (Dead: ");
					defaultInterpolatedStringHandler.AppendFormatted(monster.IsDead.ColorizeObject());
					defaultInterpolatedStringHandler.AppendLiteral(", Health: ");
					defaultInterpolatedStringHandler.AppendFormatted(monster.HealthPercentage.ColorizeObject());
					defaultInterpolatedStringHandler.AppendLiteral("%, AIState: ");
					EnemyAIController enemyAI = monster.AIController as EnemyAIController;
					defaultInterpolatedStringHandler.AppendFormatted(((enemyAI != null) ? enemyAI.State : AIState.Idle).ColorizeObject());
					defaultInterpolatedStringHandler.AppendLiteral(")\n");
					text = str + defaultInterpolatedStringHandler.ToStringAndClear();
					if (!monster.Removed)
					{
						this.debugPositions.Add(new EventManager.DebugLine(monster.WorldPosition, Color.Red));
					}
				}
			}
			return this.DrawInfoRectangle(spriteBatch, monsterEvent, text, parentRect, this.debugPositions);
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x00064794 File Offset: 0x00062994
		[NullableContext(1)]
		private Rectangle DrawInfoRectangle(SpriteBatch spriteBatch, Event @event, string text, Rectangle? parentRect = null, [Nullable(2)] List<EventManager.DebugLine> drawPoints = null)
		{
			text = text.TrimEnd(new char[]
			{
				'\r',
				'\n'
			});
			Identifier identifier = @event.Prefab.Identifier;
			if (!identifier.IsEmpty)
			{
				text = "Identifier: " + identifier.ColorizeObject() + "\n" + text;
			}
			ImmutableArray<RichTextData>? richTextData = RichTextData.GetRichTextData(text, out text);
			Vector2 size = GUIStyle.SmallFont.MeasureString(text, false);
			Vector2 pos = this.pinnedPosition;
			Rectangle? infoBarRect = null;
			Rectangle infoRect;
			if (parentRect != null)
			{
				Rectangle rect = parentRect.Value;
				pos = new Vector2(350f, (float)GameMain.GraphicsHeight / 2f - size.Y / 2f);
				infoRect = new Rectangle(pos.ToPoint(), size.ToPoint());
				infoRect.Inflate(8, 8);
				GUI.DrawLine(spriteBatch, new Vector2((float)rect.Right, (float)(rect.Y + rect.Height / 2)), new Vector2((float)infoRect.X, (float)(infoRect.Y + infoRect.Height / 2)), Color.White, 0f, 1f);
			}
			else
			{
				infoRect = new Rectangle(pos.ToPoint(), size.ToPoint());
				infoRect.Inflate(8, 8);
				Rectangle barRect = new Rectangle(infoRect.Left, infoRect.Top - 32, infoRect.Width, 32);
				GUI.DrawRectangle(spriteBatch, barRect, Color.DarkGray * 0.8f, true, 0f, 1f);
				GUI.DrawString(spriteBatch, barRect.Location.ToVector2() + barRect.Size.ToVector2() / 2f - GUIStyle.SubHeadingFont.MeasureString("Pinned event", false) / 2f, "Pinned event", Color.White, null, 0, null, ForceUpperCase.Inherit);
				GUI.DrawRectangle(spriteBatch, barRect, Color.White, false, 0f, 1f);
				infoBarRect = new Rectangle?(barRect);
			}
			if (drawPoints != null && drawPoints.Any<EventManager.DebugLine>())
			{
				Screen selected = Screen.Selected;
				if (((selected != null) ? selected.Cam : null) != null)
				{
					foreach (EventManager.DebugLine line in drawPoints)
					{
						if (line.Position != Vector2.Zero)
						{
							float xPos = (float)infoRect.Right;
							if (parentRect == null && this.pinnedPosition.X + (float)infoRect.Width / 2f > (float)GameMain.GraphicsWidth / 2f)
							{
								xPos = (float)infoRect.Left;
							}
							GUI.DrawLine(spriteBatch, new Vector2(xPos, (float)(infoRect.Top + infoRect.Height / 2)), Screen.Selected.Cam.WorldToScreen(line.Position), line.Color, 0f, 1f);
						}
					}
				}
			}
			GUI.DrawRectangle(spriteBatch, infoRect, Color.Black * 0.8f, true, 0f, 1f);
			GUI.DrawRectangle(spriteBatch, infoRect, Color.White, false, 0f, 1f);
			if (richTextData != null && richTextData.Value.Length > 0)
			{
				Vector2 pos2 = pos;
				string text2 = text;
				Color white = Color.White;
				ImmutableArray<RichTextData>? immutableArray = new ImmutableArray<RichTextData>?(richTextData.Value);
				GUI.DrawStringWithColors(spriteBatch, pos2, text2, white, immutableArray, null, 0, GUIStyle.SmallFont, 0f);
			}
			else
			{
				GUI.DrawString(spriteBatch, pos, text, Color.White, null, 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			}
			return infoBarRect.GetValueOrDefault(infoRect);
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x00064B78 File Offset: 0x00062D78
		[NullableContext(1)]
		public void ClientRead(IReadMessage msg)
		{
			if (GameMain.GameSession.IsRunning && !GameMain.Instance.LoadingScreenOpen)
			{
				this.ClientApplyNetworkMessage(msg);
				return;
			}
			CoroutineManager.StartCoroutine(this.ApplyNetworkMessageWhenRoundLoaded(msg), "");
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x00064BAC File Offset: 0x00062DAC
		[NullableContext(1)]
		public IEnumerable<CoroutineStatus> ApplyNetworkMessageWhenRoundLoaded(IReadMessage msg)
		{
			EventManager.<ApplyNetworkMessageWhenRoundLoaded>d__24 <ApplyNetworkMessageWhenRoundLoaded>d__ = new EventManager.<ApplyNetworkMessageWhenRoundLoaded>d__24(-2);
			<ApplyNetworkMessageWhenRoundLoaded>d__.<>4__this = this;
			<ApplyNetworkMessageWhenRoundLoaded>d__.<>3__msg = msg;
			return <ApplyNetworkMessageWhenRoundLoaded>d__;
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x00064BC4 File Offset: 0x00062DC4
		[NullableContext(1)]
		public void ClientApplyNetworkMessage(IReadMessage msg)
		{
			EventManager.NetworkEventType eventType = (EventManager.NetworkEventType)msg.ReadByte();
			switch (eventType)
			{
			case EventManager.NetworkEventType.CONVERSATION:
				break;
			case EventManager.NetworkEventType.CONVERSATION_SELECTED_OPTION:
			{
				ushort identifier2 = msg.ReadUInt16();
				int selectedOption = (int)(msg.ReadByte() - 1);
				ConversationAction.SelectOption(identifier2, selectedOption);
				return;
			}
			case EventManager.NetworkEventType.STATUSEFFECT:
			{
				Identifier eventIdentifier = msg.ReadIdentifier();
				ushort actionIndex = msg.ReadUInt16();
				ushort targetCount = msg.ReadUInt16();
				List<Entity> targets = new List<Entity>();
				for (int i = 0; i < (int)targetCount; i++)
				{
					ushort targetID = msg.ReadUInt16();
					Entity target = Entity.FindEntityByID(targetID);
					if (target != null)
					{
						targets.Add(target);
					}
				}
				EventPrefab eventPrefab = EventSet.GetEventPrefab(eventIdentifier);
				if (eventPrefab == null)
				{
					return;
				}
				int j = 0;
				using (IEnumerator<ContentXElement> enumerator = eventPrefab.ConfigElement.Descendants().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement element = enumerator.Current;
						if (j != (int)actionIndex)
						{
							j++;
						}
						else
						{
							using (IEnumerator<ContentXElement> enumerator2 = element.Elements().GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									ContentXElement subElement = enumerator2.Current;
									if (subElement.Name.ToString().Equals("statuseffect", StringComparison.OrdinalIgnoreCase))
									{
										ContentXElement element2 = subElement;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
										defaultInterpolatedStringHandler.AppendLiteral("EventManager.ClientRead (");
										defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventIdentifier);
										defaultInterpolatedStringHandler.AppendLiteral(")");
										StatusEffect effect = StatusEffect.Load(element2, defaultInterpolatedStringHandler.ToStringAndClear());
										foreach (Entity target2 in targets)
										{
											Item item = target2 as Item;
											if (item != null)
											{
												effect.Apply(effect.type, 1f, item, item.AllPropertyObjects, null);
											}
											else
											{
												effect.Apply(effect.type, 1f, target2, target2 as ISerializableEntity, null);
											}
										}
									}
								}
								break;
							}
						}
					}
					return;
				}
				break;
			}
			case EventManager.NetworkEventType.MISSION:
			{
				Identifier missionIdentifier = msg.ReadIdentifier();
				int locationIndex = msg.ReadInt32();
				int destinationIndex = msg.ReadInt32();
				string missionName = msg.ReadString();
				if (Screen.Selected == GameMain.NetLobbyScreen)
				{
					return;
				}
				MissionPrefab prefab = MissionPrefab.Prefabs.Find((MissionPrefab mp) => mp.Identifier == missionIdentifier);
				if (prefab == null)
				{
					return;
				}
				RichString headerText = string.Empty;
				RichString text2 = TextManager.GetWithVariable("missionunlocked", "[missionname]", missionName, FormatCapitals.No);
				LocalizedString[] buttons = Array.Empty<LocalizedString>();
				Sprite icon = prefab.Icon;
				new GUIMessageBox(headerText, text2, buttons, new Vector2?(new Vector2(0.3f, 0.15f)), new Point?(new Point(512, 128)), Alignment.TopLeft, GUIMessageBox.Type.InGame, "", icon, "", null, null, false).IconColor = prefab.IconColor;
				GameSession gameSession = GameMain.GameSession;
				Map map = (gameSession != null) ? gameSession.Map : null;
				if (map == null || locationIndex < 0 || locationIndex >= map.Locations.Count)
				{
					return;
				}
				Location location = map.Locations[locationIndex];
				map.Discover(location, false);
				LocationConnection connection = null;
				if (destinationIndex != locationIndex && destinationIndex >= 0 && destinationIndex < map.Locations.Count)
				{
					Location destination = map.Locations[destinationIndex];
					connection = map.Connections.FirstOrDefault((LocationConnection c) => c.Locations.Contains(location) && c.Locations.Contains(destination));
				}
				if (connection != null)
				{
					location.UnlockMission(prefab, connection);
					return;
				}
				location.UnlockMission(prefab);
				return;
			}
			case EventManager.NetworkEventType.UNLOCKPATH:
			{
				ushort connectionIndex = msg.ReadUInt16();
				GameSession gameSession2 = GameMain.GameSession;
				bool flag;
				if (gameSession2 == null)
				{
					flag = (null != null);
				}
				else
				{
					Map map2 = gameSession2.Map;
					flag = (((map2 != null) ? map2.Connections : null) != null);
				}
				if (!flag)
				{
					return;
				}
				if ((int)connectionIndex >= GameMain.GameSession.Map.Connections.Count)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(110, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to unlock a path on the campaign map. Connection index out of bounds (index: ");
					defaultInterpolatedStringHandler2.AppendFormatted<ushort>(connectionIndex);
					defaultInterpolatedStringHandler2.AppendLiteral(", number of connections: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(GameMain.GameSession.Map.Connections.Count);
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					return;
				}
				GameMain.GameSession.Map.Connections[(int)connectionIndex].Locked = false;
				new GUIMessageBox(string.Empty, TextManager.Get("pathunlockedgeneric"), Array.Empty<LocalizedString>(), new Vector2?(new Vector2(0.3f, 0.15f)), new Point?(new Point(512, 128)), Alignment.TopLeft, GUIMessageBox.Type.InGame, "", null, "UnlockPathIcon", null, null, false);
				return;
			}
			case EventManager.NetworkEventType.EVENTLOG:
				this.ClientReadEventLog(GameMain.Client, msg);
				return;
			case EventManager.NetworkEventType.EVENTOBJECTIVE:
				EventManager.ClientReadEventObjective(GameMain.Client, msg);
				return;
			default:
				return;
			}
			ushort identifier = msg.ReadUInt16();
			string eventSprite = msg.ReadString();
			byte dialogType = msg.ReadByte();
			bool continueConversation = msg.ReadBoolean();
			ushort speakerId = msg.ReadUInt16();
			string text = msg.ReadString();
			bool fadeToBlack = msg.ReadBoolean();
			byte optionCount = msg.ReadByte();
			List<string> options = new List<string>();
			for (int k = 0; k < (int)optionCount; k++)
			{
				options.Add(msg.ReadString());
			}
			byte endCount = msg.ReadByte();
			int[] endings = new int[(int)endCount];
			for (int l = 0; l < (int)endCount; l++)
			{
				endings[l] = (int)msg.ReadByte();
			}
			if (string.IsNullOrEmpty(text) && optionCount == 0)
			{
				GUIMessageBox.MessageBoxes.ForEachMod(delegate(GUIComponent mb)
				{
					Pair<string, ushort> pair = mb.UserData as Pair<string, ushort>;
					if (pair != null && pair.First == "ConversationAction" && pair.Second == identifier)
					{
						GUIMessageBox guimessageBox = mb as GUIMessageBox;
						if (guimessageBox == null)
						{
							return;
						}
						guimessageBox.Close();
					}
				});
			}
			else
			{
				ConversationAction.CreateDialog(text, Entity.FindEntityByID(speakerId) as Character, options, endings, eventSprite, identifier, fadeToBlack, (ConversationAction.DialogTypes)dialogType, continueConversation);
			}
			Character speaker = Entity.FindEntityByID(speakerId) as Character;
			if (speaker != null)
			{
				speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
				speaker.SetCustomInteract(null, null);
				return;
			}
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x0006521C File Offset: 0x0006341C
		[NullableContext(1)]
		private void ClientReadEventLog(GameClient client, IReadMessage msg)
		{
			EventManager.NetEventLogEntry entry = INetSerializableStruct.Read<EventManager.NetEventLogEntry>(msg);
			this.EventLog.AddEntry(entry.EventPrefabId, entry.LogEntryId, entry.Text.Replace("\\n", "\n"));
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x00065260 File Offset: 0x00063460
		[NullableContext(1)]
		private static void ClientReadEventObjective(GameClient client, IReadMessage msg)
		{
			EventManager.NetEventObjective entry = INetSerializableStruct.Read<EventManager.NetEventObjective>(msg);
			EventObjectiveAction.Trigger(entry.Type, entry.Identifier, entry.ObjectiveTag, entry.ParentObjectiveId, entry.TextTag, entry.CanBeCompleted, false, "", 450, 80);
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000AFD RID: 2813 RVA: 0x000652B0 File Offset: 0x000634B0
		public float CurrentIntensity
		{
			get
			{
				return this.currentIntensity;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000AFE RID: 2814 RVA: 0x000652B8 File Offset: 0x000634B8
		public float MusicIntensity
		{
			get
			{
				return this.musicIntensity;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000AFF RID: 2815 RVA: 0x000652C0 File Offset: 0x000634C0
		public IEnumerable<Event> ActiveEvents
		{
			get
			{
				return this.activeEvents;
			}
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x000652C8 File Offset: 0x000634C8
		public void AddTimeStamp(Event e)
		{
			this.timeStamps.Add(new EventManager.TimeStamp(e));
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x000652DC File Offset: 0x000634DC
		public EventManager()
		{
			this.isClient = (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient);
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000B02 RID: 2818 RVA: 0x000653A9 File Offset: 0x000635A9
		// (set) Token: 0x06000B03 RID: 2819 RVA: 0x000653B1 File Offset: 0x000635B1
		public int RandomSeed { get; private set; }

		// Token: 0x06000B04 RID: 2820 RVA: 0x000653BC File Offset: 0x000635BC
		public void StartRound(Level level)
		{
			EventManager.<>c__DisplayClass87_0 CS$<>8__locals1 = new EventManager.<>c__DisplayClass87_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.level = level;
			this.level = CS$<>8__locals1.level;
			if (this.isClient)
			{
				return;
			}
			this.timeStamps.Clear();
			this.pendingEventSets.Clear();
			this.selectedEvents.Clear();
			this.activeEvents.Clear();
			this.pathFinder = new PathFinder(WayPoint.WayPointList, false);
			this.totalPathLength = 0f;
			if (CS$<>8__locals1.level != null)
			{
				SteeringPath steeringPath = this.pathFinder.FindPath(ConvertUnits.ToSimUnits(CS$<>8__locals1.level.StartPosition), ConvertUnits.ToSimUnits(CS$<>8__locals1.level.EndPosition), null, null, 0f, null, null, null, true, 0f);
				this.totalPathLength = steeringPath.TotalLength;
			}
			this.SelectSettings();
			if (CS$<>8__locals1.level != null)
			{
				this.RandomSeed = ToolBox.StringToInt(CS$<>8__locals1.level.Seed);
				foreach (Identifier previousEvent in CS$<>8__locals1.level.LevelData.EventHistory)
				{
					this.RandomSeed ^= ToolBox.IdentifierToInt(previousEvent);
				}
			}
			this.random = new MTRandom(this.RandomSeed);
			GameSession gameSession = GameMain.GameSession;
			bool playingCampaign = ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode;
			EventSet initialEventSet = null;
			EventSet additiveSet = null;
			IEnumerable<EventSet> selectAlwaysEventSets = from s in this.GetAllowedEventSets(EventSet.Prefabs.ToList<EventSet>(), new bool?(playingCampaign))
			where s.SelectAlways
			select s;
			foreach (EventSet eventSet in selectAlwaysEventSets)
			{
				if (eventSet.GetCommonness(CS$<>8__locals1.level) > 0f)
				{
					if (eventSet.Additive)
					{
						additiveSet = eventSet;
					}
					else
					{
						if (initialEventSet != null)
						{
							continue;
						}
						initialEventSet = eventSet;
					}
					CS$<>8__locals1.<StartRound>g__AddSet|2(eventSet);
				}
			}
			if (initialEventSet == null)
			{
				initialEventSet = this.SelectRandomEvents(EventSet.Prefabs.ToList<EventSet>(), new bool?(playingCampaign), this.random);
			}
			if (initialEventSet != null && initialEventSet.Additive)
			{
				additiveSet = initialEventSet;
				initialEventSet = this.SelectRandomEvents((from e in EventSet.Prefabs
				where !e.Additive
				select e).ToList<EventSet>(), new bool?(playingCampaign), this.random);
			}
			if (initialEventSet != null)
			{
				CS$<>8__locals1.<StartRound>g__AddSet|2(initialEventSet);
			}
			if (additiveSet != null)
			{
				CS$<>8__locals1.<StartRound>g__AddSet|2(additiveSet);
			}
			Level level2 = CS$<>8__locals1.level;
			LevelData levelData = (level2 != null) ? level2.LevelData : null;
			bool flag;
			if (levelData == null || levelData.Type != LevelData.LevelType.Outpost)
			{
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.GameMode : null) is TestGameMode)
				{
					Submarine mainSub = Submarine.MainSub;
					if (mainSub == null)
					{
						flag = false;
					}
					else
					{
						SubmarineInfo info = mainSub.Info;
						flag = (((info != null) ? new SubmarineType?(info.Type) : null).GetValueOrDefault() == SubmarineType.Outpost);
					}
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			bool isOutpostLevel = flag;
			if (isOutpostLevel)
			{
				Level level3 = CS$<>8__locals1.level;
				bool? flag2;
				if (level3 == null)
				{
					flag2 = null;
				}
				else
				{
					Location startLocation = level3.StartLocation;
					flag2 = ((startLocation != null) ? new bool?(startLocation.Connections.Any((LocationConnection c) => c.Locked && CS$<>8__locals1.level.StartLocation.MapPosition.X < c.OtherLocation(CS$<>8__locals1.level.StartLocation).MapPosition.X)) : null);
				}
				bool? flag3 = flag2;
				if (flag3.GetValueOrDefault())
				{
					EventPrefab unlockPathEventPrefab = EventPrefab.GetUnlockPathEvent(CS$<>8__locals1.level.LevelData.Biome.Identifier, CS$<>8__locals1.level.StartLocation.Faction);
					if (unlockPathEventPrefab != null)
					{
						Event newEvent = unlockPathEventPrefab.CreateInstance(this.RandomSeed);
						this.activeEvents.Add(newEvent);
					}
					else
					{
						CS$<>8__locals1.level.StartLocation.Connections.ForEach(delegate(LocationConnection c)
						{
							c.Locked = false;
						});
					}
				}
				Level level4 = CS$<>8__locals1.level;
				Submarine outpost = ((level4 != null) ? level4.StartOutpost : null) ?? Submarine.MainSub;
				NetworkMember networkMember = GameMain.NetworkMember;
				if ((networkMember == null || !networkMember.IsClient) && outpost != null)
				{
					foreach (Identifier eventTag in outpost.Info.TriggerOutpostMissionEvents)
					{
						EventPrefab eventPrefab = EventPrefab.FindEventPrefab(Identifier.Empty, eventTag, outpost.ContentPackage);
						if (eventPrefab == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Outpost ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(outpost.Info.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral(" failed to trigger an event (tag: ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventTag);
							defaultInterpolatedStringHandler.AppendLiteral(").");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, outpost.ContentPackage, false, false);
						}
						else
						{
							Event newEvent2 = eventPrefab.CreateInstance(this.RandomSeed);
							this.ActivateEvent(newEvent2);
						}
					}
				}
			}
			Level level5 = CS$<>8__locals1.level;
			if (((level5 != null) ? level5.LevelData : null) != null)
			{
				CS$<>8__locals1.<StartRound>g__RegisterNonRepeatableChildEvents|3(initialEventSet);
			}
			for (;;)
			{
				Identifier id;
				if (!this.QueuedEventsForNextRound.TryDequeue(out id))
				{
					break;
				}
				EventPrefab eventPrefab2 = EventSet.GetEventPrefab(id) ?? (from e in EventSet.GetAllEventPrefabs()
				where e.Tags.Contains(id)
				select e).GetRandomUnsynced<EventPrefab>();
				if (eventPrefab2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(80, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in EventManager.StartRound - could not find an event with the identifier ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(id);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				}
				else
				{
					Event ev = eventPrefab2.CreateInstance(this.RandomSeed);
					if (ev != null)
					{
						this.QueuedEvents.Enqueue(ev);
					}
				}
			}
			this.PreloadContent(this.GetFilesToPreload());
			this.roundDuration = 0f;
			this.eventsInitialized = false;
			this.isCrewAway = false;
			this.crewAwayDuration = 0f;
			this.crewAwayResetTimer = 0f;
			this.intensityUpdateTimer = 0f;
			this.CalculateCurrentIntensity(0f);
			this.currentIntensity = (this.musicIntensity = this.targetIntensity);
			this.eventCoolDown = 0f;
			this.CumulativeMonsterStrengthMain = 0f;
			this.CumulativeMonsterStrengthRuins = 0f;
			this.CumulativeMonsterStrengthWrecks = 0f;
			this.CumulativeMonsterStrengthCaves = 0f;
			this.distanceTraveled = 0f;
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x00065A54 File Offset: 0x00063C54
		public void ActivateEvent(Event newEvent)
		{
			this.activeEvents.Add(newEvent);
			newEvent.Init(null);
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x00065A69 File Offset: 0x00063C69
		public void ClearEvents()
		{
			this.activeEvents.Clear();
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x00065A78 File Offset: 0x00063C78
		private void SelectSettings()
		{
			if (!EventManagerSettings.Prefabs.Any<EventManagerSettings>())
			{
				throw new InvalidOperationException("Could not select EventManager settings (no settings loaded).");
			}
			EventManagerSettings[] orderedByDifficulty = EventManagerSettings.OrderedByDifficulty.ToArray<EventManagerSettings>();
			if (this.level != null)
			{
				float extraDifficulty = 0f;
				CampaignMode campaign = GameMain.GameSession.Campaign;
				if (((campaign != null) ? campaign.Settings : null) != null)
				{
					extraDifficulty = GameMain.GameSession.Campaign.Settings.ExtraEventManagerDifficulty;
				}
				float modifiedDifficulty = Math.Clamp(this.level.Difficulty + extraDifficulty, 0f, 100f);
				EventManagerSettings[] suitableSettings = (from s in EventManagerSettings.OrderedByDifficulty
				where modifiedDifficulty >= s.MinLevelDifficulty && modifiedDifficulty <= s.MaxLevelDifficulty
				select s).ToArray<EventManagerSettings>();
				if (suitableSettings.Length == 0)
				{
					DebugConsole.ThrowError("No suitable event manager settings found for the selected level (difficulty " + this.level.Difficulty.ToString() + ")", null, null, false, false);
					this.settings = orderedByDifficulty.GetRandom(Rand.RandSync.ServerAndClient);
				}
				else
				{
					this.settings = suitableSettings.GetRandom(Rand.RandSync.ServerAndClient);
				}
				if (this.settings != null)
				{
					this.eventThreshold = this.settings.DefaultEventThreshold;
				}
				return;
			}
			if (GameMain.GameSession.GameMode is TestGameMode)
			{
				this.settings = orderedByDifficulty.GetRandom(Rand.RandSync.ServerAndClient);
				if (this.settings != null)
				{
					this.eventThreshold = this.settings.DefaultEventThreshold;
				}
				return;
			}
			throw new InvalidOperationException("Could not select EventManager settings (level not set).");
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x00065BCE File Offset: 0x00063DCE
		public IEnumerable<ContentFile> GetFilesToPreload()
		{
			EventManager.<GetFilesToPreload>d__91 <GetFilesToPreload>d__ = new EventManager.<GetFilesToPreload>d__91(-2);
			<GetFilesToPreload>d__.<>4__this = this;
			return <GetFilesToPreload>d__;
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x00065BE0 File Offset: 0x00063DE0
		public void PreloadContent(IEnumerable<ContentFile> contentFiles)
		{
			List<ContentFile> filesToPreload = contentFiles.ToList<ContentFile>();
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.WreckAI != null)
				{
					if (!sub.WreckAI.Config.DefensiveAgent.IsEmpty)
					{
						CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(sub.WreckAI.Config.DefensiveAgent);
						if (prefab != null && !filesToPreload.Any((ContentFile f) => f.Path == prefab.FilePath))
						{
							filesToPreload.Add(prefab.ContentFile);
						}
					}
					foreach (Item item in Item.ItemList)
					{
						if (item.Submarine == sub)
						{
							foreach (ItemComponent component in item.Components)
							{
								if (component.statusEffectLists != null)
								{
									foreach (List<StatusEffect> statusEffectList in component.statusEffectLists.Values)
									{
										foreach (StatusEffect statusEffect in statusEffectList)
										{
											foreach (StatusEffect.CharacterSpawnInfo spawnInfo in statusEffect.SpawnCharacters)
											{
												CharacterPrefab prefab2 = CharacterPrefab.FindBySpeciesName(spawnInfo.SpeciesName);
												if (prefab2 != null && !filesToPreload.Contains(prefab2.ContentFile))
												{
													filesToPreload.Add(prefab2.ContentFile);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			foreach (ContentFile file in filesToPreload)
			{
				file.Preload(new Action<Sprite>(this.preloadedSprites.Add));
			}
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x00065EE0 File Offset: 0x000640E0
		public void TriggerOnEndRoundActions()
		{
			foreach (Event ev in this.activeEvents)
			{
				ScriptedEvent scriptedEvent = ev as ScriptedEvent;
				if (scriptedEvent != null)
				{
					OnRoundEndAction onRoundEndAction = scriptedEvent.OnRoundEndAction;
					if (onRoundEndAction != null)
					{
						onRoundEndAction.Update(1f);
					}
				}
			}
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x00065F50 File Offset: 0x00064150
		public void EndRound()
		{
			this.pendingEventSets.Clear();
			this.selectedEvents.Clear();
			this.activeEvents.Clear();
			this.QueuedEvents.Clear();
			this.finishedEvents.Clear();
			this.nonRepeatableEvents.Clear();
			this.preloadedSprites.ForEach(delegate(Sprite s)
			{
				s.Remove();
			});
			this.preloadedSprites.Clear();
			this.timeStamps.Clear();
			this.pathFinder = null;
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x00065FE8 File Offset: 0x000641E8
		public void StoreEventDataAtRoundEnd(bool registerFinishedOnly = false)
		{
			Level level = this.level;
			if (((level != null) ? level.LevelData : null) == null)
			{
				return;
			}
			if (this.level.LevelData.Type == LevelData.LevelType.Outpost)
			{
				if (registerFinishedOnly)
				{
					foreach (Event finishedEvent in this.finishedEvents)
					{
						EventSet parentSet = finishedEvent.ParentSet;
						if (parentSet != null)
						{
							if (parentSet.Exhaustible)
							{
								this.level.LevelData.ExhaustEventSet(parentSet);
							}
							if (!this.level.LevelData.FinishedEvents.TryAdd(parentSet, 1))
							{
								Dictionary<EventSet, int> dictionary = this.level.LevelData.FinishedEvents;
								EventSet key = parentSet;
								dictionary[key]++;
							}
						}
					}
				}
				this.level.LevelData.EventHistory.AddRange(from e in this.selectedEvents.Values.SelectMany((List<Event> v) => v)
				select e.Prefab.Identifier into eventId
				where base.<StoreEventDataAtRoundEnd>g__Register|4(eventId) && !this.level.LevelData.EventHistory.Contains(eventId)
				select eventId);
				if (this.level.LevelData.EventHistory.Count > 20)
				{
					this.level.LevelData.EventHistory.RemoveRange(0, this.level.LevelData.EventHistory.Count - 20);
				}
			}
			this.level.LevelData.NonRepeatableEvents.AddRange(from eventId in this.nonRepeatableEvents
			where base.<StoreEventDataAtRoundEnd>g__Register|4(eventId) && !this.level.LevelData.NonRepeatableEvents.Contains(eventId)
			select eventId);
			if (!registerFinishedOnly)
			{
				this.level.LevelData.FinishedEvents.Clear();
			}
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x000661EC File Offset: 0x000643EC
		public void SkipEventCooldown()
		{
			this.eventCoolDown = 0f;
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x000661FC File Offset: 0x000643FC
		private float CalculateCommonness(EventPrefab eventPrefab, float baseCommonness)
		{
			if (this.level.LevelData.NonRepeatableEvents.Contains(eventPrefab.Identifier))
			{
				return 0f;
			}
			float retVal = baseCommonness;
			if (this.level.LevelData.EventHistory.Contains(eventPrefab.Identifier))
			{
				retVal *= 0.1f;
			}
			return retVal;
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x00066254 File Offset: 0x00064454
		private void CreateEvents(EventSet eventSet)
		{
			this.selectedEvents.Remove(eventSet);
			if (this.level == null)
			{
				return;
			}
			if (this.level.LevelData.HasHuntingGrounds && eventSet.DisableInHuntingGrounds)
			{
				return;
			}
			if (eventSet.Exhaustible && this.level.LevelData.IsEventSetExhausted(eventSet))
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Loading event set ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventSet.Identifier);
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.LightBlue), true);
			int applyCount = 1;
			List<Func<Level.InterestingPosition, bool>> spawnPosFilter = new List<Func<Level.InterestingPosition, bool>>();
			if (eventSet.PerRuin)
			{
				applyCount = this.level.Ruins.Count;
				using (List<Ruin>.Enumerator enumerator = this.level.Ruins.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Ruin ruin = enumerator.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Ruin == ruin);
					}
					goto IL_1F6;
				}
			}
			if (eventSet.PerCave)
			{
				applyCount = this.level.Caves.Count;
				using (List<Level.Cave>.Enumerator enumerator2 = this.level.Caves.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Level.Cave cave = enumerator2.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Cave == cave);
					}
					goto IL_1F6;
				}
			}
			if (eventSet.PerWreck)
			{
				IEnumerable<Submarine> wrecks = from s in Submarine.Loaded
				where s.Info.IsWreck && (s.WreckAI == null || !s.WreckAI.IsAlive)
				select s;
				applyCount = wrecks.Count<Submarine>();
				using (IEnumerator<Submarine> enumerator3 = wrecks.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Submarine wreck = enumerator3.Current;
						spawnPosFilter.Add((Level.InterestingPosition pos) => pos.Submarine == wreck);
					}
				}
			}
			IL_1F6:
			foreach (EventSet.SubEventPrefab subEventPrefab in eventSet.EventPrefabs)
			{
				foreach (Identifier missingId in subEventPrefab.GetMissingIdentifiers())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(81, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in event set \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(eventSet.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\" (");
					ContentFile contentFile = eventSet.ContentFile;
					string text;
					if (contentFile == null)
					{
						text = null;
					}
					else
					{
						ContentPackage contentPackage = contentFile.ContentPackage;
						text = ((contentPackage != null) ? contentPackage.Name : null);
					}
					defaultInterpolatedStringHandler2.AppendFormatted(text ?? "null");
					defaultInterpolatedStringHandler2.AppendLiteral(") - could not find an event prefab with the identifier \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(missingId);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, eventSet.ContentPackage, false, false);
				}
			}
			EventSet.SubEventPrefab[] suitablePrefabSubsets = (from e in eventSet.EventPrefabs
			where EventManager.IsFactionSuitable(e.Faction, this.level) && e.EventPrefabs.Any((EventPrefab ep) => EventManager.IsSuitable(ep, this.level))
			select e).ToArray<EventSet.SubEventPrefab>();
			for (int i = 0; i < applyCount; i++)
			{
				if (eventSet.ChooseRandom)
				{
					if (suitablePrefabSubsets.Any<EventSet.SubEventPrefab>())
					{
						List<EventSet.SubEventPrefab> unusedEvents = suitablePrefabSubsets.ToList<EventSet.SubEventPrefab>();
						int eventCount = eventSet.GetEventCount(this.level);
						int j = 0;
						while (j < eventCount && !unusedEvents.All((EventSet.SubEventPrefab e) => e.EventPrefabs.All((EventPrefab p) => this.CalculateCommonness(p, e.Commonness) <= 0f)))
						{
							EventSet.SubEventPrefab subEventPrefab2 = ToolBox.SelectWeightedRandom<EventSet.SubEventPrefab>(unusedEvents, (EventSet.SubEventPrefab e) => e.EventPrefabs.Max((EventPrefab p) => this.CalculateCommonness(p, e.Commonness)), this.random);
							EventSet.SubEventPrefab subEventPrefab3 = subEventPrefab2;
							IEnumerable<EventPrefab> enumerable;
							float num;
							float num2;
							subEventPrefab3.Deconstruct(out enumerable, out num, out num2);
							IEnumerable<EventPrefab> eventPrefabs = enumerable;
							float probability = num2;
							if (eventPrefabs != null && this.random.NextDouble() <= (double)probability)
							{
								EventPrefab eventPrefab = ToolBox.SelectWeightedRandom<EventPrefab>(from e in eventPrefabs
								where EventManager.IsSuitable(e, this.level)
								select e, (EventPrefab e) => e.Commonness, this.random);
								Event newEvent = eventPrefab.CreateInstance(this.RandomSeed);
								if (newEvent != null)
								{
									if (i < spawnPosFilter.Count)
									{
										newEvent.SpawnPosFilter = spawnPosFilter[i];
									}
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
									defaultInterpolatedStringHandler3.AppendLiteral("Initialized event ");
									defaultInterpolatedStringHandler3.AppendFormatted<Event>(newEvent);
									DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, true);
									if (!this.selectedEvents.ContainsKey(eventSet))
									{
										this.selectedEvents.Add(eventSet, new List<Event>());
									}
									this.selectedEvents[eventSet].Add(newEvent);
									unusedEvents.Remove(subEventPrefab2);
								}
							}
							j++;
						}
					}
					if (eventSet.ChildSets.Any<EventSet>())
					{
						int setCount = eventSet.SubSetCount;
						if (setCount > 1)
						{
							List<EventSet> unusedSets = eventSet.ChildSets.ToList<EventSet>();
							for (int k = 0; k < setCount; k++)
							{
								IReadOnlyList<EventSet> eventSets = unusedSets;
								Random random = this.random;
								EventSet newEventSet = this.SelectRandomEvents(eventSets, null, random);
								if (newEventSet == null)
								{
									break;
								}
								unusedSets.Remove(newEventSet);
								this.CreateEvents(newEventSet);
							}
						}
						else
						{
							IReadOnlyList<EventSet> eventSets2 = eventSet.ChildSets;
							Random random = this.random;
							EventSet newEventSet2 = this.SelectRandomEvents(eventSets2, null, random);
							if (newEventSet2 != null)
							{
								this.CreateEvents(newEventSet2);
							}
						}
					}
				}
				else
				{
					foreach (EventSet.SubEventPrefab subEventPrefab3 in suitablePrefabSubsets)
					{
						IEnumerable<EventPrefab> enumerable;
						float num;
						float num2;
						subEventPrefab3.Deconstruct(out enumerable, out num2, out num);
						IEnumerable<EventPrefab> eventPrefabs2 = enumerable;
						float probability2 = num;
						if (this.random.NextDouble() <= (double)probability2)
						{
							EventPrefab eventPrefab2 = ToolBox.SelectWeightedRandom<EventPrefab>(from e in eventPrefabs2
							where EventManager.IsSuitable(e, this.level)
							select e, (EventPrefab e) => e.Commonness, this.random);
							Event newEvent2 = eventPrefab2.CreateInstance(this.RandomSeed);
							if (newEvent2 != null)
							{
								if (i < spawnPosFilter.Count)
								{
									newEvent2.SpawnPosFilter = spawnPosFilter[i];
								}
								if (!this.selectedEvents.ContainsKey(eventSet))
								{
									this.selectedEvents.Add(eventSet, new List<Event>());
								}
								this.selectedEvents[eventSet].Add(newEvent2);
							}
						}
					}
					Location location = this.GetEventLocation();
					foreach (EventSet childEventSet in eventSet.ChildSets)
					{
						if (EventManager.IsValidForLevel(childEventSet, this.level) && this.IsValidForLocation(childEventSet, location))
						{
							this.CreateEvents(childEventSet);
						}
					}
				}
			}
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x00066930 File Offset: 0x00064B30
		private IEnumerable<EventSet> GetAllowedEventSets(IReadOnlyList<EventSet> eventSets, bool? requireCampaignSet = null)
		{
			EventManager.<>c__DisplayClass99_0 CS$<>8__locals1 = new EventManager.<>c__DisplayClass99_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.level == null)
			{
				return Enumerable.Empty<EventSet>();
			}
			IEnumerable<EventSet> allowedEventSets = from set in eventSets
			where EventManager.IsValidForLevel(set, CS$<>8__locals1.<>4__this.level)
			select set;
			if (requireCampaignSet != null)
			{
				if (requireCampaignSet.Value)
				{
					if (allowedEventSets.Any((EventSet es) => es.IsCampaignSet))
					{
						allowedEventSets = from es in allowedEventSets
						where es.IsCampaignSet
						select es;
					}
					else
					{
						DebugConsole.AddWarning("No campaign event sets available. Using a non-campaign-specific set instead.", null);
					}
				}
				else
				{
					allowedEventSets = from es in allowedEventSets
					where !es.IsCampaignSet
					select es;
				}
			}
			CS$<>8__locals1.location = this.GetEventLocation();
			allowedEventSets = from set in allowedEventSets
			where CS$<>8__locals1.<>4__this.IsValidForLocation(set, CS$<>8__locals1.location)
			select set;
			allowedEventSets = allowedEventSets.Where(delegate(EventSet set)
			{
				if (!set.CampaignTutorialOnly)
				{
					return true;
				}
				if (GameMain.IsSingleplayer)
				{
					GameSession gameSession3 = GameMain.GameSession;
					CampaignSettings campaignSettings;
					if (gameSession3 == null)
					{
						campaignSettings = null;
					}
					else
					{
						CampaignMode campaign = gameSession3.Campaign;
						campaignSettings = ((campaign != null) ? campaign.Settings : null);
					}
					CampaignSettings campaignSettings2 = campaignSettings;
					return campaignSettings2 != null && campaignSettings2.TutorialEnabled;
				}
				return false;
			});
			EventManager.<>c__DisplayClass99_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameSession gameSession = GameMain.GameSession;
			int? discoveryIndex;
			if (gameSession == null)
			{
				discoveryIndex = null;
			}
			else
			{
				Map map = gameSession.Map;
				discoveryIndex = ((map != null) ? map.GetDiscoveryIndex(CS$<>8__locals1.location) : null);
			}
			CS$<>8__locals2.discoveryIndex = discoveryIndex;
			EventManager.<>c__DisplayClass99_0 CS$<>8__locals3 = CS$<>8__locals1;
			GameSession gameSession2 = GameMain.GameSession;
			int? visitIndex;
			if (gameSession2 == null)
			{
				visitIndex = null;
			}
			else
			{
				Map map2 = gameSession2.Map;
				visitIndex = ((map2 != null) ? map2.GetVisitIndex(CS$<>8__locals1.location, false) : null);
			}
			CS$<>8__locals3.visitIndex = visitIndex;
			if (CS$<>8__locals1.discoveryIndex != null)
			{
				int? num = CS$<>8__locals1.discoveryIndex;
				int num2 = 0;
				if ((num.GetValueOrDefault() >= num2 & num != null) && allowedEventSets.Any(delegate(EventSet set)
				{
					int forceAtDiscoveredNr = set.ForceAtDiscoveredNr;
					int? discoveryIndex2 = CS$<>8__locals1.discoveryIndex;
					return forceAtDiscoveredNr == discoveryIndex2.GetValueOrDefault() & discoveryIndex2 != null;
				}))
				{
					return allowedEventSets.Where(delegate(EventSet set)
					{
						int forceAtDiscoveredNr = set.ForceAtDiscoveredNr;
						int? discoveryIndex2 = CS$<>8__locals1.discoveryIndex;
						return forceAtDiscoveredNr == discoveryIndex2.GetValueOrDefault() & discoveryIndex2 != null;
					});
				}
			}
			if (CS$<>8__locals1.visitIndex != null)
			{
				int? num = CS$<>8__locals1.visitIndex;
				int num2 = 0;
				if ((num.GetValueOrDefault() >= num2 & num != null) && allowedEventSets.Any(delegate(EventSet set)
				{
					int forceAtVisitedNr = set.ForceAtVisitedNr;
					int? visitIndex2 = CS$<>8__locals1.visitIndex;
					return forceAtVisitedNr == visitIndex2.GetValueOrDefault() & visitIndex2 != null;
				}))
				{
					return allowedEventSets.Where(delegate(EventSet set)
					{
						int forceAtVisitedNr = set.ForceAtVisitedNr;
						int? visitIndex2 = CS$<>8__locals1.visitIndex;
						return forceAtVisitedNr == visitIndex2.GetValueOrDefault() & visitIndex2 != null;
					});
				}
			}
			return from set in allowedEventSets
			where set.ForceAtDiscoveredNr < 0 && set.ForceAtVisitedNr < 0
			select set;
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x00066B90 File Offset: 0x00064D90
		private EventSet SelectRandomEvents(IReadOnlyList<EventSet> eventSets, bool? requireCampaignSet = null, Random random = null)
		{
			IEnumerable<EventSet> allowedEventSets = this.GetAllowedEventSets(eventSets, requireCampaignSet);
			if (allowedEventSets.Count<EventSet>() == 1)
			{
				return allowedEventSets.First<EventSet>();
			}
			Random rand = random ?? new MTRandom(ToolBox.StringToInt(this.level.Seed));
			float totalCommonness = allowedEventSets.Sum((EventSet e) => e.GetCommonness(this.level));
			float randomNumber = (float)rand.NextDouble();
			randomNumber *= totalCommonness;
			foreach (EventSet eventSet in allowedEventSets)
			{
				float commonness = eventSet.GetCommonness(this.level);
				if (randomNumber <= commonness)
				{
					return eventSet;
				}
				randomNumber -= commonness;
			}
			return null;
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x00066C50 File Offset: 0x00064E50
		public static bool IsSuitable(EventPrefab e, Level level)
		{
			return EventManager.IsLevelSuitable(e, level) && EventManager.IsFactionSuitable(e.Faction, level);
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x00066C6C File Offset: 0x00064E6C
		public static bool IsLevelSuitable(EventPrefab e, Level level)
		{
			if (!e.BiomeIdentifier.IsEmpty)
			{
				Identifier? identifier = new Identifier?(e.BiomeIdentifier);
				LevelData levelData = level.LevelData;
				Identifier? identifier2;
				Identifier? identifier3;
				if (levelData == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					Biome biome = levelData.Biome;
					if (biome == null)
					{
						identifier2 = null;
						identifier3 = identifier2;
					}
					else
					{
						identifier3 = new Identifier?(biome.Identifier);
					}
				}
				identifier2 = identifier3;
				if (!(identifier == identifier2))
				{
					return false;
				}
			}
			if ((e.RequiredLayer.IsEmpty || Submarine.LayerExistsInAnySub(e.RequiredLayer)) && (e.RequiredSpawnPointTag.IsEmpty || WayPoint.WayPointList.Any((WayPoint wp) => wp.Tags.Contains(e.RequiredSpawnPointTag))))
			{
				return !level.LevelData.NonRepeatableEvents.Contains(e.Identifier);
			}
			return false;
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x00066D58 File Offset: 0x00064F58
		private static bool IsFactionSuitable(Identifier factionId, Level level)
		{
			if (!factionId.IsEmpty)
			{
				Identifier? identifier = new Identifier?(factionId);
				Location startLocation = level.StartLocation;
				Identifier? identifier2;
				Identifier? identifier3;
				if (startLocation == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					Faction faction = startLocation.Faction;
					if (faction == null)
					{
						identifier2 = null;
						identifier3 = identifier2;
					}
					else
					{
						identifier3 = new Identifier?(faction.Prefab.Identifier);
					}
				}
				identifier2 = identifier3;
				if (!(identifier == identifier2))
				{
					Identifier? identifier4 = new Identifier?(factionId);
					Location startLocation2 = level.StartLocation;
					Identifier? identifier5;
					Identifier? identifier6;
					if (startLocation2 == null)
					{
						identifier5 = null;
						identifier6 = identifier5;
					}
					else
					{
						Faction secondaryFaction = startLocation2.SecondaryFaction;
						if (secondaryFaction == null)
						{
							identifier5 = null;
							identifier6 = identifier5;
						}
						else
						{
							identifier6 = new Identifier?(secondaryFaction.Prefab.Identifier);
						}
					}
					identifier5 = identifier6;
					return identifier4 == identifier5;
				}
			}
			return true;
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x00066E08 File Offset: 0x00065008
		private static bool IsValidForLevel(EventSet eventSet, Level level)
		{
			return level.IsAllowedDifficulty(eventSet.MinLevelDifficulty, eventSet.MaxLevelDifficulty) && eventSet.LevelType.HasFlag(level.LevelData.Type) && (eventSet.RequiredLayer.IsEmpty || Submarine.LayerExistsInAnySub(eventSet.RequiredLayer)) && (eventSet.RequiredSpawnPointTag.IsEmpty || WayPoint.WayPointList.Any((WayPoint wp) => wp.Tags.Contains(eventSet.RequiredSpawnPointTag))) && (eventSet.BiomeIdentifier.IsEmpty || eventSet.BiomeIdentifier == level.LevelData.Biome.Identifier);
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x00066EF4 File Offset: 0x000650F4
		private bool IsValidForLocation(EventSet eventSet, Location location)
		{
			if (location == null)
			{
				return true;
			}
			if (!eventSet.Faction.IsEmpty)
			{
				Identifier? identifier = new Identifier?(eventSet.Faction);
				Faction faction = location.Faction;
				Identifier? identifier2;
				Identifier? identifier3;
				if (faction == null)
				{
					identifier2 = null;
					identifier3 = identifier2;
				}
				else
				{
					identifier3 = new Identifier?(faction.Prefab.Identifier);
				}
				identifier2 = identifier3;
				if (identifier != identifier2)
				{
					Identifier? identifier4 = new Identifier?(eventSet.Faction);
					Faction secondaryFaction = location.SecondaryFaction;
					Identifier? identifier5;
					Identifier? identifier6;
					if (secondaryFaction == null)
					{
						identifier5 = null;
						identifier6 = identifier5;
					}
					else
					{
						identifier6 = new Identifier?(secondaryFaction.Prefab.Identifier);
					}
					identifier5 = identifier6;
					if (identifier4 != identifier5)
					{
						return false;
					}
				}
			}
			LocationType locationType = location.GetLocationTypeToDisplay();
			bool includeGenericEvents = this.level.Type == LevelData.LevelType.LocationConnection || !locationType.IgnoreGenericEvents;
			if (includeGenericEvents && new ImmutableArray<Identifier>?(eventSet.LocationTypeIdentifiers) == null)
			{
				return true;
			}
			if (new ImmutableArray<Identifier>?(eventSet.LocationTypeIdentifiers) == null)
			{
				return false;
			}
			bool hasMatchingEventLocationId = !locationType.EventLocationType.IsEmpty && eventSet.LocationTypeIdentifiers.Contains(locationType.EventLocationType);
			bool hasMatchingLocationId = eventSet.LocationTypeIdentifiers.Contains(locationType.Identifier);
			return hasMatchingEventLocationId || hasMatchingLocationId;
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x00067032 File Offset: 0x00065232
		private Location GetEventLocation()
		{
			GameSession gameSession = GameMain.GameSession;
			Location location;
			if (gameSession == null)
			{
				location = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign == null)
				{
					location = null;
				}
				else
				{
					Map map = campaign.Map;
					location = ((map != null) ? map.CurrentLocation : null);
				}
			}
			Location result;
			if ((result = location) == null)
			{
				Level level = this.level;
				if (level == null)
				{
					return null;
				}
				result = level.StartLocation;
			}
			return result;
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x00067074 File Offset: 0x00065274
		private bool CanStartEventSet(EventSet eventSet)
		{
			if (!eventSet.AllowAtStart)
			{
				ISpatialEntity refEntity = EventManager.GetRefEntity(false);
				float distFromStart = (float)Math.Sqrt(MathUtils.LineSegmentToPointDistanceSquared(this.level.StartExitPosition.ToPoint(), this.level.StartPosition.ToPoint(), refEntity.WorldPosition.ToPoint()));
				float distFromEnd = (float)Math.Sqrt(MathUtils.LineSegmentToPointDistanceSquared(this.level.EndExitPosition.ToPoint(), this.level.EndPosition.ToPoint(), refEntity.WorldPosition.ToPoint()));
				if (distFromStart * Physics.DisplayToRealWorldRatio < 50f || distFromEnd * Physics.DisplayToRealWorldRatio < 50f)
				{
					return false;
				}
			}
			return (!eventSet.DelayWhenCrewAway || ((!this.isCrewAway || this.crewAwayDuration >= this.settings.FreezeDurationWhenCrewAway) && this.crewAwayResetTimer <= 0f)) && ((Submarine.MainSub != null && this.distanceTraveled >= eventSet.MinDistanceTraveled) || this.roundDuration >= eventSet.MinMissionTime) && this.CurrentIntensity >= eventSet.MinIntensity && this.CurrentIntensity <= eventSet.MaxIntensity;
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x000671A8 File Offset: 0x000653A8
		public void Update(float deltaTime)
		{
			if (!this.Enabled)
			{
				return;
			}
			CampaignMode campaign = GameMain.GameSession.Campaign;
			if (campaign != null && campaign.DisableEvents)
			{
				return;
			}
			if (!this.eventsInitialized)
			{
				foreach (EventSet eventSet2 in this.selectedEvents.Keys)
				{
					foreach (Event ev3 in this.selectedEvents[eventSet2])
					{
						ev3.Init(eventSet2);
					}
				}
				this.eventsInitialized = true;
			}
			this.CalculateCurrentIntensity(deltaTime);
			if (this.isClient)
			{
				return;
			}
			this.roundDuration += deltaTime;
			if (this.settings == null)
			{
				DebugConsole.ThrowError("Event settings not set before updating EventManager. Attempting to select...", null, null, false, false);
				this.SelectSettings();
				if (this.settings == null)
				{
					DebugConsole.ThrowError("Could not select EventManager settings. Disabling EventManager for the round...", null, null, false, false);
					this.Enabled = false;
					return;
				}
			}
			if (this.IsCrewAway())
			{
				this.isCrewAway = true;
				this.crewAwayResetTimer = 60f;
				this.crewAwayDuration += deltaTime;
			}
			else if (this.crewAwayResetTimer > 0f)
			{
				this.isCrewAway = false;
				this.crewAwayResetTimer -= deltaTime;
			}
			else
			{
				this.isCrewAway = false;
				this.crewAwayDuration = 0f;
				this.eventThreshold += this.settings.EventThresholdIncrease * deltaTime;
				this.eventThreshold = Math.Min(this.eventThreshold, 1f);
				this.eventCoolDown -= deltaTime;
			}
			this.calculateDistanceTraveledTimer -= deltaTime;
			if (this.calculateDistanceTraveledTimer <= 0f)
			{
				this.distanceTraveled = this.CalculateDistanceTraveled();
				this.calculateDistanceTraveledTimer = 5f;
			}
			bool recheck = false;
			do
			{
				recheck = false;
				for (int i = this.pendingEventSets.Count - 1; i >= 0; i--)
				{
					EventSet eventSet = this.pendingEventSets[i];
					if ((this.eventCoolDown <= 0f || eventSet.IgnoreCoolDown) && (this.currentIntensity <= this.eventThreshold || eventSet.IgnoreIntensity) && this.CanStartEventSet(eventSet))
					{
						this.pendingEventSets.RemoveAt(i);
						if (this.selectedEvents.ContainsKey(eventSet))
						{
							Action <>9__1;
							foreach (Event ev2 in this.selectedEvents[eventSet])
							{
								this.activeEvents.Add(ev2);
								this.eventThreshold = this.settings.DefaultEventThreshold;
								if (eventSet.TriggerEventCooldown)
								{
									if (this.selectedEvents[eventSet].Any((Event e) => e.Prefab.TriggerEventCooldown))
									{
										this.eventCoolDown = this.settings.EventCooldown;
									}
								}
								if (eventSet.ResetTime > 0f)
								{
									Event @event = ev2;
									Action value;
									if ((value = <>9__1) == null)
									{
										value = (<>9__1 = delegate()
										{
											this.pendingEventSets.Add(eventSet);
											this.CreateEvents(eventSet);
											foreach (Event newEvent in this.selectedEvents[eventSet])
											{
												if (!newEvent.Initialized)
												{
													newEvent.Init(eventSet);
												}
											}
										});
									}
									@event.Finished += value;
								}
							}
						}
						foreach (EventSet childEventSet in eventSet.ChildSets)
						{
							this.pendingEventSets.Add(childEventSet);
							recheck = true;
						}
					}
				}
			}
			while (recheck);
			using (List<Event>.Enumerator enumerator5 = this.activeEvents.GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					Event ev = enumerator5.Current;
					if (!ev.IsFinished)
					{
						ev.Update(deltaTime);
					}
					else if (ev.Prefab != null && !this.finishedEvents.Any((Event e) => e.Prefab == ev.Prefab))
					{
						Level level = this.level;
						if (((level != null) ? level.LevelData : null) != null && this.level.LevelData.Type == LevelData.LevelType.Outpost && !this.level.LevelData.EventHistory.Contains(ev.Prefab.Identifier))
						{
							this.level.LevelData.EventHistory.Add(ev.Prefab.Identifier);
						}
						this.finishedEvents.Add(ev);
					}
				}
			}
			if (this.QueuedEvents.Count > 0)
			{
				this.activeEvents.Add(this.QueuedEvents.Dequeue());
			}
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x000676E0 File Offset: 0x000658E0
		public void EntitySpawned(Entity entity)
		{
			foreach (Event ev in this.activeEvents)
			{
				ScriptedEvent scriptedEvent = ev as ScriptedEvent;
				if (scriptedEvent != null)
				{
					scriptedEvent.EntitySpawned(entity);
				}
			}
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x00067740 File Offset: 0x00065940
		private void CalculateCurrentIntensity(float deltaTime)
		{
			this.intensityUpdateTimer -= deltaTime;
			if (this.intensityUpdateTimer > 0f)
			{
				return;
			}
			this.intensityUpdateTimer = 5f;
			this.avgCrewHealth = 0f;
			int characterCount = 0;
			foreach (Character character in Character.CharacterList)
			{
				if (!character.IsDead && character.TeamID != CharacterTeamType.FriendlyNPC && (character.AIController is HumanAIController || character.IsRemotePlayer))
				{
					this.avgCrewHealth += character.Vitality / character.MaxVitality * (character.IsUnconscious ? 0.5f : 1f);
					characterCount++;
				}
			}
			if (characterCount > 0)
			{
				this.avgCrewHealth /= (float)characterCount;
			}
			else
			{
				this.avgCrewHealth = 0.5f;
			}
			this.enemyDanger = 0f;
			this.monsterStrength = 0f;
			foreach (Character character2 in Character.CharacterList)
			{
				if (!character2.IsIncapacitated && !character2.IsHandcuffed && character2.Enabled && !character2.IsPet)
				{
					EnemyAIController enemyAI = character2.AIController as EnemyAIController;
					if (enemyAI != null)
					{
						if (!enemyAI.AIParams.StayInAbyss)
						{
							this.monsterStrength += enemyAI.CombatStrength;
						}
						if (Submarine.MainSub != null)
						{
							Hull currentHull = character2.CurrentHull;
							SubmarineInfo submarineInfo = (currentHull != null) ? currentHull.Submarine.Info : null;
							if (submarineInfo != null && submarineInfo.Type == SubmarineType.Player && (character2.CurrentHull.Submarine == Submarine.MainSub || Submarine.MainSub.DockedTo.Contains(character2.CurrentHull.Submarine)))
							{
								this.enemyDanger += enemyAI.CombatStrength / 500f;
								continue;
							}
						}
						AITarget selectedAiTarget = enemyAI.SelectedAiTarget;
						bool flag;
						if (selectedAiTarget == null)
						{
							flag = (null != null);
						}
						else
						{
							Entity entity = selectedAiTarget.Entity;
							flag = (((entity != null) ? entity.Submarine : null) != null);
						}
						if (flag)
						{
							this.enemyDanger += enemyAI.CombatStrength / 5000f;
						}
					}
					else
					{
						HumanAIController humanAi = character2.AIController as HumanAIController;
						if (humanAi != null && !character2.IsOnFriendlyTeam(CharacterTeamType.Team1) && character2.Submarine != null && Submarine.MainSub != null)
						{
							PhysicsBody physicsBody = character2.Submarine.PhysicsBody;
							if (physicsBody != null && physicsBody.BodyType == BodyType.Dynamic && Vector2.DistanceSquared(character2.Submarine.WorldPosition, Submarine.MainSub.WorldPosition) < 100000000f)
							{
								this.enemyDanger += 0.2f;
							}
						}
					}
				}
			}
			this.enemyDanger += this.monsterStrength / 5000f;
			this.enemyDanger = MathHelper.Clamp(this.enemyDanger, 0f, 1f);
			float holeCount = 0f;
			float waterAmount = 0f;
			float dryHullVolume = 0f;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine != null && hull.Submarine.Info.Type == SubmarineType.Player)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode)
					{
						if (hull.Submarine.TeamID != CharacterTeamType.Team1 && hull.Submarine.TeamID != CharacterTeamType.Team2)
						{
							continue;
						}
					}
					else if (hull.Submarine.TeamID != CharacterTeamType.Team1)
					{
						continue;
					}
					this.fireAmount += hull.FireSources.Sum((FireSource fs) => fs.Size.X);
					if (!hull.IsWetRoom)
					{
						foreach (Gap gap in hull.ConnectedGaps)
						{
							if (!gap.IsRoomToRoom)
							{
								holeCount += gap.Open;
							}
						}
						waterAmount += hull.WaterVolume;
						dryHullVolume += hull.Volume;
					}
				}
			}
			if (dryHullVolume > 0f)
			{
				this.floodingAmount = waterAmount / dryHullVolume;
			}
			this.avgHullIntegrity = MathHelper.Clamp(1f - holeCount / 10f, 0f, 1f);
			this.fireAmount = MathHelper.Clamp(this.fireAmount / 1000f, (this.fireAmount > 0f) ? 0.2f : 0f, 1f);
			if (this.floodingAmount < 0.1f)
			{
				this.floodingAmount = 0f;
			}
			else
			{
				this.floodingAmount *= 1.5f;
			}
			this.targetIntensity = (1f - this.avgCrewHealth + (1f - this.avgHullIntegrity) + this.floodingAmount) / 3f;
			this.targetIntensity += this.fireAmount * 0.5f;
			this.targetIntensity += this.enemyDanger;
			this.targetIntensity = MathHelper.Clamp(this.targetIntensity, 0f, 1f);
			if (this.targetIntensity > this.currentIntensity)
			{
				this.currentIntensity = Math.Min(this.currentIntensity + 0.19999999f, this.targetIntensity);
				this.musicIntensity = Math.Min(this.musicIntensity + 0.25f, this.targetIntensity);
				return;
			}
			this.currentIntensity = Math.Max(this.currentIntensity - 0.012499999f, this.targetIntensity);
			this.musicIntensity = Math.Max(this.musicIntensity - 0.25f, this.targetIntensity);
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x00067D90 File Offset: 0x00065F90
		private float CalculateDistanceTraveled()
		{
			if (this.level == null || this.pathFinder == null)
			{
				return 0f;
			}
			ISpatialEntity refEntity = EventManager.GetRefEntity(false);
			if (refEntity == null)
			{
				return 0f;
			}
			Vector2 target = ConvertUnits.ToSimUnits(this.level.EndPosition);
			SteeringPath steeringPath = this.pathFinder.FindPath(ConvertUnits.ToSimUnits(refEntity.WorldPosition), target, null, null, 0f, null, null, null, true, 0f);
			if (steeringPath.Unreachable || float.IsPositiveInfinity(this.totalPathLength))
			{
				return MathHelper.Clamp((refEntity.WorldPosition.X - this.level.StartPosition.X) / (this.level.EndPosition.X - this.level.StartPosition.X), 0f, 1f);
			}
			return MathHelper.Clamp(1f - steeringPath.TotalLength / this.totalPathLength, 0f, 1f);
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x00067E84 File Offset: 0x00066084
		public static ISpatialEntity GetRefEntity(bool acceptRemoteControlledSubs = false)
		{
			EventManager.<>c__DisplayClass113_0 CS$<>8__locals1;
			CS$<>8__locals1.acceptRemoteControlledSubs = acceptRemoteControlledSubs;
			CS$<>8__locals1.refEntity = Submarine.MainSub;
			if (Character.Controlled != null)
			{
				Submarine playerSub = Character.Controlled.Submarine;
				if (playerSub != null)
				{
					SubmarineInfo info = playerSub.Info;
					if (info != null && info.Type == SubmarineType.Player)
					{
						EventManager.<GetRefEntity>g__GetRefSubForCharacter|113_0(Character.Controlled, ref CS$<>8__locals1);
						goto IL_55;
					}
				}
				CS$<>8__locals1.refEntity = Character.Controlled;
			}
			IL_55:
			return CS$<>8__locals1.refEntity;
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00067EEC File Offset: 0x000660EC
		private bool IsCrewAway()
		{
			return Character.Controlled != null && this.IsCharacterAway(Character.Controlled);
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x00067F04 File Offset: 0x00066104
		private bool IsCharacterAway(Character character)
		{
			if (character.Submarine != null)
			{
				switch (character.Submarine.Info.Type)
				{
				case SubmarineType.Player:
				case SubmarineType.Outpost:
				case SubmarineType.OutpostModule:
					return false;
				case SubmarineType.Wreck:
				case SubmarineType.BeaconStation:
				case SubmarineType.Ruin:
					return true;
				}
			}
			if (this.level != null && !this.level.Removed)
			{
				foreach (Ruin ruin in this.level.Ruins)
				{
					Rectangle area = ruin.Area;
					area.Inflate(1000, 1000);
					if (area.Contains(character.WorldPosition))
					{
						return true;
					}
				}
				foreach (Level.Cave cave in this.level.Caves)
				{
					Rectangle area2 = cave.Area;
					area2.Inflate(1000, 1000);
					if (area2.Contains(character.WorldPosition))
					{
						return true;
					}
				}
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.Info.Type == SubmarineType.BeaconStation || sub.Info.Type == SubmarineType.Wreck)
				{
					Rectangle worldBorders = new Rectangle(sub.Borders.X + (int)sub.WorldPosition.X - 1000, sub.Borders.Y + (int)sub.WorldPosition.Y + 1000, sub.Borders.Width + 2000, sub.Borders.Height + 2000);
					if (Submarine.RectContains(worldBorders, character.WorldPosition, false))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x0006813C File Offset: 0x0006633C
		public void Load(XElement element)
		{
			foreach (Identifier id in element.GetAttributeIdentifierArray("QueuedEventsForNextRound", Array.Empty<Identifier>(), true))
			{
				this.QueuedEventsForNextRound.Enqueue(id);
			}
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x0006817D File Offset: 0x0006637D
		public XElement Save()
		{
			return new XElement("eventmanager", new XAttribute("QueuedEventsForNextRound", string.Join<Identifier>(',', this.QueuedEventsForNextRound)));
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x00068290 File Offset: 0x00066490
		[CompilerGenerated]
		internal static void <GetRefEntity>g__GetRefSubForCharacter|113_0(Character character, ref EventManager.<>c__DisplayClass113_0 A_1)
		{
			Submarine playerSub = character.Submarine;
			if (playerSub != null)
			{
				SubmarineInfo info = playerSub.Info;
				if (info != null && info.Type == SubmarineType.Player && playerSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
				{
					A_1.refEntity = playerSub;
				}
			}
			if (A_1.acceptRemoteControlledSubs)
			{
				Entity viewTarget = character.ViewTarget;
				Submarine viewedSub = (viewTarget != null) ? viewTarget.Submarine : null;
				if (viewedSub != null)
				{
					SubmarineInfo info = viewedSub.Info;
					if (info != null && info.Type == SubmarineType.Player && viewedSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
					{
						A_1.refEntity = viewedSub;
					}
				}
				Item selectedItem = character.SelectedItem;
				Submarine submarine;
				if (selectedItem == null)
				{
					submarine = null;
				}
				else
				{
					Steering component = selectedItem.GetComponent<Steering>();
					submarine = ((component != null) ? component.ControlledSub : null);
				}
				Submarine controlledSub = submarine;
				if (controlledSub != null && controlledSub.WorldPosition.X > A_1.refEntity.WorldPosition.X)
				{
					A_1.refEntity = controlledSub;
				}
			}
		}

		// Token: 0x0400058D RID: 1421
		[Nullable(1)]
		private Graph intensityGraph;

		// Token: 0x0400058E RID: 1422
		[Nullable(1)]
		private Graph targetIntensityGraph;

		// Token: 0x0400058F RID: 1423
		[Nullable(1)]
		private Graph monsterStrengthGraph;

		// Token: 0x04000590 RID: 1424
		private const float intensityGraphUpdateInterval = 10f;

		// Token: 0x04000591 RID: 1425
		private float lastIntensityUpdate;

		// Token: 0x04000592 RID: 1426
		private Vector2 pinnedPosition = new Vector2(256f, 128f);

		// Token: 0x04000593 RID: 1427
		private bool isDragging;

		// Token: 0x04000595 RID: 1429
		private bool isGraphSelected;

		// Token: 0x04000596 RID: 1430
		[Nullable(1)]
		private readonly List<EventManager.DebugLine> debugPositions = new List<EventManager.DebugLine>();

		// Token: 0x04000597 RID: 1431
		private const float IntensityUpdateInterval = 5f;

		// Token: 0x04000598 RID: 1432
		private const float CalculateDistanceTraveledInterval = 5f;

		// Token: 0x04000599 RID: 1433
		private const int MaxEventHistory = 20;

		// Token: 0x0400059A RID: 1434
		private Level level;

		// Token: 0x0400059B RID: 1435
		private readonly List<Sprite> preloadedSprites = new List<Sprite>();

		// Token: 0x0400059C RID: 1436
		private float currentIntensity;

		// Token: 0x0400059D RID: 1437
		private float targetIntensity;

		// Token: 0x0400059E RID: 1438
		private float musicIntensity;

		// Token: 0x0400059F RID: 1439
		private float eventThreshold = 0.2f;

		// Token: 0x040005A0 RID: 1440
		private float eventCoolDown;

		// Token: 0x040005A1 RID: 1441
		private float intensityUpdateTimer;

		// Token: 0x040005A2 RID: 1442
		private PathFinder pathFinder;

		// Token: 0x040005A3 RID: 1443
		private float totalPathLength;

		// Token: 0x040005A4 RID: 1444
		private float calculateDistanceTraveledTimer;

		// Token: 0x040005A5 RID: 1445
		private float distanceTraveled;

		// Token: 0x040005A6 RID: 1446
		private float avgCrewHealth;

		// Token: 0x040005A7 RID: 1447
		private float avgHullIntegrity;

		// Token: 0x040005A8 RID: 1448
		private float floodingAmount;

		// Token: 0x040005A9 RID: 1449
		private float fireAmount;

		// Token: 0x040005AA RID: 1450
		private float enemyDanger;

		// Token: 0x040005AB RID: 1451
		private float monsterStrength;

		// Token: 0x040005AC RID: 1452
		public float CumulativeMonsterStrengthMain;

		// Token: 0x040005AD RID: 1453
		public float CumulativeMonsterStrengthRuins;

		// Token: 0x040005AE RID: 1454
		public float CumulativeMonsterStrengthWrecks;

		// Token: 0x040005AF RID: 1455
		public float CumulativeMonsterStrengthCaves;

		// Token: 0x040005B0 RID: 1456
		private float roundDuration;

		// Token: 0x040005B1 RID: 1457
		private bool isCrewAway;

		// Token: 0x040005B2 RID: 1458
		private const float CrewAwayResetDelay = 60f;

		// Token: 0x040005B3 RID: 1459
		private float crewAwayResetTimer;

		// Token: 0x040005B4 RID: 1460
		private float crewAwayDuration;

		// Token: 0x040005B5 RID: 1461
		private readonly List<EventSet> pendingEventSets = new List<EventSet>();

		// Token: 0x040005B6 RID: 1462
		private readonly Dictionary<EventSet, List<Event>> selectedEvents = new Dictionary<EventSet, List<Event>>();

		// Token: 0x040005B7 RID: 1463
		private readonly List<Event> activeEvents = new List<Event>();

		// Token: 0x040005B8 RID: 1464
		private readonly HashSet<Event> finishedEvents = new HashSet<Event>();

		// Token: 0x040005B9 RID: 1465
		private readonly HashSet<Identifier> nonRepeatableEvents = new HashSet<Identifier>();

		// Token: 0x040005BA RID: 1466
		private EventManagerSettings settings;

		// Token: 0x040005BB RID: 1467
		private readonly bool isClient;

		// Token: 0x040005BC RID: 1468
		public readonly Queue<Event> QueuedEvents = new Queue<Event>();

		// Token: 0x040005BD RID: 1469
		public readonly Queue<Identifier> QueuedEventsForNextRound = new Queue<Identifier>();

		// Token: 0x040005BE RID: 1470
		private readonly List<EventManager.TimeStamp> timeStamps = new List<EventManager.TimeStamp>();

		// Token: 0x040005BF RID: 1471
		public readonly EventLog EventLog = new EventLog();

		// Token: 0x040005C0 RID: 1472
		public bool Enabled = true;

		// Token: 0x040005C1 RID: 1473
		private MTRandom random;

		// Token: 0x040005C3 RID: 1475
		private bool eventsInitialized;

		// Token: 0x020007A8 RID: 1960
		private readonly struct DebugLine
		{
			// Token: 0x06006B2C RID: 27436 RVA: 0x0035C953 File Offset: 0x0035AB53
			public DebugLine(Vector2 position, Color color)
			{
				this.Position = position;
				this.Color = color;
			}

			// Token: 0x04003B56 RID: 15190
			public readonly Vector2 Position;

			// Token: 0x04003B57 RID: 15191
			public readonly Color Color;
		}

		// Token: 0x020007A9 RID: 1961
		public enum NetworkEventType
		{
			// Token: 0x04003B59 RID: 15193
			CONVERSATION,
			// Token: 0x04003B5A RID: 15194
			CONVERSATION_SELECTED_OPTION,
			// Token: 0x04003B5B RID: 15195
			STATUSEFFECT,
			// Token: 0x04003B5C RID: 15196
			MISSION,
			// Token: 0x04003B5D RID: 15197
			UNLOCKPATH,
			// Token: 0x04003B5E RID: 15198
			EVENTLOG,
			// Token: 0x04003B5F RID: 15199
			EVENTOBJECTIVE
		}

		// Token: 0x020007AA RID: 1962
		[NetworkSerialize(25)]
		public readonly struct NetEventLogEntry : INetSerializableStruct, IEquatable<EventManager.NetEventLogEntry>
		{
			// Token: 0x06006B2D RID: 27437 RVA: 0x0035C963 File Offset: 0x0035AB63
			public NetEventLogEntry(Identifier EventPrefabId, Identifier LogEntryId, string Text)
			{
				this.EventPrefabId = EventPrefabId;
				this.LogEntryId = LogEntryId;
				this.Text = Text;
			}

			// Token: 0x170019F0 RID: 6640
			// (get) Token: 0x06006B2E RID: 27438 RVA: 0x0035C97A File Offset: 0x0035AB7A
			// (set) Token: 0x06006B2F RID: 27439 RVA: 0x0035C982 File Offset: 0x0035AB82
			public Identifier EventPrefabId { get; set; }

			// Token: 0x170019F1 RID: 6641
			// (get) Token: 0x06006B30 RID: 27440 RVA: 0x0035C98B File Offset: 0x0035AB8B
			// (set) Token: 0x06006B31 RID: 27441 RVA: 0x0035C993 File Offset: 0x0035AB93
			public Identifier LogEntryId { get; set; }

			// Token: 0x170019F2 RID: 6642
			// (get) Token: 0x06006B32 RID: 27442 RVA: 0x0035C99C File Offset: 0x0035AB9C
			// (set) Token: 0x06006B33 RID: 27443 RVA: 0x0035C9A4 File Offset: 0x0035ABA4
			public string Text { get; set; }

			// Token: 0x06006B34 RID: 27444 RVA: 0x0035C9B0 File Offset: 0x0035ABB0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetEventLogEntry");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006B35 RID: 27445 RVA: 0x0035C9FC File Offset: 0x0035ABFC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("EventPrefabId = ");
				builder.Append(this.EventPrefabId.ToString());
				builder.Append(", LogEntryId = ");
				builder.Append(this.LogEntryId.ToString());
				builder.Append(", Text = ");
				builder.Append(this.Text);
				return true;
			}

			// Token: 0x06006B36 RID: 27446 RVA: 0x0035CA71 File Offset: 0x0035AC71
			[CompilerGenerated]
			public static bool operator !=(EventManager.NetEventLogEntry left, EventManager.NetEventLogEntry right)
			{
				return !(left == right);
			}

			// Token: 0x06006B37 RID: 27447 RVA: 0x0035CA7D File Offset: 0x0035AC7D
			[CompilerGenerated]
			public static bool operator ==(EventManager.NetEventLogEntry left, EventManager.NetEventLogEntry right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006B38 RID: 27448 RVA: 0x0035CA87 File Offset: 0x0035AC87
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<EventPrefabId>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<LogEntryId>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Text>k__BackingField);
			}

			// Token: 0x06006B39 RID: 27449 RVA: 0x0035CAC7 File Offset: 0x0035ACC7
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventManager.NetEventLogEntry && this.Equals((EventManager.NetEventLogEntry)obj);
			}

			// Token: 0x06006B3A RID: 27450 RVA: 0x0035CAE0 File Offset: 0x0035ACE0
			[CompilerGenerated]
			public bool Equals(EventManager.NetEventLogEntry other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<EventPrefabId>k__BackingField, other.<EventPrefabId>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<LogEntryId>k__BackingField, other.<LogEntryId>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Text>k__BackingField, other.<Text>k__BackingField);
			}

			// Token: 0x06006B3B RID: 27451 RVA: 0x0035CB35 File Offset: 0x0035AD35
			[CompilerGenerated]
			public void Deconstruct(out Identifier EventPrefabId, out Identifier LogEntryId, out string Text)
			{
				EventPrefabId = this.EventPrefabId;
				LogEntryId = this.LogEntryId;
				Text = this.Text;
			}
		}

		// Token: 0x020007AB RID: 1963
		[NetworkSerialize(28)]
		public readonly struct NetEventObjective : INetSerializableStruct, IEquatable<EventManager.NetEventObjective>
		{
			// Token: 0x06006B3C RID: 27452 RVA: 0x0035CB57 File Offset: 0x0035AD57
			public NetEventObjective(EventObjectiveAction.SegmentActionType Type, Identifier Identifier, Identifier ObjectiveTag, Identifier TextTag, Identifier ParentObjectiveId, bool CanBeCompleted)
			{
				this.Type = Type;
				this.Identifier = Identifier;
				this.ObjectiveTag = ObjectiveTag;
				this.TextTag = TextTag;
				this.ParentObjectiveId = ParentObjectiveId;
				this.CanBeCompleted = CanBeCompleted;
			}

			// Token: 0x170019F3 RID: 6643
			// (get) Token: 0x06006B3D RID: 27453 RVA: 0x0035CB86 File Offset: 0x0035AD86
			// (set) Token: 0x06006B3E RID: 27454 RVA: 0x0035CB8E File Offset: 0x0035AD8E
			public EventObjectiveAction.SegmentActionType Type { get; set; }

			// Token: 0x170019F4 RID: 6644
			// (get) Token: 0x06006B3F RID: 27455 RVA: 0x0035CB97 File Offset: 0x0035AD97
			// (set) Token: 0x06006B40 RID: 27456 RVA: 0x0035CB9F File Offset: 0x0035AD9F
			public Identifier Identifier { get; set; }

			// Token: 0x170019F5 RID: 6645
			// (get) Token: 0x06006B41 RID: 27457 RVA: 0x0035CBA8 File Offset: 0x0035ADA8
			// (set) Token: 0x06006B42 RID: 27458 RVA: 0x0035CBB0 File Offset: 0x0035ADB0
			public Identifier ObjectiveTag { get; set; }

			// Token: 0x170019F6 RID: 6646
			// (get) Token: 0x06006B43 RID: 27459 RVA: 0x0035CBB9 File Offset: 0x0035ADB9
			// (set) Token: 0x06006B44 RID: 27460 RVA: 0x0035CBC1 File Offset: 0x0035ADC1
			public Identifier TextTag { get; set; }

			// Token: 0x170019F7 RID: 6647
			// (get) Token: 0x06006B45 RID: 27461 RVA: 0x0035CBCA File Offset: 0x0035ADCA
			// (set) Token: 0x06006B46 RID: 27462 RVA: 0x0035CBD2 File Offset: 0x0035ADD2
			public Identifier ParentObjectiveId { get; set; }

			// Token: 0x170019F8 RID: 6648
			// (get) Token: 0x06006B47 RID: 27463 RVA: 0x0035CBDB File Offset: 0x0035ADDB
			// (set) Token: 0x06006B48 RID: 27464 RVA: 0x0035CBE3 File Offset: 0x0035ADE3
			public bool CanBeCompleted { get; set; }

			// Token: 0x06006B49 RID: 27465 RVA: 0x0035CBEC File Offset: 0x0035ADEC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NetEventObjective");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006B4A RID: 27466 RVA: 0x0035CC38 File Offset: 0x0035AE38
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Type = ");
				builder.Append(this.Type.ToString());
				builder.Append(", Identifier = ");
				builder.Append(this.Identifier.ToString());
				builder.Append(", ObjectiveTag = ");
				builder.Append(this.ObjectiveTag.ToString());
				builder.Append(", TextTag = ");
				builder.Append(this.TextTag.ToString());
				builder.Append(", ParentObjectiveId = ");
				builder.Append(this.ParentObjectiveId.ToString());
				builder.Append(", CanBeCompleted = ");
				builder.Append(this.CanBeCompleted.ToString());
				return true;
			}

			// Token: 0x06006B4B RID: 27467 RVA: 0x0035CD30 File Offset: 0x0035AF30
			[CompilerGenerated]
			public static bool operator !=(EventManager.NetEventObjective left, EventManager.NetEventObjective right)
			{
				return !(left == right);
			}

			// Token: 0x06006B4C RID: 27468 RVA: 0x0035CD3C File Offset: 0x0035AF3C
			[CompilerGenerated]
			public static bool operator ==(EventManager.NetEventObjective left, EventManager.NetEventObjective right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006B4D RID: 27469 RVA: 0x0035CD48 File Offset: 0x0035AF48
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((EqualityComparer<EventObjectiveAction.SegmentActionType>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<Identifier>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<ObjectiveTag>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TextTag>k__BackingField)) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<ParentObjectiveId>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<CanBeCompleted>k__BackingField);
			}

			// Token: 0x06006B4E RID: 27470 RVA: 0x0035CDD8 File Offset: 0x0035AFD8
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventManager.NetEventObjective && this.Equals((EventManager.NetEventObjective)obj);
			}

			// Token: 0x06006B4F RID: 27471 RVA: 0x0035CDF0 File Offset: 0x0035AFF0
			[CompilerGenerated]
			public bool Equals(EventManager.NetEventObjective other)
			{
				return EqualityComparer<EventObjectiveAction.SegmentActionType>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<Identifier>k__BackingField, other.<Identifier>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<ObjectiveTag>k__BackingField, other.<ObjectiveTag>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TextTag>k__BackingField, other.<TextTag>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<ParentObjectiveId>k__BackingField, other.<ParentObjectiveId>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<CanBeCompleted>k__BackingField, other.<CanBeCompleted>k__BackingField);
			}

			// Token: 0x06006B50 RID: 27472 RVA: 0x0035CE90 File Offset: 0x0035B090
			[CompilerGenerated]
			public void Deconstruct(out EventObjectiveAction.SegmentActionType Type, out Identifier Identifier, out Identifier ObjectiveTag, out Identifier TextTag, out Identifier ParentObjectiveId, out bool CanBeCompleted)
			{
				Type = this.Type;
				Identifier = this.Identifier;
				ObjectiveTag = this.ObjectiveTag;
				TextTag = this.TextTag;
				ParentObjectiveId = this.ParentObjectiveId;
				CanBeCompleted = this.CanBeCompleted;
			}
		}

		// Token: 0x020007AC RID: 1964
		private readonly struct TimeStamp
		{
			// Token: 0x06006B51 RID: 27473 RVA: 0x0035CEE0 File Offset: 0x0035B0E0
			public TimeStamp(Event e)
			{
				this.Event = e;
				this.Time = Timing.TotalTime;
			}

			// Token: 0x04003B69 RID: 15209
			public readonly double Time;

			// Token: 0x04003B6A RID: 15210
			public readonly Event Event;
		}
	}
}
