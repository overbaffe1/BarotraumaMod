using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200027D RID: 637
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckDifficultyAction : BinaryOptionAction
	{
		// Token: 0x17000EED RID: 3821
		// (get) Token: 0x060038E6 RID: 14566 RVA: 0x00219AE0 File Offset: 0x00217CE0
		// (set) Token: 0x060038E7 RID: 14567 RVA: 0x00219AE8 File Offset: 0x00217CE8
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum difficulty of the current level for the check to succeed.", "", false)]
		public float MinDifficulty { get; set; }

		// Token: 0x17000EEE RID: 3822
		// (get) Token: 0x060038E8 RID: 14568 RVA: 0x00219AF1 File Offset: 0x00217CF1
		// (set) Token: 0x060038E9 RID: 14569 RVA: 0x00219AF9 File Offset: 0x00217CF9
		[Serialize(100f, IsPropertySaveable.Yes, "Maximum difficulty of the current level for the check to succeed.", "", false)]
		public float MaxDifficulty { get; set; }

		// Token: 0x060038EA RID: 14570 RVA: 0x00219B04 File Offset: 0x00217D04
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

		// Token: 0x060038EB RID: 14571 RVA: 0x00219BC5 File Offset: 0x00217DC5
		protected override bool? DetermineSuccess()
		{
			if (Level.Loaded == null)
			{
				return new bool?(false);
			}
			return new bool?(Level.Loaded.Difficulty >= this.MinDifficulty && Level.Loaded.Difficulty <= this.MaxDifficulty);
		}

		// Token: 0x060038EC RID: 14572 RVA: 0x00219C04 File Offset: 0x00217E04
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
