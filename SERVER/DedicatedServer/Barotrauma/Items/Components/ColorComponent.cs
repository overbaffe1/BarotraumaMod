using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004E8 RID: 1256
	internal class ColorComponent : ItemComponent
	{
		// Token: 0x17001308 RID: 4872
		// (get) Token: 0x060046F3 RID: 18163 RVA: 0x001C4C5C File Offset: 0x001C2E5C
		// (set) Token: 0x060046F4 RID: 18164 RVA: 0x001C4C64 File Offset: 0x001C2E64
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled makes the component translate the signal from HSV into RGB where red is the hue between 0 and 360, green is the saturation between 0 and 1 and blue is the value between 0 and 1.", "", true)]
		public bool UseHSV { get; set; }

		// Token: 0x060046F5 RID: 18165 RVA: 0x001C4C6D File Offset: 0x001C2E6D
		public ColorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.receivedSignal = new float[4];
			this.IsActive = true;
		}

		// Token: 0x060046F6 RID: 18166 RVA: 0x001C4C95 File Offset: 0x001C2E95
		public override void Update(float deltaTime, Camera cam)
		{
			this.item.SendSignal(this.output, "signal_out");
		}

		// Token: 0x060046F7 RID: 18167 RVA: 0x001C4CB0 File Offset: 0x001C2EB0
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

		// Token: 0x060046F8 RID: 18168 RVA: 0x001C4D9C File Offset: 0x001C2F9C
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

		// Token: 0x04002238 RID: 8760
		protected float[] receivedSignal;

		// Token: 0x04002239 RID: 8761
		private string output = "0,0,0,0";
	}
}
