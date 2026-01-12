using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Examples.GameObjects;

public class Fireworks : BaseGameObject
{
    // --- Tweakables (safe defaults) ---
    public float RocketsPerSecond { get; set; } = 0.6f;
    public float Gravity { get; set; } = 260f;

    public float RocketMinSpeed { get; set; } = 520f;
    public float RocketMaxSpeed { get; set; } = 820f;

    public int SparksMin { get; set; } = 28;
    public int SparksMax { get; set; } = 60;

    public float SparkMinSpeed { get; set; } = 110f;
    public float SparkMaxSpeed { get; set; } = 420f;

    public float SparkMinLife { get; set; } = 0.75f;
    public float SparkMaxLife { get; set; } = 1.6f;

    public float RocketTrailSpawnHz { get; set; } = 90f;
    public float SparkTrailSpawnHz { get; set; } = 60f;

    public int MaxRockets { get; set; } = 6;
    public int MaxSparks { get; set; } = 800;

    public bool DrawGlow { get; set; } = true;

    // --- Internal ---
    private IRenderTargetStrategy _renderTarget = null!;
    private int _width;
    private int _height;

    private float _rocketSpawnTimer;
    private float _rocketTrailTimer;
    private float _sparkTrailTimer;

    private readonly List<Rocket> _rockets = new();
    private readonly List<Spark> _sparks = new();
    private readonly List<TrailDot> _trail = new();

    public Fireworks()
    {
        Name = "Fireworks";
        Layer = 5;
        Camera = CameraLayer.UI;
        Visible = true;
        Enabled = true;
    }

    protected override void OnInitialize()
    {
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _width = _renderTarget.RenderWidth;
        _height = _renderTarget.RenderHeight;

        // Optional: center reference, but we mostly use screen bounds
        Position = new Vector2(_width / 2f, _height / 2f);
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Keep bounds fresh if your render target strategy can change at runtime
        _width = _renderTarget.RenderWidth;
        _height = _renderTarget.RenderHeight;

        UpdateRockets(deltaTime);
        UpdateSparks(deltaTime);
        UpdateTrail(deltaTime);

        SpawnRockets(deltaTime);
    }

    protected override void OnDraw()
    {
        // Draw trail dots first (cheap “afterglow” feeling)
        for (var i = 0; i < _trail.Count; i++)
        {
            var t = _trail[i];
            var alpha = Clamp01(t.Life / t.MaxLife);

            var color = t.Color;
            color.A = (byte)(alpha * 255);

            if (DrawGlow)
            {
                var glow = color;
                glow.A = (byte)(alpha * 60);
                Raylib_cs.Raylib.DrawCircleV(t.Position, t.Radius * 2.2f, glow);
            }
            
            Raylib_cs.Raylib.DrawCircleV(t.Position, t.Radius, color);
        }

        // Rocket bodies + streak
        for (var i = 0; i < _rockets.Count; i++)
        {
            var r = _rockets[i];

            if (DrawGlow)
            {
                var glow = r.Color;
                glow.A = 70;
                Raylib_cs.Raylib.DrawCircleV(r.Position, 8, glow);
            }

            Raylib_cs.Raylib.DrawCircleV(r.Position, 3.0f, r.Color);

            // Streak line opposite velocity
            var back = r.Position - Vector2.Normalize(r.Velocity) * 16f;
            Raylib_cs.Raylib.DrawLineV(r.Position, back, r.Color);
        }

        // Sparks (small circles + tiny streaks)
        for (var i = 0; i < _sparks.Count; i++)
        {
            var s = _sparks[i];
            var alpha = Clamp01(s.Life / s.MaxLife);

            var color = s.Color;
            color.A = (byte)(alpha * 255);

            if (DrawGlow)
            {
                var glow = color;
                glow.A = (byte)(alpha * 60);
                Raylib_cs.Raylib.DrawCircleV(s.Position, s.Radius * 2.4f, glow);
            }

            Raylib_cs.Raylib.DrawCircleV(s.Position, s.Radius, color);

            var streak = s.Position - Vector2.Normalize(s.Velocity) * 10f;
            Raylib_cs.Raylib.DrawLineV(s.Position, streak, color);
        }
    }

