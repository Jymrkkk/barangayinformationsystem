Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms.Dialogs

    Public Class ActivityDialog
        Inherits DialogBase

        Private ReadOnly _service  As New ActivityService()
        Private ReadOnly _activity As ActivityModel

        Private _txtName         As TextBox
        Private _txtDescription  As TextBox
        Private _dtpDate         As DateTimePicker
        Private _txtVenue        As TextBox
        Private _txtOrganizer    As TextBox
        Private _nudParticipants As NumericUpDown
        Private _cmbStatus       As ComboBox

        Public Sub New(activity As ActivityModel, mode As DialogMode)
            MyBase.New("Activity", mode, 620, 560)
            _activity = activity
            BuildForm()
            If activity IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim rx   = PX + half + 16
            Dim y    = PY

            AddRow("Activity Name *", y) : y += 20
            _txtName = AddTextBox(y, InnerW) : y += ROW

            AddRow("Description", y) : y += 20
            _txtDescription = AddTextBox(y, InnerW)
            _txtDescription.Multiline = True
            _txtDescription.Height    = 72
            y += 84

            AddRow("Activity Date *", y)
            AddRow("Status *", y, rx)
            y += 20
            _dtpDate   = AddDatePicker(y, half)
            _cmbStatus = AddComboBox(y, {"Upcoming", "Ongoing", "Completed", "Cancelled"}, half, rx)
            y += ROW

            AddRow("Venue *", y) : y += 20
            _txtVenue = AddTextBox(y, InnerW) : y += ROW

            AddRow("Organizer *", y) : y += 20
            _txtOrganizer = AddTextBox(y, half) : y += ROW

            AddRow("Participants", y) : y += 20
            _nudParticipants = AddNumericUpDown(y, 0, 99999, 150)
            y += ROW

            AddBottomSpacer(y)
            _cmbStatus.SelectedIndex = 0
        End Sub

        Private Sub PopulateFields()
            _txtName.Text          = _activity.ActivityName
            _txtDescription.Text   = _activity.Description
            _dtpDate.Value         = If(_activity.ActivityDate = DateTime.MinValue,
                                        DateTime.Today, _activity.ActivityDate)
            _txtVenue.Text         = _activity.Venue
            _txtOrganizer.Text     = _activity.Organizer
            _nudParticipants.Value = _activity.Participants
            Dim idx = _cmbStatus.Items.IndexOf(_activity.Status)
            If idx >= 0 Then _cmbStatus.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim model As New ActivityModel With {
                .ActivityId   = If(_activity IsNot Nothing, _activity.ActivityId, 0),
                .ActCode      = If(_activity IsNot Nothing, _activity.ActCode, ""),
                .ActivityName = _txtName.Text.Trim(),
                .Description  = _txtDescription.Text.Trim(),
                .ActivityDate = _dtpDate.Value.Date,
                .Venue        = _txtVenue.Text.Trim(),
                .Organizer    = _txtOrganizer.Text.Trim(),
                .Participants = CInt(_nudParticipants.Value),
                .Status       = _cmbStatus.SelectedItem?.ToString(),
                .CreatedBy    = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.Save(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

End Namespace
