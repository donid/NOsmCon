# NOsmCon
A console tool for processing OpenStreetMap (OSM/PBF) data files

This .**N**et **OSM** **Con**sole tool is inspired by Osmium.

Currently it can only extract nodes / ways and relations that fall into a specified bounding-box.

NOsmCon can read and write OpenStreetMap XML-files (.OSM) or binary-files (.PBF).

The 'hard work' is done by [OsmSharp](https://github.com/OsmSharp/core).

Hint: JOSM semms to always crash when it open a .pbf create with this too, so create .xml output with the arument '-f xml'.

Use 'NOsmCon.exe help extract' to see an example command-line.
