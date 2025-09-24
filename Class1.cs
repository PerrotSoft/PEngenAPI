using PEngenAPI.GnuEngen;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static PEngenAPI.GnuEngen.GnuEngenSFML;

namespace PEngenAPI
{
    public class GameEngine
    {
        private GnuEngenSFML graphicsEngine;
        private List<GameObject> gameObjects = new List<GameObject>();
        private Clock clock = new Clock();

        public GameEngine()
        {
            graphicsEngine = new GnuEngenSFML();
            graphicsEngine.InitWindow(800, 600, "Microwave Game Engine");
        }

        // --- Игровой объект ---
        public class GameObject
        {
            public float x, y;
            public float vx, vy;
            public float width, height;
            public bool isStatic = false;
            public float mass = 1.0f;
            public Action<GameObject> OnUpdate;
            public GnuEngenSFML.D2Gun visual;
        }

        public void AddObject(GameObject obj)
        {
            gameObjects.Add(obj);
            if (obj.visual != null)
                graphicsEngine.AddObject(obj.visual);
        }

        public void DeleteObject(GameObject obj)
        {
            if (gameObjects.Contains(obj))
            {
                gameObjects.Remove(obj);
                if (obj.visual != null)
                    graphicsEngine.DeleteObject(obj.visual);
            }
        }

        public void UpdateObject(GameObject oldObj, GameObject newObj)
        {
            int index = gameObjects.IndexOf(oldObj);
            if (index != -1)
            {
                gameObjects[index] = newObj;
                if (oldObj.visual != null)
                    graphicsEngine.UpdateObject(oldObj.visual, newObj.visual);
            }
        }

        public void Run()
        {
            while (true)
            {
                float deltaTime = clock.Restart().AsSeconds();

                UpdatePhysics(deltaTime);
                HandleCollisions();
                UpdateLogic(deltaTime);

                graphicsEngine.Draw();
            }
        }

        private void UpdatePhysics(float dt)
        {
            foreach (var obj in gameObjects)
            {
                if (!obj.isStatic)
                {
                    obj.x += obj.vx * dt;
                    obj.y += obj.vy * dt;

                    if (obj.visual != null)
                    {
                        obj.visual.x = (uint)obj.x;
                        obj.visual.y = (uint)obj.y;
                    }
                }
            }
        }

        private void UpdateLogic(float dt)
        {
            foreach (var obj in gameObjects)
            {
                obj.OnUpdate?.Invoke(obj);
            }
        }

        private void HandleCollisions()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                var a = gameObjects[i];
                if (a.isStatic) continue;

                for (int j = i + 1; j < gameObjects.Count; j++)
                {
                    var b = gameObjects[j];
                    if (b.isStatic) continue;

                    if (CheckAABBCollision(a, b))
                        ResolveCollision(a, b);
                }
            }
        }

        private bool CheckAABBCollision(GameObject a, GameObject b)
        {
            return a.x < b.x + b.width &&
                   a.x + a.width > b.x &&
                   a.y < b.y + b.height &&
                   a.y + a.height > b.y;
        }

        private void ResolveCollision(GameObject a, GameObject b)
        {
            float vxA = a.vx;
            float vyA = a.vy;
            float vxB = b.vx;
            float vyB = b.vy;

            // Передача импульса
            a.vx = (vxA * (a.mass - b.mass) + 2 * b.mass * vxB) / (a.mass + b.mass);
            b.vx = (vxB * (b.mass - a.mass) + 2 * a.mass * vxA) / (a.mass + b.mass);
            a.vy = (vyA * (a.mass - b.mass) + 2 * b.mass * vyB) / (a.mass + b.mass);
            b.vy = (vyB * (b.mass - a.mass) + 2 * a.mass * vyA) / (a.mass + b.mass);

            // Смещение объектов, чтобы они не "слипались"
            float overlapX = Math.Min(a.x + a.width - b.x, b.x + b.width - a.x);
            float overlapY = Math.Min(a.y + a.height - b.y, b.y + b.height - a.y);

            if (overlapX < overlapY)
            {
                if (a.x < b.x) { a.x -= overlapX / 2; b.x += overlapX / 2; }
                else { a.x += overlapX / 2; b.x -= overlapX / 2; }
            }
            else
            {
                if (a.y < b.y) { a.y -= overlapY / 2; b.y += overlapY / 2; }
                else { a.y += overlapY / 2; b.y -= overlapY / 2; }
            }

            if (a.visual != null) { a.visual.x = (uint)a.x; a.visual.y = (uint)a.y; }
            if (b.visual != null) { b.visual.x = (uint)b.x; b.visual.y = (uint)b.y; }
        }
    }
}
