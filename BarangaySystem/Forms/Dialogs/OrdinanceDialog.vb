Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms.Dialogs

    Public Class OrdinanceDialog
        Inherits DialogBase

        Private ReadOnly _service   As New OrdinanceService()
        Private ReadOnly _ordinance As OrdinanceModel

        Private _txtBoNumber     As TextBox
        Private _txtIntroducedBy As TextBox
        Private _txtDescription  As TextBox
        Private _txtFullText     As TextBox
        Private _dtpDateEnacted  As DateTimePicker
        Private _txtApprovedBy   As TextBox
        Private _cmbStatus       As ComboBox

        Public Sub New(ordinance As OrdinanceModel, mode As DialogMode)
            MyBase.New("Ordinance", mode, 640, 780)
            _ordinance = ordinance
            BuildForm()
            If ordinance IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim rx   = PX + half + 16
            Dim y    = PY

            AddRow("BO Number *  (e.g. BO-2025-001)", y) : y += 20
            _txtBoNumber = AddTextBox(y, half) : y += ROW

            AddRow("Introduced By *", y) : y += 20
            _txtIntroducedBy = AddTextBox(y, half) : y += ROW

            AddRow("Description *", y) : y += 20
            _txtDescription = AddTextBox(y, InnerW) : y += ROW

            AddRow("Full Text (optional)", y) : y += 20
            _txtFullText = AddTextBox(y, InnerW)
            _txtFullText.Multiline = True
            _txtFullText.Height    = 80
            y += 92

            AddRow("Date Enacted *", y)
            AddRow("Status *", y, rx)
            y += 20
            _dtpDateEnacted = AddDatePicker(y, half)
            _cmbStatus      = AddComboBox(y, {"Active", "Inactive", "Repealed"}, half, rx)
            y += ROW

            AddRow("Approved By", y) : y += 20
            _txtApprovedBy = AddTextBox(y, half)
            y += ROW

            AddBottomSpacer(y)
            _cmbStatus.SelectedIndex = 0
        End Sub

        Private Sub PopulateFields()
            _txtBoNumber.Text     = _ordinance.BoNumber
            _txtIntroducedBy.Text = _ordinance.IntroducedBy
            _txtDescription.Text  = _ordinance.Description
            _txtFullText.Text     = _ordinance.FullText
            _dtpDateEnacted.Value = If(_ordinance.DateEnacted = DateTime.MinValue,
                                       DateTime.Today, _ordinance.DateEnacted)
            _txtApprovedBy.Text   = _ordinance.ApprovedBy
            Dim idx = _cmbStatus.Items.IndexOf(_ordinance.Status)
            If idx >= 0 Then _cmbStatus.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim model As New OrdinanceModel With {
                .OrdinanceId  = If(_ordinance IsNot Nothing, _ordinance.OrdinanceId, 0),
                .BoNumber     = _txtBoNumber.Text.Trim(),
                .IntroducedBy = _txtIntroducedBy.Text.Trim(),
                .Description  = _txtDescription.Text.Trim(),
                .FullText     = _txtFullText.Text.Trim(),
                .DateEnacted  = _dtpDateEnacted.Value.Date,
                .ApprovedBy   = _txtApprovedBy.Text.Trim(),
                .Status       = _cmbStatus.SelectedItem?.ToString(),
                .CreatedBy    = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.SaveOrdinance(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

    Public Class ResolutionDialog
        Inherits DialogBase

        Private ReadOnly _service    As New OrdinanceService()
        Private ReadOnly _resolution As ResolutionModel

        Private _txtResNumber  As TextBox
        Private _txtSubject    As TextBox
        Private _txtSponsor    As TextBox
        Private _dtpDatePassed As DateTimePicker
        Private _cmbStatus     As ComboBox

        Public Sub New(resolution As ResolutionModel, mode As DialogMode)
            MyBase.New("Resolution", mode, 620, 580)
            _resolution = resolution
            BuildForm()
            If resolution IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim rx   = PX + half + 16
            Dim y    = PY

            AddRow("Resolution Number *  (e.g. BR-2025-001)", y) : y += 20
            _txtResNumber = AddTextBox(y, half) : y += ROW

            AddRow("Subject *", y) : y += 20
            _txtSubject = AddTextBox(y, InnerW) : y += ROW

            AddRow("Sponsor", y) : y += 20
            _txtSponsor = AddTextBox(y, half) : y += ROW

            AddRow("Date Passed *", y)
            AddRow("Status *", y, rx)
            y += 20
            _dtpDatePassed = AddDatePicker(y, half)
            _cmbStatus     = AddComboBox(y, {"Approved", "Pending", "Rejected"}, half, rx)
            y += ROW

            AddBottomSpacer(y)
            _cmbStatus.SelectedIndex = 1
        End Sub

        Private Sub PopulateFields()
            _txtResNumber.Text   = _resolution.ResNumber
            _txtSubject.Text     = _resolution.Subject
            _txtSponsor.Text     = _resolution.Sponsor
            _dtpDatePassed.Value = If(_resolution.DatePassed = DateTime.MinValue,
                                      DateTime.Today, _resolution.DatePassed)
            Dim idx = _cmbStatus.Items.IndexOf(_resolution.Status)
            If idx >= 0 Then _cmbStatus.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim model As New ResolutionModel With {
                .ResolutionId = If(_resolution IsNot Nothing, _resolution.ResolutionId, 0),
                .ResNumber    = _txtResNumber.Text.Trim(),
                .Subject      = _txtSubject.Text.Trim(),
                .Sponsor      = _txtSponsor.Text.Trim(),
                .DatePassed   = _dtpDatePassed.Value.Date,
                .Status       = _cmbStatus.SelectedItem?.ToString(),
                .CreatedBy    = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.SaveResolution(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

End Namespace
