var FDBZAP;
(function (FDBZAP) {
    let Email;
    (function (Email) {
        let Grid;
        (function (Grid) {
            function groupItems(commandProps, selectedItems, debug) {
                const actionName = "fdbzap_CreateGroupFromSelection";
                if (debug) {
                    console.log(`START GroupItems: Setting up action call to ${actionName}`);
                    console.log("Items:");
                    selectedItems.forEach((itm) => {
                        console.log("Id=" + itm.Id + "\nName=" + itm.Name + "\nTypeCode=" + itm.TypeCode.toString() + "\nTypeName=" + itm.TypeName);
                    });
                }
                const errorCallback = (e) => {
                    if (e && e === e.toString()) {
                        // TODO: show error message e?
                    }
                    if (debug) {
                        console.log("Failure...");
                    }
                };
                // Prompt for group name
                const successCallback = (e) => {
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
                        Xrm.Navigation.openForm({ entityId, entityName });
                    }
                };
                FDBZAP.Common.Popup.inputBox("Group Name", "", { title: "Enter the Group Name" })
                    .then((e) => {
                    const groupName = e.values["inputBoxValue"];
                    if (debug) {
                        console.log(`Group Name: ${groupName}`);
                    }
                    const items = new FDBZAP.Common.Data.EntityCollection();
                    items.addSelectedItemReferences(selectedItems);
                    FDBZAP.Common.Data.invokeProcessAction(actionName, { groupName, items }).then(successCallback, errorCallback);
                });
            }
            Grid.groupItems = groupItems;
        })(Grid = Email.Grid || (Email.Grid = {}));
    })(Email = FDBZAP.Email || (FDBZAP.Email = {}));
})(FDBZAP || (FDBZAP = {}));
//# sourceMappingURL=grid.js.map