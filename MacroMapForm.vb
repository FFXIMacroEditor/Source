Public Class MacroMapForm
    Public Function Add(b As Integer, r As Integer, m As Integer, a As String())
        Dim mBox As Label = New Label
        mBox.BackColor = System.Drawing.SystemColors.ControlLight
        mBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        mBox.Location = New System.Drawing.Point(13, 13)
        mBox.Size = New System.Drawing.Size(150, 100)
        mBox.TabIndex = 0
        mBox.Tag = b & "," & r & "," & m
        If m < 10 Then
            mBox.Top = 20 + ((mBox.Height + 20) * 2 * r)
            mBox.Left = 170 * m + 20
        Else
            mBox.Top = mBox.Height + 40 + ((mBox.Height + 20) * 2 * r)
            mBox.Left = 170 * (m - 10) + 20
        End If
        mBox.Text = Strings.Join(a, Chr(13) & Chr(10))
        AddHandler mBox.MouseDown, AddressOf mbox_Click
        Me.Controls.Add(mBox)
        Return True
    End Function

    Private Sub mbox_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim x = sender.tag.split(",")
        Me.ActiveControl = Nothing
        MainForm.FindMacro(x(0), x(1), x(2))
    End Sub

    Private Sub MacroMap_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True
        Me.ActiveControl = Nothing
    End Sub
End Class