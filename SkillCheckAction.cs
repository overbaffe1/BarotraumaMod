using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A5 RID: 677
	internal class SkillCheckAction : BinaryOptionAction
	{
		// Token: 0x17000F7C RID: 3964
		// (get) Token: 0x06003AD2 RID: 15058 RVA: 0x002209E2 File Offset: 0x0021EBE2
		// (set) Token: 0x06003AD3 RID: 15059 RVA: 0x002209EA File Offset: 0x0021EBEA
		[Serialize("", IsPropertySaveable.Yes, "The identifier of the skill to check.", "", false)]
		public Identifier RequiredSkill { get; set; }

		// Token: 0x17000F7D RID: 3965
		// (get) Token: 0x06003AD4 RID: 15060 RVA: 0x002209F3 File Offset: 0x0021EBF3
		// (set) Token: 0x06003AD5 RID: 15061 RVA: 0x002209FB File Offset: 0x0021EBFB
		[Serialize(0f, IsPropertySaveable.Yes, "The required skill level for the check to succeed.", "", false)]
		public float RequiredLevel { get; set; }

		// Token: 0x17000F7E RID: 3966
		// (get) Token: 0x06003AD6 RID: 15062 RVA: 0x00220A04 File Offset: 0x0021EC04
		// (set) Token: 0x06003AD7 RID: 15063 RVA: 0x00220A0C File Offset: 0x0021EC0C
		[Serialize(true, IsPropertySaveable.Yes, "Should the skill check be probability-based (i.e. if you have half the required skill level, the chance of success is 50%), or should the check always fail when under the required level and always succeed when above? ", "", false)]
		public bool ProbabilityBased { get; set; }

		// Token: 0x17000F7F RID: 3967
		// (get) Token: 0x06003AD8 RID: 15064 RVA: 0x00220A15 File Offset: 0x0021EC15
		// (set) Token: 0x06003AD9 RID: 15065 RVA: 0x00220A1D File Offset: 0x0021EC1D
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) whose skill to check. If there are multiple targets, the action succeeds if any of their skill checks succeeds.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06003ADA RID: 15066 RVA: 0x00220A28 File Offset: 0x0021EC28
		public SkillCheckAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": SkillCheckAction without a target tag (the action needs to know whose skill to check).");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06003ADB RID: 15067 RVA: 0x00220A98 File Offset: 0x0021EC98
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Character> potentialTargets = from e in this.ParentEvent.GetTargets(this.TargetTag)
			where e is Character
			select e as Character;
			if (this.ProbabilityBased)
			{
				return new bool?(potentialTargets.Any((Character chr) => chr.GetSkillLevel(this.RequiredSkill) / this.RequiredLevel > Rand.Range(0f, 1f, Rand.RandSync.Unsynced)));
			}
			return new bool?(potentialTargets.Any((Character chr) => chr.GetSkillLevel(this.RequiredSkill) >= this.RequiredLevel));
		}

		// Token: 0x06003ADC RID: 15068 RVA: 0x00220B38 File Offset: 0x0021ED38
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(base.HasBeenDetermined(), false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SkillCheckAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Target: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Skill: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.RequiredSkill.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Level: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.RequiredLevel.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Succeeded: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
