import asyncio
from bleak import BleakScanner
import socket

# Path to the file containing allowed MAC addresses
#mostafa_path = "C:/Users/mosta/OneDrive/Documents/GitHub/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
omar_path = "C:/Users/maraw/OneDrive/Documents/GitHub/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
allowed_devices_file = omar_path
# allowed_devices_file = mostafa_path

# Load allowed MAC addresses from the text file
def load_allowed_devices(file_path):
    with open(file_path, "r") as file:
        return {line.strip() for line in file}

def connect_socket():
    mySocket = socket.socket()
    mySocket.bind(('127.0.0.1', 3333))
    mySocket.listen(5)
    print("Waiting for client to connect")   
    conn, addr = mySocket.accept()
    print(f"Client successfully connected from {addr}")
    return conn

# Scan for Bluetooth devices asynchronously
async def scan_and_connect():
    allowed_devices = load_allowed_devices(allowed_devices_file)
    print("Scanning for Bluetooth devices...")

    # Discover nearby Bluetooth devices
    devices = await BleakScanner.discover()

    for device in devices:
        addr, name = device.address, device.name
        try:
            if addr in allowed_devices:
                print(f"Found device: {name} - {addr}")
                print(f"Device ({addr}) is in the allowed list.")

                conn = connect_socket()
                msg = b"default_message"  # Default message

                if addr == "BC:10:7B:F3:75:4F":
                    msg = b"MARAWAN"
                elif addr == "bc:6a:d1:b7:56:e9":
                    msg = b"kholy"

                conn.send(msg)
                print("Message sent successfully")
                conn.close()
                break
            else:
                print(f"({addr}) is NOT in the allowed list.")
        except Exception as e:
            print(f"Error while processing device {addr}: {e}")

# Run the Bluetooth scanning
asyncio.run(scan_and_connect())



# import bluetooth
# import socket

# # Path to the file containing allowed MAC addresses
# mostafa_path = "C:/Users/mosta/OneDrive/Documents/GitHub/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
# omar_path = "C:/Users/omar3/Downloads/Compressed/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
# allowed_devices_file = omar_path  # Change this as needed

# # Load allowed MAC addresses from the text file
# def load_allowed_devices(file_path):
#     with open(file_path, "r") as file:
#         return {line.strip() for line in file}

# # Start a server socket to send messages to a client
# def start_server_socket():
#     mySocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#     mySocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
#     mySocket.bind(('127.0.0.1', 3333))  # Bind only to localhost
#     mySocket.listen(5)
#     print("Waiting for a client to connect on localhost...")
#     conn, addr = mySocket.accept()
#     print(f"Client connected from {addr}")
#     return conn

# # Scan for Bluetooth devices and connect to allowed ones
# def scan_and_connect():
#     allowed_devices = load_allowed_devices(allowed_devices_file)
#     print("Scanning for Bluetooth devices...")

#     # Discover nearby devices
#     devices = bluetooth.discover_devices(duration=5, lookup_names=True, flush_cache=True, lookup_class=False)

#     for addr, name in devices:
#         try:
#             print(f"Found device: {name} - {addr}")

#             # Check if the device is in the allowed list
#             if addr in allowed_devices:
#                 print(f"Device {name} ({addr}) is in the allowed list.")

#                 # Start the server socket to communicate with the client
#                 conn = start_server_socket()

#                 # Send device-specific messages
#                 if addr == "D6:E7:0B:F0:0F:D4":
#                     msg = "abdelellah"
#                 elif addr == "5C:10:C5:FB:A8:98":
#                     msg = "omar"
#                 else:
#                     msg = "Hello from server!"

#                 conn.send(msg.encode('utf-8'))  # Encode the message as bytes
#                 conn.close()  # Close the connection after sending the message
#                 print(f"Message sent to client: {msg}")
#             else:
#                 print(f"Device {name} ({addr}) is NOT in the allowed list.")
#         except Exception as e:
#             print(f"Error while processing device {addr}: {e}")

# # Run the scanning and connecting function
# scan_and_connect()
