using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using System.Numerics;

namespace Electron2D.PhysicsBox2D
{
    public static class Physics
    {
        private enum PhysicsCallType
        {
            RemoveBody,
            ApplyForce,
            ApplyImpulse,
            ApplyTorque,
            SetAngle,
            SetAngularVelocity,
            SetVelocity,
            SetPosition,
            SetBodyFixedRotation,
            SetFilterData,
            CreateJoint,
            RemoveJoint,
        }

        // Scaling the physics so that 50 pixels equates to 1 meter in the simulation
        public static readonly float WorldScalar = 50f;

        private static Queue<(BodyDef, FixtureDef, MassData, bool)> creationQueue;
        private static Queue<(PhysicsCallType, uint, object[])> physicsCallQueue;
        private static Dictionary<uint, Body> physicsBodies;
        private static Dictionary<uint, Joint> joints;
        private static int jointsInQueue = 0;
        private static World world;
        private static bool _stepLock = false;

        /// <summary>
        /// This is called while the game is loading. Initializes the main physics world
        /// (Other physics worlds can be created separately from this using the Box2DX classes).
        /// </summary>
        /// <param name="_worldLowerBound"></param>
        /// <param name="_worldUpperBound"></param>
        /// <param name="_gravity"></param>
        /// <param name="_doSleep"></param>
        public static void Initialize(Vector2 _gravity, bool _doSleep)
        {
            creationQueue = new Queue<(BodyDef, FixtureDef, MassData, bool)>();
            physicsCallQueue = new Queue<(PhysicsCallType, uint, object[])>();
            physicsBodies = new Dictionary<uint, Body>();
            joints = new Dictionary<uint, Joint>();

            world = new World(_gravity);
            world.SetAllowSleeping(_doSleep);
            world.SetContactFilter(new ContactFilter());
            world.SetContactListener(new WorldContactListener());
            world.SetDestructionListener(new WorldDestructionListener());
        }

