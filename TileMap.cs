﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBondOfStone {
    class TileMap { //This object class represents a chunk's tile data

        List<Tile> tiles = new List<Tile>(); //List holds tiles linearly (w/ property)

        public List<Tile> Tiles {
            get { return tiles; }
        }

        //Width and height of this chunk
        private int width, height;
        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public TileMap() { }

        //Generate this chunk in a drawable format. Takes a 2D array of tile IDs and a tile size.
        public void Generate(int[,] atlas, int size) {
            //Iterate through each value of the Atlas
            for (int x = 0; x < atlas.GetLength(1); x++) {
                for (int y = 0; y < atlas.GetLength(0); y++) {
                    //Add a new tile to the tiles list with an ID and rect from the Atlas.
                    Tiles.Add(new Tile(atlas[y, x], new Microsoft.Xna.Framework.Rectangle(x * size, y * size, size, size)));

                    width = (x + 1) * size;
                    height = (y + 1) * size;
                }
            }

            //Set adjacent tiles for each tile
            int i = 0; //increment through the array of tile IDs

            for (int x = 0; x < atlas.GetLength(1); x++) { //Execute for each tile
                for (int y = 0; y < atlas.GetLength(0); y++) {
                    int thisID = Tiles[i].ID; //Current tile ID
                    bool stitchOnlySameID = true; //For stitching background tiles into foreground tiles

                    if (thisID == 2) //If this is a background tile, need to stitch it into foreground, but not vice versa
                        stitchOnlySameID = false;

                    int bkdCount = 0; //For stitching background tiles into foreground tiles

                    //if this isn't an air tile (which we don't stitch)
                    if (thisID != 0) {
                        //Construct the Adjacents array for this tile
                        //(i.e. add the cardinal tile IDs to this tile's Adjacents array in the order North West East South
                        if (y - 1 >= 0 && (atlas[y - 1, x] == thisID || (!stitchOnlySameID && atlas[y - 1, x] != 0)))
                            Tiles[i].Adjacents[0] = true;
                        else if (y - 1 >= 0 && atlas[y - 1, x] == 2) //If this cardinal has a border tile...
                            bkdCount++; //increment the count of adjacent background tiles

                        if (x - 1 >= 0 && (atlas[y, x - 1] == thisID || (!stitchOnlySameID && atlas[y, x - 1] != 0)))
                            Tiles[i].Adjacents[1] = true;
                        else if (x - 1 >= 0 && (atlas[y, x - 1] == 2))
                            bkdCount++;

                        if (x + 1 < atlas.GetLength(1) && (atlas[y, x + 1] == thisID || (!stitchOnlySameID && atlas[y, x + 1] != 0)))
                            Tiles[i].Adjacents[2] = true;
                        else if (x + 1 < atlas.GetLength(1) && (atlas[y, x + 1] == 2))
                            bkdCount++;

                        if (y + 1 < atlas.GetLength(0) && (atlas[y + 1, x] == thisID || (!stitchOnlySameID && atlas[y + 1, x] != 0)))
                            Tiles[i].Adjacents[3] = true;
                        else if (y + 1 < atlas.GetLength(0) && (atlas[y + 1, x] == 2))
                            bkdCount++;
                    }

                    //If more than 1 background tile borders this foreground tile...
                    if (bkdCount > 1) 
                        Tiles.Add(Tiles[i].AddBackgroundTile()); //...add another background tile behind it to fill gaps  
                    
                    i++; //Advance the tile list
                }
            }
        }

        public void Draw(SpriteBatch sb) {
            //The tiles are sorted and drawn in ascending order of the DrawQueue property value
            List<Tile> sortedTiles = Tiles.OrderBy(o => o.DrawQueue).ToList();
            foreach (Tile tile in sortedTiles)
                tile.Draw(sb);
        }

        //TODO: create dynamic chunk image storing/loading system

        //Reads in an image from a given path and returns that image converted to tile IDs in a 2D array
        public int[,] ReadImage(string imagePath) {
            Bitmap img = new Bitmap(imagePath); //Convert it to a readable format

            int[,] atlas = new int[img.Height, img.Width]; //Initialize dimensions of returned array

            for(int x = 0; x < atlas.GetLength(1); x++) { //Iterate through each atlas index
                for(int y = 0; y < atlas.GetLength(0); y++) {
                    Color px = img.GetPixel(y, x); //Get the pixel color at this index and convert it to a string

                    string pxStr = 
                        px.R.ToString("D3") + " " +
                        px.G.ToString("D3") + " " +
                        px.B.ToString("D3") + " " +
                        px.A.ToString("D3");

                    atlas[x, y] = TileID(pxStr); //Convert the pixel color to an integer ID and populate the array
                }
            }

            return atlas; //Return the complete array
        }

        //Switch statement which converts pixel colors as strings into integer tile IDs
        int TileID(string color) {
            int tileID = 0;

            switch (color) {
                case "092 186 072 255":
                    tileID = 1; //Grass tile
                    break;

                case "105 119 135 255":
                    tileID = 2; //Background tile
                    break;

                case "255 174 012 255":
                    tileID = 3; //Gold tile
                    break;
            }
            return tileID;
        }
    }
}
