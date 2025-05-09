import threading
import time
from Backend.PythonScripts.camerafeed.Main_Classes.autonomous_transect_main import AutonomousTransect
from camerafeed.Main_Classes.grass_monitor_main import SeagrassMonitor
from camerafeed.Main_Classes.autonomous_docking_main import AutonomousDocking
import cv2
import multiprocessing as mp
import datetime
import random


X_AXIS = 1
Y_AKSE = 0
Z_AKSE = 6
R_AKSE = 2

GST_FEED_STEREO_L = "-v udpsrc multicast-group=224.1.1.1 auto-multicast=true port=5000 ! application/x-rtp, media=video, clock-rate=90000, encoding-name=H264, payload=96 ! rtph264depay ! h264parse ! decodebin ! videoconvert ! appsink sync=false"
GST_FEED_STEREO_R = "-v udpsrc multicast-group=224.1.1.1 auto-multicast=true port=5001 ! application/x-rtp, media=video, clock-rate=90000, encoding-name=H264, payload=96 ! rtph264depay ! h264parse ! decodebin ! videoconvert ! appsink sync=false"
GST_FEED_DOWN = "-v udpsrc multicast-group=224.1.1.1 auto-multicast=true port=5002 ! application/x-rtp, media=video, clock-rate=90000, encoding-name=H264, payload=96 ! rtph264depay ! h264parse ! decodebin ! videoconvert ! appsink sync=false"
GST_FEED_MANIPULATOR = "-v udpsrc multicast-group=224.1.1.1 auto-multicast=true port=5003 ! application/x-rtp, media=video, clock-rate=90000, encoding-name=H264, payload=96 ! rtph264depay ! h264parse ! decodebin ! videoconvert ! appsink sync=false"

class Camera:
    def __init__(self, name, gst_feed = None):
        self.name = name # Name of camera
        self.gst = gst_feed # Gstreamer feed

    def get_frame(self):
        ret, frame = self.cam.read()
        if not ret:
            print("Error reading frame")
            return
        return frame
    
    def open_cam(self):
        if self.gst is None:
            self.cam = cv2.VideoCapture(0)
            if not self.isOpened:
                print(f"Error opening camera {self.name}")
                return False
            print(f"{self.name} Camera opened")
            return True
        else:
            self.cam = cv2.VideoCapture(self.gst, cv2.CAP_GSTREAMER)
            if not self.isOpened:
                print("Error opening camera")
                return False
            print(f"{self.name} Camera opened")
            return True
            
    def release_cam(self):
        self.cam.release()
        
    @property
    def isOpened(self):
        return self.cam.isOpened()
            
class CameraManager:
    def __init__(self) -> None:
        self.recording = False
        self.frames = {"StereoL": None, "StereoR": None, "Down": None, "Manipulator": None, "Manual": None, "Test": None}
        self.active_cameras = []
        
    def add_cameras(self, *camera_names):
        for name in camera_names:
            if name == "StereoL":
                cam = Camera("StereoL", GST_FEED_STEREO_L)
                self.active_cameras.append(cam)
            elif name == "StereoR":
                cam = Camera("StereoR", GST_FEED_STEREO_R)
                self.active_cameras.append(cam)
            elif name == "Down":
                cam = Camera("Down", GST_FEED_DOWN)
                self.active_cameras.append(cam)
            elif name == "Manip":
                cam = Camera("Manip", GST_FEED_MANIPULATOR)
                self.active_cameras.append(cam)
            elif name == "Manual":
                cam = Camera("Manual")
                self.active_cameras.append(cam)

    def open_cameras(self):
        for cam in self.active_cameras:
            cam.open_cam()
            
    def get_frames(self):
        for cam in self.active_cameras:
            self.frames[cam.name] = cam.get_frame()
    


    def close_all(self):
        for cam in self.active_cameras:
            try:
                cam.release_cam()
            except:
                pass
        
    def setup_video(self, name):
        self.videoresult = cv2.VideoWriter(f'camerafeed/output/{name}.avi', cv2.VideoWriter_fourcc(
            *'MJPG'), 10, (int(self.active_cameras[0].cam.get(3)), int(self.active_cameras[0].cam.get(4))))

    # Run this to start recording, and do a keyboard interrupt (ctrl + c) to stop recording
    
    
    #TODO make it so that it can record from multiple cameras at the same time
    #TODO MAKE IT WORK (Only reord if there is a camera selected) <<<<<<<<<<
    def record_video(self):
        # if self.recording == False:
        #     if len(self.active_cameras) == 0:
        #         print("No camera selected")
    
        # else:
        #     # Makes a new videofile if it is not already recording (Default)
        if self.recording == False:
            self.setup_video(f"Video{self.active_cameras[0].name}{datetime.datetime.now()}")
            self.recording = True
            
        # While recording, it will write the frames to the videofile
        if self.recording == True:
            cur_cam = self.active_cameras[0]

            frame = self.get_frame_from_cam(cur_cam)
            self.videoresult.write(frame)
            return frame
        else:
            self.videoresult.release()
            
            
    def save_image(self):
        if len(self.active_cameras) == 0:
            print("No camera selected")
        else:
            print("Cameras detected, amount: ", len(self.active_cameras))
            for i in range(len(self.active_cameras)):
                cur_cam = self.active_cameras[i]
                frame_to_save = self.get_frame_from_cam(cur_cam)
                cv2.imwrite(f"camerafeed/output/Img_Cam_{cur_cam.name}{datetime.datetime.now()}.jpg", frame_to_save)
            print("Image(s) saved")
            
