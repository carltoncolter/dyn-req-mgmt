/// <reference path="../../node_modules/@types/xrm/index.d.ts" />
/// <reference path="../../node_modules/@types/jquery/index.d.ts" />

namespace FDBZAP {
    export namespace Common {
        export namespace Buttons {
            export class ButtonController {
                public static StaticController = new ButtonController();
                public Buttons: any = {};

                public AddButton(config: IButtonConfig) {
                    this.Buttons[config.buttonid] = config;

                    // wait and then wait for setupButton to push configuration
                    // $(document).ready(() => {
                    $(() => {
                        if (config && config.getFormContext) {
                            const control = config.getFormContext().ui.controls.get(config.webresourceid);
                            if (control) {
                                const setupButton = () => {
                                    const frame = (control as any).getObject();
                                    if (frame) {
                                        const contentWindow = frame.contentWindow;
                                        if (contentWindow && contentWindow.setupButton) {
                                            contentWindow.setupButton(config);
                                            config.isbound = true;
                                        } else {
                                            // no contentWindow or setupButton method
                                            setTimeout(setupButton, 2);
                                        }
                                    } else {
                                        // no frame
                                        setTimeout(setupButton, 2);
                                    }
                                };
                                setupButton();
                            }
                        }
                    });
                }

                public EnableButton(id: string) {
                    try {
                        const btn = this.Buttons[id];
                        (btn.enable as () => void)();
                    } catch (err) {
                        alert("The button [" + id + "] was not found or initialized.");
                    }
                }

                public DisableButton(id: string) {
                    try {
                        const btn = this.Buttons[id];
                        (btn.disable as () => void)();
                    } catch (err) {
                        alert("The button [" + id + "] was not found or initialized.");
                    }
                }
            }

            export interface IButtonConfig {
                isbound?: boolean;
                label?: string;
                tooltip?: string;
                onClick?: null | (() => void);
                image?: null | string;
                buttonid: string;
                webresourceid: string;
                width?: "auto" | number | null;
                enabled?: boolean;
                data?: any; // for use with click event

                getFormContext?: () => Xrm.FormContext;
                Bind?: () => void;
                Enable?: () => void;
                Disable?: () => void;
                enable?: null | (() => void);
                disable?: null | (() => void);
            }

            export class ButtonConfig implements IButtonConfig {
                public isbound: boolean = false;
                public label: string = "Button";
                public tooltip: string = "Button";
                public onClick: null | (() => void) = null;
                public image: null | string = null;
                public buttonid: string = "embeddedbutton1";
                public webresourceid: string = "WebResource_button_something";
                public width: "auto" | number | null = "auto";
                public enabled: boolean = true;
                public enable: null | (() => void) = null; // set by html page
                public disable: null | (() => void) = null; // set by html page
                public data: any = null;

                private buttonController: ButtonController;
                private formContext: Xrm.FormContext;
                constructor(context: Xrm.FormContext, options: IButtonConfig | null = null, controller: null | ButtonController = null) {
                    const me = this;
                    me.formContext = context;
                    if (!controller) {
                        me.buttonController = Buttons.ButtonController.StaticController;
                    } else {
                        me.buttonController = controller as ButtonController;
                    }

                    if (options) {
                        Object.keys(options).forEach((key) => {
                            if (jQuery.inArray(key, ["isbound", "enable", "disable", "Enable", "Disable", "Bind", "getFormContext", "formContext", "buttonController"]) !== -1) {
                                return;
                            }
                            if (options && (options as any)[key]) {
                                (me as any)[key] = (options as any)[key];
                            }
                        });
                    }
                }

                public getFormContext() {
                    return this.formContext;
                }

                public Bind() {
                    this.buttonController.AddButton(this);
                }

                public Enable() {
                    this.buttonController.EnableButton(this.buttonid);
                }

                public Disable() {
                    this.buttonController.DisableButton(this.buttonid);
                }
            }
        }
    }
}
