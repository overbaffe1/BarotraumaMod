using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000165 RID: 357
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class MissingContentPackageException : Exception
	{
		// Token: 0x17000855 RID: 2133
		// (get) Token: 0x06001D3C RID: 7484 RVA: 0x000D125F File Offset: 0x000CF45F
		public override string Message { get; }

		// Token: 0x06001D3D RID: 7485 RVA: 0x000D1268 File Offset: 0x000CF468
		[NullableContext(2)]
		public MissingContentPackageException(ContentPackage whoAsked, string missingPackage)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 2);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			defaultInterpolatedStringHandler.AppendFormatted(((whoAsked != null) ? whoAsked.Name : null) ?? "[NULL]");
			defaultInterpolatedStringHandler.AppendLiteral("\" depends on a package ");
			defaultInterpolatedStringHandler.AppendLiteral("with name or ID \"");
			defaultInterpolatedStringHandler.AppendFormatted(missingPackage ?? "[NULL]");
			defaultInterpolatedStringHandler.AppendLiteral("\" ");
			defaultInterpolatedStringHandler.AppendLiteral("that is not currently installed.");
			this.Message = defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}
