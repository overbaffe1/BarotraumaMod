using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000E7 RID: 231
	internal abstract class MapEntity : Entity, ISpatialEntity
	{
		// Token: 0x17000884 RID: 2180
		// (get) Token: 0x06001FD1 RID: 8145 RVA: 0x0014065B File Offset: 0x0013E85B
		public static Vector2 StartMovingPos
		{
			get
			{
				return MapEntity.startMovingPos;
			}
		}

		// Token: 0x17000885 RID: 2181
		// (get) Token: 0x06001FD2 RID: 8146 RVA: 0x00140662 File Offset: 0x0013E862
		public static Vector2 SelectionPos
		{
			get
			{
				return MapEntity.selectionPos;
			}
		}

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x06001FD3 RID: 8147 RVA: 0x0014066C File Offset: 0x0013E86C
		// (remove) Token: 0x06001FD4 RID: 8148 RVA: 0x001406A4 File Offset: 0x0013E8A4
		public event Action<Rectangle> Resized;

		// Token: 0x17000886 RID: 2182
		// (get) Token: 0x06001FD5 RID: 8149 RVA: 0x001406D9 File Offset: 0x0013E8D9
		// (set) Token: 0x06001FD6 RID: 8150 RVA: 0x001406E0 File Offset: 0x0013E8E0
		public static bool Resizing { get; private set; }

		// Token: 0x17000887 RID: 2183
		// (get) Token: 0x06001FD7 RID: 8151 RVA: 0x001406E8 File Offset: 0x0013E8E8
		// (set) Token: 0x06001FD8 RID: 8152 RVA: 0x001406EF File Offset: 0x0013E8EF
		public static HashSet<MapEntity> SelectedList { get; private set; } = new HashSet<MapEntity>();

		// Token: 0x17000888 RID: 2184
		// (get) Token: 0x06001FD9 RID: 8153 RVA: 0x001406F7 File Offset: 0x0013E8F7
		public static GUIListBox HighlightedListBox
		{
			get
			{
				return MapEntity.highlightedListBox;
			}
		}

		// Token: 0x17000889 RID: 2185
		// (get) Token: 0x06001FDA RID: 8154 RVA: 0x001406FE File Offset: 0x0013E8FE
		public static GUIComponent EditingHUD
		{
			get
			{
				return MapEntity.editingHUD;
			}
		}

		// Token: 0x1700088A RID: 2186
		// (get) Token: 0x06001FDB RID: 8155 RVA: 0x00140705 File Offset: 0x0013E905
		// (set) Token: 0x06001FDC RID: 8156 RVA: 0x0014070C File Offset: 0x0013E90C
		public static bool DisableSelect
		{
			get
			{
				return MapEntity.disableSelect;
			}
			set
			{
				MapEntity.disableSelect = value;
				if (MapEntity.disableSelect)
				{
					MapEntity.StopSelection();
				}
			}
		}

		// Token: 0x1700088B RID: 2187
		// (get) Token: 0x06001FDD RID: 8157 RVA: 0x00140720 File Offset: 0x0013E920
		public virtual bool SelectableInEditor
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700088C RID: 2188
		// (get) Token: 0x06001FDE RID: 8158 RVA: 0x00140723 File Offset: 0x0013E923
		public static bool SelectedAny
		{
			get
			{
				return MapEntity.SelectedList.Count > 0;
			}
		}

		// Token: 0x1700088D RID: 2189
		// (get) Token: 0x06001FDF RID: 8159 RVA: 0x00140732 File Offset: 0x0013E932
		public bool IsSelected
		{
			get
			{
				return MapEntity.SelectedList.Contains(this);
			}
		}

		// Token: 0x1700088E RID: 2190
		// (get) Token: 0x06001FE0 RID: 8160 RVA: 0x0014073F File Offset: 0x0013E93F
		// (set) Token: 0x06001FE1 RID: 8161 RVA: 0x00140747 File Offset: 0x0013E947
		public bool IsIncludedInSelection { get; set; }

		// Token: 0x06001FE2 RID: 8162 RVA: 0x00140750 File Offset: 0x0013E950
		public virtual bool IsVisible(Rectangle worldView)
		{
			Rectangle worldRect = this.WorldRect;
			return worldRect.X <= worldView.Right && worldRect.Right >= worldView.X && worldRect.Y >= worldView.Y - worldView.Height && worldRect.Y - worldRect.Height <= worldView.Y && Screen.Selected.Cam.Zoom >= 0.05f && (float)worldRect.Width * Screen.Selected.Cam.Zoom >= 1f && (float)worldRect.Height * Screen.Selected.Cam.Zoom >= 1f;
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x00140807 File Offset: 0x0013EA07
		public virtual void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
		}

		// Token: 0x06001FE4 RID: 8164 RVA: 0x00140809 File Offset: 0x0013EA09
		public virtual float GetDrawDepth()
		{
			return 0f;
		}

		// Token: 0x06001FE5 RID: 8165 RVA: 0x00140810 File Offset: 0x0013EA10
		public float GetDrawDepth(float baseDepth, Sprite sprite)
		{
			int? num;
			if (sprite == null)
			{
				num = null;
			}
			else
			{
				Texture2D texture = sprite.Texture;
				num = ((texture != null) ? new int?(texture.SortingKey) : null);
			}
			int? num2 = num;
			float depth = baseDepth + (float)(num2.GetValueOrDefault() % 100) * 1E-06f + (float)(this.ID % 100) * 1E-07f;
			return Math.Min(depth, 1f);
		}

		// Token: 0x06001FE6 RID: 8166 RVA: 0x0014087C File Offset: 0x0013EA7C
		protected Vector2 GetCollapseEffectOffset()
		{
			Level loaded = Level.Loaded;
			float? num;
			if (loaded == null)
			{
				num = null;
			}
			else
			{
				LevelRenderer renderer = loaded.Renderer;
				num = ((renderer != null) ? new float?(renderer.CollapseEffectStrength) : null);
			}
			float? num2 = num;
			if (num2 != null)
			{
				float collapseEffectStrength = num2.GetValueOrDefault();
				if (collapseEffectStrength > 0f)
				{
					Submarine submarine = base.Submarine;
					if (submarine != null)
					{
						SubmarineInfo info = submarine.Info;
						if (info != null && info.Type == SubmarineType.Player)
						{
							goto IL_146;
						}
					}
					Vector2 noisePos = new Vector2(PerlinNoise.GetPerlin((float)(Timing.TotalTime + (double)this.ID) * 0.1f, (float)(Timing.TotalTime + (double)this.ID) * 0.5f) - 0.5f, PerlinNoise.GetPerlin((float)(Timing.TotalTime + (double)this.ID) * 0.1f, (float)(Timing.TotalTime + (double)this.ID) * 0.1f) - 0.5f);
					Vector2 offsetFromOrigin = Level.Loaded.Renderer.CollapseEffectOrigin - this.DrawPosition;
					return offsetFromOrigin * MathF.Pow(collapseEffectStrength, MathHelper.Lerp(1f, 4f, (float)(this.ID % 1000) / 1000f)) + noisePos * 100f * collapseEffectStrength;
				}
			}
			IL_146:
			return Vector2.Zero;
		}

		// Token: 0x06001FE7 RID: 8167 RVA: 0x001409D4 File Offset: 0x0013EBD4
		public static void UpdateSelecting(Camera cam)
		{
			if (MapEntity.Resizing)
			{
				if (!MapEntity.SelectedAny)
				{
					MapEntity.Resizing = false;
				}
				return;
			}
			MapEntity.ClearHighlightedEntities();
			if (MapEntity.DisableSelect)
			{
				MapEntity.DisableSelect = false;
				return;
			}
			if (MapEntity.startMovingPos == Vector2.Zero && MapEntity.selectionPos == Vector2.Zero && (GUI.MouseOn != null || !PlayerInput.MouseInsideWindow) && (MapEntity.highlightedListBox == null || (GUI.MouseOn != MapEntity.highlightedListBox && !MapEntity.highlightedListBox.IsParentOf(GUI.MouseOn, true))))
			{
				MapEntity.UpdateHighlightedListBox(null, false);
				return;
			}
			if (MapEntityPrefab.Selected != null)
			{
				MapEntity.selectionPos = Vector2.Zero;
				MapEntity.SelectedList.Clear();
				return;
			}
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (PlayerInput.KeyHit(Keys.Delete) && MapEntity.SelectedAny)
				{
					SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>(MapEntity.SelectedList), true, true));
					MapEntity.SelectedList.ForEachMod(delegate(MapEntity e)
					{
						if (!e.Removed)
						{
							e.Remove();
						}
					});
					MapEntity.SelectedList.Clear();
				}
				if (PlayerInput.IsCtrlDown())
				{
					if (PlayerInput.KeyHit(Keys.C))
					{
						MapEntity.Copy(MapEntity.SelectedList.ToList<MapEntity>());
					}
					else if (PlayerInput.KeyHit(Keys.X))
					{
						MapEntity.Cut(MapEntity.SelectedList.ToList<MapEntity>());
					}
					else if (PlayerInput.KeyHit(Keys.V))
					{
						MapEntity.Paste(cam.ScreenToWorld(PlayerInput.MousePosition));
					}
				}
			}
			Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
			MapEntity highLightedEntity = null;
			if (MapEntity.startMovingPos == Vector2.Zero)
			{
				List<MapEntity> highlightedEntities = new List<MapEntity>();
				if (MapEntity.highlightedListBox != null && MapEntity.highlightedListBox.IsParentOf(GUI.MouseOn, true))
				{
					highLightedEntity = (GUI.MouseOn.UserData as MapEntity);
				}
				else
				{
					foreach (MapEntity e5 in MapEntity.MapEntityList)
					{
						if (e5.SelectableInEditor && e5.IsMouseOn(position))
						{
							int i = 0;
							while (i < highlightedEntities.Count && e5.Sprite != null && (highlightedEntities[i].Sprite == null || highlightedEntities[i].SpriteDepth < e5.SpriteDepth))
							{
								i++;
							}
							highlightedEntities.Insert(i, e5);
							if (i == 0)
							{
								highLightedEntity = e5;
							}
						}
					}
					MapEntity.UpdateHighlighting(highlightedEntities, false);
				}
				if (highLightedEntity != null)
				{
					highLightedEntity.IsHighlighted = true;
				}
			}
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				Vector2 previousNudge = MapEntity.entityMovementNudge;
				MapEntity.entityMovementNudge = MapEntity.GetNudgeAmount(true);
				if (MapEntity.entityMovementNudge != Vector2.Zero)
				{
					if (previousNudge == Vector2.Zero)
					{
						MapEntity.oldRects = (from entity in MapEntity.SelectedList
						select entity.Rect).ToList<Rectangle>();
					}
					using (HashSet<MapEntity>.Enumerator enumerator2 = MapEntity.SelectedList.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							MapEntity entityToNudge = enumerator2.Current;
							entityToNudge.Move(MapEntity.entityMovementNudge, true);
						}
						goto IL_36D;
					}
				}
				if (previousNudge != Vector2.Zero)
				{
					SubEditorScreen.StoreCommand(new TransformCommand(new List<MapEntity>(MapEntity.SelectedList), (from entity in MapEntity.SelectedList
					select entity.Rect).ToList<Rectangle>(), MapEntity.oldRects, false));
				}
			}
			else
			{
				MapEntity.keyDelay = 0f;
			}
			IL_36D:
			bool isShiftDown = PlayerInput.IsShiftDown();
			if (MapEntity.startMovingPos != Vector2.Zero)
			{
				Item targetContainer = MapEntity.GetPotentialContainer(position, MapEntity.SelectedList);
				if (targetContainer != null)
				{
					targetContainer.IsHighlighted = true;
				}
				if (PlayerInput.PrimaryMouseButtonReleased())
				{
					Vector2 moveAmount = position - MapEntity.startMovingPos;
					if (!isShiftDown)
					{
						moveAmount.X = (float)((moveAmount.X > 0f) ? Math.Floor((double)(moveAmount.X / Submarine.GridSize.X)) : Math.Ceiling((double)(moveAmount.X / Submarine.GridSize.X))) * Submarine.GridSize.X;
						moveAmount.Y = (float)((moveAmount.Y > 0f) ? Math.Floor((double)(moveAmount.Y / Submarine.GridSize.Y)) : Math.Ceiling((double)(moveAmount.Y / Submarine.GridSize.Y))) * Submarine.GridSize.Y;
					}
					if (Math.Abs(moveAmount.X) >= Submarine.GridSize.X || Math.Abs(moveAmount.Y) >= Submarine.GridSize.Y || isShiftDown)
					{
						if (!isShiftDown)
						{
							moveAmount = Submarine.VectorToWorldGrid(moveAmount, null, false);
						}
						if (PlayerInput.IsCtrlDown())
						{
							HashSet<MapEntity> clones = (from c in MapEntity.Clone(MapEntity.SelectedList.ToList<MapEntity>())
							where c != null
							select c).ToHashSet<MapEntity>();
							if (clones.Count == 1)
							{
								WayPoint wayPoint = clones.First<MapEntity>() as WayPoint;
								if (wayPoint != null)
								{
									WayPoint originalWaypoint = MapEntity.SelectedList.First<MapEntity>() as WayPoint;
									if (originalWaypoint != null && originalWaypoint.SpawnType == SpawnType.Path)
									{
										originalWaypoint.ConnectTo(wayPoint);
									}
								}
							}
							MapEntity.SelectedList = clones;
							MapEntity.SelectedList.ForEach(delegate(MapEntity c)
							{
								c.Move(moveAmount, true);
							});
							SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>(clones), false, true));
						}
						else
						{
							List<Rectangle> oldRects = (from e in MapEntity.SelectedList
							select e.Rect).ToList<Rectangle>();
							List<MapEntity> deposited = new List<MapEntity>();
							foreach (MapEntity e2 in MapEntity.SelectedList)
							{
								e2.Move(moveAmount, true);
								if (isShiftDown)
								{
									Item item = e2 as Item;
									if (item != null && targetContainer != null)
									{
										if (targetContainer.OwnInventory.TryPutItem(item, Character.Controlled, null, true, false, true))
										{
											SoundPlayer.PlayUISound(GUISoundType.DropItem);
											deposited.Add(item);
										}
										else
										{
											SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
										}
									}
								}
							}
							SubEditorScreen.StoreCommand(new TransformCommand(new List<MapEntity>(MapEntity.SelectedList), (from entity in MapEntity.SelectedList
							select entity.Rect).ToList<Rectangle>(), oldRects, false));
							if (deposited.Any<MapEntity>())
							{
								if (deposited.Any((MapEntity entity) => entity is Item))
								{
									List<Item> depositedItems = (from entity in deposited
									where entity is Item
									select entity).Cast<Item>().ToList<Item>();
									SubEditorScreen.StoreCommand(new InventoryPlaceCommand(targetContainer.OwnInventory, depositedItems, false));
								}
							}
							deposited.ForEach(delegate(MapEntity entity)
							{
								MapEntity.SelectedList.Remove(entity);
							});
						}
					}
					MapEntity.startMovingPos = Vector2.Zero;
					return;
				}
			}
			else
			{
				if (MapEntity.selectionPos != Vector2.Zero)
				{
					MapEntity.selectionSize.X = position.X - MapEntity.selectionPos.X;
					MapEntity.selectionSize.Y = MapEntity.selectionPos.Y - position.Y;
					foreach (MapEntity entity6 in MapEntity.MapEntityList)
					{
						entity6.IsIncludedInSelection = false;
					}
					HashSet<MapEntity> newSelection = new HashSet<MapEntity>();
					if (Math.Abs(MapEntity.selectionSize.X) > Submarine.GridSize.X || Math.Abs(MapEntity.selectionSize.Y) > Submarine.GridSize.Y)
					{
						newSelection = MapEntity.FindSelectedEntities(MapEntity.selectionPos, MapEntity.selectionSize);
					}
					else if (highLightedEntity != null)
					{
						if (SubEditorScreen.IsLayerLinked(highLightedEntity))
						{
							ImmutableHashSet<MapEntity> entitiesInSameLayer = SubEditorScreen.GetEntitiesInSameLayer(highLightedEntity);
							IEnumerable<MapEntity> source = entitiesInSameLayer;
							Func<MapEntity, bool> predicate;
							Func<MapEntity, bool> <>9__10;
							if ((predicate = <>9__10) == null)
							{
								predicate = (<>9__10 = ((MapEntity e) => !newSelection.Contains(e)));
							}
							foreach (MapEntity entity2 in source.Where(predicate))
							{
								newSelection.Add(entity2);
							}
							using (ImmutableHashSet<MapEntity>.Enumerator enumerator6 = entitiesInSameLayer.GetEnumerator())
							{
								while (enumerator6.MoveNext())
								{
									MapEntity entity3 = enumerator6.Current;
									entity3.IsIncludedInSelection = true;
								}
								goto IL_906;
							}
						}
						newSelection.Add(highLightedEntity);
						highLightedEntity.IsIncludedInSelection = true;
					}
					IL_906:
					if (!PlayerInput.PrimaryMouseButtonReleased())
					{
						return;
					}
					if (PlayerInput.IsCtrlDown())
					{
						using (HashSet<MapEntity>.Enumerator enumerator7 = newSelection.GetEnumerator())
						{
							while (enumerator7.MoveNext())
							{
								MapEntity e3 = enumerator7.Current;
								if (MapEntity.SelectedList.Contains(e3))
								{
									MapEntity.RemoveSelection(e3);
								}
								else
								{
									MapEntity.AddSelection(e3);
								}
							}
							goto IL_9F2;
						}
					}
					MapEntity.SelectedList = new HashSet<MapEntity>(newSelection);
					foreach (MapEntity entity4 in newSelection)
					{
						MapEntity.HandleDoorGapLinks(entity4, delegate(Door door, Gap gap)
						{
							door.RefreshLinkedGap();
							if (!MapEntity.SelectedList.Contains(gap))
							{
								MapEntity.SelectedList.Add(gap);
							}
						}, delegate(Door door, Gap gap)
						{
							if (!MapEntity.SelectedList.Contains(door.Item))
							{
								MapEntity.SelectedList.Add(door.Item);
							}
						});
					}
					IL_9F2:
					List<Item> selectedItems = (from e in MapEntity.SelectedList
					where e is Item
					select e).Cast<Item>().ToList<Item>();
					foreach (Item item2 in Item.ItemList)
					{
						Wire wire = item2.GetComponent<Wire>();
						if (wire != null)
						{
							Connection connection = wire.Connections[0];
							Item item3 = (connection != null) ? connection.Item : null;
							Connection connection2 = wire.Connections[1];
							Item item4 = (connection2 != null) ? connection2.Item : null;
							if (item3 == null && item4 != null)
							{
								item3 = Item.ItemList.Find(delegate(Item it)
								{
									ConnectionPanel component = it.GetComponent<ConnectionPanel>();
									return component != null && component.DisconnectedWires.Contains(wire);
								});
							}
							else if (item3 != null && item4 == null)
							{
								item4 = Item.ItemList.Find(delegate(Item it)
								{
									ConnectionPanel component = it.GetComponent<ConnectionPanel>();
									return component != null && component.DisconnectedWires.Contains(wire);
								});
							}
							if (item3 != null && item4 != null && MapEntity.SelectedList.Contains(item3) && MapEntity.SelectedList.Contains(item4))
							{
								MapEntity.SelectedList.Add(item2);
							}
						}
					}
					MapEntity.selectionPos = Vector2.Zero;
					MapEntity.selectionSize = Vector2.Zero;
					using (List<MapEntity>.Enumerator enumerator10 = MapEntity.MapEntityList.GetEnumerator())
					{
						while (enumerator10.MoveNext())
						{
							MapEntity entity5 = enumerator10.Current;
							entity5.IsIncludedInSelection = false;
						}
						return;
					}
				}
				if (PlayerInput.PrimaryMouseButtonHeld() && PlayerInput.KeyUp(Keys.Space) && PlayerInput.KeyUp(Keys.LeftAlt) && PlayerInput.KeyUp(Keys.RightAlt) && (MapEntity.highlightedListBox == null || (GUI.MouseOn != MapEntity.highlightedListBox && !MapEntity.highlightedListBox.IsParentOf(GUI.MouseOn, true))))
				{
					foreach (MapEntity e4 in MapEntity.SelectedList)
					{
						if (e4.IsMouseOn(position))
						{
							MapEntity.startMovingPos = position;
						}
					}
					MapEntity.selectionPos = position;
					Screen.Selected.Cam.StopMovement();
				}
			}
		}

		// Token: 0x06001FE8 RID: 8168 RVA: 0x00141690 File Offset: 0x0013F890
		public static void StopSelection()
		{
			MapEntity.startMovingPos = Vector2.Zero;
			MapEntity.selectionSize = Vector2.Zero;
			MapEntity.selectionPos = Vector2.Zero;
		}

		// Token: 0x06001FE9 RID: 8169 RVA: 0x001416B0 File Offset: 0x0013F8B0
		public static Vector2 GetNudgeAmount(bool doHold = true)
		{
			Vector2 nudgeAmount = Vector2.Zero;
			if (doHold)
			{
				int up = (PlayerInput.KeyDown(Keys.Up) > false) ? 1 : 0;
				int down = PlayerInput.KeyDown(Keys.Down) ? -1 : 0;
				int left = PlayerInput.KeyDown(Keys.Left) ? -1 : 0;
				int right = (PlayerInput.KeyDown(Keys.Right) > false) ? 1 : 0;
				int xKeysDown = left + right;
				int yKeysDown = up + down;
				if (xKeysDown != 0 || yKeysDown != 0)
				{
					MapEntity.keyDelay += 0.016666668f;
				}
				else
				{
					MapEntity.keyDelay = 0f;
				}
				if (MapEntity.keyDelay >= 0.5f)
				{
					nudgeAmount.Y = (float)yKeysDown;
					nudgeAmount.X = (float)xKeysDown;
				}
			}
			if (PlayerInput.KeyHit(Keys.Up))
			{
				nudgeAmount.Y = 1f;
			}
			if (PlayerInput.KeyHit(Keys.Down))
			{
				nudgeAmount.Y = -1f;
			}
			if (PlayerInput.KeyHit(Keys.Left))
			{
				nudgeAmount.X = -1f;
			}
			if (PlayerInput.KeyHit(Keys.Right))
			{
				nudgeAmount.X = 1f;
			}
			return nudgeAmount;
		}

		// Token: 0x06001FEA RID: 8170 RVA: 0x001417A0 File Offset: 0x0013F9A0
		public MapEntity GetReplacementOrThis()
		{
			MapEntity replacedBy = this.ReplacedBy;
			return ((replacedBy != null) ? replacedBy.GetReplacementOrThis() : null) ?? this;
		}

		// Token: 0x06001FEB RID: 8171 RVA: 0x001417BC File Offset: 0x0013F9BC
		public static Item GetPotentialContainer(Vector2 position, HashSet<MapEntity> entities = null)
		{
			Item targetContainer = null;
			if (!PlayerInput.IsShiftDown())
			{
				return null;
			}
			foreach (MapEntity e in MapEntity.MapEntityList)
			{
				if (e.SelectableInEditor)
				{
					Item potentialContainer = e as Item;
					if (potentialContainer != null)
					{
						if (e.IsMouseOn(position))
						{
							if (entities == null)
							{
								if (potentialContainer.OwnInventory != null && potentialContainer.ParentInventory == null && !potentialContainer.OwnInventory.IsFull(true))
								{
									targetContainer = potentialContainer;
									break;
								}
							}
							else
							{
								foreach (MapEntity selectedEntity in entities)
								{
									Item selectedItem = selectedEntity as Item;
									if (selectedItem != null && potentialContainer.OwnInventory != null && potentialContainer.ParentInventory == null && potentialContainer != selectedItem && potentialContainer.OwnInventory.CanBePut(selectedItem))
									{
										targetContainer = potentialContainer;
										break;
									}
								}
							}
						}
						if (targetContainer != null)
						{
							break;
						}
					}
				}
			}
			return targetContainer;
		}

		// Token: 0x06001FEC RID: 8172 RVA: 0x001418E8 File Offset: 0x0013FAE8
		public static void UpdateHighlighting(List<MapEntity> highlightedEntities, bool wiringMode = false)
		{
			if (PlayerInput.MouseSpeed.LengthSquared() > 10f)
			{
				MapEntity.highlightTimer = 0f;
				return;
			}
			bool mouseNearHighlightBox = false;
			if (MapEntity.highlightedListBox != null)
			{
				Rectangle expandedRect = MapEntity.highlightedListBox.Rect;
				expandedRect.Inflate(20, 20);
				mouseNearHighlightBox = expandedRect.Contains(PlayerInput.MousePosition);
				if (!mouseNearHighlightBox)
				{
					MapEntity.highlightedListBox = null;
				}
			}
			MapEntity.highlightTimer += 0.016666668f;
			if (MapEntity.highlightTimer > 1f && !mouseNearHighlightBox)
			{
				MapEntity.UpdateHighlightedListBox(highlightedEntities, wiringMode);
				MapEntity.highlightTimer = 0f;
			}
		}

		// Token: 0x06001FED RID: 8173 RVA: 0x0014197C File Offset: 0x0013FB7C
		private static void UpdateHighlightedListBox(List<MapEntity> highlightedEntities, bool wiringMode)
		{
			if (highlightedEntities == null || highlightedEntities.Count < 2)
			{
				MapEntity.highlightedListBox = null;
				return;
			}
			if (MapEntity.highlightedListBox != null)
			{
				if (GUI.MouseOn == MapEntity.highlightedListBox || MapEntity.highlightedListBox.IsParentOf(GUI.MouseOn, true))
				{
					return;
				}
				if (highlightedEntities.SequenceEqual(MapEntity.highlightedInEditorList))
				{
					return;
				}
			}
			MapEntity.highlightedInEditorList = highlightedEntities;
			MapEntity.highlightedListBox = new GUIListBox(new RectTransform(new Point(180, highlightedEntities.Count * 18 + 5), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				MaxSize = new Point(int.MaxValue, 256),
				ScreenSpaceOffset = PlayerInput.MousePosition.ToPoint() + new Point(15)
			}, false, null, "GUIToolTip", true, false);
			foreach (MapEntity entity in highlightedEntities)
			{
				LocalizedString tooltip = string.Empty;
				if (wiringMode)
				{
					Item item = entity as Item;
					if (item != null)
					{
						Wire wire = item.GetComponent<Wire>();
						if (((wire != null) ? wire.Connections : null) != null)
						{
							for (int i = 0; i < wire.Connections.Length; i++)
							{
								Connection conn = wire.Connections[i];
								if (conn != null)
								{
									LocalizedString left = tooltip;
									string tag = "wirelistformat";
									ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
									int num = 0;
									string item2 = "[item]";
									Item item3 = conn.Item;
									array[num] = new ValueTuple<string, string>(item2, (item3 != null) ? item3.Name : null);
									array[1] = new ValueTuple<string, string>("[pin]", conn.Name);
									tooltip = left + TextManager.GetWithVariables(tag, array);
								}
								if (i != wire.Connections.Length - 1)
								{
									tooltip += '\n';
								}
							}
						}
					}
				}
				RectTransform rectT = new RectTransform(new Point(MapEntity.highlightedListBox.Content.Rect.Width, 15), MapEntity.highlightedListBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text = ToolBox.LimitString(entity.Name, GUIStyle.SmallFont, 140);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null);
				guitextBlock.ToolTip = tooltip;
				guitextBlock.UserData = entity;
			}
			MapEntity.highlightedListBox.OnSelected = delegate(GUIComponent component, object obj)
			{
				MapEntity entity2 = obj as MapEntity;
				if (PlayerInput.IsCtrlDown() && !wiringMode)
				{
					if (MapEntity.SelectedList.Contains(entity2))
					{
						MapEntity.RemoveSelection(entity2);
					}
					else
					{
						MapEntity.AddSelection(entity2);
					}
					return true;
				}
				MapEntity.SelectEntity(entity2);
				return true;
			};
		}

		// Token: 0x06001FEE RID: 8174 RVA: 0x00141C3C File Offset: 0x0013FE3C
		public static void AddSelection(MapEntity entity)
		{
			if (MapEntity.SelectedList.Contains(entity))
			{
				return;
			}
			MapEntity.SelectedList.Add(entity);
			MapEntity.HandleDoorGapLinks(entity, delegate(Door door, Gap gap)
			{
				door.RefreshLinkedGap();
				if (!MapEntity.SelectedList.Contains(gap))
				{
					MapEntity.SelectedList.Add(gap);
				}
			}, delegate(Door door, Gap gap)
			{
				if (!MapEntity.SelectedList.Contains(door.Item))
				{
					MapEntity.SelectedList.Add(door.Item);
				}
			});
		}

		// Token: 0x06001FEF RID: 8175 RVA: 0x00141CA8 File Offset: 0x0013FEA8
		private static void HandleDoorGapLinks(MapEntity entity, Action<Door, Gap> onGapFound, Action<Door, Gap> onDoorFound)
		{
			Item i = entity as Item;
			if (i == null)
			{
				Gap gap = entity as Gap;
				if (gap == null)
				{
					return;
				}
				Door door = gap.ConnectedDoor;
				if (door != null)
				{
					onDoorFound(door, gap);
				}
			}
			else
			{
				Door door2 = i.GetComponent<Door>();
				Gap gap2 = (door2 != null) ? door2.LinkedGap : null;
				if (gap2 != null)
				{
					onGapFound(door2, gap2);
					return;
				}
			}
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x00141D00 File Offset: 0x0013FF00
		public static void RemoveSelection(MapEntity entity)
		{
			MapEntity.SelectedList.Remove(entity);
			MapEntity.HandleDoorGapLinks(entity, delegate(Door door, Gap gap)
			{
				MapEntity.SelectedList.Remove(gap);
			}, delegate(Door door, Gap gap)
			{
				MapEntity.SelectedList.Remove(door.Item);
			});
		}

		// Token: 0x06001FF1 RID: 8177 RVA: 0x00141D60 File Offset: 0x0013FF60
		public static void DrawSelecting(SpriteBatch spriteBatch, Camera cam)
		{
			SubEditorScreen subEditor = Screen.Selected as SubEditorScreen;
			if (subEditor != null)
			{
				if (subEditor.IsMouseOnEditorGUI())
				{
					return;
				}
			}
			else if (GUI.MouseOn != null)
			{
				return;
			}
			Vector2 position = PlayerInput.MousePosition;
			position = cam.ScreenToWorld(position);
			if (MapEntity.startMovingPos != Vector2.Zero)
			{
				Vector2 moveAmount = position - MapEntity.startMovingPos;
				moveAmount.Y = -moveAmount.Y;
				bool isShiftDown = PlayerInput.IsShiftDown();
				if (!isShiftDown)
				{
					moveAmount.X = (float)((moveAmount.X > 0f) ? Math.Floor((double)(moveAmount.X / Submarine.GridSize.X)) : Math.Ceiling((double)(moveAmount.X / Submarine.GridSize.X))) * Submarine.GridSize.X;
					moveAmount.Y = (float)((moveAmount.Y > 0f) ? Math.Floor((double)(moveAmount.Y / Submarine.GridSize.Y)) : Math.Ceiling((double)(moveAmount.Y / Submarine.GridSize.Y))) * Submarine.GridSize.Y;
				}
				if (Math.Abs(moveAmount.X) >= Submarine.GridSize.X || Math.Abs(moveAmount.Y) >= Submarine.GridSize.Y || isShiftDown)
				{
					foreach (MapEntity e in MapEntity.SelectedList)
					{
						SpriteEffects spriteEffects = SpriteEffects.None;
						float spriteRotation = 0f;
						float rectangleRotation = 0f;
						Item item = e as Item;
						if (item == null)
						{
							Structure structure = e as Structure;
							if (structure == null)
							{
								WayPoint wayPoint = e as WayPoint;
								if (wayPoint != null)
								{
									Vector2 drawPos = e.WorldPosition;
									drawPos.Y = -drawPos.Y;
									drawPos += moveAmount;
									wayPoint.Draw(spriteBatch, drawPos);
									continue;
								}
								LinkedSubmarine linkedSub = e as LinkedSubmarine;
								if (linkedSub != null)
								{
									Vector2 ma = moveAmount;
									ma.Y = -ma.Y;
									Vector2 lPos = linkedSub.Position;
									lPos += ma;
									linkedSub.Draw(spriteBatch, lPos, 0.5f);
								}
							}
							else
							{
								if (structure.FlippedX && structure.Prefab.CanSpriteFlipX)
								{
									spriteEffects ^= SpriteEffects.FlipHorizontally;
								}
								if (structure.FlippedY && structure.Prefab.CanSpriteFlipY)
								{
									spriteEffects ^= SpriteEffects.FlipVertically;
								}
								rectangleRotation = MathHelper.ToRadians(structure.Rotation);
								spriteRotation = rectangleRotation;
								bool spriteIsFlippedHorizontally = structure.Sprite.effects.HasFlag(SpriteEffects.FlipHorizontally);
								bool spriteIsFlippedVertically = structure.Sprite.effects.HasFlag(SpriteEffects.FlipVertically);
								if (spriteIsFlippedHorizontally != spriteIsFlippedVertically)
								{
									spriteRotation = -spriteRotation;
								}
								if (structure.FlippedX != structure.FlippedY)
								{
									rectangleRotation = -rectangleRotation;
								}
							}
						}
						else
						{
							if (item.FlippedX && item.Prefab.CanSpriteFlipX)
							{
								spriteEffects ^= SpriteEffects.FlipHorizontally;
							}
							if (item.FlippedY && item.Prefab.CanSpriteFlipY)
							{
								spriteEffects ^= SpriteEffects.FlipVertically;
							}
							spriteRotation = MathHelper.ToRadians(item.Rotation);
							rectangleRotation = spriteRotation;
							Wire wire = item.GetComponent<Wire>();
							if (wire != null && wire.Item.body != null && !wire.Item.body.Enabled)
							{
								wire.Draw(spriteBatch, false, new Vector2(moveAmount.X, -moveAmount.Y), -1f, null);
								continue;
							}
						}
						MapEntityPrefab prefab = e.Prefab;
						if (prefab != null)
						{
							prefab.DrawPlacing(spriteBatch, new Rectangle(e.WorldRect.Location + new Point((int)moveAmount.X, (int)(-(int)moveAmount.Y)), e.WorldRect.Size), e.Scale, spriteRotation, spriteEffects);
						}
						GUI.DrawRectangle(spriteBatch, e.WorldRect.Center.ToVector2().FlipY() + moveAmount + new Vector2(0f, (float)e.WorldRect.Height), (float)e.WorldRect.Width, (float)e.WorldRect.Height, rectangleRotation, Color.White, 0f, Math.Max(3f / Screen.Selected.Cam.Zoom, 2f));
					}
					MapEntity.selectionPos = Vector2.Zero;
				}
			}
			if (MapEntity.selectionPos != Vector2.Zero)
			{
				MapEntity.DrawSelectionRect(spriteBatch, MapEntity.selectionPos, MapEntity.selectionSize, GUIStyle.Blue);
			}
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x00142234 File Offset: 0x00140434
		public static void DrawSelectionRect(SpriteBatch spriteBatch, Vector2 pos, Vector2 size, Color color)
		{
			Vector2 vector = size;
			float num;
			float num2;
			vector.Deconstruct(out num, out num2);
			float sizeX = num;
			float sizeY = num2;
			vector = pos;
			vector.Deconstruct(out num2, out num);
			float posX = num2;
			float posY = num;
			posY = -posY;
			Vector2[] corners = new Vector2[]
			{
				new Vector2(posX, posY),
				new Vector2(posX + sizeX, posY),
				new Vector2(posX + sizeX, posY + sizeY),
				new Vector2(posX, posY + sizeY)
			};
			float thickness = Math.Max(2f, 2f / Screen.Selected.Cam.Zoom);
			GUI.DrawFilledRectangle(spriteBatch, corners[0], size, color * 0.1f, 0f);
			Vector2 offset = new Vector2(0f, thickness / 2f);
			if (sizeY < 0f)
			{
				offset.Y = -offset.Y;
			}
			spriteBatch.DrawLine(corners[0], corners[1], color, thickness);
			spriteBatch.DrawLine(corners[1] - offset, corners[2] + offset, color, thickness);
			spriteBatch.DrawLine(corners[2], corners[3], color, thickness);
			spriteBatch.DrawLine(corners[3] + offset, corners[0] - offset, color, thickness);
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x001423A0 File Offset: 0x001405A0
		protected static void ColorFlipButton(GUIButton btn, bool flip)
		{
			Color color = flip ? GUIStyle.Green : Color.White;
			Vector3 hsv = ToolBox.RGBToHSV(color);
			Vector3 hsvBase = hsv;
			hsvBase.Y *= 4f;
			hsvBase.Z *= 0.8f;
			btn.Color = ToolBoxCore.HSVToRGB(hsvBase.X, hsvBase.Y, hsvBase.Z);
			btn.SelectedColor = ToolBoxCore.HSVToRGB(hsvBase.X, hsvBase.Y, hsvBase.Z);
			Vector3 hsvHover = hsv;
			hsvHover.Z *= 1.2f;
			btn.HoverColor = ToolBoxCore.HSVToRGB(hsvHover.X, hsvHover.Y, hsvHover.Z);
		}

		// Token: 0x1700088F RID: 2191
		// (get) Token: 0x06001FF4 RID: 8180 RVA: 0x00142454 File Offset: 0x00140654
		// (set) Token: 0x06001FF5 RID: 8181 RVA: 0x0014245B File Offset: 0x0014065B
		public static List<MapEntity> FilteredSelectedList { get; private set; } = new List<MapEntity>();

		// Token: 0x06001FF6 RID: 8182 RVA: 0x00142464 File Offset: 0x00140664
		public static void UpdateEditor(Camera cam, float deltaTime)
		{
			if (MapEntity.highlightedListBox != null)
			{
				MapEntity.highlightedListBox.UpdateManually(deltaTime, false, true);
			}
			if (MapEntity.editingHUD != null && (MapEntity.FilteredSelectedList.Count == 0 || MapEntity.editingHUD.UserData != MapEntity.FilteredSelectedList[0]))
			{
				foreach (GUIComponent component in MapEntity.editingHUD.Children)
				{
					GUITextBox textBox = component as GUITextBox;
					if (textBox != null)
					{
						textBox.Deselect();
					}
				}
				MapEntity.editingHUD = null;
			}
			MapEntity.FilteredSelectedList.Clear();
			if (MapEntity.SelectedList.Count == 0)
			{
				return;
			}
			foreach (MapEntity e in MapEntity.SelectedList)
			{
				Gap gap = e as Gap;
				if (gap == null || gap.ConnectedDoor == null)
				{
					MapEntity.FilteredSelectedList.Add(e);
				}
			}
			MapEntity first = MapEntity.FilteredSelectedList.FirstOrDefault<MapEntity>();
			if (first != null)
			{
				first.UpdateEditing(cam, deltaTime);
				if (first.ResizeHorizontal || first.ResizeVertical)
				{
					first.UpdateResizing(cam);
				}
			}
			if (PlayerInput.IsCtrlDown())
			{
				if (PlayerInput.KeyHit(Keys.N))
				{
					MapEntity firstSelected = MapEntity.SelectedList.First<MapEntity>();
					float minX = (float)firstSelected.WorldRect.X;
					float maxX = (float)firstSelected.WorldRect.Right;
					foreach (MapEntity entity in MapEntity.SelectedList)
					{
						minX = Math.Min(minX, (float)entity.WorldRect.X);
						maxX = Math.Max(maxX, (float)entity.WorldRect.Right);
					}
					float centerX = (minX + maxX) / 2f;
					using (HashSet<MapEntity>.Enumerator enumerator4 = MapEntity.SelectedList.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							MapEntity me = enumerator4.Current;
							me.FlipX(false, false);
							me.Move(new Vector2((centerX - me.WorldPosition.X) * 2f, 0f), true);
						}
						return;
					}
				}
				if (PlayerInput.KeyHit(Keys.M))
				{
					MapEntity firstSelected2 = MapEntity.SelectedList.First<MapEntity>();
					float minY = (float)(firstSelected2.WorldRect.Y - firstSelected2.WorldRect.Height);
					float maxY = (float)firstSelected2.WorldRect.Y;
					foreach (MapEntity entity2 in MapEntity.SelectedList)
					{
						minY = Math.Min(minY, (float)(entity2.WorldRect.Y - entity2.WorldRect.Height));
						maxY = Math.Max(maxY, (float)entity2.WorldRect.Y);
					}
					float centerY = (minY + maxY) / 2f;
					foreach (MapEntity me2 in MapEntity.SelectedList)
					{
						me2.FlipY(false, false);
						me2.Move(new Vector2(0f, (centerY - me2.WorldPosition.Y) * 2f), true);
					}
				}
			}
		}

		// Token: 0x06001FF7 RID: 8183 RVA: 0x00142804 File Offset: 0x00140A04
		public static void ResetEditingHUD()
		{
			MapEntity.editingHUD = null;
		}

		// Token: 0x06001FF8 RID: 8184 RVA: 0x0014280C File Offset: 0x00140A0C
		public static void DrawEditor(SpriteBatch spriteBatch, Camera cam)
		{
			if (MapEntity.SelectedList.Count == 1)
			{
				MapEntity firstSelected = MapEntity.SelectedList.First<MapEntity>();
				firstSelected.DrawEditing(spriteBatch, cam);
				if (firstSelected.ResizeHorizontal || firstSelected.ResizeVertical)
				{
					firstSelected.DrawResizing(spriteBatch, cam);
				}
			}
		}

		// Token: 0x06001FF9 RID: 8185 RVA: 0x00142851 File Offset: 0x00140A51
		public static void DeselectAll()
		{
			MapEntity.SelectedList.Clear();
		}

		// Token: 0x06001FFA RID: 8186 RVA: 0x0014285D File Offset: 0x00140A5D
		public static void SelectEntity(MapEntity entity)
		{
			MapEntity.DeselectAll();
			MapEntity.AddSelection(entity);
		}

		// Token: 0x06001FFB RID: 8187 RVA: 0x0014286A File Offset: 0x00140A6A
		public static void Copy(List<MapEntity> entities)
		{
			if (entities.Count == 0)
			{
				return;
			}
			MapEntity.CopyEntities(entities);
		}

		// Token: 0x06001FFC RID: 8188 RVA: 0x0014287C File Offset: 0x00140A7C
		public static void Cut(List<MapEntity> entities)
		{
			if (entities.Count == 0)
			{
				return;
			}
			MapEntity.CopyEntities(entities);
			SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>(entities), true, true));
			entities.ForEach(delegate(MapEntity e)
			{
				if (!e.Removed)
				{
					e.Remove();
				}
			});
			entities.Clear();
		}

		// Token: 0x06001FFD RID: 8189 RVA: 0x001428D8 File Offset: 0x00140AD8
		public static void Paste(Vector2 position)
		{
			if (MapEntity.CopiedList.Count == 0)
			{
				return;
			}
			List<MapEntity> prevEntities = new List<MapEntity>(MapEntity.MapEntityList);
			MapEntity.Clone(MapEntity.CopiedList);
			List<MapEntity> clones = MapEntity.MapEntityList.Except(prevEntities).ToList<MapEntity>();
			IEnumerable<MapEntity> nonWireClones = clones.Where(delegate(MapEntity c)
			{
				Item item2 = c as Item;
				return item2 == null || item2.GetComponent<Wire>() == null;
			});
			if (!nonWireClones.Any<MapEntity>())
			{
				nonWireClones = clones;
			}
			Vector2 center = Vector2.Zero;
			nonWireClones.ForEach(delegate(MapEntity c)
			{
				center += c.WorldPosition;
			});
			center = Submarine.VectorToWorldGrid(center / (float)nonWireClones.Count<MapEntity>(), null, false);
			Vector2 moveAmount = Submarine.VectorToWorldGrid(position - center, null, false);
			MapEntity.SelectedList = new HashSet<MapEntity>(clones);
			foreach (MapEntity clone in MapEntity.SelectedList)
			{
				clone.Move(moveAmount, true);
				clone.Submarine = Submarine.MainSub;
			}
			foreach (MapEntity clone2 in MapEntity.SelectedList)
			{
				Item item = clone2 as Item;
				if (item != null)
				{
					ItemContainer component = item.GetComponent<ItemContainer>();
					if (component != null)
					{
						component.SetContainedItemPositions();
					}
				}
			}
			SubEditorScreen.StoreCommand(new AddOrDeleteCommand(clones, false, false));
			SubEditorScreen subEditor = Screen.Selected as SubEditorScreen;
			if (subEditor != null)
			{
				subEditor.ReconstructLayers();
			}
		}

		// Token: 0x06001FFE RID: 8190 RVA: 0x00142A80 File Offset: 0x00140C80
		public static List<MapEntity> CopyEntities(List<MapEntity> entities)
		{
			List<MapEntity> prevEntities = new List<MapEntity>(MapEntity.MapEntityList);
			MapEntity.CopiedList = MapEntity.Clone(entities);
			List<MapEntity> newEntities = MapEntity.MapEntityList.Except(prevEntities).ToList<MapEntity>();
			newEntities.ForEach(delegate(MapEntity e)
			{
				e.ShallowRemove();
			});
			return newEntities;
		}

		// Token: 0x06001FFF RID: 8191 RVA: 0x00142ADA File Offset: 0x00140CDA
		public virtual void AddToGUIUpdateList(int order = 0)
		{
			if (MapEntity.editingHUD != null && MapEntity.editingHUD.UserData == this)
			{
				MapEntity.editingHUD.AddToGUIUpdateList(false, order);
			}
		}

		// Token: 0x06002000 RID: 8192 RVA: 0x00142AFC File Offset: 0x00140CFC
		public virtual void UpdateEditing(Camera cam, float deltaTime)
		{
		}

		// Token: 0x06002001 RID: 8193 RVA: 0x00142B00 File Offset: 0x00140D00
		protected static void PositionEditingHUD()
		{
			int maxHeight = (Screen.Selected == GameMain.SubEditorScreen) ? (GameMain.GraphicsHeight - GameMain.SubEditorScreen.EntityMenu.Rect.Height - GameMain.SubEditorScreen.TopPanel.Rect.Bottom * 2 - 20) : (HUDLayoutSettings.InventoryAreaLower.Y - HUDLayoutSettings.CrewArea.Bottom - 10);
			GUIListBox listBox = MapEntity.editingHUD.GetChild<GUIListBox>();
			if (listBox != null)
			{
				int padding = 20;
				int contentHeight = 0;
				foreach (GUIComponent child in listBox.Content.Children)
				{
					contentHeight += child.Rect.Height + listBox.Spacing;
					child.RectTransform.MaxSize = new Point(int.MaxValue, child.Rect.Height);
					child.RectTransform.MinSize = new Point(0, child.Rect.Height);
				}
				MapEntity.editingHUD.RectTransform.Resize(new Point(MapEntity.editingHUD.RectTransform.NonScaledSize.X, MathHelper.Clamp(contentHeight + padding * 2, 50, maxHeight)), false);
				listBox.RectTransform.Resize(new Point(listBox.RectTransform.NonScaledSize.X, MapEntity.editingHUD.RectTransform.NonScaledSize.Y - padding * 2), false);
			}
			MapEntity.editingHUD.RectTransform.SetPosition(Anchor.TopRight, null);
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				MapEntity.editingHUD.RectTransform.AbsoluteOffset = new Point(0, GameMain.SubEditorScreen.TopPanel.Rect.Bottom);
				return;
			}
			MapEntity.editingHUD.RectTransform.AbsoluteOffset = new Point(0, HUDLayoutSettings.HealthBarAfflictionArea.Y - MapEntity.editingHUD.Rect.Height - GUI.IntScale(10f));
		}

		// Token: 0x06002002 RID: 8194 RVA: 0x00142D20 File Offset: 0x00140F20
		public virtual void DrawEditing(SpriteBatch spriteBatch, Camera cam)
		{
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x00142D24 File Offset: 0x00140F24
		private Vector2 GetEditingHandlePos(int x, int y, Camera cam)
		{
			Vector2 handleDiff = new Vector2((float)x * ((float)this.rect.Width * 0.5f), (float)y * ((float)this.rect.Height * 0.5f));
			float rotation = -this.RotationRad;
			handleDiff = MathUtils.RotatePoint(handleDiff, rotation);
			if (this.FlippedX)
			{
				handleDiff = handleDiff.FlipX();
			}
			if (this.FlippedY)
			{
				handleDiff = handleDiff.FlipY();
			}
			return cam.WorldToScreen(this.Position + handleDiff);
		}

		// Token: 0x17000890 RID: 2192
		// (get) Token: 0x06002004 RID: 8196 RVA: 0x00142DA2 File Offset: 0x00140FA2
		private float ResizeHandleSize
		{
			get
			{
				return 10f * GUI.Scale;
			}
		}

		// Token: 0x17000891 RID: 2193
		// (get) Token: 0x06002005 RID: 8197 RVA: 0x00142DAF File Offset: 0x00140FAF
		private float ResizeHandleHighlightDistance
		{
			get
			{
				return 8f * GUI.Scale;
			}
		}

		// Token: 0x06002006 RID: 8198 RVA: 0x00142DBC File Offset: 0x00140FBC
		private void UpdateResizing(Camera cam)
		{
			this.IsHighlighted = true;
			int startX = this.ResizeHorizontal ? -1 : 0;
			int startY = this.ResizeVertical ? -1 : 0;
			for (int x = startX; x < 2; x += 2)
			{
				for (int y = startY; y < 2; y += 2)
				{
					Vector2 handlePos = this.GetEditingHandlePos(x, y, cam);
					bool highlighted = Vector2.DistanceSquared(PlayerInput.MousePosition, handlePos) < this.ResizeHandleHighlightDistance * this.ResizeHandleHighlightDistance;
					if (highlighted && PlayerInput.PrimaryMouseButtonDown())
					{
						MapEntity.selectionPos = Vector2.Zero;
						this.resizeDirX = x;
						this.resizeDirY = y;
						MapEntity.Resizing = true;
						MapEntity.startMovingPos = Vector2.Zero;
						MapEntity.ClearHighlightedEntities();
					}
				}
			}
			if (MapEntity.Resizing)
			{
				if (this.prevRect == null)
				{
					this.prevRect = new Rectangle?(this.Rect);
				}
				Vector2 placePosition = this.prevRect.Value.Location.ToVector2();
				Vector2 placeSize = this.prevRect.Value.Size.ToVector2();
				Vector2 mousePos = cam.ScreenToWorld(PlayerInput.MousePosition);
				Vector2 prevPos = placePosition;
				Vector2 prevOppositeCorner = prevPos + placeSize.FlipY();
				Vector2 prevCenter = placePosition + placeSize.FlipY() * 0.5f;
				mousePos = MapEntity.<UpdateResizing>g__flipThenRotate|89_0(mousePos, prevCenter, this.RotationRad, this.FlippedX, this.FlippedY);
				if (!PlayerInput.IsShiftDown())
				{
					mousePos = Submarine.VectorToWorldGrid(mousePos, Submarine.MainSub, true);
				}
				if (this.resizeDirX > 0)
				{
					mousePos.X = Math.Max(mousePos.X, (float)this.prevRect.Value.X + Submarine.GridSize.X);
					placeSize.X = mousePos.X - placePosition.X;
				}
				else if (this.resizeDirX < 0)
				{
					mousePos.X = Math.Min(mousePos.X, (float)this.prevRect.Value.Right - Submarine.GridSize.X);
					placeSize.X = MathF.Round(placePosition.X + placeSize.X - mousePos.X);
					placePosition.X = MathF.Round(mousePos.X);
				}
				if (this.resizeDirY < 0)
				{
					mousePos.Y = Math.Min(mousePos.Y, (float)this.prevRect.Value.Y - Submarine.GridSize.Y);
					placeSize.Y = placePosition.Y - mousePos.Y;
				}
				else if (this.resizeDirY > 0)
				{
					mousePos.Y = Math.Max(mousePos.Y, (float)(this.prevRect.Value.Y - this.prevRect.Value.Height) + Submarine.GridSize.Y);
					placeSize.Y = mousePos.Y - (float)(this.prevRect.Value.Y - this.prevRect.Value.Height);
					placePosition.Y = mousePos.Y;
				}
				Vector2 newPos = placePosition;
				Vector2 newOppositeCorner = placePosition + placeSize.FlipY();
				Vector2 transformedCornerDiff = MapEntity.<UpdateResizing>g__rotateThenFlip|89_1(newPos - prevPos, Vector2.Zero, -this.RotationRad, this.FlippedX, this.FlippedY);
				Vector2 transformedOppositeCornerDiff = MapEntity.<UpdateResizing>g__rotateThenFlip|89_1(newOppositeCorner - prevOppositeCorner, Vector2.Zero, -this.RotationRad, this.FlippedX, this.FlippedY);
				Vector2 newPosTransformed = MapEntity.<UpdateResizing>g__rotateThenFlip|89_1(prevPos, prevCenter, -this.RotationRad, this.FlippedX, this.FlippedY) + transformedCornerDiff;
				Vector2 newOppositeTransformed = MapEntity.<UpdateResizing>g__rotateThenFlip|89_1(prevOppositeCorner, prevCenter, -this.RotationRad, this.FlippedX, this.FlippedY) + transformedOppositeCornerDiff;
				Vector2 newTransformedCenter = (newPosTransformed + newOppositeTransformed) * 0.5f;
				Vector2 newDiff = (newOppositeCorner - newPos) * 0.5f;
				placePosition = newTransformedCenter - newDiff;
				if ((int)placePosition.X != this.rect.X || (int)placePosition.Y != this.rect.Y || (int)placeSize.X != this.rect.Width || (int)placeSize.Y != this.rect.Height)
				{
					this.Rect = new Rectangle((int)placePosition.X, (int)placePosition.Y, (int)placeSize.X, (int)placeSize.Y);
				}
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					MapEntity.Resizing = false;
					Action<Rectangle> resized = this.Resized;
					if (resized != null)
					{
						resized(this.rect);
					}
					if (this.prevRect != null)
					{
						List<Rectangle> newData = new List<Rectangle>
						{
							this.Rect
						};
						List<Rectangle> oldData = new List<Rectangle>
						{
							this.prevRect.Value
						};
						SubEditorScreen.StoreCommand(new TransformCommand(new List<MapEntity>
						{
							this
						}, newData, oldData, true));
					}
					Structure structure = this as Structure;
					if (structure != null)
					{
						foreach (LightSource light in structure.Lights)
						{
							light.LightTextureTargetSize = this.Rect.Size.ToVector2();
							light.Position = this.rect.Location.ToVector2();
						}
					}
					this.prevRect = null;
				}
			}
		}

		// Token: 0x06002007 RID: 8199 RVA: 0x00143344 File Offset: 0x00141544
		private void DrawResizing(SpriteBatch spriteBatch, Camera cam)
		{
			this.IsHighlighted = true;
			int startX = this.ResizeHorizontal ? -1 : 0;
			int startY = this.ResizeVertical ? -1 : 0;
			for (int x = startX; x < 2; x += 2)
			{
				for (int y = startY; y < 2; y += 2)
				{
					Vector2 handlePos = this.GetEditingHandlePos(x, y, cam);
					bool highlighted = Vector2.DistanceSquared(PlayerInput.MousePosition, handlePos) < this.ResizeHandleHighlightDistance * this.ResizeHandleHighlightDistance;
					Color color = Color.White * (highlighted ? 1f : 0.6f);
					if (highlighted && !PlayerInput.PrimaryMouseButtonHeld())
					{
						GUI.MouseCursor = CursorState.Hand;
					}
					GUI.DrawRectangle(spriteBatch, handlePos - new Vector2(this.ResizeHandleSize / 2f), new Vector2(this.ResizeHandleSize), color, true, 0f, (float)((int)Math.Max(1.5f / Screen.Selected.Cam.Zoom, 1f)));
				}
			}
		}

		// Token: 0x06002008 RID: 8200 RVA: 0x00143440 File Offset: 0x00141640
		public static HashSet<MapEntity> FindSelectedEntities(Vector2 pos, Vector2 size)
		{
			HashSet<MapEntity> foundEntities = new HashSet<MapEntity>();
			Rectangle selectionRect = Submarine.AbsRect(pos, size);
			Quad2D selectionQuad = Quad2D.FromSubmarineRectangle(selectionRect);
			Func<MapEntity, bool> <>9__0;
			foreach (MapEntity entity in MapEntity.MapEntityList)
			{
				if (entity.SelectableInEditor)
				{
					Quad2D entityQuad = entity.GetTransformedQuad();
					if (selectionQuad.Intersects(entityQuad))
					{
						foundEntities.Add(entity);
						entity.IsIncludedInSelection = true;
						if (SubEditorScreen.IsLayerLinked(entity))
						{
							ImmutableHashSet<MapEntity> entitiesInSameLayer = SubEditorScreen.GetEntitiesInSameLayer(entity);
							IEnumerable<MapEntity> source = entitiesInSameLayer;
							Func<MapEntity, bool> predicate;
							if ((predicate = <>9__0) == null)
							{
								predicate = (<>9__0 = ((MapEntity e) => !foundEntities.Contains(e)));
							}
							foreach (MapEntity layerEntity in source.Where(predicate))
							{
								foundEntities.Add(layerEntity);
								layerEntity.IsIncludedInSelection = true;
							}
						}
					}
				}
			}
			return foundEntities;
		}

		// Token: 0x17000892 RID: 2194
		// (get) Token: 0x06002009 RID: 8201 RVA: 0x0014357C File Offset: 0x0014177C
		// (set) Token: 0x0600200A RID: 8202 RVA: 0x00143590 File Offset: 0x00141790
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string DisallowedUpgrades
		{
			get
			{
				return string.Join<Identifier>(",", this.DisallowedUpgradeSet);
			}
			set
			{
				this.DisallowedUpgradeSet.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitTags = value.Split(',', StringSplitOptions.None);
					foreach (string tag in splitTags)
					{
						string[] splitTag = tag.Trim().Split(':', StringSplitOptions.None);
						this.DisallowedUpgradeSet.Add(string.Join(":", splitTag).ToIdentifier());
					}
				}
			}
		}

		// Token: 0x17000893 RID: 2195
		// (get) Token: 0x0600200B RID: 8203 RVA: 0x001435FB File Offset: 0x001417FB
		// (set) Token: 0x0600200C RID: 8204 RVA: 0x00143603 File Offset: 0x00141803
		public bool FlippedX { get; protected set; }

		// Token: 0x17000894 RID: 2196
		// (get) Token: 0x0600200D RID: 8205 RVA: 0x0014360C File Offset: 0x0014180C
		// (set) Token: 0x0600200E RID: 8206 RVA: 0x00143614 File Offset: 0x00141814
		public bool FlippedY { get; protected set; }

		// Token: 0x17000895 RID: 2197
		// (get) Token: 0x0600200F RID: 8207 RVA: 0x0014361D File Offset: 0x0014181D
		public static IEnumerable<MapEntity> HighlightedEntities
		{
			get
			{
				return MapEntity.highlightedEntities;
			}
		}

		// Token: 0x17000896 RID: 2198
		// (get) Token: 0x06002010 RID: 8208 RVA: 0x00143624 File Offset: 0x00141824
		// (set) Token: 0x06002011 RID: 8209 RVA: 0x0014362C File Offset: 0x0014182C
		public bool ExternalHighlight
		{
			get
			{
				return this.externalHighlight;
			}
			set
			{
				if (value != this.externalHighlight)
				{
					this.externalHighlight = value;
					this.CheckIsHighlighted();
				}
			}
		}

		// Token: 0x17000897 RID: 2199
		// (get) Token: 0x06002012 RID: 8210 RVA: 0x00143644 File Offset: 0x00141844
		// (set) Token: 0x06002013 RID: 8211 RVA: 0x00143656 File Offset: 0x00141856
		public bool IsHighlighted
		{
			get
			{
				return this.isHighlighted || this.ExternalHighlight;
			}
			set
			{
				if (value != this.isHighlighted)
				{
					this.isHighlighted = value;
					this.CheckIsHighlighted();
				}
			}
		}

		// Token: 0x17000898 RID: 2200
		// (get) Token: 0x06002014 RID: 8212 RVA: 0x0014366E File Offset: 0x0014186E
		// (set) Token: 0x06002015 RID: 8213 RVA: 0x00143676 File Offset: 0x00141876
		public virtual float RotationRad { get; protected set; }

		// Token: 0x17000899 RID: 2201
		// (get) Token: 0x06002016 RID: 8214 RVA: 0x0014367F File Offset: 0x0014187F
		public float RotationRadWithFlipping
		{
			get
			{
				if (!(this.FlippedX ^ this.FlippedY))
				{
					return this.RotationRad;
				}
				return -this.RotationRad;
			}
		}

		// Token: 0x1700089A RID: 2202
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x0014369E File Offset: 0x0014189E
		public float RotationWithFlipping
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRadWithFlipping);
			}
		}

		// Token: 0x1700089B RID: 2203
		// (get) Token: 0x06002018 RID: 8216 RVA: 0x001436AB File Offset: 0x001418AB
		// (set) Token: 0x06002019 RID: 8217 RVA: 0x001436B3 File Offset: 0x001418B3
		public virtual Rectangle Rect
		{
			get
			{
				return this.rect;
			}
			set
			{
				this.rect = value;
			}
		}

		// Token: 0x1700089C RID: 2204
		// (get) Token: 0x0600201A RID: 8218 RVA: 0x001436BC File Offset: 0x001418BC
		public Rectangle WorldRect
		{
			get
			{
				if (base.Submarine != null)
				{
					return new Rectangle((int)(base.Submarine.Position.X + (float)this.rect.X), (int)(base.Submarine.Position.Y + (float)this.rect.Y), this.rect.Width, this.rect.Height);
				}
				return this.rect;
			}
		}

		// Token: 0x1700089D RID: 2205
		// (get) Token: 0x0600201B RID: 8219 RVA: 0x0014372F File Offset: 0x0014192F
		public virtual Sprite Sprite
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700089E RID: 2206
		// (get) Token: 0x0600201C RID: 8220 RVA: 0x00143732 File Offset: 0x00141932
		public virtual bool DrawBelowWater
		{
			get
			{
				return this.Sprite != null && this.SpriteDepth > 0.5f;
			}
		}

		// Token: 0x1700089F RID: 2207
		// (get) Token: 0x0600201D RID: 8221 RVA: 0x0014374B File Offset: 0x0014194B
		public virtual bool DrawOverWater
		{
			get
			{
				return !this.DrawBelowWater;
			}
		}

		// Token: 0x170008A0 RID: 2208
		// (get) Token: 0x0600201E RID: 8222 RVA: 0x00143756 File Offset: 0x00141956
		public virtual bool Linkable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170008A1 RID: 2209
		// (get) Token: 0x0600201F RID: 8223 RVA: 0x0014375C File Offset: 0x0014195C
		public IEnumerable<Identifier> AllowedLinks
		{
			get
			{
				if (this.Prefab != null)
				{
					return this.Prefab.AllowedLinks;
				}
				return Enumerable.Empty<Identifier>();
			}
		}

		// Token: 0x170008A2 RID: 2210
		// (get) Token: 0x06002020 RID: 8224 RVA: 0x00143784 File Offset: 0x00141984
		public bool ResizeHorizontal
		{
			get
			{
				return this.Prefab != null && this.Prefab.ResizeHorizontal;
			}
		}

		// Token: 0x170008A3 RID: 2211
		// (get) Token: 0x06002021 RID: 8225 RVA: 0x0014379B File Offset: 0x0014199B
		public bool ResizeVertical
		{
			get
			{
				return this.Prefab != null && this.Prefab.ResizeVertical;
			}
		}

		// Token: 0x170008A4 RID: 2212
		// (get) Token: 0x06002022 RID: 8226 RVA: 0x001437B2 File Offset: 0x001419B2
		// (set) Token: 0x06002023 RID: 8227 RVA: 0x001437BF File Offset: 0x001419BF
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int RectWidth
		{
			get
			{
				return this.rect.Width;
			}
			set
			{
				if (value <= 0)
				{
					return;
				}
				this.Rect = new Rectangle(this.rect.X, this.rect.Y, value, this.rect.Height);
			}
		}

		// Token: 0x170008A5 RID: 2213
		// (get) Token: 0x06002024 RID: 8228 RVA: 0x001437F3 File Offset: 0x001419F3
		// (set) Token: 0x06002025 RID: 8229 RVA: 0x00143800 File Offset: 0x00141A00
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int RectHeight
		{
			get
			{
				return this.rect.Height;
			}
			set
			{
				if (value <= 0)
				{
					return;
				}
				this.Rect = new Rectangle(this.rect.X, this.rect.Y, this.rect.Width, value);
			}
		}

		// Token: 0x170008A6 RID: 2214
		// (get) Token: 0x06002026 RID: 8230 RVA: 0x00143834 File Offset: 0x00141A34
		// (set) Token: 0x06002027 RID: 8231 RVA: 0x0014383C File Offset: 0x00141A3C
		public bool SpriteDepthOverrideIsSet { get; private set; }

		// Token: 0x170008A7 RID: 2215
		// (get) Token: 0x06002028 RID: 8232 RVA: 0x00143845 File Offset: 0x00141A45
		public float SpriteOverrideDepth
		{
			get
			{
				return this.SpriteDepth;
			}
		}

		// Token: 0x170008A8 RID: 2216
		// (get) Token: 0x06002029 RID: 8233 RVA: 0x0014384D File Offset: 0x00141A4D
		// (set) Token: 0x0600202A RID: 8234 RVA: 0x00143878 File Offset: 0x00141A78
		[Editable(0.001f, 0.999f, 3)]
		[Serialize(float.NaN, IsPropertySaveable.Yes, "", "", false)]
		public float SpriteDepth
		{
			get
			{
				if (this.SpriteDepthOverrideIsSet)
				{
					return this._spriteOverrideDepth;
				}
				if (this.Sprite == null)
				{
					return 0f;
				}
				return this.Sprite.Depth;
			}
			set
			{
				if (!float.IsNaN(value))
				{
					this._spriteOverrideDepth = MathHelper.Clamp(value, 0.001f, 0.999999f);
					if (this is Item)
					{
						this._spriteOverrideDepth = Math.Min(this._spriteOverrideDepth, 0.9f);
					}
					this.SpriteDepthOverrideIsSet = true;
				}
			}
		}

		// Token: 0x170008A9 RID: 2217
		// (get) Token: 0x0600202B RID: 8235 RVA: 0x001438C8 File Offset: 0x00141AC8
		// (set) Token: 0x0600202C RID: 8236 RVA: 0x001438D0 File Offset: 0x00141AD0
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(0.01f, 10f, 1, DecimalCount = 3, ValueStep = 0.1f)]
		public virtual float Scale { get; set; } = 1f;

		// Token: 0x170008AA RID: 2218
		// (get) Token: 0x0600202D RID: 8237 RVA: 0x001438D9 File Offset: 0x00141AD9
		// (set) Token: 0x0600202E RID: 8238 RVA: 0x001438E1 File Offset: 0x00141AE1
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HiddenInGame { get; set; }

		// Token: 0x170008AB RID: 2219
		// (get) Token: 0x0600202F RID: 8239 RVA: 0x001438EA File Offset: 0x00141AEA
		// (set) Token: 0x06002030 RID: 8240 RVA: 0x001438F2 File Offset: 0x00141AF2
		public bool IsLayerHidden { get; set; }

		// Token: 0x170008AC RID: 2220
		// (get) Token: 0x06002031 RID: 8241 RVA: 0x001438FB File Offset: 0x00141AFB
		public bool IsHidden
		{
			get
			{
				return this.HiddenInGame || this.IsLayerHidden;
			}
		}

		// Token: 0x170008AD RID: 2221
		// (get) Token: 0x06002032 RID: 8242 RVA: 0x00143910 File Offset: 0x00141B10
		public override Vector2 Position
		{
			get
			{
				Vector2 rectPos = new Vector2((float)this.rect.X + (float)this.rect.Width / 2f, (float)this.rect.Y - (float)this.rect.Height / 2f);
				return rectPos;
			}
		}

		// Token: 0x170008AE RID: 2222
		// (get) Token: 0x06002033 RID: 8243 RVA: 0x00143963 File Offset: 0x00141B63
		public override Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x170008AF RID: 2223
		// (get) Token: 0x06002034 RID: 8244 RVA: 0x00143970 File Offset: 0x00141B70
		// (set) Token: 0x06002035 RID: 8245 RVA: 0x0014398B File Offset: 0x00141B8B
		public float SoundRange
		{
			get
			{
				if (this.aiTarget == null)
				{
					return 0f;
				}
				return this.aiTarget.SoundRange;
			}
			set
			{
				if (this.aiTarget == null)
				{
					return;
				}
				this.aiTarget.SoundRange = value;
			}
		}

		// Token: 0x170008B0 RID: 2224
		// (get) Token: 0x06002036 RID: 8246 RVA: 0x001439A2 File Offset: 0x00141BA2
		// (set) Token: 0x06002037 RID: 8247 RVA: 0x001439BD File Offset: 0x00141BBD
		public float SightRange
		{
			get
			{
				if (this.aiTarget == null)
				{
					return 0f;
				}
				return this.aiTarget.SightRange;
			}
			set
			{
				if (this.aiTarget == null)
				{
					return;
				}
				this.aiTarget.SightRange = value;
			}
		}

		// Token: 0x170008B1 RID: 2225
		// (get) Token: 0x06002038 RID: 8248 RVA: 0x001439D4 File Offset: 0x00141BD4
		// (set) Token: 0x06002039 RID: 8249 RVA: 0x001439DC File Offset: 0x00141BDC
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool RemoveIfLinkedOutpostDoorInUse { get; protected set; } = true;

		// Token: 0x170008B2 RID: 2226
		// (get) Token: 0x0600203A RID: 8250 RVA: 0x001439E5 File Offset: 0x00141BE5
		// (set) Token: 0x0600203B RID: 8251 RVA: 0x001439ED File Offset: 0x00141BED
		[Serialize("", IsPropertySaveable.Yes, "Submarine editor layer", "", false)]
		public string Layer { get; set; }

		// Token: 0x170008B3 RID: 2227
		// (get) Token: 0x0600203C RID: 8252 RVA: 0x001439F6 File Offset: 0x00141BF6
		public virtual string Name
		{
			get
			{
				return "";
			}
		}

		// Token: 0x0600203D RID: 8253 RVA: 0x00143A00 File Offset: 0x00141C00
		public MapEntity(MapEntityPrefab prefab, Submarine submarine, ushort id) : base(submarine, id)
		{
			this.Prefab = prefab;
			this.Scale = ((prefab != null) ? prefab.Scale : 1f);
		}

		// Token: 0x0600203E RID: 8254 RVA: 0x00143A88 File Offset: 0x00141C88
		protected void ParseLinks(XElement element, IdRemap idRemap)
		{
			string linkedToString = element.GetAttributeString("linked", "");
			if (!string.IsNullOrEmpty(linkedToString))
			{
				string[] linkedToIds = linkedToString.Split(',', StringSplitOptions.None);
				for (int i = 0; i < linkedToIds.Length; i++)
				{
					int srcId = int.Parse(linkedToIds[i]);
					int targetId = (int)idRemap.GetOffsetId(srcId);
					if (targetId <= 0)
					{
						if (this.unresolvedLinkedToID == null)
						{
							this.unresolvedLinkedToID = new List<ushort>();
						}
						this.unresolvedLinkedToID.Add((ushort)srcId);
					}
					else
					{
						this.linkedToID.Add((ushort)targetId);
					}
				}
			}
		}

		// Token: 0x0600203F RID: 8255 RVA: 0x00143B10 File Offset: 0x00141D10
		public void ResolveLinks(IdRemap childRemap)
		{
			if (this.unresolvedLinkedToID == null)
			{
				return;
			}
			for (int i = 0; i < this.unresolvedLinkedToID.Count; i++)
			{
				int srcId = (int)this.unresolvedLinkedToID[i];
				int targetId = (int)childRemap.GetOffsetId(srcId);
				if (targetId > 0)
				{
					MapEntity otherEntity = Entity.FindEntityByID((ushort)targetId) as MapEntity;
					this.linkedTo.Add(otherEntity);
					if (otherEntity.Linkable && otherEntity.linkedTo != null)
					{
						otherEntity.linkedTo.Add(this);
					}
					this.unresolvedLinkedToID.RemoveAt(i);
					i--;
				}
			}
		}

		// Token: 0x06002040 RID: 8256 RVA: 0x00143B9A File Offset: 0x00141D9A
		public virtual void Move(Vector2 amount, bool ignoreContacts = true)
		{
			this.rect.X = this.rect.X + (int)amount.X;
			this.rect.Y = this.rect.Y + (int)amount.Y;
		}

		// Token: 0x06002041 RID: 8257 RVA: 0x00143BC8 File Offset: 0x00141DC8
		public virtual bool IsMouseOn(Vector2 position)
		{
			return Submarine.RectContains(this.WorldRect, position, false);
		}

		// Token: 0x06002042 RID: 8258 RVA: 0x00143BD7 File Offset: 0x00141DD7
		public bool HasUpgrade(Identifier identifier)
		{
			return this.GetUpgrade(identifier) != null;
		}

		// Token: 0x06002043 RID: 8259 RVA: 0x00143BE4 File Offset: 0x00141DE4
		public Upgrade GetUpgrade(Identifier identifier)
		{
			return this.Upgrades.Find(delegate(Upgrade upgrade)
			{
				Identifier identifier2 = upgrade.Identifier;
				return identifier2 == identifier;
			});
		}

		// Token: 0x06002044 RID: 8260 RVA: 0x00143C15 File Offset: 0x00141E15
		public List<Upgrade> GetUpgrades()
		{
			return this.Upgrades;
		}

		// Token: 0x06002045 RID: 8261 RVA: 0x00143C20 File Offset: 0x00141E20
		public void SetUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			Upgrade existingUpgrade = this.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level = upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				this.AddUpgrade(upgrade, createNetworkEvent);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Set (ID: ");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")'s \"");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(upgrade.Prefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral("\" upgrade to level ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(upgrade.Level);
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06002046 RID: 8262 RVA: 0x00143CE4 File Offset: 0x00141EE4
		public virtual bool AddUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			if (!upgrade.Prefab.UpgradeCategories.Any((UpgradeCategory category) => category.CanBeApplied(this, upgrade.Prefab)))
			{
				return false;
			}
			if (this.DisallowedUpgradeSet.Contains(upgrade.Identifier))
			{
				return false;
			}
			Upgrade existingUpgrade = this.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level += upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				upgrade.ApplyUpgrade();
				this.Upgrades.Add(upgrade);
			}
			return true;
		}

		// Token: 0x06002047 RID: 8263 RVA: 0x00143DA1 File Offset: 0x00141FA1
		protected virtual void CheckIsHighlighted()
		{
			if (this.IsHighlighted || this.ExternalHighlight)
			{
				MapEntity.highlightedEntities.Add(this);
				return;
			}
			MapEntity.highlightedEntities.Remove(this);
		}

		// Token: 0x06002048 RID: 8264 RVA: 0x00143DCC File Offset: 0x00141FCC
		public static void ClearHighlightedEntities()
		{
			MapEntity.highlightedEntities.RemoveWhere((MapEntity e) => e.Removed);
			MapEntity.tempHighlightedEntities.Clear();
			MapEntity.tempHighlightedEntities.AddRange(MapEntity.highlightedEntities);
			foreach (MapEntity entity in MapEntity.tempHighlightedEntities)
			{
				entity.IsHighlighted = false;
			}
		}

		// Token: 0x06002049 RID: 8265
		public abstract MapEntity Clone();

		// Token: 0x0600204A RID: 8266 RVA: 0x00143E64 File Offset: 0x00142064
		public static List<MapEntity> Clone(List<MapEntity> entitiesToClone)
		{
			List<MapEntity> clones = new List<MapEntity>();
			foreach (MapEntity e2 in entitiesToClone)
			{
				try
				{
					clones.Add(e2.Clone());
				}
				catch (Exception ex)
				{
					DebugConsole.ThrowError("Cloning entity \"" + e2.Name + "\" failed.", ex, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:" + e2.Name, GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
					{
						"Cloning entity \"",
						e2.Name,
						"\" failed (",
						ex.Message,
						").\n",
						ex.StackTrace.CleanupStackTrace()
					}));
					return clones;
				}
			}
			for (int i = 0; i < clones.Count; i++)
			{
				if (entitiesToClone[i].linkedTo != null)
				{
					foreach (MapEntity linked in entitiesToClone[i].linkedTo)
					{
						if (entitiesToClone.Contains(linked))
						{
							clones[i].linkedTo.Add(clones[entitiesToClone.IndexOf(linked)]);
						}
					}
				}
			}
			List<Wire> orphanedWires = new List<Wire>();
			for (int j = 0; j < clones.Count; j++)
			{
				Item cloneItem = clones[j] as Item;
				if (cloneItem != null)
				{
					Door door = cloneItem.GetComponent<Door>();
					if (door != null)
					{
						door.RefreshLinkedGap();
					}
					Wire cloneWire = cloneItem.GetComponent<Wire>();
					if (cloneWire != null)
					{
						Wire originalWire = ((Item)entitiesToClone[j]).GetComponent<Wire>();
						cloneWire.SetNodes(originalWire.GetNodes());
						Predicate<MapEntity> <>9__2;
						for (int k = 0; k < 2; k++)
						{
							if (originalWire.Connections[k] == null)
							{
								Predicate<MapEntity> match;
								if ((match = <>9__2) == null)
								{
									match = (<>9__2 = delegate(MapEntity e)
									{
										Item item2 = e as Item;
										if (item2 != null)
										{
											ConnectionPanel component = item2.GetComponent<ConnectionPanel>();
											return component != null && component.DisconnectedWires.Contains(originalWire);
										}
										return false;
									});
								}
								MapEntity disconnectedFrom = entitiesToClone.Find(match);
								if (disconnectedFrom != null)
								{
									int disconnectedFromIndex = entitiesToClone.IndexOf(disconnectedFrom);
									Item item = clones[disconnectedFromIndex] as Item;
									ConnectionPanel disconnectedFromClone = (item != null) ? item.GetComponent<ConnectionPanel>() : null;
									if (disconnectedFromClone != null)
									{
										disconnectedFromClone.DisconnectedWires.Add(cloneWire);
										if (cloneWire.Item.body != null)
										{
											cloneWire.Item.body.Enabled = false;
										}
										cloneWire.IsActive = false;
									}
								}
							}
							else
							{
								Item connectedItem = originalWire.Connections[k].Item;
								if (connectedItem != null && entitiesToClone.Contains(connectedItem))
								{
									int itemIndex = entitiesToClone.IndexOf(connectedItem);
									if (itemIndex < 0)
									{
										DebugConsole.ThrowError("Error while cloning wires - item \"" + connectedItem.Name + "\" was not found in entities to clone.", null, null, false, false);
										GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:ConnectedNotFound" + connectedItem.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, "Error while cloning wires - item \"" + connectedItem.Name + "\" was not found in entities to clone.");
									}
									else
									{
										int connectionIndex = connectedItem.Connections.IndexOf(originalWire.Connections[k]);
										if (connectionIndex < 0)
										{
											DebugConsole.ThrowError(string.Concat(new string[]
											{
												"Error while cloning wires - connection \"",
												originalWire.Connections[k].Name,
												"\" was not found in connected item \"",
												connectedItem.Name,
												"\"."
											}), null, null, false, false);
											GameAnalyticsManager.AddErrorEventOnce("MapEntity.Clone:ConnectionNotFound" + connectedItem.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
											{
												"Error while cloning wires - connection \"",
												originalWire.Connections[k].Name,
												"\" was not found in connected item \"",
												connectedItem.Name,
												"\"."
											}));
										}
										else
										{
											(clones[itemIndex] as Item).Connections[connectionIndex].TryAddLink(cloneWire);
											cloneWire.Connect((clones[itemIndex] as Item).Connections[connectionIndex], k, false, false);
										}
									}
								}
							}
						}
						if (originalWire.Connections.Any((Connection c) => c != null) && (cloneWire.Connections[0] == null || cloneWire.Connections[1] == null) && cloneItem.GetComponent<DockingPort>() == null && !clones.Any(delegate(MapEntity c)
						{
							Item item2 = c as Item;
							bool? flag;
							if (item2 == null)
							{
								flag = null;
							}
							else
							{
								ConnectionPanel component = item2.GetComponent<ConnectionPanel>();
								flag = ((component != null) ? new bool?(component.DisconnectedWires.Contains(cloneWire)) : null);
							}
							bool? flag2 = flag;
							return flag2.GetValueOrDefault();
						}))
						{
							orphanedWires.Add(cloneWire);
						}
					}
				}
			}
			foreach (Wire orphanedWire in orphanedWires)
			{
				orphanedWire.Item.Remove();
				clones.Remove(orphanedWire.Item);
			}
			return clones;
		}

		// Token: 0x0600204B RID: 8267 RVA: 0x001443EC File Offset: 0x001425EC
		protected void InsertToList()
		{
			if (this.Sprite == null)
			{
				MapEntity.MapEntityList.Add(this);
				return;
			}
			int i = 0;
			Structure structure = this as Structure;
			if (structure != null && structure.DrawDamageEffect)
			{
				float drawDepth = structure.SpriteDepth;
				while (i < MapEntity.MapEntityList.Count)
				{
					Structure structure2 = MapEntity.MapEntityList[i] as Structure;
					float otherDrawDepth = (structure2 != null) ? structure2.SpriteDepth : 1f;
					if (otherDrawDepth < drawDepth)
					{
						break;
					}
					i++;
				}
				MapEntity.MapEntityList.Insert(i, this);
				return;
			}
			i = 0;
			while (i < MapEntity.MapEntityList.Count)
			{
				i++;
				MapEntity mapEntity = MapEntity.MapEntityList[i - 1];
				if (((mapEntity != null) ? mapEntity.Prefab : null) == this.Prefab)
				{
					MapEntity.MapEntityList.Insert(i, this);
					return;
				}
			}
			i = 0;
			while (i < MapEntity.MapEntityList.Count)
			{
				i++;
				Sprite existingSprite = MapEntity.MapEntityList[i - 1].Sprite;
				if (existingSprite != null && existingSprite.Texture == this.Sprite.Texture)
				{
					break;
				}
			}
			MapEntity.MapEntityList.Insert(i, this);
		}

		// Token: 0x0600204C RID: 8268 RVA: 0x001444FD File Offset: 0x001426FD
		public virtual void ShallowRemove()
		{
			base.Remove();
			MapEntity.MapEntityList.Remove(this);
			if (this.aiTarget != null)
			{
				this.aiTarget.Remove();
			}
		}

		// Token: 0x0600204D RID: 8269 RVA: 0x00144524 File Offset: 0x00142724
		public override void Remove()
		{
			base.Remove();
			MapEntity.MapEntityList.Remove(this);
			Submarine.ForceRemoveFromVisibleEntities(this);
			MapEntity.SelectedList.Remove(this);
			if (this.aiTarget != null)
			{
				this.aiTarget.Remove();
				this.aiTarget = null;
			}
			if (this.linkedTo != null)
			{
				for (int i = this.linkedTo.Count - 1; i >= 0; i--)
				{
					this.linkedTo[i].RemoveLinked(this);
				}
				this.linkedTo.Clear();
			}
		}

		// Token: 0x0600204E RID: 8270 RVA: 0x001445AC File Offset: 0x001427AC
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			MapEntity.mapEntityUpdateTick++;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				foreach (Hull hull in Hull.HullList)
				{
					hull.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
				}
				Hull.UpdateCheats(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
				foreach (Structure structure in Structure.WallList)
				{
					structure.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
				}
			}
			foreach (Gap gap in Gap.GapList)
			{
				gap.ResetWaterFlowThisFrame();
			}
			foreach (Gap gap2 in from g in Gap.GapList
			orderby Rand.Int(int.MaxValue, Rand.RandSync.Unsynced)
			select g)
			{
				gap2.Update(deltaTime, cam);
			}
			if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
			{
				Powered.UpdatePower(deltaTime * (float)MapEntity.PoweredUpdateInterval);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
			sw.Restart();
			Item.UpdatePendingConditionUpdates(deltaTime);
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				Item lastUpdatedItem = null;
				try
				{
					foreach (Item item in Item.ItemList)
					{
						if (!LuaCsSetup.Instance.Game.UpdatePriorityItems.Contains(item))
						{
							lastUpdatedItem = item;
							item.Update(deltaTime * (float)MapEntity.MapEntityUpdateInterval, cam);
						}
					}
				}
				catch (InvalidOperationException e)
				{
					GameAnalyticsManager.AddErrorEventOnce("MapEntity.UpdateAll:ItemUpdateInvalidOperation", GameAnalyticsManager.ErrorSeverity.Critical, "Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null") + ": " + e.Message);
					throw new InvalidOperationException("Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null"), e);
				}
			}
			foreach (Item item2 in LuaCsSetup.Instance.Game.UpdatePriorityItems)
			{
				if (!item2.Removed)
				{
					item2.Update(deltaTime, cam);
				}
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
			sw.Restart();
			if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
			{
				MapEntity.UpdateAllProjSpecific(deltaTime * (float)MapEntity.MapEntityUpdateInterval);
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.Update(true);
			}
		}

		// Token: 0x0600204F RID: 8271 RVA: 0x00144904 File Offset: 0x00142B04
		private static void UpdateAllProjSpecific(float deltaTime)
		{
			IEnumerable<MapEntity> entitiesToRender = Submarine.VisibleEntities ?? MapEntity.MapEntityList;
			foreach (MapEntity me in entitiesToRender)
			{
				Item item = me as Item;
				if (item != null)
				{
					item.UpdateSpriteStates(deltaTime);
				}
				else
				{
					Structure structure = me as Structure;
					if (structure != null)
					{
						structure.UpdateSpriteStates(deltaTime);
					}
				}
			}
		}

		// Token: 0x06002050 RID: 8272 RVA: 0x0014497C File Offset: 0x00142B7C
		public virtual void Update(float deltaTime, Camera cam)
		{
		}

		// Token: 0x06002051 RID: 8273 RVA: 0x00144980 File Offset: 0x00142B80
		public virtual void FlipX(bool relativeToSub, bool force = false)
		{
			this.FlippedX = !this.FlippedX;
			if (!relativeToSub || base.Submarine == null)
			{
				return;
			}
			Vector2 relative = this.WorldPosition - base.Submarine.WorldPosition;
			relative.Y = 0f;
			this.Move(-relative * 2f, true);
		}

		// Token: 0x06002052 RID: 8274 RVA: 0x001449E4 File Offset: 0x00142BE4
		public virtual void FlipY(bool relativeToSub, bool force = false)
		{
			this.FlippedY = !this.FlippedY;
			if (!relativeToSub || base.Submarine == null)
			{
				return;
			}
			Vector2 relative = this.WorldPosition - base.Submarine.WorldPosition;
			relative.X = 0f;
			this.Move(-relative * 2f, true);
		}

		// Token: 0x06002053 RID: 8275 RVA: 0x00144A46 File Offset: 0x00142C46
		public virtual Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect);
		}

		// Token: 0x06002054 RID: 8276 RVA: 0x00144A58 File Offset: 0x00142C58
		public static List<MapEntity> LoadAll(Submarine submarine, XElement parentElement, string filePath, int idOffset)
		{
			IdRemap idRemap = new IdRemap(parentElement, idOffset);
			bool containsHiddenContainers = false;
			bool hiddenContainerCreated = false;
			MTRandom hiddenContainerRNG = new MTRandom(ToolBox.StringToInt(submarine.Info.Name));
			foreach (XElement element in parentElement.Elements())
			{
				Identifier identifier2 = element.NameAsIdentifier();
				if (!(identifier2 != "Item"))
				{
					Identifier[] tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
					if (tags.Contains(Tags.HiddenItemContainer))
					{
						containsHiddenContainers = true;
						break;
					}
				}
			}
			List<MapEntity> entities = new List<MapEntity>();
			foreach (XElement element2 in parentElement.Elements())
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					CampaignMode campaign = gameSession.Campaign;
					if (campaign != null)
					{
						campaign.ThrowIfStartRoundCancellationRequested();
					}
				}
				string typeName = element2.Name.ToString();
				Type t;
				try
				{
					t = Type.GetType("Barotrauma." + typeName, true, true);
					if (t == null)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error in ",
							filePath,
							"! Could not find a entity of the type \"",
							typeName,
							"\"."
						}), null, null, false, false);
						continue;
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Error in ",
						filePath,
						"! Could not find a entity of the type \"",
						typeName,
						"\"."
					}), e, null, false, false);
					continue;
				}
				Identifier identifier = element2.GetAttributeIdentifier("identifier", "");
				Identifier replacementIdentifier = Identifier.Empty;
				if (t == typeof(Structure))
				{
					string name = element2.Attribute("name").Value;
					if (Structure.FindPrefab(name, identifier) == null)
					{
						ItemPrefab itemPrefab = ItemPrefab.Find(name, identifier);
						if (itemPrefab != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(120, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Could not find a structure with the identifier ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
							defaultInterpolatedStringHandler.AppendLiteral(", but there's a matching item with the identifier. Converting to an item.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							t = typeof(Item);
						}
					}
				}
				else if (t == typeof(Item) && !containsHiddenContainers && identifier == "vent" && submarine.Info.Type == SubmarineType.Player && !submarine.Info.HasTag(SubmarineTag.Shuttle))
				{
					if (!hiddenContainerCreated)
					{
						DebugConsole.AddWarning("There are no hidden containers such as loose vents or loose panels in the submarine \"" + submarine.Info.Name + "\". Certain traitor events require these to function properly. Converting one of the vents to a loose vent...", null);
					}
					if (!hiddenContainerCreated || hiddenContainerRNG.NextDouble() < 0.2)
					{
						replacementIdentifier = "loosevent".ToIdentifier();
						containsHiddenContainers = true;
						hiddenContainerCreated = true;
					}
				}
				try
				{
					MethodInfo loadMethod = t.GetMethod("Load", new Type[]
					{
						typeof(ContentXElement),
						typeof(Submarine),
						typeof(IdRemap)
					});
					if (loadMethod == null)
					{
						string str = "Could not find the method \"Load\" in ";
						Type type = t;
						DebugConsole.ThrowError(str + ((type != null) ? type.ToString() : null) + ".", null, null, false, false);
					}
					else if (!loadMethod.ReturnType.IsSubclassOf(typeof(MapEntity)))
					{
						DebugConsole.ThrowError("Error loading entity of the type \"" + t.ToString() + "\" - load method does not return a valid map entity.", null, null, false, false);
					}
					else
					{
						ContentXElement newElement = element2.FromPackage(null);
						if (!replacementIdentifier.IsEmpty)
						{
							newElement.SetAttributeValue("identifier", replacementIdentifier.ToString());
						}
						object newEntity = loadMethod.Invoke(t, new object[]
						{
							newElement,
							submarine,
							idRemap
						});
						if (newEntity != null)
						{
							entities.Add((MapEntity)newEntity);
						}
					}
				}
				catch (TargetInvocationException e2)
				{
					string str2 = "Error while loading entity of the type ";
					Type type2 = t;
					DebugConsole.ThrowError(str2 + ((type2 != null) ? type2.ToString() : null) + ".", e2.InnerException, null, false, false);
				}
				catch (Exception e3)
				{
					string str3 = "Error while loading entity of the type ";
					Type type3 = t;
					DebugConsole.ThrowError(str3 + ((type3 != null) ? type3.ToString() : null) + ".", e3, null, false, false);
				}
			}
			return entities;
		}

		// Token: 0x06002055 RID: 8277 RVA: 0x00144F14 File Offset: 0x00143114
		public static void MapLoaded(List<MapEntity> entities, bool updateHulls)
		{
			MapEntity.InitializeLoadedLinks(entities);
			List<LinkedSubmarine> linkedSubs = new List<LinkedSubmarine>();
			for (int i = 0; i < entities.Count; i++)
			{
				if (!entities[i].mapLoadedCalled && !entities[i].Removed)
				{
					LinkedSubmarine sub = entities[i] as LinkedSubmarine;
					if (sub != null)
					{
						linkedSubs.Add(sub);
					}
					else
					{
						entities[i].OnMapLoaded();
					}
				}
			}
			if (updateHulls)
			{
				Item.UpdateHulls();
				Gap.UpdateHulls();
			}
			entities.ForEach(delegate(MapEntity e)
			{
				e.mapLoadedCalled = true;
			});
			foreach (LinkedSubmarine linkedSub in linkedSubs)
			{
				linkedSub.OnMapLoaded();
			}
			MapEntity.CreateDroppedStacks(entities);
		}

		// Token: 0x06002056 RID: 8278 RVA: 0x00144FFC File Offset: 0x001431FC
		private static void CreateDroppedStacks(List<MapEntity> entities)
		{
			List<Item> itemsInStack = new List<Item>();
			for (int i = 0; i < entities.Count; i++)
			{
				Item item = entities[i] as Item;
				if (item != null && item.Prefab.MaxStackSize > 1)
				{
					PhysicsBody body = item.body;
					if (body != null && body.Enabled)
					{
						itemsInStack.Clear();
						itemsInStack.Add(item);
						for (int j = i + 1; j < entities.Count; j++)
						{
							Item item2 = entities[j] as Item;
							if (item2 != null && item.Prefab == item2.Prefab)
							{
								body = item2.body;
								if (body != null && body.Enabled && !item2.DroppedStack.Any<Item>() && Math.Abs(item.Position.X - item2.Position.X) <= 10f && Math.Abs(item.Position.Y - item2.Position.Y) <= 10f)
								{
									itemsInStack.Add(item2);
								}
							}
						}
						if (itemsInStack.Count > 1)
						{
							item.CreateDroppedStack(itemsInStack, true);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Merged x");
							defaultInterpolatedStringHandler.AppendFormatted<int>(itemsInStack.Count);
							defaultInterpolatedStringHandler.AppendLiteral(" of ");
							defaultInterpolatedStringHandler.AppendFormatted(item.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" into a dropped stack.");
							DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
			}
		}

		// Token: 0x06002057 RID: 8279 RVA: 0x00145188 File Offset: 0x00143388
		public static void InitializeLoadedLinks(IEnumerable<MapEntity> entities)
		{
			foreach (MapEntity e in entities)
			{
				if (!e.mapLoadedCalled && e.linkedToID != null && e.linkedToID.Count != 0)
				{
					e.linkedTo.Clear();
					foreach (ushort i in e.linkedToID)
					{
						MapEntity linked = Entity.FindEntityByID(i) as MapEntity;
						if (linked != null)
						{
							e.linkedTo.Add(linked);
						}
					}
					e.linkedToID.Clear();
					WayPoint wayPoint = e as WayPoint;
					if (wayPoint != null)
					{
						wayPoint.InitializeLinks();
					}
				}
			}
		}

		// Token: 0x06002058 RID: 8280 RVA: 0x00145274 File Offset: 0x00143474
		public virtual void OnMapLoaded()
		{
		}

		// Token: 0x06002059 RID: 8281 RVA: 0x00145276 File Offset: 0x00143476
		public virtual XElement Save(XElement parentElement)
		{
			string str = "Saving entity ";
			Type type = base.GetType();
			DebugConsole.ThrowError(str + ((type != null) ? type.ToString() : null) + " failed.", null, null, false, false);
			return null;
		}

		// Token: 0x0600205A RID: 8282 RVA: 0x001452A3 File Offset: 0x001434A3
		public void RemoveLinked(MapEntity e)
		{
			if (this.linkedTo == null)
			{
				return;
			}
			if (this.linkedTo.Contains(e))
			{
				this.linkedTo.Remove(e);
			}
		}

		// Token: 0x0600205B RID: 8283 RVA: 0x001452CC File Offset: 0x001434CC
		public HashSet<T> GetLinkedEntities<T>(HashSet<T> list = null, int? maxDepth = null, Func<T, bool> filter = null) where T : MapEntity
		{
			list = (list ?? new HashSet<T>());
			int startDepth = 0;
			MapEntity.GetLinkedEntitiesRecursive<T>(this, list, ref startDepth, maxDepth, filter);
			return list;
		}

		// Token: 0x0600205C RID: 8284 RVA: 0x001452F4 File Offset: 0x001434F4
		private static void GetLinkedEntitiesRecursive<T>(MapEntity mapEntity, HashSet<T> linkedTargets, ref int depth, int? maxDepth = null, Func<T, bool> filter = null) where T : MapEntity
		{
			int num = depth;
			int? num2 = maxDepth;
			if (num > num2.GetValueOrDefault() & num2 != null)
			{
				return;
			}
			foreach (MapEntity linkedEntity in mapEntity.linkedTo)
			{
				T linkedTarget = linkedEntity as T;
				if (linkedTarget != null && !linkedTargets.Contains(linkedTarget) && (filter == null || filter(linkedTarget)))
				{
					linkedTargets.Add(linkedTarget);
					depth++;
					MapEntity.GetLinkedEntitiesRecursive<T>(linkedEntity, linkedTargets, ref depth, maxDepth, filter);
				}
			}
		}

		// Token: 0x0600205E RID: 8286 RVA: 0x00145423 File Offset: 0x00143623
		[CompilerGenerated]
		internal static Vector2 <UpdateResizing>g__flipThenRotate|89_0(Vector2 point, Vector2 center, float angle, bool flipX, bool flipY)
		{
			if (flipX)
			{
				point = (point - center).FlipX() + center;
			}
			if (flipY)
			{
				point = (point - center).FlipY() + center;
			}
			point = MathUtils.RotatePointAroundTarget(point, center, angle, true);
			return point;
		}

		// Token: 0x0600205F RID: 8287 RVA: 0x00145460 File Offset: 0x00143660
		[CompilerGenerated]
		internal static Vector2 <UpdateResizing>g__rotateThenFlip|89_1(Vector2 point, Vector2 center, float angle, bool flipX, bool flipY)
		{
			point = MathUtils.RotatePointAroundTarget(point, center, angle, true);
			if (flipX)
			{
				point = (point - center).FlipX() + center;
			}
			if (flipY)
			{
				point = (point - center).FlipY() + center;
			}
			return point;
		}

		// Token: 0x04001056 RID: 4182
		protected static Vector2 selectionPos = Vector2.Zero;

		// Token: 0x04001057 RID: 4183
		protected static Vector2 selectionSize = Vector2.Zero;

		// Token: 0x04001058 RID: 4184
		private static Vector2 startMovingPos = Vector2.Zero;

		// Token: 0x04001059 RID: 4185
		private static float keyDelay;

		// Token: 0x0400105C RID: 4188
		private int resizeDirX;

		// Token: 0x0400105D RID: 4189
		private int resizeDirY;

		// Token: 0x0400105E RID: 4190
		private Rectangle? prevRect;

		// Token: 0x0400105F RID: 4191
		public static bool SelectionChanged;

		// Token: 0x04001061 RID: 4193
		private static List<Rectangle> oldRects = new List<Rectangle>();

		// Token: 0x04001062 RID: 4194
		private static Vector2 entityMovementNudge;

		// Token: 0x04001063 RID: 4195
		public static List<MapEntity> CopiedList = new List<MapEntity>();

		// Token: 0x04001064 RID: 4196
		private static List<MapEntity> highlightedInEditorList = new List<MapEntity>();

		// Token: 0x04001065 RID: 4197
		private static float highlightTimer;

		// Token: 0x04001066 RID: 4198
		private static GUIListBox highlightedListBox;

		// Token: 0x04001067 RID: 4199
		protected static GUIComponent editingHUD;

		// Token: 0x04001068 RID: 4200
		private static bool disableSelect;

		// Token: 0x0400106A RID: 4202
		public MapEntity ReplacedBy;

		// Token: 0x0400106C RID: 4204
		public static readonly List<MapEntity> MapEntityList = new List<MapEntity>();

		// Token: 0x0400106D RID: 4205
		public readonly MapEntityPrefab Prefab;

		// Token: 0x0400106E RID: 4206
		protected List<ushort> linkedToID;

		// Token: 0x0400106F RID: 4207
		public List<ushort> unresolvedLinkedToID;

		// Token: 0x04001070 RID: 4208
		public static int MapEntityUpdateInterval = 1;

		// Token: 0x04001071 RID: 4209
		public static int PoweredUpdateInterval = 1;

		// Token: 0x04001072 RID: 4210
		private static int mapEntityUpdateTick;

		// Token: 0x04001073 RID: 4211
		protected readonly List<Upgrade> Upgrades = new List<Upgrade>();

		// Token: 0x04001074 RID: 4212
		public readonly HashSet<Identifier> DisallowedUpgradeSet = new HashSet<Identifier>();

		// Token: 0x04001075 RID: 4213
		public readonly List<MapEntity> linkedTo = new List<MapEntity>();

		// Token: 0x04001078 RID: 4216
		public bool ShouldBeSaved = true;

		// Token: 0x04001079 RID: 4217
		protected Rectangle rect;

		// Token: 0x0400107A RID: 4218
		protected static readonly HashSet<MapEntity> highlightedEntities = new HashSet<MapEntity>();

		// Token: 0x0400107B RID: 4219
		private bool externalHighlight;

		// Token: 0x0400107C RID: 4220
		private bool isHighlighted;

		// Token: 0x0400107F RID: 4223
		private float _spriteOverrideDepth = float.NaN;

		// Token: 0x04001085 RID: 4229
		public int OriginalModuleIndex = -1;

		// Token: 0x04001086 RID: 4230
		public int OriginalContainerIndex = -1;

		// Token: 0x04001087 RID: 4231
		private static readonly List<MapEntity> tempHighlightedEntities = new List<MapEntity>();

		// Token: 0x04001088 RID: 4232
		private bool mapLoadedCalled;
	}
}
