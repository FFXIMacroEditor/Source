Option Strict Off
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.ComponentModel


Public Class MainForm
    Const WM_CUT = 768 '&H300 'Cut (Ctrl+X)
    Const WM_COPY = 769 '&H301 'Copy (Ctrl+C)
    Const WM_PASTE = 770 '&H302 'Paste (Ctrl+V)
    Const WM_CLEAR = 771 '&H303 'Delete (Del)
    Const WM_UNDO = 772 '&H304 'Undo (Ctrl+Z)

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        If keyData = (Keys.Control Or Keys.V) Then
            If Me.ActiveControl.Name.StartsWith("Lines") Then
                menuText_Paste.PerformClick()
                Return True
            ElseIf Me.ActiveControl.Name.StartsWith("Ctrl") And CleanClipBoard().StartsWith("Type: Macro") Then
                menuText_Paste.PerformClick()
                Return True
            End If
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Public Ctrls As New Dictionary(Of Integer, Button) ' Holds all the ctrl and alt macro buttons
    Public Rows As New Dictionary(Of Integer, Button) ' Holds the 10 row buttons, (0) = "1", ... (9) = "10"
    Public Lines As New Dictionary(Of Integer, TextBox) ' Holds all 7 macro lines, title included
    Public macropath As String = GetFFXIDirectory() & "USER\"
    Public importpath As String = GetFFXIDirectory() & "USER\"
    Public MacroContainer As String(,,)() = New String(19, 9, 19)() {} ' The in-memory storage of all the macro data
    Public MacroPreserved As String(,,)() = New String(19, 9, 19)() {} ' The in-memory backup of all the same data
    Public Contents As New ListBox ' Macro Books
    Public bWidth As Integer = 60 ' Macro button width
    Public bHeight As Integer = 50 ' Macro Button Height
    Public cbook As Integer = 0 ' Current Book, set when right-clicking a macro book
    Public xBook As Integer = 0 ' Selected book in the book list
    Public xRow As Integer = 0 ' Selected Row
    Public xMacro As Integer = 0 ' Selected Macro. 0-9 Ctrl, 10-19 Alt
    Public handlerStart As Integer = 0 ' Set when interacting with one of the two macro ". . ." macro buttons
    Public handlerEnd As Integer = 9 ' Set along with Handler Start
    Public Macroholder As String() ' Internal clipboard holding a macro
    Public RowHolder As String()() = New String(19)() {} ' Internal clipboard holding a row
    Public BookHolder As String(,)() = New String(9, 19)() {} ' Internal clipboard for holding a Book
    Public debuglimit As Integer = 19 ' Has to do with loading a limited number of books for testing, 0-19
    Public copiedbookname As String = "" ' For copying a book, stores the copied book name
    Public ATmenu As ToolStripMenuItem = New ToolStripMenuItem ' Auto-translate menu
    Public ATObject As New Dictionary(Of String, String) ' Auto
    Public CurrentLine As Integer = -1 ' Current selected macro line
    Public InternalClipboardMethod As String = ""
    Public SomethingEdited As Boolean = False ' Flag for a safety while closing application

    Public rs As New Resizer ' dynamic control resizer
    Public ATReadable As New Regex("\xFD(.{4})\xFD") ' For converting auto translate phrases to something readable
    Public ATWritable As New Regex("\<(.{8})\|[^<>]+\>") ' Converting back to proper file-written format
    Public lfs As New Regex("\r|\n") ' Sterilizer to make sure there are no linefeeds in textboxes
    Public Evaluation As New Assessment ' Instance of Assessment form for evaluation of macros
    Public SearchResults As New Assessment ' Instance of Assessment form for Finding a keyword
    Public MacroMap As New MacroMapForm ' Instance of Macro Map

    'Preserved for pattern reference
    'Public rgx_Macros As New Regex("[^\x00]{4}([^\x00]{366})([^\x00]{10})(?=([^\x00]{380})*$)")
    'Public rgx_Macro As New Regex("([^\x00]{60})[^\x00]")

    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        If Contents.Enabled = False Then Exit Sub
        If e.Control Then
            If e.KeyCode = Keys.Left Then 'control+left
                If xMacro >= 10 Then xMacro = -1
                Ctrls(Math.Max(0, xMacro - 1)).PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode = Keys.Right Then 'control+right
                If xMacro >= 10 Then xMacro = -1
                Ctrls(Math.Min(9, xMacro + 1)).PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode = 38 Then
                Rows(Math.Max(xRow - 1, 0)).PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode = 40 Then
                Rows(Math.Min(xRow + 1, 9)).PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode >= 48 And e.KeyCode <= 57 Then
                If e.KeyCode > 48 Then
                    Ctrls(e.KeyCode - 49).PerformClick()
                ElseIf e.KeyCode = 48 Then
                    Ctrls(9).PerformClick()
                End If
                e.SuppressKeyPress = True
            ElseIf e.Shift And (e.keycode = Keys.Tab OrElse e.KeyCode = Keys.Enter) Then
                Ctrls(Math.Max(0, xMacro - 1)).PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode = Keys.Tab OrElse e.KeyCode = Keys.enter Then
                Ctrls(Math.Min(19, xMacro + 1)).PerformClick()
                e.SuppressKeyPress = True
            End If
        ElseIf e.Alt Then
            e.SuppressKeyPress = True
            If e.KeyCode = 37 Then 'control+left
                If xMacro < 10 Then xMacro = 9
                Ctrls(Math.Max(10, xMacro - 1)).PerformClick()
            ElseIf e.KeyCode = 39 Then 'control+right
                If xMacro < 10 Then xMacro = 9
                Ctrls(Math.Min(19, xMacro + 1)).PerformClick()
            ElseIf e.KeyCode = 38 Then
                Contents.SelectedIndex = Math.Max(Math.Min(xBook - 1, Contents.Items.Count - 1), 0)
            ElseIf e.KeyCode = 40 Then
                Contents.SelectedIndex = Math.Min(Math.Max(xBook + 1, 0), Contents.Items.Count - 1)
            ElseIf e.KeyCode >= 48 And e.KeyCode <= 57 Then
                If e.KeyCode > 48 Then
                    Ctrls(e.KeyCode - 39).PerformClick()
                ElseIf e.KeyCode = 48 Then
                    Ctrls(19).PerformClick()
                End If
            End If
        End If
    End Sub

    Private Sub Form1_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        If Not Directory.Exists(My.Settings.UserDirectory) Then
            GetFFXIDirectory()
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Contents.Top = 40
        Contents.Left = 20
        Contents.Height = 400
        Contents.Width = 115
        Contents.Name = "Contents"
        Contents.Font = New System.Drawing.Font(Font.FontFamily, 11, Font.Style)
        Me.Height = Contents.Top + Contents.Height + Contents.Top + 20
        Me.Width = 990
        AddHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        AddHandler Contents.MouseDown, AddressOf Contents_MouseDown

        Me.Controls.Add(Contents)
        For i As Integer = 0 To 9
            Dim Row As New Button
            Me.Controls.Add(Row)
            Row.Height = 30
            Row.Width = 40
            Row.Left = 145
            Row.Top = 40 * (i + 1)
            Row.Tag = i
            Row.Font = Font
            Row.Text = (i + 1).ToString()
            Row.Name = "Row" & i
            Row.BackColor = Me.BackColor
            Rows.Add(i, Row)
            AddHandler Row.Click, AddressOf Row_Click
            AddHandler Row.MouseDown, AddressOf Row_Mousedown
        Next

        For i As Integer = 0 To 19
            Dim Ctrl As New Button
            Me.Controls.Add(Ctrl)
            Ctrl.Height = bHeight
            Ctrl.Width = bWidth
            Ctrl.Tag = i
            Ctrl.BackColor = Me.BackColor
            Ctrl.Name = "Ctrl" & i
            If i <= 9 Then
                Ctrl.Left = 195 + ((i + 1) * (bWidth + 10))
                Ctrl.Top = 40
                Ctrl.Text = "C" & TenToZero(i + 1)
                Ctrls.Add(i, Ctrl)
            Else
                Ctrl.Left = 195 + ((i + 1 - 10) * (bWidth + 10))
                Ctrl.Top = bHeight + 50
                Ctrl.Text = "A" & TenToZero(i - 9)
                Ctrls.Add(i, Ctrl)
            End If
            AddHandler Ctrl.Click, AddressOf Control_Click
            AddHandler Ctrl.MouseEnter, AddressOf Control_MouseEnter
            AddHandler Ctrl.MouseDown, AddressOf Control_Mousedown
        Next

        Dim LH As New Button
        Me.Controls.Add(LH)
        LH.Left = 195
        LH.Top = 40
        LH.Width = bWidth
        LH.Height = 30
        LH.Name = "LeftHandler"
        LH.BackColor = Me.BackColor
        LH.Text = "● ● ●"
        AddHandler LH.MouseDown, AddressOf RowHandler_Mousedown


        LH = New Button
        Me.Controls.Add(LH)
        LH.Left = 195
        LH.Top = 80
        LH.Width = bWidth
        LH.Height = 30
        LH.Name = "FlipHandler"
        LH.BackColor = Me.BackColor
        LH.Text = "▲ ▼"
        AddHandler LH.MouseDown, AddressOf RowHandler_Mousedown


        LH = New Button
        Me.Controls.Add(LH)
        LH.Left = 195
        LH.Top = 120
        LH.Width = bWidth
        LH.Height = 30
        LH.Name = "RightHandler"
        LH.BackColor = Me.BackColor
        LH.Text = "● ● ●"
        AddHandler LH.MouseDown, AddressOf RowHandler_Mousedown

        For i As Integer = 0 To 6
            Dim iLine As New TextBox()
            If i = 0 Then
                iLine.Left = LH.Left
                iLine.MaxLength = 8
            Else
                iLine.Left = LH.Left + 150
                iLine.MaxLength = 300
                iLine.ContextMenuStrip = MenuText
            End If
            iLine.Width = Ctrls(9).Left + Ctrls(9).Width - iLine.Left
            iLine.Top = Rows(3 + i).Top
            iLine.Tag = i
            iLine.Font = New System.Drawing.Font(Font.FontFamily, 18, Font.Style)
            iLine.Name = "Lines" & i
            Lines.Add(i, iLine)
            Me.Controls.Add(iLine)
            AddHandler iLine.KeyDown, AddressOf lines_KeyDown
            AddHandler iLine.GotFocus, AddressOf Lines_GotFocus
            AddHandler iLine.TextChanged, AddressOf Lines_TextChanged
        Next

        For Each a In Me.Controls
            If TypeOf a Is Button Or TypeOf a Is ListBox Then
                a.tabstop = False
            End If

            If Not (a.name = "MainMenu") Then
                a.enabled = False
            End If
        Next

        File_SaveRow.Enabled = False
        File_SaveAll.Enabled = False
        StatusBar.Enabled = True
        StatusH.Enabled = True
        StatusD.Enabled = True

        ParseAT()

        rs.FindAllControls(Me)

        With Contents
            Warning.Top = .Top
            Warning.Left = .Left
            Warning.Height = .Height
            Warning.Width = .Width
        End With
    End Sub

    Private Sub ATPhrase_Click(ByVal sender As Object, ByVal e As EventArgs)
        Lines(CurrentLine).SelectedText = sender.tag
    End Sub

    Private Sub Lines_TextChanged(sender As Object, e As EventArgs)
        If sender.tag = 0 Then
            Dim ta As String = Lines(0).Text.Trim() & " "
            If xMacro < 10 Then
                Ctrls(xMacro).Text = "C" & TenToZero(xMacro + 1) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
            Else
                Ctrls(xMacro).Text = "A" & TenToZero(xMacro - 9) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
            End If
            If sender.text.length > 8 Then
                Dim selectionstart As Integer = sender.selectionstart
                sender.text = sender.text.ToString().Substring(0, 8)
                sender.selectionstart = Math.Min(selectionstart, 8)
            End If
        End If
        SomethingEdited = True
        If Trim(sender.text).Length = 0 Then
            MacroContainer(xBook, xRow, xMacro)(sender.tag) = ""
        Else
            MacroContainer(xBook, xRow, xMacro)(sender.tag) = sender.text
        End If
    End Sub

    Private Sub lines_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.F2 And CurrentLine > 0 Then
            e.Handled = True
            MenuText_Opening(sender, New CancelEventArgs)
            MenuText.Show(New Point(Me.Left + sender.left + sender.width, Me.Top + sender.top))
        ElseIf e.KeyCode = Keys.Enter And CurrentLine < 6 Then
            If sender.selectionlength > 0 Then
                sender.selectionstart += sender.selectionlength
                sender.selectionlength = 0
                e.SuppressKeyPress = True
            ElseIf CurrentLine < 6 Then
                e.SuppressKeyPress = True
                Lines(CurrentLine + 1).Focus()
            End If
        ElseIf e.KeyCode = Keys.Enter And CurrentLine = 6 Then
            Lines(0).Focus()
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            If sender.selectionlength > 0 Then
                sender.selectionlength = 0
                e.SuppressKeyPress = True
            ElseIf CurrentLine < 6 Then
                e.SuppressKeyPress = True
                Lines(0).Focus()
            Else
                e.SuppressKeyPress = True
            End If
        End If
    End Sub

    Private Sub Lines_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        CurrentLine = sender.tag
    End Sub

    Private Sub Contents_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim targetbook As Integer = Contents.IndexFromPoint(New Point(e.X, e.Y))
        If targetbook >= 0 Then
            cbook = targetbook
            If e.Button = MouseButtons.Right Then
                MenuBook.Show(sender, New System.Drawing.Point(e.X + 10, cbook * Contents.ItemHeight + 10))
            End If
        End If
    End Sub

    Private Sub Contents_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        xBook = Contents.SelectedIndex
        Rows(xRow).PerformClick()
    End Sub

    Private Sub Row_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        xRow = sender.tag
        For i = 0 To 19
            Dim ta As String = MacroContainer(xBook, xRow, i)(0).Trim() & " "
            If i < 10 Then
                Ctrls(i).Text = "C" & TenToZero(i + 1) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
            Else
                Ctrls(i).Text = "A" & TenToZero(i - 9) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
            End If
        Next
        For Each a In Me.Controls
            If TypeOf a Is Button And a.name.ToString().StartsWith("Row") Then
                a.backcolor = Me.BackColor
            End If
        Next
        sender.BackColor = Color.LightGray
        Ctrls(xMacro).PerformClick()
    End Sub

    Private Sub Control_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        ControlTip.SetToolTip(sender, String.Join(Chr(10), MacroContainer(xBook, xRow, sender.tag)))
    End Sub

    Private Sub Control_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim SomethingEditedValue As Boolean = SomethingEdited
        xMacro = sender.tag
        For i = 0 To MacroContainer(xBook, xRow, sender.Tag).Length - 1
            Lines(i).Text = MacroContainer(xBook, xRow, sender.Tag)(i)
        Next

        For Each a In Me.Controls
            If TypeOf a Is Button And Not a.name.ToString().StartsWith("Row") Then
                a.backcolor = Me.BackColor
            End If
        Next
        sender.BackColor = Color.LightGray
        ' Updating the text to the current macro will try to set SomethingEdited to true.
        SomethingEdited = SomethingEditedValue
    End Sub

    Private Sub Control_Mousedown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Button = MouseButtons.Right Then
            MenuMacro.Show(sender, New System.Drawing.Point(40, 40))
        End If
    End Sub

    Private Sub Row_Mousedown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Button = MouseButtons.Right Then
            MenuRow.Show(sender, New System.Drawing.Point(20, 20))
        End If
    End Sub

    Private Sub RowHandler_Mousedown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If sender.name = "LeftHandler" Then
            handlerStart = 0
            handlerEnd = 9
            MenuHandler.Show(sender, New System.Drawing.Point(20, 20))
        ElseIf sender.name = "RightHandler" Then
            handlerStart = 10
            handlerEnd = 19
            MenuHandler.Show(sender, New System.Drawing.Point(20, 20))
        ElseIf sender.name = "FlipHandler" Then
            handlerStart = 0
            handlerEnd = 0
            Dim tmpHolder As String()() = New String(19)() {}
            For m = 0 To 9
                tmpHolder(m) = MacroContainer(xBook, xRow, m)
                MacroContainer(xBook, xRow, m) = MacroContainer(xBook, xRow, m + 10)
            Next
            For m = 10 To 19
                MacroContainer(xBook, xRow, m) = tmpHolder(m - 10)
            Next
            SomethingEdited = True
            Rows(xRow).PerformClick()
        End If
    End Sub

    Private Sub MenuMacro_Paste_Click(sender As Object, e As EventArgs) Handles MenuMacro_Paste.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        MacroContainer(xBook, xRow, x) = Macroholder
        Dim ta As String = Macroholder(0) & " "
        If x < 10 Then
            Ctrls(x).Text = "C" & TenToZero(x + 1) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
        Else
            Ctrls(x).Text = "A" & TenToZero(x - 9) & Chr(10) & ta.Substring(0, Math.Min(5, ta.Length))
        End If
        SomethingEdited = True
        Ctrls(x).PerformClick()
    End Sub

    Private Sub MenuBook_Paste_Click(sender As Object, e As EventArgs) Handles MenuBook_Paste.Click
        For r = 0 To 9
            For m = 0 To 19
                Try
                    MacroContainer(cbook, r, m) = BookHolder(r, m)
                Catch
                    For l = 0 To 6
                        Try
                            MacroContainer(cbook, r, m)(l) = BookHolder(r, m)(l)
                        Catch
                            MacroContainer(cbook, r, m)(l) = ""
                        End Try
                    Next
                End Try
            Next
            SomethingEdited = True
        Next
        RemoveHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        Contents.Items(cbook) = copiedbookname
        AddHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        Contents.SelectedIndex = cbook
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuBook_Revert_Click(sender As Object, e As EventArgs) Handles MenuBook_Revert.Click
        For r = 0 To 9
            For m = 0 To 19
                MacroContainer(cbook, r, m) = MacroPreserved(cbook, r, m).Clone()
            Next
        Next
        SomethingEdited = True
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuBook_Copy_Click(sender As Object, e As EventArgs) Handles MenuBook_Copy.Click
        For r = 0 To 9
            For m = 0 To 19
                BookHolder(r, m) = MacroContainer(cbook, r, m)
            Next
        Next
        InternalClipboardMethod = "Book"
        copiedbookname = Contents.Items(cbook)
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuBook_Cut_Click(sender As Object, e As EventArgs) Handles MenuBook_Cut.Click
        MenuBook_Copy_Click(sender, e)
        MenuBook_Clear_Click(sender, e)
    End Sub

    Private Sub MenuBook_Clear_Click(sender As Object, e As EventArgs) Handles MenuBook_Clear.Click
        For r = 0 To 9
            For m = 0 To 19
                MacroContainer(cbook, r, m) = {"", "", "", "", "", "", ""}
            Next
        Next
        SomethingEdited = True
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuBook_CopyClipboard_Click(sender As Object, e As EventArgs) Handles MenuBook_CopyClipboard.Click
        Dim cx1 As String()() = New String(20)() {}
        Dim cx2 As String() = New String(11) {}
        cx2(0) = "Type: Book " & Contents.Items(cbook)
        For r = 0 To 9
            cx1(r) = {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""}
            For m = 0 To 19
                cx1(r)(m) = String.Join(Chr(10), MacroContainer(cbook, r, m)).TrimEnd()
            Next
            cx2(r + 1) = String.Join(Chr(10) & Chr(10) & "Macro:" & Chr(10), cx1(r))
        Next
        cx2(11) = "EndBook (Please do not remove empty lines as they're part of the sharing format). If you experience problems pasting, please make sure to download an up to date version."
        Clipboard.SetText(String.Join(Chr(10) & Chr(10) & "Row, Macro:" & Chr(10), cx2))
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuBook_PasteClipboard_Click(sender As Object, e As EventArgs, Optional clp As String = "") Handles MenuBook_PasteClipboard.Click
        Dim cx As String()
        If clp Is "" Then
            cx = Strings.Split(CleanClipBoard(), Chr(10) & Chr(10) & "Row, Macro:" & Chr(10))
        Else
            cx = Strings.Split(clp, Chr(10) & Chr(10) & "Row, Macro:" & Chr(10))
        End If
        If verifyclipboard("Book", cx) = False Then
            Exit Sub
        End If
        RemoveHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        If cx(0).Substring(0, 10) = "Type: Book" Then
            Contents.Items(cbook) = cx(0).Substring(11, Math.Min(10, cx(0).Length - 11))
            cx = cx.Skip(1).ToArray()
            Array.Resize(cx, cx.Length - 1)
            For r = 0 To 9
                Dim cx1 As String() = Strings.Split(cx(r), Chr(10) & Chr(10) & "Macro:" & Chr(10))
                For m = 0 To 19
                    MacroContainer(xBook, r, m) = {"", "", "", "", "", "", ""}
                    Dim cx2 As String() = cx1(m).Split((Chr(10)))
                    For l = 0 To Math.Min(cx2.Length - 1, 6)
                        MacroContainer(cbook, r, m)(l) = cx2(l)
                    Next
                Next
            Next
        End If
        SomethingEdited = True
        AddHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        Rows(xRow).PerformClick()
    End Sub


    Private Sub MenuMacro_Revert_Click(sender As Object, e As EventArgs) Handles MenuMacro_Revert.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        MacroContainer(xBook, xRow, x) = MacroPreserved(xBook, xRow, x).Clone()
        SomethingEdited = True
        Ctrls(x).PerformClick()
    End Sub

    Private Sub MenuMacro_Copy_Click(sender As Object, e As EventArgs) Handles MenuMacro_Copy.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Macroholder = MacroContainer(xBook, xRow, x)
        InternalClipboardMethod = "Macro"
        Ctrls(x).PerformClick()
    End Sub

    Private Sub MenuMacro_Cut_Click(sender As Object, e As EventArgs) Handles MenuMacro_Cut.Click
        MenuMacro_Copy_Click(sender, e)
        MenuMacro_Clear_Click(sender, e)
    End Sub

    Private Sub MenuMacro_Clear_Click(sender As Object, e As EventArgs) Handles MenuMacro_Clear.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        MacroContainer(xBook, xRow, x) = {"", "", "", "", "", "", ""}
        SomethingEdited = True
        Ctrls(x).PerformClick()
    End Sub

    Private Sub MenuMacro_CopyClipboard_Click(sender As Object, e As EventArgs) Handles MenuMacro_CopyClipboard.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Dim c As String = "Type: Macro" & Chr(10) & String.Join(Chr(10), MacroContainer(xBook, xRow, x)).TrimEnd() & Chr(10) & "EndMacro (Please do not remove empty lines as they're part of the sharing format). If you experience problems pasting, please make sure to download an up to date version."
        Clipboard.SetText(c)
    End Sub

    Private Sub MenuMacro_PasteClipboard_Click(sender As Object, e As EventArgs, Optional x As Integer = -1) Handles MenuMacro_PasteClipboard.Click
        If x = -1 Then
            x = (sender.GetCurrentParent()).SourceControl.tag
        End If
        Dim c As String() = CleanClipBoard().Split(Chr(10))
        If c(0).StartsWith("Type: Macro") Then
            MacroContainer(xBook, xRow, x) = {"", "", "", "", "", "", ""}
            For i = 1 To Math.Min(c.Length - 2, 7)
                MacroContainer(xBook, xRow, x)(i - 1) = c(i)
            Next
        ElseIf c(0).StartsWith("Type: ") Then
            MsgBox("Clipboard data contains " & c(0) & ", canceling paste to this macro.")
        ElseIf c.Length = 7 Then
            MenuMacro_Clear.PerformClick()
            MacroContainer(xBook, xRow, x) = c
            SomethingEdited = True
        ElseIf c.Length < 7 Then
            MenuMacro_Clear.PerformClick()
            Dim p As Integer = MessageBox.Show("Is the first line the title?", "Macro Editor", MessageBoxButtons.YesNo)
            If p = DialogResult.Yes Then
                MacroContainer(xBook, xRow, x) = {"", "", "", "", "", "", ""}
                For i = 0 To c.Length - 1
                    MacroContainer(xBook, xRow, x)(i) = c(i)
                Next
            Else
                Dim q = InputBox("Enter the title for the macro", "Macro Editor", MessageBoxButtons.OK)
                MacroContainer(xBook, xRow, x) = {q, "", "", "", "", "", ""}
                For i = 0 To c.Length - 1
                    MacroContainer(xBook, xRow, x)(i + 1) = c(i)
                Next
            End If
            SomethingEdited = True
        End If
        Rows(xRow).PerformClick()
    End Sub

    Private Sub MenuRow_Paste_Click(sender As Object, e As EventArgs) Handles MenuRow_Paste.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        For m = 0 To 19
            MacroContainer(xBook, x, m) = RowHolder(m)
        Next
        SomethingEdited = True
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuRow_Revert_Click(sender As Object, e As EventArgs) Handles MenuRow_Revert.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        For i = 0 To 19
            MacroContainer(xBook, x, i) = MacroPreserved(xBook, x, i).Clone()
        Next
        SomethingEdited = True
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuRow_Copy_Click(sender As Object, e As EventArgs) Handles MenuRow_Copy.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        For m = 0 To 19
            RowHolder(m) = MacroContainer(xBook, x, m)
        Next
        InternalClipboardMethod = "Row"
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuRow_Cut_Click(sender As Object, e As EventArgs) Handles MenuRow_Cut.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        For m = 0 To 19
            RowHolder(m) = MacroContainer(xBook, x, m)
            MacroContainer(xBook, x, m) = {"", "", "", "", "", "", ""}
        Next
        SomethingEdited = True
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuRow_Clear_Click(sender As Object, e As EventArgs) Handles MenuRow_Clear.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        For i = 0 To 19
            MacroContainer(xBook, x, i) = {"", "", "", "", "", "", ""}
        Next

        SomethingEdited = True
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuRow_CopyClipboard_Click(sender As Object, e As EventArgs) Handles MenuRow_CopyClipboard.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Dim cx1 As String() = New String(21) {}
        cx1(0) = "Type: Row"
        For m = 0 To 19
            cx1(m + 1) = String.Join(Chr(10), MacroContainer(xBook, x, m)).TrimEnd()
        Next
        cx1(21) = "EndRow (Please do not remove empty lines as they're part of the sharing format). If you experience problems pasting, please make sure to download an up to date version."
        Clipboard.SetText(String.Join(Chr(10) & Chr(10) & "Macro:" & Chr(10), cx1))
    End Sub


    Private Sub MenuRow_PasteClipboard_Click(sender As Object, e As EventArgs) Handles MenuRow_PasteClipboard.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Dim cx1 As String() = Strings.Split(CleanClipBoard(), Chr(10) & Chr(10) & "Macro:" & Chr(10))
        If verifyclipboard("Row", cx1) = False Then
            Exit Sub
        End If
        cx1 = cx1.Skip(1).ToArray()
        Array.Resize(cx1, cx1.Length - 1)
        For m = 0 To 19
            Dim cx2 As String() = cx1(m).Split((Chr(10)))
            MacroContainer(xBook, x, m) = {"", "", "", "", "", "", ""}
            For l = 0 To Math.Min(cx2.Length - 1, 6)
                Try
                    MacroContainer(xBook, x, m)(l) = cx2(l)
                Catch
                    MacroContainer(xBook, x, m)(l) = ""
                End Try
            Next
        Next
        SomethingEdited = True
        Rows(x).PerformClick()
    End Sub

    Private Sub MenuMain_evaluate_Click(sender As Object, e As EventArgs) Handles MenuMain_evaluate.Click
        Rows(xRow).Focus()
        Dim errors As New Dictionary(Of Integer, String())
        Dim cautions As New Dictionary(Of Integer, String())
        For b = 0 To debuglimit
            For r = 0 To 9
                For m = 0 To 19
                    If MacroContainer(b, r, m)(0).Length > 8 Then
                        errors(errors.Count) = {b, r, m, 0, "8 characters max.", MacroContainer(b, r, m)(0)}
                    End If
                    For l = 1 To 6
                        Dim line As String = MacroContainer(b, r, m)(l)
                        Dim atline As String = ATWriter(line)
                        Dim nonprintables As Match = New Regex("[^\x00-\x7f]").Match(line)
                        If line.Length = 0 Then
                            ' do nothing
                        ElseIf atline.Length > 60 Then
                            errors(errors.Count) = {b, r, m, l, 2, "After auto-translate, the line is " & atline.Length & " characters."}
                        ElseIf nonprintables.Length Then
                            cautions(cautions.Count) = {b, r, m, l, 3, line}
                        ElseIf line.StartsWith("//") Then
                            cautions(cautions.Count) = {b, r, m, l, 4, line}
                        ElseIf Not line.StartsWith("/") Then
                            cautions(cautions.Count) = {b, r, m, l, 5, line}
                        ElseIf line.StartsWith("/wait") Then
                            cautions(cautions.Count) = {b, r, m, l, 6, line}
                        ElseIf line.contains("|UnknownItem>") Then
                            cautions(cautions.Count) = {b, r, m, l, 7, line}
                        End If
                    Next
                Next
            Next
        Next
        Evaluation.Hide()
        Evaluation = New Assessment
        If errors.Count + cautions.Count > 0 Then
            For Each kvp As KeyValuePair(Of Integer, String()) In errors
                Evaluation.AddResult(kvp.Value(0), kvp.Value(1), kvp.Value(2), kvp.Value(3), kvp.Value(4), kvp.Value(5), Color.Red, Color.White)
            Next
            For Each kvp As KeyValuePair(Of Integer, String()) In cautions
                Evaluation.AddResult(kvp.Value(0), kvp.Value(1), kvp.Value(2), kvp.Value(3), kvp.Value(4), kvp.Value(5), Color.White, Color.Black)
            Next
        Else
            MsgBox("No warnings or errors found.")
        End If
        Evaluation.Show(Me)
    End Sub

    Private Sub MenuHandler_CutSide_Click(sender As Object, e As EventArgs) Handles MenuHandler_CutSide.Click
        MenuHandler_CopySide_Click(sender, e)
        MenuHandler_ClearSide_Click(sender, e)
    End Sub

    Private Sub MenuHandler_CopySide_Click(sender As Object, e As EventArgs) Handles MenuHandler_CopySide.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Dim cx1 As String() = New String(11) {}
        cx1(0) = "Type: Side"
        If handlerStart = 10 Then
            For m = handlerStart To handlerEnd
                cx1(m - 10 + 1) = String.Join(Chr(10), MacroContainer(xBook, xRow, m))
            Next
        Else
            For m = handlerStart To handlerEnd
                cx1(m + 1) = String.Join(Chr(10), MacroContainer(xBook, xRow, m))
            Next
        End If
        cx1(11) = "EndSide (Please do not remove empty lines as they're part of the sharing format). If you experience problems pasting, please make sure to download an up to date version."
        Clipboard.SetText(String.Join(Chr(10) & Chr(10) & "Macro:" & Chr(10), cx1))
    End Sub

    Private Sub MenuHandler_PasteSide_Click(sender As Object, e As EventArgs) Handles MenuHandler_PasteSide.Click
        Dim x = (sender.GetCurrentParent()).SourceControl.tag
        Dim cx1 As String() = Strings.Split(CleanClipBoard(), Chr(10) & Chr(10) & "Macro:" & Chr(10))
        If cx1(0) = "Type: Side" Then
            If verifyclipboard("Side", cx1) = False Then
                Exit Sub
            End If
            cx1 = cx1.Skip(1).ToArray()
            Array.Resize(cx1, cx1.Length - 1)
            If handlerStart = 10 Then
                For m = handlerStart To handlerEnd
                    Dim cx2 As String() = cx1(m - 10).Split((Chr(10)))
                    For l = 0 To 6
                        MacroContainer(xBook, xRow, m)(l) = cx2(l)
                    Next
                Next
            Else
                For m = handlerStart To handlerEnd
                    Dim cx2 As String() = cx1(m).Split((Chr(10)))
                    For l = 0 To 6
                        MacroContainer(xBook, xRow, m)(l) = cx2(l)
                    Next
                Next
            End If
            SomethingEdited = True
            Rows(xRow).PerformClick()
        End If
    End Sub

    Private Sub MenuHandler_ClearSide_Click(sender As Object, e As EventArgs) Handles MenuHandler_ClearSide.Click
        For i = handlerStart To handlerEnd
            MacroContainer(xBook, xRow, i) = {"", "", "", "", "", "", ""}
        Next

        SomethingEdited = True
        Rows(xRow).PerformClick()
    End Sub


    Private Sub File_Open_Click(sender As Object, e As EventArgs) Handles File_Open.Click
        OpenDialog.InitialDirectory = macropath
        OpenDialog.Filter = "Macro Title Files|mcr.ttl"
        OpenDialog.FileName = "mcr.ttl"
        OpenDialog.Multiselect = False
        If OpenDialog.ShowDialog <> DialogResult.Cancel Then
            macropath = OpenDialog.FileName.Substring(0, OpenDialog.FileName.LastIndexOf("\"))
            Warning.visible = False
        Else
            Exit Sub
        End If

        Dim tocname As String = (macropath & "\mcr.ttl")
        Dim c() As Byte = IO.File.ReadAllBytes(tocname)
        Dim ctext As String = ""
        For i = 0 To c.Length - 1
            ctext += Convert.ToChar(c(i))
        Next
        Dim match As Match = New Regex("((.{15}\x00)+$)").Match(ctext)
        ctext = match.Groups(1).ToString()
        ctext = New Regex(Chr(0) + "+").Replace(ctext, ",")
        Dim result() As String
        result = ctext.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
        Contents.Items.Clear()

        For mBook = 0 To debuglimit
            UpdateStatusBar("Extracting...", result(mBook))
            Contents.Items.Add(result(mBook))
            Contents.Refresh()
            StatusBar.Refresh()
            For mSet = 0 To 9
                Dim filename As String = macropath & "\mcr" & KillZero(mBook, mSet) & ".dat"
                If Not File.Exists(filename) Then
                    For m = 0 To 19
                        MacroContainer(mBook, mSet, m) = {"", "", "", "", "", "", ""}
                        MacroPreserved(mBook, mSet, m) = MacroContainer(mBook, mSet, m).Clone()
                    Next
                    Continue For
                End If
                ReadMacroFile(mBook, mSet, filename, True)
            Next
        Next
        UpdateStatusBar("Extraction complete", "")

        For Each a In Me.Controls
            a.enabled = True
        Next
        MenuMain_evaluate.Enabled = True
        MenuMain_Search.Enabled = True
        Contents.SelectedIndex = 0
        File_SaveRow.Enabled = True
        File_SaveAll.Enabled = True
        SomethingEdited = False
    End Sub

    Private Sub File_SaveRow_Click(sender As Object, e As EventArgs) Handles File_SaveRow.Click
        MenuRow_Save_Click(Rows(xRow), e)
    End Sub

    Private Sub File_SaveAll_Click(sender As Object, e As EventArgs) Handles File_SaveAll.Click
        Dim p As Integer = MessageBox.Show("This will save all macros to file." & Chr(10) & Chr(10) &
                                                "Proceed?" & Chr(10) & Chr(10) &
                                                "This will also update the in-memory backup that can" & Chr(10) &
                                                "be restored with Revert from Main, Book, Row, & Macro menus.", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.Yes Then
            For mBook = 0 To 19
                For mRow = 0 To 9
                    If WriteFile(mBook, mRow) = False Then
                        StatusH.Text = "Error"
                        StatusD.Text = "An error ocurred writing files."
                        Exit Sub
                    End If
                    UpdateStatusBar(Contents.Items(mBook), "Writing " & KillZero(mBook, mRow) & ".dat")
                    StatusBar.Refresh()
                Next
            Next
        End If

        p = MessageBox.Show("Now, save macro titles?", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.Yes Then
            MenuBook_SaveBookNames.PerformClick()
            UpdateStatusBar("Titles saved.", "Save Complete.")
        End If
    End Sub

    Private Sub MenuHelp_Help_Click(sender As Object, e As EventArgs) Handles MenuHelp_Help.Click
        Dim HelpForm As New Help
        HelpForm.ShowDialog()
    End Sub

    Private Sub MenuBook_SaveBookNames_Click(sender As Object, e As EventArgs) Handles MenuBook_SaveBookNames.Click
        Dim fname As String = macropath & "\mcr.ttl"
        Dim b() As Byte = IO.File.ReadAllBytes(fname)

        Dim sb As New StringBuilder
        For bk = 0 To 19
            sb.Append(fill(Contents.Items(bk).trim().Substring(0, Math.Min(15, Contents.Items(bk).length)), 16))
        Next

        If sb.Length <> 320 Then
            MsgBox("Compilation of Macro Book titles failed")
            Exit Sub
        End If

        Dim StringBytes() As Byte = Encoding.Default.GetBytes(sb.ToString())

        Dim HashMaker As New MD5CryptoServiceProvider()
        Dim HashBytes As Byte() = HashMaker.ComputeHash(StringBytes)
        Dim Hash As New StringBuilder()
        For hx = 0 To HashBytes.Length - 1
            Hash.Append(Chr(HashBytes(hx)))
        Next

        If sb.Length <> 320 Then
            MsgBox("Compilation of Macro Names file failed.")
            Exit Sub
        End If

        Dim FileBytes(8 + HashBytes.Length + StringBytes.Length - 1) As Byte
        Array.Copy(b, 0, FileBytes, 0, 8)
        Array.Copy(HashBytes, 0, FileBytes, 8, 16)
        Array.Copy(StringBytes, 0, FileBytes, 24, StringBytes.Length)
        System.IO.File.WriteAllBytes(fname, FileBytes)
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        My.Settings.Save()
    End Sub

    Private Sub MenuText_Opening(sender As Object, e As CancelEventArgs) Handles MenuText.Opening
        If sender.name = "MenuText" Then
            OpenATMenu()
        End If
    End Sub

    Private Sub MenuBook_Opening(sender As Object, e As CancelEventArgs) Handles MenuBook.Opening
        If MenuBook.Enabled = True Then
            MenuBook_Header.Text = Contents.Items(cbook) & " (#" & (cbook + 1) & ")"
            MenuBook_Paste.Enabled = (InternalClipboardMethod = "Book")
        End If
    End Sub

    Private Sub MenuRow_Opening(sender As Object, e As CancelEventArgs) Handles MenuRow.Opening
        MenuRow_Paste.Enabled = (InternalClipboardMethod = "Row")
        If MenuRow.Enabled = True Then
            MenuRow_Save.Text = "Save Row " & (sender.sourcecontrol.tag + 1)
            MenuRow_CopyLocation.Text = "Copy Location (mcr" & KillZero(xBook, sender.sourcecontrol.tag) & ".dat)"
            MenuRow_CopyLocation.Tag = "mcr" & KillZero(xBook, sender.sourcecontrol.tag) & ".dat"
        End If
    End Sub

    Private Sub MenuMacro_Opening(sender As Object, e As CancelEventArgs) Handles MenuMacro.Opening
        MenuMacro_Paste.Enabled = (InternalClipboardMethod = "Macro")
    End Sub

    Private Sub MenuBook_Import_Click(sender As Object, e As EventArgs) Handles MenuBook_Import.Click
        Dim p As Integer = MessageBox.Show("You will need to open 10 macro files ranging from (0-9, 10-19, 120-129, et). Proceed?

The files will not be saved until you save and the originals can be recovered by Reverting the Book or individual Rows.", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.No Then
            Exit Sub
        End If
        OpenDialog.InitialDirectory = importpath
        OpenDialog.Filter = "Macro Dat Files|*.dat"
        OpenDialog.FileName = ""
        OpenDialog.Multiselect = True
        If OpenDialog.ShowDialog <> DialogResult.Cancel Then
            If OpenDialog.FileNames.Length = 10 Then
                For i = 0 To OpenDialog.FileNames.Length - 1
                    ReadMacroFile(xBook, i, OpenDialog.FileNames(i), False)
                Next
                Contents.SelectedIndex = cbook
                Rows(xRow).PerformClick()
                importpath = OpenDialog.FileName.Substring(0, OpenDialog.FileName.LastIndexOf("\"))
            Else
                MsgBox("You must select 10 files.")
            End If
        Else
            Exit Sub
        End If
    End Sub

    Private Sub MenuRow_Import_Click(sender As Object, e As EventArgs) Handles MenuRow_Import.Click
        Dim p As Integer = MessageBox.Show("You will need to open another macro dat file. Do you want to proceed? 

The file will not be saved until you save and the original can be recovered by Reverting the Row.", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.No Then
            Exit Sub
        End If
        OpenDialog.InitialDirectory = importpath
        OpenDialog.Filter = "Macro Dat Files|*.dat"
        OpenDialog.FileName = ""
        OpenDialog.Multiselect = False
        If OpenDialog.ShowDialog <> DialogResult.Cancel Then
            importpath = OpenDialog.FileName.Substring(0, OpenDialog.FileName.LastIndexOf("\"))
            ReadMacroFile(xBook, xRow, OpenDialog.FileName, False)
            Rows(xRow).PerformClick()
        Else
            Exit Sub
        End If
    End Sub

    Private Sub menuText_Paste_Click(sender As Object, e As EventArgs, Optional x As Integer = 0) Handles menuText_Paste.Click
        Dim cx1 As String() = CleanClipBoard().Trim().Split(Chr(10))
        If cx1(0) = "Type: Macro" Then
            MenuMacro_PasteClipboard_Click(Ctrls(xMacro), e, xMacro)
        Else
            If cx1.Length = 1 Then
                Lines(CurrentLine).SelectedText = CleanClipBoard()
            ElseIf CurrentLine = 0 Then
                MacroContainer(xBook, xRow, xMacro) = {"", "", "", "", "", "", ""}
                For i = 0 To cx1.Length - 1
                    Lines(i).Text = cx1(i)
                    If i = 6 Then Exit For
                Next
            Else
                If cx1.Length > 1 Then
                    Dim p As Integer = MessageBox.Show("The clipboard contains multiple lines." & Chr(10) &
                                                        "Macro editor will begin pasting at the currently selected line. Ok?" & Chr(10) &
                                                        "Press Yes for to start here, No to start at at the macro title, and cancel to abort.", "Macro Editor", MessageBoxButtons.YesNoCancel)
                    If p = DialogResult.Yes Then
                        MacroContainer(xBook, xRow, xMacro) = {"", "", "", "", "", "", ""}
                        For i = 0 To cx1.Length - 1
                            Lines(CurrentLine + i).Text = cx1(i)
                            If CurrentLine + i = 6 Then Exit For
                        Next
                    ElseIf p = DialogResult.No Then
                        For i = 0 To cx1.Length - 1
                            Lines(i).Text = cx1(i)
                            If i = 6 Then Exit For
                        Next
                    End If
                End If
            End If
        End If
        SomethingEdited = True
    End Sub

    Private Sub menuText_Copy_Click(sender As Object, e As EventArgs) Handles menuText_Copy.Click
        If Lines(CurrentLine).SelectedText Then
            Clipboard.SetText(Lines(CurrentLine).SelectedText)
        End If
    End Sub

    Private Sub menuText_Cut_Click(sender As Object, e As EventArgs) Handles menuText_Cut.Click
        menuText_Copy_Click(sender, e)
        Lines(CurrentLine).SelectedText = ""
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        rs.ResizeAllControls(Me)
    End Sub

    Private Sub MenuMain_Search_Click(sender As Object, e As EventArgs) Handles MenuMain_Search.Click
        Dim cautions As New Dictionary(Of Integer, String())
        Dim ix As String = InputBox("Enter search phrase:", "Macro Editor")
        If ix.Length > 0 Then
            Dim ixl As String = ix.ToLower()
            For b = 0 To debuglimit
                For r = 0 To 9
                    For m = 0 To 19
                        If MacroContainer(b, r, m)(0).ToLower().Contains(ixl) Then
                            cautions(cautions.Count) = {b, r, m, 0, 0, MacroContainer(b, r, m)(0)}
                        End If
                        For l = 1 To 6
                            Dim line As String = MacroContainer(b, r, m)(l)
                            If line.ToLower().Contains(ixl) Then
                                cautions(cautions.Count) = {b, r, m, l, 0, MacroContainer(b, r, m)(l)}
                            End If
                        Next
                    Next
                Next
            Next

            If cautions.Count > 1 Then
                SearchResults.Hide()
                SearchResults = New Assessment
                For Each kvp As KeyValuePair(Of Integer, String()) In cautions
                    SearchResults.AddResult(kvp.Value(0), kvp.Value(1), kvp.Value(2), kvp.Value(3), kvp.Value(4), kvp.Value(5), Color.White, Color.Black)
                Next
                SearchResults.Show(Me)
            Else
                MsgBox("No matches found in any macro.")
            End If
        End If
    End Sub

    Private Sub MenuBook_Wizard_BLM_Click(sender As Object, e As EventArgs) Handles MenuBook_Wizard_BLM.Click
        Dim clp As String = "Type: Book BLM

Row, Macro:
Home
/macro set 1

Macro:
Blizzaja
/ma ""Blizzaja"" <stnpc>

Macro:
Firaja
/ma ""Firaja"" <stnpc>

Macro:
Waterja
/ma ""Waterja"" <stnpc>

Macro:
Thundaja
/ma ""Thundaja"" <stnpc>

Macro:
Stoneja
/ma ""Stoneja"" <stnpc>

Macro:
Aeroja
/ma ""Aeroja"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:


Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizard6
/ma ""Blizzard VI"" <stnpc>

Macro:
Fire6
/ma ""Fire VI"" <stnpc>

Macro:
Water6
/ma ""Water VI"" <stnpc>

Macro:
Thunder6
/ma ""Thunder VI"" <stnpc>

Macro:
Stone6
/ma ""Stone VI"" <stnpc>

Macro:
Aero6
/ma ""Aero VI"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:


Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizard5
/ma ""Blizzard V"" <stnpc>

Macro:
Fire5
/ma ""Fire V"" <stnpc>

Macro:
Water5
/ma ""Water V"" <stnpc>

Macro:
Thunder5
/ma ""Thunder V"" <stnpc>

Macro:
Stone5
/ma ""Stone V"" <stnpc>

Macro:
Aero5
/ma ""Aero V"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizard4
/ma ""Blizzard IV"" <stnpc>

Macro:
Fire4
/ma ""Fire IV"" <stnpc>

Macro:
Water4
/ma ""Water IV"" <stnpc>

Macro:
Thunder4
/ma ""Thunder IV"" <stnpc>

Macro:
Stone4
/ma ""Stone IV"" <stnpc>

Macro:
Aero4
/ma ""Aero IV"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizard3
/ma ""Blizzard III"" <stnpc>

Macro:
Fire3
/ma ""Fire III"" <stnpc>

Macro:
Water3
/ma ""Water III"" <stnpc>

Macro:
Thunder3
/ma ""Thunder III"" <stnpc>

Macro:
Stone 3
/ma ""Stone III"" <stnpc>

Macro:
Aero3
/ma ""Aero III"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Bliz2
/ma ""Blizzard II"" <stnpc>

Macro:
Fire2
/ma ""Fire II"" <stnpc>

Macro:
Water2
/ma ""Water II"" <stnpc>

Macro:
Thun2
/ma ""Thunder II"" <stnpc>

Macro:
Stone2
/ma ""Stone II"" <stnpc>

Macro:
Aero2
/ma ""Aero II"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Sleep2
/ma ""Sleep II"" <stnpc>

Macro:
Break
/ma ""Break"" <stnpc>

Macro:
Sleep
/ma ""Sleep"" <stnpc>

Macro:
Gravity
/ma ""Gravity"" <stnpc>

Macro:
Distract
/ma ""Distract"" <stnpc>

Macro:
Frazzle
/ma ""Frazzle"" <stnpc>

Macro:
Sleepga
/ma ""Sleepga"" <stnpc>

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Sleep2
/ma ""Sleep II"" <stnpc>

Macro:
Firaga
/ma ""Firaga"" <stnpc>

Macro:
Waterga
/ma ""Waterga"" <stnpc>

Macro:
Gravity
/ma ""Gravity"" <stnpc>

Macro:
Stonega
/ma ""Stonega"" <stnpc>

Macro:
Aeroga
/ma ""Aeroga"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizzag
/ma ""Blizzaga II"" <stnpc>

Macro:
Firaga2
/ma ""Firaga II"" <stnpc>

Macro:
Waterga2
/ma ""Waterga II"" <stnpc>

Macro:
Thundag2
/ma ""Thundaga II"" <stnpc>

Macro:
Stonega2
/ma ""Stonega II"" <stnpc>

Macro:
Aeroga2
/ma ""Aeroga II"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
Home
/macro set 1

Macro:
Blizzag3
/ma ""Blizzaga III"" <stnpc>

Macro:
Firaga3
/ma ""Firaga III"" <stnpc>

Macro:
Waterga3
/ma ""Waterga III"" <stnpc>

Macro:
Thundag3
/ma ""Thundaga III"" <stnpc>

Macro:
Stonega3
/ma ""Stonega III"" <stnpc>

Macro:
Aeroga3
/ma ""Aeroga III"" <stnpc>

Macro:
CCs
/macro set 7

Macro:
Breakga
/ma ""Breakga"" <stnpc>

Macro:
Sleepga2
/ma ""Sleepga II"" <stnpc>

Macro:
Cure IV
/ma ""Cure IV"" <stal>

Macro:
Cure III
/ma ""Cure III"" <stal>

Macro:


Macro:
Aquaveil
/ma ""Aquaveil"" <me>

Macro:
Blink
/ma ""Blink"" <me>

Macro:
Stoneski
/ma ""Stoneskin"" <me>

Macro:
Refresh
/ma ""Refresh"" <stpt>

Macro:
Haste
/ma ""Haste"" <stal>

Macro:
Aspirs
/ma ""Aspir III"" <stnpc>
/ma ""Aspir II"" <lastst>
/ma ""Aspir"" <lastst>

Macro:
Stun
/ma ""Stun"" <stnpc>

Row, Macro:
EndBook (Please do not remove empty lines as they're part of the sharing format). If you experience problems pasting, please make sure to download an up to date version."
        Dim lf As New Regex("(\r\n|\r|\n)")
        MenuBook_PasteClipboard_Click(sender, e, lf.Replace(clp, Chr(10)))
    End Sub

    Private Sub MenuBook_SaveFiles_Click(sender As Object, e As EventArgs) Handles MenuBook_SaveFiles.Click
        Dim p As Integer = MessageBox.Show("This will save all macros for Book " & Contents.Items(cbook) & " to file." & Chr(10) & Chr(10) &
                                                "Proceed?" & Chr(10) & Chr(10) &
                                                "This will also update the in-memory backup that can" & Chr(10) &
                                                "be restored with Revert from Main, Book, Row, & Macro menus.", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.Yes Then
            For mRow = 0 To 9
                If WriteFile(cbook, mRow) = False Then
                    UpdateStatusBar("Error!", "An error occurred while writing " & KillZero(cbook, mRow) & ".dat")
                    Exit Sub
                End If
                UpdateStatusBar(Contents.Items(cbook), "Writing " & KillZero(cbook, mRow) & ".dat")
                StatusBar.Refresh()
            Next
        End If
    End Sub

    Private Sub MenuHelp_FeatureTour_Click(sender As Object, e As EventArgs) Handles MenuHelp_FeatureTour.Click
        Me.TopMost = True
        Me.Top = (My.Computer.Screen.Bounds.Size.Height - Me.Height) / 2
        Me.Left = (My.Computer.Screen.Bounds.Size.Width - Me.Width) / 2
        UpdateStatusBar("", "Always On Top Set While Feature Tour Is running.")
        MenuBook.Enabled = False
        MenuBook.Show(New Point(Me.Left + 50, Me.Top + 100))
        MsgBox("This Is the macro book menu, operations that can be performed on any book. You can interact with books without selecting them.")
        MenuRow.Enabled = False
        MenuRow.Show(New Point(Me.Left + 160, Me.Top + 400))
        MsgBox("The row menu, you can also interact with rows without selecting them.")
        MenuHandler.Enabled = False
        MenuHandler.Show(New Point(Me.Left + 230, Me.Top + 85))
        MsgBox("Individual menus Control-side and Alternate side.")
        MenuMacro.Enabled = False
        MenuMacro.Show(New Point(Me.Left + 800, Me.Top + 100))
        MsgBox("The macro menu. If you copy to clipboard, you can share online or select a macro and immediately press Ctrl+V.")
        MenuText.Enabled = False
        MenuText.Show(New Point(Me.Left + 800, Me.Top + 285))
        MsgBox("This menu can be accessed by right-clicking a text box or pressing F2 While inside a textbox.")
        MenuText.Hide()
        MenuBook.Enabled = True
        MenuRow.Enabled = True
        MenuHandler.Enabled = True
        MenuMacro.Enabled = True
        MenuText.Enabled = True
        Me.TopMost = False
        UpdateStatusBar("", "")
    End Sub

    Private Sub MenuBook_RenameBook_Click(sender As Object, e As EventArgs) Handles MenuBook_RenameBook.Click
        RemoveHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
        Dim ix = InputBox("Enter book name.", "Macro Editor", Contents.SelectedItem)
        If Not ix = "" Then
            Contents.Items(cbook) = ix.Substring(0, Math.Min(15, ix.Length))
        End If
        AddHandler Contents.SelectedIndexChanged, AddressOf Contents_SelectedIndexChanged
    End Sub

    Private Sub MenuBook_MacroMap_Click(sender As Object, e As EventArgs) Handles MenuBook_MacroMap.Click
        If Contents.Enabled = True Then
            MacroMap.Hide()
            MacroMap = New MacroMapForm
            For r = 0 To 9
                For m = 0 To 19
                    MacroMap.Add(cbook, r, m, MacroContainer(cbook, r, m))
                Next
            Next
            MacroMap.Show()
        End If
    End Sub

    Private Sub MenuBook_CopyMacroMap_Click(sender As Object, e As EventArgs) Handles MenuBook_CopyMacroMap.Click
        If Contents.Enabled = True Then
            Dim sb As New StringBuilder
            sb.Append("[table style='width:1500px;']")
                Dim macrobody(5) As String
            For r = 0 To 9
                sb.Append("[tr][td]Row " & (r + 1) & "[/td][/tr][tr]")
                For m = 0 To 19
                    sb.Append("[td][b]" & MacroContainer(cbook, r, m)(0) & "[/b]" & Chr(10))
                    Array.Copy(MacroContainer(cbook, r, m), 1, macrobody, 0, 1)
                    sb.Append(Strings.Join(macrobody, Chr(10)) & "[/td]" & Chr(10))
                    If m = 9 Then sb.Append("[/tr][tr]")
                Next
                sb.Append("[/tr]")
            Next
            sb.Append("[/table]")
            Clipboard.SetText(sb.ToString())
        End If
    End Sub

    Private Sub MenuRow_Save_Click(sender As Object, e As EventArgs) Handles MenuRow_Save.Click
        Dim p As Integer = MessageBox.Show("This will save this macro to file." & Chr(10) & Chr(10) &
                                        "Proceed?" & Chr(10) & Chr(10) &
                                        "This will also update the in-memory backup that can" & Chr(10) &
                                        "be restored with Revert from Main, Book, Row, & Macro menus.", "Macro Editor", MessageBoxButtons.YesNo)
        If p = DialogResult.Yes Then
            If WriteFile(xBook, xRow) = False Then
                UpdateStatusBar("Error", "An error occurred while writing files.")
                Exit Sub
            End If
        End If

    End Sub

    Private Sub MenuRow_CopyLocation_Click(sender As Object, e As EventArgs) Handles MenuRow_CopyLocation.Click
        Clipboard.SetText(macropath & "\" & sender.tag)
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing And SomethingEdited = True Then
            Dim xOut = MessageBox.Show("You have made unsaved changes. Do you really want to exit and lose all changes?", "Macro Editor", MessageBoxButtons.YesNo)
            If xOut = DialogResult.Yes Then
                ' Do nothing, close.
            Else
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub MenuMacro_Destination_Click(sender As Object, e As EventArgs) Handles MenuMacro_Destination.Click
        Dim PointToDestination As Destination = Destination
        If xBook >= 0 And xRow >= 0 Then
            PointToDestination.xBook = xBook
            PointToDestination.xRow = xRow
            PointToDestination.tMacro = (sender.GetCurrentParent()).SourceControl.tag
            PointToDestination.ShowDialog()
        End If
    End Sub

    Private Sub File_Exit_Click(sender As Object, e As EventArgs) Handles File_Exit.Click
        Me.Close()
    End Sub

    Private Sub MainForm_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        If e.Delta > 0 Then
            Rows(Math.Max(xRow - 1, 0)).PerformClick()
        Else
            Rows(Math.Min(xRow + 1, 9)).PerformClick()
        End If
    End Sub

    Private Sub MenuBook_Closing(sender As Object, e As ToolStripDropDownClosingEventArgs) Handles MenuBook.Closing
        If e.CloseReason = ToolStripDropDownCloseReason.ItemClicked Then
            Dim ItemClicked As String = MenuBook.GetItemAt(New Point(Cursor.Position.X - MenuBook.Left, Cursor.Position.Y - MenuBook.Top)).Name
            If ItemClicked = "MenuBook_Header" Then
                e.Cancel = True
            End If
        End If
    End Sub

    'names are 8 characters
    'lines are 60 characters
    'macro titles are 15
End Class
