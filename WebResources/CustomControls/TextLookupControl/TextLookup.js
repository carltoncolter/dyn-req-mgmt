/**
 * TextLookupControl
 */
var FDBZAP;
(function (FDBZAP) {
    let Controls;
    (function (Controls) {
        class TextComparer {
            constructor() {
                this._isRunning = false;
            }
            update(data) {
                this.value = data.toString();
            }
            compareTo(data, saveIfDifferent) {
                if (this.value.length !== data.length) {
                    return false;
                }
                if (this.value === data) {
                    return true;
                }
                if (saveIfDifferent) {
                    this.value = data;
                }
                return false;
            }
            hashCode(data) {
                let hash = 0;
                if (!data) {
                    return hash;
                }
                for (let i = 0; i < data.length; i++) {
                    const chr = data.charCodeAt(i);
                    // tslint:disable-next-line:no-bitwise
                    hash = ((hash << 5) - hash) + chr;
                    // tslint:disable-next-line:no-bitwise
                    hash |= 0;
                }
                return hash;
            }
        }
        Controls.TextComparer = TextComparer;
        class TextLookupControl {
            constructor() {
                this.error = null;
                console.log("TextLookupControl.constructor");
                this.comparer = new TextComparer();
                this.gettingOutput = false;
            }
            isDirty() {
                console.log("TextLookupControl.isDirty");
                return this._isDirtyValue();
            }
            _isDirtyValue() {
                console.log("TextLookupControl._isDirtyValue");
                return this.dirty || this.comparer.compareTo(this.context.parameters.value.raw);
            }
            renderReadMode() {
                console.log("TextLookupControl.renderReadMode");
                this.disableControlInteraction();
            }
            renderEditMode() {
                console.log("TextLookupControl.renderEditMode");
            }
            isError() {
                console.log("TextLookupControl.isError");
                return (this.error !== null);
            }
            setControlState(context) {
                console.log("TextLookupControl.setControlState");
                this.shouldNotifyOutputChanged = !(context.mode.isControlDisabled || !context.parameters.value.security.editable || context.page && context.page.isPageReadOnly);
                if (!this.shouldNotifyOutputChanged) {
                    this.disableControlInteraction();
                }
                else {
                    this.enableControlInteraction();
                }
            }
            isControlDisabled() {
                console.log("TextLookupControl.isControlDisabled");
                return this.context.mode.isControlDisabled;
            }
            destroyCore() {
                console.log("TextLookupControl.destroyCore");
            }
            disableControlInteraction() {
                console.log("TextLookupControl.disableControlInteraction");
                this.readonly = true;
            }
            enableControlInteraction() {
                console.log("TextLookupControl.enableControlInteraction");
                this.readonly = false;
            }
            /********** Identified as most likely required methods *******/
            updateView(context) {
                console.log("TextLookupControl.updateView");
                if (this.gettingOutput) {
                    // this stops needlessly updating the control....
                    this.gettingOutput = false;
                    return;
                }
                // update link using id and name
            }
            destroy() {
                console.log("TextLookupControl.destroy");
                this.destroyCore();
            }
            getOutputs() {
                console.log("TextLookupControl.getOutputs");
                this.gettingOutput = true;
                return { value: this.context.parameters.value.raw || "" };
            }
            init(context, notifyOutputChanged, state, container) {
                console.log("TextLookupControl.init");
                this.context = context;
                this.notifyOutputChanged = notifyOutputChanged;
                this.container = container || document.getElementById(context.client._customControlProperties.descriptor.DomId + "-FieldSectionItemContainer");
                this.state = state;
                const regardingobjectid = context.parameters.value.raw;
                this.entityNameField = context.parameters.entitynamefield.raw;
                if (this.entityNameField == null) {
                    // uh oh... abort
                    return;
                }
                const domId = context.client._customControlProperties.descriptor.DomId;
                // if entityid is not null then
                //    build client url
                //    create link to entity
                //    set linkvalue = link
                // else
                //    set linkvalue = ""
                // end if
                // create child container
                //   create label
                //   create image
                // put child container in container ( do we need the child container )
            }
            onPreNavigation() {
                console.log("TextLookupControl.onPreNavigation");
            }
            getMaxLength() {
                console.log("TextLookupControl.getMaxLength");
                return this.context.parameters.value.attributes.MaxLength;
            }
            getAttributeName() {
                console.log("TextLookupControl.getAttributeName");
                return this.context.parameters.value.attributes.LogicalName;
            }
            _onChange(evt, propertyName, newValue, oldValue) {
                console.log("TextLookupControl._onChange");
                const data = this.value;
                if (!this.comparer.compareTo(data, true)) {
                    console.log("Comparer.compareTo found they do not match");
                    this.dirty = true;
                    this.context.parameters.value.raw = data;
                    setTimeout(() => {
                        this.dirty = true;
                        this.notifyOutputChanged();
                    }, 3000); // only send notifications to update at 3 second intervals... or lost focus.
                }
                return true;
            }
        }
        Controls.TextLookupControl = TextLookupControl;
    })(Controls = FDBZAP.Controls || (FDBZAP.Controls = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=TextLookup.js.map