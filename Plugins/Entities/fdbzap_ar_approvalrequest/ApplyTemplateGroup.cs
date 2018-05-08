// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The check status action plugin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugins.Entities.fdbzap_ar_approvalrequest
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using global::Plugins.Common;
    using global::Plugins.Common.Attributes;
    using global::Plugins.Common.Constants;

    /// <summary>
    /// The check status action plugin.
    /// </summary>
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "fdbzap_ar_applytemplategroup",
        "fdbzap_ar_approvalrequest",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous,
        "",
        "Post Apply Template Group",
        1,
        IsolationModeEnum.Sandbox,
        Id = "1b64a53c-7d08-e811-a959-000d3a1087a0")]
    [Serializable]
    public class ApplyTemplateGroupAction : PluginBase
    {

        #region constructors

        [ExcludeFromCodeCoverage]
        public ApplyTemplateGroupAction() { }

        [ExcludeFromCodeCoverage]
        public ApplyTemplateGroupAction(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        /// <summary>
        /// Execute the plugin action
        /// </summary>
        /// <param name="context">
        /// The local plugin context.
        /// </param>
        public override void Execute(ILocalPluginContext context)
        {
            // Step 1: Get Valid Target
            context.Trace("Step 1: Get Valid Target");
            var approvalRequestRef = GetTargetEntityReference(context);

            // Step 2: Update Target
            context.Trace("Step 2: Update Target");
            var approvalRequest = approvalRequestRef.ToEntity();
            approvalRequest["fdbzap_ar_import_actiongroup"] = null;
            context.OrganizationService.Update(approvalRequest);

            // Step 3: Get Action Group
            context.Trace("Step 3: Get Action Group");
            var actionGroup = context.GetInputParameter<EntityReference>("ApprovalActionGroup");

            // Step 4: Get Actions
            context.Trace("Step 4: Get Action Templates");
            var result = GetActionTemplates(context, actionGroup);
            if (!(result?.Entities.Count > 0)) 
            {
                return;
            }

            // Step 5: Loop Through Actions
            context.Trace("Step 5: Loop Through Action Templates");
            foreach (var actionTemplate in result.Entities)
            {
                context.OrganizationService.Create(CreateActionUsingTemplate(actionTemplate, approvalRequestRef));
            }

            // Step 6: Look for child approvals
            context.Trace("Step 6: Look for child approvals");
            var childresults = GetChildApprovalsThatInheritFromParent(context, approvalRequestRef);

            // Step 7: If there are child approvals, apply to inherited ones
            context.Trace("Step 7: Apply to inherited approvals");
            if (childresults != null && childresults.Entities.Count > 0)
            {
                foreach (var entity in childresults.Entities)
                {
                    var req = new OrganizationRequest("fdbzap_ar_applytemplategroup");
                    req.Parameters.Add("Target", entity.ToEntityReference());
                    req.Parameters.Add("ApprovalActionGroup", actionGroup);

                    context.OrganizationService.Execute(req);
                }
            }

            // COMPLETE
            context.Trace("Plugin Comptle.");
        }

        private static EntityCollection GetChildApprovalsThatInheritFromParent(
            ILocalPluginContext context,
            EntityReference approvalRequestRef)
        {
            var qe = new QueryExpression("fdbzap_ar_approvalrequest")
                         {
                             Criteria = new FilterExpression(),
                             ColumnSet = new ColumnSet(
                                 "fdbzap_ar_approvalrequestid"),
                         };

            qe.Criteria.AddCondition("fdbzap_ar_parentid", ConditionOperator.Equal, approvalRequestRef.Id);
            qe.Criteria.AddCondition("fdbzap_ar_inheritactions", ConditionOperator.Equal, true);

            var childresults = context?.OrganizationService?.RetrieveMultiple(qe);
            return childresults;
        }

        private static Entity CreateActionUsingTemplate(Entity actionTemplate, EntityReference approvalRequestRef)
        {
            var action = new Entity("fdbzap_ar_action");
            foreach (var attr in actionTemplate.Attributes)
            {
                if (attr.Key.StartsWith("fdbzap_ar"))
                {
                    action[attr.Key] = attr.Value;
                }
            }

            action["fdbzap_ar_actiontemplateid"] = actionTemplate.ToEntityReference();
            action["fdbzap_ar_approvalrequest"] = approvalRequestRef;
            action["statecode"] = new OptionSetValue(0);
            action["statuscode"] = new OptionSetValue(0);
            return action;
        }

        private static EntityCollection GetActionTemplates(ILocalPluginContext context, EntityReference actionGroup)
        {
            var qe = new QueryExpression("fdbzap_ar_actiontemplate")
                         {
                             Criteria = new FilterExpression(),
                             ColumnSet = new ColumnSet("fdbzap_ar_actiontemplateid",
                             "fdbzap_ar_actiontype", "fdbzap_ar_allow_comments",
                             "fdbzap_ar_button_label", "fdbzap_ar_button_order",
                             "fdbzap_ar_button_tooltip", "fdbzap_ar_customactioncode",
                             "fdbzap_ar_endapprovalprocesswhenclicked",
                             "fdbzap_ar_name", "fdbzap_ar_priority", "fdbzap_ar_requirecomment")
                         };

            qe.Criteria.AddCondition("fdbzap_ar_actiongroupid", ConditionOperator.Equal, actionGroup.Id);

            return context?.OrganizationService?.RetrieveMultiple(qe);
        }

        /// <summary>
        ///     Get the target entity reference.
        /// </summary>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <returns>
        ///     The <see cref="EntityReference" /> target.
        /// </returns>
        /// <exception cref="InvalidPluginExecutionException">
        /// </exception>       
        public static EntityReference GetTargetEntityReference(ILocalPluginContext context)
        {
            if (!context.PluginExecutionContext.InputParameters.ContainsKey("Target") ||
                !(context.PluginExecutionContext.InputParameters["Target"] is EntityReference target))
            { // this should not be possible.
                throw ErrorCode.Exception(ErrorCodes.NullTarget);
            }

            return target;
        }
    }
}