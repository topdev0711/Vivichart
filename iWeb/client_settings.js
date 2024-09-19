
"use strict";

var ATTR;
var Attrs = new Array(7);

$(document).ready(function () {
    if (!$ID('hdnAttrib'))
        return;
    
    $("#IAWPanel1").removeClass("hidden");
    $("#IAWPanel1").removeClass("PanelClass");

    ATTR = JSON.parse($ID('hdnAttrib').value);

    // initialise the line attributes
    for (let i = 1; i <= 6; i++)
    {
        Attrs[i] = new LineAttrib($ID(`lblNodeLine${i}`), $ID("ddlbFont"), change_Charts);
        Attrs[i].setValue(JSON.stringify(ATTR.lines[`line${i}`]));
    }

    // initialise the colour controls
    ApplyColourControls(".SpectrumChartEmpty", true, 'vcPalette', () => change_Charts());
    ApplyColourControls(".SpectrumChart", false, 'vcPalette', () => change_Charts());
    ApplyColourControls(".SpectrumColourEmpty", true, 'vcPalette');

    // Function to toggle the color picker
    function toggleColorPicker(checkboxElement) {
        var isChecked = checkboxElement.is(':checked');
        var targetId = checkboxElement.closest('span[data-target]').data('target');
        $(targetId).spectrum(isChecked ? 'enable' : 'disable');
    }

    // Initialize the color pickers based on checkbox state
    $('span[data-target] input[type="checkbox"]').each(function () {
        toggleColorPicker($(this));
    });

    // Attach change events to checkboxes
    $('span[data-target] input[type="checkbox"]').change(function () {
        toggleColorPicker($(this));
    });
    
    // panel originally hidden to give the colour picker time to initialise
    $ID("pnlDisplay").classList.remove('hidden');

    // ensure the image type dropdown shows the correct option
    backgroundTypeChanged();

    if (window.location.pathname.includes('client_settings'))
    {
        // ensure the image type dropdown shows the correct option
        bodyBackgroundTypeChanged();

        // set vcPalette local before initialize branding colour spectrum
        setVcPalette();

        initializeGradientGenerator();
        logoColourSelection();

        $ID('cbAdvanced').checked && $ID('tpGeneral').classList.add('childTabScroll')

        if ($ID('hdnBgType').value == '01' && $ID('hdnBgContent').value != '') {
            $('.bgImgPreview').html($('<img>', { src: `Backgrounds/${$ID('hdnBgContent').value}` }));
            $('.bgImgPreview').addClass('active')
        }
        ($ID('hdnBgImageName').value != "" && $ID('hdnBgType').value === '01') && ($ID('bgImageLabel').innerText = $ID('hdnBgImageName').value)
        if ($ID('rbBgSelectImage').checked) {
            $('#divBgImage').removeAttr('hidden');
        } else {
            $('#divBgGradient').removeAttr('hidden');
        }

        // radio button on themes selection
        $('span.selectThemeRadioButton input[type=radio]').change(function () {
            // First, uncheck all RadioButtons
            $('span.selectThemeRadioButton input[type=radio]').prop('checked', false);
            $(this).prop('checked', true);
        });
    }

    //all branding colour spectrum rendered using below function call
    ApplyColourControls(".SpectrumBranding", false, 'vcPalette', () => {
        change_Branding();
    });

    ApplyColourControls("#simpPrincipal", false, 'vcPalette', () => {
        change_Branding();
        $ID('simpLightPrincipal').value = lightenRGBAColor($ID('simpPrincipal').value, 30)
    });
    ApplyColourControls("#simpContentArea", false, 'vcPalette', () => {
        change_Branding();
        $ID('simpDarkContextArea').value = darkenRGBAColor($ID('simpContentArea').value, 30)
    });
    ApplyColourControls("#simpText", false, 'vcPalette', () => {
        change_Branding();
        $ID('simpLightText').value = lightenRGBAColor($ID('simpText').value, 30)
    });
});

async function logoColourSelection() {
    const logoPreview = document.querySelector('#logoPreview')
    if (logoPreview) {
        if (!await isValidImage(logoPreview.src)) return
        if (logoPreview.complete) {
            logoPreviewLoaded(logoPreview);
        } else {
            logoPreview.onload = () => logoPreviewLoaded(logoPreview);
        }
    }
}

