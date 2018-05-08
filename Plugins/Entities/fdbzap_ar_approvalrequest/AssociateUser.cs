// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   The check status action plugin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugins.Entities.fdbzap_ar_approvalrequest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using global::Plugins.Common;
    using global::Plugins.Common.Attributes;
    using global::Plugins.Common.Constants;

    using Microsoft.Xrm.Sdk.Messages;

    /// <summary>
    /// The check status action plugin.
    /// </summary>
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "associate",
        "fdbzap_ar_approvalrequest",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous,
        "",
        "Associate Approval Request",
        1,
        IsolationModeEnum.Sandbox,
        Id = "b3b9f268-69c0-4839-addd-58750e4d2b71")]
    [Serializable]
    public class AssociateUser : PluginBase
    {
        #region constructors

        [ExcludeFromCodeCoverage]
        public AssociateUser() { }

        [ExcludeFromCodeCoverage]
        public AssociateUser(string unsecureString, string secureString) : base(unsecureString, secureString) { }

        #endregion

        /// <summary>
        /// Execute the plugin action
        /// </summary>
        /// <param name="context">
        /// The local plugin context.
        /// </param>
        public override void Execute(ILocalPluginContext context)
        {
            // Verify relationship
            var relationship = context.GetInputParameter<Relationship>("Relationship");
            if (relationship == null ||
                !relationship.SchemaName.Equals("fdbzap_ar_approvalrequest_approvers_systemuser", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            // Get Entity EventBooking reference from Target Key from context
            var target = context.GetInputParameter<EntityReference>("Target");
            if (target == null)
            {
                return;
            }

            if (!target.LogicalName.Equals("systemuser") && !target.LogicalName.Equals("fdbzap_ar_approvalrequest"))
            {
                return;
            }


            // Get Entity Contact reference from RelatedEntities Key from context
            var relatedEntities = context.GetInputParameter<EntityReferenceCollection>("RelatedEntities");

            if (relatedEntities == null || relatedEntities.Count == 0)
            {
                return;
            }

            var index = 0; // TODO: This should be the max sequence number
            foreach (var entity in relatedEntities)
            {
                CreateRequest request = null;
                if (entity.LogicalName.Equals("systemuser"))
                {
                    if (!target.LogicalName.Equals("fdbzap_ar_approvalrequest"))
                    {
                        return;
                    }

                    request = CreateApproval(context, ++index, target, entity);
                }

                if (entity.LogicalName.Equals("fdbzap_ar_approvalrequest"))
                {
                    if (!target.LogicalName.Equals("systemuser"))
                    {
                        return;
                    }

                    request = CreateApproval(context, ++index, entity, target);
                }

                if (request != null)
                {
                    context.OrganizationService.Execute(request);
                }
            }
        }

        /// <summary>
        /// The approval request cache.
        /// </summary>
        private Dictionary<Guid, Entity> approvalRequestCache = new Dictionary<Guid, Entity>();

        /// <summary>
        /// Create an approval request for the user.
        /// </summary>
        /// <param name="context">
        /// The <see cref="ILocalPluginContext"/> for the plugin.
        /// </param>
        /// <param name="index">
        /// The index or order number.
        /// </param>
        /// <param name="approvalRequest">
        /// An <see cref="EntityReference"/> that references the source\parent approval request.
        /// </param>
        /// <param name="user">
        /// An <see cref="EntityReference"/> that references the system user associated with the approval request.
        /// </param>
        /// <returns>
        /// The <see cref="CreateRequest"/> to create the new approval request.
        /// </returns>
        private CreateRequest CreateApproval(ILocalPluginContext context, int index, EntityReference approvalRequest, EntityReference user)
        {
            Entity entity = null;
            if (approvalRequestCache.ContainsKey(approvalRequest.Id))
            {
                entity = approvalRequestCache[approvalRequest.Id];
            }
            else
            {
                entity = context.OrganizationService.Retrieve(
                    approvalRequest.LogicalName,
                    approvalRequest.Id,
                    new ColumnSet(
                        "fdbzap_ar_action_comment",
                        "fdbzap_ar_actiontaken",
                        "fdbzap_ar_approvaltype",
                        "fdbzap_ar_inheritactions",
                        "fdbzap_ar_is_sequential",
                        "fdbzap_ar_order",
                        "fdbzap_ar_parentid",
                        "fdbzap_ar_regardingobjectid",
                        "fdbzap_ar_requestcomments",
                        "fdbzap_ar_requestedapprover_team",
                        "fdbzap_ar_requestedapprover_user",
                        "fdbzap_ar_requestedby",
                        "fdbzap_ar_requestedon",
                        "fdbzap_ar_requestid",
                        "fdbzap_ar_responses",
                        "fdbzap_ar_source",
                        "fdbzap_ar_templategroup"));
                approvalRequestCache.Add(approvalRequest.Id, entity);
            }

            // Set Values
            entity.Id = Guid.Empty;
            entity["fdbzap_ar_approvaltype"] = new OptionSetValue(741210001); // Single User Approval
            entity["fdbzap_ar_requestedapprover_user"] = user; // User
            entity["fdbzap_ar_order"] = index;
            entity["fdbzap_ar_parentid"] = approvalRequest;

            // return create request to create the record.
            return new CreateRequest() { Target = entity };
        }
    }
}