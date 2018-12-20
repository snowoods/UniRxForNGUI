using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniRx
{
	public static class UniRxEx_Inspection
	{
		public static IDisposable Dump<T>(this IObservable<T> source, string name)
		{
			return source.Subscribe(
				i => Debug.LogFormat("{0} --> {1}", name, i), 
				e => Debug.LogFormat("{0} failed --> {1}", name, e.Message),
				() => Debug.LogFormat("{0} completed", name));
		}

		public static IObservable<bool> Any<T>(this IObservable<T> source)
		{
			return Observable.Create<bool>(
				o =>
				{
					var hasValues = false;
					return source
						.Take(1)
						.Subscribe(
							_ => hasValues = true,
							e => o.OnError(e),
							() =>
							{
								o.OnNext(hasValues);
								o.OnCompleted();
							});
				});
		}

		// 참고 : predicate 추가 가능.
		// subject.Any(i => i > 2);
		// 같은 표현
		// subject.Where(i => i > 2).Any(); // 조건 만족하는것이 하나라도 있는가?
		public static IObservable<bool> Any<T>(this IObservable<T> source, Func<T, bool> predicate)
		{
			return source.Where(predicate).Any();
		}

		public static IObservable<bool> All<T>(this IObservable<T> source, Func<T, bool> predicate)
		{
			return Observable.Create<bool>(
				o =>
				{
					var hasValues = true;
					return source
						.SkipWhile(predicate)
						.Take(1)
						.Subscribe(
							_ => hasValues = false,
							e => o.OnError(e),
							() =>
							{
								o.OnNext(hasValues);
								o.OnCompleted();
							});
				});
		}

		// Contains
		// IComparable 이용하여 비교, Any와 비슷하나 predicate 대신 IComparable<T> 사용.
		public static IObservable<bool> Contains<T>(this IObservable<T> source, IComparable<T> value)
		{
			return Observable.Create<bool>(
				o =>
				{
					var hasValue = false;
					return source
						.SkipWhile(s => value.CompareTo(s) != 0)
						.Take(1)
						.Subscribe(
							_ => hasValue = true,
							e => o.OnError(e),
							() =>
							{
								o.OnNext(hasValue);
								o.OnCompleted();
							});
				});
		}

		// Handle an ArgumentOutOfRangeException gracefully.
		// ElementAt extension method allows us to "cherry pick" out a value at a given index. (Like the IEnumerable<T>)
		// 인덱스에 해당하는 값 획득
		public static IObservable<T> ElementAt<T>(this IObservable<T> source, int index)
		{
			return Observable.Create<T>(
				o =>
				{
					return source 
						.Skip(index)
						.Take(1)
						.Subscribe(
							v => o.OnNext(v),
							e => o.OnError(e),
							() => o.OnCompleted());
				});
		}

		// ElementAtOrDefault extension method will protect us in case the index is out of range, by pushing the Default(T) value.
		// 원하는 인덱스에 값이 없을 경우 기본 값 적용
		public static IObservable<T> ElementAtOrDefault<T>(this IObservable<T> source, int index, T defaultValue)
		{
			return Observable.Create<T>(
				o =>
				{
					var isOutOfRange = true;
					return source
						.Skip(index)
						.Take(1)
						.Subscribe(
							v =>
							{
								isOutOfRange = false;
								o.OnNext(v);
							},
							e => o.OnError(e),
							() =>
							{
								if (isOutOfRange) o.OnNext(defaultValue);
								o.OnCompleted();
							});
				});
		}

		// public static IObservable<T> SequenceEqual<T>(this IObservable<T> source, IObservable<T> other)
		// {
		// 	return Observable.Create<T>(
		// 		o =>
		// 		{
		// 			var isTheSame = true;
		// 			return source
		// 				.SkipWhile()
		// 		}
		// 	)
		// }
	}


    
}