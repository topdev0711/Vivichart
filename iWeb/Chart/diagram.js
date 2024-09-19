//-----------------------------------------------------------------------------------------------
// Initialise Editors
//-----------------------------------------------------------------------------------------------

var ControlsInitialised = false;
function InitialiseControls() {
    CreateHTMLeditor();

    if (MGLB.model_editable == true && !ControlsInitialised) {
        // create the controls for maintaining the text line attributes
        for (var i = 1; i < 7; i++) {
            nodeAttrs[i] = new LineAttrib($ID("lblTeamLine" + i), $ID("ddlbFont"), true);
            modelAttrs[i] = new LineAttrib($ID("lblModelLine" + i), $ID("ddlbFont"));
        }
        ControlsInitialised = true;
    }

    // Initialize the color pickers based on checkbox state
    $('span[data-target] input[type="checkbox"]').each(function () {
        toggleColorPicker($(this));
    }).on('change', function () {
        toggleColorPicker($(this));
    });

}
function CreateHTMLeditor() {
    if (NoteEditor) return;
    CreateTinyMCEInstance('txtNoteArea').then(editors => {
        NoteEditor = editors[0];
    });
}  
function CreateTinyMCEInstance(el) {
    // Processing font array (font_name, font_string) to create the font_formats string
    var fontArray = MGLB.fonts.map(font => `${font.font_name}=${font.font_string}`);
    var font_formats = fontArray.join(";");
    var def_font = MGLB.fonts[0].font_name;

    // these two lines clear the localStorage items that may be left after previous op
    localStorage.removeItem("tinymce-custom-colors-forecolor");
    localStorage.removeItem("tinymce-custom-colors-hilitecolor");

    return tinymce.init({
        selector: `#${el}`,
        height: 250,
        min_height: 250,
        max_height: 250,
        min_width: 350,
        resize: "both",
        elementpath: false,
        promotion: false,
        branding: false,
        menubar: false,
        toolbar: 'fontfamily fontsize | bold italic underline | forecolor backcolor ', // Customize the toolbar
        content_style: "body { font-family: " + def_font + "; }",  // sets the default font to Arial
        font_family_formats: font_formats,

        color_map: [
            "000000", "Black",
            "434343", "Dove Gray",
            "666666", "Gray",
            "999999", "Spanish Gray",
            "b7b7b7", "Gray (X11)",
            "cccccc", "Light Gray",
            "d9d9d9", "Gainsboro",
            "efefef", "Isabelline",
            "f3f3f3", "Magnolia",
            "ffffff", "White",
            "980000", "Chestnut",
            "ff0000", "Red",
            "ff9900", "Neon Carrot",
            "ffff00", "Yellow",
            "00ff00", "Lime",
            "00ffff", "Aqua",
            "4a86e8", "Blue (Crayola)",
            "0000ff", "Blue",
            "9900ff", "Violet",
            "ff00ff", "Magenta",
            "e6b8af", "Pale Chestnut",
            "f4cccc", "Bubble Gum",
            "fce5cd", "Peach-Orange",
            "fff2cc", "Peach Yellow",
            "d9ead3", "Eton Blue",
            "d9eaef", "Platinum",
            "c9daf8", "Periwinkle",
            "cfe2f3", "Lavender Blue",
            "d9d2e9", "Thistle",
            "ead1dc", "Pink Lace",
            "dd7e6b", "Copper Rose",
            "ea9999", "Cameo Pink",
            "f9cb9c", "Desert Sand",
            "ffe599", "Gold (Crayola)",
            "b6d7a8", "Granny Smith Apple",
            "a2c4c9", "Blue-Green",
            "a4c2f4", "Blue Bell",
            "9fc5e8", "Baby Blue",
            "b4a7d6", "Blue Bell",
            "d5a6bd", "English Lavender",
            "cc4125", "Vermilion",
            "e06666", "Salmon",
            "f6b26b", "Light Salmon",
            "ffd966", "Dandelion",
            "93c47d", "Asparagus",
            "76a5af", "Cadet Blue",
            "6d9eeb", "Cornflower Blue",
            "6fa8dc", "Little Boy Blue",
            "8e7cc3", "Ceil",
            "c27ba0", "English Lavender",
            "a61c00", "Mahogany",
            "cc0000", "Red (Munsell)",
            "e69138", "Tiger's Eye",
            "f1c232", "Gold (Metallic)",
            "6aa84f", "Fern Green",
            "45818e", "Steel Blue",
            "3c78d8", "Medium Persian Blue",
            "3d85c6", "Steel Blue",
            "674ea7", "Blue-Violet",
            "a64d79", "Old Rose",
            "85200c", "Caput Mortuum",
            "990000", "Carmine",
            "783f04", "Russet",
            "7f6000", "Amber (SAE/ECE)",
            "38761d", "Brunswick Green",
            "134f5c", "Dark Cyan",
            "1155cc", "Blue (RYB)",
            "0b5394", "Dark Cerulean",
            "351c75", "Indigo (Web)",
            "20124d", "Midnight Blue",
        ],
        color_cols_foreground: 10,
        color_cols_background: 10,
    });
}

//-----------------------------------------------------------------------------------------------
// Node Settings Dialog
//-----------------------------------------------------------------------------------------------

function addNodeEvents() {
    $ID("btnApplyNodeSettings").addEventListener("click", (event) => { ApplyNodeSettings(event); });
    $ID("btnApplyNewNode").addEventListener("click", (event) => { ApplyNewNode(event); });
    $ID("btnCancelNodeSettings").addEventListener("click", (event) => { CancelNodeSettings(event); });
    $ID("btnApplyDefaults").addEventListener("click", (event) => { ApplyNodeDefaults(event); });

    $ID("iUndoLine1").addEventListener("click", (event) => { ResetLineToDs(event, 1); });
    $ID("iUndoLine2").addEventListener("click", (event) => { ResetLineToDs(event, 2); });
    $ID("iUndoLine3").addEventListener("click", (event) => { ResetLineToDs(event, 3); });
    $ID("iUndoLine4").addEventListener("click", (event) => { ResetLineToDs(event, 4); });
    $ID("iUndoLine5").addEventListener("click", (event) => { ResetLineToDs(event, 5); });
    $ID("iUndoLine6").addEventListener("click", (event) => { ResetLineToDs(event, 6); });

    $ID("btnApplyLinkSettings").addEventListener("click", (event) => { ApplyLinkSettings(event); });
    $ID("btnCancelLinkSettings").addEventListener("click", (event) => { CancelLinkSettings(event); });
}
function addUndoEvents()
{
    $ID("btnUndoOk").addEventListener("click", (event) =>
    {
        ModelCancel(event); $find('mpeUndo').hide();
        typenode = "NA";
    });
    $ID("btnUndoCancel").addEventListener("click", (event) => { 
        // Cancel button on the node settings form
        event.preventDefault();

        // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
        if ($ID("btnUndoCancel").hasAttribute("disabled")) return;

        $find('mpeUndo').hide();
        typenode = "NA";
        return false;
    });
}

