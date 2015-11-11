using System;
using System.Collections.Generic;
using System.Linq;

namespace SemanticVersionManager
{
    using System.IO;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public class Program
    {
        public static Dictionary<string, List<string>> Arguments { get; set; }
        
        /// <summary>
        /// Invoke with arguments
        /// <para>VersionControlPath: path to version control xml file with the definition.</para>
        /// <para>Definition: name of Definition to look for in VersionControl.xml.</para>
        /// <para>BuildName: name of the Build tag to look for within the Definition passed.</para>
        /// <para>Action: the reason to apply the current versioning: Patch (Default) -Major -Minor -Patch -Build -Revision| Promote -destDefinition -destBuild | SetNewVersion (reset build & revision) -Major -Minor -Patch</para>
        /// <para>GenerateVersionControlFile: path to where generate a version control xml file with the definitions.</para>
        /// </summary>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            var argsParser = new ArgumentsParser("-", true);
            Arguments = argsParser.Parse(args);

            if (Arguments.ContainsKey(Parameters.GenerateVC.TL()))
            {
                return ParseGenerateVC();
            }

            if (!Arguments.ContainsKey(Parameters.VCPath.TL()) || !Arguments[Parameters.VCPath.TL()].Any())
            {
                PrintHelp(Parameters.VCPath);
                return 1;
            }

            if (!File.Exists(Arguments[Parameters.VCPath.TL()].First()))
            {
                Console.WriteLine("File not found:{Arguments[Parameters.VCPath].First()}");
                return 1;
            }

            if (!ValidateAction())
            {
                PrintHelp(Parameters.Action);
                return 1;
            }

            DoVersioning();

            return 0;
        }

        private static bool ValidateAction()
        {
            if (!Arguments.ContainsKey(Parameters.Action.TL()))
            {
                return true;
            }

            var argAction = Arguments[Parameters.Action.TL()].First();
            VersioningAction action;
            if (!Enum.TryParse(argAction, true, out action))
            {
                return false;
            }

            var result = true;
            switch (action)
            {
                case VersioningAction.SetNewVersion:
                    result = (Arguments.ContainsKey(Parameters.Major.TL()) && Arguments[Parameters.Major.TL()].Any() && ValidInt(Arguments[Parameters.Major.TL()].First()))
                             && (Arguments.ContainsKey(Parameters.Minor.TL()) && Arguments[Parameters.Minor.TL()].Any() && ValidInt(Arguments[Parameters.Minor.TL()].First()))
                             && (Arguments.ContainsKey(Parameters.Patch.TL()) && Arguments[Parameters.Patch.TL()].Any() && ValidInt(Arguments[Parameters.Patch.TL()].First()));
                    break;
                case VersioningAction.Promote:
                    result = Arguments.ContainsKey(Parameters.DestinationDefinition.TL()) && Arguments[Parameters.DestinationDefinition.TL()].Any();
                    break;
            }

            return result;
        }

        private static bool ValidInt(string number)
        {
            int notUsed;
            return int.TryParse(number, out notUsed);
        }

