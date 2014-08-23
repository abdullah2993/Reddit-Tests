Imports Reddit_Registration.Utility.Http
Imports System.Collections.Specialized
Imports System.Text

Namespace Services

    Public Class Reddit
        Implements IDisposable

#Region "Declerations"

        Private http As Utility.Http = Nothing

#End Region

#Region "Event"

        Event CaptchaRequired(ByVal cap As Image)

#End Region

#Region "Structures"

        Public Structure Result
            Dim success As Boolean
            Dim InputError As String
            Dim RateError As Boolean
            Dim RequestError As String
        End Structure

#End Region

#Region "properties"

        Private _user As String = String.Empty
        Public Property username As String
            Get
                Return _user
            End Get
            Set(ByVal value As String)
                _user = value
            End Set
        End Property

        Private _pass As String = String.Empty
        Public Property password As String
            Get
                Return _pass
            End Get
            Set(ByVal value As String)
                _pass = value
            End Set
        End Property

        Private _email As String = String.Empty
        Public Property email As String
            Get
                Return _email
            End Get
            Set(ByVal value As String)
                _email = value
            End Set
        End Property

        Private _captchares As String = String.Empty
        Public Property CaptchaResponce As String
            Get
                Return _captchares
            End Get
            Set(ByVal value As String)
                _captchares = value
            End Set
        End Property


#End Region

#Region "Constructor"

        Public Sub New()
            Me.NewSession()
			http.TimeOut=60000
        End Sub

#End Region

#Region "private methods"

        Private Sub NewSession()
            If Not IsNothing(http) Then http.Dispose()
            http = New Utility.Http
        End Sub

        Private Function GetCaptchaImage(ByVal cap As String) As Image
            Dim res As HttpResponse = http.GetResponse("https://ssl.reddit.com/captcha/" & cap & ".png")
            Return res.Image
        End Function

#End Region

#Region "API"
        Public Function Register() As Result
            Dim Result As New Result
            If Not IsNothing(http) Then http.Dispose()
            NewSession()
            With http
                .TimeOut = -1
                .Referer = "http://www.reddit.com/"
                Dim res As HttpResponse = .GetResponse("https://ssl.reddit.com/login")
                If Not IsNothing(res.RequestError) Then Result.RequestError = res.RequestError.Message
                Dim Headers As New NameValueCollection
                With Headers
                    .Add("Origin", "http://www.reddit.com/")
                    .Add("Pragma", "no-cache")
                    .Add("Cache-Control", "no-cache")
                End With
                .Accept = "application/json, text/javascript, */*; q=0.01"
                Dim PostData As New StringBuilder
                PostData.Append("op=reg")
                PostData.Append("&user=" & .UrlEncode(username))
                PostData.Append("&email=" & .UrlEncode(email))
                PostData.Append("&passwd=" & .UrlEncode(password))
                PostData.Append("&passwd2=" & .UrlEncode(password))
                PostData.Append("&iden=" & .ParseFormNameText(res.Html, "iden"))
                RaiseEvent CaptchaRequired(GetCaptchaImage(.ParseFormNameText(res.Html, "iden")))
                PostData.Append("&captcha=" & CaptchaResponce)
                PostData.Append("&rem=on&api_type=json")
                res = .GetResponse("https://ssl.reddit.com/api/register/" & .UrlEncode(username), PostData.ToString, Headers)
                If Not IsNothing(res.RequestError) Then Result.RequestError = res.RequestError.Message
                If res.Html.Contains("that username is already taken") Then Result.InputError += "Username already exist:"
                If res.Html.Contains("care to try these again") Then Result.InputError += "Captcha Wrong:"
                If res.Html.Contains("that password is unacceptable") Then Result.InputError += "Password is un acceptable"
                If res.Html.Contains("you are doing that too much. try again in") Then Result.RateError = True
                If res.Html.Contains("modhash") Then Result.success = True
                If String.IsNullOrEmpty(Result.InputError) And Result.success = False And Result.RateError = False Then Result.InputError = "Unknown error"
                Return Result
            End With
        End Function

#End Region

#Region "Destructor"
        Private disposedValue As Boolean
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    http = Nothing
                End If
            End If
            Me.disposedValue = True
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class

End Namespace