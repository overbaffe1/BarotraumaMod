using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.ClientSource.Settings;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Barotrauma.Steam;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenAL;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x02000042 RID: 66
	internal static class DebugConsole
	{
		// Token: 0x170002BA RID: 698
		// (get) Token: 0x060009A5 RID: 2469 RVA: 0x00057A27 File Offset: 0x00055C27
		// (set) Token: 0x060009A6 RID: 2470 RVA: 0x00057A2E File Offset: 0x00055C2E
		public static bool IsOpen
		{
			get
			{
				return DebugConsole.isOpen;
			}
			set
			{
				DebugConsole.isOpen = value;
			}
		}

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x060009A7 RID: 2471 RVA: 0x00057A36 File Offset: 0x00055C36
		public static GUITextBox TextBox
		{
			get
			{
				return DebugConsole.textBox;
			}
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x00057A40 File Offset: 0x00055C40
		public static void Init()
		{
			Alc.SetErrorReasonCallback(delegate(string msg)
			{
				DebugConsole.NewMessage(msg, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
			});
			DebugConsole.frame = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.45f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 300),
				AbsoluteOffset = new Point(10, 10)
			}, "", new Microsoft.Xna.Framework.Color?(new Microsoft.Xna.Framework.Color(0.4f, 0.4f, 0.4f, 0.8f)));
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), DebugConsole.frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f
			};
			GUITextBlock toggleText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("DebugConsoleHelpText"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.GreenYellow), GUIStyle.SmallFont, Alignment.CenterLeft, false, null, null);
			GUIButton closeButton = new GUIButton(new RectTransform(new Vector2(0.025f, 1f), toggleText.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), "X", Alignment.Center, null, null)
			{
				Color = Microsoft.Xna.Framework.Color.DarkRed,
				HoverColor = Microsoft.Xna.Framework.Color.Red,
				TextColor = Microsoft.Xna.Framework.Color.White,
				OutlineColor = Microsoft.Xna.Framework.Color.Red
			};
			GUIButton guibutton = closeButton;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				DebugConsole.isOpen = false;
				GUI.ForceMouseOn(null);
				DebugConsole.textBox.Deselect();
				return true;
			}));
			DebugConsole.listBox = new GUIListBox(new RectTransform(new Point(paddedFrame.Rect.Width, paddedFrame.Rect.Height - 60), paddedFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, false, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Black * 0.9f), "", true, false)
			{
				ScrollBarVisible = true
			};
			DebugConsole.textBox = new GUITextBox(new RectTransform(new Point(paddedFrame.Rect.Width, 30), paddedFrame.RectTransform, Anchor.BottomLeft, null, ScaleBasis.Normal, false)
			{
				IsFixedSize = false
			}, "", null, null, Alignment.Left, false, "", null, false, true);
			DebugConsole.textBox.MaxTextLength = new int?(1000);
			DebugConsole.textBox.OnKeyHit += delegate(GUITextBox sender, Keys key)
			{
				if (key != Keys.Tab && key != Keys.LeftShift)
				{
					DebugConsole.ResetAutoComplete();
				}
			};
			ChatManager.RegisterKeys(DebugConsole.textBox, DebugConsole.chatManager);
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x00057D7E File Offset: 0x00055F7E
		public static void AddToGUIUpdateList()
		{
			if (DebugConsole.isOpen)
			{
				DebugConsole.frame.AddToGUIUpdateList(false, 1);
			}
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x00057D94 File Offset: 0x00055F94
		public static void Update(float deltaTime)
		{
			ColoredText newMsg;
			while (DebugConsole.queuedMessages.TryDequeue(out newMsg))
			{
				DebugConsole.AddMessage(newMsg);
				if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.unsavedMessages.Add(newMsg);
					if (DebugConsole.unsavedMessages.Count >= DebugConsole.messagesPerFile)
					{
						DebugConsole.SaveLogs();
						DebugConsole.unsavedMessages.Clear();
					}
				}
			}
			if (!DebugConsole.IsOpen && GUI.KeyboardDispatcher.Subscriber == null)
			{
				foreach (KeyValuePair<KeyOrMouse, string> keyValuePair in DebugConsoleMapping.Instance.Bindings)
				{
					KeyOrMouse keyOrMouse;
					string text;
					keyValuePair.Deconstruct(out keyOrMouse, out text);
					KeyOrMouse key = keyOrMouse;
					string command = text;
					if (key.IsHit())
					{
						DebugConsole.ExecuteCommand(command);
					}
				}
			}
			GUITextBlock guitextBlock = DebugConsole.activeQuestionText;
			if (guitextBlock != null)
			{
				guitextBlock.SetAsLastChild();
			}
			if (PlayerInput.KeyHit(Keys.F3) && !PlayerInput.KeyDown(Keys.LeftControl) && !PlayerInput.KeyDown(Keys.RightControl))
			{
				DebugConsole.Toggle();
			}
			else if (DebugConsole.isOpen && PlayerInput.KeyHit(Keys.Escape))
			{
				DebugConsole.isOpen = false;
				GUI.ForceMouseOn(null);
				DebugConsole.textBox.Deselect();
				SoundPlayer.PlayUISound(GUISoundType.Select);
			}
			if (DebugConsole.isOpen)
			{
				DebugConsole.frame.UpdateManually(deltaTime, false, true);
				Character.DisableControls = true;
				if (PlayerInput.KeyHit(Keys.Tab) && !DebugConsole.textBox.IsIMEActive)
				{
					int increment = PlayerInput.KeyDown(Keys.LeftShift) ? -1 : 1;
					DebugConsole.textBox.Text = DebugConsole.AutoComplete(DebugConsole.textBox.Text, string.IsNullOrEmpty(DebugConsole.currentAutoCompletedCommand) ? 0 : increment);
				}
				if ((PlayerInput.KeyDown(Keys.LeftControl) || PlayerInput.KeyDown(Keys.RightControl)) && (PlayerInput.KeyDown(Keys.C) || PlayerInput.KeyDown(Keys.D) || PlayerInput.KeyDown(Keys.Z)) && DebugConsole.activeQuestionCallback != null)
				{
					DebugConsole.activeQuestionCallback = null;
					DebugConsole.activeQuestionText = null;
					DebugConsole.NewMessage(PlayerInput.KeyDown(Keys.C) ? "^C" : (PlayerInput.KeyDown(Keys.D) ? "^D" : "^Z"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), true);
				}
				if (PlayerInput.KeyHit(Keys.Enter))
				{
					DebugConsole.chatManager.Store(DebugConsole.textBox.Text);
					DebugConsole.ExecuteCommand(DebugConsole.textBox.Text);
					DebugConsole.textBox.Text = "";
				}
			}
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x00057FF4 File Offset: 0x000561F4
		public static void Toggle()
		{
			DebugConsole.isOpen = !DebugConsole.isOpen;
			if (DebugConsole.isOpen)
			{
				DebugConsole.textBox.Select(-1, true);
				DebugConsole.AddToGUIUpdateList();
			}
			else
			{
				GUI.ForceMouseOn(null);
				DebugConsole.textBox.Deselect();
			}
			SoundPlayer.PlayUISound(GUISoundType.Select);
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x00058034 File Offset: 0x00056234
		private static bool IsCommandPermitted(Identifier command, GameClient client)
		{
			string text = command.Value.ToLowerInvariant();
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
					if (!(text == "ban"))
					{
						goto IL_3CE;
					}
					break;
				case 4:
				{
					char c = text[0];
					if (c != 'h')
					{
						if (c != 'k')
						{
							goto IL_3CE;
						}
						if (!(text == "kick"))
						{
							goto IL_3CE;
						}
						return client.HasPermission(ClientPermissions.Kick);
					}
					else
					{
						if (!(text == "help"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 5:
				{
					char c = text[0];
					if (c != 'a')
					{
						if (c != 'b')
						{
							if (c != 'u')
							{
								goto IL_3CE;
							}
							if (!(text == "unban"))
							{
								goto IL_3CE;
							}
							goto IL_3C4;
						}
						else if (!(text == "banip"))
						{
							goto IL_3CE;
						}
					}
					else
					{
						if (!(text == "admin"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 6:
				case 11:
				case 12:
				case 16:
				case 17:
				case 18:
				case 21:
					goto IL_3CE;
				case 7:
				{
					char c = text[0];
					switch (c)
					{
					case 'b':
						if (!(text == "bindkey"))
						{
							goto IL_3CE;
						}
						return true;
					case 'c':
						goto IL_3CE;
					case 'd':
						if (!(text == "dumpids"))
						{
							goto IL_3CE;
						}
						return true;
					case 'e':
						if (!(text == "eosStat"))
						{
							goto IL_3CE;
						}
						return true;
					default:
						if (c != 'u')
						{
							goto IL_3CE;
						}
						if (!(text == "unbanip"))
						{
							goto IL_3CE;
						}
						goto IL_3C4;
					}
					break;
				}
				case 8:
				{
					char c = text[0];
					if (c != 'n')
					{
						if (c != 's')
						{
							goto IL_3CE;
						}
						if (!(text == "showperf"))
						{
							goto IL_3CE;
						}
						return true;
					}
					else
					{
						if (!(text == "netstats"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 9:
				{
					char c = text[0];
					if (c != 'e')
					{
						switch (c)
						{
						case 's':
							if (!(text == "savebinds"))
							{
								goto IL_3CE;
							}
							return true;
						case 't':
							if (!(text == "togglehud"))
							{
								goto IL_3CE;
							}
							return true;
						case 'u':
							if (!(text == "unbindkey"))
							{
								goto IL_3CE;
							}
							return true;
						default:
							goto IL_3CE;
						}
					}
					else
					{
						if (!(text == "eosUnlink"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 10:
					switch (text[0])
					{
					case 'b':
						if (!(text == "banaddress"))
						{
							goto IL_3CE;
						}
						break;
					case 'c':
						goto IL_3CE;
					case 'd':
						if (!(text == "dumptofile"))
						{
							goto IL_3CE;
						}
						return true;
					case 'e':
						if (!(text == "entitylist"))
						{
							goto IL_3CE;
						}
						return true;
					case 'f':
						if (!(text == "fpscounter"))
						{
							goto IL_3CE;
						}
						return true;
					default:
						goto IL_3CE;
					}
					break;
				case 13:
				{
					char c = text[0];
					if (c != 'f')
					{
						if (c != 'w')
						{
							goto IL_3CE;
						}
						if (!(text == "wikiimage_sub"))
						{
							goto IL_3CE;
						}
						return true;
					}
					else
					{
						if (!(text == "findentityids"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 14:
					if (!(text == "toggleupperhud"))
					{
						goto IL_3CE;
					}
					return true;
				case 15:
					if (!(text == "setfreecamspeed"))
					{
						goto IL_3CE;
					}
					return true;
				case 19:
					if (!(text == "wikiimage_character"))
					{
						goto IL_3CE;
					}
					return true;
				case 20:
				{
					char c = text[0];
					if (c != 'e')
					{
						if (c != 't')
						{
							goto IL_3CE;
						}
						if (!(text == "togglecharacternames"))
						{
							goto IL_3CE;
						}
						return true;
					}
					else
					{
						if (!(text == "eosLoginEpicViaSteam"))
						{
							goto IL_3CE;
						}
						return true;
					}
					break;
				}
				case 22:
					if (!(text == "togglevoicechatfilters"))
					{
						goto IL_3CE;
					}
					return true;
				default:
					goto IL_3CE;
				}
				return client.HasPermission(ClientPermissions.Ban);
				IL_3C4:
				return client.HasPermission(ClientPermissions.Unban);
			}
			IL_3CE:
			return client.HasConsoleCommandPermission(command);
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x00058418 File Offset: 0x00056618
		public static void DequeueMessages()
		{
			ColoredText newMsg;
			while (DebugConsole.queuedMessages.TryDequeue(out newMsg))
			{
				if (DebugConsole.listBox == null)
				{
					DebugConsole.Messages.Add(newMsg);
				}
				else
				{
					DebugConsole.AddMessage(newMsg);
				}
				if (GameSettings.CurrentConfig.SaveDebugConsoleLogs || GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.unsavedMessages.Add(newMsg);
				}
			}
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x00058474 File Offset: 0x00056674
		private static void AddMessage(ColoredText msg)
		{
			if (DebugConsole.listBox == null)
			{
				return;
			}
			if (DebugConsole.listBox.Content.CountChildren > 300)
			{
				DebugConsole.listBox.RemoveChild(DebugConsole.listBox.Content.Children.First<GUIComponent>());
			}
			DebugConsole.Messages.Add(msg);
			if (DebugConsole.Messages.Count > 300)
			{
				DebugConsole.Messages.RemoveRange(0, DebugConsole.Messages.Count - 300);
			}
			try
			{
				if (msg.IsError)
				{
					Action <>9__1;
					GUIFrame textContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0f), DebugConsole.listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White))
					{
						CanBeFocused = true,
						OnSecondaryClicked = delegate(GUIComponent component, object data)
						{
							ContextMenuOption[] array = new ContextMenuOption[1];
							int num = 0;
							string label = "editor.copytoclipboard";
							bool isEnabled = true;
							Action onSelected;
							if ((onSelected = <>9__1) == null)
							{
								onSelected = (<>9__1 = delegate()
								{
									Clipboard.SetText(msg.Text);
								});
							}
							array[num] = new ContextMenuOption(label, isEnabled, onSelected);
							GUIContextMenu.CreateContextMenu(array);
							return true;
						}
					};
					RectTransform rectTransform = new RectTransform(new Point(DebugConsole.listBox.Content.Rect.Width - 5, 0), textContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
					rectTransform.AbsoluteOffset = new Point(2, 2);
					RichString text = RichString.Rich(msg.Text, null);
					GUIFont smallFont = GUIStyle.SmallFont;
					GUITextBlock textBlock = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.TopLeft, true, "", null)
					{
						CanBeFocused = false,
						TextColor = msg.Color
					};
					textContainer.RectTransform.NonScaledSize = new Point(textContainer.RectTransform.NonScaledSize.X, textBlock.RectTransform.NonScaledSize.Y + 5);
					textBlock.SetTextPos();
				}
				else
				{
					RectTransform rectT = new RectTransform(new Vector2(1f, 0f), DebugConsole.listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = RichString.Rich(msg.Text, null);
					GUIFont smallFont = GUIStyle.SmallFont;
					GUITextBlock guitextBlock = new GUITextBlock(rectT, text2, null, smallFont, Alignment.Left, true, "", null);
					guitextBlock.CanBeFocused = false;
					guitextBlock.TextColor = msg.Color;
				}
				DebugConsole.listBox.UpdateScrollBarSize();
				DebugConsole.listBox.BarScroll = 1f;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to add a message to the debug console.", e, null, false, false);
			}
			DebugConsole.chatManager.Clear();
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x0005874C File Offset: 0x0005694C
		private static void AssignOnClientExecute(string names, Action<string[]> onClientExecute)
		{
			DebugConsole.Command command = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (command == null)
			{
				throw new Exception("AssignOnClientExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			command.OnClientExecute = onClientExecute;
			command.RelayToServer = false;
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x000587AC File Offset: 0x000569AC
		private static void AssignRelayToServer(string names, bool relay)
		{
			DebugConsole.Command command = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (command == null)
			{
				DebugConsole.Log("Could not assign to relay to server: " + names);
				return;
			}
			command.RelayToServer = relay;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x00058800 File Offset: 0x00056A00
		private unsafe static void InitProjectSpecific()
		{
			DebugConsole.InitShowSoldItems();
			DebugConsole.commands.Add(new DebugConsole.Command("eosStat", "Query and display all logged in EOS users. Normally this is at most two users, but in a developer environment it could be more.", delegate(string[] args)
			{
				if (!EosInterface.Core.IsInitialized)
				{
					DebugConsole.NewMessage("EOS not initialized", null, false);
					return;
				}
				ImmutableArray<EosInterface.ProductUserId> loggedInUsers = EosInterface.IdQueries.GetLoggedInPuids();
				if (!loggedInUsers.Any<EosInterface.ProductUserId>())
				{
					DebugConsole.NewMessage("EOS user not logged in", null, false);
					return;
				}
				DebugConsole.NewMessage("Logged in EOS users:", null, false);
				ImmutableArray<EosInterface.ProductUserId>.Enumerator enumerator = loggedInUsers.GetEnumerator();
				while (enumerator.MoveNext())
				{
					EosInterface.ProductUserId puid = enumerator.Current;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler.AppendLiteral("eosStat -> ");
					defaultInterpolatedStringHandler.AppendFormatted<EosInterface.ProductUserId>(puid);
					TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), EosInterface.IdQueries.GetSelfExternalAccountIds(puid), delegate(Task t)
					{
						Result<ImmutableArray<AccountId>, EosInterface.IdQueries.GetSelfExternalIdError> result;
						if (!t.TryGetResult(out result))
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler2.AppendLiteral(" - ");
						defaultInterpolatedStringHandler2.AppendFormatted<EosInterface.ProductUserId>(puid);
						DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
						ImmutableArray<AccountId> ids;
						if (result.TryUnwrapSuccess(out ids))
						{
							foreach (AccountId id in ids)
							{
								DebugConsole.<>c__DisplayClass23_1 CS$<>8__locals2 = new DebugConsole.<>c__DisplayClass23_1();
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(5, 1);
								defaultInterpolatedStringHandler3.AppendLiteral("   - ");
								defaultInterpolatedStringHandler3.AppendFormatted<AccountId>(id);
								DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, false);
								CS$<>8__locals2.eaid = (id as EpicAccountId);
								if (CS$<>8__locals2.eaid != null)
								{
									CS$<>8__locals2.<InitProjectSpecific>g__gameOwnershipTokenTest|150();
									EosInterface.Login.TestEosSessionTimeoutRecovery(puid);
								}
							}
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(44, 2);
						defaultInterpolatedStringHandler4.AppendLiteral("   - Failed to get external IDs linked to ");
						defaultInterpolatedStringHandler4.AppendFormatted<EosInterface.ProductUserId>(puid);
						defaultInterpolatedStringHandler4.AppendLiteral(": ");
						defaultInterpolatedStringHandler4.AppendFormatted<Result<ImmutableArray<AccountId>, EosInterface.IdQueries.GetSelfExternalIdError>>(result);
						DebugConsole.NewMessage(defaultInterpolatedStringHandler4.ToStringAndClear(), null, false);
					});
				}
			}, null, false));
			DebugConsole.AssignRelayToServer("eosStat", false);
			DebugConsole.commands.Add(new DebugConsole.Command("eosUnlink", "Unlink the primary logged in external account ID from its corresponding EOS Product User ID and close the game. This is meant to be used to test the EOS consent flow.", delegate(string[] args)
			{
				EosInterface.ProductUserId userId = EosInterface.IdQueries.GetLoggedInPuids().FirstOrDefault<EosInterface.ProductUserId>();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Unlinking external account from PUID ");
				defaultInterpolatedStringHandler.AppendFormatted<EosInterface.ProductUserId>(userId);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.CrossplayChoice = EosSteamPrimaryLogin.CrossplayChoice.Unknown;
				GameSettings.SetCurrentConfig(config);
				GameSettings.SaveCurrentConfig();
				TaskPool.Add("unlinkTask", EosInterface.Login.UnlinkExternalAccount(userId), delegate(Task _)
				{
					GameMain.Instance.Exit();
				});
			}, null, false));
			DebugConsole.AssignRelayToServer("eosUnlink", false);
			DebugConsole.commands.Add(new DebugConsole.Command("eosLoginEpicViaSteam", "Log into an Epic account via a link to the currently logged in Steam account", delegate(string[] args)
			{
				string name = "eosLoginEpicViaSteam";
				Task task = EosEpicSecondaryLogin.LoginToLinkedEpicAccount();
				Action<Task> onCompletion;
				if ((onCompletion = DebugConsole.<>O.<0>__IgnoredCallback) == null)
				{
					onCompletion = (DebugConsole.<>O.<0>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
				}
				TaskPool.Add(name, task, onCompletion);
			}, null, false));
			DebugConsole.AssignRelayToServer("eosLoginEpicViaSteam", false);
			DebugConsole.commands.Add(new DebugConsole.Command("resetgameanalyticsconsent", "Reset whether you've given your consent for the game to send statistics to GameAnalytics. After executing the command, the game should ask for your consent again on relaunch.", delegate(string[] args)
			{
				GameAnalyticsManager.ResetConsent();
			}, null, false));
			DebugConsole.AssignRelayToServer("resetgameanalyticsconsent", false);
			DebugConsole.commands.Add(new DebugConsole.Command("copyitemnames", "", delegate(string[] args)
			{
				StringBuilder sb = new StringBuilder();
				foreach (ItemPrefab mp in ItemPrefab.Prefabs)
				{
					sb.AppendLine(mp.Name.Value);
				}
				Clipboard.SetText(sb.ToString());
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autohull", "", delegate(string[] args)
			{
				if (Screen.Selected != GameMain.SubEditorScreen)
				{
					return;
				}
				if (MapEntity.MapEntityList.Any((MapEntity e) => e is Hull || e is Gap))
				{
					DebugConsole.ShowQuestionPrompt("This submarine already has hulls and/or gaps. This command will delete them. Do you want to continue? Y/N", delegate(string option)
					{
						DebugConsole.ShowQuestionPrompt("The automatic hull generation may not work correctly if your submarine uses curved walls. Do you want to continue? Y/N", delegate(string option2)
						{
							if (option2.ToLowerInvariant() == "y")
							{
								GameMain.SubEditorScreen.AutoHull();
							}
						}, null, -1);
					}, null, -1);
					return;
				}
				DebugConsole.ShowQuestionPrompt("The automatic hull generation may not work correctly if your submarine uses curved walls. Do you want to continue? Y/N", delegate(string option)
				{
					if (option.ToLowerInvariant() == "y")
					{
						GameMain.SubEditorScreen.AutoHull();
					}
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("enablecheats", "enablecheats: Enables cheat commands and disables achievements during this play session.", delegate(string[] args)
			{
				DebugConsole.CheatsEnabled = true;
				AchievementManager.CheatsEnabled = true;
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					campaign.CheatsEnabled = true;
				}
				DebugConsole.NewMessage("Enabled cheat commands.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
				DebugConsole.NewMessage("Achievements have been disabled during this play session.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, false));
			DebugConsole.AssignRelayToServer("enablecheats", true);
			DebugConsole.commands.Add(new DebugConsole.Command("mainmenu|menu", "mainmenu/menu: Go to the main menu.", delegate(string[] args)
			{
				GameMain.GameSession = null;
				List<Character> characters = new List<Character>(Character.CharacterList);
				foreach (Character c in characters)
				{
					c.Remove();
				}
				GameMain.MainMenuScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("game", "gamescreen/game: Go to the \"in-game\" view.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					DebugConsole.NewMessage("WARNING: Switching directly from the submarine editor to the game view may cause bugs and crashes. Use with caution.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
					if (Entity.Spawner == null)
					{
						Entity.Spawner = new EntitySpawner();
					}
				}
				GameMain.GameScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editsubs|subeditor", "editsubs/subeditor: Switch to the Submarine Editor to create or edit submarines.", delegate(string[] args)
			{
				if (args.Length != 0)
				{
					SubmarineInfo subInfo = new SubmarineInfo(string.Join(" ", args), "", null, true, false);
					Submarine.MainSub = Submarine.Load(subInfo, true, null);
				}
				GameMain.SubEditorScreen.Select(Screen.Selected != GameMain.GameScreen);
				EntitySpawner spawner = Entity.Spawner;
				if (spawner != null)
				{
					spawner.Remove();
				}
				Entity.Spawner = null;
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("editparticles|particleeditor", "editparticles/particleeditor: Switch to the Particle Editor to edit particle effects.", delegate(string[] args)
			{
				GameMain.ParticleEditorScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editlevels|leveleditor", "editlevels/leveleditor: Switch to the Level Editor to edit levels.", delegate(string[] args)
			{
				GameMain.LevelEditorScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editsprites|spriteeditor", "editsprites/spriteeditor: Switch to the Sprite Editor to edit the source rects and origins of sprites.", delegate(string[] args)
			{
				GameMain.SpriteEditorScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editevents|eventeditor", "editevents/eventeditor: Switch to the Event Editor to edit scripted events.", delegate(string[] args)
			{
				GameMain.EventEditorScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editcharacters|charactereditor", "editcharacters/charactereditor: Switch to the Character Editor to edit/create the ragdolls and animations of characters.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.GameScreen)
				{
					DebugConsole.NewMessage("WARNING: Switching between the character editor and the game view may cause odd behaviour or bugs. Use with caution.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
				}
				GameMain.CharacterEditorScreen.Select();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("settainted", "settainted [true/false]: Sets tainted effect on hovered genetic material.", delegate(string[] args)
			{
				if (Character.Controlled == null)
				{
					DebugConsole.NewMessage("No controlled character!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				Character controlled = Character.Controlled;
				Item item;
				if ((item = ((controlled != null) ? controlled.FocusedItem : null)) == null)
				{
					Inventory.SlotReference selectedSlot = Inventory.SelectedSlot;
					item = ((selectedSlot != null) ? selectedSlot.Item : null);
				}
				Item focusedItem = item;
				if (focusedItem == null)
				{
					DebugConsole.NewMessage("No focused item, hover on something!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				GeneticMaterial geneticMaterial = focusedItem.GetComponent<GeneticMaterial>();
				if (geneticMaterial == null)
				{
					DebugConsole.NewMessage("Not hovering on a genetic material!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				bool newValue = args.None((string arg) => string.Equals(arg, "false", StringComparison.InvariantCultureIgnoreCase));
				geneticMaterial.SetTainted(newValue, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Set tainted to ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(newValue);
				defaultInterpolatedStringHandler.AppendLiteral(" for ");
				defaultInterpolatedStringHandler.AppendFormatted(focusedItem.Name);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("quickstart", "Starts a singleplayer sandbox", delegate(string[] args)
			{
				if (Screen.Selected != GameMain.MainMenuScreen)
				{
					DebugConsole.ThrowError("This command can only be executed from the main menu.", null, null, false, false);
					return;
				}
				Identifier subName = ((args.Length != 0) ? args[0] : "").ToIdentifier();
				if (subName.IsEmpty)
				{
					DebugConsole.ThrowError("No submarine specified.", null, null, false, false);
					return;
				}
				float difficulty = 40f;
				if (args.Length > 1)
				{
					float.TryParse(args[1], out difficulty);
				}
				LevelGenerationParams levelGenerationParams = null;
				if (args.Length > 2)
				{
					string levelGenerationIdentifier = args[2];
					levelGenerationParams = LevelGenerationParams.LevelParams.FirstOrDefault((LevelGenerationParams p) => p.Identifier == levelGenerationIdentifier);
				}
				if (SubmarineInfo.SavedSubmarines.None((SubmarineInfo s) => s.Name == subName))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Cannot find a sub that matches the name \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(subName);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
				GameMain.MainMenuScreen.QuickStart(false, subName, difficulty, levelGenerationParams);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from s in (from s in SubmarineInfo.SavedSubmarines
				select s.Name).Distinct<string>()
				orderby s
				select s).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("forcewreck", "forcewreck [wreckname] (optional, ThalamusSpawn)[Random/Forced/Disabled]: When generating levels, ensures a specific wreck is generated. Second optional parameter to control thalamus spawning.", delegate(string[] args)
			{
				if (args.Length != 0)
				{
					WreckFile submarineFile = DebugConsole.GetSubmarineFile<WreckFile>(args[0]);
					if (submarineFile != null)
					{
						SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == submarineFile.Path.Value);
						if (matchingSub != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Setting ForceWreck to: ");
							defaultInterpolatedStringHandler.AppendFormatted(matchingSub.Name);
							defaultInterpolatedStringHandler.AppendLiteral(", ");
							defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(submarineFile.Path);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
							LevelData.ConsoleForceWreck = matchingSub;
						}
					}
					else
					{
						DebugConsole.NewMessage("Can't find: " + args[0], new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					}
				}
				if (args.Length <= 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(26, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Setting ThalamusSpawn to: ");
					defaultInterpolatedStringHandler2.AppendFormatted<LevelData.ThalamusSpawn>(LevelData.ThalamusSpawn.Random);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
					LevelData.ForceThalamus = LevelData.ThalamusSpawn.Random;
					return;
				}
				string forceThalamusArg = args[1];
				LevelData.ThalamusSpawn result;
				if (Enum.TryParse<LevelData.ThalamusSpawn>(forceThalamusArg, true, out result))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(26, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Setting ThalamusSpawn to: ");
					defaultInterpolatedStringHandler3.AppendFormatted<LevelData.ThalamusSpawn>(result);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
					LevelData.ForceThalamus = result;
					return;
				}
				DebugConsole.NewMessage("Can't parse argument: " + forceThalamusArg, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, () => new string[][]
			{
				DebugConsole.ListSubmarineFileNames<WreckFile>(),
				new string[]
				{
					LevelData.ThalamusSpawn.Random.ToString(),
					LevelData.ThalamusSpawn.Forced.ToString(),
					LevelData.ThalamusSpawn.Disabled.ToString()
				}
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("forcebeaconstation|forcebeacon", "forcebeaconstation [station name]: When generating levels, ensures a specific beacon station is generated.", delegate(string[] args)
			{
				if (args.Length != 0)
				{
					BeaconStationFile submarineFile = DebugConsole.GetSubmarineFile<BeaconStationFile>(args[0]);
					if (submarineFile != null)
					{
						SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == submarineFile.Path.Value);
						if (matchingSub != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Setting ForceBeaconStation to: ");
							defaultInterpolatedStringHandler.AppendFormatted(matchingSub.Name);
							defaultInterpolatedStringHandler.AppendLiteral(", ");
							defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(submarineFile.Path);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
							LevelData.ConsoleForceBeaconStation = matchingSub;
							return;
						}
					}
					else
					{
						DebugConsole.NewMessage("Can't find: " + args[0], new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					}
				}
			}, () => new string[][]
			{
				DebugConsole.ListSubmarineFileNames<BeaconStationFile>()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("reloadcontentfile", "reloadcontentfile [filepath]: Reloads a specific content xml file during runtime.", delegate(string[] args)
			{
				if (args.Length != 0)
				{
					string pathArgument = args[0];
					ContentFile contentFile = DebugConsole.GetContentFile(pathArgument);
					if (contentFile != null)
					{
						DebugConsole.NewMessage("Reloading content file: " + pathArgument, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
						contentFile.UnloadFile();
						contentFile.LoadFile();
						return;
					}
					DebugConsole.NewMessage("Can't find " + args[0] + " to reload", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
				}
			}, () => new string[][]
			{
				DebugConsole.ListContentFilePaths()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("steamnetdebug", "steamnetdebug: Toggles Steamworks networking debug logging.", delegate(string[] args)
			{
				SteamManager.SetSteamworksNetworkingDebugLog(!SteamManager.NetworkingDebugLog);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("readycheck", "Commence a ready check in multiplayer.", delegate(string[] args)
			{
				DebugConsole.NewMessage("Ready checks can only be commenced in multiplayer.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setsalary", "setsalary [0-100] [character/default]: Sets the salary of a certain character or the default salary to a percentage.", delegate(string[] args)
			{
				DebugConsole.ThrowError("This command can only be used in multiplayer campaign.", null, null, false, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = new string[]
				{
					"0",
					"100"
				};
				array[1] = new string[]
				{
					"default"
				}.Union(from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("bindkey", "bindkey [key] [command]: Binds a key to a command.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("No key or command specified.", null, null, false, false);
					return;
				}
				string keyString = args[0];
				string command = args[1];
				Keys outKey;
				MouseButton outMouseButton;
				KeyOrMouse key = Enum.TryParse<Keys>(keyString, true, out outKey) ? outKey : (Enum.TryParse<MouseButton>(keyString, true, out outMouseButton) ? outMouseButton : MouseButton.None);
				if (key.Key == Keys.None && key.MouseButton == MouseButton.None)
				{
					DebugConsole.ThrowError("Invalid key " + keyString + ".", null, null, false, false);
					return;
				}
				DebugConsoleMapping.Instance.Set(key, command);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				defaultInterpolatedStringHandler.AppendFormatted(command);
				defaultInterpolatedStringHandler.AppendLiteral("\" bound to ");
				defaultInterpolatedStringHandler.AppendFormatted<KeyOrMouse>(key);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(GUIStyle.Green), false);
				KeyValuePair<InputType, KeyOrMouse> existingBind = GameSettings.CurrentConfig.KeyMap.Bindings.FirstOrDefault((KeyValuePair<InputType, KeyOrMouse> bind) => bind.Value.Key != Keys.None && bind.Value.Key == key);
				if (existingBind.Value != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(82, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("\"");
					defaultInterpolatedStringHandler2.AppendFormatted<KeyOrMouse>(key);
					defaultInterpolatedStringHandler2.AppendLiteral("\" has already been bound to ");
					defaultInterpolatedStringHandler2.AppendFormatted<InputType>(existingBind.Key);
					defaultInterpolatedStringHandler2.AppendLiteral(". The keybind will perform both actions when pressed.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
			}, () => new string[][]
			{
				Enum.GetNames(typeof(Keys)),
				new string[]
				{
					"\"\""
				}
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("unbindkey", "unbindkey [key]: Unbinds a command.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					DebugConsole.ThrowError("No key specified.", null, null, false, false);
					return;
				}
				string keyString = args[0];
				Keys outKey;
				MouseButton outMouseButton;
				KeyOrMouse key = Enum.TryParse<Keys>(keyString, true, out outKey) ? outKey : (Enum.TryParse<MouseButton>(keyString, true, out outMouseButton) ? outMouseButton : MouseButton.None);
				if (key.Key == Keys.None && key.MouseButton == MouseButton.None)
				{
					DebugConsole.ThrowError("Invalid key " + keyString + ".", null, null, false, false);
					return;
				}
				DebugConsoleMapping.Instance.Remove(key);
				DebugConsole.NewMessage("Keybind unbound.", new Microsoft.Xna.Framework.Color?(GUIStyle.Green), false);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from k in (from keys in DebugConsoleMapping.Instance.Bindings.Keys
				select keys.ToString()).Distinct<string>()
				orderby k
				select k).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("savebinds", "savebinds: Writes current keybinds into the config file.", delegate(string[] args)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(127, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Some keybinds may render the game unusable, are you sure you want to make these keybinds persistent? (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(DebugConsoleMapping.Instance.Bindings.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" keybind(s) assigned) Y/N");
				DebugConsole.ShowQuestionPrompt(defaultInterpolatedStringHandler.ToStringAndClear(), delegate(string option2)
				{
					Identifier identifier = option2.ToIdentifier();
					if (identifier != "y")
					{
						DebugConsole.NewMessage("Aborted.", new Microsoft.Xna.Framework.Color?(GUIStyle.Red), false);
						return;
					}
					GameSettings.SaveCurrentConfig();
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglespoofeventmanagerid", "togglespoofeventmanagerid: Forces the client to report the last received event ID as always being 1, making the server believe the client is always behind.", delegate(string[] args)
			{
				if (GameMain.Client != null)
				{
					GameMain.Client.SpoofEntityManagerReceivedId = !GameMain.Client.SpoofEntityManagerReceivedId;
					DebugConsole.NewMessage(GameMain.Client.SpoofEntityManagerReceivedId ? "Spoofing enabled " : "Spoofing disabled", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
					return;
				}
				DebugConsole.NewMessage("Not connected to server", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglegrid", "Toggle visual snap grid in sub editor.", delegate(string[] args)
			{
				SubEditorScreen.ShouldDrawGrid = !SubEditorScreen.ShouldDrawGrid;
				DebugConsole.NewMessage(SubEditorScreen.ShouldDrawGrid ? "Enabled submarine grid." : "Disabled submarine grid.", new Microsoft.Xna.Framework.Color?(GUIStyle.Green), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("spreadsheetexport", "Export items in format recognized by the spreadsheet importer.", delegate(string[] args)
			{
				SpreadsheetExport.Export();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("wikiimage_character", "Save an image of the currently controlled character with a transparent background.", delegate(string[] args)
			{
				if (Character.Controlled == null)
				{
					return;
				}
				try
				{
					WikiImage.Create(Character.Controlled);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("The command 'wikiimage_character' failed.", e, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("wikiimage_sub", "Save an image of the main submarine with a transparent background.", delegate(string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				try
				{
					MapEntity.SelectedList.Clear();
					MapEntity.ClearHighlightedEntities();
					WikiImage.Create(Submarine.MainSub);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("The command 'wikiimage_sub' failed.", e, null, false, false);
				}
			}, null, false));
			DebugConsole.AssignOnExecute("loslightingfreecam", delegate(string[] args)
			{
				DebugConsole.ExecuteCommand("los");
				DebugConsole.ExecuteCommand("lighting");
				DebugConsole.ExecuteCommand("freecam");
			});
			DebugConsole.AssignRelayToServer("loslightingfreecam", false);
			DebugConsole.AssignRelayToServer("kick", false);
			DebugConsole.AssignRelayToServer("kickid", false);
			DebugConsole.AssignRelayToServer("ban", false);
			DebugConsole.AssignRelayToServer("banid", false);
			DebugConsole.AssignRelayToServer("dumpids", false);
			DebugConsole.AssignRelayToServer("dumptofile", false);
			DebugConsole.AssignRelayToServer("findentityids", false);
			DebugConsole.AssignRelayToServer("campaigninfo", false);
			DebugConsole.AssignRelayToServer("help", false);
			DebugConsole.AssignRelayToServer("verboselogging", false);
			DebugConsole.AssignRelayToServer("freecam", false);
			DebugConsole.AssignRelayToServer("steamnetdebug", false);
			DebugConsole.AssignRelayToServer("quickstart", false);
			DebugConsole.AssignRelayToServer("togglegrid", false);
			DebugConsole.AssignRelayToServer("bindkey", false);
			DebugConsole.AssignRelayToServer("unbindkey", false);
			DebugConsole.AssignRelayToServer("savebinds", false);
			DebugConsole.AssignRelayToServer("spreadsheetexport", false);
			DebugConsole.commands.Add(new DebugConsole.Command("clientlist", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("clientlist", true);
			DebugConsole.commands.Add(new DebugConsole.Command("say", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("say", true);
			DebugConsole.commands.Add(new DebugConsole.Command("msg", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("msg", true);
			DebugConsole.commands.Add(new DebugConsole.Command("setmaxplayers|maxplayers", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("setmaxplayers", true);
			DebugConsole.commands.Add(new DebugConsole.Command("setpassword|password", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("setpassword", true);
			DebugConsole.commands.Add(new DebugConsole.Command("traitorlist", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("traitorlist", true);
			DebugConsole.AssignRelayToServer("money", true);
			DebugConsole.AssignRelayToServer("showmoney", true);
			DebugConsole.AssignRelayToServer("setskill", true);
			DebugConsole.AssignRelayToServer("setsalary", true);
			DebugConsole.AssignRelayToServer("readycheck", true);
			DebugConsole.commands.Add(new DebugConsole.Command("debugjobassignment", "", delegate(string[] args)
			{
			}, null, false));
			DebugConsole.AssignRelayToServer("debugjobassignment", true);
			DebugConsole.AssignRelayToServer("givetalent", true);
			DebugConsole.AssignRelayToServer("unlocktalents", true);
			DebugConsole.AssignRelayToServer("giveexperience", true);
			DebugConsole.AssignOnExecute("control", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				if (GameMain.NetworkMember == null)
				{
					Character character = DebugConsole.FindMatchingCharacter(args, true, null, false);
					if (character != null)
					{
						Character.Controlled = character;
					}
					return;
				}
				GameClient client = GameMain.Client;
				if (client == null)
				{
					return;
				}
				client.SendConsoleCommand("control " + string.Join(' ', new string[]
				{
					args[0]
				}));
			});
			DebugConsole.AssignRelayToServer("control", true);
			DebugConsole.commands.Add(new DebugConsole.Command("shake", "", delegate(string[] args)
			{
				GameMain.GameScreen.Cam.Shake = 10f;
			}, null, false));
			DebugConsole.AssignOnExecute("explosion", delegate(string[] args)
			{
				Vector2 explosionPos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
				float range = 500f;
				float force = 10f;
				float damage = 50f;
				float structureDamage = 20f;
				float itemDamage = 100f;
				float empStrength = 0f;
				float ballastFloraStrength = 50f;
				if (args.Length != 0)
				{
					float.TryParse(args[0], out range);
				}
				if (args.Length > 1)
				{
					float.TryParse(args[1], out force);
				}
				if (args.Length > 2)
				{
					float.TryParse(args[2], out damage);
				}
				if (args.Length > 3)
				{
					float.TryParse(args[3], out structureDamage);
				}
				if (args.Length > 4)
				{
					float.TryParse(args[4], out itemDamage);
				}
				if (args.Length > 5)
				{
					float.TryParse(args[5], out empStrength);
				}
				if (args.Length > 6)
				{
					float.TryParse(args[6], out ballastFloraStrength);
				}
				new Explosion(range, force, damage, structureDamage, itemDamage, empStrength, ballastFloraStrength).Explode(explosionPos, null, null);
			});
			DebugConsole.AssignOnExecute("teleportcharacter|teleport", delegate(string[] args)
			{
				Vector2 cursorWorldPos = GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition);
				DebugConsole.TeleportCharacter(cursorWorldPos, Character.Controlled, args);
			});
			DebugConsole.AssignOnExecute("spawn|spawncharacter", delegate(string[] args)
			{
				DebugConsole.SpawnCharacter(args, GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition), false);
			});
			DebugConsole.AssignOnExecute("spawnnpc", delegate(string[] args)
			{
				DebugConsole.SpawnCharacter(args, GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition), true);
			});
			DebugConsole.AssignOnExecute("los", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameMain.LightManager.LosEnabled;
				}
				GameMain.LightManager.LosEnabled = state;
				DebugConsole.NewMessage("Line of sight effect " + (GameMain.LightManager.LosEnabled ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("los", false);
			DebugConsole.AssignOnExecute("lighting|lights", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameMain.LightManager.LightingEnabled;
				}
				GameMain.LightManager.LightingEnabled = state;
				DebugConsole.NewMessage("Lighting " + (GameMain.LightManager.LightingEnabled ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("lighting|lights", false);
			DebugConsole.AssignOnExecute("ambientlight", delegate(string[] args)
			{
				bool add = string.Equals(args.LastOrDefault<string>(), "add");
				string colorString = string.Join(",", add ? args.SkipLast(1) : args);
				if (colorString.Equals("restore", StringComparison.OrdinalIgnoreCase))
				{
					foreach (Hull hull in Hull.HullList)
					{
						if (hull.OriginalAmbientLight != null)
						{
							hull.AmbientLight = hull.OriginalAmbientLight.Value;
							hull.OriginalAmbientLight = null;
						}
					}
					DebugConsole.NewMessage("Restored all hull ambient lights", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
					return;
				}
				Microsoft.Xna.Framework.Color color = XMLExtensions.ParseColor(colorString, true);
				if (Level.Loaded != null)
				{
					Level.Loaded.GenerationParams.AmbientLightColor = color;
				}
				else
				{
					GameMain.LightManager.AmbientLight = (add ? GameMain.LightManager.AmbientLight.Add(color) : color);
				}
				foreach (Hull hull2 in Hull.HullList)
				{
					Hull hull3 = hull2;
					Microsoft.Xna.Framework.Color value = hull3.OriginalAmbientLight.GetValueOrDefault();
					if (hull3.OriginalAmbientLight == null)
					{
						value = hull2.AmbientLight;
						hull3.OriginalAmbientLight = new Microsoft.Xna.Framework.Color?(value);
					}
					hull2.AmbientLight = (add ? hull2.AmbientLight.Add(color) : color);
				}
				if (add)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Set ambient light color to ");
					defaultInterpolatedStringHandler.AppendFormatted<Microsoft.Xna.Framework.Color>(color);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(28, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Increased ambient light by ");
				defaultInterpolatedStringHandler2.AppendFormatted<Microsoft.Xna.Framework.Color>(color);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("ambientlight", false);
			DebugConsole.commands.Add(new DebugConsole.Command("multiplylights", "Multiplies the colors of all the static lights in the sub with the given Vector4 value (for example, 1,1,1,0.5).", delegate(string[] args)
			{
				if (Screen.Selected != GameMain.SubEditorScreen || args.Length < 1)
				{
					DebugConsole.ThrowError("The multiplylights command can only be used in the submarine editor.", null, null, false, false);
				}
				if (args.Length < 1)
				{
					return;
				}
				Vector4 value = XMLExtensions.ParseVector4(string.Join("", args), true);
				foreach (Item item in Item.ItemList)
				{
					if (item.ParentInventory == null && item.body == null)
					{
						LightComponent lightComponent = item.GetComponent<LightComponent>();
						if (lightComponent != null)
						{
							lightComponent.LightColor = new Microsoft.Xna.Framework.Color((float)lightComponent.LightColor.R / 255f * value.X, (float)lightComponent.LightColor.G / 255f * value.Y, (float)lightComponent.LightColor.B / 255f * value.Z, (float)lightComponent.LightColor.A / 255f * value.W);
						}
					}
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("steamtimelinetest", "steamtimelinetest: Test the Steamworks timeline feature.", delegate(string[] args)
			{
				TimelineEventHandle eventHandle = SteamTimeline.AddInstantaneousTimelineEvent("Barotrauma Test Event", "This is a test event created from the debug console", "steam_marker", 1U, 0f, TimelineEventClipPriority.Standard);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Steamworks timeline test: Added instantaneous event with handle: ");
				defaultInterpolatedStringHandler.AppendFormatted<TimelineEventHandle>(eventHandle);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setsteamtimelinegamemode", "setsteamtimelinegamemode [gamemode]: Sets the Steam timeline gamemode to the specified value.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.NewMessage("Please specify a gamemode. Available modes: " + string.Join(", ", Enum.GetNames(typeof(SteamTimelineManager.TimelineGameMode))), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				SteamTimelineManager.TimelineGameMode gameMode;
				if (Enum.TryParse<SteamTimelineManager.TimelineGameMode>(args[0], true, out gameMode))
				{
					SteamTimelineManager.SetTimelineGameMode(gameMode);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Timeline gamemode set to: ");
					defaultInterpolatedStringHandler.AppendFormatted<SteamTimelineManager.TimelineGameMode>(gameMode);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
					return;
				}
				DebugConsole.NewMessage("Invalid gamemode '" + args[0] + "'. Available modes: " + string.Join(", ", Enum.GetNames(typeof(SteamTimelineManager.TimelineGameMode))), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, () => new string[][]
			{
				Enum.GetNames(typeof(SteamTimelineManager.TimelineGameMode))
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("color|colour", "Change color (as bytes from 0 to 255) of the selected item/structure instances. Applied only in the subeditor.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					if (!MapEntity.SelectedAny)
					{
						DebugConsole.ThrowError("You have to select item(s)/structure(s) first!", null, null, false, false);
						return;
					}
					if (args.Length < 3)
					{
						DebugConsole.ThrowError("Not enough arguments provided! At least three required.", null, null, false, false);
						return;
					}
					byte r;
					if (!byte.TryParse(args[0], out r))
					{
						DebugConsole.ThrowError("Failed to parse value for RED from " + args[0], null, null, false, false);
					}
					byte g;
					if (!byte.TryParse(args[1], out g))
					{
						DebugConsole.ThrowError("Failed to parse value for GREEN from " + args[1], null, null, false, false);
					}
					byte b;
					if (!byte.TryParse(args[2], out b))
					{
						DebugConsole.ThrowError("Failed to parse value for BLUE from " + args[2], null, null, false, false);
					}
					Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color((int)r, (int)g, (int)b);
					if (args.Length > 3)
					{
						byte a;
						if (!byte.TryParse(args[3], out a))
						{
							DebugConsole.ThrowError("Failed to parse value for ALPHA from " + args[3], null, null, false, false);
						}
						else
						{
							color.A = a;
						}
					}
					foreach (MapEntity mapEntity in MapEntity.SelectedList)
					{
						Structure s = mapEntity as Structure;
						if (s != null)
						{
							s.SpriteColor = color;
						}
						else
						{
							Item i = mapEntity as Item;
							if (i != null)
							{
								i.SpriteColor = color;
							}
						}
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("listcloudfiles", "Lists all of your files on the Steam Cloud.", delegate(string[] args)
			{
				int i = 0;
				foreach (SteamRemoteStorage.RemoteFile file in SteamRemoteStorage.Files)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 3);
					defaultInterpolatedStringHandler.AppendLiteral("* ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendFormatted(file.Filename);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(file.Size);
					defaultInterpolatedStringHandler.AppendLiteral(" bytes");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
					i++;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Bytes remaining: ");
				defaultInterpolatedStringHandler2.AppendFormatted<ulong>(SteamRemoteStorage.QuotaRemainingBytes);
				defaultInterpolatedStringHandler2.AppendLiteral("/");
				defaultInterpolatedStringHandler2.AppendFormatted<ulong>(SteamRemoteStorage.QuotaBytes);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("removefromcloud", "Removes a file from Steam Cloud.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				List<SteamRemoteStorage.RemoteFile> files = SteamRemoteStorage.Files;
				int index;
				SteamRemoteStorage.RemoteFile file;
				if (int.TryParse(args[0], out index) && index >= 0 && index < files.Count)
				{
					file = files[index];
				}
				else
				{
					file = files.Find((SteamRemoteStorage.RemoteFile f) => f.Filename.Equals(args[0], StringComparison.InvariantCultureIgnoreCase));
				}
				if (!string.IsNullOrEmpty(file.Filename))
				{
					if (file.Delete())
					{
						DebugConsole.NewMessage("Deleting " + file.Filename, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
						return;
					}
					DebugConsole.ThrowError("Failed to delete " + file.Filename, null, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("resetall", "Reset all items and structures to prefabs. Only applicable in the subeditor.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					Item.ItemList.ForEach(delegate(Item i)
					{
						i.Reset();
					});
					Structure.WallList.ForEach(delegate(Structure s)
					{
						s.Reset();
					});
					foreach (MapEntity entity in MapEntity.SelectedList)
					{
						Item item = entity as Item;
						if (item != null)
						{
							item.CreateEditingHUD(false);
							break;
						}
						Structure structure = entity as Structure;
						if (structure != null)
						{
							structure.CreateEditingHUD(false);
							break;
						}
					}
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("resetentitiesbyidentifier", "resetentitiesbyidentifier [tag/identifier]: Reset items and structures with the given tag/identifier to prefabs. Only applicable in the subeditor.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					bool entityFound = false;
					foreach (MapEntity entity in MapEntity.MapEntityList)
					{
						Item item = entity as Item;
						if (item != null)
						{
							if (item.Prefab.Identifier != args[0] && !item.Tags.Contains(args[0]))
							{
								continue;
							}
							item.Reset();
							if (MapEntity.SelectedList.Contains(item))
							{
								item.CreateEditingHUD(false);
							}
							entityFound = true;
						}
						else
						{
							Structure structure = entity as Structure;
							if (structure == null || (structure.Prefab.Identifier != args[0] && !structure.Tags.Contains(args[0])))
							{
								continue;
							}
							structure.Reset();
							if (MapEntity.SelectedList.Contains(structure))
							{
								structure.CreateEditingHUD(false);
							}
							entityFound = true;
						}
						DebugConsole.NewMessage("Reset " + entity.Name + ".", null, false);
					}
					if (!entityFound && MapEntity.SelectedList.Count == 0)
					{
						DebugConsole.NewMessage("No entities selected.", null, false);
						return;
					}
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from me in MapEntityPrefab.List
				select me.Identifier.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("resetselected", "Reset selected items and structures to prefabs. Only applicable in the subeditor.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					if (MapEntity.SelectedList.Count == 0)
					{
						DebugConsole.NewMessage("No entities selected.", null, false);
						return;
					}
					foreach (MapEntity entity in MapEntity.SelectedList)
					{
						Item item = entity as Item;
						if (item != null)
						{
							item.Reset();
						}
						else
						{
							Structure structure = entity as Structure;
							if (structure == null)
							{
								continue;
							}
							structure.Reset();
						}
						DebugConsole.NewMessage("Reset " + entity.Name + ".", null, false);
					}
					foreach (MapEntity entity2 in MapEntity.SelectedList)
					{
						Item item2 = entity2 as Item;
						if (item2 != null)
						{
							item2.CreateEditingHUD(false);
							break;
						}
						Structure structure2 = entity2 as Structure;
						if (structure2 != null)
						{
							structure2.CreateEditingHUD(false);
							break;
						}
					}
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("alpha", "Change the alpha (as bytes from 0 to 255) of the selected item/structure instances. Applied only in the subeditor.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					if (!MapEntity.SelectedAny)
					{
						DebugConsole.ThrowError("You have to select item(s)/structure(s) first!", null, null, false, false);
						return;
					}
					if (args.Length != 0)
					{
						byte a;
						if (!byte.TryParse(args[0], out a))
						{
							DebugConsole.ThrowError("Failed to parse value for ALPHA from " + args[0], null, null, false, false);
							return;
						}
						using (HashSet<MapEntity>.Enumerator enumerator = MapEntity.SelectedList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								MapEntity mapEntity = enumerator.Current;
								Structure s = mapEntity as Structure;
								if (s != null)
								{
									s.SpriteColor = new Microsoft.Xna.Framework.Color(s.SpriteColor.R, s.SpriteColor.G, s.SpriteColor.G, a);
								}
								else
								{
									Item i = mapEntity as Item;
									if (i != null)
									{
										i.SpriteColor = new Microsoft.Xna.Framework.Color(i.SpriteColor.R, i.SpriteColor.G, i.SpriteColor.G, a);
									}
								}
							}
							return;
						}
					}
					DebugConsole.ThrowError("Not enough arguments provided! One required!", null, null, false, false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("cleansub", "", delegate(string[] args)
			{
				for (int i = MapEntity.MapEntityList.Count - 1; i >= 0; i--)
				{
					MapEntity me = MapEntity.MapEntityList[i];
					if (me.SimPosition.Length() > 2000f)
					{
						DebugConsole.NewMessage(string.Concat(new string[]
						{
							"Removed ",
							me.Name,
							" (simposition ",
							me.SimPosition.ToString(),
							")"
						}), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
						MapEntity.MapEntityList.RemoveAt(i);
					}
					else if (!me.ShouldBeSaved)
					{
						DebugConsole.NewMessage("Removed " + me.Name + " (!ShouldBeSaved)", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
						MapEntity.MapEntityList.RemoveAt(i);
					}
					else if (me is Item)
					{
						Item item = me as Item;
						Wire wire = item.GetComponent<Wire>();
						if (wire != null && wire.GetNodes().Count > 0)
						{
							if (!wire.Connections.Any((Barotrauma.Items.Components.Connection c) => c != null))
							{
								wire.Item.Drop(null, true, true);
								DebugConsole.NewMessage("Dropped wire (ID: " + wire.Item.ID.ToString() + ") - attached on wall but no connections found", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
							}
						}
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("messagebox|guimessagebox", "messagebox [header] [msg] [default/ingame]: Creates a message box.", delegate(string[] args)
			{
				RichString headerText = (args.Length != 0) ? args[0] : "";
				RichString text = (args.Length > 1) ? args[1] : "";
				LocalizedString[] buttons = new LocalizedString[]
				{
					"OK"
				};
				GUIMessageBox.Type type = (args.Length < 3 || args[2] == "default") ? GUIMessageBox.Type.Default : GUIMessageBox.Type.InGame;
				GUIMessageBox msgBox = new GUIMessageBox(headerText, text, buttons, null, null, Alignment.TopLeft, type, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			}, null, false));
			DebugConsole.AssignOnExecute("debugdraw", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameMain.DebugDraw;
				}
				GameMain.DebugDraw = state;
				DebugConsole.NewMessage("Debug draw mode " + (GameMain.DebugDraw ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("debugdraw", false);
			DebugConsole.AssignOnExecute("debugdrawlos", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameMain.LightManager.DebugLos;
				}
				GameMain.LightManager.DebugLos = state;
				DebugConsole.NewMessage("Los debug draw mode " + (GameMain.LightManager.DebugLos ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignOnExecute("debugwiring", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !ConnectionPanel.DebugWiringMode;
				}
				ConnectionPanel.DebugWiringMode = state;
				DebugConsole.NewMessage("Wiring debug mode " + (ConnectionPanel.DebugWiringMode ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("debugdraw", false);
			DebugConsole.AssignOnExecute("devmode", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameMain.DevMode;
				}
				GameMain.DevMode = state;
				if (GameMain.DevMode)
				{
					GameMain.LightManager.LightingEnabled = false;
					GameMain.LightManager.LosEnabled = false;
				}
				else
				{
					GameMain.LightManager.LightingEnabled = true;
					GameMain.LightManager.LosEnabled = true;
					GameMain.LightManager.LosAlpha = 1f;
				}
				DebugConsole.NewMessage("Dev mode " + (GameMain.DevMode ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("devmode", false);
			DebugConsole.AssignOnExecute("debugdrawlocalization", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !TextManager.DebugDraw;
				}
				TextManager.DebugDraw = state;
				DebugConsole.NewMessage("Localization debug draw mode " + (TextManager.DebugDraw ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("debugdraw", false);
			DebugConsole.AssignOnExecute("togglevoicechatfilters", delegate(string[] args)
			{
				bool state;
				if (args.None(null) || !bool.TryParse(args[0], out state))
				{
					state = !GameSettings.CurrentConfig.Audio.DisableVoiceChatFilters;
				}
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Audio.DisableVoiceChatFilters = state;
				GameSettings.SetCurrentConfig(config);
				DebugConsole.NewMessage("Voice chat filters " + (GameSettings.CurrentConfig.Audio.DisableVoiceChatFilters ? "disabled" : "enabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("togglevoicechatfilters", false);
			DebugConsole.commands.Add(new DebugConsole.Command("fpscounter", "fpscounter: Toggle the FPS counter.", delegate(string[] args)
			{
				GameMain.ShowFPS = !GameMain.ShowFPS;
				DebugConsole.NewMessage("FPS counter " + (GameMain.DebugDraw ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showperf", "showperf: Toggle performance statistics on/off.", delegate(string[] args)
			{
				GameMain.ShowPerf = !GameMain.ShowPerf;
				DebugConsole.NewMessage("Performance statistics " + (GameMain.ShowPerf ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, false));
			DebugConsole.AssignOnClientExecute("netstats", delegate(string[] args)
			{
				if (GameMain.Client == null)
				{
					return;
				}
				GameMain.Client.ShowNetStats = !GameMain.Client.ShowNetStats;
			});
			DebugConsole.commands.Add(new DebugConsole.Command("hudlayoutdebugdraw|debugdrawhudlayout", "hudlayoutdebugdraw: Toggle the debug drawing mode of HUD layout areas on/off.", delegate(string[] args)
			{
				HUDLayoutSettings.DebugDraw = !HUDLayoutSettings.DebugDraw;
				DebugConsole.NewMessage("HUD layout debug draw mode " + (HUDLayoutSettings.DebugDraw ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("interactdebugdraw|debugdrawinteract", "interactdebugdraw: Toggle the debug drawing mode of item interaction ranges on/off.", delegate(string[] args)
			{
				Character.DebugDrawInteract = !Character.DebugDrawInteract;
				DebugConsole.NewMessage("Interact debug draw mode " + (Character.DebugDrawInteract ? "enabled" : "disabled"), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			}, null, true));
			DebugConsole.AssignOnExecute("togglehud|hud", delegate(string[] args)
			{
				GUI.DisableHUD = !GUI.DisableHUD;
				GameMain.Instance.IsMouseVisible = !GameMain.Instance.IsMouseVisible;
				DebugConsole.NewMessage(GUI.DisableHUD ? "Disabled HUD" : "Enabled HUD", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("togglehud|hud", false);
			DebugConsole.AssignOnExecute("toggleupperhud", delegate(string[] args)
			{
				GUI.DisableUpperHUD = !GUI.DisableUpperHUD;
				DebugConsole.NewMessage(GUI.DisableUpperHUD ? "Disabled upper HUD" : "Enabled upper HUD", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("toggleupperhud", false);
			DebugConsole.AssignOnExecute("toggleitemhighlights", delegate(string[] args)
			{
				GUI.DisableItemHighlights = !GUI.DisableItemHighlights;
				DebugConsole.NewMessage(GUI.DisableItemHighlights ? "Disabled item highlights" : "Enabled item highlights", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("toggleitemhighlights", false);
			DebugConsole.AssignOnExecute("togglecharacternames", delegate(string[] args)
			{
				GUI.DisableCharacterNames = !GUI.DisableCharacterNames;
				DebugConsole.NewMessage(GUI.DisableCharacterNames ? "Disabled character names" : "Enabled character names", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("togglecharacternames", false);
			DebugConsole.AssignOnExecute("followsub", delegate(string[] args)
			{
				Camera.FollowSub = !Camera.FollowSub;
				DebugConsole.NewMessage(Camera.FollowSub ? "Set the camera to follow the closest submarine" : "Disabled submarine following.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("followsub", false);
			DebugConsole.AssignOnExecute("toggleaitargets|aitargets", delegate(string[] args)
			{
				AITarget.ShowAITargets = !AITarget.ShowAITargets;
				DebugConsole.NewMessage(AITarget.ShowAITargets ? "Enabled AI target drawing" : "Disabled AI target drawing", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("toggleaitargets|aitargets", false);
			DebugConsole.AssignOnExecute("debugai", delegate(string[] args)
			{
				HumanAIController.DebugAI = !HumanAIController.DebugAI;
				if (HumanAIController.DebugAI)
				{
					GameMain.DevMode = true;
					GameMain.DebugDraw = true;
					GameMain.LightManager.LightingEnabled = false;
					GameMain.LightManager.LosEnabled = false;
				}
				else
				{
					GameMain.DevMode = false;
					GameMain.DebugDraw = false;
					GameMain.LightManager.LightingEnabled = true;
					GameMain.LightManager.LosEnabled = true;
					GameMain.LightManager.LosAlpha = 1f;
				}
				DebugConsole.NewMessage(HumanAIController.DebugAI ? "AI debug info visible" : "AI debug info hidden", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
			});
			DebugConsole.AssignRelayToServer("debugai", false);
			DebugConsole.AssignOnExecute("showmonsters", delegate(string[] args)
			{
				CreatureMetrics.UnlockAll = true;
				CreatureMetrics.Save();
				DebugConsole.NewMessage("All monsters are now visible in the character editor.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
				if (Screen.Selected == GameMain.CharacterEditorScreen)
				{
					GameMain.CharacterEditorScreen.Deselect();
					GameMain.CharacterEditorScreen.Select();
				}
			});
			DebugConsole.AssignRelayToServer("showmonsters", false);
			DebugConsole.AssignOnExecute("hidemonsters", delegate(string[] args)
			{
				CreatureMetrics.UnlockAll = false;
				CreatureMetrics.Save();
				DebugConsole.NewMessage("All monsters that haven't yet been encountered in the game are now hidden in the character editor.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
				if (Screen.Selected == GameMain.CharacterEditorScreen)
				{
					GameMain.CharacterEditorScreen.Deselect();
					GameMain.CharacterEditorScreen.Select();
				}
			});
			DebugConsole.AssignRelayToServer("hidemonsters", false);
			DebugConsole.AssignRelayToServer("water|editwater", false);
			DebugConsole.AssignRelayToServer("fire|editfire", false);
			DebugConsole.commands.Add(new DebugConsole.Command("mute", "mute [name]: Prevent the client from speaking to anyone through the voice chat. Using this command requires a permission from the server host.", null, delegate()
			{
				if (GameMain.Client == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.Client.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("unmute", "unmute [name]: Allow the client to speak to anyone through the voice chat. Using this command requires a permission from the server host.", null, delegate()
			{
				if (GameMain.Client == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.Client.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("checkcrafting", "checkcrafting: Checks item deconstruction & crafting recipes for inconsistencies.", delegate(string[] args)
			{
				List<FabricationRecipe> fabricableItems = new List<FabricationRecipe>();
				foreach (ItemPrefab itemPrefab3 in ItemPrefab.Prefabs)
				{
					fabricableItems.AddRange(itemPrefab3.FabricationRecipes.Values);
				}
				using (IEnumerator<ItemPrefab> enumerator2 = ItemPrefab.Prefabs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ItemPrefab itemPrefab = enumerator2.Current;
						int? minCost = itemPrefab.GetMinPrice();
						int? fabricationCost = null;
						int? deconstructProductCost = null;
						FabricationRecipe fabricationRecipe = fabricableItems.Find((FabricationRecipe f) => f.TargetItem == itemPrefab && f.RequiredItems.Any<FabricationRecipe.RequiredItem>());
						if (fabricationRecipe != null)
						{
							foreach (FabricationRecipe.RequiredItem ingredient in fabricationRecipe.RequiredItems)
							{
								int? ingredientPrice = ingredient.ItemPrefabs.Min((ItemPrefab ip) => ip.GetMinPrice());
								if (ingredientPrice != null)
								{
									if (fabricationCost == null)
									{
										fabricationCost = new int?(0);
									}
									float useAmount = ingredient.UseCondition ? ingredient.MinCondition : 1f;
									fabricationCost += (int)((float)(ingredientPrice.Value * ingredient.Amount) * useAmount);
								}
							}
						}
						ImmutableArray<DeconstructItem>.Enumerator enumerator4 = itemPrefab.DeconstructItems.GetEnumerator();
						while (enumerator4.MoveNext())
						{
							DeconstructItem deconstructItem = enumerator4.Current;
							MapEntityPrefab mapEntityPrefab = MapEntityPrefab.Find(null, deconstructItem.ItemIdentifier, false);
							ItemPrefab targetItem = mapEntityPrefab as ItemPrefab;
							if (targetItem == null)
							{
								DebugConsole.ThrowErrorLocalized("Error in item \"" + itemPrefab.Name + "\" - could not find deconstruct item \"" + deconstructItem.ItemIdentifier + "\"!", null, null, false, false);
							}
							else
							{
								float avgOutCondition = (deconstructItem.OutConditionMin + deconstructItem.OutConditionMax) / 2f;
								int? deconstructProductPrice = targetItem.GetMinPrice();
								if (deconstructProductPrice != null)
								{
									if (deconstructProductCost == null)
									{
										deconstructProductCost = new int?(0);
									}
									int? num = deconstructProductCost;
									int? num2 = deconstructProductPrice;
									deconstructProductCost = num + (int)(((num2 != null) ? new float?((float)num2.GetValueOrDefault()) : null) * avgOutCondition).Value;
								}
								if (fabricationRecipe != null)
								{
									FabricationRecipe.RequiredItem ingredient2 = fabricationRecipe.RequiredItems.Find((FabricationRecipe.RequiredItem r) => r.ItemPrefabs.Contains(targetItem));
									if (ingredient2 == null)
									{
										Func<FabricationRecipe.RequiredItem, bool> <>9__180;
										foreach (FabricationRecipe.RequiredItem requiredItem in fabricationRecipe.RequiredItems)
										{
											foreach (ItemPrefab itemPrefab2 in requiredItem.ItemPrefabs)
											{
												foreach (FabricationRecipe recipe in itemPrefab2.FabricationRecipes.Values)
												{
													if (ingredient2 == null)
													{
														IReadOnlyList<FabricationRecipe.RequiredItem> list = recipe.RequiredItems;
														Func<FabricationRecipe.RequiredItem, bool> predicate;
														if ((predicate = <>9__180) == null)
														{
															predicate = (<>9__180 = ((FabricationRecipe.RequiredItem r) => r.ItemPrefabs.Contains(targetItem)));
														}
														ingredient2 = list.Find(predicate);
													}
												}
											}
										}
									}
									if (ingredient2 == null)
									{
										DebugConsole.NewMessage("Deconstructing \"" + itemPrefab.Name + "\" produces \"" + deconstructItem.ItemIdentifier + "\", which isn't required in the fabrication recipe of the item.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
									}
									else if (ingredient2.UseCondition && ingredient2.MinCondition < avgOutCondition)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 6);
										defaultInterpolatedStringHandler.AppendLiteral("Deconstructing \"");
										defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
										defaultInterpolatedStringHandler.AppendLiteral("\" produces more \"");
										defaultInterpolatedStringHandler.AppendFormatted<Identifier>(deconstructItem.ItemIdentifier);
										defaultInterpolatedStringHandler.AppendLiteral("\", than what's required to fabricate the item (required: ");
										defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(targetItem.Name);
										defaultInterpolatedStringHandler.AppendLiteral(" ");
										defaultInterpolatedStringHandler.AppendFormatted<int>((int)(ingredient2.MinCondition * 100f));
										defaultInterpolatedStringHandler.AppendLiteral("%, output: ");
										defaultInterpolatedStringHandler.AppendFormatted<Identifier>(deconstructItem.ItemIdentifier);
										defaultInterpolatedStringHandler.AppendLiteral(" ");
										defaultInterpolatedStringHandler.AppendFormatted<int>((int)(avgOutCondition * 100f));
										defaultInterpolatedStringHandler.AppendLiteral("%)");
										DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
									}
								}
							}
						}
						if (fabricationCost != null && minCost != null)
						{
							float num3 = (float)fabricationCost.Value;
							int? num = minCost;
							float? num4 = (num != null) ? new float?((float)num.GetValueOrDefault() * 0.9f) : null;
							if (num3 < num4.GetValueOrDefault() & num4 != null)
							{
								float ratio = (float)fabricationCost.Value / (float)minCost.Value;
								Microsoft.Xna.Framework.Color color = ToolBox.GradientLerp(ratio, new Microsoft.Xna.Framework.Color[]
								{
									Microsoft.Xna.Framework.Color.Red,
									Microsoft.Xna.Framework.Color.Yellow,
									Microsoft.Xna.Framework.Color.Green
								});
								DebugConsole.NewMessage("The fabrication ingredients of \"" + itemPrefab.Name + "\" only cost " + (int)(ratio * 100f) + "% of the price of the item. Item price: " + minCost.Value + ", ingredient prices: " + fabricationCost.Value, new Microsoft.Xna.Framework.Color?(color), false);
							}
							else
							{
								float num5 = (float)fabricationCost.Value;
								num = minCost;
								num4 = ((num != null) ? new float?((float)num.GetValueOrDefault() * 1.1f) : null);
								if (num5 > num4.GetValueOrDefault() & num4 != null)
								{
									float ratio2 = (float)fabricationCost.Value / (float)minCost.Value;
									Microsoft.Xna.Framework.Color color2 = ToolBox.GradientLerp(ratio2 - 1f, new Microsoft.Xna.Framework.Color[]
									{
										Microsoft.Xna.Framework.Color.Green,
										Microsoft.Xna.Framework.Color.Yellow,
										Microsoft.Xna.Framework.Color.Red
									});
									DebugConsole.NewMessage("The fabrication ingredients of \"" + itemPrefab.Name + "\" cost " + (int)(ratio2 * 100f - 100f) + "% more than the price of the item. Item price: " + minCost.Value + ", ingredient prices: " + fabricationCost.Value, new Microsoft.Xna.Framework.Color?(color2), false);
								}
							}
						}
						if (deconstructProductCost != null && minCost != null)
						{
							float num6 = (float)deconstructProductCost.Value;
							int? num = minCost;
							float? num4 = (num != null) ? new float?((float)num.GetValueOrDefault() * 0.8f) : null;
							if (num6 < num4.GetValueOrDefault() & num4 != null)
							{
								float ratio3 = (float)deconstructProductCost.Value / (float)minCost.Value;
								Microsoft.Xna.Framework.Color color3 = ToolBox.GradientLerp(ratio3, new Microsoft.Xna.Framework.Color[]
								{
									Microsoft.Xna.Framework.Color.Red,
									Microsoft.Xna.Framework.Color.Yellow,
									Microsoft.Xna.Framework.Color.Green
								});
								DebugConsole.NewMessage("The deconstruction output of \"" + itemPrefab.Name + "\" is only worth " + (int)(ratio3 * 100f) + "% of the price of the item. Item price: " + minCost.Value + ", output value: " + deconstructProductCost.Value, new Microsoft.Xna.Framework.Color?(color3), false);
							}
							else
							{
								float num7 = (float)deconstructProductCost.Value;
								num = minCost;
								num4 = ((num != null) ? new float?((float)num.GetValueOrDefault() * 1.1f) : null);
								if (num7 > num4.GetValueOrDefault() & num4 != null)
								{
									float ratio4 = (float)deconstructProductCost.Value / (float)minCost.Value;
									Microsoft.Xna.Framework.Color color4 = ToolBox.GradientLerp(ratio4 - 1f, new Microsoft.Xna.Framework.Color[]
									{
										Microsoft.Xna.Framework.Color.Green,
										Microsoft.Xna.Framework.Color.Yellow,
										Microsoft.Xna.Framework.Color.Red
									});
									DebugConsole.NewMessage("The deconstruction output of \"" + itemPrefab.Name + "\" is worth " + (int)(ratio4 * 100f - 100f) + "% more than the price of the item. Item price: " + minCost.Value + ", output value: " + deconstructProductCost.Value, new Microsoft.Xna.Framework.Color?(color4), false);
								}
							}
						}
					}
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("analyzeitem", "analyzeitem: Analyzes one item for exploits.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				List<FabricationRecipe> fabricableItems = new List<FabricationRecipe>();
				foreach (ItemPrefab iPrefab in ItemPrefab.Prefabs)
				{
					fabricableItems.AddRange(iPrefab.FabricationRecipes.Values);
				}
				string itemNameOrId = args[0].ToLowerInvariant();
				ItemPrefab itemPrefab = (MapEntityPrefab.FindByName(itemNameOrId) ?? MapEntityPrefab.FindByIdentifier(itemNameOrId.ToIdentifier())) as ItemPrefab;
				if (itemPrefab == null)
				{
					DebugConsole.NewMessage("Item not found for analyzing.", null, false);
					return;
				}
				if (itemPrefab.DefaultPrice == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Item \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" is not sellable/purchaseable.");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					return;
				}
				DebugConsole.NewMessage("Analyzing item " + itemPrefab.Name + " with base cost " + itemPrefab.DefaultPrice.Price, null, false);
				FabricationRecipe fabricationRecipe = fabricableItems.Find((FabricationRecipe f) => f.TargetItem == itemPrefab);
				if (fabricationRecipe != null)
				{
					foreach (KeyValuePair<Identifier, PriceInfo> priceInfo in itemPrefab.GetSellPricesOver(0, true))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("    If bought at ");
						defaultInterpolatedStringHandler2.AppendFormatted(DebugConsole.<InitProjectSpecific>g__GetSeller|23_182(priceInfo.Value));
						defaultInterpolatedStringHandler2.AppendLiteral(" it costs ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(priceInfo.Value.Price);
						DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
						int totalPrice = 0;
						int? totalBestPrice = new int?(0);
						foreach (FabricationRecipe.RequiredItem ingredient in fabricationRecipe.RequiredItems)
						{
							foreach (ItemPrefab ingredientItemPrefab in ingredient.ItemPrefabs)
							{
								PriceInfo defaultPrice2 = ingredientItemPrefab.DefaultPrice;
								int defaultPrice = (defaultPrice2 != null) ? defaultPrice2.Price : 0;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(38, 2);
								defaultInterpolatedStringHandler3.AppendLiteral("        Its ingredient ");
								defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(ingredientItemPrefab.Name);
								defaultInterpolatedStringHandler3.AppendLiteral(" has base cost ");
								defaultInterpolatedStringHandler3.AppendFormatted<int>(defaultPrice);
								DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, false);
								totalPrice += defaultPrice;
								totalBestPrice += ingredientItemPrefab.GetMinPrice();
								int basePrice = defaultPrice;
								foreach (KeyValuePair<Identifier, PriceInfo> ingredientItemPriceInfo in ingredientItemPrefab.GetBuyPricesUnder(0))
								{
									if (basePrice > ingredientItemPriceInfo.Value.Price)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(44, 3);
										defaultInterpolatedStringHandler4.AppendLiteral("            ");
										defaultInterpolatedStringHandler4.AppendFormatted(DebugConsole.<InitProjectSpecific>g__GetSeller|23_182(ingredientItemPriceInfo.Value).CapitaliseFirstInvariant());
										defaultInterpolatedStringHandler4.AppendLiteral(" sells ingredient ");
										defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(ingredientItemPrefab.Name);
										defaultInterpolatedStringHandler4.AppendLiteral(" for cheaper, ");
										defaultInterpolatedStringHandler4.AppendFormatted<int>(ingredientItemPriceInfo.Value.Price);
										DebugConsole.NewMessage(defaultInterpolatedStringHandler4.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
									}
									else
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(41, 3);
										defaultInterpolatedStringHandler5.AppendLiteral("            ");
										defaultInterpolatedStringHandler5.AppendFormatted(DebugConsole.<InitProjectSpecific>g__GetSeller|23_182(ingredientItemPriceInfo.Value).CapitaliseFirstInvariant());
										defaultInterpolatedStringHandler5.AppendLiteral(" sells ingredient ");
										defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(ingredientItemPrefab.Name);
										defaultInterpolatedStringHandler5.AppendLiteral(" for more, ");
										defaultInterpolatedStringHandler5.AppendFormatted<int>(ingredientItemPriceInfo.Value.Price);
										DebugConsole.NewMessage(defaultInterpolatedStringHandler5.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Teal), false);
									}
								}
							}
						}
						int costDifference = itemPrefab.DefaultPrice.Price - totalPrice;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(87, 1);
						defaultInterpolatedStringHandler6.AppendLiteral("    Constructing the item from store-bought items provides ");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(costDifference);
						defaultInterpolatedStringHandler6.AppendLiteral(" profit with default values.");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler6.ToStringAndClear(), null, false);
						if (totalBestPrice != null)
						{
							int? bestDifference = priceInfo.Value.Price - totalBestPrice;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(98, 1);
							defaultInterpolatedStringHandler7.AppendLiteral("    Constructing the item from store-bought items provides ");
							defaultInterpolatedStringHandler7.AppendFormatted<int?>(bestDifference);
							defaultInterpolatedStringHandler7.AppendLiteral(" profit with best-case scenario values.");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler7.ToStringAndClear(), null, false);
						}
					}
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = ItemPrefab.Prefabs.SelectMany((ItemPrefab p) => p.Aliases).Concat(from p in ItemPrefab.Prefabs
				select p.Identifier.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("checkcraftingexploits", "checkcraftingexploits: Finds outright item exploits created by buying store-bought ingredients and constructing them into sellable items.", delegate(string[] args)
			{
				List<FabricationRecipe> fabricableItems = new List<FabricationRecipe>();
				foreach (ItemPrefab itemPrefab2 in ItemPrefab.Prefabs)
				{
					fabricableItems.AddRange(itemPrefab2.FabricationRecipes.Values);
				}
				List<Tuple<string, int>> costDifferences = new List<Tuple<string, int>>();
				int maximumAllowedCost = 5;
				if (args.Length != 0)
				{
					int.TryParse(args[0], out maximumAllowedCost);
				}
				using (IEnumerator<ItemPrefab> enumerator2 = ItemPrefab.Prefabs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ItemPrefab itemPrefab = enumerator2.Current;
						PriceInfo defaultPrice = itemPrefab.DefaultPrice;
						int? defaultCost = (defaultPrice != null) ? new int?(defaultPrice.Price) : null;
						int? fabricationCostStore = null;
						FabricationRecipe fabricationRecipe = fabricableItems.Find((FabricationRecipe f) => f.TargetItem == itemPrefab);
						if (fabricationRecipe != null)
						{
							bool canBeBought = true;
							foreach (FabricationRecipe.RequiredItem ingredient in fabricationRecipe.RequiredItems)
							{
								int? ingredientPrice = (from p in ingredient.ItemPrefabs
								where p.CanBeBought
								select p).Min(delegate(ItemPrefab ip)
								{
									PriceInfo defaultPrice2 = ip.DefaultPrice;
									if (defaultPrice2 == null)
									{
										return null;
									}
									return new int?(defaultPrice2.Price);
								});
								if (ingredientPrice != null)
								{
									if (fabricationCostStore == null)
									{
										fabricationCostStore = new int?(0);
									}
									float useAmount = ingredient.UseCondition ? ingredient.MinCondition : 1f;
									fabricationCostStore += (int)((float)(ingredientPrice.Value * ingredient.Amount) * useAmount);
								}
								else
								{
									canBeBought = false;
								}
							}
							if (fabricationCostStore != null && defaultCost != null && canBeBought)
							{
								int costDifference = defaultCost.Value - fabricationCostStore.Value;
								if (costDifference > maximumAllowedCost || (float)costDifference < 0f)
								{
									float ratio = (float)fabricationCostStore.Value / (float)defaultCost.Value;
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 5);
									defaultInterpolatedStringHandler.AppendLiteral("Fabricating \"");
									defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(itemPrefab.Name);
									defaultInterpolatedStringHandler.AppendLiteral("\" costs ");
									defaultInterpolatedStringHandler.AppendFormatted<int>((int)(ratio * 100f));
									defaultInterpolatedStringHandler.AppendLiteral("% of the price of the item, or ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(costDifference);
									defaultInterpolatedStringHandler.AppendLiteral(" more. Item price: ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(defaultCost.Value);
									defaultInterpolatedStringHandler.AppendLiteral(", ingredient prices: ");
									defaultInterpolatedStringHandler.AppendFormatted<int>(fabricationCostStore.Value);
									string message = defaultInterpolatedStringHandler.ToStringAndClear();
									costDifferences.Add(new Tuple<string, int>(message, costDifference));
								}
							}
						}
					}
				}
				costDifferences.Sort((Tuple<string, int> x, Tuple<string, int> y) => x.Item2.CompareTo(y.Item2));
				foreach (Tuple<string, int> costDifference2 in costDifferences)
				{
					Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Yellow;
					DebugConsole.NewMessage(costDifference2.Item1, new Microsoft.Xna.Framework.Color?(color), false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("adjustprice", "adjustprice: Recursively prints out expected price adjustments for items derived from this item.", delegate(string[] args)
			{
				List<FabricationRecipe> fabricableItems = new List<FabricationRecipe>();
				foreach (ItemPrefab iP in ItemPrefab.Prefabs)
				{
					fabricableItems.AddRange(iP.FabricationRecipes.Values);
				}
				if (args.Length < 2)
				{
					DebugConsole.NewMessage("Item or value not defined.", null, false);
					return;
				}
				string itemNameOrId = args[0].ToLowerInvariant();
				ItemPrefab materialPrefab = (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as ItemPrefab;
				if (materialPrefab == null)
				{
					DebugConsole.NewMessage("Item not found for price adjustment.", null, false);
					return;
				}
				DebugConsole.AdjustItemTypes adjustItemType = DebugConsole.AdjustItemTypes.NoAdjustment;
				if (args.Length > 2)
				{
					string a = args[2].ToLowerInvariant();
					if (!(a == "add"))
					{
						if (a == "mult")
						{
							adjustItemType = DebugConsole.AdjustItemTypes.Multiplicative;
						}
					}
					else
					{
						adjustItemType = DebugConsole.AdjustItemTypes.Additive;
					}
				}
				int newPrice;
				if (int.TryParse(args[1].ToLowerInvariant(), out newPrice))
				{
					Dictionary<ItemPrefab, int> newPrices = new Dictionary<ItemPrefab, int>();
					DebugConsole.PrintItemCosts(newPrices, materialPrefab, fabricableItems, newPrice, true, "", adjustItemType);
					DebugConsole.PrintItemCosts(newPrices, materialPrefab, fabricableItems, newPrice, false, "", adjustItemType);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("deconstructvalue", "deconstructvalue: Views and compares deconstructed component prices for this item.", delegate(string[] args)
			{
				List<FabricationRecipe> fabricableItems = new List<FabricationRecipe>();
				foreach (ItemPrefab iP in ItemPrefab.Prefabs)
				{
					fabricableItems.AddRange(iP.FabricationRecipes.Values);
				}
				if (args.Length < 1)
				{
					DebugConsole.NewMessage("Item not defined.", null, false);
					return;
				}
				string itemNameOrId = args[0].ToLowerInvariant();
				ItemPrefab parentItem = (MapEntityPrefab.Find(itemNameOrId, null, false) ?? MapEntityPrefab.Find(null, itemNameOrId, false)) as ItemPrefab;
				if (parentItem == null)
				{
					DebugConsole.NewMessage("Item not found for price adjustment.", null, false);
					return;
				}
				FabricationRecipe fabricationRecipe = fabricableItems.Find((FabricationRecipe f) => f.TargetItem == parentItem);
				int totalValue = 0;
				LocalizedString left = parentItem.Name + " has the price ";
				PriceInfo defaultPrice3 = parentItem.DefaultPrice;
				DebugConsole.NewMessage(left + ((defaultPrice3 != null) ? defaultPrice3.Price : 0), null, false);
				if (fabricationRecipe != null)
				{
					DebugConsole.NewMessage("    It constructs from:", null, false);
					foreach (FabricationRecipe.RequiredItem requiredItem in fabricationRecipe.RequiredItems)
					{
						foreach (ItemPrefab itemPrefab in requiredItem.ItemPrefabs)
						{
							PriceInfo defaultPrice4 = itemPrefab.DefaultPrice;
							int defaultPrice = (defaultPrice4 != null) ? defaultPrice4.Price : 0;
							DebugConsole.NewMessage("        " + itemPrefab.Name + " has the price " + defaultPrice, null, false);
							totalValue += defaultPrice;
						}
					}
					DebugConsole.NewMessage("Its total value was: " + totalValue.ToString(), null, false);
					totalValue = 0;
				}
				DebugConsole.NewMessage("    The item deconstructs into:", null, false);
				foreach (DeconstructItem deconstructItem in parentItem.DeconstructItems)
				{
					ItemPrefab itemPrefab2 = (MapEntityPrefab.Find(deconstructItem.ItemIdentifier.Value, null, false) ?? MapEntityPrefab.Find(null, deconstructItem.ItemIdentifier, false)) as ItemPrefab;
					if (itemPrefab2 == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
						defaultInterpolatedStringHandler.AppendLiteral("       Couldn't find deconstruct product \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(deconstructItem.ItemIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral("\"!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						PriceInfo defaultPrice5 = itemPrefab2.DefaultPrice;
						int defaultPrice2 = (defaultPrice5 != null) ? defaultPrice5.Price : 0;
						DebugConsole.NewMessage("       " + itemPrefab2.Name + " has the price " + defaultPrice2, null, false);
						totalValue += defaultPrice2;
					}
				}
				DebugConsole.NewMessage("Its deconstruct value was: " + totalValue.ToString(), null, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setentityproperties", "setentityproperties [property name] [value]: Sets the value of some property on all selected items/structures in the sub editor.", delegate(string[] args)
			{
				if (args.Length != 2 || Screen.Selected != GameMain.SubEditorScreen)
				{
					return;
				}
				foreach (MapEntity me in MapEntity.SelectedList)
				{
					bool propertyFound = false;
					ISerializableEntity serializableEntity = me as ISerializableEntity;
					if (serializableEntity != null && serializableEntity.SerializableProperties != null)
					{
						SerializableProperty property;
						if (serializableEntity.SerializableProperties.TryGetValue(args[0].ToIdentifier(), out property))
						{
							propertyFound = true;
							object prevValue = property.GetValue(me);
							if (property.TrySetValue(me, args[1]))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 4);
								defaultInterpolatedStringHandler.AppendLiteral("Changed the value \"");
								defaultInterpolatedStringHandler.AppendFormatted(args[0]);
								defaultInterpolatedStringHandler.AppendLiteral("\" from ");
								defaultInterpolatedStringHandler.AppendFormatted(((prevValue != null) ? prevValue.ToString() : null) ?? null);
								defaultInterpolatedStringHandler.AppendLiteral(" to ");
								defaultInterpolatedStringHandler.AppendFormatted(args[1]);
								defaultInterpolatedStringHandler.AppendLiteral(" on entity \"");
								defaultInterpolatedStringHandler.AppendFormatted(me.ToString());
								defaultInterpolatedStringHandler.AppendLiteral("\".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.LightGreen), false);
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(53, 3);
								defaultInterpolatedStringHandler2.AppendLiteral("Failed to set the value of \"");
								defaultInterpolatedStringHandler2.AppendFormatted(args[0]);
								defaultInterpolatedStringHandler2.AppendLiteral("\" to \"");
								defaultInterpolatedStringHandler2.AppendFormatted(args[1]);
								defaultInterpolatedStringHandler2.AppendLiteral("\" on the entity \"");
								defaultInterpolatedStringHandler2.AppendFormatted(me.ToString());
								defaultInterpolatedStringHandler2.AppendLiteral("\".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
							}
						}
						Item item = me as Item;
						if (item != null)
						{
							foreach (ItemComponent ic in item.Components)
							{
								SerializableProperty componentProperty;
								ic.SerializableProperties.TryGetValue(args[0].ToIdentifier(), out componentProperty);
								if (componentProperty != null)
								{
									propertyFound = true;
									object prevValue2 = componentProperty.GetValue(ic);
									if (componentProperty.TrySetValue(ic, args[1]))
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(56, 5);
										defaultInterpolatedStringHandler3.AppendLiteral("Changed the value \"");
										defaultInterpolatedStringHandler3.AppendFormatted(args[0]);
										defaultInterpolatedStringHandler3.AppendLiteral("\" from ");
										defaultInterpolatedStringHandler3.AppendFormatted<object>(prevValue2);
										defaultInterpolatedStringHandler3.AppendLiteral(" to ");
										defaultInterpolatedStringHandler3.AppendFormatted(args[1]);
										defaultInterpolatedStringHandler3.AppendLiteral(" on item \"");
										defaultInterpolatedStringHandler3.AppendFormatted(me.ToString());
										defaultInterpolatedStringHandler3.AppendLiteral("\", component \"");
										defaultInterpolatedStringHandler3.AppendFormatted(ic.GetType().Name);
										defaultInterpolatedStringHandler3.AppendLiteral("\".");
										DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.LightGreen), false);
									}
									else
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(65, 4);
										defaultInterpolatedStringHandler4.AppendLiteral("Failed to set the value of \"");
										defaultInterpolatedStringHandler4.AppendFormatted(args[0]);
										defaultInterpolatedStringHandler4.AppendLiteral("\" to \"");
										defaultInterpolatedStringHandler4.AppendFormatted(args[1]);
										defaultInterpolatedStringHandler4.AppendLiteral("\" on the item \"");
										defaultInterpolatedStringHandler4.AppendFormatted(me.ToString());
										defaultInterpolatedStringHandler4.AppendLiteral("\", component \"");
										defaultInterpolatedStringHandler4.AppendFormatted(ic.GetType().Name);
										defaultInterpolatedStringHandler4.AppendLiteral("\".");
										DebugConsole.NewMessage(defaultInterpolatedStringHandler4.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
									}
								}
							}
						}
						if (!propertyFound)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(39, 2);
							defaultInterpolatedStringHandler5.AppendLiteral("Property \"");
							defaultInterpolatedStringHandler5.AppendFormatted(args[0]);
							defaultInterpolatedStringHandler5.AppendLiteral("\" not found in the entity \"");
							defaultInterpolatedStringHandler5.AppendFormatted(me.ToString());
							defaultInterpolatedStringHandler5.AppendLiteral("\".");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler5.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
						}
					}
				}
			}, delegate()
			{
				List<Identifier> propertyList = new List<Identifier>();
				foreach (MapEntity me in MapEntity.SelectedList)
				{
					ISerializableEntity serializableEntity = me as ISerializableEntity;
					if (serializableEntity != null && serializableEntity.SerializableProperties != null)
					{
						propertyList.AddRange(from p in serializableEntity.SerializableProperties
						select p.Key);
						Item item = me as Item;
						if (item != null)
						{
							foreach (ItemComponent ic in item.Components)
							{
								propertyList.AddRange(from p in ic.SerializableProperties
								select p.Key);
							}
						}
					}
				}
				string[][] array = new string[2][];
				array[0] = (from i in propertyList.Distinct<Identifier>()
				select i.Value into n
				orderby n
				select n).ToArray<string>();
				array[1] = Array.Empty<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("checkmissingloca", "", delegate(string[] args)
			{
				DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass23_13();
				CS$<>8__locals1.missingTexts = new HashSet<string>();
				CS$<>8__locals1.missingTags = new Dictionary<Identifier, HashSet<LanguageIdentifier>>();
				CS$<>8__locals1.tags = new Dictionary<LanguageIdentifier, HashSet<Identifier>>();
				foreach (LanguageIdentifier language in TextManager.AvailableLanguages)
				{
					DebugConsole.<InitProjectSpecific>g__SwapLanguage|23_194(language);
					CS$<>8__locals1.tags.Add(language, new HashSet<Identifier>(from t in TextManager.GetAllTagTextPairs()
					select t.Key));
				}
				foreach (LanguageIdentifier language3 in TextManager.AvailableLanguages)
				{
					DebugConsole.<>c__DisplayClass23_14 CS$<>8__locals2;
					CS$<>8__locals2.language = language3;
					foreach (MissionPrefab missionPrefab in MissionPrefab.Prefabs)
					{
						Identifier missionId = (missionPrefab.ConfigElement.GetAttribute("textidentifier") == null) ? missionPrefab.Identifier : missionPrefab.ConfigElement.GetAttributeIdentifier("textidentifier", Identifier.Empty);
						if (!CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains(missionPrefab.ConfigElement.GetAttributeIdentifier("name", Identifier.Empty)))
						{
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals3 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
							defaultInterpolatedStringHandler.AppendLiteral("missionname.");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals3.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
						}
						Identifier identifier = missionPrefab.Type;
						if (identifier == Tags.MissionTypeCombat)
						{
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals4 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(26, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("MissionDescriptionNeutral.");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals4.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals5 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("MissionDescription1.");
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals5.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler3.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals6 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(20, 1);
							defaultInterpolatedStringHandler4.AppendLiteral("MissionDescription2.");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals6.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler4.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals7 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(13, 1);
							defaultInterpolatedStringHandler5.AppendLiteral("MissionTeam1.");
							defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals7.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler5.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals8 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(13, 1);
							defaultInterpolatedStringHandler6.AppendLiteral("MissionTeam2.");
							defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals8.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler6.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
						}
						else
						{
							if (!CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains(missionPrefab.ConfigElement.GetAttributeIdentifier("description", Identifier.Empty)))
							{
								DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals9 = CS$<>8__locals1;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(19, 1);
								defaultInterpolatedStringHandler7.AppendLiteral("missiondescription.");
								defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(missionId);
								CS$<>8__locals9.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler7.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							}
							if (!CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains(missionPrefab.ConfigElement.GetAttributeIdentifier("successmessage", Identifier.Empty)))
							{
								DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals10 = CS$<>8__locals1;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(15, 1);
								defaultInterpolatedStringHandler8.AppendLiteral("missionsuccess.");
								defaultInterpolatedStringHandler8.AppendFormatted<Identifier>(missionId);
								CS$<>8__locals10.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler8.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							}
							if (missionPrefab.ConfigElement.GetAttribute("failuremessage") != null && !CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains(missionPrefab.ConfigElement.GetAttributeIdentifier("failuremessage", Identifier.Empty)))
							{
								DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals11 = CS$<>8__locals1;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(15, 1);
								defaultInterpolatedStringHandler9.AppendLiteral("missionfailure.");
								defaultInterpolatedStringHandler9.AppendFormatted<Identifier>(missionId);
								CS$<>8__locals11.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler9.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							}
						}
						int i = 0;
						while (i < missionPrefab.Messages.Length)
						{
							if (missionPrefab.Messages[i].IsNullOrWhiteSpace())
							{
								goto IL_442;
							}
							FallbackLString fallbackLString = missionPrefab.Messages[i] as FallbackLString;
							FallbackLString fallbackLString2 = ((fallbackLString != null) ? fallbackLString.GetLastFallback() : null) as FallbackLString;
							if (fallbackLString2 != null && !fallbackLString2.PrimaryIsLoaded)
							{
								goto IL_442;
							}
							IL_48F:
							i++;
							continue;
							IL_442:
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals12 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(15, 2);
							defaultInterpolatedStringHandler10.AppendLiteral("MissionMessage");
							defaultInterpolatedStringHandler10.AppendFormatted<int>(i);
							defaultInterpolatedStringHandler10.AppendLiteral(".");
							defaultInterpolatedStringHandler10.AppendFormatted<Identifier>(missionId);
							CS$<>8__locals12.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler10.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							goto IL_48F;
						}
					}
					foreach (EventPrefab eventPrefab in EventPrefab.Prefabs)
					{
						TraitorEventPrefab traitorEventPrefab = eventPrefab as TraitorEventPrefab;
						if (traitorEventPrefab != null)
						{
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals13 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(10, 1);
							defaultInterpolatedStringHandler11.AppendLiteral("eventname.");
							defaultInterpolatedStringHandler11.AppendFormatted<Identifier>(traitorEventPrefab.Identifier);
							CS$<>8__locals13.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler11.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
						}
					}
					foreach (Type itemComponentType in from type in typeof(ItemComponent).Assembly.GetTypes()
					where type.IsSubclassOf(typeof(ItemComponent))
					select type)
					{
						CS$<>8__locals1.<InitProjectSpecific>g__checkSerializableEntityType|199(itemComponentType, ref CS$<>8__locals2);
					}
					CS$<>8__locals1.<InitProjectSpecific>g__checkSerializableEntityType|199(typeof(Item), ref CS$<>8__locals2);
					CS$<>8__locals1.<InitProjectSpecific>g__checkSerializableEntityType|199(typeof(Hull), ref CS$<>8__locals2);
					CS$<>8__locals1.<InitProjectSpecific>g__checkSerializableEntityType|199(typeof(Structure), ref CS$<>8__locals2);
					foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
					{
						if (sub.Type == SubmarineType.Player && sub.IsVanillaSubmarine())
						{
							CS$<>8__locals1.<InitProjectSpecific>g__addIfMissing|196(("submarine.name." + sub.Name).ToIdentifier(), CS$<>8__locals2.language);
							CS$<>8__locals1.<InitProjectSpecific>g__addIfMissing|196(("submarine.description." + sub.Name).ToIdentifier(), CS$<>8__locals2.language);
						}
					}
					foreach (AfflictionPrefab affliction in AfflictionPrefab.List)
					{
						if (affliction.ShowIconThreshold <= affliction.MaxStrength || affliction.ShowIconToOthersThreshold <= affliction.MaxStrength || affliction.ShowInHealthScannerThreshold <= affliction.MaxStrength)
						{
							Identifier afflictionId = affliction.TranslationIdentifier;
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals14 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler12 = new DefaultInterpolatedStringHandler(15, 1);
							defaultInterpolatedStringHandler12.AppendLiteral("afflictionname.");
							defaultInterpolatedStringHandler12.AppendFormatted<Identifier>(afflictionId);
							CS$<>8__locals14.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler12.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
							if (affliction.Descriptions.Any<AfflictionPrefab.Description>())
							{
								using (ImmutableList<AfflictionPrefab.Description>.Enumerator enumerator8 = affliction.Descriptions.GetEnumerator())
								{
									while (enumerator8.MoveNext())
									{
										AfflictionPrefab.Description description = enumerator8.Current;
										CS$<>8__locals1.<InitProjectSpecific>g__addIfMissing|196(description.TextTag, CS$<>8__locals2.language);
									}
									continue;
								}
							}
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals15 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler13 = new DefaultInterpolatedStringHandler(22, 1);
							defaultInterpolatedStringHandler13.AppendLiteral("afflictiondescription.");
							defaultInterpolatedStringHandler13.AppendFormatted<Identifier>(afflictionId);
							CS$<>8__locals15.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler13.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
						}
					}
					foreach (TalentTree talentTree in TalentTree.JobTalentTrees)
					{
						foreach (TalentSubTree talentSubTree in talentTree.TalentSubTrees)
						{
							DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals16 = CS$<>8__locals1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler14 = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler14.AppendLiteral("talenttree.");
							defaultInterpolatedStringHandler14.AppendFormatted<Identifier>(talentSubTree.Identifier);
							CS$<>8__locals16.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler14.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
						}
					}
					foreach (TalentPrefab talent in TalentPrefab.TalentPrefabs)
					{
						DebugConsole.<>c__DisplayClass23_13 CS$<>8__locals17 = CS$<>8__locals1;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler15 = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler15.AppendLiteral("talentname.");
						defaultInterpolatedStringHandler15.AppendFormatted<Identifier>(talent.Identifier);
						CS$<>8__locals17.<InitProjectSpecific>g__addIfMissing|196(defaultInterpolatedStringHandler15.ToStringAndClear().ToIdentifier(), CS$<>8__locals2.language);
					}
					foreach (MapEntityPrefab me in MapEntityPrefab.List)
					{
						Identifier nameIdentifier = ("entityname." + me.Identifier.ToString()).ToIdentifier();
						if (!CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains(nameIdentifier) && !me.HideInMenus)
						{
							ContentXElement configElement = null;
							ItemPrefab itemPrefab = me as ItemPrefab;
							if (itemPrefab != null)
							{
								configElement = itemPrefab.ConfigElement;
							}
							else
							{
								StructurePrefab structurePrefab = me as StructurePrefab;
								if (structurePrefab != null)
								{
									configElement = structurePrefab.ConfigElement;
								}
							}
							ContentXElement contentXElement = null;
							if (configElement != contentXElement)
							{
								Identifier overrideIdentifier = configElement.GetAttributeIdentifier("nameidentifier", null);
								if (overrideIdentifier != null && CS$<>8__locals1.tags[CS$<>8__locals2.language].Contains("entityname." + overrideIdentifier.ToString()))
								{
									continue;
								}
							}
							CS$<>8__locals1.<InitProjectSpecific>g__addIfMissing|196(nameIdentifier, CS$<>8__locals2.language);
						}
					}
				}
				foreach (Identifier englishTag in CS$<>8__locals1.tags[TextManager.DefaultLanguage])
				{
					foreach (LanguageIdentifier language2 in TextManager.AvailableLanguages)
					{
						if (!(language2 == TextManager.DefaultLanguage))
						{
							CS$<>8__locals1.<InitProjectSpecific>g__addIfMissing|196(englishTag, language2);
						}
					}
				}
				List<string> lines = new List<string>
				{
					"Missing from English:"
				};
				Dictionary<string, List<string>> missingByLanguages = new Dictionary<string, List<string>>();
				List<string> missingFromEnglish = new List<string>();
				foreach (KeyValuePair<Identifier, HashSet<LanguageIdentifier>> kvp in CS$<>8__locals1.missingTags)
				{
					if (kvp.Value.Contains(TextManager.DefaultLanguage))
					{
						List<string> list = missingFromEnglish;
						Identifier identifier = kvp.Key;
						list.Add(identifier.Value);
					}
					else
					{
						string languagesStr = string.Join<LanguageIdentifier>(", ", from v in kvp.Value
						orderby v.Value.Value
						select v);
						if (!missingByLanguages.ContainsKey(languagesStr))
						{
							missingByLanguages.Add(languagesStr, new List<string>());
						}
						List<string> list2 = missingByLanguages[languagesStr];
						Identifier identifier = kvp.Key;
						list2.Add(identifier.Value);
					}
				}
				foreach (string text in from v in missingFromEnglish
				orderby v
				select v)
				{
					lines.Add(text);
				}
				foreach (KeyValuePair<string, List<string>> missingByLanguage in missingByLanguages)
				{
					lines.Add(string.Empty);
					lines.Add("Missing from " + missingByLanguage.Key);
					foreach (string text2 in from v in missingByLanguage.Value
					orderby v
					select v)
					{
						lines.Add(text2);
					}
				}
				string filePath = "missingloca.txt";
				Validation.SkipValidationInDebugBuilds = true;
				File.WriteAllLines(filePath, lines, null, true);
				Validation.SkipValidationInDebugBuilds = false;
				ToolBox.OpenFileWithShell(Path.GetFullPath(filePath));
				DebugConsole.<InitProjectSpecific>g__SwapLanguage|23_194(TextManager.DefaultLanguage);
				if (CS$<>8__locals1.missingTexts.Any<string>())
				{
					DebugConsole.ShowQuestionPrompt("Dump the property names and descriptions missing from English to a new xml file? Y/N", delegate(string option)
					{
						if (option.ToLowerInvariant() == "y")
						{
							string path = "newtexts.txt";
							Validation.SkipValidationInDebugBuilds = true;
							File.WriteAllLines(path, CS$<>8__locals1.missingTexts, null, true);
							Validation.SkipValidationInDebugBuilds = false;
							ToolBox.OpenFileWithShell(Path.GetFullPath(path));
							DebugConsole.<InitProjectSpecific>g__SwapLanguage|23_194(TextManager.DefaultLanguage);
						}
					}, null, -1);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("checkduplicateloca", "", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					DebugConsole.ThrowError("Please specify a file path.", null, null, false, false);
					return;
				}
				XDocument doc = XMLExtensions.TryLoadXml(args[0]);
				if (((doc != null) ? doc.Root : null) == null)
				{
					DebugConsole.ThrowError("Could not load the file \"" + args[0] + "\"", null, null, false, false);
					return;
				}
				List<ValueTuple<string, string>> texts = new List<ValueTuple<string, string>>();
				bool duplicatesFound = false;
				foreach (XElement element in doc.Root.Elements())
				{
					string tag = element.Name.ToString();
					string text2 = element.ElementInnerText();
					if (texts.Any(([TupleElementNames(new string[]
					{
						"tag",
						"text"
					})] ValueTuple<string, string> t) => t.Item1 == tag))
					{
						DebugConsole.ThrowError("Duplicate tag \"" + tag + "\".", null, null, false, false);
						duplicatesFound = true;
					}
				}
				if (duplicatesFound)
				{
					DebugConsole.ThrowError("Aborting, please fix duplicate tags in the file and try again.", null, null, false, false);
					return;
				}
				foreach (XElement element2 in doc.Root.Elements())
				{
					string tag2 = element2.Name.ToString();
					string text = element2.ElementInnerText();
					if (texts.Any(([TupleElementNames(new string[]
					{
						"tag",
						"text"
					})] ValueTuple<string, string> t) => t.Item2 == text))
					{
						if (tag2.StartsWith("sp."))
						{
							string[] split = tag2.Split('.', StringSplitOptions.None);
							if (split.Length > 3)
							{
								texts.RemoveAll(([TupleElementNames(new string[]
								{
									"tag",
									"text"
								})] ValueTuple<string, string> t) => t.Item2 == text);
								string newTag = "sp." + split[2] + "." + split[3];
								texts.Add(new ValueTuple<string, string>(newTag, text));
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Duplicate text \"");
								defaultInterpolatedStringHandler.AppendFormatted(tag2);
								defaultInterpolatedStringHandler.AppendLiteral("\", merging to \"");
								defaultInterpolatedStringHandler.AppendFormatted(newTag);
								defaultInterpolatedStringHandler.AppendLiteral("\".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
							}
							else
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("Duplicate text \"");
								defaultInterpolatedStringHandler2.AppendFormatted(tag2);
								defaultInterpolatedStringHandler2.AppendLiteral("\", using existing one \"");
								defaultInterpolatedStringHandler2.AppendFormatted(texts.Find(([TupleElementNames(new string[]
								{
									"tag",
									"text"
								})] ValueTuple<string, string> t) => t.Item2 == text).Item1);
								defaultInterpolatedStringHandler2.AppendLiteral("\".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
							}
						}
						else
						{
							texts.Add(new ValueTuple<string, string>(tag2, text));
							DebugConsole.ThrowError("Duplicate text \"" + tag2 + "\". Could not determine if the text can be merged with an existing one, please check it manually.", null, null, false, false);
						}
					}
					else
					{
						texts.Add(new ValueTuple<string, string>(tag2, text));
					}
				}
				string filePath = "uniquetexts.xml";
				Validation.SkipValidationInDebugBuilds = true;
				File.WriteAllLines(filePath, texts.Select(delegate([TupleElementNames(new string[]
				{
					"tag",
					"text"
				})] ValueTuple<string, string> t)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(5, 3);
					defaultInterpolatedStringHandler3.AppendLiteral("<");
					defaultInterpolatedStringHandler3.AppendFormatted(t.Item1);
					defaultInterpolatedStringHandler3.AppendLiteral(">");
					defaultInterpolatedStringHandler3.AppendFormatted(t.Item2);
					defaultInterpolatedStringHandler3.AppendLiteral("</");
					defaultInterpolatedStringHandler3.AppendFormatted(t.Item1);
					defaultInterpolatedStringHandler3.AppendLiteral(">");
					return defaultInterpolatedStringHandler3.ToStringAndClear();
				}), null, true);
				Validation.SkipValidationInDebugBuilds = false;
				ToolBox.OpenFileWithShell(Path.GetFullPath(filePath));
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("comparelocafiles", "comparelocafiles [file1] [file2]", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Please specify two files two compare.", null, null, false, false);
					return;
				}
				XDocument doc = XMLExtensions.TryLoadXml(args[0]);
				if (((doc != null) ? doc.Root : null) == null)
				{
					DebugConsole.ThrowError("Could not load the file \"" + args[0] + "\"", null, null, false, false);
					return;
				}
				XDocument doc2 = XMLExtensions.TryLoadXml(args[1]);
				if (((doc2 != null) ? doc2.Root : null) == null)
				{
					DebugConsole.ThrowError("Could not load the file \"" + args[1] + "\"", null, null, false, false);
					return;
				}
				Dictionary<string, string> content = DebugConsole.<InitProjectSpecific>g__getContent|23_211(doc.Root);
				Identifier language = doc.Root.GetAttributeIdentifier("language", string.Empty);
				Dictionary<string, string> content2 = DebugConsole.<InitProjectSpecific>g__getContent|23_211(doc2.Root);
				Identifier language2 = doc2.Root.GetAttributeIdentifier("language", string.Empty);
				foreach (KeyValuePair<string, string> kvp in content)
				{
					if (!content2.ContainsKey(kvp.Key))
					{
						DebugConsole.ThrowError("File 2 doesn't contain the text tag \"" + kvp.Key + "\"", null, null, false, false);
					}
					else if (language == language2 && content2[kvp.Key] != kvp.Value)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Texts for the tag \"");
						defaultInterpolatedStringHandler.AppendFormatted(kvp.Key);
						defaultInterpolatedStringHandler.AppendLiteral("\" don't match:\n1. ");
						defaultInterpolatedStringHandler.AppendFormatted(kvp.Value);
						defaultInterpolatedStringHandler.AppendLiteral("\n2. ");
						defaultInterpolatedStringHandler.AppendFormatted(content2[kvp.Key]);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
				}
				foreach (KeyValuePair<string, string> kvp2 in content2)
				{
					if (!content.ContainsKey(kvp2.Key))
					{
						DebugConsole.ThrowError("File 1 doesn't contain the text tag \"" + kvp2.Key + "\"", null, null, false, false);
					}
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("eventstats", "", delegate(string[] args)
			{
				List<string> debugLines;
				if (args.Length != 0)
				{
					Level.PositionType spawnType;
					if (!Enum.TryParse<Level.PositionType>(args[0], true, out spawnType))
					{
						string[] enums = Enum.GetNames(typeof(Level.PositionType));
						DebugConsole.ThrowError("\"" + args[0] + "\" is not a valid Level.PositionType. Available options are: " + string.Join(", ", enums), null, null, false, false);
						return;
					}
					bool fullLog = false;
					if (args.Length > 1)
					{
						bool.TryParse(args[1], out fullLog);
					}
					debugLines = EventSet.GetDebugStatistics(100, (MonsterEvent monsterEvent) => monsterEvent.SpawnPosType.HasFlag(spawnType), fullLog);
				}
				else
				{
					debugLines = EventSet.GetDebugStatistics(100, null, false);
				}
				string filePath = "eventstats.txt";
				Validation.SkipValidationInDebugBuilds = true;
				File.WriteAllLines(filePath, debugLines, null, true);
				Validation.SkipValidationInDebugBuilds = false;
				ToolBox.OpenFileWithShell(Path.GetFullPath(filePath));
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setfreecamspeed", "setfreecamspeed [speed]: Set the camera movement speed when not controlling a character. Defaults to 1.", delegate(string[] args)
			{
				if (args.Length != 0)
				{
					float speed;
					float.TryParse(args[0], NumberStyles.Number, CultureInfo.InvariantCulture, out speed);
					Screen.Selected.Cam.FreeCamMoveSpeed = speed;
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("converttowreck", "", delegate(string[] args)
			{
				if (!(Screen.Selected is SubEditorScreen))
				{
					DebugConsole.ThrowError("The command can only be used in the submarine editor.", null, null, false, false);
					return;
				}
				if (Submarine.MainSub == null)
				{
					DebugConsole.ThrowError("Load a submarine first to convert it to a wreck.", null, null, false, false);
					return;
				}
				if (Submarine.MainSub.Info.SubmarineElement == null)
				{
					DebugConsole.ThrowError("The submarine must be saved before you can convert it to a wreck.", null, null, false, false);
					return;
				}
				SubmarineInfo wreckedSubmarineInfo = new SubmarineInfo(string.Empty, "", WreckConverter.ConvertToWreck(Submarine.MainSub.Info.SubmarineElement), true, false);
				SubmarineInfo submarineInfo = wreckedSubmarineInfo;
				submarineInfo.Name += "_Wrecked";
				wreckedSubmarineInfo.Type = SubmarineType.Wreck;
				GameMain.SubEditorScreen.LoadSub(wreckedSubmarineInfo, true);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showitemxml", "showitemxml [item]: Shows the XML configuration of an item in the console and copies it to the clipboard. Useful for debugging variants that partially override the XML of the base item for example.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please specify the name or identifier of the item.", null, null, false, false);
					return;
				}
				string itemNameOrId = args[0].ToLowerInvariant();
				ItemPrefab itemPrefab = (MapEntityPrefab.FindByName(itemNameOrId) ?? MapEntityPrefab.FindByIdentifier(itemNameOrId.ToIdentifier())) as ItemPrefab;
				if (itemPrefab == null)
				{
					DebugConsole.ThrowError("Item \"{itemNameOrId}\" not found!", null, null, false, false);
					return;
				}
				string xmlStr = itemPrefab.ConfigElement.Element.ToString();
				DebugConsole.NewMessage(xmlStr, null, false);
				Clipboard.SetText(xmlStr);
			}, () => new string[][]
			{
				DebugConsole.GetItemNameOrIdParams().ToArray<string>()
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("dumptexts", "dumptexts [filepath]: Extracts all the texts from the given text xml and writes them into a file (using the same filename, but with the .txt extension). If the filepath is omitted, the EnglishVanilla.xml file is used.", delegate(string[] args)
			{
				string filePath = (args.Length != 0) ? args[0] : "Content/Texts/EnglishVanilla.xml";
				XDocument doc = XMLExtensions.TryLoadXml(filePath);
				if (doc == null)
				{
					return;
				}
				List<string> lines = new List<string>();
				foreach (XElement element in doc.Root.Elements())
				{
					lines.Add(element.ElementInnerText());
				}
				File.WriteAllLines(Path.GetFileNameWithoutExtension(filePath) + ".txt", lines, null, true);
			}, delegate()
			{
				IEnumerable<string> files = from f in TextManager.GetTextFiles()
				select f.CleanUpPath();
				string[][] array = new string[1][];
				array[0] = (from f in TextManager.GetTextFiles()
				where Path.GetExtension(f) == ".xml"
				select f).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("loadtexts", "loadtexts [sourcefile] [destinationfile]: Loads all lines of text from a given .txt file and inserts them sequientially into the elements of an xml file. If the file paths are omitted, EnglishVanilla.txt and EnglishVanilla.xml are used.", delegate(string[] args)
			{
				string sourcePath = (args.Length != 0) ? args[0] : "Content/Texts/EnglishVanilla.txt";
				string destinationPath = (args.Length > 1) ? args[1] : "Content/Texts/EnglishVanilla.xml";
				string[] lines;
				try
				{
					lines = File.ReadAllLines(sourcePath, null, false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Reading the file \"" + sourcePath + "\" failed.", e, null, false, false);
					return;
				}
				XDocument doc = XMLExtensions.TryLoadXml(destinationPath);
				if (doc == null)
				{
					return;
				}
				int i = 0;
				foreach (XElement element in doc.Root.Elements())
				{
					if (i >= lines.Length)
					{
						DebugConsole.ThrowError("Error while loading texts to the xml file. The xml has more elements than the number of lines in the text file.", null, null, false, false);
						return;
					}
					element.Value = lines[i];
					i++;
				}
				doc.SaveSafe(destinationPath, SaveOptions.None, false, 0);
			}, delegate()
			{
				IEnumerable<string> files = from f in TextManager.GetTextFiles()
				select f.CleanUpPath();
				string[][] array = new string[2][];
				array[0] = (from f in files
				where Path.GetExtension(f) == ".txt"
				select f).ToArray<string>();
				array[1] = (from f in files
				where Path.GetExtension(f) == ".xml"
				select f).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("updatetextfile", "updatetextfile [sourcefile] [destinationfile]: Inserts all the xml elements that are only present in the source file into the destination file. Can be used to update outdated translation files more easily.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					return;
				}
				string sourcePath = args[0];
				string destinationPath = args[1];
				XDocument sourceDoc = XMLExtensions.TryLoadXml(sourcePath);
				XDocument destinationDoc = XMLExtensions.TryLoadXml(destinationPath);
				if (sourceDoc == null || destinationDoc == null)
				{
					return;
				}
				XElement destinationElement = destinationDoc.Root.Elements().First<XElement>();
				foreach (XElement element in sourceDoc.Root.Elements())
				{
					if (destinationDoc.Root.Element(element.Name) == null)
					{
						element.Value = "!!!!!!!!!!!!!" + element.Value;
						destinationElement.AddAfterSelf(element);
					}
					XNode nextNode = destinationElement.NextNode;
					while ((!(nextNode is XElement) || nextNode == element) && nextNode != null)
					{
						nextNode = nextNode.NextNode;
					}
					destinationElement = (nextNode as XElement);
				}
				destinationDoc.SaveSafe(destinationPath, SaveOptions.None, false, 0);
			}, delegate()
			{
				string[] files = (from f in TextManager.GetTextFiles()
				where Path.GetExtension(f) == ".xml"
				select f.CleanUpPath()).ToArray<string>();
				return new string[][]
				{
					files,
					files
				};
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("dumpentitytexts", "dumpentitytexts [filepath]: gets the names and descriptions of all entity prefabs and writes them into a file along with xml tags that can be used in translation files. If the filepath is omitted, the file is written to Content/Texts/EntityTexts.txt", delegate(string[] args)
			{
				string filePath = (args.Length != 0) ? args[0] : "Content/Texts/EntityTexts.txt";
				List<string> lines = new List<string>();
				foreach (MapEntityPrefab me in MapEntityPrefab.List)
				{
					List<string> list = lines;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 3);
					defaultInterpolatedStringHandler.AppendLiteral("<EntityName.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(me.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(">");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(me.Name);
					defaultInterpolatedStringHandler.AppendLiteral("</EntityName.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(me.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(">");
					list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
					List<string> list2 = lines;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 3);
					defaultInterpolatedStringHandler2.AppendLiteral("<EntityDescription.");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(me.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(">");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(me.Description);
					defaultInterpolatedStringHandler2.AppendLiteral("</EntityDescription.");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(me.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(">");
					list2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				Validation.SkipValidationInDebugBuilds = true;
				File.WriteAllLines(filePath, lines, null, true);
				Validation.SkipValidationInDebugBuilds = false;
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("dumpeventtexts", "dumpeventtexts [sourcepath] [destinationpath]: gets the texts from event files and writes them into a file along with xml tags that can be used in translation files. If the filepath arguments are omitted, all event files are gone through and written to Content/Texts/EventTexts.txt", delegate(string[] args)
			{
				string sourcePath = (args.Length != 0) ? Path.GetFullPath(args[0]) : string.Empty;
				string destinationPath = (args.Length > 1) ? args[1] : "Content/Texts/EventTexts.txt";
				List<string> lines = new List<string>();
				HashSet<XDocument> docs = new HashSet<XDocument>();
				DebugConsole.<>c__DisplayClass23_19 CS$<>8__locals1;
				CS$<>8__locals1.textIds = new HashSet<string>();
				CS$<>8__locals1.existingTexts = new Dictionary<string, string>();
				foreach (EventPrefab eventPrefab in EventSet.GetAllEventPrefabs())
				{
					string dir = Path.GetDirectoryName(eventPrefab.FilePath.FullPath);
					if ((sourcePath.IsNullOrEmpty() || !(Path.GetFullPath(eventPrefab.FilePath.FullPath) != sourcePath) || !(Path.GetDirectoryName(eventPrefab.FilePath.FullPath) != sourcePath)) && !eventPrefab.Identifier.IsEmpty)
					{
						docs.Add(eventPrefab.ConfigElement.Document);
						DebugConsole.<InitProjectSpecific>g__getTextsFromElement|23_220(eventPrefab.ConfigElement, lines, eventPrefab.Identifier.Value, ref CS$<>8__locals1);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Collecting event texts from event \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventPrefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\"...");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
					}
				}
				if (!lines.None(null))
				{
					Validation.SkipValidationInDebugBuilds = true;
					try
					{
						File.WriteAllLines(destinationPath, lines, null, true);
					}
					catch (Exception e)
					{
						DebugConsole.ThrowError("Failed to write to the file \"" + destinationPath + "\".", e, null, false, false);
					}
					try
					{
						ToolBox.OpenFileWithShell(Path.GetFullPath(destinationPath));
						DebugConsole.NewMessage("Wrote the event texts to a text file in \"" + destinationPath + "\".", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
					}
					catch (Exception e2)
					{
						DebugConsole.ThrowError("Failed to open the file \"" + destinationPath + "\".", e2, null, false, false);
					}
					XmlWriterSettings settings = new XmlWriterSettings
					{
						Indent = true,
						NewLineOnAttributes = false
					};
					foreach (XDocument doc in docs)
					{
						string filePath = new Uri(doc.BaseUri).LocalPath;
						using (XmlWriter writer = XmlWriter.Create(filePath, settings))
						{
							doc.WriteTo(writer);
							writer.Flush();
							DebugConsole.NewMessage("Updated the event file \"" + filePath + "\".", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
						}
					}
					Validation.SkipValidationInDebugBuilds = false;
					return;
				}
				if (sourcePath.IsNullOrEmpty())
				{
					DebugConsole.ThrowError("Could not find any event texts. Have all the texts already been moved from the event files to the text files?", null, null, false, false);
					return;
				}
				DebugConsole.ThrowError("Could not find any event texts from \"" + sourcePath + "\". Are you sure the path is to a valid event xml file or a directory that contains event xml files?", null, null, false, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("itemcomponentdocumentation", "", delegate(string[] args)
			{
				Dictionary<string, string> typeNames = new Dictionary<string, string>
				{
					{
						"Single",
						"Float"
					},
					{
						"Int32",
						"Integer"
					},
					{
						"Boolean",
						"True/False"
					},
					{
						"String",
						"Text"
					}
				};
				List<Type> itemComponentTypes = (from type in typeof(ItemComponent).Assembly.GetTypes()
				where type.IsSubclassOf(typeof(ItemComponent))
				select type).ToList<Type>();
				itemComponentTypes.Sort((Type i1, Type i2) => i1.Name.CompareTo(i2.Name));
				itemComponentTypes.Insert(0, typeof(ItemComponent));
				string filePath = (args.Length != 0) ? args[0] : "ItemComponentDocumentation.txt";
				List<string> lines = new List<string>();
				foreach (Type t in itemComponentTypes)
				{
					lines.Add("[h1]" + t.Name + "[/h1]");
					lines.Add("");
					List<PropertyInfo> properties = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList<PropertyInfo>();
					Type baseType = t.BaseType;
					while (baseType != null && baseType != typeof(ItemComponent))
					{
						properties.AddRange(baseType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));
						baseType = baseType.BaseType;
					}
					if (!properties.Any((PropertyInfo p) => p.GetCustomAttributes(true).Any((object a) => a is Serialize)))
					{
						lines.Add("No editable properties.");
						lines.Add("");
					}
					else
					{
						lines.Add("[table]");
						lines.Add("  [tr]");
						lines.Add("    [th]Name[/th]");
						lines.Add("    [th]Type[/th]");
						lines.Add("    [th]Default value[/th]");
						lines.Add("    [th]Description[/th]");
						lines.Add("  [/tr]");
						Dictionary<Identifier, SerializableProperty> dictionary = new Dictionary<Identifier, SerializableProperty>();
						foreach (PropertyInfo property in properties)
						{
							object[] attributes = property.GetCustomAttributes(true);
							Serialize serialize = attributes.FirstOrDefault((object a) => a is Serialize) as Serialize;
							if (serialize != null)
							{
								string propertyTypeName = property.PropertyType.Name;
								if (typeNames.ContainsKey(propertyTypeName))
								{
									propertyTypeName = typeNames[propertyTypeName];
								}
								else if (property.PropertyType.IsEnum)
								{
									List<string> valueNames = new List<string>();
									foreach (object enumValue in Enum.GetValues(property.PropertyType))
									{
										valueNames.Add(enumValue.ToString());
									}
									propertyTypeName = string.Join("/", valueNames);
								}
								object defaultValue = serialize.DefaultValue;
								string defaultValueString = ((defaultValue != null) ? defaultValue.ToString() : null) ?? "";
								if (property.PropertyType == typeof(float))
								{
									defaultValueString = ((float)serialize.DefaultValue).ToString(CultureInfo.InvariantCulture);
								}
								lines.Add("  [tr]");
								lines.Add("    [td]" + property.Name + "[/td]");
								lines.Add("    [td]" + propertyTypeName + "[/td]");
								lines.Add("    [td]" + defaultValueString + "[/td]");
								Editable editable = attributes.FirstOrDefault((object a) => a is Editable) as Editable;
								if (editable != null)
								{
									if (editable.MinValueFloat > -3.4028235E+38f || editable.MaxValueFloat < 3.4028235E+38f)
									{
										string rangeText = editable.MinValueFloat.ToString() + "-" + editable.MaxValueFloat.ToString();
									}
									else if (editable.MinValueInt > -2147483648 || editable.MaxValueInt < 2147483647)
									{
										string rangeText = editable.MinValueInt.ToString() + "-" + editable.MaxValueInt.ToString();
									}
								}
								if (!string.IsNullOrEmpty(serialize.Description))
								{
									lines.Add("    [td]" + serialize.Description + "[/td]");
								}
								lines.Add("  [/tr]");
							}
						}
						lines.Add("[/table]");
						lines.Add("");
					}
				}
				Validation.SkipValidationInDebugBuilds = true;
				File.WriteAllLines(filePath, lines, null, true);
				Validation.SkipValidationInDebugBuilds = false;
				ToolBox.OpenFileWithShell(Path.GetFullPath(filePath));
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("reloadcorepackage", "", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					if (Screen.Selected == GameMain.GameScreen)
					{
						DebugConsole.ThrowError("Reloading the core package while in GameScreen WILL break everything; to do it anyway, type 'reloadcorepackage force'", null, null, false, false);
						return;
					}
					if (Screen.Selected == GameMain.SubEditorScreen)
					{
						DebugConsole.ThrowError("Reloading the core package while in sub editor WILL break everything; to do it anyway, type 'reloadcorepackage force'", null, null, false, false);
						return;
					}
				}
				if (GameMain.NetworkMember != null)
				{
					DebugConsole.ThrowError("Cannot change content packages while playing online", null, null, false, false);
					return;
				}
				ContentPackageManager.EnabledPackages.ReloadCore();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("reloadpackage", "reloapackage [name]: reloads a content package.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					DebugConsole.ThrowError("Please specify the name of the package to reload.", null, null, false, false);
					return;
				}
				if (args.Length < 2)
				{
					if (Screen.Selected == GameMain.GameScreen)
					{
						DebugConsole.ThrowError("Reloading the package while in GameScreen may break things; to do it anyway, type 'reloadpackage [name] force'", null, null, false, false);
						return;
					}
					if (Screen.Selected == GameMain.SubEditorScreen)
					{
						DebugConsole.ThrowError("Reloading the core package while in sub editor may break things; to do it anyway, type 'reloadpackage [name] force'", null, null, false, false);
						return;
					}
				}
				if (GameMain.NetworkMember != null)
				{
					DebugConsole.ThrowError("Cannot change content packages while playing online", null, null, false, false);
					return;
				}
				RegularPackage package = ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name == args[0]);
				if (package == null)
				{
					DebugConsole.ThrowError("Could not find the package " + args[0] + "!", null, null, false, false);
					return;
				}
				ContentPackageManager.EnabledPackages.ReloadPackage(package);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from p in ContentPackageManager.RegularPackages
				select p.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("startdedicatedserver", "", delegate(string[] args)
			{
				Process.Start("DedicatedServer.exe");
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("editserversettings", "", delegate(string[] args)
			{
				if (Process.GetProcessesByName("DedicatedServer").Length != 0)
				{
					DebugConsole.NewMessage("Can't be edited if DedicatedServer.exe is already running", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				Process.Start("notepad.exe", "serversettings.xml");
			}, null, false));
			DebugConsole.AssignOnClientExecute("giveperm", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				DebugConsole.NewMessage("Valid permissions are:", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
				{
					DebugConsole.NewMessage(" - " + ((ClientPermissions)obj).ToString(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				}
				DebugConsole.ShowQuestionPrompt("Permission to grant to client " + args[0] + "?", delegate(string perm)
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("giveperm \"" + args[0] + "\" " + perm);
				}, args, 1);
			});
			DebugConsole.AssignOnClientExecute("revokeperm", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.NewMessage("Valid permissions are:", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
					foreach (object obj in Enum.GetValues(typeof(ClientPermissions)))
					{
						DebugConsole.NewMessage(" - " + ((ClientPermissions)obj).ToString(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
					}
				}
				DebugConsole.ShowQuestionPrompt("Permission to revoke from client " + args[0] + "?", delegate(string perm)
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("revokeperm \"" + args[0] + "\" " + perm);
				}, args, 1);
			});
			DebugConsole.AssignOnClientExecute("giverank", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				DebugConsole.NewMessage("Valid ranks are:", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				foreach (PermissionPreset permissionPreset in PermissionPreset.List)
				{
					DebugConsole.NewMessage(" - " + permissionPreset.DisplayName, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				}
				DebugConsole.ShowQuestionPrompt("Rank to grant to client " + args[0] + "?", delegate(string rank)
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("giverank \"" + args[0] + "\" " + rank);
				}, args, 1);
			});
			DebugConsole.AssignOnClientExecute("givecommandperm", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				DebugConsole.ShowQuestionPrompt("Console command permissions to grant to client " + args[0] + "? You may enter multiple commands separated with a space or use \"all\" to give the permission to use all console commands.", delegate(string commandNames)
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("givecommandperm \"" + args[0] + "\" " + commandNames);
				}, args, 1);
			});
			DebugConsole.AssignOnClientExecute("revokecommandperm", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				DebugConsole.ShowQuestionPrompt("Console command permissions to revoke from client " + args[0] + "? You may enter multiple commands separated with a space or use \"all\" to revoke the permission to use any console commands.", delegate(string commandNames)
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("revokecommandperm \"" + args[0] + "\" " + commandNames);
				}, args, 1);
			});
			DebugConsole.AssignOnClientExecute("showperm", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					return;
				}
				GameMain.Client.SendConsoleCommand("showperm " + args[0]);
			});
			DebugConsole.AssignOnClientExecute("banaddress|banip", delegate(string[] args)
			{
				if (GameMain.Client == null || args.Length == 0)
				{
					return;
				}
				DebugConsole.ShowQuestionPrompt("Reason for banning the endpoint \"" + args[0] + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (Enter c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						GameClient client = GameMain.Client;
						if (client == null)
						{
							return;
						}
						client.SendConsoleCommand(string.Concat(new string[]
						{
							"banaddress ",
							args[0],
							" ",
							(banDuration != null) ? banDuration.Value.TotalSeconds.ToString() : "0",
							" ",
							reason
						}));
					}, null, -1);
				}, null, -1);
			});
			DebugConsole.commands.Add(new DebugConsole.Command("unban", "unban [name]: Unban a specific client.", delegate(string[] args)
			{
				if (GameMain.Client == null || args.Length == 0)
				{
					return;
				}
				string clientName = string.Join(" ", args);
				GameMain.Client.UnbanPlayer(clientName);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("unbanaddress", "unbanaddress [endpoint]: Unban a specific endpoint.", delegate(string[] args)
			{
				if (GameMain.Client == null || args.Length == 0)
				{
					return;
				}
				Endpoint endpoint;
				if (Endpoint.Parse(args[0]).TryUnwrap(out endpoint))
				{
					GameMain.Client.UnbanPlayer(endpoint);
				}
			}, null, false));
			DebugConsole.AssignOnClientExecute("campaigndestination|setcampaigndestination", delegate(string[] args)
			{
				DebugConsole.<>c__DisplayClass23_28 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass23_28();
				DebugConsole.<>c__DisplayClass23_28 CS$<>8__locals2 = CS$<>8__locals1;
				GameSession gameSession = GameMain.GameSession;
				CS$<>8__locals2.campaign = (((gameSession != null) ? gameSession.GameMode : null) as CampaignMode);
				if (CS$<>8__locals1.campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					int i = 0;
					foreach (LocationConnection connection in CS$<>8__locals1.campaign.Map.CurrentLocation.Connections)
					{
						DebugConsole.NewMessage("     " + i.ToString() + ". " + connection.OtherLocation(CS$<>8__locals1.campaign.Map.CurrentLocation).DisplayName, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
						i++;
					}
					DebugConsole.ShowQuestionPrompt("Select a destination (0 - " + (CS$<>8__locals1.campaign.Map.CurrentLocation.Connections.Count - 1).ToString() + "):", delegate(string selectedDestination)
					{
						int destinationIndex2 = -1;
						if (!int.TryParse(selectedDestination, out destinationIndex2))
						{
							return;
						}
						if (destinationIndex2 < 0 || destinationIndex2 >= CS$<>8__locals1.campaign.Map.CurrentLocation.Connections.Count)
						{
							DebugConsole.NewMessage("Index out of bounds!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
							return;
						}
						GameClient client = GameMain.Client;
						if (client == null)
						{
							return;
						}
						client.SendConsoleCommand("campaigndestination " + destinationIndex2.ToString());
					}, null, -1);
					return;
				}
				int destinationIndex = -1;
				if (!int.TryParse(args[0], out destinationIndex))
				{
					return;
				}
				if (destinationIndex < 0 || destinationIndex >= CS$<>8__locals1.campaign.Map.CurrentLocation.Connections.Count)
				{
					DebugConsole.NewMessage("Index out of bounds!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				GameMain.Client.SendConsoleCommand("campaigndestination " + destinationIndex.ToString());
			});
			DebugConsole.commands.Add(new DebugConsole.Command("limbscale", "Define the limbscale for the controlled character. Provide id or name if you want to target another character. Note: the changes are not saved!", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please give the value after the command.", null, null, false, false);
					return;
				}
				float value;
				if (!float.TryParse(args[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value))
				{
					DebugConsole.ThrowError("Failed to parse float value from the arguments", null, null, false, false);
					return;
				}
				RagdollParams ragdollParams = character.AnimController.RagdollParams;
				ragdollParams.LimbScale = MathHelper.Clamp(value, 0.1f, 2f);
				character.AnimController.RecreateAndRespawn(null);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("jointscale", "Define the jointscale for the controlled character. Provide id or name if you want to target another character. Note: the changes are not saved!", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please give the value after the command.", null, null, false, false);
					return;
				}
				float value;
				if (!float.TryParse(args[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value))
				{
					DebugConsole.ThrowError("Failed to parse float value from the arguments", null, null, false, false);
					return;
				}
				RagdollParams ragdollParams = character.AnimController.RagdollParams;
				ragdollParams.JointScale = MathHelper.Clamp(value, 0.1f, 2f);
				character.AnimController.RecreateAndRespawn(null);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ragdollscale", "Rescale the ragdoll of the controlled character. Provide id or name if you want to target another character. Note: the changes are not saved!", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please give the value after the command.", null, null, false, false);
					return;
				}
				float value;
				if (!float.TryParse(args[0], NumberStyles.Number, CultureInfo.InvariantCulture, out value))
				{
					DebugConsole.ThrowError("Failed to parse float value from the arguments", null, null, false, false);
					return;
				}
				RagdollParams ragdollParams = character.AnimController.RagdollParams;
				ragdollParams.LimbScale = MathHelper.Clamp(value, 0.1f, 2f);
				ragdollParams.JointScale = MathHelper.Clamp(value, 0.1f, 2f);
				character.AnimController.RecreateAndRespawn(null);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("recreateragdoll", "Recreate the ragdoll of the controlled character. Provide id or name if you want to target another character.", delegate(string[] args)
			{
				Character character = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, true, null, false);
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				character.AnimController.RecreateAndRespawn(null);
			}, () => new string[][]
			{
				DebugConsole.GetSpawnedSpeciesNames()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("resetragdoll", "Reset the ragdoll of the controlled character (and all of the same species). Provide species name if you want to target another character.", delegate(string[] args)
			{
				IEnumerable<Character> characters;
				if (args.Length == 0)
				{
					if (Character.Controlled == null)
					{
						DebugConsole.ThrowError("Invalid species name! Press [TAB] to get valid options in this context.", null, null, false, false);
						return;
					}
					characters = DebugConsole.FindMatchingSpecies(Character.Controlled.SpeciesName.ToString());
				}
				else
				{
					characters = DebugConsole.FindMatchingSpecies(args);
				}
				if (characters.None(null))
				{
					DebugConsole.ThrowError("Invalid species name!", null, null, false, false);
					return;
				}
				characters.ForEach(delegate(Character c)
				{
					c.AnimController.ResetRagdoll();
				});
				foreach (Character character in characters)
				{
					if (!character.VariantOf.IsEmpty)
					{
						character.AnimController.RecreateAndRespawn(null);
					}
				}
			}, () => new string[][]
			{
				DebugConsole.GetSpawnedSpeciesNames()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("loadanimation", "Loads an animation variation by name for the controlled character. The animation file has to be in the correct animations folder. Note: the changes are not saved!", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Insufficient parameters: Have to pass the type of animation (Walk, Run, SwimSlow, SwimFast, or Crouch) and the filename!", null, null, false, false);
					return;
				}
				string type = args[0];
				AnimationType animationType;
				if (!Enum.TryParse<AnimationType>(type, true, out animationType))
				{
					DebugConsole.ThrowError("Failed to parse animation type from " + type + ". Supported types are Walk, Run, SwimSlow, SwimFast, and Crouch!", null, null, false, false);
					return;
				}
				string fileName = args[1];
				AnimationParams animationParams;
				character.AnimController.TryLoadAnimation(animationType, Path.GetFileNameWithoutExtension(fileName), out animationParams, true);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("startlocalmptestsession", "startlocalmptestsession [(optional) number of clients, defaults to 2]: starts a new mp test session with multiple clients connected to local dedicated server", delegate(string[] args)
			{
				if (Screen.Selected != GameMain.MainMenuScreen)
				{
					DebugConsole.ThrowError("Must be in main menu to start.", null, null, false, false);
					return;
				}
				int numClients = 2;
				if (args.Length != 0 && !int.TryParse(args[0], out numClients))
				{
					DebugConsole.ThrowError("Failed to parse the number of clients.", null, null, false, false);
					return;
				}
				DebugConsole.StartLocalMPSession(numClients);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("reloadwearables", "Reloads the sprites of all limbs and wearable sprites (clothing) of the controlled character. Provide id or name if you want to target another character.", delegate(string[] args)
			{
				Character character = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, true, null, false);
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character or no matching character found with the provided arguments.", null, null, false, false);
					return;
				}
				DebugConsole.ReloadWearables(character, 0);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("loadwearable", "Force select certain variant for the selected character.", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character.", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No arguments provided! Give an index number for the variant starting from 1.", null, null, false, false);
					return;
				}
				int variant;
				if (int.TryParse(args[0], out variant))
				{
					DebugConsole.ReloadWearables(character, variant);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("reloadsprite|reloadsprites", "Reloads the sprites of the selected item(s)/structure(s) (hovering over or selecting in the subeditor) or the controlled character. Can also reload sprites by entity id or by the name attribute (sprite element). Example 1: reloadsprite id itemid. Example 2: reloadsprite name \"Sprite name\"", delegate(string[] args)
			{
				if (Screen.Selected is SpriteEditorScreen)
				{
					return;
				}
				if (args.Length > 1)
				{
					DebugConsole.TryDoActionOnSprite(args[0], args[1], delegate(Sprite s)
					{
						s.ReloadXML();
						s.ReloadTexture();
					});
					return;
				}
				if (Screen.Selected is SubEditorScreen)
				{
					if (!MapEntity.SelectedAny)
					{
						DebugConsole.ThrowError("You have to select item(s)/structure(s) first!", null, null, false, false);
					}
					MapEntity.SelectedList.ForEach(delegate(MapEntity e)
					{
						if (e.Sprite != null)
						{
							e.Sprite.ReloadXML();
							e.Sprite.ReloadTexture();
						}
					});
					return;
				}
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Please provide the mode (name or id) and the value so that I can find the sprite for you!", null, null, false, false);
					return;
				}
				Item item = character.FocusedItem;
				if (item != null)
				{
					item.Sprite.ReloadXML();
					item.Sprite.ReloadTexture();
					return;
				}
				DebugConsole.ReloadWearables(character, 0);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("flipx", "flipx: mirror the main submarine horizontally", delegate(string[] args)
			{
				if (GameMain.NetworkMember != null)
				{
					DebugConsole.ThrowError("Cannot use the flipx command while playing online.", null, null, false, false);
					return;
				}
				Submarine mainSub = Submarine.MainSub;
				if (((mainSub != null) ? mainSub.SubBody : null) != null)
				{
					Submarine.MainSub.FlipX(null);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("head", "Load the head sprite and the wearables (hair etc). Required argument: head id. Optional arguments: hair index, beard index, moustache index, face attachment index.", delegate(string[] args)
			{
				Character character = Character.Controlled;
				if (character == null)
				{
					DebugConsole.ThrowError("Not controlling any character!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No head id provided!", null, null, false, false);
					return;
				}
				int id;
				if (int.TryParse(args[0], out id))
				{
					int faceAttachmentIndex;
					int moustacheIndex;
					int hairIndex;
					int beardIndex = hairIndex = (moustacheIndex = (faceAttachmentIndex = -1));
					if (args.Length > 1)
					{
						int.TryParse(args[1], out hairIndex);
					}
					if (args.Length > 2)
					{
						int.TryParse(args[2], out beardIndex);
					}
					if (args.Length > 3)
					{
						int.TryParse(args[3], out moustacheIndex);
					}
					if (args.Length > 4)
					{
						int.TryParse(args[4], out faceAttachmentIndex);
					}
					character.ReloadHead(new int?(id), hairIndex, beardIndex, moustacheIndex, faceAttachmentIndex);
					foreach (Limb limb in character.AnimController.Limbs)
					{
						if (limb.type != LimbType.Head)
						{
							limb.RecreateSprites();
						}
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("spawnsub", "spawnsub [subname] [is thalamus]: Spawn a submarine at the position of the cursor", delegate(string[] args)
			{
				if (GameMain.NetworkMember != null)
				{
					DebugConsole.ThrowError("Cannot spawn additional submarines during a multiplayer session.", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please enter the name of the submarine.", null, null, false, false);
					return;
				}
				try
				{
					SubmarineInfo subInfo = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.DisplayName.Equals(args[0], StringComparison.OrdinalIgnoreCase) || s.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (subInfo == null)
					{
						DebugConsole.ThrowError("Could not find a submarine with the name \"" + args[0] + "\".", null, null, false, false);
					}
					else
					{
						Submarine spawnedSub = Submarine.Load(subInfo, false, null);
						spawnedSub.SetPosition(GameMain.GameScreen.Cam.ScreenToWorld(PlayerInput.MousePosition), null, true);
						if (subInfo.Type == SubmarineType.Wreck)
						{
							spawnedSub.MakeWreck();
							bool isThalamus;
							if (args.Length > 1 && bool.TryParse(args[1], out isThalamus))
							{
								if (isThalamus)
								{
									spawnedSub.CreateWreckAI();
								}
								else
								{
									spawnedSub.DisableWreckAI();
								}
							}
							else
							{
								spawnedSub.DisableWreckAI();
							}
						}
					}
				}
				catch (Exception e)
				{
					string errorMsg = "Failed to spawn a submarine. Arguments: \"" + string.Join(" ", args) + "\".";
					DebugConsole.ThrowError(errorMsg, e, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("DebugConsole.SpawnSubmarine:Error", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
					{
						errorMsg,
						"\n",
						e.Message,
						"\n",
						e.StackTrace.CleanupStackTrace()
					}));
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from s in SubmarineInfo.SavedSubmarines
				select s.DisplayName.Value).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("pause", "Toggles the pause state when playing offline", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null)
				{
					DebugConsole.Paused = !DebugConsole.Paused;
					DebugConsole.NewMessage("Game paused: " + DebugConsole.Paused.ToString(), null, false);
					return;
				}
				DebugConsole.NewMessage("Cannot pause when a multiplayer session is active.", null, false);
			}, null, false));
			DebugConsole.AssignOnClientExecute("showseed|showlevelseed", delegate(string[] args)
			{
				if (Level.Loaded == null)
				{
					DebugConsole.ThrowError("No level loaded.", null, null, false, false);
					return;
				}
				DebugConsole.NewMessage("Level seed: " + Level.Loaded.Seed, null, false);
				DebugConsole.NewMessage("Level generation params: " + Level.Loaded.GenerationParams.Identifier.ToString(), null, false);
				string str = "Adjacent locations: ";
				Location startLocation = Level.Loaded.StartLocation;
				string str2 = ((startLocation != null) ? startLocation.Type.Identifier : "none".ToIdentifier()).ToString();
				string str3 = ", ";
				Location startLocation2 = Level.Loaded.StartLocation;
				DebugConsole.NewMessage(str + str2 + str3 + ((startLocation2 != null) ? startLocation2.Type.Identifier : "none".ToIdentifier()).ToString(), null, false);
				DebugConsole.NewMessage("Mirrored: " + Level.Loaded.Mirrored.ToString(), null, false);
				string str4 = "Level size: ";
				Point size = Level.Loaded.Size;
				string str5 = size.X.ToString();
				string str6 = "x";
				size = Level.Loaded.Size;
				DebugConsole.NewMessage(str4 + str5 + str6 + size.Y.ToString(), null, false);
				string str7 = "Minimum main path width: ";
				LevelData levelData = Level.Loaded.LevelData;
				DebugConsole.NewMessage(str7 + (((levelData != null) ? ((levelData.MinMainPathWidth != null) ? levelData.MinMainPathWidth.GetValueOrDefault().ToString() : null) : null) ?? "unknown"), null, false);
			});
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x0005A808 File Offset: 0x00058A08
		private static void ReloadWearables(Character character, int variant = 0)
		{
			foreach (Limb limb in character.AnimController.Limbs)
			{
				Sprite sprite = limb.Sprite;
				if (sprite != null)
				{
					sprite.ReloadTexture();
				}
				Sprite damagedSprite = limb.DamagedSprite;
				if (damagedSprite != null)
				{
					damagedSprite.ReloadTexture();
				}
				DeformableSprite deformSprite = limb.DeformSprite;
				if (deformSprite != null)
				{
					deformSprite.Sprite.ReloadTexture();
				}
				foreach (WearableSprite wearable in limb.WearingItems)
				{
					if (variant > 0 && wearable.Variant > 0)
					{
						wearable.Variant = variant;
					}
					wearable.ParsePath(true);
					wearable.Sprite.ReloadXML();
					wearable.Sprite.ReloadTexture();
				}
				foreach (WearableSprite wearable2 in limb.OtherWearables)
				{
					wearable2.ParsePath(true);
					wearable2.Sprite.ReloadXML();
					wearable2.Sprite.ReloadTexture();
				}
				if (limb.HuskSprite != null)
				{
					limb.HuskSprite.Sprite.ReloadXML();
					limb.HuskSprite.Sprite.ReloadTexture();
				}
				if (limb.HerpesSprite != null)
				{
					limb.HerpesSprite.Sprite.ReloadXML();
					limb.HerpesSprite.Sprite.ReloadTexture();
				}
			}
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x0005A994 File Offset: 0x00058B94
		private static bool TryDoActionOnSprite(string firstArg, string secondArg, Action<Sprite> action)
		{
			if (!(firstArg == "name"))
			{
				if (!(firstArg == "identifier") && !(firstArg == "id"))
				{
					DebugConsole.ThrowError("The first argument must be either 'name' or 'id'", null, null, false, false);
					return false;
				}
				IEnumerable<Sprite> sprites = Sprite.LoadedSprites.Where(delegate(Sprite s)
				{
					Identifier entityIdentifier = s.EntityIdentifier;
					if (entityIdentifier != null)
					{
						Identifier entityIdentifier2 = s.EntityIdentifier;
						return entityIdentifier2 == secondArg;
					}
					return false;
				});
				if (sprites.Any<Sprite>())
				{
					foreach (Sprite s3 in sprites)
					{
						action(s3);
					}
					return true;
				}
				DebugConsole.ThrowError("Cannot find any matching sprites by the id: " + secondArg, null, null, false, false);
				return false;
			}
			else
			{
				IEnumerable<Sprite> sprites = from s in Sprite.LoadedSprites
				where s.Name != null && s.Name.Equals(secondArg, StringComparison.OrdinalIgnoreCase)
				select s;
				if (sprites.Any<Sprite>())
				{
					foreach (Sprite s2 in sprites)
					{
						action(s2);
					}
					return true;
				}
				DebugConsole.ThrowError("Cannot find any matching sprites by the name: " + secondArg, null, null, false, false);
				return false;
			}
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x0005AADC File Offset: 0x00058CDC
		private static void PrintItemCosts(Dictionary<ItemPrefab, int> newPrices, ItemPrefab materialPrefab, List<FabricationRecipe> fabricableItems, int newPrice, bool adjustDown, string depth = "", DebugConsole.AdjustItemTypes adjustItemType = DebugConsole.AdjustItemTypes.NoAdjustment)
		{
			if (newPrice < 1)
			{
				DebugConsole.NewMessage(depth + materialPrefab.Name + " cannot be adjusted to this price, because it would become less than 1.", null, false);
				return;
			}
			depth += "   ";
			newPrices.TryAdd(materialPrefab, newPrice);
			int componentCost = 0;
			int newComponentCost = 0;
			FabricationRecipe fabricationRecipe = fabricableItems.Find((FabricationRecipe f) => f.TargetItem == materialPrefab);
			if (fabricationRecipe != null)
			{
				foreach (FabricationRecipe.RequiredItem requiredItem in fabricationRecipe.RequiredItems)
				{
					foreach (ItemPrefab itemPrefab in requiredItem.ItemPrefabs)
					{
						DebugConsole.GetAdjustedPrice(itemPrefab, ref componentCost, ref newComponentCost, newPrices);
					}
				}
			}
			string componentCostMultiplier = "";
			if (componentCost > 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 4);
				defaultInterpolatedStringHandler.AppendLiteral(" (Relative difference to component cost ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(DebugConsole.GetComponentCostDifference(materialPrefab.DefaultPrice.Price, componentCost));
				defaultInterpolatedStringHandler.AppendLiteral(" => ");
				defaultInterpolatedStringHandler.AppendFormatted<double>(DebugConsole.GetComponentCostDifference(newPrice, newComponentCost));
				defaultInterpolatedStringHandler.AppendLiteral(", or flat profit ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(materialPrefab.DefaultPrice.Price - componentCost);
				defaultInterpolatedStringHandler.AppendLiteral(" => ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(newPrice - newComponentCost);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				componentCostMultiplier = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string priceAdjustment = "";
			if (newPrice != materialPrefab.DefaultPrice.Price)
			{
				priceAdjustment = ", Suggested price adjustment is " + materialPrefab.DefaultPrice.Price.ToString() + " => " + newPrice.ToString();
			}
			DebugConsole.NewMessage(depth + materialPrefab.Name + "(" + materialPrefab.DefaultPrice.Price + ") " + priceAdjustment + componentCostMultiplier, null, false);
			if (adjustDown)
			{
				if (componentCost > 0)
				{
					double newPriceMult = (double)newPrice / (double)materialPrefab.DefaultPrice.Price;
					int newPriceDiff = componentCost + newPrice - materialPrefab.DefaultPrice.Price;
					if (adjustItemType != DebugConsole.AdjustItemTypes.Additive)
					{
						if (adjustItemType == DebugConsole.AdjustItemTypes.Multiplicative)
						{
							DebugConsole.NewMessage(depth + materialPrefab.Name + "'s components should be adjusted " + componentCost + " => " + Math.Round(newPriceMult * (double)componentCost), null, false);
						}
					}
					else
					{
						DebugConsole.NewMessage(depth + materialPrefab.Name + "'s components should be adjusted " + componentCost + " => " + newPriceDiff, null, false);
					}
					if (fabricationRecipe != null)
					{
						foreach (FabricationRecipe.RequiredItem requiredItem2 in fabricationRecipe.RequiredItems)
						{
							foreach (ItemPrefab itemPrefab2 in requiredItem2.ItemPrefabs)
							{
								if (itemPrefab2.DefaultPrice != null)
								{
									switch (adjustItemType)
									{
									case DebugConsole.AdjustItemTypes.NoAdjustment:
										DebugConsole.PrintItemCosts(newPrices, itemPrefab2, fabricableItems, itemPrefab2.DefaultPrice.Price, adjustDown, depth, adjustItemType);
										break;
									case DebugConsole.AdjustItemTypes.Additive:
										DebugConsole.PrintItemCosts(newPrices, itemPrefab2, fabricableItems, itemPrefab2.DefaultPrice.Price + (int)((double)(newPrice - materialPrefab.DefaultPrice.Price) / (double)fabricationRecipe.RequiredItems.Length), adjustDown, depth, adjustItemType);
										break;
									case DebugConsole.AdjustItemTypes.Multiplicative:
										DebugConsole.PrintItemCosts(newPrices, itemPrefab2, fabricableItems, (int)((double)itemPrefab2.DefaultPrice.Price * newPriceMult), adjustDown, depth, adjustItemType);
										break;
									}
								}
							}
						}
						return;
					}
				}
			}
			else
			{
				Func<FabricationRecipe.RequiredItem, bool> <>9__2;
				IEnumerable<FabricationRecipe> fabricationRecipes = fabricableItems.Where(delegate(FabricationRecipe f)
				{
					ImmutableArray<FabricationRecipe.RequiredItem> requiredItems = f.RequiredItems;
					Func<FabricationRecipe.RequiredItem, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((FabricationRecipe.RequiredItem x) => x.ItemPrefabs.Contains(materialPrefab)));
					}
					return requiredItems.Any(predicate);
				});
				foreach (FabricationRecipe fabricationRecipeParent in fabricationRecipes)
				{
					if (fabricationRecipeParent.TargetItem.DefaultPrice != null)
					{
						int targetComponentCost = 0;
						int newTargetComponentCost = 0;
						foreach (FabricationRecipe.RequiredItem requiredItem3 in fabricationRecipeParent.RequiredItems)
						{
							foreach (ItemPrefab itemPrefab3 in requiredItem3.ItemPrefabs)
							{
								DebugConsole.GetAdjustedPrice(itemPrefab3, ref targetComponentCost, ref newTargetComponentCost, newPrices);
							}
						}
						switch (adjustItemType)
						{
						case DebugConsole.AdjustItemTypes.NoAdjustment:
							DebugConsole.PrintItemCosts(newPrices, fabricationRecipeParent.TargetItem, fabricableItems, fabricationRecipeParent.TargetItem.DefaultPrice.Price, adjustDown, depth, adjustItemType);
							break;
						case DebugConsole.AdjustItemTypes.Additive:
							DebugConsole.PrintItemCosts(newPrices, fabricationRecipeParent.TargetItem, fabricableItems, fabricationRecipeParent.TargetItem.DefaultPrice.Price + newPrice - materialPrefab.DefaultPrice.Price, adjustDown, depth, adjustItemType);
							break;
						case DebugConsole.AdjustItemTypes.Multiplicative:
						{
							double maintainedMultiplier = DebugConsole.GetComponentCostDifference(fabricationRecipeParent.TargetItem.DefaultPrice.Price, targetComponentCost);
							DebugConsole.PrintItemCosts(newPrices, fabricationRecipeParent.TargetItem, fabricableItems, (int)((double)newTargetComponentCost * maintainedMultiplier), adjustDown, depth, adjustItemType);
							break;
						}
						}
					}
				}
			}
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x0005B10C File Offset: 0x0005930C
		private static double GetComponentCostDifference(int itemCost, int componentCost)
		{
			return Math.Round((double)itemCost / (double)componentCost, 2);
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x0005B11C File Offset: 0x0005931C
		private static void GetAdjustedPrice(ItemPrefab itemPrefab, ref int componentCost, ref int newComponentCost, Dictionary<ItemPrefab, int> newPrices)
		{
			int newPrice;
			if (newPrices.TryGetValue(itemPrefab, out newPrice))
			{
				newComponentCost += newPrice;
			}
			else if (itemPrefab.DefaultPrice != null)
			{
				newComponentCost += itemPrefab.DefaultPrice.Price;
			}
			if (itemPrefab.DefaultPrice != null)
			{
				componentCost += itemPrefab.DefaultPrice.Price;
			}
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x0005B16C File Offset: 0x0005936C
		public static void StartLocalMPSession(int numClients = 2)
		{
			string extraArguments = "-multiclienttestmode";
			if (NetConfig.UseLenientHandshake)
			{
				extraArguments += " -lenienthandshake";
			}
			try
			{
				if (Process.GetProcessesByName("DedicatedServer").Length == 0)
				{
					Process.Start("DedicatedServer.exe", extraArguments);
					Thread.Sleep(1000);
				}
				GameMain.Client = new GameClient("client1", new LidgrenEndpoint(IPAddress.Loopback, 27015), "localhost", Option<int>.None());
				numClients = MathHelper.Clamp(numClients, 1, 4);
				if (numClients > 1)
				{
					for (int i = 2; i <= numClients; i++)
					{
						Thread.Sleep(1000);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 1);
						defaultInterpolatedStringHandler.AppendLiteral("-connect server localhost -username client");
						defaultInterpolatedStringHandler.AppendFormatted<int>(i);
						defaultInterpolatedStringHandler.AppendLiteral(" -skipintro");
						string clientArguments = defaultInterpolatedStringHandler.ToStringAndClear();
						Process.Start("Barotrauma.exe", clientArguments + " " + extraArguments);
					}
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to start the local MP test session", e, null, false, false);
			}
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x0005B270 File Offset: 0x00059470
		private static void InitShowSoldItems()
		{
			DebugConsole.commands.Add(new DebugConsole.Command("showsolditems", "showsolditems [filter (no-defined/only-min/only-max/name:pattern)] [Include stores (true/false)] [Sold only (true/false)] [limit (number)] [Hide store overrides from output. (true/false)]: Lists items and their shop availability settings. Filter can be availability filter or name pattern (e.g. 'name:*rifle*'). Include stores controls whether to check store-specific overrides (default true). Sold only controls whether to show only sold items (default true). Limit parameter controls how many items to show (default 50). Hide store overrides from output, defaults to false.", delegate(string[] args)
			{
				string filter = (args.Length != 0) ? args[0].ToLowerInvariant() : null;
				bool includeStores = args.Length <= 1 || !args[1].Equals("false", StringComparison.InvariantCultureIgnoreCase);
				bool soldOnly = args.Length <= 2 || !args[2].Equals("false", StringComparison.InvariantCultureIgnoreCase);
				int limit = 50;
				int parsedLimit;
				if (args.Length > 3 && int.TryParse(args[3], out parsedLimit))
				{
					limit = Math.Max(1, parsedLimit);
				}
				bool hideStoreOverrides = args.Length > 4 && args[4].Equals("true", StringComparison.InvariantCultureIgnoreCase);
				IEnumerable<ItemPrefab> itemsWithPrice = from item in ItemPrefab.Prefabs
				where item.ConfigElement.Element.Element("Price") != null
				select item;
				List<ItemPrefab> matchingItems = (from i in itemsWithPrice
				orderby i.Name.Value
				select i into item
				where DebugConsole.MatchesFilter(item, filter, includeStores, soldOnly)
				select item).ToList<ItemPrefab>();
				DebugConsole.NewMessage("=== Shop Item Availability ===", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Filter: ");
				defaultInterpolatedStringHandler.AppendFormatted(filter ?? "all");
				defaultInterpolatedStringHandler.AppendLiteral(", IncludeStores: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(includeStores);
				defaultInterpolatedStringHandler.AppendLiteral(", SoldOnly: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(soldOnly);
				defaultInterpolatedStringHandler.AppendLiteral(", Limit: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(limit);
				defaultInterpolatedStringHandler.AppendLiteral(", HideStoreOverrides: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(hideStoreOverrides);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Items: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(matchingItems.Count);
				defaultInterpolatedStringHandler2.AppendLiteral(" matching out of ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(itemsWithPrice.Count<ItemPrefab>());
				defaultInterpolatedStringHandler2.AppendLiteral(" being sold (showing first ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(Math.Min(limit, matchingItems.Count));
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.LightGreen), false);
				DebugConsole.NewMessage("", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				foreach (ItemPrefab item2 in matchingItems.Take(limit))
				{
					DebugConsole.PrintItemInfo(item2, hideStoreOverrides);
				}
			}, () => new string[][]
			{
				new string[]
				{
					"all",
					"no-defined",
					"only-min",
					"only-max",
					"name:*"
				},
				new string[]
				{
					"true",
					"false"
				},
				new string[]
				{
					"true",
					"false"
				},
				new string[]
				{
					"10",
					"25",
					"50",
					"100"
				},
				new string[]
				{
					"false",
					"true"
				}
			}, false));
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x0005B2D8 File Offset: 0x000594D8
		private static bool MatchesFilter(ItemPrefab item, string filter, bool includeStores, bool soldOnly)
		{
			XElement priceElement = item.ConfigElement.Element.Element("Price");
			if (priceElement == null)
			{
				return false;
			}
			if (!includeStores)
			{
				return DebugConsole.MatchesPriceElement(priceElement, item, filter, soldOnly);
			}
			if (DebugConsole.MatchesPriceElement(priceElement, item, filter, soldOnly))
			{
				return true;
			}
			foreach (XElement storeElement in from e in priceElement.Elements()
			where e.Name == "Price"
			select e)
			{
				if (DebugConsole.MatchesPriceElement(storeElement, item, filter, soldOnly))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x0005B390 File Offset: 0x00059590
		private static bool MatchesPriceElement(XElement priceEl, ItemPrefab itemPrefab, string filter, bool soldOnly)
		{
			bool isSold = PriceInfo.GetSold(priceEl, true);
			if (soldOnly && !isSold)
			{
				return false;
			}
			if (filter == null)
			{
				return true;
			}
			if (!filter.StartsWith("name:"))
			{
				bool hasMin = PriceInfo.HasMinAmountDefined(priceEl);
				bool hasMax = PriceInfo.HasMaxAmountDefined(priceEl);
				bool result;
				if (!(filter == "no-defined"))
				{
					if (!(filter == "only-min"))
					{
						result = (!(filter == "only-max") || (!hasMin && hasMax));
					}
					else
					{
						result = (hasMin && !hasMax);
					}
				}
				else
				{
					result = (!hasMin && !hasMax);
				}
				return result;
			}
			string pattern = filter.Substring(5);
			string name = itemPrefab.Name.Value.ToLowerInvariant();
			string identifier = itemPrefab.Identifier.Value.ToLowerInvariant();
			if (pattern.Contains('*'))
			{
				string regexPattern = Regex.Escape(pattern).Replace("\\*", ".*");
				return Regex.IsMatch(name, "^" + regexPattern + "$") || Regex.IsMatch(identifier, "^" + regexPattern + "$");
			}
			return name == pattern || identifier == pattern;
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0005B4BC File Offset: 0x000596BC
		private static void PrintItemInfo(ItemPrefab item, bool hideStoreOverrides = false)
		{
			XElement priceElement = item.ConfigElement.Element.Element("Price");
			if (priceElement == null)
			{
				return;
			}
			bool hasMinDefined = PriceInfo.HasMinAmountDefined(priceElement);
			bool hasMaxDefined = PriceInfo.HasMaxAmountDefined(priceElement);
			string minRaw = PriceInfo.GetMinAmountString(priceElement);
			string maxRaw = PriceInfo.GetMaxAmountString(priceElement);
			int minLevelDifficulty = PriceInfo.GetMinLevelDifficulty(priceElement, 0);
			PriceInfo priceInfo = new PriceInfo(priceElement);
			int resolvedMin = priceInfo.MinAvailableAmount;
			int resolvedMax = priceInfo.MaxAvailableAmount;
			string minStatus = hasMinDefined ? ("XML:" + minRaw) : "DEFAULT:1";
			string maxStatus = hasMaxDefined ? ("XML:" + maxRaw) : "DEFAULT:5";
			string text;
			if (minLevelDifficulty <= 0)
			{
				text = "";
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral(" | MinLvl: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(minLevelDifficulty);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string minLevelInfo = text;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(25, 7);
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(item.Name);
			defaultInterpolatedStringHandler2.AppendLiteral(" (");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(item.Identifier);
			defaultInterpolatedStringHandler2.AppendLiteral(") | Min: ");
			defaultInterpolatedStringHandler2.AppendFormatted(minStatus);
			defaultInterpolatedStringHandler2.AppendLiteral(" → ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(resolvedMin);
			defaultInterpolatedStringHandler2.AppendLiteral(" | Max: ");
			defaultInterpolatedStringHandler2.AppendFormatted(maxStatus);
			defaultInterpolatedStringHandler2.AppendLiteral(" → ");
			defaultInterpolatedStringHandler2.AppendFormatted<int>(resolvedMax);
			defaultInterpolatedStringHandler2.AppendFormatted(minLevelInfo);
			DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			if (hideStoreOverrides)
			{
				return;
			}
			List<string> storeOverrides = (from s in (from e in priceElement.Elements()
			where e.Name == "Price"
			select e).Select(delegate(XElement p)
			{
				string storeId = PriceInfo.GetStoreIdentifier(p, "unknown");
				string storeMin = PriceInfo.GetMinAmountString(p);
				string storeMax = PriceInfo.GetMaxAmountString(p);
				bool? storeSold = PriceInfo.HasSoldDefined(p) ? new bool?(PriceInfo.GetSold(p, true)) : null;
				if (storeMin != null || storeMax != null || storeSold != null)
				{
					List<string> parts = new List<string>();
					if (storeMin != null || storeMax != null)
					{
						parts.Add("min:" + (storeMin ?? "base") + ", max:" + (storeMax ?? "base"));
					}
					if (storeSold != null)
					{
						parts.Add("sold:" + storeSold.Value.ToString().ToLowerInvariant());
					}
					return storeId + "(" + string.Join(", ", parts) + ")";
				}
				return null;
			})
			where s != null
			select s).ToList<string>();
			if (storeOverrides.Count != 0)
			{
				DebugConsole.NewMessage("  Store overrides: " + string.Join(", ", storeOverrides), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Gray), false);
			}
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x0005B6DC File Offset: 0x000598DC
		private static void ShowHelpMessage(DebugConsole.Command command)
		{
			if (DebugConsole.listBox.Content.CountChildren > 300)
			{
				DebugConsole.listBox.RemoveChild(DebugConsole.listBox.Content.Children.First<GUIComponent>());
			}
			GUIFrame textContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0f), DebugConsole.listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "InnerFrame", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White * 0.6f))
			{
				CanBeFocused = false
			};
			RectTransform rectTransform = new RectTransform(new Point(DebugConsole.listBox.Content.Rect.Width - 170, 0), textContainer.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false);
			rectTransform.AbsoluteOffset = new Point(20, 0);
			RichString text = command.Help;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.TopLeft, true, "", null)
			{
				CanBeFocused = false,
				TextColor = Microsoft.Xna.Framework.Color.White
			};
			textContainer.RectTransform.NonScaledSize = new Point(textContainer.RectTransform.NonScaledSize.X, textBlock.RectTransform.NonScaledSize.Y + 5);
			textBlock.SetTextPos();
			new GUITextBlock(new RectTransform(new Point(150, textContainer.Rect.Height), textContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), command.Names[0].Value, null, null, Alignment.TopLeft, false, "", null);
			DebugConsole.listBox.UpdateScrollBarSize();
			DebugConsole.listBox.BarScroll = 1f;
			DebugConsole.chatManager.Clear();
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x060009BD RID: 2493 RVA: 0x0005B8D4 File Offset: 0x00059AD4
		public static List<DebugConsole.Command> Commands
		{
			get
			{
				return DebugConsole.commands;
			}
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x0005B8DC File Offset: 0x00059ADC
		public static void AssignOnExecute(string names, Action<string[]> onExecute)
		{
			DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Intersect(names.Split('|', StringSplitOptions.None).ToIdentifiers()).Any<Identifier>());
			if (matchingCommand == null)
			{
				throw new Exception("AssignOnExecute failed. Command matching the name(s) \"" + names + "\" not found.");
			}
			matchingCommand.OnExecute = onExecute;
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x0005B934 File Offset: 0x00059B34
		unsafe static DebugConsole()
		{
			DebugConsole.commands.Add(new DebugConsole.Command("help", "", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					using (List<DebugConsole.Command>.Enumerator enumerator = DebugConsole.commands.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							DebugConsole.Command c2 = enumerator.Current;
							if (!string.IsNullOrEmpty(c2.Help))
							{
								DebugConsole.ShowHelpMessage(c2);
							}
						}
						return;
					}
				}
				Func<Identifier, bool> <>9__123;
				DebugConsole.Command matchingCommand = DebugConsole.commands.Find(delegate(DebugConsole.Command c)
				{
					ImmutableArray<Identifier> names = c.Names;
					Func<Identifier, bool> predicate;
					if ((predicate = <>9__123) == null)
					{
						predicate = (<>9__123 = ((Identifier name) => name == args[0]));
					}
					return names.Any(predicate);
				});
				if (matchingCommand == null)
				{
					DebugConsole.NewMessage("Command " + args[0] + " not found.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				DebugConsole.ShowHelpMessage(matchingCommand);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from n in DebugConsole.commands.SelectMany((DebugConsole.Command c) => c.Names)
				select n.Value).ToArray<string>();
				array[1] = Array.Empty<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("items|itemlist", "itemlist: List all the item prefabs available for spawning.", delegate(string[] args)
			{
				DebugConsole.<.cctor>g__printMapEntityPrefabs|54_2<ItemPrefab>(ItemPrefab.Prefabs);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("itemassemblies", "itemassemblies: List all the item assemblies available for spawning.", delegate(string[] args)
			{
				DebugConsole.<.cctor>g__printMapEntityPrefabs|54_2<ItemAssemblyPrefab>(ItemAssemblyPrefab.Prefabs);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("netstats", "netstats: Toggles the visibility of the network statistics UI.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null)
				{
					return;
				}
				GameMain.NetworkMember.ShowNetStats = !GameMain.NetworkMember.ShowNetStats;
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("spawn|spawncharacter", "spawn [creaturename/jobname] [near/inside/outside/cursor] [team] [add to crew (true/false)] [name]: Spawn a creature at a random spawnpoint (use the second parameter to only select spawnpoints near/inside/outside the submarine). You can also enter the name of a job (e.g. \"Mechanic\") to spawn a character with a specific job and the appropriate equipment.", null, delegate()
			{
				string[] creatureAndJobNames = (from s in (from p in CharacterPrefab.Prefabs
				select p.Identifier.Value).Concat(from p in JobPrefab.Prefabs
				select p.Identifier.Value)
				orderby s
				select s).ToArray<string>();
				string[][] array = new string[4][];
				array[0] = creatureAndJobNames.ToArray<string>();
				array[1] = new string[]
				{
					"near",
					"inside",
					"outside",
					"cursor"
				};
				array[2] = (from v in Enum.GetValues<CharacterTeamType>()
				select v.ToString()).ToArray<string>();
				array[3] = new string[]
				{
					"true",
					"false"
				};
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("give|giveitem", "give|giveitem [itemname/itemidentifier] [amount] [condition] [quality]: Spawn an item in the inventory of the controlled character", delegate(string[] args)
			{
				if (Character.Controlled == null)
				{
					DebugConsole.ThrowError("No character is selected!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please give the name or identifier of the item to spawn.", null, null, false, false);
					return;
				}
				List<string> modifiedArgs = new List<string>(args);
				modifiedArgs.Insert(1, "inventory");
				DebugConsole.TrySpawnItem(modifiedArgs.ToArray());
			}, () => new string[][]
			{
				DebugConsole.GetItemNameOrIdParams().ToArray<string>(),
				new string[]
				{
					"1"
				},
				new string[]
				{
					"100"
				},
				DebugConsole.ItemQualityNames.ToArray<string>()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("spawnnpc", "spawnnpc [any/npcsetidentifier] [npcidentifier] [near/inside/outside/cursor] [team (0-3)] [add to crew (true/false)]: Spawns an pre-configured NPC at a random spawnpoint. (Use the third parameter to select a specific set of spawnpoints.)", null, delegate()
			{
				string[][] array = new string[5][];
				array[0] = "any".ToEnumerable<string>().Union(from p in NPCSet.Sets
				select p.Identifier.Value into s
				orderby s
				select s).ToArray<string>();
				array[1] = (from p in NPCSet.Sets.SelectMany((NPCSet set) => set.Humans)
				select p.Identifier.Value into s
				orderby s
				select s).ToArray<string>();
				array[2] = new string[]
				{
					"near",
					"inside",
					"outside",
					"cursor"
				};
				array[3] = (from v in Enum.GetValues<CharacterTeamType>()
				select v.ToString()).ToArray<string>();
				array[4] = new string[]
				{
					"true",
					"false"
				};
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("spawnitem", "spawnitem [itemname/itemidentifier] [cursor/inventory/cargo/random/[name]] [amount] [condition] [quality]: Spawn an item at the position of the cursor, in the inventory of the controlled character, in the inventory of the client with the given name, or at a random spawnpoint if the location parameter is omitted or \"random\".", delegate(string[] args)
			{
				DebugConsole.TrySpawnItem(args);
			}, () => new string[][]
			{
				DebugConsole.GetItemNameOrIdParams().ToArray<string>(),
				DebugConsole.GetSpawnPosParams().ToArray<string>(),
				new string[]
				{
					"1"
				},
				new string[]
				{
					"100"
				},
				DebugConsole.ItemQualityNames.ToArray<string>()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("disablecrewai", "disablecrewai: Disable the AI of the NPCs in the crew.", delegate(string[] args)
			{
				HumanAIController.DisableCrewAI = true;
				DebugConsole.NewMessage("Crew AI disabled", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("enablecrewai", "enablecrewai: Enable the AI of the NPCs in the crew.", delegate(string[] args)
			{
				HumanAIController.DisableCrewAI = false;
				DebugConsole.NewMessage("Crew AI enabled", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("disableenemyai", "disableenemyai: Disable the AI of the Enemy characters (monsters).", delegate(string[] args)
			{
				EnemyAIController.DisableEnemyAI = true;
				DebugConsole.NewMessage("Enemy AI disabled", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("enableenemyai", "enableenemyai: Enable the AI of the Enemy characters (monsters).", delegate(string[] args)
			{
				EnemyAIController.DisableEnemyAI = false;
				DebugConsole.NewMessage("Enemy AI enabled", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("triggertraitorevent|starttraitoreventimmediately", "triggertraitorevent [eventidentifier]: Skip the initial delay of the traitor events and start one immediately. You can optionally specify which event to start (otherwise a random event is chosen).", null, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from p in EventPrefab.Prefabs
				where p is TraitorEventPrefab
				select p.Identifier.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("botcount", "botcount [x]: Set the number of bots in the crew in multiplayer.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("botspawnmode", "botspawnmode [fill/normal]: Set how bots are spawned in the multiplayer.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("killdisconnectedtimer", "killdisconnectedtimer [seconds]: Set the time after which disconnect players' characters get automatically killed.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestart", "autorestart [true/false]: Enable or disable round auto-restart.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestartinterval", "autorestartinterval [seconds]: Set how long the server waits between rounds before automatically starting a new one. If set to 0, autorestart is disabled.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autorestarttimer", "autorestarttimer [seconds]: Set the current autorestart countdown to the specified value.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("startwhenclientsready", "startwhenclientsready [true/false]: Enable or disable automatically starting the round when clients are ready to start.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giveperm", "giveperm [id/steamid/endpoint/name]: Grants administrative permissions to the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from ClientPermissions v in Enum.GetValues(typeof(ClientPermissions))
				select v.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("revokeperm", "revokeperm [id/steamid/endpoint/name]: Revokes administrative permissions from the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from ClientPermissions v in Enum.GetValues(typeof(ClientPermissions))
				select v.ToString()).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giverank", "giverank [id/steamid/endpoint/name]: Assigns a specific rank (= a set of administrative permissions) to the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from pp in PermissionPreset.List
				select pp.DisplayName.Value).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("givecommandperm", "givecommandperm [id/steamid/endpoint/name]: Gives the specified client the permission to use the specified console commands.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from c in DebugConsole.commands
				select c.Names.First<Identifier>().Value).Union(new string[]
				{
					"All"
				}).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("revokecommandperm", "revokecommandperm [id/steamid/endpoint/name]: Revokes permission to use the specified console commands from the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from c in DebugConsole.commands
				select c.Names.First<Identifier>().Value).Union(new string[]
				{
					"All"
				}).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showperm", "showperm [id/steamid/endpoint/name]: Shows the current administrative permissions of the specified client.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("respawnnow", "respawnnow: Trigger a respawn immediately if there are any clients waiting to respawn.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("showkarma", "showkarma: Show the current karma values of the players.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglekarma", "togglekarma: Toggle the karma system on/off.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("resetkarma", "resetkarma [client]: Resets the karma value of the specified client to 100.", null, delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ConnectedClients : null) == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setkarma", "setkarma [client] [0-100]: Sets the karma of the specified client to the specified value.", null, delegate()
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ConnectedClients : null) == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = new string[]
				{
					"50"
				};
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglekarmatestmode", "togglekarmatestmode: Toggle the karma test mode on/off. When test mode is enabled, clients get notified when their karma value changes (including the reason for the increase/decrease) and the server doesn't ban clients whose karma decreases below the ban threshold.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("kick", "kick [name]: Kick a player out of the server.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				string playerName = string.Join(" ", args);
				DebugConsole.ShowQuestionPrompt("Reason for kicking \"" + playerName + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					GameMain.NetworkMember.KickPlayer(playerName, reason);
				}, null, -1);
			}, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("kickid", "kickid [id]: Kick the player with the specified client ID out of the server.  You can see the IDs of the clients using the command \"clientlist\".", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				int id;
				int.TryParse(args[0], out id);
				Client client = GameMain.NetworkMember.ConnectedClients.Find((Client c) => (int)c.SessionId == id);
				if (client == null)
				{
					DebugConsole.ThrowError("Client id \"" + id.ToString() + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Reason for kicking \"" + client.Name + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					GameMain.NetworkMember.KickPlayer(client.Name, reason);
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("ban", "ban [name]: Kick and ban the player from the server.", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				string clientName = string.Join(" ", args);
				DebugConsole.ShowQuestionPrompt("Reason for banning \"" + clientName + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (Enter c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						GameMain.NetworkMember.BanPlayer(clientName, reason, banDuration);
					}, null, -1);
				}, null, -1);
			}, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[1][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("banid", "banid [id]: Kick and ban the player with the specified client ID from the server. You can see the IDs of the clients using the command \"clientlist\".", delegate(string[] args)
			{
				if (GameMain.NetworkMember == null || args.Length == 0)
				{
					return;
				}
				int id;
				int.TryParse(args[0], out id);
				Client client = GameMain.NetworkMember.ConnectedClients.Find((Client c) => (int)c.SessionId == id);
				if (client == null)
				{
					DebugConsole.ThrowError("Client id \"" + id.ToString() + "\" not found.", null, null, false, false);
					return;
				}
				DebugConsole.ShowQuestionPrompt("Reason for banning \"" + client.Name + "\"? (Enter c to cancel)", delegate(string reason)
				{
					if (reason == "c" || reason == "C")
					{
						return;
					}
					DebugConsole.ShowQuestionPrompt("Enter the duration of the ban (leave empty to ban permanently, or use the format \"[days] d [hours] h\") (c to cancel)", delegate(string duration)
					{
						if (duration == "c" || duration == "C")
						{
							return;
						}
						TimeSpan? banDuration = null;
						if (!string.IsNullOrWhiteSpace(duration))
						{
							TimeSpan parsedBanDuration;
							if (!DebugConsole.TryParseTimeSpan(duration, out parsedBanDuration))
							{
								DebugConsole.ThrowError("\"" + duration + "\" is not a valid ban duration. Use the format \"[days] d [hours] h\", \"[days] d\" or \"[hours] h\".", null, null, false, false);
								return;
							}
							banDuration = new TimeSpan?(parsedBanDuration);
						}
						GameMain.NetworkMember.BanPlayer(client.Name, reason, banDuration);
					}, null, -1);
				}, null, -1);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("banaddress|banip", "banaddress [endpoint]: Ban the IP address/SteamID from the server.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("teleportcharacter|teleport", "teleport [character name] [location]: Teleport the specified character to a location , or the position of the cursor if location is omitted. If the name parameter is omitted, the controlled character will be teleported.", null, () => new string[][]
			{
				DebugConsole.ListCharacterNames(Character.Controlled != null, true),
				DebugConsole.ListAvailableLocations()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersignoreplayer", "Toggle if monsters should ignore the player character (and their equipment) when targeting.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.PlayerCharacters);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersignoresub", "Toggle if monsters should ignore the player submarines when targeting.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.PlayerSubmarines);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstersrestoretargets", "Remove any targeting restrictions from monsters.", delegate(string[] args)
			{
				DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions.None);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("monstertargetingrestrictions", "monstertargetingrestrictions [restrictions]: Set targeting restrictions for all monsters. Supports multiple options comma-separated: 'monsterargetingrestrictions PlayerCharacters,PlayerSubmarines'. Use 'None' to remove all restrictions.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ToggleEnemyAITargetingRestrictions(EnemyAIController.TargetingRestrictions);
					return;
				}
				EnemyTargetingRestrictions restrictions;
				if (Enum.TryParse<EnemyTargetingRestrictions>(args[0], true, out restrictions))
				{
					DebugConsole.ToggleEnemyAITargetingRestrictions(restrictions);
					return;
				}
				DebugConsole.NewMessage("Failed to parse argument '" + args[0] + "'", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, () => new string[][]
			{
				Enum.GetNames(typeof(EnemyTargetingRestrictions))
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("listlocations|locations", "listlocations: List all the locations in the level: subs, outposts, ruins, caves.", delegate(string[] args)
			{
				string[] availableLocations = DebugConsole.ListAvailableLocations();
				DebugConsole.NewMessage("***************", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
				foreach (string location in availableLocations)
				{
					DebugConsole.NewMessage(location, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
				}
				DebugConsole.NewMessage("***************", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("godmode", "godmode [character name] [remove afflictions (true/false)]: Toggle character godmode. Makes the targeted character invulnerable to damage. If the name parameter is omitted, the controlled character will receive godmode.", delegate(string[] args)
			{
				DebugConsole.<>c__DisplayClass54_7 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass54_7();
				CS$<>8__locals1.args = args;
				CS$<>8__locals1.godmodeStateOnFirstCharacter = null;
				DebugConsole.HandleCommandForCrewOrSingleCharacter(CS$<>8__locals1.args, new Action<Character>(CS$<>8__locals1.<.cctor>g__ToggleGodMode|161), null);
			}, () => new string[][]
			{
				DebugConsole.ListCharacterNames(Character.Controlled != null, true)
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("godmode_mainsub", "godmode_mainsub: Toggle submarine godmode. Makes the main submarine invulnerable to damage.", delegate(string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				Submarine.MainSub.GodMode = !Submarine.MainSub.GodMode;
				DebugConsole.NewMessage(Submarine.MainSub.GodMode ? "Godmode on" : "Godmode off", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("growthdelay", "growthdelay: Sets how long it takes for planters to attempt to advance a plant's growth.", delegate(string[] args)
			{
				float value;
				if (args.Length != 0 && float.TryParse(args[0], out value))
				{
					Planter.GrowthTickDelay = value;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Growth delay set to ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(value);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
					return;
				}
				DebugConsole.NewMessage("Invalid value.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lock", "lock: Lock movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockX = !Submarine.LockX;
				Submarine.LockY = Submarine.LockX;
				DebugConsole.NewMessage(Submarine.LockX ? "Submarine movement locked." : "Submarine movement unlocked.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lockx", "lockx: Lock horizontal movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockX = !Submarine.LockX;
				DebugConsole.NewMessage(Submarine.LockX ? "Horizontal submarine movement locked." : "Horizontal submarine movement unlocked.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("locky", "locky: Lock vertical movement of the main submarine.", delegate(string[] args)
			{
				Submarine.LockY = !Submarine.LockY;
				DebugConsole.NewMessage(Submarine.LockY ? "Vertical submarine movement locked." : "Vertical submarine movement unlocked.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("dumpids", "", delegate(string[] args)
			{
				try
				{
					int count = (args.Length == 0) ? 10 : int.Parse(args[0]);
					Entity.DumpIds(count, (args.Length >= 2) ? args[1] : null);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to dump ids", e, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("dumptofile", "findentityids [filename]: Outputs the contents of the debug console into a text file in the game folder. If the filename argument is omitted, \"consoleOutput.txt\" is used as the filename.", delegate(string[] args)
			{
				string filename = "consoleOutput.txt";
				if (args.Length != 0)
				{
					filename = string.Join(" ", args);
				}
				File.WriteAllLines(filename, (from m in DebugConsole.Messages
				select m.Text).ToArray<string>(), null, true);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("findentityids", "findentityids [entityname]", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				foreach (MapEntity mapEntity in MapEntity.MapEntityList)
				{
					if (mapEntity.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
					{
						DebugConsole.ThrowError(mapEntity.ID.ToString() + ": " + mapEntity.Name.ToString(), null, null, false, false);
					}
				}
				foreach (Character character in Character.CharacterList)
				{
					if (!character.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase))
					{
						Identifier speciesName = character.SpeciesName;
						if (!(speciesName == args[0]))
						{
							continue;
						}
					}
					DebugConsole.ThrowError(character.ID.ToString() + ": " + character.Name.ToString(), null, null, false, false);
				}
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("giveaffliction", "giveaffliction [affliction name] [affliction strength] [character name] [limb type] [use relative strength]: Add an affliction to a character. If the name parameter is omitted, the affliction is added to the controlled character.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					if (args.Length == 1)
					{
						DebugConsole.ThrowError("Must give a strength value!", null, null, false, false);
					}
					return;
				}
				string affliction = args[0];
				AfflictionPrefab afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Identifier == affliction);
				if (afflictionPrefab == null)
				{
					afflictionPrefab = AfflictionPrefab.List.FirstOrDefault((AfflictionPrefab a) => a.Name.Equals(affliction, StringComparison.OrdinalIgnoreCase));
				}
				if (afflictionPrefab == null)
				{
					DebugConsole.ThrowError("Affliction \"" + affliction + "\" not found.", null, null, false, false);
					return;
				}
				float afflictionStrength;
				if (!float.TryParse(args[1], out afflictionStrength))
				{
					DebugConsole.ThrowError("\"" + args[1] + "\" is not a valid affliction strength.", null, null, false, false);
					return;
				}
				bool relativeStrength = false;
				if (args.Length > 4)
				{
					bool.TryParse(args[4], out relativeStrength);
				}
				Character targetCharacter = (args.Length <= 2) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false);
				if (targetCharacter != null)
				{
					Limb targetLimb = targetCharacter.AnimController.MainLimb;
					if (args.Length > 3)
					{
						targetLimb = targetCharacter.AnimController.Limbs.FirstOrDefault((Limb l) => l.type.ToString().Equals(args[3], StringComparison.OrdinalIgnoreCase));
					}
					if (relativeStrength)
					{
						afflictionStrength *= targetCharacter.MaxVitality / afflictionPrefab.MaxStrength;
					}
					targetCharacter.CharacterHealth.ApplyAffliction(targetLimb ?? targetCharacter.AnimController.MainLimb, afflictionPrefab.Instantiate(afflictionStrength, null), true, false, true);
				}
			}, delegate()
			{
				string[][] array = new string[4][];
				array[0] = (from a in AfflictionPrefab.Prefabs
				select a.Name.Value).ToArray<string>().Concat(from a in AfflictionPrefab.Prefabs
				select a.Identifier.Value).ToArray<string>();
				array[1] = new string[]
				{
					"1"
				};
				array[2] = DebugConsole.ListCharacterNames(false, false);
				array[3] = Enum.GetNames(typeof(LimbType)).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("healme", "healme [all]: Restore controlled character to full health. By default only heals common afflictions such as physical damage and blood loss: use the \"all\" argument to heal everything, including poisonings/addictions/etc.", delegate(string[] args)
			{
				bool healAll = args.Length != 0 && args[0].Equals("all", StringComparison.OrdinalIgnoreCase);
				if (Character.Controlled != null)
				{
					DebugConsole.HealCharacter(Character.Controlled, healAll, null);
				}
			}, () => new string[][]
			{
				new string[]
				{
					"all"
				}
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("heal", "heal [character name] [all]: Restore the specified character to full health. If the name parameter is omitted, the controlled character will be healed. By default only heals common afflictions such as physical damage and blood loss: use the \"all\" argument to heal everything, including poisonings/addictions/etc.", delegate(string[] args)
			{
				bool healAll = args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase);
				DebugConsole.HandleCommandForCrewOrSingleCharacter(args, delegate(Character targetCharacter)
				{
					DebugConsole.HealCharacter(targetCharacter, healAll, null);
				}, null);
			}, () => new string[][]
			{
				DebugConsole.ListCharacterNames(true, true),
				new string[]
				{
					"all"
				}
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("listsuitabletreatments", "listsuitabletreatments [character name]: List which items are the most suitable for treating the specified character. Useful for debugging medic AI.", delegate(string[] args)
			{
				Character character = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (character != null)
				{
					Dictionary<Identifier, float> treatments = new Dictionary<Identifier, float>();
					character.CharacterHealth.GetSuitableTreatments(treatments, null, null, false, true, false, 0f);
					foreach (KeyValuePair<Identifier, float> treatment in from t in treatments
					orderby t.Value descending
					select t)
					{
						Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.White;
						color = ToolBox.GradientLerp(MathUtils.InverseLerp(-1000f, 1000f, treatment.Value), new Microsoft.Xna.Framework.Color[]
						{
							Microsoft.Xna.Framework.Color.Red,
							Microsoft.Xna.Framework.Color.Yellow,
							Microsoft.Xna.Framework.Color.White,
							Microsoft.Xna.Framework.Color.LightGreen
						});
						DebugConsole.NewMessage(((int)treatment.Value).ToString() + ": " + treatment.Key.ToString(), new Microsoft.Xna.Framework.Color?(color), false);
					}
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("revive", "revive [character name]: Bring the specified character back from the dead. If the name parameter is omitted, the controlled character will be revived.", delegate(string[] args)
			{
				Character revivedCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (revivedCharacter == null)
				{
					return;
				}
				revivedCharacter.Revive(true, false);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("freeze", "", delegate(string[] args)
			{
				if (Character.Controlled != null)
				{
					Character.Controlled.AnimController.Frozen = !Character.Controlled.AnimController.Frozen;
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ragdoll", "ragdoll [character name]: Force-ragdoll the specified character. If the name parameter is omitted, the controlled character will be ragdolled.", delegate(string[] args)
			{
				Character ragdolledCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (ragdolledCharacter != null)
				{
					ragdolledCharacter.IsForceRagdolled = !ragdolledCharacter.IsForceRagdolled;
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("freecamera|freecam", "freecam: Detach the camera from the controlled character.", delegate(string[] args)
			{
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					return;
				}
				if (GameMain.Client == null)
				{
					if (Character.Controlled != null)
					{
						DebugConsole.previousControlledCharacter = new WeakReference<Character>(Character.Controlled);
						Character.Controlled = null;
						GameMain.GameScreen.Cam.TargetPos = Vector2.Zero;
						DebugConsole.NewMessage("Entering freecam mode", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
						return;
					}
					Character prevCharacter = null;
					if (DebugConsole.previousControlledCharacter != null && DebugConsole.previousControlledCharacter.TryGetTarget(out prevCharacter) && prevCharacter != null && !prevCharacter.IsDead && !prevCharacter.Removed)
					{
						Character.Controlled = prevCharacter;
						DebugConsole.NewMessage("Exiting freecam mode", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
						return;
					}
					DebugConsole.NewMessage("Could not regain control of the previous character (dead or removed).", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				else
				{
					GameClient client = GameMain.Client;
					if (client == null)
					{
						return;
					}
					client.SendConsoleCommand("freecam");
					return;
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("eventmanager", "eventmanager: Toggle event manager on/off. No new random events are created when the event manager is disabled.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null)
				{
					GameMain.GameSession.EventManager.Enabled = !GameMain.GameSession.EventManager.Enabled;
					DebugConsole.NewMessage(GameMain.GameSession.EventManager.Enabled ? "Event manager on" : "Event manager off", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("triggerevent", "triggerevent [identifier]: Trigger an event based on identifier.", delegate(string[] args)
			{
				List<EventPrefab> allEventPrefabsWithId = (from prefab in EventSet.GetAllEventPrefabs()
				where prefab.Identifier != Identifier.Empty
				select prefab).ToList<EventPrefab>();
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.EventManager : null) != null && args.Length != 0)
				{
					string eventPrefabId = args[0];
					if (eventPrefabId == "all")
					{
						using (IEnumerator<EventPrefab> enumerator = (from e in allEventPrefabsWithId
						where e.EventType == typeof(ScriptedEvent)
						select e).GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								EventPrefab eventPrefab = enumerator.Current;
								Event newEvent = eventPrefab.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
								if (newEvent == null)
								{
									DebugConsole.NewMessage("Could not initialize event " + eventPrefabId + " because level did not meet requirements", null, false);
									return;
								}
								GameMain.GameSession.EventManager.ActivateEvent(newEvent);
							}
							goto IL_21F;
						}
					}
					EventPrefab eventPrefab2 = allEventPrefabsWithId.Find((EventPrefab prefab) => prefab.Identifier == eventPrefabId);
					if (eventPrefab2 is TraitorEventPrefab)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 1);
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(eventPrefab2.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(" is a traitor event. You need to use the 'triggertraitorevent' command to start it.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						return;
					}
					if (eventPrefab2 == null)
					{
						DebugConsole.NewMessage("Failed to trigger event because " + eventPrefabId + " is not a valid event identifier.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
						return;
					}
					Event newEvent2 = eventPrefab2.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
					if (newEvent2 == null)
					{
						DebugConsole.NewMessage("Could not initialize event " + eventPrefabId + " because level did not meet requirements", null, false);
						return;
					}
					GameMain.GameSession.EventManager.ActivateEvent(newEvent2);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Initialized event ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(eventPrefab2.Identifier);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Aqua), false);
					return;
				}
				IL_21F:
				DebugConsole.NewMessage("Failed to trigger event", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
			}, delegate()
			{
				List<EventPrefab> eventPrefabs = (from prefab in EventSet.GetAllEventPrefabs()
				where prefab.Identifier != Identifier.Empty
				select prefab).ToList<EventPrefab>();
				string[][] array = new string[1][];
				array[0] = (from id in (from prefab in eventPrefabs
				select prefab.Identifier).Distinct<Identifier>()
				select id.Value).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugevent", "debugevent [identifier]: outputs debug info about a specific event that's currently active. Mainly intended for debugging events in multiplayer: in single player, the same information is available by enabling debugdraw.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please specify the identifier of the event you want to debug.", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				EventManager eventManager = (gameSession != null) ? gameSession.EventManager : null;
				if (eventManager != null)
				{
					Event ev2 = eventManager.ActiveEvents.FirstOrDefault(delegate(Event ev)
					{
						EventPrefab prefab = ev.Prefab;
						Identifier? identifier;
						Identifier? identifier2;
						if (prefab == null)
						{
							identifier = null;
							identifier2 = identifier;
						}
						else
						{
							identifier2 = new Identifier?(prefab.Identifier);
						}
						identifier = identifier2;
						return identifier == args[0];
					});
					if (ev2 == null)
					{
						DebugConsole.ThrowError("Event \"" + args[0] + "\" not found.", null, null, false, false);
						return;
					}
					string info = ev2.GetDebugInfo();
					DebugConsole.NewMessage(info, null, false);
				}
			}, delegate()
			{
				GameSession gameSession = GameMain.GameSession;
				IEnumerable<EventPrefab> eventPrefabs;
				if (((gameSession != null) ? gameSession.EventManager : null) == null || GameMain.GameSession.EventManager.ActiveEvents.None(null))
				{
					eventPrefabs = from prefab in EventSet.GetAllEventPrefabs()
					where prefab.Identifier != Identifier.Empty
					select prefab;
				}
				else
				{
					eventPrefabs = from e in GameMain.GameSession.EventManager.ActiveEvents
					select e.Prefab;
				}
				string[][] array = new string[1][];
				array[0] = ((from ev in eventPrefabs
				select ev.Identifier.ToString()).ToArray<string>() ?? Array.Empty<string>());
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("unlockmission", "unlockmission [identifier/tag]: Unlocks a mission in a random adjacent level.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("The unlockmission command is only usable in the campaign mode.", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("Please enter the identifier or a tag of the mission you want to unlock.", null, null, false, false);
					return;
				}
				Location currentLocation = campaign.Map.CurrentLocation;
				if (MissionPrefab.Prefabs.Any((MissionPrefab p) => p.Identifier == args[0]))
				{
					currentLocation.UnlockMissionByIdentifier(args[0].ToIdentifier(), null);
				}
				else
				{
					currentLocation.UnlockMissionByTag(args[0].ToIdentifier(), null, null);
				}
				MultiPlayerCampaign mpCampaign = campaign as MultiPlayerCampaign;
				if (mpCampaign != null)
				{
					mpCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from p in MissionPrefab.Prefabs
				select p.Identifier.ToString()).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setcampaignmetadata", "setcampaignmetadata [identifier] [value]: Sets the specified campaign metadata value.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("The setcampaignmetadata command is only usable in the campaign mode.", null, null, false, false);
					return;
				}
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Please specify an identifier and a value.", null, null, false, false);
					return;
				}
				float floatVal;
				if (float.TryParse(args[1], out floatVal))
				{
					SetDataAction.PerformOperation(campaign.CampaignMetadata, args[0].ToIdentifier(), floatVal, SetDataAction.OperationType.Set);
					return;
				}
				SetDataAction.PerformOperation(campaign.CampaignMetadata, args[0].ToIdentifier(), args[1], SetDataAction.OperationType.Set);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setskill", "setskill [all/identifier] [max/level] [character]: Set your skill level.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(68, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 2 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (skill, level, name)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				Identifier skillIdentifier = args[0].ToIdentifier();
				string levelString = args[1];
				Character character = (args.Length >= 3) ? DebugConsole.FindMatchingCharacter(args.Skip(2).ToArray<string>(), false, null, false) : Character.Controlled;
				bool flag;
				if (character == null)
				{
					flag = (null != null);
				}
				else
				{
					CharacterInfo info = character.Info;
					flag = (((info != null) ? info.Job : null) != null);
				}
				if (!flag)
				{
					DebugConsole.NewMessage("Character is not valid.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				bool isMax = levelString.Equals("max", StringComparison.OrdinalIgnoreCase);
				float level;
				if (!float.TryParse(levelString, NumberStyles.Number, CultureInfo.InvariantCulture, out level) && !isMax)
				{
					DebugConsole.NewMessage(levelString + " is not a valid level. Expected number or \"max\".", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				if (isMax)
				{
					level = 100f;
				}
				if (skillIdentifier == "all")
				{
					foreach (Skill skill in character.Info.Job.GetSkills())
					{
						character.Info.SetSkillLevel(skill.Identifier, level, false);
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Set all ");
					defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler2.AppendLiteral("'s skills to ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(level);
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
					return;
				}
				character.Info.SetSkillLevel(skillIdentifier, level, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(17, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("Set ");
				defaultInterpolatedStringHandler3.AppendFormatted(character.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("'s ");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(skillIdentifier);
				defaultInterpolatedStringHandler3.AppendLiteral(" level to ");
				defaultInterpolatedStringHandler3.AppendFormatted<float>(level);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
			}, delegate()
			{
				string[][] array = new string[3][];
				int num = 0;
				Character controlled = Character.Controlled;
				string[] array2;
				if (controlled == null)
				{
					array2 = null;
				}
				else
				{
					CharacterInfo info = controlled.Info;
					if (info == null)
					{
						array2 = null;
					}
					else
					{
						Job job = info.Job;
						if (job == null)
						{
							array2 = null;
						}
						else
						{
							IEnumerable<Skill> skills = job.GetSkills();
							if (skills == null)
							{
								array2 = null;
							}
							else
							{
								array2 = (from skill in skills
								select skill.Identifier.Value).ToArray<string>();
							}
						}
					}
				}
				array[num] = (array2 ?? Array.Empty<string>());
				array[1] = new string[]
				{
					"max"
				};
				array[2] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("water|editwater", "water/editwater: Toggle water editing. Allows adding water into rooms by holding the left mouse button and removing it by holding the right mouse button.", delegate(string[] args)
			{
				Hull.EditWater = !Hull.EditWater;
				DebugConsole.NewMessage(Hull.EditWater ? "Water editing on" : "Water editing off", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("givetalent", "givetalent [talent] [player]: give the talent to the specified character. If the character argument is omitted, the talent is given to the controlled character.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				Character character = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : Character.Controlled;
				if (character != null)
				{
					TalentPrefab talentPrefab = TalentPrefab.TalentPrefabs.Find((TalentPrefab c) => c.Identifier == args[0] || c.DisplayName.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (talentPrefab == null)
					{
						DebugConsole.ThrowError("Couldn't find the talent \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					character.GiveTalent(talentPrefab, true);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Gave talent \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(talentPrefab.DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
					defaultInterpolatedStringHandler.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
				}
			}, delegate()
			{
				List<string> talentNames = new List<string>();
				foreach (TalentPrefab talent in TalentPrefab.TalentPrefabs)
				{
					talentNames.Add(talent.DisplayName.Value);
				}
				string[][] array = new string[2][];
				array[0] = (from id in talentNames
				select id).ToArray<string>();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("unlocktalents", "unlocktalents [all/[jobname]] [character]: give the specified character all the talents of the specified class", delegate(string[] args)
			{
				Character character = (args.Length >= 2) ? DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) : Character.Controlled;
				if (character == null)
				{
					return;
				}
				List<TalentTree> talentTrees = new List<TalentTree>();
				if (args.Length == 0 || args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					talentTrees.AddRange(TalentTree.JobTalentTrees);
				}
				else
				{
					JobPrefab job = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.Name != null && jp.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
					if (job == null)
					{
						DebugConsole.ThrowError("Failed to find the job \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					TalentTree talentTree;
					if (!TalentTree.JobTalentTrees.TryGet(job.Identifier, out talentTree))
					{
						DebugConsole.ThrowError("No talents configured for the job \"" + args[0] + "\".", null, null, false, false);
						return;
					}
					talentTrees.Add(talentTree);
				}
				foreach (TalentTree talentTree2 in talentTrees)
				{
					foreach (Identifier talentId in talentTree2.AllTalentIdentifiers)
					{
						character.GiveTalent(talentId, true);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unlocked talent \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(talentId);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					}
				}
			}, delegate()
			{
				List<string> availableArgs = new List<string>
				{
					"All"
				};
				availableArgs.AddRange(from j in JobPrefab.Prefabs
				select j.Name.Value);
				string[][] array = new string[2][];
				array[0] = availableArgs.ToArray();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("giveexperience", "giveexperience [amount] [character]: Give experience to character.", delegate(string[] args)
			{
				if (args.Length < 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(66, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Missing arguments. Expected at least 1 but got ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(args.Length);
					defaultInterpolatedStringHandler.AppendLiteral(" (experience, name)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					return;
				}
				string experienceString = args[0];
				Character character = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false) ?? Character.Controlled;
				if (((character != null) ? character.Info : null) == null)
				{
					DebugConsole.NewMessage("Character is not valid.", null, false);
					return;
				}
				int experience;
				if (int.TryParse(experienceString, NumberStyles.Number, CultureInfo.InvariantCulture, out experience))
				{
					character.Info.GiveExperience(experience);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Gave ");
					defaultInterpolatedStringHandler2.AppendFormatted(character.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(experience);
					defaultInterpolatedStringHandler2.AppendLiteral(" experience");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
					return;
				}
				DebugConsole.NewMessage(experienceString + " is not a valid value. Expected number.", null, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = new string[]
				{
					"100"
				};
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fire|editfire", "fire/editfire: Allows putting up fires by left clicking.", delegate(string[] args)
			{
				Hull.EditFire = !Hull.EditFire;
				DebugConsole.NewMessage(Hull.EditFire ? "Fire spawning on" : "Fire spawning off", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("explosion", "explosion [range] [force] [damage] [structuredamage] [item damage] [emp strength] [ballast flora strength]: Creates an explosion at the position of the cursor.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showseed|showlevelseed", "showseed: Show the seed of the current level.", delegate(string[] args)
			{
				if (Level.Loaded == null)
				{
					DebugConsole.ThrowError("No level loaded.", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				Map map = (gameSession != null) ? gameSession.Map : null;
				if (map != null)
				{
					DebugConsole.NewMessage("Map seed: " + map.Seed, null, false);
				}
				DebugConsole.NewMessage("Level seed: " + Level.Loaded.Seed, null, false);
				DebugConsole.NewMessage("Level generation params: " + Level.Loaded.GenerationParams.Identifier.ToString(), null, false);
				string str = "Adjacent locations: ";
				Location startLocation = Level.Loaded.StartLocation;
				string str2 = ((startLocation != null) ? startLocation.Type.Identifier : "none".ToIdentifier()).ToString();
				string str3 = ", ";
				Location startLocation2 = Level.Loaded.StartLocation;
				DebugConsole.NewMessage(str + str2 + str3 + ((startLocation2 != null) ? startLocation2.Type.Identifier : "none".ToIdentifier()).ToString(), null, false);
				DebugConsole.NewMessage("Mirrored: " + Level.Loaded.Mirrored.ToString(), null, false);
				string str4 = "Level size: ";
				Point size = Level.Loaded.Size;
				string str5 = size.X.ToString();
				string str6 = "x";
				size = Level.Loaded.Size;
				DebugConsole.NewMessage(str4 + str5 + str6 + size.Y.ToString(), null, false);
				string str7 = "Minimum main path width: ";
				LevelData levelData = Level.Loaded.LevelData;
				DebugConsole.NewMessage(str7 + (((levelData != null) ? ((levelData.MinMainPathWidth != null) ? levelData.MinMainPathWidth.GetValueOrDefault().ToString() : null) : null) ?? "unknown"), null, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("teleportsub", "teleportsub [start/end/endoutpost/cursor] [submarine_team]: Teleport the submarine to the position of the cursor, or the start or end of the level. The 'endoutpost' argument also automatically docks the sub with the outpost at the end of the level. WARNING: does not take outposts into account, so often leads to physics glitches. Only use for debugging.", delegate(string[] args)
			{
				if (Submarine.MainSub == null)
				{
					return;
				}
				Submarine submarineToTeleport = Submarine.MainSub;
				if (args.Length > 1)
				{
					foreach (Submarine sub in from s in Submarine.Loaded
					where s.PhysicsBody.BodyType == BodyType.Dynamic
					select s)
					{
						if (sub.Info.Name + "_" + sub.TeamID.ToString() == args[1])
						{
							submarineToTeleport = sub;
							break;
						}
					}
				}
				if (args.Length == 0 || args[0].Equals("cursor", StringComparison.OrdinalIgnoreCase))
				{
					submarineToTeleport.SetPosition(Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition), null, true);
					return;
				}
				if (args[0].Equals("start", StringComparison.OrdinalIgnoreCase))
				{
					if (Level.Loaded == null)
					{
						DebugConsole.NewMessage("Can't teleport the sub to the start of the level (no level loaded).", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
						return;
					}
					Vector2 pos = Level.Loaded.StartPosition;
					if (Level.Loaded.StartOutpost != null)
					{
						pos -= Vector2.UnitY * (float)(submarineToTeleport.Borders.Height + Level.Loaded.StartOutpost.Borders.Height) / 2f;
					}
					submarineToTeleport.SetPosition(pos, null, true);
					return;
				}
				else
				{
					if (!args[0].Equals("end", StringComparison.OrdinalIgnoreCase))
					{
						if (args[0].Equals("endoutpost", StringComparison.OrdinalIgnoreCase))
						{
							Level loaded = Level.Loaded;
							if (((loaded != null) ? loaded.EndOutpost : null) == null)
							{
								DebugConsole.NewMessage("Can't teleport the sub to the end outpost (no outpost at the end of the level).", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
								return;
							}
							submarineToTeleport.SetPosition(Level.Loaded.EndExitPosition - Vector2.UnitY * (float)submarineToTeleport.Borders.Height, null, true);
							DockingPort submarineDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == submarineToTeleport);
							DockingPort outpostDockingPort = DockingPort.List.FirstOrDefault((DockingPort d) => d.Item.Submarine == Level.Loaded.EndOutpost);
							if (submarineDockingPort != null && outpostDockingPort != null)
							{
								submarineDockingPort.Dock(outpostDockingPort);
							}
						}
						return;
					}
					if (Level.Loaded == null)
					{
						DebugConsole.NewMessage("Can't teleport the sub to the end of the level (no level loaded).", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
						return;
					}
					Vector2 pos2 = Level.Loaded.EndPosition;
					if (Level.Loaded.EndOutpost != null)
					{
						pos2 -= Vector2.UnitY * (float)(submarineToTeleport.Borders.Height + Level.Loaded.EndOutpost.Borders.Height) / 2f;
					}
					submarineToTeleport.SetPosition(pos2, null, true);
					return;
				}
			}, () => new string[][]
			{
				new string[]
				{
					"start",
					"end",
					"endoutpost",
					"cursor"
				},
				DebugConsole.ListAvailableSubmarines()
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showreputation", "showreputation: List the current reputation values.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null)
				{
					DebugConsole.NewMessage("Reputation:", null, false);
					using (IEnumerator<Faction> enumerator = campaign.Factions.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Faction faction = enumerator.Current;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
							defaultInterpolatedStringHandler.AppendLiteral(" - ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(faction.Prefab.Name);
							defaultInterpolatedStringHandler.AppendLiteral(": ");
							defaultInterpolatedStringHandler.AppendFormatted<float>(faction.Reputation.Value);
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
						}
						return;
					}
				}
				DebugConsole.ThrowError("Could not show reputation (no active campaign).", null, null, false, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("setlocationreputation", "setlocationreputation [value]: Set the reputation in the current location to the specified value.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("Could not set location reputation (no active campaign).", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					return;
				}
				float reputation;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out reputation))
				{
					DebugConsole.ThrowError("Could not set location reputation (" + args[0] + " is not a valid reputation value).", null, null, false, false);
					return;
				}
				Reputation reputation2 = campaign.Map.CurrentLocation.Reputation;
				if (reputation2 == null)
				{
					return;
				}
				reputation2.SetReputation(reputation);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setreputation", "setreputation [faction] [value]: Set the reputation of a cation to the specified value.", delegate(string[] args)
			{
				if (args.Length < 2)
				{
					DebugConsole.ThrowError("Insufficient arguments (expected 2)", null, null, false, false);
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("Could not set faction reputation (no active campaign).", null, null, false, false);
					return;
				}
				Faction faction = campaign.Factions.FirstOrDefault((Faction f) => f.Prefab.Identifier == args[0]);
				if (faction == null)
				{
					DebugConsole.ThrowError("Could not set faction reputation (faction " + args[0] + " not found).", null, null, false, false);
					return;
				}
				float reputation;
				if (float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out reputation))
				{
					faction.Reputation.SetReputation(reputation);
					return;
				}
				DebugConsole.ThrowError("Could not set faction reputation (" + args[1] + " is not a valid reputation value).", null, null, false, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from f in FactionPrefab.Prefabs
				select f.Identifier.Value).ToArray<string>();
				int num = 1;
				GameSession gameSession = GameMain.GameSession;
				string[] array2;
				if (gameSession == null)
				{
					array2 = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					if (campaign == null)
					{
						array2 = null;
					}
					else
					{
						array2 = (from f in campaign.Factions
						select f.Prefab.Identifier.ToString()).ToArray<string>();
					}
				}
				array[num] = (array2 ?? Array.Empty<string>());
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fixitems", "fixitems: Repairs all items and restores them to full condition.", delegate(string[] args)
			{
				foreach (Item it in Item.ItemList)
				{
					if (it.GetComponent<GeneticMaterial>() == null)
					{
						it.Condition = it.MaxCondition;
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("fixhulls|fixwalls", "fixwalls/fixhulls: Fixes all walls.", delegate(string[] args)
			{
				List<Structure> walls = new List<Structure>(Structure.WallList);
				foreach (Structure w in walls)
				{
					try
					{
						for (int i = 0; i < w.SectionCount; i++)
						{
							w.AddDamage(i, -100000f, null, true, false);
						}
					}
					catch (InvalidOperationException e)
					{
						string errorMsg = "Error while executing the fixhulls command.\n" + e.StackTrace.CleanupStackTrace();
						GameAnalyticsManager.AddErrorEventOnce("DebugConsole.FixHulls", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("maxupgrades", "maxupgrades [category] [prefab]: Maxes out all upgrades or only specific one if given arguments.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				UpgradeManager upgradeManager2;
				if (gameSession == null)
				{
					upgradeManager2 = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					upgradeManager2 = ((campaign != null) ? campaign.UpgradeManager : null);
				}
				UpgradeManager upgradeManager = upgradeManager2;
				if (upgradeManager == null)
				{
					DebugConsole.ThrowError("This command can only be used in campaign.", null, null, false, false);
					return;
				}
				string categoryIdentifier = null;
				string prefabIdentifier = null;
				int num = args.Length;
				if (num != 1)
				{
					if (num == 2)
					{
						categoryIdentifier = args[0];
						prefabIdentifier = args[1];
					}
				}
				else
				{
					categoryIdentifier = args[0];
				}
				foreach (UpgradeCategory category in UpgradeCategory.Categories)
				{
					if (string.IsNullOrWhiteSpace(categoryIdentifier) || !(category.Identifier != categoryIdentifier))
					{
						foreach (UpgradePrefab prefab in UpgradePrefab.Prefabs)
						{
							if (prefab.UpgradeCategories.Contains(category) && (string.IsNullOrWhiteSpace(prefabIdentifier) || !(prefab.Identifier != prefabIdentifier)))
							{
								int targetLevel = prefab.GetMaxLevelForCurrentSub() - upgradeManager.GetRealUpgradeLevel(prefab, category);
								for (int i = 0; i < targetLevel; i++)
								{
									upgradeManager.TryPurchaseUpgrade(prefab, category, true, null);
								}
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Upgraded ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(category.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(".");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral(" by ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(targetLevel);
								defaultInterpolatedStringHandler.AppendLiteral(" levels.");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.DarkGreen), false);
							}
						}
					}
				}
				DebugConsole.NewMessage("Start a new round to apply the upgrades.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = (from i in (from c in UpgradeCategory.Categories
				select c.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				array[1] = (from i in (from c in UpgradePrefab.Prefabs
				select c.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("power", "power: Immediately powers up the submarine's nuclear reactor.", delegate(string[] args)
			{
				Item reactorItem = Item.ItemList.Find((Item i) => i.GetComponent<Reactor>() != null);
				if (reactorItem == null)
				{
					return;
				}
				Reactor reactor = reactorItem.GetComponent<Reactor>();
				reactor.PowerUpImmediately();
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("oxygen|air", "oxygen/air: Replenishes the oxygen levels in every room to 100%.", delegate(string[] args)
			{
				foreach (Hull hull in Hull.HullList)
				{
					hull.OxygenPercentage = 100f;
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("kill", "kill [character]: Immediately kills the specified character.", delegate(string[] args)
			{
				Character killedCharacter = (args.Length == 0) ? Character.Controlled : DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (killedCharacter != null)
				{
					killedCharacter.Kill(CauseOfDeathType.Unknown, null, false, true);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("killmonsters", "killmonsters: Immediately kills all AI-controlled enemies in the level.", delegate(string[] args)
			{
				foreach (Character c in Character.CharacterList)
				{
					EnemyAIController enemyAI = c.AIController as EnemyAIController;
					if (enemyAI != null && enemyAI.PetBehavior == null)
					{
						c.SetAllDamage(200f, 0f, 0f);
					}
				}
				foreach (Hull hull in Hull.HullList)
				{
					BallastFloraBehavior ballastFlora = hull.BallastFlora;
					if (ballastFlora != null)
					{
						ballastFlora.Kill();
					}
				}
				foreach (Submarine sub in Submarine.Loaded)
				{
					WreckAI wreckAI = sub.WreckAI;
					if (wreckAI != null)
					{
						wreckAI.Kill();
					}
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("killall", "killall: Immediately kills all characters in the level.", delegate(string[] args)
			{
				foreach (Character c in Character.CharacterList)
				{
					c.Kill(CauseOfDeathType.Unknown, null, false, true);
					DebugConsole.NewMessage("Killed " + c.DisplayName + ".", null, false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("despawnnow", "despawnnow [character]: Immediately despawns the specified dead character. If the character argument is omitted, all dead characters are despawned.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					using (List<Character>.Enumerator enumerator = (from c in Character.CharacterList
					where c.IsDead
					select c).ToList<Character>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Character c2 = enumerator.Current;
							c2.DespawnNow(true);
						}
						return;
					}
				}
				Character character = DebugConsole.FindMatchingCharacter(args, false, null, false);
				if (character != null)
				{
					character.DespawnNow(true);
				}
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from n in (from c in Character.CharacterList
				where c.IsDead
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setclientcharacter", "setclientcharacter [client name] [character name]: Gives the client control of the specified character.", null, delegate()
			{
				if (GameMain.NetworkMember == null)
				{
					return null;
				}
				string[][] array = new string[2][];
				array[0] = (from c in GameMain.NetworkMember.ConnectedClients
				select c.Name).ToArray<string>();
				array[1] = (from n in (from c in Character.CharacterList
				select c.Name).Distinct<string>()
				orderby n
				select n).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("campaigninfo|campaignstatus", "campaigninfo: Display information about the state of the currently active campaign.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				campaign.LogState();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("campaigndestination|setcampaigndestination", "campaigndestination [index]: Set the location to head towards in the currently active campaign.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
				CampaignMode campaign = gameMode as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				if (args.Length == 0)
				{
					int i = 0;
					foreach (LocationConnection connection in campaign.Map.CurrentLocation.Connections)
					{
						DebugConsole.NewMessage("     " + i.ToString() + ". " + connection.OtherLocation(campaign.Map.CurrentLocation).DisplayName, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
						i++;
					}
					DebugConsole.ShowQuestionPrompt("Select a destination (0 - " + (campaign.Map.CurrentLocation.Connections.Count - 1).ToString() + "):", delegate(string selectedDestination)
					{
						int destinationIndex2 = -1;
						if (!int.TryParse(selectedDestination, out destinationIndex2))
						{
							return;
						}
						if (destinationIndex2 < 0 || destinationIndex2 >= campaign.Map.CurrentLocation.Connections.Count)
						{
							DebugConsole.NewMessage("Index out of bounds!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
							return;
						}
						Location location2 = campaign.Map.CurrentLocation.Connections[destinationIndex2].OtherLocation(campaign.Map.CurrentLocation);
						campaign.Map.SelectLocation(location2);
						DebugConsole.NewMessage(location2.DisplayName + " selected.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
					}, null, -1);
					return;
				}
				int destinationIndex = -1;
				if (!int.TryParse(args[0], out destinationIndex))
				{
					return;
				}
				if (destinationIndex < 0 || destinationIndex >= campaign.Map.CurrentLocation.Connections.Count)
				{
					DebugConsole.NewMessage("Index out of bounds!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				Location location = campaign.Map.CurrentLocation.Connections[destinationIndex].OtherLocation(campaign.Map.CurrentLocation);
				campaign.Map.SelectLocation(location);
				DebugConsole.NewMessage(location.DisplayName + " selected.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglecampaignteleport", "Toggle on/off teleportation between campaign locations by double clicking on the campaign map.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null)
				{
					DebugConsole.ThrowError("No campaign active.", null, null, false, false);
					return;
				}
				GameMain.GameSession.Map.AllowDebugTeleport = !GameMain.GameSession.Map.AllowDebugTeleport;
				DebugConsole.NewMessage((GameMain.GameSession.Map.AllowDebugTeleport ? "Enabled" : "Disabled") + " teleportation on the campaign map.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("money", "money [amount] [character]: Gives the specified amount of money to the crew when a campaign is active.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					return;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					return;
				}
				Character targetCharacter = null;
				if (args.Length >= 2)
				{
					targetCharacter = DebugConsole.FindMatchingCharacter(args.Skip(1).ToArray<string>(), false, null, false);
				}
				int money;
				if (int.TryParse(args[0], out money))
				{
					Wallet wallet = (targetCharacter == null || GameMain.IsSingleplayer) ? campaign.Bank : targetCharacter.Wallet;
					wallet.Give(money);
					GameAnalyticsManager.AddMoneyGainedEvent(money, GameAnalyticsManager.MoneySource.Cheat, "console");
					return;
				}
				DebugConsole.ThrowError("\"" + args[0] + "\" is not a valid numeric value.", null, null, false, false);
			}, delegate()
			{
				string[][] array = new string[2][];
				array[0] = new string[]
				{
					string.Empty
				};
				array[1] = (from c in Character.CharacterList
				select c.Name).Distinct<string>().ToArray<string>();
				return array;
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showmoney", "showmoney: Shows the amount of money in everyones wallet.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign == null)
				{
					DebugConsole.ThrowError("No campaign active!", null, null, false, false);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Bank: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(campaign.Bank.Balance);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("skipeventcooldown", "skipeventcooldown: Skips the currently active event cooldown and triggers pending monster spawns immediately.", delegate(string[] args)
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession == null)
				{
					return;
				}
				EventManager eventManager = gameSession.EventManager;
				if (eventManager == null)
				{
					return;
				}
				eventManager.SkipEventCooldown();
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ballastflora", "infectballast [options]: Infect ballasts and control its growth.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No action specified.", null, null, false, false);
					return;
				}
				string primaryAction = (args.Length != 0) ? args[0] : "";
				string secondaryArgument = (args.Length > 1) ? args[1] : "";
				if (Submarine.MainSub == null)
				{
					DebugConsole.ThrowError("No submarine loaded.", null, null, false, false);
					return;
				}
				if (primaryAction.Equals("infect", StringComparison.OrdinalIgnoreCase))
				{
					List<Pump> pumps = new List<Pump>();
					foreach (Item item in Submarine.MainSub.GetItems(true))
					{
						if (item.CurrentHull != null && item.HasTag(Tags.Ballast))
						{
							Pump pump = item.GetComponent<Pump>();
							if (pump != null && item.CurrentHull.BallastFlora == null)
							{
								pumps.Add(pump);
							}
						}
					}
					if (pumps.Any<Pump>())
					{
						BallastFloraPrefab prefab = string.IsNullOrWhiteSpace(secondaryArgument) ? BallastFloraPrefab.Prefabs.First<BallastFloraPrefab>() : BallastFloraPrefab.Find(secondaryArgument.ToIdentifier());
						if (prefab == null)
						{
							DebugConsole.ThrowError("No such behavior: " + secondaryArgument, null, null, false, false);
							return;
						}
						Pump random = pumps.GetRandomUnsynced<Pump>();
						random.InfectBallast(prefab.Identifier, true);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Infected ");
						defaultInterpolatedStringHandler.AppendFormatted(random.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" with ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(" in ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(random.Item.CurrentHull.DisplayName);
						defaultInterpolatedStringHandler.AppendLiteral(".");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
						return;
					}
					else
					{
						DebugConsole.ThrowError("No available pumps to infect on this submarine.", null, null, false, false);
					}
				}
				if (primaryAction.Equals("growthwarp", StringComparison.OrdinalIgnoreCase))
				{
					int value;
					if (int.TryParse(secondaryArgument, out value))
					{
						foreach (Hull hull in from h in Hull.HullList
						where h.BallastFlora != null
						select h)
						{
							BallastFloraBehavior bs = hull.BallastFlora;
							bs.GrowthWarps = value;
						}
						DebugConsole.NewMessage("Accelerating growth...", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
						return;
					}
					DebugConsole.ThrowError("Invalid integer \"" + secondaryArgument + "\".", null, null, false, false);
				}
			}, delegate()
			{
				string[] primaries = new string[]
				{
					"infect",
					"growthwarp"
				};
				string[] identifiers = (from i in (from bfp in BallastFloraPrefab.Prefabs
				select bfp.Identifier).Distinct<Identifier>()
				select i.Value).ToArray<string>();
				return new string[][]
				{
					primaries,
					identifiers
				};
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("setdifficulty|forcedifficulty", "difficulty [0-100]. Leave the parameter empty to disable.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					Level.ForcedDifficulty = null;
					DebugConsole.NewMessage("Forced difficulty level disabled.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Green), false);
					return;
				}
				float difficulty;
				if (float.TryParse(args[0], out difficulty))
				{
					Level.ForcedDifficulty = new float?(difficulty);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Set the difficulty level to ");
					defaultInterpolatedStringHandler.AppendFormatted<float?>(Level.ForcedDifficulty);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
				}
			}, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("difficulty|leveldifficulty", "difficulty [0-100]: Change the level difficulty setting in the server lobby.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("autoitemplacerdebug|outfitdebug", "autoitemplacerdebug: Toggle automatic item placer debug info on/off. The automatically placed items are listed in the debug console at the start of a round.", delegate(string[] args)
			{
				AutoItemPlacer.OutputDebugInfo = !AutoItemPlacer.OutputDebugInfo;
				DebugConsole.NewMessage((AutoItemPlacer.OutputDebugInfo ? "Enabled" : "Disabled") + " automatic item placer logging.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("verboselogging", "verboselogging: Toggle verbose console logging on/off. When on, additional debug information is written to the debug console.", delegate(string[] args)
			{
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.VerboseLogging = !GameSettings.CurrentConfig.VerboseLogging;
				GameSettings.SetCurrentConfig(config);
				DebugConsole.NewMessage((GameSettings.CurrentConfig.VerboseLogging ? "Enabled" : "Disabled") + " verbose logging.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("listtasks", "listtasks: Lists all asynchronous tasks currently in the task pool.", delegate(string[] args)
			{
				TaskPool.ListTasks(delegate(string line)
				{
					DebugConsole.NewMessage(line, null, false);
				});
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("listcoroutines", "listcoroutines: Lists all coroutines currently running.", delegate(string[] args)
			{
				CoroutineManager.ListCoroutines();
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("calculatehashes", "calculatehashes [content package name]: Show the MD5 hashes of the files in the selected content package. If the name parameter is omitted, the first content package is selected.", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					ContentPackageManager.EnabledPackages.Core.CalculateHash(true, null, null);
					return;
				}
				string packageName = string.Join(" ", args);
				ContentPackage package = ContentPackageManager.EnabledPackages.All.FirstOrDefault((ContentPackage p) => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
				if (package == null)
				{
					DebugConsole.ThrowError("Content package \"" + packageName + "\" not found.", null, null, false, false);
					return;
				}
				package.CalculateHash(true, null, null);
			}, delegate()
			{
				string[][] array = new string[1][];
				array[0] = (from cp in ContentPackageManager.EnabledPackages.All
				select cp.Name).ToArray<string>();
				return array;
			}, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedlatency", "simulatedlatency [minimumlatencyseconds] [randomlatencyseconds]: applies a simulated latency to network messages. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 2 || GameMain.NetworkMember == null)
				{
					return;
				}
				float minimumLatency;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out minimumLatency))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid latency value.", null, null, false, false);
					return;
				}
				float randomLatency;
				if (!float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out randomLatency))
				{
					DebugConsole.ThrowError(args[1] + " is not a valid latency value.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedMinimumLatency = minimumLatency;
					GameMain.NetworkMember.SimulatedRandomLatency = randomLatency;
				}
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Set simulated minimum latency to ",
					minimumLatency.ToString(CultureInfo.InvariantCulture),
					" and random latency to ",
					randomLatency.ToString(CultureInfo.InvariantCulture),
					"."
				}), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedloss", "simulatedloss [lossratio]: applies simulated packet loss to network messages. For example, a value of 0.1 would mean 10% of the packets are dropped. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 1 || GameMain.NetworkMember == null)
				{
					return;
				}
				float loss;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out loss))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid loss ratio.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedLoss = loss;
				}
				DebugConsole.NewMessage("Set simulated packet loss to " + ((int)(loss * 100f)).ToString() + "%.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("simulatedduplicateschance", "simulatedduplicateschance [duplicateratio]: simulates packet duplication in network messages. For example, a value of 0.1 would mean there's a 10% chance a packet gets sent twice. Useful for simulating real network conditions when testing the multiplayer locally.", delegate(string[] args)
			{
				if (args.Count<string>() < 1 || GameMain.NetworkMember == null)
				{
					return;
				}
				float duplicates;
				if (!float.TryParse(args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out duplicates))
				{
					DebugConsole.ThrowError(args[0] + " is not a valid duplicate ratio.", null, null, false, false);
					return;
				}
				if (GameMain.NetworkMember != null)
				{
					GameMain.NetworkMember.SimulatedDuplicatesChance = duplicates;
				}
				DebugConsole.NewMessage("Set packet duplication to " + ((int)(duplicates * 100f)).ToString() + "%.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.White), false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("startitems|startitemset", "start item set identifier", delegate(string[] args)
			{
				if (args.Length == 0)
				{
					DebugConsole.ThrowError("No start item set identifier defined!", null, null, false, false);
					return;
				}
				AutoItemPlacer.DefaultStartItemSet = args[0].ToIdentifier();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Start item set changed to \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(AutoItemPlacer.DefaultStartItemSet);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("control", "control [character name]: Start controlling the specified character (client-only).", null, () => new string[][]
			{
				DebugConsole.ListCharacterNames(false, false)
			}, true));
			DebugConsole.commands.Add(new DebugConsole.Command("los", "Toggle the line of sight effect on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("lighting|lights", "Toggle lighting on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("ambientlight", "ambientlight [color]: Change the color of the ambient light in the level.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdraw", "Toggle the debug drawing mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugwiring", "Toggle the wiring debug mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdrawlocalization", "Toggle the localization debug drawing mode on/off (client-only). Colors all text that hasn't been fetched from a localization file magenta, making it easier to spot hard-coded or missing texts.", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("debugdrawlos", "Toggle the los debug drawing mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("togglevoicechatfilters", "Toggle the radio/muffle filters in the voice chat (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglehud|hud", "Toggle the character HUD (inventories, icons, buttons, etc) on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleupperhud", "Toggle the upper part of the ingame HUD (chatbox, crewmanager) on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleitemhighlights", "Toggle the item highlight effect on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("togglecharacternames", "Toggle the names hovering above characters on/off (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("followsub", "Toggle whether the camera should follow the nearest submarine (client-only).", null, null, false));
			DebugConsole.commands.Add(new DebugConsole.Command("toggleaitargets|aitargets", "Toggle the visibility of AI targets (= targets that enemies can detect and attack/escape from) (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("debugai", "Toggle the ai debug mode on/off (works properly only in single player).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("devmode", "Toggle the dev mode on/off (client-only).", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("showmonsters", "Permanently unlocks all the monsters in the character editor. Use \"hidemonsters\" to undo.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("hidemonsters", "Permanently hides in the character editor all the monsters that haven't been encountered in the game. Use \"showmonsters\" to undo.", null, null, true));
			DebugConsole.commands.Add(new DebugConsole.Command("loslightingfreecam", "Toggles line of sight effect, lighting, and enables freecam mode. (client-only)", null, null, true));
			DebugConsole.InitProjectSpecific();
			DebugConsole.commands.Sort((DebugConsole.Command c1, DebugConsole.Command c2) => c1.Names.First<Identifier>().CompareTo(c2.Names.First<Identifier>()));
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x0005CEA0 File Offset: 0x0005B0A0
		private static void HealCharacter(Character healedCharacter, bool healAll, Client targetClient = null)
		{
			healedCharacter.SetAllDamage(0f, 0f, 0f);
			healedCharacter.Oxygen = 100f;
			healedCharacter.Bloodloss = 0f;
			healedCharacter.SetStun(0f, true, false);
			if (healAll)
			{
				healedCharacter.CharacterHealth.RemoveAllAfflictions();
			}
			string characterNameText = (healedCharacter == Character.Controlled) ? (healedCharacter.Name + " (you)") : healedCharacter.Name;
			string text = healAll ? ("Healed " + characterNameText + ": all afflictions") : ("Healed " + characterNameText + ": damage and common afflictions");
			DebugConsole.NewMessage(text, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x0005CF4C File Offset: 0x0005B14C
		public static string AutoComplete(string command, int increment = 1)
		{
			string[] splitCommand = ToolBox.SplitCommand(command);
			string[] args = splitCommand.Skip(1).ToArray<string>();
			if (args.Length != 0 || (splitCommand.Length != 0 && command.Last<char>() == ' '))
			{
				DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(splitCommand[0].ToIdentifier()));
				if (matchingCommand == null || matchingCommand.GetValidArgs == null)
				{
					return command;
				}
				int autoCompletedArgIndex = (args.Length != 0 && command.Last<char>() != ' ') ? (args.Length - 1) : args.Length;
				string[][] allArgs = matchingCommand.GetValidArgs();
				if (allArgs == null || allArgs.GetLength(0) < autoCompletedArgIndex + 1)
				{
					return command;
				}
				if (string.IsNullOrEmpty(DebugConsole.currentAutoCompletedCommand))
				{
					DebugConsole.currentAutoCompletedCommand = ((autoCompletedArgIndex > args.Length - 1) ? " " : args.Last<string>());
				}
				string[] validArgs = (from arg in allArgs[autoCompletedArgIndex]
				where DebugConsole.currentAutoCompletedCommand.Trim().Length <= arg.Length && arg.Substring(0, DebugConsole.currentAutoCompletedCommand.Trim().Length).ToLower() == DebugConsole.currentAutoCompletedCommand.Trim().ToLower()
				select arg).ToArray<string>();
				validArgs = validArgs.Concat(from arg in allArgs[autoCompletedArgIndex]
				where arg.ToLower().Contains(DebugConsole.currentAutoCompletedCommand.Trim().ToLower()) && !validArgs.Contains(arg)
				select arg).ToArray<string>();
				if (validArgs.Length == 0)
				{
					return command;
				}
				DebugConsole.currentAutoCompletedIndex = MathUtils.PositiveModulo(DebugConsole.currentAutoCompletedIndex + increment, validArgs.Length);
				string autoCompletedArg = validArgs[DebugConsole.currentAutoCompletedIndex];
				if (autoCompletedArg.Contains(' '))
				{
					autoCompletedArg = "\"" + autoCompletedArg + "\"";
				}
				for (int i = 0; i < splitCommand.Length; i++)
				{
					if (splitCommand[i].Contains(' '))
					{
						splitCommand[i] = "\"" + splitCommand[i] + "\"";
					}
				}
				string separator = " ";
				IEnumerable<string> values;
				if (autoCompletedArgIndex < args.Length)
				{
					values = splitCommand.Take(splitCommand.Length - 1);
				}
				else
				{
					IEnumerable<string> splitCommand2 = splitCommand;
					values = splitCommand2;
				}
				return string.Join(separator, values) + " " + autoCompletedArg;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(DebugConsole.currentAutoCompletedCommand))
				{
					DebugConsole.currentAutoCompletedCommand = command;
				}
				List<Identifier> matchingCommands = new List<Identifier>();
				foreach (DebugConsole.Command c2 in DebugConsole.commands)
				{
					foreach (Identifier name in c2.Names)
					{
						if (DebugConsole.currentAutoCompletedCommand.Length <= name.Value.Length && name.StartsWith(DebugConsole.currentAutoCompletedCommand))
						{
							matchingCommands.Add(name);
						}
					}
				}
				if (matchingCommands.Count == 0)
				{
					return command;
				}
				DebugConsole.currentAutoCompletedIndex = MathUtils.PositiveModulo(DebugConsole.currentAutoCompletedIndex + increment, matchingCommands.Count);
				return matchingCommands[DebugConsole.currentAutoCompletedIndex].Value;
			}
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0005D244 File Offset: 0x0005B444
		public static void ResetAutoComplete()
		{
			DebugConsole.currentAutoCompletedCommand = "";
			DebugConsole.currentAutoCompletedIndex = 0;
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x0005D258 File Offset: 0x0005B458
		public static void ExecuteCommand(string inputtedCommands)
		{
			if (string.IsNullOrWhiteSpace(inputtedCommands) || inputtedCommands == "\\" || inputtedCommands == "\n")
			{
				return;
			}
			string[] commandsToExecute = inputtedCommands.Split("\n", StringSplitOptions.None);
			string[] array = commandsToExecute;
			for (int i = 0; i < array.Length; i++)
			{
				string command = array[i];
				if (DebugConsole.activeQuestionCallback != null)
				{
					DebugConsole.activeQuestionText = null;
					DebugConsole.NewCommand(command, null);
					DebugConsole.QuestionCallback temp = DebugConsole.activeQuestionCallback;
					DebugConsole.activeQuestionCallback = null;
					temp(command);
					return;
				}
				if (string.IsNullOrWhiteSpace(command) || command == "\\")
				{
					return;
				}
				string[] splitCommand = ToolBox.SplitCommand(command);
				if (splitCommand.Length == 0)
				{
					DebugConsole.ThrowError("Failed to execute command \"" + command + "\"!", null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("DebugConsole.ExecuteCommand:LengthZero", GameAnalyticsManager.ErrorSeverity.Error, "Failed to execute command \"" + command + "\"!");
					return;
				}
				Identifier firstCommand = splitCommand[0].ToIdentifier();
				if (firstCommand != "admin")
				{
					DebugConsole.NewCommand(command, null);
				}
				if (GameMain.Client != null)
				{
					DebugConsole.Command matchingCommand = DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(firstCommand));
					if (matchingCommand == null)
					{
						GameMain.Client.SendConsoleCommand(command);
						DebugConsole.NewMessage("Server command: " + command, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
						return;
					}
					if (GameMain.Client.HasConsoleCommandPermission(firstCommand))
					{
						if (matchingCommand.RelayToServer)
						{
							GameMain.Client.SendConsoleCommand(command);
							DebugConsole.NewMessage("Server command: " + command, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
							return;
						}
						matchingCommand.ClientExecute(splitCommand.Skip(1).ToArray<string>());
						return;
					}
					else if (!DebugConsole.IsCommandPermitted(firstCommand, GameMain.Client))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
						defaultInterpolatedStringHandler.AppendLiteral("You're not permitted to use the command \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(firstCommand);
						defaultInterpolatedStringHandler.AppendLiteral("\"!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						return;
					}
				}
				bool commandFound = false;
				foreach (DebugConsole.Command c2 in DebugConsole.commands)
				{
					if (c2.Names.Contains(firstCommand))
					{
						c2.Execute(splitCommand.Skip(1).ToArray<string>());
						commandFound = true;
						break;
					}
				}
				if (!commandFound)
				{
					DebugConsole.ThrowError("Command \"" + splitCommand[0] + "\" not found.", null, null, false, false);
				}
			}
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x0005D500 File Offset: 0x0005B700
		private static string[] ListAvailableLocations()
		{
			List<string> locationNames = new List<string>();
			foreach (Submarine submarine in Submarine.Loaded)
			{
				locationNames.Add(submarine.Info.Name);
			}
			if (Level.Loaded != null)
			{
				foreach (Level.Cave cave in Level.Loaded.Caves)
				{
					string caveName = cave.CaveGenerationParams.Name;
					int index = 1;
					for (;;)
					{
						List<string> list = locationNames;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler.AppendFormatted(caveName);
						defaultInterpolatedStringHandler.AppendLiteral("_");
						defaultInterpolatedStringHandler.AppendFormatted<int>(index);
						if (!list.Contains(defaultInterpolatedStringHandler.ToStringAndClear()))
						{
							break;
						}
						index++;
					}
					List<string> list2 = locationNames;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler2.AppendFormatted(caveName);
					defaultInterpolatedStringHandler2.AppendLiteral("_");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(index);
					list2.Add(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
			}
			if (Submarine.MainSub != null)
			{
				locationNames.Add("mainsub");
			}
			locationNames.Add("cursor");
			return locationNames.ToArray();
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x0005D658 File Offset: 0x0005B858
		private static string[] ListAvailableSubmarines()
		{
			List<string> submarineNames = new List<string>();
			foreach (Submarine submarine in from s in Submarine.Loaded
			where s.PhysicsBody.BodyType == BodyType.Dynamic
			select s)
			{
				submarineNames.Add(submarine.Info.Name + "_" + submarine.TeamID.ToString());
			}
			return submarineNames.ToArray();
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x0005D6FC File Offset: 0x0005B8FC
		private static bool TryFindTeleportPosition(string locationName, out Vector2 teleportPosition)
		{
			Submarine mainSub = Submarine.MainSub;
			if (mainSub != null && string.Equals(locationName, "mainsub", StringComparison.InvariantCultureIgnoreCase))
			{
				WayPoint randomWaypoint = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|61_0(mainSub.GetWaypoints(false));
				if (randomWaypoint != null)
				{
					teleportPosition = randomWaypoint.WorldPosition;
					return true;
				}
				DebugConsole.LogError("No waypoints found in the main sub!", null, null);
			}
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (string.Equals(submarine.Info.Name, locationName, StringComparison.InvariantCultureIgnoreCase))
				{
					WayPoint randomWaypoint2 = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|61_0(submarine.GetWaypoints(false));
					if (randomWaypoint2 != null)
					{
						teleportPosition = randomWaypoint2.WorldPosition;
						return true;
					}
					DebugConsole.LogError("No waypoints found in sub " + submarine.Info.Name + "!", null, null);
				}
			}
			Level loadedLevel = Level.Loaded;
			if (loadedLevel != null)
			{
				ValueTuple<string, int> valueTuple = DebugConsole.<TryFindTeleportPosition>g__SplitIndex|61_1(locationName);
				string locationNameNoIndex = valueTuple.Item1;
				int locationIndex = valueTuple.Item2;
				int caveIndex = 1;
				foreach (Level.Cave cave in loadedLevel.Caves)
				{
					if (string.Equals(cave.CaveGenerationParams.Name, locationNameNoIndex, StringComparison.InvariantCultureIgnoreCase))
					{
						if (caveIndex != locationIndex)
						{
							caveIndex++;
						}
						else
						{
							WayPoint randomWaypoint3 = DebugConsole.<TryFindTeleportPosition>g__GetRandomWaypoint|61_0(cave.Tunnels.GetRandom(Rand.RandSync.Unsynced).WayPoints);
							if (randomWaypoint3 != null)
							{
								teleportPosition = randomWaypoint3.WorldPosition;
								return true;
							}
							DebugConsole.LogError("No waypoints found in cave " + cave.CaveGenerationParams.Name + "!", null, null);
						}
					}
				}
			}
			teleportPosition = Vector2.Zero;
			return false;
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x0005D8EC File Offset: 0x0005BAEC
		private static TFile GetSubmarineFile<TFile>(string submarineName) where TFile : BaseSubFile
		{
			List<TFile> submarineFiles = DebugConsole.GetContentFiles<TFile>();
			using (List<TFile>.Enumerator enumerator = submarineFiles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TFile file = enumerator.Current;
					SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == file.Path.Value);
					if (matchingSub != null && string.Equals(matchingSub.Name, submarineName, StringComparison.InvariantCultureIgnoreCase))
					{
						return file;
					}
				}
			}
			return default(TFile);
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x0005D984 File Offset: 0x0005BB84
		private static List<TFile> GetContentFiles<TFile>() where TFile : ContentFile
		{
			return ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<TFile>()).ToList<TFile>();
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x0005D9C4 File Offset: 0x0005BBC4
		private static List<TFile> GetSubmarineFiles<TFile>() where TFile : BaseSubFile
		{
			return (from f in DebugConsole.GetContentFiles<TFile>()
			orderby f.UintIdentifier
			select f).ToList<TFile>();
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x0005DA04 File Offset: 0x0005BC04
		private static ContentFile GetContentFile(string path)
		{
			List<ContentFile> contentFiles = DebugConsole.GetContentFiles<ContentFile>();
			return contentFiles.FirstOrDefault((ContentFile file) => string.Equals(file.Path.Value, path, StringComparison.InvariantCultureIgnoreCase));
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x0005DA38 File Offset: 0x0005BC38
		private static string[] ListContentFilePaths()
		{
			List<string> contentFilePaths = new List<string>();
			List<ContentFile> contentFiles = DebugConsole.GetContentFiles<ContentFile>();
			foreach (ContentFile contentFile in contentFiles)
			{
				contentFilePaths.Add(contentFile.Path.Value);
			}
			return contentFilePaths.ToArray();
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x0005DAA4 File Offset: 0x0005BCA4
		private static string[] ListSubmarineFileNames<TFile>() where TFile : BaseSubFile
		{
			List<string> submarineFileNames = new List<string>();
			List<TFile> submarineFiles = DebugConsole.GetSubmarineFiles<TFile>();
			using (List<TFile>.Enumerator enumerator = submarineFiles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TFile file = enumerator.Current;
					SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == file.Path.Value);
					if (matchingSub != null)
					{
						submarineFileNames.Add(matchingSub.Name);
					}
				}
			}
			return submarineFileNames.ToArray();
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x0005DB34 File Offset: 0x0005BD34
		private static IOrderedEnumerable<Character> SortSpawnedSpecies(IEnumerable<Character> characterList)
		{
			return from c in characterList
			orderby c.IsDead, c.IsHuman descending, c.Name
			select c;
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x0005DBAE File Offset: 0x0005BDAE
		private static string[] ListCharacterNames(bool includeMeArgument = false, bool includeCrewArgument = false)
		{
			return DebugConsole.GetCharacterNames(includeMeArgument, includeCrewArgument);
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x0005DBB8 File Offset: 0x0005BDB8
		private static string[] GetCharacterNames(bool includeMeArgument = false, bool includeCrewArgument = false)
		{
			List<string> characterNames = new List<string>();
			if (includeMeArgument)
			{
				characterNames.Add("/me");
			}
			if (includeCrewArgument)
			{
				characterNames.Add("/crew");
			}
			characterNames.AddRange(from c in DebugConsole.SortSpawnedSpecies(Character.CharacterList)
			select c.Name);
			return characterNames.ToArray();
		}

		// Token: 0x060009D0 RID: 2512 RVA: 0x0005DC21 File Offset: 0x0005BE21
		private static string[] GetSpawnedSpeciesNames()
		{
			return (from c in DebugConsole.SortSpawnedSpecies(Character.CharacterList)
			select c.SpeciesName.Value).Distinct<string>().ToArray<string>();
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x0005DC5C File Offset: 0x0005BE5C
		private static IEnumerable<Character> FindMatchingSpecies(string[] args)
		{
			if (args.Length == 0)
			{
				return Array.Empty<Character>();
			}
			string speciesName = args[0].ToLowerInvariant();
			return DebugConsole.FindMatchingSpecies(speciesName);
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x0005DC84 File Offset: 0x0005BE84
		private static IEnumerable<Character> FindMatchingSpecies(string speciesName)
		{
			return Character.CharacterList.FindAll((Character c) => c.SpeciesName.Value.Equals(speciesName, StringComparison.OrdinalIgnoreCase));
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x0005DCB4 File Offset: 0x0005BEB4
		private static void HandleCommandForCrewOrSingleCharacter(string[] args, Action<Character> action, Client targetClient = null)
		{
			if (args.Length != 0 && args.First<string>() == "/crew")
			{
				using (ImmutableHashSet<Character>.Enumerator enumerator = GameSession.GetSessionCrewCharacters(CharacterType.Both).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character crewCharacter = enumerator.Current;
						action(crewCharacter);
					}
					return;
				}
			}
			Character targetCharacter = (args.Length == 0 || args.First<string>() == "/me") ? (((targetClient != null) ? targetClient.Character : null) ?? Character.Controlled) : DebugConsole.FindMatchingCharacter(args, false, null, false);
			if (targetCharacter == null)
			{
				return;
			}
			action(targetCharacter);
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x0005DD60 File Offset: 0x0005BF60
		private static Character FindMatchingCharacter(string[] args, bool ignoreRemotePlayers = false, Client allowedRemotePlayer = null, bool botsOnly = false)
		{
			if (args.Length == 0)
			{
				return null;
			}
			List<Character> matchingCharacters = null;
			string characterName = null;
			int characterIndex = -1;
			foreach (string arg in args)
			{
				if (arg == "/me")
				{
					Client allowedRemotePlayer2 = allowedRemotePlayer;
					return ((allowedRemotePlayer2 != null) ? allowedRemotePlayer2.Character : null) ?? Character.Controlled;
				}
				int possibleIndex;
				if (matchingCharacters == null || matchingCharacters.None(null))
				{
					string possibleCharacterName = (arg != null) ? arg.ToLowerInvariant() : null;
					matchingCharacters = Character.CharacterList.FindAll(delegate(Character c)
					{
						if (!c.Name.Equals(possibleCharacterName, StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						if (c.IsRemotePlayer && ignoreRemotePlayers)
						{
							Client allowedRemotePlayer3 = allowedRemotePlayer;
							return ((allowedRemotePlayer3 != null) ? allowedRemotePlayer3.Character : null) == c;
						}
						return true;
					});
					if (botsOnly)
					{
						matchingCharacters = matchingCharacters.FindAll((Character c) => c is AICharacter);
					}
					if (matchingCharacters.Any<Character>())
					{
						characterName = possibleCharacterName;
					}
				}
				else if (characterName != null && int.TryParse(arg, out possibleIndex))
				{
					characterIndex = possibleIndex;
				}
			}
			if (matchingCharacters == null || matchingCharacters.None(null))
			{
				DebugConsole.NewMessage("No matching character found!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
				return null;
			}
			matchingCharacters = (from c in matchingCharacters
			orderby c.IsDead, c.IsHuman descending
			select c).ToList<Character>();
			if (characterIndex == -1)
			{
				if (matchingCharacters.Count > 1)
				{
					DebugConsole.NewMessage("Found multiple matching characters. Use \"[charactername] [0-" + (matchingCharacters.Count - 1).ToString() + "]\" to choose a specific character.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.LightGray), false);
				}
				return matchingCharacters[0];
			}
			if (characterIndex < 0 || characterIndex >= matchingCharacters.Count)
			{
				DebugConsole.ThrowError("Character index out of range. Select an index between 0 and " + (matchingCharacters.Count - 1).ToString(), null, null, false, false);
				return null;
			}
			return matchingCharacters[characterIndex];
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x0005DF64 File Offset: 0x0005C164
		private static void TeleportCharacter(Vector2 cursorWorldPos, Character controlledCharacter, string[] args)
		{
			if (Screen.Selected != GameMain.GameScreen)
			{
				DebugConsole.NewMessage("Cannot teleport a character in the menu or the editor screens.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
				return;
			}
			Character targetCharacter = controlledCharacter;
			string locationNameArgument = "";
			string text = args.FirstOrDefault<string>();
			string firstArgument = ((text != null) ? text.ToLowerInvariant() : null) ?? string.Empty;
			if (args.Length != 0)
			{
				string lastArgument = args.Last<string>();
				if (!(firstArgument == "/me") && !(firstArgument == "/crew"))
				{
					string[] availableLocations = DebugConsole.ListAvailableLocations();
					if (args.Length > 1 || availableLocations.None((string locationName) => string.Equals(locationName, lastArgument, StringComparison.OrdinalIgnoreCase)))
					{
						targetCharacter = DebugConsole.FindMatchingCharacter(args, false, null, false);
					}
				}
				int num;
				if (args.Count<string>() > 1 && (targetCharacter == null || (!targetCharacter.Name.Equals(lastArgument, StringComparison.OrdinalIgnoreCase) && !int.TryParse(lastArgument, out num))))
				{
					locationNameArgument = lastArgument;
				}
			}
			if (firstArgument == "/crew")
			{
				using (ImmutableHashSet<Character>.Enumerator enumerator = GameSession.GetSessionCrewCharacters(CharacterType.Both).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character crewCharacter = enumerator.Current;
						DebugConsole.TeleportSpecificCharacter(crewCharacter, locationNameArgument, cursorWorldPos);
					}
					return;
				}
			}
			DebugConsole.TeleportSpecificCharacter(targetCharacter, locationNameArgument, cursorWorldPos);
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x0005E0C0 File Offset: 0x0005C2C0
		private static void TeleportSpecificCharacter(Character targetCharacter, string locationNameArgument, Vector2 defaultWorldPosition)
		{
			Vector2 worldPosition = defaultWorldPosition;
			if (!string.IsNullOrWhiteSpace(locationNameArgument) && !string.Equals(locationNameArgument, "cursor", StringComparison.InvariantCultureIgnoreCase))
			{
				Vector2 teleportPosition;
				if (!DebugConsole.TryFindTeleportPosition(locationNameArgument, out teleportPosition))
				{
					DebugConsole.ThrowError("No teleport position for location \"" + locationNameArgument + "\" was found.", null, null, false, false);
					return;
				}
				worldPosition = teleportPosition;
			}
			if (targetCharacter != null)
			{
				targetCharacter.TeleportTo(worldPosition);
				targetCharacter.AnimController.BodyInRest = false;
				return;
			}
			DebugConsole.NewMessage("Invalid arguments", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x0005E13C File Offset: 0x0005C33C
		private static void SpawnCharacter(string[] args, Vector2 cursorWorldPos, bool usePreConfiguredNPC = false)
		{
			DebugConsole.<>c__DisplayClass78_0 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass78_0();
			CS$<>8__locals1.args = args;
			CS$<>8__locals1.cursorWorldPos = cursorWorldPos;
			CS$<>8__locals1.characterArgumentCount = 1;
			if (usePreConfiguredNPC)
			{
				CS$<>8__locals1.characterArgumentCount = 2;
			}
			if (CS$<>8__locals1.args.Length < CS$<>8__locals1.characterArgumentCount)
			{
				return;
			}
			for (int i = 0; i < CS$<>8__locals1.characterArgumentCount; i++)
			{
				if (string.IsNullOrWhiteSpace(CS$<>8__locals1.args[i]))
				{
					return;
				}
			}
			CS$<>8__locals1.job = null;
			CS$<>8__locals1.isHuman = true;
			if (!usePreConfiguredNPC)
			{
				string characterLowerCase = CS$<>8__locals1.args[0].ToLowerInvariant();
				if (!JobPrefab.Prefabs.ContainsKey(characterLowerCase))
				{
					CS$<>8__locals1.job = JobPrefab.Prefabs.Find((JobPrefab jp) => jp.Name != null && jp.Name.Equals(characterLowerCase, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					CS$<>8__locals1.job = JobPrefab.Prefabs[characterLowerCase];
				}
				CS$<>8__locals1.isHuman = (CS$<>8__locals1.job != null || characterLowerCase == CharacterPrefab.HumanSpeciesName);
			}
			Vector2 spawnPosition;
			CS$<>8__locals1.<SpawnCharacter>g__ParseOptionalArgs|1(out spawnPosition, out CS$<>8__locals1.spawnPoint, out CS$<>8__locals1.teamType, out CS$<>8__locals1.addToCrew, out CS$<>8__locals1.renameCharacter);
			if (usePreConfiguredNPC)
			{
				DebugConsole.<>c__DisplayClass78_2 CS$<>8__locals3 = new DebugConsole.<>c__DisplayClass78_2();
				CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals1;
				Identifier npcSetIdentifier = CS$<>8__locals3.CS$<>8__locals1.args[0].ToIdentifier();
				CS$<>8__locals3.humanPrefabIdentifier = CS$<>8__locals3.CS$<>8__locals1.args[1].ToIdentifier();
				DebugConsole.<>c__DisplayClass78_2 CS$<>8__locals4 = CS$<>8__locals3;
				HumanPrefab humanPrefab;
				if (!(npcSetIdentifier == "any"))
				{
					humanPrefab = NPCSet.Get(npcSetIdentifier, CS$<>8__locals3.humanPrefabIdentifier, true, null);
				}
				else
				{
					humanPrefab = NPCSet.Sets.SelectMany((NPCSet set) => set.Humans).FirstOrDefault((HumanPrefab human) => human.Identifier == CS$<>8__locals3.humanPrefabIdentifier);
				}
				CS$<>8__locals4.humanPrefab = humanPrefab;
				if (CS$<>8__locals3.humanPrefab != null)
				{
					Entity.Spawner.AddCharacterToSpawnQueue(CharacterPrefab.HumanSpeciesName, spawnPosition, CS$<>8__locals3.humanPrefab.CreateCharacterInfo(Rand.RandSync.Unsynced), delegate(Character newCharacter)
					{
						newCharacter.HumanPrefab = CS$<>8__locals3.humanPrefab;
						CS$<>8__locals3.CS$<>8__locals1.<SpawnCharacter>g__SetTeamAndCrew|0(newCharacter);
						CS$<>8__locals3.humanPrefab.GiveItems(newCharacter, newCharacter.Submarine, CS$<>8__locals3.CS$<>8__locals1.spawnPoint, Rand.RandSync.Unsynced, true);
						CS$<>8__locals3.humanPrefab.InitializeCharacter(newCharacter, null);
					});
					return;
				}
			}
			else
			{
				if (CS$<>8__locals1.isHuman)
				{
					int variant = (CS$<>8__locals1.job != null) ? Rand.Range(0, CS$<>8__locals1.job.Variants, Rand.RandSync.ServerAndClient) : 0;
					CharacterInfo characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", CS$<>8__locals1.job, variant, Rand.RandSync.Unsynced, default(Identifier));
					Entity.Spawner.AddCharacterToSpawnQueue(CharacterPrefab.HumanSpeciesName, spawnPosition, characterInfo, delegate(Character newCharacter)
					{
						if (CS$<>8__locals1.renameCharacter != null)
						{
							if (CS$<>8__locals1.renameCharacter.Length > 31)
							{
								CS$<>8__locals1.renameCharacter = CS$<>8__locals1.renameCharacter.Substring(0, 32);
							}
							newCharacter.Info.Name = CS$<>8__locals1.renameCharacter;
						}
						base.<SpawnCharacter>g__SetTeamAndCrew|0(newCharacter);
						GameSession gameSession = GameMain.GameSession;
						newCharacter.GiveJobItems(((gameSession != null) ? gameSession.GameMode : null) is PvPMode, CS$<>8__locals1.spawnPoint);
						newCharacter.GiveIdCardTags(CS$<>8__locals1.spawnPoint, false);
						newCharacter.Info.StartItemsGiven = true;
					});
					return;
				}
				CharacterPrefab prefab = CharacterPrefab.FindBySpeciesName(CS$<>8__locals1.args[0].ToIdentifier());
				if (prefab != null)
				{
					Entity.Spawner.AddCharacterToSpawnQueue(CS$<>8__locals1.args[0].ToIdentifier(), spawnPosition, prefab.HasCharacterInfo ? new CharacterInfo(prefab.Identifier, "", "", null, 0, Rand.RandSync.Unsynced, default(Identifier)) : null, new Action<Character>(CS$<>8__locals1.<SpawnCharacter>g__SetTeamAndCrew|0));
				}
			}
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x0005E410 File Offset: 0x0005C610
		private static IEnumerable<string> GetSpawnPosParams()
		{
			return new DebugConsole.<GetSpawnPosParams>d__79(-2);
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x0005E419 File Offset: 0x0005C619
		private static IEnumerable<string> GetItemNameOrIdParams()
		{
			return new DebugConsole.<GetItemNameOrIdParams>d__80(-2);
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x0005E424 File Offset: 0x0005C624
		private static void TrySpawnItem(string[] args)
		{
			try
			{
				Camera cam = Screen.Selected.Cam;
				string errorMsg;
				DebugConsole.SpawnItem(args, (cam != null) ? cam.ScreenToWorld(PlayerInput.MousePosition) : PlayerInput.MousePosition, Character.Controlled, out errorMsg);
				if (!string.IsNullOrWhiteSpace(errorMsg))
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
			}
			catch (Exception e)
			{
				string errorMsg2 = "Failed to spawn an item. Arguments: \"" + string.Join(" ", args) + "\".";
				DebugConsole.ThrowError(errorMsg2, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("DebugConsole.SpawnItem:Error", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					errorMsg2,
					"\n",
					e.Message,
					"\n",
					e.StackTrace.CleanupStackTrace()
				}));
			}
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x0005E4EC File Offset: 0x0005C6EC
		public static void SpawnItem(string[] args, Vector2 cursorPos, Character controlledCharacter, out string errorMsg)
		{
			DebugConsole.<>c__DisplayClass83_0 CS$<>8__locals1 = new DebugConsole.<>c__DisplayClass83_0();
			CS$<>8__locals1.args = args;
			errorMsg = "";
			if (CS$<>8__locals1.args.Length < 1)
			{
				return;
			}
			Vector2? spawnPos = null;
			Inventory spawnInventory = null;
			CS$<>8__locals1.itemNameOrId = CS$<>8__locals1.args[0].ToLowerInvariant();
			ItemPrefab itemPrefab = (MapEntityPrefab.FindByName(CS$<>8__locals1.itemNameOrId) ?? MapEntityPrefab.FindByIdentifier(CS$<>8__locals1.itemNameOrId.ToIdentifier())) as ItemPrefab;
			if (itemPrefab == null)
			{
				errorMsg = "Item \"" + CS$<>8__locals1.itemNameOrId + "\" not found!";
				ItemPrefab matching = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Name.StartsWith(CS$<>8__locals1.itemNameOrId, StringComparison.OrdinalIgnoreCase) && me != null);
				if (matching != null)
				{
					string str = errorMsg;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendLiteral(" Did you mean \"");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(matching.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\"?");
					errorMsg = str + defaultInterpolatedStringHandler.ToStringAndClear();
					if (matching.Name.Contains(" ", StringComparison.Ordinal))
					{
						string str2 = errorMsg;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(95, 1);
						defaultInterpolatedStringHandler2.AppendLiteral(" Please note that you should surround multi-word names with quotation marks (e.q. spawnitem \"");
						defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(matching.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\")");
						errorMsg = str2 + defaultInterpolatedStringHandler2.ToStringAndClear();
					}
				}
				return;
			}
			int amount = 1;
			CS$<>8__locals1.conditionPrc = 100;
			CS$<>8__locals1.itemQuality = 0;
			string spawnLocation;
			int spawnLocationIndex;
			if (CS$<>8__locals1.<SpawnItem>g__TryGetSpawnPosParam|0(out spawnLocation, out spawnLocationIndex))
			{
				if (!(spawnLocation == "cursor"))
				{
					if (!(spawnLocation == "inventory"))
					{
						if (!(spawnLocation == "cargo"))
						{
							if (!(spawnLocation == "random"))
							{
								Character matchingCharacter = DebugConsole.FindMatchingCharacter(CS$<>8__locals1.args.Skip(1).Take(1).ToArray<string>(), false, null, false);
								if (matchingCharacter != null)
								{
									spawnInventory = matchingCharacter.Inventory;
								}
							}
						}
						else
						{
							WayPoint wp = WayPoint.GetRandom(SpawnType.Cargo, null, Submarine.MainSub, false, null, false);
							spawnPos = new Vector2?((wp == null) ? Vector2.Zero : wp.WorldPosition);
						}
					}
					else
					{
						spawnInventory = ((controlledCharacter != null) ? controlledCharacter.Inventory : null);
					}
				}
				else
				{
					spawnPos = new Vector2?(cursorPos);
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 1)
				{
					if (!int.TryParse(CS$<>8__locals1.args[spawnLocationIndex + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
					{
						amount = 1;
					}
					amount = Math.Min(amount, 100);
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 2 && !int.TryParse(CS$<>8__locals1.args[spawnLocationIndex + 2], NumberStyles.Any, CultureInfo.InvariantCulture, out CS$<>8__locals1.conditionPrc))
				{
					CS$<>8__locals1.conditionPrc = 100;
				}
				if (CS$<>8__locals1.args.Length > spawnLocationIndex + 3)
				{
					for (int i = 0; i <= 3; i++)
					{
						if (CS$<>8__locals1.args[spawnLocationIndex + 3].ToLowerInvariant() == DebugConsole.ItemQualityNames[i])
						{
							CS$<>8__locals1.itemQuality = i;
						}
					}
				}
			}
			float itemCondition = itemPrefab.Health * Math.Clamp((float)CS$<>8__locals1.conditionPrc / 100f, 0f, 1f);
			if ((spawnPos == null || spawnPos == Vector2.Zero) && spawnInventory == null)
			{
				WayPoint wp2 = WayPoint.GetRandom(SpawnType.Human, null, Submarine.MainSub, false, null, false);
				spawnPos = new Vector2?((wp2 == null) ? Vector2.Zero : wp2.WorldPosition);
			}
			for (int j = 0; j < amount; j++)
			{
				if (spawnPos != null)
				{
					if (Entity.Spawner == null || Entity.Spawner.Removed)
					{
						new Item(itemPrefab, spawnPos.Value, null, 0, true);
					}
					else
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner != null)
						{
							spawner.AddItemToSpawnQueue(itemPrefab, spawnPos.Value, new float?(itemCondition), new int?(CS$<>8__locals1.itemQuality), null);
						}
					}
				}
				else if (spawnInventory != null)
				{
					if (Entity.Spawner == null)
					{
						Item spawnedItem = new Item(itemPrefab, Vector2.Zero, null, 0, true);
						spawnInventory.TryPutItem(spawnedItem, null, spawnedItem.AllowedSlots, true, false, true);
						CS$<>8__locals1.<SpawnItem>g__onItemSpawned|3(spawnedItem);
					}
					else
					{
						EntitySpawner spawner2 = Entity.Spawner;
						if (spawner2 != null)
						{
							ItemPrefab itemPrefab2 = itemPrefab;
							Inventory inventory = spawnInventory;
							Action<Item> onSpawned = new Action<Item>(CS$<>8__locals1.<SpawnItem>g__onItemSpawned|3);
							spawner2.AddItemToSpawnQueue(itemPrefab2, inventory, null, null, onSpawned, true, false, InvSlotType.None);
						}
					}
				}
			}
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x0005E920 File Offset: 0x0005CB20
		public static void AddSafeError(string error)
		{
			DebugConsole.LogError(error, null, null);
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x0005E940 File Offset: 0x0005CB40
		public static void LogError(string msg, Microsoft.Xna.Framework.Color? color = null, ContentPackage contentPackage = null)
		{
			msg = DebugConsole.AddContentPackageInfoToMessage(msg, contentPackage);
			Microsoft.Xna.Framework.Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Microsoft.Xna.Framework.Color.Red;
				color = new Microsoft.Xna.Framework.Color?(value);
			}
			DebugConsole.NewMessage(msg, color.Value, false, true);
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x0005E984 File Offset: 0x0005CB84
		public static void NewCommand(string command, Microsoft.Xna.Framework.Color? color = null)
		{
			Microsoft.Xna.Framework.Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Microsoft.Xna.Framework.Color.White;
				color = new Microsoft.Xna.Framework.Color?(value);
			}
			DebugConsole.NewMessage(command, color.Value, true, false);
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x0005E9BF File Offset: 0x0005CBBF
		public static void NewMessage(LocalizedString msg, Microsoft.Xna.Framework.Color? color = null, bool debugOnly = false)
		{
			DebugConsole.NewMessage(msg.Value, color, debugOnly);
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x0005E9D0 File Offset: 0x0005CBD0
		public static void NewMessage(string msg, Microsoft.Xna.Framework.Color? color = null, bool debugOnly = false)
		{
			Microsoft.Xna.Framework.Color value = color.GetValueOrDefault();
			if (color == null)
			{
				value = Microsoft.Xna.Framework.Color.White;
				color = new Microsoft.Xna.Framework.Color?(value);
			}
			if (!debugOnly)
			{
				DebugConsole.NewMessage(msg, color.Value, false, false);
			}
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x0005EA10 File Offset: 0x0005CC10
		private static void NewMessage(string msg, Microsoft.Xna.Framework.Color color, bool isCommand, bool isError)
		{
			if (string.IsNullOrEmpty(msg))
			{
				return;
			}
			ColoredText newMsg = new ColoredText(msg, color, isCommand, isError);
			DebugConsole.queuedMessages.Enqueue(newMsg);
			DebugConsole.MessageHandler.Invoke(newMsg);
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x0005EA48 File Offset: 0x0005CC48
		public static void ShowQuestionPrompt(string question, DebugConsole.QuestionCallback onAnswered, string[] args = null, int argCount = -1)
		{
			if (args != null && args.Length > argCount)
			{
				onAnswered(args[argCount]);
				return;
			}
			RectTransform rectT = new RectTransform(new Point(DebugConsole.listBox.Content.Rect.Width, 0), DebugConsole.listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text = "   >>" + question;
			GUIFont smallFont = GUIStyle.SmallFont;
			DebugConsole.activeQuestionText = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, true, "", null)
			{
				CanBeFocused = false,
				TextColor = Microsoft.Xna.Framework.Color.Cyan
			};
			DebugConsole.activeQuestionCallback = (DebugConsole.QuestionCallback)Delegate.Combine(DebugConsole.activeQuestionCallback, onAnswered);
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x0005EB04 File Offset: 0x0005CD04
		private static bool TryParseTimeSpan(string s, out TimeSpan timeSpan)
		{
			timeSpan = default(TimeSpan);
			if (string.IsNullOrWhiteSpace(s))
			{
				return false;
			}
			string currNum = "";
			foreach (char c in s)
			{
				if (char.IsDigit(c))
				{
					ReadOnlySpan<char> str = currNum;
					char c2 = c;
					currNum = str + new ReadOnlySpan<char>(ref c2);
				}
				else if (!char.IsWhiteSpace(c))
				{
					int parsedNum;
					if (!int.TryParse(currNum, out parsedNum) || parsedNum < 0)
					{
						return false;
					}
					try
					{
						if (c <= 'h')
						{
							if (c == 'd')
							{
								timeSpan += new TimeSpan(parsedNum, 0, 0, 0, 0);
								goto IL_108;
							}
							if (c == 'h')
							{
								timeSpan += new TimeSpan(0, parsedNum, 0, 0, 0);
								goto IL_108;
							}
						}
						else
						{
							if (c == 'm')
							{
								timeSpan += new TimeSpan(0, 0, parsedNum, 0, 0);
								goto IL_108;
							}
							if (c == 's')
							{
								timeSpan += new TimeSpan(0, 0, 0, parsedNum, 0);
								goto IL_108;
							}
						}
						return false;
						IL_108:;
					}
					catch (ArgumentOutOfRangeException)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 3);
						defaultInterpolatedStringHandler.AppendFormatted<int>(parsedNum);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<char>(c);
						defaultInterpolatedStringHandler.AppendLiteral(" exceeds the maximum supported time span. Using the maximum time span ");
						defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(TimeSpan.MaxValue);
						defaultInterpolatedStringHandler.AppendLiteral(" instead.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						timeSpan = TimeSpan.MaxValue;
						return true;
					}
					currNum = "";
				}
			}
			return true;
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x0005ECB4 File Offset: 0x0005CEB4
		public static DebugConsole.Command FindCommand(string commandName)
		{
			return DebugConsole.commands.Find((DebugConsole.Command c) => c.Names.Contains(commandName.ToIdentifier()));
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x0005ECE4 File Offset: 0x0005CEE4
		public static void Log(LocalizedString message)
		{
			DebugConsole.Log((message != null) ? message.Value : null);
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x0005ECF7 File Offset: 0x0005CEF7
		public static void Log(string message)
		{
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(message, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Gray), false);
			}
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x0005ED16 File Offset: 0x0005CF16
		public static void ThrowErrorLocalized(LocalizedString error, Exception e = null, ContentPackage contentPackage = null, bool createMessageBox = false, bool appendStackTrace = false)
		{
			DebugConsole.ThrowError(error.Value, e, contentPackage, createMessageBox, appendStackTrace);
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x0005ED28 File Offset: 0x0005CF28
		public static void ThrowError(string error, Exception e = null, ContentPackage contentPackage = null, bool createMessageBox = false, bool appendStackTrace = false)
		{
			error = DebugConsole.AddContentPackageInfoToMessage(error, contentPackage);
			SteamTimelineManager.OnError(error, e);
			if (e != null)
			{
				error = error + " {" + e.Message + "}\n";
				if (e.StackTrace != null)
				{
					error += e.StackTrace.CleanupStackTrace();
				}
				if (e.InnerException != null)
				{
					Exception innermost = e.GetInnermost();
					error = error + "\n\nInner exception: " + innermost.Message + "\n";
					if (innermost.StackTrace != null)
					{
						error += innermost.StackTrace.CleanupStackTrace();
					}
				}
			}
			else if (appendStackTrace && Environment.StackTrace != null)
			{
				error = error + "\n" + Environment.StackTrace.CleanupStackTrace();
			}
			if (createMessageBox)
			{
				CoroutineManager.StartCoroutine(DebugConsole.CreateMessageBox(error), "");
			}
			else
			{
				DebugConsole.isOpen = true;
			}
			DebugConsole.LogError(error, null, null);
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x0005EE0D File Offset: 0x0005D00D
		public static void ThrowErrorAndLogToGA(string gaIdentifier, string errorMsg)
		{
			DebugConsole.ThrowError(errorMsg, null, null, false, false);
			GameAnalyticsManager.AddErrorEventOnce(gaIdentifier, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x0005EE21 File Offset: 0x0005D021
		public static void ThrowErrorOnce(string identifier, string errorMsg, Exception e = null)
		{
			if (DebugConsole.loggedErrorIdentifiers.Contains(identifier))
			{
				return;
			}
			DebugConsole.ThrowError(errorMsg, e, null, false, false);
			DebugConsole.loggedErrorIdentifiers.Add(identifier);
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x0005EE47 File Offset: 0x0005D047
		public static void AddWarning(string warning, ContentPackage contentPackage = null)
		{
			warning = DebugConsole.AddContentPackageInfoToMessage("WARNING: " + warning, contentPackage);
			DebugConsole.NewMessage(warning, new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x0005EE70 File Offset: 0x0005D070
		private static string AddContentPackageInfoToMessage(string message, ContentPackage contentPackage)
		{
			if (contentPackage == null)
			{
				return message;
			}
			string color = Microsoft.Xna.Framework.Color.MediumPurple.ToStringHex();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 3);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler.AppendFormatted(color);
			defaultInterpolatedStringHandler.AppendLiteral("‖[");
			defaultInterpolatedStringHandler.AppendFormatted(contentPackage.Name);
			defaultInterpolatedStringHandler.AppendLiteral("]‖color:end‖ ");
			defaultInterpolatedStringHandler.AppendFormatted(message);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x0005EEDF File Offset: 0x0005D0DF
		private static IEnumerable<CoroutineStatus> CreateMessageBox(string errorMsg)
		{
			DebugConsole.<CreateMessageBox>d__102 <CreateMessageBox>d__ = new DebugConsole.<CreateMessageBox>d__102(-2);
			<CreateMessageBox>d__.<>3__errorMsg = errorMsg;
			return <CreateMessageBox>d__;
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x0005EEF0 File Offset: 0x0005D0F0
		public static void SaveLogs()
		{
			if (DebugConsole.unsavedMessages.Count == 0)
			{
				return;
			}
			if (!Directory.Exists("ConsoleLogs"))
			{
				try
				{
					Directory.CreateDirectory("ConsoleLogs", false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to create a folder for debug console logs", e, null, false, false);
					return;
				}
			}
			string fileName = "DebugConsoleLog_";
			fileName += "Client_";
			fileName = fileName + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
			ImmutableHashSet<char> invalidChars = Path.GetInvalidFileNameCharsCrossPlatform();
			foreach (char invalidChar in invalidChars)
			{
				fileName = fileName.Replace(invalidChar.ToString(), "");
			}
			string filePath = Path.Combine(new string[]
			{
				"ConsoleLogs",
				fileName
			});
			if (File.Exists(filePath + ".txt"))
			{
				int fileNum = 2;
				while (File.Exists(filePath + " (" + fileNum.ToString() + ")"))
				{
					fileNum++;
				}
				filePath = filePath + " (" + fileNum.ToString() + ")";
			}
			try
			{
				File.WriteAllLines(filePath + ".txt", from l in DebugConsole.unsavedMessages
				select "[" + l.Time + "] " + l.Text, null, false);
			}
			catch (Exception e2)
			{
				DebugConsole.unsavedMessages.Clear();
				DebugConsole.ThrowError("Saving debug console log to " + filePath + " failed", e2, null, false, false);
			}
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x0005F0B0 File Offset: 0x0005D2B0
		private static void ToggleEnemyAITargetingRestrictions(EnemyTargetingRestrictions restrictions)
		{
			if (restrictions == EnemyTargetingRestrictions.None)
			{
				EnemyAIController.TargetingRestrictions = EnemyTargetingRestrictions.None;
			}
			else if (EnemyAIController.TargetingRestrictions.HasFlag(restrictions))
			{
				EnemyAIController.TargetingRestrictions &= ~restrictions;
			}
			else
			{
				EnemyAIController.TargetingRestrictions |= restrictions;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Monster targeting restrictions is now '");
			defaultInterpolatedStringHandler.AppendFormatted<EnemyTargetingRestrictions>(EnemyAIController.TargetingRestrictions);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Yellow), false);
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x0005F13F File Offset: 0x0005D33F
		public static void DeactivateCheats()
		{
			GameMain.DebugDraw = false;
			GameMain.LightManager.LightingEnabled = true;
			Character.DebugDrawInteract = false;
			Hull.EditWater = false;
			Hull.EditFire = false;
			EnemyAIController.DisableEnemyAI = false;
			HumanAIController.DisableCrewAI = false;
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x0005F170 File Offset: 0x0005D370
		[CompilerGenerated]
		internal static string <InitProjectSpecific>g__GetSeller|23_182(PriceInfo priceInfo)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
			defaultInterpolatedStringHandler.AppendLiteral("store with identifier \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(priceInfo.StoreIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x0005F1B4 File Offset: 0x0005D3B4
		[CompilerGenerated]
		internal unsafe static void <InitProjectSpecific>g__SwapLanguage|23_194(LanguageIdentifier language)
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.Language = language;
			GameSettings.SetCurrentConfig(config);
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x0005F1DC File Offset: 0x0005D3DC
		[CompilerGenerated]
		internal static Dictionary<string, string> <InitProjectSpecific>g__getContent|23_211(XElement element)
		{
			Dictionary<string, string> content = new Dictionary<string, string>();
			foreach (XElement subElement in element.Elements())
			{
				string key = subElement.Name.ToString().ToLowerInvariant();
				if (!content.ContainsKey(key))
				{
					content.Add(key, subElement.ElementInnerText());
				}
			}
			return content;
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x0005F250 File Offset: 0x0005D450
		[CompilerGenerated]
		internal static void <InitProjectSpecific>g__getTextsFromElement|23_220(XElement element, List<string> list, string parentName, ref DebugConsole.<>c__DisplayClass23_19 A_3)
		{
			string text = element.GetAttributeString("text", null);
			string textAttribute = "text";
			XElement textElement = element;
			if (text == null)
			{
				XElement subTextElement = (element != null) ? element.Element("Text") : null;
				if (subTextElement != null)
				{
					textAttribute = "tag";
					text = ((subTextElement != null) ? subTextElement.GetAttributeString(textAttribute, null) : null);
					textElement = subTextElement;
				}
			}
			string textId = "EventText." + parentName;
			if (!string.IsNullOrEmpty(text) && !text.StartsWith("EventText.", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("Tutorial.", StringComparison.OrdinalIgnoreCase))
			{
				string existingTextId;
				if (A_3.existingTexts.TryGetValue(text, out existingTextId))
				{
					textElement.SetAttributeValue(textAttribute, existingTextId);
				}
				else
				{
					A_3.textIds.Add(parentName);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 3);
					defaultInterpolatedStringHandler.AppendLiteral("<");
					defaultInterpolatedStringHandler.AppendFormatted(textId);
					defaultInterpolatedStringHandler.AppendLiteral(">");
					defaultInterpolatedStringHandler.AppendFormatted(text);
					defaultInterpolatedStringHandler.AppendLiteral("</");
					defaultInterpolatedStringHandler.AppendFormatted(textId);
					defaultInterpolatedStringHandler.AppendLiteral(">");
					list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
					A_3.existingTexts.Add(text, textId);
					textElement.SetAttributeValue(textAttribute, textId);
				}
			}
			int conversationIndex = 1;
			int objectiveIndex = 1;
			foreach (XElement subElement in element.Elements())
			{
				string elementName = parentName;
				bool ignore = false;
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "conversationaction"))
				{
					if (!(a == "eventlogaction"))
					{
						if (!(a == "option"))
						{
							if (a == "text")
							{
								ignore = true;
							}
						}
						else
						{
							while (A_3.textIds.Contains(elementName.Substring(0, elementName.Length - 3) + ".o" + conversationIndex.ToString()))
							{
								conversationIndex++;
							}
							elementName = elementName.Substring(0, elementName.Length - 3) + ".o" + conversationIndex.ToString();
						}
					}
					else
					{
						while (A_3.textIds.Contains(elementName + ".objective" + objectiveIndex.ToString()))
						{
							objectiveIndex++;
						}
						elementName = elementName + ".objective" + objectiveIndex.ToString();
					}
				}
				else
				{
					while (A_3.textIds.Contains(elementName + ".c" + conversationIndex.ToString()))
					{
						conversationIndex++;
					}
					elementName = elementName + ".c" + conversationIndex.ToString();
				}
				if (!ignore)
				{
					DebugConsole.<InitProjectSpecific>g__getTextsFromElement|23_220(subElement, list, elementName, ref A_3);
				}
			}
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0005F52C File Offset: 0x0005D72C
		[CompilerGenerated]
		internal static void <.cctor>g__printMapEntityPrefabs|54_2<T>(IEnumerable<T> prefabs) where T : MapEntityPrefab
		{
			DebugConsole.NewMessage("***************", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
			foreach (T prefab in prefabs)
			{
				if (!prefab.Name.IsNullOrEmpty())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler.AppendLiteral("- ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(prefab.Name);
					string text = defaultInterpolatedStringHandler.ToStringAndClear();
					if (prefab.Tags.Any<Identifier>())
					{
						text = text + " (" + string.Join<Identifier>(", ", prefab.Tags) + ")";
					}
					ImmutableHashSet<Identifier> allowedLinks = prefab.AllowedLinks;
					if (allowedLinks != null && allowedLinks.Any<Identifier>())
					{
						text = text + ", Links: " + string.Join<Identifier>(", ", prefab.AllowedLinks);
					}
					DebugConsole.NewMessage(text, new Microsoft.Xna.Framework.Color?((prefab.ContentPackage == ContentPackageManager.VanillaCorePackage) ? Microsoft.Xna.Framework.Color.Cyan : Microsoft.Xna.Framework.Color.Purple), false);
				}
			}
			DebugConsole.NewMessage("***************", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Cyan), false);
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x0005F67C File Offset: 0x0005D87C
		[CompilerGenerated]
		internal static WayPoint <TryFindTeleportPosition>g__GetRandomWaypoint|61_0(IReadOnlyList<WayPoint> waypoints)
		{
			if (waypoints.None(null))
			{
				return null;
			}
			if (waypoints.Any((WayPoint point) => point.SpawnType == SpawnType.Human))
			{
				return waypoints.GetRandom((WayPoint point) => point.SpawnType == SpawnType.Human, Rand.RandSync.Unsynced);
			}
			if (waypoints.Any((WayPoint point) => point.SpawnType == SpawnType.Path))
			{
				return waypoints.GetRandom((WayPoint point) => point.SpawnType == SpawnType.Path, Rand.RandSync.Unsynced);
			}
			return waypoints.GetRandom(Rand.RandSync.Unsynced);
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x0005F738 File Offset: 0x0005D938
		[CompilerGenerated]
		internal static ValueTuple<string, int> <TryFindTeleportPosition>g__SplitIndex|61_1(string caveName)
		{
			string[] splitName = caveName.Split('_', StringSplitOptions.None);
			if (splitName.Length == 1)
			{
				return new ValueTuple<string, int>(splitName[0], -1);
			}
			return new ValueTuple<string, int>(splitName[0], int.Parse(splitName[1]));
		}

		// Token: 0x04000507 RID: 1287
		private static bool isOpen;

		// Token: 0x04000508 RID: 1288
		public static bool Paused = false;

		// Token: 0x04000509 RID: 1289
		private static GUITextBlock activeQuestionText;

		// Token: 0x0400050A RID: 1290
		private static GUIFrame frame;

		// Token: 0x0400050B RID: 1291
		private static GUIListBox listBox;

		// Token: 0x0400050C RID: 1292
		private static GUITextBox textBox;

		// Token: 0x0400050D RID: 1293
		private const int maxLength = 1000;

		// Token: 0x0400050E RID: 1294
		private static readonly ChatManager chatManager = new ChatManager(true, 64);

		// Token: 0x0400050F RID: 1295
		private static readonly ConcurrentQueue<ColoredText> queuedMessages = new ConcurrentQueue<ColoredText>();

		// Token: 0x04000510 RID: 1296
		public static readonly NamedEvent<ColoredText> MessageHandler = new NamedEvent<ColoredText>();

		// Token: 0x04000511 RID: 1297
		private const int MaxMessages = 300;

		// Token: 0x04000512 RID: 1298
		public static readonly List<ColoredText> Messages = new List<ColoredText>();

		// Token: 0x04000513 RID: 1299
		private static DebugConsole.QuestionCallback activeQuestionCallback;

		// Token: 0x04000514 RID: 1300
		private static readonly List<DebugConsole.Command> commands = new List<DebugConsole.Command>();

		// Token: 0x04000515 RID: 1301
		private static string currentAutoCompletedCommand;

		// Token: 0x04000516 RID: 1302
		private static int currentAutoCompletedIndex;

		// Token: 0x04000517 RID: 1303
		public static bool CheatsEnabled;

		// Token: 0x04000518 RID: 1304
		private static readonly List<ColoredText> unsavedMessages = new List<ColoredText>();

		// Token: 0x04000519 RID: 1305
		private static readonly int messagesPerFile = 800;

		// Token: 0x0400051A RID: 1306
		public const string SavePath = "ConsoleLogs";

		// Token: 0x0400051B RID: 1307
		private static WeakReference<Character> previousControlledCharacter;

		// Token: 0x0400051C RID: 1308
		private static ImmutableArray<string> ItemQualityNames = ImmutableCollectionsMarshal.AsImmutableArray<string>(new string[]
		{
			"normal",
			"good",
			"excellent",
			"masterwork"
		});

		// Token: 0x0400051D RID: 1309
		private static readonly HashSet<string> loggedErrorIdentifiers = new HashSet<string>();

		// Token: 0x02000740 RID: 1856
		public class Command
		{
			// Token: 0x0600685D RID: 26717 RVA: 0x0034CC30 File Offset: 0x0034AE30
			public void ClientExecute(string[] args)
			{
				bool flag;
				if (GameMain.NetworkMember == null)
				{
					GameSession gameSession = GameMain.GameSession;
					if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode))
					{
						Screen selected = Screen.Selected;
						flag = (selected != null && selected.IsEditor);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag && !DebugConsole.CheatsEnabled && this.IsCheat)
				{
					DebugConsole.NewMessage("You need to enable cheats using the command \"enablecheats\" before you can use the command \"" + this.Names[0].ToString() + "\".", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					DebugConsole.NewMessage("Enabling cheats will disable achievements during this play session.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				if (this.OnClientExecute != null)
				{
					this.OnClientExecute(args);
					return;
				}
				this.OnExecute(args);
			}

			// Token: 0x0600685E RID: 26718 RVA: 0x0034CCF8 File Offset: 0x0034AEF8
			public Command(string name, string help, Action<string[]> onExecute, Func<string[][]> getValidArgs = null, bool isCheat = false)
			{
				this.Names = name.Split('|', StringSplitOptions.None).ToIdentifiers().ToImmutableArray<Identifier>();
				this.Help = help;
				this.OnExecute = onExecute;
				this.GetValidArgs = getValidArgs;
				this.IsCheat = isCheat;
			}

			// Token: 0x0600685F RID: 26719 RVA: 0x0034CD50 File Offset: 0x0034AF50
			public void Execute(string[] args)
			{
				if (this.OnExecute == null)
				{
					return;
				}
				bool flag;
				if (GameMain.NetworkMember == null)
				{
					GameSession gameSession = GameMain.GameSession;
					if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode))
					{
						Screen selected = Screen.Selected;
						flag = (selected != null && selected.IsEditor);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = false;
				}
				if (!flag && !DebugConsole.CheatsEnabled && this.IsCheat)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 1);
					defaultInterpolatedStringHandler.AppendLiteral("You need to enable cheats using the command \"enablecheats\" before you can use the command \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Names.First<Identifier>());
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					DebugConsole.NewMessage("Enabling cheats will disable Steam achievements during this play session.", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Red), false);
					return;
				}
				this.OnExecute(args);
			}

			// Token: 0x06006860 RID: 26720 RVA: 0x0034CE20 File Offset: 0x0034B020
			public override int GetHashCode()
			{
				return this.Names.First<Identifier>().GetHashCode();
			}

			// Token: 0x04003932 RID: 14642
			public Action<string[]> OnClientExecute;

			// Token: 0x04003933 RID: 14643
			public bool RelayToServer = true;

			// Token: 0x04003934 RID: 14644
			public readonly ImmutableArray<Identifier> Names;

			// Token: 0x04003935 RID: 14645
			public readonly string Help;

			// Token: 0x04003936 RID: 14646
			public Action<string[]> OnExecute;

			// Token: 0x04003937 RID: 14647
			public Func<string[][]> GetValidArgs;

			// Token: 0x04003938 RID: 14648
			public readonly bool IsCheat;
		}

		// Token: 0x02000741 RID: 1857
		private enum AdjustItemTypes
		{
			// Token: 0x0400393A RID: 14650
			NoAdjustment,
			// Token: 0x0400393B RID: 14651
			Additive,
			// Token: 0x0400393C RID: 14652
			Multiplicative
		}

		// Token: 0x02000742 RID: 1858
		public struct ErrorCatcher : IDisposable
		{
			// Token: 0x170019E3 RID: 6627
			// (get) Token: 0x06006861 RID: 26721 RVA: 0x0034CE46 File Offset: 0x0034B046
			public IReadOnlyList<ColoredText> Errors
			{
				get
				{
					return this.errors;
				}
			}

			// Token: 0x06006862 RID: 26722 RVA: 0x0034CE50 File Offset: 0x0034B050
			private ErrorCatcher(Identifier handlerId)
			{
				this.handlerId = handlerId;
				this.wasConsoleOpen = DebugConsole.IsOpen;
				this.errors = new List<ColoredText>();
				List<ColoredText> errs = this.errors;
				DebugConsole.MessageHandler.Register(handlerId, delegate(ColoredText msg)
				{
					if (!msg.IsError)
					{
						return;
					}
					errs.Add(msg);
				});
			}

			// Token: 0x06006863 RID: 26723 RVA: 0x0034CEA3 File Offset: 0x0034B0A3
			public static DebugConsole.ErrorCatcher Create()
			{
				return new DebugConsole.ErrorCatcher(ToolBox.RandomSeed(25).ToIdentifier());
			}

			// Token: 0x06006864 RID: 26724 RVA: 0x0034CEB6 File Offset: 0x0034B0B6
			public void Dispose()
			{
				if (this.handlerId.IsEmpty)
				{
					return;
				}
				DebugConsole.MessageHandler.Deregister(this.handlerId);
				this.handlerId = Identifier.Empty;
				DebugConsole.IsOpen = this.wasConsoleOpen;
			}

			// Token: 0x0400393D RID: 14653
			private readonly List<ColoredText> errors;

			// Token: 0x0400393E RID: 14654
			private readonly bool wasConsoleOpen;

			// Token: 0x0400393F RID: 14655
			private Identifier handlerId;
		}

		// Token: 0x02000743 RID: 1859
		// (Invoke) Token: 0x06006866 RID: 26726
		public delegate void QuestionCallback(string answer);

		// Token: 0x02000744 RID: 1860
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003940 RID: 14656
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <0>__IgnoredCallback;
		}
	}
}
