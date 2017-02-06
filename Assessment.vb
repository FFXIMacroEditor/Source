Public Class Assessment
    Dim output As New Panel
    Dim output_row As New Dictionary(Of Integer, GroupBox)
    Dim iTypes As String() = {"Match Found", "Title Length", "Line Length", "Invalid characters", "Starts with '//'", "Doesn't start with '/'", "Old Style /wait", "Item Auto-Translate"}
    Dim itypes_explanation As String() = {"", "Macro titles can only be 8 characters.", "Macro line length, after auto-translate conversion, can only be 60 characters.", "Line contains invalid, likely invisible, characters try retyping the line. This is usually the result of a corrupt clipboard paste.", "If you're trying to execute a windower command, try /console [command] rather than //[command].", "Line doesn't start with '/'",
        "It may be useful to free up a line and append <waitX> to the previous line, rather than /wait X.", "Autotranslate phrases for items are not deciphered, in part due to the sheer number of items. FFXI Macro Editor interacts with these macro lines fine."}
    Private Sub Assessment_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AutoScroll = True
    End Sub

    Public Function AddResult(b, r, m, l, itype, description, bgcolor, fcolor)
        If l = 0 Then
            Results.Items.Add(New ListViewItem({String.Format("B: {0} ({1}), R: {2}, M: {3}, L: {4}", MainForm.Contents.Items(b), b + 1, r + 1, m + 1, "Title"), iTypes(itype), description, itype}))
        Else
            Results.Items.Add(New ListViewItem({String.Format("B: {0} ({1}), R: {2}, M: {3}, L: {4}", MainForm.Contents.Items(b), b + 1, r + 1, m + 1, l + 1), iTypes(itype), description, itype}))
        End If
        Results.Items(Results.Items.Count - 1).Tag = {b, r, m, l}

        Results.Items(Results.Items.Count - 1).BackColor = bgcolor
        Results.Items(Results.Items.Count - 1).ForeColor = fcolor
        Return True
    End Function

    Private Sub Results_DoubleClick(sender As Object, e As EventArgs) Handles Results.DoubleClick
        Dim dest = Results.Items(Results.SelectedIndices(0)).Tag
        MainForm.FindMacro(dest(0), dest(1), dest(2))
    End Sub

    Private Sub Results_MouseDown(sender As Object, e As MouseEventArgs) Handles Results.MouseDown
        If e.Button = MouseButtons.Right Then
            Dim thisRow As Integer = Results.GetItemAt(e.X, e.Y).Index
            If thisRow >= 0 And Results.Items(thisRow).SubItems(3).Text > 0 Then
                MsgBox(Results.Items(thisRow).SubItems(1).Text & Chr(10) & Chr(10) & itypes_explanation(Results.Items(thisRow).SubItems(3).Text))
            End If
        End If
    End Sub

    Private Sub Results_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles Results.ColumnClick
        Exit Sub ' doesn't work properly yet
        If Results.Columns.Item(e.Column).ListView.Sorting <> SortOrder.Descending Then
            Results.Columns.Item(e.Column).ListView.Sorting = SortOrder.Descending
        ElseIf Results.Columns.Item(e.Column).ListView.Sorting <> SortOrder.Ascending Then
            Results.Columns.Item(e.Column).ListView.Sorting = SortOrder.Ascending
        End If
        Results.Sort()
    End Sub
End Class