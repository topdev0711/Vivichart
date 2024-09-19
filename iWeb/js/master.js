// JScript File

// Create shortcut functions
var $ID = function (elementId) {
    return document.getElementById(elementId);
};
var $QS = function (cssSelector) {
    return document.querySelector(cssSelector);
}
var $QSAll = function (cssSelector) {
    return document.querySelectorAll(cssSelector);
}

var $StoreSet = function (StoreID, Val) {
    sessionStorage.setItem(StoreID, Val);
};
var $StoreGet = function (StoreID) {
    return sessionStorage.getItem(StoreID);
};
var $StoreRemove = function (StoreID) {
    sessionStorage.removeItem(StoreID);
}

//global vars
    var primaryAccountID="";     
//end global vars

/* START definition of general IAW javascript object */
function IAW(){
}
     /* functions for IAW */
	function IAW_setScrollPos(){
	  var e = $get(IAW.contentID + '__SCROLLPOS');
	  var content = $get('div_content');
	  content.scrollTop = e.value;
	}
	function IAW_getScrollPos(){
	  var e = $get(IAW.contentID + '__SCROLLPOS');
	  return e.value;
	}
    function IAW_saveScrollPosition(){
        var e = $get(IAW.contentID + '__SCROLLPOS');
        var content = $get('div_content');
        e.value = content.scrollTop;                       
    }	
	function IAW_setElementFocusID(objId){	   
	  var e = $get(IAW.contentID + '__EDITING');
	  e.value = objId;	  
	}
	function IAW_getElementFocusID(){
	  var e = $get(IAW.contentID + '__EDITING');
	  return e.value;	  
	}

	function IAW_stopInput() {
	    $addHandler(document, "keydown", ignoreinput);
	    $addHandler(document, "click", ignoreinput);
	    $addHandler(document, "mousedown", ignoreinput);
	    
//        document.attachEvent("onkeydown", ignoreinput); 
//        document.attachEvent("onclick", ignoreinput); 
//        document.attachEvent("onmousedown", ignoreinput);
    }

    function IAW_allowInput() {
        try {
            $removeHandler(document, "keydown", ignoreinput);
            $removeHandler(document, "click", ignoreinput);
            $removeHandler(document, "mousedown", ignoreinput);
        } catch (e) { }      
   }    		

     /* static/class functions for IAW */
    IAW.setScrollPos = IAW_setScrollPos; 
    IAW.getScrollPos = IAW_getScrollPos; 
    IAW.saveScrollPosition = IAW_saveScrollPosition; 
    IAW.setElementFocusID = IAW_setElementFocusID; 
    IAW.getElementFocusID = IAW_getElementFocusID; 
    IAW.stopInput = IAW_stopInput; 
    /* end prototype functions for IAW */
    

    /* Global IAW properties 
    these are now set in header_js.ascx
    IAW.clientID = "<% response.write(ctx.clientID()) %>";
    IAW.contentID = "<% response.write(ctx.contentID()) %>_";
    IAW.buttonBarID = "<% response.write(ctx.buttonBarID()) %>";*/
    //used by DirtyPage.checkForm_onbeforeunload to decide whether to perform a check
    //no check will be performed if the inactivity timer has caused this event  
    IAW.inactivityTimeout = false;       
    /* Global IAW properties */

/* END definition of general IAW javascript object */

    function ignoreinput(e) {        
        e.preventDefault();
        e.stopPropagation();    
    }

    // If key pressed isn't to do with tabbing, then change focus
    function autoTab(e, NextField) {
        if (!e) var e = window.event;
        var srcEl = e.srcElement ? e.srcElement : e.target; 
        if (srcEl.value.length == 0)
            return;
        if (e.keyCode == 9 || e.keyCode == 16)
            return;
        document.getElementById(NextField).focus();
    }


function setScrollPosition() {
    IAW.setScrollPos();
}

