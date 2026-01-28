import cv2
import numpy as np
from app.models.receipt import BoundingBox


def detect_receipts(image_bytes: bytes) -> list[BoundingBox]:
    # Load the image
    np_array = np.frombuffer(image_bytes, dtype=np.uint8)
    image = cv2.imdecode(np_array, cv2.IMREAD_COLOR)

    # Return empty list if there is no image
    if image is None:
        return []

    # Convert to grayscale and blur
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # Detect the edges
    edges = cv2.Canny(blur, 75, 200)

    # Detect contours
    contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    # Create a list of bounding boxes
    boxes: list[BoundingBox] = []

    # Get the image area
    image_area = image.shape[0] * image.shape[1]

    for contour in contours:
        area = cv2.contourArea(contour)

        # Ignore very small areas
        if area < image_area * 0.03:
            continue

        # Get the perimeter of the contour
        peri = cv2.arcLength(contour, True)

        # Simplifies the contour
        approx = cv2.approxPolyDP(contour, 0.02 * peri, True)

        # Look for rectangular shapes
        if len(approx) != 4:
            continue

        x, y, w, h = cv2.boundingRect(approx)

        # Receipts are often tall rectangles
        aspect_ratio = h / float(w)

        if aspect_ratio < 1.2:
            continue

        boxes.append(BoundingBox(x=x, y=y, width=w, height=h))

    # Sort the boxes from left to right
    boxes.sort(key=lambda b: (b.y, b.x))

    return boxes
