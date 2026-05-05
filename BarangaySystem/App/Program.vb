Imports System.Windows.Forms
Imports BarangaySystem.DataAccess

Namespace BarangaySystem
    Module Program
        <STAThread>
        Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' ── TASK-29: Startup DB connection check ─────────────────────
            If Not DatabaseConfig.TestConnection() Then
                MessageBox.Show(
                    "Cannot connect to the MySQL database." & Environment.NewLine & Environment.NewLine &
                    "Please check:" & Environment.NewLine &
                    "  • MySQL server is running" & Environment.NewLine &
                    "  • Credentials in DatabaseConfig.vb are correct" & Environment.NewLine &
                    "  • Database 'barangay_db' exists (run barangay_schema.sql)",
                    "Database Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
                Return
            End If

            Application.Run(New Forms.LoginForm())
        End Sub
    End Module
End Namespace
