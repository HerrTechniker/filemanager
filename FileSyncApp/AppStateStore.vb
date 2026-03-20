Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Xml.Serialization

Namespace FileSyncApp
    <Serializable()> _
    Public Class PathMappingState
        Public SourcePath As String
        Public TargetPaths As List(Of String)

        Public Sub New()
            SourcePath = String.Empty
            TargetPaths = New List(Of String)()
        End Sub
    End Class

    <Serializable()> _
    Public Class AppState
        Public Mappings As List(Of PathMappingState)
        Public LastSelectedSourceIndex As Integer

        Public Sub New()
            Mappings = New List(Of PathMappingState)()
            LastSelectedSourceIndex = -1
        End Sub
    End Class

    Public NotInheritable Class AppStateStore
        Private Sub New()
        End Sub

        Public Shared Function Load() As AppState
            Dim path As String = GetStateFilePath()
            If Not File.Exists(path) Then
                Return New AppState()
            End If

            Try
                Dim serializer As XmlSerializer = New XmlSerializer(GetType(AppState))
                Using stream As FileStream = File.OpenRead(path)
                    Return CType(serializer.Deserialize(stream), AppState)
                End Using
            Catch ex As Exception
                Return New AppState()
            End Try
        End Function

        Public Shared Sub Save(ByVal state As AppState)
            Dim path As String = GetStateFilePath()
            Dim dir As String = Path.GetDirectoryName(path)
            If dir IsNot Nothing AndAlso dir.Length > 0 AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            Dim serializer As XmlSerializer = New XmlSerializer(GetType(AppState))
            Using stream As FileStream = File.Create(path)
                serializer.Serialize(stream, state)
            End Using
        End Sub

        Private Shared Function GetStateFilePath() As String
            Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Return Path.Combine(appData, "FileSyncApp", "state.xml")
        End Function
    End Class
End Namespace
