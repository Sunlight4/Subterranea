﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Subterranea {
    public class Player : LivingObject {
        public float movement = 0;
        public float acceleration = 0.25f;
        public float maxSpeed = 8f;
        public Player(TileManager mng) : base(mng) {
        }
        public override void Update(GameTime delta){
            if (collisionAxis!=null) {
                Vector2 axis = Global.Rotate90((Vector2) collisionAxis);
                if (axis.X<0) {
                    axis = -axis;
                }
                axis = axis / axis.Length();
                
                if (axis.X != 0) {
                    movement += acceleration * manager.GetInput().X;
                    if (Math.Abs(movement)>maxSpeed) {
                        movement = Math.Sign(movement) * maxSpeed;
                    }
                  
                    velocity = Global.ProjectVec(velocity, (Vector2) collisionAxis)+axis*movement;
                    
                }
                else {
                    int a = 3;
                }

            }
            base.Update(delta);
        }
        public override void Collide(float bounce, float friction, Vector2 axis) {
            movement *= friction;
            base.Collide(bounce, friction, axis);
        }
    }
}