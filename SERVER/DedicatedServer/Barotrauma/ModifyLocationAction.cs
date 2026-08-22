using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001A6 RID: 422
	internal class ModifyLocationAction : EventAction
	{
		// Token: 0x1700090A RID: 2314
		// (get) Token: 0x06001F83 RID: 8067 RVA: 0x000D89C0 File Offset: 0x000D6BC0
		// (set) Token: 0x06001F84 RID: 8068 RVA: 0x000D89C8 File Offset: 0x000D6BC8
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction to set as the location's primary faction (optional).", "", false)]
		public Identifier Faction { get; set; }

		// Token: 0x1700090B RID: 2315
		// (get) Token: 0x06001F85 RID: 8069 RVA: 0x000D89D1 File Offset: 0x000D6BD1
		// (set) Token: 0x06001F86 RID: 8070 RVA: 0x000D89D9 File Offset: 0x000D6BD9
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction to set as the location's secondary faction (optional).", "", false)]
		public Identifier SecondaryFaction { get; set; }

		// Token: 0x1700090C RID: 2316
		// (get) Token: 0x06001F87 RID: 8071 RVA: 0x000D89E2 File Offset: 0x000D6BE2
		// (set) Token: 0x06001F88 RID: 8072 RVA: 0x000D89EA File Offset: 0x000D6BEA
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the location type to set as the location's new type (optional)", "", false)]
		public Identifier Type { get; set; }

		// Token: 0x1700090D RID: 2317
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x000D89F3 File Offset: 0x000D6BF3
		// (set) Token: 0x06001F8A RID: 8074 RVA: 0x000D89FB File Offset: 0x000D6BFB
		[Serialize("", IsPropertySaveable.Yes, "New name to give to the location (optional). Can either be the name as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Name { get; set; }

		// Token: 0x06001F8B RID: 8075 RVA: 0x000D8A04 File Offset: 0x000D6C04
		public ModifyLocationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x000D8A0E File Offset: 0x000D6C0E
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x000D8A16 File Offset: 0x000D6C16
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x000D8A20 File Offset: 0x000D6C20
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			CampaignMode campaign = GameMain.GameSession.GameMode as CampaignMode;
			if (campaign != null)
			{
				Location location = campaign.Map.CurrentLocation;
				if (location != null)
				{
					if (!this.Faction.IsEmpty)
					{
						Faction faction = campaign.Factions.Find(delegate(Faction f)
						{
							Prefab prefab2 = f.Prefab;
							Identifier faction2 = this.Faction;
							return prefab2.Identifier == faction2;
						});
						if (faction == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(82, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Error in ModifyLocationAction (");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("): could not find a faction with the identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Faction);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							string error = defaultInterpolatedStringHandler.ToStringAndClear();
							Exception e = null;
							ScriptedEvent parentEvent = this.ParentEvent;
							ContentPackage contentPackage;
							if (parentEvent == null)
							{
								contentPackage = null;
							}
							else
							{
								EventPrefab prefab = parentEvent.Prefab;
								contentPackage = ((prefab != null) ? prefab.ContentPackage : null);
							}
							DebugConsole.ThrowError(error, e, contentPackage, false, false);
						}
						else
						{
							location.Faction = faction;
						}
					}
					if (!this.SecondaryFaction.IsEmpty)
					{
						Faction secondaryFaction = campaign.Factions.Find(delegate(Faction f)
						{
							Prefab prefab2 = f.Prefab;
							Identifier secondaryFaction2 = this.SecondaryFaction;
							return prefab2.Identifier == secondaryFaction2;
						});
						if (secondaryFaction == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(82, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Error in ModifyLocationAction (");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral("): could not find a faction with the identifier \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.SecondaryFaction);
							defaultInterpolatedStringHandler2.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
						}
						else
						{
							location.SecondaryFaction = secondaryFaction;
						}
					}
					if (!this.Type.IsEmpty)
					{
						LocationType locationType = LocationType.Prefabs.Find(delegate(LocationType lt)
						{
							Identifier type = this.Type;
							return lt.Identifier == type;
						});
						if (locationType == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(88, 2);
							defaultInterpolatedStringHandler3.AppendLiteral("Error in ModifyLocationAction (");
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler3.AppendLiteral("): could not find a location type with the identifier \"");
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Type);
							defaultInterpolatedStringHandler3.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
						}
						else if (!location.LocationTypeChangesBlocked)
						{
							location.ChangeType(campaign, locationType, true, true);
						}
					}
					if (!this.Name.IsEmpty)
					{
						location.ForceName(this.Name);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x000D8C9A File Offset: 0x000D6E9A
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " ModifyLocationAction";
		}

		// Token: 0x04000F07 RID: 3847
		private bool isFinished;
	}
}
