using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000182 RID: 386
	internal class AddScoreAction : EventAction
	{
		// Token: 0x1700087A RID: 2170
		// (get) Token: 0x06001DAE RID: 7598 RVA: 0x000D2ADC File Offset: 0x000D0CDC
		// (set) Token: 0x06001DAF RID: 7599 RVA: 0x000D2AE4 File Offset: 0x000D0CE4
		[Serialize("", IsPropertySaveable.Yes, "Tag of a target (character) whose team the score should be given to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x1700087B RID: 2171
		// (get) Token: 0x06001DB0 RID: 7600 RVA: 0x000D2AED File Offset: 0x000D0CED
		// (set) Token: 0x06001DB1 RID: 7601 RVA: 0x000D2AF5 File Offset: 0x000D0CF5
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "Which team's score to add to? Ignored if TargetTag is set.", "", false)]
		public CharacterTeamType Team { get; set; }

		// Token: 0x1700087C RID: 2172
		// (get) Token: 0x06001DB2 RID: 7602 RVA: 0x000D2AFE File Offset: 0x000D0CFE
		// (set) Token: 0x06001DB3 RID: 7603 RVA: 0x000D2B06 File Offset: 0x000D0D06
		[Serialize(1, IsPropertySaveable.Yes, "How much to add to the score? Can also be negative.", "", false)]
		public int Amount { get; set; }

		// Token: 0x06001DB4 RID: 7604 RVA: 0x000D2B10 File Offset: 0x000D0D10
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

		// Token: 0x06001DB5 RID: 7605 RVA: 0x000D2C3D File Offset: 0x000D0E3D
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x000D2C45 File Offset: 0x000D0E45
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001DB7 RID: 7607 RVA: 0x000D2C50 File Offset: 0x000D0E50
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
			GameSession gameSession = GameMain.GameSession;
			IEnumerable<Mission> missions = (gameSession != null) ? gameSession.Missions : null;
			if (missions != null)
			{
				foreach (Mission mission in missions)
				{
					CombatMission combatMission = mission as CombatMission;
					if (combatMission != null)
					{
						combatMission.AddToScore(targetTeam, this.Amount);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x000D2D50 File Offset: 0x000D0F50
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

		// Token: 0x04000E59 RID: 3673
		private bool isFinished;
	}
}
