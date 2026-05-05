Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms.Dialogs

    Public Class UserDialog
        Inherits DialogBase

        Private ReadOnly _service As New UserService()
        Private ReadOnly _user    As UserModel

        Private _txtUsername As TextBox
        Private _txtFullName As TextBox
        Private _txtEmail    As TextBox
        Private _cmbRole     As ComboBox
        Private _txtPassword As TextBox
        Private _txtConfirm  As TextBox
        Private _cmbStatus   As ComboBox

        Public Sub New(user As UserModel, mode As DialogMode)
            MyBase.New("User Account", mode, 600, 680)
            _user = user
            BuildForm()
            If user IsNot Nothing Then PopulateFields()
        End Sub

        Private Sub BuildForm()
            Dim half = (InnerW - 16) \ 2
            Dim y    = PY

            AddRow("Username *", y) : y += 20
            _txtUsername = AddTextBox(y, half)
            If Mode = DialogMode.EditExisting Then
                _txtUsername.ReadOnly  = True
                _txtUsername.BackColor = UIHelper.Surface
            End If
            y += ROW

            AddRow("Full Name *", y) : y += 20
            _txtFullName = AddTextBox(y, InnerW) : y += ROW

            AddRow("Email *", y) : y += 20
            _txtEmail = AddTextBox(y, InnerW) : y += ROW

            AddRow("Role *", y) : y += 20
            _cmbRole = AddComboBox(y, {"Admin", "Encoder", "Viewer"}, 220)
            y += ROW

            AddRow("Status *", y) : y += 20
            _cmbStatus = AddComboBox(y, {"Active", "Inactive"}, 220)
            y += ROW

            If Mode = DialogMode.AddNew Then
                AddRow("Password *  (min 8 chars, 1 uppercase, 1 digit)", y) : y += 20
                _txtPassword = AddTextBox(y, InnerW)
                _txtPassword.PasswordChar = "●"c
                y += ROW

                AddRow("Confirm Password *", y) : y += 20
                _txtConfirm = AddTextBox(y, InnerW)
                _txtConfirm.PasswordChar = "●"c
                y += ROW
            Else
                Dim btnPwd As New Button With {
                    .Text      = "Change Password...",
                    .Font      = New Font("Segoe UI", 10),
                    .BackColor = UIHelper.BtnUpdate,
                    .ForeColor = Color.White,
                    .FlatStyle = FlatStyle.Flat,
                    .Size      = New Size(180, 34),
                    .Location  = New Point(PX, y),
                    .Cursor    = Cursors.Hand
                }
                btnPwd.FlatAppearance.BorderSize = 0
                AddHandler btnPwd.Click, Sub(s, e)
                    Using dlg As New ChangePasswordDialog(_user.UserId)
                        dlg.ShowDialog()
                    End Using
                End Sub
                pnlBody.Controls.Add(btnPwd)
                y += 50
            End If

            AddBottomSpacer(y)
            _cmbRole.SelectedIndex   = 0
            _cmbStatus.SelectedIndex = 0
        End Sub

        Private Sub PopulateFields()
            _txtUsername.Text = _user.Username
            _txtFullName.Text = _user.FullName
            _txtEmail.Text    = _user.Email
            Dim ri = _cmbRole.Items.IndexOf(_user.Role)
            If ri >= 0 Then _cmbRole.SelectedIndex = ri
            _cmbStatus.SelectedIndex = If(_user.IsActive, 0, 1)
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Mode = DialogMode.ViewOnly Then Me.DialogResult = DialogResult.Cancel : Return

            Dim plainPwd = ""
            If Mode = DialogMode.AddNew Then
                Dim pwdErr = ValidationHelper.ValidatePassword(_txtPassword.Text, _txtConfirm.Text)
                If pwdErr IsNot Nothing Then ShowError(pwdErr) : Return
                plainPwd = _txtPassword.Text
            End If

            Dim model As New UserModel With {
                .UserId   = If(_user IsNot Nothing, _user.UserId, 0),
                .Username = _txtUsername.Text.Trim(),
                .FullName = _txtFullName.Text.Trim(),
                .Email    = _txtEmail.Text.Trim(),
                .Role     = _cmbRole.SelectedItem?.ToString(),
                .IsActive = (_cmbStatus.SelectedItem?.ToString() = "Active")
            }

            Dim result = _service.SaveUser(model, plainPwd, Mode = DialogMode.AddNew)
            If result.Success Then
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

    Public Class ChangePasswordDialog
        Inherits DialogBase

        Private ReadOnly _service As New UserService()
        Private ReadOnly _userId  As Integer

        Private _txtNew     As TextBox
        Private _txtConfirm As TextBox

        Public Sub New(userId As Integer)
            MyBase.New("Change Password", DialogMode.AddNew, 520, 360)
            _userId = userId
            BuildForm()
        End Sub

        Private Sub BuildForm()
            Dim y = PY
            AddRow("New Password *  (min 8 chars, 1 uppercase, 1 digit)", y) : y += 20
            _txtNew = AddTextBox(y, InnerW)
            _txtNew.PasswordChar = "●"c
            y += ROW

            AddRow("Confirm New Password *", y) : y += 20
            _txtConfirm = AddTextBox(y, InnerW)
            _txtConfirm.PasswordChar = "●"c
            y += ROW

            AddBottomSpacer(y)
        End Sub

        Protected Overrides Sub BtnSave_Click(sender As Object, e As EventArgs)
            Dim err = ValidationHelper.ValidatePassword(_txtNew.Text, _txtConfirm.Text)
            If err IsNot Nothing Then ShowError(err) : Return

            Dim result = _service.ChangePassword(_userId, _txtNew.Text)
            If result.Success Then
                ShowSuccess(result.Message)
                Me.DialogResult = DialogResult.OK
            Else
                ShowError(result.Message)
            End If
        End Sub

    End Class

End Namespace
