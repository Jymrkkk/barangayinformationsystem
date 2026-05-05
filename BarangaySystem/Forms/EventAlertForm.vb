Imports System.Drawing
Imports System.Windows.Forms
Imports BarangaySystem.Helpers
Imports BarangaySystem.Models

Namespace BarangaySystem.Forms

    ''' <summary>
    ''' Styled popup notification shown when one or more activities are
    ''' scheduled for tomorrow. Appears once per session (or once per hour
    ''' if the user dismisses and the timer fires again).
    ''' </summary>
    Public Class EventAlertForm
        Inherits Form

        Private ReadOnly _activities  As List(Of ActivityModel)
        Private ReadOnly _onNavigate  As Action   ' callback → navigate to Activities module

        Public Sub New(activities As List(Of ActivityModel), onNavigate As Action)
            _activities = activities
            _onNavigate = onNavigate
            BuildUI()
        End Sub

        Private Sub BuildUI()
            Me.Text            = "Upcoming Event Alert"
            Me.FormBorderStyle = FormBorderStyle.None   ' borderless for modern look
            Me.StartPosition   = FormStartPosition.Manual
            Me.BackColor       = Color.White
            Me.Size            = New Size(420, Math.Min(520, 160 + _activities.Count * 90))
            Me.TopMost         = True

            ' Position: bottom-right corner of the screen
            Dim workArea = Screen.PrimaryScreen.WorkingArea
            Me.Location = New Point(workArea.Right - Me.Width - 16,
                                    workArea.Bottom - Me.Height - 16)

            ' ── Drop-shadow border panel ──────────────────────────────────
            Dim pnlOuter As New Panel With {
                .Dock      = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding   = New Padding(0)
            }
            AddHandler pnlOuter.Paint, Sub(s, e)
                ' Draw a subtle border
                Using pen As New Pen(UIHelper.BorderColor, 1)
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlOuter.Width - 1, pnlOuter.Height - 1)
                End Using
            End Sub

            ' ── Header bar ───────────────────────────────────────────────
            Dim pnlHeader As New Panel With {
                .Dock      = DockStyle.Top,
                .Height    = 52,
                .BackColor = UIHelper.NavBg
            }

            Dim picBell As New Label With {
                .Text      = "🔔",
                .Font      = New Font("Segoe UI Emoji", 18),
                .ForeColor = Color.White,
                .AutoSize  = True,
                .Location  = New Point(14, 10)
            }

            Dim lblTitle As New Label With {
                .Text      = "Upcoming Event Alert",
                .Font      = New Font("Segoe UI", 11, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize  = True,
                .Location  = New Point(52, 8)
            }
            Dim lblSub As New Label With {
                .Text      = $"{_activities.Count} event(s) scheduled for tomorrow",
                .Font      = New Font("Segoe UI", 8),
                .ForeColor = ColorTranslator.FromHtml("#FFAAAA"),
                .AutoSize  = True,
                .Location  = New Point(52, 30)
            }

            ' Close (X) button in header
            Dim btnX As New Button With {
                .Text      = "✕",
                .Font      = New Font("Segoe UI", 9, FontStyle.Bold),
                .ForeColor = Color.White,
                .BackColor = Color.Transparent,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(28, 28),
                .Cursor    = Cursors.Hand
            }
            btnX.FlatAppearance.BorderSize         = 0
            btnX.FlatAppearance.MouseOverBackColor = UIHelper.NavActive
            AddHandler btnX.Click, Sub(s, e) Me.Close()
            AddHandler pnlHeader.Resize, Sub(s, e)
                btnX.Location = New Point(pnlHeader.Width - 32, 12)
            End Sub

            pnlHeader.Controls.AddRange({picBell, lblTitle, lblSub, btnX})

            ' ── Scrollable event cards ────────────────────────────────────
            Dim pnlScroll As New Panel With {
                .Dock       = DockStyle.Fill,
                .AutoScroll = True,
                .BackColor  = UIHelper.Surface,
                .Padding    = New Padding(10, 8, 10, 8)
            }

            Dim cardY = 8
            For Each act In _activities
                Dim card = BuildEventCard(act)
                card.Location = New Point(8, cardY)
                pnlScroll.Controls.Add(card)
                cardY += card.Height + 8
            Next

            ' ── Footer buttons ────────────────────────────────────────────
            Dim pnlFooter As New Panel With {
                .Dock      = DockStyle.Bottom,
                .Height    = 46,
                .BackColor = UIHelper.Surface
            }
            AddHandler pnlFooter.Paint, Sub(s, e)
                e.Graphics.DrawLine(New Pen(UIHelper.BorderColor), 0, 0, pnlFooter.Width, 0)
            End Sub

            Dim btnView As New Button With {
                .Text      = "📅 View Activities",
                .Font      = New Font("Segoe UI", 9, FontStyle.Bold),
                .BackColor = UIHelper.NavBg,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(140, 30),
                .Cursor    = Cursors.Hand
            }
            btnView.FlatAppearance.BorderSize = 0
            AddHandler btnView.Click, Sub(s, e)
                Me.Close()
                _onNavigate?.Invoke()
            End Sub

            Dim btnDismiss As New Button With {
                .Text      = "Dismiss",
                .Font      = New Font("Segoe UI", 9),
                .BackColor = UIHelper.BtnPrint,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Size      = New Size(80, 30),
                .Cursor    = Cursors.Hand
            }
            btnDismiss.FlatAppearance.BorderSize = 0
            AddHandler btnDismiss.Click, Sub(s, e) Me.Close()

            AddHandler pnlFooter.Resize, Sub(s, e)
                btnDismiss.Location = New Point(pnlFooter.Width - 92, 8)
                btnView.Location    = New Point(pnlFooter.Width - 240, 8)
            End Sub

            pnlFooter.Controls.AddRange({btnView, btnDismiss})

            ' ── Assemble ─────────────────────────────────────────────────
            pnlOuter.Controls.Add(pnlScroll)
            pnlOuter.Controls.Add(pnlFooter)
            pnlOuter.Controls.Add(pnlHeader)
            Me.Controls.Add(pnlOuter)

            ' Slide-in animation: start transparent, fade in
            Me.Opacity = 0
            Dim fadeTimer As New Timer With {.Interval = 20}
            AddHandler fadeTimer.Tick, Sub(s, e)
                If Me.Opacity < 1.0 Then
                    Me.Opacity = Math.Min(1.0, Me.Opacity + 0.08)
                Else
                    fadeTimer.Stop()
                    fadeTimer.Dispose()
                End If
            End Sub
            AddHandler Me.Shown, Sub(s, e) fadeTimer.Start()

            ' Allow dragging the borderless form
            Dim _dragging  As Boolean = False
            Dim _dragStart As Point
            AddHandler pnlHeader.MouseDown, Sub(s, e)
                _dragging  = True
                _dragStart = e.Location
            End Sub
            AddHandler pnlHeader.MouseMove, Sub(s, e)
                If _dragging Then
                    Me.Location = New Point(Me.Left + e.X - _dragStart.X,
                                            Me.Top  + e.Y - _dragStart.Y)
                End If
            End Sub
            AddHandler pnlHeader.MouseUp, Sub(s, e) _dragging = False
        End Sub

        ''' <summary>Builds a single event card panel for one activity.</summary>
        Private Function BuildEventCard(act As ActivityModel) As Panel
            Dim card As New Panel With {
                .Size      = New Size(390, 80),
                .BackColor = Color.White
            }
            AddHandler card.Paint, Sub(s, e)
                ' Left accent bar
                Using b As New SolidBrush(UIHelper.NavActive)
                    e.Graphics.FillRectangle(b, 0, 0, 4, card.Height)
                End Using
                ' Border
                Using p As New Pen(UIHelper.BorderColor)
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1)
                End Using
            End Sub

            Dim lblName As New Label With {
                .Text      = act.ActivityName,
                .Font      = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .ForeColor = UIHelper.NavBg,
                .AutoSize  = False,
                .Size      = New Size(370, 20),
                .Location  = New Point(12, 8)
            }

            Dim lblDate As New Label With {
                .Text      = $"📅  {act.ActivityDate:MMMM dd, yyyy (dddd)}",
                .Font      = New Font("Segoe UI", 8.5F),
                .ForeColor = UIHelper.BtnSearch,
                .AutoSize  = False,
                .Size      = New Size(370, 16),
                .Location  = New Point(12, 30)
            }

            Dim lblVenue As New Label With {
                .Text      = $"📍  {act.Venue}   |   👤  {act.Organizer}",
                .Font      = New Font("Segoe UI", 8),
                .ForeColor = UIHelper.MutedColor,
                .AutoSize  = False,
                .Size      = New Size(370, 16),
                .Location  = New Point(12, 50)
            }

            card.Controls.AddRange({lblName, lblDate, lblVenue})
            Return card
        End Function

    End Class

End Namespace
