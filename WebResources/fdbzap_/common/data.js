// ReSharper disable InconsistentNaming
// tslint:disable:variable-name
var FDBZAP;
(function (FDBZAP) {
    let Common;
    (function (Common) {
        let Transport;
        (function (Transport) {
            let AjaxMethod;
            (function (AjaxMethod) {
                AjaxMethod["GET"] = "GET";
                AjaxMethod["PUT"] = "PUT";
                AjaxMethod["POST"] = "POST";
            })(AjaxMethod = Transport.AjaxMethod || (Transport.AjaxMethod = {}));
            let AjaxAccepts;
            (function (AjaxAccepts) {
                AjaxAccepts["*"] = "allTypes";
                AjaxAccepts["text"] = "text/plain";
                AjaxAccepts["html"] = "text/html";
                AjaxAccepts["xml"] = "application/xml, text/xml";
                AjaxAccepts["json"] = "application/json, text/javascript";
            })(AjaxAccepts = Transport.AjaxAccepts || (Transport.AjaxAccepts = {}));
            class AjaxResponse {
                constructor(options, data = null, xhr = null, error = null) {
                    this.error = error;
                    this.hasError = !!error;
                    this.xhr = xhr;
                    this.options = options;
                    this.data = data;
                }
            }
            Transport.AjaxResponse = AjaxResponse;
            class AjaxOptions {
                constructor(options = null) {
                    this.accepts = null;
                    this.async = true;
                    this.contentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    this.stringify = true;
                    if (!options) {
                        options = {};
                    }
                    this.method = options.method || AjaxMethod.GET;
                    this.headers = options.headers || {};
                    if (options.beforeSend) {
                        this.beforeSend = options.beforeSend;
                    }
                    this.data = options.data || null;
                    this.url = options.url;
                    if (options.async === false) {
                        this.async = false;
                    }
                    this.contentType = options.contentType || this.contentType;
                    this.accepts = options.accepts || null;
                    if (options.stringify === false) {
                        this.stringify = false;
                    }
                }
            }
            Transport.AjaxOptions = AjaxOptions;
            // xrmPost only allows async currently
            function xrmPost(options, data) {
                const opts = new AjaxOptions(options);
                opts.accepts = AjaxAccepts.json;
                opts.contentType = "application/json; charset=utf-8";
                opts.method = AjaxMethod.POST;
                opts.headers["OData-MaxVersion"] = "4.0";
                opts.headers["OData-Version"] = "4.0";
                opts.async = true;
                return this.ajax(opts, data);
            }
            Transport.xrmPost = xrmPost;
            function xrmGet(options, data) {
                const opts = new AjaxOptions(options);
                opts.accepts = AjaxAccepts.json;
                opts.method = AjaxMethod.GET;
                opts.headers["OData-MaxVersion"] = "4.0";
                opts.headers["OData-Version"] = "4.0";
                return this.ajax(opts, data);
            }
            Transport.xrmGet = xrmGet;
            function xrmFetch(options, pluralEntity, fetchXml) {
                const opts = new AjaxOptions(options);
                const encodedFetchXML = encodeURIComponent(fetchXml);
                const globalContext = Xrm.Utility.getGlobalContext();
                const clientUrl = globalContext.getClientUrl();
                opts.url = `${clientUrl}/api/data/v9.0/${pluralEntity}?fetchXml=${encodeURIComponent(fetchXml)}`;
                opts.stringify = false;
                return this.xrmGet(opts, null);
            }
            Transport.xrmFetch = xrmFetch;
            function ajax(options, data) {
                const opts = new AjaxOptions(options);
                opts.data = options.data || data;
                function execute(resolve, reject) {
                    if (!opts.url) {
                        const response = new AjaxResponse(options, null, null, new Error("URL Missing"));
                        if (reject) {
                            reject(response);
                        }
                        else {
                            return response;
                        }
                    }
                    if (opts.data && opts.stringify && typeof (opts.data) !== "string") {
                        opts.data = JSON.stringify(opts.data);
                    }
                    const xhr = new XMLHttpRequest();
                    // open endpoint
                    xhr.open(opts.method, opts.url, opts.async);
                    if (opts.contentType) {
                        opts.headers["Content-Type"] = opts.contentType;
                    }
                    if (opts.accepts) {
                        if (opts.accepts === AjaxAccepts["*"]) {
                            opts.headers["Accept"] = "*/".concat("*");
                        }
                        else {
                            opts.headers["Accept"] = opts.accepts;
                        }
                    }
                    for (const key in opts.headers) {
                        if (opts.headers.hasOwnProperty(key)) {
                            xhr.setRequestHeader(key, opts.headers[key]);
                        }
                    }
                    // execute before send
                    if (opts.beforeSend) {
                        if (opts.beforeSend.call(xhr, opts) === false) {
                            const response = new AjaxResponse(opts, null, xhr, Error("beforeSend failed"));
                            if (reject) {
                                reject(response);
                            }
                            else {
                                return response;
                            }
                        }
                    }
                    // send to endpoint
                    if (opts.async) {
                        // attach to readyStateChange
                        const readyState = (ev) => {
                            if (xhr.readyState === XMLHttpRequest.DONE || xhr.readyState === 4) {
                                xhr.removeEventListener("readystatechange", readyState);
                                if (xhr.status === 200 || xhr.status === 204) {
                                    // success callback this returns null since no return value available.
                                    let data = null;
                                    if (xhr.response) {
                                        data = JSON.parse(xhr.response);
                                    }
                                    if (resolve) {
                                        resolve(new AjaxResponse(options, data));
                                    }
                                }
                                else {
                                    // error callback
                                    let error = "There was an error.";
                                    try {
                                        error = JSON.parse(xhr.response).error;
                                    }
                                    catch (_a) { }
                                    const response = new AjaxResponse(options, xhr.response, xhr, new Error(error));
                                    if (reject) {
                                        reject(response);
                                    }
                                }
                            }
                        };
                        // attach listener
                        xhr.addEventListener("readystatechange", readyState);
                        // send request
                        if (opts.data) {
                            xhr.send(opts.data);
                        }
                        else {
                            xhr.send();
                        }
                    }
                    else {
                        if (opts.data) {
                            xhr.send(opts.data);
                        }
                        else {
                            // send synchronously
                            xhr.send();
                        }
                        if (xhr.status === 200 || xhr.status === 204) {
                            // synchronous success 
                            if (!xhr.response) {
                                // 204 - this may be empty
                                return new AjaxResponse(options, null);
                            }
                            return new AjaxResponse(options, JSON.parse(xhr.response));
                        }
                        else {
                            // synchronous error
                            let error = "There was an error.";
                            try {
                                error = JSON.parse(xhr.response).error;
                            }
                            catch (e) {
                            }
                            return new AjaxResponse(options, xhr.response, xhr, new Error(error));
                        }
                    }
                }
                if (opts.async) {
                    return new Promise((resolve, reject) => {
                        execute(resolve, reject);
                    });
                }
                else {
                    return execute();
                }
            }
            Transport.ajax = ajax;
        })(Transport = Common.Transport || (Common.Transport = {}));
        let Data;
        (function (Data) {
            class OptionSetValue {
                constructor() {
                    this.toJSON = () => {
                        return this.Value.toString();
                    };
                }
            }
            Data.OptionSetValue = OptionSetValue;
            class EntityReference {
                constructor(logicalName, id, name) {
                    this.toJSON = () => {
                        return `/${this.EntitySchemaName || this.LogicalName}(${this.Id.replace(/[{}]/g, "")})`;
                    };
                    this.LogicalName = logicalName || "";
                    this.Id = id || "";
                    this.Name = name;
                }
            }
            Data.EntityReference = EntityReference;
            class Entity {
                constructor(logicalName, id) {
                    this.toJSON = () => {
                        const json = [];
                        let id = `${this.LogicalName}id`;
                        if (["emailid", "letterid", "appointmentid", "faxid", "phonecallid"].indexOf(id) > -1) {
                            id = "activityid";
                        }
                        json.push(`"@odata.type":"Microsoft.Dynamics.CRM.${this.SchemaName || this.LogicalName}"`);
                        json.push(`"${id}":"${this.Id.replace(/[{}]/g, "")}"`);
                        for (let key in this.Attributes) {
                            if (this.Attributes.hasOwnProperty(key)) {
                                let value = this.Attributes[key];
                                if (value instanceof EntityReference) {
                                    key = `${key}@odata.bind`;
                                }
                                if (value.toJSON) {
                                    value = value.toJSON();
                                }
                                else {
                                    value = `"${String(value)}"`;
                                }
                                json.push(`"${key}":"${value}"`);
                            }
                        }
                        return `{${json.join(",")}}`;
                    };
                    this.LogicalName = logicalName || "";
                    this.Id = id || "";
                }
                toEntityReference() {
                    const er = new EntityReference(this.LogicalName, this.Id);
                    er.EntitySchemaName = this.SchemaName;
                    return er;
                }
                add(key, value) {
                    this.Attributes[key] = value;
                    return this;
                }
                remove(key) {
                    delete this.Attributes[key];
                    return this;
                }
            }
            Data.Entity = Entity;
            class EntityCollection {
                constructor(logicalName, entities) {
                    this.toJSON = () => {
                        const json = [];
                        for (const entity of this.Entities) {
                            json.push(entity.toJSON());
                        }
                        return `[${json.join(",")}]`;
                    };
                    this.EntityName = logicalName || "";
                    this.Entities = entities || new Array();
                }
                add(entity) {
                    this.Entities.push(entity);
                }
                remove(value) {
                    this.Entities.splice(this.Entities.indexOf(value), 1);
                }
                addSelectedItemReferences(items, setEntityType) {
                    items.forEach((i) => {
                        this.add(new Entity(i.TypeName, i.Id));
                    });
                    if (setEntityType && items.length > 0 && !this.EntityName) {
                        this.EntityName = items[0].TypeName;
                    }
                }
            }
            Data.EntityCollection = EntityCollection;
            function invokeProcessAction(action, parameters) {
                const globalContext = Xrm.Utility.getGlobalContext();
                const clientUrl = globalContext.getClientUrl();
                const json = [];
                let hasItemToStringify = false;
                for (const key in parameters) {
                    if (parameters.hasOwnProperty(key) && parameters[key].hasOwnProperty("toJSON")) {
                        json.push(`"${key}": ${parameters[key].toJSON()}`);
                        delete parameters[key];
                    }
                    else {
                        hasItemToStringify = true;
                    }
                }
                let jsonString = json.join(",");
                if (hasItemToStringify) {
                    jsonString = JSON.stringify(parameters).slice(1, -1) + "," + (jsonString || "");
                }
                jsonString = `{${jsonString}}`;
                return Transport.xrmPost({ url: `${clientUrl}/api/data/v9.0/${action}` }, jsonString);
            }
            Data.invokeProcessAction = invokeProcessAction;
            function executeWorkflow(workflowId, recordOrRecords, goodMsg = null, badMsg = null, messagePrefix = null) {
                if (!workflowId || !recordOrRecords) {
                    // nothing to run or nothing selected....
                    return;
                }
                const globalContext = Xrm.Utility.getGlobalContext();
                const clientUrl = globalContext.getClientUrl();
                const workflowQuery = "workflows(" + workflowId.replace("}", "").replace("{", "") + ")/Microsoft.Dynamics.CRM.ExecuteWorkflow";
                const url = clientUrl + "/api/data/v9.0/" + workflowQuery;
                const records = [];
                if (!Array.isArray(recordOrRecords)) {
                    if (recordOrRecords.hasOwnProperty("Id")) {
                        records.push(recordOrRecords.Id);
                    }
                    else {
                        records.push(recordOrRecords);
                    }
                }
                else {
                    (recordOrRecords).forEach((item) => {
                        if (recordOrRecords[0].TypeCode !== item.TypeCode) {
                            Xrm.Navigation.openAlertDialog({ text: "Some of the records were different types.  Please only select one type of record." });
                            return;
                        }
                        records.push(item.Id || item);
                    });
                }
                const workflowCalls = records.map((record) => () => Transport.xrmPost({ url }, { "EntityId": record }));
                const good = [];
                const bad = [];
                const run = (workflows) => workflows.reduce((prior, workflow) => prior.then(workflow).then((d) => good.push(d), (d) => bad.push(d)), Promise.resolve());
                const pluralize = (plural, m) => {
                    m = m.replace("{1}", plural ? "s" : "");
                    m = m.replace("{2}", plural ? "es" : "");
                    m = m.replace("{3}", plural ? "were" : "was");
                    m = m.replace("{4}", plural ? "they" : "it");
                    return m;
                };
                run(workflowCalls).then(() => {
                    let message = messagePrefix || "";
                    if (good.length > 0) {
                        const plural = good.length > 1;
                        // some successes occurred
                        if (goodMsg) {
                            message += pluralize(plural, goodMsg.replace("{0}", good.length.toString()));
                        }
                        else {
                            message += pluralize(plural, `  ${good.length} workflow{1} {3} successfully executed.`);
                        }
                    }
                    if (bad.length > 0) {
                        const plural = bad.length > 1;
                        // failures occurred
                        if (badMsg) {
                            message += pluralize(plural, badMsg.replace("{0}", bad.length.toString()));
                        }
                        else {
                            message += pluralize(plural, `  ${bad.length} workflow{1} {3} failed being executed.`);
                        }
                    }
                    Xrm.Navigation.openAlertDialog({ text: message });
                }, (bad) => {
                    console.log(bad);
                });
            }
            Data.executeWorkflow = executeWorkflow;
            let Workflows = {};
            function getWorkflowOptions(commandProperties, entityTypeCode, filter, removefilterprefix) {
                Workflows = {};
                const fetchData = {
                    category: "0",
                    componentstate: "0",
                    ondemand: "1",
                    primaryentity: entityTypeCode,
                    statecode: "1",
                    statuscode: "2",
                };
                const fetchXml = [
                    "<fetch top='10'>",
                    "<entity name='workflow'>",
                    "<attribute name='workflowid' />",
                    "<attribute name='name' />",
                    "<attribute name='description' />",
                    "<filter>",
                    "<condition attribute='primaryentity' operator='eq' value='", fetchData.primaryentity /*10011*/, "'/>",
                    "<condition attribute='statecode' operator='eq' value='", fetchData.statecode /*1*/, "'/>",
                    "<condition attribute='statuscode' operator='eq' value='", fetchData.statuscode /*2*/, "'/>",
                    "<condition attribute='componentstate' operator='eq' value='", fetchData.componentstate /*0*/, "'/>",
                    "<condition attribute='ondemand' operator='eq' value='", fetchData.ondemand /*1*/, "'/>",
                    "<condition attribute='category' operator='eq' value='", fetchData.category /*0*/, "'/>",
                    "<condition attribute='activeworkflowid' operator='not-null' />"
                ];
                if (filter) {
                    fetchXml.push(`<condition attribute='name' operator='begins-with' value='${filter}'/>`);
                }
                fetchXml.push("</filter>", "<order attribute='processorder' />", "<order attribute='name' />", "</entity>", "</fetch>");
                const result = Transport.xrmFetch({ async: false }, "workflows", fetchXml.join(""));
                // originally I was going to use Xrm.WebApi, but this function needs to run synchronously.
                // const result: any = new Promise((resolve, reject) => {
                //    Xrm.WebApi.retrieveMultipleRecords("workflow", "?fetchXml= " + fetchXml.join(""), 10).then(resolve, reject);
                // });
                const buttons = [];
                if (result && result.hasError) {
                    // failure
                    Xrm.Navigation.openAlertDialog({ text: "There was an error getting the list of workflows." });
                }
                else if (result && result.data && result.data.value && result.data.value.length > 0) {
                    // success - items found
                    for (let i = 0; i < result.data.value.length; i++) {
                        const entity = result.data.value[i];
                        const id = `fdbzap.workflow.button${i}`;
                        let name = entity.name.replace(/['"]+/g, "");
                        if (removefilterprefix) {
                            name = name.replace(filter, "");
                        }
                        Workflows[id] = entity.workflowid;
                        let button = `<Button Id="${id}" LabelText="${name}" Sequence="${(10 + i * 5)}" Command="fdbzap.workflow.run.command" `;
                        if (entity.description) {
                            button += `ToolTipTitle="Run Workflow" ToolTipDescription="${entity.description.replace(/['"]+/g, "")}" `;
                        }
                        button += "/>";
                        buttons.push(button);
                    }
                    // success
                }
                else {
                    // nothing to display
                    buttons.push(`<Button Id="fdbzap.workflow.button.donothing" Sequence="10" LabelText="No On Demand Workflows" Command="fdbzap.workflow.command.donothing"/>`);
                }
                const omenu = `<Menu Id="fdbzap.workflow.menu">`;
                const cmenu = `</Menu>`;
                const omenusection = `<MenuSection Id="fdbzap.workflow.menusection" Sequence="5">`;
                const cmenusection = `</MenuSection>`;
                const ocontrols = `<Controls Id="fdbzap.workflow.controls">`;
                const ccontrols = `</Controls>`;
                const popXml = omenu + omenusection + ocontrols + buttons.join("") + ccontrols + cmenusection + cmenu;
                // tslint:disable-next-line:no-debugger
                // debugger;
                commandProperties.PopulationXML = popXml;
            }
            Data.getWorkflowOptions = getWorkflowOptions;
            function executeWorkflowButton(commandProperties, record, records) {
                const id = commandProperties.SourceControlId;
                if (Workflows && Workflows[id]) {
                    executeWorkflow(Workflows[id], records || record);
                }
            }
            Data.executeWorkflowButton = executeWorkflowButton;
        })(Data = Common.Data || (Common.Data = {}));
    })(Common = FDBZAP.Common || (FDBZAP.Common = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=data.js.map