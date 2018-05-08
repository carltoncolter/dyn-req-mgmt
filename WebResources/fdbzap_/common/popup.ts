// Should we remove the Tools dependency?

namespace FDBZAP {
    export namespace Common {
        export class Popup {
            public content?: HTMLElement | string;
            public config: Popup.IPopupSettings;

            constructor(config?: Popup.IPopupSettings, content?: HTMLElement | string) {
                this.content = content;
                const settings = config || new Popup.Settings();
                this.config = {...settings};
            }
        
            public render(): Promise<Popup.IButtonClickEventData> {
                // set id
                this.config.id = this.config.id || `popup-${Tools.newGuid()}`;
                // return promise
                return new Popup.PopupInstance(this.config, this.content).promise;
            }
        }

        export namespace Popup {
            const topdoc = window.top.document;
            export interface IButton {
                name: string;
                buttonType: ButtonType | string | number;
            }

            export interface IButtonClickEventData {
                clicked: Popup.IButton;
                values: { [index: string]: any };
            }

            export interface IButtonSettings {
                buttonSettings?: number;
                buttonLayout?: Popup.ButtonLayout;
                messageType?: MessageType;
                defaultButton?: DefaultButton;
            }

            export interface IControl {
                innerControl: HTMLElement;
                focus: () => void;
                promise?: Promise<Popup.IButtonClickEventData>;
                mouseData: IMouseData;
                reject: (data: Popup.IButtonClickEventData) => void;
                resolve: (data: Popup.IButtonClickEventData) => void;
            }

            export interface IControlClickEventData {
                control: IControl;
                controlId: string;
                clickedButton: Popup.IButton;
            }

            export interface IPopupSettings extends IButtonSettings {
                width?: number;
                height?: number;
                modal?: boolean;
                id?: string;
                showImage?: boolean;
                title?: string;
            }

            export interface IMouseData {
                dragging: boolean;
                offsetTop: number;
                offsetLeft: number;
            }

            // Combined like vb MsgBox
            export enum ButtonLayout {
                OKOnly = 0,
                OKCancel = 1,
                AbortRetryIgnore = 2,
                YesNoCancel = 3,
                YesNo = 4,
                RetryCancel = 5,
                NoButtons = 6,
            }

            export enum ButtonType {
                Ok = 1,
                Cancel = 2,
                Abort = 3,
                Retry = 4,
                Ignore = 5,
                Yes = 6,
                No = 7,
            }

            export enum DefaultButton {
                DefaultButton1 = 0,
                DefaultButton2 = 256,
                DefaultButton3 = 512,
            }

            export enum MessageType {
                Normal = 0,
                Critical = 16,
                Question = 32,
                Exclamation = 48,
                Information = 64,
            }
            
            export enum MsgBoxType {
                Error = 0,
                Warning = 1,
                Alert = 2,
                Confirmation = 3,
                Input = 4,
                Loading = 5,
            }

            export function msgBox(textPrompt: string, 
                buttons: Popup.ButtonLayout | DefaultButton | MessageType | number, 
                options?: IPopupSettings, msgBoxType?: Popup.MsgBoxType): Promise<Popup.ButtonClickEventData> 
            {
                options = options || new Popup.Settings();
                options.buttonLayout = buttons as number;
                options.showImage = options.showImage || options.showImage === null;
                
                if (msgBoxType) {
                    options.title = options.title || Popup.MsgBoxType[msgBoxType];
                }

                const prompt = topdoc.createElement("span");
                prompt.textContent = textPrompt;
                prompt.className = "msgboxPrompt popupText";
                
                return new Popup(options, prompt).render();
            }

            export function inputBox(textPrompt: string, defaultValue: string, options?: IPopupSettings) 
            {
                options = options || new Popup.Settings();
                options.messageType = MessageType.Question;
                options.title = options.title || Popup.MsgBoxType[Popup.MsgBoxType.Input];
                options.id = `popup-${Tools.newGuid()}`;

                const prompt = topdoc.createElement("span");
                prompt.textContent = textPrompt; /*! label */

                const input = topdoc.createElement("input");
                input.type = "text";
                input.name = "inputBoxValue";
                input.style.marginLeft = "10px";
                input.addEventListener("keypress", (e) => {
                    if (e.keyCode === 13 || e.code === "Enter")
                    {
                        const popup = topdoc.getElementById(options!.id!);
                        const okbtn: HTMLButtonElement | null = popup!.querySelector("#popupBtnOk");
                        okbtn!.click();
                    }
                });

                const container = topdoc.createElement("span"); // why not div?
                container.appendChild(prompt);
                container.appendChild(input);

                return new Popup(options, container).render();
            }

