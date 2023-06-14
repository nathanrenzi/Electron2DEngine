using OpenGLTest.Rendering.Meshes;
using OpenGLTest.Rendering.Shaders;

namespace OpenGLTest.GameObjects
{
    public class GameObject
    {
        public Transform transform = new Transform();
        public MeshRenderer meshRenderer;

        public GameObject()
        {
            GameObjectManager.RegisterGameObject(this);
        }

        public void InitializeMeshRenderer(Shader _shader = null)
        {
            if (_shader == null) return;

            float[] vertices =
{
                // Position    UV        Color
                -0.5f, 0.5f,   0f, 0f,   1f, 0f, 0f,    // top left
                0.5f, 0.5f,    1f, 0f,   0f, 1f, 0f,    // top right
                -0.5f, -0.5f,  0f, 1f,   0f, 0f, 1f,    // bottom left

                0.5f, 0.5f,    1f, 0f,   0f, 1f, 0f,    // top right
                0.5f, -0.5f,   1f, 1f,   0f, 1f, 1f,    // bottom right
                -0.5f, -0.5f,  0f, 1f,   0f, 0f, 1f,    // bottom left
            };

            meshRenderer = new MeshRenderer(transform, _shader, vertices);
            meshRenderer.Load();
        }

        public virtual void Start()
        {
            
        }

        public virtual void Render()
        {
            if (meshRenderer != null) meshRenderer.Render();
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