# FileSyncApp (Visual Basic / WinForms für Visual Studio 2022)

Windows-Desktop-App zum Synchronisieren mehrerer Dateipfade:

- Mehrere **Quellpfade** verwalten (hinzufügen/entfernen).
- Für jede Quelle mehrere **Zielpfade** hinterlegen (hinzufügen/entfernen).
- Letzte Quellen/Ziele werden automatisch gespeichert und beim nächsten Start wieder geladen.
- Unterschiede (neu / geändert) als **TreeView** anzeigen.
- Änderungen werden im TreeView **ordnerbasiert gruppiert** dargestellt (Ordner → Dateien).
- Integrierter einfacher **Diff-Editor** (2 Textansichten mit Highlighting).
- **Blockweise Synchronisation** für Textdateien:
  - Datei im TreeView auswählen
  - erkannte neue Blöcke (ähnlich Hunk-Auswahl) in Liste prüfen
  - ausgewählte Blöcke gezielt übernehmen
- Vollsynchronisierung weiterhin per Button für markierte oder alle Ziele.

## Entwicklung mit Visual Studio 2022

1. `FileSyncApp.sln` in Visual Studio 2022 öffnen.
2. Falls angefordert: .NET 8 Desktop Runtime/SDK installieren.
3. Starten (`F5`).

## CLI Build

```bash
dotnet build FileSyncApp.sln
```

## Hinweis zu BC30420 (Sub Main nicht gefunden)

Wenn `StartupObject` auf `FileSyncApp.Program` zeigt, darf der Projekt-`RootNamespace` nicht zusätzlich denselben Namespace voranstellen. Deshalb ist `RootNamespace` bewusst leer gesetzt.
