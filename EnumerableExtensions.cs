using System;
using System.Collections.Generic;
using System.Linq;

using Whirllaxy.Utility;

namespace VodByte.Utility
{
    public static class EnumerableExtensions
    {
        ///================================================================================
        /// ███╗   ██╗██╗   ██╗██╗     ██╗          ██████╗██╗  ██╗███████╗ ██████╗██╗  ██╗
        /// ████╗  ██║██║   ██║██║     ██║         ██╔════╝██║  ██║██╔════╝██╔════╝██║ ██╔╝
        /// ██╔██╗ ██║██║   ██║██║     ██║         ██║     ███████║█████╗  ██║     █████╔╝ 
        /// ██║╚██╗██║██║   ██║██║     ██║         ██║     ██╔══██║██╔══╝  ██║     ██╔═██╗ 
        /// ██║ ╚████║╚██████╔╝███████╗███████╗    ╚██████╗██║  ██║███████╗╚██████╗██║  ██╗
        /// ╚═╝  ╚═══╝ ╚═════╝ ╚══════╝╚══════╝     ╚═════╝╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝  ╚═╝
        ///================================================================================
        ///<summary> 如果是空，返回一个空的集合 </summary>
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> _source)
        {
            return _source ?? Enumerable.Empty<T>();
        }

        ///===============================================================================
        /// ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗ █████╗ ████████╗███████╗███████╗
        /// ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝██╔══██╗╚══██╔══╝██╔════╝██╔════╝
        /// ██║  ██║██║   ██║██████╔╝██║     ██║██║     ███████║   ██║   █████╗  ███████╗
        /// ██║  ██║██║   ██║██╔═══╝ ██║     ██║██║     ██╔══██║   ██║   ██╔══╝  ╚════██║
        /// ██████╔╝╚██████╔╝██║     ███████╗██║╚██████╗██║  ██║   ██║   ███████╗███████║
        /// ╚═════╝  ╚═════╝ ╚═╝     ╚══════╝╚═╝ ╚═════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚══════╝
        ///===============================================================================
        ///<summary> 根据对象内部的属性进行去重 </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> _source, Func<TSource, TKey> _keySelector)
        {
            HashSet<TKey> seenKeys = new();
            return _source.Where(n => seenKeys.Add(_keySelector(n)));
        }

        ///---------------------------------------------------------------
        ///<summary> 找出其中重复的对象 </summary>
        public static IEnumerable<TSource> FindDuplicates<TSource, TKey>(this IEnumerable<TSource> _source, Func<TSource, TKey> _keySelector)
        {
            HashSet<TKey> seenKeys = new();
            return _source.Where(n => !seenKeys.Add(_keySelector(n)));
        }

        ///---------------------------------------------------------------
        public static IEnumerable<T> FindDuplicates<T>(this IEnumerable<T> _source)
        {
            HashSet<T> seenKeys = new();
            return _source.Where(n => !seenKeys.Add(n));
        }

        ///=======================================================
        /// ██████╗  █████╗ ███╗   ██╗██████╗  ██████╗ ███╗   ███╗
        /// ██╔══██╗██╔══██╗████╗  ██║██╔══██╗██╔═══██╗████╗ ████║
        /// ██████╔╝███████║██╔██╗ ██║██║  ██║██║   ██║██╔████╔██║
        /// ██╔══██╗██╔══██║██║╚██╗██║██║  ██║██║   ██║██║╚██╔╝██║
        /// ██║  ██║██║  ██║██║ ╚████║██████╔╝╚██████╔╝██║ ╚═╝ ██║
        /// ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝  ╚═════╝ ╚═╝     ╚═╝
        ///=======================================================
        ///<summary> 随机返回一个元素。如果来源为空或者元素数为 0，返回 default </summary>
        public static T PickRandom<T>(this IEnumerable<T> _source)
        {
            if (_source is null || _source.Count() is 0) return default;

            return _source.ElementAt(UnityEngine.Random.Range(0, _source.Count()));
        }

        ///---------------------------------------------------------------
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> _source, int _count)
        {
            if (_count <= 0) return Enumerable.Empty<T>();
            return _source.Shuffle().Take(_count);
        }

        ///---------------------------------------------------------------
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> _source)
        {
            return _source.OrderBy(x => Guid.NewGuid());
        }

        ///---------------------------------------------------------------
        public static void ShuffleInPlace<T>(this List<T> _source)
        {
            _source.Sort((n, m) => Guid.NewGuid().CompareTo(Guid.NewGuid()));
        }

        ///======================================================
        /// ██████╗ ███████╗███╗   ███╗ ██████╗ ██╗   ██╗███████╗
        /// ██╔══██╗██╔════╝████╗ ████║██╔═══██╗██║   ██║██╔════╝
        /// ██████╔╝█████╗  ██╔████╔██║██║   ██║██║   ██║█████╗  
        /// ██╔══██╗██╔══╝  ██║╚██╔╝██║██║   ██║╚██╗ ██╔╝██╔══╝  
        /// ██║  ██║███████╗██║ ╚═╝ ██║╚██████╔╝ ╚████╔╝ ███████╗
        /// ╚═╝  ╚═╝╚══════╝╚═╝     ╚═╝ ╚═════╝   ╚═══╝  ╚══════╝
        ///======================================================
        /// <summary>
        /// 从列表中移除指定的序号上的元素
        /// </summary>
        /// <typeparam name="T"> 素组元素类型 </typeparam>
        /// <param name="_indexes"> 需要移除的序号集合 </param>
        public static void RemoveAt<T>(this List<T> _source, ICollection<int> _indexes)
        {
            if (_indexes.Count is 0) return;
            var idxes = _indexes.Distinct().ToArray();

            Array.Sort(idxes);
            if (idxes[0] < 0 || idxes[^0] >= _source.Count) throw new ArgumentOutOfRangeException();
            _source.RemoveAll(n => _indexes.Contains(_source.IndexOf(n)));
        }
    }
}