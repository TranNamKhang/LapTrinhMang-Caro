import socket
import threading

def receive_messages(sock):
    while True:
        try:
            msg = sock.recv(1024).decode()
            if msg:
                print(msg)
            else:
                break
        except:
            break

def main():
    name = input("Nhập tên của bạn: ")
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(('127.0.0.1', 8888))  # Đổi IP nếu khác máy
    print("Đã kết nối thành công!")
    print("Lệnh: /chat noi dung")
    sock.send(name.encode())

    threading.Thread(target=receive_messages, args=(sock,), daemon=True).start()

    while True:
        inp = input()
        if inp.startswith("/chat "):
            chat_msg = "[Chat] " + inp[6:]
            sock.send(chat_msg.encode())

if __name__ == "__main__":
    main()