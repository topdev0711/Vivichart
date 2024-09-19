// This common function is called both when showing the PDF in an iframe and when downloading a PDF file.
// The options include:
//   "pageSize", either "A4" or "LETTER" (the default)
//   "layout", either "portrait" (the default) or "landscape"
//   "margin" for the uniform page margin on each page (default is 36 pt)
//   "padding" instead of the Diagram.padding when adjusting the Diagram.documentBounds for the area to render
//   "imgWidth", size of diagram image for one page; defaults to the page width minus margins
//   "imgHeight", size of diagram image for one page; defaults to the page height minus margins
//   "imgResolutionFactor" for how large the image should be scaled when rendered for each page;
//     larger is better but significantly increases memory usage (default is 3)
//   "parts", "background", "showTemporary", "showGrid", all are passed to Diagram.makeImageData
setTimeout(() => {
    var MGLB = window.MGLB;
}, 9000)

function generatePdf(action, diagram, options)
{
    if (!(diagram instanceof go.Diagram)) throw new Error("No Diagram provided when calling generatePdf.");
    if (!options) options = {};

    // Step 1: Get the diagram dimensions
    var bounds = diagram.documentBounds;
    var diagramWidth = bounds.width;
    var diagramHeight = bounds.height;

    console.log("Diagram dimensions (width x height):", diagramWidth, "x", diagramHeight);

    // Step 2: Determine the layout based on the dimensions with precise floating-point comparison
    var layout;

    if (options.layout)
    {
        layout = options.layout.toLowerCase();
    } else
    {
        // Use a very small epsilon value for floating-point comparison
        const epsilon = Number.EPSILON;

        // Determine if the height is greater than or nearly equal to the width
        if (diagramHeight > diagramWidth || Math.abs(diagramHeight - diagramWidth) < epsilon)
        {
            layout = "portrait";
        } else
        {
            layout = "landscape";
        }
    }

    // Ensure the layout is either "portrait" or "landscape"
    if (layout !== "portrait" && layout !== "landscape") throw new Error("Unknown layout: " + layout);

    console.log("Selected layout:", layout);

    // Step 3: Set the page size and orientation
    var pageSize = options.pageSize || "LETTER";
    pageSize = pageSize.toUpperCase();
    if (pageSize !== "LETTER" && pageSize !== "A4") throw new Error("Unknown page size: " + pageSize);

    var pageWidth = (pageSize === "LETTER" ? 612 : 595.28) * 96 / 72;
    var pageHeight = (pageSize === "LETTER" ? 792 : 841.89) * 96 / 72;

    console.log("Page size before orientation adjustment (width x height):", pageWidth, "x", pageHeight);

    // Adjust dimensions for landscape mode
    if (layout === "landscape")
    {
        var temp = pageWidth;
        pageWidth = pageHeight;
        pageHeight = temp;
    }

    console.log("Page size after orientation adjustment (width x height):", pageWidth, "x", pageHeight);

    var margin = options.margin !== undefined ? options.margin : 36;
    var padding = options.padding !== undefined ? options.padding : diagram.padding;

    var imgWidth = options.imgWidth !== undefined ? options.imgWidth : (pageWidth - (margin / 72) * 96 * 2);
    var imgHeight = options.imgHeight !== undefined ? options.imgHeight : (pageHeight - (margin / 72) * 96 * 2);
    var imgResolutionFactor = options.imgResolutionFactor !== undefined ? options.imgResolutionFactor : 2;

    console.log("Image dimensions for PDF (width x height):", imgWidth, "x", imgHeight);

    var pageOptions = {
        size: pageSize,
        margin: margin,
        layout: layout
    };

    require(["blob-stream", "pdfkit"], (blobStream, PDFDocument) =>
    {
        var doc = new PDFDocument(pageOptions);
        var stream = doc.pipe(blobStream());

        // Step 4: Determine background settings (keep original position)
        let backgroundImage = '../Backgrounds/' + MGLB.background;
        doc.rect(0, 0, doc.page.width, doc.page.height).fill(rgbaToHexWithoutAlpha(MGLB.backgroundContent));

        const handleBackground = () =>
        {
            switch (MGLB.backgroundType)
            {
                case "SolidColour":
                    return Promise.resolve();
                case "Gradient":
                    var gradientObject = parseGradient(MGLB.gradient);
                    applyGradientToPdf(doc, gradientObject, doc.page.width, doc.page.height);
                    return Promise.resolve();
                case "Image":
                    return loadImageData(backgroundImage)
                        .then(data =>
                        {
                            switch (MGLB.backgroundRepeat)
                            {
                                case "01": // no repeat
                                    doc.image(data.base64data, 0, 0, { width: data.width, height: data.height });
                                    break;
                                case "02": // fit to screen
                                    doc.image(data.base64data, 0, 0, { width: doc.page.width, height: doc.page.height });
                                    break;
                                case "03": // repeat
                                    const repeatX = Math.ceil(doc.page.width / data.width);
                                    const repeatY = Math.ceil(doc.page.height / data.height);
                                    for (let x = 0; x < repeatX; x++)
                                    {
                                        for (let y = 0; y < repeatY; y++)
                                        {
                                            doc.image(data.base64data, x * data.width, y * data.height, { width: data.width, height: data.height });
                                        }
                                    }
                                    break;
                                case "04": // repeat across
                                    const repeatXAcross = Math.ceil(doc.page.width / data.width);
                                    for (let x = 0; x < repeatXAcross; x++)
                                    {
                                        doc.image(data.base64data, x * data.width, 0, { width: data.width, height: data.height });
                                    }
                                    break;
                                case "05": // repeat down
                                    const repeatYDown = Math.ceil(doc.page.height / data.height);
                                    for (let y = 0; y < repeatYDown; y++)
                                    {
                                        doc.image(data.base64data, 0, y * data.height, { width: data.width, height: data.height });
                                    }
                                    break;
                                case "06": // fit across (100% width, auto height)
                                    const newHeight = (data.height / data.width) * doc.page.width;
                                    doc.image(data.base64data, 0, 0, { width: doc.page.width, height: newHeight });
                                    break;
                            }
                        });
            }
        };

        handleBackground().then(() =>
        {
            // Add the title box and title text here
            const titleBoxHeight = 50; 
            doc.rect(margin, margin, doc.page.width - margin * 2, titleBoxHeight).fill('#FFFFFF');
            doc.font('Helvetica-Bold').fontSize(20);
        
            const title = "Rustom"; // Change this to the title you want
        
            const titleWidth = doc.widthOfString(title);
            const titleX = (doc.page.width - titleWidth) / 2; // Center the title horizontally
            const titleY = margin + (titleBoxHeight - doc.currentLineHeight()) / 2;
        
            doc.fillColor('black').text(title, titleX, titleY);
        
            // Move the diagram image down by the height of the title box plus additional space
            var db = diagram.documentBounds.copy().subtractMargin(diagram.padding).addMargin(padding);
        
            const additionalSpace = 10; // Define additional space in points
            var imgYPosition = titleBoxHeight + margin + additionalSpace; // Add extra space
        
            var imgdata = diagram.makeImageData({
                scale: imgResolutionFactor,
                position: db.position,
                size: new go.Size(db.width * imgResolutionFactor, db.height * imgResolutionFactor),
                maxSize: new go.Size(Infinity, Infinity)
            });
        
            console.log("Rendering image to PDF...");
        
            doc.image(imgdata, margin, imgYPosition, {
                scale: 1 / (imgResolutionFactor * 96 / 72 * Math.max(1, Math.max(db.width / imgWidth, db.height / imgHeight)))
            });
            doc.end();
        });

        stream.on('finish', () =>
        {
            console.log("PDF generation completed.");
            action(stream.toBlob('application/pdf'))
        });
    });
}

