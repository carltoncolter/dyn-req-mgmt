namespace Plugins.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;

    using Microsoft.Crm.Sdk.Fakes;
    using Microsoft.Xrm.Sdk;

    using Plugins.Common;

    [ExcludeFromCodeCoverage]
    public class FakeLocalPluginContext : ILocalPluginContext
    {
        public FakeLocalPluginContext()
        {
        }

        public FakeLocalPluginContext(PluginPipeline pipeline)
        {
            Pipeline = pipeline;
        }

        public IServiceEndpointNotificationService NotificationService =>
            (IServiceEndpointNotificationService)ServiceProvider.GetService(
                typeof(IServiceEndpointNotificationService));

        public IOrganizationService OrganizationService => Pipeline.Service;

        public IOrganizationServiceFactory OrganizationServiceFactory => Pipeline.Factory;

        public PluginPipeline Pipeline { get; set; }

        public IPluginExecutionContext PluginExecutionContext => Pipeline.PluginExecutionContext;

        public EntityImageCollection PostEntityImages => Pipeline.PostImages;

        public EntityImageCollection PreEntityImages => Pipeline.PreImages;

        public IServiceProvider ServiceProvider => Pipeline.ServiceProvider;

        public ITracingService TracingService => Pipeline.TracingService;

        public T GetAttributeValue<T>(string fieldname)
        {
            if (PluginExecutionContext.InputParameters.Contains("Target"))
                if (PluginExecutionContext.InputParameters["Target"] is Entity target && target.Contains(fieldname))
                    return target.GetAttributeValue<T>(fieldname);

            Entity pre = null, post = null;
            if (PluginExecutionContext.PostEntityImages.Count > 0)
                post = PluginExecutionContext.PostEntityImages.First().Value;
            if (PluginExecutionContext.PreEntityImages.Count > 0)
                pre = PluginExecutionContext.PreEntityImages.First().Value;

            if (post != null && (pre == null || post.Contains(fieldname))) return post.GetAttributeValue<T>(fieldname);
            if (pre != null) return pre.GetAttributeValue<T>(fieldname);

            return default(T);
        }

        public Entity GetEntityImage(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (PluginExecutionContext.PostEntityImages.Count > 0)
                    return PluginExecutionContext.PostEntityImages.First().Value;
                if (PluginExecutionContext.PreEntityImages.Count > 0)
                    return PluginExecutionContext.PreEntityImages.First().Value;
                if (Pipeline.PostImages.Count > 0) return Pipeline.PostImages.First().Value;
                if (Pipeline.PreImages.Count > 0) return Pipeline.PreImages.First().Value;
            }

            if (PostEntityImages.ContainsKey(name)) return PostEntityImages[name];
            if (PreEntityImages.ContainsKey(name)) return PreEntityImages[name];

            return null;
        }

        public T GetInputParameter<T>(string field)
        {
            if (PluginExecutionContext?.InputParameters == null
                || !PluginExecutionContext.InputParameters.ContainsKey(field)) return default(T);

            return (T)PluginExecutionContext.InputParameters[field];
        }

        /// <inheritdoc />
        public void Trace(CultureInfo cultureInfo, string format, params object[] args)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;

            var message = format;
            if (args != null) message = string.Format(cultureInfo, format, args);

            if (string.IsNullOrWhiteSpace(message) || TracingService == null) return;

            if (PluginExecutionContext == null) TracingService.Trace(message);
            else

                // This should never happen
                TracingService.Trace(
                    "{0}, Correlation Id: {1}, Initiating User: {2}",
                    message,
                    PluginExecutionContext.CorrelationId,
                    PluginExecutionContext.InitiatingUserId);
        }

        /// <inheritdoc />
        public void Trace(Exception exception)
        {
            // Trace the first message using the embedded Trace to get the Correlation Id and User Id out.
            Trace($"Exception: {exception.Message}");

            // From here on use the tracing service trace
            Trace(exception.StackTrace);

            var faultException = exception as FaultException<OrganizationServiceFault>;
            if (faultException?.Detail == null) return;

            Trace($"Error Code: {faultException.Detail.ErrorCode}");
            Trace($"Detail Message: {faultException.Detail.Message}");
            if (!string.IsNullOrEmpty(faultException.Detail.TraceText))
            {
                Trace("Trace: ");
                Trace(faultException.Detail.TraceText);
            }

            if (faultException.Detail.ErrorDetails.Count > 0) Trace("Error Details: ");

            foreach (var item in faultException.Detail.ErrorDetails) Trace($"{item.Key, 20} = {item.Value}");

            if (faultException.Detail.InnerFault == null) return;

            Trace(new FaultException<OrganizationServiceFault>(faultException.Detail.InnerFault));
        }

        /// <inheritdoc />
        public void Trace(string format, params object[] args)
        {
            if (args != null) Trace(null, format, args);
        }
    }
}