        private static void DoVersioning()
        {
            var fileName = Arguments[Parameters.VCPath.TL()].First();
            var definitionName = (Arguments.ContainsKey(Parameters.Definition.TL()) && Arguments[Parameters.Definition.TL()].Any())
                                     ? Arguments[Parameters.Definition.TL()].First()
                                     : "default";
            var buildName = (Arguments.ContainsKey(Parameters.BuildName.TL()) && Arguments[Parameters.BuildName.TL()].Any())
                                ? Arguments[Parameters.BuildName.TL()].First()
                                : "default";
            var action = VersioningAction.Patch;
            var dstDefinition = string.Empty;
            var dstBuild = string.Empty;
            string major = "0", minor = "0", patch = "0";

            if (Arguments.ContainsKey(Parameters.Action.TL()))
            {
                var argAction = Arguments[Parameters.Action.TL()].First();
                action = (VersioningAction)Enum.Parse(typeof(VersioningAction), argAction, true);
                switch (action)
                {
                    case VersioningAction.Promote:
                        dstDefinition = Arguments[Parameters.DestinationDefinition.TL()].First();
                        dstBuild = Arguments[Parameters.DestinationBuild.TL()].First();
                        // validate that destination has a version lower than source
                        break;
                    case VersioningAction.SetNewVersion:
                        major = Arguments[Parameters.Major.TL()].First();
                        minor = Arguments[Parameters.Minor.TL()].First();
                        patch = Arguments[Parameters.Patch.TL()].First();
                        break;
                }
            }

            var xml = XDocument.Load(fileName);
            var element = xml.XPathSelectElement($"//Definitions/Definition[@type=\"{definitionName}\"]");
            XElement common;
            List<XElement> buildValues;

            switch (action)
            {
                case VersioningAction.SetNewVersion:
                        common = element.Element("CommonVersion");
                        common.Element("Major").Value = major;
                        common.Element("Minor").Value = minor;
                        common.Element("Patch").Value = patch;

                        var toReset = element.Descendants()
                            .SelectMany(xe => xe.Descendants())
                            .Where(xe => xe.Name == "BuildName" || xe.Name == "Revision")
                            .ToList();
                        toReset.ForEach(x => x.Value = "0");
                    break;
                case VersioningAction.Promote:
                    if (dstDefinition.ToLower() == definitionName.ToLower())
                    {
                        throw new ApplicationException("The destination definition can't be the same in a promote operation.");
                    }

                    var dstElement = xml.XPathSelectElement($"//Definitions/Definition[@type=\"{dstDefinition}\"]");
                    common = element.Element("CommonVersion");
                    major = common.Element("Major").Value;
                    minor = common.Element("Minor").Value;
                    patch = common.Element("Patch").Value;

                    common = dstElement.Element("CommonVersion");
                    common.Element("Major").Value = major;
                    common.Element("Minor").Value = minor;
                    common.Element("Patch").Value = patch;

                    buildValues = dstElement.Elements("BuildName")
                            .Where(xe => xe.Attribute("name").Value == dstBuild)
                            .Descendants()
                            .Where(xe => xe.Name == "BuildName" || xe.Name == "Revision")
                            .ToList();
                    buildValues.ForEach(x => x.Value = "0");
                    break;
                case VersioningAction.Patch:
                    DoPatching(element, buildName);
                    break;
            }

            xml.Save(fileName);
            return;
        }

