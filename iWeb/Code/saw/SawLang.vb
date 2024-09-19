Imports System.Threading
Imports System.Collections.Concurrent
Imports Microsoft.SqlServer.Server

Public Class SawLang
    Private Shared _cache As New ConcurrentDictionary(Of String, ConcurrentDictionary(Of String, TranslationEntry))()

    Shared Sub New()
        BuildCache()
    End Sub

    Public Class TranslationEntry
        Public Property LanguageRef As String
        Public Property TranslatedText As String
    End Class

    Private Shared ReadOnly lockObject As New Object()
    Private Shared Property CurrentWatermark() As Integer
        Get
            If HttpContext.Current.Application("CurrentWatermark") Is Nothing Then
                Return 0
            End If
            Return CInt(HttpContext.Current.Application("CurrentWatermark"))
        End Get
        Set(value As Integer)
            SyncLock lockObject
                HttpContext.Current.Application("CurrentWatermark") = value
            End SyncLock
        End Set
    End Property

    Private Shared Function GetLanguageWatermark() As Integer
        Return IawDB.execScalar("select cache_num from dbo.cache_list where cache_id = 'qlang_text'")
    End Function

    Private Shared Sub BuildCache()
        Dim newCache As New ConcurrentDictionary(Of String, ConcurrentDictionary(Of String, TranslationEntry))()

        ' Get data from the database
        ' ...
        Dim DT As DataTable
        Using DB As New IawDB
            DT = DB.GetTable("
            SELECT GBR.text_data As base_text,
                   GBR.text_key,
                   IsNull(QT.language_ref,QL.language_ref) as language_ref,
                   IsNull(QT.text_data,GBR.text_data) as text_data
              FROM dbo.qlang_text GBR  
                   CROSS JOIN dbo.qlang QL
                    LEFT OUTER JOIN dbo.qlang_text QT
                      ON QT.text_key = GBR.text_key
                     AND QT.language_ref = QL.language_ref
                     AND QT.text_status = '05'
                WHERE GBR.language_ref = 'GBR'
            ")
        End Using

        For Each row As DataRow In DT.Rows
            Dim baseText As String = row("base_text").ToString()
            Dim languageRef As String = row("language_ref").ToString()
            Dim textData As String = row("text_data").ToString()
            Dim textKey As String = row("text_key").ToString()
            Dim DictKey As String

            Dim translationEntry As New TranslationEntry With {
                .LanguageRef = languageRef,
                .TranslatedText = textData
            }

            ' if text_key isn't 64 bytes, then we use the text_key as the dictionary key instead of the base text
            If textKey.Length = 64 Then
                DictKey = baseText
            Else
                DictKey = textKey
            End If

            If Not newCache.ContainsKey(DictKey) Then
                newCache.TryAdd(DictKey, New ConcurrentDictionary(Of String, TranslationEntry)())
            End If
            newCache(DictKey).TryAdd(languageRef, translationEntry)
        Next

        Interlocked.Exchange(_cache, newCache)
    End Sub

    Public Shared Function GetBaseMsg(MsgRef As String) As String
        Dim i As Integer = GetLanguageWatermark()
        If i > CurrentWatermark Then
            SawUtil.Log(String.Format("GetBaseMsg - Rebuild Language Cache: {0}", i))
            BuildCache()
            CurrentWatermark = i
        End If

        If MsgRef.StartsWith("::") Then
            Dim messageRef As String = MsgRef.Substring(2)

            If _cache.ContainsKey(messageRef) AndAlso _cache(messageRef).ContainsKey("GBR") Then
                Return _cache(messageRef)("GBR").TranslatedText
            Else
                Return String.Format(ctx.Translate("::LT_S0342"), messageRef) ' Message {0} is missing
            End If
        End If
        Return MsgRef
    End Function

    Shared Function IsIntegerOrBracketed(input As String) As Boolean
        Dim number As Integer
        input = input.Trim
        If input.StartsWith("(") AndAlso input.EndsWith(")") Then
            input = input.Substring(1, input.Length - 2)
        End If
        Return Integer.TryParse(input, number)
    End Function

    Public Shared Function Translate(baseText As String) As String
        Dim i As Integer = GetLanguageWatermark()
        If i > CurrentWatermark Then
            SawDiag.LogVerbose(String.Format("Translate - Rebuild Language Cache: {0}", i))
            SawUtil.Log(String.Format("Translate - Rebuild Language Cache: {0}", i))
            BuildCache()
            CurrentWatermark = i
        End If

        If String.IsNullOrEmpty(baseText) Then Return ""
        If baseText.Trim = "" Then Return ""
        If baseText.ContainsOneOf("&nbsp;", "?", "#") Then Return baseText

        'If baseText.ContainsOneOfCI("Icon", "Help") Then
        '    Debugger.Break()
        'End If
        'If IsIntegerOrBracketed(baseText) Then
        '    Debugger.Break()
        'End If

        Dim LanguageRef As String = "GBR"

        If ctx.siteConfigDef("use_language", "false").ToString = "true" Then
            LanguageRef = ctx.languageCode
        End If

        If baseText.StartsWith("::") Then
            Dim messageRef As String = baseText.Substring(2)

            If _cache.ContainsKey(messageRef) AndAlso _cache(messageRef).ContainsKey(LanguageRef) Then
                Return _cache(messageRef)(LanguageRef).TranslatedText
            Else
                If _cache.ContainsKey("LT_S0342") AndAlso _cache("LT_S0342").ContainsKey(LanguageRef) Then
                    Return String.Format(_cache("LT_S0342")(LanguageRef).TranslatedText, messageRef)
                Else
                    baseText = String.Format("Message {0} is missing", messageRef)
                End If

                Return baseText
            End If
        Else

#If DEBUG Then
            Dim st As New StackTrace
            Debugger.Break()
#End If

            If _cache.ContainsKey(baseText) AndAlso _cache(baseText).ContainsKey(LanguageRef) Then
                    'IawDB.execNonQuery("update dbo.qlang_text
                    '                       set use_count = use_count + 1
                    '                     where language_ref = 'GBR'
                    '                       and text_data = @p1", baseText)
                    Return _cache(baseText)(LanguageRef).TranslatedText
                Else
                    ' Insert the missing base text into the database
                    SawDiag.LogVerbose(String.Format("Translate - add new base text [{0}] [{1}]", LanguageRef, baseText))

                    IawDB.execNonQuery("insert into dbo.qlang_text (language_ref,text_data,text_status)
                                        select 'GBR',@p1,'01'
                                         where not exists (select 1 from qlang_text
                                                            where language_ref = 'GBR'
                                                              and text_data = @p1)", baseText)

                    ' Update the cache with the new base text
                    Dim newEntry As New TranslationEntry With {
                        .LanguageRef = LanguageRef,
                        .TranslatedText = baseText
                    }

                    If Not _cache.ContainsKey(baseText) Then
                        _cache.TryAdd(baseText, New ConcurrentDictionary(Of String, TranslationEntry)())
                    End If

                    _cache(baseText).TryAdd(LanguageRef, newEntry)

                    Return baseText
                End If
            End If
    End Function

    Public Shared Sub RebuildCache()
        BuildCache()
    End Sub
End Class
