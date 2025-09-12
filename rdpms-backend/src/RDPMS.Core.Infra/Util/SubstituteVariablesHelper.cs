using System.Text.RegularExpressions;
using RDPMS.Core.Infra.Exceptions;

namespace RDPMS.Core.Infra.Util;

public static class SubstituteVariablesHelper
{
    private static readonly Regex VariableRegex = new(@"\$\{(?<scope>\w+)\:(?<variable_name>\w+)\}",
        RegexOptions.Compiled |
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Singleline);

    private static string GetSpecialFolder(string scope)
    {
        var folder = scope switch
        {
            "MyDocuments" => Environment.SpecialFolder.MyDocuments,
            "LocalApplicationData" => Environment.SpecialFolder.LocalApplicationData,
            "ApplicationData" => Environment.SpecialFolder.ApplicationData,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };

        return Environment.GetFolderPath(folder);
    }

    public static string SubstituteVariables(string value)
    {
        foreach (Match match in VariableRegex.Matches(value))
        {
            var replacement = match.Groups["scope"].Value switch
            {
                "special" => GetSpecialFolder(match.Groups["variable_name"].Value),
                "date" => DateTime.Now.ToString(match.Groups["variable_name"].Value),
                _ => throw new IllegalArgumentException($"scope unknown: {match.Groups["scope"].Value}")
            };
            value = value.Replace(match.Value, replacement);
        }
        
        return value;
    }
}