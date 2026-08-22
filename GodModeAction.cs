using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000292 RID: 658
	internal class GodModeAction : EventAction
	{
		// Token: 0x17000F3A RID: 3898
		// (get) Token: 0x060039E3 RID: 14819 RVA: 0x0021D592 File Offset: 0x0021B792
		// (set) Token: 0x060039E4 RID: 14820 RVA: 0x0021D59A File Offset: 0x0021B79A
		[Serialize(true, IsPropertySaveable.Yes, "Should the godmode be enabled or disabled?", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x17000F3B RID: 3899
		// (get) Token: 0x060039E5 RID: 14821 RVA: 0x0021D5A3 File Offset: 0x0021B7A3
		// (set) Token: 0x060039E6 RID: 14822 RVA: 0x0021D5AB File Offset: 0x0021B7AB
		[Serialize(false, IsPropertySaveable.Yes, "Should the character's active afflictions be updated (e.g. applying visual effects of the afflictions)", "", false)]
		public bool UpdateAfflictions { get; set; }

		// Token: 0x17000F3C RID: 3900
		// (get) Token: 0x060039E7 RID: 14823 RVA: 0x0021D5B4 File Offset: 0x0021B7B4
		// (set) Token: 0x060039E8 RID: 14824 RVA: 0x0021D5BC File Offset: 0x0021B7BC
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character whose godmode to enable/disable.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060039E9 RID: 14825 RVA: 0x0021D5C5 File Offset: 0x0021B7C5
		public GodModeAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060039EA RID: 14826 RVA: 0x0021D5CF File Offset: 0x0021B7CF
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060039EB RID: 14827 RVA: 0x0021D5D7 File Offset: 0x0021B7D7
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039EC RID: 14828 RVA: 0x0021D5E0 File Offset: 0x0021B7E0
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
			foreach (Entity target in targets)
			{
				if (target != null)
				{
					Character character = target as Character;
					if (character != null)
					{
						if (this.Enabled)
						{
							if (this.UpdateAfflictions)
							{
								character.CharacterHealth.Unkillable = true;
							}
							else
							{
								character.GodMode = true;
							}
						}
						else
						{
							character.CharacterHealth.Unkillable = false;
							character.GodMode = false;
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x060039ED RID: 14829 RVA: 0x0021D68C File Offset: 0x0021B88C
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("GodModeAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			return defaultInterpolatedStringHandler.ToStringAndClear() + (this.Enabled ? "Enable godmode" : "Disable godmode");
		}

		// Token: 0x04001DD3 RID: 7635
		private bool isFinished;
	}
}
