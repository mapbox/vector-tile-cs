

all: test

test.exe: test.cs src/Geometry/*.cs src/PbfReader/*cs src/VectorTileReader/*cs src/Util/*cs Makefile
	 mcs test.cs src/Geometry/*.cs src/PbfReader/*cs src/VectorTileReader/*cs src/Util/*cs

test/mvt-fixtures:
	git submodule update --init

test: test/mvt-fixtures test.exe
	mono test.exe ./test/mvt-fixtures/fixtures/valid/Feature-single-point.mvt

.PHONY: test test.exe