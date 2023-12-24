using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Patterns;
using Electron2D.Core.Rendering;

namespace Electron2D.Build
{
    public enum PlayerState
    {
        IDLE,
        RUN,
        ATTACK
    }
    public class PlayerAnimator
    {
        public static string PlayerTexturePath = "Build/Resources/Textures/KnightSpritesheets/";

        public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
        public float PlayerSpeedMultiplier = 1.0f;
        protected Sprite sprite;

        public PlayerAnimator(Sprite _sprite)
        {
            sprite = _sprite;

            StateMachine.Add(new PlayerIdle(this, 10));
            StateMachine.Add(new PlayerRun(this, 20));
            StateMachine.Add(new PlayerAttack(this, 18));

            Game.OnUpdateEvent += Update;
        }

        private void Update()
        {
            StateMachine.Update();
        }

        public class PlayerIdle : State<PlayerState>
        {
            public float SpritesPerSecond;
            private PlayerAnimator animator;
            private Texture2DArray texture;

            public PlayerIdle(PlayerAnimator _animator, int _spritesPerSecond) : base(PlayerState.IDLE)
            {
                animator = _animator;
                texture = ResourceManager.Instance.LoadTextureArray(PlayerTexturePath + "_Idle.png", 120, 80);
                SpritesPerSecond = _spritesPerSecond;
            }

            public override void Enter()
            {
                animator.sprite.Renderer.SpriteAnimationSpeed = SpritesPerSecond;
                animator.sprite.AddTextureToQueue(texture, false);
                base.Enter();
            }
        }
        public class PlayerRun : State<PlayerState>
        {
            public float SpritesPerSecond;
            private PlayerAnimator animator;
            private Texture2DArray texture;

            public PlayerRun(PlayerAnimator _animator, int _spritesPerSecond) : base(PlayerState.RUN)
            {
                animator = _animator;
                texture = ResourceManager.Instance.LoadTextureArray(PlayerTexturePath + "_Run.png", 120, 80);
                SpritesPerSecond = _spritesPerSecond;
            }

            public override void Enter()
            {
                animator.sprite.AddTextureToQueue(texture, false);
                base.Enter();
            }

            public override void Update()
            {
                animator.sprite.Renderer.SpriteAnimationSpeed = SpritesPerSecond * animator.PlayerSpeedMultiplier;
                base.Update();
            }
        }
        public class PlayerAttack : State<PlayerState>
        {
            public float SpritesPerSecond;
            private PlayerAnimator animator;
            private Texture2DArray texture;

            public PlayerAttack(PlayerAnimator _animator, int _spritesPerSecond) : base(PlayerState.ATTACK)
            {
                animator = _animator;
                texture = ResourceManager.Instance.LoadTextureArray(PlayerTexturePath + "_AttackNoMovement.png", 120, 80);
                SpritesPerSecond = _spritesPerSecond;
            }

            public override void Enter()
            {
                CanSwitch = false;
                ITexture tex = animator.sprite.Renderer.Material.MainTexture;
                animator.sprite.ForceTexture(texture, true);
                if(tex.GetTextureLayers() != -1)
                {
                    animator.sprite.AddTextureToQueue((Texture2DArray)tex, false);
                }
                animator.sprite.Renderer.OnCompletedSpriteLoop += FinishAttack;
                base.Enter();
            }

            private void FinishAttack()
            {
                CanSwitch = true;
            }

            public override void Exit()
            {
                animator.sprite.Renderer.OnCompletedSpriteLoop -= FinishAttack;
                base.Exit();
            }

            public override void Update()
            {
                animator.sprite.Renderer.SpriteAnimationSpeed = SpritesPerSecond;
                base.Update();
            }
        }
    }
}
