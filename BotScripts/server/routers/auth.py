from fastapi import APIRouter, Depends, HTTPException
from ..services.authService import AuthService
from ..core.database import Database
from pydantic import BaseModel


router = APIRouter(prefix='/auth', tags=['Auth'])


class RegisterRequest(BaseModel):
    username: str
    password: str  # Можно добавить валидацию: constr(min_length=8)


class LoginRequest(BaseModel):
    username: str
    password: str


def getDatabase():
    return Database(dbUrl='sqlite:///./test.db')


def getAuthService(db: Database = Depends(getDatabase)):
    return AuthService(db)


@router.post('/register', summary='Регистрация нового пользователя')
async def register(
    request: RegisterRequest,
    authService: AuthService = Depends(getAuthService)
):
    try:
        user = authService.register_user(request.username, request.password)
        return {'message': 'User registered successfully', 'user_id': user.id}
    except HTTPException as e:
        raise e


@router.post('/login', summary='Вход пользователя')
async def login(
    request: LoginRequest,
    authService: AuthService = Depends(getAuthService)
):
    try:
        user = authService.authenticate_user(request.username, request.password)
        return {'message': 'Login successful', 'user_id': user.id}
    except HTTPException as e:
        raise e
