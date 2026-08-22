using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001AF RID: 431
	internal class RNGAction : BinaryOptionAction
	{
		// Token: 0x1700092E RID: 2350
		// (get) Token: 0x06001FFF RID: 8191 RVA: 0x000DA63A File Offset: 0x000D883A
		// (set) Token: 0x06002000 RID: 8192 RVA: 0x000DA642 File Offset: 0x000D8842
		[Serialize(0f, IsPropertySaveable.Yes, "The probability of executing the Success actions. A value between 0-1.", "", false)]
		public float Chance { get; set; }

		// Token: 0x06002001 RID: 8193 RVA: 0x000DA64C File Offset: 0x000D884C
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

		// Token: 0x06002002 RID: 8194 RVA: 0x000DA70E File Offset: 0x000D890E
		protected override bool? DetermineSuccess()
		{
			this.isFinished = true;
			return new bool?(Rand.Range(0.0, 1.0, Rand.RandSync.Unsynced) <= (double)this.Chance);
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x000DA740 File Offset: 0x000D8940
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

		// Token: 0x04000F38 RID: 3896
		private bool isFinished;
	}
}
