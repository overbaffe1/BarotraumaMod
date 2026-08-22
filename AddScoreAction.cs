using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000276 RID: 630
	internal class AddScoreAction : EventAction
	{
		// Token: 0x17000ECE RID: 3790
		// (get) Token: 0x0600387A RID: 14458 RVA: 0x002180F8 File Offset: 0x002162F8
		// (set) Token: 0x0600387B RID: 14459 RVA: 0x00218100 File Offset: 0x00216300
		[Serialize("", IsPropertySaveable.Yes, "Tag of a target (character) whose team the score should be given to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000ECF RID: 3791
		// (get) Token: 0x0600387C RID: 14460 RVA: 0x00218109 File Offset: 0x00216309
		// (set) Token: 0x0600387D RID: 14461 RVA: 0x00218111 File Offset: 0x00216311
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "Which team's score to add to? Ignored if TargetTag is set.", "", false)]
		public CharacterTeamType Team { get; set; }

		// Token: 0x17000ED0 RID: 3792
		// (get) Token: 0x0600387E RID: 14462 RVA: 0x0021811A File Offset: 0x0021631A
		// (set) Token: 0x0600387F RID: 14463 RVA: 0x00218122 File Offset: 0x00216322
		[Serialize(1, IsPropertySaveable.Yes, "How much to add to the score? Can also be negative.", "", false)]
		public int Amount { get; set; }

		// Token: 0x06003880 RID: 14464 RVA: 0x0021812C File Offset: 0x0021632C
		public AddScoreAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Amount == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("AddScoreAction");
				defaultInterpolatedStringHandler.AppendLiteral(", event ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(": score set to 0, the action will do nothing.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			if (this.TargetTag.IsEmpty && this.Team == CharacterTeamType.None)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 4);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler2.AppendFormatted("AddScoreAction");
				defaultInterpolatedStringHandler2.AppendLiteral(", event ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(": neither ");
				defaultInterpolatedStringHandler2.AppendFormatted("Team");
				defaultInterpolatedStringHandler2.AppendLiteral(" or ");
				defaultInterpolatedStringHandler2.AppendFormatted("TargetTag");
				defaultInterpolatedStringHandler2.AppendLiteral(" is set.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06003881 RID: 14465 RVA: 0x00218259 File Offset: 0x00216459
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003882 RID: 14466 RVA: 0x00218261 File Offset: 0x00216461
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003883 RID: 14467 RVA: 0x0021826C File Offset: 0x0021646C
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			CharacterTeamType targetTeam = CharacterTeamType.None;
			if (this.TargetTag.IsEmpty)
			{
				targetTeam = this.Team;
			}
			else
			{
				foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
				{
					Character character = target as Character;
					if (character != null)
					{
						targetTeam = character.TeamID;
						break;
					}
				}
			}
			if (targetTeam == CharacterTeamType.None)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x06003884 RID: 14468 RVA: 0x00218304 File Offset: 0x00216504
		public override string ToDebugString()
		{
			string text;
			if (!this.TargetTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler.AppendLiteral("target: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TargetTag);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				text = "team: " + this.Team.ColorizeObject();
			}
			string target = text;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 4);
			defaultInterpolatedStringHandler2.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler2.AppendLiteral(" ");
			defaultInterpolatedStringHandler2.AppendFormatted("AddScoreAction");
			defaultInterpolatedStringHandler2.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler2.AppendFormatted(target);
			defaultInterpolatedStringHandler2.AppendLiteral(", amount: ");
			defaultInterpolatedStringHandler2.AppendFormatted(this.Amount.ColorizeObject());
			defaultInterpolatedStringHandler2.AppendLiteral(")");
			return defaultInterpolatedStringHandler2.ToStringAndClear();
		}

		// Token: 0x04001D50 RID: 7504
		private bool isFinished;
	}
}
