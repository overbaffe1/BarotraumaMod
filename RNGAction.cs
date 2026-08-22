using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A1 RID: 673
	internal class RNGAction : BinaryOptionAction
	{
		// Token: 0x17000F74 RID: 3956
		// (get) Token: 0x06003AAB RID: 15019 RVA: 0x00220156 File Offset: 0x0021E356
		// (set) Token: 0x06003AAC RID: 15020 RVA: 0x0022015E File Offset: 0x0021E35E
		[Serialize(0f, IsPropertySaveable.Yes, "The probability of executing the Success actions. A value between 0-1.", "", false)]
		public float Chance { get; set; }

		// Token: 0x06003AAD RID: 15021 RVA: 0x00220168 File Offset: 0x0021E368
		public RNGAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Chance >= 1f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(113, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Incorrectly configured RNG Action in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Probability is 1.0 (100%) or more, the action will always succeed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				return;
			}
			if (this.Chance <= 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(103, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Incorrectly configured RNG Action in event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\". Probability is 0 or less, the action will never succeed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06003AAE RID: 15022 RVA: 0x0022022A File Offset: 0x0021E42A
		protected override bool? DetermineSuccess()
		{
			this.isFinished = true;
			return new bool?(Rand.Range(0.0, 1.0, Rand.RandSync.Unsynced) <= (double)this.Chance);
		}

		// Token: 0x06003AAF RID: 15023 RVA: 0x0022025C File Offset: 0x0021E45C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("RNGAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Chance: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Chance.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E21 RID: 7713
		private bool isFinished;
	}
}
