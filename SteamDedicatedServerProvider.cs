using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Steamworks.Data;
using Steamworks.ServerList;

namespace Barotrauma
{
	// Token: 0x020000F6 RID: 246
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SteamDedicatedServerProvider : ServerProvider
	{
		// Token: 0x06002338 RID: 9016 RVA: 0x0016391C File Offset: 0x00161B1C
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<Barotrauma.Networking.ServerInfo> InfoFromListEntry(Steamworks.Data.ServerInfo entry)
		{
			if (!entry.Name.IsNullOrEmpty() && entry.Address != null)
			{
				return Option<Barotrauma.Networking.ServerInfo>.Some(new Barotrauma.Networking.ServerInfo(new Endpoint[]
				{
					new LidgrenEndpoint(entry.Address, entry.ConnectionPort)
				})
				{
					ServerName = entry.Name,
					HasPassword = entry.Passworded,
					PlayerCount = entry.Players,
					MaxPlayers = entry.MaxPlayers,
					MetadataSource = Option<Barotrauma.Networking.ServerInfo.DataSource>.Some(new SteamDedicatedServerProvider.DataSource((ushort)entry.QueryPort))
				});
			}
			return Option<Barotrauma.Networking.ServerInfo>.None();
		}

		// Token: 0x06002339 RID: 9017 RVA: 0x001639B8 File Offset: 0x00161BB8
		private void HandleResponsiveServer(Steamworks.Data.ServerInfo entry, Action<Barotrauma.Networking.ServerInfo, ServerProvider> onServerDataReceived)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
			defaultInterpolatedStringHandler.AppendLiteral("QueryServerRules (GetServers, ");
			defaultInterpolatedStringHandler.AppendFormatted(entry.Name);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<IPAddress>(entry.Address);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), entry.QueryRulesAsync(), delegate(Task t)
			{
				if (t.Status == TaskStatus.Faulted)
				{
					TaskPool.PrintTaskExceptions(t, "Failed to retrieve rules for " + entry.Name, delegate(string msg)
					{
						DebugConsole.ThrowError(msg, null, null, false, false);
					});
					return;
				}
				Dictionary<string, string> rules;
				if (!t.TryGetResult(out rules))
				{
					return;
				}
				if (rules == null)
				{
					return;
				}
				Barotrauma.Networking.ServerInfo serverInfo;
				if (!SteamDedicatedServerProvider.InfoFromListEntry(entry).TryUnwrap(out serverInfo))
				{
					return;
				}
				serverInfo.UpdateInfo(delegate(string key)
				{
					string val;
					if (rules.TryGetValue(key, out val))
					{
						return val;
					}
					return null;
				});
				serverInfo.Checked = true;
				serverInfo.HasPassword |= entry.Passworded;
				onServerDataReceived(serverInfo, this);
			});
		}

		// Token: 0x0600233A RID: 9018 RVA: 0x00163A58 File Offset: 0x00161C58
		private void HandleUnresponsiveServer(Steamworks.Data.ServerInfo entry, Action<Barotrauma.Networking.ServerInfo, ServerProvider> onServerDataReceived)
		{
			Barotrauma.Networking.ServerInfo serverInfo;
			if (!SteamDedicatedServerProvider.InfoFromListEntry(entry).TryUnwrap(out serverInfo))
			{
				return;
			}
			onServerDataReceived(serverInfo, this);
		}

		// Token: 0x0600233B RID: 9019 RVA: 0x00163A80 File Offset: 0x00161C80
		protected override void RetrieveServersImpl(Action<Barotrauma.Networking.ServerInfo, ServerProvider> onServerDataReceived, Action onQueryCompleted)
		{
			SteamDedicatedServerProvider.<>c__DisplayClass6_0 CS$<>8__locals1 = new SteamDedicatedServerProvider.<>c__DisplayClass6_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.onServerDataReceived = onServerDataReceived;
			CS$<>8__locals1.onQueryCompleted = onQueryCompleted;
			if (!SteamManager.IsInitialized)
			{
				CS$<>8__locals1.onQueryCompleted();
				return;
			}
			CS$<>8__locals1.selfServerQuery = new Internet();
			this.serverQuery = CS$<>8__locals1.selfServerQuery;
			CS$<>8__locals1.responsiveServers = new ConcurrentQueue<Steamworks.Data.ServerInfo>();
			CS$<>8__locals1.unresponsiveServers = new ConcurrentQueue<Steamworks.Data.ServerInfo>();
			CS$<>8__locals1.selfServerQuery.OnResponsiveServer = new Action<Steamworks.Data.ServerInfo>(CS$<>8__locals1.responsiveServers.Enqueue);
			CS$<>8__locals1.selfServerQuery.OnUnresponsiveServer = new Action<Steamworks.Data.ServerInfo>(CS$<>8__locals1.unresponsiveServers.Enqueue);
			CS$<>8__locals1.selfQueryCoroutine = CoroutineManager.StartCoroutine(CS$<>8__locals1.<RetrieveServersImpl>g__dequeueCoroutine|1(), "SteamDedicatedServerProvider.RetrieveServers.dequeueCoroutine");
			this.queryCoroutine = CS$<>8__locals1.selfQueryCoroutine;
			TaskPool.Add("RunServerQuery", CS$<>8__locals1.selfServerQuery.RunQueryAsync(30f), delegate(Task t)
			{
				try
				{
					CS$<>8__locals1.selfServerQuery.OnResponsiveServer = null;
					CS$<>8__locals1.selfServerQuery.OnUnresponsiveServer = null;
					CoroutineManager.StopCoroutines(CS$<>8__locals1.selfQueryCoroutine);
					base.<RetrieveServersImpl>g__dequeue|0(null);
					if (t.Status == TaskStatus.Faulted)
					{
						TaskPool.PrintTaskExceptions(t, "Failed to retrieve servers", delegate(string msg)
						{
							DebugConsole.ThrowError(msg, null, null, false, false);
						});
					}
					CS$<>8__locals1.selfServerQuery.Dispose();
				}
				finally
				{
					CS$<>8__locals1.onQueryCompleted();
				}
			});
		}

		// Token: 0x0600233C RID: 9020 RVA: 0x00163B69 File Offset: 0x00161D69
		public override void Cancel()
		{
			if (this.queryCoroutine != null)
			{
				CoroutineManager.StopCoroutines(this.queryCoroutine);
			}
			Internet internet = this.serverQuery;
			if (internet != null)
			{
				internet.Dispose();
			}
			this.serverQuery = null;
		}

		// Token: 0x040011AE RID: 4526
		[Nullable(2)]
		private Internet serverQuery;

		// Token: 0x040011AF RID: 4527
		[Nullable(2)]
		private CoroutineHandle queryCoroutine;

		// Token: 0x02000BDB RID: 3035
		[Nullable(0)]
		public class DataSource : Barotrauma.Networking.ServerInfo.DataSource
		{
			// Token: 0x06007A2A RID: 31274 RVA: 0x00380D1E File Offset: 0x0037EF1E
			public DataSource(ushort queryPort)
			{
				this.QueryPort = queryPort;
			}

			// Token: 0x06007A2B RID: 31275 RVA: 0x00380D30 File Offset: 0x0037EF30
			[return: Nullable(new byte[]
			{
				0,
				1
			})]
			public new static Option<SteamDedicatedServerProvider.DataSource> Parse(XElement element)
			{
				int result;
				Option<SteamDedicatedServerProvider.DataSource> result2;
				if (element.TryGetAttributeInt("QueryPort", out result))
				{
					int invalidPort = result;
					Option<SteamDedicatedServerProvider.DataSource> option;
					if (invalidPort <= 0 || invalidPort > 65535)
					{
						option = Option<SteamDedicatedServerProvider.DataSource>.None();
					}
					else
					{
						int queryPort = invalidPort;
						option = Option<SteamDedicatedServerProvider.DataSource>.Some(new SteamDedicatedServerProvider.DataSource((ushort)queryPort));
					}
					result2 = option;
				}
				else
				{
					result2 = Option<SteamDedicatedServerProvider.DataSource>.None();
				}
				return result2;
			}

			// Token: 0x06007A2C RID: 31276 RVA: 0x00380D7F File Offset: 0x0037EF7F
			public override void Write(XElement element)
			{
				element.SetAttributeValue("QueryPort", this.QueryPort);
			}

			// Token: 0x04004923 RID: 18723
			public readonly ushort QueryPort;
		}
	}
}
