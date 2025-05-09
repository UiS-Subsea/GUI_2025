import numpy as np
import cv2 as cv
import sys
import time 


class AutonomousDocking:
    """Autonomous docking system using computer vision and ArUco markers."""
    
    def __init__(self, camera_matrix=None, dist_coeffs=None):
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        self.frame = None
        self.down_frame = None
        self.angle_good = True
        
        self.aruco_dict = cv.aruco.getPredefinedDictionary(cv.aruco.DICT_ARUCO_ORIGINAL)
        self.aruco_params = cv.aruco.DetectorParameters()
        
        self.camera_matrix = camera_matrix if camera_matrix is not None else np.array([[1000, 0, 320], [0, 1000, 240], [0, 0, 1]])
        self.dist_coeffs = dist_coeffs if dist_coeffs is not None else np.zeros((5, 1))
        self.camera_offset_y = 50
        
        self.last_position = None
        self.stable_counter = 0
        
        self.pallet_found = False
        self.debug = True

    def undistort_image(self, image):
        """Removes distortion from the image."""
        if self.camera_matrix is None or self.dist_coeffs is None:
            return image
        h, w = image.shape[:2]
        newcameramtx, roi = cv.getOptimalNewCameraMatrix(self.camera_matrix, self.dist_coeffs, (w, h), 1, (w, h))
        mapx, mapy = cv.initUndistortRectifyMap(self.camera_matrix, self.dist_coeffs, None, newcameramtx, (w, h), 5)
        dst = cv.remap(image, mapx, mapy, cv.INTER_LINEAR)
        x, y, w, h = roi
        dst = dst[y:y + h, x:x + w]
        return dst
        
    def enhance_image(self, image):
        """Enhances image for marker detection."""
        if len(image.shape) == 3:
            image = cv.cvtColor(image, cv.COLOR_BGR2GRAY)
        image = cv.equalizeHist(image)
        gaussian_blur = cv.GaussianBlur(image, (9, 9), 2.0)
        new_image = cv.addWeighted(image, 1.5, gaussian_blur, -0.5, 0)
        return new_image
    
    def detect_markers(self, image):
        """Detects ArUco markers in the image."""
        corners, ids, rejected = cv.aruco.detectMarkers(image, self.aruco_dict, parameters=self.aruco_params)
        if self.debug:
            print(f"Detected markers: {len(corners) if corners else 0}, IDs: {ids}")
            
        if ids is not None:
            wanted_ids = np.array([28, 7, 19, 96])
            indexes = np.isin(ids.flatten(), wanted_ids)
            filtered_corners = [corners[i] for i in range(len(corners)) if indexes[i]]
            filtered_ids = ids[indexes]
            
            cv.aruco.drawDetectedMarkers(self.frame, corners, ids)
            
            return filtered_corners, np.array(filtered_ids), rejected
        return [], np.array([]), rejected
    
    def display_markers(self, corners, ids, image, pallet_center=None):
        """Displays detected markers on image."""
        if corners and len(corners) > 0:
            cv.aruco.drawDetectedMarkers(image, corners, ids)
            
        if pallet_center:
            cv.circle(image, tuple(pallet_center), 15, (0, 255, 0), -1)
            cv.putText(image, f"Pallet Center: {pallet_center}", (30, 60), 
                      cv.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
                      
        return image
    def calculate_pallet_center(self, corners, ids):
        """Calculates pallet center position from markers."""
        if ids is not None and len(ids) > 0:
            center_points = [np.mean(corner[0], axis=0) for corner in corners]
            if center_points:
                pallet_center = np.mean(center_points, axis=0).astype(int).tolist()
                
                
                pallet_center[1] += self.camera_offset_y # camera offset
                pallet_center[0] -= 10  # adjsut x 
                pallet_center[1] -= 25  # adjust y
                
                if self.debug:
                    print(f"Pallet center calculated at: {pallet_center}")
                self.pallet_found = True
                return pallet_center
        if self.debug:
            print("No pallet center could be calculated")
        return None
    def calculate_displacement(self, pallet_center, image_center):
        """Calculates displacement between pallet and image center."""
        dx = pallet_center[0] - image_center[0]
        dy = pallet_center[1] - image_center[1]
        return dx, dy
    
    def determine_commands(self, dx, dy, threshold):
        """Determines navigation commands based on displacement."""
        commands = []
        if abs(dx) > threshold:
            commands.append("RIGHT" if dx > 0 else "LEFT")
        if abs(dy) > threshold:
            commands.append("DOWN" if dy > 0 else "UP")
        return commands
    
    def get_navigation_command(self, pallet_center, image_center, threshold=10):
        """Gets navigation commands based on pallet position."""
        if not pallet_center:
            return ["SEARCH"]
        dx, dy = self.calculate_displacement(pallet_center, image_center)
        commands = self.determine_commands(dx, dy, threshold)
        if not commands:
            commands.append("STOP")
        return commands
    
    def is_centered(self, pallet_center, image):
        """Checks if pallet is centered in image."""
        centered = False
        if pallet_center:
            corrected_center = pallet_center.copy()
            corrected_center[0] += 0 
            corrected_center[1] -= 30 

            image_center = (image.shape[1] // 2, image.shape[0] // 2)
            dx, dy = self.calculate_displacement(corrected_center, image_center)
            distance = np.hypot(dx, dy)
            move_threshold = 10
            directions = []
            #cneter is 400, 600
            if abs(dx) > move_threshold or abs(dy) > move_threshold:
                if abs(dx) > move_threshold:
                    directions.append("RIGHT" if dx > 0 else "LEFT")
                if abs(dy) > move_threshold:
                    directions.append("BACK" if dy > 0 else "FORWARD")
                cv.putText(image, f"Distance: {distance:.2f}", (30, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 8)
                cv.putText(image, f"Distance: {distance:.2f}", (30, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)
                for i, direction in enumerate(directions, start=1):
                    cv.putText(image, direction, (30, 30 + i * 20), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 8)
                    cv.putText(image, direction, (30, 30 + i * 20), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 0), 2)
            else:
                centered = True
                
            cv.circle(image, tuple(corrected_center), 50, (0, 255, 0) if distance < 10 else (0, 0, 255), 3)
            
            if self.debug:
                print(f"Distance to center: {distance:.2f} pixels")
                
        return centered
    
    def search_strategy(self):
        """Implements search pattern when docking station not visible."""
        if self.debug:
            print("Executing search strategy")
        self.driving_data = [0, 0, 0, 5, 0, 0, 0, 0]
    
    def navigate(self, commands):
        """Controls navigation based on commands."""
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        for command in commands:
            if command == "FORWARD" or command == "UP":
                self.driving_data[0] = 5
                print("Moving forward/up")
            elif command == "BACK" or command == "DOWN":
                self.driving_data[0] = -5
                print("Moving backward/down")
            elif command == "LEFT":
                self.driving_data[2] = -5
                print("Moving left")
            elif command == "RIGHT":
                self.driving_data[2] = 5
                print("Moving right")
            elif command == "SEARCH":
                self.search_strategy()
            elif command == "STOP":
                print("Stopping movement")
        
        return True
    
                
    def get_driving_data(self):
        """Returns and resets driving data."""
        data = self.driving_data.copy()
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        return data
        
    def run(self, front_frame, down_frame=None):
        """Runs one cycle of the docking system."""
        self.frame = front_frame
        self.down_frame = down_frame if down_frame is not None else np.zeros_like(front_frame)
        image_center = (self.frame.shape[1] // 2, self.frame.shape[0] // 2)
        enhanced_frame = self.enhance_image(self.frame.copy())
        corners, ids, rejected = self.detect_markers(enhanced_frame)
        pallet_center = self.calculate_pallet_center(corners, ids)
        commands = self.get_navigation_command(pallet_center, image_center)
        self.navigate(commands)
        processed_frame = self.frame.copy()
        processed_frame = self.display_markers(corners, ids, processed_frame, pallet_center)
        data = self.get_driving_data()
        return processed_frame, self.down_frame, data

''' testing with webcam and iphone 
def init_camera():
    """Initializes and configures camera."""
    cap = cv.VideoCapture(0)
    if not cap.isOpened():
        print("Error: Could not open camera.")
        return None
    cap.set(cv.CAP_PROP_FRAME_WIDTH, 1920)
    cap.set(cv.CAP_PROP_FRAME_HEIGHT, 1080)
    return cap

def resize_frame(frame, target_height):
    """Resizes frame to target height maintaining aspect ratio."""
    ratio = target_height / frame.shape[0]
    new_width = int(frame.shape[1] * ratio)
    resized_frame = cv.resize(frame, (new_width, target_height), interpolation=cv.INTER_AREA)
    return resized_frame

def main():
    """Main function to run the autonomous docking system."""
    cap = init_camera()
    if not cap:
        sys.exit("Error: Unable to initialize camera.")  
    camera_matrix = np.array([[942.6, 0, 998.5], [0, 940.4, 483.6], [0, 0, 1]])
    dist_coeffs = np.array([[-0.400, 0.210, 7.31e-3, -6.25e-3, -7.03e-2]]).reshape(-1, 1)
    docking_system = AutonomousDocking(camera_matrix=camera_matrix, dist_coeffs=dist_coeffs)
    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            print("Error: Can't read frame.")
            break    
        frame = resize_frame(frame, 1000)
        processed_frame, down_frame, driving_data = docking_system.run(frame)
        image_center = (frame.shape[1] // 2, frame.shape[0] // 2)
        enhanced_frame = docking_system.enhance_image(frame)
        corners, ids, reject = docking_system.detect_markers(enhanced_frame)
        pallet_center = docking_system.calculate_pallet_center(corners, ids)

        commands = docking_system.get_navigation_command(pallet_center, image_center)
        docking_system.navigate(commands)
        for command in commands:
            print(f"Command: {command}")

        frame = docking_system.display_markers(corners, ids, frame, pallet_center)
        undistorted_frame = docking_system.undistort_image(frame)
        cv.imshow("ArUco: ", undistorted_frame)
        if docking_system.debug:
            print(f"Driving data: {driving_data}")   
        cv.imshow("ArUco Detection", processed_frame)
        if cv.waitKey(1) & 0xFF == ord('q'):
            break
    cap.release()
    cv.destroyAllWindows() '''


if __name__ == "__main__":
    start = time.perf_counter()
    a = AutonomousDocking()
    frame = cv.imread("../camerafeed/images/dockingPallet.png")
    a.run(frame, frame.copy())
    print(time.perf_counter() - start)

   