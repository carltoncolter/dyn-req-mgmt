namespace FDBZAP {
    export namespace Email {
        export namespace Grid {
            export function groupItems(commandProps: FDBZAP.Common.Data.ICommandProperties, selectedItems: FDBZAP.Common.Data.ISelectedControlSelectedItemReference[], debug?: boolean ) 
            {
                const actionName = "fdbzap_CreateGroupFromSelection";
                if (debug) {
                    console.log(`START GroupItems: Setting up action call to ${actionName}`);
                    console.log("Items:");
                    selectedItems.forEach((itm) => {
                        console.log("Id=" + itm.Id + "\nName=" + itm.Name + "\nTypeCode=" + itm.TypeCode.toString() + "\nTypeName=" + itm.TypeName);
                    });
                }

                const errorCallback = (e: any) => {
                    if (e && e === e.toString()) {
                        // TODO: show error message e?
                    }
                    if (debug) {
                        console.log("Failure...");
                    }
                };

                // Prompt for group name
                const successCallback = (e: any) => {
                    if (e.errorOccurred) {
                        errorCallback(e.responseMessage);
                    }
                    if (debug) {
                        console.log("Success...");
                    }

                    if (e.data) {
                        const odataType = e.data.result["@odata.type"];
                        const entityName = odataType.replace("#Microsoft.Dynamics.CRM.", "");
                        const entityIdFieldName = `${entityName}id`;
                        const entityId = e.data.result[entityIdFieldName];
                        Xrm.Navigation.openForm({entityId, entityName});
                    }
                };

                FDBZAP.Common.Popup.inputBox("Group Name", "", {title: "Enter the Group Name"})
                .then((e: FDBZAP.Common.Popup.IButtonClickEventData) => {
                    const groupName = e.values["inputBoxValue"];

                    if (debug) {
                        console.log(`Group Name: ${groupName}`);
                    }

                    const items = new Common.Data.EntityCollection();
                    items.addSelectedItemReferences(selectedItems);

                    FDBZAP.Common.Data.invokeProcessAction(actionName, {groupName, items}).then(successCallback, errorCallback);
                });
            }
        }
    }
}
