class LineAttrib {

    static fieldNames = {
        colour: "Text Colour",
        bg_colour: "Text Background",
        font: "Font",
        fontSize: "Font Size",
        bold: "Bold",
        boldChar: "B",
        italic: "Italic",
        italicChar: "i",
        underline: "Underline",
        underlineChar: "U",
        left: "Left Justified",
        center: "Centred",
        right: "Right Justified",
    }

    // declare class instance varaibles
    orgElement;
    orgFontSelect;
    onOpenCallback;
    onCloseCallback;
    breakLine;
    ft_fn;
    ft_container;
    ft_controls;
    ft_font;
    ft_fontSize;
    ft_colour;
    ft_bg_colour;
    ft_bold;
    ft_italic;
    ft_underline;
    ft_align_left;
    ft_align_center;
    ft_align_right;


    constructor(myElement, myFontSelect, ...args) {
        this.orgElement = myElement;
        this.orgFontSelect = myFontSelect;

        // Default values
        this.change = () => { };
        this.breakLine = false;

        // Assign optional parameters based on their presence and type
        args.forEach(arg => {
            if (typeof arg === 'function') {
                this.change = arg;
            } else if (typeof arg === 'boolean') {
                this.breakLine = arg;
            }
        });

        this.init();
    }

    init() {
        if (typeof fieldNamesOverride !== 'undefined') {
            this.ft_fn = fieldNamesOverride;
        } else {
            this.ft_fn = LineAttrib.fieldNames;
        }

        this.ft_container = document.getElementById("ft-container");
        if (this.ft_container) {
            this.setControlValues();
            return;
        }

        this.ft_container = this.newElement("div", "ft_container", "ft-container");
        this.ft_controls = this.newElement("div", "ft_controls", "ft-controls");

        this.ft_font = this.newElement("select", "ft_font", "ft-font", this.ft_fn.font)
        this.ft_fontSize = this.newElement("select", "ft_fontSize", "ft-font-size", this.ft_fn.fontSize)

        this.ft_colour = this.newElement("input", "ft_colour", "SpectrumColourEmpty", this.ft_fn.colour);
        this.ft_colour.setAttribute("data-class", "fa-solid fa-font");
        this.ft_colour.type = "text";

        this.ft_bg_colour = this.newElement("input", "ft_bg_colour", "SpectrumColourEmpty", this.ft_fn.bg_colour);
        this.ft_bg_colour.setAttribute("data-class", "fa-solid fa-paint-roller");
        this.ft_bg_colour.type = "text";

        this.ft_bold = this.newElement("span", "ft_bold", ["ft-bold", "ft-toggle"], this.ft_fn.bold);
        this.ft_italic = this.newElement("span", "ft_italic", ["ft-italic", "ft-toggle"], this.ft_fn.italic);
        this.ft_underline = this.newElement("span", "ft_underline", ["ft-underline", "ft-toggle"], this.ft_fn.underline);

        this.ft_bold.innerHTML = this.ft_fn.boldChar;
        this.ft_italic.innerHTML = this.ft_fn.italicChar;
        this.ft_underline.innerHTML = this.ft_fn.underlineChar;

        this.ft_align_left = this.newElement("i", "ft_align_left", ["ft-align-left", "ft-choose", "fa-solid", "fa-align-left"], this.ft_fn.left);
        this.ft_align_center = this.newElement("i", "ft_align_center", ["ft-align-center", "ft-choose", "fa-solid", "fa-align-center"], this.ft_fn.center);
        this.ft_align_right = this.newElement("i", "ft_align_right", ["ft-align-right", "ft-choose", "fa-solid", "fa-align-right"], this.ft_fn.right);

        // add the container to the document        
        this.orgElement.parentNode.insertBefore(this.ft_container, this.orgElement);

        // find the position of the org element and its parent (the label)
        this.ft_container.style.left = this.orgElement.getBoundingClientRect().left + "px";
        this.ft_container.style.top = this.orgElement.getBoundingClientRect().top + "px";

        // now add the controls to the container 
        this.ft_controls.appendChild(this.ft_font);
        this.ft_controls.appendChild(this.ft_fontSize);

        // Create a new line element and append it to ft_controls
        if (this.breakLine) {
            let newline = document.createElement("br");
            this.ft_controls.appendChild(newline);
        }

        this.ft_controls.appendChild(this.ft_colour);
        this.ft_controls.appendChild(this.ft_bg_colour);
        this.ft_controls.appendChild(this.ft_bold);
        this.ft_controls.appendChild(this.ft_italic);
        this.ft_controls.appendChild(this.ft_underline);
        this.ft_controls.appendChild(this.ft_align_left);
        this.ft_controls.appendChild(this.ft_align_center);
        this.ft_controls.appendChild(this.ft_align_right);

        this.ft_container.appendChild(this.ft_controls);

        this.ft_container.style.display = "inline-block";
        this.orgElement.style.display = "none";
        this.ft_controls.style.display = "inline-block";

        // Copy the available fonts from the suppied list
        for (let i = 0; i < this.orgFontSelect.options.length; i++) {
            let option = document.createElement("option");
            option.value = this.orgFontSelect.options[i].value;
            option.text = this.orgFontSelect.options[i].text;
            this.ft_font.appendChild(option);
        }

        // Add font sizes 10 to 30
        for (let i = 10; i <= 30; i++) {
            let option = document.createElement("option");
            option.value = i + 'pt';
            option.text = i + 'pt';
            this.ft_fontSize.appendChild(option);
        }

        // add the click / change events
        this.ft_bold.addEventListener('click', this.toggleOnOff.bind(this));
        this.ft_italic.addEventListener('click', this.toggleOnOff.bind(this));
        this.ft_underline.addEventListener('click', this.toggleOnOff.bind(this));

        this.ft_align_left.addEventListener('click', this.toggleOne.bind(this));
        this.ft_align_center.addEventListener('click', this.toggleOne.bind(this));
        this.ft_align_right.addEventListener('click', this.toggleOne.bind(this));

        this.ft_font.addEventListener('change', () => {
            this.updateJSON.bind(this)
            this.change();
        });
        this.ft_fontSize.addEventListener('change', () => {
            this.updateJSON.bind(this)
            this.change();
        });

        $(this.ft_colour).on('change.spectrum', (e, colour) => {
            this.updateJSON(e);
            this.change();
        });

        $(this.ft_bg_colour).on('change.spectrum', (e, colour) => {
            this.updateJSON(e);
            this.change();
        });

        // Prevent the font dropdowns from doing anything else obnoxious
        this.ft_font.addEventListener('click', function (event) {
            event.stopPropagation();
        });
        this.ft_fontSize.addEventListener('click', function (event) {
            event.stopPropagation();
        });
    }

