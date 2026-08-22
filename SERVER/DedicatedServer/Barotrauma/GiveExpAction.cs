using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200019D RID: 413
	internal class GiveExpAction : EventAction
	{
		// Token: 0x170008E1 RID: 2273
		// (get) Token: 0x06001F05 RID: 7941 RVA: 0x000D7C55 File Offset: 0x000D5E55
		// (set) Token: 0x06001F06 RID: 7942 RVA: 0x000D7C5D File Offset: 0x000D5E5D
		[Serialize(0, IsPropertySaveable.Yes, "The amount of experience to give. Cannot be negative.", "", false)]
		public int Amount { get; set; }

		// Token: 0x170008E2 RID: 2274
		// (get) Token: 0x06001F07 RID: 7943 RVA: 0x000D7C66 File Offset: 0x000D5E66
		// (set) Token: 0x06001F08 RID: 7944 RVA: 0x000D7C6E File Offset: 0x000D5E6E
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) to give the experience to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001F09 RID: 7945 RVA: 0x000D7C78 File Offset: 0x000D5E78
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

		// Token: 0x06001F0A RID: 7946 RVA: 0x000D7CFE File Offset: 0x000D5EFE
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F0B RID: 7947 RVA: 0x000D7D06 File Offset: 0x000D5F06
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x000D7D10 File Offset: 0x000D5F10
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

		// Token: 0x06001F0D RID: 7949 RVA: 0x000D7DD4 File Offset: 0x000D5FD4
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

		// Token: 0x04000ED4 RID: 3796
		private bool isFinished;
	}
}
