using System;

namespace Barotrauma.Networking
{
	// Token: 0x02000387 RID: 903
	internal class EntityEventException : Exception
	{
		// Token: 0x06003620 RID: 13856 RVA: 0x001732BF File Offset: 0x001714BF
		public EntityEventException(string errorMessage, Entity causingEntity, Exception innerException = null) : base(errorMessage, innerException)
		{
			this.Entity = causingEntity;
		}

		// Token: 0x04001B33 RID: 6963
		public readonly Entity Entity;
	}
}
