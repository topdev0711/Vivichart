/* data view javascript - additional to client_settings.js */

$(document).ready(function ()
{
    ApplyColourControls(".SpectrumColour", false, 'vcPalette');

    // enable / disable the shadow colour box based on 'show shadow checkbox'
    var isChecked = $('#cbDetailShowShadow').is(':checked');
    toggleColorPicker(isChecked);

    // On 'show shadow' checkbox change
    $('#cbDetailShowShadow').change(function () {
        var isChecked = $(this).is(':checked');
        toggleColorPicker(isChecked);
    });

    // divs originally hidden, so now we have the page drawn, show them
    $('.divScroll').show();

    const checkboxes = $QSAll('span[data-group] input[type="checkbox"]');
    const divDEStart = $ID('pnlDEStart');
    const divDELength = $ID('pnlDELength');

    function SetFloatFieldVisibility()
    {
        if ($ID("hdnFieldType").value !== "03" && $ID("hdnFieldType").value !== "04") return;

        var checkbox = $ID('cbDEcurrency');
        $QSAll('.nonCurrency').forEach(function (row) {
            row.style.display = checkbox.checked ? 'none' : 'table-row';
        });
    }

    $ID('cbDEcurrency').addEventListener('change', function () {
        SetFloatFieldVisibility()
    });
    SetFloatFieldVisibility()

    // ----------------------------------------------------------------------------------

    function SetPartVisibility() {
        let anyChecked = false;
        let midChecked = false;

        checkboxes.forEach(checkbox => {
            if (checkbox.checked) {
                anyChecked = checkbox.id === 'cbDEPartLeft' || checkbox.id === 'cbDEPartRight' || checkbox.id === 'cbDEPartMid';
                midChecked = checkbox.id === 'cbDEPartMid';
            }
        });
        divDEStart.style.display = midChecked ? 'inline' : 'none';
        divDELength.style.display = anyChecked ? 'inline' : 'none';
    }

    checkboxes.forEach(cbox => {
        cbox.addEventListener('change', function () {
            if (this.checked) {
                const group = this.closest('span[data-group]').getAttribute('data-group');
                checkboxes.forEach(box => {
                    if (box !== this && box.closest('span[data-group]').getAttribute('data-group') === group)
                        box.checked = false;
                });
            }
            SetPartVisibility();
        });
    });
    SetPartVisibility();

    // ----------------------------------------------------------------------------------

    var txtBox = $QS('.seltxtDEtext');
    var rowHover = $QS('.seltrDEtextHover');
    var lblHover = $QS('.sellblDEtextHover');

    function checkSpaces()
    {
        var regex = /^\s/;
        var startsWithSpace = regex.test(txtBox.value);
        showHideControl(startsWithSpace);
    }

    function showHideControl(showControl)
    {
        rowHover.style.display = showControl ? 'table-row' : 'none';
    }

    function replaceSpacesWithUnderscores()
    {
        txtBox.value = txtBox.value.replace(/ /g, '_');
    }

    function revertUnderscoresToSpaces()
    {
        txtBox.value = txtBox.value.replace(/_/g, ' ');
    }

    txtBox.addEventListener('keyup', checkSpaces);
    txtBox.addEventListener('blur', checkSpaces);
    checkSpaces();

    lblHover.addEventListener('mouseover', replaceSpacesWithUnderscores);
    lblHover.addEventListener('mouseout', revertUnderscoresToSpaces);
});

function toggleColorPicker(isChecked)
{
    $("#txtDetailShadow").spectrum(isChecked ? 'enable' : 'disable');
}
function ViewTabChanged()
{
    $find('mpeDataViewForm')._layout();
}
function accPostback(name)
{
    // if going between listhead and listdata then we don't need to postback
    //  or, if we are going between a non list and another non-list then we don't need to postback

    var accTypes = ["ListHead", "ListData", "ListSort"];
    var fromList = accTypes.includes($ID("hdnAccValue").value);
    var toList = accTypes.includes(name);

    $ID("hdnAccValue").value = name;

    // unless switching to and from a related data source, don't postback'
    if ((fromList && toList) || (!fromList && !toList))
        return;

    $ID('btnAccPanel').click();
}

let dragged;
let dragSource;

function dragStart(e)
{
    dragged = e.target;
    let rowIndex = e.target.rowIndex - 1;
    e.dataTransfer.setData('rowIndex', rowIndex.toString());
    //e.dataTransfer.setData("source", "field");
    dragSource = "field";
}
function dragSortStart(e)
{
    dragged = e.target;
    let rowIndex = e.target.parentElement.rowIndex;
    e.dataTransfer.setData('rowIndex', rowIndex.toString());

    // Get the 'data-source' attribute value of the cell
    let dataSource = e.target.getAttribute("data-source");

    // Set it as the 'source' data for the drag operation
    //e.dataTransfer.setData("source", dataSource);
    dragSource = dataSource;
}
function allowDrop(e)
{
    e.preventDefault();
    if (!dragged) return;
    let trElement = e.target;
    while (trElement.tagName.toLowerCase() !== 'tr')
    {
        trElement = trElement.parentElement;
    }
    // prohibit drops from field to sequence rows where the type is for headers
    let dragTarget = trElement.getAttribute("data-target");

    if (dragSource == "field" && dragTarget == "sort") return;

    // Add the dragOver class to each cell in the row
    for (let i = 0; i < trElement.cells.length - 1; i++)
    {
        trElement.cells[i].classList.add('dragOver');
    }
}
function dragLeave(e)
{
    if (!dragged) return;
    let trElement = e.target;
    while (trElement.tagName.toLowerCase() !== 'tr')
    {
        trElement = trElement.parentElement;
    }

    // Remove the dragOver class from each cell in the row
    for (let i = 0; i < trElement.cells.length - 1; i++)
    {
        trElement.cells[i].classList.remove('dragOver');
    }
}
function drop(e)
{
    e.preventDefault();
    if (dragged)
    {
        let trElement = e.target;
        while (trElement.tagName.toLowerCase() !== 'tr')
        {
            trElement = trElement.parentElement;
        }

        // Remove the dragOver class from each cell in the row
        for (let i = 0; i < trElement.cells.length; i++)
        {
            trElement.cells[i].classList.remove('dragOver');
        }

        let rowIndexSource = parseInt(e.dataTransfer.getData('rowIndex'));
        //let dragSource = e.dataTransfer.getData('source');

        let rowIndexTarget = trElement.rowIndex;

        // Populate the hidden field and trigger the hidden button
        $ID('hdnDrop').value = dragSource + '|' + rowIndexSource + '|' + rowIndexTarget;
        $ID('btnDrop').click();
        dragged = null;
    }
}

// this line is added to remove `detail` tab_pannel that is visible even when `chart` tab_pannel is open
// remove this line when bug is fixed
//setTimeout(() => {document.querySelector('.detail_tab').style.display = 'none'}, 500);
