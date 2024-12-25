import cv2
import numpy as np
import screen_brightness_control as sbc

def calculate_brightness(frame):
    # Convert the frame to grayscale for brightness estimation
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    return np.mean(gray)

def adjust_brightness_based_on_light(ambient_light):
    # Map the ambient light level to a screen brightness range (0 to 100)
    brightness = int(np.clip(ambient_light / 2.5, 10, 100))
    sbc.set_brightness(brightness)
    print(f"Ambient light level: {ambient_light:.2f}, Adjusted brightness: {brightness}%")

# Open the webcam
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

try:
    while True:
        ret, frame = cap.read()
        if not ret:
            print("Error: Could not read frame.")
            break
        
        # Calculate the ambient light level
        ambient_light = calculate_brightness(frame)
        
        # Adjust screen brightness
        adjust_brightness_based_on_light(ambient_light)
        
        # Display the camera feed (optional)
        cv2.imshow('Camera Feed', frame)
        
        # Exit on pressing 'q'
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
finally:
    # Release the camera and close windows
    cap.release()
    cv2.destroyAllWindows()
