Imports Reddit_Registration.Services.Reddit
Public Class mainfrm

    WithEvents r As Services.Reddit

    Private Sub registerbtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles registerbtn.Click
        If Not IsNothing(r) Then r.Dispose()
        r = New Services.Reddit
        With r
            .username = usertxt.Text
            .password = passtxt.Text
            .email = emailtxt.Text
            Dim result As Result = .Register
            If result.success Then MessageBox.Show("Registered Successfull") : Return
            If result.RateError Then MessageBox.Show("Limit reached wait before registering again")
            If Not String.IsNullOrEmpty(result.RequestError) Then MessageBox.Show(result.RequestError)
            If Not String.IsNullOrEmpty(result.InputError) Then MessageBox.Show(result.InputError)
        End With
    End Sub

    Private Sub r_CaptchaRequired(ByVal cap As Image) Handles r.CaptchaRequired
        Using cf As New captchafrm(cap)
            cf.ShowDialog()
            r.CaptchaResponce = cf.Responce
        End Using
    End Sub

End Class
