from fastapi import APIRouter, UploadFile, File
from app.vision.receipts import detect_receipts

router = APIRouter(prefix="/v1/receipts", tags=["receipts"])


@router.post("/detect")
async def detect_receipts_endpoint(file: UploadFile = File(...)):
    image_bytes = await file.read()
    receipts = detect_receipts(image_bytes)
    return {"receipts": receipts}
