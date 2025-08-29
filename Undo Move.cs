using System;
using System.Collections.Generic;
using System.Linq;

namespace Caro.Core
{
    /// <summary>
    /// Cell states for a Caro (Gomoku) board.
    /// </summary>
    public enum Cell
    {
        Empty = 0,
        X = 1, // Player 1
        O = 2  // Player 2
    }

    /// <summary>
    /// Immutable move description.
    /// </summary>
    public readonly record struct Move(int Row, int Col, Cell Player)
    {
        public override string ToString() => $"{Row},{Col},{(int)Player}";
        public static bool TryParse(string s, out Move move)
        {
            move = default;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out var r)) return false;
            if (!int.TryParse(parts[1], out var c)) return false;
            if (!int.TryParse(parts[2], out var p)) return false;
            move = new Move(r, c, (Cell)p);
            return true;
        }
    }

    /// <summary>
    /// Drop-in Caro game core with Undo/Redo stacks.
    /// - Works for offline and online modes (history can be serialized).
    /// - Thread safe for UI use if you call it from the UI thread.
    /// - Does not implement win detection (keep yours as-is and call it after Place()).
    /// </summary>
    public class CaroGame
    {
        public int Size { get; }
        public Cell[,] Board { get; }
        public Cell CurrentPlayer { get; private set; } = Cell.X;

        // History stacks
        private readonly Stack<Move> _history = new();
        private readonly Stack<Move> _redo = new();

        // Events to update UI/network
        public event Action<Move>? MovePlaced;
        public event Action<Move>? MoveUndone;
        public event Action<Move>? MoveRedone;
        public event Action? Cleared;

        public CaroGame(int size = 15)
        {
            if (size < 5 || size > 50) throw new ArgumentOutOfRangeException(nameof(size), "Board size should be between 5 and 50.");
            Size = size;
            Board = new Cell[Size, Size];
        }

        /// <summary>
        /// Try to place the current player's mark at (row, col).
        /// On success: history += move, redo is cleared, current player toggles.
        /// </summary>
        public bool TryPlace(int row, int col, out Move move)
        {
            move = default;
            if (!IsInside(row, col)) return false;
            if (Board[row, col] != Cell.Empty) return false;

            move = new Move(row, col, CurrentPlayer);
            Board[row, col] = CurrentPlayer;
            _history.Push(move);
            _redo.Clear();
            TogglePlayer();
            MovePlaced?.Invoke(move);
            return true;
        }

        /// <summary>
        /// Undo the last move. Returns false if history is empty.
        /// </summary>
        public bool Undo(out Move undone)
        {
            undone = default;
            if (_history.Count == 0) return false;

            undone = _history.Pop();
            Board[undone.Row, undone.Col] = Cell.Empty;
            _redo.Push(undone);
            TogglePlayer(); // Revert turn back to the player who made the undone move
            MoveUndone?.Invoke(undone);
            return true;
        }

        /// <summary>
        /// Redo the last undone move. Returns false if redo stack is empty.
        /// </summary>
        public bool Redo(out Move redone)
        {
            redone = default;
            if (_redo.Count == 0) return false;

            redone = _redo.Pop();
            Board[redone.Row, redone.Col] = redone.Player;
            _history.Push(redone);
            TogglePlayer();
            MoveRedone?.Invoke(redone);
            return true;
        }

        /// <summary>
        /// Clear board and history. Useful when starting a new match.
        /// </summary>
        public void Reset(Cell starting = Cell.X)
        {
            Array.Clear(Board, 0, Board.Length);
            _history.Clear();
            _redo.Clear();
            CurrentPlayer = starting;
            Cleared?.Invoke();
        }

        public IReadOnlyList<Move> History => _history.Reverse().ToList();
        public IReadOnlyList<Move> RedoStack => _redo.Reverse().ToList();

        public bool CanUndo => _history.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        private void TogglePlayer() => CurrentPlayer = (CurrentPlayer == Cell.X) ? Cell.O : Cell.X;
        private bool IsInside(int r, int c) => r >= 0 && r < Size && c >= 0 && c < Size;

        #region Serialization helpers (for networking / save-load)
        /// <summary>
        /// Serialize move history to a compact string. Example: "7,8,1|7,9,2|..."
        /// Send this over the network or save to file.
        /// </summary>
        public string SerializeHistory() => string.Join('|', _history.Reverse().Select(m => m.ToString()));

        /// <summary>
        /// Load history onto a cleared board. Use only on a fresh game state.
        /// </summary>
        public void LoadHistory(string history, Cell starting = Cell.X)
        {
            Reset(starting);
            if (string.IsNullOrWhiteSpace(history)) return;
            var parts = history.Split('|');
            foreach (var p in parts)
            {
                if (Move.TryParse(p, out var mv))
                {
                    // Place respecting stored player, not CurrentPlayer
                    if (!IsInside(mv.Row, mv.Col) || Board[mv.Row, mv.Col] != Cell.Empty)
                        throw new InvalidOperationException("Invalid move in history.");

                    Board[mv.Row, mv.Col] = mv.Player;
                    _history.Push(mv);
                    CurrentPlayer = (mv.Player == Cell.X) ? Cell.O : Cell.X; // next turn
                }
            }
        }
        #endregion
    }
}

/* ========================= HOW TO USE (example) =========================
// Create game
var game = new Caro.Core.CaroGame(size: 15);

// Place move
if (game.TryPlace(7, 7, out var m1))
{
    // Update UI, check win, etc.
}

// Undo/Redo
if (game.Undo(out var undone)) { /* refresh UI */ }
if (game.Redo(out var redone)) { /* refresh UI */ }

// Disable/enable buttons
undoButton.Enabled = game.CanUndo;
redoButton.Enabled = game.CanRedo;
======================================================================== */
