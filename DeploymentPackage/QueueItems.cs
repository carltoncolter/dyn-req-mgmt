using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Newtonsoft.Json;

namespace FedBizApps.RequestManagement.DeploymentPackage
{
    internal class QueueItems
    {
        // ReSharper disable once InconsistentNaming
        internal static int QUEUEITEMBATCHSIZE = 50;

        internal static void AddToQueue(TraceLogger logger, CrmServiceClient service, string filepath)
        {
            const string SEPERATOR = "------------------------------------------------------";
            logger.Log(SEPERATOR, TraceEventType.Information);
            
            logger.Log("Starting to queue items... this may take a while... ", TraceEventType.Information);

            if (!File.Exists(filepath))
            {
                logger.Log(new FileNotFoundException($"Unable to locate queueitems.json at $filepath"));
                logger.Log(SEPERATOR, TraceEventType.Information);
                return;
            }
            var json = File.ReadAllText(filepath);

            if (string.IsNullOrWhiteSpace(json))
            {
                logger.Log(new FileLoadException("Queueitems.json was empty."));
                logger.Log(SEPERATOR, TraceEventType.Information);
                return;
            }

            try
            {
                dynamic data = JsonConvert.DeserializeObject(json);

                var requests = new OrganizationRequestCollection();

                if (data == null)
                {
                    logger.Log("Json data was null", TraceEventType.Information);
                    logger.Log(SEPERATOR, TraceEventType.Information);
                    return;
                }

                int i = 0, j = 0;
                foreach (var queueitem in data)
                {
                    i++;
                    if (i >= QUEUEITEMBATCHSIZE && i % QUEUEITEMBATCHSIZE == 0) j++;

                    var request = new AddToQueueRequest();

                    if (Guid.TryParse(queueitem.queueid.ToString(), out Guid queueid))
                    {
                        if (Guid.TryParse(queueitem.targetid.ToString(), out Guid targetid))
                        {
                            if (!String.IsNullOrEmpty(queueitem.targetentity.ToString()))
                            {
                                try
                                {
                                    request.Target = new EntityReference(queueitem.targetentity.ToString(), targetid);
                                    request.DestinationQueueId = queueid;
                                    request.QueueItemProperties = new Entity("queueitem");
                                    requests.Add(request);

                                    if (requests.Count >= QUEUEITEMBATCHSIZE)
                                    {
                                        ExecuteMultipleRequests(logger, service, requests, j);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Log("Unable to queue current item.");
                                    logger.Log(ex);
                                }
                            }
                            else
                            {
                                logger.Log($"Item {i} - There was an empty target entity in the queueitems.json",
                                    TraceEventType.Information);
                            }
                        }
                        else
                        {
                            logger.Log($"Item {i} - Unable to parse targetid guid: {queueitem.targetid.ToString()}",
                                TraceEventType.Information);
                        }
                    }
                    else
                    {
                        logger.Log($"Item {i} - Unable to parse queueid guid: {queueitem.queueid.ToString()}",
                            TraceEventType.Information);
                    }
                }

                if (requests.Count>0)
                {
                    ExecuteMultipleRequests(logger, service, requests, j);
                }

            }
            catch (Exception ex)
            {
                logger.Log("Unable to queue items.");
                logger.Log(ex);
            }
            finally
            {
                logger.Log(SEPERATOR, TraceEventType.Information);
            }
        }

        private static void ExecuteMultipleRequests(TraceLogger logger, CrmServiceClient service,
            OrganizationRequestCollection requests, int batchIndex)
        {
            var response = (ExecuteMultipleResponse) service.Execute(new ExecuteMultipleRequest
            {
                Requests = requests,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            });
            if (response.IsFaulted)
            {
                foreach (var item in response.Responses)
                {
                    var itemIndex = (batchIndex * QUEUEITEMBATCHSIZE) + item.RequestIndex;

                    var itemRequest = (AddToQueueRequest) requests[item.RequestIndex];

                    logger.Log(
                        $"Item {itemIndex} - There was an error queuing the item with the id:{itemRequest.Target} to queue:{itemRequest.DestinationQueueId}",
                        TraceEventType.Information);
                    logger.Log(item.Fault.Message);
                }
            }

            requests.Clear();
        }
    }
}
