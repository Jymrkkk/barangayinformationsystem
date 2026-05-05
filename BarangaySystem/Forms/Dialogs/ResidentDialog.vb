Imports System.Drawing
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

        Public Sub New(resident As ResidentModel, mode As DialogMode)
            MyBase.New("Resident", mode, 660, 760)
            _resident = resident
            BuildForm()
            If resident IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2   ' half-width for two-column rows
            Dim rx   = PX + half + 16       ' right column x
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

            ' ── Status ────────────────────────────────────────────────────
            AddRow("Status *", y) : y += 20
            _cmbStatus = AddComboBox(y, {"Active", "Inactive"}, half)
            y += ROW

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
        End Sub

        Private Sub SetCombo(cmb As ComboBox, value As String)
            Dim idx = cmb.Items.IndexOf(value)
            If idx >= 0 Then cmb.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim model As New ResidentModel With {
                .ResidentId    = If(_resident IsNot Nothing, _resident.ResidentId, 0),
                .ResCode       = If(_resident IsNot Nothing, _resident.ResCode, ""),
                .LastName      = _txtLastName.Text.Trim(),
                .FirstName     = _txtFirstName.Text.Trim(),
                .MiddleName    = _txtMiddleName.Text.Trim(),
                .BirthDate     = _dtpBirthDate.Value.Date,
                .Gender        = _cmbGender.SelectedItem?.ToString(),
                .CivilStatus   = _cmbCivilStatus.SelectedItem?.ToString(),
                .Address       = _txtAddress.Text.Trim(),
                .Purok         = _cmbPurok.SelectedItem?.ToString(),
                .ContactNo     = _txtContact.Text.Trim(),
                .Email         = _txtEmail.Text.Trim(),
                .Occupation    = _txtOccupation.Text.Trim(),
                .IsVoter       = _chkVoter.Checked,
                .IsSoloParent  = _chkSoloParent.Checked,
                .IsActive      = (_cmbStatus.SelectedItem?.ToString() = "Active"),
                .CreatedBy     = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
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
