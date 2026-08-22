using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000FF RID: 255
	internal class CampaignEndScreen : Screen
	{
		// Token: 0x06002406 RID: 9222 RVA: 0x00168978 File Offset: 0x00166B78
		public CampaignEndScreen()
		{
			this.creditsPlayer = new CreditsPlayer(new RectTransform(Vector2.One, this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Content/Texts/Credits.xml")
			{
				AutoRestart = false,
				ScrollBarEnabled = false,
				AllowMouseWheelScroll = false
			};
			this.creditsPlayer.CloseButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				this.creditsPlayer.Scroll = 1f;
				return true;
			};
			this.cam = new Camera();
		}

		// Token: 0x06002407 RID: 9223 RVA: 0x00168A0C File Offset: 0x00166C0C
		public override void Select()
		{
			base.Select();
			SlideshowPrefab slideshow;
			if (SlideshowPrefab.Prefabs.TryGet("campaignending".ToIdentifier(), out slideshow))
			{
				this.slideshowPlayer = new SlideshowPlayer(GUICanvas.Instance, slideshow);
			}
			this.creditsPlayer.Restart();
			this.creditsPlayer.Visible = false;
			CampaignEndScreen.<Select>g__UnlockAchievement|5_0("campaigncompleted");
			GameSession gameSession = GameMain.GameSession;
			string id;
			if (gameSession != null)
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign != null)
				{
					CampaignSettings settings = campaign.Settings;
					if (settings != null && settings.RadiationEnabled)
					{
						id = "campaigncompleted_radiationenabled";
						goto IL_81;
					}
				}
			}
			id = "campaigncompleted_radiationdisabled";
			IL_81:
			CampaignEndScreen.<Select>g__UnlockAchievement|5_0(id);
		}

		// Token: 0x06002408 RID: 9224 RVA: 0x00168A9F File Offset: 0x00166C9F
		public override void Deselect()
		{
			GUI.HideCursor = false;
			SoundPlayer.OverrideMusicType = Identifier.Empty;
		}

		// Token: 0x06002409 RID: 9225 RVA: 0x00168AB1 File Offset: 0x00166CB1
		public override void Update(double deltaTime)
		{
			SlideshowPlayer slideshowPlayer = this.slideshowPlayer;
			if (slideshowPlayer != null)
			{
				slideshowPlayer.UpdateManually((float)deltaTime, false, true);
			}
			if (this.creditsPlayer.Finished)
			{
				Action onFinished = this.OnFinished;
				if (onFinished != null)
				{
					onFinished();
				}
				SoundPlayer.OverrideMusicType = Identifier.Empty;
			}
		}

		// Token: 0x0600240A RID: 9226 RVA: 0x00168AF0 File Offset: 0x00166CF0
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			graphics.Clear(Color.Black);
			SoundPlayer.OverrideMusicType = "ending".ToIdentifier();
			if (this.slideshowPlayer != null && !this.slideshowPlayer.Finished)
			{
				this.slideshowPlayer.DrawManually(spriteBatch, false, true);
			}
			else
			{
				GUI.HideCursor = false;
				this.creditsPlayer.Visible = true;
			}
			GUI.Draw(this.cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x0600240C RID: 9228 RVA: 0x00168B90 File Offset: 0x00166D90
		[CompilerGenerated]
		internal static void <Select>g__UnlockAchievement|5_0(string id)
		{
			AchievementManager.UnlockAchievement(id.ToIdentifier(), true, null, null);
		}

		// Token: 0x040011F4 RID: 4596
		private readonly CreditsPlayer creditsPlayer;

		// Token: 0x040011F5 RID: 4597
		private readonly Camera cam;

		// Token: 0x040011F6 RID: 4598
		public Action OnFinished;

		// Token: 0x040011F7 RID: 4599
		protected SlideshowPlayer slideshowPlayer;
	}
}
