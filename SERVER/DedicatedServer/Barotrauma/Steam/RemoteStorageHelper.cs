using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Steamworks;

namespace Barotrauma.Steam
{
	// Token: 0x02000364 RID: 868
	[NullableContext(1)]
	[Nullable(0)]
	internal static class RemoteStorageHelper
	{
		// Token: 0x0600334A RID: 13130 RVA: 0x0015A470 File Offset: 0x00158670
		[NullableContext(2)]
		public static bool TryRead(this SteamRemoteStorage.RemoteFile remoteFile, [NotNullWhen(true)] out byte[] bytes, bool logError = true)
		{
			bytes = SteamRemoteStorage.FileRead(remoteFile.Filename);
			bool success = bytes != null;
			if (logError && !success)
			{
				DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to read file \"" + remoteFile.Filename + "\" from remote storage: operation failed.", null, null, false, false);
			}
			return success;
		}

		// Token: 0x0600334B RID: 13131 RVA: 0x0015A4BC File Offset: 0x001586BC
		public static bool TryWrite(string localPath, [Nullable(2)] string saveAs = null, bool allowOverwrite = false, bool logError = true)
		{
			string fileName = saveAs ?? Path.GetFileName(localPath);
			if (!allowOverwrite && SteamRemoteStorage.FileExists(fileName))
			{
				if (logError)
				{
					DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to write file \"" + fileName + "\" to remote storage: file already exists.", null, null, false, false);
				}
				return false;
			}
			byte[] data;
			try
			{
				data = File.ReadAllBytes(localPath, true);
			}
			catch (Exception exception)
			{
				if (logError)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 3);
					defaultInterpolatedStringHandler.AppendFormatted(RemoteStorageHelper.DebugPrefix);
					defaultInterpolatedStringHandler.AppendLiteral(" Failed to read file \"");
					defaultInterpolatedStringHandler.AppendFormatted(fileName);
					defaultInterpolatedStringHandler.AppendLiteral("\" while writing to remote storage: ");
					defaultInterpolatedStringHandler.AppendFormatted<Exception>(exception);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return false;
			}
			bool success = SteamRemoteStorage.FileWrite(fileName, data);
			if (logError && !success)
			{
				DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to write file \"" + fileName + "\" to remote storage: operation failed.", null, null, false, false);
			}
			return success;
		}

		// Token: 0x0600334C RID: 13132 RVA: 0x0015A5A8 File Offset: 0x001587A8
		public static bool TryDelete(string fileName, bool logError = true)
		{
			bool success = SteamRemoteStorage.FileDelete(fileName);
			if (logError && !success)
			{
				DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to delete file \"" + fileName + "\" from remote storage: operation failed.", null, null, false, false);
			}
			return success;
		}

		// Token: 0x0600334D RID: 13133 RVA: 0x0015A5E1 File Offset: 0x001587E1
		public static bool IsStored(string fileName)
		{
			return SteamRemoteStorage.FileExists(fileName);
		}

		// Token: 0x0400196B RID: 6507
		public static readonly Color SteamColor = Color.DodgerBlue;

		// Token: 0x0400196C RID: 6508
		public static readonly string DebugPrefix = "‖color:" + RemoteStorageHelper.SteamColor.ToStringHex() + "‖[Remote Storage]‖end‖";
	}
}
