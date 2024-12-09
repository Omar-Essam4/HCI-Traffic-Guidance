import cv2
from ultralytics import YOLO
import socket
import threading

# Load the YOLO model
# model = YOLO(r"C:\\Users\\Abdelelah Hazem\\Downloads\\Detection\\models\\yolov8n.pt")  # Replace with your model's path
model = YOLO(r"C:\\Users\\omar3\\Downloads\\Compressed\\HCI-Traffic-Guidance\\object detection\\yolov8n.pt")  # Replace with your model's path
prevlabel = ""
label = "none"

mySocket = socket.socket()
conn = None  # Global variable for the client connection


def listen_for_connections():
    """Thread function to listen for client connections."""
    global conn, addr
    mySocket.bind(('127.0.0.1', 3334))
    mySocket.listen(5)
    print("Server is listening for connections...")

    while True:
        try:
            conn, addr = mySocket.accept()
            print(f"Device connected: {addr}")
        except Exception as e:
            print(f"Error accepting connection: {e}")


# Start the connection thread
threading.Thread(target=listen_for_connections, daemon=True).start()

# Initialize webcam
cap = cv2.VideoCapture(0)  # 0 is the default camera. Use 1 or higher for external cameras.

if not cap.isOpened():
    print("Error: Could not open webcam.")
    exit()

# Process frames from the webcam
try:
    while True:
        ret, frame = cap.read()
        if not ret:
            print("Error: Failed to capture frame.")
            break

        # Perform object detection
        results = model(frame, conf=0.5)  # Confidence threshold set to 50%

        # Annotate the frame with bounding boxes and labels
        for box in results[0].boxes:
            # Extract bounding box coordinates, class ID, and confidence
            xyxy = box.xyxy[0].cpu().numpy()  # Bounding box (x1, y1, x2, y2)
            cls = int(box.cls[0])            # Class ID
            conf = float(box.conf[0])        # Confidence score
            label = model.names[cls]         # Class label

            # Convert box coordinates to integers
            x1, y1, x2, y2 = map(int, xyxy)

            # Draw the bounding box
            cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)

            # Display label and confidence score
            label_text = f"{label} ({conf:.2f})"
            cv2.putText(frame, label_text, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX,
                        0.5, (0, 255, 0), 2)

            # Send data only when a client is connected
            if conn and label != prevlabel:
                try:
                    if label == "traffic light":
                        msg = bytes("map", 'utf-8')
                        conn.send(msg)
                        prevlabel = label
                    elif label == "car":
                        msg = bytes("traffic", 'utf-8')
                        conn.send(msg)
                        prevlabel = label
                    elif label == "bus":
                        msg = bytes(label, 'utf-8')
                        conn.send(msg)
                        prevlabel = label
                    elif label == "train":
                        msg = bytes(label, 'utf-8')
                        conn.send(msg)
                        prevlabel = label
                except Exception as e:
                    print(f"Error sending data: {e}")
                    conn = None  # Reset connection if sending fails

        # Show the annotated frame
        cv2.imshow("YOLO Object Detection", frame)

        # Press 'q' to exit the loop
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
finally:
    # Release resources
    cap.release()
    cv2.destroyAllWindows()
    mySocket.close()
    print("Resources released and socket closed.")
