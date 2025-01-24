using Electron2D.Misc;
using Electron2D.Rendering;
using System.Drawing;
using System.Numerics;
using DotnetNoise;

namespace Electron2D
{
    public class ParticleSystem : IRenderable, IGameClass
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
        private bool _colorOverLifetimeEnabled;
        public Curve SizeOverLifetime { get; private set; }
        private bool _sizeOverLifetimeEnabled;
        public Curve SpeedOverLifetime { get; private set; }
        private bool _speedOverLifetimeEnabled;
        public float NoiseStrength { get; private set; } = 0;
        public float NoiseSpeed { get; private set; } = 0;
        public float NoiseFrequency { get; private set; } = 1;
        private bool _noiseEnabled;
        public MeshRenderer Renderer { get; private set; }

        #region Private Fields
        private bool _ignorePostProcessing;
        private float[] _vertices;
        private uint[] _indices;
        private Transform _transform;
        private Transform _fakeTransform;
        private Random _random;
        private int _randomSeed;
        private bool _playOnAwake;
        private Material _material;
        private FastNoise _noise;
        private int _currentBurstAmount;
        private float _spawnInterval { get { return 1f / EmissionParticlesPerSecond; } }
        private float _spawnTime;
        private Vector2 _lastPosition;
        private Vector2 _calculatedVelocity;
        #endregion

        public ParticleSystem(Transform transform, bool playOnAwake, bool prewarm, bool isWorldSpace, bool inheritVelocity,
            int maxParticles, Material material, int renderLayer = 1, int randomSeed = -1, bool ignorePostProcessing = false)
        {
            _playOnAwake = playOnAwake;
            IsWorldSpace = isWorldSpace;
            Prewarm = prewarm;
            InheritVelocity = inheritVelocity;
            MaxParticles = maxParticles;
            RenderLayer = renderLayer;
            _material = material;
            _ignorePostProcessing = ignorePostProcessing;

            _fakeTransform = new Transform();

            // Pre-allocating arrays
            // 4 vertices * (X + Y + U + V) * total particles
            _vertices = new float[4 * 8 * maxParticles];
            // 6 indices (2 triangles) * total particles
            _indices = new uint[6 * maxParticles];

            //Pre-allocating particle list
            Particles = new List<Particle>(MaxParticles);

            _randomSeed = randomSeed == -1 ? DateTime.Now.Millisecond : randomSeed;
            _random = new Random(_randomSeed);
            _noise = new FastNoise(_randomSeed);

            _transform = transform;
            if (_transform == null)
            {
                Debug.LogError("PARTICLE SYSTEM: Cannot create particle system if entity does not have a Transform component!");
                return;
            }
            Renderer = new MeshRenderer(_transform, _material);
            Renderer.UseCustomIndexRenderCount = true;
            Renderer.OnBeforeRender += SetModelMatrix;
            BufferLayout layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);
            Renderer.SetBufferLayoutBeforeLoad(layout);
            Renderer.SetVertexArrays(_vertices, _indices, false, Renderer.IsLoaded);
            Renderer.Load(false);

            if (Prewarm) PrewarmParticles(); // Currently does not work
            if (_playOnAwake) Play();

