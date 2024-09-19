
Imports System.IO
Imports System.Web.Script.Serialization
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Public Class GenericFunctions

#Region "Images"

    Public Function ResizeIfImage(ByVal filename As String, ByVal Obj() As Byte) As Byte()

        Select Case Path.GetExtension(filename).ToLower
            Case ".jpg", ".jpeg", ".bmp", ".gif", ".png", ".tiff"

                Dim imgWidth As Integer = 0
                Dim imgHeight As Integer = 0
                Dim imgFactor As Decimal = 1.0

                Using input = New MemoryStream(Obj)
                    Using img As System.Drawing.Image = System.Drawing.Image.FromStream(input)

                        If img.Width > 1000 Then
                            imgFactor = img.Width / 1000
                        End If

                        ' perform your transformations
                        imgWidth = img.Width / imgFactor
                        imgHeight = img.Height / imgFactor

                        Dim objOptImage As System.Drawing.Image = New System.Drawing.Bitmap(imgWidth, imgHeight,
                                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb)

                        Using oGraphic As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(objOptImage)
                            With oGraphic
                                Dim oRectangle As New System.Drawing.Rectangle(0, 0, imgWidth, imgHeight)

                                .DrawImage(img, oRectangle)
                            End With
                        End Using

                        Using output = New MemoryStream()
                            objOptImage.Save(output, img.RawFormat)

                            Return output.ToArray()
                        End Using
                    End Using
                End Using

        End Select

        Return Obj

    End Function

    Public Enum SpecifyAxis
        XAxis = 1
        YAxis = 2
        Both = 3
    End Enum

    ' Resize image to specific size
    Public Shared Function ResizeImage(ByVal img As Image,
                                       ByVal size As Size,
                                       Optional ByVal Specify As SpecifyAxis = SpecifyAxis.Both) As Image
        Dim newWidth As Integer
        Dim newHeight As Integer
        Dim originalWidth As Integer = img.Width
        Dim originalHeight As Integer = img.Height

        Select Case Specify
            Case SpecifyAxis.Both
                newWidth = size.Width
                newHeight = size.Height
            Case SpecifyAxis.XAxis
                ' set the specific width and apply same percentage change to the height
                newWidth = size.Width
                newHeight = originalHeight * (CSng(size.Width) / CSng(originalWidth))
            Case SpecifyAxis.YAxis
                ' set the specific height and apply same percentage change to the width
                newHeight = size.Height
                newWidth = originalWidth * (CSng(size.Height) / CSng(originalHeight))
        End Select

        Dim newImage As Image = New Bitmap(newWidth, newHeight, Imaging.PixelFormat.Format32bppArgb)

        Using graphicsHandle As Graphics = Graphics.FromImage(newImage)
            With graphicsHandle
                Dim oRectangle As New Rectangle(0, 0, newWidth, newHeight)
                .DrawImage(img, oRectangle)
            End With

            graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
            graphicsHandle.DrawImage(img, 0, 0, newWidth, newHeight)
        End Using

        Return newImage
    End Function

    Public Shared Function BytesToImage(byteArray As Byte()) As Image
        Using ms As New MemoryStream(byteArray)
            Return Image.FromStream(ms)
        End Using
    End Function

    Public Shared Function ImageToBytes(image As Image, filename As String) As Byte()
        Using ms As New MemoryStream()
            Dim fmt As ImageFormat = GetImageFormat(Path.GetExtension(filename))
            image.Save(ms, fmt)
            Return ms.ToArray()
        End Using
    End Function

    Public Shared Function ImageToBase64(ByVal img As Image) As String
        Dim converter As New ImageConverter
        Dim ImageBytes As Byte() = converter.ConvertTo(img, GetType(Byte()))
        Dim base64String As String = Convert.ToBase64String(ImageBytes)
        Return base64String
    End Function

    Public Shared Function CreateBase64ImageFromText(ByVal text As String, Optional ByVal colour As Nullable(Of Color) = Nothing) As String
        If Not colour.HasValue Then colour = Color.Black

        Using font As New Font("Arial", 12)
            ' Create a dummy bitmap just to get a graphics object
            Using dummyImg As New Bitmap(1, 1)
                Using drawing As Graphics = Graphics.FromImage(dummyImg)
                    ' Measure the string to see how big the image needs to be
                    Dim textSize As SizeF = drawing.MeasureString(text, font)

                    ' Add a small margin around the text
                    Dim margin As Integer = 5
                    Dim imgWidth As Integer = CInt(Math.Ceiling(textSize.Width)) + (margin * 2)
                    Dim imgHeight As Integer = CInt(Math.Ceiling(textSize.Height)) + (margin * 2)

                    ' Create a new image of the right size
                    Using img As New Bitmap(imgWidth, imgHeight)
                        Using g As Graphics = Graphics.FromImage(img)
                            g.Clear(Color.Transparent)
                            g.SmoothingMode = SmoothingMode.AntiAlias
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias

                            ' Draw the text in the center
                            Dim textX As Integer = margin
                            Dim textY As Integer = margin
                            Using textBrush As New SolidBrush(colour.Value)
                                g.DrawString(text, font, textBrush, textX, textY)
                            End Using
                        End Using

                        ' Convert to base64
                        Using ms As New MemoryStream()
                            img.Save(ms, ImageFormat.Png)
                            Dim byteImage As Byte() = ms.ToArray()
                            Dim base64Image As String = Convert.ToBase64String(byteImage)
                            Return $"data:image/png;base64,{base64Image}"
                        End Using
                    End Using
                End Using
            End Using
        End Using
    End Function

    Public Shared Function CreateBase64CheckerPattern(Optional ByVal blockSize As Integer = 10, Optional ByVal width As Integer = 50, Optional ByVal height As Integer = 30) As String
        ' Define the two colors for the checker pattern
        Dim color1 As Color = Color.FromArgb(255, 255, 255, 252) ' Light color (#FFFFFC)
        Dim color2 As Color = Color.LightGray ' Alternate color (Light gray)

        ' Create a new image of the right size
        Using img As New Bitmap(width, height)
            Using g As Graphics = Graphics.FromImage(img)
                g.Clear(Color.Transparent)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

                ' Create checkerboard pattern
                For y As Integer = 0 To height - 1 Step blockSize
                    For x As Integer = 0 To width - 1 Step blockSize
                        Dim rect As New Rectangle(x, y, blockSize, blockSize)
                        ' Alternate colors
                        Dim color As Color = If((x / blockSize + y / blockSize) Mod 2 = 0, color1, color2)
                        Using brush As New SolidBrush(color)
                            g.FillRectangle(brush, rect)
                        End Using
                    Next
                Next
            End Using

            ' Convert to base64
            Using ms As New MemoryStream()
                img.Save(ms, ImageFormat.Png)
                Dim byteImage As Byte() = ms.ToArray()
                Dim base64Image As String = Convert.ToBase64String(byteImage)
                Return $"data:image/png;base64,{base64Image}"
            End Using
        End Using
    End Function


    Private Shared Function GetImageFormatFromPath(filePath As String) As ImageFormat
        Return GetImageFormat(Path.GetExtension(filePath).ToLower())
    End Function
    Private Shared Function GetImageFormat(fileExtension As String) As ImageFormat
        Select Case fileExtension.ToLower()
            Case ".bmp"
                Return ImageFormat.Bmp
            Case ".gif"
                Return ImageFormat.Gif
            Case ".jpg", ".jpeg"
                Return ImageFormat.Jpeg
            Case ".png"
                Return ImageFormat.Png
            Case ".tif", ".tiff"
                Return ImageFormat.Tiff
            Case Else
                Throw New ArgumentException("Unknown file extension")
        End Select
    End Function

    Public Shared Function ReduceImageSize(imageData As Byte(), targetSizeInBytes As Long, outputPath As String, Optional minQuality As Long = 70) As Byte()
        ' Convert the byte array to a MemoryStream
        Using inputStream As MemoryStream = New MemoryStream(imageData)
            ' Call the existing ReduceImageSize function
            Using reducedSizeStream As MemoryStream = ReduceImageSize(inputStream, targetSizeInBytes, outputPath, minQuality)
                ' Convert the resulting MemoryStream back to a byte array
                Return reducedSizeStream.ToArray()
            End Using
        End Using
    End Function

    Public Shared Function ReduceImageSize(inputImageStream As MemoryStream, targetSizeInBytes As Long, outputPath As String, Optional minQuality As Long = 70) As MemoryStream
        ' Check if the input image already meets the size requirement
        If inputImageStream.Length <= targetSizeInBytes Then
            Return inputImageStream
        End If

        Dim scaleFactor As Single = 1
        Dim currentSize As Long = inputImageStream.Length ' Initialize with input size
        Dim quality As Long = 100
        Dim ms As MemoryStream = New MemoryStream() ' Initialize to avoid "Use of unassigned local variable" error

        Dim imageFormat As ImageFormat = GetImageFormatFromPath(outputPath)

        Using inputImage As Image = Image.FromStream(inputImageStream)
            Using resizedImage As Bitmap = New Bitmap(inputImage)
                Do
                    ms.Dispose() ' Dispose previous MemoryStream to avoid memory leak
                    ms = New MemoryStream()
                    scaleFactor *= 0.9F ' Reduce the scale factor by 10% each iteration
                    Dim newWidth As Integer = CInt(resizedImage.Width * scaleFactor)
                    Dim newHeight As Integer = CInt(resizedImage.Height * scaleFactor)

                    Using scaledImage As Bitmap = New Bitmap(resizedImage, newWidth, newHeight)
                        Using g As Graphics = Graphics.FromImage(scaledImage)
                            g.CompositingQuality = CompositingQuality.HighQuality
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic
                            g.SmoothingMode = SmoothingMode.HighQuality
                            g.DrawImage(resizedImage, 0, 0, newWidth, newHeight)
                        End Using

                        Dim imgEncoder As ImageCodecInfo = GetEncoder(imageFormat)
                        Dim encoderParameters As EncoderParameters = New EncoderParameters(1)
                        encoderParameters.Param(0) = New EncoderParameter(Encoder.Quality, quality)

                        scaledImage.Save(ms, imgEncoder, encoderParameters)
                        currentSize = ms.Length

                        If currentSize > targetSizeInBytes AndAlso quality > minQuality Then
                            quality -= 10 ' Reduce the quality by 10% each iteration
                        ElseIf scaleFactor > 0.1 Then
                            quality = 100 ' Reset the quality to 100 for the next iteration
                        Else
                            Exit Do
                        End If
                    End Using
                Loop While currentSize > targetSizeInBytes
            End Using
        End Using

        Return ms
    End Function



    Private Shared Function GetEncoder(format As ImageFormat) As ImageCodecInfo
        Dim codecs As ImageCodecInfo() = ImageCodecInfo.GetImageDecoders()
        For Each codec As ImageCodecInfo In codecs
            If codec.FormatID = format.Guid Then
                Return codec
            End If
        Next codec
        Return Nothing
    End Function

