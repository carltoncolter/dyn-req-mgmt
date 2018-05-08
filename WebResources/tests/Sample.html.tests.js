/// <reference path="../testhelpers/sinon.js"/>
/// <reference path="../testhelpers/fakedocready.js"/>
/// <reference path="../testhelpers/Xrm.Page.js"/>
/// <reference path="../fdbzap_/Sample.html.js"/>

// TODO:    Rationalize when to use sinon stubs vs jasmine spies.
//          Right now, using stubs mostly for the withArgs() capability, which sinon doesn't appear to have:
//          https://github.com/jasmine/jasmine/issues/94

// NOTE:    The easies way to debug/run these tests in Visual Studio is to use: http://mmanela.github.io/chutzpah/

describe("Sample", function () {
    var _fakeResult;
    var _fakeAttribute;
    var _fireOnChange;
    var _formTypeStub;

    beforeEach(function () {
        // Fake for jquery
        // NOTE: simple faking approach.  For more elaborate html scenarios, consider using https://github.com/velesin/jasmine-jquery fixtures
        _fakeResult = function () { };
        _fakeResult.empty = function () { return _fakeResult; }
        _fakeResult.append = function (val) { }
        spyOn(_fakeResult, 'append');
        $ = sinon.stub();
        $.withArgs("#status").returns(_fakeResult);

        // Fake Xrm
        // NOTE: Simple faking approach.  Consider using https://github.com/camelCaseDave/xrm-mock-generator
        _fakeAttribute = {
            addOnChange: function (func) {
                _fireOnChange = func;
            }
        }
        spyOn(_fakeAttribute, 'addOnChange').and.callThrough();

        var attrStub = sinon.stub(Xrm.Page, "getAttribute");
        attrStub.withArgs("modifiedon").returns(_fakeAttribute);
        sinon.stub(Xrm.Page.data.entity, "getId").returns("{some-id}");
        sinon.stub(Xrm.Page.context, "getClientUrl").returns("https://some.crm.dynamics.com");
        _formTypeStub = sinon.stub(Xrm.Page.ui, "getFormType")

        // Fake XMLHttpRequest
        this.xhr = sinon.useFakeXMLHttpRequest();
        var requests = this.requests = [];

        this.xhr.onCreate = function (xhr) {
            requests.push(xhr);
        };
    });

    afterEach(function () {
        Xrm.Page.data.entity.getId.restore();
        Xrm.Page.getAttribute.restore();
        Xrm.Page.context.getClientUrl.restore();
        Xrm.Page.ui.getFormType.restore();
        this.xhr.restore();
    });

    function expectResponse(requests, val) {
        expect(requests.length).toBe(1);
        requests[0].respond(200, { "Content-Type": "application/json" },
            '{"prefix_somefield":' + val + '}');
    }

    it("Create_Form_Save", function () {
        // Arrange (majority of arrange happens in beforeEach)
        _formTypeStub.returns(1);

        //Act
        YourNamespace.YourClass.onReady();
        _fireOnChange();

        //Assert
        //expectResponse(this.requests, true);
        expect(_fakeAttribute.addOnChange).toHaveBeenCalled();
        expect(_fakeResult.append).toHaveBeenCalledWith("Loading...");
        expect(_fakeResult.append).toHaveBeenCalledWith("DONE");
    });

    it("Update_Form_Load", function () {
        // Arrange (majority of arrange happens in beforeEach)
        _formTypeStub.returns(2);

        //Act
        YourNamespace.YourClass.onReady();

        //Assert
        //expectResponse(this.requests, true);
        expect(_fakeResult.append).toHaveBeenCalledWith("Loading...");
        expect(_fakeResult.append).toHaveBeenCalledWith("DONE");
    });
});