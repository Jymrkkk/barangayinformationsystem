Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms.Dialogs

    Public Class ResidentDialog
        Inherits DialogBase

        Private ReadOnly _service  As New ResidentService()
        Private ReadOnly _resident As ResidentModel

        Private _txtLastName    As TextBox
        Private _txtFirstName   As TextBox
        Private _txtMiddleName  As TextBox
        Private _dtpBirthDate   As DateTimePicker
        Private _cmbGender      As ComboBox
        Private _cmbCivilStatus As ComboBox
        Private _txtAddress     As TextBox
        Private _cmbPurok       As ComboBox
        Private _txtContact     As TextBox
        Private _txtEmail       As TextBox
        Private _txtOccupation  As TextBox
        Private _chkVoter       As CheckBox
        Private _chkSoloParent  As CheckBox
        Private _cmbStatus      As ComboBox
        Private _lblResCode     As Label

        ' ── Birth Certificate controls ────────────────────────────────────
        Private _picCert        As PictureBox
        Private _lblCertStatus  As Label
        Private _certBytes      As Byte() = Nothing   ' in-memory buffer

        Public Sub New(resident As ResidentModel, mode As DialogMode)
            MyBase.New("Resident", mode, 660, 860)
            _resident = resident
            BuildForm()
            If resident IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim rx   = PX + half + 16
            Dim y    = PY

            ' Res Code badge
            _lblResCode = New Label With {
                .Text      = "Resident Code: (auto-generated)",
                .Font      = New Font("Segoe UI", 8.5F, FontStyle.Italic),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(PX, y)
            }
            pnlBody.Controls.Add(_lblResCode)
            y += 28

            ' ── Last Name | First Name ────────────────────────────────────
            AddRow("Last Name *", y)
            AddRow("First Name *", y, rx)
            y += 20
            _txtLastName  = AddTextBox(y, half)
            _txtFirstName = AddTextBox(y, half, rx)
            y += ROW

            ' ── Middle Name ───────────────────────────────────────────────
            AddRow("Middle Name", y) : y += 20
            _txtMiddleName = AddTextBox(y, half)
            y += ROW

            ' ── Birth Date | Gender ───────────────────────────────────────
            AddRow("Birth Date *", y)
            AddRow("Gender *", y, rx)
            y += 20
            _dtpBirthDate = AddDatePicker(y, half)
            _dtpBirthDate.Value = DateTime.Today.AddYears(-18)
            _cmbGender = AddComboBox(y, {"Male", "Female", "Other"}, half, rx)
            y += ROW

            ' ── Civil Status ──────────────────────────────────────────────
            AddRow("Civil Status *", y) : y += 20
            _cmbCivilStatus = AddComboBox(y, {"Single", "Married", "Widowed", "Separated"}, half)
            y += ROW

            ' ── Address ───────────────────────────────────────────────────
            AddRow("Address *", y) : y += 20
            _txtAddress = AddTextBox(y, InnerW)
            y += ROW

            ' ── Purok | Contact ───────────────────────────────────────────
            AddRow("Purok *", y)
            AddRow("Contact No.", y, rx)
            y += 20
            _cmbPurok   = AddComboBox(y, {"Purok 1", "Purok 2", "Purok 3", "Purok 4"}, half)
            _txtContact = AddTextBox(y, half, rx)
            y += ROW

            ' ── Email ─────────────────────────────────────────────────────
            AddRow("Email", y) : y += 20
            _txtEmail = AddTextBox(y, InnerW)
            y += ROW

            ' ── Occupation ────────────────────────────────────────────────
            AddRow("Occupation", y) : y += 20
            _txtOccupation = AddTextBox(y, InnerW)
            y += ROW

            ' ── Voter | Solo Parent ───────────────────────────────────────
            _chkVoter      = AddCheckBox("Registered Voter", y)
            _chkSoloParent = AddCheckBox("Solo Parent", y, rx)
            y += 34

            ' ── Senior Citizen (auto-computed from birth date — read-only) ──
            Dim chkSenior As New CheckBox With {
                .Text     = "Senior Citizen (60+)  — auto-detected from birth date",
                .Font     = New Font("Segoe UI", 9, FontStyle.Italic),
                .Location = New Point(PX, y),
                .AutoSize = True,
                .Enabled  = False
            }
            pnlBody.Controls.Add(chkSenior)
            ' Refresh the checkbox whenever the birth date picker changes
            AddHandler _dtpBirthDate.ValueChanged, Sub(s, e)
                Dim today = DateTime.Today
                Dim a     = today.Year - _dtpBirthDate.Value.Year
                If _dtpBirthDate.Value.Date > today.AddYears(-a) Then a -= 1
                chkSenior.Checked = (a >= 60)
            End Sub
            y += 30

            ' ── Status ────────────────────────────────────────────────────
            AddRow("Status *", y) : y += 20
            _cmbStatus = AddComboBox(y, {"Active", "Inactive"}, half)
            y += ROW + 6

            ' ── Birth Certificate section ─────────────────────────────────
            Dim sepLine As New Panel With {
                .Location  = New Point(PX, y),
                .Size      = New Size(InnerW, 1),
                .BackColor = UIHelper.BorderColor
            }
            pnlBody.Controls.Add(sepLine)
            y += 8

            Dim lblCertHeader As New Label With {
                .Text      = "Birth Certificate",
                .Font      = New Font("Segoe UI", 9, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = True,
                .Location  = New Point(PX, y)
            }
            pnlBody.Controls.Add(lblCertHeader)
            y += 22

            ' Preview box
            _picCert = New PictureBox With {
                .Size      = New Size(InnerW, 200),
                .Location  = New Point(PX, y),
                .SizeMode  = PictureBoxSizeMode.Zoom,
                .BackColor = ColorTranslator.FromHtml("#F5E8E8"),
                .BorderStyle = BorderStyle.FixedSingle
            }
            pnlBody.Controls.Add(_picCert)
            y += 208

            ' Status label (shows "No certificate uploaded" or filename)
            _lblCertStatus = New Label With {
                .Text      = "No birth certificate uploaded.",
                .Font      = New Font("Segoe UI", 8.5F, FontStyle.Italic),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = False,
                .Size      = New Size(InnerW, 18),
                .Location  = New Point(PX, y)
            }
            pnlBody.Controls.Add(_lblCertStatus)
            y += 22

            ' Action buttons — only shown when not ViewOnly
            If Mode <> DialogMode.ViewOnly Then
                Dim btnUpload As New Button With {
                    .Text      = "📂 Upload Image",
                    .Font      = New Font("Segoe UI", 8.5F),
                    .BackColor = UIHelper.BtnSearch,
                    .ForeColor = Color.White,
                    .FlatStyle = FlatStyle.Flat,
                    .Size      = New Size(120, 26),
                    .Location  = New Point(PX, y),
                    .Cursor    = Cursors.Hand
                }
                btnUpload.FlatAppearance.BorderSize = 0
                AddHandler btnUpload.Click, AddressOf BtnUploadCert_Click
                pnlBody.Controls.Add(btnUpload)

                Dim btnClearCert As New Button With {
                    .Text      = "✕ Remove",
                    .Font      = New Font("Segoe UI", 8.5F),
                    .BackColor = UIHelper.BtnDelete,
                    .ForeColor = Color.White,
                    .FlatStyle = FlatStyle.Flat,
                    .Size      = New Size(80, 26),
                    .Location  = New Point(PX + 128, y),
                    .Cursor    = Cursors.Hand
                }
                btnClearCert.FlatAppearance.BorderSize = 0
                AddHandler btnClearCert.Click, Sub(s, e)
                    _certBytes = Nothing
                    _picCert.Image = Nothing
                    _lblCertStatus.Text = "No birth certificate uploaded."
                End Sub
                pnlBody.Controls.Add(btnClearCert)
                y += 34
            Else
                ' ViewOnly — show a "View Full Size" button if cert exists
                Dim btnView As New Button With {
                    .Text      = "🔍 View Full Size",
                    .Font      = New Font("Segoe UI", 8.5F),
                    .BackColor = UIHelper.NavActive,
                    .ForeColor = Color.White,
                    .FlatStyle = FlatStyle.Flat,
                    .Size      = New Size(120, 26),
                    .Location  = New Point(PX, y),
                    .Cursor    = Cursors.Hand
                }
                btnView.FlatAppearance.BorderSize = 0
                AddHandler btnView.Click, AddressOf BtnViewCertFullSize_Click
                pnlBody.Controls.Add(btnView)
                y += 34
            End If

            AddBottomSpacer(y)
            _cmbGender.SelectedIndex      = 0
            _cmbCivilStatus.SelectedIndex = 0
            _cmbPurok.SelectedIndex       = 0
            _cmbStatus.SelectedIndex      = 0
        End Sub

        Private Sub PopulateFields()
            _lblResCode.Text    = $"Resident Code: {_resident.ResCode}"
            _txtLastName.Text   = _resident.LastName
            _txtFirstName.Text  = _resident.FirstName
            _txtMiddleName.Text = _resident.MiddleName
            _dtpBirthDate.Value = If(_resident.BirthDate = DateTime.MinValue,
                                     DateTime.Today.AddYears(-18), _resident.BirthDate)
            _txtAddress.Text    = _resident.Address
            _txtContact.Text    = _resident.ContactNo
            _txtEmail.Text      = _resident.Email
            _txtOccupation.Text = _resident.Occupation
            _chkVoter.Checked      = _resident.IsVoter
            _chkSoloParent.Checked = _resident.IsSoloParent
            SetCombo(_cmbGender,      _resident.Gender)
            SetCombo(_cmbCivilStatus, _resident.CivilStatus)
            SetCombo(_cmbPurok,       _resident.Purok)
            SetCombo(_cmbStatus,      If(_resident.IsActive, "Active", "Inactive"))

            ' Load birth certificate preview
            If _resident.BirthCertificate IsNot Nothing AndAlso _resident.BirthCertificate.Length > 0 Then
                _certBytes = _resident.BirthCertificate
                LoadCertPreview(_certBytes)
                _lblCertStatus.Text = $"Certificate on file ({_certBytes.Length \ 1024} KB)"
            End If
            ' Trigger the birth-date ValueChanged to sync the Senior Citizen checkbox
            _dtpBirthDate.Value = _dtpBirthDate.Value
        End Sub

        Private Sub SetCombo(cmb As ComboBox, value As String)
            Dim idx = cmb.Items.IndexOf(value)
            If idx >= 0 Then cmb.SelectedIndex = idx
        End Sub

        ' ── Birth certificate upload ──────────────────────────────────────
        Private Sub BtnUploadCert_Click(sender As Object, e As EventArgs)
            Using ofd As New OpenFileDialog With {
                .Title  = "Select Birth Certificate Image",
                .Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            }
                If ofd.ShowDialog() <> DialogResult.OK Then Return
                Try
                    Dim bytes = File.ReadAllBytes(ofd.FileName)
                    ' Warn if file is very large (> 5 MB)
                    If bytes.Length > 5 * 1024 * 1024 Then
                        If MessageBox.Show(
                            $"The selected file is {bytes.Length \ 1024 \ 1024} MB. Large images may slow the system." &
                            Environment.NewLine & "Continue anyway?",
                            "Large File Warning", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) = DialogResult.No Then Return
                    End If
                    _certBytes = bytes
                    LoadCertPreview(bytes)
                    _lblCertStatus.Text = $"{Path.GetFileName(ofd.FileName)}  ({bytes.Length \ 1024} KB)"
                Catch ex As Exception
                    MessageBox.Show($"Could not load image: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Sub LoadCertPreview(bytes As Byte())
            Try
                Using ms As New MemoryStream(bytes)
                    _picCert.Image = Image.FromStream(ms)
                End Using
            Catch
                _picCert.Image = Nothing
                _lblCertStatus.Text = "⚠ Could not preview image."
            End Try
        End Sub

        ' ── View full-size in a separate window ───────────────────────────
        Private Sub BtnViewCertFullSize_Click(sender As Object, e As EventArgs)
            If _certBytes Is Nothing OrElse _certBytes.Length = 0 Then
                MessageBox.Show("No birth certificate on file for this resident.",
                                "No Certificate", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Try
                Using ms As New MemoryStream(_certBytes)
                    Dim img = Image.FromStream(ms)
                    Dim viewer As New Form With {
                        .Text            = $"Birth Certificate — {_resident?.FullName}",
                        .Size            = New Size(900, 700),
                        .StartPosition   = FormStartPosition.CenterParent,
                        .FormBorderStyle = FormBorderStyle.Sizable,
                        .BackColor       = Color.Black
                    }
                    Dim pic As New PictureBox With {
                        .Dock     = DockStyle.Fill,
                        .Image    = img,
                        .SizeMode = PictureBoxSizeMode.Zoom
                    }
                    viewer.Controls.Add(pic)
                    viewer.ShowDialog()
                End Using
            Catch ex As Exception
                MessageBox.Show($"Could not display certificate: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim model As New ResidentModel With {
                .ResidentId       = If(_resident IsNot Nothing, _resident.ResidentId, 0),
                .ResCode          = If(_resident IsNot Nothing, _resident.ResCode, ""),
                .LastName         = _txtLastName.Text.Trim(),
                .FirstName        = _txtFirstName.Text.Trim(),
                .MiddleName       = _txtMiddleName.Text.Trim(),
                .BirthDate        = _dtpBirthDate.Value.Date,
                .Gender           = _cmbGender.SelectedItem?.ToString(),
                .CivilStatus      = _cmbCivilStatus.SelectedItem?.ToString(),
                .Address          = _txtAddress.Text.Trim(),
                .Purok            = _cmbPurok.SelectedItem?.ToString(),
                .ContactNo        = _txtContact.Text.Trim(),
                .Email            = _txtEmail.Text.Trim(),
                .Occupation       = _txtOccupation.Text.Trim(),
                .IsVoter          = _chkVoter.Checked,
                .IsSoloParent     = _chkSoloParent.Checked,
                .BirthCertificate = _certBytes,
                .IsActive         = (_cmbStatus.SelectedItem?.ToString() = "Active"),
                .CreatedBy        = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.SaveResident(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

End Namespace
