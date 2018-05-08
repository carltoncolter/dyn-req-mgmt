// NOTE:    Using FakeItEasy (http://fakeiteasy.readthedocs.io) and 
//          spkl.fakes (https://github.com/scottdurow/SparkleXrm/wiki/spkl.fakes) for these tests.
//          However, you might want to look at Fake Xrm Easy (https://dynamicsvalue.com/get-started/overview)
//          
//          The Plugin Test Methodology used here follows the DynamicsPlugin 
//          project (https://github.com/carltoncolter/DynamicsPlugin) with the DynamicsPlugin namespace changed
//          to Plugins.

using System;
using FakeItEasy;
using Microsoft.Crm.Sdk.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Plugins.Common.Constants;
using Plugins.Entities.fdbzap_ar_approvalrequest;

namespace PluginTests.Entities
{
    using Microsoft.Xrm.Sdk.Query;

    using Plugins.Tests;

    /// <summary>
    /// The sample testable plugin tests.
    /// </summary>
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Plugin_fdbzap_ar_approvalrequest_ApplyTemplateGroupTests
    {
        #region Test Settings

        /// <summary>
        /// The unsecure config.
        /// </summary>
        private const string UnsecureConfig = "";

        /// <summary>
        /// The secure config.
        /// </summary>
        private const string SecureConfig = "http://services.odata.org";

        #endregion

        #region Test Setup/Configuration Methods

        private void SetPipelineDefaults(PluginPipeline pipeline)
        {
            // Set the default values
            pipeline.UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            pipeline.InitiatingUserId = pipeline.UserId;
            pipeline.OrganizationId = Guid.Parse("c0000000-c000-c000-c000-c00000000000");
            pipeline.OrganizationName = "TestOrganization";
            pipeline.CorrelationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            pipeline.BusinessUnitId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            pipeline.RequestId = Guid.NewGuid();
            pipeline.OperationId = Guid.NewGuid();
            pipeline.OperationCreatedOn = DateTime.Now;
            pipeline.IsolationMode = PluginAssemblyIsolationMode.Sandbox;
            pipeline.IsExecutingOffline = false;
            pipeline.IsInTransaction = false;
            pipeline.Mode = SdkMessageProcessingStepMode.Synchronous;
        }

        #endregion

        #region Success Tests

        [TestMethod]
        public void GetInfo()
        {

        }

        [TestMethod]
        public void TestHappyPath()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_approvalrequest";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<ApplyTemplateGroupAction>(true, UnsecureConfig, SecureConfig))
            {

                #region arrange - given with pipeline

                var actionGroupId = Guid.NewGuid();
                pipeline.InputParameters["ApprovalActionGroup"] =
                    new EntityReference("fdbzap_ar_actiongroup", actionGroupId);
                pipeline.InputParameters["Target"] = target.ToEntityReference();
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests
                pipeline.FakeService.ExpectUpdate(
                    r =>
                        {
                            Assert.AreEqual("fdbzap_ar_approvalrequest", r.LogicalName);
                            Assert.AreEqual(target.Id, r.Id);
                        });

                pipeline.FakeService.ExpectRetrieveMultiple(
                    query =>
                        {
                            var queryExpression = query as QueryExpression;
                            Assert.IsNotNull(
                                queryExpression,
                                "Retrieve Multiple expected Query Expression and received something else.");

                            Assert.AreEqual(
                                "fdbzap_ar_actiontemplate",
                                queryExpression.EntityName,
                                "Wrong type of entity trying to be retrieved.");

                            // This should return a collection of actions sorted by priority.

                            var result = new EntityCollection();

                            var approveid = Guid.NewGuid();
                            var denyid = Guid.NewGuid();

                            result.Entities.AddRange(
                                new Entity("fdbzap_ar_actiontemplate", approveid)
                                {
                                    ["fdbzap_ar_name"] = "Approve",
                                    ["fdbzap_ar_button_label"] = "Approve",
                                    ["fdbzap_ar_button_tooltip"] = "Click here to approve the request.",
                                    ["fdbzap_ar_customactioncode"] = "approve",
                                    ["fdbzap_ar_actiontemplateid"] = approveid,
                                    ["fdbzap_ar_allow_comments"] = true,
                                    ["fdbzap_ar_actiontype"] = new OptionSetValue(741210000),
                                    ["fdbzap_ar_actiongroupid"] = new EntityReference("fdbzap_ar_actiongroup", actionGroupId)
                                },
                                new Entity("fdbzap_ar_action", denyid)
                                {
                                    ["fdbzap_ar_name"] = "Deny",
                                    ["fdbzap_ar_button_label"] = "Deny",
                                    ["fdbzap_ar_button_tooltip"] = "Click here to deny the request.",
                                    ["fdbzap_ar_customactioncode"] = "deny",
                                    ["fdbzap_ar_actiontemplateid"] = denyid,
                                    ["fdbzap_ar_allow_comments"] = true,
                                    ["fdbzap_ar_actiontype"] = new OptionSetValue(741210001),
                                    ["fdbzap_ar_actiongroupid"] = new EntityReference("fdbzap_ar_actiongroup", actionGroupId)
                                });

                            return result;
                        });

                pipeline.FakeService.ExpectCreate(r => Guid.NewGuid());
                pipeline.FakeService.ExpectCreate(r => Guid.NewGuid());


                // TODO: Fix testing... this stops before any recursion into child approvals.
                pipeline.FakeService.ExpectRetrieveMultiple(r => null);

                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then
                pipeline.FakeService.AssertExpectedCalls();
                #endregion

            }
        }

