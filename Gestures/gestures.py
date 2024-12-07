import pickle
import mediapipe as mp # Import mediapipe
import cv2 # Import opencv
from dollarpy import Recognizer, Template, Point
import socket
import threading
import time

mp_drawing = mp.solutions.drawing_utils # Drawing helpers
mp_drawing_styles = mp.solutions.drawing_styles

mp_holistic = mp.solutions.holistic # Mediapipe Solutions

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
def connect_socket():
    global conn,addr
    mySocket.bind(('127.0.0.1', 3333))
    mySocket.listen(5)
    conn , addr = mySocket.accept()
    print("device connected")
    
templates = load_templates("templates.pkl")
templates2 = load_templates("templates2.pkl")

recognizer = Recognizer(templates)
recognizer2 = Recognizer(templates2)

connect_socket()

def testpoints():
    cap = cv2.VideoCapture(1)#web cam =0 , else enter filename
    # Initiate holistic model
    with mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        points = []
        wrist = []
        Thumb_cmc=[]
        Thumb_mcp=[]
        Thumb_ip=[]
        Thumb_tip=[]
        Index_finger_mcp=[]
        Index_finger_pip=[]
        Index_finger_dip=[]
        Index_finger_tip=[]
        Middle_finger_mcp=[]
        Middle_finger_pip=[]
        Middle_finger_dip=[]
        Middle_finger_tip=[]
        Ring_finger_mcp=[]
        Ring_finger_pip=[]
        Ring_finger_dip=[]
        Ring_finger_tip=[]
        Pinky_mcp=[]
        Pinky_pip=[]
        Pinky_dip=[]
        Pinky_tip=[]   
        frames_buffer = []  # Buffer to hold the frames
        points_buffer = []  # Buffer to hold points of 20 frames
        gesture = "none"
        prevgesture = ""
        while cap.isOpened():
            ret, frame = cap.read()

            # Recolor Feed
            if ret==True:

                image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                image.flags.writeable = False        

                # Make Detections
                results = holistic.process(image)
                # print(results.face_landmarks)


                # Recolor image back to BGR for rendering
                image.flags.writeable = True   
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

                # Drawing on Frame (You can remove it)
                # 2. Right hand
                mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

                # 3. Left Hand
                mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)

                # # 4. Pose Detections
                # mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS,landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
                # # Export coordinates

                try:

                    # add points of wrist , elbow and shoulder
                    wrist.append(Point(results.pose_landmarks.landmark[0].x,results.pose_landmarks.landmark[0].y,1))
                    Thumb_cmc.append(Point(results.pose_landmarks.landmark[1].x,results.pose_landmarks.landmark[1].y,1))
                    Thumb_mcp.append(Point(results.pose_landmarks.landmark[2].x,results.pose_landmarks.landmark[2].y,1))
                    Thumb_ip .append(Point(results.pose_landmarks.landmark[3].x,results.pose_landmarks.landmark[3].y,1))
                    Thumb_tip .append(Point(results.pose_landmarks.landmark[4].x,results.pose_landmarks.landmark[4].y,1))
                    Index_finger_mcp.append(Point(results.pose_landmarks.landmark[5].x,results.pose_landmarks.landmark[5].y,1))
                    Index_finger_pip.append(Point(results.pose_landmarks.landmark[6].x,results.pose_landmarks.landmark[6].y,1))
                    Index_finger_dip.append(Point(results.pose_landmarks.landmark[7].x,results.pose_landmarks.landmark[7].y,1))
                    Index_finger_tip.append(Point(results.pose_landmarks.landmark[8].x,results.pose_landmarks.landmark[8].y,1))
                    Middle_finger_mcp.append(Point(results.pose_landmarks.landmark[9].x,results.pose_landmarks.landmark[9].y,1))
                    Middle_finger_pip.append(Point(results.pose_landmarks.landmark[10].x,results.pose_landmarks.landmark[10].y,1))
                    Middle_finger_dip.append(Point(results.pose_landmarks.landmark[11].x,results.pose_landmarks.landmark[11].y,1))
                    Middle_finger_tip.append(Point(results.pose_landmarks.landmark[12].x,results.pose_landmarks.landmark[12].y,1))
                    Ring_finger_mcp.append(Point(results.pose_landmarks.landmark[13].x,results.pose_landmarks.landmark[13].y,1))
                    Ring_finger_pip.append(Point(results.pose_landmarks.landmark[14].x,results.pose_landmarks.landmark[14].y,1))
                    Ring_finger_dip.append(Point(results.pose_landmarks.landmark[15].x,results.pose_landmarks.landmark[15].y,1))
                    Ring_finger_tip.append(Point(results.pose_landmarks.landmark[16].x,results.pose_landmarks.landmark[16].y,1))
                    Pinky_mcp.append(Point(results.pose_landmarks.landmark[17].x,results.pose_landmarks.landmark[17].y,1))
                    Pinky_pip.append(Point(results.pose_landmarks.landmark[18].x,results.pose_landmarks.landmark[18].y,1))
                    Pinky_dip.append(Point(results.pose_landmarks.landmark[19].x,results.pose_landmarks.landmark[19].y,1))
                    Pinky_tip.append(Point(results.pose_landmarks.landmark[20].x,results.pose_landmarks.landmark[20].y,1))
                    points = wrist + Thumb_cmc + Thumb_mcp + Thumb_ip + Thumb_tip + Index_finger_mcp + Index_finger_pip + Index_finger_dip + Index_finger_tip + Middle_finger_mcp + Middle_finger_pip + Middle_finger_dip + Middle_finger_tip + Ring_finger_mcp + Ring_finger_pip + Ring_finger_dip + Ring_finger_tip + Pinky_mcp + Pinky_pip + Pinky_dip + Pinky_tip
                    points_buffer.append(points)                    

                    result = recognizer.recognize(points)
                    result2 = []
                    all_points = []
                    if len(points_buffer) == 15:
                        for frame in points_buffer:
                            for point in frame:
                                all_points.append(point)
                        if len(all_points) > 0:               
                            result2 = recognizer2.recognize(all_points)
                            if result[0] != None and result2[0] == None :
                                cv2.putText(image, f"{result[0]} {result[1]}", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)
                                gesture = result[0]

                            elif result2[0] != None:
                                cv2.putText(image, f"{result2[0]} {result2[1]}", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 255), 2, cv2.LINE_AA)
                                gesture = result2[0]

                            points_buffer = []
                            all_points = []
                        # print(result)
                    else:
                        if result != None:
                            cv2.putText(image, f"{result[0]} {result[1]}", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)
                            gesture = result[0]
                    
                    if gesture != prevgesture:
                        msg =bytes(gesture.lower(), 'utf-8')
                        conn.send(msg)
                        prevgesture = gesture
                        if gesture == "Zoom in" or gesture == "Zoom Out":
                            time.sleep(5)

                except:
                    pass

                cv2.imshow('aaa', image)
            else :
                cap.release()
                cv2.destroyAllWindows()
                cv2.waitKey(100)
                break

            if cv2.waitKey(10) & 0xFF == ord('q'):
                cap.release()
                cv2.destroyAllWindows()
                cv2.waitKey(100)
                break

    cap.release()
    cv2.destroyAllWindows()

