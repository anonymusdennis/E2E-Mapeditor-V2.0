using System;
using System.Threading;

public class PooledObject<T> : IDisposable where T : PooledObject<T>, new()
{
	private const int disposedFalse = 1;

	private const int disposedTrue = 0;

	private int disposed;

	public static T GetInstance()
	{
		T val = StaticObjectPool.PopOrNew<T>();
		val.disposed = 1;
		return val;
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref disposed, 0) != 0)
		{
			StaticObjectPool.Push((T)this);
		}
	}

	~PooledObject()
	{
		Dispose();
	}
}
