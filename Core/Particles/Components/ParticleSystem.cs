using Electron2D.Core.ECS;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace Electron2D.Core.Particles
{
    public class ParticleSystemBaseSystem : BaseSystem<ParticleSystem> { }
    public class ParticleSystem : Component, IRenderable
    {
        #region Particle System
        public bool IsLoop { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsWorldSpace { get; set; }
        public bool InheritVelocity { get; set; }
        public int MaxParticles { get; }
        public float LoopTime { get; private set; }
        public List<Particle> Particles { get; private set; }
        public int RenderLayer { get; }
        #endregion

        #region Emission Settings
        public ParticleEmissionShape EmissionShape { get; set; }
        public Vector2 EmissionDirection { get; set; }
        public float EmissionMaxAngle { get; set; }
        public float EmissionParticlesPerSecond { get; set; }
        #endregion

        #region Start Settings
        public Vector2 StartSizeRange { get; set; }
        public Vector2 StartRotationRange { get; set; }
        public Vector2 StartAngularVelocityRange { get; set; }
        public Vector2 StartLifetimeRange { get; set; }
        public Vector2 StartSpeedRange { get; set; }
        public Gradient StartColorRange { get; set; }
        #endregion

        #region Particle Settings
        public Gradient ColorOverLifetime { get; private set; }
        public bool ColorOverLifetimeEnabled { get; private set; }
        #endregion

        #region Private
        private float[] vertices;
        private uint[] indices;
        private MeshRenderer renderer;
        private Transform transform;
        private Transform fakeTransform;
        private Random random = new Random(DateTime.Now.Millisecond);
        private bool playOnAwake;
        private Material material;
        private float spawnInterval { get { return 1f / EmissionParticlesPerSecond; } }
        private float lastSpawnedTime = -10;

        private Vector2 lastPosition;
        private Vector2 calculatedVelocity;
        #endregion

        public ParticleSystem(bool _playOnAwake, bool _isLoop, bool _isWorldSpace, bool _inheritVelocity, ParticleEmissionShape _emissionShape, Vector2 _emissionDirection,
            float _emissionMaxAngle, float _emissionParticlesPerSecond, int _maxParticles, Vector2 _startSizeRange, Vector2 _startRotationRange,
            Vector2 _startAngularVelocityRange, Vector2 _startLifetimeRange, Vector2 _startSpeedRange,
            Gradient _startColorRange, Material _material, int _renderLayer = 1)
        {
            playOnAwake = _playOnAwake;
            IsLoop = _isLoop;
            IsWorldSpace = _isWorldSpace;
            InheritVelocity = _inheritVelocity;
            EmissionShape = _emissionShape;
            EmissionDirection = _emissionDirection;
            EmissionMaxAngle = _emissionMaxAngle;
            EmissionParticlesPerSecond = _emissionParticlesPerSecond;
            MaxParticles = _maxParticles;
            StartSizeRange = _startSizeRange;
            StartRotationRange = _startRotationRange;
            StartAngularVelocityRange = _startAngularVelocityRange;
            StartLifetimeRange = _startLifetimeRange;
            StartSpeedRange = _startSpeedRange;
            StartColorRange = _startColorRange;
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

            ParticleSystemBaseSystem.Register(this);
            RenderLayerManager.OrderRenderable(this);
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
            renderer.Layout = layout;
            renderer.SetVertexArrays(vertices, indices, false, renderer.IsLoaded);
            renderer.Load(false);

            if (playOnAwake) Play();
        }

        #region Setters
        private void SetModelMatrix()
        {
            if(IsWorldSpace)
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

            if (Time.GameTime > lastSpawnedTime + spawnInterval)
            {
                lastSpawnedTime = Time.GameTime;

                SpawnParticle();
            }

            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Update();
            }

            UpdateMesh();

            // Calculating velocity
            calculatedVelocity = transform.Position - lastPosition;
            lastPosition = transform.Position;

            LoopTime += Time.DeltaTime;
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

            // Spawn direction
            float spawnDirRotation = (float)(random.NextDouble() * EmissionMaxAngle);
            Vector2 spawnDirection = MathEx.RotateVector2(EmissionDirection, spawnDirRotation - (EmissionMaxAngle / 2f));

            // Spawn speed
            float spawnSpeed = MathEx.RandomFloatInRange(random, StartSpeedRange.X, StartSpeedRange.Y);

            // Spawn rotation
            float spawnRotation = MathEx.RandomFloatInRange(random, StartRotationRange.X, StartRotationRange.Y);

            // Spawn angular velocity
            float spawnAngularVelocity = MathEx.RandomFloatInRange(random, StartAngularVelocityRange.X, StartAngularVelocityRange.Y);

            // Spawn color
            float percentage = (float)random.NextDouble();
            Color spawnColor = StartColorRange.Evaluate(percentage);

            // Spawn size
            float spawnSize = MathEx.RandomFloatInRange(random, StartSizeRange.X, StartSizeRange.Y);

            // Spawn lifetime
            float spawnLifetime = MathEx.RandomFloatInRange(random, StartLifetimeRange.X, StartLifetimeRange.Y);

            p.Initialize(transform.Position, Vector2.Zero, (spawnDirection * spawnSpeed) + (InheritVelocity ? calculatedVelocity : Vector2.Zero), spawnRotation,
                spawnAngularVelocity, spawnColor, spawnSize, spawnLifetime);
        }

        #region Meshes
        private void UpdateMesh()
        {
            int x = 0;
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i].IsDead) continue;

                CreateParticleMesh(x, Particles[i]);
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

        private void CreateParticleMesh(int _index, Particle _particle)
        {
            int i = _index * 32;
            float hs = _particle.Size / 2f;

            Vector2 tl = MathEx.RotateVector2(new Vector2(-hs, hs), _particle.Rotation);
            Vector2 tr = MathEx.RotateVector2(new Vector2(hs, hs), _particle.Rotation);
            Vector2 br = MathEx.RotateVector2(new Vector2(hs, -hs), _particle.Rotation);
            Vector2 bl = MathEx.RotateVector2(new Vector2(-hs, -hs), _particle.Rotation);

            float xpos = _particle.Position.X + (IsWorldSpace ? _particle.Origin.X - transform.Position.X : 0) * 2;
            float ypos = _particle.Position.Y + (IsWorldSpace ? _particle.Origin.Y - transform.Position.Y : 0) * 2;

            // Top Left
            vertices[i + 0] = tl.X + xpos;
            vertices[i + 1] = tl.Y + ypos;
            vertices[i + 2] = 0;
            vertices[i + 3] = 1;
            vertices[i + 4] = _particle.Color.R / 255f;
            vertices[i + 5] = _particle.Color.G / 255f;
            vertices[i + 6] = _particle.Color.B / 255f;
            vertices[i + 7] = _particle.Color.A / 255f;

            // Top Right
            vertices[i + 8] = tr.X + xpos;
            vertices[i + 9] = tr.Y + ypos;
            vertices[i + 10] = 1;
            vertices[i + 11] = 1;
            vertices[i + 12] = _particle.Color.R / 255f;
            vertices[i + 13] = _particle.Color.G / 255f;
            vertices[i + 14] = _particle.Color.B / 255f;
            vertices[i + 15] = _particle.Color.A / 255f;

            // Bottom Right
            vertices[i + 16] = br.X + xpos;
            vertices[i + 17] = br.Y + ypos;
            vertices[i + 18] = 1;
            vertices[i + 19] = 0;
            vertices[i + 20] = _particle.Color.R / 255f;
            vertices[i + 21] = _particle.Color.G / 255f;
            vertices[i + 22] = _particle.Color.B / 255f;
            vertices[i + 23] = _particle.Color.A / 255f;

            // Bottom Left
            vertices[i + 24] = bl.X + xpos;
            vertices[i + 25] = bl.Y + ypos;
            vertices[i + 26] = 0;
            vertices[i + 27] = 0;
            vertices[i + 28] = _particle.Color.R / 255f;
            vertices[i + 29] = _particle.Color.G / 255f;
            vertices[i + 30] = _particle.Color.B / 255f;
            vertices[i + 31] = _particle.Color.A / 255f;
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
            renderer.Render();
        }
    }

    public enum ParticleEmissionShape
    {
        Box,
        Circle,
        Line
    }
}