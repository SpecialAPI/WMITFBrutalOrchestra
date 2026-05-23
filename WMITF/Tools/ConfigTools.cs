using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WMITF.Tools
{
    public static class ConfigTools
    {
        private const string ElementSeparator = ",";
        private const string KVPSeparator = "=";
        private static readonly string[] ElementSeparatorArray = [ElementSeparator];

        public static void RegisterListConfigType<T>()
        {
            TomlTypeConverter.AddConverter(typeof(ConfigList<T>), new()
            {
                ConvertToString = (obj, type) => SerializeList((ConfigList<T>)obj),
                ConvertToObject = (str, type) => DeserializeList<T>(str)
            });
        }

        public static void RegisterDictionaryConfigType<TKey, TValue>()
        {
            TomlTypeConverter.AddConverter(typeof(ConfigDictionary<TKey, TValue>), new()
            {
                ConvertToString = (obj, type) => SerializeDictionary((ConfigDictionary<TKey, TValue>)obj),
                ConvertToObject = (str, type) => DeserializeDictionary<TKey, TValue>(str)
            });
        }

        public static string SerializeList<T>(List<T> list)
        {
            if (list == null)
                return string.Empty;

            var i = 0;
            var output = "";
            foreach (var item in list)
            {
                if (i++ > 0)
                    output += ElementSeparator;

                output += TomlTypeConverter.ConvertToString(item, typeof(T));
            }

            return output;
        }

        public static ConfigList<T> DeserializeList<T>(string str)
        {
            if (string.IsNullOrEmpty(str))
                return [];

            var elems = str.Split(ElementSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
            var result = new ConfigList<T>();

            foreach(var elemStr in elems)
                result.Add(TomlTypeConverter.ConvertToValue<T>(elemStr));

            return result;
        }

        public static string SerializeDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return string.Empty;

            var i = 0;
            var output = "";
            foreach(var kvp in dictionary)
            {
                if (i++ > 0)
                    output += ElementSeparator;

                var keyStr = TomlTypeConverter.ConvertToString(kvp.Key, typeof(TKey));
                var valueStr = TomlTypeConverter.ConvertToString(kvp.Value, typeof(TValue));
                output += $"{keyStr}{KVPSeparator}{valueStr}";
            }

            return output;
        }

        public static ConfigDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(string str)
        {
            if(string.IsNullOrEmpty(str))
                return [];

            var elems = str.Split(ElementSeparatorArray, StringSplitOptions.RemoveEmptyEntries);
            var result = new ConfigDictionary<TKey, TValue>();

            foreach(var elemStr in elems)
            {
                var sepIdx = elemStr.IndexOf(KVPSeparator);

                if (sepIdx < 0)
                    continue;

                var keyStr = elemStr.Substring(0, sepIdx);
                var key = TomlTypeConverter.ConvertToValue<TKey>(keyStr);

                var valueStr = elemStr.Substring(sepIdx + 1);
                var value = TomlTypeConverter.ConvertToValue<TValue>(valueStr);

                result[key] = value;
            }

            return result;
        }
    }
}
