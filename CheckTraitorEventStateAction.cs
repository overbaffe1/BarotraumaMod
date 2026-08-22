using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000286 RID: 646
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckTraitorEventStateAction : BinaryOptionAction
	{
		// Token: 0x17000F11 RID: 3857
		// (get) Token: 0x06003955 RID: 14677 RVA: 0x0021B97F File Offset: 0x00219B7F
		// (set) Token: 0x06003956 RID: 14678 RVA: 0x0021B987 File Offset: 0x00219B87
		[Serialize(TraitorEvent.State.Completed, IsPropertySaveable.Yes, "What does the state of the event need to be for the check to succeed?", "", false)]
		public TraitorEvent.State State { get; set; }

		// Token: 0x06003957 RID: 14679 RVA: 0x0021B990 File Offset: 0x00219B90
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

		// Token: 0x06003958 RID: 14680 RVA: 0x0021BA18 File Offset: 0x00219C18
		protected override bool? DetermineSuccess()
		{
			TraitorEvent traitorEvent = this.traitorEvent;
			TraitorEvent.State? state = (traitorEvent != null) ? new TraitorEvent.State?(traitorEvent.CurrentState) : null;
			TraitorEvent.State state2 = this.State;
			return new bool?(state.GetValueOrDefault() == state2 & state != null);
		}

		// Token: 0x06003959 RID: 14681 RVA: 0x0021BA64 File Offset: 0x00219C64
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

		// Token: 0x04001D9D RID: 7581
		[Nullable(2)]
		private readonly TraitorEvent traitorEvent;
	}
}
