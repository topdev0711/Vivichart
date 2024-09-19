// JScript File
function DirtyPage() {  
}
DirtyPage.origVals;
DirtyPage.unloadMsg;
DirtyPage.confirmMsg;
DirtyPage.CheckControls;
DirtyPage.noCheckFields;

//used by the dirty page handling to identify the control that raised the event
//for some reason link buttons in the button bar were showing the <td> as raising the event ??
DirtyPage.clickID = "";
//used by IAWHyperlInkbutton, btns which cause page checking call
//initial onclick sets this to true, so that the following form submit does not cause checking
//if the page is dirty and the user clicks cancel then this is set back to false
DirtyPage.page_checked = false;

DirtyPage.unloadMsg = "You have entered data without saving this form." 
DirtyPage.confirmMsg =  "You have entered data without saving this form."
DirtyPage.confirmMsg += "\n\nPress OK to carry on - you WILL LOSE any unsaved changes.";
DirtyPage.confirmMsg += "\n\nPress CANCEL to stay on this page";

DirtyPage.AddCheckControl = function(id){
    if(DirtyPage.CheckControls == null) DirtyPage.CheckControls = new Object();
    DirtyPage.CheckControls[id]=id;
    //if this control causes checking, then assume it does not check itself !!
    DirtyPage.AddNoCheckField(id);
}

DirtyPage.AddNoCheckField = 
function(id){
    if (DirtyPage.noCheckFields == null) DirtyPage.noCheckFields = new Object();
    DirtyPage.noCheckFields[id]=id;
}  

//**** functions for  storing/checking original form field values  *********

//store original field valus in an array
DirtyPage.loadValues = function(){        
    var oForm = document.forms[0];
    DirtyPage.origVals = new Object();            
    var input;
    var val;            
    for (var i=0 ; i < oForm.elements.length ; i++)
    {   
        input = oForm.elements[ i ];
        if(input.type == "hidden" || input.disabled || input.readOnly)continue;
        if(DirtyPage.ignoreControl(input.id)) continue;                
        val = DirtyPage.getInputValue(input); 
        DirtyPage.origVals[input.id] = val;                                                                                                            
    }               
}//end loadValues

//check to see if control is NOT to be checked
DirtyPage.ignoreControl = function(objID){                                                          
    for (var prop in DirtyPage.CheckControls)
    {
        if (objID == DirtyPage.noCheckFields[prop]){
            return true;
        }                   
    }                          
    return false;                 
} //end   ignoreControl    

//get the value from the input control
DirtyPage.getInputValue = function (inputObj){
    if( ( inputObj.type != "hidden" && !inputObj.disabled && !inputObj.readOnly ) && 
        (inputObj.type == "text" || inputObj.type == "textarea" || inputObj.type == "password") ){
            return inputObj.value;                                  
    }
    if(inputObj.type == "checkbox" || inputObj.type == "radio"){
        if(inputObj.checked) return true;
            return false;
    }
    if(inputObj.type == "select-one"){
        if(inputObj.selectedIndex == -1) return -1;
            return inputObj.options[inputObj.selectedIndex].value;
    } 
    return null;                     
}//end getInputValue
        
//check current form field values against original values
DirtyPage.isDirty = function() {
    var oForm = document.forms[0];
    //go throught the origVals object and check for values that have changed
    var input;
    var newVal;
    for (var objID in DirtyPage.origVals) {
    input = document.getElementById(objID);
    if (input == null) continue;
    //ignore any controls in this array, as they may have been added after the original values has been loaded
    if (DirtyPage.noCheckFields != null && DirtyPage.noCheckFields[objID]) continue;
    newVal = DirtyPage.getInputValue(input);
    if (DirtyPage.origVals[objID] != newVal) {
        DirtyPage.setPageIsDirty(true);                                     
        return true;
    }
    }
    //if a postback the no values will differ but viewstate has been maintained on the hidden field             
    if (DirtyPage.getPageIsDirty() == "true") return true;
    return false;
} //end  isDirty      
        
//**** end functions for  storing original form field values  *********
         

//**** functions for  handling the onsubmit and onbeforeunload events adn checking  *********

//onbeforeunload event handler
DirtyPage.checkForm_onbeforeunload = function() {
    //if the inactivity timer has caused this event then ignore
    if (IAW.inactivityTimeout == true) return;

    // If we see a link with that in its href, assume __doPostBack() is
    // running and skip the check.  The check will be performed by the form submit.
    var src = DirtyPage.getSrc();
    if (src == null) return;
    if (src.tagName == "A" && (src.href.indexOf("javascript:") != -1
        || src.href.indexOf("__doPostBack") != -1
        || src.href.indexOf("_DoPostBackWithOptions") != -1)) return;

    //deal with any controls where the onclick event used to set location.href, eg menus, logout control
    //DirtyPage.clickID is set in gotoUrl(), if it is = "" then this control didnot trigger the event
    //used when you click on home or a favourite for example, as otherwise it will show the current active element as the source element
    //which is not the case so DirtyPage.clickID is used as the mechanism to detect what sort of event is happening
    if (DirtyPage.clickID != null && DirtyPage.clickID != "" && src.innerHTML.indexOf("DirtyPage.gotoUrl")) return;

    if (DirtyPage.isDirty()) {
        return DirtyPage.unloadMsg;
    }
}   //end   checkForm_onbeforeunload 

