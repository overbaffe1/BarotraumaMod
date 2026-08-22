using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000619 RID: 1561
	internal class NotComponent : ItemComponent
	{
		// Token: 0x1700195F RID: 6495
		// (get) Token: 0x06006445 RID: 25669 RVA: 0x003402F3 File Offset: 0x0033E4F3
		// (set) Token: 0x06006446 RID: 25670 RVA: 0x003402FC File Offset: 0x0033E4FC
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled, the component continuously outputs \"1\" when it's not receiving a signal.", "", true)]
		public bool ContinuousOutput
		{
			get
			{
				return this.continuousOutput;
			}
			set
			{
				this.IsActive = value;
				this.continuousOutput = value;
			}
		}

		// Token: 0x06006447 RID: 25671 RVA: 0x00340319 File Offset: 0x0033E519
		public NotComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06006448 RID: 25672 RVA: 0x00340323 File Offset: 0x0033E523
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (!this.signalReceived)
			{
				this.item.SendSignal("1", "signal_out");
			}
			this.signalReceived = false;
		}

		// Token: 0x06006449 RID: 25673 RVA: 0x00340354 File Offset: 0x0033E554
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name != "signal_in")
			{
				return;
			}
			signal.value = ((signal.value == "0" || string.IsNullOrEmpty(signal.value)) ? "1" : "0");
			signal.power = 0f;
			this.item.SendSignal(signal, "signal_out");
			this.signalReceived = true;
		}

		// Token: 0x040033FC RID: 13308
		private bool signalReceived;

		// Token: 0x040033FD RID: 13309
		private bool continuousOutput;
	}
}
