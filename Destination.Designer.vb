<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Destination
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
        Me.ControlTItles = New System.Windows.Forms.TextBox()
        Me.DestContents = New System.Windows.Forms.ComboBox()
        Me.AlternateTitles = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'ControlTItles
        '
        Me.ControlTItles.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.ControlTItles.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ControlTItles.Font = New System.Drawing.Font("Courier New", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ControlTItles.Location = New System.Drawing.Point(13, 71)
        Me.ControlTItles.Name = "ControlTItles"
        Me.ControlTItles.Size = New System.Drawing.Size(949, 17)
        Me.ControlTItles.TabIndex = 1
        '
        'DestContents
        '
        Me.DestContents.Font = New System.Drawing.Font("Courier New", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DestContents.FormattingEnabled = True
        Me.DestContents.Location = New System.Drawing.Point(13, 13)
        Me.DestContents.Name = "DestContents"
        Me.DestContents.Size = New System.Drawing.Size(160, 29)
        Me.DestContents.TabIndex = 0
        '
        'AlternateTitles
        '
        Me.AlternateTitles.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.AlternateTitles.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.AlternateTitles.Font = New System.Drawing.Font("Courier New", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.AlternateTitles.Location = New System.Drawing.Point(13, 117)
        Me.AlternateTitles.Name = "AlternateTitles"
        Me.AlternateTitles.Size = New System.Drawing.Size(949, 17)
        Me.AlternateTitles.TabIndex = 2
        '
        'Destination
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(974, 180)
        Me.Controls.Add(Me.AlternateTitles)
        Me.Controls.Add(Me.ControlTItles)
        Me.Controls.Add(Me.DestContents)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "Destination"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ControlTItles As TextBox
    Friend WithEvents DestContents As ComboBox
    Friend WithEvents AlternateTitles As TextBox
End Class
