Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports FFXI = Yekyaa.FFXIEncoding
Imports Microsoft.Win32
Imports System.IO

Partial Class MainForm
    Public Function KillZero(book As Integer, row As Integer) As String
        If book = 0 Then
            If row = 0 Then
                Return ""
            Else
                Return row.ToString()
            End If
        Else
            Return book & row
        End If
    End Function

    Public Function TenToZero(ten As Integer) As String
        If Not ten = 10 Then
            Return ten.ToString()
        Else
            Return "0"
        End If
    End Function

    Public Function FindMacro(b As Integer, r As Integer, m As Integer) As Boolean
        Contents.SelectedIndex = b
        xRow = r
        xMacro = m
        Rows(r).PerformClick()
        Return True
    End Function

    Public Function AsMacro(m As Integer) As String
        If m > 9 Then
            Return "Alt " & (m - 9)
        Else
            Return "Ctl " & (m + 1)
        End If
    End Function

    Private Function FindFFXIDirectory() As Boolean
        FolderBrowserDialog1.SelectedPath = macropath
        Dim result As DialogResult = FolderBrowserDialog1.ShowDialog()

        If (result = DialogResult.OK) Then
            My.Settings.UserDirectory = FolderBrowserDialog1.SelectedPath
        End If
        My.Settings.Save()
        Return True
    End Function

    Public Function ParseAT() As Boolean
        Dim lastEle As ToolStripMenuItem
        Dim thisEle As ToolStripMenuItem
        Dim y As FFXI.FFXIATPhraseLoader = New FFXI.FFXIATPhraseLoader
        Dim r = y.ATPhrases
        lastEle = ATmenu.DropDownItems.Add("test")
        For i = 0 To r.Count - 1
            If r(i).value.Trim().Length = 0 Then
                Continue For
            ElseIf r(i).value.Substring(0, 1) = "【" Then
                lastEle = ATmenu.DropDownItems.Add(r(i).value)
            Else
                thisEle = lastEle.DropDownItems.Add(r(i).value)
                thisEle.Tag = r(i).ToString()
                ATObject.Add(r(i).ToString().Substring(1, 8), r(i).value)
                AddHandler thisEle.Click, AddressOf ATPhrase_Click
            End If
        Next
        ATmenu.Text = "Auto-Translate"
        MenuText.Items.Add(ATmenu)
        Return True
    End Function

    Public Function ATWriter(macroline As String) As String
        Return ATWritable.Replace(lfs.Replace(macroline, ""), Function(found As Match)
                                                                  Return (ATEncode(found.Groups(1).ToString()))
                                                              End Function)
    End Function


    Public Function WriteFile(Book As Integer, Row As Integer) As Boolean
        Dim fname As String = macropath & "\mcr" & KillZero(Book, Row) & ".dat"
        Dim b() As Byte
        If File.Exists(fname) Then
            Try
                b = IO.File.ReadAllBytes(fname)
            Catch ex As Exception
                MsgBox("Cannot read " & fname & ".")
                Return False
            End Try
        Else
            ' Macro rows that have never been visited in game
            ' seem to not have a macro file.
            b = New Byte() {&H1, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        End If

        Dim sb As New StringBuilder

        For m = 0 To debuglimit
            MacroPreserved(Book, Row, m) = MacroContainer(Book, Row, m).Clone()
            sb.Append(fill("", 4))
            For l = 1 To 6
                sb.Append(fill(ATWriter(MacroContainer(Book, Row, m)(l)), 61))
            Next
            sb.Append(fill(MacroContainer(Book, Row, m)(0).Substring(0, Math.Min(MacroContainer(Book, Row, m)(0).Length, 8)), 9))
            sb.Append(Chr(0))
        Next

        If sb.Length <> 7600 Then
            MsgBox("Compilation of " & KillZero(Book, Row) & "failed.")
            Return False
        End If

        Dim StringBytes() As Byte = Encoding.Default.GetBytes(sb.ToString())

        Dim HashMaker As New MD5CryptoServiceProvider()
        Dim HashBytes As Byte() = HashMaker.ComputeHash(StringBytes)
        Dim Hash As New StringBuilder()
        For hx = 0 To HashBytes.Length - 1
            Hash.Append(Chr(HashBytes(hx)))
        Next


        Dim FileBytes(8 + HashBytes.Length + StringBytes.Length - 1) As Byte
        Array.Copy(b, 0, FileBytes, 0, 8)
        Array.Copy(HashBytes, 0, FileBytes, 8, 16)
        Array.Copy(StringBytes, 0, FileBytes, 24, StringBytes.Length)
        Try
            System.IO.File.WriteAllBytes(fname, FileBytes)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Function ATDecode(phrase As String) As String ' For Reading
        Dim DecodedPhrase As String = ""
        For Each c As Char In phrase
            DecodedPhrase &= Convert.ToString(Convert.ToInt32(c), 16).PadLeft(2, "0")
        Next
        Return "<" & DecodedPhrase & "|" & ATObject.Item(DecodedPhrase.ToUpper()) & ">"
    End Function

    Function ATEncode(phrase As String) As String ' For Writing to File
        phrase = "FD" & phrase & "FD" 'FD = "ý", 236
        Dim EncodedPhrase As String = ""
        For c = 0 To phrase.Length - 2 Step 2
            EncodedPhrase &= Convert.ToChar(Convert.ToUInt32(phrase.Substring(c, 2), 16)).ToString()
        Next
        Return EncodedPhrase
    End Function

    Function CleanClipBoard() As String
        If Clipboard.ContainsText(TextDataFormat.Text) Then
            Dim lf As New Regex("(\r\n|\r|\n)")
            Return lf.Replace(Clipboard.GetText(), Chr(10))
        Else
            MsgBox("Clipboard does not contain regular text.")
            Return ""
        End If
    End Function

    Function verifyclipboard(pasteType As String, a As Array) As Boolean
        If a(0).ToString().Trim().StartsWith("Type: " & pasteType) And a(a.Length - 1).ToString.Trim().StartsWith("End" & pasteType) Then
            Return True
        Else
            MsgBox("Clipboard does not contain a " & pasteType & ".")
            Return False
        End If
    End Function

    Function fill(str As String, rpad As Integer, Optional padder As String = Chr(0)) As String
        Return str & New String(Chr(0), rpad - str.Length)
    End Function

    Function ReadMacroFile(mbook As Integer, mset As Integer, filename As String, PreserveMacros As Boolean) As Boolean
        Dim b() As Byte = IO.File.ReadAllBytes(filename)
        Dim EachRead As String = ""
        For i = 24 To b.Length - 1
            EachRead += Convert.ToChar(b(i))
        Next

        EachRead = EachRead.Replace(Chr(0), Chr(10))
        Dim mMacro As Integer = 0
        Dim ret As Boolean = True
        For nPos = 0 To EachRead.Length - 1 Step 380
            Dim mLine As Integer = 1
            MacroContainer(mbook, mset, mMacro) = {lfs.Replace(EachRead.Substring(nPos + 370, 8).Trim(), ""), "", "", "", "", "", ""}
            If PreserveMacros = True Then
                MacroPreserved(mbook, mset, mMacro) = MacroContainer(mbook, mset, mMacro).Clone()
            End If

            For iPos = 0 To 360 Step 61
                MacroContainer(mbook, mset, mMacro)(mLine) = ATReadable.Replace(lfs.Replace(EachRead.Substring(nPos + iPos, 60).Trim(), ""),
                         Function(found As Match)
                             Return ATDecode(found.Groups(1).ToString())
                         End Function)
                MacroPreserved(mbook, mset, mMacro)(mLine) = MacroContainer(mbook, mset, mMacro)(mLine).Clone()
                mLine += 1
            Next
            mMacro = mMacro + 1
        Next
        Return ret
    End Function

    Function OpenATMenu() As Boolean
        If MenuMacro.Enabled = False Then
            Return False
        End If
        Dim keyc As TextBox = Lines(CurrentLine)
        Dim keyw As String = ""
        If keyc.SelectionLength = 0 Then
            Dim StartPos As Integer = keyc.SelectionStart
            Dim lastspace As Integer = keyc.Text.Substring(0, keyc.SelectionStart).LastIndexOf(" ") + 1
            keyc.Select(lastspace, StartPos - lastspace)
        End If
        keyw = keyc.SelectedText
        For m = 0 To ATmenu.DropDownItems.Count - 1
            Dim parentele As ToolStripMenuItem = ATmenu.DropDownItems.Item(m)
            parentele.Visible = False
            For x = 0 To parentele.DropDownItems.Count - 1
                Dim thisele As ToolStripMenuItem = parentele.DropDownItems.Item(x)
                If (thisele.Text.Substring(0, Math.Min(keyw.Length, thisele.Text.Length)).ToLower() = keyw.ToLower()) = True Then
                    parentele.Visible = True
                    thisele.Visible = True
                Else
                    thisele.Visible = False
                End If
            Next
        Next
        Return True
    End Function

    Public Function GetFFXIDirectory()

        Dim s As String = [String].Empty
        ' Attempt to open the key
        Dim key As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\PlayOnlineUS\InstallFolder")
        If key Is Nothing Then
            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\PlayOnlineEU\InstallFolder")
        End If
        If key Is Nothing Then
            key = Registry.LocalMachine.OpenSubKey("SOFTWARE\PlayOnline\InstallFolder")
        End If
        If key Is Nothing Then
            key = Registry.LocalMachine.OpenSubKey("Software\Wow6432Node\PlayOnlineUS\InstallFolder")
        End If
        If key Is Nothing Then
            key = Registry.LocalMachine.OpenSubKey("Software\Wow6432Node\PlayOnlineEU\InstallFolder")
        End If
        If key Is Nothing Then
            key = Registry.LocalMachine.OpenSubKey("Software\Wow6432Node\PlayOnline\InstallFolder")
        End If

        ' Attempt to retrieve the value "0001"; if null is returned, the value
        ' doesn't exist in the registry.
        If (key IsNot Nothing) AndAlso (key.GetValue("0001") IsNot Nothing) Then
            s = DirectCast(key.GetValue("0001"), String)
            Return ([String].Format("{0}\", s.TrimEnd("\"c)))
        End If
        Return String.Empty
    End Function

    Function UpdateStatusBar(h As String, Optional d As String = "")
        If Not h = "0" Then ' I might want to retain what's in StatusH.
            StatusH.Text = h
        End If
        StatusD.Text = d
        Return True
    End Function
End Class
