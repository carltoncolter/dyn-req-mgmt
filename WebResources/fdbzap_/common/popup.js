// Should we remove the Tools dependency?
var FDBZAP;
(function (FDBZAP) {
    let Common;
    (function (Common) {
        class Popup {
            constructor(config, content) {
                this.content = content;
                const settings = config || new Popup.Settings();
                this.config = Object.assign({}, settings);
            }
            render() {
                // set id
                this.config.id = this.config.id || `popup-${Common.Tools.newGuid()}`;
                // return promise
                return new Popup.PopupInstance(this.config, this.content).promise;
            }
        }
        Common.Popup = Popup;
        (function (Popup) {
            const topdoc = window.top.document;
            // Combined like vb MsgBox
            let ButtonLayout;
            (function (ButtonLayout) {
                ButtonLayout[ButtonLayout["OKOnly"] = 0] = "OKOnly";
                ButtonLayout[ButtonLayout["OKCancel"] = 1] = "OKCancel";
                ButtonLayout[ButtonLayout["AbortRetryIgnore"] = 2] = "AbortRetryIgnore";
                ButtonLayout[ButtonLayout["YesNoCancel"] = 3] = "YesNoCancel";
                ButtonLayout[ButtonLayout["YesNo"] = 4] = "YesNo";
                ButtonLayout[ButtonLayout["RetryCancel"] = 5] = "RetryCancel";
                ButtonLayout[ButtonLayout["NoButtons"] = 6] = "NoButtons";
            })(ButtonLayout = Popup.ButtonLayout || (Popup.ButtonLayout = {}));
            let ButtonType;
            (function (ButtonType) {
                ButtonType[ButtonType["Ok"] = 1] = "Ok";
                ButtonType[ButtonType["Cancel"] = 2] = "Cancel";
                ButtonType[ButtonType["Abort"] = 3] = "Abort";
                ButtonType[ButtonType["Retry"] = 4] = "Retry";
                ButtonType[ButtonType["Ignore"] = 5] = "Ignore";
                ButtonType[ButtonType["Yes"] = 6] = "Yes";
                ButtonType[ButtonType["No"] = 7] = "No";
            })(ButtonType = Popup.ButtonType || (Popup.ButtonType = {}));
            let DefaultButton;
            (function (DefaultButton) {
                DefaultButton[DefaultButton["DefaultButton1"] = 0] = "DefaultButton1";
                DefaultButton[DefaultButton["DefaultButton2"] = 256] = "DefaultButton2";
                DefaultButton[DefaultButton["DefaultButton3"] = 512] = "DefaultButton3";
            })(DefaultButton = Popup.DefaultButton || (Popup.DefaultButton = {}));
            let MessageType;
            (function (MessageType) {
                MessageType[MessageType["Normal"] = 0] = "Normal";
                MessageType[MessageType["Critical"] = 16] = "Critical";
                MessageType[MessageType["Question"] = 32] = "Question";
                MessageType[MessageType["Exclamation"] = 48] = "Exclamation";
                MessageType[MessageType["Information"] = 64] = "Information";
            })(MessageType = Popup.MessageType || (Popup.MessageType = {}));
            let MsgBoxType;
            (function (MsgBoxType) {
                MsgBoxType[MsgBoxType["Error"] = 0] = "Error";
                MsgBoxType[MsgBoxType["Warning"] = 1] = "Warning";
                MsgBoxType[MsgBoxType["Alert"] = 2] = "Alert";
                MsgBoxType[MsgBoxType["Confirmation"] = 3] = "Confirmation";
                MsgBoxType[MsgBoxType["Input"] = 4] = "Input";
                MsgBoxType[MsgBoxType["Loading"] = 5] = "Loading";
            })(MsgBoxType = Popup.MsgBoxType || (Popup.MsgBoxType = {}));
            function msgBox(textPrompt, buttons, options, msgBoxType) {
                options = options || new Popup.Settings();
                options.buttonLayout = buttons;
                options.showImage = options.showImage || options.showImage === null;
                if (msgBoxType) {
                    options.title = options.title || Popup.MsgBoxType[msgBoxType];
                }
                const prompt = topdoc.createElement("span");
                prompt.textContent = textPrompt;
                prompt.className = "msgboxPrompt popupText";
                return new Popup(options, prompt).render();
            }
            Popup.msgBox = msgBox;
            function inputBox(textPrompt, defaultValue, options) {
                options = options || new Popup.Settings();
                options.messageType = MessageType.Question;
                options.title = options.title || Popup.MsgBoxType[Popup.MsgBoxType.Input];
                options.id = `popup-${Common.Tools.newGuid()}`;
                const prompt = topdoc.createElement("span");
                prompt.textContent = textPrompt; /*! label */
                const input = topdoc.createElement("input");
                input.type = "text";
                input.name = "inputBoxValue";
                input.style.marginLeft = "10px";
                input.addEventListener("keypress", (e) => {
                    if (e.keyCode === 13 || e.code === "Enter") {
                        const popup = topdoc.getElementById(options.id);
                        const okbtn = popup.querySelector("#popupBtnOk");
                        okbtn.click();
                    }
                });
                const container = topdoc.createElement("span"); // why not div?
                container.appendChild(prompt);
                container.appendChild(input);
                return new Popup(options, container).render();
            }
            Popup.inputBox = inputBox;
            function loading(options) {
                options = options || new Popup.Settings();
                options.messageType = MessageType.Information;
                options.title = options.title || Popup.MsgBoxType[Popup.MsgBoxType.Loading];
                options.id = `popup-${Common.Tools.newGuid()}`;
                const label = topdoc.createElement("span");
                label.textContent = "Loading. . . ";
                label.style.fontSize = "3em";
                const img = topdoc.createElement("img");
                // Should we remove this dependency?
                img.src = Common.Tools.getWebResourceBaseUrl() + "img/popup/loading.gif";
                img.alt = "loading...";
                img.style.border = "none";
                const container = topdoc.createElement("span"); // why not div?
                container.appendChild(label);
                container.appendChild(img);
                return new Popup(options, container).render();
            }
            Popup.loading = loading;
            function errorMsg(message, options) {
                return this.msgBox(message, MessageType.Critical, options, Popup.MsgBoxType.Error);
            }
            Popup.errorMsg = errorMsg;
            function warning(message, options) {
                return this.msgBox(message, MessageType.Exclamation, options, Popup.MsgBoxType.Warning);
            }
            Popup.warning = warning;
            function alert(message, options) {
                return this.msgBox(message, MessageType.Normal + Popup.ButtonLayout.OKOnly, options, Popup.MsgBoxType.Alert);
            }
            Popup.alert = alert;
            function confirm(message, options) {
                return this.msgBox(message, MessageType.Normal + Popup.ButtonLayout.OKCancel, options, Popup.MsgBoxType.Confirmation);
            }
            Popup.confirm = confirm;
            function remove(popupid) {
                const r = (id) => {
                    const el = topdoc.getElementById(id);
                    if (el) {
                        el.parentNode.removeChild(el);
                    }
                };
                r(popupid);
                r("modal-" + popupid);
            }
            Popup.remove = remove;
            class ButtonClickEventData {
                constructor(clicked, values) {
                    this.clicked = clicked;
                    this.values = values;
                }
            }
            Popup.ButtonClickEventData = ButtonClickEventData;
            class Button {
                constructor(name, buttonType) {
                    this.name = name;
                    this.buttonType = buttonType;
                }
            }
            Popup.Button = Button;
            class ButtonSettings {
                constructor(combinedStyle) {
                    this.buttonSettings = combinedStyle;
                }
                get buttonSettings() {
                    return this.innerButtonSettings;
                }
                // tslint:disable:no-bitwise
                set buttonSettings(value) {
                    this.innerButtonSettings = value;
                    this.innerButtonLayout = value & ((1 << 4) - 1);
                    const styleWithoutButtonType = value & ~((1 << 4) - 1);
                    this.innerMessageType = styleWithoutButtonType & ((1 << 8) - 1);
                    this.innerDefaultButton = value & ~((1 << 8) - 1);
                }
                get buttonLayout() {
                    return this.innerButtonLayout;
                }
                set buttonLayout(value) {
                    this.innerButtonLayout = value;
                    this.calculateStyle();
                }
                get messageType() {
                    return this.innerMessageType;
                }
                set messageType(value) {
                    this.innerMessageType = value;
                    this.calculateStyle();
                }
                get defaultButton() {
                    return this.innerDefaultButton;
                }
                set defaultButton(value) {
                    this.innerDefaultButton = value;
                    this.calculateStyle();
                }
                calculateStyle() {
                    this.innerButtonSettings = this.innerButtonLayout + this.innerMessageType + this.innerDefaultButton;
                }
            }
            Popup.ButtonSettings = ButtonSettings;
            class ControlClickEventData {
                constructor(control, clickedButton) {
                    this.control = control;
                    this.controlId = this.control.innerControl.id;
                    this.clickedButton = clickedButton;
                }
            }
            Popup.ControlClickEventData = ControlClickEventData;
            class Settings extends ButtonSettings {
                constructor() {
                    super(1);
                    this.minWidth = 300;
                    this.minHeight = 140;
                    this.maxWidth = 600;
                    this.maxHeight = 600;
                    this.width = 0;
                    this.height = 0;
                    this.modal = false;
                }
            }
            Popup.Settings = Settings;
            class MouseData {
            }
            Popup.MouseData = MouseData;
            class PopupInstance extends Settings {
                constructor(config, content) {
                    super();
                    this.mouseMoveEvent = (e) => {
                        if (!this.mouseData.dragging) {
                            return;
                        }
                        const div = e.target;
                        this.innerControl.style.position = "absolute";
                        this.innerControl.style.left = `${e.pageX - this.mouseData.offsetLeft}px`;
                        this.innerControl.style.top = `${e.pageY - this.mouseData.offsetTop}px`;
                    };
                    this.mouseUpEvent = (e) => {
                        this.mouseData.dragging = false;
                        window.top.removeEventListener("mousemove", this.mouseMoveEvent);
                        this.innerControl.removeEventListener("mouseup", this.mouseUpEvent);
                    };
                    this.mouseDownOnTitleEvent = (e) => {
                        if (this.innerControl) {
                            this.mouseData.dragging = true;
                            this.mouseData.offsetTop = e.pageY - this.innerControl.offsetTop;
                            this.mouseData.offsetLeft = e.pageX - this.innerControl.offsetLeft;
                            // this.innerControl.attr("unselectable", "on").data("dragoffset", offset);
                            // movement is tracked on body
                            window.top.addEventListener("mousemove", this.mouseMoveEvent);
                            this.innerControl.addEventListener("mouseup", this.mouseUpEvent);
                        }
                    };
                    const settings = config || new Popup.Settings();
                    for (const key in settings) {
                        if (this.hasOwnProperty(key)) {
                            this[key] = settings[key];
                        }
                    }
                    const formFactor = Common.Tools.getFormFactor();
                    // Set Default Image Path for popup images
                    this.imagePath = this.imagePath || (Common.Tools.getWebResourceBaseUrl() + "img/popup");
                    this.promise = new Promise((pg, pb) => {
                        this.resolve = pg;
                        this.reject = pb;
                    });
                    const putInRange = (i, min, max) => Math.min(max, Math.max(i, min));
                    this.width = putInRange(this.width, this.minWidth, this.maxWidth);
                    this.height = putInRange(this.height, this.minHeight, this.maxHeight);
                    // Set Default Title
                    if (!this.title) {
                        switch (this.messageType) {
                            case Popup.MessageType.Normal:
                                this.title = "Message";
                                break;
                            case Popup.MessageType.Critical:
                                this.title = "Critical Message";
                                break;
                            case Popup.MessageType.Question:
                                this.title = "Question";
                                break;
                            case Popup.MessageType.Exclamation:
                                this.title = "Important Message";
                                break;
                            case Popup.MessageType.Information:
                                this.title = "Information";
                                break;
                            default:
                                this.title = "Message";
                                break;
                        }
                    }
                    // Setup default focus event handler
                    this.focus = () => {
                        const input = topdoc.querySelector("input, select, textarea");
                        if (input) {
                            input.focus();
                        }
                    };
                    // CONTENT DIV
                    // If content empty, set content to title
                    if (typeof content === "string") {
                        // Replace \r\n or \n with <br/>
                        content = content.replace(/\r?\n/g, "<br/>");
                    }
                    const promptDiv = topdoc.createElement("div");
                    if (content) {
                        if (typeof content === "string") {
                            promptDiv.innerHTML = content;
                        }
                        else {
                            promptDiv.appendChild(content);
                        }
                    }
                    else {
                        const prompt = topdoc.createElement("span");
                        prompt.textContent = this.title;
                        prompt.className = "msgboxPrompt popupText";
                        promptDiv.appendChild(prompt);
                    }
                    let contentDiv = topdoc.createElement("div");
                    contentDiv.className = "popupContent";
                    // contentDiv.style.height = `${this.height}px`;
                    contentDiv.appendChild(promptDiv);
                    // Show Image
                    if (this.showImage && this.messageType !== Popup.MessageType.Normal) {
                        const img = topdoc.createElement("img");
                        img.src = `${Popup.MessageType[this.messageType].toLowerCase()}_32.png`;
                        img.alt = this.title;
                        const contentLeft = topdoc.createElement("div");
                        contentLeft.id = "popupLeftContent";
                        contentLeft.style.cssFloat = "left";
                        contentLeft.style.width = "40px";
                        contentLeft.appendChild(img);
                        const contentRight = topdoc.createElement("div");
                        contentRight.id = "popupRightContent";
                        contentRight.style.width = "auto";
                        contentRight.style.marginLeft = "40px";
                        contentRight.appendChild(contentDiv);
                        contentDiv = [contentLeft, contentRight];
                    }
                    // BUILD CONTAINER
                    // TODO: Update this tosupport multiple view types
                    const innerDiv = topdoc.createElement("div");
                    innerDiv.className = "popup";
                    innerDiv.id = this.id;
                    switch (formFactor) {
                        case 4 /* Phone */:
                        case 2 /* Desktop */:
                        case 3 /* Tablet */:
                        case 1 /* Unknown */:
                            innerDiv.style.zIndex = "500";
                            innerDiv.style.position = "fixed";
                            innerDiv.style.top = "50%";
                            innerDiv.style.left = "50%";
                            innerDiv.style.marginLeft = `-${Math.floor(this.width / 2).toString()}px`;
                            innerDiv.style.marginTop = `-${Math.floor(this.height / 2).toString()}px`;
                            innerDiv.style.width = this.width.toString() + "px";
                            innerDiv.style.height = this.height.toString() + "px";
                            innerDiv.style.fontFamily = "Segoe UI, Tahoma, Arial";
                            innerDiv.style.fontSize = "11px";
                            innerDiv.style.color = "#000";
                            innerDiv.style.backgroundColor = "#fff";
                            innerDiv.style.border = "3px solid #000";
                        default:
                    }
                    this.innerControl = innerDiv;
                    // TITLE DIV
                    const titleDiv = topdoc.createElement("div");
                    titleDiv.className = `popupTitle popup${Popup.MessageType[this.messageType]}`;
                    titleDiv.id = "popupTitle";
                    titleDiv.textContent = this.title;
                    titleDiv.style.cursor = "move";
                    titleDiv.addEventListener("mousedown", this.mouseDownOnTitleEvent);
                    // BUTTON DIV
                    const buttonsDiv = topdoc.createElement("div");
                    buttonsDiv.className = "popupButtons";
                    const iControl = this;
                    switch (this.buttonLayout) {
                        case Popup.ButtonLayout.OKOnly:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Ok, this.defaultButton === DefaultButton.DefaultButton1));
                            break;
                        case Popup.ButtonLayout.AbortRetryIgnore:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Abort, this.defaultButton === DefaultButton.DefaultButton3));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Retry, this.defaultButton === DefaultButton.DefaultButton2));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Ignore, this.defaultButton === DefaultButton.DefaultButton1));
                            break;
                        case Popup.ButtonLayout.YesNoCancel:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Cancel, this.defaultButton === DefaultButton.DefaultButton3));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Yes, this.defaultButton === DefaultButton.DefaultButton2));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.No, this.defaultButton === DefaultButton.DefaultButton1));
                            break;
                        case Popup.ButtonLayout.YesNo:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Yes, this.defaultButton === DefaultButton.DefaultButton1));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.No, this.defaultButton === DefaultButton.DefaultButton2));
                            break;
                        case Popup.ButtonLayout.RetryCancel:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Retry, this.defaultButton === DefaultButton.DefaultButton1));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Cancel, this.defaultButton === DefaultButton.DefaultButton2));
                            break;
                        case Popup.ButtonLayout.NoButtons:
                            // No Buttons
                            break;
                        case Popup.ButtonLayout.OKCancel:
                        default:
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Cancel, this.defaultButton === DefaultButton.DefaultButton2));
                            buttonsDiv.appendChild(this.createButton(this, ButtonType.Ok, this.defaultButton === DefaultButton.DefaultButton1));
                            break;
                    }
                    this.innerControl.appendChild(titleDiv);
                    if (Array.isArray(contentDiv)) {
                        for (const el of contentDiv) {
                            this.innerControl.appendChild(el);
                        }
                    }
                    else {
                        this.innerControl.appendChild(contentDiv);
                    }
                    this.innerControl.appendChild(buttonsDiv);
                    const css = {
                        // 'selector': 'style'
                        "div.popupButtons": {
                            "backgroundColor": "#fff",
                            "bottom": "10px",
                            "color": "#000",
                            "fontSize": "11px",
                            "fontWeight": "600",
                            "position": "absolute",
                            "right": "10px",
                        },
                        "div.popupButtons button": {
                            "backgroundColor": "ButtonFace",
                            "border": "1px solid ButtonText",
                            "color": "ButtonText",
                            "cursor": "pointer",
                            "display": "inline-block",
                            "fontSize": "11px",
                            "height": "22px",
                            "margin": "3px 6px 2px",
                            "padding": "1px 15px",
                            "textAlign": "center",
                        },
                        "div.popupContent": {
                            "backgroundColor": "#fff",
                            "overflowX": "hidden",
                            "overflowY": "auto",
                            "padding": "10px",
                        },
                        "div.popupCritical": {
                            "backgroundColor": "#AE1F23",
                            "color": "#fff",
                        },
                        "div.popupExclamation": {
                            "backgroundColor": "#fc8f00",
                            "color": "#fff",
                        },
                        "div.popupInformation": {
                            "backgroundColor": "#0B5C9E",
                            "color": "#fff",
                        },
                        "div.popupNormal": {
                            "backgroundColor": "#5f697d",
                            "color": "#fff",
                        },
                        "div.popupQuestion": {
                            "backgroundColor": "#e68107",
                            "color": "#fff",
                        },
                        "div.popupTitle": {
                            "fontSize": "18px",
                            "padding": "10px",
                        },
                    };
                    this.applyCSS(this.innerControl, css);
                    if (this.modal) {
                        const overlay = topdoc.createElement("div");
                        overlay.style.top = "0px";
                        overlay.style.left = "0px";
                        overlay.style.position = "absolute";
                        overlay.style.width = "100%";
                        overlay.style.height = "100%";
                        overlay.style.zIndex = "10";
                        overlay.style.backgroundColor = "rgba(0,0,0,0.5)";
                        topdoc.body.appendChild(overlay);
                    }
                    topdoc.body.appendChild(this.innerControl);
                    this.focus();
                }
                applyCSS(parent, css) {
                    // Apply CSS to the element by class
                    // This is done to support using the javascript without a corresponding css file
                    for (const selector in css) {
                        if (css.hasOwnProperty(selector)) {
                            const style = css[selector];
                            const elements = parent.querySelectorAll(selector);
                            // tslint:disable-next-line:prefer-for-of
                            for (let i = 0; i < elements.length; i++) {
                                const el = elements[i];
                                // tslint:disable-next-line:forin
                                for (const k in style) {
                                    el.style[k] = style[k];
                                }
                            }
                        }
                    }
                }
                createButton(control, buttonType, setfocus) {
                    let name = buttonType.toString();
                    if (buttonType in ButtonType) {
                        name = ButtonType[buttonType];
                    }
                    const btn = topdoc.createElement("button");
                    btn.type = "button";
                    btn.id = `popupBtn${name.replace(/\s/g, "")}`;
                    btn.className = "popupButton";
                    btn.textContent = (buttonType in ButtonType) ? ButtonType[buttonType] : buttonType.toString();
                    const data = new ControlClickEventData(control, new Popup.Button(name, buttonType));
                    btn.addEventListener("click", (e) => {
                        let d = e.data;
                        if (!d) {
                            d = data;
                        }
                        const values = {};
                        let inputs = topdoc.querySelectorAll(`#${d.controlId} input`);
                        let i = 0;
                        // tslint:disable-next-line:prefer-for-of
                        for (; i < inputs.length; i++) {
                            const input = inputs[i];
                            if (input.files) {
                                values[input.name] = { value: input.value, files: input.files };
                            }
                            else {
                                values[input.name] = input.value;
                            }
                        }
                        inputs = topdoc.querySelectorAll(`#${d.controlId} select`);
                        for (i = 0; i < inputs.length; i++) {
                            const input = inputs[i];
                            values[input.name] = input.value;
                        }
                        inputs = topdoc.querySelectorAll(`#${d.controlId} textarea`);
                        for (i = 0; i < inputs.length; i++) {
                            const input = inputs[i];
                            values[input.name] = input.value;
                        }
                        const result = new Popup.ButtonClickEventData(d.clickedButton, values);
                        // get and remove the popup
                        d.control.innerControl.remove();
                        const overlay = topdoc.getElementById(`overlay-${d.controlId}`);
                        if (overlay) {
                            overlay.parentNode.removeChild(overlay);
                        }
                        const clickType = d.clickedButton.buttonType;
                        if (clickType === ButtonType.Cancel ||
                            clickType === ButtonType.Abort ||
                            clickType === ButtonType.No) {
                            control.reject(result);
                            return;
                        }
                        control.resolve(result);
                    });
                    if (setfocus) {
                        control.focus = () => { btn.focus(); };
                    }
                    return btn;
                }
            }
            Popup.PopupInstance = PopupInstance;
        })(Popup = Common.Popup || (Common.Popup = {}));
    })(Common = FDBZAP.Common || (FDBZAP.Common = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=popup.js.map