function logoPreviewLoaded(logoPreview) {
    ImageToCanvas(logoPreview)
    document.getElementById('logoPreview').addEventListener('click', function (e) {
        const selectedColour = getImagePixelColour(e)
        const colorPicker = document.querySelectorAll('.SpectrumLogo')
        colorPicker[0].value = selectedColour
        colorPicker[1].value = lightenRGBAColor(selectedColour, 30)
        colorPicker[2].value = darkenRGBAColor(selectedColour, 30)
        setVcPalette()
        ApplyColourControls(".SpectrumLogo")
        change_Branding()
    })
}

function setVcPalette() {
    const colorPicker = document.querySelectorAll('.SpectrumLogo')
    if (colorPicker[0].value != '') {
    localStorage.setItem('vcPalette', `${colorPicker[2].value}; ${colorPicker[1].value}; ${colorPicker[0].value}`)
        $('.logoColourPreview > div').css({ 'visibility': 'visible' })
    } else {
        localStorage.setItem('vcPalette', false)
        $('.logoColourPreview > div').css({ 'visibility': 'hidden' })
    }
}

function change_Branding() {
    $("#pnlGrdBrand").addClass("noClick");
    hdnChangesPending.value = 'branding';
    disableOnchange()
}

const change_Charts = () => {
    hdnChangesPending.value = 'chart';
    disableOnchange()
}

function disableOnchange() {
    $('#btnGlobalSave').css({ 'visibility': 'visible' });
    $('#btnGlobalCancel').css({ 'visibility': 'visible' });
    $("#tcBranding_header").addClass("noClick");
    BannerEnabled(false);
}

function RestringData() {
    if (!$ID('hdnAttrib'))
        return;

    ATTR.lines.line1 = Attrs[1].getValue();
    ATTR.lines.line2 = Attrs[2].getValue();
    ATTR.lines.line3 = Attrs[3].getValue();
    ATTR.lines.line4 = Attrs[4].getValue();
    ATTR.lines.line5 = Attrs[5].getValue();
    ATTR.lines.line6 = Attrs[6].getValue();
    
    $ID('hdnAttrib').value = JSON.stringify(ATTR);

    if (window.location.pathname.includes('client_settings')) {
        //update CSS filter hidden fields
        if ($('#cbAdvanced').prop('checked')) {
            $ID("hdnHeaderIconsFilter").value = ConvertColourToFilter($ID("listHeaderIconsColour").value);
            $ID("hdnHeaderIconsHoverFilter").value = ConvertColourToFilter($ID("listHeaderIconsHoverColour").value);
            $ID("hdnBodyIconsFilter").value = ConvertColourToFilter($ID("listBodyIconsColour").value);
            $ID("hdnBodyIconsHoverFilter").value = ConvertColourToFilter($ID("listBodyIconsHoverColour").value);
            $ID("hdnCheckboxFilter").value = ConvertColourToFilter($ID("checkboxColour").value);
        } else {
            $ID("hdnHeaderIconsFilter").value = ConvertColourToFilter($ID("simpContentArea").value);
            $ID("hdnHeaderIconsHoverFilter").value = ConvertColourToFilter($ID("simpPrincipal").value);
            $ID("hdnBodyIconsFilter").value = ConvertColourToFilter($ID("simpLightText").value);
            $ID("hdnBodyIconsHoverFilter").value = ConvertColourToFilter($ID("simpPrincipal").value);
            $ID("hdnCheckboxFilter").value = ConvertColourToFilter($ID("simpText").value);
        }
    }
}

function RestringBackgroundData() {

    const structure = {}
    structure.gradientDetails = {}
    structure.gradientDetails.colors = []
    structure.gradientString = gradient_object.string
    structure.gradientDetails.gradientType = gradient_object.type
    structure.gradientDetails.angle = gradient_object.angle

    gradient_object.color_list.forEach(item => {
        const v = item.color.substring(item.color.indexOf('(') + 1, item.color.indexOf(')')).split(',').map(value => parseFloat(value.trim()));
        if (v[3] == undefined) v[3] = 1
        structure.gradientDetails.colors.push({red: v[0], green: v[1], blue: v[2], alpha: v[3], position: item.color_stop})
    })

    $ID('hdnBgStructure').value = JSON.stringify(structure)
}

function bgNameInput() {
    if ($ID('hdnBgType').value == '') {
        if ($ID('rbBgSelectImage').checked) {
            if ($ID('bgImageLabel').innerText != 'No File Chosen') bgChange()
        } else {
            bgChange()
        }
    } else {
        if ($ID('hdnBgType').value == '01') {
            if ($ID('bgImageLabel').innerText != 'No File Chosen') bgChange()
        } else {
            bgChange()
        }
    }
}

