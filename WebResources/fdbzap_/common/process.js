var FDBZAP;
(function (FDBZAP) {
    let Common;
    (function (Common) {
        let BusinessProcesses;
        (function (BusinessProcesses) {
            let _validation;
            function getValidator(executionContext) {
                if (_validation) {
                    return _validation;
                }
                _validation = new ProcessValidator(executionContext);
                return _validation;
            }
            BusinessProcesses.getValidator = getValidator;
            function addValidator(executionContext, eventHandler, errorMessage) {
                const v = this.getValidator(executionContext);
                v.addValidator(eventHandler, errorMessage);
            }
            BusinessProcesses.addValidator = addValidator;
            function removeValidator(executionContext, eventHandler) {
                const v = this.getValidator(executionContext);
                v.removeValidator(eventHandler);
            }
            BusinessProcesses.removeValidator = removeValidator;
            class Process {
                constructor(process, id, name) {
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
            BusinessProcesses.Process = Process;
            class Stage {
                constructor(process, stage, direction) {
                    if (process) {
                        if (process.getId) {
                            this.process = new Process(process);
                        }
                        else {
                            this.process = process;
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
                getStage() {
                    return this._rawStage;
                }
            }
            BusinessProcesses.Stage = Stage;
        })(BusinessProcesses = Common.BusinessProcesses || (Common.BusinessProcesses = {}));
        class ProcessValidator {
            constructor(executionContext) {
                this._ignoreStageChangeEvent = false;
                this._validationHandlers = [];
                this._errorMessages = [];
                const formContext = this._getContext(executionContext);
                formContext.data.process.addOnStageSelected(this._onStageSelectedWatcher);
                formContext.data.process.addOnStageChange(this._onStageChangeWatcher);
                const activeProcess = formContext.data.process.getActiveProcess();
                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, formContext.data.process.getActiveStage()));
            }
            addValidator(eventHandler, errorMessage) {
                this._validationHandlers.push(eventHandler);
                this._errorMessages.push(errorMessage || "");
            }
            removeValidator(eventHandler) {
                const i = this._validationHandlers.indexOf(eventHandler);
                if (i !== -1) {
                    this._validationHandlers = this._validationHandlers.splice(i, 1);
                    this._errorMessages = this._errorMessages.splice(i, 1);
                }
            }
            clearValidators(eventHandler) {
                this._validationHandlers = [];
                this._errorMessages = [];
            }
            getActiveProcesses(formContext) {
                formContext.data.process.getEnabledProcesses(this._setEnabledProcesses);
            }
            moveNext(formContext, bypassChangeEvents, callback) {
                this._move(formContext, "Next", bypassChangeEvents, callback);
            }
            movePrevious(formContext, bypassChangeEvents, callback) {
                this._move(formContext, "Previous", bypassChangeEvents, callback);
            }
            _onStageSelectedWatcher(executionContext) {
                const formContext = this._getContext(executionContext);
                const activeProcess = formContext.data.process.getActiveProcess();
                const eventArgs = executionContext.getEventArgs();
                this.selectedStage = new BusinessProcesses.Stage(activeProcess, eventArgs.getStage());
                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, formContext.data.process.getActiveStage()));
            }
            _onStageChangeWatcher(executionContext) {
                const formContext = this._getContext(executionContext);
                const activeProcess = formContext.data.process.getActiveProcess();
                const eventArgs = executionContext.getEventArgs();
                const processId = activeProcess.getId();
                const processName = activeProcess.getName();
                this._setActiveStage(new BusinessProcesses.Stage(activeProcess, eventArgs.getStage(), eventArgs.getDirection()));
                if (!this._ignoreStageChangeEvent) {
                    let valid = true;
                    for (let i = 0; i < this._validationHandlers.length; i++) {
                        const executeEvent = this._validationHandlers[i];
                        if (!executeEvent(this.activeStage, this._previousStage)) {
                            formContext.ui.setFormNotification(this._errorMessages[i], "ERROR", "processvalidation");
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) {
                        this._revert();
                    }
                    else {
                        formContext.ui.clearFormNotification("processvalidation");
                    }
                }
                this._ignoreStageChangeEvent = false;
            }
            _setActiveStage(stage) {
                if (this.activeStage == null || this.activeStage.id !== stage.id) {
                    this._previousStage = this.activeStage;
                    this.activeStage = stage;
                }
            }
            _getContext(executionContext) {
                if (!this._formContext) {
                    this._formContext = executionContext.getFormContext();
                }
                return this._formContext;
            }
            _setEnabledProcesses(processes) {
                this.enabledProcesses = [];
                for (const processId in processes) {
                    if (processes.hasOwnProperty(processId)) {
                        this.enabledProcesses.push(new BusinessProcesses.Process(undefined, processId, processes[processId]));
                    }
                }
            }
            _move(formContext, direction, bypassChangeEvents, callback) {
                this._moveDirection = direction;
                if (bypassChangeEvents) {
                    this._ignoreStageChangeEvent = true;
                }
                this._moveDelegate = callback;
                const executeMove = (direction === "Next") ? formContext.data.process.moveNext : formContext.data.process.movePrevious;
                if (callback) {
                    executeMove(this._moveCallback);
                }
                else {
                    executeMove();
                }
            }
            _moveCallback(status) {
                const success = status === "success";
                if (this._moveDelegate) {
                    const valid = this._moveDelegate(status, this.activeStage, this._previousStage);
                    if (!valid) {
                        this._revert();
                    }
                }
            }
            _revert() {
                this._ignoreStageChangeEvent = true;
                this._formContext.data.process.setActiveStage(this._previousStage.id);
            }
        }
        Common.ProcessValidator = ProcessValidator;
    })(Common = FDBZAP.Common || (FDBZAP.Common = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=process.js.map