import socket
import threading

clients = []
client_names = []

def broadcast(msg):
    for client in clients:
        try:
            client.send(msg.encode())
        except:
            pass

def handle_client(client):
    try:
        name = client.recv(1024).decode()
        client_names.append(name)
        clients.append(client)
        broadcast(f"[Hệ thống] {name} đã tham gia phòng!")
        print(f"[Hệ thống] {name} đã tham gia phòng!")

        while True:
            msg = client.recv(1024).decode()
            if not msg:
                break
            broadcast(f"{name}: {msg}")
            print(f"{name}: {msg}")
    except:
        pass
    finally:
        if client in clients:
            idx = clients.index(client)
            left_name = client_names[idx]
            clients.remove(client)
            client_names.remove(left_name)
            broadcast(f"[Hệ thống] {left_name} đã rời phòng!")
            print(f"[Hệ thống] {left_name} đã rời phòng!")
        client.close()

def main():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind(('0.0.0.0', 8888))
    server.listen()
    print("Server started, waiting for connection...")

    while True:
        client, addr = server.accept()
        threading.Thread(target=handle_client, args=(client,), daemon=True).start()

if __name__ == "__main__":
    main()