//postback event handler, works with page request manager
//to replace DirtyPage.checkForm_onsubmit which is no longer called when there is a script manager (now on master page)
DirtyPage.checkForm_postback = function() {

    //check to see if validation has occured and failed
   if (typeof(Page_IsValid)!="undefined") {
       if (Page_IsValid==false) return false;
    }     

    //if page has already been checked
    if (DirtyPage.page_checked == true) return true;

    if (!DirtyPage.causesCheck()) {
        //detach onbefore unload event                 
        window.onbeforeunload = null;
        //if ajax don't hide btns etc
        //for some reason .get_isInAsyncPostBack() sometimes returns false even when it is an async postback
        if (!DirtyPage.isAsync()) {
            hideButtonsOnSubmit();
            IAW.stopInput();
        }
        return true;
    }
    return DirtyPage.checkIsDirty();
}    //end  checkForm_postback
                                

DirtyPage.setAjax = function ()
{
   DirtyPage.Ajax = "true";
}

//onclick event handler
DirtyPage.linkbutton_onclick =
function(e) {
    DirtyPage.page_checked = true;
    if (!DirtyPage.checkIsDirty()) {
        e.stopPropagation();
        e.preventDefault();
    }
    return true;
}           
        
DirtyPage.checkIsDirty = function (){                                                        
                            //returns true if off, false if stay on page
                            if(DirtyPage.isDirty()){
                                if(confirm(DirtyPage.confirmMsg)){
                                    //submit form
                                    //detach onbefore unload event                    
                                    window.onbeforeunload = null;
                                    //when DirtyPage.linkbutton_onclick is called and its an ajax postback
                                    //don't hide the buttons
                                    //if ajax don't hide btns etc
                                    if (!DirtyPage.isAsync()) {
                                         hideButtonsOnSubmit();
                                         //IAW.stopInput();                                        
                                      }                                    
                                    //hideButtonsOnSubmit();
                                    //IAW.stopInput();
                                    return true;
                                }
                                else{
                                    //don't submit
                                    showbuttons();
                                    DirtyPage.page_checked = false;                                    
                                    return false;                
                                }
                            }
                            //detach onbefore unload event
                            window.onbeforeunload = null;
                            if (!DirtyPage.isAsync()) {
                                 hideButtonsOnSubmit();                                 
                              } 
                            return true;
                        }//end   checkIsDirty      
        
//**** end functions for  handling the onsubmit and onbeforeunload events and checking  *********
        
//**** utilility functions 
 
//used by input controls where the onclick event has javascript, location.href
//the location.href was causing problems with the onbeforeunload event
//currently used by the logout control, menu items
//always returns false as we don't want the onclick event to do owt other than check the page
                        DirtyPage.gotoUrl = function(url) {
                            //used by the dirty page handling to identify the control that raised the event
                            //for some reason link buttons in the button bar were showing the <td> as raising the event ??
                            var src = DirtyPage.getSrc();
                            if (src != null) DirtyPage.clickID = src.id;
                            if (DirtyPage.checkIsDirty()) {
                                location.href = url;
                                return false;
                            }
                            return false;
                        } 
 
 
//checks to see if this control causes checking            
DirtyPage.causesCheck = function (){                     
                            var src = DirtyPage.getSrc();
                            var id = "";
                            if (src == null) return false;
                            if (src.id == null || src.id == '') id = DirtyPage.findID(src);                      
                            for (var prop in DirtyPage.CheckControls)
                            {
                                if (src.id != null && src.id == DirtyPage.CheckControls[prop]) {
                                    return true;
                                }
                                //if no src id they recurse upward to the first control with an id
                                //this is used for things like calendar controls, where each date link ie, <a tag,
                                //does not have an id, the parent (calendar control id will be in ChekcControl)
                                else if (src.id == null || src.id == '') {                    
                                    if (id == DirtyPage.CheckControls[prop]) return true;                                  
                                }
                            }   
                            return false;                 
                        }//end  causesCheck

//get the element that caused the event
DirtyPage.getSrc = function (){          
                        var src;                      
                        // Check the active element if there is no event target
                        if (DirtyPage.clickID != null && DirtyPage.clickID != "") {
                            src = document.getElementById(DirtyPage.clickID);
                            if (src != null) return src;
                        }
                        if(typeof(document.activeElement) != "undefined") return document.activeElement;                      
                        if(event != null) src = event.srcElement;
                        if(src == null) return null;  
                        return src;      
                    }                        

DirtyPage.setPageIsDirty = function (val){
                                var e = document.getElementById(IAW.contentID + "__DIRTYPAGE");
                                e.value = val;            
                            }

DirtyPage.getPageIsDirty = function (){
                                var e = document.getElementById(IAW.contentID + "__DIRTYPAGE");
                                return e.value;            
                            }   
        
DirtyPage.findID = function (obj){
                       var parent = obj.parentNode;
                       if (parent != null && !parent.id) {
                            return DirtyPage.findID(parent);
                       }
                       else if (parent != null && parent.id) {
                            return parent.id;
                       }
                       return "";
                    }

DirtyPage.isAsync = function() {
                        var prm = Sys.WebForms.PageRequestManager.getInstance();
                        if (prm == null) return false;
                        //for some reason .get_isInAsyncPostBack() sometimes returns false even when it is an async postback
                        if (prm._postBackSettings != null) return prm._postBackSettings.async;
                        return prm.get_isInAsyncPostBack();
                    }

//**** end utilility functions

