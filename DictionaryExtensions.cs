
using System.Collections.Generic;

namespace VodByte.Utility
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 为一个 值 为集合的 List 的词典添加元素
        /// </summary>
        /// <typeparam name="TKey"> 键 的类型</typeparam>
        /// <typeparam name="TElement"> 作为 List 的 值 中的单个元素的类型 </typeparam>
        /// <param name="_dict"> 目标词典 </param>
        /// <param name="_key"> 键 </param>
        /// <param name="_element"> 需要添加的条目 </param>
        public static void AddOrUpdate<TKey, TElement>(this IDictionary<TKey, List<TElement>> _dict, TKey _key, TElement _element)
        {
            if (!_dict.ContainsKey(_key)) _dict.Add(_key, new List<TElement>());
            _dict[_key].Add(_element);
        }

        ///---------------------------------------------------------------
        /// <summary>
        /// 为一个 值 为集合的 HashSet 的词典添加元素
        /// </summary>
        /// <typeparam name="TKey"> 键 的类型</typeparam>
        /// <typeparam name="TElement"> 作为 Set 的 值 中的单个元素的类型 </typeparam>
        /// <param name="_dict"> 目标词典 </param>
        /// <param name="_key"> 键 </param>
        /// <param name="_element"> 需要添加的条目 </param>
        public static void AddOrUpdate<TKey, TElement>(this IDictionary<TKey, HashSet<TElement>> _dict, TKey _key, TElement _element)
        {
            if (!_dict.ContainsKey(_key)) _dict.Add(_key, new HashSet<TElement>());
            _dict[_key].Add(_element);
        }

        ///---------------------------------------------------------------
        /// <summary>
        /// 从一个值为列表的词典中删除列表中的元组
        /// </summary>
        /// <typeparam name="TKey"> 键的类型 </typeparam>
        /// <typeparam name="TElement"> 需要删除的，保存在列表中的值类型 </typeparam>
        /// <param name="_dict"> 需要被操作的词典 </param>
        /// <param name="_key"> 键 </param>
        /// <param name="_element"> 删除元素 </param>
        /// <returns></returns>
        public static bool Remove<TKey, TElement>(this IDictionary<TKey, List<TElement>> _dict, TKey _key, TElement _element)
        {
            if (_dict.ContainsKey(_key) && _dict[_key].Contains(_element))
            {
                _dict[_key].Remove(_element);
                return true;
            }

            return false;
        }

        ///---------------------------------------------------------------
        public static bool IsEmpty<TKey, TElement>(this IDictionary<TKey, List<TElement>> _dict)
        {
            if (_dict.Count is 0) return true;
            foreach (var pair in _dict)
            {
                List<TElement> list = pair.Value;
                if (list != null && list.Count > 0) return false;
            }

            return true;
        }

        ///---------------------------------------------------------------
        public static void AddRange<TKey, TElement>(this IDictionary<TKey, TElement> _dict, IDictionary<TKey, TElement> _source)
        {
            foreach (var pair in _source) _dict.Add(pair);
        }
    }
}