    newElement(elType, elId, elClass, elTitle = '') {
        let o = document.createElement(elType);
        //o.setAttribute("id", elId + this.idSuffix);
        o.setAttribute("id", this.orgElement.id + "_" + elId);
        o.title = elTitle;
        o.tabIndex = -1; // remove from tab-sequence

        if (Array.isArray(elClass)) {
            for (let i = 0; i < elClass.length; i++) {
                o.classList.add(elClass[i]);
            }
        } else {
            o.classList.add(elClass);
        }
        return o;
    }

    getValue() {
        var Align = "Left";
        var colour;
        var bg_colour;

        // if all align fields are visible then get the actual one that is set, else default to Left
        if (this.isVisible(this.ft_align_left) ||
            this.isVisible(this.ft_align_center) ||
            this.isVisible(this.ft_align_right)) {

            if (this.ft_align_left.classList.contains("ft-set-on"))
                Align = "Left";
            if (this.ft_align_center.classList.contains("ft-set-on"))
                Align = "Center";
            if (this.ft_align_right.classList.contains("ft-set-on"))
                Align = "Right";
        }

        return {
            font: this.ft_font.value,
            fontSize: this.ft_fontSize.value,
            bold: this.ft_bold.classList.contains("ft-set-on"),
            italic: this.ft_italic.classList.contains("ft-set-on"),
            underline: this.ft_underline.classList.contains("ft-set-on"),
            colour: spGetColour(this.ft_colour.id),
            bg_colour: spGetColour(this.ft_bg_colour.id),
            align: Align
        };
    }

