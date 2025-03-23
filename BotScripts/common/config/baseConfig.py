import os
from pydantic_settings import BaseSettings


class BaseConfig(BaseSettings):
    APP_NAME: str = 'Async Chat Server'
    DEBUG: bool = False
    LOG_LEVEL: str = 'INFO'

    class Config:
        env_file = r'D:/Programms/PetProjects/opossumBot/.env'
        extra = 'ignore'
