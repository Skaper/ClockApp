using System;

namespace ClockApp.Application.Integration
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
        public string Version { get; }
        public DateTime? IntroducedIn { get; set; }
        public string DeprecatedIn { get; set; }
        public string RemovedIn { get; set; }
        
        public ApiVersionAttribute(string version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }
    }
    
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ApiCompatibilityAttribute : Attribute
    {
        public ApiCompatibilityLevel Level { get; }
        public string Notes { get; set; }
        
        public ApiCompatibilityAttribute(ApiCompatibilityLevel level)
        {
            Level = level;
        }
    }
    
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class ApiDeprecatedAttribute : Attribute
    {
        public string DeprecatedInVersion { get; }
        public string RemovedInVersion { get; set; }
        public string AlternativeApi { get; set; }
        public string Reason { get; set; }
        
        public ApiDeprecatedAttribute(string deprecatedInVersion)
        {
            DeprecatedInVersion = deprecatedInVersion ?? throw new ArgumentNullException(nameof(deprecatedInVersion));
        }
    }
    
    public enum ApiCompatibilityLevel
    {
        Experimental,
        Beta,
        Stable,
        Deprecated
    }
}