<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class mainfrm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.vt = New System.Windows.Forms.ComboBox()
        Me.usr = New System.Windows.Forms.TextBox()
        Me.pass = New System.Windows.Forms.TextBox()
        Me.url = New System.Windows.Forms.TextBox()
        Me.vote = New System.Windows.Forms.Button()
        Me.logtxt = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(58, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Username:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(218, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Password:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(39, 35)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(23, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Url:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(31, 61)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(31, 13)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "Cast:"
        '
        'vt
        '
        Me.vt.FormattingEnabled = True
        Me.vt.Items.AddRange(New Object() {"Vote Up", "Vote Down", "Vote Neutral"})
        Me.vt.Location = New System.Drawing.Point(68, 58)
        Me.vt.Name = "vt"
        Me.vt.Size = New System.Drawing.Size(144, 21)
        Me.vt.TabIndex = 4
        Me.vt.Text = "Vote Up"
        '
        'usr
        '
        Me.usr.Location = New System.Drawing.Point(68, 6)
        Me.usr.Name = "usr"
        Me.usr.Size = New System.Drawing.Size(144, 20)
        Me.usr.TabIndex = 5
        '
        'pass
        '
        Me.pass.Location = New System.Drawing.Point(274, 6)
        Me.pass.Name = "pass"
        Me.pass.Size = New System.Drawing.Size(144, 20)
        Me.pass.TabIndex = 6
        '
        'url
        '
        Me.url.Location = New System.Drawing.Point(68, 32)
        Me.url.Name = "url"
        Me.url.Size = New System.Drawing.Size(350, 20)
        Me.url.TabIndex = 7
        '
        'vote
        '
        Me.vote.Location = New System.Drawing.Point(221, 56)
        Me.vote.Name = "vote"
        Me.vote.Size = New System.Drawing.Size(197, 23)
        Me.vote.TabIndex = 8
        Me.vote.Text = "Cast Vote"
        Me.vote.UseVisualStyleBackColor = True
        '
        'logtxt
        '
        Me.logtxt.Location = New System.Drawing.Point(15, 85)
        Me.logtxt.Multiline = True
        Me.logtxt.Name = "logtxt"
        Me.logtxt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.logtxt.Size = New System.Drawing.Size(403, 99)
        Me.logtxt.TabIndex = 9
        '
        'mainfrm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(427, 196)
        Me.Controls.Add(Me.logtxt)
        Me.Controls.Add(Me.vote)
        Me.Controls.Add(Me.url)
        Me.Controls.Add(Me.pass)
        Me.Controls.Add(Me.usr)
        Me.Controls.Add(Me.vt)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "mainfrm"
        Me.Text = "Reddit Vote caster example -loaylty"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents vt As System.Windows.Forms.ComboBox
    Friend WithEvents usr As System.Windows.Forms.TextBox
    Friend WithEvents pass As System.Windows.Forms.TextBox
    Friend WithEvents url As System.Windows.Forms.TextBox
    Friend WithEvents vote As System.Windows.Forms.Button
    Friend WithEvents logtxt As System.Windows.Forms.TextBox

End Class
