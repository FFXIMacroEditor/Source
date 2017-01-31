Public Class Destination
    Dim mainWin As MainForm = MainForm
    Public Rows As New Dictionary(Of Integer, Button)
    Public xBook As Integer = 0
    Public xRow As Integer = 0
    Public tMacro As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For i = 0 To mainWin.Contents.Items.Count - 1
            DestContents.Items.Add(mainWin.Contents.Items(i))
        Next

        For i As Integer = 0 To 9
            Dim Row As New Button
            Row = New Button
            Row.Height = 30
            Row.Width = 40
            Row.Left = 135 + 50 * (i + 1)
            Row.Top = 11
            Row.Tag = i
            Row.Text = (i + 1).ToString()
            Row.Name = "Row" & i
            Row.BackColor = Me.BackColor
            Rows.Add(i, Row)
            Me.Controls.Add(Row)
            AddHandler Row.Click, AddressOf Row_Click
        Next
        Dim okbutton As New Button
        okbutton.Top = 11
        okbutton.Left = 700
        okbutton.Height = 30
        okbutton.Width = 40
        okbutton.Text = "OK"
        Me.Controls.Add(okbutton)
        AddHandler okbutton.Click, Function()
                                       Array.Copy({"/macro book " & (xBook + 1), "/macro set " & (xRow + 1), "", "", "", ""}, 0, mainWin.MacroContainer(mainWin.xBook, mainWin.xRow, tMacro), 1, 6)
                                       Me.Close()
                                       mainWin.Ctrls(tMacro).PerformClick()
                                       Return True
                                   End Function
        Dim cancelButton As New Button
        cancelButton.Top = 11
        cancelButton.Left = 750
        cancelButton.Height = 30
        cancelButton.Width = 100
        cancelButton.Text = "Cancel"
        Me.Controls.Add(cancelButton)
        AddHandler cancelButton.Click, Function()
                                           Me.Close()
                                           Return True
                                       End Function
        AddHandler DestContents.SelectedIndexChanged, AddressOf DestContents_SelectedIndexChanged
        DestContents.SelectedIndex = xBook

        Me.Left = mainWin.Left + 5
        Me.Top = mainWin.Top + 50
    End Sub

    Private Sub DestContents_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        xBook = DestContents.SelectedIndex
        Rows(xRow).PerformClick()
    End Sub

    Private Sub Row_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim CtrlNames() As String = New String(9) {}
        Dim AltNames() As String = New String(9) {}

        xRow = sender.tag
        For m = 0 To 9
            CtrlNames(m) = "[" & mainWin.MacroContainer(xBook, xRow, m)(0).PadRight(8, " ") & "]"
        Next
        For m = 10 To 19
            AltNames(m - 10) = "[" & mainWin.MacroContainer(xBook, xRow, m - 10)(0).PadRight(8, " ") & "]"
        Next

        ControlTItles.Text = String.Join("", CtrlNames)
        AlternateTitles.Text = String.Join("", AltNames)
    End Sub

    Private Sub Destination_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.Black, ButtonBorderStyle.Solid)
    End Sub
End Class