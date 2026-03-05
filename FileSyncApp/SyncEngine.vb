Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace FileSyncApp
    Public Enum DifferenceType
        Added
        Modified
    End Enum

    Public Class FileDifference
        Public RelativePath As String
        Public SourceFile As String
        Public TargetFile As String
        Public Kind As DifferenceType

        Public Overrides Function ToString() As String
            Return Kind.ToString() & ": " & RelativePath
        End Function
    End Class

    Public Class PathMapping
        Public SourcePath As String
        Public TargetPaths As List(Of String)

        Public Sub New(ByVal source As String)
            SourcePath = source
            TargetPaths = New List(Of String)()
        End Sub

        Public Overrides Function ToString() As String
            Return SourcePath
        End Function
    End Class

    Public NotInheritable Class SyncEngine
        Private Sub New()
        End Sub

        Public Shared Function ScanDifferences(ByVal sourcePath As String, ByVal targetPath As String) As List(Of FileDifference)
            Dim differences As List(Of FileDifference) = New List(Of FileDifference)()

            If Not Directory.Exists(sourcePath) Then
                Return differences
            End If

            Dim sourceFiles As String() = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)
            Dim i As Integer
            For i = 0 To sourceFiles.Length - 1
                Dim sourceFile As String = sourceFiles(i)
                Dim relativePath As String = sourceFile.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar)
                Dim targetFile As String = Path.Combine(targetPath, relativePath)

                If Not File.Exists(targetFile) Then
                    differences.Add(New FileDifference With {.RelativePath = relativePath, .SourceFile = sourceFile, .TargetFile = targetFile, .Kind = DifferenceType.Added})
                ElseIf IsFileModified(sourceFile, targetFile) Then
                    differences.Add(New FileDifference With {.RelativePath = relativePath, .SourceFile = sourceFile, .TargetFile = targetFile, .Kind = DifferenceType.Modified})
                End If
            Next

            Return differences
        End Function

        Public Shared Sub SyncDifferences(ByVal differences As List(Of FileDifference))
            Dim i As Integer
            For i = 0 To differences.Count - 1
                Dim diff As FileDifference = differences(i)
                Dim targetDir As String = Path.GetDirectoryName(diff.TargetFile)
                If targetDir IsNot Nothing AndAlso targetDir.Length > 0 AndAlso Not Directory.Exists(targetDir) Then
                    Directory.CreateDirectory(targetDir)
                End If

                File.Copy(diff.SourceFile, diff.TargetFile, True)
            Next
        End Sub

        Private Shared Function IsFileModified(ByVal sourceFile As String, ByVal targetFile As String) As Boolean
            Dim sourceInfo As FileInfo = New FileInfo(sourceFile)
            Dim targetInfo As FileInfo = New FileInfo(targetFile)

            If sourceInfo.Length <> targetInfo.Length Then
                Return True
            End If

            Return sourceInfo.LastWriteTimeUtc <> targetInfo.LastWriteTimeUtc
        End Function
    End Class
End Namespace
