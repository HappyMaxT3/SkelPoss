from fastapi import APIRouter, Depends, HTTPException, UploadFile, File, Form
from services.chatService import ChatService
from common.interfaces.llmInterface import LLMInterface
from common.interfaces.sttInterface import STTInterface
from common.interfaces.ttsInterface import TTSInterface
from pydantic import BaseModel


router = APIRouter(prefix='/chat', tags=['Chat'])

ALLOWED_MIME_TYPES = {'audio/mpeg', 'audio/wav', 'audio/ogg', 'audio/mp3'}

class ChatRequest(BaseModel):
    userId: str
    message: str


def getChatService():
    return ChatService()


@router.post('/text', summary='Отправка текстового сообщения')
async def sendTextMessage(
    request: ChatRequest,
    chatService: ChatService = Depends(getChatService)
):
    try:
        response = chatService.processTextMessage(request.userId, request.message)
        return {'response': response}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post('/voice', summary='Отправка голосового сообщения')
async def sendVoiceMessage(
    userId: str = Form(...),
    file: UploadFile = File(...),
    chatService: ChatService = Depends(getChatService)
):
    if file.content_type not in ALLOWED_MIME_TYPES:
        raise HTTPException(
            status_code=400,
            detail=f'Invalid file type. Allowed types: {', '.join(ALLOWED_MIME_TYPES)}'
        )
    
    try:
        audioData  = await file.read()
        response = chatService.processVoiceMessage(userId, audioData)
        return {'audio': response}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
