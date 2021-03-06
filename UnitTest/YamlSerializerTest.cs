﻿using System.Collections.Generic;
using System.IO;

using Xunit;

using Zhenway.YamlSerializations;

namespace UnitTest
{
    public class YamlSerializerTest
    {
        [Fact]
        public void Test_Basic_1()
        {
            var s = new YamlSerializer();
            var sw = new StringWriter();
            s.Serialize(sw, new Entity { IntValue = 1, StringValue = "abc", BoolValue = true });
            Assert.Equal(@"IntValue: 1
StringValue: abc
BoolValue: true
", sw.ToString());
            var d = new YamlDeserializer();
            var obj = d.Deserialize<Entity>(new StringReader(sw.ToString()));
            Assert.Equal(1, obj.IntValue);
            Assert.Equal("abc", obj.StringValue);
            Assert.Equal(true, obj.BoolValue);
            var dict = d.Deserialize<Dictionary<string, object>>(new StringReader(sw.ToString()));
            Assert.Equal(1, dict["IntValue"]);
            Assert.Equal("abc", dict["StringValue"]);
            Assert.Equal(true, dict["BoolValue"]);
        }

        [Fact]
        public void Test_Basic_2()
        {
            var s = new YamlSerializer();
            var sw = new StringWriter();
            s.Serialize(sw, new Entity { IntValue = 1, StringValue = "true", BoolValue = true });
            Assert.Equal(@"IntValue: 1
StringValue: ""true""
BoolValue: true
", sw.ToString());
            var d = new YamlDeserializer();
            var obj = d.Deserialize<Entity>(new StringReader(sw.ToString()));
            Assert.Equal(1, obj.IntValue);
            Assert.Equal("true", obj.StringValue);
            Assert.Equal(true, obj.BoolValue);
            var dict = d.Deserialize<Dictionary<string, object>>(new StringReader(sw.ToString()));
            Assert.Equal(1, dict["IntValue"]);
            Assert.Equal("true", dict["StringValue"]);
            Assert.Equal(true, dict["BoolValue"]);
        }

        [Fact]
        public void Test_Basic_3()
        {
            var s = new YamlSerializer();
            var sw = new StringWriter();
            s.Serialize(sw, new Entity { IntValue = 1, StringValue = "123", BoolValue = true });
            Assert.Equal(@"IntValue: 1
StringValue: ""123""
BoolValue: true
", sw.ToString());
            var d = new YamlDeserializer();
            var obj = d.Deserialize<Entity>(new StringReader(sw.ToString()));
            Assert.Equal(1, obj.IntValue);
            Assert.Equal("123", obj.StringValue);
            Assert.Equal(true, obj.BoolValue);
            var dict = d.Deserialize<Dictionary<string, object>>(new StringReader(sw.ToString()));
            Assert.Equal(1, dict["IntValue"]);
            Assert.Equal("123", dict["StringValue"]);
            Assert.Equal(true, dict["BoolValue"]);
        }

        [Fact]
        public void Test_Basic_4()
        {
            var s = new YamlSerializer();
            var sw = new StringWriter();
            s.Serialize(sw, new Entity { IntValue = 1, StringValue = "1.23", BoolValue = true });
            Assert.Equal(@"IntValue: 1
StringValue: ""1.23""
BoolValue: true
", sw.ToString());
            var d = new YamlDeserializer();
            var obj = d.Deserialize<Entity>(new StringReader(sw.ToString()));
            Assert.Equal(1, obj.IntValue);
            Assert.Equal("1.23", obj.StringValue);
            Assert.Equal(true, obj.BoolValue);
            var dict = d.Deserialize<Dictionary<string, object>>(new StringReader(sw.ToString()));
            Assert.Equal(1, dict["IntValue"]);
            Assert.Equal("1.23", dict["StringValue"]);
            Assert.Equal(true, dict["BoolValue"]);
        }

        [Fact]
        public void Test_Basic_Dictionary()
        {
            var s = new YamlSerializer();
            var sw = new StringWriter();
            s.Serialize(
                sw,
                new Dictionary<string, object>
                {
                    ["IntValue"] = 1,
                    ["StringValue"] = "1.23",
                    ["BoolValue"] = true
                });
            Assert.Equal(@"IntValue: 1
StringValue: ""1.23""
BoolValue: true
", sw.ToString());
            var d = new YamlDeserializer();
            var obj = d.Deserialize<Entity>(new StringReader(sw.ToString()));
            Assert.Equal(1, obj.IntValue);
            Assert.Equal("1.23", obj.StringValue);
            Assert.Equal(true, obj.BoolValue);
            var dict = d.Deserialize<Dictionary<string, object>>(new StringReader(sw.ToString()));
            Assert.Equal(1, dict["IntValue"]);
            Assert.Equal("1.23", dict["StringValue"]);
            Assert.Equal(true, dict["BoolValue"]);
        }

        public class Entity
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
            public bool BoolValue { get; set; }
        }
    }
}
