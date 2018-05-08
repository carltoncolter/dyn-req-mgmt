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
        "fdbzap_ar_checkstatus",
        "fdbzap_ar_approvalrequest",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous,
        "",
        "Post Check Status Approval Request",
        1,
        IsolationModeEnum.Sandbox,
        Id = "52841670-dbfa-e711-a956-000d3a34a1bd")]
    [Serializable]
    public class CheckStatusAction : PluginBase
    {

        #region constructors

        [ExcludeFromCodeCoverage]
        public CheckStatusAction() { }

        [ExcludeFromCodeCoverage]
        public CheckStatusAction(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        /// <summary>
        /// Execute the plugin action
        /// </summary>
        /// <param name="context">
        /// The local plugin context.
        /// </param>
        public override void Execute(ILocalPluginContext context)
        {
            // Product Backlog Item 139
            // The plugin should verify that all child approvals are approved 
            // or rejected to meet the minimum requirements, if it is and the 
            // regarding object id is set, then it updates the current approval
            // request and runs the "fdbzap_ar_update_regardingobject" action on 
            // the current approval request otherwise it runs the "fdbzap_ar_checkstatus" 
            // action on the parent approval request.

            // Step 1: Configure Output Paramaters
            context.Trace($"Depth: {context.PluginExecutionContext.Depth}");
            context.Trace("Step 1");
            if (!context.PluginExecutionContext.OutputParameters.Keys.Contains("ActionTaken"))
            {
                context.PluginExecutionContext.OutputParameters.Add("ActionTaken", null);
            }

            // Step 2: Get Valid Target
            context.Trace("Step 2");
            var target = GetTargetEntityReference(context);

            // Step 3: Get Actions
            context.Trace("Step 3");
            var actions = GetActions(context, target);

            // Step 4: Check if there are actions to look at.
            context.Trace("Step 4");
            if (actions == null || actions.Entities.Count == 0) return;

            // Step 5: Check Action Rules
            context.Trace("Step 5");
            foreach (var action in actions.Entities)
            {
                // Rule 1: Min Action >= Count
                context.Trace("Rule 1");
                var minaction = action.GetAttributeValue<int>("fdbzap_ar_minaction");
                var count = action.GetAttributeValue<int>("fdbzap_ar_actioncount");
                if (count >= minaction || actions.Entities.Count == 0)
                {
                    // Rule 1: Success
                    context.Trace("Rule 1 Success");
                    ActionTaken(context, target, action);
                    return; // End Processing of rules.
                }
            }
        }

        /// <summary>
        ///     Updates the action taken for the approval request.
        /// </summary>
        /// <param name="context">
        ///     The local plugin context.
        /// </param>
        /// <param name="target">
        ///     The target (approval request).
        /// </param>
        /// <param name="action">
        ///     The action (taken).
        /// </param>
        private static void ActionTaken(ILocalPluginContext context, EntityReference target, Entity action)
        {
            if (target != null)
            {
                var request = new OrganizationRequest("fdbzap_ar_update_regardingobject");
                request.Parameters.Add("Target", target);
                context.OrganizationService.Execute(request);
            }

            context.PluginExecutionContext.OutputParameters["ActionTaken"] = action.ToEntityReference();
        }

        /// <summary>
        ///     Get the actions ordered by Processing Priority for the approval request.
        /// </summary>
        /// <param name="context">
        ///     The local plugin context.
        /// </param>
        /// <param name="target">
        ///     The target approval request.
        /// </param>
        /// <returns>
        ///     The <see cref="EntityCollection" /> of actions.
        /// </returns>
        private static EntityCollection GetActions(ILocalPluginContext context, EntityReference target)
        {
            var filter = new FilterExpression();
            filter.AddCondition("fdbzap_ar_approvalrequest", ConditionOperator.Equal, target.Id);

            var qe = new QueryExpression("fdbzap_ar_action")
                         {
                             NoLock = true,
                             PageInfo =
                                 new PagingInfo { Count = 500, PageNumber = 1 },
                             ColumnSet = new ColumnSet(
                                 "fdbzap_ar_actionid",
                                 "fdbzap_ar_minaction",
                                 "fdbzap_ar_actioncount"),
                             Distinct = false,
                             Criteria = filter
                         };
            qe.Orders.Add(new OrderExpression("fdbzap_ar_priority", OrderType.Ascending));
            return context.OrganizationService.RetrieveMultiple(qe);
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