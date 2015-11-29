using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using SemanticVersionManager.Exceptions;

namespace SemanticVersionManager
{

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
            var returnCode = 0;

            try
            {
                switch (InferCommandFromArgs())
                {
                    case Commands.GenerateVC:
                        ValidateFileGenerationArguments();
                        GenerateVCFile();
                        break;
                    case Commands.DoVersioning:
                        ValidateDoVersioningArguments();
                        DoVersioning();
                        break;
                    default:
                        PrintHelp(Commands.NotRecognized);
                        break;
                }
            }
            catch (WrongArgumentsException ex)
            {
                PrintHelp(ex.Command);
                returnCode = ex.HResult;
            }
            catch (ProcessCommandException ex)
            {
                Console.WriteLine($"Something went wrong.\n{ex.Message}");
                returnCode = ex.HResult;
            }

            return returnCode;
        }

        private static string InferCommandFromArgs()
        {
            if (Arguments.ContainsKey(Parameters.GenerateVC.TL()))
            {
                return Commands.GenerateVC;
            }

            return Commands.DoVersioning;
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

        private static void ValidateDoVersioningArguments()
        {
            var valid = !Arguments.ContainsKey(Parameters.VCPath.TL()) || !Arguments[Parameters.VCPath.TL()].Any();

            if (!File.Exists(Arguments[Parameters.VCPath.TL()].First()))
            {
                throw new FileNotFoundException($"File not found:{Arguments[Parameters.VCPath].First()}", Arguments[Parameters.VCPath].First());
            }

            valid = valid || !ValidateAction();

            if (!valid)
            {
                throw new WrongArgumentsException {Command = Commands.DoVersioning};
            }
        }

        private static void ValidateFileGenerationArguments()
        {
            if (Arguments.ContainsKey(Parameters.GenerateVC) && Arguments.Count > 1)
            {
                throw new WrongArgumentsException {Command = Commands.GenerateVC};
            }
        }

        private static bool ValidInt(string number)
        {
            int notUsed;
            return int.TryParse(number, out notUsed);
        }

        private static void DoVersioning()
        {
            var vOptions = new VersioningOptions(Arguments);

            var xml = XDocument.Load(vOptions.FileName);
            var element = xml.XPathSelectElement($"//Definitions/Definition[@type=\"{vOptions.Target.Name}\"]");

            switch (vOptions.Action)
            {
                case VersioningAction.SetNewVersion:
                    SetNewVersion(ref element, vOptions);
                    break;
                case VersioningAction.Promote:
                    var targetElement = xml.XPathSelectElement($"//Definitions/Definition[@type=\"{vOptions.Destination.Name}\"]");
                    PromoteVersion(ref element, targetElement, vOptions);
                    break;
                case VersioningAction.Patch:
                    DoPatching(ref element, vOptions.Target.Build);
                    break;
            }

            xml.Save(vOptions.FileName);
        }

        private static void PromoteVersion(ref XElement sourceElement, XElement targetElement, VersioningOptions versioningOptions)
        {
            var common = sourceElement.Element(XmlConstants.CommonVersion);
            var major = common.Element(XmlConstants.Major).Value;
            var minor = common.Element(XmlConstants.Minor).Value;
            var patch = common.Element(XmlConstants.Patch).Value;

            common = targetElement.Element(XmlConstants.CommonVersion);
            common.Element(XmlConstants.Major).Value = major;
            common.Element(XmlConstants.Minor).Value = minor;
            common.Element(XmlConstants.Patch).Value = patch;

            var buildValues = targetElement.Elements(Parameters.BuildName)
                .Where(xe => xe.Attribute("name").Value == versioningOptions.Destination.Build)
                .Descendants()
                .Where(xe => xe.Name == Parameters.BuildName || xe.Name == Parameters.Revision)
                .ToList();
            buildValues.ForEach(x => x.Value = "0");
        }

        private static void SetNewVersion(ref XElement element, VersioningOptions versioningOptions)
        {
            var common = element.Element(XmlConstants.CommonVersion);
            common.Element(XmlConstants.Major).Value = versioningOptions.Numbers.Major;
            common.Element(XmlConstants.Minor).Value = versioningOptions.Numbers.Minor;
            common.Element(XmlConstants.Patch).Value = versioningOptions.Numbers.Patch;

            var toReset = element.Descendants()
                .SelectMany(xe => xe.Descendants())
                .Where(xe => xe.Name == Parameters.Build || xe.Name == Parameters.Revision)
                .ToList();
            toReset.ForEach(x => x.Value = "0");
        }

        private static void DoPatching(ref XElement element, string buildName)
        {
            VersionProcessDefinition versionDefinition = new VersionProcessDefinition();
            var formatter = new VersionFormatter();

            versionDefinition.Read(element, buildName);

            versionDefinition.DoIncrements(ProcessIncrementMethod, Arguments, formatter);

            versionDefinition.ApplyPatterns(formatter.GetVersionFormatted);

            versionDefinition.Write(ref element, buildName);
        }

        private static string ProcessIncrementMethod(VersionFormatter formatter, string number, IncrementMethod incrementMethod, string settedValue = null)
        {
            switch (incrementMethod)
            {
                case IncrementMethod.Setted:
                    if (string.IsNullOrWhiteSpace(settedValue))
                    {
                        throw new ArgumentNullException(nameof(settedValue), $"The value for variable {{{number}}} was not setted.");
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

        private static void GenerateVCFile()
        {
            var fileName = Arguments[Parameters.GenerateVC].Any()
                ? Arguments[Parameters.GenerateVC].First()
                : "VersioningControl.xml";

            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SemanticVersionManager.Resources.VersioningControl.xml");
            if (stream == null)
            {
                throw new MissingManifestResourceException("A versioning control file cannot be created. The embedded resource is not found.");
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
                throw new ProcessCommandException("A versioning control file cannot be created by unauthorized access.", ex);
            }
            catch (IOException ex)
            {
                throw new ProcessCommandException($"A versioning control file cannot be created by an unknown error.\n{ex.Message}", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessCommandException($"A versioning control file cannot be created.\n{ex.Message}", ex);
            }
            finally
            {
                file?.Close();
                stream?.Close();
            }
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
    }
}
