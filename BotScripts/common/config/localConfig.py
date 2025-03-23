from .baseConfig import BaseConfig


class LocalConfig(BaseConfig):
    DEBUG: bool = True
    LOG_LEVEL: str = 'DEBUG'
