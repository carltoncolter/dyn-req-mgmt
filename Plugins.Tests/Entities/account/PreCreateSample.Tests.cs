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
using Plugins.Entities.account;

namespace PluginTests.Entities
{
    /// <summary>
    /// The sample testable plugin tests.
    /// </summary>
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class Plugin_account_PreCreateSampleTests
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
        public void CreateAccount_DownloadsDataUsingWebClient()
        {
            #region arrange - given

            var entityName = "account";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            var fakeWebClient = A.Fake<IWebClient>();

            A.CallTo(() => fakeWebClient.UploadData(A<string>.Ignored, A<string>.Ignored, A<byte[]>.Ignored))
                .Returns(null);

            A.CallTo(() => fakeWebClient.DownloadData(A<Uri>.Ignored)).Returns(null);

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Create, FakeStages.PreOperation, target))
            using (var plugin = new PluginContainer<PreCreateSample>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline
                plugin.Instance.WebClient = fakeWebClient;
                SetPipelineDefaults(pipeline);
                #endregion
                
                #region pipeline responses and tests

                //pipeline.FakeService.ExpectRetrieve((retrieveEntityName, retrieveEntityId, retrieveColumnSet) =>
                //    {
                //        return new Entity("account");
                //    }).ExpectCreate(createEntity =>
                //    {
                //        // test in create call
                //        Assert.IsTrue(createEntity.LogicalName.Equals("annotation",
                //            StringComparison.InvariantCultureIgnoreCase));
                //        return Guid.NewGuid();
                //    });

                #endregion

                #region act - when
                pipeline.Execute(plugin);
                #endregion

                #region assert - then
                A.CallTo(
                    () => fakeWebClient.DownloadData(A<Uri>.Ignored)
                ).MustHaveHappened();

                pipeline.FakeService.AssertExpectedCalls();
                #endregion
            }
        }
        #endregion

        #region Failure Tests
        [TestMethod]
        public void CreateUknownEntity_ThrowsAnError()
        {
            #region arrange - given

            var entityName = "==UnkownEntity==";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Create, FakeStages.PreOperation, target))
            using (var plugin = new PluginContainer<PreCreateSample>(UnsecureConfig, SecureConfig))
            {
                // Wrapped in try catch because a failure is expected.
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

                    Assert.AreEqual(string.Format(ResponseMessages.InvalidEntity, entityName, plugin.Instance.PluginName),
                        ex.Message);

                    #endregion
                }
            }
        }

        [TestMethod]
        public void CreateAccount_BadWebClient_ThrowsError()
        {
            #region arrange - given
            var entityName = "account";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            var fakeWebClient = A.Fake<IWebClient>();
            A.CallTo(() => fakeWebClient.DownloadData(A<Uri>.Ignored)).Throws(new InvalidCastException("error"));
            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Create, FakeStages.PreOperation, target))
            using (var plugin = new PluginContainer<PreCreateSample>(true, UnsecureConfig, SecureConfig))
            {
                #region arrange - given with pipeline
                plugin.Instance.WebClient = fakeWebClient;
                SetPipelineDefaults(pipeline);
                #endregion

                #region act and assert

                Assert.ThrowsException<InvalidPluginExecutionException>(
                    () => pipeline.Execute(plugin));
                #endregion
            }
        }

        [TestMethod]
        public void CreateAccount_NoSecureConfig_ThrowsError()
        {
            #region arrange - given

            var entityName = "account";
            var target = new Entity(entityName) { Id = Guid.NewGuid() };
            var fakeWebClient = A.Fake<IWebClient>();

            A.CallTo(
                () => fakeWebClient.UploadData(A<string>.Ignored, A<string>.Ignored, A<byte[]>.Ignored)
            ).Returns(null);

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Create, FakeStages.PreOperation, target))
            using (var plugin = new PluginContainer<PreCreateSample>(true, string.Empty, string.Empty))
            {
                #region arrange - given with pipeline
                plugin.Instance.WebClient = fakeWebClient;
                SetPipelineDefaults(pipeline);
                #endregion

                #region act and assert

                try
                {
                    // act
                    pipeline.Execute(plugin);
                    // assert error
                    Assert.Fail("An error was expected, but did not occur.");
                }
                catch (InvalidPluginExecutionException ex)
                {
                    Assert.AreEqual("No Secure Configuration", ex.Message);
                }
                #endregion
            }
        }
        #endregion
    }
}