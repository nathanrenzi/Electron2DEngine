using Electron2D.Core.ECS;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using System.Drawing;
using System.Numerics;
using DotnetNoise;

namespace Electron2D.Core
{
    public class ParticleSystemBaseSystem : BaseSystem<ParticleSystem> { }
    public class ParticleSystem : Component, IRenderable
    {
        private const int PREWARM_STEPS = 100;

        public bool IsLoop { get; set; } = true;
        public bool IsPlaying { get; set; }
        public bool Prewarm { get; private set; }
        public bool IsWorldSpace { get; set; }
        public bool InheritVelocity { get; set; }
        public int MaxParticles { get; }
        public float LoopTime { get; private set; }
        public float Duration { get; private set; } = 5f;
        public int BurstSpawnAmount { get; private set; } = 30;
        public List<Particle> Particles { get; private set; }
        public BlendMode BlendMode { get; private set; }
        public int RenderLayer { get; }
        public ParticleEmissionShape EmissionShape { get; set; } = ParticleEmissionShape.VolumeCircle;
        public ParticleEmissionMode EmissionMode { get; private set; } = ParticleEmissionMode.Constant;
        public float EmissionSize { get; set; } = 1;
        public Vector2 EmissionDirection { get; set; } = Vector2.UnitY;
        public float EmissionSpreadAngle { get; set; } = 10;
        public float EmissionParticlesPerSecond { get; set; } = 30;
        public bool EmitAlongEmissionShapeNormal { get; set; } = false;
        public bool InvertEmissionDirection { get; set; } = false;
        public Vector2 SizeRange { get; set; } = new Vector2(10);
        public Vector2 StartRotationRange { get; set; } = Vector2.Zero;
        public Vector2 AngularVelocityRange { get; set; } = Vector2.Zero;
        public Vector2 LifetimeRange { get; set; } = Vector2.One;
        public Vector2 SpeedRange { get; set; } = new Vector2(20, 20);
        public Gradient ColorRange { get; set; } = new Gradient(Color.White);
        public Gradient ColorOverLifetime { get; private set; }
        private bool colorOverLifetimeEnabled;
        public Curve SizeOverLifetime { get; private set; }
        private bool sizeOverLifetimeEnabled;
        public Curve SpeedOverLifetime { get; private set; }
        private bool speedOverLifetimeEnabled;

        public float NoiseStrength { get; private set; } = 0;
        public float NoiseSpeed { get; private set; } = 0;
        public float NoiseFrequency { get; private set; } = 1;
        private bool noiseEnabled;

        #region Private Fields
        private float[] vertices;
        private uint[] indices;
        private MeshRenderer renderer;
        private Transform transform;
        private Transform fakeTransform;
        private Random random;
        private int randomSeed;
        private bool playOnAwake;
        private Material material;
        private FastNoise noise;
        private int currentBurstAmount;
        private float spawnInterval { get { return 1f / EmissionParticlesPerSecond; } }
        private float spawnTime;

        private Vector2 lastPosition;
        private Vector2 calculatedVelocity;
        #endregion

        public ParticleSystem(bool _playOnAwake, bool _prewarm, bool _isWorldSpace, bool _inheritVelocity,
            int _maxParticles, Material _material, int _renderLayer = 1, int _randomSeed = -1)
        {
            playOnAwake = _playOnAwake;
            IsWorldSpace = _isWorldSpace;
            Prewarm = _prewarm;
            InheritVelocity = _inheritVelocity;
            MaxParticles = _maxParticles;
            RenderLayer = _renderLayer;
            material = _material;

            fakeTransform = new Transform();

            // Pre-allocating arrays
            // 4 vertices * (X + Y + U + V) * total particles
            vertices = new float[4 * 8 * _maxParticles];
            // 6 indices (2 triangles) * total particles
            indices = new uint[6 * _maxParticles];

            //Pre-allocating particle list
            Particles = new List<Particle>(MaxParticles);

            randomSeed = _randomSeed == -1 ? DateTime.Now.Millisecond : _randomSeed;
            random = new Random(randomSeed);
            noise = new FastNoise(randomSeed);

            ParticleSystemBaseSystem.Register(this);
            RenderLayerManager.OrderRenderable(this);
        }

