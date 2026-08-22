using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F0 RID: 1264
	internal class FunctionComponent : ItemComponent
	{
		// Token: 0x1700131E RID: 4894
		// (get) Token: 0x0600473F RID: 18239 RVA: 0x001C65EF File Offset: 0x001C47EF
		// (set) Token: 0x06004740 RID: 18240 RVA: 0x001C65F7 File Offset: 0x001C47F7
		[Serialize(FunctionComponent.FunctionType.Round, IsPropertySaveable.No, "Which kind of function to run the input through.", "", true)]
		public FunctionComponent.FunctionType Function { get; set; }

		// Token: 0x06004741 RID: 18241 RVA: 0x001C6600 File Offset: 0x001C4800
		public FunctionComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004742 RID: 18242 RVA: 0x001C6614 File Offset: 0x001C4814
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

		// Token: 0x02000E34 RID: 3636
		public enum FunctionType
		{
			// Token: 0x04004216 RID: 16918
			Round,
			// Token: 0x04004217 RID: 16919
			Ceil,
			// Token: 0x04004218 RID: 16920
			Floor,
			// Token: 0x04004219 RID: 16921
			Factorial,
			// Token: 0x0400421A RID: 16922
			AbsoluteValue,
			// Token: 0x0400421B RID: 16923
			SquareRoot
		}
	}
}
