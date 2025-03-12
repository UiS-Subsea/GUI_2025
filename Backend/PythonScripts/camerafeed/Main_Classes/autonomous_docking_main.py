import cv2
import numpy as np
import time
import math




class AutonomousDocking:
    def __init__(self):
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        self.frame = None
        self.down_frame = None
        self.draw_grouts = False
        self.draw_grout_boxes = True
        self.angle_good = False
        
    #def run(self, front_frame, down_frame):
    def run(self, front_frame, down_frame):
        self.frame = front_frame
        self.down_frame = down_frame
        self.update()
        data = self.get_driving_data()
        return self.frame, self.down_frame, data
    

    def update(self):
        self.rotation_commands()

        if self.angle_good:
            frame_width = self.frame.shape[1]
            frame_height = self.frame.shape[0]
            
            frame_centerpoint = (frame_width // 2, frame_height // 2)
            red_centerpoint, red_radius = self.find_red()
            
            # center = (0, 0) and r = 0 are default values, meaning no red conture is found
            if red_centerpoint == (0, 0) and red_radius == 0: 
                # print("No docking station found!")
                return "No docking station found!"
            
            width_diff, height_diff = frame_centerpoint[0] - red_centerpoint[0], frame_centerpoint[1] - red_centerpoint[1] 
            percent_width_diff = (width_diff / frame_width) * 100
            percent_height_diff = (height_diff / frame_height) * 100
            
            red_to_frame_ratio = ((math.pi * red_radius ** 2) / (frame_width * frame_height)) * 100
            
            max_red_ratio = 30 # TODO change value, maybe remove entirely. if the red dot is more than 30% of the frame, stop
            if red_to_frame_ratio > max_red_ratio:
                # print("Stop! Docking station is close enough!")
                self.driving_data = [40, [0, 0, 0, 0, 0, 0, 0, 0]]
                raise SystemExit # stops ALL running code, since docking is done.
            else:
                self.driving_data = self.regulate_position(percent_width_diff, percent_height_diff)

    def regulate_position(self, displacement_y, displacement_z):
        drive_command = ""
        if displacement_y > 2:
            #drive_command = "GO LEFT"
            drive_command = [0, -displacement_y, 0, 0, 0, 0, 0, 0]
        
        elif displacement_y < -2:
            #drive_command = "GO RIGHT"
            drive_command = [0, displacement_y, 0, 0, 0, 0, 0, 0]

        elif displacement_z > 2:
            #drive_command = "GO DOWN"
            drive_command = [0, 0, -displacement_z, 0, 0, 0, 0, 0]

        elif displacement_z < -2:
            #drive_command = "GO UP"
            drive_command = [0, 0, displacement_y, 0, 0, 0, 0, 0]
        else:
            # drive_command = "GO FORWARD"
            drive_command = [10, 0, 0, 0, 0, 0, 0, 0]
            
        return drive_command

        
    def get_driving_data(self):
        data = self.driving_data.copy()
        self.driving_data = [40, [0, 0, 0, 0, 0, 0, 0, 0]]
        return data
    
    def find_red(self):
        # create a range for isolating only red
        lower_bound, upper_bound = (10, 10, 70), (70, 40, 255)
        #remove details from image
        blurred = cv2.GaussianBlur(self.frame, (11, 13), 0) 
        # creating a mask using the inRange() function and the low, high range, then dilating it
        mask1 = cv2.inRange(blurred, lower_bound, upper_bound)
        dilated = cv2.dilate(mask1, None, iterations=6)
        red_isolated = cv2.bitwise_and(self.frame, self.frame, mask=dilated)
        canny = cv2.Canny(red_isolated, 100, 200)
        # use findContours to get a list of all contoures
        contours, _ = cv2.findContours(canny, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
        # there may be a lot of noise in the image, to find the correct contoure, we loop through the list of contoures
        red_center = (0, 0), 0
        for c in contours:
            (x, y), radius = cv2.minEnclosingCircle(c)
            if radius > red_center[1]: # TODO may need to improve filtration of contures
                red_center = (x, y), radius
        # write center and radius as integers
        center_point = (int(red_center[0][0]), int(red_center[0][1]))
        radius = int(red_center[1])
        # draw a circle around the red dot
        cv2.circle(self.frame, center_point, radius, (0, 255, 0), 2)
        return center_point, radius
        
            
    def find_grouts(self):
        lower_bound, upper_bound = (0, 0, 0), (100, 100, 100)
        grouts = cv2.inRange(self.down_frame, lower_bound, upper_bound)
        grouts_dilated = cv2.dilate(grouts, None, iterations=10)
        canny = cv2.Canny(grouts_dilated, 100, 200)
        blurred = cv2.GaussianBlur(canny, (11, 13), 0)
        grout_contours, _ = cv2.findContours(blurred, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        if self.draw_grouts:
            cv2.drawContours(self.down_frame, grout_contours, -1, (0, 255, 0), 3)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                self.draw_grouts = False
        
        return grout_contours
    
    def find_relative_angle(self):
        grout_contours = self.find_grouts()
        
        # Used to find the average angle of the grouts
        angle_sum = 0
        angle_counter = 0
        
        for c in grout_contours:
            rect = cv2.minAreaRect(c)
            _, (width, height), angle = rect
            area = width * height
            
            # These max values depend on how far away the ROV is from the bottom
            #frame_height, frame_width = self.frame.shape    <----old that dose not work
            frame_height, frame_width = self.frame.shape[:2]

            
            MAX_AREA = (frame_height * frame_width) * 0.30
            MIN_AREA = (frame_height * frame_width) * 0.05
            if (area > MAX_AREA) or (area < MIN_AREA): 
               continue
            
            if width < height:
                angle = 90 - angle
            else:
                angle = -angle
                
            angle_sum += angle
            angle_counter += 1

            if self.draw_grout_boxes:
                box = cv2.boxPoints(rect)
                box = np.intp(box)
                cv2.drawContours(self.down_frame, [box], 0, (0, 0, 255), 2)

        avg_angle = angle_sum / angle_counter

        if angle_counter == 0:
            return "NO ANGLE"
        
        return avg_angle
            
    def rotation_commands(self):
        angle = self.find_relative_angle()
        if angle == None:
            return
            
        if angle == "NO ANGLE":
            return
            
        elif angle > 2:
            # Rotating Right
            self.driving_data = [0, 0, 0, 10, 0, 0, 0, 0]
            return
        
        elif angle < -2:
            # Rotating Left
            self.driving_data = [0, 0, 0, -10, 0, 0, 0, 0]
            return
        else:
            # Going Forward
            self.angle_good = True
            return
        
        
if __name__ == "__main__":
    start = time.perf_counter()
    a = AutonomousDocking()
    frame = cv2.imread("camerafeed/Other_Classes/images/transect1.png")
    a.run(frame, frame.copy())
    print(time.perf_counter() - start)
