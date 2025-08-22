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

class CaroClient:
    def __init__(self, master):
        self.master = master
        self.master.title("Caro (Gomoku) - Multi Client")

        # --- Canvas bàn cờ ---
        self.canvas = tk.Canvas(master, width=CANVAS_SIZE, height=CANVAS_SIZE, bg="white")
        self.canvas.pack()

        # --- Thanh trạng thái ---
        self.status = tk.StringVar(value="Not connected")
        tk.Label(master, textvariable=self.status).pack(pady=5)

        # --- Nút điều khiển ---
        btn_frame = tk.Frame(master)
        btn_frame.pack()
        tk.Button(btn_frame, text="Connect", command=self.connect).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="New Game (local reset)", command=self.reset_board).pack(side=tk.LEFT, padx=5)
        tk.Button(btn_frame, text="Quit", command=master.destroy).pack(side=tk.LEFT, padx=5)

        # --- Khu vực chat ---
        chat_frame = tk.Frame(master)
        chat_frame.pack(pady=6)

        self.chat_box = tk.Text(chat_frame, height=7, width=56, state=tk.DISABLED)
        self.chat_box.pack(side=tk.LEFT)

        scroll = tk.Scrollbar(chat_frame, command=self.chat_box.yview)
        scroll.pack(side=tk.RIGHT, fill=tk.Y)
        self.chat_box['yscrollcommand'] = scroll.set

        entry_frame = tk.Frame(master)
        entry_frame.pack(pady=4)
        self.chat_entry = tk.Entry(entry_frame, width=48)
        self.chat_entry.pack(side=tk.LEFT)
        self.chat_entry.bind("<Return>", self.send_chat)
        tk.Button(entry_frame, text="Send", command=self.send_chat).pack(side=tk.LEFT, padx=6)

        # --- Binding click bàn cờ ---
        self.canvas.bind("<Button-1>", self.on_click)

        # --- Trạng thái game ---
        self.sock = None
        self.my_mark = None  # "X" or "O"
        self.turn = False
        self.board = [[None for _ in range(BOARD_SIZE)] for _ in range(BOARD_SIZE)]

        # Vẽ bàn cờ
        self.draw_grid()

    # ===== Board drawing =====
    def draw_grid(self):
        self.canvas.delete("grid")
        for i in range(BOARD_SIZE):
            x0 = PADDING + i * CELL
            self.canvas.create_line(PADDING, x0, PADDING + CELL * (BOARD_SIZE - 1), x0, tags="grid")
            self.canvas.create_line(x0, PADDING, x0, PADDING + CELL * (BOARD_SIZE - 1), tags="grid")

    def board_to_pixel(self, x, y):
        px = PADDING + x * CELL
        py = PADDING + y * CELL
        return px, py

    def pixel_to_board(self, px, py):
        x = round((px - PADDING) / CELL)
        y = round((py - PADDING) / CELL)
        return x, y

    def draw_mark(self, x, y, mark):
        px, py = self.board_to_pixel(x, y)
        offset = 10
        if mark == "X":
            self.canvas.create_line(px - offset, py - offset, px + offset, py + offset, width=2)
            self.canvas.create_line(px - offset, py + offset, px + offset, py - offset, width=2)
        else:
            self.canvas.create_oval(px - offset, py - offset, px + offset, py + offset, width=2)