            export function loading(options?: IPopupSettings)
            {
                options = options || new Popup.Settings();
                options.messageType = MessageType.Information;
                options.title = options.title || Popup.MsgBoxType[Popup.MsgBoxType.Loading];
                options.id = `popup-${Tools.newGuid()}`;

                const label = topdoc.createElement("span");
                label.textContent = "Loading. . . ";
                label.style.fontSize = "3em";

                const img = topdoc.createElement("img");
                // Should we remove this dependency?
                img.src = Tools.getWebResourceBaseUrl() + "img/popup/loading.gif";
                img.alt = "loading...";
                img.style.border = "none";

                const container = topdoc.createElement("span"); // why not div?
                container.appendChild(label);
                container.appendChild(img);

                return new Popup(options, container).render();

            }

            export function errorMsg(message: string, options?: IPopupSettings) {
                return this.msgBox(message, MessageType.Critical, options, Popup.MsgBoxType.Error);
            }

            export function warning(message: string, options?: IPopupSettings) {
                return this.msgBox(message, MessageType.Exclamation, options, Popup.MsgBoxType.Warning);
            }

            export function alert(message: string, options?: IPopupSettings) {
                return this.msgBox(message, MessageType.Normal + Popup.ButtonLayout.OKOnly, options, Popup.MsgBoxType.Alert);
            }

            export function confirm(message: string, options?: IPopupSettings) {
                return this.msgBox(message, MessageType.Normal + Popup.ButtonLayout.OKCancel, options, Popup.MsgBoxType.Confirmation);
            }

            export function remove(popupid: string) {
                const r = (id: string) => {
                    const el = topdoc.getElementById(id);
                    if (el) {
                        el.parentNode!.removeChild(el);
                    }
                };
                r(popupid);
                r("modal-" + popupid);
            }

            export class ButtonClickEventData implements Popup.IButtonClickEventData {
                public clicked: Popup.IButton;
                public values: { [index: string]: any };

                constructor(clicked: Popup.IButton, values: { [index: string]: any }) {
                    this.clicked = clicked;
                    this.values = values;
                }
            }

            export class Button implements Popup.IButton {
                public name: string;
                public buttonType: ButtonType | string | number;
                constructor(name: string, buttonType: ButtonType | string | number) {
                    this.name = name;
                    this.buttonType = buttonType;
                }
            }

            export class ButtonSettings implements IButtonSettings {
                private innerButtonSettings: number;
                get buttonSettings(): number {
                    return this.innerButtonSettings;
                }
                // tslint:disable:no-bitwise
                set buttonSettings(value: number) {
                    this.innerButtonSettings = value;

                    this.innerButtonLayout = value & ((1 << 4) - 1);

                    const styleWithoutButtonType = value & ~((1 << 4) - 1);
                    this.innerMessageType = styleWithoutButtonType & ((1 << 8) - 1);

                    this.innerDefaultButton = value & ~((1 << 8) - 1);
                }
                // tslint:enable:no-bitwise

                private innerButtonLayout: Popup.ButtonLayout;
                get buttonLayout(): Popup.ButtonLayout {
                    return this.innerButtonLayout;
                }
                set buttonLayout(value: Popup.ButtonLayout) {
                    this.innerButtonLayout = value;
                    this.calculateStyle();
                }

                private innerMessageType: MessageType;
                get messageType(): MessageType {
                    return this.innerMessageType;
                }
                set messageType(value: MessageType) {
                    this.innerMessageType = value;
                    this.calculateStyle();
                }

                private innerDefaultButton: DefaultButton;
                get defaultButton(): DefaultButton {
                    return this.innerDefaultButton;
                }
                set defaultButton(value: DefaultButton) {
                    this.innerDefaultButton = value;
                    this.calculateStyle();
                }

                constructor(combinedStyle: number) {
                    this.buttonSettings = combinedStyle;
                }

                private calculateStyle() {
                    this.innerButtonSettings = this.innerButtonLayout + this.innerMessageType + this.innerDefaultButton;
                }
            }