function bgImageRepeatTypeChange() {
    if ($ID('bgImageLabel').innerText != 'No File Chosen') bgChange()
}

function getAttr(object, lineNumber) {
    const propertyPath = `line${lineNumber}`;
    const properties = propertyPath.split('.');
    let value = object;

    for (const property of properties) {
        value = value[property];
    }

    return {
        font: value.font,
        fontSize: value.fontSize,
        bold: value.bold,
        italic: value.italic,
        underline: value.underline,
        colour: value.colour,
        bg_colour: value.bg_colour,
        alignment: value.alignment
    };
}

function setAttr(object, lineNumber, newAttr) {
    const propertyPath = `line${lineNumber}`;
    const properties = propertyPath.split('.');
    let value = object;

    // Update the properties in the original object
    value[properties[properties.length - 1]] = {
        font: newAttr.font,
        fontSize: newAttr.font_size,
        bold: newAttr.bold,
        italic: newAttr.italic,
        underline: newAttr.underline,
        colour: newAttr.colour,
        bg_colour: newAttr.bg_colour,
        alignment: newAttr.alignment
    };
}

function backgroundTypeChanged() {

    var selectedValue = $ID('ddlbBackgroundType').value;

    // Initially hide all optional rows
    $ID('trBackgroundImage').style.display = 'none';
    $ID('trBackgroundGradient').style.display = 'none';

    switch (selectedValue) {
        case 'Gradient':
            $ID('trBackgroundGradient').style.display = '';
            break;
        case 'Image':
            $ID('trBackgroundImage').style.display = '';
            break;
    }
}

function bodyBackgroundTypeChanged() {
    if (!$ID('cbAdvanced').checked) return
    var selectedValue = $ID('ddlbBodyBackgroundType').value;

    // Initially hide all optional rows
    $ID('trBodyBackgroundGradient').style.display = 'none';
    $ID('trBodyBackgroundImage').style.display = 'none';

    switch (selectedValue) {
        case 'Gradient':
            $ID('trBodyBackgroundGradient').style.display = '';
            break;
        case 'Image':
            $ID('trBodyBackgroundImage').style.display = '';
            break;
    }
}

function UpChange(elem) {
    const upFileLable = document.getElementById('labFileName')
    if (elem.files.length > 0) {
        upFileLable.innerText = elem.files[0].name
        upFileLable.title = elem.files[0].name
        populateFields(elem.files[0])
        change_Branding();
    }
}

function allowDrop(ev) {
    ev.preventDefault();
}

function highlightDropArea(ev) {
    ev.preventDefault();
    document.querySelector('.drop-here').style.display = 'flex';
}

function unhighlightDropArea(ev) {
    if (isCursorOutsideContainer(ev)) {
        document.querySelector('.drop-here').style.display = 'none';
    }
}

function drop(ev) {
    ev.preventDefault();
    document.querySelector('.drop-here').style.display = 'none';
    var file = ev.dataTransfer.files[0];
    populateFields(file)
}

function isCursorOutsideContainer(event) {
    const containerRect = document.querySelector('.drop-here').getBoundingClientRect();
    return (
        event.clientX < containerRect.left ||
        event.clientX > containerRect.right ||
        event.clientY < containerRect.top ||
        event.clientY > containerRect.bottom
    );
}

function populateFields(file) {

    //checking file size here
    const maxSizeMB = 10;
    const fileSizeMB = file.size / (1024 * 1024);
    if (fileSizeMB > maxSizeMB) return false

    var reader = new FileReader();
    reader.onload = async function (e) {
        //validating selected file
        const isImage = await isValidImage(e.target.result);
        if (isImage) {
            let canvas = document.querySelector('#logoPreview')
            if (!canvas) {
                canvas = document.createElement('canvas')
                canvas.id = 'logoPreview'
                document.querySelector('#trLogoPreview td').appendChild(canvas)

            }
            const imgTag = document.createElement('img')
            imgTag.src = e.target.result
            imgTag.id = canvas.id
            canvas.parentNode.replaceChild(imgTag, canvas);
            logoColourSelection();
            const base64Array = e.target.result.split(',')
            base64Array.shift()
            document.querySelector('#hdnLogofilename').value = file.name
            document.querySelector('#hdnLogo').value = base64Array.join(',');
            $('#trLogoPreview').removeClass('hidden')
            $('#trLogoPreview').addClass('formrow')
            $('#trLogoColours').removeClass('hidden')
            $('#trLogoPreview').addClass('formrow')
        } else {
            document.getElementById('labFileName').innerText = 'No file Chosen'
        }
    };
    reader.readAsDataURL(file);
}

