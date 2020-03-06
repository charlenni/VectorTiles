using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using VectorTiles.MapboxGL.Expressions;

namespace Tests
{
    [TestFixture]
    public class ExpressionString
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            var exp = ExpressionParser.ParseExpression("[\"length\", \"Teststring\"]", typeof(MGLStringType));
            exp = ExpressionParser.ParseExpression("[\"Test\", 1.0, true]", typeof(MGLStringType));
            exp = ExpressionParser.ParseExpression("true",typeof(MGLBooleanType));
            exp = ExpressionParser.ParseExpression("null", typeof(MGLNullType));
            exp = ExpressionParser.ParseExpression("5", typeof(MGLNumberType));
            exp = ExpressionParser.ParseExpression("4.321", typeof(MGLNumberType));
            var str = JsonConvert.DeserializeObject("\"Test\"");
            var integer = JsonConvert.DeserializeObject("4");
            var number = JsonConvert.DeserializeObject("5.3");
            var array = JsonConvert.DeserializeObject("[\"Test\", 1.0, true]");
        }
    }
}
