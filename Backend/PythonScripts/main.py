import asyncio
import multiprocessing
import threading
from WebRTC import WebRTCServer
from communication_handler import CommunicationHandler
from task_manager import TaskManager
from websocket_server import WebSocketServer
from Thread_info import ThreadWatcher

# Method for graceful shutdown.
async def cleanup(thread_watcher, 
                  task_manager, 
                  communication, 
                  websocket_server, 
                  webrtc_server):
    
    print("\nShutting down safely...")
    
    thread_watcher.stop_all_threads()
    task_manager.stop_all_tasks()
    communication.stop()
    await websocket_server.stop_server()
    await webrtc_server.shutdown()
    print("[Main] Cleanup complete")


def main():
    # Queues for communication
    command_queue = multiprocessing.Queue()  # Commands from frontend
    rov_data_queue = multiprocessing.Queue()  # ROV data to .NET

    manual_flag = multiprocessing.Value("i", 1)  # 1 = Manual mode, 0 = Autonomy

    # Video Frames to be sent to .NET
    stereo_left_queue = multiprocessing.Queue(15)
    stereo_right_queue = multiprocessing.Queue(15)
    down_queue = multiprocessing.Queue(15)
    manipulator_queue = multiprocessing.Queue(15)
    frame_queue = [stereo_left_queue, stereo_right_queue, down_queue, manipulator_queue]

    # 0 = No Mode, 1 = Manual, 2 = Docking, 3 = transect, 4 = SeaGrass, 5 = All Cameras, 6 = Test Camera
    mode_flag = multiprocessing.Value("i", 0)  

    # Start thread watcher to manage all threads.
    thread_watcher = ThreadWatcher()

    # Start network handler (communication with .NET using ZeroMQ).
    id = thread_watcher.add_thread()
    communication = CommunicationHandler(
        rov_data_queue,
        thread_watcher,
        id)
    
    communication_thread = threading.Thread(target=communication.send_rov_data, daemon=True)
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
    webrtc_server = WebRTCServer(frame_queue, mode_flag)

    webrtc_thread = threading.Thread(
        target=webrtc_server.run, daemon=True)
    
    webrtc_thread.start()

    try:
        while True:
            text = input("Waiting for input")
            if text == "shutdown":
                asyncio.run(cleanup(thread_watcher, task_manager, communication, websocket_server, webrtc_server))
                break
            pass  # Keep running

    except KeyboardInterrupt:
        print("\nShutting down safely...")
        asyncio.run(cleanup(thread_watcher, task_manager, communication, websocket_server, webrtc_server))

if __name__ == "__main__":
    main()