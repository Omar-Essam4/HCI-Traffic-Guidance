import asyncio
from bleak import BleakScanner, BleakClient

# Define an RSSI threshold that approximately corresponds to 4 meters
RSSI_THRESHOLD = -50  # Adjust based on testing in your environment

async def connect_to_device(device_address):
    async with BleakClient(device_address) as client:
        if await client.is_connected():
            print(f"Successfully connected to device with address {device_address}")
            # Add any further interaction code with the device here
        else:
            print("Failed to connect to the device.")

async def scan_and_connect():
    print("Scanning for Bluetooth devices...")
    devices = await BleakScanner.discover()

    for device in devices:
        if device.rssi >= RSSI_THRESHOLD:  # Filter based on RSSI
            print(f"Connecting to device {device.name or 'Unknown'} with RSSI: {device.rssi}")
            await connect_to_device(device.address)
            break  # Exit after connecting to the first nearby device
        else:
            print(f"Device {device.name or 'Unknown'} is out of range (RSSI: {device.rssi}).")

if __name__ == "__main__":
    asyncio.run(scan_and_connect())