        [TestMethod]
        public void TestWithNoActionTemplates()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_approvalrequest";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<ApplyTemplateGroupAction>(true, UnsecureConfig, SecureConfig))
            {

                #region arrange - given with pipeline

                var actionGroupId = Guid.NewGuid();
                pipeline.InputParameters["ApprovalActionGroup"] =
                    new EntityReference("fdbzap_ar_actiongroup", actionGroupId);
                pipeline.InputParameters["Target"] = target.ToEntityReference();
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests

                pipeline.FakeService.ExpectUpdate(r => { });

                pipeline.FakeService.ExpectRetrieveMultiple(
                    query =>
                    {
                        var queryExpression = query as QueryExpression;
                        Assert.IsNotNull(
                            queryExpression,
                            "Retrieve Multiple expected Query Expression and received something else.");

                        Assert.AreEqual(
                            "fdbzap_ar_actiontemplate",
                            queryExpression.EntityName,
                            "Wrong type of entity trying to be retrieved.");

                        return new EntityCollection();
                    });
                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then
                pipeline.FakeService.AssertExpectedCalls();
                #endregion
            }
        }

        [TestMethod]
        public void TestWithNullActionTemplates()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_approvalrequest";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<ApplyTemplateGroupAction>(true, UnsecureConfig, SecureConfig))
            {

                #region arrange - given with pipeline

                var actionGroupId = Guid.NewGuid();
                pipeline.InputParameters["ApprovalActionGroup"] =
                    new EntityReference("fdbzap_ar_actiongroup", actionGroupId);
                pipeline.InputParameters["Target"] = target.ToEntityReference();
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests

                pipeline.FakeService.ExpectUpdate(r => { });

                pipeline.FakeService.ExpectRetrieveMultiple(
                    query =>
                    {
                        var queryExpression = query as QueryExpression;
                        Assert.IsNotNull(
                            queryExpression,
                            "Retrieve Multiple expected Query Expression and received something else.");

                        Assert.AreEqual(
                            "fdbzap_ar_actiontemplate",
                            queryExpression.EntityName,
                            "Wrong type of entity trying to be retrieved.");

                        return null;
                    });
                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then
                pipeline.FakeService.AssertExpectedCalls();
                #endregion
            }
        }
        #endregion

        #region Failure Tests
        [TestMethod]
        public void GetTargetEntityReference_ThrowsErrorWhenTargetIsMissing()
        {
            #region arrange - given

            var entityName = "invalidentity";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            {
                pipeline.InputParameters.Remove("Target");
                var context = new FakeLocalPluginContext(pipeline);

                Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                    {
                        ApplyTemplateGroupAction.GetTargetEntityReference(context);
                    }, "An exception was expected, but it did not occur.");
            }
        }

        [TestMethod]
        public void GetTargetEntityReference_ThrowsErrorWhenTargetIsNull()
        {
            #region arrange - given

            var entityName = "invalidentity";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            {
                pipeline.InputParameters["Target"] = null;
                var context = new FakeLocalPluginContext(pipeline);

                Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                    {
                        ApplyTemplateGroupAction.GetTargetEntityReference(context);
                    }, "An exception was expected, but it did not occur.");
            }
        }

        /// <summary>
        /// Test: Check to see if when an invalid entity is passed as the target, the action plugin throws the expected error.
        /// </summary>
        [TestMethod]
        public void InvalidEntity_ThrowsError()
        {
            #region arrange - given

            var entityName = "invalidentity";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_applytemplategroup", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<ApplyTemplateGroupAction>(true, UnsecureConfig, SecureConfig))
            {
                try
                {
                    #region arrange - given with pipeline

                    pipeline.InputParameters["Target"] = target.ToEntityReference();
                    SetPipelineDefaults(pipeline);

                    #endregion

                    #region act - when

                    pipeline.Execute(plugin);

                    #endregion

                    #region assert - then

                    Assert.Fail("An error was expected, but did not occur.");

                    #endregion
                }
                catch (Exception ex)
                {
                    #region assert - then

                    Assert.AreEqual("The entity invalidentity is not support by ApplyTemplateGroupAction.", ex.Message);

                    #endregion
                }
            }
        }
        #endregion
    }
}