using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Barotrauma.Networking;

namespace Barotrauma.Eos
{
	// Token: 0x0200062B RID: 1579
	internal static class EosEpicSecondaryLogin
	{
		// Token: 0x060064DB RID: 25819 RVA: 0x003429AC File Offset: 0x00340BAC
		[NullableContext(1)]
		public static Task<EosEpicSecondaryLogin.ProbeResult> ProbeLinkedEpicAccount()
		{
			EosEpicSecondaryLogin.<ProbeLinkedEpicAccount>d__1 <ProbeLinkedEpicAccount>d__;
			<ProbeLinkedEpicAccount>d__.<>t__builder = AsyncTaskMethodBuilder<EosEpicSecondaryLogin.ProbeResult>.Create();
			<ProbeLinkedEpicAccount>d__.<>1__state = -1;
			<ProbeLinkedEpicAccount>d__.<>t__builder.Start<EosEpicSecondaryLogin.<ProbeLinkedEpicAccount>d__1>(ref <ProbeLinkedEpicAccount>d__);
			return <ProbeLinkedEpicAccount>d__.<>t__builder.Task;
		}

		// Token: 0x060064DC RID: 25820 RVA: 0x003429E8 File Offset: 0x00340BE8
		[NullableContext(1)]
		public static Task<Result<Unit, EosEpicSecondaryLogin.LoginError>> LoginToLinkedEpicAccount()
		{
			EosEpicSecondaryLogin.<LoginToLinkedEpicAccount>d__4 <LoginToLinkedEpicAccount>d__;
			<LoginToLinkedEpicAccount>d__.<>t__builder = AsyncTaskMethodBuilder<Result<Unit, EosEpicSecondaryLogin.LoginError>>.Create();
			<LoginToLinkedEpicAccount>d__.<>1__state = -1;
			<LoginToLinkedEpicAccount>d__.<>t__builder.Start<EosEpicSecondaryLogin.<LoginToLinkedEpicAccount>d__4>(ref <LoginToLinkedEpicAccount>d__);
			return <LoginToLinkedEpicAccount>d__.<>t__builder.Task;
		}

		// Token: 0x020014B4 RID: 5300
		public enum ProbeResult
		{
			// Token: 0x040066B3 RID: 26291
			NoAccount,
			// Token: 0x040066B4 RID: 26292
			LinkedExternalAccountsButNoPuid,
			// Token: 0x040066B5 RID: 26293
			LoggedIn
		}

		// Token: 0x020014B5 RID: 5301
		public enum LoginErrorDesc
		{
			// Token: 0x040066B7 RID: 26295
			NoPrimaryPuid,
			// Token: 0x040066B8 RID: 26296
			FailedToLogInViaLinkedEpicAccount,
			// Token: 0x040066B9 RID: 26297
			FailedToForceSteamAsPrimaryExternalAccountId,
			// Token: 0x040066BA RID: 26298
			SteamIsNoLongerLinkedToPuid,
			// Token: 0x040066BB RID: 26299
			SteamPuidMismatchedPreviousPrimaryPuid,
			// Token: 0x040066BC RID: 26300
			FailedToLinkSteamAccountToEpicAccount,
			// Token: 0x040066BD RID: 26301
			FailedToCreatePuidForEpicAccount,
			// Token: 0x040066BE RID: 26302
			UnhandledErrorCondition
		}

