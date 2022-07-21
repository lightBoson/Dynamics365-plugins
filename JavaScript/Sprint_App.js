// On Load Event for Product
function OnLoadEvent(executionContext) {
    var formContext = executionContext.getFormContext();
    var productType = formContext.getAttribute("ko_producttypeko").getText();
    var generalTab = formContext.ui.tabs.get("GENERAL_TAB");
    var clothesSection = generalTab.sections.get("clothes_section");
    var toysSection = generalTab.sections.get("toys_section");
    if (productType == "Toys") {
        clothesSection.setVisible(false);
        toysSection.setVisible(true);
    }
    else if ("Clothes") {
        toysSection.setVisible(false);
        clothesSection.setVisible(true);
    } else {
        toysSection.setVisible(false);
        clothesSection.setVisible(false);
    }
}

// OnSave Event for Product => Hello podgladacze ;-) 
function OnSaveEvent(executionContext) {
    var saveEvent = executionContext.getEventArgs();
    var formContext = executionContext.getFormContext();
    var formControl = formContext.getControl("ko_nameko");
    var productText = formContext.getAttribute("ko_nameko").getValue();
    const notValidCharacters = ["*", "/", "'"];
    if (notValidCharacters.some(char => productText.includes(char))) {
        formContext.ui.setFormNotification("Name can not contain *, /, '. Check a name's product.", "ERROR", "firstnamenotify");
        formControl.setNotification("Error: Product name is incorrect", "productnamecontrolnotification");
    }

}

function OnChangeEvent(executionContext) {
    var formContext = executionContext.getFormContext();
    var formControl = formContext.getControl("ko_nameko");
    formContext.ui.clearFormNotification("firstnamenotify")
    formControl.clearNotification("productnamecontrolnotification");
}

function CloneRecord(executionContext) {

    //debugger;
    "use strict";
    var entityFormOptions = {};
    //Entity Form Options 
    entityFormOptions["entityName"] = "ko_productko"; 
    entityFormOptions["openInNewWindow"] = true;
    var formParameters = {};
    executionContext.data.entity.attributes.forEach(
        function (attribute, index) {
            var attributeName = attribute.getName();
            var attributetype = attribute.getAttributeType();
            var attrvalue = attribute.getValue();
            if (
                attributeName === 'ko_nameko' ||     // The attributes value which you want to pass
                attributeName === 'ownerid ' ||
                attributeName === 'ko_checkboxko' ||
                attributeName === 'ko_price_ko' ||
                attributeName === 'ko_tagsko'   ||
                attributeName === 'ko_manufacturedbyko' ||
                attributeName === 'ko_countryoforginko' ||
                attributeName === 'ko_producttypeko'  ||
                attributeName === 'ko_availableko' ||
                attributeName === 'ko_description_productko' ||
                attributeName === 'ko_materialko'
            ) {
                if (attributetype === "lookup") {
                    if (attrvalue !== null) {
                        if (attrvalue[0].id !== null) {
                            var regObj = {};
                            regObj.entityType = attrvalue[0].entityType;
                            regObj.name = attrvalue[0].entityType.name;
                            regObj.id = attrvalue[0].id;
                            formParameters[attributeName] = attrvalue;
                        }
                    }
                }

                if (attributetype === "boolean") {
                    formParameters[attributeName] = attrvalue;
                }
                if (attributetype === 'datetime') {
                    formParameters[attributeName] = attrvalue.toDateString();
                }
                if (attributetype === 'optionset') {
                    formParameters[attributeName] = attrvalue;
                }
                if (attributetype === 'string') {
                    formParameters[attributeName] = attrvalue;
                }
                if (attributetype === 'money') {
                    formParameters[attributeName] = attrvalue;
                }

            }

        });
    Xrm.Navigation.openForm(entityFormOptions, formParameters);
}

function OnChangeEU (executionContext) {
    const otherCountriesOptions = [{text : "USA", value : 124040003}, {text : "United Kingdom", value : 124040004},
    {text : "Norway", value : 124040008}];
    const ueCountriesOptions = [{text : "Poland", value : 124040000}, {text : "France", value : 124040001 }, 
    {text : "Germany", value : 124040002}, {text : "Belgium", value : 124040005}, {text : "Spain", value : 124040006}, 
    {text : "Italy", value : 124040007}];

    var formContext = executionContext.getFormContext();
    var ueValue = formContext.getAttribute("ko_checkboxko").getValue();
    var dropdownControl = formContext.getControl("ko_countryoforginko");
    if(ueValue){
        dropdownControl.clearOptions();
        for(let j = 0; j < ueCountriesOptions.length; j++){
            dropdownControl.addOption(ueCountriesOptions[j]);       
        }
    } else {
        dropdownControl.clearOptions();
        for(let k = 0; k < otherCountriesOptions.length; k++){
            dropdownControl.addOption(otherCountriesOptions[k]);         
        }
    }

}