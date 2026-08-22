using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000045 RID: 69
	internal class Voting
	{
		// Token: 0x06000B41 RID: 2881 RVA: 0x0006C724 File Offset: 0x0006A924
		private void StartSubmarineVote(SubmarineInfo subInfo, bool transferItems, VoteType voteType, Client sender)
		{
			Voting.SubmarineVote subVote = new Voting.SubmarineVote(sender, subInfo, transferItems, voteType);
			Voting.StartOrEnqueueVote(subVote);
			GameMain.Server.UpdateVoteStatus(false);
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x0006C74D File Offset: 0x0006A94D
		public void StopSubmarineVote(bool passed)
		{
			if (!(Voting.ActiveVote is Voting.SubmarineVote))
			{
				return;
			}
			this.StopActiveVote(passed);
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x0006C763 File Offset: 0x0006A963
		public void StopMoneyTransferVote(bool passed)
		{
			if (!(Voting.ActiveVote is Voting.TransferVote))
			{
				return;
			}
			this.StopActiveVote(passed);
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x0006C77C File Offset: 0x0006A97C
		public void StopActiveVote(bool passed)
		{
			Voting.ActiveVote.State = (passed ? Voting.VoteState.Passed : Voting.VoteState.Failed);
			GameMain.Server.UpdateVoteStatus(false);
			for (int i = 0; i < GameMain.NetworkMember.ConnectedClients.Count; i++)
			{
				GameMain.NetworkMember.ConnectedClients[i].SetVote(Voting.ActiveVote.VoteType, 0);
			}
			Voting.ActiveVote = null;
			if (Voting.pendingVotes.Any<Voting.IVote>())
			{
				Voting.ActiveVote = Voting.pendingVotes.Dequeue();
				Client voteStarter = Voting.ActiveVote.VoteStarter;
				if (voteStarter == null)
				{
					return;
				}
				voteStarter.SetVote(Voting.ActiveVote.VoteType, 2);
			}
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x0006C829 File Offset: 0x0006AA29
		public void StartTransferVote(Client starter, Client from, int transferAmount, Client to)
		{
			if (this.ShouldRejectVote(starter, VoteType.TransferMoney))
			{
				return;
			}
			Voting.StartOrEnqueueVote(new Voting.TransferVote(starter, from, transferAmount, to));
			GameMain.Server.UpdateVoteStatus(false);
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x0006C851 File Offset: 0x0006AA51
		private static void StartOrEnqueueVote(Voting.IVote vote)
		{
			if (Voting.ActiveVote == null)
			{
				Voting.ActiveVote = vote;
				return;
			}
			Voting.pendingVotes.Enqueue(vote);
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x0006C86C File Offset: 0x0006AA6C
		public bool CanVoteToStartRound(Client client)
		{
			return !client.AFK || !GameMain.Server.ServerSettings.AllowAFK;
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x0006C88A File Offset: 0x0006AA8A
		public bool CanVoteToEndRound(Client client)
		{
			return client.HasSpawned && client.InGame;
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x0006C89C File Offset: 0x0006AA9C
		private bool ShouldRejectVote(Client sender, VoteType voteType)
		{
			if (this.rejectedVoteTimes.ContainsKey(sender))
			{
				TimeSpan remainingCooldown = this.rejectedVoteTimes[sender].Item2 + this.rejectedVoteCooldown - DateTime.Now;
				if (this.rejectedVoteTimes[sender].Item1 == voteType && remainingCooldown.TotalSeconds > 0.0)
				{
					GameMain.Server.SendDirectChatMessage(TextManager.FormatServerMessage("voterejectedpleasewait", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[time]", ((int)remainingCooldown.TotalSeconds).ToString())
					}), sender, ChatMessageType.ServerMessageBox);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x0006C948 File Offset: 0x0006AB48
		protected void RegisterRejectedVote(Voting.IVote vote)
		{
			if (vote.VoteStarter != null)
			{
				this.rejectedVoteTimes[vote.VoteStarter] = new ValueTuple<VoteType, DateTime>(vote.VoteType, DateTime.Now);
			}
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x0006C974 File Offset: 0x0006AB74
		public void Update(float deltaTime)
		{
			if (Voting.ActiveVote == null)
			{
				return;
			}
			Voting.ActiveVote.Timer += deltaTime;
			IEnumerable<Client> inGameClients = from c in GameMain.Server.ConnectedClients
			where c.InGame
			select c;
			if (Voting.ActiveVote.Timer >= GameMain.NetworkMember.ServerSettings.VoteTimeout || inGameClients.Count<Client>() == 1)
			{
				IEnumerable<Client> eligibleClients = from c in inGameClients
				where c != Voting.ActiveVote.VoteStarter
				select c;
				int yes = eligibleClients.Count((Client c) => c.GetVote<int>(Voting.ActiveVote.VoteType) == 2);
				int no = eligibleClients.Count((Client c) => c.GetVote<int>(Voting.ActiveVote.VoteType) == 1);
				int total = yes + no;
				bool passed = false;
				if (total > 0)
				{
					passed = ((float)yes / (float)total >= GameMain.NetworkMember.ServerSettings.VoteRequiredRatio || inGameClients.Count<Client>() == 1);
				}
				Voting.ActiveVote.Finish(this, passed);
			}
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x0006CAA4 File Offset: 0x0006ACA4
		public static void ResetVotes(IEnumerable<Client> connectedClients, bool resetKickVotes)
		{
			foreach (Client client in connectedClients)
			{
				client.ResetVotes(resetKickVotes);
			}
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x0006CAEC File Offset: 0x0006ACEC
		public void ServerRead(IReadMessage inc, Client sender, DoSProtection dosProtection)
		{
			if (GameMain.Server == null || sender == null)
			{
				return;
			}
			byte voteTypeByte = inc.ReadByte();
			VoteType voteType = VoteType.Unknown;
			try
			{
				voteType = (VoteType)voteTypeByte;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to cast vote type \"" + voteTypeByte.ToString() + "\"", e, null, false, false);
				return;
			}
			switch (voteType)
			{
			case VoteType.Sub:
			{
				int equalityCheckVal = inc.ReadInt32();
				string hash = (equalityCheckVal > 0) ? string.Empty : inc.ReadString();
				SubmarineInfo sub = (equalityCheckVal > 0) ? SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Type == SubmarineType.Player && s.EqualityCheckVal == equalityCheckVal) : SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Type == SubmarineType.Player && s.MD5Hash.StringRepresentation == hash);
				sender.SetVote(voteType, sub);
				break;
			}
			case VoteType.Mode:
			{
				string modeIdentifier = inc.ReadString();
				GameModePreset mode = GameModePreset.List.Find((GameModePreset gm) => gm.Identifier == modeIdentifier);
				if (mode != null && mode.Votable)
				{
					GameModePreset prevHighestVoted = Voting.HighestVoted<GameModePreset>(VoteType.Mode, GameMain.Server.ConnectedClients);
					sender.SetVote(voteType, mode);
					GameModePreset newHighestVoted = Voting.HighestVoted<GameModePreset>(VoteType.Mode, GameMain.Server.ConnectedClients);
					if (prevHighestVoted != newHighestVoted)
					{
						GameMain.NetLobbyScreen.SelectedModeIdentifier = mode.Identifier;
						NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
						ushort lastUpdateID = netLobbyScreen.LastUpdateID;
						netLobbyScreen.LastUpdateID = lastUpdateID + 1;
					}
				}
				break;
			}
			case VoteType.EndRound:
				if (!sender.HasSpawned)
				{
					return;
				}
				sender.SetVote(voteType, inc.ReadBoolean());
				break;
			case VoteType.Kick:
			{
				byte kickedClientID = inc.ReadByte();
				if (!GameMain.Server.ServerSettings.AllowVoteKick)
				{
					DebugConsole.ThrowError("Client " + sender.Name + " attempted to vote to kick a client, even though vote kicking is disabled. Ignoring the vote.", null, null, false, false);
				}
				else if ((DateTime.Now - sender.JoinTime).TotalSeconds < (double)GameMain.Server.ServerSettings.DisallowKickVoteTime)
				{
					GameMain.Server.SendDirectChatMessage("ServerMessage.kickvotedisallowed", sender, ChatMessageType.Server);
				}
				else
				{
					Client kicked = GameMain.Server.ConnectedClients.Find((Client c) => c.SessionId == kickedClientID);
					if (kicked != null && kicked.Connection != GameMain.Server.OwnerConnection && !kicked.HasKickVoteFrom(sender))
					{
						kicked.AddKickVote(sender);
						Client.UpdateKickVotes(GameMain.Server.ConnectedClients);
						GameMain.Server.SendChatMessage("ServerMessage.HasVotedToKick~[initiator]=" + sender.Name + "~[target]=" + kicked.Name, new ChatMessageType?(ChatMessageType.Server), null, null, PlayerConnectionChangeType.None, ChatMode.None);
					}
				}
				break;
			}
			case VoteType.StartRound:
			{
				bool ready = inc.ReadBoolean();
				if (ready != sender.GetVote<bool>(VoteType.StartRound))
				{
					sender.SetVote(VoteType.StartRound, ready);
					GameServer.Log(NetworkMember.ClientLogName(sender, null) + (ready ? " is ready to start the game." : " is not ready to start the game."), ServerLog.MessageType.ServerMessage);
				}
				break;
			}
			case VoteType.PurchaseAndSwitchSub:
			case VoteType.PurchaseSub:
			case VoteType.SwitchSub:
			case VoteType.TransferMoney:
			{
				bool startVote = inc.ReadBoolean();
				if (startVote)
				{
					if (voteType == VoteType.TransferMoney)
					{
						int amount = inc.ReadInt32();
						int fromClientId = (int)inc.ReadByte();
						int toClientId = (int)inc.ReadByte();
						if (!this.ShouldRejectVote(sender, voteType))
						{
							Voting.pendingVotes.Enqueue(new Voting.TransferVote(sender, GameMain.Server.ConnectedClients.Find((Client c) => (int)c.SessionId == fromClientId), amount, GameMain.Server.ConnectedClients.Find((Client c) => (int)c.SessionId == toClientId)));
						}
					}
					else
					{
						string subName = inc.ReadString();
						SubmarineInfo subInfo = GameMain.GameSession.OwnedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName) ?? SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == subName);
						bool transferItems = inc.ReadBoolean();
						if (!this.ShouldRejectVote(sender, voteType))
						{
							GameSession gameSession = GameMain.GameSession;
							MultiPlayerCampaign campaign = ((gameSession != null) ? gameSession.Campaign : null) as MultiPlayerCampaign;
							if (campaign != null && (campaign.CanPurchaseSub(subInfo, sender) || GameMain.GameSession.IsSubmarineOwned(subInfo)))
							{
								this.StartSubmarineVote(subInfo, transferItems, voteType, sender);
							}
						}
					}
				}
				else
				{
					sender.SetVote(voteType, (int)inc.ReadByte());
				}
				break;
			}
			case VoteType.Traitor:
			{
				int clientId = inc.ReadInt32();
				if (sender.InGame && sender.Character != null)
				{
					Client client = GameMain.Server.ConnectedClients.FirstOrDefault((Client c) => (int)c.SessionId == clientId);
					sender.SetVote(voteType, client);
					if (((client != null) ? client.Character : null) != null)
					{
						string msg = TextManager.GetWithVariable("traitor.blamebutton.dialog", "[name]", client.Character.DisplayName, FormatCapitals.No).Value;
						bool flaggedAsSpam;
						ChatMessage.HandleSpamFilter(sender, msg, out flaggedAsSpam, 1f);
						if (!flaggedAsSpam)
						{
							GameMain.Server.SendChatMessage(msg, new ChatMessageType?(ChatMessageType.Radio), sender, sender.Character, PlayerConnectionChangeType.None, ChatMode.None);
							sender.LastSentChatMessages.Add(msg);
						}
					}
				}
				break;
			}
			}
			inc.ReadPadBits();
			using (dosProtection.Pause(sender))
			{
				GameMain.Server.UpdateVoteStatus(true);
			}
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x0006D058 File Offset: 0x0006B258
		public void ServerWrite(IWriteMessage msg)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			msg.WriteBoolean(GameMain.Server.ServerSettings.AllowSubVoting);
			if (GameMain.Server.ServerSettings.AllowSubVoting)
			{
				bool isMultiSub = GameMain.NetLobbyScreen.SelectedMode == GameModePreset.PvP;
				msg.WriteBoolean(isMultiSub);
				IEnumerable<Client> enumerable;
				if (!isMultiSub)
				{
					IEnumerable<Client> connectedClients = GameMain.Server.ConnectedClients;
					enumerable = connectedClients;
				}
				else
				{
					enumerable = from c in GameMain.Server.ConnectedClients
					where c.PreferredTeam == CharacterTeamType.Team1
					select c;
				}
				IEnumerable<Client> subVoters = enumerable;
				IReadOnlyDictionary<SubmarineInfo, int> voteList = Voting.GetVoteCounts<SubmarineInfo>(VoteType.Sub, subVoters);
				msg.WriteByte((byte)voteList.Count);
				foreach (KeyValuePair<SubmarineInfo, int> vote in voteList)
				{
					msg.WriteByte((byte)vote.Value);
					msg.WriteString(vote.Key.Name);
				}
				if (isMultiSub)
				{
					IReadOnlyDictionary<SubmarineInfo, int> separatistsVotes = Voting.GetVoteCounts<SubmarineInfo>(VoteType.Sub, from c in GameMain.Server.ConnectedClients
					where c.PreferredTeam == CharacterTeamType.Team2
					select c);
					msg.WriteByte((byte)separatistsVotes.Count);
					foreach (KeyValuePair<SubmarineInfo, int> keyValuePair in separatistsVotes)
					{
						SubmarineInfo submarineInfo;
						int num;
						keyValuePair.Deconstruct(out submarineInfo, out num);
						SubmarineInfo info = submarineInfo;
						int amount = num;
						msg.WriteByte((byte)amount);
						msg.WriteString(info.Name);
					}
				}
			}
			msg.WriteBoolean(GameMain.Server.ServerSettings.AllowModeVoting);
			if (GameMain.Server.ServerSettings.AllowModeVoting)
			{
				IReadOnlyDictionary<GameModePreset, int> voteList2 = Voting.GetVoteCounts<GameModePreset>(VoteType.Mode, GameMain.Server.ConnectedClients);
				msg.WriteByte((byte)voteList2.Count);
				foreach (KeyValuePair<GameModePreset, int> vote2 in voteList2)
				{
					msg.WriteByte((byte)vote2.Value);
					msg.WriteIdentifier(vote2.Key.Identifier);
				}
			}
			msg.WriteBoolean(GameMain.Server.ServerSettings.AllowEndVoting);
			if (GameMain.Server.ServerSettings.AllowEndVoting)
			{
				msg.WriteByte((byte)GameMain.Server.ConnectedClients.Count((Client c) => this.CanVoteToEndRound(c) && c.GetVote<bool>(VoteType.EndRound)));
				msg.WriteByte((byte)GameMain.Server.ConnectedClients.Count((Client c) => this.CanVoteToEndRound(c)));
			}
			msg.WriteBoolean(GameMain.Server.ServerSettings.AllowVoteKick);
			Voting.IVote activeVote = Voting.ActiveVote;
			msg.WriteByte((byte)((activeVote != null) ? activeVote.State : Voting.VoteState.None));
			if (Voting.ActiveVote != null)
			{
				msg.WriteByte((byte)Voting.ActiveVote.VoteType);
				if (Voting.ActiveVote.State != Voting.VoteState.None && Voting.ActiveVote.VoteType != VoteType.Unknown)
				{
					IEnumerable<Client> eligibleClients = from c in GameMain.Server.ConnectedClients
					where c.InGame && c != Voting.ActiveVote.VoteStarter
					select c;
					IEnumerable<Client> yesClients = from c in eligibleClients
					where c.GetVote<int>(Voting.ActiveVote.VoteType) == 2
					select c;
					msg.WriteByte((byte)yesClients.Count<Client>());
					foreach (Client c4 in yesClients)
					{
						msg.WriteByte(c4.SessionId);
					}
					IEnumerable<Client> noClients = from c in eligibleClients
					where c.GetVote<int>(Voting.ActiveVote.VoteType) == 1
					select c;
					msg.WriteByte((byte)noClients.Count<Client>());
					foreach (Client c2 in noClients)
					{
						msg.WriteByte(c2.SessionId);
					}
					msg.WriteByte((byte)eligibleClients.Count<Client>());
					switch (Voting.ActiveVote.State)
					{
					case Voting.VoteState.Started:
					{
						msg.WriteByte(Voting.ActiveVote.VoteStarter.SessionId);
						msg.WriteByte((byte)GameMain.Server.ServerSettings.VoteTimeout);
						VoteType voteType = Voting.ActiveVote.VoteType;
						if (voteType - VoteType.PurchaseAndSwitchSub > 2)
						{
							if (voteType == VoteType.TransferMoney)
							{
								Voting.TransferVote transferVote = Voting.ActiveVote as Voting.TransferVote;
								Client from = transferVote.From;
								msg.WriteByte((from != null) ? from.SessionId : 0);
								Client to = transferVote.To;
								msg.WriteByte((to != null) ? to.SessionId : 0);
								msg.WriteInt32(transferVote.TransferAmount);
							}
						}
						else
						{
							Voting.SubmarineVote vote3 = Voting.ActiveVote as Voting.SubmarineVote;
							msg.WriteString(vote3.Sub.Name);
							msg.WriteBoolean(vote3.TransferItems);
						}
						break;
					}
					case Voting.VoteState.Passed:
					case Voting.VoteState.Failed:
					{
						msg.WriteBoolean(Voting.ActiveVote.State == Voting.VoteState.Passed);
						VoteType voteType2 = Voting.ActiveVote.VoteType;
						if (voteType2 - VoteType.PurchaseAndSwitchSub <= 2)
						{
							Voting.SubmarineVote subVote = Voting.ActiveVote as Voting.SubmarineVote;
							msg.WriteString(subVote.Sub.Name);
							msg.WriteBoolean(subVote.TransferItems);
						}
						break;
					}
					}
				}
			}
			IEnumerable<Client> readyClients = from c in GameMain.Server.ConnectedClients
			where c.GetVote<bool>(VoteType.StartRound)
			select c;
			msg.WriteByte((byte)readyClients.Count<Client>());
			foreach (Client c3 in readyClients)
			{
				msg.WriteByte(c3.SessionId);
			}
			msg.WritePadBits();
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x0006D670 File Offset: 0x0006B870
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

		// Token: 0x06000B50 RID: 2896 RVA: 0x0006D6F4 File Offset: 0x0006B8F4
		public static T HighestVoted<T>(VoteType voteType, IEnumerable<Client> voters)
		{
			int num;
			return Voting.HighestVoted<T>(voteType, voters, out num);
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x0006D70C File Offset: 0x0006B90C
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

		// Token: 0x040004E5 RID: 1253
		public static Voting.IVote ActiveVote;

		// Token: 0x040004E6 RID: 1254
		private static readonly Queue<Voting.IVote> pendingVotes = new Queue<Voting.IVote>();

		// Token: 0x040004E7 RID: 1255
		private readonly TimeSpan rejectedVoteCooldown = new TimeSpan(0, 1, 0);

		// Token: 0x040004E8 RID: 1256
		[TupleElementNames(new string[]
		{
			"voteType",
			"time"
		})]
		private readonly Dictionary<Client, ValueTuple<VoteType, DateTime>> rejectedVoteTimes = new Dictionary<Client, ValueTuple<VoteType, DateTime>>();

		// Token: 0x02000741 RID: 1857
		public interface IVote
		{
			// Token: 0x17001427 RID: 5159
			// (get) Token: 0x06005158 RID: 20824
			Client VoteStarter { get; }

			// Token: 0x17001428 RID: 5160
			// (get) Token: 0x06005159 RID: 20825
			VoteType VoteType { get; }

			// Token: 0x17001429 RID: 5161
			// (get) Token: 0x0600515A RID: 20826
			// (set) Token: 0x0600515B RID: 20827
			float Timer { get; set; }

			// Token: 0x1700142A RID: 5162
			// (get) Token: 0x0600515C RID: 20828
			// (set) Token: 0x0600515D RID: 20829
			Voting.VoteState State { get; set; }

			// Token: 0x0600515E RID: 20830
			void Finish(Voting voting, bool passed);
		}

		// Token: 0x02000742 RID: 1858
		public class SubmarineVote : Voting.IVote
		{
			// Token: 0x1700142B RID: 5163
			// (get) Token: 0x0600515F RID: 20831 RVA: 0x001E91E2 File Offset: 0x001E73E2
			public Client VoteStarter { get; }

			// Token: 0x1700142C RID: 5164
			// (get) Token: 0x06005160 RID: 20832 RVA: 0x001E91EA File Offset: 0x001E73EA
			public VoteType VoteType { get; }

			// Token: 0x1700142D RID: 5165
			// (get) Token: 0x06005161 RID: 20833 RVA: 0x001E91F2 File Offset: 0x001E73F2
			// (set) Token: 0x06005162 RID: 20834 RVA: 0x001E91FA File Offset: 0x001E73FA
			public float Timer { get; set; }

			// Token: 0x1700142E RID: 5166
			// (get) Token: 0x06005163 RID: 20835 RVA: 0x001E9203 File Offset: 0x001E7403
			// (set) Token: 0x06005164 RID: 20836 RVA: 0x001E920B File Offset: 0x001E740B
			public Voting.VoteState State { get; set; }

			// Token: 0x06005165 RID: 20837 RVA: 0x001E9214 File Offset: 0x001E7414
			public SubmarineVote(Client starter, SubmarineInfo subInfo, bool transferItems, VoteType voteType)
			{
				this.Sub = subInfo;
				this.TransferItems = transferItems;
				this.VoteType = voteType;
				this.State = Voting.VoteState.Started;
				this.VoteStarter = starter;
			}

			// Token: 0x06005166 RID: 20838 RVA: 0x001E9240 File Offset: 0x001E7440
			public void Finish(Voting voting, bool passed)
			{
				if (passed)
				{
					if (GameMain.Server != null && !GameMain.Server.TrySwitchSubmarine())
					{
						passed = false;
						this.State = Voting.VoteState.Failed;
					}
				}
				else
				{
					voting.RegisterRejectedVote(this);
				}
				voting.StopSubmarineVote(passed);
			}

			// Token: 0x04002C68 RID: 11368
			public SubmarineInfo Sub;

			// Token: 0x04002C69 RID: 11369
			public bool TransferItems;
		}

		// Token: 0x02000743 RID: 1859
		public class TransferVote : Voting.IVote
		{
			// Token: 0x1700142F RID: 5167
			// (get) Token: 0x06005167 RID: 20839 RVA: 0x001E9272 File Offset: 0x001E7472
			public Client VoteStarter { get; }

			// Token: 0x17001430 RID: 5168
			// (get) Token: 0x06005168 RID: 20840 RVA: 0x001E927A File Offset: 0x001E747A
			public VoteType VoteType { get; }

			// Token: 0x17001431 RID: 5169
			// (get) Token: 0x06005169 RID: 20841 RVA: 0x001E9282 File Offset: 0x001E7482
			// (set) Token: 0x0600516A RID: 20842 RVA: 0x001E928A File Offset: 0x001E748A
			public float Timer { get; set; }

			// Token: 0x17001432 RID: 5170
			// (get) Token: 0x0600516B RID: 20843 RVA: 0x001E9293 File Offset: 0x001E7493
			// (set) Token: 0x0600516C RID: 20844 RVA: 0x001E929B File Offset: 0x001E749B
			public Voting.VoteState State { get; set; }

			// Token: 0x0600516D RID: 20845 RVA: 0x001E92A4 File Offset: 0x001E74A4
			public TransferVote(Client starter, Client from, int transferAmount, Client to)
			{
				this.VoteStarter = starter;
				this.From = from;
				this.To = to;
				this.TransferAmount = transferAmount;
				this.State = Voting.VoteState.Started;
				this.VoteType = 9;
			}

			// Token: 0x0600516E RID: 20846 RVA: 0x001E92D8 File Offset: 0x001E74D8
			public void Finish(Voting voting, bool passed)
			{
				if (passed)
				{
					Wallet wallet;
					if (this.From != null)
					{
						Character character = this.From.Character;
						wallet = ((character != null) ? character.Wallet : null);
					}
					else
					{
						MultiPlayerCampaign multiPlayerCampaign = GameMain.GameSession.GameMode as MultiPlayerCampaign;
						wallet = ((multiPlayerCampaign != null) ? multiPlayerCampaign.Bank : null);
					}
					Wallet fromWallet = wallet;
					if (fromWallet != null && fromWallet.TryDeduct(this.TransferAmount))
					{
						Wallet wallet2;
						if (this.To != null)
						{
							Character character2 = this.To.Character;
							wallet2 = ((character2 != null) ? character2.Wallet : null);
						}
						else
						{
							MultiPlayerCampaign multiPlayerCampaign2 = GameMain.GameSession.GameMode as MultiPlayerCampaign;
							wallet2 = ((multiPlayerCampaign2 != null) ? multiPlayerCampaign2.Bank : null);
						}
						Wallet toWallet = wallet2;
						if (toWallet != null)
						{
							toWallet.Give(this.TransferAmount);
						}
					}
				}
				else
				{
					voting.RegisterRejectedVote(this);
				}
				voting.StopMoneyTransferVote(passed);
			}

			// Token: 0x04002C6E RID: 11374
			public readonly Client From;

			// Token: 0x04002C6F RID: 11375
			public readonly Client To;

			// Token: 0x04002C70 RID: 11376
			public readonly int TransferAmount;
		}

		// Token: 0x02000744 RID: 1860
		public enum VoteState
		{
			// Token: 0x04002C72 RID: 11378
			None,
			// Token: 0x04002C73 RID: 11379
			Started,
			// Token: 0x04002C74 RID: 11380
			Running,
			// Token: 0x04002C75 RID: 11381
			Passed,
			// Token: 0x04002C76 RID: 11382
			Failed
		}
	}
}
