namespace SemanticVersionManager
{
    using System;
    using System.Collections.Generic;

    public class ArgumentsParser
    {
        private readonly string argDelimitier;

        private readonly string notNamedArgKey = "NotNamedArg";

        private readonly bool ignoreCase = false;

        public ArgumentsParser(string argumentDelimitier, bool ignoreKeysCase = false)
        {
            argDelimitier = argumentDelimitier;
            ignoreCase = ignoreKeysCase;
            if (ignoreCase)
            {
                notNamedArgKey = notNamedArgKey.ToLower();
            }
        }

        public Dictionary<string, List<string>> Parse(string[] args, int? minArgsExpected = null, int? maxArgsExpected = null)
        {
            bool prevStartsWithDelim = false;
            var parsedArguments = new Dictionary<string, List<string>>();

            for (int i = 0; i < args.Length; i++)
            {
                var startsWithDelim = args[i].StartsWith(this.argDelimitier);
                if (!startsWithDelim && !prevStartsWithDelim)
                {
                    if (!parsedArguments.ContainsKey(notNamedArgKey))
                    {
                        parsedArguments.Add(notNamedArgKey, new List<string> { args[i] });
                    }
                    else
                    {
                        parsedArguments[notNamedArgKey].Add(args[i]);
                    }
                }

                if (startsWithDelim)
                {
                    prevStartsWithDelim = true;
                    var argPassBy = 0;
                    var value = string.Empty;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith(this.argDelimitier))
                    {
                        value = args[i + 1];
                        argPassBy = 1;
                    }

                    // if the value is empty then it's a simple argument without value
                    var key = args[i].Replace(this.argDelimitier, string.Empty);
                    if (ignoreCase)
                    {
                        key = key.ToLower();
                    }

                    if (!parsedArguments.ContainsKey(key))
                    {
                        parsedArguments.Add(key, !string.IsNullOrWhiteSpace(value) ? new List<string> { value } : null);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            parsedArguments[key].Add(value); 
                        }
                    }
                    i = i + argPassBy;
                }
            }

            if (minArgsExpected.HasValue && parsedArguments.Count < minArgsExpected)
            {
                throw new ArgumentOutOfRangeException(nameof(args), $"The number of arguments is less than expected.\nThere are {parsedArguments.Count} when expected {minArgsExpected}.");
            }

            if (maxArgsExpected.HasValue && parsedArguments.Count > maxArgsExpected)
            {
                throw new ArgumentOutOfRangeException(nameof(args), $"The number of arguments is less than expected.\nThere are {parsedArguments.Count} when expected {maxArgsExpected}.");
            }

            return parsedArguments;
        }
    }
}