using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000192 RID: 402
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CheckTalentAction : BinaryOptionAction
	{
		// Token: 0x170008BB RID: 2235
		// (get) Token: 0x06001E84 RID: 7812 RVA: 0x000D62BA File Offset: 0x000D44BA
		// (set) Token: 0x06001E85 RID: 7813 RVA: 0x000D62C2 File Offset: 0x000D44C2
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the talent to check for.", "", false)]
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x170008BC RID: 2236
		// (get) Token: 0x06001E86 RID: 7814 RVA: 0x000D62CB File Offset: 0x000D44CB
		// (set) Token: 0x06001E87 RID: 7815 RVA: 0x000D62D3 File Offset: 0x000D44D3
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001E88 RID: 7816 RVA: 0x000D62DC File Offset: 0x000D44DC
		public CheckTalentAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x000D62E8 File Offset: 0x000D44E8
		protected override bool? DetermineSuccess()
		{
			if (this.TargetTag.IsEmpty)
			{
				return new bool?(false);
			}
			Character matchingCharacter = null;
			foreach (Entity entity in this.ParentEvent.GetTargets(this.TargetTag))
			{
				Character character = entity as Character;
				if (character != null)
				{
					matchingCharacter = character;
					break;
				}
			}
			return new bool?(matchingCharacter != null && matchingCharacter.HasTalent(this.TalentIdentifier));
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x000D637C File Offset: 0x000D457C
		public override string ToDebugString()
		{
			string subActionStr = "";
			if (this.succeeded != null)
			{
				string str = "\n            Sub action: ";
				EventAction.SubactionGroup subactionGroup = this.succeeded.Value ? this.Success : this.Failure;
				subActionStr = str + ((subactionGroup != null) ? subactionGroup.CurrentSubAction.ColorizeObject() : null);
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 4);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.DetermineFinished(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckTalentAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Talent: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TalentIdentifier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(" Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(((this.succeeded != null) ? this.succeeded.Value.ToString() : "not determined").ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear() + subActionStr;
		}
	}
}
