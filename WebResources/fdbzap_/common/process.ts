namespace FDBZAP
{
    export namespace Common {
        export namespace BusinessProcesses {
            let _validation: ProcessValidator;

            export function getValidator(executionContext: Xrm.Events.EventContext): ProcessValidator
            {
                if (_validation) {
                    return _validation;
                }

                _validation = new ProcessValidator(executionContext);
                return _validation;
            }

            // Example Usage:
            // var validate = function(target,current)
            // {
            //      if (target.direction === "Next" && target.process.name === "My Process" && target.name === "My Stage") {
            //          <<do work here to set valid to true or false>>
            //
            //          if (!valid) return false;
            //      }
            //      return true;
            // }

            // If Validation fails, it will roll the stage back to the previous.
        
            // FDBZAP.Common.BusinessProcesses.addValidator(executionContext, validate, message);

            export function addValidator(executionContext: Xrm.Events.EventContext, eventHandler: BusinessProcesses.StatusChangeDelegate, errorMessage: string)
            {
                const v: ProcessValidator = this.getValidator(executionContext);
                v.addValidator(eventHandler, errorMessage);
            }

            export function removeValidator(executionContext: Xrm.Events.EventContext, eventHandler: BusinessProcesses.StatusChangeDelegate)
            {
                const v: ProcessValidator = this.getValidator(executionContext);
                v.removeValidator(eventHandler);
            }

            export type MoveDelegate = (status: string, target: Stage, current: Stage) => boolean;
            export type StatusChangeDelegate = (target: Stage, current: Stage) => boolean;
            export class Process {
                public id: string;
                public name: string;

                constructor(process?: Xrm.ProcessFlow.Process, id?: string, name?: string)
                {
                    if (process) {
                        this.id = process.getId();
                        this.name = process.getName();
                    }
                    if (id) {
                        this.id = id;
                    }
                    if (name) {
                        this.name = name;
                    }
                }
            }

            export class Stage {
                public id: string;
                public name: string;
                public entityName: string;
                public category: XrmEnum.StageCategory;
                public process: Process;
                public direction: string;

                private _rawStage: Xrm.ProcessFlow.Stage;

                constructor(process: Process | Xrm.ProcessFlow.Process, stage?: Xrm.ProcessFlow.Stage, direction?: string)
                {
                    if (process) {
                        if ((process as any).getId)
                        {
                            this.process = new Process(process as Xrm.ProcessFlow.Process);
                        } else {
                            this.process = process as Process;
                        }
                    }
                    if (stage) {
                        this.id = stage.getId();
                        this.name = stage.getName();
                        this.entityName = stage.getEntityName();
                        this.category = stage.getCategory().getValue();
                        this._rawStage = stage;
                    }

                    if (direction) {
                        this.direction = direction;
                    }

                }

                public getStage() {
                    return this._rawStage;
                }
            }
        }
        export class ProcessValidator {
            public selectedStage: BusinessProcesses.Stage;
            public activeStage: BusinessProcesses.Stage;
            public enabledProcesses: BusinessProcesses.Process[];

            private _formContext: Xrm.FormContext;
            private _ignoreStageChangeEvent: boolean;
            private _moveDelegate?: BusinessProcesses.MoveDelegate;
            private _moveDirection: string;
            private _previousStage: BusinessProcesses.Stage;
            private _validationHandlers: BusinessProcesses.StatusChangeDelegate[];
            private _errorMessages: string[];

            constructor(executionContext: Xrm.Events.EventContext)
            {
                this._ignoreStageChangeEvent = false;
                this._validationHandlers = [];
                this._errorMessages = [];

                const formContext = this._getContext(executionContext);

                formContext.data.process.addOnStageSelected(this._onStageSelectedWatcher);
                formContext.data.process.addOnStageChange(this._onStageChangeWatcher);

                const activeProcess = formContext.data.process.getActiveProcess();
                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, formContext.data.process.getActiveStage()));
            }

            public addValidator(eventHandler: BusinessProcesses.StatusChangeDelegate, errorMessage?: string)
            {
                this._validationHandlers.push(eventHandler);
                this._errorMessages.push(errorMessage || "");
            }

            public removeValidator(eventHandler: BusinessProcesses.StatusChangeDelegate)
            {
                const i = this._validationHandlers.indexOf(eventHandler);
                if (i !== -1) {
                    this._validationHandlers = this._validationHandlers.splice(i, 1);
                    this._errorMessages = this._errorMessages.splice(i, 1);
                }
            }

            public clearValidators(eventHandler: BusinessProcesses.StatusChangeDelegate)
            {
                this._validationHandlers = [];
                this._errorMessages = [];
            }

            public getActiveProcesses(formContext: Xrm.FormContext)
            {
                formContext.data.process.getEnabledProcesses(this._setEnabledProcesses);
            }

            public moveNext(formContext: Xrm.FormContext, bypassChangeEvents?: boolean, callback?: BusinessProcesses.MoveDelegate)
            {
                this._move(formContext, "Next", bypassChangeEvents, callback);
            }

            public movePrevious(formContext: Xrm.FormContext, bypassChangeEvents?: boolean, callback?: BusinessProcesses.MoveDelegate)
            {
                this._move(formContext, "Previous", bypassChangeEvents, callback);
            }

            private _onStageSelectedWatcher(executionContext: Xrm.Events.StageSelectedEventContext)
            {
                const formContext = this._getContext(executionContext);
                const activeProcess = formContext.data.process.getActiveProcess();
                const eventArgs = executionContext.getEventArgs();

                this.selectedStage = new BusinessProcesses.Stage(activeProcess, eventArgs.getStage());
                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, formContext.data.process.getActiveStage()));
            }

            private _onStageChangeWatcher(executionContext: Xrm.Events.StageChangeEventContext)
            {
                const formContext = this._getContext(executionContext);
                const activeProcess = formContext.data.process.getActiveProcess();
                const eventArgs = executionContext.getEventArgs();

                const processId = activeProcess.getId();
                const processName = activeProcess.getName();

                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, eventArgs.getStage(), eventArgs.getDirection()));

                if (!this._ignoreStageChangeEvent) {
                    let valid = true;
                    for (let i = 0; i < this._validationHandlers.length; i++)
                    {
                        const executeEvent = this._validationHandlers[i];
                        if (!executeEvent(this.activeStage, this._previousStage)) {
                            formContext.ui.setFormNotification(this._errorMessages[i], XrmEnum.FormNotificationLevel.Error, "processvalidation");
                            valid = false;
                            break;
                        }
                    }

                    if (!valid) {
                        this._revert();
                    } else {
                        formContext.ui.clearFormNotification("processvalidation");
                    }
                }
                this._ignoreStageChangeEvent = false;
            }

            private _setActiveStage(stage: BusinessProcesses.Stage)
            {
                if (this.activeStage == null || this.activeStage.id !== stage.id) {
                    this._previousStage = this.activeStage;
                    this.activeStage = stage;
                }
            }

            private _getContext(executionContext: Xrm.Events.EventContext): Xrm.FormContext
            {
                if (!this._formContext) {
                    this._formContext = executionContext.getFormContext();
                }
                return this._formContext;
            }

            private _setEnabledProcesses(processes: Xrm.ProcessFlow.ProcessDictionary): void
            {
                this.enabledProcesses = [];
                for (const processId in processes) {
                    if (processes.hasOwnProperty(processId)) {
                        this.enabledProcesses.push(new BusinessProcesses.Process(undefined, processId, processes[processId]));
                    }
                }
            }

            private _move(formContext: Xrm.FormContext, direction: string, bypassChangeEvents?: boolean, callback?: BusinessProcesses.MoveDelegate)
            {
                this._moveDirection = direction;

                if (bypassChangeEvents)
                {
                    this._ignoreStageChangeEvent = true;
                }
                this._moveDelegate = callback;
                
                const executeMove: (callbackFunction?: Xrm.ProcessFlow.ProcessCallbackDelegate) => void = 
                    (direction === "Next") ? formContext.data.process.moveNext : formContext.data.process.movePrevious;

                if (callback) {
                    executeMove(this._moveCallback);
                } else {
                    executeMove();
                }
            }

            private _moveCallback(status: string)
            {
                const success = status === "success";
                
                if (this._moveDelegate) {
                    const valid = this._moveDelegate(status, this.activeStage, this._previousStage);

                    if (!valid) {
                        this._revert();
                    }
                }
            }

            private _revert()
            {
                this._ignoreStageChangeEvent = true;
                this._formContext.data.process.setActiveStage(this._previousStage.id);
            }
        }
    }
}
