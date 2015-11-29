namespace SemanticVersionManagerTests
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SemanticVersionManager;

    /// <summary>
    /// VersionFormatter class tests.
    /// </summary>
    [TestClass]
    public class VersionFormatterTests
    {
        /// <summary>
        /// Test GetJulianFormat method to prove when ommit the date parameter use the actual date.
        /// </summary>
        [TestMethod]
        public void GetJulianFormat_WithoutProvideDate_ReturnsTheActualDateFormatted()
        {
            // Arrange
            var expectedJulianDate = DateTime.Now.Year.ToString() + DateTime.Now.DayOfYear;

            // Act
            var returnedDate = new VersionFormatter().GetJulianFormat();

            // Assert
            Assert.AreEqual(expectedJulianDate, returnedDate);
        }

        /// <summary>
        /// Test GetJulianFormat method to prove when providing the date parameter use the value passed.
        /// </summary>
        [TestMethod]
        public void GetJulianFormat_ProvidingDate_ReturnsTheValuePassedFormatted()
        {
            // Arrange
            var dateProvided = DateTime.Now.AddDays(23);
            var expectedJulianDate = dateProvided.Year.ToString() + dateProvided.DayOfYear;

            // Act
            var returnedDate = new VersionFormatter().GetJulianFormat(dateProvided);

            // Assert
            Assert.AreEqual(expectedJulianDate, returnedDate);
        }

        /// <summary>
        /// Test GetJulianFormat method to prove when providing the date parameter use the value passed.
        /// </summary>
        [TestMethod]
        public void GetJulianFormat_ProvidingDateAtBegininOfYear_ReturnsTheValuePassedFormattedInTheFormYYDDD()
        {
            // Arrange
            var dateProvided = new DateTime(DateTime.Today.Year, 1, 23);
            var expectedJulianDate = dateProvided.Year.ToString() + "023";

            // Act
            var returnedDate = new VersionFormatter().GetJulianFormat(dateProvided);

            // Assert
            Assert.AreEqual(expectedJulianDate, returnedDate);
        }

        [TestMethod]
        public void GetAutoIncremented_WhenNoValueIsPassed_ReturnsZero()
        {
            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetAutoIncremented();

            // Assert
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void GetAutoIncremented_WhenAValidValueIsPassed_ReturnsTheValueIncremented()
        {
            // Arrange
            var valueToParse = "9";
            var expectedValue = "10";

            // Act
            var versionFormatter = new VersionFormatter();
            var resultValue = versionFormatter.GetAutoIncremented(valueToParse);

            // Assert
            Assert.AreEqual(expectedValue, resultValue);
        }

        [TestMethod]
        public void GetAutoIncremented_WhenAnInvalidDoubleValueIsPassed_ThrowsFormatException()
        {
            // Arrange
            var valueToParse = "1.3";
            var exceptionMessage = $"The argument with value [{valueToParse}] cannot be converted to a valid integer.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetAutoIncremented(valueToParse);
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                // Assert
                Assert.AreEqual(exceptionMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetAutoIncremented_WhenAnInvalidStringValueIsPassed_ThrowsFormatException()
        {
            // Arrange
            var valueToParse = "not_valid_value";
            var exceptionMessage = $"The argument with value [{valueToParse}] cannot be converted to a valid integer.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetAutoIncremented(valueToParse);
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                // Assert
                Assert.AreEqual(exceptionMessage, ex.Message);
            }
        }

        [TestMethod]
        public void GetAutoIncremented_WhenAValidValueMaxIntegerIsPassed_ReturnsZero()
        {
            // Arrange
            var valueToParse = int.MaxValue.ToString();
            var expectedValue = "0";

            // Act
            var versionFormatter = new VersionFormatter();
            var resultValue = versionFormatter.GetAutoIncremented(valueToParse);

            // Assert
            Assert.AreEqual(expectedValue, resultValue);
        }

        [TestMethod]
        public void GetAutoIncremented_WhenPassedANegativeValidInteger_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var valueToParse = "-5";
            var exceptionMessage = $"The argument with value [{valueToParse}] it's out of valid range, only positive integers and zero are accepted.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetAutoIncremented(valueToParse);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.StartsWith(exceptionMessage));
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenNoValueIsPassed_ReturnsZero()
        {
            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.NormalizeTFSBuildNumber();

            // Assert
            Assert.AreEqual("0", result);
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenAValidValueIsPassed_ReturnsTheValueDecremented()
        {
            // Arrange
            var valueToParse = "9";
            var expectedValue = "8";

            // Act
            var versionFormatter = new VersionFormatter();
            var resultValue = versionFormatter.NormalizeTFSBuildNumber(valueToParse);

            // Assert
            Assert.AreEqual(expectedValue, resultValue);
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenAnInvalidDoubleValueIsPassed_ThrowsFormatException()
        {
            // Arrange
            var valueToParse = "1.3";
            var exceptionMessage = $"The argument with value [{valueToParse}] cannot be converted to a valid integer.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.NormalizeTFSBuildNumber(valueToParse);
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                // Assert
                Assert.AreEqual(exceptionMessage, ex.Message);
            }
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenAnInvalidStringValueIsPassed_ThrowsFormatException()
        {
            // Arrange
            var valueToParse = "not_valid_value";
            var exceptionMessage = $"The argument with value [{valueToParse}] cannot be converted to a valid integer.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.NormalizeTFSBuildNumber(valueToParse);
                Assert.Fail();
            }
            catch (FormatException ex)
            {
                // Assert
                Assert.AreEqual(exceptionMessage, ex.Message);
            }
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenAZeroValidIsPassed_ReturnsZero()
        {
            // Arrange
            var valueToParse = "0";
            var expectedValue = "0";

            // Act
            var versionFormatter = new VersionFormatter();
            var resultValue = versionFormatter.NormalizeTFSBuildNumber(valueToParse);

            // Assert
            Assert.AreEqual(expectedValue, resultValue);
        }

        [TestMethod]
        public void NormalizeTFSBuildNumber_WhenPassedANegativeValidInteger_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var valueToParse = "-5";
            var exceptionMessage = $"The argument with value [{valueToParse}] it's out of valid range, only positive integers and zero are accepted.";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.NormalizeTFSBuildNumber(valueToParse);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.StartsWith(exceptionMessage));
                Assert.AreEqual("tfsBuildNumber", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithPatternAndLessValuesThanUsedInPattern_ThrowsArgumentException()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}.{REVISION}";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "REVISION", "0" } };
            var expectedMessage = "The number of values is less than specified in the pattern or not all values has been provided.\nMandatory variables found: MAJOR, MINOR, PATCH, REVISION\nValues provided: MAJOR, MINOR, REVISION";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetVersionFormatted(pattern, values);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.StartsWith(expectedMessage));
                Assert.AreEqual("values", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithPatternAndOtherValuesThanUsedInPattern_ThrowsArgumentException()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}.{REVISION}";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "REVISION", "0" }, { "BUILD", "0" } };
            var expectedMessage = "The number of values is less than specified in the pattern or not all values has been provided.\nMandatory variables found: MAJOR, MINOR, PATCH, REVISION\nValues provided: MAJOR, MINOR, REVISION, BUILD";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetVersionFormatted(pattern, values);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.StartsWith(expectedMessage));
                Assert.AreEqual("values", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithInvalidPattern_ThrowsArgumentException()
        {
            // Arrange
            var pattern = "NOTHING.TO.REPLACE";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "REVISION", "0" }, { "BUILD", "0" } };
            var expectedMessage = "The pattern seems not be valid because any variable has been found to replace";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetVersionFormatted(pattern, values);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.StartsWith(expectedMessage));
                Assert.AreEqual("pattern", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithNullDictionaryInValues_ThrowsArgumentNullException()
        {
            // Arrange
            var pattern = "NOTHING.TO.REPLACE";

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetVersionFormatted(pattern, null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual("values", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithoutDictionaryValues_ThrowsArgumentNullException()
        {
            // Arrange
            var pattern = "NOTHING.TO.REPLACE";
            var values = new Dictionary<string, string>();

            // Act
            var versionFormatter = new VersionFormatter();
            try
            {
                versionFormatter.GetVersionFormatted(pattern, values);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual("values", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithPatternWithOptionalVarsAndAllValuesThanUsedInMandatoryPart_ReplaceMandatoryAndRemoveOptional()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}[.{BUILD}{REVISION}]";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "PATCH", "0" } };
            var expectedVersion = "1.1.0";

            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetVersionFormatted(pattern, values);

            // Assert
            Assert.AreEqual(expectedVersion, result);
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithPatternAndAllValuesUsed_ReplaceAllVariables()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}[.{BUILD}{REVISION}]";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "PATCH", "0" }, { "BUILD", "B" }, { "REVISION", "1" } };
            var expectedVersion = "1.1.0.B1";

            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetVersionFormatted(pattern, values);

            // Assert
            Assert.AreEqual(expectedVersion, result);
        }

        [TestMethod]
        public void GetVersionFormatted_WhenCalledWithPatternAndNotAllValuesInOptionalPart_ReplaceAllVariablesAndRemoveOptionalNotProvided()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}[.{BUILD}{REVISION}]";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "1" }, { "PATCH", "0" }, { "BUILD", "B" } };
            var expectedVersion = "1.1.0.B";

            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetVersionFormatted(pattern, values);

            // Assert
            Assert.AreEqual(expectedVersion, result);
        }

        [TestMethod]
        public void GetVersionFormatted_WithComplexPatternAndAllVarsInformed_ReplaceAllVariables()
        {
            // Arrange
            var pattern = "{MAJOR}[.{MINOR}].{PATCH}[.{BUILD}{REVISION}]";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "2" }, { "PATCH", "4" }, { "BUILD", "B" }, { "REVISION", "9" } };
            var expectedVersion = "1.2.4.B9";

            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetVersionFormatted(pattern, values);

            // Assert
            Assert.AreEqual(expectedVersion, result);
        }

        [TestMethod]
        public void GetVersionFormatted_WithSimplePatternAndAllVarsInformed_ReplaceAllVariables()
        {
            // Arrange
            var pattern = "{MAJOR}.{MINOR}.{PATCH}";
            var values = new Dictionary<string, string> { { "MAJOR", "1" }, { "MINOR", "2" }, { "PATCH", "4" } };
            var expectedVersion = "1.2.4";

            // Act
            var versionFormatter = new VersionFormatter();
            var result = versionFormatter.GetVersionFormatted(pattern, values);

            // Assert
            Assert.AreEqual(expectedVersion, result);
        }

        //[TestMethod]
        //public void RegEx_Test()
        //{
        //    var pattern = "{MAJOR}.{MINOR}.{PATCH}.{REVISION}";
        //    var pattern2 = "{MAJOR}.{MINOR}.{PATCH}[.{BUILD}{REVISION}]";

        //    // expression to detect variables
        //    var regex = new Regex(@"(?<=\{)[^}]+(?=\})");

        //    // expression to detect optional variables
        //    var regex2 = new Regex(@"(?<=\[)[^]]+(?=\])");


        //    var result = regex2.Matches(pattern2);
        //}
    }
}