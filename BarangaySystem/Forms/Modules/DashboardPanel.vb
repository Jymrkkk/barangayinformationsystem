Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Helpers

Namespace BarangaySystem.Forms.Modules

    Public Class DashboardPanel
        Inherits UserControl
        Implements IRefreshable

        Private ReadOnly _main         As MainForm
        Private ReadOnly _resService   As New ResidentService()
        Private ReadOnly _actService   As New ActivityService()
        Private ReadOnly _ordService   As New OrdinanceService()
        Private ReadOnly _stuService   As New StudentService()
        Private ReadOnly _logRepo      As New EventLogRepository()

        Private _pnlStats    As FlowLayoutPanel
        Private _pnlQuick    As FlowLayoutPanel
        Private _dgvLog      As DataGridView

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface
            Me.Padding   = New Padding(4)

            ' ── Stats row ────────────────────────────────────────────────
            Dim lblStats As New Label With {
                .Text      = "Overview",
                .Font      = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = True,
                .Location  = New Point(0, 0)
            }

            _pnlStats = New FlowLayoutPanel With {
                .AutoSize      = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents  = True,
                .Location      = New Point(0, 22),
                .Height        = 100,
                .Width         = 1000
            }

            ' ── Quick access ─────────────────────────────────────────────
            Dim lblQuick As New Label With {
                .Text      = "Quick Access Modules",
                .Font      = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = True,
                .Location  = New Point(0, 120)
            }

            _pnlQuick = New FlowLayoutPanel With {
                .AutoSize      = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents  = False,
                .Location      = New Point(0, 142),
                .Height        = 80,
                .Width         = 800
            }

            Dim quickItems = {
                ("👥", "Residents",  "residents"),
                ("🎓", "Students",   "students"),
                ("🛡", "Ordinances", "ordinances"),
                ("📅", "Activities", "activities"),
                ("📊", "Reports",    "reports")
            }
            For Each qi In quickItems
                Dim icon = qi.Item1 : Dim name = qi.Item2 : Dim key = qi.Item3
                Dim card As New Panel With {
                    .Size      = New Size(120, 70),
                    .BackColor = Color.White,
                    .Cursor    = Cursors.Hand,
                    .Margin    = New Padding(0, 0, 8, 0)
                }
                AddHandler card.Paint, Sub(s, e)
                    e.Graphics.DrawRectangle(New Pen(UIHelper.BorderColor),
                                             0, 0, card.Width - 1, card.Height - 1)
                End Sub
                Dim lblIcon As New Label With {
                    .Text      = icon,
                    .Font      = New Font("Segoe UI", 18),
                    .AutoSize  = True,
                    .Location  = New Point(44, 6),
                    .BackColor = Color.Transparent
                }
                Dim lblName As New Label With {
                    .Text      = name,
                    .Font      = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                    .ForeColor = UIHelper.TextColor,
                    .AutoSize  = False,
                    .Width     = 120,
                    .TextAlign = ContentAlignment.MiddleCenter,
                    .Location  = New Point(0, 44),
                    .BackColor = Color.Transparent
                }
                card.Controls.AddRange({lblIcon, lblName})
                Dim capturedKey = key
                AddHandler card.Click,     Sub(s, e) _main.ShowModule(capturedKey)
                AddHandler lblIcon.Click,  Sub(s, e) _main.ShowModule(capturedKey)
                AddHandler lblName.Click,  Sub(s, e) _main.ShowModule(capturedKey)
                _pnlQuick.Controls.Add(card)
            Next

            ' ── Recent log ───────────────────────────────────────────────
            Dim lblLog As New Label With {
                .Text      = "Recent Activity Log",
                .Font      = New Font("Segoe UI", 10, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = True,
                .Location  = New Point(0, 232)
            }

            _dgvLog = New DataGridView With {
                .Location = New Point(0, 254),
                .Size     = New Size(900, 200)
            }
            UIHelper.StyleDataGridView(_dgvLog)
            _dgvLog.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Date/Time",  .Name = "LogTime",     .Width = 140},
                New DataGridViewTextBoxColumn With {.HeaderText = "User",       .Name = "Username",    .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Module",     .Name = "Module",      .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Event Type", .Name = "EventType",   .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Details",    .Name = "Description", .FillWeight = 100}
            )

            Me.Controls.AddRange({lblStats, _pnlStats, lblQuick, _pnlQuick, lblLog, _dgvLog})

            AddHandler Me.Resize, Sub(s, e)
                _dgvLog.Width    = Me.Width - 28
                _pnlStats.Width  = Me.Width - 28
                _pnlQuick.Width  = Me.Width - 28
            End Sub
        End Sub

        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                ' Stats
                _pnlStats.Controls.Clear()
                Dim resStats = _resService.GetStats()
                Dim stuStats = _stuService.GetStats()
                Dim ordCount = _ordService.GetTotalOrdinances()
                Dim actCount = _actService.GetUpcomingCount()

                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Total Residents",  resStats.Total.ToString("N0"),  "↑ Records in system",    UIHelper.NavActive))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Active Residents", resStats.Active.ToString("N0"), "Currently active",       UIHelper.BtnExport))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Student Records",  stuStats.Total.ToString("N0"),  $"{stuStats.Scholars} scholars", UIHelper.BtnAdd))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Ordinances",       ordCount.ToString("N0"),        "Total enacted",          UIHelper.BtnDelete))
                _pnlStats.Controls.Add(UIHelper.BuildStatCard("Upcoming Events",  actCount.ToString("N0"),        "Scheduled activities",   UIHelper.BtnSearch))

                ' Recent logs
                _dgvLog.Rows.Clear()
                Dim logs = _logRepo.GetRecent(10)
                For Each log In logs
                    _dgvLog.Rows.Add(
                        log.LogTime.ToString("yyyy-MM-dd HH:mm"),
                        log.Username,
                        log.ModuleName,
                        log.EventType,
                        log.Description)
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

    End Class

End Namespace
