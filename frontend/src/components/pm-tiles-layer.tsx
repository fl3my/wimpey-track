import { useEffect } from "react";
import { useMap } from "react-leaflet";
import { leafletLayer } from "protomaps-leaflet";

type PMTilesLayerProps = {
  url: string;
};

export function PMTilesLayer({ url }: PMTilesLayerProps) {
  const map = useMap();

  useEffect(() => {
    const layer = leafletLayer({
      url: url,
      flavor: "light",
      lang: "en",
    });

    layer.addTo(map);

    return () => {
      layer.remove();
    };
  }, [map, url]);

  return null;
}
