using Electron2D.PhysicsBox2D;
using Electron2D.Rendering;

namespace Electron2D
{
    public struct TileData
    {
        public string Name { get; set; }
        public int SpriteColumn { get; set; }
        public int SpriteRow { get; set; }
        public Material Material { get; set; }
        public bool AllowRandomRotation { get; set; }
        public bool IsCollider { get; private set; } = false;
        public RigidbodyKinematicDef PhysicsSettings { get; private set; }
        public ITilemapRuleset Ruleset { get; set; }

        public TileData(Material _material, string _name, int _spriteColumn = 0,
            int _spriteRow = 0, bool _allowRandomRotation = false,
            ITilemapRuleset _ruleset = null)
        {
            Name = _name;
            Material = _material;
            SpriteColumn = _spriteColumn;
            SpriteRow = _spriteRow;
            AllowRandomRotation = _allowRandomRotation;
            Ruleset = _ruleset;
            PhysicsSettings = new RigidbodyKinematicDef();
        }

        public TileData(string _name, int _spriteColumn = 0, int _spriteRow = 0,
            bool _allowRandomRotation = false, ITilemapRuleset _ruleset = null)
        {
            Name = _name;
            Material = null;
            SpriteColumn = _spriteColumn;
            SpriteRow = _spriteRow;
            AllowRandomRotation = _allowRandomRotation;
            Ruleset = _ruleset;
            PhysicsSettings = new RigidbodyKinematicDef();
        }

        public TileData SetAsCollider(RigidbodyKinematicDef _physicsSettings)
        {
            IsCollider = true;
            PhysicsSettings = _physicsSettings;
            return this;
        }
    }
}
