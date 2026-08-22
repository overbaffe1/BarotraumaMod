using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200006B RID: 107
	internal class TestGameMode : GameMode
	{
		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000F7F RID: 3967 RVA: 0x00093B2A File Offset: 0x00091D2A
		// (set) Token: 0x06000F80 RID: 3968 RVA: 0x00093B32 File Offset: 0x00091D32
		public LevelGenerationParams BackgroundParams { get; private set; }

		// Token: 0x06000F81 RID: 3969 RVA: 0x00093B3C File Offset: 0x00091D3C
		public override void Start()
		{
			base.Start();
			base.CrewManager.InitSinglePlayerRound();
			foreach (Submarine submarine in Submarine.Loaded)
			{
				submarine.NeutralizeBallast();
				SubmarineType type = submarine.Info.Type;
				if (type - SubmarineType.Outpost <= 3)
				{
					submarine.PhysicsBody.BodyType = BodyType.Static;
					if (submarine.Info.ShouldBeRuin)
					{
						submarine.Info.Type = SubmarineType.Ruin;
					}
					submarine.TeamID = (submarine.Info.IsOutpost ? CharacterTeamType.FriendlyNPC : CharacterTeamType.None);
				}
			}
			if (this.SpawnOutpost)
			{
				this.GenerateOutpost(Submarine.MainSub);
			}
			if (this.TriggeredEvent != null)
			{
				this.scriptedEvent = new List<Event>
				{
					this.TriggeredEvent.CreateInstance(GameMain.GameSession.EventManager.RandomSeed)
				};
				GameMain.GameSession.EventManager.PinnedEvent = this.scriptedEvent.Last<Event>();
				this.createEventButton = new GUIButton(new RectTransform(new Point(128, 64), GUI.Canvas, Anchor.TopCenter, null, ScaleBasis.Normal, false)
				{
					ScreenSpaceOffset = new Point(0, 32)
				}, TextManager.Get("create"), Alignment.Center, "", null)
				{
					OnClicked = delegate(GUIButton <p0>, object <p1>)
					{
						this.scriptedEvent.Add(this.TriggeredEvent.CreateInstance(GameMain.GameSession.EventManager.RandomSeed));
						GameMain.GameSession.EventManager.PinnedEvent = this.scriptedEvent.Last<Event>();
						return true;
					}
				};
			}
			if (Level.Loaded == null)
			{
				if (this.BackgroundParams == null)
				{
					this.BackgroundParams = (from lp in LevelGenerationParams.LevelParams
					where !lp.AllowedBiomeIdentifiers.Contains("endzone")
					select lp).GetRandom(Rand.RandSync.Unsynced);
				}
				GameMain.LightManager.AmbientLight = this.BackgroundParams.AmbientLightColor;
			}
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x00093D14 File Offset: 0x00091F14
		public override void AddToGUIUpdateList()
		{
			base.AddToGUIUpdateList();
			GUIButton guibutton = this.createEventButton;
			if (guibutton == null)
			{
				return;
			}
			guibutton.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x06000F83 RID: 3971 RVA: 0x00093D2E File Offset: 0x00091F2E
		public override void End(CampaignMode.TransitionType transitionType = CampaignMode.TransitionType.None)
		{
			GameMain.GameSession.EventManager.PinnedEvent = null;
			Action onRoundEnd = this.OnRoundEnd;
			if (onRoundEnd == null)
			{
				return;
			}
			onRoundEnd();
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x00093D50 File Offset: 0x00091F50
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.scriptedEvent != null)
			{
				foreach (Event sEvent2 in from sEvent in this.scriptedEvent
				where !sEvent.IsFinished
				select sEvent)
				{
					sEvent2.Update(deltaTime);
				}
			}
			LevelGenerationParams backgroundParams = this.BackgroundParams;
			if (backgroundParams == null)
			{
				return;
			}
			backgroundParams.UpdateWaterParticleOffset(ref this.WaterParticleOffset, this.BackgroundParams.WaterParticleVelocity, deltaTime);
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x00093DF4 File Offset: 0x00091FF4
		private void GenerateOutpost(Submarine submarine)
		{
			Submarine outpost = OutpostGenerator.Generate(this.OutpostParams ?? OutpostGenerationParams.OutpostParams.GetRandomUnsynced<OutpostGenerationParams>(), this.OutpostType ?? LocationType.Prefabs.GetRandomUnsynced<LocationType>(), false, false);
			outpost.SetPosition(Vector2.Zero, null, true);
			float closestDistance = 0f;
			DockingPort myPort = null;
			DockingPort outPostPort = null;
			foreach (DockingPort port in DockingPort.List)
			{
				if (!port.IsHorizontal && !port.Docked)
				{
					if (port.Item.Submarine == outpost)
					{
						outPostPort = port;
					}
					else if (port.Item.Submarine == submarine && port.Item.WorldPosition.Y >= submarine.WorldPosition.Y)
					{
						float dist = Vector2.DistanceSquared(port.Item.WorldPosition, outpost.WorldPosition);
						if ((myPort == null || dist < closestDistance || port.MainDockingPort) && (myPort == null || !myPort.MainDockingPort))
						{
							myPort = port;
							closestDistance = dist;
						}
					}
				}
			}
			if (myPort != null && outPostPort != null)
			{
				Vector2 portDiff = myPort.Item.WorldPosition - submarine.WorldPosition;
				Vector2 spawnPos = outPostPort.Item.WorldPosition - portDiff - Vector2.UnitY * outPostPort.DockedDistance;
				submarine.SetPosition(spawnPos, null, true);
				myPort.Dock(outPostPort);
				myPort.Lock(true, true, true);
			}
			if (Character.Controlled != null)
			{
				Character.Controlled.TeleportTo(outpost.GetWaypoints(false).GetRandomUnsynced((WayPoint point) => point.SpawnType == SpawnType.Human).WorldPosition);
			}
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x00093FC0 File Offset: 0x000921C0
		public TestGameMode(GameModePreset preset) : base(preset)
		{
			foreach (JobPrefab jobPrefab in from p in JobPrefab.Prefabs
			orderby p.Identifier
			select p)
			{
				for (int i = 0; i < jobPrefab.InitialCount; i++)
				{
					int variant = Rand.Range(0, jobPrefab.Variants, Rand.RandSync.Unsynced);
					base.CrewManager.AddCharacterInfo(new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", jobPrefab, variant, Rand.RandSync.Unsynced, default(Identifier)));
				}
			}
		}

		// Token: 0x040007C1 RID: 1985
		public Action OnRoundEnd;

		// Token: 0x040007C2 RID: 1986
		public bool SpawnOutpost;

		// Token: 0x040007C3 RID: 1987
		public OutpostGenerationParams OutpostParams;

		// Token: 0x040007C4 RID: 1988
		public LocationType OutpostType;

		// Token: 0x040007C5 RID: 1989
		public EventPrefab TriggeredEvent;

		// Token: 0x040007C6 RID: 1990
		private List<Event> scriptedEvent;

		// Token: 0x040007C7 RID: 1991
		private GUIButton createEventButton;

		// Token: 0x040007C9 RID: 1993
		public Vector2 WaterParticleOffset;
	}
}
