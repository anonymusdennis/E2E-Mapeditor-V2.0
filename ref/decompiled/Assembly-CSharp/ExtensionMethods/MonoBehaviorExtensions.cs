using System;
using System.Collections;
using UnityEngine;

namespace ExtensionMethods;

public static class MonoBehaviorExtensions
{
	public class Coroutine<T>
	{
		private T returnVal;

		private Exception e;

		public Coroutine coroutine;

		public T Value
		{
			get
			{
				if (e != null)
				{
					throw e;
				}
				return returnVal;
			}
		}

		public IEnumerator InternalRoutine(IEnumerator coroutine)
		{
			object yielded;
			while (true)
			{
				try
				{
					if (!coroutine.MoveNext())
					{
						yield break;
					}
				}
				catch (Exception ex)
				{
					e = ex;
					yield break;
				}
				yielded = coroutine.Current;
				if (yielded != null && yielded.GetType() == typeof(T))
				{
					break;
				}
				yield return coroutine.Current;
			}
			returnVal = (T)yielded;
		}
	}

	public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator coroutine)
	{
		Coroutine<T> coroutine2 = new Coroutine<T>();
		coroutine2.coroutine = obj.StartCoroutine(coroutine2.InternalRoutine(coroutine));
		return coroutine2;
	}
}
