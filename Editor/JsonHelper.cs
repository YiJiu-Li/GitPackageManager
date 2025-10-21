using System;
using System.Collections.Generic;
using UnityEngine;

namespace GitPackageManager
{
    /// <summary>
    /// 处理JSON序列化和反序列化的辅助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 将Dictionary转换为JSON字符串
        /// </summary>
        [Serializable]
        private class DictionaryWrapper<TKey, TValue>
        {
            public List<KeyValuePairData<TKey, TValue>> items =
                new List<KeyValuePairData<TKey, TValue>>();

            public DictionaryWrapper(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    return;

                foreach (var kvp in dictionary)
                {
                    items.Add(new KeyValuePairData<TKey, TValue>(kvp.Key, kvp.Value));
                }
            }

            public Dictionary<TKey, TValue> ToDictionary()
            {
                Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
                foreach (var item in items)
                {
                    result[item.key] = item.value;
                }
                return result;
            }
        }

        /// <summary>
        /// 表示键值对的序列化类
        /// </summary>
        [Serializable]
        private class KeyValuePairData<TKey, TValue>
        {
            public TKey key;
            public TValue value;

            public KeyValuePairData(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        /// <summary>
        /// 将Dictionary序列化为JSON
        /// </summary>
        public static string ToJson<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            return JsonUtility.ToJson(new DictionaryWrapper<TKey, TValue>(dictionary));
        }

        /// <summary>
        /// 从JSON反序列化Dictionary
        /// </summary>
        public static Dictionary<TKey, TValue> FromJson<TKey, TValue>(string json)
        {
            DictionaryWrapper<TKey, TValue> wrapper = JsonUtility.FromJson<
                DictionaryWrapper<TKey, TValue>
            >(json);
            return wrapper.ToDictionary();
        }

        /// <summary>
        /// 从JSON反序列化对象数组
        /// </summary>
        public static T[] FromJsonArray<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.items;
        }

        /// <summary>
        /// 将对象数组序列化为JSON
        /// </summary>
        public static string ToJsonArray<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.items = array;
            return JsonUtility.ToJson(wrapper);
        }

        /// <summary>
        /// 用于序列化/反序列化数组的包装类
        /// </summary>
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}
