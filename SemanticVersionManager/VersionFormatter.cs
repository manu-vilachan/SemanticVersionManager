﻿namespace SemanticVersionManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VersionFormatter
    {
        private readonly Regex variableRegex = new Regex(@"(?<=\{)[^}]+(?=\})");

        private readonly Regex optionalPartRegex = new Regex(@"(?<=\[)[^]]+(?=\])");
        
        private const string defaultVersionPartValue = "0";

        /// <summary>Gets the date passed in a Julian format like YYYYDY (full year and the day of year from 1 to 366).<para>If the date is null or not provided the actual date is used.</para></summary>
        /// <param name="date">The date to be converted to Julian format.</param>
        /// <returns>A string with the Julian format.</returns>
        public string GetJulianFormat(DateTime? date = null)
        {
            date = date ?? DateTime.Now;
            var julian = date.Value.Year.ToString() + date.Value.DayOfYear.ToString().PadLeft(3, '0');
            return julian;
        }

        /// <summary>Returns the value passed incremented in one as string.<para>If the value is null, empty or a whitespace the method returns 0.</para></summary>
        /// <param name="value">Value to increment. Should be a valid non negative integer.</param>
        /// <returns>Zero or the value passed incremented in one</returns>
        /// <exception cref="FormatException">When the value passed cannot be converted to a valid integer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the value passed is a valid integer but it is less than zero.</exception>
        public string GetAutoIncremented(string value = null)
        {
            int finalValue;

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultVersionPartValue;
            }

            if (!int.TryParse(value, out finalValue))
            {
                throw new FormatException($"The argument with value [{value}] cannot be converted to a valid integer.");
            }

            if (finalValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"The argument with value [{value}] it's out of valid range, only positive integers and zero are accepted.");
            }

            if (finalValue == int.MaxValue)
            {
                return defaultVersionPartValue;
            }

            finalValue++;
            return finalValue.ToString();
        }

        /// <summary>Normalize the value passed when comes from TFS build number, that always starts from 1.</summary>
        /// <param name="tfsBuildNumber">The value generated by TFS build service.</param>
        /// <returns>A string with a corrected value starting from 0.</returns>
        /// <exception cref="FormatException">When the tfsBuildNumber passed cannot be converted to a valid integer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the tfsBuildNumber passed is a valid integer but it is less than zero.</exception>
        public string NormalizeTFSBuildNumber(string tfsBuildNumber = null)
        {
            int finalValue;

            if (string.IsNullOrWhiteSpace(tfsBuildNumber))
            {
                return defaultVersionPartValue;
            }

            if (!int.TryParse(tfsBuildNumber, out finalValue))
            {
                throw new FormatException($"The argument with value [{tfsBuildNumber}] cannot be converted to a valid integer.");
            }

            if (finalValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tfsBuildNumber), $"The argument with value [{tfsBuildNumber}] it's out of valid range, only positive integers and zero are accepted.");
            }

            if (finalValue > 0)
            {
                finalValue--;
            }

            return finalValue.ToString();
        }

        /// <summary>Apply the values provided to the input pattern.</summary>
        /// <param name="pattern">Pattern with the desired format. i.e. {MAJOR}.{MINOR}.{PATCH}[.{BUILD}].</param>
        /// <param name="values">Dictionary of names and values to apply.</param>
        /// <returns>The well formed version number.</returns>
        /// <exception cref="ArgumentNullException">If the dictionary of values is null or contains no items.</exception>
        /// <exception cref="ArgumentException">When the pattern cannot be identified as a valid version pattern or when the number of values to replace is less than mandatory values or not all values are passed.</exception>
        public string GetVersionFormatted(string pattern, Dictionary<string, string> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var mandatoryVars = new List<string>();

            var optionalParts = optionalPartRegex.Matches(pattern).Cast<Match>().Select(m => m.Value).ToList();
            var mandatoryParts = optionalPartRegex.Split(pattern).ToList().Select(m => m.Replace("[", string.Empty).Replace("]", string.Empty)).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (mandatoryParts.Any(s => variableRegex.IsMatch(s)))
            {
                mandatoryVars.AddRange(mandatoryParts.SelectMany(part => variableRegex.Matches(part).Cast<Match>()).Select(match => match.Value));
            }

            if (mandatoryVars.Count == 0 && optionalParts.Count == 0)
            {
                throw new ArgumentException("The pattern seems not be valid because any variable has been found to replace", nameof(pattern));
            }

            if (values.Count < mandatoryVars.Count || !mandatoryVars.All(values.ContainsKey))
            {
                var mandatoryVarList = mandatoryVars.Aggregate(string.Empty, (s1, s2) => $"{s1}{(string.IsNullOrWhiteSpace(s1) ? string.Empty : ", ")}{s2}");
                var valuesList = values.Select(k => k.Key).Aggregate(string.Empty, (s1, s2) => $"{s1}{(string.IsNullOrWhiteSpace(s1) ? string.Empty : ", ")}{s2}");

                throw new ArgumentException($"The number of values is less than specified in the pattern or not all values has been provided.\nMandatory variables found: {mandatoryVarList}\nValues provided: {valuesList}", nameof(values));
            }

            var mandatoryPartsReplaced = ReplaceParts(values, mandatoryParts);
            var optionalPartsReplaced = ReplaceParts(values, optionalParts);

            var versionFormatted = pattern;
            mandatoryPartsReplaced.ForEach(p => versionFormatted = versionFormatted.Replace(p.Key, p.Value));
            optionalPartsReplaced.ForEach(p => versionFormatted = versionFormatted.Replace($"[{p.Key}]", p.Value));

            versionFormatted = RemoveNotInformedVars(versionFormatted);

            return versionFormatted;
        }

        private string RemoveNotInformedVars(string versionFormatted)
        {
            var notInformedVars = variableRegex.Matches(versionFormatted);
            foreach (Match m in notInformedVars)
            {
                var replacement = $"{{{m.Value}}}";
                var index = versionFormatted.IndexOf(replacement, StringComparison.InvariantCultureIgnoreCase);
                if (versionFormatted[index - 1] == '.' || versionFormatted[index - 1] == '-')
                {
                    replacement = versionFormatted[index - 1] + replacement;
                }
                versionFormatted = versionFormatted.Replace(replacement, string.Empty);
            }
            return versionFormatted;
        }

        private List<KeyValuePair<string, string>> ReplaceParts(Dictionary<string, string> values, List<string> patterns)
        {
            var partsReplaced = patterns.Select(
                p =>
                    {
                        var original = p;
                        values.ToList().ForEach(v => p = p.Replace($"{{{v.Key}}}", v.Value));
                        return new KeyValuePair<string, string>(original, p);
                    }).ToList();
            return partsReplaced;
        }
    }
}