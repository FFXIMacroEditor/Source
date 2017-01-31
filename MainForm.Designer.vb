<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.MainMenu = New System.Windows.Forms.MenuStrip()
        Me.FileMenu = New System.Windows.Forms.ToolStripMenuItem()
        Me.File_Open = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripSeparator()
        Me.File_SaveRow = New System.Windows.Forms.ToolStripMenuItem()
        Me.File_SaveAll = New System.Windows.Forms.ToolStripMenuItem()
        Me.File_Exit = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMain_evaluate = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMain_Search = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHelp_Help = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHelp_FeatureTour = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuMacro_Cut = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro_Copy = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro_Paste = New System.Windows.Forms.ToolStripMenuItem()
        Me.h1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuMacro_Clear = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro_Revert = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro_CopyClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuMacro_PasteClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuMacro_Destination = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuRow_Cut = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_Copy = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_Paste = New System.Windows.Forms.ToolStripMenuItem()
        Me.h2 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuRow_Clear = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_Revert = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_CopyClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_PasteClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuRow_Save = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_Import = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuRow_CopyLocation = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuBook_Header = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Cut = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Copy = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Paste = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_h1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuBook_Clear = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Revert = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_RenameBook = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_SaveBookNames = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_h2 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuBook_CopyClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_PasteClipboard = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.MenuBook_SaveFiles = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Import = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Wizard = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_Wizard_BLM = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_MacroMap = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuBook_CopyMacroMap = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHandler = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.MenuHandler_ClearSide = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHandler_CutSide = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHandler_CopySide = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuHandler_PasteSide = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenDialog = New System.Windows.Forms.OpenFileDialog()
        Me.StatusBar = New System.Windows.Forms.StatusStrip()
        Me.StatusH = New System.Windows.Forms.ToolStripStatusLabel()
        Me.StatusD = New System.Windows.Forms.ToolStripStatusLabel()
        Me.MenuText = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.menuText_Cut = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuText_Copy = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuText_Paste = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem7 = New System.Windows.Forms.ToolStripSeparator()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.ControlTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.Warning = New System.Windows.Forms.TextBox()
        Me.MainMenu.SuspendLayout()
        Me.MenuMacro.SuspendLayout()
        Me.MenuRow.SuspendLayout()
        Me.MenuBook.SuspendLayout()
        Me.MenuHandler.SuspendLayout()
        Me.StatusBar.SuspendLayout()
        Me.MenuText.SuspendLayout()
        Me.SuspendLayout()
        '
        'MainMenu
        '
        Me.MainMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileMenu, Me.MenuMain_evaluate, Me.MenuMain_Search, Me.MenuHelp})
        Me.MainMenu.Location = New System.Drawing.Point(0, 0)
        Me.MainMenu.Name = "MainMenu"
        Me.MainMenu.Size = New System.Drawing.Size(284, 24)
        Me.MainMenu.TabIndex = 0
        Me.MainMenu.Text = "MenuMain"
        '
        'FileMenu
        '
        Me.FileMenu.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.File_Open, Me.ToolStripMenuItem2, Me.File_SaveRow, Me.File_SaveAll, Me.File_Exit})
        Me.FileMenu.Name = "FileMenu"
        Me.FileMenu.Size = New System.Drawing.Size(37, 20)
        Me.FileMenu.Text = "File"
        '
        'File_Open
        '
        Me.File_Open.Name = "File_Open"
        Me.File_Open.Size = New System.Drawing.Size(161, 22)
        Me.File_Open.Text = "Open"
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        Me.ToolStripMenuItem2.Size = New System.Drawing.Size(158, 6)
        '
        'File_SaveRow
        '
        Me.File_SaveRow.Name = "File_SaveRow"
        Me.File_SaveRow.Size = New System.Drawing.Size(161, 22)
        Me.File_SaveRow.Text = "Save Macro Row"
        '
        'File_SaveAll
        '
        Me.File_SaveAll.Name = "File_SaveAll"
        Me.File_SaveAll.Size = New System.Drawing.Size(161, 22)
        Me.File_SaveAll.Text = "Save All"
        '
        'File_Exit
        '
        Me.File_Exit.Name = "File_Exit"
        Me.File_Exit.Size = New System.Drawing.Size(161, 22)
        Me.File_Exit.Text = "Exit"
        '
        'MenuMain_evaluate
        '
        Me.MenuMain_evaluate.Enabled = False
        Me.MenuMain_evaluate.Name = "MenuMain_evaluate"
        Me.MenuMain_evaluate.Size = New System.Drawing.Size(63, 20)
        Me.MenuMain_evaluate.Text = "Evaluate"
        '
        'MenuMain_Search
        '
        Me.MenuMain_Search.Enabled = False
        Me.MenuMain_Search.Name = "MenuMain_Search"
        Me.MenuMain_Search.Size = New System.Drawing.Size(42, 20)
        Me.MenuMain_Search.Text = "Find"
        '
        'MenuHelp
        '
        Me.MenuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuHelp_Help, Me.MenuHelp_FeatureTour})
        Me.MenuHelp.Name = "MenuHelp"
        Me.MenuHelp.Size = New System.Drawing.Size(44, 20)
        Me.MenuHelp.Text = "Help"
        '
        'MenuHelp_Help
        '
        Me.MenuHelp_Help.Name = "MenuHelp_Help"
        Me.MenuHelp_Help.Size = New System.Drawing.Size(152, 22)
        Me.MenuHelp_Help.Text = "Help"
        '
        'MenuHelp_FeatureTour
        '
        Me.MenuHelp_FeatureTour.Name = "MenuHelp_FeatureTour"
        Me.MenuHelp_FeatureTour.Size = New System.Drawing.Size(152, 22)
        Me.MenuHelp_FeatureTour.Text = "Feature Tour"
        '
        'MenuMacro
        '
        Me.MenuMacro.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuMacro_Cut, Me.MenuMacro_Copy, Me.MenuMacro_Paste, Me.h1, Me.MenuMacro_Clear, Me.MenuMacro_Revert, Me.MenuMacro_CopyClipboard, Me.MenuMacro_PasteClipboard, Me.ToolStripMenuItem4, Me.MenuMacro_Destination})
        Me.MenuMacro.Name = "MenuMacro"
        Me.MenuMacro.Size = New System.Drawing.Size(220, 192)
        '
        'MenuMacro_Cut
        '
        Me.MenuMacro_Cut.Name = "MenuMacro_Cut"
        Me.MenuMacro_Cut.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Cut.Text = "Cut"
        '
        'MenuMacro_Copy
        '
        Me.MenuMacro_Copy.Name = "MenuMacro_Copy"
        Me.MenuMacro_Copy.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Copy.Text = "Copy"
        '
        'MenuMacro_Paste
        '
        Me.MenuMacro_Paste.Name = "MenuMacro_Paste"
        Me.MenuMacro_Paste.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Paste.Text = "Paste"
        '
        'h1
        '
        Me.h1.Name = "h1"
        Me.h1.Size = New System.Drawing.Size(216, 6)
        '
        'MenuMacro_Clear
        '
        Me.MenuMacro_Clear.Name = "MenuMacro_Clear"
        Me.MenuMacro_Clear.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Clear.Text = "Clear Macro"
        '
        'MenuMacro_Revert
        '
        Me.MenuMacro_Revert.Name = "MenuMacro_Revert"
        Me.MenuMacro_Revert.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Revert.Text = "Revert"
        '
        'MenuMacro_CopyClipboard
        '
        Me.MenuMacro_CopyClipboard.Name = "MenuMacro_CopyClipboard"
        Me.MenuMacro_CopyClipboard.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_CopyClipboard.Text = "Copy to Clipboard"
        '
        'MenuMacro_PasteClipboard
        '
        Me.MenuMacro_PasteClipboard.Name = "MenuMacro_PasteClipboard"
        Me.MenuMacro_PasteClipboard.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_PasteClipboard.Text = "Paste from Clipboard"
        '
        'ToolStripMenuItem4
        '
        Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
        Me.ToolStripMenuItem4.Size = New System.Drawing.Size(216, 6)
        '
        'MenuMacro_Destination
        '
        Me.MenuMacro_Destination.Name = "MenuMacro_Destination"
        Me.MenuMacro_Destination.Size = New System.Drawing.Size(219, 22)
        Me.MenuMacro_Destination.Text = "Point to another macro line"
        '
        'MenuRow
        '
        Me.MenuRow.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuRow_Cut, Me.MenuRow_Copy, Me.MenuRow_Paste, Me.h2, Me.MenuRow_Clear, Me.MenuRow_Revert, Me.MenuRow_CopyClipboard, Me.MenuRow_PasteClipboard, Me.ToolStripMenuItem3, Me.MenuRow_Save, Me.MenuRow_Import, Me.MenuRow_CopyLocation})
        Me.MenuRow.Name = "ContextMenuStrip1"
        Me.MenuRow.Size = New System.Drawing.Size(187, 236)
        '
        'MenuRow_Cut
        '
        Me.MenuRow_Cut.Name = "MenuRow_Cut"
        Me.MenuRow_Cut.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Cut.Text = "Cut"
        '
        'MenuRow_Copy
        '
        Me.MenuRow_Copy.Name = "MenuRow_Copy"
        Me.MenuRow_Copy.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Copy.Text = "Copy"
        '
        'MenuRow_Paste
        '
        Me.MenuRow_Paste.Name = "MenuRow_Paste"
        Me.MenuRow_Paste.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Paste.Text = "Paste"
        '
        'h2
        '
        Me.h2.Name = "h2"
        Me.h2.Size = New System.Drawing.Size(183, 6)
        '
        'MenuRow_Clear
        '
        Me.MenuRow_Clear.Name = "MenuRow_Clear"
        Me.MenuRow_Clear.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Clear.Text = "Clear Row"
        '
        'MenuRow_Revert
        '
        Me.MenuRow_Revert.Name = "MenuRow_Revert"
        Me.MenuRow_Revert.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Revert.Text = "Revert"
        '
        'MenuRow_CopyClipboard
        '
        Me.MenuRow_CopyClipboard.Name = "MenuRow_CopyClipboard"
        Me.MenuRow_CopyClipboard.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_CopyClipboard.Text = "Copy to Clipboard"
        '
        'MenuRow_PasteClipboard
        '
        Me.MenuRow_PasteClipboard.Name = "MenuRow_PasteClipboard"
        Me.MenuRow_PasteClipboard.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_PasteClipboard.Text = "Paste from Clipboard"
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        Me.ToolStripMenuItem3.Size = New System.Drawing.Size(183, 6)
        '
        'MenuRow_Save
        '
        Me.MenuRow_Save.Name = "MenuRow_Save"
        Me.MenuRow_Save.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Save.Text = "Save"
        '
        'MenuRow_Import
        '
        Me.MenuRow_Import.Name = "MenuRow_Import"
        Me.MenuRow_Import.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_Import.Text = "Import..."
        '
        'MenuRow_CopyLocation
        '
        Me.MenuRow_CopyLocation.Name = "MenuRow_CopyLocation"
        Me.MenuRow_CopyLocation.Size = New System.Drawing.Size(186, 22)
        Me.MenuRow_CopyLocation.Text = "Copy File Location"
        '
        'MenuBook
        '
        Me.MenuBook.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuBook_Header, Me.MenuBook_Cut, Me.MenuBook_Copy, Me.MenuBook_Paste, Me.MenuBook_h1, Me.MenuBook_Clear, Me.MenuBook_Revert, Me.MenuBook_RenameBook, Me.MenuBook_SaveBookNames, Me.MenuBook_h2, Me.MenuBook_CopyClipboard, Me.MenuBook_PasteClipboard, Me.ToolStripMenuItem1, Me.MenuBook_SaveFiles, Me.MenuBook_Import, Me.MenuBook_Wizard, Me.MenuBook_MacroMap, Me.MenuBook_CopyMacroMap})
        Me.MenuBook.Name = "MenuBook"
        Me.MenuBook.Size = New System.Drawing.Size(248, 472)
        '
        'MenuBook_Header
        '
        Me.MenuBook_Header.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.MenuBook_Header.Name = "MenuBook_Header"
        Me.MenuBook_Header.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Header.Text = "Header"
        '
        'MenuBook_Cut
        '
        Me.MenuBook_Cut.Name = "MenuBook_Cut"
        Me.MenuBook_Cut.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Cut.Text = "Cut"
        '
        'MenuBook_Copy
        '
        Me.MenuBook_Copy.Name = "MenuBook_Copy"
        Me.MenuBook_Copy.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Copy.Text = "Copy"
        '
        'MenuBook_Paste
        '
        Me.MenuBook_Paste.Name = "MenuBook_Paste"
        Me.MenuBook_Paste.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Paste.Text = "Paste"
        '
        'MenuBook_h1
        '
        Me.MenuBook_h1.Name = "MenuBook_h1"
        Me.MenuBook_h1.Size = New System.Drawing.Size(244, 6)
        '
        'MenuBook_Clear
        '
        Me.MenuBook_Clear.Name = "MenuBook_Clear"
        Me.MenuBook_Clear.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Clear.Text = "Clear"
        '
        'MenuBook_Revert
        '
        Me.MenuBook_Revert.Name = "MenuBook_Revert"
        Me.MenuBook_Revert.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Revert.Text = "Revert"
        '
        'MenuBook_RenameBook
        '
        Me.MenuBook_RenameBook.Name = "MenuBook_RenameBook"
        Me.MenuBook_RenameBook.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_RenameBook.Text = "Rename Book"
        '
        'MenuBook_SaveBookNames
        '
        Me.MenuBook_SaveBookNames.Name = "MenuBook_SaveBookNames"
        Me.MenuBook_SaveBookNames.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_SaveBookNames.Text = "Save Booknames"
        '
        'MenuBook_h2
        '
        Me.MenuBook_h2.Name = "MenuBook_h2"
        Me.MenuBook_h2.Size = New System.Drawing.Size(244, 6)
        '
        'MenuBook_CopyClipboard
        '
        Me.MenuBook_CopyClipboard.Name = "MenuBook_CopyClipboard"
        Me.MenuBook_CopyClipboard.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_CopyClipboard.Text = "Copy to Clipboard"
        '
        'MenuBook_PasteClipboard
        '
        Me.MenuBook_PasteClipboard.Name = "MenuBook_PasteClipboard"
        Me.MenuBook_PasteClipboard.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_PasteClipboard.Text = "Paste from Clipboard"
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(244, 6)
        '
        'MenuBook_SaveFiles
        '
        Me.MenuBook_SaveFiles.Name = "MenuBook_SaveFiles"
        Me.MenuBook_SaveFiles.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_SaveFiles.Text = "Save"
        '
        'MenuBook_Import
        '
        Me.MenuBook_Import.Name = "MenuBook_Import"
        Me.MenuBook_Import.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Import.Text = "Import..."
        '
        'MenuBook_Wizard
        '
        Me.MenuBook_Wizard.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuBook_Wizard_BLM})
        Me.MenuBook_Wizard.Name = "MenuBook_Wizard"
        Me.MenuBook_Wizard.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_Wizard.Text = "Wizard"
        '
        'MenuBook_Wizard_BLM
        '
        Me.MenuBook_Wizard_BLM.Name = "MenuBook_Wizard_BLM"
        Me.MenuBook_Wizard_BLM.Size = New System.Drawing.Size(135, 22)
        Me.MenuBook_Wizard_BLM.Text = "Black Mage"
        '
        'MenuBook_MacroMap
        '
        Me.MenuBook_MacroMap.Name = "MenuBook_MacroMap"
        Me.MenuBook_MacroMap.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_MacroMap.Text = "Macro Map"
        '
        'MenuBook_CopyMacroMap
        '
        Me.MenuBook_CopyMacroMap.Name = "MenuBook_CopyMacroMap"
        Me.MenuBook_CopyMacroMap.Size = New System.Drawing.Size(247, 30)
        Me.MenuBook_CopyMacroMap.Text = "Copy Macro Map (FFXIAH Table)"
        '
        'MenuHandler
        '
        Me.MenuHandler.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuHandler_ClearSide, Me.MenuHandler_CutSide, Me.MenuHandler_CopySide, Me.MenuHandler_PasteSide})
        Me.MenuHandler.Name = "MenuHandler"
        Me.MenuHandler.Size = New System.Drawing.Size(187, 92)
        '
        'MenuHandler_ClearSide
        '
        Me.MenuHandler_ClearSide.Name = "MenuHandler_ClearSide"
        Me.MenuHandler_ClearSide.Size = New System.Drawing.Size(186, 22)
        Me.MenuHandler_ClearSide.Text = "Clear"
        '
        'MenuHandler_CutSide
        '
        Me.MenuHandler_CutSide.Name = "MenuHandler_CutSide"
        Me.MenuHandler_CutSide.Size = New System.Drawing.Size(186, 22)
        Me.MenuHandler_CutSide.Text = "Cut to Clipboard"
        '
        'MenuHandler_CopySide
        '
        Me.MenuHandler_CopySide.Name = "MenuHandler_CopySide"
        Me.MenuHandler_CopySide.Size = New System.Drawing.Size(186, 22)
        Me.MenuHandler_CopySide.Text = "Copy to Clipboard"
        '
        'MenuHandler_PasteSide
        '
        Me.MenuHandler_PasteSide.Name = "MenuHandler_PasteSide"
        Me.MenuHandler_PasteSide.Size = New System.Drawing.Size(186, 22)
        Me.MenuHandler_PasteSide.Text = "Paste from Clipboard"
        '
        'OpenDialog
        '
        Me.OpenDialog.FileName = "mcr.ttl"
        Me.OpenDialog.Filter = "Macro Title Files|mcr.ttl"
        Me.OpenDialog.InitialDirectory = "C:\Program Files (x86)\PlayOnline\SquareEnix\FINAL FANTASY XI\USER"
        Me.OpenDialog.Title = "Find Macro Files"
        '
        'StatusBar
        '
        Me.StatusBar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusH, Me.StatusD})
        Me.StatusBar.Location = New System.Drawing.Point(0, 239)
        Me.StatusBar.Name = "StatusBar"
        Me.StatusBar.Size = New System.Drawing.Size(284, 22)
        Me.StatusBar.TabIndex = 4
        '
        'StatusH
        '
        Me.StatusH.Name = "StatusH"
        Me.StatusH.Size = New System.Drawing.Size(0, 17)
        '
        'StatusD
        '
        Me.StatusD.Name = "StatusD"
        Me.StatusD.Size = New System.Drawing.Size(0, 17)
        '
        'MenuText
        '
        Me.MenuText.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.menuText_Cut, Me.menuText_Copy, Me.menuText_Paste, Me.ToolStripMenuItem7})
        Me.MenuText.Name = "MenuText"
        Me.MenuText.Size = New System.Drawing.Size(103, 76)
        '
        'menuText_Cut
        '
        Me.menuText_Cut.Name = "menuText_Cut"
        Me.menuText_Cut.Size = New System.Drawing.Size(102, 22)
        Me.menuText_Cut.Text = "Cut"
        '
        'menuText_Copy
        '
        Me.menuText_Copy.Name = "menuText_Copy"
        Me.menuText_Copy.Size = New System.Drawing.Size(102, 22)
        Me.menuText_Copy.Text = "Copy"
        '
        'menuText_Paste
        '
        Me.menuText_Paste.Name = "menuText_Paste"
        Me.menuText_Paste.Size = New System.Drawing.Size(102, 22)
        Me.menuText_Paste.Text = "Paste"
        '
        'ToolStripMenuItem7
        '
        Me.ToolStripMenuItem7.Name = "ToolStripMenuItem7"
        Me.ToolStripMenuItem7.Size = New System.Drawing.Size(99, 6)
        '
        'Warning
        '
        Me.Warning.Font = New System.Drawing.Font("Arial", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Warning.Location = New System.Drawing.Point(80, 97)
        Me.Warning.Multiline = True
        Me.Warning.Name = "Warning"
        Me.Warning.Size = New System.Drawing.Size(100, 20)
        Me.Warning.TabIndex = 6
        Me.Warning.Text = "Use of this program may constitute a violation of the PlayOnline / FFXI User Agre" &
    "ement"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 261)
        Me.Controls.Add(Me.Warning)
        Me.Controls.Add(Me.StatusBar)
        Me.Controls.Add(Me.MainMenu)
        Me.KeyPreview = True
        Me.MainMenuStrip = Me.MainMenu
        Me.Name = "MainForm"
        Me.Text = "FFXI Macro Editor"
        Me.MainMenu.ResumeLayout(False)
        Me.MainMenu.PerformLayout()
        Me.MenuMacro.ResumeLayout(False)
        Me.MenuRow.ResumeLayout(False)
        Me.MenuBook.ResumeLayout(False)
        Me.MenuHandler.ResumeLayout(False)
        Me.StatusBar.ResumeLayout(False)
        Me.StatusBar.PerformLayout()
        Me.MenuText.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MainMenu As MenuStrip
    Friend WithEvents MenuMacro As ContextMenuStrip
    Friend WithEvents MenuMacro_Cut As ToolStripMenuItem
    Friend WithEvents MenuMacro_Copy As ToolStripMenuItem
    Friend WithEvents MenuMacro_Paste As ToolStripMenuItem
    Friend WithEvents MenuMacro_Revert As ToolStripMenuItem
    Friend WithEvents h1 As ToolStripSeparator
    Friend WithEvents MenuMacro_Clear As ToolStripMenuItem
    Friend WithEvents MenuRow As ContextMenuStrip
    Friend WithEvents MenuRow_Cut As ToolStripMenuItem
    Friend WithEvents MenuRow_Copy As ToolStripMenuItem
    Friend WithEvents MenuRow_Paste As ToolStripMenuItem
    Friend WithEvents h2 As ToolStripSeparator
    Friend WithEvents MenuRow_Clear As ToolStripMenuItem
    Friend WithEvents MenuRow_Revert As ToolStripMenuItem
    Friend WithEvents MenuMacro_CopyClipboard As ToolStripMenuItem
    Friend WithEvents MenuMacro_PasteClipboard As ToolStripMenuItem
    Friend WithEvents MenuRow_CopyClipboard As ToolStripMenuItem
    Friend WithEvents MenuRow_PasteClipboard As ToolStripMenuItem
    Friend WithEvents MenuMain_evaluate As ToolStripMenuItem
    Friend WithEvents MenuBook As ContextMenuStrip
    Friend WithEvents MenuBook_Cut As ToolStripMenuItem
    Friend WithEvents MenuBook_Copy As ToolStripMenuItem
    Friend WithEvents MenuBook_Paste As ToolStripMenuItem
    Friend WithEvents MenuBook_h1 As ToolStripSeparator
    Friend WithEvents MenuBook_Clear As ToolStripMenuItem
    Friend WithEvents MenuBook_Revert As ToolStripMenuItem
    Friend WithEvents MenuBook_h2 As ToolStripSeparator
    Friend WithEvents MenuBook_CopyClipboard As ToolStripMenuItem
    Friend WithEvents MenuBook_PasteClipboard As ToolStripMenuItem
    Friend WithEvents MenuHandler As ContextMenuStrip
    Friend WithEvents MenuHandler_ClearSide As ToolStripMenuItem
    Friend WithEvents MenuHandler_CutSide As ToolStripMenuItem
    Friend WithEvents MenuHandler_CopySide As ToolStripMenuItem
    Friend WithEvents MenuHandler_PasteSide As ToolStripMenuItem
    Friend WithEvents OpenDialog As OpenFileDialog
    Friend WithEvents FileMenu As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As ToolStripSeparator
    Friend WithEvents File_Exit As ToolStripMenuItem
    Friend WithEvents File_Open As ToolStripMenuItem
    Friend WithEvents File_SaveRow As ToolStripMenuItem
    Friend WithEvents File_SaveAll As ToolStripMenuItem
    Friend WithEvents StatusH As ToolStripStatusLabel
    Friend WithEvents StatusD As ToolStripStatusLabel
    Friend WithEvents MenuHelp As ToolStripMenuItem
    Friend WithEvents MenuBook_SaveBookNames As ToolStripMenuItem
    Friend WithEvents MenuText As ContextMenuStrip
    Friend WithEvents menuText_Cut As ToolStripMenuItem
    Friend WithEvents menuText_Copy As ToolStripMenuItem
    Friend WithEvents menuText_Paste As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem7 As ToolStripSeparator
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents ToolStripMenuItem1 As ToolStripSeparator
    Friend WithEvents MenuBook_Import As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem3 As ToolStripSeparator
    Friend WithEvents MenuRow_Import As ToolStripMenuItem
    Public WithEvents StatusBar As StatusStrip
    Friend WithEvents ControlTip As ToolTip
    Friend WithEvents MenuMain_Search As ToolStripMenuItem
    Friend WithEvents MenuBook_Wizard As ToolStripMenuItem
    Friend WithEvents MenuBook_Wizard_BLM As ToolStripMenuItem
    Friend WithEvents MenuBook_SaveFiles As ToolStripMenuItem
    Friend WithEvents MenuHelp_Help As ToolStripMenuItem
    Friend WithEvents MenuHelp_FeatureTour As ToolStripMenuItem
    Friend WithEvents MenuBook_RenameBook As ToolStripMenuItem
    Friend WithEvents MenuBook_MacroMap As ToolStripMenuItem
    Friend WithEvents MenuRow_Save As ToolStripMenuItem
    Friend WithEvents MenuRow_CopyLocation As ToolStripMenuItem
    Friend WithEvents MenuBook_CopyMacroMap As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem4 As ToolStripSeparator
    Friend WithEvents MenuMacro_Destination As ToolStripMenuItem
    Friend WithEvents MenuBook_Header As ToolStripMenuItem
    Friend WithEvents Warning As TextBox
End Class
