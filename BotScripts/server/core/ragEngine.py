from common.interfaces.ragInterface import RAGInterface
from langchain_community.document_loaders import TextLoader
from langchain.text_splitter import CharacterTextSplitter, TextSplitter
from langchain.embeddings import HuggingFaceEmbeddings
from langchain_community.document_loaders.base import BaseLoader
from langchain.embeddings.base import Embeddings

from typing import Optional, Type


class EmbeddingCreator():
    def __init__(
            self,
            loader: Optional[Type[BaseLoader]] = None,
            splitter: Optional[TextSplitter] = None,
            model: Optional[Embeddings] = None,
    ):
        if not loader:
            loader = TextLoader
        if not splitter:
            splitter = CharacterTextSplitter(
                separator="\n\n",
                chunk_size=500,
                chunk_overlap=100,
                length_function=len,
                is_separator_regex=False,
            )
        if not model:
            model = HuggingFaceEmbeddings(model_name='sentence-transformers/all-mpnet-base-v2')
        self.loader = loader
        self.model = model
        self.splitter = splitter

    
    def _loadAndSplitText(self, filePath):
        try:
            loader = self.loader(filePath, encoding='utf-8')
            documents = loader.load()
            chunks = self.splitter.split_documents(documents)

            return chunks
        except Exception as e:
            print(f'Error loading or splitting text: {e}')
            raise
    

    def create(self):
        pass


class RAG(RAGInterface):
    def __init__(self, llm):
        self.llm = llm



    def generate(self, query, context):
        pass