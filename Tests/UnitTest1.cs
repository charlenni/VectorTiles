using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using VectorTiles.MapboxGL.Json;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        StreamReader reader;
        JsonStyleFile jsonStyleFile;
        List<string> errors = new List<string>();

        [SetUp]
        public void Setup()
        {
            var stream = File.OpenRead(@"..\..\..\Fixture\root-properties.input.json");
            reader = new StreamReader(stream);

        }

        [Test]
        public void Test()
        {
            Exception ex = Assert.Throws<JsonSerializationException>(CallDeserializedObject);
        }

        void CallDeserializedObject()
        {
            var settings = new JsonSerializerSettings();

            jsonStyleFile = JsonConvert.DeserializeObject<JsonStyleFile>(reader.ReadToEnd(), settings);
        }
    }
}