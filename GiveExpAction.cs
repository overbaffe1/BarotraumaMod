using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000290 RID: 656
	internal class GiveExpAction : EventAction
	{
		// Token: 0x17000F35 RID: 3893
		// (get) Token: 0x060039CF RID: 14799 RVA: 0x0021D109 File Offset: 0x0021B309
		// (set) Token: 0x060039D0 RID: 14800 RVA: 0x0021D111 File Offset: 0x0021B311
		[Serialize(0, IsPropertySaveable.Yes, "The amount of experience to give. Cannot be negative.", "", false)]
		public int Amount { get; set; }

		// Token: 0x17000F36 RID: 3894
		// (get) Token: 0x060039D1 RID: 14801 RVA: 0x0021D11A File Offset: 0x0021B31A
		// (set) Token: 0x060039D2 RID: 14802 RVA: 0x0021D122 File Offset: 0x0021B322
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to give the experience to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060039D3 RID: 14803 RVA: 0x0021D12C File Offset: 0x0021B32C
		public GiveExpAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": ");
				defaultInterpolatedStringHandler.AppendFormatted("GiveExpAction");
				defaultInterpolatedStringHandler.AppendLiteral(" without a target tag (the action needs to know whose skill to check).");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x060039D4 RID: 14804 RVA: 0x0021D1B2 File Offset: 0x0021B3B2
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060039D5 RID: 14805 RVA: 0x0021D1BA File Offset: 0x0021B3BA
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039D6 RID: 14806 RVA: 0x0021D1C4 File Offset: 0x0021B3C4
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Character> targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
			where e is Character
			select e as Character;
			foreach (Character target in targets)
			{
				CharacterInfo info = target.Info;
				if (info != null)
				{
					info.GiveExperience(this.Amount);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x060039D7 RID: 14807 RVA: 0x0021D288 File Offset: 0x0021B488
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("GiveExpAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DCB RID: 7627
		private bool isFinished;
	}
}
