Imports System
Imports System.Drawing
Imports System.Windows.Forms

Namespace FileSyncApp
    Public NotInheritable Class DiffUtil
        Private Sub New()
        End Sub

        Public Shared Sub HighlightDifferences(ByVal leftBox As RichTextBox, ByVal rightBox As RichTextBox, ByVal leftText As String, ByVal rightText As String)
            leftBox.Clear()
            rightBox.Clear()
            leftBox.Text = leftText
            rightBox.Text = rightText

            Dim leftLines As String() = leftText.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
            Dim rightLines As String() = rightText.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
            Dim maxLine As Integer = Math.Max(leftLines.Length, rightLines.Length)

            Dim i As Integer
            For i = 0 To maxLine - 1
                Dim leftLine As String = String.Empty
                Dim rightLine As String = String.Empty

                If i < leftLines.Length Then
                    leftLine = leftLines(i)
                End If

                If i < rightLines.Length Then
                    rightLine = rightLines(i)
                End If

                If String.Compare(leftLine, rightLine, StringComparison.Ordinal) <> 0 Then
                    HighlightLine(leftBox, i, Color.MistyRose)
                    HighlightLine(rightBox, i, Color.Honeydew)
                End If
            Next

            leftBox.SelectionLength = 0
            rightBox.SelectionLength = 0
        End Sub

        Private Shared Sub HighlightLine(ByVal box As RichTextBox, ByVal line As Integer, ByVal backColor As Color)
            If line < 0 OrElse line >= box.Lines.Length Then
                Return
            End If

            Dim start As Integer = box.GetFirstCharIndexFromLine(line)
            If start < 0 Then
                Return
            End If

            Dim length As Integer = box.Lines(line).Length
            If line < box.Lines.Length - 1 Then
                length += Environment.NewLine.Length
            End If

            box.SelectionStart = start
            box.SelectionLength = length
            box.SelectionBackColor = backColor
        End Sub
    End Class
End Namespace
