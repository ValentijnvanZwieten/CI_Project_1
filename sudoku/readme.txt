Roep het programma aan op de volgende manier:
| sudoku.exe [Algoritme] [Parameters] < [Sudoku-bestand]

Waar [Algoritme] een uit ILS of SAS is, en [Sudoku-bestand] de naam van het textbestand wat de op te lossen sudoku bevat. [Parameters] is een uit volgende:
• ILS: TimeOut, S, WalkCount
• SAS: c, a

Enkele voorbeelden:
| sudoku.exe ILS 9 9 25 < 9x9.txt
| sudoku.exe SAS 0.5 0.999 < 16x16.txt