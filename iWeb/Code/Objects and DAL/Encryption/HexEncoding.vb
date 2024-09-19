Imports System.Collections.Generic

Public Class HexEncoding

    Public Sub New()
    End Sub
	
    Public Shared Function GetByteCount(ByVal hexString As String) As Integer
        Dim numHexChars As Integer = 0
        Dim c As Char
        Dim i As Integer
        ' remove all none A-F, 0-9, characters
        For i = 0 To hexString.Length - 1
            c = hexString(i)
            If IsHexDigit(c) Then numHexChars += 1
        Next
        ' if odd number of characters, discard last character
        If numHexChars Mod 2 <> 0 Then
            numHexChars -= 1
        End If

        Return numHexChars / 2 ' 2 characters per byte

    End Function



    ' Creates a byte array from the hexadecimal string. Each two characters are combined
    ' to create one byte. First two hexadecimal characters become first byte in returned array.
    ' Non-hexadecimal characters are ignored.   
    ' returns byte array, in the same left-to-right order as the hexString
    Public Shared Function GetBytes(ByVal hexString As String, ByVal discarded As Integer) As Byte()
        discarded = 0
        Dim newString As String = String.Empty
        Dim c As Char

        ' remove all none A-F, 0-9, characters
        Dim i As Integer
        For i = 0 To hexString.Length - 1
            c = hexString(i)
            If (IsHexDigit(c)) Then
                newString += c
            Else
                discarded += 1
            End If
        Next

        ' if odd number of characters, discard last character
        If (newString.Length Mod 2 <> 0) Then
            discarded += 1
            newString = newString.Substring(0, newString.Length - 1)
        End If

        Dim byteLength As Integer = newString.Length / 2
        Dim bytes(byteLength - 1) As Byte

        Dim Hex As String
        Dim j As Integer = 0

        For i = 0 To bytes.Length - 1
            Hex = New String(New Char() {newString(j), newString(j + 1)})
            bytes(i) = HexToByte(Hex)
            j = j + 2
        Next

        Return bytes

    End Function

    Public Shared Function BytesToString(ByVal bytes As Byte()) As String
        Dim hexString As String = String.Empty
        Dim i As Integer
        For i = 0 To bytes.Length - 1
            hexString += bytes(i).ToString("X2")
        Next
        Return hexString
    End Function

    ' Determines if given string is in proper hexadecimal string format
    Public Shared Function InHexFormat(ByVal hexString As String) As Boolean
        Dim hexFormat As Boolean = True
        Dim digit As Char

        For Each digit In hexString
            If Not IsHexDigit(digit) Then
                hexFormat = False
                Exit For
            End If
        Next

        Return hexFormat
    End Function


    ' Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)   
    Public Shared Function IsHexDigit(ByVal c As Char) As Boolean
        Dim numChar As Integer
        Dim numA As Integer = Convert.ToInt32(Convert.ToChar("A"))
        Dim num1 As Integer = Convert.ToInt32(Convert.ToChar("0"))
        c = Char.ToUpper(c)
        numChar = Convert.ToInt32(c)
        If (numChar >= numA And numChar < (numA + 6)) Then Return True
        If (numChar >= num1 And numChar < (num1 + 10)) Then Return True
        Return False
    End Function



    ' Converts 1 or 2 character string into equivalant byte value   
    Private Shared Function HexToByte(ByVal hex As String) As Byte
        If hex.Length > 2 Or hex.Length <= 0 Then
            Throw New ArgumentException("hex must be 1 or 2 characters in length")
        End If
        Dim newByte As Byte = Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber)
        Return newByte
    End Function


End Class
