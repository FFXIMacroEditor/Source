<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Assessment
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
        Me.Results = New System.Windows.Forms.ListView()
        Me.Results_Location = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Results_Type = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Results_Description = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Results_iType = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Results
        '
        Me.Results.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Results_Location, Me.Results_Type, Me.Results_Description, Me.Results_iType})
        Me.Results.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Results.FullRowSelect = True
        Me.Results.Location = New System.Drawing.Point(12, 12)
        Me.Results.MultiSelect = False
        Me.Results.Name = "Results"
        Me.Results.ShowItemToolTips = True
        Me.Results.Size = New System.Drawing.Size(760, 266)
        Me.Results.TabIndex = 0
        Me.Results.UseCompatibleStateImageBehavior = False
        Me.Results.View = System.Windows.Forms.View.Details
        '
        'Results_Location
        '
        Me.Results_Location.Text = "Location"
        Me.Results_Location.Width = 250
        '
        'Results_Type
        '
        Me.Results_Type.Text = "Type"
        Me.Results_Type.Width = 150
        '
        'Results_Description
        '
        Me.Results_Description.Text = "Description"
        Me.Results_Description.Width = 325
        '
        'Results_iType
        '
        Me.Results_iType.Width = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(13, 281)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(429, 20)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Double-click a row to go to macro; right click for more details"
        '
        'Assessment
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(784, 306)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Results)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "Assessment"
        Me.Text = "Results"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Results As ListView
    Friend WithEvents Results_Location As ColumnHeader
    Friend WithEvents Results_Type As ColumnHeader
    Friend WithEvents Results_Description As ColumnHeader
    Friend WithEvents Results_iType As ColumnHeader
    Friend WithEvents Label1 As Label
End Class
