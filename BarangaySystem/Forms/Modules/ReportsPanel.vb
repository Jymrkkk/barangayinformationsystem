Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Helpers

Namespace BarangaySystem.Forms.Modules

    Public Class ReportsPanel
        Inherits UserControl
        Implements IRefreshable

        Private ReadOnly _main       As MainForm
        Private ReadOnly _resService As New ResidentService()
        Private ReadOnly _stuService As New StudentService()
        Private ReadOnly _ordService As New OrdinanceService()
        Private ReadOnly _actService As New ActivityService()

        Private _tabs       As TabControl
        Private _pnlPurok   As Panel
        Private _pnlMonthly As Panel
        Private _pnlStats   As FlowLayoutPanel

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            If Me.Visible Then
                AddHandler _main.btnExport.Click, AddressOf BtnExport_Click
                AddHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
            Else
                RemoveHandler _main.btnExport.Click, AddressOf BtnExport_Click
                RemoveHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
            End If
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            MessageBox.Show("Switch to the Export tab and generate a PDF report, then print from your PDF viewer.",
                            "Print Reports", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            ' Show a quick menu of available reports
            Dim reports = New String() {
                "Residents Report (Excel)",
                "Residents Report (PDF)",
                "Students Report (Excel)",
                "Ordinances Report (PDF)",
                "Activities Report (Excel)"
            }
            Using frm As New Form With {
                .Text = "Select Report to Export", .Size = New Size(340, 280),
                .StartPosition = FormStartPosition.CenterParent,
                .FormBorderStyle = FormBorderStyle.FixedDialog,
                .MaximizeBox = False, .MinimizeBox = False
            }
                Dim lst As New ListBox With {
                    .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 10)
                }
                lst.Items.AddRange(reports)
                lst.SelectedIndex = 0

                Dim btnOk As New Button With {
                    .Text = "Export", .Dock = DockStyle.Bottom, .Height = 36,
                    .BackColor = UIHelper.BtnExport, .ForeColor = Color.White,
                    .FlatStyle = FlatStyle.Flat, .DialogResult = DialogResult.OK
                }
                btnOk.FlatAppearance.BorderSize = 0
                frm.Controls.AddRange({lst, btnOk})
                frm.AcceptButton = btnOk

                If frm.ShowDialog() = DialogResult.OK Then
                    Dim sel = lst.SelectedIndex
                    Dim svc As New ReportService()
                    Dim ext = If(sel = 0 OrElse sel = 2 OrElse sel = 4, "xlsx", "pdf")
                    Dim name = {"Residents", "Residents", "Students", "Ordinances", "Activities"}(sel)
                    Using dlg As New SaveFileDialog With {
                        .Title    = $"Export {name} Report",
                        .Filter   = If(ext = "pdf", "PDF Document (*.pdf)|*.pdf", "Excel Workbook (*.xlsx)|*.xlsx"),
                        .FileName = $"{name}_{DateTime.Now:yyyyMMdd}.{ext}"
                    }
                        If dlg.ShowDialog() = DialogResult.OK Then
                            Dim result As (Success As Boolean, Message As String)
                            Select Case sel
                                Case 0 : result = svc.ExportResidentsExcel(dlg.FileName)
                                Case 1 : result = svc.ExportResidentsPdf(dlg.FileName)
                                Case 2 : result = svc.ExportStudentsExcel(dlg.FileName)
                                Case 3 : result = svc.ExportOrdinancesPdf(dlg.FileName)
                                Case 4 : result = svc.ExportActivitiesExcel(dlg.FileName)
                                Case Else : Return
                            End Select
                            MessageBox.Show(result.Message,
                                            If(result.Success, "Export Complete", "Export Failed"),
                                            MessageBoxButtons.OK,
                                            If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                        End If
                    End Using
                End If
            End Using
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface
            _tabs = New TabControl With { .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9) }

            ' ── Overview tab ─────────────────────────────────────────────
            Dim tabOverview As New TabPage("  Overview  ")
            tabOverview.BackColor = UIHelper.Surface
            Dim pnlOv As New Panel With { .Dock = DockStyle.Fill, .AutoScroll = True, .Padding = New Padding(12) }

            ' Stat cards
            _pnlStats = New FlowLayoutPanel With {
                .AutoSize      = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents  = False,
                .Location      = New Point(0, 0),
                .Height        = 90,
                .Width         = 800
            }

            ' Purok chart
            Dim lblPurok As New Label With {
                .Text = "Population by Purok", .Font = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(0, 100)
            }
            _pnlPurok = New Panel With {
                .Location  = New Point(0, 124),
                .Size      = New Size(420, 160),
                .BackColor = Color.White,
                .Padding   = New Padding(12)
            }
            AddHandler _pnlPurok.Paint, Sub(s, e)
                e.Graphics.DrawRectangle(New Pen(UIHelper.BorderColor), 0, 0, _pnlPurok.Width - 1, _pnlPurok.Height - 1)
            End Sub

            ' Monthly chart
            Dim lblMonthly As New Label With {
                .Text = "Monthly Registrations (Current Year)", .Font = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(440, 100)
            }
            _pnlMonthly = New Panel With {
                .Location  = New Point(440, 124),
                .Size      = New Size(420, 160),
                .BackColor = Color.White,
                .Padding   = New Padding(12)
            }
            AddHandler _pnlMonthly.Paint, Sub(s, e)
                e.Graphics.DrawRectangle(New Pen(UIHelper.BorderColor), 0, 0, _pnlMonthly.Width - 1, _pnlMonthly.Height - 1)
            End Sub

            pnlOv.Controls.AddRange({_pnlStats, lblPurok, _pnlPurok, lblMonthly, _pnlMonthly})
            tabOverview.Controls.Add(pnlOv)

            ' ── Export tab ───────────────────────────────────────────────
            Dim tabExport As New TabPage("  Export Reports  ")
            tabExport.BackColor = UIHelper.Surface

            Dim dgvReports As New DataGridView With {
                .Dock = DockStyle.Fill
            }
            UIHelper.StyleDataGridView(dgvReports)
            dgvReports.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Report Name",    .Name = "Name",    .FillWeight = 120},
                New DataGridViewTextBoxColumn With {.HeaderText = "Type",           .Name = "Type",    .Width = 100},
                New DataGridViewTextBoxColumn With {.HeaderText = "Format",         .Name = "Format",  .Width = 100},
                New DataGridViewButtonColumn  With {.HeaderText = "Action",         .Name = "Action",  .Width = 90,
                                                    .Text = "Generate", .UseColumnTextForButtonValue = True}
            )

            ' Populate report list
            Dim reports = {
                ("Monthly Resident Report",    "Demographic",   "PDF / Excel", "residents_excel"),
                ("Resident Report (PDF)",      "Demographic",   "PDF",         "residents_pdf"),
                ("Student Enrollment Report",  "Education",     "Excel",       "students_excel"),
                ("Ordinance Registry",         "Legislative",   "PDF",         "ordinances_pdf"),
                ("Activities Summary",         "Events",        "Excel",       "activities_excel")
            }
            For Each r In reports
                Dim rowIdx = dgvReports.Rows.Add(r.Item1, r.Item2, r.Item3)
                dgvReports.Rows(rowIdx).Tag = r.Item4
            Next

            AddHandler dgvReports.CellClick, Sub(s, e)
                If e.RowIndex < 0 OrElse e.ColumnIndex <> dgvReports.Columns("Action").Index Then Return
                GenerateReport(dgvReports.Rows(e.RowIndex).Tag?.ToString())
            End Sub
            tabExport.Controls.Add(dgvReports)

            _tabs.TabPages.AddRange({tabOverview, tabExport})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e)
                If _tabs.SelectedIndex = 0 Then LoadData()
            End Sub
            Me.Controls.Add(_tabs)
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                ' Stat cards
                _pnlStats.Controls.Clear()
                Dim resStats = _resService.GetStats()
                Dim stuStats = _stuService.GetStats()
                Dim ordCount = _ordService.GetTotalOrdinances()

                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Total Residents",  resStats.Total.ToString("N0"),  "As of today",          UIHelper.NavActive))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Active Residents", resStats.Active.ToString("N0"), "Currently active",     UIHelper.BtnExport))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Scholars",         stuStats.Scholars.ToString("N0"), "Active scholarships", UIHelper.BtnAdd))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Active Ordinances",ordCount.ToString("N0"),         "Total enacted",        UIHelper.BtnDelete))

                ' Purok chart
                _pnlPurok.Controls.Clear()
                Dim purokData = resStats.ByPurok
                Dim maxPurok  = If(purokData.Count > 0, purokData.Values.Max(), 1)
                Dim colors    = {UIHelper.NavActive, UIHelper.BtnAdd, UIHelper.BtnSearch, UIHelper.BtnDelete}
                Dim y = 8 : Dim i = 0
                For Each kv In purokData
                    y = UIHelper.BuildProgressRow(_pnlPurok, y, $"{kv.Key} ({kv.Value:N0})",
                                                  kv.Value, maxPurok, colors(i Mod colors.Length))
                    i += 1
                Next

                ' Monthly chart (count residents created per month this year)
                _pnlMonthly.Controls.Clear()
                Dim monthNames = {"Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"}
                Dim monthCounts = GetMonthlyRegistrations()
                Dim maxMonth = If(monthCounts.Max() > 0, monthCounts.Max(), 1)
                y = 8
                For m = 0 To 11
                    y = UIHelper.BuildProgressRow(_pnlMonthly, y, monthNames(m), monthCounts(m),
                                                  maxMonth, UIHelper.NavActive)
                Next

            Catch ex As Exception
                MessageBox.Show($"Error loading reports: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Function GetMonthlyRegistrations() As Integer()
            Dim counts(11) As Integer
            Try
                Const sql = "SELECT MONTH(created_at) AS m, COUNT(*) AS cnt
                             FROM residents
                             WHERE YEAR(created_at) = YEAR(CURDATE())
                             GROUP BY MONTH(created_at)"
                Using conn = DataAccess.DatabaseConfig.GetConnection()
                Using cmd  = New MySql.Data.MySqlClient.MySqlCommand(sql, conn)
                Using rdr  = cmd.ExecuteReader()
                    While rdr.Read()
                        counts(rdr.GetInt32("m") - 1) = rdr.GetInt32("cnt")
                    End While
                End Using
                End Using
                End Using
            Catch
            End Try
            Return counts
        End Function

        Private Sub GenerateReport(reportKey As String)
            Dim filter = ""
            Dim ext    = ""
            Select Case reportKey
                Case "residents_excel" : filter = "Excel Workbook (*.xlsx)|*.xlsx" : ext = "xlsx"
                Case "residents_pdf"   : filter = "PDF Document (*.pdf)|*.pdf"     : ext = "pdf"
                Case "students_excel"  : filter = "Excel Workbook (*.xlsx)|*.xlsx" : ext = "xlsx"
                Case "ordinances_pdf"  : filter = "PDF Document (*.pdf)|*.pdf"     : ext = "pdf"
                Case "activities_excel": filter = "Excel Workbook (*.xlsx)|*.xlsx" : ext = "xlsx"
                Case Else : Return
            End Select

            Using dlg As New SaveFileDialog With {
                .Title    = "Save Report",
                .Filter   = filter,
                .FileName = $"{reportKey}_{DateTime.Now:yyyyMMdd}.{ext}"
            }
                If dlg.ShowDialog() <> DialogResult.OK Then Return
                Dim svc    As New ReportService()
                Dim result As (Success As Boolean, Message As String)
                Select Case reportKey
                    Case "residents_excel"  : result = svc.ExportResidentsExcel(dlg.FileName)
                    Case "residents_pdf"    : result = svc.ExportResidentsPdf(dlg.FileName)
                    Case "students_excel"   : result = svc.ExportStudentsExcel(dlg.FileName)
                    Case "ordinances_pdf"   : result = svc.ExportOrdinancesPdf(dlg.FileName)
                    Case "activities_excel" : result = svc.ExportActivitiesExcel(dlg.FileName)
                    Case Else : Return
                End Select
                MessageBox.Show(result.Message, If(result.Success, "Export Complete", "Export Failed"),
                                MessageBoxButtons.OK,
                                If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
            End Using
        End Sub

    End Class

End Namespace
