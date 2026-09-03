# Chess Analysis Studio — Interactive Chess (WinForms)
A chess game built in **C# WinForms**, playable against a human-like AI
opponent powered by the **Stockfish** engine.

> **Disclaimer:** This project was developed for educational,
> non-commercial purposes.

---

## Overview
This project implements a full chess board with move validation, an
interactive graphical interface, and the option to play against multiple
chess bots based on **Stockfish**, one of the strongest open-source chess
engines available.
It also includes a post-game analysis mode powered by Stockfish's
evaluation engine (expected to be included shortly).
The application follows the **MVP (Model-View-Presenter)** pattern to
separate game logic from the UI, making the board, the rules engine, and
the Stockfish integration independently testable.

---

## Features
- Interactive chessboard (WinForms UI)
- Play against Stockfish-powered AI opponents
- Multiple named AI opponents with distinct ELO ratings and playstyles
  (Andrea 800 ELO, Natasha 1500 ELO, Alejandra 2200 ELO)
- Live evaluation score display during the match
- In-game move history panel
- Undo move
- Move validation according to standard chess rules
- Post-game move analysis using Stockfish evaluation
- MVP architecture (Presenter / View separation)

---

## Screenshots

![Main Menu](Ajedrez%20interactuable%20con%20form/Recursos/Referencias/Menu.png)

![Gameplay](Ajedrez%20interactuable%20con%20form/Recursos/Referencias/Partida.png)

---

## Requirements
- .NET 8
- Visual Studio 2022 (or later)
- Stockfish engine binary (**not included in this repository** — download
  it from [stockfishchess.org](https://stockfishchess.org/download/) and
  place `stockfish.exe` inside the `Stockfish/` folder before running)

---

## Getting Started
```bash
git clone https://github.com/OscarG10101/ChessAnalysisStudio.git
```
1. Download Stockfish from [stockfishchess.org](https://stockfishchess.org/download/)
   and place `stockfish.exe` in the `Stockfish/` folder at the project root.
2. Open the solution in Visual Studio 2022.
3. Run the project (`F5`).
4. From the main menu, choose your opponent: Andrea, Natasha, or Alejandra.
5. Play chess.

---

## Project Structure
```
Ajedrez interactuable con form
│
├── Modelos/
│   ├── Movimiento.cs
│   ├── Pieza.cs
│   ├── ResultadoPartida.cs
│   ├── TableroAjedrez.cs
│   └── TipoRival.cs
├── Recursos/
│   └── Imagenes/
├── Servicios/
│   ├── PresentadorMenu.cs
│   ├── PresentadorPartida.cs
│   ├── StockfishAnalista.cs
│   └── StockfishMotor.cs
├── Stockfish/
│   └── stockfish.exe
├── Vistas/
│   ├── FormMenu.cs
│   ├── FormPartida.cs
│   ├── GloboComic.cs
│   ├── IVistaMenu.cs
│   ├── IVistaPartida.cs
│   └── SmoothPictureBox.cs
├── Program.cs
├── README.md
└── ...
```

---

## Architecture
The project follows an **MVP (Model-View-Presenter)** structure:

- **Modelos/** — game state and rules:
  - `TableroAjedrez.cs` — board state and game logic
  - `Pieza.cs` — piece representation
  - `Movimiento.cs` — move representation/validation
  - `ResultadoPartida.cs` — game outcome (checkmate, draw, etc.)
  - `TipoRival.cs` — opponent type (Human / Stockfish)
- **Vistas/** — WinForms views (pure UI, no game logic):
  - `FormMenu.cs` / `IVistaMenu.cs` — main menu
  - `FormPartida.cs` / `IVistaPartida.cs` — game screen
  - `GloboComic.cs` — speech-bubble UI element used to display in-game
    comments/messages to the player during the match
  - `SmoothPictureBox.cs` — custom `PictureBox` with double buffering and
    a smooth zoom animation (used e.g. to highlight a piece on
    hover/selection)
  - `SmoothPanel.cs` — double-buffered `Panel` to avoid flicker during
    redraws
- **Servicios/** — Presenters and engine integration:
  - `PresentadorMenu.cs` — menu logic, opponent selection
  - `PresentadorPartida.cs` — in-game logic, coordinates board + engine
  - `StockfishMotor.cs` — wraps Stockfish for move generation
  - `StockfishAnalista.cs` — wraps Stockfish for post-game analysis
- **Recursos/Imagenes/** — piece sprites and UI images
- **Stockfish/** — engine binary required at runtime (not tracked in git)

---

## How to Play
1. From the main menu, select your opponent (Andrea, Natasha, or
   Alejandra) — each with a different ELO rating and playstyle.
2. Click a piece to select it, then click the destination square to move.
3. In-game comments/hints appear via the speech-bubble UI during the
   match, alongside a live evaluation score.
4. Use the **Deshacer** (Undo) button to take back a move.
5. After the match ends, trigger post-game analysis via the pop-up
   button shown at game end.

---

## Technologies
- C# / .NET 8
- Windows Forms
- Stockfish (external chess engine)

---

## Future Improvements
- Complete post-game analysis mode
- Add more customizations to the chess board and pieces
- Save/load games (PGN)
- GUI for Database analysis

---

## Credits
- **Stockfish** — open-source chess engine used for AI opponents and
  post-game analysis, licensed under GPLv3. See
  [stockfishchess.org](https://stockfishchess.org/) for details. The
  engine binary is not distributed with this repository and must be
  downloaded separately.

---

## License
This project is licensed under the MIT License.
