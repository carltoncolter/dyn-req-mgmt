// NOTE:    Using FakeItEasy (http://fakeiteasy.readthedocs.io) and 
//          spkl.fakes (https://github.com/scottdurow/SparkleXrm/wiki/spkl.fakes) for these tests.
//          However, you might want to look at Fake Xrm Easy (https://dynamicsvalue.com/get-started/overview)
//          
//          The Plugin Test Methodology used here follows the DynamicsPlugin 
//          project (https://github.com/carltoncolter/DynamicsPlugin) with the DynamicsPlugin namespace changed
//          to Plugins.

using System;
using Microsoft.Crm.Sdk.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Plugins.Common.Constants;
using Plugins.Entities.fdbzap_ar_action;

namespace PluginTests.Entities
{
    using Plugins.Tests;

    /// <summary>
    ///     The Entity Plugin Test for fdbzap_ar_action, PostUpdate
    /// </summary>
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Plugin_fdbzap_ar_action_PostUpdateTests
    {
        #region Test Settings

        /// <summary>
        ///     The secure config.
        /// </summary>
        private const string SecureConfig = "";

        /// <summary>
        ///     The unsecure config.
        /// </summary>
        private const string UnsecureConfig = "";

        #endregion

        #region Test Setup/Configuration Methods
        /// <summary>
        /// Set pipeline defaults.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline.
        /// </param>
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

            var entityName = "fdbzap_ar_action";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            target["statuscode"] = new OptionSetValue(2); /* Status Code: Run */
            var targetApprovalRequest = new EntityReference("fdbzap_ar_approvalrequest", Guid.NewGuid());
            target["fdbzap_ar_approvalrequest"] = targetApprovalRequest;

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Update, FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<PostUpdate>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline

                pipeline.PostImages.Add("Post", target);
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests

                pipeline.FakeService
                    .ExpectUpdate((entity) => { })
                    .ExpectActionTakenUpdate(targetApprovalRequest, target)
                    .ExpectRetrieveMultipleOfApprovalRequest(
                        PostUpdateFakeServiceCalls.ExpectRetrieveMultipleOfApprovalRequestResult.NoChild)
                    .ExpectRetrieveMultiple((query) => null)
                    .ExpectExecute((request) =>
                    {
                        Assert.AreEqual("fdbzap_ar_checkstatus", request.RequestName);
                        return null;
                    });

                pipeline.FakeService.ExpectRetrieveMultiple((query) => null);
                // TODO: Complete the rest of the happy path here...

                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then

                // No errors means it exited properly
                #endregion
            }
        }

        /// <summary>
        /// Test: Check to see if when the statuscode is null, it aborts the call.
        /// </summary>
        [TestMethod]
        public void StatusCodeNull_AbortsCall()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_action";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Update, FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<PostUpdate>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline

                pipeline.PostImages.Add("Post", target);
                SetPipelineDefaults(pipeline);

                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then

                // No errors means it exited properly
                #endregion
            }
        }

        /// <summary>
        /// Test: Check to see if when the statuscode is 1, it aborts the call.
        /// </summary>
        [TestMethod]
        public void StatusCodeOne_AbortsCall()
        {
            #region arrange - given

            var entityName = "fdbzap_ar_action";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            target.Attributes["statuscode"] = new OptionSetValue(1);

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Update, FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<PostUpdate>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline

                pipeline.PostImages.Add("Post", target);
                SetPipelineDefaults(pipeline);

                #endregion

                #region act - when

                pipeline.Execute(plugin);

                #endregion

                #region assert - then

                // No errors means it exited properly
                #endregion
            }
        }

        #endregion

        #region Failure Tests


        [TestMethod]
        public void GetTarget_ThrowsError()
        {
            #region arrange - given
            var context = new FakeLocalPluginContext();
            #endregion

            #region act & assert

            Assert.ThrowsException<System.NullReferenceException>(
                () =>
                    {
                        Entity target = PostUpdate.GetTargetEntity(null);
                    });

            Assert.ThrowsException<System.NullReferenceException>(
                () =>
                    {
                        Entity target = PostUpdate.GetTargetEntity(context);
                    });

            #endregion

        }

        /// <summary>
        /// PluginBase Test: The invalid entity type throws an error.
        /// </summary>
        [TestMethod]
        public void InvalidEntityType_ThrowsError()
        {
            #region arrange - given

            var entityName = "account";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Update, FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<PostUpdate>(true, UnsecureConfig, SecureConfig))
            {
                try
                {
                    #region arrange - given with pipeline

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

                    Assert.AreEqual(
                        string.Format(ResponseMessages.InvalidEntity, entityName, plugin.Instance.PluginName),
                        ex.Message);

                    #endregion
                }
            }
        }
        #endregion

        #region Inner Test (Sub-Test) Methods
        /// <summary>
        /// The test up to approval request retrieve multiple.
        /// </summary>
        /// <param name="expectedResult">
        /// The expected result, such as null result, or one child.
        /// </param>
        private void TestUpToApprovalRequestRetrieveMultiple(
            PostUpdateFakeServiceCalls.ExpectRetrieveMultipleOfApprovalRequestResult expectedResult)
        {
            #region arrange - given

            var entityName = "fdbzap_ar_action";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            target["statuscode"] = new OptionSetValue(2); /* Status Code: Run */
            var targetApprovalRequest = new EntityReference("fdbzap_ar_approvalrequest", Guid.NewGuid());
            target["fdbzap_ar_approvalrequest"] = targetApprovalRequest;

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Update, FakeStages.PostOperation, target))
            using (var plugin = new PluginContainer<PostUpdate>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline

                pipeline.PostImages.Add("Post", target);
                SetPipelineDefaults(pipeline);
                #endregion

                #region pipeline responses and tests

                pipeline.FakeService.ExpectActionTakenUpdate(targetApprovalRequest, target)
                    .ExpectRetrieveMultipleOfApprovalRequest(expectedResult);

                pipeline.FakeService.ExpectRetrieveMultiple((query) => { return null; });
                #endregion

                #region act - when
                pipeline.Execute(plugin);

                #endregion

                #region assert - then

                // No errors means it exited properly
                #endregion
            }
        }

        #endregion

    }
}