		// Token: 0x020014B6 RID: 5302
		public readonly struct LoginError : IEquatable<EosEpicSecondaryLogin.LoginError>
		{
			// Token: 0x06009BCF RID: 39887 RVA: 0x003E6D32 File Offset: 0x003E4F32
			public LoginError(EosEpicSecondaryLogin.LoginErrorDesc ErrorDesc, Option<EosInterface.Login.LoginError> LoginEosConnectError = default(Option<EosInterface.Login.LoginError>), Option<EosInterface.Login.LinkExternalAccountToEpicAccountError> LinkExternalToEpicError = default(Option<EosInterface.Login.LinkExternalAccountToEpicAccountError>), Option<EosInterface.Login.CreateProductAccountError> CreatePuidError = default(Option<EosInterface.Login.CreateProductAccountError>))
			{
				this.ErrorDesc = ErrorDesc;
				this.LoginEosConnectError = LoginEosConnectError;
				this.LinkExternalToEpicError = LinkExternalToEpicError;
				this.CreatePuidError = CreatePuidError;
			}

			// Token: 0x17001D75 RID: 7541
			// (get) Token: 0x06009BD0 RID: 39888 RVA: 0x003E6D51 File Offset: 0x003E4F51
			// (set) Token: 0x06009BD1 RID: 39889 RVA: 0x003E6D59 File Offset: 0x003E4F59
			public EosEpicSecondaryLogin.LoginErrorDesc ErrorDesc { get; set; }

			// Token: 0x17001D76 RID: 7542
			// (get) Token: 0x06009BD2 RID: 39890 RVA: 0x003E6D62 File Offset: 0x003E4F62
			// (set) Token: 0x06009BD3 RID: 39891 RVA: 0x003E6D6A File Offset: 0x003E4F6A
			public Option<EosInterface.Login.LoginError> LoginEosConnectError { get; set; }

			// Token: 0x17001D77 RID: 7543
			// (get) Token: 0x06009BD4 RID: 39892 RVA: 0x003E6D73 File Offset: 0x003E4F73
			// (set) Token: 0x06009BD5 RID: 39893 RVA: 0x003E6D7B File Offset: 0x003E4F7B
			public Option<EosInterface.Login.LinkExternalAccountToEpicAccountError> LinkExternalToEpicError { get; set; }

			// Token: 0x17001D78 RID: 7544
			// (get) Token: 0x06009BD6 RID: 39894 RVA: 0x003E6D84 File Offset: 0x003E4F84
			// (set) Token: 0x06009BD7 RID: 39895 RVA: 0x003E6D8C File Offset: 0x003E4F8C
			public Option<EosInterface.Login.CreateProductAccountError> CreatePuidError { get; set; }

			// Token: 0x06009BD8 RID: 39896 RVA: 0x003E6D98 File Offset: 0x003E4F98
			[NullableContext(1)]
			public override string ToString()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler.AppendLiteral("LoginError (");
				defaultInterpolatedStringHandler.AppendFormatted<EosEpicSecondaryLogin.LoginErrorDesc>(this.ErrorDesc);
				string error = defaultInterpolatedStringHandler.ToStringAndClear();
				EosInterface.Login.LoginError connectError;
				if (this.LoginEosConnectError.TryUnwrap(out connectError))
				{
					string str = error;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler2.AppendLiteral(", ");
					defaultInterpolatedStringHandler2.AppendFormatted<EosInterface.Login.LoginError>(connectError);
					error = str + defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				EosInterface.Login.LinkExternalAccountToEpicAccountError externalToEpicError;
				if (this.LinkExternalToEpicError.TryUnwrap(out externalToEpicError))
				{
					string str2 = error;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler3.AppendLiteral(", ");
					defaultInterpolatedStringHandler3.AppendFormatted<EosInterface.Login.LinkExternalAccountToEpicAccountError>(externalToEpicError);
					error = str2 + defaultInterpolatedStringHandler3.ToStringAndClear();
				}
				EosInterface.Login.CreateProductAccountError createPuidError;
				if (this.CreatePuidError.TryUnwrap(out createPuidError))
				{
					string str3 = error;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler4.AppendLiteral(", ");
					defaultInterpolatedStringHandler4.AppendFormatted<EosInterface.Login.CreateProductAccountError>(createPuidError);
					error = str3 + defaultInterpolatedStringHandler4.ToStringAndClear();
				}
				return error + ")";
			}

			// Token: 0x06009BD9 RID: 39897 RVA: 0x003E6E98 File Offset: 0x003E5098
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("ErrorDesc = ");
				builder.Append(this.ErrorDesc.ToString());
				builder.Append(", LoginEosConnectError = ");
				builder.Append(this.LoginEosConnectError.ToString());
				builder.Append(", LinkExternalToEpicError = ");
				builder.Append(this.LinkExternalToEpicError.ToString());
				builder.Append(", CreatePuidError = ");
				builder.Append(this.CreatePuidError.ToString());
				return true;
			}

