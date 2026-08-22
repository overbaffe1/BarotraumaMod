using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x020004C1 RID: 1217
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ServerContentPackage : INetSerializableStruct
	{
		// Token: 0x1700144D RID: 5197
		// (get) Token: 0x06004FB1 RID: 20401 RVA: 0x002AF604 File Offset: 0x002AD804
		// (set) Token: 0x06004FB2 RID: 20402 RVA: 0x002AF62F File Offset: 0x002AD82F
		public Md5Hash Hash
		{
			get
			{
				Md5Hash result;
				if ((result = this.cachedHash) == null)
				{
					result = (this.cachedHash = Md5Hash.BytesAsHash(this.HashBytes));
				}
				return result;
			}
			set
			{
				this.cachedHash = value;
				this.HashBytes = value.ByteRepresentation;
			}
		}

		// Token: 0x1700144E RID: 5198
		// (get) Token: 0x06004FB3 RID: 20403 RVA: 0x002AF644 File Offset: 0x002AD844
		public DateTime InstallTime
		{
			get
			{
				DateTime dateTime = this.cachedDateTime.GetValueOrDefault();
				if (this.cachedDateTime == null)
				{
					dateTime = DateTime.UtcNow + TimeSpan.FromSeconds(this.InstallTimeDiffInSeconds);
					this.cachedDateTime = new DateTime?(dateTime);
					return dateTime;
				}
				return dateTime;
			}
		}

		// Token: 0x1700144F RID: 5199
		// (get) Token: 0x06004FB4 RID: 20404 RVA: 0x002AF691 File Offset: 0x002AD891
		[Nullable(2)]
		public RegularPackage RegularPackage
		{
			[NullableContext(2)]
			get
			{
				return ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Name.Equals(this.Name) && p.Hash.Equals(this.Hash)) ?? ContentPackageManager.RegularPackages.FirstOrDefault((RegularPackage p) => p.Hash.Equals(this.Hash));
			}
		}

		// Token: 0x17001450 RID: 5200
		// (get) Token: 0x06004FB5 RID: 20405 RVA: 0x002AF6C3 File Offset: 0x002AD8C3
		[Nullable(2)]
		public CorePackage CorePackage
		{
			[NullableContext(2)]
			get
			{
				return ContentPackageManager.CorePackages.FirstOrDefault((CorePackage p) => p.Name.Equals(this.Name) && p.Hash.Equals(this.Hash)) ?? ContentPackageManager.CorePackages.FirstOrDefault((CorePackage p) => p.Hash.Equals(this.Hash));
			}
		}

		// Token: 0x17001451 RID: 5201
		// (get) Token: 0x06004FB6 RID: 20406 RVA: 0x002AF6F5 File Offset: 0x002AD8F5
		[Nullable(2)]
		public ContentPackage ContentPackage
		{
			[NullableContext(2)]
			get
			{
				return this.RegularPackage ?? this.CorePackage;
			}
		}

		// Token: 0x06004FB7 RID: 20407 RVA: 0x002AF707 File Offset: 0x002AD907
		public ServerContentPackage()
		{
		}

		// Token: 0x06004FB8 RID: 20408 RVA: 0x002AF730 File Offset: 0x002AD930
		public ServerContentPackage(ContentPackage contentPackage, SerializableDateTime referenceTime)
		{
			this.Name = contentPackage.Name;
			this.Hash = contentPackage.Hash;
			ContentPackageId ugcId;
			this.UgcId = (contentPackage.UgcId.TryUnwrap(out ugcId) ? ugcId.StringRepresentation : "");
			this.IsMandatory = !contentPackage.Files.All((ContentFile f) => f is SubmarineFile);
			this.IsVanilla = (contentPackage == ContentPackageManager.VanillaCorePackage);
			SerializableDateTime installTime;
			this.InstallTimeDiffInSeconds = (contentPackage.InstallTime.TryUnwrap(out installTime) ? ((uint)(installTime - referenceTime).TotalSeconds) : 0U);
		}

		// Token: 0x06004FB9 RID: 20409 RVA: 0x002AF80C File Offset: 0x002ADA0C
		public string GetPackageStr()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 2);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			defaultInterpolatedStringHandler.AppendFormatted(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral("\" (hash ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Hash.ShortRepresentation);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040029EE RID: 10734
		[NetworkSerialize(280)]
		public string Name = "";

		// Token: 0x040029EF RID: 10735
		[NetworkSerialize(283, ArrayMaxSize = 65535)]
		public byte[] HashBytes = Array.Empty<byte>();

		// Token: 0x040029F0 RID: 10736
		[NetworkSerialize(286)]
		public string UgcId = "";

		// Token: 0x040029F1 RID: 10737
		[NetworkSerialize(289)]
		public uint InstallTimeDiffInSeconds;

		// Token: 0x040029F2 RID: 10738
		[NetworkSerialize(292)]
		public bool IsMandatory;

		// Token: 0x040029F3 RID: 10739
		[NetworkSerialize(295)]
		public bool IsVanilla;

		// Token: 0x040029F4 RID: 10740
		[Nullable(2)]
		private Md5Hash cachedHash;

		// Token: 0x040029F5 RID: 10741
		private DateTime? cachedDateTime;
	}
}