function setFormFocus() {    
    var Form1 = document.forms[0];
    var i;
    var found = 0;
    
    //dumpControls();

    for( i = 0; i < Form1.length; i++ ) {    
        if (found == 0) {
            if (Form1.elements[ i ].id != "" && Form1.elements[ i ].id == IAW.getElementFocusID()){                
                //if the element is a dropdown then move the focus on one otherwise
                if(Form1.elements[ i ].type != "select-one" 
                   && Form1.elements[ i ].type != "hidden" 
                   && !Form1.elements[ i ].disabled 
                   && !Form1.elements[ i ].readOnly)
                {     
                    //focus is failing when the control is on tab1 but the postback is going to tab2          
                    try{
                      Form1.elements[ i ].focus(); 
                      if(Form1.elements[ i ].type == "text" || Form1.elements[ i ].type == "textarea"){
                          Form1.elements[ i ].select();
                      }                                            
                    }catch(e){  
                      //do nowt                  
                    }                                  
                    break;
                }
                found=1;      
            }    
        }
        else{
            if( Form1.elements[ i ].type != "hidden" && 
              !Form1.elements[ i ].disabled && 
              !Form1.elements[ i ].readOnly && 
              (Form1.elements[ i ].type == "text" || 
               Form1.elements[ i ].type == "textarea" || 
               Form1.elements[ i ].type == "checkbox" || 
               Form1.elements[ i ].type == "select-one" || 
               Form1.elements[ i ].type == "password") ) 
            { 
                Form1.elements[ i ].focus();
                if(Form1.elements[ i ].type == "text" || Form1.elements[ i ].type == "textarea"){
                    Form1.elements[ i ].select();
                }                
                break;
            }       
        }//else found=1                                              
    } //end for
}//end setFormFocus

//called in page_dirty.js
function hideButtonsOnSubmit() {
    var div = $get(IAW.buttonBarID);
    if (div == null) return;
    div.style.visibility = "visible";
    div.style.display = "block";  
    _hideButtons();
    _showWait();
}
//used by some client payinput screens, to hide buttons where the input box does not have a valid value in
function hideButtonsNoWait() {
    _hideButtons();
}
function hidebuttons(object) {
    //check to see if validation has occured and failed
    var js = "if (typeof (Page_IsValid) != 'undefined') { "
        js += "if (Page_IsValid == false){showbuttons();}};";
     window.setTimeout(js, 100);
    _hideButtons();
    _showWait();
    if (object != null) {
        DirtyPage.clickID = object.id;
    }
}
function _hideButtons() {
    var tbl = $get(IAW.buttonBarID + '_buttonBar');
    if (tbl==null) return;
    tbl.style.visibility = "hidden";
    tbl.style.display = "none";    
}
function _showWait() {
    var wait = $get(IAW.buttonBarID + '_activityIndicator');
    if(wait != null)wait.style.display = "";
}

function showbuttons(){
    //hide the buttons on the button bar    
    //var tbl = $get("ctl00_bBar_buttonBar")
    var tbl = $get(IAW.buttonBarID + '_buttonBar');
    var wait = $get(IAW.buttonBarID + '_activityIndicator');

    if (tbl == null) return;
    tbl.style.visibility = "visible";    
    tbl.style.display = "";
    if (wait != null) wait.style.display = "none";
     //used by the dirty page handling to identify the control that raised the event
    //for some reason link buttons in the button bar were showing the <td> as raising the event ??
    DirtyPage.clickID = "";         
}

 var images = new Array();
 function preloadImages(){
     for (var x=0; x < arguments.length; x++){
        images[x] = new Image();
        images[x].src = arguments[x];
     } 
 }
 
 
 function showProperties(obj){
    var s="";
     for(var prop in obj){ 
        s = s + "name:" + prop + " value:" + obj[prop] + "\n";
     } 
     alert(s);
 }

 function dumpControls() {
    // get list of page controls into a tab sep string suitable for inport to excel
    var Form1 = document.forms[0];
    var out = "ID\tType\tDisabled\tReadOnly\tValue\n"
    var x,y;
    for (i = 0; i < Form1.length; i++) {
        x =  Form1.elements[i]
        out += x.id + "\t" +
               x.type + "\t" +
               x.disabled + "\t" +
               x.readOnly

        switch (x.type) {
            case "text":
            case "textarea":
            case "button":
            case "hidden":
                out += "\t" + x.value;
                break;
            case "checkbox":
                out += "\t" + x.checked;
                break;
            case "select-one":
                out += "\t" + x.options[x.selectedIndex].value + ":" + 
                              x.options[x.selectedIndex].text ;
                break;
        }
        out += "\n";
    }
    // put breakpoint on line below and copy string from out variable.
    out = out;
 }


 function setFocus() {            
    if(IAW.getElementFocusID() == ""){ 
        var len=document.forms[0].elements.length;              
        for(var i=0;i<len;i++){
           var e = document.forms[0].elements[i];                        
           if(e.type == "text" || e.type == "password" || e.type == "textarea"){               
               if (!e.disabled && !e.readOnly && e.style.visibility != "hidden"){
                try{
                    e.focus();
                    //ie likes it twice, why?
                    e.focus();
                }
                catch(e){                    
                }
                return;   
               }                                                                              
           }                
        }                  
    }           
}//setFocus
        
