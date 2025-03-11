from PIL import Image
import cv2
import time
import numpy as np
import matplotlib
from matplotlib.pyplot import imshow
from matplotlib import pyplot as plt

# Monitor seagrass
# Detect whether there is more or less seagrass between two images.
# Seagrass represented by 8x8 grid with green and white tiles. Green = seagrass, white = not seagrass.
# Illustrated on page 31 here: https://files.materovcompetition.org/2023/EXPLORER_Prop_Building_2023_Final.pdf

def calculate_seagrass_percent(img_name): # Takes in image name, returns percentage of area covered in seagrass. Example: image1.png
    img_path = "monitor_seagrass\images\\" + img_name
    
    # RGB values from example image:
    # Green squares:     rgba(5,   102, 69,  255)
    # White squares:     rgba(171, 186, 219, 255)
    # White background:  rgba(179, 196, 216, 255)
    # grey square lines: rgba(120, 145, 172, 255)
    
    with Image.open(img_path) as img:
        img = img.convert("RGB") # instead of RGBA for efficiency
        grey_px_count = 0
        size_x, size_y = img.size
        px = img.load() # Pixel matrix
        
        test_img = Image.new(mode = "RGB", size= (size_x, size_y), color=(255, 255, 255))
        
        for x in range(size_x):
            for y in range(size_y):
                if is_grey(px[x, y]):
                    grey_px_count += 1
                    test_img.putpixel((x, y), px[x, y])
        
        ## gray image + Gaussian Blur
        #img = cv2.imread(img_path)
        ##converted = convert_hls(img)
        #gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
        #kernel_size = 5
        #blur_gray = cv2.GaussianBlur(gray,(kernel_size, kernel_size),0)
        #
        ##Edge detection w canny
        #low_threshold = 50
        #high_threshold = 150
        #edges = cv2.Canny(blur_gray, low_threshold, high_threshold)
        #
        ##HoughLinesP to get lines
        #rho = 1  # distance resolution in pixels of the Hough grid
        #theta = np.pi / 180  # angular resolution in radians of the Hough grid
        #threshold = 10  # minimum number of votes (intersections in Hough grid cell)
        #min_line_length = 30  # minimum number of pixels making up a line
        #max_line_gap = 10  # maximum gap in pixels between connectable line segments
        #line_image = np.copy(img) * 0  # creating a blank to draw lines on
#
        ## Run Hough on edge detected image
        ## Output "lines" is an array containing endpoints of detected line segments
        #lines = cv2.HoughLinesP(edges, rho, theta, threshold, np.array([]),
        #            min_line_length, max_line_gap)
#
        #for line in lines:
        #    for x1,y1,x2,y2 in line:
        #        cv2.line(line_image,(x1,y1),(x2,y2),(255,0,0),5)
        #
        #lines_edges = cv2.addWeighted(img, 0.8, line_image, 1, 0)
        #cv2.imshow("res", lines_edges)
        #cv2.waitKey(0)  
        
        return test_img, grey_px_count
                    
                                     
def is_grey(px):
    if 80 < px[0] < 155 and 130 < px[1] < 175 and 130 < px[2] < 205:
        return True
    return False

def detect_squares(img_path):
    squares = 0
    
    img = cv2.imread(img_path)
    
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    #cv2.imshow("gray", gray)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)
    #cv2.imshow("blur", blur)
    #canny = cv2.Canny(blur, 50, 200)
    #cv2.imshow("canny", canny)
    dilated = cv2.dilate(blur, None, iterations=3)
    #cv2.imshow("dilated", dilated)
    
    _, thresh = cv2.threshold(dilated, 127, 255, cv2.THRESH_BINARY)
    #cv2.imshow("thresh", thresh)
    #cv2.waitKey(0)
    contours, _ = cv2.findContours(thresh, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
    
    for contour in contours:
        # epsilon value can be tweaked
        epsilon = 0.03*cv2.arcLength(contour, True)
        # approx is the polygonal approximation of the contour
        approx = cv2.approxPolyDP(contour, epsilon, True)
        cv2.drawContours(dilated, [approx], 0, (0), 3)
        
        if len(approx) == 4: # 4 sides means a square
            i, j = approx[0][0]
            # x,y top left corner. w,h width and height
            x, y, w, h = cv2.boundingRect(contour)
            ratio = float(w)/h
            # how long a square side needs to be in order to be counted, to remove noise
            noise_threshhold = 20
            # ratio between 0.9 and 1.1 means a square
            if  0.9 <= ratio <= 1.1 and w > noise_threshhold < h:
                squares += 1
                cv2.putText(dilated, 'Square', (i, j), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0))
    #print(squares)
    #cv2.imshow("res", dilated)
    #cv2.waitKey(0)
    return squares, dilated



