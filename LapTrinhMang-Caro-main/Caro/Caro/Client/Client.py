import socket
import threading
import tkinter as tk
from tkinter import messagebox, simpledialog

# ===== Config =====
PORT = 5000
BOARD_SIZE = 15
CELL = 30
PADDING = 20
CANVAS_SIZE = PADDING * 2 + CELL * (BOARD_SIZE - 1)
