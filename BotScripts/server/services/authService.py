from fastapi import HTTPException
from passlib.context import CryptContext
from core.database import Database
from models.user import User


pwd_context = CryptContext(schemes=['bcrypt'], deprecated='auto')


class AuthService:
    def __init__(self):
        self.db = Database()


    def verify_password(self, plain_password: str, hashed_password: str) -> bool:
        return pwd_context.verify(plain_password, hashed_password)


    def get_password_hash(self, password: str) -> str:
        return pwd_context.hash(password)


    def register_user(self, username: str, password: str) -> User:
        existing_user = self.db.query('SELECT * FROM users WHERE username = %s', (username,))
        if existing_user:
            raise HTTPException(status_code=400, detail='Username already exists')

        hashed_password = self.get_password_hash(password)

        user_id = self.db.query(
            'INSERT INTO users (username, password) VALUES (%s, %s) RETURNING id',
            (username, hashed_password)
        )
        return User(id=user_id, username=username)


    def authenticate_user(self, username: str, password: str) -> User:
        user_data = self.db.query('SELECT * FROM users WHERE username = %s', (username,))
        if not user_data:
            raise HTTPException(status_code=400, detail='Invalid username or password')

        user = User(**user_data[0])

        if not self.verify_password(password, user.password):
            raise HTTPException(status_code=400, detail='Invalid username or password')

        return user
