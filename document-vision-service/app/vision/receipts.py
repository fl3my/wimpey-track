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

    # Resize larger images for consistent processing
    max_dimension = 1500
    height, width = image.shape[:2]
    if max(height, width) > max_dimension:
        scale = max_dimension / max(height, width)
        new_width = int(width * scale)
        new_height = int(height * scale)
        image_resized = cv2.resize(image, (new_width, new_height))
    else:
        image_resized = image
        scale = 1.0

    # Convert to grayscale and blur
    gray = cv2.cvtColor(image_resized, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)

    # Detect the edges
    edges = cv2.Canny(blur, 50, 150)

    # Detect contours
    contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    # Create a list of bounding boxes
    boxes: list[BoundingBox] = []

    # Get the image area
    image_area = image.shape[0] * image.shape[1]

    for contour in contours:
        area = cv2.contourArea(contour)

        # Ignore very small areas
        if area < image_area * 0.01:
            continue

        # Get the perimeter of the contour
        peri = cv2.arcLength(contour, True)

        # Simplifies the contour
        approx = cv2.approxPolyDP(contour, 0.03 * peri, True)

        # Look for rectangular shapes
        if len(approx) < 4 or len(approx) > 6:
            continue

        x, y, w, h = cv2.boundingRect(approx)

        # Receipts are often tall rectangles
        aspect_ratio = h / float(w)

        if aspect_ratio < 1.2:
            continue

        x_orig = int(x / scale)
        y_orig = int(y / scale)
        w_orig = int(w / scale)
        h_orig = int(h / scale)

        boxes.append(BoundingBox(x=x_orig, y=y_orig, width=w_orig, height=h_orig))

    # Sort the boxes from left to right
    boxes.sort(key=lambda b: (b.y, b.x))

    return boxes
