//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

/*
    Helper functions, all are gathered in SeleniumWindowsPhoneDriver 
    namespace, just to avoid any name confilicts.

    WebElements are managed and contained within this object.
*/

var SeleniumWindowsPhoneDriver = {

    LastCallResult: null,
    CurrentId: 0,
    ElementsTabele: new Object(),
    /*
        Initialize js engine
    */
    Initialize: function () {
        ElementsTable = [];
        CurrentId = 0;
    },

    GenerateElementId: function () {
        this.CurrentId++;
        return 'InternalId' + this.CurrentId;
    },

    SaveElement: function (object) {
        var newId = this.GenerateElementId();

        this.ElementsTabele[newId] = object;

        return newId;
    },

    GetElement: function (internalId) {
        var elem = this.ElementsTabele[internalId];

        if (elem == undefined) {
            return null;
        }

        return elem;
    },

    ConvertElementsToInternlIds: function (elements) {
        var idsA = [];

        if (elements != null || elements != undefined) {
            for (var i = 0; i < elements.length; i++) {
                var id = SeleniumWindowsPhoneDriver.SaveElement(elements[i]);
                idsA.push({ ELEMENT: id });
            }
        }
        return JSON.stringify(idsA);
    },

    Type : function (internalId, param1, param2, param3) {

        var elem = SeleniumWindowsPhoneDriver.GetElement(internalId);

        if (elem != null) {
            webdriver.atoms.inputs.sendKeys(elem, param1, param2, param3);
        }
    }
}

SeleniumWindowsPhoneDriver.Initialize();