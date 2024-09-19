class Editor {

    selector;
    font_family_formats;
    font_sizes;
    default_style;
    originalWidth = 0;
    originalHeight = 0;
    originalX = 0;
    originalY = 0;
    isResizing = false;
    commands = []
    test = {}

    init(o) {
        this.selector = document.querySelector(o.selector)
        this.height = o.height || 300
        this.font_sizes = o.font_sizes
        this.font_family_formats = o.font_family_formats
        this.default_style = { ...o.default_style }
        this.genFuncs.addCssFile('editor.css')
        this.addMarkup()
    }

    get() {
        return {
            html: $('.text').html().trim(),
            width: $('.text').width()
        }
    }

    addMarkup() {
        $(this.selector).css('display', 'none')
        $(this.selector).after(
            '<div class="ed-container">' + 
                '<div class="ed-header">' + 
                    '<div class="ed-header-group">' + 
                        '<select class="dd-font-family" title="Font Style"></select>' + 
                        '<select class="dd-font-size" title="Font Size"></select>' + 
                    '</div>' + 
                    '<div class="ed-header-group">' + 
                        '<span class="ed-toggle toggleBold" title="Bold">B</span>' + 
                        '<span class="ed-toggle toggleItalic" title="Italic">I</span>' + 
                        '<span class="ed-toggle toggleUnderline" title="Underline">U</span>' + 
                    '</div>' + 
                    '<div class="ed-header-group">' + 
                        '<input type="text" title="Text Foreground Colour" class="SpectrumEditorText" value="rgba(0, 0, 0, 1)" data-class="fa-solid fa-font">' +
                        '<input type="text" title="Text Background Colour" class="SpectrumEditorBg" value="rgba(255, 255, 255, 0)" data-class="fa-solid fa-paint-roller">' +
                    '</div>' + 
                    '<div class="ed-header-group">' + 
                        '<span class="ed-toggle more">' + 
                            '<i class="fa-solid fa-ellipsis"></i>' + 
                        '</span>' + 
                    '</div>' + 
                '</div>' + 
                '<div class="ed-lower-header">' + 
                    '<div class="ed-header-group">' + 
                        '<div class="dd-toggle" title="Numbered List">' + 
                            '<i class="fa-solid fa-list-ol toggleNumList"></i>' + 
                            '<i class="fa-solid fa-chevron-down dd-toggle-arrow num"></i>' + 
                            '<ul>' + 
                                '<li data-value="1">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path><path d="M10 17v-4.8l-1.5 1v-1.1l1.6-1h1.2V17h-1.2Zm3.6.1c-.4 0-.7-.3-.7-.7 0-.4.3-.7.7-.7.5 0 .7.3.7.7 0 .4-.2.7-.7.7Zm-5 5.7c0-1.2.8-2 2.1-2s2.1.8 2.1 1.8c0 .7-.3 1.2-1.4 2.2l-1.1 1v.2h2.6v1H8.6v-.9l2-1.9c.8-.8 1-1.1 1-1.5 0-.5-.4-.8-1-.8-.5 0-.9.3-.9.9H8.5Zm6.3 4.3c-.5 0-.7-.3-.7-.7 0-.4.2-.7.7-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7ZM10 34.4v-1h.7c.6 0 1-.3 1-.8 0-.4-.4-.7-1-.7s-1 .3-1 .8H8.6c0-1.1 1-1.8 2.2-1.8 1.3 0 2.1.6 2.1 1.6 0 .7-.4 1.2-1 1.3v.1c.8.1 1.3.7 1.3 1.4 0 1-1 1.9-2.4 1.9-1.3 0-2.2-.8-2.3-2h1.2c0 .6.5 1 1.1 1 .7 0 1-.4 1-1 0-.5-.3-.8-1-.8h-.7Zm4.7 2.7c-.4 0-.7-.3-.7-.7 0-.4.3-.7.7-.7.5 0 .8.3.8.7 0 .4-.3.7-.8.7Z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="a">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path><path d="M10.3 15.2c.5 0 1-.4 1-.9V14h-1c-.5.1-.8.3-.8.6 0 .4.3.6.8.6Zm-.4.9c-1 0-1.5-.6-1.5-1.4 0-.8.6-1.3 1.7-1.4h1.1v-.4c0-.4-.2-.6-.7-.6-.5 0-.8.1-.9.4h-1c0-.8.8-1.4 2-1.4 1.1 0 1.8.6 1.8 1.6V16h-1.1v-.6h-.1c-.2.4-.7.7-1.3.7Zm4.6 0c-.5 0-.7-.3-.7-.7 0-.4.2-.7.7-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7Zm-3.2 10c-.6 0-1.2-.3-1.4-.8v.7H8.5v-6.3H10v2.5c.3-.5.8-.9 1.4-.9 1.2 0 1.9 1 1.9 2.4 0 1.5-.7 2.4-1.9 2.4Zm-.4-3.7c-.7 0-1 .5-1 1.3s.3 1.4 1 1.4c.6 0 1-.6 1-1.4 0-.8-.4-1.3-1-1.3Zm4 3.7c-.5 0-.7-.3-.7-.7 0-.4.2-.7.7-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7Zm-2.2 7h-1.2c0-.5-.4-.8-.9-.8-.6 0-1 .5-1 1.4 0 1 .4 1.4 1 1.4.5 0 .8-.2 1-.7h1c0 1-.8 1.7-2 1.7-1.4 0-2.2-.9-2.2-2.4s.8-2.4 2.2-2.4c1.2 0 2 .7 2 1.7Zm1.8 3c-.5 0-.8-.3-.8-.7 0-.4.3-.7.8-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7Z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="A">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path><path d="m12.6 17-.5-1.4h-2L9.5 17H8.3l2-6H12l2 6h-1.3ZM11 12.3l-.7 2.3h1.6l-.8-2.3Zm4.7 4.8c-.4 0-.7-.3-.7-.7 0-.4.3-.7.7-.7.5 0 .7.3.7.7 0 .4-.2.7-.7.7ZM11.4 27H8.7v-6h2.6c1.2 0 1.9.6 1.9 1.5 0 .6-.5 1.2-1 1.3.7.1 1.3.7 1.3 1.5 0 1-.8 1.7-2 1.7ZM10 22v1.5h1c.6 0 1-.3 1-.8 0-.4-.4-.7-1-.7h-1Zm0 4H11c.7 0 1.1-.3 1.1-.8 0-.6-.4-.9-1.1-.9H10V26Zm5.4 1.1c-.5 0-.8-.3-.8-.7 0-.4.3-.7.8-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7Zm-4.1 10c-1.8 0-2.8-1.1-2.8-3.1s1-3.1 2.8-3.1c1.4 0 2.5.9 2.6 2.2h-1.3c0-.7-.6-1.1-1.3-1.1-1 0-1.6.7-1.6 2s.6 2 1.6 2c.7 0 1.2-.4 1.4-1h1.2c-.1 1.3-1.2 2.2-2.6 2.2Zm4.5 0c-.5 0-.8-.3-.8-.7 0-.4.3-.7.8-.7.4 0 .7.3.7.7 0 .4-.3.7-.7.7Z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="i">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path><path d="M15.1 16v-1.2h1.3V16H15Zm0 10v-1.2h1.3V26H15Zm0 10v-1.2h1.3V36H15Z"></path><path fill-rule="nonzero" d="M12 21h1.5v5H12zM12 31h1.5v5H12zM9 21h1.5v5H9zM9 31h1.5v5H9zM6 31h1.5v5H6zM12 11h1.5v5H12zM12 19h1.5v1H12zM12 29h1.5v1H12zM9 19h1.5v1H9zM9 29h1.5v1H9zM6 29h1.5v1H6zM12 9h1.5v1H12z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="I">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path><path d="M15.1 17v-1.2h1.3V17H15Zm0 10v-1.2h1.3V27H15Zm0 10v-1.2h1.3V37H15Z"></path><path fill-rule="nonzero" d="M12 20h1.5v7H12zM12 30h1.5v7H12zM9 20h1.5v7H9zM9 30h1.5v7H9zM6 30h1.5v7H6zM12 10h1.5v7H12z"></path></g></svg>' + 
                                '</li>' + 
                            '</ul>' + 
                        '</div>' + 
                        '<div class="dd-toggle" title="Bullet List">' + 
                            '<i class="fa-solid fa-list-ul toggleBulletList"></i>' + 
                            '<i class="fa-solid fa-chevron-down dd-toggle-arrow bullet"></i>' + 
                            '<ul>' + 
                                '<li data-value="disc">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><circle cx="11" cy="14" r="3"></circle><circle cx="11" cy="24" r="3"></circle><circle cx="11" cy="34" r="3"></circle><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="circle">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path d="M11 16a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 1 0-6 3 3 0 0 1 0 6ZM11 26a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 1 0-6 3 3 0 0 1 0 6ZM11 36a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 1 0-6 3 3 0 0 1 0 6Z" fill-rule="nonzero"></path><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path></g></svg>' + 
                                '</li>' + 
                                '<li data-value="square">' + 
                                    '<svg width="48" height="48" focusable="false"><g fill-rule="evenodd"><path d="M8 11h6v6H8zM8 21h6v6H8zM8 31h6v6H8z"></path><path opacity=".2" d="M18 12h22v4H18zM18 22h22v4H18zM18 32h22v4H18z"></path></g></svg>' + 
                                '</li>' + 
                            '</ul>' + 
                        '</div>' + 
                    '</div>' + 
                    '<div class="ed-header-group">' + 
                        '<span class="ed-clickable decIndent disabled" title="Decrease Indent">' + 
                            '<i class="fa-solid fa-indent"></i>' + 
                        '</span>' + 
                        '<span class="ed-clickable incIndent" title="Increse Indent">' + 
                            '<i class="fa-solid fa-indent"></i>' + 
                        '</span>' + 
                    '</div>' + 
                '</div>' + 
                '<div class="ed-edit-area">' + 
                    '<div class="text" tabindex="0" contenteditable="true">' +
                        '<p><br></p>' + 
                    '</div>' + 
                    '<div class="resizer">' + 
                        '<svg width="10" height="10" focusable="false"><g fill-rule="nonzero"><path d="M8.1 1.1A.5.5 0 1 1 9 2l-7 7A.5.5 0 1 1 1 8l7-7ZM8.1 5.1A.5.5 0 1 1 9 6l-3 3A.5.5 0 1 1 5 8l3-3Z"></path></g></svg>' + 
                    '</div>' + 
               '</div>' + 
               '<div class="paste-conf-box" hidden>' +
                   '<div>' + 
                       '<p>Paste Formating Options</p>' + 
                       '<button class="cancel-conf-box">' + 
                           '<svg width="24" height="24" focusable="false"><path d="M17.3 8.2 13.4 12l3.9 3.8a1 1 0 0 1-1.5 1.5L12 13.4l-3.8 3.9a1 1 0 0 1-1.5-1.5l3.9-3.8-3.9-3.8a1 1 0 0 1 1.5-1.5l3.8 3.9 3.8-3.9a1 1 0 0 1 1.5 1.5Z" fill-rule="evenodd"></path></svg>' + 
                       '</button>' + 
                   '</div>' + 
                   '<div>' + 
                       '<p>Choose to keep or remove the formatting in the pasted content.</p>' + 
                   '</div>' + 
                   '<div>' + 
                       '<button class="remove-Format">Remove Formating</button>' + 
                       '<button class="keep-Format">Keep Formating</button>' +
                   '</div>' + 
               '</div>' + 
            '</div>'
        )
        this.renderMarkup()
    }

    renderMarkup() {
        // render spectrum colour picker
        this.eventFuncs.spectrumPicker()
        // render font familty dropdown
        this.font_family_formats.split(';').forEach(f => {
            const fArr = f.split('=')
            $('.dd-font-family').append(
                `<option value='${fArr[1]}' style='font-family: ${fArr[1]}' ${fArr[0] == this.default_style.font_family ? 'selected' : ''}>${fArr[0]}</option>`
            );
        })
        // render font size dropdown
        this.font_sizes.split(',').forEach((s, i) => {
            $('.dd-font-size').append(`<option value='${i + 1}' data-size-pt="${s}" data-size-px="${this.genFuncs.ptToPx(s)}" ${s == this.default_style.font_size ? 'selected' : ''}>${s}pt</option>`);
        })
        this.addEvents()
        this.addAnimationCss()

        this.default_css = `font-family: ${$('.dd-font-family').val()}; font-size: ${$('.dd-font-size option:selected').attr('data-size-pt')}pt;`
        $('.text').attr('style', this.default_css)

        $(':root').css('--ed-height', this.height + 'px');
    }

    addEvents() {
        $(document).on('click', this.eventFuncs.documentClick)
        $('.dd-font-family').on('click', (e) => e.stopPropagation())
        $('.dd-font-family').on('change', this.eventFuncs.fontFamiltyChange)
        $('.dd-font-size').on('click', (e) => e.stopPropagation())
        $('.dd-font-size').on('change', this.eventFuncs.fontSizeChange)
        $('.dd-toggle-arrow').on('click', this.eventFuncs.ddToggleArrowClicked)
        $('.ed-toggle.more').on('click', this.eventFuncs.toggleMoreOptions)
        $('.toggleBold').on('click', this.eventFuncs.toggleBold)
        $('.toggleItalic').on('click', this.eventFuncs.toggleItalic)
        $('.toggleUnderline').on('click', this.eventFuncs.toggleUnderline)
        $('.toggleNumList').on('click', this.eventFuncs.toggleNumList)
        $('.toggleNumList + i + ul li').on('click', this.eventFuncs.toggleNumListType)
        $('.toggleBulletList').on('click', this.eventFuncs.toggleBulletList)
        $('.toggleBulletList + i + ul li').on('click', this.eventFuncs.toggleBulletListType)
        $('.decIndent').on('click', this.eventFuncs.decIndent)
        $('.incIndent').on('click', this.eventFuncs.incIndent)

        $('.text').on('click', this.eventFuncs.textField_click)
        $('.text').on('keyup mouseup input', this.eventFuncs.pInput)
        $('.text').on('keydown', this.eventFuncs.textKeboardEvents)

        $('.text')[0].addEventListener('paste', async (event) => {
            event.preventDefault();
            this.test = {}
            const clipboard = event.clipboardData || window.clipboardData;
            this.clipboardDataHtml = clipboard.getData('text/html');
            this.clipboardDataText = clipboard.getData('text/plain');

            if (this.clipboardDataHtml.length > 0) {
                this.clipboardDataHtml = await this.genFuncs.reFormat(this.clipboardDataHtml, false)
                this.clipboardDataText = await this.genFuncs.reFormat(this.clipboardDataHtml, true)
                $('.paste-conf-box').removeAttr('hidden')
            } else {
                this.genFuncs.restoreCaretPosition()
                document.execCommand('insertText', false, this.clipboardDataText);
            }
        });

        $('.cancel-conf-box').on('click', (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            $('.paste-conf-box').attr('hidden', true)
        })

        $('.remove-Format').on('click', (e) => {
            e.stopPropagation();
            this.genFuncs.pasteContent(e, this.clipboardDataText)
        })

        $('.keep-Format').on('click', (e) => {
            e.stopPropagation();
            this.genFuncs.pasteContent(e, this.clipboardDataHtml)
        })

        $('.resizer').on('mousedown', this.genFuncs.resizer);

        document.addEventListener('mousemove', (e) => {
            if (!this.isResizing) return;
            const width = this.originalWidth + (e.pageX - this.originalX);
            width >= 583 && ($('.ed-container')[0].style.width = `${width}px`)
        });

        document.addEventListener('mouseup', () => {
            this.isResizing = false;
        });

    }

    addAnimationCss() {
        setTimeout(() => {
            $('.ed-header').css({
                'transition': 'border-color 0.3s ease 0.15s'
            })

            $('.ed-lower-header').css({
                'transition': 'margin-top 0.3s ease'
            })

            $('.ed-edit-area .text').css({
                'transition': 'outline-color 0.2s ease'
            })
        }, 300)
    }

    eventFuncs = {
        documentClick: (e) => {
            if ($('.text')[0].contains(e.target)) {
                $('.text').addClass('active');
            } else {
                $('.text').removeClass('active');
            }

            if (!$('.dd-toggle-arrow')[0].contains(e.target)) {
                $('.dd-toggle-arrow').removeClass('active')
            }
        },
        fontFamiltyChange: (e) => {
            this.genFuncs.restoreCaretPosition()
            this.commands.push({ fontName: e.target.value })
            this.genFuncs.execCommand()
        },
        fontSizeChange: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            this.commands.push({ fontSize: e.target.value })
            this.genFuncs.execCommand()
        },
        toggleMoreOptions: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            const isActive = this.genFuncs.Togglor(e)
            if (isActive) {
                $('.ed-container').addClass('lower-header-active');
            } else {
                $('.ed-container').removeClass('lower-header-active');
            }

        },
        toggleBold: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            const isActive = this.genFuncs.Togglor(e)
            this.commands.push('bold')
            this.genFuncs.execCommand()
            
        },
        toggleItalic: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            const isActive = this.genFuncs.Togglor(e)
            this.commands.push('italic')
            this.genFuncs.execCommand()
        },
        toggleUnderline: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition()
            const isActive = this.genFuncs.Togglor(e)
            this.commands.push('underline')
            this.genFuncs.execCommand()
        },
        spectrumPicker: () => {
            ApplyColourControls(".SpectrumEditorText", false, 'vcPalette', (color) => {
                this.genFuncs.restoreCaretPosition();
                this.commands.push({ foreColor: color })
                this.genFuncs.execCommand()
            });
            ApplyColourControls(".SpectrumEditorBg", false, 'vcPalette', (color) => {
                this.genFuncs.restoreCaretPosition();
                this.commands.push({ hiliteColor: color })
                this.genFuncs.execCommand()
            });

        },
        ddToggleArrowClicked: (e) => {
            e.stopPropagation();
            $('.dd-toggle-arrow').not(e.target).removeClass('active')
            const isActive = this.genFuncs.Togglor(e)
        },
        toggleNumList: (e) => {
            e.stopPropagation();
            $('.toggleBulletList').removeClass('active')
            $('.toggleBulletList + i + ul li').removeClass('active')
            const isActive = this.genFuncs.Togglor(e)
            this.genFuncs.restoreCaretPosition();
            this.commands.push('insertOrderedList')
            this.genFuncs.execCommand()
            if (isActive) {
                if (!$('.toggleNumList + i + ul li').hasClass('active')) {
                    const prevValue = $('.toggleNumList').attr('data-value')
                    if (prevValue) {
                        $('.toggleNumList + i + ul li[data-value="' + prevValue + '"]').addClass('active')
                    } else {
                        $('.toggleNumList + i + ul li').first().addClass('active')
                    }
                }
                const cAC = this.caretPosition && this.caretPosition.commonAncestorContainer
                cAC && $(cAC).closest('ol').attr('type', $(e.target).attr('data-value'));
                this.genFuncs.sendCaretToEnd();
            } else {
                $('.toggleNumList + i + ul li').removeClass('active')
            }
        },
        toggleNumListType: (e) => {
            e.stopPropagation();
            $('.toggleNumList + i + ul li').not(e.target).removeClass('active')
            const isActive = this.genFuncs.Togglor(e)
            this.genFuncs.restoreCaretPosition();
            if (!isActive) {
                $('.toggleNumList').trigger('click');
            } else {
                $('.toggleNumList').attr('data-value', e.target.dataset.value)
                if (!$('.toggleNumList').hasClass('active')) {
                    $('.toggleNumList').trigger('click');
                } else {
                    const cAC = this.caretPosition && this.caretPosition.commonAncestorContainer
                    cAC && $(cAC).closest('ol').attr('type', e.target.dataset.value)
                }
            }
            $('.dd-toggle-arrow').removeClass('active')
        },
        toggleBulletList: (e) => {
            e.stopPropagation();
            $('.toggleNumList').removeClass('active')
            $('.toggleNumList + i + ul li').removeClass('active')
            const isActive = this.genFuncs.Togglor(e)
            this.genFuncs.restoreCaretPosition();
            this.commands.push('insertUnorderedList')
            this.genFuncs.execCommand()
            if (isActive) {
                if (!$('.toggleBulletList + i + ul li').hasClass('active')) {
                    const prevValue = $('.toggleBulletList').attr('data-value')
                    if (prevValue) {
                        $('.toggleBulletList + i + ul li[data-value="' + prevValue + '"]').addClass('active')
                    } else {
                        $('.toggleBulletList + i + ul li').first().addClass('active')
                    }
                }
                const cAC = this.caretPosition && this.caretPosition.commonAncestorContainer
                cAC && $(cAC).closest('ul').css('list-style-type', $(e.target).attr('data-value'))
                this.genFuncs.sendCaretToEnd();
            } else {
                $('.toggleBulletList + i + ul li').removeClass('active')
                }
        },
        toggleBulletListType: (e) => {
            e.stopPropagation();
            $('.toggleBulletList + i + ul li').not(e.target).removeClass('active')
            const isActive = this.genFuncs.Togglor(e)
            this.genFuncs.restoreCaretPosition();
            if (!isActive) {
                $('.toggleBulletList').trigger('click');
            } else {
                $('.toggleBulletList').attr('data-value', e.target.dataset.value)
                if (!$('.toggleBulletList').hasClass('active')) {
                    $('.toggleBulletList').trigger('click');
                } else {
                    const cAC = this.caretPosition && this.caretPosition.commonAncestorContainer
                    cAC && $(cAC).closest('ul').css('list-style-type', e.target.dataset.value)
                }
            }
            $('.dd-toggle-arrow').removeClass('active')
        },
        decIndent: (e) => {
            e.stopPropagation();
            if ($(e.target).hasClass('disabled')) return
            this.genFuncs.restoreCaretPosition();
            this.commands.push('outdent')
            this.genFuncs.execCommand()
        },
        incIndent: (e) => {
            e.stopPropagation();
            this.genFuncs.restoreCaretPosition();
            this.commands.push('indent')
            this.genFuncs.execCommand()
        },
        textField_click: (e) => {
                $('.text').trigger('focus')
        },
        pInput: (e) => {
            const spActive = $('.sp-replacer').hasClass('sp-active')
            if (!spActive) {
                this.genFuncs.saveCaretPosition(e);
            this.genFuncs.restoreAllControls()
                    }
            this.commands = []
        },
        textKeboardEvents: (e) => {
            e.stopPropagation();

            switch (e.key) {
                case 'Enter':

                    break;
                case 'Backspace':
                    
                    break;
            }
        }
    }

    genFuncs = {

        // add css file in head of document
        addCssFile: (file) => {
            if (!file) {
                console.error('File name is required to add css file');
                return;
            }

            // Create a link element
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = this.genFuncs.absPath() + '/' + file;
            // Append the link element to the head
            document.head.appendChild(link);
        },

        // absolute path to the library
        absPath: () => {
            const scripts = document.getElementsByTagName('script');
            let currentScript = null;
            for (let i = 0; i < scripts.length; i++) {
                if (scripts[i].src && scripts[i].src.includes('editor.js')) {
                    currentScript = scripts[i];
                    break;
                }
            }

            const scriptUrl = currentScript.src;
            const path = scriptUrl.substring(0, scriptUrl.lastIndexOf('/'));

            return path
        },
        execCommand: () => {
            this.commands.forEach(cmd => {
                const key = Object.keys(cmd)[0]
                if (key == 0) {
                    document.execCommand(cmd, false, null);
                } else {
                    document.execCommand(key, false, cmd[key]);
                }
            })
        },
        saveCaretPosition: () => {
            const selection = window.getSelection();
            if (selection.rangeCount > 0) {
                this.caretPosition = selection.getRangeAt(0);
            }
        },
        restoreCaretPosition: () => {
            const selection = window.getSelection();
            if (this.caretPosition && $('.text').hasClass('active')) {
                selection.removeAllRanges();
                selection.addRange(this.caretPosition);
            }
        },
        sendCaretToEnd: () => {
            if (!this.caretPosition) return
            let cAC = this.caretPosition.commonAncestorContainer

            while (cAC && cAC.nodeType !== Node.ELEMENT_NODE) {
                cAC = cAC.parentNode;
            }

            if (cAC.nodeName != 'LI') {
                cAC = cAC.closest('li')
            }

            while (cAC && cAC.lastChild) {
                cAC = cAC.lastChild;
            }

            if (!cAC) return

                const range = document.createRange();
                const selection = window.getSelection();
            if (cAC && cAC.nodeType === Node.TEXT_NODE) {
                range.setStart(cAC, cAC.length);
                range.setEnd(cAC, cAC.length);
            } else {
                range.setStartAfter(cAC);
                range.setEndAfter(cAC);
            }

                selection.removeAllRanges();
                selection.addRange(range);

            this.genFuncs.saveCaretPosition()
        },
        Togglor: (e) => {
            const bool = $(e.target).hasClass('active')
            if (bool) {
                $(e.target).removeClass('active')
            } else {
                $(e.target).addClass('active')
            }

            return !bool
        },
        restoreAllControls: () => {
            let e = this.caretPosition.startContainer;
            while (e && e.nodeType !== Node.ELEMENT_NODE) {
                e = e.parentNode;
            }
            var $fontFamily = $('.dd-font-family');
            var $fontSize = $('.dd-font-size');
            $fontFamily.val($(e).css('font-family'))
            $('.dd-font-size option[data-size-px="' + Math.floor(parseInt($(e).css('font-size'))) + '"]').prop('selected', true);
            $('.toggleBold').toggleClass('active', $(e).css('font-weight') === '700');
            $('.toggleItalic').toggleClass('active', $(e).css('font-style') === 'italic');
            $('.toggleUnderline').toggleClass('active', $(e).css('text-decoration').includes('underline'));
            $('.SpectrumEditorText').val($(e).css('color'))
            $('.SpectrumEditorBg').val($(e).css('background-color'))

            if (!$fontFamily.val()) $fontFamily.val($fontFamily.find('option:first').val());

            if (!$fontSize.val()) $fontSize.val($fontSize.find('option:first').val());

            this.eventFuncs.spectrumPicker()

            if ($(e).closest('li').length > 0 || $(e).closest('blockquote').length > 0) {
                $('.decIndent').removeClass('disabled')
            } else {
                $('.decIndent').addClass('disabled')
                            }
                    
            this.genFuncs.unActiveListControls()
            if ($(e).closest('li').length > 0) {
                const li = e.closest('li')
                const parent = li.parentElement
                if (parent.nodeName === 'OL') {
                    $('.toggleNumList').addClass('active')
                    const attr = $(parent).attr('type')
                    const list = $('.toggleNumList + i + ul li')
                    switch (attr) {
                        case 'a':
                            list.eq(1).addClass('active')
                            break;
                        case 'A':
                            list.eq(2).addClass('active')
                            break;
                        case 'i':
                            list.eq(3).addClass('active')
                            break;
                        case 'I':
                            list.eq(4).addClass('active')
                            break;
                        default:
                            list.eq(0).addClass('active')
                                }
                } else {
                    $('.toggleBulletList').addClass('active')
                    const css = $(parent).css('list-style-type')
                    const list = $('.toggleBulletList + i + ul li')
                    switch (css) {
                        case 'circle':
                            list.eq(1).addClass('active')
                            break;
                        case 'square':
                            list.eq(2).addClass('active')
                            break;
                        default:
                            list.eq(0).addClass('active')
                        }
            }
            }
        },
        ptToPx: (pt) => {
            const pxPerPt = 96 / 72;
            return Math.floor(pt * pxPerPt)
        },
        unActiveListControls: () => {
            $('.toggleNumList').removeClass('active')
            $('.toggleNumList + i + ul li').removeClass('active')
            $('.toggleBulletList').removeClass('active')
            $('.toggleBulletList + i + ul li').removeClass('active')
        },
        convertFontSize: (size) => {
            const pxToFont = this.font_sizes.split(',');
            if (size <= 7) return pxToFont[size - 1];
            return pxToFont.findIndex(px => size <= px) + 1 || 7;
        },
        resizer: (e) => {
            e.preventDefault();
            this.originalWidth = $('.ed-container')[0].offsetWidth;
            this.originalHeight = $('.ed-container')[0].offsetHeight;
            this.originalX = e.pageX;
            this.originalY = e.pageY;
            this.isResizing = true;
        },
        pasteContent: (e, content) => {
            this.genFuncs.restoreCaretPosition()
            document.execCommand('insertHTML', false, content);
            Object.keys(this.test).forEach(item => {
                const e = $('.' + item)
                if (e.length > 0) {
                    e.attr('style', this.test[item])
                    e.removeClass(item)
            }
            })
            $('.cancel-conf-box').trigger('click')
        },
        reFormat: (html, remFormat) => {
            const promises = [];
            const e = $('<div>').html(html)
            e.find('*').each(async (i, item) => {
                var attributes = $.map(item.attributes, function (item) {
                    return item.name;
                });

                var element = $(item);
                if (element[0].nodeName === 'IMG') {
                    const promise = this.genFuncs.imageToBase64(element.attr('src')).then(base64 => {
                        element.attr('src', base64);
                    });
                    promises.push(promise);
                } else {
                    $.each(attributes, (i, item) => {
                        remFormat && element.removeAttr(item);
                    });
                    if (!remFormat) {
                        const className = this.genFuncs.generateRandomClassName();
                        element.addClass(className);

                        // Capture style attributes
                        const style = element.attr('style');
                        this.test[className] = style;
                    }
                }
            });

            return Promise.all(promises).then(() => e.html().toString());
        },
        generateRandomClassName: () => {
            return 'class-' + Math.random().toString(36).slice(2, 11);
        },
        escapeHtml: (unsafe) => {
            return unsafe.replace(/[&<"']/g, function (m) {
                switch (m) {
                    case '&':
                        return '&amp;';
                    case '<':
                        return '&lt;';
                    case '>':
                        return '&gt;';
                    case '"':
                        return '&quot;';
                    default:
                        return '&#039;';
                }
            });
        },
        imageToBase64: (src) => {
            return new Promise((resolve) => {
                const img = new Image();
                img.crossOrigin = "Anonymous";
                img.src = src;
                img.onload = function () {
                    const canvas = document.createElement('canvas');
                    const ctx = canvas.getContext('2d');
                    canvas.width = this.width;
                    canvas.height = this.height;
                    ctx.drawImage(this, 0, 0);
                    const base64String = canvas.toDataURL('image/png');
                    resolve(base64String)
                }
            })
        }
    }

}

window.editor = new Editor()