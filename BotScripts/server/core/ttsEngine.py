from common.interfaces.ttsInterface  import TTSInterface


class TTSEngine(TTSInterface):
    def __init__(self, ):
        pass


    def generateAudio(self, text: str) -> bytes:
        return b'Generated audio data'