// Usage with dynamic orientation:
var pdfOptions = {
    showTemporary: true,
    pageSize: "A4" // Only the page size is specified, orientation will be determined dynamically
};

function showPdf()
{
    generatePdf(blob =>
    {
        var datauri = window.URL.createObjectURL(blob);
        var frame = document.getElementById("myFrame");
        if (frame)
        {
            frame.style.display = "block";
            frame.src = datauri; // doesn't work in IE 11, but works everywhere else
            setTimeout(() => window.URL.revokeObjectURL(datauri), 1);
        }
    }, myDiagram, pdfOptions);
}

function generatePdfPromise(diagram, pdfOptions) {
    // Generate the PDF after the layout is completed
    return new Promise((resolve, reject) => {
    generatePdf(blob => {
        var datauri = window.URL.createObjectURL(blob);
        var a = document.createElement("a");
        a.style = "display: none";
        a.href = datauri;
        a.download = MGLB.oca_text + '  '+ MGLB.chart_date + '.pdf';

        document.body.appendChild(a);
        requestAnimationFrame(() => {
            a.click();
            window.URL.revokeObjectURL(datauri);
            document.body.removeChild(a);
                resolve(); // Resolve the promise after PDF generation
        });
        }, diagram, pdfOptions);
    });
}
async function downloadPdf()
{
    let button_position = MGLB.button_position;
    toggleButtons('none');
    
    // Wait for the PDF generation to complete
    await generatePdfPromise(myDiagram, pdfOptions);
    toggleButtons(button_position);
}

