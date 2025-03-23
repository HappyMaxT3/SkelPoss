FROM python:3.12-slim


WORKDIR /app


COPY requirements.txt .


RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# COPY --from=builder /usr/local/lib/python3.12/site/-packages /usr/local/lib/python3.12/site-packages
# COPY --from=builder /usr/local/bin /usr/local/bin

COPY . .


ENV PYTHONPATH=/app

CMD ["python", "server/main.py"]
# CMD ["uvicorn", "server.main:app", "--host", "0.0.0.0", "--port", "8421"]