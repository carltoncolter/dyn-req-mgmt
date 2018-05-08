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
    public class Plugin_fdbzap_ar_approvalrequest_CheckStatusActionTests
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
            pipeline.Depth = 1;
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
        public void TestHappyPath()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_approvalrequest";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            var actionTakenId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            #endregion

            using (var pipeline = new PluginPipeline("fdbzap_ar_checkstatus", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<CheckStatusAction>(true, UnsecureConfig, SecureConfig))
            {

                #region arrange - given with pipeline

                pipeline.InputParameters["Target"] = target.ToEntityReference();
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests

                pipeline.FakeService.ExpectRetrieveMultiple(
                        query =>
                        {
                            var queryExpression = query as QueryExpression;
                            Assert.IsNotNull(
                                queryExpression,
                                "Retrieve Multiple expected Query Expression and received something else.");

                            Assert.AreEqual(
                                "fdbzap_ar_action",
                                queryExpression.EntityName,
                                "Wrong type of entity trying to be retrieved.");

                            // This should return a collection of actions sorted by priority.

                            var result = new EntityCollection();
                            result.Entities.AddRange(
                                new Entity("fdbzap_ar_action", Guid.NewGuid())
                                {
                                    ["fdbzap_ar_priority"] = 1,
                                    ["fdbzap_ar_minaction"] = 3,
                                    ["fdbzap_ar_actioncount"] = 2
                                },
                                new Entity("fdbzap_ar_action", Guid.NewGuid())
                                {
                                    ["fdbzap_ar_priority"] = 2,
                                    ["fdbzap_ar_minaction"] = 18,
                                    ["fdbzap_ar_actioncount"] = 5
                                },
                                new Entity("fdbzap_ar_action", actionTakenId) /* This action will be the action taken */
                                {
                                    ["fdbzap_ar_priority"] = 3,
                                    ["fdbzap_ar_minaction"] = 1,
                                    ["fdbzap_ar_actioncount"] = 1
                                });

                            return result;

                        })
                    .ExpectExecute((request) =>
                    {
                        Assert.AreEqual("fdbzap_ar_update_regardingobject", request.RequestName);
                        return null;
                    });

                // TODO: Complete the rest of the happy path here...

                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then
                pipeline.FakeService.AssertExpectedCalls();
                Assert.IsTrue(pipeline.OutputParameters.Contains("ActionTaken"), "Output Parameter 'ActionTaken' is missing.");
                var actionTaken = pipeline.OutputParameters["ActionTaken"] as EntityReference;
                Assert.AreEqual(actionTakenId, actionTaken?.Id, "ActionTaken does not match expected result.");
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

            using (var pipeline = new PluginPipeline("fdbzap_ar_checkstatus", FakeStages.PostOperation, target))
            {
                pipeline.InputParameters.Remove("Target");
                var context = new FakeLocalPluginContext(pipeline);

                Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                    {
                        CheckStatusAction.GetTargetEntityReference(context);
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

            using (var pipeline = new PluginPipeline("fdbzap_ar_checkstatus", FakeStages.PostOperation, target))
            {
                pipeline.InputParameters["Target"] = null;
                var context = new FakeLocalPluginContext(pipeline);

                Assert.ThrowsException<InvalidPluginExecutionException>(() =>
                    {
                        CheckStatusAction.GetTargetEntityReference(context);
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

            using (var pipeline = new PluginPipeline("fdbzap_ar_checkstatus", FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<CheckStatusAction>(true, UnsecureConfig, SecureConfig))
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

                    Assert.AreEqual("The entity invalidentity is not support by CheckStatusAction.", ex.Message);

                    #endregion
                }
            }
        }
        #endregion
    }
}