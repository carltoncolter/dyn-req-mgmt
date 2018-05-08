using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Microsoft.Uii.Common.Entities;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageCore;

namespace FedBizApps.RequestManagement.DeploymentPackage
{
    /// <inheritdoc />
    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension
    {
        private const string Seperator = "------------------------------------------------------";

        private void LogStart(string name)
        {
            PackageLog.Log(Seperator, TraceEventType.Information);
            PackageLog.Log($"{name.ToUpper()} STARTING");
        }

        private void LogEnd(string name)
        {
            PackageLog.Log($"{name.ToUpper()} FINISHED");
            PackageLog.Log(Seperator, TraceEventType.Information);
        }

        /// <summary>
        /// Called Before Import Completes.
        /// </summary>
        /// <returns></returns>
        public override bool BeforeImportStage()
        {
            return true; // do nothing here.
        }

        /// <summary>
        /// Called for each UII record imported into the system
        /// This is UII Specific and is not generally used by Package Developers
        /// </summary>
        /// <param name="app">App Record</param>
        /// <returns></returns>
        public override ApplicationRecord BeforeApplicationRecordImport(ApplicationRecord app)
        {
            return app;  // do nothing here.
        }

        /// <summary>
        /// Called during a solution upgrade while both solutions are present in the target CRM instance.
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring.
        /// </summary>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }

        /// <summary>
        /// Called after Import completes.
        /// </summary>
        /// <returns></returns>
        public override bool AfterPrimaryImport()
        {
            LogStart("AfterPrimaryImport");
            PackageLog.Log("Running AfterPrimaryImport");
            // Verify Connection
            if (CrmSvc == null || !CrmSvc.IsReady)
            {
                PackageLog.Log("Error: Not connected to crm service.", TraceEventType.Critical);
                LogEnd("AfterPrimaryImport");
                return false;
            }

            // Log Version
            var version = CrmSvc.ConnectedOrgVersion;
            PackageLog.Log($"Connected CRM Version: {version}", TraceEventType.Information);

            // Queue Items
            QueueItems.AddToQueue(PackageLog, CrmSvc, GetImportPackageDataFolderPath("queueitems.json"));

            // Enable all the steps in the plugin assembly
            EnablePluginSteps();

            LogEnd("AfterPrimaryImport");
            return true;
        }

        public override void InitializeCustomExtension()
        {
            // Do Nothing
#if DEBUG
            DataImportBypass = true; // when developing in debug mode, skip data imports so dev/test is easier
#endif
        }

        private void EnablePluginSteps()
        {
            LogStart("EnablePluginSteps");

            var assemblyname = "FedBizApps.RequestManagement.Plugins";

            // Create the QueryExpression object to retrieve plug-in type
            var query = new QueryExpression { EntityName = "plugintype" };
            query.Criteria.AddCondition("assemblyname", ConditionOperator.Equal, assemblyname);
            var retrievedPluginType = CrmSvc.RetrieveMultiple(query)[0];

            var pluginTypeId = (Guid)retrievedPluginType.Attributes["plugintypeid"];

            query = new QueryExpression
            {
                EntityName = "sdkmessageprocessingstep",
                ColumnSet = new ColumnSet(new[] { "sdkmessageprocessingstepid", "statecode" })
            };

            // Set the properties of the QueryExpression object.
            query.Criteria.AddCondition(new ConditionExpression("plugintypeid", ConditionOperator.Equal, pluginTypeId));
            var retrievedSteps = CrmSvc.RetrieveMultiple(query);

            foreach (var step in retrievedSteps.Entities)
            {
                // Enable the step by setting it's state code
                step.Attributes["statecode"] = new OptionSetValue(0); // 0 = Enabled
                step.Attributes["statuscode"] = new OptionSetValue(1); // 1 = Enabled
                CrmSvc.Update(step);
            }

            LogEnd("EnablePluginSteps");
        }

        /// <summary>
        /// Path to the folder for the Package data.
        /// </summary>
        /// <param name="childPath">A child path to a file or folder in the PkgFolder.</param>
        /// <returns>The combined path.</returns>
        public string GetImportPackageDataFolderPath(string childPath = "") =>
            // Core.CoreData.SelectedPluginPath is where the PackageDeployer stores the PackageDirectory
            Path.Combine(Core.CoreData.SelectedPluginPath, GetImportPackageDataFolderName, childPath);

        #region Properties

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        /// <returns></returns>
        public override string GetNameOfImport(bool plural) => "Request Management";

        /// <summary>
        /// Folder Name for the Package data.
        /// </summary>
        /// <remarks>
        /// WARNING this value directly correlates to the folder name in the Solution Explorer where the ImportConfig.xml and sub content is located.
        /// Changing this name requires that you also change the correlating name in the Solution Explorer
        /// </remarks>
        public override string GetImportPackageDataFolderName => "PkgFolder";

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText => "The Request Management Solution Package";

        /// <summary>
        /// Long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport => "The Request Management Solution Package";

        #endregion Properties
    }
}