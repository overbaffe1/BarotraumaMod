using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Networking;
using Barotrauma.Steam;

namespace Barotrauma.Eos
{
	// Token: 0x0200062D RID: 1581
	[NullableContext(1)]
	[Nullable(0)]
	internal static class EosSessionManager
	{
		// Token: 0x060064F1 RID: 25841 RVA: 0x003432D0 File Offset: 0x003414D0
		public static void LeaveSession()
		{
			EosInterface.Sessions.OwnedSession ownedSession;
			if (!EosSessionManager.CurrentOwnedSession.TryUnwrap(out ownedSession))
			{
				return;
			}
			ownedSession.Dispose();
			Option.UnspecifiedNone none = Option.None;
			EosSessionManager.CurrentOwnedSession = none;
		}

		// Token: 0x060064F2 RID: 25842 RVA: 0x00343304 File Offset: 0x00341504
		public static void UpdateOwnedSession(Endpoint endpoint, ServerSettings serverSettings)
		{
			EosSessionManager.UpdateOwnedSession(Option.Some<Endpoint>(endpoint), serverSettings);
		}

		// Token: 0x060064F3 RID: 25843 RVA: 0x00343314 File Offset: 0x00341514
		public static void UpdateOwnedSession([Nullable(new byte[]
		{
			0,
			1
		})] Option<Endpoint> endpoint, ServerSettings serverSettings)
		{
			EosSessionManager.<>c__DisplayClass3_0 CS$<>8__locals1 = new EosSessionManager.<>c__DisplayClass3_0();
			CS$<>8__locals1.endpoint = endpoint;
			CS$<>8__locals1.serverSettings = serverSettings;
			if (!EosInterface.Core.IsInitialized)
			{
				return;
			}
			if (!CS$<>8__locals1.serverSettings.IsPublic)
			{
				EosSessionManager.LeaveSession();
				return;
			}
			ImmutableArray<EosInterface.ProductUserId> selfPuids = EosInterface.IdQueries.GetLoggedInPuids();
			if (!EosSessionManager.CurrentOwnedSession.TryUnwrap(out CS$<>8__locals1.ownedSession))
			{
				if (!TaskPool.IsTaskRunning("CreateOwnedSession"))
				{
					string name = "CreateOwnedSession";
					Option<EosInterface.ProductUserId> puidOption;
					if (!selfPuids.Any<EosInterface.ProductUserId>())
					{
						Option.UnspecifiedNone none = Option.None;
						puidOption = none;
					}
					else
					{
						puidOption = Option.Some<EosInterface.ProductUserId>(selfPuids.First<EosInterface.ProductUserId>());
					}
					TaskPool.Add(name, EosInterface.Sessions.CreateSession(puidOption, "OwnedSession".ToIdentifier(), CS$<>8__locals1.serverSettings.MaxPlayers), delegate(Task t)
					{
						EosSessionManager.LeaveSession();
						Result<EosInterface.Sessions.OwnedSession, EosInterface.Sessions.CreateError> result;
						if (!t.TryGetResult(out result))
						{
							return;
						}
						EosInterface.Sessions.OwnedSession newOwnedSession;
						if (result.TryUnwrapSuccess(out newOwnedSession))
						{
							EosSessionManager.CurrentOwnedSession = Option.Some<EosInterface.Sessions.OwnedSession>(newOwnedSession);
							EosSessionManager.UpdateOwnedSession(CS$<>8__locals1.endpoint, CS$<>8__locals1.serverSettings);
							return;
						}
						EosInterface.Sessions.CreateError error;
						if (result.TryUnwrapFailure(out error) && error == EosInterface.Sessions.CreateError.SessionAlreadyExists)
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to create Epic Online Services session: ");
						defaultInterpolatedStringHandler.AppendFormatted<Result<EosInterface.Sessions.OwnedSession, EosInterface.Sessions.CreateError>>(result);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					});
				}
				return;
			}
			if (selfPuids.Length > 0)
			{
				CS$<>8__locals1.endpoint = Option<Endpoint>.Some(new EosP2PEndpoint(selfPuids.First<EosInterface.ProductUserId>()));
			}
			CS$<>8__locals1.ownedSession.HostAddress = from e1 in CS$<>8__locals1.endpoint
			select e1.StringRepresentation;
			Endpoint e2;
			if (CS$<>8__locals1.endpoint.TryUnwrap(out e2))
			{
				LidgrenEndpoint lidgrenEndpoint = e2 as LidgrenEndpoint;
				if (lidgrenEndpoint != null)
				{
					int port = lidgrenEndpoint.Port;
					CS$<>8__locals1.<UpdateOwnedSession>g__SetAttributeValue|3("Port".ToIdentifier(), port.ToString());
					goto IL_166;
				}
			}
			if (CS$<>8__locals1.serverSettings.Port != 0)
			{
				CS$<>8__locals1.<UpdateOwnedSession>g__SetAttributeValue|3("Port".ToIdentifier(), CS$<>8__locals1.serverSettings.Port.ToString());
			}
			IL_166:
			SteamId steamId;
			if (SteamManager.GetSteamId().TryUnwrap(out steamId))
			{
				CS$<>8__locals1.<UpdateOwnedSession>g__SetAttributeValue|3("SteamP2PEndpoint".ToIdentifier(), steamId.StringRepresentation);
			}
			CS$<>8__locals1.serverSettings.UpdateServerListInfo(new Action<Identifier, object>(CS$<>8__locals1.<UpdateOwnedSession>g__SetAttributeValue|3));
			TaskPool.Add("UpdateOwnedSessionAttributes", CS$<>8__locals1.ownedSession.UpdateAttributes(), delegate(Task t)
			{
				Result<Unit, EosInterface.Sessions.AttributeUpdateError> result;
				if (!t.TryGetResult(out result))
				{
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EOS UpdateOwnedSessionAttributes result: ");
				defaultInterpolatedStringHandler.AppendFormatted<Result<Unit, EosInterface.Sessions.AttributeUpdateError>>(result);
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			});
		}

		// Token: 0x04003461 RID: 13409
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<EosInterface.Sessions.OwnedSession> CurrentOwnedSession;
	}
}