			// Token: 0x06009BDA RID: 39898 RVA: 0x003E6F42 File Offset: 0x003E5142
			[CompilerGenerated]
			public static bool operator !=(EosEpicSecondaryLogin.LoginError left, EosEpicSecondaryLogin.LoginError right)
			{
				return !(left == right);
			}

			// Token: 0x06009BDB RID: 39899 RVA: 0x003E6F4E File Offset: 0x003E514E
			[CompilerGenerated]
			public static bool operator ==(EosEpicSecondaryLogin.LoginError left, EosEpicSecondaryLogin.LoginError right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009BDC RID: 39900 RVA: 0x003E6F58 File Offset: 0x003E5158
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((EqualityComparer<EosEpicSecondaryLogin.LoginErrorDesc>.Default.GetHashCode(this.<ErrorDesc>k__BackingField) * -1521134295 + EqualityComparer<Option<EosInterface.Login.LoginError>>.Default.GetHashCode(this.<LoginEosConnectError>k__BackingField)) * -1521134295 + EqualityComparer<Option<EosInterface.Login.LinkExternalAccountToEpicAccountError>>.Default.GetHashCode(this.<LinkExternalToEpicError>k__BackingField)) * -1521134295 + EqualityComparer<Option<EosInterface.Login.CreateProductAccountError>>.Default.GetHashCode(this.<CreatePuidError>k__BackingField);
			}

			// Token: 0x06009BDD RID: 39901 RVA: 0x003E6FBA File Offset: 0x003E51BA
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EosEpicSecondaryLogin.LoginError && this.Equals((EosEpicSecondaryLogin.LoginError)obj);
			}

			// Token: 0x06009BDE RID: 39902 RVA: 0x003E6FD4 File Offset: 0x003E51D4
			[CompilerGenerated]
			public bool Equals(EosEpicSecondaryLogin.LoginError other)
			{
				return EqualityComparer<EosEpicSecondaryLogin.LoginErrorDesc>.Default.Equals(this.<ErrorDesc>k__BackingField, other.<ErrorDesc>k__BackingField) && EqualityComparer<Option<EosInterface.Login.LoginError>>.Default.Equals(this.<LoginEosConnectError>k__BackingField, other.<LoginEosConnectError>k__BackingField) && EqualityComparer<Option<EosInterface.Login.LinkExternalAccountToEpicAccountError>>.Default.Equals(this.<LinkExternalToEpicError>k__BackingField, other.<LinkExternalToEpicError>k__BackingField) && EqualityComparer<Option<EosInterface.Login.CreateProductAccountError>>.Default.Equals(this.<CreatePuidError>k__BackingField, other.<CreatePuidError>k__BackingField);
			}

			// Token: 0x06009BDF RID: 39903 RVA: 0x003E7041 File Offset: 0x003E5241
			[CompilerGenerated]
			public void Deconstruct(out EosEpicSecondaryLogin.LoginErrorDesc ErrorDesc, out Option<EosInterface.Login.LoginError> LoginEosConnectError, out Option<EosInterface.Login.LinkExternalAccountToEpicAccountError> LinkExternalToEpicError, out Option<EosInterface.Login.CreateProductAccountError> CreatePuidError)
			{
				ErrorDesc = this.ErrorDesc;
				LoginEosConnectError = this.LoginEosConnectError;
				LinkExternalToEpicError = this.LinkExternalToEpicError;
				CreatePuidError = this.CreatePuidError;
			}
		}

		// Token: 0x020014B7 RID: 5303
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040066C3 RID: 26307
			public static Func<EpicAccountId, Task<Result<Unit, EosInterface.Login.LogoutEpicAccountError>>> <0>__LogoutEpicAccount;
		}
	}
}
