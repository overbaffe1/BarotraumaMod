using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200060D RID: 1549
	internal class ColorComponent : ItemComponent
	{
		// Token: 0x1700194E RID: 6478
		// (get) Token: 0x06006402 RID: 25602 RVA: 0x0033F2F4 File Offset: 0x0033D4F4
		// (set) Token: 0x06006403 RID: 25603 RVA: 0x0033F2FC File Offset: 0x0033D4FC
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled makes the component translate the signal from HSV into RGB where red is the hue between 0 and 360, green is the saturation between 0 and 1 and blue is the value between 0 and 1.", "", true)]
		public bool UseHSV { get; set; }

		// Token: 0x06006404 RID: 25604 RVA: 0x0033F305 File Offset: 0x0033D505
		public ColorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.receivedSignal = new float[4];
			this.IsActive = true;
		}

		// Token: 0x06006405 RID: 25605 RVA: 0x0033F32D File Offset: 0x0033D52D
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.output, "signal_out");
		}

		// Token: 0x06006406 RID: 25606 RVA: 0x0033F348 File Offset: 0x0033D548
		private void UpdateOutput()
		{
			float signalR = this.receivedSignal[0];
			float signalG = this.receivedSignal[1];
			float signalB = this.receivedSignal[2];
			float signalA = this.receivedSignal[3];
			if (this.UseHSV)
			{
				Color hsvColor = ToolBoxCore.HSVToRGB(signalR, signalG, signalB);
				signalR = (float)hsvColor.R;
				signalG = (float)hsvColor.G;
				signalB = (float)hsvColor.B;
			}
			this.output = signalR.ToString("G", CultureInfo.InvariantCulture);
			this.output = this.output + "," + signalG.ToString("G", CultureInfo.InvariantCulture);
			this.output = this.output + "," + signalB.ToString("G", CultureInfo.InvariantCulture);
			this.output = this.output + "," + signalA.ToString("G", CultureInfo.InvariantCulture);
		}

		// Token: 0x06006407 RID: 25607 RVA: 0x0033F434 File Offset: 0x0033D634
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_r")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[0]);
				this.UpdateOutput();
				return;
			}
			if (name == "signal_g")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[1]);
				this.UpdateOutput();
				return;
			}
			if (name == "signal_b")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[2]);
				this.UpdateOutput();
				return;
			}
			if (!(name == "signal_a"))
			{
				return;
			}
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[3]);
			this.UpdateOutput();
		}

		// Token: 0x040033E4 RID: 13284
		protected float[] receivedSignal;

		// Token: 0x040033E5 RID: 13285
		private string output = "0,0,0,0";
	}
}
