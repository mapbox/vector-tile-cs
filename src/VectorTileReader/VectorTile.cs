﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Mapbox.VectorTile {

	[DebuggerDisplay("{Zoom}/{TileColumn}/{TileRow}")]
	public class VectorTile {

		public VectorTile(byte[] data, bool validate = true) {
			_Layers = new List<VectorTileLayer>();
			_VTR = new VectorTileReader(data, validate);
		}


		private VectorTileReader _VTR;
		private List<VectorTileLayer> _Layers;


		public ReadOnlyCollection<string> LayerNames() {
			return _VTR.LayerNames();
		}

		public VectorTileLayer GetLayer(string layerName) {
			return _VTR.GetLayer(layerName);
		}



	}
}
