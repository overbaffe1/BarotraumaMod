using System;
using System.Threading;
using LightInject;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000509 RID: 1289
	public class PerThreadLifetime : ILifetime
	{
		// Token: 0x06005383 RID: 21379 RVA: 0x002CBF08 File Offset: 0x002CA108
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

		// Token: 0x04002C27 RID: 11303
		private readonly ThreadLocal<object> _instance = new ThreadLocal<object>();
	}
}