function loadImageData(filename) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = "anonymous";  // This might be needed if fetching images from a different domain

        img.onload = function () {

            const imgWidth = img.width;
            const imgHeight = img.height;

            const canvas = document.createElement('canvas');
            canvas.width = imgWidth;
            canvas.height = imgHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0);

            const base64data = canvas.toDataURL();

            // Explicitly clear references for potential garbage collection
            canvas.width = 0;
            canvas.height = 0;

            resolve({
                base64data: base64data,
                width: imgWidth,
                height: imgHeight
            });
        };

        img.onerror = function () {
            reject(new Error("There was an error loading the image."));
        };

        // Set the src after the handlers to ensure they are in place before the image starts loading
        img.src = filename;
    });
}

function applyGradientToPdf(doc, gradientObj, width, height) {
    let gradient;
    if (gradientObj.type === 'linear') {
        // Convert CSS gradient angle to PDFKit angle

        // css - pdf
        //   0 - 270    bottom to top
        //  45 - 315    bottom left to top right
        //  90 -   0    left to right
        // 135 -  45    top left to bottom right
        // 180 -  90    top to bottom
        // 225 - 135    top right to bottom left
        // 270 - 180    right to left
        // 315 - 225    bottom right to top left

        const pdfKitAngle = (gradientObj.angle - 90) % 360;

        const angleRad = (pdfKitAngle * Math.PI) / 180;

        const x1 = width / 2 - Math.cos(angleRad) * width / 2;
        const y1 = height / 2 - Math.sin(angleRad) * height / 2;
        const x2 = width / 2 + Math.cos(angleRad) * width / 2;
        const y2 = height / 2 + Math.sin(angleRad) * height / 2;

        gradient = doc.linearGradient(x1, y1, x2, y2);
    } else {
        const cx = width / 2;
        const cy = height / 2;
        const r1 = 0;
        const r2 = Math.min(width, height) / 2;
        gradient = doc.radialGradient(cx, cy, r1, cx, cy, r2);
    }
    gradientObj.color_list.forEach(colorStop => {
        gradient.stop(colorStop.color_stop / 100.01, rgbaToHexWithoutAlpha(colorStop.color));
    });
    doc.rect(0, 0, width, height).fill(gradient);
}

function parseGradient(gString) {

    const gradient_object = {
        type: '',
        angle: null,
        color_list: []
    }

    if (gString != '') {
        const gradient = JSON.parse(gString)
        gradient_object.type = gradient.gradientDetails.gradientType
        gradient_object.angle = gradient.gradientDetails.angle
        gradient.gradientDetails.colors.forEach((item, index) => {
            gradient_object.color_list.push({ color: `rgba(${item.red}, ${item.green}, ${item.blue}, ${item.alpha})`, color_stop: item.position })
    })
    }

    return gradient_object
}

function rgbaToHexWithoutAlpha(rgba) {
    const v = rgba.substring(rgba.indexOf('(') + 1, rgba.indexOf(')')).split(',').map(value => parseFloat(value.trim()));
    if (v[3] == undefined) v[3] = 1
    // Assuming white background
    const whiteColor = [255, 255, 255];
    // Calculate the blended color without alpha
    const blendedColor = [
        Math.round((1 - v[3]) * whiteColor[0] + v[3] * v[0]),
        Math.round((1 - v[3]) * whiteColor[1] + v[3] * v[1]),
        Math.round((1 - v[3]) * whiteColor[2] + v[3] * v[2])
    ];
    // Convert to hex
    const hexValue = "#" + blendedColor.map(component => component.toString(16).padStart(2, '0')).join('');
    return hexValue.toUpperCase();
}