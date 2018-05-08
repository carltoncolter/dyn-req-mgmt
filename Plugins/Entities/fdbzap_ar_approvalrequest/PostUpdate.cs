using System;
using System.Diagnostics.CodeAnalysis;
using Plugins.Common;
using Plugins.Common.Attributes;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins.Entities.fdbzap_ar_approvalrequest
{
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "Update",
        "fdbzap_ar_approvalrequest",
        StageEnum.PostOperation,
        ExecutionModeEnum.Asynchronous,
        "",
        "Post Update Approval Request",
        1,
        IsolationModeEnum.Sandbox)]
    [Serializable]
    public class PostUpdate : PluginBase
    {

        #region constructors

        [ExcludeFromCodeCoverage]
        public PostUpdate() { }

        [ExcludeFromCodeCoverage]
        public PostUpdate(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        #region PluginSettings

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
            // Step 1: Get the action taken
            context.Trace($"Depth: {context.PluginExecutionContext.Depth}");
            if (context.PluginExecutionContext.Depth > 1) return;

            context.Trace("Step 1");
            var action = context.GetAttributeValue<EntityReference>("fdbzap_ar_actiontaken");

            if (action == null) return;

            // Step 2: Set action taken statuscode to run - 2
            context.Trace("Step 2");
            var actionEntity = new Entity(action.LogicalName);
            actionEntity.Id = action.Id;
            actionEntity["statuscode"] = new OptionSetValue(2);
            context.OrganizationService.Update(actionEntity);
        }
    }
}