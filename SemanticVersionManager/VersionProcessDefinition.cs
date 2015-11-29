using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SemanticVersionManager
{
    public class VersionProcessDefinition
    {
        public VersionNumbers Numbers { get; set; } = new VersionNumbers();

        public VersionNumbersIncrement Increments { get; set; } = new VersionNumbersIncrement();

        public VersionNumbersPattern Patterns { get; set; } = new VersionNumbersPattern();

        public void Read(XElement element, string buildName)
        {
            var common = element.Element(XmlConstants.CommonVersion);
            var buildElem = element.Descendants(XmlConstants.Build).First(xe => xe.Attribute("name").Value == buildName);

            Numbers.Major = common.Element(XmlConstants.Major).Value;
            Numbers.Minor = common.Element(XmlConstants.Minor).Value;
            Numbers.Patch = common.Element(XmlConstants.Patch).Value;
            Numbers.Build = buildElem.Element(XmlConstants.Build).Value;
            Numbers.Revision = buildElem.Element(XmlConstants.Revision).Value;
            Numbers.Suffix = buildElem.Element(XmlConstants.PreReleaseSuffix).Value;

            Increments.Major = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element(XmlConstants.MajorIncrementMethod).Value, true);
            Increments.Minor = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element(XmlConstants.MinorIncrementMethod).Value, true);
            Increments.Patch = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element(XmlConstants.PatchIncrementMethod).Value, true);
            Increments.Build = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), buildElem.Element(XmlConstants.BuildIncrementMethod).Value, true);
            Increments.Revision = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), buildElem.Element(XmlConstants.RevisionIncrementMethod).Value, true);

            Patterns.AssemblyVersion = common.Element(XmlConstants.VersionNumberFormat).Value;
            Patterns.AssemblyInformationVersion = common.Element(XmlConstants.VersionInformationalFormat).Value;
            Patterns.PackageVersion = common.Element(XmlConstants.VersionNamePackageFormat).Value;
        }

        public void Write(ref XElement element, string buildName)
        {
            var common = element.Element(XmlConstants.CommonVersion);
            var buildElem = element.Descendants(XmlConstants.Build).First(xe => xe.Attribute("name").Value == buildName);

            common.Element(XmlConstants.Major).Value = Numbers.Major;
            common.Element(XmlConstants.Minor).Value = Numbers.Minor;
            common.Element(XmlConstants.Patch).Value = Numbers.Patch;
            buildElem.Element(XmlConstants.Build).Value = Numbers.Build;
            buildElem.Element(XmlConstants.Revision).Value = Numbers.Revision;

            buildElem.Element(XmlConstants.ActualGeneratedVersion).Element(XmlConstants.VersionNumber).Value = Patterns.AssemblyVersion;
            buildElem.Element(XmlConstants.ActualGeneratedVersion).Element(XmlConstants.VersionInformationalNumber).Value = Patterns.AssemblyInformationVersion;
            buildElem.Element(XmlConstants.ActualGeneratedVersion).Element(XmlConstants.VersionNamePackage).Value = Patterns.PackageVersion;
        }

        public void DoIncrements(Func<VersionFormatter, string, IncrementMethod, string, string> processIncrementMethod, Dictionary<string, List<string>> arguments, VersionFormatter formatter)
        {
            Numbers.Major = processIncrementMethod(formatter, Numbers.Major, Increments.Major, arguments[Parameters.Major]?.First());
            Numbers.Minor = processIncrementMethod(formatter, Numbers.Minor, Increments.Minor, arguments[Parameters.Minor]?.First());
            Numbers.Patch = processIncrementMethod(formatter, Numbers.Patch, Increments.Patch, arguments[Parameters.Patch]?.First());
            Numbers.Build = processIncrementMethod(formatter, Numbers.Build, Increments.Build, arguments[Parameters.Build]?.First());
            Numbers.Revision = processIncrementMethod(formatter, Numbers.Revision, Increments.Revision, arguments[Parameters.Revision]?.First());
        }

        public void ApplyPatterns(Func<string, Dictionary<string, string>, string> patternTransform)
        {
            var values = GetValues();

            Patterns.AssemblyVersion = patternTransform(Patterns.AssemblyVersion, values);
            Patterns.AssemblyInformationVersion = patternTransform(Patterns.AssemblyInformationVersion, values);
            Patterns.PackageVersion = patternTransform(Patterns.PackageVersion, values);
        }

        private Dictionary<string, string> GetValues()
        {
            var values = PatternConstants.GetDictionary();
            values[PatternConstants.Major] = Numbers.Major;
            values[PatternConstants.Minor] = Numbers.Minor;
            values[PatternConstants.Patch] = Numbers.Patch;
            values[PatternConstants.Build] = Numbers.Build;
            values[PatternConstants.Revision] = Numbers.Revision;
            values[PatternConstants.Suffix] = Numbers.Suffix;

            return values;
        }
    }
}