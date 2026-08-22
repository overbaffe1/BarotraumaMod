using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F6 RID: 1270
	internal class NotComponent : ItemComponent
	{
		// Token: 0x17001330 RID: 4912
		// (get) Token: 0x06004775 RID: 18293 RVA: 0x001C7627 File Offset: 0x001C5827
		// (set) Token: 0x06004776 RID: 18294 RVA: 0x001C7630 File Offset: 0x001C5830
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

		// Token: 0x06004777 RID: 18295 RVA: 0x001C764D File Offset: 0x001C584D
		public NotComponent(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004778 RID: 18296 RVA: 0x001C7657 File Offset: 0x001C5857
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (!this.signalReceived)
			{
				this.item.SendSignal("1", "signal_out");
			}
			this.signalReceived = false;
		}

		// Token: 0x06004779 RID: 18297 RVA: 0x001C7688 File Offset: 0x001C5888
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

		// Token: 0x04002278 RID: 8824
		private bool signalReceived;

		// Token: 0x04002279 RID: 8825
		private bool continuousOutput;
	}
}
