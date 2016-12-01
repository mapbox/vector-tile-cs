using System.Collections.Generic;
using System.Linq;
using Mapbox.VectorTile.Geometry;
using System.Globalization;
using System;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Mapbox.VectorTile
{

    [DebuggerDisplay("{Zoom}/{TileColumn}/{TileRow}")]
    public class VectorTile
    {

        public VectorTile(byte[] data)
        {
            _Layers = new List<VectorTileLayer>();
            _VTR = new VectorTileReader(data);
        }


        private VectorTileReader _VTR;
        private List<VectorTileLayer> _Layers;


        public ReadOnlyCollection<string> LayerNames()
        {
            return _VTR.LayerNames();
        }

        public VectorTileLayer GetLayer(string layerName)
        {
            return _VTR.GetLayer(layerName);
        }

    }


}
