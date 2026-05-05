Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Forms.Dialogs

Namespace BarangaySystem.Forms.Modules

    Public Class OrdinancesPanel
        Inherits UserControl
        Implements IRefreshable, ISearchable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _service As New OrdinanceService()

        Private _tabs         As TabControl
        Private _dgvOrdinances As DataGridView
        Private _dgvResolutions As DataGridView

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface
            _tabs = New TabControl With { .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9) }

            ' Ordinances tab
            Dim tabOrd As New TabPage("  Ordinance List  ")
            _dgvOrdinances = New DataGridView()
            UIHelper.StyleDataGridView(_dgvOrdinances)
            _dgvOrdinances.Dock = DockStyle.Fill
            _dgvOrdinances.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "BO Number",     .Name = "BoNumber",     .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Introduced By", .Name = "IntroducedBy", .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Description",   .Name = "Description",  .FillWeight = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Date Enacted",  .Name = "DateEnacted",  .Width = 100},
                New DataGridViewTextBoxColumn With {.HeaderText = "Approved By",   .Name = "ApprovedBy",   .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",        .Name = "Status",       .Width = 80}
            )
            AddHandler _dgvOrdinances.CellDoubleClick, Sub(s, e)
                If e.RowIndex < 0 Then Return
                EditOrdinance(CInt(_dgvOrdinances.Rows(e.RowIndex).Tag))
            End Sub
            tabOrd.Controls.Add(_dgvOrdinances)

            ' Resolutions tab
            Dim tabRes As New TabPage("  Resolutions  ")
            _dgvResolutions = New DataGridView()
            UIHelper.StyleDataGridView(_dgvResolutions)
            _dgvResolutions.Dock = DockStyle.Fill
            _dgvResolutions.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Resolution No.", .Name = "ResNumber",  .Width = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Subject",        .Name = "Subject",    .FillWeight = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "Sponsor",        .Name = "Sponsor",    .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Date Passed",    .Name = "DatePassed", .Width = 100},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",         .Name = "Status",     .Width = 80}
            )
            AddHandler _dgvResolutions.CellDoubleClick, Sub(s, e)
                If e.RowIndex < 0 Then Return
                EditResolution(CInt(_dgvResolutions.Rows(e.RowIndex).Tag))
            End Sub
            tabRes.Controls.Add(_dgvResolutions)

            _tabs.TabPages.AddRange({tabOrd, tabRes})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e) LoadData()
            Me.Controls.Add(_tabs)
        End Sub

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
                If _tabs.SelectedIndex = 0 Then
                    _dgvOrdinances.Rows.Clear()
                    For Each o In _service.GetOrdinances("", _main.txtSearch.Text.Trim())
                        Dim rowIdx = _dgvOrdinances.Rows.Add(o.BoNumber, o.IntroducedBy, o.Description,
                                                              o.DateEnacted.ToString("MM/dd/yyyy"), o.ApprovedBy, o.Status)
                        _dgvOrdinances.Rows(rowIdx).Tag = o.OrdinanceId
                        ColorStatus(_dgvOrdinances.Rows(rowIdx).Cells("Status"), o.Status)
                    Next
                Else
                    _dgvResolutions.Rows.Clear()
                    For Each r In _service.GetResolutions("", _main.txtSearch.Text.Trim())
                        Dim rowIdx = _dgvResolutions.Rows.Add(r.ResNumber, r.Subject, r.Sponsor,
                                                               r.DatePassed.ToString("MM/dd/yyyy"), r.Status)
                        _dgvResolutions.Rows(rowIdx).Tag = r.ResolutionId
                        ColorStatus(_dgvResolutions.Rows(rowIdx).Cells("Status"), r.Status)
                    Next
                End If
            Catch ex As Exception
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Public Sub FilterData(term As String) Implements ISearchable.FilterData
            LoadData()
        End Sub

        Private Sub ColorStatus(cell As DataGridViewCell, status As String)
            Select Case status
                Case "Active", "Approved"
                    cell.Style.BackColor = UIHelper.BadgeActiveBg
                    cell.Style.ForeColor = UIHelper.BadgeActiveFg
                Case "Inactive", "Rejected"
                    cell.Style.BackColor = UIHelper.BadgeInactiveBg
                    cell.Style.ForeColor = UIHelper.BadgeInactiveFg
                Case "Pending"
                    cell.Style.BackColor = UIHelper.BadgePendingBg
                    cell.Style.ForeColor = UIHelper.BadgePendingFg
                Case Else
                    cell.Style.BackColor = UIHelper.BadgeResolvedBg
                    cell.Style.ForeColor = UIHelper.BadgeResolvedFg
            End Select
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _tabs.SelectedIndex = 0 Then
                Using dlg As New OrdinanceDialog(Nothing, DialogMode.AddNew)
                    If dlg.ShowDialog() = DialogResult.OK Then LoadData()
                End Using
            Else
                Using dlg As New ResolutionDialog(Nothing, DialogMode.AddNew)
                    If dlg.ShowDialog() = DialogResult.OK Then LoadData()
                End Using
            End If
        End Sub

        Private Sub BtnUpdate_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _tabs.SelectedIndex = 0 Then
                If _dgvOrdinances.CurrentRow Is Nothing Then Return
                EditOrdinance(CInt(_dgvOrdinances.CurrentRow.Tag))
            Else
                If _dgvResolutions.CurrentRow Is Nothing Then Return
                EditResolution(CInt(_dgvResolutions.CurrentRow.Tag))
            End If
        End Sub

        Private Sub EditOrdinance(id As Integer)
            Dim ord = New DataAccess.OrdinanceRepository().GetById(id)
            Using dlg As New OrdinanceDialog(ord, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub EditResolution(id As Integer)
            Dim res = New DataAccess.ResolutionRepository().GetById(id)
            Using dlg As New ResolutionDialog(res, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _tabs.SelectedIndex = 0 Then
                If _dgvOrdinances.CurrentRow Is Nothing Then Return
                Dim id  = CInt(_dgvOrdinances.CurrentRow.Tag)
                Dim num = _dgvOrdinances.CurrentRow.Cells("BoNumber").Value?.ToString()
                If MessageBox.Show($"Delete ordinance '{num}'?", "Confirm Delete",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                    Dim result = _service.DeleteOrdinance(id)
                    MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                    If result.Success Then LoadData()
                End If
            Else
                If _dgvResolutions.CurrentRow Is Nothing Then Return
                Dim id  = CInt(_dgvResolutions.CurrentRow.Tag)
                Dim num = _dgvResolutions.CurrentRow.Cells("ResNumber").Value?.ToString()
                If MessageBox.Show($"Delete resolution '{num}'?", "Confirm Delete",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                    Dim result = _service.DeleteResolution(id)
                    MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                    If result.Success Then LoadData()
                End If
            End If
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _tabs.SelectedIndex = 0 Then
                PrintHelper.PrintGrid(_dgvOrdinances, "Barangay Ordinances Report")
            Else
                PrintHelper.PrintGrid(_dgvResolutions, "Barangay Resolutions Report")
            End If
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
                .Title    = "Export Ordinances",
                .Filter   = If(isPdf, "PDF Document (*.pdf)|*.pdf", "Excel Workbook (*.xlsx)|*.xlsx"),
                .FileName = $"Ordinances_{DateTime.Now:yyyyMMdd}.{ext}"
            }
                If dlg.ShowDialog() = DialogResult.OK Then
                    Dim svc    As New ReportService()
                    Dim result = svc.ExportOrdinancesPdf(dlg.FileName)
                    MessageBox.Show(result.Message,
                                    If(result.Success, "Export Complete", "Export Failed"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                End If
            End Using
        End Sub

    End Class

End Namespace