function NodeSettingsFormConfiguration(node_type, data) {

    // hide all the tabs
    ATC_setTabVisibility('nodeTabContainer', 'tpNodeGeneral', false); // Hide
    ATC_setTabVisibility('nodeTabContainer', 'tpNodeTeam', false); // Hide
    ATC_setTabVisibility('nodeTabContainer', 'tpNodeNote', true); // Always Show
    ATC_setTabVisibility('nodeTabContainer', 'tpNodeIcon', false); // Hide
    $('#nodeTabContainer').removeClass('extended');

    // set the class of the node tab container to the standard 'large screen' class
    $("#nodeTabContainer").removeClass("nodeTabContainerAll");
    $("#nodeTabContainer").removeClass("nodeTabContainerGroup");
    $("#nodeTabContainer").addClass("nodeTabContainerAll");

    // show / hide controls based on whether the user is allowed to override settings
    elHide("tooltipColours");
    elHide("ddlbApplyTypes");
    elHide("ddlbApplyOptions");

    if (MGLB.can_override == true) {
        elShow("tooltipColours");
        elShow("ddlbApplyTypes");
        elShow("ddlbApplyOptions");
    }

    $ID("ddlbApplyOptions").value = 'Apply';                    // reset to apply to this node only

    // the lines on the team page.. we have to be able to suppress lines 2-6 on the parent group screen,
    //  so we have to ensure they're visible for the others when the popup is opened again.

    $("#trLine1").show();
    $("#trLine2").show();
    $("#trLine3").show();
    $("#trLine4").show();
    $("#trLine5").show();
    $("#trLine6").show();

    if (MGLB.can_override) {
        $(".teamAttr").show();
    } else {
        $(".teamAttr").hide();
    }

    nodeAttrs[1].setControlVisibility("left", true);
    nodeAttrs[1].setControlVisibility("center", true);
    nodeAttrs[1].setControlVisibility("right", true);
    nodeAttrs[1].setControlVisibility("colour", true);
    nodeAttrs[1].setControlVisibility("bg_colour", true);

    // label for the 1st Line normally says 'Lines' , but for parent group, it needs to say "Title"
    $('#lblLines').show();
    $('#lblGroupTitle').hide();

    // box height and width are not relevant to the parentgroup so ensure they're re-displayed here
    // and hidden in the parentgroup specific section bedlow
    $('#trBoxWidth').show();
    $('#trBoxHeight').show();

    // nodes across is only relevant to the parent group, so hide it
    $('#trNodesAcross').hide();
    $('#trNodesAcrossInfo').hide();

    // show the relevant sections of the dialog and change the header text
    switch (node_type) {
        case 'Detail':
            $('#popNodeSettings fieldset legend').text(MGLB.dialog_node_details);
            ATC_setTabVisibility('nodeTabContainer', 'tpNodeTeam', true); // show

            if (MGLB.can_override == true) {
                ATC_setTabVisibility('nodeTabContainer', 'tpNodeGeneral', true); // show
                ATC_setActiveTab('nodeTabContainer', 'tpNodeGeneral');
            } else {
                ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');
            }
            break;

        case 'Team':
            $('#popNodeSettings fieldset legend').text(MGLB.dialog_team_details);

            if (MGLB.can_override == true) {
                ATC_setTabVisibility('nodeTabContainer', 'tpNodeGeneral', true); // show
            }
            ATC_setTabVisibility('nodeTabContainer', 'tpNodeTeam', true); // show
            break;
            
        case 'Vacant':
            $('#popNodeSettings fieldset legend').text(MGLB.dialog_vacant_details);
            ATC_setTabVisibility('nodeTabContainer', 'tpNodeTeam', true); // show

            if (MGLB.can_override == true) {
                ATC_setTabVisibility('nodeTabContainer', 'tpNodeGeneral', true); // show
                ATC_setActiveTab('nodeTabContainer', 'tpNodeGeneral');
            } else {
                ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');
            }

            break;

        case 'Label':
            $('#popNodeSettings fieldset legend').text(MGLB.dialog_label_details);
            elHide("ddlbApplyOptions");
            elHide("ddlbApplyTypes");

            $("#nodeTabContainer").addClass("nodeTabContainerGroup");

            ATC_setTabVisibility('nodeTabContainer', 'tpNodeNote', true); // show
            ATC_setTabVisibility('nodeTabContainer', 'tpNodeIcon', true); // show
            $('#nodeTabContainer').addClass('extended');

            break;

        case 'ParentGroup':
            $('#popNodeSettings fieldset legend').text(MGLB.parent_group_text);

            $("#nodeTabContainer").addClass("nodeTabContainerGroup");

            $("#trLine2").hide();
            $("#trLine3").hide();
            $("#trLine4").hide();
            $("#trLine5").hide();
            $("#trLine6").hide();

            if (!data.node_text_bg_block) {
                nodeAttrs[1].setControlVisibility("left", false);
                nodeAttrs[1].setControlVisibility("center", false);
                nodeAttrs[1].setControlVisibility("right", false);
            }
            nodeAttrs[1].setControlVisibility("colour", false);
            nodeAttrs[1].setControlVisibility("bg_colour", false);

            $('#lblLines').hide();              // hide the 'Lines' label
            $('#lblGroupTitle').show();         // show the 'Title' label
            $('#trBoxWidth').hide();            // hide the box width and height sliders
            $('#trBoxHeight').hide();
            $('#trNodesAcross').show();         // nodes across only relevant to parent groups
            $('#trNodesAcrossInfo').show();     // nodes across only relevant to parent groups

            ATC_setTabVisibility('nodeTabContainer', 'tpNodeGeneral', true); // show
            ATC_setTabVisibility('nodeTabContainer', 'tpNodeTeam', true); // show
            ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');

            break;
    }
    ATC_fixTabClasses('nodeTabContainer');
}
function AddTeamNode(node) {
    // Pouplate the fields on the node settings form and then display it
    typenode = "Team";

    // apply the spectrum colour dialog control
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    hideUndos();

    for (var i = 1; i < 7; i++)
    {
        // set the text boxes to blank
        $ID("txtTeamLine" + i).value = "";

        // set the font/size/colour etc attributes for the line
        nodeAttrs[i].setValue(JSON.stringify(GetSingleLineAttrs(MGLB.line_attr, i)));
    }

    var TeamCount = 0;
    myDiagram.nodes.each(function (node)
    {
        if (node.data.node_type === "Team")
        {
            TeamCount += 1;
        }
    });

    $ID("txtTeamLine1").value = MGLB.dialog_team_details + ' ' + MGLB.hash_char + (TeamCount + 1);

    $ID("txtTooltip").value = "";
    NoteEditor.setContent('');
    SetNoteWidth(600);

    $ID("trShowPhoto").style.display = "none";
    $ID("cbPrivateLabel").checked = false

    spSetColour("txtNodeBoxfg", MGLB.node_fg);
    spSetColour("txtNodeBoxbg", MGLB.node_bg);
    spSetColour("txtNodeBorderfg", MGLB.node_border_fg);
    spSetColour("txtNodeTextBg", MGLB.node_text_bg);
    spSetColour("txtNodeIconfg", MGLB.node_icon_fg);
    spSetColour("txtNodeIconHover", MGLB.node_icon_hover);

    var checkbox = $ID("cbNodeTextBgBlock");
    checkbox.checked = MGLB.node_text_bg_block;
    checkbox.removeEventListener('change', blockCheckboxChangeHandler);

    spSetColour("txtNodeTooltipfg", MGLB.node_tt_fg);
    spSetColour("txtNodeTooltipbg", MGLB.node_tt_bg);
    spSetColour("txtNodeTooltipBorder", MGLB.node_tt_border);

    $ID('numNodeBoxHeight').value = MGLB.node_height;
    $ID('numNodeBoxWidth').value = MGLB.node_width;
    $find('bhNodeBoxHeight').set_value(MGLB.node_height);
    $find('bhNodeBoxWidth').set_value(MGLB.node_width);

    $ID('rbNodeCornerRectangle').checked = MGLB.node_corners == 'Rectangle';
    $ID('rbNodeCornerRoundedRectangle').checked = MGLB.node_corners == 'RoundedRectangle';

    $ID('cbNodeShowShadow').checked = MGLB.showShadow;
    spSetColour("txtNodeShadowColour", MGLB.shadowColour);

    NodeSettingsFormConfiguration("Team");
    ShowNodeApply();

    // Display the dialog
    $ID("btnApplyNewNode").style.display = "inline-block";
    $find('mpeNodeSettingsForm').show();

    ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');
    setTimeout(() => {
        $ID('txtTeamLine1').focus();
    }, 0);

    return false;
}
function AddLabelNode() {
    // Pouplate the fields on the node settings form and then display it

    typenode = "Label";

    // apply the spectrum colour dialog control
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    // for the icon background change
    ApplyColourControls(".SpectrumColourChange", true, 'vcPalette', () => RecolourIconBG());

    elHide("trShowPhoto");
    $ID("txtTooltip").value = "";
    NoteEditor.setContent('');
    SetNoteWidth(600);

    $ID("cbPrivateLabel").checked = false

    $ID("rbLabelIcon").value = "";
    $('#rbLabelIcon input[type="radio"]').first().prop('checked', true);

    spSetColour("txtIconfg", MGLB.node_fg);
    spSetColour("txtIconbg", MGLB.node_bg);
    spSetColour("txtIconBorder", MGLB.node_border_fg);

    $ID('rbIconCircle').checked = true;
    $ID('rbIconSquare').checked = false;

    spSetColour("txtNodeTooltipfg", MGLB.node_tt_fg);
    spSetColour("txtNodeTooltipbg", MGLB.node_tt_bg);
    spSetColour("txtNodeTooltipBorder", MGLB.node_tt_border);

    NodeSettingsFormConfiguration("Label");
    RecolourIconBG();
    ShowNodeApply();

    // Display the dialog
    $ID("btnApplyNewNode").style.display = "inline-block";
    $find('mpeNodeSettingsForm').show();

    return false;
}
function NodeSettingsForm(data) {

    // apply the spectrum colour dialog control
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    if (data.node_type == 'Label') {
        ApplyColourControls(".SpectrumColourChange", true, 'vcPalette', () => RecolourIconBG());
    }

    $ID("txtTooltip").value = data.tooltip == undefined ? "" : data.tooltip;
    spSetColour("txtNodeTooltipfg", data.node_tt_fg || MGLB.node_tt_fg);
    spSetColour("txtNodeTooltipbg", data.node_tt_bg || MGLB.node_tt_bg);
    spSetColour("txtNodeTooltipBorder", data.node_tt_border || MGLB.node_tt_border);

    NoteEditor.setContent(data.label_text == undefined ? "" : data.label_text);
    SetNoteWidth(data.note_width || 600);

    $ID("cbPrivateLabel").checked = data.private_label;

    if (data.node_type == 'Label') {

        $('#rbLabelIcon input:radio:checked').prop("checked", false);
        $('#rbLabelIcon input[value="' + data.label_icon + '"]').prop('checked', true);

        spSetColour("txtIconfg", data.node_fg || MGLB.node_fg);
        spSetColour("txtIconbg", data.node_bg || MGLB.node_bg);
        spSetColour("txtIconBorder", data.node_border_fg || MGLB.node_border_fg);

        $ID('rbIconSquare').checked = data.node_corners == 'Square';
        $ID('rbIconCircle').checked = data.node_corners == 'Circle';

        RecolourIconBG();

    } else {
        if(MGLB.image_position != 'inline') $ID('sliderNodeImageHeight').style.display = "block";
        else $ID('sliderNodeImageHeight').style.display = "none";

        $('#cbNodeShowPhoto').change(function() {
            if (MGLB.image_position != 'inline' && $ID("cbNodeShowPhoto").checked) $ID('sliderNodeImageHeight').style.display = "block";
            else $ID('sliderNodeImageHeight').style.display = "none";
        });
        $ID("cbNodeShowPhoto").checked = data.photoshow;

        if (MGLB.show_photos == true
            && MGLB.photos_applicable == true
            && data.node_type == "Detail")
        {
            elShow("trShowPhoto");
            //$ID("trShowPhoto").style.display = "table-row";
        }
        else {
            elHide("trShowPhoto");
            //$ID("trShowPhoto").style.display = "none";
        }
        $ID('numNodeImageHeight').value = data.image_height;
        $find('bhNodeImageHeight').set_value(data.image_height);

        $ID("txtTeamLine1").value = data.line1;
        $ID("txtTeamLine2").value = data.line2;
        $ID("txtTeamLine3").value = data.line3;
        $ID("txtTeamLine4").value = data.line4;
        $ID("txtTeamLine5").value = data.line5;
        $ID("txtTeamLine6").value = data.line6;

        if (!data.line_attr || data.line_attr == "")
            data.line_attr = deepCopy(MGLB.line_attr);

        // apply hidden class to all of the undo icons
        hideUndos();
        if (data.node_type == "Detail") {
            // if the value is not the same as the ds value then we show the undo icon by remving the hidden class
            if (data.line1 != data.ds_line1) $ID("iUndoLine1").classList.remove("hidden");
            if (data.line2 != data.ds_line2) $ID("iUndoLine2").classList.remove("hidden");
            if (data.line3 != data.ds_line3) $ID("iUndoLine3").classList.remove("hidden");
            if (data.line4 != data.ds_line4) $ID("iUndoLine4").classList.remove("hidden");
            if (data.line5 != data.ds_line5) $ID("iUndoLine5").classList.remove("hidden");
            if (data.line6 != data.ds_line6) $ID("iUndoLine6").classList.remove("hidden");
        }

        for (var i = 1; i < 7; i++) {
            // set the font/size/colour etc attributes for the line
            var a = GetSingleLineAttrs(data.line_attr, i);
            nodeAttrs[i].setValue(JSON.stringify(a));
        }

        spSetColour("txtNodeBoxfg", data.node_fg ?? MGLB.node_fg);
        spSetColour("txtNodeBoxbg", data.node_bg ?? MGLB.node_bg);
        spSetColour("txtNodeBorderfg", data.node_border_fg ?? MGLB.node_border_fg);

        spSetColour("txtNodeTextBg", data.node_text_bg ?? MGLB.node_text_bg);
        $ID("cbNodeTextBgBlock").checked = data.node_text_bg_block ?? MGLB.node_text_bg_block;

        spSetColour("txtNodeIconfg", data.node_icon_fg ?? MGLB.node_icon_fg);
        spSetColour("txtNodeIconHover", data.node_icon_hover ?? MGLB.node_icon_hover);

        var checkbox = $ID("cbNodeTextBgBlock");
        checkbox.checked = data.node_text_bg_block ?? MGLB.node_text_bg_block;
        checkbox.removeEventListener('change', blockCheckboxChangeHandler);
        if (data.node_type == "ParentGroup")
            checkbox.addEventListener('change', blockCheckboxChangeHandler);

        $ID('numNodeBoxHeight').value = data.node_height;
        $find('bhNodeBoxHeight').set_value(data.node_height);

        $ID('numNodeBoxWidth').value = data.node_width;
        $find('bhNodeBoxWidth').set_value(data.node_width);

        $ID('numNodesAcross').value = data.nodes_across;
        $find('bhNodesAcross').set_value(data.nodes_across);

        $ID('rbNodeCornerRectangle').checked = data.node_corners == 'Rectangle';
        $ID('rbNodeCornerRoundedRectangle').checked = data.node_corners == 'RoundedRectangle';

        $ID('cbNodeShowShadow').checked = data.showShadow ?? MGLB.showShadow;
        spSetColour("txtNodeShadowColour", data.shadowColour ?? MGLB.shadowColour);
    }

    // spectrum colour controls have to be in place before we try to show / hide any elements
    NodeSettingsFormConfiguration(data.node_type, data);
    ShowNodeApply();

    // set the "apply" options
    
    RestrictApplyOptions(data);
    ChangeApplyOptions(data);

    $ID('cbApplySizes').checked = true;

    elHide("ddlbApplyTypes");
    elHide($ID('cbApplySizes').parentElement);  // have to hide the surrounding span, not the actual checkbox
    if (data.node_type == 'ParentGroup')
    {
        $ID("ddlbApplyOptions").onchange = null;
    } else {
        $ID("ddlbApplyOptions").onchange = function ()
        {
            ChangeApplyOptions(data);
        };
    }

    // set the show shadow colour picker based on the checkbox.. has to be after initialising the colour controls
    toggleColorPicker($('#cbNodeShowShadow'));

    // Display the dialog
    $find('mpeNodeSettingsForm').show();

    // now that it is visible, we can set focus when we need to
    if (data.node_type == "ParentGroup")
    setTimeout(() => {
        $ID('txtTeamLine1').focus();
    }, 0);

    return false;
}
function LinkSettingsForm(linkLine) {
    let data = linkLine.toNode.data;

    // apply the spectrum colour dialog control
    ApplyColourControls();

    spSetColour("txtLinkColour", data.linkColour);
    spSetColour("txtLinkHover", data.linkHover);
    $ID('numLinkWidth').value = data.linkWidth;
    $find('bhLinkWidth').set_value(data.linkWidth);
    setDropDownListSelectedValue($ID("ddlbLinkStyle"), data.linkType);
    setDropDownListSelectedValue($ID("ddlbApplyLinkOptions"), "currentLine");

    $ID('txtLinkTooltip').value = data.linkTooltip;
    spSetColour('txtLinkTooltipFg', data.linkTooltipForeground);
    spSetColour('txtLinkTooltipBg', data.linkTooltipBackground);
    spSetColour('txtLinkTooltipBorder', data.linkTooltipBorder);

    if (MGLB.can_override == false) {
        $("#trLinkColour").addClass("hidden");
        $("#trLinkWidth").addClass("hidden");
        $("#trTooltipColours").addClass("hidden");
        $("#trApplyLinkOptions").addClass("hidden");
    }

    // Display the dialog
    $find('mpeLinkSettingsForm').show();

    return false;
}
function ApplyNewNode(event) {
    event.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnApplyNewNode").hasAttribute("disabled")) return;

    if (typenode == "Team")
        if (!ApplyNewTeamNode()) return;

    if (typenode == "Label")
        if (!ApplyNewLabelNode()) return;
    
    ModelChanged(true);
    typenode = "NA";

    myDiagram.layoutDiagram(true);

    $find('mpeNodeSettingsForm').hide();
    return false;
}
function ApplyNewTeamNode() {

    if ($ID("txtTeamLine1").value.trim() == "" &&
        $ID("txtTeamLine2").value.trim() == "" &&
        $ID("txtTeamLine3").value.trim() == "" &&
        $ID("txtTeamLine4").value.trim() == "" &&
        $ID("txtTeamLine5").value.trim() == "" &&
        $ID("txtTeamLine6").value.trim() == "") {
        ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');
        setTimeout(() => {
            $ID('txtTeamLine1').focus();
        }, 0);
        return false;
    }

    myDiagram.startTransaction("add group node");

    // set the line attributes data
    var line = "";
    var TeamAttr = deepCopy(MGLB.line_attr);
    for (var i = 1; i < 7; i++) {
        SetSingleLineAttrs(TeamAttr, nodeAttrs[i].getValue(), i);
    }
    // get the breakdown for font, colour, bg_colour, underlined and alignment
    var attr = GetLineAttrs(TeamAttr);

    var ParentKey = 0;
    var NextSeq = 0;
    if (JSON.parse(myDiagram.model.toJson()).nodeDataArray.length != 0) {
        var ParentNode = myDiagram.selection.first();
        ParentKey = ParentNode.data.key;
        NextSeq = ResequenceChildren(ParentNode);
        SetProperty(ParentNode, "isTreeExpanded", true);
    }

    var newdata = {
        parent: ParentKey,
        sequence: NextSeq,
        name: $ID("txtTeamLine1").value,
        source: "",
        item_ref: "",
        line1: $ID("txtTeamLine1").value,
        line2: $ID("txtTeamLine2").value,
        line3: $ID("txtTeamLine3").value,
        line4: $ID("txtTeamLine4").value,
        line5: $ID("txtTeamLine5").value,
        line6: $ID("txtTeamLine6").value,

        line_attr: TeamAttr,

        font1: attr.font[1],
        font2: attr.font[2],
        font3: attr.font[3],
        font4: attr.font[4],
        font5: attr.font[5],
        font6: attr.font[6],

        isUnderline1: attr.isUnderline[1],
        isUnderline2: attr.isUnderline[2],
        isUnderline3: attr.isUnderline[3],
        isUnderline4: attr.isUnderline[4],
        isUnderline5: attr.isUnderline[5],
        isUnderline6: attr.isUnderline[6],

        fontColour1: attr.colour[1],
        fontColour2: attr.colour[2],
        fontColour3: attr.colour[3],
        fontColour4: attr.colour[4],
        fontColour5: attr.colour[5],
        fontColour6: attr.colour[6],

        fontBgColour1: attr.bg_colour[1],
        fontBgColour2: attr.bg_colour[2],
        fontBgColour3: attr.bg_colour[3],
        fontBgColour4: attr.bg_colour[4],
        fontBgColour5: attr.bg_colour[5],
        fontBgColour6: attr.bg_colour[6],

        alignment1: attr.align[1],
        alignment2: attr.align[2],
        alignment3: attr.align[3],
        alignment4: attr.align[4],
        alignment5: attr.align[5],
        alignment6: attr.align[6],

        node_bg: spGetColour("txtNodeBoxbg") || MGLB.node_bg,
        node_fg: spGetColour("txtNodeBoxfg") || MGLB.node_fg,
        node_border_fg: spGetColour("txtNodeBorderfg") || MGLB.node_border_fg,
        node_text_bg: spGetColour("txtNodeTextBg") || MGLB.node_text_bg,
        node_text_bg_block: $ID("cbNodeTextBgBlock").checked,
        node_icon_fg: spGetColour("txtNodeIconfg") || MGLB.node_icon_fg,
        node_icon_hover: spGetColour("txtNodeIconHover") || MGLB.node_icon_hover,

        node_height: parseInt($ID('numNodeBoxHeight').value),
        node_width: parseInt($ID('numNodeBoxWidth').value),
        node_corners: $ID('rbNodeCornerRectangle').checked == true ? 'Rectangle' : 'RoundedRectangle',
        show_photos: false,
        photoshow: false,
        visible: false,
        sort_name: "",
        existnode: "0",
        dragtype: "oca",
        vacent: true,
        isTreeExpanded: true,
        isAssistant: false,
        isCoParent: false,
        node_type: typenode,
        tooltip: $ID("txtTooltip").value,      // TooltipEditor.getData(),
        node_tt_fg: spGetColour("txtNodeTooltipfg"),
        node_tt_bg: spGetColour("txtNodeTooltipbg"),
        node_tt_border: spGetColour("txtNodeTooltipBorder"),
        showShadow: $ID('cbNodeShowShadow').checked,
        shadowColour: spGetColour("txtNodeShadowColour"),

        show_detail: false,
        label_icon: "",
        label_text: NoteEditor.getContent(),
        note_width: GetNoteWidth(),
        private_label: $ID("cbPrivateLabel").checked,

        alignment: go.Spot.Center,
        margin: new go.Margin(0, 5, 0, 5),
        info: "i",
        isNote: NoteEditor.getContent() == "" ? false : true,
        picture_width: 0,
        node_table_width: parseInt($ID('numNodeBoxWidth').value),
        linkColour: MGLB.linkColour,
        linkHover: MGLB.linkHover,
        linkWidth: MGLB.linkWidth,
        linkType: MGLB.linkType,
        linkTooltip: $ID('txtLinkTooltip').value,
        linkTooltipForeground: spGetColour('txtLinkTooltipFg'),
        linkTooltipBackground: spGetColour('txtLinkTooltipBg'),
        linkTooltipBorder: spGetColour('txtLinkTooltipBorder')
    }
    myDiagram.model.addNodeData(newdata);
    var newNodeData = myDiagram.findNodeForData(newdata);

    if (ParentNode)
        addModelLink(ParentNode.data.key, newNodeData.data.key);

    myDiagram.commitTransaction("add group node");

    return true;
}
function ApplyNewLabelNode() {

    if (NoteEditor.getContent().trim() == "") {
        ATC_setActiveTab('nodeTabContainer', 'tpNodeNote');        
        return false;
    }

    // create the new label (text) node here
    // will need the x/y coordinates from the context menu click
    myDiagram.startTransaction("add label node");
    let labelNode = null;
    myDiagram.nodes.each(function(node) {
        if (node.data.node_type === "Label") {
          labelNode = node;
          return;
        }
      });
    var AddNewData = {
        name: "",
        sort_name: "pop",
        vacant: false,
        parent: "0",
        line1: "",
        line2: "",
        line3: "",
        line4: "",
        line5: "",
        line6: "",
        item_ref: "",
        isAssistant: false,
        isCoParent: false,
        success: "true",
        node_fg: spGetColour("txtIconfg") || MGLB.node_fg,
        node_bg: spGetColour("txtIconbg") || MGLB.node_bg,
        node_border_fg: spGetColour("txtIconBorder") || MGLB.node_border_fg,
        node_corners: $ID("rbIconCircle").checked ? "Circle" : "Square",
        visible: false,
        node_width: labelNode == null ? 50 : labelNode.data.node_width,
        node_height: labelNode == null ? 50 : labelNode.data.node_height,
        photoshow: false,
        isTreeExpanded: false,
        node_type: "Label",
        node_tt_fg: spGetColour("txtNodeTooltipfg"),
        node_tt_bg: spGetColour("txtNodeTooltipbg"),
        node_tt_border: spGetColour("txtNodeTooltipBorder"),
        tooltip: $ID("txtTooltip").value,   // TooltipEditor.getData(),
        label_text: NoteEditor.getContent(),
        label_icon: $('#rbLabelIcon').find(":checked").val(),
        note_width: GetNoteWidth(),
        private_label: $ID("cbPrivateLabel").checked,
        individualphotoshow: false,
        existnode: 1,
        alignment: go.Spot.Left,
        margin: new go.Margin(0, 0, 0, 8),
        category: "Label",
        sequence: 0,
        line_attr: "",
        linkTooltip: "",
        linkTooltipBackground: "",
        linkTooltipBorder: "",
        linkTooltipForeground: "",
        linkType: "",
        linkWidth: 1
    }

    //   const newnode = myDiagram.findNodeForData(AddNewData);
    myDiagram.model.addNodeData(AddNewData);
    myDiagram.commitTransaction("add label node");
    return true;
}
function ApplyNodeDefaults(evt) {
    evt.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnApplyDefaults").hasAttribute("disabled")) return;

    // apply the spectrum colour dialog control
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    var NodeType;
    if (typenode == "NA")
        NodeType = myDiagram.selection.first().data.node_type;
    else
        NodeType = typenode;

    if (NodeType == "Label") {
        spSetColour("txtIconfg", MGLB.node_fg);
        spSetColour("txtIconbg", MGLB.node_bg);
        spSetColour("txtIconBorder", MGLB.node_border_fg);

        $ID('rbIconCircle').checked = true;
        $ID('rbIconSquare').checked = false;

        RecolourIconBG();
    } else {
        spSetColour("txtNodeBoxfg", MGLB.node_fg);
        spSetColour("txtNodeBoxbg", MGLB.node_bg);
        spSetColour("txtNodeBorderfg", MGLB.node_border_fg);
        spSetColour("txtNodeTextBg", MGLB.node_text_bg);
        $ID("cbNodeTextBgBlock").checked = MGLB.node_text_bg_block;
        spSetColour("txtNodeIconfg", MGLB.node_icon_fg);
        spSetColour("txtNodeIconHover", MGLB.node_icon_hover);

        $ID('numNodeBoxHeight').value = MGLB.node_height;
        $ID('numNodeBoxWidth').value = MGLB.node_width;
        $find('bhNodeBoxHeight').set_value(MGLB.node_height);
        $find('bhNodeBoxWidth').set_value(MGLB.node_width);
        $ID('rbNodeCornerRectangle').checked = MGLB.node_corners == 'Rectangle';
        $ID('rbNodeCornerRoundedRectangle').checked = MGLB.node_corners == 'RoundedRectangle';

        for (var i = 1; i < 7; i++) {
            // set the font/size/colour etc attributes for the line
            nodeAttrs[i].setValue(JSON.stringify(GetSingleLineAttrs(MGLB.line_attr, i)));
        }
    }

    spSetColour("txtNodeTooltipfg", MGLB.node_tt_fg);
    spSetColour("txtNodeTooltipbg", MGLB.node_tt_bg);
    spSetColour("txtNodeTooltipBorder", MGLB.node_tt_border);

    return false;
}
function ApplyLinkSettings(event) {
    event.preventDefault();

    myDiagram.startTransaction("SaveLinkSettings");

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnApplyLinkSettings").hasAttribute("disabled")) return;

    var nodes = [];
    var currentLink = myDiagram.selection.first();
    var ApplyTo = $ID("ddlbApplyLinkOptions").value;

    var linkType = $ID('ddlbLinkStyle').value;
    var linkColour = spGetColour("txtLinkColour");
    var linkHover = spGetColour("txtLinkHover");
    var linkWidth = parseFloat($ID('numLinkWidth').value);
    var linkTooltip = $ID('txtLinkTooltip').value;
    var linkTooltipForeground = spGetColour('txtLinkTooltipFg');
    var linkTooltipBackground = spGetColour('txtLinkTooltipBg');
    var linkTooltipBorder = spGetColour('txtLinkTooltipBorder');

    switch (ApplyTo) {
        case 'currentLine':
            UpdateLink(currentLink, linkColour, linkHover, linkWidth, linkType, linkTooltip, linkTooltipForeground, linkTooltipBackground, linkTooltipBorder)
            break;
        case 'allLine':
            myDiagram.links.each(function (link) {
                UpdateLink(link, linkColour, linkHover, linkWidth, linkType, link.toNode.data.linkTooltip, linkTooltipForeground, linkTooltipBackground, linkTooltipBorder)
            });
            break;
    }
    myDiagram.commitTransaction('SaveLinkSettings');

    $find('mpeLinkSettingsForm').hide();

    ModelChanged(true);

    myDiagram.clearSelection();

    return false;
}
function UpdateLink(link, linkColour, linkHover, linkWidth, linkType, linkTT, linkTTFore, linkTTBack, linkTTBorder) {
    SetProperty(link.toNode.data, "linkColour", linkColour);
    SetProperty(link.toNode.data, "linkHover", linkHover);
    SetProperty(link.toNode.data, "linkWidth", linkWidth);
    SetProperty(link.toNode.data, "linkType", linkType);
    SetProperty(link.toNode.data, "linkTooltip", linkTT);
    SetProperty(link.toNode.data, "linkTooltipForeground", linkTTFore);
    SetProperty(link.toNode.data, "linkTooltipBackground", linkTTBack);
    SetProperty(link.toNode.data, "linkTooltipBorder", linkTTBorder);

    var linkShape = link.findObject("OBJSHAPE");
    linkShape.stroke = linkColour;
    linkShape.strokeWidth = linkWidth;
    linkShape.strokeDashArray = GetLinkLineType(link.toNode.data);
}
function ApplyNodeSettings(event) {
    event.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnApplyNodeSettings").hasAttribute("disabled")) return;

    var ApplyTo = $ID("ddlbApplyOptions").value;
    var nodes = [];
    var currentNode = myDiagram.selection.first();

    // for team and label nodes, ensure there is something to display
    if (currentNode.data.node_type == "Label") {
        if (NoteEditor.getContent().trim() == "") {
            ATC_setActiveTab('nodeTabContainer', 'tpNodeNote');
            return;
        }
    }
    if (currentNode.data.node_type == "Team") {
        if ($ID("txtTeamLine1").value.trim() == "" &&
            $ID("txtTeamLine2").value.trim() == "" &&
            $ID("txtTeamLine3").value.trim() == "" &&
            $ID("txtTeamLine4").value.trim() == "" &&
            $ID("txtTeamLine5").value.trim() == "" &&
            $ID("txtTeamLine6").value.trim() == "") {
            ATC_setActiveTab('nodeTabContainer', 'tpNodeTeam');
            setTimeout(() => {
                $ID('txtTeamLine1').focus();
            }, 0);
            return false;
        }
    }

    switch (ApplyTo) {
        case 'Apply':
            nodes = [currentNode];
            break;
        case 'ApplyBelow':
            nodes = currentNode.findTreeParts().filter(function (node) {
                return node instanceof go.Node;
            });
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                var childrenList = ParentNode.findTreeChildrenNodes();
                if (childrenList.count > 0) {
                    var it = childrenList.iterator;
                    while (it.next()) {
                        var g = it.value;
                        if (g instanceof go.Group) {
                            g.memberParts.filter(function (p) {
                                if (p instanceof go.Node) {
                                    nodes.add(p);
                                }
                            });
                        }

                    }
                }
            }
            break;
        case 'Siblings':
            var currentParent = currentNode.findTreeParentNode();
            if (currentParent) {
                nodes = currentParent.findTreeChildrenNodes();
            } else {
                nodes = [currentNode];
            }
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                ParentNode.memberParts.each(function (m) {
                    if (m instanceof go.Node) {
                        nodes.push(m);
                    }
                });
                var currentParent2 = ParentNode.findTreeParentNode();
                if (currentParent2) {
                    var childrenList = currentParent2.findTreeChildrenNodes();
                    if (childrenList.count > 0) {
                        var it = childrenList.iterator;
                        while (it.next()) {
                            var g = it.value;
                            if (g instanceof go.Group) {
                                g.memberParts.each(function (p) {
                                    if (p instanceof go.Node) {
                                        nodes.push(p);
                                    }
                                });
                            }
                        }
                    }
                }
            }
            break;
        case 'SiblingsBelow':
            var currentParent = currentNode.findTreeParentNode();
            var tmpNodes = [];
            if (currentParent) {
                tmpNodes = currentParent.findTreeChildrenNodes();
            } else {
                tmpNodes = [currentNode];
            }
            tmpNodes.map(function (node) {
                node.findTreeParts().map(function (node) {
                    if (node instanceof go.Node) {
                        nodes.push(node);
                    }
                });
            });

            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                ParentNode.memberParts.each(function (m) {
                    if (m instanceof go.Node) {
                        nodes.push(m);
                    }
                });
                var currentParent2 = ParentNode.findTreeParentNode();
                if (currentParent2) {
                    var childrenList = currentParent2.findTreeChildrenNodes();
                    if (childrenList.count > 0) {
                        var it = childrenList.iterator;
                        while (it.next()) {
                            var g = it.value;
                            if (g instanceof go.Group) {
                                g.memberParts.each(function (p) {
                                    if (p instanceof go.Node) {
                                        nodes.push(p);
                                    }
                                });
                                var childList = g.findTreeChildrenNodes();
                                if (childList.count > 0) {
                                    var it2 = childList.iterator;
                                    while (it2.next()) {
                                        var gr = it2.value;
                                        if (gr instanceof go.Group) {
                                            gr.memberParts.each(function (n) {
                                                if (n instanceof go.Node) {
                                                    nodes.push(n);
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            break;
        case 'Level':
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                var currentLevel = ParentNode.findTreeLevel();
                myDiagram.nodes.each(function (node) {
                    if (node instanceof go.Group && node.findTreeLevel() === currentLevel) {
                        node.memberParts.each(function (n) {
                            if (n instanceof go.Node) {
                                nodes.push(n);
                            }
                        });
                    }
                });
            } else {
                var currentLevel = currentNode.findTreeLevel();
                nodes = myDiagram.nodes.filter(function (node) {
                    return node.findTreeLevel() === currentLevel;
                });
            }
            break;
        case 'LevelBelow':
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                var currentLevel = ParentNode.findTreeLevel();
                myDiagram.nodes.each(function (node) {
                    if (node.findTreeLevel() === currentLevel) {
                        if (node instanceof go.Group) {
                            node.memberParts.each(function (n) {
                                if (n instanceof go.Node) {
                                    nodes.push(n);
                                }
                            });
                        }
                        var childList = node.findTreeChildrenNodes();
                        if (childList.count > 0) {
                            var it2 = childList.iterator;
                            while (it2.next()) {
                                var gr = it2.value;
                                if (gr instanceof go.Group) {
                                    gr.memberParts.each(function (n) {
                                        if (n instanceof go.Node) {
                                            nodes.push(n);
                                        }
                                    });
                                }
                            }
                        }
                    }

                });
            } else {
                var currentLevel = currentNode.findTreeLevel();
                var tmpNodes = myDiagram.nodes.filter(function (node) {
                    return node.findTreeLevel() === currentLevel;
                });
                tmpNodes.map(function (node) {
                    node.findTreeParts().map(function (node) {
                        if (node instanceof go.Node) {
                            nodes.push(node);
                        }
                    });
                });
            }
            break;
        case 'All':
            nodes = myDiagram.nodes.filter(function (node) {
                return node instanceof go.Node;
            });
            break;
        case 'CoParents':
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                ParentNode.memberParts.each(function (m) {
                    if (m instanceof go.Node) {
                        nodes.push(m);
                    }
                });
            }
            break;
        case 'CoParentsBelow':
            if (currentNode.data.isCoParent == true) {
                var ParentNode = myDiagram.findNodeForKey(currentNode.data.parent);
                ParentNode.memberParts.each(function (m) {
                    if (m instanceof go.Node) {
                        nodes.push(m);
                    }
                });
                var childList = ParentNode.findTreeChildrenNodes();
                if (childList.count > 0) {
                    var it2 = childList.iterator;
                    while (it2.next()) {
                        var node = it2.value;
                        if (node instanceof go.Group) {
                            node.memberParts.each(function (n) {
                                if (n instanceof go.Node) {
                                    nodes.push(n);
                                }
                            });
                        }
                        addTreePartsMembers(node, nodes);
                    }
                }
            }
            break;
    }

    // remove all of the nodes that don't match the type, assistant flags or isCoParent
    var DoAllNodes = false;
    var ApplySizes = false;
    var ApplyImageSizes = false;

    switch (currentNode.data.node_type)
    {
        case "ParentGroup":
            break;
        case "Team":
        case "Vacant":
            DoAllNodes = $ID('ddlbApplyTypes').value == 'alltypes';
            ApplySizes = $ID('cbApplySizes').checked;
        case "Detail":
            DoAllNodes = $ID('ddlbApplyTypes').value == 'alltypes';
            ApplySizes = $ID('cbApplySizes').checked;
            if (MGLB.image_position != 'inline')
            {
                ApplyImageSizes = $ID('cbApplySizes').checked;
            }
    }

    if (!DoAllNodes)
    {
        nodes = nodes.filter(function (node)
        {
            return node.data.node_type === currentNode.data.node_type &&
                node.data.isAssistant === currentNode.data.isAssistant &&
                node.data.isCoParent === currentNode.data.isCoParent
        });
    }

    // update the line_attr with any changes                
    var LineAttr = deepCopy(currentNode.data.line_attr);
    if (currentNode.data.node_type != "Label") {
        var line = "";
        if (LineAttr == null || LineAttr == "")
            LineAttr = deepCopy(MGLB.line_attr);
        for (var i = 1; i < 7; i++) {
            line = nodeAttrs[i].getValue();
            SetSingleLineAttrs(LineAttr, line, i);
        }
        var attr = GetLineAttrs(LineAttr);
    }

    myDiagram.startTransaction("SaveNodeSettings");

    nodes.map(function (node) {

        if (node !== null) {
            var thisNode = node.data;

            // Note, if we are currently editing a label node, then that will be the only node we're updating here
            // as it will always be restricted to Apply

            if (currentNode.data.key == thisNode.key) {
                // this only applies if we're dealing with the current selected node

                SetProperty(thisNode, "tooltip", $ID("txtTooltip").value);
                SetProperty(thisNode, "label_text", NoteEditor.getContent());
                SetProperty(thisNode, "note_width", GetNoteWidth());

                SetProperty(thisNode, "private_label", $ID("cbPrivateLabel").checked);

                if (thisNode.node_type == "Label") {
                    // attributes that only apply to a label type node
                    SetProperty(thisNode, "label_icon", $('#rbLabelIcon').find(":checked").val());

                    SetProperty(thisNode, "node_fg", spGetColour('txtIconfg'));
                    SetProperty(thisNode, "node_bg", spGetColour('txtIconbg'));
                    SetProperty(thisNode, "node_border_fg", spGetColour('txtIconBorder'));

                    SetProperty(thisNode, "node_corners", $ID('rbIconCircle').checked ? 'Circle' : 'Square');

                } else
                {
                    // common settings for non-label type

                    SetProperty(thisNode, "line1", $ID("txtTeamLine1").value);
                    SetProperty(thisNode, "line2", $ID("txtTeamLine2").value);
                    SetProperty(thisNode, "line3", $ID("txtTeamLine3").value);
                    SetProperty(thisNode, "line4", $ID("txtTeamLine4").value);
                    SetProperty(thisNode, "line5", $ID("txtTeamLine5").value);
                    SetProperty(thisNode, "line6", $ID("txtTeamLine6").value);

                    SetProperty(thisNode, "isNote", NoteEditor.getContent() == "" ? false : true);
                }
            }

            if (thisNode.node_type != "Label") {

                SetProperty(thisNode, "line_attr", LineAttr);

                for (let i = 1; i <= 6; i++) {
                    SetProperty(thisNode, `font${i}`, attr.font[i]);
                    SetProperty(thisNode, `isUnderline${i}`, attr.isUnderline[i]);
                    SetProperty(thisNode, `fontColour${i}`, attr.colour[i]);
                    SetProperty(thisNode, `fontBgColour${i}`, attr.bg_colour[i]);
                    SetProperty(thisNode, `alignment${i}`, attr.align[i]);
                }

                SetProperty(thisNode, "node_bg", spGetColour('txtNodeBoxbg'));
                SetProperty(thisNode, "node_fg", spGetColour('txtNodeBoxfg'));
                SetProperty(thisNode, "node_border_fg", spGetColour('txtNodeBorderfg'));
                SetProperty(thisNode, "node_text_bg", spGetColour("txtNodeTextBg"));
                SetProperty(thisNode, "node_text_bg_block", $ID("cbNodeTextBgBlock").checked);
                SetProperty(thisNode, "node_icon_fg", spGetColour('txtNodeIconfg'));
                SetProperty(thisNode, "node_icon_hover", spGetColour('txtNodeIconHover'));

                if (ApplySizes || currentNode.data.key == thisNode.key)
                {
                    SetProperty(thisNode, "node_width", parseInt($ID('numNodeBoxWidth').value));
                    SetProperty(thisNode, "node_height", parseInt($ID('numNodeBoxHeight').value));
                }
                SetProperty(thisNode, "node_corners", $ID('rbNodeCornerRectangle').checked == true ? 'Rectangle' : 'RoundedRectangle');

                SetProperty(thisNode, "nodes_across", parseInt($ID('numNodesAcross').value));

                SetProperty(thisNode, "showShadow", $ID('cbNodeShowShadow').checked);
                SetProperty(thisNode, "shadowColour", $ID('txtNodeShadowColour').value);
               
                //SetProperty(thisNode, "photoshow", $ID('cbNodeShowPhoto').checked);
                //SetProperty(thisNode, "individualphotoshow", $ID('cbNodeShowPhoto').checked);

                if (ApplyImageSizes || currentNode.data.key == thisNode.key)
                {
                    SetProperty(thisNode, "image_height", parseInt($ID('numNodeImageHeight').value));
                }

                var getimage = GetNodePicture(thisNode.item_ref, parseInt(thisNode.node_height));

                SetProperty(thisNode, "picture_width", getimage.picture_width);
                SetProperty(thisNode, "node_table_width",
                    CalcNodeTableWidth(thisNode.node_type,
                        $ID('cbNodeShowPhoto').checked,
                        parseInt($ID('numNodeBoxWidth').value),
                        getimage.picture_width));

                setTimeout(() => {
                    SetProperty(thisNode, "source", getimage.node_picture);
                }, 100);
            }

            SetProperty(thisNode, "node_tt_bg", spGetColour('txtNodeTooltipbg'));
            SetProperty(thisNode, "node_tt_fg", spGetColour('txtNodeTooltipfg'));
            SetProperty(thisNode, "node_tt_border", spGetColour('txtNodeTooltipBorder'));
        }
    })
    myDiagram.commitTransaction('SaveNodeSettings');

    $find('mpeNodeSettingsForm').hide();

    ModelChanged(true);

    return false;
}
function addTreePartsMembers(node, nodes) {
    node.findTreeParts().map(function (n) {
        if (n instanceof go.Group) {
            n.memberParts.each(function (n1) {
                if (n1 instanceof go.Node) {
                    nodes.push(n1);
                }
            });
        }
    });
}
function CancelNodeSettings(event) {
    // Cancel button on the node settings form
    event.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnCancelNodeSettings").hasAttribute("disabled")) return;

    $find('mpeNodeSettingsForm').hide();
    typenode = "NA";
    return false;
}
function CancelLinkSettings(event) {
    // Cancel button on the node settings form
    event.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnCancelLinkSettings").hasAttribute("disabled")) return;

    $find('mpeLinkSettingsForm').hide();
    typenode = "NA";
    return false;
}
function ShowNodeApply() {
    if (typenode == "NA") {
        // editing an existing item
        $ID("btnApplyNodeSettings").style.display = "inline-block";
        $ID("btnApplyNewNode").style.display = "none";
    }
    else {
        // adding a new label or team node
        $ID("btnApplyNewNode").style.display = "inline-block";
        $ID("btnApplyNodeSettings").style.display = "none";
    }
}
function RecolourIconBG() {
    $ID("rbLabelIcon").style.color = $ID("txtIconfg").value
    $ID("rbLabelIcon").style.backgroundColor = $ID("txtIconbg").value
}
function ResetLineToDs(evt, line) {

    var node = myDiagram.selection.first();
    switch (line) {
        case 1:
            $ID("txtTeamLine1").value = node.data.ds_line1;
            $ID("iUndoLine1").classList.add("hidden");
            break;
        case 2:
            $ID("txtTeamLine2").value = node.data.ds_line2;
            $ID("iUndoLine2").classList.add("hidden");
            break;
        case 3:
            $ID("txtTeamLine3").value = node.data.ds_line3;
            $ID("iUndoLine3").classList.add("hidden");
            break;
        case 4:
            $ID("txtTeamLine4").value = node.data.ds_line4;
            $ID("iUndoLine4").classList.add("hidden");
            break;
        case 5:
            $ID("txtTeamLine5").value = node.data.ds_line5;
            $ID("iUndoLine5").classList.add("hidden");
            break;
        case 6:
            $ID("txtTeamLine6").value = node.data.ds_line6;
            $ID("iUndoLine6").classList.add("hidden");
            break;
    }
}
function hideUndos() {
    for (var i = 1; i < 7; i++) {
        $ID("iUndoLine" + i).classList.add("hidden");
    }
}
function blockCheckboxChangeHandler(event) {
    var checked = event.target.checked;
    nodeAttrs[1].setControlVisibility("left", checked);
    nodeAttrs[1].setControlVisibility("center", checked);
    nodeAttrs[1].setControlVisibility("right", checked);
}

//----------------------------------------------------------------------------------------------------------------------
// Model Settings Dialog
//----------------------------------------------------------------------------------------------------------------------

var PreviousNodeHeight,
    PreviousNodeWidth,
    PreviousNodeBackColour,
    PreviousNodeTextColour,
    PreviousNodeBorderColour,
    PreviousNodeTooltipBackColour,
    PreviousNodeTooltipTextColour,
    PreviousNodeTooltipBorderColour,
    PreviousNodeCorners,
    PreviousLineAttr,
    PreviousImagePosition;

function addModelEvents() {
    $ID("btnApplyModelSettings").addEventListener("click", (event) => { ApplyModelSettings(event); });
    $ID("btnCancelModelSettings").addEventListener("click", (event) => { CancelModelSettings(event); });
    $ID("btnCancelItemDetail").addEventListener("click", (event) => { CancelItemDetail(event); });
}
function ModelSettingsForm() {
    // apply the spectrum colour dialog control
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    if (MGLB.photos_applicable == true)
        $ID("trModelShowPhoto").style.display = "table-row";
    else
        $ID("trModelShowPhoto").style.display = "none";
    
    $ID('ddlbChartBackgroundType').value = MGLB.backgroundType;
    spSetColour('txtChartBackgroundColour', MGLB.backgroundContent);

    switch (MGLB.backgroundType) {
        case 'Gradient':
            setDropDownListSelectedValue($ID("ddlbChartBackgroundGradient"), MGLB.backgroundID);
            break;
        case 'Image':
            setDropDownListSelectedValue($ID("ddlbChartBackgroundImage"), MGLB.backgroundID);
            break;
    }
    ChangedBackgroundType();

    $ID('numModelBoxHeight').value = MGLB.node_height;
    $ID('numModelBoxWidth').value = MGLB.node_width;
    $find('bhModelBoxHeight').set_value(MGLB.node_height);
    $find('bhModelBoxWidth').set_value(MGLB.node_width);

    $ID('rbModelCornerRectangle').checked = MGLB.node_corners == 'Rectangle';
    $ID('rbModelCornerRoundedRectangle').checked = MGLB.node_corners == 'RoundedRectangle';

    $ID('cbShowPhotos').checked = MGLB.show_photos;
    setDropDownListSelectedValue($ID('ddlbImagePosition'), MGLB.image_position);
    setDropDownListSelectedValue($ID('ddlbImageShape'), MGLB.image_shape)

    // Show photo shows or hides the image position/shape dropdowns
    $ID('cbShowPhotos').addEventListener('change', function ()
    {
        $ID('trModelImagePosition').style.display = $ID('cbShowPhotos').checked ? 'table-row' : 'none';
        $ID('trModelImageShape').style.display = $ID('cbShowPhotos').checked ? 'table-row' : 'none';
    });
    $ID('cbShowPhotos').dispatchEvent(new Event('change'));

    // image position will show/hide the image shape as it is only relevant when image above
    $ID('ddlbImagePosition').addEventListener('change', function ()
    {
        $ID('trModelImageShape').style.display = $ID('cbShowPhotos').checked == false || $ID('ddlbImagePosition').value == 'inline' ? 'none' : 'table-row';
    });
    $ID('ddlbImagePosition').dispatchEvent(new Event('change'));

    $ID('numButtonImageHeight').value = MGLB.image_height;
    $find('bhButtonImageHeight').set_value(MGLB.image_height);

    // set the line attributes data
    for (var i = 1; i <= 6; i++) {
        modelAttrs[i].setValue(JSON.stringify(MGLB.line_attr["line" + i]));
    }

    spSetColour('txtNodebg', MGLB.node_bg);
    spSetColour('txtNodefg', MGLB.node_fg);
    spSetColour('txtBorderfg', MGLB.node_border_fg);
    spSetColour('txtNodeTxtBg', MGLB.node_text_bg);
    $ID('cbTextBgBlock').checked = MGLB.node_text_bg_block;
    spSetColour('txtModelIconfg', MGLB.node_icon_fg);
    spSetColour('txtModelIconHover', MGLB.node_icon_hover);
    spSetColour("txtTTfg", MGLB.node_tt_fg);
    spSetColour("txtTTbg", MGLB.node_tt_bg);
    spSetColour("txtTTBorder", MGLB.node_tt_border);
    spSetColour("txtHighlightfg", MGLB.node_highlight_fg);
    spSetColour("txtHighlightbg", MGLB.node_highlight_bg);
    $ID('cbModelShowShadow').checked = MGLB.showShadow;
    spSetColour('txtModelShadowColour', MGLB.shadowColour);
    spSetColour('txtModelLinkColour', MGLB.linkColour);
    spSetColour('txtModelLinkHover', MGLB.linkHover);
    $ID('numModelLinkWidth').value = MGLB.linkWidth;
    $find('bhModelLinkWidth').set_value(MGLB.linkWidth);
    setDropDownListSelectedValue($ID("ddlbModelLinkStyle"), MGLB.linkType);

    spSetColour('txtModelLinkTooltipFg', MGLB.linkTooltipForeground);
    spSetColour('txtModelLinkTooltipBg', MGLB.linkTooltipBackground);
    spSetColour('txtModelLinkTooltipBorder', MGLB.linkTooltipBorder);

    rbButtonPositionLeft.checked = MGLB.button_position == "left"
    rbButtonPositionBelow.checked = MGLB.button_position == "bottomleft"
    rbButtonFontSmall.checked = MGLB.button_font == '8pt Arial';
    rbButtonFontMedium.checked = MGLB.button_font == '10pt Arial';
    rbButtonShapeSquare.checked = MGLB.button_shape == 'Rectangle';
    rbButtonShapeRounded.checked = MGLB.button_shape == 'RoundedRectangle';
    buttonPositionChanged();

    spSetColour("txtButtonTextColour", MGLB.button_text_colour);
    spSetColour("txtButtonBackgroundColour", MGLB.button_back_colour);
    spSetColour("txtButtonBorderColour", MGLB.button_border_colour);
    spSetColour("txtButtonTextHover", MGLB.button_text_hover);
    spSetColour("txtButtonBackgroundHover", MGLB.button_back_hover);
    spSetColour("txtButtonBorderHover", MGLB.button_border_hover);

    $ID("txtButtonDetailText").value = MGLB.button_detail_text;
    $ID("txtButtonNoteText").value = MGLB.button_note_text;

    // store the current colours for node and tooltip so that if they are changed, we can recolour all 
    // the current values.. make the colours hex uppercase so for comparsion purposes becuase  #abcdef <> #ABCDEF
    PreviousNodeHeight = parseInt(MGLB.node_height);
    PreviousNodeWidth = parseInt(MGLB.node_width);
    PreviousNodeCorners = MGLB.node_corners;
    PreviousNodeBackColour = MGLB.node_bg.toUpperCase();
    PreviousNodeTextColour = MGLB.node_fg.toUpperCase();
    PreviousNodeBorderColour = MGLB.node_border_fg.toUpperCase();
    PreviousNodeTooltipBackColour = MGLB.node_tt_bg.toUpperCase();
    PreviousNodeTooltipTextColour = MGLB.node_tt_fg.toUpperCase();
    PreviousNodeTooltipBorderColour = MGLB.node_tt_border.toUpperCase();
    PreviousLineAttr = deepCopy(MGLB.line_attr);
    PreviousImagePosition = MGLB.image_position;

    // set the show shadow colour picker based on the checkbox.. has to be after initialising the colour controls
    toggleColorPicker($('#cbModelShowShadow'));
    $find('mpeModelSettingsForm').show();

    return false;
}
function ApplyModelSettings(event) {
    event.preventDefault();
    var ddl;
    var ddlbIndex;

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnApplyModelSettings").hasAttribute("disabled")) return;

    MGLB.backgroundID = 0;
    MGLB.backgroundType = $ID('ddlbChartBackgroundType').value;
    MGLB.backgroundContent = spGetColour('txtChartBackgroundColour');
    switch (MGLB.backgroundType) {
        case 'Gradient':
            ddlbIndex = $ID("ddlbChartBackgroundGradient").selectedIndex
            selectedOption = $ID("ddlbChartBackgroundGradient").options[ddlbIndex];
            MGLB.backgroundID = selectedOption.value;
            MGLB.background = selectedOption.getAttribute("data-content");
            MGLB.gradient = selectedOption.getAttribute("data-gradient");
            break;
        case 'Image':
            ddlbIndex = $ID("ddlbChartBackgroundImage").selectedIndex
            selectedOption = $ID("ddlbChartBackgroundImage").options[ddlbIndex];
            MGLB.backgroundID = selectedOption.value;
            MGLB.background = selectedOption.getAttribute("data-content");
            MGLB.backgroundRepeat = selectedOption.getAttribute("data-imagerepeat");
            break;
    }
    SetChartBackground();

    MGLB.node_height = parseInt($ID('numModelBoxHeight').value);
    MGLB.node_width = parseInt($ID('numModelBoxWidth').value);
    MGLB.node_corners = $ID('rbModelCornerRectangle').checked == true ? 'Rectangle' : 'RoundedRectangle';

    MGLB.node_bg = spGetColour('txtNodebg');
    MGLB.node_fg = spGetColour('txtNodefg');
    MGLB.node_border_fg = spGetColour('txtBorderfg');
    MGLB.node_text_bg = spGetColour('txtNodeTxtBg');
    MGLB.node_text_bg_block = $ID('cbTextBgBlock').checked;
    MGLB.node_icon_fg = spGetColour('txtModelIconfg');
    MGLB.node_icon_hover = spGetColour('txtModelIconHover');

    MGLB.show_photos = $ID('cbShowPhotos').checked;
    MGLB.image_position = $ID('ddlbImagePosition').value; 
    MGLB.image_shape = $ID('ddlbImageShape').value; 
    MGLB.image_height = parseInt($ID('numButtonImageHeight').value);

    MGLB.node_tt_fg = spGetColour("txtTTfg");
    MGLB.node_tt_bg = spGetColour("txtTTbg");
    MGLB.node_tt_border = spGetColour("txtTTBorder");
    MGLB.node_highlight_fg = spGetColour("txtHighlightfg");
    MGLB.node_highlight_bg = spGetColour("txtHighlightbg");

    MGLB.showShadow = $ID("cbModelShowShadow").checked;
    MGLB.shadowColour = spGetColour("txtModelShadowColour");

    MGLB.linkType = $ID('ddlbModelLinkStyle').value;
    MGLB.linkColour = spGetColour("txtModelLinkColour");
    MGLB.linkHover = spGetColour("txtModelLinkHover");
    MGLB.linkWidth = parseFloat($ID('numModelLinkWidth').value);
    MGLB.linkTooltipForeground = spGetColour("txtModelLinkTooltipFg");
    MGLB.linkTooltipBackground = spGetColour("txtModelLinkTooltipBg");
    MGLB.linkTooltipBorder = spGetColour("txtModelLinkTooltipBorder");

    MGLB.button_position = rbButtonPositionLeft.checked ? "left" : "bottomleft"

    MGLB.button_font = $ID("rbButtonFontSmall").checked ? "8pt Arial" : "10pt Arial";
    MGLB.button_shape = $ID("rbButtonShapeSquare").checked ? "Rectangle" : "RoundedRectangle";
    MGLB.button_text_colour = spGetColour("txtButtonTextColour");
    MGLB.button_back_colour = spGetColour("txtButtonBackgroundColour");
    MGLB.button_border_colour = spGetColour("txtButtonBorderColour");
    MGLB.button_text_hover = spGetColour("txtButtonTextHover");
    MGLB.button_back_hover = spGetColour("txtButtonBackgroundHover");
    MGLB.button_border_hover = spGetColour("txtButtonBorderHover");
    MGLB.button_detail_text = $ID("txtButtonDetailText").value;
    MGLB.button_note_text = $ID("txtButtonNoteText").value;

    // image position can be inline (on the left) or 'above eft', 'above center','above right'
    // if changing from or to one of the above items and inline then we have to do something
    var curPosInline = MGLB.image_position === 'inline' ? true : false;
    var oldPosInline = PreviousImagePosition === 'inline' ? true : false;

    // update the default height if we are switching between inline and above
    if (curPosInline != oldPosInline)
    {
        if (curPosInline) MGLB.node_height /= 2;
        if (oldPosInline) MGLB.node_height *= 2;
    }

    myDiagram.startTransaction("ApplyNodeSetting");

    $ID('myDiagramDiv').style.backgroundColor = MGLB.model_bg;

    // set the line attributes data
    for (let i = 1; i <= 6; i++) {
        MGLB.line_attr["line" + i] = modelAttrs[i].getValue();
    }

    // apply settings to all nodes
    myDiagram.nodes.each(function (part)
    {
        if (part !== null)
        {
            var thisNode = part.data;

            // Ensure the hex values are uppercase for comparison purposes

            thisNode.node_bg = thisNode.node_bg.toUpperCase();
            thisNode.node_fg = thisNode.node_fg.toUpperCase();
            thisNode.node_border_fg = thisNode.node_border_fg.toUpperCase();
            thisNode.node_tt_bg = thisNode.node_tt_bg.toUpperCase();
            thisNode.node_tt_fg = thisNode.node_tt_fg.toUpperCase();
            thisNode.node_tt_border = thisNode.node_tt_border.toUpperCase();

            /*  To apply the attributes to a node, that node must have the same data as global before any
             *   changes we have just made, if any are different, then don't update the node'
             */
            if (thisNode.node_height == PreviousNodeHeight &&
                thisNode.node_width == PreviousNodeWidth &&
                thisNode.node_corners == PreviousNodeCorners &&
                thisNode.node_bg == PreviousNodeBackColour &&
                thisNode.node_fg == PreviousNodeTextColour &&
                thisNode.node_border_fg == PreviousNodeBorderColour &&
                thisNode.node_tt_bg == PreviousNodeTooltipBackColour &&
                thisNode.node_tt_fg == PreviousNodeTooltipTextColour &&
                thisNode.node_tt_border == PreviousNodeTooltipBorderColour &&
                compareLineAttr(thisNode.line_attr,PreviousLineAttr))
            {
                SetProperty(thisNode, "node_height", parseInt(MGLB.node_height));
                SetProperty(thisNode, "node_width", parseInt(MGLB.node_width));
                SetProperty(thisNode, "node_corners", MGLB.node_corners);

                SetProperty(thisNode, "node_bg", MGLB.node_bg);
                SetProperty(thisNode, "node_fg", MGLB.node_fg);
                SetProperty(thisNode, "node_border_fg", MGLB.node_border_fg);

                SetProperty(thisNode, "node_tt_bg", MGLB.node_tt_bg);
                SetProperty(thisNode, "node_tt_fg", MGLB.node_tt_fg);
                SetProperty(thisNode, "node_border_tt_fg", MGLB.node_tt_border);

                SetProperty(thisNode, "showShadow", MGLB.showShadow);
                SetProperty(thisNode, "shadowColour", MGLB.shadowColour);

                for (var i = 1; i <= 6; i++)
                {
                    var attr = GetLineAttrs(MGLB.line_attr, i);
                    SetProperty(thisNode, "font" + i, attr.font[i]);
                    SetProperty(thisNode, "isUnderline" + i, attr.isUnderline[i]);
                    SetProperty(thisNode, "fontColour" + i, attr.colour[i]);
                    SetProperty(thisNode, "fontBgColour" + i, attr.bg_colour[i]);
                    SetProperty(thisNode, "alignment" + i, attr.align[i]);
                }

                var imageWidth = 0;
                if (MGLB.show_photos == true)
                {
                    SetProperty(thisNode, "photoshow", thisNode.individualphotoshow == false || thisNode.individualphotoshow == 'false' ? false : MGLB.show_photos);
                    var getimage = GetNodePicture(thisNode.item_ref, MGLB.node_height);
                    SetProperty(thisNode, "source", getimage.node_picture);
                    imageWidth = getimage.picture_width
                }
                else
                {
                    SetProperty(thisNode, "photoshow", MGLB.show_photos);
                }

                SetProperty(thisNode, "node_table_width",
                    CalcNodeTableWidth(thisNode.node_type,
                        thisNode.photoshow,
                        parseInt(MGLB.node_width),
                        imageWidth));
            }

            if (thisNode.node_type === 'Detail' && curPosInline !== oldPosInline && thisNode.photoshow && MGLB.show_photos)
            {
                let nodeWidth = parseInt(thisNode.node_width);
                let nodeHeight = parseInt(thisNode.node_height);
                let imageWidth = parseInt(thisNode.picture_width);
                let imageHeight = parseInt(thisNode.image_height);
                let newHeight = 0;
                let newWidth = 0;

                if (curPosInline)
                {
                    // moving from above to inline
                    // get the height of the node, subtract the image height to get the new node height
                    // set the image height to the new node height and finally get the picture at the new
                    // node height..  IF the height of the image has actually changed
                    newHeight = Math.max(nodeHeight - imageHeight, 50);
                    newWidth = Math.min(nodeWidth + imageWidth, 500);

                    // image now is the height of the node
                    SetProperty(thisNode, "image_height", newHeight);

                    var getimage = GetNodePicture(thisNode.item_ref, parseInt(newHeight));

                    SetProperty(thisNode, "picture_width", getimage.picture_width);
                    SetProperty(thisNode, "node_table_width",
                        CalcNodeTableWidth(thisNode.node_type,
                            $ID('cbNodeShowPhoto').checked,
                            parseInt($ID('numNodeBoxWidth').value),
                            getimage.picture_width));

                    setTimeout(() =>
                    {
                        SetProperty(thisNode, "source", getimage.node_picture);
                    }, 100);

                } else
                {
                    // moving from inline to above
                    // double the height of the node, set the width to the width minus the image width 
                    // the picture height doesn't change as we are just moving up above the text table
                    newHeight = Math.min(thisNode.node_table_width + imageHeight, 200);
                    newWidth = Math.max(nodeWidth - imageWidth, 100);
                }

                SetProperty(thisNode, "node_height", newHeight);
                SetProperty(thisNode, "node_width", newWidth);
            }
        }
    });

    myDiagram.commitTransaction('ApplyNodeSetting');
    myDiagram.rebuildParts();
    myDiagram.requestUpdate();

    $find('mpeModelSettingsForm').hide();
    ModelChanged(true);

    return false;
}
function CancelModelSettings(event) {
    // Cancel button for the modal modal settings
    event.preventDefault();

    // if the button is currently disabled, ensure we don't allow it to process as event handers ignore disabled attrib
    if ($ID("btnCancelModelSettings").hasAttribute("disabled")) return;

    $find('mpeModelSettingsForm').hide();
    return false;
}
function ToggleModelApply(flag) {
}
function ModelTabChanged() {
    $find("mpeModelSettingsForm")._layout();
}
function ItemDetailForm(node) {
    var data = node.data

    // if (data.node_type !== "Detail" && data.node_type !== "Team") return;
    $ID("divItemDetail").innerHTML = GetDetailInfo(data.item_ref);

    // Display the dialog
    $find('mpeItemDetailForm').show();
    return false;
}
function CancelItemDetail(event) {
    event.preventDefault();
    $find('mpeItemDetailForm').hide();
    myDiagram.focus();
    return false;
}
function TextBlockChanged(event) {
}
function NodeTabChanged() {
    $find("mpeNodeSettingsForm")._layout();
}
function ChangedBackgroundType() {
    // Get the selected value of the dropdown
    var selectedType = $ID('ddlbChartBackgroundType').value;

    $ID('trBackgroundImage').style.display = selectedType == 'Image' ? '' : 'none';
    $ID('trBackgroundGradient').style.display = selectedType == 'Gradient' ? '' : 'none';
}

function RestrictApplyOptions(data)
{
    // Show or hide the options in the apply dropdown based on the node type.
    //  see ddlbApplyOptions in diagram.aspx
    // detail, vacant, team gets the standard list
    // parent group gets a specific set with "parent group" instead of "node" in the lines
    // co-parent nodes get the standard list plus a couple more for co-parent

    var category = "node";
    if (data.node_type == "ParentGroup")
        category = "pg";
    else if (data.isCoParent)
        category = "coparent";

    var AlreadySetFirst = false;
    $('#ddlbApplyOptions option').each(function ()
    {
        var item = $(this);
        var categories = item.attr('data-cat');
        item.prop('selected', false);
        if (categories && categories.split(' ').includes(category)) {
            item.show();
            if (!AlreadySetFirst) {
                AlreadySetFirst = true
                item.prop('selected', true);
            }
        } else {
            item.hide();
        }
    });
}

function ChangeApplyOptions(data)
{
    var opt = $ID('ddlbApplyOptions');
    var ddlbTypes = $ID('ddlbApplyTypes');
    var cbSizes =  $ID('cbApplySizes').parentElement;

    if (opt.value == 'Apply')
    {
        elHide(ddlbTypes);
        elHide(cbSizes);
    } else {
        elShow(ddlbTypes);
        elShow(cbSizes);

        Array.from(ddlbTypes.options).forEach(function (option)
        {
            if (option.value === 'alltypes')
            {
                option.style.display = 'block'; // Always show 'All Types'
                return;
            }

            if ((option.value == 'detail' && data.node_type == 'Detail' && !data.isCoParent) ||
                (option.value == 'team' && data.node_type == 'Team' && !data.isCoParent) ||
                (option.value == 'vacant' && data.node_type == 'Vacant' && !data.isCoParent) ||
                (option.value == 'coparent' && data.isCoParent) ||
                (option.value == 'assistant' && data.isAssistant))
            {
                option.style.display = 'block'; // Show the matching option
                return;
            }
            option.style.display = 'none'; // Hide other options
        });
    }
}
function GetLineAttrs(attr)
{
    // get the line attributes for display

    var font = new Array(7);
    var under = new Array(7);
    var colour = new Array(7);
    var bg_colour = new Array(7);
    var align = new Array(7);

    for (var i = 1; i <= 6; i++) {
        font[i] = "Arial 10px";
        under[i] = false;
        colour[i] = "";
        bg_colour[i] = "";
        align[i] = go.Spot.Left;
    }

    if (!attr || attr == "") 
        return {
            font: font,
            isUnderline: under,
            colour: colour,
            bg_colour: bg_colour,
            align: align
        };

    // we need to extract the (up to) 6 lines of attribute information
    for (var i = 1; i < 7; i++) {
        var ln = attr["line" + i];

        font[i] = ""
        if (ln.bold) font[i] += 'bold '
        if (ln.italic) font[i] += 'italic '
        font[i] += ln.fontSize + " " + ln.font;

        under[i] = ln.underline;
        colour[i] = ln.colour.trim();
        bg_colour[i] = ln.bg_colour ? ln.bg_colour.trim() : "";

        switch (ln.align.toLowerCase()) {
            case '':
            case 'left':
                align[i] = go.Spot.Left;
                break;
            case "center":
                align[i] = go.Spot.Center;
                break;
            case "right":
                align[i] = go.Spot.Right;
                break;
        }
    }

    return {
        font: font,
        isUnderline: under,
        colour: colour,
        bg_colour: bg_colour,
        align: align
    };
}
function GetSingleLineAttrs(line_attr, line_number)
{
    var ln = line_attr["line" + line_number] || {};

    return {
        font: ln.font || 'Arial',
        fontSize: ln.fontSize || '10pt',
        bold: ln.bold || false,
        italic: ln.italic || false,
        underline: ln.underline || false,
        colour: ln.colour || '',
        bg_colour: ln.bg_colour || '',
        align: ln.align || 'left'
    };
}
function SetSingleLineAttrs(line_attr, attr, line_number) {
    var ln = line_attr["line" + line_number];
    if (ln) {
        for (let prop in attr) {
            if (attr.hasOwnProperty(prop)) {
                ln[prop] = attr[prop];
            }
        }
    }
}
function deepCopy(obj) {
    if (obj === null || typeof obj !== 'object') {
        return obj;
    }

    if (Array.isArray(obj)) {
        return obj.map(deepCopy);
    }

    const copy = {};
    for (let key in obj) {
        if (obj.hasOwnProperty(key)) {
            copy[key] = deepCopy(obj[key]);
        }
    }
    return copy;
}
function toggleColorPicker(checkboxElement) {
    // Function to toggle the color picker
    var isChecked = checkboxElement.is(':checked');
    var targetId = checkboxElement.closest('span[data-target]').data('target');
    $(targetId).spectrum(isChecked ? 'enable' : 'disable');
}
function buttonPositionChanged() {
    // if button position = left then disable the radio buttons
    rbButtonFontSmall.disabled = rbButtonPositionLeft.checked
    rbButtonFontMedium.disabled = rbButtonPositionLeft.checked
    rbButtonShapeSquare.disabled = rbButtonPositionLeft.checked
    rbButtonShapeRounded.disabled = rbButtonPositionLeft.checked

    $ID("lblButtonDetailText").style.display = rbButtonPositionLeft.checked ? "none" : "inline-block";
    $ID("lblButtonDetailTooltip").style.display = rbButtonPositionLeft.checked ? "inline-block" : "none";
    $ID("lblButtonNoteText").style.display = rbButtonPositionLeft.checked ? "none" : "inline-block";
    $ID("lblButtonNoteTooltip").style.display = rbButtonPositionLeft.checked ? "inline-block" : "none"; 

    $ID("trButtonImageHeight").style.display = rbButtonPositionLeft.checked ? "none" : "table-row";
}
function GetNoteWidth()
{
    return NoteEditor.getContainer().offsetWidth;
}
function SetNoteWidth(width)
{
    NoteEditor.getContainer().style.width = width + 'px';
}
function toggleControlEnabled(checkboxId, otherControlId)
{
    var checkbox = $ID(checkboxId);
    var control = $ID(otherControlId);
    control.disabled = !checkbox.checked;
}

function compareLineAttr(lineAttr1, lineAttr2)
{
    // Check if both are objects and not null
    if (typeof lineAttr1 === 'object' && lineAttr1 !== null &&
        typeof lineAttr2 === 'object' && lineAttr2 !== null) {

        // List of properties to compare
        const properties = ['font', 'fontSize', 'bold', 'italic', 'underline', 'colour', 'bg_colour', 'align'];

        // Compare each property
        for (let prop of properties) {
            if (lineAttr1[prop] !== lineAttr2[prop]) {
                return false;
            }
        }

        // If all properties match, return true
        return true;
    } else {
        // If either is not an object or is null, return false
        return false;
    }
}