    protected override void OnDispose()
    {
        _rockets.Clear();
        _sparks.Clear();
        _trail.Clear();
    }

    private void SpawnRockets(float deltaTime)
    {
        if (_rockets.Count >= MaxRockets)
            return;

        _rocketSpawnTimer += deltaTime;

        var interval = 1f / MathF.Max(0.0001f, RocketsPerSecond);
        while (_rocketSpawnTimer >= interval)
        {
            _rocketSpawnTimer -= interval;

            var x = RandomRange(20, _width - 20);
            var start = new Vector2(x, _height + 20);

            var speed = RandomRange(RocketMinSpeed, RocketMaxSpeed);
            var angle = RandomRange(-0.25f, 0.25f); // small left/right variation
            var velocity = new Vector2(MathF.Sin(angle) * 120f, -speed);

            var color = PickFireworkColor();

            // explode somewhere in upper part of the screen
            var explodeY = RandomRange(_height * 0.15f, _height * 0.45f);

            _rockets.Add(new Rocket
            {
                Position = start,
                Velocity = velocity,
                Color = color,
                ExplodeY = explodeY
            });
        }
    }

    private void UpdateRockets(float deltaTime)
    {
        _rocketTrailTimer += deltaTime;
        var trailInterval = 1f / MathF.Max(1f, RocketTrailSpawnHz);

        for (var i = _rockets.Count - 1; i >= 0; i--)
        {
            var r = _rockets[i];

            r.Velocity.Y += Gravity * deltaTime;
            r.Position += r.Velocity * deltaTime;

            if (_rocketTrailTimer >= trailInterval)
            {
                var dot = new TrailDot
                {
                    Position = r.Position,
                    Radius = RandomRange(1.2f, 2.2f),
                    Life = 0.35f,
                    MaxLife = 0.35f,
                    Color = FadeToSmoke(r.Color)
                };

                _trail.Add(dot);
            }

            // explode condition
            if (r.Position.Y <= r.ExplodeY || r.Velocity.Y > 0f)
            {
                Explode(r.Position, r.Color);
                _rockets.RemoveAt(i);
                continue;
            }

            // safety cleanup if it goes too far offscreen
            if (r.Position.Y > _height + 200)
                _rockets.RemoveAt(i);
            else
                _rockets[i] = r;
        }

        if (_rocketTrailTimer >= trailInterval)
            _rocketTrailTimer = 0f;
    }

    private void UpdateSparks(float deltaTime)
    {
        _sparkTrailTimer += deltaTime;
        var trailInterval = 1f / MathF.Max(1f, SparkTrailSpawnHz);

        for (var i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];

            s.Velocity.Y += Gravity * 0.35f * deltaTime; // sparks fall slower than rockets
            s.Velocity *= 0.985f; // drag
            s.Position += s.Velocity * deltaTime;

            s.Life -= deltaTime;

            if (_sparkTrailTimer >= trailInterval)
            {
                var alpha = Clamp01(s.Life / s.MaxLife);
                if (alpha > 0.05f)
                {
                    var trailColor = s.Color;
                    trailColor.A = (byte)(alpha * 90);

                    _trail.Add(new TrailDot
                    {
                        Position = s.Position,
                        Radius = MathF.Max(0.9f, s.Radius - 0.6f),
                        Life = 0.22f,
                        MaxLife = 0.22f,
                        Color = trailColor
                    });
                }
            }

            // kill spark
            if (s.Life <= 0f)
            {
                _sparks.RemoveAt(i);
                continue;
            }

            // soft bounds: if it disappears far away, remove it
            if (s.Position.X < -200 || s.Position.X > _width + 200 || s.Position.Y > _height + 250)
            {
                _sparks.RemoveAt(i);
                continue;
            }

            _sparks[i] = s;
        }

        if (_sparkTrailTimer >= trailInterval)
            _sparkTrailTimer = 0f;

