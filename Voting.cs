using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000F8 RID: 248
	internal class Voting
	{
		// Token: 0x06002341 RID: 9025 RVA: 0x00163C38 File Offset: 0x00161E38
		public int GetVoteCountYes(VoteType voteType)
		{
			int value;
			this.voteCountYes.TryGetValue(voteType, out value);
			return value;
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x00163C58 File Offset: 0x00161E58
		public int GetVoteCountNo(VoteType voteType)
		{
			int value;
			this.voteCountNo.TryGetValue(voteType, out value);
			return value;
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x00163C78 File Offset: 0x00161E78
		public int GetVoteCountMax(VoteType voteType)
		{
			int value;
			this.voteCountMax.TryGetValue(voteType, out value);
			return value;
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x00163C95 File Offset: 0x00161E95
		public void SetVoteCountYes(VoteType voteType, int value)
		{
			this.voteCountYes[voteType] = value;
		}

		// Token: 0x06002345 RID: 9029 RVA: 0x00163CA4 File Offset: 0x00161EA4
		public void SetVoteCountNo(VoteType voteType, int value)
		{
			this.voteCountNo[voteType] = value;
		}

		// Token: 0x06002346 RID: 9030 RVA: 0x00163CB3 File Offset: 0x00161EB3
		public void SetVoteCountMax(VoteType voteType, int value)
		{
			this.voteCountMax[voteType] = value;
		}

		// Token: 0x06002347 RID: 9031 RVA: 0x00163CC4 File Offset: 0x00161EC4
		public void UpdateVoteTexts(IEnumerable<Client> clients, VoteType voteType)
		{
			switch (voteType)
			{
			case VoteType.Sub:
			{
				GUIListBox subList = GameMain.NetLobbyScreen.SubList;
				foreach (GUIComponent comp in subList.Content.Children)
				{
					Voting.<UpdateVoteTexts>g__TryRemoveVoteText|10_0(comp);
					GUILayoutGroup container = comp.GetChild<GUILayoutGroup>();
					GUIFrame imageFrame = container.GetChild<GUIFrame>();
					GUIComponent coalIcon = imageFrame.GetChildByUserData("coalitionIcon");
					GUIComponent sepIcon = imageFrame.GetChildByUserData("separatistsIcon");
					coalIcon.Enabled = false;
					sepIcon.Enabled = false;
					Voting.<UpdateVoteTexts>g__TryRemoveVoteText|10_0(coalIcon);
					Voting.<UpdateVoteTexts>g__TryRemoveVoteText|10_0(sepIcon);
				}
				if (clients == null)
				{
					return;
				}
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				bool isPvP = ((netLobbyScreen != null) ? netLobbyScreen.SelectedMode : null) == GameModePreset.PvP;
				if (isPvP)
				{
					IReadOnlyDictionary<SubmarineInfo, int> coalitionVoteList = Voting.GetVoteCounts<SubmarineInfo>(voteType, from c in clients
					where c.PreferredTeam == CharacterTeamType.Team1
					select c);
					IReadOnlyDictionary<SubmarineInfo, int> separatistVoteList = Voting.GetVoteCounts<SubmarineInfo>(voteType, from c in clients
					where c.PreferredTeam == CharacterTeamType.Team2
					select c);
					foreach (KeyValuePair<SubmarineInfo, int> keyValuePair in coalitionVoteList)
					{
						KeyValuePair<SubmarineInfo, int> keyValuePair;
						SubmarineInfo submarineInfo;
						int num;
						keyValuePair.Deconstruct(out submarineInfo, out num);
						SubmarineInfo subInfo = submarineInfo;
						int amount = num;
						this.SetSubVoteText(subList, subInfo, amount, CharacterTeamType.Team1);
					}
					using (IEnumerator<KeyValuePair<SubmarineInfo, int>> enumerator3 = separatistVoteList.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							KeyValuePair<SubmarineInfo, int> keyValuePair = enumerator3.Current;
							SubmarineInfo submarineInfo;
							int num;
							keyValuePair.Deconstruct(out submarineInfo, out num);
							SubmarineInfo subInfo2 = submarineInfo;
							int amount2 = num;
							this.SetSubVoteText(subList, subInfo2, amount2, CharacterTeamType.Team2);
						}
						return;
					}
				}
				IReadOnlyDictionary<SubmarineInfo, int> subVoteList = Voting.GetVoteCounts<SubmarineInfo>(voteType, clients);
				using (IEnumerator<KeyValuePair<SubmarineInfo, int>> enumerator4 = subVoteList.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						KeyValuePair<SubmarineInfo, int> keyValuePair = enumerator4.Current;
						SubmarineInfo submarineInfo;
						int num;
						keyValuePair.Deconstruct(out submarineInfo, out num);
						SubmarineInfo subInfo3 = submarineInfo;
						int amount3 = num;
						this.SetSubVoteText(subList, subInfo3, amount3, CharacterTeamType.None);
					}
					return;
				}
				break;
			}
			case VoteType.Mode:
				break;
			case VoteType.EndRound:
			case VoteType.Kick:
				return;
			case VoteType.StartRound:
				goto IL_2CD;
			default:
				return;
			}
			GUIListBox modeList = GameMain.NetLobbyScreen.ModeList;
			foreach (GUIComponent comp2 in modeList.Content.Children)
			{
				GUITextBlock voteText = comp2.FindChild("votes", false) as GUITextBlock;
				if (voteText != null)
				{
					comp2.RemoveChild(voteText);
				}
			}
			if (clients == null)
			{
				return;
			}
			IReadOnlyDictionary<GameModePreset, int> modeVoteList = Voting.GetVoteCounts<GameModePreset>(voteType, clients);
			using (IEnumerator<KeyValuePair<GameModePreset, int>> enumerator6 = modeVoteList.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					KeyValuePair<GameModePreset, int> keyValuePair2 = enumerator6.Current;
					int num;
					GameModePreset gameModePreset;
					keyValuePair2.Deconstruct(out gameModePreset, out num);
					GameModePreset preset = gameModePreset;
					int amount4 = num;
					this.SetVoteText(modeList, preset, amount4);
				}
				return;
			}
			IL_2CD:
			if (clients == null)
			{
				return;
			}
			foreach (Client client in clients)
			{
				GUIComponent guicomponent = GameMain.NetLobbyScreen.PlayerList.Content.FindChild(client, false);
				GUIComponent clientReady = (guicomponent != null) ? guicomponent.FindChild("clientready", false) : null;
				if (clientReady != null)
				{
					clientReady.Visible = client.GetVote<bool>(VoteType.StartRound);
				}
			}
		}

		// Token: 0x06002348 RID: 9032 RVA: 0x00164064 File Offset: 0x00162264
		private void SetSubVoteText(GUIListBox subListBox, SubmarineInfo userData, int votes, CharacterTeamType type)
		{
			GUIComponent subElement = subListBox.Content.GetChildByUserData(userData);
			if (subElement == null)
			{
				DebugConsole.ThrowError("Failed to find the submarine element in the listbox", null, null, false, false);
				return;
			}
			ValueTuple<GUIComponent, GUIComponent> valueTuple = Voting.<SetSubVoteText>g__GetPvPIcons|11_1(subElement);
			GUIComponent coalIcon = valueTuple.Item1;
			GUIComponent sepIcon = valueTuple.Item2;
			switch (type)
			{
			case CharacterTeamType.None:
				this.SetVoteText(subListBox, userData, votes);
				return;
			case CharacterTeamType.Team1:
				coalIcon.Enabled = (votes > 0);
				Voting.<SetSubVoteText>g__CreateSubmarineVoteText|11_0(coalIcon, votes);
				return;
			case CharacterTeamType.Team2:
				sepIcon.Enabled = (votes > 0);
				Voting.<SetSubVoteText>g__CreateSubmarineVoteText|11_0(sepIcon, votes);
				return;
			default:
				return;
			}
		}

		// Token: 0x06002349 RID: 9033 RVA: 0x001640E4 File Offset: 0x001622E4
		private void SetVoteText(GUIListBox listBox, object userData, int votes)
		{
			if (userData == null)
			{
				return;
			}
			foreach (GUIComponent comp in listBox.Content.Children)
			{
				if (comp.UserData == userData)
				{
					GUITextBlock voteText = comp.FindChild("votes", false) as GUITextBlock;
					if (voteText == null)
					{
						voteText = new GUITextBlock(new RectTransform(new Point(GUI.IntScale(30f), comp.Rect.Height), comp.RectTransform, Anchor.CenterRight, null, ScaleBasis.Normal, false), "", null, null, Alignment.Center, false, "", null)
						{
							Padding = Vector4.Zero,
							UserData = "votes"
						};
					}
					voteText.Text = ((votes == 0) ? "" : votes.ToString());
				}
			}
		}

		// Token: 0x0600234A RID: 9034 RVA: 0x001641EC File Offset: 0x001623EC
		public void ResetVotes(IEnumerable<Client> connectedClients)
		{
			foreach (Client client in connectedClients)
			{
				client.ResetVotes();
			}
			foreach (object obj in Enum.GetValues(typeof(VoteType)))
			{
				VoteType voteType = (VoteType)obj;
				this.SetVoteCountYes(voteType, 0);
				this.SetVoteCountNo(voteType, 0);
				this.SetVoteCountMax(voteType, 0);
			}
			this.UpdateVoteTexts(connectedClients, VoteType.Mode);
			this.UpdateVoteTexts(connectedClients, VoteType.Sub);
		}

		// Token: 0x0600234B RID: 9035 RVA: 0x001642A8 File Offset: 0x001624A8
		public bool ClientWrite(IWriteMessage msg, VoteType voteType, object data)
		{
			msg.WriteByte((byte)voteType);
			switch (voteType)
			{
			case VoteType.Sub:
			{
				SubmarineInfo sub = data as SubmarineInfo;
				if (sub == null)
				{
					return false;
				}
				msg.WriteInt32(sub.EqualityCheckVal);
				if (sub.EqualityCheckVal <= 0)
				{
					msg.WriteString(sub.MD5Hash.StringRepresentation);
				}
				break;
			}
			case VoteType.Mode:
			{
				GameModePreset gameMode = data as GameModePreset;
				if (gameMode == null)
				{
					return false;
				}
				msg.WriteIdentifier(gameMode.Identifier);
				break;
			}
			case VoteType.EndRound:
			{
				if (!(data is bool))
				{
					return false;
				}
				bool endRound = (bool)data;
				msg.WriteBoolean(endRound);
				break;
			}
			case VoteType.Kick:
			{
				Client votedClient = data as Client;
				if (votedClient == null)
				{
					return false;
				}
				msg.WriteByte(votedClient.SessionId);
				break;
			}
			case VoteType.StartRound:
			{
				if (!(data is bool))
				{
					return false;
				}
				bool startRound = (bool)data;
				msg.WriteBoolean(startRound);
				break;
			}
			case VoteType.PurchaseAndSwitchSub:
			case VoteType.PurchaseSub:
			case VoteType.SwitchSub:
			{
				ITuple tuple = data as ITuple;
				if (tuple != null)
				{
					int length = tuple.Length;
					if (length == 2)
					{
						object obj = tuple[0];
						SubmarineInfo voteSub = obj as SubmarineInfo;
						if (voteSub != null)
						{
							object obj2 = tuple[1];
							if (obj2 is bool)
							{
								bool transferItems = (bool)obj2;
								msg.WriteBoolean(true);
								msg.WriteString(voteSub.Name);
								msg.WriteBoolean(transferItems);
								break;
							}
						}
					}
				}
				else if (data is int)
				{
					int vote = (int)data;
					msg.WriteBoolean(false);
					msg.WriteInt32(vote);
					break;
				}
				return false;
			}
			case VoteType.TransferMoney:
			{
				if (!(data is int))
				{
					return false;
				}
				int money = (int)data;
				msg.WriteBoolean(false);
				msg.WriteInt32(money);
				break;
			}
			case VoteType.Traitor:
			{
				Client client = data as Client;
				msg.WriteInt32((int)((client != null) ? client.SessionId : 0));
				break;
			}
			}
			msg.WritePadBits();
			return true;
		}

		// Token: 0x0600234C RID: 9036 RVA: 0x0016447C File Offset: 0x0016267C
		public void ClientRead(IReadMessage inc)
		{
			Voting.<>c__DisplayClass15_0 CS$<>8__locals1;
			CS$<>8__locals1.inc = inc;
			GameMain.Client.ServerSettings.AllowSubVoting = CS$<>8__locals1.inc.ReadBoolean();
			if (GameMain.Client.ServerSettings.AllowSubVoting)
			{
				this.UpdateVoteTexts(null, VoteType.Sub);
				bool isMultiSub = CS$<>8__locals1.inc.ReadBoolean();
				int votableCount = (int)CS$<>8__locals1.inc.ReadByte();
				List<SubmarineInfo> serversubs = new List<SubmarineInfo>();
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				bool flag;
				if (netLobbyScreen == null)
				{
					flag = (null != null);
				}
				else
				{
					GUIListBox subList = netLobbyScreen.SubList;
					flag = (((subList != null) ? subList.Content : null) != null);
				}
				if (flag)
				{
					foreach (GUIComponent item in GameMain.NetLobbyScreen.SubList.Content.Children)
					{
						SubmarineInfo info = item.UserData as SubmarineInfo;
						if (info != null)
						{
							serversubs.Add(info);
						}
					}
				}
				for (int i = 0; i < votableCount; i++)
				{
					int votes = (int)CS$<>8__locals1.inc.ReadByte();
					string subName = CS$<>8__locals1.inc.ReadString();
					SubmarineInfo sub = serversubs.FirstOrDefault((SubmarineInfo s) => s.Name == subName);
					this.SetSubVoteText(GameMain.NetLobbyScreen.SubList, sub, votes, isMultiSub ? CharacterTeamType.Team1 : CharacterTeamType.None);
				}
				if (isMultiSub)
				{
					int separatistsCount = (int)CS$<>8__locals1.inc.ReadByte();
					for (int j = 0; j < separatistsCount; j++)
					{
						int votes2 = (int)CS$<>8__locals1.inc.ReadByte();
						string subName = CS$<>8__locals1.inc.ReadString();
						SubmarineInfo sub2 = serversubs.FirstOrDefault((SubmarineInfo s) => s.Name == subName);
						this.SetSubVoteText(GameMain.NetLobbyScreen.SubList, sub2, votes2, CharacterTeamType.Team2);
					}
				}
			}
			GameMain.Client.ServerSettings.AllowModeVoting = CS$<>8__locals1.inc.ReadBoolean();
			if (GameMain.Client.ServerSettings.AllowModeVoting)
			{
				this.UpdateVoteTexts(null, VoteType.Mode);
				int votableCount2 = (int)CS$<>8__locals1.inc.ReadByte();
				for (int k = 0; k < votableCount2; k++)
				{
					int votes3 = (int)CS$<>8__locals1.inc.ReadByte();
					string modeIdentifier = CS$<>8__locals1.inc.ReadString();
					GameModePreset mode = GameModePreset.List.Find((GameModePreset m) => m.Identifier == modeIdentifier);
					this.SetVoteText(GameMain.NetLobbyScreen.ModeList, mode, votes3);
				}
			}
			GameMain.Client.ServerSettings.AllowEndVoting = CS$<>8__locals1.inc.ReadBoolean();
			if (GameMain.Client.ServerSettings.AllowEndVoting)
			{
				this.SetVoteCountYes(VoteType.EndRound, (int)CS$<>8__locals1.inc.ReadByte());
				this.SetVoteCountMax(VoteType.EndRound, (int)CS$<>8__locals1.inc.ReadByte());
			}
			GameMain.Client.ServerSettings.AllowVoteKick = CS$<>8__locals1.inc.ReadBoolean();
			byte activeVoteStateByte = CS$<>8__locals1.inc.ReadByte();
			Voting.VoteState activeVoteState = Voting.VoteState.None;
			try
			{
				activeVoteState = (Voting.VoteState)activeVoteStateByte;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to cast vote type \"" + activeVoteStateByte.ToString() + "\"", e, null, false, false);
			}
			if (activeVoteState != Voting.VoteState.None)
			{
				byte voteTypeByte = CS$<>8__locals1.inc.ReadByte();
				Voting.<>c__DisplayClass15_4 CS$<>8__locals5;
				CS$<>8__locals5.voteType = VoteType.Unknown;
				try
				{
					CS$<>8__locals5.voteType = (VoteType)voteTypeByte;
				}
				catch (Exception e2)
				{
					DebugConsole.ThrowError("Failed to cast vote type \"" + voteTypeByte.ToString() + "\"", e2, null, false, false);
				}
				int yesClientCount = Voting.<ClientRead>g__readVote|15_4(2, ref CS$<>8__locals1, ref CS$<>8__locals5);
				int noClientCount = Voting.<ClientRead>g__readVote|15_4(1, ref CS$<>8__locals1, ref CS$<>8__locals5);
				byte maxClientCount = CS$<>8__locals1.inc.ReadByte();
				this.SetVoteCountYes(CS$<>8__locals5.voteType, yesClientCount);
				this.SetVoteCountNo(CS$<>8__locals5.voteType, noClientCount);
				this.SetVoteCountMax(CS$<>8__locals5.voteType, (int)maxClientCount);
				switch (activeVoteState)
				{
				case Voting.VoteState.Started:
				{
					byte starterID = CS$<>8__locals1.inc.ReadByte();
					Client starterClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == starterID);
					float timeOut = (float)CS$<>8__locals1.inc.ReadByte();
					Client myClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == GameMain.Client.SessionId);
					if (myClient == null || !myClient.InGame)
					{
						return;
					}
					VoteType voteType = CS$<>8__locals5.voteType;
					if (voteType - VoteType.PurchaseAndSwitchSub > 2)
					{
						if (voteType == VoteType.TransferMoney)
						{
							byte fromClientId = CS$<>8__locals1.inc.ReadByte();
							byte toClientId = CS$<>8__locals1.inc.ReadByte();
							int transferAmount = CS$<>8__locals1.inc.ReadInt32();
							Client fromClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == fromClientId);
							Client toClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == toClientId);
							GameMain.Client.ShowMoneyTransferVoteInterface(starterClient, fromClient, transferAmount, toClient, timeOut);
						}
					}
					else
					{
						string subName1 = CS$<>8__locals1.inc.ReadString();
						bool transferItems = CS$<>8__locals1.inc.ReadBoolean();
						SubmarineInfo info2 = GameMain.GameSession.OwnedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName1) ?? GameMain.Client.ServerSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName1);
						if (info2 == null)
						{
							DebugConsole.ThrowError("Failed to find a matching submarine, vote aborted", null, null, false, false);
							return;
						}
						GameMain.Client.ShowSubmarineChangeVoteInterface(starterClient, info2, CS$<>8__locals5.voteType, transferItems, timeOut);
					}
					break;
				}
				case Voting.VoteState.Passed:
				case Voting.VoteState.Failed:
				{
					bool passed = CS$<>8__locals1.inc.ReadBoolean();
					Voting.SubmarineVoteInfo submarineVoteInfo = default(Voting.SubmarineVoteInfo);
					VoteType voteType = CS$<>8__locals5.voteType;
					if (voteType - VoteType.PurchaseAndSwitchSub <= 2)
					{
						string subName2 = CS$<>8__locals1.inc.ReadString();
						bool transferItems2 = CS$<>8__locals1.inc.ReadBoolean();
						if (GameMain.GameSession != null)
						{
							SubmarineInfo submarineInfo = GameMain.GameSession.OwnedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName2) ?? GameMain.Client.ServerSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName2);
							if (submarineInfo == null)
							{
								DebugConsole.ThrowError("Failed to find a matching submarine, vote aborted", null, null, false, false);
								return;
							}
							submarineVoteInfo = new Voting.SubmarineVoteInfo(submarineInfo, transferItems2);
						}
					}
					VotingInterface votingInterface = GameMain.Client.VotingInterface;
					if (votingInterface != null)
					{
						votingInterface.EndVote(passed, yesClientCount, noClientCount);
					}
					if (passed)
					{
						SubmarineInfo subInfo = submarineVoteInfo.SubmarineInfo;
						if (subInfo != null)
						{
							switch (CS$<>8__locals5.voteType)
							{
							case VoteType.PurchaseAndSwitchSub:
								if (GameMain.GameSession.TryPurchaseSubmarine(subInfo, null))
								{
									GameMain.GameSession.SwitchSubmarine(subInfo, submarineVoteInfo.TransferItems, null);
								}
								break;
							case VoteType.PurchaseSub:
								GameMain.GameSession.TryPurchaseSubmarine(subInfo, null);
								break;
							case VoteType.SwitchSub:
								GameMain.GameSession.SwitchSubmarine(subInfo, submarineVoteInfo.TransferItems, null);
								break;
							}
							SubmarineSelection.ContentRefreshRequired = true;
						}
					}
					break;
				}
				}
			}
			GameMain.NetworkMember.ConnectedClients.ForEach(delegate(Client c)
			{
				c.SetVote(VoteType.StartRound, false);
			});
			byte readyClientCount = CS$<>8__locals1.inc.ReadByte();
			for (int l = 0; l < (int)readyClientCount; l++)
			{
				byte clientId = CS$<>8__locals1.inc.ReadByte();
				Client matchingClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == clientId);
				if (matchingClient != null)
				{
					matchingClient.SetVote(VoteType.StartRound, true);
				}
			}
			this.UpdateVoteTexts(GameMain.NetworkMember.ConnectedClients, VoteType.StartRound);
			CS$<>8__locals1.inc.ReadPadBits();
		}

		// Token: 0x0600234D RID: 9037 RVA: 0x00164C18 File Offset: 0x00162E18
		private static IReadOnlyDictionary<T, int> GetVoteCounts<T>(VoteType voteType, IEnumerable<Client> voters)
		{
			Dictionary<T, int> voteList = new Dictionary<T, int>();
			foreach (Client voter in voters)
			{
				T vote = voter.GetVote<T>(voteType);
				if (vote != null)
				{
					if (!voteList.ContainsKey(vote))
					{
						voteList.Add(vote, 1);
					}
					else
					{
						Dictionary<T, int> dictionary = voteList;
						T key = vote;
						int num = dictionary[key];
						dictionary[key] = num + 1;
					}
				}
			}
			return voteList;
		}

		// Token: 0x0600234E RID: 9038 RVA: 0x00164C9C File Offset: 0x00162E9C
		public static T HighestVoted<T>(VoteType voteType, IEnumerable<Client> voters)
		{
			int num;
			return Voting.HighestVoted<T>(voteType, voters, out num);
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x00164CB4 File Offset: 0x00162EB4
		public static T HighestVoted<T>(VoteType voteType, IEnumerable<Client> voters, out int voteCount)
		{
			voteCount = 0;
			if (voteType == VoteType.Sub && !GameMain.NetworkMember.ServerSettings.AllowSubVoting)
			{
				return default(T);
			}
			if (voteType == VoteType.Mode && !GameMain.NetworkMember.ServerSettings.AllowModeVoting)
			{
				return default(T);
			}
			IReadOnlyDictionary<T, int> voteList = Voting.GetVoteCounts<T>(voteType, voters);
			T selected = default(T);
			int highestVotes = 0;
			foreach (KeyValuePair<T, int> votable in voteList)
			{
				if (voteType == VoteType.Sub)
				{
					SubmarineInfo subInfo = votable.Key as SubmarineInfo;
					if (subInfo != null && GameMain.NetworkMember.ServerSettings.HiddenSubs.Contains(subInfo.Name))
					{
						continue;
					}
				}
				if (selected == null || votable.Value > highestVotes)
				{
					highestVotes = votable.Value;
					selected = votable.Key;
				}
			}
			voteCount = highestVotes;
			return selected;
		}

		// Token: 0x06002351 RID: 9041 RVA: 0x00164DD8 File Offset: 0x00162FD8
		[CompilerGenerated]
		internal static void <UpdateVoteTexts>g__TryRemoveVoteText|10_0(GUIComponent component)
		{
			GUITextBlock foundText = component.FindChild("votes", false) as GUITextBlock;
			if (foundText != null)
			{
				component.RemoveChild(foundText);
			}
		}

		// Token: 0x06002352 RID: 9042 RVA: 0x00164E04 File Offset: 0x00163004
		[CompilerGenerated]
		internal static void <SetSubVoteText>g__CreateSubmarineVoteText|11_0(GUIComponent parent, int votes)
		{
			if (parent == null)
			{
				return;
			}
			RectTransform rectT = new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
			defaultInterpolatedStringHandler.AppendFormatted<int>(votes);
			GUITextBlock voteText = new GUITextBlock(rectT, defaultInterpolatedStringHandler.ToStringAndClear(), null, null, Alignment.Center, false, "", null)
			{
				Padding = Vector4.Zero,
				UserData = "votes",
				Shadow = true
			};
			voteText.RectTransform.RelativeOffset = new Vector2(0.33f, 0.33f);
		}

		// Token: 0x06002353 RID: 9043 RVA: 0x00164EBC File Offset: 0x001630BC
		[CompilerGenerated]
		[return: TupleElementNames(new string[]
		{
			"CoalitionIcon",
			"SeparatistsIcon"
		})]
		internal static ValueTuple<GUIComponent, GUIComponent> <SetSubVoteText>g__GetPvPIcons|11_1(GUIComponent child)
		{
			GUILayoutGroup container = child.GetChild<GUILayoutGroup>();
			GUIFrame imageFrame = container.GetChild<GUIFrame>();
			GUIComponent coalIcon = imageFrame.GetChildByUserData("coalitionIcon");
			GUIComponent sepIcon = imageFrame.GetChildByUserData("separatistsIcon");
			return new ValueTuple<GUIComponent, GUIComponent>(coalIcon, sepIcon);
		}

		// Token: 0x06002354 RID: 9044 RVA: 0x00164EF8 File Offset: 0x001630F8
		[CompilerGenerated]
		internal static int <ClientRead>g__readVote|15_4(int value, ref Voting.<>c__DisplayClass15_0 A_1, ref Voting.<>c__DisplayClass15_4 A_2)
		{
			byte clientCount = A_1.inc.ReadByte();
			for (int i = 0; i < (int)clientCount; i++)
			{
				byte clientId = A_1.inc.ReadByte();
				Client matchingClient = GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.SessionId == clientId);
				if (matchingClient != null)
				{
					matchingClient.SetVote(A_2.voteType, value);
				}
			}
			return (int)clientCount;
		}

		// Token: 0x040011B1 RID: 4529
		private readonly Dictionary<VoteType, int> voteCountYes = new Dictionary<VoteType, int>();

		// Token: 0x040011B2 RID: 4530
		private readonly Dictionary<VoteType, int> voteCountNo = new Dictionary<VoteType, int>();

		// Token: 0x040011B3 RID: 4531
		private readonly Dictionary<VoteType, int> voteCountMax = new Dictionary<VoteType, int>();

		// Token: 0x02000BE4 RID: 3044
		private struct SubmarineVoteInfo
		{
			// Token: 0x17001AC4 RID: 6852
			// (get) Token: 0x06007A43 RID: 31299 RVA: 0x003812C7 File Offset: 0x0037F4C7
			// (set) Token: 0x06007A44 RID: 31300 RVA: 0x003812CF File Offset: 0x0037F4CF
			public SubmarineInfo SubmarineInfo { readonly get; set; }

			// Token: 0x17001AC5 RID: 6853
			// (get) Token: 0x06007A45 RID: 31301 RVA: 0x003812D8 File Offset: 0x0037F4D8
			// (set) Token: 0x06007A46 RID: 31302 RVA: 0x003812E0 File Offset: 0x0037F4E0
			public bool TransferItems { readonly get; set; }

			// Token: 0x06007A47 RID: 31303 RVA: 0x003812E9 File Offset: 0x0037F4E9
			public SubmarineVoteInfo(SubmarineInfo submarineInfo, bool transferItems)
			{
				this.SubmarineInfo = submarineInfo;
				this.TransferItems = transferItems;
			}
		}

		// Token: 0x02000BE5 RID: 3045
		public enum VoteState
		{
			// Token: 0x04004940 RID: 18752
			None,
			// Token: 0x04004941 RID: 18753
			Started,
			// Token: 0x04004942 RID: 18754
			Running,
			// Token: 0x04004943 RID: 18755
			Passed,
			// Token: 0x04004944 RID: 18756
			Failed
		}
	}
}
