using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000018 RID: 24
	internal class ConversationAction : EventAction
	{
		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600036F RID: 879 RVA: 0x0001E343 File Offset: 0x0001C543
		// (set) Token: 0x06000370 RID: 880 RVA: 0x0001E34B File Offset: 0x0001C54B
		public int SelectedOption
		{
			get
			{
				return this.selectedOption;
			}
			set
			{
				this.selectedOption = value;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000371 RID: 881 RVA: 0x0001E354 File Offset: 0x0001C554
		public IEnumerable<Client> TargetClients
		{
			get
			{
				this.UpdateIgnoredClients();
				return from c in this.targetClients
				where !this.ignoredClients.ContainsKey(c)
				select c;
			}
		}

		// Token: 0x06000372 RID: 882 RVA: 0x0001E374 File Offset: 0x0001C574
		private void UpdateIgnoredClients()
		{
			if (this.ignoredClients.Any<KeyValuePair<Client, DateTime>>())
			{
				HashSet<Client> clientsToRemove = null;
				foreach (Client i in this.ignoredClients.Keys)
				{
					if (this.ignoredClients[i] < DateTime.Now)
					{
						if (clientsToRemove == null)
						{
							clientsToRemove = new HashSet<Client>();
						}
						clientsToRemove.Add(i);
					}
				}
				if (clientsToRemove != null)
				{
					foreach (Client j in clientsToRemove)
					{
						this.ignoredClients.Remove(j);
					}
				}
			}
		}

		// Token: 0x06000373 RID: 883 RVA: 0x0001E448 File Offset: 0x0001C648
		public bool CanClientStartConversation(Client client)
		{
			if (!this.TargetTag.IsEmpty)
			{
				IEnumerable<Entity> targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
				where this.IsValidTarget(e, true)
				select e;
				return targets.Contains(client.Character);
			}
			return true;
		}

		// Token: 0x06000374 RID: 884 RVA: 0x0001E498 File Offset: 0x0001C698
		public void IgnoreClient(Client c, float seconds)
		{
			if (!this.ignoredClients.ContainsKey(c))
			{
				this.ignoredClients.Add(c, DateTime.Now);
			}
			this.ignoredClients[c] = DateTime.Now + TimeSpan.FromSeconds((double)seconds);
			ConversationAction lastActive;
			if (ConversationAction.lastActiveAction.TryGetValue(c, out lastActive) && lastActive == this)
			{
				ConversationAction.lastActiveAction.Remove(c);
			}
			this.Reset();
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0001E508 File Offset: 0x0001C708
		private bool IsBlockedByAnotherConversation(IEnumerable<Entity> targets, float duration)
		{
			if (targets == null || targets.None(null))
			{
				using (IEnumerator<Client> enumerator = GameMain.Server.ConnectedClients.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Client client = enumerator.Current;
						if (this.IsBlockedByAnotherConversation(client, duration))
						{
							return true;
						}
					}
					return false;
				}
			}
			foreach (Entity e in targets)
			{
				Character character = e as Character;
				if (character != null && character.IsRemotePlayer)
				{
					Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
					if (targetClient != null && this.IsBlockedByAnotherConversation(targetClient, duration))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0001E604 File Offset: 0x0001C804
		private bool IsBlockedByAnotherConversation(Client targetClient, float duration)
		{
			return ConversationAction.lastActiveAction.ContainsKey(targetClient) && !ConversationAction.lastActiveAction[targetClient].ParentEvent.IsFinished && ConversationAction.lastActiveAction[targetClient].ParentEvent != this.ParentEvent && Timing.TotalTime < ConversationAction.lastActiveAction[targetClient].lastActiveTime + (double)duration;
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0001E66A File Offset: 0x0001C86A
		private bool CanClientReceive(Client c)
		{
			return c != null && c.InGame && c.Character != null && !this.ignoredClients.ContainsKey(c);
		}

		// Token: 0x06000378 RID: 888 RVA: 0x0001E690 File Offset: 0x0001C890
		public void ServerWrite(Character speaker, Client client, bool interrupt)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(0);
			outmsg.WriteUInt16(this.Identifier);
			outmsg.WriteString(this.EventSprite);
			outmsg.WriteByte((byte)this.DialogType);
			outmsg.WriteBoolean(this.ContinueConversation);
			if (interrupt)
			{
				outmsg.WriteUInt16((speaker != null) ? speaker.ID : 0);
				outmsg.WriteString(string.Empty);
				outmsg.WriteBoolean(false);
				outmsg.WriteByte(0);
				outmsg.WriteByte(0);
			}
			else
			{
				outmsg.WriteUInt16((speaker != null) ? speaker.ID : 0);
				IWriteMessage writeMessage = outmsg;
				LocalizedString displayText = this.GetDisplayText();
				writeMessage.WriteString(((displayText != null) ? displayText.Value : null) ?? string.Empty);
				outmsg.WriteBoolean(this.FadeToBlack);
				outmsg.WriteByte((byte)this.Options.Count);
				for (int i = 0; i < this.Options.Count; i++)
				{
					outmsg.WriteString(this.Options[i].Text);
				}
				int[] endings = this.GetEndingOptions();
				outmsg.WriteByte((byte)endings.Length);
				foreach (int end in endings)
				{
					outmsg.WriteByte((byte)end);
				}
			}
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			ServerPeer serverPeer = server.ServerPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06000379 RID: 889 RVA: 0x0001E7F4 File Offset: 0x0001C9F4
		public void ServerWriteSelectedOption(Client client)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(1);
			outmsg.WriteUInt16(this.Identifier);
			outmsg.WriteByte((byte)(this.selectedOption + 1));
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			ServerPeer serverPeer = server.ServerPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600037A RID: 890 RVA: 0x0001E853 File Offset: 0x0001CA53
		// (set) Token: 0x0600037B RID: 891 RVA: 0x0001E85B File Offset: 0x0001CA5B
		[Serialize("", IsPropertySaveable.Yes, "The text to display in the prompt. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Text { get; set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x0600037C RID: 892 RVA: 0x0001E864 File Offset: 0x0001CA64
		// (set) Token: 0x0600037D RID: 893 RVA: 0x0001E86C File Offset: 0x0001CA6C
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the speaker will send the Text in chat, or if ForceSayText is not empty, will send that instead. Note: requires a valid SpeakerTag to be defined.", "", false)]
		public bool ForceSay { get; set; }

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0001E875 File Offset: 0x0001CA75
		// (set) Token: 0x0600037F RID: 895 RVA: 0x0001E87D File Offset: 0x0001CA7D
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the message sent in chat by the speaker will be sent in radio chat instead.", "", false)]
		public bool ForceSayInRadio { get; set; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0001E886 File Offset: 0x0001CA86
		// (set) Token: 0x06000381 RID: 897 RVA: 0x0001E88E File Offset: 0x0001CA8E
		[Serialize("", IsPropertySaveable.Yes, "Message sent in chat by the speaker, if empty, Text is used instead.", "", false)]
		public string ForceSayText { get; set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0001E897 File Offset: 0x0001CA97
		// (set) Token: 0x06000383 RID: 899 RVA: 0x0001E89F File Offset: 0x0001CA9F
		[Serialize(true, IsPropertySaveable.Yes, "Should the chat message be stripped of any quotation mark characters?", "", false)]
		public bool ForceSayRemoveQuotes { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0001E8A8 File Offset: 0x0001CAA8
		// (set) Token: 0x06000385 RID: 901 RVA: 0x0001E8B0 File Offset: 0x0001CAB0
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character who's speaking. Makes a speech bubble icon appear above the character to indicate you can speak with them, and stops the character in place when the conversation triggers. Also allows the conversation to be interrupted if the speaker dies or becomes incapacitated mid-conversation.", "", false)]
		public Identifier SpeakerTag { get; set; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000386 RID: 902 RVA: 0x0001E8B9 File Offset: 0x0001CAB9
		// (set) Token: 0x06000387 RID: 903 RVA: 0x0001E8C1 File Offset: 0x0001CAC1
		[Serialize("", IsPropertySaveable.Yes, "Tag of the player the conversation is shown to. If empty, the conversation is shown to everyone. If SpeakerTag is defined, the conversation is always only shown to the player who interacts with the speaker.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000388 RID: 904 RVA: 0x0001E8CA File Offset: 0x0001CACA
		// (set) Token: 0x06000389 RID: 905 RVA: 0x0001E8D2 File Offset: 0x0001CAD2
		[Serialize(true, IsPropertySaveable.Yes, "Should someone interact with the speaker for the conversation to trigger?", "", false)]
		public bool WaitForInteraction { get; set; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x0600038A RID: 906 RVA: 0x0001E8DB File Offset: 0x0001CADB
		// (set) Token: 0x0600038B RID: 907 RVA: 0x0001E8E3 File Offset: 0x0001CAE3
		[Serialize("", IsPropertySaveable.Yes, "Tag to assign to whoever invokes the conversation.", "", false)]
		public Identifier InvokerTag { get; set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600038C RID: 908 RVA: 0x0001E8EC File Offset: 0x0001CAEC
		// (set) Token: 0x0600038D RID: 909 RVA: 0x0001E8F4 File Offset: 0x0001CAF4
		[Serialize(false, IsPropertySaveable.Yes, "Should the screen fade to black when the conversation is active?", "", false)]
		public bool FadeToBlack { get; set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600038E RID: 910 RVA: 0x0001E8FD File Offset: 0x0001CAFD
		// (set) Token: 0x0600038F RID: 911 RVA: 0x0001E905 File Offset: 0x0001CB05
		[Serialize(true, IsPropertySaveable.Yes, "Should the event end if the conversations is interrupted (e.g. if the speaker dies or falls unconscious mid-conversation). Defaults to true.", "", false)]
		public bool EndEventIfInterrupted { get; set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000390 RID: 912 RVA: 0x0001E90E File Offset: 0x0001CB0E
		// (set) Token: 0x06000391 RID: 913 RVA: 0x0001E916 File Offset: 0x0001CB16
		[Serialize("", IsPropertySaveable.Yes, "Identifier of an event sprite to display in the corner of the conversation prompt.", "", false)]
		public string EventSprite { get; set; }

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000392 RID: 914 RVA: 0x0001E91F File Offset: 0x0001CB1F
		// (set) Token: 0x06000393 RID: 915 RVA: 0x0001E927 File Offset: 0x0001CB27
		[Serialize(ConversationAction.DialogTypes.Regular, IsPropertySaveable.Yes, "Type of the dialog prompt.", "", false)]
		public ConversationAction.DialogTypes DialogType { get; set; }

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000394 RID: 916 RVA: 0x0001E930 File Offset: 0x0001CB30
		// (set) Token: 0x06000395 RID: 917 RVA: 0x0001E938 File Offset: 0x0001CB38
		[Serialize(false, IsPropertySaveable.Yes, "Does this conversation continue after this ConversationAction? If you have multiple successive ConversationActions, perhaps with some actions happening in between, you can enable this to prevent the dialog prompt from closing between the actions. Not necessary if the ConversationActions are nested inside each other: those are always considered parts of the same conversation, and shown in the same prompt.", "", false)]
		public bool ContinueConversation { get; set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000396 RID: 918 RVA: 0x0001E941 File Offset: 0x0001CB41
		// (set) Token: 0x06000397 RID: 919 RVA: 0x0001E949 File Offset: 0x0001CB49
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the event will not stop to wait for the conversation to be dismissed.", "", false)]
		public bool ContinueAutomatically { get; set; }

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000398 RID: 920 RVA: 0x0001E952 File Offset: 0x0001CB52
		// (set) Token: 0x06000399 RID: 921 RVA: 0x0001E95A File Offset: 0x0001CB5A
		[Serialize(false, IsPropertySaveable.Yes, "If SpeakerTag is defined, the conversation is interrupted by default if the speaker and the target end up too far from each other. This can be used to disable that behavior, keeping the dialog prompt open regardless of the distance.", "", false)]
		public bool IgnoreInterruptDistance { get; set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600039A RID: 922 RVA: 0x0001E963 File Offset: 0x0001CB63
		// (set) Token: 0x0600039B RID: 923 RVA: 0x0001E96B File Offset: 0x0001CB6B
		public Character Speaker { get; private set; }

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600039C RID: 924 RVA: 0x0001E974 File Offset: 0x0001CB74
		// (set) Token: 0x0600039D RID: 925 RVA: 0x0001E97C File Offset: 0x0001CB7C
		public List<ConversationAction.OptionActionGroup> Options { get; private set; }

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600039E RID: 926 RVA: 0x0001E985 File Offset: 0x0001CB85
		// (set) Token: 0x0600039F RID: 927 RVA: 0x0001E98D File Offset: 0x0001CB8D
		public EventAction.SubactionGroup Interrupted { get; private set; }

		// Token: 0x060003A0 RID: 928 RVA: 0x0001E998 File Offset: 0x0001CB98
		public ConversationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			ConversationAction.actionCount += 1;
			this.Identifier = ConversationAction.actionCount;
			this.Options = new List<ConversationAction.OptionActionGroup>();
			foreach (ContentXElement elem in element.Elements())
			{
				if (elem.Name.LocalName.Equals("option", StringComparison.OrdinalIgnoreCase))
				{
					this.Options.Add(new ConversationAction.OptionActionGroup(this.ParentEvent, elem));
				}
				else if (elem.Name.LocalName.Equals("interrupt", StringComparison.OrdinalIgnoreCase))
				{
					this.Interrupted = new EventAction.SubactionGroup(this.ParentEvent, elem);
				}
				else if (elem.Name.LocalName.Equals("text", StringComparison.OrdinalIgnoreCase))
				{
					this.Text = elem.GetAttributeString("tag", string.Empty);
					this.textElement = elem;
				}
				else
				{
					string thisName = "ConversationAction";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(148, 5);
					defaultInterpolatedStringHandler.AppendLiteral("Error in ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(" in the event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					defaultInterpolatedStringHandler.AppendLiteral(" - unrecognized child element \"");
					defaultInterpolatedStringHandler.AppendFormatted<XName>(elem.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\". If it's an action intended to execute after the ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendLiteral("it should be after the ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(", not inside it.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
			}
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x0001EB98 File Offset: 0x0001CD98
		public LocalizedString GetDisplayText()
		{
			LocalizedString text = string.Empty;
			if (this.textElement != null)
			{
				TextManager.ConstructDescription(ref text, this.textElement, new Func<string, string>(this.ParentEvent.GetTextForReplacementElement));
			}
			else
			{
				text = TextManager.Get(this.Text).Fallback(this.Text, true);
				if (text.Value.IsNullOrEmpty())
				{
					text = text.Fallback(this.Text, true);
				}
			}
			return this.ParentEvent.ReplaceVariablesInEventText(text);
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0001EC22 File Offset: 0x0001CE22
		public override IEnumerable<EventAction> GetSubActions()
		{
			return this.Options.SelectMany((ConversationAction.OptionActionGroup group) => group.Actions);
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x0001EC50 File Offset: 0x0001CE50
		public override bool IsFinished(ref string goTo)
		{
			if (this.interrupt)
			{
				if (this.dialogOpened)
				{
					foreach (Client c in GameMain.Server.ConnectedClients)
					{
						if (c.InGame && c.Character != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Conversation ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" finished, communicating to clients...");
							DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
							this.ServerWrite(this.Speaker, c, this.interrupt);
						}
					}
					this.ResetSpeaker();
					this.dialogOpened = false;
				}
				if (this.Interrupted == null)
				{
					if (this.EndEventIfInterrupted)
					{
						goTo = "_end";
					}
					return true;
				}
				return this.Interrupted.IsFinished(ref goTo);
			}
			else
			{
				if (this.ContinueAutomatically && this.Options.None(null))
				{
					return this.dialogOpened;
				}
				if (this.selectedOption >= 0 && (this.Options.None(null) || this.Options[this.selectedOption].IsFinished(ref goTo)))
				{
					this.ResetSpeaker();
					return true;
				}
				return false;
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0001EDA0 File Offset: 0x0001CFA0
		public override void Reset()
		{
			this.Options.ForEach(delegate(ConversationAction.OptionActionGroup a)
			{
				a.Reset();
			});
			this.ResetSpeaker();
			this.selectedOption = -1;
			this.interrupt = false;
			this.dialogOpened = false;
			this.Speaker = null;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0001EDF9 File Offset: 0x0001CFF9
		public void RetriggerAfter(float delay)
		{
			this.startDelay = delay;
			this.dialogOpened = false;
			this.selectedOption = -1;
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0001EE10 File Offset: 0x0001D010
		public override bool SetGoToTarget(string goTo)
		{
			this.selectedOption = -1;
			for (int i = 0; i < this.Options.Count; i++)
			{
				if (this.Options[i].SetGoToTarget(goTo))
				{
					this.selectedOption = i;
					this.interrupt = false;
					this.dialogOpened = true;
					return true;
				}
			}
			return false;
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0001EE68 File Offset: 0x0001D068
		private void ResetSpeaker()
		{
			if (this.Speaker == null)
			{
				return;
			}
			this.Speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
			this.Speaker.ActiveConversation = null;
			this.Speaker.SetCustomInteract(null, null);
			GameMain.NetworkMember.CreateEntityEvent(this.Speaker, default(Character.AssignCampaignInteractionEventData));
			HumanAIController humanAI = this.Speaker.AIController as HumanAIController;
			if (humanAI != null && !this.Speaker.IsDead && !this.Speaker.Removed)
			{
				humanAI.ClearForcedOrder();
				if (this.prevIdleObjective != null)
				{
					humanAI.ObjectiveManager.AddObjective(this.prevIdleObjective);
				}
				if (this.prevGotoObjective != null && !this.prevGotoObjective.Abandon)
				{
					humanAI.ObjectiveManager.AddObjective(this.prevGotoObjective);
				}
				humanAI.ObjectiveManager.SortObjectives();
			}
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0001EF40 File Offset: 0x0001D140
		public int[] GetEndingOptions()
		{
			List<int> endings = (from @group in this.Options.Where(delegate(ConversationAction.OptionActionGroup @group)
			{
				if (@group.EndConversation || !@group.Actions.Any<EventAction>())
				{
					return true;
				}
				if (@group.Actions.None((EventAction a) => a is ConversationAction))
				{
					return @group.Actions.Any(delegate(EventAction a)
					{
						GoTo goTo = a as GoTo;
						return goTo != null && goTo.EndConversation;
					});
				}
				return false;
			})
			select this.Options.IndexOf(@group)).ToList<int>();
			if (!this.ContinueConversation)
			{
				endings.Add(-1);
			}
			return endings.ToArray();
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x0001EFA4 File Offset: 0x0001D1A4
		public override void Update(float deltaTime)
		{
			this.startDelay -= deltaTime;
			if (this.startDelay > 0f)
			{
				return;
			}
			if (this.interrupt)
			{
				EventAction.SubactionGroup interrupted = this.Interrupted;
				if (interrupted == null)
				{
					return;
				}
				interrupted.Update(deltaTime);
				return;
			}
			else if (this.selectedOption < 0)
			{
				if (this.dialogOpened)
				{
					this.lastActiveTime = Timing.TotalTime;
					if (this.ShouldInterrupt(true))
					{
						this.ResetSpeaker();
						this.interrupt = true;
					}
					return;
				}
				if (this.SpeakerTag.IsEmpty)
				{
					this.TryStartConversation(this.Speaker, null);
					return;
				}
				if (this.npcWaitObjective != null)
				{
					this.npcWaitObjective.ForceHighestPriority = true;
				}
				if (this.Speaker != null && !this.Speaker.Removed && this.Speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk)
				{
					ConversationAction activeConversation = this.Speaker.ActiveConversation;
					if (((activeConversation != null) ? activeConversation.ParentEvent : null) != this.ParentEvent)
					{
						return;
					}
				}
				this.Speaker = (this.ParentEvent.GetTargets(this.SpeakerTag).FirstOrDefault((Entity e) => e is Character) as Character);
				if (this.Speaker == null || this.Speaker.Removed)
				{
					return;
				}
				if (this.Speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk)
				{
					ConversationAction activeConversation2 = this.Speaker.ActiveConversation;
					if (((activeConversation2 != null) ? activeConversation2.ParentEvent : null) != this.ParentEvent)
					{
						return;
					}
				}
				if (!this.WaitForInteraction)
				{
					this.TryStartConversation(this.Speaker, null);
					return;
				}
				if (this.Speaker.ActiveConversation != this)
				{
					this.Speaker.CampaignInteractionType = CampaignMode.InteractionType.Talk;
					this.Speaker.ActiveConversation = this;
					this.Speaker.SetCustomInteract(new Action<Character, Character>(this.TryStartConversation), TextManager.Get("CampaignInteraction.Talk"));
					GameMain.NetworkMember.CreateEntityEvent(this.Speaker, default(Character.AssignCampaignInteractionEventData));
				}
				return;
			}
			else
			{
				if (this.ShouldInterrupt(false))
				{
					this.ResetSpeaker();
					this.interrupt = true;
					return;
				}
				if (this.Options.Any<ConversationAction.OptionActionGroup>())
				{
					this.Options[this.selectedOption].Update(deltaTime);
				}
				return;
			}
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0001F1D0 File Offset: 0x0001D3D0
		private bool ShouldInterrupt(bool requireTarget)
		{
			IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
			if (!this.TargetTag.IsEmpty & requireTarget)
			{
				targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
				where this.IsValidTarget(e, requireTarget)
				select e;
				if (!targets.Any<Entity>())
				{
					return true;
				}
			}
			if (this.Speaker == null)
			{
				return false;
			}
			if ((!this.TargetTag.IsEmpty & requireTarget) && !this.IgnoreInterruptDistance && targets.All((Entity t) => Vector2.DistanceSquared(t.WorldPosition, this.Speaker.WorldPosition) > 90000f))
			{
				return true;
			}
			HumanAIController humanAI = this.Speaker.AIController as HumanAIController;
			return (humanAI != null && !humanAI.AllowCampaignInteraction()) || this.Speaker.Removed || this.Speaker.IsDead || this.Speaker.IsIncapacitated;
		}

		// Token: 0x060003AB RID: 939 RVA: 0x0001F2C8 File Offset: 0x0001D4C8
		private bool IsValidTarget(Entity e, bool requirePlayerControlled = true)
		{
			Character character = e as Character;
			bool isValid = character != null && !character.Removed && !character.IsDead && !character.IsIncapacitated && (character == Character.Controlled || character.IsRemotePlayer || !requirePlayerControlled);
			if (!this.dialogOpened)
			{
				this.UpdateIgnoredClients();
				isValid &= !this.ignoredClients.Keys.Any((Client c) => c.Character == e);
			}
			return isValid;
		}

		// Token: 0x060003AC RID: 940 RVA: 0x0001F358 File Offset: 0x0001D558
		private void TryStartConversation(Character speaker, Character targetCharacter = null)
		{
			IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
			if (!this.TargetTag.IsEmpty)
			{
				targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
				where this.IsValidTarget(e, true)
				select e;
				if (!targets.Any<Entity>() || this.IsBlockedByAnotherConversation(targets, 5f))
				{
					return;
				}
				if (targetCharacter != null && !targets.Contains(targetCharacter))
				{
					return;
				}
			}
			else
			{
				if (GameMain.NetworkMember != null)
				{
					this.UpdateIgnoredClients();
					if (GameMain.NetworkMember.ConnectedClients.None((Client c) => this.CanClientReceive(c)))
					{
						return;
					}
				}
				if (this.IsBlockedByAnotherConversation((targetCharacter != null) ? targetCharacter.ToEnumerable<Character>() : null, 5f))
				{
					return;
				}
			}
			Character speaker2 = speaker;
			HumanAIController humanAI = ((speaker2 != null) ? speaker2.AIController : null) as HumanAIController;
			if (humanAI != null)
			{
				this.prevIdleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
				this.prevGotoObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveGoTo>();
				this.npcWaitObjective = humanAI.SetForcedOrder(new Order(OrderPrefab.Prefabs["wait"], Barotrauma.Identifier.Empty, null, null));
				if (targets.Any<Entity>() || targetCharacter != null)
				{
					Entity closestTarget = targetCharacter;
					float closestDist = float.MaxValue;
					foreach (Entity entity in targets)
					{
						float dist = Vector2.DistanceSquared(entity.WorldPosition, speaker.WorldPosition);
						if (dist < closestDist)
						{
							closestTarget = entity;
							closestDist = dist;
						}
					}
					if (closestTarget != null)
					{
						humanAI.FaceTarget(closestTarget);
					}
				}
			}
			if (targetCharacter != null && !this.InvokerTag.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.InvokerTag, targetCharacter);
			}
			if (this.ForceSay)
			{
				Character speaker3 = speaker;
				if (speaker3 != null)
				{
					speaker3.ForceSay(this.ForceSayText.IsNullOrEmpty() ? TextManager.Get(this.Text).Fallback(this.Text, true) : TextManager.Get(this.ForceSayText).Fallback(this.ForceSayText, true), this.ForceSayInRadio, this.ForceSayRemoveQuotes, 0.7f);
				}
			}
			this.ShowDialog(this.Speaker, targetCharacter);
			this.dialogOpened = true;
			if (this.Speaker != null)
			{
				this.Speaker = speaker;
				this.Options.SelectMany((ConversationAction.OptionActionGroup op) => op.Actions).OfType<ConversationAction>().ForEach(delegate(ConversationAction action)
				{
					action.Speaker = speaker;
				});
				speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
				speaker.SetCustomInteract(null, null);
				GameMain.NetworkMember.CreateEntityEvent(speaker, default(Character.AssignCampaignInteractionEventData));
			}
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0001F640 File Offset: 0x0001D840
		private void ShowDialog(Character speaker, Character targetCharacter)
		{
			this.targetClients.Clear();
			if (!this.TargetTag.IsEmpty)
			{
				IEnumerable<Entity> entities = this.ParentEvent.GetTargets(this.TargetTag);
				using (IEnumerator<Entity> enumerator = entities.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Entity e = enumerator.Current;
						Character character = e as Character;
						if (character != null && character.IsRemotePlayer)
						{
							Client targetClient = GameMain.Server.ConnectedClients.Find((Client c) => c.Character == character);
							if (targetClient != null)
							{
								this.targetClients.Add(targetClient);
								ConversationAction.lastActiveAction[targetClient] = this;
								this.lastActiveTime = Timing.TotalTime;
								this.ServerWrite(speaker, targetClient, this.interrupt);
							}
						}
					}
					return;
				}
			}
			foreach (Client c2 in GameMain.Server.ConnectedClients)
			{
				if (this.CanClientReceive(c2) && (targetCharacter == null || targetCharacter == c2.Character))
				{
					this.targetClients.Add(c2);
					ConversationAction.lastActiveAction[c2] = this;
					this.lastActiveTime = Timing.TotalTime;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Sending conversationaction ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(" to client...");
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					this.ServerWrite(speaker, c2, this.interrupt);
				}
			}
		}

		// Token: 0x060003AE RID: 942 RVA: 0x0001F818 File Offset: 0x0001DA18
		public override string ToDebugString()
		{
			if (!this.interrupt)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
				defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.selectedOption > -1, this.selectedOption < 0 && this.dialogOpened));
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted("ConversationAction");
				defaultInterpolatedStringHandler.AppendLiteral(" -> (Selected option: ");
				defaultInterpolatedStringHandler.AppendFormatted(this.selectedOption.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return ToolBox.GetDebugSymbol(true, this.selectedOption < 0 && this.dialogOpened) + " ConversationAction -> (Interrupted)";
		}

		// Token: 0x04000194 RID: 404
		private static readonly Dictionary<Client, ConversationAction> lastActiveAction = new Dictionary<Client, ConversationAction>();

		// Token: 0x04000195 RID: 405
		private readonly HashSet<Client> targetClients = new HashSet<Client>();

		// Token: 0x04000196 RID: 406
		private readonly Dictionary<Client, DateTime> ignoredClients = new Dictionary<Client, DateTime>();

		// Token: 0x04000197 RID: 407
		private const float InterruptDistance = 300f;

		// Token: 0x04000198 RID: 408
		private const float BlockOtherConversationsDuration = 5f;

		// Token: 0x040001AA RID: 426
		private AIObjective prevIdleObjective;

		// Token: 0x040001AB RID: 427
		private AIObjective prevGotoObjective;

		// Token: 0x040001AC RID: 428
		private AIObjective npcWaitObjective;

		// Token: 0x040001AF RID: 431
		private static ushort actionCount;

		// Token: 0x040001B0 RID: 432
		public readonly ushort Identifier;

		// Token: 0x040001B1 RID: 433
		private float startDelay;

		// Token: 0x040001B2 RID: 434
		private int selectedOption = -1;

		// Token: 0x040001B3 RID: 435
		private bool dialogOpened;

		// Token: 0x040001B4 RID: 436
		private double lastActiveTime;

		// Token: 0x040001B5 RID: 437
		private bool interrupt;

		// Token: 0x040001B6 RID: 438
		private readonly XElement textElement;

		// Token: 0x020005A4 RID: 1444
		public class OptionActionGroup : EventAction.SubactionGroup
		{
			// Token: 0x170013BD RID: 5053
			// (get) Token: 0x06004B9A RID: 19354 RVA: 0x001DBD93 File Offset: 0x001D9F93
			// (set) Token: 0x06004B9B RID: 19355 RVA: 0x001DBD9B File Offset: 0x001D9F9B
			[Serialize("", IsPropertySaveable.Yes, "The text to display in the option.", "", false)]
			public string Text { get; set; }

			// Token: 0x170013BE RID: 5054
			// (get) Token: 0x06004B9C RID: 19356 RVA: 0x001DBDA4 File Offset: 0x001D9FA4
			// (set) Token: 0x06004B9D RID: 19357 RVA: 0x001DBDAC File Offset: 0x001D9FAC
			[Serialize(false, IsPropertySaveable.Yes, "Should this option end the conversation (closing the conversation prompt?). By default, options that don't have any actions inside them, or that only have a GoTo action, end the conversation. But if there are other actions inside the option, the game assumes there may be some kind of a follow-up coming to the conversation, and by default leaves it open.", "", false)]
			public bool EndConversation { get; set; }

			// Token: 0x170013BF RID: 5055
			// (get) Token: 0x06004B9E RID: 19358 RVA: 0x001DBDB5 File Offset: 0x001D9FB5
			// (set) Token: 0x06004B9F RID: 19359 RVA: 0x001DBDBD File Offset: 0x001D9FBD
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the player will send the Text in chat when selecting the option, or if ForceSayText is not empty, will send that instead.", "", false)]
			public bool ForceSay { get; set; }

			// Token: 0x170013C0 RID: 5056
			// (get) Token: 0x06004BA0 RID: 19360 RVA: 0x001DBDC6 File Offset: 0x001D9FC6
			// (set) Token: 0x06004BA1 RID: 19361 RVA: 0x001DBDCE File Offset: 0x001D9FCE
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the message sent in chat will be sent in radio chat instead.", "", false)]
			public bool ForceSayInRadio { get; set; }

			// Token: 0x170013C1 RID: 5057
			// (get) Token: 0x06004BA2 RID: 19362 RVA: 0x001DBDD7 File Offset: 0x001D9FD7
			// (set) Token: 0x06004BA3 RID: 19363 RVA: 0x001DBDDF File Offset: 0x001D9FDF
			[Serialize("", IsPropertySaveable.Yes, "Message sent in chat, if empty, Text is used instead.", "", false)]
			public string ForceSayText { get; set; }

			// Token: 0x170013C2 RID: 5058
			// (get) Token: 0x06004BA4 RID: 19364 RVA: 0x001DBDE8 File Offset: 0x001D9FE8
			// (set) Token: 0x06004BA5 RID: 19365 RVA: 0x001DBDF0 File Offset: 0x001D9FF0
			[Serialize(true, IsPropertySaveable.Yes, "Should the chat message be stripped of any quotation mark characters?", "", false)]
			public bool ForceSayRemoveQuotes { get; set; }

			// Token: 0x06004BA6 RID: 19366 RVA: 0x001DBDF9 File Offset: 0x001D9FF9
			public OptionActionGroup(ScriptedEvent scriptedEvent, ContentXElement element) : base(scriptedEvent, element)
			{
			}
		}

		// Token: 0x020005A5 RID: 1445
		public enum DialogTypes
		{
			// Token: 0x04002725 RID: 10021
			Regular,
			// Token: 0x04002726 RID: 10022
			Small,
			// Token: 0x04002727 RID: 10023
			Mission
		}
	}
}
