﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Subterranea {
    public class TileManager {
        public const int MAPX = 1000; // Fixed size of map
        public const int MAPY = 1000; // Fixed size of map
        public Tile nulltile;
        public Tile[,] tiles; // Tile data: 0 - empty  1 - filled  2 - out of bounds
        Random rand = new Random(); // RNG
        public int[][] sideOffsets = new int[][] {
            new int[] {1, 0},
            new int[] {0, 1},
            new int[] {-1, 0},
            new int[] {0, -1}
        }; // Tile offsets for side-neighboring tiles
        private HashSet<LivingObject> objects;
        public void Add(LivingObject obj) {
            objects.Add(obj);
            obj.collisionFlag = true;
        }
        public void Remove(LivingObject obj) {
            objects.Remove(obj);
            foreach (Tile tile in obj.tiles) {
                tile.Remove(obj);
            }
        }
        public TileManager() {
            nulltile = new Tile();
            objects = new HashSet<LivingObject>();
        }
        public void Update(GameTime delta) {
            foreach (LivingObject obj in objects) {
                obj.Update(delta);
                if (obj.collisionFlag) {
                    obj.collisionFlag = false;
                    Rectangle bounds = obj.shape.getBounds();
                    foreach (Tile tile in obj.tiles) {
                        tile.Remove(obj);
                    }
                    for (int i = ((int) Math.Floor((decimal) bounds.Left)); i<=((int)Math.Ceiling((decimal)bounds.Right)); i++) {
                        for (int j = ((int)Math.Floor((decimal)bounds.Top)); j <= ((int)Math.Ceiling((decimal)bounds.Bottom)); j++) {
                            Tile tile = GetAt(i, j);
                            if (tile.Filled) {
                                Physics.checkCollision(obj, tile);
                            }
                            foreach (LivingObject coll in tile.objects) {
                                if (!coll.collisionFlag) {
                                    Physics.checkCollision(obj, coll);
                                }
                            }

                        }
                    }
                }
            }
        }
        public static int Sign(float n) { // Taking code from Nested Dungeon's Player.cs
            if (n < 0) {
                return -1;
            }
            if (n == 0) {
                return 0;
            }
            return 1;
        }
        public bool IsValid(int x, int y) { // Is tile within map bounds
            if (x < 0 || y < 0 || x >= MAPX || y >= MAPY) {
                return false;
            }
            return true;
        }
        public bool IsOutside(int x, int y) { // Shortcut
            return !IsValid(x, y);
        }
        public bool SetAt(int x, int y, bool filled) { // Safe setter method. Returns false if unsuccessful, but will not crash
            if (IsOutside(x, y)) {
                return false;
            }
            tiles[x, y] = new Tile(this, filled, new Vector2(x, y));

            return true;
        }
        public Tile GetAt(int x, int y) { // Returns 2 if out of bounds
            if (IsOutside(x, y)) {
                return nulltile;
            }
            return tiles[x, y];
        }
        public void BatchSet(List<int[]> batch,bool value) {
            foreach (int[] tile in batch) {
                SetAt(tile[0],tile[1],value);
            }
        }
        public void Smooth(int minNeighbors = 2) {
            List<int[]> toRemove = new List<int[]>();
            for (int x = 0; x < MAPX; x++) {
                for (int y = 0; y < MAPY; y++) {
                    int neighbors = 0;
                    foreach (int[] offset in sideOffsets) {
                        if (GetAt(offset[0] + x, offset[1] + y).Filled) {
                            neighbors++;
                        }
                    }
                    if (neighbors < minNeighbors) {
                        toRemove.Add(new int[] { x, y });
                    }
                }
            }
            BatchSet(toRemove, false);
          
        }
        public void Generate() {
            tiles = new Tile[MAPX, MAPY];


            // First pass - Fill map
            for (int x = 0; x < MAPX; x++) {
                for (int y = 0; y < MAPY; y++) {
                    SetAt(x, y, true);
                }
            }
            // Second pass - Generate caves
            for (int x = 0; x < MAPX; x++) {
                for (int y = 0; y < MAPY; y++) {
                    if (rand.Next(1, Global.CAVEINDEX) == 1) { // 1 in 50 tiles are seeded for caves
                        Expand(x, y, rand.Next(Global.MINCAVESIZE, Global.MAXCAVESIZE));
                    }

                }
            }

            // Third pass - Remove floating and hanging blocks
            for (int i = 0; i < 5;i++) {
                Smooth(2);

            }

        }
        public void Expand(int x, int y, int life) { // Recursive function for generating caves
            Tile thisTile = GetAt(x, y);
            if (!thisTile.Filled || life == 0) {
                return;
            }
            SetAt(x, y, false);
            foreach (int[] offset in sideOffsets) {
                if (rand.Next(1, 4) == 1) { // Adds rough edges to cave walls
                    continue;
                }
                Expand(x + offset[0], y + offset[1], life - 1);
            }
        }
        public bool IsTile(int x, int y) {
            return GetAt(x, y).Filled;
        }
        public void UpdateTile(int x, int y) { // Apply normal to given tile
            if (!(GetAt(x, y).Filled)) {
                return;
            }
            int? slope = null;

            if (IsTile(x + 1, y) && !IsTile(x - 1, y)) {
                if (IsTile(x, y + 1) && !IsTile(x, y - 1)) {
                    slope = 0;
                }
                if (!IsTile(x, y + 1) && IsTile(x, y - 1)) {
                    slope = 270;
                }
            }
            if (!IsTile(x + 1, y) && IsTile(x - 1, y)) {
                if (IsTile(x, y + 1) && !IsTile(x, y - 1)) {
                    slope = 90;
                }
                if (!IsTile(x, y + 1) && IsTile(x, y - 1)) {
                    slope = 180;

                }
            }
            if (slope != null) {
                Tile tile = GetAt(x, y);
                tile.slopeRotation = (int)slope;
                tile.sloped = true;
            }



        }
        public void UpdateSlopes(Rectangle bounds) { // Updates side values for a given area
            for (int x = bounds.X; x < bounds.Width + 1; x++) {
                for (int y = bounds.Y; y < bounds.Height + 1; y++) {
                    UpdateTile(x, y);
                }
            }
        }
        public void UpdateSlopes() {
            UpdateSlopes(new Rectangle(1, 1, MAPX - 1, MAPY - 1));
        }


    }
}