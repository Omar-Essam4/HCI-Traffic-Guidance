import pickle
import mediapipe as mp
import cv2
from dollarpy import Recognizer, Point
import socket
import threading
import time

# === Load Gesture Templates ===
def load_templates(filename):
    try:
        with open(filename, 'rb') as f:
            templates = pickle.load(f)
        print("Templates loaded from", filename)
        return templates
    except FileNotFoundError:
        print("No templates found.")
        return []

templates = load_templates("templates.pkl")
templates2 = load_templates("templates2.pkl")

recognizer = Recognizer(templates)
recognizer2 = Recognizer(templates2)

# === Socket Setup ===
def connect_socket():
    s = socket.socket()
    s.connect(('127.0.0.1', 5555))
    print("Connected to C# Gesture Receiver")
    return s

conn = None

# === Mediapipe Setup ===
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic

# === Gesture Detection ===
def testpoints():
    global conn
    print("Gesture detection started")
    cap = cv2.VideoCapture(0)

    if not cap.isOpened():
        print("Error: Camera could not be opened.")
        return

    conn = connect_socket()

    with mp_holistic.Holistic(min_detection_confidence=0.35, min_tracking_confidence=0.35) as holistic:
        points_buffer = []
        last_gesture = None

        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image.flags.writeable = False
            results = holistic.process(image)
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

            gesture = "None"

            if results.right_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS
                )

                try:
                    points = [Point(lm.x, lm.y, 1) for lm in results.right_hand_landmarks.landmark]
                    points_buffer.append(points)

                    if len(points_buffer) >= 12:
                        flat_points = [pt for frame in points_buffer for pt in frame]

                        result1 = recognizer.recognize(flat_points)
                        result2 = recognizer2.recognize(flat_points)

                        gesture = result1[0] if result1[0] else result2[0] if result2[0] else "Unknown"

                        if gesture != last_gesture:
                            print(f"Gesture: {gesture}")
                            if conn:
                                try:
                                    conn.send(gesture.lower().encode())
                                    print("Gesture sent.")
                                except Exception as e:
                                    print(f"Socket error: {e}. Reconnecting...")
                                    try:
                                        conn.close()
                                    except:
                                        pass
                                    conn = connect_socket()

                            last_gesture = gesture

                        points_buffer.clear()

                except Exception as e:
                    print(f"Error processing gesture: {e}")

            if results.left_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS
                )

            # Display gesture on screen
            cv2.putText(
                image, f"Gesture: {gesture}", (50, 50),
                cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2
            )

            # Show webcam image
            cv2.imshow('Gesture Recognition', image)

            if cv2.waitKey(10) & 0xFF == ord('q'):
                break

        cap.release()
        cv2.destroyAllWindows()
        if conn:
            conn.close()

# === Run Gesture Detection ===
if __name__ == "__main__":
    testpoints()
