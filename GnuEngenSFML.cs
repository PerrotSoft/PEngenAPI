using SFML.Graphics;
using SFML.Window;
using SFML.System;
using System;
using System.Collections.Generic;

namespace PEngenAPI.GnuEngen
{
    public class GnuEngenSFML
    {
        public RenderWindow window;

        // --- Анимация ---
        public class Animation
        {
            public List<Texture> Frames = new List<Texture>();
            public float FrameSpeed = 0.1f;
            public int CurrentFrame = 0;
            private float timer = 0;

            public void Update(float deltaTime)
            {
                if (Frames.Count == 0) return;
                timer += deltaTime;
                if (timer >= FrameSpeed)
                {
                    timer = 0;
                    CurrentFrame++;
                    if (CurrentFrame >= Frames.Count)
                        CurrentFrame = 0;
                }
            }

            public Texture GetCurrentFrame()
            {
                if (Frames.Count == 0) return null;
                return Frames[CurrentFrame];
            }
        }

        public enum ObjectType { Cube, Line, Sprite, Text, Mesh, Button, TextButton, Input }

        public class D2Gun
        {
            public uint x, y, sx, sy;
            public ObjectType type;
            public uint base_color;
            public Texture texture;
            public Animation animation;
            public string text;
            public Font font;
            public Action onClick;
            public string inputString = "";
            public bool isActive = false;
        }

        public List<D2Gun> objects = new List<D2Gun>();
        private Clock clock = new Clock();

        public void InitWindow(uint width = 800, uint height = 600, string title = "PEngen SFML Window")
        {
            if (window == null)
            {
                window = new RenderWindow(new VideoMode(width, height), title);
                window.Closed += (sender, e) => window.Close();
                window.TextEntered += Window_TextEntered;
            }
            var logo = new D2Gun
            {
                x = 200,
                y = 80,
                sx = 400,
                sy = 450,
                texture = new Texture("res\\ico-engen.png"),
                type = ObjectType.Sprite
            };
            this.AddObject(logo);
            WaitSeconds(2f);
            this.DeleteObject(logo);
        }
        public void WaitSeconds(float seconds)
        {
            Clock clock = new Clock();
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                float dt = clock.Restart().AsSeconds();
                elapsed += dt;
                this.Draw();
            }
        }
        private void Window_TextEntered(object sender, TextEventArgs e)
        {
            foreach (var obj in objects)
            {
                if (obj.type == ObjectType.Input && obj.isActive)
                {
                    if (e.Unicode[0] == 8)
                    {
                        if (!string.IsNullOrEmpty(obj.inputString))
                            obj.inputString = obj.inputString.Substring(0, obj.inputString.Length - 1);
                    }
                    else
                    {
                        obj.inputString += e.Unicode;
                    }
                }
            }
        }

        public void AddObject(D2Gun obj)
        {
            objects.Add(obj);
        }

        // --- Удаление объекта ---
        public void DeleteObject(D2Gun obj)
        {
            if (objects.Contains(obj))
                objects.Remove(obj);
        }

        // --- Обновление объекта ---
        public void UpdateObject(D2Gun oldObj, D2Gun newObj)
        {
            int index = objects.IndexOf(oldObj);
            if (index != -1)
                objects[index] = newObj;
        }

        public void Draw()
        {

            float deltaTime = clock.Restart().AsSeconds();
            window.DispatchEvents();
            window.Clear(Color.Black);

            Vector2i mousePos = Mouse.GetPosition(window);

            foreach (var obj in objects)
            {
                Color color = new Color(
                    (byte)((obj.base_color >> 16) & 0xFF),
                    (byte)((obj.base_color >> 8) & 0xFF),
                    (byte)(obj.base_color & 0xFF)
                );

                switch (obj.type)
                {
                    case ObjectType.Cube:
                        RectangleShape rect = new RectangleShape(new Vector2f(obj.sx, obj.sy))
                        {
                            Position = new Vector2f(obj.x, obj.y),
                            FillColor = color
                        };
                        window.Draw(rect);
                        break;

                    case ObjectType.Line:
                        Vertex[] line = new Vertex[2]
                        {
                            new Vertex(new Vector2f(obj.x, obj.y), color),
                            new Vertex(new Vector2f(obj.x + obj.sx, obj.y + obj.sy), color)
                        };
                        window.Draw(line, PrimitiveType.Lines);
                        break;

                    case ObjectType.Sprite:
                        DrawSprite(obj, deltaTime);
                        break;

                    case ObjectType.Text:
                        DrawText(obj, color);
                        break;

                    case ObjectType.Button:
                        DrawButton(obj, mousePos, deltaTime);
                        break;

                    case ObjectType.TextButton:
                        DrawTextButton(obj, mousePos);
                        break;

                    case ObjectType.Input:
                        DrawInput(obj, mousePos);
                        break;

                    case ObjectType.Mesh:
                        Vertex[] mesh = new Vertex[]
                        {
                            new Vertex(new Vector2f(obj.x, obj.y), color),
                            new Vertex(new Vector2f(obj.x + obj.sx, obj.y), color),
                            new Vertex(new Vector2f(obj.x + obj.sx, obj.y + obj.sy), color),
                            new Vertex(new Vector2f(obj.x, obj.y + obj.sy), color)
                        };
                        window.Draw(mesh, PrimitiveType.Quads);
                        break;
                }
            }

            window.Display();
        }

