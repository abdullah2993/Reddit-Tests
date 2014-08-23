Imports System.Collections.Specialized
Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Net.ServicePointManager
Imports System.Reflection
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.HttpUtility

Namespace Utility

    ''' <summary>Wrapper for HttpWebRequest/HttpWebResponse to make life easier :D</summary>
    ''' <author>idb</author>
    ''' <author_url>http://s.olution.cc/</author_url>
    ''' <remarks>
    ''' Feel free to use this class but please give credit where it's due. I've spent a lot of time on this
    ''' and I'm sharing it with the world for free. What I'm really trying to say is Don't steal my shit and
    ''' claim it's yours.
    ''' </remarks>
    ''' <last_update>Thursday, May 24, 2012</last_update>
    ''' <update_history>
    ''' Friday, January 13, 2012
    '''          Added another GetUploadResponse method that allows you to pass PostData as Byte().
    ''' Saturday, January 14, 2012
    '''         Handled "The underlying connection was closed:" exceptions in ProcessException function.
    '''         Included Request (HttpWebRequest) object into HttpResponse class.
    ''' Wednesday, February 22, 2012
    '''          Accept, Accept-Language, Accept-Encoding will no longer be sent if the property is empty.
    ''' Tuesday, February 28, 2012
    '''          Removed some unnecessary DirectCast's in the GetResponse and GetUploadResponse methods.
    ''' Wednesday, March 21, 2012
    '''          Handled GMT dates on cookie expiration parsing
    '''          Added RequestUri/ResponseUri and RequestHeaders/ResponseHeaders to HttpResponse class
    ''' Sunday, March 25, 2012
    '''          Added IsValidUri function
    ''' Tuesday, March 27, 2012
    '''          Fixed an error in the ParseCookies function
    '''          Got rid of unnecessary FindCookie functions
    ''' Wednesday, April 4, 2012
    '''          Updated ParseCookies, GetCookies, and FindCookie functions
    ''' Tuesday, May 22, 2012
    '''          Updated/Consolidated GetResponse and GetUploadResponse methods. 
    '''          Removed CancelRequest method.
    '''          Removed ForceHttps property. 
    ''' Thursday, May 24, 2012
    '''          Updated GetRedirectUrl function.
    ''' </update_history>

    Public Class Http
        Implements IDisposable

        Public RedirectBlacklist As New List(Of String)
        Private Cookies As List(Of HttpCookie)

#Region "Structures"
        Public Structure HttpProxy
            Dim Server As String
            Dim Port As Integer
            Dim UserName As String
            Dim Password As String

            Public Sub New(ByVal pServer As String, ByVal pPort As Integer, Optional ByVal pUserName As String = "", Optional ByVal pPassword As String = "")
                Server = pServer
                Port = pPort
                UserName = pUserName
                Password = pPassword
            End Sub
        End Structure
        Structure UploadData
            Dim Contents As Byte()
            Dim FileName As String
            Dim FieldName As String
            Public Sub New(ByVal uContents As Byte(), ByVal uFileName As String, ByVal uFieldName As String)
                Contents = uContents
                FileName = uFileName
                FieldName = uFieldName
            End Sub
        End Structure
#End Region

