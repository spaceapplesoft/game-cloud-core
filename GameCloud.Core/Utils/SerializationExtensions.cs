using System.Collections.Generic;
using System.Text;

namespace GameCloud.Core.Utils
{
    public static class SerializationExtensions
    {
        public static byte[] ToBytes(this Dictionary<string, string> dictionary)
        {
            var writer = new NetWriter();
            writer.Write(dictionary.Count);

            foreach (var pair in dictionary)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            return writer.ToArray();
        }

        public static byte[] ToBytes(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        public static Dictionary<string, string> FromReader(this Dictionary<string, string> dictionary,
            NetReader reader)
        {
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key] = value;
                }
                else
                {
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }

        public static Dictionary<string, string> FromBytes(this Dictionary<string, string> dictionary, byte[] data)
        {
            var reader = new NetReader(data);
            return dictionary.FromReader(reader);
        }
    }
}