class SeagrassMonitor:
    def __init__(self):
        self.done = False
        self.seagrass_counter = 0
        self.growth = 0 
        self.frame = None
        self.prev_frame = None
        self.next_frame = None
        self.counter = 0
        
    def run(self, frame_under):
        self.frame = frame_under
        self.counter += 1
        if self.counter == 1:
            self.prev_frame = self.frame
        if self.counter == 2:
            self.next_frame = self.frame
            squares_before = self.detect_squares(self.prev_frame)
            squares_after = self.detect_squares(self.next_frame)
            self.growth = self.calculate_seagrass(squares_before, squares_after) 
            self.counter = 0

        return self.growth

    def detect_squares(self, frame):
        squares = 0
        
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        blur = cv2.GaussianBlur(gray, (5, 5), 0) 
        dilated = cv2.dilate(blur, None, iterations=3)
        _, thresh = cv2.threshold(dilated, 127, 255, cv2.THRESH_BINARY)
        
        contours, _ = cv2.findContours(thresh, cv2.RETR_LIST, cv2.CHAIN_APPROX_SIMPLE)
        
        for contour in contours:
            # epsilon value can be tweaked
            epsilon = 0.03*cv2.arcLength(contour, True)
            # approx is the polygonal approximation of the contour
            approx = cv2.approxPolyDP(contour, epsilon, True)
            cv2.drawContours(dilated, [approx], 0, (0), 3)
            
            if len(approx) == 4: # 4 sides means a square
                i, j = approx[0][0]
                # x,y top left corner. w,h width and height
                x, y, w, h = cv2.boundingRect(contour)
                ratio = float(w)/h
                # how long a square side needs to be in order to be counted, to remove noise
                noise_threshhold = 20
                # ratio between 0.9 and 1.1 means a square
                if  0.9 <= ratio <= 1.1 and w > noise_threshhold < h:
                    squares += 1
                    cv2.putText(dilated, 'Square', (i, j), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0))
        #print(squares)
        #cv2.imshow("res", dilated)
        #cv2.waitKey(0)
        return squares
    
    def calculate_seagrass(self, squares_before, squares_after):
        percentage_difference = (squares_after / squares_before) * 100
        return percentage_difference
                

if __name__ == "__main__":
    #picture, amount = calculate_seagrass_percent("Example1_grey.png")
    #print(amount)
    #picture.show()
    # tik = time.time()
    # squares, img = detect_squares("monitor_seagrass\images\Example1.png")
    # tok = time.time()
    # print("tid brukt1:" + str(round(tok - tik, 4)))
    # tik = time.time()
    # squares2, img2 = detect_squares("monitor_seagrass\images\Example2.png")
    # tok = time.time()
    # print("tid brukt2:" + str(round(tok - tik, 4)))
    # print(squares, squares2)
    #cv2.imshow("res", img)
    #cv2.imshow("res2", img2)
    #cv2.waitKey(0)

    grass1 = cv2.imread("monitor_seagrass\images\Example1.png")
    grass2 = cv2.imread("monitor_seagrass\images\Example2.png")
    seagrass_monitor = SeagrassMonitor()
    seagrass_monitor.update(grass1)
    seagrass_monitor.update(grass2)
    seagrass_monitor.update()
    print(seagrass_monitor.growth)
