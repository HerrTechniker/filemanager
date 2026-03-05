Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

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

    Public Class LineBlock
        Public StartLine As Integer
        Public EndLine As Integer
        Public Lines As List(Of String)

        Public Overrides Function ToString() As String
            Return "Zeilen " & (StartLine + 1).ToString() & "-" & (EndLine + 1).ToString()
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
                SyncFullFile(differences(i))
            Next
        End Sub

        Public Shared Sub SyncFullFile(ByVal diff As FileDifference)
            EnsureTargetDirectory(diff.TargetFile)
            File.Copy(diff.SourceFile, diff.TargetFile, True)
        End Sub

        Public Shared Function CanPartialSync(ByVal diff As FileDifference) As Boolean
            If diff.Kind = DifferenceType.Added Then
                Return False
            End If

            Return IsLikelyTextFile(diff.SourceFile) AndAlso IsLikelyTextFile(diff.TargetFile)
        End Function

        Public Shared Function BuildAddedBlocks(ByVal sourceFile As String, ByVal targetFile As String) As List(Of LineBlock)
            Dim sourceLines As String() = ReadAllLinesSafe(sourceFile)
            Dim targetLines As String() = ReadAllLinesSafe(targetFile)

            Dim targetSet As Dictionary(Of String, Boolean) = New Dictionary(Of String, Boolean)(StringComparer.Ordinal)
            Dim i As Integer
            For i = 0 To targetLines.Length - 1
                Dim key As String = targetLines(i)
                If Not targetSet.ContainsKey(key) Then
                    targetSet(key) = True
                End If
            Next

            Dim blocks As List(Of LineBlock) = New List(Of LineBlock)()
            Dim activeStart As Integer = -1
            Dim activeLines As List(Of String) = New List(Of String)()

            For i = 0 To sourceLines.Length - 1
                Dim srcLine As String = sourceLines(i)
                Dim isAdded As Boolean = Not targetSet.ContainsKey(srcLine)

                If isAdded Then
                    If activeStart = -1 Then
                        activeStart = i
                        activeLines = New List(Of String)()
                    End If
                    activeLines.Add(srcLine)
                ElseIf activeStart <> -1 Then
                    blocks.Add(New LineBlock With {.StartLine = activeStart, .EndLine = i - 1, .Lines = activeLines})
                    activeStart = -1
                    activeLines = New List(Of String)()
                End If
            Next

            If activeStart <> -1 Then
                blocks.Add(New LineBlock With {.StartLine = activeStart, .EndLine = sourceLines.Length - 1, .Lines = activeLines})
            End If

            Return blocks
        End Function

        Public Shared Sub ApplySelectedBlocks(ByVal sourceFile As String, ByVal targetFile As String, ByVal blocks As List(Of LineBlock))
            Dim targetLines As List(Of String) = New List(Of String)(ReadAllLinesSafe(targetFile))

            Dim i As Integer
            For i = 0 To blocks.Count - 1
                Dim j As Integer
                For j = 0 To blocks(i).Lines.Count - 1
                    targetLines.Add(blocks(i).Lines(j))
                Next
            Next

            EnsureTargetDirectory(targetFile)
            File.WriteAllText(targetFile, String.Join(Environment.NewLine, targetLines.ToArray()), Encoding.UTF8)
        End Sub

        Private Shared Sub EnsureTargetDirectory(ByVal targetFile As String)
            Dim targetDir As String = Path.GetDirectoryName(targetFile)
            If targetDir IsNot Nothing AndAlso targetDir.Length > 0 AndAlso Not Directory.Exists(targetDir) Then
                Directory.CreateDirectory(targetDir)
            End If
        End Sub

        Private Shared Function IsLikelyTextFile(ByVal filePath As String) As Boolean
            If Not File.Exists(filePath) Then
                Return True
            End If

            Dim ext As String = Path.GetExtension(filePath).ToLowerInvariant()
            If ext = ".txt" OrElse ext = ".vb" OrElse ext = ".cs" OrElse ext = ".xml" OrElse ext = ".json" OrElse ext = ".config" OrElse ext = ".md" Then
                Return True
            End If

            Dim bytes As Byte() = File.ReadAllBytes(filePath)
            Dim maxLen As Integer = Math.Min(bytes.Length, 1024)
            Dim i As Integer
            For i = 0 To maxLen - 1
                If bytes(i) = 0 Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Shared Function ReadAllLinesSafe(ByVal filePath As String) As String()
            If Not File.Exists(filePath) Then
                Return New String() {}
            End If

            Return File.ReadAllText(filePath).Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
        End Function

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