            export class ControlClickEventData implements IControlClickEventData {
                public control: IControl;
                public controlId: string;
                public clickedButton: Popup.IButton;
                constructor(control: IControl, clickedButton: Popup.IButton) {
                    this.control = control;
                    this.controlId = this.control.innerControl.id;
                    this.clickedButton = clickedButton;
                }
            }

            export class Settings extends ButtonSettings implements IPopupSettings, IButtonSettings {
                public minWidth: number = 300;
                public minHeight: number = 140;
                public maxWidth: number = 600;
                public maxHeight: number = 600;
                public width: number = 0;
                public height: number = 0;
                public modal: boolean = false;
                public id?: string;
                public showImage?: boolean;
                public title: string;
                public imagePath?: string;

                constructor() {
                    super(1);
                }
            }

            export class MouseData implements IMouseData {
                public dragging: boolean;
                public offsetLeft: number;
                public offsetTop: number;
            }

            export class PopupInstance extends Settings implements IControl {
                public style: string;
                public innerControl: HTMLDivElement;
                public focus: () => void;
                public promise: Promise<Popup.IButtonClickEventData>;

                public mouseData: IMouseData;                

                public reject: (data: Popup.IButtonClickEventData) => void;
                public resolve: (data: Popup.IButtonClickEventData) => void;

