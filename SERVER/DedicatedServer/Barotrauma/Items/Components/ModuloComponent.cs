using System;
using System.Globalization;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F2 RID: 1266
	internal class ModuloComponent : ItemComponent
	{
		// Token: 0x1700131F RID: 4895
		// (get) Token: 0x06004746 RID: 18246 RVA: 0x001C6898 File Offset: 0x001C4A98
		// (set) Token: 0x06004747 RID: 18247 RVA: 0x001C68A0 File Offset: 0x001C4AA0
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

		// Token: 0x06004748 RID: 18248 RVA: 0x001C68C2 File Offset: 0x001C4AC2
		public ModuloComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004749 RID: 18249 RVA: 0x001C68D4 File Offset: 0x001C4AD4
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

		// Token: 0x04002266 RID: 8806
		private float modulus;
	}
}
