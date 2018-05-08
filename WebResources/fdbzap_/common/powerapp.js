var FDBZAP;
(function (FDBZAP) {
    class PowerApp {
        static SetApprovalSrc(context, webResourceName) {
            const form = context.getFormContext();
            PowerApp.SetSrc(form, webResourceName, FDBZAP.Settings.ApprovalPowerAppGuid, {
                ID: form.data.entity.attributes.get("fdbzap_ar_requestid").getValue(),
            });
        }
        static SetSrc(form, webResourceName, appid, parms) {
            const iCtrl = form.ui.controls.get(webResourceName);
            if (iCtrl) {
                parms.hideNavBar = "false";
                const urlparams = Object.keys(parms).map((k) => {
                    return encodeURIComponent(k) + "=" + encodeURIComponent(parms[k]);
                }).join("&");
                const url = "https://web.powerapps.com/webplayer/iframeapp?source=iframe&appId=/providers/Microsoft.PowerApps/apps/" + appid + "&" + urlparams;
                iCtrl.setSrc(url);
            }
        }
    }
    FDBZAP.PowerApp = PowerApp;
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=powerapp.js.map