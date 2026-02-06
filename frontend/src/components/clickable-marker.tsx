import type { LatLngTuple } from "leaflet";
import { Marker, useMapEvents } from "react-leaflet";

export function ClickableMarker({
  position,
  onChange,
}: {
  position: LatLngTuple | null;
  onChange: (pos: LatLngTuple) => void;
}) {
  useMapEvents({
    click(e) {
      onChange([e.latlng.lat, e.latlng.lng]);
    },
  });

  return position ? <Marker position={position} /> : null;
}
