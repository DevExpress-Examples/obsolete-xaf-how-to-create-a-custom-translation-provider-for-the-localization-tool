Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Web
Imports System.Text
Imports System.Collections.Generic
Imports DevExpress.ExpressApp.Utils

Namespace GoogleTranslatorProvider
	Public Class GoogleTranslatorProvider
		Inherits TranslatorProviderBase
		Private Shared GoogleLanguages() As String = {"sq", "ar", "bg", "zh", "ca", "hr", "cs", "da", "nl", "en", "et", "fi", "fr", "gl", "de", "el", "he", "hi", "hu", "id", "it", "ja", "co", "lv", "lt", "mt", "nn", "fa", "pl", "pt", "ro", "ru", "es", "sr", "sk", "sl", "sl", "sv", "th", "tr", "uk", "vi"}

		Public Sub New()
			MyBase.New("<br />", 5000)
		End Sub
		Friend Shared Function CalculatePairSeparatorsBlocks(ByVal text As String) As IEnumerable(Of String)
			Dim result As New List(Of String)()
			Dim leftSeparators() As String = { """", "^'", " '", "('", "{" }
			Dim rightSeparators() As String = { """", "'", "'", "'", "}" }
			Dim index As Integer = 0
			Dim leftSeparatorIndex As Integer = 0
			Dim rightSeparatorIndex As Integer = 0
			Dim iSeparator As Integer = 0
			Dim leftSeparatorSize As Integer = 0
			Do While index < text.Length
				leftSeparatorIndex = -1
				For i As Integer = 0 To leftSeparators.Length - 1
					Dim separatorIndex As Integer = -1
					If leftSeparators(i).Chars(0) = "^"c Then
						separatorIndex = text.IndexOf(leftSeparators(i).Substring(1), index)
						If separatorIndex = 0 Then
							iSeparator = i
							leftSeparatorIndex = separatorIndex
							leftSeparatorSize = leftSeparators(i).Length - 1
						End If
					Else
						separatorIndex = text.IndexOf(leftSeparators(i), index)
						If separatorIndex >= 0 AndAlso (leftSeparatorIndex < 0 OrElse separatorIndex < leftSeparatorIndex) Then
							iSeparator = i
							leftSeparatorIndex = separatorIndex
							leftSeparatorSize = leftSeparators(i).Length
						End If
					End If
				Next i
				If leftSeparatorIndex >= 0 Then
					rightSeparatorIndex = text.IndexOf(rightSeparators(iSeparator), leftSeparatorIndex + leftSeparatorSize)
					If rightSeparatorIndex >= 0 Then
						Dim block As String = text.Substring(index, leftSeparatorIndex - index).Trim()
						If block.Length > 0 Then
							result.Add(block)
						End If
						index = rightSeparatorIndex + rightSeparators(iSeparator).Length
						Continue Do
					End If
				End If
				result.Add(text.Substring(index, text.Length - index).Trim())
				index = text.Length
			Loop
			Return result
		End Function

		Protected Function TranslateCore(ByVal text As String, ByVal sourceLanguageCode As String, ByVal desinationLanguageCode As String) As String
			Dim serviceUri As New Uri("http://ajax.googleapis.com/ajax/services/language/translate")
			Dim encoding As New UTF8Encoding()
			Dim postData As String = "v=1.0"
			postData &= ("&q=" & HttpUtility.UrlEncode(text))
			postData &= ("&langpair=" & sourceLanguageCode & "|" & desinationLanguageCode)
			Dim data() As Byte = encoding.GetBytes(postData)

			Dim httpRequest As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(serviceUri), System.Net.HttpWebRequest)
			httpRequest.Timeout = 15000
			httpRequest.Method = "POST"
			httpRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8"
			httpRequest.ContentLength = data.Length
			Using requestStream As Stream = httpRequest.GetRequestStream()
				requestStream.Write(data, 0, data.Length)
			End Using

			Dim response As System.Net.HttpWebResponse = CType(httpRequest.GetResponse(), System.Net.HttpWebResponse)
			Dim resp As String = Nothing
			Using sReader As New StreamReader(response.GetResponseStream(), encoding)
				resp = sReader.ReadToEnd()
			End Using
			Return resp
		End Function
		Friend Shared Function GetPropertyValueFromJson(ByVal text As String, ByVal propertyName As String) As String
			Dim index As Integer = text.IndexOf(propertyName)
			If index < 0 Then
				Return Nothing
			End If
			Dim indexColon As Integer = text.IndexOf(":", index)
			Dim indexValueStart As Integer = indexColon + 1

			Do While text.Chars(indexValueStart) = " "c
				indexValueStart += 1
			Loop
			If text.Chars(indexValueStart) = """"c Then
				indexValueStart += 1
				Dim indexValueEnd As Integer = text.IndexOf("""", indexValueStart)
				Return text.Substring(indexValueStart, indexValueEnd - indexValueStart)
			ElseIf text.Chars(indexValueStart) >= "0"c AndAlso text.Chars(indexValueStart) <= "9"c Then
				index = indexValueStart
				Dim number As String = String.Empty
				Do While text.Chars(index) >= "0"c AndAlso text.Chars(index) <= "9"c AndAlso index < text.Length
					number &= text.Chars(index)
					index += 1
				Loop
				Return number
			End If
			Return Nothing
		End Function
		Friend Shared Function DecodeASCIIToUnicode(ByVal text As String) As String
			Dim result As String = text
			Dim unicodeSymbolIndex As Integer = 0
			unicodeSymbolIndex = result.IndexOf("\u")
			Do While unicodeSymbolIndex >= 0
				Dim unicodeCharString As String = result.Substring(unicodeSymbolIndex, 6)
				Dim unicodeCharStringCode As String = result.Substring(unicodeSymbolIndex + 2, 4)
				Dim unicodeChar As String = Char.ConvertFromUtf32(Convert.ToInt32(unicodeCharStringCode, 16))
				result = result.Replace(unicodeCharString, unicodeChar)
				unicodeSymbolIndex = result.IndexOf("\u")
			Loop
			Return result
		End Function

		Public Overrides Function CalculateSentences(ByVal text As String) As IEnumerable(Of String)
			Dim sentences As New List(Of String)()
			Dim lineBlocks As IEnumerable(Of String) = text.Split(New String() { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
			For Each lineBlock As String In lineBlocks
				sentences.AddRange(CalculatePairSeparatorsBlocks(lineBlock))
			Next lineBlock
			Return sentences
		End Function

		#Region "ITranslatorProvider Members"

		Public Overrides ReadOnly Property Caption() As String
			Get
				Return "Google Translate"
			End Get
		End Property
		Public Overrides ReadOnly Property Description() As String
			Get
				Return "Powered by <b>Google Translate</b>"
			End Get
		End Property
		Public Overrides Function GetLanguages() As String()
			Return GoogleLanguages
		End Function
		Public Overrides Function Translate(ByVal text As String, ByVal sourceLanguageCode As String, ByVal desinationLanguageCode As String) As String
			Dim jsonText As String = TranslateCore(text, sourceLanguageCode, desinationLanguageCode)
			Dim responseStatus As String = GetPropertyValueFromJson(jsonText, "responseStatus")
			If responseStatus = "200" Then
				Dim translatedText As String = GetPropertyValueFromJson(jsonText, "translatedText")
				Dim unicodeDecodedText As String = DecodeASCIIToUnicode(translatedText)
				Return HttpUtility.HtmlDecode(unicodeDecodedText)
			Else
				Return text
			End If
		End Function
		#End Region
	End Class
End Namespace
