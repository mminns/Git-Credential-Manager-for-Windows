using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Alm.Authentication;
using Newtonsoft.Json;

namespace Prototype
{
    public class SimpleFileStorage : Storage
    {
        private string _filename = "simplefile.storage";

        public SimpleFileStorage(RuntimeContext context) : base(context)
        {
        }

        public override bool TryReadSecureData(string key, out string name, out byte[] data)
        {
            if (File.Exists(_filename))
            {
                var lines = File.ReadAllLines(_filename);

                var entry = FindByKey(key, lines);
                if (entry != null)
                {
                    name = entry.Name;

                    data = Encoding.UTF8.GetBytes(entry.Data);
                    return true;
                }
            }

            name = null;
            data = null;
            return false;
        }

        private static Entry FindByKey(string key, IEnumerable<string> lines)
        {
            var entry = lines.Select(l => JsonConvert.DeserializeObject<Entry>(l))
                .FirstOrDefault(e => e.Key.Equals(key));
            return entry;
        }

        public override bool TryWriteSecureData(string key, string name, byte[] data)
        {
            var entry = new Entry() {Key = key, Name = name, Data = Encoding.UTF8.GetString(data, 0, data.Length)};
            var lines = new List<string>();

            if (File.Exists(_filename))
            {
                lines.AddRange(File.ReadAllLines(_filename).ToList());
            }

            var existingEntry = FindByKey(key, lines);

            if (existingEntry != null)
            {
                lines.Remove(lines.FirstOrDefault(l => l.Equals(JsonConvert.SerializeObject(existingEntry))));
            }

            lines.Add(JsonConvert.SerializeObject(entry));

            File.WriteAllLines(_filename, lines);
            return true;
        }
    }

    public class Entry
    {
        [JsonProperty]
        public string Key { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Data { get; set; }

    }
}