#Region "Properties"
        Private _TimeOut As Integer = 7000
        Public Property TimeOut() As Integer
            Get
                Return _TimeOut
            End Get
            Set(ByVal value As Integer)
                _TimeOut = value
            End Set
        End Property

        Private _Proxy As HttpProxy = New HttpProxy
        Public Property Proxy() As HttpProxy
            Get
                Return _Proxy
            End Get
            Set(ByVal value As HttpProxy)
                _Proxy = value
            End Set
        End Property

        Private _UserAgent As String = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0"
        Public Property Useragent() As String
            Get
                Return _UserAgent
            End Get
            Set(ByVal value As String)
                _UserAgent = value
            End Set
        End Property

        Private _Referer As String = String.Empty
        Public Property Referer() As String
            Get
                Return _Referer
            End Get
            Set(ByVal value As String)
                _Referer = value
            End Set
        End Property

        Private _AutoRedirect As Boolean = True
        Public Property AutoRedirect As Boolean
            Get
                Return _AutoRedirect
            End Get
            Set(ByVal value As Boolean)
                _AutoRedirect = value
            End Set
        End Property

        Private _StoreCookies As Boolean = True
        Public Property StoreCookies() As Boolean
            Get
                Return _StoreCookies
            End Get
            Set(ByVal value As Boolean)
                _StoreCookies = value
            End Set
        End Property

        Private _SendCookies As Boolean = True
        Public Property SendCookies() As Boolean
            Get
                Return _SendCookies
            End Get
            Set(ByVal value As Boolean)
                _SendCookies = value
            End Set
        End Property

        Private _LastRequestUri As String = String.Empty
        Public Property LastRequestUri As String
            Get
                Return _LastRequestUri
            End Get
            Set(ByVal value As String)
                _LastRequestUri = value
            End Set
        End Property

        Private _LastResponseUri As String = String.Empty
        Public Property LastResponseUri As String
            Get
                Return _LastResponseUri
            End Get
            Set(ByVal value As String)
                _LastResponseUri = value
            End Set
        End Property

        Private _KeepAlive As Boolean = True
        Public Property KeepAlive As Boolean
            Get
                Return _KeepAlive
            End Get
            Set(ByVal value As Boolean)
                _KeepAlive = value
            End Set
        End Property

        Private _Version As System.Version = HttpVersion.Version11
        Public Property Version As System.Version
            Get
                Return _Version
            End Get
            Set(ByVal value As System.Version)
                _Version = value
            End Set
        End Property

        Private _Accept As String = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
        Public Property Accept As String
            Get
                Return _Accept
            End Get
            Set(ByVal value As String)
                _Accept = value
            End Set
        End Property

        Private _AllowExpect100 As Boolean = False
        Public Property AllowExpect100 As Boolean
            Get
                Return _AllowExpect100
            End Get
            Set(ByVal value As Boolean)
                _AllowExpect100 = value
            End Set
        End Property

        Private _ContentType As String = "application/x-www-form-urlencoded; charset=UTF-8"
        Public Property ContentType As String
            Get
                Return _ContentType
            End Get
            Set(ByVal value As String)
                _ContentType = value
            End Set
        End Property

        Private _DebugMode As Boolean = False
        Public Property DebugMode As Boolean
            Get
                Return _DebugMode
            End Get
            Set(ByVal value As Boolean)
                _DebugMode = value
            End Set
        End Property

        Private _AcceptEncoding As String = "gzip, deflate"
        Public Property AcceptEncoding As String
            Get
                Return _AcceptEncoding
            End Get
            Set(value As String)
                _AcceptEncoding = value
            End Set
        End Property

        Private _AcceptLanguage As String = "en-us,en;q=0.5"
        Public Property AcceptLanguage As String
            Get
                Return _AcceptLanguage
            End Get
            Set(value As String)
                _AcceptLanguage = value
            End Set
        End Property

        Private _AcceptCharset As String = "ISO-8859-1,utf-8;q=0.7,*;q=0.7"
        Public Property AcceptCharset As String
            Get
                Return _AcceptCharset
            End Get
            Set(value As String)
                _AcceptCharset = value
            End Set
        End Property
#End Region

#Region "Constructor"
        Public Sub New()
            SetUnsafeHeaderParsing(True)

            DefaultConnectionLimit = 500
            Expect100Continue = Me.AllowExpect100
            ServerCertificateValidationCallback = AddressOf AcceptAllCertifications
            UseNagleAlgorithm = False
            SecurityProtocol = SecurityProtocolType.Ssl3

            Cookies = New List(Of HttpCookie)
        End Sub
#End Region

#Region "Deconstructor"
        Private disposedValue As Boolean
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    RedirectBlacklist.Clear()
                    Cookies.Clear()
                End If
            End If
            Me.disposedValue = True
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#Region "Public Methods"
        Public Overloads Function GetResponse(ByVal Uri As String, PostData As String, Optional Headers As NameValueCollection = Nothing) As HttpResponse
            Dim Data() As Byte = Nothing
            If Not String.IsNullOrEmpty(PostData) Then Data = Encoding.UTF8.GetBytes(PostData)
            Dim Result As HttpResponse = GetResponse(Uri, Data, Headers)
            Return Result
        End Function
        Public Overloads Function GetResponse(ByVal Uri As String, Optional PostData() As Byte = Nothing, Optional Headers As NameValueCollection = Nothing) As HttpResponse
            Dim hr As New HttpResponse
            Dim e As Exception = Nothing

            Dim Response As HttpWebResponse = Nothing

