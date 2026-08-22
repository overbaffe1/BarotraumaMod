using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Steamworks;

namespace Barotrauma.Steam
{
	// Token: 0x0200042B RID: 1067
	[NullableContext(1)]
	[Nullable(0)]
	internal static class RemoteStorageHelper
	{
		// Token: 0x06004794 RID: 18324 RVA: 0x00272CE4 File Offset: 0x00270EE4
		[NullableContext(2)]
		public static void AskToEnable(Action onAccepted = null, Action onRejected = null)
		{
			GUIMessageBox confirmBox = new GUIMessageBox(TextManager.Get("RemoteStorageEnablePopup.Header"), TextManager.Get("RemoteStorageEnablePopup.Text"), new LocalizedString[]
			{
				TextManager.Get("Yes"),
				TextManager.Get("No")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, () => !SteamRemoteStorage.IsCloudEnabledForAccount || SteamRemoteStorage.IsCloudEnabledForApp, false);
			GUIButton guibutton = confirmBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
			{
				SteamRemoteStorage.IsCloudEnabledForApp = true;
				Action onAccepted2 = onAccepted;
				if (onAccepted2 != null)
				{
					onAccepted2();
				}
				return confirmBox.Close(btn, data);
			}));
			GUIButton guibutton2 = confirmBox.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object data)
			{
				Action onRejected2 = onRejected;
				if (onRejected2 != null)
				{
					onRejected2();
				}
				return confirmBox.Close(btn, data);
			}));
		}

		// Token: 0x06004795 RID: 18325 RVA: 0x00272DF4 File Offset: 0x00270FF4
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

		// Token: 0x06004796 RID: 18326 RVA: 0x00272E40 File Offset: 0x00271040
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

		// Token: 0x06004797 RID: 18327 RVA: 0x00272F2C File Offset: 0x0027112C
		public static bool TryDelete(string fileName, bool logError = true)
		{
			bool success = SteamRemoteStorage.FileDelete(fileName);
			if (logError && !success)
			{
				DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to delete file \"" + fileName + "\" from remote storage: operation failed.", null, null, false, false);
			}
			return success;
		}

		// Token: 0x06004798 RID: 18328 RVA: 0x00272F65 File Offset: 0x00271165
		public static bool IsStored(string fileName)
		{
			return SteamRemoteStorage.FileExists(fileName);
		}

		// Token: 0x04002531 RID: 9521
		public static readonly Color SteamColor = Color.DodgerBlue;

		// Token: 0x04002532 RID: 9522
		public static readonly string DebugPrefix = "‖color:" + RemoteStorageHelper.SteamColor.ToStringHex() + "‖[Remote Storage]‖end‖";
	}
}
