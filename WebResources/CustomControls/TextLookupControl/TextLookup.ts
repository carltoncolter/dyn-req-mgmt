/**
 * TextLookupControl
 */

namespace FDBZAP {
	export namespace Controls {
		export class TextComparer {
			public hash: number;
			private _isRunning: boolean;
			private value: string;
			constructor() {
				this._isRunning = false;
			}
			public update(data: string) {
				this.value = data.toString();
			}
			public compareTo(data: string, saveIfDifferent?: boolean): boolean {
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
			public hashCode(data: string): number {
				let hash: number = 0;
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
		export class TextLookupControl {
			private state: any;
			private context: any;
			private dirty: boolean;
			private comparer: TextComparer;
			private container: HTMLElement | null;
			private shouldNotifyOutputChanged: boolean;
			private value: string;
			private error: any = null;
			private readonly: boolean;
			private gettingOutput: boolean;
			private entityNameField: string;

            /**
             * Notify framework value was changed
             */
			private notifyOutputChanged: () => void;

			constructor() {
				console.log("TextLookupControl.constructor");
				this.comparer = new TextComparer();
				this.gettingOutput = false;
			}

			public isDirty() {
				console.log("TextLookupControl.isDirty");
				return this._isDirtyValue();
			}

			public _isDirtyValue() {
				console.log("TextLookupControl._isDirtyValue");
				return this.dirty || this.comparer.compareTo(this.context.parameters.value.raw);
			}

			public renderReadMode() {
				console.log("TextLookupControl.renderReadMode");
				this.disableControlInteraction();
			}

			public renderEditMode() {
				console.log("TextLookupControl.renderEditMode");
			}

			public isError() {
				console.log("TextLookupControl.isError");
				return (this.error !== null);
			}

			public setControlState(context: any) {
				console.log("TextLookupControl.setControlState");
				this.shouldNotifyOutputChanged = !(context.mode.isControlDisabled || !context.parameters.value.security.editable || context.page && context.page.isPageReadOnly);
				if (!this.shouldNotifyOutputChanged) {
					this.disableControlInteraction();
				} else {
					this.enableControlInteraction();
				}
			}

			public isControlDisabled() {
				console.log("TextLookupControl.isControlDisabled");
				return this.context.mode.isControlDisabled;
			}

			public destroyCore() {
				console.log("TextLookupControl.destroyCore");
			}

			public disableControlInteraction() {
				console.log("TextLookupControl.disableControlInteraction");
				this.readonly = true;
			}

			public enableControlInteraction() {
				console.log("TextLookupControl.enableControlInteraction");
				this.readonly = false;
			}

			/********** Identified as most likely required methods *******/
			public updateView(context: any) {
				console.log("TextLookupControl.updateView");
				if (this.gettingOutput) {
					// this stops needlessly updating the control....
					this.gettingOutput = false;
					return;
				}
				// update link using id and name
				
			}

			public destroy() {
				console.log("TextLookupControl.destroy");
				this.destroyCore();
			}

			public getOutputs() {
				console.log("TextLookupControl.getOutputs");
				this.gettingOutput = true;
				return { value: this.context.parameters.value.raw || "" };
			}

			public init(context: any, notifyOutputChanged: () => void, state?: any, container?: HTMLDivElement) {
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

			public onPreNavigation() {
				console.log("TextLookupControl.onPreNavigation");
			}

			private getMaxLength() {
				console.log("TextLookupControl.getMaxLength");
				return this.context.parameters.value.attributes.MaxLength;
			}

			private getAttributeName() {
				console.log("TextLookupControl.getAttributeName");
				return this.context.parameters.value.attributes.LogicalName;
			}

			private _onChange(evt?: any, propertyName?: string, newValue?: any, oldValue?: any) {
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
	}
}
