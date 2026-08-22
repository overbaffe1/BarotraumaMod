using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x020001E4 RID: 484
	internal class GameMode
	{
		// Token: 0x17000A0D RID: 2573
		// (get) Token: 0x060022EE RID: 8942 RVA: 0x000E9770 File Offset: 0x000E7970
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

		// Token: 0x17000A0E RID: 2574
		// (get) Token: 0x060022EF RID: 8943 RVA: 0x000E9782 File Offset: 0x000E7982
		public virtual IEnumerable<Mission> Missions
		{
			get
			{
				return Enumerable.Empty<Mission>();
			}
		}

		// Token: 0x17000A0F RID: 2575
		// (get) Token: 0x060022F0 RID: 8944 RVA: 0x000E9789 File Offset: 0x000E7989
		public bool IsSinglePlayer
		{
			get
			{
				return this.preset.IsSinglePlayer;
			}
		}

		// Token: 0x17000A10 RID: 2576
		// (get) Token: 0x060022F1 RID: 8945 RVA: 0x000E9796 File Offset: 0x000E7996
		public LocalizedString Name
		{
			get
			{
				return this.preset.Name;
			}
		}

		// Token: 0x17000A11 RID: 2577
		// (get) Token: 0x060022F2 RID: 8946 RVA: 0x000E97A3 File Offset: 0x000E79A3
		public virtual bool Paused
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060022F3 RID: 8947 RVA: 0x000E97A6 File Offset: 0x000E79A6
		public virtual void UpdateWhilePaused(float deltaTime)
		{
		}

		// Token: 0x17000A12 RID: 2578
		// (get) Token: 0x060022F4 RID: 8948 RVA: 0x000E97A8 File Offset: 0x000E79A8
		public GameModePreset Preset
		{
			get
			{
				return this.preset;
			}
		}

		// Token: 0x060022F5 RID: 8949 RVA: 0x000E97B0 File Offset: 0x000E79B0
		public GameMode(GameModePreset preset)
		{
			this.preset = preset;
		}

		// Token: 0x060022F6 RID: 8950 RVA: 0x000E97BF File Offset: 0x000E79BF
		public virtual void Start()
		{
			this.startTime = DateTime.Now;
		}

		// Token: 0x060022F7 RID: 8951 RVA: 0x000E97CC File Offset: 0x000E79CC
		public virtual void ShowStartMessage()
		{
		}

		// Token: 0x060022F8 RID: 8952 RVA: 0x000E97CE File Offset: 0x000E79CE
		public virtual void AddExtraMissions(LevelData levelData)
		{
		}

		// Token: 0x060022F9 RID: 8953 RVA: 0x000E97D0 File Offset: 0x000E79D0
		public virtual void AddToGUIUpdateList()
		{
		}

		// Token: 0x060022FA RID: 8954 RVA: 0x000E97D2 File Offset: 0x000E79D2
		public virtual void Update(float deltaTime)
		{
			CrewManager crewManager = this.CrewManager;
			if (crewManager == null)
			{
				return;
			}
			crewManager.Update(deltaTime);
		}

		// Token: 0x060022FB RID: 8955 RVA: 0x000E97E5 File Offset: 0x000E79E5
		public virtual void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
		}

		// Token: 0x060022FC RID: 8956 RVA: 0x000E97E7 File Offset: 0x000E79E7
		public virtual void Remove()
		{
		}

		// Token: 0x040010DC RID: 4316
		public static List<GameModePreset> PresetList = new List<GameModePreset>();

		// Token: 0x040010DD RID: 4317
		protected DateTime startTime;

		// Token: 0x040010DE RID: 4318
		protected GameModePreset preset;
	}
}
