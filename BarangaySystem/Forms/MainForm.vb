Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models
Imports BarangaySystem.Forms.Modules

Namespace BarangaySystem.Forms

    Public Class MainForm
        Inherits Form

        ' ── Layout panels ────────────────────────────────────────────────
        Private pnlSidebar    As Panel
        Private pnlProfile    As Panel
        Private pnlContent    As Panel
        Private pnlHeader     As Panel
        Private pnlToolbar    As Panel
        Private pnlStatusBar  As Panel

        ' ── Header controls ──────────────────────────────────────────────
        Private lblHdrTitle   As Label
        Private lblHdrSub     As Label
        Private lblHdrDate    As Label

        ' ── Toolbar controls ─────────────────────────────────────────────
        Public btnAdd         As Button
        Public btnUpdate      As Button
        Public btnDelete      As Button
        Public btnPrint       As Button
        Public btnExport      As Button
        Public txtSearch      As TextBox
        Public btnSearch      As Button

        ' ── Status bar ───────────────────────────────────────────────────
        Private lblStatusDot  As Label
        Private lblStatusDB   As Label
        Private lblStatusUser As Label
        Private lblStatusMod  As Label
        Private lblStatusTime As Label

        ' ── Nav buttons ──────────────────────────────────────────────────
        Private _navButtons   As New List(Of Button)
        Private _activeNav    As Button

        ' ── Module panels ────────────────────────────────────────────────
        Private _panels       As New Dictionary(Of String, UserControl)
        Private _currentKey   As String = "home"

        ' ── Search debounce timer ────────────────────────────────────────
        Private _searchTimer  As New Timer With {.Interval = 300}

        ' ── Clock timer ──────────────────────────────────────────────────
        Private _clockTimer   As New Timer With {.Interval = 1000}

        ' ── Event alert timer ────────────────────────────────────────────
        ' Fires 3 s after login, then every hour, to check for tomorrow's events.
        Private _alertTimer   As New Timer With {.Interval = 3000}

        Public Sub New()
            InitializeComponent()
            LoadPanels()
            BuildSidebar()
            ApplyRolePermissions()
            ShowModule("home")
            _clockTimer.Start()
            _alertTimer.Start()   ' first check fires 3 s after the form loads
        End Sub

        ' ── Form init ────────────────────────────────────────────────────
        Private Sub InitializeComponent()
            Me.Text            = "Barangay Centralized Information System — v1.0"
            Me.Size            = New Size(1200, 700)
            Me.MinimumSize     = New Size(1024, 600)
            Me.StartPosition   = FormStartPosition.CenterScreen
            Me.BackColor       = UIHelper.Surface
            Me.Font            = New Font("Segoe UI", 9)

            ' ── Sidebar ──────────────────────────────────────────────────
            pnlSidebar = New Panel With {
                .Dock      = DockStyle.Left,
                .Width     = 170,
                .BackColor = UIHelper.NavBg
            }

            ' ── Main area ────────────────────────────────────────────────
            Dim pnlMain As New Panel With { .Dock = DockStyle.Fill }

            ' Header
            pnlHeader = New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 52,
                .BackColor = UIHelper.HeaderBg
            }
            AddHandler pnlHeader.Paint, Sub(s, e)
                e.Graphics.DrawLine(New Pen(UIHelper.NavActive, 2),
                                    0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1)
            End Sub

            lblHdrTitle = New Label With {
                .Font      = New Font("Segoe UI", 13, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize  = True,
                .Location  = New Point(14, 8)
            }
            lblHdrSub = New Label With {
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = ColorTranslator.FromHtml("#a0bce0"),
                .AutoSize  = True,
                .Location  = New Point(14, 30)
            }
            lblHdrDate = New Label With {
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = ColorTranslator.FromHtml("#d0e8ff"),
                .AutoSize  = True,
                .Location  = New Point(900, 18)
            }
            pnlHeader.Controls.AddRange({lblHdrTitle, lblHdrSub, lblHdrDate})

            ' Toolbar
            pnlToolbar = New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 40,
                .BackColor = ColorTranslator.FromHtml("#F5E8E8"),
                .Padding   = New Padding(8, 5, 8, 5)
            }
            AddHandler pnlToolbar.Paint, Sub(s, e)
                e.Graphics.DrawLine(New Pen(UIHelper.BorderColor),
                                    0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1)
            End Sub

            btnAdd    = MakeToolbarBtn("+ Add",     UIHelper.BtnAdd)
            btnUpdate = MakeToolbarBtn("✎ Update",  UIHelper.BtnUpdate)
            btnDelete = MakeToolbarBtn("✕ Delete",  UIHelper.BtnDelete)
            btnPrint  = MakeToolbarBtn("⎙ Print",   UIHelper.BtnPrint)
            btnExport = MakeToolbarBtn("↓ Export",  UIHelper.BtnExport)

            txtSearch = New TextBox With {
                .Font        = New Font("Segoe UI", 9),
                .Width       = 180,
                .BorderStyle = BorderStyle.FixedSingle,
                .PlaceholderText = "Search records..."
            }
            AddHandler txtSearch.TextChanged, Sub(s, e)
                _searchTimer.Stop()
                _searchTimer.Start()
            End Sub

            btnSearch = MakeToolbarBtn("🔍 Search", UIHelper.BtnSearch)

            ' Position toolbar controls
            Dim x = 8
            For Each b In {btnAdd, btnUpdate, btnDelete, btnPrint, btnExport}
                b.Location = New Point(x, 6)
                x += b.Width + 6
            Next
            txtSearch.Location = New Point(pnlToolbar.Width - 260, 8)
            btnSearch.Location = New Point(pnlToolbar.Width - 76, 6)
            AddHandler pnlToolbar.Resize, Sub(s, e)
                txtSearch.Location = New Point(pnlToolbar.Width - 260, 8)
                btnSearch.Location = New Point(pnlToolbar.Width - 76, 6)
            End Sub
            pnlToolbar.Controls.AddRange({btnAdd, btnUpdate, btnDelete,
                                           btnPrint, btnExport, txtSearch, btnSearch})

            ' Content area
            pnlContent = New Panel With {
                .Dock      = DockStyle.Fill,
                .BackColor = UIHelper.Surface,
                .AutoScroll = True,
                .Padding   = New Padding(12)
            }

            ' Status bar
            pnlStatusBar = New Panel With {
                .Dock      = DockStyle.Bottom,
                .Height    = 22,
                .BackColor = ColorTranslator.FromHtml("#F5E0E0")
            }
            AddHandler pnlStatusBar.Paint, Sub(s, e)
                e.Graphics.DrawLine(New Pen(UIHelper.BorderColor),
                                    0, 0, pnlStatusBar.Width, 0)
            End Sub

            lblStatusDot  = MakeStatusLabel("● System Online", ColorTranslator.FromHtml("#27ae60"))
            lblStatusDB   = MakeStatusLabel("DB: Connected",   UIHelper.MutedColor)
            lblStatusUser = MakeStatusLabel($"User: {Session.CurrentUser?.FullName}", UIHelper.MutedColor)
            lblStatusMod  = MakeStatusLabel("Module: Dashboard", UIHelper.MutedColor)
            lblStatusTime = MakeStatusLabel("", UIHelper.MutedColor)

            Dim sx = 10
            For Each lbl In {lblStatusDot, lblStatusDB, lblStatusUser, lblStatusMod, lblStatusTime}
                lbl.Location = New Point(sx, 3)
                sx += lbl.Width + 20
            Next
            pnlStatusBar.Controls.AddRange({lblStatusDot, lblStatusDB,
                                             lblStatusUser, lblStatusMod, lblStatusTime})

            pnlMain.Controls.Add(pnlContent)
            pnlMain.Controls.Add(pnlToolbar)
            pnlMain.Controls.Add(pnlHeader)
            pnlMain.Controls.Add(pnlStatusBar)

            Me.Controls.Add(pnlMain)
            Me.Controls.Add(pnlSidebar)

            ' Timers
            AddHandler _clockTimer.Tick, Sub(s, e)
                lblStatusTime.Text = DateTime.Now.ToString("hh:mm:ss tt")
                lblHdrDate.Text    = DateTime.Now.ToString("MMM dd, yyyy")
            End Sub
            AddHandler _searchTimer.Tick, Sub(s, e)
                _searchTimer.Stop()
                If _panels.ContainsKey(_currentKey) Then
                    Dim panel = _panels(_currentKey)
                    If TypeOf panel Is ISearchable Then
                        DirectCast(panel, ISearchable).FilterData(txtSearch.Text)
                    End If
                End If
            End Sub

            ' Alert timer: first tick = 3 s after login; subsequent = every hour
            AddHandler _alertTimer.Tick, Sub(s, e)
                _alertTimer.Stop()
                _alertTimer.Interval = 60 * 60 * 1000   ' switch to 1-hour interval
                CheckAndShowEventAlerts()
                _alertTimer.Start()
            End Sub

            AddHandler Me.FormClosing, Sub(s, e) AuthService.Logout()
        End Sub

        ' ── Load all module panels ────────────────────────────────────────
        Private Sub LoadPanels()
            _panels("home")       = New DashboardPanel(Me)
            _panels("residents")  = New ResidentsPanel(Me)
            _panels("activities") = New ActivitiesPanel(Me)
            _panels("ordinances") = New OrdinancesPanel(Me)
            _panels("students")   = New StudentsPanel(Me)
            _panels("reports")    = New ReportsPanel(Me)
            _panels("eventlogs")  = New EventLogsPanel(Me)
            _panels("accounts")   = New AccountsPanel(Me)

            For Each p In _panels.Values
                p.Dock    = DockStyle.Fill
                p.Visible = False
                pnlContent.Controls.Add(p)
            Next
        End Sub

        ' ── Build sidebar nav ────────────────────────────────────────────
        Private Sub BuildSidebar()
            ' Profile section
            pnlProfile = New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 90,
                .BackColor = UIHelper.TitleBar,
                .Padding   = New Padding(10)
            }

            ' Logo
            Dim picLogo As New PictureBox With {
                .Size     = New Size(44, 44),
                .Location = New Point(10, 10),
                .SizeMode = PictureBoxSizeMode.Zoom,
                .BackColor = Color.Transparent
            }
            Dim logo = UIHelper.GetLogo()
            If logo IsNot Nothing Then picLogo.Image = logo

            Dim lblName As New Label With {
                .Text      = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.FullName, ""),
                .Font      = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .ForeColor = ColorTranslator.FromHtml("#FFD0D0"),
                .AutoSize  = False,
                .Size      = New Size(106, 18),
                .Location  = New Point(60, 14)
            }
            Dim lblRole As New Label With {
                .Text      = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.Role, ""),
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = ColorTranslator.FromHtml("#CC8888"),
                .AutoSize  = False,
                .Size      = New Size(106, 16),
                .Location  = New Point(60, 34)
            }
            Dim lblBrgy As New Label With {
                .Text      = "Barangay System",
                .Font      = New Font("Segoe UI", 7F, FontStyle.Italic),
                .ForeColor = ColorTranslator.FromHtml("#AA6666"),
                .AutoSize  = False,
                .Size      = New Size(106, 14),
                .Location  = New Point(60, 54)
            }
            pnlProfile.Controls.AddRange({picLogo, lblName, lblRole, lblBrgy})
            pnlSidebar.Controls.Add(pnlProfile)

            ' Nav items
            Dim navItems = {
                ("home",       "🏠",  "Home"),
                ("",           "",    "── RECORDS ──"),
                ("residents",  "👥",  "Residents Info"),
                ("activities", "📅",  "Activities"),
                ("ordinances", "🛡",  "Ordinances"),
                ("students",   "🎓",  "Student Records"),
                ("",           "",    "── MANAGEMENT ──"),
                ("reports",    "📊",  "Reports"),
                ("eventlogs",  "🕐",  "Event Logs"),
                ("accounts",   "👤",  "Account Mgmt")
            }

            Dim yPos = pnlProfile.Height
            For Each item In navItems
                If String.IsNullOrEmpty(item.Item1) Then
                    ' Section label
                    Dim lbl As New Label With {
                        .Text      = item.Item3,
                        .Font      = New Font("Segoe UI", 7.5F),
                        .ForeColor = ColorTranslator.FromHtml("#CC8888"),
                        .AutoSize  = False,
                        .Size      = New Size(170, 22),
                        .Location  = New Point(0, yPos),
                        .Padding   = New Padding(12, 4, 0, 0)
                    }
                    pnlSidebar.Controls.Add(lbl)
                    yPos += 22
                Else
                    Dim key  = item.Item1
                    Dim icon = item.Item2
                    Dim name = item.Item3

                    ' Hide Accounts for non-admin
                    If key = "accounts" AndAlso
                       (Session.CurrentUser Is Nothing OrElse Session.CurrentUser.Role <> "Admin") Then
                        Continue For
                    End If

                    Dim btn As New Button With {
                        .Text      = $"  {icon}  {name}",
                        .TextAlign = ContentAlignment.MiddleLeft,
                        .Font      = New Font("Segoe UI", 9.5F),
                        .ForeColor = ColorTranslator.FromHtml("#FFCCCC"),
                        .BackColor = UIHelper.NavBg,
                        .FlatStyle = FlatStyle.Flat,
                        .Size      = New Size(170, 34),
                        .Location  = New Point(0, yPos),
                        .Cursor    = Cursors.Hand,
                        .Tag       = key
                    }
                    btn.FlatAppearance.BorderSize         = 0
                    btn.FlatAppearance.MouseOverBackColor = UIHelper.NavHover
                    AddHandler btn.Click, Sub(s, e)
                        ShowModule(DirectCast(DirectCast(s, Button).Tag, String))
                    End Sub
                    AddHandler btn.MouseEnter, Sub(s, e)
                        If DirectCast(s, Button) IsNot _activeNav Then
                            DirectCast(s, Button).ForeColor = Color.White
                        End If
                    End Sub
                    AddHandler btn.MouseLeave, Sub(s, e)
                        If DirectCast(s, Button) IsNot _activeNav Then
                            DirectCast(s, Button).ForeColor = ColorTranslator.FromHtml("#FFCCCC")
                        End If
                    End Sub
                    _navButtons.Add(btn)
                    pnlSidebar.Controls.Add(btn)
                    yPos += 34
                End If
            Next

            ' Logout button at bottom
            Dim btnLogout As New Button With {
                .Text      = "  🚪  Logout",
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font      = New Font("Segoe UI", 9.5F),
                .ForeColor = ColorTranslator.FromHtml("#e74c3c"),
                .BackColor = UIHelper.NavBg,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(170, 34),
                .Dock      = DockStyle.Bottom,
                .Cursor    = Cursors.Hand
            }
            btnLogout.FlatAppearance.BorderSize = 0
            AddHandler btnLogout.Click, Sub(s, e)
                If MessageBox.Show("Are you sure you want to logout?", "Logout",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    AuthService.Logout()
                    Me.Close()
                End If
            End Sub
            pnlSidebar.Controls.Add(btnLogout)
        End Sub

        ' ── Show module ──────────────────────────────────────────────────
        Public Sub ShowModule(key As String)
            If Not _panels.ContainsKey(key) Then key = "home"
            _currentKey = key

            For Each p In _panels.Values : p.Visible = False : Next
            _panels(key).Visible = True

            ' Refresh data on activation
            If TypeOf _panels(key) Is IRefreshable Then
                DirectCast(_panels(key), IRefreshable).LoadData()
            End If

            ' Update nav highlight
            For Each btn In _navButtons
                If btn.Tag?.ToString() = key Then
                    btn.BackColor = UIHelper.NavActive
                    btn.ForeColor = Color.White
                    _activeNav    = btn
                Else
                    btn.BackColor = UIHelper.NavBg
                    btn.ForeColor = ColorTranslator.FromHtml("#FFCCCC")
                End If
            Next

            ' Update header
            Dim titles = GetModuleTitles(key)
            lblHdrTitle.Text   = titles.Title
            lblHdrSub.Text     = titles.Sub_
            lblStatusMod.Text  = $"Module: {titles.Label}"
            txtSearch.Text     = ""

            ApplyRolePermissions()
        End Sub

        Private Function GetModuleTitles(key As String) As (Title As String, Sub_ As String, Label As String)
            Select Case key
                Case "home"       : Return ("Dashboard — Overview",       "Barangay Centralized Information System", "Dashboard")
                Case "residents"  : Return ("Residents Information",       "Records of Barangay Residents",           "Residents")
                Case "activities" : Return ("Barangay Activities",         "Records of Barangay Activities",          "Activities")
                Case "ordinances" : Return ("Barangay Ordinances",         "Records of Barangay Ordinances",          "Ordinances")
                Case "students"   : Return ("Student Records",             "Barangay Student Information",            "Students")
                Case "reports"    : Return ("Reports & Analytics",         "Automated Report Generation",             "Reports")
                Case "eventlogs"  : Return ("Event Logs",                  "System Audit Trail",                      "Event Logs")
                Case "accounts"   : Return ("Account Management",          "User & Access Management",                "Accounts")
                Case Else         : Return ("Dashboard — Overview",        "Barangay Centralized Information System", "Dashboard")
            End Select
        End Function

        ' ── Role-based toolbar permissions ───────────────────────────────
        Public Sub ApplyRolePermissions()
            Dim role      = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.Role, "Viewer")
            Dim canWrite  = (role = "Admin" OrElse role = "Encoder")
            Dim canDelete = (role = "Admin")

            ' Modules where Add/Update/Delete make no sense — hide them entirely
            Dim hideEditBtns = (_currentKey = "home" OrElse
                                _currentKey = "eventlogs" OrElse
                                _currentKey = "reports")

            btnAdd.Visible    = Not hideEditBtns
            btnUpdate.Visible = Not hideEditBtns
            btnDelete.Visible = Not hideEditBtns

            ' For modules that show the buttons, apply role-based enable/disable
            If Not hideEditBtns Then
                btnAdd.Enabled    = canWrite
                btnUpdate.Enabled = canWrite
                btnDelete.Enabled = canDelete
            End If

            btnPrint.Enabled  = True
            btnExport.Enabled = True
        End Sub

        ' ── Toolbar button factory ────────────────────────────────────────
        Private Function MakeToolbarBtn(text As String, color As Color) As Button
            Dim btn As New Button With {
                .Text      = text,
                .BackColor = color,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Font      = New Font("Segoe UI", 8.5F),
                .Size      = New Size(80, 28),
                .Cursor    = Cursors.Hand
            }
            btn.FlatAppearance.BorderSize = 0
            Return btn
        End Function

        Private Function MakeStatusLabel(text As String, color As Color) As Label
            Return New Label With {
                .Text      = text,
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = color,
                .AutoSize  = False,
                .Width     = 130,
                .Height    = 16
            }
        End Function

        ' ── Event alert check ────────────────────────────────────────────
        ''' <summary>
        ''' Queries for Upcoming activities scheduled for tomorrow.
        ''' If any are found, shows the EventAlertForm popup.
        ''' Safe to call on the UI thread (timer tick runs on UI thread).
        ''' </summary>
        Private Sub CheckAndShowEventAlerts()
            Try
                Dim svc      As New ActivityService()
                Dim tomorrow = svc.GetTomorrowAlerts()
                If tomorrow.Count = 0 Then Return

                Dim alert As New EventAlertForm(tomorrow,
                    Sub() ShowModule("activities"))
                alert.Show(Me)   ' non-modal so the main form stays usable
            Catch
                ' Silently ignore DB errors during alert check
            End Try
        End Sub

    End Class

    ' ── Interfaces for panel contracts ───────────────────────────────────

    Public Interface IRefreshable
        Sub LoadData()
    End Interface

    Public Interface ISearchable
        Sub FilterData(term As String)
    End Interface

End Namespace
