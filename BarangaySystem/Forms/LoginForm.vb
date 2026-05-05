Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.BusinessLogic
Imports BarangaySystem.Helpers

Namespace BarangaySystem.Forms

    Public Class LoginForm
        Inherits Form

        Private txtUsername As TextBox
        Private txtPassword As TextBox
        Private chkShow     As CheckBox
        Private btnLogin    As Button
        Private lblError    As Label

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text            = "Barangay Centralized Information System — Login"
            Me.Size            = New Size(860, 540)
            Me.MinimumSize     = New Size(860, 540)
            Me.StartPosition   = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox     = False
            Me.BackColor       = UIHelper.TitleBar

            ' ── Root TableLayoutPanel: 2 columns ─────────────────────────
            Dim tbl As New TableLayoutPanel With {
                .Dock        = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount    = 1,
                .BackColor   = UIHelper.NavBg
            }
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 380))
            tbl.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            ' ── Left branding panel ──────────────────────────────────────
            Dim pnlLeft As New Panel With {
                .Dock      = DockStyle.Fill,
                .BackColor = UIHelper.NavBg
            }

            ' ── System logo (new — Barangay System logo) ─────────────────
            Dim picLogo As New PictureBox With {
                .Size      = New Size(110, 110),
                .Location  = New Point(135, 60),
                .SizeMode  = PictureBoxSizeMode.Zoom,
                .BackColor = Color.Transparent
            }
            Dim logo = UIHelper.GetLogo()
            If logo IsNot Nothing Then picLogo.Image = logo

            Dim lblSystem As New Label With {
                .Text      = "Barangay" & Environment.NewLine &
                             "Centralized" & Environment.NewLine &
                             "Information System",
                .Font      = New Font("Segoe UI", 18, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize  = False,
                .Size      = New Size(340, 110),
                .Location  = New Point(20, 185),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            Dim lblSub As New Label With {
                .Text      = "Digitizing barangay records for a" & Environment.NewLine &
                             "better-served community.",
                .Font      = New Font("Segoe UI", 10),
                .ForeColor = ColorTranslator.FromHtml("#FFAAAA"),
                .AutoSize  = False,
                .Size      = New Size(340, 50),
                .Location  = New Point(20, 305),
                .TextAlign = ContentAlignment.MiddleCenter
            }

            ' ── Footer: university logo + credit text ─────────────────────
            Dim pnlFooter As New Panel With {
                .Size      = New Size(340, 52),
                .Location  = New Point(20, 430),
                .BackColor = Color.Transparent
            }

            Dim picUniLogo As New PictureBox With {
                .Size      = New Size(36, 36),
                .Location  = New Point(8, 8),
                .SizeMode  = PictureBoxSizeMode.Zoom,
                .BackColor = Color.Transparent
            }
            Dim uniLogo = UIHelper.GetUniversityLogo()
            If uniLogo IsNot Nothing Then picUniLogo.Image = uniLogo

            Dim lblUniCredit As New Label With {
                .Text      = "Developed Final Project" & Environment.NewLine &
                             "v1.0  |  © 2026 Barangay System",
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = ColorTranslator.FromHtml("#CC8888"),
                .AutoSize  = False,
                .Size      = New Size(284, 36),
                .Location  = New Point(50, 8),
                .TextAlign = ContentAlignment.MiddleLeft
            }
            pnlFooter.Controls.AddRange({picUniLogo, lblUniCredit})

            ' Decorative bottom accent line
            AddHandler pnlLeft.Paint, Sub(s, e)
                Using b As New SolidBrush(ColorTranslator.FromHtml("#B22222"))
                    e.Graphics.FillRectangle(b, 0, pnlLeft.Height - 4, pnlLeft.Width, 4)
                End Using
            End Sub

            pnlLeft.Controls.AddRange({picLogo, lblSystem, lblSub, pnlFooter})

            ' ── Right login panel ────────────────────────────────────────
            Dim pnlRight As New Panel With {
                .Dock      = DockStyle.Fill,
                .BackColor = UIHelper.Surface
            }

            ' Card
            Dim pnlCard As New Panel With {
                .Size      = New Size(360, 400),
                .BackColor = Color.White
            }
            AddHandler pnlRight.Resize, Sub(s, e)
                pnlCard.Location = New Point(
                    Math.Max(20, (pnlRight.Width  - pnlCard.Width)  \ 2),
                    Math.Max(20, (pnlRight.Height - pnlCard.Height) \ 2))
            End Sub

            ' Card border + top accent
            AddHandler pnlCard.Paint, Sub(s, e)
                Using p As New Pen(UIHelper.BorderColor)
                    e.Graphics.DrawRectangle(p, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1)
                End Using
                Using b As New SolidBrush(UIHelper.NavActive)
                    e.Graphics.FillRectangle(b, 0, 0, pnlCard.Width, 4)
                End Using
            End Sub

            ' Small logo inside card
            Dim picCardLogo As New PictureBox With {
                .Size     = New Size(48, 48),
                .Location = New Point(156, 18),
                .SizeMode = PictureBoxSizeMode.Zoom,
                .BackColor = Color.Transparent
            }
            If logo IsNot Nothing Then picCardLogo.Image = logo

            Dim lblTitle As New Label With {
                .Text      = "Sign In",
                .Font      = New Font("Segoe UI", 16, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = True,
                .Location  = New Point(130, 72)
            }

            Dim lblUser As New Label With {
                .Text      = "Username",
                .Font      = New Font("Segoe UI", 9),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(30, 118)
            }
            txtUsername = New TextBox With {
                .Font        = New Font("Segoe UI", 10),
                .Size        = New Size(300, 28),
                .Location    = New Point(30, 136),
                .BorderStyle = BorderStyle.FixedSingle,
                .BackColor   = Color.White
            }

            Dim lblPass As New Label With {
                .Text      = "Password",
                .Font      = New Font("Segoe UI", 9),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(30, 174)
            }
            txtPassword = New TextBox With {
                .Font         = New Font("Segoe UI", 10),
                .Size         = New Size(300, 28),
                .Location     = New Point(30, 192),
                .PasswordChar = "●"c,
                .BorderStyle  = BorderStyle.FixedSingle,
                .BackColor    = Color.White
            }

            chkShow = New CheckBox With {
                .Text      = "Show password",
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = True,
                .Location  = New Point(30, 228)
            }
            AddHandler chkShow.CheckedChanged, Sub(s, e)
                txtPassword.PasswordChar = If(chkShow.Checked, ControlChars.NullChar, "●"c)
            End Sub

            btnLogin = New Button With {
                .Text      = "LOGIN",
                .Font      = New Font("Segoe UI", 10, FontStyle.Bold),
                .Size      = New Size(300, 40),
                .Location  = New Point(30, 258),
                .BackColor = UIHelper.NavBg,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Cursor    = Cursors.Hand
            }
            btnLogin.FlatAppearance.BorderSize = 0
            AddHandler btnLogin.Click, AddressOf BtnLogin_Click

            lblError = New Label With {
                .Text      = "",
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.BtnDelete,
                .AutoSize  = False,
                .Size      = New Size(300, 32),
                .Location  = New Point(30, 306),
                .TextAlign = ContentAlignment.MiddleCenter
            }

            Dim lblFooter As New Label With {
                .Text      = "Default: admin / Admin@1234",
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = False,
                .Size      = New Size(300, 18),
                .Location  = New Point(30, 368),
                .TextAlign = ContentAlignment.MiddleCenter
            }

            pnlCard.Controls.AddRange({picCardLogo, lblTitle, lblUser, txtUsername,
                                        lblPass, txtPassword, chkShow,
                                        btnLogin, lblError, lblFooter})

            pnlRight.Controls.Add(pnlCard)

            tbl.Controls.Add(pnlLeft,  0, 0)
            tbl.Controls.Add(pnlRight, 1, 0)
            Me.Controls.Add(tbl)
            Me.AcceptButton = btnLogin

            AddHandler Me.Shown, Sub(s, e)
                pnlCard.Location = New Point(
                    Math.Max(20, (pnlRight.Width  - pnlCard.Width)  \ 2),
                    Math.Max(20, (pnlRight.Height - pnlCard.Height) \ 2))
                txtUsername.Focus()
            End Sub
        End Sub

        Private Sub BtnLogin_Click(sender As Object, e As EventArgs)
            lblError.Text         = ""
            txtUsername.BackColor = Color.White
            txtPassword.BackColor = Color.White

            Dim user = txtUsername.Text.Trim()
            Dim pass = txtPassword.Text

            If String.IsNullOrWhiteSpace(user) Then
                txtUsername.BackColor = ColorTranslator.FromHtml("#fadbd8")
                lblError.Text = "Username is required."
                txtUsername.Focus()
                Return
            End If
            If String.IsNullOrWhiteSpace(pass) Then
                txtPassword.BackColor = ColorTranslator.FromHtml("#fadbd8")
                lblError.Text = "Password is required."
                txtPassword.Focus()
                Return
            End If

            btnLogin.Enabled = False
            btnLogin.Text    = "Signing in..."
            Me.Cursor        = Cursors.WaitCursor

            Try
                Dim result = AuthService.Login(user, pass)
                If result.Success Then
                    Dim main As New MainForm()
                    main.Show()
                    Me.Hide()
                    AddHandler main.FormClosed, Sub(s, ev) Me.Close()
                Else
                    lblError.Text         = result.Message
                    txtUsername.BackColor = ColorTranslator.FromHtml("#fadbd8")
                    txtPassword.BackColor = ColorTranslator.FromHtml("#fadbd8")
                    txtPassword.Clear()
                    txtPassword.Focus()
                End If
            Catch ex As Exception
                lblError.Text = "Connection error. Check DB settings."
            Finally
                btnLogin.Enabled = True
                btnLogin.Text    = "LOGIN"
                Me.Cursor        = Cursors.Default
            End Try
        End Sub

    End Class

End Namespace
