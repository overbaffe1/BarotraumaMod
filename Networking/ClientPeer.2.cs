using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000469 RID: 1129
	[NullableContext(2)]
	[Nullable(0)]
	internal abstract class ClientPeer
	{
		// Token: 0x17001350 RID: 4944
		// (get) Token: 0x06004BD9 RID: 19417 RVA: 0x0029CDBE File Offset: 0x0029AFBE
		// (set) Token: 0x06004BDA RID: 19418 RVA: 0x0029CDC6 File Offset: 0x0029AFC6
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<ServerContentPackage> ServerContentPackages { [return: Nullable(new byte[]
		{
			0,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1
		})] set; } = ImmutableArray<ServerContentPackage>.Empty;

		// Token: 0x17001351 RID: 4945
		// (get) Token: 0x06004BDB RID: 19419 RVA: 0x0029CDCF File Offset: 0x0029AFCF
		// (set) Token: 0x06004BDC RID: 19420 RVA: 0x0029CDD7 File Offset: 0x0029AFD7
		public bool AllowModDownloads { get; private set; } = true;

		// Token: 0x17001352 RID: 4946
		// (get) Token: 0x06004BDD RID: 19421 RVA: 0x0029CDE0 File Offset: 0x0029AFE0
		// (set) Token: 0x06004BDE RID: 19422 RVA: 0x0029CDE8 File Offset: 0x0029AFE8
		public NetworkConnection ServerConnection { get; protected set; }

		// Token: 0x17001353 RID: 4947
		// (get) Token: 0x06004BDF RID: 19423 RVA: 0x0029CDF1 File Offset: 0x0029AFF1
		protected bool IsOwner
		{
			get
			{
				return this.ownerKey.IsSome();
			}
		}

		// Token: 0x17001354 RID: 4948
		// (get) Token: 0x06004BE0 RID: 19424 RVA: 0x0029CDFE File Offset: 0x0029AFFE
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
		}

		// Token: 0x06004BE1 RID: 19425 RVA: 0x0029CE08 File Offset: 0x0029B008
		[NullableContext(0)]
		protected ClientPeer([Nullable(1)] Endpoint serverEndpoint, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Endpoint> allServerEndpoints, ClientPeer.Callbacks callbacks, Option<int> ownerKey)
		{
			this.ServerEndpoint = serverEndpoint;
			this.AllServerEndpoints = allServerEndpoints;
			this.callbacks = callbacks;
			this.ownerKey = ownerKey;
		}

		// Token: 0x06004BE2 RID: 19426
		public abstract void Start();

		// Token: 0x06004BE3 RID: 19427
		public abstract void Close(PeerDisconnectPacket peerDisconnectPacket);

		// Token: 0x06004BE4 RID: 19428
		public abstract void Update(float deltaTime);

		// Token: 0x06004BE5 RID: 19429
		[NullableContext(1)]
		public abstract void Send(IWriteMessage msg, DeliveryMethod deliveryMethod, bool compressPastThreshold = true);

		// Token: 0x06004BE6 RID: 19430
		[NullableContext(1)]
		public abstract void SendPassword(string password);

		// Token: 0x06004BE7 RID: 19431
		protected abstract void SendMsgInternal(PeerPacketHeaders headers, INetSerializableStruct body);

		// Token: 0x17001355 RID: 4949
		// (get) Token: 0x06004BE8 RID: 19432 RVA: 0x0029CE55 File Offset: 0x0029B055
		// (set) Token: 0x06004BE9 RID: 19433 RVA: 0x0029CE5D File Offset: 0x0029B05D
		public bool ContentPackageOrderReceived { get; set; }

		// Token: 0x17001356 RID: 4950
		// (get) Token: 0x06004BEA RID: 19434 RVA: 0x0029CE66 File Offset: 0x0029B066
		public bool WaitingForPassword
		{
			get
			{
				return this.isActive && this.initializationStep == ConnectionInitialization.Password && this.passwordMsgBox != null && GUIMessageBox.MessageBoxes.Contains(this.passwordMsgBox);
			}
		}

		// Token: 0x06004BEB RID: 19435
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		protected abstract Task<Option<AccountId>> GetAccountId();

		// Token: 0x06004BEC RID: 19436 RVA: 0x0029CE93 File Offset: 0x0029B093
		protected void OnInitializationComplete()
		{
			GUIMessageBox guimessageBox = this.passwordMsgBox;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			if (this.initializationStep == ConnectionInitialization.Success)
			{
				return;
			}
			this.callbacks.OnInitializationComplete();
			this.initializationStep = ConnectionInitialization.Success;
		}

		// Token: 0x06004BED RID: 19437 RVA: 0x0029CEC8 File Offset: 0x0029B0C8
		protected void ReadConnectionInitializationStep(ClientPeer.IncomingInitializationMessage inc)
		{
			if (inc.InitializationStep != ConnectionInitialization.Password)
			{
				GUIMessageBox guimessageBox = this.passwordMsgBox;
				if (guimessageBox != null)
				{
					guimessageBox.Close();
				}
			}
			switch (inc.InitializationStep)
			{
			case ConnectionInitialization.AuthInfoAndVersion:
				if (this.initializationStep != ConnectionInitialization.AuthInfoAndVersion)
				{
					return;
				}
				TaskPool.Add(base.GetType().Name + ".GetAccountId", this.GetAccountId(), delegate(Task t)
				{
					if (!this.IsActive)
					{
						return;
					}
					Option<AccountId> accountId;
					if (!t.TryGetResult(out accountId))
					{
						this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationFailed));
					}
					PeerPacketHeaders headers2 = new PeerPacketHeaders
					{
						DeliveryMethod = DeliveryMethod.Reliable,
						PacketHeader = PacketHeader.IsConnectionInitializationStep,
						Initialization = new ConnectionInitialization?(ConnectionInitialization.AuthInfoAndVersion)
					};
					ClientAuthTicketAndVersionPacket clientAuthTicketAndVersionPacket = default(ClientAuthTicketAndVersionPacket);
					GameClient client = GameMain.Client;
					clientAuthTicketAndVersionPacket.Name = (((client != null) ? client.Name : null) ?? "Unknown");
					clientAuthTicketAndVersionPacket.OwnerKey = this.ownerKey;
					clientAuthTicketAndVersionPacket.AccountId = accountId;
					clientAuthTicketAndVersionPacket.AuthTicket = this.authTicket;
					clientAuthTicketAndVersionPacket.GameVersion = GameMain.Version.ToString();
					clientAuthTicketAndVersionPacket.Language = GameSettings.CurrentConfig.Language.Value;
					ClientAuthTicketAndVersionPacket body = clientAuthTicketAndVersionPacket;
					this.SendMsgInternal(headers2, body);
				});
				return;
			case ConnectionInitialization.ContentPackageOrder:
			{
				ConnectionInitialization connectionInitialization = this.initializationStep;
				bool flag = connectionInitialization == ConnectionInitialization.AuthInfoAndVersion || connectionInitialization == ConnectionInitialization.Password;
				if (flag)
				{
					this.initializationStep = ConnectionInitialization.ContentPackageOrder;
				}
				if (this.initializationStep != ConnectionInitialization.ContentPackageOrder)
				{
					return;
				}
				PeerPacketHeaders headers = new PeerPacketHeaders
				{
					DeliveryMethod = DeliveryMethod.Reliable,
					PacketHeader = PacketHeader.IsConnectionInitializationStep,
					Initialization = new ConnectionInitialization?(ConnectionInitialization.ContentPackageOrder)
				};
				ServerPeerContentPackageOrderPacket orderPacket = INetSerializableStruct.Read<ServerPeerContentPackageOrderPacket>(inc.Message);
				if (!this.ContentPackageOrderReceived)
				{
					this.ServerContentPackages = orderPacket.ContentPackages;
					this.AllowModDownloads = orderPacket.AllowModDownloads;
					if (this.ServerContentPackages.Length == 0)
					{
						string errorMsg = "Error in ContentPackageOrder message: list of content packages enabled on the server was empty.";
						GameAnalyticsManager.AddErrorEventOnce("ClientPeer.ReadConnectionInitializationStep:NoContentPackages", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
					}
					this.ContentPackageOrderReceived = true;
				}
				this.SendMsgInternal(headers, null);
				return;
			}
			case ConnectionInitialization.Password:
			{
				if (this.initializationStep == ConnectionInitialization.AuthInfoAndVersion)
				{
					this.initializationStep = ConnectionInitialization.Password;
				}
				if (this.initializationStep != ConnectionInitialization.Password)
				{
					return;
				}
				ServerPeerPasswordPacket passwordPacket = INetSerializableStruct.Read<ServerPeerPasswordPacket>(inc.Message);
				if (this.WaitingForPassword)
				{
					return;
				}
				passwordPacket.Salt.TryUnwrap(out this.passwordSalt);
				int retries;
				passwordPacket.RetriesLeft.TryUnwrap(out retries);
				if (!string.IsNullOrWhiteSpace(this.AutomaticallyAttemptedPassword))
				{
					this.SendPassword(this.AutomaticallyAttemptedPassword);
					return;
				}
				LocalizedString pwMsg = TextManager.Get("PasswordRequired");
				GUIMessageBox guimessageBox2 = this.passwordMsgBox;
				if (guimessageBox2 != null)
				{
					guimessageBox2.Close();
				}
				this.passwordMsgBox = new GUIMessageBox(pwMsg, "", new LocalizedString[]
				{
					TextManager.Get("OK"),
					TextManager.Get("Cancel")
				}, new Vector2?(new Vector2(0.25f, 0.1f)), new Point?(new Point(400, GUI.IntScale(170f))), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUILayoutGroup passwordHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), this.passwordMsgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter);
				GUITextBox passwordBox = new GUITextBox(new RectTransform(new Vector2(0.8f, 1f), passwordHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, "", null, null, Alignment.Left, false, "", null, false, true)
				{
					Censor = true
				};
				if (retries > 0)
				{
					GUITextBlock incorrectPasswordText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), passwordHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("incorrectpassword"), new Color?(GUIStyle.Red), GUIStyle.Font, Alignment.Center, false, "", null);
					incorrectPasswordText.RectTransform.MinSize = new Point(0, (int)incorrectPasswordText.TextSize.Y);
					passwordHolder.Recalculate();
				}
				this.passwordMsgBox.Content.Recalculate();
				this.passwordMsgBox.Content.RectTransform.MinSize = new Point(0, this.passwordMsgBox.Content.RectTransform.Children.Sum((RectTransform c) => c.Rect.Height));
				this.passwordMsgBox.Content.Parent.RectTransform.MinSize = new Point(0, (int)((float)this.passwordMsgBox.Content.RectTransform.MinSize.Y / this.passwordMsgBox.Content.RectTransform.RelativeSize.Y));
				GUIButton okButton = this.passwordMsgBox.Buttons[0];
				GUIButton okButton3 = okButton;
				okButton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(okButton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object __)
				{
					this.SendPassword(passwordBox.Text);
					return true;
				}));
				GUIButton okButton2 = okButton;
				okButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(okButton2.OnClicked, new GUIButton.OnClickedHandler(this.passwordMsgBox.Close));
				GUIButton cancelButton = this.passwordMsgBox.Buttons[1];
				cancelButton.OnClicked = delegate(GUIButton _, object __)
				{
					this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
					GUIMessageBox guimessageBox3 = this.passwordMsgBox;
					if (guimessageBox3 != null)
					{
						guimessageBox3.Close();
					}
					this.passwordMsgBox = null;
					return true;
				};
				GUITextBox passwordBox2 = passwordBox;
				passwordBox2.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(passwordBox2.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox _, string __)
				{
					okButton.OnClicked(okButton, okButton.UserData);
					return true;
				}));
				passwordBox.Select(-1, false);
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x040027AA RID: 10154
		[Nullable(1)]
		public string AutomaticallyAttemptedPassword = string.Empty;

		// Token: 0x040027AB RID: 10155
		protected readonly ClientPeer.Callbacks callbacks;

		// Token: 0x040027AC RID: 10156
		[Nullable(1)]
		public readonly Endpoint ServerEndpoint;

		// Token: 0x040027AD RID: 10157
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public readonly ImmutableArray<Endpoint> AllServerEndpoints;

		// Token: 0x040027AF RID: 10159
		[Nullable(0)]
		protected readonly Option<int> ownerKey;

		// Token: 0x040027B0 RID: 10160
		protected bool isActive;

		// Token: 0x040027B1 RID: 10161
		protected ConnectionInitialization initializationStep;

		// Token: 0x040027B3 RID: 10163
		protected int passwordSalt;

		// Token: 0x040027B4 RID: 10164
		[Nullable(0)]
		protected Option<AuthenticationTicket> authTicket;

		// Token: 0x040027B5 RID: 10165
		private GUIMessageBox passwordMsgBox;

		// Token: 0x020011F3 RID: 4595
		[NullableContext(1)]
		[Nullable(0)]
		public readonly struct Callbacks : IEquatable<ClientPeer.Callbacks>
		{
			// Token: 0x060092B6 RID: 37558 RVA: 0x003C9E90 File Offset: 0x003C8090
			public Callbacks(ClientPeer.Callbacks.MessageCallback OnMessageReceived, ClientPeer.Callbacks.DisconnectCallback OnDisconnect, ClientPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete)
			{
				this.OnMessageReceived = OnMessageReceived;
				this.OnDisconnect = OnDisconnect;
				this.OnInitializationComplete = OnInitializationComplete;
			}

			// Token: 0x17001CDB RID: 7387
			// (get) Token: 0x060092B7 RID: 37559 RVA: 0x003C9EA7 File Offset: 0x003C80A7
			// (set) Token: 0x060092B8 RID: 37560 RVA: 0x003C9EAF File Offset: 0x003C80AF
			public ClientPeer.Callbacks.MessageCallback OnMessageReceived { get; set; }

			// Token: 0x17001CDC RID: 7388
			// (get) Token: 0x060092B9 RID: 37561 RVA: 0x003C9EB8 File Offset: 0x003C80B8
			// (set) Token: 0x060092BA RID: 37562 RVA: 0x003C9EC0 File Offset: 0x003C80C0
			public ClientPeer.Callbacks.DisconnectCallback OnDisconnect { get; set; }

			// Token: 0x17001CDD RID: 7389
			// (get) Token: 0x060092BB RID: 37563 RVA: 0x003C9EC9 File Offset: 0x003C80C9
			// (set) Token: 0x060092BC RID: 37564 RVA: 0x003C9ED1 File Offset: 0x003C80D1
			public ClientPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete { get; set; }

			// Token: 0x060092BD RID: 37565 RVA: 0x003C9EDC File Offset: 0x003C80DC
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("Callbacks");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060092BE RID: 37566 RVA: 0x003C9F28 File Offset: 0x003C8128
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("OnMessageReceived = ");
				builder.Append(this.OnMessageReceived);
				builder.Append(", OnDisconnect = ");
				builder.Append(this.OnDisconnect);
				builder.Append(", OnInitializationComplete = ");
				builder.Append(this.OnInitializationComplete);
				return true;
			}

			// Token: 0x060092BF RID: 37567 RVA: 0x003C9F81 File Offset: 0x003C8181
			[CompilerGenerated]
			public static bool operator !=(ClientPeer.Callbacks left, ClientPeer.Callbacks right)
			{
				return !(left == right);
			}

			// Token: 0x060092C0 RID: 37568 RVA: 0x003C9F8D File Offset: 0x003C818D
			[CompilerGenerated]
			public static bool operator ==(ClientPeer.Callbacks left, ClientPeer.Callbacks right)
			{
				return left.Equals(right);
			}

			// Token: 0x060092C1 RID: 37569 RVA: 0x003C9F97 File Offset: 0x003C8197
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ClientPeer.Callbacks.MessageCallback>.Default.GetHashCode(this.<OnMessageReceived>k__BackingField) * -1521134295 + EqualityComparer<ClientPeer.Callbacks.DisconnectCallback>.Default.GetHashCode(this.<OnDisconnect>k__BackingField)) * -1521134295 + EqualityComparer<ClientPeer.Callbacks.InitializationCompleteCallback>.Default.GetHashCode(this.<OnInitializationComplete>k__BackingField);
			}

			// Token: 0x060092C2 RID: 37570 RVA: 0x003C9FD7 File Offset: 0x003C81D7
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ClientPeer.Callbacks && this.Equals((ClientPeer.Callbacks)obj);
			}

			// Token: 0x060092C3 RID: 37571 RVA: 0x003C9FF0 File Offset: 0x003C81F0
			[CompilerGenerated]
			public bool Equals(ClientPeer.Callbacks other)
			{
				return EqualityComparer<ClientPeer.Callbacks.MessageCallback>.Default.Equals(this.<OnMessageReceived>k__BackingField, other.<OnMessageReceived>k__BackingField) && EqualityComparer<ClientPeer.Callbacks.DisconnectCallback>.Default.Equals(this.<OnDisconnect>k__BackingField, other.<OnDisconnect>k__BackingField) && EqualityComparer<ClientPeer.Callbacks.InitializationCompleteCallback>.Default.Equals(this.<OnInitializationComplete>k__BackingField, other.<OnInitializationComplete>k__BackingField);
			}

			// Token: 0x060092C4 RID: 37572 RVA: 0x003CA045 File Offset: 0x003C8245
			[CompilerGenerated]
			public void Deconstruct(out ClientPeer.Callbacks.MessageCallback OnMessageReceived, out ClientPeer.Callbacks.DisconnectCallback OnDisconnect, out ClientPeer.Callbacks.InitializationCompleteCallback OnInitializationComplete)
			{
				OnMessageReceived = this.OnMessageReceived;
				OnDisconnect = this.OnDisconnect;
				OnInitializationComplete = this.OnInitializationComplete;
			}

			// Token: 0x020015C1 RID: 5569
			// (Invoke) Token: 0x06009EE3 RID: 40675
			[NullableContext(0)]
			public delegate void MessageCallback(IReadMessage message);

			// Token: 0x020015C2 RID: 5570
			// (Invoke) Token: 0x06009EE7 RID: 40679
			[NullableContext(0)]
			public delegate void DisconnectCallback(PeerDisconnectPacket disconnectPacket);

			// Token: 0x020015C3 RID: 5571
			// (Invoke) Token: 0x06009EEB RID: 40683
			[NullableContext(0)]
			public delegate void InitializationCompleteCallback();
		}

		// Token: 0x020011F4 RID: 4596
		[NullableContext(0)]
		public struct IncomingInitializationMessage
		{
			// Token: 0x04005DB9 RID: 23993
			public ConnectionInitialization InitializationStep;

			// Token: 0x04005DBA RID: 23994
			[Nullable(1)]
			public IReadMessage Message;
		}
	}
}
