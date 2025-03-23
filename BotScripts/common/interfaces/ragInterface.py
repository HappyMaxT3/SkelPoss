from abc import ABC, abstractmethod
import asyncio


class RAGInterface(ABC):
    @abstractmethod
    async def generate(self, query, context):
        pass
