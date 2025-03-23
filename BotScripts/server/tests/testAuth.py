import pytest
from fastapi.testclient import TestClient
from ..main import app


client = TestClient(app)


def testRegister():
    response = client.post(
        '/auth/register',
        json={
            'username': 'admin',
            'password': '12345678',
        }
    )

    assert response.status_code == 200
    assert 'response' in response.json()


def testLogin():
    response = client.post(
        '/auth/login',
        json={
            'username': 'admin',
            'password': '12345678',
        }
    )

    assert response.status_code == 200
    assert 'response' in response.json()


def testWrongUsername():
    response = client.post(
        '/auth/login',
        json={
            'username': 'notAdmin',
            'password': '12345678',
        }
    )

    assert response.status_code == 400
    assert 'detail' in response.json()


def testWrongPassword():
    response = client.post(
        '/auth/login',
        json={
            'username': 'admin',
            'password': '12345679',
        }
    )

    assert response.status_code == 400
    assert 'detail' in response.json()


def testExistingUsername():
    response = client.post(
        '/auth/register',
        json={
            'username': 'admin',
            'password': '12345679'
        }
    )

    assert response.status_code == 400
    assert 'detail' in response.json()
