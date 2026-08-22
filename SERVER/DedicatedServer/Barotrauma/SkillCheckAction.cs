using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001B3 RID: 435
	internal class SkillCheckAction : BinaryOptionAction
	{
		// Token: 0x17000936 RID: 2358
		// (get) Token: 0x06002026 RID: 8230 RVA: 0x000DAEC6 File Offset: 0x000D90C6
		// (set) Token: 0x06002027 RID: 8231 RVA: 0x000DAECE File Offset: 0x000D90CE
		[Serialize("", IsPropertySaveable.Yes, "The identifier of the skill to check.", "", false)]
		public Identifier RequiredSkill { get; set; }

		// Token: 0x17000937 RID: 2359
		// (get) Token: 0x06002028 RID: 8232 RVA: 0x000DAED7 File Offset: 0x000D90D7
		// (set) Token: 0x06002029 RID: 8233 RVA: 0x000DAEDF File Offset: 0x000D90DF
		[Serialize(0f, IsPropertySaveable.Yes, "The required skill level for the check to succeed.", "", false)]
		public float RequiredLevel { get; set; }

		// Token: 0x17000938 RID: 2360
		// (get) Token: 0x0600202A RID: 8234 RVA: 0x000DAEE8 File Offset: 0x000D90E8
		// (set) Token: 0x0600202B RID: 8235 RVA: 0x000DAEF0 File Offset: 0x000D90F0
		[Serialize(true, IsPropertySaveable.Yes, "Should the skill check be probability-based (i.e. if you have half the required skill level, the chance of success is 50%), or should the check always fail when under the required level and always succeed when above? ", "", false)]
		public bool ProbabilityBased { get; set; }

		// Token: 0x17000939 RID: 2361
		// (get) Token: 0x0600202C RID: 8236 RVA: 0x000DAEF9 File Offset: 0x000D90F9
		// (set) Token: 0x0600202D RID: 8237 RVA: 0x000DAF01 File Offset: 0x000D9101
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character(s) whose skill to check. If there are multiple targets, the action succeeds if any of their skill checks succeeds.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x0600202E RID: 8238 RVA: 0x000DAF0C File Offset: 0x000D910C
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

		// Token: 0x0600202F RID: 8239 RVA: 0x000DAF7C File Offset: 0x000D917C
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

		// Token: 0x06002030 RID: 8240 RVA: 0x000DB01C File Offset: 0x000D921C
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
