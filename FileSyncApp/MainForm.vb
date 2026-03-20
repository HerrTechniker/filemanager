Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Windows.Forms

Namespace FileSyncApp
    Public Class MainForm
        Inherits Form

        Private ReadOnly _mappings As List(Of PathMapping)
        Private ReadOnly _diffByNode As Dictionary(Of TreeNode, FileDifference)
        Private ReadOnly _blockByItem As Dictionary(Of ListViewItem, LineBlock)
        Private _activeDiff As FileDifference

        Private txtSource As TextBox
        Private btnAddSource As Button
        Private btnRemoveSource As Button
        Private btnBrowseSource As Button
        Private lstSources As ListBox

        Private txtTarget As TextBox
        Private btnAddTarget As Button
        Private btnRemoveTarget As Button
        Private btnBrowseTarget As Button
        Private lstTargets As CheckedListBox

        Private tvChanges As TreeView
        Private btnScan As Button
        Private btnSyncChecked As Button
        Private btnSyncAll As Button
        Private btnApplySelectedBlocks As Button

        Private rtbSource As RichTextBox
        Private rtbTarget As RichTextBox
        Private lvBlocks As ListView

        Public Sub New()
            _mappings = New List(Of PathMapping)()
            _diffByNode = New Dictionary(Of TreeNode, FileDifference)()
            _blockByItem = New Dictionary(Of ListViewItem, LineBlock)()
            InitializeComponent()
            AddHandler Me.Load, AddressOf MainForm_Load
            AddHandler Me.FormClosing, AddressOf MainForm_FormClosing
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Dateipfad-Synchronisation (VB8)"
            Me.Width = 1380
            Me.Height = 820
            Me.StartPosition = FormStartPosition.CenterScreen

            Dim leftPanel As Panel = New Panel()
            leftPanel.Dock = DockStyle.Left
            leftPanel.Width = 360

            Dim lblSource As Label = New Label()
            lblSource.Text = "Quellpfad"
            lblSource.Left = 10
            lblSource.Top = 12
            lblSource.Width = 100

            txtSource = New TextBox()
            txtSource.Left = 10
            txtSource.Top = 32
            txtSource.Width = 240

            btnBrowseSource = New Button()
            btnBrowseSource.Text = "..."
            btnBrowseSource.Left = 256
            btnBrowseSource.Top = 30
            btnBrowseSource.Width = 35
            AddHandler btnBrowseSource.Click, AddressOf BrowseSource_Click

            btnAddSource = New Button()
            btnAddSource.Text = "Quelle hinzufügen"
            btnAddSource.Left = 10
            btnAddSource.Top = 62
            btnAddSource.Width = 150
            AddHandler btnAddSource.Click, AddressOf AddSource_Click

            btnRemoveSource = New Button()
            btnRemoveSource.Text = "Quelle entfernen"
            btnRemoveSource.Left = 170
            btnRemoveSource.Top = 62
            btnRemoveSource.Width = 150
            AddHandler btnRemoveSource.Click, AddressOf RemoveSource_Click

            lstSources = New ListBox()
            lstSources.Left = 10
            lstSources.Top = 98
            lstSources.Width = 330
            lstSources.Height = 190
            AddHandler lstSources.SelectedIndexChanged, AddressOf Sources_SelectedIndexChanged

            Dim lblTarget As Label = New Label()
            lblTarget.Text = "Zielpfad (mehrere möglich)"
            lblTarget.Left = 10
            lblTarget.Top = 296
            lblTarget.Width = 250

            txtTarget = New TextBox()
            txtTarget.Left = 10
            txtTarget.Top = 316
            txtTarget.Width = 240

            btnBrowseTarget = New Button()
            btnBrowseTarget.Text = "..."
            btnBrowseTarget.Left = 256
            btnBrowseTarget.Top = 314
            btnBrowseTarget.Width = 35
            AddHandler btnBrowseTarget.Click, AddressOf BrowseTarget_Click

            btnAddTarget = New Button()
            btnAddTarget.Text = "Ziel hinzufügen"
            btnAddTarget.Left = 10
            btnAddTarget.Top = 346
            btnAddTarget.Width = 120
            AddHandler btnAddTarget.Click, AddressOf AddTarget_Click

            btnRemoveTarget = New Button()
            btnRemoveTarget.Text = "Ziel entfernen"
            btnRemoveTarget.Left = 140
            btnRemoveTarget.Top = 346
            btnRemoveTarget.Width = 120
            AddHandler btnRemoveTarget.Click, AddressOf RemoveTarget_Click

            lstTargets = New CheckedListBox()
            lstTargets.Left = 10
            lstTargets.Top = 380
            lstTargets.Width = 330
            lstTargets.Height = 145

            btnScan = New Button()
            btnScan.Text = "Unterschiede scannen"
            btnScan.Left = 10
            btnScan.Top = 536
            btnScan.Width = 160
            AddHandler btnScan.Click, AddressOf Scan_Click

            btnSyncChecked = New Button()
            btnSyncChecked.Text = "Markierte Ziele synchronisieren"
            btnSyncChecked.Left = 10
            btnSyncChecked.Top = 570
            btnSyncChecked.Width = 220
            AddHandler btnSyncChecked.Click, AddressOf SyncChecked_Click

            btnSyncAll = New Button()
            btnSyncAll.Text = "Alle Ziele synchronisieren"
            btnSyncAll.Left = 10
            btnSyncAll.Top = 604
            btnSyncAll.Width = 220
            AddHandler btnSyncAll.Click, AddressOf SyncAll_Click

            leftPanel.Controls.Add(lblSource)
            leftPanel.Controls.Add(txtSource)
            leftPanel.Controls.Add(btnBrowseSource)
            leftPanel.Controls.Add(btnAddSource)
            leftPanel.Controls.Add(btnRemoveSource)
            leftPanel.Controls.Add(lstSources)
            leftPanel.Controls.Add(lblTarget)
            leftPanel.Controls.Add(txtTarget)
            leftPanel.Controls.Add(btnBrowseTarget)
            leftPanel.Controls.Add(btnAddTarget)
            leftPanel.Controls.Add(btnRemoveTarget)
            leftPanel.Controls.Add(lstTargets)
            leftPanel.Controls.Add(btnScan)
            leftPanel.Controls.Add(btnSyncChecked)
            leftPanel.Controls.Add(btnSyncAll)

            Dim splitMain As SplitContainer = New SplitContainer()
            splitMain.Dock = DockStyle.Fill
            splitMain.Orientation = Orientation.Horizontal
            splitMain.SplitterDistance = 330

            tvChanges = New TreeView()
            tvChanges.Dock = DockStyle.Fill
            AddHandler tvChanges.AfterSelect, AddressOf Changes_AfterSelect
            splitMain.Panel1.Controls.Add(tvChanges)

            Dim splitBottom As SplitContainer = New SplitContainer()
            splitBottom.Dock = DockStyle.Fill
            splitBottom.Orientation = Orientation.Vertical
            splitBottom.SplitterDistance = 880

            Dim splitDiff As SplitContainer = New SplitContainer()
            splitDiff.Dock = DockStyle.Fill
            splitDiff.Orientation = Orientation.Vertical
            splitDiff.SplitterDistance = 440

            rtbSource = New RichTextBox()
            rtbSource.Dock = DockStyle.Fill
            rtbSource.ReadOnly = True
            rtbSource.WordWrap = False

            rtbTarget = New RichTextBox()
            rtbTarget.Dock = DockStyle.Fill
            rtbTarget.ReadOnly = True
            rtbTarget.WordWrap = False

            splitDiff.Panel1.Controls.Add(rtbSource)
            splitDiff.Panel2.Controls.Add(rtbTarget)

            lvBlocks = New ListView()
            lvBlocks.Dock = DockStyle.Fill
            lvBlocks.CheckBoxes = True
            lvBlocks.View = View.Details
            lvBlocks.FullRowSelect = True
            lvBlocks.Columns.Add("Block", 120)
            lvBlocks.Columns.Add("Vorschau", 320)

            btnApplySelectedBlocks = New Button()
            btnApplySelectedBlocks.Text = "Ausgewählte Blöcke übernehmen"
            btnApplySelectedBlocks.Dock = DockStyle.Bottom
            btnApplySelectedBlocks.Height = 36
            AddHandler btnApplySelectedBlocks.Click, AddressOf ApplySelectedBlocks_Click

            splitBottom.Panel1.Controls.Add(splitDiff)
            splitBottom.Panel2.Controls.Add(lvBlocks)
            splitBottom.Panel2.Controls.Add(btnApplySelectedBlocks)

            splitMain.Panel2.Controls.Add(splitBottom)

            Me.Controls.Add(splitMain)
            Me.Controls.Add(leftPanel)
        End Sub

        Private Function CurrentMapping() As PathMapping
            If lstSources.SelectedIndex < 0 Then
                Return Nothing
            End If
            Return CType(lstSources.SelectedItem, PathMapping)
        End Function

        Private Sub BrowseSource_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using dialog As FolderBrowserDialog = New FolderBrowserDialog()
                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    txtSource.Text = dialog.SelectedPath
                End If
            End Using
        End Sub

        Private Sub BrowseTarget_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using dialog As FolderBrowserDialog = New FolderBrowserDialog()
                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    txtTarget.Text = dialog.SelectedPath
                End If
            End Using
        End Sub

        Private Sub AddSource_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim source As String = txtSource.Text.Trim()
            If source.Length = 0 OrElse Not Directory.Exists(source) Then
                MessageBox.Show(Me, "Bitte einen gültigen Quellordner angeben.")
                Return
            End If

            Dim i As Integer
            For i = 0 To _mappings.Count - 1
                If String.Compare(_mappings(i).SourcePath, source, StringComparison.OrdinalIgnoreCase) = 0 Then
                    MessageBox.Show(Me, "Diese Quelle ist bereits vorhanden.")
                    Return
                End If
            Next

            Dim mapping As PathMapping = New PathMapping(source)
            _mappings.Add(mapping)
            lstSources.Items.Add(mapping)
            lstSources.SelectedItem = mapping
            txtSource.Clear()
            SaveState()
        End Sub

        Private Sub RemoveSource_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim mapping As PathMapping = CurrentMapping()
            If mapping Is Nothing Then
                Return
            End If

            _mappings.Remove(mapping)
            lstSources.Items.Remove(mapping)
            tvChanges.Nodes.Clear()
            lvBlocks.Items.Clear()
            rtbSource.Clear()
            rtbTarget.Clear()
            ReloadTargets()
            SaveState()
        End Sub

        Private Sub Sources_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
            ReloadTargets()
        End Sub

        Private Sub ReloadTargets()
            lstTargets.Items.Clear()
            Dim mapping As PathMapping = CurrentMapping()
            If mapping Is Nothing Then
                Return
            End If

            Dim i As Integer
            For i = 0 To mapping.TargetPaths.Count - 1
                lstTargets.Items.Add(mapping.TargetPaths(i), True)
            Next
        End Sub

        Private Sub AddTarget_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim mapping As PathMapping = CurrentMapping()
            If mapping Is Nothing Then
                MessageBox.Show(Me, "Bitte zuerst einen Quellpfad auswählen.")
                Return
            End If

            Dim target As String = txtTarget.Text.Trim()
            If target.Length = 0 Then
                MessageBox.Show(Me, "Bitte einen Zielpfad angeben.")
                Return
            End If

            If String.Compare(mapping.SourcePath, target, StringComparison.OrdinalIgnoreCase) = 0 Then
                MessageBox.Show(Me, "Quelle und Ziel dürfen nicht identisch sein.")
                Return
            End If

            Dim i As Integer
            For i = 0 To mapping.TargetPaths.Count - 1
                If String.Compare(mapping.TargetPaths(i), target, StringComparison.OrdinalIgnoreCase) = 0 Then
                    MessageBox.Show(Me, "Dieses Ziel ist bereits vorhanden.")
                    Return
                End If
            Next

            mapping.TargetPaths.Add(target)
            ReloadTargets()
            txtTarget.Clear()
            SaveState()
        End Sub

        Private Sub RemoveTarget_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim mapping As PathMapping = CurrentMapping()
            If mapping Is Nothing OrElse lstTargets.SelectedIndex < 0 Then
                Return
            End If

            mapping.TargetPaths.Remove(CStr(lstTargets.SelectedItem))
            ReloadTargets()
            SaveState()
        End Sub

        Private Sub MainForm_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim state As AppState = AppStateStore.Load()
            LoadState(state)
        End Sub

        Private Sub MainForm_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
            SaveState()
        End Sub

        Private Sub LoadState(ByVal state As AppState)
            _mappings.Clear()
            lstSources.Items.Clear()
            lstTargets.Items.Clear()

            Dim i As Integer
            For i = 0 To state.Mappings.Count - 1
                Dim saved As PathMappingState = state.Mappings(i)
                If saved.SourcePath Is Nothing OrElse saved.SourcePath.Length = 0 Then
                    Continue For
                End If

                Dim mapping As PathMapping = New PathMapping(saved.SourcePath)
                If saved.TargetPaths IsNot Nothing Then
                    Dim j As Integer
                    For j = 0 To saved.TargetPaths.Count - 1
                        mapping.TargetPaths.Add(saved.TargetPaths(j))
                    Next
                End If

                _mappings.Add(mapping)
                lstSources.Items.Add(mapping)
            Next

            If lstSources.Items.Count > 0 Then
                Dim idx As Integer = state.LastSelectedSourceIndex
                If idx < 0 OrElse idx >= lstSources.Items.Count Then
                    idx = 0
                End If
                lstSources.SelectedIndex = idx
            End If
        End Sub

        Private Sub SaveState()
            Dim state As AppState = New AppState()
            Dim i As Integer
            For i = 0 To _mappings.Count - 1
                Dim mapping As PathMapping = _mappings(i)
                Dim saved As PathMappingState = New PathMappingState()
                saved.SourcePath = mapping.SourcePath

                Dim j As Integer
                For j = 0 To mapping.TargetPaths.Count - 1
                    saved.TargetPaths.Add(mapping.TargetPaths(j))
                Next

                state.Mappings.Add(saved)
            Next

            state.LastSelectedSourceIndex = lstSources.SelectedIndex
            AppStateStore.Save(state)
        End Sub

        Private Sub Scan_Click(ByVal sender As Object, ByVal e As EventArgs)
            SyncMappings(False, False)
        End Sub

        Private Sub SyncChecked_Click(ByVal sender As Object, ByVal e As EventArgs)
            SyncMappings(True, True)
        End Sub

        Private Sub SyncAll_Click(ByVal sender As Object, ByVal e As EventArgs)
            SyncMappings(True, False)
        End Sub

        Private Sub SyncMappings(ByVal doSync As Boolean, ByVal onlyChecked As Boolean)
            tvChanges.Nodes.Clear()
            _diffByNode.Clear()
            _activeDiff = Nothing
            lvBlocks.Items.Clear()

            Dim i As Integer
            For i = 0 To _mappings.Count - 1
                AddSourceNode(_mappings(i), doSync, onlyChecked)
            Next

            tvChanges.ExpandAll()
            If doSync Then
                MessageBox.Show(Me, "Synchronisierung abgeschlossen.")
            End If
        End Sub

        Private Sub AddSourceNode(ByVal mapping As PathMapping, ByVal doSync As Boolean, ByVal onlyChecked As Boolean)
            Dim sourceNode As TreeNode = tvChanges.Nodes.Add(mapping.SourcePath)
            sourceNode.NodeFont = New Drawing.Font(tvChanges.Font, Drawing.FontStyle.Bold)

            Dim j As Integer
            For j = 0 To mapping.TargetPaths.Count - 1
                Dim target As String = mapping.TargetPaths(j)
                If onlyChecked AndAlso Not IsTargetSelected(target) Then
                    Continue For
                End If

                Dim targetNode As TreeNode = sourceNode.Nodes.Add("→ " & target)
                Dim differences As List(Of FileDifference) = SyncEngine.ScanDifferences(mapping.SourcePath, target)

                If differences.Count = 0 Then
                    targetNode.Nodes.Add("Keine Änderungen")
                Else
                    If doSync Then
                        SyncEngine.SyncDifferences(differences)
                    End If

                    Dim k As Integer
                    For k = 0 To differences.Count - 1
                        Dim diff As FileDifference = differences(k)
                        AddDifferenceTreeNode(targetNode, diff)
                    Next
                End If
            Next
        End Sub

        Private Sub AddDifferenceTreeNode(ByVal targetNode As TreeNode, ByVal diff As FileDifference)
            Dim separators As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Dim parts As String() = diff.RelativePath.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length = 0 Then
                Return
            End If

            Dim currentNode As TreeNode = targetNode
            Dim i As Integer
            For i = 0 To parts.Length - 2
                currentNode = FindOrCreateChildNode(currentNode, parts(i))
            Next

            Dim fileText As String = diff.Kind.ToString() & ": " & parts(parts.Length - 1)
            Dim fileNode As TreeNode = currentNode.Nodes.Add(fileText)
            _diffByNode(fileNode) = diff
        End Sub

        Private Function FindOrCreateChildNode(ByVal parent As TreeNode, ByVal text As String) As TreeNode
            Dim i As Integer
            For i = 0 To parent.Nodes.Count - 1
                If String.Compare(parent.Nodes(i).Text, text, StringComparison.OrdinalIgnoreCase) = 0 Then
                    Return parent.Nodes(i)
                End If
            Next

            Return parent.Nodes.Add(text)
        End Function

        Private Function IsTargetSelected(ByVal target As String) As Boolean
            Dim i As Integer
            For i = 0 To lstTargets.CheckedItems.Count - 1
                If String.Compare(CStr(lstTargets.CheckedItems(i)), target, StringComparison.OrdinalIgnoreCase) = 0 Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private Sub Changes_AfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs)
            Dim diff As FileDifference = Nothing
            If Not _diffByNode.TryGetValue(e.Node, diff) Then
                _activeDiff = Nothing
                rtbSource.Clear()
                rtbTarget.Clear()
                lvBlocks.Items.Clear()
                Return
            End If

            _activeDiff = diff
            Dim sourceText As String = ReadFileSafe(diff.SourceFile)
            Dim targetText As String = ReadFileSafe(diff.TargetFile)
            DiffUtil.HighlightDifferences(rtbSource, rtbTarget, sourceText, targetText)
            LoadBlocksForDiff(diff)
        End Sub

        Private Sub LoadBlocksForDiff(ByVal diff As FileDifference)
            lvBlocks.Items.Clear()
            _blockByItem.Clear()

            If Not SyncEngine.CanPartialSync(diff) Then
                Dim info As ListViewItem = lvBlocks.Items.Add("Vollständige Datei")
                info.SubItems.Add("Teil-Sync für diesen Dateityp nicht verfügbar.")
                info.Checked = False
                Return
            End If

            Dim blocks As List(Of LineBlock) = SyncEngine.BuildAddedBlocks(diff.SourceFile, diff.TargetFile)
            Dim i As Integer
            For i = 0 To blocks.Count - 1
                Dim preview As String = String.Join(" ", blocks(i).Lines.ToArray())
                If preview.Length > 80 Then
                    preview = preview.Substring(0, 80) & "..."
                End If

                Dim item As ListViewItem = lvBlocks.Items.Add(blocks(i).ToString())
                item.SubItems.Add(preview)
                item.Checked = True
                _blockByItem(item) = blocks(i)
            Next

            If blocks.Count = 0 Then
                Dim info As ListViewItem = lvBlocks.Items.Add("Keine neuen Blöcke")
                info.SubItems.Add("Inhalt unterscheidet sich, aber keine klaren neuen Zeilen gefunden.")
                info.Checked = False
            End If
        End Sub

        Private Sub ApplySelectedBlocks_Click(ByVal sender As Object, ByVal e As EventArgs)
            If _activeDiff Is Nothing Then
                MessageBox.Show(Me, "Bitte zuerst eine Datei im Änderungsbaum auswählen.")
                Return
            End If

            If Not SyncEngine.CanPartialSync(_activeDiff) Then
                Dim dr As DialogResult = MessageBox.Show(Me, "Für diese Datei ist nur Vollsynchronisierung möglich. Komplett synchronisieren?", "Hinweis", MessageBoxButtons.YesNo)
                If dr = DialogResult.Yes Then
                    SyncEngine.SyncFullFile(_activeDiff)
                    MessageBox.Show(Me, "Datei vollständig synchronisiert.")
                End If
                Return
            End If

            Dim selectedBlocks As List(Of LineBlock) = New List(Of LineBlock)()
            Dim i As Integer
            For i = 0 To lvBlocks.Items.Count - 1
                Dim item As ListViewItem = lvBlocks.Items(i)
                If item.Checked AndAlso _blockByItem.ContainsKey(item) Then
                    selectedBlocks.Add(_blockByItem(item))
                End If
            Next

            If selectedBlocks.Count = 0 Then
                MessageBox.Show(Me, "Bitte mindestens einen Block auswählen.")
                Return
            End If

            SyncEngine.ApplySelectedBlocks(_activeDiff.SourceFile, _activeDiff.TargetFile, selectedBlocks)
            MessageBox.Show(Me, "Ausgewählte Blöcke wurden in die Zieldatei übernommen.")

            Dim sourceText As String = ReadFileSafe(_activeDiff.SourceFile)
            Dim targetText As String = ReadFileSafe(_activeDiff.TargetFile)
            DiffUtil.HighlightDifferences(rtbSource, rtbTarget, sourceText, targetText)
            LoadBlocksForDiff(_activeDiff)
        End Sub

        Private Function ReadFileSafe(ByVal filePath As String) As String
            If Not File.Exists(filePath) Then
                Return String.Empty
            End If

            Try
                Return File.ReadAllText(filePath)
            Catch ex As Exception
                Return "[Datei kann nicht als Text angezeigt werden: " & ex.Message & "]"
            End Try
        End Function
    End Class
End Namespace
