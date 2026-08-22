using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000615 RID: 1557
	internal class GreaterComponent : EqualsComponent
	{
		// Token: 0x06006434 RID: 25652 RVA: 0x0033FF9E File Offset: 0x0033E19E
		public GreaterComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006435 RID: 25653 RVA: 0x0033FFB0 File Offset: 0x0033E1B0
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

		// Token: 0x06006436 RID: 25654 RVA: 0x00340030 File Offset: 0x0033E230
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

		// Token: 0x040033F9 RID: 13305
		private float val1;

		// Token: 0x040033FA RID: 13306
		private float val2;
	}
}
