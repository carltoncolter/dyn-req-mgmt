using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xrm.Sdk;
using Plugins.Common;
using Plugins.Common.Attributes;

namespace Plugins.Entities.account
{
    // TODO: Replace with first needed plugin.  Just starter code so there is a testable plugin in the template

    /// <summary>
    /// The sample testable plugin.
    /// Replace with first needed plugin.  Just starter code so there is a testable plugin in the template
    /// </summary>
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "Create",
        "account", StageEnum.PreOperation, ExecutionModeEnum.Synchronous,
        "", "PreCreate Account", 1, IsolationModeEnum.Sandbox,
        Id = "81d929f2-83f4-e711-a94b-000d3a36478d")]
    [Serializable]
    public class PreCreateSample : PluginBase
    {
        /*
         * Override AutoLoadConfig to false if you do not want to autoload the config 
         * Config Files are built in to the base class to handle both json and xml config files.
         * 
         * Constructors should be left empty. Any initialization code should be put into InitializePlugin
         */

        #region constructors

        [ExcludeFromCodeCoverage]
        public PreCreateSample() { }

        [ExcludeFromCodeCoverage]
        public PreCreateSample(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        #region PluginSettings
        public string ODataEndpoint { get; set; }

        public IWebClient WebClient { get; set; }
        #endregion

        /// <inheritdoc />
        /// <remarks>
        /// The <c>InitializePlugin</c> method is executed whenever a new plugin object is created.
        /// </remarks>
        public override void InitializePlugin()
        {
            // TODO: Add initialization work here or remove method
        }

        public override void Execute(ILocalPluginContext context)
        {
            // TODO: Write plug-in code here.
            // TODO: Remove sample code.
            #region This is sample code that should be removed.
            
            // Get ODataEndpoint
            ODataEndpoint = ConfigString;

            // Get WebClient
            if (WebClient == null)
            {
                WebClient = WebClientFactory.Create();
            }

            if (!string.IsNullOrEmpty(ODataEndpoint))
            {
                try
                {
                    var odataQuery = $"{ODataEndpoint}/TripPinRESTierService/Airports('KSFO')/Name/$value";
                    WebClient.DownloadData(new Uri(odataQuery));
                }
                catch (Exception e)
                {
                    Dispose();
                    throw new InvalidPluginExecutionException(e.Message);
                }
            }
            else
            {
                Dispose();
                throw new InvalidPluginExecutionException("No Secure Configuration");
            }

            Dispose();

            #endregion
        }

        /// <summary>
        /// The dispose, clears the webclient, forcing it to be set for the next execute even if still in memory.
        /// </summary>
        private void Dispose()
        {
            WebClient = null;
        }
    }
}