request:
            Try
                If Not IsValidUri(Uri) Then
                    e = New Exception(String.Format("'{0}' is not a valid Uri.", Uri))
                    Exit Try
                End If

                Dim Request As HttpWebRequest = SendRequest(Uri, PostData, Headers)
                Response = DirectCast(Request.GetResponse(), HttpWebResponse)
                If StoreCookies Then ParseCookies(Response)

                With hr
                    .WebRequest = Request
                    .RequestUri = Request.RequestUri.ToString
                    .RequestHeaders = GetRequestHeaders(Request)
                    If Not IsNothing(PostData) Then .RequestHeaders &= vbCrLf & vbCrLf & "POST: " & Encoding.UTF8.GetString(PostData)

                    .WebResponse = Response
                    .ResponseUri = Response.ResponseUri.ToString
                    .ResponseHeaders = GetResponseHeaders(Response)
                End With

                With Response
                    If Not .StatusCode = HttpStatusCode.OK Then
                        Select Case .StatusCode
                            Case HttpStatusCode.Found, HttpStatusCode.Redirect, HttpStatusCode.Moved, HttpStatusCode.MovedPermanently, HttpStatusCode.RedirectMethod, HttpStatusCode.RedirectKeepVerb
                                If AutoRedirect Then
                                    Dim Redirect As String = GetRedirectUrl(hr.RequestUri, .Headers("Location"))
                                    If Not String.IsNullOrEmpty(Redirect) Then
                                        If Not IsBlackListed(Redirect) Then
                                            Uri = Redirect
                                            PostData = Nothing
                                            GoTo request
                                        Else
                                            'Debug.Print("Redirect '{0}' is blacklisted", Redirect)
                                        End If
                                    Else
                                        Debug.Print("Could not combine redirect Url with base Url")
                                    End If
                                End If
                        End Select
                    Else
                        LastResponseUri = Response.ResponseUri.ToString
                        If Not IsNothing(.Headers(HttpResponseHeader.ContentType)) Then
                            If .Headers(HttpResponseHeader.ContentType).StartsWith("image") Then
                                hr.Image = System.Drawing.Image.FromStream(.GetResponseStream)
                                Exit Try
                            End If
                        End If
                    End If
                End With

                hr.Html = HtmlDecode(ProcessResponse(Response))
                _LastResponseUri = Uri

                If AutoRedirect Then
                    If hr.Html.ToLower.Contains("<meta http-equiv=""refresh") Then
                        Dim Redirect As String = GetRedirectUrl(hr.RequestUri, ParseMetaRefreshUrl(hr.Html))
                        If Not String.IsNullOrEmpty(Redirect) Then
                            If Not IsBlackListed(Redirect) Then
                                Uri = Redirect
                                PostData = Nothing
                                GoTo request
                            Else
                                'Debug.Print("Redirect '{0}' is blacklisted", Redirect)
                            End If
                        Else
                            Debug.Print("Could not combine redirect Url with base Url")
                        End If
                    End If
                End If

            Catch we As WebException
                e = we
                If Not we.Response Is Nothing Then
                    hr.WebResponse = we.Response
                    hr.Html = ProcessResponse(hr.WebResponse)
                End If
            Catch ex As Exception
                e = ex
            Finally
                If Not IsNothing(e) Then hr.RequestError = ProcessException(e)
                If Not IsNothing(Response) Then Response.Close()
                Me.Referer = String.Empty
            End Try

            Return hr
        End Function
        Public Overloads Function GetResponse(ByVal Uri As String, ByVal Headers As NameValueCollection, ByVal Fields As NameValueCollection, ByVal ParamArray Upload As UploadData()) As HttpResponse
            Dim Boundary As String = Guid.NewGuid().ToString().Replace("-", "")

            Dim Stream As New MemoryStream()
            Dim Writer As New StreamWriter(Stream)
            With Writer
                If Not IsNothing(Fields) Then
                    For Index As Integer = 0 To (Fields.Count - 1)
                        .Write(("--" & Boundary) & vbCrLf)
                        .Write("Content-Disposition: form-data; name=""{0}""{1}{1}{2}{1}", Fields.Keys(Index), vbCrLf, Fields(Index))
                    Next
                End If
                If Not IsNothing(Upload) Then
                    For Each u As UploadData In Upload
                        .Write(("--" & Boundary) & vbCrLf)
                        .Write("Content-Disposition: form-data; name=""{0}""; filename=""{1}""{2}", u.FieldName, u.FileName, vbCrLf)
                        .Write(("Content-Type: " & GetContentType(u.FileName) & vbCrLf) & vbCrLf)
                        .Flush()
                        If Not (u.Contents Is Nothing) Then Stream.Write(u.Contents, 0, u.Contents.Length)
                        .Write(vbCrLf)
                    Next
                End If
                .Write("--{0}--{1}", Boundary, vbCrLf)
                .Flush()
            End With

            Dim Result As HttpResponse = GetResponse(Uri, BytesFromStream(Stream), Headers)
            Return Result
        End Function

        Public Function ResolveIP(ByRef IP As String) As String
            Try
                Return System.Net.Dns.GetHostEntry(IP).HostName
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Function ResolveHost(Host As String) As IPAddress()
            Try
                Return System.Net.Dns.GetHostEntry(Host).AddressList
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Sub ClearCookies()
            Me.Cookies.Clear()
        End Sub
        Public Sub AddCookie(ByVal c As HttpCookie)
            Cookies.Add(c)
        End Sub
        Public Sub AddCookie(ByVal c() As HttpCookie)
            Cookies.AddRange(c)
        End Sub
        Public Sub RemoveCookie(ByVal c As HttpCookie)
            Me.Cookies.Remove(c)
        End Sub
        Public Sub RemoveCookie(ByVal Name As String)
            Dim c As HttpCookie = FindCookie(Name)
            If Not c Is Nothing Then RemoveCookie(c)
        End Sub
        Public Function FindCookie(ByVal Name As String) As HttpCookie
            Dim Result As HttpCookie = Nothing
            For Each c As HttpCookie In Cookies
                If c.Name = Name Then
                    Result = c
                    Exit For
                End If
            Next
            Return Result
        End Function
        Public Function FindCookie(ByVal Name As String, ByVal Domain As String) As HttpCookie
            Dim Result As HttpCookie = Nothing
            For Each c As HttpCookie In Cookies
                If c.Name = Name AndAlso c.Domain = Domain Then
                    Result = c
                    Exit For
                End If
            Next
            Return Result
        End Function
        Public Function GetAllCookies() As List(Of HttpCookie)
            Return Me.Cookies
        End Function

        Public Function UrlEncode(ByVal Data As String) As String
            Return HttpUtility.UrlEncode(Data)
        End Function
        Public Function UrlEncode2(ByVal Value As String) As String
            ' Proper casing of UrlEncode (MS is encodes lowercase)

            Dim Unused As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"
            Dim sb As New StringBuilder()
            For Each c As Char In Value
                sb.Append(IIf(Unused.IndexOf(c) <> -1, c, "%" + String.Format("{0:X2}", Convert.ToInt32(c))))
            Next
            Return sb.ToString()
        End Function
        Public Function UrlDecode(ByVal Data As String) As String
            Return HttpUtility.UrlDecode(Data)
        End Function
        Public Function HtmlEncode(ByVal Data As String) As String
            Return HttpUtility.HtmlEncode(Data)
        End Function
        Public Function HtmlDecode(ByVal Data As String) As String
            Return HttpUtility.HtmlDecode(Data)
        End Function

        Public Function EscapeUnicode(ByVal Data As String) As String
            Return Regex.Unescape(Data)
        End Function
        Public Function FixData(ByVal Data As String) As String
            ' Ghetto hack
            Return HtmlDecode(Data.Replace("\/\/", "//").Replace("\/", "/").Replace("\""", """").Replace("\u003e", ">").Replace("\u003c", "<").Replace("\u003a", ":").Replace("\u003b", ";").Replace("\u003f", "?").Replace("\u003d", "=").Replace("\u002f", "/").Replace("\u0026", "&").Replace("\u002b", "+").Replace("\u0025", "%").Replace("\u0027", "'").Replace("\u007b", "{").Replace("\u007d", "}").Replace("\u007c", "|").Replace("\u0022", """").Replace("\u0023", "#").Replace("\u0021", "!").Replace("\u0024", "$").Replace("\u0040", "@").Replace("\002f", "/").Replace("\r\n", vbCrLf & vbCrLf).Replace("\n", vbCrLf)).Replace("\x3a", ":").Replace("\x2f", "/").Replace("\x3f", "?").Replace("\x3d", "=").Replace("\x26", "&")
        End Function
        Public Function TrimHtml(ByVal Data As String) As String
            Return IIf(String.IsNullOrEmpty(Data), String.Empty, Regex.Replace(Data, "<.*?>", ""))
        End Function

        Public Function ParseMetaRefreshUrl(ByVal Html As String) As String
            If String.IsNullOrEmpty(Html) Then Return String.Empty
            Dim Result As String = Html.Substring(Html.ToLower.IndexOf("<meta http-equiv=""refresh""") + "<meta http-equiv=""refresh""".Length)
            Result = ParseBetween(Result.ToLower, "url=", """", "url=".Length).Trim
            If Result.StartsWith("'") Then Result = Result.Substring(1)
            If Result.EndsWith("'") Then Result = Result.Substring(0, Result.Length - 1)
            Return Result
        End Function
        Public Function ParseBetween(ByVal Html As String, ByVal Before As String, ByVal After As String, ByVal Offset As Integer) As String
            If String.IsNullOrEmpty(Html) Then Return String.Empty
            If Html.Contains(Before) Then
                Dim Result As String = Html.Substring(Html.IndexOf(Before) + Offset)
                If Result.Contains(After) AndAlso Not String.IsNullOrEmpty(After) Then Result = Result.Substring(0, Result.IndexOf(After))
                Return Result
            Else
                Return String.Empty
            End If
        End Function
        Public Function ParseFormIdText(ByVal Html As String, ByVal Id As String, Optional ByVal Highlighter As String = """") As String
            If String.IsNullOrEmpty(Html) Then Return String.Empty
            Dim value As String = String.Empty
            Try
                Html = Html.Substring(Html.IndexOf("id=" & Highlighter & Id & Highlighter) + 5)
                value = ParseBetween(Html, "value=" & Highlighter, Highlighter, 7)
            Catch
            End Try
            Return value
        End Function
        Public Function ParseFormNameText(ByVal Html As String, ByVal Name As String, Optional ByVal Highlighter As String = """") As String
            If String.IsNullOrEmpty(Html) Then Return String.Empty
            Dim value As String = String.Empty
            Try
                Html = Html.Substring(Html.IndexOf("name=" & Highlighter & Name & Highlighter) + 5)
                value = ParseBetween(Html, "value=" & Highlighter, Highlighter, 7)
            Catch
            End Try
            Return value
        End Function
        Public Function ParseFormClassText(ByVal Html As String, ByVal ClassName As String, Optional ByVal Highlighter As String = """") As String
            If String.IsNullOrEmpty(Html) Then Return String.Empty
            Dim value As String = String.Empty
            Try
                Html = Html.Substring(Html.IndexOf("class=" & Highlighter & ClassName & Highlighter) + 7)
                value = ParseBetween(Html, "value=" & Highlighter, Highlighter, 7)
            Catch
            End Try
            Return value
        End Function

        Public Function TimeStamp() As String
            Return CInt(Now.Subtract(CDate("1.1.1970 00:00:00")).TotalSeconds).ToString
        End Function

        Public Function GetContentType(ByVal Path As String) As String
            Dim Result As String = "application/octet-stream"
            Select Case New FileInfo(Path).Extension.ToLower

                Case ".atom", ".xml"
                    Result = "application/atom+xml"
                Case ".json"
                    Result = "application/json"
                Case ".js"
                    Result = "application/javascript"
                Case ".ogg"
                    Result = "application/ogg"
                Case ".pdf"
                    Result = "application/pdf"
                Case ".ps"
                    Result = "application/postscript"
                Case ".woff"
                    Result = "application/x-woff"
                Case ".xhtml", ".xht", ".xml", ".html", ".htm"
                    Result = "application/xhtml+xml"
                Case ".dtd"
                    Result = "application/xml-dtd"
                Case ".zip"
                    Result = "application/zip"
                Case ".gz"
                    Result = "application/x-gzip"

                Case ".au", ".snd"
                    Result = "audio/basic"
                Case ".rmi", ".mid"
                    Result = "audio/mid"
                Case ".mp3"
                    Result = "audio/mpeg"
                Case ".aiff", ".aifc", ".aif"
                    Result = "audio/x-aiff"
                Case ".m3u"
                    Result = "audio/x-mpegurl"
                Case ".ra"
                    Result = "audio/x-pn-realaudio"
                Case ".ram"
                    Result = "audio/x-pn-realaudio"
                Case ".wav"
                    Result = "audio/x-wav"

                Case ".bmp"
                    Result = "image/bmp"
                Case ".cod"
                    Result = "image/cis-cod"
                Case ".gif"
                    Result = "image/gif"
                Case ".ief"
                    Result = "image/ief"
                Case ".jpe", ".jpeg", ".jpg"
                    Result = "image/jpeg"
                Case ".jfif"
                    Result = "image/pipeg"
                Case ".jpeg"
                    Result = "image/pjpeg"
                Case ".png"
                    Result = "image/png"
                Case ".svg"
                    Result = "image/svg+xml"
                Case ".tif", ".tiff"
                    Result = "image/tiff"
                Case ".ras"
                    Result = "image/x-cmu-raster"
                Case ".cmx"
                    Result = "image/x-cmx"
                Case ".ico"
                    Result = "image/x-icon"
                Case ".png"
                    Result = "image/x-png"
                Case ".pnm"
                    Result = "image/x-portable-anymap"
                Case ".pbm"
                    Result = "image/x-portable-bitmap"
                Case ".pgm"
                    Result = "image/x-portable-graymap"
                Case ".ppm"
                    Result = "image/x-portable-pixmap"
                Case ".rgb"
                    Result = "image/x-rgb"
                Case ".xbm"
                    Result = "image/x-xbitmap"
                Case ".xpm"
                    Result = "image/x-xpixmap"
                Case ".xwd"
                    Result = "image/x-xwindowdump"

                Case ".mp2"
                    Result = "video/mpeg"
                Case ".mpa"
                    Result = "video/mpeg"
                Case ".mpe"
                    Result = "video/mpeg"
                Case ".mpeg"
                    Result = "video/mpeg"
                Case ".mpg"
                    Result = "video/mpeg"
                Case ".mpv2"
                    Result = "video/mpeg"
                Case ".mov", ".qt"
                    Result = "video/quicktime"
                Case ".lsf", ".lsx"
                    Result = "video/x-la-asf"
                Case ".asf", ".asr", ".asx"
                    Result = "video/x-ms-asf"
                Case ".avi"
                    Result = "video/x-msvideo"
                Case ".movie"
                    Result = "video/x-sgi-movie"

                Case Else
                    Result = "application/octet-stream"
            End Select
            Return Result
        End Function

        Public Function CountOccurance(ByVal Data As String, ByVal Search As String, Optional ByVal CaseSensitive As Boolean = False) As Integer
            Return (IIf(CaseSensitive, (Data.Length - (Data.Replace(Search, "").Length)), (Data.Length - (Data.ToLower.Replace(Search.ToLower, "").Length))) / Search.Length)
        End Function

        Public Function IsValidUri(ByVal Url As String) As Boolean
            Try
                Dim Uri As New Uri(Url)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
#End Region

#Region "Private Methods"
        Private Function SendRequest(Uri As String, PostData() As Byte, Headers As NameValueCollection) As HttpWebRequest
            Dim Request As HttpWebRequest = WebRequest.Create(Uri)
            With Request
                Dim ServicePoint As ServicePoint = .ServicePoint
                Dim Prop As Reflection.PropertyInfo = ServicePoint.[GetType]().GetProperty("HttpBehaviour", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                Prop.SetValue(ServicePoint, CByte(0), Nothing)

                .Method = IIf(IsNothing(PostData), "GET", "POST")
                If Me.DebugMode Then Debug.Print(String.Format("[{0}] {1} >> {2}", Now.ToString("hh:mm:ss tt").ToLower, .Method, Uri))

                .AllowWriteStreamBuffering = False
                .AllowAutoRedirect = False
                .KeepAlive = KeepAlive
                .UserAgent = Useragent
                .ContentType = IIf(.Method = "POST", ContentType, String.Empty)
                .AutomaticDecompression = DecompressionMethods.GZip And DecompressionMethods.Deflate
                .Accept = Accept
                .Timeout = TimeOut
                .ProtocolVersion = Version
                .ServicePoint.Expect100Continue = AllowExpect100
                If Not String.IsNullOrEmpty(Referer) Then .Referer = Referer

                If Not String.IsNullOrEmpty(Proxy.Server) Then
                    .Proxy = New WebProxy(Proxy.Server, Proxy.Port)
                    If Not String.IsNullOrEmpty(Proxy.UserName) Then .Proxy.Credentials = New NetworkCredential(Proxy.UserName, Proxy.Password)
                End If

                If SendCookies Then
                    Dim Cookie As String = GetCookies(IIf(Uri.StartsWith("https://"), "https://", "http://") & New Uri(Uri).Host)
                    If Not String.IsNullOrEmpty(Cookie) Then .Headers.Add("Cookie", Cookie)
                End If

                If Not String.IsNullOrEmpty(AcceptEncoding) Then .Headers.Add(HttpRequestHeader.AcceptEncoding, AcceptEncoding)
                If Not String.IsNullOrEmpty(AcceptLanguage) Then .Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLanguage)
                If Not String.IsNullOrEmpty(AcceptCharset) Then .Headers.Add(HttpRequestHeader.AcceptCharset, AcceptCharset)

                If Not IsNothing(Headers) Then
                    For Index As Integer = 0 To (Headers.Count - 1)
                        If Headers.Keys(Index) = "Content-Type" Then
                            .ContentType = Headers(Index)
                        Else
                            .Headers.Add(Headers.Keys(Index), Headers(Index))
                        End If
                    Next
                End If

                If Not IsNothing(PostData) Then
                    .ContentLength = PostData.Length
                    Dim dataStream As Stream = .GetRequestStream()
                    dataStream.Write(PostData, 0, PostData.Length)
                    dataStream.Close() : dataStream.Dispose()
                End If
            End With

            LastRequestUri = Uri
            Return Request
        End Function

        Private Function BytesFromStream(ByVal Stream As System.IO.Stream) As Byte()
            Dim Length As Integer = Convert.ToInt32(Stream.Length)
            Dim Data As Byte() = New Byte(Length) {}
            Stream.Read(Data, 0, Length)
            Stream.Close()
            Return Data
        End Function

        Private Function ProcessResponse(ByVal Response As System.Net.HttpWebResponse) As String
            Try
                Dim sb As New StringBuilder
                With Response
                    Dim Stream As System.IO.Stream = .GetResponseStream

                    If (Response.ContentEncoding.ToLower().Contains("gzip")) Then
                        Stream = New GZipStream(Stream, CompressionMode.Decompress)
                    ElseIf (Response.ContentEncoding.ToLower().Contains("deflate")) Then
                        Stream = New DeflateStream(Stream, CompressionMode.Decompress)
                    End If

                    Dim Reader As New StreamReader(Stream)
                    Dim Buffer(1024) As [Char]
                    Dim Read As Integer = Reader.Read(Buffer, 0, 1024)
                    While Read > 0
                        Dim outputData As New [String](Buffer, 0, Read)
                        outputData = Replace(outputData, vbNullChar, String.Empty)
                        sb.Append(outputData)
                        Read = Reader.Read(Buffer, 0, 1024)
                    End While
                    Reader.Close() : Reader.Dispose() : Reader = Nothing
                    Stream.Close() : Stream.Dispose() : Stream = Nothing
                End With
                Return sb.ToString
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Private Function ProcessException(ByVal Ex As Object) As HttpError
            Dim Result As New HttpError
            Dim Message As String = String.Empty

            Result.Exception = Ex

            If TypeOf Ex Is WebException Then
                Dim we As WebException = DirectCast(Ex, WebException)
                If we.Message.Contains("The underlying connection was closed:") Then
                    Message = we.Message
                    If Not String.IsNullOrEmpty(Me.Proxy.Server) Then Result.IsProxyError = True
                ElseIf we.Message.Contains("The remote server returned an error: (") Then
                    Message = we.Message
                    If Not String.IsNullOrEmpty(Me.Proxy.Server) Then Result.IsProxyError = True
                Else
                    Select Case we.Status
                        Case WebExceptionStatus.Timeout
                            Message = "Timed out."
                            If Not String.IsNullOrEmpty(Me.Proxy.Server) Then Result.IsProxyError = True
                        Case WebExceptionStatus.ConnectFailure
                            If we.Message.Trim = "Unable to connect to the remote server" Then
                                Message = IIf(Not String.IsNullOrEmpty(Me.Proxy.Server), "Could not connect to proxy server.", "Could not connect to server.")
                                If Not String.IsNullOrEmpty(Me.Proxy.Server) Then Result.IsProxyError = True
                            Else
                                Message = we.Message
                            End If
                        Case WebExceptionStatus.KeepAliveFailure
                            Message = IIf(Not String.IsNullOrEmpty(Me.Proxy.Server), "Disconnected from proxy server.", we.Message)
                            If Not String.IsNullOrEmpty(Me.Proxy.Server) Then Result.IsProxyError = True
                        Case Else
                            Message = we.Message
                            Debug.Print("Exception else: " & we.Status & " - " & we.Message)
                    End Select
                End If
            Else
                Message = TryCast(Ex, Exception).Message
            End If
            Result.Message = Message
            Return Result
        End Function

        Private Function GetCookies(ByVal RequestUri As String) As String
            Dim Result As String = String.Empty
            Dim Uri As New Uri(RequestUri)

            If Not Cookies Is Nothing Then
                With Cookies
                    If .Count = 0 Then Return Result
                    For Each item As HttpCookie In Cookies
                        If item.Domain.ToLower.Trim = Uri.Host.ToLower.Trim Then
                            Result &= item.Name & IIf(String.IsNullOrEmpty(item.Value), "", "=" & item.Value) & "; "
                        Else
                            If item.Domain.StartsWith(".") AndAlso Uri.Host.Contains(item.Domain) Then
                                Result &= item.Name & IIf(String.IsNullOrEmpty(item.Value), "", "=" & item.Value) & "; "
                            ElseIf item.Domain.StartsWith(".") AndAlso CountOccurance(Uri.Host, ".") = 1 Then
                                Result &= item.Name & IIf(String.IsNullOrEmpty(item.Value), "", "=" & item.Value) & "; "
                            ElseIf item.Domain.Contains(Uri.Host) Then
                                Result &= item.Name & IIf(String.IsNullOrEmpty(item.Value), "", "=" & item.Value) & "; "
                            Else
                                'Debug.Print("here: " & Uri.Host & " / " & item.Domain)
                            End If
                        End If
nextItem:
                    Next
                    If Result.EndsWith("; ") Then Result = Result.Substring(0, Result.Length - 2)
                End With
            Else
                Cookies = New List(Of HttpCookie)
            End If
            Return Result
        End Function
        Private Sub ParseCookies(ByVal Response As HttpWebResponse)
            Try
                If Cookies Is Nothing Then Cookies = New List(Of HttpCookie)

                Dim Data As String = Response.Headers("Set-Cookie")
                If Data = Nothing Then Exit Sub
                If String.IsNullOrEmpty(Data) Then Exit Sub
                Data = Data.Replace("Mon,", "Mon").Replace("Tue,", "Tue").Replace("Wed,", "Wed").Replace("Thu,", "Thu").Replace("Fri,", "Fri").Replace("Sat,", "Sat").Replace("Sun,", "Sun")
                Data = Data.Replace("Monday,", "Mon").Replace("Tuesday,", "Tue").Replace("Wednesday,", "Wed").Replace("Thursday,", "Thurs").Replace("Friday,", "Fri").Replace("Saturday,", "Sat").Replace("Sunday,", "Sun")

                For Each c As String In Data.Split(",")
                    Dim Cookie As New HttpCookie
                    With Cookie
                        .Name = c.Split("=")(0).Trim
                        .HttpOnly = False
                        .Secure = False

                        If c.Contains(";") Then
                            .Value = c.Substring(0, c.IndexOf(";"))
                            .Value = .Value.Split("=")(1).Trim
                        Else
                            .Value = c.Split("=")(1).Trim
                        End If

                        If Not .Value.ToLower = "deleted" Then
                            For Each Parameter As String In Split(c, ";")
                                Parameter = Parameter.Trim
                                If Not String.IsNullOrEmpty(Parameter) Then
                                    If Parameter.Contains("=") Then
                                        Dim Key As String = Parameter.Split("=")(0).Trim
                                        Dim Value As String = Parameter.Substring(Parameter.IndexOf("=") + 1).Trim

                                        Select Case Key.ToLower
                                            Case .Name.ToLower
                                                .Value = Value
                                            Case "path"
                                                .Path = Value
                                            Case "expires"
                                                Try
                                                    If Value.ToLower.EndsWith("utc") Or Value.ToLower.EndsWith("gmt") Then
                                                        Value = Value.Substring(0, Value.ToLower.IndexOf(IIf(Value.ToLower.EndsWith("utc"), "utc", "gmt"))).Trim
                                                        Try
                                                            .Expires = System.DateTime.Parse(Value)
                                                        Catch ex As Exception
                                                            .Expires = Nothing
                                                        End Try
                                                    Else
                                                        .Expires = Value
                                                    End If
                                                Catch ex As Exception
                                                    .Expires = Nothing
                                                End Try
                                            Case "domain"
                                                If CountOccurance(Value, ".") = 1 Then Value = Response.ResponseUri.Host 'Value = Response.ResponseUri.Host
                                                .Domain = Value
                                            Case "httponly"
                                                .HttpOnly = True
                                            Case "secure"
                                                .Secure = True
                                            Case "version"
                                                .Version = Value
                                            Case Else
                                                Debug.Print("unknown with value: " & Key & " - " & Value)
                                        End Select
                                    Else
                                        Select Case Parameter.ToLower
                                            Case "secure"
                                                .Secure = True
                                            Case "httponly"
                                                .HttpOnly = True
                                            Case Else
                                                Debug.Print("unknown without value: " & Parameter)
                                        End Select
                                    End If
                                End If
                            Next

                            If String.IsNullOrEmpty(.Path) Then .Path = Response.ResponseUri.AbsolutePath
                            If String.IsNullOrEmpty(.Domain) Then .Domain = Response.ResponseUri.Host

                            If Not FindCookie(Cookie.Name, Cookie.Domain) Is Nothing Then Cookies.Remove(FindCookie(Cookie.Name, Cookie.Domain))
                            Cookies.Add(Cookie)
                        Else
                            If Not FindCookie(Cookie.Name, Cookie.Domain) Is Nothing Then Cookies.Remove(FindCookie(Cookie.Name, Cookie.Domain))
                        End If
                    End With
                Next
            Catch ex As Exception
                Debug.Print(ex.ToString)
            End Try
        End Sub

        Private Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
            Return True
        End Function

        Private Sub SetUnsafeHeaderParsing(ByVal Allow As Boolean)
            ' Credit: lessthandot.com
            ' Source: http://wiki.lessthandot.com/index.php/Setting_unsafeheaderparsing

            Dim SettingsSection As New System.Net.Configuration.SettingsSection
            Dim Assembly As System.Reflection.Assembly = System.Reflection.Assembly.GetAssembly(SettingsSection.GetType)
            Dim SettingsType As Type = Assembly.GetType("System.Net.Configuration.SettingsSectionInternal")
            Dim Args As Object() = Nothing
            Dim Instance As Object = SettingsType.InvokeMember("Section", System.Reflection.BindingFlags.Static Or BindingFlags.GetProperty Or BindingFlags.NonPublic, Nothing, Nothing, Args)
            Dim UseUnsafeHeaderParsing As FieldInfo = SettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic Or BindingFlags.Instance)
            UseUnsafeHeaderParsing.SetValue(Instance, Allow)
        End Sub

        Private Function GetRequestHeaders(Request As HttpWebRequest) As String
            Return String.Format("Request Headers -----------------------------------{0}{1}", vbCrLf, Request.Headers.ToString)
        End Function
        Private Function GetResponseHeaders(Response As HttpWebResponse) As String
            Return String.Format("Response Headers -----------------------------------{0}{1}", vbCrLf, Response.Headers.ToString)
        End Function

        Private Function GetRedirectUrl(RequestUri As String, Redirect As String) As String
            Dim Result As String = String.Empty
            If IsValidUri(Redirect) Then
                Result = Redirect
            Else
                If Redirect.StartsWith("&") Or Redirect.StartsWith("?") Then
                    Result = RequestUri & Redirect
                ElseIf Redirect.StartsWith("/") Then
                    Dim Built As Boolean = False
                    For Each Part As String In Split(Redirect, "/")
                        If RequestUri.EndsWith("/" & Part) Then
                            Result = RequestUri & Redirect.Substring(Redirect.IndexOf("/" & Part) + Part.Length + 1)
                            Exit For
                        End If
                    Next
                    If Not Built Then Result = IIf(RequestUri.StartsWith("https"), "https://", "http://") & New Uri(RequestUri).Host & Redirect
                Else
                    Debug.Print("RequestUri: " & RequestUri)
                    Debug.Print("Redirect: " & Redirect)
                End If
            End If
            Return Result
        End Function

        Private Function IsBlackListed(Url As String) As Boolean
            For Each r As String In RedirectBlacklist
                If Url.ToLower.Contains(r.ToLower) Then
                    Return True
                    Exit For
                End If
            Next
            Return False
        End Function
#End Region

        <Serializable()> Public Class HttpCookie
            Public Name As String = String.Empty
            Public Value As String = String.Empty
            Public Domain As String = String.Empty
            Public Path As String = String.Empty
            Public Expires As Date = Nothing
            Public HttpOnly As Boolean = False
            Public Secure As Boolean = False
            Public Version As Integer = -1

            Public Sub New()
            End Sub
            Public Sub New(cName As String, cValue As String, cDomain As String)
                Name = cName
                Value = cValue
                Domain = cDomain
            End Sub
            Public Sub New(cName As String, cValue As String, cDomain As String, cPath As String, cExpires As Date, cHttpOnly As Boolean, cSecure As Boolean, cVersion As Integer)
                Name = cName
                Value = cValue
                Domain = cDomain
                Path = cPath
                Expires = cExpires
                HttpOnly = cHttpOnly
                Secure = cSecure
                Version = cVersion
            End Sub
        End Class
        Public Class HttpResponse
            Public WebRequest As HttpWebRequest = Nothing
            Public RequestHeaders As String = String.Empty
            Public RequestUri As String = String.Empty

            Public WebResponse As HttpWebResponse = Nothing
            Public ResponseHeaders As String = String.Empty
            Public ResponseUri As String = String.Empty

            Public RequestError As HttpError = Nothing

            Public Html As String = String.Empty
            Public Image As Image = Nothing
        End Class
        Public Class HttpError
            Public Exception As Object = Nothing
            Public Message As String = String.Empty
            Public IsProxyError As Boolean = False
            Public Html As String = String.Empty
        End Class
    End Class
End Namespace