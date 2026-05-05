Imports System.Drawing
Imports System.Windows.Forms
Imports System.Linq
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Helpers

Namespace BarangaySystem.Forms.Modules

    Public Class EventLogsPanel
        Inherits UserControl
        Implements IRefreshable, ISearchable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _logRepo As New EventLogRepository()

        Private _tabs      As TabControl
        Private _dgvAll    As DataGridView
        Private _dgvLogin  As DataGridView
        Private _cmbType   As ComboBox
        Private _cmbModule As ComboBox

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            ' Event Logs is read-only: only Print and Export are wired
            If Me.Visible Then
                AddHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
                AddHandler _main.btnExport.Click, AddressOf BtnExport_Click
            Else
                RemoveHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
                RemoveHandler _main.btnExport.Click, AddressOf BtnExport_Click
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface

            ' Filter bar
            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top, .Height = 36, .BackColor = UIHelper.Surface
            }
            Dim lblType As New Label With {
                .Text = "Event Type:", .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, 8)
            }
            _cmbType = New ComboBox With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font = New Font("Segoe UI", 8.5F), .Width = 110, .Location = New Point(72, 4)
            }
            _cmbType.Items.AddRange(New String() {"All Types", "LOGIN", "LOGOUT", "INSERT", "UPDATE", "DELETE", "EXPORT", "PRINT"})
            _cmbType.SelectedIndex = 0

            Dim lblMod As New Label With {
                .Text = "Module:", .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.MutedColor, .AutoSize = True, .Location = New Point(194, 8)
            }
            _cmbModule = New ComboBox With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font = New Font("Segoe UI", 8.5F), .Width = 110, .Location = New Point(240, 4)
            }
            _cmbModule.Items.AddRange(New String() {"All Modules", "System", "Residents", "Activities",
                                                     "Ordinances", "Students", "Reports", "Accounts"})
            _cmbModule.SelectedIndex = 0

            Dim btnFilter As New Button With {
                .Text = "Filter", .Font = New Font("Segoe UI", 8.5F),
                .BackColor = UIHelper.BtnSearch, .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat, .Size = New Size(60, 24),
                .Location = New Point(360, 4), .Cursor = Cursors.Hand
            }
            btnFilter.FlatAppearance.BorderSize = 0
            AddHandler btnFilter.Click, Sub(s, e) LoadData()
            pnlFilter.Controls.AddRange({lblType, _cmbType, lblMod, _cmbModule, btnFilter})

            ' Tabs
            _tabs = New TabControl With {.Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9)}

            ' All Logs tab
            Dim tabAll As New TabPage("  All Logs  ")
            _dgvAll = New DataGridView()
            UIHelper.StyleDataGridView(_dgvAll)
            _dgvAll.Dock = DockStyle.Fill
            _dgvAll.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Log ID",      .Name = "LogId",       .Width = 65},
                New DataGridViewTextBoxColumn With {.HeaderText = "Timestamp",   .Name = "LogTime",     .Width = 140},
                New DataGridViewTextBoxColumn With {.HeaderText = "User",        .Name = "Username",    .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Event Type",  .Name = "EventType",   .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "Module",      .Name = "Module",      .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Description", .Name = "Description", .FillWeight = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "IP Address",  .Name = "IpAddress",   .Width = 110}
            )
            tabAll.Controls.Add(_dgvAll)

            ' Login History tab
            Dim tabLogin As New TabPage("  Login History  ")
            _dgvLogin = New DataGridView()
            UIHelper.StyleDataGridView(_dgvLogin)
            _dgvLogin.Dock = DockStyle.Fill
            _dgvLogin.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Log ID",     .Name = "LogId",    .Width = 65},
                New DataGridViewTextBoxColumn With {.HeaderText = "Timestamp",  .Name = "LogTime",  .Width = 140},
                New DataGridViewTextBoxColumn With {.HeaderText = "User",       .Name = "Username", .Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Action",     .Name = "Action",   .Width = 80},
                New DataGridViewTextBoxColumn With {.HeaderText = "IP Address", .Name = "IpAddress",.Width = 110},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",     .Name = "Status",   .Width = 80}
            )
            tabLogin.Controls.Add(_dgvLogin)

            _tabs.TabPages.AddRange({tabAll, tabLogin})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e) LoadData()

            Me.Controls.Add(_tabs)
            Me.Controls.Add(pnlFilter)
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                Dim typeFilter = If(_cmbType.SelectedIndex   <= 0, "", _cmbType.SelectedItem.ToString())
                Dim modFilter  = If(_cmbModule.SelectedIndex <= 0, "", _cmbModule.SelectedItem.ToString())

                If _tabs.SelectedIndex = 0 Then
                    _dgvAll.Rows.Clear()
                    Dim logs = _logRepo.GetRecent(200, typeFilter, modFilter)
                    For Each log In logs
                        Dim rowIdx = _dgvAll.Rows.Add(
                            log.LogId,
                            log.LogTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.Username, log.EventType, log.ModuleName,
                            log.Description, log.IpAddress)
                        ColorEventType(_dgvAll.Rows(rowIdx).Cells("EventType"), log.EventType)
                    Next
                Else
                    _dgvLogin.Rows.Clear()
                    Dim logs = _logRepo.GetRecent(200, "", "System")
                    For Each log In logs.Where(Function(l) l.EventType = "LOGIN" OrElse l.EventType = "LOGOUT")
                        Dim ok = Not log.Description.ToLower().Contains("fail")
                        Dim rowIdx = _dgvLogin.Rows.Add(
                            log.LogId,
                            log.LogTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.Username, log.EventType, log.IpAddress,
                            If(ok, "Success", "Failed"))
                        Dim sc = _dgvLogin.Rows(rowIdx).Cells("Status")
                        If ok Then
                            sc.Style.BackColor = UIHelper.BadgeActiveBg
                            sc.Style.ForeColor = UIHelper.BadgeActiveFg
                        Else
                            sc.Style.BackColor = UIHelper.BadgeInactiveBg
                            sc.Style.ForeColor = UIHelper.BadgeInactiveFg
                        End If
                    Next
                End If
            Catch ex As Exception
                MessageBox.Show("Error loading logs: " & ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Public Sub FilterData(term As String) Implements ISearchable.FilterData
            Dim dgv = If(_tabs.SelectedIndex = 0, _dgvAll, _dgvLogin)
            For Each row As DataGridViewRow In dgv.Rows
                Dim match = row.Cells.Cast(Of DataGridViewCell)().Any(
                    Function(c) c.Value IsNot Nothing AndAlso
                                c.Value.ToString().ToLower().Contains(term.ToLower()))
                row.Visible = match OrElse String.IsNullOrWhiteSpace(term)
            Next
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim dgv = If(_tabs.SelectedIndex = 0, _dgvAll, _dgvLogin)
            PrintHelper.PrintGrid(dgv, "Barangay System - Event Logs")
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Using dlg As New SaveFileDialog With {
                .Title    = "Export Event Logs",
                .Filter   = "Excel Workbook (*.xlsx)|*.xlsx",
                .FileName = "EventLogs_" & DateTime.Now.ToString("yyyyMMdd") & ".xlsx"
            }
                If dlg.ShowDialog() <> DialogResult.OK Then Return
                Try
                    Dim dgv = If(_tabs.SelectedIndex = 0, _dgvAll, _dgvLogin)
                    ExportToExcel(dgv, dlg.FileName)
                    MessageBox.Show("Event logs exported successfully.", "Export Complete",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                    _logRepo.Log("EXPORT", "Event Logs", "Exported event logs to Excel")
                Catch ex As Exception
                    MessageBox.Show("Export error: " & ex.Message, "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Sub ExportToExcel(dgv As DataGridView, filePath As String)
            Using pkg As New OfficeOpenXml.ExcelPackage()
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial
                Dim ws = pkg.Workbook.Worksheets.Add("Event Logs")

                ' Title
                ws.Cells(1, 1, 1, dgv.Columns.Count).Merge = True
                ws.Cells(1, 1).Value = "Barangay System - Event Logs"
                ws.Cells(1, 1).Style.Font.Bold = True
                ws.Cells(1, 1).Style.Font.Size = 13

                ' Column headers
                For c = 0 To dgv.Columns.Count - 1
                    ws.Cells(2, c + 1).Value = dgv.Columns(c).HeaderText
                    With ws.Cells(2, c + 1).Style
                        .Font.Bold = True
                        .Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid
                        .Fill.BackgroundColor.SetColor(UIHelper.TableHead)
                        .Font.Color.SetColor(Color.White)
                    End With
                Next

                ' Data
                For r = 0 To dgv.Rows.Count - 1
                    For c = 0 To dgv.Columns.Count - 1
                        Dim v = dgv.Rows(r).Cells(c).Value
                        ws.Cells(r + 3, c + 1).Value = If(v IsNot Nothing, v.ToString(), "")
                    Next
                Next
                ws.Cells(ws.Dimension.Address).AutoFitColumns()
                pkg.SaveAs(New System.IO.FileInfo(filePath))
            End Using
        End Sub

        Private Sub ColorEventType(cell As DataGridViewCell, eventType As String)
            Select Case eventType
                Case "INSERT"
                    cell.Style.BackColor = UIHelper.BadgeActiveBg
                    cell.Style.ForeColor = UIHelper.BadgeActiveFg
                Case "UPDATE"
                    cell.Style.BackColor = UIHelper.BadgePendingBg
                    cell.Style.ForeColor = UIHelper.BadgePendingFg
                Case "DELETE"
                    cell.Style.BackColor = UIHelper.BadgeInactiveBg
                    cell.Style.ForeColor = UIHelper.BadgeInactiveFg
                Case "LOGIN", "LOGOUT"
                    cell.Style.BackColor = UIHelper.BadgeResolvedBg
                    cell.Style.ForeColor = UIHelper.BadgeResolvedFg
                Case "EXPORT", "PRINT"
                    cell.Style.BackColor = ColorTranslator.FromHtml("#e8d5f5")
                    cell.Style.ForeColor = ColorTranslator.FromHtml("#6c3483")
            End Select
        End Sub

    End Class

End Namespace
