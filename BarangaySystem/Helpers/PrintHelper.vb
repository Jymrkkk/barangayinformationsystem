Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Linq
Imports System.Windows.Forms

Namespace BarangaySystem.Helpers

    ''' <summary>
    ''' Prints the contents of a DataGridView using PrintDocument.
    ''' Call PrintGrid(dgv, title) to show the print dialog and print.
    ''' </summary>
    Public Module PrintHelper

        Private _dgv       As DataGridView
        Private _title     As String
        Private _pageIndex As Integer
        Private _rowIndex  As Integer
        Private _colWidths As Integer()
        Private _font      As Font
        Private _headerFont As Font
        Private _titleFont  As Font

        Public Sub PrintGrid(dgv As DataGridView, title As String)
            _dgv        = dgv
            _title      = title
            _pageIndex  = 0
            _rowIndex   = 0
            _font       = New Font("Segoe UI", 8)
            _headerFont = New Font("Segoe UI", 8, FontStyle.Bold)
            _titleFont  = New Font("Segoe UI", 12, FontStyle.Bold)

            ' Calculate column widths proportionally
            Dim totalDgvWidth = dgv.Columns.Cast(Of DataGridViewColumn)().
                                    Where(Function(c) c.Visible).
                                    Sum(Function(c) c.Width)
            _colWidths = dgv.Columns.Cast(Of DataGridViewColumn)().
                             Where(Function(c) c.Visible).
                             Select(Function(c) c.Width).ToArray()

            Using pd As New PrintDocument()
                pd.DefaultPageSettings.Landscape = True
                pd.DefaultPageSettings.Margins   = New Margins(40, 40, 40, 40)
                AddHandler pd.PrintPage, AddressOf OnPrintPage

                Using dlg As New PrintDialog With {.Document = pd, .UseEXDialog = True}
                    If dlg.ShowDialog() = DialogResult.OK Then
                        _rowIndex  = 0
                        _pageIndex = 0
                        pd.Print()
                    End If
                End Using
            End Using
        End Sub

        Private Sub OnPrintPage(sender As Object, e As PrintPageEventArgs)
            Dim g       = e.Graphics
            Dim margins = e.MarginBounds
            Dim x       = CSng(margins.Left)
            Dim y       = CSng(margins.Top)
            Dim pageW   = CSng(margins.Width)
            Dim rowH    = 20.0F

            ' Scale column widths to page width
            Dim visibleCols = _dgv.Columns.Cast(Of DataGridViewColumn)().
                                   Where(Function(c) c.Visible).ToList()
            Dim totalW = visibleCols.Sum(Function(c) c.Width)
            Dim scale  = pageW / totalW

            ' Title
            If _pageIndex = 0 AndAlso _rowIndex = 0 Then
                g.DrawString(_title, _titleFont, Brushes.Black, x, y)
                y += 24
                g.DrawString($"Generated: {DateTime.Now:MMMM dd, yyyy  hh:mm tt}",
                             _font, Brushes.Gray, x, y)
                y += 20
            End If

            ' Column headers
            Dim hx = x
            Using hBrush As New SolidBrush(ColorTranslator.FromHtml("#7B1A1A"))
                g.FillRectangle(hBrush, x, y, pageW, rowH)
            End Using
            For Each col In visibleCols
                Dim cw = col.Width * scale
                g.DrawString(col.HeaderText, _headerFont, Brushes.White,
                             New RectangleF(hx + 2, y + 2, cw - 4, rowH - 4))
                hx += cw
            Next
            y += rowH

            ' Data rows
            Dim altBrush As New SolidBrush(ColorTranslator.FromHtml("#eaf1fb"))
            Dim rowNum   = 0
            While _rowIndex < _dgv.Rows.Count
                Dim row = _dgv.Rows(_rowIndex)
                If row.IsNewRow Then _rowIndex += 1 : Continue While

                If rowNum Mod 2 = 0 Then
                    g.FillRectangle(altBrush, x, y, pageW, rowH)
                End If

                Dim cx = x
                For Each col In visibleCols
                    Dim cw  = col.Width * scale
                    Dim val = If(row.Cells(col.Index).Value IsNot Nothing,
                                 row.Cells(col.Index).Value.ToString(), "")
                    g.DrawString(val, _font, Brushes.Black,
                                 New RectangleF(cx + 2, y + 2, cw - 4, rowH - 4))
                    cx += cw
                Next
                g.DrawLine(Pens.LightGray, x, y + rowH, x + pageW, y + rowH)

                y += rowH
                _rowIndex += 1
                rowNum    += 1

                If y + rowH > margins.Bottom Then
                    e.HasMorePages = True
                    _pageIndex    += 1
                    altBrush.Dispose()
                    Return
                End If
            End While

            ' Footer
            g.DrawString($"Page {_pageIndex + 1}  |  Total rows: {_dgv.Rows.Count}",
                         _font, Brushes.Gray, x, CSng(margins.Bottom) - 14)

            e.HasMorePages = False
            altBrush.Dispose()
        End Sub

    End Module

End Namespace
