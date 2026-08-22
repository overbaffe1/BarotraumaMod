using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000612 RID: 1554
	internal class EqualsComponent : ItemComponent
	{
		// Token: 0x17001956 RID: 6486
		// (get) Token: 0x06006421 RID: 25633 RVA: 0x0033FA53 File Offset: 0x0033DC53
		// (set) Token: 0x06006422 RID: 25634 RVA: 0x0033FA5B File Offset: 0x0033DC5B
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

		// Token: 0x17001957 RID: 6487
		// (get) Token: 0x06006423 RID: 25635 RVA: 0x0033FA6A File Offset: 0x0033DC6A
		// (set) Token: 0x06006424 RID: 25636 RVA: 0x0033FA74 File Offset: 0x0033DC74
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

		// Token: 0x17001958 RID: 6488
		// (get) Token: 0x06006425 RID: 25637 RVA: 0x0033FAD6 File Offset: 0x0033DCD6
		// (set) Token: 0x06006426 RID: 25638 RVA: 0x0033FAE0 File Offset: 0x0033DCE0
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

		// Token: 0x17001959 RID: 6489
		// (get) Token: 0x06006427 RID: 25639 RVA: 0x0033FB42 File Offset: 0x0033DD42
		// (set) Token: 0x06006428 RID: 25640 RVA: 0x0033FB4C File Offset: 0x0033DD4C
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

		// Token: 0x06006429 RID: 25641 RVA: 0x0033FB98 File Offset: 0x0033DD98
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

		// Token: 0x0600642A RID: 25642 RVA: 0x0033FC0C File Offset: 0x0033DE0C
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

		// Token: 0x0600642B RID: 25643 RVA: 0x0033FCBC File Offset: 0x0033DEBC
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

		// Token: 0x040033F0 RID: 13296
		protected string output;

		// Token: 0x040033F1 RID: 13297
		protected string falseOutput;

		// Token: 0x040033F2 RID: 13298
		protected float[] timeSinceReceived;

		// Token: 0x040033F3 RID: 13299
		protected string[] receivedSignal;

		// Token: 0x040033F4 RID: 13300
		private readonly Character[] signalSender = new Character[2];

		// Token: 0x040033F5 RID: 13301
		protected float timeFrame;

		// Token: 0x040033F6 RID: 13302
		private int maxOutputLength;
	}
}
