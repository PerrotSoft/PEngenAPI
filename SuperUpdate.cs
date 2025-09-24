using PEngenAPI;
using PEngenAPI.GnuEngen;
using System;
using System.Collections.Generic;
using static PEngenAPI.GnuEngen.GnuEngenSFML;

namespace PEngenAPI.SuperUpdate
{
    class SuperUpdate
    {
        public enum GnuType { Gnu, Engen }

        public struct GameOBJN
        {
            public string name;
            public GnuEngenSFML.D2Gun d2Gun;
            public GameEngine.GameObject gameObject;
            public GnuType gnuType;
        }

        private List<GameOBJN> gameObjects = new List<GameOBJN>();

        private GnuEngenSFML gEngine;
        private GameEngine engine;

        public SuperUpdate(GnuEngenSFML gEngine, GameEngine engine)
        {
            this.gEngine = gEngine;
            this.engine = engine;
        }

        public void AddObject(GameOBJN obj)
        {
            gameObjects.Add(obj);
            if (obj.gnuType == GnuType.Gnu)
            {
                gEngine.AddObject(obj.d2Gun);
            }
            else if (obj.gnuType == GnuType.Engen)
            {
                engine.AddObject(obj.gameObject);
            }
        }

        public void DeleteObject(string name)
        {
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                if (gameObjects[i].name == name)
                {
                    var obj = gameObjects[i];
                    if (obj.gnuType == GnuType.Gnu)
                        gEngine.DeleteObject(obj.d2Gun);
                    else if (obj.gnuType == GnuType.Engen)
                        engine.DeleteObject(obj.gameObject);

                    gameObjects.RemoveAt(i);
                }
            }
        }
        public GameOBJN? FindObject(string name)
        {
            foreach (var obj in gameObjects)
            {
                if (obj.name == name)
                    return obj;
            }
            return null; // если ничего не найдено
        }
        public void Clear()
        {
            foreach (var obj in gameObjects)
            {
                if (obj.gnuType == GnuType.Gnu)
                    gEngine.DeleteObject(obj.d2Gun);
                else if (obj.gnuType == GnuType.Engen)
                    engine.DeleteObject(obj.gameObject);
            }
            gameObjects.Clear();
        }
        public bool Exists(string name)
        {
            foreach (var obj in gameObjects)
            {
                if (obj.name == name)
                    return true;
            }
            return false;
        }

        public void UpdateObject(GameOBJN obj)
        {
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                if (gameObjects[i].name == obj.name)
                    gameObjects[i] = obj;
            }
        }

        public void Update()
        {
            foreach (var item in gameObjects)
            {
                if (item.gnuType == GnuType.Gnu)
                {
                    gEngine.UpdateObject(item.d2Gun, item.d2Gun);
                }
                else if (item.gnuType == GnuType.Engen)
                {
                    engine.UpdateObject(item.gameObject, item.gameObject);
                }
            }
        }
    }
}
