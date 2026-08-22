using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A2 RID: 1186
	[NullableContext(1)]
	[Nullable(0)]
	internal class PowerDistributor : PowerTransfer, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x060041C1 RID: 16833 RVA: 0x001A6238 File Offset: 0x001A4438
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			PowerDistributor.EventType eventType;
			PowerDistributor.PowerGroup powerGroup;
			string newName;
			float newRatio;
			this.SharedEventRead(msg, out eventType, out powerGroup, out newName, out newRatio);
			if (this.item.CanClientAccess(c))
			{
				if (eventType != PowerDistributor.EventType.NameChange)
				{
					if (eventType == PowerDistributor.EventType.RatioChange)
					{
						powerGroup.SupplyRatio = newRatio;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 3);
						defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(c.Character));
						defaultInterpolatedStringHandler.AppendLiteral(" changed supply ratio of power group \"");
						defaultInterpolatedStringHandler.AppendFormatted(powerGroup.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
						defaultInterpolatedStringHandler.AppendFormatted<float>(powerGroup.SupplyRatio);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.ItemInteraction);
					}
				}
				else
				{
					powerGroup.Name = newName;
				}
			}
			this.item.CreateServerEvent<PowerDistributor>(this, new PowerDistributor.EventData(powerGroup, eventType));
		}

		// Token: 0x060041C2 RID: 16834 RVA: 0x001A62FD File Offset: 0x001A44FD
		public void ServerEventWrite(IWriteMessage msg, Client c, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			this.SharedEventWrite(msg, extraData);
		}

		// Token: 0x17001184 RID: 4484
		// (get) Token: 0x060041C3 RID: 16835 RVA: 0x001A6307 File Offset: 0x001A4507
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Relay;
			}
		}

		// Token: 0x060041C4 RID: 16836 RVA: 0x001A630A File Offset: 0x001A450A
		public PowerDistributor(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060041C5 RID: 16837 RVA: 0x001A632C File Offset: 0x001A452C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			IEnumerable<Connection> ratioInputs = from conn in base.Item.Connections
			where !conn.IsOutput && conn.Name.StartsWith("set_supply_ratio")
			select conn;
			IEnumerable<Connection> ratioOutputs = from conn in base.Item.Connections
			where conn.IsOutput && conn.Name.StartsWith("supply_ratio_out")
			select conn;
			for (int i = 0; i < this.powerOuts.Count; i++)
			{
				new PowerDistributor.PowerGroup(this, this.powerOuts[i], this.cachedGroupData.ElementAtOrDefault(i), ratioInputs.ElementAtOrDefault(i), ratioOutputs.ElementAtOrDefault(i));
			}
			this.cachedGroupData.Clear();
		}

		// Token: 0x060041C6 RID: 16838 RVA: 0x001A63F0 File Offset: 0x001A45F0
		public override void Clone(ItemComponent original)
		{
			PowerDistributor originalPowerDistributor = original as PowerDistributor;
			if (originalPowerDistributor == null)
			{
				return;
			}
			for (int i = 0; i < this.powerOuts.Count; i++)
			{
				this.powerGroups[i].SupplyRatio = originalPowerDistributor.powerGroups[i].SupplyRatio;
				this.powerGroups[i].Name = originalPowerDistributor.powerGroups[i].Name;
			}
		}

		// Token: 0x060041C7 RID: 16839 RVA: 0x001A6464 File Offset: 0x001A4664
		protected override void SendSignals()
		{
			Item item = this.item;
			GridInfo grid = this.powerIn.Grid;
			item.SendSignal(MathUtils.RoundToInt((grid != null) ? grid.Power : 0f).ToString(), "power_value_out");
			this.item.SendSignal(MathUtils.RoundToInt(this.GetCurrentPowerConsumption(this.powerIn)).ToString(), "load_value_out");
			this.powerGroups.ForEach(delegate(PowerDistributor.PowerGroup group)
			{
				group.SendRatioSignal();
			});
		}

		// Token: 0x060041C8 RID: 16840 RVA: 0x001A64FC File Offset: 0x001A46FC
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.item.Condition <= 0f || connection.IsPower)
			{
				return;
			}
			if (connection.IsOutput)
			{
				return;
			}
			PowerDistributor.PowerGroup powerGroup = this.powerGroups.FirstOrDefault((PowerDistributor.PowerGroup group) => group.RatioInput == connection);
			if (powerGroup == null)
			{
				return;
			}
			powerGroup.ReceiveRatioSignal(signal);
		}

		// Token: 0x060041C9 RID: 16841 RVA: 0x001A6566 File Offset: 0x001A4766
		private bool IsShortCircuited(Connection conn)
		{
			return this.powerIn.Grid == conn.Grid;
		}

		// Token: 0x060041CA RID: 16842 RVA: 0x001A657B File Offset: 0x001A477B
		[NullableContext(2)]
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn)
			{
				return -1f;
			}
			if (this.isBroken)
			{
				return 0f;
			}
			return this.powerGroups.Sum(delegate(PowerDistributor.PowerGroup group)
			{
				if (!this.IsShortCircuited(group.PowerOut))
				{
					return group.ModifiedLoad;
				}
				return 0f;
			}) + base.ExtraLoad;
		}

		// Token: 0x060041CB RID: 16843 RVA: 0x001A65B8 File Offset: 0x001A47B8
		private float CalculatePowerOut(PowerDistributor.PowerGroup group)
		{
			if (this.isBroken || this.powerIn.Grid == null || this.IsShortCircuited(group.PowerOut))
			{
				return 0f;
			}
			return Math.Max(group.ModifiedLoad * base.Voltage, 0f);
		}

		// Token: 0x060041CC RID: 16844 RVA: 0x001A6608 File Offset: 0x001A4808
		public override float GetConnectionPowerOut(Connection connection, float power, PowerRange minMaxPower, float load)
		{
			if (connection == this.powerIn)
			{
				return 0f;
			}
			PowerDistributor.PowerGroup group2 = this.powerGroups.First((PowerDistributor.PowerGroup group) => group.PowerOut == connection);
			group2.Load = load;
			return this.CalculatePowerOut(group2);
		}

		// Token: 0x060041CD RID: 16845 RVA: 0x001A6660 File Offset: 0x001A4860
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			foreach (PowerDistributor.PowerGroup powerGroup in this.powerGroups)
			{
				componentElement.Add(new XElement("PowerGroup", new object[]
				{
					new XAttribute("name", powerGroup.Name),
					new XAttribute("ratio", powerGroup.SupplyRatio)
				}));
			}
			return componentElement;
		}

		// Token: 0x060041CE RID: 16846 RVA: 0x001A6708 File Offset: 0x001A4908
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (usePrefabValues)
			{
				return;
			}
			foreach (ContentXElement cxe in componentElement.Elements())
			{
				XElement element = cxe;
				this.cachedGroupData.Add(element);
			}
		}

		// Token: 0x060041CF RID: 16847 RVA: 0x001A6770 File Offset: 0x001A4970
		private void SharedEventWrite(IWriteMessage msg, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			PowerDistributor.EventData data = base.ExtractEventData<PowerDistributor.EventData>(extraData);
			msg.WriteRangedInteger((int)data.EventType, 0, 1);
			msg.WriteRangedInteger(this.powerGroups.IndexOf(data.PowerGroup), 0, this.powerGroups.Count - 1);
			PowerDistributor.EventType eventType = data.EventType;
			if (eventType == PowerDistributor.EventType.NameChange)
			{
				msg.WriteString(data.PowerGroup.Name);
				return;
			}
			if (eventType != PowerDistributor.EventType.RatioChange)
			{
				return;
			}
			msg.WriteRangedInteger(MathUtils.RoundToInt(data.PowerGroup.SupplyRatio / 0.05f), 0, 20);
		}

		// Token: 0x060041D0 RID: 16848 RVA: 0x001A6800 File Offset: 0x001A4A00
		private void SharedEventRead(IReadMessage msg, out PowerDistributor.EventType eventType, out PowerDistributor.PowerGroup powerGroup, out string newName, out float newRatio)
		{
			eventType = (PowerDistributor.EventType)msg.ReadRangedInteger(0, 1);
			powerGroup = this.powerGroups[msg.ReadRangedInteger(0, this.powerGroups.Count - 1)];
			newName = ((eventType == PowerDistributor.EventType.NameChange) ? string.Concat<char>(msg.ReadString().Take(32)) : powerGroup.Name);
			newRatio = ((eventType == PowerDistributor.EventType.RatioChange) ? ((float)msg.ReadRangedInteger(0, 20) * 0.05f) : powerGroup.SupplyRatio);
		}

		// Token: 0x04001F95 RID: 8085
		private const int MaxNameLength = 32;

		// Token: 0x04001F96 RID: 8086
		private const int SupplyRatioSteps = 20;

		// Token: 0x04001F97 RID: 8087
		private const float SupplyRatioStep = 0.05f;

		// Token: 0x04001F98 RID: 8088
		private readonly List<PowerDistributor.PowerGroup> powerGroups = new List<PowerDistributor.PowerGroup>();

		// Token: 0x04001F99 RID: 8089
		private readonly List<XElement> cachedGroupData = new List<XElement>();

		// Token: 0x02000DBC RID: 3516
		[Nullable(0)]
		private class PowerGroup
		{
			// Token: 0x17001671 RID: 5745
			// (get) Token: 0x06006826 RID: 26662 RVA: 0x00221F63 File Offset: 0x00220163
			// (set) Token: 0x06006827 RID: 26663 RVA: 0x00221F6B File Offset: 0x0022016B
			public string Name
			{
				get
				{
					return this.name;
				}
				set
				{
					this.name = value;
					this.DisplayName = TextManager.Get(this.name).Fallback(this.name, true);
				}
			}

			// Token: 0x17001672 RID: 5746
			// (get) Token: 0x06006828 RID: 26664 RVA: 0x00221F96 File Offset: 0x00220196
			// (set) Token: 0x06006829 RID: 26665 RVA: 0x00221F9E File Offset: 0x0022019E
			public LocalizedString DisplayName { get; private set; }

			// Token: 0x17001673 RID: 5747
			// (get) Token: 0x0600682A RID: 26666 RVA: 0x00221FA7 File Offset: 0x002201A7
			// (set) Token: 0x0600682B RID: 26667 RVA: 0x00221FAF File Offset: 0x002201AF
			public float SupplyRatio
			{
				get
				{
					return this.supplyRatio;
				}
				set
				{
					if (!MathUtils.IsValid(value))
					{
						return;
					}
					this.supplyRatio = MathUtils.RoundTowardsClosest(MathHelper.Clamp(value, 0f, 1f), 0.05f);
				}
			}

			// Token: 0x17001674 RID: 5748
			// (get) Token: 0x0600682C RID: 26668 RVA: 0x00221FDA File Offset: 0x002201DA
			// (set) Token: 0x0600682D RID: 26669 RVA: 0x00221FEE File Offset: 0x002201EE
			public float DisplayRatio
			{
				get
				{
					return (float)MathUtils.RoundToInt(this.supplyRatio * 100f);
				}
				set
				{
					this.SupplyRatio = value / 100f;
				}
			}

			// Token: 0x17001675 RID: 5749
			// (get) Token: 0x0600682E RID: 26670 RVA: 0x00221FFD File Offset: 0x002201FD
			public float ModifiedLoad
			{
				get
				{
					return this.Load * this.SupplyRatio;
				}
			}

			// Token: 0x0600682F RID: 26671 RVA: 0x0022200C File Offset: 0x0022020C
			[NullableContext(2)]
			public PowerGroup([Nullable(1)] PowerDistributor distributor, [Nullable(1)] Connection power, XElement element = null, Connection ratioInput = null, Connection ratioOutput = null)
			{
				this.distributor = distributor;
				this.PowerOut = power;
				this.RatioInput = ratioInput;
				this.RatioOutput = ratioOutput;
				distributor.powerGroups.Add(this);
				this.name = TextManager.GetWithVariable("groupx", "[num]", distributor.powerGroups.Count.ToString(), FormatCapitals.No).Value;
				this.SupplyRatio = 1f;
				if (element != null)
				{
					this.name = element.GetAttributeString("name", this.name);
					this.SupplyRatio = element.GetAttributeFloat("ratio", this.SupplyRatio);
				}
				this.DisplayName = TextManager.Get(this.name).Fallback(this.name, true);
			}

			// Token: 0x06006830 RID: 26672 RVA: 0x002220E8 File Offset: 0x002202E8
			public void ReceiveRatioSignal(Signal signal)
			{
				float receivedSignal;
				if (!float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal) || !MathUtils.IsValid(receivedSignal))
				{
					return;
				}
				this.DisplayRatio = receivedSignal;
			}

			// Token: 0x06006831 RID: 26673 RVA: 0x00222120 File Offset: 0x00220320
			public void SendRatioSignal()
			{
				this.distributor.item.SendSignal(new Signal(this.DisplayRatio.ToString(), 0, null, null, 0f, 1f), this.RatioOutput);
			}

			// Token: 0x0400408A RID: 16522
			private readonly PowerDistributor distributor;

			// Token: 0x0400408B RID: 16523
			public readonly Connection PowerOut;

			// Token: 0x0400408C RID: 16524
			[Nullable(2)]
			public readonly Connection RatioInput;

			// Token: 0x0400408D RID: 16525
			[Nullable(2)]
			public readonly Connection RatioOutput;

			// Token: 0x0400408E RID: 16526
			private string name;

			// Token: 0x04004090 RID: 16528
			private float supplyRatio = 1f;

			// Token: 0x04004091 RID: 16529
			public float Load;
		}

		// Token: 0x02000DBD RID: 3517
		[NullableContext(0)]
		private enum EventType
		{
			// Token: 0x04004093 RID: 16531
			NameChange,
			// Token: 0x04004094 RID: 16532
			RatioChange
		}

		// Token: 0x02000DBE RID: 3518
		[NullableContext(0)]
		private readonly struct EventData : ItemComponent.IEventData, IEquatable<PowerDistributor.EventData>
		{
			// Token: 0x06006832 RID: 26674 RVA: 0x00222163 File Offset: 0x00220363
			[NullableContext(1)]
			public EventData(PowerDistributor.PowerGroup PowerGroup, PowerDistributor.EventType EventType)
			{
				this.PowerGroup = PowerGroup;
				this.EventType = EventType;
			}

			// Token: 0x17001676 RID: 5750
			// (get) Token: 0x06006833 RID: 26675 RVA: 0x00222173 File Offset: 0x00220373
			// (set) Token: 0x06006834 RID: 26676 RVA: 0x0022217B File Offset: 0x0022037B
			[Nullable(1)]
			public PowerDistributor.PowerGroup PowerGroup { [NullableContext(1)] get; [NullableContext(1)] set; }

			// Token: 0x17001677 RID: 5751
			// (get) Token: 0x06006835 RID: 26677 RVA: 0x00222184 File Offset: 0x00220384
			// (set) Token: 0x06006836 RID: 26678 RVA: 0x0022218C File Offset: 0x0022038C
			public PowerDistributor.EventType EventType { get; set; }

			// Token: 0x06006837 RID: 26679 RVA: 0x00222198 File Offset: 0x00220398
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("EventData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006838 RID: 26680 RVA: 0x002221E4 File Offset: 0x002203E4
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("PowerGroup = ");
				builder.Append(this.PowerGroup);
				builder.Append(", EventType = ");
				builder.Append(this.EventType.ToString());
				return true;
			}

			// Token: 0x06006839 RID: 26681 RVA: 0x00222232 File Offset: 0x00220432
			[CompilerGenerated]
			public static bool operator !=(PowerDistributor.EventData left, PowerDistributor.EventData right)
			{
				return !(left == right);
			}

			// Token: 0x0600683A RID: 26682 RVA: 0x0022223E File Offset: 0x0022043E
			[CompilerGenerated]
			public static bool operator ==(PowerDistributor.EventData left, PowerDistributor.EventData right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600683B RID: 26683 RVA: 0x00222248 File Offset: 0x00220448
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<PowerDistributor.PowerGroup>.Default.GetHashCode(this.<PowerGroup>k__BackingField) * -1521134295 + EqualityComparer<PowerDistributor.EventType>.Default.GetHashCode(this.<EventType>k__BackingField);
			}

			// Token: 0x0600683C RID: 26684 RVA: 0x00222271 File Offset: 0x00220471
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is PowerDistributor.EventData && this.Equals((PowerDistributor.EventData)obj);
			}

			// Token: 0x0600683D RID: 26685 RVA: 0x00222289 File Offset: 0x00220489
			[CompilerGenerated]
			public bool Equals(PowerDistributor.EventData other)
			{
				return EqualityComparer<PowerDistributor.PowerGroup>.Default.Equals(this.<PowerGroup>k__BackingField, other.<PowerGroup>k__BackingField) && EqualityComparer<PowerDistributor.EventType>.Default.Equals(this.<EventType>k__BackingField, other.<EventType>k__BackingField);
			}

			// Token: 0x0600683E RID: 26686 RVA: 0x002222BB File Offset: 0x002204BB
			[NullableContext(1)]
			[CompilerGenerated]
			public void Deconstruct(out PowerDistributor.PowerGroup PowerGroup, out PowerDistributor.EventType EventType)
			{
				PowerGroup = this.PowerGroup;
				EventType = this.EventType;
			}
		}
	}
}
