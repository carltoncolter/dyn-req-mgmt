using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Plugins.Common;
using Plugins.Common.Attributes;

namespace Plugins.Entities.email
{
    [CrmPluginConfiguration(ConfigType = ConfigType.String, IgnoreUnsecureConfig = true)]
    [CrmPluginRegistration(
        "Create",
        "email",
        StageEnum.PostOperation,
        ExecutionModeEnum.Asynchronous,
        "",
        "Post Create Email",
        1,
        IsolationModeEnum.Sandbox,
        Id = "b26e8be7-0430-4a08-9166-621dbab82e5f")]
    [Serializable]
    public class PostCreateTaskCompletion : PluginBase
    {
        /// <inheritdoc />
        /// <remarks>
        ///     The <c>InitializePlugin</c> method is executed whenever a new plugin object is created.
        /// </remarks>
        public override void InitializePlugin()
        {
            // TODO: Add initialization work here or remove method
        }

        private string Html2Text(string source)
        {
            // Modification of https://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text
            try
            {
                // Process line breaks
                var result = source.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = Regex.Replace(result, @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*head([^>])*(>.*(<( )*(/)( )*head( )*>)|/>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*script([^>])*(>.*(<( )*(/)( )*script( )*>)|/>)", string.Empty, RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = Regex.Replace(result, @"<( )*style([^>])*(>.*(<( )*(/)( )*style( )*>)|/>)", string.Empty, RegexOptions.IgnoreCase);
               
                // insert tabs in spaces of <td> tags
                result = Regex.Replace(result, @"<( )*td([^>])*>", "\t", RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = Regex.Replace(result, @"<( )*(br|li)([ /])*>", "\r", RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = Regex.Replace(result, @"<( )*(div|tr|p)([^>])*>", "\r\r", RegexOptions.IgnoreCase);

                // remove links
                result = Regex.Replace(result, @"<( )*a( )*(href=""(?<href>.*)"")*([^>])*>(?<name>.*)</a>", "${name} ${href}", RegexOptions.IgnoreCase);

                // Remove remaining tags like <tags - anything that's enclosed inside < >
                result = Regex.Replace(result, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);

                // replace special characters:
                result = Regex.Replace(result,@"&bull;", " * ",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&lsaquo;", "<",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&rsaquo;", ">",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&trade;", "(tm)",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&frasl;", "/",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&lt;", "<",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&gt;", ">",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&copy;", "(c)",RegexOptions.IgnoreCase);
                result = Regex.Replace(result,@"&reg;", "(r)",RegexOptions.IgnoreCase);
                
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = Regex.Replace(result, @"&(.{2,6});", string.Empty, RegexOptions.IgnoreCase);

                // for testing
                // System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = Regex.Replace(result, "(\r)( |\t)+(\r)", "\r\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\t)", "\t\t", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\t)( )+(\r)", "\t\r", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(\r)( )+(\t)", "\r\t", RegexOptions.IgnoreCase);
                
                // Remove multiple tabs following a line break with just one tab
                result = Regex.Replace(result, "(\r)(\t)+", "\r\t", RegexOptions.IgnoreCase);

                // Simplify spacing and line breaks
                result = Regex.Replace(result, "\t\t\t\t(\t)+", "\t\t\t\t");
                result = Regex.Replace(result, "\r\r(\r)+", "\r\r");

                // That's it.
                return result;
            }
            catch
            {
                return source;
            }
        }

        public override void Execute(ILocalPluginContext context)
        {
            // only systemuser senders will be processed
            var sender = GetSender(context);
            if (sender == null || !sender.LogicalName.Equals("systemuser")) return;

            // Get Body
            var body = context.GetAttributeValue<string>("description");
            if (string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            // Get Task Id from Body & verify owner of task is the sender
            var taskid = GetTaskId(context, body, sender);
            if (!taskid.HasValue)
            {
                // If there is no task id, stop work
                return;
            }
            
            // Update the task
            CompleteTask(context, taskid.Value);

            var attachments = GetAttachments(context);

            // TODO: process attachments
        }

        private static void CompleteTask(ILocalPluginContext context, Guid taskid)
        {
            var task = new Entity("task")
            {
                Id = taskid,
                ["statecode"] = new OptionSetValue(1 /* Completed */),
                ["statuscode"] = new OptionSetValue(5 /* Completed */)
            };

            context.OrganizationService.Update(task);
        }

        private static EntityReference GetSender(ILocalPluginContext context)
        {
            var from = context.GetAttributeValue<EntityCollection>("from");
            EntityReference sender = null;

            if (@from != null && @from.Entities.Count > 0)
            {
                var party = @from.Entities[0];
                sender = party.GetAttributeValue<EntityReference>("partyid");
            }

            return sender;
        }

        private static EntityCollection GetAttachments(ILocalPluginContext context)
        {
            var mfetch = @"<fetch top='100'><entity name='activitymimeattachment'>" +
                         @"<attribute name='body'/>" +
                         @"<attribute name='filename'/>" +
                         @"<attribute name='filesize'/>" +
                         @"<attribute name='mimetype'/>" +
                         @"<filter>" +
                         $"<condition attribute='activityid' operator='eq' value='${context.PluginExecutionContext.PrimaryEntityId}'/>" +
                         @"</filter></entity></fetch>";

            var attachments = context.OrganizationService.RetrieveMultiple(new FetchExpression(mfetch));
            return attachments;
        }

        private Guid? GetTaskId(ILocalPluginContext context, string body, EntityReference sender)
        {
            if (body.IndexOf("completed", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                // no completed tag
                return null;
            }

            if (body.IndexOf("#TSK", StringComparison.InvariantCulture) == -1)
            {
                // no task tag
                return null;
            }

            if (Regex.IsMatch(body, "^[ \r\n\t]*<"))
            {
                // treat as html
                body = Html2Text(body);
            }

            if (!Regex.IsMatch(body, "^[ \r\n\t]*completed", RegexOptions.IgnoreCase))
            {
                // completed is not at start, so stop processing email.
                return null;
            }

            string taskidStr;
            try
            {
                taskidStr = Regex.Match(body, "#TSK-(?<taskid>...........-...........)#", RegexOptions.IgnoreCase)
                    .Groups["taskid"].Value;
            }
            catch
            {
                // Syntax error in the regular expression
                return null;
            }

            if (String.IsNullOrEmpty(taskidStr))
            {
                // no id to handle
                return null;
            }

            var taskid = Utilities.DecompressGuid(taskidStr);

            var fetch = @"<fetch top='1'><entity name='task'><attribute name='activityid'/><filter>" +
                        $"<condition attribute='ownerid' operator='eq' value='${sender.Id}'/>" +
                        $"<condition attribute='activityid' operator='eq' value='${taskid}'/>" +
                        @"</filter></entity></fetch>";

            var tasks = context.OrganizationService.RetrieveMultiple(new FetchExpression(fetch));

            // return result
            return tasks == null || tasks.Entities.Count != 1 ? (Guid?) null : tasks[0].Id;
        }

        #region constructors

        [ExcludeFromCodeCoverage]
        public PostCreateTaskCompletion()
        {
        }

        [ExcludeFromCodeCoverage]
        public PostCreateTaskCompletion(string unsecureString, string secureString) : base(unsecureString, secureString)
        {
        }

        #endregion
    }
}