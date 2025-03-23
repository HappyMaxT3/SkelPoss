import uvicorn  
from fastapi import FastAPI
from .routers.auth import router as authRouter
from .routers.chat import router as chatRouter
from common.config.monolithConfig import MonolithConfig


app = FastAPI(
    title='Opossum Server API',
    description='API for Opossum server',
    version='1.0.0'
)

app.include_router(chatRouter)
app.include_router(authRouter)


@app.get('/', tags=['Main'])
async def main():
    return {'message': 'Server is running'}


if __name__ == '__main__':
    # load_dotenv('../.env')
    # appHost = os.getenv('APP_HOST', '127.0.0.1')
    # appPort = int(os.getenv('APP_PORT', '8000'))
    # print(f'Server will run on {appHost}:{appPort}')
    config = MonolithConfig()
    
    uvicorn.run(
        'main:app',
        host=config.SERVER_HOST,
        port=config.SERVER_PORT,
        reload=True
    )
