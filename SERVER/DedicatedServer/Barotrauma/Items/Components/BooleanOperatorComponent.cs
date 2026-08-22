using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E5 RID: 1253
	internal abstract class BooleanOperatorComponent : ItemComponent
	{
		// Token: 0x17001304 RID: 4868
		// (get) Token: 0x060046E3 RID: 18147 RVA: 0x001C490F File Offset: 0x001C2B0F
		// (set) Token: 0x060046E4 RID: 18148 RVA: 0x001C4918 File Offset: 0x001C2B18
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

		// Token: 0x17001305 RID: 4869
		// (get) Token: 0x060046E5 RID: 18149 RVA: 0x001C4963 File Offset: 0x001C2B63
		// (set) Token: 0x060046E6 RID: 18150 RVA: 0x001C496B File Offset: 0x001C2B6B
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

		// Token: 0x17001306 RID: 4870
		// (get) Token: 0x060046E7 RID: 18151 RVA: 0x001C497A File Offset: 0x001C2B7A
		// (set) Token: 0x060046E8 RID: 18152 RVA: 0x001C4984 File Offset: 0x001C2B84
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

		// Token: 0x17001307 RID: 4871
		// (get) Token: 0x060046E9 RID: 18153 RVA: 0x001C49ED File Offset: 0x001C2BED
		// (set) Token: 0x060046EA RID: 18154 RVA: 0x001C49F8 File Offset: 0x001C2BF8
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

		// Token: 0x060046EB RID: 18155 RVA: 0x001C4A64 File Offset: 0x001C2C64
		public BooleanOperatorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.IsActive = true;
		}

		// Token: 0x060046EC RID: 18156
		protected abstract bool GetOutput(int numTrueInputs);

		// Token: 0x060046ED RID: 18157 RVA: 0x001C4ACC File Offset: 0x001C2CCC
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

		// Token: 0x060046EE RID: 18158 RVA: 0x001C4B88 File Offset: 0x001C2D88
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

		// Token: 0x04002232 RID: 8754
		protected string output;

		// Token: 0x04002233 RID: 8755
		protected string falseOutput;

		// Token: 0x04002234 RID: 8756
		protected float[] timeSinceReceived;

		// Token: 0x04002235 RID: 8757
		protected float timeFrame;

		// Token: 0x04002236 RID: 8758
		protected readonly Character[] signalSender = new Character[2];

		// Token: 0x04002237 RID: 8759
		private int maxOutputLength;
	}
}
