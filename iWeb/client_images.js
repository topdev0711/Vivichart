"use strict";

let image_workspace;
let cropper_controls_container;
let cropperObject = {};
let currentCropShape;
let rotation = 0;
let flipX = 1;
let flipY = 1;
let options = {}
let square_btn = false
let circle_btn = false
let prev_image_data = '' // hold the previous image data to use in case of uploading new image and then cancel, 
let img_type = null

const allowdImageSizeInMB = 1 // MB for 500kb use 0.5

//$(document).ready(function () {
$(function () {

    image_workspace = $QS('.image-workspace img');

    cropper_controls_container = $QS('.cropper-controls');

    // square and circle cropping buttons
    $ID('btnCropSquare').addEventListener('click', async function () {
        handleCropperMode('square')
        $ID("image-preview").classList.remove("circular-cropper")
        currentCropShape = "rectangle"
        renderCropper()
        setTimeout(() => {
            cropperObject.cropper.scaleY(flipY);
            cropperObject.cropper.scaleX(flipX);
            cropperObject.cropper.rotate(rotation);
        })
    });
    $ID('btnCropCircle').addEventListener('click', async function () {
        handleCropperMode('circle')
        renderCropper()
        setTimeout(() => {
            $ID("image-preview").classList.add("circular-cropper")
            currentCropShape = "circle"
            cropperObject.cropper_face.classList.add('circular-cropper')
            cropperObject.cropper_view_box.classList.add('circular-cropper')
            cropperObject.cropper.scaleY(flipY);
            cropperObject.cropper.scaleX(flipX);
            cropperObject.cropper.rotate(rotation);
        }, 10);
    });

    // rotate image left / right
    $ID('btnRotateLeft').addEventListener('click', async function () {
        rotation = (rotation - 90) % 360;
        cropperObject.cropper.rotate(-90);
        show_hide_icons('show')
    });
    $ID('btnRotateRight').addEventListener('click', async function () {
        rotation = (rotation + 90) % 360;
        cropperObject.cropper.rotate(90);
        show_hide_icons('show')
    });

    // flip image horizontally / vertically
    $ID('btnFlipHorizontal').addEventListener('click', async function () {
        if (Math.abs(rotation) % 180 == 0) {
            flipX = -flipX;
            cropperObject.cropper.scaleX(flipX);
        } else {
            flipY = -flipY;
            cropperObject.cropper.scaleY(flipY);
        }
        show_hide_icons('show')
    });
    $ID('btnFlipVertical').addEventListener('click', async function () {
        if (Math.abs(rotation) % 180 == 0) {
            flipY = -flipY;
            cropperObject.cropper.scaleY(flipY);
        } else {
            flipX = -flipX;
            cropperObject.cropper.scaleX(flipX);
        }
        show_hide_icons('show')
    });

    // when dragging image over the area
    $ID('modal-body').ondragover = (e) => {
        e.preventDefault();
    }

    // upload image on drop
    $ID('modal-body').ondrop = (e) => {
        e.preventDefault();
        var file = e.dataTransfer.files[0];
        newImageAdded(file)
    }

    // save the croped image
    $ID("btnImageSave").addEventListener('click', async function (e) {
        e.preventDefault();
        let base64 = null
        if (currentCropShape === 'circle') {
            base64 = getCropperImage('image/png')
            base64 = await cropToCircle(base64)
        } else {
            base64 = getCropperImage(img_type)
        }
        $ID('hdnImage').value = base64

        // we now cause a postback to save the image
        $ID("btnSavePostback").click();
    });

    // Cancel edit
    $ID("btnImageCancel").addEventListener('click', function (e) {
        e.preventDefault();
        // $ID('divEditMode').style.display = "none"
        show_hide_icons('hide')
        currentCropShape;
        rotation = 0;
        flipX = 1;
        flipY = 1;
        square_btn = false
        circle_btn = false
        options.autoCrop = false
        $('.modal-footer:first-child').hide();
        $ID('btnCropSquare').classList.remove('active')
        $ID('btnCropCircle').classList.remove('active')
        $ID("image-preview-container").classList.remove("show")
        $ID('hdnImage').value = prev_image_data
        renderCropper()
        !prev_image_data && $('.cropper-container').hide()
    });

    // upload image on click button
    $ID("btnImageUpload").addEventListener('click', function () {
        $ID("btnHiddenUpload").click();
    });

    // common function for handeling selected file 
    $ID("btnHiddenUpload").onchange = () => {
        var file = $ID("btnHiddenUpload").files[0];
        newImageAdded(file)
    }

    $ID('image-crop-area').ondragover = () => {
        $ID('custom-drop-area').classList.add('visible')
    }  

    $ID('image-crop-area').ondragleave = () => {
        $ID('custom-drop-area').classList.remove('visible');
    }
    
    $ID('image-crop-area').ondrop = () => {
        $ID('custom-drop-area').classList.remove('visible');
    }

    prev_image_data = $ID('hdnImage').value
    show_hide_icons('hide')
    renderCropper()

    $ID('hdnIsNewImage').value = false
});