                constructor(config: IPopupSettings, content?: JQuery<HTMLElement> | HTMLElement | string) {
                    super();
                    const settings = config || new Popup.Settings();
                    for (const key in settings) 
                    {
                        if (this.hasOwnProperty(key)) 
                        {
                            (this as any)[key] = (settings as any)[key];
                        }
                    }
                
                    const formFactor = Tools.getFormFactor();

                    // Set Default Image Path for popup images
                    this.imagePath = this.imagePath || (Tools.getWebResourceBaseUrl() + "img/popup");
                    
                    this.promise = new Promise<Popup.IButtonClickEventData>((pg, pb) => {
                        this.resolve = pg;
                        this.reject = pb;
                    });

                    const putInRange = (i: number, min: number, max: number) => Math.min(max, Math.max(i, min));

                    this.width = putInRange(this.width, this.minWidth, this.maxWidth);
                    this.height = putInRange(this.height, this.minHeight, this.maxHeight);

                    // Set Default Title
                    if (!this.title) {
                        switch (this.messageType) {
                            case Popup.MessageType.Normal: this.title = "Message"; break;
                            case Popup.MessageType.Critical: this.title = "Critical Message"; break;
                            case Popup.MessageType.Question: this.title = "Question"; break;
                            case Popup.MessageType.Exclamation: this.title = "Important Message"; break;
                            case Popup.MessageType.Information: this.title = "Information"; break;
                            default: this.title = "Message"; break;
                        }
                    }
                    
                    // Setup default focus event handler
                    this.focus = () => {
                        const input = topdoc.querySelector("input, select, textarea");
                        if (input) {
                            (input as HTMLElement).focus();
                        }
                    };

                    // CONTENT DIV
                    // If content empty, set content to title
                    if (typeof content === "string") {
                        // Replace \r\n or \n with <br/>
                        content = (content as string).replace(/\r?\n/g, "<br/>");
                    }
                    
                    const promptDiv = topdoc.createElement("div");
                    if (content) {
                        if (typeof content === "string") {
                            promptDiv.innerHTML = content;
                        } else {
                            promptDiv.appendChild(content as HTMLElement);
                        }
                    } else {
                        const prompt = topdoc.createElement("span");
                        prompt.textContent = this.title;
                        prompt.className = "msgboxPrompt popupText";
                        promptDiv.appendChild(prompt);
                    }

                    let contentDiv: HTMLElement | HTMLElement[] = topdoc.createElement("div");
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
                    innerDiv.id = this.id!;
                    switch (formFactor)
                    {
                        case XrmEnum.ClientFormFactor.Phone:
                        case XrmEnum.ClientFormFactor.Desktop:
                        case XrmEnum.ClientFormFactor.Tablet:
                        case XrmEnum.ClientFormFactor.Unknown:
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

                    const iControl = this as IControl;

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
                        for (const el of contentDiv)
                        {
                            this.innerControl.appendChild(el);
                        }
                    } else {
                        this.innerControl.appendChild(contentDiv as HTMLElement);
                    }
                    this.innerControl.appendChild(buttonsDiv);

                    const css: any = {
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

                public mouseMoveEvent = (e: MouseEvent) => {
                    if (!this.mouseData.dragging) {
                        return;
                    }
                    const div = e.target as HTMLDivElement;
                    this.innerControl.style.position = "absolute";
                    this.innerControl.style.left = `${e.pageX - this.mouseData.offsetLeft}px`;
                    this.innerControl.style.top = `${e.pageY - this.mouseData.offsetTop}px`;
                }

                public mouseUpEvent = (e: MouseEvent) => {
                    this.mouseData.dragging = false;
                    window.top.removeEventListener("mousemove", this.mouseMoveEvent);
                    this.innerControl.removeEventListener("mouseup", this.mouseUpEvent);
                }

                public mouseDownOnTitleEvent = (e: MouseEvent) => {
                    if (this.innerControl) {
                        this.mouseData.dragging = true;
                        this.mouseData.offsetTop = e.pageY - this.innerControl.offsetTop;
                        this.mouseData.offsetLeft = e.pageX - this.innerControl.offsetLeft;
                        // this.innerControl.attr("unselectable", "on").data("dragoffset", offset);

                        // movement is tracked on body
                        window.top.addEventListener("mousemove", this.mouseMoveEvent);
                        this.innerControl.addEventListener("mouseup", this.mouseUpEvent);
                    }
                }
                
                protected applyCSS(parent: HTMLElement, css: any) {
                    // Apply CSS to the element by class
                    // This is done to support using the javascript without a corresponding css file
                    for (const selector in css) {
                        if (css.hasOwnProperty(selector)) {
                            const style = css[selector];
                            const elements = parent.querySelectorAll(selector);
                            // tslint:disable-next-line:prefer-for-of
                            for (let i = 0; i < elements.length; i++)
                            {
                                const el = elements[i] as HTMLElement;
                                // tslint:disable-next-line:forin
                                for (const k in style) {
                                    (el.style as any)[k] = style[k];
                                }
                            }
                        }
                    }
                }

                protected createButton(control: IControl, buttonType: ButtonType | string | number, setfocus: boolean): HTMLElement {
                    let name = buttonType.toString();
                    if (buttonType in ButtonType) {
                        name = ButtonType[buttonType as ButtonType];
                    }

                    const btn = topdoc.createElement("button");
                    btn.type = "button";
                    btn.id = `popupBtn${name.replace(/\s/g, "")}`;
                    btn.className = "popupButton";
                    btn.textContent = (buttonType in ButtonType) ? ButtonType[buttonType as ButtonType] : buttonType.toString();
                    
                    const data = new ControlClickEventData(control, new Popup.Button(name, buttonType));

                    btn.addEventListener("click", (e: any) => {
                        let d = e.data;
                        if (!d) {
                            d = data;
                        }
                        
                        const values: { [index: string]: any } = {};
                        let inputs = topdoc.querySelectorAll(`#${d.controlId} input`) as NodeListOf<HTMLElement>;
                        
                        let i: number = 0;

                        // tslint:disable-next-line:prefer-for-of
                        for (; i < inputs.length; i++)
                        {
                            const input = inputs[i] as HTMLInputElement;
                            if (input.files) {
                                values[input.name] = { value: input.value, files: input.files };
                            } else {
                                values[input.name] = input.value;
                            }
                        }

                        inputs = topdoc.querySelectorAll(`#${d.controlId} select`);
                        for (i = 0; i < inputs.length; i++)
                        {
                            const input = inputs[i] as HTMLSelectElement;
                            values[input.name] = input.value;
                        }

                        inputs = topdoc.querySelectorAll(`#${d.controlId} textarea`);
                        for (i = 0; i < inputs.length; i++)
                        {
                            const input = inputs[i] as HTMLTextAreaElement;
                            values[input.name] = input.value;
                        }

                        const result = new Popup.ButtonClickEventData(d.clickedButton, values);

                        // get and remove the popup
                        d.control.innerControl.remove();

                        const overlay = topdoc.getElementById(`overlay-${d.controlId}`);
                        if (overlay) {
                            overlay.parentNode!.removeChild(overlay);
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
        }
    }
}
