Imports System.Diagnostics
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
        ' iText7 requires writer → PdfDocument → Document to be closed in
        ' reverse order so all bytes are flushed before we open the file.
        ' A single Using block on Document alone does NOT close the writer.

        Private Sub WritePdf(filePath As String,
                             title As String, subtitle As String,
                             headers As String(),
                             addRows As Action(Of Table))
            Dim writer As PdfWriter   = Nothing
            Dim pdf    As PdfDocument = Nothing
            Dim doc    As Document    = Nothing
            Try
                writer = New PdfWriter(filePath)
                pdf    = New PdfDocument(writer)
                doc    = New Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate())
                doc.SetMargins(36, 36, 36, 36)

                AddPdfHeader(doc, title, subtitle)
                Dim tbl = MakePdfTable(headers)
                addRows(tbl)
                doc.Add(tbl)
            Finally
                ' Close in reverse order — flushes all pending bytes to disk
                If doc    IsNot Nothing Then doc.Close()
                If pdf    IsNot Nothing Then pdf.Close()
                If writer IsNot Nothing Then writer.Close()
            End Try
        End Sub

        Private Sub AddPdfHeader(doc As Document, title As String, subtitle As String)
            Dim headerColor = New DeviceRgb(107, 0, 0)   ' maroon — matches app theme
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
            Dim headBg = New DeviceRgb(123, 26, 26)
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
            Dim altBg = New DeviceRgb(255, 240, 240)
            For Each v In values
                Dim cell = New Cell().Add(New Paragraph(If(v, "")).SetFontSize(8)).SetPadding(3)
                If rowIndex Mod 2 = 0 Then cell.SetBackgroundColor(altBg)
                tbl.AddCell(cell)
            Next
        End Sub

        ' ── Excel Helpers ────────────────────────────────────────────────
        ' LicenseContext MUST be set before ExcelPackage is instantiated.

        Private Shared Sub SetExcelLicense()
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial
        End Sub

        Private Function CreateExcelSheet(pkg As ExcelPackage, sheetName As String,
                                          title As String, headers As String()) As ExcelWorksheet
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
            ws.Cells(2, 1).Style.Font.Size   = 9
            ws.Cells(2, 1).Style.Font.Italic = True

            ' Column header row
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
            Dim altColor = System.Drawing.ColorTranslator.FromHtml("#FFF0F0")
            For r = startRow To endRow
                If r Mod 2 = 0 Then
                    For c = 1 To colCount
                        ws.Cells(r, c).Style.Fill.PatternType = ExcelFillStyle.Solid
                        ws.Cells(r, c).Style.Fill.BackgroundColor.SetColor(altColor)
                    Next
                End If
            Next
            If ws.Dimension IsNot Nothing Then
                ws.Cells(ws.Dimension.Address).AutoFitColumns()
            End If
        End Sub

        ' ── Open exported file with the default OS application ───────────
        Private Shared Sub OpenFile(filePath As String)
            Try
                Dim psi As New ProcessStartInfo(filePath) With {.UseShellExecute = True}
                Process.Start(psi)
            Catch
                ' If the OS has no handler for the file type, silently ignore.
                ' The file is still saved at the chosen path.
            End Try
        End Sub

        ' ── Residents Report ─────────────────────────────────────────────

        Public Function ExportResidentsPdf(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim residents = _resRepo.GetAll()
                Dim rowIdx    = 0
                WritePdf(filePath,
                         "Barangay Residents Report",
                         "Residents Information Module",
                         {"Res. ID", "Full Name", "Age", "Gender", "Civil Status", "Purok", "Contact No.", "Status"},
                         Sub(tbl)
                             For Each r In residents
                                 AddPdfRow(tbl, {r.ResCode, r.FullName, r.Age.ToString(), r.Gender,
                                                 r.CivilStatus, r.Purok, r.ContactNo,
                                                 If(r.IsActive, "Active", "Inactive")}, rowIdx)
                                 rowIdx += 1
                             Next
                         End Sub)
                _logRepo.Log("EXPORT", "Reports", $"Exported Residents PDF — {residents.Count} records")
                OpenFile(filePath)
                Return (True, $"PDF exported successfully. ({residents.Count} records)")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        Public Function ExportResidentsExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                SetExcelLicense()
                Dim residents = _resRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Res. ID", "Last Name", "First Name", "Age", "Gender",
                                   "Civil Status", "Address", "Purok", "Contact No.", "Email", "Status"}
                    Dim ws  = CreateExcelSheet(pkg, "Residents", "Barangay Residents Report", headers)
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
                OpenFile(filePath)
                Return (True, $"Excel exported successfully. ({residents.Count} records)")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Students Report ──────────────────────────────────────────────

        Public Function ExportStudentsExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                SetExcelLicense()
                Dim students = _stuRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Stud. ID", "Last Name", "First Name", "School", "Grade/Year",
                                   "School Year", "Purok", "Scholar", "Status"}
                    Dim ws  = CreateExcelSheet(pkg, "Students", "Barangay Student Records", headers)
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
                OpenFile(filePath)
                Return (True, $"Excel exported successfully. ({students.Count} records)")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Ordinances Report ────────────────────────────────────────────

        Public Function ExportOrdinancesPdf(filePath As String) As (Success As Boolean, Message As String)
            Try
                Dim ords   = _ordRepo.GetAll()
                Dim rowIdx = 0
                WritePdf(filePath,
                         "Barangay Ordinance Registry",
                         "Ordinances & Resolutions Module",
                         {"BO Number", "Introduced By", "Description", "Date Enacted", "Approved By", "Status"},
                         Sub(tbl)
                             For Each o In ords
                                 AddPdfRow(tbl, {o.BoNumber, o.IntroducedBy, o.Description,
                                                 o.DateEnacted.ToString("MM/dd/yyyy"),
                                                 o.ApprovedBy, o.Status}, rowIdx)
                                 rowIdx += 1
                             Next
                         End Sub)
                _logRepo.Log("EXPORT", "Reports", $"Exported Ordinances PDF — {ords.Count} records")
                OpenFile(filePath)
                Return (True, $"PDF exported successfully. ({ords.Count} records)")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Activities Report ────────────────────────────────────────────

        Public Function ExportActivitiesExcel(filePath As String) As (Success As Boolean, Message As String)
            Try
                SetExcelLicense()
                Dim acts = _actRepo.GetAll()
                Using pkg As New ExcelPackage()
                    Dim headers = {"Act. ID", "Activity Name", "Date", "Venue", "Organizer", "Participants", "Status"}
                    Dim ws  = CreateExcelSheet(pkg, "Activities", "Barangay Activities Summary", headers)
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
                OpenFile(filePath)
                Return (True, $"Excel exported successfully. ({acts.Count} records)")
            Catch ex As Exception
                Return (False, $"Export error: {ex.Message}")
            End Try
        End Function

        ' ── Student Excel Import ─────────────────────────────────────────
        ' Expected columns (row 1 = header, data starts row 2):
        '   A: Last Name *   B: First Name *   C: Middle Name
        '   D: Birth Date    E: Gender         F: Address *
        '   G: Purok *       H: School Name    I: Grade/Year *
        '   J: School Year * K: Scholar (Yes/No)  L: Status

        Public Function ImportStudentsFromExcel(filePath As String) As (Success As Boolean, Message As String, Imported As Integer, Skipped As Integer)
            Try
                SetExcelLicense()
                Dim stuRepo    As New StudentRepository()
                Dim schoolRepo As New SchoolRepository()
                Dim schools    = schoolRepo.GetAll()

                Dim toInsert   As New List(Of StudentModel)
                Dim skipped    = 0
                Dim errors     As New List(Of String)

                Using pkg As New ExcelPackage(New FileInfo(filePath))
                    If pkg.Workbook.Worksheets.Count = 0 Then
                        Return (False, "The Excel file has no worksheets.", 0, 0)
                    End If
                    Dim ws   = pkg.Workbook.Worksheets(0)
                    Dim rows = ws.Dimension?.Rows
                    If rows Is Nothing OrElse rows < 2 Then
                        Return (False, "No data rows found. Make sure row 1 is the header.", 0, 0)
                    End If

                    For r = 2 To rows
                        Dim lastName  = ws.Cells(r, 1).Text.Trim()
                        Dim firstName = ws.Cells(r, 2).Text.Trim()
                        Dim address   = ws.Cells(r, 6).Text.Trim()
                        Dim purok     = ws.Cells(r, 7).Text.Trim()
                        Dim gradeYear = ws.Cells(r, 9).Text.Trim()
                        Dim schoolYr  = ws.Cells(r, 10).Text.Trim()

                        ' Skip blank rows
                        If String.IsNullOrWhiteSpace(lastName) AndAlso
                           String.IsNullOrWhiteSpace(firstName) Then Continue For

                        ' Validate required fields
                        If String.IsNullOrWhiteSpace(lastName) Then
                            errors.Add($"Row {r}: Last Name is required.") : skipped += 1 : Continue For
                        End If
                        If String.IsNullOrWhiteSpace(firstName) Then
                            errors.Add($"Row {r}: First Name is required.") : skipped += 1 : Continue For
                        End If
                        If String.IsNullOrWhiteSpace(address) Then
                            errors.Add($"Row {r}: Address is required.") : skipped += 1 : Continue For
                        End If
                        If String.IsNullOrWhiteSpace(purok) Then
                            errors.Add($"Row {r}: Purok is required.") : skipped += 1 : Continue For
                        End If
                        If String.IsNullOrWhiteSpace(gradeYear) Then
                            errors.Add($"Row {r}: Grade/Year is required.") : skipped += 1 : Continue For
                        End If
                        If String.IsNullOrWhiteSpace(schoolYr) Then
                            errors.Add($"Row {r}: School Year is required.") : skipped += 1 : Continue For
                        End If

                        ' Resolve school name → school_id
                        Dim schoolName = ws.Cells(r, 8).Text.Trim()
                        Dim schoolId   As Integer? = Nothing
                        If Not String.IsNullOrWhiteSpace(schoolName) Then
                            Dim sc = schools.FirstOrDefault(
                                Function(s) String.Equals(s.SchoolName, schoolName,
                                            StringComparison.OrdinalIgnoreCase))
                            If sc IsNot Nothing Then schoolId = sc.SchoolId
                        End If

                        ' Parse birth date (flexible — accepts MM/dd/yyyy or yyyy-MM-dd)
                        Dim bd As DateTime? = Nothing
                        Dim bdText = ws.Cells(r, 4).Text.Trim()
                        If Not String.IsNullOrWhiteSpace(bdText) Then
                            Dim parsed As DateTime
                            If DateTime.TryParse(bdText, parsed) Then bd = parsed.Date
                        End If

                        ' Parse scholar flag
                        Dim scholarText = ws.Cells(r, 11).Text.Trim().ToLower()
                        Dim isScholar   = (scholarText = "yes" OrElse scholarText = "1" OrElse scholarText = "true")

                        ' Parse status
                        Dim statusText = ws.Cells(r, 12).Text.Trim()
                        Dim status     = If({"Enrolled", "Dropped", "Graduated"}.Contains(statusText), statusText, "Enrolled")

                        Dim model As New StudentModel With {
                            .StudCode   = stuRepo.NextStudCode(),
                            .LastName   = lastName,
                            .FirstName  = firstName,
                            .MiddleName = ws.Cells(r, 3).Text.Trim(),
                            .BirthDate  = bd,
                            .Gender     = ws.Cells(r, 5).Text.Trim(),
                            .Address    = address,
                            .Purok      = purok,
                            .SchoolId   = schoolId,
                            .GradeYear  = gradeYear,
                            .SchoolYear = schoolYr,
                            .IsScholar  = isScholar,
                            .Status     = status,
                            .CreatedBy  = If(Session.CurrentUser IsNot Nothing, Session.CurrentUser.UserId, 0)
                        }
                        toInsert.Add(model)
                    Next
                End Using

                If toInsert.Count = 0 Then
                    Dim msg = If(errors.Count > 0,
                                 "No valid rows to import." & Environment.NewLine & String.Join(Environment.NewLine, errors.Take(5)),
                                 "No data rows found in the file.")
                    Return (False, msg, 0, skipped)
                End If

                Dim inserted = stuRepo.BulkInsert(toInsert)
                _logRepo.Log("INSERT", "Students", $"Imported {inserted} students from Excel")

                Dim summary = $"Import complete: {inserted} student(s) added, {skipped} row(s) skipped."
                If errors.Count > 0 Then
                    summary &= Environment.NewLine & "Skipped rows:" & Environment.NewLine &
                               String.Join(Environment.NewLine, errors.Take(10))
                End If
                Return (True, summary, inserted, skipped)

            Catch ex As Exception
                Return (False, $"Import error: {ex.Message}", 0, 0)
            End Try
        End Function

    End Class

End Namespace
