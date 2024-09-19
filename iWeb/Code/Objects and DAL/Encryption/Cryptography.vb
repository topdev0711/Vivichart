
Imports System.Security.Cryptography
Imports System.IO

Public Class Cryptography

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
        key = "xKIketwq"
        iv = "t7unbdg4"
    End Sub

    Sub New(ByVal aKey As String, ByVal aIv As String)
        key = aKey
        iv = aIv
    End Sub

    Public Function DBEncrypt(ByVal StrToEncrypt As String) As String
        Return encrypt(StrToEncrypt, "A9sai5lk", "kd82lc8q")
    End Function
    Public Function DBDecrypt(ByVal StrToEncrypt As String) As String
        Return decrypt(StrToEncrypt, "A9sai5lk", "kd82lc8q")
    End Function

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

    Public Function GenerateSaltAndHash(ByVal Password As String) As String
        Dim PBKDF2 As New Rfc2898DeriveBytes(password, 32, 10000)
        Dim hash() As Byte = PBKDF2.GetBytes(20)
        Dim salt() As Byte = PBKDF2.Salt

        Return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash)
    End Function

    Public Function CheckSaltedPassword(ByVal newPassword As String, ByVal encSalt As String, ByVal encHash As String) As Boolean
        Dim OrgSalt() As Byte = Convert.FromBase64String(encSalt)
        Dim OrgHash() As Byte = Convert.FromBase64String(encHash)
        Dim NewHash() As Byte = New Rfc2898DeriveBytes(newPassword, OrgSalt, 10000).GetBytes(20)

        Return Convert.ToBase64String(OrgHash) = Convert.ToBase64String(NewHash)
    End Function


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



End Class

