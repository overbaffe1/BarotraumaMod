using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000BF RID: 191
	internal class VotingInterface
	{
		// Token: 0x170005DB RID: 1499
		// (get) Token: 0x060017A1 RID: 6049 RVA: 0x000E9511 File Offset: 0x000E7711
		private static Color SubmarineColor
		{
			get
			{
				return GUIStyle.Orange;
			}
		}

		// Token: 0x170005DC RID: 1500
		// (get) Token: 0x060017A2 RID: 6050 RVA: 0x000E951D File Offset: 0x000E771D
		public bool TimedOut
		{
			get
			{
				return this.VoteRunning && this.timer - this.votingTime > 10f;
			}
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x000E9540 File Offset: 0x000E7740
		public static VotingInterface CreateSubmarineVotingInterface(Client starter, SubmarineInfo info, VoteType type, bool transferItems, float votingTime)
		{
			if (starter == null || info == null)
			{
				return null;
			}
			VotingInterface subVoting = new VotingInterface
			{
				votingTime = votingTime,
				getYesVotes = delegate()
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					int? num;
					if (networkMember == null)
					{
						num = null;
					}
					else
					{
						Voting voting = networkMember.Voting;
						num = ((voting != null) ? new int?(voting.GetVoteCountYes(type)) : null);
					}
					int? num2 = num;
					return num2.GetValueOrDefault();
				},
				getNoVotes = delegate()
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					int? num;
					if (networkMember == null)
					{
						num = null;
					}
					else
					{
						Voting voting = networkMember.Voting;
						num = ((voting != null) ? new int?(voting.GetVoteCountNo(type)) : null);
					}
					int? num2 = num;
					return num2.GetValueOrDefault();
				},
				getMaxVotes = delegate()
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					int? num;
					if (networkMember == null)
					{
						num = null;
					}
					else
					{
						Voting voting = networkMember.Voting;
						num = ((voting != null) ? new int?(voting.GetVoteCountMax(type)) : null);
					}
					int? num2 = num;
					return num2.GetValueOrDefault();
				}
			};
			subVoting.onVoteEnd = delegate()
			{
				subVoting.SendSubmarineVoteEndMessage(info, type);
			};
			subVoting.SetSubmarineVotingText(starter, info, transferItems, type);
			subVoting.Initialize(starter, type);
			return subVoting;
		}

		// Token: 0x060017A4 RID: 6052 RVA: 0x000E9600 File Offset: 0x000E7800
		public static VotingInterface CreateMoneyTransferVotingInterface(Client starter, Client from, Client to, int amount, float votingTime)
		{
			VotingInterface.<>c__DisplayClass26_0 CS$<>8__locals1 = new VotingInterface.<>c__DisplayClass26_0();
			CS$<>8__locals1.from = from;
			CS$<>8__locals1.to = to;
			CS$<>8__locals1.amount = amount;
			if (starter == null)
			{
				return null;
			}
			if (CS$<>8__locals1.from == null && CS$<>8__locals1.to == null)
			{
				return null;
			}
			VotingInterface.<>c__DisplayClass26_0 CS$<>8__locals2 = CS$<>8__locals1;
			VotingInterface votingInterface = new VotingInterface();
			votingInterface.votingTime = votingTime;
			votingInterface.getYesVotes = delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				int? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					Voting voting = networkMember.Voting;
					num = ((voting != null) ? new int?(voting.GetVoteCountYes(VoteType.TransferMoney)) : null);
				}
				int? num2 = num;
				return num2.GetValueOrDefault();
			};
			votingInterface.getNoVotes = delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				int? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					Voting voting = networkMember.Voting;
					num = ((voting != null) ? new int?(voting.GetVoteCountNo(VoteType.TransferMoney)) : null);
				}
				int? num2 = num;
				return num2.GetValueOrDefault();
			};
			votingInterface.getMaxVotes = delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				int? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					Voting voting = networkMember.Voting;
					num = ((voting != null) ? new int?(voting.GetVoteCountMax(VoteType.TransferMoney)) : null);
				}
				int? num2 = num;
				return num2.GetValueOrDefault();
			};
			CS$<>8__locals2.transferVoting = votingInterface;
			CS$<>8__locals1.transferVoting.onVoteEnd = delegate()
			{
				CS$<>8__locals1.transferVoting.SendMoneyTransferVoteEndMessage(CS$<>8__locals1.from, CS$<>8__locals1.to, CS$<>8__locals1.amount);
			};
			CS$<>8__locals1.transferVoting.SetMoneyTransferVotingText(starter, CS$<>8__locals1.from, CS$<>8__locals1.to, CS$<>8__locals1.amount);
			CS$<>8__locals1.transferVoting.Initialize(starter, VoteType.TransferMoney);
			return CS$<>8__locals1.transferVoting;
		}

		// Token: 0x060017A5 RID: 6053 RVA: 0x000E970A File Offset: 0x000E790A
		private void Initialize(Client starter, VoteType type)
		{
			this.currentVoteType = type;
			this.CreateVotingGUI();
			if (starter.SessionId == GameMain.Client.SessionId)
			{
				this.SetGUIToVotedState(2);
			}
			this.VoteRunning = true;
		}

		// Token: 0x060017A6 RID: 6054 RVA: 0x000E973C File Offset: 0x000E793C
		private void CreateVotingGUI()
		{
			this.createdForResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			GUIFrame guiframe = this.frame;
			if (guiframe != null)
			{
				guiframe.Parent.RemoveChild(this.frame);
			}
			this.frame = new GUIFrame(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.VotingArea, GameMain.Client.InGameHUD.RectTransform), "", null);
			int padding = HUDLayoutSettings.Padding * 2;
			int spacing = HUDLayoutSettings.Padding;
			int yOffset = padding;
			int paddedWidth = this.frame.Rect.Width - padding * 2;
			this.votingTextBlock = new GUITextBlock(new RectTransform(new Point(paddedWidth, 0), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), this.votingOnText, null, null, Alignment.Left, true, "", null);
			RectTransform rectTransform = this.votingTextBlock.RectTransform;
			RectTransform rectTransform2 = this.votingTextBlock.RectTransform;
			RectTransform rectTransform3 = this.votingTextBlock.RectTransform;
			Point point = new Point(this.votingTextBlock.Rect.Width, this.votingTextBlock.Rect.Height);
			rectTransform3.MaxSize = point;
			rectTransform.NonScaledSize = (rectTransform2.MinSize = point);
			this.votingTextBlock.RectTransform.IsFixedSize = true;
			this.votingTextBlock.RectTransform.AbsoluteOffset = new Point(padding, yOffset);
			yOffset += this.votingTextBlock.Rect.Height + spacing;
			this.voteCounter = new GUITextBlock(new RectTransform(new Point(paddedWidth, 0), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "(0/0)", new Color?(GUIStyle.Green), null, Alignment.Center, false, "", null);
			RectTransform rectTransform4 = this.voteCounter.RectTransform;
			RectTransform rectTransform5 = this.voteCounter.RectTransform;
			RectTransform rectTransform6 = this.voteCounter.RectTransform;
			point = new Point(this.voteCounter.Rect.Width, this.voteCounter.Rect.Height);
			rectTransform6.MaxSize = point;
			rectTransform4.NonScaledSize = (rectTransform5.MinSize = point);
			this.voteCounter.RectTransform.IsFixedSize = true;
			this.voteCounter.RectTransform.AbsoluteOffset = new Point(padding, yOffset);
			yOffset += this.voteCounter.Rect.Height + spacing;
			this.votingTimer = new GUIProgressBar(new RectTransform(new Point(paddedWidth, Math.Max(spacing, 8)), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(padding, yOffset)
			}, (float)HUDLayoutSettings.Padding, null, "", true);
			this.votingTimer.RectTransform.IsFixedSize = true;
			yOffset += this.votingTimer.Rect.Height + spacing;
			int buttonWidth = (int)((float)paddedWidth * 0.3f);
			this.yesVoteButton = new GUIButton(new RectTransform(new Point(buttonWidth, 0), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point((int)((float)this.frame.Rect.Width / 2f - (float)buttonWidth - (float)spacing), yOffset)
			}, TextManager.Get("yes"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton applyButton, object obj)
				{
					this.SetGUIToVotedState(2);
					GameMain.Client.Vote(this.currentVoteType, 2);
					return true;
				}
			};
			this.noVoteButton = new GUIButton(new RectTransform(new Point(buttonWidth, 0), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(this.yesVoteButton.RectTransform.AbsoluteOffset.X + this.yesVoteButton.Rect.Width + padding, yOffset)
			}, TextManager.Get("no"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton applyButton, object obj)
				{
					this.SetGUIToVotedState(1);
					GameMain.Client.Vote(this.currentVoteType, 1);
					return true;
				}
			};
			this.votedTextBlock = new GUITextBlock(new RectTransform(new Point(paddedWidth, this.yesVoteButton.Rect.Height), this.frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), string.Empty, null, null, Alignment.Center, false, "", null);
			this.votedTextBlock.RectTransform.IsFixedSize = true;
			this.votedTextBlock.RectTransform.AbsoluteOffset = new Point(padding, yOffset);
			this.votedTextBlock.Visible = false;
			yOffset += this.yesVoteButton.Rect.Height;
			this.frame.RectTransform.NonScaledSize = new Point(this.frame.Rect.Width, yOffset + padding);
		}

		// Token: 0x060017A7 RID: 6055 RVA: 0x000E9C30 File Offset: 0x000E7E30
		private void SetGUIToVotedState(int vote)
		{
			this.yesVoteButton.Visible = (this.noVoteButton.Visible = false);
			this.votedTextBlock.Text = TextManager.Get((vote == 2) ? "yesvoted" : "novoted");
			this.votedTextBlock.Visible = true;
		}

		// Token: 0x060017A8 RID: 6056 RVA: 0x000E9C88 File Offset: 0x000E7E88
		public void Update(float deltaTime)
		{
			if (!this.VoteRunning)
			{
				return;
			}
			if (GameMain.GraphicsWidth != this.createdForResolution.X || GameMain.GraphicsHeight != this.createdForResolution.Y)
			{
				this.CreateVotingGUI();
			}
			this.yesVotes = this.getYesVotes();
			this.noVotes = this.getNoVotes();
			this.maxVotes = this.getMaxVotes();
			GUITextBlock guitextBlock = this.voteCounter;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.yesVotes + this.noVotes);
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.maxVotes);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			guitextBlock.Text = defaultInterpolatedStringHandler.ToStringAndClear();
			this.timer += deltaTime;
			this.votingTimer.BarSize = this.timer / this.votingTime;
		}

		// Token: 0x060017A9 RID: 6057 RVA: 0x000E9D86 File Offset: 0x000E7F86
		public void EndVote(bool passed, int yesVoteFinal, int noVoteFinal)
		{
			this.VoteRunning = false;
			this.votePassed = passed;
			this.yesVotes = yesVoteFinal;
			this.noVotes = noVoteFinal;
			Action action = this.onVoteEnd;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x060017AA RID: 6058 RVA: 0x000E9DB4 File Offset: 0x000E7FB4
		private void SetSubmarineVotingText(Client starter, SubmarineInfo info, bool transferItems, VoteType type)
		{
			int price = info.GetPrice(null, null);
			string name = starter.Name;
			JobPrefab jobPrefab;
			if (starter == null)
			{
				jobPrefab = null;
			}
			else
			{
				Character character = starter.Character;
				if (character == null)
				{
					jobPrefab = null;
				}
				else
				{
					CharacterInfo info2 = character.Info;
					if (info2 == null)
					{
						jobPrefab = null;
					}
					else
					{
						Job job = info2.Job;
						jobPrefab = ((job != null) ? job.Prefab : null);
					}
				}
			}
			JobPrefab prefab = jobPrefab;
			Color nameColor = (prefab != null) ? prefab.UIColor : Color.White;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.R);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.G);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.B);
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
			string characterRichString = defaultInterpolatedStringHandler.ToStringAndClear();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler2.AppendFormatted<byte>(VotingInterface.SubmarineColor.R);
			defaultInterpolatedStringHandler2.AppendLiteral(",");
			defaultInterpolatedStringHandler2.AppendFormatted<byte>(VotingInterface.SubmarineColor.G);
			defaultInterpolatedStringHandler2.AppendLiteral(",");
			defaultInterpolatedStringHandler2.AppendFormatted<byte>(VotingInterface.SubmarineColor.B);
			defaultInterpolatedStringHandler2.AppendLiteral("‖");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(info.DisplayName);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:end‖");
			string submarineRichString = defaultInterpolatedStringHandler2.ToStringAndClear();
			string tag = string.Empty;
			LocalizedString text = string.Empty;
			switch (type)
			{
			case VoteType.PurchaseAndSwitchSub:
				tag = (transferItems ? "submarinepurchaseandswitchwithitemsvote" : "submarinepurchaseandswitchvote");
				text = TextManager.GetWithVariables(tag, new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[playername]", characterRichString),
					new ValueTuple<string, LocalizedString>("[submarinename]", submarineRichString),
					new ValueTuple<string, LocalizedString>("[amount]", price.ToString()),
					new ValueTuple<string, LocalizedString>("[currencyname]", TextManager.Get("credit").ToLower())
				});
				break;
			case VoteType.PurchaseSub:
				text = TextManager.GetWithVariables("submarinepurchasevote", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[playername]", characterRichString),
					new ValueTuple<string, LocalizedString>("[submarinename]", submarineRichString),
					new ValueTuple<string, LocalizedString>("[amount]", price.ToString()),
					new ValueTuple<string, LocalizedString>("[currencyname]", TextManager.Get("credit").ToLower())
				});
				break;
			case VoteType.SwitchSub:
				tag = (transferItems ? "submarineswitchwithitemsnofeevote" : "submarineswitchnofeevote");
				text = TextManager.GetWithVariables(tag, new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[playername]", characterRichString),
					new ValueTuple<string, string>("[submarinename]", submarineRichString)
				});
				break;
			}
			this.votingOnText = RichString.Rich(text, null);
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x000EA0C4 File Offset: 0x000E82C4
		private void SendSubmarineVoteEndMessage(SubmarineInfo info, VoteType type)
		{
			GameMain.NetworkMember.AddChatMessage(this.GetSubmarineVoteResultMessage(info, type, this.yesVotes, this.noVotes, this.votePassed).Value, ChatMessageType.Server, "", null, null, PlayerConnectionChangeType.None, null);
		}

		// Token: 0x060017AC RID: 6060 RVA: 0x000EA10C File Offset: 0x000E830C
		private LocalizedString GetSubmarineVoteResultMessage(SubmarineInfo info, VoteType type, int yesVoteCount, int noVoteCount, bool votePassed)
		{
			int price = info.GetPrice(null, null);
			LocalizedString result = string.Empty;
			switch (type)
			{
			case VoteType.PurchaseAndSwitchSub:
				result = TextManager.GetWithVariables(votePassed ? "submarinepurchaseandswitchvotepassed" : "submarinepurchaseandswitchvotefailed", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[submarinename]", info.DisplayName),
					new ValueTuple<string, LocalizedString>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", price)),
					new ValueTuple<string, LocalizedString>("[currencyname]", TextManager.Get("credit").ToLower()),
					new ValueTuple<string, LocalizedString>("[yesvotecount]", yesVoteCount.ToString()),
					new ValueTuple<string, LocalizedString>("[novotecount]", noVoteCount.ToString())
				});
				break;
			case VoteType.PurchaseSub:
				result = TextManager.GetWithVariables(votePassed ? "submarinepurchasevotepassed" : "submarinepurchasevotefailed", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[submarinename]", info.DisplayName),
					new ValueTuple<string, LocalizedString>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", price)),
					new ValueTuple<string, LocalizedString>("[currencyname]", TextManager.Get("credit").ToLower()),
					new ValueTuple<string, LocalizedString>("[yesvotecount]", yesVoteCount.ToString()),
					new ValueTuple<string, LocalizedString>("[novotecount]", noVoteCount.ToString())
				});
				break;
			case VoteType.SwitchSub:
				result = TextManager.GetWithVariables(votePassed ? "submarineswitchnofeevotepassed" : "submarineswitchnofeevotefailed", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[submarinename]", info.DisplayName),
					new ValueTuple<string, LocalizedString>("[yesvotecount]", yesVoteCount.ToString()),
					new ValueTuple<string, LocalizedString>("[novotecount]", noVoteCount.ToString())
				});
				break;
			}
			return result;
		}

		// Token: 0x060017AD RID: 6061 RVA: 0x000EA32C File Offset: 0x000E852C
		private void SetMoneyTransferVotingText(Client starter, Client from, Client to, int amount)
		{
			string name = starter.Name;
			JobPrefab jobPrefab;
			if (starter == null)
			{
				jobPrefab = null;
			}
			else
			{
				Character character = starter.Character;
				if (character == null)
				{
					jobPrefab = null;
				}
				else
				{
					CharacterInfo info = character.Info;
					if (info == null)
					{
						jobPrefab = null;
					}
					else
					{
						Job job = info.Job;
						jobPrefab = ((job != null) ? job.Prefab : null);
					}
				}
			}
			JobPrefab prefab = jobPrefab;
			Color nameColor = (prefab != null) ? prefab.UIColor : Color.White;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 4);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.R);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.G);
			defaultInterpolatedStringHandler.AppendLiteral(",");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(nameColor.B);
			defaultInterpolatedStringHandler.AppendLiteral("‖");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
			string characterRichString = defaultInterpolatedStringHandler.ToStringAndClear();
			LocalizedString text = string.Empty;
			if (from == null && to != null)
			{
				text = TextManager.GetWithVariables("crewwallet.requestbanktoselfvote", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[requester]", characterRichString),
					new ValueTuple<string, string>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount))
				});
			}
			else if (from != null && to == null)
			{
				text = TextManager.GetWithVariables("crewwallet.requestselftobankvote", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[requester]", characterRichString),
					new ValueTuple<string, string>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount))
				});
			}
			else
			{
				LocalizedString bankName = TextManager.Get("crewwallet.bank");
				text = TextManager.GetWithVariables("crewwallet.requesttransfervote", new ValueTuple<string, LocalizedString>[]
				{
					new ValueTuple<string, LocalizedString>("[requester]", characterRichString),
					new ValueTuple<string, LocalizedString>("[player1]", (((from != null) ? from.Character : null) == null) ? bankName : from.Character.Name),
					new ValueTuple<string, LocalizedString>("[player2]", (((to != null) ? to.Character : null) == null) ? bankName : to.Character.Name),
					new ValueTuple<string, LocalizedString>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount))
				});
			}
			this.votingOnText = RichString.Rich(text, null);
		}

		// Token: 0x060017AE RID: 6062 RVA: 0x000EA590 File Offset: 0x000E8790
		private void SendMoneyTransferVoteEndMessage(Client from, Client to, int amount)
		{
			GameMain.NetworkMember.AddChatMessage(VotingInterface.GetMoneyTransferVoteResultMessage(from, to, amount, this.yesVotes, this.noVotes, this.votePassed).Value, ChatMessageType.Server, "", null, null, PlayerConnectionChangeType.None, null);
		}

		// Token: 0x060017AF RID: 6063 RVA: 0x000EA5D8 File Offset: 0x000E87D8
		public static LocalizedString GetMoneyTransferVoteResultMessage(Client from, Client to, int transferAmount, int yesVoteCount, int noVoteCount, bool votePassed)
		{
			LocalizedString result = string.Empty;
			if (from == null && to != null)
			{
				result = TextManager.GetWithVariables(votePassed ? "crewwallet.banktoplayer.votepassed" : "crewwallet.banktoplayer.votefailed", new ValueTuple<string, string>[]
				{
					new ValueTuple<string, string>("[playername]", to.Name),
					new ValueTuple<string, string>("[amount]", string.Format(CultureInfo.InvariantCulture, "{0:N0}", transferAmount)),
					new ValueTuple<string, string>("[yesvotecount]", yesVoteCount.ToString()),
					new ValueTuple<string, string>("[novotecount]", noVoteCount.ToString())
				});
			}
			return result;
		}

		// Token: 0x060017B0 RID: 6064 RVA: 0x000EA686 File Offset: 0x000E8886
		public void Remove()
		{
			if (this.frame != null)
			{
				this.frame.Parent.RemoveChild(this.frame);
				this.frame = null;
			}
		}

		// Token: 0x04000C1F RID: 3103
		public bool VoteRunning;

		// Token: 0x04000C20 RID: 3104
		private GUIFrame frame;

		// Token: 0x04000C21 RID: 3105
		private GUITextBlock votingTextBlock;

		// Token: 0x04000C22 RID: 3106
		private GUITextBlock votedTextBlock;

		// Token: 0x04000C23 RID: 3107
		private GUITextBlock voteCounter;

		// Token: 0x04000C24 RID: 3108
		private GUIProgressBar votingTimer;

		// Token: 0x04000C25 RID: 3109
		private GUIButton yesVoteButton;

		// Token: 0x04000C26 RID: 3110
		private GUIButton noVoteButton;

		// Token: 0x04000C27 RID: 3111
		private Action onVoteEnd;

		// Token: 0x04000C28 RID: 3112
		private int yesVotes;

		// Token: 0x04000C29 RID: 3113
		private int noVotes;

		// Token: 0x04000C2A RID: 3114
		private int maxVotes;

		// Token: 0x04000C2B RID: 3115
		private Func<int> getYesVotes;

		// Token: 0x04000C2C RID: 3116
		private Func<int> getNoVotes;

		// Token: 0x04000C2D RID: 3117
		private Func<int> getMaxVotes;

		// Token: 0x04000C2E RID: 3118
		private bool votePassed;

		// Token: 0x04000C2F RID: 3119
		private RichString votingOnText;

		// Token: 0x04000C30 RID: 3120
		private float votingTime = 100f;

		// Token: 0x04000C31 RID: 3121
		private float timer;

		// Token: 0x04000C32 RID: 3122
		private VoteType currentVoteType;

		// Token: 0x04000C33 RID: 3123
		private Point createdForResolution;
	}
}
