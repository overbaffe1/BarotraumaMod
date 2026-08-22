using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000388 RID: 904
	internal abstract class NetEntityEventManager
	{
		// Token: 0x06003621 RID: 13857 RVA: 0x001732D0 File Offset: 0x001714D0
		protected void Write(IWriteMessage msg, List<NetEntityEvent> eventsToSync, out List<NetEntityEvent> sentEvents, Client recipient = null)
		{
			IWriteMessage tempBuffer = new WriteOnlyMessage();
			sentEvents = new List<NetEntityEvent>();
			int eventCount = 0;
			foreach (NetEntityEvent e in eventsToSync)
			{
				IWriteMessage tempEventBuffer = new WriteOnlyMessage();
				try
				{
					this.WriteEvent(tempEventBuffer, e, recipient);
				}
				catch (Exception exception)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to write an event (ID: ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(e.ID);
					defaultInterpolatedStringHandler.AppendLiteral(") for the entity \"");
					defaultInterpolatedStringHandler.AppendFormatted<Entity>(e.Entity);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					string error = defaultInterpolatedStringHandler.ToStringAndClear();
					Exception e2 = exception;
					Entity entity = e.Entity;
					DebugConsole.ThrowError(error, e2, (entity != null) ? entity.ContentPackage : null, false, false);
					string identifier = "NetEntityEventManager.Write:WriteFailed" + e.Entity.ToString();
					GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
					string str = "Failed to write an event for the entity \"";
					Entity entity2 = e.Entity;
					GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, str + ((entity2 != null) ? entity2.ToString() : null) + "\"\n" + exception.StackTrace.CleanupStackTrace());
					tempBuffer.WriteUInt16(0);
					tempBuffer.WriteVariableUInt32(0U);
					eventCount++;
					continue;
				}
				if (eventCount > 0 && msg.LengthBytes + tempBuffer.LengthBytes + tempEventBuffer.LengthBytes > 1024)
				{
					break;
				}
				tempBuffer.WriteUInt16(e.EntityID);
				tempBuffer.WriteVariableUInt32((uint)tempEventBuffer.LengthBytes);
				tempBuffer.WriteBytes(tempEventBuffer.Buffer, 0, tempEventBuffer.LengthBytes);
				sentEvents.Add(e);
				eventCount++;
			}
			if (eventCount > 0)
			{
				msg.WritePadBits();
				msg.WriteUInt16(eventsToSync[0].ID);
				msg.WriteByte((byte)eventCount);
				msg.WriteBytes(tempBuffer.Buffer, 0, tempBuffer.LengthBytes);
			}
		}

		// Token: 0x06003622 RID: 13858 RVA: 0x001734C4 File Offset: 0x001716C4
		protected static bool ValidateEntity(INetSerializable entity)
		{
			NetEntityEventManager.<>c__DisplayClass2_0 CS$<>8__locals1;
			CS$<>8__locals1.entity = entity;
			Entity entity2 = CS$<>8__locals1.entity as Entity;
			if (entity2 == null)
			{
				NetEntityEventManager.<ValidateEntity>g__error|2_0("input is not of type Entity", ref CS$<>8__locals1);
				return false;
			}
			bool removed = entity2.Removed;
			bool idFreed = entity2.IdFreed;
			if (removed)
			{
				NetEntityEventManager.<ValidateEntity>g__error|2_0("the entity has been removed", ref CS$<>8__locals1);
				return false;
			}
			if (idFreed)
			{
				NetEntityEventManager.<ValidateEntity>g__error|2_0("the ID of the entity has been freed", ref CS$<>8__locals1);
				return false;
			}
			return true;
		}

		// Token: 0x06003623 RID: 13859
		protected abstract void WriteEvent(IWriteMessage buffer, NetEntityEvent entityEvent, Client recipient = null);

		// Token: 0x06003625 RID: 13861 RVA: 0x00173530 File Offset: 0x00171730
		[CompilerGenerated]
		internal static void <ValidateEntity>g__error|2_0(string reason, ref NetEntityEventManager.<>c__DisplayClass2_0 A_1)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Can't create an entity event for ");
			defaultInterpolatedStringHandler.AppendFormatted<INetSerializable>(A_1.entity);
			defaultInterpolatedStringHandler.AppendLiteral(" - ");
			defaultInterpolatedStringHandler.AppendFormatted(reason);
			defaultInterpolatedStringHandler.AppendLiteral(".\n");
			defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
		}

		// Token: 0x04001B34 RID: 6964
		public const int MaxEventBufferLength = 1024;
	}
}
