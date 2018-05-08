/// <reference path="../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../node_modules/@types/jquery/index.d.ts" />
var FDBZAP;
(function (FDBZAP) {
    let Common;
    (function (Common) {
        let Buttons;
        (function (Buttons) {
            class ButtonController {
                constructor() {
                    this.Buttons = {};
                }
                AddButton(config) {
                    this.Buttons[config.buttonid] = config;
                    // wait and then wait for setupButton to push configuration
                    // $(document).ready(() => {
                    $(() => {
                        if (config && config.getFormContext) {
                            const control = config.getFormContext().ui.controls.get(config.webresourceid);
                            if (control) {
                                const setupButton = () => {
                                    const frame = control.getObject();
                                    if (frame) {
                                        const contentWindow = frame.contentWindow;
                                        if (contentWindow && contentWindow.setupButton) {
                                            contentWindow.setupButton(config);
                                            config.isbound = true;
                                        }
                                        else {
                                            // no contentWindow or setupButton method
                                            setTimeout(setupButton, 2);
                                        }
                                    }
                                    else {
                                        // no frame
                                        setTimeout(setupButton, 2);
                                    }
                                };
                                setupButton();
                            }
                        }
                    });
                }
                EnableButton(id) {
                    try {
                        const btn = this.Buttons[id];
                        btn.enable();
                    }
                    catch (err) {
                        alert("The button [" + id + "] was not found or initialized.");
                    }
                }
                DisableButton(id) {
                    try {
                        const btn = this.Buttons[id];
                        btn.disable();
                    }
                    catch (err) {
                        alert("The button [" + id + "] was not found or initialized.");
                    }
                }
            }
            ButtonController.StaticController = new ButtonController();
            Buttons.ButtonController = ButtonController;
            class ButtonConfig {
                constructor(context, options = null, controller = null) {
                    this.isbound = false;
                    this.label = "Button";
                    this.tooltip = "Button";
                    this.onClick = null;
                    this.image = null;
                    this.buttonid = "embeddedbutton1";
                    this.webresourceid = "WebResource_button_something";
                    this.width = "auto";
                    this.enabled = true;
                    this.enable = null; // set by html page
                    this.disable = null; // set by html page
                    this.data = null;
                    const me = this;
                    me.formContext = context;
                    if (!controller) {
                        me.buttonController = Buttons.ButtonController.StaticController;
                    }
                    else {
                        me.buttonController = controller;
                    }
                    if (options) {
                        Object.keys(options).forEach((key) => {
                            if (jQuery.inArray(key, ["isbound", "enable", "disable", "Enable", "Disable", "Bind", "getFormContext", "formContext", "buttonController"]) !== -1) {
                                return;
                            }
                            if (options && options[key]) {
                                me[key] = options[key];
                            }
                        });
                    }
                }
                getFormContext() {
                    return this.formContext;
                }
                Bind() {
                    this.buttonController.AddButton(this);
                }
                Enable() {
                    this.buttonController.EnableButton(this.buttonid);
                }
                Disable() {
                    this.buttonController.DisableButton(this.buttonid);
                }
            }
            Buttons.ButtonConfig = ButtonConfig;
        })(Buttons = Common.Buttons || (Common.Buttons = {}));
    })(Common = FDBZAP.Common || (FDBZAP.Common = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=Buttons.js.map