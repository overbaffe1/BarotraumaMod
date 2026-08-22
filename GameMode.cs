using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000067 RID: 103
	internal class GameMode
	{
		// Token: 0x06000EA3 RID: 3747 RVA: 0x0008AA41 File Offset: 0x00088C41
		public virtual void HUDScaleChanged()
		{
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x0008AA43 File Offset: 0x00088C43
		public virtual void Draw(SpriteBatch spriteBatch)
		{
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06000EA5 RID: 3749 RVA: 0x0008AA45 File Offset: 0x00088C45
		public CrewManager CrewManager
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return null;
				}
				return gameSession.CrewManager;
			}
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06000EA6 RID: 3750 RVA: 0x0008AA57 File Offset: 0x00088C57
		public virtual IEnumerable<Mission> Missions
		{
			get
			{
				return Enumerable.Empty<Mission>();
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06000EA7 RID: 3751 RVA: 0x0008AA5E File Offset: 0x00088C5E
		public bool IsSinglePlayer
		{
			get
			{
				return this.preset.IsSinglePlayer;
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06000EA8 RID: 3752 RVA: 0x0008AA6B File Offset: 0x00088C6B
		public LocalizedString Name
		{
			get
			{
				return this.preset.Name;
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06000EA9 RID: 3753 RVA: 0x0008AA78 File Offset: 0x00088C78
		public virtual bool Paused
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x0008AA7B File Offset: 0x00088C7B
		public virtual void UpdateWhilePaused(float deltaTime)
		{
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06000EAB RID: 3755 RVA: 0x0008AA7D File Offset: 0x00088C7D
		public GameModePreset Preset
		{
			get
			{
				return this.preset;
			}
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x0008AA85 File Offset: 0x00088C85
		public GameMode(GameModePreset preset)
		{
			this.preset = preset;
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x0008AA94 File Offset: 0x00088C94
		public virtual void Start()
		{
			this.startTime = DateTime.Now;
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x0008AAA1 File Offset: 0x00088CA1
		public virtual void ShowStartMessage()
		{
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x0008AAA3 File Offset: 0x00088CA3
		public virtual void AddExtraMissions(LevelData levelData)
		{
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x0008AAA5 File Offset: 0x00088CA5
		public virtual void AddToGUIUpdateList()
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null)
			{
				return;
			}
			gameSession.CrewManager.AddToGUIUpdateList();
		}

		// Token: 0x06000EB1 RID: 3761 RVA: 0x0008AABB File Offset: 0x00088CBB
		public virtual void Update(float deltaTime)
		{
			CrewManager crewManager = this.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.Update(deltaTime);
		}

		// Token: 0x06000EB2 RID: 3762 RVA: 0x0008AACE File Offset: 0x00088CCE
		public virtual void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
		}

		// Token: 0x06000EB3 RID: 3763 RVA: 0x0008AAD0 File Offset: 0x00088CD0
		public virtual void Remove()
		{
		}

		// Token: 0x0400077A RID: 1914
		public static List<GameModePreset> PresetList = new List<GameModePreset>();

		// Token: 0x0400077B RID: 1915
		protected DateTime startTime;

		// Token: 0x0400077C RID: 1916
		protected GameModePreset preset;
	}
}
