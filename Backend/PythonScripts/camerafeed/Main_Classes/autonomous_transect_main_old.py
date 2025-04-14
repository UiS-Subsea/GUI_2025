import cv2
import numpy as np
import time

# What to tweak for water test:
# 1. cv2.inRange() lower and upper range
# 2. Range of acceptable angles
# 3. Range of acceptable ratios of displacement from center

class AutonomousTransect:
    def __init__(self):
        self.canStabilize = False
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        self.frame = None

    #takes in frame, finds all the contours of objects with dark blue color
    #returns angle between             
    def run(self, frame):
        if frame is None:
            print("Error: Input frame is None!")
            return None, None
        
        self.frame = frame
        self.update()
        data = self.get_driving_data()
        return self.frame, data
        
    def update(self):
        self.stabilize_angle()
        self.stabilize_alignment()
        
    def get_driving_data(self):
        data = self.driving_data.copy()
        self.driving_data = [0, 0, 0, 0, 0, 0, 0, 0]
        return data

    def get_angle_between_pipes(self, pipe1, pipe2):
        angle1 = pipe1[2]
        angle2 = pipe2[2]
        if angle1 > 45:
            angle1 -= 90
        if angle2 > 45:
            angle2 -= 90
        # avg angle = 0 means pipes are parallel, negative means pipes tilt to left, positive right
        avg_angle = (angle1 + angle2) / 2 #average angle of the pipes to find direction
        # print("Average angle: ", avg_angle)
        return avg_angle


    #takes in a list of contours
    #filters the contours according to 
    def find_pipes(self):
        contours = self.find_dark_blue_contours()
        pipes = [] #pipe looks like -> ((x, y), (w, h), angle)
    
        for contour in contours:
            rect = cv2.minAreaRect(contour)
            (x, y), (w, h), angle = rect
    
            box = cv2.boxPoints(rect)
            box = np.intp(box)
            if w > 1 and h > 1:
                if (w / h < 0.25 or w / h > 2) and cv2.contourArea(contour) > 300 and (w > self.frame.shape[0] - 200 or h > self.frame.shape[0] - 200 or w > self.frame.shape[1] - 200 or h > self.frame.shape[1] - 200):
                    straightRect = cv2.boundingRect(contour)
                    x, y, w, h = straightRect
                    cv2.rectangle(self.frame, (x, y), (x + w, y + h), (255, 100, 0), 2)
                    cv2.drawContours(self.frame, [box], 0, (0, 0, 255), 3)    
                    # print((x, y), (w, h), angle)
    
                    pipes.append(rect)
        if len(pipes) == 2:
            return pipes
            
        else:
            # print("No pipes found")
            return "SKIP"
      
        
    def find_dark_blue_contours(self):
        low_blue_range = (0, 0, 0) #b, g, r, TODO should mabye be (70, 0, 0)
        high_blue_range = (255, 60, 60)
    
        transect_pipe_mask = cv2.inRange(self.frame, low_blue_range, high_blue_range)
        pipe_contours, _ = cv2.findContours(transect_pipe_mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

        # cv2.drawContours(frame, pipe_contours, -1, (0, 255, 0), 5)
        # cv2.imshow("contours", frame)
        # cv2.waitKey(0)
        # cv2.destroyAllWindows()
        
        return pipe_contours
    
    # driving packet: [id, [x, y, z, r, 0, 0, 0, 0]]
    
    def stabilize_angle(self):
        pipes = self.find_pipes()
        if pipes == "SKIP":
            return
            
        
        transect_angle = self.get_angle_between_pipes(pipes[0], pipes[1])
        
        if transect_angle < -2:
            # print("Turn left")
            self.driving_data = [0, 0, 0, -10, 0, 0, 0, 0]
    
        elif transect_angle > 2:
            self.driving_data = [0, 0, 0, 10, 0, 0, 0, 0]
            # print("Turn right")
            
        else:
            # print("Clear for stabilization")
            self.canStabilize = True
        

    def stabilize_alignment(self):
        if self.canStabilize:
            pipes = self.find_pipes()
            if pipes == "SKIP":
                print("SKIPPING FRAME")
                return
            
            # Find leftmost pipe:
            if pipes[0][0] < pipes[1][0]:
                leftPipe = pipes[0]
                rightPipe = pipes[1]
            else:
                leftPipe = pipes[1]
                rightPipe = pipes[0]
            distanceFromLeftPipe = leftPipe[0][0]
            distanceFromRightPipe = self.frame.shape[1] - rightPipe[0][0]
            ratio = distanceFromLeftPipe / distanceFromRightPipe # ratio = 1 means perfect
            
            # print("Distance from left pipe: ", distanceFromLeftPipe)
            # print("Distance from right pipe: ", distanceFromRightPipe)
            # print("Ratio: ", ratio)
            
            if 0.95 > ratio:
                self.driving_data = [-10, 0, 0, 0, 0, 0, 0, 0]
                # print("Move to left")
                
            elif 1.05 < ratio:
                # print("Move to right")
                self.driving_data = [10, 0, 0, 0, 0, 0, 0, 0]
                
            else:
                # print("Go forward")
                self.driving_data = [0, 10, 0, 0, 0, 0, 0, 0]

            self.canStabilize = False
            return
            
        else:
            # print("Waiting for ROV to stabilize angle")
            return
         



    
if __name__ == "__main__":
    frame = cv2.imread("camerafeed/Other_Classes/images/transect1.png")
    transect = AutonomousTransect()
    transect.run(frame)
