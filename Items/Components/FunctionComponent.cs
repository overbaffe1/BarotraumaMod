using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000614 RID: 1556
	internal class FunctionComponent : ItemComponent
	{
		// Token: 0x1700195B RID: 6491
		// (get) Token: 0x06006430 RID: 25648 RVA: 0x0033FE23 File Offset: 0x0033E023
		// (set) Token: 0x06006431 RID: 25649 RVA: 0x0033FE2B File Offset: 0x0033E02B
		[Serialize(FunctionComponent.FunctionType.Round, IsPropertySaveable.No, "Which kind of function to run the input through.", "", true)]
		public FunctionComponent.FunctionType Function { get; set; }

		// Token: 0x06006432 RID: 25650 RVA: 0x0033FE34 File Offset: 0x0033E034
		public FunctionComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06006433 RID: 25651 RVA: 0x0033FE48 File Offset: 0x0033E048
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name != "signal_in")
			{
				return;
			}
			float value;
			if (!float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				return;
			}
			switch (this.Function)
			{
			case FunctionComponent.FunctionType.Round:
				value = MathF.Round(value);
				if (value == 0f)
				{
					value = 0f;
				}
				break;
			case FunctionComponent.FunctionType.Ceil:
				value = MathF.Ceiling(value);
				if (value == 0f)
				{
					value = 0f;
				}
				break;
			case FunctionComponent.FunctionType.Floor:
				value = MathF.Floor(value);
				break;
			case FunctionComponent.FunctionType.Factorial:
			{
				int intVal = (int)Math.Min(value, 20f);
				ulong factorial = 1UL;
				for (int i = intVal; i > 0; i--)
				{
					factorial *= (ulong)((long)i);
				}
				value = factorial;
				break;
			}
			case FunctionComponent.FunctionType.AbsoluteValue:
				value = MathF.Abs(value);
				break;
			case FunctionComponent.FunctionType.SquareRoot:
				if (value < 0f)
				{
					return;
				}
				value = MathF.Sqrt(value);
				break;
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Function ");
				defaultInterpolatedStringHandler.AppendFormatted<FunctionComponent.FunctionType>(this.Function);
				defaultInterpolatedStringHandler.AppendLiteral(" has not been implemented.");
				throw new NotImplementedException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			}
			signal.value = value.ToString("G", CultureInfo.InvariantCulture);
			this.item.SendSignal(signal, "signal_out");
		}

		// Token: 0x0200149E RID: 5278
		public enum FunctionType
		{
			// Token: 0x0400665F RID: 26207
			Round,
			// Token: 0x04006660 RID: 26208
			Ceil,
			// Token: 0x04006661 RID: 26209
			Floor,
			// Token: 0x04006662 RID: 26210
			Factorial,
			// Token: 0x04006663 RID: 26211
			AbsoluteValue,
			// Token: 0x04006664 RID: 26212
			SquareRoot
		}
	}
}
