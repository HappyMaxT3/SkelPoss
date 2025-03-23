import pytest
from fastapi.testclient import TestClient
from ..main import app


client = TestClient(app)


def testSendMessage():
    response = client.post(
        '/chat/text',
        json={"message": "Hello, how are you?"}
    )

    assert response.status_code == 200
    assert 'response' in response.json()


def testSendVoiceMessage():
    with open('testAudio.wav', 'rb') as f:
        response = client.post(
            '/chat/voice',
            files={'file': ('testAudio.wav', f, 'audio/wav')}
        )

        assert response == 200
        assert response.content


def testMissingData():
    response = client.post('/chat/text')

    assert response.status_code == 422
    assert 'detail' in response.json()


def testInvalidFileUpload():
    response = client.post(
        '/chat/voice',
        files={'file': ('invalid.txt', b'Invalid file content', 'text/plain')}
    )

    assert response.status_code == 400
    assert 'detail' in response.json()