// cropper options
options = {
    dragMode: 'move',
    // this is the aspect ratio for fixed selector,
    // remove this to make the crop selector freely dragable
    aspectRatio: NaN,
    autoCrop: false,
    viewMode: 0,
    // turn modal: false for removing black modal arround the cropper selector
    modal: true,
    background: false,
    ready: function () {
        // when ready get the cropper selector
        cropperObject.cropper_view_box = $QS('.cropper-view-box')
        cropperObject.cropper_face = $QS('.cropper-face')
        //console.log('ready');
    },
    crop: function (event) {
        var canvas = this.cropper.getCroppedCanvas();
        $ID('image-preview').src = canvas.toDataURL(img_type);
    }
}

function handleCropperMode(btn_type){
    show_hide_icons('show')
    if(btn_type === 'square'){
        if(square_btn){
            $ID('btnCropSquare').classList.remove('active')
            // $ID('divEditMode').style.display = "none"
            $ID("image-preview-container").classList.remove("show")
            options.autoCrop = false
        }else{
            $ID('btnCropSquare').classList.add('active')
            // $ID('divEditMode').style.display = "flex"
            $ID("image-preview-container").classList.add("show")
            options.autoCrop = true
            options.aspectRatio = NaN
            circle_btn = false
        }
        $ID('btnCropCircle').classList.remove('active')
        square_btn = !square_btn
    }else{
        if(circle_btn){
            $ID('btnCropCircle').classList.remove('active')
            // $ID('divEditMode').style.display = "none"
            $ID("image-preview-container").classList.remove("show")
            options.autoCrop = false
        }else{
            $ID('btnCropCircle').classList.add('active')
            // $ID('divEditMode').style.display = "flex"
            $ID("image-preview-container").classList.add("show")
            options.autoCrop = true
            options.aspectRatio = 1 / 1
            square_btn = false
        }
        $ID('btnCropSquare').classList.remove('active')
        circle_btn = !circle_btn
    }
}

function show_hide_icons(state){
    const icons = document.querySelector('#divAccButtons')
    const datasources = document.querySelector('#divDatasources')
    const list = document.querySelector('#divList')
    if(state === 'hide'){
        icons.style.display = 'none'
        datasources.classList.remove('noClick')
        Array.from(list.parentElement.children).slice(1).forEach(el => el.classList.remove('noClick'))
        BannerEnabled(true);
    }else{
        datasources.classList.add('noClick')
        Array.from(list.parentElement.children).slice(1).forEach(el => el.classList.add('noClick'))
        icons.style.display = 'block'
        BannerEnabled(false);
    }
}

// function startEditImage() {
//     renderCropper();
//     return false;
// }

// get image from the croped canvas
function getCropperImage(type) {
    return cropperObject.cropper.getCroppedCanvas().toDataURL(type);
}

// If we have an image, show it
// function renderImage() {
//     if ($ID('hdnImage').value) {
//         $ID('divDisplayMode').style.display = "flex"
//         $ID('divEditMode').style.display = "none"

//         $ID('image-crop-area').style.display = "none"
//         $ID('image-drop-area').style.display = 'none'
//         $ID('image-display-area').style.display = "flex"
//         cropper_controls_container.style.display = 'none'
//         $ID('image-display').src = $ID('hdnImage').value
//         cropperObject.cropper && cropperObject.cropper.destroy()
//     } else {
//         $ID('image-drop-area').style.display = 'flex'
//         renderCropper();
//     }
// }

// render cropper if hidden field have string
// if not then upload image
function renderCropper() {

    $ID("image-preview").classList.remove("circular-cropper")

    // $ID('btnCropCircle').classList.remove('active')
    // $ID('btnCropSquare').classList.add('active')
    $ID('image-drop-area').style.display = 'flex'

    currentCropShape = "rectangle";

    // check for image string
    if ($ID('hdnImage').value) {
        // $ID('divDisplayMode').style.display = "none"
        // $ID('divEditMode').style.display = "flex"

        img_type = $ID('hdnImage').value.match(/^data:(image\/[a-zA-Z+]+);base64,/)[1]
        $('.modal-body').removeClass("noImagePreview");
        cropper_controls_container.style.display = 'flex'
        $ID('image-crop-area').style.display = "flex"
        $ID('image-display-area').style.display = "none"

        image_workspace.src = $ID('hdnImage').value
        cropperObject.cropper && cropperObject.cropper.destroy()
        const cropper = new Cropper(image_workspace, options)
        cropperObject.cropper = cropper
    } else {
        $ID("btnImageSave").disabled = true
        $ID("btnImageSave").style.cursor = 'not-allowed'
        $('.modal-body').addClass("noImagePreview");
    }
}

