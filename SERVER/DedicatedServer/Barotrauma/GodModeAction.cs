using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200019F RID: 415
	internal class GodModeAction : EventAction
	{
		// Token: 0x170008E6 RID: 2278
		// (get) Token: 0x06001F19 RID: 7961 RVA: 0x000D80DE File Offset: 0x000D62DE
		// (set) Token: 0x06001F1A RID: 7962 RVA: 0x000D80E6 File Offset: 0x000D62E6
		[Serialize(true, IsPropertySaveable.Yes, "Should the godmode be enabled or disabled?", "", false)]
		public bool Enabled { get; set; }

		// Token: 0x170008E7 RID: 2279
		// (get) Token: 0x06001F1B RID: 7963 RVA: 0x000D80EF File Offset: 0x000D62EF
		// (set) Token: 0x06001F1C RID: 7964 RVA: 0x000D80F7 File Offset: 0x000D62F7
		[Serialize(false, IsPropertySaveable.Yes, "Should the character's active afflictions be updated (e.g. applying visual effects of the afflictions)", "", false)]
		public bool UpdateAfflictions { get; set; }

		// Token: 0x170008E8 RID: 2280
		// (get) Token: 0x06001F1D RID: 7965 RVA: 0x000D8100 File Offset: 0x000D6300
		// (set) Token: 0x06001F1E RID: 7966 RVA: 0x000D8108 File Offset: 0x000D6308
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character whose godmode to enable/disable.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06001F1F RID: 7967 RVA: 0x000D8111 File Offset: 0x000D6311
		public GodModeAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F20 RID: 7968 RVA: 0x000D811B File Offset: 0x000D631B
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x000D8123 File Offset: 0x000D6323
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x000D812C File Offset: 0x000D632C
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

		// Token: 0x06001F23 RID: 7971 RVA: 0x000D81D8 File Offset: 0x000D63D8
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

		// Token: 0x04000EDC RID: 3804
		private bool isFinished;
	}
}