        private static void DoPatching(XElement element, string buildName)
        {
            var common = element.Element("CommonVersion");
            var major = common.Element("Major").Value;
            var minor = common.Element("Minor").Value;
            var patch = common.Element("Patch").Value;
            var majorIM = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element("MajorIncrementMethod").Value, true);
            var minorIM = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element("MinorIncrementMethod").Value, true);
            var patchIM = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), common.Element("PatchIncrementMethod").Value, true);
            var versionPattern = common.Element("VersionNumberFormat").Value;
            var versionInfoPattern = common.Element("VersionInformationalFormat").Value;
            var versionPkgPattern = common.Element("VersionNamePackageFormat").Value;

            var buildElem = element.Descendants("Build").First(xe => xe.Attribute("name").Value == buildName);
            var suffix = buildElem.Element("PreReleaseSuffix").Value;
            var build = buildElem.Element("Build").Value;
            var rev = buildElem.Element("Revision").Value;
            var buildIM = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), buildElem.Element("BuildIncrementMethod").Value, true);
            var revIM = (IncrementMethod)Enum.Parse(typeof(IncrementMethod), buildElem.Element("RevisionIncrementMethod").Value, true);

            var majorSetted = string.Empty;
            var minorSetted = string.Empty;
            var patchSetted = string.Empty;
            var buildSetted = string.Empty;
            var revSetted = string.Empty;

            if (majorIM == IncrementMethod.Setted)
                majorSetted = Arguments[Parameters.Major].First();
            if (minorIM == IncrementMethod.Setted)
                minorSetted = Arguments[Parameters.Minor].First();
            if (patchIM == IncrementMethod.Setted)
                patchSetted = Arguments[Parameters.Patch].First();
            if (buildIM == IncrementMethod.Setted)
                buildSetted = Arguments[Parameters.Build].First();
            if (revIM == IncrementMethod.Setted)
                revSetted = Arguments[Parameters.Revision].First();

            major = ProcessIncrementMethod(major, majorIM, majorSetted);
            minor = ProcessIncrementMethod(minor, minorIM, minorSetted);
            patch = ProcessIncrementMethod(patch, patchIM, patchSetted);
            build = ProcessIncrementMethod(build, buildIM, buildSetted);
            rev = ProcessIncrementMethod(rev, revIM, revSetted);

            var formatter = new VersionFormatter();
            var values = new Dictionary<string, string>
                             {
                                 { "MAJOR", major },
                                 { "MINOR", minor },
                                 { "PATCH", patch },
                                 { "BUILD", build },
                                 { "REVISION", rev },
                                 { "PRSUFFIX", suffix }
                             };
            var version = formatter.GetVersionFormatted(versionPattern, values);
            var versionInfo = formatter.GetVersionFormatted(versionInfoPattern, values);
            var versionPkg = formatter.GetVersionFormatted(versionPkgPattern, values);

            common.Element("Major").Value = major;
            common.Element("Minor").Value = minor;
            common.Element("Patch").Value = patch;
            buildElem.Element("Build").Value = build;
            buildElem.Element("Revision").Value = rev;

            buildElem.Element("ActualGeneratedVersion").Element("VersionNumber").Value = version;
            buildElem.Element("ActualGeneratedVersion").Element("VersionInformationalNumber").Value = versionInfo;
            buildElem.Element("ActualGeneratedVersion").Element("VersionNamePackage").Value = versionPkg;
        }

        private static string ProcessIncrementMethod(string number, IncrementMethod incrementMethod, string settedValue = null)
        {
            var formatter = new VersionFormatter();
            switch (incrementMethod)
            {
                case IncrementMethod.Setted:
                    if (string.IsNullOrWhiteSpace(settedValue))
                    {
                        throw new ArgumentNullException(nameof(settedValue));
                    }
                    number = settedValue;
                    break;
                case IncrementMethod.Julian:
                    number = formatter.GetJulianFormat();
                    break;
                case IncrementMethod.Auto:
                    number = formatter.GetAutoIncremented(number);
                    break;
            }
            return number;
        }

        private static int ParseGenerateVC()
        {
            int retValue = 0;
            if (Arguments.ContainsKey(Parameters.GenerateVC) && Arguments.Count > 1)
            {
                PrintHelp(Parameters.GenerateVC);
                return 1;
            }

            string fileName = "VersioningControl.xml";
            if (Arguments[Parameters.GenerateVC].Any())
            {
                fileName = Arguments[Parameters.GenerateVC].First();
            }

            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SemanticVersionManager.Resources.VersioningControl.xml");
            if (stream == null)
            {
                Console.WriteLine("A versioning control file cannot be created. The embedded resource is not found.");
                return 1;
            }

            FileStream file = null;
            try
            {
                file = new FileStream(fileName, FileMode.Create);
                var b = new byte[stream.Length + 1];
                stream.Read(b, 0, Convert.ToInt32(stream.Length));
                file.Write(b, 0, Convert.ToInt32(b.Length - 1));
                file.Flush();
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("A versioning control file cannot be created:\n{ex.Message}");
                retValue = ex.HResult;
            }
            catch (IOException ex)
            {
                Console.WriteLine("A versioning control file cannot be created:\n{ex.Message}");
                retValue = ex.HResult;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("A versioning control file cannot be created:\n{ex.Message}");
                retValue = ex.HResult;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }

            return retValue;
        }

        private static void PrintHelp(string helpPart)
        {
            if (helpPart.TL() == Parameters.GenerateVC.TL())
            {
                Console.WriteLine($"-{Parameters.GenerateVC}");
                Console.WriteLine("This argument can be used to generate a sample xml with build definitions.");
                Console.WriteLine("Can provide a full path (including file name) where generate the xml,\notherwise the local folder is used with filename VersioningControl.xml.");
                Console.WriteLine("If this argument is used then no other Arguments can be specified.");
                Console.WriteLine("i.e.:\n\tSemanticVersionManager.exe -GenerateVC \"c:\\projects\\vc.xml\"");
            }
        }

        private static VersioningAction GetAction(string[] args)
        {
            return VersioningAction.Patch;
        }
    }
}
