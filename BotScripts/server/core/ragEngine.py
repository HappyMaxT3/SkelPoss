from common.interfaces.ragInterface import RAGInterface
<<<<<<< HEAD
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
=======
from common.utils.vectorstore_utils import load_vectorstore
from common.utils.types import types
from langchain.tools import Tool
from langchain.prompts import PromptTemplate
from langchain_core.output_parsers import StrOutputParser
from langchain_groq import ChatGroq
from langchain_gigachat import GigaChat
from langgraph.checkpoint.memory import MemorySaver
from langgraph.prebuilt.chat_agent_executor import create_react_agent
from collections import Counter
# from Ipython.display import display, Markdown
import os


templates = {
    'get_recomendations_template': PromptTemplate(
        template="""
        Пользователь хочет найти товар подходящий под его описание. СТРОГО следуй такому плану ответа:
        1. Опиши каждый подходящий запросу пользователя товар, дай его необходимые характеристики и немного дополнительных, опиши его плюсы и минусы (ОТНОСИТЕЛЬНО ЗАПРОСА ПОЛЬЗОВАТЕЛЯ), причины, по которым ты выбрал этот товар как подходящий.
        2. Основываясь на своих объективных рассуждениях (на основе характеристик подходящих товаров), сравни подходящие товары между собой (ОТНОСИТЕЛЬНО ЗАПРОСА ПОЛЬЗОВАТЕЛЯ), дай пользователю рекомендации.
        3. ЕСЛИ ТОВАР НЕ ПОДХОДИТ ПОД ЗАПРОС ПОЛЬЗОВАТЕЛЯ, ТЫ ЕГО НЕ ПИШЕШЬ.
        Запрос пользователя: {msg}

        Характеристики товаров: {products}

        Подходящие товары, их описания с плюсами и минусами, их сравние и рекомендации на русском:
        """,
        input_variables=['msg','products']
    ),
    'get_type_prompt': PromptTemplate(
        template = """
        Запрос пользователя: {msg}

        Определи, какие типы товаров из доступных подойдут к этому запросу пользователя.
        В ответе пиши ТОЛЬКО типы товаров в точности как я дал тебе, другие типы писать не разрешается.
        Пиши в строчку через запятую. Если нет подходящих товаров, то напиши "нет".
        ПИШИ ТОЛЬКО ПОДХОДЯЩИЕ ПОД ЗАПРОС ТИПЫ ТОВАРОВ. НЕ ПИШИ БОЛЕЕ 3 ТИПОВ.
        Доступные типы товаров: {types}
        Подходящие типы товаров:
        """,
        input_variables = ['msg','types']
    ),
    'get_chars_prompt': PromptTemplate(
        template="""
        Пожелания пользователя:
        {msg}

        По пожеланиям определи, какие характеристики важны для пользователя.
        Напиши эти характеристики в формате название1: значение1\n название2: значение2\n и т.д.
        Пиши ТОЛЬКО ПАРЫ. Старайся угадать примерные ЧИСЛЕННЫЕ (там где они могут быть численными) значения.
        названия характеристик бери из данных тебе примеров, строго соблюдай их.
        Значения ты должен приблизительно оценить исходя из пожеланий пользователя.
        Пиши конкретные значения, без уточнений, сносок. Если характеристика не относится к пожеланиям пользователя то ты ее не пишешь.


        Примеры характеристик (значения могут не соответствовать пожеланиям):
        {examples}

        """,
        input_variables = ['msg','examples']
    ),
}


