// NOTE:    Using FakeItEasy (http://fakeiteasy.readthedocs.io) and 
//          spkl.fakes (https://github.com/scottdurow/SparkleXrm/wiki/spkl.fakes) for these tests.
//          However, you might want to look at Fake Xrm Easy (https://dynamicsvalue.com/get-started/overview)
//          
//          The Plugin Test Methodology used here follows the DynamicsPlugin 
//          project (https://github.com/carltoncolter/DynamicsPlugin) with the DynamicsPlugin namespace changed
//          to Plugins.
//
//
// NOTE:    The static FakeServiceCalls class should all be expected behaviors that are used across multiple 
//          tests for the entity.

using Microsoft.Crm.Sdk.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace PluginTests.Entities
{
    using System;

    using Microsoft.Xrm.Sdk.Query;

    public static class PostUpdateFakeServiceCalls
    {
        public static FakeOrganzationService ExpectActionTakenUpdate(
            this FakeOrganzationService service,
            EntityReference targetApprovalRequest,
            Entity target)
        {
            return service.ExpectUpdate(
                (updateEntity) =>
                    {
                        Assert.AreEqual(
                            targetApprovalRequest,
                            updateEntity.ToEntityReference(),
                            "It is not updating the correct approval request record or the entity type is incorrect.");

                        Assert.AreEqual(
                            target.ToEntityReference(),
                            updateEntity.GetAttributeValue<EntityReference>("fdbzap_ar_actiontaken"),
                            "Action Taken does not reference the action ran.");

                        Assert.AreEqual(
                            4,
                            updateEntity.GetAttributeValue<OptionSetValue>("statuscode").Value,
                            "Statuscode should be 4, action taken.");
                    });
        }

        public enum ExpectRetrieveMultipleOfApprovalRequestResult
        {
            NullResult,
            OneChild,
            NoChild
        }

        public static FakeOrganzationService ExpectRetrieveMultipleOfApprovalRequest(
            this FakeOrganzationService service, ExpectRetrieveMultipleOfApprovalRequestResult expectedResult)
        {
            return service.ExpectRetrieveMultiple(
                query =>
                    {
                        var queryExpression = query as QueryExpression;
                        Assert.IsNotNull(
                            queryExpression,
                            "Retrieve Multiple expected Query Expression and received something else.");


                        switch (expectedResult)
                        {
                            case ExpectRetrieveMultipleOfApprovalRequestResult.NullResult:
                                return null;
                            case ExpectRetrieveMultipleOfApprovalRequestResult.OneChild:
                                var result = new EntityCollection();
                                result.Entities.Add(new Entity(queryExpression.EntityName) { Id = Guid.NewGuid() });
                                return result;
                            case ExpectRetrieveMultipleOfApprovalRequestResult.NoChild:
                                return new EntityCollection();
                        }

                        return null;
                    });
        }
    }
}
