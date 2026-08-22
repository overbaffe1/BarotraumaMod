using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003C6 RID: 966
	public class ServerLog
	{
		// Token: 0x17000F5A RID: 3930
		// (get) Token: 0x060037FA RID: 14330 RVA: 0x0017701B File Offset: 0x0017521B
		// (set) Token: 0x060037FB RID: 14331 RVA: 0x00177023 File Offset: 0x00175223
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

		// Token: 0x060037FC RID: 14332 RVA: 0x00177034 File Offset: 0x00175234
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

		// Token: 0x060037FD RID: 14333 RVA: 0x001772A0 File Offset: 0x001754A0
		public void WriteLine(string line, ServerLog.MessageType messageType, bool logToConsole = true)
		{
			ServerLog.LogMessage newText = new ServerLog.LogMessage(line, messageType);
			if (logToConsole)
			{
				DebugConsole.NewMessage(newText.Text.SanitizedValue, new Color?(this.messageColor[messageType]), false);
			}
			this.lines.Enqueue(newText);
			this.unsavedLines.Enqueue(newText);
			if (this.unsavedLines.Count >= this.LinesPerFile)
			{
				this.Save();
				this.unsavedLines.Clear();
			}
			while (this.lines.Count > this.LinesPerFile)
			{
				this.lines.Dequeue();
			}
		}

		// Token: 0x060037FE RID: 14334 RVA: 0x00177338 File Offset: 0x00175538
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

		// Token: 0x04001C02 RID: 7170
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

		// Token: 0x04001C03 RID: 7171
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

		// Token: 0x04001C04 RID: 7172
		private int linesPerFile = 800;

		// Token: 0x04001C05 RID: 7173
		public const string SavePath = "ServerLogs";

		// Token: 0x04001C06 RID: 7174
		private readonly Queue<ServerLog.LogMessage> lines;

		// Token: 0x04001C07 RID: 7175
		private readonly Queue<ServerLog.LogMessage> unsavedLines;

		// Token: 0x04001C08 RID: 7176
		public string ServerName;

		// Token: 0x02000C4F RID: 3151
		private struct LogMessage
		{
			// Token: 0x060063BC RID: 25532 RVA: 0x002130DC File Offset: 0x002112DC
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

			// Token: 0x04003C09 RID: 15369
			public readonly RichString Text;

			// Token: 0x04003C0A RID: 15370
			public readonly ServerLog.MessageType Type;
		}

		// Token: 0x02000C50 RID: 3152
		public enum MessageType
		{
			// Token: 0x04003C0C RID: 15372
			Chat,
			// Token: 0x04003C0D RID: 15373
			ItemInteraction,
			// Token: 0x04003C0E RID: 15374
			Inventory,
			// Token: 0x04003C0F RID: 15375
			Attack,
			// Token: 0x04003C10 RID: 15376
			Spawning,
			// Token: 0x04003C11 RID: 15377
			Wiring,
			// Token: 0x04003C12 RID: 15378
			ServerMessage,
			// Token: 0x04003C13 RID: 15379
			ConsoleUsage,
			// Token: 0x04003C14 RID: 15380
			Money,
			// Token: 0x04003C15 RID: 15381
			DoSProtection,
			// Token: 0x04003C16 RID: 15382
			Karma,
			// Token: 0x04003C17 RID: 15383
			Talent,
			// Token: 0x04003C18 RID: 15384
			Traitors,
			// Token: 0x04003C19 RID: 15385
			Error
		}
	}
}
