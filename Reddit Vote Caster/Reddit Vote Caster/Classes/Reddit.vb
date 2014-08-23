Imports Reddit_Vote_Caster.Utility.Http

Namespace Services
    Public Class Reddit
        Implements IDisposable

#Region "Decleration"
        Private http As Utility.Http = Nothing
#End Region

#Region "Structures"

        Public Structure Result
            Dim Success As Boolean
            Dim ErrorMsg As String
            Dim html As String
        End Structure

#End Region

#Region "Enum"
        Private VoteType As Integer() = New Integer() {1, -1, 0}
        Public Enum Vote As Integer
            Up = 0
            Down = 1
            Neutral = 2
        End Enum
#End Region

#Region "Properties"

        Private _username As String = String.Empty
        Public Property Username As String
            Get
                Return _username
            End Get
            Set(ByVal value As String)
                _username = value
            End Set
        End Property

        Private _password As String = String.Empty
        Public Property Password As String
            Get
                Return _password
            End Get
            Set(ByVal value As String)
                _password = value
            End Set
        End Property

        Private _modhash As String = String.Empty
        Private ReadOnly Property ModHash As String
            Get
                Return _modhash
            End Get
        End Property

        Private _sessioncookie As HttpCookie = Nothing
        Private ReadOnly Property SessionCookie As HttpCookie
            Get
                Return _sessioncookie
            End Get
        End Property

#End Region

#Region "constructor"
        Public Sub New()
            Me.NewSession()
        End Sub
        Public Sub New(ByVal user As String, ByVal pass As String)
            Username = user : Password = pass
        End Sub
#End Region

#Region "Api"

        Public Function Login() As Result
            Me.NewSession()
            Dim Result As New Result
            With http
                Dim res As HttpResponse = .GetResponse("http://www.reddit.com/api/login/" & Username, String.Format("api_type=json&user={0}&passwd={1}", Username, Password))
                If Not IsNothing(res.RequestError) Then Result.ErrorMsg = res.RequestError.Message
                If res.Html.Contains("invalid password") Then Result.ErrorMsg = "Invalid Password/username"
                If res.Html.Contains("modhash") Then Result.Success = True : SetSessionCookie()
                Result.html = res.Html
                Return Result
            End With
        End Function

        Public Function CastVote(ByVal url As String, ByVal v As Integer) As Boolean
            http.ClearCookies()
            http.AddCookie(SessionCookie)
            SetModhash()
            Dim res As HttpResponse = http.GetResponse("http://www.reddit.com/api/vote", String.Format("id={0}&dir={1}&uh={2}", GetThingID(url), VoteType(v), ModHash))
            If res.RequestError Is Nothing Then If res.Html.Contains("{}") Then Return True
            Return False
        End Function

#End Region

#Region "private methods"
        Private Sub NewSession()
            If Not IsNothing(http) Then http.Dispose()
            http = New Utility.Http
            _modhash = ""
            _sessioncookie = Nothing
        End Sub

        Private Sub SetSessionCookie()
            _sessioncookie = http.FindCookie("reddit_session")
        End Sub

        Private Sub SetModhash()
            http.ClearCookies()
            http.AddCookie(SessionCookie)
            Dim res As HttpResponse = http.GetResponse("http://www.reddit.com/api/me.json")
            If IsNothing(res.RequestError) Then
                _modhash = http.ParseBetween(res.Html, """modhash"": """, """", """modhash"": """.Length)
            End If
        End Sub

        Private Function GetThingID(ByVal url As String) As String
            http.ClearCookies()
            http.AddCookie(SessionCookie)
            Dim res As HttpResponse = http.GetResponse("http://www.reddit.com/api/info.json?url=" & url)
            If IsNothing(res.RequestError) Then
                Return http.ParseBetween(res.Html, """name"": """, """", """name"": """.Length)
            End If
            Return String.Empty
        End Function

#End Region

#Region "Destructor"

        Private disposedValue As Boolean
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If Not IsNothing(http) Then http.Dispose()
                End If
            End If
            Me.disposedValue = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class
End Namespace