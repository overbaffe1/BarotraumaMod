using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000491 RID: 1169
	internal class DockingPort : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06003E42 RID: 15938 RVA: 0x0018FBE4 File Offset: 0x0018DDE4
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.docked);
			if (this.docked)
			{
				msg.WriteUInt16(this.DockingTarget.item.ID);
				msg.WriteBoolean(this.IsLocked);
			}
		}

		// Token: 0x06003E43 RID: 15939 RVA: 0x0018FC1C File Offset: 0x0018DE1C
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			DockingPort.AllowOutpostAutoDocking allowOutpostAutoDocking = (DockingPort.AllowOutpostAutoDocking)msg.ReadByte();
			if (this.outpostAutoDockingPromptShown && CampaignMode.AllowedToManageCampaign(c, ClientPermissions.ManageMap))
			{
				this.allowOutpostAutoDocking = allowOutpostAutoDocking;
			}
		}

		// Token: 0x17001066 RID: 4198
		// (get) Token: 0x06003E44 RID: 15940 RVA: 0x0018FC4C File Offset: 0x0018DE4C
		public static IEnumerable<DockingPort> List
		{
			get
			{
				return DockingPort.list;
			}
		}

		// Token: 0x17001067 RID: 4199
		// (get) Token: 0x06003E45 RID: 15941 RVA: 0x0018FC53 File Offset: 0x0018DE53
		// (set) Token: 0x06003E46 RID: 15942 RVA: 0x0018FC5B File Offset: 0x0018DE5B
		public int DockingDir { get; set; }

		// Token: 0x17001068 RID: 4200
		// (get) Token: 0x06003E47 RID: 15943 RVA: 0x0018FC64 File Offset: 0x0018DE64
		// (set) Token: 0x06003E48 RID: 15944 RVA: 0x0018FC6C File Offset: 0x0018DE6C
		[Serialize("32.0,32.0", IsPropertySaveable.No, "How close the docking port has to be to another port to dock.", "", false)]
		public Vector2 DistanceTolerance { get; set; }

		// Token: 0x17001069 RID: 4201
		// (get) Token: 0x06003E49 RID: 15945 RVA: 0x0018FC75 File Offset: 0x0018DE75
		// (set) Token: 0x06003E4A RID: 15946 RVA: 0x0018FC7D File Offset: 0x0018DE7D
		[Serialize(32f, IsPropertySaveable.No, "How close together the docking ports are forced when docked.", "", false)]
		public float DockedDistance { get; set; }

		// Token: 0x1700106A RID: 4202
		// (get) Token: 0x06003E4B RID: 15947 RVA: 0x0018FC86 File Offset: 0x0018DE86
		// (set) Token: 0x06003E4C RID: 15948 RVA: 0x0018FC8E File Offset: 0x0018DE8E
		[Serialize(true, IsPropertySaveable.No, "Is the port horizontal.", "", false)]
		public bool IsHorizontal { get; set; }

		// Token: 0x1700106B RID: 4203
		// (get) Token: 0x06003E4D RID: 15949 RVA: 0x0018FC97 File Offset: 0x0018DE97
		// (set) Token: 0x06003E4E RID: 15950 RVA: 0x0018FC9F File Offset: 0x0018DE9F
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, this docking port is used when spawning the submarine docked to an outpost (if possible).", "", false)]
		public bool MainDockingPort { get; set; }

		// Token: 0x1700106C RID: 4204
		// (get) Token: 0x06003E4F RID: 15951 RVA: 0x0018FCA8 File Offset: 0x0018DEA8
		// (set) Token: 0x06003E50 RID: 15952 RVA: 0x0018FCB0 File Offset: 0x0018DEB0
		[Editable]
		[Serialize(true, IsPropertySaveable.No, "Should the OnUse StatusEffects trigger when docking (on vanilla docking ports these effects emit particles and play a sound).)", "", false)]
		public bool ApplyEffectsOnDocking { get; set; }

		// Token: 0x1700106D RID: 4205
		// (get) Token: 0x06003E51 RID: 15953 RVA: 0x0018FCB9 File Offset: 0x0018DEB9
		// (set) Token: 0x06003E52 RID: 15954 RVA: 0x0018FCC1 File Offset: 0x0018DEC1
		[Editable]
		[Serialize(DockingPort.DirectionType.None, IsPropertySaveable.No, "Which direction the port is allowed to dock in. For example, \"Top\" would mean the port can dock to another port above it.\nNormally there's no need to touch this setting, but if you notice the docking position is incorrect (for example due to some unusual docking port configuration without hulls or doors), you can use this to enforce the direction.", "", false)]
		public DockingPort.DirectionType ForceDockingDirection { get; set; }

		// Token: 0x1700106E RID: 4206
		// (get) Token: 0x06003E53 RID: 15955 RVA: 0x0018FCCA File Offset: 0x0018DECA
		// (set) Token: 0x06003E54 RID: 15956 RVA: 0x0018FCD2 File Offset: 0x0018DED2
		[Serialize(false, IsPropertySaveable.Yes, "Was the docking port docked at the end of the previous round.", "", false)]
		public bool WasDocked { get; set; }

		// Token: 0x1700106F RID: 4207
		// (get) Token: 0x06003E55 RID: 15957 RVA: 0x0018FCDB File Offset: 0x0018DEDB
		// (set) Token: 0x06003E56 RID: 15958 RVA: 0x0018FCE3 File Offset: 0x0018DEE3
		public DockingPort DockingTarget { get; private set; }

		// Token: 0x17001070 RID: 4208
		// (get) Token: 0x06003E57 RID: 15959 RVA: 0x0018FCEC File Offset: 0x0018DEEC
		public bool AtStartExit
		{
			get
			{
				Submarine submarine = base.Item.Submarine;
				return submarine != null && submarine.AtStartExit;
			}
		}

		// Token: 0x17001071 RID: 4209
		// (get) Token: 0x06003E58 RID: 15960 RVA: 0x0018FD10 File Offset: 0x0018DF10
		public bool AtEndExit
		{
			get
			{
				Submarine submarine = base.Item.Submarine;
				return submarine != null && submarine.AtEndExit;
			}
		}

		// Token: 0x17001072 RID: 4210
		// (get) Token: 0x06003E59 RID: 15961 RVA: 0x0018FD34 File Offset: 0x0018DF34
		// (set) Token: 0x06003E5A RID: 15962 RVA: 0x0018FD3C File Offset: 0x0018DF3C
		public Door Door { get; private set; }

		// Token: 0x17001073 RID: 4211
		// (get) Token: 0x06003E5B RID: 15963 RVA: 0x0018FD45 File Offset: 0x0018DF45
		// (set) Token: 0x06003E5C RID: 15964 RVA: 0x0018FD4D File Offset: 0x0018DF4D
		public bool Docked
		{
			get
			{
				return this.docked;
			}
			set
			{
				if (this.docked || !value)
				{
					if (this.docked && !value)
					{
						this.Undock(true);
					}
					return;
				}
				if (this.DockingTarget == null)
				{
					this.AttemptDock();
				}
				if (this.DockingTarget == null)
				{
					return;
				}
				this.docked = true;
			}
		}

		// Token: 0x17001074 RID: 4212
		// (get) Token: 0x06003E5D RID: 15965 RVA: 0x0018FD8D File Offset: 0x0018DF8D
		public bool IsLocked
		{
			get
			{
				if (!(this.joint is WeldJoint))
				{
					DockingPort dockingTarget = this.DockingTarget;
					return ((dockingTarget != null) ? dockingTarget.joint : null) is WeldJoint;
				}
				return true;
			}
		}

		// Token: 0x17001075 RID: 4213
		// (get) Token: 0x06003E5E RID: 15966 RVA: 0x0018FDB8 File Offset: 0x0018DFB8
		public bool AnotherPortInProximity
		{
			get
			{
				return this.FindAdjacentPort() != null;
			}
		}

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x06003E5F RID: 15967 RVA: 0x0018FDC4 File Offset: 0x0018DFC4
		// (remove) Token: 0x06003E60 RID: 15968 RVA: 0x0018FDFC File Offset: 0x0018DFFC
		public event Action OnDocked;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06003E61 RID: 15969 RVA: 0x0018FE34 File Offset: 0x0018E034
		// (remove) Token: 0x06003E62 RID: 15970 RVA: 0x0018FE6C File Offset: 0x0018E06C
		public event Action OnUnDocked;

		// Token: 0x06003E63 RID: 15971 RVA: 0x0018FEA4 File Offset: 0x0018E0A4
		public DockingPort(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string texturePath = subElement.GetAttributeString("texture", "");
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "sprite")
				{
					this.overlaySprite = new Sprite(subElement, texturePath.Contains("/") ? "" : Path.GetDirectoryName(item.Prefab.FilePath), "", false, 1f);
				}
			}
			this.IsActive = true;
			DockingPort.list.Add(this);
		}

		// Token: 0x06003E64 RID: 15972 RVA: 0x0018FF7C File Offset: 0x0018E17C
		public override void FlipX(bool relativeToSub)
		{
			if (this.DockingTarget != null)
			{
				if (this.IsHorizontal)
				{
					this.DockingDir = 0;
					this.DockingDir = this.GetDir(this.DockingTarget);
					this.DockingTarget.DockingDir = -this.DockingDir;
				}
				DockingPort prevDockingTarget = this.DockingTarget;
				this.Undock(false);
				this.Dock(prevDockingTarget);
				this.Lock(true, false, false);
			}
		}

		// Token: 0x06003E65 RID: 15973 RVA: 0x0018FFE2 File Offset: 0x0018E1E2
		public override void FlipY(bool relativeToSub)
		{
			this.FlipX(relativeToSub);
		}

		// Token: 0x06003E66 RID: 15974 RVA: 0x0018FFEC File Offset: 0x0018E1EC
		private DockingPort FindAdjacentPort()
		{
			float closestDist = float.MaxValue;
			DockingPort closestPort = null;
			foreach (DockingPort port in DockingPort.list)
			{
				if (port != this && port.item.Submarine != this.item.Submarine && port.IsHorizontal == this.IsHorizontal)
				{
					float xDist = Math.Abs(port.item.WorldPosition.X - this.item.WorldPosition.X);
					if (xDist <= this.DistanceTolerance.X)
					{
						float yDist = Math.Abs(port.item.WorldPosition.Y - this.item.WorldPosition.Y);
						if (yDist <= this.DistanceTolerance.Y)
						{
							float dist = xDist + yDist;
							if (port.item.NonInteractable)
							{
								dist *= 2f;
							}
							if (dist < closestDist)
							{
								closestPort = port;
								closestDist = dist;
							}
						}
					}
				}
			}
			return closestPort;
		}

		// Token: 0x06003E67 RID: 15975 RVA: 0x00190110 File Offset: 0x0018E310
		private void AttemptDock()
		{
			DockingPort adjacentPort = this.FindAdjacentPort();
			if (adjacentPort != null)
			{
				this.Dock(adjacentPort);
			}
		}

		// Token: 0x06003E68 RID: 15976 RVA: 0x00190130 File Offset: 0x0018E330
		public void Dock(DockingPort target)
		{
			if (this.item.Submarine.DockedTo.Contains(target.item.Submarine))
			{
				return;
			}
			this.forceLockTimer = 0f;
			this.dockingCooldown = 0.1f;
			if (this.DockingTarget != null)
			{
				this.Undock(true);
			}
			if (target.item.Submarine == this.item.Submarine)
			{
				DebugConsole.ThrowError("Error - tried to dock a submarine to itself", null, null, false, false);
				this.DockingTarget = null;
				return;
			}
			target.InitializeLinks();
			if (!this.item.linkedTo.Contains(target.item))
			{
				this.item.linkedTo.Add(target.item);
			}
			if (!target.item.linkedTo.Contains(this.item))
			{
				target.item.linkedTo.Add(this.item);
			}
			if (!target.item.Submarine.DockedTo.Contains(this.item.Submarine))
			{
				target.item.Submarine.ConnectedDockingPorts.Add(this.item.Submarine, target);
				target.item.Submarine.RefreshConnectedSubs();
			}
			if (!this.item.Submarine.DockedTo.Contains(target.item.Submarine))
			{
				this.item.Submarine.ConnectedDockingPorts.Add(target.item.Submarine, this);
				this.item.Submarine.RefreshConnectedSubs();
			}
			this.DockingTarget = target;
			this.DockingTarget.DockingTarget = this;
			this.docked = true;
			this.DockingTarget.Docked = true;
			if (Character.Controlled != null && (Character.Controlled.Submarine == this.DockingTarget.item.Submarine || Character.Controlled.Submarine == this.item.Submarine))
			{
				GameMain.GameScreen.Cam.Shake = Vector2.Distance(this.DockingTarget.item.Submarine.Velocity, this.item.Submarine.Velocity);
			}
			this.DockingDir = this.GetDir(this.DockingTarget);
			this.DockingTarget.DockingDir = -this.DockingDir;
			this.CreateJoint(false);
			if (GameMain.Server != null)
			{
				Submarine submarine = this.item.Submarine;
				if (submarine == null || !submarine.Loading)
				{
					this.item.CreateServerEvent<DockingPort>(this);
				}
			}
			Action onDocked = this.OnDocked;
			if (onDocked != null)
			{
				onDocked();
			}
			this.OnDocked = null;
			this.WasDocked = true;
			this.DockingTarget.Docked = true;
		}

		// Token: 0x06003E69 RID: 15977 RVA: 0x001903DC File Offset: 0x0018E5DC
		public void Lock(bool isNetworkMessage, bool applyEffects = true, bool moveSubs = true)
		{
			if (this.DockingTarget == null)
			{
				DebugConsole.ThrowError("Error - attempted to lock a docking port that's not connected to anything", null, null, false, false);
				return;
			}
			if (this.joint == null)
			{
				string str = "Error while locking a docking port (joint between submarines doesn't exist). Submarine: ";
				Submarine submarine = this.item.Submarine;
				string str2 = ((submarine != null) ? submarine.Info.Name : null) ?? "null";
				string str3 = ", target submarine: ";
				Submarine submarine2 = this.DockingTarget.item.Submarine;
				string errorMsg = str + str2 + str3 + (((submarine2 != null) ? submarine2.Info.Name : null) ?? "null");
				GameAnalyticsManager.AddErrorEventOnce("DockingPort.Lock:JointNotCreated", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (!(this.joint is WeldJoint))
			{
				this.DockingDir = this.GetDir(this.DockingTarget);
				this.DockingTarget.DockingDir = -this.DockingDir;
				if (applyEffects && this.ApplyEffectsOnDocking)
				{
					base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, null, null, 1f);
				}
				if (moveSubs)
				{
					Vector2 jointDiff = this.joint.WorldAnchorB - this.joint.WorldAnchorA;
					if (this.item.Submarine.PhysicsBody.Mass < this.DockingTarget.item.Submarine.PhysicsBody.Mass || this.DockingTarget.item.Submarine.Info.IsOutpost)
					{
						this.item.Submarine.SubBody.SetPosition(this.item.Submarine.SubBody.Position + ConvertUnits.ToDisplayUnits(jointDiff));
					}
					else if (this.DockingTarget.item.Submarine.PhysicsBody.Mass < this.item.Submarine.PhysicsBody.Mass || this.item.Submarine.Info.IsOutpost)
					{
						this.DockingTarget.item.Submarine.SubBody.SetPosition(this.DockingTarget.item.Submarine.SubBody.Position - ConvertUnits.ToDisplayUnits(jointDiff));
					}
				}
				this.ConnectWireBetweenPorts();
				this.CreateJoint(true);
				this.item.SendSignal("1", "on_dock");
				this.DockingTarget.Item.SendSignal("1", "on_dock");
				if (GameMain.Server != null)
				{
					Submarine submarine3 = this.item.Submarine;
					if (submarine3 == null || !submarine3.Loading)
					{
						this.item.CreateServerEvent<DockingPort>(this);
					}
				}
			}
			List<MapEntity> removedEntities = (from e in this.item.linkedTo
			where e.Removed
			select e).ToList<MapEntity>();
			foreach (MapEntity removed in removedEntities)
			{
				this.item.linkedTo.Remove(removed);
			}
			if (!this.item.linkedTo.Any((MapEntity e) => e is Hull))
			{
				if (!this.DockingTarget.item.linkedTo.Any((MapEntity e) => e is Hull))
				{
					this.CreateHulls();
				}
			}
			if (this.Door != null && this.DockingTarget.Door != null)
			{
				WayPoint myWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => this.Door.LinkedGap == wp.ConnectedGap);
				WayPoint targetWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => this.DockingTarget.Door.LinkedGap == wp.ConnectedGap);
				if (myWayPoint != null && targetWayPoint != null)
				{
					myWayPoint.FindHull();
					targetWayPoint.FindHull();
					myWayPoint.ConnectTo(targetWayPoint);
				}
			}
		}

		// Token: 0x06003E6A RID: 15978 RVA: 0x001907C4 File Offset: 0x0018E9C4
		private void CreateJoint(bool useWeldJoint)
		{
			if (this.joint != null)
			{
				GameMain.World.Remove(this.joint);
				this.joint = null;
			}
			Vector2 offset = this.IsHorizontal ? (Vector2.UnitX * (float)this.DockingDir) : (Vector2.UnitY * (float)this.DockingDir);
			offset *= this.DockedDistance * 0.5f * this.item.Scale;
			Vector2 pos = this.item.WorldPosition + offset;
			Vector2 pos2 = this.DockingTarget.item.WorldPosition - offset;
			if (useWeldJoint)
			{
				this.joint = JointFactory.CreateWeldJoint(GameMain.World, this.item.Submarine.PhysicsBody.FarseerBody, this.DockingTarget.item.Submarine.PhysicsBody.FarseerBody, ConvertUnits.ToSimUnits(pos), ConvertUnits.ToSimUnits(pos2), true);
				((WeldJoint)this.joint).FrequencyHz = 1f;
				this.joint.CollideConnected = false;
				return;
			}
			DistanceJoint distanceJoint = JointFactory.CreateDistanceJoint(GameMain.World, this.item.Submarine.PhysicsBody.FarseerBody, this.DockingTarget.item.Submarine.PhysicsBody.FarseerBody, ConvertUnits.ToSimUnits(pos), ConvertUnits.ToSimUnits(pos2), true);
			distanceJoint.Length = 0.01f;
			distanceJoint.Frequency = 1f;
			distanceJoint.DampingRatio = 0.8f;
			this.joint = distanceJoint;
			this.joint.CollideConnected = true;
		}

		// Token: 0x06003E6B RID: 15979 RVA: 0x00190954 File Offset: 0x0018EB54
		public int GetDir(DockingPort dockingTarget = null)
		{
			int forcedDockingDir = this.GetForcedDockingDir();
			if (forcedDockingDir != 0)
			{
				return forcedDockingDir;
			}
			if (dockingTarget != null)
			{
				forcedDockingDir = -dockingTarget.GetForcedDockingDir();
				if (forcedDockingDir != 0)
				{
					return forcedDockingDir;
				}
			}
			if (this.DockingDir != 0)
			{
				return this.DockingDir;
			}
			if (this.Door != null && this.Door.LinkedGap.linkedTo.Count > 0)
			{
				Hull refHull = null;
				float largestHullSize = 0f;
				foreach (MapEntity linked in this.Door.LinkedGap.linkedTo)
				{
					Hull hull = linked as Hull;
					if (hull != null && hull.Volume > largestHullSize)
					{
						refHull = hull;
						largestHullSize = hull.Volume;
					}
				}
				if (refHull != null)
				{
					if (!this.IsHorizontal)
					{
						return Math.Sign(this.Door.Item.WorldPosition.Y - refHull.WorldPosition.Y);
					}
					return Math.Sign(this.Door.Item.WorldPosition.X - refHull.WorldPosition.X);
				}
			}
			bool flag;
			if (dockingTarget == null)
			{
				flag = (null != null);
			}
			else
			{
				Door door = dockingTarget.Door;
				flag = (((door != null) ? door.LinkedGap : null) != null);
			}
			if (flag && dockingTarget.Door.LinkedGap.linkedTo.Count > 0)
			{
				Hull refHull2 = null;
				float largestHullSize2 = 0f;
				foreach (MapEntity linked2 in dockingTarget.Door.LinkedGap.linkedTo)
				{
					Hull hull2 = linked2 as Hull;
					if (hull2 != null && hull2.Volume > largestHullSize2)
					{
						refHull2 = hull2;
						largestHullSize2 = hull2.Volume;
					}
				}
				if (refHull2 != null)
				{
					if (!this.IsHorizontal)
					{
						return Math.Sign(refHull2.WorldPosition.Y - dockingTarget.Door.Item.WorldPosition.Y);
					}
					return Math.Sign(refHull2.WorldPosition.X - dockingTarget.Door.Item.WorldPosition.X);
				}
			}
			if (dockingTarget != null)
			{
				int dir = this.IsHorizontal ? Math.Sign(dockingTarget.item.WorldPosition.X - this.item.WorldPosition.X) : Math.Sign(dockingTarget.item.WorldPosition.Y - this.item.WorldPosition.Y);
				if (dir != 0)
				{
					return dir;
				}
			}
			if (this.item.Submarine == null)
			{
				return 0;
			}
			if (!this.IsHorizontal)
			{
				return Math.Sign(this.item.WorldPosition.Y - this.item.Submarine.WorldPosition.Y);
			}
			return Math.Sign(this.item.WorldPosition.X - this.item.Submarine.WorldPosition.X);
		}

		// Token: 0x06003E6C RID: 15980 RVA: 0x00190C5C File Offset: 0x0018EE5C
		private int GetForcedDockingDir()
		{
			switch (this.ForceDockingDirection)
			{
			case DockingPort.DirectionType.Top:
				return 1;
			case DockingPort.DirectionType.Bottom:
				return -1;
			case DockingPort.DirectionType.Left:
				return -1;
			case DockingPort.DirectionType.Right:
				return 1;
			default:
				return 0;
			}
		}

		// Token: 0x06003E6D RID: 15981 RVA: 0x00190C94 File Offset: 0x0018EE94
		private void ConnectWireBetweenPorts()
		{
			Wire wire = this.item.GetComponent<Wire>();
			if (wire == null)
			{
				return;
			}
			wire.Locked = true;
			wire.Hidden = true;
			if (base.Item.Connections == null)
			{
				return;
			}
			Connection powerConnection = base.Item.Connections.Find((Connection c) => c.IsPower);
			if (powerConnection == null)
			{
				return;
			}
			if (this.DockingTarget == null || this.DockingTarget.item.Connections == null)
			{
				return;
			}
			Connection recipient = this.DockingTarget.item.Connections.Find((Connection c) => c.IsPower);
			if (recipient == null)
			{
				return;
			}
			wire.RemoveConnection(this.item);
			wire.RemoveConnection(this.DockingTarget.item);
			powerConnection.TryAddLink(wire);
			wire.TryConnect(powerConnection, false, false);
			recipient.TryAddLink(wire);
			wire.TryConnect(recipient, false, false);
			Powered.ChangedConnections.Add(powerConnection);
			Powered.ChangedConnections.Add(recipient);
		}

		// Token: 0x06003E6E RID: 15982 RVA: 0x00190DB0 File Offset: 0x0018EFB0
		private void CreateDoorBody()
		{
			if (this.doorBody != null)
			{
				GameMain.World.Remove(this.doorBody);
				this.doorBody = null;
			}
			Vector2 position = ConvertUnits.ToSimUnits(this.item.Position + (this.DockingTarget.Door.Item.WorldPosition - this.item.WorldPosition));
			if (!MathUtils.IsValid(position))
			{
				string errorMsg = string.Concat(new string[]
				{
					"Attempted to create a door body at an invalid position (item pos: ",
					this.item.Position.ToString(),
					", item world pos: ",
					this.item.WorldPosition.ToString(),
					", docking target world pos: ",
					this.DockingTarget.Door.Item.WorldPosition.ToString(),
					")\n",
					Environment.StackTrace.CleanupStackTrace()
				});
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("DockingPort.CreateDoorBody:InvalidPosition", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				position = Vector2.Zero;
			}
			this.doorBody = GameMain.World.CreateRectangle(this.DockingTarget.Door.Body.Width, this.DockingTarget.Door.Body.Height, 1f, position, 0f, BodyType.Static, Category.Cat1, Category.All, true);
			this.doorBody.UserData = this.DockingTarget.Door;
			this.doorBody.CollisionCategories = Category.Cat1;
			this.doorBody.BodyType = BodyType.Static;
		}

		// Token: 0x06003E6F RID: 15983 RVA: 0x00190F50 File Offset: 0x0018F150
		private void CreateHulls()
		{
			Rectangle[] hullRects = new Rectangle[]
			{
				this.item.WorldRect,
				this.DockingTarget.item.WorldRect
			};
			Submarine[] subs = new Submarine[]
			{
				this.item.Submarine,
				this.DockingTarget.item.Submarine
			};
			this.bodies = new Body[4];
			if (this.DockingTarget.Door != null)
			{
				this.CreateDoorBody();
			}
			if (this.Door != null)
			{
				this.DockingTarget.CreateDoorBody();
			}
			if (this.IsHorizontal)
			{
				if (hullRects[0].Center.X > hullRects[1].Center.X)
				{
					hullRects = new Rectangle[]
					{
						this.DockingTarget.item.WorldRect,
						this.item.WorldRect
					};
					subs = new Submarine[]
					{
						this.DockingTarget.item.Submarine,
						this.item.Submarine
					};
				}
				int scaledDockedDistance = (int)(this.DockedDistance / 2f * this.item.Scale);
				hullRects[0] = new Rectangle(hullRects[0].Center.X, hullRects[0].Y, scaledDockedDistance, hullRects[0].Height);
				hullRects[1] = new Rectangle(hullRects[1].Center.X - scaledDockedDistance, hullRects[1].Y, scaledDockedDistance, hullRects[1].Height);
				int leftSubRightSide = int.MinValue;
				int rightSubLeftSide = int.MaxValue;
				foreach (Hull hull in Hull.HullList)
				{
					for (int i = 0; i < 2; i++)
					{
						if (hull.Submarine == subs[i] && hull.WorldRect.Y - 5 >= hullRects[i].Y - hullRects[i].Height && hull.WorldRect.Y - hull.WorldRect.Height + 5 <= hullRects[i].Y)
						{
							if (i == 0)
							{
								if (hull.WorldPosition.X <= (float)hullRects[0].Center.X)
								{
									leftSubRightSide = Math.Max(hull.WorldRect.Right, leftSubRightSide);
								}
							}
							else if (hull.WorldPosition.X >= (float)hullRects[1].Center.X)
							{
								rightSubLeftSide = Math.Min(hull.WorldRect.X, rightSubLeftSide);
							}
						}
					}
				}
				if (leftSubRightSide == -2147483648 || rightSubLeftSide == 2147483647)
				{
					DebugConsole.NewMessage("Creating hulls between docking ports failed. Could not find a hull next to the docking port.", null, false);
					return;
				}
				int leftHullDiff = hullRects[0].X - leftSubRightSide + 5;
				if (leftHullDiff > 0)
				{
					if (leftHullDiff > 100)
					{
						DebugConsole.NewMessage("Creating hulls between docking ports failed. The leftmost docking port seems to be very far from any hulls in the left-side submarine.", null, false);
						return;
					}
					Rectangle[] array = hullRects;
					int num = 0;
					array[num].X = array[num].X - leftHullDiff;
					Rectangle[] array2 = hullRects;
					int num2 = 0;
					array2[num2].Width = array2[num2].Width + leftHullDiff;
				}
				int rightHullDiff = rightSubLeftSide - hullRects[1].Right + 5;
				if (rightHullDiff > 0)
				{
					if (rightHullDiff > 100)
					{
						DebugConsole.NewMessage("Creating hulls between docking ports failed. The rightmost docking port seems to be very far from any hulls in the right-side submarine.", null, false);
						return;
					}
					Rectangle[] array3 = hullRects;
					int num3 = 1;
					array3[num3].Width = array3[num3].Width + rightHullDiff;
				}
				int expand = 5;
				for (int j = 0; j < 2; j++)
				{
					Rectangle[] array4 = hullRects;
					int num4 = j;
					array4[num4].X = array4[num4].X - expand;
					Rectangle[] array5 = hullRects;
					int num5 = j;
					array5[num5].Width = array5[num5].Width + expand * 2;
					Rectangle[] array6 = hullRects;
					int num6 = j;
					array6[num6].Location = array6[num6].Location - MathUtils.ToPoint(subs[j].WorldPosition - subs[j].HiddenSubPosition);
					this.hulls[j] = new Hull(hullRects[j], subs[j], 0)
					{
						RoomName = (this.IsHorizontal ? "entityname.dockingport" : "entityname.dockinghatch"),
						AvoidStaying = true,
						IsWetRoom = true
					};
					this.hulls[j].AddToGrid(subs[j]);
					this.hulls[j].FreeID();
					for (int k = 0; k < 2; k++)
					{
						this.bodies[j + k * 2] = GameMain.World.CreateEdge(ConvertUnits.ToSimUnits(new Vector2((float)hullRects[j].X, (float)(hullRects[j].Y - hullRects[j].Height * k))), ConvertUnits.ToSimUnits(new Vector2((float)hullRects[j].Right, (float)(hullRects[j].Y - hullRects[j].Height * k))), BodyType.Static, Category.Cat1, Category.All, true);
					}
				}
				if (rightHullDiff <= 100 && this.hulls[0].Submarine != null)
				{
					this.outsideBlocker = this.hulls[0].Submarine.PhysicsBody.FarseerBody.CreateRectangle(ConvertUnits.ToSimUnits(hullRects[0].Width + hullRects[1].Width), ConvertUnits.ToSimUnits(hullRects[0].Height), 0f, ConvertUnits.ToSimUnits(new Vector2((float)hullRects[0].Right, (float)(hullRects[0].Y - hullRects[0].Height / 2)) - this.hulls[0].Submarine.HiddenSubPosition), Category.Cat1, Category.Cat2 | Category.Cat5 | Category.Cat6 | Category.Cat7);
					this.outsideBlocker.UserData = this;
				}
				this.gap = new Gap(new Rectangle(hullRects[0].Right - 2, hullRects[0].Y, 4, hullRects[0].Height), true, subs[0], false, 0);
			}
			else
			{
				if (hullRects[0].Center.Y > hullRects[1].Center.Y)
				{
					hullRects = new Rectangle[]
					{
						this.DockingTarget.item.WorldRect,
						this.item.WorldRect
					};
					subs = new Submarine[]
					{
						this.DockingTarget.item.Submarine,
						this.item.Submarine
					};
				}
				int scaledDockedDistance2 = (int)(this.DockedDistance / 2f * this.item.Scale);
				hullRects[0] = new Rectangle(hullRects[0].X, hullRects[0].Y - hullRects[0].Height / 2 + scaledDockedDistance2, hullRects[0].Width, scaledDockedDistance2);
				hullRects[1] = new Rectangle(hullRects[1].X, hullRects[1].Y - hullRects[1].Height / 2, hullRects[1].Width, scaledDockedDistance2);
				int upperSubBottom = int.MaxValue;
				int lowerSubTop = int.MinValue;
				foreach (Hull hull2 in Hull.HullList)
				{
					for (int l = 0; l < 2; l++)
					{
						if (hull2.Submarine == subs[l] && hull2.WorldRect.Right - 5 >= hullRects[l].X && hull2.WorldRect.X + 5 <= hullRects[l].Right)
						{
							if (l == 0)
							{
								if (hull2.WorldPosition.Y <= (float)(hullRects[l].Y - hullRects[l].Height / 2))
								{
									lowerSubTop = Math.Max(hull2.WorldRect.Y, lowerSubTop);
								}
							}
							else if (hull2.WorldPosition.Y >= (float)(hullRects[l].Y - hullRects[l].Height / 2))
							{
								upperSubBottom = Math.Min(hull2.WorldRect.Y - hull2.WorldRect.Height, upperSubBottom);
							}
						}
					}
				}
				if (upperSubBottom == 2147483647 || lowerSubTop == -2147483648)
				{
					DebugConsole.NewMessage("Creating hulls between docking ports failed. Could not find a hull next to the docking port.", null, false);
					return;
				}
				int lowerHullDiff = hullRects[0].Y - hullRects[0].Height - lowerSubTop + 5;
				if (lowerHullDiff > 0)
				{
					if (lowerHullDiff > 100)
					{
						DebugConsole.NewMessage("Creating hulls between docking ports failed. The lower docking port seems to be very far from any hulls in the lower submarine.", null, false);
						return;
					}
					Rectangle[] array7 = hullRects;
					int num7 = 0;
					array7[num7].Height = array7[num7].Height + lowerHullDiff;
				}
				int upperHullDiff = upperSubBottom - hullRects[1].Y + 5;
				if (upperHullDiff > 0)
				{
					if (upperHullDiff > 100)
					{
						DebugConsole.NewMessage("Creating hulls between docking ports failed. The upper docking port seems to be very far from any hulls in the upper submarine.", null, false);
						return;
					}
					Rectangle[] array8 = hullRects;
					int num8 = 1;
					array8[num8].Y = array8[num8].Y + upperHullDiff;
					Rectangle[] array9 = hullRects;
					int num9 = 1;
					array9[num9].Height = array9[num9].Height + upperHullDiff;
				}
				int midHullDiff = hullRects[1].Y - hullRects[1].Height - hullRects[0].Y + 2;
				if (midHullDiff > 100)
				{
					DebugConsole.NewMessage("Creating hulls between docking ports failed. The upper hull seems to be very far from the lower hull.", null, false);
					return;
				}
				if (midHullDiff > 0)
				{
					Rectangle[] array10 = hullRects;
					int num10 = 0;
					array10[num10].Height = array10[num10].Height + (midHullDiff / 2 + 1);
					Rectangle[] array11 = hullRects;
					int num11 = 1;
					array11[num11].Y = array11[num11].Y - (midHullDiff / 2 + 1);
					Rectangle[] array12 = hullRects;
					int num12 = 1;
					array12[num12].Height = array12[num12].Height + (midHullDiff / 2 + 1);
				}
				int expand2 = 5;
				for (int m = 0; m < 2; m++)
				{
					Rectangle[] array13 = hullRects;
					int num13 = m;
					array13[num13].Y = array13[num13].Y + expand2;
					Rectangle[] array14 = hullRects;
					int num14 = m;
					array14[num14].Height = array14[num14].Height + expand2 * 2;
					Rectangle[] array15 = hullRects;
					int num15 = m;
					array15[num15].Location = array15[num15].Location - MathUtils.ToPoint(subs[m].WorldPosition - subs[m].HiddenSubPosition);
					this.hulls[m] = new Hull(hullRects[m], subs[m], 0)
					{
						RoomName = (this.IsHorizontal ? "entityname.dockingport" : "entityname.dockinghatch"),
						AvoidStaying = true
					};
					this.hulls[m].AddToGrid(subs[m]);
					this.hulls[m].FreeID();
					for (int n = 0; n < 2; n++)
					{
						this.bodies[m + n * 2] = GameMain.World.CreateEdge(ConvertUnits.ToSimUnits(new Vector2((float)(hullRects[m].X + hullRects[m].Width * n), (float)hullRects[m].Y)), ConvertUnits.ToSimUnits(new Vector2((float)(hullRects[m].X + hullRects[m].Width * n), (float)(hullRects[m].Y - hullRects[m].Height))), BodyType.Static, Category.Cat1, Category.All, true);
					}
				}
				if (midHullDiff <= 100 && this.hulls[0].Submarine != null)
				{
					this.outsideBlocker = this.hulls[0].Submarine.PhysicsBody.FarseerBody.CreateRectangle(ConvertUnits.ToSimUnits(hullRects[0].Width), ConvertUnits.ToSimUnits(hullRects[0].Height + hullRects[1].Height), 0f, ConvertUnits.ToSimUnits(new Vector2((float)hullRects[0].Center.X, (float)hullRects[0].Y) - this.hulls[0].Submarine.HiddenSubPosition), Category.Cat1, Category.Cat2 | Category.Cat5 | Category.Cat6 | Category.Cat7);
					this.outsideBlocker.UserData = this;
				}
				this.gap = new Gap(new Rectangle(hullRects[0].X, hullRects[0].Y + 2, hullRects[0].Width, 4), false, subs[0], false, 0);
			}
			this.LinkHullsToGaps();
			Item.UpdateHulls();
			this.hulls[0].ShouldBeSaved = false;
			this.hulls[1].ShouldBeSaved = false;
			this.item.linkedTo.Add(this.hulls[0]);
			this.item.linkedTo.Add(this.hulls[1]);
			this.gap.FreeID();
			this.gap.DisableHullRechecks = true;
			this.gap.ShouldBeSaved = false;
			this.item.linkedTo.Add(this.gap);
			foreach (Body body in this.bodies)
			{
				if (body != null)
				{
					body.BodyType = BodyType.Static;
					body.Friction = 0.5f;
				}
			}
		}

		// Token: 0x06003E70 RID: 15984 RVA: 0x00191CB4 File Offset: 0x0018FEB4
		private void LinkHullsToGaps()
		{
			if (this.gap == null || this.hulls == null || this.hulls[0] == null || this.hulls[1] == null)
			{
				return;
			}
			this.gap.linkedTo.Clear();
			if (this.IsHorizontal)
			{
				if (this.hulls[0].WorldRect.X > this.hulls[1].WorldRect.X)
				{
					Hull temp = this.hulls[0];
					this.hulls[0] = this.hulls[1];
					this.hulls[1] = temp;
				}
				this.gap.linkedTo.Add(this.hulls[0]);
				this.gap.linkedTo.Add(this.hulls[1]);
			}
			else
			{
				if (this.hulls[0].WorldRect.Y < this.hulls[1].WorldRect.Y)
				{
					Hull temp2 = this.hulls[0];
					this.hulls[0] = this.hulls[1];
					this.hulls[1] = temp2;
				}
				this.gap.linkedTo.Add(this.hulls[0]);
				this.gap.linkedTo.Add(this.hulls[1]);
			}
			for (int i = 0; i < 2; i++)
			{
				Gap gap;
				if (i != 0)
				{
					DockingPort dockingTarget = this.DockingTarget;
					if (dockingTarget == null)
					{
						gap = null;
					}
					else
					{
						Door door = dockingTarget.Door;
						gap = ((door != null) ? door.LinkedGap : null);
					}
				}
				else
				{
					Door door2 = this.Door;
					gap = ((door2 != null) ? door2.LinkedGap : null);
				}
				Gap doorGap = gap;
				if (doorGap != null)
				{
					doorGap.DisableHullRechecks = true;
					if (doorGap.linkedTo.Count < 2)
					{
						if (this.IsHorizontal)
						{
							if (doorGap.WorldPosition.X < this.gap.WorldPosition.X)
							{
								if (!doorGap.linkedTo.Contains(this.hulls[0]))
								{
									doorGap.linkedTo.Add(this.hulls[0]);
								}
							}
							else if (!doorGap.linkedTo.Contains(this.hulls[1]))
							{
								doorGap.linkedTo.Add(this.hulls[1]);
							}
							if (doorGap.linkedTo.Count > 1 && doorGap.linkedTo[0].WorldRect.X > doorGap.linkedTo[1].WorldRect.X)
							{
								MapEntity temp3 = doorGap.linkedTo[0];
								doorGap.linkedTo[0] = doorGap.linkedTo[1];
								doorGap.linkedTo[1] = temp3;
							}
						}
						else
						{
							if (doorGap.WorldPosition.Y > this.gap.WorldPosition.Y)
							{
								if (!doorGap.linkedTo.Contains(this.hulls[0]))
								{
									doorGap.linkedTo.Add(this.hulls[0]);
								}
							}
							else if (!doorGap.linkedTo.Contains(this.hulls[1]))
							{
								doorGap.linkedTo.Add(this.hulls[1]);
							}
							if (doorGap.linkedTo.Count > 1 && doorGap.linkedTo[0].WorldRect.Y < doorGap.linkedTo[1].WorldRect.Y)
							{
								MapEntity temp4 = doorGap.linkedTo[0];
								doorGap.linkedTo[0] = doorGap.linkedTo[1];
								doorGap.linkedTo[1] = temp4;
							}
						}
					}
				}
			}
		}

		// Token: 0x06003E71 RID: 15985 RVA: 0x0019202C File Offset: 0x0019022C
		public void Undock(bool applyEffects = true)
		{
			if (this.DockingTarget == null || !this.docked)
			{
				return;
			}
			this.forceLockTimer = 0f;
			this.dockingCooldown = 0.1f;
			if (applyEffects)
			{
				base.ApplyStatusEffects(ActionType.OnSecondaryUse, 1f, null, null, null, null, null, 1f);
			}
			this.DockingTarget.item.Submarine.ConnectedDockingPorts.Remove(this.item.Submarine);
			this.DockingTarget.item.Submarine.RefreshConnectedSubs();
			this.item.Submarine.ConnectedDockingPorts.Remove(this.DockingTarget.item.Submarine);
			this.item.Submarine.RefreshConnectedSubs();
			if (this.Door != null && this.DockingTarget.Door != null)
			{
				WayPoint myWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => this.Door.LinkedGap == wp.ConnectedGap);
				WayPoint targetWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => this.DockingTarget.Door.LinkedGap == wp.ConnectedGap);
				if (myWayPoint != null && targetWayPoint != null)
				{
					myWayPoint.FindHull();
					if (myWayPoint.linkedTo.Contains(targetWayPoint))
					{
						myWayPoint.linkedTo.Remove(targetWayPoint);
						Action<WayPoint> onLinksChanged = myWayPoint.OnLinksChanged;
						if (onLinksChanged != null)
						{
							onLinksChanged(myWayPoint);
						}
					}
					targetWayPoint.FindHull();
					if (targetWayPoint.linkedTo.Contains(myWayPoint))
					{
						targetWayPoint.linkedTo.Remove(myWayPoint);
						Action<WayPoint> onLinksChanged2 = targetWayPoint.OnLinksChanged;
						if (onLinksChanged2 != null)
						{
							onLinksChanged2(targetWayPoint);
						}
					}
				}
			}
			this.item.linkedTo.Clear();
			this.docked = false;
			this.item.SendSignal("1", "on_undock");
			base.Item.Submarine.RefreshOutdoorNodes();
			base.Item.Submarine.EnableObstructedWaypoints(this.DockingTarget.Item.Submarine);
			this.obstructedWayPointsDisabled = false;
			this.WasDocked = false;
			this.DockingTarget.WasDocked = false;
			this.DockingTarget.Undock(true);
			this.DockingTarget = null;
			Connection powerConnection = base.Item.Connections.Find((Connection c) => c.IsPower);
			if (powerConnection != null)
			{
				Powered.ChangedConnections.Add(powerConnection);
			}
			if (this.doorBody != null)
			{
				GameMain.World.Remove(this.doorBody);
				this.doorBody = null;
			}
			Wire wire = this.item.GetComponent<Wire>();
			if (wire != null)
			{
				wire.Drop(null, true);
			}
			if (this.joint != null)
			{
				GameMain.World.Remove(this.joint);
				this.joint = null;
			}
			Hull hull = this.hulls[0];
			if (hull != null)
			{
				hull.Remove();
			}
			this.hulls[0] = null;
			Hull hull2 = this.hulls[1];
			if (hull2 != null)
			{
				hull2.Remove();
			}
			this.hulls[1] = null;
			if (this.gap != null)
			{
				this.gap.Remove();
				this.gap = null;
			}
			if (this.bodies != null)
			{
				foreach (Body body in this.bodies)
				{
					if (body != null)
					{
						GameMain.World.Remove(body);
					}
				}
				this.bodies = null;
			}
			Fixture fixture = this.outsideBlocker;
			if (fixture != null)
			{
				fixture.Body.Remove(this.outsideBlocker);
			}
			this.outsideBlocker = null;
			if (GameMain.Server != null)
			{
				Submarine submarine = this.item.Submarine;
				if (submarine == null || !submarine.Loading)
				{
					this.item.CreateServerEvent<DockingPort>(this);
				}
			}
			Action onUnDocked = this.OnUnDocked;
			if (onUnDocked != null)
			{
				onUnDocked();
			}
			this.OnUnDocked = null;
		}

		// Token: 0x06003E72 RID: 15986 RVA: 0x001923CC File Offset: 0x001905CC
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.docked && this.WasDocked)
			{
				this.item.SendSignal("1", "on_undock");
				this.WasDocked = false;
			}
			this.dockingCooldown -= deltaTime;
			if (this.DockingTarget == null)
			{
				this.dockingState = MathHelper.Lerp(this.dockingState, 0f, deltaTime * 10f);
				if (this.dockingState < 0.01f)
				{
					this.docked = false;
				}
				this.item.SendSignal("0", "state_out");
				this.item.SendSignal(this.AnotherPortInProximity ? "1" : "0", "proximity_sensor");
			}
			else
			{
				if (!this.docked)
				{
					this.Dock(this.DockingTarget);
					if (this.DockingTarget == null)
					{
						return;
					}
				}
				if (this.joint is DistanceJoint)
				{
					this.dockingState = MathHelper.Lerp(this.dockingState, 0.5f, deltaTime * 10f);
					this.forceLockTimer += deltaTime;
					Vector2 jointDiff = this.joint.WorldAnchorB - this.joint.WorldAnchorA;
					if (jointDiff.LengthSquared() > 0.0016f && this.forceLockTimer < 1f)
					{
						float totalMass = this.item.Submarine.PhysicsBody.Mass + this.DockingTarget.item.Submarine.PhysicsBody.Mass;
						float massRatio;
						float massRatio2;
						if (this.item.Submarine.PhysicsBody.BodyType != BodyType.Dynamic)
						{
							massRatio = 0f;
							massRatio2 = 1f;
						}
						else if (this.DockingTarget.item.Submarine.PhysicsBody.BodyType != BodyType.Dynamic)
						{
							massRatio = 1f;
							massRatio2 = 0f;
						}
						else
						{
							massRatio = this.DockingTarget.item.Submarine.PhysicsBody.Mass / totalMass;
							massRatio2 = this.item.Submarine.PhysicsBody.Mass / totalMass;
						}
						Vector2 relativeVelocity = this.DockingTarget.item.Submarine.Velocity - this.item.Submarine.Velocity;
						Vector2 desiredRelativeVelocity = Vector2.Normalize(jointDiff);
						this.item.Submarine.Velocity += (relativeVelocity + desiredRelativeVelocity) * massRatio;
						this.DockingTarget.item.Submarine.Velocity += (-relativeVelocity - desiredRelativeVelocity) * massRatio2;
					}
					else
					{
						this.Lock(false, true, true);
					}
				}
				else
				{
					if (this.DockingTarget.Door != null && this.doorBody != null)
					{
						Body body = this.doorBody;
						bool enabled;
						if (this.DockingTarget.Door.Body.Enabled)
						{
							Fixture fixture = this.DockingTarget.Door.Body.FarseerBody.FixtureList.FirstOrDefault<Fixture>();
							enabled = (fixture == null || !fixture.IsSensor);
						}
						else
						{
							enabled = false;
						}
						body.Enabled = enabled;
					}
					this.dockingState = MathHelper.Lerp(this.dockingState, 1f, deltaTime * 10f);
				}
				this.item.SendSignal(this.IsLocked ? "1" : "0", "state_out");
			}
			if (!this.obstructedWayPointsDisabled && this.dockingState >= 0.99f)
			{
				base.Item.Submarine.RefreshOutdoorNodes();
				Submarine submarine = base.Item.Submarine;
				DockingPort dockingTarget = this.DockingTarget;
				submarine.DisableObstructedWayPoints((dockingTarget != null) ? dockingTarget.Item.Submarine : null);
				this.obstructedWayPointsDisabled = true;
			}
		}

		// Token: 0x06003E73 RID: 15987 RVA: 0x00192778 File Offset: 0x00190978
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			DockingPort.list.Remove(this);
			Hull hull = this.hulls[0];
			if (hull != null)
			{
				hull.Remove();
			}
			this.hulls[0] = null;
			Hull hull2 = this.hulls[1];
			if (hull2 != null)
			{
				hull2.Remove();
			}
			this.hulls[1] = null;
			Gap gap = this.gap;
			if (gap != null)
			{
				gap.Remove();
			}
			this.gap = null;
			Sprite sprite = this.overlaySprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.overlaySprite = null;
		}

		// Token: 0x06003E74 RID: 15988 RVA: 0x00192800 File Offset: 0x00190A00
		private void InitializeLinks()
		{
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			float maxXDist = this.item.Prefab.Sprite.size.X * this.item.Prefab.Scale / 2f;
			float closestYDist = this.item.Prefab.Sprite.size.Y * this.item.Prefab.Scale / 2f;
			foreach (Item it in Item.ItemList)
			{
				if (it.Submarine == this.item.Submarine)
				{
					Door doorComponent = it.GetComponent<Door>();
					if (doorComponent != null && doorComponent.IsHorizontal != this.IsHorizontal)
					{
						float yDist = Math.Abs(it.Position.Y - this.item.Position.Y);
						if (this.item.linkedTo.Contains(it))
						{
							yDist = Math.Min(closestYDist, yDist);
						}
						else if (Math.Abs(it.Position.X - this.item.Position.X) > maxXDist)
						{
							continue;
						}
						if (yDist <= closestYDist)
						{
							this.Door = doorComponent;
							closestYDist = yDist;
						}
					}
				}
			}
			if (!this.item.linkedTo.Any<MapEntity>())
			{
				return;
			}
			List<MapEntity> linked = new List<MapEntity>(this.item.linkedTo);
			foreach (MapEntity entity in linked)
			{
				Hull hull = entity as Hull;
				if (hull != null)
				{
					hull.Remove();
					this.item.linkedTo.Remove(hull);
				}
				else
				{
					Gap gap = entity as Gap;
					if (gap != null)
					{
						gap.Remove();
					}
				}
			}
		}

		// Token: 0x06003E75 RID: 15989 RVA: 0x00192A10 File Offset: 0x00190C10
		public override void OnMapLoaded()
		{
			this.InitializeLinks();
			Wire wire = this.item.GetComponent<Wire>();
			if (wire != null)
			{
				wire.Locked = true;
				wire.Hidden = true;
				if (wire.Connections.Contains(null))
				{
					wire.Drop(null, true);
				}
			}
			if (this.item.linkedTo.Any<MapEntity>())
			{
				List<MapEntity> linked = new List<MapEntity>(this.item.linkedTo);
				foreach (MapEntity entity in linked)
				{
					Item linkedItem = entity as Item;
					if (linkedItem != null)
					{
						DockingPort dockingPort = linkedItem.GetComponent<DockingPort>();
						if (dockingPort != null)
						{
							this.Dock(dockingPort);
						}
					}
				}
			}
		}

		// Token: 0x06003E76 RID: 15990 RVA: 0x00192AD4 File Offset: 0x00190CD4
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.dockingCooldown > 0f)
			{
				return;
			}
			bool wasDocked = this.docked;
			DockingPort prevDockingTarget = this.DockingTarget;
			bool newDockedState = wasDocked;
			string name = connection.Name;
			if (!(name == "toggle"))
			{
				if (name == "set_active" || name == "set_state")
				{
					newDockedState = (signal.value != "0");
				}
			}
			else if (signal.value != "0")
			{
				newDockedState = !this.docked;
			}
			if (newDockedState != wasDocked)
			{
				DockingPort targetPort = this.docked ? this.DockingTarget : this.FindAdjacentPort();
				Submarine submarine = this.item.Submarine;
				bool flag;
				if (submarine != null)
				{
					SubmarineInfo info = submarine.Info;
					if (info != null && !info.IsOutpost)
					{
						Submarine submarine2 = this.item.Submarine;
						submarine = ((submarine2 != null) ? submarine2.Submarine : null);
						if (submarine != null)
						{
							info = submarine.Info;
							if (info != null && info.IsOutpost)
							{
								goto IL_130;
							}
						}
						if (targetPort != null)
						{
							Item item = targetPort.item;
							if (item != null)
							{
								submarine = item.Submarine;
								if (submarine != null)
								{
									info = submarine.Info;
									if (info != null)
									{
										flag = info.IsOutpost;
										goto IL_131;
									}
								}
							}
						}
						flag = false;
						goto IL_131;
					}
				}
				IL_130:
				flag = false;
				IL_131:
				bool tryingToToggleOutpostDocking = flag;
				if (GameMain.NetworkMember != null && tryingToToggleOutpostDocking && signal.sender == null)
				{
					if (this.allowOutpostAutoDocking == DockingPort.AllowOutpostAutoDocking.Ask)
					{
						this.outpostAutoDockingPromptShown = true;
						return;
					}
					if (this.allowOutpostAutoDocking == DockingPort.AllowOutpostAutoDocking.No)
					{
						return;
					}
				}
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				this.Docked = newDockedState;
			}
			if (signal.sender != null && this.docked != wasDocked)
			{
				if (this.docked)
				{
					if (this.item.Submarine != null)
					{
						DockingPort dockingTarget = this.DockingTarget;
						bool flag2;
						if (dockingTarget == null)
						{
							flag2 = (null != null);
						}
						else
						{
							Item item2 = dockingTarget.item;
							flag2 = (((item2 != null) ? item2.Submarine : null) != null);
						}
						if (flag2)
						{
							GameServer.Log(string.Concat(new string[]
							{
								GameServer.CharacterLogName(signal.sender),
								" docked ",
								this.item.Submarine.Info.Name,
								" to ",
								this.DockingTarget.item.Submarine.Info.Name
							}), ServerLog.MessageType.ItemInteraction);
							return;
						}
					}
				}
				else if (this.item.Submarine != null)
				{
					bool flag3;
					if (prevDockingTarget == null)
					{
						flag3 = (null != null);
					}
					else
					{
						Item item3 = prevDockingTarget.item;
						flag3 = (((item3 != null) ? item3.Submarine : null) != null);
					}
					if (flag3)
					{
						GameServer.Log(string.Concat(new string[]
						{
							GameServer.CharacterLogName(signal.sender),
							" undocked ",
							this.item.Submarine.Info.Name,
							" from ",
							prevDockingTarget.item.Submarine.Info.Name
						}), ServerLog.MessageType.ItemInteraction);
					}
				}
			}
		}

		// Token: 0x04001DC4 RID: 7620
		private static readonly List<DockingPort> list = new List<DockingPort>();

		// Token: 0x04001DC5 RID: 7621
		private Sprite overlaySprite;

		// Token: 0x04001DC6 RID: 7622
		private float dockingState;

		// Token: 0x04001DC7 RID: 7623
		private Joint joint;

		// Token: 0x04001DC8 RID: 7624
		private readonly Hull[] hulls = new Hull[2];

		// Token: 0x04001DC9 RID: 7625
		private Gap gap;

		// Token: 0x04001DCA RID: 7626
		private Body[] bodies;

		// Token: 0x04001DCB RID: 7627
		private Fixture outsideBlocker;

		// Token: 0x04001DCC RID: 7628
		private Body doorBody;

		// Token: 0x04001DCD RID: 7629
		private float dockingCooldown;

		// Token: 0x04001DCE RID: 7630
		private bool docked;

		// Token: 0x04001DCF RID: 7631
		private bool obstructedWayPointsDisabled;

		// Token: 0x04001DD0 RID: 7632
		private float forceLockTimer;

		// Token: 0x04001DD1 RID: 7633
		private const float ForceLockDelay = 1f;

		// Token: 0x04001DDE RID: 7646
		private bool outpostAutoDockingPromptShown;

		// Token: 0x04001DDF RID: 7647
		private DockingPort.AllowOutpostAutoDocking allowOutpostAutoDocking;

		// Token: 0x04001DE0 RID: 7648
		private bool initialized;

		// Token: 0x02000D73 RID: 3443
		public enum DirectionType
		{
			// Token: 0x04003FC3 RID: 16323
			None,
			// Token: 0x04003FC4 RID: 16324
			Top,
			// Token: 0x04003FC5 RID: 16325
			Bottom,
			// Token: 0x04003FC6 RID: 16326
			Left,
			// Token: 0x04003FC7 RID: 16327
			Right
		}

		// Token: 0x02000D74 RID: 3444
		private enum AllowOutpostAutoDocking
		{
			// Token: 0x04003FC9 RID: 16329
			Ask,
			// Token: 0x04003FCA RID: 16330
			Yes,
			// Token: 0x04003FCB RID: 16331
			No
		}
	}
}