        public ParticleSystem SetBlendMode(BlendMode blendMode)
        {
            BlendMode = blendMode;
            return this;
        }

        public ParticleSystem SetConstantEmissionMode(bool isLoop, float duration = -1, float emissionsPerSecond = -1)
        {
            if (emissionsPerSecond != -1)
            {
                EmissionParticlesPerSecond = emissionsPerSecond;
            }
            EmissionMode = ParticleEmissionMode.Constant;
            IsLoop = isLoop;
            if(!IsLoop && duration == -1)
            {
                Debug.LogError("Duration of -1 is not valid for a non-looping particle system.");
                Duration = 1;
            }
            else
            {
                Duration = duration;
            }
            return this;
        }

        public ParticleSystem SetBurstEmissionMode(bool isLoop, int burstSpawnAmount, float loopDelay = -1, float emissionsPerSecond = -1)
        {
            if(emissionsPerSecond != -1)
            {
                EmissionParticlesPerSecond = emissionsPerSecond;
            }
            EmissionMode = ParticleEmissionMode.Burst;
            IsLoop = isLoop;
            if (!IsLoop && loopDelay == -1)
            {
                Debug.LogError("Loop delay (duration) of -1 is not valid for a non-looping burst particle system.");
                Duration = 1;
            }
            else
            {
                Duration = loopDelay;
            }
            BurstSpawnAmount = burstSpawnAmount;
            return this;
        }

        public ParticleSystem SetInvertEmissionDirection(bool flag)
        {
            InvertEmissionDirection = flag;
            return this;
        }

        public ParticleSystem SetEmitAlongEmissionShapeNormal(bool flag)
        {
            EmitAlongEmissionShapeNormal = flag;
            return this;
        }

        public ParticleSystem SetEmissionsPerSecond(float emissionsPerSecond)
        {
            EmissionParticlesPerSecond = emissionsPerSecond;
            return this;
        }

        public ParticleSystem SetEmissionDirection(Vector2 direction)
        {
            EmissionDirection = direction;
            return this;
        }

        public ParticleSystem SetEmissionSpreadAngle(float angle)
        {
            EmissionSpreadAngle = angle;
            return this;
        }

        public ParticleSystem SetEmissionShape(ParticleEmissionShape shape, float size)
        {
            EmissionShape = shape;
            EmissionSize = size;
            return this;
        }

        public ParticleSystem SetSize(float min, float max)
        {
            SizeRange = new Vector2(min, max);
            return this;
        }

        public ParticleSystem SetSize(float size)
        {
            SizeRange = new Vector2(size, size);
            return this;
        }

        public ParticleSystem SetStartRotation(float min, float max)
        {
            StartRotationRange = new Vector2(min, max);
            return this;
        }

        public ParticleSystem SetStartRotation(float rotation)
        {
            StartRotationRange = new Vector2(rotation, rotation);
            return this;
        }

        public ParticleSystem SetAngularVelocity(float min, float max)
        {
            AngularVelocityRange = new Vector2(min, max);
            return this;
        }

        public ParticleSystem SetAngularVelocity(float angularVelocity)
        {
            AngularVelocityRange = new Vector2(angularVelocity, angularVelocity);
            return this;
        }

        public ParticleSystem SetLifetime(float min, float max)
        {
            LifetimeRange = new Vector2(min, max);
            return this;
        }

        public ParticleSystem SetLifetime(float lifetime)
        {
            LifetimeRange = new Vector2(lifetime, lifetime);
            return this;
        }

        public ParticleSystem SetSpeed(float min, float max)
        {
            SpeedRange = new Vector2(min, max);
            return this;
        }

