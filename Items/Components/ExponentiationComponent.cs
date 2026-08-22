using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000613 RID: 1555
	internal class ExponentiationComponent : ItemComponent
	{
		// Token: 0x1700195A RID: 6490
		// (get) Token: 0x0600642C RID: 25644 RVA: 0x0033FD58 File Offset: 0x0033DF58
		// (set) Token: 0x0600642D RID: 25645 RVA: 0x0033FD60 File Offset: 0x0033DF60
		[InGameEditable]
		[Serialize(1f, IsPropertySaveable.No, "The exponent of the operation.", "", true)]
		public float Exponent
		{
			get
			{
				return this.exponent;
			}
			set
			{
				this.exponent = value;
			}
		}

		// Token: 0x0600642E RID: 25646 RVA: 0x0033FD69 File Offset: 0x0033DF69
		public ExponentiationComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600642F RID: 25647 RVA: 0x0033FD7C File Offset: 0x0033DF7C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "set_exponent" || name == "exponent")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.exponent);
				return;
			}
			if (!(name == "signal_in"))
			{
				return;
			}
			float value;
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
			signal.value = MathUtils.Pow(value, this.Exponent).ToString("G", CultureInfo.InvariantCulture);
			this.item.SendSignal(signal, "signal_out");
		}

		// Token: 0x040033F7 RID: 13303
		private float exponent;
	}
}
