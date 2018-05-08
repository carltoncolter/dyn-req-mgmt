// --------------------------------------------------------------------------------------------------------------------
// <summary>
// Create a group from a selection of letters and/or emails for a common response.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Plugins.Common;
using Plugins.Common.Attributes;

namespace Plugins.Actions
{
    /// <summary>
    ///     The check status action plugin.
    /// </summary>
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "fdbzap_CreateGroupFromSelection",
        "none",
        StageEnum.PostOperation,
        ExecutionModeEnum.Synchronous, 
        "",
        "Post CreateGroupFromSelection Action",
        1,
        IsolationModeEnum.Sandbox,
        Id = "7bd68e84-5871-438e-9759-762d2e75fabe")]
    [Serializable]
    public class CreateGroupFromSelection : PluginBase
    {
        public override bool IsGlobalAction => true;

        /// <summary>
        ///     Execute the plugin action
        /// </summary>
        /// <param name="context">
        ///     The local plugin context.
        /// </param>
        public override void Execute(ILocalPluginContext context)
        {
            // Get Input
            var groupName = context.GetInputParameter<string>("groupName");
            var ec = context.GetInputParameter<EntityCollection>("items");

            // Validate Items
            if (ec == null || ec.Entities.Count == 0)
            {
                Fail(context, "No items were selected.  Please make a selection before creating the group.");
                return;
            }

            // Build Entity Reference Collections
            var emails = new EntityReferenceCollection(ec.Entities
                .Where(i => i.LogicalName.Equals("email"))
                .Select(i => i.ToEntityReference()).ToList());

            var letters = new EntityReferenceCollection(ec.Entities
                .Where(i => i.LogicalName.Equals("letter"))
                .Select(i => i.ToEntityReference()).ToList());

            // Validate Items
            if (emails.Count == 0 && letters.Count == 0)
            {
                Fail(context,
                    "No letters or emails were selected.  Only letters and emails can be grouped for response.");
                return;
            }

            // Create Group
            var group = new Entity("fdbzap_cor_group");
            group["fdbzap_name"] = groupName ?? "A New Group";
            group.Id = context.OrganizationService.Create(group);

            // Associate Records

            // Associate the emails
            if (emails.Count > 0)
            {
                var emailRelationship = new Relationship("fdbzap_email_group");
                context.OrganizationService.Associate(group.LogicalName, group.Id, emailRelationship, emails);
            }

            // Associate the letters
            if (letters.Count > 0)
            {
                var emailRelationship = new Relationship("fdbzap_letter_group");
                context.OrganizationService.Associate(group.LogicalName, group.Id, emailRelationship, emails);
            }

            Done(context, group.ToEntityReference());
        }

        private static void Done(ILocalPluginContext context, EntityReference result) =>
            SetOutput(context, false, string.Empty, result);

        private static void Fail(ILocalPluginContext context, string errorMessage) =>
            SetOutput(context, true, errorMessage);

        private static void SetOutput(ILocalPluginContext context, bool error, string msg,
            EntityReference result = null)
        {
            context.PluginExecutionContext.OutputParameters["result"] = result;
            context.PluginExecutionContext.OutputParameters["errorOccurred"] = error;
            context.PluginExecutionContext.OutputParameters["responseMessage"] = msg;
        }

        #region constructors

        [ExcludeFromCodeCoverage]
        public CreateGroupFromSelection()
        {
        }

        [ExcludeFromCodeCoverage]
        public CreateGroupFromSelection(string unsecureString, string secureString) : base(unsecureString, secureString)
        {
        }

        #endregion
    }
}