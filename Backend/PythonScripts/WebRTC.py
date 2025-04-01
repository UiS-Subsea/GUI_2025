import asyncio
import time

import numpy as np
from aiortc import RTCPeerConnection, RTCSessionDescription, VideoStreamTrack
from av import VideoFrame
from aiohttp import web
import aiohttp_cors
import multiprocessing


class WebRTCServer:
    def __init__(self, frame_queues, mode_value):
        self.frame_queues = frame_queues
        self.mode_value = mode_value
        self.pcs = set()
        self.active_flags = [multiprocessing.Value("b", False) for _ in frame_queues]
        self.server_runner = None
        self.server_site = None

    class ProcessedFrameStream(VideoStreamTrack):
        """ This is the inner class that processes the video frames and streams them """
        def __init__(self, frame_queue, active_flag):
            super().__init__()
            self.frame_queue = frame_queue
            self.active_flag = active_flag
            self.last_time = time.time()  # For FPS calculation
            self.frame_count = 0  # To count the number of frames
            self.fps = 0  # To store the calculated FPS
            self.first_frame_sent = False

        async def recv(self):
            while True:
                # To send one black frame to frontend to mute the tracks as start status.
                if not self.first_frame_sent:
                    self.first_frame_sent = True
                    current_time = time.time()

                    pts, time_base = await self.next_timestamp()
                    dummy_frame = np.zeros((720, 1280, 3), dtype=np.uint8)  # Black frame
                    dummy_video_frame = VideoFrame.from_ndarray(dummy_frame, format="bgr24")
                    dummy_video_frame.pts = pts
                    dummy_video_frame.time_base = time_base
                    return dummy_video_frame  # Return the dummy frame to the track

                if not self.active_flag.value:
                    await asyncio.sleep(0.5)
                    continue
                current_time = time.time()
            
                if self.frame_queue.empty():
                    await asyncio.sleep(0.01)
                    continue

                # Track the time before getting the frame
                current_time = time.time()

                pts, time_base = await self.next_timestamp()

                frame = self.frame_queue.get()

                # Calculate FPS (every 1 second)
                self.frame_count += 1
                time_diff = current_time - self.last_time

                if time_diff >= 1.0:  # If 1 second has passed
                    self.fps = self.frame_count / time_diff
                    self.frame_count = 0 
                    self.last_time = current_time
                    print(f"FPS: {self.fps:.2f}")

                video_frame = VideoFrame.from_ndarray(frame, format="bgr24")
                video_frame.pts = pts
                video_frame.time_base = time_base

                return video_frame

    async def handle_peerjs_offer(self, request):
        """ Handles WebRTC offers from PeerJS clients """
        pc = RTCPeerConnection()
        self.pcs.add(pc)

        try:
            data = await request.json()

            offer = RTCSessionDescription(sdp=data["sdp"], type=data["type"])
            await pc.setRemoteDescription(offer)

            print("[WebRTC] Received offer from PeerJS client")

            # Attach the correct video streams
            for i, queue in enumerate(self.frame_queues):
                track = self.ProcessedFrameStream(queue, self.active_flags[i])
                pc.addTrack(track)

            # Create an SDP answer and send it back
            answer = await pc.createAnswer()
            await pc.setLocalDescription(answer)

            print("[WebRTC] Sending Answer to PeerJS Client")
            return web.json_response({
                "sdp": pc.localDescription.sdp,
                "type": pc.localDescription.type
            })

        except Exception as e:
            print(f"[Error] WebRTC handling failed: {e}")
            return web.Response(status=500)

        finally:
            self.pcs.discard(pc)

    async def start_peer_server(self):
        """ Starts the aiohttp web server for handling PeerJS signaling requests """
        app = web.Application()

        # Set up CORS
        cors = aiohttp_cors.setup(app, defaults={
            "*": aiohttp_cors.ResourceOptions(
                allow_credentials=True,
                expose_headers="*",
                allow_headers="*"
            )
        })

        app.router.add_post("/connect", self.handle_peerjs_offer)

        # Enable CORS for the route
        for route in list(app.router.routes()):
            cors.add(route)

        self.server_runner = web.AppRunner(app)
        await self.server_runner.setup()

        self.server_site = web.TCPSite(self.server_runner, "0.0.0.0", 9001)  # Start server on port 9001
        await self.server_site.start()
        print("[WebRTC] PeerJS backend running on port 9001")

    async def watch_mode_and_update_streams(self):
        """ Update active flags based on mode """
        prev_mode = None

        while True:
            current_mode = self.mode_value.value

            if current_mode != prev_mode:
                print(f"[WebRTC] Mode changed to: {current_mode}")

                # Update active flags for video streams based on mode
                mode_configs = {
                    0: [False, False, False, False],  # Disable all
                    1: [False, False, True, True],  # Manual mode
                    2: [False, False, True, True],  # Docking
                    3: [False, False, True, False],  # Transect
                    4: [True, True, True, False],  # Seagrass
                    5: [True, True, True, True],  # All cameras
                    6: [True, False, True, False]  # Test mode
                }
                # What each index represents:
                #[stereo_left_queue, stereo_right_queue, down_queue, manipulator_queue]

                for i in range(len(self.active_flags)):
                    self.active_flags[i].value = mode_configs.get(current_mode, [False] * len(self.active_flags))[i]

                prev_mode = current_mode

            await asyncio.sleep(1)

    async def main(self):
        """ Starts the WebRTC server and mode watching concurrently """
        await asyncio.gather(
            self.start_peer_server(),  # Start the signaling server
            self.watch_mode_and_update_streams() # Watch and update streams based on mode
        )

    async def shutdown(self):
        """ Shutdown the WebRTC server and clean up resources """
        print("[WebRTC] Shutting down...")

        # Close active WebRTC connections gracefully
        tasks = [pc.close() for pc in self.pcs]
        self.pcs.clear()  # Clear the set of peer connections
        await asyncio.gather(*tasks)  # Ensure all peer connections are closed

        # Stop the HTTP server on port 9001
        if self.server_site:
            print("[WebRTC] Stopping HTTP server on port 9001...")
            await self.server_site.stop()  # Gracefully stop the server

        if self.server_runner:
            print("[WebRTC] Cleaning up the HTTP server runner...")
            await self.server_runner.cleanup()  # Cleanup the server runner

        print("[WebRTC] Shutdown complete.")

    def run(self):
        """ Starts the WebRTC server as a separate process """
        print("[WebRTC] Starting the loop")
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        loop.run_until_complete(self.main())  # Run the server