        private void DrawSprite(D2Gun obj, float deltaTime)
        {
            Texture tex = obj.animation != null ? obj.animation.GetCurrentFrame() : obj.texture;
            if (tex != null)
            {
                if (obj.animation != null) obj.animation.Update(deltaTime);
                Sprite sprite = new Sprite(tex)
                {
                    Position = new Vector2f(obj.x, obj.y),
                    Scale = new Vector2f((float)obj.sx / tex.Size.X, (float)obj.sy / tex.Size.Y)
                };
                window.Draw(sprite);
            }
        }

        private void DrawText(D2Gun obj, Color color)
        {
            if (obj.font != null && !string.IsNullOrEmpty(obj.text))
            {
                Text text = new Text(obj.text, obj.font, 20)
                {
                    Position = new Vector2f(obj.x, obj.y),
                    FillColor = color
                };
                window.Draw(text);
            }
        }

        private void DrawButton(D2Gun obj, Vector2i mousePos, float deltaTime)
        {
            Texture tex = obj.animation != null ? obj.animation.GetCurrentFrame() : obj.texture;
            if (tex != null)
            {
                if (obj.animation != null) obj.animation.Update(deltaTime);
                Sprite sprite = new Sprite(tex)
                {
                    Position = new Vector2f(obj.x, obj.y),
                    Scale = new Vector2f((float)obj.sx / tex.Size.X, (float)obj.sy / tex.Size.Y)
                };
                FloatRect bounds = sprite.GetGlobalBounds();
                if (bounds.Contains(mousePos.X, mousePos.Y))
                {
                    sprite.Color = new Color(200, 200, 255);
                    if (Mouse.IsButtonPressed(Mouse.Button.Left) && obj.onClick != null)
                        obj.onClick.Invoke();
                }
                window.Draw(sprite);
            }
        }

        private void DrawTextButton(D2Gun obj, Vector2i mousePos)
        {
            RectangleShape rect = new RectangleShape(new Vector2f(obj.sx, obj.sy))
            {
                Position = new Vector2f(obj.x, obj.y),
                FillColor = new Color(100, 100, 100)
            };
            FloatRect bounds = rect.GetGlobalBounds();
            if (bounds.Contains(mousePos.X, mousePos.Y))
            {
                rect.FillColor = new Color(150, 150, 255);
                if (Mouse.IsButtonPressed(Mouse.Button.Left) && obj.onClick != null)
                    obj.onClick.Invoke();
            }
            window.Draw(rect);

            if (obj.font != null && !string.IsNullOrEmpty(obj.text))
            {
                Text text = new Text(obj.text, obj.font, 20)
                {
                    Position = new Vector2f(obj.x + 10, obj.y + 10),
                    FillColor = Color.White
                };
                window.Draw(text);
            }
        }

        private void DrawInput(D2Gun obj, Vector2i mousePos)
        {
            RectangleShape rect = new RectangleShape(new Vector2f(obj.sx, obj.sy))
            {
                Position = new Vector2f(obj.x, obj.y),
                FillColor = obj.isActive ? new Color(255, 255, 200) : new Color(50, 50, 50)
            };
            window.Draw(rect);

            FloatRect bounds = rect.GetGlobalBounds();
            if (bounds.Contains(mousePos.X, mousePos.Y))
            {
                if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    obj.isActive = true;
            }
            else if (Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                obj.isActive = false;
            }

            if (obj.font != null)
            {
                Text text = new Text(obj.inputString ?? "", obj.font, 20)
                {
                    Position = new Vector2f(obj.x + 5, obj.y + 5),
                    FillColor = Color.White
                };
                window.Draw(text);
            }
        }
    }
}
