using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000193 RID: 403
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckTraitorEventStateAction : BinaryOptionAction
	{
		// Token: 0x170008BD RID: 2237
		// (get) Token: 0x06001E8B RID: 7819 RVA: 0x000D6483 File Offset: 0x000D4683
		// (set) Token: 0x06001E8C RID: 7820 RVA: 0x000D648B File Offset: 0x000D468B
		[Serialize(TraitorEvent.State.Completed, IsPropertySaveable.Yes, "What does the state of the event need to be for the check to succeed?", "", false)]
		public TraitorEvent.State State { get; set; }

		// Token: 0x06001E8D RID: 7821 RVA: 0x000D6494 File Offset: 0x000D4694
		public CheckTraitorEventStateAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			TraitorEvent traitorEvent = parentEvent as TraitorEvent;
			if (traitorEvent != null)
			{
				this.traitorEvent = traitorEvent;
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Cannot use the action ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorEventStateAction");
			defaultInterpolatedStringHandler.AppendLiteral(" in the event \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral("\" because it's not a traitor event.");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x000D651C File Offset: 0x000D471C
		protected override bool? DetermineSuccess()
		{
			TraitorEvent traitorEvent = this.traitorEvent;
			TraitorEvent.State? state = (traitorEvent != null) ? new TraitorEvent.State?(traitorEvent.CurrentState) : null;
			TraitorEvent.State state2 = this.State;
			return new bool?(state.GetValueOrDefault() == state2 & state != null);
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x000D6568 File Offset: 0x000D4768
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorEventStateAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> ");
			defaultInterpolatedStringHandler.AppendLiteral("State: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.State.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000EA6 RID: 3750
		[Nullable(2)]
		private readonly TraitorEvent traitorEvent;
	}
}
