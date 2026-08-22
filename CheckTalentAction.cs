using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000285 RID: 645
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CheckTalentAction : BinaryOptionAction
	{
		// Token: 0x17000F0F RID: 3855
		// (get) Token: 0x0600394E RID: 14670 RVA: 0x0021B7B6 File Offset: 0x002199B6
		// (set) Token: 0x0600394F RID: 14671 RVA: 0x0021B7BE File Offset: 0x002199BE
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the talent to check for.", "", false)]
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x17000F10 RID: 3856
		// (get) Token: 0x06003950 RID: 14672 RVA: 0x0021B7C7 File Offset: 0x002199C7
		// (set) Token: 0x06003951 RID: 14673 RVA: 0x0021B7CF File Offset: 0x002199CF
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06003952 RID: 14674 RVA: 0x0021B7D8 File Offset: 0x002199D8
		public CheckTalentAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003953 RID: 14675 RVA: 0x0021B7E4 File Offset: 0x002199E4
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

		// Token: 0x06003954 RID: 14676 RVA: 0x0021B878 File Offset: 0x00219A78
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
