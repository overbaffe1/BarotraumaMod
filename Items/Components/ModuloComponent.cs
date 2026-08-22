using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000616 RID: 1558
	internal class ModuloComponent : ItemComponent
	{
		// Token: 0x1700195C RID: 6492
		// (get) Token: 0x06006437 RID: 25655 RVA: 0x003400CC File Offset: 0x0033E2CC
		// (set) Token: 0x06006438 RID: 25656 RVA: 0x003400D4 File Offset: 0x0033E2D4
		[InGameEditable]
		[Serialize(1f, IsPropertySaveable.No, "The modulus of the operation. Must be non-zero.", "", true)]
		public float Modulus
		{
			get
			{
				return this.modulus;
			}
			set
			{
				this.modulus = (MathUtils.NearlyEqual(value, 0f, 0.0001f) ? 1f : value);
			}
		}

		// Token: 0x06006439 RID: 25657 RVA: 0x003400F6 File Offset: 0x0033E2F6
		public ModuloComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600643A RID: 25658 RVA: 0x00340108 File Offset: 0x0033E308
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "set_modulus" || name == "modulus")
			{
				float newModulus;
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newModulus);
				this.Modulus = newModulus;
				return;
			}
			if (!(name == "signal_in"))
			{
				return;
			}
			float value;
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
			signal.value = (value % this.modulus).ToString("G", CultureInfo.InvariantCulture);
			this.item.SendSignal(signal, "signal_out");
		}

		// Token: 0x040033FB RID: 13307
		private float modulus;
	}
}
