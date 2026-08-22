using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma.Tutorials
{
	// Token: 0x02000628 RID: 1576
	internal sealed class Tutorial
	{
		// Token: 0x1700197F RID: 6527
		// (get) Token: 0x060064B2 RID: 25778 RVA: 0x003420E3 File Offset: 0x003402E3
		public LocalizedString DisplayName { get; }

		// Token: 0x17001980 RID: 6528
		// (get) Token: 0x060064B3 RID: 25779 RVA: 0x003420EB File Offset: 0x003402EB
		public LocalizedString Description { get; }

		// Token: 0x17001981 RID: 6529
		// (get) Token: 0x060064B4 RID: 25780 RVA: 0x003420F3 File Offset: 0x003402F3
		// (set) Token: 0x060064B5 RID: 25781 RVA: 0x003420FB File Offset: 0x003402FB
		public bool Completed
		{
			get
			{
				return this.completed;
			}
			private set
			{
				if (this.completed == value)
				{
					return;
				}
				this.completed = value;
				if (value)
				{
					CompletedTutorials.Instance.Add(this.Identifier);
				}
				GameSettings.SaveCurrentConfig();
			}
		}

		// Token: 0x17001982 RID: 6530
		// (get) Token: 0x060064B6 RID: 25782 RVA: 0x00342126 File Offset: 0x00340326
		private string SubmarinePath
		{
			get
			{
				return this.TutorialPrefab.SubmarinePath.Value;
			}
		}

		// Token: 0x17001983 RID: 6531
		// (get) Token: 0x060064B7 RID: 25783 RVA: 0x00342138 File Offset: 0x00340338
		private string StartOutpostPath
		{
			get
			{
				return this.TutorialPrefab.OutpostPath.Value;
			}
		}

		// Token: 0x17001984 RID: 6532
		// (get) Token: 0x060064B8 RID: 25784 RVA: 0x0034214A File Offset: 0x0034034A
		private string LevelSeed
		{
			get
			{
				return this.TutorialPrefab.LevelSeed;
			}
		}

		// Token: 0x17001985 RID: 6533
		// (get) Token: 0x060064B9 RID: 25785 RVA: 0x00342157 File Offset: 0x00340357
		private string LevelParams
		{
			get
			{
				return this.TutorialPrefab.LevelParams;
			}
		}

		// Token: 0x17001986 RID: 6534
		// (get) Token: 0x060064BA RID: 25786 RVA: 0x00342164 File Offset: 0x00340364
		// (set) Token: 0x060064BB RID: 25787 RVA: 0x0034216C File Offset: 0x0034036C
		public bool Paused { get; private set; }

		// Token: 0x060064BC RID: 25788 RVA: 0x00342178 File Offset: 0x00340378
		public Tutorial(TutorialPrefab prefab)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("tutorial.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
			this.Identifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			this.DisplayName = TextManager.Get(this.Identifier);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("tutorial.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
			defaultInterpolatedStringHandler2.AppendLiteral(".description");
			this.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			this.TutorialPrefab = prefab;
			this.eventPrefab = EventSet.GetEventPrefab(prefab.EventIdentifier);
		}

		// Token: 0x060064BD RID: 25789 RVA: 0x00342235 File Offset: 0x00340435
		private IEnumerable<CoroutineStatus> Loading()
		{
			Tutorial.<Loading>d__34 <Loading>d__ = new Tutorial.<Loading>d__34(-2);
			<Loading>d__.<>4__this = this;
			return <Loading>d__;
		}

		// Token: 0x060064BE RID: 25790 RVA: 0x00342248 File Offset: 0x00340448
		public void Start()
		{
			GameMain.Instance.ShowLoading(this.Loading(), true);
			ObjectiveManager.ResetObjectives();
			foreach (Item item in Item.ItemList)
			{
				Door door = item.GetComponent<Door>();
				if (door != null)
				{
					if (door.RequiredItems.Values.None((List<RelatedItem> ris) => ris.None((RelatedItem ri) => ri.Identifiers.None((Identifier i) => i == "locked"))))
					{
						door.RequiredItems.Clear();
					}
				}
			}
		}

		// Token: 0x060064BF RID: 25791 RVA: 0x003422F0 File Offset: 0x003404F0
		public void Update()
		{
			if (this.character != null)
			{
				if (this.character.Oxygen < 1f)
				{
					this.character.Oxygen = 1f;
				}
				if (this.character.IsDead)
				{
					CoroutineManager.StartCoroutine(this.Dead(), "");
					return;
				}
				if (Character.Controlled == null)
				{
					if (this.tutorialCoroutine != null)
					{
						CoroutineManager.StopCoroutines(this.tutorialCoroutine);
					}
					if (this.completedCoroutine == null && !CoroutineManager.IsCoroutineRunning(this.completedCoroutine))
					{
						GUI.PreventPauseMenuToggle = false;
					}
					ObjectiveManager.ClearContent();
					return;
				}
				this.character = Character.Controlled;
			}
		}

		// Token: 0x060064C0 RID: 25792 RVA: 0x0034238F File Offset: 0x0034058F
		private IEnumerable<CoroutineStatus> Dead()
		{
			Tutorial.<Dead>d__37 <Dead>d__ = new Tutorial.<Dead>d__37(-2);
			<Dead>d__.<>4__this = this;
			return <Dead>d__;
		}

		// Token: 0x060064C1 RID: 25793 RVA: 0x0034239F File Offset: 0x0034059F
		public IEnumerable<CoroutineStatus> UpdateState()
		{
			Tutorial.<UpdateState>d__38 <UpdateState>d__ = new Tutorial.<UpdateState>d__38(-2);
			<UpdateState>d__.<>4__this = this;
			return <UpdateState>d__;
		}

		// Token: 0x060064C2 RID: 25794 RVA: 0x003423B0 File Offset: 0x003405B0
		public void Complete()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Tutorial:");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(":Completed");
			GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
			this.completedCoroutine = CoroutineManager.StartCoroutine(this.<Complete>g__TutorialCompleted|39_0(), "");
		}

		// Token: 0x060064C3 RID: 25795 RVA: 0x0034240E File Offset: 0x0034060E
		private bool Restart(GUIButton button, object obj)
		{
			GUIMessageBox.MessageBoxes.Clear();
			GameMain.MainMenuScreen.ReturnToMainMenu(button, obj);
			this.Start();
			return true;
		}

		// Token: 0x060064C4 RID: 25796 RVA: 0x0034242E File Offset: 0x0034062E
		public void Stop()
		{
			if (this.tutorialCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.tutorialCoroutine);
			}
			ObjectiveManager.ResetUI();
		}

		// Token: 0x060064C5 RID: 25797 RVA: 0x00342448 File Offset: 0x00340648
		[CompilerGenerated]
		private IEnumerable<CoroutineStatus> <Complete>g__TutorialCompleted|39_0()
		{
			Tutorial.<<Complete>g__TutorialCompleted|39_0>d <<Complete>g__TutorialCompleted|39_0>d = new Tutorial.<<Complete>g__TutorialCompleted|39_0>d(-2);
			<<Complete>g__TutorialCompleted|39_0>d.<>4__this = this;
			return <<Complete>g__TutorialCompleted|39_0>d;
		}

		// Token: 0x0400344D RID: 13389
		private const SpawnType SpawnPointType = SpawnType.Human;

		// Token: 0x0400344E RID: 13390
		private const float FadeOutTime = 3f;

		// Token: 0x0400344F RID: 13391
		private const float WaitBeforeFade = 4f;

		// Token: 0x04003450 RID: 13392
		public readonly Identifier Identifier;

		// Token: 0x04003453 RID: 13395
		private bool completed;

		// Token: 0x04003454 RID: 13396
		public readonly TutorialPrefab TutorialPrefab;

		// Token: 0x04003455 RID: 13397
		private readonly EventPrefab eventPrefab;

		// Token: 0x04003456 RID: 13398
		private CoroutineHandle tutorialCoroutine;

		// Token: 0x04003457 RID: 13399
		private CoroutineHandle completedCoroutine;

		// Token: 0x04003458 RID: 13400
		private Character character;

		// Token: 0x04003459 RID: 13401
		private SubmarineInfo startOutpost;

		// Token: 0x0400345A RID: 13402
		[TupleElementNames(new string[]
		{
			"entity",
			"iconStyle"
		})]
		public readonly List<ValueTuple<Entity, Identifier>> Icons = new List<ValueTuple<Entity, Identifier>>();
	}
}
