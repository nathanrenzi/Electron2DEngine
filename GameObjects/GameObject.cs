using Electron2D.Rendering.Meshes;
using Electron2D.Rendering.Shaders;

namespace Electron2D.GameObjects
{
    public abstract class GameObject
    {
        public Transform transform = new Transform();
        public MeshRenderer renderer;
        public Shader shader;
        public bool useRendering { get; private set; }

        public GameObject(bool _useRendering = true)
        {
            useRendering = _useRendering;
            GameObjectManager.RegisterGameObject(this);

            // Initializing the shader and transform to their default values
            shader = ShaderLoader.GetShader(DefaultShaderType.SPRITE);
            renderer = new MeshRenderer(transform, shader);
        }

        public virtual void Start()
        {
            
        }

        /// <summary>
        /// Called automatically after Start if rendering is enabled. Initializes the mesh renderer and inputs
        /// </summary>
        public void InitializeMeshRenderer()
        {
            renderer.Load();
        }

        public virtual void Render()
        {
            if (renderer != null) renderer.Render();
        }

        public virtual void Update()
        {

        }

        public void Destroy()
        {
            GameObjectManager.UnregisterGameObject(this);
            OnDestroy();
        }

        public virtual void OnDestroy()
        {

        }
    }
}