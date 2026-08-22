using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E0 RID: 1504
	internal class Wire : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x1700189C RID: 6300
		// (get) Token: 0x06006183 RID: 24963 RVA: 0x0032B052 File Offset: 0x00329252
		public Vector2 DrawSize
		{
			get
			{
				return this.sectionExtents;
			}
		}

		// Token: 0x1700189D RID: 6301
		// (get) Token: 0x06006184 RID: 24964 RVA: 0x0032B05A File Offset: 0x0032925A
		public static Wire DraggingWire
		{
			get
			{
				return Wire.draggingWire;
			}
		}

		// Token: 0x06006185 RID: 24965 RVA: 0x0032B064 File Offset: 0x00329264
		public static Sprite ExtractWireSprite(ContentXElement element)
		{
			if (Wire.defaultWireSprite == null)
			{
				Wire.defaultWireSprite = new Sprite("Content/Items/Electricity/signalcomp.png", new Rectangle?(new Rectangle(970, 47, 14, 16)), new Vector2?(new Vector2(0.5f, 0.5f)), 0f)
				{
					Depth = 0.855f
				};
			}
			Sprite overrideSprite = null;
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("wiresprite", StringComparison.OrdinalIgnoreCase))
				{
					overrideSprite = new Sprite(subElement, "", "", false, 1f);
					break;
				}
			}
			return overrideSprite ?? Wire.defaultWireSprite;
		}

		// Token: 0x06006186 RID: 24966 RVA: 0x0032B138 File Offset: 0x00329338
		public void RegisterSignal(Signal signal, Connection source)
		{
			this.lastReceivedSignal = new Wire.VisualSignal((float)Timing.TotalTimeUnpaused, Wire.GetSignalColor(signal), (source == this.connections[0]) ? 1 : -1);
		}

		// Token: 0x06006187 RID: 24967 RVA: 0x0032B160 File Offset: 0x00329360
		private static Color GetSignalColor(Signal signal)
		{
			if (signal.value == "0")
			{
				return Color.Red;
			}
			if (signal.value == "1")
			{
				return Color.LightGreen;
			}
			float floatValue;
			if (float.TryParse(signal.value, out floatValue))
			{
				return ToolBox.GradientLerp(Math.Abs(floatValue / 200f), Wire.dataSignalColors);
			}
			return Color.LightBlue;
		}

		// Token: 0x06006188 RID: 24968 RVA: 0x0032B1C8 File Offset: 0x003293C8
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			this.Draw(spriteBatch, editing, Vector2.Zero, itemDepth, overrideColor);
		}

		// Token: 0x06006189 RID: 24969 RVA: 0x0032B1DC File Offset: 0x003293DC
		public void Draw(SpriteBatch spriteBatch, bool editing, Vector2 offset, float itemDepth = -1f, Color? overrideColor = null)
		{
			if ((this.sections.Count == 0 && !this.IsActive) || this.Hidden)
			{
				base.Drawable = false;
				return;
			}
			if (this.Width * this.wireSprite.size.Y * Screen.Selected.Cam.Zoom < 1f)
			{
				return;
			}
			Vector2 drawOffset = this.GetDrawOffset() + offset;
			float baseDepth = this.UseSpriteDepth ? this.item.SpriteDepth : this.wireSprite.Depth;
			float depth = this.item.IsSelected ? 0f : (SubEditorScreen.IsWiringMode() ? 0.02f : (baseDepth + (float)(this.item.ID % 100) * 1E-06f));
			if (this.item.IsHighlighted)
			{
				using (List<Wire.WireSection>.Enumerator enumerator = this.sections.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Wire.WireSection section = enumerator.Current;
						section.Draw(spriteBatch, this.wireSprite, (Screen.Selected == GameMain.GameScreen) ? Wire.higlightColor : Wire.editorHighlightColor, drawOffset, depth + 1E-05f, this.Width * 2f);
					}
					goto IL_198;
				}
			}
			if (this.item.IsSelected)
			{
				foreach (Wire.WireSection section2 in this.sections)
				{
					section2.Draw(spriteBatch, this.wireSprite, Wire.editorSelectedColor, drawOffset, depth + 1E-05f, this.Width * 2f);
				}
			}
			IL_198:
			foreach (Wire.WireSection section3 in this.sections)
			{
				section3.Draw(spriteBatch, this.wireSprite, overrideColor ?? this.item.Color, drawOffset, depth, this.Width);
			}
			if (this.nodes.Count > 0)
			{
				if (!this.IsActive)
				{
					if (this.connections[0] == null)
					{
						this.DrawHangingWire(spriteBatch, this.nodes[0] + drawOffset, depth);
					}
					if (this.connections[1] == null)
					{
						this.DrawHangingWire(spriteBatch, this.nodes.Last<Vector2>() + drawOffset, depth);
					}
				}
				if (this.IsActive)
				{
					Inventory parentInventory = this.item.ParentInventory;
					Character user = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
					if (user != null && user == Character.Controlled)
					{
						if (user.CanInteract && this.currLength < this.MaxLength)
						{
							Vector2 gridPos = Character.Controlled.Position;
							Vector2 roundedGridPos = new Vector2(MathUtils.RoundTowardsClosest(Character.Controlled.Position.X, Submarine.GridSize.X), MathUtils.RoundTowardsClosest(Character.Controlled.Position.Y, Submarine.GridSize.Y));
							if (this.item.Submarine == null)
							{
								Structure attachTarget = Structure.GetAttachTarget(this.item.WorldPosition);
								if (attachTarget != null && attachTarget.Submarine != null)
								{
									gridPos += attachTarget.Submarine.Position;
									roundedGridPos += attachTarget.Submarine.Position;
								}
							}
							else
							{
								gridPos += this.item.Submarine.Position;
								roundedGridPos += this.item.Submarine.Position;
							}
							if (!SubEditorScreen.IsSubEditor() || !SubEditorScreen.ShouldDrawGrid)
							{
								Submarine.DrawGrid(spriteBatch, 14, gridPos, roundedGridPos, 0.25f, null);
							}
							Sprite sprite = this.wireSprite;
							List<Vector2> list = this.nodes;
							Wire.WireSection.Draw(spriteBatch, sprite, list[list.Count - 1] + drawOffset, new Vector2(this.newNodePos.X, this.newNodePos.Y) + drawOffset, overrideColor ?? this.item.Color, 0f, this.Width);
							Wire.WireSection.Draw(spriteBatch, this.wireSprite, new Vector2(this.newNodePos.X, this.newNodePos.Y) + drawOffset, this.item.DrawPosition, overrideColor ?? this.item.Color, itemDepth, this.Width);
							GUI.DrawRectangle(spriteBatch, new Vector2(this.newNodePos.X + drawOffset.X, -(this.newNodePos.Y + drawOffset.Y)) - Vector2.One * 3f, Vector2.One * 6f, this.item.Color, false, 0f, 1f);
						}
						else
						{
							Sprite sprite2 = this.wireSprite;
							List<Vector2> list2 = this.nodes;
							Wire.WireSection.Draw(spriteBatch, sprite2, list2[list2.Count - 1] + drawOffset, this.item.DrawPosition, overrideColor ?? this.item.Color, 0f, this.Width);
						}
					}
				}
			}
			if (ConnectionPanel.ShouldDebugDrawWiring)
			{
				this.DebugDraw(spriteBatch, 0.2f);
			}
			if (!editing || !GameMain.SubEditorScreen.WiringMode)
			{
				return;
			}
			int i = 0;
			while (i < this.nodes.Count)
			{
				Vector2 drawPos = this.nodes[i];
				if (this.item.Submarine != null)
				{
					drawPos += this.item.Submarine.Position + this.item.Submarine.HiddenSubPosition;
				}
				drawPos.Y = -drawPos.Y;
				int? num = Wire.highlightedNodeIndex;
				int num2 = i;
				if ((num.GetValueOrDefault() == num2 & num != null) && this.item.IsHighlighted)
				{
					goto IL_633;
				}
				num = Wire.selectedNodeIndex;
				num2 = i;
				if ((num.GetValueOrDefault() == num2 & num != null) && this.item.IsSelected)
				{
					goto IL_633;
				}
				IL_66E:
				if (this.item.IsSelected)
				{
					GUI.DrawRectangle(spriteBatch, drawPos + new Vector2(-5f, -5f), new Vector2(10f, 10f), this.item.Color, true, 0f, 1f);
				}
				else
				{
					GUI.DrawRectangle(spriteBatch, drawPos + new Vector2(-3f, -3f), new Vector2(6f, 6f), this.item.Color, true, 0.015f, 1f);
				}
				i++;
				continue;
				IL_633:
				GUI.DrawRectangle(spriteBatch, drawPos + new Vector2(-10f, -10f), new Vector2(20f, 20f), Wire.editorHighlightColor, false, 0f, 1f);
				goto IL_66E;
			}
		}

		// Token: 0x0600618A RID: 24970 RVA: 0x0032B928 File Offset: 0x00329B28
		public void DebugDraw(SpriteBatch spriteBatch, float alpha = 1f)
		{
			if (this.sections.Count == 0 || this.Hidden)
			{
				return;
			}
			Vector2 drawOffset = this.GetDrawOffset();
			Color currentHighlightColor = Color.Transparent;
			float highlightScale = 0f;
			if (this.connections[0] != null && this.connections[1] != null)
			{
				float voltage = Math.Max(this.<DebugDraw>g__GetVoltage|23_0(0), this.<DebugDraw>g__GetVoltage|23_0(1));
				if (voltage > 0f)
				{
					float pulseSpeed = (voltage > 1.2f) ? 10f : 5f;
					float pulseAmount = (MathF.Sin((float)Timing.TotalTimeUnpaused * pulseSpeed) + 1.5f) / 2.5f;
					voltage = Math.Min(voltage, 1f);
					highlightScale = MathHelper.Lerp(1.5f, 2.5f, voltage);
					currentHighlightColor = Color.Red * voltage * pulseAmount;
				}
			}
			if (highlightScale > 0f)
			{
				foreach (Wire.WireSection section in this.sections)
				{
					section.Draw(spriteBatch, this.wireSprite, currentHighlightColor * alpha, drawOffset, 0f, this.Width * highlightScale);
				}
			}
			float signalDuration = (float)Timing.TotalTimeUnpaused - this.lastReceivedSignal.TimeSent;
			if (ConnectionPanel.ShouldDebugDrawWiring && signalDuration < 1f)
			{
				float offset = (this.item.ID % 2 == 1) ? 7.5f : 0f;
				float signalProgress = (float)(Timing.TotalTimeUnpaused * 100.0 + (double)offset) % 15f * (float)this.lastReceivedSignal.Direction;
				foreach (Wire.WireSection section2 in this.sections)
				{
					for (float x = 0f; x < section2.Length; x += 15f)
					{
						Vector2 dir = (section2.End - section2.Start) / section2.Length;
						float posOnSection = x + signalProgress;
						if (posOnSection >= 0f && posOnSection <= section2.Length)
						{
							Vector2 signalPos = section2.Start + drawOffset + dir * posOnSection;
							float a = 1f - Vector2.Distance(Screen.Selected.Cam.WorldViewCenter, signalPos) / 500f;
							if (a >= 0f)
							{
								signalPos.Y = -signalPos.Y;
								GUI.DrawRectangle(spriteBatch, signalPos - Vector2.One * 2.5f, Vector2.One * 5f, this.lastReceivedSignal.Color * a * (1f - signalDuration) * alpha, true, 0f, 1f);
							}
						}
					}
				}
			}
		}

		// Token: 0x0600618B RID: 24971 RVA: 0x0032BC4C File Offset: 0x00329E4C
		private Vector2 GetDrawOffset()
		{
			Submarine sub = this.item.Submarine;
			if (this.IsActive && sub == null)
			{
				if (this.connections[0] != null && this.connections[0].Item.Submarine != null)
				{
					sub = this.connections[0].Item.Submarine;
				}
				if (this.connections[1] != null && this.connections[1].Item.Submarine != null)
				{
					sub = this.connections[1].Item.Submarine;
				}
			}
			if (sub == null)
			{
				return Vector2.Zero;
			}
			return sub.DrawPosition + sub.HiddenSubPosition;
		}

		// Token: 0x0600618C RID: 24972 RVA: 0x0032BCEC File Offset: 0x00329EEC
		private void DrawHangingWire(SpriteBatch spriteBatch, Vector2 start, float depth)
		{
			float angle = (float)Math.Sin(GameMain.GameScreen.GameTime * 2.0 + (double)this.item.ID) * 0.2f;
			Vector2 endPos = start + new Vector2((float)Math.Sin((double)angle), -(float)Math.Cos((double)angle)) * 50f;
			Wire.WireSection.Draw(spriteBatch, this.wireSprite, start, endPos, GUIStyle.Orange, depth + 1E-05f, 0.2f);
			Wire.WireSection.Draw(spriteBatch, this.wireSprite, start, start + (endPos - start) * 0.7f, this.item.Color, depth, 0.3f);
		}

		// Token: 0x0600618D RID: 24973 RVA: 0x0032BDA8 File Offset: 0x00329FA8
		public static void UpdateEditing(List<Wire> wires)
		{
			bool doubleClicked = PlayerInput.DoubleClicked();
			Item item2 = Character.Controlled.HeldItems.FirstOrDefault((Item it) => it.GetComponent<Wire>() != null);
			Wire equippedWire = (item2 != null) ? item2.GetComponent<Wire>() : null;
			if (equippedWire != null && GUI.MouseOn == null)
			{
				if (PlayerInput.PrimaryMouseButtonClicked() && Character.Controlled.SelectedItem == null)
				{
					equippedWire.Use(1f, Character.Controlled);
				}
				return;
			}
			if (Wire.draggingWire == null || doubleClicked)
			{
				bool updateHighlight = true;
				float nodeSelectDist = 10f;
				float sectionSelectDist = 5f;
				Wire.highlightedNodeIndex = null;
				if (MapEntity.SelectedList.Count == 1)
				{
					Item selectedItem = MapEntity.SelectedList.FirstOrDefault<MapEntity>() as Item;
					if (selectedItem != null)
					{
						Wire selectedWire = selectedItem.GetComponent<Wire>();
						if (selectedWire != null)
						{
							Vector2 mousePos = GameMain.SubEditorScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
							if (selectedWire.item.Submarine != null)
							{
								mousePos -= selectedWire.item.Submarine.Position + selectedWire.item.Submarine.HiddenSubPosition;
							}
							if (PlayerInput.KeyDown(Keys.RightControl) || PlayerInput.KeyDown(Keys.LeftControl))
							{
								if (PlayerInput.PrimaryMouseButtonClicked())
								{
									if (Character.Controlled != null)
									{
										Character.Controlled.DisableInteract = true;
										Character.Controlled.ClearInputs();
									}
									float num;
									int closestSectionIndex = selectedWire.GetClosestSectionIndex(mousePos, sectionSelectDist, out num);
									if (closestSectionIndex > -1)
									{
										selectedWire.nodes.Insert(closestSectionIndex + 1, mousePos);
										selectedWire.UpdateSections();
									}
								}
							}
							else
							{
								float num;
								int closestIndex = selectedWire.GetClosestNodeIndex(mousePos, nodeSelectDist, out num);
								if (closestIndex > -1)
								{
									Wire.highlightedNodeIndex = new int?(closestIndex);
									Vector2 nudge = MapEntity.GetNudgeAmount(false);
									if (nudge != Vector2.Zero && closestIndex < selectedWire.nodes.Count)
									{
										selectedWire.MoveNode(closestIndex, nudge);
									}
									if (PlayerInput.PrimaryMouseButtonHeld())
									{
										if (Character.Controlled != null)
										{
											Character.Controlled.DisableInteract = true;
											Character.Controlled.ClearInputs();
										}
										Wire.draggingWire = selectedWire;
										return;
									}
									if (PlayerInput.SecondaryMouseButtonClicked() && closestIndex > 0 && closestIndex < selectedWire.nodes.Count - 1)
									{
										selectedWire.nodes.RemoveAt(closestIndex);
										selectedWire.UpdateSections();
									}
									else if (doubleClicked && equippedWire == null && Character.Controlled != null)
									{
										if (selectedWire.connections.Any((Connection conn) => conn != null) && ((selectedWire.connections[0] == null && closestIndex == 0) || (selectedWire.connections[1] == null && closestIndex == selectedWire.nodes.Count - 1)))
										{
											selectedWire.IsActive = true;
											selectedWire.nodes.RemoveAt(closestIndex);
											selectedWire.UpdateSections();
											if (closestIndex == 0)
											{
												selectedWire.nodes.Reverse();
												selectedWire.connections[0] = selectedWire.connections[1];
												selectedWire.connections[1] = null;
											}
											selectedWire.shouldClearConnections = false;
											Character.Controlled.Inventory.TryPutItem(selectedWire.item, Character.Controlled, new List<InvSlotType>
											{
												InvSlotType.LeftHand,
												InvSlotType.RightHand
											}, true, false, true);
											foreach (MapEntity entity in MapEntity.MapEntityList)
											{
												Item item = entity as Item;
												if (item != null)
												{
													ConnectionPanel component = item.GetComponent<ConnectionPanel>();
													if (component != null)
													{
														component.DisconnectedWires.Remove(selectedWire);
													}
												}
											}
											MapEntity.SelectedList.Clear();
											selectedWire.shouldClearConnections = true;
											updateHighlight = false;
										}
									}
								}
							}
						}
					}
				}
				Wire highlighted = null;
				if (GUI.MouseOn == null)
				{
					float closestDist = float.PositiveInfinity;
					foreach (Wire w in wires)
					{
						Vector2 mousePos2 = GameMain.SubEditorScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
						if (w.item.Submarine != null)
						{
							mousePos2 -= w.item.Submarine.Position + w.item.Submarine.HiddenSubPosition;
						}
						float dist;
						int highlightedNode = w.GetClosestNodeIndex(mousePos2, (highlighted == null) ? nodeSelectDist : closestDist, out dist);
						if (highlightedNode > -1 && dist < closestDist)
						{
							Wire.highlightedNodeIndex = new int?(highlightedNode);
							highlighted = w;
							closestDist = dist;
						}
						if (w.GetClosestSectionIndex(mousePos2, (highlighted == null) ? sectionSelectDist : closestDist, out dist) > -1 && dist + nodeSelectDist * 0.5f < closestDist)
						{
							Wire.highlightedNodeIndex = null;
							highlighted = w;
							closestDist = dist + nodeSelectDist * 0.5f;
						}
					}
				}
				if (highlighted != null && updateHighlight)
				{
					highlighted.item.IsHighlighted = true;
					if (PlayerInput.PrimaryMouseButtonClicked())
					{
						MapEntity.DisableSelect = true;
						MapEntity.SelectEntity(highlighted.item);
					}
				}
				return;
			}
			if (Character.Controlled != null)
			{
				Character.Controlled.FocusedItem = null;
				Character.Controlled.DisableInteract = true;
				Character.Controlled.ClearInputs();
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				Wire.draggingWire = null;
				Wire.selectedNodeIndex = null;
				return;
			}
			MapEntity.DisableSelect = true;
			Submarine sub = Wire.draggingWire.item.Submarine;
			if (Wire.draggingWire.connections[0] != null && Wire.draggingWire.connections[0].Item.Submarine != null)
			{
				sub = Wire.draggingWire.connections[0].Item.Submarine;
			}
			if (Wire.draggingWire.connections[1] != null && Wire.draggingWire.connections[1].Item.Submarine != null)
			{
				sub = Wire.draggingWire.connections[1].Item.Submarine;
			}
			Vector2 nodeWorldPos = GameMain.SubEditorScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
			if (sub != null)
			{
				nodeWorldPos = nodeWorldPos - sub.HiddenSubPosition - sub.Position;
			}
			if (Wire.selectedNodeIndex != null && Wire.selectedNodeIndex.Value >= Wire.draggingWire.nodes.Count)
			{
				Wire.selectedNodeIndex = null;
			}
			if (Wire.highlightedNodeIndex != null && Wire.highlightedNodeIndex.Value >= Wire.draggingWire.nodes.Count)
			{
				Wire.highlightedNodeIndex = null;
			}
			if (Wire.selectedNodeIndex != null)
			{
				if (!PlayerInput.IsShiftDown())
				{
					nodeWorldPos.X = MathUtils.Round(nodeWorldPos.X, Submarine.GridSize.X / 2f);
					nodeWorldPos.Y = MathUtils.Round(nodeWorldPos.Y, Submarine.GridSize.Y / 2f);
				}
				Wire.draggingWire.nodes[Wire.selectedNodeIndex.Value] = nodeWorldPos;
				Wire.draggingWire.UpdateSections();
			}
			else
			{
				float dragDistance = Submarine.GridSize.X * Submarine.GridSize.Y;
				dragDistance *= 0.5f;
				if ((Wire.highlightedNodeIndex != null && Vector2.DistanceSquared(nodeWorldPos, Wire.draggingWire.nodes[Wire.highlightedNodeIndex.Value]) >= dragDistance) || PlayerInput.IsShiftDown())
				{
					Wire.selectedNodeIndex = Wire.highlightedNodeIndex;
				}
			}
			MapEntity.SelectEntity(Wire.draggingWire.item);
		}

		// Token: 0x0600618E RID: 24974 RVA: 0x0032C524 File Offset: 0x0032A724
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (!this.item.IsSelected)
			{
				return;
			}
			Vector2 wireNodeOffset = (this.item.Submarine == null) ? Vector2.Zero : (this.item.Submarine.HiddenSubPosition + amount);
			int i = 0;
			while (i < this.nodes.Count)
			{
				if (i != 0 && i != this.nodes.Count - 1)
				{
					goto IL_16C;
				}
				Connection connection = this.connections[0];
				if (((connection != null) ? connection.Item : null) == null || this.connections[0].Item.IsSelected || (!Submarine.RectContains(this.connections[0].Item.Rect, this.nodes[i] + wireNodeOffset, false) && !Submarine.RectContains(this.connections[0].Item.Rect, this.nodes[i] + wireNodeOffset - amount, false)))
				{
					Connection connection2 = this.connections[1];
					if (((connection2 != null) ? connection2.Item : null) == null || this.connections[1].Item.IsSelected || (!Submarine.RectContains(this.connections[1].Item.Rect, this.nodes[i] + wireNodeOffset, false) && !Submarine.RectContains(this.connections[1].Item.Rect, this.nodes[i] + wireNodeOffset - amount, false)))
					{
						goto IL_16C;
					}
				}
				IL_189:
				i++;
				continue;
				IL_16C:
				List<Vector2> list = this.nodes;
				int index = i;
				list[index] += amount;
				goto IL_189;
			}
			this.UpdateSections();
		}

		// Token: 0x0600618F RID: 24975 RVA: 0x0032C6D8 File Offset: 0x0032A8D8
		public bool IsMouseOn()
		{
			if (GUI.MouseOn == null)
			{
				Vector2 mousePos = GameMain.SubEditorScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
				if (this.item.Submarine != null)
				{
					mousePos -= this.item.Submarine.Position + this.item.Submarine.HiddenSubPosition;
				}
				float num;
				if (this.GetClosestNodeIndex(mousePos, 10f, out num) > -1)
				{
					return true;
				}
				if (this.GetClosestSectionIndex(mousePos, 10f, out num) > -1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06006190 RID: 24976 RVA: 0x0032C764 File Offset: 0x0032A964
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int eventIndex = msg.ReadRangedInteger(0, (int)Math.Ceiling(8.5));
			int nodeCount = msg.ReadRangedInteger(0, 30);
			int nodeStartIndex = eventIndex * 30;
			Vector2[] nodePositions = new Vector2[nodeStartIndex + nodeCount];
			int i = 0;
			while (i < this.nodes.Count && i < nodePositions.Length)
			{
				nodePositions[i] = this.nodes[i];
				i++;
			}
			for (int j = 0; j < nodeCount; j++)
			{
				nodePositions[nodeStartIndex + j] = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			}
			if (nodePositions.Any((Vector2 n) => !MathUtils.IsValid(n)))
			{
				this.nodes.Clear();
				return;
			}
			this.nodes = nodePositions.ToList<Vector2>();
			this.UpdateSections();
			base.Drawable = this.nodes.Any<Vector2>();
			bool isActive;
			if (this.connections[0] == null ^ this.connections[1] == null)
			{
				CharacterInventory characterInventory = this.item.ParentInventory as CharacterInventory;
				if (characterInventory != null)
				{
					Character character = characterInventory.Owner as Character;
					isActive = (character != null && character.HasEquippedItem(this.item, null, null));
				}
				else
				{
					isActive = false;
				}
			}
			else
			{
				isActive = false;
			}
			this.IsActive = isActive;
		}

		// Token: 0x06006191 RID: 24977 RVA: 0x0032C8BC File Offset: 0x0032AABC
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			Wire.ClientEventData clientEventData;
			return base.TryExtractEventData<Wire.ClientEventData>(data, out clientEventData);
		}

		// Token: 0x06006192 RID: 24978 RVA: 0x0032C8D4 File Offset: 0x0032AAD4
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			Wire.ClientEventData eventData = base.ExtractEventData<Wire.ClientEventData>(extraData);
			int nodeCount = eventData.NodeCount;
			msg.WriteByte((byte)nodeCount);
			if (nodeCount > 0)
			{
				msg.WriteSingle(this.nodes.Last<Vector2>().X);
				msg.WriteSingle(this.nodes.Last<Vector2>().Y);
			}
		}

		// Token: 0x1700189E RID: 6302
		// (get) Token: 0x06006193 RID: 24979 RVA: 0x0032C928 File Offset: 0x0032AB28
		// (set) Token: 0x06006194 RID: 24980 RVA: 0x0032C98F File Offset: 0x0032AB8F
		public bool Locked
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null && !GameMain.NetworkMember.ServerSettings.AllowRewiring)
				{
					return false;
				}
				if (!this.locked)
				{
					return this.connections.Any((Connection c) => c != null && (c.ConnectionPanel.Locked || c.ConnectionPanel.TemporarilyLocked));
				}
				return true;
			}
			set
			{
				this.locked = value;
			}
		}

		// Token: 0x1700189F RID: 6303
		// (get) Token: 0x06006195 RID: 24981 RVA: 0x0032C998 File Offset: 0x0032AB98
		public Connection[] Connections
		{
			get
			{
				return this.connections;
			}
		}

		// Token: 0x170018A0 RID: 6304
		// (get) Token: 0x06006196 RID: 24982 RVA: 0x0032C9A0 File Offset: 0x0032ABA0
		// (set) Token: 0x06006197 RID: 24983 RVA: 0x0032C9A8 File Offset: 0x0032ABA8
		public float Length { get; private set; }

		// Token: 0x170018A1 RID: 6305
		// (get) Token: 0x06006198 RID: 24984 RVA: 0x0032C9B1 File Offset: 0x0032ABB1
		// (set) Token: 0x06006199 RID: 24985 RVA: 0x0032C9B9 File Offset: 0x0032ABB9
		[Serialize(0.3f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float Width { get; set; }

		// Token: 0x170018A2 RID: 6306
		// (get) Token: 0x0600619A RID: 24986 RVA: 0x0032C9C2 File Offset: 0x0032ABC2
		// (set) Token: 0x0600619B RID: 24987 RVA: 0x0032C9CA File Offset: 0x0032ABCA
		[Serialize(5000f, IsPropertySaveable.No, "The maximum distance the wire can extend (in pixels).", "", false)]
		public float MaxLength { get; set; }

		// Token: 0x170018A3 RID: 6307
		// (get) Token: 0x0600619C RID: 24988 RVA: 0x0032C9D3 File Offset: 0x0032ABD3
		// (set) Token: 0x0600619D RID: 24989 RVA: 0x0032C9DB File Offset: 0x0032ABDB
		[Serialize(false, IsPropertySaveable.No, "If enabled, the wire will not be visible in connection panels outside the submarine editor.", "", false)]
		public bool HiddenInGame { get; set; }

		// Token: 0x170018A4 RID: 6308
		// (get) Token: 0x0600619E RID: 24990 RVA: 0x0032C9E4 File Offset: 0x0032ABE4
		// (set) Token: 0x0600619F RID: 24991 RVA: 0x0032C9EC File Offset: 0x0032ABEC
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, this wire will be ignored by the \"Lock all default wires\" setting.", "", true)]
		public bool NoAutoLock { get; set; }

		// Token: 0x170018A5 RID: 6309
		// (get) Token: 0x060061A0 RID: 24992 RVA: 0x0032C9F5 File Offset: 0x0032ABF5
		// (set) Token: 0x060061A1 RID: 24993 RVA: 0x0032C9FD File Offset: 0x0032ABFD
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, this wire will use the sprite depth instead of a constant depth.", "", false)]
		public bool UseSpriteDepth { get; set; }

		// Token: 0x170018A6 RID: 6310
		// (get) Token: 0x060061A2 RID: 24994 RVA: 0x0032CA06 File Offset: 0x0032AC06
		// (set) Token: 0x060061A3 RID: 24995 RVA: 0x0032CA0E File Offset: 0x0032AC0E
		[Serialize(true, IsPropertySaveable.Yes, "If disabled, the wire will not be dropped when connecting. Used in circuit box to store the wires inside the box.", "", false)]
		public bool DropOnConnect { get; set; }

		// Token: 0x060061A4 RID: 24996 RVA: 0x0032CA18 File Offset: 0x0032AC18
		public Wire(Item item, ContentXElement element) : base(item, element)
		{
			this.nodes = new List<Vector2>();
			this.sections = new List<Wire.WireSection>();
			this.connections = new Connection[2];
			this.IsActive = false;
			item.IsShootable = true;
			this.InitProjSpecific(element);
		}

		// Token: 0x060061A5 RID: 24997 RVA: 0x0032CA6B File Offset: 0x0032AC6B
		private void InitProjSpecific(ContentXElement element)
		{
			this.wireSprite = Wire.ExtractWireSprite(element);
			if (this.wireSprite != Wire.defaultWireSprite)
			{
				this.overrideSprite = this.wireSprite;
			}
		}

		// Token: 0x060061A6 RID: 24998 RVA: 0x0032CA92 File Offset: 0x0032AC92
		public Connection OtherConnection(Connection connection)
		{
			if (connection == this.connections[0])
			{
				return this.connections[1];
			}
			if (connection == this.connections[1])
			{
				return this.connections[0];
			}
			return null;
		}

		// Token: 0x060061A7 RID: 24999 RVA: 0x0032CABD File Offset: 0x0032ACBD
		public bool IsConnectedTo(Item item)
		{
			return (this.connections[0] != null && this.connections[0].Item == item) || (this.connections[1] != null && this.connections[1].Item == item);
		}

		// Token: 0x060061A8 RID: 25000 RVA: 0x0032CAF8 File Offset: 0x0032ACF8
		public void RemoveConnection(Item item)
		{
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] != null && this.connections[i].Item == item)
				{
					if (this.connections[i].Wires.Contains(this))
					{
						this.SetConnectedDirty();
						this.connections[i].DisconnectWire(this);
					}
					this.connections[i] = null;
				}
			}
		}

		// Token: 0x060061A9 RID: 25001 RVA: 0x0032CB5D File Offset: 0x0032AD5D
		public void RemoveConnection(Connection connection)
		{
			if (connection == this.connections[0])
			{
				this.connections[0] = null;
			}
			if (connection == this.connections[1])
			{
				this.connections[1] = null;
			}
			this.SetConnectedDirty();
		}

		// Token: 0x060061AA RID: 25002 RVA: 0x0032CB8D File Offset: 0x0032AD8D
		public bool TryConnect(Connection newConnection, bool addNode = true, bool sendNetworkEvent = false)
		{
			if (this.connections[0] == null)
			{
				return this.Connect(newConnection, 0, addNode, sendNetworkEvent);
			}
			return this.connections[1] == null && this.Connect(newConnection, 1, addNode, sendNetworkEvent);
		}

		// Token: 0x060061AB RID: 25003 RVA: 0x0032CBBC File Offset: 0x0032ADBC
		public bool Connect(Connection newConnection, int connectionIndex, bool addNode = true, bool sendNetworkEvent = false)
		{
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] == newConnection)
				{
					return false;
				}
			}
			if (connectionIndex < 0 || connectionIndex > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error while connecting a wire to ");
				defaultInterpolatedStringHandler.AppendFormatted<Item>(newConnection.Item);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(connectionIndex);
				defaultInterpolatedStringHandler.AppendLiteral(" is not a valid index.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return false;
			}
			if (this.connections[connectionIndex] != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(77, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error while connecting a wire to ");
				defaultInterpolatedStringHandler2.AppendFormatted<Item>(newConnection.Item);
				defaultInterpolatedStringHandler2.AppendLiteral(": a wire is already connected to the index ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(connectionIndex);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return false;
			}
			for (int j = 0; j < 2; j++)
			{
				if (this.connections[j] != null && this.connections[j].Item == newConnection.Item)
				{
					addNode = false;
					break;
				}
			}
			if (this.item.body != null)
			{
				this.item.Submarine = newConnection.Item.Submarine;
			}
			newConnection.ConnectionPanel.DisconnectedWires.Remove(this);
			this.connections[connectionIndex] = newConnection;
			this.FixNodeEnds();
			if (addNode)
			{
				this.AddNode(newConnection, connectionIndex);
			}
			this.SetConnectedDirty();
			if (this.DropOnConnect && this.connections[0] != null && this.connections[1] != null)
			{
				foreach (ItemComponent ic in this.item.Components)
				{
					if (ic != this)
					{
						ic.Drop(null, true);
					}
				}
				Item container = this.item.Container;
				if (container != null)
				{
					container.RemoveContained(this.item);
				}
				if (this.item.body != null)
				{
					this.item.body.Enabled = false;
				}
				this.IsActive = false;
				this.CleanNodes();
			}
			if (this.item.body != null)
			{
				this.item.Submarine = newConnection.Item.Submarine;
			}
			if (sendNetworkEvent)
			{
				this.IsActive = (this.item.ParentInventory is CharacterInventory && (this.connections[0] == null ^ this.connections[1] == null));
			}
			base.Drawable = (this.IsActive || this.nodes.Any<Vector2>());
			this.UpdateSections();
			return true;
		}

		// Token: 0x060061AC RID: 25004 RVA: 0x0032CE60 File Offset: 0x0032B060
		private void AddNode(Connection newConnection, int selectedIndex)
		{
			Submarine refSub = newConnection.Item.Submarine;
			if (refSub == null)
			{
				Structure attachTarget = Structure.GetAttachTarget(newConnection.Item.WorldPosition);
				if (attachTarget == null)
				{
					Holdable component = newConnection.Item.GetComponent<Holdable>();
					if (component == null || !component.Attached)
					{
						this.connections[selectedIndex] = null;
						return;
					}
				}
				refSub = ((attachTarget != null) ? attachTarget.Submarine : null);
			}
			Vector2 nodePos = Wire.RoundNode(newConnection.Item.Position);
			if (refSub != null)
			{
				nodePos -= refSub.HiddenSubPosition;
			}
			if (this.nodes.Count > 0 && this.nodes[0] == nodePos)
			{
				return;
			}
			if (this.nodes.Count > 1 && this.nodes[this.nodes.Count - 1] == nodePos)
			{
				return;
			}
			int newNodeIndex = 0;
			if (this.nodes.Count > 1)
			{
				if (this.connections[0] != null && this.connections[0] != newConnection)
				{
					if (Vector2.DistanceSquared(this.nodes[0], this.connections[0].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)) < Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], this.connections[0].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)))
					{
						newNodeIndex = this.nodes.Count;
					}
				}
				else if (this.connections[1] != null && this.connections[1] != newConnection)
				{
					if (Vector2.DistanceSquared(this.nodes[0], this.connections[1].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)) < Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], this.connections[1].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)))
					{
						newNodeIndex = this.nodes.Count;
					}
				}
				else if (Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], nodePos) < Vector2.DistanceSquared(this.nodes[0], nodePos))
				{
					newNodeIndex = this.nodes.Count;
				}
			}
			if (newNodeIndex == 0 && this.nodes.Count > 1)
			{
				this.nodes.Insert(0, nodePos);
				return;
			}
			this.nodes.Add(nodePos);
		}

		// Token: 0x060061AD RID: 25005 RVA: 0x0032D103 File Offset: 0x0032B303
		public override void Equip(Character character)
		{
			if (this.shouldClearConnections)
			{
				this.ClearConnections(character);
			}
			this.IsActive = true;
		}

		// Token: 0x060061AE RID: 25006 RVA: 0x0032D11B File Offset: 0x0032B31B
		public override void Unequip(Character character)
		{
			this.ClearConnections(character);
			this.IsActive = false;
		}

		// Token: 0x060061AF RID: 25007 RVA: 0x0032D12B File Offset: 0x0032B32B
		public override void Drop(Character dropper, bool setTransform = true)
		{
			if (this.shouldClearConnections)
			{
				this.ClearConnections(dropper);
			}
			this.IsActive = false;
		}

		// Token: 0x060061B0 RID: 25008 RVA: 0x0032D144 File Offset: 0x0032B344
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.nodes.Count == 0)
			{
				return;
			}
			Inventory parentInventory = this.item.ParentInventory;
			Character user = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
			this.editNodeDelay = ((((user != null) ? user.SelectedItem : null) == null) ? (this.editNodeDelay - deltaTime) : 0.5f);
			Submarine sub = this.item.Submarine;
			if (this.connections[0] != null && this.connections[0].Item.Submarine != null)
			{
				sub = this.connections[0].Item.Submarine;
			}
			if (this.connections[1] != null && this.connections[1].Item.Submarine != null)
			{
				sub = this.connections[1].Item.Submarine;
			}
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				if (user != null)
				{
					this.NoAutoLock = true;
				}
				if (this.item.Submarine != sub && sub != null && this.item.Submarine != null)
				{
					this.ClearConnections(null);
					return;
				}
				if (this.item.CurrentHull == null)
				{
					Structure attachTarget = Structure.GetAttachTarget(this.item.WorldPosition);
					this.canPlaceNode = (attachTarget != null);
					if (sub == null)
					{
						sub = ((attachTarget != null) ? attachTarget.Submarine : null);
					}
					Vector2 attachPos = this.GetAttachPosition(user);
					this.newNodePos = ((sub == null) ? attachPos : (attachPos - sub.Position - sub.HiddenSubPosition));
				}
				else
				{
					this.newNodePos = this.GetAttachPosition(user);
					if (sub != null)
					{
						this.newNodePos -= sub.HiddenSubPosition;
					}
					this.canPlaceNode = true;
				}
				if (this.nodes.Count > 0)
				{
					if (user == null)
					{
						return;
					}
					Vector2 prevNodePos = this.nodes[this.nodes.Count - 1];
					if (sub != null)
					{
						prevNodePos += sub.HiddenSubPosition;
					}
					this.currLength = 0f;
					for (int i = 0; i < this.nodes.Count - 1; i++)
					{
						this.currLength += Vector2.Distance(this.nodes[i], this.nodes[i + 1]);
					}
					Vector2 itemPos = this.item.Position;
					if (sub != null && user.Submarine == null)
					{
						prevNodePos += sub.Position;
					}
					this.currLength += Vector2.Distance(prevNodePos, itemPos);
					if (this.currLength > this.MaxLength)
					{
						Vector2 diff = prevNodePos - user.Position;
						Vector2 pullBackDir = (diff == Vector2.Zero) ? Vector2.Zero : Vector2.Normalize(diff);
						Vector2 forceDir = pullBackDir;
						if (!user.AnimController.InWater)
						{
							forceDir.Y = 0f;
						}
						user.AnimController.Collider.ApplyForce(forceDir * user.Mass * 50f, 32f);
						if (diff.LengthSquared() > 2500f)
						{
							user.AnimController.UpdateUseItem(!user.IsClimbing, user.WorldPosition + pullBackDir * Math.Min(150f, diff.Length()));
						}
						if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && this.currLength > this.MaxLength * 1.5f)
						{
							this.ClearConnections(null);
							return;
						}
					}
				}
			}
			else
			{
				this.newNodePos = ((SubEditorScreen.IsSubEditor() && PlayerInput.IsShiftDown()) ? this.item.Position : Wire.RoundNode(this.item.Position));
				if (sub != null)
				{
					this.newNodePos -= sub.HiddenSubPosition;
				}
				this.canPlaceNode = true;
			}
			if (this.item != null)
			{
				Vector2 relativeNodePos = this.newNodePos - this.item.Position;
				if (sub != null)
				{
					relativeNodePos += sub.HiddenSubPosition;
				}
				this.sectionExtents = new Vector2(Math.Max(Math.Abs(relativeNodePos.X), this.sectionExtents.X), Math.Max(Math.Abs(relativeNodePos.Y), this.sectionExtents.Y));
			}
		}

		// Token: 0x060061B1 RID: 25009 RVA: 0x0032D57C File Offset: 0x0032B77C
		private Vector2 GetAttachPosition(Character user)
		{
			if (user == null)
			{
				return this.item.Position;
			}
			Vector2 mouseDiff = user.CursorWorldPosition - user.WorldPosition;
			mouseDiff = mouseDiff.ClampLength(150f);
			return Wire.RoundNode(user.Position + mouseDiff);
		}

		// Token: 0x060061B2 RID: 25010 RVA: 0x0032D5C8 File Offset: 0x0032B7C8
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character == null || character != Character.Controlled)
			{
				return false;
			}
			if (character.HasSelectedAnyItem)
			{
				return false;
			}
			if (Screen.Selected == GameMain.SubEditorScreen && !PlayerInput.PrimaryMouseButtonClicked())
			{
				return false;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				return false;
			}
			if (this.newNodePos != Vector2.Zero && this.canPlaceNode && this.editNodeDelay <= 0f && this.nodes.Count > 0 && Vector2.DistanceSquared(this.newNodePos, this.nodes[this.nodes.Count - 1]) > 49f)
			{
				if (this.nodes.Count >= 255)
				{
					this.nodes.RemoveAt(this.nodes.Count - 1);
				}
				this.nodes.Add(this.newNodePos);
				this.CleanNodes();
				this.UpdateSections();
				base.Drawable = true;
				this.newNodePos = Vector2.Zero;
				if (GameMain.NetworkMember != null)
				{
					this.item.CreateClientEvent<Wire>(this, new Wire.ClientEventData(this.nodes.Count));
				}
			}
			this.editNodeDelay = 0.1f;
			return true;
		}

		// Token: 0x060061B3 RID: 25011 RVA: 0x0032D714 File Offset: 0x0032B914
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			if (character == null || character != Character.Controlled)
			{
				return false;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				return false;
			}
			if (this.nodes.Count > 1 && this.editNodeDelay <= 0f)
			{
				this.nodes.RemoveAt(this.nodes.Count - 1);
				this.UpdateSections();
				if (GameMain.NetworkMember != null)
				{
					this.item.CreateClientEvent<Wire>(this, new Wire.ClientEventData(this.nodes.Count));
				}
			}
			this.editNodeDelay = 0.1f;
			base.Drawable = (this.IsActive || this.sections.Count > 0);
			return true;
		}

		// Token: 0x060061B4 RID: 25012 RVA: 0x0032D7CF File Offset: 0x0032B9CF
		public override bool Pick(Character picker)
		{
			this.ClearConnections(picker);
			return true;
		}

		// Token: 0x060061B5 RID: 25013 RVA: 0x0032D7D9 File Offset: 0x0032B9D9
		public List<Vector2> GetNodes()
		{
			return new List<Vector2>(this.nodes);
		}

		// Token: 0x060061B6 RID: 25014 RVA: 0x0032D7E6 File Offset: 0x0032B9E6
		public void SetNodes(IEnumerable<Vector2> nodes)
		{
			this.nodes = nodes.ToList<Vector2>();
			this.UpdateSections();
		}

		// Token: 0x060061B7 RID: 25015 RVA: 0x0032D7FC File Offset: 0x0032B9FC
		public void MoveNode(int index, Vector2 amount)
		{
			if (index < 0 || index >= this.nodes.Count)
			{
				return;
			}
			List<Vector2> list = this.nodes;
			list[index] += amount;
			this.UpdateSections();
		}

		// Token: 0x060061B8 RID: 25016 RVA: 0x0032D840 File Offset: 0x0032BA40
		public void MoveNodes(Vector2 amount)
		{
			for (int i = 0; i < this.nodes.Count; i++)
			{
				List<Vector2> list = this.nodes;
				int index = i;
				list[index] += amount;
			}
			this.UpdateSections();
		}

		// Token: 0x060061B9 RID: 25017 RVA: 0x0032D888 File Offset: 0x0032BA88
		public void UpdateSections()
		{
			this.sections.Clear();
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				this.sections.Add(new Wire.WireSection(this.nodes[i], this.nodes[i + 1]));
			}
			base.Drawable = (this.IsActive || this.sections.Count > 0);
			float length;
			if (this.sections.Count <= 0)
			{
				length = 0f;
			}
			else
			{
				length = this.sections.Sum((Wire.WireSection s) => s.Length);
			}
			this.Length = length;
			this.CalculateExtents();
		}

		// Token: 0x060061BA RID: 25018 RVA: 0x0032D94C File Offset: 0x0032BB4C
		private void CalculateExtents()
		{
			this.sectionExtents = Vector2.Zero;
			if (this.sections.Count > 0)
			{
				for (int i = 0; i < this.nodes.Count; i++)
				{
					this.sectionExtents.X = Math.Max(Math.Abs(this.nodes[i].X - this.item.Position.X), this.sectionExtents.X);
					this.sectionExtents.Y = Math.Max(Math.Abs(this.nodes[i].Y - this.item.Position.Y), this.sectionExtents.Y);
				}
			}
			this.item.ResetCachedVisibleSize();
		}

		// Token: 0x060061BB RID: 25019 RVA: 0x0032DA20 File Offset: 0x0032BC20
		public void ClearConnections(Character user = null)
		{
			this.nodes.Clear();
			this.sections.Clear();
			foreach (Item item in Item.ItemList)
			{
				ConnectionPanel connectionPanel = item.GetComponent<ConnectionPanel>();
				if (connectionPanel != null && connectionPanel.DisconnectedWires.Contains(this) && !item.Removed)
				{
					connectionPanel.DisconnectedWires.Remove(this);
				}
			}
			this.SetConnectedDirty();
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] != null)
				{
					Wire wire = this.connections[i].FindWireByItem(this.item);
					if (wire != null)
					{
						this.connections[i].DisconnectWire(wire);
						this.connections[i] = null;
					}
				}
			}
			base.Drawable = (this.sections.Count > 0);
		}

		// Token: 0x060061BC RID: 25020 RVA: 0x0032DB10 File Offset: 0x0032BD10
		private static Vector2 RoundNode(Vector2 position)
		{
			Vector2 halfGrid = Submarine.GridSize / 2f;
			position += halfGrid;
			position.X = MathUtils.RoundTowardsClosest(position.X, Submarine.GridSize.X / 2f);
			position.Y = MathUtils.RoundTowardsClosest(position.Y, Submarine.GridSize.Y / 2f);
			return position - halfGrid;
		}

		// Token: 0x060061BD RID: 25021 RVA: 0x0032DB84 File Offset: 0x0032BD84
		public void SetConnectedDirty()
		{
			for (int i = 0; i < 2; i++)
			{
				Connection connection = this.connections[i];
				if (((connection != null) ? connection.Item : null) != null)
				{
					PowerTransfer component = this.connections[i].Item.GetComponent<PowerTransfer>();
					if (component != null)
					{
						component.SetConnectionDirty(this.connections[i]);
					}
					this.connections[i].SetRecipientsDirty();
				}
			}
		}

		// Token: 0x060061BE RID: 25022 RVA: 0x0032DBE8 File Offset: 0x0032BDE8
		private void CleanNodes()
		{
			bool removed;
			do
			{
				removed = false;
				for (int i = this.nodes.Count - 2; i > 0; i--)
				{
					if (Math.Abs(this.nodes[i - 1].X - this.nodes[i].X) < 1f && Math.Abs(this.nodes[i + 1].X - this.nodes[i].X) < 1f && Math.Sign(this.nodes[i - 1].Y - this.nodes[i].Y) != Math.Sign(this.nodes[i + 1].Y - this.nodes[i].Y))
					{
						this.nodes.RemoveAt(i);
						removed = true;
					}
					else if (Math.Abs(this.nodes[i - 1].Y - this.nodes[i].Y) < 1f && Math.Abs(this.nodes[i + 1].Y - this.nodes[i].Y) < 1f && Math.Sign(this.nodes[i - 1].X - this.nodes[i].X) != Math.Sign(this.nodes[i + 1].X - this.nodes[i].X))
					{
						this.nodes.RemoveAt(i);
						removed = true;
					}
				}
			}
			while (removed);
		}

		// Token: 0x060061BF RID: 25023 RVA: 0x0032DDB4 File Offset: 0x0032BFB4
		public void FixNodeEnds()
		{
			Connection connection = this.connections[0];
			Item item0 = (connection != null) ? connection.Item : null;
			Connection connection2 = this.connections[1];
			Item item = (connection2 != null) ? connection2.Item : null;
			if (item0 == null && item != null)
			{
				item0 = Item.ItemList.Find(delegate(Item it)
				{
					ConnectionPanel component = it.GetComponent<ConnectionPanel>();
					return component != null && component.DisconnectedWires.Contains(this);
				});
			}
			else if (item0 != null && item == null)
			{
				item = Item.ItemList.Find(delegate(Item it)
				{
					ConnectionPanel component = it.GetComponent<ConnectionPanel>();
					return component != null && component.DisconnectedWires.Contains(this);
				});
			}
			if (item0 == null || item == null || this.nodes.Count == 0)
			{
				return;
			}
			Vector2 nodePos = this.nodes[0];
			Submarine refSub = item0.Submarine ?? item.Submarine;
			if (refSub != null)
			{
				nodePos += refSub.HiddenSubPosition;
			}
			float dist = Vector2.DistanceSquared(item0.Position, nodePos);
			float dist2 = Vector2.DistanceSquared(item.Position, nodePos);
			if (dist > dist2)
			{
				this.nodes.Reverse();
				this.UpdateSections();
			}
		}

		// Token: 0x060061C0 RID: 25024 RVA: 0x0032DE9C File Offset: 0x0032C09C
		private int GetClosestNodeIndex(Vector2 pos, float maxDist, out float closestDist)
		{
			closestDist = 0f;
			int closestIndex = -1;
			for (int i = 0; i < this.nodes.Count; i++)
			{
				float dist = Vector2.Distance(this.nodes[i], pos);
				if (dist <= maxDist && (closestIndex == -1 || dist < closestDist))
				{
					closestIndex = i;
					closestDist = dist;
				}
			}
			return closestIndex;
		}

		// Token: 0x060061C1 RID: 25025 RVA: 0x0032DEF0 File Offset: 0x0032C0F0
		private int GetClosestSectionIndex(Vector2 mousePos, float maxDist, out float closestDist)
		{
			closestDist = 0f;
			int closestIndex = -1;
			maxDist *= maxDist;
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				if ((Math.Abs(this.nodes[i].X - this.nodes[i + 1].X) < 5f || Math.Sign(mousePos.X - this.nodes[i].X) != Math.Sign(mousePos.X - this.nodes[i + 1].X)) && (Math.Abs(this.nodes[i].Y - this.nodes[i + 1].Y) < 5f || Math.Sign(mousePos.Y - this.nodes[i].Y) != Math.Sign(mousePos.Y - this.nodes[i + 1].Y)))
				{
					float dist = MathUtils.LineToPointDistanceSquared(this.nodes[i], this.nodes[i + 1], mousePos);
					if (dist <= maxDist && (closestIndex == -1 || dist < closestDist))
					{
						closestIndex = i;
						closestDist = dist;
					}
				}
			}
			closestDist = (float)Math.Sqrt((double)closestDist);
			return closestIndex;
		}

		// Token: 0x060061C2 RID: 25026 RVA: 0x0032E04C File Offset: 0x0032C24C
		public override void FlipX(bool relativeToSub)
		{
			if (this.item.ParentInventory != null)
			{
				return;
			}
			if (!relativeToSub)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					Submarine submarine = this.item.Submarine;
					if (submarine == null || !submarine.Loading)
					{
						goto IL_37;
					}
				}
				return;
			}
			IL_37:
			Vector2 refPos = (this.item.Submarine == null) ? Vector2.Zero : (this.item.Position - this.item.Submarine.HiddenSubPosition);
			for (int i = 0; i < this.nodes.Count; i++)
			{
				this.nodes[i] = (relativeToSub ? new Vector2(-this.nodes[i].X, this.nodes[i].Y) : new Vector2(refPos.X - (this.nodes[i].X - refPos.X), this.nodes[i].Y));
			}
			this.UpdateSections();
		}

		// Token: 0x060061C3 RID: 25027 RVA: 0x0032E150 File Offset: 0x0032C350
		public override void FlipY(bool relativeToSub)
		{
			Vector2 refPos = (this.item.Submarine == null) ? Vector2.Zero : (this.item.Position - this.item.Submarine.HiddenSubPosition);
			for (int i = 0; i < this.nodes.Count; i++)
			{
				this.nodes[i] = (relativeToSub ? new Vector2(this.nodes[i].X, -this.nodes[i].Y) : new Vector2(this.nodes[i].X, refPos.Y - (this.nodes[i].Y - refPos.Y)));
			}
			this.UpdateSections();
		}

		// Token: 0x060061C4 RID: 25028 RVA: 0x0032E21C File Offset: 0x0032C41C
		public static IEnumerable<Vector2> ExtractNodes(XElement element)
		{
			Wire.<ExtractNodes>d__112 <ExtractNodes>d__ = new Wire.<ExtractNodes>d__112(-2);
			<ExtractNodes>d__.<>3__element = element;
			return <ExtractNodes>d__;
		}

		// Token: 0x060061C5 RID: 25029 RVA: 0x0032E22C File Offset: 0x0032C42C
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.nodes.AddRange(Wire.ExtractNodes(componentElement));
			base.Drawable = this.nodes.Any<Vector2>();
		}

		// Token: 0x060061C6 RID: 25030 RVA: 0x0032E260 File Offset: 0x0032C460
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			if (this.nodes == null || this.nodes.Count == 0)
			{
				return componentElement;
			}
			string[] nodeCoords = new string[this.nodes.Count * 2];
			for (int i = 0; i < this.nodes.Count; i++)
			{
				string[] array = nodeCoords;
				int num = i * 2;
				Vector2 vector = this.nodes[i];
				array[num] = vector.X.ToString(CultureInfo.InvariantCulture);
				string[] array2 = nodeCoords;
				int num2 = i * 2 + 1;
				vector = this.nodes[i];
				array2[num2] = vector.Y.ToString(CultureInfo.InvariantCulture);
			}
			componentElement.Add(new XAttribute("nodes", string.Join(";", nodeCoords)));
			return componentElement;
		}

		// Token: 0x060061C7 RID: 25031 RVA: 0x0032E31E File Offset: 0x0032C51E
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x060061C8 RID: 25032 RVA: 0x0032E320 File Offset: 0x0032C520
		protected override void RemoveComponentSpecific()
		{
			Item container = this.item.Container;
			CircuitBox circuitBox = (container != null) ? container.GetComponent<CircuitBox>() : null;
			if (circuitBox != null)
			{
				circuitBox.RemoveWire(this);
			}
			this.ClearConnections(null);
			base.RemoveComponentSpecific();
			if (Wire.DraggingWire == this)
			{
				Wire.draggingWire = null;
			}
			Sprite sprite = this.overrideSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.overrideSprite = null;
			this.wireSprite = null;
		}

		// Token: 0x060061CA RID: 25034 RVA: 0x0032E40C File Offset: 0x0032C60C
		[CompilerGenerated]
		private float <DebugDraw>g__GetVoltage|23_0(int connectionIndex)
		{
			Connection connection = this.connections[connectionIndex];
			Connection connection2 = this.connections[1 - connectionIndex];
			if (connection.IsOutput)
			{
				GridInfo grid = connection.Grid;
				if (grid != null && grid.Power > 0.01f)
				{
					Powered powered = connection2.Item.GetComponent<Powered>();
					if (powered != null && (powered.GetCurrentPowerConsumption(connection2) > 0f || powered is PowerTransfer))
					{
						return grid.Voltage;
					}
				}
			}
			return 0f;
		}

		// Token: 0x04003246 RID: 12870
		public static Color higlightColor = Color.LightGreen;

		// Token: 0x04003247 RID: 12871
		public static Color editorHighlightColor = Color.Yellow;

		// Token: 0x04003248 RID: 12872
		public static Color editorSelectedColor = Color.Red;

		// Token: 0x04003249 RID: 12873
		private static Sprite defaultWireSprite;

		// Token: 0x0400324A RID: 12874
		private Sprite overrideSprite;

		// Token: 0x0400324B RID: 12875
		private Sprite wireSprite;

		// Token: 0x0400324C RID: 12876
		private static Wire draggingWire;

		// Token: 0x0400324D RID: 12877
		private static int? selectedNodeIndex;

		// Token: 0x0400324E RID: 12878
		private static int? highlightedNodeIndex;

		// Token: 0x0400324F RID: 12879
		private Wire.VisualSignal lastReceivedSignal;

		// Token: 0x04003250 RID: 12880
		private static readonly Color[] dataSignalColors = new Color[]
		{
			Color.White,
			Color.LightBlue,
			Color.CornflowerBlue,
			Color.Blue,
			Color.BlueViolet,
			Color.Violet
		};

		// Token: 0x04003251 RID: 12881
		private bool shouldClearConnections = true;

		// Token: 0x04003252 RID: 12882
		private const float MaxAttachDistance = 150f;

		// Token: 0x04003253 RID: 12883
		private const float MinNodeDistance = 7f;

		// Token: 0x04003254 RID: 12884
		private const int MaxNodeCount = 255;

		// Token: 0x04003255 RID: 12885
		private const int MaxNodesPerNetworkEvent = 30;

		// Token: 0x04003256 RID: 12886
		private List<Vector2> nodes;

		// Token: 0x04003257 RID: 12887
		private readonly List<Wire.WireSection> sections;

		// Token: 0x04003258 RID: 12888
		private readonly Connection[] connections;

		// Token: 0x04003259 RID: 12889
		private bool canPlaceNode;

		// Token: 0x0400325A RID: 12890
		private Vector2 newNodePos;

		// Token: 0x0400325B RID: 12891
		private Vector2 sectionExtents;

		// Token: 0x0400325C RID: 12892
		private float currLength;

		// Token: 0x0400325D RID: 12893
		public bool Hidden;

		// Token: 0x0400325E RID: 12894
		private float editNodeDelay;

		// Token: 0x0400325F RID: 12895
		private bool locked;

		// Token: 0x02001477 RID: 5239
		private readonly struct ClientEventData : ItemComponent.IEventData
		{
			// Token: 0x06009B03 RID: 39683 RVA: 0x003E3FDA File Offset: 0x003E21DA
			public ClientEventData(int nodeCount)
			{
				this.NodeCount = nodeCount;
			}

			// Token: 0x040065D8 RID: 26072
			public readonly int NodeCount;
		}

		// Token: 0x02001478 RID: 5240
		public class WireSection
		{
			// Token: 0x06009B04 RID: 39684 RVA: 0x003E3FE4 File Offset: 0x003E21E4
			private void RecalculateVertices(Sprite wireSprite, float width)
			{
				if (MathUtils.NearlyEqual(this.cachedWidth, width, 0.0001f))
				{
					return;
				}
				this.cachedWidth = width;
				this.vertices = new VertexPositionColorTexture[4];
				Vector2 expandDir = this.start - this.end;
				expandDir.Normalize();
				float temp = expandDir.X;
				expandDir.X = -expandDir.Y;
				expandDir.Y = -temp;
				Rectangle srcRect = wireSprite.SourceRect;
				expandDir *= width * (float)srcRect.Height * 0.5f;
				Vector2 rectLocation = srcRect.Location.ToVector2();
				Vector2 rectSize = srcRect.Size.ToVector2();
				Vector2 textureSize = new Vector2((float)wireSprite.Texture.Width, (float)wireSprite.Texture.Height);
				Vector2 topLeftUv = rectLocation / textureSize;
				Vector2 bottomRightUv = (rectLocation + rectSize) / textureSize;
				Vector2 invStart = new Vector2(this.start.X, -this.start.Y);
				Vector2 invEnd = new Vector2(this.end.X, -this.end.Y);
				this.vertices[0] = new VertexPositionColorTexture(new Vector3(invStart + expandDir, 0f), Color.White, topLeftUv);
				this.vertices[2] = new VertexPositionColorTexture(new Vector3(invEnd + expandDir, 0f), Color.White, new Vector2(bottomRightUv.X, topLeftUv.Y));
				this.vertices[1] = new VertexPositionColorTexture(new Vector3(invStart - expandDir, 0f), Color.White, new Vector2(topLeftUv.X, bottomRightUv.Y));
				this.vertices[3] = new VertexPositionColorTexture(new Vector3(invEnd - expandDir, 0f), Color.White, bottomRightUv);
				this.shiftedVertices = (VertexPositionColorTexture[])this.vertices.Clone();
			}

			// Token: 0x06009B05 RID: 39685 RVA: 0x003E41E8 File Offset: 0x003E23E8
			public void Draw(ISpriteBatch spriteBatch, Sprite wireSprite, Color color, Vector2 offset, float depth, float width = 0.3f)
			{
				if (width <= 0f)
				{
					return;
				}
				this.RecalculateVertices(wireSprite, width);
				for (int i = 0; i < this.vertices.Length; i++)
				{
					this.shiftedVertices[i].Color = color;
					this.shiftedVertices[i].Position = this.vertices[i].Position;
					VertexPositionColorTexture[] array = this.shiftedVertices;
					int num = i;
					array[num].Position.X = array[num].Position.X + offset.X;
					VertexPositionColorTexture[] array2 = this.shiftedVertices;
					int num2 = i;
					array2[num2].Position.Y = array2[num2].Position.Y - offset.Y;
				}
				spriteBatch.Draw(wireSprite.Texture, this.shiftedVertices, depth, null);
			}

			// Token: 0x06009B06 RID: 39686 RVA: 0x003E42B4 File Offset: 0x003E24B4
			public static void Draw(ISpriteBatch spriteBatch, Sprite wireSprite, Vector2 start, Vector2 end, Color color, float depth, float width = 0.3f)
			{
				start.Y = -start.Y;
				end.Y = -end.Y;
				spriteBatch.Draw(wireSprite.Texture, start, new Rectangle?(wireSprite.SourceRect), color, MathUtils.VectorToAngle(end - start), new Vector2(0f, wireSprite.size.Y / 2f), new Vector2(Vector2.Distance(start, end) / wireSprite.size.X, width), SpriteEffects.None, depth);
			}

			// Token: 0x17001D64 RID: 7524
			// (get) Token: 0x06009B07 RID: 39687 RVA: 0x003E433B File Offset: 0x003E253B
			public Vector2 Start
			{
				get
				{
					return this.start;
				}
			}

			// Token: 0x17001D65 RID: 7525
			// (get) Token: 0x06009B08 RID: 39688 RVA: 0x003E4343 File Offset: 0x003E2543
			public Vector2 End
			{
				get
				{
					return this.end;
				}
			}

			// Token: 0x06009B09 RID: 39689 RVA: 0x003E434B File Offset: 0x003E254B
			public WireSection(Vector2 start, Vector2 end)
			{
				this.start = start;
				this.end = end;
				this.angle = MathUtils.VectorToAngle(end - start);
				this.Length = Vector2.Distance(start, end);
			}

			// Token: 0x040065D9 RID: 26073
			public VertexPositionColorTexture[] vertices;

			// Token: 0x040065DA RID: 26074
			public VertexPositionColorTexture[] shiftedVertices;

			// Token: 0x040065DB RID: 26075
			private float cachedWidth;

			// Token: 0x040065DC RID: 26076
			private Vector2 start;

			// Token: 0x040065DD RID: 26077
			private Vector2 end;

			// Token: 0x040065DE RID: 26078
			private readonly float angle;

			// Token: 0x040065DF RID: 26079
			public readonly float Length;
		}

		// Token: 0x02001479 RID: 5241
		public readonly struct VisualSignal : IEquatable<Wire.VisualSignal>
		{
			// Token: 0x06009B0A RID: 39690 RVA: 0x003E4380 File Offset: 0x003E2580
			public VisualSignal(float TimeSent, Color Color, int Direction)
			{
				this.TimeSent = TimeSent;
				this.Color = Color;
				this.Direction = Direction;
			}

			// Token: 0x17001D66 RID: 7526
			// (get) Token: 0x06009B0B RID: 39691 RVA: 0x003E4397 File Offset: 0x003E2597
			// (set) Token: 0x06009B0C RID: 39692 RVA: 0x003E439F File Offset: 0x003E259F
			public float TimeSent { get; set; }

			// Token: 0x17001D67 RID: 7527
			// (get) Token: 0x06009B0D RID: 39693 RVA: 0x003E43A8 File Offset: 0x003E25A8
			// (set) Token: 0x06009B0E RID: 39694 RVA: 0x003E43B0 File Offset: 0x003E25B0
			public Color Color { get; set; }

			// Token: 0x17001D68 RID: 7528
			// (get) Token: 0x06009B0F RID: 39695 RVA: 0x003E43B9 File Offset: 0x003E25B9
			// (set) Token: 0x06009B10 RID: 39696 RVA: 0x003E43C1 File Offset: 0x003E25C1
			public int Direction { get; set; }

			// Token: 0x06009B11 RID: 39697 RVA: 0x003E43CC File Offset: 0x003E25CC
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("VisualSignal");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009B12 RID: 39698 RVA: 0x003E4418 File Offset: 0x003E2618
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("TimeSent = ");
				builder.Append(this.TimeSent.ToString());
				builder.Append(", Color = ");
				builder.Append(this.Color.ToString());
				builder.Append(", Direction = ");
				builder.Append(this.Direction.ToString());
				return true;
			}

			// Token: 0x06009B13 RID: 39699 RVA: 0x003E449B File Offset: 0x003E269B
			[CompilerGenerated]
			public static bool operator !=(Wire.VisualSignal left, Wire.VisualSignal right)
			{
				return !(left == right);
			}

			// Token: 0x06009B14 RID: 39700 RVA: 0x003E44A7 File Offset: 0x003E26A7
			[CompilerGenerated]
			public static bool operator ==(Wire.VisualSignal left, Wire.VisualSignal right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009B15 RID: 39701 RVA: 0x003E44B1 File Offset: 0x003E26B1
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<float>.Default.GetHashCode(this.<TimeSent>k__BackingField) * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Direction>k__BackingField);
			}

			// Token: 0x06009B16 RID: 39702 RVA: 0x003E44F1 File Offset: 0x003E26F1
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Wire.VisualSignal && this.Equals((Wire.VisualSignal)obj);
			}

			// Token: 0x06009B17 RID: 39703 RVA: 0x003E450C File Offset: 0x003E270C
			[CompilerGenerated]
			public bool Equals(Wire.VisualSignal other)
			{
				return EqualityComparer<float>.Default.Equals(this.<TimeSent>k__BackingField, other.<TimeSent>k__BackingField) && EqualityComparer<Color>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Direction>k__BackingField, other.<Direction>k__BackingField);
			}

			// Token: 0x06009B18 RID: 39704 RVA: 0x003E4561 File Offset: 0x003E2761
			[CompilerGenerated]
			public void Deconstruct(out float TimeSent, out Color Color, out int Direction)
			{
				TimeSent = this.TimeSent;
				Color = this.Color;
				Direction = this.Direction;
			}
		}
	}
}
