using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A0 RID: 1184
	internal class Steering : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001162 RID: 4450
		// (get) Token: 0x06004162 RID: 16738 RVA: 0x001A398D File Offset: 0x001A1B8D
		// (set) Token: 0x06004163 RID: 16739 RVA: 0x001A3995 File Offset: 0x001A1B95
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool MaintainPos { get; set; }

		// Token: 0x17001163 RID: 4451
		// (get) Token: 0x06004164 RID: 16740 RVA: 0x001A399E File Offset: 0x001A1B9E
		// (set) Token: 0x06004165 RID: 16741 RVA: 0x001A39A6 File Offset: 0x001A1BA6
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool LevelStartSelected { get; set; }

		// Token: 0x17001164 RID: 4452
		// (get) Token: 0x06004166 RID: 16742 RVA: 0x001A39AF File Offset: 0x001A1BAF
		// (set) Token: 0x06004167 RID: 16743 RVA: 0x001A39B7 File Offset: 0x001A1BB7
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool LevelEndSelected { get; set; }

		// Token: 0x17001165 RID: 4453
		// (get) Token: 0x06004168 RID: 16744 RVA: 0x001A39C0 File Offset: 0x001A1BC0
		// (set) Token: 0x06004169 RID: 16745 RVA: 0x001A39C8 File Offset: 0x001A1BC8
		public bool UnsentChanges
		{
			get
			{
				return this.unsentChanges;
			}
			set
			{
				this.unsentChanges = value;
			}
		}

		// Token: 0x0600416A RID: 16746 RVA: 0x001A39D1 File Offset: 0x001A1BD1
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.pathFinder = null;
		}

		// Token: 0x0600416B RID: 16747 RVA: 0x001A39E0 File Offset: 0x001A1BE0
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool autoPilot = msg.ReadBoolean();
			bool dockingButtonClicked = msg.ReadBoolean();
			Vector2 newSteeringInput = this.targetVelocity;
			Vector2? newPosToMaintain = null;
			bool headingToStart = false;
			if (autoPilot)
			{
				bool maintainPos = msg.ReadBoolean();
				if (maintainPos)
				{
					newPosToMaintain = new Vector2?(new Vector2(msg.ReadSingle(), msg.ReadSingle()));
				}
				else
				{
					headingToStart = msg.ReadBoolean();
				}
			}
			else
			{
				newSteeringInput = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			}
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			this.user = c.Character;
			this.AutoPilot = autoPilot;
			if (dockingButtonClicked)
			{
				this.item.SendSignal(new Signal("1", 0, c.Character, null, 0f, 1f), "toggle_docking");
				this.item.CreateServerEvent<Steering>(this, new Steering.EventData(true));
			}
			if (!this.AutoPilot)
			{
				this.steeringInput = newSteeringInput;
				this.steeringAdjustSpeed = MathHelper.Lerp(0.2f, 1f, c.Character.GetSkillLevel(Tags.HelmSkill) / 100f);
			}
			else
			{
				this.MaintainPos = (newPosToMaintain != null);
				this.posToMaintain = newPosToMaintain;
				if (this.posToMaintain == null)
				{
					this.LevelStartSelected = headingToStart;
					this.LevelEndSelected = !headingToStart;
					this.UpdatePath();
				}
				else
				{
					this.LevelStartSelected = false;
					this.LevelEndSelected = false;
				}
			}
			this.unsentChanges = true;
		}

		// Token: 0x0600416C RID: 16748 RVA: 0x001A3B48 File Offset: 0x001A1D48
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.autoPilot);
			Steering.EventData eventData;
			msg.WriteBoolean(base.TryExtractEventData<Steering.EventData>(extraData, out eventData) && eventData.DockingButtonClicked);
			Character character = this.user;
			msg.WriteUInt16((character != null) ? character.ID : 0);
			if (!this.autoPilot)
			{
				msg.WriteSingle(this.steeringInput.X);
				msg.WriteSingle(this.steeringInput.Y);
				msg.WriteSingle(this.targetVelocity.X);
				msg.WriteSingle(this.targetVelocity.Y);
				msg.WriteSingle(this.steeringAdjustSpeed);
				return;
			}
			msg.WriteBoolean(this.posToMaintain != null);
			if (this.posToMaintain != null)
			{
				msg.WriteSingle(this.posToMaintain.Value.X);
				msg.WriteSingle(this.posToMaintain.Value.Y);
				return;
			}
			msg.WriteBoolean(this.LevelStartSelected);
		}

		// Token: 0x17001166 RID: 4454
		// (get) Token: 0x0600416D RID: 16749 RVA: 0x001A3C43 File Offset: 0x001A1E43
		public Submarine ControlledSub
		{
			get
			{
				return this.controlledSub;
			}
		}

		// Token: 0x17001167 RID: 4455
		// (get) Token: 0x0600416E RID: 16750 RVA: 0x001A3C4B File Offset: 0x001A1E4B
		// (set) Token: 0x0600416F RID: 16751 RVA: 0x001A3C53 File Offset: 0x001A1E53
		public Vector2 AITacticalTarget { get; set; }

		// Token: 0x17001168 RID: 4456
		// (get) Token: 0x06004170 RID: 16752 RVA: 0x001A3C5C File Offset: 0x001A1E5C
		// (set) Token: 0x06004171 RID: 16753 RVA: 0x001A3C64 File Offset: 0x001A1E64
		public float AIRamTimer { get; set; }

		// Token: 0x17001169 RID: 4457
		// (get) Token: 0x06004172 RID: 16754 RVA: 0x001A3C6D File Offset: 0x001A1E6D
		// (set) Token: 0x06004173 RID: 16755 RVA: 0x001A3C78 File Offset: 0x001A1E78
		[Serialize(false, IsPropertySaveable.Yes, "Is autopilot currently on or not?", "", false, AlwaysUseInstanceValues = true)]
		public bool AutoPilot
		{
			get
			{
				return this.autoPilot;
			}
			set
			{
				if (value == this.autoPilot)
				{
					return;
				}
				this.autoPilot = value;
				if (this.autoPilot)
				{
					this.MaintainPos = true;
					if (this.posToMaintain == null)
					{
						this.RefreshPosToMaintain();
						return;
					}
				}
				else
				{
					this.PosToMaintain = null;
					this.MaintainPos = false;
					this.LevelEndSelected = false;
					this.LevelStartSelected = false;
				}
			}
		}

		// Token: 0x1700116A RID: 4458
		// (get) Token: 0x06004174 RID: 16756 RVA: 0x001A3CDD File Offset: 0x001A1EDD
		// (set) Token: 0x06004175 RID: 16757 RVA: 0x001A3CE5 File Offset: 0x001A1EE5
		[Editable(0f, 1f, 4)]
		[Serialize(0.5f, IsPropertySaveable.Yes, "How full the ballast tanks should be when the submarine is not being steered upwards/downwards. Can be used to compensate if the ballast tanks are too large/small relative to the size of the submarine.", "", false)]
		public float NeutralBallastLevel
		{
			get
			{
				return this.neutralBallastLevel;
			}
			set
			{
				this.neutralBallastLevel = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x1700116B RID: 4459
		// (get) Token: 0x06004176 RID: 16758 RVA: 0x001A3CFD File Offset: 0x001A1EFD
		// (set) Token: 0x06004177 RID: 16759 RVA: 0x001A3D05 File Offset: 0x001A1F05
		[Serialize(1000f, IsPropertySaveable.Yes, "How close the docking port has to be to another docking port for the docking mode to become active.", "", false)]
		public float DockingAssistThreshold { get; set; }

		// Token: 0x1700116C RID: 4460
		// (get) Token: 0x06004178 RID: 16760 RVA: 0x001A3D0E File Offset: 0x001A1F0E
		// (set) Token: 0x06004179 RID: 16761 RVA: 0x001A3D18 File Offset: 0x001A1F18
		public Vector2 TargetVelocity
		{
			get
			{
				return this.targetVelocity;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					if (!MathUtils.IsValid(this.targetVelocity))
					{
						this.targetVelocity = Vector2.Zero;
					}
					return;
				}
				this.targetVelocity.X = MathHelper.Clamp(value.X, -100f, 100f);
				this.targetVelocity.Y = MathHelper.Clamp(value.Y, -100f, 100f);
			}
		}

		// Token: 0x1700116D RID: 4461
		// (get) Token: 0x0600417A RID: 16762 RVA: 0x001A3D88 File Offset: 0x001A1F88
		public float TargetVelocityLengthSquared
		{
			get
			{
				return this.TargetVelocity.LengthSquared();
			}
		}

		// Token: 0x1700116E RID: 4462
		// (get) Token: 0x0600417B RID: 16763 RVA: 0x001A3DA3 File Offset: 0x001A1FA3
		// (set) Token: 0x0600417C RID: 16764 RVA: 0x001A3DAC File Offset: 0x001A1FAC
		public Vector2 SteeringInput
		{
			get
			{
				return this.steeringInput;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.steeringInput.X = MathHelper.Clamp(value.X, -100f, 100f);
				this.steeringInput.Y = MathHelper.Clamp(value.Y, -100f, 100f);
			}
		}

		// Token: 0x1700116F RID: 4463
		// (get) Token: 0x0600417D RID: 16765 RVA: 0x001A3E02 File Offset: 0x001A2002
		public SteeringPath SteeringPath
		{
			get
			{
				return this.steeringPath;
			}
		}

		// Token: 0x17001170 RID: 4464
		// (get) Token: 0x0600417E RID: 16766 RVA: 0x001A3E0A File Offset: 0x001A200A
		// (set) Token: 0x0600417F RID: 16767 RVA: 0x001A3E12 File Offset: 0x001A2012
		public Vector2? PosToMaintain
		{
			get
			{
				return this.posToMaintain;
			}
			set
			{
				this.posToMaintain = value;
			}
		}

		// Token: 0x17001171 RID: 4465
		// (get) Token: 0x06004180 RID: 16768 RVA: 0x001A3E1B File Offset: 0x001A201B
		// (set) Token: 0x06004181 RID: 16769 RVA: 0x001A3E2D File Offset: 0x001A202D
		public bool DockingModeEnabled
		{
			get
			{
				return this.UseAutoDocking && this.dockingModeEnabled;
			}
			set
			{
				this.dockingModeEnabled = value;
			}
		}

		// Token: 0x17001172 RID: 4466
		// (get) Token: 0x06004182 RID: 16770 RVA: 0x001A3E36 File Offset: 0x001A2036
		// (set) Token: 0x06004183 RID: 16771 RVA: 0x001A3E3E File Offset: 0x001A203E
		public bool UseAutoDocking { get; set; } = true;

		// Token: 0x06004184 RID: 16772 RVA: 0x001A3E48 File Offset: 0x001A2048
		private void FindConnectedDockingPort()
		{
			this.searchedConnectedDockingPort = true;
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item item = linkedTo as Item;
				if (item != null)
				{
					DockingPort port = item.GetComponent<DockingPort>();
					if (port != null)
					{
						this.DockingSources.Add(port);
					}
				}
			}
			Connection dockingConnection = this.item.Connections.FirstOrDefault((Connection c) => c.Name == "toggle_docking");
			if (dockingConnection != null)
			{
				List<DockingPort> connectedPorts = this.item.GetConnectedComponentsRecursive<DockingPort>(dockingConnection, false, false);
				this.DockingSources.AddRange(from p in connectedPorts
				where p.Item.Submarine != null && !p.Item.Submarine.Info.IsOutpost
				select p);
			}
		}

		// Token: 0x06004185 RID: 16773 RVA: 0x001A3F38 File Offset: 0x001A2138
		public Steering(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004186 RID: 16774 RVA: 0x001A3F87 File Offset: 0x001A2187
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.sonar = this.item.GetComponent<Sonar>();
		}

		// Token: 0x06004187 RID: 16775 RVA: 0x001A3FA0 File Offset: 0x001A21A0
		public override bool Select(Character character)
		{
			if (!base.CanBeSelected)
			{
				return false;
			}
			this.user = character;
			return true;
		}

		// Token: 0x06004188 RID: 16776 RVA: 0x001A3FB4 File Offset: 0x001A21B4
		public void RefreshPosToMaintain()
		{
			this.posToMaintain = new Vector2?((this.controlledSub != null) ? this.controlledSub.WorldPosition : ((this.item.Submarine == null) ? this.item.WorldPosition : this.item.Submarine.WorldPosition));
		}

		// Token: 0x06004189 RID: 16777 RVA: 0x001A400B File Offset: 0x001A220B
		public override void OnMapLoaded()
		{
			if (this.MaintainPos)
			{
				this.RefreshPosToMaintain();
			}
		}

		// Token: 0x0600418A RID: 16778 RVA: 0x001A401C File Offset: 0x001A221C
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.searchedConnectedDockingPort)
			{
				this.FindConnectedDockingPort();
			}
			this.networkUpdateTimer -= deltaTime;
			if (this.unsentChanges && this.networkUpdateTimer <= 0f)
			{
				this.item.CreateServerEvent<Steering>(this);
				this.networkUpdateTimer = 0.1f;
				this.unsentChanges = false;
			}
			this.controlledSub = this.item.Submarine;
			Sonar sonar = this.item.GetComponent<Sonar>();
			if (sonar != null && sonar.UseTransducers)
			{
				this.controlledSub = (sonar.ConnectedTransducers.Any<SonarTransducer>() ? sonar.ConnectedTransducers.First<SonarTransducer>().Item.Submarine : null);
			}
			if (!this.HasPower)
			{
				return;
			}
			if (this.user != null && this.user.Removed)
			{
				this.user = null;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float userSkill = 0f;
			if (this.user != null && this.controlledSub != null && (this.user.SelectedItem == this.item || this.item.linkedTo.Contains(this.user.SelectedItem)))
			{
				userSkill = this.user.GetSkillLevel(Tags.HelmSkill) / 100f;
			}
			if (this.AIRamTimer > 0f && this.controlledSub != null)
			{
				this.AIRamTimer -= deltaTime;
				this.TargetVelocity = this.GetSteeringVelocity(this.AITacticalTarget, 0f);
			}
			else if (this.AutoPilot)
			{
				if (this.lastReceivedSteeringSignalTime < Timing.TotalTime - 1.0)
				{
					this.UpdateAutoPilot(deltaTime);
					float throttle = 1f;
					if (this.controlledSub != null)
					{
						throttle = MathHelper.Clamp(Vector2.Dot(this.controlledSub.Velocity, this.TargetVelocity) / 100f, 0f, 1f);
					}
					float maxSpeed = MathHelper.Lerp(0.5f, 1f, userSkill) * 100f;
					this.TargetVelocity = this.TargetVelocity.ClampLength(MathHelper.Lerp(100f, maxSpeed, throttle));
				}
			}
			else
			{
				this.showIceSpireWarning = false;
				if (this.user != null && this.user.Info != null && this.user.SelectedItem == this.item)
				{
					this.IncreaseSkillLevel(this.user, deltaTime);
				}
				Vector2 velocityDiff = this.steeringInput - this.targetVelocity;
				if (velocityDiff != Vector2.Zero)
				{
					if (this.steeringAdjustSpeed >= 0.99f)
					{
						this.TargetVelocity = this.steeringInput;
					}
					else
					{
						float steeringChange = 1f / (1f - this.steeringAdjustSpeed);
						steeringChange *= steeringChange * 10f;
						this.TargetVelocity += Vector2.Normalize(velocityDiff) * Math.Min(steeringChange * deltaTime, velocityDiff.Length());
					}
				}
			}
			float velX = this.targetVelocity.X;
			if (this.controlledSub != null && this.controlledSub.FlippedX)
			{
				velX *= -1f;
			}
			this.item.SendSignal(new Signal(velX.ToString(CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "velocity_x_out");
			float velY = MathHelper.Lerp((this.neutralBallastLevel * 100f - 50f) * 2f, (float)(-100 * Math.Sign(this.targetVelocity.Y)), Math.Abs(this.targetVelocity.Y) / 100f);
			this.item.SendSignal(new Signal(velY.ToString(CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "velocity_y_out");
			Submarine sub = this.controlledSub;
			if (sub != null)
			{
				this.item.SendSignal(new Signal((ConvertUnits.ToDisplayUnits(sub.Velocity.X * Physics.DisplayToRealWorldRatio) * 3.6f).ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_velocity_x");
				this.item.SendSignal(new Signal((ConvertUnits.ToDisplayUnits(sub.Velocity.Y * Physics.DisplayToRealWorldRatio) * -3.6f).ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_velocity_y");
				Vector2 pos = new Vector2(sub.WorldPosition.X * Physics.DisplayToRealWorldRatio, sub.RealWorldDepth);
				if (sonar != null && sonar.UseTransducers && sonar.CenterOnTransducers && sonar.ConnectedTransducers.Any<SonarTransducer>())
				{
					pos = Vector2.Zero;
					foreach (SonarTransducer connectedTransducer in sonar.ConnectedTransducers)
					{
						pos += connectedTransducer.Item.WorldPosition;
					}
					pos /= (float)sonar.ConnectedTransducers.Count<SonarTransducer>();
					float x = pos.X * Physics.DisplayToRealWorldRatio;
					Level loaded = Level.Loaded;
					pos = new Vector2(x, (loaded != null) ? loaded.GetRealWorldDepth(pos.Y) : (-pos.Y * Physics.DisplayToRealWorldRatio));
				}
				this.item.SendSignal(new Signal(pos.X.ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_position_x");
				this.item.SendSignal(new Signal(pos.Y.ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_position_y");
			}
			if (this.navigateTactically && (this.user == null || this.user.SelectedItem != this.item))
			{
				this.navigateTactically = false;
				this.AIRamTimer = 0f;
				this.SetMaintainPosition();
			}
		}

		// Token: 0x0600418B RID: 16779 RVA: 0x001A4650 File Offset: 0x001A2850
		private void IncreaseSkillLevel(Character user, float deltaTime)
		{
			if (this.controlledSub == null)
			{
				return;
			}
			if (this.controlledSub.Velocity.LengthSquared() < 0.01f)
			{
				return;
			}
			if (((user != null) ? user.Info : null) == null)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) != null)
			{
				if (this.controlledSub.DockedTo.Any((Submarine d) => d.PhysicsBody.BodyType == BodyType.Static))
				{
					return;
				}
			}
			float speedMultiplier = MathHelper.Clamp(this.TargetVelocity.Length() / 100f, 0f, 1f);
			user.Info.ApplySkillGain(Tags.HelmSkill, SkillSettings.Current.SkillIncreasePerSecondWhenSteering * speedMultiplier * deltaTime, false, 2f, false);
		}

		// Token: 0x0600418C RID: 16780 RVA: 0x001A4720 File Offset: 0x001A2920
		private void UpdateAutoPilot(float deltaTime)
		{
			if (this.controlledSub == null)
			{
				return;
			}
			if (this.posToMaintain != null)
			{
				Vector2 steeringVel = this.GetSteeringVelocity(this.posToMaintain.Value, 10f);
				this.TargetVelocity = Vector2.Lerp(this.TargetVelocity, steeringVel, 0.1f);
				this.showIceSpireWarning = false;
				return;
			}
			this.autopilotRayCastTimer -= deltaTime;
			this.autopilotRecalculatePathTimer -= deltaTime;
			if (this.autopilotRecalculatePathTimer <= 0f)
			{
				this.UpdatePath();
				this.autopilotRecalculatePathTimer = 5f;
			}
			if (this.steeringPath == null)
			{
				this.showIceSpireWarning = false;
				return;
			}
			this.steeringPath.CheckProgress(ConvertUnits.ToSimUnits(this.controlledSub.WorldPosition), 10f);
			this.connectedSubUpdateTimer -= deltaTime;
			if (this.connectedSubUpdateTimer <= 0f)
			{
				this.connectedSubs.Clear();
				this.connectedSubs.AddRange(this.controlledSub.GetConnectedSubs());
				this.connectedSubUpdateTimer = 1f;
			}
			if (this.autopilotRayCastTimer <= 0f && this.steeringPath.NextNode != null)
			{
				Vector2 diff = ConvertUnits.ToSimUnits(this.steeringPath.NextNode.Position - this.controlledSub.WorldPosition);
				float lengthSqr = diff.LengthSquared();
				if (lengthSqr > 0.001f && lengthSqr < 900f)
				{
					diff = Vector2.Normalize(diff);
					bool nextVisible = true;
					for (int x = -1; x < 2; x += 2)
					{
						for (int y = -1; y < 2; y += 2)
						{
							Vector2 cornerPos = new Vector2((float)(this.controlledSub.Borders.Width * x), (float)(this.controlledSub.Borders.Height * y)) / 2f;
							cornerPos = ConvertUnits.ToSimUnits(cornerPos * 1.1f + this.controlledSub.WorldPosition);
							float dist = Vector2.Distance(cornerPos, this.steeringPath.NextNode.SimPosition);
							if (Submarine.PickBody(cornerPos, cornerPos + diff * dist, null, new Category?(Category.Cat8), true, null, false) != null)
							{
								nextVisible = false;
								x = 2;
								y = 2;
							}
						}
					}
					if (nextVisible)
					{
						this.steeringPath.SkipToNextNode();
					}
				}
				this.autopilotRayCastTimer = 0.5f;
			}
			Vector2 newVelocity = Vector2.Zero;
			if (this.steeringPath.CurrentNode != null)
			{
				newVelocity = this.GetSteeringVelocity(this.steeringPath.CurrentNode.WorldPosition, 2f);
			}
			Vector2 avoidDist = new Vector2(Math.Max(1000f * Math.Abs(this.controlledSub.Velocity.X), (float)this.controlledSub.Borders.Width * 0.75f), Math.Max(1000f * Math.Abs(this.controlledSub.Velocity.Y), (float)this.controlledSub.Borders.Height * 0.75f));
			float avoidRadius = avoidDist.Length();
			float damagingWallAvoidRadius = MathHelper.Clamp(avoidRadius * 1.5f, 5000f, 10000f);
			Vector2 newAvoidStrength = Vector2.Zero;
			this.debugDrawObstacles.Clear();
			this.showIceSpireWarning = false;
			List<VoronoiCell> closeCells = Level.Loaded.GetCells(this.controlledSub.WorldPosition, 4);
			foreach (VoronoiCell cell in closeCells)
			{
				if (!cell.DoesDamage)
				{
					Body body = cell.Body;
					if (body == null || body.BodyType != BodyType.Dynamic)
					{
						goto IL_4F5;
					}
				}
				using (List<GraphEdge>.Enumerator enumerator2 = cell.Edges.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GraphEdge edge = enumerator2.Current;
						Vector2 closestPoint = MathUtils.GetClosestPointOnLineSegment(edge.Point1 + cell.Translation, edge.Point2 + cell.Translation, this.controlledSub.WorldPosition);
						Vector2 diff2 = closestPoint - this.controlledSub.WorldPosition;
						float dist2 = diff2.Length() - (float)(Math.Max(this.controlledSub.Borders.Width, this.controlledSub.Borders.Height) / 2);
						if (dist2 <= damagingWallAvoidRadius)
						{
							Vector2 normalizedDiff = Vector2.Normalize(diff2);
							float dot = Vector2.Dot(normalizedDiff, this.controlledSub.Velocity);
							float avoidStrength = MathHelper.Clamp(MathHelper.Lerp(1f, 0f, dist2 / damagingWallAvoidRadius - dot), 0f, 1f);
							Vector2 avoid = -normalizedDiff * avoidStrength;
							newAvoidStrength += avoid;
							this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge, new Vector2?(edge.Center), 1f, avoid, cell.Translation));
							if (dot > 0f && cell.DoesDamage)
							{
								this.showIceSpireWarning = true;
							}
						}
					}
					continue;
				}
				IL_4F5:
				foreach (GraphEdge edge2 in cell.Edges)
				{
					Vector2 intersection;
					if (MathUtils.GetLineSegmentIntersection(edge2.Point1 + cell.Translation, edge2.Point2 + cell.Translation, this.controlledSub.WorldPosition, cell.Center, out intersection))
					{
						Vector2 diff3 = this.controlledSub.WorldPosition - intersection;
						if (Math.Abs(diff3.X) > avoidDist.X && Math.Abs(diff3.Y) > avoidDist.Y)
						{
							this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), 0f, Vector2.Zero, Vector2.Zero));
						}
						else
						{
							if (diff3.LengthSquared() < 1f)
							{
								diff3 = Vector2.UnitY;
							}
							Vector2 normalizedDiff2 = Vector2.Normalize(diff3);
							float dot2 = (this.controlledSub.Velocity == Vector2.Zero) ? 0f : Vector2.Dot(this.controlledSub.Velocity, -normalizedDiff2);
							if ((double)dot2 < 1.0)
							{
								this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), dot2, Vector2.Zero, cell.Translation));
							}
							else
							{
								Vector2 change = normalizedDiff2 * Math.Max(avoidRadius - diff3.Length(), 0f) / avoidRadius;
								if (change.LengthSquared() >= 0.001f)
								{
									newAvoidStrength += change * (dot2 - 1f);
									this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), dot2 - 1f, change * (dot2 - 1f), cell.Translation));
								}
							}
						}
					}
				}
			}
			this.avoidStrength = Vector2.Lerp(this.avoidStrength, newAvoidStrength, deltaTime * 10f);
			this.TargetVelocity = Vector2.Lerp(this.TargetVelocity, newVelocity + this.avoidStrength * 100f, 0.1f);
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub != this.controlledSub && !this.connectedSubs.Contains(sub))
				{
					Vector2 minDist = (this.controlledSub.Borders.Size + sub.Borders.Size).ToVector2() / 2f;
					Vector2 diff4 = this.controlledSub.WorldPosition - sub.WorldPosition;
					float xDist = Math.Abs(diff4.X);
					float yDist = Math.Abs(diff4.Y);
					Vector2 maxAvoidDistance = minDist * 2f;
					if (xDist <= maxAvoidDistance.X && yDist <= maxAvoidDistance.Y)
					{
						float dot3 = (this.controlledSub.Velocity == Vector2.Zero) ? 0f : Vector2.Dot(Vector2.Normalize(this.controlledSub.Velocity), -diff4);
						if (dot3 >= 0f)
						{
							float distanceFactor = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(maxAvoidDistance.X + maxAvoidDistance.Y, minDist.X + minDist.Y, xDist + yDist));
							float velocityFactor = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 3f, this.controlledSub.Velocity.Length()));
							this.TargetVelocity += 100f * Vector2.Normalize(diff4) * distanceFactor * velocityFactor;
						}
					}
				}
			}
			float velMagnitude = this.TargetVelocity.Length();
			if (velMagnitude > 100f)
			{
				this.TargetVelocity *= 100f / velMagnitude;
			}
		}

		// Token: 0x0600418D RID: 16781 RVA: 0x001A50E4 File Offset: 0x001A32E4
		private float? GetNodePenalty(PathNode node, PathNode nextNode)
		{
			WayPoint waypoint = node.Waypoint;
			if (((waypoint != null) ? waypoint.Tunnel : null) == null || this.controlledSub == null || node.Waypoint.Tunnel.Type == Level.TunnelType.MainPath)
			{
				return new float?(0f);
			}
			if (node.Waypoint.Tunnel.Type == Level.TunnelType.MainPath)
			{
				WayPoint waypoint2 = nextNode.Waypoint;
				bool flag;
				if (waypoint2 == null)
				{
					flag = true;
				}
				else
				{
					Level.Tunnel tunnel = waypoint2.Tunnel;
					Level.TunnelType? tunnelType = (tunnel != null) ? new Level.TunnelType?(tunnel.Type) : null;
					Level.TunnelType tunnelType2 = Level.TunnelType.MainPath;
					flag = !(tunnelType.GetValueOrDefault() == tunnelType2 & tunnelType != null);
				}
				if (flag)
				{
					return null;
				}
			}
			return new float?(1000f);
		}

		// Token: 0x0600418E RID: 16782 RVA: 0x001A5198 File Offset: 0x001A3398
		private void UpdatePath()
		{
			if (Level.Loaded == null)
			{
				return;
			}
			if (this.pathFinder == null)
			{
				this.pathFinder = new PathFinder(WayPoint.WayPointList, false)
				{
					GetNodePenalty = new PathFinder.GetNodePenaltyHandler(this.GetNodePenalty)
				};
			}
			Vector2 target;
			if (this.navigateTactically)
			{
				target = ConvertUnits.ToSimUnits(this.AITacticalTarget);
			}
			else if (this.LevelEndSelected)
			{
				target = ConvertUnits.ToSimUnits(Level.Loaded.EndExitPosition);
			}
			else
			{
				target = ConvertUnits.ToSimUnits(Level.Loaded.StartExitPosition);
			}
			PathFinder pathFinder = this.pathFinder;
			Vector2 start = ConvertUnits.ToSimUnits((this.controlledSub == null) ? this.item.WorldPosition : this.controlledSub.WorldPosition);
			Vector2 end = target;
			Submarine hostSub = null;
			string str = "(Autopilot, target: ";
			Vector2 vector = target;
			this.steeringPath = pathFinder.FindPath(start, end, hostSub, str + vector.ToString() + ")", 0f, null, null, null, true, 0f);
		}

		// Token: 0x0600418F RID: 16783 RVA: 0x001A527C File Offset: 0x001A347C
		public void SetDestinationLevelStart()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelEndSelected = false;
			this.navigateTactically = false;
			if (!this.LevelStartSelected)
			{
				this.LevelStartSelected = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06004190 RID: 16784 RVA: 0x001A52BB File Offset: 0x001A34BB
		public void SetDestinationLevelEnd()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelStartSelected = false;
			this.navigateTactically = false;
			if (!this.LevelEndSelected)
			{
				this.LevelEndSelected = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06004191 RID: 16785 RVA: 0x001A52FA File Offset: 0x001A34FA
		private void SetDestinationTactical()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelStartSelected = false;
			this.LevelEndSelected = false;
			if (!this.navigateTactically)
			{
				this.navigateTactically = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06004192 RID: 16786 RVA: 0x001A533C File Offset: 0x001A353C
		private void SetMaintainPosition()
		{
			if (!this.MaintainPos)
			{
				this.unsentChanges = true;
				this.MaintainPos = true;
			}
			if (this.posToMaintain == null)
			{
				this.unsentChanges = true;
				this.posToMaintain = new Vector2?((this.controlledSub != null) ? this.controlledSub.WorldPosition : ((this.item.Submarine == null) ? this.item.WorldPosition : this.item.Submarine.WorldPosition));
			}
		}

		// Token: 0x06004193 RID: 16787 RVA: 0x001A53C0 File Offset: 0x001A35C0
		private Vector2 GetSteeringVelocity(Vector2 worldPosition, float slowdownAmount)
		{
			Vector2 futurePosition = ConvertUnits.ToDisplayUnits(this.controlledSub.Velocity) * slowdownAmount;
			Vector2 targetSpeed = worldPosition - this.controlledSub.WorldPosition - futurePosition;
			if (targetSpeed.LengthSquared() > 250000f)
			{
				return Vector2.Normalize(targetSpeed) * 100f;
			}
			return targetSpeed / 5f;
		}

		// Token: 0x06004194 RID: 16788 RVA: 0x001A5428 File Offset: 0x001A3628
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			character.AIController.SteeringManager.Reset();
			if (objective.Override && this.user != character && this.user != null && this.user.SelectedItem == this.item && character.IsOnPlayerTeam)
			{
				character.Speak(TextManager.Get("DialogSteeringTaken").Value, null, 0f, "steeringtaken".ToIdentifier(), 10f);
			}
			this.user = character;
			if (base.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(base.Item, character))
			{
				if (base.Item.Repairables.Average((Repairable r) => r.DegreeOfSuccess(character)) > 0.4f)
				{
					objective.AddSubObjective(new AIObjectiveRepairItem(character, base.Item, objective.objectiveManager, 1f, true), false);
					return false;
				}
				Character character2 = character;
				string value = TextManager.Get("DialogNavTerminalIsBroken").Value;
				Identifier identifier = "navterminalisbroken".ToIdentifier();
				character2.Speak(value, null, 0f, identifier, 30f);
			}
			if (!this.AutoPilot)
			{
				this.unsentChanges = true;
				this.AutoPilot = true;
			}
			this.IncreaseSkillLevel(this.user, deltaTime);
			if (objective.Option == "maintainposition")
			{
				if (objective.Override)
				{
					this.SetMaintainPosition();
				}
			}
			else if (!Level.IsLoadedOutpost)
			{
				if (objective.Option == "navigateback")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || this.LevelEndSelected || !this.LevelStartSelected || this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationLevelStart();
					}
				}
				else if (objective.Option == "navigatetodestination")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || !this.LevelEndSelected || this.LevelStartSelected || this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationLevelEnd();
					}
				}
				else if (objective.Option == "navigatetactical")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || this.LevelEndSelected || this.LevelStartSelected || !this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationTactical();
					}
				}
			}
			Sonar sonar = this.sonar;
			if (sonar != null)
			{
				sonar.CrewAIOperate(deltaTime, character, objective);
			}
			if (!this.MaintainPos && this.showIceSpireWarning && character.IsOnPlayerTeam)
			{
				character.Speak(TextManager.Get("dialogicespirespottedsonar").Value, null, 0f, "icespirespottedsonar".ToIdentifier(), 60f);
			}
			return false;
		}

		// Token: 0x06004195 RID: 16789 RVA: 0x001A57F4 File Offset: 0x001A39F4
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name == "velocity_in")
			{
				this.steeringAdjustSpeed = 0.2f;
				this.steeringInput = XMLExtensions.ParseVector2(signal.value, false);
				this.steeringInput.X = MathHelper.Clamp(this.steeringInput.X, -100f, 100f);
				this.steeringInput.Y = MathHelper.Clamp(-this.steeringInput.Y, -100f, 100f);
				this.TargetVelocity = this.steeringInput;
				this.lastReceivedSteeringSignalTime = Timing.TotalTime;
				return;
			}
			base.ReceiveSignal(signal, connection);
		}

		// Token: 0x04001F5C RID: 8028
		public const float AutopilotMinDistToPathNode = 30f;

		// Token: 0x04001F5D RID: 8029
		private const float AutopilotRayCastInterval = 0.5f;

		// Token: 0x04001F5E RID: 8030
		private const float RecalculatePathInterval = 5f;

		// Token: 0x04001F5F RID: 8031
		private const float AutoPilotSteeringLerp = 0.1f;

		// Token: 0x04001F60 RID: 8032
		private const float AutoPilotMaxSpeed = 0.5f;

		// Token: 0x04001F61 RID: 8033
		private const float AIPilotMaxSpeed = 1f;

		// Token: 0x04001F62 RID: 8034
		public const float PressureWarningThreshold = 500f;

		// Token: 0x04001F63 RID: 8035
		private const float DefaultSteeringAdjustSpeed = 0.2f;

		// Token: 0x04001F64 RID: 8036
		private Vector2 targetVelocity;

		// Token: 0x04001F65 RID: 8037
		private Vector2 steeringInput;

		// Token: 0x04001F66 RID: 8038
		private bool autoPilot;

		// Token: 0x04001F67 RID: 8039
		private Vector2? posToMaintain;

		// Token: 0x04001F68 RID: 8040
		private SteeringPath steeringPath;

		// Token: 0x04001F69 RID: 8041
		private PathFinder pathFinder;

		// Token: 0x04001F6A RID: 8042
		private float networkUpdateTimer;

		// Token: 0x04001F6B RID: 8043
		private bool unsentChanges;

		// Token: 0x04001F6C RID: 8044
		private float autopilotRayCastTimer;

		// Token: 0x04001F6D RID: 8045
		private float autopilotRecalculatePathTimer;

		// Token: 0x04001F6E RID: 8046
		private Vector2 avoidStrength;

		// Token: 0x04001F6F RID: 8047
		private float neutralBallastLevel;

		// Token: 0x04001F70 RID: 8048
		private float steeringAdjustSpeed = 1f;

		// Token: 0x04001F71 RID: 8049
		private Character user;

		// Token: 0x04001F72 RID: 8050
		private Sonar sonar;

		// Token: 0x04001F73 RID: 8051
		private Submarine controlledSub;

		// Token: 0x04001F76 RID: 8054
		private bool navigateTactically;

		// Token: 0x04001F77 RID: 8055
		private bool showIceSpireWarning;

		// Token: 0x04001F78 RID: 8056
		private List<Submarine> connectedSubs = new List<Submarine>();

		// Token: 0x04001F79 RID: 8057
		private const float ConnectedSubUpdateInterval = 1f;

		// Token: 0x04001F7A RID: 8058
		private float connectedSubUpdateTimer;

		// Token: 0x04001F7B RID: 8059
		private double lastReceivedSteeringSignalTime;

		// Token: 0x04001F7D RID: 8061
		private List<Steering.ObstacleDebugInfo> debugDrawObstacles = new List<Steering.ObstacleDebugInfo>();

		// Token: 0x04001F7E RID: 8062
		public List<DockingPort> DockingSources = new List<DockingPort>();

		// Token: 0x04001F7F RID: 8063
		private bool searchedConnectedDockingPort;

		// Token: 0x04001F80 RID: 8064
		private bool dockingModeEnabled;

		// Token: 0x02000DB8 RID: 3512
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x0600681A RID: 26650 RVA: 0x00221E8F File Offset: 0x0022008F
			public EventData(bool dockingButtonClicked)
			{
				this.DockingButtonClicked = dockingButtonClicked;
			}

			// Token: 0x0400407C RID: 16508
			public readonly bool DockingButtonClicked;
		}

		// Token: 0x02000DB9 RID: 3513
		private struct ObstacleDebugInfo
		{
			// Token: 0x0600681B RID: 26651 RVA: 0x00221E98 File Offset: 0x00220098
			public ObstacleDebugInfo(GraphEdge edge, Vector2? intersection, float dot, Vector2 avoidStrength, Vector2 translation)
			{
				this.Point1 = edge.Point1 + translation;
				this.Point2 = edge.Point2 + translation;
				this.Intersection = intersection;
				this.Dot = dot;
				this.AvoidStrength = avoidStrength;
			}

			// Token: 0x0400407D RID: 16509
			public Vector2 Point1;

			// Token: 0x0400407E RID: 16510
			public Vector2 Point2;

			// Token: 0x0400407F RID: 16511
			public Vector2? Intersection;

			// Token: 0x04004080 RID: 16512
			public float Dot;

			// Token: 0x04004081 RID: 16513
			public Vector2 AvoidStrength;
		}
	}
}
