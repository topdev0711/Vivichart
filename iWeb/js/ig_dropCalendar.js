// This file allows to link drop-down WebCalendar with WebDateTimeEdit
//
// reference to drop-down calendar
var ig_calToDrop;
// reference to transparent iframe used to get around bugs in IE related to <select>
var ig_frameUnderCal;
// event fired by WebDateTimeEdit on CustomButton
function ig_openCalEvent(oEdit)
{
	// if it belongs to another oEdit, then close ig_calToDrop
	if(ig_calToDrop.oEdit != oEdit)
	{
		ig_showHideCal(null, false);
		// set reference in ig_calToDrop to this oEdit
		ig_calToDrop.oEdit = oEdit;
	}
	// show calendar with date from editor
	ig_showHideCal(oEdit.getDate(), true, true, true);
}
// synchronize dates in DateEdit and calendar, and show/close calendar
function ig_showHideCal(date, show, update, toggle)
{
	if(update != true) update = false;
	var oEdit = ig_calToDrop.oEdit;
	if(toggle == true && ig_calToDrop.isDisplayed == true)
		show = update = false;
	// update editor with latest date
	if(update && oEdit != null)
	{
		if(ig_calToDrop.isDisplayed) oEdit.setDate(date);
		else ig_calToDrop.setSelectedDate(oEdit.getDate());
	}
	// check current state of calendar
	if(ig_calToDrop.isDisplayed == show)
		return;
	// show/hide calendar
	ig_calToDrop.element.style.display = show ? "" : "none";
	ig_calToDrop.element.style.visibility = show ? "visible" : "hidden";
	ig_calToDrop.isDisplayed = show;
	if(show)
		ig_setCalPosition();
	else
	{
		if(ig_frameUnderCal != null)
			ig_frameUnderCal.hide();
		ig_calToDrop.oEdit = null;
	}
}
// set position of calendar below DateEdit
function ig_setCalPosition()
{
	var edit = ig_calToDrop.oEdit.Element;
	var pan = ig_calToDrop.element;
	if(ig_frameUnderCal == null && ig_csom.IsIEWin)
	{	
		ig_frameUnderCal = ig_csom.createTransparentPanel();
		if(ig_frameUnderCal != null)
			ig_frameUnderCal.Element.style.zIndex = 10001;
	}
	//v-v-v-v-v-v-v-v-v-v
	// Lines below came from ig_WebDropDown.js and should cover most situations.
	// Note: WebCalendar should be located directly in "body", rather inside of <div>, <td>, etc.
	// That allows to avoid potentially buggy "move" which is not implemented here.
	var panH=pan.offsetHeight,panW=pan.offsetWidth,e=edit,body=window.document.body;
	var editH=e.offsetHeight,editW=e.offsetWidth;
	if(editH==null)editH=0;
	var f,z,ok=0,x=0,y=0,pe=e,bp=body.parentNode;
	while(e!=null)
	{
		if(ok<1||e==body){if((z=e.offsetLeft)!=null)x+=z;if((z=e.offsetTop)!=null)y+=z;}
		if(e.nodeName=="HTML")body=e;if(e==body)break;
		z=e.scrollLeft;if(z==null||z==0)z=pe.scrollLeft;if(z!=null&&z>0)x-=z;
		z=e.scrollTop;if(z==null||z==0)z=pe.scrollTop;if(z!=null&&z>0)y-=z;
		pe=e.parentNode;e=e.offsetParent;if(pe.tagName=="TR")pe=e;
		if(e==body&&pe.tagName=="DIV"){e=pe;ok++;}
	}
	if(document.elementFromPoint)
	{
		var xOld=x,yOld=y;ok=true;
		var i=1,x0=body.scrollLeft,y0=body.scrollTop,ed=ig_calToDrop.oEdit.elem;
		while(++i<16)
		{
			z=(i>2)?((i&2)-1)*(i&14)/2*5:2;
			e=document.elementFromPoint(x+z-x0,y+z-y0);
			if(!e||e==ed||e==edit)break;
		}
		if(i>15||!e)ok=false;
		x+=z;y+=z;i=0;z=0;
		while(ok&&++i<22)
		{
			if(z==0)x--;else y--;
			e=document.elementFromPoint(x-x0,y-y0);
			if(!e||i>20)ok=false;
			if(e!=ed&&e!=edit)if(z>0)break;else{i=z=1;x++;}
		}
		if(ok){x--;y--;}else{x=xOld;y=yOld;}
	}
	y+=editH;
	z=body.clientHeight;
	if(z==null||z<20){z=pe.offsetHeight;f=body.offsetHeight;if(f>z)z=f;}
	else{if(bp&&(f=bp.offsetHeight)!=null)if(f>panH&&f<z)z=f-10;}
	if((f=body.scrollTop)==null)f=0;if(f==0&&bp)if((f=bp.scrollTop)==null)f=0;
	if(z<y-f+panH){if(y-f-3>panH+editH)y-=panH+editH;else y=z+f-panH;}if(y<f)y=f;
	z=body.clientWidth;
	if(z==null||z<20){z=pe.offsetWidth;f=body.offsetWidth;if(f>z)z=f;}
	else{if(bp&&(f=bp.offsetWidth)!=null)if(f>panW&&f<z)z=f-20;}
	if((f=body.scrollLeft)==null)f=0;if(f==0&&bp)if((f=bp.scrollLeft)==null)f=0;
	if(x+panW>z+f)x=z+f-panW;if(x<f)x=f;
	if(ig_csom.IsMac&&(ig_csom.IsIE||ig_csom.IsSafari)){x+=ig_csom.IsIE?5:-5;y+=ig_csom.IsIE?11:-7;}
	pan.style.left=x+"px";
	pan.style.top=y+"px";
	//^-^-^-^-^-^-^-^-^-^
	if(ig_frameUnderCal != null)
	{
		ig_frameUnderCal.setPosition(y - 1, x - 1, panW + 2, panH + 2);
		ig_frameUnderCal.show();
	}
}
// process mouse click events for page: close drop-down
function ig_globalMouseDown(evt)
{
	// find source element
	if(evt == null) evt = window.event;
	if(evt != null)
	{
		var elem = evt.srcElement;
		if(elem == null) if((elem = evt.target) == null) elem = this;
		while(elem != null)
		{
			// ignore events that belong to calendar
			if(elem == ig_calToDrop.element) return;
			elem = elem.offsetParent;
		}
	}
	// close calendar
	ig_showHideCal(null, false, false);
}
function ig_closeCalEvent(oEdit){ig_showHideCal(null, false);}
function ig_initDropCalendar(calendarAndDates)
{
	var ids = calendarAndDates.split(" ");
	ig_frameUnderCal = null;
	ig_calToDrop = igcal_getCalendarById(ids[0]);
	if(ig_calToDrop == null)
	{
		alert("WebCalendar with id=" + ids[0] + " was not found");
		return;
	}
	ig_calToDrop.element.style.zIndex = 10002;
	ig_calToDrop.element.style.position = "absolute";
	ig_calToDrop.isDisplayed = true;
	// hide drop-down calendar
	ig_showHideCal(null, false);
	// it is called by date click events of WebCalendar
	// Note: that name should match with the ClientSideEvents.DateClicked property
	//  which is set in aspx for WebCalendar
	ig_calToDrop.onValueChanged = function(cal, date)
	{
		// update editor with latest date and hide calendar
		ig_showHideCal(date, false, true);
	}
	// add listener to mouse click events for page
	ig_csom.addEventListener(window.document, "mousedown", ig_globalMouseDown);
	for(var i = 1; i < ids.length; i++)
	{
		var edit = igedit_getById(ids[i]);
		if(edit == null)
		{
			alert("WebDateTimeEdit with id=" + ids[i] + " was not found");
			continue;
		}
		edit.addEventListener("focus", ig_closeCalEvent, edit);
		edit.addEventListener("spin", ig_closeCalEvent, edit);
		edit.addEventListener("keydown", ig_closeCalEvent, edit);
		edit.addEventListener("custombutton", ig_openCalEvent, edit);
	}
}