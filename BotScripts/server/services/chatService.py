from typing import Union
from common.interfaces.llmInterface import LLMInterface
from common.interfaces.sttInterface import STTInterface
from common.interfaces.ttsInterface import TTSInterface
from ..core.llm import LLM
from ..core.sttEngine import STTEngine
from ..core.ttsEngine import TTSEngine


class ChatService:
    def __init__(self):
        self.llm: LLMInterface = LLM()
        self.stt: STTInterface = STTEngine()
        self.tts: TTSInterface = TTSEngine()
    

    def processTextMessage(self, message: str) -> str:
        response = self.llm.generateResponse(message)
        return response
    

    def processVoiceMessage(self, audioData: bytes) -> Union[str, bytes]:
        text = self.stt.transcribe(audioData)
        llmResponse = self.llm.generateResponse(text)

        audioResponse = self.tts.generateAudio(llmResponse)

        return audioResponse
