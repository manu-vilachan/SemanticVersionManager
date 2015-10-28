using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SemanticVersionManagerTests
{
    using System;

    using SemanticVersionManager;

    [TestClass]
    public class ArgumentsParserTests
    {
        [TestMethod]
        public void Parse_WithSimpleArgsWithDelimitier_ReturnsDictionaryWithEmptyValues()
        {
            var testedArguments = new[] { "--Arg1", "--Arg2", "--Arg3" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(3, result.Count);

            foreach (var pair in result)
            {
                Assert.IsFalse(pair.Key.StartsWith("--"));
                Assert.IsNull(pair.Value);
            }
        }

        [TestMethod]
        public void Parse_WithSimpleArgsWithoutDelimitier_ReturnsDictionaryWithValuesAndGenericKeys()
        {
            var testedArguments = new[] { "Arg1", "Arg2", "Arg3" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(1, result.Count);

            string key = "NotNamedArg";
            for (int i = 0; i < testedArguments.Length; i++)
            {
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(testedArguments[i], result[key][i]);
            }
        }

        [TestMethod]
        public void Parse_WithArgsWithDelimitier_ReturnsDictionaryWithValues()
        {
            var testedArguments = new[] { "--Arg1", "Value1", "--Arg2", "Value2" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(2, result.Count);

            for (int i = 0; i < testedArguments.Length; i = i + 2)
            {
                var key = testedArguments[i].Replace("--", "");
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(testedArguments[i+1], result[key][0]);
            }
        }

        [TestMethod]
        public void Parse_WithMixedArgsWithoutDelimitierAtBegining_ReturnsDictionaryWithValues()
        {
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--Arg2", "Value2", "--SimpleArg" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(4, result.Count);

            var notNamedKey = "NotNamedArg";
            Assert.IsTrue(result.ContainsKey(notNamedKey));
            Assert.AreEqual(testedArguments[0], result[notNamedKey][0]);

            for (int i = 1; i < testedArguments.Length-1; i = i + 2)
            {
                var key = testedArguments[i].Replace("--", "");
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(testedArguments[i + 1], result[key][0]);
            }

            var simpleArg = "SimpleArg";
            Assert.IsTrue(result.ContainsKey(simpleArg));
            Assert.IsNull(result[simpleArg]);
        }

        [TestMethod]
        public void Parse_WithMixedAndNotOrderedArgsWithoutDelimitierAtBegining_ReturnsDictionaryWithValues()
        {
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg2", "Value2" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(4, result.Count);

            var notNamedKey = "NotNamedArg";
            Assert.IsTrue(result.ContainsKey(notNamedKey));
            Assert.AreEqual(testedArguments[0], result[notNamedKey][0]);

            var numberedArg = "Arg1";
            Assert.IsTrue(result.ContainsKey(numberedArg));
            Assert.AreEqual(testedArguments[2], result[numberedArg][0]);

            numberedArg = "Arg2";
            Assert.IsTrue(result.ContainsKey(numberedArg));
            Assert.AreEqual(testedArguments[5], result[numberedArg][0]);

            var simpleArg = "SimpleArg";
            Assert.IsTrue(result.ContainsKey(simpleArg));
            Assert.IsNull(result[simpleArg]);
        }

        [TestMethod]
        public void Parse_WhenThereAreMoreArgsThanSpecified_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg2", "Value2" };
            var parser = new ArgumentsParser("--");

            // Act
            try
            {
                parser.Parse(testedArguments, maxArgsExpected: 1);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                //Assert
                Assert.IsTrue(ex.Message.StartsWith("The number of arguments is less than expected.\nThere are 4 when expected 1."));
                Assert.AreEqual("args", ex.ParamName);
            }
        }

        [TestMethod]
        public void Parse_WhenThereAreLessArgsThanSpecified_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg2", "Value2" };
            var parser = new ArgumentsParser("--");

            // Act
            try
            {
                parser.Parse(testedArguments, minArgsExpected: 10);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                //Assert
                Assert.IsTrue(ex.Message.StartsWith("The number of arguments is less than expected.\nThere are 4 when expected 10."));
                Assert.AreEqual("args", ex.ParamName);
            }
        }

        [TestMethod]
        public void Parse_WithRepeatedArgs_ContainsAllValuesInSinglekey()
        {
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg1", "Value2" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(3, result.Count);

            var notNamedKey = "NotNamedArg";
            Assert.IsTrue(result.ContainsKey(notNamedKey));
            Assert.AreEqual(testedArguments[0], result[notNamedKey][0]);

            var numberedArg = "Arg1";
            Assert.IsTrue(result.ContainsKey(numberedArg));
            Assert.AreEqual(testedArguments[2], result[numberedArg][0]);
            Assert.AreEqual(testedArguments[5], result[numberedArg][1]);

            var simpleArg = "SimpleArg";
            Assert.IsTrue(result.ContainsKey(simpleArg));
            Assert.IsNull(result[simpleArg]);
        }

        [TestMethod]
        public void Parse_WithRepeatedSimpleArgs_ContainsAllValuesInSinglekeyAndNotThrowException()
        {
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg1", "Value2", "--SimpleArg" };

            var parser = new ArgumentsParser("--");

            var result = parser.Parse(testedArguments);

            Assert.AreEqual(3, result.Count);

            var notNamedKey = "NotNamedArg";
            Assert.IsTrue(result.ContainsKey(notNamedKey));
            Assert.AreEqual(testedArguments[0], result[notNamedKey][0]);

            var numberedArg = "Arg1";
            Assert.IsTrue(result.ContainsKey(numberedArg));
            Assert.AreEqual(testedArguments[2], result[numberedArg][0]);
            Assert.AreEqual(testedArguments[5], result[numberedArg][1]);

            var simpleArg = "SimpleArg";
            Assert.IsTrue(result.ContainsKey(simpleArg));
            Assert.IsNull(result[simpleArg]);
        }

        [TestMethod]
        public void Parse_WithArgsWithinTheRange_DoNotThrowException()
        {
            // Arrange
            var testedArguments = new[] { "Value 0", "--Arg1", "Value1", "--SimpleArg", "--Arg1", "Value2", "--SimpleArg" };
            var parser = new ArgumentsParser("--");

            // Act
            try
            {
                parser.Parse(testedArguments, 1, 10);
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Fail();
            }

            // Assert
        }
    }
}
