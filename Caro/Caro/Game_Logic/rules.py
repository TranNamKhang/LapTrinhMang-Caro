from .constants import DIRECTIONS, EMPTY, P1, P2
from .move import in_bounds

def count_dir(board, r, c, dr, dc, player):
    cnt = 0
    rr, cc = r + dr, c + dc
    while in_bounds(board, rr, cc) and board[rr][cc] == player:
        cnt += 1
        rr += dr; cc += dc
    return cnt

def check_win(board, r, c, player, win_len=5):
    for dr, dc in DIRECTIONS:
        total = 1 + count_dir(board, r, c, dr, dc, player) + count_dir(board, r, c, -dr, -dc, player)
        if total >= win_len:
            return True
    return False

def check_draw(board):
    return all(cell != EMPTY for row in board for cell in row)

def get_winner(board, last_move, last_player, win_len=5):
    if last_move and last_player in (P1, P2):
        r, c = last_move
        if check_win(board, r, c, last_player, win_len):
            return last_player
    return None
