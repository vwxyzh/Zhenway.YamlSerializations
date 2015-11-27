using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using YamlDotNet.Serialization;

using Zhenway.YamlSerializations;

namespace PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceList = new Item[10000];
            for (int i = 0; i < sourceList.Length; i++)
            {
                sourceList[i] = new Item { IntValue = i, StringValue = i.ToString() };
            }
            var mySerializer = new YamlSerializer();
            var myDeserializer = new YamlDeserializer();
            var defaultSerializer = new Serializer();
            var defaultDeserializer = new Deserializer();
            var watch = new Stopwatch();

            while (true)
            {
                var sw = new StringWriter();
                watch.Restart();
                mySerializer.Serialize(sw, sourceList);
                var stime = watch.ElapsedMilliseconds;
                watch.Restart();
                var list = myDeserializer.Deserialize<List<Item>>(new StringReader(sw.ToString()));
                var dtime = watch.ElapsedMilliseconds;
                Console.WriteLine("My - Serialize time: {0}ms, Deserialize time: {1}ms", stime, dtime);

                sw = new StringWriter();
                watch.Restart();
                defaultSerializer.Serialize(sw, sourceList);
                stime = watch.ElapsedMilliseconds;
                watch.Restart();
                list = defaultDeserializer.Deserialize<List<Item>>(new StringReader(sw.ToString()));
                dtime = watch.ElapsedMilliseconds;
                Console.WriteLine("Default - Serialize time: {0}ms, Deserialize time: {1}ms", stime, dtime);
            }
        }
    }

    public class Item
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
    }

}