        public ParticleSystem SetSpeed(float speed)
        {
            SpeedRange = new Vector2(speed, speed);
            return this;
        }

        public ParticleSystem SetColor(Color color)
        {
            ColorRange = new Gradient(color);
            return this;
        }

        public ParticleSystem SetColor(Gradient gradient)
        {
            ColorRange = gradient;
            return this;
        }

        public ParticleSystem SetColorOverLifetime(Gradient colorOverLifetime)
        {
            colorOverLifetimeEnabled = true;
            ColorOverLifetime = colorOverLifetime;
            return this;
        }

        public ParticleSystem SetSizeOverLifetime(Curve sizeCurve)
        {
            sizeOverLifetimeEnabled = true;
            SizeOverLifetime = sizeCurve;
            return this;
        }

        public ParticleSystem SetSpeedOverLifetime(Curve speedCurve)
        {
            speedOverLifetimeEnabled = true;
            SpeedOverLifetime = speedCurve;
            return this;
        }

        public ParticleSystem SetNoiseSettings(float noiseStrength, float noiseFrequency, float noiseSpeed)
        {
            noiseEnabled = true;
            NoiseStrength = noiseStrength;
            NoiseFrequency = noiseFrequency;
            NoiseSpeed = noiseSpeed;
            return this;
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
            if (transform == null)
            {
                Debug.LogError("PARTICLE SYSTEM: Cannot create particle system if entity does not have a Transform component!");
                return;
            }
            renderer = new MeshRenderer(transform, material);
            renderer.UseCustomIndexRenderCount = true;
            renderer.OnBeforeRender += SetModelMatrix;
            BufferLayout layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);
            renderer.SetBufferLayoutBeforeLoad(layout);
            renderer.SetVertexArrays(vertices, indices, false, renderer.IsLoaded);
            renderer.Load(false);
            if (Prewarm) PrewarmParticles(); // Currently does not work

            if (playOnAwake) Play();
        }

        private void PrewarmParticles()
        {
            int spawnCount = (int)(LifetimeRange.Y / EmissionParticlesPerSecond);
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnParticle();
            }

            float deltaTime = LifetimeRange.Y / PREWARM_STEPS;

            for (int x = 0; x < PREWARM_STEPS; x++)
            {
                // Updating all particles
                for (int i = 0; i < Particles.Count; i++)
                {
                    Particle particle = Particles[i];

                    // Used for particle over-lifetime effects (if enabled)
                    float t = 1 - (particle.Lifetime / particle.InitialLifetime);

                    particle.Position += particle.Velocity * deltaTime * (speedOverLifetimeEnabled ? SpeedOverLifetime.Evaluate(t) : 1);
                    particle.Rotation += particle.AngularVelocity * deltaTime;
                    particle.Lifetime -= deltaTime;
                    if (particle.Lifetime <= 0)
                    {
                        particle.IsDead = true;
                    }
                }
            }