class RAGEngine(RAGInterface):
    def __init__(self, groqApiKey: str, vectorDbPath: str, device: str = 'cpu'):
        self.llm = ChatGroq(
            groq_api_key=groqApiKey,
            model_name='llama3-70b-8192',
            temperature=0.5,
            max_tokens=500,
        )
        
        self.llm2 = GigaChat(
            temperature=0.2,
            max_tokens=700,
            verify_ssl_certs=False,
            credentials=os.getenv('GIGACHAT_CREDENTIALS'),
            scope='GIGACHAT_API_PERS',
        )

        self.vectorestore = load_vectorstore(vectorDbPath, device)
        self.get_type_chain = templates['get_type_prompt']|self.llm|StrOutputParser()
        self.get_chars_chain = templates['get_chars_prompt']|self.llm|StrOutputParser()
        self.get_recomendations_chain = templates['get_recomendations_template']|self.llm|StrOutputParser()

    

    def get_suiting_product_data(self, query: str) -> str:
        searched_types = self.get_type_chain.invoke({'msg':query,"types":', '.join(types)}).split(', ')
        #проверка на наличие
        if not len(searched_types) or searched_types[0]=='нет':
            return f"Кажется, нет типов товаров, подходящих под данный запрос. Доступные типы товаров : {', '.join(types)}"
        #сюда 'шаблоны' (примеры документа с товаром) пишем
        product_examples = []
        #накидываем шаблоны
        for x in searched_types:
            if len(self.vectorstore.get(where={"Тип":x},limit=1)['ids']):
                filepath = self.vectorstore.get(where={"Тип":x},limit=1)['metadatas'][0]['source']
            else:
                continue
            with open(filepath,'r') as f:
                product_examples.append(f.read())
        #форматируем в роботочитаемый текст
        product_examples=["Пример "+str(x)+":"+y+'\n\n' for x,y in enumerate(product_examples)]
        #теперь найдем примерные характеристики товара по шаблону
        chars = self.get_chars_chain.invoke({'msg':query,'examples':product_examples}).split('\n')
        #добавим сообщение пользователя для поиска по описанию
        chars.append(query)
        #ищем по этим характеристикам
        filter = {"Тип": {"$in": searched_types}}
        suiting_models = []
        for char in chars:
            res = self.vectorstore.similarity_search(char,filter=filter,k=10)
            models = {x.metadata['Модель'] for x in res}
            suiting_models+=models
        #тут мы короче сделали запрос по каждой характеристике, посчитали кол-во вхождений одной и той же модели
        #(кол-во попадающих под запрос характеристик)
        products =[]
        chosen_models=[x[0] for x in Counter(suiting_models).most_common(3)]
        print(chosen_models)
        for x in chosen_models:
            filepath = self.vectorstore.get(where={"Модель":x},limit=1)['metadatas'][0]['source']
            with open(filepath,'r') as f:
                products.append(f'Модель - {x}\n'+f.read())
        products = '\n'.join(products)
        return products
    

    def get_product_by_model(self, model: str) -> str:
        filter = {"Модель": model}
        if len(self.vectorstore.get(where={"Модель":model},limit=1)['metadatas']):
            filepath = self.vectorstore.get(where={"Модель":model},limit=1)['metadatas'][0]['source']
            with open(filepath,'r') as f:
                return f'Точное совпадение, Модель - {model}\n'+f.read()
        else:
            results = [x.metadata['Модель'] for x in self.vectorstore.similarity_search(f'Модель: {model}',k=5)]
            print(results)
            return "Не найдено точное совпадение, возможно данные отрывки текста смогут помочь?\n"+', '.join(results)
        
    
    def getTools(self):
        tools = [
            Tool(
                name='GetSuitingProducts',
                func=lambda query: self.get_suiting_product_data(query),
                description='Принимает на вход ЗАПРОС (СТРОКУ). Находит подходящие по запросу товары. Указание конкретных характеристик повысит точность.'
            ),
            Tool(
                name='GetProductByModel',
                func=lambda model: self.get_product_by_model(model),
                description='На вход принимает СТРОКУ с названием модели бытовой техники. Возвращает (СТРОКУ) характеристики техники в случае точного совпадения. Иначе вернет список похожишь значений'
            )
        ]
        return self.llm, tools


if __name__ == '__main__':
    GROQ_API_KEY = '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'
    VECTOR_DB_PATH = ''

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

    engine = RAGEngine(
        groqApiKey=GROQ_API_KEY,
        vectorDbPath=VECTOR_DB_PATH,
        device='cpu'
    )

    llm, tools = engine.getTools()
    memory = MemorySaver()
    agentExecutor = create_react_agent(
        llm=llm,
        tools=tools,
        checkpointer=memory,
        debug=False,
        prompt=prefix
    )
    config = {'configurable': {'thread_id': 'abc123'}}

    result = agentExecutor.invoke({
        "messages": [{"role": "user", "content": "Хочу пылесос"}]
    }, config)
    print('Запрос 1: Хочу пылесос')


# class EmbeddingCreator():
#     def __init__(
#             self,
#             loader: Optional[Type[BaseLoader]] = None,
#             splitter: Optional[TextSplitter] = None,
#             model: Optional[Embeddings] = None,
#     ):
#         if not loader:
#             loader = TextLoader
#         if not splitter:
#             splitter = CharacterTextSplitter(
#                 separator="\n\n",
#                 chunk_size=500,
#                 chunk_overlap=100,
#                 length_function=len,
#                 is_separator_regex=False,
#             )
#         if not model:
#             model = HuggingFaceEmbeddings(model_name='sentence-transformers/all-mpnet-base-v2')
#         self.loader = loader
#         self.model = model
#         self.splitter = splitter

    
#     def _loadAndSplitText(self, filePath):
#         try:
#             loader = self.loader(filePath, encoding='utf-8')
#             documents = loader.load()
#             chunks = self.splitter.split_documents(documents)

#             return chunks
#         except Exception as e:
#             print(f'Error loading or splitting text: {e}')
#             raise
    

#     def create(self):
#         pass


# class RAG(RAGInterface):
#     def __init__(self, llm):
#         self.llm = llm



#     def generate(self, query, context):
#         pass
>>>>>>> 09265954c4fc3a083dcac05637b5d8818149b6b2