        /// <summary>
        /// Gets the physics world.
        /// </summary>
        /// <returns></returns>
        public static World GetWorld()
        {
            return world;
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
            while(creationQueue.Count > 0)
            {
                (BodyDef, FixtureDef, MassData, bool) data = creationQueue.Dequeue();
                CreatePhysicsBody(data.Item1, data.Item2, data.Item3, data.Item4, true);
            }

            // Calling physics methods that were queued during the step
            while(physicsCallQueue.Count > 0)
            {
                (PhysicsCallType, uint, object[]) data = physicsCallQueue.Dequeue();
                switch(data.Item1)
                {
                    case PhysicsCallType.RemoveBody:
                        RemovePhysicsBody(data.Item2);
                        break;
                    case PhysicsCallType.ApplyForce:
                        ApplyForce(data.Item2, (Vector2)data.Item3[0], (Vector2)data.Item3[1]);
                        break;
                    case PhysicsCallType.ApplyImpulse:
                        ApplyImpulse(data.Item2, (Vector2)data.Item3[0], (Vector2)data.Item3[1]);
                        break;
                    case PhysicsCallType.ApplyTorque:
                        ApplyTorque(data.Item2, (float)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetAngle:
                        SetAngle(data.Item2, (float)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetAngularVelocity:
                        SetAngularVelocity(data.Item2, (float)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetVelocity:
                        SetVelocity(data.Item2, (Vector2)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetPosition:
                        SetPosition(data.Item2, (Vector2)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetBodyFixedRotation:
                        SetBodyFixedRotation(data.Item2, (bool)data.Item3[0]);
                        break;
                    case PhysicsCallType.SetFilterData:
                        SetFilterData(data.Item2, (Filter)data.Item3[0]);
                        break;
                    case PhysicsCallType.CreateJoint:
                        CreateJoint((JointDef)data.Item3[0], true);
                        jointsInQueue--;
                        break;
                    case PhysicsCallType.RemoveJoint:
                        RemoveJoint(data.Item2);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a joint from a joint definition.
        /// </summary>
        /// <param name="_jointDef"></param>
        /// <returns></returns>
        public static uint CreateJoint(JointDef _jointDef)
        {
            return CreateJoint(_jointDef, false);
        }

        private static uint CreateJoint(JointDef _jointDef, bool _ignoreQueueCount)
        {
            uint id = (uint)(joints.Count + (_ignoreQueueCount ? 0 : jointsInQueue));
            if (_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.CreateJoint, id, new object[] { _jointDef }));
                jointsInQueue++;
                return id;
            }
            else
            {
                Joint joint = world.CreateJoint(_jointDef);
                joint.UserData = id;
                joints.Add(id, joint);
                return id;
            }
        }

        /// <summary>
        /// Removes a joint.
        /// </summary>
        /// <param name="_jointDef"></param>
        /// <returns></returns>
        public static void RemoveJoint(uint _id)
        {
            if (_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.RemoveJoint, _id, new object[0]));
            }
            else
            {
                if(!joints.ContainsKey(_id))
                {
                    Debug.LogError($"PHYSICS: Joint with ID: [ {_id} ] does not exist, cannot remove.");
                }
                else
                {
                    Joint joint = joints[_id];
                    world.DestroyJoint(joint);
                }
            }
        }

        /// <summary>
        /// Returns the joint with the given ID, or null if it does not exist
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Joint GetJoint(uint _id)
        {
            if(joints.ContainsKey(_id))
            {
                return joints[_id];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a dynamic physics body and returns it's ID.
        /// </summary>
        /// <param name="_bodyDefinition">The definition of the physics body.</param>
        /// <param name="_fixtureDef">The definition of the fixture. This holds the parameters of the physics body.</param>
        /// <param name="_massData">The mass data of the physics body.</param>
        /// <returns></returns>
        public static uint CreatePhysicsBody(BodyDef _bodyDefinition, FixtureDef _fixtureDef, MassData _massData, bool _isKinematic)
        {
            return CreatePhysicsBody(_bodyDefinition, _fixtureDef, _massData, _isKinematic, false);
        }

        private static uint CreatePhysicsBody(BodyDef _bodyDefinition, FixtureDef _fixtureDef, MassData _massData, bool _isKinematic, bool _ignoreQueueCount)
        {
            uint id = (uint)(physicsBodies.Count + (_ignoreQueueCount ? 0 : creationQueue.Count));
            try
            {
                if (_stepLock)
                {
                    creationQueue.Enqueue((_bodyDefinition, _fixtureDef, _massData, _isKinematic));
                    return id;
                }

                Body b = world.CreateBody(_bodyDefinition);
                if (b == null)
                {
                    Debug.LogError("PHYSICS: Error creating physics body! Physics step likely in progress!");
                    return 0;
                }
                b.SetUserData(id);
                b.CreateFixture(_fixtureDef);
                b.SetMassData(_massData);
                b.SetType(_isKinematic ? BodyType.Kinematic : BodyType.Dynamic);
                physicsBodies.Add(id, b);

                return id;
            }
            catch (Exception e)
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
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.RemoveBody, _id, new object[0]));
            }
            else
            {
                if(!physicsBodies.ContainsKey(_id))
                {
                    Debug.LogError($"PHYSICS: Body with ID: [ {_id} ] does not exist, cannot remove.");
                }
                world.DestroyBody(physicsBodies[_id]);
            }
        }

        /// <summary>
        /// Gets the position of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Vector2 GetBodyPosition(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return Vector2.Zero;
            Vector2 vec = physicsBodies[_id].GetPosition();
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
            Vector2 vec = physicsBodies[_id].GetLinearVelocity();
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
        public static Filter[] GetFilterData(uint _id)
        {
            if (!physicsBodies.ContainsKey(_id)) return null;

            List<Filter> filters = new List<Filter>();

            Fixture f = physicsBodies[_id].GetFixtureList();
            filters.Add(f.FilterData);
            while (f.Next != null)
            {
                f = f.Next;
                filters.Add(f.FilterData);
            }

            return filters.ToArray();
        }

        /// <summary>
        /// Gets the physics body associated with the given ID
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Body GetBody(uint _id)
        {
            if (physicsBodies.ContainsKey(_id))
            {
                return physicsBodies[_id];
            }
            else
            {
                return null;
            }
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
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.ApplyForce, _id, new object[] { _force, _point }));
            }
            else
            {
                physicsBodies[_id].ApplyLinearImpulse(_force, _point);
            }
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
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.ApplyImpulse, _id, new object[] { _impulse, _point }));
            }
            else
            {
                physicsBodies[_id].ApplyLinearImpulse(_impulse, _point);
            }
        }

        /// <summary>
        /// Applies a torque to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_torque"></param>
        public static void ApplyTorque(uint _id, float _torque)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.ApplyTorque, _id, new object[] { _torque }));
            }
            else
            {
                physicsBodies[_id].ApplyTorque(_torque);
            }
        }

        /// <summary>
        /// Sets the angle (in degrees) of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngle(uint _id, float _angle)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.ApplyTorque, _id, new object[] { _angle }));
            }
            else
            {
                physicsBodies[_id].SetTransform(physicsBodies[_id].Position, _angle * (MathF.PI / 180));
            }
        }

        /// <summary>
        /// Sets the angular velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngularVelocity(uint _id, float _angularVelocity)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.SetAngularVelocity, _id, new object[] { _angularVelocity }));
            }
            else
            {
                physicsBodies[_id].SetAngularVelocity(_angularVelocity);
            }
        }

        /// <summary>
        /// Sets the linear velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_linearVelocity"></param>
        public static void SetVelocity(uint _id, Vector2 _linearVelocity)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.SetVelocity, _id, new object[] { _linearVelocity }));
            }
            else
            {
                physicsBodies[_id].SetLinearVelocity(_linearVelocity);
            }
        }

        /// <summary>
        /// Sets the position of a physics body. Use this instead of <see cref="Transform.Position"/> so that movements are handled within the physics engine.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_position"></param>
        public static void SetPosition(uint _id, Vector2 _position)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.SetPosition, _id, new object[] { _position }));
            }
            else
            {
                physicsBodies[_id].SetTransform(_position / WorldScalar, physicsBodies[_id].GetAngle());
            }
        }

        /// <summary>
        /// Toggles the fixed rotation of a physics body.
        /// </summary>
        /// <param name="_fixedRotation"></param>
        public static void SetBodyFixedRotation(uint _id, bool _fixedRotation)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.SetBodyFixedRotation, _id, new object[] { _fixedRotation }));
            }
            else
            {
                physicsBodies[_id].SetFixedRotation(_fixedRotation);
            }
        }

        /// <summary>
        /// Sets the filter data of the fixtures on a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_filter"></param>
        public static void SetFilterData(uint _id, Filter _filter)
        {
            if (!physicsBodies.ContainsKey(_id)) return;
            if(_stepLock)
            {
                physicsCallQueue.Enqueue((PhysicsCallType.SetFilterData, _id, new object[] { _filter }));
            }
            else
            {
                Fixture f = physicsBodies[_id].GetFixtureList();
                f.FilterData = _filter;
                f.Refilter();

                while (f.Next != null)
                {
                    f = f.Next;
                    f.FilterData = _filter;
                    f.Refilter();
                }
            }
        }

        /// <summary>
        /// Casts a ray into the physics world, and returns the hit data.
        /// </summary>
        /// <param name="_point">The starting point of the ray.</param>
        /// <param name="_direction">The direction of the ray.</param>
        /// <param name="_maxDistance">The maximum distance the ray can travel.</param>
        /// <param name="_solidShapes"></param>
        /// <returns></returns>
        public static bool Raycast(Vector2 _point, Vector2 _direction, float _maxDistance, out RaycastHit hit)
        {
            hit = new RaycastHit();
            hit.MaxDistance = _maxDistance;
            try
            {
                world.RayCast(hit.Callback, _point, _point + (_direction * _maxDistance));
            }
            catch (Exception e)
            {
                return false;
            }
            return hit.Hit;
        }

        private static uint GetIDFromBody(Body _body)
        {
            return _body.GetUserData<uint>();
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

                if (fixtureA.IsSensor())
                {
                    RigidbodySensor.InvokeCollision(idA, idB, true);
                }
                else if (fixtureB.IsSensor())
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

            public override void BeginContact(in Contact contact)
            {
                
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

                if (fixtureA.IsSensor())
                {
                    RigidbodySensor.InvokeCollision(idA, idB, false);
                }
                else if (fixtureB.IsSensor())
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

            public override void EndContact(in Contact contact)
            {
                
            }

            public void PostSolve(Contact contact, ContactImpulse impulse)
            {
                
            }

            public override void PostSolve(in Contact contact, in ContactImpulse impulse)
            {
                
            }

            public void PreSolve(Contact contact, Manifold oldManifold)
            {
                
            }

            public override void PreSolve(in Contact contact, in Manifold oldManifold)
            {
                
            }
        }

        public class WorldDestructionListener : DestructionListener
        {
            public override void SayGoodbye(Joint joint)
            {
                throw new NotImplementedException();
            }

            public override void SayGoodbye(Fixture fixture)
            {
                foreach (var rigidbody in Rigidbody.Rigidbodies)
                {
                    if (rigidbody.ID == fixture.Body.GetUserData<uint>())
                    {
                        rigidbody.Dispose();
                    }
                }

                foreach (var sensor in RigidbodySensor.Sensors)
                {
                    if (sensor.ID == fixture.Body.GetUserData<uint>())
                    {
                        sensor.Dispose();
                    }
                }
            }
        }
    }
}
