using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000F0 RID: 240
	internal class WayPoint : MapEntity
	{
		// Token: 0x06002298 RID: 8856 RVA: 0x0015D649 File Offset: 0x0015B849
		public override bool IsVisible(Rectangle worldView)
		{
			return (Screen.Selected == GameMain.SubEditorScreen || GameMain.DebugDraw) && base.IsVisible(worldView);
		}

		// Token: 0x1700096B RID: 2411
		// (get) Token: 0x06002299 RID: 8857 RVA: 0x0015D667 File Offset: 0x0015B867
		public override bool SelectableInEditor
		{
			get
			{
				return this.ShouldDrawIcon();
			}
		}

		// Token: 0x0600229A RID: 8858 RVA: 0x0015D670 File Offset: 0x0015B870
		public override void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
			if (!editing && (!GameMain.DebugDraw || Screen.Selected.Cam.Zoom < 0.1f))
			{
				return;
			}
			if (!this.ShouldDrawIcon())
			{
				return;
			}
			Vector2 drawPos = this.Position;
			if (base.Submarine != null)
			{
				drawPos += base.Submarine.DrawPosition;
			}
			drawPos.Y = -drawPos.Y;
			this.Draw(spriteBatch, drawPos);
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x0015D6E0 File Offset: 0x0015B8E0
		public void Draw(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			Color clr = (this.CurrentHull == null) ? Color.DodgerBlue : GUIStyle.Green;
			if (this.spawnType != SpawnType.Path)
			{
				clr = Color.Gray;
			}
			if (!this.IsTraversable)
			{
				clr = Color.Black;
			}
			if (base.IsHighlighted || base.IsHighlighted)
			{
				clr = Color.Lerp(clr, Color.White, 0.8f);
			}
			Structure stairs = this.Stairs;
			if (stairs != null && stairs.Removed)
			{
				this.Stairs = null;
			}
			Ladder ladders = this.Ladders;
			if (ladders != null)
			{
				Item item = ladders.Item;
				if (item != null && item.Removed)
				{
					this.Ladders = null;
				}
			}
			Gap connectedGap = this.ConnectedGap;
			if (connectedGap != null && connectedGap.Removed)
			{
				this.ConnectedGap = null;
			}
			int iconSize = (this.spawnType == SpawnType.Path) ? 12 : 32;
			if (this.ConnectedDoor != null || this.Ladders != null || this.Stairs != null || this.SpawnType != SpawnType.Path)
			{
				iconSize = (int)((float)iconSize * 1.5f);
			}
			if (base.IsSelected || base.IsHighlighted)
			{
				int glowSize = (int)((float)iconSize * 1.5f);
				GUIStyle.UIGlowCircular.Draw(spriteBatch, new Rectangle((int)(drawPos.X - (float)(glowSize / 2)), (int)(drawPos.Y - (float)(glowSize / 2)), glowSize, glowSize), Color.White, SpriteEffects.None);
			}
			Sprite sprite2 = null;
			Sprite sprite3;
			WayPoint.iconSprites.TryGetValue(this.SpawnType.ToString(), out sprite3);
			if (this.spawnType == SpawnType.Human)
			{
				JobPrefab assignedJob = this.AssignedJob;
				if (((assignedJob != null) ? assignedJob.Icon : null) != null)
				{
					sprite3 = WayPoint.iconSprites["Path"];
					goto IL_219;
				}
			}
			if (this.ConnectedDoor != null)
			{
				sprite3 = WayPoint.iconSprites["Door"];
				if (this.Ladders != null)
				{
					sprite2 = WayPoint.iconSprites["Ladder"];
				}
				else if (this.ConnectedDoor.IsHorizontal)
				{
					clr = Color.Yellow;
				}
				if (!Submarine.RectContains(this.ConnectedDoor.Item.WorldRect, this.WorldPosition, false))
				{
					clr = Color.Red;
				}
			}
			else if (this.Ladders != null)
			{
				sprite3 = WayPoint.iconSprites["Ladder"];
			}
			IL_219:
			if (sprite3 != null)
			{
				float spriteScale = (float)iconSize / (float)sprite3.SourceRect.Width;
				if (this.Ladders == null && this.ConnectedDoor == null && this.ConnectedGap != null)
				{
					clr = Color.White;
					spriteScale *= 1.5f;
				}
				sprite3.Draw(spriteBatch, drawPos, clr, sprite3.size / 2f, 0f, spriteScale, SpriteEffects.None, new float?(0.001f));
				if (sprite2 != null)
				{
					sprite2.Draw(spriteBatch, drawPos + sprite3.size * spriteScale * 0.5f, clr, sprite2.size / 2f, 0f, spriteScale, SpriteEffects.None, new float?(0.001f));
				}
			}
			if (this.spawnType == SpawnType.Human)
			{
				JobPrefab assignedJob2 = this.AssignedJob;
				if (((assignedJob2 != null) ? assignedJob2.Icon : null) != null)
				{
					this.AssignedJob.Icon.Draw(spriteBatch, drawPos, this.AssignedJob.UIColor, 0f, (float)iconSize / (float)this.AssignedJob.Icon.SourceRect.Width * 0.8f, SpriteEffects.None, new float?(0f));
				}
			}
			if (MapEntity.StartMovingPos != Vector2.Zero && MapEntity.SelectedList.Contains(this) && PlayerInput.IsCtrlDown())
			{
				GUI.DrawLine(spriteBatch, drawPos, new Vector2(MapEntity.StartMovingPos.X, -MapEntity.StartMovingPos.Y), (this.IsTraversable ? GUIStyle.Green : Color.Gray) * 0.7f, 0.002f, 5f);
			}
			else
			{
				foreach (MapEntity e in this.linkedTo)
				{
					GUI.DrawLine(spriteBatch, drawPos, new Vector2(e.DrawPosition.X, -e.DrawPosition.Y), (this.IsTraversable ? GUIStyle.Green : Color.Gray) * 0.7f, 0.002f, 5f);
				}
			}
			if (this.ConnectedGap != null)
			{
				GUI.DrawLine(spriteBatch, drawPos, new Vector2(this.ConnectedGap.DrawPosition.X, -this.ConnectedGap.DrawPosition.Y), Color.White, 0f, 1f);
			}
			if (this.Ladders != null)
			{
				GUI.DrawLine(spriteBatch, drawPos, new Vector2(this.Ladders.Item.DrawPosition.X, -this.Ladders.Item.DrawPosition.Y), Color.White, 0f, 1f);
			}
			Color color = Color.WhiteSmoke;
			if (this.spawnType == SpawnType.Path)
			{
				if (this.linkedTo.Count < 2)
				{
					if (this.linkedTo.Count == 0)
					{
						color = Color.Red;
					}
					else if (this.CurrentHull == null)
					{
						color = ((this.Ladders == null) ? Color.Red : Color.Yellow);
					}
					else
					{
						color = Color.Yellow;
					}
				}
			}
			else if (this.spawnType == SpawnType.ExitPoint && this.ExitPointSize != Point.Zero)
			{
				GUI.DrawRectangle(spriteBatch, drawPos - this.ExitPointSize.ToVector2() / 2f, this.ExitPointSize.ToVector2(), Color.Cyan, false, 0f, 5f);
			}
			Screen selected = Screen.Selected;
			Camera camera = (selected != null) ? selected.Cam : null;
			if (camera != null && camera.Zoom > 0.4f)
			{
				GUIStyle.SmallFont.DrawString(spriteBatch, this.ID.ToString(), new Vector2(this.DrawPosition.X - 10f, -this.DrawPosition.Y - 30f), color, ForceUpperCase.Inherit, false);
				Level.Tunnel tunnel = this.Tunnel;
				bool flag;
				if (tunnel == null)
				{
					flag = false;
				}
				else
				{
					Level.TunnelType type = tunnel.Type;
					flag = true;
				}
				if (flag)
				{
					GUIStyle.SmallFont.DrawString(spriteBatch, this.Tunnel.Type.ToString(), new Vector2(this.DrawPosition.X - 10f, -this.DrawPosition.Y - 45f), color, ForceUpperCase.Inherit, false);
				}
			}
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x0015DD58 File Offset: 0x0015BF58
		public override bool IsMouseOn(Vector2 position)
		{
			if (!this.ShouldDrawIcon())
			{
				return false;
			}
			float dist = Vector2.DistanceSquared(position, this.WorldPosition);
			float radius = (float)((this.SpawnType == SpawnType.Path) ? 12 : 32) * 0.6f;
			return dist < radius * radius;
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x0015DD98 File Offset: 0x0015BF98
		private bool ShouldDrawIcon()
		{
			if (!SubEditorScreen.IsLayerVisible(this))
			{
				return false;
			}
			if (this.spawnType == SpawnType.Path)
			{
				return GameMain.DebugDraw || WayPoint.ShowWayPoints;
			}
			return GameMain.DebugDraw || WayPoint.ShowSpawnPoints;
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x0015DDCC File Offset: 0x0015BFCC
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD();
			}
			if (base.IsSelected && PlayerInput.PrimaryMouseButtonClicked() && GUI.MouseOn == null)
			{
				Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
				if (PlayerInput.KeyDown(Keys.Space))
				{
					using (IEnumerator<MapEntity> enumerator = MapEntity.HighlightedEntities.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							MapEntity e = enumerator.Current;
							if (e is WayPoint && e != this)
							{
								if (this.linkedTo.Contains(e))
								{
									this.linkedTo.Remove(e);
									e.linkedTo.Remove(this);
								}
								else
								{
									this.linkedTo.Add(e);
									e.linkedTo.Add(this);
								}
							}
						}
						return;
					}
				}
				this.FindHull();
				this.UpdateLinkedEntity<Gap>(position, Gap.GapList, delegate(Gap gap)
				{
					this.ConnectedGap = gap;
				}, delegate(Gap gap)
				{
					if (this.ConnectedGap == gap)
					{
						this.ConnectedGap = null;
					}
				}, 0);
				this.UpdateLinkedEntity<Item>(position, Item.ItemList, delegate(Item i)
				{
					Ladder ladder = (i != null) ? i.GetComponent<Ladder>() : null;
					if (ladder != null)
					{
						this.Ladders = ladder;
					}
				}, delegate(Item i)
				{
					Ladder ladder = (i != null) ? i.GetComponent<Ladder>() : null;
					if (ladder != null && this.Ladders == ladder)
					{
						this.Ladders = null;
					}
				}, 5);
				this.FindStairs();
			}
		}

		// Token: 0x0600229F RID: 8863 RVA: 0x0015DF0C File Offset: 0x0015C10C
		private void UpdateLinkedEntity<T>(Vector2 worldPos, IEnumerable<T> list, Action<T> match, Action<T> noMatch, int inflate = 0) where T : MapEntity
		{
			foreach (T entity in list)
			{
				Rectangle rect = entity.WorldRect;
				rect.Inflate(inflate, inflate);
				if (Submarine.RectContains(rect, worldPos, false))
				{
					match(entity);
				}
				else
				{
					noMatch(entity);
				}
			}
		}

		// Token: 0x060022A0 RID: 8864 RVA: 0x0015DF80 File Offset: 0x0015C180
		private bool ChangeSpawnType(GUIButton button, object obj)
		{
			SpawnType prevSpawnType = this.spawnType;
			GUITextBlock spawnTypeText = button.Parent.GetChildByUserData("spawntypetext") as GUITextBlock;
			SpawnType[] values = (SpawnType[])Enum.GetValues(typeof(SpawnType));
			int currIndex = values.IndexOf(this.spawnType);
			currIndex += (int)button.UserData;
			int firstIndex = 1;
			int lastIndex = values.Length - 1;
			if (currIndex > lastIndex)
			{
				currIndex = firstIndex;
			}
			if (currIndex < firstIndex)
			{
				currIndex = lastIndex;
			}
			this.spawnType = values[currIndex];
			spawnTypeText.Text = this.spawnType.ToString();
			if (this.spawnType == SpawnType.ExitPoint || prevSpawnType == SpawnType.ExitPoint)
			{
				this.CreateEditingHUD();
			}
			return true;
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x0015E030 File Offset: 0x0015C230
		private GUIComponent CreateEditingHUD()
		{
			MapEntity.editingHUD = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.15f), GUI.Canvas, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 0)
			}, "", null)
			{
				UserData = this
			};
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.85f), MapEntity.editingHUD.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = (int)(GUI.Scale * 5f)
			};
			if (this.spawnType == SpawnType.Path)
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text7 = TextManager.Get("Waypoint");
				GUIFont font = GUIStyle.LargeFont;
				new GUITextBlock(rectT, text7, null, font, Alignment.Left, false, "", null);
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LinkWaypoint"), null, null, Alignment.Left, false, "", null);
			}
			else
			{
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get("Spawnpoint");
				GUIFont font = GUIStyle.LargeFont;
				new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
				if (!base.Layer.IsNullOrEmpty())
				{
					GUITextBlock layerText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("editor.layer"),
						base.Layer
					}), null, null, Alignment.Left, false, "", null);
				}
				GUILayoutGroup spawnTypeContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), spawnTypeContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SpawnType"), null, null, Alignment.Left, false, "", null);
				GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, spawnTypeContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIMinusButton", null);
				guibutton.UserData = -1;
				guibutton.OnClicked = new GUIButton.OnClickedHandler(this.ChangeSpawnType);
				new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), spawnTypeContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), this.spawnType.ToString(), null, null, Alignment.Center, false, "", null).UserData = "spawntypetext";
				GUIButton guibutton2 = new GUIButton(new RectTransform(Vector2.One, spawnTypeContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIPlusButton", null);
				guibutton2.UserData = 1;
				guibutton2.OnClicked = new GUIButton.OnClickedHandler(this.ChangeSpawnType);
				RectTransform rectT3 = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = TextManager.Get("IDCardDescription");
				font = GUIStyle.SmallFont;
				GUITextBlock descText = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null)
				{
					ToolTip = TextManager.Get("IDCardDescriptionTooltip")
				};
				GUITextBox propertyBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), descText.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), this.IdCardDesc, null, null, Alignment.Left, false, "", null, false, true)
				{
					MaxTextLength = new int?(150),
					ToolTip = TextManager.Get("IDCardDescriptionTooltip")
				};
				propertyBox.OnTextChanged += delegate(GUITextBox textBox, string text)
				{
					this.IdCardDesc = text;
					return true;
				};
				GUITextBox guitextBox = propertyBox;
				guitextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
				{
					this.IdCardDesc = text;
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
					return true;
				}));
				propertyBox.OnDeselected += delegate(GUITextBox textBox, Keys keys)
				{
					this.IdCardDesc = textBox.Text;
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				};
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = TextManager.Get("IDCardTags");
				font = GUIStyle.SmallFont;
				GUITextBlock idCardTagsText = new GUITextBlock(rectT4, text4, null, font, Alignment.Left, false, "", null)
				{
					ToolTip = TextManager.Get("IDCardTagsTooltip")
				};
				propertyBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), idCardTagsText.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Join(", ", this.idCardTags), null, null, Alignment.Left, false, "", null, false, true)
				{
					MaxTextLength = new int?(60),
					ToolTip = TextManager.Get("IDCardTagsTooltip")
				};
				propertyBox.OnTextChanged += delegate(GUITextBox textBox, string text)
				{
					this.IdCardTags = text.Split(',', StringSplitOptions.None);
					return true;
				};
				GUITextBox guitextBox2 = propertyBox;
				guitextBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
				{
					textBox.Text = string.Join(",", this.IdCardTags);
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
					return true;
				}));
				propertyBox.OnDeselected += delegate(GUITextBox textBox, Keys keys)
				{
					textBox.Text = string.Join(",", this.IdCardTags);
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				};
				RectTransform rectT5 = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text5 = TextManager.Get("SpawnpointJobs");
				font = GUIStyle.SmallFont;
				GUITextBlock jobsText = new GUITextBlock(rectT5, text5, null, font, Alignment.Left, false, "", null)
				{
					ToolTip = TextManager.Get("SpawnpointJobsTooltip")
				};
				GUIDropDown jobDropDown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), jobsText.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f)
				{
					ToolTip = TextManager.Get("SpawnpointJobsTooltip"),
					OnSelected = delegate(GUIComponent selected, object userdata)
					{
						this.AssignedJob = (userdata as JobPrefab);
						return true;
					}
				};
				jobDropDown.AddItem(TextManager.Get("Any"), null, null, null, null);
				foreach (JobPrefab jobPrefab in JobPrefab.Prefabs)
				{
					if (!jobPrefab.Name.IsNullOrWhiteSpace())
					{
						jobDropDown.AddItem(jobPrefab.Name, jobPrefab, null, null, null);
					}
				}
				jobDropDown.SelectItem(this.AssignedJob);
				RectTransform rectT6 = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text6 = TextManager.Get("spawnpointtags");
				font = GUIStyle.SmallFont;
				GUITextBlock tagsText = new GUITextBlock(rectT6, text6, null, font, Alignment.Left, false, "", null);
				propertyBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), tagsText.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Join<Identifier>(", ", this.tags), null, null, Alignment.Left, false, "", null, false, true)
				{
					MaxTextLength = new int?(60),
					ToolTip = TextManager.Get("spawnpointtagstooltip")
				};
				propertyBox.OnTextChanged += delegate(GUITextBox textBox, string text)
				{
					this.tags = text.ToIdentifiers(",").ToHashSet<Identifier>();
					return true;
				};
				GUITextBox guitextBox3 = propertyBox;
				guitextBox3.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox3.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
				{
					textBox.Text = string.Join<Identifier>(",", this.tags);
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
					return true;
				}));
				propertyBox.OnDeselected += delegate(GUITextBox textBox, Keys keys)
				{
					textBox.Text = string.Join<Identifier>(",", this.tags);
					textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				};
				if (this.SpawnType == SpawnType.ExitPoint)
				{
					GUIComponent sizeField = GUI.CreatePointField(this.ExitPointSize, GUI.IntScale(20f), TextManager.Get("dimensions"), paddedFrame.RectTransform, null);
					GUINumberInput xField = null;
					GUINumberInput yField = null;
					foreach (GUIComponent child in sizeField.GetAllChildren())
					{
						if (yField == null)
						{
							yField = (child as GUINumberInput);
						}
						else
						{
							xField = (child as GUINumberInput);
							if (xField != null)
							{
								break;
							}
						}
					}
					xField.MinValueInt = new int?(0);
					xField.OnValueChanged = delegate(GUINumberInput numberInput)
					{
						this.ExitPointSize = new Point(numberInput.IntValue, this.ExitPointSize.Y);
					};
					yField.MinValueInt = new int?(0);
					yField.OnValueChanged = delegate(GUINumberInput numberInput)
					{
						this.ExitPointSize = new Point(this.ExitPointSize.X, numberInput.IntValue);
					};
				}
			}
			MapEntity.editingHUD.RectTransform.Resize(new Point(MapEntity.editingHUD.Rect.Width, (int)((float)paddedFrame.Children.Sum((GUIComponent c) => c.Rect.Height + paddedFrame.AbsoluteSpacing) / paddedFrame.RectTransform.RelativeSize.Y)), true);
			MapEntity.PositionEditingHUD();
			return MapEntity.editingHUD;
		}

		// Token: 0x1700096C RID: 2412
		// (get) Token: 0x060022A2 RID: 8866 RVA: 0x0015EBE8 File Offset: 0x0015CDE8
		public bool IsInWater
		{
			get
			{
				return this.CurrentHull == null || this.CurrentHull.Surface > this.Position.Y;
			}
		}

		// Token: 0x1700096D RID: 2413
		// (get) Token: 0x060022A3 RID: 8867 RVA: 0x0015EC0C File Offset: 0x0015CE0C
		public bool IsTraversable
		{
			get
			{
				return !this.IsObstructed && (this.openGaps == null || this.openGaps.Count == 0 || this.IsInWater);
			}
		}

		// Token: 0x060022A4 RID: 8868 RVA: 0x0015EC35 File Offset: 0x0015CE35
		public void OnGapStateChanged(bool open, Gap gap)
		{
			if (this.openGaps == null)
			{
				this.openGaps = new HashSet<Gap>();
			}
			if (open)
			{
				this.openGaps.Add(gap);
				return;
			}
			this.openGaps.Remove(gap);
		}

		// Token: 0x1700096E RID: 2414
		// (get) Token: 0x060022A5 RID: 8869 RVA: 0x0015EC68 File Offset: 0x0015CE68
		// (set) Token: 0x060022A6 RID: 8870 RVA: 0x0015EC70 File Offset: 0x0015CE70
		public Gap ConnectedGap { get; set; }

		// Token: 0x1700096F RID: 2415
		// (get) Token: 0x060022A7 RID: 8871 RVA: 0x0015EC79 File Offset: 0x0015CE79
		public Door ConnectedDoor
		{
			get
			{
				Gap connectedGap = this.ConnectedGap;
				if (connectedGap == null)
				{
					return null;
				}
				return connectedGap.ConnectedDoor;
			}
		}

		// Token: 0x17000970 RID: 2416
		// (get) Token: 0x060022A8 RID: 8872 RVA: 0x0015EC8C File Offset: 0x0015CE8C
		// (set) Token: 0x060022A9 RID: 8873 RVA: 0x0015EC94 File Offset: 0x0015CE94
		public Hull CurrentHull { get; private set; }

		// Token: 0x17000971 RID: 2417
		// (get) Token: 0x060022AA RID: 8874 RVA: 0x0015EC9D File Offset: 0x0015CE9D
		// (set) Token: 0x060022AB RID: 8875 RVA: 0x0015ECA5 File Offset: 0x0015CEA5
		public SpawnType SpawnType
		{
			get
			{
				return this.spawnType;
			}
			set
			{
				this.spawnType = value;
			}
		}

		// Token: 0x17000972 RID: 2418
		// (get) Token: 0x060022AC RID: 8876 RVA: 0x0015ECAE File Offset: 0x0015CEAE
		// (set) Token: 0x060022AD RID: 8877 RVA: 0x0015ECB6 File Offset: 0x0015CEB6
		public Point ExitPointSize { get; private set; }

		// Token: 0x17000973 RID: 2419
		// (get) Token: 0x060022AE RID: 8878 RVA: 0x0015ECC0 File Offset: 0x0015CEC0
		public Rectangle ExitPointWorldRect
		{
			get
			{
				return new Rectangle((int)this.WorldPosition.X - this.ExitPointSize.X / 2, (int)this.WorldPosition.Y + this.ExitPointSize.Y / 2, this.ExitPointSize.X, this.ExitPointSize.Y);
			}
		}

		// Token: 0x17000974 RID: 2420
		// (get) Token: 0x060022AF RID: 8879 RVA: 0x0015ED1C File Offset: 0x0015CF1C
		// (set) Token: 0x060022B0 RID: 8880 RVA: 0x0015ED24 File Offset: 0x0015CF24
		public Action<WayPoint> OnLinksChanged { get; set; }

		// Token: 0x17000975 RID: 2421
		// (get) Token: 0x060022B1 RID: 8881 RVA: 0x0015ED2D File Offset: 0x0015CF2D
		public override string Name
		{
			get
			{
				if (this.spawnType != SpawnType.Path)
				{
					return "SpawnPoint";
				}
				return "WayPoint";
			}
		}

		// Token: 0x17000976 RID: 2422
		// (get) Token: 0x060022B2 RID: 8882 RVA: 0x0015ED42 File Offset: 0x0015CF42
		// (set) Token: 0x060022B3 RID: 8883 RVA: 0x0015ED4A File Offset: 0x0015CF4A
		public string IdCardDesc { get; private set; }

		// Token: 0x17000977 RID: 2423
		// (get) Token: 0x060022B4 RID: 8884 RVA: 0x0015ED53 File Offset: 0x0015CF53
		// (set) Token: 0x060022B5 RID: 8885 RVA: 0x0015ED5C File Offset: 0x0015CF5C
		public string[] IdCardTags
		{
			get
			{
				return this.idCardTags;
			}
			private set
			{
				this.idCardTags = value;
				for (int i = 0; i < this.idCardTags.Length; i++)
				{
					this.idCardTags[i] = this.idCardTags[i].Trim().ToLowerInvariant();
				}
			}
		}

		// Token: 0x17000978 RID: 2424
		// (get) Token: 0x060022B6 RID: 8886 RVA: 0x0015ED9D File Offset: 0x0015CF9D
		public IEnumerable<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x17000979 RID: 2425
		// (get) Token: 0x060022B7 RID: 8887 RVA: 0x0015EDA5 File Offset: 0x0015CFA5
		// (set) Token: 0x060022B8 RID: 8888 RVA: 0x0015EDAD File Offset: 0x0015CFAD
		public JobPrefab AssignedJob { get; private set; }

		// Token: 0x060022B9 RID: 8889 RVA: 0x0015EDB6 File Offset: 0x0015CFB6
		public WayPoint(Vector2 position, SpawnType spawnType, Submarine submarine, Gap gap = null) : this(new Rectangle((int)position.X - 3, (int)position.Y + 3, 6, 6), submarine)
		{
			this.spawnType = spawnType;
			this.ConnectedGap = gap;
		}

		// Token: 0x060022BA RID: 8890 RVA: 0x0015EDE8 File Offset: 0x0015CFE8
		public WayPoint(MapEntityPrefab prefab, Rectangle rectangle) : this(rectangle, Submarine.MainSub)
		{
			if (prefab.Identifier.Contains("spawn"))
			{
				this.spawnType = SpawnType.Human;
			}
			else
			{
				this.SpawnType = SpawnType.Path;
			}
			if (SubEditorScreen.IsSubEditor())
			{
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
				{
					this
				}, false, true));
			}
		}

		// Token: 0x060022BB RID: 8891 RVA: 0x0015EE42 File Offset: 0x0015D042
		public WayPoint(Rectangle newRect, Submarine submarine) : this(WayPoint.Type.WayPoint, newRect, submarine, 0)
		{
		}

		// Token: 0x060022BC RID: 8892 RVA: 0x0015EE50 File Offset: 0x0015D050
		public WayPoint(WayPoint.Type type, Rectangle newRect, Submarine submarine, ushort id = 0) : base((type == WayPoint.Type.WayPoint) ? CoreEntityPrefab.WayPointPrefab : CoreEntityPrefab.SpawnPointPrefab, submarine, id)
		{
			this.rect = newRect;
			this.idCardTags = Array.Empty<string>();
			this.tags = new HashSet<Identifier>();
			if (WayPoint.iconSprites == null)
			{
				WayPoint.iconSprites = new Dictionary<string, Sprite>
				{
					{
						"Path",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(0, 0, 128, 128)), null, 0f)
					},
					{
						"Human",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(128, 0, 128, 128)), null, 0f)
					},
					{
						"Enemy",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(256, 0, 128, 128)), null, 0f)
					},
					{
						"Cargo",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(384, 0, 128, 128)), null, 0f)
					},
					{
						"Corpse",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(512, 0, 128, 128)), null, 0f)
					},
					{
						"Ladder",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(0, 128, 128, 128)), null, 0f)
					},
					{
						"Door",
						new Sprite("Content/UI/MainIconsAtlas.png", new Rectangle?(new Rectangle(128, 128, 128, 128)), null, 0f)
					},
					{
						"Submarine",
						new Sprite("Content/UI/CommandUIBackground.png", new Rectangle?(new Rectangle(0, 896, 128, 128)), null, 0f)
					},
					{
						"ExitPoint",
						new Sprite("Content/UI/CommandUIBackground.png", new Rectangle?(new Rectangle(0, 896, 128, 128)), null, 0f)
					}
				};
			}
			base.InsertToList();
			WayPoint.WayPointList.Add(this);
			DebugConsole.Log("Created waypoint (" + this.ID.ToString() + ")");
			this.FindHull();
		}

		// Token: 0x060022BD RID: 8893 RVA: 0x0015F104 File Offset: 0x0015D304
		public override MapEntity Clone()
		{
			return new WayPoint(this.rect, base.Submarine)
			{
				IdCardDesc = this.IdCardDesc,
				idCardTags = this.idCardTags,
				tags = this.tags,
				spawnType = this.spawnType,
				AssignedJob = this.AssignedJob
			};
		}

		// Token: 0x060022BE RID: 8894 RVA: 0x0015F160 File Offset: 0x0015D360
		public static bool GenerateSubWaypoints(Submarine submarine)
		{
			if (!Hull.HullList.Any<Hull>())
			{
				DebugConsole.ThrowError("Couldn't generate waypoints: no hulls found.", null, null, false, false);
				return false;
			}
			List<WayPoint> existingWaypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Path);
			foreach (WayPoint wayPoint in existingWaypoints)
			{
				wayPoint.Remove();
			}
			List<Door> openDoors = new List<Door>();
			foreach (Item item in Item.ItemList)
			{
				Door door = item.GetComponent<Door>();
				if (door != null && !door.Body.Enabled)
				{
					openDoors.Add(door);
					door.Body.Enabled = true;
				}
			}
			bool isRuin = submarine.Info.ShouldBeRuin;
			float diffFromHullEdge = 50f;
			float minDist = 100f;
			float heightFromFloor = 110f;
			float hullMinHeight = 100f;
			HashSet<WayPoint> removals = new HashSet<WayPoint>();
			foreach (Hull hull in Hull.HullList)
			{
				if (isRuin)
				{
					diffFromHullEdge = 75f;
					List<WayPoint> hullWaypoints = new List<WayPoint>();
					float top = (float)hull.Rect.Y;
					float bottom = (float)(hull.Rect.Y - hull.Rect.Height);
					if (hull.Rect.Width < 300 || hull.Rect.Height < 300)
					{
						if (hull.Rect.Width > hull.Rect.Height)
						{
							float y = (float)(hull.Rect.Y - hull.Rect.Height / 2);
							for (float x = (float)hull.Rect.X + diffFromHullEdge; x <= (float)hull.Rect.Right - diffFromHullEdge; x += minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x, y), SpawnType.Path, submarine, null));
							}
						}
						else
						{
							float x2 = (float)(hull.Rect.X + hull.Rect.Width / 2);
							for (float y2 = top - diffFromHullEdge; y2 >= bottom + diffFromHullEdge; y2 -= minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x2, y2), SpawnType.Path, submarine, null));
							}
						}
					}
					if (hullWaypoints.None(null))
					{
						for (float x3 = (float)hull.Rect.X + diffFromHullEdge; x3 <= (float)hull.Rect.Right - diffFromHullEdge; x3 += minDist)
						{
							for (float y3 = top - diffFromHullEdge; y3 >= bottom + diffFromHullEdge; y3 -= minDist)
							{
								hullWaypoints.Add(new WayPoint(new Vector2(x3, y3), SpawnType.Path, submarine, null));
							}
						}
						if (hullWaypoints.None(null))
						{
							hullWaypoints.Add(new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height / 2)), SpawnType.Path, submarine, null));
						}
						foreach (WayPoint wp7 in hullWaypoints)
						{
							foreach (Structure wall in Structure.WallList)
							{
								if (wall.HasBody)
								{
									Rectangle rect = wall.Rect;
									rect.Inflate(10, 10);
									if (rect.ContainsWorld(wp7.Position))
									{
										removals.Add(wp7);
									}
								}
							}
						}
					}
					using (List<WayPoint>.Enumerator enumerator6 = hullWaypoints.GetEnumerator())
					{
						while (enumerator6.MoveNext())
						{
							WayPoint wayPoint2 = enumerator6.Current;
							for (int dir = -1; dir <= 1; dir += 2)
							{
								WayPoint closest = wayPoint2.FindClosest(dir, true, new Vector2(minDist * 1.9f, minDist), null, null, null);
								if (closest != null && closest.CurrentHull == wayPoint2.CurrentHull)
								{
									wayPoint2.ConnectTo(closest);
								}
								closest = wayPoint2.FindClosest(dir, false, new Vector2(minDist, minDist * 1.9f), null, null, null);
								if (closest != null && closest.CurrentHull == wayPoint2.CurrentHull)
								{
									wayPoint2.ConnectTo(closest);
								}
							}
						}
						continue;
					}
				}
				if ((float)hull.Rect.Height >= hullMinHeight)
				{
					Body floor = null;
					for (int i = 0; i < 5; i++)
					{
						float horizontalOffset = 0f;
						switch (i)
						{
						case 1:
							horizontalOffset = (float)hull.RectWidth * 0.2f;
							break;
						case 2:
							horizontalOffset = (float)hull.RectWidth * 0.4f;
							break;
						case 3:
							horizontalOffset = (float)(-(float)hull.RectWidth) * 0.2f;
							break;
						case 4:
							horizontalOffset = (float)(-(float)hull.RectWidth) * 0.4f;
							break;
						}
						horizontalOffset = ConvertUnits.ToSimUnits(horizontalOffset);
						Vector2 floorPos = new Vector2(hull.SimPosition.X + horizontalOffset, ConvertUnits.ToSimUnits(hull.Rect.Y - hull.RectHeight - 50));
						floor = Submarine.PickBody(new Vector2(hull.SimPosition.X + horizontalOffset, hull.SimPosition.Y), floorPos, null, new Category?(Category.Cat1 | Category.Cat3), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
						if (floor != null)
						{
							break;
						}
					}
					if (floor != null)
					{
						float waypointHeight = ((float)hull.Rect.Height > heightFromFloor * 2f) ? heightFromFloor : ((float)(hull.Rect.Height / 2));
						if ((float)hull.Rect.Width < diffFromHullEdge * 3f)
						{
							new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
						}
						else
						{
							WayPoint previousWaypoint = null;
							for (float x4 = (float)hull.Rect.X + diffFromHullEdge; x4 <= (float)hull.Rect.Right - diffFromHullEdge; x4 += minDist)
							{
								WayPoint wayPoint3 = new WayPoint(new Vector2(x4, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
								if (wayPoint3.FindStairs() != null)
								{
									removals.Add(wayPoint3);
								}
								else
								{
									if (previousWaypoint != null)
									{
										wayPoint3.ConnectTo(previousWaypoint);
									}
									previousWaypoint = wayPoint3;
								}
							}
							if (previousWaypoint == null)
							{
								new WayPoint(new Vector2((float)hull.Rect.X + (float)hull.Rect.Width / 2f, (float)(hull.Rect.Y - hull.Rect.Height) + waypointHeight), SpawnType.Path, submarine, null);
							}
						}
					}
				}
			}
			foreach (Structure platform in Structure.WallList)
			{
				if (platform.IsPlatform)
				{
					float waypointHeight2 = heightFromFloor;
					WayPoint prevWaypoint = null;
					for (float x5 = (float)platform.Rect.X + diffFromHullEdge; x5 <= (float)platform.Rect.Right - diffFromHullEdge; x5 += minDist)
					{
						WayPoint wayPoint4 = new WayPoint(new Vector2(x5, (float)platform.Rect.Y + waypointHeight2), SpawnType.Path, submarine, null);
						if (prevWaypoint != null)
						{
							wayPoint4.ConnectTo(prevWaypoint);
						}
						if (wayPoint4 != null)
						{
							for (int dir2 = -1; dir2 <= 1; dir2 += 2)
							{
								if (wayPoint4.FindClosest(dir2, true, new Vector2(minDist, heightFromFloor), null, prevWaypoint.ToEnumerable<WayPoint>(), null) != null)
								{
									wayPoint4.Remove();
									wayPoint4 = null;
									break;
								}
							}
						}
						prevWaypoint = wayPoint4;
					}
				}
			}
			float outSideWaypointInterval = 100f;
			if (!isRuin && submarine.Info.Type != SubmarineType.OutpostModule)
			{
				List<ValueTuple<WayPoint, int>> outsideWaypoints = new List<ValueTuple<WayPoint, int>>();
				Rectangle borders = Hull.GetBorders();
				int originalWidth = borders.Width;
				int originalHeight = borders.Height;
				borders.X -= Math.Min(500, originalWidth / 4);
				borders.Y += Math.Min(500, originalHeight / 4);
				borders.Width += Math.Min(1500, originalWidth / 2);
				borders.Height += Math.Min(1000, originalHeight / 2);
				borders.Location -= MathUtils.ToPoint(submarine.HiddenSubPosition);
				if ((float)borders.Width <= outSideWaypointInterval * 2f)
				{
					borders.Inflate(outSideWaypointInterval * 2f - (float)borders.Width, 0f);
				}
				if ((float)borders.Height <= outSideWaypointInterval * 2f)
				{
					int inflateAmount = (int)(outSideWaypointInterval * 2f) - borders.Height;
					borders.Y += inflateAmount / 2;
					borders.Height += inflateAmount;
				}
				WayPoint[,] cornerWaypoint = new WayPoint[2, 2];
				for (int j = 0; j < 2; j++)
				{
					for (float x6 = (float)borders.X + outSideWaypointInterval; x6 < (float)borders.Right - outSideWaypointInterval; x6 += outSideWaypointInterval)
					{
						WayPoint wayPoint5 = new WayPoint(new Vector2(x6, (float)(borders.Y - borders.Height * j)) + submarine.HiddenSubPosition, SpawnType.Path, submarine, null);
						outsideWaypoints.Add(new ValueTuple<WayPoint, int>(wayPoint5, j));
						if (x6 == (float)borders.X + outSideWaypointInterval)
						{
							cornerWaypoint[j, 0] = wayPoint5;
						}
						else
						{
							wayPoint5.ConnectTo(WayPoint.WayPointList[WayPoint.WayPointList.Count - 2]);
						}
					}
					cornerWaypoint[j, 1] = WayPoint.WayPointList[WayPoint.WayPointList.Count - 1];
				}
				for (int k = 0; k < 2; k++)
				{
					WayPoint wayPoint6 = null;
					for (float y4 = (float)(borders.Y - borders.Height); y4 < (float)borders.Y; y4 += outSideWaypointInterval)
					{
						wayPoint6 = new WayPoint(new Vector2((float)(borders.X + borders.Width * k), y4) + submarine.HiddenSubPosition, SpawnType.Path, submarine, null);
						outsideWaypoints.Add(new ValueTuple<WayPoint, int>(wayPoint6, k));
						if (y4 == (float)(borders.Y - borders.Height))
						{
							wayPoint6.ConnectTo(cornerWaypoint[1, k]);
						}
						else
						{
							wayPoint6.ConnectTo(WayPoint.WayPointList[WayPoint.WayPointList.Count - 2]);
						}
					}
					wayPoint6.ConnectTo(cornerWaypoint[0, k]);
				}
				Vector2 center = ConvertUnits.ToSimUnits(submarine.HiddenSubPosition);
				float halfHeight = ConvertUnits.ToSimUnits(borders.Height / 2);
				foreach (ValueTuple<WayPoint, int> wayPoint7 in outsideWaypoints)
				{
					WayPoint wp2 = wayPoint7.Item1;
					float xDiff = center.X - wp2.SimPosition.X;
					Vector2 targetPos = new Vector2(center.X - xDiff * 0.5f, center.Y);
					Body wall2 = Submarine.PickBody(wp2.SimPosition, targetPos, null, new Category?(Category.Cat1), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					if (wall2 == null)
					{
						targetPos = new Vector2(center.X - xDiff, center.Y);
						wall2 = Submarine.PickBody(wp2.SimPosition, targetPos, null, new Category?(Category.Cat1), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					}
					if (wall2 != null)
					{
						float distanceFromWall = 1f;
						if (xDiff > 0f && !submarine.Info.HasTag(SubmarineTag.Shuttle))
						{
							float yDist = Math.Abs(center.Y - wp2.SimPosition.Y);
							distanceFromWall = MathHelper.Lerp(1f, 3f, MathUtils.InverseLerp(halfHeight, 0f, yDist));
						}
						Vector2 newPos = Submarine.LastPickedPosition + Submarine.LastPickedNormal * distanceFromWall;
						wp2.rect = new Rectangle(ConvertUnits.ToDisplayUnits(newPos).ToPoint(), wp2.rect.Size);
						wp2.FindHull();
					}
				}
				WayPoint previous = null;
				float tooClose = outSideWaypointInterval / 2f;
				foreach (ValueTuple<WayPoint, int> wayPoint8 in outsideWaypoints)
				{
					WayPoint wp3 = wayPoint8.Item1;
					if (wp3.CurrentHull == null)
					{
						if (Submarine.PickBody(wp3.SimPosition, wp3.SimPosition + Vector2.Normalize(center - wp3.SimPosition) * 0.1f, null, new Category?(Category.Cat1 | Category.Cat5), true, (Fixture f) => !(f.Body.UserData is Submarine), true) == null)
						{
							foreach (ValueTuple<WayPoint, int> otherWayPoint in outsideWaypoints)
							{
								WayPoint otherWp = otherWayPoint.Item1;
								if (otherWp != wp3 && !removals.Contains(otherWp))
								{
									float sqrDist = Vector2.DistanceSquared(wp3.Position, otherWp.Position);
									if (!removals.Contains(previous) && sqrDist < tooClose * tooClose)
									{
										removals.Add(wp3);
									}
								}
							}
							previous = wp3;
							continue;
						}
					}
					removals.Add(wp3);
					previous = wp3;
				}
				using (HashSet<WayPoint>.Enumerator enumerator11 = removals.GetEnumerator())
				{
					while (enumerator11.MoveNext())
					{
						WayPoint wp = enumerator11.Current;
						outsideWaypoints.RemoveAll((ValueTuple<WayPoint, int> w) => w.Item1 == wp);
					}
				}
				removals.ForEach(delegate(WayPoint wp)
				{
					wp.Remove();
				});
				WayPoint.<>c__DisplayClass79_2 CS$<>8__locals3 = new WayPoint.<>c__DisplayClass79_2();
				CS$<>8__locals3.i = 0;
				Func<MapEntity, bool> <>9__9;
				Func<MapEntity, bool> <>9__10;
				while (CS$<>8__locals3.i < outsideWaypoints.Count)
				{
					WayPoint.<>c__DisplayClass79_3 CS$<>8__locals4 = new WayPoint.<>c__DisplayClass79_3();
					CS$<>8__locals4.CS$<>8__locals1 = CS$<>8__locals3;
					CS$<>8__locals4.current = outsideWaypoints[CS$<>8__locals4.CS$<>8__locals1.i].Item1;
					IEnumerable<MapEntity> linkedTo = CS$<>8__locals4.current.linkedTo;
					Func<MapEntity, bool> predicate;
					if ((predicate = <>9__9) == null)
					{
						predicate = (<>9__9 = ((MapEntity l) => !removals.Contains(l)));
					}
					if (linkedTo.Count(predicate) <= 1)
					{
						CS$<>8__locals4.next = null;
						int maxConnections = 2;
						float tooFar = outSideWaypointInterval * 5f;
						int n = 0;
						while (n < maxConnections && CS$<>8__locals4.current.linkedTo.Count < maxConnections)
						{
							float num = tooFar;
							IEnumerable<MapEntity> linkedTo2 = CS$<>8__locals4.current.linkedTo;
							Func<MapEntity, bool> predicate2;
							if ((predicate2 = <>9__10) == null)
							{
								predicate2 = (<>9__10 = ((MapEntity l) => !removals.Contains(l)));
							}
							tooFar = num / (float)linkedTo2.Count(predicate2);
							WayPoint.<>c__DisplayClass79_3 CS$<>8__locals5 = CS$<>8__locals4;
							WayPoint current = CS$<>8__locals4.current;
							IEnumerable<ValueTuple<WayPoint, int>> waypointList = outsideWaypoints;
							float tolerance2 = tooFar;
							Body ignoredBody = null;
							IEnumerable<WayPoint> ignored = null;
							Func<ValueTuple<WayPoint, int>, bool> filter;
							if ((filter = CS$<>8__locals4.<>9__11) == null)
							{
								filter = (CS$<>8__locals4.<>9__11 = delegate(ValueTuple<WayPoint, int> wp)
								{
									if (wp.Item1 != CS$<>8__locals4.next)
									{
										IEnumerable<MapEntity> linkedTo3 = wp.Item1.linkedTo;
										Func<MapEntity, bool> predicate3;
										if ((predicate3 = CS$<>8__locals4.<>9__12) == null)
										{
											predicate3 = (CS$<>8__locals4.<>9__12 = ((MapEntity e) => CS$<>8__locals4.current.linkedTo.Contains(e)));
										}
										if (linkedTo3.None(predicate3) && wp.Item1.linkedTo.Count < 2)
										{
											return wp.Item2 < CS$<>8__locals4.CS$<>8__locals1.i;
										}
									}
									return false;
								});
							}
							CS$<>8__locals5.next = current.FindClosestOutside(waypointList, tolerance2, ignoredBody, ignored, filter);
							if (CS$<>8__locals4.next != null)
							{
								CS$<>8__locals4.current.ConnectTo(CS$<>8__locals4.next);
							}
							n++;
						}
					}
					int i2 = CS$<>8__locals3.i;
					CS$<>8__locals3.i = i2 + 1;
				}
			}
			removals.ForEach(delegate(WayPoint wp)
			{
				wp.Remove();
			});
			removals.Clear();
			foreach (MapEntity mapEntity in MapEntity.MapEntityList.ToList<MapEntity>())
			{
				Structure structure = mapEntity as Structure;
				if (structure != null && structure.StairDirection != Direction.None)
				{
					WayPoint[] stairPoints = new WayPoint[3];
					float margin = -32f;
					stairPoints[0] = new WayPoint(new Vector2((float)(structure.Rect.X + 5), (float)structure.Rect.Y - ((structure.StairDirection == Direction.Left) ? margin : ((float)(structure.Rect.Height - 100)))), SpawnType.Path, submarine, null);
					stairPoints[1] = new WayPoint(new Vector2((float)(structure.Rect.Right - 5), (float)structure.Rect.Y - ((structure.StairDirection == Direction.Left) ? ((float)(structure.Rect.Height - 100)) : margin)), SpawnType.Path, submarine, null);
					for (int m = 0; m < 2; m++)
					{
						for (int dir3 = -1; dir3 <= 1; dir3 += 2)
						{
							WayPoint closest2 = stairPoints[m].FindClosest(dir3, true, new Vector2(minDist * 1.5f, minDist / 2f), null, null, (WayPoint wp) => wp.Stairs == null) ?? stairPoints[m].FindClosest(dir3, true, new Vector2(minDist * 1.5f, minDist / 2f), null, null, null);
							if (closest2 != null)
							{
								stairPoints[m].ConnectTo(closest2);
							}
						}
					}
					stairPoints[2] = new WayPoint((stairPoints[0].Position + stairPoints[1].Position) / 2f, SpawnType.Path, submarine, null);
					stairPoints[0].ConnectTo(stairPoints[2]);
					stairPoints[2].ConnectTo(stairPoints[1]);
					stairPoints.ForEach(delegate(WayPoint wp)
					{
						wp.FindStairs();
					});
				}
			}
			foreach (Item item2 in Item.ItemList)
			{
				Ladder ladders2 = item2.GetComponent<Ladder>();
				if (ladders2 != null)
				{
					Vector2 bottomPoint = new Vector2((float)item2.Rect.Center.X, (float)(item2.Rect.Top - item2.Rect.Height + 10));
					List<ValueTuple<WayPoint, bool>> ladderPoints = new List<ValueTuple<WayPoint, bool>>
					{
						new ValueTuple<WayPoint, bool>(new WayPoint(bottomPoint, SpawnType.Path, submarine, null), true)
					};
					List<Body> ignoredBodies = new List<Body>();
					WayPoint lowestPoint = ladderPoints[0].Item1;
					WayPoint prevPoint = lowestPoint;
					Vector2 prevPos = prevPoint.SimPosition;
					Body ground = Submarine.PickBody(lowestPoint.SimPosition, lowestPoint.SimPosition - Vector2.UnitY, ignoredBodies, new Category?(Category.Cat1 | Category.Cat3 | Category.Cat4), true, (Fixture f) => !(f.Body.UserData is Submarine), false);
					float startHeight = (ground != null) ? ConvertUnits.ToDisplayUnits(ground.Position.Y) : bottomPoint.Y;
					startHeight += heightFromFloor;
					WayPoint startPoint = lowestPoint;
					Vector2 nextPos = new Vector2((float)item2.Rect.Center.X, startHeight);
					if (lowestPoint == null || (Math.Abs(startPoint.Position.Y - startHeight) > 40f && Hull.FindHull(nextPos, null, true, true) != null))
					{
						startPoint = new WayPoint(nextPos, SpawnType.Path, submarine, null);
						ladderPoints.Add(new ValueTuple<WayPoint, bool>(startPoint, true));
						if (lowestPoint != null)
						{
							startPoint.ConnectTo(lowestPoint);
						}
						prevPoint = startPoint;
						prevPos = prevPoint.SimPosition;
					}
					for (float y5 = startPoint.Position.Y + 75f; y5 < (float)item2.Rect.Y - 1f; y5 += 75f)
					{
						Body pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(new Vector2(startPoint.Position.X, y5)), prevPos, ignoredBodies, new Category?(Category.Cat1), false, delegate(Fixture f)
						{
							Item pickedItem = f.Body.UserData as Item;
							return pickedItem != null && pickedItem.GetComponent<Door>() != null;
						}, false);
						Door pickedDoor = null;
						if (pickedBody != null)
						{
							pickedDoor = (((pickedBody != null) ? pickedBody.UserData : null) as Item).GetComponent<Door>();
						}
						else
						{
							pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(new Vector2(startPoint.Position.X, y5)), prevPos, ignoredBodies, null, false, (Fixture f) => f.Body.UserData is Structure, false);
						}
						if (pickedBody != null)
						{
							ignoredBodies.Add(pickedBody);
						}
						if (pickedDoor != null)
						{
							WayPoint newPoint = new WayPoint(pickedDoor.Item.Position, SpawnType.Path, submarine, null);
							ladderPoints.Add(new ValueTuple<WayPoint, bool>(newPoint, true));
							newPoint.ConnectedGap = pickedDoor.LinkedGap;
							newPoint.ConnectTo(prevPoint);
							prevPoint = newPoint;
							prevPos = new Vector2(prevPos.X, ConvertUnits.ToSimUnits(pickedDoor.Item.Position.Y - (float)pickedDoor.Item.Rect.Height));
							y5 = Math.Max(pickedDoor.Item.Position.Y, y5);
						}
						else
						{
							Vector2 pos = (pickedBody == null) ? new Vector2(startPoint.Position.X, y5) : (ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition) + Vector2.UnitY * heightFromFloor);
							WayPoint newPoint2 = new WayPoint(pos, SpawnType.Path, submarine, null);
							ladderPoints.Add(new ValueTuple<WayPoint, bool>(newPoint2, pickedBody != null));
							newPoint2.ConnectTo(prevPoint);
							prevPoint = newPoint2;
							prevPos = ConvertUnits.ToSimUnits(newPoint2.Position);
							if (pickedBody != null)
							{
								y5 = Math.Max(newPoint2.Position.Y, y5);
							}
						}
					}
					if (prevPoint.rect.Y < item2.Rect.Y - 40)
					{
						WayPoint wayPoint9 = new WayPoint(new Vector2((float)item2.Rect.Center.X, (float)item2.Rect.Y - 1f), SpawnType.Path, submarine, null);
						ladderPoints.Add(new ValueTuple<WayPoint, bool>(wayPoint9, true));
						wayPoint9.ConnectTo(prevPoint);
					}
					IEnumerable<WayPoint> ladderWaypoints = from lp in ladderPoints
					select lp.Item1;
					foreach (ValueTuple<WayPoint, bool> ladderPoint in ladderPoints)
					{
						WayPoint wp4 = ladderPoint.Item1;
						wp4.Ladders = ladders2;
						if (ladderPoint.Item2)
						{
							bool isHatch = wp4.ConnectedGap != null && !wp4.ConnectedGap.IsRoomToRoom;
							for (int dir4 = -1; dir4 <= 1; dir4 += 2)
							{
								WayPoint wayPoint13;
								if (!isHatch)
								{
									WayPoint wayPoint12 = wp4;
									int dir7 = dir4;
									bool horizontalSearch = true;
									Vector2 tolerance3 = new Vector2(150f, 100f);
									Gap connectedGap = wp4.ConnectedGap;
									Body ignoredBody2;
									if (connectedGap == null)
									{
										ignoredBody2 = null;
									}
									else
									{
										Door connectedDoor = connectedGap.ConnectedDoor;
										ignoredBody2 = ((connectedDoor != null) ? connectedDoor.Body.FarseerBody : null);
									}
									wayPoint13 = wayPoint12.FindClosest(dir7, horizontalSearch, tolerance3, ignoredBody2, ladderWaypoints, null);
								}
								else
								{
									WayPoint wayPoint14 = wp4;
									int dir8 = dir4;
									bool horizontalSearch2 = true;
									Vector2 tolerance4 = new Vector2(500f, 1000f);
									Gap connectedGap2 = wp4.ConnectedGap;
									Body ignoredBody3;
									if (connectedGap2 == null)
									{
										ignoredBody3 = null;
									}
									else
									{
										Door connectedDoor2 = connectedGap2.ConnectedDoor;
										ignoredBody3 = ((connectedDoor2 != null) ? connectedDoor2.Body.FarseerBody : null);
									}
									wayPoint13 = wayPoint14.FindClosest(dir8, horizontalSearch2, tolerance4, ignoredBody3, ladderWaypoints, (WayPoint wp) => wp.CurrentHull == null);
								}
								WayPoint closest3 = wayPoint13;
								if (closest3 != null)
								{
									wp4.ConnectTo(closest3);
								}
							}
						}
					}
				}
			}
			foreach (Item item3 in Item.ItemList)
			{
				Ladder ladders = item3.GetComponent<Ladder>();
				if (ladders != null)
				{
					IOrderedEnumerable<WayPoint> wps = from wp in WayPoint.WayPointList
					where wp.Ladders == ladders
					orderby wp.Rect.Y descending
					select wp;
					WayPoint cap = wps.First<WayPoint>();
					WayPoint above = cap.FindClosest(1, false, new Vector2(25f, 50f), null, null, (WayPoint wp) => wp.Ladders != null && wp.Ladders != ladders);
					if (above != null)
					{
						above.ConnectTo(cap);
					}
					WayPoint bottom2 = wps.Last<WayPoint>();
					WayPoint below = bottom2.FindClosest(-1, false, new Vector2(25f, 50f), null, null, (WayPoint wp) => wp.Ladders != null && wp.Ladders != ladders);
					if (below != null)
					{
						below.ConnectTo(bottom2);
					}
				}
			}
			using (List<Gap>.Enumerator enumerator16 = Gap.GapList.GetEnumerator())
			{
				while (enumerator16.MoveNext())
				{
					Gap gap = enumerator16.Current;
					if (gap.IsHorizontal)
					{
						if (isRuin)
						{
							if (gap.Rect.Height < 50)
							{
								continue;
							}
						}
						else if ((float)gap.Rect.Height < hullMinHeight)
						{
							continue;
						}
						Vector2 pos2 = new Vector2((float)gap.Rect.Center.X, (float)(gap.Rect.Y - gap.Rect.Height) + heightFromFloor);
						if (isRuin)
						{
							pos2.Y = (float)(gap.Rect.Y - gap.Rect.Height / 2);
						}
						WayPoint wayPoint10 = new WayPoint(pos2, SpawnType.Path, submarine, gap);
						Vector2 tolerance = (gap.IsRoomToRoom && !isRuin) ? new Vector2(150f, 70f) : new Vector2(1000f, 1000f);
						for (int dir5 = -1; dir5 <= 1; dir5 += 2)
						{
							WayPoint wayPoint15 = wayPoint10;
							int dir9 = dir5;
							bool horizontalSearch3 = true;
							Vector2 tolerance5 = tolerance;
							Door connectedDoor3 = gap.ConnectedDoor;
							WayPoint closest4 = wayPoint15.FindClosest(dir9, horizontalSearch3, tolerance5, (connectedDoor3 != null) ? connectedDoor3.Body.FarseerBody : null, null, null);
							if (closest4 != null)
							{
								wayPoint10.ConnectTo(closest4);
							}
						}
					}
					else
					{
						if (!isRuin)
						{
							if (gap.IsRoomToRoom)
							{
								continue;
							}
							if (gap.linkedTo.None((MapEntity l) => l is Hull))
							{
								continue;
							}
						}
						if ((float)gap.Rect.Width >= 50f)
						{
							Vector2 pos3 = new Vector2((float)gap.Rect.Center.X, (float)(gap.Rect.Y - gap.Rect.Height / 2));
							if (!WayPoint.WayPointList.Any((WayPoint wp) => wp.ConnectedGap == gap))
							{
								WayPoint wayPoint11 = new WayPoint(pos3, SpawnType.Path, submarine, gap);
								Hull connectedHull = (Hull)gap.linkedTo.First((MapEntity l) => l is Hull);
								int dir6 = Math.Sign(connectedHull.Position.Y - gap.Position.Y);
								WayPoint closest5 = wayPoint11.FindClosest(dir6, false, isRuin ? new Vector2(500f, 500f) : new Vector2(50f, 100f), null, null, null);
								if (closest5 != null)
								{
									wayPoint11.ConnectTo(closest5);
								}
								if (isRuin)
								{
									closest5 = wayPoint11.FindClosest(-dir6, false, isRuin ? new Vector2(500f, 500f) : new Vector2(50f, 100f), null, null, null);
									if (closest5 != null)
									{
										wayPoint11.ConnectTo(closest5);
									}
								}
								for (dir6 = -1; dir6 <= 1; dir6 += 2)
								{
									WayPoint wayPoint16 = wayPoint11;
									int dir10 = dir6;
									bool horizontalSearch4 = true;
									Vector2 tolerance6 = new Vector2(500f, 1000f);
									Door connectedDoor4 = gap.ConnectedDoor;
									closest5 = wayPoint16.FindClosest(dir10, horizontalSearch4, tolerance6, (connectedDoor4 != null) ? connectedDoor4.Body.FarseerBody : null, null, (WayPoint wp) => wp.CurrentHull == null);
									if (closest5 != null)
									{
										wayPoint11.ConnectTo(closest5);
									}
								}
							}
						}
					}
				}
			}
			List<WayPoint> orphans = WayPoint.WayPointList.FindAll((WayPoint w) => w.spawnType == SpawnType.Path && w.linkedTo.None(null));
			foreach (WayPoint wp5 in orphans)
			{
				wp5.Remove();
			}
			foreach (WayPoint wp6 in WayPoint.WayPointList)
			{
				if (wp6.SpawnType == SpawnType.Path && wp6.CurrentHull == null && wp6.Ladders == null && wp6.linkedTo.Count < 2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(133, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Couldn't automatically link the waypoint ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(wp6.ID);
					defaultInterpolatedStringHandler.AppendLiteral(" outside of the submarine. You should do it manually. The waypoint ID is shown in red color.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			foreach (Door door2 in openDoors)
			{
				door2.Body.Enabled = false;
			}
			return true;
		}

		// Token: 0x060022BF RID: 8895 RVA: 0x00161170 File Offset: 0x0015F370
		private WayPoint FindClosestOutside(IEnumerable<ValueTuple<WayPoint, int>> waypointList, float tolerance, Body ignoredBody = null, IEnumerable<WayPoint> ignored = null, Func<ValueTuple<WayPoint, int>, bool> filter = null)
		{
			float closestDist = 0f;
			WayPoint closest = null;
			foreach (ValueTuple<WayPoint, int> wayPoint in waypointList)
			{
				WayPoint wp = wayPoint.Item1;
				if (wp.SpawnType == SpawnType.Path && wp != this && !this.linkedTo.Contains(wp) && (ignored == null || !ignored.Contains(wp)) && (filter == null || filter(wayPoint)))
				{
					float sqrDist = Vector2.DistanceSquared(this.Position, wp.Position);
					if (sqrDist <= tolerance * tolerance && (closest == null || sqrDist < closestDist))
					{
						Body body = Submarine.CheckVisibility(this.SimPosition, wp.SimPosition, true, true, false, true, true, null);
						if (body == null || body == ignoredBody || body.UserData is Submarine || (!(body.UserData is Structure) && !body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1)))
						{
							closestDist = sqrDist;
							closest = wp;
						}
					}
				}
			}
			return closest;
		}

		// Token: 0x060022C0 RID: 8896 RVA: 0x001612A0 File Offset: 0x0015F4A0
		private WayPoint FindClosest(int dir, bool horizontalSearch, Vector2 tolerance, Body ignoredBody = null, IEnumerable<WayPoint> ignored = null, Func<WayPoint, bool> filter = null)
		{
			if (dir != -1 && dir != 1)
			{
				return null;
			}
			float closestDist = 0f;
			WayPoint closest = null;
			foreach (WayPoint wp in WayPoint.WayPointList)
			{
				if (wp.SpawnType == SpawnType.Path && wp != this)
				{
					float xDiff = wp.Position.X - this.Position.X;
					float yDiff = wp.Position.Y - this.Position.Y;
					float xDist = Math.Abs(xDiff);
					float yDist = Math.Abs(yDiff);
					if (tolerance.X >= xDist && tolerance.Y >= yDist)
					{
						float diff;
						float dist;
						if (horizontalSearch)
						{
							diff = xDiff;
							dist = xDist + yDist / 5f;
						}
						else
						{
							diff = yDiff;
							dist = yDist + xDist / 5f;
							if (wp.Ladders != null)
							{
								dist *= 0.5f;
							}
						}
						if (Math.Sign(diff) == dir && !this.linkedTo.Contains(wp) && (ignored == null || !ignored.Contains(wp)) && (filter == null || filter(wp)) && (closest == null || dist < closestDist))
						{
							Body body = Submarine.CheckVisibility(this.SimPosition, wp.SimPosition, true, true, false, true, true, null);
							if (body != null && body != ignoredBody && !(body.UserData is Submarine))
							{
								if (body.UserData is Structure)
								{
									continue;
								}
								if (body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1))
								{
									Item i = body.UserData as Item;
									if (i != null && i.GetComponent<Door>() != null)
									{
										continue;
									}
								}
							}
							closestDist = dist;
							closest = wp;
						}
					}
				}
			}
			return closest;
		}

		// Token: 0x060022C1 RID: 8897 RVA: 0x00161498 File Offset: 0x0015F698
		public void ConnectTo(WayPoint wayPoint2)
		{
			if (!this.linkedTo.Contains(wayPoint2))
			{
				this.linkedTo.Add(wayPoint2);
				Action<WayPoint> onLinksChanged = this.OnLinksChanged;
				if (onLinksChanged != null)
				{
					onLinksChanged(this);
				}
			}
			if (!wayPoint2.linkedTo.Contains(this))
			{
				wayPoint2.linkedTo.Add(this);
				Action<WayPoint> onLinksChanged2 = wayPoint2.OnLinksChanged;
				if (onLinksChanged2 == null)
				{
					return;
				}
				onLinksChanged2(wayPoint2);
			}
		}

		// Token: 0x060022C2 RID: 8898 RVA: 0x001614FC File Offset: 0x0015F6FC
		public static WayPoint GetRandom(SpawnType spawnType = SpawnType.Human, JobPrefab assignedJob = null, Submarine sub = null, bool useSyncedRand = false, string spawnPointTag = null, bool ignoreSubmarine = false)
		{
			Func<Identifier, bool> <>9__1;
			return WayPoint.WayPointList.GetRandom(delegate(WayPoint wp)
			{
				if ((ignoreSubmarine || wp.Submarine == sub) && !wp.spawnType.HasFlag(SpawnType.Disabled) && wp.spawnType == spawnType)
				{
					if (!spawnPointTag.IsNullOrEmpty())
					{
						IEnumerable<Identifier> source = wp.Tags;
						Func<Identifier, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Identifier t) => t == spawnPointTag));
						}
						if (!source.Any(predicate))
						{
							return false;
						}
					}
					return assignedJob == null || (assignedJob != null && wp.AssignedJob == assignedJob);
				}
				return false;
			}, useSyncedRand ? Rand.RandSync.ServerAndClient : Rand.RandSync.Unsynced);
		}

		// Token: 0x060022C3 RID: 8899 RVA: 0x00161554 File Offset: 0x0015F754
		public static WayPoint[] SelectCrewSpawnPoints(List<CharacterInfo> crew, Submarine submarine)
		{
			List<WayPoint> subWayPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Submarine == submarine);
			if (submarine.ForcedOutpostModuleWayPoints != null && submarine.ForcedOutpostModuleWayPoints.Any<WayPoint>())
			{
				subWayPoints = new List<WayPoint>(submarine.ForcedOutpostModuleWayPoints);
				submarine.ForcedOutpostModuleWayPoints.Clear();
			}
			subWayPoints.Shuffle(Rand.RandSync.Unsynced);
			List<WayPoint> unassignedWayPoints = subWayPoints.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Human);
			WayPoint[] assignedWayPoints = new WayPoint[crew.Count];
			for (int i = 0; i < crew.Count; i++)
			{
				for (int j = 0; j < unassignedWayPoints.Count; j++)
				{
					if (crew[i].Job.Prefab == unassignedWayPoints[j].AssignedJob)
					{
						assignedWayPoints[i] = unassignedWayPoints[j];
						unassignedWayPoints.RemoveAt(j);
						break;
					}
				}
			}
			for (int k = 0; k < crew.Count; k++)
			{
				if (assignedWayPoints[k] == null)
				{
					foreach (WayPoint wp2 in subWayPoints)
					{
						if (wp2.spawnType == SpawnType.Human && wp2.AssignedJob == crew[k].Job.Prefab)
						{
							assignedWayPoints[k] = wp2;
							break;
						}
					}
					if (assignedWayPoints[k] == null)
					{
						List<WayPoint> nonJobSpecificPoints = subWayPoints.FindAll((WayPoint wp) => wp.spawnType == SpawnType.Human && wp.AssignedJob == null);
						if (nonJobSpecificPoints.Any<WayPoint>())
						{
							assignedWayPoints[k] = nonJobSpecificPoints[Rand.Int(nonJobSpecificPoints.Count, Rand.RandSync.ServerAndClient)];
						}
						if (assignedWayPoints[k] == null)
						{
							assignedWayPoints[k] = WayPoint.GetRandom(SpawnType.Human, null, submarine, true, null, false);
						}
					}
				}
			}
			for (int l = 0; l < assignedWayPoints.Length; l++)
			{
				if (assignedWayPoints[l] == null)
				{
					DebugConsole.AddWarning("Couldn't find a waypoint for " + crew[l].Name + "!", null);
					assignedWayPoints[l] = WayPoint.WayPointList[0];
				}
			}
			return assignedWayPoints;
		}

		// Token: 0x060022C4 RID: 8900 RVA: 0x001617A0 File Offset: 0x0015F9A0
		public static WayPoint[] SelectOutpostSpawnPoints(List<CharacterInfo> crew, CharacterTeamType teamID)
		{
			List<WayPoint> potentialSpawnPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Human && wp.Submarine == Level.Loaded.StartOutpost);
			if (GameMain.GameSession.GameMode is PvPMode)
			{
				Identifier teamSpawnTag = ("deathmatch" + teamID.ToString()).ToIdentifier();
				if (potentialSpawnPoints.Any((WayPoint wp) => wp.Tags.Contains(teamSpawnTag)))
				{
					potentialSpawnPoints = potentialSpawnPoints.FindAll((WayPoint wp) => wp.Tags.Contains(teamSpawnTag));
				}
			}
			else
			{
				potentialSpawnPoints = potentialSpawnPoints.FindAll(delegate(WayPoint wp)
				{
					Hull currentHull = wp.CurrentHull;
					return ((currentHull != null) ? currentHull.OutpostModuleTags : null) != null && wp.CurrentHull.OutpostModuleTags.Contains(Barotrauma.Tags.Airlock);
				});
			}
			if (potentialSpawnPoints.None(null))
			{
				return potentialSpawnPoints.ToArray();
			}
			List<WayPoint> spawnPoints = new List<WayPoint>();
			int i;
			Func<WayPoint, bool> <>9__4;
			int j;
			for (i = 0; i < crew.Count; i = j + 1)
			{
				IEnumerable<WayPoint> source = potentialSpawnPoints;
				Func<WayPoint, bool> predicate;
				if ((predicate = <>9__4) == null)
				{
					predicate = (<>9__4 = ((WayPoint wp) => wp.AssignedJob == crew[i].Job.Prefab));
				}
				IEnumerable<WayPoint> spawnPointsForJob = source.Where(predicate);
				IEnumerable<WayPoint> spawnPointsForAnyJob = from wp in potentialSpawnPoints
				where wp.AssignedJob == null
				select wp;
				if (spawnPointsForJob.Any<WayPoint>())
				{
					spawnPoints.Add(spawnPointsForJob.GetRandomUnsynced<WayPoint>());
				}
				else if (spawnPointsForAnyJob.Any<WayPoint>())
				{
					spawnPoints.Add(spawnPointsForAnyJob.GetRandomUnsynced<WayPoint>());
				}
				else
				{
					spawnPoints.Add(potentialSpawnPoints.GetRandomUnsynced<WayPoint>());
				}
				j = i;
			}
			return spawnPoints.ToArray();
		}

		// Token: 0x060022C5 RID: 8901 RVA: 0x00161948 File Offset: 0x0015FB48
		public void FindHull()
		{
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
			if (Screen.Selected == GameMain.SubEditorScreen && this.CurrentHull == null)
			{
				this.CurrentHull = Hull.FindHullUnoptimized(this.WorldPosition, null, true, true);
			}
		}

		// Token: 0x060022C6 RID: 8902 RVA: 0x00161998 File Offset: 0x0015FB98
		public override void OnMapLoaded()
		{
			if (base.Submarine == null)
			{
				return;
			}
			this.InitializeLinks();
			this.FindHull();
			this.FindStairs();
		}

		// Token: 0x060022C7 RID: 8903 RVA: 0x001619B8 File Offset: 0x0015FBB8
		private Structure FindStairs()
		{
			this.Stairs = null;
			Body pickedBody = Submarine.PickBody(this.SimPosition, this.SimPosition - new Vector2(0f, 1.2f), null, new Category?(Category.Cat4), true, null, false);
			if (pickedBody != null)
			{
				Structure structure = pickedBody.UserData as Structure;
				if (structure != null && structure.StairDirection != Direction.None)
				{
					this.Stairs = structure;
				}
			}
			return this.Stairs;
		}

		// Token: 0x060022C8 RID: 8904 RVA: 0x00161A24 File Offset: 0x0015FC24
		public void InitializeLinks()
		{
			if (this.gapId > 0)
			{
				this.ConnectedGap = (Entity.FindEntityByID(this.gapId) as Gap);
				this.gapId = 0;
			}
			if (this.ladderId > 0)
			{
				Item ladderItem = Entity.FindEntityByID(this.ladderId) as Item;
				if (ladderItem != null)
				{
					this.Ladders = ladderItem.GetComponent<Ladder>();
				}
				this.ladderId = 0;
			}
		}

		// Token: 0x060022C9 RID: 8905 RVA: 0x00161A88 File Offset: 0x0015FC88
		public static WayPoint Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			Rectangle rect = new Rectangle(int.Parse(element.GetAttribute("x").Value), int.Parse(element.GetAttribute("y").Value), (int)Submarine.GridSize.X, (int)Submarine.GridSize.Y);
			SpawnType spawnType;
			Enum.TryParse<SpawnType>(element.GetAttributeString("spawn", "Path"), out spawnType);
			WayPoint w = new WayPoint((spawnType == SpawnType.Path) ? WayPoint.Type.WayPoint : WayPoint.Type.SpawnPoint, rect, submarine, idRemap.GetOffsetId(element))
			{
				spawnType = spawnType,
				Layer = element.GetAttributeString("Layer", null)
			};
			string idCardDescString = element.GetAttributeString("idcarddesc", "");
			if (!string.IsNullOrWhiteSpace(idCardDescString))
			{
				w.IdCardDesc = idCardDescString;
			}
			string idCardTagString = element.GetAttributeString("idcardtags", "");
			if (!string.IsNullOrWhiteSpace(idCardTagString))
			{
				w.IdCardTags = idCardTagString.Split(',', StringSplitOptions.None);
			}
			WayPoint wayPoint = w;
			string key = "exitpointsize";
			Point zero = Point.Zero;
			wayPoint.ExitPointSize = element.GetAttributePoint(key, zero);
			w.tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			Identifier jobIdentifier = element.GetAttributeIdentifier("job", Identifier.Empty);
			if (!jobIdentifier.IsEmpty)
			{
				w.AssignedJob = JobPrefab.Get(jobIdentifier);
			}
			w.linkedToID = new List<ushort>();
			w.ladderId = idRemap.GetOffsetId(element.GetAttributeInt("ladders", 0));
			w.gapId = idRemap.GetOffsetId(element.GetAttributeInt("gap", 0));
			int i = 0;
			while (element.GetAttribute("linkedto" + i.ToString()) != null)
			{
				int srcId = int.Parse(element.GetAttribute("linkedto" + i.ToString()).Value);
				int destId = (int)idRemap.GetOffsetId(srcId);
				if (destId > 0)
				{
					w.linkedToID.Add((ushort)destId);
				}
				else
				{
					WayPoint wayPoint2 = w;
					if (wayPoint2.unresolvedLinkedToID == null)
					{
						wayPoint2.unresolvedLinkedToID = new List<ushort>();
					}
					w.unresolvedLinkedToID.Add((ushort)srcId);
				}
				i++;
			}
			return w;
		}

		// Token: 0x060022CA RID: 8906 RVA: 0x00161C9C File Offset: 0x0015FE9C
		public override XElement Save(XElement parentElement)
		{
			if (!this.ShouldBeSaved)
			{
				return null;
			}
			XElement element = new XElement("WayPoint");
			element.Add(new object[]
			{
				new XAttribute("ID", this.ID),
				new XAttribute("x", (int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)),
				new XAttribute("y", (int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)),
				new XAttribute("spawn", this.spawnType),
				new XAttribute("Layer", base.Layer ?? string.Empty)
			});
			if (this.SpawnType == SpawnType.ExitPoint)
			{
				element.Add(new XAttribute("exitpointsize", XMLExtensions.PointToString(this.ExitPointSize)));
			}
			if (!string.IsNullOrWhiteSpace(this.IdCardDesc))
			{
				element.Add(new XAttribute("idcarddesc", this.IdCardDesc));
			}
			if (this.idCardTags.Length != 0)
			{
				element.Add(new XAttribute("idcardtags", string.Join(",", this.idCardTags)));
			}
			if (this.tags.Count > 0)
			{
				element.Add(new XAttribute("tags", string.Join<Identifier>(",", this.tags)));
			}
			if (this.AssignedJob != null)
			{
				element.Add(new XAttribute("job", this.AssignedJob.Identifier));
			}
			if (this.ConnectedGap != null)
			{
				element.Add(new XAttribute("gap", this.ConnectedGap.ID));
			}
			if (this.Ladders != null)
			{
				element.Add(new XAttribute("ladders", this.Ladders.Item.ID));
			}
			parentElement.Add(element);
			if (this.linkedTo != null)
			{
				int i = 0;
				foreach (MapEntity e in this.linkedTo)
				{
					if (e.ShouldBeSaved && e.Removed == base.Removed)
					{
						Submarine submarine = e.Submarine;
						SubmarineType? submarineType = (submarine != null) ? new SubmarineType?(submarine.Info.Type) : null;
						Submarine submarine2 = base.Submarine;
						SubmarineType? submarineType2 = (submarine2 != null) ? new SubmarineType?(submarine2.Info.Type) : null;
						if (submarineType.GetValueOrDefault() == submarineType2.GetValueOrDefault() & submarineType != null == (submarineType2 != null))
						{
							element.Add(new XAttribute("linkedto" + i.ToString(), e.ID));
							i++;
						}
					}
				}
			}
			return element;
		}

		// Token: 0x060022CB RID: 8907 RVA: 0x00161FEC File Offset: 0x001601EC
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			WayPoint.WayPointList.Remove(this);
		}

		// Token: 0x060022CC RID: 8908 RVA: 0x00162000 File Offset: 0x00160200
		public override void Remove()
		{
			base.Remove();
			this.CurrentHull = null;
			this.ConnectedGap = null;
			this.Tunnel = null;
			this.Ruin = null;
			this.Stairs = null;
			this.Ladders = null;
			this.OnLinksChanged = null;
			WayPoint.WayPointList.Remove(this);
		}

		// Token: 0x0400116E RID: 4462
		private static Dictionary<string, Sprite> iconSprites;

		// Token: 0x0400116F RID: 4463
		private const int WaypointSize = 12;

		// Token: 0x04001170 RID: 4464
		private const int SpawnPointSize = 32;

		// Token: 0x04001171 RID: 4465
		public static List<WayPoint> WayPointList = new List<WayPoint>();

		// Token: 0x04001172 RID: 4466
		public static bool ShowWayPoints = true;

		// Token: 0x04001173 RID: 4467
		public static bool ShowSpawnPoints = true;

		// Token: 0x04001174 RID: 4468
		public const float LadderWaypointInterval = 75f;

		// Token: 0x04001175 RID: 4469
		protected SpawnType spawnType;

		// Token: 0x04001176 RID: 4470
		private string[] idCardTags;

		// Token: 0x04001177 RID: 4471
		private ushort ladderId;

		// Token: 0x04001178 RID: 4472
		public Ladder Ladders;

		// Token: 0x04001179 RID: 4473
		public Structure Stairs;

		// Token: 0x0400117A RID: 4474
		private HashSet<Identifier> tags;

		// Token: 0x0400117B RID: 4475
		public bool IsObstructed;

		// Token: 0x0400117C RID: 4476
		private HashSet<Gap> openGaps;

		// Token: 0x0400117D RID: 4477
		private ushort gapId;

		// Token: 0x04001180 RID: 4480
		public Level.Tunnel Tunnel;

		// Token: 0x04001181 RID: 4481
		public Ruin Ruin;

		// Token: 0x04001182 RID: 4482
		public Level.Cave Cave;

		// Token: 0x02000BBF RID: 3007
		public enum Type
		{
			// Token: 0x040048C2 RID: 18626
			WayPoint,
			// Token: 0x040048C3 RID: 18627
			SpawnPoint
		}
	}
}