function isValidImage(base64String) {
    return new Promise(resolve => {
        const image = new Image();
        image.src = base64String;

        image.onload = function () {
            if (image.width > 0 && image.height > 0) {
                resolve(true)
            } else {
                resolve(false)
            }
        };
        image.onerror = function () {
            resolve(false)
        };
    })
}

function bgTypeSelection(type) {
    $ID('hdnBgContent').value = ''
    switch (type) {
        case 'gradient':
            $ID('divBgGradient').removeAttribute('hidden');
            $ID('divBgImage').setAttribute('hidden', 'hidden');
            renderGradientGenerator()
            if ($ID('txtBgName').value != '') bgChange()
            break;
        default:
            $ID('divBgGradient').setAttribute('hidden', 'hidden');
            $ID('divBgImage').removeAttribute('hidden');
            if ($ID('bgImageLabel').innerText == 'No File Chosen') $('#btnBackgroundSave').css({ display: 'none' })
            break;
    }
}

let angle_range
let add_new_color
let gardient_type


const gradient_object = {
    type: 'linear',
    angle: null,
    color_list: [
        { color: "rgba(255, 255, 255, 1)", color_stop: 0 },
        { color: "rgba(0, 0, 0, 1)", color_stop: 100 }
    ],
    string: null
}

const bgChange = () => $('#btnBackgroundSave').css({ display: 'inline-block' });

function initializeGradientGenerator() {
    angle_range = document.querySelector('#angle')
    add_new_color = document.querySelector('#btnAddNewColour')
    gardient_type = document.querySelectorAll('.gardient_type')

    gradient_object.angle = Math.round(angle_range.value)
    parseGradient()
    renderGradientGenerator(true)
    renderColorList()
    angle_range.oninput = () => {
        angle_range.nextElementSibling.innerText = Math.round(angle_range.value) 
        gradient_object.angle = angle_range.value
        renderGradientGenerator();
    }

    add_new_color.onclick = (e) => {
        e.preventDefault();
        const n = document.querySelectorAll('.gradient-color-list li')
        const new_stop_value_array = divide100IntoNParts(n.length + 1)
        gradient_object.color_list.push({ color: "rgba(0, 0, 0, 1)", color_stop: 100 })
        updateGradientStopArray(new_stop_value_array)
        renderColorList();
        renderGradientGenerator()
        bgChange()
        const listAreaDiv = document.querySelector('.list-area > div')
        listAreaDiv.scrollTop = listAreaDiv.scrollHeight;
    }

    angle_range.value = gradient_object.angle
    angle_range.nextElementSibling.innerText = Math.round(angle_range.value) 
    if (gradient_object.type != 'linear') {
        $('#rbLinearGradient').prop('checked', false)
        $('#rbRadialGradient').prop('checked', true)
        $('.angle-selector').css({ 'visibility': 'hidden' })
    }
}

function gradientTypeChange(value) {
    bgChange()
    gradient_object.type = value
    gradient_object.angle = value === 'linear' ? Math.round(angle_range.value) : NaN
    renderGradientGenerator()
    value === 'radial' ? $('.angle-selector').css({ 'visibility': 'hidden' }) : $('.angle-selector').css({ 'visibility': 'visible' })
}

function renderColorList() {
    const list_container = document.querySelector('.gradient-color-list')
    list_container.innerHTML = ''
    gradient_object.color_list.forEach((item, index) => {
        const color_list_html = `
            <li>
                <input type="text" class="SpectrumGradient" value="${item.color}" data-index="${index}" />
                <input type="range" oninput="updateGradientStopValue(this, ${index})" class="gradient-stop-range" value="${item.color_stop}" onchange="bgChange()">
                <span>${item.color_stop}%</span>
                <button onclick="deleteColorItem('${index}')">&#10005;</button>
            </li>
        `;
        list_container.innerHTML += color_list_html
    })
    ApplyColourControls(".SpectrumGradient", false, 'vcPalette', (color, index) => {
        gradient_object.color_list[index].color = `rgba(${color._r}, ${color._g}, ${color._b}, ${color.getAlpha()})`
        renderGradientGenerator()
        bgChange()
    });
}

