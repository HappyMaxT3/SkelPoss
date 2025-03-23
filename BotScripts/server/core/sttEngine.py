from common.interfaces.sttInterface import STTInterface


class STTEngine(STTInterface):
    def __init__(self, ):
        pass


    def transcribe(self, audioData: bytes) -> str:
        return 'Transribed text from audio'
