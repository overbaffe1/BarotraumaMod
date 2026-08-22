using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FC RID: 1276
	internal class SignalCheckComponent : ItemComponent
	{
		// Token: 0x17001340 RID: 4928
		// (get) Token: 0x060047B1 RID: 18353 RVA: 0x001C87DE File Offset: 0x001C69DE
		// (set) Token: 0x060047B2 RID: 18354 RVA: 0x001C87E6 File Offset: 0x001C69E6
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

		// Token: 0x17001341 RID: 4929
		// (get) Token: 0x060047B3 RID: 18355 RVA: 0x001C87F5 File Offset: 0x001C69F5
		// (set) Token: 0x060047B4 RID: 18356 RVA: 0x001C8800 File Offset: 0x001C6A00
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

		// Token: 0x17001342 RID: 4930
		// (get) Token: 0x060047B5 RID: 18357 RVA: 0x001C8862 File Offset: 0x001C6A62
		// (set) Token: 0x060047B6 RID: 18358 RVA: 0x001C886C File Offset: 0x001C6A6C
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

		// Token: 0x17001343 RID: 4931
		// (get) Token: 0x060047B7 RID: 18359 RVA: 0x001C88CE File Offset: 0x001C6ACE
		// (set) Token: 0x060047B8 RID: 18360 RVA: 0x001C88D6 File Offset: 0x001C6AD6
		[InGameEditable]
		[Serialize("", IsPropertySaveable.Yes, "The value to compare the received signals against.", "", true)]
		public string TargetSignal { get; set; }

		// Token: 0x060047B9 RID: 18361 RVA: 0x001C88DF File Offset: 0x001C6ADF
		public SignalCheckComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060047BA RID: 18362 RVA: 0x001C88EC File Offset: 0x001C6AEC
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

		// Token: 0x040022A5 RID: 8869
		private int maxOutputLength;

		// Token: 0x040022A6 RID: 8870
		private string output;

		// Token: 0x040022A7 RID: 8871
		private string falseOutput;
	}
}
