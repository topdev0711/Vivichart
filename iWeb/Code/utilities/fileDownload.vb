Imports System.IO

Public Class fileDownload

    Private _HTTP As HttpContext
    Private _forceDialog As Boolean
    Private _saveAsFile As String
    Private _filePath As String
    Private _mimeType As String
    Private _ext As String
    Private _isBinary As Boolean
    Private _isFile As Boolean
    Private _dataAsBytes As Byte()
    Private _dataAsString As String

    Public WriteOnly Property HTTP() As HttpContext
        Set(ByVal Value As HttpContext)
            If Value Is Nothing Then Throw New Exception("HTTP context is not allowed to be nothing")
            _HTTP = Value
        End Set
    End Property

#Region "long list of mime types"

    Private ReadOnly Property resolveMimeType() As String
        Get
            Dim Ext As String = Path.GetExtension(_saveAsFile)
            If Not String.IsNullOrEmpty(Ext) Then
                Select Case Ext.ToLower()
                    Case ".pdf"
                        Return "application/pdf"  'Adobe Portable Document Format
                    Case ".doc"
                        Return "application/msword"  'Microsoft Word
                    Case ".docx"
                        Return "application/vnd.openxmlformats-officedocument.wordprocessingml.document"  'Microsoft Office - OOXML - Word Document
                    Case ".xls"
                        Return "application/vnd.ms-excel"  'Microsoft Excel
                    Case ".xlsx"
                        Return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"  'Microsoft Office - OOXML - Spreadsheet
                    Case ".jpeg", ".jpg"
                        Return "image/jpeg"  'JPEG Image
                    Case ".gif"
                        Return "image/gif"  'Graphics Interchange Format
                    Case ".png"
                        Return "image/png"  'Portable Network Graphics (PNG)
                    Case ".txt"
                        Return "text/plain"  'Text File
                    Case ".xps"
                        Return "application/vnd.ms-xpsdocument"  'Microsoft XML Paper Specification
                    Case ".zip"
                        Return "application/zip"  'Zip Archive
                    Case ".msg"
                        Return "application/vnd.ms-outlook" 'Outlook mail message

                    Case ".gdoc"
                        Return "application/vnd.google-apps.document"   'Google Drive Document
                    Case ".gslides"
                        Return "application/vnd.google-apps.presentation"  'Google Drive Presentation
                    Case ".gsheet"
                        Return "application/vnd.google-apps.spreadsheet"   'Google Drive Spreadsheet
                    Case ".gdraw"
                        Return "application/vnd.google-apps.drawing"  'Google Drive Drawing
                    Case ".gtable"
                        Return "application/vnd.google-apps.fusiontable"   'Google Drive Fusion Table
                    Case ".gform"
                        Return "application/vnd.google-apps.form"   'Google Drive Form

                    Case ".123"
                        Return "application/vnd.lotus-1-2-3"  'Lotus 1-2-3
                    Case ".3dml"
                        Return "text/vnd.in3d.3dml"  'In3D - 3DML
                    Case ".3g2"
                        Return "video/3gpp2"  '3GP2
                    Case ".3gp"
                        Return "video/3gpp"  '3GP
                    Case ".7z"
                        Return "application/x-7z-compressed"  '7-Zip
                    Case ".aab"
                        Return "application/x-authorware-bin"  'Adobe (Macropedia) Authorware - Binary File
                    Case ".aac"
                        Return "audio/x-aac"  'Advanced Audio Coding (AAC)
                    Case ".aam"
                        Return "application/x-authorware-map"  'Adobe (Macropedia) Authorware - Map
                    Case ".aas"
                        Return "application/x-authorware-seg"  'Adobe (Macropedia) Authorware - Segment File
                    Case ".abw"
                        Return "application/x-abiword"  'AbiWord
                    Case ".ac"
                        Return "application/pkix-attr-cert"  'Attribute Certificate
                    Case ".acc"
                        Return "application/vnd.americandynamics.acc"  'Active Content Compression
                    Case ".ace"
                        Return "application/x-ace-compressed"  'Ace Archive
                    Case ".acu"
                        Return "application/vnd.acucobol"  'ACU Cobol
                    Case ".adp"
                        Return "audio/adpcm"  'Adaptive differential pulse-code modulation
                    Case ".aep"
                        Return "application/vnd.audiograph"  'Audiograph
                    Case ".afp"
                        Return "application/vnd.ibm.modcap"  'MO:DCA-P
                    Case ".ahead"
                        Return "application/vnd.ahead.space"  'Ahead AIR Application
                    Case ".ai"
                        Return "application/postscript"  'PostScript
                    Case ".aif"
                        Return "audio/x-aiff"  'Audio Interchange File Format
                    Case ".air"
                        Return "application/vnd.adobe.air-application-installer-package+zip"  'Adobe AIR Application
                    Case ".ait"
                        Return "application/vnd.dvb.ait"  'Digital Video Broadcasting
                    Case ".ami"
                        Return "application/vnd.amiga.ami"  'AmigaDE
                    Case ".apk"
                        Return "application/vnd.android.package-archive"  'Android Package Archive
                    Case ".application"
                        Return "application/x-ms-application"  'Microsoft ClickOnce
                    Case ".apr"
                        Return "application/vnd.lotus-approach"  'Lotus Approach
                    Case ".asf"
                        Return "video/x-ms-asf"  'Microsoft Advanced Systems Format (ASF)
                    Case ".aso"
                        Return "application/vnd.accpac.simply.aso"  'Simply Accounting
                    Case ".atc"
                        Return "application/vnd.acucorp"  'ACU Cobol
                    Case ".atom"
                        Return "application/atom+xml"  'Atom Syndication Format
                    Case ".atomcat"
                        Return "application/atomcat+xml"  'Atom Publishing Protocol
                    Case ".atomsvc"
                        Return "application/atomsvc+xml"  'Atom Publishing Protocol Service Document
                    Case ".atx"
                        Return "application/vnd.antix.game-component"  'Antix Game Player
                    Case ".au"
                        Return "audio/basic"  'Sun Audio - Au file format
                    Case ".avi"
                        Return "video/x-msvideo"  'Audio Video Interleave (AVI)
                    Case ".aw"
                        Return "application/applixware"  'Applixware
                    Case ".azf"
                        Return "application/vnd.airzip.filesecure.azf"  'AirZip FileSECURE
                    Case ".azs"
                        Return "application/vnd.airzip.filesecure.azs"  'AirZip FileSECURE
                    Case ".azw"
                        Return "application/vnd.amazon.ebook"  'Amazon Kindle eBook format
                    Case ".bcpio"
                        Return "application/x-bcpio"  'Binary CPIO Archive
                    Case ".bdf"
                        Return "application/x-font-bdf"  'Glyph Bitmap Distribution Format
                    Case ".bdm"
                        Return "application/vnd.syncml.dm+wbxml"  'SyncML - Device Management
                    Case ".bed"
                        Return "application/vnd.realvnc.bed"  'RealVNC
                    Case ".bh2"
                        Return "application/vnd.fujitsu.oasysprs"  'Fujitsu Oasys
                    Case ".bin"
                        Return "application/octet-stream"  'Binary Data
                    Case ".bmi"
                        Return "application/vnd.bmi"  'BMI Drawing Data Interchange
                    Case ".bmp"
                        Return "image/bmp"  'Bitmap Image File
                    Case ".box"
                        Return "application/vnd.previewsystems.box"  'Preview Systems ZipLock/VBox
                    Case ".btif"
                        Return "image/prs.btif"  'BTIF
                    Case ".bz"
                        Return "application/x-bzip"  'Bzip Archive
                    Case ".bz2"
                        Return "application/x-bzip2"  'Bzip2 Archive
                    Case ".c"
                        Return "text/x-c"  'C Source File
                    Case ".c11amc"
                        Return "application/vnd.cluetrust.cartomobile-config"  'ClueTrust CartoMobile - Config
                    Case ".c11amz"
                        Return "application/vnd.cluetrust.cartomobile-config-pkg"  'ClueTrust CartoMobile - Config Package
                    Case ".c4g"
                        Return "application/vnd.clonk.c4group"  'Clonk Game
                    Case ".cab"
                        Return "application/vnd.ms-cab-compressed"  'Microsoft Cabinet File
                    Case ".car"
                        Return "application/vnd.curl.car"  'CURL Applet
                    Case ".cat"
                        Return "application/vnd.ms-pki.seccat"  'Microsoft Trust UI Provider - Security Catalog
                    Case ".ccxml"
                        Return "application/ccxml+xml,"  'Voice Browser Call Control
                    Case ".cdbcmsg"
                        Return "application/vnd.contact.cmsg"  'CIM Database
                    Case ".cdkey"
                        Return "application/vnd.mediastation.cdkey"  'MediaRemote
                    Case ".cdmia"
                        Return "application/cdmi-capability"  'Cloud Data Management Interface (CDMI) - Capability
                    Case ".cdmic"
                        Return "application/cdmi-container"  'Cloud Data Management Interface (CDMI) - Contaimer
                    Case ".cdmid"
                        Return "application/cdmi-domain"  'Cloud Data Management Interface (CDMI) - Domain
                    Case ".cdmio"
                        Return "application/cdmi-object"  'Cloud Data Management Interface (CDMI) - Object
                    Case ".cdmiq"
                        Return "application/cdmi-queue"  'Cloud Data Management Interface (CDMI) - Queue
                    Case ".cdx"
                        Return "chemical/x-cdx"  'ChemDraw eXchange file
                    Case ".cdxml"
                        Return "application/vnd.chemdraw+xml"  'CambridgeSoft Chem Draw
                    Case ".cdy"
                        Return "application/vnd.cinderella"  'Interactive Geometry Software Cinderella
                    Case ".cer"
                        Return "application/pkix-cert"  'Internet Public Key Infrastructure - Certificate
                    Case ".cgm"
                        Return "image/cgm"  'Computer Graphics Metafile
                    Case ".chat"
                        Return "application/x-chat"  'pIRCh
                    Case ".chm"
                        Return "application/vnd.ms-htmlhelp"  'Microsoft Html Help File
                    Case ".chrt"
                        Return "application/vnd.kde.kchart"  'KDE KOffice Office Suite - KChart
                    Case ".cif"
                        Return "chemical/x-cif"  'Crystallographic Interchange Format
                    Case ".cii"
                        Return "application/vnd.anser-web-certificate-issue-initiation"  'ANSER-WEB Terminal Client - Certificate Issue
                    Case ".cil"
                        Return "application/vnd.ms-artgalry"  'Microsoft Artgalry
                    Case ".cla"
                        Return "application/vnd.claymore"  'Claymore Data Files
                    Case ".class"
                        Return "application/java-vm"  'Java Bytecode File
                    Case ".clkk"
                        Return "application/vnd.crick.clicker.keyboard"  'CrickSoftware - Clicker - Keyboard
                    Case ".clkp"
                        Return "application/vnd.crick.clicker.palette"  'CrickSoftware - Clicker - Palette
                    Case ".clkt"
                        Return "application/vnd.crick.clicker.template"  'CrickSoftware - Clicker - Template
                    Case ".clkw"
                        Return "application/vnd.crick.clicker.wordbank"  'CrickSoftware - Clicker - Wordbank
                    Case ".clkx"
                        Return "application/vnd.crick.clicker"  'CrickSoftware - Clicker
                    Case ".clp"
                        Return "application/x-msclip"  'Microsoft Clipboard Clip
                    Case ".cmc"
                        Return "application/vnd.cosmocaller"  'CosmoCaller
                    Case ".cmdf"
                        Return "chemical/x-cmdf"  'CrystalMaker Data Format
                    Case ".cml"
                        Return "chemical/x-cml"  'Chemical Markup Language
                    Case ".cmp"
                        Return "application/vnd.yellowriver-custom-menu"  'CustomMenu
                    Case ".cmx"
                        Return "image/x-cmx"  'Corel Metafile Exchange (CMX)
                    Case ".cod"
                        Return "application/vnd.rim.cod"  'Blackberry COD File
                    Case ".cpio"
                        Return "application/x-cpio"  'CPIO Archive
                    Case ".cpt"
                        Return "application/mac-compactpro"  'Compact Pro
                    Case ".crd"
                        Return "application/x-mscardfile"  'Microsoft Information Card
                    Case ".crl"
                        Return "application/pkix-crl"  'Internet Public Key Infrastructure - Certificate Revocation Lists
                    Case ".cryptonote"
                        Return "application/vnd.rig.cryptonote"  'CryptoNote
                    Case ".csh"
                        Return "application/x-csh"  'C Shell Script
                    Case ".csml"
                        Return "chemical/x-csml"  'Chemical Style Markup Language
                    Case ".csp"
                        Return "application/vnd.commonspace"  'Sixth Floor Media - CommonSpace
                    Case ".css"
                        Return "text/css"  'Cascading Style Sheets (CSS)
                    Case ".csv"
                        Return "text/csv"  'Comma-Seperated Values
                    Case ".cu"
                        Return "application/cu-seeme"  'CU-SeeMe
                    Case ".curl"
                        Return "text/vnd.curl"  'Curl - Applet
                    Case ".cww"
                        Return "application/prs.cww"  'CU-Writer
                    Case ".dae"
                        Return "model/vnd.collada+xml"  'COLLADA
                    Case ".daf"
                        Return "application/vnd.mobius.daf"  'Mobius Management Systems - UniversalArchive
                    Case ".davmount"
                        Return "application/davmount+xml"  'Web Distributed Authoring and Versioning
                    Case ".dcurl"
                        Return "text/vnd.curl.dcurl"  'Curl - Detached Applet
                    Case ".dd2"
                        Return "application/vnd.oma.dd2+xml"  'OMA Download Agents
                    Case ".ddd"
                        Return "application/vnd.fujixerox.ddd"  'Fujitsu - Xerox 2D CAD Data
                    Case ".deb"
                        Return "application/x-debian-package"  'Debian Package
                    Case ".der"
                        Return "application/x-x509-ca-cert"  'X.509 Certificate
                    Case ".dfac"
                        Return "application/vnd.dreamfactory"  'DreamFactory
                    Case ".dir"
                        Return "application/x-director"  'Adobe Shockwave Player
                    Case ".dis"
                        Return "application/vnd.mobius.dis"  'Mobius Management Systems - Distribution Database
                    Case ".djvu"
                        Return "image/vnd.djvu"  'DjVu
                    Case ".dmg"
                        Return "application/x-apple-diskimage"  'Apple Disk Image
                    Case ".dna"
                        Return "application/vnd.dna"  'New Moon Liftoff/DNA
                    Case ".docm"
                        Return "application/vnd.ms-word.document.macroenabled.12"  'Microsoft Word - Macro-Enabled Document
                    Case ".dp"
                        Return "application/vnd.osgi.dp"  'OSGi Deployment Package
                    Case ".dpg"
                        Return "application/vnd.dpgraph"  'DPGraph
                    Case ".dra"
                        Return "audio/vnd.dra"  'DRA Audio
                    Case ".dsc"
                        Return "text/prs.lines.tag"  'PRS Lines Tag
                    Case ".dssc"
                        Return "application/dssc+der"  'Data Structure for the Security Suitability of Cryptographic Algorithms
                    Case ".dtb"
                        Return "application/x-dtbook+xml"  'Digital Talking Book
                    Case ".dtd"
                        Return "application/xml-dtd"  'Document Type Definition
                    Case ".dts"
                        Return "audio/vnd.dts"  'DTS Audio
                    Case ".dtshd"
                        Return "audio/vnd.dts.hd"  'DTS High Definition Audio
                    Case ".dvi"
                        Return "application/x-dvi"  'Device Independent File Format (DVI)
                    Case ".dwf"
                        Return "model/vnd.dwf"  'Autodesk Design Web Format (DWF)
                    Case ".dwg"
                        Return "image/vnd.dwg"  'DWG Drawing
                    Case ".dxf"
                        Return "image/vnd.dxf"  'AutoCAD DXF
                    Case ".dxp"
                        Return "application/vnd.spotfire.dxp"  'TIBCO Spotfire
                    Case ".ecelp4800"
                        Return "audio/vnd.nuera.ecelp4800"  'Nuera ECELP 4800
                    Case ".ecelp7470"
                        Return "audio/vnd.nuera.ecelp7470"  'Nuera ECELP 7470
                    Case ".ecelp9600"
                        Return "audio/vnd.nuera.ecelp9600"  'Nuera ECELP 9600
                    Case ".edm"
                        Return "application/vnd.novadigm.edm"  'Novadigm's RADIA and EDM products
                    Case ".edx"
                        Return "application/vnd.novadigm.edx"  'Novadigm's RADIA and EDM products
                    Case ".efif"
                        Return "application/vnd.picsel"  'Pcsel eFIF File
                    Case ".ei6"
                        Return "application/vnd.pg.osasli"  'Proprietary P&G Standard Reporting System
                    Case ".eml"
                        Return "message/rfc822"  'Email Message
                    Case ".emma"
                        Return "application/emma+xml"  'Extensible MultiModal Annotation
                    Case ".eol"
                        Return "audio/vnd.digital-winds"  'Digital Winds Music
                    Case ".eot"
                        Return "application/vnd.ms-fontobject"  'Microsoft Embedded OpenType
                    Case ".epub"
                        Return "application/epub+zip"  'Electronic Publication
                    Case ".es"
                        Return "application/ecmascript"  'ECMAScript
                    Case ".es3"
                        Return "application/vnd.eszigno3+xml"  'MICROSEC e-Szign¢
                    Case ".esf"
                        Return "application/vnd.epson.esf"  'QUASS Stream Player
                    Case ".etx"
                        Return "text/x-setext"  'Setext
                    Case ".exe"
                        Return "application/x-msdownload"  'Microsoft Application
                    Case ".exi"
                        Return "application/exi"  'Efficient XML Interchange
                    Case ".ext"
                        Return "application/vnd.novadigm.ext"  'Novadigm's RADIA and EDM products
                    Case ".ez2"
                        Return "application/vnd.ezpix-album"  'EZPix Secure Photo Album
                    Case ".ez3"
                        Return "application/vnd.ezpix-package"  'EZPix Secure Photo Album
                    Case ".f"
                        Return "text/x-fortran"  'Fortran Source File
                    Case ".f4v"
                        Return "video/x-f4v"  'Flash Video
                    Case ".fbs"
                        Return "image/vnd.fastbidsheet"  'FastBid Sheet
                    Case ".fcs"
                        Return "application/vnd.isac.fcs"  'International Society for Advancement of Cytometry
                    Case ".fdf"
                        Return "application/vnd.fdf"  'Forms Data Format
                    Case ".fe_launch"
                        Return "application/vnd.denovo.fcselayout-link"  'FCS Express Layout Link
                    Case ".fg5"
                        Return "application/vnd.fujitsu.oasysgp"  'Fujitsu Oasys
                    Case ".fh"
                        Return "image/x-freehand"  'FreeHand MX
                    Case ".fig"
                        Return "application/x-xfig"  'Xfig
                    Case ".fli"
                        Return "video/x-fli"  'FLI/FLC Animation Format
                    Case ".flo"
                        Return "application/vnd.micrografx.flo"  'Micrografx
                    Case ".flv"
                        Return "video/x-flv"  'Flash Video
                    Case ".flw"
                        Return "application/vnd.kde.kivio"  'KDE KOffice Office Suite - Kivio
                    Case ".flx"
                        Return "text/vnd.fmi.flexstor"  'FLEXSTOR
                    Case ".fly"
                        Return "text/vnd.fly"  'mod_fly / fly.cgi
                    Case ".fm"
                        Return "application/vnd.framemaker"  'FrameMaker Normal Format
                    Case ".fnc"
                        Return "application/vnd.frogans.fnc"  'Frogans Player
                    Case ".fpx"
                        Return "image/vnd.fpx"  'FlashPix
                    Case ".fsc"
                        Return "application/vnd.fsc.weblaunch"  'Friendly Software Corporation
                    Case ".fst"
                        Return "image/vnd.fst"  'FAST Search & Transfer ASA
                    Case ".ftc"
                        Return "application/vnd.fluxtime.clip"  'FluxTime Clip
                    Case ".fti"
                        Return "application/vnd.anser-web-funds-transfer-initiation"  'ANSER-WEB Terminal Client - Web Funds Transfer
                    Case ".fvt"
                        Return "video/vnd.fvt"  'FAST Search & Transfer ASA
                    Case ".fxp"
                        Return "application/vnd.adobe.fxp"  'Adobe Flex Project
                    Case ".fzs"
                        Return "application/vnd.fuzzysheet"  'FuzzySheet
                    Case ".g2w"
                        Return "application/vnd.geoplan"  'GeoplanW
                    Case ".g3"
                        Return "image/g3fax"  'G3 Fax Image
                    Case ".g3w"
                        Return "application/vnd.geospace"  'GeospacW
                    Case ".gac"
                        Return "application/vnd.groove-account"  'Groove - Account
                    Case ".gdl"
                        Return "model/vnd.gdl"  'Geometric Description Language (GDL)
                    Case ".geo"
                        Return "application/vnd.dynageo"  'DynaGeo
                    Case ".gex"
                        Return "application/vnd.geometry-explorer"  'GeoMetry Explorer
                    Case ".ggb"
                        Return "application/vnd.geogebra.file"  'GeoGebra
                    Case ".ggt"
                        Return "application/vnd.geogebra.tool"  'GeoGebra
                    Case ".ghf"
                        Return "application/vnd.groove-help"  'Groove - Help
                    Case ".gim"
                        Return "application/vnd.groove-identity-message"  'Groove - Identity Message
                    Case ".gmx"
                        Return "application/vnd.gmx"  'GameMaker ActiveX
                    Case ".gnumeric"
                        Return "application/x-gnumeric"  'Gnumeric
                    Case ".gph"
                        Return "application/vnd.flographit"  'NpGraphIt
                    Case ".gqf"
                        Return "application/vnd.grafeq"  'GrafEq
                    Case ".gram"
                        Return "application/srgs"  'Speech Recognition Grammar Specification
                    Case ".grv"
                        Return "application/vnd.groove-injector"  'Groove - Injector
                    Case ".grxml"
                        Return "application/srgs+xml"  'Speech Recognition Grammar Specification - XML
                    Case ".gsf"
                        Return "application/x-font-ghostscript"  'Ghostscript Font
                    Case ".gtar"
                        Return "application/x-gtar"  'GNU Tar Files
                    Case ".gtm"
                        Return "application/vnd.groove-tool-message"  'Groove - Tool Message
                    Case ".gtw"
                        Return "model/vnd.gtw"  'Gen-Trix Studio
                    Case ".gv"
                        Return "text/vnd.graphviz"  'Graphviz
                    Case ".gxt"
                        Return "application/vnd.geonext"  'GEONExT and JSXGraph
                    Case ".h261"
                        Return "video/h261"  'H.261
                    Case ".h263"
                        Return "video/h263"  'H.263
                    Case ".h264"
                        Return "video/h264"  'H.264
                    Case ".hal"
                        Return "application/vnd.hal+xml"  'Hypertext Application Language
                    Case ".hbci"
                        Return "application/vnd.hbci"  'Homebanking Computer Interface (HBCI)
                    Case ".hdf"
                        Return "application/x-hdf"  'Hierarchical Data Format
                    Case ".hlp"
                        Return "application/winhlp"  'WinHelp
                    Case ".hpgl"
                        Return "application/vnd.hp-hpgl"  'HP-GL/2 and HP RTL
                    Case ".hpid"
                        Return "application/vnd.hp-hpid"  'Hewlett Packard Instant Delivery
                    Case ".hps"
                        Return "application/vnd.hp-hps"  'Hewlett-Packard's WebPrintSmart
                    Case ".hqx"
                        Return "application/mac-binhex40"  'Macintosh BinHex 4.0
                    Case ".htke"
                        Return "application/vnd.kenameaapp"  'Kenamea App
                    Case ".html"
                        Return "text/html"  'HyperText Markup Language (HTML)
                    Case ".hvd"
                        Return "application/vnd.yamaha.hv-dic"  'HV Voice Dictionary
                    Case ".hvp"
                        Return "application/vnd.yamaha.hv-voice"  'HV Voice Parameter
                    Case ".hvs"
                        Return "application/vnd.yamaha.hv-script"  'HV Script
                    Case ".i2g"
                        Return "application/vnd.intergeo"  'Interactive Geometry Software
                    Case ".icc"
                        Return "application/vnd.iccprofile"  'ICC profile
                    Case ".ice"
                        Return "x-conference/x-cooltalk"  'CoolTalk
                    Case ".ico"
                        Return "image/x-icon"  'Icon Image
                    Case ".ics"
                        Return "text/calendar"  'iCalendar
                    Case ".ief"
                        Return "image/ief"  'Image Exchange Format
                    Case ".ifm"
                        Return "application/vnd.shana.informed.formdata"  'Shana Informed Filler
                    Case ".igl"
                        Return "application/vnd.igloader"  'igLoader
                    Case ".igm"
                        Return "application/vnd.insors.igm"  'IOCOM Visimeet
                    Case ".igs"
                        Return "model/iges"  'Initial Graphics Exchange Specification (IGES)
                    Case ".igx"
                        Return "application/vnd.micrografx.igx"  'Micrografx iGrafx Professional
                    Case ".iif"
                        Return "application/vnd.shana.informed.interchange"  'Shana Informed Filler
                    Case ".imp"
                        Return "application/vnd.accpac.simply.imp"  'Simply Accounting - Data Import
                    Case ".ims"
                        Return "application/vnd.ms-ims"  'Microsoft Class Server
                    Case ".ipfix"
                        Return "application/ipfix"  'Internet Protocol Flow Information Export
                    Case ".ipk"
                        Return "application/vnd.shana.informed.package"  'Shana Informed Filler
                    Case ".irm"
                        Return "application/vnd.ibm.rights-management"  'IBM DB2 Rights Manager
                    Case ".irp"
                        Return "application/vnd.irepository.package+xml"  'iRepository / Lucidoc Editor
                    Case ".itp"
                        Return "application/vnd.shana.informed.formtemplate"  'Shana Informed Filler
                    Case ".ivp"
                        Return "application/vnd.immervision-ivp"  'ImmerVision PURE Players
                    Case ".ivu"
                        Return "application/vnd.immervision-ivu"  'ImmerVision PURE Players
                    Case ".jad"
                        Return "text/vnd.sun.j2me.app-descriptor"  'J2ME App Descriptor
                    Case ".jam"
                        Return "application/vnd.jam"  'Lightspeed Audio Lab
                    Case ".jar"
                        Return "application/java-archive"  'Java Archive
                    Case ".java"
                        Return "text/x-java-source,java"  'Java Source File
                    Case ".jisp"
                        Return "application/vnd.jisp"  'RhymBox
                    Case ".jlt"
                        Return "application/vnd.hp-jlyt"  'HP Indigo Digital Press - Job Layout Languate
                    Case ".jnlp"
                        Return "application/x-java-jnlp-file"  'Java Network Launching Protocol
                    Case ".joda"
                        Return "application/vnd.joost.joda-archive"  'Joda Archive
                    Case ".jpgv"
                        Return "video/jpeg"  'JPGVideo
                    Case ".jpm"
                        Return "video/jpm"  'JPEG 2000 Compound Image File Format
                    Case ".js"
                        Return "application/javascript"  'JavaScript
                    Case ".json"
                        Return "application/json"  'JavaScript Object Notation (JSON)
                    Case ".karbon"
                        Return "application/vnd.kde.karbon"  'KDE KOffice Office Suite - Karbon
                    Case ".kfo"
                        Return "application/vnd.kde.kformula"  'KDE KOffice Office Suite - Kformula
                    Case ".kia"
                        Return "application/vnd.kidspiration"  'Kidspiration
                    Case ".kml"
                        Return "application/vnd.google-earth.kml+xml"  'Google Earth - KML
                    Case ".kmz"
                        Return "application/vnd.google-earth.kmz"  'Google Earth - Zipped KML
                    Case ".kne"
                        Return "application/vnd.kinar"  'Kinar Applications
                    Case ".kon"
                        Return "application/vnd.kde.kontour"  'KDE KOffice Office Suite - Kontour
                    Case ".kpr"
                        Return "application/vnd.kde.kpresenter"  'KDE KOffice Office Suite - Kpresenter
                    Case ".ksp"
                        Return "application/vnd.kde.kspread"  'KDE KOffice Office Suite - Kspread
                    Case ".ktx"
                        Return "image/ktx"  'OpenGL Textures (KTX)
                    Case ".ktz"
                        Return "application/vnd.kahootz"  'Kahootz
                    Case ".kwd"
                        Return "application/vnd.kde.kword"  'KDE KOffice Office Suite - Kword
                    Case ".lasxml"
                        Return "application/vnd.las.las+xml"  'Laser App Enterprise
                    Case ".latex"
                        Return "application/x-latex"  'LaTeX
                    Case ".lbd"
                        Return "application/vnd.llamagraphics.life-balance.desktop"  'Life Balance - Desktop Edition
                    Case ".lbe"
                        Return "application/vnd.llamagraphics.life-balance.exchange+xml"  'Life Balance - Exchange Format
                    Case ".les"
                        Return "application/vnd.hhe.lesson-player"  'Archipelago Lesson Player
                    Case ".link66"
                        Return "application/vnd.route66.link66+xml"  'ROUTE 66 Location Based Services
                    Case ".lrm"
                        Return "application/vnd.ms-lrm"  'Microsoft Learning Resource Module
                    Case ".ltf"
                        Return "application/vnd.frogans.ltf"  'Frogans Player
                    Case ".lvp"
                        Return "audio/vnd.lucent.voice"  'Lucent Voice
                    Case ".lwp"
                        Return "application/vnd.lotus-wordpro"  'Lotus Wordpro
                    Case ".m21"
                        Return "application/mp21"  'MPEG-21
                    Case ".m3u"
                        Return "audio/x-mpegurl"  'M3U (Multimedia Playlist)
                    Case ".m3u8"
                        Return "application/vnd.apple.mpegurl"  'Multimedia Playlist Unicode
                    Case ".m4v"
                        Return "video/x-m4v"  'M4v
                    Case ".ma"
                        Return "application/mathematica"  'Mathematica Notebooks
                    Case ".mads"
                        Return "application/mads+xml"  'Metadata Authority Description Schema
                    Case ".mag"
                        Return "application/vnd.ecowin.chart"  'EcoWin Chart
                    Case ".mathml"
                        Return "application/mathml+xml"  'Mathematical Markup Language
                    Case ".mbk"
                        Return "application/vnd.mobius.mbk"  'Mobius Management Systems - Basket file
                    Case ".mbox"
                        Return "application/mbox"  'Mbox database files
                    Case ".mc1"
                        Return "application/vnd.medcalcdata"  'MedCalc
                    Case ".mcd"
                        Return "application/vnd.mcd"  'Micro CADAM Helix D&D
                    Case ".mcurl"
                        Return "text/vnd.curl.mcurl"  'Curl - Manifest File
                    Case ".mdb"
                        Return "application/x-msaccess"  'Microsoft Access
                    Case ".mdi"
                        Return "image/vnd.ms-modi"  'Microsoft Document Imaging Format
                    Case ".meta4"
                        Return "application/metalink4+xml"  'Metalink
                    Case ".mets"
                        Return "application/mets+xml"  'Metadata Encoding and Transmission Standard
                    Case ".mfm"
                        Return "application/vnd.mfmp"  'Melody Format for Mobile Platform
                    Case ".mgp"
                        Return "application/vnd.osgeo.mapguide.package"  'MapGuide DBXML
                    Case ".mgz"
                        Return "application/vnd.proteus.magazine"  'EFI Proteus
                    Case ".mid"
                        Return "audio/midi"  'MIDI - Musical Instrument Digital Interface
                    Case ".mif"
                        Return "application/vnd.mif"  'FrameMaker Interchange Format
                    Case ".mj2"
                        Return "video/mj2"  'Motion JPEG 2000
                    Case ".mlp"
                        Return "application/vnd.dolby.mlp"  'Dolby Meridian Lossless Packing
                    Case ".mmd"
                        Return "application/vnd.chipnuts.karaoke-mmd"  'Karaoke on Chipnuts Chipsets
                    Case ".mmf"
                        Return "application/vnd.smaf"  'SMAF File
                    Case ".mmr"
                        Return "image/vnd.fujixerox.edmics-mmr"  'EDMICS 2000
                    Case ".mny"
                        Return "application/x-msmoney"  'Microsoft Money
                    Case ".mods"
                        Return "application/mods+xml"  'Metadata Object Description Schema
                    Case ".movie"
                        Return "video/x-sgi-movie"  'SGI Movie
                    Case ".mp4"
                        Return "video/mp4"  'MPEG-4 Video
                    Case ".mp4a"
                        Return "audio/mp4"  'MPEG-4 Audio
                    Case ".mpc"
                        Return "application/vnd.mophun.certificate"  'Mophun Certificate
                    Case ".mpeg"
                        Return "video/mpeg"  'MPEG Video
                    Case ".mpga"
                        Return "audio/mpeg"  'MPEG Audio
                    Case ".mpkg"
                        Return "application/vnd.apple.installer+xml"  'Apple Installer Package
                    Case ".mpm"
                        Return "application/vnd.blueice.multipass"  'Blueice Research Multipass
                    Case ".mpn"
                        Return "application/vnd.mophun.application"  'Mophun VM
                    Case ".mpp"
                        Return "application/vnd.ms-project"  'Microsoft Project
                    Case ".mpy"
                        Return "application/vnd.ibm.minipay"  'MiniPay
                    Case ".mqy"
                        Return "application/vnd.mobius.mqy"  'Mobius Management Systems - Query File
                    Case ".mrc"
                        Return "application/marc"  'MARC Formats
                    Case ".mrcx"
                        Return "application/marcxml+xml"  'MARC21 XML Schema
                    Case ".mscml"
                        Return "application/mediaservercontrol+xml"  'Media Server Control Markup Language
                    Case ".mseq"
                        Return "application/vnd.mseq"  '3GPP MSEQ File
                    Case ".msf"
                        Return "application/vnd.epson.msf"  'QUASS Stream Player
                    Case ".msh"
                        Return "model/mesh"  'Mesh Data Type
                    Case ".msl"
                        Return "application/vnd.mobius.msl"  'Mobius Management Systems - Script Language
                    Case ".msty"
                        Return "application/vnd.muvee.style"  'Muvee Automatic Video Editing
                    Case ".mts"
                        Return "model/vnd.mts"  'Virtue MTS
                    Case ".mus"
                        Return "application/vnd.musician"  'MUsical Score Interpreted Code Invented for the ASCII designation of Notation
                    Case ".musicxml"
                        Return "application/vnd.recordare.musicxml+xml"  'Recordare Applications
                    Case ".mvb"
                        Return "application/x-msmediaview"  'Microsoft MediaView
                    Case ".mwf"
                        Return "application/vnd.mfer"  'Medical Waveform Encoding Format
                    Case ".mxf"
                        Return "application/mxf"  'Material Exchange Format
                    Case ".mxl"
                        Return "application/vnd.recordare.musicxml"  'Recordare Applications
                    Case ".mxml"
                        Return "application/xv+xml"  'MXML
                    Case ".mxs"
                        Return "application/vnd.triscape.mxs"  'Triscape Map Explorer
                    Case ".mxu"
                        Return "video/vnd.mpegurl"  'MPEG Url
                    Case ".n3"
                        Return "text/n3"  'Notation3
                    Case ".nbp"
                        Return "application/vnd.wolfram.player"  'Mathematica Notebook Player
                    Case ".nc"
                        Return "application/x-netcdf"  'Network Common Data Form (NetCDF)
                    Case ".ncx"
                        Return "application/x-dtbncx+xml"  'Navigation Control file for XML (for ePub)
                    Case ".n-gage"
                        Return "application/vnd.nokia.n-gage.symbian.install"  'N-Gage Game Installer
                    Case ".ngdat"
                        Return "application/vnd.nokia.n-gage.data"  'N-Gage Game Data
                    Case ".nlu"
                        Return "application/vnd.neurolanguage.nlu"  'neuroLanguage
                    Case ".nml"
                        Return "application/vnd.enliven"  'Enliven Viewer
                    Case ".nnd"
                        Return "application/vnd.noblenet-directory"  'NobleNet Directory
                    Case ".nns"
                        Return "application/vnd.noblenet-sealer"  'NobleNet Sealer
                    Case ".nnw"
                        Return "application/vnd.noblenet-web"  'NobleNet Web
                    Case ".npx"
                        Return "image/vnd.net-fpx"  'FlashPix
                    Case ".nsf"
                        Return "application/vnd.lotus-notes"  'Lotus Notes
                    Case ".oa2"
                        Return "application/vnd.fujitsu.oasys2"  'Fujitsu Oasys
                    Case ".oa3"
                        Return "application/vnd.fujitsu.oasys3"  'Fujitsu Oasys
                    Case ".oas"
                        Return "application/vnd.fujitsu.oasys"  'Fujitsu Oasys
                    Case ".obd"
                        Return "application/x-msbinder"  'Microsoft Office Binder
                    Case ".oda"
                        Return "application/oda"  'Office Document Architecture
                    Case ".odb"
                        Return "application/vnd.oasis.opendocument.database"  'OpenDocument Database
                    Case ".odc"
                        Return "application/vnd.oasis.opendocument.chart"  'OpenDocument Chart
                    Case ".odf"
                        Return "application/vnd.oasis.opendocument.formula"  'OpenDocument Formula
                    Case ".odft"
                        Return "application/vnd.oasis.opendocument.formula-template"  'OpenDocument Formula Template
                    Case ".odg"
                        Return "application/vnd.oasis.opendocument.graphics"  'OpenDocument Graphics
                    Case ".odi"
                        Return "application/vnd.oasis.opendocument.image"  'OpenDocument Image
                    Case ".odm"
                        Return "application/vnd.oasis.opendocument.text-master"  'OpenDocument Text Master
                    Case ".odp"
                        Return "application/vnd.oasis.opendocument.presentation"  'OpenDocument Presentation
                    Case ".ods"
                        Return "application/vnd.oasis.opendocument.spreadsheet"  'OpenDocument Spreadsheet
                    Case ".odt"
                        Return "application/vnd.oasis.opendocument.text"  'OpenDocument Text
                    Case ".oga"
                        Return "audio/ogg"  'Ogg Audio
                    Case ".ogv"
                        Return "video/ogg"  'Ogg Video
                    Case ".ogx"
                        Return "application/ogg"  'Ogg
                    Case ".onetoc"
                        Return "application/onenote"  'Microsoft OneNote
                    Case ".opf"
                        Return "application/oebps-package+xml"  'Open eBook Publication Structure
                    Case ".org"
                        Return "application/vnd.lotus-organizer"  'Lotus Organizer
                    Case ".osf"
                        Return "application/vnd.yamaha.openscoreformat"  'Open Score Format
                    Case ".osfpvg"
                        Return "application/vnd.yamaha.openscoreformat.osfpvg+xml"  'OSFPVG
                    Case ".otc"
                        Return "application/vnd.oasis.opendocument.chart-template"  'OpenDocument Chart Template
                    Case ".otf"
                        Return "application/x-font-otf"  'OpenType Font File
                    Case ".otg"
                        Return "application/vnd.oasis.opendocument.graphics-template"  'OpenDocument Graphics Template
                    Case ".oth"
                        Return "application/vnd.oasis.opendocument.text-web"  'Open Document Text Web
                    Case ".oti"
                        Return "application/vnd.oasis.opendocument.image-template"  'OpenDocument Image Template
                    Case ".otp"
                        Return "application/vnd.oasis.opendocument.presentation-template"  'OpenDocument Presentation Template
                    Case ".ots"
                        Return "application/vnd.oasis.opendocument.spreadsheet-template"  'OpenDocument Spreadsheet Template
                    Case ".ott"
                        Return "application/vnd.oasis.opendocument.text-template"  'OpenDocument Text Template
                    Case ".oxt"
                        Return "application/vnd.openofficeorg.extension"  'Open Office Extension
                    Case ".p"
                        Return "text/x-pascal"  'Pascal Source File
                    Case ".p10"
                        Return "application/pkcs10"  'PKCS #10 - Certification Request Standard
                    Case ".p12"
                        Return "application/x-pkcs12"  'PKCS #12 - Personal Information Exchange Syntax Standard
                    Case ".p7b"
                        Return "application/x-pkcs7-certificates"  'PKCS #7 - Cryptographic Message Syntax Standard (Certificates)
                    Case ".p7m"
                        Return "application/pkcs7-mime"  'PKCS #7 - Cryptographic Message Syntax Standard
                    Case ".p7r"
                        Return "application/x-pkcs7-certreqresp"  'PKCS #7 - Cryptographic Message Syntax Standard (Certificate Request Response)
                    Case ".p7s"
                        Return "application/pkcs7-signature"  'PKCS #7 - Cryptographic Message Syntax Standard
                    Case ".p8"
                        Return "application/pkcs8"  'PKCS #8 - Private-Key Information Syntax Standard
                    Case ".par"
                        Return "text/plain-bas"  'BAS Partitur Format
                    Case ".paw"
                        Return "application/vnd.pawaafile"  'PawaaFILE
                    Case ".pbd"
                        Return "application/vnd.powerbuilder6"  'PowerBuilder
                    Case ".pbm"
                        Return "image/x-portable-bitmap"  'Portable Bitmap Format
                    Case ".pcf"
                        Return "application/x-font-pcf"  'Portable Compiled Format
                    Case ".pcl"
                        Return "application/vnd.hp-pcl"  'HP Printer Command Language
                    Case ".pclxl"
                        Return "application/vnd.hp-pclxl"  'PCL 6 Enhanced (Formely PCL XL)
                    Case ".pcurl"
                        Return "application/vnd.curl.pcurl"  'CURL Applet
                    Case ".pcx"
                        Return "image/x-pcx"  'PCX Image
                    Case ".pdb"
                        Return "application/vnd.palm"  'PalmOS Data
                    Case ".pfa"
                        Return "application/x-font-type1"  'PostScript Fonts
                    Case ".pfr"
                        Return "application/font-tdpfr"  'Portable Font Resource
                    Case ".pgm"
                        Return "image/x-portable-graymap"  'Portable Graymap Format
                    Case ".pgn"
                        Return "application/x-chess-pgn"  'Portable Game Notation (Chess Games)
                    Case ".pgp"
                        Return "application/pgp-encrypted"  'Pretty Good Privacy
                    Case ".pic"
                        Return "image/x-pict"  'PICT Image
                    Case ".pjpeg"
                        Return "image/pjpeg"  'JPEG Image (Progressive)
                    Case ".pki"
                        Return "application/pkixcmp"  'Internet Public Key Infrastructure - Certificate Management Protocole
                    Case ".pkipath"
                        Return "application/pkix-pkipath"  'Internet Public Key Infrastructure - Certification Path
                    Case ".plb"
                        Return "application/vnd.3gpp.pic-bw-large"  '3rd Generation Partnership Project - Pic Large
                    Case ".plc"
                        Return "application/vnd.mobius.plc"  'Mobius Management Systems - Policy Definition Language File
                    Case ".plf"
                        Return "application/vnd.pocketlearn"  'PocketLearn Viewers
                    Case ".pls"
                        Return "application/pls+xml"  'Pronunciation Lexicon Specification
                    Case ".pml"
                        Return "application/vnd.ctc-posml"  'PosML
                    Case ".pnm"
                        Return "image/x-portable-anymap"  'Portable Anymap Image
                    Case ".portpkg"
                        Return "application/vnd.macports.portpkg"  'MacPorts Port System
                    Case ".potm"
                        Return "application/vnd.ms-powerpoint.template.macroenabled.12"  'Microsoft PowerPoint - Macro-Enabled Template File
                    Case ".potx"
                        Return "application/vnd.openxmlformats-officedocument.presentationml.template"  'Microsoft Office - OOXML - Presentation Template
                    Case ".ppam"
                        Return "application/vnd.ms-powerpoint.addin.macroenabled.12"  'Microsoft PowerPoint - Add-in file
                    Case ".ppd"
                        Return "application/vnd.cups-ppd"  'Adobe PostScript Printer Description File Format
                    Case ".ppm"
                        Return "image/x-portable-pixmap"  'Portable Pixmap Format
                    Case ".ppsm"
                        Return "application/vnd.ms-powerpoint.slideshow.macroenabled.12"  'Microsoft PowerPoint - Macro-Enabled Slide Show File
                    Case ".ppsx"
                        Return "application/vnd.openxmlformats-officedocument.presentationml.slideshow"  'Microsoft Office - OOXML - Presentation (Slideshow)
                    Case ".ppt"
                        Return "application/vnd.ms-powerpoint"  'Microsoft PowerPoint
                    Case ".pptm"
                        Return "application/vnd.ms-powerpoint.presentation.macroenabled.12"  'Microsoft PowerPoint - Macro-Enabled Presentation File
                    Case ".pptx"
                        Return "application/vnd.openxmlformats-officedocument.presentationml.presentation"  'Microsoft Office - OOXML - Presentation
                    Case ".prc"
                        Return "application/x-mobipocket-ebook"  'Mobipocket
                    Case ".pre"
                        Return "application/vnd.lotus-freelance"  'Lotus Freelance
                    Case ".prf"
                        Return "application/pics-rules"  'PICSRules
                    Case ".psb"
                        Return "application/vnd.3gpp.pic-bw-small"  '3rd Generation Partnership Project - Pic Small
                    Case ".psd"
                        Return "image/vnd.adobe.photoshop"  'Photoshop Document
                    Case ".psf"
                        Return "application/x-font-linux-psf"  'PSF Fonts
                    Case ".pskcxml"
                        Return "application/pskc+xml"  'Portable Symmetric Key Container
                    Case ".ptid"
                        Return "application/vnd.pvi.ptid1"  'Princeton Video Image
                    Case ".pub"
                        Return "application/x-mspublisher"  'Microsoft Publisher
                    Case ".pvb"
                        Return "application/vnd.3gpp.pic-bw-var"  '3rd Generation Partnership Project - Pic Var
                    Case ".pwn"
                        Return "application/vnd.3m.post-it-notes"  '3M Post It Notes
                    Case ".pya"
                        Return "audio/vnd.ms-playready.media.pya"  'Microsoft PlayReady Ecosystem
                    Case ".pyv"
                        Return "video/vnd.ms-playready.media.pyv"  'Microsoft PlayReady Ecosystem Video
                    Case ".qam"
                        Return "application/vnd.epson.quickanime"  'QuickAnime Player
                    Case ".qbo"
                        Return "application/vnd.intu.qbo"  'Open Financial Exchange
                    Case ".qfx"
                        Return "application/vnd.intu.qfx"  'Quicken
                    Case ".qps"
                        Return "application/vnd.publishare-delta-tree"  'PubliShare Objects
                    Case ".qt"
                        Return "video/quicktime"  'Quicktime Video
                    Case ".qxd"
                        Return "application/vnd.quark.quarkxpress"  'QuarkXpress
                    Case ".ram"
                        Return "audio/x-pn-realaudio"  'Real Audio Sound
                    Case ".rar"
                        Return "application/x-rar-compressed"  'RAR Archive
                    Case ".ras"
                        Return "image/x-cmu-raster"  'CMU Image
                    Case ".rcprofile"
                        Return "application/vnd.ipunplugged.rcprofile"  'IP Unplugged Roaming Client
                    Case ".rdf"
                        Return "application/rdf+xml"  'Resource Description Framework
                    Case ".rdz"
                        Return "application/vnd.data-vision.rdz"  'RemoteDocs R-Viewer
                    Case ".rep"
                        Return "application/vnd.businessobjects"  'BusinessObjects
                    Case ".res"
                        Return "application/x-dtbresource+xml"  'Digital Talking Book - Resource File
                    Case ".rgb"
                        Return "image/x-rgb"  'Silicon Graphics RGB Bitmap
                    Case ".rif"
                        Return "application/reginfo+xml"  'IMS Networks
                    Case ".rip"
                        Return "audio/vnd.rip"  'Hit'n'Mix
                    Case ".rl"
                        Return "application/resource-lists+xml"  'XML Resource Lists
                    Case ".rlc"
                        Return "image/vnd.fujixerox.edmics-rlc"  'EDMICS 2000
                    Case ".rld"
                        Return "application/resource-lists-diff+xml"  'XML Resource Lists Diff
                    Case ".rm"
                        Return "application/vnd.rn-realmedia"  'RealMedia
                    Case ".rmp"
                        Return "audio/x-pn-realaudio-plugin"  'Real Audio Sound
                    Case ".rms"
                        Return "application/vnd.jcp.javame.midlet-rms"  'Mobile Information Device Profile
                    Case ".rnc"
                        Return "application/relax-ng-compact-syntax"  'Relax NG Compact Syntax
                    Case ".rp9"
                        Return "application/vnd.cloanto.rp9"  'RetroPlatform Player
                    Case ".rpss"
                        Return "application/vnd.nokia.radio-presets"  'Nokia Radio Application - Preset
                    Case ".rpst"
                        Return "application/vnd.nokia.radio-preset"  'Nokia Radio Application - Preset
                    Case ".rq"
                        Return "application/sparql-query"  'SPARQL - Query
                    Case ".rs"
                        Return "application/rls-services+xml"  'XML Resource Lists
                    Case ".rsd"
                        Return "application/rsd+xml"  'Really Simple Discovery
                    Case ".rss", ".xml"
                        Return "application/rss+xml"  'RSS - Really Simple Syndication
                    Case ".rtf"
                        Return "application/rtf"  'Rich Text Format
                    Case ".rtx"
                        Return "text/richtext"  'Rich Text Format (RTF)
                    Case ".s"
                        Return "text/x-asm"  'Assembler Source File
                    Case ".saf"
                        Return "application/vnd.yamaha.smaf-audio"  'SMAF Audio
                    Case ".sbml"
                        Return "application/sbml+xml"  'Systems Biology Markup Language
                    Case ".sc"
                        Return "application/vnd.ibm.secure-container"  'IBM Electronic Media Management System - Secure Container
                    Case ".scd"
                        Return "application/x-msschedule"  'Microsoft Schedule+
                    Case ".scm"
                        Return "application/vnd.lotus-screencam"  'Lotus Screencam
                    Case ".scq"
                        Return "application/scvp-cv-request"  'Server-Based Certificate Validation Protocol - Validation Request
                    Case ".scs"
                        Return "application/scvp-cv-response"  'Server-Based Certificate Validation Protocol - Validation Response
                    Case ".scurl"
                        Return "text/vnd.curl.scurl"  'Curl - Source Code
                    Case ".sda"
                        Return "application/vnd.stardivision.draw"  'StarOffice - Draw
                    Case ".sdc"
                        Return "application/vnd.stardivision.calc"  'StarOffice - Calc
                    Case ".sdd"
                        Return "application/vnd.stardivision.impress"  'StarOffice - Impress
                    Case ".sdkm"
                        Return "application/vnd.solent.sdkm+xml"  'SudokuMagic
                    Case ".sdp"
                        Return "application/sdp"  'Session Description Protocol
                    Case ".sdw"
                        Return "application/vnd.stardivision.writer"  'StarOffice - Writer
                    Case ".see"
                        Return "application/vnd.seemail"  'SeeMail
                    Case ".seed"
                        Return "application/vnd.fdsn.seed"  'Digital Siesmograph Networks - SEED Datafiles
                    Case ".sema"
                        Return "application/vnd.sema"  'Secured eMail
                    Case ".semd"
                        Return "application/vnd.semd"  'Secured eMail
                    Case ".semf"
                        Return "application/vnd.semf"  'Secured eMail
                    Case ".ser"
                        Return "application/java-serialized-object"  'Java Serialized Object
                    Case ".setpay"
                        Return "application/set-payment-initiation"  'Secure Electronic Transaction - Payment
                    Case ".setreg"
                        Return "application/set-registration-initiation"  'Secure Electronic Transaction - Registration
                    Case ".sfd-hdstx"
                        Return "application/vnd.hydrostatix.sof-data"  'Hydrostatix Master Suite
                    Case ".sfs"
                        Return "application/vnd.spotfire.sfs"  'TIBCO Spotfire
                    Case ".sgl"
                        Return "application/vnd.stardivision.writer-global"  'StarOffice - Writer (Global)
                    Case ".sgml"
                        Return "text/sgml"  'Standard Generalized Markup Language (SGML)
                    Case ".sh"
                        Return "application/x-sh"  'Bourne Shell Script
                    Case ".shar"
                        Return "application/x-shar"  'Shell Archive
                    Case ".shf"
                        Return "application/shf+xml"  'S Hexdump Format
                    Case ".sis"
                        Return "application/vnd.symbian.install"  'Symbian Install Package
                    Case ".sit"
                        Return "application/x-stuffit"  'Stuffit Archive
                    Case ".sitx"
                        Return "application/x-stuffitx"  'Stuffit Archive
                    Case ".skp"
                        Return "application/vnd.koan"  'SSEYO Koan Play File
                    Case ".sldm"
                        Return "application/vnd.ms-powerpoint.slide.macroenabled.12"  'Microsoft PowerPoint - Macro-Enabled Open XML Slide
                    Case ".sldx"
                        Return "application/vnd.openxmlformats-officedocument.presentationml.slide"  'Microsoft Office - OOXML - Presentation (Slide)
                    Case ".slt"
                        Return "application/vnd.epson.salt"  'SimpleAnimeLite Player
                    Case ".sm"
                        Return "application/vnd.stepmania.stepchart"  'StepMania
                    Case ".smf"
                        Return "application/vnd.stardivision.math"  'StarOffice - Math
                    Case ".smi"
                        Return "application/smil+xml"  'Synchronized Multimedia Integration Language
                    Case ".snf"
                        Return "application/x-font-snf"  'Server Normal Format
                    Case ".spf"
                        Return "application/vnd.yamaha.smaf-phrase"  'SMAF Phrase
                    Case ".spl"
                        Return "application/x-futuresplash"  'FutureSplash Animator
                    Case ".spot"
                        Return "text/vnd.in3d.spot"  'In3D - 3DML
                    Case ".spp"
                        Return "application/scvp-vp-response"  'Server-Based Certificate Validation Protocol - Validation Policies - Response
                    Case ".spq"
                        Return "application/scvp-vp-request"  'Server-Based Certificate Validation Protocol - Validation Policies - Request
                    Case ".src"
                        Return "application/x-wais-source"  'WAIS Source
                    Case ".sru"
                        Return "application/sru+xml"  'Search/Retrieve via URL Response Format
                    Case ".srx"
                        Return "application/sparql-results+xml"  'SPARQL - Results
                    Case ".sse"
                        Return "application/vnd.kodak-descriptor"  'Kodak Storyshare
                    Case ".ssf"
                        Return "application/vnd.epson.ssf"  'QUASS Stream Player
                    Case ".ssml"
                        Return "application/ssml+xml"  'Speech Synthesis Markup Language
                    Case ".st"
                        Return "application/vnd.sailingtracker.track"  'SailingTracker
                    Case ".stc"
                        Return "application/vnd.sun.xml.calc.template"  'OpenOffice - Calc Template (Spreadsheet)
                    Case ".std"
                        Return "application/vnd.sun.xml.draw.template"  'OpenOffice - Draw Template (Graphics)
                    Case ".stf"
                        Return "application/vnd.wt.stf"  'Worldtalk
                    Case ".sti"
                        Return "application/vnd.sun.xml.impress.template"  'OpenOffice - Impress Template (Presentation)
                    Case ".stk"
                        Return "application/hyperstudio"  'Hyperstudio
                    Case ".stl"
                        Return "application/vnd.ms-pki.stl"  'Microsoft Trust UI Provider - Certificate Trust Link
                    Case ".str"
                        Return "application/vnd.pg.format"  'Proprietary P&G Standard Reporting System
                    Case ".stw"
                        Return "application/vnd.sun.xml.writer.template"  'OpenOffice - Writer Template (Text - HTML)
                    Case ".sub"
                        Return "image/vnd.dvb.subtitle"  'Close Captioning - Subtitle
                    Case ".sus"
                        Return "application/vnd.sus-calendar"  'ScheduleUs
                    Case ".sv4cpio"
                        Return "application/x-sv4cpio"  'System V Release 4 CPIO Archive
                    Case ".sv4crc"
                        Return "application/x-sv4crc"  'System V Release 4 CPIO Checksum Data
                    Case ".svc"
                        Return "application/vnd.dvb.service"  'Digital Video Broadcasting
                    Case ".svd"
                        Return "application/vnd.svd"  'SourceView Document
                    Case ".svg"
                        Return "image/svg+xml"  'Scalable Vector Graphics (SVG)
                    Case ".swf"
                        Return "application/x-shockwave-flash"  'Adobe Flash
                    Case ".swi"
                        Return "application/vnd.aristanetworks.swi"  'Arista Networks Software Image
                    Case ".sxc"
                        Return "application/vnd.sun.xml.calc"  'OpenOffice - Calc (Spreadsheet)
                    Case ".sxd"
                        Return "application/vnd.sun.xml.draw"  'OpenOffice - Draw (Graphics)
                    Case ".sxg"
                        Return "application/vnd.sun.xml.writer.global"  'OpenOffice - Writer (Text - HTML)
                    Case ".sxi"
                        Return "application/vnd.sun.xml.impress"  'OpenOffice - Impress (Presentation)
                    Case ".sxm"
                        Return "application/vnd.sun.xml.math"  'OpenOffice - Math (Formula)
                    Case ".sxw"
                        Return "application/vnd.sun.xml.writer"  'OpenOffice - Writer (Text - HTML)
                    Case ".t"
                        Return "text/troff"  'troff
                    Case ".tao"
                        Return "application/vnd.tao.intent-module-archive"  'Tao Intent
                    Case ".tar"
                        Return "application/x-tar"  'Tar File (Tape Archive)
                    Case ".tcap"
                        Return "application/vnd.3gpp2.tcap"  '3rd Generation Partnership Project - Transaction Capabilities Application Part
                    Case ".tcl"
                        Return "application/x-tcl"  'Tcl Script
                    Case ".teacher"
                        Return "application/vnd.smart.teacher"  'SMART Technologies Apps
                    Case ".tei"
                        Return "application/tei+xml"  'Text Encoding and Interchange
                    Case ".tex"
                        Return "application/x-tex"  'TeX
                    Case ".texinfo"
                        Return "application/x-texinfo"  'GNU Texinfo Document
                    Case ".tfi"
                        Return "application/thraud+xml"  'Sharing Transaction Fraud Data
                    Case ".tfm"
                        Return "application/x-tex-tfm"  'TeX Font Metric
                    Case ".thmx"
                        Return "application/vnd.ms-officetheme"  'Microsoft Office System Release Theme
                    Case ".tiff"
                        Return "image/tiff"  'Tagged Image File Format
                    Case ".tmo"
                        Return "application/vnd.tmobile-livetv"  'MobileTV
                    Case ".torrent"
                        Return "application/x-bittorrent"  'BitTorrent
                    Case ".tpl"
                        Return "application/vnd.groove-tool-template"  'Groove - Tool Template
                    Case ".tpt"
                        Return "application/vnd.trid.tpt"  'TRI Systems Config
                    Case ".tra"
                        Return "application/vnd.trueapp"  'True BASIC
                    Case ".trm"
                        Return "application/x-msterminal"  'Microsoft Windows Terminal Services
                    Case ".tsd"
                        Return "application/timestamped-data"  'Time Stamped Data Envelope
                    Case ".tsv"
                        Return "text/tab-separated-values"  'Tab Seperated Values
                    Case ".ttf"
                        Return "application/x-font-ttf"  'TrueType Font
                    Case ".ttl"
                        Return "text/turtle"  'Turtle (Terse RDF Triple Language)
                    Case ".twd"
                        Return "application/vnd.simtech-mindmapper"  'SimTech MindMapper
                    Case ".txd"
                        Return "application/vnd.genomatix.tuxedo"  'Genomatix Tuxedo Framework
                    Case ".txf"
                        Return "application/vnd.mobius.txf"  'Mobius Management Systems - Topic Index File
                    Case ".ufd"
                        Return "application/vnd.ufdl"  'Universal Forms Description Language
                    Case ".umj"
                        Return "application/vnd.umajin"  'UMAJIN
                    Case ".unityweb"
                        Return "application/vnd.unity"  'Unity 3d
                    Case ".uoml"
                        Return "application/vnd.uoml+xml"  'Unique Object Markup Language
                    Case ".uri"
                        Return "text/uri-list"  'URI Resolution Services
                    Case ".ustar"
                        Return "application/x-ustar"  'Ustar (Uniform Standard Tape Archive)
                    Case ".utz"
                        Return "application/vnd.uiq.theme"  'User Interface Quartz - Theme (Symbian)
                    Case ".uu"
                        Return "text/x-uuencode"  'UUEncode
                    Case ".uva"
                        Return "audio/vnd.dece.audio"  'DECE Audio
                    Case ".uvh"
                        Return "video/vnd.dece.hd"  'DECE High Definition Video
                    Case ".uvi"
                        Return "image/vnd.dece.graphic"  'DECE Graphic
                    Case ".uvm"
                        Return "video/vnd.dece.mobile"  'DECE Mobile Video
                    Case ".uvp"
                        Return "video/vnd.dece.pd"  'DECE PD Video
                    Case ".uvs"
                        Return "video/vnd.dece.sd"  'DECE SD Video
                    Case ".uvu"
                        Return "video/vnd.uvvu.mp4"  'DECE MP4
                    Case ".uvv"
                        Return "video/vnd.dece.video"  'DECE Video
                    Case ".vcd"
                        Return "application/x-cdlink"  'Video CD
                    Case ".vcf"
                        Return "text/x-vcard"  'vCard
                    Case ".vcg"
                        Return "application/vnd.groove-vcard"  'Groove - Vcard
                    Case ".vcs"
                        Return "text/x-vcalendar"  'vCalendar
                    Case ".vcx"
                        Return "application/vnd.vcx"  'VirtualCatalog
                    Case ".vis"
                        Return "application/vnd.visionary"  'Visionary
                    Case ".viv"
                        Return "video/vnd.vivo"  'Vivo
                    Case ".vsd"
                        Return "application/vnd.visio"  'Microsoft Visio
                    Case ".vsdx"
                        Return "application/vnd.visio2013"  'Microsoft Visio 2013
                    Case ".vsf"
                        Return "application/vnd.vsf"  'Viewport+
                    Case ".vtu"
                        Return "model/vnd.vtu"  'Virtue VTU
                    Case ".vxml"
                        Return "application/voicexml+xml"  'VoiceXML
                    Case ".wad"
                        Return "application/x-doom"  'Doom Video Game
                    Case ".wav"
                        Return "audio/x-wav"  'Waveform Audio File Format (WAV)
                    Case ".wax"
                        Return "audio/x-ms-wax"  'Microsoft Windows Media Audio Redirector
                    Case ".wbmp"
                        Return "image/vnd.wap.wbmp"  'WAP Bitamp (WBMP)
                    Case ".wbs"
                        Return "application/vnd.criticaltools.wbs+xml"  'Critical Tools - PERT Chart EXPERT
                    Case ".wbxml"
                        Return "application/vnd.wap.wbxml"  'WAP Binary XML (WBXML)
                    Case ".weba"
                        Return "audio/webm"  'Open Web Media Project - Audio
                    Case ".webm"
                        Return "video/webm"  'Open Web Media Project - Video
                    Case ".webp"
                        Return "image/webp"  'WebP Image
                    Case ".wg"
                        Return "application/vnd.pmi.widget"  'Qualcomm's Plaza Mobile Internet
                    Case ".wgt"
                        Return "application/widget"  'Widget Packaging and XML Configuration
                    Case ".wm"
                        Return "video/x-ms-wm"  'Microsoft Windows Media
                    Case ".wma"
                        Return "audio/x-ms-wma"  'Microsoft Windows Media Audio
                    Case ".wmd"
                        Return "application/x-ms-wmd"  'Microsoft Windows Media Player Download Package
                    Case ".wmf"
                        Return "application/x-msmetafile"  'Microsoft Windows Metafile
                    Case ".wml"
                        Return "text/vnd.wap.wml"  'Wireless Markup Language (WML)
                    Case ".wmlc"
                        Return "application/vnd.wap.wmlc"  'Compiled Wireless Markup Language (WMLC)
                    Case ".wmls"
                        Return "text/vnd.wap.wmlscript"  'Wireless Markup Language Script (WMLScript)
                    Case ".wmlsc"
                        Return "application/vnd.wap.wmlscriptc"  'WMLScript
                    Case ".wmv"
                        Return "video/x-ms-wmv"  'Microsoft Windows Media Video
                    Case ".wmx"
                        Return "video/x-ms-wmx"  'Microsoft Windows Media Audio/Video Playlist
                    Case ".wmz"
                        Return "application/x-ms-wmz"  'Microsoft Windows Media Player Skin Package
                    Case ".woff"
                        Return "application/x-font-woff"  'Web Open Font Format
                    Case ".wpd"
                        Return "application/vnd.wordperfect"  'Wordperfect
                    Case ".wpl"
                        Return "application/vnd.ms-wpl"  'Microsoft Windows Media Player Playlist
                    Case ".wps"
                        Return "application/vnd.ms-works"  'Microsoft Works
                    Case ".wqd"
                        Return "application/vnd.wqd"  'SundaHus WQ
                    Case ".wri"
                        Return "application/x-mswrite"  'Microsoft Wordpad
                    Case ".wrl"
                        Return "model/vrml"  'Virtual Reality Modeling Language
                    Case ".wsdl"
                        Return "application/wsdl+xml"  'WSDL - Web Services Description Language
                    Case ".wspolicy"
                        Return "application/wspolicy+xml"  'Web Services Policy
                    Case ".wtb"
                        Return "application/vnd.webturbo"  'WebTurbo
                    Case ".wvx"
                        Return "video/x-ms-wvx"  'Microsoft Windows Media Video Playlist
                    Case ".x3d"
                        Return "application/vnd.hzn-3d-crossword"  '3D Crossword Plugin
                    Case ".xap"
                        Return "application/x-silverlight-app"  'Microsoft Silverlight
                    Case ".xar"
                        Return "application/vnd.xara"  'CorelXARA
                    Case ".xbap"
                        Return "application/x-ms-xbap"  'Microsoft XAML Browser Application
                    Case ".xbd"
                        Return "application/vnd.fujixerox.docuworks.binder"  'Fujitsu - Xerox DocuWorks Binder
                    Case ".xbm"
                        Return "image/x-xbitmap"  'X BitMap
                    Case ".xdf"
                        Return "application/xcap-diff+xml"  'XML Configuration Access Protocol - XCAP Diff
                    Case ".xdm"
                        Return "application/vnd.syncml.dm+xml"  'SyncML - Device Management
                    Case ".xdp"
                        Return "application/vnd.adobe.xdp+xml"  'Adobe XML Data Package
                    Case ".xdssc"
                        Return "application/dssc+xml"  'Data Structure for the Security Suitability of Cryptographic Algorithms
                    Case ".xdw"
                        Return "application/vnd.fujixerox.docuworks"  'Fujitsu - Xerox DocuWorks
                    Case ".xenc"
                        Return "application/xenc+xml"  'XML Encryption Syntax and Processing
                    Case ".xer"
                        Return "application/patch-ops-error+xml"  'XML Patch Framework
                    Case ".xfdf"
                        Return "application/vnd.adobe.xfdf"  'Adobe XML Forms Data Format
                    Case ".xfdl"
                        Return "application/vnd.xfdl"  'Extensible Forms Description Language
                    Case ".xhtml"
                        Return "application/xhtml+xml"  'XHTML - The Extensible HyperText Markup Language
                    Case ".xif"
                        Return "image/vnd.xiff"  'eXtended Image File Format (XIFF)
                    Case ".xlam"
                        Return "application/vnd.ms-excel.addin.macroenabled.12"  'Microsoft Excel - Add-In File
                    Case ".xlsb"
                        Return "application/vnd.ms-excel.sheet.binary.macroenabled.12"  'Microsoft Excel - Binary Workbook
                    Case ".xlsm"
                        Return "application/vnd.ms-excel.sheet.macroenabled.12"  'Microsoft Excel - Macro-Enabled Workbook
                    Case ".xltm"
                        Return "application/vnd.ms-excel.template.macroenabled.12"  'Microsoft Excel - Macro-Enabled Template File
                    Case ".xltx"
                        Return "application/vnd.openxmlformats-officedocument.spreadsheetml.template"  'Microsoft Office - OOXML - Spreadsheet Template
                    Case ".xml"
                        Return "application/xml"  'XML - Extensible Markup Language
                    Case ".xo"
                        Return "application/vnd.olpc-sugar"  'Sugar Linux Application Bundle
                    Case ".xop"
                        Return "application/xop+xml"  'XML-Binary Optimized Packaging
                    Case ".xpi"
                        Return "application/x-xpinstall"  'XPInstall - Mozilla
                    Case ".xpm"
                        Return "image/x-xpixmap"  'X PixMap
                    Case ".xpr"
                        Return "application/vnd.is-xpr"  'Express by Infoseek
                    Case ".xpw"
                        Return "application/vnd.intercon.formnet"  'Intercon FormNet
                    Case ".xslt"
                        Return "application/xslt+xml"  'XML Transformations
                    Case ".xsm"
                        Return "application/vnd.syncml+xml"  'SyncML
                    Case ".xspf"
                        Return "application/xspf+xml"  'XSPF - XML Shareable Playlist Format
                    Case ".xul"
                        Return "application/vnd.mozilla.xul+xml"  'XUL - XML User Interface Language
                    Case ".xwd"
                        Return "image/x-xwindowdump"  'X Window Dump
                    Case ".xyz"
                        Return "chemical/x-xyz"  'XYZ File Format
                    Case ".yaml"
                        Return "text/yaml"  'YAML Ain't Markup Language / Yet Another Markup Language
                    Case ".yang"
                        Return "application/yang"  'YANG Data Modeling Language
                    Case ".yin"
                        Return "application/yin+xml"  'YIN (YANG - XML)
                    Case ".zaz"
                        Return "application/vnd.zzazz.deck+xml"  'Zzazz Deck
                    Case ".zir"
                        Return "application/vnd.zul"  'Z.U.L. Geometry
                    Case ".zmm"
                        Return "application/vnd.handheld-entertainment+xml"  'ZVUE Media Manager
                    Case Else
                        Return "text/plain"
                End Select
            End If
            Return _mimeType
        End Get
    End Property
