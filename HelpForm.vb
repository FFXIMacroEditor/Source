Imports System.Text.RegularExpressions
Public Class Help
    Private Sub Help_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim FindHeaders As Match = New Regex("(##.*)").Match(HelpInfo.Text)
        Dim rotation As Integer = 0
        Do While FindHeaders.Success
            HelpInfo.SelectionStart = FindHeaders.Groups(0).Index - rotation
            HelpInfo.SelectionLength = FindHeaders.Groups(0).Length
            HelpInfo.SelectionFont = New System.Drawing.Font(Font.FontFamily, 25, Font.Style)
            HelpInfo.SelectedText = HelpInfo.SelectedText.Substring(2)
            rotation += 2
            FindHeaders = FindHeaders.NextMatch()
        Loop
        HelpInfo.BackColor = Color.White

        Dim FindSpecials As Match = New Regex("@([^@]+)@").Match(HelpInfo.Text)
        rotation = 0
        Do While FindSpecials.Success
            HelpInfo.SelectionStart = FindSpecials.Groups(0).Index - rotation
            HelpInfo.SelectionLength = FindSpecials.Groups(0).Length
            HelpInfo.SelectionColor = Color.Red
            HelpInfo.SelectedText = FindSpecials.Groups(1).Value
            rotation += 2
            FindSpecials = FindSpecials.NextMatch()
        Loop
        HelpInfo.BackColor = Me.BackColor

        HelpInfo.SelectionStart = 0
        HelpInfo.SelectionLength = 0
        HelpInfo.ScrollToCaret()

    End Sub
End Class