using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200029A RID: 666
	internal class NPCChangeTeamAction : EventAction
	{
		// Token: 0x17000F56 RID: 3926
		// (get) Token: 0x06003A48 RID: 14920 RVA: 0x0021EA78 File Offset: 0x0021CC78
		// (set) Token: 0x06003A49 RID: 14921 RVA: 0x0021EA80 File Offset: 0x0021CC80
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) whose team to change.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000F57 RID: 3927
		// (get) Token: 0x06003A4A RID: 14922 RVA: 0x0021EA89 File Offset: 0x0021CC89
		// (set) Token: 0x06003A4B RID: 14923 RVA: 0x0021EA91 File Offset: 0x0021CC91
		[Serialize(CharacterTeamType.None, IsPropertySaveable.Yes, "The team to move the NPC to. None = unspecified, Team1 = player crew, Team2 = the team opposing Team1 (= hostile to player crew), FriendlyNPC = friendly to all other teams.", "", false)]
		public CharacterTeamType TeamID { get; set; }

		// Token: 0x17000F58 RID: 3928
		// (get) Token: 0x06003A4C RID: 14924 RVA: 0x0021EA9A File Offset: 0x0021CC9A
		// (set) Token: 0x06003A4D RID: 14925 RVA: 0x0021EAA2 File Offset: 0x0021CCA2
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be added to the player crew?", "", false)]
		public bool AddToCrew { get; set; }

		// Token: 0x17000F59 RID: 3929
		// (get) Token: 0x06003A4E RID: 14926 RVA: 0x0021EAAB File Offset: 0x0021CCAB
		// (set) Token: 0x06003A4F RID: 14927 RVA: 0x0021EAB3 File Offset: 0x0021CCB3
		[Serialize(false, IsPropertySaveable.Yes, "Should the NPC be removed from the player crew?", "", false)]
		public bool RemoveFromCrew { get; set; }

		// Token: 0x06003A50 RID: 14928 RVA: 0x0021EABC File Offset: 0x0021CCBC
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

		// Token: 0x06003A51 RID: 14929 RVA: 0x0021EBC8 File Offset: 0x0021CDC8
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

		// Token: 0x06003A52 RID: 14930 RVA: 0x0021EEDC File Offset: 0x0021D0DC
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A53 RID: 14931 RVA: 0x0021EEE4 File Offset: 0x0021D0E4
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A54 RID: 14932 RVA: 0x0021EEF0 File Offset: 0x0021D0F0
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

		// Token: 0x04001DF8 RID: 7672
		private bool isFinished;

		// Token: 0x04001DF9 RID: 7673
		private List<Character> affectedNpcs;
	}
}
