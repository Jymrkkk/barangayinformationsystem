Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models
Imports BarangaySystem.Forms.Dialogs

Namespace BarangaySystem.Forms.Modules

    Public Class AccountsPanel
        Inherits UserControl
        Implements IRefreshable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _service As New UserService()

        Private _tabs       As TabControl
        Private _dgvUsers   As DataGridView
        Private _dgvRoles   As DataGridView

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface
            _tabs = New TabControl With { .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9) }

            ' User Accounts tab
            Dim tabUsers As New TabPage("  User Accounts  ")
            _dgvUsers = New DataGridView()
            UIHelper.StyleDataGridView(_dgvUsers)
            _dgvUsers.Dock = DockStyle.Fill
            _dgvUsers.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "User ID",    .Name = "UserId",    .Width = 65},
                New DataGridViewTextBoxColumn With {.HeaderText = "Username",   .Name = "Username",  .Width = 100},
                New DataGridViewTextBoxColumn With {.HeaderText = "Full Name",  .Name = "FullName",  .Width = 160},
                New DataGridViewTextBoxColumn With {.HeaderText = "Role",       .Name = "Role",      .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Email",      .Name = "Email",     .FillWeight = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",     .Name = "Status",    .Width = 70}
            )
            AddHandler _dgvUsers.CellDoubleClick, Sub(s, e)
                If e.RowIndex < 0 Then Return
                EditUser(CInt(_dgvUsers.Rows(e.RowIndex).Tag))
            End Sub
            tabUsers.Controls.Add(_dgvUsers)

            ' Roles & Permissions tab
            Dim tabRoles As New TabPage("  Roles & Permissions  ")
            _dgvRoles = New DataGridView()
            UIHelper.StyleDataGridView(_dgvRoles)
            _dgvRoles.Dock = DockStyle.Fill
            _dgvRoles.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Role",           .Name = "Role",       .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Can Add",        .Name = "CanAdd",     .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Can Edit",       .Name = "CanEdit",    .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Can Delete",     .Name = "CanDelete",  .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Can Export",     .Name = "CanExport",  .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Manage Users",   .Name = "ManageUsers",.Width = 100}
            )
            ' Static permission matrix
            _dgvRoles.Rows.Add("Admin",   "✅", "✅", "✅", "✅", "✅")
            _dgvRoles.Rows.Add("Encoder", "✅", "✅", "❌", "✅", "❌")
            _dgvRoles.Rows.Add("Viewer",  "❌", "❌", "❌", "✅", "❌")
            tabRoles.Controls.Add(_dgvRoles)

            _tabs.TabPages.AddRange({tabUsers, tabRoles})
            Me.Controls.Add(_tabs)
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            If Me.Visible Then
                AddHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                AddHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                AddHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
            Else
                RemoveHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                RemoveHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                RemoveHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
            End If
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                _dgvUsers.Rows.Clear()
                For Each u In _service.GetAll()
                    Dim rowIdx = _dgvUsers.Rows.Add(u.UserId, u.Username, u.FullName, u.Role, u.Email,
                                                     If(u.IsActive, "Active", "Inactive"))
                    _dgvUsers.Rows(rowIdx).Tag = u.UserId
                    Dim roleCell   = _dgvUsers.Rows(rowIdx).Cells("Role")
                    Dim statusCell = _dgvUsers.Rows(rowIdx).Cells("Status")

                    Select Case u.Role
                        Case "Admin"
                            roleCell.Style.BackColor = UIHelper.BadgeResolvedBg
                            roleCell.Style.ForeColor = UIHelper.BadgeResolvedFg
                        Case "Encoder"
                            roleCell.Style.BackColor = UIHelper.BadgePendingBg
                            roleCell.Style.ForeColor = UIHelper.BadgePendingFg
                        Case Else
                            roleCell.Style.BackColor = UIHelper.BadgeInactiveBg
                            roleCell.Style.ForeColor = UIHelper.BadgeInactiveFg
                    End Select

                    If u.IsActive Then
                        statusCell.Style.BackColor = UIHelper.BadgeActiveBg
                        statusCell.Style.ForeColor = UIHelper.BadgeActiveFg
                    Else
                        statusCell.Style.BackColor = UIHelper.BadgeInactiveBg
                        statusCell.Style.ForeColor = UIHelper.BadgeInactiveFg
                    End If
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading accounts: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Using dlg As New UserDialog(Nothing, DialogMode.AddNew)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnUpdate_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvUsers.CurrentRow Is Nothing Then Return
            EditUser(CInt(_dgvUsers.CurrentRow.Tag))
        End Sub

        Private Sub EditUser(id As Integer)
            Dim user = _service.GetById(id)
            Using dlg As New UserDialog(user, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvUsers.CurrentRow Is Nothing Then Return
            Dim id   = CInt(_dgvUsers.CurrentRow.Tag)
            Dim name = _dgvUsers.CurrentRow.Cells("Username").Value?.ToString()
            If MessageBox.Show($"Delete user '{name}'?", "Confirm Delete",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                Dim result = _service.DeleteUser(id)
                MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                MessageBoxButtons.OK,
                                If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                If result.Success Then LoadData()
            End If
        End Sub

    End Class

End Namespace
