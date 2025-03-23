from abc import ABC,abstractmethod


class LLMInterface(ABC):
    @abstractmethod
    def generateResponse(self, prompt) -> str:
        pass
