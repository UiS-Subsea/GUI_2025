import zmq
import json
import threading
from Thread_info import ThreadWatcher

class CommunicationHandler:
    def __init__(self,
                 rov_data_queue,
                 thread_watcher: ThreadWatcher,
                 id: int,
                 host="127.0.0.1",
                 data_port=5006):
        
        self.context = zmq.Context()

        # PUSH socket to send ROV data to C# (PULL)
        self.push_socket = self.context.socket(zmq.PUSH)
        self.push_socket.connect(f"tcp://{host}:{data_port}")

        self.rov_data_queue = rov_data_queue
        self.thread_watcher = thread_watcher
        self.id = id

    def start(self):
        """Starts sending ROV data to .NET."""
        threading.Thread(target=self.send_rov_data, daemon=True).start()

    def send_rov_data(self):
        while self.thread_watcher.should_run(self.id):
            if not self.rov_data_queue.empty():
                data = self.rov_data_queue.get()

                if isinstance(data, dict) and "autonomdata" in data:
                    payload = data["autonomdata"]
                    if isinstance(payload, list) and len(payload) >= 4:
                        formatted_data = {"autonom_data": payload[:4]}
                        message = json.dumps(formatted_data)
                    else:
                        message = json.dumps({"error": "Invalid payload format"})
                else:
                    message = json.dumps({"error": "Unexpected data structure"})

                try:
                    self.push_socket.send_string(message)
                    print(f"[NETWORK] Sent ROV data: {message}")
                except zmq.ZMQError as e:
                    print(f"[NETWORK] Failed to send ROV data: {e}")


    def stop(self):
        """Stops network operations and closes sockets."""
        self.push_socket.close()
        self.context.term()
