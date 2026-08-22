using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000189 RID: 393
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckDifficultyAction : BinaryOptionAction
	{
		// Token: 0x17000899 RID: 2201
		// (get) Token: 0x06001E1A RID: 7706 RVA: 0x000D452C File Offset: 0x000D272C
		// (set) Token: 0x06001E1B RID: 7707 RVA: 0x000D4534 File Offset: 0x000D2734
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum difficulty of the current level for the check to succeed.", "", false)]
		public float MinDifficulty { get; set; }

		// Token: 0x1700089A RID: 2202
		// (get) Token: 0x06001E1C RID: 7708 RVA: 0x000D453D File Offset: 0x000D273D
		// (set) Token: 0x06001E1D RID: 7709 RVA: 0x000D4545 File Offset: 0x000D2745
		[Serialize(100f, IsPropertySaveable.Yes, "Maximum difficulty of the current level for the check to succeed.", "", false)]
		public float MaxDifficulty { get; set; }

		// Token: 0x06001E1E RID: 7710 RVA: 0x000D4550 File Offset: 0x000D2750
		public CheckDifficultyAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.MaxDifficulty <= this.MinDifficulty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(94, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in event ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetEventDebugName());
				defaultInterpolatedStringHandler.AppendLiteral(": maximum difficulty (");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.MaxDifficulty);
				defaultInterpolatedStringHandler.AppendLiteral(") is not larger than minimum difficulty (");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.MinDifficulty);
				defaultInterpolatedStringHandler.AppendLiteral(") in ");
				defaultInterpolatedStringHandler.AppendFormatted("CheckDifficultyAction");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				ContentPackage contentPackage = parentEvent.Prefab.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
			}
		}

		// Token: 0x06001E1F RID: 7711 RVA: 0x000D4611 File Offset: 0x000D2811
		protected override bool? DetermineSuccess()
		{
			if (Level.Loaded == null)
			{
				return new bool?(false);
			}
			return new bool?(Level.Loaded.Difficulty >= this.MinDifficulty && Level.Loaded.Difficulty <= this.MaxDifficulty);
		}

		// Token: 0x06001E20 RID: 7712 RVA: 0x000D4650 File Offset: 0x000D2850
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.DetermineFinished(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckDifficultyAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (min: ");
			defaultInterpolatedStringHandler.AppendFormatted<float>(this.MinDifficulty);
			defaultInterpolatedStringHandler.AppendLiteral(", max: ");
			defaultInterpolatedStringHandler.AppendFormatted<float>(this.MaxDifficulty);
			defaultInterpolatedStringHandler.AppendLiteral(" Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(((this.succeeded != null) ? this.succeeded.Value.ToString() : "not determined").ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
