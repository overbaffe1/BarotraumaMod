using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Networking
{
	// Token: 0x02000470 RID: 1136
	public class ServerLog
	{
		// Token: 0x06004C91 RID: 19601 RVA: 0x002A3170 File Offset: 0x002A1370
		private bool OnReverseClicked(GUIButton btn, object obj)
		{
			this.SetMessageReversal(!this.reverseOrder);
			return false;
		}

		// Token: 0x06004C92 RID: 19602 RVA: 0x002A3184 File Offset: 0x002A1384
		public void CreateLogFrame()
		{
			this.LogFrame = new GUIButton(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock)
					{
						this.LogFrame = null;
					}
					return true;
				}
			};
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.LogFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			GUIButton guibutton = new GUIButton(new RectTransform(Vector2.One, this.LogFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", Alignment.Center, null, null);
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				this.LogFrame = null;
				return true;
			}));
			GUIFrame innerFrame = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.5f), this.LogFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(700, 500)
			}, "", null);
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.95f, 0.9f), innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup tickBoxContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.25f, 1f), paddedFrame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			int y = 30;
			List<GUITickBox> tickBoxes = new List<GUITickBox>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(ServerLog.MessageType)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ServerLog.MessageType msgType = (ServerLog.MessageType)enumerator.Current;
					GUITickBox tickBox = new GUITickBox(new RectTransform(new Point(tickBoxContainer.Rect.Width, 30), tickBoxContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("ServerLog." + this.messageTypeName[msgType]), GUIStyle.SmallFont, "")
					{
						Selected = true,
						TextColor = this.messageColor[msgType],
						OnSelected = delegate(GUITickBox tb)
						{
							this.msgTypeHidden[(int)msgType] = !tb.Selected;
							this.FilterMessages();
							return true;
						}
					};
					tickBox.TextBlock.SelectedTextColor = tickBox.TextBlock.TextColor;
					tickBox.Selected = !this.msgTypeHidden[(int)msgType];
					tickBoxes.Add(tickBox);
					y += 20;
				}
			}
			tickBoxes.Last<GUITickBox>().TextBlock.RectTransform.SizeChanged += delegate()
			{
				GUITextBlock.AutoScaleAndNormalize(from t in tickBoxes
				select t.TextBlock, true, false, new float?(1f));
			};
			GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(new Vector2(0.75f, 1f), paddedFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), false, Anchor.TopRight)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			GUILayoutGroup filterArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.05f), rightColumn.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			RectTransform rectT = new RectTransform(new Vector2(0.2f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("ServerLog.Filter");
			GUIFont font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text3, null, font, Alignment.Left, false, "", null);
			RectTransform rectT2 = new RectTransform(new Vector2(0.8f, 1f), filterArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.SmallFont;
			GUITextBox searchBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, true, true);
			searchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.msgFilter = text;
				this.FilterMessages();
				return true;
			};
			GUI.KeyboardDispatcher.Subscriber = searchBox;
			filterArea.RectTransform.MinSize = new Point(0, filterArea.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			GUILayoutGroup listBoxLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.95f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0f
			};
			this.reverseButton = new GUIButton(new RectTransform(new Vector2(1f, 0.05f), listBoxLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "UIToggleButtonVertical", null);
			this.reverseButton.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = (this.reverseOrder ? SpriteEffects.FlipVertically : SpriteEffects.None);
			});
			this.reverseButton.OnClicked = new GUIButton.OnClickedHandler(this.OnReverseClicked);
			this.listBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.95f), listBoxLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				AutoHideScrollBar = false
			};
			new GUIButton(new RectTransform(new Vector2(0.25f, 0.05f), rightColumn.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object userData)
			{
				this.LogFrame = null;
				return true;
			};
			rightColumn.Recalculate();
			List<ServerLog.LogMessage> currLines = this.lines.ToList<ServerLog.LogMessage>();
			foreach (ServerLog.LogMessage line in currLines)
			{
				this.AddLine(line);
			}
			this.FilterMessages();
			this.listBox.UpdateScrollBarSize();
			this.listBox.BarScroll = 1f;
			this.msgFilter = "";
		}

		// Token: 0x06004C93 RID: 19603 RVA: 0x002A3950 File Offset: 0x002A1B50
		public void AssignLogFrame(GUIButton inReverseButton, GUIListBox inListBox, GUIComponent tickBoxContainer, GUITextBox searchBox)
		{
			searchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				this.msgFilter = text;
				this.FilterMessages();
				return true;
			};
			tickBoxContainer.ClearChildren();
			List<GUITickBox> tickBoxes = new List<GUITickBox>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(ServerLog.MessageType)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ServerLog.MessageType msgType = (ServerLog.MessageType)enumerator.Current;
					GUITickBox tickBox = new GUITickBox(new RectTransform(new Point(tickBoxContainer.Rect.Width, (int)(25f * GUI.Scale)), tickBoxContainer.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("ServerLog." + this.messageTypeName[msgType]).Fallback(this.messageTypeName[msgType], true), GUIStyle.SmallFont, "")
					{
						Selected = true,
						TextColor = this.messageColor[msgType],
						OnSelected = delegate(GUITickBox tb)
						{
							this.msgTypeHidden[(int)msgType] = !tb.Selected;
							this.FilterMessages();
							return true;
						}
					};
					tickBox.TextBlock.SelectedTextColor = tickBox.TextBlock.TextColor;
					tickBox.Selected = !this.msgTypeHidden[(int)msgType];
					tickBoxes.Add(tickBox);
				}
			}
			tickBoxes.Last<GUITickBox>().TextBlock.RectTransform.SizeChanged += delegate()
			{
				GUITextBlock.AutoScaleAndNormalize(from t in tickBoxes
				select t.TextBlock, true, false, new float?(1f));
			};
			inListBox.ClearChildren();
			this.listBox = inListBox;
			this.reverseButton = inReverseButton;
			this.reverseButton.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = (this.reverseOrder ? SpriteEffects.FlipVertically : SpriteEffects.None);
			});
			this.reverseButton.OnClicked = new GUIButton.OnClickedHandler(this.OnReverseClicked);
			List<ServerLog.LogMessage> currLines = this.lines.ToList<ServerLog.LogMessage>();
			foreach (ServerLog.LogMessage line in currLines)
			{
				this.AddLine(line);
			}
			this.FilterMessages();
			this.listBox.UpdateScrollBarSize();
		}

		// Token: 0x06004C94 RID: 19604 RVA: 0x002A3BCC File Offset: 0x002A1DCC
		private void AddLine(ServerLog.LogMessage line)
		{
			float prevSize = this.listBox.BarSize;
			GUIComponent firstVisibleLine = this.listBox.Content.Children.FirstOrDefault((GUIComponent c) => c.Rect.Y > this.listBox.Content.Rect.Y);
			int firstVisibileYPos = (firstVisibleLine != null) ? firstVisibleLine.Rect.Y : 0;
			while (this.listBox.Content.CountChildren > 500)
			{
				this.listBox.Content.RemoveChild(this.reverseOrder ? this.listBox.Content.Children.Last<GUIComponent>() : this.listBox.Content.Children.First<GUIComponent>());
			}
			GUIFrame textContainer = null;
			Anchor anchor = Anchor.TopLeft;
			Pivot pivot = Pivot.TopLeft;
			RichString richString = line.Text;
			if (richString != null && richString.RichTextData != null)
			{
				foreach (RichTextData data in richString.RichTextData.Value)
				{
					Client client = data.ExtractClient();
					if (client != null && client.Karma < 40f)
					{
						textContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0f), this.listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(new Color(4279308629U)))
						{
							CanBeFocused = false
						};
						anchor = Anchor.CenterLeft;
						pivot = Pivot.CenterLeft;
						break;
					}
				}
			}
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), (textContainer ?? this.listBox.Content).RectTransform, anchor, new Pivot?(pivot), null, null, ScaleBasis.Normal);
			RichString text = line.Text;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, true, "", null)
			{
				TextColor = this.messageColor[line.Type],
				Visible = !this.ShouldFilterMessage(line),
				CanBeFocused = false,
				UserData = line
			};
			if (textContainer != null)
			{
				textContainer.RectTransform.NonScaledSize = new Point(textContainer.RectTransform.NonScaledSize.X, textBlock.RectTransform.NonScaledSize.Y + 5);
				textBlock.SetTextPos();
				textBlock.RectTransform.Resize(textContainer.RectTransform.NonScaledSize, true);
			}
			if (this.reverseOrder)
			{
				textBlock.RectTransform.SetAsFirstChild();
			}
			if (richString != null && richString.RichTextData != null)
			{
				foreach (RichTextData data2 in richString.RichTextData.Value)
				{
					textBlock.ClickableAreas.Add(new GUITextBlock.ClickableArea
					{
						Data = data2,
						OnClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.SelectPlayer),
						OnSecondaryClick = new GUITextBlock.ClickableArea.OnClickDelegate(GameMain.NetLobbyScreen.ShowPlayerContextMenu)
					});
				}
			}
			if ((MathUtils.NearlyEqual(prevSize, 1f, 0.0001f) && MathUtils.NearlyEqual(this.listBox.BarScroll, 0f, 0.0001f)) || (prevSize < 1f && MathUtils.NearlyEqual(this.listBox.BarScroll, 1f, 0.0001f)))
			{
				this.listBox.BarScroll = 1f;
				return;
			}
			if (firstVisibleLine != null)
			{
				this.listBox.UpdateScrollBarSize();
				this.listBox.RecalculateChildren();
				int diff = firstVisibleLine.Rect.Y - firstVisibileYPos;
				if (diff != 0)
				{
					this.listBox.BarScroll += (float)diff / this.listBox.TotalSize * (prevSize / this.listBox.BarSize);
				}
			}
		}

		// Token: 0x06004C95 RID: 19605 RVA: 0x002A3FD8 File Offset: 0x002A21D8
		private bool FilterMessages()
		{
			foreach (GUIComponent child in this.listBox.Content.Children)
			{
				if (child is GUITextBlock)
				{
					child.Visible = true;
					child.Visible = !this.ShouldFilterMessage((ServerLog.LogMessage)child.UserData);
				}
			}
			this.listBox.UpdateScrollBarSize();
			this.listBox.BarScroll = 1f;
			return true;
		}

		// Token: 0x06004C96 RID: 19606 RVA: 0x002A4070 File Offset: 0x002A2270
		private bool ShouldFilterMessage(ServerLog.LogMessage message)
		{
			if (this.msgTypeHidden[(int)message.Type])
			{
				return true;
			}
			string text = message.Text.SanitizedValue;
			return !string.IsNullOrEmpty(this.msgFilter) && !text.Contains(this.msgFilter, StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x06004C97 RID: 19607 RVA: 0x002A40BC File Offset: 0x002A22BC
		private void SetMessageReversal(bool reverse)
		{
			if (this.reverseOrder == reverse)
			{
				return;
			}
			this.reverseOrder = reverse;
			this.reverseButton.Children.ForEach(delegate(GUIComponent c)
			{
				c.SpriteEffects = (this.reverseOrder ? SpriteEffects.FlipVertically : SpriteEffects.None);
			});
			this.listBox.Content.RectTransform.ReverseChildren();
		}

		// Token: 0x06004C98 RID: 19608 RVA: 0x002A410C File Offset: 0x002A230C
		public bool ClearFilter(GUIComponent button, object _)
		{
			GUITextBox searchBox = button.UserData as GUITextBox;
			if (searchBox != null)
			{
				searchBox.Text = "";
			}
			this.msgFilter = "";
			this.FilterMessages();
			return true;
		}

		// Token: 0x1700137F RID: 4991
		// (get) Token: 0x06004C99 RID: 19609 RVA: 0x002A4146 File Offset: 0x002A2346
		// (set) Token: 0x06004C9A RID: 19610 RVA: 0x002A414E File Offset: 0x002A234E
		public int LinesPerFile
		{
			get
			{
				return this.linesPerFile;
			}
			set
			{
				this.linesPerFile = Math.Max(value, 10);
			}
		}

		// Token: 0x06004C9B RID: 19611 RVA: 0x002A4160 File Offset: 0x002A2360
		public ServerLog(string serverName)
		{
			this.ServerName = serverName;
			this.lines = new Queue<ServerLog.LogMessage>();
			this.unsavedLines = new Queue<ServerLog.LogMessage>();
			foreach (object obj in Enum.GetValues(typeof(ServerLog.MessageType)))
			{
				ServerLog.MessageType messageType = (ServerLog.MessageType)obj;
			}
		}

		// Token: 0x06004C9C RID: 19612 RVA: 0x002A43EC File Offset: 0x002A25EC
		public void WriteLine(string line, ServerLog.MessageType messageType, bool logToConsole = true)
		{
			ServerLog.LogMessage newText = new ServerLog.LogMessage(line, messageType);
			this.lines.Enqueue(newText);
			this.unsavedLines.Enqueue(newText);
			if (this.listBox != null)
			{
				this.AddLine(newText);
				this.listBox.UpdateScrollBarSize();
			}
			if (this.unsavedLines.Count >= this.LinesPerFile)
			{
				this.Save();
				this.unsavedLines.Clear();
			}
			while (this.lines.Count > this.LinesPerFile)
			{
				this.lines.Dequeue();
			}
			while (this.listBox != null && this.listBox.Content.CountChildren > this.LinesPerFile)
			{
				this.listBox.Content.RemoveChild((!this.reverseOrder) ? this.listBox.Content.Children.First<GUIComponent>() : this.listBox.Content.Children.Last<GUIComponent>());
			}
		}

		// Token: 0x06004C9D RID: 19613 RVA: 0x002A44E0 File Offset: 0x002A26E0
		public void Save()
		{
			if (!Directory.Exists("ServerLogs"))
			{
				try
				{
					Directory.CreateDirectory("ServerLogs", false);
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to create a folder for server logs", e, null, false, false);
					return;
				}
			}
			string fileName = this.ServerName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH:mm");
			fileName = ToolBox.RemoveInvalidFileNameChars(fileName);
			string filePath = Path.Combine(new string[]
			{
				"ServerLogs",
				fileName + ".txt"
			});
			int i = 2;
			while (File.Exists(filePath))
			{
				filePath = Path.Combine(new string[]
				{
					"ServerLogs",
					fileName + " (" + i.ToString() + ").txt"
				});
				i++;
			}
			try
			{
				File.WriteAllLines(filePath, from l in this.unsavedLines
				select l.Text.SanitizedValue, null, false);
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Saving the server log to " + filePath + " failed", e2, null, false, false);
			}
		}

		// Token: 0x040027FA RID: 10234
		private const int MaxLines = 500;

		// Token: 0x040027FB RID: 10235
		public GUIButton LogFrame;

		// Token: 0x040027FC RID: 10236
		private GUIListBox listBox;

		// Token: 0x040027FD RID: 10237
		private GUIButton reverseButton;

		// Token: 0x040027FE RID: 10238
		private string msgFilter;

		// Token: 0x040027FF RID: 10239
		private bool reverseOrder;

		// Token: 0x04002800 RID: 10240
		private readonly bool[] msgTypeHidden = new bool[Enum.GetValues(typeof(ServerLog.MessageType)).Length];

		// Token: 0x04002801 RID: 10241
		private readonly Dictionary<ServerLog.MessageType, Color> messageColor = new Dictionary<ServerLog.MessageType, Color>
		{
			{
				ServerLog.MessageType.Chat,
				Color.LightBlue
			},
			{
				ServerLog.MessageType.ItemInteraction,
				new Color(205, 205, 180)
			},
			{
				ServerLog.MessageType.Inventory,
				new Color(255, 234, 85)
			},
			{
				ServerLog.MessageType.Attack,
				new Color(204, 74, 78)
			},
			{
				ServerLog.MessageType.Spawning,
				new Color(163, 73, 164)
			},
			{
				ServerLog.MessageType.Wiring,
				new Color(255, 157, 85)
			},
			{
				ServerLog.MessageType.ServerMessage,
				new Color(157, 225, 160)
			},
			{
				ServerLog.MessageType.ConsoleUsage,
				new Color(0, 162, 232)
			},
			{
				ServerLog.MessageType.Money,
				Color.Green
			},
			{
				ServerLog.MessageType.DoSProtection,
				Color.OrangeRed
			},
			{
				ServerLog.MessageType.Karma,
				new Color(75, 88, 255)
			},
			{
				ServerLog.MessageType.Talent,
				new Color(125, 125, 255)
			},
			{
				ServerLog.MessageType.Traitors,
				new Color(107, 69, 158)
			},
			{
				ServerLog.MessageType.Error,
				Color.Red
			}
		};

		// Token: 0x04002802 RID: 10242
		private readonly Dictionary<ServerLog.MessageType, string> messageTypeName = new Dictionary<ServerLog.MessageType, string>
		{
			{
				ServerLog.MessageType.Chat,
				"ChatMessage"
			},
			{
				ServerLog.MessageType.ItemInteraction,
				"ItemInteraction"
			},
			{
				ServerLog.MessageType.Inventory,
				"InventoryUsage"
			},
			{
				ServerLog.MessageType.Attack,
				"AttackDeath"
			},
			{
				ServerLog.MessageType.Spawning,
				"Spawning"
			},
			{
				ServerLog.MessageType.Wiring,
				"Wiring"
			},
			{
				ServerLog.MessageType.ServerMessage,
				"ServerMessage"
			},
			{
				ServerLog.MessageType.ConsoleUsage,
				"ConsoleUsage"
			},
			{
				ServerLog.MessageType.Money,
				"Money"
			},
			{
				ServerLog.MessageType.DoSProtection,
				"DoSProtection"
			},
			{
				ServerLog.MessageType.Karma,
				"Karma"
			},
			{
				ServerLog.MessageType.Talent,
				"Talent"
			},
			{
				ServerLog.MessageType.Traitors,
				"Traitors"
			},
			{
				ServerLog.MessageType.Error,
				"Error"
			}
		};

		// Token: 0x04002803 RID: 10243
		private int linesPerFile = 800;

		// Token: 0x04002804 RID: 10244
		public const string SavePath = "ServerLogs";

		// Token: 0x04002805 RID: 10245
		private readonly Queue<ServerLog.LogMessage> lines;

		// Token: 0x04002806 RID: 10246
		private readonly Queue<ServerLog.LogMessage> unsavedLines;

		// Token: 0x04002807 RID: 10247
		public string ServerName;

		// Token: 0x0200121D RID: 4637
		private struct LogMessage
		{
			// Token: 0x06009329 RID: 37673 RVA: 0x003CB56C File Offset: 0x003C976C
			public LogMessage(string text, ServerLog.MessageType type)
			{
				if (type.HasFlag(ServerLog.MessageType.Chat))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted<DateTime>(DateTime.Now);
					defaultInterpolatedStringHandler.AppendLiteral("]\n  ");
					defaultInterpolatedStringHandler.AppendFormatted(text);
					text = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(5, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("[");
					defaultInterpolatedStringHandler2.AppendFormatted<DateTime>(DateTime.Now);
					defaultInterpolatedStringHandler2.AppendLiteral("]\n  ");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.GetServerMessage(text));
					text = defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				this.Text = RichString.Rich(text, null);
				this.Type = type;
			}

			// Token: 0x04005E34 RID: 24116
			public readonly RichString Text;

			// Token: 0x04005E35 RID: 24117
			public readonly ServerLog.MessageType Type;
		}

		// Token: 0x0200121E RID: 4638
		public enum MessageType
		{
			// Token: 0x04005E37 RID: 24119
			Chat,
			// Token: 0x04005E38 RID: 24120
			ItemInteraction,
			// Token: 0x04005E39 RID: 24121
			Inventory,
			// Token: 0x04005E3A RID: 24122
			Attack,
			// Token: 0x04005E3B RID: 24123
			Spawning,
			// Token: 0x04005E3C RID: 24124
			Wiring,
			// Token: 0x04005E3D RID: 24125
			ServerMessage,
			// Token: 0x04005E3E RID: 24126
			ConsoleUsage,
			// Token: 0x04005E3F RID: 24127
			Money,
			// Token: 0x04005E40 RID: 24128
			DoSProtection,
			// Token: 0x04005E41 RID: 24129
			Karma,
			// Token: 0x04005E42 RID: 24130
			Talent,
			// Token: 0x04005E43 RID: 24131
			Traitors,
			// Token: 0x04005E44 RID: 24132
			Error
		}
	}
}