function addOnFocusHandlers(){
   var len=document.forms[0].elements.length;                 
   
    for(var i=0;i<len;i++){            
       var e = document.forms[0].elements[i];
       if (e.type == "text" || e.type == "password" || e.type == "textarea") {
           $addHandler(e, "focus", setBgColor);
           $addHandler(e, "blur", removeBgColor);                                                                           
       }                
    }   
}//addOnFocusHandlers

//ignore enterkey press
function PreventEnter(e) {
    var ele = e.target;

    if (e.keyCode == 13)
    {
        e.stopPropagation();
        e.preventDefault();
    }
}


function setBgColor(e){
    var ele=e.target;       
    if (!ele.readOnly){
        var bgEle = $get("formFocus");
        var bgColor;

        if (!IAW.formFocus) {
            if (Sys.Browser.agent == Sys.Browser.InternetExplorer) {
                bgColor = bgEle.currentStyle.backgroundColor;
            }
            else {
                for (var x = 0; x < document.styleSheets.length; x++) {
                    if (document.styleSheets[x] != null) {
                        if (document.styleSheets[x].href != null) {
                            if (document.styleSheets[x].href.indexOf("form.css") > -1) {
                                for (var x2 = 0; x2 < document.styleSheets[x].cssRules.length; x2++) {
                                    if (document.styleSheets[x].cssRules[x2].selectorText == ".formFocus") {
                                        bgColor = document.styleSheets[x].cssRules[x2].style.backgroundColor;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        } else
            bgColor = IAW.formFocus;  
         
        if (!bgColor || bgColor == null || bgColor == "" || bgColor == "transparent") bgColor = "#ECEA44";
        IAW.formFocus = bgColor;
        ele.style.backgroundColor = bgColor; 
    }
}

function removeBgColor(e){
    var ele = e.target;
    if (!ele.readOnly) {
        ele.style.backgroundColor = "white";
    }
}

function openHelp(url) {
    window.open(url, "iWebHelp", "toolbar=no,menubar=no,scrollbars=yes,resizable=yes,status=no,top=50,left=50,width=700,height=400", true);
}

function changeCSS(theClass, element, value) {  
    var cssRules;   
    var added = false;
    for (var S = 0; S < document.styleSheets.length; S++) {

        if (document.styleSheets[S]['rules']) {
            cssRules = 'rules';
        } else if (document.styleSheets[S]['cssRules']) {
            cssRules = 'cssRules';
        } else {
            //no rules found... browser unknown        
        }

        for (var R = 0; R < document.styleSheets[S][cssRules].length; R++) {
            if (document.styleSheets[S][cssRules][R].selectorText == theClass) {
                if (document.styleSheets[S][cssRules][R].style[element]) {
                    document.styleSheets[S][cssRules][R].style[element] = value;
                    added = true;                    
                    break;
                }
            }
        }
        if (!added) {
            if (document.styleSheets[S].insertRule) {
                document.styleSheets[S].insertRule(theClass + ' { ' + element + ': ' + value + '; }', document.styleSheets[S][cssRules].length);
            } else if (document.styleSheets[S].addRule) {
                document.styleSheets[S].addRule(theClass, element + ': ' + value + ';');
            }
        }
    }
}

        
function toggle(id){ 
   var obj = $get(id);            
   if (obj.style.display=='none'){
    obj.style.display='';
   }
   else{           
     obj.style.display='none';
   }       
}  

//function funkyToggle(parent, objID){
//    var parentObj = $get(arguments[0]);
//    //apply filter
//    if (parentObj.filters.length > 0 ) parentObj.filters[0].Apply();             
//    for(var i = 1; i < arguments.length ; i++){                                
//         toggleVis(arguments[i]);             
//    }
//    //play filter                                     
//    if (parentObj.filters.length > 0 )  parentObj.filters[0].Play();        
//}   
        
       function toggleVis(objID){
            for(var i = 0; i < arguments.length ; i++){               
               var obj = $get(arguments[i]); 
               if(!obj) break;                                                                
               if (obj.style.visibility=='hidden'){
                obj.style.visibility='visible';                
               }
               else if(obj.style.display=='none'){           
                 obj.style.display='';                 
               }
               else if (obj.style.display!='none' && (obj.style.visibility==null || obj.style.visibility=='' ) ) {           
                 obj.style.display='none';                 
               }  
               else if(obj.style.visibility=='visible'){           
                 obj.style.visibility='hidden';                 
               }                
            }                          
        }   
        
        
        
           function getMouseX(e){
            var posx = 0;	            
	            if (!e) var e = window.event;
	            if (e.pageX)
	            {
		            posx = e.pageX;		            
	            }
	            else if (e.clientX)
	            {
		            posx = e.clientX + document.body.scrollLeft;		           
	            }
	           return posx         
            }
            
           function getMouseY(e){
            var posy = 0;	            
	            if (!e) var e = window.event;
	            if (e.pageY)
	            {
		            posy = e.pageY;		            
	            }
	            else if (e.clientY)
	            {
		            posy = e.clientY + document.body.scrollLeft;		           
	            }
	           return posy          
            }
            
            function findPosX(obj)
            {
	            var curleft = 0;
	            if (obj.offsetParent)
	            {
		            while (obj.offsetParent)
		            {
			            curleft += obj.offsetLeft
			            obj = obj.offsetParent;
		            }
	            }
	            else if (obj.x)
		            curleft += obj.x;
	            return curleft;
            }

            function findPosY(obj)
            {
	            var curtop = 0;
	            if (obj.offsetParent)
	            {
		            while (obj.offsetParent)
		            {
			            curtop += obj.offsetTop
			            obj = obj.offsetParent;
		            }
	            }
	            else if (obj.y)
		            curtop += obj.y;
	            return curtop;
            }

            var firsttime=0
            
			function setLayer(id){			
				var targ;
	            if (!e) var e = window.event;
	            if (e.target) targ = e.target;
	            else if (e.srcElement) targ = e.srcElement;
		                            
			    var x = new getObj(id);
			    if (typeof e != "undefined")
                {
                    if (x.style.zIndex > 0)
                    {
                        x.style.zIndex=-10;                        
                        x.style.display='none';
                    }
                    else
                    {
                         x.style.zIndex=50;       
                         x.style.display='';                 
                         if (firsttime==0){
                            x.style.top=  findPosY(targ) ;               
                            x.style.left=  findPosX(targ)+ targ.offsetWidth;
                            firsttime=1;
                         }
                                              
                    }                    
                }                            		    
			}           
             
             function getObj(name)
            {
              if ($get)
              {
                this.obj = $get(name);
                this.style = $get(name).style;
              }
              else if (document.all)
              {
                this.obj = document.all[name];
                this.style = document.all[name].style;
              }
              else if (document.layers)
              {
                this.obj = document.layers[name];
                this.style = document.layers[name];
              }
            }
     
     
     //**************** CODE FOR CLICK AND DRAG *****************************//
     
        mouseover=true
        id=""
        function coordinates(objName)
        {
            obj=new getObj(objName);
            if(!obj) return;            
            id=objName;      
            mouseover=true;	        
            pleft=obj.style.pixelLeft;	        
            ptop=obj.style.pixelTop;       
            xcoor=getMouseX();
            ycoor=getMouseY();
            document.onmousemove=moveImage;	        
        }

        function moveImage()
        {
        if (mouseover && event.button==1)
	        {
	        obj=new getObj(id);
	        obj.style.pixelLeft=pleft+ getMouseX() - xcoor;
	        obj.style.pixelTop=ptop+ getMouseY() - ycoor;
	        return false;
	        }
        }

        function mouseup()
        {
            mouseover=false;
            id="";
        }
     //**************** END CODE FOR CLICK AND DRAG *****************************//


/**
* strTrim
*
* examples:
* // example of using trim, ltrim, and rtrim
* var myString = " hello my name is ";
* alert("*"+myString.trim()+"*");
* alert("*"+myString.ltrim()+"*");
* alert("*"+myString.rtrim()+"*");
*/
String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g, "");
}
String.prototype.ltrim = function() {
    return this.replace(/^\s+/, "");
}
String.prototype.rtrim = function() {
    return this.replace(/\s+$/, "");
}

/**
 * strPad
 *
 * Pad a string to a certain length with another string
 *
 * This functions returns the input string padded on the left, the right, or both sides
 * to the specified padding length. If the optional argument pad_string is not supplied,
 * the output is padded with spaces, otherwise it is padded with characters from pad_string
 * up to the limit.
 *
 * The optional argument pad_type can be STR_PAD_RIGHT, STR_PAD_LEFT, or STR_PAD_BOTH.
 * If pad_type is not specified it is assumed to be STR_PAD_RIGHT.
 *
 * If the value of pad_length is negative or less than the length of the input string,
 * no padding takes place.
 *
 * object string
 * return string
 *
 * examples:
 *   var input = 'foo';
 *   input.strPad(9);                      // returns "foo      "
 *   input.strPad(9, "*+", STR_PAD_LEFT);  // returns "*+*+*+foo"
 *   input.strPad(9, "*", STR_PAD_BOTH);   // returns "***foo***"
 *   input.strPad(9 , "*********");        // returns "foo******"
 */

var STR_PAD_LEFT  = 0;
var STR_PAD_RIGHT = 1;
var STR_PAD_BOTH  = 2;

String.prototype.strPad = function(pad_length, pad_string, pad_type)
{
  /* Helper variables */
  var num_pad_chars   = pad_length - this.length;/* Number of padding characters */
  var result          = '';                       /* Resulting string */
  var pad_str_val     = ' ';
  var pad_str_len     = 1;                        /* Length of the padding string */
  var pad_type_val    = STR_PAD_RIGHT;            /* The padding type value */
  var i               = 0;
  var left_pad        = 0;
  var right_pad       = 0;
  var error           = false;
  var error_msg       = '';
  var output           = this;

  if (arguments.length < 2 || arguments.length > 4)
  {
    error     = true;
    error_msg = "Wrong parameter count.";
  }


  else if(isNaN(arguments[0]) == true)
  {
    error     = true;
    error_msg = "Padding length must be an integer.";
  }
  /* Setup the padding string values if specified. */
  if (arguments.length > 2)
  {
    if (pad_string.length == 0)
    {
      error     = true;
      error_msg = "Padding string cannot be empty.";
    }
    pad_str_val = pad_string;
    pad_str_len = pad_string.length;

    if (arguments.length == 3)
    {
      pad_type_val = pad_type;
      if (pad_type_val < STR_PAD_LEFT || pad_type_val > STR_PAD_BOTH)
      {
        error     = true;
        error_msg = "Padding type has to be STR_PAD_LEFT, STR_PAD_RIGHT, or STR_PAD_BOTH."
      }
    }
  }

  if(error) throw error_msg;

  if(num_pad_chars > 0 && !error)
  {
    /* We need to figure out the left/right padding lengths. */
    switch (pad_type_val)
    {
      case STR_PAD_RIGHT:
        left_pad  = 0;
        right_pad = num_pad_chars;
        break;

      case STR_PAD_LEFT:
        left_pad  = num_pad_chars;
        right_pad = 0;
        break;

      case STR_PAD_BOTH:
        left_pad  = Math.floor(num_pad_chars / 2);
        right_pad = num_pad_chars - left_pad;
        break;
    }

    for(i = 0; i < left_pad; i++)
    {
      output = pad_str_val.substr(0,num_pad_chars) + output;
    }

    for(i = 0; i < right_pad; i++)
    {
      output += pad_str_val.substr(0,num_pad_chars);
    }
  }

  return output;
}
 
 //refeshes the parent window and closes the currrent one
 function refreshParent() {  
  window.opener.location.reload();
  if (window.opener.progressWindow)		
  {
    window.opener.progressWindow.close()
  }
  window.close();
}

//dropdown lists, finds the index with passed in value
function GetIndexOfValue(obj, val) {
    if (obj.type == "select-one") {
        for (var i = 0; i < obj.length; i++) {
            if (obj.options[i].text == val) return i;
        }
    }
    return 0;
} //end

/*
GRID.JS
*/
function getParentTableID(ele) {
    if (ele.parentNode && ele.parentNode.tagName == "TABLE") {
        return ele.parentNode.id;
    } else if (ele.parentNode) {
        return getParentTableID(ele.parentNode);
    } else {
        //no parent
        return undefined;
    }
}
function CheckAll(ckBox) {
    var prefix = getParentTableID(ckBox);
    for (i = 0; i < document.forms[0].length; i++) {
        var o = document.forms[0][i];
        if (o.type == 'checkbox') {
            if (ckBox.id != o.id) {
                if (o.id.substring(0, prefix.length) == prefix) {
                    // Must be this way
                    o.checked = !ckBox.checked;
                    o.click();
                }
            }
        }
    }
}

// Initialises all Spectrum colour controls
function ApplyColourControls(argSelector = ".SpectrumColour", argRequired = false, localStorageKey = false, onchange = () => { }) {

    $(argSelector).spectrum({
        allowEmpty: argRequired,
        showInput: true,
        showInitial: true,
        showPalette: true,
        showSelectionPalette: false,
        maxPaletteSize: 81,
        clickoutFiresChange: true,
        preferredFormat: "rgb",
        showAlpha: true,
        cancelText: translations.spectrum_cancelText,
        chooseText: translations.spectrum_chooseText,
        showPaletteOnly: false,
        togglePaletteOnly: false,
        togglePaletteMoreText: translations.spectrum_moreText,
        togglePaletteLessText: translations.spectrum_lessText,
        localStorageKey: localStorageKey,
        darkenTooltip: translations.spectrum_darkenTooltip,
        lightenTooltip: translations.spectrum_lightenTooltip,
        transparentTooltip: translations.spectrum_transparentTooltip,
        clearTextTooltip: translations.spectrum_clearTooltip,
        noColorSelectedTooltip: translations.spectrum_noColorSelectedTooltip,
        redTooltip: translations.spectrum_redTooltip,
        greenTooltip: translations.spectrum_greenTooltip,
        blueTooltip: translations.spectrum_blueTooltip,
        transparencyTooltip: translations.spectrum_transparencyTooltip,
        palette: [
            ["#000000", "#434343", "666666", "#999999", "#b7b7b7", "#cccccc", "#d9d9d9", "#efefef", "#f3f3f3", "#ffffff"],
            ["#980000", "#ff0000", "ff9900", "#ffff00", "#00ff00", "#00ffff", "#4a86e8", "#0000ff", "#9900ff", "#ff00ff"],
            ["#e6b8af", "#f4cccc", "fce5cd", "#fff2cc", "#d9ead3", "#d9eaef", "#c9daf8", "#cfe2f3", "#d9d2e9", "#ead1dc"],
            ["#dd7e6b", "#ea9999", "f9cb9c", "#ffe599", "#b6d7a8", "#a2c4c9", "#a4c2f4", "#9fc5e8", "#b4a7d6", "#d5a6bd"],
            ["#cc4125", "#e06666", "f6b26b", "#ffd966", "#93c47d", "#76a5af", "#6d9eeb", "#6fa8dc", "#8e7cc3", "#c27ba0"],
            ["#a61c00", "#cc0000", "e69138", "#f1c232", "#6aa84f", "#45818e", "#3c78d8", "#3d85c6", "#674ea7", "#a64d79"],
            ["#85200c", "#990000", "b45f06", "#bf9000", "#38761d", "#134f5c", "#1155cc", "#0b5394", "#351c75", "#741b47"],
            ["#5b0f00", "#660000", "783f04", "#7f6000", "#274e13", "#0c343d", "#1c4587", "#073763", "#20124d", "#4c1130"],
            ["#00000000"]
        ],

        move: function (color) {
            if (!color) return
            var el = $ID($(this).attr('id'));
            if (el) {
                el.value = color.toRgbString()
            }
            //callback onchange function
            onchange(color, this.dataset.index)
        }
    });
}

// resize a div to fit on the screen
$.fn.ResizeDiv = function () {
    this.each((i, divo) => {
        if (divo) {
        var vh = (window.innerHeight || document.documentElement.clientHeight),
            rect = divo.getBoundingClientRect(),
            sTop = window.scrollY || document.documentElement.scrollTop;
            divo.style.height = (vh - (rect.top + sTop) - 20) + "px";
    }
    })
}

//----------------------------------------------------------------------------------------------------------------------
// Logging
//----------------------------------------------------------------------------------------------------------------------
var iawDebugLevel = 2;       // 0 = No logging, 1 = Errors only, 2 = Logging, 3 = Verbose
var iawError   = iawDebugLevel > 0 ? console.error.bind(window.console) : function () {};
var iawLog     = iawDebugLevel > 1 ? console.log.bind(window.console) : function () {};
var iawVerbose = iawDebugLevel > 2 ? console.log.bind(window.console) : function () {};

//----------------------------------------------------------------------------------------------------------------------
// Maintain Scroll position for div elements that have the divScroll class
//----------------------------------------------------------------------------------------------------------------------
function restoreScrollPosition() {
    // Get the hidden field using shortcut function
    var hiddenField = $ID('hdnDivSrollPos');

    // Set the scroll positions from the hidden field
    if (hiddenField.value) {
        var scrollData = JSON.parse(hiddenField.value);
        scrollData.forEach(function (data) {
            var div = $ID(data.id);
            if (div) {
                div.scrollTop = data.scrollTop;
            }
        });
    }

    // Add scroll event listeners to the divs
    var divs = $QSAll('.divScroll');
    divs.forEach(function (div) {
        div.addEventListener('scroll', saveScrollPositions);

        var selectedRow = div.querySelector('.selected_row');
        if (selectedRow) {
            // You might want to adjust the scrolling behavior or position here
            selectedRow.scrollIntoView({ block: "nearest" });
        }
    });
}
function saveScrollPositions() {
    // Save the scroll positions to the hidden field
    var scrollData = [];
    var divs = $QSAll('.divScroll');
    divs.forEach(function (div) {
        scrollData.push({ id: div.id, scrollTop: div.scrollTop });
    });
    var hiddenField = $ID('hdnDivSrollPos');
    hiddenField.value = JSON.stringify(scrollData);
}
function resetScrollPosition(divId) {
    var div = $ID(divId);
    if (div) {
        div.scrollTop = 0;
    }
}
window.onload = function () {
    restoreScrollPosition();
}
window.onresize = () => $('.grid-to-bottom').ResizeDiv();
$(document).ready(function () {
    $('.grid-to-bottom').ResizeDiv();
});


//----------------------------------------------------------------------------------------------------------------------
// fix problem with jquery touch events
//----------------------------------------------------------------------------------------------------------------------
//$(document).ready(function () {
$(function () {
    // fix warnings caused by jquery
    jQuery.event.special.touchstart = {
        setup: function (_, ns, handle) {
            if (ns.includes("noPreventDefault")) {
                this.addEventListener("touchstart", handle, { passive: false });
            } else {
                this.addEventListener("touchstart", handle, { passive: true });
            }
        }
    }

    const menuButton = document.getElementById('Menu1n0')
    if (menuButton) {
        menuButton.onmousedown = () => {
            const menuItems = document.querySelector('.DynamicMenuStyle')
            setTimeout(() => {
                menuItems.style.visibility = 'visible';
                menuItems.style.display = 'inline';
            }, 10)
        }
    }

})

//----------------------------------------------------------------------------------------------------------------------
// Ajax Control Toolkit Tab Container functions
//----------------------------------------------------------------------------------------------------------------------
function ATC_setTabVisibility(tabContainerId, tabPanelId, isVisible, autoFix) {
    autoFix = autoFix === undefined ? false : autoFix;

    var tabContainer = $find(tabContainerId);
    if (tabContainer) {
        var tabPanels = tabContainer.get_tabs();
        for (var i = 0; i < tabPanels.length; i++) {
            if (tabPanels[i].get_id() === tabPanelId) {
                var displayStyle = isVisible ? "" : "none";

                // Set the visibility of the tab header
                tabPanels[i].get_headerTab().style.display = displayStyle;

                if (autoFix)
                    ATC_fixTabClasses(tabContainerId);

                break;
            }
        }
    } else {
        iawError('TabContainer not found');
    }
}

function ATC_fixTabClasses(tabContainerId) {
    var tabContainer = $find(tabContainerId);
    if (tabContainer) {
        // Mark this tab as manual first-last control
        var tabHeader = $ID(tabContainerId + "_header");
        if (tabHeader) {
            tabHeader.classList.add("manual-tab");
        }

        var tabPanels = tabContainer.get_tabs();
        var visibleTabs = [];

        // Find all visible tabs
        for (var i = 0; i < tabPanels.length; i++) {
            if (tabPanels[i].get_headerTab().style.display !== 'none') {
                visibleTabs.push(tabPanels[i]);
            }
        }

        // Remove existing classes from all tabs
        for (var i = 0; i < tabPanels.length; i++) {
            $(tabPanels[i].get_headerTab()).removeClass('first-tab last-tab');
        }

        // Set class for the first visible tab
        if (visibleTabs.length > 0) {
            $(visibleTabs[0].get_headerTab()).addClass('first-tab');
            ATC_setActiveTab(tabContainerId, visibleTabs[0].get_id());

            // If there's only one visible tab, it's also the last tab
            if (visibleTabs.length === 1) {
                $(visibleTabs[0].get_headerTab()).addClass('last-tab');
            }
        }

        // Set class for the last visible tab (if there's more than one visible tab)
        if (visibleTabs.length > 1) {
            $(visibleTabs[visibleTabs.length - 1].get_headerTab()).addClass('last-tab');
        }
    } else {
        iawError('TabContainer not found');
    }
}

function ATC_setActiveTab(tabContainerId, tabPanelId) {
    var tabContainer = $find(tabContainerId);
    if (tabContainer) {
        var tabPanels = tabContainer.get_tabs();
        for (var i = 0; i < tabPanels.length; i++) {
            if (tabPanels[i].get_id() === tabPanelId) {
                // Set the active tab by its index
                tabContainer.set_activeTabIndex(i);
                break;
            }
        }
    } else {
        iawError('TabContainer not found');
    }
}

// function to set the value of a dropdown
function setDropDownListSelectedValue(ddl, value) {
    for (var i = 0; i < ddl.options.length; i++) {
        if (ddl.options[i].value == value) {
            ddl.selectedIndex = i;
            break;
        }
    }
}

function tableFilter(elm) {
    const query = elm.value.toLowerCase();
}

function BannerEnabled(isEnabled) {
    if (isEnabled) {
        $("#Menu1").removeClass("noClick");
        $("#lnkLogout").removeClass("noClick");
        $("#lnkMessages").removeClass("noClick");
    } else {
        $("#Menu1").addClass("noClick");
        $("#lnkLogout").addClass("noClick");
        $("#lnkMessages").addClass("noClick");
    }
}

// ----------------------------------------------------------------------------------------
// Spectrum Colour Functions
//
function spGetColour(el) {
    // Get the colour value out of the picker
    el = spGetElement(el); 
    if (!el) return ""; 
    if (!el.spectrum("props").visibility) return "";
    var val = el.spectrum("get");
    return val == null || val === "" ? "" : val.toRgbString().toUpperCase();
}

function spSetColour(el, val) {
    // Set the colour value of the picker
    el = spGetElement(el);
    if (!el || val === null) return;
    el.spectrum("set", val);
}

function spShowControl(el) {
    // Show the colour picker
    el = spGetElement(el); 
    if (!el) return; 
    el.spectrum("showSpect");
}

function spHideControl(el) {
    // Hide the colour picker
    el = spGetElement(el);
    if (!el) return; 
    el.spectrum("hideSpect");
}

function spControlVisible(el) {
    // Is the colour picker visible?
    el = spGetElement(el); 
    if (!el) return false; 
    return el.spectrum("props").visibility;
}

// Utility function to get jQuery element from ID or DOM element
function spGetElement(el)
{
    // Check if el is a string (ID) or a DOM element
    if (typeof el === 'string')
    {
        // Ensure the element exists before wrapping
        return $('#' + el).length ? $('#' + el) : undefined;
    } else if (el instanceof HTMLElement)
    {
        // If it's a DOM element, wrap it with jQuery
        return $(el);
    } else
    {
        // If el is neither a string nor a DOM element, return undefined
        return undefined;
    }
}

function elShow(el)
{
    el = spGetElement(el);
    if (!el) return;

    switch (el.prop('nodeName').toLowerCase())
    {
        case 'tr':
            el.css('display', 'table-row');
            break;
        case 'option':
            el.show();
            break;
        default:
            el.removeClass("hidden");
            break;
    }
}

function elHide(el)
{
    el = spGetElement(el);
    if (!el) return;

    switch (el.prop('nodeName').toLowerCase())
    {
        case 'tr':
            el.css('display', 'none');
            break;
        case 'option':
            el.hide();
            break;
        default:
            el.addClass("hidden");
            break;
    }
}

function setDisplay(setting, ...elementIds)
{
    elementIds.forEach(function (id)
    {
        const element = $ID(id);
        if (element)
        {
            element.style.display = setting;
        }
    });
}


