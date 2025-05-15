import cv2
from ultralytics import YOLO
import socket
import threading

# === Socket Setup ===
def connect_socket():
    mySocket = socket.socket()
    mySocket.connect(('127.0.0.1', 5555))  # Connect to C# server on the same port as gaze
    print("Connected to C# Receiver from YOLO")
    return mySocket

# === Load YOLO model ===
model = YOLO(r"C:\\Users\\maraw\\OneDrive\\Documents\\GitHub\\HCI-Traffic-Guidance\\object detection\\yolov8n.pt")

# === Initialize webcam ===
cap = cv2.VideoCapture(1)

if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

prev_label = ""
conn = None

try:
    conn = connect_socket()
    
    while True:
        ret, frame = cap.read()
        if not ret:
            print("Error: Failed to capture frame.")
            break

        # Run detection
        results = model(frame, conf=0.5)

        for box in results[0].boxes:
            xyxy = box.xyxy[0].cpu().numpy()
            cls = int(box.cls[0])
            conf = float(box.conf[0])
            label = model.names[cls]

            x1, y1, x2, y2 = map(int, xyxy)
            label_text = f"{label} ({conf:.2f})"

            # Draw box and label
            cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
            cv2.putText(frame, label_text, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX,
                        0.5, (0, 255, 0), 2)

            # Send to C# if label changed
            if label != prev_label:
                try:
                    conn.send(label.encode())
                    print(f"Sent to C#: {label}")
                    prev_label = label
                except Exception as e:
                    print(f"Error sending data: {e}")
                    conn.close()
                    conn = connect_socket()

        # Show frame
        cv2.imshow("YOLO Object Detection", frame)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

finally:
    cap.release()
    cv2.destroyAllWindows()
    if conn:
        conn.close()