threading.Thread(target=testpoints).start()
   
   
# import pickle
# import mediapipe as mp
# import cv2
# from dollarpy import Recognizer, Point
# import socket
# import threading

# # Load templates
# def load_templates(filename):
#     try:
#         with open(filename, 'rb') as f:
#             templates = pickle.load(f)
#         print(f"Templates loaded from {filename}")
#         return templates
#     except FileNotFoundError:
#         print(f"No templates found: {filename}")
#         return []

# # Socket setup
# class SocketHandler:
#     def __init__(self, host='127.0.0.1', port=3333):
#         self.server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#         self.server.bind((host, port))
#         self.server.listen(1)
#         print(f"Waiting for connection on {host}:{port}")
#         self.conn, self.addr = self.server.accept()
#         print(f"Device connected: {self.addr}")

#     def send_message(self, message):
#         try:
#             self.conn.sendall(message.encode('utf-8'))
#         except Exception as e:
#             print(f"Socket error: {e}")

# # Gesture recognition
# def recognize_gesture():
#     templates = load_templates("templates.pkl")
#     templates2 = load_templates("templates2.pkl")
#     recognizer = Recognizer(templates)
#     recognizer2 = Recognizer(templates2)

#     cap = cv2.VideoCapture(1)  # Use default camera
#     socket_handler = SocketHandler()

#     with mp.solutions.holistic.Holistic(
#         min_detection_confidence=0.5, min_tracking_confidence=0.5
#     ) as holistic:
#         points_buffer = []
#         while cap.isOpened():
#             ret, frame = cap.read()
#             if not ret:
#                 break

#             # Preprocess frame
#             image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
#             image.flags.writeable = False
#             results = holistic.process(image)
#             image.flags.writeable = True
#             image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

#             # Draw hand landmarks
#             mp.solutions.drawing_utils.draw_landmarks(
#                 image, results.right_hand_landmarks, mp.solutions.holistic.HAND_CONNECTIONS
#             )
#             mp.solutions.drawing_utils.draw_landmarks(
#                 image, results.left_hand_landmarks, mp.solutions.holistic.HAND_CONNECTIONS
#             )

#             try:
#                 # Collect points for recognition
#                 if results.right_hand_landmarks:
#                     points = [
#                         Point(lm.x, lm.y, 1) for lm in results.right_hand_landmarks.landmark
#                     ]
#                     points_buffer.append(points)

#                     # Recognize gesture
#                     if len(points_buffer) >= 15:  # Process every 15 frames
#                         flattened_points = [pt for frame in points_buffer for pt in frame]
#                         gesture1 = recognizer.recognize(flattened_points)
#                         gesture2 = recognizer2.recognize(flattened_points)

#                         if gesture1[0]:
#                             gesture = gesture1[0]
#                         elif gesture2[0]:
#                             gesture = gesture2[0]
#                         else:
#                             gesture = "Unknown"

#                         # Send gesture via socket
#                         socket_handler.send_message(gesture)
#                         cv2.putText(
#                             image, f"Gesture: {gesture}", (50, 50),
#                             cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA
#                         )

#                         points_buffer.clear()

#             except Exception as e:
#                 print(f"Error: {e}")

#             cv2.imshow('Gesture Recognition', image)
#             if cv2.waitKey(10) & 0xFF == ord('q'):
#                 break

#     cap.release()
#     cv2.destroyAllWindows()

# # Run the program
# if __name__ == "__main__":
#     threading.Thread(target=recognize_gesture).start()
