Imports System.Drawing
Imports System.Windows.Forms
Imports System.Linq
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Helpers
Imports BarangaySystem.Forms.Dialogs

Namespace BarangaySystem.Forms.Modules

    Public Class StudentsPanel
        Inherits UserControl
        Implements IRefreshable, ISearchable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _service As New StudentService()

        Private _tabs           As TabControl
        Private _dgvStudents    As DataGridView
        Private _dgvScholarships As DataGridView
        Private _pnlBySchool    As Panel

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface
            _tabs = New TabControl With { .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9) }

            ' Tab 1 — Student List
            Dim tabList As New TabPage("  Student List  ")
            _dgvStudents = New DataGridView()
            UIHelper.StyleDataGridView(_dgvStudents)
            _dgvStudents.Dock = DockStyle.Fill
            _dgvStudents.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Stud. ID",   .Name = "StudCode",   .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Full Name",  .Name = "FullName",   .Width = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "School",     .Name = "SchoolName", .Width = 160},
                New DataGridViewTextBoxColumn With {.HeaderText = "Grade/Year", .Name = "GradeYear",  .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "School Year",.Name = "SchoolYear", .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Purok",      .Name = "Purok",      .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Scholar",    .Name = "Scholar",    .Width = 60},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",     .Name = "Status",     .Width = 70}
            )
            AddHandler _dgvStudents.CellDoubleClick, Sub(s, e)
                If e.RowIndex < 0 Then Return
                EditStudent(CInt(_dgvStudents.Rows(e.RowIndex).Tag))
            End Sub
            tabList.Controls.Add(_dgvStudents)

            ' Tab 2 — By School
            Dim tabSchool As New TabPage("  By School  ")
            tabSchool.BackColor = UIHelper.Surface
            _pnlBySchool = New Panel With { .Dock = DockStyle.Fill, .AutoScroll = True, .Padding = New Padding(12) }
            tabSchool.Controls.Add(_pnlBySchool)

            ' Tab 3 — Scholarships
            Dim tabScholar As New TabPage("  Scholarships  ")
            Dim pnlScholar As New Panel With { .Dock = DockStyle.Fill }

            Dim btnAddScholar As New Button With {
                .Text      = "+ Add Scholarship",
                .Font      = New Font("Segoe UI", 8.5F),
                .BackColor = UIHelper.BtnAdd,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(130, 26),
                .Location  = New Point(4, 4),
                .Cursor    = Cursors.Hand
            }
            btnAddScholar.FlatAppearance.BorderSize = 0
            AddHandler btnAddScholar.Click, Sub(s, e)
                Using dlg As New ScholarshipDialog(Nothing, DialogMode.AddNew)
                    If dlg.ShowDialog() = DialogResult.OK Then LoadScholarships()
                End Using
            End Sub

            _dgvScholarships = New DataGridView With { .Location = New Point(0, 36),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right }
            UIHelper.StyleDataGridView(_dgvScholarships)
            _dgvScholarships.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Scholar ID",  .Name = "ScholarCode", .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Student",     .Name = "StudentName", .Width = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "School",      .Name = "SchoolName",  .Width = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "Grant Type",  .Name = "GrantType",   .Width = 130},
                New DataGridViewTextBoxColumn With {.HeaderText = "Amount",      .Name = "Amount",      .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "School Year", .Name = "SchoolYear",  .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",      .Name = "Status",      .Width = 70}
            )
            AddHandler pnlScholar.Resize, Sub(s, e)
                _dgvScholarships.Size = New Size(pnlScholar.Width - 4, pnlScholar.Height - 40)
            End Sub
            pnlScholar.Controls.AddRange({btnAddScholar, _dgvScholarships})
            tabScholar.Controls.Add(pnlScholar)

            _tabs.TabPages.AddRange({tabList, tabSchool, tabScholar})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e)
                Select Case _tabs.SelectedIndex
                    Case 1 : LoadBySchool()
                    Case 2 : LoadScholarships()
                    Case Else : LoadData()
                End Select
            End Sub
            Me.Controls.Add(_tabs)
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            If Me.Visible Then
                AddHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                AddHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                AddHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
                AddHandler _main.btnExport.Click, AddressOf BtnExport_Click
            Else
                RemoveHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
                RemoveHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
                RemoveHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
                RemoveHandler _main.btnExport.Click, AddressOf BtnExport_Click
            End If
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                _dgvStudents.Rows.Clear()
                Dim list = _service.GetStudents(_main.txtSearch.Text.Trim())
                For Each s In list
                    Dim rowIdx = _dgvStudents.Rows.Add(s.StudCode, s.FullName, s.SchoolName,
                                                        s.GradeYear, s.SchoolYear, s.Purok,
                                                        If(s.IsScholar, "Yes", "No"), s.Status)
                    _dgvStudents.Rows(rowIdx).Tag = s.StudentId
                    Dim statusCell = _dgvStudents.Rows(rowIdx).Cells("Status")
                    If s.Status = "Enrolled" Then
                        statusCell.Style.BackColor = UIHelper.BadgeActiveBg
                        statusCell.Style.ForeColor = UIHelper.BadgeActiveFg
                    ElseIf s.Status = "Dropped" Then
                        statusCell.Style.BackColor = UIHelper.BadgeInactiveBg
                        statusCell.Style.ForeColor = UIHelper.BadgeInactiveFg
                    End If
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Public Sub FilterData(term As String) Implements ISearchable.FilterData
            LoadData()
        End Sub

        Private Sub LoadBySchool()
            Try
                _pnlBySchool.Controls.Clear()
                Dim stats = _service.GetStats()
                Dim maxVal = If(stats.BySchool.Count > 0, stats.BySchool.Max(Function(x) x.Item2), 1)
                Dim colors = {UIHelper.NavActive, UIHelper.BtnAdd, UIHelper.BtnSearch, UIHelper.BtnDelete}
                Dim y = 8
                For i = 0 To stats.BySchool.Count - 1
                    Dim item = stats.BySchool(i)
                    y = UIHelper.BuildProgressRow(_pnlBySchool, y, item.Item1, item.Item2,
                                                  maxVal, colors(i Mod colors.Length))
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading school stats: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub LoadScholarships()
            Try
                _dgvScholarships.Rows.Clear()
                For Each s In _service.GetScholarships()
                    Dim rowIdx = _dgvScholarships.Rows.Add(s.ScholarCode, s.StudentName, s.SchoolName,
                                                            s.GrantType, s.Amount.ToString("C"), s.SchoolYear, s.Status)
                    _dgvScholarships.Rows(rowIdx).Tag = s.ScholarshipId
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading scholarships: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Using dlg As New StudentDialog(Nothing, DialogMode.AddNew)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnUpdate_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvStudents.CurrentRow Is Nothing Then Return
            EditStudent(CInt(_dgvStudents.CurrentRow.Tag))
        End Sub

        Private Sub EditStudent(id As Integer)
            Dim stu = _service.GetById(id)
            Using dlg As New StudentDialog(stu, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvStudents.CurrentRow Is Nothing Then Return
            Dim id   = CInt(_dgvStudents.CurrentRow.Tag)
            Dim name = _dgvStudents.CurrentRow.Cells("FullName").Value?.ToString()
            If MessageBox.Show($"Delete student '{name}'?", "Confirm Delete",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                Dim result = _service.DeleteStudent(id)
                MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                MessageBoxButtons.OK,
                                If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                If result.Success Then LoadData()
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
                .Title    = "Export Students",
                .Filter   = If(isPdf, "PDF Document (*.pdf)|*.pdf", "Excel Workbook (*.xlsx)|*.xlsx"),
                .FileName = $"Students_{DateTime.Now:yyyyMMdd}.{ext}"
            }
                If dlg.ShowDialog() = DialogResult.OK Then
                    Dim svc    As New ReportService()
                    Dim result = svc.ExportStudentsExcel(dlg.FileName)
                    MessageBox.Show(result.Message,
                                    If(result.Success, "Export Complete", "Export Failed"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                End If
            End Using
        End Sub

    End Class

End Namespace
