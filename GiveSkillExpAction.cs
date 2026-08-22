using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000291 RID: 657
	internal class GiveSkillExpAction : EventAction
	{
		// Token: 0x17000F37 RID: 3895
		// (get) Token: 0x060039D8 RID: 14808 RVA: 0x0021D32F File Offset: 0x0021B52F
		// (set) Token: 0x060039D9 RID: 14809 RVA: 0x0021D337 File Offset: 0x0021B537
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the skill to increase.", "", false)]
		public Identifier Skill { get; set; }

		// Token: 0x17000F38 RID: 3896
		// (get) Token: 0x060039DA RID: 14810 RVA: 0x0021D340 File Offset: 0x0021B540
		// (set) Token: 0x060039DB RID: 14811 RVA: 0x0021D348 File Offset: 0x0021B548
		[Serialize(0f, IsPropertySaveable.Yes, "How much the skill should increase.", "", false)]
		public float Amount { get; set; }

		// Token: 0x17000F39 RID: 3897
		// (get) Token: 0x060039DC RID: 14812 RVA: 0x0021D351 File Offset: 0x0021B551
		// (set) Token: 0x060039DD RID: 14813 RVA: 0x0021D359 File Offset: 0x0021B559
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) whose skill to increase.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060039DE RID: 14814 RVA: 0x0021D364 File Offset: 0x0021B564
		public GiveSkillExpAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": ");
				defaultInterpolatedStringHandler.AppendFormatted("GiveSkillExpAction");
				defaultInterpolatedStringHandler.AppendLiteral(" without a target tag (the action needs to know whose skill to check).");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x060039DF RID: 14815 RVA: 0x0021D3EA File Offset: 0x0021B5EA
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060039E0 RID: 14816 RVA: 0x0021D3F2 File Offset: 0x0021B5F2
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039E1 RID: 14817 RVA: 0x0021D3FC File Offset: 0x0021B5FC
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
					info.IncreaseSkillLevel(this.Skill, this.Amount, false, false);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x060039E2 RID: 14818 RVA: 0x0021D4C8 File Offset: 0x0021B6C8
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("GiveSkillExpAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Skill: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Skill.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001DCF RID: 7631
		private bool isFinished;
	}
}
