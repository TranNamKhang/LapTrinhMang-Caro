from .constants import EMPTY, P1, P2

def in_bounds(board, r, c):
    return 0 <= r < len(board) and 0 <= c < len(board[0])

def is_valid_move(board, r, c):
    return in_bounds(board, r, c) and board[r][c] == EMPTY

def apply_move(board, r, c, player):
    if not is_valid_move(board, r, c):
        raise ValueError("Invalid move")
    board[r][c] = player
