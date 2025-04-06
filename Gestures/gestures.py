import pickle
import mediapipe as mp  # Import mediapipe
import cv2  # Import opencv
from dollarpy import Recognizer, Point
import socket
import threading
import time

mp_drawing = mp.solutions.drawing_utils  # Drawing helpers
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic  # Mediapipe Solutions

# Load templates with pickle
def load_templates(filename):
    try:
        with open(filename, 'rb') as f:
            templates = pickle.load(f)
        print("Templates loaded from", filename)
        return templates
    except FileNotFoundError:
        print("No templates found.")
        return []

mySocket = socket.socket()
conn = None  # Will hold the client connection

def socket_thread():
    global conn, addr
    try:
        mySocket.bind(('127.0.0.1', 3333))
        mySocket.listen(5)
        print("Waiting for device to connect...")
        conn, addr = mySocket.accept()
        print("Device connected from", addr)
    except Exception as e:
        print(f"Socket error: {e}")

templates = load_templates("templates.pkl")
templates2 = load_templates("templates2.pkl")

recognizer = Recognizer(templates)
recognizer2 = Recognizer(templates2)

def testpoints():
    print("testpoints thread started")
    cap = cv2.VideoCapture(0)  # Webcam = 0

    if not cap.isOpened():
        print("Error: Camera could not be opened.")
        return

    with mp_holistic.Holistic(min_detection_confidence=0.35, min_tracking_confidence=0.35) as holistic:
        points_buffer = []
        gesture = "none"
        prevgesture = ""

        while cap.isOpened():
            ret, frame = cap.read()

            if ret:
                image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                image.flags.writeable = False

                # Make detections
                results = holistic.process(image)

                # Recolor image back to BGR for rendering
                image.flags.writeable = True
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

                # Drawing landmarks if detected
                if results.right_hand_landmarks:
                    mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

                if results.left_hand_landmarks:
                    mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

                try:
                    if results.right_hand_landmarks:
                        points = [Point(lm.x, lm.y, 1) for lm in results.right_hand_landmarks.landmark]
                        points_buffer.append(points)

                        if len(points_buffer) >= 12:
                            flattened_points = [pt for frame in points_buffer for pt in frame]

                            result1 = recognizer.recognize(flattened_points)
                            result2 = recognizer2.recognize(flattened_points)

                            if result1[0]:
                                gesture = result1[0]
                            elif result2[0]:
                                gesture = result2[0]
                            else:
                                gesture = "Unknown"

                            if gesture != prevgesture and conn:
                                try:
                                    conn.send(bytes(gesture.lower(), 'utf-8'))
                                    print(f"Sent gesture: {gesture}")
                                except Exception as send_err:
                                    print(f"Send error: {send_err}")
                                prevgesture = gesture

                            points_buffer.clear()

                        cv2.putText(
                            image, f"Gesture: {gesture}", (50, 50),
                            cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA
                        )
                except Exception as e:
                    print(f"Error: {e}")

                # Display the image
                cv2.imshow('Gesture Recognition', image)
            else:
                break

            if cv2.waitKey(10) & 0xFF == ord('q'):
                break

        cap.release()
        cv2.destroyAllWindows()

# Start socket server thread
threading.Thread(target=socket_thread, daemon=True).start()

# Start camera gesture recognition thread
threading.Thread(target=testpoints).start()

