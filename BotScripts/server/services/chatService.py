from typing import Union
<<<<<<< HEAD
from common.interfaces.llmInterface import LLMInterface
from common.interfaces.sttInterface import STTInterface
from common.interfaces.ttsInterface import TTSInterface
from core.llm import LLM
from core.sttEngine import STTEngine
from core.ttsEngine import TTSEngine
=======
from common.interfaces.ragInterface import RAGInterface
from common.interfaces.sttInterface import STTInterface
from common.interfaces.ttsInterface import TTSInterface
from core.ragEngine import RAGEngine
from core.sttEngine import STTEngine
from core.ttsEngine import TTSEngine
from langgraph.checkpoint.memory import MemorySaver
from langgraph.prebuilt.chat_agent_executor import create_react_agent


GROQ_API_KEY = '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'
VECTOR_DB_PATH = '../chroma_db_1'
>>>>>>> 09265954c4fc3a083dcac05637b5d8818149b6b2


class ChatService:
    def __init__(self):
<<<<<<< HEAD
        self.llm: LLMInterface = LLM()
=======
        self.rag: RAGInterface = RAGEngine(
            groqApiKey=GROQ_API_KEY,
            vectorDbPath=VECTOR_DB_PATH
        )
>>>>>>> 09265954c4fc3a083dcac05637b5d8818149b6b2
        self.stt: STTInterface = STTEngine()
        self.tts: TTSInterface = TTSEngine()
    

    def processTextMessage(self, userId: str, message: str) -> str:
<<<<<<< HEAD
        response = self.llm.generateResponse(message)
        return response
    
=======
        prefix = """
                Ты — ассистент по подбору бытовой техники. Твоя задача:
                1. Найди информацию, связанную с запросом пользователя. Или попроси пользователя уточнить запрос.
                2. Выбери подходящую под запрос пользователя технику. Не подходящую не показывай.
                3. Опиши характеристики подходящей техники. Ее плюсы и минусы, дай рекомендацию пользователю на основе данных.
                4(опционально). Уточни информацию, если пользователь попросит.
                функция GetSuitingProducts - принимает запрос (query) для поиска данных. Ты можешь уточнять запрос по просьбе пользователя, старайся использовать эту функцию реже.
                функция GetProductByModel - принимает модель техники. Возвращает точное описание модели техники при точном совпадении иначе - список похожих моделей.
                Используй GetProductByModel для уточнения характеристик ранее упомянутых тобою моделей техники, если потребуется.
            """
            
        llm, tools = self.rag.getTools()
        memory = MemorySaver()
        agentExecutor = create_react_agent(
            llm=llm,
            tools=tools,
            checkpointer=memory,
            debug=True,
            prompt=prefix
        )
        config = {'configurable': {'thread_id': 'abc123'}}

        result = agentExecutor.invoke({
            'messages': [{'role': 'user', 'content': message}]
        }, config)

        return result['message'][-1].content


>>>>>>> 09265954c4fc3a083dcac05637b5d8818149b6b2

    def processVoiceMessage(self, userId: str, audioData: bytes) -> Union[str, bytes]:
        text = self.stt.transcribe(audioData)
        llmResponse = self.llm.generateResponse(text)

        audioResponse = self.tts.generateAudio(llmResponse)

        return audioResponse
