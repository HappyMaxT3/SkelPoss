from common.interfaces.llmInterface import LLMInterface


class LLM(LLMInterface):
    def __init__(self, llm=None):
        self.llm = llm
    

    def generateResponse(self, prompt: str = 'Hello') -> str:
        return f'Response to {prompt}'
