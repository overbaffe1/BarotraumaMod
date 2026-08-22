using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001A8 RID: 424
	internal class NPCChangeTeamAction : EventAction
	{
		// Token: 0x17000910 RID: 2320
		// (get) Token: 0x06001F9C RID: 8092 RVA: 0x000D8F5C File Offset: 0x000D715C
		// (set) Token: 0x06001F9D RID: 8093 RVA: 0x000D8F64 File Offset: 0x000D7164
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) whose team to change.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000911 RID: 2321
		// (get) Token: 0x06001F9E RID: 8094 RVA: 0x000D8F6D File Offset: 0x000D716D
		// (set) Token: 0x06001F9F RID: 8095 RVA: 0x000D8F75 File Offset: 0x000D7175
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "The team to move the NPC to. None = unspecified, Team1 = player crew, Team2 = the team opposing Team1 (= hostile to player crew), FriendlyNPC = friendly to all other teams.", "", false)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x17000912 RID: 2322
		// (get) Token: 0x06001FA0 RID: 8096 RVA: 0x000D8F7E File Offset: 0x000D717E
		// (set) Token: 0x06001FA1 RID: 8097 RVA: 0x000D8F86 File Offset: 0x000D7186
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be added to the player crew?", "", false)]
		public bool AddToCrew { get; set; }

		// Token: 0x17000913 RID: 2323
		// (get) Token: 0x06001FA2 RID: 8098 RVA: 0x000D8F8F File Offset: 0x000D718F
		// (set) Token: 0x06001FA3 RID: 8099 RVA: 0x000D8F97 File Offset: 0x000D7197
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be removed from the player crew?", "", false)]
		public bool RemoveFromCrew { get; set; }

		// Token: 0x06001FA4 RID: 8100 RVA: 0x000D8FA0 File Offset: 0x000D71A0
		public NPCChangeTeamAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			string key = "teamtag";
			string key2 = "team";
			CharacterTeamType teamID = this.TeamID;
			CharacterTeamType attributeEnum = element.GetAttributeEnum<CharacterTeamType>(key2, teamID);
			this.TeamID = element.GetAttributeEnum<CharacterTeamType>(key, attributeEnum);
			IEnumerable<CharacterTeamType> enums = Enum.GetValues(typeof(CharacterTeamType)).Cast<CharacterTeamType>();
			if (!enums.Contains(this.TeamID))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("NPCChangeTeamAction");
				defaultInterpolatedStringHandler.AppendLiteral(" in the event ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(". \"");
				defaultInterpolatedStringHandler.AppendFormatted<CharacterTeamType>(this.TeamID);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid Team ID. Valid values are ");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(',', Enum.GetNames(typeof(CharacterTeamType))));
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x000D90AC File Offset: 0x000D72AC
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			CharacterTeamType teamID = this.TeamID;
			bool flag = teamID - CharacterTeamType.Team1 <= 1;
			bool isPlayerTeam = flag;
			this.affectedNpcs = this.ParentEvent.GetTargets(this.NPCTag).OfType<Character>().ToList<Character>();
			using (List<Character>.Enumerator enumerator = this.affectedNpcs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NPCChangeTeamAction.<>c__DisplayClass19_0 CS$<>8__locals1 = new NPCChangeTeamAction.<>c__DisplayClass19_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.npc = enumerator.Current;
					CS$<>8__locals1.npc.SetOriginalTeamAndChangeTeam(this.TeamID, false);
					foreach (Item item in CS$<>8__locals1.npc.Inventory.AllItems)
					{
						IdCard idCard = item.GetComponent<IdCard>();
						if (idCard != null)
						{
							idCard.TeamID = this.TeamID;
							if (isPlayerTeam)
							{
								idCard.SubmarineSpecificID = 0;
							}
						}
					}
					CrewManager crewManager = GameMain.GameSession.CrewManager;
					if (crewManager != null)
					{
						if (this.AddToCrew && isPlayerTeam)
						{
							CharacterInfo info = CS$<>8__locals1.npc.Info;
							if (info != null)
							{
								info.StartItemsGiven = true;
								crewManager.AddCharacter(CS$<>8__locals1.npc);
							}
							else
							{
								DebugConsole.AddWarning("Attempted to change the team of a character (" + CS$<>8__locals1.npc.Name + ") that doesn't have Character Info. Can't add to the crew.", null);
							}
							CS$<>8__locals1.<Update>g__ChangeItemTeam|0(Submarine.MainSub ?? Submarine.Loaded.FirstOrDefault((Submarine s) => s.TeamID == this.TeamID), true);
							if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
							{
								GameMain.NetworkMember.CreateEntityEvent(CS$<>8__locals1.npc, new Character.AddToCrewEventData(this.TeamID, CS$<>8__locals1.npc.Inventory.FindAllItems(null, true, null)));
							}
						}
						else
						{
							bool removeFromCrew = this.RemoveFromCrew;
							bool flag2 = removeFromCrew;
							if (flag2)
							{
								teamID = CS$<>8__locals1.npc.TeamID;
								flag = (teamID - CharacterTeamType.Team1 <= 1);
								flag2 = flag;
							}
							if (flag2)
							{
								CharacterInfo info2 = CS$<>8__locals1.npc.Info;
								if (info2 != null)
								{
									info2.StartItemsGiven = true;
									crewManager.RemoveCharacter(CS$<>8__locals1.npc, true, true);
								}
								else
								{
									DebugConsole.AddWarning("Attempted to change the team of a character (" + CS$<>8__locals1.npc.Name + ") that doesn't have Character Info. Can't remove from the crew.", null);
								}
								Submarine sub = Submarine.Loaded.FirstOrDefault((Submarine s) => s.TeamID == this.TeamID);
								CS$<>8__locals1.<Update>g__ChangeItemTeam|0(sub, false);
								if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
								{
									GameMain.NetworkMember.CreateEntityEvent(CS$<>8__locals1.npc, new Character.RemoveFromCrewEventData(this.TeamID, CS$<>8__locals1.npc.Inventory.FindAllItems(null, true, null)));
								}
							}
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x000D93C0 File Offset: 0x000D75C0
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x000D93C8 File Offset: 0x000D75C8
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x000D93D4 File Offset: 0x000D75D4
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("NPCChangeTeamAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (NPCTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.NPCTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F0F RID: 3855
		private bool isFinished;

		// Token: 0x04000F10 RID: 3856
		private List<Character> affectedNpcs;
	}
}
