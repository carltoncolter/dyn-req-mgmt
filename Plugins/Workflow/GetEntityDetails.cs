using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Plugins.Common;
using Microsoft.Xrm.Sdk.Workflow;

namespace FedBizApps.RequestManagement.Plugins.Workflow
{
    [CrmPluginRegistration("GetEntityDetails", "Get Entity Details", "Get Entity Logical Name and Id","FedBizApps", IsolationModeEnum.Sandbox)]
    [Serializable]
    public sealed class GetEntityDetails : CodeActivity
    {
        [Output("Entity Name")]
        public OutArgument<string> EntityName { get; set; }

        [Output("Entity Id")]
        public OutArgument<string> EntityId { get; set; }

        [Output("Compressed Id")]
        public OutArgument<string> CompressedId { get; set; }


        protected override void Execute(CodeActivityContext eContext)
        {
            var context = eContext.GetExtension<IWorkflowContext>();

            var target = new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId);

            EntityName.Set(eContext, target.LogicalName);
            EntityId.Set(eContext, target.Id.ToString());
            CompressedId.Set(eContext, Utilities.CompressGuid(target.Id));
        }
    }
}
