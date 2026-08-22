using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004EF RID: 1263
	internal class ExponentiationComponent : ItemComponent
	{
		// Token: 0x1700131D RID: 4893
		// (get) Token: 0x0600473B RID: 18235 RVA: 0x001C6524 File Offset: 0x001C4724
		// (set) Token: 0x0600473C RID: 18236 RVA: 0x001C652C File Offset: 0x001C472C
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

		// Token: 0x0600473D RID: 18237 RVA: 0x001C6535 File Offset: 0x001C4735
		public ExponentiationComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600473E RID: 18238 RVA: 0x001C6548 File Offset: 0x001C4748
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

		// Token: 0x04002262 RID: 8802
		private float exponent;
	}
}