// add image to hidden field and proceed for cropping
//function newImageAdded(file) {
//    show_hide_icons('show')
//    var fileSizeInBytes = file.size;
//    var fileSizeInMB = fileSizeInBytes / (1024 * 1024)
//    if (fileSizeInMB > allowdImageSizeInMB) {
//        alert('Images having size greater then 1MB are not allowed.')
//        return true
//    }
//    // anable crop button and controls after image upload
//    cropper_controls_container.style.display = 'flex'
//    $ID("btnImageSave").disabled = false
//    $ID("btnImageSave").style.cursor = 'pointer'
//    // save base64 in hidden field
//    fileToBase64(file)
//        .then((base64Data) => {
//            $ID('hdnImage').value = `data:image/jpeg;base64,${base64Data}`
//        })
//    const url = window.URL.createObjectURL(new Blob([file], { type: 'image/jpg' }))
//    image_workspace.src = url
//    cropperObject.cropper && cropperObject.cropper.destroy()
//    const cropper = new Cropper(image_workspace, options)
//    cropperObject.cropper = cropper
//}

//// convert uploaded image to base64 in case of user new file uploading
//function fileToBase64(file) {
//    return new Promise((resolve, reject) => {
//        const reader = new FileReader();

//        reader.onload = () => {
//            const base64Data = reader.result.split(',')[1];
//            resolve(base64Data);
//        };

//        reader.onerror = (error) => {
//            reject(error);
//        };

//        reader.readAsDataURL(file);
//    });
//}

function newImageAdded(file) {
    show_hide_icons('show')
    var fileSizeInBytes = file.size;
    var fileSizeInMB = fileSizeInBytes / (1024 * 1024)
    if (fileSizeInMB > allowdImageSizeInMB) {
        alert('Images having size greater then 1MB are not allowed.')
        return true
    }
    // anable crop button and controls after image upload
    cropper_controls_container.style.display = 'flex'
    $ID("btnImageSave").disabled = false
    $ID("btnImageSave").style.cursor = 'pointer'
    img_type = file.type
    // save base64 in hidden field
    fileToBase64(file)
        .then((base64Data) => {
            $ID('hdnImage').value = `data:${file.type};base64,${base64Data}`
            $ID('hdnIsNewImage').value = true
        })
    const url = window.URL.createObjectURL(new Blob([file], { type: file.type }))
    image_workspace.src = url
    cropperObject.cropper && cropperObject.cropper.destroy()
    const cropper = new Cropper(image_workspace, options)
    cropperObject.cropper = cropper
    $('.modal-body').removeClass("noImagePreview");
}

// convert croped rectangle image to circle image 
function cropToCircle(img) {
    return new Promise((resolve) => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');

        // Create an image element
        const image = new Image();
        image.src = img; // Replace with the URL of your image

        // Wait for the image to load
        image.onload = function () {
            // Set the canvas size to match the image size
            canvas.width = image.width;
            canvas.height = image.height;

            // Clear the canvas
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            // Calculate the clipping parameters based on aspect ratio
            const scaleX = canvas.width / image.width;
            const scaleY = canvas.height / image.height;

            // Draw the oval image
            ctx.save();
            ctx.beginPath();
            ctx.ellipse(canvas.width / 2, canvas.height / 2, canvas.width / 2, canvas.height / 2, 0, 0, 2 * Math.PI);
            ctx.closePath();
            ctx.clip();

            ctx.drawImage(image, 0, 0, canvas.width, canvas.height);

            ctx.restore();

            // Get the base64-encoded image data
            const base64 = canvas.toDataURL('image/png');
            resolve(base64)
        };
    })
};

// convert uploaded image to base64 in case of user new file uploading
function fileToBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();

        reader.onload = () => {
            const base64Data = reader.result.split(',')[1];
            resolve(base64Data);
        };

        reader.onerror = (error) => {
            reject(error);
        };

        reader.readAsDataURL(file);
    });
}

//function searchFilter(e) {
//    const query = e.target.value
//    const tr = document.querySelectorAll('#divList > div > table > tbody > tr')
//    tr.forEach((item) => {
//        if (item.innerText.replace(/\n/g, ' ').trim().toLowerCase().includes(query.trim().toLowerCase())) {
//            item.style.display = 'table-row'
//        } else {
//            item.style.display = 'none'
//        }
//    })
//}