            UpdateMesh();
        }

        #region Setters
        private void SetModelMatrix()
        {
            if (IsWorldSpace)
            {
                renderer.Material.Shader.SetMatrix4x4("model", fakeTransform.GetScaleMatrix() * fakeTransform.GetRotationMatrix() * transform.GetPositionMatrix());
            }
        }
        #endregion

        #region Getters
        public MeshRenderer GetRenderer() => renderer;
        public int GetRenderLayer() => RenderLayer;
        #endregion

        #region Playback
        public void Play()
        {
            renderer.Enabled = true;
            IsPlaying = true;
            currentBurstAmount = 0;
            LoopTime = 0;
        }

        public void SetPaused(bool _pause)
        {
            IsPlaying = !_pause;
        }

        public void Stop()
        {
            renderer.Enabled = false;
            IsPlaying = false;
            LoopTime = 0;
            Particles.Clear();
        }

        public void Restart()
        {
            Stop();
            Play();
        }
        #endregion

        public override void Update()
        {
            if (!IsPlaying) return;

            // Particle spawn check
            if(EmissionMode == ParticleEmissionMode.Constant)
            {
                if(IsLoop || (!IsLoop && LoopTime <= Duration))
                {
                    while (spawnTime > spawnInterval)
                    {
                        spawnTime -= spawnInterval;
                        SpawnParticle();
                    }
                }
            }
            else if(EmissionMode == ParticleEmissionMode.Burst)
            {
                if(IsLoop && LoopTime > Duration)
                {
                    currentBurstAmount = 0;
                    LoopTime = 0;
                }
                while (spawnTime > spawnInterval && currentBurstAmount < BurstSpawnAmount)
                {
                    spawnTime -= spawnInterval;
                    currentBurstAmount++;
                    SpawnParticle();
                }
            }

            ApplyNoise();

            // Updating all particles
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle particle = Particles[i];

                // Used for particle over-lifetime effects (if enabled)
                float t = 1 - (particle.Lifetime / particle.InitialLifetime);

                particle.Position += particle.Velocity * Time.DeltaTime * (speedOverLifetimeEnabled ? SpeedOverLifetime.Evaluate(t) : 1);
                particle.Rotation += particle.AngularVelocity * Time.DeltaTime;
                particle.Lifetime -= Time.DeltaTime;
                if (particle.Lifetime <= 0)
                {
                    particle.IsDead = true;
                }
            }

            UpdateMesh();

            // Calculating velocity
            calculatedVelocity = transform.Position - lastPosition;
            lastPosition = transform.Position;

            LoopTime += Time.DeltaTime;
            if(EmissionMode == ParticleEmissionMode.Constant)
            {
                spawnTime += Time.DeltaTime;
            }
            else if(EmissionMode == ParticleEmissionMode.Burst)
            {
                if(currentBurstAmount < BurstSpawnAmount)
                {
                    spawnTime += Time.DeltaTime;
                }
            }
        }

        private void ApplyNoise()
        {
            if (!noiseEnabled) return;
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle p = Particles[i];

                float x = noise.GetSimplex(randomSeed + (p.Position.X + p.Origin.X) * NoiseFrequency + LoopTime * NoiseSpeed,
                    randomSeed + (p.Position.Y + p.Origin.Y) * NoiseFrequency + LoopTime * NoiseSpeed);
                float y = noise.GetSimplex(randomSeed + (p.Position.X + p.Origin.X) * NoiseFrequency + LoopTime * NoiseSpeed,
                    randomSeed + (p.Position.Y + p.Origin.Y) * NoiseFrequency + LoopTime * NoiseSpeed, randomSeed * 1337 * NoiseFrequency);
                Vector2 velocityOffset = new Vector2(x, y) * NoiseStrength;

                p.Velocity += velocityOffset * Time.DeltaTime;
            }
        }

        private void SpawnParticle()
        {
            Particle p = null;

            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i].IsDead)
                {
                    p = Particles[i];
                    break;
                }
            }

            if (p == null)
            {
                // If a new particle must be created, check to see if there is room left
                if (Particles.Count >= MaxParticles) return;
                p = new Particle();
                Particles.Add(p);
            }

            // Spawn speed
            float spawnSpeed = MathEx.RandomFloatInRange(random, SpeedRange.X, SpeedRange.Y);

            // Spawn rotation
            float spawnRotation = MathEx.RandomFloatInRange(random, StartRotationRange.X, StartRotationRange.Y);

            // Spawn angular velocity
            float spawnAngularVelocity = MathEx.RandomFloatInRange(random, AngularVelocityRange.X, AngularVelocityRange.Y);

            // Spawn color
            float percentage = (float)random.NextDouble();
            Color spawnColor = ColorRange.Evaluate(percentage);

            // Spawn size
            float spawnSize = MathEx.RandomFloatInRange(random, SizeRange.X, SizeRange.Y);

            // Spawn lifetime
            float spawnLifetime = MathEx.RandomFloatInRange(random, LifetimeRange.X, LifetimeRange.Y);

            // Spawn position
            Vector2 spawnPosition = IsWorldSpace ? transform.Position : Vector2.Zero;
            Vector2 alongNormal = Vector2.Zero;
            switch (EmissionShape)
            {
                case ParticleEmissionShape.VolumeSquare:
                    Vector2 squareVolumePos = new Vector2(MathEx.RandomFloatInRange(random, -EmissionSize / 2f, EmissionSize / 2f),
                        MathEx.RandomFloatInRange(random, -EmissionSize / 2f, EmissionSize / 2f));
                    spawnPosition += squareVolumePos;
                    alongNormal = Vector2.Normalize(squareVolumePos);
                    break;
                case ParticleEmissionShape.VolumeCircle:
                    Vector2 circleVolumePos = MathEx.RandomPositionInsideCircle(random, EmissionSize / 2f);
                    spawnPosition += circleVolumePos;
                    alongNormal = Vector2.Normalize(circleVolumePos);
                    break;
                case ParticleEmissionShape.Square:
                    float x;
                    float y;
                    float sign = random.NextDouble() < 0.5 ? -1 : 1;
                    bool xAxis = random.NextDouble() < 0.5;
                    if (xAxis)
                    {
                        x = sign;
                        y = MathEx.RandomFloatInRange(random, -1, 1);
                    }
                    else
                    {
                        x = MathEx.RandomFloatInRange(random, -1, 1);
                        y = sign;
                    }
                    Vector2 squarePos = new Vector2(x, y) * EmissionSize / 2f;
                    spawnPosition += squarePos;
                    alongNormal = new Vector2(xAxis ? sign : 0, !xAxis ? sign : 0);
                    break;
                case ParticleEmissionShape.Circle:
                    Vector2 circlePos = MathEx.RandomPositionOnCircle(random, EmissionSize / 2f);
                    spawnPosition += circlePos;
                    alongNormal = Vector2.Normalize(circlePos);
                    break;
                case ParticleEmissionShape.Line:
                    float lineOffset = MathEx.RandomFloatInRange(random, -EmissionSize / 2f, EmissionSize / 2f);
                    spawnPosition += Vector2.Normalize(new Vector2(EmissionDirection.Y, EmissionDirection.X)) * lineOffset;
                    alongNormal = EmissionDirection;
                    break;
            }

            // Spawn direction
            float spawnDirRotation = (float)(random.NextDouble() * EmissionSpreadAngle);
            Vector2 spawnDirection = MathEx.RotateVector2(EmitAlongEmissionShapeNormal ? alongNormal : EmissionDirection,
                spawnDirRotation - (EmissionSpreadAngle / 2f));
            if (InvertEmissionDirection) spawnDirection *= -1;

            p.Initialize(spawnPosition, Vector2.Zero, (spawnDirection * spawnSpeed) + (InheritVelocity ? calculatedVelocity : Vector2.Zero), spawnRotation,
                spawnAngularVelocity, spawnColor, spawnSize, spawnLifetime);
        }

        #region Meshes
        private void UpdateMesh()
        {
            int x = 0;
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i].IsDead) continue;

                UpdateParticleMesh(x, Particles[i]);
                indices[(x * 6) + 0] = (uint)((x * 4) + 2);
                indices[(x * 6) + 1] = (uint)((x * 4) + 1);
                indices[(x * 6) + 2] = (uint)((x * 4) + 0);
                indices[(x * 6) + 3] = (uint)((x * 4) + 3);
                indices[(x * 6) + 4] = (uint)((x * 4) + 2);
                indices[(x * 6) + 5] = (uint)((x * 4) + 0);

                x++;
            }

            if (x == 0)
            {
                renderer.Enabled = false;
            }
            else
            {
                renderer.Enabled = true;
                renderer.CustomIndexRenderCount = x * 6;
                if (renderer.IsLoaded)
                {
                    renderer.SetVertexArrays(vertices, indices, !renderer.IsLoaded, renderer.IsLoaded);
                }
            }
        }

        private void UpdateParticleMesh(int _index, Particle _particle)
        {
            // Used for particle over-lifetime effects (if enabled)
            float t = 1 - (_particle.Lifetime / _particle.InitialLifetime);

            int i = _index * 32;
            float hs = _particle.Size * (sizeOverLifetimeEnabled ? SizeOverLifetime.Evaluate(t) : 1) / 2f;

            Vector2 tl = MathEx.RotateVector2(new Vector2(-hs, hs), _particle.Rotation);
            Vector2 tr = MathEx.RotateVector2(new Vector2(hs, hs), _particle.Rotation);
            Vector2 br = MathEx.RotateVector2(new Vector2(hs, -hs), _particle.Rotation);
            Vector2 bl = MathEx.RotateVector2(new Vector2(-hs, -hs), _particle.Rotation);

            float xpos = _particle.Position.X + (IsWorldSpace ? _particle.Origin.X - transform.Position.X : _particle.Origin.X) * 2;
            float ypos = _particle.Position.Y + (IsWorldSpace ? _particle.Origin.Y - transform.Position.Y : _particle.Origin.Y) * 2;

            Vector4 color;
            if (colorOverLifetimeEnabled)
            {
                Color eval = ColorOverLifetime.Evaluate(t);
                color = new Vector4((_particle.Color.R / 255f) * (eval.R / 255f), (_particle.Color.G / 255f) * (eval.G / 255f),
                    (_particle.Color.B / 255f) * (eval.B / 255f), (_particle.Color.A / 255f) * (eval.A / 255f));
            }
            else
            {
                color = new Vector4(_particle.Color.R, _particle.Color.G, _particle.Color.B, _particle.Color.A);
            }

            // Top Left
            vertices[i + 0] = tl.X + xpos;
            vertices[i + 1] = tl.Y + ypos;
            vertices[i + 2] = 0;
            vertices[i + 3] = 1;
            vertices[i + 4] = color.X;
            vertices[i + 5] = color.Y;
            vertices[i + 6] = color.Z;
            vertices[i + 7] = color.W;

            // Top Right
            vertices[i + 8] = tr.X + xpos;
            vertices[i + 9] = tr.Y + ypos;
            vertices[i + 10] = 1;
            vertices[i + 11] = 1;
            vertices[i + 12] = color.X;
            vertices[i + 13] = color.Y;
            vertices[i + 14] = color.Z;
            vertices[i + 15] = color.W;

            // Bottom Right
            vertices[i + 16] = br.X + xpos;
            vertices[i + 17] = br.Y + ypos;
            vertices[i + 18] = 1;
            vertices[i + 19] = 0;
            vertices[i + 20] = color.X;
            vertices[i + 21] = color.Y;
            vertices[i + 22] = color.Z;
            vertices[i + 23] = color.W;

            // Bottom Left
            vertices[i + 24] = bl.X + xpos;
            vertices[i + 25] = bl.Y + ypos;
            vertices[i + 26] = 0;
            vertices[i + 27] = 0;
            vertices[i + 28] = color.X;
            vertices[i + 29] = color.Y;
            vertices[i + 30] = color.Z;
            vertices[i + 31] = color.W;
        }
        #endregion

        protected override void OnDispose()
        {
            // remove all particles
            renderer?.Dispose();
            ParticleSystemBaseSystem.Unregister(this);
            RenderLayerManager.RemoveRenderable(this);
        }

        public void Render()
        {
            Program.Game.SetBlendingMode(BlendMode);
            renderer.Render();
            Program.Game.SetBlendingMode(BlendMode.Interpolative);
        }
    }

    public enum ParticleEmissionShape
    {
        VolumeSquare,
        VolumeCircle,
        Square,
        Circle,
        Line
    }

    public enum ParticleEmissionMode
    {
        Constant,
        Burst
    }
}