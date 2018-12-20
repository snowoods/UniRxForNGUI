using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniRx
{
	public static class Comparer
	{
		// .Net 4.5 이상에서 비교 연산자(IComparer<T>) 생성
		/* 예)
			public static Comparer<Room> RoomComparer = 
			Comparer.Create<Room>((a, b) => a.RoomId.CompareTo(b.RoomId))
		*/		
		public static Comparer<T> Create<T>(Comparison<T> comparison)
		{
			if (comparison == null) throw new ArgumentNullException("comparison");
			return new ComparisonComparer<T>(comparison);
		}
		private sealed class ComparisonComparer<T> : Comparer<T>
		{
			private readonly Comparison<T> comparison;
			public ComparisonComparer(Comparison<T> comparison)
			{
				this.comparison = comparison;
			}
			public override int Compare(T x, T y)
			{
				return comparison(x, y);
			}
		}
	}

	public static class UniRxEx_Aggregation
	{
		public static IObservable<int> Count<T>(this IObservable<T> source)
		{
			return Observable.Create<int>(
				o =>
				{
					int count = 0;
					return source
						.Subscribe(
							_ => count++,
							e => o.OnError(e),
							() =>
							{
								o.OnNext(count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<long> LongCount<T>(this IObservable<T> source)
		{
			return Observable.Create<long>(
				o =>
				{
					long count = 0;
					return source
						.Subscribe(
							_ => count++,
							e => o.OnError(e),
							() =>
							{
								o.OnNext(count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<T> Min<T>(this IObservable<T> source)
		{
			return source.Aggregate(
				(min, current) => Comparer<T>
					.Default
					.Compare(min, current) > 0
						? current
						: min);
		}

		public static IObservable<T> Min<T>(this IObservable<T> source, T seed)
		{
			return source.Aggregate(seed,
				(min, current) => Comparer<T>
					.Default
					.Compare(min, current) > 0
						? current
						: min);
		}

		public static IObservable<T> Max<T>(this IObservable<T> source)
		{
			// "Comparer<T>.Default"
			//  : It simply returns an instance of the internal class GenericComparer<T> something like that.
			/*
			internal class GenericComparer<T> : Comparer<T> where T : IComparable<T>
			{
				public override int Compare(T x, T y)
				{
					if (x != null)
					{
						if (y != null)
							return x.CompareTo(y);
						return 1;
					}
					else
					{
						if (y != null)
							return -1;
						return 0;
					}
				}

				// ...
			}
			 */
			// This class has the constraint that T must implement IComparable<T>
			// so it can simply delegate the calls to it Compare method to the Compare methods of the instances it gets passed.			 
			var comparer = Comparer<T>.Default;

			Func<T, T, T> max = 
				(x, y) =>
				{
					if (comparer.Compare(x, y) < 0)
						return y;
					return x;
				};
			return source.Aggregate(max);
		}

		public static IObservable<T> Max<T>(this IObservable<T> source, T seed)
		{
			var comparer = Comparer<T>.Default;
			Func<T, T, T> max = 
				(x, y) =>
				{
					if (comparer.Compare(x, y) < 0)
						return y;
					return x;
				};
			return source.Aggregate(seed, max);
		}

		// Sum : int, long, float, double
		// C# v7.1 부터 지원 ---> T t = default(T);
		// 확장 메소드에서는 기본값 지정이 안된다.
		public static IObservable<int> Sum(this IObservable<int> source) { return source.Aggregate((acc, currentValue) => acc + currentValue); }
		public static IObservable<int> Sum(this IObservable<int> source, int seed) { return source.Aggregate(seed, (acc, currentValue) => acc + currentValue); }
		public static IObservable<long> Sum(this IObservable<long> source) { return source.Aggregate((acc, currentValue) => acc + currentValue); }
		public static IObservable<long> Sum(this IObservable<long> source, long seed) { return source.Aggregate(seed, (acc, currentValue) => acc + currentValue); }
		public static IObservable<float> Sum(this IObservable<float> source) { return source.Aggregate((acc, currentValue) => acc + currentValue); }
		public static IObservable<float> Sum(this IObservable<float> source, float seed) { return source.Aggregate(seed, (acc, currentValue) => acc + currentValue); }
		public static IObservable<double> Sum(this IObservable<double> source) { return source.Aggregate((acc, currentValue) => acc + currentValue); }
		public static IObservable<double> Sum(this IObservable<double> source, double seed) { return source.Aggregate(seed, (acc, currentValue) => acc + currentValue); }

		public static IObservable<int> Average(this IObservable<int> source)
		{
			return Observable.Create<int>(
				o =>
				{
					int count = 1;
					int total = 0;
					return source
						.Aggregate((acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<int> Average(this IObservable<int> source, int seed)
		{
			return Observable.Create<int>(
				o =>
				{
					int count = 1;
					int total = 0;
					return source
						.Aggregate(seed, (acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<long> Average(this IObservable<long> source)
		{
			return Observable.Create<long>(
				o =>
				{
					long count = 1;
					long total = 0;
					return source
						.Aggregate((acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<long> Average(this IObservable<long> source, long seed)
		{
			return Observable.Create<long>(
				o =>
				{
					long count = 1;
					long total = 0;
					return source
						.Aggregate(seed, (acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<float> Average(this IObservable<float> source)
		{
			return Observable.Create<float>(
				o =>
				{
					float count = 1;
					float total = 0;
					return source
						.Aggregate((acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<float> Average(this IObservable<float> source, float seed)
		{
			return Observable.Create<float>(
				o =>
				{
					float count = 1;
					float total = 0;
					return source
						.Aggregate(seed, (acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<double> Average(this IObservable<double> source)
		{
			return Observable.Create<double>(
				o =>
				{
					double count = 1;
					double total = 0;
					return source
						.Aggregate((acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}

		public static IObservable<double> Average(this IObservable<double> source, double seed)
		{
			return Observable.Create<double>(
				o =>
				{
					double count = 1;
					double total = 0;
					return source
						.Aggregate(seed, (acc, currentValue) => 
						{
							count++;
							return acc + currentValue;
						})
						.Take(1)
						.Subscribe(
							acc => total = acc,
							e => o.OnError(e),
							() =>
							{
								//Debug.LogFormat("total {0} count {1}", total, count);
								if (0 < count) o.OnNext(total / count);
								o.OnCompleted();
							});
				});
		}


		private static T MinOf<T>(T x, T y)
		{
			var comparer = Comparer<T>.Default;
			if (comparer.Compare(x, y) < 0)
				return x;
			return y;
		}
		public static IObservable<T> RunningMin<T>(this IObservable<T> source)
		{
			return source.Scan(MinOf).DistinctUntilChanged();
		}

		private static T MaxOf<T>(T x, T y)
		{
			var comparer = Comparer<T>.Default;
			if (comparer.Compare(x, y) < 0)
				return y;
			return x;
		}
		public static IObservable<T> RunningMax<T>(this IObservable<T> source)
		{
			return source.Scan(MaxOf).DistinctUntilChanged();
		}


		/// <summary>
		/// 가장 작은 키의 값 리스트를 획득
		/// </summary>
		/// <param name="source">원본 데이타</param>
		/// <param name="keySelector">원본 데이타에 적합한 키를 keySelector로 선택</param>
		/// <returns>가장 작은 키의 값 리스트</returns>
		public static IObservable<IList<TSource>> MinBy<TSource, TKey>(this IObservable<TSource> source, 
																			Func<TSource, TKey> keySelector)
		{
			return Observable.Create<IList<TSource>>(
				o =>
				{
					IList<TSource> valueList = new List<TSource>();
					IList<TKey> keyList = new List<TKey>();
					IDictionary<TKey, List<TSource>> dic = new Dictionary<TKey, List<TSource>>();
					return source
						.Subscribe(
							v => 
							{
								TKey k = keySelector(v);
								keyList.Add(k);
								if (dic.ContainsKey(k))
									dic[k].Add(v);
								else
								{
									dic.Add(k, new List<TSource>());
									dic[k].Add(v);
								}
							},
							e => o.OnError(e),
							() =>
							{
								// 가장 작은 키의 값 리스트를 전달
								keyList.ToObservable()
										.Min()
										.Subscribe(
											minValue => valueList = dic[minValue],
											ke => o.OnError(ke),
											() =>
											{
												o.OnNext(valueList);
												o.OnCompleted();
											}	
										);
							});
				}
			);
		}

		/// <summary>
		/// keyComparer 정렬 후 가장 적합한 키의 값 리스트를 획득
		/// </summary>
		/// <param name="source">원본 데이타</param>
		/// <param name="keySelector">원본 데이타에 적합한 키를 keySelector로 선택</param>
		/// <param name="keyComparer">키를 keyComparer로 소팅 후 획득(Take(1))</param>
		/// <returns>가장 적합한 키의 값 리스트</returns>
		public static IObservable<IList<TSource>> MinBy<TSource, TKey>(this IObservable<TSource> source, 
																			Func<TSource, TKey> keySelector,
																			IComparer<TKey> keyComparer)
		{
			return Observable.Create<IList<TSource>>(
				o =>
				{
					IList<TSource> valueList = new List<TSource>();
					IList<TKey> keyList = new List<TKey>();
					IDictionary<TKey, List<TSource>> dic = new Dictionary<TKey, List<TSource>>();
					return source
						.Subscribe(
							v => 
							{
								TKey k = keySelector(v);
								keyList.Add(k);
								if (dic.ContainsKey(k))
									dic[k].Add(v);
								else
								{
									dic.Add(k, new List<TSource>());
									dic[k].Add(v);
								}
							},
							e => o.OnError(e),
							() =>
							{
								// 정렬 후 가장 적합한 키의 값 리스트를 전달
								(keyList as List<TKey>).Sort(keyComparer);
								keyList.ToObservable()
										.Take(1)
										.Subscribe(
											minValue => valueList = dic[minValue],
											ke => o.OnError(ke),
											() =>
											{
												o.OnNext(valueList);
												o.OnCompleted();
											}	
										);
							});
				}
			);
		}

		/// <summary>
		/// 가장 큰 키의 값 리스트를 획득
		/// </summary>
		/// <param name="source">원본 데이타</param>
		/// <param name="keySelector">원본 데이타에 적합한 키를 keySelector로 선택</param>
		/// <returns>가장 큰 키의 값 리스트</returns>
		public static IObservable<IList<TSource>> MaxBy<TSource, TKey>(this IObservable<TSource> source, 
																			Func<TSource, TKey> keySelector)
		{
			return Observable.Create<IList<TSource>>(
				o =>
				{
					IList<TSource> valueList = new List<TSource>();
					IList<TKey> keyList = new List<TKey>();
					IDictionary<TKey, List<TSource>> dic = new Dictionary<TKey, List<TSource>>();
					return source
						.Subscribe(
							v => 
							{
								TKey k = keySelector(v);
								keyList.Add(k);
								if (dic.ContainsKey(k))
									dic[k].Add(v);
								else
								{
									dic.Add(k, new List<TSource>());
									dic[k].Add(v);
								}
							},
							e => o.OnError(e),
							() =>
							{
								// 가장 큰 키의 값 리스트를 전달
								keyList.ToObservable()
										.Max()
										.Subscribe(
											minValue => valueList = dic[minValue],
											ke => o.OnError(ke),
											() =>
											{
												o.OnNext(valueList);
												o.OnCompleted();
											}	
										);
							});
				}
			);
		}

		/// <summary>
		/// keyComparer 정렬 후 가장 적합한 키의 값 리스트를 획득
		/// </summary>
		/// <param name="source">원본 데이타</param>
		/// <param name="keySelector">원본 데이타에 적합한 키를 keySelector로 선택</param>
		/// <param name="keyComparer">키를 keyComparer로 소팅 후 획득(Take(1))</param>
		/// <returns>가장 적합한 키의 값 리스트</returns>
		public static IObservable<IList<TSource>> MaxBy<TSource, TKey>(this IObservable<TSource> source, 
																			Func<TSource, TKey> keySelector,
																			IComparer<TKey> keyComparer)
		{
			return source.MinBy(keySelector, keyComparer);
		}
	}
}