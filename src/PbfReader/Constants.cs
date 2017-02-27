﻿using System;
using System.Collections.Generic;


namespace Mapbox.VectorTile {


	// WIRE types
	public enum WireTypes {
		VARINT = 0,// varint: int32, int64, uint32, uint64, sint32, sint64, bool, enum
		FIXED64 = 1, // 64-bit: double, fixed64, sfixed64
		BYTES = 2, // length-delimited: string, bytes, embedded messages, packed repeated fields
		FIXED32 = 5, // 32-bit: float, fixed32, sfixed32
		UNDEFINED = 99
	}


	public enum Commands {
		MoveTo = 1,
		LineTo = 2,
		ClosePath = 7
	}


	public enum TileType {
		Layers = 3
	}


	public enum LayerType {
		Version = 15,
		Name = 1,
		Features = 2,
		Keys = 3,
		Values = 4,
		Extent = 5
	}

	public static class duMMY {
		public static readonly Dictionary<int, string> TileType = new Dictionary<int, string>()
		{
			{3, "Layers" }
		};

		public static readonly Dictionary<int, string> LayerType = new Dictionary<int, string>()
		{
			{15, "Version" },
			{1, "Name" },
			{2,"Features" },
			{3, "Keys" },
			{4, "Values" },
			{5, "Extent" }
		};

		public static readonly Dictionary<int, string> FeatureType = new Dictionary<int, string>()
		{
			{ 1, "Id"},
			{ 2, "Tags"},
			{ 3, "Type"},
			{ 4, "Geometry"},
			{ 5, "Raster"}
		};

		public static readonly Dictionary<int, string> GeomType = new Dictionary<int, string>()
		{
			{ 0, "Unknown"},
			{ 1, "Point"},
			{ 2, "LineString"},
			{ 3, "Polygon"}
		};
	}

	public enum FeatureType {
		Id = 1,
		Tags = 2,
		Type = 3,
		Geometry = 4,
		Raster = 5
	}


	public enum ValueType {
		String = 1,
		Float = 2,
		Double = 3,
		Int = 4,
		UInt = 5,
		SInt = 6,
		Bool = 7
	}

}