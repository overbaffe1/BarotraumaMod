using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000298 RID: 664
	internal class ModifyLocationAction : EventAction
	{
		// Token: 0x17000F50 RID: 3920
		// (get) Token: 0x06003A2F RID: 14895 RVA: 0x0021E5F0 File Offset: 0x0021C7F0
		// (set) Token: 0x06003A30 RID: 14896 RVA: 0x0021E5F8 File Offset: 0x0021C7F8
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction to set as the location's primary faction (optional).", "", false)]
		public Identifier Faction { get; set; }

		// Token: 0x17000F51 RID: 3921
		// (get) Token: 0x06003A31 RID: 14897 RVA: 0x0021E601 File Offset: 0x0021C801
		// (set) Token: 0x06003A32 RID: 14898 RVA: 0x0021E609 File Offset: 0x0021C809
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction to set as the location's secondary faction (optional).", "", false)]
		public Identifier SecondaryFaction { get; set; }

		// Token: 0x17000F52 RID: 3922
		// (get) Token: 0x06003A33 RID: 14899 RVA: 0x0021E612 File Offset: 0x0021C812
		// (set) Token: 0x06003A34 RID: 14900 RVA: 0x0021E61A File Offset: 0x0021C81A
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the location type to set as the location's new type (optional)", "", false)]
		public Identifier Type { get; set; }

		// Token: 0x17000F53 RID: 3923
		// (get) Token: 0x06003A35 RID: 14901 RVA: 0x0021E623 File Offset: 0x0021C823
		// (set) Token: 0x06003A36 RID: 14902 RVA: 0x0021E62B File Offset: 0x0021C82B
		[Serialize("", IsPropertySaveable.Yes, "New name to give to the location (optional). Can either be the name as-is, or a tag referring to a line in a text file.", "", false)]
		public Identifier Name { get; set; }

		// Token: 0x06003A37 RID: 14903 RVA: 0x0021E634 File Offset: 0x0021C834
		public ModifyLocationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003A38 RID: 14904 RVA: 0x0021E63E File Offset: 0x0021C83E
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A39 RID: 14905 RVA: 0x0021E646 File Offset: 0x0021C846
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A3A RID: 14906 RVA: 0x0021E650 File Offset: 0x0021C850
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

		// Token: 0x06003A3B RID: 14907 RVA: 0x0021E8CA File Offset: 0x0021CACA
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " ModifyLocationAction";
		}

		// Token: 0x04001DF0 RID: 7664
		private bool isFinished;
	}
}
