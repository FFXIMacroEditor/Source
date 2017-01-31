<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Help
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Help))
        Me.HelpInfo = New System.Windows.Forms.RichTextBox()
        Me.SuspendLayout()
        '
        'HelpInfo
        '
        Me.HelpInfo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.HelpInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.HelpInfo.Location = New System.Drawing.Point(13, 13)
        Me.HelpInfo.Name = "HelpInfo"
        Me.HelpInfo.ReadOnly = True
        Me.HelpInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        Me.HelpInfo.Size = New System.Drawing.Size(733, 490)
        Me.HelpInfo.TabIndex = 0
        Me.HelpInfo.Text = resources.GetString("HelpInfo.Text")
        '
        'Help
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(758, 515)
        Me.Controls.Add(Me.HelpInfo)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Help"
        Me.Text = "Help"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents HelpInfo As RichTextBox
End Class
