import asyncio
from bleak import BleakScanner, BleakClient

# Path to the file containing allowed MAC addresses
mostafa_path = "C:/Users/mosta/OneDrive/Documents/GitHub/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
omar_path = "C:/Users/omar3/Downloads/Compressed/HCI-Traffic-Guidance/Bluetooth/bluetooth_devices.txt"
allowed_devices_file = omar_path
# allowed_devices_file = mostafa_path

# Load allowed MAC addresses from the text file
def load_allowed_devices(file_path):
    with open(file_path, "r") as file:
        return {line.strip() for line in file}

# Scan for Bluetooth devices
async def scan_and_connect():
    allowed_devices = load_allowed_devices(allowed_devices_file)
    print("Scanning for Bluetooth devices...")

    # Perform the scan
    devices = await BleakScanner.discover(timeout=20)
    for device in devices:
        print(f"Found device: {device.name} - {device.address}")
        
        # Check if the device is in the allowed list
        if device.address in allowed_devices:
            print(f"Device {device.name} ({device.address}) is in the allowed list.")
            try:
                async with BleakClient(device.address) as client:
                    print(f"Connected to {device.name} ({device.address})")
                    # Here you can add logic to interact with the device if needed
            except Exception as e:
                print(f"Failed to connect to {device.name} ({device.address}): {e}")
        else:
            print(f"Device {device.name} ({device.address}) is NOT in the allowed list.")

# Run the scanning and connecting function
asyncio.run(scan_and_connect())