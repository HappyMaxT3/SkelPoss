from abc import ABC, abstractmethod


class DBInterface(ABC):
    @abstractmethod
    def connect(self):
        pass

    
    @abstractmethod
    def query(self, sql: str, params=None):
        pass
