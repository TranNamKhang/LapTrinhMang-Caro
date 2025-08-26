from typing import List
import json
from .constants import EMPTY, P1, P2

Board = List[List[int]]

def create_board(rows: int = 20, cols: int = 20) -> Board:
    return [[EMPTY for _ in range(cols)] for _ in range(rows)]

def serialize_board(board: Board) -> str:
    return json.dumps(board)

def deserialize_board(s: str) -> Board:
    return json.loads(s)

def pretty(board: Board) -> str:
    symbols = {EMPTY: ".", P1: "X", P2: "O"}
    return "\n".join(" ".join(symbols[c] for c in row) for row in board)
