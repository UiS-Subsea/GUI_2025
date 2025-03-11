class ThreadWatcher:
    def __init__(self) -> None:
        self.id = -1
        self.threads = {}

    def add_thread(self) -> int:
        self.id += 1
        self.threads[self.id] = True
        return self.id

    def should_run(self, id)-> bool:
        if id in self.threads:
            return self.threads[id]
        return True

    def stop_thread(self, id):
        self.threads[id] = False

    def stop_all_threads(self):
        print("ThreadWatch: attempting to stop all threads")
        for i in range(len(self.threads)):
            self.stop_thread(i)
        print(self.threads)
        
    def __str__(self):
        return str(self.threads)
