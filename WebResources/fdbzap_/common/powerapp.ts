namespace FDBZAP
{
    export class PowerApp
    {
        public static SetApprovalSrc(context: Xrm.Events.EventContext, webResourceName: string)
        {
            const form = context.getFormContext();
            PowerApp.SetSrc(form, webResourceName, FDBZAP.Settings.ApprovalPowerAppGuid, {
                ID:  form.data.entity.attributes.get("fdbzap_ar_requestid").getValue(),
            });
        }
        public static SetSrc(form: Xrm.FormContext, webResourceName: string, appid: string, parms: any)
        {
            const iCtrl: Xrm.Controls.FramedControl = form.ui.controls.get(webResourceName) as Xrm.Controls.FramedControl;

            if (iCtrl) {
                parms.hideNavBar = "false";

                const urlparams: string = Object.keys(parms).map((k) => {
                    return encodeURIComponent(k) + "=" + encodeURIComponent(parms[k]);
                }).join("&");
                
                const url = "https://web.powerapps.com/webplayer/iframeapp?source=iframe&appId=/providers/Microsoft.PowerApps/apps/" + appid + "&" + urlparams;

                iCtrl.setSrc(url);
            }
        }
    }
}
