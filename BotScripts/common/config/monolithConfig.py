from .baseConfig import BaseConfig


class MonolithConfig(BaseConfig):
    SERVER_HOST: str = '0.0.0.0'
    SERVER_PORT: int = 8000
    DATABASE_URL: str = 'sqlite:///./test.db'
