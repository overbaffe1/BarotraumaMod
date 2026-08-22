using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061F RID: 1567
	internal class SignalCheckComponent : ItemComponent
	{
		// Token: 0x1700196F RID: 6511
		// (get) Token: 0x06006481 RID: 25729 RVA: 0x0034149A File Offset: 0x0033F69A
		// (set) Token: 0x06006482 RID: 25730 RVA: 0x003414A2 File Offset: 0x0033F6A2
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

		// Token: 0x17001970 RID: 6512
		// (get) Token: 0x06006483 RID: 25731 RVA: 0x003414B1 File Offset: 0x0033F6B1
		// (set) Token: 0x06006484 RID: 25732 RVA: 0x003414BC File Offset: 0x0033F6BC
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal this item outputs when the received signal matches the target signal.", "", true)]
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

		// Token: 0x17001971 RID: 6513
		// (get) Token: 0x06006485 RID: 25733 RVA: 0x0034151E File Offset: 0x0033F71E
		// (set) Token: 0x06006486 RID: 25734 RVA: 0x00341528 File Offset: 0x0033F728
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal this item outputs when the received signal does not match the target signal.", "", true)]
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

		// Token: 0x17001972 RID: 6514
		// (get) Token: 0x06006487 RID: 25735 RVA: 0x0034158A File Offset: 0x0033F78A
		// (set) Token: 0x06006488 RID: 25736 RVA: 0x00341592 File Offset: 0x0033F792
		[InGameEditable]
		[Serialize("", IsPropertySaveable.Yes, "The value to compare the received signals against.", "", true)]
		public string TargetSignal { get; set; }

		// Token: 0x06006489 RID: 25737 RVA: 0x0034159B File Offset: 0x0033F79B
		public SignalCheckComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600648A RID: 25738 RVA: 0x003415A8 File Offset: 0x0033F7A8
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "signal_in"))
			{
				if (name == "set_output")
				{
					this.Output = signal.value;
					return;
				}
				if (!(name == "set_targetsignal"))
				{
					return;
				}
				this.TargetSignal = signal.value;
				return;
			}
			else
			{
				string signalOut = (signal.value == this.TargetSignal) ? this.Output : this.FalseOutput;
				if (string.IsNullOrEmpty(signalOut))
				{
					return;
				}
				signal.value = signalOut;
				this.item.SendSignal(signal, "signal_out");
				return;
			}
		}

		// Token: 0x04003429 RID: 13353
		private int maxOutputLength;

		// Token: 0x0400342A RID: 13354
		private string output;

		// Token: 0x0400342B RID: 13355
		private string falseOutput;
	}
}
