using System;
using System.Threading;
using LightInject;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003F5 RID: 1013
	public class PerThreadLifetime : ILifetime
	{
		// Token: 0x06003A5F RID: 14943 RVA: 0x001877C8 File Offset: 0x001859C8
		public object GetInstance(Func<object> createInstance, Scope scope)
		{
			if (this._instance.Value == null)
			{
				object inst = createInstance();
				IDisposable disposable = inst as IDisposable;
				if (disposable != null)
				{
					if (scope == null)
					{
						throw new InvalidOperationException("Attempt disposable object without a valid scope.");
					}
					scope.TrackInstance(disposable);
				}
				this._instance.Value = inst;
			}
			return this._instance.Value;
		}

		// Token: 0x04001D43 RID: 7491
		private readonly ThreadLocal<object> _instance = new ThreadLocal<object>();
	}
}
