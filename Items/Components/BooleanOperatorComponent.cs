using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060A RID: 1546
	internal abstract class BooleanOperatorComponent : ItemComponent
	{
		// Token: 0x1700194A RID: 6474
		// (get) Token: 0x060063F2 RID: 25586 RVA: 0x0033EFA7 File Offset: 0x0033D1A7
		// (set) Token: 0x060063F3 RID: 25587 RVA: 0x0033EFB0 File Offset: 0x0033D1B0
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The item sends the output if both inputs have received a non-zero signal within the timeframe. If set to 0, the inputs must receive a signal at the same time.", "", true)]
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

		// Token: 0x1700194B RID: 6475
		// (get) Token: 0x060063F4 RID: 25588 RVA: 0x0033EFFB File Offset: 0x0033D1FB
		// (set) Token: 0x060063F5 RID: 25589 RVA: 0x0033F003 File Offset: 0x0033D203
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

		// Token: 0x1700194C RID: 6476
		// (get) Token: 0x060063F6 RID: 25590 RVA: 0x0033F012 File Offset: 0x0033D212
		// (set) Token: 0x060063F7 RID: 25591 RVA: 0x0033F01C File Offset: 0x0033D21C
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
				this.IsActive = true;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x1700194D RID: 6477
		// (get) Token: 0x060063F8 RID: 25592 RVA: 0x0033F085 File Offset: 0x0033D285
		// (set) Token: 0x060063F9 RID: 25593 RVA: 0x0033F090 File Offset: 0x0033D290
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
				this.IsActive = true;
				if (this.falseOutput.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.falseOutput = this.falseOutput.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x060063FA RID: 25594 RVA: 0x0033F0FC File Offset: 0x0033D2FC
		public BooleanOperatorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.IsActive = true;
		}

		// Token: 0x060063FB RID: 25595
		protected abstract bool GetOutput(int numTrueInputs);

		// Token: 0x060063FC RID: 25596 RVA: 0x0033F164 File Offset: 0x0033D364
		public sealed override void Update(float deltaTime, Camera cam)
		{
			int receivedInputs = 0;
			bool allInputsTimedOut = true;
			for (int i = 0; i < this.timeSinceReceived.Length; i++)
			{
				if (this.timeSinceReceived[i] <= this.timeFrame)
				{
					allInputsTimedOut = false;
					receivedInputs++;
				}
				this.timeSinceReceived[i] += deltaTime;
			}
			bool state = this.GetOutput(receivedInputs);
			string signalOut = state ? this.output : this.falseOutput;
			if (string.IsNullOrEmpty(signalOut))
			{
				if (!state && allInputsTimedOut)
				{
					this.IsActive = false;
				}
				return;
			}
			this.item.SendSignal(new Signal(signalOut, 0, this.signalSender[0] ?? this.signalSender[1], null, 0f, 1f), "signal_out");
		}

		// Token: 0x060063FD RID: 25597 RVA: 0x0033F220 File Offset: 0x0033D420
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "signal_in1"))
			{
				if (!(name == "signal_in2"))
				{
					if (!(name == "set_output"))
					{
						return;
					}
					this.output = signal.value;
					return;
				}
				else
				{
					if (signal.value == "0")
					{
						return;
					}
					this.timeSinceReceived[1] = 0f;
					this.signalSender[1] = signal.sender;
					this.IsActive = true;
					return;
				}
			}
			else
			{
				if (signal.value == "0")
				{
					return;
				}
				this.timeSinceReceived[0] = 0f;
				this.signalSender[0] = signal.sender;
				this.IsActive = true;
				return;
			}
		}

		// Token: 0x040033DE RID: 13278
		protected string output;

		// Token: 0x040033DF RID: 13279
		protected string falseOutput;

		// Token: 0x040033E0 RID: 13280
		protected float[] timeSinceReceived;

		// Token: 0x040033E1 RID: 13281
		protected float timeFrame;

		// Token: 0x040033E2 RID: 13282
		protected readonly Character[] signalSender = new Character[2];

		// Token: 0x040033E3 RID: 13283
		private int maxOutputLength;
	}
}
