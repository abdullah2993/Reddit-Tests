Public Class captchafrm
    Sub New(ByVal captcha As Image)
        InitializeComponent()
        PictureBox1.Image = captcha
    End Sub

    Private _res As String
    Public ReadOnly Property Responce As String
        Get
            Return _res
        End Get
    End Property

    Private Sub restxt_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles restxt.KeyUp
        If e.KeyCode = Keys.Enter Then
            _res = restxt.Text
            Me.Close()
        End If
    End Sub

End Class