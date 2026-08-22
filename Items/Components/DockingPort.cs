using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.IO;
using Barotrauma.Lights;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E5 RID: 1509
	internal class DockingPort : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001906 RID: 6406
		// (get) Token: 0x060062EF RID: 25327 RVA: 0x00337568 File Offset: 0x00335768
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x060062F0 RID: 25328 RVA: 0x00337570 File Offset: 0x00335770
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.dockingState == 0f)
			{
				return;
			}
			if (this.overlaySprite != null)
			{
				Vector2 drawPos = this.item.DrawPosition;
				drawPos.Y = -drawPos.Y;
				Rectangle rect = this.overlaySprite.SourceRect;
				if (this.IsHorizontal)
				{
					drawPos.Y -= (float)(rect.Height / 2);
					if (this.DockingDir == 1)
					{
						spriteBatch.Draw(this.overlaySprite.Texture, drawPos, new Rectangle?(new Rectangle(rect.Center.X + (int)((float)(rect.Width / 2) * (1f - this.dockingState)), rect.Y, (int)((float)(rect.Width / 2) * this.dockingState), rect.Height)), overrideColor ?? Color.White);
					}
					else
					{
						spriteBatch.Draw(this.overlaySprite.Texture, drawPos - Vector2.UnitX * ((float)(rect.Width / 2) * this.dockingState), new Rectangle?(new Rectangle(rect.X, rect.Y, (int)((float)(rect.Width / 2) * this.dockingState), rect.Height)), overrideColor ?? Color.White);
					}
				}
				else
				{
					drawPos.X -= (float)(rect.Width / 2);
					if (this.DockingDir == 1)
					{
						spriteBatch.Draw(this.overlaySprite.Texture, drawPos - Vector2.UnitY * ((float)(rect.Height / 2) * this.dockingState), new Rectangle?(new Rectangle(rect.X, rect.Y, rect.Width, (int)((float)(rect.Height / 2) * this.dockingState))), overrideColor ?? Color.White);
					}
					else
					{
						spriteBatch.Draw(this.overlaySprite.Texture, drawPos, new Rectangle?(new Rectangle(rect.X, rect.Y + rect.Height / 2 + (int)((float)(rect.Height / 2) * (1f - this.dockingState)), rect.Width, (int)((float)(rect.Height / 2) * this.dockingState))), overrideColor ?? Color.White);
					}
				}
			}
			if (!GameMain.DebugDraw)
			{
				return;
			}
			if (this.bodies != null)
			{
				for (int i = 0; i < this.bodies.Length; i++)
				{
					Body body = this.bodies[i];
					if (body != null)
					{
						AABB aabb;
						body.FixtureList[0].GetAABB(out aabb, 0);
						Vector2 bodyDrawPos = ConvertUnits.ToDisplayUnits(new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y));
						if (i == 1 || i == 3)
						{
							DockingPort dockingTarget = this.DockingTarget;
							bool flag;
							if (dockingTarget == null)
							{
								flag = (null != null);
							}
							else
							{
								Item item = dockingTarget.item;
								flag = (((item != null) ? item.Submarine : null) != null);
							}
							if (flag)
							{
								bodyDrawPos += this.DockingTarget.item.Submarine.Position;
							}
						}
						if ((i == 0 || i == 2) && this.item.Submarine != null)
						{
							bodyDrawPos += this.item.Submarine.Position;
						}
						bodyDrawPos.Y = -bodyDrawPos.Y;
						GUI.DrawRectangle(spriteBatch, bodyDrawPos, ConvertUnits.ToDisplayUnits(aabb.Extents * 2f), Color.Gray, false, 0f, 4f);
					}
				}
			}
			if (this.doorBody != null && this.doorBody.Enabled)
			{
				AABB aabb2;
				this.doorBody.FixtureList[0].GetAABB(out aabb2, 0);
				Vector2 bodyDrawPos2 = ConvertUnits.ToDisplayUnits(new Vector2(aabb2.LowerBound.X, aabb2.UpperBound.Y));
				Item item2 = this.item;
				if (((item2 != null) ? item2.Submarine : null) != null)
				{
					bodyDrawPos2 += this.item.Submarine.Position;
				}
				bodyDrawPos2.Y = -bodyDrawPos2.Y;
				GUI.DrawRectangle(spriteBatch, bodyDrawPos2, ConvertUnits.ToDisplayUnits(aabb2.Extents * 2f), Color.Gray, false, 0f, 8f);
			}
		}

		// Token: 0x060062F1 RID: 25329 RVA: 0x003379D8 File Offset: 0x00335BD8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			bool isDocked = msg.ReadBoolean();
			for (int i = 0; i < 2; i++)
			{
				if (this.hulls[i] != null)
				{
					this.item.linkedTo.Remove(this.hulls[i]);
					this.hulls[i].Remove();
					this.hulls[i] = null;
				}
			}
			if (this.gap != null)
			{
				this.item.linkedTo.Remove(this.gap);
				this.gap.Remove();
				this.gap = null;
			}
			if (isDocked)
			{
				ushort dockingTargetID = msg.ReadUInt16();
				bool isLocked = msg.ReadBoolean();
				Entity targetEntity = Entity.FindEntityByID(dockingTargetID);
				if (targetEntity == null || !(targetEntity is Item))
				{
					DebugConsole.ThrowError("Invalid docking port network event (can't dock to " + (((targetEntity != null) ? targetEntity.ToString() : null) ?? "null") + ")", null, null, false, false);
					return;
				}
				this.DockingTarget = (targetEntity as Item).GetComponent<DockingPort>();
				if (this.DockingTarget == null)
				{
					string str = "Invalid docking port network event (";
					Entity entity = targetEntity;
					DebugConsole.ThrowError(str + ((entity != null) ? entity.ToString() : null) + " doesn't have a docking port component)", null, null, false, false);
					return;
				}
				this.Dock(this.DockingTarget);
				if (this.joint == null)
				{
					string str2 = "Error while reading a docking port network event (Dock method did not create a joint between the ports). Submarine: ";
					Submarine submarine = this.item.Submarine;
					string str3 = ((submarine != null) ? submarine.Info.Name : null) ?? "null";
					string str4 = ", target submarine: ";
					Submarine submarine2 = this.DockingTarget.item.Submarine;
					string errorMsg = str2 + str3 + str4 + (((submarine2 != null) ? submarine2.Info.Name : null) ?? "null");
					Submarine submarine3 = this.item.Submarine;
					if (submarine3 != null && submarine3.ConnectedDockingPorts.ContainsKey(this.DockingTarget.item.Submarine))
					{
						errorMsg += "\nAlready docked.";
					}
					if (this.item.Submarine == this.DockingTarget.item.Submarine)
					{
						errorMsg += "\nTrying to dock the submarine to itself.";
					}
					GameAnalyticsManager.AddErrorEventOnce("DockingPort.ClientRead:JointNotCreated", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				}
				if (isLocked)
				{
					if (this.DockingTarget.joint != null)
					{
						this.DockingTarget.Lock(true, true, true);
						return;
					}
					this.Lock(true, true, true);
					return;
				}
			}
			else
			{
				this.Undock(true);
			}
		}

		// Token: 0x060062F2 RID: 25330 RVA: 0x00337C17 File Offset: 0x00335E17
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)this.allowOutpostAutoDocking);
		}

		// Token: 0x17001907 RID: 6407
		// (get) Token: 0x060062F3 RID: 25331 RVA: 0x00337C26 File Offset: 0x00335E26
		public static IEnumerable<DockingPort> List
		{
			get
			{
				return DockingPort.list;
			}
		}

		// Token: 0x17001908 RID: 6408
		// (get) Token: 0x060062F4 RID: 25332 RVA: 0x00337C2D File Offset: 0x00335E2D
		// (set) Token: 0x060062F5 RID: 25333 RVA: 0x00337C35 File Offset: 0x00335E35
		public int DockingDir { get; set; }

		// Token: 0x17001909 RID: 6409
		// (get) Token: 0x060062F6 RID: 25334 RVA: 0x00337C3E File Offset: 0x00335E3E
		// (set) Token: 0x060062F7 RID: 25335 RVA: 0x00337C46 File Offset: 0x00335E46
		[Serialize("32.0,32.0", IsPropertySaveable.No, "How close the docking port has to be to another port to dock.", "", false)]
		public Vector2 DistanceTolerance { get; set; }

		// Token: 0x1700190A RID: 6410
		// (get) Token: 0x060062F8 RID: 25336 RVA: 0x00337C4F File Offset: 0x00335E4F
		// (set) Token: 0x060062F9 RID: 25337 RVA: 0x00337C57 File Offset: 0x00335E57
		[Serialize(32f, IsPropertySaveable.No, "How close together the docking ports are forced when docked.", "", false)]
		public float DockedDistance { get; set; }

		// Token: 0x1700190B RID: 6411
		// (get) Token: 0x060062FA RID: 25338 RVA: 0x00337C60 File Offset: 0x00335E60
		// (set) Token: 0x060062FB RID: 25339 RVA: 0x00337C68 File Offset: 0x00335E68
		[Serialize(true, IsPropertySaveable.No, "Is the port horizontal.", "", false)]
		public bool IsHorizontal { get; set; }

		// Token: 0x1700190C RID: 6412
		// (get) Token: 0x060062FC RID: 25340 RVA: 0x00337C71 File Offset: 0x00335E71
		// (set) Token: 0x060062FD RID: 25341 RVA: 0x00337C79 File Offset: 0x00335E79
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, this docking port is used when spawning the submarine docked to an outpost (if possible).", "", false)]
		public bool MainDockingPort { get; set; }

		// Token: 0x1700190D RID: 6413
		// (get) Token: 0x060062FE RID: 25342 RVA: 0x00337C82 File Offset: 0x00335E82
		// (set) Token: 0x060062FF RID: 25343 RVA: 0x00337C8A File Offset: 0x00335E8A
		[Editable]
		[Serialize(true, IsPropertySaveable.No, "Should the OnUse StatusEffects trigger when docking (on vanilla docking ports these effects emit particles and play a sound).)", "", false)]
		public bool ApplyEffectsOnDocking { get; set; }

		// Token: 0x1700190E RID: 6414
		// (get) Token: 0x06006300 RID: 25344 RVA: 0x00337C93 File Offset: 0x00335E93
		// (set) Token: 0x06006301 RID: 25345 RVA: 0x00337C9B File Offset: 0x00335E9B
		[Editable]
		[Serialize(DockingPort.DirectionType.None, IsPropertySaveable.No, "Which direction the port is allowed to dock in. For example, \"Top\" would mean the port can dock to another port above it.\nNormally there's no need to touch this setting, but if you notice the docking position is incorrect (for example due to some unusual docking port configuration without hulls or doors), you can use this to enforce the direction.", "", false)]
		public DockingPort.DirectionType ForceDockingDirection { get; set; }

		// Token: 0x1700190F RID: 6415
		// (get) Token: 0x06006302 RID: 25346 RVA: 0x00337CA4 File Offset: 0x00335EA4
		// (set) Token: 0x06006303 RID: 25347 RVA: 0x00337CAC File Offset: 0x00335EAC
		[Serialize(false, IsPropertySaveable.Yes, "Was the docking port docked at the end of the previous round.", "", false)]
		public bool WasDocked { get; set; }

		// Token: 0x17001910 RID: 6416
		// (get) Token: 0x06006304 RID: 25348 RVA: 0x00337CB5 File Offset: 0x00335EB5
		// (set) Token: 0x06006305 RID: 25349 RVA: 0x00337CBD File Offset: 0x00335EBD
		public DockingPort DockingTarget { get; private set; }

		// Token: 0x17001911 RID: 6417
		// (get) Token: 0x06006306 RID: 25350 RVA: 0x00337CC8 File Offset: 0x00335EC8
		public bool AtStartExit
		{
			get
			{
				Submarine submarine = base.Item.Submarine;
				return submarine != null && submarine.AtStartExit;
			}
		}

		// Token: 0x17001912 RID: 6418
		// (get) Token: 0x06006307 RID: 25351 RVA: 0x00337CEC File Offset: 0x00335EEC
		public bool AtEndExit
		{
			get
			{
				Submarine submarine = base.Item.Submarine;
				return submarine != null && submarine.AtEndExit;
			}
		}

		// Token: 0x17001913 RID: 6419
		// (get) Token: 0x06006308 RID: 25352 RVA: 0x00337D10 File Offset: 0x00335F10
		// (set) Token: 0x06006309 RID: 25353 RVA: 0x00337D18 File Offset: 0x00335F18
		public Door Door { get; private set; }

		// Token: 0x17001914 RID: 6420
		// (get) Token: 0x0600630A RID: 25354 RVA: 0x00337D21 File Offset: 0x00335F21
		// (set) Token: 0x0600630B RID: 25355 RVA: 0x00337D29 File Offset: 0x00335F29
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

		// Token: 0x17001915 RID: 6421
		// (get) Token: 0x0600630C RID: 25356 RVA: 0x00337D69 File Offset: 0x00335F69
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

		// Token: 0x17001916 RID: 6422
		// (get) Token: 0x0600630D RID: 25357 RVA: 0x00337D94 File Offset: 0x00335F94
		public bool AnotherPortInProximity
		{
			get
			{
				return this.FindAdjacentPort() != null;
			}
		}

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x0600630E RID: 25358 RVA: 0x00337DA0 File Offset: 0x00335FA0
		// (remove) Token: 0x0600630F RID: 25359 RVA: 0x00337DD8 File Offset: 0x00335FD8
		public event Action OnDocked;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06006310 RID: 25360 RVA: 0x00337E10 File Offset: 0x00336010
		// (remove) Token: 0x06006311 RID: 25361 RVA: 0x00337E48 File Offset: 0x00336048
		public event Action OnUnDocked;

		// Token: 0x06006312 RID: 25362 RVA: 0x00337E80 File Offset: 0x00336080
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

		// Token: 0x06006313 RID: 25363 RVA: 0x00337F64 File Offset: 0x00336164
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

		// Token: 0x06006314 RID: 25364 RVA: 0x00337FCA File Offset: 0x003361CA
		public override void FlipY(bool relativeToSub)
		{
			this.FlipX(relativeToSub);
		}

		// Token: 0x06006315 RID: 25365 RVA: 0x00337FD4 File Offset: 0x003361D4
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

		// Token: 0x06006316 RID: 25366 RVA: 0x003380F8 File Offset: 0x003362F8
		private void AttemptDock()
		{
			DockingPort adjacentPort = this.FindAdjacentPort();
			if (adjacentPort != null)
			{
				this.Dock(adjacentPort);
			}
		}

		// Token: 0x06006317 RID: 25367 RVA: 0x00338118 File Offset: 0x00336318
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
			Action onDocked = this.OnDocked;
			if (onDocked != null)
			{
				onDocked();
			}
			this.OnDocked = null;
			this.WasDocked = true;
			this.DockingTarget.Docked = true;
		}

		// Token: 0x06006318 RID: 25368 RVA: 0x00338394 File Offset: 0x00336594
		public void Lock(bool isNetworkMessage, bool applyEffects = true, bool moveSubs = true)
		{
			if (GameMain.Client != null && !isNetworkMessage)
			{
				return;
			}
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
				if (GameMain.Client != null && GameMain.Client.MidRoundSyncing && Submarine.MainSub != null && (this.item.Submarine == Submarine.MainSub || this.DockingTarget.item.Submarine == Submarine.MainSub))
				{
					Screen.Selected.Cam.Position = Submarine.MainSub.WorldPosition;
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

		// Token: 0x06006319 RID: 25369 RVA: 0x003387B4 File Offset: 0x003369B4
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

		// Token: 0x0600631A RID: 25370 RVA: 0x00338944 File Offset: 0x00336B44
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

		// Token: 0x0600631B RID: 25371 RVA: 0x00338C4C File Offset: 0x00336E4C
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

		// Token: 0x0600631C RID: 25372 RVA: 0x00338C84 File Offset: 0x00336E84
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

		// Token: 0x0600631D RID: 25373 RVA: 0x00338DA0 File Offset: 0x00336FA0
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

		// Token: 0x0600631E RID: 25374 RVA: 0x00338F40 File Offset: 0x00337140
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
			this.RemoveConvexHulls();
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
				for (int l = 0; l < 2; l++)
				{
					this.convexHulls[l] = new ConvexHull(new Rectangle(new Point((int)this.item.Position.X, this.item.Rect.Y - this.item.Rect.Height * l), new Point((int)(this.DockingTarget.item.WorldPosition.X - this.item.WorldPosition.X), 0)), this.IsHorizontal, this.item);
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
					for (int m = 0; m < 2; m++)
					{
						if (hull2.Submarine == subs[m] && hull2.WorldRect.Right - 5 >= hullRects[m].X && hull2.WorldRect.X + 5 <= hullRects[m].Right)
						{
							if (m == 0)
							{
								if (hull2.WorldPosition.Y <= (float)(hullRects[m].Y - hullRects[m].Height / 2))
								{
									lowerSubTop = Math.Max(hull2.WorldRect.Y, lowerSubTop);
								}
							}
							else if (hull2.WorldPosition.Y >= (float)(hullRects[m].Y - hullRects[m].Height / 2))
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
				for (int n = 0; n < 2; n++)
				{
					Rectangle[] array13 = hullRects;
					int num13 = n;
					array13[num13].Y = array13[num13].Y + expand2;
					Rectangle[] array14 = hullRects;
					int num14 = n;
					array14[num14].Height = array14[num14].Height + expand2 * 2;
					Rectangle[] array15 = hullRects;
					int num15 = n;
					array15[num15].Location = array15[num15].Location - MathUtils.ToPoint(subs[n].WorldPosition - subs[n].HiddenSubPosition);
					this.hulls[n] = new Hull(hullRects[n], subs[n], 0)
					{
						RoomName = (this.IsHorizontal ? "entityname.dockingport" : "entityname.dockinghatch"),
						AvoidStaying = true
					};
					this.hulls[n].AddToGrid(subs[n]);
					this.hulls[n].FreeID();
					for (int j2 = 0; j2 < 2; j2++)
					{
						this.bodies[n + j2 * 2] = GameMain.World.CreateEdge(ConvertUnits.ToSimUnits(new Vector2((float)(hullRects[n].X + hullRects[n].Width * j2), (float)hullRects[n].Y)), ConvertUnits.ToSimUnits(new Vector2((float)(hullRects[n].X + hullRects[n].Width * j2), (float)(hullRects[n].Y - hullRects[n].Height))), BodyType.Static, Category.Cat1, Category.All, true);
					}
				}
				for (int i2 = 0; i2 < 2; i2++)
				{
					this.convexHulls[i2] = new ConvexHull(new Rectangle(new Point(this.item.Rect.X + this.item.Rect.Width * i2, (int)this.item.Position.Y), new Point(0, (int)(this.DockingTarget.item.WorldPosition.Y - this.item.WorldPosition.Y))), this.IsHorizontal, this.item);
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

		// Token: 0x0600631F RID: 25375 RVA: 0x00339DE0 File Offset: 0x00337FE0
		private void RemoveConvexHulls()
		{
			for (int i = 0; i < this.convexHulls.Length; i++)
			{
				ConvexHull convexHull = this.convexHulls[i];
				if (convexHull != null)
				{
					convexHull.Remove();
				}
				this.convexHulls[i] = null;
			}
		}

		// Token: 0x06006320 RID: 25376 RVA: 0x00339E1C File Offset: 0x0033801C
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

		// Token: 0x06006321 RID: 25377 RVA: 0x0033A194 File Offset: 0x00338394
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
			this.RemoveConvexHulls();
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
			GUIMessageBox guimessageBox = this.autodockingVerification;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			this.autodockingVerification = null;
			Action onUnDocked = this.OnUnDocked;
			if (onUnDocked != null)
			{
				onUnDocked();
			}
			this.OnUnDocked = null;
		}

		// Token: 0x06006322 RID: 25378 RVA: 0x0033A524 File Offset: 0x00338724
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

		// Token: 0x06006323 RID: 25379 RVA: 0x0033A8D0 File Offset: 0x00338AD0
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
			this.RemoveConvexHulls();
			Sprite sprite = this.overlaySprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.overlaySprite = null;
		}

		// Token: 0x06006324 RID: 25380 RVA: 0x0033A960 File Offset: 0x00338B60
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

		// Token: 0x06006325 RID: 25381 RVA: 0x0033AB70 File Offset: 0x00338D70
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

		// Token: 0x06006326 RID: 25382 RVA: 0x0033AC34 File Offset: 0x00338E34
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) != null && !CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
				{
					return;
				}
			}
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
								goto IL_163;
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
										goto IL_164;
									}
								}
							}
						}
						flag = false;
						goto IL_164;
					}
				}
				IL_163:
				flag = false;
				IL_164:
				bool tryingToToggleOutpostDocking = flag;
				if (GameMain.NetworkMember != null && tryingToToggleOutpostDocking && signal.sender == null)
				{
					if (this.allowOutpostAutoDocking == DockingPort.AllowOutpostAutoDocking.Ask)
					{
						if (!this.outpostAutoDockingPromptShown)
						{
							this.autodockingVerification = new GUIMessageBox(string.Empty, TextManager.Get(newDockedState ? "autodockverification" : "autoundockverification"), new LocalizedString[]
							{
								TextManager.Get("Yes"),
								TextManager.Get("No")
							}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
							GUIButton guibutton = this.autodockingVerification.Buttons[0];
							guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
							{
								GUIMessageBox guimessageBox = this.autodockingVerification;
								if (guimessageBox != null)
								{
									guimessageBox.Close();
								}
								this.autodockingVerification = null;
								if (this.item.Removed || GameMain.Client == null)
								{
									return false;
								}
								this.allowOutpostAutoDocking = DockingPort.AllowOutpostAutoDocking.Yes;
								this.item.CreateClientEvent<DockingPort>(this);
								return true;
							}));
							GUIButton guibutton2 = this.autodockingVerification.Buttons[1];
							guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
							{
								GUIMessageBox guimessageBox = this.autodockingVerification;
								if (guimessageBox != null)
								{
									guimessageBox.Close();
								}
								this.autodockingVerification = null;
								if (this.item.Removed || GameMain.Client == null)
								{
									return false;
								}
								this.allowOutpostAutoDocking = DockingPort.AllowOutpostAutoDocking.No;
								this.item.CreateClientEvent<DockingPort>(this);
								return true;
							}));
						}
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
		}

		// Token: 0x0400332A RID: 13098
		private GUIMessageBox autodockingVerification;

		// Token: 0x0400332B RID: 13099
		private readonly ConvexHull[] convexHulls = new ConvexHull[2];

		// Token: 0x0400332C RID: 13100
		private static readonly List<DockingPort> list = new List<DockingPort>();

		// Token: 0x0400332D RID: 13101
		private Sprite overlaySprite;

		// Token: 0x0400332E RID: 13102
		private float dockingState;

		// Token: 0x0400332F RID: 13103
		private Joint joint;

		// Token: 0x04003330 RID: 13104
		private readonly Hull[] hulls = new Hull[2];

		// Token: 0x04003331 RID: 13105
		private Gap gap;

		// Token: 0x04003332 RID: 13106
		private Body[] bodies;

		// Token: 0x04003333 RID: 13107
		private Fixture outsideBlocker;

		// Token: 0x04003334 RID: 13108
		private Body doorBody;

		// Token: 0x04003335 RID: 13109
		private float dockingCooldown;

		// Token: 0x04003336 RID: 13110
		private bool docked;

		// Token: 0x04003337 RID: 13111
		private bool obstructedWayPointsDisabled;

		// Token: 0x04003338 RID: 13112
		private float forceLockTimer;

		// Token: 0x04003339 RID: 13113
		private const float ForceLockDelay = 1f;

		// Token: 0x04003346 RID: 13126
		private bool outpostAutoDockingPromptShown;

		// Token: 0x04003347 RID: 13127
		private DockingPort.AllowOutpostAutoDocking allowOutpostAutoDocking;

		// Token: 0x04003348 RID: 13128
		private bool initialized;

		// Token: 0x0200148C RID: 5260
		public enum DirectionType
		{
			// Token: 0x0400661C RID: 26140
			None,
			// Token: 0x0400661D RID: 26141
			Top,
			// Token: 0x0400661E RID: 26142
			Bottom,
			// Token: 0x0400661F RID: 26143
			Left,
			// Token: 0x04006620 RID: 26144
			Right
		}

		// Token: 0x0200148D RID: 5261
		private enum AllowOutpostAutoDocking
		{
			// Token: 0x04006622 RID: 26146
			Ask,
			// Token: 0x04006623 RID: 26147
			Yes,
			// Token: 0x04006624 RID: 26148
			No
		}
	}
}
