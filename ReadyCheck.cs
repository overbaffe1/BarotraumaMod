using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000072 RID: 114
	[NullableContext(1)]
	[Nullable(0)]
	internal class ReadyCheck
	{
		// Token: 0x0600108D RID: 4237 RVA: 0x0009D87D File Offset: 0x0009BA7D
		private static LocalizedString ReadyCheckBody(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				return TextManager.GetWithVariable("readycheck.body", "[player]", name, FormatCapitals.No);
			}
			return TextManager.Get("readycheck.serverbody");
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x0009D8A8 File Offset: 0x0009BAA8
		private static LocalizedString ReadyCheckStatus(int ready, int total)
		{
			return TextManager.GetWithVariables("readycheck.readycount", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[ready]", ready.ToString()),
				new ValueTuple<string, string>("[total]", total.ToString())
			});
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x0009D8F5 File Offset: 0x0009BAF5
		private static LocalizedString ReadyCheckPleaseWait(int seconds)
		{
			return TextManager.GetWithVariable("readycheck.pleasewait", "[seconds]", seconds.ToString(), FormatCapitals.No);
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x0009D913 File Offset: 0x0009BB13
		[NullableContext(2)]
		public static bool IsReadyCheck(GUIComponent msgBox)
		{
			return ((msgBox != null) ? msgBox.UserData : null) as string == "ReadyCheck" || ((msgBox != null) ? msgBox.UserData : null) as string == "ReadyCheckResults";
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x0009D950 File Offset: 0x0009BB50
		private void CreateMessageBox(string author)
		{
			Vector2 relativeSize = new Vector2(0.2f / GUI.AspectRatioAdjustment, 0.15f);
			Point minSize = new Point(300, 200);
			this.msgBox = new GUIMessageBox(ReadyCheck.readyCheckHeader, ReadyCheck.ReadyCheckBody(author), new LocalizedString[]
			{
				ReadyCheck.yesButton,
				ReadyCheck.noButton
			}, new Vector2?(relativeSize), new Point?(minSize), Alignment.TopLeft, GUIMessageBox.Type.Vote, "", null, "", null, null, false)
			{
				UserData = "ReadyCheck",
				Draggable = true
			};
			GUILayoutGroup contentLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.125f), this.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			new GUIProgressBar(new RectTransform(new Vector2(0.8f, 1f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Orange), "", true).UserData = "Timer";
			this.msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				this.msgBox.Close();
				if (GameMain.Client == null)
				{
					return true;
				}
				ReadyCheck.SendState(ReadyStatus.Yes);
				this.CreateResultsMessage();
				return true;
			};
			this.msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				this.msgBox.Close();
				if (GameMain.Client == null)
				{
					return true;
				}
				ReadyCheck.SendState(ReadyStatus.No);
				this.CreateResultsMessage();
				return true;
			};
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x0009DADC File Offset: 0x0009BCDC
		private void CreateResultsMessage()
		{
			if (GameMain.Client == null)
			{
				return;
			}
			Vector2 relativeSize = new Vector2(0.2f, 0.3f);
			Point minSize = new Point(300, 400);
			this.resultsBox = new GUIMessageBox(ReadyCheck.readyCheckHeader, string.Empty, new LocalizedString[]
			{
				ReadyCheck.closeButton
			}, new Vector2?(relativeSize), new Point?(minSize), Alignment.TopLeft, GUIMessageBox.Type.Vote, "", null, "", null, null, false)
			{
				UserData = "ReadyCheckResults",
				Draggable = true
			};
			if (this.msgBox != null)
			{
				this.resultsBox.RectTransform.ScreenSpaceOffset = this.msgBox.RectTransform.ScreenSpaceOffset;
			}
			GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), this.resultsBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				UserData = "ReadyUserList"
			};
			foreach (KeyValuePair<byte, ReadyStatus> keyValuePair in this.Clients)
			{
				byte id2;
				ReadyStatus readyStatus;
				keyValuePair.Deconstruct(out id2, out readyStatus);
				byte id = id2;
				ReadyStatus status = readyStatus;
				Client client = GameMain.Client.ConnectedClients.FirstOrDefault((Client c) => c.SessionId == id);
				if (client == null)
				{
					string list = GameMain.Client.ConnectedClients.Aggregate("Available clients:\n", delegate(string current, Client c)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 2);
						defaultInterpolatedStringHandler3.AppendFormatted<byte>(c.SessionId);
						defaultInterpolatedStringHandler3.AppendLiteral(": ");
						defaultInterpolatedStringHandler3.AppendFormatted(c.Name);
						defaultInterpolatedStringHandler3.AppendLiteral("\n");
						return current + defaultInterpolatedStringHandler3.ToStringAndClear();
					});
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Client ID ");
					defaultInterpolatedStringHandler.AppendFormatted<byte>(id);
					defaultInterpolatedStringHandler.AppendLiteral(" was reported in ready check but was not found.\n");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + list.TrimEnd('\n'), null);
				}
				else
				{
					GUIFrame container = new GUIFrame(new RectTransform(new Vector2(1f, 0.15f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
					{
						UserData = id
					};
					GUILayoutGroup frame = new GUILayoutGroup(new RectTransform(Vector2.One, container.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
					{
						Stretch = true
					};
					int height = frame.Rect.Height;
					JobPrefab jobPrefab2;
					if (client == null)
					{
						jobPrefab2 = null;
					}
					else
					{
						Character character = client.Character;
						if (character == null)
						{
							jobPrefab2 = null;
						}
						else
						{
							CharacterInfo info = character.Info;
							if (info == null)
							{
								jobPrefab2 = null;
							}
							else
							{
								Job job = info.Job;
								jobPrefab2 = ((job != null) ? job.Prefab : null);
							}
						}
					}
					JobPrefab jobPrefab = jobPrefab2;
					if (((jobPrefab != null) ? jobPrefab.Icon : null) != null)
					{
						new GUIImage(new RectTransform(new Point(height, height), frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), jobPrefab.Icon, true, null).Color = jobPrefab.UIColor;
					}
					RectTransform rectT = new RectTransform(new Vector2(0.75f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					string str;
					if ((str = ((client != null) ? client.Name : null)) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Unknown ID ");
						defaultInterpolatedStringHandler2.AppendFormatted<byte>(id);
						str = defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					new GUITextBlock(rectT, str, new Color?((jobPrefab != null) ? jobPrefab.UIColor : Color.White), null, Alignment.Center, false, "", null).AutoScaleHorizontal = true;
					GUIImage statusIcon = new GUIImage(new RectTransform(new Point(height, height), frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, true)
					{
						UserData = "ReadySprite"
					};
					ReadyCheck.UpdateStatusIcon(statusIcon, status);
				}
			}
			this.resultsBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				this.resultsBox.Close();
				return true;
			};
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x0009DF68 File Offset: 0x0009C168
		private void UpdateBar()
		{
			double elapsedTime = (DateTime.Now - this.startTime).TotalSeconds;
			GUIMessageBox guimessageBox = this.msgBox;
			if (guimessageBox != null && !guimessageBox.Closed && GUIMessageBox.MessageBoxes.Contains(this.msgBox))
			{
				GUIProgressBar bar = this.msgBox.FindChild("Timer", true) as GUIProgressBar;
				if (bar != null)
				{
					bar.BarSize = (float)(elapsedTime / (this.endTime - this.startTime).TotalSeconds);
				}
			}
			int second = (int)Math.Ceiling(elapsedTime);
			if (second > this.lastSecond)
			{
				guimessageBox = this.msgBox;
				if (guimessageBox != null && !guimessageBox.Closed)
				{
					SoundPlayer.PlayUISound(GUISoundType.PopupMenu);
				}
				this.lastSecond = second;
			}
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0009E024 File Offset: 0x0009C224
		private static void CloseLingeringPopups()
		{
			foreach (GUIComponent box in GUIMessageBox.MessageBoxes.ToImmutableArray<GUIComponent>())
			{
				GUIMessageBox msgBox = box as GUIMessageBox;
				if (msgBox != null)
				{
					object userData = msgBox.UserData;
					string text = userData as string;
					bool flag = text != null && (text == "ReadyCheck" || text == "ReadyCheckResults");
					if (flag)
					{
						msgBox.Close();
					}
				}
			}
		}

		// Token: 0x06001095 RID: 4245 RVA: 0x0009E0A4 File Offset: 0x0009C2A4
		public static void ClientRead(IReadMessage inc)
		{
			ReadyCheckState state = (ReadyCheckState)inc.ReadByte();
			GameSession gameSession = GameMain.GameSession;
			CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
			IReadOnlyList<Client> otherClients = GameMain.Client.ConnectedClients;
			if (crewManager == null || otherClients == null)
			{
				if (state == ReadyCheckState.Start)
				{
					ReadyCheck.SendState(ReadyStatus.No);
				}
				return;
			}
			switch (state)
			{
			case ReadyCheckState.Start:
			{
				ReadyCheck.CloseLingeringPopups();
				bool isOwn = false;
				byte authorId = 0;
				long startTime = inc.ReadInt64();
				long endTime = inc.ReadInt64();
				string author = inc.ReadString();
				bool hasAuthor = inc.ReadBoolean();
				if (hasAuthor)
				{
					authorId = inc.ReadByte();
					isOwn = (authorId == GameMain.Client.SessionId);
				}
				ushort clientCount = inc.ReadUInt16();
				List<byte> clients = new List<byte>();
				for (int i = 0; i < (int)clientCount; i++)
				{
					clients.Add(inc.ReadByte());
				}
				ReadyCheck rCheck = new ReadyCheck(clients, DateTimeOffset.FromUnixTimeSeconds(startTime).LocalDateTime, DateTimeOffset.FromUnixTimeSeconds(endTime).LocalDateTime);
				crewManager.ActiveReadyCheck = rCheck;
				if (isOwn)
				{
					ReadyCheck.SendState(ReadyStatus.Yes);
					rCheck.CreateResultsMessage();
				}
				else
				{
					rCheck.CreateMessageBox(author);
				}
				if (hasAuthor && rCheck.Clients.ContainsKey(authorId))
				{
					rCheck.Clients[authorId] = ReadyStatus.Yes;
					return;
				}
				break;
			}
			case ReadyCheckState.Update:
			{
				ReadyStatus newState = (ReadyStatus)inc.ReadByte();
				byte targetId = inc.ReadByte();
				ReadyCheck activeReadyCheck = crewManager.ActiveReadyCheck;
				if (activeReadyCheck == null)
				{
					return;
				}
				activeReadyCheck.UpdateState(targetId, newState);
				return;
			}
			case ReadyCheckState.End:
			{
				ushort count = inc.ReadUInt16();
				for (int j = 0; j < (int)count; j++)
				{
					byte id = inc.ReadByte();
					ReadyStatus status = (ReadyStatus)inc.ReadByte();
					ReadyCheck activeReadyCheck2 = crewManager.ActiveReadyCheck;
					if (activeReadyCheck2 != null)
					{
						activeReadyCheck2.UpdateState(id, status);
					}
				}
				ReadyCheck activeReadyCheck3 = crewManager.ActiveReadyCheck;
				if (activeReadyCheck3 != null)
				{
					activeReadyCheck3.EndReadyCheck();
				}
				ReadyCheck activeReadyCheck4 = crewManager.ActiveReadyCheck;
				if (activeReadyCheck4 != null)
				{
					GUIMessageBox guimessageBox = activeReadyCheck4.msgBox;
					if (guimessageBox != null)
					{
						guimessageBox.Close();
					}
				}
				crewManager.ActiveReadyCheck = null;
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x06001096 RID: 4246 RVA: 0x0009E278 File Offset: 0x0009C478
		private void UpdateState(byte id, ReadyStatus status)
		{
			if (this.Clients.ContainsKey(id))
			{
				this.Clients[id] = status;
			}
			if (this.resultsBox == null || this.resultsBox.Closed || !GUIMessageBox.MessageBoxes.Contains(this.resultsBox))
			{
				return;
			}
			GUIListBox userList = this.resultsBox.Content.FindChild("ReadyUserList", false) as GUIListBox;
			if (userList == null)
			{
				return;
			}
			GUIComponent child = userList.Content.FindChild(id, false);
			GUIImage image = ((child != null) ? child.GetChild<GUILayoutGroup>().FindChild("ReadySprite", false) : null) as GUIImage;
			if (image == null)
			{
				return;
			}
			ReadyCheck.UpdateStatusIcon(image, status);
		}

		// Token: 0x06001097 RID: 4247 RVA: 0x0009E324 File Offset: 0x0009C524
		private static void UpdateStatusIcon(GUIImage image, ReadyStatus status)
		{
			string style;
			if (status != ReadyStatus.Yes)
			{
				if (status != ReadyStatus.No)
				{
					return;
				}
				style = "MissionFailedIcon";
			}
			else
			{
				style = "MissionCompletedIcon";
			}
			image.ApplyStyle(GUIStyle.GetComponentStyle(style));
		}

		// Token: 0x06001098 RID: 4248 RVA: 0x0009E354 File Offset: 0x0009C554
		private static void SendState(ReadyStatus status)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(22);
			msg.WriteByte(1);
			msg.WriteByte((byte)status);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x0009E39C File Offset: 0x0009C59C
		public static void CreateReadyCheck()
		{
			if (!(ReadyCheck.ReadyCheckCooldown < DateTime.Now))
			{
				GUIMessageBox msgBox = new GUIMessageBox(ReadyCheck.readyCheckHeader, ReadyCheck.ReadyCheckPleaseWait((ReadyCheck.ReadyCheckCooldown - DateTime.Now).Seconds), new LocalizedString[]
				{
					ReadyCheck.closeButton
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
				{
					msgBox.Close();
					return true;
				};
				return;
			}
			ReadyCheck.ReadyCheckCooldown = DateTime.Now.AddMinutes(1.0);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteByte(22);
			msg.WriteByte(0);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x0600109A RID: 4250 RVA: 0x0009E498 File Offset: 0x0009C698
		public ReadyCheck(List<byte> clients, DateTime startTime, DateTime endTime) : this(clients)
		{
			this.startTime = startTime;
			this.endTime = endTime;
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x0009E4AF File Offset: 0x0009C6AF
		public ReadyCheck(List<byte> clients, float duration) : this(clients)
		{
			this.startTime = DateTime.Now;
			this.endTime = this.startTime + new TimeSpan(0, 0, 0, 0, (int)(duration * 1000f));
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x0009E4E8 File Offset: 0x0009C6E8
		private ReadyCheck(List<byte> clients)
		{
			this.Clients = new Dictionary<byte, ReadyStatus>();
			foreach (byte client in clients)
			{
				if (!this.Clients.ContainsKey(client))
				{
					this.Clients.Add(client, ReadyStatus.Unanswered);
				}
			}
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x0009E564 File Offset: 0x0009C764
		private void EndReadyCheck()
		{
			if (this.IsFinished)
			{
				return;
			}
			this.IsFinished = true;
			int readyCount = this.Clients.Count((KeyValuePair<byte, ReadyStatus> pair) => pair.Value == ReadyStatus.Yes);
			int totalCount = this.Clients.Count;
			GameMain.Client.AddChatMessage(ChatMessage.Create(string.Empty, ReadyCheck.ReadyCheckStatus(readyCount, totalCount).Value, ChatMessageType.Server, null, null, PlayerConnectionChangeType.None, null));
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x0009E5E5 File Offset: 0x0009C7E5
		public void Update(float deltaTime)
		{
			if (DateTime.Now < this.endTime)
			{
				this.UpdateBar();
				return;
			}
			this.EndReadyCheck();
			GUIMessageBox guimessageBox = this.msgBox;
			if (guimessageBox == null)
			{
				return;
			}
			guimessageBox.Close();
		}

		// Token: 0x0400081D RID: 2077
		private static readonly LocalizedString readyCheckHeader = TextManager.Get("ReadyCheck.Title");

		// Token: 0x0400081E RID: 2078
		private static readonly LocalizedString noButton = TextManager.Get("No");

		// Token: 0x0400081F RID: 2079
		private static readonly LocalizedString yesButton = TextManager.Get("Yes");

		// Token: 0x04000820 RID: 2080
		private static readonly LocalizedString closeButton = TextManager.Get("Close");

		// Token: 0x04000821 RID: 2081
		private const string TimerData = "Timer";

		// Token: 0x04000822 RID: 2082
		private const string PromptData = "ReadyCheck";

		// Token: 0x04000823 RID: 2083
		private const string ResultData = "ReadyCheckResults";

		// Token: 0x04000824 RID: 2084
		private const string UserListData = "ReadyUserList";

		// Token: 0x04000825 RID: 2085
		private const string ReadySpriteData = "ReadySprite";

		// Token: 0x04000826 RID: 2086
		private int lastSecond = 1;

		// Token: 0x04000827 RID: 2087
		[Nullable(2)]
		private GUIMessageBox msgBox;

		// Token: 0x04000828 RID: 2088
		[Nullable(2)]
		private GUIMessageBox resultsBox;

		// Token: 0x04000829 RID: 2089
		public static DateTime ReadyCheckCooldown = DateTime.MinValue;

		// Token: 0x0400082A RID: 2090
		private readonly DateTime endTime;

		// Token: 0x0400082B RID: 2091
		private readonly DateTime startTime;

		// Token: 0x0400082C RID: 2092
		public readonly Dictionary<byte, ReadyStatus> Clients;

		// Token: 0x0400082D RID: 2093
		public bool IsFinished;
	}
}