#End Region

    Public Sub New(ByVal context As HttpContext)
        _HTTP = context
        _isBinary = False
        _isFile = False
    End Sub

    Public Sub DownloadFile(ByVal saveAsFile As String, ByVal filePath As String, ByVal forceDialog As Boolean, Optional ByVal mimeType As String = "text/plain")
        _saveAsFile = saveAsFile
        _filePath = filePath
        _forceDialog = forceDialog
        _mimeType = mimeType
        _isFile = True
        Download()
    End Sub

    Public Sub DownloadFile(ByVal saveAsFile As String, ByVal forceDialog As Boolean, ByVal data As String, Optional ByVal mimeType As String = "text/plain")
        _saveAsFile = saveAsFile
        _forceDialog = forceDialog
        _mimeType = mimeType
        _dataAsString = data
        Download()
    End Sub

    Public Sub DownloadFile(ByVal saveAsFile As String, ByVal forceDialog As Boolean, ByVal data As Byte(), Optional ByVal mimeType As String = "application/pdf")
        _saveAsFile = saveAsFile
        _forceDialog = forceDialog
        _mimeType = mimeType
        _isBinary = True
        _dataAsBytes = data
        Download()
    End Sub

    Private Sub Download()
        _HTTP.Response.Clear()
        _HTTP.Response.ClearHeaders()

        _HTTP.Response.Cache.SetNoStore()
        _HTTP.Response.Cache.SetCacheability(HttpCacheability.Private)
        _HTTP.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1))
        _HTTP.Response.ContentType = resolveMimeType

        If _forceDialog Then
            _HTTP.Response.AppendHeader("content-disposition", "attachment;filename=" + _saveAsFile)
        Else
            _HTTP.Response.AppendHeader("content-disposition", "inline;filename=" + _saveAsFile)
        End If

        ' _HTTP.Response.AppendHeader("p3p", "CP=""IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT""")

        writeData()
        _HTTP.Response.Flush()
        '_HTTP.Response.Close()
        _HTTP.Response.End()
    End Sub

    Private Sub writeData()
        If _isFile Then
            _HTTP.Response.WriteFile(_filePath)
            _HTTP.Response.AddHeader("Content-Length", New System.IO.FileInfo(_filePath).Length.ToString)
            Return
        End If
        If _isBinary Then
            _HTTP.Response.BinaryWrite(_dataAsBytes)
            _HTTP.Response.AddHeader("Content-Length", _dataAsBytes.Length.ToString)
        Else
            _HTTP.Response.Write(_dataAsString)
            _HTTP.Response.AddHeader("Content-Length", _dataAsString.Length.ToString)
        End If
    End Sub

End Class