            RenderLayerManager.OrderRenderable(this);
            Program.Game.RegisterGameClass(this);
        }

        ~ParticleSystem()
        {
            Dispose();
        }

        public void Dispose()
        {
            // remove all particles
            Renderer?.Dispose();
            Program.Game.UnregisterGameClass(this);
            RenderLayerManager.RemoveRenderable(this);
            GC.SuppressFinalize(this);
        }

        public void FixedUpdate() { }


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

        public ParticleSystem SetBurstEmissionMode(bool isLoop, int burstSpawnAmount, float loopDelay = -1,
            float emissionsPerSecond = -1)
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
            _colorOverLifetimeEnabled = true;
            ColorOverLifetime = colorOverLifetime;
            return this;
        }

        public ParticleSystem SetSizeOverLifetime(Curve sizeCurve)
        {
            _sizeOverLifetimeEnabled = true;
            SizeOverLifetime = sizeCurve;
            return this;
        }

        public ParticleSystem SetSpeedOverLifetime(Curve speedCurve)
        {
            _speedOverLifetimeEnabled = true;
            SpeedOverLifetime = speedCurve;
            return this;
        }

        public ParticleSystem SetNoiseSettings(float noiseStrength, float noiseFrequency, float noiseSpeed)
        {
            _noiseEnabled = true;
            NoiseStrength = noiseStrength;
            NoiseFrequency = noiseFrequency;
            NoiseSpeed = noiseSpeed;
            return this;
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

                    particle.Position += particle.Velocity * deltaTime * (_speedOverLifetimeEnabled ? SpeedOverLifetime.Evaluate(t) : 1);
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
                Renderer.Material.Shader.SetMatrix4x4("model", _fakeTransform.GetScaleMatrix() * _fakeTransform.GetRotationMatrix() * _transform.GetPositionMatrix());
            }
        }
        #endregion

        #region Getters
        public MeshRenderer GetRenderer() => Renderer;
        public int GetRenderLayer() => RenderLayer;
        #endregion

        #region Playback
        public void Play()
        {
            Renderer.Enabled = true;
            IsPlaying = true;
            _currentBurstAmount = 0;
            LoopTime = 0;
        }

        public void SetPaused(bool _pause)
        {
            IsPlaying = !_pause;
        }

        public void Stop()
        {
            Renderer.Enabled = false;
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

        public void Update()
        {
            if (!IsPlaying) return;

            // Particle spawn check
            if(EmissionMode == ParticleEmissionMode.Constant)
            {
                if(IsLoop || (!IsLoop && LoopTime <= Duration))
                {
                    while (_spawnTime > _spawnInterval)
                    {
                        _spawnTime -= _spawnInterval;
                        SpawnParticle();
                    }
                }
            }
            else if(EmissionMode == ParticleEmissionMode.Burst)
            {
                if(IsLoop && LoopTime > Duration)
                {
                    _currentBurstAmount = 0;
                    LoopTime = 0;
                }
                while (_spawnTime > _spawnInterval && _currentBurstAmount < BurstSpawnAmount)
                {
                    _spawnTime -= _spawnInterval;
                    _currentBurstAmount++;
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

                particle.Position += particle.Velocity * Time.DeltaTime * (_speedOverLifetimeEnabled ? SpeedOverLifetime.Evaluate(t) : 1);
                particle.Rotation += particle.AngularVelocity * Time.DeltaTime;
                particle.Lifetime -= Time.DeltaTime;
                if (particle.Lifetime <= 0)
                {
                    particle.IsDead = true;
                }
            }

            UpdateMesh();

            // Calculating velocity
            _calculatedVelocity = _transform.Position - _lastPosition;
            _lastPosition = _transform.Position;

            LoopTime += Time.DeltaTime;
            if(EmissionMode == ParticleEmissionMode.Constant)
            {
                _spawnTime += Time.DeltaTime;
            }
            else if(EmissionMode == ParticleEmissionMode.Burst)
            {
                if(_currentBurstAmount < BurstSpawnAmount)
                {
                    _spawnTime += Time.DeltaTime;
                }
            }
        }

        private void ApplyNoise()
        {
            if (!_noiseEnabled) return;
            for (int i = 0; i < Particles.Count; i++)
            {
                Particle p = Particles[i];

                float x = _noise.GetSimplex(_randomSeed + (p.Position.X + p.Origin.X) * NoiseFrequency + LoopTime * NoiseSpeed,
                    _randomSeed + (p.Position.Y + p.Origin.Y) * NoiseFrequency + LoopTime * NoiseSpeed);
                float y = _noise.GetSimplex(_randomSeed + (p.Position.X + p.Origin.X) * NoiseFrequency + LoopTime * NoiseSpeed,
                    _randomSeed + (p.Position.Y + p.Origin.Y) * NoiseFrequency + LoopTime * NoiseSpeed, _randomSeed * 1337 * NoiseFrequency);
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
            float spawnSpeed = MathEx.RandomFloatInRange(_random, SpeedRange.X, SpeedRange.Y);

            // Spawn rotation
            float spawnRotation = MathEx.RandomFloatInRange(_random, StartRotationRange.X, StartRotationRange.Y);

            // Spawn angular velocity
            float spawnAngularVelocity = MathEx.RandomFloatInRange(_random, AngularVelocityRange.X, AngularVelocityRange.Y);

            // Spawn color
            float percentage = (float)_random.NextDouble();
            Color spawnColor = ColorRange.Evaluate(percentage);

            // Spawn size
            float spawnSize = MathEx.RandomFloatInRange(_random, SizeRange.X, SizeRange.Y);

            // Spawn lifetime
            float spawnLifetime = MathEx.RandomFloatInRange(_random, LifetimeRange.X, LifetimeRange.Y);

            // Spawn position
            Vector2 spawnPosition = IsWorldSpace ? _transform.Position : Vector2.Zero;
            Vector2 alongNormal = Vector2.Zero;
            switch (EmissionShape)
            {
                case ParticleEmissionShape.VolumeSquare:
                    Vector2 squareVolumePos = new Vector2(MathEx.RandomFloatInRange(_random, -EmissionSize / 2f, EmissionSize / 2f),
                        MathEx.RandomFloatInRange(_random, -EmissionSize / 2f, EmissionSize / 2f));
                    spawnPosition += squareVolumePos;
                    alongNormal = Vector2.Normalize(squareVolumePos);
                    break;
                case ParticleEmissionShape.VolumeCircle:
                    Vector2 circleVolumePos = MathEx.RandomPositionInsideCircle(_random, EmissionSize / 2f);
                    spawnPosition += circleVolumePos;
                    alongNormal = Vector2.Normalize(circleVolumePos);
                    break;
                case ParticleEmissionShape.Square:
                    float x;
                    float y;
                    float sign = _random.NextDouble() < 0.5 ? -1 : 1;
                    bool xAxis = _random.NextDouble() < 0.5;
                    if (xAxis)
                    {
                        x = sign;
                        y = MathEx.RandomFloatInRange(_random, -1, 1);
                    }
                    else
                    {
                        x = MathEx.RandomFloatInRange(_random, -1, 1);
                        y = sign;
                    }
                    Vector2 squarePos = new Vector2(x, y) * EmissionSize / 2f;
                    spawnPosition += squarePos;
                    alongNormal = new Vector2(xAxis ? sign : 0, !xAxis ? sign : 0);
                    break;
                case ParticleEmissionShape.Circle:
                    Vector2 circlePos = MathEx.RandomPositionOnCircle(_random, EmissionSize / 2f);
                    spawnPosition += circlePos;
                    alongNormal = Vector2.Normalize(circlePos);
                    break;
                case ParticleEmissionShape.Line:
                    float lineOffset = MathEx.RandomFloatInRange(_random, -EmissionSize / 2f, EmissionSize / 2f);
                    spawnPosition += Vector2.Normalize(new Vector2(EmissionDirection.Y, EmissionDirection.X)) * lineOffset;
                    alongNormal = EmissionDirection;
                    break;
            }

            // Spawn direction
            float spawnDirRotation = (float)(_random.NextDouble() * EmissionSpreadAngle);
            Vector2 spawnDirection = MathEx.RotateVector2(EmitAlongEmissionShapeNormal ? alongNormal : EmissionDirection,
                spawnDirRotation - (EmissionSpreadAngle / 2f));
            if (InvertEmissionDirection) spawnDirection *= -1;

            p.Initialize(spawnPosition, Vector2.Zero, (spawnDirection * spawnSpeed) + (InheritVelocity ? _calculatedVelocity : Vector2.Zero), spawnRotation,
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
                _indices[(x * 6) + 0] = (uint)((x * 4) + 2);
                _indices[(x * 6) + 1] = (uint)((x * 4) + 1);
                _indices[(x * 6) + 2] = (uint)((x * 4) + 0);
                _indices[(x * 6) + 3] = (uint)((x * 4) + 3);
                _indices[(x * 6) + 4] = (uint)((x * 4) + 2);
                _indices[(x * 6) + 5] = (uint)((x * 4) + 0);

                x++;
            }

            if (x == 0)
            {
                Renderer.Enabled = false;
            }
            else
            {
                Renderer.Enabled = true;
                Renderer.CustomIndexRenderCount = x * 6;
                if (Renderer.IsLoaded)
                {
                    Renderer.SetVertexArrays(_vertices, _indices, !Renderer.IsLoaded, Renderer.IsLoaded);
                }
            }
        }

        private void UpdateParticleMesh(int _index, Particle _particle)
        {
            // Used for particle over-lifetime effects (if enabled)
            float t = 1 - (_particle.Lifetime / _particle.InitialLifetime);

            int i = _index * 32;
            float hs = _particle.Size * (_sizeOverLifetimeEnabled ? SizeOverLifetime.Evaluate(t) : 1) / 2f;

            Vector2 tl = MathEx.RotateVector2(new Vector2(-hs, hs), _particle.Rotation);
            Vector2 tr = MathEx.RotateVector2(new Vector2(hs, hs), _particle.Rotation);
            Vector2 br = MathEx.RotateVector2(new Vector2(hs, -hs), _particle.Rotation);
            Vector2 bl = MathEx.RotateVector2(new Vector2(-hs, -hs), _particle.Rotation);

            float xpos = _particle.Position.X + (IsWorldSpace ? _particle.Origin.X - _transform.Position.X : _particle.Origin.X) * 2;
            float ypos = _particle.Position.Y + (IsWorldSpace ? _particle.Origin.Y - _transform.Position.Y : _particle.Origin.Y) * 2;

            Vector4 color;
            if (_colorOverLifetimeEnabled)
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
            _vertices[i + 0] = tl.X + xpos;
            _vertices[i + 1] = tl.Y + ypos;
            _vertices[i + 2] = 0;
            _vertices[i + 3] = 1;
            _vertices[i + 4] = color.X;
            _vertices[i + 5] = color.Y;
            _vertices[i + 6] = color.Z;
            _vertices[i + 7] = color.W;

            // Top Right
            _vertices[i + 8] = tr.X + xpos;
            _vertices[i + 9] = tr.Y + ypos;
            _vertices[i + 10] = 1;
            _vertices[i + 11] = 1;
            _vertices[i + 12] = color.X;
            _vertices[i + 13] = color.Y;
            _vertices[i + 14] = color.Z;
            _vertices[i + 15] = color.W;

            // Bottom Right
            _vertices[i + 16] = br.X + xpos;
            _vertices[i + 17] = br.Y + ypos;
            _vertices[i + 18] = 1;
            _vertices[i + 19] = 0;
            _vertices[i + 20] = color.X;
            _vertices[i + 21] = color.Y;
            _vertices[i + 22] = color.Z;
            _vertices[i + 23] = color.W;

            // Bottom Left
            _vertices[i + 24] = bl.X + xpos;
            _vertices[i + 25] = bl.Y + ypos;
            _vertices[i + 26] = 0;
            _vertices[i + 27] = 0;
            _vertices[i + 28] = color.X;
            _vertices[i + 29] = color.Y;
            _vertices[i + 30] = color.Z;
            _vertices[i + 31] = color.W;
        }
        #endregion

        public void Render()
        {
            Program.Game.SetBlendingMode(BlendMode);
            Renderer.Render();
            Program.Game.SetBlendingMode(BlendMode.Interpolative);
        }

        public bool ShouldIgnorePostProcessing()
        {
            return _ignorePostProcessing;
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