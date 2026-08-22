using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200025A RID: 602
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class MissingContentPackageException : Exception
	{
		// Token: 0x17000EB3 RID: 3763
		// (get) Token: 0x0600381F RID: 14367 RVA: 0x00216E77 File Offset: 0x00215077
		public override string Message { get; }

		// Token: 0x06003820 RID: 14368 RVA: 0x00216E80 File Offset: 0x00215080
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
