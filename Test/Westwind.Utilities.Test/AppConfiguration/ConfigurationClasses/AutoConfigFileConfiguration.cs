using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Utilities.Configuration.Tests
{
    /// <summary>
    /// Default implementation that uses only base constructors
    /// for configuration.
    /// 
    /// Default setup allows for no configuration of the provider
    /// since we're just calling back to the base constructors
    /// 
    /// Note: for config files ONLY you can implement the default 
    /// constructor automatically since no serialization is used.
    /// When using XML, String, Database the default constructor 
    /// needs to be left at default to avoid recursive loading
    /// </summary>
    class AutoConfigFileConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {

        public AutoConfigFileConfiguration()
        { }

        public AutoConfigFileConfiguration(IConfigurationProvider provider,string section = "AutoConfigFileConfiguration")
            :base(provider, section)
        { }

        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }

        protected override void Initialize()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.Default;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
        }
    }


    class AutoConfigFile2Configuration : AppConfiguration
    {

        public AutoConfigFile2Configuration()
            : base(null,"AutoConfigFile2Configuration")
        { }

        public string ApplicationName { get; set; }
        public DebugModes DebugMode { get; set; }
        public int MaxDisplayListItems { get; set; }
        public bool SendAdminEmailConfirmations { get; set; }

        protected override void Initialize()
        {
            ApplicationName = "Configuration Tests";
            DebugMode = DebugModes.ApplicationErrorMessage;
            MaxDisplayListItems = 15;
            SendAdminEmailConfirmations = false;
        }
    }

}
