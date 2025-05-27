using System;
using UnityEditor;
using UnityEngine.Scripting;

namespace ClockApp.Application.Integration
{
    public class ApiVersion
    {
        public const string Current = "1.0.0";
        
        public const int Major = 1;
        public const int Minor = 0;
        public const int Patch = 0;
        
        public const string MinimumSupported = "1.0.0";
        
        public static readonly DateTime ReleaseDate = new DateTime(2025, 6, 1);
        
        public static bool IsCompatible(string requiredVersion)
        {
            if (string.IsNullOrEmpty(requiredVersion))
                return false;
                
            try
            {
                var required = ParseVersion(requiredVersion);
                var current = ParseVersion(Current);
                
                if (required.Major != current.Major)
                    return false;
                
                return current.Minor > required.Minor || 
                       (current.Minor == required.Minor && current.Patch >= required.Patch);
            }
            catch
            {
                return false;
            }
        }
        
        public static string GetVersionInfo()
        {
            return $"ClockApp API v{Current} - Released {ReleaseDate:yyyy-MM-dd}";
        }
        
        private static (int Major, int Minor, int Patch) ParseVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid version format");
                
            return (
                int.Parse(parts[0]),
                int.Parse(parts[1]), 
                int.Parse(parts[2])
            );
        }
    }
}