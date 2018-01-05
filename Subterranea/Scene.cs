﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Subterranea {
    public class Scene {
        public TileManager tileManager;
        public InputManager inputManager;
        public PhysicsManager physicsManager;
        public List<Object> objects;
        public Scene() {
            objects = new List<Object>();
            tileManager = new TileManager();
            inputManager = new InputManager();
            physicsManager = new PhysicsManager();
        }
        public void Add(Object obj) {
            objects.Add(obj);
            obj.scene = this;
        }
        public void Add(PhysicsObject obj) {
            physicsManager.Add(obj);
            Add((Object)obj);
        }
        public void Update(GameTime delta) {
            tileManager.Update(delta);
            inputManager.Update(delta);
            physicsManager.Update(delta);
        }
    }
}
