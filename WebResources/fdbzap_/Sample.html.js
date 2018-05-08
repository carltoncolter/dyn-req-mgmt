// TODO: Replace with first needed js web resource.  Just starter code so there is a testable web resource in the template

var YourNamespace;
(function (YourNamespace) {
    var YourClass = (function () {
        function YourClass() {
        }

        YourClass.init = function () {
        };

        YourClass.onReady = function () {
            switch (Xrm.Page.ui.getFormType()) {
                case 1://Create
                    Xrm.Page.getAttribute("modifiedon").addOnChange(YourClass.doSomething);
                    break;
                case 2://Update
                    YourClass.doSomething();
                    break;
                default:
            }
        };

        YourClass.doSomething = function () {
            $("#status").empty().append("Loading...");
            var id = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
            var odataEndpoint = Xrm.Page.context.getClientUrl() + "/api/data/v8.2";

            //DO something

            $("#status").empty().append("DONE");
        };

        return YourClass;
    }());

    YourNamespace.YourClass = YourClass;
    YourClass.init();
})(YourNamespace || (YourNamespace = {}));

var Xrm = window.parent.Xrm;
$(document).ready(YourNamespace.YourClass.onReady);