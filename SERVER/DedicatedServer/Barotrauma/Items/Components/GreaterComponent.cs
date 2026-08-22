using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F1 RID: 1265
	internal class GreaterComponent : EqualsComponent
	{
		// Token: 0x06004743 RID: 18243 RVA: 0x001C676A File Offset: 0x001C496A
		public GreaterComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004744 RID: 18244 RVA: 0x001C677C File Offset: 0x001C497C
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
				string signalOut = (this.val1 > this.val2) ? this.output : this.falseOutput;
				if (string.IsNullOrEmpty(signalOut))
				{
					return;
				}
				this.item.SendSignal(signalOut, "signal_out");
			}
		}

		// Token: 0x06004745 RID: 18245 RVA: 0x001C67FC File Offset: 0x001C49FC
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.val1);
				this.timeSinceReceived[0] = 0f;
				return;
			}
			if (name == "signal_in2")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.val2);
				this.timeSinceReceived[1] = 0f;
				return;
			}
			if (!(name == "set_output"))
			{
				return;
			}
			this.output = signal.value;
		}

		// Token: 0x04002264 RID: 8804
		private float val1;

		// Token: 0x04002265 RID: 8805
		private float val2;
	}
}