    setValue(data) {
        if (!data)
            return;

        data = JSON.parse(data);

        // set the font and font size
        this.selectOption(this.ft_font, data.font);
        this.selectOption(this.ft_fontSize, data.fontSize);

        // set the colour
        //this.ft_colour.value = data.colour || "";
        //this.ft_bg_colour.value = data.bg_colour || "";

        spSetColour(this.ft_colour,data.colour || "");
        spSetColour(this.ft_bg_colour, data.bg_colour || "");

        // set the bold, italic, and underline toggles
        if (data.bold) {
            this.ft_bold.classList.add("ft-set-on");
        } else {
            this.ft_bold.classList.remove("ft-set-on");
        }

        if (data.italic) {
            this.ft_italic.classList.add("ft-set-on");
        } else {
            this.ft_italic.classList.remove("ft-set-on");
        }

        if (data.underline) {
            this.ft_underline.classList.add("ft-set-on");
        } else {
            this.ft_underline.classList.remove("ft-set-on");
        }

        // set the align toggle
        this.ft_align_left.classList.remove("ft-set-on");
        this.ft_align_center.classList.remove("ft-set-on");
        this.ft_align_right.classList.remove("ft-set-on");

        switch (data.align.toLowerCase()) {
            case "left":
                this.ft_align_left.classList.add("ft-set-on");
                break;
            case "center":
                this.ft_align_center.classList.add("ft-set-on");
                break;
            case "right":
                this.ft_align_right.classList.add("ft-set-on");
                break;
        }
        this.updateJSON();
    }

    // Helper function to select an option
    selectOption(selectElement, valueToSelect) {
        Array.from(selectElement.options).forEach((option) => {
            option.selected = option.value === valueToSelect;
        });
    }

    // update JSON in the textbox
    updateJSON(event) {
        this.orgElement.value = JSON.stringify(this.getValue());
    }
    toggleOnOff(event) {
        $(event.target).toggleClass("ft-set-on");
        this.updateJSON(event);
        this.change();
    }
    toggleOne(event) {
        $(this.ft_align_left).removeClass("ft-set-on");
        $(this.ft_align_center).removeClass("ft-set-on");
        $(this.ft_align_right).removeClass("ft-set-on");
        $(event.target).addClass("ft-set-on");
        this.updateJSON(event);
        this.change();
    }

    setControlVisibility(control, visibility) {
        // Determine which element's visibility is being set
        let element = "";
        switch (control.toLowerCase()) {
            case "font":
                element = this.ft_font;
                break;
            case "fontsize":
                element = this.ft_fontSize;
                break;
            case "colour":
                if (visibility)
                    spShowControl(this.ft_colour);
                else
                    spHideControl(this.ft_colour);
                break;
            case "bg_colour":
                if (visibility)
                    spShowControl(this.ft_bg_colour);
                else
                    spHideControl(this.ft_bg_colour);
                break;
            case "bold":
                element = this.ft_bold;
                break;
            case "italic":
                element = this.ft_italic;
                break;
            case "underline":
                element = this.ft_underline;
                break;
            case "left":
                element = this.ft_align_left;
                break;
            case "center":
                element = this.ft_align_center;
                break;
            case "right":
                element = this.ft_align_right;
                break;
        }

        // Toggle the "hidden" class based on the visibility parameter

        if (element != "") {
            if (visibility) {
                element.classList.remove("hidden");
            } else {
                element.classList.add("hidden");
            }
        }
    }

    isVisible(element) {
        return !element.classList.contains("hidden");
    }

}
