using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200022F RID: 559
	internal class Gap : MapEntity, ISerializableEntity
	{
		// Token: 0x17000B00 RID: 2816
		// (get) Token: 0x0600264F RID: 9807 RVA: 0x000FA292 File Offset: 0x000F8492
		// (set) Token: 0x06002650 RID: 9808 RVA: 0x000FA29A File Offset: 0x000F849A
		public bool IsHorizontal { get; private set; }

		// Token: 0x17000B01 RID: 2817
		// (get) Token: 0x06002651 RID: 9809 RVA: 0x000FA2A3 File Offset: 0x000F84A3
		public bool IsDiagonal { get; }

		// Token: 0x17000B02 RID: 2818
		// (get) Token: 0x06002652 RID: 9810 RVA: 0x000FA2AB File Offset: 0x000F84AB
		// (set) Token: 0x06002653 RID: 9811 RVA: 0x000FA2B4 File Offset: 0x000F84B4
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
								Gap.<set_Open>g__InformWaypointsAboutGapState|31_1(this, true);
								return;
							}
							if (this.open < prevValue && prevValue >= 1f)
							{
								Gap.<set_Open>g__InformWaypointsAboutGapState|31_1(this, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x17000B03 RID: 2819
		// (get) Token: 0x06002654 RID: 9812 RVA: 0x000FA38D File Offset: 0x000F858D
		public float Size
		{
			get
			{
				return (float)(this.IsHorizontal ? this.Rect.Height : this.Rect.Width);
			}
		}

		// Token: 0x17000B04 RID: 2820
		// (get) Token: 0x06002655 RID: 9813 RVA: 0x000FA3B0 File Offset: 0x000F85B0
		public float PressureDistributionSpeed
		{
			get
			{
				return this.Size / 100f * this.open;
			}
		}

		// Token: 0x17000B05 RID: 2821
		// (get) Token: 0x06002656 RID: 9814 RVA: 0x000FA3C5 File Offset: 0x000F85C5
		// (set) Token: 0x06002657 RID: 9815 RVA: 0x000FA3EE File Offset: 0x000F85EE
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

		// Token: 0x17000B06 RID: 2822
		// (get) Token: 0x06002658 RID: 9816 RVA: 0x000FA3F7 File Offset: 0x000F85F7
		public Vector2 LerpedFlowForce
		{
			get
			{
				return this.lerpedFlowForce;
			}
		}

		// Token: 0x17000B07 RID: 2823
		// (get) Token: 0x06002659 RID: 9817 RVA: 0x000FA3FF File Offset: 0x000F85FF
		public Hull FlowTargetHull
		{
			get
			{
				return this.flowTargetHull;
			}
		}

		// Token: 0x17000B08 RID: 2824
		// (get) Token: 0x0600265A RID: 9818 RVA: 0x000FA407 File Offset: 0x000F8607
		public bool IsRoomToRoom
		{
			get
			{
				return this.linkedTo.Count == 2;
			}
		}

		// Token: 0x17000B09 RID: 2825
		// (get) Token: 0x0600265B RID: 9819 RVA: 0x000FA417 File Offset: 0x000F8617
		// (set) Token: 0x0600265C RID: 9820 RVA: 0x000FA41F File Offset: 0x000F861F
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

		// Token: 0x17000B0A RID: 2826
		// (get) Token: 0x0600265D RID: 9821 RVA: 0x000FA42E File Offset: 0x000F862E
		public override string Name
		{
			get
			{
				return "Gap";
			}
		}

		// Token: 0x17000B0B RID: 2827
		// (get) Token: 0x0600265E RID: 9822 RVA: 0x000FA435 File Offset: 0x000F8635
		public Dictionary<Identifier, SerializableProperty> SerializableProperties
		{
			get
			{
				return this.properties;
			}
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x000FA43D File Offset: 0x000F863D
		public Gap(Rectangle rectangle) : this(rectangle, Submarine.MainSub)
		{
		}

		// Token: 0x06002660 RID: 9824 RVA: 0x000FA44B File Offset: 0x000F864B
		public Gap(Rectangle rect, Submarine submarine) : this(rect, rect.Width < rect.Height, submarine, false, 0)
		{
		}

		// Token: 0x06002661 RID: 9825 RVA: 0x000FA468 File Offset: 0x000F8668
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
			this.wasRoomToRoom = this.IsRoomToRoom;
			this.RefreshOutsideCollider();
			DebugConsole.Log("Created gap (" + this.ID.ToString() + ")");
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x000FA594 File Offset: 0x000F8794
		public override MapEntity Clone()
		{
			return new Gap(this.rect, this.IsHorizontal, base.Submarine, false, 0);
		}

		// Token: 0x06002663 RID: 9827 RVA: 0x000FA5B0 File Offset: 0x000F87B0
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

		// Token: 0x06002664 RID: 9828 RVA: 0x000FA628 File Offset: 0x000F8828
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

		// Token: 0x06002665 RID: 9829 RVA: 0x000FA6B8 File Offset: 0x000F88B8
		public override bool IsMouseOn(Vector2 position)
		{
			return Gap.ShowGaps && Submarine.RectContains(base.WorldRect, position, false) && !Submarine.RectContains(MathUtils.ExpandRect(base.WorldRect, -5), position, false);
		}

		// Token: 0x06002666 RID: 9830 RVA: 0x000FA6EC File Offset: 0x000F88EC
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

		// Token: 0x06002667 RID: 9831 RVA: 0x000FA820 File Offset: 0x000F8A20
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

		// Token: 0x06002668 RID: 9832 RVA: 0x000FAA70 File Offset: 0x000F8C70
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
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x000FACA0 File Offset: 0x000F8EA0
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
					Gap.<UpdateRoomToRoom>g__changePressure|67_0(hull1, avgLethality, this.PressureDistributionSpeed, deltaTime);
					Gap.<UpdateRoomToRoom>g__changePressure|67_0(hull2, avgLethality, this.PressureDistributionSpeed, deltaTime);
					return;
				}
				hull1.LethalPressure -= 10f * this.PressureDistributionSpeed * deltaTime;
				hull2.LethalPressure -= 10f * this.PressureDistributionSpeed * deltaTime;
			}
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x000FB444 File Offset: 0x000F9644
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

		// Token: 0x0600266B RID: 9835 RVA: 0x000FB49C File Offset: 0x000F969C
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
						Gap.<UpdateRoomToOut>g__CreateWave|69_0(this.rect, hull1, hull1.WaveY.Length - 1, hull1.WaveY.Length - 2, this.flowForce, deltaTime);
					}
					else
					{
						Gap.<UpdateRoomToOut>g__CreateWave|69_0(this.rect, hull1, 0, 1, this.flowForce, deltaTime);
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

		// Token: 0x0600266C RID: 9836 RVA: 0x000FB734 File Offset: 0x000F9934
		private Hull GetOtherLinkedHull(Hull hull1)
		{
			if (this.linkedTo.Count != 2 || hull1 == null)
			{
				return null;
			}
			return ((this.linkedTo[0] == hull1) ? this.linkedTo[1] : this.linkedTo[0]) as Hull;
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x000FB782 File Offset: 0x000F9982
		public void ResetWaterFlowThisFrame()
		{
			this.waterFlowThisFrame = 0f;
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x000FB790 File Offset: 0x000F9990
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

		// Token: 0x0600266F RID: 9839 RVA: 0x000FB828 File Offset: 0x000F9A28
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

		// Token: 0x06002670 RID: 9840 RVA: 0x000FB978 File Offset: 0x000F9B78
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

		// Token: 0x06002671 RID: 9841 RVA: 0x000FBA28 File Offset: 0x000F9C28
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

		// Token: 0x06002672 RID: 9842 RVA: 0x000FBC04 File Offset: 0x000F9E04
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

		// Token: 0x06002673 RID: 9843 RVA: 0x000FBD64 File Offset: 0x000F9F64
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

		// Token: 0x06002674 RID: 9844 RVA: 0x000FBEE4 File Offset: 0x000FA0E4
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

		// Token: 0x06002675 RID: 9845 RVA: 0x000FC0A8 File Offset: 0x000FA2A8
		private void FlagOverlappingGapsDirty()
		{
			foreach (Gap overlappingGap in this.overlappingGaps)
			{
				overlappingGap.overlappingGapsDirty = true;
			}
		}

		// Token: 0x06002676 RID: 9846 RVA: 0x000FC0FC File Offset: 0x000FA2FC
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			Gap.GapList.Remove(this);
			foreach (Hull hull in Hull.HullList)
			{
				hull.ConnectedGaps.Remove(this);
			}
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x000FC168 File Offset: 0x000FA368
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

		// Token: 0x06002678 RID: 9848 RVA: 0x000FC1FC File Offset: 0x000FA3FC
		public override void OnMapLoaded()
		{
			if (!this.DisableHullRechecks)
			{
				this.FindHulls();
			}
		}

		// Token: 0x06002679 RID: 9849 RVA: 0x000FC20C File Offset: 0x000FA40C
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

		// Token: 0x0600267A RID: 9850 RVA: 0x000FC318 File Offset: 0x000FA518
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

		// Token: 0x0600267C RID: 9852 RVA: 0x000FC498 File Offset: 0x000FA698
		[CompilerGenerated]
		internal static void <set_Open>g__InformWaypointsAboutGapState|31_1(Gap gap, bool open)
		{
			foreach (WayPoint wp in WayPoint.WayPointList)
			{
				if (Gap.<set_Open>g__IsWaypointRightAboveGap|31_2(gap, wp))
				{
					wp.OnGapStateChanged(open, gap);
				}
			}
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x000FC4F4 File Offset: 0x000FA6F4
		[CompilerGenerated]
		internal static bool <set_Open>g__IsWaypointRightAboveGap|31_2(Gap gap, WayPoint wp)
		{
			return wp.SpawnType == SpawnType.Path && gap.linkedTo.Contains(wp.CurrentHull) && wp.Position.Y >= (float)gap.Rect.Top && wp.Position.X <= (float)gap.Rect.Right && wp.Position.X >= (float)gap.Rect.Left;
		}

		// Token: 0x0600267E RID: 9854 RVA: 0x000FC57C File Offset: 0x000FA77C
		[CompilerGenerated]
		internal static void <UpdateRoomToRoom>g__changePressure|67_0(Hull hull, float target, float speed, float deltaTime)
		{
			float diff = target - hull.LethalPressure;
			float maxChange = 15f * speed * deltaTime;
			hull.LethalPressure += MathHelper.Clamp(diff, -maxChange, maxChange);
		}

		// Token: 0x0600267F RID: 9855 RVA: 0x000FC5B4 File Offset: 0x000FA7B4
		[CompilerGenerated]
		internal static void <UpdateRoomToOut>g__CreateWave|69_0(Rectangle rect, Hull hull1, int index1, int index2, Vector2 flowForce, float deltaTime)
		{
			float vel = (float)(rect.Y - rect.Height / 2) - (hull1.Surface + hull1.WaveY[index1]);
			vel *= Math.Min(Math.Abs(flowForce.X) / 200f, 1f);
			if (vel > 0f)
			{
				hull1.WaveVel[index1] += vel * deltaTime;
				hull1.WaveVel[index2] += vel * deltaTime;
			}
		}

		// Token: 0x040012BA RID: 4794
		public static List<Gap> GapList = new List<Gap>();

		// Token: 0x040012BB RID: 4795
		private const float MaxFlowForce = 500f;

		// Token: 0x040012BC RID: 4796
		public static bool ShowGaps = true;

		// Token: 0x040012BD RID: 4797
		private const float OutsideColliderRaycastIntervalLowPrio = 1.5f;

		// Token: 0x040012BE RID: 4798
		private const float OutsideColliderRaycastIntervalHighPrio = 0.1f;

		// Token: 0x040012C1 RID: 4801
		public readonly float GlowEffectT;

		// Token: 0x040012C2 RID: 4802
		private readonly List<Gap> overlappingGaps = new List<Gap>();

		// Token: 0x040012C3 RID: 4803
		private bool overlappingGapsDirty;

		// Token: 0x040012C4 RID: 4804
		private float overlappingGapFlowRateReduction;

		// Token: 0x040012C5 RID: 4805
		private float open;

		// Token: 0x040012C6 RID: 4806
		private Vector2 flowForce;

		// Token: 0x040012C7 RID: 4807
		private Hull flowTargetHull;

		// Token: 0x040012C8 RID: 4808
		private float openedTimer = 1f;

		// Token: 0x040012C9 RID: 4809
		private float higherSurface;

		// Token: 0x040012CA RID: 4810
		private float lowerSurface;

		// Token: 0x040012CB RID: 4811
		private float waterFlowThisFrame;

		// Token: 0x040012CC RID: 4812
		private Vector2 lerpedFlowForce;

		// Token: 0x040012CD RID: 4813
		public bool DisableHullRechecks;

		// Token: 0x040012CE RID: 4814
		public bool PassAmbientLight;

		// Token: 0x040012CF RID: 4815
		private Body outsideCollisionBlocker;

		// Token: 0x040012D0 RID: 4816
		private float outsideColliderRaycastTimer;

		// Token: 0x040012D1 RID: 4817
		private bool wasRoomToRoom;

		// Token: 0x040012D2 RID: 4818
		private Door connectedDoor;

		// Token: 0x040012D3 RID: 4819
		public Structure ConnectedWall;

		// Token: 0x040012D4 RID: 4820
		public readonly Dictionary<Identifier, SerializableProperty> properties;

		// Token: 0x040012D5 RID: 4821
		private int updateCount;

		// Token: 0x040012D6 RID: 4822
		private static readonly HashSet<Hull> checkedHulls = new HashSet<Hull>();
	}
}
