Imports System
Imports System.Windows.Forms

Namespace FileSyncApp
    Friend NotInheritable Class Program
        Private Sub New()
        End Sub

        <STAThread()> _
        Public Shared Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub
    End Class
End Namespace
