from abc import ABC, abstractmethod


class STTInterface(ABC):
    @abstractmethod
    def transcribe(self, audioData: bytes) -> str:
        pass
