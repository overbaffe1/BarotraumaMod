using System;
using Barotrauma.Tutorials;

namespace Barotrauma
{
	// Token: 0x0200006C RID: 108
	internal class TutorialMode : GameMode
	{
		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06000F88 RID: 3976 RVA: 0x000940CD File Offset: 0x000922CD
		public override bool Paused
		{
			get
			{
				return this.Tutorial.Paused;
			}
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x000940DA File Offset: 0x000922DA
		public TutorialMode(GameModePreset preset) : base(preset)
		{
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x000940E4 File Offset: 0x000922E4
		public override void Start()
		{
			base.Start();
			GameMain.GameSession.CrewManager = new CrewManager(true);
			foreach (Item item in Item.ItemList)
			{
				item.SpawnedInCurrentOutpost = false;
			}
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x0009414C File Offset: 0x0009234C
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			this.Tutorial.Update();
		}

		// Token: 0x040007CA RID: 1994
		public Tutorial Tutorial;
	}
}
