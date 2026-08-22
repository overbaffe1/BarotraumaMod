using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000071 RID: 113
	internal class PvPMode : MissionMode
	{
		// Token: 0x06001089 RID: 4233 RVA: 0x0009D30C File Offset: 0x0009B50C
		private void InitUI()
		{
			this.scoreContainer = new GUILayoutGroup(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.TutorialObjectiveListArea, GUI.Canvas), false, Anchor.TopRight)
			{
				CanBeFocused = false
			};
			for (int i = 0; i < 2; i++)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.scoreContainer.Rect.Width, GUI.IntScale(80f)), this.scoreContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null)
				{
					CanBeFocused = false
				};
				new GUIImage(new RectTransform(Vector2.One, frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight), (i == 0) ? "CoalitionIcon" : "SeparatistIcon", GUIImage.ScalingMode.None).CanBeFocused = false;
				this.scoreTextShadows[i] = new GUITextBlock(new RectTransform(Vector2.One, frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight)
				{
					AbsoluteOffset = new Point(GUI.IntScale(38f), GUI.IntScale(2f))
				}, string.Empty, new Color?(GUIStyle.TextColorDark), GUIStyle.SubHeadingFont, Alignment.CenterRight, false, "", null)
				{
					CanBeFocused = false
				};
				GUITextBlock[] array = this.scoreTexts;
				int num = i;
				RectTransform rectTransform = new RectTransform(Vector2.One, frame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight);
				rectTransform.AbsoluteOffset = new Point(GUI.IntScale(40f), 0);
				RichString text = string.Empty;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				array[num] = new GUITextBlock(rectTransform, text, null, subHeadingFont, Alignment.CenterRight, false, "", null)
				{
					CanBeFocused = false
				};
			}
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x0009D504 File Offset: 0x0009B704
		public override void AddToGUIUpdateList()
		{
			base.AddToGUIUpdateList();
			if (this.scoreContainer == null)
			{
				this.InitUI();
			}
			this.scoreContainer.Visible = false;
			foreach (Mission mission in this.Missions)
			{
				CombatMission combatMission = mission as CombatMission;
				if (combatMission != null && combatMission.HasWinScore)
				{
					for (int i = 0; i < 2; i++)
					{
						GUITextBlock scoreText = this.scoreTexts[i];
						if (((float)combatMission.Scores[i] > (float)combatMission.WinScore * 0.9f || combatMission.Scores[i] == combatMission.WinScore - 1) && scoreText.Parent.FlashTimer <= 0f)
						{
							scoreText.Parent.Flash(new Color?(GUIStyle.Orange), 1.5f, false, false, null);
							scoreText.Pulsate(Vector2.One, Vector2.One * 1.2f, scoreText.Parent.FlashTimer);
						}
						if (this.prevScores[i] != combatMission.Scores[i] || scoreText.Text.IsNullOrEmpty())
						{
							GUITextBlock guitextBlock = scoreText;
							GUITextBlock guitextBlock2 = this.scoreTextShadows[i];
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
							defaultInterpolatedStringHandler.AppendFormatted<int>(combatMission.Scores[i]);
							defaultInterpolatedStringHandler.AppendLiteral("/");
							defaultInterpolatedStringHandler.AppendFormatted<int>(combatMission.WinScore);
							guitextBlock.Text = (guitextBlock2.Text = defaultInterpolatedStringHandler.ToStringAndClear());
							scoreText.Parent.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
							scoreText.Parent.GetAnyChild<GUIImage>().Pulsate(Vector2.One, Vector2.One * 1.2f, scoreText.Parent.FlashTimer);
							SoundPlayer.PlayUISound(GUISoundType.UIMessage);
						}
						scoreText.Parent.RectTransform.NonScaledSize = new Point((int)(scoreText.TextSize.X + scoreText.Padding.X + scoreText.Padding.X) + scoreText.Parent.GetChild<GUIImage>().Rect.Width + GUI.IntScale(10f), scoreText.Parent.Rect.Height);
						scoreText.Parent.ForceLayoutRecalculation();
						this.prevScores[i] = combatMission.Scores[i];
					}
					this.scoreContainer.Visible = true;
				}
			}
			this.scoreContainer.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x0009D7C4 File Offset: 0x0009B9C4
		public PvPMode(GameModePreset preset, IEnumerable<MissionPrefab> missionPrefabs) : base(preset, MissionMode.ValidateMissionPrefabs(missionPrefabs, MissionPrefab.PvPMissionClasses))
		{
			if (this.Missions.None(null))
			{
				throw new Exception("Attempted to start PvPMode without a mission.");
			}
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x0009D820 File Offset: 0x0009BA20
		public PvPMode(GameModePreset preset, IEnumerable<Identifier> missionTypes, string seed) : base(preset, MissionMode.ValidateMissionTypes(missionTypes, MissionPrefab.PvPMissionClasses), seed)
		{
			if (this.Missions.None(null))
			{
				throw new Exception("Attempted to start PvPMode without a mission.");
			}
		}

		// Token: 0x04000819 RID: 2073
		private GUIComponent scoreContainer;

		// Token: 0x0400081A RID: 2074
		private readonly GUITextBlock[] scoreTexts = new GUITextBlock[2];

		// Token: 0x0400081B RID: 2075
		private readonly GUITextBlock[] scoreTextShadows = new GUITextBlock[2];

		// Token: 0x0400081C RID: 2076
		private readonly int[] prevScores = new int[2];
	}
}
