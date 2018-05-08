using System;
using System.Diagnostics.CodeAnalysis;
using Plugins.Common;
using Plugins.Common.Attributes;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins.Entities.fdbzap_ar_action
{
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "Update",
        "fdbzap_ar_action",
        StageEnum.PostOperation,
        ExecutionModeEnum.Asynchronous,
        "",
        "Post Update Approval Action",
        1,
        IsolationModeEnum.Sandbox,
        Image1Name = "Post",
        Image1Attributes = "statuscode,fdbzap_ar_approvalrequest",
        Image1Type = ImageTypeEnum.PostImage,
        Id = "cce47740-dbfa-e711-a953-000d3a34afa9")]
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

        private static readonly string ApprovalRequestLogicalName = "fdbzap_ar_approvalrequest";

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
            // Step 1: Get the status code
            context.Trace($"Depth: {context.PluginExecutionContext.Depth}");
            //if (context.PluginExecutionContext.Depth > 10) return;

            context.Trace("Step 1");
            var status = context.GetAttributeValue<OptionSetValue>("statuscode");

            // Step 2: Check to see if statuscode is run
            context.Trace("Step 2");
            if (status == null || status.Value != 2 /* Run */)
            {
                context.Trace("Status Reason is not set to 2 'Run'.");
                return; // abort plugin execution
            }

            // Update run to complete: 3
            var actionUpdate = new Entity(context.PluginExecutionContext.PrimaryEntityName);
            actionUpdate.Id = context.PluginExecutionContext.PrimaryEntityId;
            actionUpdate["statuscode"] = new OptionSetValue(3);
            context.OrganizationService.Update(actionUpdate);

            // Step 3: Set ActionTaken on Approval Request
            context.Trace("Step 3");
            var approvalrequestReference = context.GetAttributeValue<EntityReference>(ApprovalRequestLogicalName);
            if (approvalrequestReference == null)
            {
                throw ErrorCode.Exception(ErrorCodes.NoApprovalRequestOnAction);
            }

            // Step 4: Has a valid reference, perform the update.
            context.Trace("Step 4");
            var actionTaken = context.GetInputParameter<Entity>("Target").ToEntityReference();
            UpdateApprovalRequest(context, approvalrequestReference, actionTaken);


            // TODO: Evaluate if this is necessary with the rollup action below?
            if (ApprovalRequestHasAtleastOneChild(context, approvalrequestReference))
            {
                context.Trace("Approval Request has at least 1 child, aborting...");
                // nothing to check / update because the action was taken on an approval with children.
                // actions should only be *run* on approval requests without children.

                // It is the action on the nth child level that causes the recursive checks to fire back up the tree.
                return;
            }

            // Approval Request has no children, update the parent action count and run the approval request status check on the ar.

            var rollupAction = GetRollupAction(context, actionTaken);

            if (rollupAction != null)
            {
                context.Trace("Rollup Action Found");
                // Update Parent Approval Request Action Count
                IncrementActionCount(context, rollupAction);

                // Run Parent Approval Request Status Check
                CheckApprovalRequestStatus(context, rollupAction.GetAttributeValue<EntityReference>("fdbzap_ar_approvalrequest"));
            }

            // TODO: Run  action to update regarding object for approvalrequest (approvalrequestReference)
            UpdateRegardingObject(context, approvalrequestReference);
        }

        private static OrganizationResponse UpdateRegardingObject(ILocalPluginContext context,
            EntityReference approvalRequestReference) =>
            context.OrganizationService.Execute(
                new OrganizationRequest("fdbzap_ar_checkstatus") {["Target"] = approvalRequestReference});

        internal static void IncrementActionCount(ILocalPluginContext context, Entity action)
        {
            var count = action.GetAttributeValue<int?>("fdbzap_ar_actioncount");

            if (!count.HasValue)
            {
                count = 1;
            }
            else
            {
                count++;
            }

            action["fdbzap_ar_actioncount"] = count;

            context.OrganizationService.Update(action);
        }

        internal static OrganizationResponse CheckApprovalRequestStatus(ILocalPluginContext context, 
            EntityReference approvaRequestReference) => 
            context.OrganizationService.Execute(new OrganizationRequest("fdbzap_ar_checkstatus") { ["Target"] = approvaRequestReference });

        internal static Entity GetRollupAction(ILocalPluginContext context, EntityReference actionTaken)
        {
            var fetchXml = $@"
<fetch page='1' count='1' distinct='true' no-lock='true'>
  <entity name='fdbzap_ar_action'>
    <attribute name='fdbzap_ar_actionid' />
    <attribute name='fdbzap_ar_priority' />
    <attribute name='fdbzap_ar_actioncount' />
    <attribute name='fdbzap_ar_approvalrequest' />
    <order descending='false' attribute='fdbzap_ar_priority' />
    <link-entity name='fdbzap_ar_action' from='fdbzap_ar_rollupaction' to='fdbzap_ar_actionid' link-type='inner' alias='childaction'>
      <filter type='and'>
        <condition attribute='fdbzap_ar_actionid' operator='eq' value='{actionTaken.Id}'/>
      </filter>
    </link-entity>
  </entity>
</fetch>";

            var response = context.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            if (response == null || response.Entities.Count == 0)
            {
                return null;
            }

            return response.Entities[0];
        }

        internal static bool ApprovalRequestHasAtleastOneChild(
            ILocalPluginContext context,
            EntityReference approvalrequestReference)
        {
            var fe = new FilterExpression();
            fe.AddCondition("fdbzap_ar_parentid", ConditionOperator.Equal, approvalrequestReference.Id);
            fe.AddCondition("statecode", ConditionOperator.Equal, 0);

            var qe = new QueryExpression(ApprovalRequestLogicalName)
            {
                TopCount = 1,
                Criteria = fe,
                ColumnSet = new ColumnSet(
                                 "fdbzap_ar_approvalrequestid")
            };

            var results = context.OrganizationService.RetrieveMultiple(qe);
            return results != null && results?.Entities.Count > 0;
        }

        /// <summary>
        ///     Get the target entity.
        /// </summary>
        /// <remarks>
        /// There is no need to check to see if the target is an entity because the PluginBase 
        /// verifies the message is Update.  Update only has the Target defined as an entity.
        /// This also removes the need for safe casting using as and checking for null.
        /// </remarks>
        /// <param name="context">
        ///     The local plugin context.
        /// </param>
        /// <returns>
        ///     The <see cref="EntityReference" /> target.
        /// </returns>
        /// <exception cref="InvalidPluginExecutionException">
        /// </exception>
        // TODO: Make this internal once spkl.fakes is strongly named, and so is Plugins.Tests
        public static Entity GetTargetEntity(ILocalPluginContext context) =>
            (Entity)context.PluginExecutionContext.InputParameters["Target"];

        /// <summary>
        ///     Update the approval request with the action that was taken.
        /// </summary>
        /// <param name="context">
        ///     The local plugin context.
        /// </param>
        /// <param name="approvalrequestReference">
        ///     The approval request <see cref="EntityReference" />.
        /// </param>
        /// <param name="actionTaken">
        ///     The action <see cref="EntityReference" />.
        /// </param>
        internal static void UpdateApprovalRequest(
            ILocalPluginContext context,
            EntityReference approvalrequestReference,
            EntityReference actionTaken)
        {
            var approvalrequest = approvalrequestReference.ToEntity();
            approvalrequest["fdbzap_ar_actiontaken"] = actionTaken;
            approvalrequest["fdbzap_ar_actiontakenon"] = context.PluginExecutionContext.OperationCreatedOn;
            approvalrequest["statuscode"] = new OptionSetValue(4 /* Action Taken */);
            context.OrganizationService.Update(approvalrequest);
        }
    }
}