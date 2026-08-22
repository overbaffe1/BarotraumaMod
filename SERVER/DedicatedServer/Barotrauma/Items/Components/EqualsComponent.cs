using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004EE RID: 1262
	internal class EqualsComponent : ItemComponent
	{
		// Token: 0x17001319 RID: 4889
		// (get) Token: 0x06004730 RID: 18224 RVA: 0x001C621F File Offset: 0x001C441F
		// (set) Token: 0x06004731 RID: 18225 RVA: 0x001C6227 File Offset: 0x001C4427
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the output strings. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x1700131A RID: 4890
		// (get) Token: 0x06004732 RID: 18226 RVA: 0x001C6236 File Offset: 0x001C4436
		// (set) Token: 0x06004733 RID: 18227 RVA: 0x001C6240 File Offset: 0x001C4440
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal sent when the condition is met.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.output = value;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x1700131B RID: 4891
		// (get) Token: 0x06004734 RID: 18228 RVA: 0x001C62A2 File Offset: 0x001C44A2
		// (set) Token: 0x06004735 RID: 18229 RVA: 0x001C62AC File Offset: 0x001C44AC
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal sent when the condition is met (if empty, no signal is sent).", "", true)]
		public string FalseOutput
		{
			get
			{
				return this.falseOutput;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.falseOutput = value;
				if (this.falseOutput.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.falseOutput = this.falseOutput.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x1700131C RID: 4892
		// (get) Token: 0x06004736 RID: 18230 RVA: 0x001C630E File Offset: 0x001C450E
		// (set) Token: 0x06004737 RID: 18231 RVA: 0x001C6318 File Offset: 0x001C4518
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The maximum amount of time between the received signals. If set to 0, the signals must be received at the same time.", "", true)]
		public float TimeFrame
		{
			get
			{
				return this.timeFrame;
			}
			set
			{
				if (value > this.timeFrame)
				{
					this.timeSinceReceived[0] = (this.timeSinceReceived[1] = Math.Max(value * 2f, 0.1f));
				}
				this.timeFrame = Math.Max(0f, value);
			}
		}

		// Token: 0x06004738 RID: 18232 RVA: 0x001C6364 File Offset: 0x001C4564
		public EqualsComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.receivedSignal = new string[2];
			this.IsActive = true;
		}

		// Token: 0x06004739 RID: 18233 RVA: 0x001C63D8 File Offset: 0x001C45D8
		public override void Update(float deltaTime, Camera cam)
		{
			bool sendOutput = false;
			for (int i = 0; i < this.timeSinceReceived.Length; i++)
			{
				if (this.timeSinceReceived[i] <= this.timeFrame)
				{
					sendOutput = true;
				}
				this.timeSinceReceived[i] += deltaTime;
			}
			if (sendOutput)
			{
				string signalOut = (this.receivedSignal[0] == this.receivedSignal[1]) ? this.output : this.falseOutput;
				if (string.IsNullOrEmpty(signalOut))
				{
					return;
				}
				this.item.SendSignal(new Signal(signalOut, 0, this.signalSender[0] ?? this.signalSender[1], null, 0f, 1f), "signal_out");
			}
		}

		// Token: 0x0600473A RID: 18234 RVA: 0x001C6488 File Offset: 0x001C4688
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				this.receivedSignal[0] = signal.value;
				this.timeSinceReceived[0] = 0f;
				this.signalSender[0] = signal.sender;
				return;
			}
			if (name == "signal_in2")
			{
				this.receivedSignal[1] = signal.value;
				this.timeSinceReceived[1] = 0f;
				this.signalSender[1] = signal.sender;
				return;
			}
			if (!(name == "set_output"))
			{
				return;
			}
			this.output = signal.value;
		}

		// Token: 0x0400225B RID: 8795
		protected string output;

		// Token: 0x0400225C RID: 8796
		protected string falseOutput;

		// Token: 0x0400225D RID: 8797
		protected float[] timeSinceReceived;

		// Token: 0x0400225E RID: 8798
		protected string[] receivedSignal;

		// Token: 0x0400225F RID: 8799
		private readonly Character[] signalSender = new Character[2];

		// Token: 0x04002260 RID: 8800
		protected float timeFrame;

		// Token: 0x04002261 RID: 8801
		private int maxOutputLength;
	}
}
