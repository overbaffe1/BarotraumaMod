using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000287 RID: 647
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckTraitorVoteAction : BinaryOptionAction
	{
		// Token: 0x17000F12 RID: 3858
		// (get) Token: 0x0600395A RID: 14682 RVA: 0x0021BB0B File Offset: 0x00219D0B
		// (set) Token: 0x0600395B RID: 14683 RVA: 0x0021BB13 File Offset: 0x00219D13
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier Target { get; set; }

		// Token: 0x0600395C RID: 14684 RVA: 0x0021BB1C File Offset: 0x00219D1C
		public CheckTraitorVoteAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (!(parentEvent is TraitorEvent))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" - ");
				defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorVoteAction");
				defaultInterpolatedStringHandler.AppendLiteral(" can only be used in traitor events.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x0600395D RID: 14685 RVA: 0x0021BB9C File Offset: 0x00219D9C
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Entity> targetEntities = this.ParentEvent.GetTargets(this.Target);
			return new bool?(false);
		}

		// Token: 0x0600395E RID: 14686 RVA: 0x0021BBC4 File Offset: 0x00219DC4
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.succeeded != null, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorVoteAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Target.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
