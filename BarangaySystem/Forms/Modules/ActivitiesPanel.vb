Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models
Imports BarangaySystem.Forms.Dialogs

Namespace BarangaySystem.Forms.Modules

    Public Class ActivitiesPanel
        Inherits UserControl
        Implements IRefreshable, ISearchable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _service As New ActivityService()

        Private _tabs As TabControl
        Private _dgvAll       As DataGridView
        Private _dgvUpcoming  As DataGridView
        Private _dgvCompleted As DataGridView

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface

            _tabs = New TabControl With { .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9) }

            _dgvAll       = BuildDgv()
            _dgvUpcoming  = BuildDgv()
            _dgvCompleted = BuildDgv()

            Dim tabAll       As New TabPage("  All Activities  ")
            Dim tabUpcoming  As New TabPage("  Upcoming  ")
            Dim tabCompleted As New TabPage("  Completed  ")

            tabAll.Controls.Add(_dgvAll)
            tabUpcoming.Controls.Add(_dgvUpcoming)
            tabCompleted.Controls.Add(_dgvCompleted)

            _tabs.TabPages.AddRange({tabAll, tabUpcoming, tabCompleted})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e) LoadData()

            Me.Controls.Add(_tabs)
        End Sub

        Private Function BuildDgv() As DataGridView
            Dim dgv As New DataGridView()
            UIHelper.StyleDataGridView(dgv)
            dgv.Dock = DockStyle.Fill
            dgv.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Act. ID",      .Name = "ActCode",      .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Activity Name",.Name = "ActivityName", .FillWeight = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Date",         .Name = "Date",         .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Venue",        .Name = "Venue",        .Width = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Organizer",    .Name = "Organizer",    .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Participants", .Name = "Participants", .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",       .Name = "Status",       .Width = 80}
            )
            AddHandler dgv.CellDoubleClick, Sub(s, e)
                If e.RowIndex < 0 Then Return
                Dim id = CInt(dgv.Rows(e.RowIndex).Tag)
                EditActivity(id)
            End Sub
            Return dgv
        End Function

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            If Me.Visible Then
                AddHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                AddHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                AddHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
                AddHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
                AddHandler _main.btnExport.Click, AddressOf BtnExport_Click
            Else
                RemoveHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                RemoveHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                RemoveHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
                RemoveHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
                RemoveHandler _main.btnExport.Click, AddressOf BtnExport_Click
            End If
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                Dim statusFilter = ""
                Select Case _tabs.SelectedIndex
                    Case 1 : statusFilter = "Upcoming"
                    Case 2 : statusFilter = "Completed"
                End Select

                Dim dgv  = If(_tabs.SelectedIndex = 0, _dgvAll,
                              If(_tabs.SelectedIndex = 1, _dgvUpcoming, _dgvCompleted))
                Dim list = _service.GetActivities(statusFilter, _main.txtSearch.Text.Trim())
                dgv.Rows.Clear()
                For Each a In list
                    Dim rowIdx = dgv.Rows.Add(a.ActCode, a.ActivityName,
                                              a.ActivityDate.ToString("MM/dd/yyyy"),
                                              a.Venue, a.Organizer, a.Participants, a.Status)
                    dgv.Rows(rowIdx).Tag = a.ActivityId
                    ColorStatusCell(dgv.Rows(rowIdx).Cells("Status"), a.Status)
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading activities: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Public Sub FilterData(term As String) Implements ISearchable.FilterData
            LoadData()
        End Sub

        Private Sub ColorStatusCell(cell As DataGridViewCell, status As String)
            Select Case status
                Case "Upcoming"
                    cell.Style.BackColor = UIHelper.BadgePendingBg
                    cell.Style.ForeColor = UIHelper.BadgePendingFg
                Case "Completed"
                    cell.Style.BackColor = UIHelper.BadgeResolvedBg
                    cell.Style.ForeColor = UIHelper.BadgeResolvedFg
                Case "Cancelled"
                    cell.Style.BackColor = UIHelper.BadgeInactiveBg
                    cell.Style.ForeColor = UIHelper.BadgeInactiveFg
                Case Else
                    cell.Style.BackColor = UIHelper.BadgeActiveBg
                    cell.Style.ForeColor = UIHelper.BadgeActiveFg
            End Select
        End Sub

        Private Function GetActiveDgv() As DataGridView
            Return If(_tabs.SelectedIndex = 0, _dgvAll,
                      If(_tabs.SelectedIndex = 1, _dgvUpcoming, _dgvCompleted))
        End Function

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Using dlg As New ActivityDialog(Nothing, DialogMode.AddNew)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnUpdate_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim dgv = GetActiveDgv()
            If dgv.CurrentRow Is Nothing Then
                MessageBox.Show("Select an activity to update.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            EditActivity(CInt(dgv.CurrentRow.Tag))
        End Sub

        Private Sub EditActivity(id As Integer)
            Dim act = _service.GetById(id)
            Using dlg As New ActivityDialog(act, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim dgv = GetActiveDgv()
            If dgv.CurrentRow Is Nothing Then
                MessageBox.Show("Select an activity to delete.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim id   = CInt(dgv.CurrentRow.Tag)
            Dim name = dgv.CurrentRow.Cells("ActivityName").Value?.ToString()
            If MessageBox.Show($"Delete activity '{name}'?", "Confirm Delete",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                Dim result = _service.Delete(id)
                MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                MessageBoxButtons.OK,
                                If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                If result.Success Then LoadData()
            End If
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim dgv = GetActiveDgv()
            PrintHelper.PrintGrid(dgv, "Barangay Activities Report")
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim choice = MessageBox.Show(
                "Click YES to export as Excel (.xlsx)" & Environment.NewLine &
                "Click NO  to export as PDF (.pdf)",
                "Choose Export Format", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If choice = DialogResult.Cancel Then Return
            Dim isPdf = (choice = DialogResult.No)
            Dim ext   = If(isPdf, "pdf", "xlsx")
            Using dlg As New SaveFileDialog With {
                .Title    = "Export Activities",
                .Filter   = If(isPdf, "PDF Document (*.pdf)|*.pdf", "Excel Workbook (*.xlsx)|*.xlsx"),
                .FileName = $"Activities_{DateTime.Now:yyyyMMdd}.{ext}"
            }
                If dlg.ShowDialog() = DialogResult.OK Then
                    Dim svc    As New ReportService()
                    Dim result = svc.ExportActivitiesExcel(dlg.FileName)
                    If isPdf Then result = svc.ExportOrdinancesPdf(dlg.FileName)  ' fallback
                    MessageBox.Show(result.Message,
                                    If(result.Success, "Export Complete", "Export Failed"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                End If
            End Using
        End Sub

    End Class

End Namespace
