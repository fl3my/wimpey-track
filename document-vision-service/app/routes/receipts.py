from fastapi import APIRouter, UploadFile, File
from app.vision.receipts import detect_receipts

router = APIRouter(prefix="/v1/receipts", tags=["receipts"])


@router.post("/detect")
async def detect_receipts_endpoint(image: UploadFile = File(...)):
    image_bytes = await image.read()
    receipts = detect_receipts(image_bytes)
    return {"receipts": receipts}
