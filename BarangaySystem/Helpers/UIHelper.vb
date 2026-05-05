Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Windows.Forms

Namespace BarangaySystem.Helpers

    Public Module UIHelper

        ' ── Maroon Theme Colors ───────────────────────────────────────────
        ' Primary maroon palette
        Public ReadOnly NavBg       As Color = ColorTranslator.FromHtml("#6B0000")   ' deep maroon sidebar
        Public ReadOnly NavHover    As Color = ColorTranslator.FromHtml("#8B0000")   ' dark red hover
        Public ReadOnly NavActive   As Color = ColorTranslator.FromHtml("#A52A2A")   ' active item
        Public ReadOnly TitleBar    As Color = ColorTranslator.FromHtml("#4A0000")   ' darkest maroon
        Public ReadOnly HeaderBg    As Color = ColorTranslator.FromHtml("#6B0000")   ' header bar
        Public ReadOnly TableHead   As Color = ColorTranslator.FromHtml("#7B1A1A")   ' DGV header
        Public ReadOnly TableAlt    As Color = ColorTranslator.FromHtml("#FFF0F0")   ' alternating row
        Public ReadOnly TableHover  As Color = ColorTranslator.FromHtml("#FFD6D6")   ' row hover
        Public ReadOnly BtnAdd      As Color = ColorTranslator.FromHtml("#2ecc71")   ' green — keep
        Public ReadOnly BtnUpdate   As Color = ColorTranslator.FromHtml("#C0392B")   ' maroon-red update
        Public ReadOnly BtnDelete   As Color = ColorTranslator.FromHtml("#922B21")   ' darker red delete
        Public ReadOnly BtnSearch   As Color = ColorTranslator.FromHtml("#B7950B")   ' gold search
        Public ReadOnly BtnPrint    As Color = ColorTranslator.FromHtml("#7f8c8d")   ' grey print
        Public ReadOnly BtnExport   As Color = ColorTranslator.FromHtml("#27ae60")   ' green export
        Public ReadOnly BtnPurple   As Color = ColorTranslator.FromHtml("#8e44ad")
        Public ReadOnly Surface     As Color = ColorTranslator.FromHtml("#FDF5F5")   ' warm off-white
        Public ReadOnly BorderColor As Color = ColorTranslator.FromHtml("#D4A0A0")   ' muted rose border
        Public ReadOnly TextColor   As Color = ColorTranslator.FromHtml("#1a0000")   ' near-black
        Public ReadOnly MutedColor  As Color = ColorTranslator.FromHtml("#7B4A4A")   ' muted maroon-grey
        Public ReadOnly CardBg      As Color = Color.White

        ' Badge colors
        Public ReadOnly BadgeActiveBg   As Color = ColorTranslator.FromHtml("#d4efdf")
        Public ReadOnly BadgeActiveFg   As Color = ColorTranslator.FromHtml("#1e8449")
        Public ReadOnly BadgeInactiveBg As Color = ColorTranslator.FromHtml("#fadbd8")
        Public ReadOnly BadgeInactiveFg As Color = ColorTranslator.FromHtml("#a93226")
        Public ReadOnly BadgePendingBg  As Color = ColorTranslator.FromHtml("#fdebd0")
        Public ReadOnly BadgePendingFg  As Color = ColorTranslator.FromHtml("#d35400")
        Public ReadOnly BadgeResolvedBg As Color = ColorTranslator.FromHtml("#f5d5d5")
        Public ReadOnly BadgeResolvedFg As Color = ColorTranslator.FromHtml("#6B0000")

        ' ── Fonts ────────────────────────────────────────────────────────
        Public ReadOnly FontNormal  As New Font("Segoe UI", 9)
        Public ReadOnly FontBold    As New Font("Segoe UI", 9, FontStyle.Bold)
        Public ReadOnly FontSmall   As New Font("Segoe UI", 8)
        Public ReadOnly FontTitle   As New Font("Segoe UI", 14, FontStyle.Bold)
        Public ReadOnly FontHeader  As New Font("Segoe UI", 11, FontStyle.Bold)

        ' ── Logo loader ──────────────────────────────────────────────────
        Private _logoCache As Image = Nothing

        Public Function GetLogo() As Image
            If _logoCache IsNot Nothing Then Return _logoCache
            Try
                Dim asm  = Assembly.GetExecutingAssembly()
                Dim name = asm.GetManifestResourceNames().
                               FirstOrDefault(Function(n) n.EndsWith("logo.jpg",
                                              StringComparison.OrdinalIgnoreCase))
                If name IsNot Nothing Then
                    Using stream = asm.GetManifestResourceStream(name)
                        _logoCache = Image.FromStream(stream)
                    End Using
                End If
            Catch
                ' Logo not found — silently ignore
            End Try
            Return _logoCache
        End Function

        ' ── DataGridView Styling ─────────────────────────────────────────
        Public Sub StyleDataGridView(dgv As DataGridView)
            dgv.BackgroundColor                                    = Color.White
            dgv.BorderStyle                                        = BorderStyle.None
            dgv.GridColor                                          = ColorTranslator.FromHtml("#E8C8C8")
            dgv.CellBorderStyle                                    = DataGridViewCellBorderStyle.SingleHorizontal
            dgv.ColumnHeadersBorderStyle                           = DataGridViewHeaderBorderStyle.None
            dgv.ColumnHeadersDefaultCellStyle.BackColor            = TableHead
            dgv.ColumnHeadersDefaultCellStyle.ForeColor            = Color.White
            dgv.ColumnHeadersDefaultCellStyle.Font                 = New Font("Segoe UI", 9, FontStyle.Regular)
            dgv.ColumnHeadersDefaultCellStyle.Padding              = New Padding(6, 4, 6, 4)
            dgv.ColumnHeadersHeight                                = 30
            dgv.ColumnHeadersHeightSizeMode                        = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            dgv.AlternatingRowsDefaultCellStyle.BackColor          = TableAlt
            dgv.DefaultCellStyle.Font                              = New Font("Segoe UI", 9)
            dgv.DefaultCellStyle.ForeColor                         = TextColor
            dgv.DefaultCellStyle.Padding                           = New Padding(4, 2, 4, 2)
            dgv.DefaultCellStyle.SelectionBackColor                = NavActive
            dgv.DefaultCellStyle.SelectionForeColor                = Color.White
            dgv.RowHeadersVisible                                  = False
            dgv.SelectionMode                                      = DataGridViewSelectionMode.FullRowSelect
            dgv.MultiSelect                                        = False
            dgv.ReadOnly                                           = True
            dgv.AutoSizeColumnsMode                                = DataGridViewAutoSizeColumnsMode.Fill
            dgv.AllowUserToAddRows                                 = False
            dgv.AllowUserToDeleteRows                              = False
            dgv.AllowUserToResizeRows                              = False
            dgv.RowTemplate.Height                                 = 28
            dgv.EnableHeadersVisualStyles                          = False
        End Sub

        ' ── Button Styling ───────────────────────────────────────────────
        Public Sub StyleButton(btn As Button, bgColor As Color,
                               Optional fgColor As Color = Nothing)
            If fgColor = Nothing Then fgColor = Color.White
            btn.BackColor   = bgColor
            btn.ForeColor   = fgColor
            btn.FlatStyle   = FlatStyle.Flat
            btn.FlatAppearance.BorderSize  = 0
            btn.Font        = New Font("Segoe UI", 9, FontStyle.Regular)
            btn.Cursor      = Cursors.Hand
            btn.Height      = 28
        End Sub

        ' ── Stat card builder ────────────────────────────────────────────
        Public Function BuildStatCard(label As String, value As String,
                                      sub_ As String, accentColor As Color) As Panel
            Dim card As New Panel With {
                .Size      = New Size(190, 90),
                .BackColor = CardBg,
                .Margin    = New Padding(0, 0, 10, 10)
            }
            AddHandler card.Paint, Sub(s, e)
                e.Graphics.FillRectangle(New SolidBrush(accentColor), 0, 0, 4, card.Height)
                e.Graphics.DrawRectangle(New Pen(BorderColor), 0, 0, card.Width - 1, card.Height - 1)
            End Sub

            Dim lblLabel As New Label With {
                .Text      = label.ToUpper(),
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = MutedColor,
                .AutoSize  = False,
                .Size      = New Size(174, 16),
                .Location  = New Point(12, 8)
            }
            Dim lblValue As New Label With {
                .Text      = value,
                .Font      = New Font("Segoe UI", 22, FontStyle.Bold),
                .ForeColor = TextColor,
                .AutoSize  = False,
                .Size      = New Size(174, 36),
                .Location  = New Point(12, 26)
            }
            Dim lblSub As New Label With {
                .Text      = sub_,
                .Font      = New Font("Segoe UI", 7.5F),
                .ForeColor = NavActive,
                .AutoSize  = False,
                .Size      = New Size(174, 16),
                .Location  = New Point(12, 66)
            }
            card.Controls.AddRange({lblLabel, lblValue, lblSub})
            Return card
        End Function

        ' ── Progress bar row builder ─────────────────────────────────────
        Public Function BuildProgressRow(parent As Panel, yPos As Integer,
                                         labelText As String, value As Integer,
                                         maxValue As Integer, barColor As Color) As Integer
            Dim pct = If(maxValue > 0, CInt(value * 100 / maxValue), 0)

            Dim lblName As New Label With {
                .Text      = labelText,
                .Font      = FontSmall,
                .ForeColor = TextColor,
                .AutoSize  = False,
                .Width     = 160,
                .Location  = New Point(0, yPos)
            }
            Dim lblVal As New Label With {
                .Text      = value.ToString("N0"),
                .Font      = FontSmall,
                .ForeColor = MutedColor,
                .AutoSize  = False,
                .Width     = 60,
                .TextAlign = ContentAlignment.MiddleRight,
                .Location  = New Point(parent.Width - 70, yPos)
            }
            Dim track As New Panel With {
                .BackColor = ColorTranslator.FromHtml("#F0D8D8"),
                .Size      = New Size(parent.Width - 10, 8),
                .Location  = New Point(0, yPos + 18)
            }
            Dim fill As New Panel With {
                .BackColor = barColor,
                .Size      = New Size(CInt(track.Width * pct / 100), 8),
                .Location  = New Point(0, 0)
            }
            track.Controls.Add(fill)
            parent.Controls.AddRange({lblName, lblVal, track})
            Return yPos + 34
        End Function

        ' ── Loading helper ───────────────────────────────────────────────
        Public Sub SetLoading(ctrl As Control, loading As Boolean)
            ctrl.Enabled = Not loading
            ctrl.Cursor  = If(loading, Cursors.WaitCursor, Cursors.Default)
        End Sub

    End Module

End Namespace
