var FDBZAP;
(function (FDBZAP) {
    let Search;
    (function (Search) {
        // recursive searcher of objects...
        function locate(container, findKey, ischild, iskey) {
            function ismatch(k, m, o, iskey) {
                if (k !== m) {
                    return false;
                }
                return (iskey && iskey(o)) || (!iskey);
            }
            const stack = [];
            stack.push(container);
            do {
                const stackItem = stack.pop();
                for (const key in stackItem) {
                    if (stackItem.hasOwnProperty(key)) {
                        const item = stackItem[key];
                        if (ismatch(key, findKey, item, iskey)) {
                            return { found: true, value: stackItem[key] };
                        }
                        if (typeof (item) === "object") {
                            if (ischild && !ischild(item)) {
                                continue;
                            }
                            stack.push(item);
                        }
                    }
                }
            } while (stack.length > 0);
            return { found: false };
        }
        Search.locate = locate;
    })(Search = FDBZAP.Search || (FDBZAP.Search = {}));
    // The below code may look a bit odd at first glance, so here is an explanation.  $ is jQuery, but it isn't always
    // in your frame, but you still need/want jQuery, so this will recursively search for it using the stack.
    let _$ = null;
    function getJQuery() {
        // This does not work in firefox.... *sad*
        // TODO: Figure out a way to check the origin to avoid CORS issues.
        if (_$) {
            return _$;
        }
        if (window.$) {
            _$ = window.$;
            return _$;
        }
        const result = Search.locate(top, "$", (o) => {
            return !!(o && o.setInterval && o.self) && o === o.self;
        }, (o) => {
            return typeof o === "function" && !!o().jquery;
        });
        if (result.found) {
            return _$ = result.value;
        }
        else {
            throw new Error("Unable to locate jQuery");
        }
    }
    FDBZAP.getJQuery = getJQuery;
    let Common;
    (function (Common) {
        let Tools;
        (function (Tools) {
            function getFormFactor() {
                const clientContext = Xrm.Utility.getGlobalContext().client;
                return clientContext.getFormFactor();
            }
            Tools.getFormFactor = getFormFactor;
            function getWebResourceBaseUrl() {
                const globalContext = Xrm.Utility.getGlobalContext();
                return globalContext.getClientUrl() + "/WebResources/fdbzap_/";
            }
            Tools.getWebResourceBaseUrl = getWebResourceBaseUrl;
            function newGuid() {
                // ReSharper disable once UnusedParameter
                let s4 = (i) => {
                    // tslint:disable-next-line:no-bitwise
                    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
                };
                const valArray = new Uint16Array(8);
                const crypto = window.crypto || window.msCrypto || {};
                if (crypto.hasOwnProperty("getRandomValues")) {
                    crypto.getRandomValues(valArray);
                    s4 = (i) => {
                        let v = valArray[i].toString(16);
                        while (v.length < 4) {
                            v = `0${v}`;
                        }
                        return v;
                    };
                }
                // uses secure crypto when available
                return `${s4(0)}${s4(1)}-${s4(2)}-${s4(3)}-${s4(4)}-${s4(5)}${s4(6)}${s4(7)}`;
            }
            Tools.newGuid = newGuid;
            // Work with David Parry to put this in
            function generateId(context, prefix, seperator, suffixLength, fieldname, lockfield) {
                const id = [prefix, new Date().toISOString().substring(2, 10).replace(/-/g, "")];
                let rstr = "";
                const chars = "023456789ABCDEFGHJKMNPQRSTUVWXYZ";
                for (let i = 0; i < (suffixLength || 5); i++) {
                    rstr += chars[Math.floor(Math.random() * chars.length)];
                }
                id.push(rstr);
                const genId = id.join(seperator || "-");
                if (fieldname && context) {
                    const formContext = context.getFormContext();
                    const attribute = formContext.data.entity.attributes.get(fieldname);
                    if (attribute) {
                        if ((attribute.getValue() || "").trim().length === 0) {
                            attribute.setValue(genId);
                        }
                        if (lockfield) {
                            const control = formContext.ui.controls.get(fieldname);
                            if (control) {
                                control.setDisabled(true);
                            }
                        }
                    }
                }
                return genId;
            }
            Tools.generateId = generateId;
            class Page {
                static getPage() {
                    if (Page.getPageCache === 0) {
                        let search = location.search;
                        let url = location.href;
                        let page = "";
                        if (search === "") {
                            const pos = url.toLowerCase().indexOf("%3f");
                            if (pos > -1) {
                                search = "?" + decodeURIComponent(url.substring(pos + 3));
                                page = url.substring(0, pos);
                                url = page + search;
                            }
                        }
                        else {
                            page = url.substring(0, url.indexOf("?"));
                        }
                        Page.getPageCache = {
                            page: (page),
                            search: (search),
                            url: (url),
                        };
                    }
                    return Page.getPageCache;
                }
                static buildURL(baseUrl, querystring) {
                    let url = baseUrl;
                    if (url === "") {
                        url = Page.getPage().url;
                    }
                    if (querystring.length > 0) {
                        if (querystring[0] !== "?") {
                            url = url + "?";
                        }
                        url = url + querystring;
                    }
                    return url;
                }
                static getUrlParams() {
                    if (Page.getUrlParamsCache === 0) {
                        const result = {};
                        const search = Page.getPage().search;
                        const paramArray = search.substr(1).split("&");
                        for (const parameter of paramArray) {
                            if (parameter[0].toLowerCase() !== "data") {
                                result[parameter[0].toLowerCase()] = decodeURIComponent(parameter[1]);
                            }
                            else {
                                const dResult = {};
                                let data = parameter[1];
                                if (parameter.length > 2) {
                                    data = "";
                                    for (let n = 1; n < parameter.length; n = n + 2) {
                                        data = data + parameter[n] + "=" + parameter[n + 1] + "&";
                                    }
                                    data = data.substring(0, data.length - 1);
                                }
                                const dataArray = decodeURIComponent(data).split("&");
                                for (const statement of dataArray) {
                                    const parsed = statement.split("=");
                                    dResult[parsed[0].toLowerCase()] = parsed[1];
                                }
                                result["data"] = dResult;
                            }
                        }
                        Page.getUrlParamsCache = result;
                    }
                    return Page.getUrlParamsCache;
                }
                static getQueryParameter(key) {
                    const cleanedKey = key.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
                    const regex = new RegExp("[\\?&]" + cleanedKey + "=([^&#]*)");
                    const results = regex.exec(Page.getPage().search); // was location.search
                    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
                }
                static getDataParameter(dataValue) {
                    let dataParameter = null;
                    if (dataValue !== "" && dataValue) {
                        dataParameter = {};
                        const vals = decodeURIComponent(dataValue).split("&");
                        for (const val of vals) {
                            const splitVal = val.replace(/\+/g, " ").split("=");
                            dataParameter[splitVal[0]] = splitVal[1];
                        }
                    }
                    return dataParameter;
                }
            }
            Page.getPageCache = 0;
            Page.getUrlParamsCache = 0;
            Tools.Page = Page;
            class Format {
                static date(date) {
                    let monthString;
                    const rawMonth = (date.getMonth() + 1).toString();
                    if (rawMonth.length === 1) {
                        monthString = "0" + rawMonth;
                    }
                    else {
                        monthString = rawMonth;
                    }
                    let dateString;
                    const rawDate = date.getDate().toString();
                    if (rawDate.length === 1) {
                        dateString = "0" + rawDate;
                    }
                    else {
                        dateString = rawDate;
                    }
                    return "datetime\'" + date.getFullYear() + "-" + monthString + "-" + dateString + "T00:00:00Z\'";
                }
                static dateTime(date) {
                    let monthString;
                    const rawMonth = (date.getMonth() + 1).toString();
                    if (rawMonth.length === 1) {
                        monthString = "0" + rawMonth;
                    }
                    else {
                        monthString = rawMonth;
                    }
                    let dateString;
                    const rawDate = date.getDate().toString();
                    if (rawDate.length === 1) {
                        dateString = "0" + rawDate;
                    }
                    else {
                        dateString = rawDate;
                    }
                    return "datetime\'" + date.getFullYear() + "-" + monthString + "-" + dateString +
                        "T" + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds() + "Z\'";
                }
                static telephone(inphone) {
                    if (!inphone) {
                        return "";
                    }
                    let phone = inphone;
                    let ext = "";
                    if (0 !== phone.indexOf("+")) {
                        if (1 < phone.lastIndexOf("x")) {
                            ext = phone.slice(phone.lastIndexOf("x"));
                            phone = phone.slice(0, phone.lastIndexOf("x"));
                        }
                        phone = phone.replace(/[^\d]/gi, "");
                        let result = phone;
                        if (7 === phone.length) {
                            result = phone.slice(0, 3) + "-" + phone.slice(3);
                        }
                        if (10 === phone.length) {
                            result = "(" + phone.slice(0, 3) + ") " + phone.slice(3, 6) + "-" + phone.slice(6);
                        }
                        if (11 === phone.length) {
                            result = phone.slice(0, 1) + " (" + phone.slice(1, 4) + ") " + phone.slice(4, 7) + "-" + phone.slice(7);
                        }
                        if (0 < ext.length) {
                            result = result + " " + ext;
                        }
                        return result;
                    }
                    return inphone;
                }
            }
            Tools.Format = Format;
            let RequestHelper;
            (function (RequestHelper) {
                RequestHelper.DEFAULT_SETTINGS = {
                    headers: {
                        "Accept": "application/json",
                        "Content-Type": "application/json; charset=utf-8",
                        "OData-MaxVersion": "4.0",
                        "OData-Version": "4.0",
                        "Prefer": "odata.include-annotations=\"*\"",
                    },
                    ignoreCache: false,
                    // default max duration for a request
                    timeout: 5000,
                };
                function Request(method, url, body = null, options = RequestHelper.DEFAULT_SETTINGS) {
                    const result = $.Deferred();
                    const createResponse = (xhr, data, fail) => {
                        let ok = xhr.status >= 200 && xhr.status < 300;
                        if (fail) {
                            ok = false;
                        }
                        const result = {
                            data: (data),
                            headers: xhr.getAllResponseHeaders(),
                            json: JSON.parse(data),
                            ok: (ok),
                            status: xhr.status,
                            statusText: xhr.statusText,
                        };
                        return result;
                    };
                    const settings = $.extend(true, RequestHelper.DEFAULT_SETTINGS, options);
                    const xhr = new XMLHttpRequest();
                    xhr.open(method, url);
                    if (settings.headers) {
                        Object.keys(settings.headers).forEach((key) => xhr.setRequestHeader(key, settings.headers[key]));
                    }
                    if (settings.ignoreCache) {
                        xhr.setRequestHeader("Cache-Control", "no-cache");
                    }
                    xhr.onload = (evt) => {
                        result.resolveWith(createResponse(xhr, xhr.response, false));
                    };
                    xhr.onerror = (evt) => {
                        result.rejectWith(createResponse(xhr, "Failed to make request.", true));
                    };
                    xhr.timeout = settings.timeout;
                    xhr.ontimeout = (evt) => {
                        result.rejectWith(createResponse(xhr, "Failed to make request.", true));
                    };
                    if ((method === "post" || method === "POST") && body) {
                        xhr.setRequestHeader("Content-Type", "application/json");
                        xhr.send(JSON.stringify(body));
                    }
                    else {
                        xhr.send();
                    }
                    return result.promise();
                }
                RequestHelper.Request = Request;
            })(RequestHelper = Tools.RequestHelper || (Tools.RequestHelper = {}));
        })(Tools = Common.Tools || (Common.Tools = {}));
    })(Common = FDBZAP.Common || (FDBZAP.Common = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=tools.js.map