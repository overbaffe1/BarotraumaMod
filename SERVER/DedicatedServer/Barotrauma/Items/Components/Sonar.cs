using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D0 RID: 1232
	internal class Sonar : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004633 RID: 17971 RVA: 0x001C093C File Offset: 0x001BEB3C
		static Sonar()
		{
			Sonar.DirectionalPingDotProduct = (float)Math.Cos((double)(MathHelper.ToRadians(30f) * 0.5f));
		}

		// Token: 0x170012D5 RID: 4821
		// (get) Token: 0x06004634 RID: 17972 RVA: 0x001C096E File Offset: 0x001BEB6E
		public bool UseDirectionalPing
		{
			get
			{
				return this.useDirectionalPing;
			}
		}

		// Token: 0x170012D6 RID: 4822
		// (get) Token: 0x06004635 RID: 17973 RVA: 0x001C0976 File Offset: 0x001BEB76
		public IEnumerable<SonarTransducer> ConnectedTransducers
		{
			get
			{
				return from t in this.connectedTransducers
				select t.Transducer;
			}
		}

		// Token: 0x170012D7 RID: 4823
		// (get) Token: 0x06004636 RID: 17974 RVA: 0x001C09A2 File Offset: 0x001BEBA2
		// (set) Token: 0x06004637 RID: 17975 RVA: 0x001C09AC File Offset: 0x001BEBAC
		[Serialize(10000f, IsPropertySaveable.No, "The maximum range of the sonar.", "", false)]
		public float Range
		{
			get
			{
				return this.range;
			}
			set
			{
				this.range = MathHelper.Clamp(value, 0f, 100000f);
				Item item = this.item;
				if (((item != null) ? item.AiTarget : null) != null && this.item.AiTarget.MaxSoundRange <= 0f)
				{
					this.item.AiTarget.MaxSoundRange = this.range;
				}
			}
		}

		// Token: 0x170012D8 RID: 4824
		// (get) Token: 0x06004638 RID: 17976 RVA: 0x001C0A10 File Offset: 0x001BEC10
		// (set) Token: 0x06004639 RID: 17977 RVA: 0x001C0A18 File Offset: 0x001BEC18
		[Serialize(false, IsPropertySaveable.No, "Should the sonar display the walls of the submarine it is inside.", "", false)]
		public bool DetectSubmarineWalls { get; set; }

		// Token: 0x170012D9 RID: 4825
		// (get) Token: 0x0600463A RID: 17978 RVA: 0x001C0A21 File Offset: 0x001BEC21
		// (set) Token: 0x0600463B RID: 17979 RVA: 0x001C0A29 File Offset: 0x001BEC29
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the sonar have to be connected to external transducers to work.", "", false)]
		public bool UseTransducers { get; set; }

		// Token: 0x170012DA RID: 4826
		// (get) Token: 0x0600463C RID: 17980 RVA: 0x001C0A32 File Offset: 0x001BEC32
		// (set) Token: 0x0600463D RID: 17981 RVA: 0x001C0A3A File Offset: 0x001BEC3A
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Should the sonar view be centered on the transducers or the submarine's center of mass. Only has an effect if UseTransducers is enabled.", "", false)]
		public bool CenterOnTransducers { get; set; }

		// Token: 0x170012DB RID: 4827
		// (get) Token: 0x0600463E RID: 17982 RVA: 0x001C0A43 File Offset: 0x001BEC43
		// (set) Token: 0x0600463F RID: 17983 RVA: 0x001C0A4B File Offset: 0x001BEC4B
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the sonar have mineral scanning mode. ", "", false)]
		public bool HasMineralScanner
		{
			get
			{
				return this.hasMineralScanner;
			}
			set
			{
				this.hasMineralScanner = value;
			}
		}

		// Token: 0x170012DC RID: 4828
		// (get) Token: 0x06004640 RID: 17984 RVA: 0x001C0A54 File Offset: 0x001BEC54
		// (set) Token: 0x06004641 RID: 17985 RVA: 0x001C0A5C File Offset: 0x001BEC5C
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool UseMineralScanner { get; set; }

		// Token: 0x170012DD RID: 4829
		// (get) Token: 0x06004642 RID: 17986 RVA: 0x001C0A65 File Offset: 0x001BEC65
		// (set) Token: 0x06004643 RID: 17987 RVA: 0x001C0A6D File Offset: 0x001BEC6D
		public float Zoom
		{
			get
			{
				return this.zoom;
			}
			set
			{
				this.zoom = MathHelper.Clamp(value, 1f, 4f);
			}
		}

		// Token: 0x170012DE RID: 4830
		// (get) Token: 0x06004644 RID: 17988 RVA: 0x001C0A85 File Offset: 0x001BEC85
		// (set) Token: 0x06004645 RID: 17989 RVA: 0x001C0A90 File Offset: 0x001BEC90
		public Sonar.Mode CurrentMode
		{
			get
			{
				return this.currentMode;
			}
			set
			{
				bool changed = this.currentMode != value;
				this.currentMode = value;
			}
		}

		// Token: 0x06004646 RID: 17990 RVA: 0x001C0AB4 File Offset: 0x001BECB4
		public Sonar(Item item, ContentXElement element) : base(item, element)
		{
			this.connectedTransducers = new List<Sonar.ConnectedTransducer>();
			this.IsActive = true;
			this.CurrentMode = Sonar.Mode.Passive;
			Sonar.SonarList.Add(this);
		}

		// Token: 0x06004647 RID: 17991 RVA: 0x001C0B28 File Offset: 0x001BED28
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
			if (this.UseTransducers)
			{
				foreach (Sonar.ConnectedTransducer transducer in this.connectedTransducers)
				{
					transducer.DisconnectTimer -= deltaTime;
				}
				this.connectedTransducers.RemoveAll((Sonar.ConnectedTransducer t) => t.DisconnectTimer <= 0f);
			}
			for (int pingIndex = 0; pingIndex < this.activePingsCount; pingIndex++)
			{
				this.activePings[pingIndex].State += deltaTime * 0.5f;
			}
			if (this.currentMode == Sonar.Mode.Active)
			{
				if (this.HasPower && (!this.UseTransducers || this.connectedTransducers.Count > 0))
				{
					if (this.currentPingIndex != -1)
					{
						Sonar.ActivePing activePing = this.activePings[this.currentPingIndex];
						if (activePing.State > 1f)
						{
							this.aiPingCheckPending = true;
							this.currentPingIndex = -1;
						}
					}
					if (this.currentPingIndex == -1 && this.activePingsCount < this.activePings.Length)
					{
						int num = this.activePingsCount;
						this.activePingsCount = num + 1;
						this.currentPingIndex = num;
						if (this.activePings[this.currentPingIndex] == null)
						{
							this.activePings[this.currentPingIndex] = new Sonar.ActivePing();
						}
						this.activePings[this.currentPingIndex].IsDirectional = this.useDirectionalPing;
						this.activePings[this.currentPingIndex].Direction = this.pingDirection;
						this.activePings[this.currentPingIndex].State = 0f;
						this.activePings[this.currentPingIndex].PrevPingRadius = 0f;
						foreach (AITarget aiTarget in this.GetAITargets())
						{
							aiTarget.SectorDegrees = (this.useDirectionalPing ? 30f : 360f);
							aiTarget.SectorDir = new Vector2(this.pingDirection.X, -this.pingDirection.Y);
						}
						this.item.Use(deltaTime, null, null, null, null);
					}
				}
				else
				{
					this.aiPingCheckPending = false;
				}
			}
			int pingIndex2 = 0;
			while (pingIndex2 < this.activePingsCount)
			{
				foreach (AITarget aiTarget2 in this.GetAITargets())
				{
					float range = MathUtils.InverseLerp(aiTarget2.MinSoundRange, aiTarget2.MaxSoundRange, this.Range * this.activePings[pingIndex2].State / this.zoom);
					aiTarget2.SoundRange = Math.Max(aiTarget2.SoundRange, MathHelper.Lerp(aiTarget2.MinSoundRange, aiTarget2.MaxSoundRange, range));
				}
				if (this.activePings[pingIndex2].State > 1f)
				{
					int num = this.activePingsCount - 1;
					this.activePingsCount = num;
					int lastIndex = num;
					Sonar.ActivePing oldActivePing = this.activePings[pingIndex2];
					this.activePings[pingIndex2] = this.activePings[lastIndex];
					this.activePings[lastIndex] = oldActivePing;
					if (this.currentPingIndex == lastIndex)
					{
						this.currentPingIndex = pingIndex2;
					}
				}
				else
				{
					pingIndex2++;
				}
			}
		}

		// Token: 0x06004648 RID: 17992 RVA: 0x001C0EAC File Offset: 0x001BF0AC
		private IEnumerable<AITarget> GetAITargets()
		{
			Sonar.<GetAITargets>d__58 <GetAITargets>d__ = new Sonar.<GetAITargets>d__58(-2);
			<GetAITargets>d__.<>4__this = this;
			return <GetAITargets>d__;
		}

		// Token: 0x06004649 RID: 17993 RVA: 0x001C0EBC File Offset: 0x001BF0BC
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			if (this.currentMode != Sonar.Mode.Active)
			{
				return this.powerConsumption * 0.1f;
			}
			return this.powerConsumption;
		}

		// Token: 0x0600464A RID: 17994 RVA: 0x001C0EF0 File Offset: 0x001BF0F0
		public override bool Use(float deltaTime, Character character = null)
		{
			return this.currentPingIndex != -1 && (character == null || this.characterUsable);
		}

		// Token: 0x0600464B RID: 17995 RVA: 0x001C0F08 File Offset: 0x001BF108
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			if (this.currentMode == Sonar.Mode.Passive || !this.aiPingCheckPending)
			{
				return false;
			}
			foreach (List<Character> targetGroup in Sonar.targetGroups.Values)
			{
				targetGroup.Clear();
			}
			foreach (Character c in Character.CharacterList)
			{
				if (!c.IsDead && !c.Removed && c.Enabled && c.AnimController.CurrentHull == null && !c.Params.HideInSonar && (!this.DetectSubmarineWalls || c.AnimController.CurrentHull != null || this.item.CurrentHull == null) && Vector2.DistanceSquared(c.WorldPosition, this.item.WorldPosition) <= this.range * this.range)
				{
					string directionName = this.GetDirectionName(c.WorldPosition - this.item.WorldPosition).Value;
					if (!Sonar.targetGroups.ContainsKey(directionName))
					{
						Sonar.targetGroups.Add(directionName, new List<Character>());
					}
					Sonar.targetGroups[directionName].Add(c);
				}
			}
			foreach (KeyValuePair<string, List<Character>> targetGroup2 in Sonar.targetGroups)
			{
				if (targetGroup2.Value.Any<Character>())
				{
					string dialogTag = "DialogSonarTarget";
					if (targetGroup2.Value.Count > 1)
					{
						dialogTag = "DialogSonarTargetMultiple";
					}
					else if (targetGroup2.Value[0].Mass > 100f)
					{
						dialogTag = "DialogSonarTargetLarge";
					}
					if (character.IsOnPlayerTeam)
					{
						string value = TextManager.GetWithVariables(dialogTag, new ValueTuple<string, string, FormatCapitals>[]
						{
							new ValueTuple<string, string, FormatCapitals>("[direction]", targetGroup2.Key.ToString(), FormatCapitals.Yes),
							new ValueTuple<string, string, FormatCapitals>("[count]", targetGroup2.Value.Count.ToString(), FormatCapitals.No)
						}).Value;
						ChatMessageType? messageType = null;
						float delay = 0f;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler.AppendLiteral("sonartarget");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(targetGroup2.Value[0].ID);
						character.Speak(value, messageType, delay, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), 60f);
					}
					for (int i = 1; i < targetGroup2.Value.Count; i++)
					{
						character.DisableLine("sonartarget" + targetGroup2.Value[i].ID.ToString());
					}
				}
			}
			return true;
		}

		// Token: 0x0600464C RID: 17996 RVA: 0x001C1240 File Offset: 0x001BF440
		private LocalizedString GetDirectionName(Vector2 dir)
		{
			float angle = MathUtils.WrapAngleTwoPi((float)(-(float)Math.Atan2((double)dir.Y, (double)dir.X)) + 1.5707964f);
			int clockDir = (int)Math.Round((double)(angle / 6.2831855f * 12f));
			if (clockDir == 0)
			{
				clockDir = 12;
			}
			return TextManager.GetWithVariable("roomname.subdiroclock", "[dir]", clockDir.ToString(), FormatCapitals.No);
		}

		// Token: 0x0600464D RID: 17997 RVA: 0x001C12A8 File Offset: 0x001BF4A8
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			base.ReceiveSignal(signal, connection);
			if (connection.Name == "transducer_in")
			{
				SonarTransducer transducer = signal.source.GetComponent<SonarTransducer>();
				if (transducer == null)
				{
					return;
				}
				transducer.ConnectedSonar = this;
				Sonar.ConnectedTransducer connectedTransducer = this.connectedTransducers.Find((Sonar.ConnectedTransducer t) => t.Transducer == transducer);
				if (connectedTransducer == null)
				{
					this.connectedTransducers.Add(new Sonar.ConnectedTransducer(transducer, signal.strength, 1f));
					return;
				}
				connectedTransducer.SignalStrength = signal.strength;
				connectedTransducer.DisconnectTimer = 1f;
			}
		}

		// Token: 0x0600464E RID: 17998 RVA: 0x001C1352 File Offset: 0x001BF552
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sonar.SonarList.Remove(this);
		}

		// Token: 0x0600464F RID: 17999 RVA: 0x001C1368 File Offset: 0x001BF568
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool isActive = msg.ReadBoolean();
			bool directionalPing = this.useDirectionalPing;
			float zoomT = this.zoom;
			float pingDirectionT = 0f;
			bool mineralScanner = this.UseMineralScanner;
			if (isActive)
			{
				zoomT = msg.ReadRangedSingle(0f, 1f, 8);
				directionalPing = msg.ReadBoolean();
				if (directionalPing)
				{
					pingDirectionT = msg.ReadRangedSingle(0f, 1f, 8);
				}
				mineralScanner = msg.ReadBoolean();
			}
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			this.CurrentMode = (isActive ? Sonar.Mode.Active : Sonar.Mode.Passive);
			if (isActive)
			{
				this.zoom = MathHelper.Lerp(1f, 4f, zoomT);
				this.useDirectionalPing = directionalPing;
				if (this.useDirectionalPing)
				{
					float pingAngle = MathHelper.Lerp(0f, 6.2831855f, pingDirectionT);
					this.pingDirection = new Vector2((float)Math.Cos((double)pingAngle), (float)Math.Sin((double)pingAngle));
				}
				this.UseMineralScanner = mineralScanner;
			}
			this.item.CreateServerEvent<Sonar>(this);
		}

		// Token: 0x06004650 RID: 18000 RVA: 0x001C1458 File Offset: 0x001BF658
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.currentMode == Sonar.Mode.Active);
			if (this.currentMode == Sonar.Mode.Active)
			{
				msg.WriteRangedSingle(this.zoom, 1f, 4f, 8);
				msg.WriteBoolean(this.useDirectionalPing);
				if (this.useDirectionalPing)
				{
					float pingAngle = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(this.pingDirection));
					msg.WriteRangedSingle(MathUtils.InverseLerp(0f, 6.2831855f, pingAngle), 0f, 1f, 8);
				}
				msg.WriteBoolean(this.UseMineralScanner);
			}
		}

		// Token: 0x040021B9 RID: 8633
		public static List<Sonar> SonarList = new List<Sonar>();

		// Token: 0x040021BA RID: 8634
		public const float DefaultSonarRange = 10000f;

		// Token: 0x040021BB RID: 8635
		public const float PassivePowerConsumption = 0.1f;

		// Token: 0x040021BC RID: 8636
		private const float DirectionalPingSector = 30f;

		// Token: 0x040021BD RID: 8637
		private static readonly float DirectionalPingDotProduct;

		// Token: 0x040021BE RID: 8638
		private float range;

		// Token: 0x040021BF RID: 8639
		private const float PingFrequency = 0.5f;

		// Token: 0x040021C0 RID: 8640
		private Sonar.Mode currentMode = Sonar.Mode.Passive;

		// Token: 0x040021C1 RID: 8641
		private Sonar.ActivePing[] activePings = new Sonar.ActivePing[8];

		// Token: 0x040021C2 RID: 8642
		private int activePingsCount;

		// Token: 0x040021C3 RID: 8643
		private int currentPingIndex = -1;

		// Token: 0x040021C4 RID: 8644
		private const float MinZoom = 1f;

		// Token: 0x040021C5 RID: 8645
		private const float MaxZoom = 4f;

		// Token: 0x040021C6 RID: 8646
		private float zoom = 1f;

		// Token: 0x040021C7 RID: 8647
		private bool useDirectionalPing;

		// Token: 0x040021C8 RID: 8648
		private Vector2 pingDirection = new Vector2(1f, 0f);

		// Token: 0x040021C9 RID: 8649
		private bool aiPingCheckPending;

		// Token: 0x040021CA RID: 8650
		private readonly List<Sonar.ConnectedTransducer> connectedTransducers;

		// Token: 0x040021CE RID: 8654
		private bool hasMineralScanner;

		// Token: 0x040021D0 RID: 8656
		private static readonly Dictionary<string, List<Character>> targetGroups = new Dictionary<string, List<Character>>();

		// Token: 0x02000E23 RID: 3619
		public enum Mode
		{
			// Token: 0x040041D6 RID: 16854
			Active,
			// Token: 0x040041D7 RID: 16855
			Passive
		}

		// Token: 0x02000E24 RID: 3620
		private class ConnectedTransducer
		{
			// Token: 0x06006991 RID: 27025 RVA: 0x00224DAB File Offset: 0x00222FAB
			public ConnectedTransducer(SonarTransducer transducer, float signalStrength, float disconnectTimer)
			{
				this.Transducer = transducer;
				this.SignalStrength = signalStrength;
				this.DisconnectTimer = disconnectTimer;
			}

			// Token: 0x040041D8 RID: 16856
			public readonly SonarTransducer Transducer;

			// Token: 0x040041D9 RID: 16857
			public float SignalStrength;

			// Token: 0x040041DA RID: 16858
			public float DisconnectTimer;
		}

		// Token: 0x02000E25 RID: 3621
		private class ActivePing
		{
			// Token: 0x040041DB RID: 16859
			public float State;

			// Token: 0x040041DC RID: 16860
			public bool IsDirectional;

			// Token: 0x040041DD RID: 16861
			public Vector2 Direction;

			// Token: 0x040041DE RID: 16862
			public float PrevPingRadius;
		}
	}
}