function updateGradientStopValue(target, index) {
    const prev = parseInt(target.parentNode.previousElementSibling?.querySelector('span').innerText)
    const next = parseInt(target.parentNode.nextElementSibling?.querySelector('span').innerText)
    if (target.value < prev || target.value > next) target.value = target.value < prev ? prev : next;

    target.nextElementSibling.innerText = Math.round(target.value) + '%'
    gradient_object.color_list[index].color_stop = Number(target.value)
    renderGradientGenerator()
}

function deleteColorItem(index) {
    const n = document.querySelectorAll('.gradient-color-list li')
    const new_stop_value_array = divide100IntoNParts(n.length - 1)
    gradient_object.color_list.splice(index, 1);
    updateGradientStopArray(new_stop_value_array)
    renderColorList();
    renderGradientGenerator()
    bgChange()
}

function updateGradientStopArray(new_array) {
    for (let i = 0; i < gradient_object.color_list.length; i++) {
        gradient_object.color_list[i].color_stop = new_array[i];
    }
}

function renderGradientGenerator(stopResult) {
    const formattedArray = gradient_object.color_list.map(item => `${item.color} ${item.color_stop}%`);
    let gradient = null
    if (gradient_object.type === "linear") {
        gradient = `linear-gradient(${gradient_object.angle}deg, ${formattedArray.join(', ')})`
    } else {
        gradient = `radial-gradient(circle, ${formattedArray.join(', ')})`
    }
    document.querySelector('.gradient-preview').style.background = gradient
    gradient_object.string = gradient
    
    if (!stopResult) {
        $ID("hdnBgContent").value = gradient
    }
    $find('mpeBackgroundForm')._layout();
}

function divide100IntoNParts(n) {
    const result = [];
    const interval = 100 / (n - 1);

    for (let i = 0; i < n; i++) {
        const value = i * interval;
        result.push(Math.round(value));
    }

    return result;
}

function bgImageChange(elem) {
    const label = document.getElementById('bgImageLabel')
    if (elem.files.length > 0) {
        var reader = new FileReader();
        reader.onload = async function (e) {
            //validating selected file
            const isImage = await isValidImage(e.target.result);
            if (isImage) {
                bgChange()
                label.innerText = elem.files[0].name
                const base64Array = e.target.result.split(',')
                base64Array.shift()
                $ID("hdnBgContent").value = base64Array.join(',')
                $('.bgImgPreview').html($('<img>', { src: e.target.result }));
                $('.bgImgPreview').addClass('active')
                $ID("hdnBgImageName").value = elem.files[0].name
            } else {
                label.innerText = 'No File Chosen'
                $ID("hdnBgContent").value = ''
            }
        };
        reader.readAsDataURL(elem.files[0]);
    } else {
        label.innerText = 'No File Chosen'
        $ID("hdnBgContent").value = ''
    }
}

function parseGradient() {
    const gString = $ID('hdnBgStructure').value
    if (gString != '') {
        const gradient = JSON.parse(gString)
        gradient_object.type = gradient.gradientDetails.gradientType
        gradient_object.angle = gradient.gradientDetails.angle
        gradient_object.string = gradient.gradientstring
        const colorlist = []
        gradient.gradientDetails.colors.forEach((item, index) => {
            colorlist.push({ color: `rgba(${item.red}, ${item.green}, ${item.blue}, ${item.alpha})`, color_stop: item.position })
        })
        gradient_object.color_list = colorlist
    }
}

function isGradient(content) {
    if (content != "") {
        const arr = content.split('-')[0]
        return (arr === 'linear' || arr === 'radial') ? true : false
    } else {
        return false
    }
}

function ImageToCanvas(imgTag) {
    const canvas = document.createElement('canvas');
    canvas.id = imgTag.id
    canvas.width = imgTag.width;
    canvas.height = imgTag.height;
    const context = canvas.getContext('2d');
    context.drawImage(imgTag, 0, 0, imgTag.width, imgTag.height);

    imgTag.parentNode.replaceChild(canvas, imgTag);
}

function getImagePixelColour(e) {
    var canvas = e.target
    var ctx = canvas.getContext('2d');

    var rect = canvas.getBoundingClientRect();
    var x = e.clientX - rect.left;
    var y = e.clientY - rect.top;

    var pixel = ctx.getImageData(x, y, 1, 1).data;
    return `rgba(${pixel[0]}, ${pixel[1]}, ${pixel[2]}, ${pixel[3] !== 255 ? (pixel[3] / 255).toFixed(2) : 1})`;

}