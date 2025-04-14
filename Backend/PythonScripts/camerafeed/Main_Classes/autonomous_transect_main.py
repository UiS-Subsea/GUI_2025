import cv2 as cv
import numpy as np
import time
import math

class AutonomousTransect:
    def __init__(self):
        self.frame = None
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        self.cantabilize = False
        
        # Navigation
        self.navigation_angle = None
        self.navigation_vector = None
        
        # Pipeline tracking
        self.pipeline_detected = False  #flag
        self.pipeline_contour = None
        self.pipeline_center = None
        self.pipeline_direction = None
        self.yellow_lower = np.array([20, 100, 100])  # HSV values for yellow
        self.yellow_upper = np.array([40, 255, 255])
        
        # ArUco marker detection
        self.aruco_dict = cv.aruco.getPredefinedDictionary(cv.aruco.DICT_ARUCO_ORIGINAL)
        self.aruco_params = cv.aruco.DetectorParameters()
        self.detected_markers_ids = []
        self.marker_positions = {} 
        self.last_marker_time = time.time()
        self.no_marker_timeout = 30
        
        # State management
        self.tracking_state = "FIND_PIPELINE"  # States: FIND_PIPELINE, TRACK_PIPELINE, COMPLETE
        self.autonomous_mode = False
        self.return_home = False
        
        # TODO: Either implement or remove these
        self.pinger_detected = False 
        self.pinger_location = None 
        self.launch_coordinates = None

    def detect_markers(self):
        """Detect ArUco markers in the current frame and update the markers list"""
        gray = cv.cvtColor(self.frame, cv.COLOR_BGR2GRAY)
        corners, ids, _ = cv.aruco.detectMarkers(
            gray,
            self.aruco_dict,
            parameters=self.aruco_params
        )
        
        if ids is not None and len(ids) > 0:
            cv.aruco.drawDetectedMarkers(self.frame, corners, ids)
            for i, marker_id in enumerate(ids):
                id_int = int(marker_id[0])
                # Calculate marker center
                center = np.mean(corners[i][0], axis=0).astype(int)
                
                # Add to list if not already present
                if id_int not in self.detected_markers_ids:
                    self.detected_markers_ids.append(id_int)
                    self.marker_positions[id_int] = center
                    self.last_marker_time = time.time()
                    print(f"New marker detected! ID: {id_int}")
                
                # Display ID and center on frame
                cv.putText(self.frame, f"ID: {id_int}", 
                          (center[0] + 10, center[1]), 
                          cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)
                cv.circle(self.frame, tuple(center), 5, (0, 255, 0), -1)

    def detect_pinger(self, acoustic_data=None):
        # TODO: Implement actual acoustic signal processing
       pass 

   
    
    def run(self, frame):
        self.frame = frame
        self.update()
        data = self.get_driving_data()
        return self.frame, data, self.detected_markers_ids, self.navigation_angle, self.navigation_vector
    
    def update(self):
        """Update method to manage pipeline tracking states"""
        if self.tracking_state == "FIND_PIPELINE":
            self.find_and_approach_pipeline()
        elif self.tracking_state == "TRACK_PIPELINE":
            self.track_pipeline_and_markers()
        elif self.tracking_state == "COMPLETE":

            if self.return_home:
                self.autonomous_return()
            else:
                self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]  # Stop movement

    def find_and_approach_pipeline(self):
        """Find and approach the pipeline"""
        self.detect_pipeline()
        
        if self.pipeline_detected:  # Pipeline found, transition to tracking state
            self.tracking_state = "TRACK_PIPELINE"
            print("Pipeline detected! Transitioning to tracking.")
        else:
            self.driving_data = [5, 0, 0, 2, 0, 0, 0, 0]  # Move forward slowly while rotating

    def track_pipeline_and_markers(self):
        """Track pipeline and detect markers"""
        # Detect markers
        self.detect_markers()
        
        # Detect and follow pipeline
        self.detect_pipeline()
        
        if self.pipeline_detected:
            # Calculate driving commands to follow pipeline
           vector, angle = self.regulate_position()
           self.navigation_vector = vector
           self.navigation_angle = angle
        else:  # Lost pipeline, search locally
            self.driving_data = [0, 0, 0, 5, 0, 0, 0, 0]  # Rotate in place
            self.navigation_vector = None
            self.navigation_angle = None
        if time.time() - self.last_marker_time > self.no_marker_timeout:
            if len(self.detected_markers_ids) > 0:
                print("Pipeline inspection complete!")
                self.tracking_state = "COMPLETE"

    def get_driving_data(self):
        """Return the current driving data"""
        data = self.driving_data.copy()
        if self.frame is not None: # driving commands on frame
            cmd_text = f"Drive: [{data[0]:.1f}, {data[1]:.1f}, {data[2]:.1f}, {data[3]:.1f}]"
            cv.putText(self.frame, cmd_text, (10, self.frame.shape[0] - 10), 
                      cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)
        
        return data

    def detect_pipeline(self):
        """Detect yellow pipeline with special handling for elbows and bends"""
        hsv = cv.cvtColor(self.frame, cv.COLOR_BGR2HSV)  # Convert to HSV color space
        pipeline_mask = cv.inRange(hsv, self.yellow_lower, self.yellow_upper)  # Threshold for yellow color
        kernel = np.ones((7, 7), np.uint8) #opening operation to clean up the mask
        pipeline_mask = cv.morphologyEx(pipeline_mask, cv.MORPH_OPEN, kernel) #closing operation 
        pipe_contours, _ = cv.findContours(pipeline_mask, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE)  # extract contours from the binary mask 
        valid_contours = [cnt for cnt in pipe_contours if cv.contourArea(cnt) > 100]    # filters out the npises and merge contours
        
        if valid_contours:
            full_pipeline_contour = np.vstack(valid_contours)
            cv.drawContours(self.frame, valid_contours, -1, (0, 255, 0), 2)
            
            # Check if contour is large enough to be a pipeline
            if cv.contourArea(full_pipeline_contour) > 500:  
                self.pipeline_detected = True
                self.pipeline_contour = full_pipeline_contour
                # Calculate center of pipeline (overall)
                M = cv.moments(full_pipeline_contour)
                if M["m00"] != 0:
                    cx = int(M["m10"] / M["m00"])
                    cy = int(M["m01"] / M["m00"])
                    self.pipeline_center = (cx, cy)
                    # Draw center of pipeline
                    cv.circle(self.frame, self.pipeline_center, 7, (255, 0, 0), -1)
                    
                    # ------ BEND/ELBOW DETECTION-----
                    epsilon = 0.02 * cv.arcLength(full_pipeline_contour, True)
                    approx = cv.approxPolyDP(full_pipeline_contour, epsilon, True)
                    cv.drawContours(self.frame, [approx], -1, (255, 0, 255), 2) #the approximated shape
                    rect = cv.minAreaRect(full_pipeline_contour)
                    box = cv.boxPoints(rect)
                    box = np.int0(box)
                    cv.drawContours(self.frame, [box], 0, (0, 0, 255), 2) # the bounding rectangle
                    is_bent = False #  Analyze the contour shape to detect bends
                    is_elbow = False
                    bend_direction = None

                    if 5 <= len(approx) <= 8: #check for elbow 
                        is_bent = True
                        width, height = rect[1]
                        if 0.7 < width/height < 1.3:  # Close to square aspect ratio often indicates an elbow
                            is_elbow = True
                    
                    angle = None
                    prev_direction = self.pipeline_direction
    
                    if is_elbow:  
                        leftmost = tuple(full_pipeline_contour[full_pipeline_contour[:, :, 0].argmin()][0])
                        rightmost = tuple(full_pipeline_contour[full_pipeline_contour[:, :, 0].argmax()][0])
                        topmost = tuple(full_pipeline_contour[full_pipeline_contour[:, :, 1].argmin()][0])
                        bottommost = tuple(full_pipeline_contour[full_pipeline_contour[:, :, 1].argmax()][0])
                        
                        # visualise extreme points
                        cv.circle(self.frame, leftmost, 5, (255, 0, 0), -1)
                        cv.circle(self.frame, rightmost, 5, (0, 255, 0), -1)
                        cv.circle(self.frame, topmost, 5, (0, 0, 255), -1)
                        cv.circle(self.frame, bottommost, 5, (255, 255, 0), -1)
                        
                    
                        #distances to extremes from center
                        dist_left = np.sqrt((cx - leftmost[0])**2 + (cy - leftmost[1])**2)
                        dist_right = np.sqrt((cx - rightmost[0])**2 + (cy - rightmost[1])**2)
                        dist_top = np.sqrt((cx - topmost[0])**2 + (cy - topmost[1])**2)
                        dist_bottom = np.sqrt((cx - bottommost[0])**2 + (cy - bottommost[1])**2)
                        
                        #the two closest extremes - these will be near the bend point
                        distances = {
                            "left": dist_left,
                            "right": dist_right,
                            "top": dist_top,
                            "bottom": dist_bottom
                        }
                        
                        sorted_extremes = sorted(distances.items(), key=lambda x: x[1])         
                        farthest_extremes = [sorted_extremes[2][0], sorted_extremes[3][0]]

                        # This is a first approximation
                        if "right" in farthest_extremes and "bottom" in farthest_extremes:
                            bend_direction = "bottom-right"
                            angle = 45  # Point toward bottom-right
                        elif "right" in farthest_extremes and "top" in farthest_extremes:
                            bend_direction = "top-right"
                            angle = -45  # Point toward top-right
                        elif "left" in farthest_extremes and "bottom" in farthest_extremes:
                            bend_direction = "bottom-left"
                            angle = 135  # Point toward bottom-left
                        elif "left" in farthest_extremes and "top" in farthest_extremes:
                            bend_direction = "top-left"
                            angle = -135  # Point toward top-left
                        
    
                        if prev_direction is not None:
                            # Convert previous angle to unit vector
                            prev_vec_x = np.cos(np.radians(prev_direction))
                            prev_vec_y = np.sin(np.radians(prev_direction))
                            candidates = []
                            
                            if "right" in farthest_extremes:
                                candidates.append(0)  # Right
                            if "bottom" in farthest_extremes:
                                candidates.append(90)  # Down
                            if "left" in farthest_extremes:
                                candidates.append(180)  # Left
                            if "top" in farthest_extremes:
                                candidates.append(-90)  # Up
                    
                            best_angle = None
                            best_dot = -2  # Initialize with impossible dot product #TODO: check compare
                            
                            for candidate in candidates:
                                cand_vec_x = np.cos(np.radians(candidate))
                                cand_vec_y = np.sin(np.radians(candidate))
                                
                                #  dot product
                                dot_product = prev_vec_x * cand_vec_x + prev_vec_y * cand_vec_y
                                if dot_product > best_dot:
                                    best_dot = dot_product
                                    best_angle = candidate
                            if best_angle is not None:
                                angle = best_angle
                            
                    else:  # For straight section
                        angle = rect[2]
                        width, height = rect[1]
                        if width < height:
                            angle = angle - 90 if angle > 0 else angle + 90  # Vertical rectangle
                        # Ensure consistent directionality
                        if prev_direction is not None:
                            vec_x = np.cos(np.radians(angle))  # Convert angle to vector 
                            is_vertical = height > width
                            if is_vertical:
                                preferred_angle = 90 
                            else:
                                preferred_angle = 0 if vec_x >= 0 else 180
                            angle_diff = ((preferred_angle - angle + 180) % 360) - 180
            
                            if abs(angle_diff) > 90:
                                angle = (angle + 180) % 360
                                if angle > 180:# Normalize to -180 to 180 range
                                    angle -= 360
                    
                  
                    if prev_direction is not None:
                        angle_diff = ((angle - prev_direction + 180) % 360) - 180
                        smooth_factor = 0.9 if is_elbow else 0.7
                        angle = prev_direction + angle_diff * (1 - smooth_factor)
            
                    self.pipeline_direction = angle
                    length = 50
                    angle_rad = math.radians(angle+180)
                    end_x = int(cx + length * math.cos(angle_rad))
                    end_y = int(cy + length * math.sin(angle_rad))
                    cv.arrowedLine(self.frame, self.pipeline_center, (end_x, end_y), (255, 0, 255), 2)
                    
                    # debug visuali
                    elbow_text = "ELBOW" if is_elbow else ("BEND" if is_bent else "STRAIGHT")
                    cv.putText(self.frame, f"Dir: {angle:.1f} ({elbow_text})", 
                      (cx + 15, cy - 15), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 0, 255), 2)
                    
                    if bend_direction:
                        cv.putText(self.frame, f"Bend: {bend_direction}", 
                            (cx + 15, cy + 15), cv.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 255), 2)
                    cv.putText(self.frame, f"Contours: {len(valid_contours)} Points: {len(approx)}", (10, 60), 
                        cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)
                return
        #No Pipeline
        self.pipeline_detected = False
        self.pipeline_contour = None
        self.pipeline_center = None
       
    def regulate_position(self):
        """Control ROV position relative to pipeline and return navigation vector"""
        if not self.pipeline_detected or self.pipeline_center is None:
            return [0, 0], 0  #no pipeline is detected
            
        frame_height, frame_width = self.frame.shape[:2]
        frame_center_x = frame_width // 2
        frame_center_y = frame_height // 2
        # Calculate displacement from center 
        dx = self.pipeline_center[0] - frame_center_x # Normalize displacement as percentage  frm dimenions
        dy = self.pipeline_center[1] - frame_center_y 
        dx_percent = (dx / frame_width) * 100
        dy_percent = (dy / frame_height) * 100
        threshold = 5 #hreshold for movement (dead zone)
        scale = 0.2  # frm percentage to motor power
        forward_speed = 10  #moving forward along pipeline
        # horizontal adjustment
        if abs(dx_percent) > threshold:
            y_power = -dx_percent * scale
        else:
            y_power = 0    
        #vertical adjustment
        if abs(dy_percent) > threshold:
            z_power = -dy_percent * scale #vertical position
        else:
            z_power = 0
        angle = self.pipeline_direction if self.pipeline_direction is not None else 0
        
        # angular adjustment to align with pipeline direction
        if self.pipeline_direction is not None:
            heading_error = self.pipeline_direction
            rot_power = heading_error * 0.1  # Proportional control
        else:
            rot_power = 0
        self.driving_data = [forward_speed, y_power, z_power, rot_power, 0, 0, 0, 0]   # combinign  movements

        cv.putText(self.frame, f"Drive: Fwd={forward_speed:.1f}, Y={y_power:.1f}, Z={z_power:.1f}, Rot={rot_power:.1f}",
                (10, 30), cv.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 2)     
        self.canstabilize = True
        return [dx, dy], angle #for regulation 
    
        
    def autonomous_return(self):
        """Return autonomously to launch site"""
        # TODO: Implementing navigation back to the pinger location autonomously
        pass 


if __name__ == "__main__":
    transect = AutonomousTransect()
    cap = cv.VideoCapture("../camerafeed/videos/video3.mp4")
    while True:
        ret, frame = cap.read()
        if not ret:
            print("End of video or failed to read frame")
            break
        processed_frame, data, markers, nav_vector, nav_angle = transect.run(frame)
        cv.imshow("Pipeline Inspection", processed_frame)
        if cv.waitKey(25) & 0xFF == ord('q'):
            break
    cap.release()
    cv.destroyAllWindows()