#End Region

#Region "Logging"
    Public Sub TCFLogger(ByVal text As String)
        Dim filepath As String = HttpContext.Current.Server.MapPath("..\logs")
        Dim sw As StreamWriter
        sw = New StreamWriter(filepath + "\debug.log", True)
        sw.WriteLine(text)
        sw.Close()
    End Sub
#End Region

#Region "Serialise JSON"

    Public Shared Function ConvertToJSON(ByVal DT As DataTable) As Object
        Dim jsSerializer As JavaScriptSerializer = New JavaScriptSerializer
        jsSerializer.MaxJsonLength = Int32.MaxValue

        Dim childRow As Dictionary(Of String, Object)
        Dim parentRow As New List(Of Dictionary(Of String, Object))


        If DT IsNot Nothing Then
            For Each row As DataRow In DT.Rows
                childRow = New Dictionary(Of String, Object)
                For Each col As DataColumn In DT.Columns
                    childRow.Add(col.ColumnName, row(col))
                Next
                parentRow.Add(childRow)
            Next
        End If

        Return jsSerializer.Serialize(parentRow)
    End Function

    Public Shared Function ConvertToJSON(ByVal ex As Exception) As Object
        ' record the error
        LogException.HandleException(ex, False)

        Dim json As New Dictionary(Of String, String)

        json.Add("error_message", ex.Message)
        json.Add("error_stackTrace", ex.StackTrace)

        Return json
    End Function

#End Region

End Class

