import face_recognition
import cv2
import numpy as np

import cv2
import face_recognition
import numpy as np
from datetime import datetime
import threading
import time
import socket

# Initialize webcam feed
video_capture = cv2.VideoCapture(0)

# Load known face image and encoding
# abdelellah_image = face_recognition.load_image_file("C:\\Users\\Abdelelah Hazem\\Downloads\\Face Recognition (1)\\people//Abdelellah.jpg")
abdelellah_image = face_recognition.load_image_file("C:\\Users\\maraw\\OneDrive\\Documents\\GitHub\\HCI-Traffic-Guidance\\Face Recognition hci\\people\\Abdelellah.jpg")
omar_image = face_recognition.load_image_file("C:\\Users\\maraw\\OneDrive\\Documents\\GitHub\\HCI-Traffic-Guidance\\Face Recognition hci\\people\\Omar.jpg")
mostafa_image = face_recognition.load_image_file("C:\\Users\\maraw\\OneDrive\\Documents\\GitHub\\HCI-Traffic-Guidance\\Face Recognition hci\\people\\Mostafa.jpeg")
abdelrahman_image = face_recognition.load_image_file("C:\\Users\\maraw\\OneDrive\\Documents\\GitHub\\HCI-Traffic-Guidance\\Face Recognition hci\\people\\Abdelrahman.jpeg")

abdelellah_face_encoding = face_recognition.face_encodings(abdelellah_image)[0]
omar_face_encoding = face_recognition.face_encodings(omar_image)[0]
mostafa_face_encoding = face_recognition.face_encodings(mostafa_image)[0]
abdelrahman_face_encoding = face_recognition.face_encodings(abdelrahman_image)[0]

# Known faces
known_face_encodings = [abdelellah_face_encoding, omar_face_encoding, mostafa_face_encoding, abdelrahman_face_encoding]
known_face_names = ["Abdelellah Hazem", "Omar", "Mostafa", "Abdelrahman"]

# Distance threshold
threshold = 0.6  # Adjust this value as needed for stricter or more lenient matching

# Initialize variables
face_locations = []
face_encodings = []
face_names = []
process_this_frame = True
user_recognised = False

mySocket = socket.socket()   
def connect_socket():
    global conn,addr
    mySocket.bind(('127.0.0.1', 3333))
    mySocket.listen(5)
    conn , addr = mySocket.accept()
    print("device connected")

socket_thread = threading.Thread(target=connect_socket, daemon=True)
socket_thread.start()

while True:
    # Grab a single frame of video
    ret, frame = video_capture.read()
    if not ret:  
        print("Failed to grab frame. Exiting...")
        break

    # Process every other frame to save time
    if process_this_frame:
        # Resize frame to 1/4 size for faster processing
        small_frame = cv2.resize(frame, (0, 0), fx=0.25, fy=0.25)
        rgb_small_frame = cv2.cvtColor(small_frame, cv2.COLOR_BGR2RGB)

        # Detect faces
        face_locations = face_recognition.face_locations(rgb_small_frame)
        face_encodings = face_recognition.face_encodings(rgb_small_frame, face_locations)

        face_names = []
        for face_encoding in face_encodings:
            matches = face_recognition.compare_faces(known_face_encodings, face_encoding)
            name = "None"  # Default to None for unrecognized faces
            prevname = ""

            # Calculate face distances
            face_distances = face_recognition.face_distance(known_face_encodings, face_encoding)
            best_match_index = np.argmin(face_distances)

            # Use threshold to determine if the match is valid
            if face_distances[best_match_index] < threshold:
                name = known_face_names[best_match_index]

            face_names.append(name)

            if prevname != name and not user_recognised:
                msg = bytes(name.lower(), 'utf-8')
                try:
                    conn.send(msg)
                    print(name)
                except Exception as e:
                    print(f"Error sending message: {e}")
                prevname = name
                user_recognised = True

                # Log recognized faces
                if name != "None":
                    print(f"Hello, {name}! You have been recognized.")
                    with open("log.txt", "a") as log_file:
                        log_file.write(f"{name} recognized at {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")

    process_this_frame = not process_this_frame

    # Draw results
    for (top, right, bottom, left), name in zip(face_locations, face_names):
        # Scale back up face locations to the original frame size
        top *= 4
        right *= 4
        bottom *= 4
        left *= 4

        # Draw the box around the face
        cv2.rectangle(frame, (left, top), (right, bottom), (0, 0, 255), 2)

        # Display the name above the face box
        cv2.putText(frame, name, (left, top - 10), cv2.FONT_HERSHEY_DUPLEX, 1.0, (0, 255, 0), 2)

    # Display the frame
    cv2.imshow('Video', frame)

    # Break on 'q'
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Cleanup
video_capture.release()
cv2.destroyAllWindows()
