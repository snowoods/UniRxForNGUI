using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniRx
{
	public static class UniRxEx_Transformation
	{
		public static IObservable<T> MyWhere<T>(this IObservable<T> source, Func<T, bool> predicate)
		{
			return source.SelectMany(
				item =>
				{
					if (predicate(item))
					{
						return Observable.Return(item);
					}
					else
					{
						return Observable.Empty<T>();
					}
				});
		}

		public static IObservable<TResult> MySelect<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
		{
			return source.SelectMany(value => Observable.Return(selector(value)));
		}
	}    
}