Imports System.IO
Imports BarangaySystem.DataAccess
Imports BarangaySystem.Models
Imports iText.Kernel.Pdf
Imports iText.Layout
Imports iText.Layout.Element
Imports iText.Layout.Properties
Imports iText.Kernel.Colors
Imports OfficeOpenXml
Imports OfficeOpenXml.Style

Namespace BarangaySystem.BusinessLogic

    Public Class ReportService

        Private ReadOnly _resRepo  As New ResidentRepository()
        Private ReadOnly _stuRepo  As New StudentRepository()
        Private ReadOnly _ordRepo  As New OrdinanceRepository()
        Private ReadOnly _actRepo  As New ActivityRepository()
        Private ReadOnly _logRepo  As New EventLogRepository()

        ' ── PDF Helpers ──────────────────────────────────────────────────

        Private Function CreatePdfDocument(filePath As String) As Document
            Dim writer   As New PdfWriter(filePath)
            Dim pdf      As New PdfDocument(writer)
            Dim doc      As New Document(pdf, iText.Kernel.Geom.PageSize.A4)
            doc.SetMargins(36, 36, 36, 36)
            Return doc
        End Function

        Private Sub AddPdfHeader(doc As Document, title As String, subtitle As String)
            Dim headerColor = New DeviceRgb(26, 58, 107)
            Dim titlePara = New Paragraph(title) _
                .SetFontSize(16).SetBold() _
                .SetFontColor(ColorConstants.WHITE) _
                .SetBackgroundColor(headerColor) _
                .SetPadding(8).SetTextAlignment(TextAlignment.CENTER)
            doc.Add(titlePara)

            Dim generatedBy = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.FullName, "System")
            Dim subText     = subtitle & "   |   Generated: " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt") &
                              "   |   By: " & generatedBy
            Dim subPara = New Paragraph(subText) _
                .SetFontSize(8).SetFontColor(ColorConstants.GRAY) _
                .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(10)
            doc.Add(subPara)
        End Sub

        Private Function MakePdfTable(headers As String()) As Table
            Dim tbl    As New Table(UnitValue.CreatePercentArray(headers.Length))
            tbl.UseAllAvailableWidth()
            Dim headBg = New DeviceRgb(44, 82, 130)
            Dim white  = ColorConstants.WHITE
            For Each h In headers
                Dim cellPara = New Paragraph(h).SetBold().SetFontSize(9).SetFontColor(white)
                Dim cell     = New Cell().Add(cellPara)
                cell.SetBackgroundColor(headBg).SetPadding(4)
                tbl.AddHeaderCell(cell)
            Next
            Return tbl
        End Function

        Private Sub AddPdfRow(tbl As Table, values As String(), rowIndex As Integer)
            Dim altBg = New DeviceRgb(234, 241, 251)
            For Each v In values
                Dim cell = New Cell().Add(New Paragraph(v).SetFontSize(8)).SetPadding(3)
                If rowIndex Mod 2 = 0 Then cell.SetBackgroundColor(altBg)
                tbl.AddCell(cell)
            Next
        End Sub

        ' ── Excel Helpers ────────────────────────────────────────────────

        Private Function CreateExcelSheet(pkg As ExcelPackage, sheetName As String,
                                          title As String, headers As String()) As ExcelWorksheet
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial
            Dim ws = pkg.Workbook.Worksheets.Add(sheetName)

            ' Title row
            ws.Cells(1, 1, 1, headers.Length).Merge = True
            ws.Cells(1, 1).Value = title
            With ws.Cells(1, 1).Style
                .Font.Bold = True : .Font.Size = 14
                .Fill.PatternType = ExcelFillStyle.Solid
                .Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#6B0000"))
                .Font.Color.SetColor(System.Drawing.Color.White)
                .HorizontalAlignment = ExcelHorizontalAlignment.Center
            End With

            ' Sub-header row
            ws.Cells(2, 1, 2, headers.Length).Merge = True
            ws.Cells(2, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}  |  By: {If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.FullName, "System")}"
            ws.Cells(2, 1).Style.Font.Size = 9
            ws.Cells(2, 1).Style.Font.Italic = True

            ' Header row
            For i = 0 To headers.Length - 1
                ws.Cells(3, i + 1).Value = headers(i)
                With ws.Cells(3, i + 1).Style
                    .Font.Bold = True
                    .Fill.PatternType = ExcelFillStyle.Solid
                    .Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#7B1A1A"))
                    .Font.Color.SetColor(System.Drawing.Color.White)
                End With
            Next
            Return ws
        End Function

        Private Sub StyleExcelRows(ws As ExcelWorksheet, startRow As Integer,
                                   endRow As Integer, colCount As Integer)
            Dim altColor = System.Drawing.ColorTranslator.FromHtml("#eaf1fb")
            For r = startRow To endRow
                If r Mod 2 = 0 Then
                    For c = 1 To colCount
                        ws.Cells(r, c).Style.Fill.PatternType = ExcelFillStyle.Solid
                        ws.Cells(r, c).Style.Fill.BackgroundColor.SetColor(altColor)
                    Next
                End If
            Next
            ws.Cells(ws.Dimension.Address).AutoFitColumns()
        End Sub

        ' ── Residents Report ─────────────────────────────────────────────

        Public Function ExportResidentsPdf(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim residents = _resRepo.GetAll()
                Using doc = CreatePdfDocument(filePath)
                    AddPdfHeader(doc, "Barangay Residents Report", "Residents Information Module")
                    Dim headers = {"Res. ID", "Full Name", "Age", "Gender", "Civil Status", "Purok", "Contact No.", "Status"}
                    Dim tbl = MakePdfTable(headers)
                    For i = 0 To residents.Count - 1
                        Dim r = residents(i)
                        AddPdfRow(tbl, {r.ResCode, r.FullName, r.Age.ToString(), r.Gender,
                                        r.CivilStatus, r.Purok, r.ContactNo,
                                        If(r.IsActive, "Active", "Inactive")}, i)
                    Next
                    doc.Add(tbl)
                    doc.Add(New Paragraph($"Total Records: {residents.Count}").SetFontSize(9).SetMarginTop(8))
                End Using
                _logRepo.Log("EXPORT", "Reports", $"Exported Residents PDF — {residents.Count} records")
                Return (True, "PDF exported successfully.")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        Public Function ExportResidentsExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim residents = _resRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Res. ID", "Last Name", "First Name", "Age", "Gender",
                                   "Civil Status", "Address", "Purok", "Contact No.", "Email", "Status"}
                    Dim ws = CreateExcelSheet(pkg, "Residents", "Barangay Residents Report", headers)
                    Dim row = 4
                    For Each r In residents
                        ws.Cells(row, 1).Value  = r.ResCode
                        ws.Cells(row, 2).Value  = r.LastName
                        ws.Cells(row, 3).Value  = r.FirstName
                        ws.Cells(row, 4).Value  = r.Age
                        ws.Cells(row, 5).Value  = r.Gender
                        ws.Cells(row, 6).Value  = r.CivilStatus
                        ws.Cells(row, 7).Value  = r.Address
                        ws.Cells(row, 8).Value  = r.Purok
                        ws.Cells(row, 9).Value  = r.ContactNo
                        ws.Cells(row, 10).Value = r.Email
                        ws.Cells(row, 11).Value = If(r.IsActive, "Active", "Inactive")
                        row += 1
                    Next
                    StyleExcelRows(ws, 4, row - 1, headers.Length)
                    pkg.SaveAs(New FileInfo(filePath))
                End Using
                _logRepo.Log("EXPORT", "Reports", $"Exported Residents Excel — {residents.Count} records")
                Return (True, "Excel exported successfully.")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Students Report ──────────────────────────────────────────────

        Public Function ExportStudentsExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim students = _stuRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Stud. ID", "Last Name", "First Name", "School", "Grade/Year",
                                   "School Year", "Purok", "Scholar", "Status"}
                    Dim ws = CreateExcelSheet(pkg, "Students", "Barangay Student Records", headers)
                    Dim row = 4
                    For Each s In students
                        ws.Cells(row, 1).Value = s.StudCode
                        ws.Cells(row, 2).Value = s.LastName
                        ws.Cells(row, 3).Value = s.FirstName
                        ws.Cells(row, 4).Value = s.SchoolName
                        ws.Cells(row, 5).Value = s.GradeYear
                        ws.Cells(row, 6).Value = s.SchoolYear
                        ws.Cells(row, 7).Value = s.Purok
                        ws.Cells(row, 8).Value = If(s.IsScholar, "Yes", "No")
                        ws.Cells(row, 9).Value = s.Status
                        row += 1
                    Next
                    StyleExcelRows(ws, 4, row - 1, headers.Length)
                    pkg.SaveAs(New FileInfo(filePath))
                End Using
                _logRepo.Log("EXPORT", "Reports", $"Exported Students Excel — {students.Count} records")
                Return (True, "Excel exported successfully.")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Ordinances Report ────────────────────────────────────────────

        Public Function ExportOrdinancesPdf(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim ords = _ordRepo.GetAll()
                Using doc = CreatePdfDocument(filePath)
                    AddPdfHeader(doc, "Barangay Ordinance Registry", "Ordinances & Resolutions Module")
                    Dim headers = {"BO Number", "Introduced By", "Description", "Date Enacted", "Approved By", "Status"}
                    Dim tbl = MakePdfTable(headers)
                    For i = 0 To ords.Count - 1
                        Dim o = ords(i)
                        AddPdfRow(tbl, {o.BoNumber, o.IntroducedBy, o.Description,
                                        o.DateEnacted.ToString("MM/dd/yyyy"), o.ApprovedBy, o.Status}, i)
                    Next
                    doc.Add(tbl)
                    doc.Add(New Paragraph($"Total Ordinances: {ords.Count}").SetFontSize(9).SetMarginTop(8))
                End Using
                _logRepo.Log("EXPORT", "Reports", $"Exported Ordinances PDF — {ords.Count} records")
                Return (True, "PDF exported successfully.")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Activities Report ────────────────────────────────────────────

        Public Function ExportActivitiesExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim acts = _actRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Act. ID", "Activity Name", "Date", "Venue", "Organizer", "Participants", "Status"}
                    Dim ws = CreateExcelSheet(pkg, "Activities", "Barangay Activities Summary", headers)
                    Dim row = 4
                    For Each a In acts
                        ws.Cells(row, 1).Value = a.ActCode
                        ws.Cells(row, 2).Value = a.ActivityName
                        ws.Cells(row, 3).Value = a.ActivityDate.ToString("MM/dd/yyyy")
                        ws.Cells(row, 4).Value = a.Venue
                        ws.Cells(row, 5).Value = a.Organizer
                        ws.Cells(row, 6).Value = a.Participants
                        ws.Cells(row, 7).Value = a.Status
                        row += 1
                    Next
                    StyleExcelRows(ws, 4, row - 1, headers.Length)
                    pkg.SaveAs(New FileInfo(filePath))
                End Using
                _logRepo.Log("EXPORT", "Reports", $"Exported Activities Excel — {acts.Count} records")
                Return (True, "Excel exported successfully.")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

    End Class

End Namespace
