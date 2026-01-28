from fastapi import FastAPI
from app.routes.receipts import router as receipts_router

app = FastAPI(title="Document Vision Service", version="1.0.0")


@app.get("/health")
async def health():
    return {
        "status": "ok",
        "service": "document-vision-service",
        "version": "1.0.0",
    }


app.include_router(receipts_router)
