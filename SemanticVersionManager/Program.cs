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

        public static void Main(string[] args)
        {
            var argsParser = new ArgumentsParser("-");

            foreach (var pair in argsParser.Parse(args))
            {
                Console.WriteLine($"ArgName: {pair.Key} \t\tArgValue: {pair.Value?.Aggregate(string.Empty, (s, s1) => $"{s}{(string.IsNullOrWhiteSpace(s) ? string.Empty : ",")}{s1}")}");
            }
            Console.ReadLine();
        }

        private static VersionAction GetAction(string[] args)
        {
            return VersionAction.Version;
        }
    }
}
