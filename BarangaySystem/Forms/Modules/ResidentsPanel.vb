Imports System.Drawing
Imports System.Windows.Forms
Imports System.Linq
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models
Imports BarangaySystem.Forms.Dialogs

Namespace BarangaySystem.Forms.Modules

    Public Class ResidentsPanel
        Inherits UserControl
        Implements IRefreshable, ISearchable

        Private ReadOnly _main    As MainForm
        Private ReadOnly _service As New ResidentService()
        Private ReadOnly _certRepo As New CertificateRepository()

        Private _tabs         As TabControl
        Private _dgvResidents As DataGridView
        Private _dgvCerts     As DataGridView
        Private _cmbPurok     As ComboBox
        Private _cmbStatus    As ComboBox
        Private _lblCount     As Label

        ' Demographics panels — expanded
        Private _pnlDemoGender   As Panel
        Private _pnlDemoAge      As Panel
        Private _pnlDemoMarital  As Panel
        Private _pnlDemoSpecial  As Panel
        Private _pnlDemoContainer As Panel

        Private _residents As List(Of ResidentModel)

        Public Sub New(main As MainForm)
            _main = main
            InitializeComponent()
            WireToolbar()
        End Sub

        Private Sub InitializeComponent()
            Me.BackColor = UIHelper.Surface

            ' ── Filter bar ───────────────────────────────────────────────
            Dim pnlFilter As New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 36,
                .BackColor = UIHelper.Surface,
                .Padding   = New Padding(0, 4, 0, 4)
            }

            Dim lblPurok As New Label With {
                .Text      = "Purok:",
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(0, 8)
            }
            _cmbPurok = New ComboBox With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font          = New Font("Segoe UI", 8.5F),
                .Width         = 110,
                .Location      = New Point(42, 4)
            }
            _cmbPurok.Items.AddRange({"All Puroks", "Purok 1", "Purok 2", "Purok 3", "Purok 4"})
            _cmbPurok.SelectedIndex = 0

            Dim lblStatus As New Label With {
                .Text      = "Status:",
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(162, 8)
            }
            _cmbStatus = New ComboBox With {
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font          = New Font("Segoe UI", 8.5F),
                .Width         = 100,
                .Location      = New Point(206, 4)
            }
            _cmbStatus.Items.AddRange({"All", "Active", "Inactive"})
            _cmbStatus.SelectedIndex = 0

            Dim btnFilter As New Button With {
                .Text      = "Filter",
                .Font      = New Font("Segoe UI", 8.5F),
                .BackColor = UIHelper.BtnSearch,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(60, 24),
                .Location  = New Point(316, 4),
                .Cursor    = Cursors.Hand
            }
            btnFilter.FlatAppearance.BorderSize = 0
            AddHandler btnFilter.Click, Sub(s, e) LoadData()

            _lblCount = New Label With {
                .Text      = "",
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(390, 8)
            }
            pnlFilter.Controls.AddRange({lblPurok, _cmbPurok, lblStatus, _cmbStatus, btnFilter, _lblCount})

            ' ── Tab control ──────────────────────────────────────────────
            _tabs = New TabControl With {
                .Dock      = DockStyle.Fill,
                .Font      = New Font("Segoe UI", 9)
            }

            ' Tab 1 — Resident List
            Dim tabList As New TabPage("  Resident List  ")
            _dgvResidents = New DataGridView()
            UIHelper.StyleDataGridView(_dgvResidents)
            _dgvResidents.Dock = DockStyle.Fill
            _dgvResidents.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Res. ID",     .Name = "ResCode",     .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Full Name",   .Name = "FullName",    .Width = 160},
                New DataGridViewTextBoxColumn With {.HeaderText = "Age",         .Name = "Age",         .Width = 45},
                New DataGridViewTextBoxColumn With {.HeaderText = "Gender",      .Name = "Gender",      .Width = 65},
                New DataGridViewTextBoxColumn With {.HeaderText = "Civil Status",.Name = "CivilStatus", .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Address",     .Name = "Address",     .Width = 150},
                New DataGridViewTextBoxColumn With {.HeaderText = "Purok",       .Name = "Purok",       .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Contact",     .Name = "ContactNo",   .Width = 100},
                New DataGridViewTextBoxColumn With {.HeaderText = "Status",      .Name = "Status",      .Width = 65}
            )
            ' Double-click a row to view the resident
            AddHandler _dgvResidents.CellDoubleClick, AddressOf DgvResidents_CellDoubleClick
            tabList.Controls.Add(_dgvResidents)

            ' Tab 2 — Demographics
            Dim tabDemo As New TabPage("  Demographics  ")
            tabDemo.BackColor = UIHelper.Surface
            _pnlDemoContainer = New Panel With {
                .Dock = DockStyle.Fill, .AutoScroll = True, .BackColor = UIHelper.Surface,
                .Padding = New Padding(10)
            }
            tabDemo.Controls.Add(_pnlDemoContainer)

            ' Tab 3 — Certificates
            Dim tabCerts As New TabPage("  Certificates  ")
            Dim pnlCerts As New Panel With { .Dock = DockStyle.Fill }

            Dim btnAddCert As New Button With {
                .Text      = "+ Issue Certificate",
                .Font      = New Font("Segoe UI", 8.5F),
                .BackColor = UIHelper.BtnAdd,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(140, 26),
                .Location  = New Point(4, 4),
                .Cursor    = Cursors.Hand
            }
            btnAddCert.FlatAppearance.BorderSize = 0
            AddHandler btnAddCert.Click, Sub(s, e) IssueCertificate()

            _dgvCerts = New DataGridView With { .Location = New Point(0, 36),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right }
            UIHelper.StyleDataGridView(_dgvCerts)
            _dgvCerts.Columns.AddRange(
                New DataGridViewTextBoxColumn With {.HeaderText = "Cert. ID",   .Name = "CertCode",     .Width = 70},
                New DataGridViewTextBoxColumn With {.HeaderText = "Resident",   .Name = "ResidentName", .Width = 160},
                New DataGridViewTextBoxColumn With {.HeaderText = "Type",       .Name = "CertType",     .Width = 160},
                New DataGridViewTextBoxColumn With {.HeaderText = "Date Issued",.Name = "IssuedDate",   .Width = 90},
                New DataGridViewTextBoxColumn With {.HeaderText = "Purpose",    .Name = "Purpose",      .Width = 130},
                New DataGridViewTextBoxColumn With {.HeaderText = "Issued By",  .Name = "IssuedBy",     .Width = 120}
            )
            AddHandler pnlCerts.Resize, Sub(s, e)
                _dgvCerts.Size = New Size(pnlCerts.Width - 4, pnlCerts.Height - 40)
            End Sub
            pnlCerts.Controls.AddRange({btnAddCert, _dgvCerts})
            tabCerts.Controls.Add(pnlCerts)

            _tabs.TabPages.AddRange({tabList, tabDemo, tabCerts})
            AddHandler _tabs.SelectedIndexChanged, Sub(s, e)
                If _tabs.SelectedIndex = 1 Then LoadDemographics()
                If _tabs.SelectedIndex = 2 Then LoadCertificates()
            End Sub

            Me.Controls.Add(_tabs)
            Me.Controls.Add(pnlFilter)
        End Sub

        ' ── Wire toolbar buttons ─────────────────────────────────────────
        Private Sub WireToolbar()
            AddHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
            AddHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
            AddHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
            AddHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
            AddHandler _main.btnExport.Click, AddressOf BtnExport_Click
        End Sub

        Private Sub UnwireToolbar()
            RemoveHandler _main.btnAdd.Click,    AddressOf BtnAdd_Click
            RemoveHandler _main.btnUpdate.Click, AddressOf BtnUpdate_Click
            RemoveHandler _main.btnDelete.Click, AddressOf BtnDelete_Click
            RemoveHandler _main.btnPrint.Click,  AddressOf BtnPrint_Click
            RemoveHandler _main.btnExport.Click, AddressOf BtnExport_Click
        End Sub

        Protected Overrides Sub OnVisibleChanged(e As EventArgs)
            MyBase.OnVisibleChanged(e)
            If Me.Visible Then WireToolbar() Else UnwireToolbar()
        End Sub

        ' ── Load data ────────────────────────────────────────────────────
        Public Sub LoadData() Implements IRefreshable.LoadData
            Try
                Dim purok  = If(_cmbPurok.SelectedIndex  <= 0, "", _cmbPurok.SelectedItem.ToString())
                Dim status = If(_cmbStatus.SelectedIndex <= 0, "", _cmbStatus.SelectedItem.ToString())
                _residents = _service.GetResidents(_main.txtSearch.Text.Trim(), purok, status)
                PopulateGrid(_residents)
            Catch ex As Exception
                MessageBox.Show($"Error loading residents: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub PopulateGrid(list As List(Of ResidentModel))
            _dgvResidents.Rows.Clear()
            For Each r In list
                Dim rowIdx = _dgvResidents.Rows.Add(
                    r.ResCode, r.FullName, r.Age, r.Gender, r.CivilStatus,
                    r.Address, r.Purok, r.ContactNo,
                    If(r.IsActive, "Active", "Inactive"))
                _dgvResidents.Rows(rowIdx).Tag = r.ResidentId
                ' Color status cell
                Dim statusCell = _dgvResidents.Rows(rowIdx).Cells("Status")
                If r.IsActive Then
                    statusCell.Style.ForeColor   = UIHelper.BadgeActiveFg
                    statusCell.Style.BackColor   = UIHelper.BadgeActiveBg
                Else
                    statusCell.Style.ForeColor   = UIHelper.BadgeInactiveFg
                    statusCell.Style.BackColor   = UIHelper.BadgeInactiveBg
                End If
            Next
            _lblCount.Text = $"Showing {list.Count} record(s)"
        End Sub

        Public Sub FilterData(term As String) Implements ISearchable.FilterData
            LoadData()
        End Sub

        ' ── Demographics ─────────────────────────────────────────────────
        Private Sub LoadDemographics()
            If _residents Is Nothing Then LoadData()
            _pnlDemoContainer.Controls.Clear()

            Dim total = _residents.Count
            If total = 0 Then
                _pnlDemoContainer.Controls.Add(New Label With {
                    .Text = "No resident data available.", .AutoSize = True,
                    .Location = New Point(10, 10), .ForeColor = UIHelper.MutedColor
                })
                Return
            End If

            ' ── Compute all stats ─────────────────────────────────────────
            Dim males = 0, females = 0
            Dim youth = 0, adult = 0, senior = 0
            Dim single_ = 0, married = 0, widowed = 0, separated = 0
            Dim voters = 0, active = 0, soloParent = 0

            For Each r In _residents
                If r.Gender = "Male"   Then males   += 1
                If r.Gender = "Female" Then females += 1
                Dim age = r.Age
                If age < 18                   Then youth  += 1
                If age >= 18 AndAlso age < 60 Then adult  += 1
                If age >= 60                  Then senior += 1
                Select Case r.CivilStatus
                    Case "Single"    : single_   += 1
                    Case "Married"   : married   += 1
                    Case "Widowed"   : widowed   += 1
                    Case "Separated" : separated += 1
                End Select
                If r.IsVoter      Then voters     += 1
                If r.IsActive     Then active     += 1
                If r.IsSoloParent Then soloParent += 1
            Next

            Dim cardW = 380
            Dim cardH_2 = 80   ' 2-bar card
            Dim cardH_3 = 110  ' 3-bar card
            Dim cardH_4 = 140  ' 4-bar card
            Dim gap     = 10
            Dim col2x   = cardW + gap + 10
            Dim y       = 10

            ' ── Row 1: Gender | Age Groups ───────────────────────────────
            Dim lblG As New Label With {.Text = "Gender", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(10, y)}
            _pnlDemoContainer.Controls.Add(lblG)
            Dim lblA As New Label With {.Text = "Age Groups", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(col2x, y)}
            _pnlDemoContainer.Controls.Add(lblA)
            y += 20

            _pnlDemoGender = BuildDemoCard(10, y, cardW, cardH_2)
            _pnlDemoAge    = BuildDemoCard(col2x, y, cardW, cardH_3)
            _pnlDemoContainer.Controls.AddRange({_pnlDemoGender, _pnlDemoAge})

            Dim gy = 8
            gy = UIHelper.BuildProgressRow(_pnlDemoGender, gy, $"Male ({males})",    males,   total, UIHelper.NavActive)
            gy = UIHelper.BuildProgressRow(_pnlDemoGender, gy, $"Female ({females})", females, total, UIHelper.BtnSearch)

            Dim ay = 8
            ay = UIHelper.BuildProgressRow(_pnlDemoAge, ay, $"Youth 0-17 ({youth})",  youth,  total, UIHelper.BtnAdd)
            ay = UIHelper.BuildProgressRow(_pnlDemoAge, ay, $"Adult 18-59 ({adult})", adult,  total, UIHelper.NavActive)
            ay = UIHelper.BuildProgressRow(_pnlDemoAge, ay, $"Senior 60+ ({senior})", senior, total, UIHelper.BtnDelete)

            y += cardH_3 + gap + 10

            ' ── Row 2: Marital Status | Special Categories ───────────────
            Dim lblM As New Label With {.Text = "Marital Status", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(10, y)}
            _pnlDemoContainer.Controls.Add(lblM)
            Dim lblS As New Label With {.Text = "Special Categories", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(col2x, y)}
            _pnlDemoContainer.Controls.Add(lblS)
            y += 20

            _pnlDemoMarital = BuildDemoCard(10, y, cardW, cardH_4)
            _pnlDemoSpecial = BuildDemoCard(col2x, y, cardW, cardH_4)
            _pnlDemoContainer.Controls.AddRange({_pnlDemoMarital, _pnlDemoSpecial})

            Dim my = 8
            my = UIHelper.BuildProgressRow(_pnlDemoMarital, my, $"Single ({single_})",     single_,   total, UIHelper.NavActive)
            my = UIHelper.BuildProgressRow(_pnlDemoMarital, my, $"Married ({married})",     married,   total, UIHelper.BtnAdd)
            my = UIHelper.BuildProgressRow(_pnlDemoMarital, my, $"Widowed ({widowed})",     widowed,   total, UIHelper.BtnSearch)
            my = UIHelper.BuildProgressRow(_pnlDemoMarital, my, $"Separated ({separated})", separated, total, UIHelper.BtnDelete)

            Dim sy = 8
            sy = UIHelper.BuildProgressRow(_pnlDemoSpecial, sy, $"Active Voters ({voters})",    voters,     total, UIHelper.NavActive)
            sy = UIHelper.BuildProgressRow(_pnlDemoSpecial, sy, $"Active Residents ({active})", active,     total, UIHelper.BtnExport)
            sy = UIHelper.BuildProgressRow(_pnlDemoSpecial, sy, $"Senior Citizens ({senior})",  senior,     total, UIHelper.BtnSearch)
            sy = UIHelper.BuildProgressRow(_pnlDemoSpecial, sy, $"Solo Parents ({soloParent})", soloParent, total, UIHelper.BtnDelete)

            y += cardH_4 + gap + 10

            ' ── Summary stat cards ────────────────────────────────────────
            Dim lblSum As New Label With {.Text = "Summary", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = UIHelper.NavBg, .AutoSize = True, .Location = New Point(10, y)}
            _pnlDemoContainer.Controls.Add(lblSum)
            y += 20

            Dim flow As New FlowLayoutPanel With {
                .Location      = New Point(10, y),
                .AutoSize      = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents  = True,
                .Width         = cardW * 2 + gap
            }
            flow.Controls.Add(UIHelper.BuildStatCard("Total Residents",  total.ToString("N0"),       "All records",          UIHelper.NavActive))
            flow.Controls.Add(UIHelper.BuildStatCard("Active Residents", active.ToString("N0"),      "Currently active",     UIHelper.BtnExport))
            flow.Controls.Add(UIHelper.BuildStatCard("Active Voters",    voters.ToString("N0"),      "Registered voters",    UIHelper.BtnAdd))
            flow.Controls.Add(UIHelper.BuildStatCard("Senior Citizens",  senior.ToString("N0"),      "Age 60 and above",     UIHelper.BtnSearch))
            flow.Controls.Add(UIHelper.BuildStatCard("Solo Parents",     soloParent.ToString("N0"),  "Solo parent households",UIHelper.BtnDelete))
            _pnlDemoContainer.Controls.Add(flow)
        End Sub

        Private Function BuildDemoCard(x As Integer, y As Integer,
                                       w As Integer, h As Integer) As Panel
            Dim pnl As New Panel With {
                .Location  = New Point(x, y),
                .Size      = New Size(w, h),
                .BackColor = Color.White,
                .Padding   = New Padding(8)
            }
            AddHandler pnl.Paint, Sub(s, e)
                e.Graphics.DrawRectangle(New Pen(UIHelper.BorderColor), 0, 0, pnl.Width - 1, pnl.Height - 1)
            End Sub
            Return pnl
        End Function

        ' ── Certificates ─────────────────────────────────────────────────
        Private Sub LoadCertificates()
            Try
                _dgvCerts.Rows.Clear()
                Dim certs = _certRepo.GetAll()
                For Each c In certs
                    _dgvCerts.Rows.Add(c.CertCode, c.ResidentName, c.CertType,
                                       c.IssuedDate.ToString("MM/dd/yyyy"), c.Purpose, c.IssuedBy)
                Next
            Catch ex As Exception
                MessageBox.Show($"Error loading certificates: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub IssueCertificate()
            Using dlg As New CertificateDialog()
                If dlg.ShowDialog() = DialogResult.OK Then LoadCertificates()
            End Using
        End Sub

        ' ── DGV double-click to view resident ───────────────────────────
        Private Sub DgvResidents_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
            If e.RowIndex < 0 Then Return
            Dim residentId = CInt(_dgvResidents.Rows(e.RowIndex).Tag)
            ViewResident(residentId)
        End Sub

        ' ── Toolbar handlers ─────────────────────────────────────────────
        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Using dlg As New ResidentDialog(Nothing, DialogMode.AddNew)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnUpdate_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvResidents.CurrentRow Is Nothing Then
                MessageBox.Show("Please select a resident to update.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim id = CInt(_dgvResidents.CurrentRow.Tag)
            Dim resident = _service.GetById(id)
            Using dlg As New ResidentDialog(resident, DialogMode.EditExisting)
                If dlg.ShowDialog() = DialogResult.OK Then LoadData()
            End Using
        End Sub

        Private Sub BtnDelete_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            If _dgvResidents.CurrentRow Is Nothing Then
                MessageBox.Show("Please select a resident to delete.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim id   = CInt(_dgvResidents.CurrentRow.Tag)
            Dim name = _dgvResidents.CurrentRow.Cells("FullName").Value?.ToString()
            If MessageBox.Show($"Delete resident '{name}'? This cannot be undone.",
                               "Confirm Delete", MessageBoxButtons.YesNo,
                               MessageBoxIcon.Warning) = DialogResult.Yes Then
                Dim result = _service.DeleteResident(id)
                MessageBox.Show(result.Message, If(result.Success, "Success", "Error"),
                                MessageBoxButtons.OK,
                                If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                If result.Success Then LoadData()
            End If
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            PrintHelper.PrintGrid(_dgvResidents, "Barangay Residents Report")
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            If Not Me.Visible Then Return
            Dim choice = MessageBox.Show(
                "Click YES to export as Excel (.xlsx)" & Environment.NewLine &
                "Click NO  to export as PDF (.pdf)",
                "Choose Export Format",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If choice = DialogResult.Cancel Then Return

            Dim isPdf = (choice = DialogResult.No)
            Dim ext   = If(isPdf, "pdf", "xlsx")
            Dim filt  = If(isPdf, "PDF Document (*.pdf)|*.pdf",
                                  "Excel Workbook (*.xlsx)|*.xlsx")

            Using dlg As New SaveFileDialog With {
                .Title    = "Export Residents",
                .Filter   = filt,
                .FileName = $"Residents_{DateTime.Now:yyyyMMdd}.{ext}"
            }
                If dlg.ShowDialog() = DialogResult.OK Then
                    Dim svc    As New ReportService()
                    Dim result = If(isPdf,
                                   svc.ExportResidentsPdf(dlg.FileName),
                                   svc.ExportResidentsExcel(dlg.FileName))
                    MessageBox.Show(result.Message,
                                    If(result.Success, "Export Complete", "Export Failed"),
                                    MessageBoxButtons.OK,
                                    If(result.Success, MessageBoxIcon.Information, MessageBoxIcon.Error))
                End If
            End Using
        End Sub

        Private Sub ViewResident(id As Integer)
            Dim resident = _service.GetById(id)
            If resident Is Nothing Then Return
            Using dlg As New ResidentDialog(resident, DialogMode.ViewOnly)
                dlg.ShowDialog()
            End Using
        End Sub

    End Class

    ' ── CertificateDialog — defined here to avoid cross-namespace resolution issues ──

    Public Class CertificateDialog
        Inherits Form

        Private ReadOnly _certRepo As New DataAccess.CertificateRepository()
        Private ReadOnly _resRepo  As New DataAccess.ResidentRepository()
        Private ReadOnly _logRepo  As New DataAccess.EventLogRepository()

        Private _cmbResident   As ComboBox
        Private _cmbCertType   As ComboBox
        Private _txtPurpose    As TextBox
        Private _txtIssuedBy   As TextBox
        Private _dtpIssuedDate As DateTimePicker
        Private _txtOrNumber   As TextBox
        Private _nudAmount     As NumericUpDown
        Private _residents     As List(Of Models.ResidentModel)

        Public Sub New()
            Me.Text            = "Issue Certificate"
            Me.Size            = New Size(480, 460)
            Me.StartPosition   = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox     = False
            Me.MinimizeBox     = False
            Me.BackColor       = Helpers.UIHelper.Surface

            Dim pnlHeader As New Panel With {
                .Dock = DockStyle.Top, .Height = 44, .BackColor = Helpers.UIHelper.TitleBar
            }
            Dim lblTitle As New Label With {
                .Text = "Issue Certificate", .Font = New Font("Segoe UI", 11, FontStyle.Bold),
                .ForeColor = Color.White, .AutoSize = True, .Location = New Point(14, 10)
            }
            pnlHeader.Controls.Add(lblTitle)

            Dim pnlBody As New Panel With {
                .Dock = DockStyle.Fill, .AutoScroll = True,
                .Padding = New Padding(16, 12, 16, 8), .BackColor = Color.White
            }

            Dim y = 0
            pnlBody.Controls.Add(New Label With {.Text = "Resident *", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            y += 18
            _cmbResident = New ComboBox With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(420, 26), .Location = New Point(0, y), .DropDownStyle = ComboBoxStyle.DropDownList}
            pnlBody.Controls.Add(_cmbResident)
            y += 32

            pnlBody.Controls.Add(New Label With {.Text = "Certificate Type *", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            y += 18
            _cmbCertType = New ComboBox With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(300, 26), .Location = New Point(0, y), .DropDownStyle = ComboBoxStyle.DropDownList}
            _cmbCertType.Items.AddRange(New String() {"Barangay Clearance", "Certificate of Residency", "Indigency Certificate", "Business Clearance", "Good Moral"})
            _cmbCertType.SelectedIndex = 0
            pnlBody.Controls.Add(_cmbCertType)
            y += 32

            pnlBody.Controls.Add(New Label With {.Text = "Purpose", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            y += 18
            _txtPurpose = New TextBox With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(420, 26), .Location = New Point(0, y), .BorderStyle = BorderStyle.FixedSingle}
            pnlBody.Controls.Add(_txtPurpose)
            y += 32

            pnlBody.Controls.Add(New Label With {.Text = "Issued By", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            y += 18
            _txtIssuedBy = New TextBox With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(200, 26), .Location = New Point(0, y), .BorderStyle = BorderStyle.FixedSingle}
            pnlBody.Controls.Add(_txtIssuedBy)
            y += 32

            pnlBody.Controls.Add(New Label With {.Text = "Date Issued *", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            pnlBody.Controls.Add(New Label With {.Text = "OR Number", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(210, y)})
            y += 18
            _dtpIssuedDate = New DateTimePicker With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(190, 26), .Location = New Point(0, y), .Format = DateTimePickerFormat.Short}
            pnlBody.Controls.Add(_dtpIssuedDate)
            _txtOrNumber = New TextBox With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(190, 26), .Location = New Point(210, y), .BorderStyle = BorderStyle.FixedSingle}
            pnlBody.Controls.Add(_txtOrNumber)
            y += 32

            pnlBody.Controls.Add(New Label With {.Text = "Amount (Php)", .Font = New Font("Segoe UI", 8.5F), .ForeColor = Helpers.UIHelper.MutedColor, .AutoSize = True, .Location = New Point(0, y)})
            y += 18
            _nudAmount = New NumericUpDown With {.Font = New Font("Segoe UI", 9.5F), .Size = New Size(150, 26), .Location = New Point(0, y), .Minimum = 0, .Maximum = 99999, .DecimalPlaces = 2}
            pnlBody.Controls.Add(_nudAmount)

            Dim pnlFooter As New Panel With {.Dock = DockStyle.Bottom, .Height = 46, .BackColor = Helpers.UIHelper.Surface}
            Dim btnSave As New Button With {.Text = "Save", .Font = New Font("Segoe UI", 9, FontStyle.Bold), .BackColor = Helpers.UIHelper.BtnAdd, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Size = New Size(90, 30), .Location = New Point(270, 8), .Cursor = Cursors.Hand}
            btnSave.FlatAppearance.BorderSize = 0
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            Dim btnCancel As New Button With {.Text = "Cancel", .Font = New Font("Segoe UI", 9), .BackColor = Helpers.UIHelper.BtnPrint, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Size = New Size(90, 30), .Location = New Point(368, 8), .Cursor = Cursors.Hand}
            btnCancel.FlatAppearance.BorderSize = 0
            AddHandler btnCancel.Click, Sub(s, e) Me.DialogResult = DialogResult.Cancel
            pnlFooter.Controls.AddRange({btnSave, btnCancel})

            Me.Controls.AddRange({pnlHeader, pnlBody, pnlFooter})
            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel

            LoadResidents()
        End Sub

        Private Sub LoadResidents()
            Try
                _residents = _resRepo.GetAll()
                _cmbResident.Items.Add("-- Select Resident --")
                For Each res In _residents
                    _cmbResident.Items.Add(res.FullName)
                Next
                _cmbResident.SelectedIndex = 0
            Catch
                _residents = New List(Of Models.ResidentModel)()
            End Try
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If _cmbResident.SelectedIndex <= 0 Then
                MessageBox.Show("Please select a resident.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim resident = _residents(_cmbResident.SelectedIndex - 1)
            Dim certType = If(_cmbCertType.SelectedItem IsNot Nothing, _cmbCertType.SelectedItem.ToString(), "")
            Dim model As New Models.CertificateModel With {
                .CertCode   = _certRepo.NextCertCode(),
                .ResidentId = resident.ResidentId,
                .CertType   = certType,
                .Purpose    = _txtPurpose.Text.Trim(),
                .IssuedBy   = _txtIssuedBy.Text.Trim(),
                .IssuedDate = _dtpIssuedDate.Value.Date,
                .OrNumber   = _txtOrNumber.Text.Trim(),
                .Amount     = _nudAmount.Value,
                .CreatedBy  = If(Models.Session.CurrentUser IsNot Nothing, Models.Session.CurrentUser.UserId, 0)
            }
            Try
                If _certRepo.Insert(model) Then
                    _logRepo.Log("INSERT", "Residents", "Issued " & certType & " to " & resident.FullName)
                    Me.DialogResult = DialogResult.OK
                Else
                    MessageBox.Show("Failed to issue certificate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Catch ex As Exception
                MessageBox.Show("Database error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

    End Class

End Namespace
