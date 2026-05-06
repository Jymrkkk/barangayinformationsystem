Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.Helpers

Namespace BarangaySystem.Forms.Dialogs

    Public Enum DialogMode
        AddNew
        EditExisting
        ViewOnly
    End Enum

    Public MustInherit Class DialogBase
        Inherits Form

        Protected ReadOnly Mode As DialogMode

        ' pnlBody is the scrollable container — controls go inside _inner
        Protected pnlBody   As Panel
        Protected btnSave   As Button
        Protected btnCancel As Button

        ' All field builders place controls relative to these offsets
        Protected Const PX  As Integer = 20   ' left margin inside body
        Protected Const PY  As Integer = 65  ' top margin — first label starts here
        Protected Const ROW As Integer = 38   ' vertical step per field row (label + input + gap)

        Private Shared Function ModeLabel(m As DialogMode) As String
            Select Case m
                Case DialogMode.AddNew       : Return "Add"
                Case DialogMode.EditExisting : Return "Edit"
                Case DialogMode.ViewOnly     : Return "View"
                Case Else                    : Return m.ToString()
            End Select
        End Function

        Public Sub New(title As String, mode As DialogMode,
                       Optional width As Integer = 620,
                       Optional height As Integer = 580)
            Me.Mode            = mode
            Dim lbl            = ModeLabel(mode)
            Me.Text            = $"{lbl} {title}"
            Me.Size            = New Size(width, height)
            Me.MinimumSize     = New Size(width, height)
            Me.StartPosition   = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox     = False
            Me.MinimizeBox     = False
            Me.BackColor       = UIHelper.Surface

            ' ── Header ───────────────────────────────────────────────────
            Dim pnlHeader As New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 54,
                .BackColor = UIHelper.TitleBar
            }
            Dim lblHeader As New Label With {
                .Text      = $"{lbl} {title}",
                .Font      = New Font("Segoe UI", 13, FontStyle.Bold),
                .ForeColor = Color.White,
                .Dock      = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding   = New Padding(18, 0, 0, 0)
            }
            pnlHeader.Controls.Add(lblHeader)

            ' ── Body — plain panel, no Padding (we use PX/PY constants) ──
            pnlBody = New Panel With {
                .Dock       = DockStyle.Fill,
                .AutoScroll = True,
                .BackColor  = Color.White
            }

            ' ── Footer ───────────────────────────────────────────────────
            Dim pnlFooter As New Panel With {
                .Dock      = DockStyle.Bottom,
                .Height    = 56,
                .BackColor = UIHelper.Surface
            }
            AddHandler pnlFooter.Paint, Sub(s, e)
                e.Graphics.DrawLine(New Pen(UIHelper.BorderColor), 0, 0, pnlFooter.Width, 0)
            End Sub

            btnSave = New Button With {
                .Text      = If(mode = DialogMode.ViewOnly, "Close", "Save"),
                .Font      = New Font("Segoe UI", 10, FontStyle.Bold),
                .BackColor = If(mode = DialogMode.ViewOnly, UIHelper.BtnPrint, UIHelper.BtnAdd),
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(110, 36),
                .Cursor    = Cursors.Hand
            }
            btnSave.FlatAppearance.BorderSize = 0

            btnCancel = New Button With {
                .Text      = "Cancel",
                .Font      = New Font("Segoe UI", 10),
                .BackColor = UIHelper.BtnPrint,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(110, 36),
                .Cursor    = Cursors.Hand,
                .Visible   = (mode <> DialogMode.ViewOnly)
            }
            btnCancel.FlatAppearance.BorderSize = 0

            Dim PositionBtns = Sub()
                btnCancel.Location = New Point(pnlFooter.Width - 130, 10)
                btnSave.Location   = New Point(pnlFooter.Width - 250, 10)
                If mode = DialogMode.ViewOnly Then
                    btnSave.Location = New Point(pnlFooter.Width - 130, 10)
                End If
            End Sub
            AddHandler pnlFooter.Resize, Sub(s, e) PositionBtns()
            AddHandler Me.Shown,         Sub(s, e) PositionBtns()

            AddHandler btnSave.Click,   AddressOf BtnSave_Click
            AddHandler btnCancel.Click, Sub(s, e) Me.DialogResult = DialogResult.Cancel

            pnlFooter.Controls.AddRange({btnSave, btnCancel})
            Me.Controls.AddRange({pnlHeader, pnlBody, pnlFooter})
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Protected MustOverride Sub BtnSave_Click(sender As Object, e As EventArgs)

        ' ── Usable inner width ────────────────────────────────────────────
        Protected ReadOnly Property InnerW As Integer
            Get
                Return Me.ClientSize.Width - PX * 2 - 24  ' 24 = scrollbar allowance
            End Get
        End Property

        ' ── Field builders ────────────────────────────────────────────────

        Protected Function AddRow(text As String, yPos As Integer,
                                  Optional xPos As Integer = PX) As (Label As Label, Y As Integer)
            Dim lbl As New Label With {
                .Text      = text,
                .Font      = New Font("Segoe UI", 9),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(xPos, yPos)
            }
            pnlBody.Controls.Add(lbl)
            Return (lbl, yPos + 20)
        End Function

        Protected Function AddTextBox(yPos As Integer,
                                      Optional width As Integer = -1,
                                      Optional xPos As Integer = PX,
                                      Optional isReadOnly As Boolean = False) As TextBox
            If width < 0 Then width = InnerW
            Dim txt As New TextBox With {
                .Font        = New Font("Segoe UI", 10),
                .Size        = New Size(width, 28),
                .Location    = New Point(xPos, yPos),
                .BorderStyle = BorderStyle.FixedSingle,
                .ReadOnly    = isReadOnly OrElse (Mode = DialogMode.ViewOnly),
                .BackColor   = If(isReadOnly OrElse Mode = DialogMode.ViewOnly,
                                  UIHelper.Surface, Color.White)
            }
            pnlBody.Controls.Add(txt)
            Return txt
        End Function

        Protected Function AddComboBox(yPos As Integer, items As String(),
                                       Optional width As Integer = 220,
                                       Optional xPos As Integer = PX) As ComboBox
            Dim cmb As New ComboBox With {
                .Font          = New Font("Segoe UI", 10),
                .Size          = New Size(width, 28),
                .Location      = New Point(xPos, yPos),
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Enabled       = (Mode <> DialogMode.ViewOnly)
            }
            cmb.Items.AddRange(items)
            pnlBody.Controls.Add(cmb)
            Return cmb
        End Function

        Protected Function AddDatePicker(yPos As Integer,
                                         Optional width As Integer = 220,
                                         Optional xPos As Integer = PX) As DateTimePicker
            Dim dtp As New DateTimePicker With {
                .Font     = New Font("Segoe UI", 10),
                .Size     = New Size(width, 28),
                .Location = New Point(xPos, yPos),
                .Format   = DateTimePickerFormat.Short,
                .Enabled  = (Mode <> DialogMode.ViewOnly)
            }
            pnlBody.Controls.Add(dtp)
            Return dtp
        End Function

        Protected Function AddCheckBox(text As String, yPos As Integer,
                                       Optional xPos As Integer = PX) As CheckBox
            Dim chk As New CheckBox With {
                .Text     = text,
                .Font     = New Font("Segoe UI", 10),
                .Location = New Point(xPos, yPos),
                .AutoSize = True,
                .Enabled  = (Mode <> DialogMode.ViewOnly)
            }
            pnlBody.Controls.Add(chk)
            Return chk
        End Function

        Protected Function AddNumericUpDown(yPos As Integer, min As Integer, max As Integer,
                                            Optional width As Integer = 140,
                                            Optional xPos As Integer = PX) As NumericUpDown
            Dim nud As New NumericUpDown With {
                .Font     = New Font("Segoe UI", 10),
                .Size     = New Size(width, 28),
                .Location = New Point(xPos, yPos),
                .Minimum  = min,
                .Maximum  = max,
                .Enabled  = (Mode <> DialogMode.ViewOnly)
            }
            pnlBody.Controls.Add(nud)
            Return nud
        End Function

        Protected Sub ShowError(message As String)
            MessageBox.Show(message, "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Sub

        Protected Sub ShowSuccess(message As String)
            MessageBox.Show(message, "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        ''' <summary>
        ''' Call this at the END of every BuildForm() to add a bottom spacer.
        ''' This ensures the last field is never hidden behind the footer panel.
        ''' Pass the y position of the last field + its height.
        ''' </summary>
        Protected Sub AddBottomSpacer(lastFieldBottomY As Integer)
            Dim spacer As New Panel With {
                .Location  = New Point(0, lastFieldBottomY + 16),
                .Size      = New Size(1, 1),
                .BackColor = Color.Transparent
            }
            pnlBody.Controls.Add(spacer)
        End Sub

    End Class

End Namespace
