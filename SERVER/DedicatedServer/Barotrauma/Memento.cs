using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000266 RID: 614
	public class Memento<T>
	{
		// Token: 0x17000D42 RID: 3394
		// (get) Token: 0x06002C35 RID: 11317 RVA: 0x00124CAD File Offset: 0x00122EAD
		// (set) Token: 0x06002C36 RID: 11318 RVA: 0x00124CB5 File Offset: 0x00122EB5
		public T Current { get; private set; }

		// Token: 0x17000D43 RID: 3395
		// (get) Token: 0x06002C37 RID: 11319 RVA: 0x00124CBE File Offset: 0x00122EBE
		public int UndoCount
		{
			get
			{
				return this.undoStack.Count;
			}
		}

		// Token: 0x17000D44 RID: 3396
		// (get) Token: 0x06002C38 RID: 11320 RVA: 0x00124CCB File Offset: 0x00122ECB
		public int RedoCount
		{
			get
			{
				return this.redoStack.Count;
			}
		}

		// Token: 0x06002C39 RID: 11321 RVA: 0x00124CD8 File Offset: 0x00122ED8
		public void Store(T newState)
		{
			this.redoStack.Clear();
			if (this.Current != null)
			{
				T t = this.Current;
				if (!t.Equals(default(T)))
				{
					this.undoStack.Push(this.Current);
				}
			}
			this.Current = newState;
		}

		// Token: 0x06002C3A RID: 11322 RVA: 0x00124D39 File Offset: 0x00122F39
		public T Undo()
		{
			if (this.undoStack.Any<T>())
			{
				this.redoStack.Push(this.Current);
				this.Current = this.undoStack.Pop();
			}
			return this.Current;
		}

		// Token: 0x06002C3B RID: 11323 RVA: 0x00124D70 File Offset: 0x00122F70
		public T Redo()
		{
			if (this.redoStack.Any<T>())
			{
				this.undoStack.Push(this.Current);
				this.Current = this.redoStack.Pop();
			}
			return this.Current;
		}

		// Token: 0x06002C3C RID: 11324 RVA: 0x00124DA8 File Offset: 0x00122FA8
		public void Clear()
		{
			this.undoStack.Clear();
			this.redoStack.Clear();
			this.Current = default(T);
		}

		// Token: 0x040015D0 RID: 5584
		private Stack<T> undoStack = new Stack<T>();

		// Token: 0x040015D1 RID: 5585
		private Stack<T> redoStack = new Stack<T>();
	}
}
