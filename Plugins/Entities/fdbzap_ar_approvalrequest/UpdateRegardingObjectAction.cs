// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Update the regarding object according to the configuration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugins.Entities.fdbzap_ar_approvalrequest
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Common;
    using Common.Attributes;
    using Common.Constants;

    /// <summary>
    /// The update approval request regarding object action plugin.
    /// </summary>
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "fdbzap_ar_update_regardingobject",
        "fdbzap_ar_approvalrequest",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous,
        "",
        "Post Update Approval Request Regarding Object",
        1,
        IsolationModeEnum.Sandbox,
        Id = "11c8414d-c548-e811-a94d-000d3a3669fe")]
    [Serializable]
    public class UpdateRegardingObjectAction : PluginBase
    {

        #region constructors

        [ExcludeFromCodeCoverage]
        public UpdateRegardingObjectAction() { }

        [ExcludeFromCodeCoverage]
        public UpdateRegardingObjectAction(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        /// <summary>
        /// Execute the plugin action
        /// </summary>
        /// <param name="context">
        /// The local plugin context.
        /// </param>
        public override void Execute(ILocalPluginContext context)
        {
            // Product Backlog Item 139: Corerspondence Management
            // The plugin should verify that all child approvals are approved 
            // or rejected to meet the minimum requirements, if it is and the 
            // regarding object id is set, then it updates the current approval
            // request and runs the "fdbzap_ar_update_regardingobject" action on 
            // the current approval request otherwise it runs the "fdbzap_ar_checkstatus" 
            // action on the parent approval request.

            // This process is launched automatically by the fdbzap_ar_action PostUpdate.

            // Step 1: Get Regarding Object Config
            context.Trace($"Depth: {context.PluginExecutionContext.Depth}");
            context.Trace("Step 1");

            var appreqRef = context.GetInputParameter<EntityReference>("Target");
            if (appreqRef == null) return;

            var approval = context.OrganizationService.Retrieve(appreqRef.LogicalName, appreqRef.Id,
                new ColumnSet("fdbzap_ar_parentid", "fdbzap_ar_actiontakenon",
                    "fdbzap_ar_actiontaken", "fdbzap_ar_regardingobjectid", "fdbzap_ar_regardinggobjectentityname"));

            if (approval == null)
            {
                context.Trace("No Approval Request!");
                return;
            }

            // Step 2
            context.Trace("Step 2");
            var config = GetRegardingObjectConfig(context);
            if (config == null)
            {
                // nothing to do... no config
                context.Trace("Config not found.");
                return;
            }

            // Step 3: Get Regarding Object
            context.Trace("Step 3");
            var regarding = GetRegardingObject(approval);

            // Step 3: Update Regarding Object with changes.
            context.Trace("Step 3");
            UpdateRegardingObject(context, config, regarding, approval);
        }

        private static void UpdateRegardingObject(ILocalPluginContext context, Entity config, EntityReference regarding, Entity approval)
        {
            
            var isChildApprovalRequest = approval.GetAttributeValue<EntityReference>("fdbzap_ar_parentid") != null;
            var entityType = config.GetAttributeValue<string>("fdbzap_ar_entitytype");

            // validate entity type if specified
            if (!string.IsNullOrEmpty(entityType) && !entityType.Equals(regarding.LogicalName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(ResponseMessages.RegardingConfigMismatch);
            }

            Entity target = new Entity(regarding.LogicalName), action = null;
            target.Id = regarding.Id;

            var lastaction = approval.GetAttributeValue<DateTime>("fdbzap_ar_actiontakenon");
            var actionRef = approval.GetAttributeValue<EntityReference>("fdbzap_ar_actiontaken");

            if (actionRef != null)
            {
                action = context.OrganizationService.Retrieve(actionRef.LogicalName, actionRef.Id, new ColumnSet("fdbzap_ar_actiontype", "fdbzap_ar_name"));
            }

            var completedon = config.GetAttributeValue<string>("fdbzap_ar_completedon_fieldname");
            if (!isChildApprovalRequest && !string.IsNullOrEmpty(completedon) && action!=null)
            {
                var actionType = action.GetAttributeValue<OptionSetValue>("fdbzap_ar_actiontype");
                if (actionType != null)
                {
                    var aval = actionType.Value - 741210000 /* solution prefix */;
                    if (aval == 0 || aval == 2 || aval == 4) /* Approve, Concur, Acknowledge */
                    {
                        target[completedon] = lastaction;
                    }
                }
            }

            var modifiedon = config.GetAttributeValue<string>("fdbzap_ar_lastupdatedon_fieldname");
            if (!string.IsNullOrEmpty(modifiedon))
            {
                target[modifiedon] = lastaction;
            }

            var statusfield = config.GetAttributeValue<string>("fdbzap_ar_status_fieldname");
            if (!isChildApprovalRequest && !string.IsNullOrEmpty(statusfield) && action != null)
            {
                var actionName = action.GetAttributeValue<string>("fdbzap_ar_name");
                target[statusfield] = actionName;
            }

            context.OrganizationService.Update(target);
        }

        private EntityReference GetRegardingObject(Entity approval)
        {
            var idStr = approval.GetAttributeValue<string>("fdbzap_ar_regardingobjectid");
            var entityName = approval.GetAttributeValue<string>("fdbzap_ar_regardinggobjectentityname");

            if (String.IsNullOrWhiteSpace(idStr) || String.IsNullOrWhiteSpace(entityName))
            {
                return null;
            }

            if (!Guid.TryParse(idStr, out var id) || id == Guid.Empty)
            {
                return null;
            }

            return new EntityReference(entityName, id);
        }

        private Entity GetRegardingObjectConfig(ILocalPluginContext context)
        {
            var approvalrequestid = context.PluginExecutionContext.PrimaryEntityId;

            
            var fetchXml = @"<fetch top='1'>" +
                           @"<entity name='fdbzap_ar_regardingconfig'>" +
                           @"<attribute name='fdbzap_ar_entitytype' />" +
                           @"<attribute name='fdbzap_ar_completedon_fieldname' />" +
                           @"<attribute name='fdbzap_ar_lastupdatedon_fieldname' />" +
                           @"<attribute name='fdbzap_ar_status_fieldname' />" +
                           @"<link-entity name='fdbzap_ar_approvalrequest' from='fdbzap_ar_regarding_config' to='fdbzap_ar_regardingconfigid' link-type='inner'>" +
                           @"<filter>" +
                           $@"<condition attribute='fdbzap_ar_approvalrequestid' operator='eq' value='{approvalrequestid}'/>" +
                           @"</filter></link-entity></entity></fetch>";


            var response = context.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            if (response == null || response.Entities.Count < 1)
            {
                return null;
            }

            return response.Entities[0];
        }
    }
}