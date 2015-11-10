using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticVersionManager
{
    public class Program
    {
        private List<string> invokeCommandErrors = new List<string>();

        /// <summary>
        /// Invoke with arguments
        /// <para>VersionControlPath: path to version control xml file with the definition.</para>
        /// <para>BuildDefinitionName: name of BuildDefinition to look for in VersionControl.xml</para>
        /// <para>VersioningAction Patch (Default) | Promote -sourceDefinition -destDefinition | SetNewVersion (reset build & revision) -Major -Minor -Patch</para>
        /// <para>GenerateVersionControlFile: path to where generate a version control xml file with the definitions.</para>
        /// </summary>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            var argsParser = new ArgumentsParser("-");
            var arguments = argsParser.Parse(args);

            if (arguments.ContainsKey("GenerateVC") && arguments.Count > 1)
            {
                PrintHelp("GenerateVC");
                Console.ReadLine();
                return 1;
            }

            foreach (var pair in argsParser.Parse(args))
            {
                Console.WriteLine($"ArgName: {pair.Key} \t\tArgValue: {pair.Value?.Aggregate(string.Empty, (s, s1) => $"{s}{(string.IsNullOrWhiteSpace(s) ? string.Empty : ",")}{s1}")}");
            }
            Console.ReadLine();
            return 0;
        }

        private static void PrintHelp(string helpPart)
        {
            if (helpPart.ToLower() == "generatevc")
            {
                Console.WriteLine("-GenerateVC");
                Console.WriteLine("This argument can be used to generate a sample xml with build definitions.");
                Console.WriteLine("Can provide a full path (including file name) where generate the xml,\notherwise the local folder is used with filename VersionControl.xml.");
                Console.WriteLine("If this argument is used then no other arguments can be specified.");
                Console.WriteLine("i.e.:\n\tSemanticVersionManager.exe -GenerateVC \"c:\\projects\\vc.xml\"");
            }
        }

        private static VersioningAction GetAction(string[] args)
        {
            return VersioningAction.Patch;
        }
    }
}
