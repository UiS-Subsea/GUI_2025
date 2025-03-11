import multiprocessing
import threading
from WebRTC import run_webrtc_server
from communication_handler import CommunicationHandler
from task_manager import TaskManager
from websocket_server import WebSocketServer
from Thread_info import ThreadWatcher


def main():
    # Queues for communication
    command_queue = multiprocessing.Queue()  # Commands from frontend
    rov_data_queue = multiprocessing.Queue()  # ROV data to .NET
    manual_flag = multiprocessing.Value("i", 1)  # 1 = Manual mode, 0 = Autonomy

    # Video Frames to be sent to .NET
    stereo_left_queue = multiprocessing.Queue()
    stereo_right_queue = multiprocessing.Queue()
    down_queue = multiprocessing.Queue()
    manipulator_queue = multiprocessing.Queue()
    frame_queue = [stereo_left_queue, stereo_right_queue, down_queue, manipulator_queue]

    mode_flag = multiprocessing.Value("i", 0)  
    # 0 = No Mode, 1 = Manual, 2 = Docking, 3 = transect, 4 = SeaGrass, 5 = All Cameras, 6 = Test Camera

    # Start thread watcher to manage all threads.
    thread_watcher = ThreadWatcher()

    # Start network handler (communication with .NET using ZeroMQ).
    id = thread_watcher.add_thread()
    communication = CommunicationHandler(
        rov_data_queue,
        thread_watcher,
        id)
    
    communication_thread = threading.Thread(target=communication.start, daemon=True)
    communication_thread.start()


    # Start task manager (controls execution).
    id = thread_watcher.add_thread()
    task_manager = TaskManager(
        command_queue,
        rov_data_queue,
        stereo_left_queue,
        stereo_right_queue,
        down_queue,
        manipulator_queue,
        manual_flag,
        mode_flag, 
        thread_watcher, id)
    
    task_thread = threading.Thread(target=task_manager.run, daemon=True)
    task_thread.start()


    # Start WebSocket server in its own thread (To receive Commands from Frontend).
    id = thread_watcher.add_thread()
    websocket_server = WebSocketServer(command_queue, thread_watcher, id)
    
    websocket_thread = threading.Thread(target=websocket_server.run, daemon=True)
    websocket_thread.start()

    # Start WebRTC server for sending Video Feed to fronted (UDP).
#    id = thread_watcher.add_thread()
#    webrtc_process = multiprocessing.Process(
#        target=run_webrtc_server,
#        args=(frame_queue, mode_flag),
#        daemon=True)
    
#    webrtc_process.start()

    rov_data_queue.put({"autonomdata": [1,2,3,4,0,0,0,0]})

    def cleanup():
        print("\nShutting down safely...")
        communication.stop()
        websocket_server.stop_server()
        thread_watcher.stop_all_threads()

    try:
        while True:
            text = input("Waiting for input")
            if text == "shutdown":
                cleanup()
                break
            pass  # Keep running
    except KeyboardInterrupt:
        print("\nShutting down safely...")
        communication.stop()
        #webrtc_process.terminate()  # Ensure WebRTC process stops
        thread_watcher.stop_all_threads()

if __name__ == "__main__":
    main()



# TO DO List

# 3. Need to find a way to test if it works

# 3.1 Might try to send fake UDP Gstream from test server (find right format videos)

# 3.2 Check if its able to send WebRTC to frontend and display video feed

# 3.4 check that task_manager works as intended(work for docking, gets same error as old GUI)