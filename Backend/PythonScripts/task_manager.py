import multiprocessing
import threading
import queue
import time

from Thread_info import ThreadWatcher
from camerafeed.GUI_Camerafeed_Main import CameraManager, ExecutionClass

class TaskManager:
    def __init__(
            self,
            command_queue: multiprocessing.Queue,
            rov_data_queue: multiprocessing.Queue,
            stereo_left_queue: multiprocessing.Queue,
            stereo_right_queue: multiprocessing.Queue,
            down_queue: multiprocessing.Queue,
            manipulator_queue: multiprocessing.Queue,
            manual_flag,
            mode_flag,
            thread_watcher: ThreadWatcher,
            id: int):
        
        self.command_queue = command_queue
        self.stereo_left_queue = stereo_left_queue
        self.stereo_right_queue = stereo_right_queue
        self.down_queue = down_queue
        self.manipulator_queue = manipulator_queue

        self.execution = ExecutionClass(
            rov_data_queue,
            stereo_left_queue,
            stereo_right_queue,
            down_queue,
            manipulator_queue,
            manual_flag)
        
        self.camera = CameraManager()
        self.current_task = None
        self.running = True
        self.manual_flag = manual_flag
        self.mode_flag = mode_flag
        self.active_task_thread = None  # Reference to the running thread
        self.image_save_thread = None
        self.thread_watcher = thread_watcher
        self.id = id

    def run(self):
        while self.thread_watcher.should_run(self.id): 
            try:
                command = self.command_queue.get(timeout=1)
                self.execute_command(command)
            except queue.Empty:
                time.sleep(0.5)
                pass

    def execute_command(self, command):
        print(f"[TASK MANAGER] Received command: {command}")

        valid_commands = {
            "START_CAMERA": self.start_camera,
            "START_PIPELINE": self.start_pipeline,
            "START_DOCKING": self.start_docking,
            "START_TEST": self.start_test_camera, # Not in use.
            "START_MANUAL": self.start_manual,
            "SAVE_IMAGE": self.save_image,
        }

        if command in valid_commands:
            valid_commands[command]()  # Call the corresponding function
        else:
            print(f"[TASK MANAGER] Unknown command: {command}")

    def stop_all_tasks(self):
        if self.current_task:
            print("[TASK MANAGER] Stopping current task")
            self.execution.stop_everything()
            self.current_task = None
            self.mode_flag.value = 0
            self.manual_flag.value = 1  # Switch to manual mode

    def start_camera(self): # This is not in use right now.
        self.stop_all_tasks()
        print("[TASK MANAGER] Starting camera feed")
        self.current_task = "CAMERA"
        self.mode_flag.value = 5
        self.manual_flag.value = 1
        self.start_task_in_thread(self.execution.show_all_cameras)

    def start_pipeline(self):
        self.stop_all_tasks()
        print("[TASK MANAGER] Starting pipeline mode")
        self.current_task = "PIPELINE"
        self.mode_flag.value = 3
        self.manual_flag.value = 0
        self.start_task_in_thread(self.execution.pipeline)

    def start_docking(self):
        self.stop_all_tasks()
        print("[TASK MANAGER] Starting docking mode")
        self.current_task = "DOCKING"
        self.mode_flag.value = 2
        self.manual_flag.value = 0
        self.start_task_in_thread(self.execution.docking)

    def save_image(self):
        print("[TASK MANAGER] Save Image")
        self.image_save_thread = threading.Thread(target=self.execution.save_image, daemon=True)
        self.image_save_thread.start()

    def start_test_camera(self): # This is not in use right now.
        self.stop_all_tasks()
        print("[TASK MANAGER] Running camera test")
        self.current_task = "TEST"
        self.mode_flag.value = 5
        self.manual_flag.value = 0
        self.start_task_in_thread(self.execution.camera_test)

    def start_manual(self): # might need camera open before(all or some)
        """Switch to manual mode"""
        self.stop_all_tasks()
        print("[TASK MANAGER] Switching to manual mode")
        self.current_task = "MANUAL"
        self.mode_flag.value = 1
        self.manual_flag.value = 1
        self.start_task_in_thread(self.execution.show_manual_cameras)

    def start_task_in_thread(self, task_function):
        """Start a task in a separate thread"""
        if self.active_task_thread and self.active_task_thread.is_alive():
            print("[TASK MANAGER] Stopping currently running task before starting new one.")
            self.active_task_thread.join()  # Wait for the current task to finish

        # Start the new task in a separate thread
        self.active_task_thread = threading.Thread(target=task_function, daemon=True)
        self.active_task_thread.start()
