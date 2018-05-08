using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins.Common;
using Plugins.Entities.email;
using Plugins.Entities.fdbzap_ar_approvalrequest;
using PluginTests;

namespace Plugins.Tests.Entities.email
{
    [TestClass]
    public class PostCreateTaskCompletion_Tests
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

            var taskid = Guid.NewGuid();
            var compressedId = Utilities.CompressGuid(taskid);

            var from = new EntityCollection(new[]
            {
                new Entity
                {
                    ["partyid"] = new EntityReference("systemuser", Guid.NewGuid())
                }
            });

            var target = new Entity("email")
            {
                Id = Guid.NewGuid(),
                ["from"] = from,
                ["description"] = @"
<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40""><head><META HTTP-EQUIV=""Content-Type"" CONTENT=""text/html; charset=us-ascii""><meta name=Generator content=""Microsoft Word 15 (filtered medium)""><style><!--
/* Font Definitions */
@font-face
	{font-family:""Cambria Math"";
	panose-1:2 4 5 3 5 4 6 3 2 4;}
@font-face
	{font-family:Calibri;
	panose-1:2 15 5 2 2 2 4 3 2 4;}
/* Style Definitions */
p.MsoNormal, li.MsoNormal, div.MsoNormal
	{margin:0in;
	margin-bottom:.0001pt;
	font-size:11.0pt;
	font-family:""Calibri"",sans-serif;}
a:link, span.MsoHyperlink
	{mso-style-priority:99;
	color:#0563C1;
	text-decoration:underline;}
a:visited, span.MsoHyperlinkFollowed
	{mso-style-priority:99;
	color:#954F72;
	text-decoration:underline;}
span.EmailStyle17
	{mso-style-type:personal-compose;
	font-family:""Calibri"",sans-serif;
	color:windowtext;}
.MsoChpDefault
	{mso-style-type:export-only;
	font-family:""Calibri"",sans-serif;}
@page WordSection1
	{size:8.5in 11.0in;
	margin:1.0in 1.0in 1.0in 1.0in;}
div.WordSection1
	{page:WordSection1;}
--></style><!--[if gte mso 9]><xml>
<o:shapedefaults v:ext=""edit"" spidmax=""1026"" />
</xml><![endif]--><!--[if gte mso 9]><xml>
<o:shapelayout v:ext=""edit"">
<o:idmap v:ext=""edit"" data=""1"" />
</o:shapelayout></xml><![endif]--></head>
<body lang=EN-US link=""#0563C1"" vlink=""#954F72"">
<div class=WordSection1>
<p class=MsoNormal>Completed Blah Blah<o:p></o:p></p>
<p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal>"
                                  + "#TSK-" + compressedId + "#"
                                  + "<o:p></o:p></p></div></body></html>"
            };

            #endregion

            using (var pipeline = new PluginPipeline(FakeMessageNames.Create, FakeStages.PreOperation, target))
            using (var plugin = new PluginContainer<PostCreateTaskCompletion>(true, UnsecureConfig, SecureConfig))
            {

                #region arrange - given with pipeline

                pipeline.InputParameters["Target"] = target;
                SetPipelineDefaults(pipeline);

                #endregion

                #region pipeline responses and tests

                pipeline.FakeService
                        .ExpectRetrieveMultiple((request) => new EntityCollection(new[] {new Entity() {Id = taskid}}))
                        .ExpectUpdate((updateTask) => { })
                        .ExpectRetrieveMultiple((request) =>
                        {
                            var qe = (FetchExpression) request;
                            Assert.IsTrue(qe.Query.Contains("<fetch top='100'><entity name='activitymimeattachment'>"));
                            return null;
                        });


                // TODO: Complete the rest of the happy path here...

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

        #endregion
    }
}