        // hard cap
        if (_sparks.Count > MaxSparks)
            _sparks.RemoveRange(0, _sparks.Count - MaxSparks);
    }

    private void UpdateTrail(float deltaTime)
    {
        for (var i = _trail.Count - 1; i >= 0; i--)
        {
            var t = _trail[i];
            t.Life -= deltaTime;

            if (t.Life <= 0f)
            {
                _trail.RemoveAt(i);
                continue;
            }

            _trail[i] = t;
        }

        // keep trail list bounded too (just in case)
        if (_trail.Count > 2000)
            _trail.RemoveRange(0, _trail.Count - 2000);
    }

    private void Explode(Vector2 position, Color baseColor)
    {
        var sparks = Raylib_cs.Raylib.GetRandomValue(SparksMin, SparksMax);

        for (var i = 0; i < sparks; i++)
        {
            if (_sparks.Count >= MaxSparks)
                break;

            var angle = RandomRange(0f, MathF.Tau);
            var speed = RandomRange(SparkMinSpeed, SparkMaxSpeed);
            var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;

            var life = RandomRange(SparkMinLife, SparkMaxLife);

            var color = JitterColor(baseColor);
            var radius = RandomRange(1.2f, 2.8f);

            _sparks.Add(new Spark
            {
                Position = position,
                Velocity = vel,
                Life = life,
                MaxLife = life,
                Color = color,
                Radius = radius
            });
        }

        // little flash bloom using trail dots
        if (DrawGlow)
        {
            for (var j = 0; j < 10; j++)
            {
                _trail.Add(new TrailDot
                {
                    Position = position + new Vector2(RandomRange(-6, 6), RandomRange(-6, 6)),
                    Radius = RandomRange(3f, 7f),
                    Life = 0.18f,
                    MaxLife = 0.18f,
                    Color = baseColor
                });
            }
        }
    }

    private static float RandomRange(float min, float max)
    {
        // Raylib GetRandomValue is int-based; this keeps it simple and deterministic enough
        var v = Raylib_cs.Raylib.GetRandomValue(0, 10000) / 10000f;
        return min + (max - min) * v;
    }

    private static int RandomRange(int min, int max)
    {
        return Raylib_cs.Raylib.GetRandomValue(min, max);
    }

    private static float Clamp01(float value)
    {
        if (value < 0f)
            return 0f;

        if (value > 1f)
            return 1f;

        return value;
    }

    private static Color PickFireworkColor()
    {
        // Hand-picked “firework-ish” palette (no textures needed)
        var pick = Raylib_cs.Raylib.GetRandomValue(0, 7);

        return pick switch
        {
            0 => new Color(255, 80, 80, 255),
            1 => new Color(255, 190, 60, 255),
            2 => new Color(255, 255, 120, 255),
            3 => new Color(80, 255, 140, 255),
            4 => new Color(80, 170, 255, 255),
            5 => new Color(190, 110, 255, 255),
            6 => new Color(255, 120, 220, 255),
            _ => Color.White
        };
    }

    private static Color JitterColor(Color c)
    {
        var r = ClampByte(c.R + Raylib_cs.Raylib.GetRandomValue(-30, 30));
        var g = ClampByte(c.G + Raylib_cs.Raylib.GetRandomValue(-30, 30));
        var b = ClampByte(c.B + Raylib_cs.Raylib.GetRandomValue(-30, 30));

        return new Color(r, g, b, (byte)255);
    }

    private static byte ClampByte(int v)
    {
        if (v < 0)
            return 0;

        if (v > 255)
            return 255;

        return (byte)v;
    }

    private static Color FadeToSmoke(Color c)
    {
        // Trail wants to look a bit “ashy”
        var r = (byte)((c.R * 0.6f) + 30);
        var g = (byte)((c.G * 0.6f) + 30);
        var b = (byte)((c.B * 0.6f) + 30);
        return new Color(r, g, b, (byte)200);
    }

    private struct Rocket
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float ExplodeY;
    }

    private struct Spark
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public Color Color;
        public float Radius;
    }

    private struct TrailDot
    {
        public Vector2 Position;
        public float Radius;
        public float Life;
        public float MaxLife;
        public Color Color;
    }
}