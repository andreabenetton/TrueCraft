﻿using System;
using NUnit.Framework;
using TrueCraft.API;
using TrueCraft.API.Entities;
using TrueCraft.API.Physics;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Physics;
using TrueCraft.Core.TerrainGen;

namespace Test.TrueCraft.Core.Physics
{
    [TestFixture]
    public class PhysicsEngineTest
    {
        private class TestEntity : IAABBEntity
        {
            public TestEntity()
            {
                TerminalVelocity = 10;
                Size = new Size(1);
                CollisionOccured = false;
            }

            public bool BeginUpdate()
            {
                return true;
            }

            public void EndUpdate(Vector3 newPosition)
            {
                Position = newPosition;
            }

            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }
            public float AccelerationDueToGravity { get; set; }
            public float Drag { get; set; }
            public float TerminalVelocity { get; set; }
            public Vector3 CollisionPoint { get; set; }
            public bool CollisionOccured { get; set; }

            public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
            {
                CollisionPoint = collisionPoint;
                CollisionOccured = true;
            }

            public BoundingBox BoundingBox
            {
                get
                {
                    return new BoundingBox(Position, Position + Size);
                }
            }

            public Size Size { get; set; }
        }

        private IBlockPhysicsProvider GetBlockRepository()
        {
            var repository = new BlockRepository();
            repository.RegisterBlockProvider(new AirBlock());
            repository.RegisterBlockProvider(new StoneBlock());
            repository.RegisterBlockProvider(new GrassBlock());
            repository.RegisterBlockProvider(new DirtBlock());
            repository.RegisterBlockProvider(new BedrockBlock());
            return repository;
        }

        [Test]
        public void TestGravity()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 100, 0);
            entity.AccelerationDueToGravity = 1;
            entity.Drag = 0;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(99, entity.Position.Y);

            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(97, entity.Position.Y);
        }

        [Test]
        public void TestDrag()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 100, 0);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0.5f;
            entity.Velocity = Vector3.Down * 2;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(99, entity.Position.Y);
        }

        [Test]
        public void TestTerrainCollision()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Size = new Size(0.6, 1.8, 0.6);
            entity.Position = new Vector3(-10.9, 4, -10.9);
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(4, entity.Position.Y);

            physics.Update(TimeSpan.FromSeconds(5));

            Assert.AreEqual(4, entity.Position.Y);
        }

        [Test]
        public void TestExtremeTerrainCollision()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 4, 0);
            entity.AccelerationDueToGravity = 10;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(4, entity.Position.Y);
        }

        [Test]
        public void TestAdjacentFall()
        {
            // Tests an entity that falls alongside a wall

            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 10, 0);
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Create a wall
            for (int y = 0; y < 12; y++)
                world.SetBlockID(new Coordinates3D(1, y, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(9, entity.Position.Y);
            Assert.IsFalse(entity.CollisionOccured);
        }

        [Test]
        public void TestCollisionPoint()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 5, 0);
            entity.AccelerationDueToGravity = 1;
            entity.Drag = 0;
            physics.AddEntity(entity);

            world.SetBlockID(new Coordinates3D(0, 4, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(new Vector3(0, 4, 0), entity.CollisionPoint);
        }

        [Test]
        public void TestHorizontalCollision()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 5, 0);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;
            entity.Velocity = new Vector3(1, 0, 0);
            physics.AddEntity(entity);
            world.SetBlockID(new Coordinates3D(1, 5, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(0, entity.Velocity.X);
        }

        [Test]
        public void TestCornerCollision()
        {
            var repository = GetBlockRepository();
            var world = new global::TrueCraft.Core.World.World("default", new FlatlandGenerator());
            var physics = new PhysicsEngine(world, repository);
            var entity = new TestEntity();
            entity.Position = new Vector3(-1, 10, -1);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;
            entity.Velocity = new Vector3(1, 0, 1);
            physics.AddEntity(entity);
            world.SetBlockID(new Coordinates3D(0, 10, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(-1, entity.Position.X);
            Assert.AreEqual(-1, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(0, entity.Velocity.Z);
        }
    }
}