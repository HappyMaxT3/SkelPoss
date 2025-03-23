from abc import ABC, abstractmethod


class TTSInterface(ABC):
    @abstractmethod
    def generateAudio(self, text: str) -> bytes:
        pass
