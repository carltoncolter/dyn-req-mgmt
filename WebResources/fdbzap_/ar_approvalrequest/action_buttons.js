var FDBZAP;
(function (FDBZAP) {
    let ApprovalRequest;
    (function (ApprovalRequest) {
        class ActionButtons {
            static GetActions(context, webresourceId) {
                const formContext = context.getFormContext();
                const approvalrequestid = formContext.data.entity.getId().substr(1, 36);
                function clickEvent() {
                    const buttonid = $(this).attr("id");
                    const controller = FDBZAP.Common.Buttons.ButtonController.StaticController;
                    const buttonConfig = controller.Buttons[(buttonid)];
                    if (buttonConfig) {
                        const actionid = buttonConfig.data["fdbzap_ar_actionid"];
                        const attrActionTaken = formContext.data.entity.attributes.get("fdbzap_ar_actiontaken");
                        const attrActionTakenOn = formContext.data.entity.attributes.get("fdbzap_ar_actiontakenon");
                        attrActionTaken.setValue([{
                                entityType: "fdbzap_ar_action",
                                id: actionid,
                                name: buttonConfig.data["fdbzap_ar_name"],
                            }]);
                        attrActionTakenOn.setValue(new Date());
                        buttonConfig.disable();
                        Object.keys(controller.Buttons).forEach((key) => {
                            const currentButton = controller.Buttons[key];
                            if (currentButton) {
                                if (currentButton.enable && currentButton.buttonid !== buttonConfig.buttonid) {
                                    currentButton.enable();
                                }
                            }
                        });
                        const commentSection = formContext.ui.tabs.get("tab_approvalrequest").sections.get("comment_section");
                        commentSection.setVisible(buttonConfig.data["fdbzap_ar_allow_comments"] || buttonConfig.data["fdbzap_ar_requirecomment"]);
                        let level = "none";
                        if (buttonConfig.data["fdbzap_ar_requirecomment"]) {
                            level = "required";
                        }
                        const commentAttr = formContext.data.entity.attributes.get("fdbzap_ar_action_comment");
                        commentAttr.setRequiredLevel((level));
                        if (!buttonConfig.data["fdbzap_ar_allow_comments"] && !buttonConfig.data["fdbzap_ar_requirecomment"]) {
                            commentAttr.setValue("");
                        }
                    }
                }
                const clientUrl = context.getContext().getClientUrl();
                const attributes = [
                    "_fdbzap_ar_approvalrequest_value",
                    "fdbzap_ar_actionid",
                    "fdbzap_ar_button_label",
                    "fdbzap_ar_button_order",
                    "fdbzap_ar_button_tooltip",
                    "fdbzap_ar_customactioncode",
                    "fdbzap_ar_endapprovalprocesswhenclicked",
                    "fdbzap_ar_priority",
                    "fdbzap_ar_allow_comments",
                    "fdbzap_ar_requirecomment",
                    "fdbzap_ar_name",
                ];
                const filter = "&$filter=_fdbzap_ar_approvalrequest_value eq " + approvalrequestid;
                const order = "&$orderby=fdbzap_ar_button_order asc,fdbzap_ar_priority asc,fdbzap_ar_name asc";
                const url = clientUrl + "/api/data/v9.0/fdbzap_ar_actions?$select=" + attributes.join(",") + filter + order;
                const method = "GET";
                FDBZAP.Common.Tools.RequestHelper.Request("GET", url)
                    .done(function () {
                    const results = this;
                    const records = [];
                    const buttons = [];
                    for (let i = 0; i < results.json.value.length; i++) {
                        const data = results.json.value[i];
                        const record = {};
                        const button = new FDBZAP.Common.Buttons.ButtonConfig(formContext, {
                            buttonid: "button-" + data["fdbzap_ar_customactioncode"] + i,
                            label: data["fdbzap_ar_button_label"],
                            onClick: clickEvent,
                            tooltip: data["fdbzap_ar_button_tooltip"],
                            webresourceid: webresourceId,
                        });
                        record["_fdbzap_ar_approvalrequest_value"] = data["_fdbzap_ar_approvalrequest_value"];
                        record["fdbzap_ar_button_label"] = data["fdbzap_ar_button_label"];
                        record["fdbzap_ar_button_order"] = data["fdbzap_ar_button_order"];
                        record["fdbzap_ar_button_tooltip"] = data["fdbzap_ar_button_tooltip"];
                        record["fdbzap_ar_customactioncode"] = data["fdbzap_ar_customactioncode"];
                        record["fdbzap_ar_endapprovalprocesswhenclicked"] = data["fdbzap_ar_endapprovalprocesswhenclicked"];
                        record["fdbzap_ar_requirecomment"] = data["fdbzap_ar_requirecomment"];
                        record["fdbzap_ar_name"] = data["fdbzap_ar_name"];
                        record["fdbzap_ar_actionid"] = data["fdbzap_ar_actionid"];
                        record["fdbzap_ar_allow_comments"] = data["fdbzap_ar_allow_comments"];
                        button.data = record;
                        button.Bind();
                    }
                })
                    .fail(function () {
                    Xrm.Navigation.openAlertDialog({ text: this.data });
                });
            }
        }
        ApprovalRequest.ActionButtons = ActionButtons;
    })(ApprovalRequest = FDBZAP.ApprovalRequest || (FDBZAP.ApprovalRequest = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=action_buttons.js.map