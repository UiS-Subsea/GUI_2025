import asyncio
import json
import signal
import websockets
import cv2
from aiortc import RTCPeerConnection, RTCSessionDescription, VideoStreamTrack
from av import VideoFrame

# Global peer connection (one-to-one)
pcs = set()

class ProcessedFrameStream(VideoStreamTrack):
    """ WebRTC VideoStreamTrack that streams processed frames """
    def __init__(self, frame_queue, active_flag):
        super().__init__()
        self.frame_queue = frame_queue  # This is a specific camera's queue
        self.active_flag = active_flag  # Controls if this stream is active

    async def recv(self):
        while True:
            if not self.active_flag.value:  # Check if stream should be active
                await asyncio.sleep(0.1) # Pause when inactive
                continue

            if self.frame_queue.empty():
                await asyncio.sleep(0.01)
                continue

            frame = self.frame_queue.get()
            frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            video_frame = VideoFrame.from_ndarray(frame, format="rgb24")
            return video_frame


async def handle_offer(websocket, frame_queues, active_flags):
    """ Handles WebRTC signaling """
    global pcs
    pc = RTCPeerConnection()
    pcs.add(pc)

    try:
        async for message in websocket:
            data = json.loads(message)

            if data["type"] == "offer":
                print("[WebRTC] Received WebRTC offer")

                # Add all tracks but let them be inactive when needed
                for queue, flag in zip(frame_queues, active_flags):
                    track = ProcessedFrameStream(queue, flag)
                    pc.addTrack(track)

                # Set remote description
                offer = RTCSessionDescription(sdp=data["sdp"], type=data["type"])
                await pc.setRemoteDescription(offer)

                # Create answer
                answer = await pc.createAnswer()
                await pc.setLocalDescription(answer)

                # Send back answer
                response = json.dumps({
                    "sdp": pc.localDescription.sdp,
                    "type": pc.localDescription.type
                })
                await websocket.send(response)
                print("[WebRTC] Sent WebRTC answer")

    except websockets.exceptions.ConnectionClosed:
        print("[WebRTC] WebSocket closed")
    finally:
        pcs.discard(pc)
        await pc.close()

async def watch_mode_and_update_streams(mode_value, active_flags):
    """ Monitors mode and enables/disables streams dynamically """
    prev_mode = None

    while True:
        current_mode = mode_value.value

        if current_mode != prev_mode:
            print(f"[WebRTC] Mode changed to: {current_mode}")
            
            # Update which streams are active (but keep connections open)
            if current_mode == 0:  # Disable all streams
                active_flags[0] = False  # Stereo Left
                active_flags[1] = False  # Stereo Right
                active_flags[2] = False  # Down
                active_flags[3] = False  # Manipulator
            elif current_mode == 1:  # Manual mode (dont have any in code??)
                active_flags[0] = True
                active_flags[1] = True
                active_flags[2] = True
                active_flags[3] = False
            elif current_mode == 2:  # Docking mode (need to check if it should be both stereo)
                active_flags[0] = True
                active_flags[1] = True
                active_flags[2] = False
                active_flags[3] = False
            elif current_mode == 3:  # Transect mode
                active_flags[0] = False
                active_flags[1] = False
                active_flags[2] = True
                active_flags[3] = False
            elif current_mode == 4:  # SeaGras mode (Doesn't seem to be implemented)
                active_flags[0] = True
                active_flags[1] = True
                active_flags[2] = True
                active_flags[3] = False
            elif current_mode == 5:  # All Camera mode
                active_flags[0] = True
                active_flags[1] = True
                active_flags[2] = True
                active_flags[3] = True
            elif current_mode == 6:  # Test Camera mode
                active_flags[0] = True
                active_flags[1] = False
                active_flags[2] = True
                active_flags[3] = False
            else:
                for flag in active_flags:
                    flag = False  # Disable all streams

            prev_mode = current_mode

        await asyncio.sleep(1)  # Poll every second


def graceful_shutdown():
    """ Cleans up on shutdown """
    global pcs
    print("[WebRTC] Shutting down WebRTC connections...")
    for pc in pcs:
        asyncio.create_task(pc.close())  # Ensure cleanup happens in event loop
    pcs.clear()
    exit(0)

async def start_servers(frame_queues, mode_value):
    """ Starts WebRTC signaling server and mode watcher """
    active_flags = [False, False, False, False]

    signal_server = websockets.serve(
        lambda websocket, path: handle_offer(websocket, path, frame_queues, active_flags),
        "localhost", 8766
    )
    
    await asyncio.gather(
        signal_server,
        watch_mode_and_update_streams(mode_value, active_flags)  # Background task
    )

def run_webrtc_server(frame_queues, mode_value):
    """ Runs WebRTC and WebSocket servers in an event loop """
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)

    # Handle process exit signals
    signal.signal(signal.SIGINT, lambda sig, frame: graceful_shutdown())
    signal.signal(signal.SIGTERM, lambda sig, frame: graceful_shutdown())

    loop.run_until_complete(start_servers(frame_queues, mode_value))
