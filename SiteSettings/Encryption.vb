Imports System
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.IO
Imports System.Collections.Generic

Public Class Cryptography

    Public Shared Function oldEncryption(ByVal as_string As String) As String
        Dim li_nums() As Integer = New Integer() {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233}
        Dim ls_alpha As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        Dim ls_cipher As String = "9876543210zyxwvutsrqponmlkjihgfedcbaZYXWVUTSRQPONMLKJIHGFEDCBA"
        Dim ls_out As String = String.Empty
        Dim li_loop, li_pos, li_cnt, li_offset As Integer

        For li_loop = 0 To as_string.Length - 1
            li_pos = 999
            For li_cnt = 0 To 61
                If as_string.Substring(li_loop, 1) = ls_alpha.Substring(li_cnt, 1) Then
                    li_pos = li_cnt
                    Exit For
                End If
            Next
            If li_pos = 999 Then
                ls_out += as_string.Substring(li_loop, 1)
            Else
                li_offset = li_pos + li_nums(49 - (li_loop Mod 50))
                Do While li_offset > 61
                    li_offset -= 62
                Loop
                ls_out += ls_cipher.Substring(li_offset, 1)
            End If
        Next
        Return ls_out
    End Function

    Private _key As String = "xKIketwq"
    Private _iv As String = "t7unbdg4"

    Public WriteOnly Property key() As String
        Set(ByVal value As String)
            If value Is Nothing Or value.Length < 6 Then Return
            _key = value
        End Set
    End Property

    Public WriteOnly Property iv() As String
        Set(ByVal value As String)
            If value Is Nothing Or value.Length < 6 Then Return
            _iv = value
        End Set
    End Property

    Sub New()
        _key = "xKIketwq"
        _iv = "t7unbdg4"
    End Sub

    Public Function encrypt(ByVal aToEncrypt As String) As String
        Return encrypt(aToEncrypt, _key, _iv)
    End Function

    Public Function encrypt(ByVal aToEncrypt As String, ByVal aKey As String, ByVal aIV As String) As String
        Dim original As String = aToEncrypt
        Dim textConverter As New ASCIIEncoding
        Dim rc2CSP As New RC2CryptoServiceProvider
        Dim encrypted() As Byte
        Dim toEncrypt() As Byte
        Dim hexString As String = String.Empty
        Dim key() As Byte
        Dim IV() As Byte

        'create the key and IV
        rc2CSP.Key = textConverter.GetBytes(aKey)
        rc2CSP.IV = textConverter.GetBytes(aIV)

        'Get the key and IV.
        key = rc2CSP.Key
        IV = rc2CSP.IV

        Try
            'Get an encryptor.
            Dim encryptor As ICryptoTransform = rc2CSP.CreateEncryptor(key, IV)

            'Encrypt the data.
            Dim msEncrypt As New MemoryStream
            Dim csEncrypt As New CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)

            'Convert the data to a byte array.
            toEncrypt = textConverter.GetBytes(original)

            'Write all data to the crypto stream and flush it.
            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length)
            csEncrypt.FlushFinalBlock()

            'Get encrypted array of bytes.
            encrypted = msEncrypt.ToArray()
            'convert to hexidecimal

            hexString = HexEncoding.BytesToString(encrypted)
        Catch ex As Exception

        End Try

        Return hexString

    End Function

    Public Function decrypt(ByVal aToDecrypt As String) As String
        Return decrypt(aToDecrypt, _key, _iv)
    End Function

    Public Function decrypt(ByVal aHexValue As String, ByVal aKey As String, ByVal aIV As String) As String
        Dim HexString As String = aHexValue
        Dim decryptedString As String = String.Empty
        Dim TextConverter As New ASCIIEncoding
        Dim rc2CSP As New RC2CryptoServiceProvider
        Dim EncrytedByteArray() As Byte
        Dim Key() As Byte
        Dim IV() As Byte

        'create the key and IV
        rc2CSP.Key = TextConverter.GetBytes(aKey)
        rc2CSP.IV = TextConverter.GetBytes(aIV)

        'Get the key and IV.
        Key = rc2CSP.Key
        IV = rc2CSP.IV

        Try
            Dim BytesFromHex() As Byte = HexEncoding.GetBytes(HexString, 0)
            Dim decryptor As ICryptoTransform = rc2CSP.CreateDecryptor(Key, IV)
            'Now decrypt the previously encrypted message using the decryptor
            ' obtained in the above step.
            Dim msDecrypt As New MemoryStream(BytesFromHex)
            Dim csDecrypt As New CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)

            EncrytedByteArray = New Byte(BytesFromHex.Length - 1) {}

            'Read the data out of the crypto stream.
            csDecrypt.Read(EncrytedByteArray, 0, EncrytedByteArray.Length)

            'Convert the byte array back into a string.
            decryptedString = TextConverter.GetString(EncrytedByteArray)

        Catch ex As Exception

        End Try

        Dim regex As New Regex("\0", RegexOptions.None)
        decryptedString = regex.Replace(decryptedString, "")

        Return decryptedString

    End Function

End Class

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
            j += 2
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


