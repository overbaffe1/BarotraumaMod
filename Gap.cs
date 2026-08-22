using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000CF RID: 207
	internal class Gap : MapEntity, ISerializableEntity
	{
		// Token: 0x17000707 RID: 1799
		// (get) Token: 0x06001B5F RID: 7007 RVA: 0x0010EB62 File Offset: 0x0010CD62
		public override bool SelectableInEditor
		{
			get
			{
				return Gap.ShowGaps && SubEditorScreen.IsLayerVisible(this);
			}
		}

		// Token: 0x06001B60 RID: 7008 RVA: 0x0010EB73 File Offset: 0x0010CD73
		public override bool IsVisible(Rectangle worldView)
		{
			return (Screen.Selected == GameMain.SubEditorScreen || GameMain.DebugDraw) && base.IsVisible(worldView);
		}

		// Token: 0x06001B61 RID: 7009 RVA: 0x0010EB94 File Offset: 0x0010CD94
		public override void Draw(SpriteBatch sb, bool editing, bool back = true)
		{
			Gap.<>c__DisplayClass4_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.sb = sb;
			CS$<>8__locals1.depth = (float)(this.ID % 255) * 1E-06f;
			if (GameMain.DebugDraw && Screen.Selected.Cam.Zoom > 0.1f)
			{
				if (this.FlowTargetHull != null)
				{
					this.<Draw>g__DrawArrow|4_0(this.FlowTargetHull, (float)(this.IsHorizontal ? this.rect.Height : this.rect.Width), Math.Abs(this.lerpedFlowForce.Length()), Color.Red * 0.3f, ref CS$<>8__locals1);
				}
				if (base.Submarine != null && this.outsideCollisionBlocker != null && this.outsideCollisionBlocker.Enabled)
				{
					EdgeShape edgeShape = this.outsideCollisionBlocker.FixtureList[0].Shape as EdgeShape;
					Vector2 startPos = ConvertUnits.ToDisplayUnits(this.outsideCollisionBlocker.GetWorldPoint(edgeShape.Vertex1)) + base.Submarine.Position;
					Vector2 endPos = ConvertUnits.ToDisplayUnits(this.outsideCollisionBlocker.GetWorldPoint(edgeShape.Vertex2)) + base.Submarine.Position;
					startPos.Y = -startPos.Y;
					endPos.Y = -endPos.Y;
					GUI.DrawLine(CS$<>8__locals1.sb, startPos, endPos, Color.Gray, 0f, 5f);
				}
			}
			if (!editing || !Gap.ShowGaps || !SubEditorScreen.IsLayerVisible(this))
			{
				return;
			}
			Color clr = (this.open == 0f) ? GUIStyle.Red : Color.Cyan;
			if (base.IsHighlighted)
			{
				clr = Color.Gold;
			}
			GUI.DrawRectangle(CS$<>8__locals1.sb, new Rectangle(base.WorldRect.X, -base.WorldRect.Y, this.rect.Width, this.rect.Height), clr * 0.2f, true, CS$<>8__locals1.depth, 1f);
			int lineWidth = 5;
			if (this.IsHorizontal)
			{
				GUI.DrawLine(CS$<>8__locals1.sb, new Vector2((float)base.WorldRect.X, (float)(-(float)base.WorldRect.Y + lineWidth / 2)), new Vector2((float)base.WorldRect.Right, (float)(-(float)base.WorldRect.Y + lineWidth / 2)), clr * 0.6f, 0f, (float)lineWidth);
				GUI.DrawLine(CS$<>8__locals1.sb, new Vector2((float)base.WorldRect.X, (float)(-(float)base.WorldRect.Y + this.rect.Height - lineWidth / 2)), new Vector2((float)base.WorldRect.Right, (float)(-(float)base.WorldRect.Y + this.rect.Height - lineWidth / 2)), clr * 0.6f, 0f, (float)lineWidth);
			}
			else
			{
				GUI.DrawLine(CS$<>8__locals1.sb, new Vector2((float)(base.WorldRect.X + lineWidth / 2), (float)(-(float)base.WorldRect.Y)), new Vector2((float)(base.WorldRect.X + lineWidth / 2), (float)(-(float)base.WorldRect.Y + this.rect.Height)), clr * 0.6f, 0f, (float)lineWidth);
				GUI.DrawLine(CS$<>8__locals1.sb, new Vector2((float)(base.WorldRect.Right - lineWidth / 2), (float)(-(float)base.WorldRect.Y)), new Vector2((float)(base.WorldRect.Right - lineWidth / 2), (float)(-(float)base.WorldRect.Y + this.rect.Height)), clr * 0.6f, 0f, (float)lineWidth);
			}
			if (this.linkedTo.Count != 2 || this.linkedTo[0] != this.linkedTo[1])
			{
				for (int i = 0; i < this.linkedTo.Count; i++)
				{
					Hull hull = this.linkedTo[i] as Hull;
					if (hull != null)
					{
						this.<Draw>g__DrawArrow|4_0(hull, 32f, 15f, clr, ref CS$<>8__locals1);
					}
				}
			}
			if (base.IsSelected)
			{
				GUI.DrawRectangle(CS$<>8__locals1.sb, new Vector2((float)(base.WorldRect.X - 5), (float)(-(float)base.WorldRect.Y - 5)), new Vector2((float)(this.rect.Width + 10), (float)(this.rect.Height + 10)), GUIStyle.Red, false, CS$<>8__locals1.depth, (float)((int)Math.Max(1.5f / Screen.Selected.Cam.Zoom, 1f)));
			}
		}

		// Token: 0x06001B62 RID: 7010 RVA: 0x0010F078 File Offset: 0x0010D278
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD(false);
			}
		}

		// Token: 0x06001B63 RID: 7011 RVA: 0x0010F09C File Offset: 0x0010D29C
		private GUIComponent CreateEditingHUD(bool inGame = false)
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
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("entityname.gap");
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.Left, false, "", null);
			GUITickBox hiddenInGameTickBox = new GUITickBox(new RectTransform(new Vector2(0.5f, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("sp.hiddeningame.name"), null, "")
			{
				Selected = base.HiddenInGame
			};
			GUITickBox guitickBox = hiddenInGameTickBox;
			guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tickbox)
			{
				this.HiddenInGame = tickbox.Selected;
				return true;
			}));
			MapEntity.editingHUD.RectTransform.Resize(new Point(MapEntity.editingHUD.Rect.Width, (int)((float)paddedFrame.Children.Sum((GUIComponent c) => c.Rect.Height + paddedFrame.AbsoluteSpacing) / paddedFrame.RectTransform.RelativeSize.Y * 1.25f)), true);
			MapEntity.PositionEditingHUD();
			return MapEntity.editingHUD;
		}

		// Token: 0x17000708 RID: 1800
		// (get) Token: 0x06001B64 RID: 7012 RVA: 0x0010F2DB File Offset: 0x0010D4DB
		// (set) Token: 0x06001B65 RID: 7013 RVA: 0x0010F2E3 File Offset: 0x0010D4E3
		public bool IsHorizontal { get; private set; }

		// Token: 0x17000709 RID: 1801
		// (get) Token: 0x06001B66 RID: 7014 RVA: 0x0010F2EC File Offset: 0x0010D4EC
		public bool IsDiagonal { get; }

		// Token: 0x1700070A RID: 1802
		// (get) Token: 0x06001B67 RID: 7015 RVA: 0x0010F2F4 File Offset: 0x0010D4F4
		// (set) Token: 0x06001B68 RID: 7016 RVA: 0x0010F2FC File Offset: 0x0010D4FC
		public float Open
		{
			get
			{
				return this.open;
			}
			set
			{
				if (float.IsNaN(value))
				{
					return;
				}
				float prevValue = this.open;
				if (value > this.open)
				{
					this.openedTimer = 1f;
				}
				this.open = MathHelper.Clamp(value, 0f, 1f);
				if (!MathUtils.NearlyEqual(this.open, prevValue, 0.0001f))
				{
					this.overlappingGapsDirty = true;
					this.FlagOverlappingGapsDirty();
					if (this.connectedDoor == null && !this.IsHorizontal)
					{
						if (this.linkedTo.Any((MapEntity e) => e is Hull))
						{
							if (this.open > prevValue && this.open >= 1f)
							{
								Gap.<set_Open>g__InformWaypointsAboutGapState|38_1(this, true);
								return;
							}
							if (this.open < prevValue && prevValue >= 1f)
							{
								Gap.<set_Open>g__InformWaypointsAboutGapState|38_1(this, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x1700070B RID: 1803
		// (get) Token: 0x06001B69 RID: 7017 RVA: 0x0010F3D5 File Offset: 0x0010D5D5
		public float Size
		{
			get
			{
				return (float)(this.IsHorizontal ? this.Rect.Height : this.Rect.Width);
			}
		}

		// Token: 0x1700070C RID: 1804
		// (get) Token: 0x06001B6A RID: 7018 RVA: 0x0010F3F8 File Offset: 0x0010D5F8
		public float PressureDistributionSpeed
		{
			get
			{
				return this.Size / 100f * this.open;
			}
		}

		// Token: 0x1700070D RID: 1805
		// (get) Token: 0x06001B6B RID: 7019 RVA: 0x0010F40D File Offset: 0x0010D60D
		// (set) Token: 0x06001B6C RID: 7020 RVA: 0x0010F436 File Offset: 0x0010D636
		public Door ConnectedDoor
		{
			get
			{
				if (this.connectedDoor != null && this.connectedDoor.Item.Removed)
				{
					this.connectedDoor = null;
				}
				return this.connectedDoor;
			}
			set
			{
				this.connectedDoor = value;
			}
		}

		// Token: 0x1700070E RID: 1806
		// (get) Token: 0x06001B6D RID: 7021 RVA: 0x0010F43F File Offset: 0x0010D63F
		public Vector2 LerpedFlowForce
		{
			get
			{
				return this.lerpedFlowForce;
			}
		}

		// Token: 0x1700070F RID: 1807
		// (get) Token: 0x06001B6E RID: 7022 RVA: 0x0010F447 File Offset: 0x0010D647
		public Hull FlowTargetHull
		{
			get
			{
				return this.flowTargetHull;
			}
		}

		// Token: 0x17000710 RID: 1808
		// (get) Token: 0x06001B6F RID: 7023 RVA: 0x0010F44F File Offset: 0x0010D64F
		public bool IsRoomToRoom
		{
			get
			{
				return this.linkedTo.Count == 2;
			}
		}

		// Token: 0x17000711 RID: 1809
		// (get) Token: 0x06001B70 RID: 7024 RVA: 0x0010F45F File Offset: 0x0010D65F
		// (set) Token: 0x06001B71 RID: 7025 RVA: 0x0010F467 File Offset: 0x0010D667
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				base.Rect = value;
				this.FindHulls();
			}
		}

		// Token: 0x17000712 RID: 1810
		// (get) Token: 0x06001B72 RID: 7026 RVA: 0x0010F476 File Offset: 0x0010D676
		public override string Name
		{
			get
			{
				return "Gap";
			}
		}

		// Token: 0x17000713 RID: 1811
		// (get) Token: 0x06001B73 RID: 7027 RVA: 0x0010F47D File Offset: 0x0010D67D
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.properties;
			}
		}

		// Token: 0x06001B74 RID: 7028 RVA: 0x0010F485 File Offset: 0x0010D685
		public Gap(Rectangle rectangle) : this(rectangle, Submarine.MainSub)
		{
			if (SubEditorScreen.IsSubEditor())
			{
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
				{
					this
				}, false, true));
			}
		}

		// Token: 0x06001B75 RID: 7029 RVA: 0x0010F4B2 File Offset: 0x0010D6B2
		public Gap(Rectangle rect, Submarine submarine) : this(rect, rect.Width < rect.Height, submarine, false, 0)
		{
		}

		// Token: 0x06001B76 RID: 7030 RVA: 0x0010F4CC File Offset: 0x0010D6CC
		public Gap(Rectangle rect, bool isHorizontal, Submarine submarine, bool isDiagonal = false, ushort id = 0) : base(CoreEntityPrefab.GapPrefab, submarine, id)
		{
			this.rect = rect;
			this.flowForce = Vector2.Zero;
			this.IsHorizontal = isHorizontal;
			this.IsDiagonal = isDiagonal;
			this.open = 1f;
			this.properties = SerializableProperty.GetProperties(this);
			this.FindHulls();
			Gap.GapList.Add(this);
			base.InsertToList();
			this.GlowEffectT = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
			float blockerSize = ConvertUnits.ToSimUnits(Math.Max(rect.Width, rect.Height)) / 2f;
			this.outsideCollisionBlocker = GameMain.World.CreateEdge(-Vector2.UnitX * blockerSize, Vector2.UnitX * blockerSize, BodyType.Static, Category.Cat1, Category.Cat2, false);
			this.outsideCollisionBlocker.UserData = this;
			this.outsideCollisionBlocker.Enabled = false;
			base.Resized += delegate(Rectangle newRect)
			{
				this.IsHorizontal = (newRect.Width < newRect.Height);
			};
			this.wasRoomToRoom = this.IsRoomToRoom;
			this.RefreshOutsideCollider();
			DebugConsole.Log("Created gap (" + this.ID.ToString() + ")");
		}

		// Token: 0x06001B77 RID: 7031 RVA: 0x0010F60A File Offset: 0x0010D80A
		public override MapEntity Clone()
		{
			return new Gap(this.rect, this.IsHorizontal, base.Submarine, false, 0);
		}

		// Token: 0x06001B78 RID: 7032 RVA: 0x0010F628 File Offset: 0x0010D828
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move a gap by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			base.Move(amount, ignoreContacts);
			if (!this.DisableHullRechecks)
			{
				this.FindHulls();
			}
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x0010F6A0 File Offset: 0x0010D8A0
		public static void UpdateHulls()
		{
			foreach (Gap g in Gap.GapList)
			{
				for (int i = g.linkedTo.Count - 1; i >= 0; i--)
				{
					if (g.linkedTo[i].Removed)
					{
						g.linkedTo.RemoveAt(i);
					}
				}
				if (!g.DisableHullRechecks)
				{
					g.FindHulls();
				}
			}
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x0010F730 File Offset: 0x0010D930
		public override bool IsMouseOn(Vector2 position)
		{
			return Gap.ShowGaps && Submarine.RectContains(base.WorldRect, position, false) && !Submarine.RectContains(MathUtils.ExpandRect(base.WorldRect, -5), position, false);
		}

		// Token: 0x06001B7B RID: 7035 RVA: 0x0010F764 File Offset: 0x0010D964
		public void AutoOrient()
		{
			Vector2 searchPosLeft = new Vector2((float)this.rect.X, (float)(this.rect.Y - this.rect.Height / 2));
			Hull hullLeft = Hull.FindHullUnoptimized(searchPosLeft, null, false, true);
			Vector2 searchPosRight = new Vector2((float)this.rect.Right, (float)(this.rect.Y - this.rect.Height / 2));
			Hull hullRight = Hull.FindHullUnoptimized(searchPosRight, null, false, true);
			if (hullLeft != null && hullRight != null && hullLeft != hullRight)
			{
				this.IsHorizontal = true;
				return;
			}
			Vector2 searchPosTop = new Vector2((float)this.rect.Center.X, (float)this.rect.Y);
			Hull hullTop = Hull.FindHullUnoptimized(searchPosTop, null, false, true);
			Vector2 searchPosBottom = new Vector2((float)this.rect.Center.X, (float)(this.rect.Y - this.rect.Height));
			Hull hullBottom = Hull.FindHullUnoptimized(searchPosBottom, null, false, true);
			if (hullTop != null && hullBottom != null && hullTop != hullBottom)
			{
				this.IsHorizontal = false;
				return;
			}
			if (hullLeft == null != (hullRight == null))
			{
				this.IsHorizontal = true;
				return;
			}
			if (hullTop == null != (hullBottom == null))
			{
				this.IsHorizontal = false;
			}
		}

		// Token: 0x06001B7C RID: 7036 RVA: 0x0010F898 File Offset: 0x0010DA98
		private void FindHulls()
		{
			Hull[] hulls = new Hull[2];
			foreach (MapEntity linked in this.linkedTo)
			{
				Hull hull = linked as Hull;
				if (hull != null)
				{
					hull.ConnectedGaps.Remove(this);
				}
			}
			this.linkedTo.Clear();
			int tolerance = 1;
			Vector2[] searchPos = new Vector2[2];
			if (this.IsHorizontal)
			{
				searchPos[0] = new Vector2((float)(this.rect.X - tolerance), (float)(this.rect.Y - this.rect.Height / 2));
				searchPos[1] = new Vector2((float)(this.rect.Right + tolerance), (float)(this.rect.Y - this.rect.Height / 2));
			}
			else
			{
				searchPos[0] = new Vector2((float)this.rect.Center.X, (float)(this.rect.Y + tolerance));
				searchPos[1] = new Vector2((float)this.rect.Center.X, (float)(this.rect.Y - this.rect.Height - tolerance));
			}
			for (int i = 0; i < 2; i++)
			{
				hulls[i] = Hull.FindHullUnoptimized(searchPos[i], null, false, true);
				if (hulls[i] == null)
				{
					hulls[i] = Hull.FindHullUnoptimized(searchPos[i], null, false, true);
				}
			}
			if (hulls[0] == null && hulls[1] == null)
			{
				return;
			}
			if (hulls[0] == null && hulls[1] != null)
			{
				Hull temp = hulls[0];
				hulls[0] = hulls[1];
				hulls[1] = temp;
			}
			this.flowTargetHull = hulls[0];
			for (int j = 0; j < 2; j++)
			{
				if (hulls[j] != null)
				{
					this.linkedTo.Add(hulls[j]);
					if (!hulls[j].ConnectedGaps.Contains(this))
					{
						hulls[j].ConnectedGaps.Add(this);
					}
					foreach (Gap gap in hulls[j].ConnectedGaps)
					{
						gap.overlappingGapsDirty = true;
					}
				}
			}
		}

		// Token: 0x06001B7D RID: 7037 RVA: 0x0010FAE8 File Offset: 0x0010DCE8
		public override void Update(float deltaTime, Camera cam)
		{
			Hull hull = (this.linkedTo.Count < 1) ? null : (this.linkedTo[0] as Hull);
			Hull hull2 = (this.linkedTo.Count < 2) ? null : ((Hull)this.linkedTo[1]);
			int updateInterval = 4;
			if (hull != null && hull2 != null && hull.LethalPressure > 0f != hull2.LethalPressure > 0f)
			{
				updateInterval = 1;
			}
			else
			{
				float flowMagnitude = this.flowForce.LengthSquared();
				if (flowMagnitude < 1f)
				{
					updateInterval = 8;
				}
				else if (this.linkedTo.Count == 2 && flowMagnitude > 10f)
				{
					updateInterval = 1;
				}
			}
			this.updateCount++;
			if (this.updateCount < updateInterval)
			{
				return;
			}
			deltaTime *= (float)this.updateCount;
			this.updateCount = 0;
			if (this.overlappingGapsDirty)
			{
				this.RefreshOverlappingGaps();
				this.overlappingGapsDirty = false;
			}
			this.flowForce = Vector2.Zero;
			this.outsideColliderRaycastTimer -= deltaTime;
			if (this.IsRoomToRoom != this.wasRoomToRoom)
			{
				this.RefreshOutsideCollider();
				this.wasRoomToRoom = this.IsRoomToRoom;
			}
			if (this.open == 0f || this.linkedTo.Count == 0)
			{
				this.lerpedFlowForce = Vector2.Zero;
				return;
			}
			if (hull == hull2)
			{
				return;
			}
			this.UpdateOxygen(hull, hull2, deltaTime);
			if (this.linkedTo.Count == 1)
			{
				this.UpdateRoomToOut(deltaTime, hull);
			}
			else if (this.linkedTo.Count == 2)
			{
				this.UpdateRoomToRoom(deltaTime, hull, hull2);
			}
			this.flowForce.X = MathHelper.Clamp(this.flowForce.X, -500f, 500f);
			this.flowForce.Y = MathHelper.Clamp(this.flowForce.Y, -500f, 500f);
			if (this.openedTimer > 0f && this.flowForce.LengthSquared() > this.lerpedFlowForce.LengthSquared())
			{
				this.lerpedFlowForce = this.flowForce;
			}
			else
			{
				this.lerpedFlowForce = Vector2.Lerp(this.lerpedFlowForce, this.flowForce, deltaTime * 5f);
			}
			this.openedTimer -= deltaTime;
			this.EmitParticles(deltaTime);
		}

		// Token: 0x06001B7E RID: 7038 RVA: 0x0010FD20 File Offset: 0x0010DF20
		private void EmitParticles(float deltaTime)
		{
			Gap.<>c__DisplayClass73_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.flowTargetHull == null)
			{
				return;
			}
			if (this.linkedTo.Count == 2)
			{
				Hull hull = this.linkedTo[0] as Hull;
				if (hull != null)
				{
					Hull hull2 = this.linkedTo[1] as Hull;
					if (hull2 != null)
					{
						if (hull.linkedTo.Contains(hull2))
						{
							return;
						}
						foreach (MapEntity linkedEntity in hull.linkedTo)
						{
							Hull h = linkedEntity as Hull;
							if (h != null && h.linkedTo.Contains(hull) && h.linkedTo.Contains(hull2))
							{
								return;
							}
						}
						foreach (MapEntity linkedEntity2 in hull2.linkedTo)
						{
							Hull h2 = linkedEntity2 as Hull;
							if (h2 != null && h2.linkedTo.Contains(hull) && h2.linkedTo.Contains(hull2))
							{
								return;
							}
						}
					}
				}
			}
			CS$<>8__locals1.pos = this.Position;
			if (this.IsHorizontal)
			{
				CS$<>8__locals1.pos.X = CS$<>8__locals1.pos.X + (float)Math.Sign(this.flowForce.X);
				CS$<>8__locals1.pos.Y = MathHelper.Clamp(Rand.Range(this.higherSurface, this.lowerSurface, Rand.RandSync.Unsynced), (float)(this.rect.Y - this.rect.Height), (float)this.rect.Y);
			}
			if (this.flowTargetHull != null)
			{
				CS$<>8__locals1.pos.X = MathHelper.Clamp(CS$<>8__locals1.pos.X, (float)(this.flowTargetHull.Rect.X + 1), (float)(this.flowTargetHull.Rect.Right - 1));
				CS$<>8__locals1.pos.Y = MathHelper.Clamp(CS$<>8__locals1.pos.Y, (float)(this.flowTargetHull.Rect.Y - this.flowTargetHull.Rect.Height + 1), (float)(this.flowTargetHull.Rect.Y - 1));
			}
			float particleAmountMultiplier = 1f - (float)GameMain.ParticleManager.ParticleCount / (float)GameMain.ParticleManager.MaxParticles;
			particleAmountMultiplier *= particleAmountMultiplier;
			if (this.LerpedFlowForce.LengthSquared() > 20000f)
			{
				this.particleTimer += deltaTime;
				if (this.IsHorizontal)
				{
					float particlesPerSec = this.open * (float)this.rect.Height * 0.1f * particleAmountMultiplier;
					if (this.openedTimer > 0f)
					{
						particlesPerSec *= 1f + this.openedTimer * 10f;
					}
					float emitInterval = 1f / particlesPerSec;
					while (this.particleTimer > emitInterval)
					{
						Vector2 velocity = new Vector2(MathHelper.Clamp(this.flowForce.X, -5000f, 5000f) * Rand.Range(0.5f, 0.7f, Rand.RandSync.Unsynced), this.flowForce.Y * Rand.Range(0.5f, 0.7f, Rand.RandSync.Unsynced));
						if (this.flowTargetHull.WaterVolume < this.flowTargetHull.Volume * 0.95f)
						{
							Particle particle = GameMain.ParticleManager.CreateParticle("watersplash", ((base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position)) - Vector2.UnitY * Rand.Range(0f, 10f, Rand.RandSync.Unsynced), velocity, 0f, this.flowTargetHull, 0f, null);
							if (particle != null)
							{
								if (particle.CurrentHull == null)
								{
									GameMain.ParticleManager.RemoveParticle(particle);
								}
								particle.Size *= Math.Min(Math.Abs(this.flowForce.X / 500f), 5f);
							}
							if (this.<EmitParticles>g__GapSize|73_1(ref CS$<>8__locals1) <= 96f || !this.IsRoomToRoom)
							{
								this.<EmitParticles>g__CreateWaterSpatter|73_0(ref CS$<>8__locals1);
							}
						}
						if (Math.Abs(this.flowForce.X) > 300f && this.flowTargetHull.WaterVolume > this.flowTargetHull.Volume * 0.1f)
						{
							CS$<>8__locals1.pos.X = CS$<>8__locals1.pos.X + (float)Math.Sign(this.flowForce.X) * 10f;
							if (this.rect.Height < 32)
							{
								CS$<>8__locals1.pos.Y = (float)(this.rect.Y - this.rect.Height / 2);
							}
							else
							{
								float bottomY = (float)(this.rect.Y - this.rect.Height + 16);
								float topY = MathHelper.Clamp(this.lowerSurface, bottomY, (float)(this.rect.Y - 16));
								CS$<>8__locals1.pos.Y = Rand.Range(bottomY, topY, Rand.RandSync.Unsynced);
							}
							GameMain.ParticleManager.CreateParticle("bubbles", (base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position), velocity, 0f, this.flowTargetHull, 0f, null);
						}
						this.particleTimer -= emitInterval;
					}
					return;
				}
				if (Math.Sign(this.flowTargetHull.WorldPosition.Y - this.WorldPosition.Y) != Math.Sign(this.lerpedFlowForce.Y))
				{
					return;
				}
				float particlesPerSec2 = Math.Max(this.open * (float)this.rect.Width * particleAmountMultiplier, 10f);
				float emitInterval2 = 1f / particlesPerSec2;
				while (this.particleTimer > emitInterval2)
				{
					CS$<>8__locals1.pos.X = (float)Rand.Range(this.rect.X, this.rect.X + this.rect.Width + 1, Rand.RandSync.Unsynced);
					Vector2 velocity2 = new Vector2(this.lerpedFlowForce.X * Rand.Range(0.5f, 0.7f, Rand.RandSync.Unsynced), MathHelper.Clamp(this.lerpedFlowForce.Y, -500f, 1000f) * Rand.Range(0.5f, 0.7f, Rand.RandSync.Unsynced));
					if (this.flowTargetHull.WaterVolume < this.flowTargetHull.Volume * 0.95f)
					{
						Particle splash = GameMain.ParticleManager.CreateParticle("watersplash", (base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position), velocity2, 0f, this.FlowTargetHull, 0f, null);
						if (splash != null)
						{
							if (splash.CurrentHull == null)
							{
								GameMain.ParticleManager.RemoveParticle(splash);
							}
							splash.Size *= MathHelper.Clamp((float)this.rect.Width / 50f, 1.5f, 4f);
						}
						if (this.<EmitParticles>g__GapSize|73_1(ref CS$<>8__locals1) <= 96f || !this.IsRoomToRoom)
						{
							this.<EmitParticles>g__CreateWaterSpatter|73_0(ref CS$<>8__locals1);
						}
					}
					if (Math.Abs(this.flowForce.Y) > 190f && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.3f && this.flowTargetHull.WaterVolume > this.flowTargetHull.Volume * 0.1f)
					{
						GameMain.ParticleManager.CreateParticle("bubbles", (base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position), this.flowForce / 2f, 0f, this.FlowTargetHull, 0f, null);
					}
					this.particleTimer -= emitInterval2;
				}
				return;
			}
			else
			{
				if (this.LerpedFlowForce.LengthSquared() > 100f && (this.<EmitParticles>g__GapSize|73_1(ref CS$<>8__locals1) <= 96f || !this.IsRoomToRoom))
				{
					this.particleTimer += deltaTime;
					float particlesPerSec3 = this.open * 10f * particleAmountMultiplier;
					float emitInterval3 = 1f / particlesPerSec3;
					while (this.particleTimer > emitInterval3)
					{
						Vector2 velocity3 = this.flowForce;
						if (!this.IsHorizontal)
						{
							velocity3.X *= Rand.Range(1f, 3f, Rand.RandSync.Unsynced);
						}
						if (this.flowTargetHull.WaterVolume < this.flowTargetHull.Volume)
						{
							GameMain.ParticleManager.CreateParticle((Rand.Range(0f, this.open, Rand.RandSync.Unsynced) < 0.05f) ? "waterdrop" : "watersplash", (base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position), velocity3, 0f, this.flowTargetHull, 0f, null);
							this.<EmitParticles>g__CreateWaterSpatter|73_0(ref CS$<>8__locals1);
						}
						GameMain.ParticleManager.CreateParticle("bubbles", (base.Submarine == null) ? CS$<>8__locals1.pos : (CS$<>8__locals1.pos + base.Submarine.Position), velocity3, 0f, this.flowTargetHull, 0f, null);
						this.particleTimer -= emitInterval3;
					}
					return;
				}
				this.particleTimer = 0f;
			}
		}

		// Token: 0x06001B7F RID: 7039 RVA: 0x001106CC File Offset: 0x0010E8CC
		private void UpdateRoomToRoom(float deltaTime, Hull hull1, Hull hull2)
		{
			Vector2 subOffset = Vector2.Zero;
			if (hull1.Submarine != base.Submarine)
			{
				subOffset = base.Submarine.Position - hull1.Submarine.Position;
			}
			else if (hull2.Submarine != base.Submarine)
			{
				subOffset = hull2.Submarine.Position - base.Submarine.Position;
			}
			if ((double)hull1.WaterVolume <= 0.0 && (double)hull2.WaterVolume <= 0.0)
			{
				return;
			}
			float sizeModifier = this.Size / 100f * this.open * (1f - this.overlappingGapFlowRateReduction);
			if (this.IsHorizontal)
			{
				this.higherSurface = Math.Max(hull1.Surface, hull2.Surface + subOffset.Y);
				float delta = 0f;
				if (Math.Max(hull1.Surface + hull1.WaveY[hull1.WaveY.Length - 1], hull2.Surface + subOffset.Y + hull2.WaveY[0]) > (float)this.rect.Y - this.Size)
				{
					int dir = (hull1.Pressure > hull2.Pressure + subOffset.Y) ? 1 : -1;
					if (dir == -1)
					{
						if (hull2.WaterVolume <= 0f)
						{
							return;
						}
						this.lowerSurface = hull1.Surface - hull1.WaveY[hull1.WaveY.Length - 1];
						this.flowTargetHull = hull1;
						delta = Math.Min((hull2.Pressure + subOffset.Y - hull1.Pressure) * 300f * sizeModifier * deltaTime, Math.Min(hull2.WaterVolume, hull2.Volume));
						delta = Math.Min(delta, hull1.Volume * 1.05f - hull1.WaterVolume);
						hull1.WaterVolume += delta;
						hull2.WaterVolume -= delta;
						this.waterFlowThisFrame += delta;
						if (hull1.WaterVolume > hull1.Volume)
						{
							hull1.Pressure = Math.Max(hull1.Pressure, (hull1.Pressure + hull2.Pressure + subOffset.Y) / 2f);
						}
						this.flowForce = new Vector2(-delta * (float)(0.016666666666666666 / (double)deltaTime), 0f);
					}
					else if (dir == 1)
					{
						if (hull1.WaterVolume <= 0f)
						{
							return;
						}
						this.lowerSurface = hull2.Surface - hull2.WaveY[hull2.WaveY.Length - 1];
						this.flowTargetHull = hull2;
						delta = Math.Min((hull1.Pressure - (hull2.Pressure + subOffset.Y)) * 300f * sizeModifier * deltaTime, Math.Min(hull1.WaterVolume, hull1.Volume));
						delta = Math.Min(delta, hull2.Volume * 1.05f - hull2.WaterVolume);
						hull1.WaterVolume -= delta;
						hull2.WaterVolume += delta;
						if (hull2.WaterVolume > hull2.Volume)
						{
							hull2.Pressure = Math.Max(hull2.Pressure, (hull1.Pressure - subOffset.Y + hull2.Pressure) / 2f);
						}
						this.waterFlowThisFrame += delta;
						this.flowForce = new Vector2(delta * (float)(0.016666666666666666 / (double)deltaTime), 0f);
					}
					if (delta > 1.5f && subOffset == Vector2.Zero)
					{
						float avg = (hull1.Surface + hull2.Surface) / 2f;
						if (hull1.WaterVolume < hull1.Volume / 1.05f && hull1.Surface + hull1.WaveY[hull1.WaveY.Length - 1] < (float)this.rect.Y)
						{
							hull1.WaveVel[hull1.WaveY.Length - 1] = (avg - (hull1.Surface + hull1.WaveY[hull1.WaveY.Length - 1])) * 0.1f;
							hull1.WaveVel[hull1.WaveY.Length - 2] = hull1.WaveVel[hull1.WaveY.Length - 1];
						}
						if (hull2.WaterVolume < hull2.Volume / 1.05f && hull2.Surface + hull2.WaveY[0] < (float)this.rect.Y)
						{
							hull2.WaveVel[0] = (avg - (hull2.Surface + hull2.WaveY[0])) * 0.1f;
							hull2.WaveVel[1] = hull2.WaveVel[0];
						}
					}
				}
			}
			else if (hull2.Pressure + subOffset.Y > hull1.Pressure && hull2.WaterVolume > 0f)
			{
				float delta2 = Math.Min(hull2.WaterVolume - hull2.Volume + hull2.Volume * 1.05f, deltaTime * 8000f * sizeModifier);
				if (hull1.WaterVolume + delta2 > hull1.Volume * 1.05f)
				{
					delta2 -= hull1.WaterVolume + delta2 - hull1.Volume * 1.05f;
				}
				delta2 = Math.Max(delta2, 0f);
				hull1.WaterVolume += delta2;
				hull2.WaterVolume -= delta2;
				this.waterFlowThisFrame += delta2;
				this.flowForce = new Vector2(0f, Math.Min(Math.Min(hull2.Pressure + subOffset.Y - hull1.Pressure, 200f), delta2 * (float)(0.016666666666666666 / (double)deltaTime)));
				this.flowTargetHull = hull1;
				if (hull1.WaterVolume > hull1.Volume)
				{
					hull1.Pressure = Math.Max(hull1.Pressure, (hull1.Pressure + (hull2.Pressure + subOffset.Y)) / 2f);
				}
			}
			else if (hull1.WaterVolume > 0f)
			{
				this.flowTargetHull = hull2;
				float delta3 = Math.Min(hull1.WaterVolume, deltaTime * 25000f * sizeModifier);
				if (hull2.WaterVolume + delta3 > hull2.Volume * 1.05f)
				{
					delta3 -= hull2.WaterVolume + delta3 - hull2.Volume * 1.05f;
				}
				hull1.WaterVolume -= delta3;
				hull2.WaterVolume += delta3;
				this.waterFlowThisFrame += delta3;
				this.flowForce = new Vector2(hull1.WaveY[hull1.GetWaveIndex((float)this.rect.X)] - hull1.WaveY[hull1.GetWaveIndex((float)this.rect.Right)], MathHelper.Clamp(-delta3 * (float)(0.016666666666666666 / (double)deltaTime), -200f, 0f));
				if (hull2.WaterVolume > hull2.Volume)
				{
					hull2.Pressure = Math.Max(hull2.Pressure, (hull1.Pressure - subOffset.Y + hull2.Pressure) / 2f);
				}
			}
			if (this.open > 0f)
			{
				if (hull1.WaterVolume > hull1.Volume / 1.05f && hull2.WaterVolume > hull2.Volume / 1.05f)
				{
					float avgLethality = (hull1.LethalPressure + hull2.LethalPressure) / 2f;
					Gap.<UpdateRoomToRoom>g__changePressure|74_0(hull1, avgLethality, this.PressureDistributionSpeed, deltaTime);
					Gap.<UpdateRoomToRoom>g__changePressure|74_0(hull2, avgLethality, this.PressureDistributionSpeed, deltaTime);
					return;
				}
				hull1.LethalPressure -= 10f * this.PressureDistributionSpeed * deltaTime;
				hull2.LethalPressure -= 10f * this.PressureDistributionSpeed * deltaTime;
			}
		}

		// Token: 0x06001B80 RID: 7040 RVA: 0x00110E70 File Offset: 0x0010F070
		private float GetWaterFlowFromOutside(Hull hull, float deltaTime, bool ignoreCurrentWater = false)
		{
			float sizeModifier = this.Size * this.open * this.open * (1f - this.overlappingGapFlowRateReduction);
			float delta = 500f * sizeModifier * deltaTime;
			if (!ignoreCurrentWater)
			{
				delta = Math.Min(delta, hull.Volume * 1.05f - hull.WaterVolume);
			}
			return delta;
		}

		// Token: 0x06001B81 RID: 7041 RVA: 0x00110EC8 File Offset: 0x0010F0C8
		private void UpdateRoomToOut(float deltaTime, Hull hull1)
		{
			float delta = this.GetWaterFlowFromOutside(hull1, deltaTime, false);
			hull1.WaterVolume += delta;
			if (hull1.WaterVolume > hull1.Volume)
			{
				hull1.Pressure += 100f * deltaTime;
			}
			this.flowTargetHull = hull1;
			if (this.IsHorizontal)
			{
				if ((float)this.rect.X > (float)hull1.Rect.X + (float)hull1.Rect.Width / 2f)
				{
					this.flowForce = new Vector2(-delta * (float)(0.016666666666666666 / (double)deltaTime), 0f);
				}
				else
				{
					this.flowForce = new Vector2(delta * (float)(0.016666666666666666 / (double)deltaTime), 0f);
				}
				this.higherSurface = hull1.Surface;
				this.lowerSurface = (float)this.rect.Y;
				if (hull1.WaterVolume < hull1.Volume / 1.05f && hull1.Surface < (float)this.rect.Y)
				{
					if ((float)this.rect.X > (float)hull1.Rect.X + (float)hull1.Rect.Width / 2f)
					{
						Gap.<UpdateRoomToOut>g__CreateWave|76_0(this.rect, hull1, hull1.WaveY.Length - 1, hull1.WaveY.Length - 2, this.flowForce, deltaTime);
					}
					else
					{
						Gap.<UpdateRoomToOut>g__CreateWave|76_0(this.rect, hull1, 0, 1, this.flowForce, deltaTime);
					}
				}
				else
				{
					hull1.LethalPressure += ((base.Submarine != null && base.Submarine.AtDamageDepth) ? 100f : 15f) * this.PressureDistributionSpeed * deltaTime;
				}
			}
			else
			{
				if ((float)this.rect.Y > (float)hull1.Rect.Y - (float)hull1.Rect.Height / 2f)
				{
					this.flowForce = new Vector2(0f, -delta * (float)(0.016666666666666666 / (double)deltaTime));
				}
				else
				{
					this.flowForce = new Vector2(0f, delta * (float)(0.016666666666666666 / (double)deltaTime));
				}
				if (hull1.WaterVolume >= hull1.Volume / 1.05f)
				{
					hull1.LethalPressure += ((base.Submarine != null && base.Submarine.AtDamageDepth) ? 100f : 15f) * this.PressureDistributionSpeed * deltaTime;
				}
			}
			if (hull1.LethalPressure > 0f)
			{
				this.SimulateWaterFlowFromOutsideToConnectedHulls(hull1, this.GetWaterFlowFromOutside(hull1, deltaTime, true), deltaTime);
			}
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x00111160 File Offset: 0x0010F360
		private Hull GetOtherLinkedHull(Hull hull1)
		{
			if (this.linkedTo.Count != 2 || hull1 == null)
			{
				return null;
			}
			return ((this.linkedTo[0] == hull1) ? this.linkedTo[1] : this.linkedTo[0]) as Hull;
		}

		// Token: 0x06001B83 RID: 7043 RVA: 0x001111AE File Offset: 0x0010F3AE
		public void ResetWaterFlowThisFrame()
		{
			this.waterFlowThisFrame = 0f;
		}

		// Token: 0x06001B84 RID: 7044 RVA: 0x001111BC File Offset: 0x0010F3BC
		private void SimulateWaterFlowFromOutsideToConnectedHulls(Hull hull, float maxFlow, float deltaTime)
		{
			Gap.checkedHulls.Clear();
			Gap.checkedHulls.Add(hull);
			foreach (Gap connectedGap in hull.ConnectedGaps)
			{
				if (connectedGap != this && connectedGap.IsRoomToRoom && connectedGap.open > 0f)
				{
					Hull otherHull = connectedGap.GetOtherLinkedHull(hull);
					if (otherHull != null)
					{
						Gap.SimulateWaterFlowFromOutsideToConnectedHullsRecursive(otherHull, connectedGap, Gap.checkedHulls, hull, maxFlow, deltaTime);
					}
				}
			}
		}

		// Token: 0x06001B85 RID: 7045 RVA: 0x00111254 File Offset: 0x0010F454
		private static void SimulateWaterFlowFromOutsideToConnectedHullsRecursive(Hull targetHull, Gap gap, HashSet<Hull> checkedHulls, Hull originHull, float maxFlow, float deltaTime)
		{
			maxFlow = Math.Min(maxFlow, gap.GetWaterFlowFromOutside(targetHull, deltaTime, true)) * 0.95f;
			Hull sourceHull = gap.GetOtherLinkedHull(targetHull);
			if (sourceHull != null && !sourceHull.linkedTo.Contains(targetHull))
			{
				maxFlow *= 0.5f;
			}
			maxFlow -= gap.waterFlowThisFrame;
			if (maxFlow <= 0.001f)
			{
				return;
			}
			checkedHulls.Add(targetHull);
			gap.waterFlowThisFrame += maxFlow;
			targetHull.WaterVolume += maxFlow;
			if (targetHull.WaterVolume > targetHull.Volume)
			{
				targetHull.LethalPressure = Math.Max(targetHull.LethalPressure, MathHelper.Lerp(targetHull.LethalPressure, originHull.LethalPressure, 0.1f));
			}
			if (targetHull.LethalPressure <= 0f || targetHull.WaterVolume < targetHull.Volume)
			{
				return;
			}
			foreach (Gap connectedGap in targetHull.ConnectedGaps)
			{
				if (connectedGap != gap && connectedGap.IsRoomToRoom && connectedGap.open > 0f)
				{
					Hull otherHull = connectedGap.GetOtherLinkedHull(targetHull);
					if (otherHull != null && !checkedHulls.Contains(otherHull))
					{
						Gap.SimulateWaterFlowFromOutsideToConnectedHullsRecursive(otherHull, connectedGap, checkedHulls, originHull, maxFlow, deltaTime);
					}
				}
			}
		}

		// Token: 0x06001B86 RID: 7046 RVA: 0x001113A4 File Offset: 0x0010F5A4
		public bool RefreshOutsideCollider()
		{
			if (this.outsideCollisionBlocker == null)
			{
				return false;
			}
			if (this.IsRoomToRoom || base.Submarine == null || this.open <= 0f || this.linkedTo.Count == 0 || !(this.linkedTo[0] is Hull))
			{
				this.outsideCollisionBlocker.Enabled = false;
				return false;
			}
			if (this.outsideColliderRaycastTimer <= 0f)
			{
				this.UpdateOutsideColliderState((Hull)this.linkedTo[0]);
				this.outsideColliderRaycastTimer = (this.outsideCollisionBlocker.Enabled ? 0.1f : 1.5f);
			}
			return this.outsideCollisionBlocker.Enabled;
		}

		// Token: 0x06001B87 RID: 7047 RVA: 0x00111454 File Offset: 0x0010F654
		private void UpdateOutsideColliderState(Hull hull)
		{
			if (base.Submarine == null || this.IsRoomToRoom || Level.Loaded == null)
			{
				return;
			}
			Vector2 rayDir;
			if (this.IsHorizontal)
			{
				rayDir = new Vector2((float)Math.Sign(this.rect.Center.X - hull.Rect.Center.X), 0f);
			}
			else
			{
				rayDir = new Vector2(0f, (float)Math.Sign(this.rect.Y - this.rect.Height / 2 - (hull.Rect.Y - hull.Rect.Height / 2)));
			}
			Vector2 rayStart = ConvertUnits.ToSimUnits(this.WorldPosition);
			Vector2 rayEnd = rayStart + rayDir * 5f;
			List<VoronoiCell> levelCells = Level.Loaded.GetCells(this.WorldPosition, 1);
			foreach (VoronoiCell cell in levelCells)
			{
				if (cell.IsPointInside(this.WorldPosition))
				{
					this.outsideCollisionBlocker.Enabled = true;
					Vector2 colliderPos = rayStart - base.Submarine.SimPosition;
					float colliderRotation = MathUtils.VectorToAngle(rayDir) - 1.5707964f;
					this.outsideCollisionBlocker.SetTransformIgnoreContacts(ref colliderPos, colliderRotation);
					return;
				}
			}
			Body blockingBody = Submarine.CheckVisibility(rayStart, rayEnd, false, false, true, true, true, null);
			if (blockingBody != null)
			{
				if (blockingBody.UserData == base.Submarine)
				{
					return;
				}
				this.outsideCollisionBlocker.Enabled = true;
				Vector2 colliderPos2 = Submarine.LastPickedPosition - base.Submarine.SimPosition;
				float colliderRotation2 = MathUtils.VectorToAngle(Submarine.LastPickedNormal) - 1.5707964f;
				this.outsideCollisionBlocker.SetTransformIgnoreContacts(ref colliderPos2, colliderRotation2);
				return;
			}
			else
			{
				this.outsideCollisionBlocker.Enabled = false;
			}
		}

		// Token: 0x06001B88 RID: 7048 RVA: 0x00111630 File Offset: 0x0010F830
		private void UpdateOxygen(Hull hull1, Hull hull2, float deltaTime)
		{
			if (hull1 == null || hull2 == null)
			{
				return;
			}
			if (this.IsHorizontal && Math.Max(hull1.WorldSurface + hull1.WaveY[hull1.WaveY.Length - 1], hull2.WorldSurface + hull2.WaveY[0]) > (float)base.WorldRect.Y)
			{
				return;
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventGapOxygenUpdate>(delegate(IEventGapOxygenUpdate x)
			{
				bool? flag = x.OnGapOxygenUpdate(this, hull1, hull2);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			float totalOxygen = hull1.Oxygen + hull2.Oxygen;
			float totalVolume = hull1.Volume + hull2.Volume;
			float deltaOxygen = totalOxygen * hull1.Volume / totalVolume - hull1.Oxygen;
			deltaOxygen = MathHelper.Clamp(deltaOxygen, -30000f * deltaTime, 30000f * deltaTime);
			hull1.Oxygen += deltaOxygen;
			hull2.Oxygen -= deltaOxygen;
		}

		// Token: 0x06001B89 RID: 7049 RVA: 0x00111790 File Offset: 0x0010F990
		public static Gap FindAdjacent(IEnumerable<Gap> gaps, Vector2 worldPos, float allowedOrthogonalDist, bool allowRoomToRoom = false)
		{
			foreach (Gap gap in gaps)
			{
				if (gap.Open != 0f && (!gap.IsRoomToRoom || allowRoomToRoom))
				{
					if (gap.ConnectedWall != null)
					{
						int sectionIndex = gap.ConnectedWall.FindSectionIndex(gap.Position, false, false);
						if (sectionIndex > -1 && !gap.ConnectedWall.SectionBodyDisabled(sectionIndex))
						{
							continue;
						}
					}
					if ((gap.IsHorizontal || gap.IsDiagonal) && worldPos.Y < (float)gap.WorldRect.Y && worldPos.Y > (float)(gap.WorldRect.Y - gap.WorldRect.Height) && Math.Abs((float)gap.WorldRect.Center.X - worldPos.X) < allowedOrthogonalDist)
					{
						return gap;
					}
					if ((!gap.IsHorizontal || gap.IsDiagonal) && worldPos.X > (float)gap.WorldRect.X && worldPos.X < (float)gap.WorldRect.Right && Math.Abs((float)(gap.WorldRect.Y - gap.WorldRect.Height / 2) - worldPos.Y) < allowedOrthogonalDist)
					{
						return gap;
					}
				}
			}
			return null;
		}

		// Token: 0x06001B8A RID: 7050 RVA: 0x00111910 File Offset: 0x0010FB10
		private void RefreshOverlappingGaps()
		{
			this.overlappingGapFlowRateReduction = 0f;
			this.overlappingGaps.Clear();
			foreach (MapEntity linked in this.linkedTo)
			{
				Hull hull = linked as Hull;
				if (hull != null)
				{
					foreach (Gap connectedGap in hull.ConnectedGaps)
					{
						if (connectedGap != this && connectedGap.IsRoomToRoom == this.IsRoomToRoom)
						{
							if (connectedGap.open > this.open || (connectedGap.open == this.open && connectedGap.CreationIndex < this.CreationIndex))
							{
								Rectangle intersection = Rectangle.Intersect(this.rect.ToWorldRect(), connectedGap.rect.ToWorldRect());
								if (intersection.Width > 0 && intersection.Height > 0)
								{
									float relativeOverlap = this.IsHorizontal ? ((float)intersection.Height / (float)this.rect.Height) : ((float)intersection.Width / (float)this.rect.Width);
									this.overlappingGapFlowRateReduction += relativeOverlap * connectedGap.open;
									this.overlappingGaps.Add(connectedGap);
								}
							}
							if (this.overlappingGapFlowRateReduction >= 1f)
							{
								this.overlappingGapFlowRateReduction = 1f;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06001B8B RID: 7051 RVA: 0x00111AD4 File Offset: 0x0010FCD4
		private void FlagOverlappingGapsDirty()
		{
			foreach (Gap overlappingGap in this.overlappingGaps)
			{
				overlappingGap.overlappingGapsDirty = true;
			}
		}

		// Token: 0x06001B8C RID: 7052 RVA: 0x00111B28 File Offset: 0x0010FD28
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			Gap.GapList.Remove(this);
			foreach (Hull hull in Hull.HullList)
			{
				hull.ConnectedGaps.Remove(this);
			}
		}

		// Token: 0x06001B8D RID: 7053 RVA: 0x00111B94 File Offset: 0x0010FD94
		public override void Remove()
		{
			base.Remove();
			Gap.GapList.Remove(this);
			Gap.checkedHulls.Clear();
			foreach (Hull hull in Hull.HullList)
			{
				hull.ConnectedGaps.Remove(this);
			}
			if (this.outsideCollisionBlocker != null)
			{
				GameMain.World.Remove(this.outsideCollisionBlocker);
				this.outsideCollisionBlocker = null;
			}
		}

		// Token: 0x06001B8E RID: 7054 RVA: 0x00111C28 File Offset: 0x0010FE28
		public override void OnMapLoaded()
		{
			if (!this.DisableHullRechecks)
			{
				this.FindHulls();
			}
		}

		// Token: 0x06001B8F RID: 7055 RVA: 0x00111C38 File Offset: 0x0010FE38
		public static Gap Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			Rectangle rect;
			if (element.GetAttribute("rect") != null)
			{
				string key = "rect";
				Rectangle empty = Rectangle.Empty;
				rect = element.GetAttributeRect(key, empty);
			}
			else
			{
				rect = new Rectangle(int.Parse(element.GetAttribute("x").Value), int.Parse(element.GetAttribute("y").Value), int.Parse(element.GetAttribute("width").Value), int.Parse(element.GetAttribute("height").Value));
			}
			bool isHorizontal = rect.Height > rect.Width;
			XAttribute horizontalAttribute = element.GetAttribute("horizontal");
			if (horizontalAttribute != null)
			{
				isHorizontal = (horizontalAttribute.Value.ToString() == "true");
			}
			Gap g = new Gap(rect, isHorizontal, submarine, false, idRemap.GetOffsetId(element))
			{
				linkedToID = new List<ushort>(),
				Layer = element.GetAttributeString("Layer", null)
			};
			g.HiddenInGame = element.GetAttributeBool("HiddenInGame", g.HiddenInGame);
			return g;
		}

		// Token: 0x06001B90 RID: 7056 RVA: 0x00111D44 File Offset: 0x0010FF44
		public override XElement Save(XElement parentElement)
		{
			XElement element = new XElement("Gap");
			element.Add(new object[]
			{
				new XAttribute("ID", this.ID),
				new XAttribute("horizontal", this.IsHorizontal ? "true" : "false"),
				new XAttribute("HiddenInGame", base.HiddenInGame),
				new XAttribute("Layer", base.Layer ?? string.Empty)
			});
			element.Add(new XAttribute("rect", string.Concat(new string[]
			{
				((int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)).ToString(),
				",",
				((int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)).ToString(),
				",",
				this.rect.Width.ToString(),
				",",
				this.rect.Height.ToString()
			})));
			parentElement.Add(element);
			return element;
		}

		// Token: 0x06001B92 RID: 7058 RVA: 0x00111EC4 File Offset: 0x001100C4
		[CompilerGenerated]
		private void <Draw>g__DrawArrow|4_0(Hull targetHull, float arrowWidth, float arrowLength, Color clr, ref Gap.<>c__DisplayClass4_0 A_5)
		{
			Vector2 dir = this.IsHorizontal ? new Vector2((float)Math.Sign(targetHull.Rect.Center.X - this.rect.Center.X), 0f) : new Vector2(0f, (float)Math.Sign((float)this.rect.Y - (float)this.rect.Height / 2f - ((float)targetHull.Rect.Y - (float)targetHull.Rect.Height / 2f)));
			Vector2 arrowPos = new Vector2((float)base.WorldRect.Center.X, (float)(base.WorldRect.Y - base.WorldRect.Height / 2));
			if (base.Submarine != null)
			{
				arrowPos += base.Submarine.DrawPosition - base.Submarine.Position;
			}
			arrowPos.Y = -arrowPos.Y;
			arrowPos += new Vector2(dir.X * (float)(base.WorldRect.Width / 2), dir.Y * (float)(base.WorldRect.Height / 2));
			bool invalidDir = false;
			if (dir == Vector2.Zero)
			{
				invalidDir = true;
				dir = (this.IsHorizontal ? Vector2.UnitX : Vector2.UnitY);
			}
			GUI.Arrow.Draw(A_5.sb, arrowPos, invalidDir ? Color.Red : (clr * 0.8f), GUI.Arrow.Origin, MathUtils.VectorToAngle(dir) + 1.5707964f, this.IsHorizontal ? new Vector2(Math.Min((float)this.rect.Height, arrowWidth) / GUI.Arrow.size.X, arrowLength / GUI.Arrow.size.Y) : new Vector2(Math.Min((float)this.rect.Width, arrowWidth) / GUI.Arrow.size.X, arrowLength / GUI.Arrow.size.Y), SpriteEffects.None, new float?(A_5.depth));
		}

		// Token: 0x06001B93 RID: 7059 RVA: 0x001120F0 File Offset: 0x001102F0
		[CompilerGenerated]
		internal static void <set_Open>g__InformWaypointsAboutGapState|38_1(Gap gap, bool open)
		{
			foreach (WayPoint wp in WayPoint.WayPointList)
			{
				if (Gap.<set_Open>g__IsWaypointRightAboveGap|38_2(gap, wp))
				{
					wp.OnGapStateChanged(open, gap);
				}
			}
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x0011214C File Offset: 0x0011034C
		[CompilerGenerated]
		internal static bool <set_Open>g__IsWaypointRightAboveGap|38_2(Gap gap, WayPoint wp)
		{
			return wp.SpawnType == SpawnType.Path && gap.linkedTo.Contains(wp.CurrentHull) && wp.Position.Y >= (float)gap.Rect.Top && wp.Position.X <= (float)gap.Rect.Right && wp.Position.X >= (float)gap.Rect.Left;
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x001121EC File Offset: 0x001103EC
		[CompilerGenerated]
		private void <EmitParticles>g__CreateWaterSpatter|73_0(ref Gap.<>c__DisplayClass73_0 A_1)
		{
			Vector2 spatterPos = A_1.pos;
			float rotation;
			if (this.IsHorizontal)
			{
				rotation = ((this.LerpedFlowForce.X > 0f) ? 0f : 3.1415927f);
				spatterPos.Y = (float)(this.rect.Y - this.rect.Height / 2);
			}
			else
			{
				rotation = ((this.LerpedFlowForce.Y > 0f) ? -1.5707964f : 1.5707964f);
				spatterPos.X = (float)this.rect.Center.X;
			}
			Particle spatter = GameMain.ParticleManager.CreateParticle("waterspatter", (base.Submarine == null) ? spatterPos : (spatterPos + base.Submarine.Position), Vector2.Zero, rotation, this.flowTargetHull, 0f, null);
			if (spatter != null)
			{
				if (spatter.CurrentHull == null)
				{
					GameMain.ParticleManager.RemoveParticle(spatter);
				}
				spatter.Size *= MathHelper.Clamp(this.LerpedFlowForce.Length() / 200f, 0.5f, 1f);
			}
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x00112309 File Offset: 0x00110509
		[CompilerGenerated]
		private float <EmitParticles>g__GapSize|73_1(ref Gap.<>c__DisplayClass73_0 A_1)
		{
			return (float)(this.IsHorizontal ? this.rect.Height : this.rect.Width);
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x0011232C File Offset: 0x0011052C
		[CompilerGenerated]
		internal static void <UpdateRoomToRoom>g__changePressure|74_0(Hull hull, float target, float speed, float deltaTime)
		{
			float diff = target - hull.LethalPressure;
			float maxChange = 15f * speed * deltaTime;
			hull.LethalPressure += MathHelper.Clamp(diff, -maxChange, maxChange);
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x00112364 File Offset: 0x00110564
		[CompilerGenerated]
		internal static void <UpdateRoomToOut>g__CreateWave|76_0(Rectangle rect, Hull hull1, int index1, int index2, Vector2 flowForce, float deltaTime)
		{
			float vel = (float)(rect.Y - rect.Height / 2) - (hull1.Surface + hull1.WaveY[index1]);
			vel *= Math.Min(Math.Abs(flowForce.X) / 200f, 1f);
			if (vel > 0f)
			{
				hull1.WaveVel[index1] += vel * deltaTime;
				hull1.WaveVel[index2] += vel * deltaTime;
			}
		}

		// Token: 0x04000E02 RID: 3586
		private float particleTimer;

		// Token: 0x04000E03 RID: 3587
		public static List<Gap> GapList = new List<Gap>();

		// Token: 0x04000E04 RID: 3588
		private const float MaxFlowForce = 500f;

		// Token: 0x04000E05 RID: 3589
		public static bool ShowGaps = true;

		// Token: 0x04000E06 RID: 3590
		private const float OutsideColliderRaycastIntervalLowPrio = 1.5f;

		// Token: 0x04000E07 RID: 3591
		private const float OutsideColliderRaycastIntervalHighPrio = 0.1f;

		// Token: 0x04000E0A RID: 3594
		public readonly float GlowEffectT;

		// Token: 0x04000E0B RID: 3595
		private readonly List<Gap> overlappingGaps = new List<Gap>();

		// Token: 0x04000E0C RID: 3596
		private bool overlappingGapsDirty;

		// Token: 0x04000E0D RID: 3597
		private float overlappingGapFlowRateReduction;

		// Token: 0x04000E0E RID: 3598
		private float open;

		// Token: 0x04000E0F RID: 3599
		private Vector2 flowForce;

		// Token: 0x04000E10 RID: 3600
		private Hull flowTargetHull;

		// Token: 0x04000E11 RID: 3601
		private float openedTimer = 1f;

		// Token: 0x04000E12 RID: 3602
		private float higherSurface;

		// Token: 0x04000E13 RID: 3603
		private float lowerSurface;

		// Token: 0x04000E14 RID: 3604
		private float waterFlowThisFrame;

		// Token: 0x04000E15 RID: 3605
		private Vector2 lerpedFlowForce;

		// Token: 0x04000E16 RID: 3606
		public bool DisableHullRechecks;

		// Token: 0x04000E17 RID: 3607
		public bool PassAmbientLight;

		// Token: 0x04000E18 RID: 3608
		private Body outsideCollisionBlocker;

		// Token: 0x04000E19 RID: 3609
		private float outsideColliderRaycastTimer;

		// Token: 0x04000E1A RID: 3610
		private bool wasRoomToRoom;

		// Token: 0x04000E1B RID: 3611
		private Door connectedDoor;

		// Token: 0x04000E1C RID: 3612
		public Structure ConnectedWall;

		// Token: 0x04000E1D RID: 3613
		public readonly Dictionary<Identifier, SerializableProperty> properties;

		// Token: 0x04000E1E RID: 3614
		private int updateCount;

		// Token: 0x04000E1F RID: 3615
		private static readonly HashSet<Hull> checkedHulls = new HashSet<Hull>();
	}
}