class ExecutionClass:
    def __init__(self, driving_queue, manual_flag):
        self.AutonomousTransect = AutonomousTransect()
        self.Docking = AutonomousDocking()
        self.Seagrass = SeagrassMonitor()
        self.Camera = CameraManager()
        self.counter = 0
        self.done = False
        self.manual_flag = manual_flag
        self.driving_queue = driving_queue

    def update_down(self):
        self.frame_down = self.Camera.get_frame_down()
        
    def update_stereo_L(self):
        self.frame_stereoL = self.Camera.get_frame_stereo_L()
        
    def update_stereo_R(self):
        self.frame_stereoR = self.Camera.get_frame_stereo_R()
        
    def update_manipulator(self):
        self.frame_manipulator = self.Camera.get_frame_manipulator()
        
    def update_manual(self):
        self.frame_manual = self.Camera.get_frame_manual()

    def update_test_cam(self):
        self.frame_test = self.Camera.get_frame_test()
        
    def show(self, frame, name="frame"):
        cv2.imshow(name, frame)
        if cv2.waitKey(1) == ord("q"):
            self.stop_everything()
            
    def testing_for_torr(self):
        self.done = False
        while not self.done:
            self.update_stereo_L()
            self.update_stereo_R()
            self.show(self.frame_stereoL, "StereoL")
            self.show(self.frame_stereoR, "StereoR")
            
    def camera_test(self):
        while self.done:
            # self.update_manual()
            self.update_down()
            self.update_stereo_L()
            # self.update_stereo_R()
            # self.update_manip()
            
            self.show(self.frame_down, "Down")
            # self.show(self.frame_manual, "Manual")
            self.show(self.frame_stereoL, "StereoL")
            # self.show(self.frame_stereoR, "StereoR")
            # self.show(self.frame_manipulator, "Manip")
        
    def send_data_test(self):
        self.done = False
        start = 0
        while not self.done:
            cur_time = time.time()
            if (cur_time - start) > 0.02:
                random_data = [random.randint(0,10) for _ in range(8)]
                self.send_data_to_rov(random_data)
                
                start = time.time()
        
    def sleep_func(self):
        threading.Timer(1000, self.sleep_func).start()

    def transect(self):
        self.done = False
        self.Camera.start_down_cam() # TODO should be down frame
        while not self.done and self.manual_flag.value == 0:
            self.update_down() # TODO Should be down frame
            transect_frame, driving_data_packet = self.AutonomousTransect.run(self.frame_down)
            self.show(transect_frame, "Transect")
            self.send_data_to_rov(driving_data_packet)
        else:
            self.stop_everything()

    def seagrass(self):
        growth = self.Seagrass.run(self.frame.copy())
        return growth

    def docking(self):
        self.done = False
        self.Camera.start_stereo_cam_L()
        self.Camera.start_stereo_cam_R() # TODO shoould be down camera
        while not self.done and self.manual_flag.value == 0:
            # Needs stereo L, and Down Cameras
            self.update_stereo_R()
            self.update_stereo_L() # TODO should be down camera
            docking_frame, frame_under, driving_data_packet = self.Docking.run(self.frame_stereoL, self.frame_stereoR) # TODO should be down camera
            self.show(docking_frame, "Docking")
            self.show(frame_under, "Frame Under")
            self.send_data_to_rov(driving_data_packet)
            # self.show(frame_under, "Frame Under")
        else:
            self.stop_everything()
            
    def send_data_to_rov(self, datapacket):
        data_to_send = {"autonomdata": datapacket}
        self.driving_queue.put((2, data_to_send))

    def normal_camera(self):
        self.done = False
        self.Camera.start_manual_cam()
        while not self.done:
            self.update_manual()
            self.show(self.frame_manual, "Manual")
            
    def show_all_cameras(self):
        self.done = False
        self.Camera.start_stereo_cam_L()
        self.Camera.start_stereo_cam_R()
        self.Camera.start_down_cam()
        # self.Camera.start_manipulator_cam()
        while not self.done:
            self.update_stereo_L()
            self.update_stereo_R()
            self.update_down()
            self.show(self.frame_stereoL, "StereoL")
            self.show(self.frame_stereoR, "StereoR")
            self.show(self.frame_down, "Down")
            # self.show(self.frame_manipulator, "Manip")
            
    def stop_everything(self):
        print("Stopping other processes, returning to manual control")
        try:
            self.done = True
            cv2.destroyAllWindows()
            self.Camera.close_all()
            self.Camera.active_cameras = []
        except:
            pass

    def transect_test(self):
        print("Running Transect!")
        
    def record(self):
        self.done = False
        
        # If the amera is already recording, then this stops it
        if self.Camera.recording:
            self.Camera.recording = False
            print("Recording stopped")
            self.done = True

        
        active_cams = self.Camera.active_cameras
        if len(active_cams) >= 1:
            print("Cameras detected, amount: ", len(active_cams))
        else:
            print("No active cameras")
            self.done = True
            
        while not self.done and len(self.Camera.active_cameras) >= 1:
            frame = self.Camera.record_video()
            self.show(frame, "Recording")
        else:
            self.Camera.recording = False
            cv2.destroyWindow("Recording")
            print("Recording stopped or no active cameras")
        
    def save_image(self):
        self.Camera.save_image()
        

if __name__ == "__main__":
    cam = CameraManager()
    execution = ExecutionClass()
    while True:
        execution.update_stereo()
        # execution.show(execution.frame_down, "Down")
        execution.show(execution.frame_stereoL, "StereoL")
        execution.show(execution.frame_stereoR, "StereoR")
        # execution.show(execution.frame_manipulator, "Manip")
