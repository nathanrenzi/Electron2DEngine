using Box2DX.Collision;
using Box2DX.Dynamics;
using Box2DX.Common;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    //https://box2d.org/documentation/md__d_1__git_hub_box2d_docs_hello.html
    public static class Physics
    {
        // Scaling the physics so that 50 pixels equates to 1 meter in the simulation
        public static readonly float WorldScalar = 50f;

        private static Queue<(BodyDef, FixtureDef, MassData, bool, bool)> creationQueue;
        private static Dictionary<uint, Body> physicsBodies;
        private static World world;
        private static AABB aabb;
        private static bool _stepLock = false;

        /// <summary>
        /// This is called while the game is loading. Initializes the main physics world
        /// (Other physics worlds can be created separately from this using the Box2DX classes).
        /// </summary>
        /// <param name="_worldLowerBound"></param>
        /// <param name="_worldUpperBound"></param>
        /// <param name="_gravity"></param>
        /// <param name="_doSleep"></param>
        public static void Initialize(Vector2 _worldLowerBound, Vector2 _worldUpperBound, Vector2 _gravity, bool _doSleep)
        {
            aabb = new AABB()
            {
                LowerBound = new Vec2(_worldLowerBound.X, _worldLowerBound.Y),
                UpperBound = new Vec2(_worldUpperBound.X, _worldUpperBound.Y),
            };

            creationQueue = new Queue<(BodyDef, FixtureDef, MassData, bool, bool)>();
            physicsBodies = new Dictionary<uint, Body>();

            world = new World(aabb, new Vec2(_gravity.X, _gravity.Y), _doSleep);
            world.SetContactFilter(new ContactFilter());
            world.SetContactListener(new WorldContactListener());
        }

        /// <summary>
        /// Steps the physics simulation. This should only be called by the physics thread.
        /// </summary>
        /// <param name="_deltaTime"></param>
        /// <param name="_velocityIterations"></param>
        /// <param name="_positionIterations"></param>
        public static void Step(float _deltaTime, int _velocityIterations, int _positionIterations)
        {
            _stepLock = true;
            world.Step(_deltaTime, _velocityIterations, _positionIterations);
            _stepLock = false;

            // Creating physics bodies that were queued during the step
            const int maxCreationPerStep = 20;
            int count = 0;
            while(creationQueue.Count > 0 && count < maxCreationPerStep)
            {
                (BodyDef, FixtureDef, MassData, bool, bool) data = creationQueue.Dequeue();

                // If the MassData is being used (MassData cannot be null)
                if(data.Item4)
                {
                    // Dynamic with specified mass
                    CreatePhysicsBody(data.Item1, data.Item2, data.Item3, true);
                }
                else
                {
                    // Dynamic or static
                    CreatePhysicsBody(data.Item1, data.Item2, data.Item5, true);
                }
                count++;
            }
        }

        /// <summary>
        /// Creates a physics body and returns it's ID.
        /// </summary>
        /// <param name="_bodyDefinition">The definition of the physics body.</param>
        /// <param name="_fixtureDef">The definition of the fixture. This determines the shape of the physics body.</param>
        /// <param name="_autoSetMass">If this is set to true, the physics body will use the shape and density to detemine the mass.
        /// This also makes the physics body dynamic.</param>
        /// <returns></returns>
        public static uint CreatePhysicsBody(BodyDef _bodyDefinition, FixtureDef _fixtureDef, bool _autoSetMass = false, bool _ignoreQueueCount = false)
        {
            uint id = (uint)(physicsBodies.Count + (_ignoreQueueCount ? 0 : creationQueue.Count));

            try
            {
                if (_stepLock)
                {
                    creationQueue.Enqueue((_bodyDefinition, _fixtureDef, new MassData(), false, _autoSetMass));
                    return id;
                }

                Body b = world.CreateBody(_bodyDefinition);
                if (b == null)
                {
                    Debug.LogError("PHYSICS: Error creating physics body! Physics step likely in progress!");
                }

                b.CreateFixture(_fixtureDef);
                if (_autoSetMass) b.SetMassFromShapes();
                physicsBodies.Add(id, b);

                return id;
            }
            catch(Exception e)
            {
                Debug.LogError($"PHYSICS: Could not create physics body [{id}]");
                Debug.LogError(e.Message);

                return uint.MaxValue;
            }
        }

        /// <summary>
        /// Creates a dynamic physics body and returns it's ID.
        /// </summary>
        /// <param name="_bodyDefinition">The definition of the physics body.</param>
        /// <param name="_fixtureDef">The definition of the fixture. This determines the shape of the physics body.</param>
        /// <param name="_massData">The mass data of the physics body. By setting this, the physics body will be dynamic.</param>
        /// <returns></returns>
        public static uint CreatePhysicsBody(BodyDef _bodyDefinition, FixtureDef _fixtureDef, MassData _massData, bool _ignoreQueueCount = false)
        {
            uint id = (uint)(physicsBodies.Count + (_ignoreQueueCount ? 0 : creationQueue.Count));

            try
            {
                if (_stepLock)
                {
                    creationQueue.Enqueue((_bodyDefinition, _fixtureDef, _massData, true, false));
                    return id;
                }

                Body b = world.CreateBody(_bodyDefinition);
                if (b == null)
                {
                    Debug.LogError("PHYSICS: Error creating physics body! Physics step likely in progress!");
                }
                b.CreateFixture(_fixtureDef);
                b.SetMass(_massData);
                physicsBodies.Add(id, b);

                return id;
            }
            catch(Exception e)
            {
                Debug.LogError($"PHYSICS: Could not create physics body [{id}]");
                Debug.LogError(e.Message);

                return uint.MaxValue;
            }
        }

        /// <summary>
        /// Removes a physics body from the simulation.
        /// </summary>
        /// <param name="_id"></param>
        public static void RemovePhysicsBody(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            world.DestroyBody(physicsBodies[_id]);
        }

        /// <summary>
        /// Gets the position of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Vector2 GetBodyPosition(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return Vector2.Zero;
            Vec2 vec = physicsBodies[_id].GetPosition();
            return new Vector2(vec.X * WorldScalar, vec.Y * WorldScalar);
        }

        /// <summary>
        /// Gets the rotation of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static float GetBodyRotation(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return 0;
            return 180 / MathF.PI * physicsBodies[_id].GetAngle();
        }

        /// <summary>
        /// Gets the linear velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Vector2 GetBodyVelocity(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return Vector2.Zero;
            Vec2 vec = physicsBodies[_id].GetLinearVelocity();
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// Gets the angular velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static float GetBodyAngularVelocity(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return 0;
            return physicsBodies[_id].GetAngularVelocity();
        }

        /// <summary>
        /// Gets the filter data from the fixtures on a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static FilterData[] GetFilterData(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return null;

            List<FilterData> filters = new List<FilterData>();

            Fixture f = physicsBodies[_id].GetFixtureList();
            filters.Add(f.Filter);
            while (f.Next != null)
            {
                f = f.Next;
                filters.Add(f.Filter);
            }

            return filters.ToArray();
        }

        /// <summary>
        /// Applies a force to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_force"></param>
        /// <param name="_point">The point the force is applied to.</param>
        public static void ApplyForce(uint _id, Vector2 _force, Vector2 _point)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].ApplyForce(new Vec2(_force.X, _force.Y), new Vec2(_point.X, _point.Y));
        }

        /// <summary>
        /// Applies an impulse to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_impulse"></param>
        /// <param name="_point">The point the force is applied to.</param>
        public static void ApplyImpulse(uint _id, Vector2 _impulse, Vector2 _point)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].ApplyImpulse(new Vec2(_impulse.X, _impulse.Y), new Vec2(_point.X, _point.Y));
        }

        /// <summary>
        /// Applies a torque to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_torque"></param>
        public static void ApplyTorque(uint _id, float _torque)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].ApplyTorque(_torque);
        }

        /// <summary>
        /// Sets the angle (in degrees) of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngle(uint _id, float _angle)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].SetAngle(_angle * (MathF.PI / 180));
        }

        /// <summary>
        /// Sets the angular velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngularVelocity(uint _id, float _angularVelocity)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].SetAngularVelocity(_angularVelocity);
        }

        /// <summary>
        /// Sets the linear velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_linearVelocity"></param>
        public static void SetVelocity(uint _id, Vector2 _linearVelocity)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].SetLinearVelocity(new Vec2(_linearVelocity.X, _linearVelocity.Y));
        }

        /// <summary>
        /// Sets the position of a physics body. Use this instead of <see cref="Transform.Position"/> so that movements are handled within the physics engine.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_position"></param>
        public static void SetPosition(uint _id, Vector2 _position)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].SetPosition(new Vec2(_position.X / WorldScalar, _position.Y / WorldScalar));
        }

        /// <summary>
        /// Toggles the fixed rotation of a physics body.
        /// </summary>
        /// <param name="_fixedRotation"></param>
        public static void SetBodyFixedRotation(uint _id, bool _fixedRotation)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            physicsBodies[_id].SetFixedRotation(_fixedRotation);
        }

        /// <summary>
        /// Sets the filter data of the fixtures on a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_filterData"></param>
        public static void SetFilterData(uint _id, FilterData _filterData)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            Fixture f = physicsBodies[_id].GetFixtureList();
            f.Filter = _filterData;
            world.Refilter(f);

            while (f.Next != null)
            {
                f = f.Next;
                f.Filter = _filterData;
                world.Refilter(f);
            }
        }

        public class WorldContactListener : ContactListener
        {
            public void BeginContact(Contact contact)
            {
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;

                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if(bodyA == null || bodyB == null)
                {
                    Debug.LogError("PHYSICS: Contact body is null, cannot invoke collision.");
                    return;
                }

                uint idA = GetIDFromBody(bodyA);
                uint idB = GetIDFromBody(bodyB);

                if (fixtureA.IsSensor)
                {
                    RigidbodySensor.InvokeCollision(idA, idB, true);
                }
                else if (fixtureB.IsSensor)
                {
                    RigidbodySensor.InvokeCollision(idB, idA, true);
                }
                else
                {
                    // Both are rigidbodies
                    Rigidbody.InvokeCollision(idA, idB, true);
                    Rigidbody.InvokeCollision(idB, idA, true);
                }
            }

            public void EndContact(Contact contact)
            {
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;

                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA == null || bodyB == null)
                {
                    Debug.LogError("PHYSICS: Contact body is null, cannot invoke collision.");
                    return;
                }

                uint idA = GetIDFromBody(bodyA);
                uint idB = GetIDFromBody(bodyB);

                if (fixtureA.IsSensor)
                {
                    RigidbodySensor.InvokeCollision(idA, idB, false);
                }
                else if (fixtureB.IsSensor)
                {
                    RigidbodySensor.InvokeCollision(idB, idA, false);
                }
                else
                {
                    // Both are rigidbodies
                    Rigidbody.InvokeCollision(idA, idB, false);
                    Rigidbody.InvokeCollision(idB, idA, false);
                }
            }

            public void PostSolve(Contact contact, ContactImpulse impulse)
            {
                
            }

            public void PreSolve(Contact contact, Manifold oldManifold)
            {
                
            }
        }

        private static uint GetIDFromBody(Body _body)
        {
            foreach (var pair in physicsBodies)
            {
                if (pair.Value == _body)
                {
                    return pair.Key;
                }
            }

            return uint.MaxValue;
        }
    }
}
