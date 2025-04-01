import asyncio
import websockets
import json
import queue
from Thread_info import ThreadWatcher

class WebSocketServer:
    def __init__(
            self,
            command_queue: queue.Queue, 
            thread_watcher: ThreadWatcher, 
            id: int):
        self.command_queue = command_queue
        self.thread_watcher = thread_watcher
        self.id = id
        self.websocket = None  # Store reference to the active websocket connection
        self.server_address = "ws://localhost:8765"
        self.server = None

    async def handle_connection(self, websocket, path=None):
        """Handles a single WebSocket connection and listens for messages."""
        print("[WEBSOCKET] Frontend connected")
        self.websocket = websocket  # Store the active connection

        try:
            while self.thread_watcher.should_run(self.id):
                # Continuously listen for messages from the client
                message = await websocket.recv()  # Non-blocking wait for message
                self.on_message(message)  # Process incoming message

        except websockets.exceptions.ConnectionClosed:
            print("[WEBSOCKET] Frontend disconnected")

    def on_message(self, message):
        """Handles an incoming message from the WebSocket."""
        try:
            data = json.loads(message)
            command = data.get("command")

            if command:
                print(f"[WEBSOCKET] Received command: {command}")
                self.command_queue.put(command)  # Place command in queue
            else:
                print("[WEBSOCKET] Invalid message format, missing 'command'.")
        except json.JSONDecodeError:
            print("[WEBSOCKET] Failed to parse JSON message.")

    async def start_server(self):
        """Starts the WebSocket server and listens for incoming connections."""
        try:
            self.server = await websockets.serve(self.handle_connection, "localhost", 8765)
            print(f"[WEBSOCKET] WebSocket server started on {self.server_address}")

            # Monitor server until shutdown is requested
            while self.thread_watcher.should_run(self.id):
                await asyncio.sleep(1)  # Wait to ensure the server stays up
        except Exception as e:
            print(f"[WEBSOCKET] Server error: {str(e)}")
        finally:
            print("[WEBSOCKET] Server stopped.")
            await self.cleanup()

    async def cleanup(self):
        """Cleanup resources before shutdown."""
        if self.websocket:
            await self.websocket.close()  # Close the websocket connection if open
            print("[WEBSOCKET] WebSocket connection closed.")


    async def reconnect(self):
        """Attempts to reconnect the WebSocket connection."""
        print("[WEBSOCKET] Attempting to reconnect...")
        while self.thread_watcher.should_run(self.id):
            try:
                self.websocket = await websockets.connect(self.server_address)
                print("[WEBSOCKET] Reconnected to frontend.")
                break  # Exit reconnect loop once connected

            except websockets.exceptions.ConnectionClosedError:
                print("[WEBSOCKET] Reconnection failed. Retrying...")
                await asyncio.sleep(5)  # Wait before retrying connection

    def run(self):
        """Runs the WebSocket server in a separate thread."""
        self.loop = asyncio.new_event_loop()  # Create a new event loop
        asyncio.set_event_loop(self.loop)  # Set the event loop for the current thread

        self.loop.run_until_complete(self.start_server()) # Run the server in the event loop

    async def stop_server(self):
        """Handles graceful shutdown of the server when a signal is received."""
        print("[WEBSOCKET] Received shutdown signal. Stopping server...")

        if self.websocket:
            print("[WEBSOCKET]Closing active connections...")
            await  self.websocket.close() # Wait for WebSocket to close

        if self.server:
            print("[WEBSOCKET] Closing WebSocket server...")
            self.server.close()
            await self.server.wait_closed()  # Ensure the server is fully closed

        print("[WEBSOCKET] Shutdown complete.")