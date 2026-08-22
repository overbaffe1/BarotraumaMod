using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000D0 RID: 208
	internal class Hull : MapEntity, ISerializableEntity, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17000714 RID: 1812
		// (get) Token: 0x06001B9A RID: 7066 RVA: 0x001123E1 File Offset: 0x001105E1
		// (set) Token: 0x06001B9B RID: 7067 RVA: 0x001123EC File Offset: 0x001105EC
		public float DrawSurface
		{
			get
			{
				return this.drawSurface;
			}
			set
			{
				if (Math.Abs(this.drawSurface - value) < 1E-05f)
				{
					return;
				}
				this.drawSurface = MathHelper.Clamp(value, (float)(this.rect.Y - this.rect.Height), (float)this.rect.Y);
				this.update = true;
			}
		}

		// Token: 0x17000715 RID: 1813
		// (get) Token: 0x06001B9C RID: 7068 RVA: 0x00112445 File Offset: 0x00110645
		public override bool SelectableInEditor
		{
			get
			{
				return Hull.ShowHulls && SubEditorScreen.IsLayerVisible(this);
			}
		}

		// Token: 0x17000716 RID: 1814
		// (get) Token: 0x06001B9D RID: 7069 RVA: 0x00112456 File Offset: 0x00110656
		public override bool DrawBelowWater
		{
			get
			{
				return this.decals.Count > 0 || this.BallastFlora != null;
			}
		}

		// Token: 0x17000717 RID: 1815
		// (get) Token: 0x06001B9E RID: 7070 RVA: 0x00112471 File Offset: 0x00110671
		public override bool DrawOverWater
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x00112474 File Offset: 0x00110674
		public override bool IsVisible(Rectangle worldView)
		{
			return this.BallastFlora != null || ((Screen.Selected == GameMain.SubEditorScreen || GameMain.DebugDraw || this.decals.Count != 0 || this.paintAmount >= this.minimumPaintAmountToDraw) && base.IsVisible(worldView));
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x001124C2 File Offset: 0x001106C2
		public override bool IsMouseOn(Vector2 position)
		{
			return (GameMain.DebugDraw || Hull.ShowHulls) && Submarine.RectContains(base.WorldRect, position, false) && !Submarine.RectContains(MathUtils.ExpandRect(base.WorldRect, -8), position, false);
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x001124FC File Offset: 0x001106FC
		private GUIComponent CreateEditingHUD(bool inGame = false)
		{
			int heightScaled = GUI.IntScale(20f);
			MapEntity.editingHUD = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.25f), GUI.Canvas, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 0)
			}, "", null)
			{
				UserData = this
			};
			GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(0.95f, 0.8f), MapEntity.editingHUD.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				CanTakeKeyBoardFocus = false
			};
			SerializableEntityEditor hullEditor = new SerializableEntityEditor(listBox.Content.RectTransform, this, inGame, true, "", 24, GUIStyle.LargeFont, true);
			if (!inGame && this.Linkable)
			{
				RectTransform rectT = new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true);
				RichString text = TextManager.Get("HoldToLink");
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock linkText = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
				RectTransform rectT2 = new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true);
				RichString text2 = TextManager.Get("hulllinkinfo");
				smallFont = GUIStyle.SmallFont;
				GUITextBlock hullLinkText = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Left, false, "", null);
				RectTransform rectT3 = new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true);
				RichString text3 = TextManager.Get("AllowedLinks");
				smallFont = GUIStyle.SmallFont;
				GUITextBlock itemsText = new GUITextBlock(rectT3, text3, null, smallFont, Alignment.Left, false, "", null);
				LocalizedString allowedItems = base.AllowedLinks.None(null) ? TextManager.Get("None") : string.Join<Identifier>(", ", base.AllowedLinks);
				itemsText.Text = TextManager.AddPunctuation(':', new LocalizedString[]
				{
					itemsText.Text,
					allowedItems
				});
				hullEditor.AddCustomContent(linkText, 1);
				hullEditor.AddCustomContent(hullLinkText, 2);
				hullEditor.AddCustomContent(itemsText, 3);
				linkText.TextColor = GUIStyle.Orange;
				hullLinkText.TextColor = GUIStyle.Orange;
				itemsText.TextColor = GUIStyle.Orange;
			}
			MapEntity.PositionEditingHUD();
			return MapEntity.editingHUD;
		}

		// Token: 0x06001BA2 RID: 7074 RVA: 0x001127E0 File Offset: 0x001109E0
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData as Hull != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD(Screen.Selected != GameMain.SubEditorScreen);
			}
			if (!PlayerInput.KeyDown(Keys.Space))
			{
				return;
			}
			bool lClick = PlayerInput.PrimaryMouseButtonClicked();
			bool rClick = PlayerInput.SecondaryMouseButtonClicked();
			if (!lClick && !rClick)
			{
				return;
			}
			Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
			foreach (MapEntity entity in MapEntity.HighlightedEntities)
			{
				if (entity != this && entity.IsMouseOn(position) && entity.linkedTo != null && entity.Linkable)
				{
					if (entity.linkedTo.Contains(this) || this.linkedTo.Contains(entity) || rClick)
					{
						entity.linkedTo.Remove(this);
						this.linkedTo.Remove(entity);
					}
					else
					{
						if (!entity.linkedTo.Contains(this))
						{
							entity.linkedTo.Add(this);
						}
						if (!this.linkedTo.Contains(this))
						{
							this.linkedTo.Add(entity);
						}
					}
				}
			}
		}

		// Token: 0x06001BA3 RID: 7075 RVA: 0x0011292C File Offset: 0x00110B2C
		public static void UpdateCheats(float deltaTime, Camera cam)
		{
			bool primaryMouseButtonHeld = PlayerInput.PrimaryMouseButtonHeld();
			bool secondaryMouseButtonHeld = PlayerInput.SecondaryMouseButtonHeld();
			bool doubleClicked = PlayerInput.DoubleClicked();
			bool secondaryDoubleClicked = PlayerInput.SecondaryDoubleClicked();
			if (!primaryMouseButtonHeld && !secondaryMouseButtonHeld && !doubleClicked && !secondaryDoubleClicked)
			{
				return;
			}
			Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
			Screen selected = Screen.Selected;
			Hull.<>c__DisplayClass25_0 CS$<>8__locals1;
			CS$<>8__locals1.hull = ((selected != null && selected.IsEditor) ? Hull.FindHullUnoptimized(position, null, true, true) : Hull.FindHull(position, null, true, true));
			if (CS$<>8__locals1.hull == null || CS$<>8__locals1.hull.IdFreed)
			{
				return;
			}
			if (Hull.EditWater)
			{
				if (primaryMouseButtonHeld)
				{
					Hull.<UpdateCheats>g__SetWaterVolume|25_0(CS$<>8__locals1.hull.WaterVolume + 100000f * deltaTime, ref CS$<>8__locals1);
				}
				else if (secondaryMouseButtonHeld)
				{
					Hull.<UpdateCheats>g__SetWaterVolume|25_0(CS$<>8__locals1.hull.WaterVolume - 100000f * deltaTime, ref CS$<>8__locals1);
				}
				if (doubleClicked)
				{
					Hull.<UpdateCheats>g__SetWaterVolume|25_0(CS$<>8__locals1.hull.Volume * 1.05f, ref CS$<>8__locals1);
					return;
				}
				if (secondaryDoubleClicked)
				{
					Hull.<UpdateCheats>g__SetWaterVolume|25_0(0f, ref CS$<>8__locals1);
					return;
				}
			}
			else if (Hull.EditFire)
			{
				bool networkUpdate = false;
				if (primaryMouseButtonHeld)
				{
					new FireSource(position, CS$<>8__locals1.hull, null, true);
					networkUpdate = true;
				}
				else if (secondaryMouseButtonHeld || secondaryDoubleClicked)
				{
					for (int index = CS$<>8__locals1.hull.FireSources.Count - 1; index >= 0; index--)
					{
						FireSource currentFireSource = CS$<>8__locals1.hull.FireSources[index];
						if (secondaryMouseButtonHeld)
						{
							currentFireSource.Extinguish(deltaTime, 120f);
							networkUpdate = true;
						}
						else
						{
							currentFireSource.Remove();
							networkUpdate = true;
						}
					}
				}
				if (networkUpdate)
				{
					CS$<>8__locals1.hull.networkUpdatePending = true;
					CS$<>8__locals1.hull.serverUpdateDelay = 0.5f;
				}
			}
		}

		// Token: 0x06001BA4 RID: 7076 RVA: 0x00112AC8 File Offset: 0x00110CC8
		private void DrawDecals(SpriteBatch spriteBatch)
		{
			Rectangle hullDrawRect = this.rect;
			if (base.Submarine != null)
			{
				hullDrawRect.Location += base.Submarine.DrawPosition.ToPoint();
			}
			float depth = 1f;
			foreach (Decal d in this.decals)
			{
				d.Draw(spriteBatch, this, depth);
				depth -= 1E-06f;
			}
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x00112B64 File Offset: 0x00110D64
		public override void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
			Hull.<>c__DisplayClass27_0 CS$<>8__locals1;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (back && Screen.Selected != GameMain.SubEditorScreen)
			{
				BallastFloraBehavior ballastFlora = this.BallastFlora;
				if (ballastFlora != null)
				{
					ballastFlora.Draw(CS$<>8__locals1.spriteBatch);
				}
				this.DrawDecals(CS$<>8__locals1.spriteBatch);
				return;
			}
			if ((!Hull.ShowHulls || !SubEditorScreen.IsLayerVisible(this)) && !GameMain.DebugDraw)
			{
				return;
			}
			if (!editing && (!GameMain.DebugDraw || Screen.Selected.Cam.Zoom < 0.1f))
			{
				return;
			}
			float alpha = 1f;
			float hideTimeAfterEdit = 3f;
			if (this.lastAmbientLightEditTime > Timing.TotalTime - (double)(hideTimeAfterEdit * 2f))
			{
				alpha = Math.Min((float)(Timing.TotalTime - this.lastAmbientLightEditTime) / hideTimeAfterEdit - 1f, 1f);
			}
			CS$<>8__locals1.drawRect = ((base.Submarine == null) ? this.rect : new Rectangle((int)(base.Submarine.DrawPosition.X + (float)this.rect.X), (int)(base.Submarine.DrawPosition.Y + (float)this.rect.Y), this.rect.Width, this.rect.Height));
			if (editing)
			{
				if (base.IsSelected || base.IsHighlighted)
				{
					GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.drawRect.X, (float)(-(float)CS$<>8__locals1.drawRect.Y)), new Vector2((float)this.rect.Width, (float)this.rect.Height), (base.IsHighlighted ? (Color.LightBlue * 0.8f) : (GUIStyle.Red * 0.5f)) * alpha, false, 0f, (float)((int)Math.Max(5f / Screen.Selected.Cam.Zoom, 1f)));
				}
				float waterHeight = this.WaterVolume / (float)this.rect.Width;
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.drawRect.X, (float)(-(float)CS$<>8__locals1.drawRect.Y + CS$<>8__locals1.drawRect.Height) - waterHeight), new Vector2((float)CS$<>8__locals1.drawRect.Width, waterHeight), Color.Blue * 0.25f, true, 0f, 1f);
			}
			GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.drawRect.X, (float)(-(float)CS$<>8__locals1.drawRect.Y)), new Vector2((float)this.rect.Width, (float)this.rect.Height), Color.Blue * alpha, false, (float)(this.ID % 255) * 1E-06f, (float)((int)Math.Max(MathF.Ceiling(1.5f / Screen.Selected.Cam.Zoom), 1f)));
			GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(CS$<>8__locals1.drawRect.X, -CS$<>8__locals1.drawRect.Y, this.rect.Width, this.rect.Height), GUIStyle.Red * ((100f - this.OxygenPercentage) / 400f) * alpha, true, 0f, (float)((int)Math.Max(MathF.Ceiling(1.5f / Screen.Selected.Cam.Zoom), 1f)));
			if (GameMain.DebugDraw)
			{
				Screen selected = Screen.Selected;
				Camera camera = (selected != null) ? selected.Cam : null;
				if (camera != null && camera.Zoom > 0.5f)
				{
					GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, "Pressure: " + ((int)this.pressure - this.rect.Y).ToString() + " - Oxygen: " + ((int)this.OxygenPercentage).ToString(), new Vector2((float)(CS$<>8__locals1.drawRect.X + 5), (float)(-(float)CS$<>8__locals1.drawRect.Y + 5)), Color.White, ForceUpperCase.Inherit, false);
					GUIStyle.SmallFont.DrawString(CS$<>8__locals1.spriteBatch, this.waterVolume.ToString() + " / " + this.Volume.ToString(), new Vector2((float)(CS$<>8__locals1.drawRect.X + 5), (float)(-(float)CS$<>8__locals1.drawRect.Y + 20)), Color.White, ForceUpperCase.Inherit, false);
					if (this.WaterVolume > 0f)
					{
						Hull.<Draw>g__drawProgressBar|27_0(50, new Point(0, 0), Math.Min(this.waterVolume / this.Volume, 1f), Color.Cyan, ref CS$<>8__locals1);
						if (this.WaterVolume > this.Volume)
						{
							float maxExcessWater = this.Volume * 1.05f;
							Hull.<Draw>g__drawProgressBar|27_0(50, new Point(0, 0), (this.waterVolume - this.Volume) / maxExcessWater, GUIStyle.Red, ref CS$<>8__locals1);
						}
					}
					if (this.lethalPressure > 0f)
					{
						Hull.<Draw>g__drawProgressBar|27_0(50, new Point(20, 0), this.lethalPressure / 100f, Color.Red, ref CS$<>8__locals1);
					}
					foreach (FireSource fs in this.FireSources)
					{
						Rectangle fireSourceRect = new Rectangle((int)fs.WorldPosition.X, -(int)fs.WorldPosition.Y, (int)fs.Size.X, (int)fs.Size.Y);
						GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, fireSourceRect, GUIStyle.Red, false, 0f, 5f);
						GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(fireSourceRect.X - (int)fs.DamageRange, fireSourceRect.Y, fireSourceRect.Width + (int)fs.DamageRange * 2, fireSourceRect.Height), GUIStyle.Orange, false, 0f, 5f);
						Vector2 topCenter = new Vector2((float)fireSourceRect.Center.X, (float)fireSourceRect.Y);
						GUI.DrawLine(CS$<>8__locals1.spriteBatch, topCenter, topCenter - Vector2.UnitY * fs.FlameHeight, GUIStyle.Red * 0.7f, 0f, 5f);
					}
					foreach (FireSource fs2 in this.FakeFireSources)
					{
						Rectangle fireSourceRect2 = new Rectangle((int)fs2.WorldPosition.X, -(int)fs2.WorldPosition.Y, (int)fs2.Size.X, (int)fs2.Size.Y);
						GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, fireSourceRect2, GUIStyle.Red, false, 0f, 5f);
						GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(fireSourceRect2.X - (int)fs2.DamageRange, fireSourceRect2.Y, fireSourceRect2.Width + (int)fs2.DamageRange * 2, fireSourceRect2.Height), GUIStyle.Orange, false, 0f, 5f);
					}
					float worldSurface = this.surface + base.Submarine.DrawPosition.Y;
					GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2((float)CS$<>8__locals1.drawRect.X, -worldSurface), new Vector2((float)CS$<>8__locals1.drawRect.Right, -worldSurface), Color.Cyan * 0.5f, 0f, 1f);
					for (int i = 0; i < this.waveY.Length - 1; i++)
					{
						GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2((float)(CS$<>8__locals1.drawRect.X + 32 * i), -this.WorldSurface - this.waveY[i] - 10f), new Vector2((float)(CS$<>8__locals1.drawRect.X + 32 * (i + 1)), -this.WorldSurface - this.waveY[i + 1] - 10f), Color.Blue * 0.5f, 0f, 1f);
					}
				}
			}
			foreach (MapEntity e in this.linkedTo)
			{
				Hull linkedHull = e as Hull;
				if (linkedHull != null)
				{
					Rectangle connectedHullRect = (e.Submarine == null) ? linkedHull.rect : new Rectangle((int)(base.Submarine.DrawPosition.X + linkedHull.WorldPosition.X), (int)(base.Submarine.DrawPosition.Y + linkedHull.WorldPosition.Y), linkedHull.WorldRect.Width, linkedHull.WorldRect.Height);
					Rectangle currentHullRect = (base.Submarine == null) ? base.WorldRect : new Rectangle((int)(base.Submarine.DrawPosition.X + this.WorldPosition.X), (int)(base.Submarine.DrawPosition.Y + this.WorldPosition.Y), base.WorldRect.Width, base.WorldRect.Height);
					GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2((float)currentHullRect.X, (float)(-(float)currentHullRect.Y)), new Vector2((float)connectedHullRect.X, (float)(-(float)connectedHullRect.Y)), GUIStyle.Green, 0f, 2f);
				}
			}
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x00113574 File Offset: 0x00111774
		public void DrawSectionColors(SpriteBatch spriteBatch)
		{
			if (this.BackgroundSections == null || this.BackgroundSections.Count == 0)
			{
				return;
			}
			Vector2 drawOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.DrawPosition;
			Point sectionSize = this.BackgroundSections[0].Rect.Size;
			Vector2 drawPos = drawOffset + new Vector2((float)(this.rect.Location.X + sectionSize.X / 2), (float)(this.rect.Location.Y - sectionSize.Y / 2));
			for (int i = 0; i < this.BackgroundSections.Count; i++)
			{
				BackgroundSection section = this.BackgroundSections[i];
				if (section.ColorStrength >= 0.01f && section.Color.A >= 1)
				{
					if (section.GrimeSprite == null)
					{
						GUI.DrawRectangle(spriteBatch, new Vector2(drawOffset.X + (float)this.rect.X + (float)section.Rect.X, -(drawOffset.Y + (float)this.rect.Y + (float)section.Rect.Y)), new Vector2((float)sectionSize.X, (float)sectionSize.Y), section.GetStrengthAdjustedColor(), true, 0f, (float)((int)Math.Max(1.5f / Screen.Selected.Cam.Zoom, 1f)));
					}
					else
					{
						Vector2 sectionPos = new Vector2(drawPos.X + (float)section.Rect.Location.X, -(drawPos.Y + (float)section.Rect.Location.Y));
						Vector2 randomOffset = new Vector2(section.Noise.X - 0.5f, section.Noise.Y - 0.5f) * 15f;
						section.GrimeSprite.Draw(spriteBatch, sectionPos + randomOffset, section.GetStrengthAdjustedColor(), 0f, 1.25f, SpriteEffects.None, null);
					}
				}
			}
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x0011379C File Offset: 0x0011199C
		public static void UpdateVertices(Camera cam, WaterRenderer renderer)
		{
			foreach (EntityGrid entityGrid in Hull.EntityGrids)
			{
				if (entityGrid.WorldRect.X <= cam.WorldView.Right && entityGrid.WorldRect.Right >= cam.WorldView.X && entityGrid.WorldRect.Y - entityGrid.WorldRect.Height <= cam.WorldView.Y && entityGrid.WorldRect.Y >= cam.WorldView.Y - cam.WorldView.Height)
				{
					IEnumerable<MapEntity> allEntities = entityGrid.GetAllEntities();
					foreach (MapEntity mapEntity in allEntities)
					{
						Hull hull = (Hull)mapEntity;
						hull.UpdateVertices(cam, entityGrid, renderer);
					}
				}
			}
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x001138C0 File Offset: 0x00111AC0
		private void UpdateVertices(Camera cam, EntityGrid entityGrid, WaterRenderer renderer)
		{
			Vector2 submarinePos = (base.Submarine == null) ? Vector2.Zero : base.Submarine.DrawPosition;
			if (renderer.PositionInBuffer > renderer.vertices.Length - 6)
			{
				return;
			}
			float top = (float)this.rect.Y + submarinePos.Y;
			float bottom = top - (float)this.rect.Height;
			float renderSurface = this.drawSurface + submarinePos.Y;
			if (bottom > (float)cam.WorldView.Y || top < (float)(cam.WorldView.Y - cam.WorldView.Height))
			{
				return;
			}
			if ((float)this.rect.X + submarinePos.X > (float)cam.WorldView.Right || (float)this.rect.Right + submarinePos.X < (float)cam.WorldView.X)
			{
				return;
			}
			Matrix transform = cam.Transform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 1f) * 0.5f;
			if (!this.update)
			{
				Hull.corners[0] = new Vector3((float)this.rect.X, (float)this.rect.Y, 0f);
				Hull.corners[1] = new Vector3((float)(this.rect.X + this.rect.Width), (float)this.rect.Y, 0f);
				Hull.corners[2] = new Vector3(Hull.corners[1].X, (float)(this.rect.Y - this.rect.Height), 0f);
				Hull.corners[3] = new Vector3(Hull.corners[0].X, Hull.corners[2].Y, 0f);
				for (int i = 0; i < 4; i++)
				{
					Hull.corners[i] += new Vector3(submarinePos, 0f);
					Hull.uvCoords[i] = Vector2.Transform(new Vector2(Hull.corners[i].X, -Hull.corners[i].Y), transform);
				}
				renderer.vertices[renderer.PositionInBuffer] = new VertexPositionTexture(Hull.corners[0], Hull.uvCoords[0]);
				renderer.vertices[renderer.PositionInBuffer + 1] = new VertexPositionTexture(Hull.corners[1], Hull.uvCoords[1]);
				renderer.vertices[renderer.PositionInBuffer + 2] = new VertexPositionTexture(Hull.corners[2], Hull.uvCoords[2]);
				renderer.vertices[renderer.PositionInBuffer + 3] = new VertexPositionTexture(Hull.corners[0], Hull.uvCoords[0]);
				renderer.vertices[renderer.PositionInBuffer + 4] = new VertexPositionTexture(Hull.corners[2], Hull.uvCoords[2]);
				renderer.vertices[renderer.PositionInBuffer + 5] = new VertexPositionTexture(Hull.corners[3], Hull.uvCoords[3]);
				renderer.PositionInBuffer += 6;
				return;
			}
			if (!renderer.IndoorsVertices.ContainsKey(entityGrid))
			{
				renderer.IndoorsVertices[entityGrid] = new VertexPositionColorTexture[3000];
			}
			if (!renderer.PositionInIndoorsBuffer.ContainsKey(entityGrid))
			{
				renderer.PositionInIndoorsBuffer[entityGrid] = 0;
			}
			float x = (float)this.rect.X;
			if (base.Submarine != null)
			{
				x += base.Submarine.DrawPosition.X;
			}
			int start = (int)Math.Floor((double)(((float)cam.WorldView.X - x) / 32f));
			start = Math.Max(start, 0);
			int end = this.waveY.Length - 1 - (int)Math.Floor((double)((x + (float)this.rect.Width - (float)cam.WorldView.Right) / 32f));
			end = Math.Min(end, this.waveY.Length - 1);
			x += (float)(start * 32);
			int width = 32;
			for (int j = start; j < end; j++)
			{
				Hull.corners[0] = new Vector3(x, top, 0f);
				Hull.corners[3] = new Vector3(Hull.corners[0].X, renderSurface + this.waveY[j], 0f);
				Hull.corners[1] = new Vector3(x + (float)width, top, 0f);
				Hull.corners[2] = new Vector3(Hull.corners[1].X, renderSurface + this.waveY[j + 1], 0f);
				Hull.corners[4] = new Vector3(x, bottom, 0f);
				Hull.corners[5] = new Vector3(x + (float)width, bottom, 0f);
				for (int k = 0; k < 4; k++)
				{
					Hull.uvCoords[k] = Vector2.Transform(new Vector2(Hull.corners[k].X, -Hull.corners[k].Y), transform);
				}
				if (renderer.PositionInBuffer <= renderer.vertices.Length - 6)
				{
					if (j == start)
					{
						Hull.prevCorners[0] = Hull.corners[0];
						Hull.prevCorners[1] = Hull.corners[3];
						Hull.prevUVs[0] = Hull.uvCoords[0];
						Hull.prevUVs[1] = Hull.uvCoords[3];
					}
					if (j == end - 1 || j == start || Math.Abs(Hull.prevCorners[1].Y - Hull.corners[2].Y) > 0.01f)
					{
						renderer.vertices[renderer.PositionInBuffer] = new VertexPositionTexture(Hull.prevCorners[0], Hull.prevUVs[0]);
						renderer.vertices[renderer.PositionInBuffer + 1] = new VertexPositionTexture(Hull.corners[1], Hull.uvCoords[1]);
						renderer.vertices[renderer.PositionInBuffer + 2] = new VertexPositionTexture(Hull.corners[2], Hull.uvCoords[2]);
						renderer.vertices[renderer.PositionInBuffer + 3] = new VertexPositionTexture(Hull.prevCorners[0], Hull.prevUVs[0]);
						renderer.vertices[renderer.PositionInBuffer + 4] = new VertexPositionTexture(Hull.corners[2], Hull.uvCoords[2]);
						renderer.vertices[renderer.PositionInBuffer + 5] = new VertexPositionTexture(Hull.prevCorners[1], Hull.prevUVs[1]);
						Hull.prevCorners[0] = Hull.corners[1];
						Hull.prevCorners[1] = Hull.corners[2];
						Hull.prevUVs[0] = Hull.uvCoords[1];
						Hull.prevUVs[1] = Hull.uvCoords[2];
						renderer.PositionInBuffer += 6;
					}
				}
				if (renderer.PositionInIndoorsBuffer[entityGrid] <= renderer.IndoorsVertices[entityGrid].Length - 12 && cam.Zoom > 0.6f)
				{
					float surfaceScale = 1f - MathHelper.Clamp(Hull.corners[3].Y - (top - 10f), 0f, 1f);
					Vector3 surfaceOffset = new Vector3(0f, -10f, 0f);
					surfaceOffset.Y += (float)Math.Sin((double)((float)(this.rect.X + j * 32) * 0.01f + renderer.WavePos.X * 0.25f)) * 2f;
					surfaceOffset.Y += (float)Math.Sin((double)((float)(this.rect.X + j * 32) * 0.05f - renderer.WavePos.X)) * 2f;
					surfaceOffset *= surfaceScale;
					Vector3 surfaceOffset2 = new Vector3(0f, -10f, 0f);
					surfaceOffset2.Y += (float)Math.Sin((double)((float)(this.rect.X + j * 32 + width) * 0.01f + renderer.WavePos.X * 0.25f)) * 2f;
					surfaceOffset2.Y += (float)Math.Sin((double)((float)(this.rect.X + j * 32 + width) * 0.05f - renderer.WavePos.X)) * 2f;
					surfaceOffset2 *= surfaceScale;
					int posInBuffer = renderer.PositionInIndoorsBuffer[entityGrid];
					renderer.IndoorsVertices[entityGrid][posInBuffer] = new VertexPositionColorTexture(Hull.corners[3] + surfaceOffset, renderer.IndoorsWaterColor, Vector2.Zero);
					renderer.IndoorsVertices[entityGrid][posInBuffer + 1] = new VertexPositionColorTexture(Hull.corners[2] + surfaceOffset2, renderer.IndoorsWaterColor, Vector2.Zero);
					renderer.IndoorsVertices[entityGrid][posInBuffer + 2] = new VertexPositionColorTexture(Hull.corners[5], renderer.IndoorsWaterColor, Vector2.Zero);
					renderer.IndoorsVertices[entityGrid][posInBuffer + 3] = new VertexPositionColorTexture(Hull.corners[3] + surfaceOffset, renderer.IndoorsWaterColor, Vector2.Zero);
					renderer.IndoorsVertices[entityGrid][posInBuffer + 4] = new VertexPositionColorTexture(Hull.corners[5], renderer.IndoorsWaterColor, Vector2.Zero);
					renderer.IndoorsVertices[entityGrid][posInBuffer + 5] = new VertexPositionColorTexture(Hull.corners[4], renderer.IndoorsWaterColor, Vector2.Zero);
					posInBuffer += 6;
					renderer.PositionInIndoorsBuffer[entityGrid] = posInBuffer;
					if (surfaceScale > 0f)
					{
						renderer.IndoorsVertices[entityGrid][posInBuffer] = new VertexPositionColorTexture(Hull.corners[3], renderer.IndoorsSurfaceTopColor, Vector2.Zero);
						renderer.IndoorsVertices[entityGrid][posInBuffer + 1] = new VertexPositionColorTexture(Hull.corners[2], renderer.IndoorsSurfaceTopColor, Vector2.Zero);
						renderer.IndoorsVertices[entityGrid][posInBuffer + 2] = new VertexPositionColorTexture(Hull.corners[2] + surfaceOffset2, renderer.IndoorsSurfaceBottomColor, Vector2.Zero);
						renderer.IndoorsVertices[entityGrid][posInBuffer + 3] = new VertexPositionColorTexture(Hull.corners[3], renderer.IndoorsSurfaceTopColor, Vector2.Zero);
						renderer.IndoorsVertices[entityGrid][posInBuffer + 4] = new VertexPositionColorTexture(Hull.corners[2] + surfaceOffset2, renderer.IndoorsSurfaceBottomColor, Vector2.Zero);
						renderer.IndoorsVertices[entityGrid][posInBuffer + 5] = new VertexPositionColorTexture(Hull.corners[3] + surfaceOffset, renderer.IndoorsSurfaceBottomColor, Vector2.Zero);
						Dictionary<EntityGrid, int> positionInIndoorsBuffer = renderer.PositionInIndoorsBuffer;
						positionInIndoorsBuffer[entityGrid] += 6;
					}
				}
				x += 32f;
				if (j == end - 2)
				{
					width -= (int)Math.Max(x + 32f - ((base.Submarine == null) ? ((float)this.rect.Right) : ((float)this.rect.Right + base.Submarine.DrawPosition.X)), 0f);
				}
			}
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x0011457C File Offset: 0x0011277C
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			Hull.IEventData eventData = extraData as Hull.IEventData;
			if (eventData == null)
			{
				throw new Exception("Malformed hull event: expected Hull.IEventData");
			}
			msg.WriteRangedInteger((int)eventData.EventType, 0, 3);
			if (eventData is Hull.StatusEventData)
			{
				Hull.StatusEventData statusEventData = (Hull.StatusEventData)eventData;
				this.SharedStatusWrite(msg);
				return;
			}
			if (eventData is Hull.BackgroundSectionsEventData)
			{
				Hull.BackgroundSectionsEventData backgroundSectionsEventData = (Hull.BackgroundSectionsEventData)eventData;
				this.SharedBackgroundSectionsWrite(msg, backgroundSectionsEventData);
				return;
			}
			if (eventData is Hull.DecalEventData)
			{
				Hull.DecalEventData decalEventData = (Hull.DecalEventData)eventData;
				Decal decal = decalEventData.Decal;
				int decalIndex = this.decals.IndexOf(decal);
				msg.WriteByte((byte)((decalIndex < 0) ? 255 : decalIndex));
				msg.WriteRangedSingle(decal.BaseAlpha, 0f, 1f, 8);
				return;
			}
			throw new Exception("Malformed hull event: did not expect " + eventData.GetType().Name);
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x00114650 File Offset: 0x00112850
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			Hull.EventType eventType = (Hull.EventType)msg.ReadRangedInteger(0, 3);
			switch (eventType)
			{
			case Hull.EventType.Status:
			{
				this.remoteOxygenPercentage = msg.ReadRangedSingle(0f, 100f, 8);
				float newWaterVolume;
				Hull.NetworkFireSource[] newFireSources;
				this.SharedStatusRead(msg, out newWaterVolume, out newFireSources);
				this.remoteWaterVolume = newWaterVolume;
				this.remoteFireSources = newFireSources;
				break;
			}
			case Hull.EventType.Decal:
			{
				int decalCount = msg.ReadRangedInteger(0, 10);
				if (decalCount == 0)
				{
					this.decals.Clear();
				}
				this.remoteDecals.Clear();
				for (int i = 0; i < decalCount; i++)
				{
					uint decalId = msg.ReadUInt32();
					int spriteIndex = (int)msg.ReadByte();
					float normalizedXPos = msg.ReadRangedSingle(0f, 1f, 8);
					float normalizedYPos = msg.ReadRangedSingle(0f, 1f, 8);
					float decalScale = msg.ReadRangedSingle(0f, 2f, 12);
					float decalAlpha = msg.ReadRangedSingle(0f, 1f, 8);
					this.remoteDecals.Add(new Hull.RemoteDecal(decalId, spriteIndex, new Vector2(normalizedXPos, normalizedYPos), decalScale, decalAlpha));
				}
				break;
			}
			case Hull.EventType.BackgroundSections:
			{
				int i;
				int num;
				this.SharedBackgroundSectionRead(msg, delegate(Hull.BackgroundSectionNetworkUpdate bsnu)
				{
					int i = bsnu.SectionIndex;
					Color color = bsnu.Color;
					float colorStrength = bsnu.ColorStrength;
					BackgroundSection remoteBackgroundSection = this.remoteBackgroundSections.Find((BackgroundSection s) => (int)s.Index == i);
					if (remoteBackgroundSection != null)
					{
						remoteBackgroundSection.SetColorStrength(colorStrength);
						remoteBackgroundSection.SetColor(color);
						return;
					}
					this.remoteBackgroundSections.Add(new BackgroundSection(new Rectangle(0, 0, 1, 1), (ushort)i, colorStrength, color, 0));
				}, out num);
				this.paintAmount = this.BackgroundSections.Sum((BackgroundSection s) => s.ColorStrength);
				break;
			}
			case Hull.EventType.BallastFlora:
			{
				BallastFloraBehavior.NetworkHeader header = (BallastFloraBehavior.NetworkHeader)msg.ReadByte();
				if (header == BallastFloraBehavior.NetworkHeader.Spawn)
				{
					Identifier identifier = msg.ReadIdentifier();
					float x = msg.ReadSingle();
					float y = msg.ReadSingle();
					this.BallastFlora = new BallastFloraBehavior(this, BallastFloraPrefab.Find(identifier), new Vector2(x, y), true)
					{
						PowerConsumptionTimer = msg.ReadSingle()
					};
				}
				else
				{
					BallastFloraBehavior ballastFlora = this.BallastFlora;
					if (ballastFlora != null)
					{
						ballastFlora.ClientRead(msg, header);
					}
				}
				break;
			}
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Malformed incoming hull event: ");
				defaultInterpolatedStringHandler.AppendFormatted<Hull.EventType>(eventType);
				defaultInterpolatedStringHandler.AppendLiteral(" is not a supported event type");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			}
			if (this.serverUpdateDelay > 0f)
			{
				return;
			}
			this.ApplyRemoteState();
		}

		// Token: 0x06001BAB RID: 7083 RVA: 0x0011486C File Offset: 0x00112A6C
		private void ApplyRemoteState()
		{
			foreach (BackgroundSection remoteBackgroundSection in this.remoteBackgroundSections)
			{
				float prevColorStrength = this.BackgroundSections[(int)remoteBackgroundSection.Index].ColorStrength;
				this.BackgroundSections[(int)remoteBackgroundSection.Index].SetColor(remoteBackgroundSection.Color);
				this.BackgroundSections[(int)remoteBackgroundSection.Index].SetColorStrength(remoteBackgroundSection.ColorStrength);
				this.paintAmount = Math.Max(0f, this.paintAmount + (this.BackgroundSections[(int)remoteBackgroundSection.Index].ColorStrength - prevColorStrength) / (float)this.BackgroundSections.Count);
			}
			this.remoteBackgroundSections.Clear();
			if (this.remoteDecals.Count > 0)
			{
				this.decals.Clear();
				foreach (Hull.RemoteDecal remoteDecal in this.remoteDecals)
				{
					float decalPosX = MathHelper.Lerp((float)this.rect.X, (float)this.rect.Right, remoteDecal.NormalizedPos.X);
					float decalPosY = MathHelper.Lerp((float)(this.rect.Y - this.rect.Height), (float)this.rect.Y, remoteDecal.NormalizedPos.Y);
					if (base.Submarine != null)
					{
						decalPosX += base.Submarine.Position.X;
						decalPosY += base.Submarine.Position.Y;
					}
					Decal decal = this.AddDecal(remoteDecal.DecalId, new Vector2(decalPosX, decalPosY), remoteDecal.Scale, true, new int?(remoteDecal.SpriteIndex));
					decal.BaseAlpha = remoteDecal.DecalAlpha;
				}
				this.remoteDecals.Clear();
			}
			if (this.remoteFireSources == null)
			{
				return;
			}
			this.WaterVolume = this.remoteWaterVolume;
			this.OxygenPercentage = this.remoteOxygenPercentage;
			for (int i = 0; i < this.remoteFireSources.Length; i++)
			{
				Vector2 pos = this.remoteFireSources[i].Position;
				float size = this.remoteFireSources[i].Size;
				FireSource newFire = (i < this.FireSources.Count) ? this.FireSources[i] : new FireSource((base.Submarine == null) ? pos : (pos + base.Submarine.Position), null, null, true);
				newFire.Position = pos;
				newFire.Size = new Vector2(size, newFire.Size.Y);
				if (!this.FireSources.Contains(newFire))
				{
					newFire.Remove();
				}
			}
			for (int j = this.FireSources.Count - 1; j >= this.remoteFireSources.Length; j--)
			{
				this.FireSources[j].Remove();
				if (j < this.FireSources.Count)
				{
					this.FireSources.RemoveAt(j);
				}
			}
			this.remoteFireSources = null;
		}

		// Token: 0x17000718 RID: 1816
		// (get) Token: 0x06001BAC RID: 7084 RVA: 0x00114BCC File Offset: 0x00112DCC
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.properties;
			}
		}

		// Token: 0x17000719 RID: 1817
		// (get) Token: 0x06001BAD RID: 7085 RVA: 0x00114BD4 File Offset: 0x00112DD4
		public override string Name
		{
			get
			{
				return "Hull";
			}
		}

		// Token: 0x1700071A RID: 1818
		// (get) Token: 0x06001BAE RID: 7086 RVA: 0x00114BDB File Offset: 0x00112DDB
		// (set) Token: 0x06001BAF RID: 7087 RVA: 0x00114BE3 File Offset: 0x00112DE3
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x1700071B RID: 1819
		// (get) Token: 0x06001BB0 RID: 7088 RVA: 0x00114BEC File Offset: 0x00112DEC
		public IEnumerable<Identifier> OutpostModuleTags
		{
			get
			{
				return this.moduleTags;
			}
		}

		// Token: 0x1700071C RID: 1820
		// (get) Token: 0x06001BB1 RID: 7089 RVA: 0x00114BF4 File Offset: 0x00112DF4
		// (set) Token: 0x06001BB2 RID: 7090 RVA: 0x00114BFC File Offset: 0x00112DFC
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "RoomName.", false)]
		public string RoomName
		{
			get
			{
				return this.roomName;
			}
			set
			{
				if (this.roomName == value)
				{
					return;
				}
				this.roomName = value;
				this.DisplayName = TextManager.Get(this.roomName).Fallback(this.roomName, true);
				if (!this.IsWetRoom && this.ForceAsWetRoom)
				{
					this.IsWetRoom = true;
				}
			}
		}

		// Token: 0x1700071D RID: 1821
		// (get) Token: 0x06001BB3 RID: 7091 RVA: 0x00114C58 File Offset: 0x00112E58
		// (set) Token: 0x06001BB4 RID: 7092 RVA: 0x00114C60 File Offset: 0x00112E60
		[Editable]
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "", "", false)]
		public Color AmbientLight
		{
			get
			{
				return this.ambientLight;
			}
			set
			{
				this.ambientLight = value;
				this.lastAmbientLightEditTime = Timing.TotalTime;
			}
		}

		// Token: 0x1700071E RID: 1822
		// (get) Token: 0x06001BB5 RID: 7093 RVA: 0x00114C74 File Offset: 0x00112E74
		// (set) Token: 0x06001BB6 RID: 7094 RVA: 0x00114C7C File Offset: 0x00112E7C
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				float prevOxygenPercentage = this.OxygenPercentage;
				if (value.Width != this.rect.Width)
				{
					int arraySize = (int)Math.Ceiling((double)((float)value.Width / 32f + 1f));
					this.waveY = new float[arraySize];
					this.waveVel = new float[arraySize];
					this.leftDelta = new float[arraySize];
					this.rightDelta = new float[arraySize];
				}
				base.Rect = value;
				if (base.Submarine == null || !base.Submarine.Loading)
				{
					Item.UpdateHulls();
					Gap.UpdateHulls();
				}
				this.OxygenPercentage = prevOxygenPercentage;
				this.surface = (float)(this.rect.Y - this.rect.Height) + this.WaterVolume / (float)this.rect.Width;
				this.drawSurface = this.surface;
				this.Pressure = this.surface;
				this.CreateBackgroundSections();
			}
		}

		// Token: 0x1700071F RID: 1823
		// (get) Token: 0x06001BB7 RID: 7095 RVA: 0x00114D6B File Offset: 0x00112F6B
		public override bool Linkable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000720 RID: 1824
		// (get) Token: 0x06001BB8 RID: 7096 RVA: 0x00114D6E File Offset: 0x00112F6E
		// (set) Token: 0x06001BB9 RID: 7097 RVA: 0x00114D76 File Offset: 0x00112F76
		public float LethalPressure
		{
			get
			{
				return this.lethalPressure;
			}
			set
			{
				this.lethalPressure = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17000721 RID: 1825
		// (get) Token: 0x06001BBA RID: 7098 RVA: 0x00114D8E File Offset: 0x00112F8E
		public Vector2 Size
		{
			get
			{
				return new Vector2((float)this.rect.Width, (float)this.rect.Height);
			}
		}

		// Token: 0x17000722 RID: 1826
		// (get) Token: 0x06001BBB RID: 7099 RVA: 0x00114DAD File Offset: 0x00112FAD
		// (set) Token: 0x06001BBC RID: 7100 RVA: 0x00114DB5 File Offset: 0x00112FB5
		public float CeilingHeight { get; private set; }

		// Token: 0x17000723 RID: 1827
		// (get) Token: 0x06001BBD RID: 7101 RVA: 0x00114DBE File Offset: 0x00112FBE
		public float Surface
		{
			get
			{
				return this.surface;
			}
		}

		// Token: 0x17000724 RID: 1828
		// (get) Token: 0x06001BBE RID: 7102 RVA: 0x00114DC6 File Offset: 0x00112FC6
		public float WorldSurface
		{
			get
			{
				if (base.Submarine != null)
				{
					return this.surface + base.Submarine.Position.Y;
				}
				return this.surface;
			}
		}

		// Token: 0x17000725 RID: 1829
		// (get) Token: 0x06001BBF RID: 7103 RVA: 0x00114DEE File Offset: 0x00112FEE
		// (set) Token: 0x06001BC0 RID: 7104 RVA: 0x00114DF8 File Offset: 0x00112FF8
		public float WaterVolume
		{
			get
			{
				return this.waterVolume;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.waterVolume = MathHelper.Clamp(value, 0f, this.Volume * 1.05f);
				if (this.waterVolume <= this.Volume)
				{
					this.Pressure = (float)(this.rect.Y - this.rect.Height) + this.waterVolume / (float)this.rect.Width;
				}
				if (this.waterVolume > 0f)
				{
					this.update = true;
				}
			}
		}

		// Token: 0x17000726 RID: 1830
		// (get) Token: 0x06001BC1 RID: 7105 RVA: 0x00114E7F File Offset: 0x0011307F
		// (set) Token: 0x06001BC2 RID: 7106 RVA: 0x00114E87 File Offset: 0x00113087
		[Serialize(100000f, IsPropertySaveable.Yes, "", "", false)]
		public float Oxygen
		{
			get
			{
				return this.oxygen;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.oxygen = MathHelper.Clamp(value, 0f, this.Volume);
			}
		}

		// Token: 0x17000727 RID: 1831
		// (get) Token: 0x06001BC3 RID: 7107 RVA: 0x00114EA9 File Offset: 0x001130A9
		// (set) Token: 0x06001BC4 RID: 7108 RVA: 0x00114EB1 File Offset: 0x001130B1
		public bool IsAirlock { get; private set; }

		// Token: 0x17000728 RID: 1832
		// (get) Token: 0x06001BC5 RID: 7109 RVA: 0x00114EBC File Offset: 0x001130BC
		private bool ForceAsWetRoom
		{
			get
			{
				return this.roomName != null && (this.roomName.Contains("ballast", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("bilge", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("airlock", StringComparison.OrdinalIgnoreCase) || this.roomName.Contains("dockingport", StringComparison.OrdinalIgnoreCase));
			}
		}

		// Token: 0x17000729 RID: 1833
		// (get) Token: 0x06001BC6 RID: 7110 RVA: 0x00114F1F File Offset: 0x0011311F
		// (set) Token: 0x06001BC7 RID: 7111 RVA: 0x00114F27 File Offset: 0x00113127
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "It's normal for this hull to be filled with water. If the room name contains 'ballast', 'bilge', or 'airlock', you can't disable this setting.", "", false)]
		public bool IsWetRoom
		{
			get
			{
				return this.isWetRoom;
			}
			set
			{
				this.isWetRoom = value;
				if (this.ForceAsWetRoom)
				{
					this.isWetRoom = true;
				}
			}
		}

		// Token: 0x1700072A RID: 1834
		// (get) Token: 0x06001BC8 RID: 7112 RVA: 0x00114F3F File Offset: 0x0011313F
		// (set) Token: 0x06001BC9 RID: 7113 RVA: 0x00114F51 File Offset: 0x00113151
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Bots avoid staying here, but they are still allowed to access the room when needed and go through it. Forced true for wet rooms.", "", false)]
		public bool AvoidStaying
		{
			get
			{
				return this.avoidStaying || this.IsWetRoom;
			}
			set
			{
				this.avoidStaying = value;
				if (this.IsWetRoom)
				{
					this.avoidStaying = true;
				}
			}
		}

		// Token: 0x1700072B RID: 1835
		// (get) Token: 0x06001BCA RID: 7114 RVA: 0x00114F69 File Offset: 0x00113169
		public float WaterPercentage
		{
			get
			{
				return MathUtils.Percentage(this.WaterVolume, this.Volume);
			}
		}

		// Token: 0x1700072C RID: 1836
		// (get) Token: 0x06001BCB RID: 7115 RVA: 0x00114F7C File Offset: 0x0011317C
		// (set) Token: 0x06001BCC RID: 7116 RVA: 0x00114FA4 File Offset: 0x001131A4
		public float OxygenPercentage
		{
			get
			{
				if (this.Volume > 0f)
				{
					return this.oxygen / this.Volume * 100f;
				}
				return 100f;
			}
			set
			{
				this.Oxygen = value / 100f * this.Volume;
			}
		}

		// Token: 0x1700072D RID: 1837
		// (get) Token: 0x06001BCD RID: 7117 RVA: 0x00114FBA File Offset: 0x001131BA
		public float Volume
		{
			get
			{
				return (float)(this.rect.Width * this.rect.Height);
			}
		}

		// Token: 0x1700072E RID: 1838
		// (get) Token: 0x06001BCE RID: 7118 RVA: 0x00114FD4 File Offset: 0x001131D4
		// (set) Token: 0x06001BCF RID: 7119 RVA: 0x00114FDC File Offset: 0x001131DC
		public float Pressure
		{
			get
			{
				return this.pressure;
			}
			set
			{
				this.pressure = value;
			}
		}

		// Token: 0x1700072F RID: 1839
		// (get) Token: 0x06001BD0 RID: 7120 RVA: 0x00114FE5 File Offset: 0x001131E5
		public float[] WaveY
		{
			get
			{
				return this.waveY;
			}
		}

		// Token: 0x17000730 RID: 1840
		// (get) Token: 0x06001BD1 RID: 7121 RVA: 0x00114FED File Offset: 0x001131ED
		public float[] WaveVel
		{
			get
			{
				return this.waveVel;
			}
		}

		// Token: 0x17000731 RID: 1841
		// (get) Token: 0x06001BD2 RID: 7122 RVA: 0x00114FF5 File Offset: 0x001131F5
		// (set) Token: 0x06001BD3 RID: 7123 RVA: 0x00114FFD File Offset: 0x001131FD
		public List<BackgroundSection> BackgroundSections { get; private set; }

		// Token: 0x17000732 RID: 1842
		// (get) Token: 0x06001BD4 RID: 7124 RVA: 0x00115006 File Offset: 0x00113206
		public bool SupportsPaintedColors
		{
			get
			{
				return this.BackgroundSections != null;
			}
		}

		// Token: 0x17000733 RID: 1843
		// (get) Token: 0x06001BD5 RID: 7125 RVA: 0x00115011 File Offset: 0x00113211
		// (set) Token: 0x06001BD6 RID: 7126 RVA: 0x00115019 File Offset: 0x00113219
		public Color AveragePaintedColor { get; private set; }

		// Token: 0x17000734 RID: 1844
		// (get) Token: 0x06001BD7 RID: 7127 RVA: 0x00115022 File Offset: 0x00113222
		public bool IsRed
		{
			get
			{
				return ColorExtensions.IsRedDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x17000735 RID: 1845
		// (get) Token: 0x06001BD8 RID: 7128 RVA: 0x00115036 File Offset: 0x00113236
		public bool IsGreen
		{
			get
			{
				return ColorExtensions.IsGreenDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x17000736 RID: 1846
		// (get) Token: 0x06001BD9 RID: 7129 RVA: 0x0011504A File Offset: 0x0011324A
		public bool IsBlue
		{
			get
			{
				return ColorExtensions.IsBlueDominant(this.AveragePaintedColor, 2f, 100);
			}
		}

		// Token: 0x17000737 RID: 1847
		// (get) Token: 0x06001BDA RID: 7130 RVA: 0x0011505E File Offset: 0x0011325E
		// (set) Token: 0x06001BDB RID: 7131 RVA: 0x00115066 File Offset: 0x00113266
		public List<FireSource> FireSources { get; private set; }

		// Token: 0x17000738 RID: 1848
		// (get) Token: 0x06001BDC RID: 7132 RVA: 0x0011506F File Offset: 0x0011326F
		// (set) Token: 0x06001BDD RID: 7133 RVA: 0x00115077 File Offset: 0x00113277
		public List<DummyFireSource> FakeFireSources { get; private set; }

		// Token: 0x17000739 RID: 1849
		// (get) Token: 0x06001BDE RID: 7134 RVA: 0x00115080 File Offset: 0x00113280
		public int FireCount
		{
			get
			{
				List<FireSource> fireSources = this.FireSources;
				if (fireSources == null)
				{
					return 0;
				}
				return fireSources.Count;
			}
		}

		// Token: 0x1700073A RID: 1850
		// (get) Token: 0x06001BDF RID: 7135 RVA: 0x00115093 File Offset: 0x00113293
		// (set) Token: 0x06001BE0 RID: 7136 RVA: 0x0011509B File Offset: 0x0011329B
		public BallastFloraBehavior BallastFlora { get; set; }

		// Token: 0x06001BE1 RID: 7137 RVA: 0x001150A4 File Offset: 0x001132A4
		public Hull(Rectangle rectangle) : this(rectangle, Submarine.MainSub, 0)
		{
			if (SubEditorScreen.IsSubEditor())
			{
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
				{
					this
				}, false, true));
			}
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x001150D4 File Offset: 0x001132D4
		public Hull(Rectangle rectangle, Submarine submarine, ushort id = 0) : base(CoreEntityPrefab.HullPrefab, submarine, id)
		{
			this.rect = rectangle;
			if (this.BackgroundSections == null)
			{
				this.CreateBackgroundSections();
			}
			this.OxygenPercentage = 100f;
			this.FireSources = new List<FireSource>();
			this.FakeFireSources = new List<DummyFireSource>();
			this.properties = SerializableProperty.GetProperties(this);
			int arraySize = (int)Math.Ceiling((double)((float)rectangle.Width / 32f + 1f));
			this.waveY = new float[arraySize];
			this.waveVel = new float[arraySize];
			this.leftDelta = new float[arraySize];
			this.rightDelta = new float[arraySize];
			this.surface = (float)(this.rect.Y - this.rect.Height);
			if (((submarine != null) ? submarine.Info : null) != null && !submarine.Info.IsWreck)
			{
				this.aiTarget = new AITarget(this)
				{
					MinSightRange = 1000f,
					MaxSightRange = 5000f,
					SoundRange = 0f
				};
			}
			Hull.HullList.Add(this);
			if (submarine == null || !submarine.Loading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			this.CreateBackgroundSections();
			this.WaterVolume = 0f;
			base.InsertToList();
			DebugConsole.Log("Created hull (" + this.ID.ToString() + ")");
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x00115298 File Offset: 0x00113498
		public static Rectangle GetBorders()
		{
			if (!Hull.HullList.Any<Hull>())
			{
				return Rectangle.Empty;
			}
			Rectangle rect = Hull.HullList[0].rect;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Rect.X < rect.X)
				{
					rect.Width += rect.X - hull.rect.X;
					rect.X = hull.rect.X;
				}
				if (hull.rect.Right > rect.Right)
				{
					rect.Width = hull.rect.Right - rect.X;
				}
				if (hull.rect.Y > rect.Y)
				{
					rect.Height += hull.rect.Y - rect.Y;
					rect.Y = hull.rect.Y;
				}
				if (hull.rect.Y - hull.rect.Height < rect.Y - rect.Height)
				{
					rect.Height = rect.Y - (hull.rect.Y - hull.rect.Height);
				}
			}
			return rect;
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x00115418 File Offset: 0x00113618
		public override MapEntity Clone()
		{
			Hull clone = new Hull(this.rect, base.Submarine, 0);
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (property.Value.Attributes.OfType<Serialize>().Any<Serialize>())
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			clone.lastAmbientLightEditTime = 0.0;
			return clone;
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x001154C8 File Offset: 0x001136C8
		public static EntityGrid GenerateEntityGrid(Rectangle worldRect)
		{
			EntityGrid newGrid = new EntityGrid(worldRect, 200f);
			Hull.EntityGrids.Add(newGrid);
			return newGrid;
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x001154F0 File Offset: 0x001136F0
		public static EntityGrid GenerateEntityGrid(Submarine submarine)
		{
			EntityGrid newGrid = new EntityGrid(submarine, 200f);
			Hull.EntityGrids.Add(newGrid);
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine == submarine && !hull.IdFreed)
				{
					newGrid.InsertEntity(hull);
				}
			}
			return newGrid;
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x0011556C File Offset: 0x0011376C
		public void SetModuleTags(IEnumerable<Identifier> tags)
		{
			this.moduleTags.Clear();
			foreach (Identifier tag in tags)
			{
				this.moduleTags.Add(tag);
			}
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x001155C8 File Offset: 0x001137C8
		public override void OnMapLoaded()
		{
			this.CeilingHeight = (float)this.Rect.Height;
			Body lowerPickedBody = Submarine.PickBody(this.SimPosition, this.SimPosition - new Vector2(0f, ConvertUnits.ToSimUnits((float)this.rect.Height / 2f + 0.1f)), null, new Category?(Category.Cat1), true, null, false);
			if (lowerPickedBody != null)
			{
				Vector2 lowerPickedPos = Submarine.LastPickedPosition;
				if (Submarine.PickBody(this.SimPosition, this.SimPosition + new Vector2(0f, ConvertUnits.ToSimUnits((float)this.rect.Height / 2f + 0.1f)), null, new Category?(Category.Cat1), true, null, false) != null)
				{
					Vector2 upperPickedPos = Submarine.LastPickedPosition;
					this.CeilingHeight = ConvertUnits.ToDisplayUnits(upperPickedPos.Y - lowerPickedPos.Y);
				}
			}
			this.Pressure = (float)(this.rect.Y - this.rect.Height) + this.waterVolume / (float)this.rect.Width;
			this.DetermineIsAirlock();
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.OnMapLoaded();
			}
			this.lastAmbientLightEditTime = 0.0;
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x001156F8 File Offset: 0x001138F8
		public void AddToGrid(Submarine submarine)
		{
			foreach (EntityGrid grid in Hull.EntityGrids)
			{
				if (grid.Submarine == submarine)
				{
					this.rect.Location = this.rect.Location - MathUtils.ToPoint(submarine.HiddenSubPosition);
					grid.InsertEntity(this);
					this.rect.Location = this.rect.Location + MathUtils.ToPoint(submarine.HiddenSubPosition);
					break;
				}
			}
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x00115798 File Offset: 0x00113998
		public int GetWaveIndex(Vector2 position)
		{
			return this.GetWaveIndex(position.X);
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x001157A8 File Offset: 0x001139A8
		public int GetWaveIndex(float xPos)
		{
			int index = (int)(xPos - (float)this.rect.X) / 32;
			return MathHelper.Clamp(index, 0, this.waveY.Length - 1);
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x001157DC File Offset: 0x001139DC
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move a hull by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			this.rect.X = this.rect.X + (int)amount.X;
			this.rect.Y = this.rect.Y + (int)amount.Y;
			if (base.Submarine == null || !base.Submarine.Loading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			this.surface = (float)(this.rect.Y - this.rect.Height) + this.WaterVolume / (float)this.rect.Width;
			this.drawSurface = this.surface;
			this.Pressure = this.surface;
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x001158D4 File Offset: 0x00113AD4
		public override void ShallowRemove()
		{
			base.Remove();
			Hull.HullList.Remove(this);
			if (base.Submarine == null || (!base.Submarine.Loading && !Submarine.Unloading))
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			List<FireSource> fireSourcesToRemove = new List<FireSource>(this.FireSources);
			fireSourcesToRemove.AddRange(this.FakeFireSources);
			foreach (FireSource fireSource in fireSourcesToRemove)
			{
				fireSource.Remove();
			}
			this.FireSources.Clear();
			this.FakeFireSources.Clear();
			if (Hull.EntityGrids != null)
			{
				foreach (EntityGrid entityGrid in Hull.EntityGrids)
				{
					entityGrid.RemoveEntity(this);
				}
			}
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x001159D4 File Offset: 0x00113BD4
		public override void Remove()
		{
			base.Remove();
			Hull.HullList.Remove(this);
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Remove();
			}
			if (base.Submarine != null && !base.Submarine.Loading && !Submarine.Unloading)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			List<BackgroundSection> backgroundSections = this.BackgroundSections;
			if (backgroundSections != null)
			{
				backgroundSections.Clear();
			}
			List<FireSource> fireSourcesToRemove = new List<FireSource>(this.FireSources);
			foreach (FireSource fireSource in fireSourcesToRemove)
			{
				fireSource.Remove();
			}
			this.FireSources.Clear();
			if (Hull.EntityGrids != null)
			{
				foreach (EntityGrid entityGrid in Hull.EntityGrids)
				{
					entityGrid.RemoveEntity(this);
				}
			}
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x00115ADC File Offset: 0x00113CDC
		public void AddFireSource(FireSource fireSource)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && base.IdFreed)
			{
				return;
			}
			DummyFireSource dummyFire = fireSource as DummyFireSource;
			if (dummyFire != null)
			{
				this.FakeFireSources.Add(dummyFire);
				return;
			}
			if (this.FireSources.Count >= 16)
			{
				return;
			}
			this.FireSources.Add(fireSource);
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x00115B38 File Offset: 0x00113D38
		public Decal AddDecal(uint decalId, Vector2 worldPosition, float scale, bool isNetworkEvent, int? spriteIndex = null)
		{
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return null;
			}
			DecalPrefab decal = DecalManager.Prefabs.Find((DecalPrefab p) => p.UintIdentifier == decalId);
			if (decal == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a decal prefab with the UInt identifier ");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(decalId);
				defaultInterpolatedStringHandler.AppendLiteral("!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return null;
			}
			return this.AddDecal(decal.Name, worldPosition, scale, isNetworkEvent, spriteIndex);
		}

		// Token: 0x06001BF1 RID: 7153 RVA: 0x00115BD8 File Offset: 0x00113DD8
		public Decal AddDecal(string decalName, Vector2 worldPosition, float scale, bool isNetworkEvent, int? spriteIndex = null)
		{
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return null;
			}
			if (this.decals.Count >= 10)
			{
				return null;
			}
			Decal decal = DecalManager.CreateDecal(decalName, scale, worldPosition, this, spriteIndex);
			if (decal != null)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, default(Hull.DecalEventData));
				}
				this.decals.Add(decal);
			}
			return decal;
		}

		// Token: 0x06001BF2 RID: 7154 RVA: 0x00115C58 File Offset: 0x00113E58
		private void SharedStatusWrite(IWriteMessage msg)
		{
			msg.WriteSingle(this.waterVolume);
			msg.WriteRangedInteger(Math.Min(this.FireSources.Count, 16), 0, 16);
			for (int i = 0; i < Math.Min(this.FireSources.Count, 16); i++)
			{
				FireSource fireSource = this.FireSources[i];
				Vector2 normalizedPos = new Vector2((fireSource.Position.X - (float)this.rect.X) / (float)this.rect.Width, (fireSource.Position.Y - (float)(this.rect.Y - this.rect.Height)) / (float)this.rect.Height);
				msg.WriteRangedSingle(MathHelper.Clamp(normalizedPos.X, 0f, 1f), 0f, 1f, 8);
				msg.WriteRangedSingle(MathHelper.Clamp(normalizedPos.Y, 0f, 1f), 0f, 1f, 8);
				msg.WriteRangedSingle(MathHelper.Clamp(fireSource.Size.X / (float)this.rect.Width, 0f, 1f), 0f, 1f, 8);
			}
		}

		// Token: 0x06001BF3 RID: 7155 RVA: 0x00115DA0 File Offset: 0x00113FA0
		private void SharedBackgroundSectionsWrite(IWriteMessage msg, in Hull.BackgroundSectionsEventData backgroundSectionsEventData)
		{
			int sectorToUpdate = backgroundSectionsEventData.SectorStartIndex;
			int start = sectorToUpdate * 16;
			int end = Math.Min((sectorToUpdate + 1) * 16, this.BackgroundSections.Count - 1);
			msg.WriteRangedInteger(sectorToUpdate, 0, this.BackgroundSections.Count - 1);
			for (int i = start; i <= end; i++)
			{
				msg.WriteRangedSingle(this.BackgroundSections[i].ColorStrength, 0f, 1f, 8);
				msg.WriteUInt32(this.BackgroundSections[i].Color.PackedValue);
			}
		}

		// Token: 0x06001BF4 RID: 7156 RVA: 0x00115E38 File Offset: 0x00114038
		private void SharedStatusRead(IReadMessage msg, out float newWaterVolume, out Hull.NetworkFireSource[] newFireSources)
		{
			newWaterVolume = msg.ReadSingle();
			int fireSourceCount = msg.ReadRangedInteger(0, 16);
			newFireSources = new Hull.NetworkFireSource[fireSourceCount];
			for (int i = 0; i < fireSourceCount; i++)
			{
				float x = MathHelper.Clamp(msg.ReadRangedSingle(0f, 1f, 8), 0.05f, 0.95f);
				float y = MathHelper.Clamp(msg.ReadRangedSingle(0f, 1f, 8), 0.05f, 0.95f);
				float size = msg.ReadRangedSingle(0f, 1f, 8);
				newFireSources[i] = new Hull.NetworkFireSource(this, new Vector2(x, y), size);
			}
		}

		// Token: 0x06001BF5 RID: 7157 RVA: 0x00115ED8 File Offset: 0x001140D8
		private void SharedBackgroundSectionRead(IReadMessage msg, Action<Hull.BackgroundSectionNetworkUpdate> action, out int sectionToUpdate)
		{
			sectionToUpdate = msg.ReadRangedInteger(0, this.BackgroundSections.Count - 1);
			int start = sectionToUpdate * 16;
			int end = Math.Min((sectionToUpdate + 1) * 16, this.BackgroundSections.Count - 1);
			for (int i = start; i <= end; i++)
			{
				float colorStrength = msg.ReadRangedSingle(0f, 1f, 8);
				Color color = new Color(msg.ReadUInt32());
				action(new Hull.BackgroundSectionNetworkUpdate(i, color, colorStrength));
			}
		}

		// Token: 0x06001BF6 RID: 7158 RVA: 0x00115F58 File Offset: 0x00114158
		public override void Update(float deltaTime, Camera cam)
		{
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Update(deltaTime);
			}
			this.UpdateProjSpecific(deltaTime, cam);
			this.Oxygen -= 0.3f * deltaTime;
			if (this.FakeFireSources.Count > 0)
			{
				Character controlled = Character.Controlled;
				float? num;
				if (controlled == null)
				{
					num = null;
				}
				else
				{
					CharacterHealth characterHealth = controlled.CharacterHealth;
					if (characterHealth == null)
					{
						num = null;
					}
					else
					{
						Affliction affliction = characterHealth.GetAffliction("psychosis", true);
						num = ((affliction != null) ? new float?(affliction.Strength) : null);
					}
				}
				float? num2 = num;
				if (num2.GetValueOrDefault() <= 0f)
				{
					for (int i = this.FakeFireSources.Count - 1; i >= 0; i--)
					{
						if (this.FakeFireSources[i].CausedByPsychosis)
						{
							this.FakeFireSources[i].Remove();
						}
					}
				}
				FireSource.UpdateAll(this.FakeFireSources, deltaTime);
			}
			FireSource.UpdateAll(this.FireSources, deltaTime);
			foreach (Decal decal in this.decals)
			{
				decal.Update(deltaTime);
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsClient)
			{
				for (int j = this.decals.Count - 1; j >= 0; j--)
				{
					Decal decal2 = this.decals[j];
					if (decal2.FadeTimer >= decal2.LifeTime || decal2.BaseAlpha <= 0.001f)
					{
						this.decals.RemoveAt(j);
					}
				}
			}
			if (this.aiTarget != null)
			{
				this.aiTarget.SightRange = ((base.Submarine == null) ? this.aiTarget.MinSightRange : MathHelper.Lerp(this.aiTarget.MinSightRange, this.aiTarget.MaxSightRange, base.Submarine.Velocity.Length() / 10f));
				this.aiTarget.SoundRange -= deltaTime * 1000f;
			}
			if (!this.update)
			{
				this.lethalPressure = 0f;
				return;
			}
			float waterDepth = this.WaterVolume / (float)this.rect.Width;
			if (waterDepth < 1f)
			{
				waterDepth = 0f;
			}
			this.surface = Math.Max(MathHelper.Lerp(this.surface, (float)(this.rect.Y - this.rect.Height) + waterDepth, deltaTime * 10f), (float)(this.rect.Y - this.rect.Height));
			for (int k = 0; k < this.waveY.Length; k++)
			{
				this.waveY[k] = this.waveY[k] + this.waveVel[k];
				if (this.surface + this.waveY[k] > (float)this.rect.Y)
				{
					float excess = this.surface + this.waveY[k] - (float)this.rect.Y;
					this.waveY[k] -= excess;
					this.waveVel[k] = this.waveVel[k] * -0.5f;
				}
				else if (this.surface + this.waveY[k] < (float)(this.rect.Y - this.rect.Height))
				{
					float excess2 = this.surface + this.waveY[k] - (float)(this.rect.Y - this.rect.Height);
					this.waveY[k] -= excess2;
					this.waveVel[k] = this.waveVel[k] * -0.5f;
				}
				float a = -Hull.WaveStiffness * this.waveY[k] - this.waveVel[k] * Hull.WaveDampening;
				this.waveVel[k] = this.waveVel[k] + a;
			}
			for (int l = 0; l < 2; l++)
			{
				for (int m = 1; m < this.waveY.Length - 1; m++)
				{
					this.leftDelta[m] = Hull.WaveSpread * (this.waveY[m] - this.waveY[m - 1]);
					this.waveVel[m - 1] += this.leftDelta[m];
					this.rightDelta[m] = Hull.WaveSpread * (this.waveY[m] - this.waveY[m + 1]);
					this.waveVel[m + 1] += this.rightDelta[m];
				}
			}
			foreach (Gap gap in this.ConnectedGaps)
			{
				if (this == gap.linkedTo.FirstOrDefault<MapEntity>() as Hull && gap.IsRoomToRoom && gap.IsHorizontal && gap.Open > 0f && this.surface <= (float)gap.Rect.Y && this.surface >= (float)(gap.Rect.Y - gap.Rect.Height))
				{
					Hull hull2 = (this == gap.linkedTo[0]) ? ((Hull)gap.linkedTo[1]) : ((Hull)gap.linkedTo[0]);
					float otherSurfaceY = hull2.surface;
					if (otherSurfaceY <= (float)gap.Rect.Y && otherSurfaceY >= (float)(gap.Rect.Y - gap.Rect.Height))
					{
						float surfaceDiff = (this.surface - otherSurfaceY) * gap.Open;
						for (int n = 0; n < 2; n++)
						{
							this.rightDelta[this.waveY.Length - 1] = Hull.WaveSpread * (hull2.waveY[0] - this.waveY[this.waveY.Length - 1] - surfaceDiff) * 0.5f;
							this.waveVel[this.waveY.Length - 1] += this.rightDelta[this.waveY.Length - 1];
							this.waveY[this.waveY.Length - 1] += this.rightDelta[this.waveY.Length - 1];
							hull2.leftDelta[0] = Hull.WaveSpread * (this.waveY[this.waveY.Length - 1] - hull2.waveY[0] + surfaceDiff) * 0.5f;
							hull2.waveVel[0] += hull2.leftDelta[0];
							hull2.waveY[0] += hull2.leftDelta[0];
						}
						if (surfaceDiff < 32f)
						{
							hull2.waveY[0] = surfaceDiff * 0.5f;
							this.waveY[this.waveY.Length - 1] = -surfaceDiff * 0.5f;
						}
					}
				}
			}
			for (int j2 = 0; j2 < 2; j2++)
			{
				for (int i2 = 1; i2 < this.waveY.Length - 1; i2++)
				{
					this.waveY[i2 - 1] += this.leftDelta[i2];
					this.waveY[i2 + 1] += this.rightDelta[i2];
				}
			}
			if (this.waterVolume < this.Volume)
			{
				float waterVolumeFactor = Math.Max((100f - this.WaterPercentage) / 10f, 1f);
				this.LethalPressure -= 10f * waterVolumeFactor * deltaTime;
				if (this.WaterVolume <= 0f)
				{
					if (this.drawSurface > (float)(this.rect.Y - this.rect.Height + 1))
					{
						return;
					}
					for (int i3 = 1; i3 < this.waveY.Length - 1; i3++)
					{
						if (this.waveY[i3] > 0.1f)
						{
							return;
						}
					}
					this.update = false;
				}
			}
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x001167BC File Offset: 0x001149BC
		private void UpdateProjSpecific(float deltaTime, Camera cam)
		{
			float waterDepth = this.WaterVolume / (float)this.rect.Width;
			this.drawSurface = Math.Max(MathHelper.Lerp(this.drawSurface, (float)(this.rect.Y - this.rect.Height) + waterDepth, deltaTime * 10f), (float)(this.rect.Y - this.rect.Height));
			if (GameMain.Client != null)
			{
				this.serverUpdateDelay -= deltaTime;
				if (this.serverUpdateDelay <= 0f)
				{
					this.ApplyRemoteState();
				}
				if (this.networkUpdatePending)
				{
					this.networkUpdateTimer += deltaTime;
					if (this.networkUpdateTimer > 0.2f)
					{
						if (!this.pendingSectorUpdates.Any<int>() && !this.pendingDecalUpdates.Any<Decal>())
						{
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.CreateEntityEvent(this, default(Hull.StatusEventData), false);
							}
						}
						foreach (Decal decal in this.pendingDecalUpdates)
						{
							GameClient client2 = GameMain.Client;
							if (client2 != null)
							{
								client2.CreateEntityEvent(this, new Hull.DecalEventData(decal));
							}
						}
						this.pendingDecalUpdates.Clear();
						foreach (int pendingSectorUpdate in this.pendingSectorUpdates)
						{
							GameClient client3 = GameMain.Client;
							if (client3 != null)
							{
								client3.CreateEntityEvent(this, new Hull.BackgroundSectionsEventData(pendingSectorUpdate));
							}
						}
						this.pendingSectorUpdates.Clear();
						this.networkUpdatePending = false;
						this.networkUpdateTimer = 0f;
					}
				}
			}
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x00116998 File Offset: 0x00114B98
		public void ApplyFlowForces(float deltaTime, Item item)
		{
			if (item.body.Mass <= 0f)
			{
				return;
			}
			foreach (Gap gap2 in from gap in this.ConnectedGaps
			where gap.Open > 0f
			select gap)
			{
				float distance = MathHelper.Max(Vector2.DistanceSquared(item.Position, gap2.Position) / 1000f, 1f);
				Vector2 force = gap2.LerpedFlowForce / distance * deltaTime;
				if (force.LengthSquared() > 0.01f)
				{
					item.body.ApplyForce(force, 64f);
				}
			}
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x00116A6C File Offset: 0x00114C6C
		public void Extinguish(float deltaTime, float amount, Vector2 position, bool extinguishRealFires = true, bool extinguishFakeFires = true)
		{
			if (extinguishRealFires)
			{
				for (int i = this.FireSources.Count - 1; i >= 0; i--)
				{
					this.FireSources[i].Extinguish(deltaTime, amount, position);
				}
			}
			if (extinguishFakeFires)
			{
				for (int j = this.FakeFireSources.Count - 1; j >= 0; j--)
				{
					this.FakeFireSources[j].Extinguish(deltaTime, amount, position);
				}
			}
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x00116ADC File Offset: 0x00114CDC
		public void RemoveFire(FireSource fire)
		{
			this.FireSources.Remove(fire);
			DummyFireSource dummyFire = fire as DummyFireSource;
			if (dummyFire != null)
			{
				this.FakeFireSources.Remove(dummyFire);
			}
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x00116B10 File Offset: 0x00114D10
		public IEnumerable<Hull> GetConnectedHulls(bool includingThis, int? searchDepth = null, bool ignoreClosedGaps = false)
		{
			this.adjacentHulls.Clear();
			int startStep = 0;
			int value = searchDepth.GetValueOrDefault();
			if (searchDepth == null)
			{
				value = 100;
				searchDepth = new int?(value);
			}
			this.GetAdjacentHulls(this.adjacentHulls, ref startStep, searchDepth.Value, ignoreClosedGaps);
			if (!includingThis)
			{
				this.adjacentHulls.Remove(this);
			}
			return this.adjacentHulls;
		}

		// Token: 0x06001BFC RID: 7164 RVA: 0x00116B74 File Offset: 0x00114D74
		private void GetAdjacentHulls(HashSet<Hull> connectedHulls, ref int step, int searchDepth, bool ignoreClosedGaps = false)
		{
			connectedHulls.Add(this);
			if (step > searchDepth)
			{
				return;
			}
			foreach (Gap g in this.ConnectedGaps)
			{
				if (!ignoreClosedGaps || g.Open > 0f)
				{
					int i = 0;
					while (i < 2 && i < g.linkedTo.Count)
					{
						Hull hull = g.linkedTo[i] as Hull;
						if (hull != null && !connectedHulls.Contains(hull))
						{
							step++;
							hull.GetAdjacentHulls(connectedHulls, ref step, searchDepth, ignoreClosedGaps);
						}
						i++;
					}
				}
			}
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x00116C28 File Offset: 0x00114E28
		public float GetApproximateDistance(Vector2 startPos, Vector2 endPos, Hull targetHull, float maxDistance, float distanceMultiplierPerClosedDoor = 0f, float minimumGapOpenness = 0.5f)
		{
			Hull.cachedDistances.Clear();
			Hull.priorityQueue.Clear();
			Hull.cachedDistances[this] = 0f;
			Hull.priorityQueue.Enqueue(new ValueTuple<Hull, Vector2>(this, startPos), 0f);
			ValueTuple<Hull, Vector2> current;
			float currentDist;
			while (Hull.priorityQueue.TryDequeue(out current, out currentDist))
			{
				Hull currentHull = current.Item1;
				Vector2 currentPos = current.Item2;
				if (currentDist > maxDistance)
				{
					return float.MaxValue;
				}
				if (currentHull == targetHull)
				{
					return currentDist + Vector2.Distance(currentPos, endPos);
				}
				using (List<Gap>.Enumerator enumerator = currentHull.ConnectedGaps.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Gap g = enumerator.Current;
						float distanceMultiplier = 1f;
						if (g.ConnectedDoor != null && !g.ConnectedDoor.IsBroken)
						{
							if (((g.ConnectedDoor.IsClosed && g.ConnectedDoor.PredictedState == null) || (g.ConnectedDoor.PredictedState != null && !g.ConnectedDoor.PredictedState.Value)) && g.ConnectedDoor.OpenState < 0.1f)
							{
								if (distanceMultiplierPerClosedDoor <= 0f)
								{
									continue;
								}
								distanceMultiplier *= distanceMultiplierPerClosedDoor;
							}
						}
						else if (g.Open < minimumGapOpenness)
						{
							continue;
						}
						int i = 0;
						while (i < 2 && i < g.linkedTo.Count)
						{
							Hull nextHull = g.linkedTo[i] as Hull;
							if (nextHull != null && nextHull != currentHull)
							{
								float newDist = currentDist + Vector2.Distance(currentPos, g.Position) * distanceMultiplier;
								float oldDist;
								if (!Hull.cachedDistances.TryGetValue(nextHull, out oldDist) || newDist < oldDist)
								{
									Hull.cachedDistances[nextHull] = newDist;
									Hull.priorityQueue.Enqueue(new ValueTuple<Hull, Vector2>(nextHull, g.Position), newDist);
								}
							}
							i++;
						}
					}
					continue;
				}
				break;
			}
			return float.MaxValue;
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x00116E44 File Offset: 0x00115044
		public static Hull FindHull(Vector2 position, Hull guess = null, bool useWorldCoordinates = true, bool inclusive = true)
		{
			if (Hull.EntityGrids == null)
			{
				return null;
			}
			if (guess != null && Submarine.RectContains(useWorldCoordinates ? guess.WorldRect : guess.rect, position, inclusive))
			{
				return guess;
			}
			foreach (EntityGrid entityGrid in Hull.EntityGrids)
			{
				if (entityGrid.Submarine != null && !entityGrid.Submarine.Loading)
				{
					Rectangle borders = entityGrid.Submarine.Borders;
					if (useWorldCoordinates)
					{
						Vector2 worldPos = entityGrid.Submarine.WorldPosition;
						borders.Location += new Point((int)worldPos.X, (int)worldPos.Y);
					}
					else
					{
						borders.Location += new Point((int)entityGrid.Submarine.HiddenSubPosition.X, (int)entityGrid.Submarine.HiddenSubPosition.Y);
					}
					if (position.X < (float)borders.X - 128f || position.X > (float)borders.Right + 128f || position.Y > (float)borders.Y + 128f || position.Y < (float)(borders.Y - borders.Height) - 128f)
					{
						continue;
					}
				}
				Vector2 transformedPosition = position;
				if (useWorldCoordinates && entityGrid.Submarine != null)
				{
					transformedPosition -= entityGrid.Submarine.Position;
				}
				List<MapEntity> entities = entityGrid.GetEntities(transformedPosition);
				if (entities != null)
				{
					foreach (MapEntity mapEntity in entities)
					{
						Hull hull = (Hull)mapEntity;
						if (Submarine.RectContains(hull.rect, transformedPosition, inclusive))
						{
							return hull;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x0011705C File Offset: 0x0011525C
		public static Hull FindHullUnoptimized(Vector2 position, Hull guess = null, bool useWorldCoordinates = true, bool inclusive = true)
		{
			if (guess != null && Hull.HullList.Contains(guess) && Submarine.RectContains(useWorldCoordinates ? guess.WorldRect : guess.rect, position, inclusive))
			{
				return guess;
			}
			foreach (Hull hull in Hull.HullList)
			{
				if (Submarine.RectContains(useWorldCoordinates ? hull.WorldRect : hull.rect, position, inclusive))
				{
					return hull;
				}
			}
			return null;
		}

		// Token: 0x06001C00 RID: 7168 RVA: 0x001170F4 File Offset: 0x001152F4
		public void GetLinkedHulls(List<Hull> linkedHulls, bool includeHiddenHulls = false)
		{
			foreach (MapEntity linkedEntity in this.linkedTo)
			{
				Hull linkedHull = linkedEntity as Hull;
				if (linkedHull != null && !linkedHulls.Contains(linkedHull) && (includeHiddenHulls || !linkedHull.IsHidden))
				{
					linkedHulls.Add(linkedHull);
					linkedHull.GetLinkedHulls(linkedHulls, includeHiddenHulls);
				}
			}
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x00117170 File Offset: 0x00115370
		public static void DetectItemVisibility(Character c = null)
		{
			if (c == null)
			{
				using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item it = enumerator.Current;
						it.Visible = true;
					}
					return;
				}
			}
			Hull h = c.CurrentHull;
			Hull.HullList.ForEach(delegate(Hull j)
			{
				j.Visible = false;
			});
			List<Hull> visibleHulls;
			if (h == null || c.Submarine == null)
			{
				visibleHulls = Hull.HullList.FindAll((Hull j) => j.CanSeeOther(null, false));
			}
			else
			{
				visibleHulls = Hull.HullList.FindAll((Hull j) => h.CanSeeOther(j, true));
			}
			visibleHulls.ForEach(delegate(Hull j)
			{
				j.Visible = true;
			});
			foreach (Item it2 in Item.ItemList)
			{
				if (it2.CurrentHull == null || visibleHulls.Contains(it2.CurrentHull))
				{
					it2.Visible = true;
				}
				else
				{
					it2.Visible = false;
				}
			}
		}

		// Token: 0x06001C02 RID: 7170 RVA: 0x001172E4 File Offset: 0x001154E4
		private bool CanSeeOther(Hull other, bool allowIndirect = true)
		{
			if (other == this)
			{
				return true;
			}
			if (other != null && other.Submarine == base.Submarine)
			{
				using (List<Gap>.Enumerator enumerator = this.ConnectedGaps.GetEnumerator())
				{
					Func<Hull, bool> <>9__1;
					Func<Hull, bool> <>9__2;
					while (enumerator.MoveNext())
					{
						Gap g = enumerator.Current;
						if (g.ConnectedWall == null || !g.ConnectedWall.CastShadow)
						{
							List<Hull> otherHulls = Hull.HullList.FindAll((Hull h) => h.ConnectedGaps.Contains(g) && h != this);
							IEnumerable<Hull> source = otherHulls;
							Func<Hull, bool> predicate;
							if ((predicate = <>9__1) == null)
							{
								predicate = (<>9__1 = ((Hull h) => h == other));
							}
							bool retVal = source.Any(predicate);
							if (!retVal && allowIndirect)
							{
								IEnumerable<Hull> source2 = otherHulls;
								Func<Hull, bool> predicate2;
								if ((predicate2 = <>9__2) == null)
								{
									predicate2 = (<>9__2 = ((Hull h) => h.CanSeeOther(other, false)));
								}
								retVal = source2.Any(predicate2);
							}
							if (retVal)
							{
								return true;
							}
						}
					}
					return false;
				}
			}
			using (List<Gap>.Enumerator enumerator2 = this.ConnectedGaps.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Gap g = enumerator2.Current;
					if (g.ConnectedDoor != null && !Hull.HullList.Any((Hull h) => h.ConnectedGaps.Contains(g) && h != this))
					{
						return true;
					}
				}
			}
			List<MapEntity> structures = MapEntity.MapEntityList.FindAll((MapEntity me) => me is Structure && me.Rect.Intersects(this.Rect));
			return structures.Any((MapEntity st) => !(st as Structure).CastShadow);
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x0011750C File Offset: 0x0011570C
		public string CreateRoomName()
		{
			List<string> roomItems = new List<string>();
			foreach (Item item in Item.ItemList)
			{
				if (item.CurrentHull == this)
				{
					if (item.GetComponent<Reactor>() != null)
					{
						roomItems.Add("reactor");
					}
					if (item.GetComponent<Engine>() != null)
					{
						roomItems.Add("engine");
					}
					if (item.GetComponent<Steering>() != null)
					{
						roomItems.Add("steering");
					}
					if (item.GetComponent<Sonar>() != null)
					{
						roomItems.Add("sonar");
					}
					if (item.HasTag(Tags.Ballast))
					{
						roomItems.Add("ballast");
					}
				}
			}
			if (roomItems.Contains("reactor"))
			{
				return "RoomName.ReactorRoom";
			}
			if (roomItems.Contains("engine"))
			{
				return "RoomName.EngineRoom";
			}
			if (roomItems.Contains("steering") && roomItems.Contains("sonar"))
			{
				return "RoomName.CommandRoom";
			}
			if (roomItems.Contains("ballast"))
			{
				return "RoomName.Ballast";
			}
			Submarine submarine = base.Submarine;
			IEnumerable<Identifier> enumerable;
			if (submarine == null)
			{
				enumerable = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				if (info == null)
				{
					enumerable = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo = info.OutpostModuleInfo;
					enumerable = ((outpostModuleInfo != null) ? outpostModuleInfo.ModuleFlags : null);
				}
			}
			IEnumerable<Identifier> moduleFlags = enumerable ?? this.moduleTags;
			if (moduleFlags != null && moduleFlags.Any<Identifier>() && (base.Submarine.Info.Type == SubmarineType.OutpostModule || base.Submarine.Info.Type == SubmarineType.Outpost))
			{
				if (moduleFlags.Contains(Tags.Airlock))
				{
					if (this.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.ConnectedDoor != null))
					{
						return "RoomName.Airlock";
					}
				}
			}
			else if (this.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.ConnectedDoor != null))
			{
				return "RoomName.Airlock";
			}
			Rectangle subRect = base.Submarine.Borders;
			Alignment roomPos;
			if ((float)(this.rect.Y - this.rect.Height / 2) > (float)subRect.Y + (float)subRect.Height * 0.66f)
			{
				roomPos = Alignment.Top;
			}
			else if ((float)(this.rect.Y - this.rect.Height / 2) > (float)subRect.Y + (float)subRect.Height * 0.33f)
			{
				roomPos = Alignment.CenterY;
			}
			else
			{
				roomPos = Alignment.Bottom;
			}
			if ((float)this.rect.Center.X < (float)subRect.X + (float)subRect.Width * 0.33f)
			{
				roomPos |= Alignment.Left;
			}
			else if ((float)this.rect.Center.X < (float)subRect.X + (float)subRect.Width * 0.66f)
			{
				roomPos |= Alignment.CenterX;
			}
			else
			{
				roomPos |= Alignment.Right;
			}
			return "RoomName.Sub" + roomPos.ToString();
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x001177F8 File Offset: 0x001159F8
		private void DetermineIsAirlock()
		{
			if (this.RoomName != null && this.RoomName.Contains("airlock", StringComparison.OrdinalIgnoreCase))
			{
				this.IsAirlock = true;
				return;
			}
			Identifier airlockTag = "airlock".ToIdentifier();
			foreach (Item item in Item.ItemList)
			{
				if (item.CurrentHull != this && item.HasTag(airlockTag))
				{
					this.IsAirlock = true;
					return;
				}
			}
			this.IsAirlock = false;
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x00117894 File Offset: 0x00115A94
		public bool LeadsOutside(Character character)
		{
			foreach (Gap gap in this.ConnectedGaps)
			{
				if (gap.ConnectedDoor != null && !gap.IsRoomToRoom && (gap.ConnectedDoor.CanBeTraversed || (character != null && gap.ConnectedDoor.HasAccess(character))))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x00117918 File Offset: 0x00115B18
		private void CreateBackgroundSections()
		{
			int sectionWidth;
			int sectionHeight = sectionWidth = 16;
			this.xBackgroundMax = this.rect.Width / sectionWidth;
			this.yBackgroundMax = this.rect.Height / sectionHeight;
			this.BackgroundSections = new List<BackgroundSection>(this.xBackgroundMax * this.yBackgroundMax);
			int sections = this.xBackgroundMax * this.yBackgroundMax;
			float xSectors = (float)this.xBackgroundMax / 4f;
			for (int y = 0; y < this.yBackgroundMax; y++)
			{
				for (int x = 0; x < this.xBackgroundMax; x++)
				{
					ushort index = (ushort)this.BackgroundSections.Count;
					int sector = (int)Math.Floor((double)((float)index / 4f - xSectors * (float)y)) + y / 4 * (int)Math.Ceiling((double)xSectors);
					this.BackgroundSections.Add(new BackgroundSection(new Rectangle(x * sectionWidth, y * -sectionHeight, sectionWidth, sectionHeight), index, (ushort)y));
				}
			}
			this.minimumPaintAmountToDraw = 0.7f / (float)this.BackgroundSections.Count;
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x00117A20 File Offset: 0x00115C20
		public static Hull GetCleanTarget(Vector2 worldPosition)
		{
			foreach (Hull hull in Hull.HullList)
			{
				Rectangle worldRect = hull.WorldRect;
				if (worldPosition.X >= (float)worldRect.X && worldPosition.X <= (float)worldRect.Right && worldPosition.Y <= (float)worldRect.Y && worldPosition.Y >= (float)(worldRect.Y - worldRect.Height))
				{
					return hull;
				}
			}
			return null;
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x00117AC0 File Offset: 0x00115CC0
		public BackgroundSection GetBackgroundSection(Vector2 worldPosition)
		{
			if (!this.SupportsPaintedColors)
			{
				return null;
			}
			Vector2 subOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.Position;
			Vector2 relativePosition = new Vector2(worldPosition.X - subOffset.X - (float)this.rect.X, worldPosition.Y - subOffset.Y - (float)this.rect.Y);
			int xIndex = (int)Math.Floor((double)(relativePosition.X / 16f));
			if (xIndex < 0 || xIndex >= this.xBackgroundMax)
			{
				return null;
			}
			int yIndex = (int)Math.Floor((double)(-(double)relativePosition.Y / 16f));
			if (yIndex < 0 || yIndex >= this.yBackgroundMax)
			{
				return null;
			}
			return this.BackgroundSections[xIndex + yIndex * this.xBackgroundMax];
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x00117B8C File Offset: 0x00115D8C
		public Vector2 GetBackgroundSectionWorldPos(BackgroundSection backgroundSection)
		{
			Vector2 subOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.Position;
			return this.Rect.Location.ToVector2() + subOffset + new Vector2((float)backgroundSection.Rect.X, (float)backgroundSection.Rect.Y);
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x00117BF2 File Offset: 0x00115DF2
		public IEnumerable<BackgroundSection> GetBackgroundSectionsViaContaining(Rectangle rectArea)
		{
			Hull.<GetBackgroundSectionsViaContaining>d__229 <GetBackgroundSectionsViaContaining>d__ = new Hull.<GetBackgroundSectionsViaContaining>d__229(-2);
			<GetBackgroundSectionsViaContaining>d__.<>4__this = this;
			<GetBackgroundSectionsViaContaining>d__.<>3__rectArea = rectArea;
			return <GetBackgroundSectionsViaContaining>d__;
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x00117C09 File Offset: 0x00115E09
		public bool DoesSectionMatch(int index, int row)
		{
			return index >= 0 && row >= 0 && this.BackgroundSections.Count > index && this.BackgroundSections[index] != null && (int)this.BackgroundSections[index].RowIndex == row;
		}

		// Token: 0x06001C0C RID: 7180 RVA: 0x00117C48 File Offset: 0x00115E48
		public void IncreaseSectionColorOrStrength(BackgroundSection section, Color? color, float? strength, bool requiresUpdate, bool isCleaning)
		{
			bool sectionUpdated = isCleaning;
			if (color != null)
			{
				if (section.Color != color.Value && strength != null)
				{
					float changeSpeed = strength.Value / Math.Max(section.ColorStrength * section.ColorStrength, 0.001f) * 0.1f;
					if (section.LerpColor(color.Value, changeSpeed))
					{
						sectionUpdated = true;
					}
				}
				else if (section.SetColor(color.Value))
				{
					sectionUpdated = true;
				}
			}
			if (strength != null)
			{
				float previous = section.SetColorStrength(Math.Max(0f, Math.Min(0.7f, section.ColorStrength + strength.Value)));
				if (previous != -1f)
				{
					this.paintAmount = Math.Max(0f, this.paintAmount + (section.ColorStrength - previous) / (float)this.BackgroundSections.Count);
					sectionUpdated = true;
				}
				this.RefreshAveragePaintedColor();
			}
			if (sectionUpdated && GameMain.NetworkMember != null && requiresUpdate)
			{
				this.networkUpdatePending = true;
				this.pendingSectorUpdates.Add((int)Math.Floor((double)((float)section.Index / 16f)));
				this.serverUpdateDelay = 0.5f;
			}
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x00117D7C File Offset: 0x00115F7C
		private void RefreshAveragePaintedColor()
		{
			Vector4 avgColor = Vector4.Zero;
			foreach (BackgroundSection anySection in this.BackgroundSections)
			{
				avgColor += anySection.Color.ToVector4();
			}
			avgColor /= (float)this.BackgroundSections.Count;
			this.AveragePaintedColor = new Color(avgColor);
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x00117E04 File Offset: 0x00116004
		public void SetSectionColorOrStrength(BackgroundSection section, Color? color, float? strength)
		{
			if (color != null)
			{
				section.SetColor(color.Value);
			}
			if (strength != null)
			{
				float previous = section.SetColorStrength(Math.Max(0f, Math.Min(0.7f, section.ColorStrength + strength.Value)));
				if (previous != -1f)
				{
					this.paintAmount = Math.Max(0f, this.paintAmount + (section.ColorStrength - previous) / (float)this.BackgroundSections.Count);
				}
			}
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x00117E90 File Offset: 0x00116090
		public void CleanSection(BackgroundSection section, float cleanVal, bool updateRequired)
		{
			bool decalsCleaned = false;
			foreach (Decal decal in this.decals)
			{
				if (decal.BaseAlpha > 0.001f && decal.AffectsSection(section))
				{
					decal.Clean(cleanVal);
					decalsCleaned = true;
					this.pendingDecalUpdates.Add(decal);
					this.networkUpdatePending = true;
				}
			}
			if (section.ColorStrength == 0f && !decalsCleaned)
			{
				return;
			}
			this.IncreaseSectionColorOrStrength(section, null, new float?(cleanVal), updateRequired, true);
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x00117F3C File Offset: 0x0011613C
		public static Hull Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			Rectangle rect;
			if (element.GetAttribute("rect") != null)
			{
				string key = "rect";
				Rectangle rectangle = Rectangle.Empty;
				rect = element.GetAttributeRect(key, rectangle);
			}
			else
			{
				rect = new Rectangle(int.Parse(element.GetAttribute("x").Value), int.Parse(element.GetAttribute("y").Value), int.Parse(element.GetAttribute("width").Value), int.Parse(element.GetAttribute("height").Value));
			}
			Hull hull = new Hull(rect, submarine, idRemap.GetOffsetId(element))
			{
				WaterVolume = element.GetAttributeFloat("water", 0f)
			};
			hull.linkedToID = new List<ushort>();
			hull.ParseLinks(element, idRemap);
			string originalAmbientLight = element.GetAttributeString("originalambientlight", null);
			if (!string.IsNullOrWhiteSpace(originalAmbientLight))
			{
				hull.OriginalAmbientLight = new Color?(XMLExtensions.ParseColor(originalAmbientLight, false));
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "decal"))
				{
					if (a == "ballastflorabehavior")
					{
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						BallastFloraPrefab prefab = BallastFloraPrefab.Find(identifier);
						if (prefab != null)
						{
							hull.BallastFlora = new BallastFloraBehavior(hull, prefab, Vector2.Zero, false);
							hull.BallastFlora.LoadSave(subElement, idRemap);
						}
					}
				}
				else
				{
					string id = subElement.GetAttributeString("id", "");
					ContentXElement contentXElement = subElement;
					string key2 = "pos";
					Vector2 zero = Vector2.Zero;
					Vector2 pos = contentXElement.GetAttributeVector2(key2, zero);
					float scale = subElement.GetAttributeFloat("scale", 1f);
					float timer = subElement.GetAttributeFloat("timer", 1f);
					float baseAlpha = subElement.GetAttributeFloat("alpha", 1f);
					Hull hull2 = hull;
					string decalName = id;
					Vector2 value = pos;
					Rectangle rectangle = hull.WorldRect;
					Decal decal = hull2.AddDecal(decalName, value + rectangle.Location.ToVector2(), scale, true, null);
					if (decal != null)
					{
						decal.FadeTimer = timer;
						decal.BaseAlpha = baseAlpha;
					}
				}
			}
			string backgroundSectionStr = element.GetAttributeString("backgroundsections", "");
			if (!string.IsNullOrEmpty(backgroundSectionStr))
			{
				string[] backgroundSectionStrSplit = backgroundSectionStr.Split(';', StringSplitOptions.None);
				foreach (string str in backgroundSectionStrSplit)
				{
					string[] backgroundSectionData = str.Split(':', StringSplitOptions.None);
					if (backgroundSectionData.Length == 3)
					{
						Color color = XMLExtensions.ParseColor(backgroundSectionData[1], true);
						int index;
						float strength;
						if (int.TryParse(backgroundSectionData[0], out index) && float.TryParse(backgroundSectionData[2], NumberStyles.Any, CultureInfo.InvariantCulture, out strength))
						{
							hull.SetSectionColorOrStrength(hull.BackgroundSections[index], new Color?(color), new float?(strength));
						}
					}
				}
			}
			hull.RefreshAveragePaintedColor();
			SerializableProperty.DeserializeProperties(hull, element);
			if (element.GetAttribute("oxygen") == null)
			{
				hull.Oxygen = hull.Volume;
			}
			return hull;
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x00118288 File Offset: 0x00116488
		public override XElement Save(XElement parentElement)
		{
			if (base.Submarine == null)
			{
				string errorMsg = "Error - tried to save a hull that's not a part of any submarine.\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Hull.Save:WorldHull", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return null;
			}
			XElement element = new XElement("Hull");
			element.Add(new object[]
			{
				new XAttribute("ID", this.ID),
				new XAttribute("rect", string.Concat(new string[]
				{
					((int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)).ToString(),
					",",
					((int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)).ToString(),
					",",
					this.rect.Width.ToString(),
					",",
					this.rect.Height.ToString()
				})),
				new XAttribute("water", this.waterVolume)
			});
			if (this.linkedTo != null && this.linkedTo.Count > 0)
			{
				List<MapEntity> saveableLinked = (from l in this.linkedTo
				where l.ShouldBeSaved && l.Removed == base.Removed
				select l).ToList<MapEntity>();
				element.Add(new XAttribute("linked", string.Join(",", from l in saveableLinked
				select l.ID.ToString())));
			}
			if (this.OriginalAmbientLight != null)
			{
				element.Add(new XAttribute("originalambientlight", XMLExtensions.ColorToString(this.OriginalAmbientLight.Value)));
			}
			if (this.BackgroundSections != null && this.BackgroundSections.Count > 0)
			{
				element.Add(new XAttribute("backgroundsections", string.Join<string>(';', from b in this.BackgroundSections
				where b.ColorStrength > 0.01f
				select string.Concat(new string[]
				{
					b.Index.ToString(),
					":",
					XMLExtensions.ColorToString(b.Color),
					":",
					b.ColorStrength.ToString("G", CultureInfo.InvariantCulture)
				}))));
			}
			foreach (Decal decal in this.decals)
			{
				element.Add(new XElement("decal", new object[]
				{
					new XAttribute("id", decal.Prefab.Identifier),
					new XAttribute("pos", XMLExtensions.Vector2ToString(decal.NonClampedPosition)),
					new XAttribute("scale", decal.Scale.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("timer", decal.FadeTimer.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("alpha", decal.BaseAlpha.ToString("G", CultureInfo.InvariantCulture))
				}));
			}
			BallastFloraBehavior ballastFlora = this.BallastFlora;
			if (ballastFlora != null)
			{
				ballastFlora.Save(element);
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			parentElement.Add(element);
			return element;
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0011863C File Offset: 0x0011683C
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 3);
			defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.DisplayName ?? "unnamed");
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			Submarine submarine = base.Submarine;
			string text;
			if (submarine == null)
			{
				text = null;
			}
			else
			{
				SubmarineInfo info = submarine.Info;
				text = ((info != null) ? info.Name : null);
			}
			defaultInterpolatedStringHandler.AppendFormatted(text ?? "no sub");
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x00118759 File Offset: 0x00116959
		[CompilerGenerated]
		internal static void <UpdateCheats>g__SetWaterVolume|25_0(float newVolume, ref Hull.<>c__DisplayClass25_0 A_1)
		{
			Hull.ShowHulls = true;
			A_1.hull.WaterVolume = newVolume;
			A_1.hull.networkUpdatePending = true;
			A_1.hull.serverUpdateDelay = 0.5f;
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x0011878C File Offset: 0x0011698C
		[CompilerGenerated]
		internal static void <Draw>g__drawProgressBar|27_0(int height, Point offset, float fillAmount, Color color, ref Hull.<>c__DisplayClass27_0 A_4)
		{
			GUI.DrawRectangle(A_4.spriteBatch, new Rectangle(A_4.drawRect.Center.X - 2 + offset.X, -A_4.drawRect.Y - 2 + A_4.drawRect.Height / 2 + offset.Y, 14, height + 4), Color.Black * 0.8f, true, 0.01f, 1f);
			int barHeight = (int)(fillAmount * (float)height);
			GUI.DrawRectangle(A_4.spriteBatch, new Rectangle(A_4.drawRect.Center.X + offset.X, -A_4.drawRect.Y + A_4.drawRect.Height / 2 + height - barHeight + offset.Y, 10, barHeight), color, true, 0f, 1f);
		}

		// Token: 0x04000E20 RID: 3616
		private float serverUpdateDelay;

		// Token: 0x04000E21 RID: 3617
		private float remoteWaterVolume;

		// Token: 0x04000E22 RID: 3618
		private float remoteOxygenPercentage;

		// Token: 0x04000E23 RID: 3619
		private Hull.NetworkFireSource[] remoteFireSources;

		// Token: 0x04000E24 RID: 3620
		private readonly List<BackgroundSection> remoteBackgroundSections = new List<BackgroundSection>();

		// Token: 0x04000E25 RID: 3621
		private readonly List<Hull.RemoteDecal> remoteDecals = new List<Hull.RemoteDecal>();

		// Token: 0x04000E26 RID: 3622
		private readonly HashSet<Decal> pendingDecalUpdates = new HashSet<Decal>();

		// Token: 0x04000E27 RID: 3623
		private double lastAmbientLightEditTime;

		// Token: 0x04000E28 RID: 3624
		private float drawSurface;

		// Token: 0x04000E29 RID: 3625
		private float paintAmount;

		// Token: 0x04000E2A RID: 3626
		private float minimumPaintAmountToDraw;

		// Token: 0x04000E2B RID: 3627
		private static readonly Vector3[] corners = new Vector3[6];

		// Token: 0x04000E2C RID: 3628
		private static readonly Vector2[] uvCoords = new Vector2[4];

		// Token: 0x04000E2D RID: 3629
		private static readonly Vector3[] prevCorners = new Vector3[2];

		// Token: 0x04000E2E RID: 3630
		private static readonly Vector2[] prevUVs = new Vector2[2];

		// Token: 0x04000E2F RID: 3631
		public static readonly List<Hull> HullList = new List<Hull>();

		// Token: 0x04000E30 RID: 3632
		public static readonly List<EntityGrid> EntityGrids = new List<EntityGrid>();

		// Token: 0x04000E31 RID: 3633
		public static bool ShowHulls = true;

		// Token: 0x04000E32 RID: 3634
		public static bool EditWater;

		// Token: 0x04000E33 RID: 3635
		public static bool EditFire;

		// Token: 0x04000E34 RID: 3636
		public const float OxygenDistributionSpeed = 30000f;

		// Token: 0x04000E35 RID: 3637
		public const float OxygenDeteriorationSpeed = 0.3f;

		// Token: 0x04000E36 RID: 3638
		public const float OxygenConsumptionSpeed = 700f;

		// Token: 0x04000E37 RID: 3639
		private const float DecalAlphaRemoveThreshold = 0.001f;

		// Token: 0x04000E38 RID: 3640
		public const int WaveWidth = 32;

		// Token: 0x04000E39 RID: 3641
		public static float WaveStiffness = 0.01f;

		// Token: 0x04000E3A RID: 3642
		public static float WaveSpread = 0.02f;

		// Token: 0x04000E3B RID: 3643
		public static float WaveDampening = 0.02f;

		// Token: 0x04000E3C RID: 3644
		public const float MaxCompress = 1.05f;

		// Token: 0x04000E3D RID: 3645
		public const int BackgroundSectionSize = 16;

		// Token: 0x04000E3E RID: 3646
		public const int BackgroundSectionsPerNetworkEvent = 16;

		// Token: 0x04000E3F RID: 3647
		public readonly Dictionary<Identifier, SerializableProperty> properties;

		// Token: 0x04000E40 RID: 3648
		public const float PressureBuildUpSpeed = 15f;

		// Token: 0x04000E41 RID: 3649
		public const float PressureDropSpeed = 10f;

		// Token: 0x04000E42 RID: 3650
		private float lethalPressure;

		// Token: 0x04000E43 RID: 3651
		private float surface;

		// Token: 0x04000E44 RID: 3652
		private float waterVolume;

		// Token: 0x04000E45 RID: 3653
		private float pressure;

		// Token: 0x04000E46 RID: 3654
		private float oxygen;

		// Token: 0x04000E47 RID: 3655
		private bool update;

		// Token: 0x04000E48 RID: 3656
		public bool Visible = true;

		// Token: 0x04000E49 RID: 3657
		private float[] waveY;

		// Token: 0x04000E4A RID: 3658
		private float[] waveVel;

		// Token: 0x04000E4B RID: 3659
		private float[] leftDelta;

		// Token: 0x04000E4C RID: 3660
		private float[] rightDelta;

		// Token: 0x04000E4D RID: 3661
		public const int MaxDecalsPerHull = 10;

		// Token: 0x04000E4E RID: 3662
		private readonly List<Decal> decals = new List<Decal>();

		// Token: 0x04000E4F RID: 3663
		public readonly List<Gap> ConnectedGaps = new List<Gap>();

		// Token: 0x04000E51 RID: 3665
		private readonly HashSet<Identifier> moduleTags = new HashSet<Identifier>();

		// Token: 0x04000E52 RID: 3666
		private string roomName;

		// Token: 0x04000E53 RID: 3667
		public Color? OriginalAmbientLight;

		// Token: 0x04000E54 RID: 3668
		private Color ambientLight;

		// Token: 0x04000E57 RID: 3671
		private bool isWetRoom;

		// Token: 0x04000E58 RID: 3672
		private bool avoidStaying;

		// Token: 0x04000E5A RID: 3674
		private readonly HashSet<int> pendingSectorUpdates = new HashSet<int>();

		// Token: 0x04000E5B RID: 3675
		public int xBackgroundMax;

		// Token: 0x04000E5C RID: 3676
		public int yBackgroundMax;

		// Token: 0x04000E5D RID: 3677
		private const int SectionWidth = 4;

		// Token: 0x04000E5E RID: 3678
		private const int SectionHeight = 4;

		// Token: 0x04000E5F RID: 3679
		private const float minColorStrength = 0f;

		// Token: 0x04000E60 RID: 3680
		private const float maxColorStrength = 0.7f;

		// Token: 0x04000E61 RID: 3681
		private bool networkUpdatePending;

		// Token: 0x04000E62 RID: 3682
		private float networkUpdateTimer;

		// Token: 0x04000E64 RID: 3684
		public const int MaxFireSources = 16;

		// Token: 0x04000E68 RID: 3688
		private readonly HashSet<Hull> adjacentHulls = new HashSet<Hull>();

		// Token: 0x04000E69 RID: 3689
		private static readonly Dictionary<Hull, float> cachedDistances = new Dictionary<Hull, float>();

		// Token: 0x04000E6A RID: 3690
		[TupleElementNames(new string[]
		{
			"hull",
			"pos"
		})]
		private static readonly PriorityQueue<ValueTuple<Hull, Vector2>, float> priorityQueue = new PriorityQueue<ValueTuple<Hull, Vector2>, float>();

		// Token: 0x02000ABE RID: 2750
		private class RemoteDecal
		{
			// Token: 0x06007647 RID: 30279 RVA: 0x003784A2 File Offset: 0x003766A2
			public RemoteDecal(uint decalId, int spriteIndex, Vector2 normalizedPos, float scale, float decalAlpha)
			{
				this.DecalId = decalId;
				this.SpriteIndex = spriteIndex;
				this.NormalizedPos = normalizedPos;
				this.Scale = scale;
				this.DecalAlpha = decalAlpha;
			}

			// Token: 0x04004569 RID: 17769
			public readonly uint DecalId;

			// Token: 0x0400456A RID: 17770
			public readonly int SpriteIndex;

			// Token: 0x0400456B RID: 17771
			public readonly Vector2 NormalizedPos;

			// Token: 0x0400456C RID: 17772
			public readonly float Scale;

			// Token: 0x0400456D RID: 17773
			public readonly float DecalAlpha;
		}

		// Token: 0x02000ABF RID: 2751
		public readonly struct NetworkFireSource
		{
			// Token: 0x06007648 RID: 30280 RVA: 0x003784D0 File Offset: 0x003766D0
			public NetworkFireSource(Hull hull, Vector2 normalizedPosition, float normalizedSize)
			{
				this.Position = hull.Rect.Location.ToVector2() + new Vector2(0f, (float)(-(float)hull.Rect.Height)) + normalizedPosition * hull.Rect.Size.ToVector2();
				this.Size = normalizedSize * (float)hull.Rect.Width;
			}

			// Token: 0x0400456E RID: 17774
			public readonly Vector2 Position;

			// Token: 0x0400456F RID: 17775
			public readonly float Size;
		}

		// Token: 0x02000AC0 RID: 2752
		private readonly struct BackgroundSectionNetworkUpdate
		{
			// Token: 0x06007649 RID: 30281 RVA: 0x0037854A File Offset: 0x0037674A
			public BackgroundSectionNetworkUpdate(int sectionIndex, Color color, float colorStrength)
			{
				this.SectionIndex = sectionIndex;
				this.Color = color;
				this.ColorStrength = colorStrength;
			}

			// Token: 0x04004570 RID: 17776
			public readonly int SectionIndex;

			// Token: 0x04004571 RID: 17777
			public readonly Color Color;

			// Token: 0x04004572 RID: 17778
			public readonly float ColorStrength;
		}

		// Token: 0x02000AC1 RID: 2753
		[Flags]
		public enum EventType
		{
			// Token: 0x04004574 RID: 17780
			Status = 0,
			// Token: 0x04004575 RID: 17781
			Decal = 1,
			// Token: 0x04004576 RID: 17782
			BackgroundSections = 2,
			// Token: 0x04004577 RID: 17783
			BallastFlora = 3,
			// Token: 0x04004578 RID: 17784
			MinValue = 0,
			// Token: 0x04004579 RID: 17785
			MaxValue = 3
		}

		// Token: 0x02000AC2 RID: 2754
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001A9E RID: 6814
			// (get) Token: 0x0600764A RID: 30282
			Hull.EventType EventType { get; }
		}

		// Token: 0x02000AC3 RID: 2755
		private readonly struct StatusEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A9F RID: 6815
			// (get) Token: 0x0600764B RID: 30283 RVA: 0x00378561 File Offset: 0x00376761
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.Status;
				}
			}
		}

		// Token: 0x02000AC4 RID: 2756
		private readonly struct DecalEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001AA0 RID: 6816
			// (get) Token: 0x0600764C RID: 30284 RVA: 0x00378564 File Offset: 0x00376764
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.Decal;
				}
			}

			// Token: 0x0600764D RID: 30285 RVA: 0x00378567 File Offset: 0x00376767
			public DecalEventData(Decal decal)
			{
				this.Decal = decal;
			}

			// Token: 0x0400457A RID: 17786
			public readonly Decal Decal;
		}

		// Token: 0x02000AC5 RID: 2757
		private readonly struct BackgroundSectionsEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001AA1 RID: 6817
			// (get) Token: 0x0600764E RID: 30286 RVA: 0x00378570 File Offset: 0x00376770
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.BackgroundSections;
				}
			}

			// Token: 0x0600764F RID: 30287 RVA: 0x00378573 File Offset: 0x00376773
			public BackgroundSectionsEventData(int sectorStartIndex)
			{
				this.SectorStartIndex = sectorStartIndex;
			}

			// Token: 0x0400457B RID: 17787
			public readonly int SectorStartIndex;
		}

		// Token: 0x02000AC6 RID: 2758
		public readonly struct BallastFloraEventData : Hull.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001AA2 RID: 6818
			// (get) Token: 0x06007650 RID: 30288 RVA: 0x0037857C File Offset: 0x0037677C
			public Hull.EventType EventType
			{
				get
				{
					return Hull.EventType.BallastFlora;
				}
			}

			// Token: 0x06007651 RID: 30289 RVA: 0x0037857F File Offset: 0x0037677F
			public BallastFloraEventData(BallastFloraBehavior behavior, BallastFloraBehavior.IEventData subEventData)
			{
				this.Behavior = behavior;
				this.SubEventData = subEventData;
			}

			// Token: 0x0400457C RID: 17788
			public readonly BallastFloraBehavior Behavior;

			// Token: 0x0400457D RID: 17789
			public readonly BallastFloraBehavior.IEventData SubEventData;
		}
	}
}
