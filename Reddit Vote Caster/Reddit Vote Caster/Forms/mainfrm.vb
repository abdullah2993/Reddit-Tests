Imports Reddit_Vote_Caster.Services

Public Class mainfrm

    Private Sub vote_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles vote.Click
        Dim Vote As Reddit.Vote
        Select Case vt.Text
            Case "Vote Up"
                Vote = Reddit.Vote.Up
            Case "Vote Down"
                Vote = Reddit.Vote.Down
            Case "Vote Neutral"
                Vote = Reddit.Vote.Neutral
        End Select

        Log("Started...")
        Using reddit As New Reddit(usr.Text, pass.Text)
            Log("Trying to login...")
            Dim Result As Reddit.Result = reddit.Login
            If Result.Success Then
                Log("Login successfull!")
                Log("Casting Vote for: " & url.Text)
                If reddit.CastVote(url.Text, Vote) Then
                    Log("Vote casted Successfully!")
                Else
                    Log("Vote casting failed!")
                End If
            Else
                Log("Login failed!")
                Log("Error: " & Result.ErrorMsg)
                Debug.WriteLine(Result.html)
            End If
        End Using
    End Sub

    Private Sub Log(ByVal str As String)
        logtxt.Text += String.Format("[{0}] {1} {2}", DateAndTime.Now.ToShortTimeString, str, vbCrLf)
        logtxt.Select(logtxt.Text.Length - 1, 1)
        logtxt.ScrollToCaret()
        Application.DoEvents()
    End Sub

End Class
