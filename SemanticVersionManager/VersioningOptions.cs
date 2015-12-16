using System;
using System.Collections.Generic;
using System.Linq;
using SemanticVersionManager.Exceptions;

namespace SemanticVersionManager
{
    public class VersioningOptions
    {
        private const string Default = "default";

        public string FileName { get; set; }

        public Definition Target { get; set; }

        public Definition Destination { get; set; }

        public VersioningAction Action { get; private set; } = VersioningAction.Patch;

        public VersionNumbers Numbers { get; set; } = new VersionNumbers() { Major = "0", Minor = "0", Patch = "0" };

        public VersioningOptions(Dictionary<string, List<string>> arguments)
        {
            FileName = arguments[Parameters.VCPath.TL()].First();
            Target = new Definition
            {
                Name = (arguments.ContainsKey(Parameters.Definition.TL()) && arguments[Parameters.Definition.TL()].Any())
                    ? arguments[Parameters.Definition.TL()].First()
                    : Default,
                Build = (arguments.ContainsKey(Parameters.BuildName.TL()) && arguments[Parameters.BuildName.TL()].Any())
                    ? arguments[Parameters.BuildName.TL()].First()
                    : Default
            };

            ParseAction(arguments);
            ValidateAction(arguments);
            ValidateOptions();
        }

        private void ValidateOptions()
        {
            switch (Action)
            {
                case VersioningAction.Patch:
                    break;
                case VersioningAction.Promote:
                    if (string.Equals(Destination.Name, Target.Name, StringComparison.InvariantCultureIgnoreCase)
                        && string.Equals(Destination.Build, Target.Build, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ProcessCommandException("The destination definition can't be the same in a promote operation.");
                    }
                    break;
                case VersioningAction.SetNewVersion:
                    break;
                case VersioningAction.ReBuild:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ParseAction(Dictionary<string, List<string>> arguments)
        {
            if (arguments.ContainsKey(Parameters.Action.TL()))
            {
                var argAction = arguments[Parameters.Action.TL()].First();
                Action = (VersioningAction) Enum.Parse(typeof (VersioningAction), argAction, true);
            }
        }

        private void ValidateAction(Dictionary<string, List<string>> arguments)
        {
            switch (Action)
            {
                case VersioningAction.Promote:
                    Destination = new Definition
                    {
                        Name = arguments[Parameters.DestinationDefinition.TL()].First(), Build = arguments[Parameters.DestinationBuild.TL()].First()
                    };
                    // validate that destination has a version lower than source
                    break;
                case VersioningAction.SetNewVersion:
                    Numbers.Major = arguments[Parameters.Major.TL()].First();
                    Numbers.Minor = arguments[Parameters.Minor.TL()].First();
                    Numbers.Patch = arguments[Parameters.Patch.TL()].First();
                    break;
            }
        }
    }

    public class Definition
    {
        public string Name { get; set; }

        public string Build { get; set; }
    }
}