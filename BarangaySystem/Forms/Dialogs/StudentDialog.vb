Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms.Dialogs

    Public Class StudentDialog
        Inherits DialogBase

        Private ReadOnly _service As New StudentService()
        Private ReadOnly _student As StudentModel

        Private _txtLastName   As TextBox
        Private _txtFirstName  As TextBox
        Private _txtMiddleName As TextBox
        Private _dtpBirthDate  As DateTimePicker
        Private _cmbGender     As ComboBox
        Private _txtAddress    As TextBox
        Private _cmbPurok      As ComboBox
        Private _cmbSchool     As ComboBox
        Private _txtGradeYear  As TextBox
        Private _txtSchoolYear As TextBox
        Private _chkScholar    As CheckBox
        Private _cmbStatus     As ComboBox
        Private _schools       As List(Of SchoolModel)

        Public Sub New(student As StudentModel, mode As DialogMode)
            MyBase.New("Student", mode, 660, 820)
            _student = student
            LoadSchools()
            BuildForm()
            If student IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub LoadSchools()
            Try
                _schools = New SchoolRepository().GetAll()
            Catch
                _schools = New List(Of SchoolModel)()
            End Try
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim rx   = PX + half + 16
            Dim y    = PY

            ' Last Name | First Name
            AddRow("Last Name *", y)
            AddRow("First Name *", y, rx)
            y += 20
            _txtLastName  = AddTextBox(y, half)
            _txtFirstName = AddTextBox(y, half, rx)
            y += ROW

            ' Middle Name
            AddRow("Middle Name", y) : y += 20
            _txtMiddleName = AddTextBox(y, half) : y += ROW

            ' Birth Date | Gender
            AddRow("Birth Date", y)
            AddRow("Gender", y, rx)
            y += 20
            _dtpBirthDate = AddDatePicker(y, half)
            _cmbGender    = AddComboBox(y, {"Male", "Female", "Other"}, half, rx)
            y += ROW

            ' Address
            AddRow("Address *", y) : y += 20
            _txtAddress = AddTextBox(y, InnerW) : y += ROW

            ' Purok
            AddRow("Purok *", y) : y += 20
            _cmbPurok = AddComboBox(y, {"Purok 1", "Purok 2", "Purok 3", "Purok 4"}, half)
            y += ROW

            ' School
            AddRow("School", y) : y += 20
            _cmbSchool = New ComboBox With {
                .Font          = New Font("Segoe UI", 10),
                .Size          = New Size(InnerW, 28),
                .Location      = New Point(PX, y),
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Enabled       = (Mode <> DialogMode.ViewOnly)
            }
            _cmbSchool.Items.Add("-- Select School --")
            For Each s In _schools : _cmbSchool.Items.Add(s.SchoolName) : Next
            _cmbSchool.SelectedIndex = 0
            pnlBody.Controls.Add(_cmbSchool)
            y += ROW

            ' Grade/Year | School Year
            AddRow("Grade / Year Level *", y)
            AddRow("School Year *", y, rx)
            y += 20
            _txtGradeYear  = AddTextBox(y, half)
            _txtSchoolYear = AddTextBox(y, half, rx)
            y += ROW

            ' Status | Scholar
            AddRow("Status *", y) : y += 20
            _cmbStatus  = AddComboBox(y, {"Enrolled", "Dropped", "Graduated"}, half)
            _chkScholar = AddCheckBox("Scholarship Recipient", y, rx)
            y += ROW

            AddBottomSpacer(y)
            _cmbGender.SelectedIndex = 0
            _cmbPurok.SelectedIndex  = 0
            _cmbStatus.SelectedIndex = 0
        End Sub

        Private Sub PopulateFields()
            _txtLastName.Text   = _student.LastName
            _txtFirstName.Text  = _student.FirstName
            _txtMiddleName.Text = _student.MiddleName
            If _student.BirthDate.HasValue Then _dtpBirthDate.Value = _student.BirthDate.Value
            _txtAddress.Text    = _student.Address
            _txtGradeYear.Text  = _student.GradeYear
            _txtSchoolYear.Text = _student.SchoolYear
            _chkScholar.Checked = _student.IsScholar
            SetCombo(_cmbGender, _student.Gender)
            SetCombo(_cmbPurok,  _student.Purok)
            SetCombo(_cmbStatus, _student.Status)
            If _student.SchoolId.HasValue Then
                Dim sc = _schools.FirstOrDefault(Function(s) s.SchoolId = _student.SchoolId.Value)
                If sc IsNot Nothing Then
                    Dim idx = _cmbSchool.Items.IndexOf(sc.SchoolName)
                    If idx >= 0 Then _cmbSchool.SelectedIndex = idx
                End If
            End If
        End Sub

        Private Sub SetCombo(cmb As ComboBox, value As String)
            Dim idx = cmb.Items.IndexOf(value)
            If idx >= 0 Then cmb.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim schoolId As Integer? = Nothing
            If _cmbSchool.SelectedIndex > 0 Then
                Dim sn = _cmbSchool.SelectedItem.ToString()
                Dim sc = _schools.FirstOrDefault(Function(s) s.SchoolName = sn)
                If sc IsNot Nothing Then schoolId = sc.SchoolId
            End If

            Dim model As New StudentModel With {
                .StudentId  = If(_student IsNot Nothing, _student.StudentId, 0),
                .StudCode   = If(_student IsNot Nothing, _student.StudCode, ""),
                .LastName   = _txtLastName.Text.Trim(),
                .FirstName  = _txtFirstName.Text.Trim(),
                .MiddleName = _txtMiddleName.Text.Trim(),
                .BirthDate  = _dtpBirthDate.Value.Date,
                .Gender     = _cmbGender.SelectedItem?.ToString(),
                .Address    = _txtAddress.Text.Trim(),
                .Purok      = _cmbPurok.SelectedItem?.ToString(),
                .SchoolId   = schoolId,
                .GradeYear  = _txtGradeYear.Text.Trim(),
                .SchoolYear = _txtSchoolYear.Text.Trim(),
                .IsScholar  = _chkScholar.Checked,
                .Status     = _cmbStatus.SelectedItem?.ToString(),
                .CreatedBy  = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.SaveStudent(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

    Public Class ScholarshipDialog
        Inherits DialogBase

        Private ReadOnly _service     As New StudentService()
        Private ReadOnly _scholarship As ScholarshipModel

        Private _cmbStudent    As ComboBox
        Private _txtGrantType  As TextBox
        Private _nudAmount     As NumericUpDown
        Private _txtSchoolYear As TextBox
        Private _cmbStatus     As ComboBox
        Private _students      As List(Of StudentModel)

        Public Sub New(scholarship As ScholarshipModel, mode As DialogMode)
            MyBase.New("Scholarship", mode, 600, 620)
            _scholarship = scholarship
            LoadStudents()
            BuildForm()
            If scholarship IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub LoadStudents()
            Try
                _students = _service.GetStudents()
            Catch
                _students = New List(Of StudentModel)()
            End Try
        End Sub

        Private Sub BuildForm()
            Dim y = PY

            AddRow("Student *", y) : y += 20
            _cmbStudent = New ComboBox With {
                .Font          = New Font("Segoe UI", 10),
                .Size          = New Size(InnerW, 28),
                .Location      = New Point(PX, y),
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Enabled       = (Mode <> DialogMode.ViewOnly)
            }
            _cmbStudent.Items.Add("-- Select Student --")
            For Each s In _students : _cmbStudent.Items.Add(s.FullName) : Next
            _cmbStudent.SelectedIndex = 0
            pnlBody.Controls.Add(_cmbStudent)
            y += ROW

            AddRow("Grant Type *", y) : y += 20
            _txtGrantType = AddTextBox(y, InnerW) : y += ROW

            AddRow("Amount (Php)", y) : y += 20
            _nudAmount = AddNumericUpDown(y, 0, 999999, 180)
            _nudAmount.DecimalPlaces = 2
            y += ROW

            AddRow("School Year *  (e.g. 2024-2025)", y) : y += 20
            _txtSchoolYear = AddTextBox(y, 200) : y += ROW

            AddRow("Status *", y) : y += 20
            _cmbStatus = AddComboBox(y, {"Active", "Inactive", "Completed"}, 220)
            y += ROW

            AddBottomSpacer(y)
            _cmbStatus.SelectedIndex = 0
        End Sub

        Private Sub PopulateFields()
            _txtGrantType.Text  = _scholarship.GrantType
            _nudAmount.Value    = _scholarship.Amount
            _txtSchoolYear.Text = _scholarship.SchoolYear
            Dim idx = _cmbStatus.Items.IndexOf(_scholarship.Status)
            If idx >= 0 Then _cmbStatus.SelectedIndex = idx
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim studentId = 0
            If _cmbStudent.SelectedIndex > 0 Then
                studentId = _students(_cmbStudent.SelectedIndex - 1).StudentId
            End If
            If studentId = 0 Then ShowError("Please select a student.") : Return

            Dim model As New ScholarshipModel With {
                .ScholarshipId = If(_scholarship IsNot Nothing, _scholarship.ScholarshipId, 0),
                .ScholarCode   = If(_scholarship IsNot Nothing, _scholarship.ScholarCode, ""),
                .StudentId     = studentId,
                .GrantType     = _txtGrantType.Text.Trim(),
                .Amount        = _nudAmount.Value,
                .SchoolYear    = _txtSchoolYear.Text.Trim(),
                .Status        = _cmbStatus.SelectedItem?.ToString(),
                .CreatedBy     = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
            }

            Dim result = _service.SaveScholarship(model, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

End Namespace
