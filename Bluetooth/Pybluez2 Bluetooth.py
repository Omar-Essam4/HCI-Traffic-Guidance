import bluetooth
import socket

# Path to the file containing allowed MAC addresses
mostafa_path = "C:/Users/mosta/OneDrive/Documents/GitHub/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
omar_path = "C:/Users/omar3/Downloads/Compressed/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
allowed_devices_file = omar_path
# allowed_devices_file = mostafa_path

# Load allowed MAC addresses from the text file
def load_allowed_devices(file_path):
    with open(file_path, "r") as file:
        return {line.strip() for line in file}

# Scan for Bluetooth devices and connect to allowed ones
def scan_and_connect():
    allowed_devices = load_allowed_devices(allowed_devices_file)
    print("Scanning for Bluetooth devices...")
    
    # Discover nearby devices
    devices = bluetooth.discover_devices(duration=20, lookup_names=True, flush_cache=True, lookup_class=False)
    
    for addr, name in devices:
        try:
            print(f"Found device: {name} - {addr}")
            
            # Check if the device is in the allowed list
            if addr in allowed_devices:
                print(f"Device {name} ({addr}) is in the allowed list.")
                
                mySocket = socket.socket()
                def connect_socket():
                    global conn,addr
                    mySocket.bind(('127.0.0.1', 3333))
                    mySocket.listen(5)
                    conn , addr = mySocket.accept()
                    print("device connected")
                    
                connect_socket()
                
                msg =bytes("device connected", 'utf-8')
                conn.send(msg)
                
            else:
                print(f"Device {name} ({addr}) is NOT in the allowed list.")
        except Exception as e:
            print(f"Error while processing device {addr}: {e}")

# Run the scanning and connecting function
scan_and_connect()
