using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000483 RID: 1155
	internal class EntityEventException : Exception
	{
		// Token: 0x06004DDC RID: 19932 RVA: 0x002ABC87 File Offset: 0x002A9E87
		public EntityEventException(string errorMessage, Entity causingEntity, Exception innerException = null) : base(errorMessage, innerException)
		{
			this.Entity = causingEntity;
		}

		// Token: 0x0400291E RID: 10526
		public readonly Entity Entity;
	}
}
